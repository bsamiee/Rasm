using System.Buffers;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Scenes;
using Rasm.Rhino.Render.Slots;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Viewers;
using Rasm.Rhino.UI.Views;
using Rhino;
using Rhino.Collections;
using Rhino.PlugIns;
using Rhino.UI;

namespace Rasm.Rhino.Render;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>(SkipIParsable = true)]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class PresetName {
    private static readonly SearchValues<char> Reserved = SearchValues.Create(Path.GetInvalidFileNameChars());

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref string value) =>
        (value, validationError) = value.TrimOrNullify() is { } trimmed && !trimmed.AsSpan().ContainsAny(Reserved)
            ? (trimmed, default(InvalidRhinoValue?))
            : (value, new InvalidRhinoValue());
}

public sealed record Preset(PresetName Name, FileInfo File, Fin<ValueSet> Values);

public sealed record PresetShelf(Map<PresetName, Preset> Stock, Map<PresetName, Preset> User) {
    public Seq<Preset> Listed =>
        toSeq(Enumerable.OrderBy(Stock.Values.Concat(User.Values), static preset => (string)preset.Name, Comparer<string>.Create(Localization.LogicalSort)));

    public Option<Preset> Find(PresetName name) => Stock.Find(name) | User.Find(name);

    public Option<Preset> Current(ValueSet captured, LanguageExt.HashSet<EntryKey> locks) =>
        Listed.Find(preset => preset.Values.Exists(values => Matches(values, captured, locks)));

    public static ValueDiff Preview(ValueSet values, ValueSet captured, LanguageExt.HashSet<EntryKey> locks) =>
        captured.Diff(new ValueSet(values.Entries.Intersect(captured.Entries.Keys)).Over(captured, locks));

