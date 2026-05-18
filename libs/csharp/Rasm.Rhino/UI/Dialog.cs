using Eto.Forms;
using ColorChangedEvent = global::Rhino.UI.Dialogs.OnColorChangedEvent;
using DrawingColor = System.Drawing.Color;
using UiDialogs = global::Rhino.UI.Dialogs;

namespace Rasm.Rhino.UI;

// --- [MODELS] ---------------------------------------------------------------------------
public static class UiDialog {
    public static UiDialog<T> Eto<T>(Dialog<T> dialog, bool semiModal = false) =>
        UiDialog<T>.Eto(dialog: dialog, semiModal: semiModal);

    public static UiDialog<global::Rhino.UI.ShowMessageResult> Message(
        string message,
        string title,
        global::Rhino.UI.ShowMessageButton buttons = global::Rhino.UI.ShowMessageButton.OK,
        global::Rhino.UI.ShowMessageIcon icon = global::Rhino.UI.ShowMessageIcon.Information,
        global::Rhino.UI.ShowMessageDefaultButton defaultButton = global::Rhino.UI.ShowMessageDefaultButton.Button1,
        global::Rhino.UI.ShowMessageOptions options = global::Rhino.UI.ShowMessageOptions.None,
        global::Rhino.UI.ShowMessageMode mode = global::Rhino.UI.ShowMessageMode.ApplicationModal) =>
        UiDialog<global::Rhino.UI.ShowMessageResult>.Message(
            message: message,
            title: title,
            buttons: buttons,
            icon: icon,
            defaultButton: defaultButton,
            options: options,
            mode: mode);

    public static UiDialog<DrawingColor> Color(
        DrawingColor initial,
        bool includeButtonColors = false,
        string title = "",
        global::Rhino.UI.NamedColorList? colors = null) =>
        UiDialog<DrawingColor>.Color(initial: initial, includeButtonColors: includeButtonColors, title: title, colors: colors);

    public static UiDialog<Color4f> Color(
        Color4f initial,
        bool allowAlpha = false,
        global::Rhino.UI.NamedColorList? colors = null,
        ColorChangedEvent? changed = null) =>
        UiDialog<Color4f>.Color(initial: initial, allowAlpha: allowAlpha, colors: colors, changed: changed);

    public static UiDialog<string> OpenFile(string title = "", string filter = "", string directory = "", string extension = "") =>
        UiDialog<string>.OpenFile(title: title, filter: filter, directory: directory, extension: extension);

    public static UiDialog<Seq<string>> OpenFiles(string title = "", string filter = "", string directory = "", string extension = "") =>
        UiDialog<Seq<string>>.OpenFiles(title: title, filter: filter, directory: directory, extension: extension);

    public static UiDialog<string> SaveFile(string title = "", string filter = "", string directory = "", string extension = "") =>
        UiDialog<string>.SaveFile(title: title, filter: filter, directory: directory, extension: extension);
}

public abstract partial record UiDialog<T> {
    private UiDialog() { }

    internal static UiDialog<T> Eto(Dialog<T> dialog, bool semiModal = false) =>
        new EtoCase(Dialog: dialog, SemiModal: semiModal);

    internal static UiDialog<global::Rhino.UI.ShowMessageResult> Message(
        string message,
        string title,
        global::Rhino.UI.ShowMessageButton buttons = global::Rhino.UI.ShowMessageButton.OK,
        global::Rhino.UI.ShowMessageIcon icon = global::Rhino.UI.ShowMessageIcon.Information,
        global::Rhino.UI.ShowMessageDefaultButton defaultButton = global::Rhino.UI.ShowMessageDefaultButton.Button1,
        global::Rhino.UI.ShowMessageOptions options = global::Rhino.UI.ShowMessageOptions.None,
        global::Rhino.UI.ShowMessageMode mode = global::Rhino.UI.ShowMessageMode.ApplicationModal) =>
        new MessageCase(
            Text: message,
            Title: title,
            Buttons: buttons,
            Icon: icon,
            DefaultButton: defaultButton,
            Options: options,
            Mode: mode);

    internal static UiDialog<DrawingColor> Color(
        DrawingColor initial,
        bool includeButtonColors,
        string title,
        global::Rhino.UI.NamedColorList? colors) =>
        new ColorCase(Initial: initial, IncludeButtonColors: includeButtonColors, Title: title, Colors: Optional(colors));

    internal static UiDialog<Color4f> Color(
        Color4f initial,
        bool allowAlpha,
        global::Rhino.UI.NamedColorList? colors,
        ColorChangedEvent? changed) =>
        new Color4fCase(Initial: initial, AllowAlpha: allowAlpha, Colors: Optional(colors), Changed: Optional(changed));

    internal static UiDialog<string> OpenFile(string title = "", string filter = "", string directory = "", string extension = "") =>
        new OpenFileCase(Title: title, Filter: filter, Directory: directory, Extension: extension);

    internal static UiDialog<Seq<string>> OpenFiles(string title = "", string filter = "", string directory = "", string extension = "") =>
        new OpenFilesCase(Title: title, Filter: filter, Directory: directory, Extension: extension);

    internal static UiDialog<string> SaveFile(string title = "", string filter = "", string directory = "", string extension = "") =>
        new SaveFileCase(Title: title, Filter: filter, Directory: directory, Extension: extension);

    internal abstract Fin<T> Show(RhinoDoc document);

