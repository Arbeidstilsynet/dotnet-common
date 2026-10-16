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

    // Country options with custom configuration
    services.AddLandOptions(new LandOptionsConfiguration
    {
        OptionsId = "land",                                        // default
        OptionValueIsoType = LandOptionsConfiguration.IsoType.Alpha3, // default; or Alpha2
        CustomOrderFunc = countries => countries.OrderBy(c => c.Land), // default is alphabetical
    });
}
```

### `LandOptionsConfiguration`

| Property | Type | Default | Description |
|---|---|---|---|
| `OptionsId` | `string` | `"land"` | The Altinn optionsId used to identify this option list. |
| `CustomOrderFunc` | `Func<IEnumerable<Landskode>, IEnumerable<Landskode>>?` | `null` (alphabetical) | Custom ordering function for the list of countries. |
| `OptionValueIsoType` | `LandOptionsConfiguration.IsoType` | `Alpha3` | Which ISO code to use as the option value (`Alpha3` or `Alpha2`). |

---

## Structured Data

Attach a strongly-typed `DataElement` to your Altinn instance to hold computed/derived data.

### 1. Register in DI

Two overloads are available — one accepting an explicit `StructuredDataConfiguration`, and one accepting `IHostEnvironment` (which auto-configures `IncludeErrorDetails` based on the environment):

```csharp
// Option A: Explicit configuration
services.AddStructuredData<TDataModel, TStructuredData>(
    dataModel => new TStructuredData { /* map properties */ },
    new StructuredDataConfiguration
    {
        StructuredDataTypeId = "structured-data",       // default
        MainPdfDataTypeId = "ref-data-as-pdf",          // default
        IncludeErrorDetails = false,                    // default
    });

// Option B: Environment-based (IncludeErrorDetails = true in non-production)
services.AddStructuredData<TDataModel, TStructuredData>(
    dataModel => new TStructuredData { /* map properties */ },
    builder.Environment);
```

#### `StructuredDataConfiguration`

| Property | Type | Default | Description |
|---|---|---|---|
| `StructuredDataTypeId` | `string` | `"structured-data"` | The `DataElement.DataType` id for the structured data element. |
| `MainPdfDataTypeId` | `string` | `"ref-data-as-pdf"` | The `DataElement.DataType` id for the main PDF document. |
| `IncludeErrorDetails` | `bool` | `false` | Whether to include error details in the structured data on mapping errors. |

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

### 3. Configure form-model cleanup in application metadata

The package no longer deletes the form model; `KeepAppDataModelAfterMapping` was removed in 2.12.1. Merge this fragment into the existing **form-model** data type, preserving `classRef` and other properties:

```json
{
  "id": "skjema",
  "appLogic": {
    "autoDeleteOnProcessEnd": true
  }
}
```

Leave automatic deletion disabled for tests or applications that intentionally retain the model.

### Learnings

In Altinn.App.Core 8.12.11's default lifecycle, automatic data-type deletion runs before process-completed event registration, while app-defined `IProcessEnd` handlers run after registration. This ordering existed when `IProcessEnd` was introduced; it was not a change in 8.12.11. Never use `IProcessEnd` cleanup, validation, or post-processing as a pre-event barrier: post-processors and explicit recovery must tolerate an already-deleted model, and retries are not guaranteed. Keep structured-data insertion awaited at task end, synchronize successful package mutations into `instance.Data`, and refresh storage once when process-end structured data is missing locally; post-processors must use fresh state including framework-generated PDFs.

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

`ListProcessor` extends `MemberProcessor` for list properties. It seals `ProcessMember` to iterate items by their `AltinnRowId` (a `Guid` property required on `TItem`) and calls `ProcessItem` for each added, removed, or changed item.

```csharp
public class AttachmentListProcessor : ListProcessor<MyDataModel, AttachmentInfo>
{
    protected override List<AttachmentInfo>? AccessMember(MyDataModel dataModel)
        => dataModel.Attachments;

    protected override async Task ProcessItem(
        AttachmentInfo? currentItem,
        AttachmentInfo? previousItem,
        MyDataModel currentDataModel,
        MyDataModel previousDataModel)
    {
        // currentItem is null  → item was deleted
        // previousItem is null → item was added
        // both non-null        → item was changed
    }
}
```

### `PreSubmitDataModelProcessor<TDataModel>` - Task-end model processing

Implements `IProcessTaskEnd`, which runs before automatic form-model deletion. Retrieves the form model, calls `ProcessDataModel`, and saves the result. Use `TaskIdFilter` to restrict execution to specific tasks. Task end does not necessarily mean final submission, and this base class does not establish ordering relative to other task-end handlers.

```csharp
public class MyPreSubmitProcessor : PreSubmitDataModelProcessor<MyDataModel>
{
    public MyPreSubmitProcessor(IDataClient dataClient, IApplicationClient applicationClient)
        : base(dataClient, applicationClient) { }

    protected override Task<MyDataModel> ProcessDataModel(
        MyDataModel currentDataModel,
        Instance instance)
    {
        currentDataModel.SubmittedAt = DateTime.UtcNow;
        return Task.FromResult(currentDataModel);
    }
}
```

For explicit ordering before dependent mapping or PDF generation, consider a BPMN service task. Structured-data post-processors run after event registration and automatic model deletion and are not equivalent to task-end form-model processing.

### Registration

```csharp
services.AddTransient<IDataProcessor, MyFormDataProcessor>();
services.AddTransient<IDataProcessor, EmailChangeProcessor>();
services.AddTransient<IDataProcessor, AttachmentListProcessor>();
services.AddTransient<IProcessTaskEnd, MyPreSubmitProcessor>();
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

## ApplicationClient Extensions

```csharp
using Arbeidstilsynet.Common.AltinnApp.Extensions;

// Get the DataElement for your data model type (matched via AppLogic.ClassRef)
DataElement element = await applicationClient.GetRequiredDataModelElement<MyDataModel>(instance);
// Throws InvalidOperationException if the application, data type, or data element is not found.
```

---

## Country Code Lookup (`ILandskodeLookup`)

```csharp
public class AddressService(ILandskodeLookup landskodeLookup)
{
    public async Task<string> GetCountryInfo(string isoCode)
    {
        // Look up a single country by alpha-3 code
        var country = await landskodeLookup.GetLandskode("NOR");
        return country != null ? $"{country.Land} ({country.Kode})" : "Unknown";
        // → "Norway (+47)"
    }

    public async Task<IEnumerable<string>> GetAllCountryNames()
    {
        // Get all available country codes
        var all = await landskodeLookup.GetLandskoder();
        return all.Select(kvp => kvp.Value.Land);
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

1. Choose the appropriate base class: `BaseDataProcessor`, `MemberProcessor`, `ListProcessor`, or `PreSubmitDataModelProcessor`
2. Implement the abstract methods (`ProcessData`, `ProcessMember`, `ProcessItem`, or `ProcessDataModel`)
3. Register as `services.AddTransient<IDataProcessor, MyProcessor>()` (or `IProcessTaskEnd` for `PreSubmitDataModelProcessor`)
4. Test by submitting form data changes in a local Altinn app instance
