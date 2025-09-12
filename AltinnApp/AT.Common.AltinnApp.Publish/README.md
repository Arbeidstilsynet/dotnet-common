# Introduction

A collection of common patterns and extensions for cross-cutting concerns in Altinn applications.

## 📖 Installation

To install the package, you can use the following command in your terminal:

```bash
dotnet add package Arbeidstilsynet.Common.AltinnApp
```

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
        // Get form data with default data type "skjema"
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
