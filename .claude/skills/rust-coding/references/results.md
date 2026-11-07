# [RESULTS]

## [01]-[VALIDATION]

Forms validate each field through its `nutype` constructor, and `MarketError` takes each generated error through a transparent `#[from]` variant:

```rust
#[must_use]
pub fn bid(form: &BidForm) -> Validated<Bid, MarketError> {
    Validated::from(Price::try_new(form.price).map_err(MarketError::from)).map2(Validated::from(Volume::try_new(form.volume).map_err(MarketError::from)), Bid::new)
}
```

`map2` runs `Bid::new` (from `derive_more::Constructor`) only when both fields pass. Forms over four fields nest `bid` as one operand.

## [02]-[TRAVERSAL]

Items join through `collect`:

```rust
#[must_use]
pub fn units<'fleet>(fleet: &'fleet Fleet, schedule: &Schedule) -> Validated<Vec<&'fleet Unit>, MarketError> {
    schedule.blocks.iter().map(|block| fleet.units.get(&block.unit).ok_or(MarketError::Refused { unit: block.unit, reason: Refusal::Missing })).collect()
}
```

Each unlisted unit reports its own `Refused`, an empty schedule passes, and consumers read the found units with no second lookup. Items yielding `Validated` collect the same way into one `NEVec` of every failure. Checks returning nothing collect into `Validated<(), E>` through `bool::ok_or`. Foreign batch calls stopping at their first failure run once per element, every failure joining. Refusals an iterator yields collect into `Validated` in place of `partition_map` and an `NEVec::try_from_vec` test of the refused side, `try_from_vec` wrapping a failure list a package returns built. Subjects one check refuses reach its variant through `try_into_nonempty_iter().map(|subjects| <Variant> { subjects: subjects.collect() }).map_or(Ok(()), Err)`. Functions returning `Result<T, NEVec<E>>` lift into a join through `map_or_else(Validated::Fail, Validated::Good)`, `Validated::from` wrapping a single `E`. Crate functions every caller joins return `Validated`. Dependent heads feeding independent later steps bind then join in one expression, `head.map_or_else(Validated::fail, |head| rest.map(step).collect::<Validated<Vec<_>, _>>().map(|tail| NEVec::from((head, tail))))`.

## [03]-[BINDING]

Steps reading joined values bind through `.ok()?`, and later dependent steps bind through `?`:

```rust
pub fn cleared(headroom: &OrdMap<UnitId, u32>, form: &BidForm) -> Result<(OrdMap<UnitId, u32>, Bid), NEVec<MarketError>> {
    let bid = bid(form).ok()?;
    let refused = |reason| MarketError::Refused { unit: form.unit, reason };
    let left = headroom.get(&form.unit).ok_or_else(|| refused(Refusal::Missing))?.checked_sub(bid.volume.into_inner()).ok_or_else(|| refused(Refusal::Short))?;
    Ok((headroom.update(form.unit, left), bid))
}
```

`MarketError` holds `impl From<MarketError> for NEVec<MarketError>` over `nev![value]`, each `?` lifting through it. Each dependent step names the reason its absence or underflow means, and one closure joins the subject every reason refuses. Absence becomes a default (`unwrap_or_default`) only where the domain defines one: a count of nothing is zero, and a missing headroom entry is a refusal. `update` returns a successor sharing structure with an unchanged `headroom`.

## [04]-[EARLY_EXIT]

`try_fold` threads its accumulator as `Continue` and stops at the first `Break`:

```rust
pub fn first_over_budget(costs: &[Decimal], budget: Decimal) -> ControlFlow<usize, Decimal> {
    costs.iter().enumerate().try_fold(Decimal::ZERO, |spent, (index, cost)| spent.checked_add(*cost).filter(|total| *total <= budget).map_or(ControlFlow::Break(index), ControlFlow::Continue))
}
```

`Break` holds the first index whose running total overflows or passes `budget`, and `Continue` holds a total within it. `?` on a `ControlFlow` returns its `Break`, and a recursive walk or an outer `try_fold` stops at the first `Break` a child or an inner fold returns. `break_ok` and `continue_ok` turn the outcome into a `Result`, `into_value` under `control_flow_into_value` reads values of one type, after `map_continue` or `map_break` wraps one side's value (`map_continue(Ok)`), and a fold editing its state in place is `try_for_each` over `&mut` state with `ControlFlow<()>`. `by_ref()` before `try_fold` leaves the unconsumed items readable after a `Break`.
