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

    private static GrasshopperUiPolicy PolicyOf(EditorOp op) =>
        op switch {
            EditorOp.StateCase or EditorOp.BeginRhinoGetterCase => GrasshopperUiPolicy.Read,
            EditorOp.ShowCase show => GrasshopperUiPolicy.Canvas(openEditor: show.Visible),
            EditorOp.EnsureVisibleCase => GrasshopperUiPolicy.Canvas(openEditor: true),
            EditorOp.ShellCase => GrasshopperUiPolicy.Canvas(openEditor: true),
            _ => GrasshopperUiPolicy.Read,
        };

    private static Fin<EditorResult> Dispatch(EditorOp op) => op.Switch(
        showCase: static show => Try.lift(f: () => {
            _ = GhEditor.ShowEditor(createVisible: show.Visible, layoutRules: show.Layout.IfNone(string.Empty));
            return EditorResult.Unit;
        }).Run().MapFail(error => UiFault.GhEditor(detail: $"{nameof(EditorOp.Show)}: {error.Message}")),
        stateCase: static state => DispatchState(),
        ensureVisibleCase: static ensure => Try.lift(f: () => {
            _ = GhEditor.ShowEditor(createVisible: true, layoutRules: string.Empty);
            return EditorResult.Unit;
        }).Run().MapFail(static error => UiFault.GhEditor(detail: $"{nameof(EditorOp.EnsureVisible)}: {error.Message}")),
        shellCase: DispatchShell,
        beginRhinoGetterCase: DispatchRhinoGetter);

    private static Fin<EditorResult> DispatchState() =>
        Fin.Succ<EditorResult>(value: new EditorResult.StateResult(Snapshot: GhEditor.Instance switch {
            GhEditor editor => new EditorSnapshot(
                HasEditor: true,
                HasCanvas: editor.Canvas is not null,
                HasDocument: (editor.Documents.Current ?? editor.Canvas?.Document) is not null,
                Collapsed: editor.Collapsed,
                HasStatusBar: editor.StatusBar is not null,
                ShowNotes: editor.ShowNotes,
                ShowUndoHistory: editor.Canvas?.ShowUndoHistory ?? false,
                InitialLayout: GhEditor.InitialLayout,
                DefinedLayouts: toSeq(GhEditor.DefinedLayouts),
                MostRecentActiveDocument: Optional(editor.MostRecentActiveDocument),
                MostRecentLoadedDocuments: toSeq(editor.MostRecentLoadedDocuments),
                MostRecentCount: editor.MostRecentCount),
            _ => new EditorSnapshot(
                HasEditor: false,
                HasCanvas: false,
                HasDocument: false,
                Collapsed: false,
                HasStatusBar: false,
                ShowNotes: false,
                ShowUndoHistory: false,
                InitialLayout: GhEditor.InitialLayout,
                DefinedLayouts: toSeq(GhEditor.DefinedLayouts),
                MostRecentActiveDocument: Option<string>.None,
                MostRecentLoadedDocuments: Seq<string>(),
                MostRecentCount: 0),
        }));

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
