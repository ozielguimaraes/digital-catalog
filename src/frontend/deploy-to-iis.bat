@echo off
REM Script Batch para deploy da aplicação Angular no IIS
REM Execute como Administrador

set SITE_NAME=MeuCatalogo
set PHYSICAL_PATH=C:\inetpub\wwwroot\%SITE_NAME%

echo === Deploy da Aplicacao Angular para IIS ===

REM Verificar se o diretório de build existe
if not exist "dist\ng-tailadmin" (
    echo ERRO: Diretorio de build nao encontrado. Execute 'npm run build' primeiro.
    pause
    exit /b 1
)

REM Criar diretório se não existir
if not exist "%PHYSICAL_PATH%" (
    echo Criando diretorio: %PHYSICAL_PATH%
    mkdir "%PHYSICAL_PATH%"
)

REM Copiar arquivos
echo Copiando arquivos...
xcopy "dist\ng-tailadmin\*" "%PHYSICAL_PATH%\" /E /Y /I

REM Configurar permissões (requer privilégios de administrador)
echo Configurando permissoes...
icacls "%PHYSICAL_PATH%" /grant "IIS_IUSRS:(OI)(CI)F" /T

echo === Deploy Concluido! ===
echo Site: %SITE_NAME%
echo Caminho: %PHYSICAL_PATH%
echo.
echo IMPORTANTE: Configure o site no IIS Manager:
echo 1. Abra o IIS Manager
echo 2. Crie um novo site ou configure um existente
echo 3. Defina o caminho fisico como: %PHYSICAL_PATH%
echo 4. Configure o Application Pool para .NET Framework v4.0 ou superior
echo 5. Certifique-se de que o web.config esta no diretorio raiz

pause
