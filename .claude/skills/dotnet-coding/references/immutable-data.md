# [IMMUTABLE_DATA]

Covers snapshot transitions, entity registries, equality through a snapshot graph, and persistent collection costs.

## [01]-[TRANSITIONS]

Snapshots are sealed records whose members are their transitions. A transition a rule can reject returns `Fin<Self>`, and the previous snapshot stays valid for undo, audit, and comparison. Facts that follow from other fields are getters, a stored copy admits a snapshot whose fields disagree:

```csharp
internal sealed record Account(decimal Limit, Seq<decimal> Entries) {
    public decimal Balance => Foldable.sum(Entries);

    public Fin<Account> Posted(decimal amount) =>
        Fault.Unless(Balance + amount <= Limit, Reason.Range, (Balance, amount, Limit)).Map(_ => this with { Entries = Entries.Add(amount) });
}
```

## [02]-[REGISTRIES]

Entity identity is the key of a `HashMap<K, V>` from identity to current snapshot, and each change picks the member whose behavior on a present or absent key states the precondition:

| [INDEX] | [MEMBER]                       | [ABSENT_KEY]               | [PRESENT_KEY]              |
| :-----: | :----------------------------- | :------------------------- | :------------------------- |
|  [01]   | `Add(key, value)`              | Adds                       | Throws `ArgumentException` |
|  [02]   | `TryAdd(key, value)`           | Adds                       | Keeps the current value    |
|  [03]   | `AddOrUpdate(key, value)`      | Adds                       | Replaces                   |
|  [04]   | `AddOrUpdate(key, Some, None)` | Adds the `None` result     | Maps the current value     |
|  [05]   | `SetItem(key, Func<V, V>)`     | Throws `ArgumentException` | Maps the current value     |
|  [06]   | `TrySetItem(key, Func<V, V>)`  | Returns the map unchanged  | Maps the current value     |

Throwing members hold a precondition the caller already established, and a broken one is a defect. A fallible transition at a key lifts an unknown key as `Reason.Missing`, and the write-back sets the key the lookup established:

```csharp
internal static class Registries {
    extension<K, V>(HashMap<K, V> entities) {
        public Fin<HashMap<K, V>> Transitioned(K id, Func<V, Fin<V>> transition) =>
            entities.Find(id).ToFin(Fault.Of(Reason.Missing, id)).Bind(transition).Map(next => entities.SetItem(id, next));
    }
}
```

`accounts.Transitioned(id, account => account.Posted(amount))` serves every registry and transition.

## [03]-[EQUALITY]

Record equality compares each field through `EqualityComparer<T>.Default`, so a snapshot compares by value only as deep as its fields do. LanguageExt collections compare element by element and cache their hash code per instance. Arrays, `List<T>`, `ImmutableArray<T>`, `ImmutableList<T>`, and frozen collections compare by reference, and snapshots holding equal contents in one compare unequal. Snapshot fields hold LanguageExt collections, so `next == current` detects a transition that changed nothing and a snapshot serves as a map key or `memo` argument.

## [04]-[COSTS]

Persistent collections share every part an update leaves untouched, and the structure sets what it copies:

| [INDEX] | [TYPE]                        | [STRUCTURE]                    | [READ]             | [UPDATE]                                         |
| :-----: | :---------------------------- | :----------------------------- | :----------------- | :----------------------------------------------- |
|  [01]   | `Seq<A>`                      | Array, free slots at both ends | `O(1)` index       | `Cons`/`Add` on newest version, amortized `O(1)` |
|  [02]   | `Arr<A>`                      | Array                          | `O(1)` index       | Copies the array, `O(n)`                         |
|  [03]   | `Lst<A>`                      | AVL tree with subtree counts   | `O(log n)` index   | `Insert`, `RemoveAt`, `SetItem` in `O(log n)`    |
|  [04]   | `Map<K, V>`, `Set<A>`         | AVL tree ordered by key        | `O(log n)`, ranges | `O(log n)`                                       |
|  [05]   | `HashMap<K, V>`, `HashSet<A>` | CHAMP trie, 32-way nodes       | `O(log32 n)`       | `O(log32 n)`                                     |
|  [06]   | `Stck<A>`                     | Linked cells                   | `O(1)` top         | `Push`, `Pop` on any version in `O(1)`           |

- `Seq<A>` fills a free slot once per version and only at the end it last grew: a repeat `Cons` or `Add` on one version, growth of a `Tail` or `Init` view, or growth at the opposite end copies the array
- Successors branching from one shared version go in a `Stck<A>` (undo stacks, speculative edits), or a `Lst<A>` where they edit by index
- `toHashMap` builds its trie from the source in place where a `Fold` of `Add` copies a path per item, and keeps the first value of a repeated key
