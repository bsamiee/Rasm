# [FUNCTIONS]

## [01]-[KEY_EXTRACTION]

Cursor pages over sorted rows are one generic function taking the row's key:

```rust
#[must_use]
pub fn page<T, K>(rows: &[T], mut key: impl FnMut(&T) -> K, after: Option<K>, size: NonZero<usize>) -> (&[T], Option<K>)
where K: Ord {
    let (_, rest) = rows.split_at(after.map_or_default(|cursor| rows.partition_point(|row| key(row) <= cursor)));
    let (taken, left) = rest.split_at_checked(size.get()).unwrap_or((rest, &[]));
    (taken, taken.last().filter(|_| !left.is_empty()).map(key))
}
```

`page(&orders, |order| order.id, cursor, size)` serves listings sorted by a unique key. `partition_point` supplies a valid split of the same slice, requiring no fallback. A requested page longer than remaining rows takes them all. The next cursor is the last returned key while more rows remain.

## [02]-[RULES]

Threshold rules are one `match` over a tuple of readings, each guard testing a bound of the caller's validated record:

```rust
#[nutype(
    validate(predicate = |band| !band.is_empty()),
    derive(Debug, Clone, Copy, PartialEq, Eq, Deref)
)]
pub struct Warning(Range<Celsius>);

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Bands {
    pub heat: Warning,
    pub humidity: Percent,
}

#[must_use]
pub fn level(reading: &Reading, bands: &Bands) -> Level {
    match (reading.celsius, reading.humidity) {
        (celsius, _) if celsius >= bands.heat.end => Level::Critical,
        (celsius, humidity) if celsius >= bands.heat.start || humidity >= bands.humidity => Level::Warning,
        (_, _) => Level::Normal,
    }
}
```

`Celsius` and `Percent` are `Copy` integer wrappers deriving `Ord`, `Range` is the `core::range` type, and `Warning` refuses a critical bound at or below its warning bound, so every constructible `Bands` leaves a warning band between `Normal` and `Critical`. Findings a report collects without failing are an enum with one variant per kind rendering its message. `VariantArray` lists the variants, one `match` holds each predicate, and a new variant fails to compile until its predicate exists:

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

`findings(order).collect::<Vec<_>>()` reports every violated check, `findings(order).next().is_none()` tests a pass and stops at the first finding.

## [03]-[RETURNED_CLOSURES]

Rate bands index once into a `BTreeMap` the returned closure reads per spend:

```rust
pub fn rate_for(bands: &[(Cents, Rate)]) -> impl Fn(Cents) -> Option<Rate> + use<> {
    let floors: BTreeMap<Cents, Rate> = bands.iter().copied().collect();
    move |spend| floors.range(..=spend).next_back().map(|(_, rate)| *rate)
}
```

`range(..=spend).next_back()` reads the highest floor at or below a spend in logarithmic time, and a spend below every floor is `None`. `use<>` states that the closure borrows nothing and outlives `bands`. Closures from one factory share its return type, and `[rate_for(&retail), rate_for(&wholesale)]` is an array with no box.

## [04]-[COMPOSITION_ROOT]

Factories bind a resource once and return an async closure applying a pure transition through that resource's atomic update:

```rust
pub type Depositing<L: Ledger> = impl AsyncFn(Deposit) -> Result<Account, LedgerError>;

#[define_opaque(Depositing)]
pub fn depositing<L>(ledger: L) -> Depositing<L>
where L: Ledger {
    async move |command: Deposit| ledger.update(command.account, |account| account.post(command.amount)).await
}
```

`Ledger::update` reads an account and commits a successful transition atomically, returning `Result<Account, LedgerError>`. Missing accounts and refused transitions leave stored state unchanged. Native transactions or conditional writes implement that contract; separate load and save calls lose concurrent updates. Retryable update callbacks remain pure. `main` binds `depositing(store)` once, each request borrowing the captured ledger. `#[define_opaque]` fixes `Depositing<L>` to the returned closure type; records hold `deposit: Depositing<Store>` without a box or another type parameter.
