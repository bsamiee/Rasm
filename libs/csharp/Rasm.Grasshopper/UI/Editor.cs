using Foundation.CSharp.Analyzers.Contracts;
using Rasm.Domain;
using Rhino;
using GhEditor = Grasshopper2.UI.Editor;

namespace Rasm.Grasshopper.UI;

// --- [MODELS] ----------------------------------------------------------------------------
public readonly record struct EditorSnapshot(bool HasEditor, bool HasCanvas, bool HasDocument, bool Collapsed);

// --- [TYPES] -----------------------------------------------------------------------------
public abstract record EditorRequest<T> : GhUiRequest<T> {
    public sealed record Show(bool Visible = true, Option<string> Layout = default) : EditorRequest<Unit> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) =>
            Try.lift<Unit>(f: () => {
                GhEditor shown = GhEditor.ShowEditor(createVisible: Visible, layoutRules: Layout.IfNone(string.Empty));
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
    public sealed record EnsureVisible : EditorRequest<Unit> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) =>
            Try.lift<Unit>(f: () => { GhEditor shown = GhEditor.ShowEditor(createVisible: true); return unit; }).Run().MapFail(_ => UiFault.RhinoEditor(detail: nameof(EnsureVisible)));
    }
    public sealed record Collapse(bool Collapsed) : EditorRequest<Unit> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) =>
            Optional(GhEditor.Instance).ToFin(Fail: UiFault.MissingScope(field: "editor")).Map(editor => { editor.Collapsed = Collapsed; return unit; });
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
}
