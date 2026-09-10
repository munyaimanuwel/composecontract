using System.Collections;
using System.Reflection;

namespace StackContract.Options;

public static class OptionsKeyExtractor
{
    public static IReadOnlyList<string> ExtractFromAssembly(string assemblyPath, IEnumerable<string>? typeNames = null)
    {
        var asm = Assembly.LoadFrom(Path.GetFullPath(assemblyPath));
        var types = typeNames?.Any() == true
            ? typeNames.Select(n => asm.GetType(n, throwOnError: false)).Where(t => t is not null).Cast<Type>()
            : asm.GetExportedTypes().Where(LooksLikeOptions);
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var type in types)
            Collect(type, prefix: null, keys, new HashSet<Type>());
        return keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static bool LooksLikeOptions(Type t) =>
        t is { IsClass: true, IsAbstract: false } &&
        (t.Name.EndsWith("Options", StringComparison.Ordinal) ||
         t.GetCustomAttributes(false).Any(a => a.GetType().Name.Contains("Options", StringComparison.Ordinal)));

    private static void Collect(Type type, string? prefix, HashSet<string> keys, HashSet<Type> seen)
    {
        if (!seen.Add(type)) return;
        foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!prop.CanRead) continue;
            var name = prop.Name;
            var path = string.IsNullOrEmpty(prefix) ? name : $"{prefix}__{name}";
            var pt = prop.PropertyType;
            if (IsScalar(pt))
            {
                keys.Add(path);
                continue;
            }
            if (typeof(IEnumerable).IsAssignableFrom(pt) && pt != typeof(string))
            {
                keys.Add(path);
                continue;
            }
            if (pt.IsClass) Collect(pt, path, keys, seen);
        }
    }

    private static bool IsScalar(Type t)
    {
        t = Nullable.GetUnderlyingType(t) ?? t;
        return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal) ||
               t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(Guid) || t == typeof(TimeSpan);
    }
}
