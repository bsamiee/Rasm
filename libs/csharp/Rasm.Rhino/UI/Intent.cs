using System.Reflection;
using Eto.Forms;
using Rasm.Rhino.Exchange;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingColor = System.Drawing.Color;
using DrawingSize = System.Drawing.Size;

namespace Rasm.Rhino.UI;

// --- [TYPES] ------------------------------------------------------------------------------
public enum UiWindowMode { Modal, SemiModal }

[Union(SwitchMapStateParameterName = "scope")]
public abstract partial record UiLayerRequest {
    private UiLayerRequest(string title, Option<Seq<int>> selected, bool showNewLayer) =>
        (Title, Selected, ShowNewLayer) = (title, selected, showNewLayer);

    public string Title { get; }
    public Option<Seq<int>> Selected { get; }
    public bool ShowNewLayer { get; }

    public sealed record One(
        string Title,
        Option<Seq<int>> Selected = default,
        bool ShowNewLayer = false,
        bool ShowSetCurrent = false,
        bool InitialSetCurrent = false) : UiLayerRequest(Title, Selected, ShowNewLayer);

    public sealed record Multiple(
        string Title,
        Option<Seq<int>> Selected = default,
        bool ShowNewLayer = false) : UiLayerRequest(Title, Selected, ShowNewLayer);

