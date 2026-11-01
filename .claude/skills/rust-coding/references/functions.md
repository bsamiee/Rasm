# [FUNCTIONS]

Covers functions as values, from selectors and rule selection to staged closures and the composition root.

## [01]-[SELECTORS]

Functions written once take their varying part as a closure or future parameter, and work repeated across types or handlers is one generic function in place of a macro or a copy per site. A page of sorted rows takes its key:

```rust
#[must_use]
pub fn page<T, K>(rows: &[T], mut key: impl FnMut(&T) -> K, after: Option<K>, size: NonZero<usize>) -> (&[T], Option<K>)
where K: Ord {
    let rest: &[T] = rows.split_at(after.map_or(0, |cursor| rows.partition_point(|row| key(row) <= cursor))).1;
    let (taken, left): (&[T], &[T]) = rest.split_at_checked(size.get()).unwrap_or((rest, &[]));
    (taken, left.first().and(taken.last()).map(key))
}
```

`page(&orders, |order| order.id, cursor, size)` serves every listing sorted by a unique key. `partition_point` binary-searches past the cursor and returns an index within the slice, so `split_at` holds, and the next cursor is the last row's key while a row remains.

## [02]-[RULES]

Ordered conditions over fixed thresholds are one `match`: range patterns and guards state each condition and arm order states priority. A last arm listing the remaining ranges in place of a wildcard lets the compiler prove the thresholds cover every value:

```rust
#[must_use]
pub const fn level(reading: &Reading) -> Level {
    match (reading.celsius, reading.humidity) {
        (Celsius(60..), _) => Level::Critical,
        (Celsius(40..), _) | (_, Percent(90..)) => Level::Warning,
        (Celsius(..40), Percent(..90)) => Level::Normal,
    }
}
```

`Celsius` and `Percent` wrap an integer in a public field, and a threshold edit that opens a gap between arms fails the exhaustiveness check. Checks a caller reports by name are a fieldless enum: `VariantArray` lists the checks, one `match` holds each predicate, and a new variant fails to compile until its predicate exists:

```rust
#[derive(Debug, Clone, Copy, PartialEq, Eq, strum::VariantArray, derive_more::Display)]
pub enum Finding {
    #[display("order holds no lines")]
    Empty,
    #[display("order total exceeds its credit limit")]
    OverLimit,
    #[display("shipping address is unverified")]
    Unverified,
}

pub fn findings(order: &Order) -> impl Iterator<Item = Finding> {
    Finding::VARIANTS.iter().copied().filter(move |finding| match finding {
        Finding::Empty => order.lines.is_empty(),
        Finding::OverLimit => order.total > order.limit,
        Finding::Unverified => !order.address.verified,
    })
}
```

`Order` holds `lines`, `total`, `limit`, and `address.verified`. `findings(order).collect::<Vec<_>>()` reports every violated check, and `findings(order).next().is_none()` answers whether the order passes and stops at its first finding.

## [03]-[STAGING]

Partial application is staging: a factory takes the fixed arguments, does once the work they allow (compile, sort, index), and returns a closure doing the per-input rest. A factory whose closure only stores its arguments restates the function it wraps:

```rust
#[must_use]
pub fn rate_for(bands: &[(Cents, Rate)]) -> impl Fn(Cents) -> Option<Rate> + use<> {
    let floors: BTreeMap<Cents, Rate> = bands.iter().copied().collect();
    move |spend| floors.range(..=spend).next_back().map(|(_, rate)| *rate)
}
```

`range(..=spend).next_back()` reads the highest floor at or below a spend in logarithmic time, and a spend below every floor is `None`. `use<>` states that the closure borrows nothing, so it outlives `bands` where edition 2024 ties a returned `impl Trait` to every lifetime in scope. Closures from one factory share its return type, so `[rate_for(&retail), rate_for(&wholesale)]` is an array with no box. Closures written at different sites are different types: non-capturing ones coerce to one `fn` pointer type, and capturing ones share a collection as `Box<dyn Fn>`.

## [04]-[COMPOSITION_ROOT]

Composition roots read configuration, build each capability and specialization once, and pass each handler what it calls. A handler is generic over a capability trait whose methods are its effects, and a request alternates a load effect, a pure transition, and a save effect:

```rust
pub async fn deposit<L>(ledger: &L, now: Timestamp, command: Deposit) -> Result<Receipt, LedgerError>
where L: Ledger {
    let next: Account = ledger.load(command.account).await?.ok_or(LedgerError::Refused { account: command.account, reason: Refusal::Missing })?.deposit(command.amount)?;
    ledger.save(&next).await?;
    Ok(Receipt { account: next.id, balance: next.balance, at: now })
}
```

`Ledger` declares `load` returning `Result<Option<Account>, LedgerError>` and `save` returning `Result<(), LedgerError>` as `async fn` methods. A handler calling one effect takes it as an `impl AsyncFn` parameter in place of a trait, and a test passes an async closure.
