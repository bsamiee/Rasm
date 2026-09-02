# [CONVENIENCE_METHODS]

`Thinktecture.Runtime.Extensions` provides a small set of plain static members beside its generators:
- `Empty` and `SingleItem` return cached or single-item collections
- `ToReadOnlyCollection` wraps a sequence without copying it, and `TrimOrNullify` normalizes text

## [01]-[EMPTY]

`Empty.Action` is an overload set, `Action()` through `Action<T1, ..., T16>`, each with an empty body. Method group conversions pick the overload matching the target delegate, `Empty.Action` assigns to every `Action` delegate type in that range. `Empty.Disposable()` returns one cached `IDisposable` with a no-op `Dispose`, and `Empty.AsyncDisposable()` returns one cached `IAsyncDisposable` with a `DisposeAsync` returning a completed `ValueTask`.

Spell `Thinktecture.Empty` where a `using static` import of another `Empty` member makes the plain name ambiguous. The collection members return cached instances, and repeated calls with the same type arguments return the same reference:
- `Empty.Collection()` returns the non-generic `System.Collections.IEnumerable`
- `Empty.Collection<T>()` returns `IReadOnlyList<T>` backed by `Array.Empty<T>()`, and a parameter of type `IList<T>` needs an explicit cast
- `Empty.Dictionary<TKey, TValue>()` requires `TKey : notnull` and returns `IReadOnlyDictionary<TKey, TValue>`
- `Empty.Lookup<TKey, TValue>()` returns `ILookup<TKey, TValue>` with no constraint
- `Empty.Set<T>()` returns `IReadOnlySet<T>`

```csharp
internal static class EmptyShapes {
    public static Action<string, int> Ignore() => Thinktecture.Empty.Action;
    public static System.Collections.IEnumerable Untyped() => Thinktecture.Empty.Collection();
    public static IReadOnlyList<int> NoNumbers() => Thinktecture.Empty.Collection<int>();
}
```

The empty dictionary and the empty set follow the argument rules of `Dictionary<TKey, TValue>` and `HashSet<T>`.

- The dictionary indexer, `ContainsKey`, and `TryGetValue` throw `ArgumentNullException` with parameter name `key` for a null key
- The dictionary indexer throws `KeyNotFoundException` for every non-null key, and `ContainsKey` and `TryGetValue` return `false`
- The empty lookup accepts a null key without an exception, `Contains` returns `false`, and the indexer returns an empty sequence
- Every comparison method of the empty set returns the result of an empty `HashSet<T>`
- Null arguments to a set comparison throw `ArgumentNullException` with parameter name `other`

## [02]-[SINGLE_ITEM]

`SingleItem` builds a read-only collection around one item without a backing array or hash table. Every method takes an optional equality comparer as its last parameter and falls back to the default comparer.

- `SingleItem.Set<T>(T item, IEqualityComparer<T>? equalityComparer = null)` returns `IReadOnlySet<T>`
- `SingleItem.Dictionary<TKey, TValue>(TKey key, TValue value, IEqualityComparer<TKey>? equalityComparer = null)` requires `TKey : notnull` and returns `IReadOnlyDictionary<TKey, TValue>`
- `SingleItem.Lookup<TKey, TElement>(TKey key, IEnumerable<TElement> elements, IEqualityComparer<TKey>? equalityComparer = null)` requires `TKey : notnull` and returns `ILookup<TKey, TElement>`

The common use is one overload that delegates to another. The overload for one item wraps its argument with `SingleItem` and calls the collection overload. The logic exists once.

```csharp
internal static class Recipients {
    public static int Notify(IReadOnlySet<string> names) => names.Count;
    public static int Notify(string name) => Notify(SingleItem.Set(name));
    public static IReadOnlyDictionary<string, int> Quota(string user, int limit) => SingleItem.Dictionary(user, limit, StringComparer.OrdinalIgnoreCase);
    public static ILookup<int, string> Aliases(int id, ImmutableArray<string> names) => SingleItem.Lookup(id, names);
}
```

`SingleItem.Dictionary` rejects a null key at creation with `ArgumentNullException` and parameter name `key`. Its indexer, `ContainsKey`, and `TryGetValue` throw the same exception for a null key. The indexer throws `KeyNotFoundException` for a key that differs under the comparer. `SingleItem.Set` accepts a null item at creation. `SingleItem.Lookup` throws `ArgumentNullException` with parameter name `elements` for a null sequence and does not inspect the key.

`SingleItem.Lookup` stores the `elements` sequence by reference. The indexer returns that same sequence for the matching key and an empty array for every other key. The lookup shows a later change to the source list. `Aliases` takes an `ImmutableArray<string>`. `Count` is always one, even when `elements` is empty. Enumeration yields one `IGrouping<TKey, TElement>` that re-enumerates `elements` on every pass.

