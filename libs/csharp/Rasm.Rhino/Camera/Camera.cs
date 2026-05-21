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
        viewportId.Case switch {
            Guid id => Scope(viewportId: id),
            _ => Active().Bind(active => Optional(active.Views.ActiveView)
                .ToFin(Fail: Op.Of(name: nameof(Scope)).MissingContext())
                .Bind(view => Scope(document: active, view: view, viewport: view.ActiveViewport, detail: ActiveDetail(view: view)))),
        };

    public Fin<CameraScope> Scope(Guid viewportId) =>
        from active in Active()
        from id in viewportId switch {
            Guid value when value != Guid.Empty => Fin.Succ(value: value),
            _ => Fin.Fail<Guid>(error: Op.Of(name: nameof(Scope)).InvalidInput()),
        }
        from resolved in Resolve(document: active, viewportId: id)
        select resolved;

    public Fin<CameraScope> Scope(RhinoView view) =>
        from active in Active()
        from validView in Optional(view).ToFin(Fail: Op.Of(name: nameof(Scope)).InvalidInput())
        from scope in Scope(document: active, view: validView, viewport: validView.ActiveViewport, detail: ActiveDetail(view: validView))
        select scope;

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

    internal static Fin<Unit> UnitResult(bool success, Op op) =>
        success switch {
            true => Fin.Succ(value: unit),
            false => Fin.Fail<Unit>(error: op.InvalidResult()),
        };

    private Fin<CameraScope> Resolve(RhinoDoc document, Guid viewportId) =>
        from view in (Optional(document.Views.Find(mainViewportId: viewportId)) |
                      toSeq(document.Views.GetViewList(filter: ViewTypeFilter.All)).Find(candidate =>
                          candidate.MainViewport.Id == viewportId ||
                          candidate.ActiveViewportID == viewportId ||
                          (candidate is RhinoPageView page && (page.ActiveDetailId == viewportId || toSeq(page.GetDetailViews()).Exists(detail => detail.Id == viewportId || detail.Viewport.Id == viewportId)))))
            .ToFin(Fail: Op.Of(name: nameof(Resolve)).MissingContext())
        from target in Resolve(view: view, viewportId: viewportId)
        from scope in Scope(document: document, view: view, viewport: target.Viewport, detail: target.Detail)
        select scope;

    private static Fin<CameraScope> Scope(RhinoDoc document, RhinoView view, RhinoViewport viewport, Option<DetailViewObject> detail) =>
        from validDocument in Optional(document).ToFin(Fail: Op.Of(name: nameof(Scope)).InvalidInput())
        from validView in Optional(view).ToFin(Fail: Op.Of(name: nameof(Scope)).InvalidInput())
        from validViewport in Optional(viewport).ToFin(Fail: Op.Of(name: nameof(Scope)).InvalidInput())
        select new CameraScope(Document: validDocument, View: validView, Viewport: validViewport, Detail: detail);

    private static Fin<(RhinoViewport Viewport, Option<DetailViewObject> Detail)> Resolve(RhinoView view, Guid viewportId) =>
        (view switch {
            RhinoView native when native.MainViewport.Id == viewportId => Some((native.MainViewport, Option<DetailViewObject>.None)),
            RhinoPageView page when page.ActiveDetailId == viewportId => Optional(page.ActiveDetail).Map<(RhinoViewport Viewport, Option<DetailViewObject> Detail)>(detail => (detail.Viewport, Some(detail))) | Optional(page.ActiveViewport).Map<(RhinoViewport Viewport, Option<DetailViewObject> Detail)>(static viewport => (viewport, Option<DetailViewObject>.None)),
            RhinoPageView page => toSeq(page.GetDetailViews()).Find(detail => detail.Id == viewportId || detail.Viewport.Id == viewportId).Map<(RhinoViewport Viewport, Option<DetailViewObject> Detail)>(detail => (detail.Viewport, Some(detail))) | Optional(page.ActiveViewport).Map<(RhinoViewport Viewport, Option<DetailViewObject> Detail)>(static viewport => (viewport, Option<DetailViewObject>.None)),
            RhinoView native when native.ActiveViewportID == viewportId => Some((native.ActiveViewport, Option<DetailViewObject>.None)),
            _ => Option<(RhinoViewport Viewport, Option<DetailViewObject> Detail)>.None,
        }).ToFin(Fail: Op.Of(name: nameof(Resolve)).MissingContext());

    private static Option<DetailViewObject> ActiveDetail(RhinoView view) =>
        view switch {
            RhinoPageView page => Optional(page.ActiveDetail),
            _ => Option<DetailViewObject>.None,
        };
}
