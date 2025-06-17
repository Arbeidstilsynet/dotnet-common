using Arbeidstilsynet.Common.BlubExtensions.Model;

namespace Arbeidstilsynet.Common.BlubExtensions;

/// <summary>
/// Interface which can be dependency injected to use methods of BlubExtensions
/// </summary>
public interface IBlubExtensions
{
    /// <summary>
    /// Required XML summary of the Get method
    /// </summary>
    /// <returns></returns>
    Task<BlubExtensionsDto> Get();
}
