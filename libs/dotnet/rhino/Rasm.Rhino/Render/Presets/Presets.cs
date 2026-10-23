using System.Buffers;
using Rasm.Imaging.Files;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rhino.UI;

namespace Rasm.Rhino.Render.Presets;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>(SkipIParsable = true)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class PresetName {
    private static readonly SearchValues<char> Reserved = SearchValues.Create(Path.GetInvalidFileNameChars());

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        (value, validationError) = value.TrimOrNullify() is { } trimmed && !trimmed.AsSpan().ContainsAny(Reserved)
            ? (trimmed, default(ValidationError?))
            : (value, new ValidationError("preset name is blank or holds invalid file name character"));
}

public sealed record Preset(PresetName Name, Fin<ValueSet> Values) {
    public Option<FileInfo> File { get; internal init; }

    public string Caption(IPlugInSink sink) => File.IsSome ? Name : Wording.English.Shown(Name, sink);

    public static Preset Of(PresetName name, Seq<ValueSet> parts) =>
        new(name, new ValueSet(toHashMap(parts.Bind(static part => toSeq(part.Entries.AsIterable())))));
}

public sealed record PresetShelf(Seq<Preset> Stock, Map<PresetName, Preset> User) {
    public Seq<Preset> Listed =>
        Stock + toSeq(Enumerable.OrderBy(User.Filter((name, _) => Stock.ForAll(preset => preset.Name != name)).Values,
            static preset => (string)preset.Name, Comparer<string>.Create(Localization.LogicalSort)));

    public Option<Preset> Find(PresetName name) => Stock.Find(preset => preset.Name == name) | User.Find(name);

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
    private readonly Seq<Preset> stock;
    private readonly DirectoryInfo user;

    public PresetBinding(RowScope scope, BindingGroup bound, string owner, Seq<Preset> stock = default) =>
        (this.scope, this.bound, this.stock, user) = (scope, bound, stock, new DirectoryInfo(Path.Combine(((IPlugInViews)scope.Sink).PlugIn.SettingsDirectory, "Presets", owner)));

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
        from file in IO.lift(preset.File.ToFin(new NotFound("preset", preset.Name)))
        from values in scope.Shown(bound)
        from written in Written(file, values, overwrite: true)
        select preset with { Values = values };

    public IO<Preset> Rename(Preset preset, PresetName to) =>
        from shelf in Shelf
        from file in IO.lift(preset.File.ToFin(new NotFound("preset", preset.Name)))
        from free in IO.lift(shelf.Find(to).ForAll(other => other.File.Exists(held => string.Equals(held.FullName, file.FullName, StringComparison.Ordinal))) ? Fin.Succ(unit) : Error.New($"{nameof(Rename)} already holds {to}"))
        let target = UserFile(to)
        from moved in IO.lift(() => File.Move(file.FullName, target.FullName))
        select preset with { Name = to, File = Some(target) };

    public static IO<Unit> Delete(Preset preset) =>
        from file in IO.lift(preset.File.ToFin(new NotFound("preset", preset.Name)))
        from deleted in IO.lift(file.Delete)
        select deleted;

    public IO<Preset> Import(FileInfo source) =>
        from name in IO.lift(PresetName.Validated(Path.GetFileNameWithoutExtension(source.Name)))
        from values in IO.lift(() => Read(source))
        from added in Added(name, values, nameof(Import))
        select added;

    public static IO<Unit> Export(Preset preset, FileInfo target) =>
        preset.File.Match(Some: static file => IO.lift(() => Read(file)), None: () => IO.lift(preset.Values))
            .Bind(values => Written(target, values, overwrite: true));

