using System.Reflection;

var core = Assembly.LoadFrom(
    Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".nuget", "packages", "flashcap.core", "1.11.0", "lib", "net8.0", "FlashCap.Core.dll"));

foreach (var t in core.GetExportedTypes().OrderBy(t => t.FullName))
{
    Console.WriteLine($"\n=== {t.FullName} ({(t.IsEnum ? "enum" : t.IsClass ? "class" : t.IsInterface ? "iface" : "struct")}) ===");
    if (t.BaseType != null && t.BaseType != typeof(object) && t.BaseType != typeof(ValueType) && t.BaseType != typeof(Enum))
        Console.WriteLine($"  : {t.BaseType.FullName}");

    if (t.IsEnum)
    {
        foreach (var name in Enum.GetNames(t))
            Console.WriteLine($"  {name} = {(int)Enum.Parse(t, name)}");
        continue;
    }

    foreach (var c in t.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
    {
        var pars = string.Join(", ", c.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
        Console.WriteLine($"  ctor({pars})");
    }

    foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        Console.WriteLine($"  prop {p.PropertyType.Name} {p.Name}");

    foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
        .Where(m => !m.IsSpecialName))
    {
        var pars = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
        Console.WriteLine($"  method {m.ReturnType.Name} {m.Name}({pars})");
    }
}
