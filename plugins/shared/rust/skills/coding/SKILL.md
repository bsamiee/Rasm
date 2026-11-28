---
name: coding
description: "Use when writing or reviewing Rust, covering functional design with algebraic types, results, traits, and async effects"
---

# [RUST_CODING]

Code treats each crate as part of the language:
- Errors and exit status: `thiserror`, `sysexits`
- Validated values and derives: `validated`, `nutype`, `derive_more`, `strum`
- Collections and iteration: `nonempty-collections`, `enum-map`, `itertools`
- Async: `tokio`, `tokio-util`, `futures`
- Wire, time, and money: `serde`, `jiff`, `rust_decimal`
- Tracing: `tracing`

[REFERENCES]:
- [01]-[FUNCTIONS](references/functions.md): Functions as values
- [02]-[TYPES](references/types.md): Domain type design
- [03]-[RESULTS](references/results.md): Fallible flows
- [04]-[TRAITS](references/traits.md): Capability traits
- [05]-[ITERATORS](references/iterators.md): Iterator pipelines, sequential and parallel
- [06]-[ASYNC](references/async.md): Async effects
- [07]-[BOUNDARIES](references/boundaries.md): Program input and output
- [08]-[LANGUAGE_FEATURES](references/language-features.md): Nightly language features
- [09]-[NUMERICS](references/numerics.md): Numeric code

## [01]-[FUNCTIONS]

