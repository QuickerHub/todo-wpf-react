# PowerShell script to generate TypeScript API client
Write-Host "🚀 Starting API client generation..." -ForegroundColor Green

# Start the API server in background
Write-Host "📡 Starting API server..." -ForegroundColor Yellow
$apiProcess = Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", "TodoApp.Api" -PassThru -WindowStyle Hidden

# Wait for API server to start
Write-Host "⏳ Waiting for API server to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

try {
    # Generate TypeScript client
    Write-Host "🔧 Generating TypeScript client..." -ForegroundColor Yellow
    nswag run nswag.json

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ TypeScript API client generated successfully!" -ForegroundColor Green
        Write-Host "📁 Output: TodoApp.Web/src/api/generated/api-client.ts" -ForegroundColor Cyan
    } else {
        Write-Host "❌ Failed to generate TypeScript client" -ForegroundColor Red
    }
} finally {
    # Stop the API server
    Write-Host "🛑 Stopping API server..." -ForegroundColor Yellow
    if ($apiProcess -and !$apiProcess.HasExited) {
        $apiProcess.Kill()
        $apiProcess.WaitForExit()
    }
}

Write-Host "🎉 Generation process completed!" -ForegroundColor Green
