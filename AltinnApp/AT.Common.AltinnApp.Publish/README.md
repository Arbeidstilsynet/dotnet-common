# Introduction

A collection of common patterns and extensions for cross-cutting concerns in Altinn applications.

## 📖 Installation

To install the package, you can use the following command in your terminal:

```bash
dotnet add package Arbeidstilsynet.Common.AltinnApp
```

## 🚀 Features

- **Extension Methods** for common Altinn operations
- **Abstract Data Processors** for handling data changes
- **Country Code Lookup** with country dialing codes and names
- **Altinn Options Provider** for country dropdowns
- **Embedded Resource Utilities** for JSON data

## 🧑‍💻 Usage

### Dependency Injection Setup

#### Altinn Application Utilities

Add services to your service collection:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Add country code lookup functionality
    services.AddLandskoder();
    
    // OR add country options for Altinn dropdowns (includes AddLandskoder)
    services.AddLandOptions();

}
```

### Structured Data

#### 1. Implement a mapper and register it, specifying the data model and structured data types

The mapper is resolved from the service provider, so it can use constructor injection (e.g. `ILandskodeLookup`) and be unit tested on its own.

```csharp
internal class MeldingMapper(ILandskodeLookup landskoder)
    : IStructuredDataMapper<MeldingDataModel, MeldingStructuredData>
{
    public async Task<MeldingStructuredData> Map(MeldingDataModel dataModel)
    {
        var land = await landskoder.GetLandskode(dataModel.LandKode);
        return new MeldingStructuredData
        {
            // Property mappings here
        };
    }
}
```

```csharp
    // Adds a DataElement to your instance to hold structured data of type MeldingStructuredData.
    // Content type will be "application/json".
    services.AddStructuredData<MeldingDataModel, MeldingStructuredData, MeldingMapper>(env);
```

Alternatively, adjust the environment-based defaults (error details are included outside production):

```csharp
    services.AddStructuredData<MeldingDataModel, MeldingStructuredData, MeldingMapper>(
        env,
        config => config with { TaskIdFilter = ["Task_1"] }
    );
```

> The `AddStructuredData<TDataModel, TStructuredData>(Func<TDataModel, TStructuredData> mapFunc, ...)` overloads are obsolete. Move the lambda into an `IStructuredDataMapper` implementation instead.

#### 2. Add the data element to your App/config/applicationmetadata.json dataTypes


```json
{
  "dataTypes": [
    {
      "id": "structured-data",
      "allowedContentTypes": [
        "application/json"
      ],
      "allowedContributors": [
        "app:owned"
      ]
    },
    /// ...Other data types
  ]
}

```

#### 3. (Optional) Post-process the instance and data elements

Implement `IStructuredDataPostProcessor` to run logic from inside the structured data feature at process end. Unlike a plain `IProcessEnd`, a post-processor is guaranteed that both the structured data element and the main PDF element exist on the instance. The instance is freshly fetched from storage, including framework-created elements such as PDFs. Altinn-managed automatic form-model deletion runs before these post-processors: they must not depend on the form model still being available. Post-processors should be idempotent.

Post-processors are responsible for persisting their own changes (e.g. via `IDataClient.Update`).

```csharp
internal class MyPostProcessor(IDataClient dataClient) : IStructuredDataPostProcessor
{
    public async Task Process(StructuredDataPostProcessingContext context)
    {
        var pdf = context.MainPdfElement;
        pdf.Metadata ??= [];
        pdf.Metadata.Add(new KeyValueEntry { Key = "MyKey", Value = "MyValue" });
        await dataClient.Update(context.Instance, pdf);
    }
}

