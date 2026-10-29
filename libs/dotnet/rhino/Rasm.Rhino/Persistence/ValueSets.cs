using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record EntryKey(string Owner, Seq<string> Path) {
    private const char Separator = '.';

    public static string TypeOwner(Guid type) => type.ToString("D", CultureInfo.InvariantCulture);

    public static string Name(Seq<string> path) => string.Join(Separator, path);

    public static EntryKey Named(string owner, string name) => new(owner, toSeq(name.Split(Separator)));
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
        ArchivableDictionaries.Decoded(owner, () => Merged(
                JsonSerializer.SerializeToNode(current, ArchivableDictionaries.Options),
                Grown(toSeq(Enumerable.OrderBy(Entries.AsIterable().Filter(entry => string.Equals(entry.Key.Owner, owner, StringComparison.Ordinal)), static entry => EntryKey.Name(entry.Key.Path), StringComparer.Ordinal)).Map(static entry => (entry.Key.Path, entry.Value)),
                    Parsed, Branch))
            .Deserialize<T>(ArchivableDictionaries.Options));

    public string Text() => JsonSerializer.Serialize(this, ValueSetContext.Default.ValueSet);

    public static Fin<ValueSet> Read(string text) =>
        ArchivableDictionaries.Decoded(nameof(ValueSet), () => JsonSerializer.Deserialize(text, ValueSetContext.Default.ValueSet));

    public ValueSet Scoped(Option<IterableNE<EntryKey>> keys) =>
        keys.Match(Some: held => new ValueSet(Entries.Intersect(held)), None: () => this);

    public ValueSet Over(ValueSet current, HashSet<EntryKey> locks) =>
        new(current.Entries.AddOrUpdateRange(Entries.Except(locks).AsIterable()));

    public ValueDiff Diff(ValueSet after) =>
        new(Entries.Union<string, Change<string>>(after.Entries,
                static (_, held) => Change<string>.Removed(held),
                static (_, recalled) => Change<string>.Added(recalled),
                static (_, held, recalled) => ValueDiff.Between<string>(held, recalled))
            .Filter(static change => change.HasChanged));

    internal Seq<(string Name, TNode Node)> Tree<TNode>(Func<string, TNode> leaf, Func<Seq<(string Name, TNode Node)>, TNode> branch) =>
        toSeq(Enumerable.OrderBy(Entries.AsIterable(), static entry => entry.Key.Owner, StringComparer.Ordinal).ThenBy(static entry => EntryKey.Name(entry.Key.Path), StringComparer.Ordinal).GroupBy(static entry => entry.Key.Owner, StringComparer.Ordinal))
            .Map(owner => (owner.Key, Grown(toSeq(owner).Map(static entry => (entry.Key.Path, entry.Value)), leaf, branch)));

    internal static Seq<(Seq<string> Path, string Text)> Leaves(JsonNode? node, Seq<string> path) =>
        node is JsonObject branch
            ? toSeq(branch).Bind(child => Leaves(child.Value, path.Add(child.Key)))
            : Seq((path, node?.ToJsonString() ?? "null"));

    internal static readonly Func<string, JsonNode?> Parsed = static text => JsonNode.Parse(text);

    internal static JsonObject Branch(Seq<(string Name, JsonNode? Node)> children) =>
        children.Fold(new JsonObject(), static (branch, child) => {
            branch[child.Name] = child.Node;
            return branch;
        });

    private static JsonNode? Merged(JsonNode? held, JsonNode? stored) =>
        (held, stored) is (JsonObject kept, JsonObject read) && read.Where(static entry => entry.Key.StartsWith('$')).All(entry => JsonNode.DeepEquals(kept[entry.Key], entry.Value))
            ? Branch(toSeq(kept.Select(static entry => entry.Key).Union(read.Select(static entry => entry.Key), StringComparer.Ordinal))
                .Map(key => (key, read.TryGetPropertyValue(key, out JsonNode? stored) ? Merged(kept[key]?.DeepClone(), stored?.DeepClone()) : kept[key]?.DeepClone())))
            : stored;

    private static TNode Grown<TNode>(Seq<(Seq<string> Path, string Text)> rows, Func<string, TNode> leaf, Func<Seq<(string Name, TNode Node)>, TNode> branch) =>
        rows.Find(static row => row.Path.IsEmpty).Match(
            Some: row => leaf(row.Text),
            None: () => branch(toSeq(rows.GroupBy(static row => row.Path[0], StringComparer.Ordinal))
                .Map(group => (group.Key, Grown(toSeq(group).Map(static row => (row.Path.Tail, row.Text)), leaf, branch)))));
}

