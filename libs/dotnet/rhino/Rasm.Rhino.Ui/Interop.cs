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
}
