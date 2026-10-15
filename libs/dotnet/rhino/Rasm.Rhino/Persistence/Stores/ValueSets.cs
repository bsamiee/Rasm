using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Rasm.Imaging.Pixels;
using Rhino.Collections;

namespace Rasm.Rhino.Persistence.Stores;

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct EntryKey(string Owner, Seq<string> Path) : IComparable<EntryKey> {
    private const char Separator = '.';

    public static string TypeOwner(Guid type) => type.ToString("D", CultureInfo.InvariantCulture);

    public static string Name(Seq<string> path) => string.Join(Separator, path);

    public static EntryKey Named(string owner, string name) => new(owner, toSeq(name.Split(Separator)));

    public int CompareTo(EntryKey other) =>
        string.CompareOrdinal(Owner, other.Owner) is var owner and not 0
            ? owner
            : Path.Zip(other.Path, string.CompareOrdinal).Find(static order => order is not 0).IfNone(Path.Count.CompareTo(other.Path.Count));

    public Fin<TValue> Read<TValue>(string text, Func<string, Fin<TValue>> read) =>
        this switch {
            var key => read(text).MapFail(cause => new RefusedEntry(key, text, cause)),
        };

    public static bool operator <(EntryKey left, EntryKey right) => left.CompareTo(right) < 0;

    public static bool operator <=(EntryKey left, EntryKey right) => left.CompareTo(right) <= 0;

    public static bool operator >(EntryKey left, EntryKey right) => left.CompareTo(right) > 0;

    public static bool operator >=(EntryKey left, EntryKey right) => left.CompareTo(right) >= 0;
}

public sealed record ValueSet(HashMap<EntryKey, string> Entries) {
    public static ValueSet Empty { get; } = new(HashMap<EntryKey, string>());

    public static DictionaryCodec<ValueSet> Codec { get; } = new("ValueSet", 1, Encoded, Decoded);

    public static ValueSet Fields<TRecord>(string owner, TRecord record, Seq<IStateParameter<TRecord>> parameters) where TRecord : notnull =>
        new(toHashMap(parameters.Bind(FieldTexts.Of).Choose(field => field.Capture(record).Map(text => (new EntryKey(owner, field.Path), text)))));

    public string Text() => JsonSerializer.Serialize(this, ValueSetContext.Default.ValueSet);

    public static Fin<ValueSet> Read(string text) =>
        Try.lift(() => JsonSerializer.Deserialize(text, ValueSetContext.Default.ValueSet)).Run()
            .BindFail(error => error.HasException<JsonException>() ? new UnreadText(text, typeof(ValueSet), Some(error)) : error)
            .Bind(read => Optional(read).ToFin(new UnreadText(text, typeof(ValueSet))));

    public ValueSet Scoped(Option<IterableNE<EntryKey>> keys) =>
        keys.Match(Some: held => new ValueSet(Entries.Intersect(held)), None: () => this);

    public ValueSet Over(ValueSet current, LanguageExt.HashSet<EntryKey> locks) =>
        new(current.Entries.AddOrUpdateRange(Entries.Except(locks).AsIterable()));

    public ValueDiff Diff(ValueSet after) =>
        new(Entries.Union<string, Change<string>>(after.Entries,
                static (_, held) => Change<string>.Removed(held),
                static (_, recalled) => Change<string>.Added(recalled),
                static (_, held, recalled) => ValueStore.Between<string>(held, recalled))
            .Filter(static change => change.HasChanged));

    internal Seq<(string Name, TNode Node)> Tree<TNode>(Func<string, TNode> leaf, Func<Seq<(string Name, TNode Node)>, TNode> branch) =>
        toSeq(Entries.AsIterable().OrderBy(static entry => entry.Key).GroupBy(static entry => entry.Key.Owner, StringComparer.Ordinal))
            .Map(owner => (owner.Key, Grown(toSeq(owner).Map(static entry => (entry.Key.Path, entry.Value)), leaf, branch)));

    private static TNode Grown<TNode>(Seq<(Seq<string> Path, string Text)> rows, Func<string, TNode> leaf, Func<Seq<(string Name, TNode Node)>, TNode> branch) =>
        rows.Find(static row => row.Path.IsEmpty).Match(
            Some: row => leaf(row.Text),
            None: () => branch(toSeq(rows.GroupBy(static row => row.Path[0], StringComparer.Ordinal))
                .Map(group => (group.Key, Grown(toSeq(group).Map(static row => (row.Path.Tail, row.Text)), leaf, branch)))));

    private static Fin<ArchivableDictionary> Encoded(ValueSet set) =>
        Filled(set.Tree(static text => Fin.Succ(Left<string, ArchivableDictionary>(text)), static children => Filled(children).Map(static held => Right<string, ArchivableDictionary>(held))));

