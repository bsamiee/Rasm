# [RESULTS]

Covers result-type flows, from the crate error and absence to dependent steps, accumulated validation, early exit, and host translation.

## [01]-[CRATE_ERROR]

Every flow raises one crate error, each variant holding the subject a consumer needs and its reason as data:

```rust
#[derive(Debug, thiserror::Error)]
pub enum ShopError {
    #[error("{reason}: {product}")]
    Refused { product: ProductId, reason: Refusal },
    #[error("email rejected")]
    Email(#[from] EmailError),
    #[error("quantity rejected")]
    Quantity(#[from] QuantityError),
}
```

`Refusal` is a fieldless enum rendering each reason (`Missing`, `Short`) through `Display`, and both reasons share a `ProductId` subject. `Email` and `Quantity` hold generated `nutype` errors with no subject to add, so `?` and `ShopError::from` lift them through `#[from]`. Messages open lowercase, end without punctuation, and leave out the source: `Report` appends each source after `: `, and `email rejected: <source message>` reads as one line.

## [02]-[ABSENCE]

Lookups return `Option` (`get`, `find`, `first`, `checked_*`). Dependent lookups sequence with `?`, independent ones join through `zip`, and the first `None` ends the function:

```rust
#[must_use]
pub fn shipping_rate(catalog: &Catalog, line: &Line) -> Option<Cents> {
    let (product, zone) = catalog.products.get(&line.product).zip(catalog.zones.get(&line.destination))?;
    catalog.rates.get(&(product.class, *zone))?.checked_mul(line.quantity.into_inner())
}
```

Product and zone read nothing from each other, and the rate lookup reads both before `checked_mul` scales it. Absence becomes a failure at the call that knows its reason: `ok_or` takes an error built from literals and copied fields, `ok_or_else` one built by a call or an allocation. A default replacing absence (`unwrap_or_default`) stays where the domain defines it, a count of nothing is zero and a missing price is a failure.

## [03]-[DEPENDENT_STEPS]

State transitions take the state and return its successor in a `Result`. Steps work in `Result<_, Refusal>` and add the subject once at their end:

```rust
impl Stock {
    pub fn reserve(self, line: &Line) -> Result<Self, ShopError> {
        self.counts
            .get(&line.product)
            .ok_or(Refusal::Missing)
            .and_then(|held| held.checked_sub(line.quantity.into_inner()).ok_or(Refusal::Short))
            .map(|left| Self { counts: self.counts.update(line.product, left) })
            .map_err(|reason| ShopError::Refused { product: line.product, reason })
    }
}
```

`Stock::counts` is an `imbl::HashMap<ProductId, u32>`, so `update` returns a successor map sharing structure with its source. `ok_or` names each reason where absence or underflow appears, `and_then` runs the subtraction only on a held count, and `map_err` builds `ShopError::Refused` once with its product. Carts reserve through `cart.lines.iter().try_fold(stock, Stock::reserve)`, each line reading the stock its predecessor left until a refusal stops.

- `?` lifts a step's error through `From`, `Quantity::try_new(raw)?` needs no `map_err`
- Optional input parses through `map` and `transpose`: `form.note.as_deref().map(Note::try_new).transpose()?`
- Conversions that cannot fail return `Result<T, Infallible>`, and `let Ok(value) = converted;` binds without a branch

## [04]-[VALIDATION]

Independent fields join through `Validated`, and a caller repairing every violation at once receives each failure:

```rust
#[must_use]
pub fn signup(form: &SignupForm) -> Validated<Signup, ShopError> {
    Validated::from(Email::try_new(&form.email).map_err(ShopError::from)).map2(Validated::from(Quantity::try_new(form.quantity).map_err(ShopError::from)), Signup::new)
}
```

`Validated::from` lifts each `Result` once `map_err` moves its error into `ShopError`, and `map2` runs `Signup::new` (derived by `derive_more::Constructor`) only when both fields pass. Returning `Validated` lets a larger form join `signup` as one operand, and the reporting boundary calls `ok` for `Result<Signup, NEVec<ShopError>>`. Five or more fields nest `map2` to `map4` calls, each building a sub-record. `Validated` has no `and_then`: a rule reading joined values runs on the `Result` from `ok` and joins its one failure through `map_err(NEVec::new)`.

Checks over every item with no value to return collect into `Validated<(), E>`, and an empty input passes:

```rust
#[must_use]
pub fn listed(catalog: &Catalog, cart: &Cart) -> Validated<(), ShopError> {
    cart.lines.iter().map(|line| catalog.products.contains_key(&line.product).then_some(()).ok_or(ShopError::Refused { product: line.product, reason: Refusal::Missing })).collect()
}
```

Each unlisted product reports its own `Refused`, and `ok` returns `Result<(), NEVec<ShopError>>`.

## [05]-[EARLY_EXIT]

`ControlFlow<B, C>` ends a fold early with a value that is no failure. `try_fold` threads its accumulator as `Continue` and stops at the first `Break`:

```rust
pub fn first_over_budget(costs: &[Cents], budget: Cents) -> ControlFlow<usize, Cents> {
    costs.iter().enumerate().try_fold(Cents::default(), |spent, (index, cost)| spent.checked_add(*cost).filter(|total| *total <= budget).map_or(ControlFlow::Break(index), ControlFlow::Continue))
}
```

`Break` holds the first index whose running total passes `budget`, and `Continue` holds a total within it. `filter` folds an overflow and a total past `budget` into one `None`, and `map_or` maps `None` to `Break`.

- `?` on a `ControlFlow` returns its `Break`, so a recursive walk stops at the first `Break` any child returns
- `break_ok` converts to `Result<B, C>` for a consumer treating `Continue` as the failure, `continue_ok` converts the other way
- `break_value` and `continue_value` keep one side as an `Option`
- Searches ending at a found value are `find_map`, folds ending at a failure are `try_fold` over `Result`

## [06]-[TRANSLATION]

Hosts derive each outcome from the variant and its reason:

```rust
pub fn exit_code(failure: &ShopError) -> ExitCode {
    match failure {
        ShopError::Refused { product: _, reason: Refusal::Short } => ExitCode::from(75_u8),
        ShopError::Refused { product: _, reason: Refusal::Missing } | ShopError::Email(_) | ShopError::Quantity(_) => ExitCode::from(65_u8),
    }
}
```

Shortage clears once stock arrives and exits as a temporary failure (75), rejected input exits as a data error (65). Terminals render `Report::new(failure).pretty(true)`, listing each source on its own line under `Caused by:`.
