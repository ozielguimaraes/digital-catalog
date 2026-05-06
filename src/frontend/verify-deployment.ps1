# Script para verificar se o deploy foi realizado com sucesso
param(
    [Parameter(Mandatory=$false)]
    [string]$SiteName = "MeuCatalogo",
    
    [Parameter(Mandatory=$false)]
    [string]$Url = "http://localhost"
)

Write-Host "=== Verificacao do Deploy ===" -ForegroundColor Green

# Verificar se o site existe no IIS
$site = Get-IISSite -Name $SiteName -ErrorAction SilentlyContinue
if ($site) {
    Write-Host "✓ Site '$SiteName' encontrado no IIS" -ForegroundColor Green
    Write-Host "  Status: $($site.State)" -ForegroundColor Cyan
    Write-Host "  Caminho: $($site.PhysicalPath)" -ForegroundColor Cyan
} else {
    Write-Host "✗ Site '$SiteName' não encontrado no IIS" -ForegroundColor Red
    exit 1
}

# Verificar se os arquivos essenciais existem
$physicalPath = $site.PhysicalPath
$essentialFiles = @("index.html", "web.config", "main-FEZ2EJMF.js", "styles-Z63MUVMI.css")

Write-Host "`nVerificando arquivos essenciais..." -ForegroundColor Yellow
foreach ($file in $essentialFiles) {
    $filePath = Join-Path $physicalPath $file
    if (Test-Path $filePath) {
        Write-Host "✓ $file" -ForegroundColor Green
    } else {
        Write-Host "✗ $file - ARQUIVO FALTANDO!" -ForegroundColor Red
    }
}

# Verificar conectividade HTTP
Write-Host "`nTestando conectividade..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri $Url -Method Get -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ Site respondendo corretamente (HTTP $($response.StatusCode))" -ForegroundColor Green
    } else {
        Write-Host "⚠ Site respondendo com status: $($response.StatusCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Erro ao acessar o site: $($_.Exception.Message)" -ForegroundColor Red
}

# Verificar se é uma SPA (Single Page Application)
Write-Host "`nVerificando configuração SPA..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$Url/rota-inexistente" -Method Get -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ URL Rewriting configurado corretamente (SPA funcionando)" -ForegroundColor Green
    } else {
        Write-Host "⚠ URL Rewriting pode não estar funcionando" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Erro ao testar URL Rewriting" -ForegroundColor Red
}

# Verificar Application Pool
$appPool = Get-IISAppPool -Name "AppPool_$SiteName" -ErrorAction SilentlyContinue
if ($appPool) {
    Write-Host "✓ Application Pool 'AppPool_$SiteName' encontrado" -ForegroundColor Green
    Write-Host "  Status: $($appPool.State)" -ForegroundColor Cyan
} else {
    Write-Host "⚠ Application Pool personalizado não encontrado" -ForegroundColor Yellow
}

Write-Host "`n=== Verificacao Concluida ===" -ForegroundColor Green
Write-Host "URL do Site: $Url" -ForegroundColor Cyan
Write-Host "Caminho Fisico: $physicalPath" -ForegroundColor Cyan
