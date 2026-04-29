---
name: altinn-app
description: Utilities for building Altinn applications — data processors, country code lookup, instance/data-client extensions, structured data, and embedded resource helpers using AT.Common.AltinnApp. Use this skill when developing or maintaining an Altinn app (altinn/app-lib-dotnet based) and needing shared cross-cutting utilities.
license: MIT
metadata:
  domain: altinn-app
  tags: altinn altinn-app data-processor country-code instance extensions structured-data dotnet
---

# AltinnApp Skill — Arbeidstilsynet/dotnet-common

`Arbeidstilsynet.Common.AltinnApp` (`AT.Common.AltinnApp.Publish`) provides reusable patterns and utilities for Altinn applications built on `altinn/app-lib-dotnet`.

---

## Installation

```bash
dotnet add package Arbeidstilsynet.Common.AltinnApp
```

---

## Dependency Injection Setup

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Country code lookup only
    services.AddLandskoder();

    // Country options for Altinn dropdowns (includes AddLandskoder)
    services.AddLandOptions();
}
```

---

## Structured Data

Attach a strongly-typed `DataElement` to your Altinn instance to hold computed/derived data.

### 1. Register in DI

```csharp
services.AddStructuredData<TDataModel, TStructuredData>(dataModel =>
    new TStructuredData
    {
        // Map properties from the form data model
    });
```

### 2. Declare the data type in `App/config/applicationmetadata.json`

```json
{
  "dataTypes": [
    {
      "id": "structured-data",
      "allowedContentTypes": ["application/json"],
      "allowedContributors": ["app:owned"]
    }
  ]
}
```

---

## Data Processors

Data processors react to form data changes. Register them as `IDataProcessor` in DI.

### `BaseDataProcessor<TDataModel>` — Full model diff

```csharp
public class MyFormDataProcessor : BaseDataProcessor<MyDataModel>
{
    protected override async Task ProcessData(MyDataModel current, MyDataModel? previous)
    {
        if (previous == null)
        {
            // New form data created
            return;
        }
        // React to updates
    }
}
```

### `MemberProcessor<TDataModel, TMember>` — Single property diff

```csharp
public class EmailChangeProcessor : MemberProcessor<MyDataModel, string>
{
    protected override string? AccessMember(MyDataModel dataModel) => dataModel.Email;

    protected override async Task ProcessMember(
        string? currentEmail,
        string? previousEmail,
        MyDataModel currentDataModel,
        MyDataModel previousDataModel)
    {
        // React to email changes only
    }
}
```

### `ListProcessor<TDataModel, TItem>` — Collection diff (add/remove/item change)

```csharp
public class AttachmentListProcessor : ListProcessor<MyDataModel, AttachmentInfo>
{
    protected override List<AttachmentInfo>? AccessMember(MyDataModel dataModel)
        => dataModel.Attachments;

    protected override async Task ProcessListChange(
        List<AttachmentInfo>? currentList,
        List<AttachmentInfo>? previousList,
        MyDataModel currentDataModel,
        MyDataModel previousDataModel)
    {
        // React to the list being added to or removed from
    }

    protected override async Task ProcessItem(
        AttachmentInfo currentItem,
        AttachmentInfo previousItem,
        MyDataModel currentDataModel,
        MyDataModel previousDataModel)
    {
        // React to individual item changes
    }
}
```

### Registration

```csharp
services.AddTransient<IDataProcessor, MyFormDataProcessor>();
services.AddTransient<IDataProcessor, EmailChangeProcessor>();
services.AddTransient<IDataProcessor, AttachmentListProcessor>();
```

---

## Instance Extensions

Extract common information from Altinn `Instance` objects:

```csharp
using Arbeidstilsynet.Common.AltinnApp.Extensions;

Guid instanceId       = instance.GetInstanceGuid();
string appName        = instance.GetAppName();
int partyId           = instance.GetInstanceOwnerPartyId();
```

---

## DataClient Extensions

Simplified wrappers around `IDataClient`:

```csharp
using Arbeidstilsynet.Common.AltinnApp.Extensions;

// Get form data (default data type: "structured-data")
var formData = await dataClient.GetSkjemaData<MyDataModel>(instance);

// Get form data with a custom data type
var formData = await dataClient.GetSkjemaData<MyDataModel>(instance, "customDataType");

// Delete a data element (delayed deletion)
await dataClient.DeleteElement(instance, attachment, delay: true);
```

---

## Country Code Lookup (`ILandskodeLookup`)

```csharp
public class AddressService(ILandskodeLookup landskodeLookup)
{
    public async Task<string> GetCountryInfo(string isoCode)
    {
        var country = await landskodeLookup.GetLandskode("NOR");
        return country != null ? $"{country.Land} ({country.Kode})" : "Unknown";
        // → "Norway (+47)"
    }
}
```

---

## Embedded Resource Utilities

Load and deserialise embedded JSON resources from an assembly:

```csharp
using Arbeidstilsynet.Common.AltinnApp.Extensions;

var assembly = Assembly.GetExecutingAssembly();
var resource = await assembly.GetEmbeddedResource<MyResourceType>("resource.json");
```

---

## Adding a New Data Processor — Checklist

1. Choose the appropriate base class: `BaseDataProcessor`, `MemberProcessor`, or `ListProcessor`
2. Implement the abstract `ProcessData` / `ProcessMember` / `ProcessListChange` + `ProcessItem` methods
3. Register as `services.AddTransient<IDataProcessor, MyProcessor>()`
4. Test by submitting form data changes in a local Altinn app instance
