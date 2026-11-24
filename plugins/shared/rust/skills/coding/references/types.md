# [TYPES]

## [01]-[CARDINALITY]

`Flagged` and `Payment` each hold an order's payment:

```rust
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Flagged {
    pub paid: bool,
    pub paid_at: Option<Timestamp>,
    pub refund: Option<Refund>,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum Payment {
    Unpaid,
    Paid { at: Timestamp },
    Refunded { at: Timestamp, refund: Refund },
}
```

`T` counts instants and `R` refunds:
- `Flagged` counts `2 × (1 + T) × (1 + R)` values, a paid order with no instant and an unpaid order holding a refund among them
- `Payment` counts `1 + T + T × R`, one value per state an order reaches
- Consumers match `Payment` once with no flag tested against its `Option`

Key sets are an enum deriving `Enum`, and `EnumMap::from_fn` over a `match` builds a table that fails to compile when a key lacks its arm:

```rust
#[derive(Debug, Clone, Copy, PartialEq, Eq, Enum)]
pub enum Zone {
    Domestic,
    Continental,
    Overseas,
}

#[must_use]
pub fn tariff() -> EnumMap<Zone, Decimal> {
    EnumMap::from_fn(|zone| match zone {
        Zone::Domestic => Decimal::new(450, 2),
        Zone::Continental => Decimal::new(1_200, 2),
        Zone::Overseas => Decimal::new(3_500, 2),
    })
}
```

`tariff()[zone]` has no absent case, where a `HashMap<Zone, Decimal>` answers `get` with an `Option` and panics on a missing index.

## [02]-[CONSTRAINED_VALUES]

Rules beyond a literal integer range take `nutype`, and a literal range takes a `bounded_integer` alias:

```rust
#[nutype(
    sanitize(trim, lowercase),
    validate(not_empty, len_char_max = 254, predicate = |text: &str| text.contains('@')),
    derive(Debug, Clone, PartialEq, Eq, Hash, Display, AsRef, FromStr)
)]
pub struct Email(String);

pub type Rating = BoundedU8<1, 5>;
```

`nutype` and `bounded_integer` generate each constructor and its refusal:
- `Email::try_new` and derived `FromStr` trim and lowercase before they validate
- `EmailError` holds one variant per validator (`NotEmptyViolated`, `LenCharMaxViolated`, `PredicateViolated`)
- `Rating::new` returns `Option`, and `Rating::const_new::<3>()` fails to compile out of range
- `serde1` refuses an out-of-range `Rating` on decode, and `schemars1` writes both bounds into a derived schema

## [03]-[IDENTITIES]

Identities are one type over a `ConstParamTy` kind enum, holding any value that names a thing, and one `Display` and `FromStr` serve every kind:

```rust
#[derive(Debug, Clone, Copy, PartialEq, Eq, ConstParamTy, Display, FromStr)]
#[display(rename_all = "lowercase")]
#[from_str(rename_all = "lowercase")]
pub enum Kind {
    Order,
    Customer,
}

#[derive(Debug, Clone, PartialEq, Eq, Hash, Display, SerializeDisplay, DeserializeFromStr)]
#[display("{KIND}:{name}")]
pub struct Id<const KIND: Kind> {
    pub name: Name,
}

impl<const KIND: Kind> FromStr for Id<KIND> {
    type Err = IdError;

    fn from_str(s: &str) -> Result<Self, Self::Err> {
        let refused = || IdError { expected: KIND, found: s.to_owned() };
        let (kind, name) = s.split_once(':').ok_or_else(refused)?;
        match (kind.parse::<Kind>(), Name::try_new(name)) {
            (Ok(parsed), Ok(name)) if parsed == KIND => Ok(Self { name }),
            _ => Err(refused()),
        }
    }
}
```

`Name` is a `nutype` over `String` refusing an empty name or one holding `:`, and `IdError` holds the kind it expected with the text it refused:
- `Id<{ Kind::Order }>` passed where `Id<{ Kind::Customer }>` belongs fails to compile
- `order:a1` read as a customer's identity is refused
- `SerializeDisplay` and `DeserializeFromStr` write an identity as its text and read it through `from_str`

UUID identities take `TypedUuid<K>` of `newtype-uuid`, one row of `impl_typed_uuid_kinds!` (`newtype-uuid-macros`) declaring each kind and its alias:

