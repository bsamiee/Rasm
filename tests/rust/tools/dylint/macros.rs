// --- [OPERATIONS] ----------------------------------------------------------------------

snapshots! {
fn assert_matches(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: assertion tests a pattern through a boolean
      --> tools/dylint/../../tests/rust/tools/dylint/macro_matching.rs:96:5
       |
    96 |     assert!(value.is_some(), "received value");
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: assert `core::option::Option::Some(_)` through `core::assert_matches!`, retaining custom messages
       = note: `#[warn(assert_matches)]` on by default

    warning: assertion tests a pattern through a boolean
      --> tools/dylint/../../tests/rust/tools/dylint/macro_matching.rs:97:5
       |
    97 |     debug_assert!(value.is_none());
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: assert `core::option::Option::None` through `core::debug_assert_matches!`, retaining custom messages

    warning: assertion tests a pattern through a boolean
      --> tools/dylint/../../tests/rust/tools/dylint/macro_matching.rs:98:5
       |
    98 |     assert!(matches!(value, Some(_)));
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: assert `Some(_)` through `core::assert_matches!`, retaining custom messages

    warning: assertion tests a pattern through a boolean
      --> tools/dylint/../../tests/rust/tools/dylint/macro_matching.rs:99:5
       |
    99 |     assert!(matches!(value, Some(_) if black_box(true)), "guard");
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: assert `Some(_) if black_box(true)` through `core::assert_matches!`, retaining custom messages

    warning: assertion tests a pattern through a boolean
       --> tools/dylint/../../tests/rust/tools/dylint/macro_matching.rs:113:5
        |
    113 |     assert!(matches!(traced.state, State::Ready(1)));
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: assert `State::Ready(1)` through `core::assert_matches!`, retaining custom messages

    warning: assertion tests a pattern through a boolean
       --> tools/dylint/../../tests/rust/tools/dylint/macro_matching.rs:122:5
        |
    122 |     require!(fits!(value, Some(_)));
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: assert `Some(_)` through `core::assert_matches!`, retaining custom messages

    warning: assertion tests a pattern through a boolean
       --> tools/dylint/../../tests/rust/tools/dylint/macro_matching.rs:123:5
        |
    123 |     require!(fits!(value, Some(count) if count > 1));
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: assert `Some(count) if count > 1` through `core::assert_matches!`, retaining custom messages

    warning: assertion tests a pattern through a boolean
       --> tools/dylint/../../tests/rust/tools/dylint/macro_matching.rs:169:5
        |
    169 |     assert!(present.is_some());
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: assert `core::option::Option::Some(_)` through `core::assert_matches!`, retaining custom messages

    warning: assertion tests a pattern through a boolean
       --> tools/dylint/../../tests/rust/tools/dylint/macro_matching.rs:170:5
        |
    170 |     assert!(absent.is_none());
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: assert `core::option::Option::None` through `core::assert_matches!`, retaining custom messages
    "#);
}

fn chained_matches(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: native pattern tests repeat the same stable subject
      --> tools/dylint/../../tests/rust/tools/dylint/macro_matching.rs:42:9
       |
    42 |         matches!(value, State::Ready(_)) || matches!(value, State::Waiting),
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: test `State::Ready(_) | State::Waiting` in one `matches!` invocation
       = note: `#[warn(chained_matches)]` on by default

    warning: native pattern tests repeat the same stable subject
      --> tools/dylint/../../tests/rust/tools/dylint/macro_matching.rs:43:9
       |
    43 |         !matches!(value, State::Ready(_)) && !matches!(value, State::Waiting),
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: test `State::Ready(_) | State::Waiting` in one negated `matches!` invocation

    warning: native pattern tests repeat the same stable subject
      --> tools/dylint/../../tests/rust/tools/dylint/macro_matching.rs:44:9
       |
    44 |         matches!(states[index], State::Ready(_)) | matches!(states[index], State::Waiting),
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: test `State::Ready(_) | State::Waiting` in one `matches!` invocation

    warning: native pattern tests repeat the same stable subject
      --> tools/dylint/../../tests/rust/tools/dylint/macro_matching.rs:45:9
       |
    45 |         matches!(traced.state, State::Ready(1)) || matches!(traced.state, State::Ready(2)) || matches!(traced.state, State::Waiting),
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: test `State::Ready(1) | State::Ready(2) | State::Waiting` in one `matches!` invocation

    warning: native pattern tests repeat the same stable subject
      --> tools/dylint/../../tests/rust/tools/dylint/macro_matching.rs:46:9
       |
    46 |         matches!(*value, State::Ready(_)) || matches!(*value, State::Waiting),
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: test `State::Ready(_) | State::Waiting` in one `matches!` invocation

    warning: native pattern tests repeat the same stable subject
      --> tools/dylint/../../tests/rust/tools/dylint/macro_matching.rs:74:9
       |
    74 |         matches!(value, itertools::Either::Left(_part @ 1)) || matches!(value, itertools::Either::Right(_part @ 2)),
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: test `itertools::Either::Left(_part @ 1) | itertools::Either::Right(_part @ 2)` in one `matches!` invocation

    warning: native pattern tests repeat the same stable subject
      --> tools/dylint/../../tests/rust/tools/dylint/macro_matching.rs:75:9
       |
    75 |         matches!(value, itertools::Either::Left(ref _part @ 1)) || matches!(value, itertools::Either::Right(ref _part @ 2)),
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: test `itertools::Either::Left(ref _part @ 1) | itertools::Either::Right(ref _part @ 2)` in one `matches!` invocation

    warning: native pattern tests repeat the same stable subject
      --> tools/dylint/../../tests/rust/tools/dylint/macro_matching.rs:80:62
       |
    80 | ...ng, u8>) -> bool { matches!(value, itertools::Either::Left(_)) || matches!(value, itertools::Either::Right(1)) }
       |                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: test `itertools::Either::Left(_) | itertools::Either::Right(1)` in one `matches!` invocation
    ");
}

