using System.Buffers;
using System.Text.Json;
using Rasm.Imaging.Files;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.Collections;
using Rhino.UI;

namespace Rasm.Rhino.Render.Presets;

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

public sealed record Preset(PresetName Name, Fin<ValueSet> Values) {
    public Option<FileInfo> File { get; internal init; }

    public static Preset Of(PresetName name, Seq<ValueSet> parts) =>
        new(name, new ValueSet(toHashMap(parts.Bind(static part => toSeq(part.Entries.AsIterable())))));
}

public sealed record PresetShelf(Map<PresetName, Preset> Stock, Map<PresetName, Preset> User) {
    public Seq<Preset> Listed =>
        toSeq(Enumerable.OrderBy(Stock.Values.Concat(User.Filter((name, _) => !Stock.ContainsKey(name)).Values),
            static preset => (string)preset.Name, Comparer<string>.Create(Localization.LogicalSort)));

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
    private const string Extension = ".json";

    private readonly RowScope scope;
    private readonly BindingGroup bound;
    private readonly Map<PresetName, Preset> stock;
    private readonly DirectoryInfo user;

    public PresetBinding(RowScope scope, BindingGroup bound, DirectoryInfo user, Seq<Preset> stock = default) =>
        (this.scope, this.bound, this.stock, this.user) = (scope, bound, toMap(stock.Map(static preset => (preset.Name, preset))), user);

    // --- [READS]
    public IO<PresetShelf> Shelf =>
        from saved in Listed(user)
        select new PresetShelf(stock, saved);

    public IO<Option<Preset>> Current => Shelf.Bind(Matched);

    private IO<Option<Preset>> Matched(PresetShelf shelf) =>
        from captured in scope.Shown(bound)
        from locks in scope.Locked
        select shelf.Current(captured, locks);

    // --- [WRITES]
    public IO<ValueDiff> Apply(Preset preset) =>
        IO.lift(preset.Values).Bind(values => scope.Commit(bound, values));

    public IO<ValueDiff> Stepped(bool forward) =>
        from shelf in Shelf
        from current in Matched(shelf)
        let listed = forward ? shelf.Listed : shelf.Listed.Rev()
        from diff in (current.Bind(held => toSeq(listed.SkipWhile(preset => preset != held)).Tail.Head) | listed.Head)
            .Match(Some: Apply, None: static () => IO.pure(ValueDiff.Empty))
        select diff;

    public IO<Preset> Save(PresetName name) =>
        scope.Shown(bound).Bind(values => Added(name, values, nameof(Save)));

    public IO<Preset> Replace(Preset preset) =>
        from file in IO.lift(preset.File.ToFin(new UnknownPreset(preset.Name)))
        from values in scope.Shown(bound)
        from written in Written(file, values, overwrite: true)
        select preset with { Values = values };

    public IO<Preset> Rename(Preset preset, PresetName to) =>
        from shelf in Shelf
        from file in IO.lift(preset.File.ToFin(new UnknownPreset(preset.Name)))
        from free in IO.lift(Taken.Unless(shelf.Find(to).ForAll(other => other.File.Exists(held => string.Equals(held.FullName, file.FullName, StringComparison.Ordinal))), nameof(Rename), to))
        let target = UserFile(to)
        from moved in IO.lift(() => File.Move(file.FullName, target.FullName, overwrite: false))
        select preset with { Name = to, File = Some(target) };

    public static IO<Unit> Delete(Preset preset) =>
        from file in IO.lift(preset.File.ToFin(new UnknownPreset(preset.Name)))
        from deleted in IO.lift(file.Delete)
        select deleted;

    public IO<Preset> Import(FileInfo source) =>
        from name in IO.lift(Conversions.Validated<PresetName, string, InvalidRhinoValue>(Path.GetFileNameWithoutExtension(source.Name)))
        from values in IO.lift(() => Read(source))
        from added in Added(name, values, nameof(Import))
        select added;

    public static IO<Unit> Export(Preset preset, FileInfo target) =>
        preset.File.Match(Some: static file => IO.lift(() => Read(file)), None: () => IO.lift(preset.Values))
            .Bind(values => Written(target, values, overwrite: true));

