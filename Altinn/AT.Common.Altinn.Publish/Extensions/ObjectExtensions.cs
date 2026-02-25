namespace Arbeidstilsynet.Common.Altinn.Extensions;

internal static class ObjectExtensions
{
    public enum MergeStrategy
    {
        SetNull,
        IgnoreNull,
    }

    public static T Merge<T>(
        this T source,
        T? patch,
        MergeStrategy mergeStrategy = MergeStrategy.IgnoreNull
    )
        where T : notnull
    {
        var result = Activator.CreateInstance<T>();

        patch ??= Activator.CreateInstance<T>();

        foreach (var property in typeof(T).GetProperties().Where(p => p.CanWrite))
        {
            var patchValue = property.GetValue(patch);
            var sourceValue = property.GetValue(source);

            if (patchValue != null)
            {
                property.SetValue(result, patchValue);
            }
            else
            {
                if (mergeStrategy == MergeStrategy.SetNull)
                {
                    property.SetValue(result, null);
                }
                else if (mergeStrategy == MergeStrategy.IgnoreNull)
                {
                    property.SetValue(result, sourceValue);
                }
            }
        }

        return result;
    }
}
