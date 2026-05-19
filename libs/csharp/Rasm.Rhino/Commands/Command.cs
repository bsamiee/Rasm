using Result = Rhino.Commands.Result;

namespace Rasm.Rhino.Commands;

// --- [SERVICES] -------------------------------------------------------------------------
public abstract class RasmCommand<TSelf> : Command where TSelf : RasmCommand<TSelf> {
    public override string EnglishName => typeof(TSelf).Name;

    protected sealed override Result RunCommand(RhinoDoc doc, RunMode mode) =>
        RhinoCommandContext.Of(doc: doc, mode: mode)
            .Bind(Run)
            .Match(Succ: static result => result, Fail: static fault => fault is Fault.Cancelled ? Result.Cancel : Result.Failure);

    protected abstract Fin<Result> Run(RhinoCommandContext context);
}
