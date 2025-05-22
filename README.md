# 🌈 dotnet-common
Monorepository for felles pakker som skal publiseres og som skal bli brukt på tvers av prosjekter.

# 📦 Add new package

```cmd
dotnet new install Arbeidstilsynet.Templates.CommonPackage
dotnet new common-package -n NewFancyClient
```

Begynn med å implemtere det du vil i den nye `NewFancyClient` mappen.
Per konvensjon får du tre nye prosjekter:
* AT.Common.NewFancyClient.Adapters
* AT.Common.NewFancyClient.Ports
* AT.Common.NewFancyClient.Test

``Ports`` skal inneholde alle Type DTOs og Interfaces/Extension metoder som kan brukes via DependencyInjection. Den skal ikke ha noe som helst referanser.

``Adapters`` skal inneholde tilsvarende implementasjoner til Port Interfaces. Alt som ligger i denne pakken burde være internal. ``Adapters`` har en referanse til ``Ports``. Den her pakken er den som blir publisert til slutt.

``Test`` skal inneholder tester som kvalitetssikre integriteten av hele pakken.

# 🚧 Pre-Release

Hvis du vil teste dine endringer ved å importere det i en annen prosjekt, så kan du bruke [Prerelease Packages](https://learn.microsoft.com/en-us/nuget/create-packages/prerelease-packages). For å få det til må du bare oppdatere versjonsnummer med en `alpha`, `beta` eller `rc` suffix, f.eks. `0.0.1-alpha`. Versjonsnummer finner du i `AT.Common.NewFancyClient.Adapters.csproj`.

# 🚀 Publish

Lag en ny branch og pull request. Husk å inkrementere versjon i `AT.Common.NewFancyClient.Adapters.csproj`. Når pull requesten er merged, vil en ny release pipeline starte automatisk.
