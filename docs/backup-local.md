# Backup e recuperação local

O backup completo inclui clientes, produtos, fotos, encomendas, despesas e recebimentos. A exportação CSV não substitui o backup.

## Criar cópia

1. Abra o SQL Server Management Studio e conecte à instância usada pelo projeto.
2. Clique com o botão direito em `RoseCakesDB` e selecione **Tarefas → Fazer Backup**.
3. Selecione o tipo **Completo** e um destino em disco com extensão `.bak`.
4. Use um nome que identifique a data da cópia e confira o destino antes de confirmar.
5. Aguarde a mensagem de conclusão do SQL Server.

Copie periodicamente o arquivo para outro dispositivo protegido. Não envie backups ao GitHub; eles contêm dados do negócio. O arquivo `.gitignore` inclui `*.bak`.

## Recuperar sem sobrescrever o banco em uso

1. No SQL Server Management Studio, conecte à instância e escolha **Bancos de Dados → Restaurar Banco de Dados**.
2. Selecione **Dispositivo** e o arquivo `.bak` desejado.
3. Como destino, use um banco novo, por exemplo `RoseCakesRecuperado_20261026`.
4. Na página **Arquivos**, confira que os caminhos `.mdf` e `.ldf` também são novos. Não use `WITH REPLACE` e não selecione o banco original como destino.
5. Conclua a restauração e confira as tabelas e quantidades de registros.
6. Para validar pelo sistema, aponte uma instância separada da API para o banco recuperado por `ConnectionStrings__DefaultConnection`. Confira clientes, histórico, fotos, recebimentos e totais antes de utilizá-lo.

Confirme a recuperação em um banco de teste antes de depender do backup. O banco original deve permanecer intacto durante essa conferência. A restauração não envolve a execução novamente dos scripts de criação se o backup já contiver a versão atual.
