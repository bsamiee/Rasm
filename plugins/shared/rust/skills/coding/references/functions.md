# [FUNCTIONS]

## [01]-[KEY_EXTRACTION]

Cursor pages over sorted rows are one generic function taking the row's key:

```rust
#[must_use]
pub fn page<T, K>(rows: &[T], mut key: impl FnMut(&T) -> K, after: Option<K>, size: NonZero<usize>) -> (&[T], Option<K>)
where K: Ord {
    let start = after.map_or_default(|cursor| rows.partition_point(|row| key(row) <= cursor));
    let (_, rest) = rows.split_at(start);
    let (taken, left) = rest.split_at_checked(size.get()).unwrap_or((rest, &[]));
    (taken, taken.last().filter(|_| !left.is_empty()).map(key))
}
```

`page(&orders, |order| order.id, cursor, size)` serves listings sorted by a unique key:
- `partition_point` returns an index `split_at` accepts on every input
- Pages longer than the remaining rows take them all
- `page` returns the last key it took as its cursor while more rows remain

## [02]-[RULE_TABLES]

Rules differing per case are rows of one `const` table, each naming a field, the cases refusing it, and its test, and one filter reads every row:

```rust
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Carrier {
    Post,
    Courier,
    Freight,
}

pub fn refused(parcel: &Parcel, carrier: Carrier) -> impl Iterator<Item = Field> {
    const RULES: [(Field, &[Carrier], fn(&Parcel) -> bool); 4] = [
        (Field::Weight, &[Carrier::Post], |parcel| parcel.grams > 2_000),
        (Field::Weight, &[Carrier::Courier], |parcel| parcel.grams > 30_000),
        (Field::Insurance, &[Carrier::Post], |parcel| parcel.insured.is_some()),
        (Field::Signature, &[Carrier::Post, Carrier::Freight], |parcel| parcel.signed),
    ];
    RULES.into_iter().filter(move |(_, refusing, test)| refusing.contains(&carrier) && test(parcel)).map(|(field, _, _)| field)
}
```

`Parcel` holds `grams`, `insured`, and `signed`, and `Field` names each of them:
- `RULES`'s type names its `fn` column, and each closure capturing nothing coerces to it with no `as fn(..)` cast
- New carriers and rules are one row each, where a function per carrier restates each test it shares with another

## [03]-[FINDINGS]

Checks a report collects without failing are one enum with a variant per check, its name the finding's text, and one `match` holds each predicate:

```rust
#[derive(Debug, Clone, Copy, PartialEq, Eq, VariantArray, Display)]
#[display(rename_all = "kebab-case")]
pub enum Finding {
    Empty,
    OverLimit,
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

`findings(order).collect::<Vec<_>>()` reports every violated check, and `findings(order).next().is_none()` stops at the first finding to test a pass:
- `VariantArray` lists the variants, and a new variant fails to compile until its predicate exists

## [04]-[RETURNED_CLOSURES]

Factories do once the work their fixed arguments allow and return a closure for the one operation left:

```rust
pub fn rate_for(bands: &[(Cents, Rate)]) -> impl Fn(Cents) -> Option<Rate> + use<> {
    let floors: BTreeMap<Cents, Rate> = bands.iter().copied().collect();
    move |spend| floors.range(..=spend).next_back().map(|(_, rate)| *rate)
}
```

`range(..=spend).next_back()` reads the highest floor at or below a spend in logarithmic time, and a spend below every floor is `None`:
- `use<>` states that the closure borrows nothing and outlives `bands`
- Closures from one factory share its return type, and `[rate_for(&retail), rate_for(&wholesale)]` is an array with no box

## [05]-[COMPOSITION_ROOT]

Factories bind a resource once and return an async closure applying a pure transition through the resource's atomic update:

```rust
pub type Depositing<L: Ledger> = impl AsyncFn(Deposit) -> Result<Account, LedgerError>;

#[define_opaque(Depositing)]
pub fn bind<L>(ledger: L) -> Depositing<L>
where L: Ledger {
    async move |command: Deposit| ledger.update(command.account, |account| account.post(command.amount)).await
}
```

`Ledger::update` commits the transition its closure returns in one atomic step, where a load and a save apart lose a concurrent update:
- `main` calls `bind(store)` once, and each request borrows the captured ledger
- `#[define_opaque]` under `type_alias_impl_trait` fixes `Depositing<L>` to the returned closure type
- Opaque aliases take no inherent or trait `impl`, and `bind` stays a free `fn` named by its verb
- Records hold `deposit: Depositing<Store>` with no box or type parameter
