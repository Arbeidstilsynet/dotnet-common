using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions;

internal class CustomControllerFeatureProvider : ControllerFeatureProvider
{
    protected override bool IsController(TypeInfo typeInfo)
    {
        var isCustomController =
            !typeInfo.IsAbstract && typeof(ControllerBase).IsAssignableFrom(typeInfo);
        return isCustomController || base.IsController(typeInfo);
    }
}
