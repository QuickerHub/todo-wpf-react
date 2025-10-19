# TodoApp Setup Test Script
# This script tests if the project is properly configured

Write-Host "🧪 Testing TodoApp Configuration..." -ForegroundColor Green

$allTestsPassed = $true

# Test 1: Check if pnpm is installed
Write-Host "`n1️⃣ Testing pnpm installation..." -ForegroundColor Yellow
try {
    $pnpmVersion = pnpm --version
    Write-Host "  ✅ pnpm version: $pnpmVersion" -ForegroundColor Green
} catch {
    Write-Host "  ❌ pnpm is not installed" -ForegroundColor Red
    $allTestsPassed = $false
}

# Test 2: Check if .NET is installed
Write-Host "`n2️⃣ Testing .NET installation..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "  ✅ .NET version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "  ❌ .NET is not installed" -ForegroundColor Red
    $allTestsPassed = $false
}

# Test 3: Check if frontend dependencies are installed
Write-Host "`n3️⃣ Testing frontend dependencies..." -ForegroundColor Yellow
Set-Location "TodoApp.Web"
if (Test-Path "node_modules") {
    Write-Host "  ✅ Frontend dependencies are installed" -ForegroundColor Green
} else {
    Write-Host "  ⚠️ Frontend dependencies not found, installing..." -ForegroundColor Yellow
    pnpm install
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✅ Frontend dependencies installed successfully" -ForegroundColor Green
    } else {
        Write-Host "  ❌ Failed to install frontend dependencies" -ForegroundColor Red
        $allTestsPassed = $false
    }
}
Set-Location ".."

# Test 4: Test WPF project build
Write-Host "`n4️⃣ Testing WPF project build..." -ForegroundColor Yellow
Set-Location "TodoApp.Desktop"
dotnet build --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✅ WPF project builds successfully" -ForegroundColor Green
} else {
    Write-Host "  ❌ WPF project build failed" -ForegroundColor Red
    $allTestsPassed = $false
}
Set-Location ".."

# Test 5: Test API project build
Write-Host "`n5️⃣ Testing API project build..." -ForegroundColor Yellow
Set-Location "TodoApp.Api"
dotnet build --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✅ API project builds successfully" -ForegroundColor Green
} else {
    Write-Host "  ❌ API project build failed" -ForegroundColor Red
    $allTestsPassed = $false
}
Set-Location ".."

# Test 6: Test React build
Write-Host "`n6️⃣ Testing React build..." -ForegroundColor Yellow
Set-Location "TodoApp.Web"
pnpm run build --silent
if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✅ React project builds successfully" -ForegroundColor Green
} else {
    Write-Host "  ❌ React project build failed" -ForegroundColor Red
    $allTestsPassed = $false
}
Set-Location ".."

# Test 7: Check if scripts exist
Write-Host "`n7️⃣ Testing development scripts..." -ForegroundColor Yellow
if (Test-Path "dev.ps1") {
    Write-Host "  ✅ dev.ps1 script exists" -ForegroundColor Green
} else {
    Write-Host "  ❌ dev.ps1 script not found" -ForegroundColor Red
    $allTestsPassed = $false
}

if (Test-Path "publish.ps1") {
    Write-Host "  ✅ publish.ps1 script exists" -ForegroundColor Green
} else {
    Write-Host "  ❌ publish.ps1 script not found" -ForegroundColor Red
    $allTestsPassed = $false
}

# Final results
Write-Host "`n" + "="*50 -ForegroundColor Gray
if ($allTestsPassed) {
    Write-Host "🎉 All tests passed! The project is ready for development." -ForegroundColor Green
    Write-Host "`n📋 Next steps:" -ForegroundColor Cyan
    Write-Host "  • Run .\dev.ps1 to start development environment" -ForegroundColor White
    Write-Host "  • Run .\publish.ps1 to build for production" -ForegroundColor White
    Write-Host "  • Check README.md for detailed instructions" -ForegroundColor White
} else {
    Write-Host "❌ Some tests failed. Please check the errors above." -ForegroundColor Red
    Write-Host "`n🔧 Troubleshooting:" -ForegroundColor Yellow
    Write-Host "  • Make sure .NET 8.0 SDK is installed" -ForegroundColor White
    Write-Host "  • Make sure pnpm is installed: npm install -g pnpm" -ForegroundColor White
    Write-Host "  • Try running the build commands manually" -ForegroundColor White
}
Write-Host "`nPress any key to continue..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