services.AddStructuredDataPostProcessor<MyPostProcessor>();
```

Multiple post-processors run in registration order.

#### Instance synchronization and process-end validation

Structured data is still mapped and its insertion awaited at **task end**. Successful package-owned insertions and deletions are reflected in the supplied `instance.Data` collection, preserving unrelated elements and avoiding duplicate entries. Failed operations propagate errors without recording a mutation that did not succeed; earlier successful deletions are not rolled back.

At process end, the configured structured-data type must exist regardless of the task filter. If it is missing locally, the package fetches the instance from storage once and replaces the supplied instance's data collection. It throws the missing-structured-data exception only if that authoritative recheck also lacks the element. Storage failures propagate instead. There are no delays, polling, or speculative retries.

Post-processors always use fresh storage state, reusing the fallback refresh when one was needed. Without post-processors, an element already present locally requires no additional lookup. Process-end handling does not require a form-model element or delete any data.

Synchronization covers mutations made by this package, not elements created by other framework components. The identified issue is stale in-memory state, not a demonstrated unfinished-write race. In Altinn.App.Core 8.12.11, process-event registration is attempted before app-defined `IProcessEnd` handlers, so this process-end guard is **not** a pre-event write barrier. Keep structured-data mapping at task end.

#### Migrating form-model cleanup ownership

**Breaking change in 2.12.1:** `KeepAppDataModelAfterMapping` and package-managed form-model deletion have been removed. Although shipped as a patch to correct the intended cleanup timing, this is a source and behavior change. Apps must explicitly configure Altinn-managed cleanup if the form-model XML should be deleted.

Deleting in `IProcessEnd` was too late to prevent an event consumer from fetching the form-model XML: process-completed event registration is attempted first. This ordering was not introduced in 8.12.11; it was already present when [`IProcessEnd` was introduced](https://github.com/Altinn/app-lib-dotnet/commit/35b534e4cf628dd163a7a882d504ed4602f11d02).

To migrate:

1. Remove assignments to `KeepAppDataModelAfterMapping` from registration callbacks and tests. Remove any `CS0618` suppressions used only for that option.

2. Enable Altinn-managed deletion on the **form-model data type**, not the structured-data or PDF type:

   ```json
   {
     "id": "skjema",
     "appLogic": {
       "autoDeleteOnProcessEnd": true
     }
   }
   ```

   This is a fragment to merge into the existing data-type configuration in `App/config/applicationmetadata.json`. Preserve the existing `classRef`, other `appLogic` properties, and data-type settings; use your actual form-model data-type ID.

3. For tests or applications that intentionally retain the model, leave `autoDeleteOnProcessEnd` disabled. Retention is now controlled solely by application metadata, not a package option. Adapt form-model-dependent post-processors and recovery before enabling automatic cleanup.

**Ordering and failure/retry implications verified against Altinn.App.Core 8.12.11:**

- [`DefaultAppEvents.OnEndAppEvent`](https://github.com/Altinn/app-lib-dotnet/blob/v8.12.11/src/Altinn.App.Core/Implementation/DefaultAppEvents.cs) awaits automatic data-element deletion, using `DeleteData` with `delay: true` (marking elements as deleted). The removed package-managed form-model cleanup used `delay: false`.
- [`EndEventEventHandler`](https://github.com/Altinn/app-lib-dotnet/blob/v8.12.11/src/Altinn.App.Core/Internal/Process/EventHandlers/EndEventEventHandler.cs) invokes this during process-event handling. [`ProcessEngine`](https://github.com/Altinn/app-lib-dotnet/blob/v8.12.11/src/Altinn.App.Core/Internal/Process/ProcessEngine.cs) handles these events, persists process state/events, and attempts event registration **before** invoking app-defined `IProcessEnd` handlers. Consequently, framework-managed model deletion occurs **before this package's guard and post-processors**.
- Automatic deletion awaits all deletion tasks. A deletion failure propagates before the app-defined process-end handlers run; successful deletions may already have occurred. There is no transactional rollback. A later guard or post-processor failure cannot undo framework cleanup or the already-persisted process transition.
- Do not assume a failing post-processor will be automatically retried by resubmission. Validate the application's explicit recovery/retry path, and keep post-processors idempotent. With automatic cleanup enabled, post-processors and recovery must not depend on the form model still being available.

Framework-managed cleanup is the idiomatic way to remove the form-model XML before process-completed event registration. The package's guard and post-processors still run later; failures there cannot prevent event registration or preserve a model already deleted by Altinn. Post-processor writes are likewise not guaranteed to precede event consumption. Reverify ordering when upgrading Altinn or overriding its app-event implementation.

Validate the metadata migration and submission-to-backend flow when upgrading, then remove app-local refresh workarounds that are no longer needed. Without the metadata setting the package now retains the form model; do not rely on cleanup to happen implicitly. Keep consumer-side form-model attachment exclusions as defense in depth.

#### Task-end form-model processing

`PreSubmitDataModelProcessor<TDataModel>` remains supported. It implements `IProcessTaskEnd`, so it processes and saves the form model before automatic process-end deletion. Register subclasses as `IProcessTaskEnd` and use `TaskIdFilter` to restrict the tasks on which they run.

Task end is not necessarily final submission, and this base class does not establish ordering relative to other task-end handlers. If model mutations must precede specific structured-data mapping or PDF generation, validate that ordering or use an explicitly placed BPMN service task. Use `IStructuredDataPostProcessor` for structured-data/PDF/attachment post-processing that can run after event registration and automatic form-model deletion, not as a replacement for task-end model mutations.

### Extension Methods

#### Instance Extensions

Easily extract common information from Altinn instances:

```csharp
using Arbeidstilsynet.Common.AltinnApp.Extensions;

