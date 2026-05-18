using Result = Rhino.Commands.Result;

namespace Rasm.Rhino;

// --- [MODELS] ---------------------------------------------------------------------------
public readonly record struct CommandOutcome(Result Result) {
    public static CommandOutcome Success => From(result: Result.Success);
    public static CommandOutcome Cancel => From(result: Result.Cancel);
    public static CommandOutcome Nothing => From(result: Result.Nothing);
    public static CommandOutcome ExitRhino => From(result: Result.ExitRhino);
    public static CommandOutcome From(Result result) => new(Result: result);
}

// --- [SERVICES] -------------------------------------------------------------------------
public abstract class RasmCommand<TSelf> : Command where TSelf : RasmCommand<TSelf> {
    public override string EnglishName => typeof(TSelf).Name;

    protected sealed override Result RunCommand(RhinoDoc doc, RunMode mode) =>
        Optional(doc)
            .ToFin(Fail: Op.Of(name: EnglishName).MissingContext())
            .Bind(document => Run(context: RhinoCommandContext.Of(doc: document, mode: mode)))
            .Map(static outcome => outcome.Result)
            .Match(Succ: static result => result, Fail: static fault => fault is Fault.Cancelled ? Result.Cancel : Result.Failure);

    protected abstract Fin<CommandOutcome> Run(RhinoCommandContext context);
}
