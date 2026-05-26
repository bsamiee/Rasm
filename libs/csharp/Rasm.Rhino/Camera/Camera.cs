namespace Rasm.Rhino.Camera;

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed class RhinoCamera(RhinoDoc document) {
    private static readonly Op ScopeKey = Op.Of(name: nameof(Scope));
    private readonly RhinoDoc document = document ?? throw new ArgumentNullException(paramName: nameof(document));

    public Fin<CameraScope> Scope(ViewportTarget target) =>
        from active in document switch {
            { IsAvailable: true, IsClosing: false, IsInitializing: false, IsOpening: false } active => Fin.Succ(value: active),
            _ => Fin.Fail<RhinoDoc>(error: ScopeKey.MissingContext()),
        }
        from valid in Optional(target).ToFin(Fail: ScopeKey.InvalidInput())
        from scope in valid.Resolve(document: active, op: ScopeKey)
        select scope;

    public Fin<T> In<T>(ViewportTarget target, Func<CameraScope, Fin<T>> use) =>
        from valid in Optional(use).ToFin(Fail: Op.Of(name: nameof(In)).InvalidInput())
        from scope in Scope(target: target)
        from result in valid(arg: scope)
        select result;

    public Fin<T> Run<T>(CameraOp<T> operation, ViewportTarget target) =>
        from valid in Optional(operation).ToFin(Fail: Op.Of(name: nameof(Run)).InvalidInput())
        from result in In(target: target, use: valid.Run)
        select result;

    public static UI.UiIntent<T> Intent<T>(CameraOp<T> operation, ViewportTarget target) =>
        UI.UiIntent.Operation(run: (doc, _) => new RhinoCamera(document: doc).Run(operation: operation, target: target));
}