    private sealed record EtoCase(Dialog<T> Dialog, bool SemiModal) : UiDialog<T> {
        internal override Fin<T> Show(RhinoDoc document) =>
            Optional(Dialog)
                .ToFin(Fail: Op.Of(name: nameof(Eto)).InvalidInput())
                .Map(dialog => SemiModal switch {
                    true => global::Rhino.UI.EtoExtensions.ShowSemiModal(dialog, document, parent: RhinoUi.Parent(document: document)),
                    false => dialog.ShowModal(owner: RhinoUi.Parent(document: document)),
                });
    }

    private sealed record MessageCase(
        string Text,
        string Title,
        global::Rhino.UI.ShowMessageButton Buttons,
        global::Rhino.UI.ShowMessageIcon Icon,
        global::Rhino.UI.ShowMessageDefaultButton DefaultButton,
        global::Rhino.UI.ShowMessageOptions Options,
        global::Rhino.UI.ShowMessageMode Mode) : UiDialog<global::Rhino.UI.ShowMessageResult> {
        internal override Fin<global::Rhino.UI.ShowMessageResult> Show(RhinoDoc document) =>
            Fin.Succ(value: UiDialogs.ShowMessage(
                parent: RhinoUi.Parent(document: document),
                message: Text,
                title: Title,
                buttons: Buttons,
                icon: Icon,
                defaultButton: DefaultButton,
                options: Options,
                mode: Mode));
    }

    private sealed record ColorCase(
        DrawingColor Initial,
        bool IncludeButtonColors,
        string Title,
        Option<global::Rhino.UI.NamedColorList> Colors) : UiDialog<DrawingColor> {
        internal override Fin<DrawingColor> Show(RhinoDoc document) {
            DrawingColor selected = Initial;
            return Pick(selected: ref selected) switch {
                true => Fin.Succ(value: selected),
                false => Fin.Fail<DrawingColor>(error: new Fault.Cancelled()),
            };
        }

        private bool Pick(ref DrawingColor selected) =>
            Colors.Case switch {
                global::Rhino.UI.NamedColorList colors => UiDialogs.ShowColorDialog(
                    color: ref selected,
                    includeButtonColors: IncludeButtonColors,
                    dialogTitle: Title,
                    namedColorList: colors),
                _ => UiDialogs.ShowColorDialog(color: ref selected, includeButtonColors: IncludeButtonColors, dialogTitle: Title),
            };
    }

    private sealed record Color4fCase(
        Color4f Initial,
        bool AllowAlpha,
        Option<global::Rhino.UI.NamedColorList> Colors,
        Option<ColorChangedEvent> Changed) : UiDialog<Color4f> {
        internal override Fin<Color4f> Show(RhinoDoc document) {
            Color4f selected = Initial;
            return Pick(document: document, selected: ref selected) switch {
                true => Fin.Succ(value: selected),
                false => Fin.Fail<Color4f>(error: new Fault.Cancelled()),
            };
        }

        private bool Pick(RhinoDoc document, ref Color4f selected) =>
            (Colors.Case, Changed.Case) switch {
                (global::Rhino.UI.NamedColorList colors, ColorChangedEvent changed) => UiDialogs.ShowColorDialog(
                    parent: RhinoUi.Parent(document: document),
                    color: ref selected,
                    allowAlpha: AllowAlpha,
                    namedColorList: colors,
                    colorCallback: changed),
                (global::Rhino.UI.NamedColorList colors, _) => UiDialogs.ShowColorDialog(
                    parent: RhinoUi.Parent(document: document),
                    color: ref selected,
                    allowAlpha: AllowAlpha,
                    namedColorList: colors,
                    colorCallback: null),
                (_, ColorChangedEvent changed) => UiDialogs.ShowColorDialog(
                    parent: RhinoUi.Parent(document: document),
                    color: ref selected,
                    allowAlpha: AllowAlpha,
                    colorCallback: changed),
                _ => UiDialogs.ShowColorDialog(
                    parent: RhinoUi.Parent(document: document),
                    color: ref selected,
                    allowAlpha: AllowAlpha,
                    colorCallback: null),
            };
    }

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
                true => toSeq(dialog.FileNames) switch {
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
                true => Optional(dialog.FileName).ToFin(Fail: Op.Of(name: nameof(Save)).InvalidResult()),
                false => Fin.Fail<string>(error: new Fault.Cancelled()),
            };
        }
    }

    private sealed record OpenFileCase(string Title, string Filter, string Directory, string Extension) : UiDialog<string> {
        internal override Fin<string> Show(RhinoDoc document) =>
            new FileDialogSpec(Title: Title, Filter: Filter, Directory: Directory, Extension: Extension)
                .Open(multiSelect: false)
                .Bind(names => names.Head.ToFin(Fail: Op.Of(name: nameof(OpenFile)).InvalidResult()));
    }

    private sealed record OpenFilesCase(string Title, string Filter, string Directory, string Extension) : UiDialog<Seq<string>> {
        internal override Fin<Seq<string>> Show(RhinoDoc document) =>
            new FileDialogSpec(Title: Title, Filter: Filter, Directory: Directory, Extension: Extension).Open(multiSelect: true);
    }

    private sealed record SaveFileCase(string Title, string Filter, string Directory, string Extension) : UiDialog<string> {
        internal override Fin<string> Show(RhinoDoc document) =>
            new FileDialogSpec(Title: Title, Filter: Filter, Directory: Directory, Extension: Extension).Save();
    }
}
