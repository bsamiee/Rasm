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

fn single_reader(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `NONE` has one reader
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:41:1
       |
    41 | const NONE: u32 = 0;
       | ^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:43:62
       |
    43 | fn named_bound(count: NonZero<u32>) -> bool { count.get() == NONE }
       |                                                              ^^^^
       = help: move `NONE` into `named_bound` as a nested item
       = note: `#[warn(single_reader)]` on by default

    warning: `infallible` has one reader
       --> tools/dylint/examples/discarded_error.rs:15:1
        |
     15 | fn infallible(value: u32) -> Result<u32, Infallible> { Ok(value) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/discarded_error.rs:135:33
        |
    135 | fn certain(value: u32) -> u32 { infallible(value).unwrap_or_default() }
        |                                 ^^^^^^^^^^
        = help: move `infallible` into `certain` as a nested item
        = note: `#[warn(single_reader)]` on by default

    warning: `reopen` has one reader
       --> tools/dylint/examples/foreign_error_return.rs:72:5
        |
     72 |     fn reopen(self) -> io::Result<u16> { self.open() }
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/foreign_error_return.rs:125:9
        |
    125 |         Disk.reopen(),
        |         ^^^^^^^^^^^^^
        = help: move `reopen` into `main` as a nested item
        = note: `#[warn(single_reader)]` on by default

    warning: `hidden` has one reader
       --> tools/dylint/examples/foreign_error_return.rs:103:1
        |
    103 | fn hidden(text: &str) -> Result<u16, ParseIntError> { text.trim().parse() }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/foreign_error_return.rs:130:17
        |
    130 |         generic(hidden(text)),
        |                 ^^^^^^
        = help: move `hidden` into `main` as a nested item

    warning: `new` has one reader
      --> tools/dylint/examples/manual_default_field_values.rs:91:5
       |
    91 |     fn new(attempts: u8) -> Self { Self { attempts } }
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/examples/manual_default_field_values.rs:95:28
       |
    95 |     fn default() -> Self { Self::new(2) }
       |                            ^^^^^^^^^
       = help: move `new` into `default` as a nested item
       = note: `#[warn(single_reader)]` on by default

    warning: `passed` has one reader
      --> tools/dylint/examples/partition_by_result.rs:15:1
       |
    15 | const fn passed(result: &Result<u8, ParseIntError>) -> bool { result.is_ok() }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/examples/partition_by_result.rs:19:151
       |
    19 | ...<Result<u8, ParseIntError>>) { results.into_iter().partition(passed) }
       |                                                                 ^^^^^^
       = help: move `passed` into `split` as a nested item
       = note: `#[warn(single_reader)]` on by default

    warning: `small` has one reader
      --> tools/dylint/examples/partition_by_result.rs:21:1
       |
    21 | fn small(result: &Result<u8, ParseIntError>) -> bool { result.is_small() }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/examples/partition_by_result.rs:23:152
       |
    23 | ...c<Result<u8, ParseIntError>>) { results.into_iter().partition(small) }
       |                                                                  ^^^^^
       = help: move `small` into `ranked` as a nested item

    warning: `large` has one reader
      --> tools/dylint/examples/partition_by_result.rs:17:1
       |
    17 | const fn large(value: &u8) -> bool { *value > 9 }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/examples/partition_by_result.rs:25:80
       |
    25 | fn sized(values: Vec<u8>) -> (Vec<u8>, Vec<u8>) { values.into_iter().partition(large) }
       |                                                                                ^^^^^
       = help: move `large` into `sized` as a nested item

    warning: `run` has one reader
       --> tools/dylint/examples/single_impl_trait.rs:168:1
        |
    168 | / fn run<H>(handler: &H) -> u32
    169 | | where H: Handler {
        | |________________^
        |
    note: read here
       --> tools/dylint/examples/single_impl_trait.rs:173:19
        |
    173 | fn ran() -> u32 { run(&|key: u32| key.strict_add(1)) }
        |                   ^^^
        = help: move `run` into `ran` as a nested item
        = note: `#[warn(single_reader)]` on by default

    warning: `bound` has one reader
      --> tools/dylint/examples/single_reader.rs:53:5
       |
    53 |     fn bound(&self) -> usize { self.width.strict_mul(SHARED) }
       |     ^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/examples/single_reader.rs:51:49
       |
    51 |     fn clamped(&self) -> usize { self.width.min(self.bound()) }
       |                                                 ^^^^^^^^^^^^
       = help: move `bound` into `clamped` as a nested item
       = note: `#[warn(single_reader)]` on by default

    warning: `MULTIPLE` has one reader
      --> tools/dylint/examples/single_reader.rs:48:5
       |
    48 |     const MULTIPLE: usize = 3;
       |     ^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/examples/single_reader.rs:57:56
       |
    57 |     fn tripled(&self) -> usize { self.width.strict_mul(Self::MULTIPLE).strict_add(Self::OFFSET) }
       |                                                        ^^^^^^^^^^^^^^
       = help: move `MULTIPLE` into `tripled` as a nested item

    warning: `digits` has one reader
       --> tools/dylint/examples/single_reader.rs:104:1
        |
    104 | fn digits(text: &str) -> usize { text.chars().filter(char::is_ascii_digit).count() }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:106:34
        |
    106 | fn parsed(text: &str) -> usize { digits(text).min(LIMIT) }
        |                                  ^^^^^^
        = help: move `digits` into `parsed` as a nested item

    warning: `LIMIT` has one reader
       --> tools/dylint/examples/single_reader.rs:8:1
        |
      8 | const LIMIT: usize = 8;
        | ^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:106:51
        |
    106 | fn parsed(text: &str) -> usize { digits(text).min(LIMIT) }
        |                                                   ^^^^^
        = help: move `LIMIT` into `parsed` as a nested item

    warning: `countdown` has one reader
       --> tools/dylint/examples/single_reader.rs:108:1
        |
    108 | fn countdown(steps: usize) -> usize { if steps == 0 { 0 } else { countdown(steps.strict_sub(1)) } }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:110:37
        |
    110 | fn started(steps: usize) -> usize { countdown(steps).strict_add(1) }
        |                                     ^^^^^^^^^
        = help: move `countdown` into `started` as a nested item

    warning: `BANNER` has one reader
       --> tools/dylint/examples/single_reader.rs:12:1
        |
     12 | static BANNER: &str = "ports";
        | ^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:118:35
        |
    118 | fn titled() -> String { format!("{BANNER}:") }
        |                                   ^^^^^^
        = help: move `BANNER` into `titled` as a nested item

    warning: `expanded` has one reader
       --> tools/dylint/examples/single_reader.rs:122:1
        |
    122 | fn expanded(text: &str) -> usize { text.chars().count() }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    123 |
    124 | macro_rules! measured {
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:126:9
        |
    126 |         expanded($text)
        |         ^^^^^^^^
    ...
    130 | fn measuring(text: &str) -> usize { measured!(text) }
        |                                     --------------- in this macro invocation
        = help: move `expanded` into `measuring` as a nested item

    warning: `scaled` has one reader
       --> tools/dylint/examples/single_reader.rs:132:1
        |
    132 | fn scaled(width: usize) -> usize { width.strict_add(1) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:140:18
        |
    140 | reader!(widened, scaled);
        |                  ^^^^^^
        = help: move `scaled` into `widened` as a nested item

    warning: `exported` has one reader
       --> tools/dylint/examples/single_reader.rs:142:1
        |
    142 | pub fn exported(text: &str) -> usize { text.len().strict_add(SHARED) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:144:35
        |
    144 | fn relayed(text: &str) -> usize { exported(text).strict_mul(2) }
        |                                   ^^^^^^^^
        = help: move `exported` into `relayed` as a nested item

    warning: `DEPTH` has one reader
       --> tools/dylint/examples/single_reader.rs:14:1
        |
     14 | const DEPTH: usize = 2;
        | ^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:164:31
        |
    164 | fn padded() -> usize { [1_u8; DEPTH + 1].len() }
        |                               ^^^^^
        = help: move `DEPTH` into `padded` as a nested item

    warning: `seed` has one reader
       --> tools/dylint/examples/single_reader.rs:168:1
        |
    168 | const fn seed() -> u8 { 3 }
        | ^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:174:18
        |
    174 | const SEED: u8 = seed();
        |                  ^^^^
        = help: move `seed` into `SEED` as a nested item

    warning: `PORTS` has one reader
       --> tools/dylint/examples/single_reader.rs:20:1
        |
     20 | static PORTS: LazyLock<Vec<u16>> = LazyLock::new(|| (1..=4).map(|step: u16| step.strict_mul(80)).collect());
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:180:40
        |
    180 | fn port(index: usize) -> Option<u16> { PORTS.get(index).copied() }
        |                                        ^^^^^
        = help: move `PORTS` into `port` as a nested item

    warning: `graded` has one reader
       --> tools/dylint/examples/single_reader.rs:188:1
        |
    188 | fn graded(score: u8) -> u8 { if score > 4 { if score > 7 { 2 } else { 1 } } else { 0 } }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:192:29
        |
    192 | fn grade(score: u8) -> u8 { graded(score).strict_add(ranked(score)) }
        |                             ^^^^^^
        = help: move `graded` into `grade` as a nested item

    warning: `floored` has one reader
       --> tools/dylint/examples/single_reader.rs:194:1
        |
    194 | fn floored(width: usize) -> usize { if width.is_power_of_two() { width } else { width.next_power_of_two() } }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:197:43
        |
    197 |     fn unwrapped(width: usize) -> usize { floored(width).strict_mul(2) }
        |                                           ^^^^^^^
        = help: move `floored` into `unwrapped` as a nested item

    warning: `halved` has one reader
       --> tools/dylint/examples/single_reader.rs:162:1
        |
    162 | fn halved(width: usize) -> usize { width.strict_div(2) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:215:42
        |
    215 |     fn narrowed(width: usize) -> usize { halved(width).strict_add(1) }
        |                                          ^^^^^^
        = help: move `halved` into `narrowed` as a nested item
    "#);
}

// --- [COMPOSITION] ---------------------------------------------------------------------

snapshots!(needless_import_rename, single_reader);