    private IO<Preset> Added(PresetName name, ValueSet values, string member) =>
        from shelf in Shelf
        from free in IO.lift(shelf.Find(name).IsNone ? Fin.Succ(unit) : Error.New($"{member} already holds {name}"))
        let file = UserFile(name)
        from written in Written(file, values, overwrite: false)
            .Catch(static error => error.IsType<DestinationTaken>(), _ => IO.fail<Unit>(Error.New($"{member} already holds {name}")))
        select new Preset(name, values) { File = Some(file) };

    // --- [FILES]
    private FileInfo UserFile(PresetName name) => new(Path.Combine(user.FullName, name + Extension));

    private static IO<Map<PresetName, Preset>> Listed(DirectoryInfo folder) =>
        IO.lift(() => toSeq(folder.EnumerateFiles($"*{Extension}")).Strict())
            .Catch(static error => error.HasException<DirectoryNotFoundException>(), static _ => IO.pure(Seq<FileInfo>()))
            .Map(static files => files
                .Map(static file => PresetName.Validated(Path.GetFileNameWithoutExtension(file.Name)).ToOption()
                    .Map(name => (Name: name, Preset: new Preset(name, Read(file)) { File = Some(file) })))
                .Somes().Strict())
            .Bind(static rows => IO.lift(Callbacks.Unique(rows, static row => row.Name, nameof(Listed))))
            .Map(static rows => toMap(rows));

    private static Fin<ValueSet> Read(FileInfo file) =>
        Try.lift(() => File.ReadAllText(file.FullName)).Run()
            .Bind(text => ValueSet.Read(text).MapFail(cause => new FileRefused(file.FullName, "holds no preset", cause)));

    private static IO<Unit> Written(FileInfo file, ValueSet values, bool overwrite) =>
        JsonFiles.Write(OutputPath.Create(file.FullName), values, ValueSetContext.Default.ValueSet, overwrite);

    // --- [ROWS]
    public IO<Seq<MenuPick>> Listing() =>
        from shelf in Shelf
        from current in Matched(shelf)
        select shelf.Listed.Map(preset => (MenuPick)new MenuPick.Item(preset.Caption(scope.Sink), current.Exists(held => held == preset), Apply(preset).Map(unit)))
            .Add(new MenuPick.Item(RowText.Localize("Save Preset...").Local, Marked: false, Saved(current)))
            .Concat(current.Filter(static held => held.File.IsSome)
                .Map(static held => (MenuPick)new MenuPick.Item(RowText.Localize("Delete {0}", arguments: [(string)held.Name]).Local, Marked: false, Delete(held)))
                .ToSeq());

