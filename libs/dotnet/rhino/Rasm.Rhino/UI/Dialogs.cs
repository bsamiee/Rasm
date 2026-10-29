using System.Collections.Generic;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Persistence;
using Eto.Drawing;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.UI;
using Rhino.UI.Controls;
using Rhino.UI.Forms;

namespace Rasm.Rhino.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record MenuChoice<T>(ChoiceItem<T> Item) where T : notnull {
    public bool Enabled { get; init; } = true;
    public bool Checked { get; init; }

    internal int Mode => (Checked ? 2 : 0) + (Enabled ? 0 : 1);
}

[SmartEnum]
public sealed partial class FileBrowse {
    public static readonly FileBrowse OpenFile = new(static (title, current, types, _) =>
        HostDialogs.Ask(() => new OpenFileDialog { Title = title.Local, FileName = current, Filter = HostDialogs.Filter(types.Map(static row => (row.Description, row.Extensions))) } switch {
            var dialog => (dialog.ShowOpenDialog(), toSeq(dialog.FileNames).Head),
        }));

    public static readonly FileBrowse SaveFile = new(static (title, current, types, _) =>
        HostDialogs.Ask(() => new SaveFileDialog { Title = title.Local, FileName = current, Filter = HostDialogs.Filter(types.Map(static row => (row.Description, row.Extensions))), DefaultExt = Conversions.Unset(types.Head.Bind(static row => row.Extensions.Head)) } switch {
            var dialog => (dialog.ShowSaveDialog(), Some(dialog.FileName)),
        }));

    public static readonly FileBrowse SelectFolder = new(static (title, current, _, scope) =>
        HostDialogs.ShowSelectFolderDialog(scope.Document, title, Conversions.Present(current)).Map(static picked => Some(picked)));

