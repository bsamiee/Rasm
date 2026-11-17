# [TRAITS]

## [01]-[CAPABILITIES]

Foreign traits can use `&mut State` for local state and return borrowed compound values without a separate adapter type.

`where Self: Sized` keeps a generic default method off the vtable:

```rust
#[auto_impl(&, Arc, Box)]
pub trait Codec {
    type Value;
    type Error;

    fn decode(&self, bytes: &[u8]) -> Result<Self::Value, Self::Error>;

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

Consumers fix one associated type and bound another in one clause: `C: Codec<Value = Reading, Error: Into<StoreError>>`. `Box<dyn Codec<Value = Reading, Error = CodecError> + Send + Sync>` names every associated type and auto trait its holder needs. `keep_default_for` keeps the default on each proxy in place of forwarding it, and generated impls relax to `T: ?Sized`. Boxed trait objects pass that `C: Codec` bound and reach `decode_all` through the default.

## [02]-[ASYNC_CAPABILITIES]

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

Implementors write `async fn put`, and `DynArchive::new_box` or `DynArchive::new_arc` erases a concrete archive. `dynosaur` implements `Archive` for `Box<T>`, `&T`, and `&mut T` with `T: ?Sized`. One `replicate` serves a slice of one archive type statically and a `Vec<Box<DynArchive<'_>>>` of archives chosen at run time. Every target's failure reaches the caller. `dynosaur` rejects `where Self: Sized`, and a generic default method belongs to an `auto_impl` trait.
