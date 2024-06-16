.PHONY: build run test publish clean

build:
	dotnet build

run:
	dotnet run --project src/CustomerLedger.Web

test:
	dotnet test tests/CustomerLedger.UnitTests/CustomerLedger.UnitTests.csproj

publish:
	dotnet publish src/CustomerLedger.Web/CustomerLedger.Web.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/

clean:
	dotnet clean