    public sealed record Material(
        string Title,
        Option<Seq<int>> Selected = default,
        bool ShowNewLayer = false) : UiLayerRequest(Title, Selected, ShowNewLayer);
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record UiIntent<T> {
    private readonly Func<RhinoUi.Scope, Fin<T>> run;

    internal UiIntent(Func<RhinoUi.Scope, Fin<T>> run, bool interactive, Option<Func<RhinoUi.Scope, Fin<T>>> scripted = default) =>
        (this.run, Interactive, Scripted) = (run, interactive, scripted);

    internal bool Interactive { get; }
    internal Option<Func<RhinoUi.Scope, Fin<T>>> Scripted { get; }

    public UiIntent<TNext> Map<TNext>(Func<T, TNext> project) =>
        new(
            run: scope => Op.Of(name: nameof(Map)).Need(project).Bind(valid => run(arg: scope).Map(value => valid(arg: value))),
            interactive: Interactive,
            scripted: Scripted.Map<Func<RhinoUi.Scope, Fin<TNext>>>(script => scope => Op.Of(name: nameof(Map)).Need(project).Bind(valid => script(arg: scope).Map(value => valid(arg: value)))));

    public UiIntent<T> WithScripted(Func<RhinoDoc, RunMode, Fin<T>> fallback) =>
        new(
            run: run,
            interactive: Interactive,
            scripted: Optional(fallback).Map<Func<RhinoUi.Scope, Fin<T>>>(project => scope => project(arg1: scope.Document, arg2: scope.Mode)));

    public UiIntent<T> WithScripted(Func<RhinoDoc, Fin<T>> fallback) =>
        WithScripted(fallback: (document, _) => Op.Of(name: nameof(WithScripted)).Need(fallback).Bind(project => project(arg: document)));

    internal Fin<T> Run(RhinoUi.Scope scope) => run(arg: scope);
}

public readonly record struct UiColorSpec(Color4f Initial, Option<global::Rhino.UI.NamedColorList> Named = default, global::Rhino.UI.Dialogs.OnColorChangedEvent? Changed = null, bool AllowAlpha = false);

public readonly record struct UiLayerResult(Seq<int> LayerIndices, bool SetCurrent, bool MaterialAccepted) {
    public Option<int> LayerIndex => LayerIndices.Find(static index => index >= 0);
}

public readonly record struct UiMessageSpec(string Message, string Title, global::Rhino.UI.ShowMessageButton Buttons = default, global::Rhino.UI.ShowMessageIcon Icon = default, global::Rhino.UI.ShowMessageDefaultButton DefaultButton = default, global::Rhino.UI.ShowMessageOptions Options = default, global::Rhino.UI.ShowMessageMode Mode = default);

public readonly record struct UiWindowHandle(uint DocumentSerialNumber, string WindowType, string Title, bool Visible, bool Restored = false);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class UiIntent {
    public static UiIntent<T> Of<T>(Func<RhinoDoc, RunMode, Fin<T>> run) =>
        new(run: scope => Op.Of(name: nameof(Of)).Need(run).Bind(valid => valid(arg1: scope.Document, arg2: scope.Mode)), interactive: true);

    public static UiIntent<T> Operation<T>(Func<RhinoDoc, RunMode, Fin<T>> run) =>
        OfScope(
            run: scope => Op.Of(name: nameof(Operation)).Need(run).Bind(valid => valid(arg1: scope.Document, arg2: scope.Mode)),
            interactive: false);

    public static UiIntent<T> Request<T>(UiRequest<T> request) =>
        OfScope(
            run: scope => Op.Of(name: nameof(Request)).Need(request).Bind(valid => valid.Run(scope: scope)),
            interactive: Optional(request).Map(static value => value.Interactive).IfNone(noneValue: false));

    public static UiIntent<Unit> Enqueue(Action run, string name = nameof(Enqueue)) =>
        OfScope(_ => RhinoUi.Enqueue(run: run, name: name), interactive: false);

    public static UiIntent<T> Window<T>(Dialog<T> dialog, UiWindowMode mode = UiWindowMode.Modal) =>
        Request(name: nameof(Window), run: scope => Op.Of(name: nameof(Window)).Need(dialog).Bind(valid => mode switch {
            // ShowSemiModal's parent is non-nullable Control and carries no button prerequisite — fall back to the doc main window when no Owner is set
            UiWindowMode.SemiModal => Fin.Succ(value: global::Rhino.UI.EtoExtensions.ShowSemiModal(valid, scope.Document, parent: scope.Parent ?? global::Rhino.UI.RhinoEtoApp.MainWindowForDocument(scope.Document))),
            UiWindowMode.Modal => Fin.Succ(value: valid.ShowModal(owner: scope.Parent)),
            _ => Fin.Fail<T>(error: Op.Of(name: nameof(Window)).InvalidInput()),
        }));

    public static UiIntent<UiWindowHandle> Window(Form form, bool restoreLocation = false, bool savePosition = false) =>
        Request(name: nameof(Window), run: scope => Op.Of(name: nameof(Window)).Need(form).Map(valid => {
            global::Rhino.UI.EtoExtensions.UseRhinoStyle(valid);
            bool restored = restoreLocation && global::Rhino.UI.EtoExtensions.RestorePosition(window: valid);
            _ = Op.SideWhen(savePosition, () => valid.Closed += (_, _) => global::Rhino.UI.EtoExtensions.SavePosition(window: valid));
            global::Rhino.UI.EtoExtensions.Show(valid, scope.Document);
            return new UiWindowHandle(DocumentSerialNumber: scope.Document.RuntimeSerialNumber, WindowType: valid.GetType().FullName ?? valid.GetType().Name, Title: valid.Title ?? string.Empty, Visible: valid.Visible, Restored: restored);
        }));

    public static UiIntent<Unit> Status(UiStatus status) =>
        OfScope(_ => status.Apply());
    public static UiIntent<Unit> Redraw(RedrawTarget target) =>
        OfScope(run: _ => RhinoUi.Protect(valid: () => Op.Of(name: nameof(Redraw)).Need(target).Map(valid => valid.Repaint())), interactive: false);

    public static UiIntent<Unit> Float(RhinoView view, bool floating = true) =>
        OfScope(run: _ => RhinoUi.Protect(valid: () => Op.Of(name: nameof(Float)).Need(view).Map(valid => Op.Side(() => valid.Floating = floating))), interactive: false);

    public static UiIntent<T> Wait<T>(Func<Fin<T>> run) =>
        OfScope(run: _ => RhinoUi.Protect(valid: () => Op.Of(name: nameof(Wait)).Need(run).Bind(valid => {
            using global::Rhino.UI.WaitCursor cursor = new();
            return valid();
        })), interactive: true);

    public static UiIntent<T> Progress<T>(UiProgressSpec spec, Func<UiProgress, Fin<T>> run) =>
        OfScope(scope => Op.Of(name: nameof(Progress)).Need(run).Bind(valid => UiProgress.Use(document: scope.Document, spec: spec, run: valid)));

    public static UiIntent<T> Gumball<T>(UiGumballSpec spec, Func<UiGumball, Fin<T>> run, bool interactive = true) =>
        OfScope(scope => Op.Of(name: nameof(Gumball)).Need(run).Bind(valid => UiGumball.Use(document: scope.Document, spec: spec, run: valid)), interactive: interactive);

    public static UiIntent<global::Rhino.UI.ShowMessageResult> Message(UiMessageSpec spec) =>
        Request(
            name: nameof(Message),
            run: scope => guard(!string.IsNullOrWhiteSpace(value: spec.Message), Op.Of(name: nameof(Message)).InvalidInput()).ToFin()
                .Map(_ => global::Rhino.UI.Dialogs.ShowMessage(parent: scope.Parent, message: spec.Message, title: spec.Title, buttons: spec.Buttons, icon: spec.Icon, defaultButton: spec.DefaultButton, options: spec.Options, mode: spec.Mode)));

    public static UiIntent<Unit> Text(string body, string title = "") =>
        Request(
            name: nameof(Text),
            run: _ => guard(!string.IsNullOrWhiteSpace(value: body), Op.Of(name: nameof(Text)).InvalidInput()).ToFin()
                .Bind(_ => Op.Of(name: nameof(Text)).Catch(() => {
                    global::Rhino.UI.Dialogs.ShowTextDialog(message: body, title: title ?? string.Empty);
                    return Fin.Succ(value: unit);
                })));
    public static UiIntent<T> Choice<T>(string title, string message, IEnumerable<T> items, Option<T> selected = default) =>
        Request(name: nameof(Choice), run: _ => Items(values: items, name: nameof(Choice))
            .Bind(choices => Picked(selected.Case switch {
                T current => global::Rhino.UI.Dialogs.ShowListBox(title: title, message: message, items: choices.ToList(), selectedItem: current),
                _ => global::Rhino.UI.Dialogs.ShowListBox(title: title, message: message, items: choices.ToList()),
            }).Bind(result => result is T typed ? Fin.Succ(value: typed) : Fin.Fail<T>(error: Op.Of(name: nameof(Choice)).InvalidResult()))));

    public static UiIntent<Seq<string>> MultiChoice(string title, string message, IEnumerable<string> items, IEnumerable<string>? selected = null) =>
        Request(name: nameof(MultiChoice), run: _ => Items(values: items, name: nameof(MultiChoice))
            .Bind(values => Picked(global::Rhino.UI.Dialogs.ShowMultiListBox(
                title: title,
                message: message,
                items: [.. values],
                defaults: [.. Optional(selected).Map(static current => toSeq(current)).IfNone(Seq<string>())])).Map(static result => toSeq(result))));

    public static UiIntent<Seq<bool>> Checklist(string title, string message, IEnumerable<string> items, IEnumerable<bool> states) =>
        Request(name: nameof(Checklist), run: _ => (Items(values: items, name: nameof(Checklist)), Op.Of(name: nameof(Checklist)).Need(states).Map(static source => toSeq(source))).Apply(static (values, checks) => (Values: values, Checks: checks)).As().Bind(values => values switch {
            (Seq<string> choices, Seq<bool> checks) when !choices.IsEmpty && choices.Count == checks.Count => Picked(global::Rhino.UI.Dialogs.ShowCheckListBox(title: title, message: message, items: (string[])[.. choices], checkState: (bool[])[.. checks])).Map(static result => toSeq(result)),
            _ => Fin.Fail<Seq<bool>>(error: Op.Of(name: nameof(Checklist)).InvalidInput()),
        }));

    public static UiIntent<Seq<(string Name, string Value)>> Properties(string title, string message, IEnumerable<(string Name, string Value)> values) =>
        Request(name: nameof(Properties), run: _ => Items(values: values, name: nameof(Properties)).Bind(items =>
                Picked(global::Rhino.UI.Dialogs.ShowPropertyListBox(title: title, message: message, items: (string[])[.. items.Map(static item => item.Name)], values: (string[])[.. items.Map(static item => item.Value)]))
                .Bind(result => toSeq(result) switch {
                    Seq<string> updated when updated.Count == items.Count => Fin.Succ(value: items.Zip(updated).Map(static pair => (pair.First.Name, pair.Second))),
                    _ => Fin.Fail<Seq<(string Name, string Value)>>(error: Op.Of(name: nameof(Properties)).InvalidResult()),
                })));

    public static UiIntent<UiLayerResult> Layer(UiLayerRequest request) =>
        Request(name: nameof(Layer), run: scope => request.Switch(scope,
            one: static (sc, single) => {
                bool setCurrent = single.InitialSetCurrent;
                Seq<int> selected = single.Selected.IfNone(Seq<int>());
                int index = selected.IsEmpty ? -1 : selected[0];
                return selected.Count switch {
                    > 1 => Fin.Fail<UiLayerResult>(error: Op.Of(name: nameof(Layer)).InvalidInput()),
                    _ => global::Rhino.UI.Dialogs.ShowSelectLayerDialog(
                            layerIndex: ref index, dialogTitle: single.Title,
                            showNewLayerButton: single.ShowNewLayer,
                            showSetCurrentButton: single.ShowSetCurrent,
                            initialSetCurrentState: ref setCurrent) switch {
                                true => LayerIndices(values: Seq(index)).Map(indices => new UiLayerResult(LayerIndices: indices, SetCurrent: setCurrent, MaterialAccepted: false)),
                                false => Fin.Fail<UiLayerResult>(error: new Fault.Cancelled()),
                            },
                };
            },
            multiple: static (_, multiple) => MultipleLayers(request: multiple).Map(static indices => new UiLayerResult(LayerIndices: indices, SetCurrent: false, MaterialAccepted: false)),
            material: static (sc, material) => MultipleLayers(request: material).Bind(indices => Picked(global::Rhino.UI.Dialogs.ShowLayerMaterialDialog(doc: sc.Document, layerIndices: indices.AsIterable()), new UiLayerResult(LayerIndices: indices, SetCurrent: false, MaterialAccepted: true)))));

    public static UiIntent<Guid> Linetype(string title, string message, Option<Guid> selected = default) =>
        Request(name: nameof(Linetype), run: scope =>
            global::Rhino.UI.Dialogs.ShowLineTypes(title: title, message: message, doc: scope.Document, selectedLineTypeId: selected.IfNone(Guid.Empty)) switch {
                Guid id when id != Guid.Empty => Fin.Succ(value: id),   // Guid.Empty is the no-preselection sentinel AND the cancel signal
                _ => Fin.Fail<Guid>(error: new Fault.Cancelled()),
            });

    public static UiIntent<int> LinetypeIndex(int selectedIndex = -1, bool displayByLayer = false) =>
        Request(name: nameof(LinetypeIndex), run: _ => {
            int index = selectedIndex;
            return Picked(global::Rhino.UI.Dialogs.ShowSelectLinetypeDialog(linetypeIndex: ref index, displayByLayer: displayByLayer), index);
        });

    public static UiIntent<int> ContextMenu(IEnumerable<string> items, System.Drawing.Point screenPoint, IEnumerable<int>? modes = null) =>
        Request(name: nameof(ContextMenu), run: _ => Items(values: items, name: nameof(ContextMenu)).Bind(menuItems => (menuItems, Optional(modes).Map(static value => toSeq(value)).IfNone(Seq<int>())) switch {
            (Seq<string> entries, Seq<int> flags) when !flags.IsEmpty && flags.Count != entries.Count => Fin.Fail<int>(error: Op.Of(name: nameof(ContextMenu)).InvalidInput()),
            (Seq<string> entries, Seq<int> flags) => global::Rhino.UI.Dialogs.ShowContextMenu(items: entries.AsIterable(), screenPoint: screenPoint, modes: (flags.IsEmpty ? entries.Map(static _ => 0) : flags).AsIterable()) switch {   // mode 0 = enabled (1 = disabled, 2 = separator) — default-1 rendered every no-modes item unclickable
                int index when index >= 0 => Fin.Succ(value: index),
                _ => Fin.Fail<int>(error: new Fault.Cancelled()),
            },
        }));

    public static UiIntent<string> Edit(string title, string message, string value = "", bool expanded = false) =>
        Request(name: nameof(Edit), run: _ => Picked(global::Rhino.UI.Dialogs.ShowEditBox(title: title, message: message, defaultText: value, multiline: expanded, text: out string result), result));

    public static UiIntent<double> Number(string title, string message, double value = 0.0, Option<(double Lower, double Upper)> bounds = default) =>
        Request(name: nameof(Number), run: _ => bounds.Case switch {
            // native min/max constrain the spinbox, not the initial display — clamp the seed (always >= lower post-clamp) instead of rejecting an out-of-bounds value
            (double lower, double upper) when lower <= upper && (value = Math.Clamp(value, lower, upper)) >= lower =>
                Picked(global::Rhino.UI.Dialogs.ShowNumberBox(title: title, message: message, number: ref value, minimum: lower, maximum: upper), value),
            (double, double) => Fin.Fail<double>(error: Op.Of(name: nameof(Number)).InvalidInput()),
            _ => Picked(global::Rhino.UI.Dialogs.ShowNumberBox(title: title, message: message, number: ref value), value),
        });

    public static UiIntent<double> PrintWidth(string title, string message, Option<double> selected = default) =>
        Request(name: nameof(PrintWidth), run: _ => {
            double width = selected.Case switch {
                double value => global::Rhino.UI.Dialogs.ShowPrintWidths(title: title, message: message, selectedWidth: value),
                _ => global::Rhino.UI.Dialogs.ShowPrintWidths(title: title, message: message),
            };
            return Picked(width >= 0d, width);   // negative/UnsetValue signals cancel
        });

    public static UiIntent<Unit> Sun() =>
        Request(name: nameof(Sun), run: scope => Picked(global::Rhino.UI.Dialogs.ShowSunDialog(sun: scope.Document.Lights.Sun), unit));

    public static UiIntent<DrawingBitmap> Mesh(IEnumerable<Mesh> meshes, DrawingSize size, IEnumerable<DrawingColor>? colors = null) =>
        Of(
            name: nameof(Mesh),
            run: (document, _) =>
                (Op.Of(name: nameof(Mesh)).Need(meshes).Bind(static source => toSeq(source).TraverseM(mesh => Op.Of(name: nameof(Mesh)).Need(mesh)).As()),
                 Optional(colors).Map(static source => toSeq(source)).IfNone(Seq<DrawingColor>()).TraverseM(static color => guard(!color.IsEmpty, Op.Of(name: nameof(Mesh)).InvalidInput()).ToFin().Map(_ => color)).As())
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
                    from bitmap in Project(op: Op.Of(name: nameof(Mesh)), native: () => global::Rhino.UI.DrawingUtilities.CreateMeshPreviewImage(doc: document, meshes: values.Meshes, colors: colors, size: size))
                    select bitmap));

