# [RESULTS]

## [01]-[VALIDATION]

Forms validate each field through its `nutype` constructor, and `ShopError` takes each generated error through a transparent `#[from]` variant:

```rust
#[must_use]
pub fn order(form: &OrderForm) -> Validated<Order, ShopError> {
    Validated::from(Email::try_new(&form.email).map_err(ShopError::from)).map2(Validated::from(Quantity::try_new(form.quantity).map_err(ShopError::from)), Order::new)
}
```

`map2` runs `Order::new` (from `derive_more::Constructor`) only when both fields pass. Forms over four fields nest `order` as one operand.

## [02]-[TRAVERSAL]

Items join through `collect`:

```rust
#[must_use]
pub fn products<'catalog>(catalog: &'catalog Catalog, cart: &Cart) -> Validated<Vec<&'catalog Product>, ShopError> {
    cart.lines.iter().map(|line| catalog.products.get(&line.product).ok_or(ShopError::Refused { product: line.product, reason: Refusal::Missing })).collect()
}
```

Each unlisted product reports its own `Refused`, an empty cart passes, and consumers read the found products with no second lookup. Items yielding `Validated` collect the same way into one `NEVec` of every failure. Checks returning nothing collect into `Validated<(), E>` through `bool::ok_or`.

## [03]-[BINDING]

Steps reading joined values bind through `.ok()?`, and one `From` impl per crate error lifts each later failure through `?`:

```rust
impl From<ShopError> for NEVec<ShopError> {
    fn from(value: ShopError) -> Self { Self::new(value) }
}

pub fn place(stock: &OrdMap<ProductId, u32>, form: &OrderForm) -> Result<(OrdMap<ProductId, u32>, Order), NEVec<ShopError>> {
    let order = order(form).ok()?;
    let refused = |reason| ShopError::Refused { product: form.product, reason };
    let left = stock.get(&form.product).ok_or_else(|| refused(Refusal::Missing))?.checked_sub(order.quantity.into_inner()).ok_or_else(|| refused(Refusal::Short))?;
    Ok((stock.update(form.product, left), order))
}
```

Each dependent step names the reason its absence or underflow means, and one closure joins the subject every reason refuses. Absence becomes a default (`unwrap_or_default`) only where the domain defines one: a count of nothing is zero, and a missing stock entry is a refusal. `update` returns a successor sharing structure with an unchanged `stock`.

## [04]-[EARLY_EXIT]

`try_fold` threads its accumulator as `Continue` and stops at the first `Break`:

```rust
pub fn first_over_budget(costs: &[Decimal], budget: Decimal) -> ControlFlow<usize, Decimal> {
    costs.iter().enumerate().try_fold(Decimal::ZERO, |spent, (index, cost)| spent.checked_add(*cost).filter(|total| *total <= budget).map_or(ControlFlow::Break(index), ControlFlow::Continue))
}
```

`Break` holds the first index whose running total overflows or passes `budget`, and `Continue` holds a total within it. `?` on a `ControlFlow` returns its `Break`, and a recursive walk stops at the first `Break` any child returns.
