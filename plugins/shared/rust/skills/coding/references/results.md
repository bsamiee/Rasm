# [RESULTS]

## [01]-[CHECKS]

Orders refuse a repeated line, a bundle naming no listed line, and a quantity past a limit, one function per relation:

```rust
#[derive(Debug, Clone, PartialEq, Eq, thiserror::Error)]
pub enum Refusal {
    #[error("{sku} is listed more than once")]
    Duplicate { sku: Sku },
    #[error("{sku} bundles {missing}, which the order does not list")]
    Unresolved { sku: Sku, missing: Sku },
    #[error("{sku} asks for {quantity}, past the limit of {limit}")]
    Exceeded { sku: Sku, quantity: u32, limit: u32 },
}

fn duplicates(order: &Order) -> impl Iterator<Item = Refusal> {
    let skus = order.lines.iter().map(|line| line.sku);
    skus.duplicates().map(|sku| Refusal::Duplicate { sku })
}

fn unresolved(order: &Order) -> impl Iterator<Item = Refusal> {
    let listed = order.lines.iter().map(|line| line.sku).collect::<BTreeSet<_>>();
    let bundled = order.lines.iter().flat_map(|line| line.bundles.iter().map(|missing| (line.sku, *missing)));
    bundled.filter(move |(_, missing)| !listed.contains(missing)).map(|(sku, missing)| Refusal::Unresolved { sku, missing })
}

fn exceeded(order: &Order, limit: u32) -> impl Iterator<Item = Refusal> {
    let over = order.lines.iter().filter(move |line| line.quantity > limit);
    over.map(move |line| Refusal::Exceeded { sku: line.sku, quantity: line.quantity, limit })
}

pub fn accepted(order: Order, limit: u32) -> Result<Order, NEVec<Refusal>> {
    let refusals = duplicates(&order).chain(unresolved(&order)).chain(exceeded(&order, limit)).collect();
    NEVec::try_from_vec(refusals).map_or(Ok(order), Err)
}
```

`Order` holds `lines` of a `sku` key, a `quantity`, and the `bundles` it names, and `accepted` returns it whole or every refusal:
- `Itertools::duplicates` yields each repeated `Hash` key once however often it repeats, `duplicates_by` a key read from each item
- Checks refusing at most once return `Option<Refusal>` from `bool::then`, which `chain` reads as an iterator
- Refusals naming every match build as `matches.try_into_nonempty_iter().map(|skus| <Variant> { skus: skus.collect() })`
- Nested records return their checks' iterator to the parent's check, and the top level alone collects
- Rules are checked by the stage holding every value they read, a rule over a default checked where the default is filled

## [02]-[CONSTRUCTION]

Bids join a checked price and a checked volume, and a form refusing both reaches its caller with both refusals:

```rust
impl Bid {
    #[must_use]
    pub fn from_form(form: &BidForm) -> Validated<Self, BidError> {
        let price = Validated::from(Price::try_new(form.price).map_err(BidError::from));
        let volume = Validated::from(Volume::try_new(form.volume).map_err(BidError::from));
        price.map2(volume, |price, volume| Self { unit: form.unit, price, volume })
    }
}
```

`BidForm` holds a `unit` and raw values `nutype` types `Price` and `Volume` check, and `BidError` takes each refusal through `#[from]`:
- `map2` runs its closure only when both parts pass, and otherwise returns every failure in argument order
- Fifth parts join a `map4` result as one operand
- `Validated` holds no `and_then` and implements no `Try`, `ok()` converting it to `Result<T, NEVec<E>>` for `?`
- `Result<T, NEVec<E>>` values join through `map_or_else(Validated::Fail, Validated::Good)`, and `Validated::from` takes a one-error `Result`
- Functions every caller joins return `Validated`, and functions callers bind return `Result<T, NEVec<E>>`

## [03]-[TRAVERSAL]

Schedules resolve the unit of each block, refusing every block whose unit the fleet lacks:

```rust
#[must_use]
pub fn units<'fleet>(fleet: &'fleet Fleet, blocks: &[Block]) -> Validated<Vec<&'fleet Unit>, FleetError> {
    let found = |block: &Block| fleet.units.get(&block.unit).ok_or(FleetError::Refused { unit: block.unit, reason: Reason::Missing });
    blocks.iter().map(found).collect()
}
```

`Fleet` holds `units` keyed by the `unit` each `Block` names. Consumers read the found units with no second lookup, and empty `blocks` pass:
- Items yielding `Validated` collect the same way, each item's failures appended in item order
- `collect::<Validated<Vec<U>, E>>()` replaces `partition_result` followed by `NEVec::try_from_vec` of the failures
- `Validated` implements `Sum` over `Result` and `Validated` items, a total holding every failure of its terms
- Foreign batch calls stopping at their first failure run once per item, every item's failure joining

## [04]-[BINDING]

Steps reading a joined value bind it through `.ok()?`, and later dependent steps bind through `?`:

```rust
impl Placed {
    pub fn from_form(headroom: &BTreeMap<UnitId, u32>, form: &BidForm) -> Result<Self, NEVec<BidError>> {
        let bid = Bid::from_form(form).ok()?;
        let refused = |reason| BidError::Refused { unit: form.unit, reason };
        let held = headroom.get(&form.unit).ok_or_else(|| refused(Reason::Missing))?;
        let left = held.checked_sub(bid.volume.into_inner()).ok_or_else(|| refused(Reason::Short))?;
        Ok(Self { bid, left })
    }
}
```

`Placed` holds the bid and its headroom `left`, and `impl From<BidError> for NEVec<BidError>` converts each error at `?`:
- One closure builds every refusal of the unit, and each step names the reason its absence or underflow means
- Absence becomes a default (`unwrap_or_default`) only where the domain defines one, a unit with no headroom being a refusal

## [05]-[EARLY_EXIT]

`try_fold` threads its accumulator as `Continue` and stops at the first `Break`:

```rust
pub fn first_over(costs: &[u64], budget: u64) -> ControlFlow<usize, u64> {
    costs.iter().enumerate().try_fold(0_u64, |spent, (index, cost)| {
        let total = spent.checked_add(*cost).filter(|total| *total <= budget);
        total.map_or(ControlFlow::Break(index), ControlFlow::Continue)
    })
}
```

`Break` holds the first index whose running total overflows or passes `budget`, and `Continue` the total within it:
- `?` on a `ControlFlow` returns its `Break`, and an outer fold or a recursive walk stops at the first `Break` an inner one returns
- `break_ok` and `continue_ok` convert the outcome into a `Result`
- `into_value` under `control_flow_into_value` reads a `ControlFlow<T, T>`, after `map_break` or `map_continue` gives both sides one type
- `try_fold` and `try_for_each` borrow their iterator mutably, which yields the items left after a `Break` with no `by_ref()`
- Folds editing a captured `&mut` state run `try_for_each` returning `ControlFlow<()>`
