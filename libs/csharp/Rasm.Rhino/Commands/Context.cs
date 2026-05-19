namespace Rasm.Rhino.Commands;

public sealed class RhinoCommandContext {
    private RhinoCommandContext(RhinoDoc document, RunMode mode, Rasm.Domain.Context domain) {
        Document = document;
        Mode = mode;
        Domain = domain;
        Scope = Analyze.From(doc: document);
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

    public Fin<RhinoCommandContext> Interactive() =>
        Mode switch {
            RunMode.Interactive => Fin.Succ(value: this),
            _ => Fin.Fail<RhinoCommandContext>(error: Op.Of(name: nameof(Interactive)).InvalidInput()),
        };

    public Fin<RhinoCommandContext> Scripted() =>
        Mode switch {
            RunMode.Scripted => Fin.Succ(value: this),
            _ => Fin.Fail<RhinoCommandContext>(error: Op.Of(name: nameof(Scripted)).InvalidInput()),
        };

    public Fin<RhinoDoc> Active() =>
        Document switch {
            { IsAvailable: true, IsClosing: false, IsInitializing: false, IsOpening: false } document => Fin.Succ(value: document),
            _ => Fin.Fail<RhinoDoc>(error: Op.Of(name: nameof(Active)).MissingContext()),
        };

    public Fin<RhinoView> View() =>
        Active().Bind(document => Optional(document.Views.ActiveView).ToFin(Fail: Op.Of(name: nameof(View)).MissingContext()));

    public Fin<RhinoView> View(Guid mainViewportId) =>
        from document in Active()
        from id in mainViewportId switch {
            Guid value when value != Guid.Empty => Fin.Succ(value: value),
            _ => Fin.Fail<Guid>(error: Op.Of(name: nameof(View)).InvalidInput()),
        }
        from view in (Optional(document.Views.Find(mainViewportId: id)) |
                      toSeq(document.Views.GetViewList(filter: ViewTypeFilter.All))
                          .Find(candidate =>
                              candidate.MainViewport.Id == id ||
                              candidate.ActiveViewportID == id ||
                              (candidate is RhinoPageView page && (page.ActiveDetailId == id || toSeq(page.GetDetailViews()).Exists(detail => detail.Id == id || detail.Viewport.Id == id)))))
            .ToFin(Fail: Op.Of(name: nameof(View)).MissingContext())
        select view;
}
