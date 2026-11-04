// --- [OPERATIONS] ----------------------------------------------------------------------

snapshots! {
fn duplicate_body(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `Route::url_path` repeats the body and signature of `Route::path`
       --> tools/dylint/examples/duplicate_body.rs:118:5
        |
    118 |     fn url_path(&self) -> String { self.segments.join("/") }
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `Route::path` declared here
       --> tools/dylint/examples/duplicate_body.rs:116:5
        |
    116 |     fn path(&self) -> String { self.segments.join("/") }
        |     ^^^^^^^^^^^^^^^^^^^^^^^^
        = help: delete `Route::url_path` and call `Route::path` at each call site
        = note: `#[warn(duplicate_body)]` on by default

    warning: `Display` impls for `Square` and `Circle` have the same `fmt` body
       --> tools/dylint/examples/duplicate_body.rs:134:5
        |
    134 |     fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str("shape") }
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: repeated here
       --> tools/dylint/examples/duplicate_body.rs:138:5
        |
    138 |     fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str("shape") }
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: generate the `Display` impls from one `macro_rules!` over the implementing type

    warning: `Display` impls for `Labeled<u8>` and `Labeled<u16>` have the same `fmt` body
       --> tools/dylint/examples/duplicate_body.rs:168:5
        |
    168 |     fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str(self.name) }
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: repeated here
       --> tools/dylint/examples/duplicate_body.rs:172:5
        |
    172 |     fn fmt(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str(self.name) }
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: replace the impls with one `Display` impl generic over `Labeled` type arguments

    warning: `Extend` impls for `Square` and `Circle` have the same `extend` body
       --> tools/dylint/examples/duplicate_body.rs:142:5
        |
    142 | /     fn extend<I>(&mut self, items: I)
    143 | |     where I: IntoIterator<Item = C> {
        | |___________________________________^
        |
    note: repeated here
       --> tools/dylint/examples/duplicate_body.rs:149:5
        |
    149 | /     fn extend<I>(&mut self, items: I)
    150 | |     where I: IntoIterator<Item = C> {
        | |___________________________________^
        = help: generate the `Extend` impls from one `macro_rules!` over the implementing type

    warning: `sum` repeats the body and signature of `total`
       --> tools/dylint/examples/duplicate_body.rs:223:1
        |
    223 | fn sum(numbers: &[u32]) -> u32 { numbers.iter().sum() }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `total` declared here
       --> tools/dylint/examples/duplicate_body.rs:221:1
        |
    221 | fn total(values: &[u32]) -> u32 { values.iter().sum() }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: delete `sum` and call `total` at each call site

    warning: `second` repeats the body and signature of `first`
       --> tools/dylint/examples/duplicate_body.rs:231:1
        |
    231 | async fn second(value: u32) -> u32 { value.strict_add(1) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `first` declared here
       --> tools/dylint/examples/duplicate_body.rs:229:1
        |
    229 | async fn first(value: u32) -> u32 { value.strict_add(1) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: delete `second` and call `first` at each call site

    warning: `contracts::renamed` repeats the body and signature of `contracts::borrowed`
       --> tools/dylint/examples/duplicate_body.rs:386:5
        |
    386 |     pub(super) fn renamed<'other>(value: &'other str) -> &'other str { value }
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `contracts::borrowed` declared here
       --> tools/dylint/examples/duplicate_body.rs:384:5
        |
    384 |     pub(super) fn borrowed<'a>(value: &'a str) -> &'a str { value }
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: delete `contracts::renamed` and call `contracts::borrowed` at each call site

    warning: `contracts::renamed_outlives` repeats the body and signature of `contracts::outlives`
       --> tools/dylint/examples/duplicate_body.rs:394:5
        |
    394 |     pub(super) fn renamed_outlives<'a: 'b, 'b>(first: &'a str, second: &'b str) -> (&'a str, &'b str) { (first, second) }
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `contracts::outlives` declared here
       --> tools/dylint/examples/duplicate_body.rs:392:5
        |
    392 |     pub(super) fn outlives<'long: 'short, 'short>(first: &'long str, second: &'short str) -> (&'long str, &'short str) { (first, se...
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: delete `contracts::renamed_outlives` and call `contracts::outlives` at each call site

    warning: `parameter_drops::values::renamed` repeats the body and signature of `parameter_drops::values::declared`
       --> tools/dylint/examples/duplicate_body.rs:412:9
        |
    412 |         fn renamed<T>(Fields { first: _head, last: _tail }: Fields<T>, visit: impl FnOnce()) { visit() }
        |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `parameter_drops::values::declared` declared here
       --> tools/dylint/examples/duplicate_body.rs:408:9
        |
    408 |         fn declared<T>(Fields { first: _first, last: _last }: Fields<T>, visit: impl FnOnce()) { visit() }
        |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: delete `parameter_drops::values::renamed` and call `parameter_drops::values::declared` at each call site

    warning: `stateful_binary::repeated_calls` repeats the body and signature of `stateful_binary::ordered_calls`
       --> tools/dylint/examples/duplicate_body.rs:442:5
        |
    442 |     pub(super) fn repeated_calls(state: &Cell<u8>, left: u8, right: u8) -> (u8, u8) { (state.replace(left), state.replace(right)) }
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `stateful_binary::ordered_calls` declared here
       --> tools/dylint/examples/duplicate_body.rs:438:5
        |
    438 |     pub(super) fn ordered_calls(state: &Cell<u8>, left: u8, right: u8) -> (u8, u8) { (state.replace(left), state.replace(right)) }
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: delete `stateful_binary::repeated_calls` and call `stateful_binary::ordered_calls` at each call site

    warning: every `Counted` impl has the same `count` body
       --> tools/dylint/examples/duplicate_body.rs:471:9
        |
    471 |         fn count(values: &[u8]) -> usize;
        |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: repeated here
       --> tools/dylint/examples/duplicate_body.rs:511:9
        |
    511 |         fn count(values: &[u8]) -> usize { values.len() }
        |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    ...
    515 |         fn count(values: &[u8]) -> usize { values.len() }
        |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: move the body into `Counted::count` as its default and delete `count` from each impl

    warning: every `Described` impl has the same `width` body
       --> tools/dylint/examples/duplicate_body.rs:475:9
        |
    475 |         fn width(value: &T) -> usize;
        |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: repeated here
       --> tools/dylint/examples/duplicate_body.rs:519:9
        |
    519 |         fn width(value: &T) -> usize { value.to_string().len() }
        |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    ...
    523 |         fn width(value: &T) -> usize { value.to_string().len() }
        |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: move the body into `Described::width` as its default and delete `width` from each impl

    warning: `Iterator` impls for `Countdown` and `Repeated` have the same `next` body
      --> tools/dylint/examples/manual_gen_block.rs:59:5
       |
    59 |     fn next(&mut self) -> Option<u8> {
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: repeated here
      --> tools/dylint/examples/manual_gen_block.rs:89:5
       |
    89 |     fn next(&mut self) -> Option<u8> {
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: generate the `Iterator` impls from one `macro_rules!` over the implementing type
       = note: `#[warn(duplicate_body)]` on by default

    warning: `AsRef` impls for `Title` and `Conditional` have the same `as_ref` body
       --> tools/dylint/examples/manual_reference_impl.rs:91:5
        |
     91 |     fn as_ref(&self) -> &str { self.0.as_ref() }
        |     ^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: repeated here
       --> tools/dylint/examples/manual_reference_impl.rs:207:5
        |
    207 |     fn as_ref(&self) -> &str { self.0.as_ref() }
        |     ^^^^^^^^^^^^^^^^^^^^^^^^
        = help: generate the `AsRef` impls from one `macro_rules!` over the implementing type
        = note: `#[warn(duplicate_body)]` on by default
    "#);
}

fn duplicate_trait_constant(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: every `Arity` impl sets `ARITY` to `2_usize`
      --> tools/dylint/examples/duplicate_trait_constant.rs:6:5
       |
     6 |     const ARITY: usize;
       |     ^^^^^^^^^^^^^^^^^^
       |
    note: set here
      --> tools/dylint/examples/duplicate_trait_constant.rs:39:5
       |
    39 |     const ARITY: usize = 2;
       |     ^^^^^^^^^^^^^^^^^^
    ...
    43 |     const ARITY: usize = 2;
       |     ^^^^^^^^^^^^^^^^^^
       = help: set the default of `Arity::ARITY` to `2_usize` and delete `ARITY` from each impl
       = note: `#[warn(duplicate_trait_constant)]` on by default

    warning: every `Depth` impl sets `DEPTH` to `4_u8`
      --> tools/dylint/examples/duplicate_trait_constant.rs:14:5
       |
    14 |     const DEPTH: u8 = 1;
       |     ^^^^^^^^^^^^^^^
       |
    note: set here
      --> tools/dylint/examples/duplicate_trait_constant.rs:55:5
       |
    55 |     const DEPTH: u8 = 4;
       |     ^^^^^^^^^^^^^^^
    ...
    59 |     const DEPTH: u8 = 4;
       |     ^^^^^^^^^^^^^^^
       = help: set the default of `Depth::DEPTH` to `4_u8` and delete `DEPTH` from each impl

    warning: every `Meta` impl sets `VALUE` to `5_u32`
       --> tools/dylint/examples/duplicate_trait_constant.rs:125:9
        |
    125 |         const VALUE: u32;
        |         ^^^^^^^^^^^^^^^^
        |
    note: set here
       --> tools/dylint/examples/duplicate_trait_constant.rs:142:13
        |
    142 |             const VALUE: u32 = LIMIT;
        |             ^^^^^^^^^^^^^^^^
    ...
    154 |             const VALUE: u32 = LIMIT;
        |             ^^^^^^^^^^^^^^^^
        = help: set the default of `Meta::VALUE` to `5_u32` and delete `VALUE` from each impl

    warning: every `Other` impl sets `NAME` to `"shared"`
       --> tools/dylint/examples/duplicate_trait_constant.rs:167:9
        |
    167 |         const NAME: &'static str;
        |         ^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: set here
       --> tools/dylint/examples/duplicate_trait_constant.rs:173:9
        |
    173 |         const NAME: &'static str = "shared";
        |         ^^^^^^^^^^^^^^^^^^^^^^^^
    ...
    177 |         const NAME: &'static str = "shared";
        |         ^^^^^^^^^^^^^^^^^^^^^^^^
        = help: set the default of `Other::NAME` to `"shared"` and delete `NAME` from each impl

    warning: every `Own` impl sets `VALUE` to `<Self as Own>::NAME`
       --> tools/dylint/examples/duplicate_trait_constant.rs:238:9
        |
    238 |         const VALUE: &'static str;
        |         ^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: set here
       --> tools/dylint/examples/duplicate_trait_constant.rs:265:9
        |
    265 |         const VALUE: &'static str = <Self as Own>::NAME;
        |         ^^^^^^^^^^^^^^^^^^^^^^^^^
    ...
    270 |         const VALUE: &'static str = <Self as Own>::NAME;
        |         ^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: set the default of `Own::VALUE` to `<Self as Own>::NAME` and delete `VALUE` from each impl

    warning: every `Generic` impl sets `VALUE` to `T::NAME`
       --> tools/dylint/examples/duplicate_trait_constant.rs:258:9
        |
    258 |         const VALUE: &'static str;
        |         ^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: set here
       --> tools/dylint/examples/duplicate_trait_constant.rs:314:9
        |
    314 |         const VALUE: &'static str = T::NAME;
        |         ^^^^^^^^^^^^^^^^^^^^^^^^^
    ...
    318 |         const VALUE: &'static str = T::NAME;
        |         ^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: set the default of `Generic::VALUE` to `T::NAME` and delete `VALUE` from each impl

    warning: every `Shortened` impl sets `VALUE` to `<Self as StaticSource>::NAME`
       --> tools/dylint/examples/duplicate_trait_constant.rs:355:9
        |
    355 |         const VALUE: &'a str;
        |         ^^^^^^^^^^^^^^^^^^^^
        |
    note: set here
       --> tools/dylint/examples/duplicate_trait_constant.rs:393:9
        |
    393 |         const VALUE: &'a str = <Self as StaticSource>::NAME;
        |         ^^^^^^^^^^^^^^^^^^^^
    ...
    397 |         const VALUE: &'a str = <Self as StaticSource>::NAME;
        |         ^^^^^^^^^^^^^^^^^^^^
        = help: set the default of `Shortened::VALUE` to `<Self as StaticSource>::NAME` and delete `VALUE` from each impl
    "#);
}

fn duplicate_type(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `Endpoint` duplicates `Origin`
      --> tools/dylint/examples/duplicate_type.rs:19:1
       |
    19 | struct Endpoint {
       | ^^^^^^^^^^^^^^^
       |
    note: `Origin` declared here
      --> tools/dylint/examples/duplicate_type.rs:13:1
       |
    13 | struct Origin {
       | ^^^^^^^^^^^^^
       = help: use `Origin` in place of `Endpoint`
       = note: `#[warn(duplicate_type)]` on by default

    warning: `Groups` repeats `Users` with `Group` in place of `User`
      --> tools/dylint/examples/duplicate_type.rs:45:1
       |
    45 | struct Groups {
       | ^^^^^^^^^^^^^
       |
    note: `Users` declared here
      --> tools/dylint/examples/duplicate_type.rs:39:1
       |
    39 | struct Users {
       | ^^^^^^^^^^^^
       = help: use `Users` generic over the differing field types in place of `Groups`

    warning: `Members` duplicates `Paged<User>`
      --> tools/dylint/examples/duplicate_type.rs:57:1
       |
    57 | struct Members {
       | ^^^^^^^^^^^^^^
       |
    note: `Paged` declared here
      --> tools/dylint/examples/duplicate_type.rs:51:1
       |
    51 | struct Paged<T> {
       | ^^^^^^^^^^^^^^^
       = help: use `Paged<User>` in place of `Members`

    warning: every variant of `Event` repeats the same fields
      --> tools/dylint/examples/duplicate_type.rs:63:1
       |
    63 | enum Event {
       | ^^^^^^^^^^
       |
       = help: declare `Event` as a struct of the shared fields and a fieldless enum of its variants

    warning: variant `Fetch` repeats the fields of `Origin`
      --> tools/dylint/examples/duplicate_type.rs:70:5
       |
    70 |     Fetch { host: String, port: u16 },
       |     ^^^^^
       |
       = help: declare `Fetch(Origin)`

    warning: `Location` repeats `Link` fields `host` and `path`
       --> tools/dylint/examples/duplicate_type.rs:82:1
        |
     82 | struct Location {
        | ^^^^^^^^^^^^^^^
        |
    note: built from `Link` fields here
       --> tools/dylint/examples/duplicate_type.rs:419:39
        |
    419 | fn located(link: &Link) -> Location { Location { host: link.host.clone(), path: link.path.clone() } }
        |                                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: hold `Location` in `Link` in place of `host` and `path`

    warning: `AddressDto` repeats `Address` member by member
       --> tools/dylint/examples/duplicate_type.rs:106:1
        |
    106 | struct AddressDto {
        | ^^^^^^^^^^^^^^^^^
        |
    note: converted from `Address` here
       --> tools/dylint/examples/duplicate_type.rs:501:15
        |
    501 |     let dto = AddressDto { port: u16::from(address.port), limit: u32::from(address.limit) };
        |               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: use `Address` generic over the differing member types in place of `AddressDto`

    warning: `AccountRow` repeats `Account` member by member
       --> tools/dylint/examples/duplicate_type.rs:118:1
        |
    118 | struct AccountRow {
        | ^^^^^^^^^^^^^^^^^
        |
    note: converted from `Account` here
       --> tools/dylint/examples/duplicate_type.rs:382:15
        |
    382 |     let row = AccountRow { user: account.login.clone(), address: account.email.clone() };
        |               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: use `Account` in place of `AccountRow`

    warning: `Reading` repeats `Celsius` member by member
       --> tools/dylint/examples/duplicate_type.rs:127:1
        |
    127 | struct Reading(f64);
        | ^^^^^^^^^^^^^^
        |
    note: converted from `Celsius` here
       --> tools/dylint/examples/duplicate_type.rs:386:50
        |
    386 | const fn reading(celsius: &Celsius) -> Reading { Reading(celsius.0) }
        |                                                  ^^^^^^^^^^^^^^^^^^
        = help: use `Celsius` in place of `Reading`

    warning: `ShapeKind` repeats the variants of `Shape` without their fields
       --> tools/dylint/examples/duplicate_type.rs:171:1
        |
    171 | enum ShapeKind {
        | ^^^^^^^^^^^^^^
        |
    note: built from a match on `Shape` here
       --> tools/dylint/examples/duplicate_type.rs:363:9
        |
    363 | /         match self {
    364 | |             Self::Circle(_) => ShapeKind::Circle,
    365 | |             Self::Square(_) => ShapeKind::Square,
    366 | |         }
        | |_________^
        = help: match `Shape` in place of `ShapeKind`

    warning: `Signal` repeats `Phase` member by member
       --> tools/dylint/examples/duplicate_type.rs:196:1
        |
    196 | enum Signal {
        | ^^^^^^^^^^^
        |
    note: converted from `Phase` here
       --> tools/dylint/examples/duplicate_type.rs:391:5
        |
    391 | /     match phase {
    392 | |         Phase::Start => Signal::Begin,
    393 | |         Phase::Stop => Signal::End,
    394 | |     }
        | |_____^
        = help: use `Phase` in place of `Signal`

    warning: `Invocation` duplicates `Notice<M, std::option::Option<P>>`
       --> tools/dylint/examples/duplicate_type.rs:220:1
        |
    220 | struct Invocation<M, P> {
        | ^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `Notice` declared here
       --> tools/dylint/examples/duplicate_type.rs:214:1
        |
    214 | struct Notice<M, P> {
        | ^^^^^^^^^^^^^^^^^^^
        = help: use `Notice<M, std::option::Option<P>>` in place of `Invocation`

    warning: `Frame` duplicates `Message`
       --> tools/dylint/examples/duplicate_type.rs:239:1
        |
    239 | enum Frame {
        | ^^^^^^^^^^
        |
    note: `Message` declared here
       --> tools/dylint/examples/duplicate_type.rs:233:1
        |
    233 | enum Message {
        | ^^^^^^^^^^^^
        = help: use `Message` in place of `Frame`

    warning: `Bounds` duplicates `std::ops::Range<u32>`
       --> tools/dylint/examples/duplicate_type.rs:259:1
        |
    259 | struct Bounds {
        | ^^^^^^^^^^^^^
        |
        = help: use `std::ops::Range<u32>` in place of `Bounds`

    warning: `Captured` duplicates `std::process::Output`
       --> tools/dylint/examples/duplicate_type.rs:284:1
        |
    284 | struct Captured {
        | ^^^^^^^^^^^^^^^
        |
        = help: use `std::process::Output` in place of `Captured`

    warning: `Rejected` duplicates `std::sync::mpmc::TrySendError<T>`
       --> tools/dylint/examples/duplicate_type.rs:291:1
        |
    291 | enum Rejected<T> {
        | ^^^^^^^^^^^^^^^^
        |
        = help: use `std::sync::mpmc::TrySendError<T>` in place of `Rejected`

    warning: `Outcome` duplicates `std::result::Result<T, E>`
       --> tools/dylint/examples/duplicate_type.rs:303:1
        |
    303 | enum Outcome<T, E> {
        | ^^^^^^^^^^^^^^^^^^
        |
        = help: use `std::result::Result<T, E>` in place of `Outcome`

    warning: error variants `Read` and `Write` of `ManifestError` repeat the same fields
       --> tools/dylint/examples/duplicate_type.rs:318:1
        |
    318 | enum ManifestError {
        | ^^^^^^^^^^^^^^^^^^
        |
    note: repeated here
       --> tools/dylint/examples/duplicate_type.rs:320:5
        |
    320 |     Read(#[source] io::Error),
        |     ^^^^
    321 |     #[error("cannot write the manifest")]
    322 |     Write(#[source] io::Error),
        |     ^^^^^
        = help: replace `Read` and `Write` with one variant of the shared fields and a fieldless `derive_more::Display` enum for their messages

    warning: error variants `Runtime` and `Serve` of `ServeError` repeat the same fields
       --> tools/dylint/examples/duplicate_type.rs:328:1
        |
    328 | enum ServeError<X> {
        | ^^^^^^^^^^^^^^^^^^
        |
    note: repeated here
       --> tools/dylint/examples/duplicate_type.rs:330:5
        |
    330 |     Runtime(#[source] io::Error),
        |     ^^^^^^^
    331 |     #[error("cannot serve the endpoint")]
    332 |     Serve(#[source] io::Error),
        |     ^^^^^
        = help: replace `Runtime` and `Serve` with one variant of the shared fields and a fieldless `derive_more::Display` enum for their messages

    warning: error variants `Invalid` and `Truncated` of `SchemaError` repeat the same fields
       --> tools/dylint/examples/duplicate_type.rs:346:1
        |
    346 | enum SchemaError {
        | ^^^^^^^^^^^^^^^^
        |
    note: repeated here
       --> tools/dylint/examples/duplicate_type.rs:348:5
        |
    348 |     Invalid { path: String, line: u32 },
        |     ^^^^^^^
    349 |     #[error("{path} ends at line {line}")]
    350 |     Truncated { path: String, line: u32 },
        |     ^^^^^^^^^
        = help: replace `Invalid` and `Truncated` with one variant of the shared fields and a fieldless `derive_more::Display` enum for their messages

    warning: `Shim` duplicates `AddressDto`
       --> tools/dylint/examples/duplicate_type.rs:411:5
        |
    411 |     struct Shim {
        |     ^^^^^^^^^^^
        |
    note: `AddressDto` declared here
       --> tools/dylint/examples/duplicate_type.rs:106:1
        |
    106 | struct AddressDto {
        | ^^^^^^^^^^^^^^^^^
        = help: use `AddressDto` in place of `Shim`

    warning: `NativeOrder` duplicates `std::cmp::Ordering`
       --> tools/dylint/examples/duplicate_type.rs:611:5
        |
    611 |     enum NativeOrder {
        |     ^^^^^^^^^^^^^^^^
        |
        = help: use `std::cmp::Ordering` in place of `NativeOrder`

    warning: `Echo` duplicates `contracts::Packet<N>`
       --> tools/dylint/examples/duplicate_type.rs:624:5
        |
    624 |     struct Echo<const N: usize> {
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `Packet` declared here
       --> tools/dylint/examples/duplicate_type.rs:618:5
        |
    618 |     struct Packet<const N: usize> {
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: use `contracts::Packet<N>` in place of `Echo`

    warning: `Numbered` duplicates `contracts::Unnumbered`
       --> tools/dylint/examples/duplicate_type.rs:630:5
        |
    630 |     struct Numbered<const N: usize> {
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `Unnumbered` declared here
       --> tools/dylint/examples/duplicate_type.rs:636:5
        |
    636 |     struct Unnumbered {
        |     ^^^^^^^^^^^^^^^^^
        = help: use `contracts::Unnumbered` in place of `Numbered`

    warning: `Defaulted` duplicates `contracts::Unnumbered`
       --> tools/dylint/examples/duplicate_type.rs:642:5
        |
    642 |     struct Defaulted<const N: usize = 4> {
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `Unnumbered` declared here
       --> tools/dylint/examples/duplicate_type.rs:636:5
        |
    636 |     struct Unnumbered {
        |     ^^^^^^^^^^^^^^^^^
        = help: use `contracts::Unnumbered` in place of `Defaulted`

    warning: `Strict` duplicates `contracts::Loose<T>`
       --> tools/dylint/examples/duplicate_type.rs:648:5
        |
    648 |     struct Strict<T: Clone> {
        |     ^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `Loose` declared here
       --> tools/dylint/examples/duplicate_type.rs:654:5
        |
    654 |     struct Loose<T> {
        |     ^^^^^^^^^^^^^^^
        = help: use `contracts::Loose<T>` in place of `Strict`

    warning: `Fixed` duplicates `contracts::Borrowed<'static>`
       --> tools/dylint/examples/duplicate_type.rs:660:5
        |
    660 |     struct Fixed {
        |     ^^^^^^^^^^^^
        |
    note: `Borrowed` declared here
       --> tools/dylint/examples/duplicate_type.rs:666:5
        |
    666 |     struct Borrowed<'a> {
        |     ^^^^^^^^^^^^^^^^^^^
        = help: use `contracts::Borrowed<'static>` in place of `Fixed`

    warning: `Joined` duplicates `contracts::Forward<&'a str>`
       --> tools/dylint/examples/duplicate_type.rs:672:5
        |
    672 |     struct Joined<'a> {
        |     ^^^^^^^^^^^^^^^^^
        |
    note: `Forward` declared here
       --> tools/dylint/examples/duplicate_type.rs:573:5
        |
    573 |     struct Forward<T> {
        |     ^^^^^^^^^^^^^^^^^
        = help: use `contracts::Forward<&'a str>` in place of `Joined`

    warning: `Renamed` repeats `Source` member by member
       --> tools/dylint/examples/duplicate_type.rs:890:5
        |
    890 |     enum Renamed {
        |     ^^^^^^^^^^^^
        |
    note: converted from `Source` here
       --> tools/dylint/examples/duplicate_type.rs:932:9
        |
    932 | /         match source {
    933 | |             Source::Pair(first, last) => Renamed::Values(first, last),
    934 | |             Source::Empty => Renamed::Finished,
    935 | |         }
        | |_________^
        = help: use `Source` in place of `Renamed`

    warning: error variants `Retry` and `Text` of `PortError` repeat the same fields
      --> tools/dylint/examples/redundant_error_conversion.rs:24:1
       |
    24 | enum PortError {
       | ^^^^^^^^^^^^^^
       |
    note: repeated here
      --> tools/dylint/examples/redundant_error_conversion.rs:26:5
       |
    26 |     Text(#[from] Utf8Error),
       |     ^^^^
    ...
    30 |     Retry(#[source] Utf8Error),
       |     ^^^^^
       = help: replace `Retry` and `Text` with one variant of the shared fields and a fieldless `derive_more::Display` enum for their messages
       = note: `#[warn(duplicate_type)]` on by default

    warning: error variants `Accept`, `Close` and `Receive` of `Failure` repeat the same fields
      --> tools/dylint/examples/source_in_error_message.rs:7:1
       |
     7 | enum Failure {
       | ^^^^^^^^^^^^
       |
    note: repeated here
      --> tools/dylint/examples/source_in_error_message.rs:32:5
       |
    32 |     Close { source: io::Error },
       |     ^^^^^
    33 |     #[error(r"cannot receive: {source}")]
    34 |     Receive { source: io::Error },
       |     ^^^^^^^
    35 |     #[error("cannot accept: \u{7b}source\u{7d}")]
    36 |     Accept { source: io::Error },
       |     ^^^^^^
       = help: replace `Accept`, `Close` and `Receive` with one variant of the shared fields and a fieldless `derive_more::Display` enum for their messages
       = note: `#[warn(duplicate_type)]` on by default

    warning: error variants `Message` and `Received` of `ConfigError` repeat the same fields
       --> tools/dylint/examples/untyped_error.rs:119:1
        |
    119 | enum ConfigError {
        | ^^^^^^^^^^^^^^^^
        |
    note: repeated here
       --> tools/dylint/examples/untyped_error.rs:121:5
        |
    121 |     Message(String),
        |     ^^^^^^^
    122 |     #[error("{0}")]
    123 |     Received(String),
        |     ^^^^^^^^
        = help: replace `Message` and `Received` with one variant of the shared fields and a fieldless `derive_more::Display` enum for their messages
        = note: `#[warn(duplicate_type)]` on by default
    "#);
}

fn parameterizable_body(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `keyword_json` repeats the body of `decoded` with another type in place of `Port`
      --> tools/dylint/examples/decode_without_path.rs:44:1
       |
    44 | fn decoded(text: &str) -> Result<Port, serde_json::Error> { Port::deserialize(&mut serde_json::Deserializer::from_str(text)) }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: repeated here
      --> tools/dylint/examples/decode_without_path.rs:63:1
       |
    63 | fn keyword_json(text: &str) -> Result<Keyword, serde_json::Error> { Keyword::deserialize(&mut serde_json::Deserializer::from_str(tex...
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: replace `decoded` and `keyword_json` with one function generic over `S: Deserialize<'_>`
       = note: `#[warn(parameterizable_body)]` on by default

    warning: `scalar_record` and `custom_unit` repeat the body of `keyword` with another type in place of `Keyword`
      --> tools/dylint/examples/decode_without_path.rs:61:1
       |
    61 | fn keyword(text: &str) -> Result<Keyword, value::Error> { Keyword::deserialize(value::StrDeserializer::new(text)) }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: repeated here
      --> tools/dylint/examples/decode_without_path.rs:65:1
       |
    65 | fn scalar_record(text: &str) -> Result<Port, value::Error> { Port::deserialize(value::StrDeserializer::new(text)) }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    ...
    69 | fn custom_unit(text: &str) -> Result<Delegated, value::Error> { Delegated::deserialize(value::StrDeserializer::new(text)) }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: replace `keyword`, `scalar_record` and `custom_unit` with one function generic over `S: Deserialize<'_>`

    warning: `<Banner as Render>::render` and `<Footer as Render>::render` repeat the body of `<Page as Render>::render` with another value in place of `"page"`
      --> tools/dylint/examples/parameterizable_body.rs:76:5
       |
    76 |     fn render(&self) -> String { String::from("page") }
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: repeated here
      --> tools/dylint/examples/parameterizable_body.rs:80:5
       |
    80 |     fn render(&self) -> String { String::from("banner") }
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^
    ...
    84 |     fn render(&self) -> String { String::from("footer") }
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: replace `Page`, `Banner` and `Footer` with one struct holding the value, one `const` per replaced type, and one `Render` impl
       = note: `#[warn(parameterizable_body)]` on by default

    warning: `retry_writes` repeats the body of `retry_reads` with another value in place of `3`
       --> tools/dylint/examples/parameterizable_body.rs:119:1
        |
    119 | const fn retry_reads(client: &Client) -> Policy { Policy::new(client, 3, TIMEOUT.saturating_mul(2)) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: repeated here
       --> tools/dylint/examples/parameterizable_body.rs:121:1
        |
    121 | const fn retry_writes(client: &Client) -> Policy { Policy::new(client, 5, TIMEOUT.saturating_mul(2)) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: call `Policy::new` directly at each call site of `retry_reads` and `retry_writes`

    warning: `total_circles` repeats the body of `total_squares` with another type in place of `Square`
       --> tools/dylint/examples/parameterizable_body.rs:123:1
        |
    123 | fn total_squares(shapes: &[Square]) -> f64 { shapes.iter().map(Shape::area).sum() }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: repeated here
       --> tools/dylint/examples/parameterizable_body.rs:125:1
        |
    125 | fn total_circles(shapes: &[Circle]) -> f64 { shapes.iter().map(Shape::area).sum() }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: replace `total_squares` and `total_circles` with one function generic over `S: Shape`

    warning: `ticked` repeats the body of `started` with another type in place of `Uptime`
       --> tools/dylint/examples/parameterizable_body.rs:165:1
        |
    165 | fn started(value: u64) -> Uptime { Uptime::from(value.strict_add(1)) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: repeated here
       --> tools/dylint/examples/parameterizable_body.rs:167:1
        |
    167 | fn ticked(value: u64) -> Clock { Clock::from(value.strict_add(1)) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: replace `started` and `ticked` with one function generic over `S: From<u64>`
    "#);
}

}
