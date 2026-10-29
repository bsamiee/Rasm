using Rasm.Rhino.Document.Files;
using Rhino.PlugIns;
using Rhino.UI;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>(SkipIParsable = true)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[ValidationError<RhinoFault>]
public sealed partial class PresetName {
    public static Fin<PresetName> Validated(string value) => TryCreate(value, out PresetName? name, out RhinoFault? fault) ? name : fault;

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref string value) =>
        (value, validationError) = value.TrimOrNullify() is { } trimmed && !trimmed.AsSpan().ContainsAny(NamePart.Reserved)
            ? (trimmed, default(RhinoFault?))
            : (value, new RhinoFault(value, RhinoReason.Unqualified));
}

public sealed record Preset(PresetName Name, Fin<ValueSet> Values) {
    public Option<FileInfo> File { get; internal init; }

    public static Preset Of(PresetName name, Seq<ValueSet> parts) =>
        new(name, new ValueSet(toHashMap(parts.Bind(static part => toSeq(part.Entries.AsIterable())))));
}

public sealed record PresetShelf(Seq<Preset> Stock, Map<PresetName, Preset> User) {
    public Seq<Preset> Listed => Stock + Sorted(toSeq(User.Filter((name, _) => Stock.ForAll(preset => preset.Name != name)).Values));

    public Option<Preset> Find(PresetName name) => Stock.Find(preset => preset.Name == name) | User.Find(name);

    public Option<Preset> Current(ValueSet captured, HashSet<EntryKey> locks) =>
        Listed.Find(preset => preset.Values.Exists(values => Matches(values, captured, locks)));

    public static ValueDiff Preview(ValueSet values, ValueSet captured, HashSet<EntryKey> locks) =>
        captured.Diff(new ValueSet(values.Entries.Intersect(captured.Entries.Keys)).Over(captured, locks));

    public static bool Matches(ValueSet values, ValueSet captured, HashSet<EntryKey> locks) =>
        values.Entries.Exists((key, _) => captured.Entries.ContainsKey(key) && !locks.Contains(key)) && Preview(values, captured, locks).Changes.IsEmpty;

    private static Seq<Preset> Sorted(Seq<Preset> presets) =>
        toSeq(presets.OrderBy(static preset => (string)preset.Name, System.Collections.Generic.Comparer<string>.Create(Localization.LogicalSort)));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record PresetBinding(DirectoryInfo Folder, Seq<Preset> Stock, IO<ValueSet> Shown, IO<HashSet<EntryKey>> Locked, Func<ValueSet, IO<ValueDiff>> Commit) {
    private const string Extension = ".json";

    public static DirectoryInfo SettingsFolder(PlugIn plugIn, string owner) => new(Path.Combine(plugIn.SettingsDirectory, "Presets", owner));

    // --- [READS]
    public IO<PresetShelf> Shelf =>
        from saved in Listed(Folder)
        select new PresetShelf(Stock, saved);

    public IO<Option<Preset>> Current => Shelf.Bind(CurrentIn);

    private IO<Option<Preset>> CurrentIn(PresetShelf shelf) =>
        from captured in Shown
        from locks in Locked
        select shelf.Current(captured, locks);

    // --- [WRITES]
    public IO<ValueDiff> Apply(Preset preset) => IO.lift(preset.Values).Bind(Commit);

    public IO<ValueDiff> Stepped(int steps) =>
        from shelf in Shelf
        from current in CurrentIn(shelf)
        let listed = shelf.Listed
        let at = current.Map(held => listed.TakeWhile(preset => preset != held).Count).IfNone(steps > 0 ? -1 : 0)
        from diff in (listed.IsEmpty ? Option<Preset>.None : listed.At((((at + steps) % listed.Count) + listed.Count) % listed.Count))
            .Match(Some: Apply, None: static () => IO.pure(ValueDiff.Empty))
        select diff;

    public IO<Preset> Save(PresetName name) => Added(name, Shown);

    public IO<Preset> Replace(Preset preset) =>
        from file in IO.lift(preset.File.ToFin(new RhinoFault(preset.Name, RhinoReason.Missing)))
        from values in Shown
        from written in Written(file, values, overwrite: true)
        select preset with { Values = values };

    public IO<Preset> Rename(Preset preset, PresetName to) =>
        from shelf in Shelf
        from file in IO.lift(preset.File.ToFin(new RhinoFault(preset.Name, RhinoReason.Missing)))
        from free in IO.lift(shelf.Find(to).ForAll(other => other.File.Exists(held => string.Equals(held.FullName, file.FullName, StringComparison.Ordinal))) ? Fin.Succ(unit) : new RhinoFault(to, RhinoReason.Taken))
        let target = Named(to)
        from moved in IO.lift(() => File.Move(file.FullName, target.FullName))
        select preset with { Name = to, File = Some(target) };

    public static IO<Unit> Delete(Preset preset) =>
        from file in IO.lift(preset.File.ToFin(new RhinoFault(preset.Name, RhinoReason.Missing)))
        from deleted in IO.lift(file.Delete)
        select deleted;

    public IO<Preset> Import(FileInfo source) =>
        IO.lift(PresetName.Validated(Path.GetFileNameWithoutExtension(source.Name))).Bind(name => Added(name, IO.lift(() => Read(source))));

    public static IO<Unit> Export(Preset preset, FileInfo target) =>
        IO.lift(() => preset.File.Match(Some: Read, None: () => preset.Values)).Bind(values => Written(target, values, overwrite: true));

    private IO<Preset> Added(PresetName name, IO<ValueSet> read) =>
        from shelf in Shelf
        from free in IO.lift(shelf.Find(name).IsNone ? Fin.Succ(unit) : new RhinoFault(name, RhinoReason.Taken))
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
            .Bind(static rows => IO.lift(Conversions.Unique(rows, static row => row.Name)))
            .Map(static rows => toMap(rows));

    private static Fin<ValueSet> Read(FileInfo file) =>
        Try.lift(() => File.ReadAllText(file.FullName)).Run()
            .Bind(text => ValueSet.Read(text).MapFail(cause => new RhinoFault(file.FullName, RhinoReason.Unreadable, Inner: cause)));

    private static IO<Unit> Written(FileInfo file, ValueSet values, bool overwrite) =>
        IO.lift(OutputPath.Of(file.FullName)).Bind(path => FilePublication.Write(path, values, ValueSetContext.Default.ValueSet, overwrite));
}
