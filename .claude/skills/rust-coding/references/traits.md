# [TRAITS]

Covers traits as capabilities and conversions.

## [01]-[CAPABILITIES]

Capability traits with a generic default method keep it off the vtable:

```rust
#[auto_impl(&, Arc, Box)]
pub trait Codec {
    type Value;
    type Error;

    fn decode(&self, bytes: &[u8]) -> Result<Self::Value, Self::Error>;
    fn encode(&self, value: &Self::Value) -> Bytes;

    #[auto_impl(keep_default_for(&, Arc, Box))]
    fn decode_all<I>(&self, frames: I) -> Validated<Vec<Self::Value>, Self::Error>
    where
        I: IntoIterator<Item: AsRef<[u8]>>,
        Self: Sized,
    {
        frames.into_iter().map(|frame| self.decode(frame.as_ref())).collect()
    }
}
```

Consumers fix one associated type and bound another in one clause: `C: Codec<Value = Reading, Error: Into<StoreError>>`. `where Self: Sized` keeps the generic `decode_all` off the vtable, and `Box<dyn Codec<Value = Reading, Error = CodecError> + Send + Sync>` names every associated type and auto trait its holder needs. `keep_default_for` lets each proxy keep the default in place of forwarding it, the generated impls relax to `T: ?Sized`, and the boxed trait object passes to that consumer and reaches `decode_all` through the default.

## [02]-[ASYNC_CAPABILITIES]

Implementations holding a non-`Send` value in an unspawned method's future serve spawning and single-threaded consumers alike, each consumer bounding only the futures it spawns.

`dynosaur` boxes each future its dynamic type returns with only the bounds its trait states. Traits with erased handles reaching spawned tasks declare `Send` on their supertraits and on each method's future:

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

Implementors write `async fn put`, and `DynArchive::new_box` or `DynArchive::new_arc` erases a concrete archive. `dynosaur` implements `Archive` for `Box<T>`, `&T`, and `&mut T` with `T: ?Sized`, and one `replicate` serves a slice of one concrete archive with static dispatch and a `Vec<Box<DynArchive<'_>>>` mixing archives chosen at run time. Every target's failure reaches the caller. `dynosaur` rejects `where Self: Sized`, and a generic default method belongs to an `auto_impl` trait.

## [03]-[CONVERSIONS]

Fieldless enums with a `#[repr]` discriminant derive it and their text from `strum`, and `serialize_all` states the spelling once:

```rust
#[derive(Debug, Clone, Copy, PartialEq, Eq, strum::FromRepr, strum::Display, strum::EnumString)]
#[repr(u8)]
#[strum(serialize_all = "kebab-case")]
pub enum Channel {
    Primary = 1,
    Secondary = 2,
    Diagnostic = 9,
}
```

`Channel::from_repr(byte)` is a `const fn` returning `Option<Channel>`, and a boundary names the rejection with its subject: `.ok_or(FrameError::Channel { offset, byte })`. `Display` and `FromStr` read one spelling, and `Channel::Diagnostic.to_string()` and `"diagnostic".parse::<Channel>()` round-trip.
