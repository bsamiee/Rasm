# [DATA]

Covers the snapshot and transition model, from state machines to persistent collections, optics, and comparison values.

## [01]-[SNAPSHOTS]

Snapshots are immutable values, and a transition takes its input first and returns a function from one snapshot to the next, so one shape serves `pipe`, `Ref.update`, and `reduce`. A snapshot one collection states is that collection, and its data-last members are the transitions. `Change` is a `Schema.Class` of a `Sku` brand and a `delta`, `changes` a list of them, and `cart` a `Ref` of the lines:

```ts
const adjusted = ({ sku, delta }: Change) => HashMap.modifyAt(sku, flow(Option.match({ onNone: () => delta, onSome: Number.sum(delta) }), Option.liftPredicate(Number.isGreaterThan(0))));

const replayed = changes.reduce((lines, change) => adjusted(change)(lines), HashMap.empty<Sku, number>());
const recorded = (change: Change) => Ref.update(cart, adjusted(change));
```

- Each state has one representation, a quantity reaching zero removes its key, so lines emptied one by one equal `HashMap.empty()`
- `Equal` and `Hash` read every key on a class's prototype chain, getters included, so a getter stays a cheap, total function of the fields
- `Equal` and `Hash` cache each object's result, so a value stays unchanged once compared, hashed, or stored as a key
- `Schema.toDifferJsonPatch` derives a `Differ` that states a snapshot change as JSON Patch and replays it through decode, so an undo log holds patches

## [02]-[STATE_MACHINES]

State machines are a `Data.TaggedEnum` of states, each holding the data valid in it, and each input is one data-last `$match` returning the next state:

```ts
type Breaker = Data.TaggedEnum<{
    Closed: { readonly failures: number };
    Open: { readonly since: DateTime.Utc };
}>;

const Breaker = Data.taggedEnum<Breaker>();

const failed = (limit: number) => (now: DateTime.Utc) =>
    Breaker.$match({
        Closed: ({ failures }) => (failures < limit ? Breaker.Closed({ failures: failures + 1 }) : Breaker.Open({ since: now })),
        Open: Function.identity<Breaker>,
    });
```

- Folds take the policy, then the input, then the state, the shape of a snapshot transition
- Counts distinguishing states are a field of one case, read against a limit the caller passes
- Cases an input leaves unchanged return themselves through `Function.identity`
- `$is('<case>')` reads `_tag` alone and narrows a value the constructors built

## [03]-[COLLECTIONS]

Persistent collections look keys up through `Equal` and `Hash`, so a composite key is a struct or tuple of its parts. `reading.cell` is a `{ row, column }` struct:

```ts
const byCell = HashMap.fromIterable(Array.map(readings, (reading) => [reading.cell, reading] as const));
const origin = HashMap.get(byCell, { row: 0, column: 0 });
```

Lookup and update costs decide which collection holds a snapshot's data:

| [INDEX] | [COLLECTION]                          | [LOOKUP]                           | [UPDATE]                         |
| :-----: | :------------------------------------ | :--------------------------------- | :------------------------------- |
|  [01]   | `HashMap<K, V>`, `HashSet<A>`         | `Equal` and `Hash`                 | Copies one path, shares the rest |
|  [02]   | `Trie<V>`                             | String key, prefix, longest prefix | Copies one path, shares the rest |
|  [03]   | `Chunk<A>`                            | Index                              | Shares both halves per append    |
|  [04]   | `ReadonlyArray<A>`                    | Index                              | Copies every element             |
|  [05]   | `ReadonlyMap<K, V>`, `ReadonlySet<A>` | Object keys by reference           | Copies every entry               |

- `HashMap.modifyAt` maps a key's present value as an `Option` to the next one, so insert, update, and removal are one call
- `HashMap.mutate` applies many updates to a transient copy no reader sees and returns one new map
- Indexes over a collection are a `HashMap` their owner builds once and every lookup reads
- `HashSet` union, intersection, difference, and subset tests compare members through `Equal`

## [04]-[OPTICS]

Optics declared once focus one path, read it, and rebuild every enclosing value on write. `Account` is a `Schema.Class` nesting `owner.address.city`, and `Catalog` a struct of `items` in a `Data.TaggedEnum` with a `Priced` case:

```ts
const city = Schema.toIso(Account).key('owner').key('address').key('city');
const prices = Optic.id<Catalog>()
    .key('items')
    .forEach((item) => item.tag('Priced').key('price'));

const normalized = Array.map(accounts, city.modify(String.trim));
const discounted = prices.modifyAll(BigDecimal.multiply(rate))(catalog);
```

- `Schema.toIso` reads through the class encode, a nested class reads as a struct, an `Option` as `{ _tag, value }`, a `HashMap` as entry tuples
- `Schema.toIso` writes through the class decode, so a replaced value holds every check, a value failing one throws, and runtime input decodes before it enters
- `Optic.id` paths copy plain objects and arrays, which `Data.taggedEnum` constructors build, and a `Data.Class` field takes `Optic.makeLens` with a replacer that rebuilds through `new`
- `key` keeps a lens, `tag`, `check`, and `refine` focus a part that can be absent, `modify` and `replace` return such a whole unchanged
- `getResult` and `replaceResult` answer a `Result` holding the `SchemaIssue` where a caller acts on an absent focus
- `forEach` focuses each element an inner optic reaches, `modifyAll` maps them, `Optic.getAll` reads them, and other elements stay unchanged
- `modify` answers a data-last update and `replace(value, whole)` a data-first one
- `Optic.makeIso` states a lossless conversion pair and composes into any path

## [05]-[ORDER_AND_EQUIVALENCE]

Types hold their `Order` as a class static composed from the orders value modules export:

```ts
class Task extends Schema.Class<Task>('Task')({ priority: Schema.Int, due: Schema.DateTimeUtc }) {
    static readonly order = Order.Struct({ priority: Order.flip(Number.Order), due: DateTime.Order });
}

const queue = Array.sort(tasks, Task.order);
const ahead = Order.isLessThan(Task.order);
```

- `Order.Struct` compares fields in key order and stops at the first difference, so a stored field orders through its entry and `Order.flip` reverses one
- Container modules lift an element order, `Option.makeOrder` places `none` before every `some`
- `Array.dedupe` and `HashSet` group through `Equal` and `Hash` in one pass
- `Array.dedupeWith` tests each element against every kept one, and takes the `Equivalence` that `Equal` cannot state (a key, a tolerance)
- `Equivalence.Struct` and `Equivalence.mapInput` state a key equality the way `Order.Struct` and `Order.mapInput` state an order
