param(
    [string]$Servidor = '.\SQLEXPRESS',
    [ValidatePattern('^[A-Za-z][A-Za-z0-9_]*$')][string]$Banco = 'RoseCakesDB'
)
$ErrorActionPreference = 'Stop'
if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) { throw 'Instale o utilitário sqlcmd para executar o backup.' }
$nomeArquivo = $Banco + '_' + (Get-Date -Format 'yyyyMMdd_HHmmss_fff') + '_' + [guid]::NewGuid().ToString('N').Substring(0,8) + '.bak'
$sql = @"
SET NOCOUNT ON;
DECLARE @pasta nvarchar(4000) = CONVERT(nvarchar(4000), SERVERPROPERTY('InstanceDefaultBackupPath'));
IF @pasta IS NULL THROW 50000, 'Pasta padrão de backup não encontrada.', 1;
IF RIGHT(@pasta, 1) NOT IN ('\', '/') SET @pasta = @pasta + '\';
DECLARE @arquivo nvarchar(4000) = @pasta + N'$nomeArquivo';
BACKUP DATABASE [$Banco] TO DISK = @arquivo WITH COPY_ONLY, CHECKSUM;
RESTORE VERIFYONLY FROM DISK = @arquivo WITH CHECKSUM;
PRINT @arquivo;
"@
& sqlcmd -S $Servidor -d master -E -C -b -Q $sql
if ($LASTEXITCODE -ne 0) { throw 'O backup ou a verificação falhou. Confira as permissões do SQL Server.' }
Write-Output 'Backup completo criado e verificado. Guarde uma cópia em outro dispositivo protegido.'
