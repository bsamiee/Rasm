using System.Drawing;
using Eto.Forms;
using DrawingColor = System.Drawing.Color;

namespace Rasm.Rhino.UI;

public sealed record UiIntent<T> {
    private readonly Func<RhinoUi.Scope, Fin<T>> run;

    internal UiIntent(Func<RhinoUi.Scope, Fin<T>> run, bool interactive) => (this.run, Interactive) = (run, interactive);

    internal bool Interactive { get; }

    internal Fin<T> Run(RhinoUi.Scope scope) => run(arg: scope);
}

public static class UiIntent {
    public static UiIntent<T> Of<T>(Func<RhinoDoc, RunMode, Fin<T>> run, bool interactive = false) =>
        new(run: scope => Optional(run).ToFin(Fail: Op.Of(name: nameof(Of)).InvalidInput()).Bind(valid => valid(arg1: scope.Document, arg2: scope.Mode)), interactive: interactive);

    public static UiIntent<T> Of<T>(Func<RhinoDoc, Fin<T>> run, bool interactive = false) =>
        Of(run: (document, _) => Optional(run).ToFin(Fail: Op.Of(name: nameof(Of)).InvalidInput()).Bind(valid => valid(arg: document)), interactive: interactive);

    internal static UiIntent<T> OfScope<T>(Func<RhinoUi.Scope, Fin<T>> run, bool interactive = false) =>
        new(run: run, interactive: interactive);

    public static UiIntent<T> Dialog<T>(Func<Window?, RhinoDoc, Fin<T>> show) =>
        OfScope(run: scope => Optional(show).ToFin(Fail: Op.Of(name: nameof(Dialog)).InvalidInput()).Bind(valid => valid(arg1: RhinoUi.Parent(document: scope.Document), arg2: scope.Document)), interactive: true);

    public static UiIntent<T> Eto<T>(Eto.Forms.Dialog<T> dialog, bool semiModal = false) =>
        OfScope(
            run: scope => Optional(dialog)
                .ToFin(Fail: Op.Of(name: nameof(Eto)).InvalidInput())
                .Map(valid => semiModal switch {
                    true => global::Rhino.UI.EtoExtensions.ShowSemiModal(valid, scope.Document, parent: RhinoUi.Parent(document: scope.Document)),
                    false => valid.ShowModal(owner: RhinoUi.Parent(document: scope.Document)),
                }),
            interactive: true);

    public static UiIntent<Unit> Modeless(Form form) =>
        OfScope(
            run: scope => Optional(form)
                .ToFin(Fail: Op.Of(name: nameof(Modeless)).InvalidInput())
                .Map(valid => {
                    global::Rhino.UI.EtoExtensions.UseRhinoStyle(valid);
                    global::Rhino.UI.EtoExtensions.Show(valid, scope.Document);
                    return unit;
                }),
            interactive: true);

    public static UiIntent<Unit> Status(UiStatus status) =>
        OfScope(_ => status.Apply());

    public static UiIntent<T> Wait<T>(Func<Fin<T>> run) =>
        OfScope(run: _ => RhinoUi.Protect(valid: () => Optional(run).ToFin(Fail: Op.Of(name: nameof(Wait)).InvalidInput()).Bind(valid => {
            using global::Rhino.UI.WaitCursor cursor = new();
            return valid();
        })), interactive: true);

    public static UiIntent<T> Panel<TPanel, T>(PanelOp<TPanel, T> operation) where TPanel : RasmPanel =>
        OfScope(run: scope => Optional(operation).ToFin(Fail: Op.Of(name: nameof(Panel)).InvalidInput()).Bind(valid => valid.Run(document: scope.Document)), interactive: Optional(operation).Map(static value => value.Interactive).IfNone(false));

    public static UiIntent<T> Progress<T>(UiProgressSpec spec, Func<UiProgress, Fin<T>> run) =>
        OfScope(scope => Optional(run).ToFin(Fail: Op.Of(name: nameof(Progress)).InvalidInput()).Bind(valid => UiProgress.Use(document: scope.Document, spec: spec, run: valid)));

    public static UiIntent<T> Gumball<T>(UiGumballSpec spec, Func<UiGumball, Fin<T>> run) =>
        OfScope(scope => Optional(run).ToFin(Fail: Op.Of(name: nameof(Gumball)).InvalidInput()).Bind(valid => UiGumball.Use(document: scope.Document, spec: spec, run: valid)), interactive: true);

    public static UiIntent<Color4f> Color(UiColorSpec spec, bool allowAlpha = false) =>
        OfScope(scope => {
            Color4f color = spec.Initial;
            global::Rhino.UI.NamedColorList? named = spec.Named.Case is global::Rhino.UI.NamedColorList list ? list : null;
            object? parent = RhinoUi.Parent(document: scope.Document);
            return global::Rhino.UI.Dialogs.ShowColorDialog(parent: parent, color: ref color, allowAlpha: allowAlpha, namedColorList: named, colorCallback: null) switch {
                true => Fin.Succ(value: color),
                false => Fin.Fail<Color4f>(error: new Fault.Cancelled()),
            };
        }, interactive: true);

