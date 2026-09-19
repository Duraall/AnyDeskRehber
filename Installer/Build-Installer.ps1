$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'AnyDeskRehber.WinUI3.csproj'
$publish = Join-Path $root 'bin\publish\win-x64'
$iss = Join-Path $PSScriptRoot 'AnyDeskRehber.iss'

Write-Host '1/2 - WinUI 3 uygulaması self-contained olarak publish ediliyor...'
dotnet publish $project -c Release -p:Platform=x64 -p:PublishProfile=win-x64

if (-not (Test-Path (Join-Path $publish 'AnyDeskRehber.WinUI3.exe'))) {
    throw "Publish tamamlandı fakat uygulama EXE'si bulunamadı: $publish"
}

Write-Host '2/2 - Inno Setup aranıyor...'
$candidates = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe"
)
$iscc = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $iscc) {
    throw "Inno Setup 6 bulunamadı. Inno Setup 6'yı kurduktan sonra bu scripti tekrar çalıştırın."
}

& $iscc $iss

$output = Join-Path $PSScriptRoot 'Output\AnyDeskRehber_Setup.exe'
if (-not (Test-Path $output)) {
    throw "Setup.exe oluşturulamadı: $output"
}

Write-Host "`nHAZIR: $output"
