using System.Reflection;
using Eto.Forms;
using Rasm.Rhino.Exchange;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingColor = System.Drawing.Color;
using DrawingSize = System.Drawing.Size;

namespace Rasm.Rhino.UI;

// --- [TYPES] ------------------------------------------------------------------------------
public enum UiWindowMode { Modal, SemiModal }
public enum UiLayerMode { Single, Multiple, Material }

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record UiIntent<T> {
    private readonly Func<RhinoUi.Scope, Fin<T>> run;

    internal UiIntent(Func<RhinoUi.Scope, Fin<T>> run, bool interactive, Option<Func<RhinoUi.Scope, Fin<T>>> scripted = default) =>
        (this.run, Interactive, Scripted) = (run, interactive, scripted);

    internal bool Interactive { get; }
    internal Option<Func<RhinoUi.Scope, Fin<T>>> Scripted { get; }

    internal Fin<T> Run(RhinoUi.Scope scope) => run(arg: scope);

    public UiIntent<T> WithScripted(Func<RhinoDoc, RunMode, Fin<T>> fallback) =>
        new(
            run: run,
            interactive: Interactive,
            scripted: Optional(fallback).Map<Func<RhinoUi.Scope, Fin<T>>>(project => scope => project(arg1: scope.Document, arg2: scope.Mode)));

    public UiIntent<T> WithScripted(Func<RhinoDoc, Fin<T>> fallback) =>
        WithScripted(fallback: (document, _) => Op.Of(name: nameof(WithScripted)).Need(fallback).Bind(project => project(arg: document)));
}

public readonly record struct UiWindowHandle(uint DocumentSerialNumber, string WindowType, string Title, bool Visible);

