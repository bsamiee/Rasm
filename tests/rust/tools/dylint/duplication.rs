// --- [OPERATIONS] ----------------------------------------------------------------------

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

    warning: every `Shape` impl has the same `doubled` body
       --> tools/dylint/examples/duplicate_body.rs:13:5
        |
     13 |     fn doubled(&self) -> f64;
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: repeated here
       --> tools/dylint/examples/duplicate_body.rs:124:5
        |
    124 |     fn doubled(&self) -> f64 { self.area() * 2.0 }
        |     ^^^^^^^^^^^^^^^^^^^^^^^^
    ...
    130 |     fn doubled(&self) -> f64 { self.area() * 2.0 }
        |     ^^^^^^^^^^^^^^^^^^^^^^^^
        = help: move the body into `Shape::doubled` as its default and delete `doubled` from each impl

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

    warning: `Catalog::labels` repeats the body and signature of `Catalog::names`
       --> tools/dylint/examples/duplicate_body.rs:178:5
        |
    178 |     fn labels(&self) -> Vec<String> { self.items.iter().map(|entry| entry.name.clone()).collect() }
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `Catalog::names` declared here
       --> tools/dylint/examples/duplicate_body.rs:176:5
        |
    176 |     fn names(&self) -> Vec<String> { self.items.iter().map(|item| item.name.clone()).collect() }
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: delete `Catalog::labels` and call `Catalog::names` at each call site

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

    warning: `Iterator` impls for `Countdown` and `Repeated` have the same `next` body
      --> tools/dylint/examples/manual_gen_block.rs:21:5
       |
    21 |     fn next(&mut self) -> Option<u8> {
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: repeated here
      --> tools/dylint/examples/manual_gen_block.rs:39:5
       |
    39 |     fn next(&mut self) -> Option<u8> {
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: generate the `Iterator` impls from one `macro_rules!` over the implementing type
       = note: `#[warn(duplicate_body)]` on by default

    warning: `TryFrom` impls for `Port`, `Decoded` and `Guarded` have the same `try_from` body
       --> tools/dylint/examples/unchecked_construction.rs:17:9
        |
     17 | ...   fn try_from(value: u16) -> Result<Self, Self::Error> { if value < 1024 { Err(RangeError(u32::from(value))) } else { Ok(Self(v...
        |       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: repeated here
       --> tools/dylint/examples/unchecked_construction.rs:93:9
        |
     93 | ...   fn try_from(value: u16) -> Result<Self, Self::Error> { if value < 1024 { Err(RangeError(u32::from(value))) } else { Ok(Self(v...
        |       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    ...
    111 | ...   fn try_from(value: u16) -> Result<Self, Self::Error> { if value < 1024 { Err(RangeError(u32::from(value))) } else { Ok(Self(v...
        |       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: generate the `TryFrom` impls from one `macro_rules!` over the implementing type
        = note: `#[warn(duplicate_body)]` on by default
    "#);
}

fn duplicate_trait_constant(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: every `Arity` impl sets `ARITY` to `2`
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
       = help: set the default of `Arity::ARITY` to `2` and delete `ARITY` from each impl
       = note: `#[warn(duplicate_trait_constant)]` on by default

    warning: every `Depth` impl sets `DEPTH` to `4`
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
       = help: set the default of `Depth::DEPTH` to `4` and delete `DEPTH` from each impl
    ");
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
       --> tools/dylint/examples/duplicate_type.rs:411:39
        |
    411 | fn located(link: &Link) -> Location { Location { host: link.host.clone(), path: link.path.clone() } }
        |                                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: hold `Location` in `Link` in place of `host` and `path`

    warning: `AddressDto` repeats `Address` member by member
       --> tools/dylint/examples/duplicate_type.rs:106:1
        |
    106 | struct AddressDto {
        | ^^^^^^^^^^^^^^^^^
        |
    note: converted from `Address` here
       --> tools/dylint/examples/duplicate_type.rs:493:15
        |
    493 |     let dto = AddressDto { port: u16::from(address.port), limit: u32::from(address.limit) };
        |               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: use `Address` generic over the differing member types in place of `AddressDto`

    warning: `AccountRow` repeats `Account` member by member
       --> tools/dylint/examples/duplicate_type.rs:118:1
        |
    118 | struct AccountRow {
        | ^^^^^^^^^^^^^^^^^
        |
    note: converted from `Account` here
       --> tools/dylint/examples/duplicate_type.rs:374:15
        |
    374 |     let row = AccountRow { user: account.login.clone(), address: account.email.clone() };
        |               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: use `Account` in place of `AccountRow`

    warning: `Reading` repeats `Celsius` member by member
       --> tools/dylint/examples/duplicate_type.rs:127:1
        |
    127 | struct Reading(f64);
        | ^^^^^^^^^^^^^^
        |
    note: converted from `Celsius` here
       --> tools/dylint/examples/duplicate_type.rs:378:50
        |
    378 | const fn reading(celsius: &Celsius) -> Reading { Reading(celsius.0) }
        |                                                  ^^^^^^^^^^^^^^^^^^
        = help: use `Celsius` in place of `Reading`

    warning: `ShapeKind` repeats the variants of `Shape` without their fields
       --> tools/dylint/examples/duplicate_type.rs:171:1
        |
    171 | enum ShapeKind {
        | ^^^^^^^^^^^^^^
        |
    note: built from a match on `Shape` here
       --> tools/dylint/examples/duplicate_type.rs:355:9
        |
    355 | /         match self {
    356 | |             Self::Circle(_) => ShapeKind::Circle,
    357 | |             Self::Square(_) => ShapeKind::Square,
    358 | |         }
        | |_________^
        = help: match `Shape` in place of `ShapeKind`

    warning: `Order` duplicates `std::cmp::Ordering`
       --> tools/dylint/examples/duplicate_type.rs:183:1
        |
    183 | enum Order {
        | ^^^^^^^^^^
        |
        = help: use `std::cmp::Ordering` in place of `Order`

    warning: `Signal` repeats `Phase` member by member
       --> tools/dylint/examples/duplicate_type.rs:196:1
        |
    196 | enum Signal {
        | ^^^^^^^^^^^
        |
    note: converted from `Phase` here
       --> tools/dylint/examples/duplicate_type.rs:383:5
        |
    383 | /     match phase {
    384 | |         Phase::Start => Signal::Begin,
    385 | |         Phase::Stop => Signal::End,
    386 | |     }
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

    warning: `Rejected` duplicates `std::sync::mpsc::TrySendError<T>`
       --> tools/dylint/examples/duplicate_type.rs:291:1
        |
    291 | enum Rejected<T> {
        | ^^^^^^^^^^^^^^^^
        |
        = help: use `std::sync::mpsc::TrySendError<T>` in place of `Rejected`

    warning: `Maybe` duplicates `std::option::Option<T>`
       --> tools/dylint/examples/duplicate_type.rs:297:1
        |
    297 | enum Maybe<T> {
        | ^^^^^^^^^^^^^
        |
        = help: use `std::option::Option<T>` in place of `Maybe`

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
       --> tools/dylint/examples/duplicate_type.rs:338:1
        |
    338 | enum SchemaError {
        | ^^^^^^^^^^^^^^^^
        |
    note: repeated here
       --> tools/dylint/examples/duplicate_type.rs:340:5
        |
    340 |     Invalid { path: String, line: u32 },
        |     ^^^^^^^
    341 |     #[error("{path} ends at line {line}")]
    342 |     Truncated { path: String, line: u32 },
        |     ^^^^^^^^^
        = help: replace `Invalid` and `Truncated` with one variant of the shared fields and a fieldless `derive_more::Display` enum for their messages

    warning: `Shim` duplicates `AddressDto`
       --> tools/dylint/examples/duplicate_type.rs:403:5
        |
    403 |     struct Shim {
        |     ^^^^^^^^^^^
        |
    note: `AddressDto` declared here
       --> tools/dylint/examples/duplicate_type.rs:106:1
        |
    106 | struct AddressDto {
        | ^^^^^^^^^^^^^^^^^
        = help: use `AddressDto` in place of `Shim`

    warning: `Limits` duplicates `Config`
      --> tools/dylint/examples/manual_default_field_values.rs:59:1
       |
    59 | struct Limits {
       | ^^^^^^^^^^^^^
       |
    note: `Config` declared here
      --> tools/dylint/examples/manual_default_field_values.rs:42:1
       |
    42 | struct Config {
       | ^^^^^^^^^^^^^
       = help: use `Config` in place of `Limits`
       = note: `#[warn(duplicate_type)]` on by default

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

    warning: error variants `Flush`, `Forwarded` and `Wrapped` of `Failure` repeat the same fields
      --> tools/dylint/examples/source_in_error_message.rs:7:1
       |
     7 | enum Failure {
       | ^^^^^^^^^^^^
       |
    note: repeated here
      --> tools/dylint/examples/source_in_error_message.rs:13:5
       |
    13 |     Wrapped(#[source] io::Error),
       |     ^^^^^^^
    ...
    17 |     Forwarded(io::Error),
       |     ^^^^^^^^^
    ...
    28 |     Flush(#[source] io::Error),
       |     ^^^^^
       = help: replace `Flush`, `Forwarded` and `Wrapped` with one variant of the shared fields and a fieldless `derive_more::Display` enum for their messages
       = note: `#[warn(duplicate_type)]` on by default
    "#);
}

fn parameterizable_body(found: &str) {
    insta::assert_snapshot!(found, @r#"
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

// --- [COMPOSITION] ---------------------------------------------------------------------

snapshots!(duplicate_body, duplicate_trait_constant, duplicate_type, parameterizable_body);
