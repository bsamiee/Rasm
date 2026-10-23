using System.Reflection;
using System.Runtime.Loader;
using System.Xml.Linq;
using Rasm.Rhino.UI;

namespace Rasm.Rui;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class RuiFiles {
    // --- [RESOLUTION]
    public static IO<Assembly> Load(string plugIn, Seq<string> references) =>
        references.Fold(HashMap<string, string>(), static (held, path) => held.TryAdd(Path.GetFileNameWithoutExtension(path), path)) switch {
            var located => IO.lift(() => {
                AssemblyLoadContext.Default.Resolving += (context, name) => Optional(name.Name).Bind(located.Find).Map(context.LoadFromAssemblyPath).ValueUnsafe();
                return AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(plugIn));
            }),
        };

    // --- [DOCUMENT]
    public static IO<Option<XDocument>> Document(Assembly plugIn) =>
        IO.lift(() =>
            from library in toSeq(plugIn.GetReferencedAssemblies())
                .Find(static reference => reference.Name?.EndsWith($"{typeof(Toolbars).Assembly.GetName().Name}", StringComparison.Ordinal) is true)
                .ToFin(Error.New($"{plugIn.GetName().Name} references no {typeof(Toolbars).Assembly.GetName().Name} copy"))
            from type in Optional(AssemblyLoadContext.Default.LoadFromAssemblyName(library).GetType($"{typeof(Toolbars).FullName}"))
                .ToFin(Error.New($"{library.Name} holds no {typeof(Toolbars).FullName}"))
            from method in Optional(type.GetMethod(nameof(Toolbars.Document), [typeof(Assembly)]))
                .ToFin(Error.New($"{type.FullName} holds no {nameof(Toolbars.Document)}"))
            from document in method.Invoke(obj: null, [plugIn]) is Fin<Option<XDocument>> written
                ? written
                : Error.New($"{type.FullName}.{nameof(Toolbars.Document)} answers no {nameof(Fin<>)}<{nameof(XDocument)}>")
            select document);

    public static IO<Unit> Write(Option<XDocument> document, string path) =>
        IO.lift(() => document.Match(Some: held => held.Save(path), None: () => File.Delete(path)));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
internal static class Program {
    public static int Main(string[] args) =>
        (from request in IO.lift(Fin<(string PlugIn, string Rui, string References)> () => args is [var plugIn, var rui, var references]
                ? (plugIn, rui, references)
                : Error.New("Pass the plug-in assembly, its RUI path, and the file listing its reference assembly paths"))
         from references in IO.liftAsync(env => File.ReadAllLinesAsync(request.References, env.Token))
         from plugIn in RuiFiles.Load(request.PlugIn, toSeq(references))
         from document in RuiFiles.Document(plugIn)
         from written in RuiFiles.Write(document, request.Rui)
         select written).RunSafe().Match(
            Succ: static _ => 0,
            Fail: static error => {
                Console.Error.WriteLine(string.Join('\n', error.AsIterable().Map(static leaf => leaf.IsExceptional ? leaf.ToException().ToString() : leaf.Message)));
                return 1;
            });
}
