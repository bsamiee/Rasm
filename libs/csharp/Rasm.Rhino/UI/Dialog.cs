using Eto.Forms;
using ColorChangedEvent = Rhino.UI.Dialogs.OnColorChangedEvent;
using DrawingColor = System.Drawing.Color;
using UiDialogs = Rhino.UI.Dialogs;

namespace Rasm.Rhino.UI;

// --- [MODELS] ---------------------------------------------------------------------------
public static class UiDialog {
    private enum FileDialogMode { OpenOne, OpenMany, Save }

    public static UiDialog<T> Eto<T>(Dialog<T> dialog, bool semiModal = false) =>
        new EtoCase<T>(Dialog: dialog, SemiModal: semiModal);

    public static UiDialog<Unit> Modeless(Form form) =>
        new ModelessCase(Form: form);

    public static UiDialog<global::Rhino.UI.ShowMessageResult> Message(
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

    public static UiDialog<DrawingColor> Color(
        DrawingColor initial,
        bool includeButtonColors = false,
        string title = "",
        global::Rhino.UI.NamedColorList? colors = null) =>
        new ColorCase(Initial: initial, IncludeButtonColors: includeButtonColors, Title: title, Colors: Optional(colors));

    public static UiDialog<Color4f> Color(
        Color4f initial,
        bool allowAlpha = false,
        global::Rhino.UI.NamedColorList? colors = null,
        ColorChangedEvent? changed = null) =>
        new Color4fCase(Initial: initial, AllowAlpha: allowAlpha, Colors: Optional(colors), Changed: Optional(changed));

    public static UiDialog<string> OpenFile(string title = "", string filter = "", string directory = "", string extension = "") =>
        new FileCase<string>(Mode: FileDialogMode.OpenOne, Spec: new FileDialogSpec(Title: title, Filter: filter, Directory: directory, Extension: extension));

    public static UiDialog<Seq<string>> OpenFiles(string title = "", string filter = "", string directory = "", string extension = "") =>
        new FileCase<Seq<string>>(Mode: FileDialogMode.OpenMany, Spec: new FileDialogSpec(Title: title, Filter: filter, Directory: directory, Extension: extension));

    public static UiDialog<string> SaveFile(string title = "", string filter = "", string directory = "", string extension = "") =>
        new FileCase<string>(Mode: FileDialogMode.Save, Spec: new FileDialogSpec(Title: title, Filter: filter, Directory: directory, Extension: extension));

    private sealed record EtoCase<T>(Dialog<T> Dialog, bool SemiModal) : UiDialog<T> {
        internal override Fin<T> Show(RhinoDoc document) =>
            Optional(Dialog)
                .ToFin(Fail: Op.Of(name: nameof(Eto)).InvalidInput())
                .Map(dialog => SemiModal switch {
                    true => global::Rhino.UI.EtoExtensions.ShowSemiModal(dialog, document, parent: RhinoUi.Parent(document: document)),
                    false => dialog.ShowModal(owner: RhinoUi.Parent(document: document)),
                });
    }

    private sealed record ModelessCase(Form Form) : UiDialog<Unit> {
        internal override Fin<Unit> Show(RhinoDoc document) =>
            Optional(Form)
                .ToFin(Fail: Op.Of(name: nameof(Modeless)).InvalidInput())
                .Map(form => {
                    global::Rhino.UI.EtoExtensions.UseRhinoStyle(form);
                    global::Rhino.UI.EtoExtensions.Show(form, document);
                    return unit;
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

    private sealed record FileCase<T>(FileDialogMode Mode, FileDialogSpec Spec) : UiDialog<T> {
        internal override Fin<T> Show(RhinoDoc document) =>
            Mode switch {
                FileDialogMode.OpenOne => Spec.Open(multiSelect: false)
                    .Bind(names => names.Head.ToFin(Fail: Op.Of(name: nameof(OpenFile)).InvalidResult()))
                    .Bind(Accept<string>),
                FileDialogMode.OpenMany => Spec.Open(multiSelect: true).Bind(Accept<Seq<string>>),
                FileDialogMode.Save => Spec.Save().Bind(Accept<string>),
                _ => Fin.Fail<T>(error: Op.Of(name: nameof(FileCase<T>)).InvalidInput()),
            };

        private static Fin<T> Accept<TSource>(TSource value) =>
            value switch {
                T typed => Fin.Succ(value: typed),
                _ => Fin.Fail<T>(error: Op.Of(name: nameof(FileCase<T>)).InvalidResult()),
            };
    }
}

public abstract partial record UiDialog<T> {
    private protected UiDialog() { }

    internal abstract Fin<T> Show(RhinoDoc document);
}
