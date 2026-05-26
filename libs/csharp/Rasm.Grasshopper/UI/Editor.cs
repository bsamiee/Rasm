using Rhino;
using GhEditor = Grasshopper2.UI.Editor;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[Union]
public partial record EditorOp {
    private EditorOp() { }
    public sealed record ShowCase(bool Visible, Option<string> Layout) : EditorOp;
    public sealed record StateCase : EditorOp;
    public sealed record EnsureVisibleCase : EditorOp;
    public sealed record ShellCase(Option<bool> Collapsed, Option<bool> ShowNotes, Option<bool> ShowUndoHistory, Option<string> Layout) : EditorOp;
    public sealed record BeginRhinoGetterCase(Option<RhinoDoc> Document) : EditorOp;

    public static EditorOp Show(bool visible = true, string? layout = null) => new ShowCase(Visible: visible, Layout: Optional(layout));
    public static readonly EditorOp State = new StateCase();
    public static readonly EditorOp EnsureVisible = new EnsureVisibleCase();
    public static EditorOp Shell(Option<bool> collapsed = default, Option<bool> showNotes = default, Option<bool> showUndoHistory = default, Option<string> layout = default) =>
        new ShellCase(Collapsed: collapsed, ShowNotes: showNotes, ShowUndoHistory: showUndoHistory, Layout: layout);
    public static EditorOp BeginRhinoGetter(RhinoDoc? document = null) => new BeginRhinoGetterCase(Document: Optional(document));
}

[Union]
public partial record EditorResult {
    private EditorResult() { }
    public sealed record UnitResult : EditorResult;
    public sealed record StateResult(EditorSnapshot Snapshot) : EditorResult;
    public sealed record ShellResult(Snapshot<EditorShellSnapshot> Snapshot) : EditorResult;
    public static readonly EditorResult Unit = new UnitResult();
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct EditorSnapshot(
    bool HasEditor,
    bool HasCanvas,
    bool HasDocument,
    bool Collapsed,
    bool HasStatusBar,
    bool ShowNotes,
    bool ShowUndoHistory,
    string InitialLayout,
    Seq<string> DefinedLayouts,
    Option<string> MostRecentActiveDocument,
    Seq<string> MostRecentLoadedDocuments,
    int MostRecentCount);

[StructLayout(LayoutKind.Auto)]
public readonly record struct EditorShellSnapshot(
    bool Collapsed,
    bool ShowNotes,
    bool ShowUndoHistory,
    string InitialLayout,
    Seq<string> DefinedLayouts);

internal sealed record EditorRequest(EditorOp Op) : GhUiRequest<EditorResult> {
    internal override GrasshopperUiPolicy Policy => PolicyOf(op: Op);
    internal override Fin<EditorResult> Apply(GrasshopperUi.Scope scope) => Dispatch(op: Op);

    private static GrasshopperUiPolicy PolicyOf(EditorOp op) => op.Switch(
        showCase: static show => GrasshopperUiPolicy.Canvas(openEditor: show.Visible),
        stateCase: static _ => GrasshopperUiPolicy.Read,
        ensureVisibleCase: static _ => GrasshopperUiPolicy.Canvas(openEditor: true),
        shellCase: static _ => GrasshopperUiPolicy.Canvas(openEditor: true),
        beginRhinoGetterCase: static _ => GrasshopperUiPolicy.Read);

    private static Fin<EditorResult> Dispatch(EditorOp op) => op.Switch(
        showCase: static show => ShowEditor(visible: show.Visible, layoutRules: show.Layout.IfNone(string.Empty), errorTag: nameof(EditorOp.Show)),
        stateCase: static _ => DispatchState(),
        ensureVisibleCase: static _ => ShowEditor(visible: true, layoutRules: string.Empty, errorTag: nameof(EditorOp.EnsureVisible)),
        shellCase: DispatchShell,
        beginRhinoGetterCase: DispatchRhinoGetter);

