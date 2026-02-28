using System.Runtime.InteropServices;
var methods = typeof(ComWrappers).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
foreach (var m in methods)
    Console.WriteLine($"{m.ReturnType.Name} {m.Name}({string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))})");