public readonly record struct UiLayerSpec(string Title, UiLayerMode Mode = UiLayerMode.Single, Option<Seq<int>> Selected = default, bool ShowNewLayer = false, bool ShowSetCurrent = false, bool InitialSetCurrent = false);
public readonly record struct UiLayerResult(Seq<int> Indices, bool SetCurrent, bool MaterialChanged) {
    public Option<int> Single => Indices.Find(static index => index >= 0);
}
public readonly record struct UiMessageSpec(string Message, string Title, global::Rhino.UI.ShowMessageButton Buttons = default, global::Rhino.UI.ShowMessageIcon Icon = default, global::Rhino.UI.ShowMessageDefaultButton DefaultButton = default, global::Rhino.UI.ShowMessageOptions Options = default, global::Rhino.UI.ShowMessageMode Mode = default);
public readonly record struct UiColorSpec(Color4f Initial, Option<global::Rhino.UI.NamedColorList> Named = default, global::Rhino.UI.Dialogs.OnColorChangedEvent? Changed = null);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class UiIntent {
    public static UiIntent<T> Of<T>(Func<RhinoDoc, RunMode, Fin<T>> run) =>
        new(run: scope => Op.Of(name: nameof(Of)).Need(run).Bind(valid => valid(arg1: scope.Document, arg2: scope.Mode)), interactive: true);

    public static UiIntent<T> Operation<T>(Func<RhinoDoc, RunMode, Fin<T>> run) =>
        OfScope(
            run: scope => Op.Of(name: nameof(Operation)).Need(run).Bind(valid => valid(arg1: scope.Document, arg2: scope.Mode)),
            interactive: false);

    internal static UiIntent<T> OfScope<T>(Func<RhinoUi.Scope, Fin<T>> run, bool interactive = false, Option<Func<RhinoUi.Scope, Fin<T>>> scripted = default) =>
        new(run: run, interactive: interactive, scripted: scripted);

    public static UiIntent<Unit> Enqueue(Action run, string name = nameof(Enqueue)) =>
        OfScope(_ => RhinoUi.Enqueue(run: run, name: name), interactive: false);

    internal static UiIntent<T> Dialog<T>(Func<Window?, RhinoDoc, Fin<T>> show) =>
        Request(name: nameof(Dialog), run: scope => Op.Of(name: nameof(Dialog)).Need(show).Bind(valid => valid(arg1: scope.Parent, arg2: scope.Document)));

    public static UiIntent<T> Window<T>(Dialog<T> dialog, UiWindowMode mode = UiWindowMode.Modal) =>
        Request(name: nameof(Window), run: scope => Op.Of(name: nameof(Window)).Need(dialog).Bind(valid => mode switch {
            UiWindowMode.SemiModal => (valid.DefaultButton, valid.AbortButton) switch {
                (Button, Button) => Fin.Succ(value: global::Rhino.UI.EtoExtensions.ShowSemiModal(valid, scope.Document, parent: scope.Parent)),
                _ => Fin.Fail<T>(error: Op.Of(name: nameof(Window)).InvalidInput()),
            },
            UiWindowMode.Modal => Fin.Succ(value: valid.ShowModal(owner: scope.Parent)),
            _ => Fin.Fail<T>(error: Op.Of(name: nameof(Window)).InvalidInput()),
        }));

    public static UiIntent<UiWindowHandle> Window(Form form, bool restoreLocation = false) =>
        Request(name: nameof(Window), run: scope => Op.Of(name: nameof(Window)).Need(form).Map(valid => {
            global::Rhino.UI.EtoExtensions.UseRhinoStyle(valid);
            _ = Op.SideWhen(restoreLocation, () => global::Rhino.UI.EtoExtensions.LocalizeAndRestore(window: valid));
            global::Rhino.UI.EtoExtensions.Show(valid, scope.Document);
            return new UiWindowHandle(DocumentSerialNumber: scope.Document.RuntimeSerialNumber, WindowType: valid.GetType().FullName ?? valid.GetType().Name, Title: valid.Title ?? string.Empty, Visible: valid.Visible);
        }));

    public static UiIntent<Unit> Status(UiStatus status) =>
        OfScope(_ => status.Apply());

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
            run: scope => string.IsNullOrWhiteSpace(value: spec.Message) switch {
                false => Fin.Succ(value: global::Rhino.UI.Dialogs.ShowMessage(parent: scope.Parent, message: spec.Message, title: spec.Title, buttons: spec.Buttons, icon: spec.Icon, defaultButton: spec.DefaultButton, options: spec.Options, mode: spec.Mode)),
                true => Fin.Fail<global::Rhino.UI.ShowMessageResult>(error: Op.Of(name: nameof(Message)).InvalidInput()),
            });

    public static UiIntent<Unit> Text(string body, string title = "") =>
        Request(
            name: nameof(Text),
            run: _ => string.IsNullOrWhiteSpace(value: body) switch {
                true => Fin.Fail<Unit>(error: Op.Of(name: nameof(Text)).InvalidInput()),
                false => Op.Of(name: nameof(Text)).Catch(() => {
                    global::Rhino.UI.Dialogs.ShowTextDialog(message: body, title: title ?? string.Empty);
                    return Fin.Succ(value: unit);
                }),
            });

    public static UiIntent<T> Choice<T>(string title, string message, IEnumerable<T> items, Option<T> selected = default, bool combo = false) =>
        Request(name: nameof(Choice), run: _ => {
            Fin<Seq<T>> values = Op.Of(name: nameof(Choice)).Need(items)
                .Bind(source => toSeq(source) switch {
                    Seq<T> choices when !choices.IsEmpty => Fin.Succ(value: choices),
                    _ => Fin.Fail<Seq<T>>(error: Op.Of(name: nameof(Choice)).InvalidInput()),
                });
            return values.Bind(choices => Optional((combo, selected.Case) switch {
                (true, _) => global::Rhino.UI.Dialogs.ShowComboListBox(title: title, message: message, items: choices.ToList()),
                (false, T current) => global::Rhino.UI.Dialogs.ShowListBox(title: title, message: message, items: choices.ToList(), selectedItem: current),
                _ => global::Rhino.UI.Dialogs.ShowListBox(title: title, message: message, items: choices.ToList()),
            }).ToFin(Fail: new Fault.Cancelled()).Bind(result => result is T typed ? Fin.Succ(value: typed) : Fin.Fail<T>(error: Op.Of(name: nameof(Choice)).InvalidResult())));
        });

    public static UiIntent<Seq<string>> MultiChoice(string title, string message, IEnumerable<string> items, IEnumerable<string>? selected = null) =>
        Request(name: nameof(MultiChoice), run: _ => Op.Of(name: nameof(MultiChoice)).Need(items).Bind(source => (toSeq(source), Optional(selected).Map(static current => toSeq(current)).IfNone(Seq<string>())) switch {
            (Seq<string> values, _) when values.IsEmpty => Fin.Fail<Seq<string>>(error: Op.Of(name: nameof(MultiChoice)).InvalidInput()),
            (Seq<string> values, Seq<string> defaults) => Optional(global::Rhino.UI.Dialogs.ShowMultiListBox(title: title, message: message, items: [.. values], defaults: [.. defaults])).ToFin(Fail: new Fault.Cancelled()).Map(static result => toSeq(result)),
        }));

    public static UiIntent<Seq<bool>> Checklist(string title, string message, IEnumerable<string> items, IEnumerable<bool> states) =>
        Request(name: nameof(Checklist), run: _ => (Op.Of(name: nameof(Checklist)).Need(items).Map(static source => toSeq(source)), Op.Of(name: nameof(Checklist)).Need(states).Map(static source => toSeq(source))).Apply(static (values, checks) => (Values: values, Checks: checks)).As().Bind(values => values switch {
            (Seq<string> choices, Seq<bool> checks) when !choices.IsEmpty && choices.Count == checks.Count => Optional(global::Rhino.UI.Dialogs.ShowCheckListBox(title: title, message: message, items: (string[])[.. choices], checkState: (bool[])[.. checks])).ToFin(Fail: new Fault.Cancelled()).Map(static result => toSeq(result)),
            _ => Fin.Fail<Seq<bool>>(error: Op.Of(name: nameof(Checklist)).InvalidInput()),
        }));

    public static UiIntent<Seq<(string Name, string Value)>> Properties(string title, string message, IEnumerable<(string Name, string Value)> values) =>
        Request(name: nameof(Properties), run: _ => Op.Of(name: nameof(Properties)).Need(values).Bind(source => toSeq(source) switch {
            Seq<(string Name, string Value)> items when !items.IsEmpty => Optional(global::Rhino.UI.Dialogs.ShowPropertyListBox(title: title, message: message, items: [.. items.Map(static item => new KeyValuePair<string, string>(key: item.Name, value: item.Value))]))
                .ToFin(Fail: new Fault.Cancelled())
                .Bind(result => toSeq(result) switch {
                    Seq<string> updated when updated.Count == items.Count => Fin.Succ(value: items.Zip(updated).Map(static pair => (pair.First.Name, pair.Second))),
                    _ => Fin.Fail<Seq<(string Name, string Value)>>(error: Op.Of(name: nameof(Properties)).InvalidResult()),
                }),
            _ => Fin.Fail<Seq<(string Name, string Value)>>(error: Op.Of(name: nameof(Properties)).InvalidInput()),
        }));

    public static UiIntent<UiLayerResult> Layer(UiLayerSpec spec) =>
        Request(name: nameof(Layer), run: scope => spec.Mode switch {
            UiLayerMode.Single => SingleLayer(spec: spec),
            UiLayerMode.Multiple => MultipleLayers(spec: spec).Map(static indices => new UiLayerResult(Indices: indices, SetCurrent: false, MaterialChanged: false)),
            UiLayerMode.Material => MultipleLayers(spec: spec).Bind(indices => global::Rhino.UI.Dialogs.ShowLayerMaterialDialog(doc: scope.Document, layerIndices: indices.AsIterable()) switch {
                true => Fin.Succ(value: new UiLayerResult(Indices: indices, SetCurrent: false, MaterialChanged: true)),
                false => Fin.Fail<UiLayerResult>(error: new Fault.Cancelled()),
            }),
            _ => Fin.Fail<UiLayerResult>(error: Op.Of(name: nameof(Layer)).InvalidInput()),
        });

    public static UiIntent<Guid> Linetype(string title, string message, Option<Guid> selected = default) =>
        Request(name: nameof(Linetype), run: scope =>
            selected.Case switch {
                Guid id => global::Rhino.UI.Dialogs.ShowLineTypes(title: title, message: message, doc: scope.Document, selectedLineTypeId: id) switch {
                    Guid value when value != Guid.Empty => Fin.Succ(value: value),
                    _ => Fin.Fail<Guid>(error: new Fault.Cancelled()),
                },
                _ => Optional(global::Rhino.UI.Dialogs.ShowLineTypes(title: title, message: message, doc: scope.Document))
                    .ToFin(Fail: new Fault.Cancelled())
                    .Bind(static result => result switch {
                        Guid value when value != Guid.Empty => Fin.Succ(value: value),
                        _ => Fin.Fail<Guid>(error: Op.Of(name: nameof(Linetype)).InvalidResult()),
                    }),
            });

    public static UiIntent<int> LinetypeIndex(int selectedIndex = -1, bool displayByLayer = false) =>
        Request(name: nameof(LinetypeIndex), run: _ => {
            int index = selectedIndex;
            return global::Rhino.UI.Dialogs.ShowSelectLinetypeDialog(linetypeIndex: ref index, displayByLayer: displayByLayer) switch {
                true => Fin.Succ(value: index),
                false => Fin.Fail<int>(error: new Fault.Cancelled()),
            };
        });

    public static UiIntent<int> ContextMenu(IEnumerable<string> items, System.Drawing.Point screenPoint, IEnumerable<int>? modes = null) =>
        Request(name: nameof(ContextMenu), run: _ => Op.Of(name: nameof(ContextMenu)).Need(items).Bind(source => (toSeq(source), Optional(modes).Map(static value => toSeq(value)).IfNone(Seq<int>())) switch {
            (Seq<string> values, _) when values.IsEmpty => Fin.Fail<int>(error: Op.Of(name: nameof(ContextMenu)).InvalidInput()),
            (Seq<string> values, Seq<int> flags) when !flags.IsEmpty && flags.Count != values.Count => Fin.Fail<int>(error: Op.Of(name: nameof(ContextMenu)).InvalidInput()),
            (Seq<string> values, Seq<int> flags) => global::Rhino.UI.Dialogs.ShowContextMenu(items: values.AsIterable(), screenPoint: screenPoint, modes: (flags.IsEmpty ? values.Map(static _ => 1) : flags).AsIterable()) switch {
                int index when index >= 0 => Fin.Succ(value: index),
                _ => Fin.Fail<int>(error: new Fault.Cancelled()),
            },
        }));

    internal static UiIntent<Seq<FileEndpoint>> ExchangeFile(FilePrompt prompt) =>
        Request(name: nameof(ExchangeFile), run: scope =>
            from spec in Op.Of(name: nameof(ExchangeFile)).Need(prompt)
            from paths in spec.Mode switch {
                FilePromptMode.Save => new global::Rhino.UI.SaveFileDialog { Title = spec.Title, Filter = spec.Filter, FileName = spec.FileName.IfNone(string.Empty), InitialDirectory = spec.InitialDirectory.IfNone(string.Empty), DefaultExt = spec.DefaultExtension.IfNone(string.Empty) } switch {
                    global::Rhino.UI.SaveFileDialog dialog => dialog.ShowSaveDialog() switch {
                        true => Fin.Succ(value: Seq(dialog.FileName)),
                        false => Fin.Fail<Seq<string>>(error: new Fault.Cancelled()),
                    },
                },
                FilePromptMode.OpenOne or FilePromptMode.OpenMany => new global::Rhino.UI.OpenFileDialog { Title = spec.Title, Filter = spec.Filter, FileName = spec.FileName.IfNone(string.Empty), InitialDirectory = spec.InitialDirectory.IfNone(string.Empty), DefaultExt = spec.DefaultExtension.IfNone(string.Empty), MultiSelect = spec.Mode == FilePromptMode.OpenMany } switch {
                    global::Rhino.UI.OpenFileDialog dialog => dialog.ShowOpenDialog() switch {
                        true => Fin.Succ(value: spec.Mode == FilePromptMode.OpenMany ? toSeq(dialog.FileNames) : Seq(dialog.FileName)),
                        false => Fin.Fail<Seq<string>>(error: new Fault.Cancelled()),
                    },
                },
                FilePromptMode.Folder => new SelectFolderDialog { Title = spec.Title, Directory = spec.InitialDirectory.IfNone(string.Empty) } switch {
                    SelectFolderDialog dialog => dialog.ShowDialog(parent: scope.Parent) switch {
                        DialogResult.Ok => Optional(dialog.Directory).ToFin(Fail: new Fault.Cancelled()).Map(value => Seq(value)),
                        _ => Fin.Fail<Seq<string>>(error: new Fault.Cancelled()),
                    },
                },
                _ => Fin.Fail<Seq<string>>(error: Op.Of(name: nameof(ExchangeFile)).InvalidInput()),
            }
            from endpoints in paths.TraverseM(path => FileEndpoint.From(path: path, name: spec.Name, write: spec.Write)).As()
            select endpoints);

    public static UiIntent<string> Edit(string title, string message, string value = "", bool expanded = false) =>
        Request(name: nameof(Edit), run: _ => global::Rhino.UI.Dialogs.ShowEditBox(title: title, message: message, defaultText: value, multiline: expanded, text: out string result) switch {
            true => Fin.Succ(value: result),
            false => Fin.Fail<string>(error: new Fault.Cancelled()),
        });

    public static UiIntent<double> Number(string title, string message, double value = 0.0, Option<(double Lower, double Upper)> bounds = default) =>
        Request(name: nameof(Number), run: _ => bounds.Case switch {
            (double lower, double upper) when lower <= upper && value >= lower && value <= upper => global::Rhino.UI.Dialogs.ShowNumberBox(title: title, message: message, number: ref value, minimum: lower, maximum: upper) switch {
                true => value >= lower && value <= upper ? Fin.Succ(value: value) : Fin.Fail<double>(error: Op.Of(name: nameof(Number)).InvalidResult()),
                false => Fin.Fail<double>(error: new Fault.Cancelled()),
            },
            (double, double) => Fin.Fail<double>(error: Op.Of(name: nameof(Number)).InvalidInput()),
            _ => global::Rhino.UI.Dialogs.ShowNumberBox(title: title, message: message, number: ref value) switch {
                true => Fin.Succ(value: value),
                false => Fin.Fail<double>(error: new Fault.Cancelled()),
            },
        });

    private static UiIntent<T> Request<T>(string name, Func<RhinoUi.Scope, Fin<T>> run, bool interactive = true) =>
        OfScope(
            run: scope => Op.Of(name: name).Need(run).Bind(valid => valid(arg: scope)),
            interactive: interactive);

    private static Fin<UiLayerResult> SingleLayer(UiLayerSpec spec) {
        bool setCurrent = spec.InitialSetCurrent;
        Seq<int> selected = spec.Selected.IfNone(Seq<int>());
        int index = selected.IsEmpty ? -1 : selected[0];
        return global::Rhino.UI.Dialogs.ShowSelectLayerDialog(layerIndex: ref index, dialogTitle: spec.Title, showNewLayerButton: spec.ShowNewLayer, showSetCurrentButton: spec.ShowSetCurrent, initialSetCurrentState: ref setCurrent) switch {
            true => LayerIndices(values: Seq(index)).Map(indices => new UiLayerResult(Indices: indices, SetCurrent: setCurrent, MaterialChanged: false)),
            false => Fin.Fail<UiLayerResult>(error: new Fault.Cancelled()),
        };
    }

    private static Fin<Seq<int>> MultipleLayers(UiLayerSpec spec) =>
        global::Rhino.UI.Dialogs.ShowSelectMultipleLayersDialog(defaultLayerIndices: spec.Selected.IfNone(Seq<int>()).AsIterable(), dialogTitle: spec.Title, showNewLayerButton: spec.ShowNewLayer, layerIndices: out int[] indices) switch {
            true => LayerIndices(values: toSeq(indices)),
            false => Fin.Fail<Seq<int>>(error: new Fault.Cancelled()),
        };

    private static Fin<Seq<int>> LayerIndices(Seq<int> values) =>
        values.Filter(static index => index >= 0).Distinct() switch {
            Seq<int> indices when !indices.IsEmpty => Fin.Succ(value: indices),
            _ => Fin.Fail<Seq<int>>(error: Op.Of(name: nameof(Layer)).InvalidResult()),
        };
}

