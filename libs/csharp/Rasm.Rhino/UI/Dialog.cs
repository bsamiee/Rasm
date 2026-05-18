using Eto.Forms;
using ColorChangedEvent = Rhino.UI.Dialogs.OnColorChangedEvent;
using DrawingColor = System.Drawing.Color;
using DrawingPoint = System.Drawing.Point;
using UiDialogs = Rhino.UI.Dialogs;

namespace Rasm.Rhino.UI;

// --- [MODELS] ---------------------------------------------------------------------------
public sealed record UiDialogIntent<T> {
    private readonly Func<RhinoDoc, Fin<T>> show;

    internal UiDialogIntent(Func<RhinoDoc, Fin<T>> show) => this.show = show;

    internal Fin<T> Show(RhinoDoc document) =>
        Optional(show)
            .ToFin(Fail: Op.Of(name: nameof(UiDialogIntent<T>)).InvalidInput())
            .Bind(run => run(arg: document));
}

public static class UiDialogIntent {
    public static UiDialogIntent<T> Eto<T>(Dialog<T> dialog, bool semiModal = false) =>
        Of(document => Optional(dialog)
            .ToFin(Fail: Op.Of(name: nameof(Eto)).InvalidInput())
            .Map(valid => semiModal switch {
                true => global::Rhino.UI.EtoExtensions.ShowSemiModal(valid, document, parent: RhinoUi.Parent(document: document)),
                false => valid.ShowModal(owner: RhinoUi.Parent(document: document)),
            }));

    public static UiDialogIntent<Unit> Modeless(Form form) =>
        Of(document => Optional(form)
            .ToFin(Fail: Op.Of(name: nameof(Modeless)).InvalidInput())
            .Map(valid => {
                global::Rhino.UI.EtoExtensions.UseRhinoStyle(valid);
                global::Rhino.UI.EtoExtensions.Show(valid, document);
                return unit;
            }));

    public static UiDialogIntent<global::Rhino.UI.ShowMessageResult> Message(
        string message,
        string title,
        global::Rhino.UI.ShowMessageButton buttons = global::Rhino.UI.ShowMessageButton.OK,
        global::Rhino.UI.ShowMessageIcon icon = global::Rhino.UI.ShowMessageIcon.Information,
        global::Rhino.UI.ShowMessageDefaultButton defaultButton = global::Rhino.UI.ShowMessageDefaultButton.Button1,
        global::Rhino.UI.ShowMessageOptions options = global::Rhino.UI.ShowMessageOptions.None,
        global::Rhino.UI.ShowMessageMode mode = global::Rhino.UI.ShowMessageMode.ApplicationModal) =>
        Of(document => Fin.Succ(value: UiDialogs.ShowMessage(
            parent: RhinoUi.Parent(document: document),
            message: message,
            title: title,
            buttons: buttons,
            icon: icon,
            defaultButton: defaultButton,
            options: options,
            mode: mode)));

    public static UiDialogIntent<DrawingColor> Color(
        DrawingColor initial,
        bool includeButtonColors = false,
        string title = "",
        global::Rhino.UI.NamedColorList? colors = null) =>
        Of(_ => {
            DrawingColor selected = initial;
            bool picked = Optional(colors).Case switch {
                global::Rhino.UI.NamedColorList list => UiDialogs.ShowColorDialog(color: ref selected, includeButtonColors: includeButtonColors, dialogTitle: title, namedColorList: list),
                _ => UiDialogs.ShowColorDialog(color: ref selected, includeButtonColors: includeButtonColors, dialogTitle: title),
            };
            return picked switch {
                true => Fin.Succ(value: selected),
                false => Fin.Fail<DrawingColor>(error: new Fault.Cancelled()),
            };
        });