    public static UiIntent<Seq<Point2f[]>> Curve(Curve curve, Linetype linetype, DrawingSize size) =>
        Of(
            name: nameof(Curve),
            run: (_, _) =>
                (Op.Of(name: nameof(Curve)).Need(curve),
                 Op.Of(name: nameof(Curve)).Need(linetype))
                .Apply(static (validCurve, validLinetype) => (Curve: validCurve, Linetype: validLinetype))
                .As()
                .Bind(values =>
                    from _ in guard(size.Width > 0 && size.Height > 0, Op.Of(name: nameof(Curve)).InvalidInput())
                    from preview in Project(op: Op.Of(name: nameof(Curve)), native: () => global::Rhino.UI.DrawingUtilities.CreateCurvePreviewGeometry(curve: values.Curve, linetype: values.Linetype, width: size.Width, height: size.Height))
                    select toSeq(preview)));
    public static UiIntent<TOut> Resource<TOut>(string resourceName, DrawingSize size, Assembly assembly, bool scaleDown = false) where TOut : class =>
        Of(
            name: nameof(Resource),
            run: (_, _) =>
                from name in Op.Of(name: nameof(Resource)).AcceptText(value: resourceName).MapFail(_ => Op.Of(name: nameof(Resource)).InvalidInput())
                from validAssembly in Op.Of(name: nameof(Resource)).Need(assembly)
                from _ in guard(size.Width > 0 && size.Height > 0, Op.Of(name: nameof(Resource)).InvalidInput())
                from resource in typeof(TOut) switch {
                    Type t when t == typeof(DrawingBitmap) => Project(op: Op.Of(name: nameof(Resource)), native: () => (TOut?)(object?)(scaleDown
                        ? global::Rhino.UI.DrawingUtilities.LoadBitmapWithScaleDown(iconName: name, sizeDesired: size.Width, assembly: validAssembly)
                        : global::Rhino.UI.DrawingUtilities.BitmapFromIconResource(name, size, validAssembly))),
                    Type t when t == typeof(System.Drawing.Icon) => Project(op: Op.Of(name: nameof(Resource)), native: () => (TOut?)(object?)(scaleDown
                        ? global::Rhino.UI.DrawingUtilities.LoadIconWithScaleDown(iconName: name, sizeDesired: size.Width, assembly: validAssembly)
                        : global::Rhino.UI.DrawingUtilities.IconFromResource(name, size, validAssembly))),
                    Type t when t == typeof(System.Drawing.Image) => Project(op: Op.Of(name: nameof(Resource)), native: () => (TOut?)(object?)global::Rhino.UI.DrawingUtilities.ImageFromResource(name, validAssembly)),
                    _ => Fin.Fail<TOut>(error: Op.Of(name: nameof(Resource)).Unsupported(geometryType: typeof(TOut), outputType: typeof(TOut))),
                }
                select resource);

