<!-- Fully integrated into dotnet-coding-languageext [06] and [07], the conduit code of [06] also feeds its streams reference -->
# [LANGUAGEEXT_COLLECTIONS_AND_SHARED_STATE]

Every collection in domain code is a LanguageExt collection, and `Seq<A>` is the default. BCL `List<T>` or `Dictionary<K, V>` stays inside a scope publishing an immutable value, and `toSeq` is the conversion at that boundary. This file documents collection types, fold and sequence operations, lenses, and shared-state types.

<!-- Integrated into .claude/skills/dotnet-coding-languageext/SKILL.md
## [01]-[COLLECTION_TYPES]

| [INDEX] | [TYPE]          | [PURPOSE]                       | [CONSTRUCTION]                                 |
| :-----: | :-------------- | :------------------------------ | :--------------------------------------------- |
|  [01]   | `Seq<A>`        | Ordered, memoized               | `Seq(1, 2, 3)`, `toSeq(source)`                |
|  [02]   | `Arr<A>`        | Indexed reads                   | `Array(10, 20, 30)`                            |
|  [03]   | `Lst<A>`        | `Insert`, `RemoveAt`, `SetItem` | `List(1, 2, 3)`                                |
|  [04]   | `Map<K, V>`     | Keyed, ordered by key           | `Map(("b", 2), ("a", 1))`                      |
|  [05]   | `HashMap<K, V>` | Keyed, hashed                   | `HashMap(("a", 1))`, `toHashMap(pairs)`        |
|  [06]   | `Set<A>`        | Unique, ordered                 | `Set(3, 1, 2)`, `toSet(items)`                 |
|  [07]   | `HashSet<A>`    | Unique, hashed                  | `HashSet(3, 1, 2)`                             |
|  [08]   | `Iterable<A>`   | Lazy over `IEnumerable`         | `source.AsIterable()`, `ToSeq()` forces it     |
|  [09]   | `IterableNE<A>` | Non-empty, `Head` is a value    | `IterableNE.create(1, 2, 3)`, `AsIterableNE()` |

`Seq<A>` reads its source once and memoizes every item. Second enumerations do not run the source again. `toSeq` eagerly copies an array, a list, or a collection. `Map` and `Filter` on a `Seq` are deferred until enumeration. `Iterable<A>` does not memoize items. Second enumerations run the source again. `AsIterableNE` returns `Option<IterableNE<A>>` because a source can be empty. `Range(from, count)` takes a count as its second argument. `Range(1, 3)` yields `1, 2, 3`. The declared type of a hash set is `LanguageExt.HashSet<A>`, because the simple name collides with `System.Collections.Generic.HashSet<T>`.

```csharp
internal static class Partition {
    public static Seq<int> Strict => Seq(1, 2, 3);
    public static Seq<int> Converted(IEnumerable<int> source) => toSeq(source);
    public static Arr<int> Indexed => Array(10, 20, 30);
    public static int Third(Arr<int> items) => items[2];
    public static Lst<int> Editable => List(1, 2, 3);
    public static Lst<int> Edited(Lst<int> items) => items.Insert(1, 9).RemoveAt(0).SetItem(0, 7);
    public static Map<string, int> Ordered => Map(("b", 2), ("a", 1));
    public static HashMap<string, int> Hashed => HashMap(("a", 1), ("b", 2));
    public static HashMap<string, int> HashedFrom(Seq<(string, int)> pairs) => toHashMap(pairs);
    public static Set<int> Sorted => Set(3, 1, 2);
    public static Set<int> SortedFrom(Seq<int> items) => toSet(items);
    public static LanguageExt.HashSet<int> Unordered => HashSet(3, 1, 2);
    public static Seq<int> Doubled(IEnumerable<int> source) {
        Iterable<int> lazy = source.AsIterable().Map(static x => x * 2);
        return lazy.ToSeq();
    }
    public static IterableNE<int> NonEmpty => IterableNE.create(1, 2, 3);
    public static Option<IterableNE<int>> NonEmptyFrom(Seq<int> items) => items.AsIterableNE();
    public static Seq<int> Counted => toSeq(Range(1, 3));
}
```
-->

<!-- Integrated into .claude/skills/dotnet-coding-languageext/SKILL.md
## [02]-[FOLDS]

`Fold` folds left to right with a seed, and `FoldBack` folds right to left. `FoldWhile` reads the state and the next element before each step and stops when the predicate returns `false`. `FoldUntil` stops when the predicate returns `true`. Both predicates receive a `(State, Value)` tuple. `FoldM` binds each step through a monad and folds right to left, while `FoldBackM` folds left to right. Both return `K<M, S>`. Call `.As()` to convert that value to the concrete monad type. The seedless `Fold()` combines a monoid and prepends each element to the accumulator. Folding a sequence of sequences returns the groups in reverse order. `Exists` stops at the first match, and `ForAll` stops at the first failure.

