# [LANGUAGE_FEATURES]

Each crate root names the gates its code uses in `#![feature(..)]`, and each integration test or example file is a crate root of its own.

## [01]-[TRY_BLOCKS]

`try` blocks (`try_blocks`) wrap their final expression in `Some` or `Ok`, in place of an immediately called closure or a helper function:

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

Outcomes paired with their key are a `try` block inside the pair, and a line with no bin, no zone, or an overflowing shelf pairs with `None`:
- Plain `try` blocks convert no error, every `?` operand holding the block's own error type even under a typed `let`
- Blocks whose `?` operands differ in error type are `try bikeshed Result<Stop, PickError> { .. }` under `try_blocks_heterogeneous`
- Heterogeneous blocks convert each `?` through the error's `From` impls (`#[from]`), in place of a trailing `map_err`

## [02]-[DEFAULT_FIELD_VALUES]

`default_field_values` states each default on its field, in place of a hand-written `impl Default` or a `new` restating every field:

```rust
#[derive(Debug, Clone, PartialEq, Eq, Default, Deserialize)]
#[serde(default)]
pub struct Wrap {
    pub width: Option<NonZero<u16>> = None,
    pub indent: u8 = 4,
}

#[derive(Debug, Clone)]
pub struct Layout {
    pub title: String,
    pub wrap: Wrap = Wrap { .. },
    pub newline: &'static str = cfg_select! { windows => "\r\n", _ => "\n" },
}
```

`Layout { title, wrap: Wrap { width: Some(columns), .. }, .. }` sets a title and wraps at the `columns` a caller read from its terminal:
- Required fields (`title`) take no default and appear in every literal, and their record derives no `Default` filling them with an empty value
- `#[derive(Default)]` reads each declared default, and a container-level `#[serde(default)]` decodes a missing field to it
- Field-level `#[serde(default)]` decodes a missing field to its type's `Default` (`0` for `indent`), never the declared value
- Defaults are constant expressions, and text defaults through `&'static str` or `Cow::Borrowed` where `String::from` fails to compile
- Crates writing a `..` literal of a foreign record enable `default_field_values`, and `Default::default()` needs no gate
- Typed dependencies (a store, a layer) enter as a parameter beside the record, never a defaulted field

## [03]-[TRAIT_ALIASES]

`trait_alias` names a bound list once, in place of a memberless trait with a blanket `impl` over its supertraits:

```rust
pub trait Rule = Fn(&Order) -> Option<Discount> + Send + Sync + 'static;

pub fn first(rules: Vec<Box<dyn Rule>>) -> impl Rule { move |order: &Order| rules.iter().find_map(|rule| rule(order)) }
```

Aliases add no member and take no `impl`, and `dyn` takes an alias naming one trait beside auto traits.

## [04]-[ASSOCIATED_IMPL_TRAIT]

`impl_trait_in_assoc_type` names the `async` block or closure chain a method builds as its trait's associated type, in place of a boxed future:

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

`Answering` holds `index: Arc<Index>`, and an `IntoIterator::IntoIter` names its closure chain the same way:
- Auto traits of the hidden type leak, and a consumer bounding `S::Future: Send` accepts it with no bound declared
- Hidden types capture only the impl's parameters, and state the block reads moves in (`Arc::clone`), a borrow of `self` failing to compile

## [05]-[DEREF_PATTERNS]

`deref_patterns` matches through `Box`, `Rc`, `Arc`, `Vec`, `String`, and `Cow` as if each were absent, in place of a nested `match` per level:

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

`Expr` holds `Lit(i64)`, `Neg(Box<Self>)`, and `Add(Box<Self>, Box<Self>)`:
- By-value matches move out of each `Box` they pass through, a binding at a pointer position holds the pointer, and `*inner` takes its value
- Matches through `Vec`, `Rc`, or `Arc` move no non-`Copy` field out, and a match over a reference to one binds each field by reference
- String literal patterns match a `String`, a `Box<str>`, or a `Cow<str>`, and slice patterns match a `Vec`

## [06]-[CONST_PARAMETERS]

`min_adt_const_params` takes a type deriving `ConstParamTy` as a const parameter, in place of a marker type per kind:

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

`Quote<{ Side::Bid }>` names a bid, braces around the variant, and a bid passed where an ask belongs fails to compile:
- `ConstParamTy` derives beside `PartialEq` and `Eq` over integers, `bool`, `char`, arrays, tuples, and other `ConstParamTy` types
- Float and `Option` fields refuse the derive, and a struct under `min_adt_const_params` holds fields as visible as itself
- Private fields take `adt_const_params`, and reference fields (`&'static str`) add `unsized_const_params`
- Incomplete gates (`unsized_const_params`) fail `-D warnings` unless the root states `#![expect(incomplete_features, reason = "..")]`
- Kinds holding data of their own are one struct per kind in a type parameter's position
