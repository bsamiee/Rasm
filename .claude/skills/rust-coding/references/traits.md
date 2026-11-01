# [TRAITS]

Covers traits as capabilities, from associated types, proxies, and trait objects to async capabilities, bounded operations, and conversions.

## [01]-[CAPABILITIES]

Capability traits hold required methods, default methods derived from them, and associated types each implementor fixes. `auto_impl` derives the forwarding implementations for references and handles:

```rust
#[auto_impl(&, Arc, Box)]
pub trait Codec {
    type Value;
    type Error;

    fn decode(&self, bytes: &[u8]) -> Result<Self::Value, Self::Error>;
    fn encode(&self, value: &Self::Value) -> Bytes;

    #[auto_impl(keep_default_for(&, Arc, Box))]
    fn decode_all<I>(&self, frames: I) -> Validated<Vec<Self::Value>, (usize, Self::Error)>
    where
        I: IntoIterator<Item: AsRef<[u8]>>,
        Self: Sized,
    {
        frames.into_iter().enumerate().map(|(index, frame)| self.decode(frame.as_ref()).map_err(|error| (index, error))).collect()
    }
}
```

Consumers fix one associated type and bound another in one clause: `C: Codec<Value = Reading, Error: Into<StoreError>>`. `where Self: Sized` keeps the generic `decode_all` off the vtable, so `Box<dyn Codec<Value = Reading, Error = CodecError> + Send + Sync>` names every associated type and auto trait its holder needs. `keep_default_for` lets each proxy keep the default in place of forwarding it, the generated impls relax to `T: ?Sized`, and the boxed trait object passes to that consumer and reaches `decode_all` through the default. Implementations of a package trait write the required methods their role answers, and its provided methods stand.

## [02]-[ASYNC_CAPABILITIES]

`async fn` in a trait leaves each future's `Send` open: a spawning consumer bounds the one method it spawns, `S: Store<load(..): Send> + Send + Sync + 'static`, a single-threaded consumer states no bound, and an implementation whose other futures hold a non-`Send` value still serves both.

Implementations chosen at run time go through `dynosaur`, which generates a dynamic type that boxes each returned future. Boxed futures keep only the bounds their trait states, so a trait whose erased handles reach spawned tasks declares `Send` on its supertraits and on each method's future:

```rust
#[dynosaur::dynosaur(pub DynArchive = dyn(box) Archive)]
pub trait Archive: Send + Sync {
    fn put(&self, key: &str, bytes: Bytes) -> impl Future<Output = Result<(), ArchiveError>> + Send;
}

pub async fn replicate<A>(targets: &[A], key: &str, bytes: Bytes) -> Result<(), NEVec<ArchiveError>>
where A: Archive {
    join_all(targets.iter().map(|target| target.put(key, bytes.clone()))).await.into_iter().collect::<Validated<(), _>>().ok()
}
```

Implementors write `async fn put`. `DynArchive::new_box(archive)` and `DynArchive::new_arc(archive)` erase a concrete archive, and `dynosaur` implements `Archive` for `Box<T>`, `&T`, and `&mut T` with `T: ?Sized`. One `replicate` serves a slice of one concrete archive with static dispatch and a `Vec<Box<DynArchive<'_>>>` mixing archives chosen at run time, and every target's failure reaches the caller.

## [03]-[BOUNDED_OPERATIONS]

Operations over a foreign type, or over every type meeting a bound, are generic functions whose bound states what the body reads. `Borrow` in an associated type bound accepts owned and borrowed elements through one signature:

```rust
#[must_use]
pub fn peak<I>(readings: I) -> Option<Celsius>
where I: IntoIterator<Item: Borrow<Reading>> {
    readings.into_iter().map(|reading| reading.borrow().celsius).max()
}
```

`Reading` holds an ordered `celsius`, `peak(&readings)` reads a borrowed `Vec`, `peak(readings)` consumes it, and `peak(iter::once(reading))` reads one value. Foreign types sharing no trait stay apart, each consumer takes each by its own type, and no crate trait unifies them.

## [04]-[CONVERSIONS]

Fieldless enums derive their discriminant conversion and their text form, and each spelling rule is stated once:

```rust
#[derive(Debug, Clone, Copy, PartialEq, Eq, derive_more::TryFrom, strum::Display, strum::EnumString)]
#[try_from(repr)]
#[repr(u8)]
#[strum(serialize_all = "kebab-case")]
pub enum Channel {
    Primary = 1,
    Secondary = 2,
    Diagnostic = 9,
}
```

`Channel::try_from(byte)` returns `Result<Channel, TryFromReprError<u8>>`, and a boundary adds the offset the rejection lacks: `.map_err(|source| FrameError::Channel { offset, source })`. `serialize_all` drives `Display` and `FromStr` alike, so `Channel::Diagnostic.to_string()` and `"diagnostic".parse::<Channel>()` round-trip through one spelling. A generic host states the conversion it reads as a bound on the target, `for<'error> u8: From<&'error E>`, and each crate error implements `From<&LedgerError> for u8` once.