```csharp
internal static class Folds {
    public static int Total(Seq<int> values) => values.Fold(0, static (sum, x) => sum + x);
    public static string Forward(Seq<int> values) => values.Fold("", static (text, x) => string.Create(CultureInfo.InvariantCulture, $"{text}{x}"));
    public static string Backward(Seq<int> values) => values.FoldBack("", static (text, x) => string.Create(CultureInfo.InvariantCulture, $"{text}{x}"));
    public static int WhileUnderTen(Seq<int> values) => values.FoldWhile(0, static (sum, x) => sum + x, static pair => pair.State < 10);
    public static int UntilNegative(Seq<int> values) => values.FoldUntil(0, static (sum, x) => sum + x, static pair => pair.Value < 0);
    public static Option<string> MonadicTailFirst(Seq<int> values) =>
        values.FoldM("", static (string text, int x) => Some(string.Create(CultureInfo.InvariantCulture, $"{text}{x}"))).As();
    public static Option<string> MonadicHeadFirst(Seq<int> values) =>
        values.FoldBackM("", static (string text, int x) => Some(string.Create(CultureInfo.InvariantCulture, $"{text}{x}"))).As();
    public static Seq<int> Joined(Seq<Seq<int>> groups) => groups.Fold();
    public static bool AnyEven(Seq<int> values, Atom<int> visited) => values.Exists(x => visited.Swap(static n => n + 1) > 0 && x % 2 == 0);
    public static bool AllPositive(Seq<int> values, Atom<int> visited) => values.ForAll(x => visited.Swap(static n => n + 1) > 0 && x > 0);
}
```
-->

<!-- Integrated into .claude/skills/dotnet-coding-languageext/SKILL.md
## [03]-[SEQUENCE_OPERATIONS]

`Choose` maps to `Option` and keeps the `Some` values in one pass. `Partition` splits by a predicate into a tuple that deconstructs. `Zip` pairs two sequences, and its projection overload takes a function. `Scan` emits the seed first. The result has one more element than the source. `At(Index)` returns `Option<A>`. `Head` and `Last` are `Option<A>`, and `Tail` is a `Seq<A>` that is empty for an empty source. The indexed `Map` passes the item first and the index second. `Bind` flattens, `Somes` drops `None`, and `Rev` reverses. `LanguageExt.List.unfold` runs a state seed until the step returns `None`. The static import of `Prelude` binds `List` to `Prelude.List`. `LanguageExt.List.unfold` names the namespace explicitly. `Cons` resolves with extension-method syntax, `head.Cons(tail)`, because `LanguageExt.Pretty.Cons<A>` is a type.

```csharp
internal static class Shapes {
    public static Seq<int> Parsed(Seq<string> texts) => texts.Choose(static text => parseInt(text));
    public static (Seq<int> Evens, Seq<int> Odds) Split(Seq<int> values) => values.Partition(static x => x % 2 == 0);
    public static Seq<(int First, string Second)> Paired(Seq<int> numbers, Seq<string> names) => numbers.Zip(names);
    public static Seq<string> Labelled(Seq<int> numbers, Seq<string> names) => numbers.Zip(names, static (n, name) => string.Create(CultureInfo.InvariantCulture, $"{n}:{name}"));
    public static Seq<int> Running(Seq<int> values) => values.Scan(0, static (sum, x) => sum + x);
    public static Option<int> Second(Seq<int> values) => values.At(1);
    public static Option<int> First(Seq<int> values) => values.Head;
    public static Seq<int> Rest(Seq<int> values) => values.Tail;
    public static Option<int> Final(Seq<int> values) => values.Last;
    public static Seq<int> Offset(Seq<int> values) => values.Map(static (x, i) => x + i);
    public static Seq<int> Positive(Seq<int> values) => values.Filter(static x => x > 0);
    public static Seq<int> Flattened(Seq<Seq<int>> groups) => groups.Bind(static group => group);
    public static Seq<int> Present(Seq<Option<int>> values) => values.Somes();
    public static Seq<int> Reversed(Seq<int> values) => values.Rev();
    public static Seq<int> Doubling(int limit) => toSeq(LanguageExt.List.unfold(1, state => state <= limit ? Some((state, state * 2)) : Option<(int, int)>.None));
    public static Seq<int> Prepended(int head, Seq<int> tail) => head.Cons(tail);
}
```
-->

<!-- Integrated into .claude/skills/dotnet-coding-languageext/SKILL.md
## [04]-[EQUALITY_PITFALLS]

`==` on `Seq<A>` compares the items in order. `Zip` names the tuple elements `First` and `Second`. Comparing this result with a `Seq` of unnamed tuples is ambiguous (CS9342). Declare the same tuple names in the expected value. `Contains`, `Sum`, and `Average` on a `Seq` are ambiguous with the LINQ extensions (CS0121). Use `Exists` for membership and `Fold` for summing values. `Seq<A>.Empty` in expression context fails with CS0119 because the simple name `Seq` binds to the `Prelude` function. Use `Seq<A>()` for the empty value. `Seq<A>` has no `Sort` instance. Sorting requires LINQ `Order()` followed by `toSeq`.

