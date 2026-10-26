using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Rasm.Rhino.Persistence.Stores;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record EntryKey(string Owner, Seq<string> Path) : IComparable<EntryKey> {
    private const char Separator = '.';

    public static string TypeOwner(Guid type) => type.ToString("D", CultureInfo.InvariantCulture);

    public static string Name(Seq<string> path) => string.Join(Separator, path);

    public static EntryKey Named(string owner, string name) => new(owner, toSeq(name.Split(Separator)));

    public int CompareTo(EntryKey? other) =>
        other is null ? 1
        : string.CompareOrdinal(Owner, other.Owner) is var owner and not 0 ? owner
        : Path.Zip(other.Path, string.CompareOrdinal).Find(static order => order is not 0).IfNone(Path.Count.CompareTo(other.Path.Count));

    public static bool operator <(EntryKey? left, EntryKey? right) => Comparer<EntryKey>.Default.Compare(left, right) < 0;

    public static bool operator <=(EntryKey? left, EntryKey? right) => Comparer<EntryKey>.Default.Compare(left, right) <= 0;

    public static bool operator >(EntryKey? left, EntryKey? right) => Comparer<EntryKey>.Default.Compare(left, right) > 0;

    public static bool operator >=(EntryKey? left, EntryKey? right) => Comparer<EntryKey>.Default.Compare(left, right) >= 0;
}

[JsonConverter(typeof(ValueSetConverter))]
public sealed record ValueSet(HashMap<EntryKey, string> Entries) {
    public static ValueSet Empty { get; } = new(HashMap<EntryKey, string>());

    public static DictionaryCodec<ValueSet> Codec { get; } = ArchivableDictionaries.Json<ValueSet>(nameof(ValueSet), 3);

    public static ValueSet Fields<T>(string owner, T record) =>
        new(toHashMap(Paths(record).AsIterable().Map(entry => (new EntryKey(owner, entry.Key), entry.Value))));

    public static HashMap<Seq<string>, string> Paths<T>(T record) =>
        toHashMap(Leaves(JsonSerializer.SerializeToNode(record, ArchivableDictionaries.Options), []));

    public Fin<T> Recalled<T>(string owner, T current) =>
        ArchivableDictionaries.Decoded<T>(owner, () => Merged(
                JsonSerializer.SerializeToNode(current, ArchivableDictionaries.Options),
                Grown(toSeq(Entries.AsIterable().Filter(entry => string.Equals(entry.Key.Owner, owner, StringComparison.Ordinal)).OrderBy(static entry => entry.Key)).Map(static entry => (entry.Key.Path, entry.Value)),
                    static text => JsonNode.Parse(text), static children => new JsonObject(children.Map(static child => KeyValuePair.Create(child.Name, child.Node)))))
            .Deserialize<T>(ArchivableDictionaries.Options));

    public string Text() => JsonSerializer.Serialize(this, ValueSetContext.Default.ValueSet);

    public static Fin<ValueSet> Read(string text) =>
        ArchivableDictionaries.Decoded(nameof(ValueSet), () => JsonSerializer.Deserialize(text, ValueSetContext.Default.ValueSet));

    public ValueSet Scoped(Option<IterableNE<EntryKey>> keys) =>
        keys.Match(Some: held => new ValueSet(Entries.Intersect(held)), None: () => this);

    public ValueSet Over(ValueSet current, LanguageExt.HashSet<EntryKey> locks) =>
        new(current.Entries.AddOrUpdateRange(Entries.Except(locks).AsIterable()));

    public ValueDiff Diff(ValueSet after) =>
        new(Entries.Union<string, Change<string>>(after.Entries,
                static (_, held) => Change<string>.Removed(held),
                static (_, recalled) => Change<string>.Added(recalled),
                static (_, held, recalled) => ValueStores.Between<string>(held, recalled))
            .Filter(static change => change.HasChanged));

    internal Seq<(string Name, TNode Node)> Tree<TNode>(Func<string, TNode> leaf, Func<Seq<(string Name, TNode Node)>, TNode> branch) =>
        toSeq(Entries.AsIterable().OrderBy(static entry => entry.Key).GroupBy(static entry => entry.Key.Owner, StringComparer.Ordinal))
            .Map(owner => (owner.Key, Grown(toSeq(owner).Map(static entry => (entry.Key.Path, entry.Value)), leaf, branch)));

    internal static Seq<(Seq<string> Path, string Text)> Leaves(JsonNode? node, Seq<string> path) =>
        node is JsonObject branch
            ? toSeq(branch).Bind(child => Leaves(child.Value, path.Add(child.Key)))
            : Seq((path, node?.ToJsonString() ?? "null"));

    private static JsonNode? Merged(JsonNode? held, JsonNode? stored) =>
        (held, stored) is (JsonObject kept, JsonObject read) && read.Where(static entry => entry.Key.StartsWith('$')).All(entry => JsonNode.DeepEquals(kept[entry.Key], entry.Value))
            ? new JsonObject(kept.Select(static entry => entry.Key).Union(read.Select(static entry => entry.Key), StringComparer.Ordinal)
                .Select(key => KeyValuePair.Create(key, read.ContainsKey(key) ? Merged(kept[key]?.DeepClone(), read[key]?.DeepClone()) : kept[key]?.DeepClone())))
            : stored;

    private static TNode Grown<TNode>(Seq<(Seq<string> Path, string Text)> rows, Func<string, TNode> leaf, Func<Seq<(string Name, TNode Node)>, TNode> branch) =>
        rows.Find(static row => row.Path.IsEmpty).Match(
            Some: row => leaf(row.Text),
            None: () => branch(toSeq(rows.GroupBy(static row => row.Path[0], StringComparer.Ordinal))
                .Map(group => (group.Key, Grown(toSeq(group).Map(static row => (row.Path.Tail, row.Text)), leaf, branch)))));
}

