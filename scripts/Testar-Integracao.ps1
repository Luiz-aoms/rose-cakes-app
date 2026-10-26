param([string]$Servidor = '.\SQLEXPRESS', [int]$Porta = 5119)
$ErrorActionPreference = 'Stop'
$raiz = Split-Path $PSScriptRoot -Parent
$token = [guid]::NewGuid().ToString('N')
$banco = 'RoseCakesTeste_' + $token
$recuperado = 'RoseCakesRecuperado_' + $token
$processo = $null
function Sql([string]$comando, [string]$destino = 'master') {
    & sqlcmd -S $Servidor -d $destino -E -C -b -l 10 -Q $comando
    if ($LASTEXITCODE -ne 0) { throw 'Falha em comando SQL de validação.' }
}
function Api([string]$rota, [string]$metodo = 'GET', $dados = $null, [int]$esperado = 200) {
    $argsHttp = @{ Uri = "http://127.0.0.1:$Porta/api/$rota"; Method = $metodo; SkipHttpErrorCheck = $true; TimeoutSec = 10 }
    if ($null -ne $dados) { $argsHttp.ContentType = 'application/json'; $argsHttp.Body = ($dados | ConvertTo-Json -Depth 12) }
    $res = Invoke-WebRequest @argsHttp
    if ([int]$res.StatusCode -ne $esperado) { throw "$metodo $rota retornou $($res.StatusCode): $($res.Content)" }
    if ([int]$res.StatusCode -eq 204) { return }
    if ($res.Content.StartsWith('{') -or $res.Content.StartsWith('[')) { return $res.Content | ConvertFrom-Json }
}
function Conferir([bool]$condicao, [string]$mensagem) { if (-not $condicao) { throw $mensagem }; Write-Output "OK: $mensagem" }
try {
    Sql "CREATE DATABASE [$banco];"
    foreach ($arquivo in Get-ChildItem (Join-Path $raiz 'database/*.sql') | Sort-Object Name) {
        & sqlcmd -S $Servidor -d $banco -E -C -b -i $arquivo.FullName
        if ($LASTEXITCODE -ne 0) { throw "Falha no script $($arquivo.Name)" }
    }
    $conexaoAnterior = $env:ConnectionStrings__DefaultConnection
    try {
        $env:ConnectionStrings__DefaultConnection = "Server=$Servidor;Database=$banco;Trusted_Connection=True;TrustServerCertificate=True;"
        $processo = Start-Process dotnet -ArgumentList @((Join-Path $raiz 'backend/validation-build/RoseCakes.Api.dll'), '--urls', "http://127.0.0.1:$Porta") -WorkingDirectory (Join-Path $raiz 'backend/RoseCakes.Api') -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $env:TEMP "rose-api-$token.log") -RedirectStandardError (Join-Path $env:TEMP "rose-api-$token-error.log")
    } finally { $env:ConnectionStrings__DefaultConnection = $conexaoAnterior }
    $pronto = $false
    for ($i=0; $i -lt 25; $i++) { try { Api 'produtos' | Out-Null; $pronto = $true; break } catch { Start-Sleep -Milliseconds 400 } }
    if (-not $pronto) { throw 'API de validação não iniciou.' }
    $cliente = Api 'gestao/clientes' POST @{nome='Cliente de validação';telefone='11999990000'}
    $produto = Api 'produtos' POST @{nome='Bolo de teste';categoria='Bolos';precoCusto=40;precoVenda=120;ativo=$true;unidadeVenda='Bolo';rendimento=1}
    $venda = Api 'gestao/vendas' POST @{clienteId=$cliente.id;dataVenda='2026-10-26';pagamento='Pix';itens=@(@{produtoId=$produto.id;quantidade=1});dataEntrega='2026-10-31T14:30:00';tipoEntrega='Retirada';valorRecebido=40}
    Conferir ($venda.total -eq 120 -and $venda.custo -eq 40 -and $venda.recebido -eq 40 -and $venda.saldo -eq 80) 'Encomenda com sinal e saldo corretos'
    Api "produtos/$($produto.id)" PUT @{nome='Bolo atualizado';categoria='Bolos';precoCusto=50;precoVenda=150;ativo=$true;unidadeVenda='Bolo';rendimento=1} | Out-Null
    $historico = @(Api 'gestao/vendas')[0]
    Conferir ($historico.total -eq 120 -and $historico.custo -eq 40 -and $historico.itens[0].produtoNome -eq 'Bolo de teste') 'Venda preserva nome, preço e custo históricos'
    $quitada = Api "gestao/vendas/$($venda.id)/recebimentos" POST @{valor=80;dataPagamento='2026-10-27';forma='Dinheiro'}
    Conferir ($quitada.recebido -eq 120 -and $quitada.saldo -eq 0 -and $quitada.recebimentos.Count -eq 2) 'Pagamento posterior quita o saldo'
    Api "gestao/vendas/$($venda.id)/recebimentos" POST @{valor=1;dataPagamento='2026-10-27';forma='Pix'} 400 | Out-Null
    $itemOriginal = $quitada.itens[0]
    $edicao = @{clienteId=$cliente.id;dataVenda='2026-10-26';dataEntrega='2026-11-01T15:00:00';tipoEntrega='Entrega';enderecoEntrega='Endereço de teste';observacoes='Pedido alterado';itens=@(@{id=$itemOriginal.id;produtoId=$produto.id;quantidade=1;precoUnitario=100})}
    Api "gestao/vendas/$($venda.id)" PUT $edicao 400 | Out-Null
    $edicao.itens[0].precoUnitario = 120
    $edicao.itens[0].quantidade = 2
    $alterada = Api "gestao/vendas/$($venda.id)" PUT $edicao
    Conferir ($alterada.total -eq 240 -and $alterada.custo -eq 80 -and $alterada.recebido -eq 120 -and $alterada.saldo -eq 120 -and $alterada.itens[0].produtoNome -eq 'Bolo de teste' -and $alterada.observacoes -eq 'Pedido alterado') 'Editar pedido preserva recebimentos, nome e custo históricos'
    $edicao.itens[0].quantidade = 1
    Api "gestao/vendas/$($venda.id)" PUT $edicao | Out-Null
    $pronta = Api "gestao/vendas/$($venda.id)/etapa" PUT @{etapa='Pronto'}
    Conferir ($pronta.etapa -eq 'Pronto') 'Etapa de produção atualizada'
    Api "gestao/vendas/$($venda.id)/cancelamento" POST @{motivo='Teste de cancelamento'} | Out-Null
    Api "gestao/vendas/$($venda.id)/recebimentos" POST @{valor=1;dataPagamento='2026-10-27';forma='Pix'} 400 | Out-Null
    Api "gestao/vendas/$($venda.id)/cancelamento" POST @{motivo='Duplicado'} 400 | Out-Null
    Api 'gestao/vendas' POST @{dataVenda='2026-10-26';pagamento='Pix';itens=@(@{produtoId=$produto.id;quantidade=1});dataEntrega='2026-10-31T14:30:00';tipoEntrega='Entrega'} 400 | Out-Null
    $despesa = Api 'gestao/despesas' POST @{descricao='Transporte teste';dataDespesa='2026-10-26';valor=10;categoria='Transporte'}
    $editada = Api "gestao/despesas/$($despesa.id)" PUT @{descricao='Transporte corrigido';dataDespesa='2026-10-27';valor=15;categoria='Entrega'}
    Conferir ($editada.valor -eq 15 -and $editada.descricao -eq 'Transporte corrigido') 'Edição de despesa preserva o registro'
    Api "gestao/despesas/$($despesa.id)/cancelamento" POST @{motivo='Lançamento duplicado'} | Out-Null
    Api "gestao/despesas/$($despesa.id)" PUT @{descricao='Inválida';dataDespesa='2026-10-26';valor=5} 400 | Out-Null
    Api 'produtos' POST @{nome='Foto inválida';categoria='Bolos';precoCusto=1;precoVenda=2;foto='data:image/svg+xml;base64,PHN2Zz48L3N2Zz4='} 400 | Out-Null
    $foto='data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+j6xkAAAAASUVORK5CYII='
    $comFoto=Api 'produtos' POST @{nome='Foto de teste';categoria='Nova categoria';precoCusto=1;precoVenda=2;foto=$foto;unidadeVenda='Fatia';rendimento=8}
    Conferir ($comFoto.foto -eq $foto -and $comFoto.rendimento -eq 8) 'Foto e dados do catálogo persistidos'
    # Regressão: venda paga anterior não deve quitar automaticamente uma nova.
    $nova = Api 'gestao/vendas' POST @{clienteId=$cliente.id;dataVenda='2026-10-26';pagamento='Pix';itens=@(@{produtoId=$produto.id;quantidade=1})}
    Conferir ($nova.recebido -eq 0 -and $nova.saldo -eq 150 -and $nova.recebimentos.Count -eq 0) 'Nova venda após outra paga começa pendente'
    $paga = Api "gestao/vendas/$($nova.id)/recebimentos" POST @{valor=150;dataPagamento='2026-10-26';forma='Pix'}
    $recebimentoId = $paga.recebimentos[0].id
    $corrigida = Api "gestao/vendas/$($nova.id)/recebimentos/$recebimentoId" PUT @{valor=30;dataPagamento='2026-10-26';forma='Dinheiro'}
    Conferir ($corrigida.recebido -eq 30 -and $corrigida.saldo -eq 120) 'Alterar recebimento recalcula o saldo'
    Api "gestao/vendas/$($nova.id)/recebimentos/$recebimentoId" DELETE @{confirmar=$false} 400 | Out-Null
    Api "gestao/vendas/$($nova.id)/recebimentos/$recebimentoId" DELETE @{confirmar=$true} 204 | Out-Null
    $semRecebimento = @(Api 'gestao/vendas') | Where-Object id -eq $nova.id
    Conferir ($semRecebimento.recebido -eq 0 -and $semRecebimento.saldo -eq 150) 'Excluir recebimento incorreto volta a deixar a venda pendente'
    $itemNova = $semRecebimento.itens[0]
    $edicaoNova = @{clienteId=$cliente.id;dataVenda='2026-10-26';itens=@(@{id=$itemNova.id;produtoId=$produto.id;quantidade=1;precoUnitario=150},@{produtoId=$produto.id;quantidade=1;precoUnitario=70});observacoes='Venda alterada'}
    $novaComItem = Api "gestao/vendas/$($nova.id)" PUT $edicaoNova
    Conferir ($novaComItem.total -eq 220 -and $novaComItem.itens.Count -eq 2) 'Editar venda permite incluir item e alterar preço'
    $itemAdicionado = $novaComItem.itens | Where-Object precoUnitario -eq 70
    $edicaoNova.itens = @(@{id=$itemAdicionado.id;produtoId=$produto.id;quantidade=1;precoUnitario=70})
    $novaSemItem = Api "gestao/vendas/$($nova.id)" PUT $edicaoNova
    Conferir ($novaSemItem.total -eq 70 -and $novaSemItem.itens.Count -eq 1) 'Editar venda remove item e recalcula total'
    $pagaParaExcluir = Api 'gestao/vendas' POST @{clienteId=$cliente.id;dataVenda='2026-10-26';pagamento='Pix';valorRecebido=10;itens=@(@{produtoId=$produto.id;quantidade=1})}
    Api "gestao/vendas/$($pagaParaExcluir.id)" DELETE @{confirmar=$true} 204 | Out-Null
    Api "gestao/vendas/$($nova.id)" DELETE @{confirmar=$false} 400 | Out-Null
    Api "gestao/vendas/$($nova.id)" DELETE @{confirmar=$true} 204 | Out-Null
    Conferir (-not (@(Api 'gestao/vendas') | Where-Object id -eq $nova.id)) 'Exclusão confirmada remove a venda'
    Api "produtos/$($produto.id)" DELETE @{confirmar=$true} 409 | Out-Null
    Api "gestao/clientes/$($cliente.id)" DELETE @{confirmar=$true} 409 | Out-Null
    Api "gestao/despesas/$($despesa.id)" DELETE @{confirmar=$true} 204 | Out-Null
    $clienteAvulso = Api 'gestao/clientes' POST @{nome='Cliente sem vínculo';telefone='11999990001'}
    Api "gestao/clientes/$($clienteAvulso.id)" DELETE @{confirmar=$true} 204 | Out-Null
    $produtoAvulso = Api 'produtos' POST @{nome='Produto sem vínculo';categoria='Teste';precoCusto=1;precoVenda=2;ativo=$true}
    Api "produtos/$($produtoAvulso.id)" DELETE @{confirmar=$true} 204 | Out-Null
    Write-Output 'OK: exclusões exigem confirmação e respeitam os vínculos históricos.'
    & (Join-Path $PSScriptRoot 'Backup-Local.ps1') -Servidor $Servidor -Banco $banco
    # Restaurar o backup mais recente do banco de teste em um NOVO banco.
    $sqlRestore = @"
