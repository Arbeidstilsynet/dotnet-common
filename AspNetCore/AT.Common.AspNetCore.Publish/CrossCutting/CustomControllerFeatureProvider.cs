using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions;

internal class CustomControllerFeatureProvider : ControllerFeatureProvider
{
    protected override bool IsController(TypeInfo typeInfo)
    {
        var isControllerWithinExecutingAssembly =
            Assembly.GetExecutingAssembly() == typeInfo.Assembly;
        var isInternalController =
            !typeInfo.IsAbstract
            && !typeInfo.IsPublic
            && typeof(ControllerBase).IsAssignableFrom(typeInfo);
        // if a controller is internal it should only be added when it is a part of the executing assembly
        return (isControllerWithinExecutingAssembly && isInternalController)
            || base.IsController(typeInfo);
    }
}