public static class UiPreview {
    public static UiIntent<T> Of<T>(string name, Func<RhinoDoc, RunMode, Fin<T>> run, bool interactive = false) {
        Op operation = Op.Of(name: string.IsNullOrWhiteSpace(value: name) ? nameof(UiPreview) : name);
        return UiIntent.OfScope(
            run: scope => operation.Need(run).Bind(valid => valid(arg1: scope.Document, arg2: scope.Mode)),
            interactive: interactive);
    }

    public static UiIntent<DrawingBitmap> Mesh(IEnumerable<Mesh> meshes, DrawingSize size, IEnumerable<DrawingColor>? colors = null) =>
        Of(
            name: nameof(Mesh),
            run: (document, _) =>
                (Op.Of(name: nameof(Mesh)).Need(meshes).Bind(static source => toSeq(source).TraverseM(mesh => Op.Of(name: nameof(Mesh)).Need(mesh)).As()),
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
                    from preview in Optional(global::Rhino.UI.DrawingUtilities.CreateCurvePreviewGeometry(curve: values.Curve, linetype: values.Linetype, width: size.Width, height: size.Height))
                        .ToFin(Fail: Op.Of(name: nameof(Curve)).InvalidResult())
                    select toSeq(preview)));

    public static UiIntent<DrawingBitmap> Icon(string resourceName, DrawingSize size, Assembly assembly) =>
        Of(
            name: nameof(Icon),
            run: (_, _) =>
                from name in Op.Of(name: nameof(Icon)).AcceptText(value: resourceName).MapFail(_ => Op.Of(name: nameof(Icon)).InvalidInput())
                from validAssembly in Op.Of(name: nameof(Icon)).Need(assembly)
                from _ in guard(size.Width > 0 && size.Height > 0, Op.Of(name: nameof(Icon)).InvalidInput())
                from bitmap in Optional(global::Rhino.UI.DrawingUtilities.BitmapFromIconResource(name, size, validAssembly))
                    .ToFin(Fail: Op.Of(name: nameof(Icon)).InvalidResult())
                select bitmap);

