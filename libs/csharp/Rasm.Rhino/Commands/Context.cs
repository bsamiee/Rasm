namespace Rasm.Rhino.Commands;

public sealed class RhinoCommandContext {
    private RhinoCommandContext(RhinoDoc document, RunMode mode, Rasm.Domain.Context domain) {
        Document = document;
        Mode = mode;
        Domain = domain;
        Scope = Analyze.In(context: domain);
        Input = new(document: document, domain: domain);
        Edit = new(document: document, domain: domain);
        Ui = new(document: document, mode: mode);
    }

    public RhinoDoc Document { get; }
    public RunMode Mode { get; }
    public Rasm.Domain.Context Domain { get; }
    public Analyze.Scope Scope { get; }
    public CommandInput Input { get; }
    public DocumentEdit Edit { get; }
    public Rasm.Rhino.UI.RhinoUi Ui { get; }

    public static Fin<RhinoCommandContext> Of(RhinoDoc doc, RunMode mode) =>
        Optional(doc)
            .ToFin(Fail: Op.Of(name: nameof(RhinoCommandContext)).MissingContext())
            .Bind(document => document switch {
                { IsAvailable: true, IsClosing: false, IsInitializing: false, IsOpening: false } =>
                    Rasm.Domain.Context.Of(doc: document).ToFin().Map(domain => new RhinoCommandContext(document: document, mode: mode, domain: domain)),
                _ => Fin.Fail<RhinoCommandContext>(error: Op.Of(name: nameof(RhinoCommandContext)).MissingContext()),
            });

    public Fin<CommandGet<TValue>> Get<TValue>(params CommandInputPolicy[] policies) => Input.Get(request: CommandInputs.Get<TValue>(policies: policies));

    public Fin<RhinoCommandContext> Require(RunMode mode, string? name = null) =>
        (Mode == mode) switch {
            true => Fin.Succ(value: this),
            _ => Fin.Fail<RhinoCommandContext>(error: Op.Of(name: string.IsNullOrWhiteSpace(value: name) ? nameof(Require) : name).InvalidInput()),
        };

    public Fin<RhinoDoc> Active() =>
        Document switch {
            { IsAvailable: true, IsClosing: false, IsInitializing: false, IsOpening: false } document => Fin.Succ(value: document),
            _ => Fin.Fail<RhinoDoc>(error: Op.Of(name: nameof(Active)).MissingContext()),
        };

    public Fin<RhinoView> View() =>
        Active().Bind(document => Optional(document.Views.ActiveView).ToFin(Fail: Op.Of(name: nameof(View)).MissingContext()));

    public Fin<RhinoView> View(Guid viewportId) =>
        Viewport(viewportId: viewportId).Map(static hit => hit.View);

    public Fin<(RhinoView View, RhinoViewport Viewport)> Viewport(Guid viewportId) =>
        from document in Active() from id in viewportId switch { Guid value when value != Guid.Empty => Fin.Succ(value: value), _ => Fin.Fail<Guid>(error: Op.Of(name: nameof(Viewport)).InvalidInput()) }
        from view in (Optional(document.Views.Find(mainViewportId: id)) |
                      toSeq(document.Views.GetViewList(filter: ViewTypeFilter.All)).Find(candidate => candidate.MainViewport.Id == id || candidate.ActiveViewportID == id || (candidate is RhinoPageView page && (page.ActiveDetailId == id || toSeq(page.GetDetailViews()).Exists(detail => detail.Id == id || detail.Viewport.Id == id)))))
            .ToFin(Fail: Op.Of(name: nameof(Viewport)).MissingContext())
        from viewport in (view switch {
            RhinoView native when native.MainViewport.Id == id => Some(native.MainViewport),
            RhinoPageView page when page.ActiveDetailId == id => Optional(page.ActiveViewport),
            RhinoPageView page => toSeq(page.GetDetailViews()).Find(detail => detail.Id == id || detail.Viewport.Id == id).Map(static detail => detail.Viewport) | Optional(page.ActiveViewport),
            RhinoView native when native.ActiveViewportID == id => Some(native.ActiveViewport),
            _ => Option<RhinoViewport>.None,
        }).ToFin(Fail: Op.Of(name: nameof(Viewport)).MissingContext())
        select (View: view, Viewport: viewport);

    public Fin<T> InViewport<T>(Option<Guid> viewportId, Func<RhinoView, RhinoViewport, Fin<T>> use) =>
        from valid in Optional(use).ToFin(Fail: Op.Of(name: nameof(InViewport)).InvalidInput())
        from target in viewportId.Case switch {
            Guid id => Viewport(viewportId: id),
            _ => View().Map(static view => (View: view, Viewport: view.ActiveViewport)),
        }
        from result in valid(arg1: target.View, arg2: target.Viewport)
        select result;

    public Fin<T> UseInViewport<T>(Option<Guid> viewportId, Func<RhinoView, RhinoViewport, Rasm.Rhino.UI.UiIntent<T>> intent) =>
        from project in Optional(intent).ToFin(Fail: Op.Of(name: nameof(UseInViewport)).InvalidInput())
        from result in InViewport(viewportId: viewportId, use: (view, viewport) => Ui.Use(intent: project(arg1: view, arg2: viewport)))
        select result;
}