    public static UiDialogIntent<Color4f> Color(
        Color4f initial,
        bool allowAlpha = false,
        global::Rhino.UI.NamedColorList? colors = null,
        ColorChangedEvent? changed = null) =>
        Of(document => {
            Color4f selected = initial;
            bool picked = (Optional(colors).Case, Optional(changed).Case) switch {
                (global::Rhino.UI.NamedColorList list, ColorChangedEvent callback) => UiDialogs.ShowColorDialog(parent: RhinoUi.Parent(document: document), color: ref selected, allowAlpha: allowAlpha, namedColorList: list, colorCallback: callback),
                (global::Rhino.UI.NamedColorList list, _) => UiDialogs.ShowColorDialog(parent: RhinoUi.Parent(document: document), color: ref selected, allowAlpha: allowAlpha, namedColorList: list, colorCallback: null),
                (_, ColorChangedEvent callback) => UiDialogs.ShowColorDialog(parent: RhinoUi.Parent(document: document), color: ref selected, allowAlpha: allowAlpha, colorCallback: callback),
                _ => UiDialogs.ShowColorDialog(parent: RhinoUi.Parent(document: document), color: ref selected, allowAlpha: allowAlpha, colorCallback: null),
            };
            return picked switch {
                true => Fin.Succ(value: selected),
                false => Fin.Fail<Color4f>(error: new Fault.Cancelled()),
            };
        });

    public static UiDialogIntent<string> OpenFile(string title = "", string filter = "", string directory = "", string extension = "") =>
        Of(_ => new FileDialogSpec(Title: title, Filter: filter, Directory: directory, Extension: extension)
            .Open(multiSelect: false)
            .Bind(names => names.Head.ToFin(Fail: Op.Of(name: nameof(OpenFile)).InvalidResult())));

    public static UiDialogIntent<Seq<string>> OpenFiles(string title = "", string filter = "", string directory = "", string extension = "") =>
        Of(_ => new FileDialogSpec(Title: title, Filter: filter, Directory: directory, Extension: extension).Open(multiSelect: true));

    public static UiDialogIntent<string> SaveFile(string title = "", string filter = "", string directory = "", string extension = "") =>
        Of(_ => new FileDialogSpec(Title: title, Filter: filter, Directory: directory, Extension: extension).Save());

    public static UiDialogIntent<string> Choice(string title, string message, IEnumerable<string> items, bool combo = false) =>
        Of(_ => combo switch {
            true => UiDialogs.ShowComboListBox(title: title, message: message, items: Items(items).ToArray()),
            false => UiDialogs.ShowListBox(title: title, message: message, items: Items(items).ToArray()),
        } switch {
            string value => Fin.Succ(value: value),
            object value => Fin.Succ(value: value.ToString() ?? string.Empty),
            _ => Fin.Fail<string>(error: new Fault.Cancelled()),
        });

    public static UiDialogIntent<Seq<bool>> CheckList(string title, string message, IEnumerable<string> items, IEnumerable<bool> checks) =>
        Of(_ => Optional(UiDialogs.ShowCheckListBox(title: title, message: message, items: Items(items).ToArray(), checkState: Bools(checks).ToArray()))
            .ToFin(Fail: new Fault.Cancelled())
            .Map(static values => toSeq(values)));

    public static UiDialogIntent<string> Edit(string title, string message, string text = "", bool multiline = false) =>
        Of(_ => UiDialogs.ShowEditBox(title: title, message: message, defaultText: text, multiline: multiline, text: out string value) switch {
            true => Fin.Succ(value: value),
            false => Fin.Fail<string>(error: new Fault.Cancelled()),
        });

    public static UiDialogIntent<double> Number(string title, string message, double value = 0, Option<double> lower = default, Option<double> upper = default) =>
        Of(_ => {
            double number = value;
            bool picked = (lower.Case, upper.Case) switch {
                (double lo, double hi) => UiDialogs.ShowNumberBox(title: title, message: message, number: ref number, minimum: lo, maximum: hi),
                _ => UiDialogs.ShowNumberBox(title: title, message: message, number: ref number),
            };
            return picked switch {
                true => Fin.Succ(value: number),
                false => Fin.Fail<double>(error: new Fault.Cancelled()),
            };
        });