    public static UiIntent<T> Choice<T>(string title, string message, IEnumerable<T> items, Option<T> selected = default, bool combo = false) =>
        OfScope(run: _ => Optional(items)
            .ToFin(Fail: Op.Of(name: nameof(Choice)).InvalidInput())
            .Bind(values => NonEmpty(values: toSeq(values)))
            .Bind(values => Optional(combo switch {
                true => global::Rhino.UI.Dialogs.ShowComboListBox(title: title, message: message, items: values.ToList()),
                false => selected.Case switch {
                    T current => global::Rhino.UI.Dialogs.ShowListBox(title: title, message: message, items: values.ToList(), selectedItem: current),
                    _ => global::Rhino.UI.Dialogs.ShowListBox(title: title, message: message, items: values.ToList()),
                },
            }).ToFin(Fail: new Fault.Cancelled()).Bind(Cast<T>)), interactive: true);

    public static UiIntent<Seq<string>> MultiChoice(string title, string message, IEnumerable<string> items, IEnumerable<string>? selected = null) =>
        OfScope(run: _ => Optional(items).ToFin(Fail: Op.Of(name: nameof(MultiChoice)).InvalidInput()).Bind(source => (toSeq(source), Optional(selected).Map(static current => toSeq(current)).IfNone(Seq<string>())) switch {
            (Seq<string> values, _) when values.IsEmpty => Fin.Fail<Seq<string>>(error: Op.Of(name: nameof(MultiChoice)).InvalidInput()),
            (Seq<string> values, Seq<string> defaults) => Optional(global::Rhino.UI.Dialogs.ShowMultiListBox(title: title, message: message, items: [.. values], defaults: [.. defaults])).ToFin(Fail: new Fault.Cancelled()).Map(static result => toSeq(result)),
        }), interactive: true);

    public static UiIntent<string> Edit(string title, string message, string value = "", bool multiline = false) =>
        OfScope(run: _ => global::Rhino.UI.Dialogs.ShowEditBox(title: title, message: message, defaultText: value, multiline: multiline, text: out string result) switch {
            true => Fin.Succ(value: result),
            false => Fin.Fail<string>(error: new Fault.Cancelled()),
        }, interactive: true);

    public static UiIntent<double> Number(string title, string message, double value = 0.0, Option<(double Lower, double Upper)> bounds = default) =>
        OfScope(run: _ => bounds.Case switch {
            (double lower, double upper) => global::Rhino.UI.Dialogs.ShowNumberBox(title: title, message: message, number: ref value, minimum: lower, maximum: upper),
            _ => global::Rhino.UI.Dialogs.ShowNumberBox(title: title, message: message, number: ref value),
        } switch {
            true => Fin.Succ(value: value),
            false => Fin.Fail<double>(error: new Fault.Cancelled()),
        }, interactive: true);

    private static Fin<Seq<T>> NonEmpty<T>(Seq<T> values) =>
        values.IsEmpty switch {
            true => Fin.Fail<Seq<T>>(error: Op.Of(name: nameof(UiIntent)).InvalidInput()),
            false => Fin.Succ(value: values),
        };

    private static Fin<T> Cast<T>(object value) =>
        value is T typed ? Fin.Succ(value: typed) : Fin.Fail<T>(error: Op.Of(name: nameof(Cast)).InvalidResult());

    public static UiIntent<Bitmap> MeshPreview(IEnumerable<Mesh> meshes, IEnumerable<DrawingColor> colors, Size size) =>
        OfScope(scope => (Optional(meshes).ToFin(Fail: Op.Of(name: nameof(MeshPreview)).InvalidInput()), Optional(colors).ToFin(Fail: Op.Of(name: nameof(MeshPreview)).InvalidInput()))
            .Apply(static (geometry, swatches) => (Meshes: geometry, Colors: swatches)).As()
            .Bind(values => Optional(global::Rhino.UI.DrawingUtilities.CreateMeshPreviewImage(doc: scope.Document, meshes: values.Meshes, colors: values.Colors, size: size)).ToFin(Fail: Op.Of(name: nameof(MeshPreview)).InvalidResult())));

    public static UiIntent<Seq<Point2f[]>> CurvePreview(Curve curve, Linetype linetype, Size size) =>
        OfScope(_ => Optional(global::Rhino.UI.DrawingUtilities.CreateCurvePreviewGeometry(curve: curve, linetype: linetype, width: size.Width, height: size.Height)).ToFin(Fail: Op.Of(name: nameof(CurvePreview)).InvalidResult()).Map(static result => toSeq(result)));

    public static UiIntent<Seq<global::Rhino.UI.NamedColor>> NamedColors(global::Rhino.UI.NamedColorList? source = null) =>
        OfScope(_ => Fin.Succ(value: toSeq(Optional(source).IfNone(global::Rhino.UI.NamedColorList.Default))));
}

public readonly record struct UiColorSpec(Color4f Initial, Option<global::Rhino.UI.NamedColorList> Named = default);
