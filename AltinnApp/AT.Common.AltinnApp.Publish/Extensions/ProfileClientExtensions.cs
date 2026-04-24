using Altinn.App.Core.Extensions;
using Altinn.App.Core.Internal.Profile;
using Altinn.Platform.Profile.Models;
using Microsoft.AspNetCore.Http;

namespace Arbeidstilsynet.Common.AltinnApp.Extensions;

public static class ProfileClientExtensions
{
    /// <summary>
    /// Gets the <see cref="UserProfile"/> based on the userId in the http context.
    /// </summary>
    /// <param name="profileClient"></param>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public static async Task<UserProfile?> GetUserProfile(this IProfileClient profileClient, IHttpContextAccessor httpContext)
    {
        return httpContext.GetUserId() is { } userId
               && await profileClient.GetUserProfile(userId) is { } innloggetBruker
            ? innloggetBruker
            : null;
    }
    
    private static int? GetUserId(this IHttpContextAccessor httpContext)
    {
        return httpContext.HttpContext?.User?.GetUserIdAsInt();
    }
}