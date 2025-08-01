using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions;

internal class CustomControllerFeatureProvider : ControllerFeatureProvider
{
    protected override bool IsController(TypeInfo typeInfo)
    {
        var isControllerPartOfEntryAssembly = Assembly.GetEntryAssembly() == typeInfo.Assembly;
        var isInternalController =
            !typeInfo.IsAbstract
            && !typeInfo.IsPublic
            && typeInfo.IsDefined(typeof(ControllerAttribute));
        // if a controller is internal it should only be added when it is a part of assembly which starts the app
        return (isControllerPartOfEntryAssembly && isInternalController)
            || base.IsController(typeInfo);
    }
}
