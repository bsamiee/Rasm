# [DATA]

## [01]-[TRANSITIONS]

Transitions take their input first and return a function from one state to the next, the one shape `pipe`, `Ref.update`, `Stream.scan`, and a fold apply. States one collection holds are the collection itself, with the value brand deciding what it holds:

```ts
const adjusted = (sku: Sku, delta: number) => HashMap.modifyAt(sku, flow(Option.match({ onNone: Function.constant(delta), onSome: Number.sum(delta) }), Quantity.makeOption));
```

`Ref.update(reserved, adjusted(sku, 1))` applies one change to a warehouse's reservations, and a fold from `HashMap.empty()` replays a log of changes:
- `modifyAt` maps the held `Option` to the stored one, inserting, updating, and removing in one function
- Skus with a count `Quantity.makeOption` rejects leave the map, and a zero reservation has no representation
- `Equal.equals` and `Hash.hash` compare plain objects, arrays, `Map`, and `Set` by contents and class instances by class and fields, a `_tag` separating kinds
- `Hash.hash` caches per object and a hashed value stays unchanged, `Equal.byReference(handle)` compares a host handle by identity
- `Schema.toDifferJsonPatch(X)` states a change as JSON Patch for a peer or store, with `patch` decoding the result through `X`

## [02]-[STATE_MACHINES]

State machines are a `Data.TaggedEnum` with cases holding the data valid in each state, and `Data.taggedEnum` derives constructors and fold. Transitions read the one fact they need from the state they leave and build the next through one constructor:

```ts
type Link = Data.TaggedEnum<{
    Up: { readonly since: DateTime.Utc };
    Down: { readonly attempt: number; readonly retry: DateTime.Utc };
}>;

const Link = Data.taggedEnum<Link>();

const dropped =
    (now: DateTime.Utc, base = Duration.seconds(1)) =>
    (link: Link) => {
        const drops = Link.$match(link, { Up: () => 0, Down: ({ attempt }) => attempt });
        return Link.Down({ attempt: drops + 1, retry: DateTime.addDuration(now, Duration.times(base, 2 ** drops)) });
    };
```

Each drop doubles the wait from `base`:
- Steps deciding a value beside the next state return `[value, next]` to `Ref.modify`, `Effect.flatten` running an effect value after the update
- Loading states are `AsyncResult` from `effect/reactivity`, with a `waiting` flag on every case

## [03]-[COLLECTIONS]

Persistent collections key on structural values: a composite key is a struct or tuple of its parts, and a lookup builds an equal value. Grouping by a composite key is one fold taking the key function:

```ts
const grouped = <K, A>(key: (value: A) => K) => Iterable.reduce(HashMap.empty<K, Chunk.Chunk<A>>(), (index, value: A) => HashMap.modifyAt(index, key(value), flow(Option.getOrElse(Chunk.empty), Chunk.append(value), Option.some)));
```

`grouped((reading: Reading) => reading.cell)(readings)` indexes readings by a `{ row, column }` cell, and `HashMap.get(index, { row: 0, column: 0 })` reads one group:
- `HashMap.mutate` applies a batch of updates to a transient copy no reader sees and returns one new map
- `Trie` keys strings in key order, reading by prefix through `keysWithPrefix` and by longest prefix through `longestPrefixOf`
- `Chunk` shares both sides of a concatenation, where a `readonly A[]` copies every element

## [04]-[OPTICS]

Optics state a path once and serve every read and write through it, rebuilding each enclosing value on write. Traversals focus every element of one case:

```ts
const prices = Optic.id<readonly (typeof Item.Type)[]>().forEach((item) => item.tag('Priced').key('price'));
```

`Item` is a `Schema.TaggedUnion` with a `Priced` case. `prices.modifyAll(BigDecimal.multiply(rate))` reprices every priced item, `Optic.getAll(prices)` reads their prices, and other items stay unchanged:
- `key` keeps a lens, `tag`, `refine`, and `check` focus a part that can be absent, and `modify` returns a whole lacking it unchanged
- `getResult` and `replaceResult` return the `SchemaIssue` where a caller acts on an absent focus
- `Optic.id` copies plain objects and arrays, the values `Schema.Struct`, `Schema.TaggedUnion`, and `Data.taggedEnum` build
- `Schema.toIso(X)` focuses a class instance through class encode and decode, and a replaced value failing a check throws
- `Optic.some`, `Optic.success`, and `Optic.entries` focus inside an `Option`, a `Result`, and a record's entries
- Plain objects changing one level deep take a spread or `Struct.evolve`, an optic stands where a path crosses a level or a case

## [05]-[ORDER_AND_EQUIVALENCE]

Orders compose from the orders value modules export and sit on their type as a static that `Array.max`, `Order.clamp`, and `Combiner.max` read. One function orders every literal schema by its declaration order:

```ts
const ranked = <const L extends readonly SchemaAST.LiteralValue[]>(schema: Schema.Literals<L>) => Order.mapInput(Number.Order, (value: L[number]) => schema.literals.indexOf(value));
```

`Severity` lists its literals from lowest to highest, and `Alert.order = Order.Struct({ severity: Order.flip(ranked(Severity)), raised: DateTime.Order })` sorts alerts most severe, then oldest, first:
- `Order.Struct` compares fields in key order until one differs, and `Order.mapInput(Order.Struct(...), f)` orders by a nested part
- `Schema.toEquivalence(X)` derives field equality, `Equivalence.mapInput` builds one by key for a type lacking it (`URL` by `href`) as a static
- `Array.containsWith`, `Array.dedupeWith`, `Array.unionWith`, and `Stream.changesWith` read an `Equivalence` in place of a hand `===` on a key
- `Array.dedupe` and `HashSet` group through `Equal` and `Hash` in one pass, `Array.dedupeWith` compares each element with every kept one
- `Struct.makeReducer` composes reducers (`Number.ReducerSum`, `Option.makeReducer(r)`, `Array.makeReducerConcat()`), `combineAll` folding states