    private static Fin<ArchivableDictionary> Filled(Seq<(string Name, Fin<Either<string, ArchivableDictionary>> Node)> children) =>
        from target in Fin.Succ(new ArchivableDictionary())
        from written in Callbacks.Each(children, (child, _) => child.Node.Bind(node => node.Match(
            Left: text => ArchivableDictionaries.Set(target, child.Name, text),
            Right: held => ArchivableDictionaries.Set(target, child.Name, held))))
        select target;

    private static Fin<ValueSet> Decoded(ArchivableDictionary source) =>
        from owners in ArchivableDictionaries.Children(source)
        from parts in Callbacks.Each(owners, static (owner, _) => Leaves(owner.Key, owner.Value, []))
        select new ValueSet(toHashMap(parts.Flatten()));

    private static Fin<Seq<(EntryKey Key, string Text)>> Leaves(string owner, ArchivableDictionary source, Seq<string> path) =>
        Callbacks.Each(toSeq(source), (entry, _) => entry.Value switch {
            string text => Fin.Succ(Seq((new EntryKey(owner, path.Add(entry.Key)), text))),
            ArchivableDictionary child => Leaves(owner, child, path.Add(entry.Key)),
            var held => new TypeMismatch(entry.Key, held.GetType(), typeof(string)),
        }).Map(static parts => parts.Flatten());
}

public sealed record ValueDiff(HashMap<EntryKey, Change<string>> Changes) {
    public static ValueDiff Empty { get; } = new(HashMap<EntryKey, Change<string>>());

    public static ValueDiff Of<TValue>(Change<TValue> change, TValue @default, Func<TValue, ValueSet> capture) =>
        change.HasChanged ? capture(ValueStore.Before(change).IfNone(@default)).Diff(capture(change.ToOption().IfNone(@default))) : Empty;

    public ValueSet After() =>
        new(toHashMap(toSeq(Changes.AsIterable()).Choose(static entry => entry.Value.ToOption().Map(text => (entry.Key, text)))));

    public ValueDiff Inverse() => new(Changes.Map(static change => ValueStore.Between(change.ToOption(), ValueStore.Before(change))));

    public ValueDiff Then(ValueDiff later) =>
        new(Changes.Union(later.Changes, static (_, earlier, next) => ValueStore.Between(ValueStore.Before(earlier), next.ToOption())).Filter(static change => change.HasChanged));

    public static ValueDiff Together(Seq<ValueDiff> diffs) =>
        new(diffs.Fold(HashMap<EntryKey, Change<string>>(), static (earlier, later) => earlier.Union(later.Changes)));
}

public sealed record NameText(Seq<string> Path, string Caption, Func<string, string> Shown);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record ValueBinding(
    string Owner, Seq<NameText> Texts, Func<HashMap<Seq<string>, string>> Defaults, IO<HashMap<Seq<string>, string>> Capture, Func<HashMap<Seq<string>, string>, IO<IO<ValueDiff>>> Prepare) {
    public Seq<Seq<string>> Paths() => Texts.Map(static text => text.Path);

    public ValueBinding Under(Seq<string> prefix) =>
        new(Owner,
            Texts.Map(text => text with { Path = prefix + text.Path }),
            () => toHashMap(Defaults().AsIterable().Map(entry => (prefix + entry.Key, entry.Value))),
            Capture.Map(entries => toHashMap(entries.AsIterable().Map(entry => (prefix + entry.Key, entry.Value)))),
            entries => Prepare(toHashMap(entries.AsIterable()
                .Filter(entry => entry.Key.Take(prefix.Count).Equals(prefix))
                .Map(entry => (entry.Key.Skip(prefix.Count), entry.Value))))
                .Map(write => write.Map(diff => new ValueDiff(toHashMap(diff.Changes.AsIterable().Map(entry =>
                    (new EntryKey(entry.Key.Owner, prefix + entry.Key.Path), entry.Value)))))));

    public static ValueBinding Key<TValue, TRaw>(string owner, ValueKey<TValue, TRaw> key, ValueStore<TValue> store)
        where TValue : notnull
        where TRaw : notnull =>
        Seq(key.Name) switch {
            var path => new(owner, [new NameText(path, key.Caption, static text => text)],
                () => HashMap((path, key.Text(key.Default))),
                store.Read.Map(held => HashMap((path, key.Text(held.IfNone(key.Default))))),
                entries => entries.Find(path).Match(
                    Some: text => IO.lift(new EntryKey(owner, path).Read(text, key.Read))
                        .Map(value => store.Write(Some(value)).Map(change => ValueDiff.Of(change, key.Default,
                            held => new ValueSet(HashMap((new EntryKey(owner, path), key.Text(held))))))),
                    None: static () => IO.pure(IO.pure(ValueDiff.Empty)))),
        };

    public static ValueBinding Fields<TValue>(string owner, ValueStore<TValue> store, TValue @default, Seq<(IStateParameter<TValue> Parameter, string Caption)> fields)
        where TValue : notnull =>
        (from field in fields from text in FieldTexts.Of(field.Parameter) select (Name: new NameText(text.Path, field.Caption, text.Shown), Text: text)).Strict() switch {
            var items => fun((TValue value) => toHashMap(items.Choose(item => item.Text.Capture(value).Map(text => (item.Text.Path, text))))) switch {
                var capture => new(owner, items.Map(static item => item.Name),
                    () => capture(@default),
                    store.Read.Map(held => held.IfNone(@default)).Map(capture),
                    entries =>
                        from held in store.Read
                        from next in FieldTexts.Recalled(owner, fields.Map(static field => field.Parameter), path => IO.pure(entries.Find(path)), held.IfNone(@default))
                        select store.Write(Some(next)).Map(change => ValueDiff.Of(change, @default,
                            value => new ValueSet(toHashMap(capture(value).AsIterable().Map(entry => (new EntryKey(owner, entry.Key), entry.Value))))))),
            },
        };
}

