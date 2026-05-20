using System.Runtime.InteropServices;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.UI;
using Rasm.Domain;
using Rhino;
using GhEditor = Grasshopper2.UI.Editor;

namespace Rasm.Grasshopper.UI;

// --- [MODELS] ----------------------------------------------------------------------------
public readonly record struct EditorSnapshot(bool HasEditor, bool HasCanvas, bool HasDocument, bool Collapsed);

[StructLayout(LayoutKind.Auto)]
public readonly record struct EditorShellSnapshot(
    bool Collapsed,
    bool ShowNotes,
    bool ShowUndoHistory,
    string InitialLayout,
    Seq<string> DefinedLayouts);

// --- [TYPES] -----------------------------------------------------------------------------
public abstract record EditorRequest<T> : GhUiRequest<T> {
    public sealed record Show(bool Visible = true, Option<string> Layout = default) : EditorRequest<Unit> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) =>
            Try.lift<Unit>(f: () => {
                GhEditor editor = GhEditor.ShowEditor(createVisible: Visible, layoutRules: Layout.IfNone(string.Empty));
                _ = Visible ? RecoverVisibility(editor: editor) : unit;
                return unit;
            }).Run().MapFail(_ => UiFault.RhinoEditor(detail: nameof(Show)));
    }
    public sealed record State : EditorRequest<EditorSnapshot> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<EditorSnapshot> Apply(GrasshopperUi.Scope scope) =>
            Fin.Succ(value: GhEditor.Instance switch {
                GhEditor editor => new EditorSnapshot(HasEditor: true, HasCanvas: editor.Canvas is not null, HasDocument: editor.Canvas?.Document is not null, Collapsed: editor.Collapsed),
                _ => new EditorSnapshot(HasEditor: false, HasCanvas: false, HasDocument: false, Collapsed: false),
            });
    }
    public sealed record Shell(
        Option<bool> Collapsed = default,
        Option<bool> ShowNotes = default,
        Option<bool> ShowUndoHistory = default,
        Option<string> Layout = default) : EditorRequest<Snapshot<EditorShellSnapshot>> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<Snapshot<EditorShellSnapshot>> Apply(GrasshopperUi.Scope scope) =>
            Try.lift<Snapshot<EditorShellSnapshot>>(f: () => {
                GhEditor current = Optional(GhEditor.Instance)
                    .IfNone(() => GhEditor.ShowEditor(createVisible: true, layoutRules: Layout.IfNone(string.Empty)));
                _ = Layout.Iter(value => {
                    current = GhEditor.ShowEditor(createVisible: true, layoutRules: value);
                    _ = RecoverVisibility(editor: current);
                });
                _ = Layout.IsNone ? RecoverVisibility(editor: current) : unit;
                _ = Collapsed.Iter(value => current.Collapsed = value);
                _ = ShowNotes.Iter(value => current.ShowNotes = value);
                _ = ShowUndoHistory.Iter(value => current.Canvas.ShowUndoHistory = value);
                return Snapshot.Of(new EditorShellSnapshot(
                    Collapsed: current.Collapsed,
                    ShowNotes: current.ShowNotes,
                    ShowUndoHistory: current.Canvas.ShowUndoHistory,
                    InitialLayout: GhEditor.InitialLayout,
                    DefinedLayouts: toSeq(GhEditor.DefinedLayouts)));
            }).Run().MapFail(_ => UiFault.RhinoEditor(detail: nameof(Shell)));
    }
    public sealed record BeginRhinoGetter(Option<RhinoDoc> Document = default) : EditorRequest<Unit> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) =>
            from active in Optional(Document.IfNone(RhinoDoc.ActiveDoc)).ToFin(Fail: UiFault.MissingScope(field: "rhino-doc"))
            from started in Try.lift<bool>(f: () => GhEditor.BeginRhinoGetter(doc: active)).Run().MapFail(_ => UiFault.RhinoEditor(detail: nameof(BeginRhinoGetter)))
            from valid in started
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(error: UiFault.RhinoEditor(detail: "Rhino getter is already active or no document can receive it"))
            select valid;
    }

    private static Unit RecoverVisibility(GhEditor editor) {
        const System.Reflection.BindingFlags Flags =
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Static |
            System.Reflection.BindingFlags.Instance;
        System.Reflection.MethodInfo? method = typeof(GhEditor).GetMethod(name: "EnsureVisible", bindingAttr: Flags, binder: null, types: Type.EmptyTypes, modifiers: null);
        _ = Optional(method).Iter(visible => visible.Invoke(obj: visible.IsStatic ? null : editor, parameters: null));
        return unit;
    }
}
