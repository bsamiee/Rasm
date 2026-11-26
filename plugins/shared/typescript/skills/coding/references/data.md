# [DATA]

## [01]-[TRANSITIONS]

Transitions take their input first and return a function from one state to the next, the one shape `pipe`, `Ref.update`, `Stream.scan`, and a fold apply. Keyed state is the collection itself, and `HashMap.modifyAt` maps the held `Option` to the stored one, inserting, updating, and removing in one function:

```ts
const sighted = (key: string, at: string, value: Resource['value']) =>
    HashMap.modifyAt(key, (held: Option.Option<Resource>) => Option.some({ value, uris: Option.match(held, { onNone: () => HashSet.make(key, at), onSome: ({ uris }) => HashSet.add(uris, at) }) }));
```

`Resource` holds an MCP resource's last read `value` and the `uris` it was read under. `SubscriptionRef.update(store, sighted(key, at, value))` applies one read, the first read of a key inserting it and a later one adding its URI, and a fold from `HashMap.empty()` replays a log of reads:
- Transitions an input decides are steps folded once (`steps.reduce((state, step) => step(state), initial)`), each input case adding its steps
- `Equal.equals` and `Hash.hash` compare plain objects, arrays, `Date`, `Map`, and `Set` by contents, class instances by own and prototype keys
- `Hash.hash` and an outermost `Equal.equals` cache their result per object, and a value hashed or compared stays unchanged
- `Equal.byReferenceUnsafe(node)` marks a syntax tree node or host handle for identity, `Equal.byReference` marking a proxy in its place
- `Schema.toDifferJsonPatch(X)` states a change as JSON Patch for a peer or store, `diff` encoding both values and `patch` decoding through `X`

## [02]-[STATE_MACHINES]

State machines are one `Data.taggedEnum` stating its cases in its type argument, each case holding the data valid in that state, and its constructors and fold type every transition. Transitions read the one fact they need from the state they leave and build the next through one constructor:

```ts
type Workspace = Data.TaggedEnum<{
    Entries: { readonly last: Option.Option<Entry['id']> };
    Entry: { readonly entry: Entry['id']; readonly composition: Option.Option<Composition['id']> };
    Place: { readonly entry: Entry['id']; readonly asset: Option.Option<keyof Assets['Service']> };
}>;
const Workspace = Data.taggedEnum<Workspace>();

const back = Workspace.$match({
    Entries: (entries) => entries,
    Entry: ({ entry }) => Workspace.Entries({ last: Option.some(entry) }),
    Place: ({ entry }) => Workspace.Entry({ entry, composition: Option.none() }),
});
```

`Workspace` is the portfolio editor's view, `Entry`, `Composition`, and `Assets` its records. `back` leaves an entry for the list with that entry marked last, and a file placement for its entry, each next state typed by `$match`:
- Steps deciding a value beside the next state return `[value, next]` to `Ref.modify`, `Effect.flatten` running an effect value after the update
- `SubscriptionRef.modify(links, (held) => [Record.get(held, name), Record.set(held, name, entry)])` swaps a peer and returns the replaced one
- Loading states are `AsyncResult` from `effect/reactivity`, its `waiting` flag on every case keeping the last value shown while a refresh runs
- Tagged enums one class field holds declare cases in the field's type, `static readonly Bound = Data.taggedEnum<Column['bounds'][number]>()`
- Constructors build plain objects, so `Equal` compares cases by tag and fields and optics write through them
- Enums whose every case has one builder and one reader go, each value written once where it prints

## [03]-[COLLECTIONS]

Persistent collections key on structural values: a composite key is a struct or tuple of its parts, a tagged enum case included, and a lookup builds an equal value:

```ts
const fresh = (heard: HashSet.HashSet<{ readonly text: string; readonly scope: Scope }>, agent: Action['agent']) => (notes: Tally['notes']) =>
    notes.map(({ text, once }) => ({ text, scope: scopes[once]({ agent }) })).filter((note) => !HashSet.has(heard, note));
```

`Scope` is a tagged enum of how long a policy note stays told (`Session`, `Agent` holding the agent, `Call`), and `scopes` builds one per `once` value. `fresh` keeps the notes the set lacks, where a `''` key for the session collided with the main agent's:
- Rows read by key more than once index once through `Record.fromIterableBy(rows, Struct.get('name'))`, each read a `Record.get` in place of a `find`
- Membership by name tests a `Set` of names, an `Array.difference` over whole records compared by `Equal` restating it
- Records over a key list build through `Struct.Record(keys, value)` and `Record.map`, `Record.fromIterableWith` widening keys to `string`
- Records rekeyed and enriched in one walk take `Record.mapEntries`, reading sibling records by the source key, a `mapKeys` per record restating it
- `Map.groupBy` groups by a number or other non-string key, `Array.groupBy` by a string key into non-empty groups
- Derivations repeated per key within one call fill a local `Map` through `getOrInsertComputed`, passed to their reader as a `(key) => value`
- Indexes over one immutable value build once through `Function.memoize`, a `WeakMap` by identity, never a field or getter `Equal` and encoding read
- `HashMap.mutate` applies a batch of updates to a transient copy no reader sees and returns one new map
- `Trie` keys strings: `longestPrefixOf` finds the project owning a path, `valuesWithPrefix` the flags an abbreviation names
- `Chunk` shares both sides of a concatenation in a balanced tree, where an array spread copies every element

