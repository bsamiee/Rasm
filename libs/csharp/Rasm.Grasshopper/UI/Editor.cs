using Rhino;
using GhCanvas = Grasshopper2.UI.Canvas.Canvas;
using GhEditor = Grasshopper2.UI.Editor;
using Op = Rasm.Domain.Op;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[GenerateUnionOps]
[Union]
public partial record EditorOp : IUiOp<EditorResult> {
    private EditorOp() { }
    public sealed partial record ShowCase(bool Visible, Option<string> Layout) : EditorOp;
    public sealed partial record StateCase : EditorOp;
    public sealed partial record ShellCase(Option<bool> Collapsed, Option<bool> ShowNotes, Option<bool> ShowUndoHistory, Option<string> Layout) : EditorOp;
    public sealed partial record BeginRhinoGetterCase(RhinoDoc Document) : EditorOp;

    public static EditorOp Show(bool visible = true, string? layout = null) => new ShowCase(Visible: visible, Layout: Optional(layout));
    public static readonly EditorOp State = new StateCase();
    public static EditorOp Shell(Option<bool> collapsed = default, Option<bool> showNotes = default, Option<bool> showUndoHistory = default, Option<string> layout = default) =>
        new ShellCase(Collapsed: collapsed, ShowNotes: showNotes, ShowUndoHistory: showUndoHistory, Layout: layout);
    public static EditorOp BeginRhinoGetter(RhinoDoc document) => new BeginRhinoGetterCase(Document: document);

    // Show is Read-scoped: it opens the editor in-body; headless opens construct the singleton without StatusBar paint.
    GrasshopperUiIntent<EditorResult> IUiOp<EditorResult>.Intent() =>
        new(
            policy: Switch(
                showCase: static _ => GrasshopperUiPolicy.Read,
                stateCase: static _ => GrasshopperUiPolicy.Read,
                shellCase: static _ => GrasshopperUiPolicy.Canvas(openEditor: true),
                beginRhinoGetterCase: static _ => GrasshopperUiPolicy.Read),
            run: _ => Editor.Dispatch(op: this));
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
[StructLayout(LayoutKind.Auto)]
public readonly record struct EditorSnapshot(bool HasEditor = false, bool HasCanvas = false, bool HasDocument = false, bool Collapsed = false, bool HasStatusBar = false, Option<Guid> StatusBarDocumentHash = default, bool ShowNotes = false, bool ShowUndoHistory = false, string InitialLayout = "", Seq<string> DefinedLayouts = default, Option<string> MostRecentActiveDocument = default, Seq<string> MostRecentLoadedDocuments = default, int MostRecentCount = 0);

[StructLayout(LayoutKind.Auto)]
public readonly record struct EditorShellSnapshot(bool Collapsed, bool ShowNotes, bool ShowUndoHistory, string InitialLayout, Seq<string> DefinedLayouts);

// --- [SERVICES] ---------------------------------------------------------------------------
internal static partial class Editor {
    internal static Fin<EditorResult> Dispatch(EditorOp op) => op.Switch(
        showCase: static show => ShowEditor(visible: show.Visible, layoutRules: show.Layout.IfNone(string.Empty), op: Op.Of(name: nameof(EditorOp.Show))),
        stateCase: static _ => DispatchState(),
        shellCase: static shell => ShowEditor(visible: true, layoutRules: shell.Layout.IfNone(string.Empty), op: Op.Of(name: nameof(EditorOp.Shell)), shell: Some(shell)),
        beginRhinoGetterCase: static getter => DispatchRhinoGetter(getter: getter));

    // ShowEditor(false) still Form.Show()s; headless loads construct directly, visible requests use GhEditor.ShowEditor.
    // Shell snapshots after optional chrome writes; op.Attempt re-rails native throws to GhEditor.
    private static Fin<EditorResult> ShowEditor(bool visible, string layoutRules, Op op, Option<EditorOp.ShellCase> shell = default) =>
        op.Attempt(
            body: () => visible
                ? GhEditor.ShowEditor(createVisible: true, layoutRules: layoutRules)
                : GhEditor.Instance ?? new GhEditor(),
            what: "open editor")
        .MapFail(error => UiFault.GhEditor(op: op, detail: error.Message))
        .Bind(current => shell.Match(
            Some: s => ApplyShell(current: current, shell: s),
            None: () => Fin.Succ(value: EditorResult.Unit)));

    private static Fin<EditorResult> ApplyShell(GhEditor current, EditorOp.ShellCase shell) {
        Option<GhCanvas> canvas = Optional(current.Canvas);
        bool requiresHistory = shell.ShowUndoHistory.IfNone(noneValue: false);
        return (requiresHistory, canvas.IsSome) switch {
            (true, false) => Fin.Fail<EditorResult>(error: UiFault.GhEditor(op: Op.Of(name: nameof(EditorOp.Shell)), detail: "ShowUndoHistory requires an active canvas")),
            _ => Fin.Succ<EditorResult>(ShellResultOf(current: current, shell: shell, canvas: canvas)),
        };
    }

    private static (string Initial, Seq<string> Defined) LayoutDefaults() =>
        (Initial: GhEditor.InitialLayout, Defined: toSeq(GhEditor.DefinedLayouts));

    private static EditorResult.ShellResult ShellResultOf(GhEditor current, EditorOp.ShellCase shell, Option<GhCanvas> canvas) {
        _ = shell.Collapsed.Iter(value => current.Collapsed = value);
        _ = shell.ShowNotes.Iter(value => current.ShowNotes = value);
        _ = canvas.Iter(c => shell.ShowUndoHistory.Iter(value => c.ShowUndoHistory = value));
        (string initial, Seq<string> defined) = LayoutDefaults();
        return new EditorResult.ShellResult(Snapshot: new Snapshot<EditorShellSnapshot>(
            OwnerId: Option<Guid>.None,
            Payload: new EditorShellSnapshot(
                Collapsed: current.Collapsed,
                ShowNotes: current.ShowNotes,
                ShowUndoHistory: canvas.Map(static c => c.ShowUndoHistory).IfNone(noneValue: false),
                InitialLayout: initial,
                DefinedLayouts: defined)));
    }

    private static Fin<EditorResult> DispatchState() =>
        Fin.Succ<EditorResult>(value: new EditorResult.StateResult(Snapshot: SnapshotEditor(editor: Optional(GhEditor.Instance))));

    private static EditorSnapshot SnapshotEditor(Option<GhEditor> editor) {
        (string layout, Seq<string> definedLayouts) = LayoutDefaults();
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
    // BeginRhinoGetter returns false for both RhinoGet.InGet and the pending getter timer.
    private static Fin<EditorResult> DispatchRhinoGetter(EditorOp.BeginRhinoGetterCase getter) {
        Op op = Op.Of(name: nameof(EditorOp.BeginRhinoGetter));
        return from document in Optional(getter.Document).ToFin(Fail: UiFault.InvalidInput(op: op, detail: "RhinoDoc is required"))
               from started in op.Attempt(body: () => GhEditor.BeginRhinoGetter(doc: document), what: "begin Rhino getter")
                   .MapFail(error => UiFault.GhEditor(op: op, detail: error.Message))
               from valid in started
                   ? Fin.Succ(value: EditorResult.Unit)
                   : Fin.Fail<EditorResult>(error: UiFault.GhEditor(op: op, detail: "a Rhino getter is already active (command-line getter or pending getter timer)"))
               select valid;
    }
}
