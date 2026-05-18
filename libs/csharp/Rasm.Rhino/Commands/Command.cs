using Result = Rhino.Commands.Result;

namespace Rasm.Rhino.Commands;

// --- [SERVICES] -------------------------------------------------------------------------
public abstract class RasmCommand<TSelf> : Command where TSelf : RasmCommand<TSelf> {
    public override string EnglishName => typeof(TSelf).Name;

    protected sealed override Result RunCommand(RhinoDoc doc, RunMode mode) =>
        Optional(doc)
            .ToFin(Fail: Op.Of(name: EnglishName).MissingContext())
            .Bind(document => Run(context: RhinoCommandContext.Of(doc: document, mode: mode)))
            .Match(Succ: static result => result, Fail: static fault => fault is Fault.Cancelled ? Result.Cancel : Result.Failure);

    protected abstract Fin<Result> Run(RhinoCommandContext context);
}