    private IO<Preset> Added(PresetName name, ValueSet values, string member) =>
        from shelf in Shelf
        from free in IO.lift(Taken.Unless(shelf.Find(name).IsNone, member, name))
        let file = UserFile(name)
        from written in Written(file, values, overwrite: false)
            .Catch(static error => error.IsType<DestinationTaken>(), _ => IO.fail<Unit>(new Taken(member, name)))
        select new Preset(name, values) { File = Some(file) };

    // --- [FILES]
    private FileInfo UserFile(PresetName name) => new(Path.Combine(user.FullName, name + Extension));

    private static IO<Map<PresetName, Preset>> Listed(DirectoryInfo folder) =>
        IO.lift(() => toSeq(folder.EnumerateFiles($"*{Extension}"))
                .Map(static file => Conversions.Validated<PresetName, string, InvalidRhinoValue>(Path.GetFileNameWithoutExtension(file.Name)).ToOption()
                    .Map(name => (Name: name, Preset: new Preset(name, Read(file)) { File = Some(file) })))
                .Somes().Strict())
            .Bind(static rows => IO.lift(Callbacks.Unique(rows, static row => row.Name, nameof(Listed)).ToFin()))
            .Map(static rows => toMap(rows))
            .Catch(static error => error.HasException<DirectoryNotFoundException>(), static _ => IO.pure(Map<PresetName, Preset>()));

    private static Fin<ValueSet> Read(FileInfo file) =>
        Try.lift(() => File.ReadAllText(file.FullName)).Run()
            .Bind(text => ValueSet.Read(text).MapFail(cause => new MalformedPreset(file.FullName, cause)));

    private static IO<Unit> Written(FileInfo file, ValueSet values, bool overwrite) =>
        FilePublication.Write(OutputPath.Create(file.FullName), overwrite,
            stream => IO.lift(() => JsonSerializer.Serialize(stream, values, ValueSetContext.Default.ValueSet)));

    // --- [ROWS]
    public IO<Seq<MenuPick>> Listing(Seq<(string Caption, ValueSet Values)> looks) =>
        from shelf in Shelf
        from captured in scope.Shown(bound)
        from locks in scope.Locked
        let current = shelf.Current(captured, locks)
        select looks.Map(look => (MenuPick)new MenuPick.Item(look.Caption, PresetShelf.Matches(look.Values, captured, locks), scope.Commit(bound, look.Values).Map(static _ => unit)))
            .Concat(shelf.Listed.Map(preset => (MenuPick)new MenuPick.Item(preset.Name, current.Exists(held => held == preset), Apply(preset).Map(static _ => unit))))
            .Add(new MenuPick.Item(RowText.Localize("Save Preset...").Local, Marked: false, Saved(current)))
            .Concat(current.Filter(static held => held.File.IsSome)
                .Map(static held => (MenuPick)new MenuPick.Item(RowText.Localize("Delete {0}", arguments: [(string)held.Name]).Local, Marked: false, Delete(held)))
                .ToSeq());

