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
            : Path.Zip(other.Path, string.CompareOrdinal).Find(static order => order != 0).IfNone(Path.Count.CompareTo(other.Path.Count));

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
                static (_, held) => ValueDiff.Between(held, None),
                static (_, recalled) => ValueDiff.Between(None, recalled),
                static (_, held, recalled) => ValueDiff.Between(held, recalled))
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

    public ValueSet After() =>
        new(toHashMap(toSeq(Changes.AsIterable()).Choose(static entry => entry.Value.ToOption().Map(text => (entry.Key, text)))));

    public ValueDiff Inverse() => new(Changes.Map(static change => Between(change.ToOption(), Held(change))));

    public ValueDiff Then(ValueDiff later) =>
        new(Changes.Union(later.Changes, static (_, earlier, next) => Between(Held(earlier), next.ToOption())).Filter(static change => change.HasChanged));

    internal static Change<string> Between(Option<string> before, Option<string> after) =>
        (before.Case, after.Case) switch {
            (string held, string recalled) => string.Equals(held, recalled, StringComparison.Ordinal) ? Change<string>.None : Change<string>.Mapped(held, recalled),
            (string held, _) => Change<string>.Removed(held),
            (_, string recalled) => Change<string>.Added(recalled),
            _ => Change<string>.None,
        };

    private static Option<string> Held(Change<string> change) =>
        change switch {
            EntryRemoved<string> removed => removed.OldValue,
            EntryMapped<string, string> mapped => mapped.From,
            _ => None,
        };
}

public sealed record NameText(Seq<string> Path, string Caption, Func<string, string> Shown);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record ValueBinding(
    string Owner, Seq<NameText> Texts, IO<HashMap<Seq<string>, string>> Capture, Func<HashMap<Seq<string>, string>, Validation<Error, IO<Unit>>> Recall) {
    public Seq<Seq<string>> Paths() => Texts.Map(static text => text.Path);

    public ValueBinding Under(Seq<string> prefix) =>
        new(Owner,
            Texts.Map(text => text with { Path = prefix + text.Path }),
            Capture.Map(entries => toHashMap(entries.AsIterable().Map(entry => (prefix + entry.Key, entry.Value)))),
            entries => Recall(toHashMap(entries.AsIterable()
                .Filter(entry => entry.Key.Take(prefix.Count).Equals(prefix))
                .Map(entry => (entry.Key.Skip(prefix.Count), entry.Value)))));

    public static ValueBinding Key<TValue, TRaw, TError>(string owner, ValueKey<TValue, TRaw, TError> key, ValueStore<TValue> store)
        where TValue : notnull
        where TRaw : notnull =>
        Seq(key.Name) switch {
            var path => new(owner, [new NameText(path, key.Caption, static text => text)],
                store.Read.Map(held => HashMap((path, key.Text(held.IfNone(key.Default))))),
                entries => entries.Find(path).Match(
                    Some: text => new EntryKey(owner, path).Read(text, key.Read).ToValidation().Map(value => store.Write(Some(value)).Map(static _ => unit)),
                    None: static () => IO.pure(unit))),
        };

    public static ValueBinding Fields<TValue>(string owner, ValueStore<TValue> store, TValue @default, Seq<(IStateParameter<TValue> Parameter, string Caption)> fields)
        where TValue : notnull =>
        (from field in fields from text in FieldTexts.Of(field.Parameter) select (Name: new NameText(text.Path, field.Caption, text.Shown), Text: text)).Strict() switch {
            var items => new(owner, items.Map(static item => item.Name),
                store.Read.Map(held => held.IfNone(@default)).Map(value => toHashMap(items.Choose(item => item.Text.Capture(value).Map(text => (item.Text.Path, text))))),
                entries => items
                    .Choose(item => entries.Find(item.Text.Path).Map(text => new EntryKey(owner, item.Text.Path).Read(text, item.Text.Recall).ToValidation()))
                    .Traverse(static step => step)
                    .As()
                    .Map(steps =>
                        from sets in steps.Traverse(static step => step).As()
                        from held in store.Read
                        from written in store.Write(Some(sets.Fold(held.IfNone(@default), static (value, set) => set(value))))
                        select unit)),
        };
}

public sealed record BindingGroup {
    private BindingGroup(Seq<ValueBinding> bindings) => Bindings = bindings;

    public Seq<ValueBinding> Bindings { get; }

    public Seq<EntryKey> Keys() =>
        from binding in Bindings
        from path in binding.Paths()
        select new EntryKey(binding.Owner, path);

    public IO<ValueSet> Capture =>
        Bindings.TraverseM(static binding => binding.Capture.Map(entries => toSeq(entries.AsIterable()).Map(entry => (new EntryKey(binding.Owner, entry.Key), entry.Value))))
            .As()
            .Map(static parts => new ValueSet(toHashMap(parts.Flatten())));

    public static Validation<Error, BindingGroup> Of(Seq<ValueBinding> bindings) =>
        new BindingGroup(bindings) switch {
            var group => Callbacks.Unique(group.Keys(), identity, nameof(BindingGroup)).Map(_ => group),
        };

    public Option<NameText> Text(EntryKey key) =>
        Bindings.Filter(binding => string.Equals(binding.Owner, key.Owner, StringComparison.Ordinal)).Bind(static binding => binding.Texts).Find(text => text.Path.Equals(key.Path));

    public IO<ValueDiff> Apply(ValueSet incoming, LanguageExt.HashSet<EntryKey> locks) =>
        from before in Capture
        let pending = before.Diff(new ValueSet(incoming.Entries.Intersect(Keys())).Over(before, locks))
        from writes in IO.lift(Bindings
            .Map(binding => (Binding: binding, Entries: Changed(pending, binding)))
            .Filter(static held => !held.Entries.IsEmpty)
            .Traverse(static held => held.Binding.Recall(held.Entries))
            .As()
            .ToFin())
        from written in writes.TraverseM(static write => write).As()
        from after in Capture
        select before.Diff(after);

    private static HashMap<Seq<string>, string> Changed(ValueDiff diff, ValueBinding binding) =>
        toHashMap(binding.Paths().Choose(path => diff.Changes.Find(new EntryKey(binding.Owner, path)).Bind(static change => change.ToOption()).Map(text => (path, text))));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal sealed class ValueSetConverter : JsonConverter<ValueSet> {
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
            JsonValue text when text.GetValueKind() == JsonValueKind.String => Seq((path.Add(child.Key), text.GetValue<string>())),
            _ => throw new JsonException(),
        }
        select leaf;
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Strict, WriteIndented = true, Converters = [typeof(ValueSetConverter)])]
[JsonSerializable(typeof(ValueSet))]
[JsonSerializable(typeof(JsonObject))]
internal sealed partial class ValueSetContext : JsonSerializerContext;