    public static UiDialogIntent<int> Layer(string title, int index = 0, bool showNew = false) =>
        Of(_ => {
            int layerIndex = index;
            bool current = false;
            return UiDialogs.ShowSelectLayerDialog(layerIndex: ref layerIndex, dialogTitle: title, showNewLayerButton: showNew, showSetCurrentButton: false, initialSetCurrentState: ref current) switch {
                true => Fin.Succ(value: layerIndex),
                false => Fin.Fail<int>(error: new Fault.Cancelled()),
            };
        });

    public static UiDialogIntent<Guid> LineType(string title, string message, Option<Guid> selected = default) =>
        Of(document => selected.Case switch {
            Guid id => UiDialogs.ShowLineTypes(title: title, message: message, doc: document, selectedLineTypeId: id),
            _ => UiDialogs.ShowLineTypes(title: title, message: message, doc: document),
        } switch {
            Guid id when id != Guid.Empty => Fin.Succ(value: id),
            _ => Fin.Fail<Guid>(error: new Fault.Cancelled()),
        });

    public static UiDialogIntent<double> PrintWidth(string title, string message, Option<double> selected = default) =>
        Of(_ => selected.Case switch {
            double width => Fin.Succ(value: UiDialogs.ShowPrintWidths(title: title, message: message, selectedWidth: width)),
            _ => Fin.Succ(value: UiDialogs.ShowPrintWidths(title: title, message: message)),
        });

    public static UiDialogIntent<Seq<string>> PropertyList(string title, string message, IEnumerable<string> names, IEnumerable<string> values) =>
        Of(_ => Optional(UiDialogs.ShowPropertyListBox(title: title, message: message, items: Items(names).ToArray(), values: Items(values).ToArray()))
            .ToFin(Fail: new Fault.Cancelled())
            .Map(static result => toSeq(result)));

    public static UiDialogIntent<int> ContextMenu(IEnumerable<string> items, DrawingPoint screenPoint, IEnumerable<int>? modes = null) =>
        Of(_ => UiDialogs.ShowContextMenu(items: Items(items).AsIterable(), screenPoint: screenPoint, modes: Optional(modes).Map(static values => toSeq(values)).IfNone(Seq<int>()).AsIterable()) switch {
            int index and >= 0 => Fin.Succ(value: index),
            _ => Fin.Fail<int>(error: new Fault.Cancelled()),
        });

    private static UiDialogIntent<T> Of<T>(Func<RhinoDoc, Fin<T>> show) =>
        new(show: show);

    private static Seq<string> Items(IEnumerable<string>? values) =>
        Optional(values).Map(static items => toSeq(items)).IfNone(Seq<string>());

    private static Seq<bool> Bools(IEnumerable<bool>? values) =>
        Optional(values).Map(static items => toSeq(items)).IfNone(Seq<bool>());

    private readonly record struct FileDialogSpec(string Title, string Filter, string Directory, string Extension) {
        internal Fin<Seq<string>> Open(bool multiSelect) {
            global::Rhino.UI.OpenFileDialog dialog = new() {
                Title = Title,
                Filter = Filter,
                InitialDirectory = Directory,
                DefaultExt = Extension,
                MultiSelect = multiSelect,
            };
            return dialog.ShowOpenDialog() switch {
                true => toSeq(dialog.FileNames).Filter(static name => !string.IsNullOrWhiteSpace(value: name)) switch {
                    Seq<string> names when !names.IsEmpty => Fin.Succ(value: names),
                    _ => Fin.Fail<Seq<string>>(error: Op.Of(name: nameof(Open)).InvalidResult()),
                },
                false => Fin.Fail<Seq<string>>(error: new Fault.Cancelled()),
            };
        }

        internal Fin<string> Save() {
            global::Rhino.UI.SaveFileDialog dialog = new() {
                Title = Title,
                Filter = Filter,
                InitialDirectory = Directory,
                DefaultExt = Extension,
            };
            return dialog.ShowSaveDialog() switch {
                true => dialog.FileName switch {
                    string name when !string.IsNullOrWhiteSpace(value: name) => Fin.Succ(value: name),
                    _ => Fin.Fail<string>(error: Op.Of(name: nameof(Save)).InvalidResult()),
                },
                false => Fin.Fail<string>(error: new Fault.Cancelled()),
            };
        }
    }
}
