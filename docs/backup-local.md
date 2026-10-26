# Backup e recuperação local

O backup completo inclui clientes, produtos, fotos, encomendas, despesas e recebimentos. A exportação CSV não substitui o backup.

## Criar cópia

Na raiz do projeto, execute no PowerShell:

```powershell
.\scripts\Backup-Local.ps1
```

O script usa autenticação do Windows e a pasta padrão de backups da instância SQL Server. O nome do arquivo é único, sem sobrescrever backups anteriores. Faz `COPY_ONLY` com checksum e `RESTORE VERIFYONLY`; mostra o caminho do arquivo verificado. A conta que executa precisa de permissão de backup no banco. A conta do serviço SQL Server precisa escrever na pasta de destino.

Copie periodicamente o arquivo para outro dispositivo protegido. Não envie backups ao GitHub; eles contêm dados do negócio. O arquivo `.gitignore` inclui `*.bak`.

## Recuperar sem sobrescrever o banco em uso

1. No SQL Server Management Studio, conecte à instância e escolha **Bancos de Dados → Restaurar Banco de Dados**.
2. Selecione **Dispositivo** e o arquivo `.bak` desejado.
3. Como destino, use um banco novo, por exemplo `RoseCakesRecuperado_20261026`.
4. Na página **Arquivos**, confira que os caminhos `.mdf` e `.ldf` também são novos. Não use `WITH REPLACE` e não selecione o banco original como destino.
5. Conclua a restauração e confira as tabelas e quantidades de registros.
6. Para validar pelo sistema, aponte uma instância separada da API para o banco recuperado por `ConnectionStrings__DefaultConnection`. Confira clientes, histórico, fotos, recebimentos e totais antes de utilizá-lo.

O `VERIFYONLY` detecta problemas no arquivo, mas não substitui uma restauração de teste. O banco original deve permanecer intacto durante essa conferência. A restauração não envolve a execução novamente dos scripts de criação se o backup já contiver a versão atual.
