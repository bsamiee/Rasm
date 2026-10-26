using System.Buffers;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence.Stores;
using Rhino.PlugIns;
using Rhino.UI;

namespace Rasm.Rhino.Persistence;

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
public sealed record PresetBinding(DirectoryInfo Folder, Seq<Preset> Stock, IO<ValueSet> Shown, IO<LanguageExt.HashSet<EntryKey>> Locked, Func<ValueSet, IO<ValueDiff>> Commit) {
    private const string Extension = ".json";

    public static DirectoryInfo Under(PlugIn plugIn, string owner) => new(Path.Combine(plugIn.SettingsDirectory, "Presets", owner));

    // --- [READS]
    public IO<PresetShelf> Shelf =>
        from saved in Listed(Folder)
        select new PresetShelf(Stock, saved);

    public IO<Option<Preset>> Current => Shelf.Bind(Matched);

    private IO<Option<Preset>> Matched(PresetShelf shelf) =>
        from captured in Shown
        from locks in Locked
        select shelf.Current(captured, locks);

    // --- [WRITES]
    public IO<ValueDiff> Apply(Preset preset) => IO.lift(preset.Values).Bind(Commit);

    public IO<ValueDiff> Stepped(int steps) =>
        from shelf in Shelf
        from current in Matched(shelf)
        let listed = shelf.Listed
        let at = current.Map(held => listed.TakeWhile(preset => preset != held).Count).IfNone(steps > 0 ? -1 : 0)
        from diff in (listed.IsEmpty ? Option<Preset>.None : listed.At((((at + steps) % listed.Count) + listed.Count) % listed.Count))
            .Match(Some: Apply, None: static () => IO.pure(ValueDiff.Empty))
        select diff;

    public IO<Preset> Save(PresetName name) => Added(name, Shown, nameof(Save));

    public IO<Preset> Replace(Preset preset) =>
        from file in IO.lift(preset.File.ToFin(new NotFound("preset", preset.Name)))
        from values in Shown
        from written in Written(file, values, overwrite: true)
        select preset with { Values = values };

    public IO<Preset> Rename(Preset preset, PresetName to) =>
        from shelf in Shelf
        from file in IO.lift(preset.File.ToFin(new NotFound("preset", preset.Name)))
        from free in IO.lift(shelf.Find(to).ForAll(other => other.File.Exists(held => string.Equals(held.FullName, file.FullName, StringComparison.Ordinal))) ? Fin.Succ(unit) : Error.New($"{nameof(Rename)} already holds {to}"))
        let target = Named(to)
        from moved in IO.lift(() => File.Move(file.FullName, target.FullName))
        select preset with { Name = to, File = Some(target) };

    public static IO<Unit> Delete(Preset preset) =>
        from file in IO.lift(preset.File.ToFin(new NotFound("preset", preset.Name)))
        from deleted in IO.lift(file.Delete)
        select deleted;

    public IO<Preset> Import(FileInfo source) =>
        IO.lift(PresetName.Validated(Path.GetFileNameWithoutExtension(source.Name))).Bind(name => Added(name, IO.lift(() => Read(source)), nameof(Import)));

    public static IO<Unit> Export(Preset preset, FileInfo target) =>
        IO.lift(() => preset.File.Match(Some: Read, None: () => preset.Values)).Bind(values => Written(target, values, overwrite: true));

    private IO<Preset> Added(PresetName name, IO<ValueSet> read, string member) =>
        from shelf in Shelf
        from free in IO.lift(shelf.Find(name).IsNone ? Fin.Succ(unit) : Error.New($"{member} already holds {name}"))
        from values in read
        let file = Named(name)
        from written in Written(file, values, overwrite: false)
        select new Preset(name, values) { File = Some(file) };

    // --- [FILES]
    private FileInfo Named(PresetName name) => new(Path.Combine(Folder.FullName, name + Extension));

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
            .Bind(text => ValueSet.Read(text).MapFail(cause => new FileRefused(file.FullName, FileReason.Unreadable, cause)));

    private static IO<Unit> Written(FileInfo file, ValueSet values, bool overwrite) =>
        IO.lift(OutputPath.Of(file.FullName)).Bind(path => JsonFiles.Write(path, values, ValueSetContext.Default.ValueSet, overwrite));
}
