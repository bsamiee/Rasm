using Result = Rhino.Commands.Result;

namespace Rasm.Rhino.Commands;

public abstract class RasmCommand<TSelf> : Command where TSelf : RasmCommand<TSelf> {
    public override string EnglishName => typeof(TSelf).Name;

    protected sealed override Result RunCommand(RhinoDoc doc, RunMode mode) =>
        RhinoCommandContext.Of(doc: doc, mode: mode)
            .Bind(context =>
                from _ in Ready(context: context)
                from result in Run(context: context)
                select result)
            .Match(Succ: static result => result, Fail: FailureResult);

    protected abstract Fin<Result> Run(RhinoCommandContext context);

    protected virtual Fin<Unit> Ready(RhinoCommandContext context) =>
        from active in Optional(context).ToFin(Fail: Op.Of(name: nameof(Ready)).InvalidInput())
        from _ in active.Scope.Context.Map(static _ => unit)
        select unit;

    protected virtual Result FailureResult(Error fault) => fault is Fault.Cancelled ? Result.Cancel : Result.Failure;
}