public class MyService
{
    public void ProcessInstance(Instance instance)
    {
        // Extract instance GUID
        Guid instanceId = instance.GetInstanceGuid();
        
        // Get application name
        string appName = instance.GetAppName();
        
        // Get party ID of instance owner
        int partyId = instance.GetInstanceOwnerPartyId();
        
        Console.WriteLine($"Processing {appName} instance {instanceId} for party {partyId}");
    }
}
```

#### DataClient Extensions

Simplified data operations with Altinn instances:

```csharp
using Arbeidstilsynet.Common.AltinnApp.Extensions;

public class DataService
{
    private readonly IDataClient _dataClient;
    
    public DataService(IDataClient dataClient)
    {
        _dataClient = dataClient;
    }
    
    public async Task<MyDataModel?> GetFormData(Instance instance)
    {
        // Get form data with default data type "structured-data"
        return await _dataClient.GetSkjemaData<MyDataModel>(instance);
        
        // Or specify custom data type
        return await _dataClient.GetSkjemaData<MyDataModel>(instance, "customDataType");
    }
    
    public async Task CleanupAttachments(Instance instance, List<DataElement> unusedAttachments)
    {
        foreach (var attachment in unusedAttachments)
        {
            // Delete data element and remove from instance
            await _dataClient.DeleteElement(instance, attachment, delay: true);
        }
    }
}
```

#### Assembly Extensions

Load and deserialize embedded JSON resources:

```csharp
using Arbeidstilsynet.Common.AltinnApp.Extensions;

public class ConfigurationService
{
    public Task<MyResourceType> GetResource()
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        // Load embedded JSON resource and deserialize
        return assembly.GetEmbeddedResource<MyResourceType>("resource.json");
    }
}
```

### Country Code Lookup

Use the built-in country code lookup service:

```csharp
public class AddressService
{
    private readonly ILandskodeLookup _landskodeLookup;
    
    public AddressService(ILandskodeLookup landskodeLookup)
    {
        _landskodeLookup = landskodeLookup;
    }
    
    public async Task<string> GetCountryInfo(string countryCode)
    {
        // Look up specific country by ISO code
        var country = await _landskodeLookup.GetLandskode("NOR");
        if (country != null)
        {
            return $"{country.Land} ({country.Kode})"; // "Norway (+47)"
        }
        
        return "Unknown country";
    }
}
```

### Abstract Data Processors

Create data processors that respond to form data changes:

#### Simple Data Processor

```csharp
using Arbeidstilsynet.Common.AltinnApp.Abstract.Processing;

public class MyFormDataProcessor : BaseDataProcessor<MyDataModel>
{
    private readonly ILogger<MyFormDataProcessor> _logger;
    
    public MyFormDataProcessor(ILogger<MyFormDataProcessor> logger)
    {
        _logger = logger;
    }
    
    protected override async Task ProcessData(MyDataModel current, MyDataModel? previous)
    {
        if (previous == null)
        {
            _logger.LogInformation("New form data created");
            return;
        }
        
        _logger.LogInformation("Form data updated");
        
        // Your custom processing logic here
    }
}
```

#### Member-Specific Processor

```csharp
public class EmailChangeProcessor : MemberProcessor<MyDataModel, string>
{    
    protected override string? AccessMember(MyDataModel dataModel)
    {
        return dataModel.Email;
    }
    
    protected override async Task ProcessMember(
        string? currentEmail, 
        string? previousEmail, 
        MyDataModel currentDataModel, 
        MyDataModel previousDataModel)
    {
        // Your custom logic to handle the changed member
    }
}
```

#### List Processor

```csharp
public class AttachmentListProcessor : ListProcessor<MyDataModel, AttachmentInfo>
{
    protected override List<AttachmentInfo>? AccessMember(MyDataModel dataModel)
    {
        return dataModel.Attachments; // Point to the list we want to process
    }
    
    protected override async Task ProcessListChange(
        List<AttachmentInfo>? currentList, 
        List<AttachmentInfo>? previousList, 
        MyDataModel currentDataModel, 
        MyDataModel previousDataModel)
    {
        // Process that attachments have been removed or added
    }
    
    protected override async Task ProcessItem(
        AttachmentInfo currentItem, 
        AttachmentInfo previousItem, 
        MyDataModel currentDataModel, 
        MyDataModel previousDataModel)
    {
        // Process individual item changes
    }
}
```

### Registration of Data Processors

Register your data processors in dependency injection:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Register data processors
    services.AddTransient<IDataProcessor, MyFormDataProcessor>();
    services.AddTransient<IDataProcessor, EmailChangeProcessor>();
    services.AddTransient<IDataProcessor, AttachmentListProcessor>();
}
```