Parameter types state every value a function accepts, and its return type every outcome a caller handles:
- Parameters run from dependencies (capability traits, closures) and configuration, to a behavior policy, to the runtime value
- Parameters take their target type, and callers convert through `.into()` in place of an `impl Into<T>` parameter
- Async callbacks take `impl AsyncFn(A) -> B`, filled by an `async` closure or an `async fn` passed by name (`fetch`)
- Members and local `macro_rules!` differing only in a number, type, flag, or applied function are one function taking it as a parameter
- Choices a caller names are an enum parameter, and `bool` holds a computed fact
- Defaults needing no context are the field's own (`field: T = value` under `default_field_values`), and `Option` holds absence a later owner fills
- Values differing per platform or build are one `cfg_select!` around the differing part, and an item one platform alone holds takes `#[cfg]`
- Values following a rule compute from parameters or a variant's rank (`plan >= Plan::Team`), and a table holds values no rule derives
- Facts a dependency or the build states come from it (a protocol version from its package's constant, `env!("CARGO_PKG_NAME")`)
- Strings hold text a reader receives or a wire's fixed token, and a kind, key, or name derives from its owner (a type, a variant, a field)
- Text no program prints and no host shows goes
- Inherent methods exist only where they keep their type's invariant, and other behavior goes in functions
- Helpers one function reads are closures inside it, or a nested `fn` that is generic, `const`, recursive, or returns a borrow of a parameter
- Bodies doing more than one job split by job into functions reading their parameters, never into a run of named closures
- Lints wrong for one site take `#[expect(<lint>, reason = "<fact>")]`, and a lint wrong for a whole crate leaves its manifest with that reason

### [01.1]-[TOTAL_FUNCTIONS]

Total functions produce a declared output for each declared input, a partial one narrowing its input to a constructed type or widening its output:
- `impl From<u32> for Tier` is incomplete when some counts are invalid, `impl From<Quantity> for Tier` holds for every constructible `Quantity`
- Empty collections are results, and `Option<Vec<T>>` exists only where a consumer treats no collection and an empty one apart
- Inputs a dependency asserts on are refused at construction or before its call by a refusal naming the value, never clamped into range
- Associated constants a generic body needs in range take `const { assert!(<test>, "<invariant>") }`, failing at monomorphization

### [01.2]-[PURITY]

Pure functions take no `async`, `&mut`, or capability trait, and the composition root passes clock readings and host state in:

```rust
#[must_use]
pub fn expired(token: &Token, now: Timestamp, leeway: SignedDuration) -> bool { now.duration_since(token.expires) > leeway }
```

## [02]-[EXPRESSIONS]

Returned values are one struct literal setting each field, and a step a later step reads is a named `let`, never a chain a reader must unpick:

```rust
impl From<&[Reading]> for Profile {
    fn from(readings: &[Reading]) -> Self {
        let span = readings.iter().map(|reading| reading.at).minmax().into_option();
        let peak = readings.iter().max_by_key(|reading| reading.celsius).map(|reading| reading.sensor);
        Self { count: readings.len(), span, peak }
    }
}
```

- Iterator pipelines hold every traversal, a named adapter (`minmax`, `counts_by`, `dedup`) in place of a `fold` or a buffer pass
- `fold` holds a reduction no member names, never a side collection a second pass zips back by index
- Set operations yielding a set take their operator (`&held & &stated`, `|`, `-`, `^`) in place of `intersection(..).copied().collect()`
- Values depending on more than one input come from one `match` over their tuple, and an input one arm alone reads goes in its guard
- Tuple scrutinees evaluate every member before matching, and a member with a side effect goes in a `let` chain ahead of the `match`
- Arms and closures of a combinator choosing a value (`map_or`, `unwrap_or_else`) nest no decision

### [02.1]-[PATTERN_MATCHING]

Pattern matching keeps recognition, extraction, guards, and results together:

```rust
impl Notice {
    #[must_use]
    pub fn of(delivery: &Delivery, now: Timestamp, recent: SignedDuration) -> Self {
        match delivery {
            Delivery::Shipped { carrier: _, tracking } => Self::Track(tracking.clone()),
            Delivery::Delivered { at } if now.duration_since(*at) < recent => Self::Arrived,
            Delivery::Returned { at: _, reason } => Self::Refund(*reason),
            Delivery::Pending | Delivery::Delivered { at: _ } => Self::Quiet,
        }
    }
}
```

- Matches over an enum without `#[non_exhaustive]` list every variant in place of a catch-all, a new variant failing each match handling it
- Patterns over a crate's own struct name every field (`field: _` for one ignored), and code tracking each field (a renderer) destructures it whole
- Arms read what their pattern and an `if let` guard bind, never a nested `match` re-reading a value the arm narrowed
- Arms no value reaches go with the shape allowing them, a type narrowed at construction replacing an `unreachable!` arm
- Matches through a smart pointer or an owned buffer are deref patterns, never a match on `as_str` or `as_slice`
- Slice patterns state each length (`[] => .., [first, ..] => ..`) in place of `first`, `split_last`, or a length test
- External values become an enum or an `Option` once near their source, and code matches that typed value

### [02.2]-[RECURSION]

Depth bound and the shape of the work select the form, as Rust guarantees no tail-call elimination:

| [INDEX] | [APPROACH]                    | [USE_WHEN]                        | [COST]                         |
| :-----: | :---------------------------- | :-------------------------------- | :----------------------------- |
|  [01]   | Direct recursion              | Small proven depth                | Each level holds a stack frame |
|  [02]   | `iter::successors`            | Unbounded depth, each state read  | Holds one state per step       |
|  [03]   | `try_fold` over a state range | Unbounded depth, final state read | Intermediate states are lost   |
|  [04]   | Arena with typed keys         | Shared or cyclic successors       | Graph built before the walk    |

Stack depth includes native callees and implicit `Drop`, `Clone`, and equality traversal.

## [03]-[TYPES]

Types make every constructible value valid, and counting their values (struct counts multiply, enum counts add) exposes states no domain holds.

### [03.1]-[ALGEBRAIC_TYPES]

Each state a consumer inspects is one variant holding exactly the data valid in that state:

```rust
#[derive(Debug, Clone, PartialEq, Eq, strum::EnumDiscriminants)]
#[strum_discriminants(name(Stage), derive(PartialOrd, Ord, Hash, enum_map::Enum))]
pub enum Delivery {
    Pending,
    Shipped { carrier: Carrier, tracking: Tracking },
    Delivered { at: Timestamp },
    Returned { at: Timestamp, reason: ReturnReason },
}
```

- `IntoDiscriminant::discriminant` returns a fieldless `Stage` ranked in declaration order, and `EnumMap<Stage, V>` holds one value per stage
- Data every variant shares goes in a struct beside a field holding the enum, never repeated per variant
- Variants differing only by a tag are one struct keyed by a fieldless enum, and fields named per key (`early_start`, `late_start`) one `EnumMap`
- Flags and `Option` fields one state reads become one variant per valid state, and fields read only together one `Option` of a record
- Results of more than one kind about one value are one `Vec` of their enum, never a record holding a list per kind
- `Result` holds failure alone, and a refused input is a failure, never a warning, a `filter_map` skip, or a lookup's `None` read as no constraint
- Variants standing for absence (`Closed`, `Unset`) are an `Option` around the enum, and foreign sentinels (id 0) become `None` where they enter

### [03.2]-[CONSTRAINED_VALUES]

Rules the domain owns become types whose constructor enforces them, and the inner value is reachable only through that constructor:
- `nutype` declares a validated newtype with its `<Name>Error` enum, or returns the crate error through `validate(with = <fn>, error = <Error>)`
- Values whose consumers read a derived quantity are a struct holding it, its constructor computing and testing it once
- Invariants consumers rely on (a sorted list) are a type each operation takes and returns, never a check repeated per use
- Guarded records expose fields read-only through `derive_more` `#[as_ref]`, and public fields hold values no rule across fields constrains
- Constants build in a const context, an invalid value failing the build, and a `NonZero` constant is `NonZero::new(<value>).unwrap()`
- Money is `Decimal`, and time is jiff's `Timestamp`, `Zoned`, `Span`, or `SignedDuration`, never a float or an integer count of units

### [03.3]-[OWNERSHIP]

Ownership states who decides a value's lifetime:
- Functions storing an argument take it by value, the caller deciding whether to clone, and readers borrow it
- Single-pass readers take `IntoIterator<Item: Borrow<T>>`, serving a borrowed `Vec`, an owned one, and `iter::once(value)` alike
- Buffers kept across calls hold at most the depth or batch their reader takes
- Values tasks share for reading are `Arc<str>`, `Arc<[T]>`, or the handle a dependency states for them
- Steps only some inputs take return `Cow<'_, T>`, the untouched path borrowing its input
- Interior mutability belongs to boundary types that own shared state, and domain types hold none

### [03.4]-[IMMUTABILITY]

Fallible transitions borrow the old state and return the next, and infallible ones consume the state and return it (`fn held(mut self) -> Self`):

```rust
impl Account {
    pub fn post(&self, amount: Decimal) -> Result<Self, LedgerError> {
        let refused = LedgerError::Refused { account: self.id, reason: Refusal::Overflow };
        let balance = self.balance.checked_add(amount).ok_or(refused)?;
        Ok(Self { balance, ..*self })
    }
}
```

- Struct update copies unchanged fields from a state (`..*self`), a foreign default, or a foreign value (`..event`)
- Stages deciding one field change its type by a defaulted parameter (`Job<R = Option<Retry>>`), `Job { retry, ..stated }` copying the rest
- `&mut self` methods stay on a type whose purpose is an owned buffer or an index updated in place (a builder, a parser cursor)

### [03.5]-[COLLECTIONS]

Collections follow their most frequent operation:

| [INDEX] | [OPERATION]                   | [COLLECTION]                                   |
| :-----: | :---------------------------- | :--------------------------------------------- |
|  [01]   | Ordered reads                 | `Vec<T>`, `Box<[T]>` once frozen               |
|  [02]   | Snapshots shared or versioned | `imbl::Vector`, `imbl::OrdMap`                 |
|  [03]   | Keyed, insertion order read   | `IndexMap`, `IndexSet`                         |
|  [04]   | Keyed, key order read         | `BTreeMap`, `BTreeSet`                         |
|  [05]   | Keyed, no order read          | `HashMap`, `HashSet`, `Vec` over a dense index |
|  [06]   | One value per enum variant    | `EnumMap`                                      |
|  [07]   | At least one element          | `NEVec`, `NESet`                               |
|  [08]   | Nodes referring to each other | `SlotMap` with typed keys                      |

## [04]-[RESULTS]

Every fallible function returns its outcome as a value, and one type serves one concern:

| [INDEX] | [TYPE]                             | [CONCERN]                                   |
| :-----: | :--------------------------------- | :------------------------------------------ |
|  [01]   | `Option<T>`                        | Absence that holds no reason                |
|  [02]   | `Result<T, E>`                     | Expected failure, `?` stops at the first    |
|  [03]   | `Result<T, NEVec<E>>`              | Every refusal of one pass                   |
|  [04]   | `Validated<T, E>`                  | Value built from independent fallible parts |
|  [05]   | `ControlFlow<B, C>`                | Early exit from a fold with a value         |
|  [06]   | `impl Stream<Item = Result<T, E>>` | Values over time, each failure one item     |

Input boundaries select the result type, functions keep it, and one point converts it by the target's method (`ok_or`, `ok`, `transpose`).

### [04.1]-[COMPOSITION]

Each step is a function, and its operator follows its signature and whether steps depend on each other:

| [INDEX] | [SIGNATURE]                       | [OPERATOR]                                        | [BEHAVIOR]                      |
| :-----: | :-------------------------------- | :------------------------------------------------ | :------------------------------ |
|  [01]   | `T -> Result<U, E>`               | `?` in sequence, `and_then` in a chain            | Continues with a dependent step |
|  [02]   | `Option<T>` or `bool` to `Result` | `ok_or`, `bool::ok_or`                            | Names the error absence means   |
|  [03]   | Independent `Result`s             | `Validated::from`, `map2` to `map4`               | Joins every failure             |
|  [04]   | Items with `T -> Result<U, E>`    | `collect::<Result<Vec<U>, E>>()`                  | Stops at the first failure      |
|  [05]   | Items with `T -> Result<U, E>`    | `collect::<Validated<Vec<U>, E>>()`               | Joins every failure             |
|  [06]   | Refusals of checks over one value | `NEVec::try_from_vec(all).map_or(Ok(value), Err)` | Value, or every refusal         |
|  [07]   | Items, both sides read            | `partition_result`                                | Splits successes from failures  |

- Checks over one built value each return `impl Iterator<Item = E>`, chained and collected once
- `Validated` joins fallible constructions, never a pass rechecking a value its type guarantees
- `NEVec` implements no `FromIterator`, and a `NonEmptyIterator` collects into it through `FromNonEmptyIterator`
- Steps returning `Result<(), E>` bind as `<step>?;` in a function or `try` block, never `.and_then(|()| ..)`
- Failures across elements are one variant over an `NEVec` of element and typed source pairs, never `NEVec<Self>`
- Infallible callbacks a foreign API types as fallible return `Ok::<_, !>(..)`, and the chain reads its value through `into_ok()`

### [04.2]-[ERRORS]

Panics mark a broken invariant. Refusals are one crate error enum, a variant per relation a caller branches on, and its derived `Display` is the one place their text is written:

```rust
#[derive(Debug, thiserror::Error)]
pub enum LedgerError {
    #[error("{account} is {reason}")]
    Refused { account: AccountId, reason: Refusal },
    #[error("{account} is unreadable")]
    Store { account: AccountId, source: io::Error },
}
```

`Refusal` is a fieldless enum deriving `derive_more::Display` under `#[display(rename_all = "kebab-case")]`, its variant names as its text:
- Messages render typed fields, printing neither the source nor a value its `Display` writes, and branch through `fmt::from_fn`
- Fields named `source` hold the typed foreign error, and text it adds beyond that (a position) is a field the message prints
- Refusals are variants naming their values, never `Other(format!(..))` or a `FromStr` with `Err = String`
- Call sites build a variant from values and spell no sentence, a wording per fieldless variant restating the name `rename_all` prints
- `rename_all = "lowercase"` joins a name's words (`NonFinite` as `nonfinite`), and a reason of more than one word takes kebab or snake case
- Field names a caller passes print through a fieldless enum under `rename_all = "snake_case"` (`Input::InitialStep` as `initial_step`)
- Context one scope's failures share is one variant holding its `#[source]` fault, never a prefix each variant restates
- Quantities, sizes, and durations print through their type: `uom` `into_format_args(<unit>, Abbreviation)`, `bytesize::ByteSize`, jiff `SignedDuration` under `{:#}`
- Crates serving tools or commands add the retry or next call as data of their protocol error
- Foreign errors describing the whole failure stay direct, or reach one variant through `#[error(transparent)]` and `#[from]`
- Boundaries `map_err` a foreign error into its variant beside an operation field and the path or key it omits, alike at every call
- Functions returning `Result<_, NEVec<E>>` take one `impl From<E> for NEVec<E>`, a single-error step converting through `?`
- Absence a foreign call reports as an error (`io::ErrorKind::NotFound`) becomes `Ok(None)` through one arm matching it alone
- Discards match each error variant they drop, and `Err(_)`, `is_err`, and `if let Ok` drop it unnamed

### [04.3]-[EXIT]

Entry points report each failure as `Report::new(&error)` (`error_reporter`), then convert it by value into `sysexits::ExitCode`:

```rust
impl From<LedgerError> for ExitCode {
    fn from(value: LedgerError) -> Self {
        match value {
            LedgerError::Refused { account: _, reason: Refusal::Missing } => Self::NoInput,
            LedgerError::Refused { account: _, reason: Refusal::Overflow | Refusal::Frozen } => Self::DataErr,
            LedgerError::Store { account: _, source } => source.into(),
        }
    }
}
```

Inner layers return failures unreported, and accumulated failures exit with their first item's status.

## [05]-[TRAITS]

Traits state a capability a consumer needs, and dispatch follows what varies and who knows the cases:

| [INDEX] | [SITUATION]                           | [FORM]                                 |
| :-----: | :------------------------------------ | :------------------------------------- |
|  [01]   | Closed set of cases the crate knows   | `enum` with a `match`                  |
|  [02]   | Implementation chosen at compile time | Generic parameter with a `where` bound |
|  [03]   | One unnamed return type               | `impl Trait` return                    |
|  [04]   | Implementation chosen at run time     | `Box<dyn Trait>`, `Arc<dyn Trait>`     |
|  [05]   | Async capability chosen at run time   | `dynosaur` dynamic type over the trait |

- Associated types hold an output the implementor decides, and generic parameters a choice the caller makes
- Async methods of a public trait or of a trait reaching spawned tasks declare `-> impl Future<Output = T> + Send`, and implementors write `async fn`
- Bounds name the traits whose members a generic body calls, and bounds repeated across signatures are one trait alias (`trait_alias`)
- Members restated across traits are one supertrait, and a trait with one implementor no caller fills is an inherent `impl` on a crate type
- Operations on a foreign type are functions generic over the bound they read, a trait existing for many implementations or a contract a caller fills
- Extension traits are a public `<Trait>Ext: <Trait>` with one blanket impl over `T: <Trait> + ?Sized` and provided methods continuing a caller's chain
- Text one site renders is a `fmt::from_fn` closure, never a struct holding only a `Display` impl

### [05.1]-[DERIVES]

Implementations a derive generates are derived, and a hand-written `impl` states only behavior no derive covers:

| [INDEX] | [CAPABILITY]                        | [DERIVE]                                                 |
| :-----: | :---------------------------------- | :------------------------------------------------------- |
|  [01]   | Error message and source chain      | `thiserror::Error`                                       |
|  [02]   | Text name and parse of an enum      | `derive_more` `Display`, `FromStr`                       |
|  [03]   | Variant list, count, discriminants  | `strum` `VariantArray`, `EnumCount`, `EnumDiscriminants` |
|  [04]   | Checked newtype and its error       | `nutype`                                                 |
|  [05]   | Conversions of an unchecked newtype | `derive_more` `From`, `Into`, `AsRef`, `Display`         |
|  [06]   | Traits adding no bound on a generic | `derive_where`                                           |
|  [07]   | Wire encode and decode              | `serde`, field conversions through `serde_with`          |

## [06]-[EFFECTS]

Async functions hold I/O:
- Libraries return futures and streams, and every task they spawn has an owner that joins it (`JoinQueue`, `JoinSet`, `TaskTracker`)
- Blocking or CPU-bound work runs on `spawn_blocking` or the `rayon` pool, and a runtime worker runs futures alone
- Acquisition and release pair in one scope through `Drop` or `mem::DropGuard`, and `DropGuard::dismiss(guard)` keeps the value once work commits
- Independent releases each run and join their failures through `Validated`, where a `?` between them skips every later release

Effects are independent when the domain says so, and a bound the caller passes sizes each fan-out:

| [INDEX] | [SCENARIO]                        | [FORM]                                            | [BEHAVIOR]                         |
| :-----: | :-------------------------------- | :------------------------------------------------ | :--------------------------------- |
|  [01]   | Fixed set of independent effects  | `tokio::join!`, then `Validated` `map2` to `map4` | Concurrent, joins every failure    |
|  [02]   | Collection of independent effects | `buffer_unordered(bound.get())`                   | At most `bound`, every failure     |
|  [03]   | Ordered results                   | `buffered(bound.get())`                           | At most `bound`, input order       |
|  [04]   | Dependent effects                 | `.await?` in sequence                             | Serial, stops at the first failure |

## [07]-[TESTS]

Tests bind through `?` in place of `unwrap`, assert variants through `assert_matches!`, name each `rstest` case, and keep golden output in `insta`:

```rust
#[rstest]
#[case::lowest(1)]
#[case::highest(100)]
fn quantity_within_its_range_is_kept(#[case] count: u32) -> Result<(), QuantityError> {
    assert_eq!(Quantity::try_new(count)?.into_inner(), count, "1 to 100 inclusive");
    Ok(())
}
```

Use `code:test` for test design.