DECLARE @arquivo nvarchar(4000) = (SELECT TOP 1 bmf.physical_device_name FROM msdb.dbo.backupset bs JOIN msdb.dbo.backupmediafamily bmf ON bs.media_set_id=bmf.media_set_id WHERE bs.database_name=N'$banco' ORDER BY bs.backup_finish_date DESC);
DECLARE @dados nvarchar(4000)=CONVERT(nvarchar(4000),SERVERPROPERTY('InstanceDefaultDataPath'))+N'$recuperado.mdf';
DECLARE @log nvarchar(4000)=CONVERT(nvarchar(4000),SERVERPROPERTY('InstanceDefaultLogPath'))+N'${recuperado}_log.ldf';
DECLARE @logicoDados nvarchar(128)=(SELECT name FROM sys.master_files WHERE database_id=DB_ID(N'$banco') AND type=0);
DECLARE @logicoLog nvarchar(128)=(SELECT name FROM sys.master_files WHERE database_id=DB_ID(N'$banco') AND type=1);
RESTORE DATABASE [$recuperado] FROM DISK=@arquivo WITH MOVE @logicoDados TO @dados, MOVE @logicoLog TO @log;
IF (SELECT COUNT(*) FROM [$recuperado].dbo.Recebimentos) <> 2 THROW 50000, 'Recebimentos divergentes após restauração.', 1;
IF (SELECT COUNT(*) FROM [$recuperado].dbo.Produtos WHERE Foto IS NOT NULL) <> 1 THROW 50000, 'Foto ausente após restauração.', 1;
"@
    Sql $sqlRestore
    Write-Output 'OK: backup e restauração em banco separado preservaram dados e foto.'
    Write-Output 'Todos os testes de integração passaram.'
} finally {
    if ($processo -and -not $processo.HasExited) { Stop-Process -Id $processo.Id }
    # Apenas os nomes aleatórios criados por esta execução podem ser removidos.
    foreach ($temporario in @($banco,$recuperado)) {
        if ($temporario -notmatch '^RoseCakes(Teste|Recuperado)_[a-f0-9]{32}$') { throw 'Nome inesperado: limpeza interrompida.' }
        Sql "IF DB_ID(N'$temporario') IS NOT NULL BEGIN ALTER DATABASE [$temporario] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$temporario]; END;"
    }
}
