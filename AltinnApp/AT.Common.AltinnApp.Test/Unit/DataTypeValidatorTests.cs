using Altinn.App.Core.Models.Validation;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.AltinnApp.Abstract;
using Arbeidstilsynet.Common.AltinnApp.Test.Unit.TestFixtures;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.AltinnApp.Test.Unit;

public class DataTypeValidatorTests
{
    private readonly Instance _instance = AltinnData.CreateTestInstance();
    private readonly DataElement _dataElement = new() { Id = Guid.NewGuid().ToString() };

    [Fact]
    public async Task ValidateFormData_FindsDirectProperty()
    {
        var sut = new AddressValidator();
        var model = new PersonModel { Home = new Address { Street = "Main St" } };

        var issues = await sut.ValidateFormData(_instance, _dataElement, model, null);

        issues.ShouldHaveSingleItem();
        issues[0].Field.ShouldBe("Home");
        issues[0].CustomTextKey.ShouldBe("Main St");
    }

    [Fact]
    public async Task ValidateFormData_FindsNestedProperty()
    {
        var sut = new AddressValidator();
        var model = new PersonModel
        {
            Employer = new Company { Office = new Address { Street = "Office Rd" } },
        };

        var issues = await sut.ValidateFormData(_instance, _dataElement, model, null);

        issues.ShouldHaveSingleItem();
        issues[0].Field.ShouldBe("Employer.Office");
        issues[0].CustomTextKey.ShouldBe("Office Rd");
    }

    [Fact]
    public async Task ValidateFormData_FindsItemsInCollection()
    {
        var sut = new AddressValidator();
        var model = new PersonModel
        {
            PreviousAddresses =
            [
                new Address { Street = "First" },
                new Address { Street = "Second" },
            ],
        };

        var issues = await sut.ValidateFormData(_instance, _dataElement, model, null);

        issues.Count.ShouldBe(2);
        issues[0].Field.ShouldBe("PreviousAddresses[0]");
        issues[0].CustomTextKey.ShouldBe("First");
        issues[1].Field.ShouldBe("PreviousAddresses[1]");
        issues[1].CustomTextKey.ShouldBe("Second");
    }

    [Fact]
    public async Task ValidateFormData_FindsItemsInNestedCollection()
    {
        var sut = new AddressValidator();
        var model = new PersonModel
        {
            Employer = new Company
            {
                Branches = [new Address { Street = "Branch1" }, new Address { Street = "Branch2" }],
            },
        };

        var issues = await sut.ValidateFormData(_instance, _dataElement, model, null);

        issues.Count.ShouldBe(2);
        issues[0].Field.ShouldBe("Employer.Branches[0]");
        issues[1].Field.ShouldBe("Employer.Branches[1]");
    }

    [Fact]
    public async Task ValidateFormData_FindsAllInstancesAcrossGraph()
    {
        var sut = new AddressValidator();
        var model = new PersonModel
        {
            Home = new Address { Street = "Home" },
            Employer = new Company
            {
                Office = new Address { Street = "Office" },
                Branches = [new Address { Street = "Branch" }],
            },
            PreviousAddresses = [new Address { Street = "Prev" }],
        };

        var issues = await sut.ValidateFormData(_instance, _dataElement, model, null);

        issues.Count.ShouldBe(4);
        issues
            .Select(i => i.Field)
            .ShouldBe(
                ["Home", "Employer.Office", "Employer.Branches[0]", "PreviousAddresses[0]"],
                ignoreOrder: false
            );
    }

    [Fact]
    public async Task ValidateFormData_NullProperties_AreSkipped()
    {
        var sut = new AddressValidator();
        var model = new PersonModel
        {
            Home = null,
            Employer = null,
            PreviousAddresses = null!,
        };

        var issues = await sut.ValidateFormData(_instance, _dataElement, model, null);

        issues.ShouldBeEmpty();
    }

    [Fact]
    public async Task ValidateFormData_EmptyCollections_ReturnsNoIssues()
    {
        var sut = new AddressValidator();
        var model = new PersonModel { PreviousAddresses = [] };

        var issues = await sut.ValidateFormData(_instance, _dataElement, model, null);

        issues.ShouldBeEmpty();
    }

    [Fact]
    public async Task ValidateFormData_WrongDataType_ReturnsEmpty()
    {
        var sut = new AddressValidator();

        var issues = await sut.ValidateFormData(_instance, _dataElement, "not a model", null);

        issues.ShouldBeEmpty();
    }

