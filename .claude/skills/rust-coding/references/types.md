# [TYPES]

Covers algebraic type design and constrained values.

## [01]-[STATE_COUNTING]

Fields every parcel holds sit in one record, and each varying field composes `Option`, `Result`, and tuples until its count of values equals the domain's count of states:

```rust
#[derive(Debug, Clone, Copy, PartialEq, Eq, Enum)]
pub enum Zone {
    Local,
    Regional,
    International,
}

#[derive(Debug)]
pub struct Parcel {
    pub id: ParcelUuid,
    pub route: (Zone, Option<Zone>),
    pub booking: Option<(Timestamp, Option<Result<Label, CarrierError>>)>,
}

#[must_use]
pub fn charge(tariff: &EnumMap<Zone, Decimal>, (origin, relay): (Zone, Option<Zone>)) -> Option<Decimal> { relay.map_or(Some(tariff[origin]), |zone| tariff[origin].checked_add(tariff[zone])) }
```

`(Zone, Option<Zone>)` holds a direct route per zone and a relayed route per zone pair, the exact count of a route enum with `Direct` and `Relayed` variants. `booking` counts an unbooked parcel, a request pending since its instant, and a request finished with a label or a carrier error, and a consumer reaches the label through `?`, `map`, and `ok`. An enum is declared for a fieldless key set like `Zone`, or where states hold the same type of data (`Delivered { at }` beside `Returned { at }`) that no composition of standard sums tells apart. `EnumMap` indexing has no absent case, and `EnumMap::from_fn` with a `match` over the key builds a tariff that fails to compile when a zone lacks an entry, or computes each entry from its key when entries follow a rule. Facts a state determines (a charge, a transit time) are one function over the state, and other consumers pass the state onward. `Label` and `CarrierError` are placeholders.

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

`Email::try_new(text)` trims and lowercases before validating, and returns an `EmailError` naming the violated validator (`NotEmptyViolated`, `LenCharMaxViolated`, `PredicateViolated`). Derived `FromStr` and `Deserialize` run the same path at a boundary, and `into_inner` returns the `String`. `Rating::new(stars)` returns `Option`, `Rating::const_new::<3>()` rejects an out-of-range constant at compile time, `Deserialize` rejects one on decode, and the `schemars1` feature writes both bounds into a derived schema.

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

Each row derives an uninhabited `ParcelKind`, its tag `parcel`, and the alias `ParcelUuid` for `TypedUuid<ParcelKind>`. `ParcelUuid::new_v7(stamp)` creates a creation-ordered identifier from the `uuid::Timestamp` that `Timestamp::from_unix(&context, seconds, nanos)` builds from a caller's instant, the caller's `ContextV7` ordering identifiers created within one millisecond.

## [03]-[TYPESTATE]

Generic state parameters hold each state's data, marker impls declare the lifecycle's transitions, and one function moves an order along any of them:

```rust
#[derive(Debug, Clone)]
pub struct Order<S> {
    pub id: OrderUuid,
    pub lines: NEVec<Line>,
    pub state: S,
}

#[derive(Debug, Clone)]
pub struct Draft;

#[derive(Debug, Clone)]
pub struct Placed {
    pub at: Timestamp,
}

#[derive(Debug, Clone)]
pub struct Paid {
    pub at: Timestamp,
    pub reference: PaymentReference,
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

`advance(order, Placed { at })` compiles for a draft alone, and a paid order has no transition onward. A branching state implements `Precedes` once per successor, and the orphan rule keeps other crates from adding a transition between these states. `enum Stored { Draft(Order<Draft>), Placed(Order<Placed>), Paid(Order<Paid>) }` holds a stored order, and a load matches it once. `Line` and `PaymentReference` are placeholders.

## [04]-[SNAPSHOTS]

Each `imbl` transition (`update`, `alter`, `without`) returns a successor sharing every node off its changed path, and a consumer reacting to change diffs successive versions:

```rust
pub fn changes<'map, K, V>(before: &'map OrdMap<K, V>, after: &'map OrdMap<K, V>) -> impl Iterator<Item = (&'map K, Option<&'map V>)>
where
    K: Ord,
    V: PartialEq,
{
    before.diff(after).map(|change| match change {
        DiffItem::Add(key, value) | DiffItem::Update { new: (key, value), .. } => (key, Some(value)),
        DiffItem::Remove(key, _) => (key, None),
    })
}
```

`diff` skips nodes both versions share and reads changed paths alone. Removed keys report `None`. `alter` reads a key's present value and returns the successor in one call, its `None` removing the key. A `Vector` of versions holds an undo history at the cost of changed paths. Each access costs a logarithmic factor over `std` collections.

## [05]-[ARENAS]

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

`SlotMap` issues a generational key per hub. A key held past its hub's removal reads `None` from `get` and fails `contains_key`, and the graph leaves out each link to a removed hub. Edges borrow each `Link` from the arena, and the graph copies no record. `Bfs::new(&graph, origin).iter(&graph)` walks the hubs reachable from `origin`, and `toposort(&graph, None)` orders the linked hubs or returns the `Cycle` naming one on it. `SecondaryMap<HubKey, V>` adds a column computed later (a layout, a load) without widening `Hub`. `Hub` holds its outgoing `links` as `(HubKey, Link)` pairs, and `Link` is a placeholder.