```csharp
internal static class Equality {
    public static bool SameItems(Seq<int> left, Seq<int> right) => left == right;
    public static bool SamePairs(Seq<int> numbers, Seq<string> names) {
        Seq<(int First, string Second)> expected = [(1, "a"), (2, "b")];
        return numbers.Zip(names) == expected;
    }
    public static bool Has(Seq<string> names, string name) => names.Exists(item => string.Equals(item, name, StringComparison.Ordinal));
    public static int Sum(Seq<int> values) => values.Fold(0, static (sum, x) => sum + x);
    public static Seq<int> Empty() => Seq<int>();
    public static Seq<int> Ascending(Seq<int> values) => toSeq(values.Order());
}
```
-->

<!-- Integrated into .claude/skills/dotnet-coding-languageext/SKILL.md
## [05]-[LENSES]

`Lens<A, B>.New` takes a getter and a curried setter. `Get` reads the focus. `Set` writes a value, `Update` applies a function, and both return a new `A`. `lens(outer, inner)` composes two lenses into one lens that focuses on a value in a nested record.

```csharp
internal sealed record Address(string City, string Postcode);
internal sealed record Customer(string Name, Address Address);

internal static class Lenses {
    public static readonly Lens<Customer, Address> AddressOf =
        Lens<Customer, Address>.New(static customer => customer.Address, static address => customer => customer with { Address = address });
    public static readonly Lens<Address, string> PostcodeOf =
        Lens<Address, string>.New(static address => address.Postcode, static postcode => address => address with { Postcode = postcode });
    public static readonly Lens<Customer, string> CustomerPostcode = lens(AddressOf, PostcodeOf);

    public static Customer Moved(Customer customer, string postcode) => CustomerPostcode.Set(postcode, customer);
    public static Customer Uppercased(Customer customer) => CustomerPostcode.Update(static postcode => postcode.ToUpperInvariant(), customer);
    public static string Read(Customer customer) => CustomerPostcode.Get(customer);
}
```
-->

<!-- Integrated into .claude/skills/dotnet-coding-languageext/SKILL.md
## [06]-[SHARED_STATE]

`Atom<A>` manages one value with compare-and-swap. `Swap` returns the new value. Conflicts can rerun the update function. The function must have no side effects. `SwapMaybe` keeps the state on `None` and returns the current value. `AtomHashMap<K, V>` updates in place, and its update functions must have no side effects. `TryAdd` ignores a present key. `SwapKey(key, Func<V, V>)` updates a present key, and `SwapKey(key, Func<Option<V>, Option<V>>)` also inserts. `Find` reads, and `FindOrAdd` atomically adds a missing value or returns the existing value. `Ref<A>` updates occur inside `atomic`. `swap` reads the transactional value. `commute` applies its function inside the transaction and again at the commit point against the last committed value. `atomic(Func<R>)` returns the function result from the transaction. `Isolation.Serialisable` sets serializable transaction isolation. `TrackingHashMap<K, V>` records each key change in `Changes`, and `Snapshot()` clears the change log and preserves the entries. `memo(Func<A, B>)` caches one result per argument. `memo(Func<A>)` returns a `Memo<A>`, and its `Value` runs the thunk once. `memoK` caches the construction of a `K<F, A>`, not its execution. Memoized `IO` is constructed once and runs each time `Value` is read.

```csharp
internal static class SharedState {
    public static int Increment(Atom<int> counter) => counter.Swap(static n => n + 1);
    public static int Capped(Atom<int> counter, int limit) => counter.SwapMaybe(n => n < limit ? Some(n + 1) : Option<int>.None);
    public static Unit Register(AtomHashMap<string, int> registry, string key, int value) => registry.TryAdd(key, value);
    public static Unit Bump(AtomHashMap<string, int> registry, string key) => registry.SwapKey(key, static n => n + 1);
    public static Unit BumpOrStart(AtomHashMap<string, int> registry, string key) => registry.SwapKey(key, static n => n.Map(static v => v + 1) | Some(1));
    public static Option<int> Read(AtomHashMap<string, int> registry, string key) => registry.Find(key);
    public static int ReadOrRegister(AtomHashMap<string, int> registry, string key, int value) => registry.FindOrAdd(key, value);
    public static decimal Move(Ref<decimal> source, Ref<decimal> target, decimal amount) =>
        atomic(() => {
            _ = swap(source, balance => balance - amount);
            return commute(target, balance => balance + amount);
        }, Isolation.Serialisable);
    public static TrackingHashMap<string, int> Tracked(TrackingHashMap<string, int> stock) => stock.Add("a", 1).SetItem("a", 2).Add("b", 3).Remove("b");
    public static int Logged(TrackingHashMap<string, int> stock) => stock.Changes.Count;
    public static TrackingHashMap<string, int> Cleared(TrackingHashMap<string, int> stock) => stock.Snapshot();
    public static Func<int, int> Squares(Atom<int> calls) =>
        memo((int x) => {
            _ = calls.Swap(static n => n + 1);
            return x * x;
        });
    public static Memo<int> Once(Atom<int> calls) => memo(() => calls.Swap(static n => n + 1));
    public static Memo<IO, int> Built(Atom<int> builds, Atom<int> runs) =>
        memoK<IO, int>(() => {
            _ = builds.Swap(static n => n + 1);
            return IO.lift(() => runs.Swap(static n => n + 1));
        });
}
```
-->