    [UseDelegateFromConstructor]
    public partial IO<Option<string>> Asked(LocalizeStringPair title, string current, Seq<FileTypeRow> types, RowScope scope);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Suppressible<T> {
    public sealed record Shown(T Value) : Suppressible<T>;

    public sealed record Suppressed : Suppressible<T>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostDialogs {
    // --- [ASKS]
    public static readonly IO<Unit> Attended =
        IO.lift(static () => guard(!RhinoApp.IsRunningUnattended, Errors.Cancelled).ToFin());

    public static IO<T> Ask<T>(Func<Option<T>> show) => Attended.Bind(_ => IO.lift(() => show().ToFin(Errors.Cancelled)));

    public static IO<T> Ask<T>(Func<(bool Accepted, T Answer)> show) => Ask(() => show() switch { var (accepted, answer) => Conversions.Present(accepted, answer) });

    private static Eto.Forms.Window Parent(Option<RhinoDoc> doc) => doc.Map(RhinoEtoApp.MainWindowForDocument).IfNone(static () => RhinoEtoApp.MainWindow);

    // --- [MESSAGES]
    public static IO<ShowMessageResult> ShowMessage(Option<RhinoDoc> doc, LocalizeStringPair message, LocalizeStringPair title, ShowMessageButton buttons, ShowMessageIcon icon, ShowMessageDefaultButton defaultButton, ShowMessageOptions options, ShowMessageMode mode) =>
        Ask(() => (true, Dialogs.ShowMessage(Parent(doc), message.Local, title.Local, buttons, icon, defaultButton, options, mode)));

    // --- [LISTS]
    public static IO<T> ShowListBox<T>(LocalizeStringPair title, LocalizeStringPair message, IterableNE<ChoiceItem<T>> items, Option<ChoiceItem<T>> selected) where T : notnull =>
        Ask(() => Optional(Dialogs.ShowListBox(title.Local, message.Local, items.ToArray(), selected.ValueUnsafe()) as ChoiceItem<T>)).Map(static picked => picked.Value);

    public static IO<T> ShowComboListBox<T>(LocalizeStringPair title, LocalizeStringPair message, IterableNE<ChoiceItem<T>> items) where T : notnull =>
        Ask(() => Optional(Dialogs.ShowComboListBox(title.Local, message.Local, items.ToArray()) as ChoiceItem<T>)).Map(static picked => picked.Value);

    public static IO<Seq<T>> ShowMultiListBox<T>(LocalizeStringPair title, LocalizeStringPair message, IterableNE<ChoiceItem<T>> items, Seq<T> chosen) where T : notnull =>
        Ask(() => Optional(Dialogs.ShowMultiListBox(title.Local, message.Local, [.. items.Map(static item => item.ToString())], [.. toSeq(items).Filter(item => chosen.Exists(held => EqualityComparer<T>.Default.Equals(held, item.Value))).Map(static item => item.ToString())])))
            .Map(picked => toSeq(items).Filter(item => picked.Contains(item.ToString(), StringComparer.Ordinal)).Map(static item => item.Value).Strict());

    public static IO<Seq<(T Item, bool Checked)>> ShowCheckListBox<T>(LocalizeStringPair title, LocalizeStringPair message, Seq<(ChoiceItem<T> Row, bool Checked)> rows) where T : notnull =>
        Ask(() => Optional(Dialogs.ShowCheckListBox(title.Local, message.Local, rows.Map(static row => row.Row).ToArray(), [.. rows.Map(static row => row.Checked)])))
            .Map(states => rows.Zip(toSeq(states), static (row, state) => (Item: row.Row.Value, Checked: state)).Strict());

    public static IO<Seq<(LocalizeStringPair Name, string Value)>> ShowPropertyListBox(LocalizeStringPair title, LocalizeStringPair message, Seq<(LocalizeStringPair Name, string Value)> rows) =>
        Ask(() => Optional(Dialogs.ShowPropertyListBox(title.Local, message.Local, [.. rows.Map(static row => new KeyValuePair<string, string>(row.Name.Local, row.Value))])))
            .Map(values => rows.Zip(toSeq(values), static (row, value) => (row.Name, Value: value)).Strict());

    public static IO<T> ShowContextMenu<T>(Seq<Seq<MenuChoice<T>>> groups, System.Drawing.Point screen) where T : notnull =>
        groups.Map(static group => group.Map(static choice => Some(choice))).Intersperse(Seq(Option<MenuChoice<T>>.None)).Flatten() switch {
            var listed => Ask(() => listed.At(Dialogs.ShowContextMenu(listed.Map(static held => held.Map(static choice => choice.Item.ToString()).IfNone("")), screen, listed.Map(static held => held.Map(static choice => choice.Mode).IfNone(0))))
                .Flatten()
                .Map(static choice => choice.Item.Value)),
        };

    // --- [VALUES]
    public static IO<(Length Min, Length Max)> ShowRangeDialog(RhinoDoc doc, LocalizeStringPair title, (Length Min, Length Max) range, (bool Min, bool Max) editable) =>
        from units in IO.lift(() => Space.Model.Read(doc))
        from chosen in ShowRangeDialog(doc, title, (Quantities.As(range.Min, units.Unit), Quantities.As(range.Max, units.Unit)), editable, units.Precision)
        select (Quantities.From(chosen.Min, units.Unit), Quantities.From(chosen.Max, units.Unit));

    public static IO<(double Min, double Max)> ShowRangeDialog(RhinoDoc doc, LocalizeStringPair title, (double Min, double Max) range, (bool Min, bool Max) editable, int decimals) =>
        (from dialog in use(() => new RangeDialog(range.Min, range.Max, decimals, decimals, editable.Min, editable.Max) { Title = title.Local })
         from chosen in Ask(() => (dialog.ShowSemiModal(doc, RhinoEtoApp.MainWindowForDocument(doc)), (dialog.Min, dialog.Max)))
         select chosen).Bracket();

    public static IO<PlotWeight> ShowPrintWidths(LocalizeStringPair title, LocalizeStringPair message, Option<PlotWeight> selected) =>
        Ask(() => Conversions.Present(selected.Match(
                Some: weight => Dialogs.ShowPrintWidths(title.Local, message.Local, weight.ToHost()),
                None: () => Dialogs.ShowPrintWidths(title.Local, message.Local)))
            .Map(PlotWeight.FromHost));

    // --- [COLORS]
    public static IO<Color4f> ShowColorDialog(RhinoDoc doc, Color4f initial, bool allowAlpha, Option<NamedColorList> palette, Option<Func<Color4f, IO<Unit>>> preview) =>
        Ask(() => {
            Color4f color = initial;
            return (Dialogs.ShowColorDialog(RhinoEtoApp.MainWindowForDocument(doc), ref color, allowAlpha, palette.ValueUnsafe(), preview.Map(static deliver => (Dialogs.OnColorChangedEvent)(shown => Session.Start(deliver(shown)))).ValueUnsafe()), color);
        });

    public static IO<ColorListEntry> ShowColorList(Option<RhinoDoc> doc, ColorList colors, Color initial, bool allowAlpha) =>
        (from dialog in use(() => new ColorListDialog { ColorList = colors, Color = initial, AllowAlpha = allowAlpha })
         from chosen in Ask(() => Conversions.Present(dialog.ShowDialog(Parent(doc)) is Eto.Forms.DialogResult.Ok, dialog.SelectedEntry).Bind(static entry => Optional(entry)))
         select chosen).Bracket();

    // --- [TABLES]
    public static IO<(ComponentRef<Layer> Layer, bool MadeCurrent)> ShowSelectLayerDialog(RhinoDoc doc, ComponentRef<Layer> initial, LocalizeStringPair title, bool showNewLayerButton, bool showSetCurrentButton, bool initialSetCurrentState) =>
        from start in Document.Tables.Rows.Find(doc.Layers, initial)
        from chosen in Ask(() => {
            int index = start.Index;
            bool current = initialSetCurrentState;
            return (Dialogs.ShowSelectLayerDialog(ref index, title.Local, showNewLayerButton, showSetCurrentButton, ref current), (Layer: (ComponentRef<Layer>)new ComponentRef<Layer>.ByIndex(index), MadeCurrent: current));
        })
        select chosen;

    public static IO<Seq<ComponentRef<Layer>>> ShowSelectMultipleLayersDialog(RhinoDoc doc, Seq<ComponentRef<Layer>> defaults, LocalizeStringPair title, bool showNewLayerButton) =>
        from indices in Indices(doc, defaults)
        from chosen in Ask(() => (Dialogs.ShowSelectMultipleLayersDialog(indices, title.Local, showNewLayerButton, out int[] layerIndices),
            toSeq(layerIndices).Map<ComponentRef<Layer>>(static index => new ComponentRef<Layer>.ByIndex(index)).Strict()))
        select chosen;

    public static IO<Unit> ShowLayerMaterialDialog(RhinoDoc doc, Seq<ComponentRef<Layer>> layers) =>
        from indices in Indices(doc, layers)
        from shown in Ask(() => (Dialogs.ShowLayerMaterialDialog(doc, indices), unit))
        select shown;

    public static IO<ComponentRef<Linetype>> ShowLineTypes(RhinoDoc doc, LocalizeStringPair title, LocalizeStringPair message, Option<ComponentRef<Linetype>> selected) =>
        from current in selected.TraverseM(linetype => Document.Tables.Rows.Find(doc.Linetypes, linetype).Map(static found => found.Id)).As()
        from chosen in Ask(() => Conversions.Present(Dialogs.ShowLineTypes(title.Local, message.Local, doc, Conversions.Unset(current))).Map<ComponentRef<Linetype>>(static id => new ComponentRef<Linetype>.ById(id)))
        select chosen;

    public static IO<LinetypeRef> ShowSelectLinetypeDialog(RhinoDoc doc, LinetypeRef initial, bool displayByLayer) =>
        from index in initial.Resolve(doc)
        from chosen in Ask(() => {
            int selected = index;
            return (Dialogs.ShowSelectLinetypeDialog(ref selected, displayByLayer), selected);
        })
        from linetype in IO.lift(() => Conversions.Required(LinetypeRef.FromHost(chosen)))
        select linetype;

    private static IO<Seq<int>> Indices(RhinoDoc doc, Seq<ComponentRef<Layer>> layers) =>
        Products.Each(layers.Map(layer => Document.Tables.Rows.Find(doc.Layers, layer).Map(static found => found.Index)));

    // --- [FILES]
    public static string Filter(Seq<(string Description, Seq<string> Extensions)> rows) =>
        string.Join('|', rows.Map(static row => row.Description + "|" + string.Join(';', row.Extensions.Map(static extension => "*" + extension))));

    public static IO<string> ShowSelectFolderDialog(Option<RhinoDoc> doc, LocalizeStringPair title, Option<string> directory) =>
        (from dialog in use(() => new Eto.Forms.SelectFolderDialog { Title = title.Local })
         from seeded in IO.lift(() => directory.Iter(path => dialog.Directory = path))
         from chosen in Ask(() => (dialog.ShowDialog(Parent(doc)) is Eto.Forms.DialogResult.Ok, dialog.Directory))
         select chosen).Bracket();

    // --- [SUPPRESSION]
    public static Seq<Preference<bool>> Suppressions(ViewCatalog views) =>
        views.Listed<View.Dialog>().Choose(static dialog => dialog.Suppression);

    public static IO<Suppressible<T>> Suppressing<T>(Preference<bool> shows, Func<Eto.Forms.CheckBox, IO<(T Value, bool Suppress)>> show) =>
        from showing in shows.Read
        from asked in showing
            ? (from box in use(static () => new Eto.Forms.CheckBox { Text = LocalText.Of("Do not show again").Local })
               from answer in show(box)
               from kept in when(answer.Suppress, shows.Write(value: false)).As()
               select (Suppressible<T>)new Suppressible<T>.Shown(answer.Value)).Bracket()
            : IO.pure<Suppressible<T>>(new Suppressible<T>.Suppressed())
        select asked;

    public static IO<Unit> ResetMessageBoxes(ViewCatalog views) => Products.Each(Suppressions(views).Map(static shows => shows.Write(value: true))).Map(unit);
}
