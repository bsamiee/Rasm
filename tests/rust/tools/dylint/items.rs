// --- [OPERATIONS] ----------------------------------------------------------------------

fn needless_import_rename(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `UnsafeCell` renamed to `Cell` with no other `UnsafeCell` in scope
     --> tools/dylint/examples/needless_import_rename.rs:2:5
      |
    2 |     cell::UnsafeCell as Cell,
      |     ^^^^^^^^^^^^^^^^^^^^^^^^
      |
      = help: remove `as Cell` and name `UnsafeCell` at each use
      = note: `#[warn(needless_import_rename)]` on by default

    warning: `Reverse` renamed to `Flipped` with no other `Reverse` in scope
     --> tools/dylint/examples/needless_import_rename.rs:3:11
      |
    3 |     cmp::{Reverse as Flipped, *},
      |           ^^^^^^^^^^^^^^^^^^
      |
      = help: remove `as Flipped` and name `Reverse` at each use

    warning: `fmt` renamed to `format` with no other `fmt` in scope
     --> tools/dylint/examples/needless_import_rename.rs:4:11
      |
    4 |     fmt::{self as format, Error, Write as _},
      |           ^^^^^^^^^^^^^^
      |
      = help: remove `as format` and name `fmt` at each use

    warning: `take` renamed to `taken` with no other `take` in scope
     --> tools/dylint/examples/needless_import_rename.rs:6:5
      |
    6 |     mem::take as taken,
      |     ^^^^^^^^^^^^^^^^^^
      |
      = help: remove `as taken` and name `take` at each use

    warning: `BTreeMap` renamed to `Map` with no other `BTreeMap` in scope
      --> tools/dylint/examples/needless_import_rename.rs:14:5
       |
    14 |     collections::BTreeMap as Map,
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: remove `as Map` and name `BTreeMap` at each use

    warning: `Arc` renamed to `Shared` with no other `Arc` in scope
      --> tools/dylint/examples/needless_import_rename.rs:18:5
       |
    18 |     sync::Arc as Shared,
       |     ^^^^^^^^^^^^^^^^^^^
       |
       = help: remove `as Shared` and name `Arc` at each use

    warning: `Duration` renamed to `Span` with no other `Duration` in scope
      --> tools/dylint/examples/needless_import_rename.rs:54:9
       |
    54 |     use core::time::Duration as Span;
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: remove `as Span` and name `Duration` at each use
    ");
}

