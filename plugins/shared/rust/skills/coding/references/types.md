# [TYPES]

## [01]-[CARDINALITY]

Fields every parcel holds sit in one record. Varying fields compose `Option`, `Result`, and tuples until their value count equals the domain's state count:

```rust
#[derive(Debug)]
pub struct Parcel {
    pub id: ParcelUuid,
    pub route: (Zone, Option<Zone>),
    pub booking: Option<(Timestamp, Option<Result<Label, CarrierError>>)>,
}

#[must_use]
pub fn charge(tariff: &EnumMap<Zone, Decimal>, (origin, relay): (Zone, Option<Zone>)) -> Option<Decimal> { relay.into_iter().try_fold(tariff[origin], |total, zone| total.checked_add(tariff[zone])) }
```

`(Zone, Option<Zone>)` holds a direct route per zone and a relayed route per zone pair, the count of a `Direct` and `Relayed` route enum. `booking` counts an unbooked parcel, a request pending since its instant, and a finished request's label or carrier error. Consumers reach the label through `?`, `map`, and `ok`. Enums declare a key set (`Zone`, deriving `Enum`) and states with same-typed data no standard sum tells apart (`Delivered { at }`, `Returned { at }`). `EnumMap` indexing has no absent case. `EnumMap::from_fn` over a key `match` builds a tariff that fails to compile when a zone lacks an entry, or computes entries a rule derives. Facts a state determines (a charge, a transit time) are one function over the state, and other consumers pass the state onward.

## [02]-[CONSTRAINED_VALUES]

Rules beyond a literal integer range take `nutype`, and a literal range takes a `bounded_integer` alias:

```rust
#[nutype(
    sanitize(trim, lowercase),
    validate(not_empty, len_char_max = 254, predicate = |text: &str| text.contains('@')),
    derive(Debug, Clone, PartialEq, Eq, Hash, Display, AsRef, FromStr, Serialize, Deserialize)
)]
pub struct Email(String);

pub type Rating = BoundedU8<1, 5>;
```

`Email::try_new` sanitizes first, and `EmailError` has a variant per validator (`NotEmptyViolated`, `LenCharMaxViolated`, `PredicateViolated`). Derived `FromStr` and `Deserialize` run the same path at a boundary. `Rating::new` returns `Option`, `Rating::const_new::<3>()` rejects an out-of-range constant at compile time, and `Deserialize` rejects one on decode. The `schemars1` feature writes both bounds into a derived schema.

One `impl_typed_uuid_kinds!` row per entity states its identifier type:

```rust
impl_typed_uuid_kinds! {
    kinds = {
        Parcel = {},
        Order = {},
        Customer = {},
    },
}
```

Each row derives an uninhabited `ParcelKind`, its tag `parcel`, and the alias `ParcelUuid` for `TypedUuid<ParcelKind>`. `ParcelUuid::new_v7(stamp)` creates a time-ordered identifier from `uuid::Timestamp::from_unix(&context, seconds, nanos)` over a caller's instant. The caller's `ContextV7` orders identifiers created within one millisecond.

## [03]-[MEMORY]

Native `collect` can reuse an owned iterator's allocation. Small retained results can keep large source allocations; `Box<[T]>` fits frozen storage when spare capacity has no future consumer. Converting `Vec` to `Arc<[T]>` allocates and moves elements; direct shared-slice collection lets native specialization choose construction. Exact output length can enable direct allocation, while `ExactSizeIterator` alone promises no specialization and filtering can lose it. Unsafe iterator markers remain with their owning implementation.

`Vec` and `String` use native `clone_from` or `clone_into` to reuse destination storage when replacement requires cloning; element reuse follows that element's implementation. Generic `Clone` bounds promise no reuse. Borrow or move an available owner when duplication serves no consumer. `Bytes::slice` shares backing storage in constant time; copying a small retained part can release a larger source after other owners drop. Allocation count and retained bytes are separate costs.

Borrowed map lookup avoids temporary key ownership. `hashbrown::HashMap::entry_ref(...).or_insert_with(...)` constructs an owned key and value only on vacancy. `or_insert_with_key` derives a value from its key; `get_key_value` returns both stored facts in one lookup. Preserve output ordering and hasher requirements when choosing the collection.

## [04]-[TYPESTATE]

Generic state parameters hold each state's data, marker impls declare the lifecycle's transitions, and one function moves an order along any of them:

```rust
#[derive(Debug, Clone)]
pub struct Order<S> {
    pub id: OrderUuid,
    pub lines: NEVec<Line>,
    pub state: S,
}

pub trait Precedes<T> {}

impl Precedes<Placed> for Draft {}
impl Precedes<Paid> for Placed {}

#[must_use]
pub fn advance<S, T>(order: Order<S>, state: T) -> Order<T>
where S: Precedes<T> {
    Order { state, ..order }
}
```

`Draft`, `Placed { at }`, and `Paid { at, reference }` are the state records. `advance(order, Placed { at })` accepts a draft, and declared impls give paid orders no successor. Branching states implement `Precedes` per successor; downstream crates can extend the public trait with their own state types. `enum Stored { Draft(Order<Draft>), Placed(Order<Placed>), Paid(Order<Paid>) }` holds stored orders, and a load matches once. Closed lifecycle cases use an enum when consumers inspect state.

## [05]-[PERSISTENT_COLLECTIONS]

`imbl` transitions (`update`, `alter`, `without`) return a successor sharing every node off the changed path. Derived values follow each version through a diff:

```rust
#[must_use]
pub fn retotal(total: Decimal, before: &OrdMap<Sku, Decimal>, after: &OrdMap<Sku, Decimal>) -> Option<Decimal> {
    before.diff(after).try_fold(total, |total, change| match change {
        DiffItem::Add(_, added) => total.checked_add(*added),
        DiffItem::Update { old: (_, old), new: (_, new) } => total.checked_sub(*old)?.checked_add(*new),
        DiffItem::Remove(_, removed) => total.checked_sub(*removed),
    })
}
```

`diff` skips every subtree both versions share by pointer, and a total, index, or view updates at the cost of changed entries alone. Versions sharing a root diff empty. `alter` reads a key's present value and returns the successor in one call, and a `None` from its closure removes the key. A `Vector` of versions holds an undo history at the cost of changed paths.

## [06]-[ARENAS]

Records referring to each other sit in an arena owning every record, references are keys, and the graph an algorithm reads derives from the arena:

```rust
new_key_type! {
    pub struct HubKey;
}

#[must_use]
pub fn network(hubs: &SlotMap<HubKey, Hub>) -> DiGraphMap<HubKey, &Link> {
    DiGraphMap::from_edges(hubs.iter().flat_map(|(from, hub)| hub.links.iter().filter_map(move |(to, link)| hubs.contains_key(*to).then_some((from, *to, link)))))
}
```

`Hub` holds its outgoing `links` as `(HubKey, Link)` pairs. Generational keys outliving their hub fail `get` and `contains_key` in place of naming a hub that reused the slot. The graph leaves out links to removed hubs. Edges borrow each `Link` from the arena, and the graph copies no record. `Bfs::new(&graph, origin).iter(&graph)` walks hubs reachable from `origin`. `toposort(&graph, None)` orders linked hubs or returns a `Cycle` naming a hub on one. `SecondaryMap<HubKey, V>` adds a column computed later (a layout, a load) without widening `Hub`.

Dense `SecondaryMap` storage and iteration cover primary slots; `SparseSecondaryMap` fits columns populated for a minority of keys. Derived columns need later consumers. Arena lifetime, removal, and destruction semantics select the owner; dropless storage excludes values requiring destruction.

`Dfs::reset` starts an independent traversal, while `move_to` retains discovered nodes. `has_path_connecting` accepts reusable `DfsSpace` and stops when the target is found. Preserve direction, revisit behavior, mutation rules, and simultaneously live outputs when reusing workspace.

## [07]-[LAYOUT]

`Option` preserves size, alignment, and call ABI for the references, global boxes, function pointers, nonzero integers, `NonNull`, and transparent wrappers its contract lists. Custom enums and arbitrary smart pointers carry no blanket niche guarantee. Use the domain's optional value instead of a separate presence flag or sentinel, with no unsafe representation conversion.

Inline storage saves allocation when its footprint and spill behavior fit the workload. `SmallVec` embeds capacity even when empty; domain bounds or observed lengths determine capacity. Large enum variants affect every instance's layout, while boxing adds allocation and indirection and can remove `Copy`. Variant distribution, access patterns, native size and alignment determine whether representation changes improve the operation. `-Zprint-type-sizes` in `RUSTFLAGS` prints size, alignment, field order, and padding per type at build.
