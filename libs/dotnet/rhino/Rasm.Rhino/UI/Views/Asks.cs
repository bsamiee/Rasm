using System.Numerics;
using Eto.Forms;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Numeric;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Views;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Captioned<T>(T Value, LocalizeStringPair Caption) where T : notnull {
    public override string ToString() => Caption.Local;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Suppressible<T> {
    public sealed record Shown(T Value) : Suppressible<T>;

    public sealed record Suppressed : Suppressible<T>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostDialogs {
    // --- [ATTENDANCE]
    public static readonly IO<Unit> Attended =
        IO.lift(static () => guard(!RhinoApp.IsRunningUnattended, Errors.Cancelled).ToFin());

    private static Fin<A> Accepted<A>(bool accepted, A answer) =>
        guard(accepted, Errors.Cancelled).ToFin().Map(_ => answer);

    // --- [MESSAGES]
    public static IO<ShowMessageResult> ShowMessage(Option<RhinoDoc> doc, LocalizeStringPair message, LocalizeStringPair title, ShowMessageButton buttons, ShowMessageIcon icon, ShowMessageDefaultButton defaultButton, ShowMessageOptions options, ShowMessageMode mode) =>
        IO.lift(() => Dialogs.ShowMessage(RhinoEtoApp.MainWindowForDocument(doc.ValueUnsafe()), message.Local, title.Local, buttons, icon, defaultButton, options, mode));

    public static IO<Unit> ShowTextDialog(LocalizeStringPair title, string text) =>
        Attended.Bind(_ => IO.lift(() => Dialogs.ShowTextDialog(text, title.Local)));

    // --- [LISTS]
    public static IO<T> ShowListBox<T>(LocalizeStringPair title, LocalizeStringPair message, IterableNE<Captioned<T>> items, Option<Captioned<T>> selected) where T : notnull =>
        IO.lift(() => Optional((Captioned<T>?)Dialogs.ShowListBox(title.Local, message.Local, items.ToArray(), selected.ValueUnsafe()))
            .Map(static picked => picked.Value)
            .ToFin(Errors.Cancelled));

    public static IO<T> ShowComboListBox<T>(LocalizeStringPair title, LocalizeStringPair message, IterableNE<Captioned<T>> items) where T : notnull =>
        IO.lift(() => Optional((Captioned<T>?)Dialogs.ShowComboListBox(title.Local, message.Local, items.ToArray()))
            .Map(static picked => picked.Value)
            .ToFin(Errors.Cancelled));

    public static IO<Seq<(T Item, bool Checked)>> ShowCheckListBox<T>(LocalizeStringPair title, LocalizeStringPair message, Seq<(Captioned<T> Row, bool Checked)> rows) where T : notnull =>
        IO.lift(() => Optional(Dialogs.ShowCheckListBox(title.Local, message.Local, rows.Map(static row => row.Row).ToArray(), rows.Map(static row => row.Checked).ToArray()))
            .Map(states => rows.Zip(toSeq(states), static (row, state) => (Item: row.Row.Value, Checked: state)).Strict())
            .ToFin(Errors.Cancelled));

    public static IO<Seq<(LocalizeStringPair Name, string Value)>> ShowPropertyListBox(LocalizeStringPair title, LocalizeStringPair message, Seq<(LocalizeStringPair Name, string Value)> rows) =>
        IO.lift(() => Optional(Dialogs.ShowPropertyListBox(title.Local, message.Local, [.. rows.Map(static row => new KeyValuePair<string, string>(row.Name.Local, row.Value))]))
            .Map(values => rows.Zip(toSeq(values), static (row, value) => (row.Name, Value: value)).Strict())
            .ToFin(Errors.Cancelled));

    // --- [VALUES]
    public static IO<(TValue Min, TValue Max)> ShowRangeDialog<TValue, TError>(RhinoDoc doc, LocalizeStringPair title, (TValue Min, TValue Max) range, (bool Min, bool Max) editable)
        where TValue : IObjectFactory<TValue, Length, TError>, IConvertible<Length>
        where TError : Error, IValidationError<TError> =>
        from display in DistanceDisplay.Read(doc, modelUnits: true)
        from modelUnit in IO.lift(() => doc.ModelUnits)
        from chosen in Ranged(doc, title, range, editable, display.Precision,
            value => Quantities.As(value.ToValue(), modelUnit),
            number => Conversions.Validated<TValue, Length, TError>(Quantities.From(number, modelUnit)))
        select chosen;

    public static IO<(TValue Min, TValue Max)> ShowRangeDialog<TValue, TKey, TError>(RhinoDoc doc, LocalizeStringPair title, Presentation<TValue, TKey> presentation, (TValue Min, TValue Max) range, (bool Min, bool Max) editable)
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IMinMaxValue<TValue>
        where TKey : struct, INumber<TKey>
        where TError : Error, IValidationError<TError> =>
        Quantities.Scalar(presentation) switch {
            var display => Ranged(doc, title, range, editable, display.Decimals,
                value => display.Shown(double.CreateSaturating(value.ToValue())),
                number => Conversions.Validated<TValue, TKey, TError>(NumericRows.Narrowed<TKey>(display.Key(number)))),
        };

    public static IO<T> ShowEditBox<T, TError>(LocalizeStringPair title, LocalizeStringPair message, Option<T> initial)
        where T : IObjectFactory<T, string, TError>, IConvertible<string>
        where TError : Error, IValidationError<TError> =>
        IO.lift(() => Accepted(Dialogs.ShowEditBox(title.Local, message.Local, Conversions.Unset(initial.Map(static held => held.ToValue())), multiline: true, out string text), text)
            .Bind(Conversions.Validated<T, string, TError>));

    public static IO<PlotWeight> ShowPrintWidths(LocalizeStringPair title, LocalizeStringPair message, Option<PlotWeight> selected) =>
        IO.lift(() => Conversions.Present(selected.Match(
                Some: weight => Dialogs.ShowPrintWidths(title.Local, message.Local, weight.ToHost()),
                None: () => Dialogs.ShowPrintWidths(title.Local, message.Local)))
            .Map(PlotWeight.FromHost)
            .ToFin(Errors.Cancelled));

    public static IO<Color4f> ShowColorDialog(RhinoDoc doc, Color4f initial, bool allowAlpha, Option<NamedColorList> palette, Option<(Func<Color4f, IO<Unit>> Deliver, CallbackSite Site)> preview) =>
        from attended in Attended
        from chosen in IO.lift(() => {
            Color4f color = initial;
            return Accepted(
                Dialogs.ShowColorDialog(RhinoEtoApp.MainWindowForDocument(doc), ref color, allowAlpha, palette.ValueUnsafe(), preview.Map(Previewed).ValueUnsafe()),
                color);
        })
        select chosen;

    public static IO<Unit> ShowSunDialog(RhinoDoc doc) =>
        Attended.Bind(_ => IO.lift(() => Accepted(Dialogs.ShowSunDialog(doc.Lights.Sun), unit)));

    private static Dialogs.OnColorChangedEvent Previewed((Func<Color4f, IO<Unit>> Deliver, CallbackSite Site) preview) =>
        shown => _ = Callbacks.Answer(preview.Deliver(shown), static () => unit, preview.Site);

    private static IO<(TValue Min, TValue Max)> Ranged<TValue>(RhinoDoc doc, LocalizeStringPair title, (TValue Min, TValue Max) range, (bool Min, bool Max) editable, int decimals, Func<TValue, double> shown, Func<double, Fin<TValue>> entered) =>
        from attended in Attended
        from ends in use(() => new RangeDialog(shown(range.Min), shown(range.Max), decimals, decimals, editable.Min, editable.Max) { Title = title.Local })
            .Bind(dialog => IO.lift(() => Accepted(dialog.ShowSemiModal(doc, RhinoEtoApp.MainWindowForDocument(doc)), (dialog.Min, dialog.Max))))
            .Bracket()
        from chosen in IO.lift((entered(ends.Min).ToValidation(), entered(ends.Max).ToValidation()).Apply(static (low, high) => (Min: low, Max: high)).As().ToFin())
        select chosen;

    // --- [TABLES]
    public static IO<(ComponentRef<Layer> Layer, bool MadeCurrent)> ShowSelectLayerDialog(RhinoDoc doc, ComponentRef<Layer> initial, LocalizeStringPair title, bool showNewLayerButton, bool showSetCurrentButton, bool initialSetCurrentState) =>
        from attended in Attended
        from start in TableOps.Find(doc.Layers, initial, includeDeleted: false)
        from chosen in IO.lift(() => {
            int index = start.Index;
            bool current = initialSetCurrentState;
            return Accepted<(ComponentRef<Layer> Layer, bool MadeCurrent)>(
                Dialogs.ShowSelectLayerDialog(ref index, title.Local, showNewLayerButton, showSetCurrentButton, ref current),
                (new ComponentRef<Layer>.ByIndex(index), current));
        })
        select chosen;

    public static IO<Seq<ComponentRef<Layer>>> ShowSelectMultipleLayersDialog(RhinoDoc doc, Seq<ComponentRef<Layer>> defaults, LocalizeStringPair title, bool showNewLayerButton) =>
        from attended in Attended
        from indices in Indices(doc, defaults)
        from chosen in IO.lift(() => Accepted(
            Dialogs.ShowSelectMultipleLayersDialog(indices, title.Local, showNewLayerButton, out int[] layerIndices),
            toSeq(layerIndices).Map<ComponentRef<Layer>>(static index => new ComponentRef<Layer>.ByIndex(index)).Strict()))
        select chosen;

    public static IO<Unit> ShowLayerMaterialDialog(RhinoDoc doc, Seq<ComponentRef<Layer>> layers) =>
        from attended in Attended
        from indices in Indices(doc, layers)
        from shown in IO.lift(() => Accepted(Dialogs.ShowLayerMaterialDialog(doc, indices), unit))
        select shown;

    public static IO<ComponentRef<Linetype>> ShowLineTypes(RhinoDoc doc, LocalizeStringPair title, LocalizeStringPair message, Option<ComponentRef<Linetype>> selected) =>
        from current in selected.TraverseM(linetype => TableOps.Find(doc.Linetypes, linetype, includeDeleted: false).Map(static found => found.Id)).As()
        from chosen in IO.lift(() => Conversions.Present(Dialogs.ShowLineTypes(title.Local, message.Local, doc, Conversions.Unset(current)))
            .Map<ComponentRef<Linetype>>(static id => new ComponentRef<Linetype>.ById(id))
            .ToFin(Errors.Cancelled))
        select chosen;

    public static IO<LinetypeRef> ShowSelectLinetypeDialog(RhinoDoc doc, LinetypeRef initial, bool displayByLayer) =>
        from attended in Attended
        from index in initial.Resolve(doc)
        from chosen in IO.lift(() => {
            int selected = index;
            return Accepted(Dialogs.ShowSelectLinetypeDialog(ref selected, displayByLayer), selected)
                .Bind(static answer => LinetypeRef.FromHost(answer).ToFin(new InvalidAnswer(nameof(Dialogs.ShowSelectLinetypeDialog))));
        })
        select chosen;

    private static IO<Seq<int>> Indices(RhinoDoc doc, Seq<ComponentRef<Layer>> layers) =>
        layers.TraverseM(layer => TableOps.Find(doc.Layers, layer, includeDeleted: false).Map(static found => found.Index)).As();

    // --- [FILES]
    public static IO<Seq<string>> ShowOpenDialog(global::Rhino.UI.OpenFileDialog dialog) =>
        Attended.Bind(_ => IO.lift(() => Accepted(dialog.ShowOpenDialog(), toSeq(dialog.FileNames))));

    public static IO<string> ShowSaveDialog(global::Rhino.UI.SaveFileDialog dialog) =>
        Attended.Bind(_ => IO.lift(() => Accepted(dialog.ShowSaveDialog(), dialog.FileName)));

    public static string Filter(Seq<FileTypeRow> rows) =>
        string.Join('|', rows.Map(static row => row.Description + "|" + string.Join(';', row.Extensions.Map(static extension => "*" + extension))));

    public static IO<string> ShowSelectFolderDialog(RhinoDoc doc, LocalizeStringPair title, Option<string> directory) =>
        from attended in Attended
        from folder in (
            from dialog in use(() => new SelectFolderDialog { Title = title.Local })
            from seeded in IO.lift(() => directory.Iter(path => dialog.Directory = path))
            from chosen in IO.lift(() => Accepted(dialog.ShowDialog(RhinoEtoApp.MainWindowForDocument(doc)) == DialogResult.Ok, dialog.Directory))
            select chosen).Bracket()
        select folder;

    // --- [SUPPRESSION]
    public static Seq<PlugInSetting<bool, bool, InvalidRhinoValue>> Suppressions(ViewCollection views) =>
        views.Listed<View.Dialog>().Choose(static dialog => dialog.Suppression);

    public static IO<Suppressible<T>> Suppressing<T>(SettingsNode node, PlugInSetting<bool, bool, InvalidRhinoValue> row, Func<CheckBox, IO<(T Value, bool Suppress)>> show) =>
        from shows in PlugInSettings.Current(node, row)
        from asked in shows
            ? (from box in use(static () => new CheckBox { Text = RowText.Localize("Do not show this dialog again").Local })
               from answer in show(box)
               from kept in when(answer.Suppress, PlugInSettings.Store(node, row).Write(Some(value: false)).Map(static _ => unit)).As()
               select (Suppressible<T>)new Suppressible<T>.Shown(answer.Value)).Bracket()
            : IO.pure<Suppressible<T>>(new Suppressible<T>.Suppressed())
        select asked;

    public static IO<Unit> ResetMessageBoxes(ViewCollection views, SettingsNode node) =>
        ValueStore.Commit(Suppressions(views).Map(row => PlugInSettings.Store(node, row).Edit(None)));
}
