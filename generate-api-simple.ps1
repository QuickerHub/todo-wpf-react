# Simple PowerShell script to generate TypeScript API client
Write-Host "🚀 Generating TypeScript API client..." -ForegroundColor Green

# Generate TypeScript client using local swagger file
Write-Host "🔧 Generating TypeScript client from local swagger..." -ForegroundColor Yellow

# Create a simple nswag config for local generation
$nswagConfig = @"
{
  "`$schema": "http://json.schemastore.org/nswag",
  "runtime": "Net80",
  "defaultVariables": null,
  "documentGenerator": {
    "fromDocument": {
      "json": "",
      "url": "http://localhost:5000/swagger/v1/swagger.json"
    }
  },
  "codeGenerators": {
    "openApiToTypeScriptClient": {
      "className": "ApiClient",
      "moduleName": "",
      "namespace": "",
      "typeScriptVersion": 4.3,
      "template": "Angular",
      "promiseType": "Promise",
      "httpClass": "HttpClient",
      "useSingletonProvider": true,
      "injectionTokenType": "InjectionToken",
      "rxJsVersion": 7.0,
      "dateTimeType": "Date",
      "nullValue": "Undefined",
      "generateClientClasses": true,
      "generateClientInterfaces": true,
      "generateOptionalParameters": false,
      "exportTypes": true,
      "wrapDtoExceptions": true,
      "exceptionClass": "ApiException",
      "clientClassVisibility": "public",
      "typeStyle": "Class",
      "classTypes": [],
      "extendedClasses": [],
      "extensionCode": "",
      "generateDefaultValues": true,
      "generateTypeCheckFunctions": false,
      "enumNameGenerationMode": "Auto",
      "enumValueGenerationMode": "Auto",
      "serviceHost": "",
      "serviceSchemes": [],
      "output": "TodoApp.Web/src/api/generated/api-client.ts",
      "newLineBehavior": "Auto"
    }
  }
}
"@

# Write config to temp file
$tempConfig = "nswag-temp.json"
$nswagConfig | Out-File -FilePath $tempConfig -Encoding UTF8

try {
    # Generate TypeScript client
    nswag run $tempConfig

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ TypeScript API client generated successfully!" -ForegroundColor Green
        Write-Host "📁 Output: TodoApp.Web/src/api/generated/api-client.ts" -ForegroundColor Cyan
    } else {
        Write-Host "❌ Failed to generate TypeScript client" -ForegroundColor Red
    }
} finally {
    # Clean up temp file
    if (Test-Path $tempConfig) {
        Remove-Item $tempConfig
    }
}

Write-Host "🎉 Generation process completed!" -ForegroundColor Green