The single-item set applies its comparer in every comparison, and every comparison method throws `ArgumentNullException` with parameter name `other` for a null argument.

| [INDEX] | [METHOD]                    | [RESULT]                                                            |
| :-----: | :-------------------------- | :------------------------------------------------------------------ |
|  [01]   | `Contains(candidate)`       | `true` when the comparer matches `candidate` to `item`              |
|  [02]   | `IsSubsetOf(other)`         | `true` when `other` contains `item`                                 |
|  [03]   | `IsProperSubsetOf(other)`   | `true` when `other` contains `item` and one element that differs    |
|  [04]   | `IsSupersetOf(other)`       | `true` when every element of `other` equals `item`, including empty |
|  [05]   | `IsProperSupersetOf(other)` | `true` when `other` is empty                                        |
|  [06]   | `Overlaps(other)`           | `true` when `other` contains `item`                                 |
|  [07]   | `SetEquals(other)`          | `true` when `other` is non-empty and every element equals `item`    |

## [03]-[TO_READ_ONLY_COLLECTION]

The `ToReadOnlyCollection` extension methods produce an `IReadOnlyCollection<T>` without materializing a list. `ToReadOnlyCollection<T>(this IEnumerable<T> items, int count)` wraps the sequence and reports `count` as `Count`. The wrapper never enumerates to count. The caller owns the correctness of `count`.

- Wrong `count` yields a `Count` that disagrees with the enumeration, and `Enumerable.Count()`, `ToArray()`, and `ToList()` follow the enumeration
- The wrapper implements `IReadOnlyCollection<T>` and not `ICollection<T>`, and `TryGetNonEnumeratedCount` returns `false`
- `ToList` allocates without the count, and only a direct read of `Count` sees the number
- Negative `count` throws `ArgumentOutOfRangeException` with parameter name `count`
- Null sequences throw `ArgumentNullException` with parameter name `items`
- The wrapper calls `GetEnumerator` on the source for every enumeration, and a deferred `Select` runs again each time

`ToReadOnlyCollection<T, TResult>(this IReadOnlyCollection<T> items, Func<T, TResult> selector)` composes `items.Select(selector)` with the count overload and passes `items.Count` at call time. The selector runs once per element per enumeration, and `Count` keeps the value read when the wrapper was created. Null `selector` throws `ArgumentNullException` with parameter name `selector`, and null `items` throws with parameter name `source`, because `Enumerable.Select` raises that exception first.

```csharp
internal sealed record User(string Name);

internal static class Projections {
    public static IReadOnlyCollection<User> Sample() => [new User("ada"), new User("grace")];
    public static IReadOnlyCollection<string> Names(IReadOnlyCollection<User> users) => users.ToReadOnlyCollection(static user => user.Name);
    public static IReadOnlyCollection<string> UpperNames(IReadOnlyCollection<User> users) => users.Select(static user => user.Name.ToUpperInvariant()).ToReadOnlyCollection(users.Count);
}
```

## [04]-[TRIM_OR_NULLIFY]

`TrimOrNullify(this string? text)` returns `null` for a null, empty, or whitespace-only string and `text.Trim()` for every other input. `TrimOrNullify(this string? text, int maxLength)` rejects `maxLength <= 0` with `ArgumentException` and parameter name `maxLength` before it reads `text`. It then applies the null test, trims, and cuts the trimmed text with `Substring(0, maxLength)` when the length exceeds `maxLength`. The cut runs after the trim, and no second trim follows: `"ab cd".TrimOrNullify(3)` returns `"ab "`. The cut counts `char` values, and a `maxLength` that splits a surrogate pair returns a lone surrogate.

The main use is normalization inside `ValidateFactoryArguments` of a string value object. The hook receives the key as `ref string value`. The generated `Validate` rejects a null key before the hook runs: `value` is never null inside the hook. The trimmed text flows into equality, serialization, and persistence only when the hook assigns it back. Null results from `TrimOrNullify` reject empty input in one step. `ProductName.Create` throws `System.ComponentModel.DataAnnotations.ValidationException` with the message of the `ValidationError`, `TryCreate` returns `false`, and `Validate` returns the error. The key member of `ProductName` is a private field, and the implicit conversion to `string` reads it.

The ASP.NET Core model binder applies `TrimOrNullify` to the bound text when `ModelMetadata.ConvertEmptyStringToNull` is set. Bound value objects receive trimmed input before validation. When the trim yields `null`, the binder reports success with a null model and never calls the factory. Value objects implementing `IDisallowDefaultValue` take the error path instead.

```csharp
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
internal sealed partial class ProductName {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        string? trimmed = value.TrimOrNullify(maxLength: 50);
        if (trimmed is null) {
            validationError = new ValidationError("Product name must not be empty.");
            return;
        }
        value = trimmed;
    }
}

internal static class Trimming {
    public static string? Shortened(string? text) => text.TrimOrNullify(maxLength: 8);
    public static string Stored(string raw) => ProductName.Create(raw);
}
```