    [Fact]
    public void HasRelevantChanges_WhenInstanceChanged_ReturnsTrue()
    {
        var sut = new AddressValidator();

        var result = sut.HasRelevantChanges(
            new PersonModel { Home = new Address { Street = "New" } },
            new PersonModel { Home = new Address { Street = "Old" } }
        );

        result.ShouldBeTrue();
    }

    [Fact]
    public void HasRelevantChanges_WhenInstanceAdded_ReturnsTrue()
    {
        var sut = new AddressValidator();

        var result = sut.HasRelevantChanges(
            new PersonModel { Home = new Address { Street = "A" } },
            new PersonModel { Home = null }
        );

        result.ShouldBeTrue();
    }

    [Fact]
    public void HasRelevantChanges_WhenUnchanged_ReturnsFalse()
    {
        var sut = new AddressValidator();
        var addr = new Address { Street = "Same" };

        var result = sut.HasRelevantChanges(
            new PersonModel { Home = addr },
            new PersonModel { Home = addr }
        );

        result.ShouldBeFalse();
    }

    [Fact]
    public void HasRelevantChanges_WrongType_ReturnsFalse()
    {
        var sut = new AddressValidator();

        sut.HasRelevantChanges("not a model", "also not").ShouldBeFalse();
    }

    [Fact]
    public void CircularTypeReference_ThrowsOnFirstUse()
    {
        var sut = new CircularValidator();

        Should.Throw<InvalidOperationException>(() =>
            sut.ValidateFormData(_instance, _dataElement, new CircularModel(), null)
        );
    }

    [Fact]
    public async Task ValidateFormData_ArrayProperty_IsWalked()
    {
        var sut = new AddressValidator();
        var model = new PersonModel
        {
            AddressArray = [new Address { Street = "Arr0" }, new Address { Street = "Arr1" }],
        };

        var issues = await sut.ValidateFormData(_instance, _dataElement, model, null);

        issues.Count.ShouldBe(2);
        issues[0].Field.ShouldBe("AddressArray[0]");
        issues[1].Field.ShouldBe("AddressArray[1]");
    }

    [Fact]
    public async Task ValidateFormData_FindsTargetsNestedInsideCollectionItems()
    {
        var sut = new AddressValidator();
        var model = new PersonModel
        {
            Companies =
            [
                new Company
                {
                    Office = new Address { Street = "HQ" },
                    Branches = [new Address { Street = "B1" }],
                },
                new Company { Office = new Address { Street = "Remote" } },
            ],
        };

        var issues = await sut.ValidateFormData(_instance, _dataElement, model, null);

        issues.Count.ShouldBe(3);
        issues[0].Field.ShouldBe("Companies[0].Office");
        issues[0].CustomTextKey.ShouldBe("HQ");
        issues[1].Field.ShouldBe("Companies[0].Branches[0]");
        issues[1].CustomTextKey.ShouldBe("B1");
        issues[2].Field.ShouldBe("Companies[1].Office");
        issues[2].CustomTextKey.ShouldBe("Remote");
    }

    #region Test models

    public class Address
    {
        public string? Street { get; set; }
    }

    public class Company
    {
        public Address? Office { get; set; }
        public List<Address> Branches { get; set; } = [];
    }

    public class PersonModel
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public Address? Home { get; set; }
        public Company? Employer { get; set; }
        public List<Address> PreviousAddresses { get; set; } = [];
        public Address[]? AddressArray { get; set; }
        public List<Company> Companies { get; set; } = [];
    }

    public class CircularModel
    {
        public CircularModel? Self { get; set; }
    }

    #endregion

    #region Test validators

    private class AddressValidator : DataTypeValidator<PersonModel, Address>
    {
        protected override Task<List<ValidationIssue>> ValidateInstance(
            Address instance,
            string path
        ) =>
            Task.FromResult(
                new List<ValidationIssue>
                {
                    new()
                    {
                        Field = path,
                        Severity = ValidationIssueSeverity.Error,
                        CustomTextKey = instance.Street ?? "null",
                    },
                }
            );
    }

    private class CircularValidator : DataTypeValidator<CircularModel, Address>
    {
        protected override Task<List<ValidationIssue>> ValidateInstance(
            Address instance,
            string path
        ) => Task.FromResult(new List<ValidationIssue>());
    }

    #endregion
}
