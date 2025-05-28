using Arbeidstilsynet.Common.Extensions;
using Shouldly;
using Xunit;

namespace AT.Common.Extensions.Test.Unit;

public class StartupExtensionsTests
{
    [Theory]
    [InlineData("PascalCase", "pascal-case")]
    [InlineData("LongAppName", "long-app-name")]
    [InlineData("LongAppNameWithNumbers123", "long-app-name-with-numbers123")]
    [InlineData("camelCase", "camel-case")]
    public void ConvertToOtelServiceName_ValidInput_ShouldReturnConvertedName(
        string input,
        string expectedOutput
    )
    {
        // Act
        var result = input.ConvertToOtelServiceName();

        // Assert
        result.ShouldBe(expectedOutput);
    }
}
