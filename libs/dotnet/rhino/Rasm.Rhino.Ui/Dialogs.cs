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
        from parent in HostWindows.Parent(doc, None)
        from result in IO.lift(() => Dialogs.ShowMessage(parent, message.Local, title.Local, buttons, icon, defaultButton, options: options, mode: mode))
        select result;

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

    public static IO<Seq<(T Item, bool Checked)>> CheckListBox<T>(LocalizeStringPair title, LocalizeStringPair message, Seq<(T Item, bool Checked)> rows) where T : notnull =>
        IO.lift(() =>
            Optional(Dialogs.ShowCheckListBox(title.Local, message.Local, rows.Map(static row => row.Item).ToArray(), [.. rows.Map(static row => row.Checked)]))
                .Map(states => rows.Zip(toSeq(states), static (row, state) => (row.Item, Checked: state)).Strict())
                .ToFin(new Canceled()));

    public static IO<Seq<(LocalizeStringPair Name, string Value)>> PropertyListBox(LocalizeStringPair title, LocalizeStringPair message, Seq<(LocalizeStringPair Name, string Value)> rows) =>
        IO.lift(() =>
            Optional(Dialogs.ShowPropertyListBox(title.Local, message.Local, [.. rows.Map(static row => new KeyValuePair<string, string>(row.Name.Local, row.Value))]))
                .Map(values => rows.Zip(toSeq(values), static (row, value) => (row.Name, Value: value)).Strict())
                .ToFin(new Canceled()));

    public static IO<T> ComboListBox<T>(LocalizeStringPair title, LocalizeStringPair message, Seq<T> items) where T : notnull =>
        IO.lift(() =>
            Dialogs.ShowComboListBox(title.Local, message.Local, items.ToArray()) is T picked ? picked : Fin.Fail<T>(new Canceled()));

    // --- [VALUES]
    public static IO<(double Min, double Max)> Range(RhinoDoc doc, double min, double max, int decimals, int increment, bool minAdjustable, bool maxAdjustable) =>
        DisposalOps.Using(() => new RangeDialog(min, max, decimals, increment, minAdjustable, maxAdjustable), dialog =>
            HostWindows.ShowModal(doc, None, owner => dialog.ShowSemiModal(doc, owner))
                .Bind(accepted => IO.lift(() => Canceled.Unless(accepted, (dialog.Min, dialog.Max)))));

    public static IO<PlotWeight> PrintWidth(LocalizeStringPair title, LocalizeStringPair message, Option<PlotWeight> selected) =>
        IO.lift(() =>
            selected.Match(Some: weight => Dialogs.ShowPrintWidths(title.Local, message.Local, weight.ToHost()), None: () => Dialogs.ShowPrintWidths(title.Local, message.Local)) switch {
                RhinoMath.UnsetValue => Fin.Fail<PlotWeight>(new Canceled()),
                double width => PlotWeight.FromHost(width),
            });

    public static IO<string> EditBox(LocalizeStringPair title, LocalizeStringPair message, string defaultText, bool multiline) =>
        IO.lift(() => Canceled.Unless(Dialogs.ShowEditBox(title.Local, message.Local, defaultText, multiline, out string text), text));

    public static IO<double> NumberBox(LocalizeStringPair title, LocalizeStringPair message, double initial, Option<(double Minimum, double Maximum)> bounds) =>
        IO.lift(() => {
            double number = initial;
            return Canceled.Unless(
                bounds.Match(
                    Some: limits => Dialogs.ShowNumberBox(title.Local, message.Local, ref number, limits.Minimum, limits.Maximum),
                    None: () => Dialogs.ShowNumberBox(title.Local, message.Local, ref number)),
                number);
        });

    public static IO<Color4f> Color(RhinoDoc doc, Color4f initial, bool allowAlpha, Option<NamedColorList> palette, Option<Func<Color4f, IO<Unit>>> preview, Action<Error> reject) =>
        from parent in HostWindows.Parent(doc, None)
        from chosen in IO.lift(() => {
            Color4f color = initial;
            return Canceled.Unless(
                Dialogs.ShowColorDialog(
                    parent,
                    ref color,
                    allowAlpha,
                    palette.ValueUnsafe(),
                    preview.Map(deliver => new Dialogs.OnColorChangedEvent(shown => _ = Answers.Answer(deliver(shown), reject, unit))).ValueUnsafe()),
                color);
        })
        select chosen;

    public static IO<Unit> Sun(Sun sun) =>
        IO.lift(() => Canceled.Unless(Dialogs.ShowSunDialog(sun), unit));

    // --- [FILES]
    public static IO<Seq<string>> OpenFile(OpenFileDialog dialog) =>
        IO.lift(() => Canceled.Unless(dialog.ShowOpenDialog(), toSeq(dialog.FileNames)));

    public static IO<string> SaveFile(SaveFileDialog dialog) =>
        IO.lift(() => Canceled.Unless(dialog.ShowSaveDialog(), dialog.FileName));

    // --- [TABLES]
    public static IO<(int Index, bool MadeCurrent)> SelectLayer(int initial, LocalizeStringPair title, bool showNewLayer, bool showSetCurrent, bool setCurrentInitial) =>
        IO.lift(() => {
            int index = initial;
            bool current = setCurrentInitial;
            return Canceled.Unless(Dialogs.ShowSelectLayerDialog(ref index, title.Local, showNewLayer, showSetCurrent, ref current), (Index: index, MadeCurrent: current));
        });

    public static IO<Seq<int>> SelectLayers(Seq<int> defaults, LocalizeStringPair title, bool showNewLayer) =>
        IO.lift(() => Canceled.Unless(Dialogs.ShowSelectMultipleLayersDialog(defaults, title.Local, showNewLayer, out int[] indices), toSeq(indices)));

    public static IO<Unit> LayerMaterial(RhinoDoc doc, Seq<int> layerIndices) =>
        IO.lift(() => Canceled.Unless(Dialogs.ShowLayerMaterialDialog(doc, layerIndices), unit));

    public static IO<Guid> LineTypes(RhinoDoc doc, LocalizeStringPair title, LocalizeStringPair message, Option<Guid> selected) =>
        IO.lift(() => Answers.Present(Dialogs.ShowLineTypes(title.Local, message.Local, doc, Answers.Unset(selected))).ToFin(new Canceled()));

    public static IO<LinetypeRef> SelectLinetype(RhinoDoc doc, LinetypeRef initial, bool displayByLayer) =>
        from index in initial.Resolve(doc)
        from chosen in IO.lift(() => {
            int selected = index;
            return Canceled.Unless(Dialogs.ShowSelectLinetypeDialog(ref selected, displayByLayer), selected)
                .Bind(static answer => LinetypeRef.FromHost(answer).ToFin(new InvalidAnswer(nameof(Dialogs.ShowSelectLinetypeDialog))));
        })
        select chosen;
}