public sealed record ValueDiff(HashMap<EntryKey, Change<string>> Changes) {
    public static ValueDiff Empty { get; } = new(HashMap<EntryKey, Change<string>>());

    public static ValueDiff Of<TValue>(Change<TValue> change, TValue @default, Func<TValue, ValueSet> capture) =>
        change.HasChanged ? capture(ValueStores.Before(change).IfNone(@default)).Diff(capture(change.ToOption().IfNone(@default))) : Empty;

    public ValueSet After() =>
        new(toHashMap(toSeq(Changes.AsIterable()).Choose(static entry => entry.Value.ToOption().Map(text => (entry.Key, text)))));

    public ValueDiff Inverse() => new(Changes.Map(static change => ValueStores.Between(change.ToOption(), ValueStores.Before(change))));

    public ValueDiff Then(ValueDiff later) =>
        new(Changes.Union(later.Changes, static (_, earlier, next) => ValueStores.Between(ValueStores.Before(earlier), next.ToOption())).Filter(static change => change.HasChanged));

    public static ValueDiff Together(Seq<ValueDiff> diffs) =>
        new(diffs.Fold(HashMap<EntryKey, Change<string>>(), static (earlier, later) => earlier.Union(later.Changes)));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record ValueBinding(
    string Owner, HashMap<Seq<string>, string> Defaults, IO<HashMap<Seq<string>, string>> Capture, Func<HashMap<Seq<string>, string>, IO<IO<ValueDiff>>> Prepare) {
    public Seq<Seq<string>> Paths() => toSeq(Defaults.Keys);

    public ValueBinding Under(Seq<string> prefix) =>
        new(Owner,
            toHashMap(Defaults.AsIterable().Map(entry => (prefix + entry.Key, entry.Value))),
            Capture.Map(entries => toHashMap(entries.AsIterable().Map(entry => (prefix + entry.Key, entry.Value)))),
            entries => Prepare(toHashMap(entries.AsIterable()
                .Filter(entry => entry.Key.Take(prefix.Count).Equals(prefix))
                .Map(entry => (entry.Key.Skip(prefix.Count), entry.Value))))
                .Map(write => write.Map(diff => new ValueDiff(toHashMap(diff.Changes.AsIterable().Map(entry =>
                    (new EntryKey(entry.Key.Owner, prefix + entry.Key.Path), entry.Value)))))));

    public static ValueBinding Of<TValue>(string owner, TValue @default, ValueStore<TValue> store) where TValue : notnull =>
        new(owner, ValueSet.Paths(@default),
            store.Read.Map(held => ValueSet.Paths(held.IfNone(@default))),
            entries =>
                from held in store.Read
                from next in IO.lift(() => new ValueSet(toHashMap(entries.AsIterable().Map(entry => (new EntryKey(owner, entry.Key), entry.Value)))).Recalled(owner, held.IfNone(@default)))
                select store.Write(Some(next)).Map(change => ValueDiff.Of(change, @default, value => ValueSet.Fields(owner, value))));
}

public sealed record BindingGroup(Seq<ValueBinding> Bindings) {
    public Seq<EntryKey> Keys() =>
        from binding in Bindings
        from path in binding.Paths()
        select new EntryKey(binding.Owner, path);

    public ValueSet Defaults =>
        new(toHashMap(from binding in Bindings
                      from entry in binding.Defaults.AsIterable()
                      select (new EntryKey(binding.Owner, entry.Key), entry.Value)));

    public IO<ValueSet> Capture =>
        Callbacks.Each(Bindings.Map(static binding => binding.Capture.Map(entries => toSeq(entries.AsIterable()).Map(entry => (new EntryKey(binding.Owner, entry.Key), entry.Value)))))
            .Map(static parts => new ValueSet(toHashMap(parts.Flatten())));

    public IO<ValueDiff> Apply(ValueSet incoming, LanguageExt.HashSet<EntryKey> locks) =>
        from entries in IO.pure(incoming.Entries.Except(locks))
        from prepared in Callbacks.Each(Bindings
            .Map(binding => (Binding: binding, Entries: toHashMap(binding.Paths()
                .Choose(path => entries.Find(new EntryKey(binding.Owner, path)).Map(text => (path, text))))))
            .Filter(static held => !held.Entries.IsEmpty)
            .Map(static held => held.Binding.Prepare(held.Entries)))
        from written in Callbacks.Each(prepared)
        select ValueDiff.Together(written);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class ValueSetConverter : JsonConverter<ValueSet> {
    public override ValueSet Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(toHashMap(
            from owner in toSeq(JsonSerializer.Deserialize(ref reader, ValueSetContext.Default.JsonObject))
            from leaf in ValueSet.Leaves(owner.Value, [])
            select (new EntryKey(owner.Key, leaf.Path), leaf.Text)));

    public override void Write(Utf8JsonWriter writer, ValueSet value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, Branch(value.Tree(static text => JsonNode.Parse(text), Branch)), ValueSetContext.Default.JsonObject);

    private static JsonObject Branch(Seq<(string Name, JsonNode? Node)> children) =>
        new(children.Map(static child => KeyValuePair.Create(child.Name, child.Node)));
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Strict, WriteIndented = true)]
[JsonSerializable(typeof(ValueSet))]
[JsonSerializable(typeof(JsonObject))]
public sealed partial class ValueSetContext : JsonSerializerContext;