```rust
impl_typed_uuid_kinds! {
    kinds = {
        Parcel = {},
    },
}
```

`ParcelKind` is uninhabited, and `ParcelUuid` names `TypedUuid<ParcelKind>`:
- `ParcelUuid::new_v7` takes the `uuid::Timestamp` that `from_unix(&context, seconds, nanos)` builds from a caller's instant
- `ContextV7` of the caller orders identifiers created within one millisecond

## [04]-[STAGE_TYPES]

`resolved` fills a stated job's retry, and struct update under `type_changing_struct_update` copies the other fields:

```rust
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Job<R = Option<Retry>> {
    pub name: String,
    pub command: Vec<String>,
    pub env: BTreeMap<String, String>,
    pub retry: R,
}

#[must_use]
pub fn resolved(stated: Job, fallback: Retry) -> Job<Retry> {
    let retry = stated.retry.unwrap_or(fallback);
    Job { retry, ..stated }
}
```

`Job` names a stated record wherever a decoder or a caller writes it, and `Job<Retry>` a resolved one, read with no `None` arm:
- Fields a later stage decides each take a parameter of their own (`Job<R = Option<Retry>, H = Option<HostId>>`)
- Stages write their field beside `..`

Stage forms without a type parameter each lose a guarantee:

| [INDEX] | [FORM]                                        | [FAULT]                                  |
| :-----: | :-------------------------------------------- | :--------------------------------------- |
|  [01]   | `struct ResolvedJob` restating `Job`'s fields | Fields written twice, second copy drifts |
|  [02]   | `Job` returned with `retry: Some(fallback)`   | Consumers keep unreachable `None` arm    |

## [05]-[TYPESTATE]

Lifecycles one owner drives through fixed steps take their state as a type parameter, and each transition is a declared `Precedes` impl:

```rust
#[derive(Debug, Clone)]
pub struct Order<S> {
    pub id: OrderId,
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

`Draft`, `Placed { at }`, and `Paid { at, reference }` are the state records, and `advance` moves an order along any declared transition:
- `advance(order, Placed { at })` accepts a draft alone, and a paid order has no successor
- Operations valid in one state take its type alone (`fn refund(order: &Order<Paid>, ..)`)
- Branching states implement `Precedes` once per successor
- Stored orders load as `enum Stored { Draft(Order<Draft>), Placed(Order<Placed>), Paid(Order<Paid>) }`, one match reaching each state's operations
- Lifecycles with no owner driving fixed steps, their state read at each use, stay one enum with no type parameter

## [06]-[PERSISTENT_COLLECTIONS]

`imbl` transitions (`update`, `alter`, `without`) return a successor sharing each node off the changed path, derived values updating through `diff`:

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

`diff` skips each subtree both versions share by pointer, and a total, index, or view updates at the cost of changed entries alone:
- Versions sharing a root diff empty
- `alter` passes a key's present value to its closure and returns a successor in one call, a `None` from the closure removing the key
- Versions kept in a `Vector` hold an undo history at the cost of changed paths

## [07]-[ARENAS]

Records referring to each other go in an arena owning every record, references are keys, and a graph an algorithm reads derives from the arena:

```rust
new_key_type! {
    pub struct HubKey;
}

#[must_use]
pub fn network(hubs: &SlotMap<HubKey, Hub>) -> DiGraphMap<HubKey, &Link> {
    let edges = hubs.iter().flat_map(|(from, hub)| hub.links.iter().map(move |(to, link)| (from, *to, link)));
    DiGraphMap::from_edges(edges.filter(|(_, to, _)| hubs.contains_key(*to)))
}
```

`Hub` holds its outgoing `links` as `(HubKey, Link)` pairs:
- Keys outliving their hub fail `get` and `contains_key` in place of naming a hub that reused their slot
- `network` leaves out links to removed hubs, and edges borrow each `Link` from the arena with no record copied
- `Bfs::new(&graph, origin).iter(&graph)` walks hubs reachable from `origin` through `Walker`
- `toposort(&graph, None)` orders linked hubs or returns a `Cycle` naming a hub on one
- `SecondaryMap<HubKey, V>` holds a column computed later (a layout, a load) without widening `Hub`
