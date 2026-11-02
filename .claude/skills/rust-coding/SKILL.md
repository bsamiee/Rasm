---
name: rust-coding
description: "Use when writing or reviewing Rust, covering functional design with algebraic types, results, traits, and async effects"
---

# [RUST_CODING]

Code treats each crate as part of the language:
- Errors and exit status: `thiserror`, `sysexits`
- Validated values and derives: `validated`, `nutype`, `derive_more`, `strum`
- Collections and iteration: `nonempty-collections`, `enum-map`, `itertools`
- Async: `tokio`, `tokio-util`, `futures`
- Wire and time: `serde`, `jiff`
- Tracing: `tracing`

[PRINCIPLES]:
- Facts are stated once where they belong, and everything else derives from that source
- Packages and hosts are used directly, with no layer that restates or wraps them, and a conversion between packages' types is no such layer
- Surface stays minimal inside and outside each module, and every type, export, and helper states something nothing else does
- Values and methods that vary are parameters, a caller choosing among methods of one result (a percentile or BCa interval) rather than the callee fixing the costlier one, and kinds, keys, and states are typed values rather than strings
- Names are domain or package vocabulary and never coined, a field or parameter holding a package value takes the member's name, or the word the package documents for it where a workspace lint rejects that name (t-digest `k` is its `compression`), a file its concept's

[REFERENCES]:
- [01]-[FUNCTIONS](references/functions.md): Functions as values and composition-root factories
- [02]-[TYPES](references/types.md): Algebraic type design and constrained values
- [03]-[RESULTS](references/results.md): `Option`, `Result`, and `Validated` pipelines over the crate error
- [04]-[TRAITS](references/traits.md): Capability traits under static and dynamic dispatch
- [05]-[ITERATORS](references/iterators.md): Iterator pipelines, generator blocks, and data parallelism
- [06]-[ASYNC](references/async.md): Async effects, tasks, streams, and shared state
- [07]-[BOUNDARIES](references/boundaries.md): Wire decode, encode, configuration, and the entry point
- [08]-[LANGUAGE_FEATURES](references/language-features.md): Nightly features that replace a hand-written form

## [01]-[FUNCTIONS]

Function signatures are contracts, parameter types state every value a function accepts and its return type every outcome a caller handles:
- Async callbacks take `impl AsyncFn(A) -> B`, a closure returning its callee's future (`|id| fetch(id)`) or an `async` closure holding more work supplies it
- Bounds sit in a `where` clause, argument-position `impl Trait` stays for a parameter no other position names
- Members and local `macro_rules!` differing only in a number, type, flag, or applied function are one function taking it as a parameter or generic, each caller passing the value from where it holds it
- Defaults sit on one options struct whose `default_field_values` state each, an optional input defaulting to `None` among them in place of a positional `Option` parameter unless `None` would equal the type's default, and a caller writes the fields it changes before `..`
- Values and types differing per platform or build configuration (`panic = "abort"`) are one `cfg_select!` around the part that differs (`SelectAll<cfg_select! { unix => A, windows => B }>`), an element, variant, or arm one platform alone holds takes `#[cfg]` on itself, and a share, lock, import, or nightly feature only one configuration reads sits under its `cfg` or `cfg_attr`, the plain owned value standing elsewhere
- Values following a rule compute from parameters or a variant's rank (`plan >= Plan::Team`), a table holds only values no rule derives
- Facts a dependency or the build states come from it: a protocol version from the package's constant, the crate name from `env!("CARGO_PKG_NAME")`, and a field default a dependency states only on a value with drop glue (`Stroke::new(width).join`) stays a literal, since a const default cannot drop it, and a value an input already holds (a mesh's cached `local_aabb`) is read from it, never recomputed
- Strings hold text a person reads or a wire's fixed token, a kind, key, or name derives from the type or member that owns it
- Choices a caller names are an enum parameter, `bool` holds a computed fact
- Data sits in structs and enums, behavior in functions, and an inherent method exists only where it keeps its type's invariant
- Helpers one function reads nest inside it as closures reading its generics, a nested `fn` holds a generic of its own, a `const fn`, a return borrowing from its parameter, or recursion no closure reaches, a closure calling its enclosing function recursing through it
- Helpers adding a fact to a package call serve only calls needing it

### [01.1]-[TOTAL_FUNCTIONS]

