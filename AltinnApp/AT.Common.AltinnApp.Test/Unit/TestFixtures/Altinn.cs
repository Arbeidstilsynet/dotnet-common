using Altinn.App.Core.Internal.AppModel;
using Altinn.Platform.Storage.Interface.Models;

namespace Arbeidstilsynet.Common.AltinnApp.Test.Unit.TestFixtures;

public static class AltinnData
{
    public static Instance CreateTestInstance(
        string? org = null,
        string? appId = null,
        string? partyId = null,
        string? instanceId = null,
        string? dataElementId = null,
        string? dataType = null)
    {
        return new Instance
        {
            Id = instanceId ?? "50001234/fa0678ad-960d-4307-aba2-ba29c9804c9d",
            Org = org ?? "testOrg",
            AppId = appId ?? "testOrg/testApp",
            InstanceOwner = new InstanceOwner { PartyId = partyId ?? "50001234" },
            Data = new List<DataElement>
            {
                new()
                {
                    Id = dataElementId ?? "12345678-1234-1234-1234-123456789012",
                    DataType = dataType ?? "testDataType"
                }
            }
        };
    }

    public static Application CreateTestApplication(
        string? dataTypeId = null,
        string? classRef = null)
    {
        return new Application
        {
            DataTypes = new List<DataType>
            {
                new()
                {
                    Id = dataTypeId ?? "testDataType",
                    AppLogic = new ApplicationLogic
                    {
                        ClassRef = classRef
                    }
                }
            }
        };
    }
}
