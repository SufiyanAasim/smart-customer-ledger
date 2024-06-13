# Deployment Guide — Smart Customer Ledger

## Self-Contained Single File Deployment (.exe)

Publishing a standalone executable for Windows x64:

```powershell
dotnet publish src/CustomerLedger.Web/CustomerLedger.Web.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/
```

The compiled binary will be placed at `publish/CustomerLedger.Web.exe`.

## Docker Container Deployment

```bash
docker build -t smart-customer-ledger:v7.0.0 .
docker run -d -p 5260:5260 --name smart-customer-ledger smart-customer-ledger:v7.0.0
```
