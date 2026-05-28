using Rhino;
using GhCanvas = Grasshopper2.UI.Canvas.Canvas;
using GhEditor = Grasshopper2.UI.Editor;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[GenerateUnionOps]
[Union]
public partial record EditorOp {
    private EditorOp() { }
    public sealed partial record ShowCase(bool Visible, Option<string> Layout) : EditorOp;
    public sealed partial record StateCase : EditorOp;
    public sealed partial record EnsureVisibleCase : EditorOp;
    public sealed partial record ShellCase(Option<bool> Collapsed, Option<bool> ShowNotes, Option<bool> ShowUndoHistory, Option<string> Layout) : EditorOp;
    public sealed partial record BeginRhinoGetterCase(RhinoDoc Document) : EditorOp;

    public static EditorOp Show(bool visible = true, string? layout = null) => new ShowCase(Visible: visible, Layout: Optional(layout));
    public static readonly EditorOp State = new StateCase();
    public static readonly EditorOp EnsureVisible = new EnsureVisibleCase();
    public static EditorOp Shell(Option<bool> collapsed = default, Option<bool> showNotes = default, Option<bool> showUndoHistory = default, Option<string> layout = default) =>
        new ShellCase(Collapsed: collapsed, ShowNotes: showNotes, ShowUndoHistory: showUndoHistory, Layout: layout);
    public static EditorOp BeginRhinoGetter(RhinoDoc document) => new BeginRhinoGetterCase(Document: document);

    internal GrasshopperUiPolicy UiPolicy => Switch(
        showCase: static s => GrasshopperUiPolicy.Canvas(openEditor: s.Visible),
        stateCase: static _ => GrasshopperUiPolicy.Read,
        ensureVisibleCase: static _ => GrasshopperUiPolicy.Canvas(openEditor: true),
        shellCase: static _ => GrasshopperUiPolicy.Canvas(openEditor: true),
        beginRhinoGetterCase: static _ => GrasshopperUiPolicy.Read);
}

[SkipUnionOps]
[Union]
public partial record EditorResult {
    private EditorResult() { }
    public sealed record UnitResult : EditorResult;
    public sealed record StateResult(EditorSnapshot Snapshot) : EditorResult;
    public sealed record ShellResult(Snapshot<EditorShellSnapshot> Snapshot) : EditorResult;
    public static readonly EditorResult Unit = new UnitResult();
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct EditorSnapshot(bool HasEditor = false, bool HasCanvas = false, bool HasDocument = false, bool Collapsed = false, bool HasStatusBar = false, Option<Guid> StatusBarDocumentHash = default, bool ShowNotes = false, bool ShowUndoHistory = false, string InitialLayout = "", Seq<string> DefinedLayouts = default, Option<string> MostRecentActiveDocument = default, Seq<string> MostRecentLoadedDocuments = default, int MostRecentCount = 0);

[StructLayout(LayoutKind.Auto)]
public readonly record struct EditorShellSnapshot(bool Collapsed, bool ShowNotes, bool ShowUndoHistory, string InitialLayout, Seq<string> DefinedLayouts);

// --- [SERVICES] ---------------------------------------------------------------------------
internal static partial class Editor {
    internal static Fin<EditorResult> Dispatch(EditorOp op) => op.Switch(
        showCase: static show => ShowEditor(visible: show.Visible, layoutRules: show.Layout.IfNone(string.Empty), errorTag: nameof(EditorOp.Show)),
        stateCase: static _ => DispatchState(),
        ensureVisibleCase: static _ => ShowEditor(visible: true, layoutRules: string.Empty, errorTag: nameof(EditorOp.EnsureVisible)),
        shellCase: static shell => ShowEditor(visible: true, layoutRules: shell.Layout.IfNone(string.Empty), errorTag: nameof(EditorOp.Shell), shell: Some(shell)),
        beginRhinoGetterCase: static getter => DispatchRhinoGetter(getter: getter));

    // Show/EnsureVisible/Shell share GhEditor.ShowEditor; Shell projects EditorShellSnapshot after optional chrome writes.
    private static Fin<EditorResult> ShowEditor(
        bool visible,
        string layoutRules,
        string errorTag,
        Option<EditorOp.ShellCase> shell = default) =>
        Try.lift(f: () => GhEditor.ShowEditor(createVisible: visible, layoutRules: layoutRules)).Run()
            .MapFail(error => UiFault.GhEditor(detail: $"{errorTag}: {error.Message}"))
            .Bind(current => shell.Match(
                Some: s => ApplyShell(current: current, shell: s),
                None: () => Fin.Succ(value: EditorResult.Unit)));

    private static Fin<EditorResult> ApplyShell(GhEditor current, EditorOp.ShellCase shell) {
        Option<GhCanvas> canvas = Optional(current.Canvas);
        bool requiresHistory = shell.ShowUndoHistory.IfNone(false);
        return (requiresHistory, canvas.IsSome) switch {
            (true, false) => Fin.Fail<EditorResult>(error: UiFault.GhEditor(detail: "ShowUndoHistory requires an active canvas")),
            _ => Fin.Succ<EditorResult>(ShellResultOf(current: current, shell: shell, canvas: canvas)),
        };
    }

    private static EditorResult.ShellResult ShellResultOf(GhEditor current, EditorOp.ShellCase shell, Option<GhCanvas> canvas) {
        _ = shell.Collapsed.Iter(value => current.Collapsed = value);
        _ = shell.ShowNotes.Iter(value => current.ShowNotes = value);
        _ = canvas.Iter(c => shell.ShowUndoHistory.Iter(value => c.ShowUndoHistory = value));
        return new EditorResult.ShellResult(Snapshot: Snapshot.Of(new EditorShellSnapshot(
            Collapsed: current.Collapsed,
            ShowNotes: current.ShowNotes,
            ShowUndoHistory: canvas.Map(static c => c.ShowUndoHistory).IfNone(false),
            InitialLayout: GhEditor.InitialLayout,
            DefinedLayouts: toSeq(GhEditor.DefinedLayouts))));
    }

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
                StatusBarDocumentHash: Optional(e.StatusBar?.Document).Map(static doc => doc.Hash),
                ShowNotes: e.ShowNotes,
                ShowUndoHistory: e.Canvas?.ShowUndoHistory ?? false,
                InitialLayout: layout,
                DefinedLayouts: definedLayouts,
                MostRecentActiveDocument: Optional(e.MostRecentActiveDocument),
                MostRecentLoadedDocuments: toSeq(e.MostRecentLoadedDocuments),
                MostRecentCount: e.MostRecentCount),
            None: () => new EditorSnapshot(InitialLayout: layout, DefinedLayouts: definedLayouts));
    }
    private static Fin<EditorResult> DispatchRhinoGetter(EditorOp.BeginRhinoGetterCase getter) =>
        from started in Try.lift(f: () => GhEditor.BeginRhinoGetter(doc: getter.Document)).Run().MapFail(static error => UiFault.GhEditor(detail: $"{nameof(EditorOp.BeginRhinoGetter)}: {error.Message}"))
        from valid in started
            ? Fin.Succ(value: EditorResult.Unit)
            : Fin.Fail<EditorResult>(error: UiFault.GhEditor(detail: "Rhino getter is already active or no document can receive it"))
        select valid;
}