`Stored("  Widget  ")` returns `"Widget"`, and `ProductName.Create` on text longer than the limit stores the first `maxLength` characters of the trimmed text.

## [05]-[COMPARERS]

The namespace `Thinktecture.Collections` holds public equality comparers that any `IEqualityComparer<T>` consumer accepts. `ProjectionEqualityComparer<T, TItem>` has one constructor that takes a `Func<T, TItem>` selector and uses `EqualityComparer<TItem>.Default`, and one that also takes an `IEqualityComparer<TItem>`. Null comparers throw `ArgumentNullException` with parameter name `comparer`, and a null selector throws with parameter name `selector`. It compares the projections, returns `true` when both arguments are null, and hashes a null projection to zero.

`StringKeyedObjectComparer<T>` requires `T : IConvertible<string>`, which every string-keyed value object and smart enum implements. Its static fields are `Ordinal`, `OrdinalIgnoreCase`, `CurrentCulture`, `CurrentCultureIgnoreCase`, `InvariantCulture`, and `InvariantCultureIgnoreCase`. Each field compares `ToValue()` results with the matching `StringComparer`. `Equals` accepts null arguments, and `GetHashCode` throws `NullReferenceException` for a null item.

`ProductName` equality ignores case: `ProductName.Create("Widget")` equals `ProductName.Create("WIDGET")`. `StringKeyedObjectComparer<ProductName>.Ordinal` restores case-sensitive membership for one collection without touching the type.

```csharp
internal static class Comparers {
    public static IReadOnlySet<ProductName> ExactNames(ProductName name) => SingleItem.Set(name, Thinktecture.Collections.StringKeyedObjectComparer<ProductName>.Ordinal);
    public static IReadOnlySet<User> ByName(User user) => SingleItem.Set(user, new Thinktecture.Collections.ProjectionEqualityComparer<User, string>(static u => u.Name, StringComparer.OrdinalIgnoreCase));
}
```

`ExactNames(ProductName.Create("Widget"))` does not contain `ProductName.Create("WIDGET")`. `ByName(new User("ada"))` contains `new User("ADA")`, although the records differ.

## [06]-[ANTI_PATTERNS]

| [INDEX] | [WRONG_FORM]                                                                                | [CORRECT_FORM]                                                            |
| :-----: | :------------------------------------------------------------------------------------------ | :------------------------------------------------------------------------ |
|  [01]   | `ContainsKey(null)` on an `Empty` or `SingleItem` dictionary as a `false` test              | Reject the null key before the lookup, the dictionary throws              |
|  [02]   | `IsSubsetOf(null)` or any set comparison with a null argument as an answer                  | Pass `Thinktecture.Empty.Collection<T>()` for a missing sequence          |
|  [03]   | `SingleItem.Lookup` over a list that a later step mutates                                   | Pass a snapshot, the lookup exposes the live sequence                     |
|  [04]   | `ToReadOnlyCollection(count)` with a count from a different sequence                        | Pass the count of the wrapped sequence, as `UpperNames` shows             |
|  [05]   | `ToReadOnlyCollection(selector)` re-enumerated with an expensive selector                   | `ToList()` once, the wrapper re-runs the selector per enumeration         |
|  [06]   | `TrimOrNullify(maxLength)` as a display formatter for text with surrogate pairs             | Trim with `TrimOrNullify()` and cut on text elements                      |
|  [07]   | `TrimOrNullify(maxLength)` in a validation hook as a length rule                            | Reject the over-long input, truncation maps two names to one value object |
|  [08]   | Validation hook that trims into a local and never assigns `value`                           | `value = trimmed`, as `ProductName` shows                                 |
|  [09]   | `new List<T>()` returned for an empty `IReadOnlyList<T>` result                             | `Thinktecture.Empty.Collection<T>()`                                      |
|  [10]   | `StringKeyedObjectComparer<T>` as the comparer of a hash-based collection that holds a null | Keep nulls out, `GetHashCode` dereferences the item                       |

On a type keyed with `StringOrdinalIgnoreCase`, do not pass `StringKeyedObjectComparer<T>.OrdinalIgnoreCase` to a collection. The type equality already ignores case, and `Ordinal` restores exact matches. The wrappers stay at the BCL boundary: domain code uses `Seq<A>()`, `Seq(x)`, `Set(x)`, and `toSeq` in place of `Empty.Collection<T>()`, `SingleItem.Set(x)`, and `ToReadOnlyCollection`. `TrimOrNullify` is not the absence marker of a domain value. `Option<string>` through `Optional` marks absence at the boundary, and `TrimOrNullify` stays inside a validation hook.