    public static ControlRow Browser(Func<RowScope, IO<PresetBinding>> bindings) =>
        ListRows.Browser(RowText.Localize("Presets").Local, RowText.Localize("Presets of this section's values").Local, new BrowserRow<Preset, PresetName>(
                Key: static preset => preset.Name,
                Label: static (_, preset) => preset.Name,
                Tile: static (scope, view, preset, box, retain) =>
                    from tiles in IO.lift(() => PreviewTiles.Current.Of(scope.Sink))
                    from request in Requested(scope, preset).OnUiThread()
                    from thumbnail in tiles.Thumbnail(view, request, box, retain)
                    select thumbnail,
                Items: scope => new ValueStore<Seq<Preset>>(
                    bindings(scope).Bind(static binding => binding.Shelf).Map(static shelf => Some(shelf.Listed)),
                    items => items.Match(Some: held => bindings(scope).Bind(binding => binding.Renamed(held)), None: static () => IO.pure(Change<Seq<Preset>>.None)),
                    EqualityComparer<Seq<Preset>>.Default.Equals,
                    Applied.Live,
                    None),
                Current: scope => bindings(scope).Bind(static binding => binding.Current).Map(static held => held.Map(static preset => preset.Name)),
                Choose: (scope, name) => bindings(scope).Bind(binding => binding.Picked(name)),
                Mode: BrowserMode.Grid,
                Extent: new ReadoutSource<PixelExtent>.Model(Framing.Extent),
                Empty: RowText.Localize("No presets").Local) {
            Numbered = true,
            Ordered = Some<Func<Preset, string>>(static preset => preset.Name),
            Search = Some(new ListSearch<Preset>(RowText.Localize("Search presets").Local, static (_, preset) => preset.Name, None)),
            Rename = Some<Func<Preset, string, Fin<Preset>>>(static (preset, text) =>
                preset.File.ToFin(new UnknownPreset(preset.Name))
                    .Bind(_ => Conversions.Validated<PresetName, string, InvalidRhinoValue>(text)).Map(name => preset with { Name = name })),
            File = Some<Func<Preset, Option<string>>>(static preset => preset.File.Map(static file => file.FullName)),
            Preview = Some<Func<RowScope, Preset, Action<IDisposable>, IO<Unit>>>(static (scope, hovered, retain) =>
                from comparison in IO.lift(() => RenderingComparison.Current.Of(scope.Sink))
                from request in Requested(scope, hovered)
                from previewed in comparison.Previewed(request, retain)
                select previewed),
            Reloads = Seq(RenderHistory.Changed, EventKind.DocumentPropertiesChanged.Choose(static _ => Some(unit))),
            Commands = selection => [
                Run("preset-save", "Save Preset...", "Save the section's values as a preset",
                    scope => from binding in bindings(scope) from current in binding.Current from saved in binding.Saved(current) select saved),
                Run("preset-replace", "Replace Preset", "Write the section's values over the selected presets",
                    scope => from binding in bindings(scope) from chosen in selection from replaced in Callbacks.Each(chosen.Map(binding.Replaced)) select unit),
                Run("preset-import", "Import Presets...", "Copy preset files into this section's presets",
                    scope => bindings(scope).Bind(static binding => binding.Imported)),
                Run("preset-export", "Export Presets...", "Write each selected preset to a file",
                    _ => from chosen in selection from exported in chosen.TraverseM(Exported).As() select unit),
                Run("preset-delete", "Delete Presets", "Delete the selected user presets",
                    _ => from chosen in selection from deleted in Callbacks.Each(chosen.Map(Delete)) select unit),
                Run("preset-previous", "Previous Preset", "Apply the preset listed before the current one",
                    scope => bindings(scope).Bind(static binding => binding.Stepped(forward: false)).Map(static _ => unit)),
                Run("preset-next", "Next Preset", "Apply the preset listed after the current one",
                    scope => bindings(scope).Bind(static binding => binding.Stepped(forward: true)).Map(static _ => unit)),
            ],
            Wording = Wording.Localized,
        },
            RowRules.Always);

    private static CommandRow.Run Run(string name, string caption, string help, Func<RowScope, IO<Unit>> execute) =>
        new(new CommandFace(name, RowText.Localize(caption).Local, RowText.Localize(help).Local, None, None) { Wording = Wording.Localized }, None, execute);

    private static IO<TileRequest> Requested(RowScope scope, Preset preset) =>
        from doc in IO.lift(scope.Document.ToFin(new Missing(nameof(RowScope.Document))))
        from history in IO.lift(() => RenderHistory.Current.Of(scope.Sink))
        from renderings in history.Renderings
        from newest in IO.lift(renderings.Head.ToFin(new Missing(nameof(RenderHistory.Renderings))))
        from grading in IO.lift(() => history.Grading(doc))
        from current in grading.Capture
        from locks in scope.Locked
        from entries in RenderSettingsSources.Read(doc, static scene => EffectRegistry.Read(scene.Settings))
        from request in IO.lift(TileRequest.Applied(newest, preset, current, locks, EffectPipeline.Ordered(entries, history.Kinds)))
        select request;

    private IO<Unit> Picked(PresetName name) =>
        from shelf in Shelf
        from held in IO.lift(shelf.Find(name).ToFin(new UnknownPreset(name)))
        from diff in Apply(held)
        select unit;

    private IO<Unit> Saved(Option<Preset> current) =>
        from name in HostDialogs.ShowEditBox<PresetName, InvalidRhinoValue>(RowText.Localize("Save Preset"), RowText.Localize("Preset name"), current.Map(static held => held.Name))
        from shelf in Shelf
        from free in IO.lift(Taken.Unless(!shelf.Stock.ContainsKey(name), nameof(Save), name))
        from saved in shelf.User.Find(name).Match(Some: held => Replaced(held), None: () => Save(name).Map(static _ => unit))
        select saved;