Functions are total when each declared input produces a declared output, and a panic is an outcome the signature omits. Make a partial function total by narrowing the input to a constructed type or to a bound excluding the values it fails on (`Neg` beside `Sub` refusing an unsigned measure), or by widening the output:
- `fn tier(u32) -> Tier` is incomplete when some counts are invalid, `fn tier(Quantity) -> Tier` holds for every constructible `Quantity`
- Empty collections are results, `Option<Vec<T>>` exists only where a consumer treats no collection and an empty one differently
- Clamps hiding an input the callee rejects (`.max(0.0)` before an asserting call) are a narrowed input type excluding only what the callee rejects, a value it gives a meaning (a `0.0` tolerance as the subject's own, a `0.0` epsilon as an exact test) staying reachable as `None`, and a clamp stays where the domain defines it
- Inputs a dependency asserts on (a cone's dimension, a raster past its size) are refused at construction or before the call, naming their subject
- Associated constants a dependency panics on (a zero period) take `const { assert!(..) }` in the generic body, failing at monomorphization

### [01.2]-[PURITY]

Mutation of a local that never escapes is no side effect, and a refusal leaves a `&mut` subject unchanged, every input tested before the first write. Expose each varying dependency as an argument, a clock reading as a `Timestamp` and a tolerance as a `SignedDuration`:

```rust
#[must_use]
pub fn expired(token: &Token, now: Timestamp, leeway: SignedDuration) -> bool { now.duration_since(token.expires) > leeway }
```

Pure functions take no `async`, no `&mut` parameter, and no capability trait, and a public pure function takes `#[must_use]` unless its return type is `#[must_use]` (`Result`). `Timestamp::now()`, `Zoned::now()`, and seeded generators appear at the composition root, which passes their values inward.

### [01.3]-[HIGHER_ORDER_FUNCTIONS]

Order parameters left to right: dependencies and configuration the composition root knows, then a behavior policy, then the runtime value. Dependencies are capability traits or closures: a validator `Fn(T) -> Result<T, E>`, a lookup `AsyncFn(OrderId) -> Result<Option<Order>, E>`.

Select the form by the call site:
- `match` over ranges, guards, or variants states a first-match decision, arm order stating priority
- Returned closures narrow a wide API to one operation, their factory doing once the work fixed arguments allow

## [02]-[EXPRESSIONS]

Blocks, `if`, `match`, and `loop` are expressions. A function body is one expression where the work allows, a chain already of the return type standing as that expression rather than inside `Ok(..?)`, and a returned value is one struct literal that sets each field at creation:

```rust
#[must_use]
pub fn profile(readings: &[Reading]) -> Profile {
    Profile { count: readings.len(), span: readings.iter().map(|reading| reading.at).minmax().into_option(), peak: readings.iter().max_by_key(|reading| reading.celsius).map(|reading| reading.sensor) }
}
```

- Iterator pipelines hold every traversal, with a named adapter or reduction (`minmax`, `counts_by`, `unique_by`, `coalesce`, `dedup`) in place of a `fold` or an in-place pass over a collected buffer (`Vec::dedup`, `sort`, `retain`), a kept order (`Acyclic::nodes_iter`) read in place of a sort, and `fold` holds a reduction no member names
- Adjacent pairs (`array_windows`, `map_windows`, `circular_tuple_windows` closing a ring) and a bounded range per pair (`series` under `take_while`) state a step reading its predecessor
- Values depending on more than one subject come from one `match` over their tuple or an arm guard, and a subject one arm alone reads sits in its guard
- Arms and closures of a combinator choosing a value (`map_or`, `map_or_else`, `unwrap_or_else`, `Some(x).filter(p).unwrap_or_else(f)`) nest no decision
- `let ... else` binds a refutable pattern and diverges otherwise, `if let` chains join conditions and bindings in one test
- Irrefutable `let`s inside a let chain name a value later conditions and the arm read
- Side effects on an `Option` run inside `if let` in place of `into_iter().for_each` or a `filter` ignoring its item
- `Option::is_some_and`, `Option::is_none_or`, and `map_or_default` test or project a present value in one call, `Result::is_ok_and` and `is_err_and` test a result without `ok()` discarding its error
- Builder steps over an `Option` or iterator fold its items into the builder with the method path as the step (`limit.map(NonZero::get).into_iter().fold(builder, Builder::limit)`), a fallible setter through `try_fold`
- `&mut` setters over `Option`s run as one `try_for_each` over `(value, method)` rows, a method taking `IntoIterator` receiving the `Option` whole
- `LazyCell` locals defer a value only some inputs read, and a value built lazily per element is a `OnceLock` in that element's tuple or record, never a parallel collection matched by index

### [02.1]-[PATTERN_MATCHING]

Pattern matching selects a result from a value's variant, shape, or contents, and keeps recognition, extraction, guards, and results together:

```rust
#[must_use]
pub fn notice(delivery: &Delivery, now: Timestamp, recent: SignedDuration) -> Notice {
    match delivery {
        Delivery::Shipped { carrier: _, tracking } => Notice::Track(tracking.clone()),
        Delivery::Delivered { at } if now.duration_since(*at) < recent => Notice::Arrived,
        Delivery::Returned { at: _, reason } => Notice::Refund(*reason),
        Delivery::Pending | Delivery::Delivered { at: _ } => Notice::Quiet,
    }
}
```

- Matches over a workspace enum list every variant, a new variant then fails each match that must handle it
- Wildcard arms stay for a foreign `#[non_exhaustive]` enum, range arms over a primitive list each remaining range
- Struct patterns name every field, and `field: _` marks a field the arm ignores
- `if let` guards bind from a second refutable pattern inside an arm, deref patterns match through `Box`, `Rc`, `Arc`, `Vec`, `String`, and `Cow`
- Slice patterns state each length and bind what it holds (`[] => .., [origin, ..] => ..`) in place of `first`, `split_last`, or a length test, and `<[T; 1]>::try_from(items)` takes an owned single item, `Err` returning the `Vec` whole
- Interpret an external value once near its source into an enum or an `Option`, then match the typed result

### [02.2]-[RECURSION]

Rust guarantees no tail-call elimination, the depth bound and the shape of the work select the form:

| [INDEX] | [APPROACH]                    | [USE_WHEN]                                          | [COST]                         |
| :-----: | :---------------------------- | :-------------------------------------------------- | :----------------------------- |
|  [01]   | Direct recursion              | Depth bounded by a small known value or tree height | Each level holds a stack frame |
|  [02]   | `iter::successors`            | Unbounded depth, intermediate states are meaningful | Holds one state per step       |
|  [03]   | `try_fold` over a state range | Unbounded depth, only the final state or failure    | Intermediate states are lost   |
|  [04]   | Arena with typed keys         | Shared or cyclic successors                         | Graph built before the walk    |

## [03]-[TYPES]

Types make every constructible value valid, and a consumer receives a value that needs no further check. Counting a type's values exposes unneeded states: a struct multiplies its fields' counts, an enum adds its variants' counts, `Option<T>` adds one. Std, package, and host types (`NonZero`, `Range`, `Timestamp`, `NEVec`, `EnumMap`) state most concepts. Tuples and closures hold intermediate values no consumer names. Aliases name one generic instantiation a consumer or a module's signatures repeat (`TypedUuid<OrderKind>`), a package alias (`UnitVector3<f64>`) standing for each instantiation it names, and a removed alias leaves its target written at each use, never a package alias of another instantiation in its place.

### [03.1]-[ALGEBRAIC_TYPES]

Model each state a consumer inspects as one variant holding exactly the data valid in that state:

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

- Kinds derive from the enum: `discriminant()` returns a fieldless `Stage` ranked in declaration order, `EnumMap<Stage, V>` one value per stage
- Data every variant shares sits in a struct beside a field holding the enum, variants sharing fields are one variant over an enum of the rest
- Variants differing only by a tag are one struct keyed by a fieldless enum, and fields named per key (`early_start`, `late_start`) one `EnumMap`
- Flags, `Option` fields, and `Option` tuples one state reads become one variant per valid state, a setting one variant reads sits inside it, and `Option` fields read only together (`address.zip(port)`) one `Option` of a record
- Shapes a package sum states take it (`EitherOrBoth` for one side or both, `Either` for a plain choice), `Result` holding failure alone
- Enums fit a growing set of operations, a trait fits a growing set of cases
- Absence and failure stay separate when a consumer responds differently to each, and an input owed once per element (a start per chain) arrives whole, a shortfall refused rather than filled by a callee draw the caller cannot see
- Variants standing for absence (`Closed`, `Unset`) are an `Option` around the enum, a wire token spelling absence (`none`) stays a variant, and a foreign sentinel for absence (an id 0, an index -1, an empty name) becomes `None` where it enters (`u32::try_from(id).ok().and_then(NonZero::new)`)

### [03.2]-[CONSTRAINED_VALUES]

Rules the domain owns become types whose constructor enforces them, and the inner value is reachable only through that constructor:
- `nutype` declares a validated newtype with its sanitizers, validators, and a generated `<Name>Error` enum, or the crate error through `validate(with = <fn>, error = <Error>)`
- `derive_more` derives `From`, `Into`, `AsRef`, and `Display` on a newtype with no invariant, `#[as_ref]` exposes a guarded record's field read-only
- `NonZero<T>` holds a nonzero integer, a constant of a type with a `const fn` constructor written in its const context (a field default included) as `NonZero::new(<value>).unwrap()`, or `try_new(<value>).ok().unwrap()` over a `Result` constructor (nutype `const_fn`) under `const_result_trait_fn` and `const_trait_impl`, an invalid value failing the build, with `const { .. }` around it in a runtime body, `bounded_integer` a range of integers, `NotNan<f64>` a stored ordered float key, and `f64::total_cmp` orders a key nothing stores
- `TypedUuid<Kind>` names the entity an identifier refers to, each entity with its own identifier type
- `Decimal` holds money and exact values, `Timestamp`, `Zoned`, `Span`, and `SignedDuration` hold time
- `uom` quantities hold physical values with dimensions the compiler checks
- Rules across fields take `nutype` over a standard type of that shape, else over a declared record: an interval is a `core::range` value (`RangeInclusive<f64>`, `Copy` with public ends), an integer window whose type's maximum is a valid member (a Sobol index through `u16::MAX`) a nutype over `RangeInclusive<u16>` refusing `start > last`, never a half-open range or a count that tops out one short or admits an empty window, and a `core::ops` range stays where a wire derive reads it (serde implements `ops::Range<Timestamp>` alone)
- Public fields hold values no rule constrains
- Constructors test each quantity the invariant states (an unsigned volume tests extent, not orientation), never a diagnostic agreeing on some inputs
- Invariants every consumer reads (a finite path, an invertible affine) are a constructed type each operation takes and returns, never a check repeated at each entry or a guard emptying the result
- Tokens a value issues carry an invariant `'id` brand its scoped constructor binds, so another issuer's token fails to compile

### [03.3]-[OWNERSHIP]

Ownership states who decides a value's lifetime:
- Functions storing an argument take it by value, the caller decides whether to clone, and functions only reading one borrow it, a collection through `IntoIterator<Item: Borrow<T>>` (`IntoIter: Clone` when walked more than once), so a borrowed collection a package hands out passes without a copy, and a `&mut self` callback a hot loop calls (a sampler's density) owns its scratch buffers, sized at construction and cleared per call, never allocating per call
- Values shared across tasks for reading are `Arc<str>` and `Arc<[T]>`, or the shared handle a dependency states for the concept (linebender_resource_handle `FontData` over font bytes and collection index)
- Lifetimes appear where an output borrows from an input, and a named lifetime names the borrowed source
- Steps only some inputs take return `Cow<'_, T>`, the untouched path borrowing its input
- Interior mutability belongs to boundary types that own shared state, domain types hold none

### [03.4]-[IMMUTABILITY]

Values are fixed once built, and a state transition consumes the old state and returns the next:

```rust
impl Account {
    pub fn post(self, amount: Decimal) -> Result<Self, LedgerError> {
        self.balance.checked_add(amount).ok_or(LedgerError::Refused { account: self.id, reason: Refusal::Overflow }).map(|balance| Self { balance, ..self })
    }
}
```

- `Account` holds an `id` and a `balance`, and one signed `amount` states a credit and a debit alike
- Struct update copies unchanged fields from a replaced state (`..self`, a changed type parameter included), a foreign default, or a foreign value (`..event`)
- `&mut self` methods stay on a type whose purpose is an owned buffer (a builder, a parser cursor)

Select collections by their most frequent operation:

| [INDEX] | [OPERATION]                          | [COLLECTION]                                   |
| :-----: | :----------------------------------- | :--------------------------------------------- |
|  [01]   | Ordered reads of an owned sequence   | `Vec<T>`, `Box<[T]>` once frozen               |
|  [02]   | Snapshots shared or versioned        | `imbl::Vector`, `imbl::OrdMap`                 |
|  [03]   | Keyed lookups with output order read | `IndexMap`, `IndexSet`                         |
|  [04]   | Keyed lookups in key order           | `BTreeMap`, `BTreeSet`                         |
|  [05]   | Keyed lookups with no order read     | `HashMap`, `HashSet`, `Vec` over a dense index |
|  [06]   | One value per enum variant           | `EnumMap`                                      |
|  [07]   | At least one element                 | `NEVec`, `NESet`                               |
|  [08]   | Graph nodes referring to each other  | `SlotMap` with typed keys                      |

### [03.5]-[TYPESTATE]

States with different operations are different types, each holding only the operations valid in its state. Typestate fits a lifecycle a single owner drives through fixed steps, an enum fits a state a consumer inspects or stores.

## [04]-[RESULTS]

Every fallible function returns its outcome as a value, and one type serves one concern:

| [INDEX] | [TYPE]                             | [CONCERN]                                          |
| :-----: | :--------------------------------- | :------------------------------------------------- |
|  [01]   | `Option<T>`                        | Absence that holds no reason                       |
|  [02]   | `Result<T, E>`                     | Expected failure, `?` stops at the first           |
|  [03]   | `Validated<T, E>`                  | Independent checks, every failure in an `NEVec<E>` |
|  [04]   | `ControlFlow<B, C>`                | Early exit from a fold with a value, not a failure |
|  [05]   | `impl Stream<Item = Result<T, E>>` | Values over time, each failure one item            |

Input boundaries select the result type, functions preserve it, and one point converts by the target's method (`ok_or`, `ok`, `transpose`, `into`). An operational failure stays an error to its consumer, `None` holds absence alone, and a result its inputs cannot measure (a bias correction from resamples all on one side) is a refusal tested before the step that turns it infinite, never an `Ok` holding NaN or an infinity.

### [04.1]-[COMPOSITION]

Give each step a function and select the operator by the step's signature and by whether the steps depend on each other:

| [INDEX] | [SIGNATURE]                    | [OPERATOR]                             | [BEHAVIOR]                      |
| :-----: | :----------------------------- | :------------------------------------- | :------------------------------ |
|  [01]   | `T -> U`                       | `map`                                  | Transforms a present value      |
|  [02]   | `T -> Result<U, E>`            | `?` in sequence, `and_then` in a chain | Continues with a dependent step |
|  [03]   | `Option<T>` to `Result<T, E>`  | `ok_or`, `bool::ok_or`                 | Names the error absence means   |
|  [04]   | Independent `Result`s          | `Validated::from`, `map2` to `map4`    | Joins every failure             |
|  [05]   | Items with `T -> Result<U, E>` | `try_collect::<Vec<U>>()`              | Stops at the first failure      |
|  [06]   | Items with `T -> Result<U, E>` | `collect::<Validated<Vec<U>, E>>()`    | Joins every failure             |
|  [07]   | Items, failures kept apart     | `partition_result`                     | Splits successes from failures  |
|  [08]   | `Option<Result<T, E>>`         | `transpose`                            | Swaps nesting for `?`           |
|  [09]   | `Option<(T, U)>`               | `unzip`                                | Splits pair, one `Option` each  |

Each dependent chain binds through `?`, and independent chains join through `Validated`:

```rust
#[must_use]
pub fn transfer(accounts: &HashMap<AccountId, Account, impl BuildHasher>, from: AccountId, to: AccountId, amount: Decimal) -> Validated<[Account; 2], LedgerError> {
    let posted = |account: AccountId, change: Decimal| accounts.get(&account).cloned().ok_or(LedgerError::Refused { account, reason: Refusal::Missing })?.post(change);
    Validated::from(posted(from, -amount)).map2(Validated::from(posted(to, amount)), |source, target| [source, target])
}
```

- Failures of independent chains all reach the caller: a missing source beside an overflowing target, each refused element of one batch
- Files importing `Itertools` write `collect::<Result<Vec<U>, E>>()` or `collect::<Option<_>>()`, a `try_collect` call there resolving to `Itertools::try_collect`
- `Validated` joins constructions and operations that can fail, never a pass rechecking a value its type already guarantees
- Foreign trait methods fixing one error type (`from_arg_matches_mut` on a record generic over `Args`) bind independent steps through `?` in one struct literal
- `<step>?;` in a function or `try` block and `try { <statements>; <value> }` elsewhere replace an `.and_then(|()| ..)` step, a helper function exists only for a name
- Errors from one traversed element name the element, its index or its key, a check over a matrix naming its column and row (`Start { chain, dimension }`), and each check runs where the input enters, before the step that would turn a bad value non-finite
- Nested contexts stay nested, a consumer matches `Result<Option<T>, E>` once
- Fallible callbacks a foreign API takes receive `Ok::<_, !>(..)`, its `Result<T, !>` read through `into_ok()` in the chain

### [04.2]-[ERRORS]

Expected failures are data in return types, and a panic is a defect: a violated invariant the types cannot express. Each crate declares one error enum named for it, with one variant per source (a domain refusal, each foreign error type) holding its typed subject. Reasons and operations sharing a source are an enum field on its variant, fieldless but for an operation's own subject (`Export(Utf8PathBuf)` under `#[display("export into {_0}")]`), its `Display` derived from variant names, and a variant whose words differ from its name (`Shutdown` as `shut down`) takes its own `#[display(..)]`:

```rust
#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Refusal {
    Missing,
    Overflow,
    Frozen,
}

#[derive(Debug, thiserror::Error)]
pub enum LedgerError {
    #[error("{reason}: {account}")]
    Refused { account: AccountId, reason: Refusal },
    #[error("unreadable: {account}")]
    Store { account: AccountId, source: io::Error },
}
```

- Messages render from typed fields through `#[error]` and print neither source nor a value its `Display` writes (`source.path()`), `Report::new(&error)` renders the chain
- Messages branching on a field or listing a variant's `NEVec` (one `Report::new` per item) render through `#[error("{}", fmt::from_fn(..))]`, a list of plain values through `Itertools::format`
- Fields named `source` hold the typed foreign error a variant wraps, and a serializer's refusal is a variant of its `type Error` over `custom` text
- Foreign errors mapping to one variant with no subject take `#[error(transparent)]` and `#[from]`, and one with a single value (`SetLoggerError`) maps to a unit variant through `map_err(|SetLoggerError { .. }| ..)`
- Boundaries `map_err` a foreign error into a variant holding the operation or subject it omits (the path, the key) beside the source, an error omitting none reaching its variant through `From` and `?`, the crate's own error a foreign call hands back from a crate callback mapping alike, and calls differing in operation alone share one curried mapper (`|operation| move |source| ..`)
- Foreign signatures fixing `anyhow::Error` downcast to a concrete source, a report with none records as a `tracing` event beside a variant naming the operation, and every sibling call of that operation passes the same mapping, a bare `?` on one dropping the subject the others carry
- `fs_err` file calls name operation and path in `io::Error`, other `io::Error`s (runtime start, socket bind) take a variant naming the operation
- Layers generic over a consumer error `X` hold it in one `#[error(transparent)]` variant through `map_err`, a `#[from]` on `X` overlaps every other `From`
- Functions generic over a consumer error only to call `E::from` return the crate error, and the caller converts through `?`
- Callers convert an `NEVec` of crate errors through `into_nonempty_iter().map(E::from).collect()`
- Discards match each error variant they drop (`Err(LatitudeError::PredicateViolated)`) or record the failure as an event, an error type reaching a fallback, `None`, or an idle arm through `Err(_)` or `Err(<Type> { .. })` drops every variant unnamed, `Err(<Type> { .. })` stays for a returned value that is no error (`Err(Vec { .. })`), and an `if let` on a success variant (`Ok`, `Validated::Good`) discards the failure it skips alike
- Consumers branch on variants, message text is for people

### [04.3]-[EXIT]

Entry points run the composed effect once and translate its result into an exit status. Crate errors convert by value into `sysexits::ExitCode`, as sysexits converts `io::Error`, once the entry point has reported them by reference. Generic entry points bound `E: Into<ExitCode>` with the error type as the bound's subject, so a nested entry point infers `E` from its closure, and an associated error type states the bound in its declaration. Layer errors convert through `impl<X> From<LayerError<X>> for ExitCode where X: Into<Self>`. Accumulated failures exit with the first one's status, the variant holding an `NEVec` of crate errors converting its first through `Self::from` in place of a status of its own:

```rust
impl From<LedgerError> for ExitCode {
    fn from(value: LedgerError) -> Self {
        match value {
            LedgerError::Refused { account: _, reason: Refusal::Missing } => Self::NoInput,
            LedgerError::Refused { account: _, reason: Refusal::Overflow } => Self::DataErr,
            LedgerError::Refused { account: _, reason: Refusal::Frozen } => Self::NoPerm,
            LedgerError::Store { account: _, source } => source.into(),
        }
    }
}
```

One `match` holds every variant, and a source classifying itself converts inside its arm (`source.io_error_kind().map_or(Self::DataErr, Self::from)`), unless another step reads the same class (a broken pipe ending output), when the boundary maps the source into the variant both read. Entry points log each failure that reaches their translation as `Report::new(&error)`, inner layers return their result unlogged.

## [05]-[TRAITS]

Traits state a capability a consumer needs. Associated types state an output the implementor decides, generic parameters a caller's choice, and default methods derive from required ones. Select dispatch by what varies and who knows the cases:

| [INDEX] | [SITUATION]                                                   | [FORM]                                 |
| :-----: | :------------------------------------------------------------ | :------------------------------------- |
|  [01]   | Closed set of cases the crate knows                           | `enum` with a `match`                  |
|  [02]   | One implementation per use, chosen at compile time            | Generic parameter with a `where` bound |
|  [03]   | Function returning one unnamed type                           | `impl Trait` return                    |
|  [04]   | Implementations chosen at run time or mixed in one collection | `Box<dyn Trait>`, `Arc<dyn Trait>`     |
|  [05]   | Async capability chosen at run time                           | `dynosaur` dynamic type over the trait |

- Public traits state `Send` on each async method as `-> impl Future<Output = T> + Send`, implementors write `async fn` for a body that awaits and return `ready(..)` under the trait's signature for one that awaits nothing
- `#[auto_impl(&, Arc)]` implements a capability for its references and shared handles from one implementation
- Associated types set to `impl Trait` name an implementor's iterator or future, a boxed `dyn Future` stays for a `dyn` consumer
- Bounds repeated across signatures or crates are one trait alias over the traits each body reads or its totality needs, members restated across traits (an error type, a default `serve`) one supertrait each extends, a crate-private trait with one implementor is that type's inherent `impl`
- Foreign trait implementations write required methods and each default their type or a forwarded inner type answers differently, other defaults stand
- Operations over a foreign type or every type meeting a bound are generic functions, a trait exists only for a capability with many implementations
- Text one site renders is a `fmt::from_fn` closure in place of a struct holding only a `Display` impl
- Parameters take the target type, callers convert with `.into()` at the call site

Bounds name the traits with members a generic body calls (clap `CommandFactory + FromArgMatches` in place of `Parser`, a subtrait adding none), and `Borrow` in an associated type bound accepts owned and borrowed elements through one signature:

```rust
#[must_use]
pub fn peak<I>(readings: I) -> Option<Celsius>
where I: IntoIterator<Item: Borrow<Reading>> {
    readings.into_iter().map(|reading| reading.borrow().celsius).max()
}
```

`peak(&readings)` reads a borrowed `Vec`, `peak(readings)` consumes it, and `peak(iter::once(reading))` reads one value.

### [05.1]-[DERIVES]

Implementations a derive generates are derived, and a hand-written `impl` states only behavior no derive covers:

| [INDEX] | [CAPABILITY]                            | [DERIVE]                                                                  |
| :-----: | :-------------------------------------- | :------------------------------------------------------------------------ |
|  [01]   | Error message and source chain          | `thiserror::Error`                                                        |
|  [02]   | Text name and parse of an enum          | `derive_more` `Display`, `FromStr`, `strum` `IntoStaticStr`, `EnumString` |
|  [03]   | Variant list, count, and discriminants  | `strum` `VariantArray`, `EnumCount`, `EnumDiscriminants`, `FromRepr`      |
|  [04]   | Variant tests                           | `derive_more` `IsVariant`                                                 |
|  [05]   | One value per variant or combination    | `enum_map::Enum`                                                          |
|  [06]   | Checked newtype and its error           | `nutype`                                                                  |
|  [07]   | Conversions of an unchecked newtype     | `derive_more` `From`, `Into`, `AsRef`                                     |
|  [08]   | Traits adding no bound on a generic     | `derive_where`, `skip` or `skip_inner` for fields lacking the trait       |
|  [09]   | `Debug` over a foreign field lacking it | `derive_more` `Debug`, `#[debug(skip)]`                                   |
|  [10]   | Wire encode and decode                  | `serde`, field conversions through `serde_with`                           |
|  [11]   | Field mapping between domain and wire   | `o2o`                                                                     |

## [06]-[EFFECTS]

Pure functions hold the domain, async functions hold I/O, and the binary's `main` starts the runtime and runs the composed effect once:
- Libraries return futures and streams, start no runtime, and every task they spawn has an owner that joins it
- Tasks spawn inside a scope that awaits them: `JoinQueue` in spawn order, `JoinSet` in completion order, `TaskTracker`, or `AbortOnDropHandle`
- `CancellationToken` stops a scope, `run_until_cancelled` races a future against it, and `timeout` bounds one future
- `tokio::select!` races futures, each branch producing its value, `biased;` ranking branches ready together, `OptionFuture` disabling an absent future
- Blocking or CPU-bound work runs on `spawn_blocking` or the `rayon` thread pool, a runtime worker runs futures alone
- Futures a blocking thread drives through `executor::block_on` race the cancellation signal as awaited futures do
- Acquisition and release pair in one scope through `Drop` or `mem::DropGuard`, and `DropGuard::dismiss` returns the value once the work commits, the scope ending before a later phase reads the release (a boxed stream dropping its spans' progress bars before a summary draws)
- Independent releases each run and join their failures through `Validated`, a `?` between them skips every later release
- Blocking releases run concurrently under `thread::scope` through `Builder::spawn_scoped`, a refused spawn joining `Validated` as one more failure
- Operations at a boundary open a span with `#[instrument]`, and events record typed fields

Dependency structure and concurrency bound select the combinator:

| [INDEX] | [SCENARIO]                        | [FORM]                                          | [BEHAVIOR]                                  |
| :-----: | :-------------------------------- | :---------------------------------------------- | :------------------------------------------ |
|  [01]   | Fixed set of independent effects  | `join!`, then `Validated` `map2` to `map4`      | Concurrent, joins every failure             |
|  [02]   | Collection of independent effects | `buffer_unordered(n)`, collected to `Validated` | At most `n` at once, joins every failure    |
|  [03]   | Ordered results                   | `buffered(n)`                                   | At most `n` at once, results in input order |
|  [04]   | Dependent effects                 | `.await?` in sequence                           | Serial, stops at the first failure          |

Independent I/O over a collection runs concurrently on the current task through a stream, under a bound the caller passes:

```rust
pub async fn fetch_all<S>(source: &S, limit: NonZero<usize>, ids: impl IntoIterator<Item = ItemId>) -> Result<Vec<Item>, NEVec<SourceError>>
where S: Source {
    stream::iter(ids).map(|id| source.fetch(id)).buffered(limit.get()).collect::<Vec<_>>().await.into_iter().collect::<Validated<Vec<_>, _>>().ok()
}
```

`buffered(0)` polls no fetch, and the bound takes `NonZero`. Every fetch runs to its outcome, and `Validated` keeps each failure for the caller. Concurrency on one task needs no `Send`, no `Arc`, and no `'static` capture, and fits I/O that waits more than it computes.

Shared state takes the construct its access pattern names, and values changing together sit in one construct owning value and notification:

| [INDEX] | [ACCESS]                                      | [CONSTRUCT]                                     |
| :-----: | :-------------------------------------------- | :---------------------------------------------- |
|  [01]   | Read-mostly snapshot replaced whole           | `ArcSwap<T>`                                    |
|  [02]   | Latest value observed by many tasks           | `watch` channel edited through `send_modify`    |
|  [03]   | Every event delivered to many tasks           | `broadcast` channel, a `Lagged` item resyncs    |
|  [04]   | Commands serialized against one owner         | Task owning the state, `mpsc` in, `oneshot` out |
|  [05]   | Concurrent keyed reads and writes             | `papaya::HashMap`                               |
|  [06]   | Keyed results with expiry and coalesced loads | `moka::future::Cache`                           |
|  [07]   | Reusable connections and handles              | `deadpool` pool                                 |

Libraries return their own error, and the application composes each failure policy around that result:
- Fallback runs a lower-priority effect after the preferred one fails
- Retry reruns a transient failure on a schedule
- Recovery maps a final failure to a value at the entry point

## [07]-[NUMERICS]

Numeric facts come from the type or package that states them:
- Integer and `Decimal` arithmetic is `checked_*` returning `Option`, `strict_*` where overflow is a defect no caller input reaches, `saturating_*` where clamping rules, and a dependency bound stated as `< 2^n` is the full `u<n>` range of indices, never a `u<n>` count, which reaches one short
- Division and remainder take a `NonZero` divisor, `rem_euclid` states the sign of a remainder, `is_multiple_of` tests divisibility
- Tolerances and bounds are values a caller passes and floats compare within, a bound the callee derives (a makespan) reachable as `None`
- Absent bounds stay `None` at each read, never a stand-in (`f64::INFINITY` for no limit, `pending()` for no deadline)
- Encodings name their byte order (`to_le_bytes`, `from_be_bytes`)
- Quantities enter and leave through `uom` at a signature, and the kernel between them runs on `f64`
- Generic arithmetic bounds `CheckedAdd` and `CheckedSub` and refuses overflow, an `Add::add` call hiding the unchecked operator from the lint
- Operators state float, dual, and matrix arithmetic, `arithmetic-side-effects-allowed` rows list operand types in place of `.sum()` over a pair
- Durations convert units through their constructors (`SignedDuration::from_mins`)
- Counts enter float arithmetic through `nalgebra::convert::<usize, f64>`, and `cmp::minmax_by(a, b, f64::total_cmp)` orders a float pair
- Floats narrow through `cast::<f32>()` and saturate to integers by `to_int_saturating`, `convert_unchecked` and integer `convert` casting by `as`
- Formulas a member names take it (`Point::lerp`, `f64::midpoint`, `Vector2::perp`, `Unit::try_new_and_get`), `mul_add` fusing a step none names

Roots, quadrature, factorizations, derivatives, quantiles, and optimization are package members:

| [INDEX] | [PROBLEM]         | [MEMBER]                                 |
| :-----: | :---------------- | :--------------------------------------- |
|  [01]   | Scalar root       | `argmin` `BrentRoot`                     |
|  [02]   | Quadrature        | `GaussLegendre::integrate`               |
|  [03]   | Factorization     | `faer` `svd`, `qr`, `partial_piv_lu`     |
|  [04]   | Derivative        | `num_dual::first_derivative`, `gradient` |
|  [05]   | Quantile          | `statrs` `OrderStatistics::quantile`     |
|  [06]   | Linear program    | `good_lp` over the `highs` solver        |
|  [07]   | Conic program     | `clarabel` `DefaultSolver`               |
|  [08]   | Nonlinear program | `nlopt` `Nlopt::optimize`                |