## [04]-[OPTICS]

Optics state a path once and serve every read and write through it, rebuilding each enclosing value on write:

```ts
const loaded = Optic.id<Atom.Type<typeof draft>>()
    .refine(AsyncResult.isSuccess)
    .compose(Optic.makeLens((result: AsyncResult.Success<Atom.Success<typeof draft>, Atom.Failure<typeof draft>>) => result.value, AsyncResult.success));
const hero = loaded.key('portfolio').optionalKey('hero');
```

`draft` is the editor's atom holding an `AsyncResult` of the loaded draft, and `Portfolio` an `Opaque` record with an optional `hero`. `Optic.replace(hero, placement)` sets the hero of a loaded draft, `Optic.replace(hero, undefined)` deletes the key, and a draft still loading passes unchanged:
- `key` keeps a lens, `optionalKey` deletes a key written `undefined`, and `tag`, `refine`, and `check` focus a part that can be absent
- `modify` and `replace` return a whole lacking the focus unchanged, `getResult` and `replaceResult` its `SchemaIssue`
- `forEach((item) => item.tag('<Case>').key('<field>'))` focuses each element's field, `modifyAll` writing and `Optic.getAll` reading them
- Writes copy plain objects and arrays, the values `Schema.Struct`, `Schema.Opaque`, `Schema.TaggedUnion`, and `Data.taggedEnum` build
- Writes through a class instance (`Schema.Class`, `Data.Class`, `AsyncResult`) throw, a `makeLens` over its constructor rebuilding it
- `Schema.toIso(X)` focuses a class instance by encoding on read and decoding on replace, a replaced value failing a check throwing
- `Optic.some`, `Optic.success`, and `Optic.entries` focus inside an `Option`, a `Result`, and a record's entries
- Plain objects changing one level deep take `Struct.evolve` or `Struct.assign`, an optic standing where a path crosses a level or a case

## [05]-[ORDER_AND_EQUIVALENCE]

Orders compose from the orders value modules export and sit on their type as a static that `Array.max`, `Array.sort`, `Order.clamp`, and `Combiner.max` read. Literal schemas order by declaration through each value's index in `literals`:

```ts
const byRun = Order.mapInput(Number.Order, (run: (typeof Action.cases.Shell.Type)['run']) => Action.cases.Shell.fields.run.literals.indexOf(run));
```

`Action` is the policy's union of agent tool calls, its `Shell` case listing `run` modes from `foreground` to `monitored`. A new mode is one literal at its rank, and a hand list of the modes passed to `indexOf` restates the schema:
- `Order.Struct` and `Order.Tuple` compare fields in key or position order until one differs, `Order.mapInput` over one ordering by a part
- Versions order through `Array.makeOrder(Order.Number)` over their numeric parts, `Option.makeOrder` ranking `None` below every `Some`
- `Array.sortWith(items, (item) => [item.asking, item.focused] as const, Order.Tuple([Order.Boolean, Order.Boolean]))` ranks by a tuple of facts
- `Order.Boolean` ranks `false` first and `Order.flip` `true` first, a `0 : 1` key restating them
- `Schema.toEquivalence(X)` derives field equality, `Equivalence.mapInput(Equivalence.String, Struct.get('<key>'))` builds one by key
- `Array.containsWith`, `dedupeWith`, `differenceWith`, `unionWith`, and `Stream.changesWith` read an `Equivalence` in place of a hand `===`
- `Array.dedupe` and `HashSet` group through hash buckets in one pass, `Array.dedupeWith` compares each element with every kept one

## [06]-[REDUCERS]

Folds over records combine each field through the reducer its value module exports, and the reducer's identity stands for a part adding nothing:

```ts
const bounds = Struct.makeReducer({ top: Number.ReducerMin, left: Number.ReducerMin, bottom: Number.ReducerMax, right: Number.ReducerMax });
const extent = (corners: readonly (readonly [number, number])[]) => Extent.make(bounds.combineAll(Array.map(corners, ([x, y]) => ({ top: y, left: x, bottom: y, right: x }))));
```

`corners` are the spread coordinates InDesign resolves for placed items' outer stroke bounds, and `Extent` the record of a box's edges. Each corner is a box of no size, and `combineAll` keeps the least top and left and the greatest bottom and right in one pass:
- `Number.ReducerSum`, `ReducerMin`, `ReducerMax`, and `Boolean.ReducerAnd` hold their identity (`0`, `Infinity`, `-Infinity`, `true`)
- `Array.getReadonlyReducerConcat()` concatenates `readonly A[]` fields, where `Array.makeReducerConcat()` types mutable arrays and fails them
- `Record.makeReducerUnion(Combiner.last())` merges records a later one overrides, folding an inherited chain ordered from root to nearest
- `Option.makeReducer(combiner)` lifts a `Combiner` with no identity (`Combiner.make(join)`) into a reducer whose identity is `None`
- `Reducer.make(combine, initial)` states a fold no module exports, a hand `reduce` with a seed restating the pair
