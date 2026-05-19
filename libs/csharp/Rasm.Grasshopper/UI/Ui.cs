using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using Grasshopper2.UI.Skinning;
using Rhino;
using GhCanvas = Grasshopper2.UI.Canvas.Canvas;
using GhEditor = Grasshopper2.UI.Editor;

namespace Rasm.Grasshopper.UI;

// --- [MODELS] ---------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct GrasshopperUiPolicy(
    bool OpenEditor = false,
    bool RequireCanvas = false,
    bool RequireDocument = false,
    bool Repaint = false) {
    public static GrasshopperUiPolicy Read => default;

    public static GrasshopperUiPolicy Canvas(bool openEditor = false, bool repaint = false) =>
        new(OpenEditor: openEditor, RequireCanvas: true, Repaint: repaint);

    public static GrasshopperUiPolicy Document(bool repaint = false) =>
        new(RequireCanvas: true, RequireDocument: true, Repaint: repaint);

    public static GrasshopperUiPolicy operator |(GrasshopperUiPolicy left, GrasshopperUiPolicy right) =>
        BitwiseOr(left: left, right: right);

    public static GrasshopperUiPolicy BitwiseOr(GrasshopperUiPolicy left, GrasshopperUiPolicy right) =>
        new(
            OpenEditor: left.OpenEditor || right.OpenEditor,
            RequireCanvas: left.RequireCanvas || right.RequireCanvas,
            RequireDocument: left.RequireDocument || right.RequireDocument,
            Repaint: left.Repaint || right.Repaint);
}

public sealed record GrasshopperUiIntent<T> {
    private readonly Func<GrasshopperUi.Scope, Fin<T>> run;

    internal GrasshopperUiIntent(Func<GrasshopperUi.Scope, Fin<T>> run, GrasshopperUiPolicy policy) {
        this.run = run;
        Policy = policy;
    }

    internal GrasshopperUiPolicy Policy { get; }

    internal Fin<T> Run(GrasshopperUi.Scope scope) => run(arg: scope);
}

public static class GrasshopperUiIntent {
    internal static GrasshopperUiIntent<T> Of<T>(Func<GrasshopperUi.Scope, Fin<T>> run, GrasshopperUiPolicy policy = default) =>
        new(run: run, policy: policy);

    public static GrasshopperUiIntent<Unit> BeginRhinoGetter(RhinoDoc? document = null) =>
        new(
            run: _ =>
                from active in Optional(document ?? RhinoDoc.ActiveDoc).ToFin(Fail: Op.Of(name: nameof(BeginRhinoGetter)).InvalidInput())
                from started in GhEditor.BeginRhinoGetter(doc: active) switch {
                    true => Fin.Succ(value: unit),
                    false => Fin.Fail<Unit>(error: Op.Of(name: nameof(BeginRhinoGetter)).InvalidResult()),
                }
                select started,
            policy: GrasshopperUiPolicy.Read);
}

// --- [SERVICES] -------------------------------------------------------------------------
[BoundaryAdapter]
public sealed partial record GrasshopperUi {
    [StructLayout(LayoutKind.Auto)]
    internal readonly record struct Scope(
        Option<GhEditor> Editor,
        Option<GhCanvas> Canvas,
        Option<Document> Document,
        Option<DocumentMethods> Methods,
        Option<ObjectList> Objects,
        Option<Skin> Skin) {
        internal static Fin<Scope> Resolve(GrasshopperUiPolicy policy) {
            GhEditor? editor = (GhEditor.Instance, policy.OpenEditor) switch {
                (GhEditor current, _) => current,
                (_, true) => GhEditor.ShowEditor(createVisible: true),
                _ => null,
            };
            GhCanvas? canvas = editor?.Canvas;
            Document? document = canvas?.Document;
            Scope scope = new(
                Editor: Optional(editor),
                Canvas: Optional(canvas),
                Document: Optional(document),
                Methods: Optional(document?.Methods),
                Objects: Optional(document?.Objects),
                Skin: Optional(canvas?.Skin));
            return (policy.RequireCanvas, scope.Canvas.IsSome, policy.RequireDocument, scope.Document.IsSome) switch {
                (true, false, _, _) => Fin.Fail<Scope>(error: Op.Of(name: nameof(Resolve)).InvalidInput()),
                (_, _, true, false) => Fin.Fail<Scope>(error: Op.Of(name: nameof(Resolve)).InvalidInput()),
                _ => Fin.Succ(value: scope),
            };
        }
    }

    public Fin<T> Use<T>(GrasshopperUiIntent<T> intent) =>
        from valid in Optional(intent).ToFin(Fail: Op.Of(name: nameof(Use)).InvalidInput())
        from result in OnUiThread(run: () =>
            from scope in Scope.Resolve(policy: valid.Policy)
            from value in valid.Run(scope: scope)
            select Repaint(scope: scope, policy: valid.Policy, value: value))
        select result;

    internal static Fin<T> OnUiThread<T>(Func<Fin<T>> run) =>
        Optional(run)
            .ToFin(Fail: Op.Of(name: nameof(OnUiThread)).InvalidInput())
            .Bind(valid => RhinoApp.IsOnMainThread switch {
                true => Protect(valid: valid),
                false => Try.lift<Fin<T>>(f: () => {
                    Fin<T> result = Fin.Fail<T>(error: Op.Of(name: nameof(OnUiThread)).InvalidResult());
                    RhinoApp.InvokeAndWait(action: () => result = Protect(valid: valid));
                    return result;
                })
                    .Run()
                    .MapFail(_ => Op.Of(name: nameof(OnUiThread)).InvalidResult())
                    .Bind(static result => result),
            });

    internal static Fin<T> Protect<T>(Func<Fin<T>> valid, [CallerMemberName] string name = "") =>
        Optional(valid)
            .ToFin(Fail: Op.Of(name: name).InvalidInput())
            .Bind(callback => Try.lift<Fin<T>>(f: callback)
                .Run()
                .MapFail(_ => Op.Of(name: name).InvalidResult())
                .Bind(static result => result));

    private static T Repaint<T>(Scope scope, GrasshopperUiPolicy policy, T value) {
        _ = policy.Repaint ? scope.Canvas.Map(static canvas => {
            canvas.Invalidate();
            return unit;
        }) : Option<Unit>.None;
        return value;
    }
}