fn display_format(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `format!` wraps one `Display` value without formatting options
      --> tools/dylint/examples/macro_formatting.rs:47:70
       |
    47 | fn displayed(value: &Reading) -> impl IntoIterator<Item = String> { [format!("{value}"), format!("{0}", value), format!("{}", value)...
       |                                                                      ^^^^^^^^^^^^^^^^^^
       |
       = help: call the `ToString::to_string` trait method on the formatted value
       = note: `#[warn(display_format)]` on by default

    warning: `format!` wraps one `Display` value without formatting options
      --> tools/dylint/examples/macro_formatting.rs:47:90
       |
    47 | fn displayed(value: &Reading) -> impl IntoIterator<Item = String> { [format!("{value}"), format!("{0}", value), format!("{}", value)...
       |                                                                                          ^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call the `ToString::to_string` trait method on the formatted value

    warning: `format!` wraps one `Display` value without formatting options
      --> tools/dylint/examples/macro_formatting.rs:47:113
       |
    47 | ...String> { [format!("{value}"), format!("{0}", value), format!("{}", value), render!("{value}"), ToString::to_string(value)] }
       |                                                          ^^^^^^^^^^^^^^^^^^^^
       |
       = help: call the `ToString::to_string` trait method on the formatted value

    warning: `format!` wraps one `Display` value without formatting options
      --> tools/dylint/examples/macro_formatting.rs:47:135
       |
    47 | ...value}"), format!("{0}", value), format!("{}", value), render!("{value}"), ToString::to_string(value)] }
       |                                                           ^^^^^^^^^^^^^^^^^^
       |
       = help: call the `ToString::to_string` trait method on the formatted value

    warning: `format!` wraps one `Display` value without formatting options
      --> tools/dylint/examples/macro_formatting.rs:65:63
       |
    65 | fn literal_arguments() -> impl IntoIterator<Item = String> { [format!("{}", 42_u32), format!("{}", format_args!("{}", 7_u8))] }
       |                                                               ^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call the `ToString::to_string` trait method on the formatted value

    warning: `format!` wraps one `Display` value without formatting options
      --> tools/dylint/examples/macro_formatting.rs:65:86
       |
    65 | fn literal_arguments() -> impl IntoIterator<Item = String> { [format!("{}", 42_u32), format!("{}", format_args!("{}", 7_u8))] }
       |                                                                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call the `ToString::to_string` trait method on the formatted value

    warning: `format!` wraps one `Display` value without formatting options
      --> tools/dylint/examples/manual_fmt_from_fn.rs:97:18
       |
    97 |     let joined = format!("{}", Joined(&values));
       |                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call the `ToString::to_string` trait method on the formatted value
       = note: `#[warn(display_format)]` on by default

    warning: `format!` wraps one `Display` value without formatting options
       --> tools/dylint/examples/stringified_error.rs:188:40
        |
    188 | fn kind(error: &io::Error) -> String { format!("{}", error.kind()) }
        |                                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: call the `ToString::to_string` trait method on the formatted value
        = note: `#[warn(display_format)]` on by default

    warning: `format!` wraps one `Display` value without formatting options
      --> tools/dylint/examples/stringified_uuid.rs:46:50
       |
    46 | fn counted(id: Uuid) -> Session { Session { key: format!("{}", id.as_bytes().len()) } }
       |                                                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call the `ToString::to_string` trait method on the formatted value
       = note: `#[warn(display_format)]` on by default
    "#);
}

fn expect_used_in_macro(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `std::result::Result::expect` in a local macro expansion panics on `Err`
      --> tools/dylint/examples/restriction_in_macro.rs:27:16
       |
    25 | macro_rules! required {
    26 |     ($value:expr) => {
    27 |         $value.expect("value is present")
       |                ^^^^^^^^^^^^^^^^^^^^^^^^^^
    ...
    59 | fn expected(value: Result<u32, ParseIntError>) -> u32 { required!(value) }
       |                                                         ---------------- in this macro invocation
       |
       = help: return the error with `?`
       = note: `#[warn(expect_used_in_macro)]` on by default

    warning: `std::result::Result::expect` in a local macro expansion panics on `Err`
      --> tools/dylint/examples/restriction_in_macro.rs:21:9
       |
    19 | macro_rules! required_qualified {
    20 |     ($value:expr) => {
    21 |         Result::expect($value, "value is present")
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    ...
    63 | fn expected_qualified(value: Result<u32, ParseIntError>) -> u32 { required_qualified!(value) }
       |                                                                   -------------------------- in this macro invocation
       |
       = help: return the error with `?`
    "#);
}

fn literal_write(found: &str) {
    insta::assert_snapshot!(found, @r##"
    warning: formatter writes a literal through formatting arguments
      --> tools/dylint/examples/macro_formatting.rs:53:9
       |
    53 |         write!(formatter, "plain")?;
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    help: write the literal directly
      --> tools/dylint/examples/macro_formatting.rs:53:9
       |
    53 |         write!(formatter, "plain")?;
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^
       = note: `#[warn(literal_write)]` on by default

    warning: formatter writes a literal through formatting arguments
      --> tools/dylint/examples/macro_formatting.rs:54:9
       |
    54 |         write!(formatter, "{{escaped}}\n")?;
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    help: write the literal directly
      --> tools/dylint/examples/macro_formatting.rs:54:9
       |
    54 |         write!(formatter, "{{escaped}}\n")?;
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    warning: formatter writes a literal through formatting arguments
      --> tools/dylint/examples/macro_formatting.rs:55:9
       |
    55 |         write!(formatter, r#"raw "{{value}}""#)?;
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    help: write the literal directly
      --> tools/dylint/examples/macro_formatting.rs:55:9
       |
    55 |         write!(formatter, r#"raw "{{value}}""#)?;
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    warning: formatter writes a literal through formatting arguments
      --> tools/dylint/examples/macro_formatting.rs:56:9
       |
    56 |         writeln!(formatter, "row")?;
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    help: write the literal directly
      --> tools/dylint/examples/macro_formatting.rs:56:9
       |
    56 |         writeln!(formatter, "row")?;
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^

    warning: formatter writes a literal through formatting arguments
      --> tools/dylint/examples/macro_formatting.rs:57:9
       |
    57 |         writeln!(formatter)?;
       |         ^^^^^^^^^^^^^^^^^^^
       |
    help: write the literal directly
      --> tools/dylint/examples/macro_formatting.rs:57:9
       |
    57 |         writeln!(formatter)?;
       |         ^^^^^^^^^^^^^^^^^^^

    warning: formatter writes a literal through formatting arguments
      --> tools/dylint/examples/macro_formatting.rs:58:9
       |
    58 |         put!(formatter, "alias")?;
       |         ^^^^^^^^^^^^^^^^^^^^^^^^
       |
    help: write the literal directly
      --> tools/dylint/examples/macro_formatting.rs:58:9
       |
    58 |         put!(formatter, "alias")?;
       |         ^^^^^^^^^^^^^^^^^^^^^^^^
    "##);
}

fn panic_in_macro(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `panic!` in a local macro expansion panics
      --> tools/dylint/examples/restriction_in_macro.rs:34:13
       |
    31 | macro_rules! ensured {
    ...
    34 |             panic!("broken")
       |             ^^^^^^^^^^^^^^^^
    ...
    65 | fn checked(value: u32) { ensured!(value > 1) }
       |                          ------------------- in this macro invocation
       |
       = help: return a typed error for a failure the input causes, or state the invariant in the types
       = note: `#[warn(panic_in_macro)]` on by default

    warning: `todo!` in a local macro expansion panics
      --> tools/dylint/examples/restriction_in_macro.rs:41:9
       |
    39 | macro_rules! pending {
    40 |     () => {
    41 |         todo!()
       |         ^^^^^^^
    ...
    67 | fn unfinished() -> u32 { pending!() }
       |                          ---------- in this macro invocation
       |
       = help: replace `todo!` with the implementation
    "#);
}

fn print_in_macro(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `println!` in a local macro expansion writes to stdout
      --> tools/dylint/examples/restriction_in_macro.rs:47:9
       |
    45 | macro_rules! said {
    46 |     ($($argument:tt)*) => {
    47 |         println!($($argument)*)
       |         ^^^^^^^^^^^^^^^^^^^^^^^
    ...
    69 | fn printed(value: u32) { said!("{value}") }
       |                          ---------------- in this macro invocation
       |
       = help: emit a `tracing` event in place of `println!`
       = note: `#[warn(print_in_macro)]` on by default
    "#);
}

fn renamed_function_params_in_macro(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `scaled!` names the `Scale::scale` parameter `factor` as `by`
      --> tools/dylint/examples/renamed_function_params_in_macro.rs:33:15
       |
    33 |         scale(by) => u32::wrapping_mul;
       |               ^^
       |
    note: expanded here
      --> tools/dylint/examples/renamed_function_params_in_macro.rs:32:5
       |
    32 | /     scaled! {
    33 | |         scale(by) => u32::wrapping_mul;
    34 | |         clamp(limit) => u32::min;
    35 | |     }
       | |_____^
       = help: rename `by` to `factor`
       = note: `#[warn(renamed_function_params_in_macro)]` on by default

    warning: `offset!` names the `Scale::shift` parameter `offset` as `delta`
      --> tools/dylint/examples/renamed_function_params_in_macro.rs:27:25
       |
    25 | macro_rules! offset {
    26 |     () => {
    27 |         fn shift(&self, delta: u32) -> u32 { self.0 + delta }
       |                         ^^^^^
    ...
    37 |     offset!();
       |     --------- in this macro invocation
       |
    note: expanded here
      --> tools/dylint/examples/renamed_function_params_in_macro.rs:37:5
       |
    37 |     offset!();
       |     ^^^^^^^^^
       = help: rename `delta` to `offset`
    ");
}

fn tokio_pin(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `tokio::pin!` duplicates standard pinning
      --> tools/dylint/../../tests/rust/tools/dylint/expansions.rs:11:5
       |
    11 |     tokio::pin!(future);
       |     ^^^^^^^^^^^^^^^^^^^
       |
       = help: bind pinned values with `core::pin::pin!`
       = note: `#[warn(tokio_pin)]` on by default

    warning: `tokio::pin!` duplicates standard pinning
      --> tools/dylint/examples/restriction_in_macro.rs:96:5
       |
    96 |     tokio::pin!(first, second);
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: bind pinned values with `core::pin::pin!`
       = note: `#[warn(tokio_pin)]` on by default

    warning: `tokio::pin!` duplicates standard pinning
       --> tools/dylint/examples/restriction_in_macro.rs:102:5
        |
    102 |     pinned!(value);
        |     ^^^^^^^^^^^^^^
        |
        = help: bind pinned values with `core::pin::pin!`

    warning: `tokio::pin!` duplicates standard pinning
       --> tools/dylint/examples/restriction_in_macro.rs:107:5
        |
    107 | /     tokio::pin! {
    108 | |         let outer = async {
    109 | |             let inner = future::ready(4);
    110 | |             pinned!(inner);
    ...   |
    113 | |         let other = future::ready(5);
    114 | |     }
        | |_____^
        |
        = help: bind pinned values with `core::pin::pin!`

    warning: `tokio::pin!` duplicates standard pinning
       --> tools/dylint/examples/restriction_in_macro.rs:110:13
        |
    110 |             pinned!(inner);
        |             ^^^^^^^^^^^^^^
        |
        = help: bind pinned values with `core::pin::pin!`
    ");
}

fn unwrap_used_in_macro(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `std::option::Option::unwrap` in a local macro expansion panics on `None`
      --> tools/dylint/examples/restriction_in_macro.rs:9:16
       |
     7 | macro_rules! taken {
     8 |     ($value:expr) => {
     9 |         $value.unwrap()
       |                ^^^^^^^^
    ...
    57 | fn unwrapped(value: Option<u32>) -> u32 { taken!(value) }
       |                                           ------------- in this macro invocation
       |
       = help: convert `None` into an error with `ok_or_else` and return it with `?`
       = note: `#[warn(unwrap_used_in_macro)]` on by default

    warning: `std::option::Option::unwrap` in a local macro expansion panics on `None`
      --> tools/dylint/examples/restriction_in_macro.rs:15:9
       |
    13 | macro_rules! taken_qualified {
    14 |     ($value:expr) => {
    15 |         Option::unwrap($value)
       |         ^^^^^^^^^^^^^^^^^^^^^^
    ...
    61 | fn unwrapped_qualified(value: Option<u32>) -> u32 { taken_qualified!(value) }
       |                                                     ----------------------- in this macro invocation
       |
       = help: convert `None` into an error with `ok_or_else` and return it with `?`
    ");
}

fn use_debug_in_macro(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `write!` in a local macro expansion formats through `Debug`
      --> tools/dylint/examples/restriction_in_macro.rs:85:9
       |
    83 | macro_rules! written {
    84 |     ($out:expr, $value:expr) => {
    85 |         write!($out, "{:?}", $value)
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    ...
    91 | fn writing(out: &mut String, value: u32) -> core::fmt::Result { written!(out, value) }
       |                                                                 -------------------- in this macro invocation
       |
       = help: format through `Display` with `{}`
       = note: `#[warn(use_debug_in_macro)]` on by default
    "#);
}

}
