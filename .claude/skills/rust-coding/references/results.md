# [RESULTS]

Covers result-type flows over one crate error.

## [01]-[CRATE_ERROR]

`ShopError` is the crate error every flow raises:

```rust
#[derive(Debug, thiserror::Error)]
pub enum ShopError {
    #[error("{reason}: {product}")]
    Refused { product: ProductId, reason: Refusal },
    #[error(transparent)]
    Email(#[from] EmailError),
    #[error(transparent)]
    Quantity(#[from] QuantityError),
}
```

`Refusal` holds `Missing` and `Short` under one `ProductId` subject, and each `nutype` error message names its type.

## [02]-[ABSENCE]

Dependent lookups sequence with `?`, independent ones join through `zip`, and the first `None` ends the function:

```rust
#[must_use]
pub fn shipping(catalog: &Catalog, line: &Line) -> Option<Decimal> {
    let (product, zone) = catalog.products.get(&line.product).zip(catalog.zones.get(&line.destination))?;
    catalog.rates.get(&(product.class, *zone))?.checked_mul(line.quantity.into_inner().into())
}
```

Defaults replacing absence (`unwrap_or_default`) stay where the domain defines one: a count of nothing is zero, and a missing price is a failure.

## [03]-[DEPENDENT_STEPS]

Multi-step transitions name a reason at each step where absence or underflow appears, and one closure joins the subject every reason refuses:

```rust
pub fn reserve(stock: &imbl::OrdMap<ProductId, u32>, product: ProductId, quantity: Quantity) -> Result<imbl::OrdMap<ProductId, u32>, ShopError> {
    let refused = |reason| ShopError::Refused { product, reason };
    let held = stock.get(&product).ok_or_else(|| refused(Refusal::Missing))?;
    held.checked_sub(quantity.into_inner()).map(|left| stock.update(product, left)).ok_or_else(|| refused(Refusal::Short))
}
```

`update` returns a successor sharing structure with an unchanged `stock`.

## [04]-[VALIDATION]

Forms validate each field through its `nutype` constructor:

```rust
#[must_use]
pub fn order(form: &OrderForm) -> Validated<Order, ShopError> {
    Validated::from(Email::try_new(&form.email).map_err(ShopError::from)).map2(Validated::from(Quantity::try_new(form.quantity).map_err(ShopError::from)), Order::new)
}
```

`map2` runs `Order::new` (from `derive_more::Constructor`) only when both fields pass, a larger form joins `order` as one operand, and five or more fields nest `map2` to `map4` calls over tuples. Steps reading joined values bind through `.ok()?`, and one `From` impl per crate error lifts each later failure through `?`:

```rust
impl From<ShopError> for NEVec<ShopError> {
    fn from(value: ShopError) -> Self { Self::new(value) }
}

pub fn place(stock: &imbl::OrdMap<ProductId, u32>, form: &OrderForm) -> Result<(imbl::OrdMap<ProductId, u32>, Order), NEVec<ShopError>> {
    let order = order(form).ok()?;
    Ok((reserve(stock, form.product, order.quantity)?, order))
}
```

Items join through `collect`:

```rust
#[must_use]
pub fn products<'catalog>(catalog: &'catalog Catalog, cart: &Cart) -> Validated<Vec<&'catalog Product>, ShopError> {
    cart.lines.iter().map(|line| catalog.products.get(&line.product).ok_or(ShopError::Refused { product: line.product, reason: Refusal::Missing })).collect()
}
```

Each unlisted product reports its own `Refused`, an empty cart passes, and consumers read the found products with no second lookup. Checks with nothing to return collect into `Validated<(), E>` through `bool::ok_or`, and `sum::<Validated<Decimal, E>>()` totals fallible amounts while keeping every failure.

## [05]-[EARLY_EXIT]

`try_fold` threads its accumulator as `Continue` and stops at the first `Break`:

```rust
pub fn first_over_budget(costs: &[Decimal], budget: Decimal) -> ControlFlow<usize, Decimal> {
    costs.iter().enumerate().try_fold(Decimal::ZERO, |spent, (index, cost)| spent.checked_add(*cost).filter(|total| *total <= budget).map_or(ControlFlow::Break(index), ControlFlow::Continue))
}
```

`Break` holds the first index whose running total overflows or passes `budget`, and `Continue` holds a total within it. `?` on a `ControlFlow` returns its `Break`, and a recursive walk stops at the first `Break` any child returns.
