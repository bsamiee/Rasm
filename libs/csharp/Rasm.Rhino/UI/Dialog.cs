using System.Reflection;
using Eto.Forms;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingColor = System.Drawing.Color;
using DrawingSize = System.Drawing.Size;

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
        Request(name: nameof(Dialog), run: scope => Optional(show).ToFin(Fail: Op.Of(name: nameof(Dialog)).InvalidInput()).Bind(valid => valid(arg1: RhinoUi.Parent(document: scope.Document), arg2: scope.Document)));

    public static UiIntent<T> Eto<T>(Eto.Forms.Dialog<T> dialog, bool semiModal = false) =>
        Request(
            name: nameof(Eto),
            run: scope => Optional(dialog)
                .ToFin(Fail: Op.Of(name: nameof(Eto)).InvalidInput())
                .Map(valid => semiModal switch {
                    true => global::Rhino.UI.EtoExtensions.ShowSemiModal(valid, scope.Document, parent: RhinoUi.Parent(document: scope.Document)),
                    false => valid.ShowModal(owner: RhinoUi.Parent(document: scope.Document)),
                }));

    public static UiIntent<Unit> Modeless(Form form) =>
        Request(
            name: nameof(Modeless),
            run: scope => Optional(form)
                .ToFin(Fail: Op.Of(name: nameof(Modeless)).InvalidInput())
                .Map(valid => {
                    global::Rhino.UI.EtoExtensions.UseRhinoStyle(valid);
                    global::Rhino.UI.EtoExtensions.Show(valid, scope.Document);
                    return unit;
                }));

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

    public static UiIntent<T> Preview<T>(UiPreview<T> preview) =>
        OfScope(
            run: scope => Optional(preview)
                .ToFin(Fail: Op.Of(name: nameof(Preview)).InvalidInput())
                .Bind(valid => valid.Run(scope: scope)),
            interactive: Optional(preview).Map(static value => value.Interactive).IfNone(false));

    public static UiIntent<global::Rhino.UI.ShowMessageResult> Message(UiMessageSpec spec) =>
        Request(
            name: nameof(Message),
            run: scope => string.IsNullOrWhiteSpace(value: spec.Message) switch {
                false => Fin.Succ(value: global::Rhino.UI.Dialogs.ShowMessage(parent: RhinoUi.Parent(document: scope.Document), message: spec.Message, title: spec.Title, buttons: spec.Buttons, icon: spec.Icon, defaultButton: spec.DefaultButton, options: spec.Options, mode: spec.Mode)),
                true => Fin.Fail<global::Rhino.UI.ShowMessageResult>(error: Op.Of(name: nameof(Message)).InvalidInput()),
            });

    public static UiIntent<T> Choice<T>(string title, string message, IEnumerable<T> items, Option<T> selected = default, bool combo = false) =>
        Request(name: nameof(Choice), run: _ => {
            Fin<Seq<T>> values = Optional(items)
                .ToFin(Fail: Op.Of(name: nameof(Choice)).InvalidInput())
                .Bind(source => toSeq(source) switch {
                    Seq<T> choices when !choices.IsEmpty => Fin.Succ(value: choices),
                    _ => Fin.Fail<Seq<T>>(error: Op.Of(name: nameof(Choice)).InvalidInput()),
                });
            return values.Bind(choices => Optional(combo switch {
                true => global::Rhino.UI.Dialogs.ShowComboListBox(title: title, message: message, items: choices.ToList()),
                false => selected.Case switch {
                    T current => global::Rhino.UI.Dialogs.ShowListBox(title: title, message: message, items: choices.ToList(), selectedItem: current),
                    _ => global::Rhino.UI.Dialogs.ShowListBox(title: title, message: message, items: choices.ToList()),
                },
            }).ToFin(Fail: new Fault.Cancelled()).Bind(result => result is T typed ? Fin.Succ(value: typed) : Fin.Fail<T>(error: Op.Of(name: nameof(Choice)).InvalidResult())));
        });

    public static UiIntent<Seq<string>> MultiChoice(string title, string message, IEnumerable<string> items, IEnumerable<string>? selected = null) =>
        Request(name: nameof(MultiChoice), run: _ => Optional(items).ToFin(Fail: Op.Of(name: nameof(MultiChoice)).InvalidInput()).Bind(source => (toSeq(source), Optional(selected).Map(static current => toSeq(current)).IfNone(Seq<string>())) switch {
            (Seq<string> values, _) when values.IsEmpty => Fin.Fail<Seq<string>>(error: Op.Of(name: nameof(MultiChoice)).InvalidInput()),
            (Seq<string> values, Seq<string> defaults) => Optional(global::Rhino.UI.Dialogs.ShowMultiListBox(title: title, message: message, items: [.. values], defaults: [.. defaults])).ToFin(Fail: new Fault.Cancelled()).Map(static result => toSeq(result)),
        }));

    public static UiIntent<string> Edit(string title, string message, string value = "", bool multiline = false) =>
        Request(name: nameof(Edit), run: _ => global::Rhino.UI.Dialogs.ShowEditBox(title: title, message: message, defaultText: value, multiline: multiline, text: out string result) switch {
            true => Fin.Succ(value: result),
            false => Fin.Fail<string>(error: new Fault.Cancelled()),
        });

    public static UiIntent<double> Number(string title, string message, double value = 0.0, Option<(double Lower, double Upper)> bounds = default) =>
        Request(name: nameof(Number), run: _ => bounds.Case switch {
            (double lower, double upper) => global::Rhino.UI.Dialogs.ShowNumberBox(title: title, message: message, number: ref value, minimum: lower, maximum: upper),
            _ => global::Rhino.UI.Dialogs.ShowNumberBox(title: title, message: message, number: ref value),
        } switch {
            true => Fin.Succ(value: value),
            false => Fin.Fail<double>(error: new Fault.Cancelled()),
        });

    private static UiIntent<T> Request<T>(string name, Func<RhinoUi.Scope, Fin<T>> run, bool interactive = true) =>
        OfScope(
            run: scope => Optional(run)
                .ToFin(Fail: Op.Of(name: name).InvalidInput())
                .Bind(valid => valid(arg: scope)),
            interactive: interactive);
}

