namespace Arbeidstilsynet.Common.Altinn.Extensions;

internal static class ObjectExtensions
{
    public static T Merge<T>(this T source, T? patch)
        where T : notnull
    {
        var result = Activator.CreateInstance<T>();

        patch ??= Activator.CreateInstance<T>();

        foreach (
            var property in typeof(T)
                .GetProperties()
                .Where(p => p is { CanWrite: true, CanRead: true })
        )
        {
            var patchValue = property.GetValue(patch);
            var sourceValue = property.GetValue(source);

            property.SetValue(result, patchValue ?? sourceValue);
        }

        return result;
    }
}
