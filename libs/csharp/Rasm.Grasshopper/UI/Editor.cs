using Foundation.CSharp.Analyzers.Contracts;
using Rasm.Domain;
using Rhino;
using GhEditor = Grasshopper2.UI.Editor;

namespace Rasm.Grasshopper.UI;

// --- [SERVICES] --------------------------------------------------------------------------
public static partial class Editor {
    public static GrasshopperUiIntent<Unit> Show(bool visible = true, string? layoutName = null) =>
        IntentFactory.Read<Unit>(run: _ =>
            Try.lift<Unit>(f: () => { _ = GhEditor.ShowEditor(createVisible: visible); return unit; })
                .Run()
                .MapFail(_ => UiFault.RhinoEditor(detail: nameof(Show))));

    public static GrasshopperUiIntent<Unit> EnsureVisible() =>
        IntentFactory.Read<Unit>(run: _ =>
            Optional(GhEditor.Instance)
                .ToFin(Fail: UiFault.MissingScope(field: "editor"))
                .Map(editor => { editor.EnsureVisible(); return unit; }));

    public static GrasshopperUiIntent<Unit> Collapsed(bool collapsed) =>
        IntentFactory.Read<Unit>(run: _ =>
            Optional(GhEditor.Instance)
                .ToFin(Fail: UiFault.MissingScope(field: "editor"))
                .Map(editor => { editor.Collapsed = collapsed; return unit; }));

    public static GrasshopperUiIntent<Unit> BeginRhinoGetter(RhinoDoc? document = null) =>
        IntentFactory.Read<Unit>(run: _ =>
            from active in Optional(document ?? RhinoDoc.ActiveDoc).ToFin(Fail: UiFault.MissingScope(field: "rhino-doc"))
            from _ in Try.lift<Unit>(f: () => { GhEditor.BeginRhinoGetter(doc: active); return unit; })
                .Run()
                .MapFail(_ => UiFault.RhinoEditor(detail: nameof(BeginRhinoGetter)))
            select unit);
}