public sealed record UiPreview<T> {
    private readonly string name;
    private readonly Func<RhinoUi.Scope, Fin<T>> run;

    internal UiPreview(string name, Func<RhinoUi.Scope, Fin<T>> run, bool interactive) =>
        (this.name, this.run, Interactive) = (name, run, interactive);

    internal bool Interactive { get; }

    internal Fin<T> Run(RhinoUi.Scope scope) =>
        Optional(run)
            .ToFin(Fail: Op.Of(name: name).InvalidInput())
            .Bind(valid => valid(arg: scope));
}

public static class UiPreview {
    public static UiPreview<T> Of<T>(string name, Func<RhinoDoc, RunMode, Fin<T>> run, bool interactive = false) {
        string operation = string.IsNullOrWhiteSpace(value: name) switch {
            false => name,
            true => nameof(UiPreview),
        };
        return new(
            name: operation,
            run: scope => Optional(run)
                .ToFin(Fail: Op.Of(name: operation).InvalidInput())
                .Bind(valid => valid(arg1: scope.Document, arg2: scope.Mode)),
            interactive: interactive);
    }

    public static UiPreview<DrawingBitmap> Mesh(IEnumerable<Mesh> meshes, DrawingSize size, IEnumerable<DrawingColor>? colors = null) =>
        Of(
            name: nameof(Mesh),
            run: (document, _) =>
                (Optional(meshes).ToFin(Fail: Op.Of(name: nameof(Mesh)).InvalidInput()).Bind(static source => toSeq(source).TraverseM(mesh => Optional(mesh).ToFin(Fail: Op.Of(name: nameof(Mesh)).InvalidInput())).As()),
                 Optional(colors).Map(static source => toSeq(source)).IfNone(Seq<DrawingColor>()).TraverseM(static color => color.IsEmpty switch {
                     false => Fin.Succ(value: color),
                     true => Fin.Fail<DrawingColor>(error: Op.Of(name: nameof(Mesh)).InvalidInput()),
                 }).As())
                .Apply(static (geometry, swatches) => (Meshes: geometry, Colors: swatches))
                .As()
                .Bind(values =>
                    from _ in guard(!values.Meshes.IsEmpty && size.Width > 0 && size.Height > 0, Op.Of(name: nameof(Mesh)).InvalidInput())
                    from colors in values.Colors.Count switch {
                        0 => Fin.Succ(value: values.Meshes.Map(_ => document.CreateDefaultAttributes().DrawColor(document))),
                        1 => Fin.Succ(value: values.Meshes.Map(_ => values.Colors[0])),
                        int count when count == values.Meshes.Count => Fin.Succ(value: values.Colors),
                        _ => Fin.Fail<Seq<DrawingColor>>(error: Op.Of(name: nameof(Mesh)).InvalidInput()),
                    }
                    from bitmap in Optional(global::Rhino.UI.DrawingUtilities.CreateMeshPreviewImage(doc: document, meshes: values.Meshes, colors: colors, size: size))
                        .ToFin(Fail: Op.Of(name: nameof(Mesh)).InvalidResult())
                    select bitmap));