    public static UiIntent<DrawingBitmap> Svg(string svg, int width, int height) =>
        Of(
            name: nameof(Svg),
            run: (_, _) =>
                from source in Op.Of(name: nameof(Svg)).AcceptText(value: svg).MapFail(_ => Op.Of(name: nameof(Svg)).InvalidInput())
                from _ in guard(width > 0 && height > 0, Op.Of(name: nameof(Svg)).InvalidInput())
                from bitmap in Project(op: Op.Of(name: nameof(Svg)), native: () => global::Rhino.UI.DrawingUtilities.BitmapFromSvg(svg: source, width: width, height: height, adjustForDarkMode: global::Rhino.Runtime.HostUtils.RunningInDarkMode))
                select bitmap);

    public static UiIntent<Seq<global::Rhino.UI.NamedColor>> NamedColors(Option<global::Rhino.UI.NamedColorList> source = default) =>
        Of(
            name: nameof(NamedColors),
            run: (_, _) => Fin.Succ(value: toSeq(source.IfNone(global::Rhino.UI.NamedColorList.Default))));

    public static UiIntent<CaptureResult> CaptureFrame(RhinoView view, CaptureFormat format, CaptureRecipe recipe = default) =>
        Of(name: nameof(CaptureFrame), run: (_, _) => recipe.WithPolicy(
            fallbackDpi: CaptureRecipe.DefaultScreenDpi,
            fallbackDecor: CaptureRecipe.DefaultScreenDecor,
            rewrite: static (decor, _) => decor).Render(
            view: view,
            viewport: recipe.Viewport(view: view),
            project: settings => CaptureCodec.Of(format: format).Render(settings: settings, op: Op.Of(name: nameof(CaptureFrame))),
            op: Op.Of(name: nameof(CaptureFrame))));

