// --- [OPERATIONS] ----------------------------------------------------------------------

fn needless_import_rename(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `Cell` renames `UnsafeCell`, which collides with no other item in scope
     --> tools/dylint/examples/needless_import_rename.rs:2:5
      |
    2 |     cell::UnsafeCell as Cell,
      |     ^^^^^^^^^^^^^^^^^^^^^^^^
      |
      = help: import `UnsafeCell` under its own name
      = note: `#[warn(needless_import_rename)]` on by default

    warning: `Flipped` renames `Reverse`, which collides with no other item in scope
     --> tools/dylint/examples/needless_import_rename.rs:3:11
      |
    3 |     cmp::{Reverse as Flipped, *},
      |           ^^^^^^^^^^^^^^^^^^
      |
      = help: import `Reverse` under its own name

    warning: `format` renames `fmt`, which collides with no other item in scope
     --> tools/dylint/examples/needless_import_rename.rs:4:11
      |
    4 |     fmt::{self as format, Error, Write as _},
      |           ^^^^^^^^^^^^^^
      |
      = help: import `fmt` under its own name

    warning: `taken` renames `take`, which collides with no other item in scope
     --> tools/dylint/examples/needless_import_rename.rs:6:5
      |
    6 |     mem::take as taken,
      |     ^^^^^^^^^^^^^^^^^^
      |
      = help: import `take` under its own name

    warning: `Map` renames `BTreeMap`, which collides with no other item in scope
      --> tools/dylint/examples/needless_import_rename.rs:14:5
       |
    14 |     collections::BTreeMap as Map,
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: import `BTreeMap` under its own name

    warning: `Shared` renames `Arc`, which collides with no other item in scope
      --> tools/dylint/examples/needless_import_rename.rs:18:5
       |
    18 |     sync::Arc as Shared,
       |     ^^^^^^^^^^^^^^^^^^^
       |
       = help: import `Arc` under its own name

    warning: `Span` renames `Duration`, which collides with no other item in scope
      --> tools/dylint/examples/needless_import_rename.rs:54:9
       |
    54 |     use core::time::Duration as Span;
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: import `Duration` under its own name
    ");
}

