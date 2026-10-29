using Rasm.Rhino.Document.Files;
using Rhino;
using Rhino.NodeInCode;
using Rhino.Runtime;

namespace Rasm.Rhino.Commands;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Scripts {
    // --- [PYTHON]
    public static IO<PythonScript> Engine(RhinoDoc doc, HashMap<string, object?> variables) =>
        from engine in Products.Of(static () => PythonScript.Create())
        from configured in IO.lift(() => engine.SetupScriptContext(doc))
        from bound in IO.lift(() => variables.Iter(engine.SetVariable))
        select engine;

    public static IO<T> Run<T>(PythonScript engine, IO<T> call) =>
        call | @catch(static error => error.IsExceptional, error => IO.fail<T>(new RhinoFault(engine.GetStackTraceFromException(error.ToException()), RhinoReason.Raised, Inner: error)));

    // --- [RHINOCODE]
    public static IO<Unit> RunFile(RhinoDoc doc, InputPath path) =>
        Documents.RunScript(doc, $"""
            _-ScriptEditor _Run "{path}"
            """, echo: false, display: None);

    // --- [NODES]
    public static IO<(HashMap<string, Option<object>> Outputs, Seq<string> Warnings)> Evaluate(ComponentFunctionInfo node, Seq<Option<object>> inputs, bool keepTree) =>
        IO.lift(() => Conversions.Required(node.Evaluate(inputs.Map(static input => input.ValueUnsafe()), keepTree, out string[] warnings)).Map(values => (
            toHashMap(toSeq(node.OutputNames).Zip(toSeq(values), static (name, value) => (name, Optional(value)))),
            Conversions.Rows(warnings))));
}