    public static UiIntent<Color4f> Color(UiColorSpec spec) =>
        Dialog((parent, _) => Op.Of(name: nameof(Color)).Catch(() => {
            Color4f color = spec.Initial;
            global::Rhino.UI.NamedColorList? named = spec.Named.Case switch { global::Rhino.UI.NamedColorList v => v, _ => null };
            return global::Rhino.UI.Dialogs.ShowColorDialog(parent: parent, color: ref color, allowAlpha: spec.AllowAlpha, namedColorList: named, colorCallback: spec.Changed)
                switch { true => Fin.Succ(value: color), false => Fin.Fail<Color4f>(error: new Fault.Cancelled()) };
        }));

    internal static UiIntent<T> OfScope<T>(Func<RhinoUi.Scope, Fin<T>> run, bool interactive = false, Option<Func<RhinoUi.Scope, Fin<T>>> scripted = default) =>
        new(run: run, interactive: interactive, scripted: scripted);

    internal static UiIntent<T> Dialog<T>(Func<Window?, RhinoDoc, Fin<T>> show) =>
        Request(name: nameof(Dialog), run: scope => Op.Of(name: nameof(Dialog)).Need(show).Bind(valid => valid(arg1: scope.Parent, arg2: scope.Document)));

    internal static UiIntent<Seq<FileEndpoint>> ExchangeFile(FilePrompt prompt) =>
        Request(name: nameof(ExchangeFile), run: scope =>
            from spec in Op.Of(name: nameof(ExchangeFile)).Need(prompt)
            from paths in spec.Mode switch {
                FilePromptMode.Save => new global::Rhino.UI.SaveFileDialog { Title = spec.Title, Filter = spec.Filter, FileName = spec.FileName.IfNone(string.Empty), InitialDirectory = spec.InitialDirectory.IfNone(string.Empty), DefaultExt = spec.DefaultExtension.IfNone(string.Empty) } switch {
                    global::Rhino.UI.SaveFileDialog dialog => Picked(dialog.ShowSaveDialog(), Seq(dialog.FileName)),
                },
                FilePromptMode.OpenSingle or FilePromptMode.OpenMultiple => new global::Rhino.UI.OpenFileDialog { Title = spec.Title, Filter = spec.Filter, FileName = spec.FileName.IfNone(string.Empty), InitialDirectory = spec.InitialDirectory.IfNone(string.Empty), DefaultExt = spec.DefaultExtension.IfNone(string.Empty), MultiSelect = spec.Mode == FilePromptMode.OpenMultiple } switch {
                    global::Rhino.UI.OpenFileDialog dialog => Picked(dialog.ShowOpenDialog(), spec.Mode == FilePromptMode.OpenMultiple ? toSeq(dialog.FileNames) : Seq(dialog.FileName)),
                },
                FilePromptMode.Folder => new SelectFolderDialog { Title = spec.Title, Directory = spec.InitialDirectory.IfNone(string.Empty) } switch {
                    SelectFolderDialog dialog => dialog.ShowDialog(parent: scope.Parent) switch {
                        DialogResult.Ok => Picked(dialog.Directory).Map(static value => Seq(value)),
                        _ => Fin.Fail<Seq<string>>(error: new Fault.Cancelled()),
                    },
                },
                _ => Fin.Fail<Seq<string>>(error: Op.Of(name: nameof(ExchangeFile)).InvalidInput()),
            }
            from endpoints in paths.TraverseM(path => FileEndpoint.From(path: path, name: spec.Name, write: spec.Write)).As()
            select endpoints);

