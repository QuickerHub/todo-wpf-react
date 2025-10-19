# TodoApp Production Build Script
# This script builds the application for production deployment

Write-Host "🏗️ Building TodoApp for Production..." -ForegroundColor Green

# Check if pnpm is installed
try {
    $pnpmVersion = pnpm --version
    Write-Host "✅ pnpm version: $pnpmVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ pnpm is not installed. Please install pnpm first:" -ForegroundColor Red
    Write-Host "npm install -g pnpm" -ForegroundColor Yellow
    exit 1
}

# Check if .NET is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET is not installed. Please install .NET 8.0 SDK" -ForegroundColor Red
    exit 1
}

# Clean previous builds
Write-Host "`n🧹 Cleaning previous builds..." -ForegroundColor Yellow

$wwwrootPath = ".\TodoApp.Api\wwwroot"
if (Test-Path $wwwrootPath) {
    Write-Host "  Removing existing wwwroot directory..." -ForegroundColor Gray
    Remove-Item -Path $wwwrootPath -Recurse -Force
}

$publishPath = ".\publish"
if (Test-Path $publishPath) {
    Write-Host "  Removing existing publish directory..." -ForegroundColor Gray
    Remove-Item -Path $publishPath -Recurse -Force
}

# Build React application
Write-Host "`n⚛️ Building React application..." -ForegroundColor Yellow
Set-Location "TodoApp.Web"

Write-Host "  Installing dependencies..." -ForegroundColor Gray
pnpm install
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to install frontend dependencies" -ForegroundColor Red
    Set-Location ".."
    exit 1
}

Write-Host "  Building production bundle..." -ForegroundColor Gray
pnpm run build
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to build React application" -ForegroundColor Red
    Set-Location ".."
    exit 1
}

Set-Location ".."

# Publish the WPF application
Write-Host "`n🖥️ Publishing WPF application..." -ForegroundColor Yellow
Write-Host "  Building and publishing TodoApp.Desktop..." -ForegroundColor Gray

dotnet publish TodoApp.Desktop -c Release -o publish
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to publish WPF application" -ForegroundColor Red
    exit 1
}

# Verify the build
Write-Host "`n✅ Build completed successfully!" -ForegroundColor Green
Write-Host "`n📁 Output location: .\publish\" -ForegroundColor Cyan
Write-Host "`n📋 Build contents:" -ForegroundColor Yellow

if (Test-Path ".\publish") {
    Get-ChildItem ".\publish" | ForEach-Object {
        if ($_.PSIsContainer) {
            Write-Host "  📁 $($_.Name)/" -ForegroundColor White
        } else {
            Write-Host "  📄 $($_.Name)" -ForegroundColor White
        }
    }
}

Write-Host "`n🚀 To run the application:" -ForegroundColor Green
Write-Host "  cd publish" -ForegroundColor White
Write-Host "  .\TodoApp.Desktop.exe" -ForegroundColor White

Write-Host "`n💡 Production Notes:" -ForegroundColor Yellow
Write-Host "  • The React app is embedded in the WPF application" -ForegroundColor White
Write-Host "  • No external web server is required" -ForegroundColor White
Write-Host "  • The application uses WebView2 for rendering" -ForegroundColor White

Write-Host "`nPress any key to continue..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
