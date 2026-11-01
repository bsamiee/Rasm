# [TYPES]

Covers algebraic types, from state counting and constrained values to typestate, persistent snapshots, and arenas.

## [01]-[STATE_COUNTING]

Shipments hold the fields every shipment shares, a route enum holds what varies by path, and a booking holds one variant per valid state with exactly that state's data:

```rust
#[derive(Debug, Clone, Copy, PartialEq, Eq, Enum)]
pub enum Zone {
    Local,
    Regional,
    International,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Route {
    Direct(Zone),
    Relayed { inbound: Zone, outbound: Zone },
}

#[derive(Debug)]
pub enum Booking {
    Requested { since: Timestamp },
    Settled(Result<Label, CarrierError>),
}

#[derive(Debug)]
pub struct Shipment {
    pub id: ShipmentId,
    pub route: Route,
    pub booking: Option<Booking>,
}

#[must_use]
pub fn charge(tariff: &EnumMap<Zone, Decimal>, route: Route) -> Option<Decimal> {
    match route {
        Route::Direct(zone) => Some(tariff[zone]),
        Route::Relayed { inbound, outbound } => tariff[inbound].checked_add(tariff[outbound]),
    }
}
```

Unbooked shipments are `None`, and `Settled` holds a `Result`, so a consumer reaches a label through `?`, `map`, and `ok` with no variant pair restating success and failure. Enums exist for states no standard sum names, here a request holding its instant. `EnumMap` indexing has no absent case, `enum_map!` builds a tariff through a `match` that fails to compile when a zone is missing, and `EnumMap::from_fn` computes each entry from its key when entries follow a rule. Facts a variant determines (a charge, a transit time) are one function matching the enum, and other consumers pass `Route` onward. `Label` and `CarrierError` are placeholders.

## [02]-[CONSTRAINED_VALUES]

`nutype` declares a rule on its type, and the generated `try_new` is the one path to a value:

```rust
#[nutype(
    validate(greater_or_equal = 1, less_or_equal = 10_000),
    derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash, Display, TryFrom, Serialize, Deserialize)
)]
pub struct Quantity(u32);

#[nutype(
    sanitize(trim, lowercase),
    validate(not_empty, len_char_max = 254, predicate = |text: &str| text.contains('@')),
    derive(Debug, Clone, PartialEq, Eq, Hash, Display, AsRef, Serialize, Deserialize)
)]
pub struct Email(String);
```

`Quantity::try_new(count)` returns `Result<Quantity, QuantityError>` with one variant per violated validator, `into_inner` returns the `u32`, and derived `TryFrom` and `Deserialize` call `try_new` at a boundary. Sanitizers run before validators, so an `Email` holds trimmed lowercase text. Ranges with literal bounds take `bounded_integer` (`BoundedU16<1, 500>`, `new` returning `Option`, `const_new::<50>()` checked at compile time) and a nonzero count `NonZero<u32>`, each with no declaration.

Identifiers name their entity through `TypedUuid<Kind>`, and one macro states the kind shape once for every entity:

```rust
macro_rules! entities {
    ($($name:ident),+ $(,)?) => {$(
        #[derive(Debug)]
        pub struct ${concat($name, Kind)};

        impl TypedUuidKind for ${concat($name, Kind)} {
            fn tag() -> TypedUuidTag { const { TypedUuidTag::new(stringify!($name)) } }
        }

        pub type ${concat($name, Id)} = TypedUuid<${concat($name, Kind)}>;
    )+};
}

entities!(Shipment, Order, Customer);
```

`${concat(..)}` under the `macro_metavar_expr_concat` gate derives `ShipmentKind` and `ShipmentId` from one name, `stringify!` derives the tag from the same name, and an inline `const` block rejects an invalid tag at compile time. `ShipmentId::new_v7(stamp)` creates a creation-ordered identifier from the `uuid::Timestamp` that `Timestamp::from_unix(&context, seconds, nanos)` builds from a caller's instant, the caller's `ContextV7` ordering identifiers created within one millisecond.

## [03]-[TYPESTATE]

Generic state parameters hold each state's data, and a transition consumes an order in one state and returns it in the next:

```rust
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

#[derive(Debug, Clone)]
pub struct Order<S> {
    pub id: OrderId,
    pub lines: NEVec<Line>,
    pub state: S,
}

#[must_use]
pub fn place(order: Order<Draft>, at: Timestamp) -> Order<Placed> { Order { state: Placed { at }, ..order } }

#[must_use]
pub fn pay(order: Order<Placed>, at: Timestamp, reference: PaymentReference) -> Order<Paid> { Order { state: Paid { at, reference }, ..order } }
```

`pay` accepts only a placed order, and a paid order has no `place`. `..order` copies every unchanged field while `S` changes, under the `type_changing_struct_update` gate. Stored or inspected orders are an enum over the state types (`enum Stored { Draft(Order<Draft>), Placed(Order<Placed>), Paid(Order<Paid>) }`) that a load matches once. States holding no data sit in a `PhantomData<S>` field, and `#[derive_where(Debug, Clone)]` derives each trait without bounding `S`. `Line` and `PaymentReference` are placeholders.

## [04]-[SNAPSHOTS]

Versions many readers keep sit in `imbl` collections, each transition (`update`, `alter`, `without`) returns a successor sharing every node off its changed path, and a consumer reacting to change diffs successive versions of an `OrdMap`:

```rust
pub fn changes(before: &OrdMap<CustomerId, Decimal>, after: &OrdMap<CustomerId, Decimal>) -> impl Iterator<Item = (CustomerId, Option<Decimal>)> {
    before.diff(after).map(|change| match change {
        DiffItem::Add(customer, balance) | DiffItem::Update { old: _, new: (customer, balance) } => (*customer, Some(*balance)),
        DiffItem::Remove(customer, _) => (*customer, None),
    })
}
```

`diff` skips each subtree the versions share, so a successor compared against its predecessor reads its changed paths alone, and a removed customer reports `None`. `alter` reads a key's present value and returns the successor in one call, its `None` removing the key. A `Vector` of versions holds an undo history at the cost of changed paths. Each access costs a logarithmic factor over `std` collections, a fit for a state broadcast to tasks or compared across versions.

## [05]-[ARENAS]

Records referring to each other sit in an arena owning every record, references are keys, and the graph an algorithm reads derives from the arena:

```rust
new_key_type! {
    pub struct HubKey;
}

#[must_use]
pub fn network(hubs: &SlotMap<HubKey, Hub>) -> DiGraphMap<HubKey, &Lane> {
    DiGraphMap::from_edges(hubs.iter().flat_map(|(from, hub)| hub.lanes.iter().filter_map(move |(to, lane)| hubs.contains_key(*to).then_some((from, *to, lane)))))
}
```

`SlotMap` issues a generational key per hub, so a key held past its hub's removal reads `None` from `get` and fails `contains_key`, and the graph leaves out each lane to a removed hub. Edges borrow each `Lane` from the arena, and the graph copies no record. `Bfs::new(&graph, origin).iter(&graph)` walks the hubs reachable from `origin`, and `toposort(&graph, None)` orders the linked hubs or returns the `Cycle` naming one on it. `SecondaryMap<HubKey, V>` adds a column computed later (a layout, a load) without widening `Hub`. `Hub` holds its outgoing `lanes` as `(HubKey, Lane)` pairs, and `Lane` is a placeholder.
