// --- [OPERATIONS] ----------------------------------------------------------------------

fn expect_used_in_macro(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `std::result::Result::expect` in a local macro expansion panics on `Err`
      --> tools/dylint/examples/restriction_in_macro.rs:13:16
       |
    11 | macro_rules! required {
    12 |     ($value:expr) => {
    13 |         $value.expect("value is present")
       |                ^^^^^^^^^^^^^^^^^^^^^^^^^^
    ...
    45 | fn expected(value: Result<u32, ParseIntError>) -> u32 { required!(value) }
       |                                                         ---------------- in this macro invocation
       |
       = help: return the error with `?`
       = note: `#[warn(expect_used_in_macro)]` on by default
    "#);
}

fn map_err_ignore_in_macro(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `map_err(|_| ..)` in a local macro expansion discards `ParseIntError`
       --> tools/dylint/examples/restriction_in_macro.rs:89:16
        |
     87 | macro_rules! ignored {
     88 |     ($value:expr) => {
     89 |         $value.map_err(|_| Refused)
        |                ^^^^^^^^^^^^^^^^^^^^
    ...
    111 | fn ignoring(value: Result<u32, ParseIntError>) -> Result<u32, Refused> { ignored!(value) }
        |                                                                          --------------- in this macro invocation
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds
        = note: `#[warn(map_err_ignore_in_macro)]` on by default
    ");
}

fn panic_in_macro(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `panic!` in a local macro expansion panics
      --> tools/dylint/examples/restriction_in_macro.rs:20:13
       |
    17 | macro_rules! ensured {
    ...
    20 |             panic!("broken")
       |             ^^^^^^^^^^^^^^^^
    ...
    47 | fn checked(value: u32) { ensured!(value > 1) }
       |                          ------------------- in this macro invocation
       |
       = help: return a typed error for a failure the input causes, or state the invariant in the types
       = note: `#[warn(panic_in_macro)]` on by default

    warning: `todo!` in a local macro expansion panics
      --> tools/dylint/examples/restriction_in_macro.rs:27:9
       |
    25 | macro_rules! pending {
    26 |     () => {
    27 |         todo!()
       |         ^^^^^^^
    ...
    49 | fn unfinished() -> u32 { pending!() }
       |                          ---------- in this macro invocation
       |
       = help: replace `todo!` with the implementation
    "#);
}

fn print_in_macro(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `println!` in a local macro expansion writes to stdout
      --> tools/dylint/examples/restriction_in_macro.rs:33:9
       |
    31 | macro_rules! said {
    32 |     ($($argument:tt)*) => {
    33 |         println!($($argument)*)
       |         ^^^^^^^^^^^^^^^^^^^^^^^
    ...
    51 | fn printed(value: u32) { said!("{value}") }
       |                          ---------------- in this macro invocation
       |
       = help: emit a `tracing` event in place of `println!`
       = note: `#[warn(print_in_macro)]` on by default
    "#);
}

fn redundant_closure_call_in_macro(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `applied!` calls its closure argument in its expansion
      --> tools/dylint/examples/redundant_closure_call_in_macro.rs:34:25
       |
     5 | macro_rules! applied {
    ...
    34 |     black_box([applied!(|value: u32| value + 1, 1), applied!(increment, 2), applied!(handler.apply, 3), bound!(|value| value + 1, 4)...
       |                         ^^^^^^^^^^^^^^^^^^^^^^
       |
    note: called here
      --> tools/dylint/examples/redundant_closure_call_in_macro.rs:7:9
       |
     7 |         ($transform)($value)
       |         ^^^^^^^^^^^^^^^^^^^^
    ...
    34 |     black_box([applied!(|value: u32| value + 1, 1), applied!(increment, 2), applied!(handler.apply, 3), bound!(|value| value + 1, 4)...
       |                ----------------------------------- in this macro invocation
       = help: match the closure as `|$parameter:ident $(: $type:ty)?| $body:expr` and expand to `{ let $parameter $(: $type)? = <argument>; $body }`
       = note: `#[warn(redundant_closure_call_in_macro)]` on by default

    warning: `doubled!` calls the closure it declares in its expansion
      --> tools/dylint/examples/redundant_closure_call_in_macro.rs:20:9
       |
    18 | ..._rules! doubled {
    19 | ...$value:expr) => {
    20 | ...   (|value: u32| value * 2)($value)
       |       ^^^^^^^^^^^^^^^^^^^^^^^^
    ...
    34 | ...lack_box([applied!(|value: u32| value + 1, 1), applied!(increment, 2), applied!(handler.apply, 3), bound!(|value| value + 1, 4), doubled!(5), (...
       |                                                                                                                                     ----------- in this macro invocation
       |
       = help: replace the call with `{ let <parameter> = <argument>; <body> }`
    ");
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

fn unwrap_used_in_macro(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `std::option::Option::unwrap` in a local macro expansion panics on `None`
      --> tools/dylint/examples/restriction_in_macro.rs:7:16
       |
     5 | macro_rules! taken {
     6 |     ($value:expr) => {
     7 |         $value.unwrap()
       |                ^^^^^^^^
    ...
    43 | fn unwrapped(value: Option<u32>) -> u32 { taken!(value) }
       |                                           ------------- in this macro invocation
       |
       = help: convert `None` into an error with `ok_or_else` and return it with `?`
       = note: `#[warn(unwrap_used_in_macro)]` on by default
    ");
}

fn use_debug_in_macro(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `write!` in a local macro expansion formats through `Debug`
       --> tools/dylint/examples/restriction_in_macro.rs:83:9
        |
     81 | macro_rules! written {
     82 |     ($out:expr, $value:expr) => {
     83 |         write!($out, "{:?}", $value)
        |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    ...
    109 | fn writing(out: &mut String, value: u32) -> core::fmt::Result { written!(out, value) }
        |                                                                 -------------------- in this macro invocation
        |
        = help: format through `Display` with `{}`
        = note: `#[warn(use_debug_in_macro)]` on by default
    "#);
}

// --- [COMPOSITION] ---------------------------------------------------------------------

snapshots!(expect_used_in_macro, map_err_ignore_in_macro, panic_in_macro, print_in_macro, redundant_closure_call_in_macro, renamed_function_params_in_macro, unwrap_used_in_macro, use_debug_in_macro);
