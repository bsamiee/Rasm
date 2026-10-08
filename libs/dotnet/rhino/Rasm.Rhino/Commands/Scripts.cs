using Rasm.Rhino.Document.Files;
using Rhino;
using Rhino.NodeInCode;
using Rhino.Runtime;

namespace Rasm.Rhino.Commands;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record PythonOp {
    public sealed record ExecuteScript(string Script) : PythonOp;

    public sealed record ExecuteFile(string Path) : PythonOp;

    public sealed record ExecuteFileInScope(string Path) : PythonOp;

    public sealed record Execute(PythonCompiledCode Code) : PythonOp;
}

public sealed record NodeResult(Seq<(string Name, Option<object> Value)> Outputs, Seq<string> Warnings);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Scripts {
    // --- [PYTHON]
    public static IO<PythonScript> Engine(RhinoDoc doc, HashMap<string, object?> variables) =>
        from engine in IO.lift(static () => Missing.Unless(PythonScript.Create(), nameof(PythonScript.Create)))
        from _ in IO.lift(() => engine.SetupScriptContext(doc))
        from __ in IO.lift(() => variables.Iter(engine.SetVariable))
        select engine;

    public static IO<Unit> Run(PythonScript engine, PythonOp op) =>
        Raising(engine, IO.lift(() => op.Switch<PythonScript, Fin<Unit>>(
            engine,
            executeScript: static (scope, run) => Refused.Unless(scope.ExecuteScript(run.Script), nameof(PythonScript.ExecuteScript)),
            executeFile: static (scope, run) => Exchange.ExistingPath(run.Path).Bind(path => Refused.Unless(scope.ExecuteFile(path), nameof(PythonScript.ExecuteFile))),
            executeFileInScope: static (scope, run) =>
                Exchange.ExistingPath(run.Path).Bind(path => Refused.Unless(scope.ExecuteFileInScope(path), nameof(PythonScript.ExecuteFileInScope))),
            execute: static (scope, run) => fun<PythonScript>(run.Code.Execute)(scope))));

    public static IO<Option<object>> EvaluateExpression(PythonScript engine, string statements, string expression) =>
        Raising(engine, IO.lift(() => Optional(engine.EvaluateExpression(statements, expression))));

    public static IO<PythonCompiledCode> Compile(PythonScript engine, string script) =>
        Raising(engine, IO.lift(() => Optional(engine.Compile(script)).ToFin(new Refused(nameof(PythonScript.Compile)))));

    public static IO<Option<object>> GetVariable(PythonScript engine, string name) =>
        IO.lift(() => engine.ContainsVariable(name) ? Optional(engine.GetVariable(name)) : None);

    private static IO<T> Raising<T>(PythonScript engine, IO<T> call) =>
        call | @catch(static error => error.IsExceptional, error => IO.fail<T>(new ScriptRaised(engine.GetStackTraceFromException(error.ToException()), error)));

    // --- [RHINOCODE]
    public static IO<Unit> RunFile(RhinoDoc doc, string path) =>
        IO.lift(() => Exchange.ExistingPath(path)).Bind(existing => Documents.RunScript(doc, $"_-ScriptEditor _Run \"{existing}\"", echo: false, display: None));

    // --- [NODES]
    public static IO<Seq<ComponentFunctionInfo>> NodeInCodeFunctions() =>
        IO.lift(static () => toSeq(Components.NodeInCodeFunctions.GetDynamicMembers()).Strict())
            .Catch(static error => error.HasException<ApplicationException>(), static _ => IO.fail<Seq<ComponentFunctionInfo>>(new Missing(nameof(Components.NodeInCodeFunctions))));

    public static IO<ComponentFunctionInfo> FindComponent(string fullName) =>
        IO.lift(() => Missing.Unless(Components.FindComponent(fullName), nameof(Components.FindComponent)))
            .Catch(
                static error => error.HasException<ArgumentNullException>() || error.HasException<ApplicationException>(),
                static _ => IO.fail<ComponentFunctionInfo>(new Missing(nameof(Components.FindComponent))));

    public static IO<NodeResult> Evaluate(ComponentFunctionInfo node, Seq<Option<object>> inputs, bool keepTree) =>
        IO.lift(() => Optional(node.Evaluate(inputs.Map(static input => input.ValueUnsafe()), keepTree, out string[] warnings))
            .ToFin(new Refused(nameof(ComponentFunctionInfo.Evaluate)))
            .Map(values => new NodeResult(
                toSeq(node.OutputNames).Zip(toSeq(values)).Map(static output => (Name: output.First, Value: Optional(output.Second))),
                Conversions.Rows(warnings))));
}
