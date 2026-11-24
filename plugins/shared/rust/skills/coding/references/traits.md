# [TRAITS]

## [01]-[CAPABILITIES]

`#[auto_impl]` forwards a trait from each listed pointer type to its pointee, and `where Self: Sized` keeps a generic default off the vtable:

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

`keep_default_for` runs the default body on each listed proxy in place of forwarding it to the pointee:
- Generated impls take `T: ?Sized` unless a method they forward takes `self`, returns `Self`, or bounds `Self: Sized`
- `Box<dyn Codec<Value = Reading, Error = CodecError> + Send + Sync>` implements `Codec` through that relaxation and runs `decode_all` itself
- `Fn` proxies let a closure implement a trait of one method and no associated item, and `&` or `Box` beside `Fn` overlap it

## [02]-[ASSOCIATED_TYPE_BOUNDS]

Bounds fix one associated type and constrain another in one clause, with no type parameter naming it:

```rust
pub fn stored<C>(codec: &C, frames: &[Vec<u8>]) -> Result<Vec<Reading>, NEVec<StoreError>>
where C: Codec<Value = Reading, Error: Into<StoreError>> {
    codec.decode_all(frames).ok().map_err(|errors| errors.into_nonempty_iter().map(Into::into).collect())
}
```

`StoreError` converts from each codec's error, and a boxed, shared, or borrowed codec passes `C` through the proxy impls:
- `Item: AsRef<[u8]>` and `Error: Into<StoreError>` hold in `where` clauses, generic parameters, and `impl Trait` in either position
- `dyn` types refuse an associated type bound, and a `dyn` holder names the type with `=`

## [03]-[ASYNC_CAPABILITIES]

`dynosaur` gives a trait returning futures a dynamic type, boxing each future with the bounds its method states:

```rust
#[dynosaur::dynosaur(pub DynArchive = dyn(box) Archive)]
pub trait Archive: Send + Sync {
    fn put(&self, key: &str, bytes: &[u8]) -> impl Future<Output = Result<(), ArchiveError>> + Send;
}

pub async fn mirrored<A, B>(primary: &A, replica: &B, key: &str, bytes: &[u8]) -> Result<(), NEVec<ArchiveError>>
where
    A: Archive + ?Sized,
    B: Archive + ?Sized,
{
    let (first, second) = future::join(primary.put(key, bytes), replica.put(key, bytes)).await;
    Validated::from(first).map2(Validated::from(second), |(), ()| ()).ok()
}
```

One `mirrored` serves a concrete archive, a `&DynArchive<'_>`, and a `Box<DynArchive<'_>>`, its future `Send` in each case:
- Traits whose dynamic values reach spawned tasks state `Send + Sync` as supertraits
- `new_box`, `new_arc`, and `new_rc` erase an owned value, and `from_box`, `from_ref`, and `from_mut` an existing pointer, with no implicit coercion
- Bridge impls cover `Box<T>`, `&mut T` where every receiver borrows, and `&T` where each is `&self`, all over `T: ?Sized`
- `bridge(dyn)` or `bridge(none)` replaces the bridge impls where a crate's own impl overlaps them
- `where Self: Sized` fails to compile under `dynosaur`, and a generic default method belongs to an `auto_impl` trait
- `dynosaur` copies a public trait into a private module, and crates denying `unreachable_pub` set its `#[expect]` on the declaring module