    private IO<Unit> Replaced(Preset preset) =>
        from answer in HostDialogs.ShowMessage(scope.Document, RowText.Localize("Replace the preset of this name with the section's values?"), RowText.Localize("Replace Preset"),
            ShowMessageButton.YesNo, ShowMessageIcon.Question, ShowMessageDefaultButton.Button2, ShowMessageOptions.None, ShowMessageMode.ApplicationModal)
        from confirmed in IO.lift(guard(answer == ShowMessageResult.Yes, Errors.Cancelled).ToFin())
        from replaced in Replace(preset)
        select unit;

    private IO<Unit> Imported =>
        from dialog in IO.lift(static () => new OpenFileDialog { Filter = Filter, MultiSelect = true, Title = RowText.Localize("Import Presets").Local })
        from paths in HostDialogs.ShowOpenDialog(dialog)
        from added in Callbacks.Each(paths.Map(path => Import(new FileInfo(path))))
        select unit;

    private static IO<Unit> Exported(Preset preset) =>
        from dialog in IO.lift(() => new SaveFileDialog { Filter = Filter, FileName = preset.Name + Extension, Title = RowText.Localize("Export Preset").Local })
        from path in HostDialogs.ShowSaveDialog(dialog)
        from exported in Export(preset, new FileInfo(path))
        select exported;

    private static string Filter => HostDialogs.Filter(Seq((RowText.Localize("Presets").Local, Seq(Extension))));

    private IO<Change<Seq<Preset>>> Renamed(Seq<Preset> items) =>
        from shelf in Shelf
        let edits = from item in items
                    from file in item.File.ToSeq()
                    from held in toSeq(shelf.User.Values)
                    from stored in held.File.ToSeq()
                    where string.Equals(stored.FullName, file.FullName, StringComparison.Ordinal) && !string.Equals(held.Name, item.Name, StringComparison.Ordinal)
                    select Rename(held, item.Name).Map(written => (Before: held.Name, After: written))
        from renamed in Callbacks.Each(edits)
        let next = shelf with { User = renamed.Fold(shelf.User, static (users, edit) => users.Remove(edit.Before).AddOrUpdate(edit.After.Name, edit.After)) }
        select ValueStore.Between<Seq<Preset>>(shelf.Listed, next.Listed);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class LockSets {
    // --- [STORE]
    public static IO<Ref<HashMap<Guid, LanguageExt.HashSet<EntryKey>>>> Registry(RhinoDoc doc) =>
        IO.lift(() => doc.RuntimeData.GetValue(typeof(LockSets), static _ => Ref(HashMap<Guid, LanguageExt.HashSet<EntryKey>>())));

    public static ValueStore<LanguageExt.HashSet<EntryKey>> Store(RhinoDoc doc, Guid section) =>
        new(
            Registry(doc).Bind(static registry => registry.ValueIO).Map(registry => registry.Find(section)),
            locks =>
                from registry in Registry(doc)
                from changed in atomicIO(() => {
                    HashMap<Guid, LanguageExt.HashSet<EntryKey>> before = registry.Value;
                    registry.Value = locks.Match(Some: held => before.AddOrUpdate(section, held), None: () => before.Remove(section));
                    return ValueStore.Between(before.Find(section), locks);
                })
                from modified in when(changed.HasChanged, IO.lift(() => { doc.Modified = true; })).As()
                select changed,
            EqualityComparer<LanguageExt.HashSet<EntryKey>>.Default.Equals,
            Applied.Live,
            None);

    // --- [ARCHIVE]
    private const uint TypeCode = 0x4C4F_434Bu;

    public static DictionaryCodec<LanguageExt.HashSet<EntryKey>> Keys { get; } = new("EntryKeys", 1, Encoded, Locked);

    private static DictionaryCodec<HashMap<Guid, LanguageExt.HashSet<EntryKey>>> Codec { get; } = new("LockSets", 1, Encoded, Sections);

    public static DocumentFrame Frame { get; } = new DocumentFrame<HashMap<Guid, LanguageExt.HashSet<EntryKey>>>(
        TypeCode,
        Codec,
        HashMap<Guid, LanguageExt.HashSet<EntryKey>>(),
        static (doc, _) => Registry(doc).Bind(static registry => registry.ValueIO),
        static (doc, locks) => Registry(doc).Bind(registry => atomicIO(() => registry.Value = registry.Value.AddOrUpdateRange(locks.AsIterable()))).Map(static _ => unit));

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
