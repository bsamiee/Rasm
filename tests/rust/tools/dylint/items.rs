// --- [OPERATIONS] ----------------------------------------------------------------------

snapshots! {
fn needless_import_rename(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `assert` renamed to `require` with no other `assert` in scope
       --> tools/dylint/../../tests/rust/tools/dylint/macro_matching.rs:119:16
        |
    119 |     use core::{assert as require, matches as fits};
        |                ^^^^^^^^^^^^^^^^^
        |
        = help: remove `as require` and name `assert` at each use
        = note: `#[warn(needless_import_rename)]` on by default

    warning: `matches` renamed to `fits` with no other `matches` in scope
       --> tools/dylint/../../tests/rust/tools/dylint/macro_matching.rs:119:35
        |
    119 |     use core::{assert as require, matches as fits};
        |                                   ^^^^^^^^^^^^^^^
        |
        = help: remove `as fits` and name `matches` at each use

    warning: `assert` renamed to `require` with no other `assert` in scope
      --> tools/dylint/../../tests/rust/tools/dylint/expansions.rs:68:13
       |
    68 |         use core::assert as require;
       |             ^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: remove `as require` and name `assert` at each use

    warning: `write` renamed to `put` with no other `write` in scope
     --> tools/dylint/examples/macro_formatting.rs:1:34
      |
    1 | use core::{fmt, hint::black_box, write as put};
      |                                  ^^^^^^^^^^^^
      |
      = help: remove `as put` and name `write` at each use
      = note: `#[warn(needless_import_rename)]` on by default

    warning: `format` renamed to `render` with no other `format` in scope
     --> tools/dylint/examples/macro_formatting.rs:2:5
      |
    2 | use std::format as render;
      |     ^^^^^^^^^^^^^^^^^^^^^
      |
      = help: remove `as render` and name `format` at each use

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
     --> tools/dylint/examples/needless_import_rename.rs:6:28
      |
    6 |     mem::{drop as discard, take as taken},
      |                            ^^^^^^^^^^^^^
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
      --> tools/dylint/examples/needless_import_rename.rs:55:9
       |
    55 |     use core::time::Duration as Span;
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: remove `as Span` and name `Duration` at each use

    warning: `take` renamed to `removed` with no other `take` in scope
       --> tools/dylint/examples/needless_import_rename.rs:113:9
        |
    113 |     use core::mem::take as removed;
        |         ^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: remove `as removed` and name `take` at each use
    ");
}

fn orphan_module(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `tools/dylint/examples/orphan_module/nested/mod.rs` sits in this module's folder with no module declaring it, declare it here or delete it
     --> tools/dylint/examples/orphan_module.rs:1:1
      |
    1 | use core::hint::black_box;
      | ^
      |
      = note: `#[warn(orphan_module)]` on by default

    warning: `tools/dylint/examples/orphan_module/undeclared.rs` sits in this module's folder with no module declaring it, declare it here or delete it
     --> tools/dylint/examples/orphan_module.rs:1:1
      |
    1 | use core::hint::black_box;
      | ^
    ");
}

fn single_reader(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `NONE` has one reader, `named_bound`
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:52:1
       |
    52 | const NONE: u32 = 0;
       | ^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:54:62
       |
    54 | fn named_bound(count: NonZero<u32>) -> bool { count.get() == NONE }
       |                                                              ^^^^
       = help: move `NONE` into `named_bound` as a nested item
       = note: `#[warn(single_reader)]` on by default

    warning: `called` has one reader, `inline_callback`
      --> tools/dylint/examples/blocking_call_in_async.rs:37:1
       |
    37 | fn called<F: FnOnce() + 'static>(call: F) { call(); }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/examples/blocking_call_in_async.rs:39:30
       |
    39 | async fn inline_callback() { called(|| thread::sleep(Duration::ZERO)); }
       |                              ^^^^^^
       = help: move `called` into `inline_callback` as a nested `fn`
       = note: `#[warn(single_reader)]` on by default

    warning: `blocked_frame` has one reader, `spawned_frame`
       --> tools/dylint/examples/blocking_call_in_async.rs:97:1
        |
     97 | async fn blocked_frame() {
        | ^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/blocking_call_in_async.rs:105:58
        |
    105 | fn spawned_frame() -> task::JoinHandle<()> { task::spawn(blocked_frame()) }
        |                                                          ^^^^^^^^^^^^^
        = help: move `blocked_frame` into `spawned_frame` as a closure

    warning: `ignored_tracker` has one reader, `untracked`
       --> tools/dylint/examples/detached_task.rs:105:1
        |
    105 | fn ignored_tracker(tracker: &TaskTracker) -> JoinHandle<()> {
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/detached_task.rs:117:5
        |
    117 |     ignored_tracker(tracker);
        |     ^^^^^^^^^^^^^^^
        = help: move `ignored_tracker` into `untracked` as a closure
        = note: `#[warn(single_reader)]` on by default

    warning: `ignored_future` has one reader, `untracked`
       --> tools/dylint/examples/detached_task.rs:110:1
        |
    110 | / fn ignored_future<F>(future: F) -> JoinHandle<()>
    111 | | where F: Future + Send + 'static {
        | |________________________________^
        |
    note: read here
       --> tools/dylint/examples/detached_task.rs:118:5
        |
    118 |     ignored_future(tracker.track_future(async {}));
        |     ^^^^^^^^^^^^^^
        = help: move `ignored_future` into `untracked` as a nested `fn`

    warning: `infallible` has one reader, `certain`
       --> tools/dylint/examples/discarded_error.rs:77:1
        |
     77 | fn infallible(value: u32) -> Result<u32, Infallible> { Ok(value) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/discarded_error.rs:210:33
        |
    210 | fn certain(value: u32) -> u32 { infallible(value).unwrap_or_default() }
        |                                 ^^^^^^^^^^
        = help: move `infallible` into `certain` as a closure
        = note: `#[warn(single_reader)]` on by default

    warning: `LIMIT` has one reader, `VALUE`
       --> tools/dylint/examples/duplicate_trait_constant.rs:137:9
        |
    137 |         const LIMIT: u32 = 5;
        |         ^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/duplicate_trait_constant.rs:142:32
        |
    142 |             const VALUE: u32 = LIMIT;
        |                                ^^^^^
        = help: move `LIMIT` into `VALUE` as a nested item
        = note: `#[warn(single_reader)]` on by default

    warning: `LIMIT` has one reader, `VALUE`
       --> tools/dylint/examples/duplicate_trait_constant.rs:149:9
        |
    149 |         const LIMIT: u32 = 5;
        |         ^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/duplicate_trait_constant.rs:154:32
        |
    154 |             const VALUE: u32 = LIMIT;
        |                                ^^^^^
        = help: move `LIMIT` into `VALUE` as a nested item

    warning: `reordered` has one reader, `values`
       --> tools/dylint/examples/duplicate_type.rs:917:5
        |
    917 |     const fn reordered(source: Source) -> Reordered {
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/duplicate_type.rs:948:18
        |
    948 |                 [reordered(Source::Pair(1, 2)), reordered(Source::Empty)].map(|value| match value {
        |                  ^^^^^^^^^                      ^^^^^^^^^
        = help: move `reordered` into `values` as a nested `fn`
        = note: `#[warn(single_reader)]` on by default

    warning: `repeated` has one reader, `values`
       --> tools/dylint/examples/duplicate_type.rs:924:5
        |
    924 |     const fn repeated(source: Source) -> Repeated {
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/duplicate_type.rs:952:18
        |
    952 |                 [repeated(Source::Pair(3, 4)), repeated(Source::Empty)].map(|value| match value {
        |                  ^^^^^^^^                      ^^^^^^^^
        = help: move `repeated` into `values` as a nested `fn`

    warning: `renamed` has one reader, `values`
       --> tools/dylint/examples/duplicate_type.rs:931:5
        |
    931 |     const fn renamed(source: Source) -> Renamed {
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/duplicate_type.rs:956:18
        |
    956 |                 [renamed(Source::Pair(5, 6)), renamed(Source::Empty)].map(|value| match value {
        |                  ^^^^^^^                      ^^^^^^^
        = help: move `renamed` into `values` as a nested `fn`

    warning: `projected` has one reader, `values`
       --> tools/dylint/examples/duplicate_type.rs:938:5
        |
    938 |     const fn projected(source: Source) -> Projected {
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/duplicate_type.rs:961:84
        |
    961 | ...(9_u32..10).mode(), matches!(projected(Source::Pair(11, 12)), Projected::Pair), matches!(projected(Source::Empty), Projected::Em...
        |                                 ^^^^^^^^^                                                   ^^^^^^^^^
        = help: move `projected` into `values` as a nested `fn`

    warning: `POSITIVE` has one reader, `main`
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:7:1
       |
     7 | const POSITIVE: () = assert!(LIMIT > 0);
       | ^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:78:9
       |
    78 |         POSITIVE,
       |         ^^^^^^^^
       = help: move `POSITIVE` into `main` as a nested item
       = note: `#[warn(single_reader)]` on by default

    warning: `bounded` has one reader, `main`
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:19:1
       |
    19 | fn bounded(values: &[usize]) -> usize {
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:79:9
       |
    79 |         bounded(&[1, 2]),
       |         ^^^^^^^
       = help: move `bounded` into `main` as a closure

    warning: `paired` has one reader, `main`
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:24:1
       |
    24 | fn paired(left: &[usize], right: &[usize]) -> usize {
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:80:9
       |
    80 |         paired(&[1], &[2]),
       |         ^^^^^^
       = help: move `paired` into `main` as a closure

    warning: `distinct` has one reader, `main`
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:29:1
       |
    29 | fn distinct(left: usize, right: usize) -> usize {
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:81:9
       |
    81 |         distinct(1, 2),
       |         ^^^^^^^^
       = help: move `distinct` into `main` as a closure

    warning: `accepted` has one reader, `main`
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:34:1
       |
    34 | fn accepted(reply: &Reply) -> usize {
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:82:9
       |
    82 |         accepted(&Reply::Accepted(1)),
       |         ^^^^^^^^
    83 |         accepted(&Reply::Rejected),
       |         ^^^^^^^^
       = help: move `accepted` into `main` as a closure

    warning: `compiled` has one reader, `main`
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:58:1
       |
    58 | fn compiled() -> usize {
       | ^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:84:9
       |
    84 |         compiled(),
       |         ^^^^^^^^
       = help: move `compiled` into `main` as a closure

    warning: `equal` has one reader, `main`
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:42:1
       |
    42 | fn equal(left: usize, right: usize) -> usize {
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:85:9
       |
    85 |         equal(1, 1),
       |         ^^^^^
       = help: move `equal` into `main` as a closure

    warning: `checked` has one reader, `main`
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:47:1
       |
    47 | fn checked(values: &[usize]) -> usize {
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:86:9
       |
    86 |         checked(&[1]),
       |         ^^^^^^^
       = help: move `checked` into `main` as a closure

    warning: `rejected` has one reader, `main`
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:53:1
       |
    53 | fn rejected(reply: &Reply) -> bool {
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:87:9
       |
    87 |         rejected(&Reply::Rejected),
       |         ^^^^^^^^
       = help: move `rejected` into `main` as a closure

    warning: `wrapped` has one reader, `main`
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:69:1
       |
    69 | fn wrapped(count: usize) -> usize {
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:88:9
       |
    88 |         wrapped(1),
       |         ^^^^^^^
       = help: move `wrapped` into `main` as a closure

    warning: `helper` has one reader, `asserted`
       --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:94:5
        |
     94 |     fn helper(value: usize) {
        |     ^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:101:9
        |
    101 |         helper(1);
        |         ^^^^^^
        = help: move `helper` into `asserted` as a closure

    warning: `HITS` has one reader, `main`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:19:1
       |
    19 | static HITS: AtomicU32 = AtomicU32::new(0);
       | ^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:56:5
       |
    56 |     HITS.fetch_add(1, Ordering::Relaxed);
       |     ^^^^
    57 |     CALLS.fetch_add(DEPTH.with(|depth| *depth), Ordering::Relaxed);
    58 |     VISITS.set(HITS.load(Ordering::Relaxed));
       |                ^^^^
    ...
    64 |         HITS.fetch_add(*limit, Ordering::Relaxed);
       |         ^^^^
    65 |     }
    66 |     HITS.fetch_add(Options::parse_from(["fixture"]).port, Ordering::Relaxed);
       |     ^^^^
       = help: move `HITS` into `main` as a nested item

    warning: `CALLS` has one reader, `main`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:13:9
       |
    11 | macro_rules! counter {
    12 |     ($name:ident) => {
    13 |         static $name: AtomicU32 = AtomicU32::new(0);
       |         ^^^^^^^^^^^^^^^^^^^^^^^
    ...
    40 | counter!(CALLS);
       | --------------- in this macro invocation
       |
    note: read here
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:57:5
       |
    57 |     CALLS.fetch_add(DEPTH.with(|depth| *depth), Ordering::Relaxed);
       |     ^^^^^
       = help: move `CALLS` into `main` as a nested item

    warning: `ROOT` has one reader, `main`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:21:1
       |
    21 | static ROOT: OnceLock<String> = OnceLock::new();
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:59:5
       |
    59 |     ROOT.get_or_init(|| GREETING.to_owned());
       |     ^^^^
       = help: move `ROOT` into `main` as a nested item

    warning: `GREETING` has one reader, `main`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:31:1
       |
    31 | static GREETING: &str = "hello";
       | ^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:59:25
       |
    59 |     ROOT.get_or_init(|| GREETING.to_owned());
       |                         ^^^^^^^^
    60 |     ORIGIN.get_or_init(|| GREETING.to_owned());
       |                           ^^^^^^^^
       = help: move `GREETING` into `main` as a nested item

    warning: `ORIGIN` has one reader, `main`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:23:1
       |
    23 | pub static ORIGIN: OnceLock<String> = OnceLock::new();
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:60:5
       |
    60 |     ORIGIN.get_or_init(|| GREETING.to_owned());
       |     ^^^^^^
       = help: move `ORIGIN` into `main` as a nested item

    warning: `COUNTER` has one reader, `main`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:27:1
       |
    27 | static COUNTER: LazyLock<AtomicU32> = LazyLock::new(|| AtomicU32::new(1));
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:61:5
       |
    61 |     COUNTER.fetch_add(1, Ordering::Relaxed);
       |     ^^^^^^^
       = help: move `COUNTER` into `main` as a nested item

    warning: `SHARED` has one reader, `main`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:29:1
       |
    29 | static SHARED: LazyLock<Arc<Mutex<Vec<u8>>>> = LazyLock::new(|| Arc::new(Mutex::new(Vec::new())));
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:62:5
       |
    62 |     SHARED.lock().map(|mut bytes| bytes.push(1)).unwrap_or_default();
       |     ^^^^^^
       = help: move `SHARED` into `main` as a nested item

    warning: `TABLE` has one reader, `main`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:25:1
       |
    25 | static TABLE: LazyLock<Vec<u32>> = LazyLock::new(|| (1..4).collect());
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:63:18
       |
    63 |     for limit in TABLE.iter().chain(&LIMITS) {
       |                  ^^^^^
       = help: move `TABLE` into `main` as a nested item

    warning: `LIMITS` has one reader, `main`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:33:1
       |
    33 | static LIMITS: [u32; 2] = [1, 2];
       | ^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:63:38
       |
    63 |     for limit in TABLE.iter().chain(&LIMITS) {
       |                                      ^^^^^^
       = help: move `LIMITS` into `main` as a nested item

    warning: `TEST_VALUE` has one reader, `counted`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:73:5
       |
    73 |     static TEST_VALUE: AtomicU32 = AtomicU32::new(0);
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:78:9
       |
    78 |         TEST_VALUE.fetch_add(1, Ordering::Relaxed);
       |         ^^^^^^^^^^
       = help: move `TEST_VALUE` into `counted` as a nested item

    warning: `PRIVATE_LIMIT` has one reader, `default`
       --> tools/dylint/examples/manual_default_field_values.rs:400:5
        |
    400 |     const PRIVATE_LIMIT: u8 = 6;
        |     ^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/manual_default_field_values.rs:405:49
        |
    405 |         fn default() -> Self { Self { attempts: PRIVATE_LIMIT } }
        |                                                 ^^^^^^^^^^^^^
        = help: move `PRIVATE_LIMIT` into `default` as a nested item
        = note: `#[warn(single_reader)]` on by default

    warning: `EMPTY` has one reader, `as_ref`
      --> tools/dylint/examples/manual_partition.rs:45:1
       |
    45 | static EMPTY: Catalog = Catalog { words: Vec::new() };
       | ^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/examples/manual_partition.rs:24:10
       |
    24 |         &EMPTY
       |          ^^^^^
       = help: move `EMPTY` into `as_ref` as a nested item
       = note: `#[warn(single_reader)]` on by default

    warning: `trim` has one reader, `main`
      --> tools/dylint/examples/manual_path_is_empty_no_std.rs:11:1
       |
    11 | fn trim(recent: &mut VecDeque<u8>) {
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/examples/manual_path_is_empty_no_std.rs:23:5
       |
    23 |     trim(&mut recent);
       |     ^^^^
       = help: move `trim` into `main` as a closure
       = note: `#[warn(single_reader)]` on by default

    warning: `values` has one reader, `main`
      --> tools/dylint/examples/manual_path_is_empty_no_std.rs:17:1
       |
    17 | fn values(count: usize) -> Vec<usize> { (0..count).map(|index| index.strict_mul(2)).collect() }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/examples/manual_path_is_empty_no_std.rs:24:62
       |
    24 |     black_box((Path::new("").as_os_str().is_empty(), recent, values(2)));
       |                                                              ^^^^^^
       = help: move `values` into `main` as a closure

    warning: `consume_guard` has one reader, `guarded`
      --> tools/dylint/examples/needless_cell.rs:64:1
       |
    64 | fn consume_guard(value: Ref<'_, String>) -> usize { value.len() }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/examples/needless_cell.rs:68:5
       |
    68 |     consume_guard(value.borrow())
       |     ^^^^^^^^^^^^^
       = help: move `consume_guard` into `guarded` as a closure
       = note: `#[warn(single_reader)]` on by default

    warning: `consume_cells` has one reader, `array_view`
      --> tools/dylint/examples/needless_cell.rs:79:1
       |
    79 | fn consume_cells(values: &[Cell<u32>; 2]) -> u32 { values.iter().map(Cell::get).sum() }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/examples/needless_cell.rs:83:5
       |
    83 |     consume_cells(values.as_array_of_cells())
       |     ^^^^^^^^^^^^^
       = help: move `consume_cells` into `array_view` as a closure

    warning: `passed` has one reader, `named`
      --> tools/dylint/examples/partition_by_result.rs:23:1
       |
    23 | const fn passed(result: &Parsed) -> bool { result.is_ok() }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/examples/partition_by_result.rs:29:94
       |
    29 | fn named(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(passed) }
       |                                                                                              ^^^^^^
       = help: move `passed` into `named` as a nested `fn`
       = note: `#[warn(single_reader)]` on by default

    warning: `small` has one reader, `ranked`
      --> tools/dylint/examples/partition_by_result.rs:27:1
       |
    27 | fn small(result: &Parsed) -> bool { result.is_small() }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/examples/partition_by_result.rs:58:95
       |
    58 | fn ranked(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(small) }
       |                                                                                               ^^^^^
       = help: move `small` into `ranked` as a closure

    warning: `large` has one reader, `sized`
      --> tools/dylint/examples/partition_by_result.rs:25:1
       |
    25 | const fn large(value: &u8) -> bool { *value > 9 }
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: read here
      --> tools/dylint/examples/partition_by_result.rs:69:80
       |
    69 | fn sized(values: Vec<u8>) -> (Vec<u8>, Vec<u8>) { values.into_iter().partition(large) }
       |                                                                                ^^^^^
       = help: move `large` into `sized` as a nested `fn`

    warning: `run` has one reader, `ran`
       --> tools/dylint/examples/single_impl_trait.rs:189:1
        |
    189 | / fn run<H>(handler: &H) -> u32
    190 | | where H: Handler {
        | |________________^
        |
    note: read here
       --> tools/dylint/examples/single_impl_trait.rs:194:19
        |
    194 | fn ran() -> u32 { run(&|key: u32| key.strict_add(1)) }
        |                   ^^^
        = help: move `run` into `ran` as a nested `fn`
        = note: `#[warn(single_reader)]` on by default

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
        = note: `#[warn(single_reader)]` on by default

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
       --> tools/dylint/examples/single_reader.rs:184:31
        |
    184 | fn padded() -> usize { [1_u8; DEPTH + 1].len() }
        |                               ^^^^^
        = help: move `DEPTH` into `padded` as a nested item

    warning: `framed` has one reader, `gridded`
       --> tools/dylint/examples/single_reader.rs:252:1
        |
    252 | fn framed(grid: &Grid) -> usize { grid.cells.len() }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:186:36
        |
    186 | fn gridded(grid: &Grid) -> usize { framed(grid).strict_add(WIDTH) }
        |                                    ^^^^^^
        = help: move `framed` into `gridded` as a closure

    warning: `seed` has one reader, `SEED`
       --> tools/dylint/examples/single_reader.rs:188:1
        |
    188 | const fn seed() -> u8 { 3 }
        | ^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:194:18
        |
    194 | const SEED: u8 = seed();
        |                  ^^^^
        = help: move `seed` into `SEED` as a nested `fn`

    warning: `PORTS` has one reader, `port`
       --> tools/dylint/examples/single_reader.rs:20:1
        |
     20 | static PORTS: LazyLock<Vec<u16>> = LazyLock::new(|| (1..=4).map(|step: u16| step.strict_mul(80)).collect());
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:200:40
        |
    200 | fn port(index: usize) -> Option<u16> { PORTS.get(index).copied() }
        |                                        ^^^^^
        = help: move `PORTS` into `port` as a nested item

    warning: `graded` has one reader, `grade`
       --> tools/dylint/examples/single_reader.rs:208:1
        |
    208 | fn graded(score: u8) -> u8 { if score > 4 { if score > 7 { 2 } else { 1 } } else { 0 } }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:212:29
        |
    212 | fn grade(score: u8) -> u8 { graded(score).strict_add(ranked(score)) }
        |                             ^^^^^^
        = help: move `graded` into `grade` as a closure

    warning: `floored` has one reader, `unwrapped`
       --> tools/dylint/examples/single_reader.rs:214:1
        |
    214 | fn floored(width: usize) -> usize { if width.is_power_of_two() { width } else { width.next_power_of_two() } }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:217:43
        |
    217 |     fn unwrapped(width: usize) -> usize { floored(width).strict_mul(2) }
        |                                           ^^^^^^^
        = help: move `floored` into `unwrapped` as a closure

    warning: `optional` has one reader, `doubled`
       --> tools/dylint/examples/single_reader.rs:233:1
        |
    233 | fn optional<T>(item: T) -> Option<T> { Some(item) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:235:51
        |
    235 | fn doubled<T: Copy>(item: T) -> [Option<T>; 2] { [optional(item), optional(item)] }
        |                                                   ^^^^^^^^        ^^^^^^^^
        = help: move `optional` into `doubled` as a closure

    warning: `listed` has one reader, `paired`
       --> tools/dylint/examples/single_reader.rs:231:1
        |
    231 | fn listed<T>(item: T) -> Vec<T> { vec![item] }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:239:6
        |
    239 |     (listed(item), item)
        |      ^^^^^^
        = help: move `listed` into `paired` as a closure

    warning: `repeated` has one reader, `tripled`
       --> tools/dylint/examples/single_reader.rs:242:1
        |
    242 | fn repeated<T: Copy>(item: T, levels: usize) -> Vec<T> { levels.checked_sub(1).map_or_else(Vec::new, |rest| [vec![item], repeated(i...
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:244:42
        |
    244 | fn tripled<T: Copy>(item: T) -> Vec<T> { repeated(item, 3) }
        |                                          ^^^^^^^^
        = help: move `repeated` into `tripled` as a nested `fn`

    warning: `advanced` has one reader, `stepped`
       --> tools/dylint/examples/single_reader.rs:246:1
        |
    246 | fn advanced(cursor: &Cursor) -> usize { cursor.position.strict_add(1) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:250:46
        |
    250 | fn stepped(cursor: &Cursor) -> [usize; 2] { [advanced(cursor), rewound(cursor)] }
        |                                              ^^^^^^^^
        = help: move `advanced` into `stepped` as a closure

    warning: `rewound` has one reader, `stepped`
       --> tools/dylint/examples/single_reader.rs:248:1
        |
    248 | fn rewound(cursor: &Cursor) -> usize { cursor.position.saturating_sub(1) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:250:64
        |
    250 | fn stepped(cursor: &Cursor) -> [usize; 2] { [advanced(cursor), rewound(cursor)] }
        |                                                                ^^^^^^^
        = help: move `rewound` into `stepped` as a closure

    warning: `spanned` has one reader, `compared`
       --> tools/dylint/examples/single_reader.rs:254:1
        |
    254 | fn spanned(left: &Window, right: &Window) -> usize { left.width.abs_diff(right.width) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:256:38
        |
    256 | fn compared(width: usize) -> usize { spanned(&Window { width }, &Window { width: 1 }) }
        |                                      ^^^^^^^
        = help: move `spanned` into `compared` as a closure

    warning: `halved` has one reader, `narrowed`
       --> tools/dylint/examples/single_reader.rs:182:1
        |
    182 | fn halved(width: usize) -> usize { width.strict_div(2) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/single_reader.rs:262:42
        |
    262 |     fn narrowed(width: usize) -> usize { halved(width).strict_add(1) }
        |                                          ^^^^^^
        = help: move `halved` into `narrowed` as a closure

    warning: `error_text` has one reader, `factory_errored`
       --> tools/dylint/examples/stringified_error.rs:103:1
        |
    103 | const fn error_text(message: String) -> Result<u16, String> { Err(message) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/stringified_error.rs:105:68
        |
    105 | fn factory_errored(error: &ParseIntError) -> Result<u16, String> { error_text(error.to_string()) }
        |                                                                    ^^^^^^^^^^
        = help: move `error_text` into `factory_errored` as a nested `fn`
        = note: `#[warn(single_reader)]` on by default

    warning: `success_text` has one reader, `factory_view`
       --> tools/dylint/examples/stringified_error.rs:107:1
        |
    107 | const fn success_text(message: String) -> Result<String, ParseIntError> { Ok(message) }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/stringified_error.rs:109:77
        |
    109 | fn factory_view(error: &ParseIntError) -> Result<usize, ParseIntError> { Ok(success_text(error.to_string())?.len()) }
        |                                                                             ^^^^^^^^^^^^
        = help: move `success_text` into `factory_view` as a nested `fn`

    warning: `replaced` has one reader, `unrelated`
       --> tools/dylint/examples/stringified_error.rs:173:1
        |
    173 | fn replaced(message: String) -> String {
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/stringified_error.rs:180:47
        |
    180 | fn unrelated(error: &ParseIntError) -> bool { replaced(error.to_string()) == "unrelated" }
        |                                               ^^^^^^^^
        = help: move `replaced` into `unrelated` as a closure

    warning: `viewed` has one reader, `inspected`
       --> tools/dylint/examples/stringified_error.rs:168:1
        |
    168 | fn viewed(message: String) -> PhantomData<String> {
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/stringified_error.rs:182:62
        |
    182 | fn inspected(error: &ParseIntError) -> PhantomData<String> { viewed(error.to_string()) }
        |                                                              ^^^^^^
        = help: move `viewed` into `inspected` as a closure

    warning: `returned_formatter` has one reader, `viewed_formatters`
       --> tools/dylint/examples/stringified_error.rs:238:1
        |
    238 | fn returned_formatter() -> fn(&ParseIntError) -> String { ToString::to_string }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/stringified_error.rs:242:43
        |
    242 |     (log(&(renderer.format)(error)), log(&returned_formatter()(error)))
        |                                           ^^^^^^^^^^^^^^^^^^
        = help: move `returned_formatter` into `viewed_formatters` as a closure

    warning: `discarded_stream` has one reader, `disconnected_stream`
       --> tools/dylint/examples/unending_join.rs:219:1
        |
    219 | fn discarded_stream(stream: WatchStream<u32>) -> WatchStream<u32> {
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/unending_join.rs:226:19
        |
    226 |     let updates = discarded_stream(WatchStream::new(sender.subscribe()));
        |                   ^^^^^^^^^^^^^^^^
        = help: move `discarded_stream` into `disconnected_stream` as a closure
        = note: `#[warn(single_reader)]` on by default

    warning: `transformed` has one reader, `indirect_key`
       --> tools/dylint/examples/untyped_json_access.rs:137:1
        |
    137 | fn transformed(key: &str) -> String { format!("prefix-{key}") }
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: read here
       --> tools/dylint/examples/untyped_json_access.rs:139:68
        |
    139 | fn indirect_key(document: &Value) -> Option<&Value> { document.get(transformed("name")) }
        |                                                                    ^^^^^^^^^^^
        = help: move `transformed` into `indirect_key` as a closure
        = note: `#[warn(single_reader)]` on by default
    "#);
}

}
