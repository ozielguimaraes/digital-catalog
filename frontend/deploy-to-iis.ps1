# Script PowerShell para deploy da aplicação Angular no IIS
# Execute como Administrador

param(
    [Parameter(Mandatory=$true)]
    [string]$SiteName,
    
    [Parameter(Mandatory=$false)]
    [string]$PhysicalPath = "C:\inetpub\wwwroot\$SiteName"
)

Write-Host "=== Deploy da Aplicação Angular para IIS ===" -ForegroundColor Green

# Verificar se o IIS está instalado
if (-not (Get-WindowsFeature -Name IIS-WebServerRole).InstallState -eq "Installed") {
    Write-Error "IIS não está instalado. Instale o IIS primeiro."
    exit 1
}

# Verificar se o site existe
$site = Get-IISSite -Name $SiteName -ErrorAction SilentlyContinue

if ($site) {
    Write-Host "Site '$SiteName' encontrado. Atualizando..." -ForegroundColor Yellow
    Stop-IISSite -Name $SiteName
} else {
    Write-Host "Criando novo site '$SiteName'..." -ForegroundColor Yellow
}

# Criar diretório se não existir
if (-not (Test-Path $PhysicalPath)) {
    New-Item -ItemType Directory -Path $PhysicalPath -Force
    Write-Host "Diretório criado: $PhysicalPath" -ForegroundColor Green
}

# Copiar arquivos do build
$sourcePath = ".\dist\ng-tailadmin\*"
Write-Host "Copiando arquivos de $sourcePath para $PhysicalPath..." -ForegroundColor Yellow

Copy-Item -Path $sourcePath -Destination $PhysicalPath -Recurse -Force

# Configurar permissões
Write-Host "Configurando permissões..." -ForegroundColor Yellow
$acl = Get-Acl $PhysicalPath
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($accessRule)
Set-Acl -Path $PhysicalPath -AclObject $acl

# Criar ou atualizar site
if ($site) {
    Set-IISSite -Name $SiteName -PhysicalPath $PhysicalPath
    Start-IISSite -Name $SiteName
} else {
    New-IISSite -Name $SiteName -PhysicalPath $PhysicalPath -Port 80
}

# Configurar Application Pool
$appPoolName = "AppPool_$SiteName"
if (-not (Get-IISAppPool -Name $appPoolName -ErrorAction SilentlyContinue)) {
    New-IISAppPool -Name $appPoolName
    Set-ItemProperty -Path "IIS:\AppPools\$appPoolName" -Name processModel.identityType -Value ApplicationPoolIdentity
    Set-ItemProperty -Path "IIS:\AppPools\$appPoolName" -Name managedRuntimeVersion -Value ""
}

# Associar site ao Application Pool
Set-ItemProperty -Path "IIS:\Sites\$SiteName" -Name applicationPool -Value $appPoolName

Write-Host "=== Deploy Concluído! ===" -ForegroundColor Green
Write-Host "Site: $SiteName" -ForegroundColor Cyan
Write-Host "URL: http://localhost" -ForegroundColor Cyan
Write-Host "Caminho: $PhysicalPath" -ForegroundColor Cyan

# Verificar se o site está rodando
Start-Sleep -Seconds 3
$siteStatus = Get-IISSite -Name $SiteName
Write-Host "Status do Site: $($siteStatus.State)" -ForegroundColor $(if ($siteStatus.State -eq "Started") { "Green" } else { "Red" })