fn single_reader(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `NONE` has one reader, `named_bound`
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

    warning: `infallible` has one reader, `certain`
       --> tools/dylint/examples/discarded_error.rs:21:1
        |
     21 | fn infallible(value: u32) -> Result<u32, Infallible> { Ok(value) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/discarded_error.rs:141:33
        |
    141 | fn certain(value: u32) -> u32 { infallible(value).unwrap_or_default() }
        |                                 ^^^^^^^^^^
        = help: move `infallible` into `certain` as a closure
        = note: `#[warn(single_reader)]` on by default

    warning: `reopen` has one reader, `main`
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
        = help: move `reopen` into `main` as a closure
        = note: `#[warn(single_reader)]` on by default

    warning: `hidden` has one reader, `main`
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
        = help: move `hidden` into `main` as a closure

    warning: `new` has one reader, `default`
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
       = help: move `new` into `default` as a closure
       = note: `#[warn(single_reader)]` on by default

    warning: `passed` has one reader, `named`
      --> tools/dylint/examples/partition_by_result.rs:20:1
       |
    20 | const fn passed(result: &Parsed) -> bool { result.is_ok() }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/examples/partition_by_result.rs:26:94
       |
    26 | fn named(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(passed) }
       |                                                                                              ^^^^^^
       = help: move `passed` into `named` as a nested `fn`
       = note: `#[warn(single_reader)]` on by default

    warning: `small` has one reader, `ranked`
      --> tools/dylint/examples/partition_by_result.rs:24:1
       |
    24 | fn small(result: &Parsed) -> bool { result.is_small() }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/examples/partition_by_result.rs:49:95
       |
    49 | fn ranked(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(small) }
       |                                                                                               ^^^^^
       = help: move `small` into `ranked` as a closure

    warning: `large` has one reader, `sized`
      --> tools/dylint/examples/partition_by_result.rs:22:1
       |
    22 | const fn large(value: &u8) -> bool { *value > 9 }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/examples/partition_by_result.rs:60:80
       |
    60 | fn sized(values: Vec<u8>) -> (Vec<u8>, Vec<u8>) { values.into_iter().partition(large) }
       |                                                                                ^^^^^
       = help: move `large` into `sized` as a nested `fn`

    warning: `run` has one reader, `ran`
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
        = help: move `run` into `ran` as a nested `fn`
        = note: `#[warn(single_reader)]` on by default

    warning: `bound` has one reader, `clamped`
      --> tools/dylint/examples/single_reader.rs:57:5
       |
    57 |     fn bound(&self) -> usize { self.width.strict_mul(SHARED) }
       |     ^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/examples/single_reader.rs:55:49
       |
    55 |     fn clamped(&self) -> usize { self.width.min(self.bound()) }
       |                                                 ^^^^^^^^^^^^
       = help: move `bound` into `clamped` as a closure
       = note: `#[warn(single_reader)]` on by default

    warning: `MULTIPLE` has one reader, `tripled`
      --> tools/dylint/examples/single_reader.rs:52:5
       |
    52 |     const MULTIPLE: usize = 3;
       |     ^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/examples/single_reader.rs:61:56
       |
    61 |     fn tripled(&self) -> usize { self.width.strict_mul(Self::MULTIPLE).strict_add(Self::OFFSET) }
       |                                                        ^^^^^^^^^^^^^^
       = help: move `MULTIPLE` into `tripled` as a nested item

    warning: `digits` has one reader, `parsed`
       --> tools/dylint/examples/single_reader.rs:108:1
        |
    108 | fn digits(text: &str) -> usize { text.chars().filter(char::is_ascii_digit).count() }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:110:34
        |
    110 | fn parsed(text: &str) -> usize { digits(text).min(LIMIT) }
        |                                  ^^^^^^
        = help: move `digits` into `parsed` as a closure

    warning: `LIMIT` has one reader, `parsed`
       --> tools/dylint/examples/single_reader.rs:8:1
        |
      8 | const LIMIT: usize = 8;
        | ^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:110:51
        |
    110 | fn parsed(text: &str) -> usize { digits(text).min(LIMIT) }
        |                                                   ^^^^^
        = help: move `LIMIT` into `parsed` as a nested item

    warning: `countdown` has one reader, `started`
       --> tools/dylint/examples/single_reader.rs:112:1
        |
    112 | fn countdown(steps: usize) -> usize { if steps == 0 { 0 } else { countdown(steps.strict_sub(1)) } }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:114:37
        |
    114 | fn started(steps: usize) -> usize { countdown(steps).strict_add(1) }
        |                                     ^^^^^^^^^
        = help: move `countdown` into `started` as a nested `fn`

    warning: `BANNER` has one reader, `titled`
       --> tools/dylint/examples/single_reader.rs:12:1
        |
     12 | static BANNER: &str = "ports";
        | ^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:122:35
        |
    122 | fn titled() -> String { format!("{BANNER}:") }
        |                                   ^^^^^^
        = help: move `BANNER` into `titled` as a nested item

    warning: `expanded` has one reader, `measuring`
       --> tools/dylint/examples/single_reader.rs:126:1
        |
    126 | fn expanded(text: &str) -> usize { text.chars().count() }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    127 |
    128 | macro_rules! measured {
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:130:9
        |
    130 |         expanded($text)
        |         ^^^^^^^^
    ...
    134 | fn measuring(text: &str) -> usize { measured!(text) }
        |                                     --------------- in this macro invocation
        = help: move `expanded` into `measuring` as a closure

    warning: `scaled` has one reader, `widened`
       --> tools/dylint/examples/single_reader.rs:136:1
        |
    136 | fn scaled(width: usize) -> usize { width.strict_add(1) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:144:18
        |
    144 | reader!(widened, scaled);
        |                  ^^^^^^
        = help: move `scaled` into `widened` as a closure

    warning: `exported` has one reader, `relayed`
       --> tools/dylint/examples/single_reader.rs:146:1
        |
    146 | pub fn exported(text: &str) -> usize { text.len().strict_add(SHARED) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:148:35
        |
    148 | fn relayed(text: &str) -> usize { exported(text).strict_mul(2) }
        |                                   ^^^^^^^^
        = help: move `exported` into `relayed` as a closure

    warning: `DEPTH` has one reader, `padded`
       --> tools/dylint/examples/single_reader.rs:14:1
        |
     14 | const DEPTH: usize = 2;
        | ^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:168:31
        |
    168 | fn padded() -> usize { [1_u8; DEPTH + 1].len() }
        |                               ^^^^^
        = help: move `DEPTH` into `padded` as a nested item

    warning: `seed` has one reader, `SEED`
       --> tools/dylint/examples/single_reader.rs:172:1
        |
    172 | const fn seed() -> u8 { 3 }
        | ^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:178:18
        |
    178 | const SEED: u8 = seed();
        |                  ^^^^
        = help: move `seed` into `SEED` as a nested `fn`

    warning: `PORTS` has one reader, `port`
       --> tools/dylint/examples/single_reader.rs:20:1
        |
     20 | static PORTS: LazyLock<Vec<u16>> = LazyLock::new(|| (1..=4).map(|step: u16| step.strict_mul(80)).collect());
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:184:40
        |
    184 | fn port(index: usize) -> Option<u16> { PORTS.get(index).copied() }
        |                                        ^^^^^
        = help: move `PORTS` into `port` as a nested item

    warning: `graded` has one reader, `grade`
       --> tools/dylint/examples/single_reader.rs:192:1
        |
    192 | fn graded(score: u8) -> u8 { if score > 4 { if score > 7 { 2 } else { 1 } } else { 0 } }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:196:29
        |
    196 | fn grade(score: u8) -> u8 { graded(score).strict_add(ranked(score)) }
        |                             ^^^^^^
        = help: move `graded` into `grade` as a closure

    warning: `floored` has one reader, `unwrapped`
       --> tools/dylint/examples/single_reader.rs:198:1
        |
    198 | fn floored(width: usize) -> usize { if width.is_power_of_two() { width } else { width.next_power_of_two() } }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:201:43
        |
    201 |     fn unwrapped(width: usize) -> usize { floored(width).strict_mul(2) }
        |                                           ^^^^^^^
        = help: move `floored` into `unwrapped` as a closure

    warning: `optional` has one reader, `doubled`
       --> tools/dylint/examples/single_reader.rs:217:1
        |
    217 | fn optional<T>(item: T) -> Option<T> { Some(item) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:219:51
        |
    219 | fn doubled<T: Copy>(item: T) -> [Option<T>; 2] { [optional(item), optional(item)] }
        |                                                   ^^^^^^^^        ^^^^^^^^
        = help: move `optional` into `doubled` as a closure

    warning: `listed` has one reader, `paired`
       --> tools/dylint/examples/single_reader.rs:215:1
        |
    215 | fn listed<T>(item: T) -> Vec<T> { vec![item] }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:223:6
        |
    223 |     (listed(item), item)
        |      ^^^^^^
        = help: move `listed` into `paired` as a closure

    warning: `repeated` has one reader, `tripled`
       --> tools/dylint/examples/single_reader.rs:226:1
        |
    226 | fn repeated<T: Copy>(item: T, levels: usize) -> Vec<T> { levels.checked_sub(1).map_or_else(Vec::new, |rest| [vec![item], repeated(i...
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:228:42
        |
    228 | fn tripled<T: Copy>(item: T) -> Vec<T> { repeated(item, 3) }
        |                                          ^^^^^^^^
        = help: move `repeated` into `tripled` as a nested `fn`

    warning: `halved` has one reader, `narrowed`
       --> tools/dylint/examples/single_reader.rs:166:1
        |
    166 | fn halved(width: usize) -> usize { width.strict_div(2) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:238:42
        |
    238 |     fn narrowed(width: usize) -> usize { halved(width).strict_add(1) }
        |                                          ^^^^^^
        = help: move `halved` into `narrowed` as a closure
    "#);
}

fn unreachable_public_item(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `pub` associated function is unreachable from the crate root
      --> tools/dylint/examples/unreachable_public_item.rs:15:9
       |
    15 |         pub const fn opened() -> Self { Self }
       |         ---^^^^^^^^^^^^^^^^^^^^^^^^^^
       |         |
       |         help: narrow the visibility to the crate: `pub(crate)`
       |
       = note: `#[warn(unreachable_public_item)]` on by default
    applicability MachineApplicable: narrow the visibility to the crate

    warning: `pub` function is unreachable from the crate root
      --> tools/dylint/examples/unreachable_public_item.rs:22:5
       |
    22 |     pub const fn balance() -> u32 { 1 }
       |     ---^^^^^^^^^^^^^^^^^^^^^^^^^^
       |     |
       |     help: narrow the visibility to the crate: `pub(crate)`
    applicability MachineApplicable: narrow the visibility to the crate

    warning: `pub` function is unreachable from the crate root
      --> tools/dylint/examples/unreachable_public_item.rs:8:13
       |
     6 |     macro_rules! counted {
     7 |         ($name:ident) => {
     8 |             pub fn $name() -> u32 { 3 }
       |             ^^^^^^^^^^^^^^^^^^^^^
    ...
    26 |     counted!(count);
       |     --------------- in this macro invocation
       |
    help: narrow the visibility to the crate
      --> tools/dylint/examples/unreachable_public_item.rs:8:13
       |
     8 |             pub fn $name() -> u32 { 3 }
       |             ^^^
    ...
    26 |     counted!(count);
       |     --------------- in this macro invocation
    ");
}

// --- [COMPOSITION] ---------------------------------------------------------------------

snapshots!(needless_import_rename, single_reader, unreachable_public_item);
