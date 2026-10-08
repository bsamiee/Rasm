using Rasm.Rhino.Document.Files;
using Rhino;
using Rhino.NodeInCode;
using Rhino.Runtime;

namespace Rasm.Rhino.Commands;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record NodeResult(Seq<(string Name, Option<object> Value)> Outputs, Seq<string> Warnings);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public static class Scripts {
    // --- [PYTHON]
    public static IO<PythonScript> Engine(RhinoDoc doc, HashMap<string, object?> variables) =>
        from engine in IO.lift(static () => Missing.Unless(PythonScript.Create(), nameof(PythonScript.Create)))
        from mapped in IO.lift(() => Context(doc, engine))
        from configured in IO.lift(() => engine.SetupScriptContext(doc))
        from bound in IO.lift(() => variables.Iter(engine.SetVariable))
        select engine;

    public static IO<Unit> Run(PythonScript engine, Either<string, PythonCompiledCode> source) =>
        Raising(engine, source.Match(
            Left: script => IO.lift(() => Refused.Unless(engine.ExecuteScript(script), nameof(PythonScript.ExecuteScript))),
            Right: code => IO.lift(() => code.Execute(engine))));

    public static IO<Unit> RunFile(PythonScript engine, string path, bool inScope) =>
        Raising(engine,
            from existing in IO.lift(() => Exchange.ExistingPath(path))
            from ran in IO.lift(() => Refused.Unless(
                inScope ? engine.ExecuteFileInScope(existing) : engine.ExecuteFile(existing),
                inScope ? nameof(PythonScript.ExecuteFileInScope) : nameof(PythonScript.ExecuteFile)))
            select ran);

    public static IO<Option<object>> EvaluateExpression(PythonScript engine, string statements, string expression) =>
        Raising(engine,
            from configured in IO.lift(() => engine.SetupScriptContext(engine.ScriptContextDoc))
            from answer in IO.lift(() => Optional(engine.EvaluateExpression(statements, expression)))
            select answer);

    public static IO<PythonCompiledCode> Compile(PythonScript engine, string script) =>
        Raising(engine, IO.lift(() => Optional(engine.Compile(script)).ToFin(new Refused(nameof(PythonScript.Compile)))));

    public static IO<Option<object>> GetVariable(PythonScript engine, string name) =>
        IO.lift(() => Optional(engine.GetVariable(name)));

    private static IO<T> Raising<T>(PythonScript engine, IO<T> call) =>
        call | @catch(static error => error.IsExceptional, error => IO.fail<T>(new ScriptRaised(engine.GetStackTraceFromException(error.ToException()), error)));

    // --- [RHINOCODE]
    public static IO<Unit> RunFile(RhinoDoc doc, string path) =>
        from existing in IO.lift(() => Exchange.ExistingPath(path))
        from ran in Documents.RunScript(doc, $"_-ScriptEditor _Run \"{existing}\"", echo: false, display: None)
        select ran;

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
        from answer in IO.lift(() => (Values: node.Evaluate(inputs.Map(static input => input.ValueUnsafe()), keepTree, out string[] warnings), Warnings: warnings))
        from values in IO.lift(Optional(answer.Values).ToFin(new Refused(nameof(ComponentFunctionInfo.Evaluate))))
        select new NodeResult(
            toSeq(node.OutputNames).Zip(toSeq(values), static (name, value) => (Name: name, Value: Optional(value))),
            Conversions.Rows(answer.Warnings));
}
