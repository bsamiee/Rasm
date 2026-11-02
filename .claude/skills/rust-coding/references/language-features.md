# [LANGUAGE_FEATURES]

Crates enable each gate they use with `#![feature(..)]` at their root.

## [01]-[TRY_BLOCKS]

`try` blocks wrap the final expression, and every `?` inside shares one residual type the block takes. An outcome kept as data beside its subject is a `try` block inside the expression building the pair:

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

Blocks with `?` operands of different error types are `try bikeshed Result<Stop, PickError> { .. }` under `try_blocks_heterogeneous`, each `?` entering the crate error through its `#[from]` variants in place of a typed `let` binding or a trailing `map_err`, and a block with one error type across every `?` is a plain `try` that infers its type.

## [02]-[DEFAULT_FIELD_VALUES]

Records state each default on its field, a nested record defaults to its own `..` literal, and a platform value is one `cfg_select!`:

```rust
#[derive(Debug, Clone, Default)]
pub struct Wrap {
    pub width: u16 = 100,
    pub indent: u8 = 4,
}

#[derive(Debug, Clone)]
pub struct Layout {
    pub title: String,
    pub wrap: Wrap = Wrap { .. },
    pub newline: &'static str = cfg_select! { windows => "\r\n", _ => "\n" },
}
```

`Layout { title, wrap: Wrap { width: 72, .. }, .. }` sets a title and wrap width. Crates writing a `..` literal of a foreign record enable `default_field_values`.
- Required fields (`title`) appear in every literal, and their record derives no `Default` to fill them with an empty value
- `#[derive(Default)]` reads each declared default: `Wrap::default()` and a container-level `#[serde(default)]` decode agree with `Wrap { .. }`
- Defaults are constant expressions, text no caller builds at run time is `&'static str`, and text a caller can own defaults through `Cow::Borrowed`
- Typed dependencies (a store, a layer) enter as a parameter beside the record, never a defaulted field

## [03]-[TRAIT_ALIASES]

Traits implemented only by a blanket `impl` over their supertraits are aliases, valid in every bound position including `dyn` and return `impl`:

```rust
pub trait Rule = Fn(&Order) -> Option<Discount> + Send + Sync + 'static;

pub fn first(rules: Vec<Box<dyn Rule>>) -> impl Rule { move |order: &Order| rules.iter().find_map(|rule| rule(order)) }
```

Aliases add no member: a type implements the traits an alias names, and `dyn` takes an alias holding one trait beside auto traits.

## [04]-[ASSOCIATED_IMPL_TRAIT]

Foreign traits fix an associated type the implementation fills (`Service::Future`, `IntoIterator::IntoIter`). `impl Trait` in that position names the `async` block or closure chain the method builds:

```rust
impl Service<Query> for Answering {
    type Response = Answer;
    type Error = IndexError;
    type Future = impl Future<Output = Result<Answer, IndexError>>;

    fn poll_ready(&mut self, _: &mut Context<'_>) -> Poll<Result<(), IndexError>> { Poll::Ready(Ok(())) }

    fn call(&mut self, req: Query) -> Self::Future {
        let index = Arc::clone(&self.index);
        async move { index.lookup(req).await }
    }
}
```

- Impl methods define the hidden type and its auto traits leak: a consumer bounding `S::Future: Send` accepts the future with no bound declared
- Hidden types capture only the impl's parameters, and state the future reads moves into the block before `async move`

## [05]-[DEREF_PATTERNS]

Deref patterns match as if the pointer were absent, and a rewrite over an enum with `Box<Self>` operands states each nested shape in one arm:

```rust
#[must_use]
pub fn rewritten(expr: Expr) -> Expr {
    match expr {
        Expr::Neg(Expr::Neg(inner)) => *inner,
        Expr::Neg(Expr::Lit(value)) if let Some(negated) = value.checked_neg() => Expr::Lit(negated),
        unchanged @ (Expr::Lit(_) | Expr::Neg(Expr::Lit(_) | Expr::Add(_, _)) | Expr::Add(_, _)) => unchanged,
    }
}
```

By-value matches move out of each `Box` they pass through, a binding at a pointer position holds the pointer, and `*inner` takes its value.

## [06]-[CONST_PARAMETERS]

Data-free kinds the compiler keeps apart are one `ConstParamTy` enum a type takes as a const parameter, and one generic body matches the parameter:

```rust
#[derive(Debug, Clone, Copy, PartialEq, Eq, ConstParamTy)]
pub enum Side {
    Bid,
    Ask,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Quote<const SIDE: Side> {
    pub price: Decimal,
    pub size: u32,
}

#[must_use]
pub fn improves<const SIDE: Side>(quote: &Quote<SIDE>, best: &Quote<SIDE>) -> bool {
    match SIDE {
        Side::Bid => quote.price > best.price,
        Side::Ask => quote.price < best.price,
    }
}
```

- `adt_const_params` enables the parameter, and a variant in argument position sits in braces (`Quote<{ Side::Bid }>`)
- Each instantiation compiles one arm of `match SIDE`, and a bid passed where an ask belongs fails to compile
- Kinds holding data of their own are one struct per kind in a type parameter's position
