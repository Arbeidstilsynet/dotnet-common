# 🌈 dotnet-common
Monorepository for felles pakker som skal publiseres og som skal bli brukt på tvers av prosjekter.

# 📦 Add new package

```cmd
dotnet new install Arbeidstilsynet.Templates.CommonPackage
dotnet new common-package -n NewFancyClient
```

Begynn med å implemtere det du vil i den nye `NewFancyClient` mappen.
Per konvensjon får du tre nye prosjekter:
* AT.Common.NewFancyClient.Public
* AT.Common.NewFancyClient.Internal
* AT.Common.NewFancyClient.Test

``Public`` skal inneholde alle Type DTOs og Interfaces/Extension metoder som kan brukes via DependencyInjection. Den her pakken er den som blir publisert til slutt.

``Internal`` skal inneholde tilsvarende implementasjoner til Interfaces.

``Test`` skal inneholder tester som kvalitetssikre integriteten av hele pakken.

# 🚀 Publish

Lag en ny branch og pull request. Husk å inkrementere versjon i `AT.Common.NewFancyClient.Public.csproj`. Når pull requesten er merged, vil en ny release pipeline starte automatisk.