    public static UiIntent<DrawingBitmap> Svg(string svg, int width, int height) =>
        Of(
            name: nameof(Svg),
            run: (_, _) =>
                from source in Op.Of(name: nameof(Svg)).AcceptText(value: svg).MapFail(_ => Op.Of(name: nameof(Svg)).InvalidInput())
                from _ in guard(width > 0 && height > 0, Op.Of(name: nameof(Svg)).InvalidInput())
                from bitmap in Optional(global::Rhino.UI.DrawingUtilities.BitmapFromSvg(svg: source, width: width, height: height, adjustForDarkMode: global::Rhino.Runtime.HostUtils.RunningInDarkMode))
                    .ToFin(Fail: Op.Of(name: nameof(Svg)).InvalidResult())
                select bitmap);

    public static UiIntent<Seq<global::Rhino.UI.NamedColor>> NamedColors(Option<global::Rhino.UI.NamedColorList> source = default) =>
        Of(
            name: nameof(NamedColors),
            run: (_, _) => Fin.Succ(value: toSeq(source.IfNone(global::Rhino.UI.NamedColorList.Default))));

    public static UiIntent<Color4f> Color(UiColorSpec spec, bool allowAlpha = false) =>
        UiIntent.OfScope(
            run: scope => {
                Color4f color = spec.Initial;
                global::Rhino.UI.NamedColorList? named = spec.Named.Case switch {
                    global::Rhino.UI.NamedColorList value => value,
                    _ => null,
                };
                return global::Rhino.UI.Dialogs.ShowColorDialog(parent: scope.Parent, color: ref color, allowAlpha: allowAlpha, namedColorList: named, colorCallback: spec.Changed) switch {
                    true => Fin.Succ(value: color),
                    false => Fin.Fail<Color4f>(error: new Fault.Cancelled()),
                };
            },
            interactive: true);
}
