namespace Rasm.Rhino.Camera;

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed class RhinoCamera(RhinoDoc document) {
    private readonly RhinoDoc document = document ?? throw new ArgumentNullException(paramName: nameof(document));

    public Fin<RhinoDoc> Active() =>
        document switch {
            { IsAvailable: true, IsClosing: false, IsInitializing: false, IsOpening: false } active => Fin.Succ(value: active),
            _ => Fin.Fail<RhinoDoc>(error: Op.Of(name: nameof(Active)).MissingContext()),
        };

    public Fin<CameraScope> Scope(Option<Guid> viewportId = default) =>
        from active in Active()
        from scope in viewportId.Case switch {
            Guid id when id != Guid.Empty => Pinpoint(active: active, viewportId: id),
            _ => from view in Optional(active.Views.ActiveView).ToFin(Fail: Op.Of(name: nameof(Scope)).MissingContext())
                 select new CameraScope(Document: active, View: view, Viewport: view.ActiveViewport,
                     Detail: view is RhinoPageView page ? Optional(page.ActiveDetail) : Option<DetailViewObject>.None),
        }
        select scope;

    public Fin<CameraScope> Scope(Guid viewportId) =>
        Scope(viewportId: viewportId == Guid.Empty ? Option<Guid>.None : Some(viewportId));

    public Fin<CameraScope> Scope(RhinoView view) =>
        from active in Active()
        from valid in Optional(view).ToFin(Fail: Op.Of(name: nameof(Scope)).InvalidInput())
        select new CameraScope(Document: active, View: valid, Viewport: valid.ActiveViewport,
            Detail: valid is RhinoPageView page ? Optional(page.ActiveDetail) : Option<DetailViewObject>.None);

    public Fin<T> In<T>(Option<Guid> viewportId, Func<CameraScope, Fin<T>> use) =>
        from valid in Optional(use).ToFin(Fail: Op.Of(name: nameof(In)).InvalidInput())
        from scope in Scope(viewportId: viewportId)
        from result in valid(arg: scope)
        select result;

    public Fin<T> Run<T>(CameraOp<T> operation, Option<Guid> viewportId = default) =>
        from valid in Optional(operation).ToFin(Fail: Op.Of(name: nameof(Run)).InvalidInput())
        from result in In(viewportId: viewportId, use: valid.Apply)
        select result;

    public static UI.UiIntent<T> Intent<T>(CameraOp<T> operation, Option<Guid> viewportId = default) =>
        UI.UiIntent.Operation(run: (doc, _) => new RhinoCamera(document: doc).Run(operation: operation, viewportId: viewportId));

    private static Fin<CameraScope> Pinpoint(RhinoDoc active, Guid viewportId) {
        Op op = Op.Of(name: nameof(Scope));
        return from viewport in Optional(RhinoViewport.FromId(id: viewportId)).ToFin(Fail: op.MissingContext())
               from view in Optional(viewport.ParentView).ToFin(Fail: op.MissingContext())
               select new CameraScope(
                   Document: active, View: view, Viewport: viewport,
                   Detail: view is RhinoPageView page
                       ? toSeq(page.GetDetailViews()).Find(detail => detail.Viewport.Id == viewportId || detail.Id == viewportId)
                       : Option<DetailViewObject>.None);
    }
}