    // Show/EnsureVisible share the Try.lift(GhEditor.ShowEditor) capsule with only visibility +
    // layoutRules differing — one helper subsumes both arms; `errorTag` preserves arm provenance.
    private static Fin<EditorResult> ShowEditor(bool visible, string layoutRules, string errorTag) =>
        Try.lift(f: () => {
            _ = GhEditor.ShowEditor(createVisible: visible, layoutRules: layoutRules);
            return EditorResult.Unit;
        }).Run().MapFail(error => UiFault.GhEditor(detail: $"{errorTag}: {error.Message}"));

    // Optional(GhEditor.Instance) discharges the "running" vs "not yet shown" branches polymorphically;
    // shared InitialLayout + DefinedLayouts hoisted once instead of duplicated across arms.
    private static Fin<EditorResult> DispatchState() =>
        Fin.Succ<EditorResult>(value: new EditorResult.StateResult(Snapshot: SnapshotEditor(editor: Optional(GhEditor.Instance))));

    private static EditorSnapshot SnapshotEditor(Option<GhEditor> editor) {
        string layout = GhEditor.InitialLayout;
        Seq<string> definedLayouts = toSeq(GhEditor.DefinedLayouts);
        return editor.Match(
            Some: e => new EditorSnapshot(
                HasEditor: true,
                HasCanvas: e.Canvas is not null,
                HasDocument: (e.Documents.Current ?? e.Canvas?.Document) is not null,
                Collapsed: e.Collapsed,
                HasStatusBar: e.StatusBar is not null,
                ShowNotes: e.ShowNotes,
                ShowUndoHistory: e.Canvas?.ShowUndoHistory ?? false,
                InitialLayout: layout,
                DefinedLayouts: definedLayouts,
                MostRecentActiveDocument: Optional(e.MostRecentActiveDocument),
                MostRecentLoadedDocuments: toSeq(e.MostRecentLoadedDocuments),
                MostRecentCount: e.MostRecentCount),
            None: () => new EditorSnapshot(
                HasEditor: false,
                HasCanvas: false,
                HasDocument: false,
                Collapsed: false,
                HasStatusBar: false,
                ShowNotes: false,
                ShowUndoHistory: false,
                InitialLayout: layout,
                DefinedLayouts: definedLayouts,
                MostRecentActiveDocument: Option<string>.None,
                MostRecentLoadedDocuments: Seq<string>(),
                MostRecentCount: 0));
    }

    private static Fin<EditorResult> DispatchShell(EditorOp.ShellCase shell) =>
        Try.lift(f: () => {
            GhEditor current = GhEditor.ShowEditor(createVisible: true, layoutRules: shell.Layout.IfNone(string.Empty));
            _ = shell.Collapsed.Iter(value => current.Collapsed = value);
            _ = shell.ShowNotes.Iter(value => current.ShowNotes = value);
            _ = shell.ShowUndoHistory.Iter(value => current.Canvas.ShowUndoHistory = value);
            return Snapshot.Of(new EditorShellSnapshot(
                Collapsed: current.Collapsed,
                ShowNotes: current.ShowNotes,
                ShowUndoHistory: current.Canvas.ShowUndoHistory,
                InitialLayout: GhEditor.InitialLayout,
                DefinedLayouts: toSeq(GhEditor.DefinedLayouts)));
        }).Run().MapFail(error => UiFault.GhEditor(detail: $"{nameof(EditorOp.Shell)}: {error.Message}"))
        .Map(static snapshot => (EditorResult)new EditorResult.ShellResult(Snapshot: snapshot));

    private static Fin<EditorResult> DispatchRhinoGetter(EditorOp.BeginRhinoGetterCase getter) =>
        from active in Optional(getter.Document.IfNone(RhinoDoc.ActiveDoc)).ToFin(Fail: UiFault.MissingScope(field: "rhino-doc"))
        from started in Try.lift(f: () => GhEditor.BeginRhinoGetter(doc: active)).Run().MapFail(static error => UiFault.GhEditor(detail: $"{nameof(EditorOp.BeginRhinoGetter)}: {error.Message}"))
        from valid in started
            ? Fin.Succ(value: EditorResult.Unit)
            : Fin.Fail<EditorResult>(error: UiFault.GhEditor(detail: "Rhino getter is already active or no document can receive it"))
        select valid;
}