public sealed record ValueDiff(HashMap<EntryKey, Change<string>> Changes) {
    public static ValueDiff Empty { get; } = new(HashMap<EntryKey, Change<string>>());

    public ValueSet After() =>
        new(toHashMap(toSeq(Changes.AsIterable()).Choose(static entry => entry.Value.ToOption().Map(text => (entry.Key, text)))));

    public ValueDiff Inverse() => new(Changes.Map(static change => Between(change.ToOption(), Before(change))));

    public ValueDiff Then(ValueDiff later) =>
        new(Changes.Union(later.Changes, static (_, earlier, next) => Between(Before(earlier), next.ToOption())).Filter(static change => change.HasChanged));

    public static ValueDiff Together(Seq<ValueDiff> diffs) =>
        new(diffs.Fold(HashMap<EntryKey, Change<string>>(), static (earlier, later) => earlier.Union(later.Changes)));

    internal static Change<T> Between<T>(Option<T> before, Option<T> after) =>
        (before.Case, after.Case) switch {
            (T held, T written) => Equals(held, written) ? Change<T>.None : Change<T>.Mapped(held, written),
            (T held, _) => Change<T>.Removed(held),
            (_, T written) => Change<T>.Added(written),
            _ => Change<T>.None,
        };

    private static Option<T> Before<T>(Change<T> change) =>
        change switch {
            EntryRemoved<T> removed => removed.OldValue,
            EntryMapped<T, T> mapped => mapped.From,
            _ => None,
        };
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record ValueBinding(
    string Owner, HashMap<Seq<string>, string> Defaults, IO<HashMap<Seq<string>, string>> Capture, Func<HashMap<Seq<string>, string>, IO<IO<ValueDiff>>> Prepare) {
    public Seq<Seq<string>> Paths() => toSeq(Defaults.Keys);

    public ValueBinding Prefixed(Seq<string> prefix) =>
        new(Owner,
            toHashMap(Defaults.AsIterable().Map(entry => (prefix + entry.Key, entry.Value))),
            Capture.Map(entries => toHashMap(entries.AsIterable().Map(entry => (prefix + entry.Key, entry.Value)))),
            entries => Prepare(toHashMap(entries.AsIterable()
                .Filter(entry => entry.Key.Take(prefix.Count).Equals(prefix))
                .Map(entry => (entry.Key.Skip(prefix.Count), entry.Value))))
                .Map(write => write.Map(diff => new ValueDiff(toHashMap(diff.Changes.AsIterable().Map(entry =>
                    (new EntryKey(entry.Key.Owner, prefix + entry.Key.Path), entry.Value)))))));

    public static ValueBinding Of<TValue>(string owner, TValue @default, Preference<TValue> preference) where TValue : notnull =>
        new(owner, ValueSet.Paths(@default),
            preference.Read.Map(static held => ValueSet.Paths(held)),
            entries =>
                from held in preference.Read
                from next in IO.lift(() => new ValueSet(toHashMap(entries.AsIterable().Map(entry => (new EntryKey(owner, entry.Key), entry.Value)))).Recalled(owner, held))
                select preference.Write(next).Map(_ => ValueSet.Fields(owner, held).Diff(ValueSet.Fields(owner, next))));
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
        Products.Each(Bindings.Map(static binding => binding.Capture.Map(entries => toSeq(entries.AsIterable()).Map(entry => (new EntryKey(binding.Owner, entry.Key), entry.Value)))))
            .Map(static parts => new ValueSet(toHashMap(parts.Flatten())));

    public IO<ValueDiff> Apply(ValueSet incoming, HashSet<EntryKey> locks) =>
        from entries in IO.pure(incoming.Entries.Except(locks))
        from prepared in Products.Each(Bindings
            .Map(binding => (Binding: binding, Entries: toHashMap(binding.Paths()
                .Choose(path => entries.Find(new EntryKey(binding.Owner, path)).Map(text => (path, text))))))
            .Filter(static held => !held.Entries.IsEmpty)
            .Map(static held => held.Binding.Prepare(held.Entries)))
        from written in Products.Each(prepared)
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
        JsonSerializer.Serialize(writer, ValueSet.Branch(value.Tree(ValueSet.Parsed, ValueSet.Branch)), ValueSetContext.Default.JsonObject);
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Strict, WriteIndented = true)]
[JsonSerializable(typeof(ValueSet))]
[JsonSerializable(typeof(JsonObject))]
public sealed partial class ValueSetContext : JsonSerializerContext;
