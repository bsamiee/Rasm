using Eto.Forms;
using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.Render;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.Ui;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostDialogs {
    // --- [MESSAGES]
    public static IO<ShowMessageResult> Message(RhinoDoc doc, LocalizeStringPair message, LocalizeStringPair title, ShowMessageButton buttons, ShowMessageIcon icon, ShowMessageDefaultButton defaultButton, ShowMessageOptions options, ShowMessageMode mode) =>
        from parent in HostWindows.Parent(doc, Option<Control>.None)
        from result in IO.lift(() => Dialogs.ShowMessage(parent, message.Local, title.Local, buttons, icon, defaultButton, options: options, mode: mode))
        select result;

    public static IO<Unit> Text(string text, LocalizeStringPair title) =>
        IO.lift(() => Dialogs.ShowTextDialog(text, title.Local));

    // --- [LISTS]
    public static IO<T> ListBox<T>(LocalizeStringPair title, LocalizeStringPair message, Seq<T> items, Option<T> selected) where T : notnull =>
        IO.lift(() => selected.Match(
                Some: chosen => Dialogs.ShowListBox(title.Local, message.Local, items.ToArray(), chosen),
                None: () => Dialogs.ShowListBox(title.Local, message.Local, items.ToArray())) is T picked
            ? picked
            : Fin.Fail<T>(new Canceled()));

    public static IO<Seq<string>> MultiListBox(LocalizeStringPair title, LocalizeStringPair message, Seq<string> items, Seq<string> defaults) =>
        IO.lift(() =>
            Optional(Dialogs.ShowMultiListBox(title.Local, message.Local, [.. items], [.. defaults])).Map(static picked => toSeq(picked)).ToFin(new Canceled()));

    public static IO<Seq<bool>> CheckListBox<T>(LocalizeStringPair title, LocalizeStringPair message, Seq<(T Item, bool Checked)> rows) where T : notnull =>
        IO.lift(() =>
            Optional(Dialogs.ShowCheckListBox(title.Local, message.Local, rows.Map(static row => row.Item).ToArray(), [.. rows.Map(static row => row.Checked)]))
                .Map(static states => toSeq(states))
                .ToFin(new Canceled()));

    public static IO<Seq<string>> PropertyListBox(LocalizeStringPair title, LocalizeStringPair message, Seq<(LocalizeStringPair Name, string Value)> rows) =>
        IO.lift(() =>
            Optional(Dialogs.ShowPropertyListBox(title.Local, message.Local, [.. rows.Map(static row => new KeyValuePair<string, string>(row.Name.Local, row.Value))]))
                .Map(static values => toSeq(values))
                .ToFin(new Canceled()));

    public static IO<T> ComboListBox<T>(LocalizeStringPair title, LocalizeStringPair message, Seq<T> items) where T : notnull =>
        IO.lift(() =>
            Dialogs.ShowComboListBox(title.Local, message.Local, items.ToArray()) is T picked ? picked : Fin.Fail<T>(new Canceled()));

    public static IO<int> ContextMenu(Seq<LocalizeStringPair> items, System.Drawing.Point screenPoint, Seq<int> modes) =>
        IO.lift(() =>
            Answers.Present(Dialogs.ShowContextMenu(items.Map(static item => item.Local), screenPoint, modes)).ToFin(new Canceled()));

    // --- [VALUES]
    public static IO<(double Min, double Max)> Range(RhinoDoc doc, double min, double max, int decimals, int increment, bool minAdjustable, bool maxAdjustable) =>
        Disposal.Using(() => new RangeDialog(min, max, decimals, increment, minAdjustable, maxAdjustable), dialog =>
            from accepted in HostWindows.ShowModal(doc, Option<Control>.None, owner => dialog.ShowSemiModal(doc, owner))
            from answered in IO.lift(() => Canceled.Unless(accepted))
            select (dialog.Min, dialog.Max));

    public static IO<PlotWeight> PrintWidth(LocalizeStringPair title, LocalizeStringPair message, Option<PlotWeight> selected) =>
        IO.lift(() =>
            selected.Match(Some: weight => Dialogs.ShowPrintWidths(title.Local, message.Local, weight.ToHost()), None: () => Dialogs.ShowPrintWidths(title.Local, message.Local)) switch {
                RhinoMath.UnsetValue => Fin.Fail<PlotWeight>(new Canceled()),
                double width => PlotWeight.FromHost(width),
            });

    public static IO<string> EditBox(LocalizeStringPair title, LocalizeStringPair message, string defaultText, bool multiline) =>
        IO.lift(() =>
            Canceled.Unless(Dialogs.ShowEditBox(title.Local, message.Local, defaultText, multiline, out string text)).Map(_ => text));

    public static IO<double> NumberBox(LocalizeStringPair title, LocalizeStringPair message, double initial, Option<(double Minimum, double Maximum)> bounds) =>
        IO.lift(() => {
            double number = initial;
            return Canceled.Unless(bounds.Match(
                    Some: limits => Dialogs.ShowNumberBox(title.Local, message.Local, ref number, limits.Minimum, limits.Maximum),
                    None: () => Dialogs.ShowNumberBox(title.Local, message.Local, ref number)))
                .Map(_ => number);
        });

    public static IO<Color4f> Color(RhinoDoc doc, Color4f initial, bool allowAlpha, Option<NamedColorList> palette, Option<Action<Color4f>> preview) =>
        from parent in HostWindows.Parent(doc, Option<Control>.None)
        from chosen in IO.lift(() => {
            Color4f color = initial;
            return Canceled.Unless(Dialogs.ShowColorDialog(parent, ref color, allowAlpha, palette.ValueUnsafe(), preview.Map<Dialogs.OnColorChangedEvent>(static deliver => deliver.Invoke).ValueUnsafe())).Map(_ => color);
        })
        select chosen;

    public static IO<Unit> Sun(Sun sun) =>
        IO.lift(() => Canceled.Unless(Dialogs.ShowSunDialog(sun)));

    // --- [FILES]
    public static IO<Seq<string>> OpenFile(global::Rhino.UI.OpenFileDialog dialog) =>
        IO.lift(() =>
            Canceled.Unless(dialog.ShowOpenDialog()).Map(_ => dialog.MultiSelect ? toSeq(dialog.FileNames) : Seq(dialog.FileName)));

    public static IO<string> SaveFile(global::Rhino.UI.SaveFileDialog dialog) =>
        IO.lift(() =>
            Canceled.Unless(dialog.ShowSaveDialog()).Map(_ => dialog.FileName));

    // --- [TABLES]
    public static IO<(int Index, bool MadeCurrent)> SelectLayer(RhinoDoc doc, int initial, LocalizeStringPair title, bool showNewLayer, bool showSetCurrent, bool setCurrentInitial) =>
        from supported in IO.lift(() => IndexOutOfRange.Unless(initial, doc.Layers.Count, nameof(Dialogs.ShowSelectLayerDialog)))
        from answer in IO.lift(() => {
            int index = initial;
            bool current = setCurrentInitial;
            return Canceled.Unless(Dialogs.ShowSelectLayerDialog(ref index, title.Local, showNewLayer, showSetCurrent, ref current)).Map(_ => (Index: index, MadeCurrent: current));
        })
        select answer;

    public static IO<Seq<int>> SelectLayers(RhinoDoc doc, Seq<int> defaults, LocalizeStringPair title, bool showNewLayer) =>
        from supported in IO.lift(() => Answers.InRange(defaults, doc.Layers.Count, nameof(Dialogs.ShowSelectMultipleLayersDialog)))
        from picked in IO.lift(() => Canceled.Unless(Dialogs.ShowSelectMultipleLayersDialog(defaults, title.Local, showNewLayer, out int[] indices)).Map(_ => toSeq(indices)))
        select picked;

    public static IO<Unit> LayerMaterial(RhinoDoc doc, Seq<int> layerIndices) =>
        from supported in IO.lift(() => Answers.InRange(layerIndices, doc.Layers.Count, nameof(Dialogs.ShowLayerMaterialDialog)))
        from shown in IO.lift(() => Canceled.Unless(Dialogs.ShowLayerMaterialDialog(doc, layerIndices)))
        select shown;

    public static IO<Guid> LineTypes(RhinoDoc doc, LocalizeStringPair title, LocalizeStringPair message, Option<Guid> selected) =>
        IO.lift(() => Answers.Present(Dialogs.ShowLineTypes(title.Local, message.Local, doc, selected.IfNone(Guid.Empty))).ToFin(new Canceled()));

    public static IO<int> SelectLinetype(RhinoDoc doc, int initial, bool displayByLayer) =>
        from supported in IO.lift(() => unless(displayByLayer && (initial == -1), IndexOutOfRange.Unless(initial, doc.Linetypes.Count, nameof(Dialogs.ShowSelectLinetypeDialog))).As())
        from answer in IO.lift(() => {
            int index = initial;
            return Canceled.Unless(Dialogs.ShowSelectLinetypeDialog(ref index, displayByLayer)).Map(_ => index);
        })
        select answer;
}
