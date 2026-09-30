using Rasm.Rhino.Document;
using Rhino.NodeInCode;
using Rhino.Runtime;

namespace Rasm.Rhino.Ui;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScriptSource {
    public sealed record Script(string Text) : ScriptSource;

    public sealed record File(string Path) : ScriptSource;

    public sealed record FileInScope(string Path) : ScriptSource;

    public sealed record Compiled(PythonCompiledCode CompiledCode) : ScriptSource;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostInterop {
    // --- [PYTHON]
    public static IO<PythonScript> Engine() =>
        IO.lift(static () => Missing.Unless(PythonScript.Create(), nameof(PythonScript.Create)));

    public static IO<Unit> Run(PythonScript engine, ScriptSource source, Seq<(string Name, object Value)> bindings) =>
        from bound in IO.lift(() => Bound(engine, bindings))
        from ran in source.Switch(
            engine,
            script: static (scope, script) => IO.lift(() => Refused.Unless(scope.ExecuteScript(script.Text), nameof(PythonScript.ExecuteScript))),
            file: static (scope, file) => IO.lift(() => Refused.Unless(scope.ExecuteFile(file.Path), nameof(PythonScript.ExecuteFile))),
            fileInScope: static (scope, file) => IO.lift(() => Refused.Unless(scope.ExecuteFileInScope(file.Path), nameof(PythonScript.ExecuteFileInScope))),
            compiled: static (scope, compiled) => IO.lift(() => compiled.CompiledCode.Execute(scope)))
        select ran;

    public static IO<PythonCompiledCode> Compile(PythonScript engine, string script) =>
        IO.lift(() => Missing.Unless(engine.Compile(script), nameof(PythonScript.Compile)));

    public static IO<Option<object>> EvaluateExpression(PythonScript engine, string statements, string expression, Seq<(string Name, object Value)> bindings) =>
        from bound in IO.lift(() => Bound(engine, bindings))
        from value in IO.lift(() => Optional(engine.EvaluateExpression(statements, expression)))
        select value;

    private static Unit Bound(PythonScript engine, Seq<(string Name, object Value)> bindings) =>
        bindings.Iter(binding => engine.SetVariable(binding.Name, binding.Value));

    // --- [COMPONENTS]
    public static IO<Option<ComponentFunctionInfo>> FindComponent(string fullName) =>
        IO.lift(() => Optional(Components.FindComponent(fullName)));

    public static IO<(Seq<object> Values, Seq<string> Warnings)> Evaluate(ComponentFunctionInfo function, Seq<object> args, bool keepTree) =>
        IO.lift(() => (Values: toSeq(function.Evaluate(args, keepTree, out string[] warnings)), Warnings: toSeq(warnings)));

    // --- [CALLBACKS]
    private static readonly AtomHashMap<string, Guid> Names = AtomHashMap<string, Guid>();

    public static IO<IDisposable> Register(string name, Func<NamedParametersEventArgs, IO<Unit>> reply, Action<Error> reject) =>
        from token in IO.lift(static () => Guid.NewGuid())
        from attached in Events.AttachAll(
            Seq(
                IO.lift(() => Taken.Unless(Names.FindOrAdd(name, token) == token, nameof(HostUtils.RegisterNamedCallback), name)).Map<IDisposable>(_ => new Disposal(() => Names.Remove(name))),
                Events.Attach(h => HostUtils.RegisterNamedCallback(name, h), _ => HostUtils.RemoveNamedCallback(name), Answers.Handler(reply, reject))),
            reject)
        select attached;

    public static IO<T> Execute<T>(string name, Action<NamedParametersEventArgs> write, Func<NamedParametersEventArgs, Option<T>> read) =>
        DisposalOps.Using(static () => new NamedParametersEventArgs(), args =>
            from written in IO.lift(() => write(args))
            from answered in IO.lift(() => HostUtils.ExecuteNamedCallback(name, args))
            from reply in IO.lift(() => (answered ? read(args) : None).ToFin(new Missing(name)))
            select reply);
}
