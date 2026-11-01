# [LANGUAGE_FEATURES]

Covers nightly features that replace a hand-written body or a declaration family with one form. A crate enables each gate its code writes (`try_blocks`, `try_blocks_heterogeneous`, `default_field_values`, `trait_alias`, `impl_trait_in_assoc_type`, `deref_patterns`) with `#![feature(..)]` at its root, a `..` literal of another crate's record included. Generator blocks are worked through in [ITERATORS](iterators.md), return-type notation in [TRAITS](traits.md), and type-changing struct update in [TYPES](types.md).

## [01]-[TRY_BLOCKS]

`try` blocks scope `?` to the block, wrap the final expression, and take their type from the `?` operands. An outcome kept as data beside its subject is a `try` block inside the expression building the pair, where a closure's `?` would leave the closure:

```rust
#[must_use]
pub fn stops(order: &Order, catalog: &Catalog) -> Vec<(Sku, Option<Stop>)> {
    order
        .lines
        .iter()
        .map(|line| {
            (line.sku, try {
                let bin = catalog.bins.get(&line.sku)?;
                let zone = catalog.zones.get(&bin.aisle)?;
                Stop { zone: zone.id, shelf: bin.shelf.checked_add(zone.offset)? }
            })
        })
        .collect()
}
```

Every `?` in a `try` block yields one residual type, here `Option`. `try bikeshed Result<Total, PriceError> { .. }` names the block's type and converts each `?` through `From`, for steps whose errors differ and enter the crate error through `#[from]`.

## [02]-[DEFAULT_FIELD_VALUES]

Values a composition varies are fields with declared defaults, and a caller states the fields it changes and ends the literal with `..`. A typed dependency (a store, a layer) enters as a parameter:

```rust
#[derive(Debug, Clone, Default)]
pub struct Retry {
    pub attempts: u8 = 3,
    pub backoff: Duration = Duration::from_millis(200),
}

#[derive(Debug, Clone)]
pub struct Served {
    pub name: String,
    pub timeout: Duration = Duration::from_secs(30),
    pub retry: Retry = Retry { .. },
}

#[must_use]
pub const fn patient(name: String) -> Served { Served { name, timeout: Duration::from_secs(120), retry: Retry { attempts: 5, .. }, .. } }
```

- Fields without a default (`name`) stay required in every literal, and a record holding one derives no `Default`
- `#[derive(Default)]` reads each declared default, so `Retry::default()` and a `#[serde(default)]` decode agree with `Retry { .. }` with no hand-written `impl`
- Defaults are constant expressions, a nested record defaults through its own `..` literal
- Defaults differing by target are one `cfg_select!` expression (`cfg_select! { windows => "\r\n", _ => "\n" }`)

## [03]-[TRAIT_ALIASES]

Trait aliases name a bound set that signatures repeat, and a trait whose one implementation is a blanket `impl` over its supertraits is an alias. The alias stands in every position its bounds take, `dyn` and return `impl` included:

```rust
pub trait Rule = Fn(&Order) -> Option<Discount> + Send + Sync + 'static;

pub fn first(rules: Vec<Box<dyn Rule>>) -> impl Rule { move |order: &Order| rules.iter().find_map(|rule| rule(order)) }
```

Aliases add no member: a type implements the traits an alias names, `dyn` takes an alias holding one trait beside auto traits, and bounds one signature uses stay in its `where` clause.

## [04]-[ASSOCIATED_IMPL_TRAIT]

Foreign traits fix an associated type the implementation fills (`Service::Future`, `IntoIterator::IntoIter`). `impl Trait` in that position takes the `async` block or closure chain the method builds, with no `Box::pin` and no adapter type spelled over `fn` pointers:

```rust
impl Service<Query> for Answering {
    type Response = Answer;
    type Error = IndexError;
    type Future = impl Future<Output = Result<Answer, IndexError>>;

    fn poll_ready(&mut self, _: &mut Context<'_>) -> Poll<Result<(), IndexError>> { Poll::Ready(Ok(())) }

    fn call(&mut self, query: Query) -> Self::Future {
        let index = Arc::clone(&self.index);
        async move { index.lookup(query).await }
    }
}
```

- Methods of the impl define the hidden type, and auto traits leak from it: a consumer bounding `S::Future: Send` accepts this future with no bound on the declaration
- Hidden types capture only the impl's parameters, and state the future reads moves into the block before `async move`

## [05]-[DEREF_PATTERNS]

Patterns match through `Box`, `Rc`, `Arc`, `Vec`, `String`, and `Cow` as if the pointer were absent, and a rewrite over an enum whose operands are `Box<Self>` states each nested shape in one arm:

```rust
#[must_use]
pub fn rewritten(expr: Expr) -> Expr {
    match expr {
        Expr::Neg(Expr::Neg(inner)) => *inner,
        Expr::Neg(Expr::Lit(value)) if let Some(negated) = value.checked_neg() => Expr::Lit(negated),
        Expr::Add(Expr::Lit(left), Expr::Lit(right)) if let Some(sum) = left.checked_add(right) => Expr::Lit(sum),
        Expr::Add(Expr::Lit(0), other) | Expr::Add(other, Expr::Lit(0)) => *other,
        unchanged => unchanged,
    }
}
```

- By-value matches move out of each `Box` they pass through, a binding at a pointer position holds the pointer and `*inner` takes its value
- `if let` guards bind a fallible step for the arm, and a `None` passes the value to the next arm
- `Vec` fields match slice patterns (`[only]`, `[first, .., last]`), `String` fields match string literals at a decode boundary
