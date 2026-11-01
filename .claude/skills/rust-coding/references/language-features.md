# [LANGUAGE_FEATURES]

Covers nightly features that replace a hand-written body or a declaration family with one form. Crates enable each gate their code writes with `#![feature(..)]` at their root.

## [01]-[TRY_BLOCKS]

`try` blocks wrap the final expression and take their type from the `?` operands. An outcome kept as data beside its subject is a `try` block inside the expression building the pair:

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

Every `?` in a `try` block shares one residual type, `Option` in `stops`. `try bikeshed Result<Stop, ShopError> { .. }` names the block's type and converts each `?` through `From`, and steps failing with different errors enter the crate error through its `#[from]` variants.

## [02]-[DEFAULT_FIELD_VALUES]

Typed dependencies (a store, a layer) enter as a parameter beside the record of defaulted fields:

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

`Layout { title, wrap: Wrap { width: 72, .. }, .. }` sets a title and a wrap width, and a crate writing a `..` literal of another crate's record enables `default_field_values`.
- Required fields (`title`) appear in every literal, and their record derives no `Default` to fill them with an empty value
- `#[derive(Default)]` reads each declared default: `Wrap::default()` and a container-level `#[serde(default)]` decode agree with `Wrap { .. }`
- Defaults are constant expressions, owned text defaults through `Cow::Borrowed` in a `Cow<'static, str>` field

## [03]-[TRAIT_ALIASES]

Traits with one blanket `impl` over their supertraits as their only implementation are aliases, and an alias stands in every position its bounds take, `dyn` and return `impl` included:

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

- Methods of the impl define the hidden type, and auto traits leak from it: a consumer bounding `S::Future: Send` accepts this future with no bound on the declaration
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

- By-value matches move out of each `Box` they pass through, a binding at a pointer position holds the pointer and `*inner` takes its value
- Failed `if let` guards pass the value to the next arm, and the last arm names each variant it keeps, so a new variant fails to compile until an arm takes it
- `Vec` fields match slice patterns (`[only]`, `[first, .., last]`), `String` fields match string literals at a decode boundary
