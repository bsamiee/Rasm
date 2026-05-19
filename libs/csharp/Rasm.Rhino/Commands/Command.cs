using Result = Rhino.Commands.Result;

namespace Rasm.Rhino.Commands;

public abstract class RasmCommand<TSelf> : Command where TSelf : RasmCommand<TSelf> {
    public override string EnglishName => typeof(TSelf).Name;

    protected sealed override Result RunCommand(RhinoDoc doc, RunMode mode) => RhinoCommandContext.Of(doc: doc, mode: mode).Bind(Run).Match(Succ: static result => result, Fail: FailureResult);

    protected abstract Fin<Result> Run(RhinoCommandContext context);

    protected virtual Result FailureResult(Error fault) => fault is Fault.Cancelled ? Result.Cancel : Result.Failure;
}