    public static UiPreview<Seq<Point2f[]>> Curve(Curve curve, Linetype linetype, DrawingSize size) =>
        Of(
            name: nameof(Curve),
            run: (_, _) =>
                (Optional(curve).ToFin(Fail: Op.Of(name: nameof(Curve)).InvalidInput()),
                 Optional(linetype).ToFin(Fail: Op.Of(name: nameof(Curve)).InvalidInput()))
                .Apply(static (validCurve, validLinetype) => (Curve: validCurve, Linetype: validLinetype))
                .As()
                .Bind(values =>
                    from _ in guard(size.Width > 0 && size.Height > 0, Op.Of(name: nameof(Curve)).InvalidInput())
                    from preview in Optional(global::Rhino.UI.DrawingUtilities.CreateCurvePreviewGeometry(curve: values.Curve, linetype: values.Linetype, width: size.Width, height: size.Height))
                        .ToFin(Fail: Op.Of(name: nameof(Curve)).InvalidResult())
                    select toSeq(preview)));

    public static UiPreview<DrawingBitmap> Icon(string resourceName, DrawingSize size, Assembly assembly) =>
        Of(
            name: nameof(Icon),
            run: (_, _) =>
                from name in Optional(resourceName).ToFin(Fail: Op.Of(name: nameof(Icon)).InvalidInput())
                    .Bind(static value => string.IsNullOrWhiteSpace(value: value) switch {
                        false => Fin.Succ(value: value),
                        true => Fin.Fail<string>(error: Op.Of(name: nameof(Icon)).InvalidInput()),
                    })
                from validAssembly in Optional(assembly).ToFin(Fail: Op.Of(name: nameof(Icon)).InvalidInput())
                from _ in guard(size.Width > 0 && size.Height > 0, Op.Of(name: nameof(Icon)).InvalidInput())
                from bitmap in Optional(global::Rhino.UI.DrawingUtilities.BitmapFromIconResource(name, size, validAssembly))
                    .ToFin(Fail: Op.Of(name: nameof(Icon)).InvalidResult())
                select bitmap);

    public static UiPreview<DrawingBitmap> Svg(string svg, int width, int height) =>
        Of(
            name: nameof(Svg),
            run: (_, _) =>
                from source in Optional(svg).ToFin(Fail: Op.Of(name: nameof(Svg)).InvalidInput())
                    .Bind(static value => string.IsNullOrWhiteSpace(value: value) switch {
                        false => Fin.Succ(value: value),
                        true => Fin.Fail<string>(error: Op.Of(name: nameof(Svg)).InvalidInput()),
                    })
                from _ in guard(width > 0 && height > 0, Op.Of(name: nameof(Svg)).InvalidInput())
                from bitmap in Optional(global::Rhino.UI.DrawingUtilities.BitmapFromSvg(svg: source, width: width, height: height, adjustForDarkMode: global::Rhino.Runtime.HostUtils.RunningInDarkMode))
                    .ToFin(Fail: Op.Of(name: nameof(Svg)).InvalidResult())
                select bitmap);

    public static UiPreview<Seq<global::Rhino.UI.NamedColor>> NamedColors(Option<global::Rhino.UI.NamedColorList> source = default) =>
        Of(
            name: nameof(NamedColors),
            run: (_, _) => Fin.Succ(value: toSeq(source.IfNone(global::Rhino.UI.NamedColorList.Default))));

    public static UiPreview<Color4f> Color(UiColorSpec spec, bool allowAlpha = false) =>
        Of(
            name: nameof(Color),
            run: (document, _) => {
                Color4f color = spec.Initial;
                global::Rhino.UI.NamedColorList? named = spec.Named.Case switch {
                    global::Rhino.UI.NamedColorList value => value,
                    _ => null,
                };
                return global::Rhino.UI.Dialogs.ShowColorDialog(parent: RhinoUi.Parent(document: document), color: ref color, allowAlpha: allowAlpha, namedColorList: named, colorCallback: spec.Changed) switch {
                    true => Fin.Succ(value: color),
                    false => Fin.Fail<Color4f>(error: new Fault.Cancelled()),
                };
            },
            interactive: true);

    public static UiPreview<T> Viewport<T>(UiViewportPreview preview, Func<UiPreviewScope, Fin<T>> run, Option<UiGumballSpec> gumball = default) =>
        Of(
            name: nameof(Viewport),
            run: (document, _) => UiViewportPreview.Use(document: document, preview: preview, gumball: gumball, run: run),
            interactive: true);
}

public readonly record struct UiMessageSpec(
    string Message,
    string Title = "Rasm",
    global::Rhino.UI.ShowMessageButton Buttons = default,
    global::Rhino.UI.ShowMessageIcon Icon = default,
    global::Rhino.UI.ShowMessageDefaultButton DefaultButton = default,
    global::Rhino.UI.ShowMessageOptions Options = default,
    global::Rhino.UI.ShowMessageMode Mode = default);

public readonly record struct UiColorSpec(
    Color4f Initial,
    Option<global::Rhino.UI.NamedColorList> Named = default,
    global::Rhino.UI.Dialogs.OnColorChangedEvent? Changed = null);