public sealed record BindingGroup(Seq<ValueBinding> Bindings) {
    public Seq<EntryKey> Keys() =>
        from binding in Bindings
        from path in binding.Paths()
        select new EntryKey(binding.Owner, path);

    public ValueSet Defaults =>
        new(toHashMap(from binding in Bindings
                      from entry in binding.Defaults().AsIterable()
                      select (new EntryKey(binding.Owner, entry.Key), entry.Value)));

    public IO<ValueSet> Capture =>
        Callbacks.Each(Bindings.Map(static binding => binding.Capture.Map(entries => toSeq(entries.AsIterable()).Map(entry => (new EntryKey(binding.Owner, entry.Key), entry.Value)))))
            .Map(static parts => new ValueSet(toHashMap(parts.Flatten())));

    public Option<NameText> Text(EntryKey key) =>
        Bindings.Filter(binding => string.Equals(binding.Owner, key.Owner, StringComparison.Ordinal)).Bind(static binding => binding.Texts).Find(text => text.Path.Equals(key.Path));

    public IO<ValueDiff> Apply(ValueSet incoming, LanguageExt.HashSet<EntryKey> locks) =>
        from entries in IO.pure(incoming.Entries.Except(locks))
        from prepared in Bindings
            .Map(binding => (Binding: binding, Entries: toHashMap(binding.Paths()
                .Choose(path => entries.Find(new EntryKey(binding.Owner, path)).Map(text => (path, text))))))
            .Filter(static held => !held.Entries.IsEmpty)
            .Map<K<IO, IO<ValueDiff>>>(static held => held.Binding.Prepare(held.Entries))
            .PartitionFallible().As()
        from accepted in unless(prepared.Fails.IsEmpty, IO.fail<Unit>(Error.Many(prepared.Fails))).As()
        from written in Callbacks.Each(prepared.Succs)
        select ValueDiff.Together(written);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class ValueSetConverter : JsonConverter<ValueSet> {
    public override ValueSet Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(toHashMap(
            from owner in toSeq(JsonSerializer.Deserialize(ref reader, ValueSetContext.Default.JsonObject))
            from leaf in Leaves(owner.Value as JsonObject ?? throw new JsonException(), [])
            select (new EntryKey(owner.Key, leaf.Path), leaf.Text)));

    public override void Write(Utf8JsonWriter writer, ValueSet value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, Branch(value.Tree<JsonNode?>(static text => JsonValue.Create(text), Branch)), ValueSetContext.Default.JsonObject);

    private static JsonObject Branch(Seq<(string Name, JsonNode? Node)> children) =>
        new(children.Map(static child => KeyValuePair.Create(child.Name, child.Node)));

    private static Seq<(Seq<string> Path, string Text)> Leaves(JsonObject node, Seq<string> path) =>
        from child in toSeq(node)
        from leaf in child.Value switch {
            JsonObject branch => Leaves(branch, path.Add(child.Key)),
            JsonValue text when text.GetValueKind() is JsonValueKind.String => Seq((path.Add(child.Key), text.GetValue<string>())),
            _ => throw new JsonException(),
        }
        select leaf;
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Strict, WriteIndented = true, Converters = [typeof(ValueSetConverter)])]
[JsonSerializable(typeof(ValueSet))]
[JsonSerializable(typeof(JsonObject))]
public sealed partial class ValueSetContext : JsonSerializerContext;