    public static bool Matches(ValueSet values, ValueSet captured, LanguageExt.HashSet<EntryKey> locks) =>
        values.Entries.Exists((key, _) => captured.Entries.ContainsKey(key) && !locks.Contains(key)) && Preview(values, captured, locks).Changes.IsEmpty;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PresetBinding {
    // --- [STATE]
    private const string Root = "Presets";
    private const string Extension = ".json";

    private readonly RowScope scope;
    private readonly HistoryBinding history;
    private readonly PlugIn plugIn;
    private readonly Option<DirectoryInfo> stock;
    private readonly DirectoryInfo user;

    private PresetBinding(RowScope scope, HistoryBinding history, PlugIn plugIn, Option<DirectoryInfo> stock, DirectoryInfo user) =>
        (this.scope, this.history, this.plugIn, this.stock, this.user) = (scope, history, plugIn, stock, user);

    public static IO<PresetBinding> Of(RowScope scope) =>
        from history in IO.lift(scope.History.ToFin(new Missing(nameof(RowScope.History))))
        let plugIn = (PlugIn)scope.Sink
        let folder = Path.Combine(Root, StoredText.Format(history.Id))
        select new PresetBinding(
            scope, history, plugIn,
            Conversions.Present(plugIn.Assembly.Location).Bind(static path => Optional(Path.GetDirectoryName(path))).Map(directory => new DirectoryInfo(Path.Combine(directory, folder))),
            new DirectoryInfo(Path.Combine(plugIn.SettingsDirectory, folder)));

    // --- [READS]
    public IO<PresetShelf> Shelf =>
        from stocked in stock.Match(Some: Listed, None: static () => IO.pure(Map<PresetName, Preset>()))
        from saved in Listed(user)
        select new PresetShelf(stocked, saved);

    public IO<Option<Preset>> Current => Shelf.Bind(Matched);

    private IO<Option<Preset>> Matched(PresetShelf shelf) =>
        from captured in scope.Shown(history.Group)
        from locks in scope.Locked
        select shelf.Current(captured, locks);

    // --- [WRITES]
    public IO<ValueDiff> Apply(Preset preset) =>
        IO.lift(preset.Values).Bind(values => scope.Commit(history.Group, values));

    public IO<ValueDiff> Stepped(bool forward) =>
        from shelf in Shelf
        from current in Matched(shelf)
        let listed = forward ? shelf.Listed : shelf.Listed.Rev()
        from diff in (current.Bind(held => toSeq(listed.SkipWhile(preset => preset != held)).Tail.Head) | listed.Head)
            .Match(Some: Apply, None: static () => IO.pure(ValueDiff.Empty))
        select diff;

    public IO<Preset> Save(PresetName name) =>
        scope.Shown(history.Group).Bind(values => Added(name, values, nameof(Save)));

    public IO<Preset> Replace(Preset preset) =>
        from shelf in Shelf
        from held in IO.lift(Owned(shelf, preset))
        from values in scope.Shown(history.Group)
        from written in Written(held.File, values)
        select held with { Values = values };

    public IO<Preset> Rename(Preset preset, PresetName to) =>
        from shelf in Shelf
        from held in IO.lift(Owned(shelf, preset))
        from free in IO.lift(Taken.Unless(shelf.Find(to).ForAll(other => other == held), nameof(Rename), to))
        let target = UserFile(to)
        from moved in IO.lift(() => File.Move(held.File.FullName, target.FullName, overwrite: false))
        select held with { Name = to, File = target };

    public IO<Unit> Delete(Preset preset) =>
        from shelf in Shelf
        from held in IO.lift(Owned(shelf, preset))
        from deleted in IO.lift(held.File.Delete)
        select deleted;

    public IO<Preset> Import(FileInfo source) =>
        from name in IO.lift(Conversions.Validated<PresetName, string, InvalidRhinoValue>(Path.GetFileNameWithoutExtension(source.Name)))
        from values in IO.lift(() => Read(source))
        from added in Added(name, values, nameof(Import))
        select added;

    public static IO<Unit> Export(Preset preset, FileInfo target) =>
        IO.lift(() => Read(preset.File)).Bind(values => Written(target, values));

    private IO<Preset> Added(PresetName name, ValueSet values, string member) =>
        from shelf in Shelf
        from free in IO.lift(Taken.Unless(shelf.Find(name).IsNone, member, name))
        from created in IO.lift(user.Create)
        let file = UserFile(name)
        from written in Written(file, values)
        select new Preset(name, file, values);

    private static Fin<Preset> Owned(PresetShelf shelf, Preset preset) =>
        toSeq(shelf.User.Values).Find(held => string.Equals(held.File.FullName, preset.File.FullName, StringComparison.Ordinal)).ToFin(new UnknownPreset(preset.Name));

    // --- [FILES]
    private FileInfo UserFile(PresetName name) => new(Path.Combine(user.FullName, name + Extension));

    private static IO<Map<PresetName, Preset>> Listed(DirectoryInfo folder) =>
        IO.lift(() => toMap(toSeq(folder.EnumerateFiles($"*{Extension}"))
                .Map(static file => Conversions.Validated<PresetName, string, InvalidRhinoValue>(Path.GetFileNameWithoutExtension(file.Name)).ToOption()
                    .Map(name => (name, new Preset(name, file, Read(file)))))
                .Somes()))
            .Catch(static error => error.HasException<DirectoryNotFoundException>(), static _ => IO.pure(Map<PresetName, Preset>()));

    private static Fin<ValueSet> Read(FileInfo file) =>
        Try.lift(() => File.ReadAllText(file.FullName)).Run()
            .Bind(text => ValueSet.Read(text).MapFail(cause => new MalformedPreset(file.FullName, cause)));

    private static IO<Unit> Written(FileInfo file, ValueSet values) =>
        $"{file.FullName}.tmp" switch {
            var temp =>
                from staged in IO.lift(() => File.WriteAllText(temp, values.Text()))
                from moved in IO.lift(() => File.Move(temp, file.FullName, overwrite: true))
                select moved,
        };

    // --- [ROWS]
    public static IO<Seq<MenuPick>> Listing(RowScope scope, Seq<(string Caption, ValueSet Values)> looks) =>
        from binding in Of(scope)
        from shelf in binding.Shelf
        from captured in scope.Shown(binding.history.Group)
        from locks in scope.Locked
        let current = shelf.Current(captured, locks)
        select looks.Map(look => (MenuPick)new MenuPick.Item(look.Caption, PresetShelf.Matches(look.Values, captured, locks), scope.Commit(binding.history.Group, look.Values).Map(static _ => unit)))
            .Concat(shelf.Listed.Map(preset => (MenuPick)new MenuPick.Item(preset.Name, current.Exists(held => held == preset), binding.Apply(preset).Map(static _ => unit))))
            .Add(new MenuPick.Item(RowText.Localize("Save Preset...").Local, Marked: false, binding.Saved(current)))
            .Concat(current.Filter(held => Owned(shelf, held).IsSucc)
                .Map(held => (MenuPick)new MenuPick.Item(RowText.Localize("Delete {0}", arguments: [(string)held.Name]).Local, Marked: false, binding.Delete(held)))
                .ToSeq());

    public static ControlRow Browser { get; } =
        ListRows.Browser(RowText.Localize("Presets").Local, RowText.Localize("Presets of this section's values").Local, new BrowserRow<Preset, PresetName>(
                Key: static preset => preset.Name,
                Label: static (_, preset) => preset.Name,
                Tile: static (scope, view, preset, box) =>
                    from tiles in IPlugInRendering.Served(((IPlugInRendering)scope.Sink).Tiles, nameof(IPlugInRendering.Tiles))
                    from request in Requested(scope, preset).Post()
                    from thumbnail in tiles.Thumbnail(view, request, box)
                    select thumbnail,
                Items: static scope => ValueStore.Of(
                    Of(scope).Bind(static binding => binding.Shelf).Map(static shelf => Some(shelf.Listed)),
                    items => items.Match(Some: held => Of(scope).Bind(binding => binding.Renamed(held)), None: static () => IO.pure(unit)),
                    Applied.Live,
                    None),
                Current: static scope => ValueStore.Of(
                    Of(scope).Bind(static binding => binding.Current).Map(static held => held.Map(static preset => preset.Name)),
                    name => Of(scope).Bind(binding => name.Match(Some: held => binding.Picked(held), None: static () => IO.pure(unit))),
                    Applied.Live,
                    None),
                Mode: BrowserMode.Grid,
                Extent: new ReadoutSource<PixelExtent>.Model(Framing.Extent),
                Empty: RowText.Localize("No presets").Local) {
            Numbered = true,
            Ordered = Some<Func<Preset, string>>(static preset => preset.Name),
            Search = Some(new ListSearch<Preset>(RowText.Localize("Search presets").Local, static (_, preset) => preset.Name, None)),
            Rename = Some<Func<Preset, string, Fin<Preset>>>(static (preset, text) =>
                Conversions.Validated<PresetName, string, InvalidRhinoValue>(text).Map(name => preset with { Name = name })),
            File = Some<Func<Preset, string>>(static preset => preset.File.FullName),
            Preview = Some<Func<RowScope, Option<Preset>, IO<Unit>>>(static (scope, hovered) =>
                hovered.Traverse(preset => Requested(scope, preset)).As().Bind(SlotComparison.Previewed)),
            Reloads = Seq(EventKind.ImageFileSaved.Choose(static _ => Some(unit)), EventKind.DocumentPropertiesChanged.Choose(static _ => Some(unit))),
            Commands = static selection => [
                Run("preset-save", "Save Preset...", "Save the section's values as a preset",
                    static scope => from binding in Of(scope) from current in binding.Current from saved in binding.Saved(current) select saved),
                Run("preset-replace", "Replace Preset", "Write the section's values over the selected presets",
                    scope => from binding in Of(scope) from chosen in selection from replaced in chosen.TraverseM(preset => binding.Replaced(preset)).As() select unit),
                Run("preset-import", "Import Presets...", "Copy preset files into this section's presets",
                    static scope => Of(scope).Bind(static binding => binding.Imported)),
                Run("preset-export", "Export Presets...", "Write each selected preset to a file",
                    scope => from binding in Of(scope) from chosen in selection from exported in chosen.TraverseM(preset => binding.Exported(preset)).As() select unit),
                Run("preset-delete", "Delete Presets", "Delete the selected user presets",
                    scope => from binding in Of(scope) from chosen in selection from deleted in chosen.TraverseM(preset => binding.Delete(preset)).As() select unit),
                Run("preset-previous", "Previous Preset", "Apply the preset listed before the current one",
                    static scope => Of(scope).Bind(static binding => binding.Stepped(forward: false)).Map(static _ => unit)),
                Run("preset-next", "Next Preset", "Apply the preset listed after the current one",
                    static scope => Of(scope).Bind(static binding => binding.Stepped(forward: true)).Map(static _ => unit)),
            ],
            Wording = Wording.Localized,
        },
            RowRules.Always);

    private static CommandRow.Run Run(string name, string caption, string help, Func<RowScope, IO<Unit>> execute) =>
        new(new CommandFace(name, RowText.Localize(caption).Local, RowText.Localize(help).Local, None, None) { Wording = Wording.Localized }, None, execute);

    private static IO<TileRequest> Requested(RowScope scope, Preset preset) =>
        (IPlugInRendering)scope.Sink switch {
            var rendering =>
                from doc in IO.lift(scope.Document.ToFin(new Missing(nameof(RowScope.Document))))
                from history in IPlugInRendering.Served(rendering.History, nameof(IPlugInRendering.History))
                from slots in history.Slots
                from newest in IO.lift(slots.Head.ToFin(new Missing(nameof(RenderHistory.Slots))))
                from grading in IO.lift(history.Grading(doc).ToFin())
                from current in grading.Capture
                from locks in scope.Locked
                from entries in EffectRegistry.Apply(doc.RenderSettings, Seq<EffectRequest>())
                from request in IO.lift(TileRequest.Applied(newest, preset, current, locks, EffectPipeline.Ordered(entries, rendering.Effects)))
                select request,
        };

    private IO<Unit> Picked(PresetName name) =>
        from shelf in Shelf
        from held in IO.lift(shelf.Find(name).ToFin(new UnknownPreset(name)))
        from diff in Apply(held)
        select unit;

    private IO<Unit> Saved(Option<Preset> current) =>
        from name in HostDialogs.ShowEditBox<PresetName, InvalidRhinoValue>(RowText.Localize("Save Preset"), RowText.Localize("Preset name"), current.Map(static held => held.Name))
        from shelf in Shelf
        from saved in shelf.User.Find(name).Match(Some: held => Replaced(held), None: () => Save(name).Map(static _ => unit))
        select saved;

    private IO<Unit> Replaced(Preset preset) =>
        from answer in HostDialogs.ShowMessage(scope.Document, RowText.Localize("Replace the preset of this name with the section's values?"), RowText.Localize("Replace Preset"),
            ShowMessageButton.YesNo, ShowMessageIcon.Question, ShowMessageDefaultButton.Button2, ShowMessageOptions.None, ShowMessageMode.ApplicationModal)
        from confirmed in IO.lift(guard(answer == ShowMessageResult.Yes, Errors.Cancelled).ToFin())
        from replaced in Replace(preset)
        select unit;

    private IO<Unit> Imported =>
        from dialog in IO.lift(() => new OpenFileDialog { Filter = Filter, MultiSelect = true, Title = RowText.Localize("Import Presets").Local })
        from paths in HostDialogs.ShowOpenDialog(dialog)
        from added in paths.TraverseM(path => Import(new FileInfo(path))).As()
        select unit;

    private IO<Unit> Exported(Preset preset) =>
        from dialog in IO.lift(() => new SaveFileDialog { Filter = Filter, FileName = preset.File.Name, Title = RowText.Localize("Export Preset").Local })
        from path in HostDialogs.ShowSaveDialog(dialog)
        from exported in Export(preset, new FileInfo(path))
        select exported;

    private string Filter => HostDialogs.Filter(Seq(new FileTypeRow(plugIn.Id, RowText.Localize("Presets").Local, Seq(Extension))));

    private IO<Unit> Renamed(Seq<Preset> items) =>
        items.Filter(static item => !string.Equals(Path.GetFileNameWithoutExtension(item.File.Name), item.Name, StringComparison.Ordinal))
            .TraverseM(item => Rename(item, item.Name))
            .As()
            .Map(static _ => unit);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class LockSets {
    // --- [STORE]
    public static IO<AtomHashMap<Guid, LanguageExt.HashSet<EntryKey>>> Registry(RhinoDoc doc) =>
        IO.lift(() => doc.RuntimeData.GetValue(typeof(LockSets), static _ => AtomHashMap<Guid, LanguageExt.HashSet<EntryKey>>()));

    public static ValueStore<LanguageExt.HashSet<EntryKey>> Store(RhinoDoc doc, Guid section) =>
        ValueStore.Of(
            Registry(doc).Map(registry => registry.Find(section)),
            locks =>
                from registry in Registry(doc)
                from swapped in IO.lift(() => registry.SwapKey(section, _ => locks))
                from modified in IO.lift(() => doc.Modified = true)
                select unit,
            Applied.Live,
            None);

    // --- [ARCHIVE]
    public const uint TypeCode = 0x4C4F_434Bu;

    public static DictionaryCodec<LanguageExt.HashSet<EntryKey>> Keys { get; } = new("EntryKeys", 1, Encoded, Locked);

    public static DictionaryCodec<HashMap<Guid, LanguageExt.HashSet<EntryKey>>> Codec { get; } = new("LockSets", 1, Encoded, Sections);

    private static Fin<ArchivableDictionary> Encoded(LanguageExt.HashSet<EntryKey> keys) =>
        Branch(toSeq(keys.Order()).Map(static key => key.Owner.Cons(key.Path)));

    private static Fin<ArchivableDictionary> Branch(Seq<Seq<string>> paths) =>
        ArchivableDictionaries.Nested(toSeq(paths.Filter(static path => !path.IsEmpty).GroupBy(static path => path[0], StringComparer.Ordinal))
            .Map(static group => (group.Key, Branch(toSeq(group).Map(static path => path.Tail)))));

    private static Fin<ArchivableDictionary> Encoded(HashMap<Guid, LanguageExt.HashSet<EntryKey>> sections) =>
        ArchivableDictionaries.Nested(toSeq(sections.AsIterable()).Map(static section => (StoredText.Format(section.Key), Keys.ToDictionary(section.Value))));

    private static Fin<LanguageExt.HashSet<EntryKey>> Locked(ArchivableDictionary source) =>
        Paths(source).Map(static paths => toHashSet(paths.Map(static path => new EntryKey(path[0], path.Tail))));

    private static Fin<Seq<Seq<string>>> Paths(ArchivableDictionary source) =>
        from rows in ArchivableDictionaries.Children(source)
        from paths in Callbacks.Each(rows, static (row, _) =>
            row.Value.Count == 0 ? Fin.Succ(Seq(Seq(row.Key))) : Paths(row.Value).Map(tails => tails.Map(tail => row.Key.Cons(tail))))
        select paths.Flatten();

    private static Fin<HashMap<Guid, LanguageExt.HashSet<EntryKey>>> Sections(ArchivableDictionary source) =>
        from rows in ArchivableDictionaries.Children(source)
        from sections in Callbacks.Each(rows, static (row, _) =>
            (StoredText.Parse<Guid>(row.Key).ToValidation(), Keys.FromDictionary(row.Value).ToValidation())
                .Apply(static (section, keys) => (section, keys)).As().ToFin())
        select toHashMap(sections);
}
