# TodoApp Development Environment Startup Script
# This script starts all three components of the hybrid application

Write-Host "🚀 Starting TodoApp Development Environment..." -ForegroundColor Green

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

Write-Host "`n📦 Installing frontend dependencies..." -ForegroundColor Yellow
Set-Location "TodoApp.Web"
pnpm install
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to install frontend dependencies" -ForegroundColor Red
    exit 1
}
Set-Location ".."

Write-Host "`n🔧 Starting ASP.NET Core API server..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd TodoApp.Api; Write-Host '🔧 Starting API Server...' -ForegroundColor Yellow; dotnet run" -WindowStyle Normal

# Wait a moment for the API server to start
Start-Sleep -Seconds 3

Write-Host "`n⚛️ Starting React development server..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd TodoApp.Web; Write-Host '⚛️ Starting React Dev Server...' -ForegroundColor Yellow; pnpm run dev" -WindowStyle Normal

# Wait a moment for the React server to start
Start-Sleep -Seconds 3

Write-Host "`n🖥️ Starting WPF desktop application..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd TodoApp.Desktop; Write-Host '🖥️ Starting WPF App...' -ForegroundColor Yellow; dotnet run" -WindowStyle Normal

Write-Host "`n🎉 All services started successfully!" -ForegroundColor Green
Write-Host "`n📋 Service URLs:" -ForegroundColor Cyan
Write-Host "  • React Dev Server: http://localhost:5173" -ForegroundColor White
Write-Host "  • ASP.NET Core API: http://localhost:5000" -ForegroundColor White
Write-Host "  • API Documentation: http://localhost:5000/swagger" -ForegroundColor White
Write-Host "  • WPF Desktop App: Will open automatically" -ForegroundColor White

Write-Host "`n💡 Tips:" -ForegroundColor Yellow
Write-Host "  • Frontend changes will hot-reload automatically" -ForegroundColor White
Write-Host "  • Backend changes require restarting the API server" -ForegroundColor White
Write-Host "  • Use Ctrl+C in each terminal to stop services" -ForegroundColor White

Write-Host "`nPress any key to continue..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