    private static UiIntent<T> Of<T>(string name, Func<RhinoDoc, RunMode, Fin<T>> run, bool interactive = false) {
        Op operation = Op.Of(name: string.IsNullOrWhiteSpace(value: name) ? nameof(UiIntent) : name);
        return OfScope(
            run: scope => operation.Need(run).Bind(valid => valid(arg1: scope.Document, arg2: scope.Mode)),
            interactive: interactive);
    }

    private static Fin<T> Project<T>(Op op, Func<T?> native) where T : class => Optional(native()).ToFin(Fail: op.InvalidResult());

    private static Fin<T> Picked<T>(bool ok, T value) => ok ? Fin.Succ(value: value) : Fin.Fail<T>(error: new Fault.Cancelled());
    private static Fin<T> Picked<T>(T? value) where T : class => Optional(value).ToFin(Fail: new Fault.Cancelled());

    private static UiIntent<T> Request<T>(string name, Func<RhinoUi.Scope, Fin<T>> run, bool interactive = true) =>
        OfScope(
            run: scope => Op.Of(name: name).Need(run).Bind(valid => valid(arg: scope)),
            interactive: interactive);

    private static Fin<Seq<T>> Items<T>(IEnumerable<T> values, string name) =>
        Op.Of(name: name).Need(values).Bind(source => toSeq(source) switch {
            Seq<T> items when !items.IsEmpty => Fin.Succ(value: items),
            _ => Fin.Fail<Seq<T>>(error: Op.Of(name: name).InvalidInput()),
        });

    private static Fin<Seq<int>> MultipleLayers(UiLayerRequest request) =>
        global::Rhino.UI.Dialogs.ShowSelectMultipleLayersDialog(defaultLayerIndices: request.Selected.IfNone(Seq<int>()).AsIterable(), dialogTitle: request.Title, showNewLayerButton: request.ShowNewLayer, layerIndices: out int[] indices) switch {
            true => LayerIndices(values: toSeq(indices)),
            false => Fin.Fail<Seq<int>>(error: new Fault.Cancelled()),
        };

    private static Fin<Seq<int>> LayerIndices(Seq<int> values) =>
        values.Filter(static index => index >= 0).Distinct() switch {
            Seq<int> indices when !indices.IsEmpty => Fin.Succ(value: indices),
            _ => Fin.Fail<Seq<int>>(error: Op.Of(name: nameof(Layer)).InvalidResult()),
        };
}
