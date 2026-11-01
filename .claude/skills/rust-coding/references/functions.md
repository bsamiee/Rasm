# [FUNCTIONS]

Covers functions as values.

## [01]-[SELECTORS]

Cursor pages over sorted rows are one generic function taking the row's key:

```rust
#[must_use]
pub fn page<T, K>(rows: &[T], mut key: impl FnMut(&T) -> K, after: Option<K>, size: NonZero<usize>) -> (&[T], Option<K>)
where K: Ord {
    let (_, rest) = rows.split_at_checked(after.map_or(0, |cursor| rows.partition_point(|row| key(row) <= cursor))).unwrap_or_default();
    let (taken, left) = rest.split_at_checked(size.get()).unwrap_or((rest, &[]));
    (taken, taken.last().filter(|_| !left.is_empty()).map(key))
}
```

`page(&orders, |order| order.id, cursor, size)` serves every listing sorted by a unique key. `partition_point` binary-searches past the cursor, and a page longer than the rows left takes them all. The next cursor is the last row's key while a row remains.

## [02]-[RULES]

Threshold rules are one `match` over a tuple of readings:

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

`Celsius` and `Percent` wrap an integer in a public field, and a threshold edit that opens a gap between arms fails the exhaustiveness check. Findings a report collects without failing are an enum with one variant per kind rendering its message. `VariantArray` lists the variants, one `match` holds each predicate, and a new variant fails to compile until its predicate exists:

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

`findings(order).collect::<Vec<_>>()` reports every violated check, and `findings(order).next().is_none()` answers whether the order passes and stops at its first finding.

## [03]-[STAGING]

Rate bands index once into a `BTreeMap` the returned closure reads per spend:

```rust
pub fn rate_for(bands: &[(Cents, Rate)]) -> impl Fn(Cents) -> Option<Rate> + use<> {
    let floors: BTreeMap<Cents, Rate> = bands.iter().copied().collect();
    move |spend| floors.range(..=spend).next_back().map(|(_, rate)| *rate)
}
```

`range(..=spend).next_back()` reads the highest floor at or below a spend in logarithmic time, and a spend below every floor is `None`. `use<>` states that the closure borrows nothing and outlives `bands`. Closures from one factory share its return type, and `[rate_for(&retail), rate_for(&wholesale)]` is an array with no box. Closures written at different sites are different types: non-capturing ones coerce to one `fn` pointer type, and capturing ones share a collection as `Box<dyn Fn>`.

## [04]-[COMPOSITION_ROOT]

Composition-root factories take the capability trait with their effects as methods, and the returned closure runs a load, a pure transition, and a save per request:

```rust
pub fn depositing<L>(ledger: L) -> impl AsyncFn(Deposit) -> Result<Account, LedgerError>
where L: Ledger {
    async move |command: Deposit| {
        let next = ledger.load(command.account).await?.ok_or(LedgerError::Refused { account: command.account, reason: Refusal::Missing })?.post(command.amount)?;
        ledger.save(&next).await.map(|()| next)
    }
}
```

`Ledger` declares `load` returning `Result<Option<Account>, LedgerError>` and `save` returning `Result<(), LedgerError>` as `async fn` methods. `main` binds `depositing(store)` once over the store it opened, and the server awaits that closure per request. Each call's future borrows the ledger the closure owns, and no request clones a handle.
