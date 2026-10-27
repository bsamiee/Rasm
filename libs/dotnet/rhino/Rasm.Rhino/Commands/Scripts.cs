using Rasm.Rhino.Document.Files;
using Rhino;
using Rhino.Runtime;

namespace Rasm.Rhino.Commands;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Scripts {
    // --- [PYTHON]
    public static IO<PythonScript> Engine(RhinoDoc doc, HashMap<string, object?> variables) =>
        from engine in IO.lift(static () => Conversions.Found(PythonScript.Create()))
        from configured in IO.lift(() => engine.SetupScriptContext(doc))
        from bound in IO.lift(() => variables.Iter(engine.SetVariable))
        select engine;

    public static IO<Unit> Run(PythonScript engine, Either<string, PythonCompiledCode> source) =>
        Raising(engine, source.Match(
            Left: script => IO.lift(() => RhinoFault.Unless(engine.ExecuteScript(script))),
            Right: code => IO.lift(() => code.Execute(engine))));

    public static IO<Unit> RunFile(PythonScript engine, InputPath path, bool inScope) =>
        Raising(engine, IO.lift(() => RhinoFault.Unless(inScope ? engine.ExecuteFileInScope(path) : engine.ExecuteFile(path))));

    public static IO<Option<object>> EvaluateExpression(PythonScript engine, string statements, string expression) =>
        Raising(engine,
            from configured in IO.lift(() => engine.SetupScriptContext(engine.ScriptContextDoc))
            from answer in IO.lift(() => Optional(engine.EvaluateExpression(statements, expression)))
            select answer);

    public static IO<PythonCompiledCode> Compile(PythonScript engine, string script) =>
        Raising(engine, IO.lift(() => Conversions.Found(engine.Compile(script))));

    public static IO<Option<object>> GetVariable(PythonScript engine, string name) =>
        IO.lift(() => Optional(engine.GetVariable(name)));

    private static IO<T> Raising<T>(PythonScript engine, IO<T> call) =>
        call | @catch(static error => error.IsExceptional, error => IO.fail<T>(new RhinoFault(engine.GetStackTraceFromException(error.ToException()), RhinoReason.Raised, Inner: error)));

    // --- [RHINOCODE]
    public static IO<Unit> RunFile(RhinoDoc doc, InputPath path) =>
        Documents.RunScript(doc, $"""
            _-ScriptEditor _Run "{path}"
            """, echo: false, display: None);
}
