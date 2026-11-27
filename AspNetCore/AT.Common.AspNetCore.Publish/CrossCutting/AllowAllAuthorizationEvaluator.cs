using Microsoft.AspNetCore.Authorization;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;

/// <summary>
/// Treats every authorization evaluation as successful, even if a handler called <see cref="AuthorizationHandlerContext.Fail()"/>.
/// Only registered when <see cref="AuthConfiguration.DisableAuth"/> is true.
/// </summary>
internal sealed class AllowAllAuthorizationEvaluator : IAuthorizationEvaluator
{
    public AuthorizationResult Evaluate(AuthorizationHandlerContext context) =>
        AuthorizationResult.Success();
}