    public static ControlRow Browser(EffectSource source, string owner, Seq<Preset> stock) =>
        fun((RowScope scope) => EffectRegistry.Bindings(source, scope.Sink).Map(group => new PresetBinding(scope, group, owner, stock))) switch {
            var bindings => ListRows.Browser("Presets", "Stock looks and saved presets", new BrowserRow<Preset, PresetName>(
                    Key: static preset => preset.Name,
                    Label: static (scope, preset) => preset.Caption(scope.Sink),
                    Tile: (scope, view, preset, box, retain) =>
                        from tiles in PreviewTiles.Current.Of(scope.Sink)
                        from binding in bindings(scope)
                        from request in binding.Requested(source, preset).OnUiThread()
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
                    Extent: new ReadoutSource<Eto.Drawing.Size>.Model(Framing.Extent),
                    Empty: "No presets") {
                Numbered = true,
                Search = Some(new ListSearch<Preset>("Search presets", static (scope, preset) => preset.Caption(scope.Sink), None)),
                Rename = Some<Func<Preset, string, Fin<Preset>>>(static (preset, text) =>
                    preset.File.ToFin(new NotFound("preset", preset.Name))
                        .Bind(_ => PresetName.Validated(text)).Map(name => preset with { Name = name })),
                File = Some<Func<Preset, Option<string>>>(static preset => preset.File.Map(static file => file.FullName)),
                Preview = Some<Func<RowScope, Preset, Action<IDisposable>, IO<Unit>>>((scope, hovered, retain) =>
                    from comparison in RenderingComparison.Current.Of(scope.Sink)
                    from binding in bindings(scope)
                    from request in binding.Requested(source, hovered)
                    from previewed in comparison.Previewed(request, retain)
                    select previewed),
                Reloads = Seq(RenderHistory.Changed, source.Changed),
                Commands = selection => [
                    Run("preset-save", "Save Preset...", "Save section values as preset",
                        scope => from binding in bindings(scope) from current in binding.Current from saved in binding.Saved(current) select saved),
                    Run("preset-replace", "Replace Preset", "Write section values over selected presets",
                        scope => from binding in bindings(scope) from chosen in selection from replaced in Callbacks.Each(chosen.Map(binding.Replaced)) select unit),
                    Run("preset-import", "Import Presets...", "Copy preset files into section presets",
                        scope => bindings(scope).Bind(static binding => binding.Imported)),
                    Run("preset-export", "Export Presets...", "Write each selected preset to file",
                        _ => from chosen in selection from exported in chosen.TraverseM(Exported).As() select unit),
                    Run("preset-delete", "Delete Presets", "Delete selected user presets",
                        _ => from chosen in selection from deleted in Callbacks.Each(chosen.Map(Delete)) select unit),
                    Run("preset-previous", "Previous Preset", "Apply preset listed before current",
                        scope => bindings(scope).Bind(static binding => binding.Stepped(forward: false)).Map(unit)),
                    Run("preset-next", "Next Preset", "Apply preset listed after current",
                        scope => bindings(scope).Bind(static binding => binding.Stepped(forward: true)).Map(unit)),
                ],
                Wording = Wording.Library,
            },
                RowRules.Always),
        };

    private static CommandRow.Run Run(string name, string caption, string help, Func<RowScope, IO<Unit>> execute) =>
        new(new CommandFace(name, caption, help, None, None) { Wording = Wording.Library }, None, execute);

    private IO<TileRequest> Requested(EffectSource source, Preset preset) =>
        from history in RenderHistory.Current.Of(scope.Sink)
        from renderings in history.Renderings
        from newest in IO.lift(renderings.Head.ToFin(new Refused(nameof(RenderHistory.Renderings), Refused.Absent)))
        from current in bound.Capture
        from locks in scope.Locked
        from entries in EffectRegistry.Read(source)
        from kinds in EffectRegistry.Kinds.Of(scope.Sink)
        from request in IO.lift(TileRequest.Applied(newest, preset, current, locks, EffectPipeline.Ordered(entries, kinds)))
        select request;

    private IO<Unit> Picked(PresetName name) =>
        from shelf in Shelf
        from held in IO.lift(shelf.Find(name).ToFin(new NotFound("preset", name)))
        from diff in Apply(held)
        select unit;

    private IO<Unit> Saved(Option<Preset> current) =>
        from name in HostDialogs.ShowEditBox(RowText.Localize("Save Preset"), RowText.Localize("Preset name"), current.Map(static held => held.Name))
        from shelf in Shelf
        from free in IO.lift(shelf.Stock.Exists(preset => preset.Name == name) ? Error.New($"{nameof(Save)} already holds {name}") : Fin.Succ(unit))
        from saved in shelf.User.Find(name).Match(Some: held => Replaced(held), None: () => Save(name).Map(unit))
        select saved;

    private IO<Unit> Replaced(Preset preset) =>
        from answer in HostDialogs.ShowMessage(scope.Document, RowText.Localize("Replace preset {0} with section values?", arguments: [(string)preset.Name]), RowText.Localize("Replace Preset"),
            ShowMessageButton.YesNo, ShowMessageIcon.Question, ShowMessageDefaultButton.Button2, ShowMessageOptions.None, ShowMessageMode.ApplicationModal)
        from confirmed in IO.lift(guard(answer is ShowMessageResult.Yes, Errors.Cancelled).ToFin())
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