fn single_caller(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `NONE` is used only by `named_bound`
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:41:1
       |
    41 | const NONE: u32 = 0;
       | ^^^^^^^^^^^^^^^
       |
    note: used here
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:43:62
       |
    43 | fn named_bound(count: NonZero<u32>) -> bool { count.get() == NONE }
       |                                                              ^^^^
       = help: move `NONE` into `named_bound` as a nested item
       = note: `#[warn(single_caller)]` on by default

    warning: `infallible` is used only by `certain`
       --> tools/dylint/examples/discarded_error.rs:15:1
        |
     15 | fn infallible(value: u32) -> Result<u32, Infallible> { Ok(value) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: used here
       --> tools/dylint/examples/discarded_error.rs:135:33
        |
    135 | fn certain(value: u32) -> u32 { infallible(value).unwrap_or_default() }
        |                                 ^^^^^^^^^^
        = help: move `infallible` into `certain` as a nested item
        = note: `#[warn(single_caller)]` on by default

    warning: `reopen` is used only by `main`
       --> tools/dylint/examples/foreign_error_return.rs:72:5
        |
     72 |     fn reopen(self) -> io::Result<u16> { self.open() }
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: used here
       --> tools/dylint/examples/foreign_error_return.rs:125:9
        |
    125 |         Disk.reopen(),
        |         ^^^^^^^^^^^^^
        = help: move `reopen` into `main` as a nested item
        = note: `#[warn(single_caller)]` on by default

    warning: `hidden` is used only by `main`
       --> tools/dylint/examples/foreign_error_return.rs:103:1
        |
    103 | fn hidden(text: &str) -> Result<u16, ParseIntError> { text.trim().parse() }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: used here
       --> tools/dylint/examples/foreign_error_return.rs:130:17
        |
    130 |         generic(hidden(text)),
        |                 ^^^^^^
        = help: move `hidden` into `main` as a nested item

    warning: `new` is used only by `default`
      --> tools/dylint/examples/manual_default_field_values.rs:51:5
       |
    51 |     fn new() -> Self { Self { depth: 0, name: String::new(), enabled: false, label: "", parent: None } }
       |     ^^^^^^^^^^^^^^^^
       |
    note: used here
      --> tools/dylint/examples/manual_default_field_values.rs:55:28
       |
    55 |     fn default() -> Self { Self::new() }
       |                            ^^^^^^^^^
       = help: move `new` into `default` as a nested item
       = note: `#[warn(single_caller)]` on by default

    warning: `new` is used only by `default`
      --> tools/dylint/examples/manual_default_field_values.rs:68:5
       |
    68 |     fn new() -> Self { Self { depth: 4, name: String::new(), enabled: false, label: "", parent: None } }
       |     ^^^^^^^^^^^^^^^^
       |
    note: used here
      --> tools/dylint/examples/manual_default_field_values.rs:72:28
       |
    72 |     fn default() -> Self { Self::new() }
       |                            ^^^^^^^^^
       = help: move `new` into `default` as a nested item

    warning: `new` is used only by `default`
      --> tools/dylint/examples/manual_default_field_values.rs:91:5
       |
    91 |     fn new(attempts: u8) -> Self { Self { attempts } }
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: used here
      --> tools/dylint/examples/manual_default_field_values.rs:95:28
       |
    95 |     fn default() -> Self { Self::new(2) }
       |                            ^^^^^^^^^
       = help: move `new` into `default` as a nested item

    warning: `passed` is used only by `split`
      --> tools/dylint/examples/partition_by_result.rs:15:1
       |
    15 | const fn passed(result: &Result<u8, ParseIntError>) -> bool { result.is_ok() }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: used here
      --> tools/dylint/examples/partition_by_result.rs:19:151
       |
    19 | ...<Result<u8, ParseIntError>>) { results.into_iter().partition(passed) }
       |                                                                 ^^^^^^
       = help: move `passed` into `split` as a nested item
       = note: `#[warn(single_caller)]` on by default

    warning: `small` is used only by `ranked`
      --> tools/dylint/examples/partition_by_result.rs:21:1
       |
    21 | fn small(result: &Result<u8, ParseIntError>) -> bool { result.is_small() }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: used here
      --> tools/dylint/examples/partition_by_result.rs:23:152
       |
    23 | ...c<Result<u8, ParseIntError>>) { results.into_iter().partition(small) }
       |                                                                  ^^^^^
       = help: move `small` into `ranked` as a nested item

    warning: `large` is used only by `sized`
      --> tools/dylint/examples/partition_by_result.rs:17:1
       |
    17 | const fn large(value: &u8) -> bool { *value > 9 }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: used here
      --> tools/dylint/examples/partition_by_result.rs:25:80
       |
    25 | fn sized(values: Vec<u8>) -> (Vec<u8>, Vec<u8>) { values.into_iter().partition(large) }
       |                                                                                ^^^^^
       = help: move `large` into `sized` as a nested item

    warning: `bound` is used only by `clamped`
      --> tools/dylint/examples/single_caller.rs:42:5
       |
    42 |     fn bound(&self) -> usize { self.width.strict_mul(SHARED) }
       |     ^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: used here
      --> tools/dylint/examples/single_caller.rs:40:49
       |
    40 |     fn clamped(&self) -> usize { self.width.min(self.bound()) }
       |                                                 ^^^^^^^^^^^^
       = help: move `bound` into `clamped` as a nested item
       = note: `#[warn(single_caller)]` on by default

    warning: `digits` is used only by `parsed`
      --> tools/dylint/examples/single_caller.rs:63:1
       |
    63 | fn digits(text: &str) -> usize { text.chars().filter(char::is_ascii_digit).count() }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: used here
      --> tools/dylint/examples/single_caller.rs:65:34
       |
    65 | fn parsed(text: &str) -> usize { digits(text).min(LIMIT) }
       |                                  ^^^^^^
       = help: move `digits` into `parsed` as a nested item

    warning: `LIMIT` is used only by `parsed`
      --> tools/dylint/examples/single_caller.rs:7:1
       |
     7 | const LIMIT: usize = 8;
       | ^^^^^^^^^^^^^^^^^^
       |
    note: used here
      --> tools/dylint/examples/single_caller.rs:65:51
       |
    65 | fn parsed(text: &str) -> usize { digits(text).min(LIMIT) }
       |                                                   ^^^^^
       = help: move `LIMIT` into `parsed` as a nested item

    warning: `countdown` is used only by `started`
      --> tools/dylint/examples/single_caller.rs:67:1
       |
    67 | fn countdown(steps: usize) -> usize { if steps == 0 { 0 } else { countdown(steps.strict_sub(1)) } }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: used here
      --> tools/dylint/examples/single_caller.rs:69:37
       |
    69 | fn started(steps: usize) -> usize { countdown(steps) }
       |                                     ^^^^^^^^^
       = help: move `countdown` into `started` as a nested item

    warning: `BANNER` is used only by `titled`
      --> tools/dylint/examples/single_caller.rs:11:1
       |
    11 | static BANNER: &str = "ports";
       | ^^^^^^^^^^^^^^^^^^^
       |
    note: used here
      --> tools/dylint/examples/single_caller.rs:77:35
       |
    77 | fn titled() -> String { format!("{BANNER}:") }
       |                                   ^^^^^^
       = help: move `BANNER` into `titled` as a nested item

    warning: `expanded` is used only by `measuring`
      --> tools/dylint/examples/single_caller.rs:81:1
       |
    81 | fn expanded(text: &str) -> usize { text.chars().count() }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    82 |
    83 | macro_rules! measured {
       |
    note: used here
      --> tools/dylint/examples/single_caller.rs:85:9
       |
    85 |         expanded($text)
       |         ^^^^^^^^
    ...
    89 | fn measuring(text: &str) -> usize { measured!(text) }
       |                                     --------------- in this macro invocation
       = help: move `expanded` into `measuring` as a nested item

    warning: `scaled` is used only by `widened`
      --> tools/dylint/examples/single_caller.rs:91:1
       |
    91 | fn scaled(width: usize) -> usize { width.strict_add(1) }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: used here
      --> tools/dylint/examples/single_caller.rs:99:18
       |
    99 | reader!(widened, scaled);
       |                  ^^^^^^
       = help: move `scaled` into `widened` as a nested item

    warning: `exported` is used only by `relayed`
       --> tools/dylint/examples/single_caller.rs:101:1
        |
    101 | pub fn exported(text: &str) -> usize { text.len().strict_add(SHARED) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: used here
       --> tools/dylint/examples/single_caller.rs:103:35
        |
    103 | fn relayed(text: &str) -> usize { exported(text).strict_mul(2) }
        |                                   ^^^^^^^^
        = help: move `exported` into `relayed` as a nested item

    warning: `seed` is used only by `SEED`
       --> tools/dylint/examples/single_caller.rs:127:1
        |
    127 | const fn seed() -> u8 { 3 }
        | ^^^^^^^^^^^^^^^^^^^^^
        |
    note: used here
       --> tools/dylint/examples/single_caller.rs:133:18
        |
    133 | const SEED: u8 = seed();
        |                  ^^^^
        = help: move `seed` into `SEED` as a nested item

    warning: `halved` is used only by `narrowed`
       --> tools/dylint/examples/single_caller.rs:121:1
        |
    121 | fn halved(width: usize) -> usize { width.strict_div(2) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: used here
       --> tools/dylint/examples/single_caller.rs:141:42
        |
    141 |     fn narrowed(width: usize) -> usize { halved(width) }
        |                                          ^^^^^^
        = help: move `halved` into `narrowed` as a nested item
    "#);
}

// --- [COMPOSITION] ---------------------------------------------------------------------

snapshots!(needless_import_rename, single_caller);
