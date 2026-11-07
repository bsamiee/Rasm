// --- [OPERATIONS] ----------------------------------------------------------------------

#[rustfmt::skip]
snapshots! {
fn manual_bool_toggle(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: assignment negates `*flag` by hand
      --> tools/dylint/examples/manual_bool_toggle.rs:42:31
       |
    42 | fn flipped(flag: &mut bool) { *flag = !*flag; }
       |                               ^^^^^^^^^^^^^^ help: call `bool::toggle`: `<bool>::toggle(&mut *flag)`
       |
       = note: `#[warn(manual_bool_toggle)]` on by default
    applicability MachineApplicable: call `bool::toggle`

    warning: assignment negates `*flag` by hand
      --> tools/dylint/examples/manual_bool_toggle.rs:44:29
       |
    44 | fn xored(flag: &mut bool) { *flag ^= true; }
       |                             ^^^^^^^^^^^^^ help: call `bool::toggle`: `<bool>::toggle(&mut *flag)`
    applicability MachineApplicable: call `bool::toggle`

    warning: assignment negates `**flag` by hand
      --> tools/dylint/examples/manual_bool_toggle.rs:58:34
       |
    58 | fn boxed(flag: &mut Box<bool>) { **flag = !**flag; }
       |                                  ^^^^^^^^^^^^^^^^ help: call `bool::toggle`: `<bool>::toggle(&mut **flag)`
    applicability MachineApplicable: call `bool::toggle`

    warning: assignment negates `*flag` by hand
      --> tools/dylint/examples/manual_bool_toggle.rs:63:5
       |
    63 |     *flag = !*flag;
       |     ^^^^^^^^^^^^^^ help: call `bool::toggle`: `<bool>::toggle(&mut *flag)`
    applicability MachineApplicable: call `bool::toggle`

    warning: assignment negates `self.ready` by hand
      --> tools/dylint/examples/manual_gen_block.rs:99:9
       |
    99 |         self.ready = !self.ready;
       |         ^^^^^^^^^^^^^^^^^^^^^^^^ help: call `bool::toggle`: `<bool>::toggle(&mut self.ready)`
       |
       = note: `#[warn(manual_bool_toggle)]` on by default
    applicability MachineApplicable: call `bool::toggle`

    warning: assignment negates `self.ready` by hand
       --> tools/dylint/examples/manual_gen_block.rs:108:9
        |
    108 |         self.ready = !self.ready;
        |         ^^^^^^^^^^^^^^^^^^^^^^^^ help: call `bool::toggle`: `<bool>::toggle(&mut self.ready)`
    applicability MachineApplicable: call `bool::toggle`
    ");
}

fn manual_default_field_values(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `Default` for `Retry` builds each field from a constant
      --> tools/dylint/examples/manual_default_field_values.rs:40:28
       |
    40 |     fn default() -> Self { Self { attempts: 3, label: String::new() } }
       |                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: declare `attempts: u8 = 3` under `#![feature(default_field_values)]`, derive `Default`, and delete the impl
       = note: `#[warn(manual_default_field_values)]` on by default

    warning: `Default` for `Config` builds each field from its type's default through `new`
      --> tools/dylint/examples/manual_default_field_values.rs:65:28
       |
    65 |     fn default() -> Self { Self::new() }
       |                            ^^^^^^^^^^^
       |
    note: fields built here
      --> tools/dylint/examples/manual_default_field_values.rs:61:24
       |
    61 |     fn new() -> Self { Self { depth: 0, name: String::default(), enabled: false, label: "", parent: None } }
       |                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: derive `Default` and delete the impl

    warning: `Default` for `Limits` builds each field from a constant through `new`
      --> tools/dylint/examples/manual_default_field_values.rs:82:28
       |
    82 |     fn default() -> Self { Self::new() }
       |                            ^^^^^^^^^^^
       |
    note: fields built here
      --> tools/dylint/examples/manual_default_field_values.rs:78:24
       |
    78 |     fn new() -> Self { Self { depth: 4, name: String::new(), enabled: false, label: "network", parent: None } }
       |                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: declare `depth: u8 = 4`, `label: &'static str = "network"` under `#![feature(default_field_values)]`, derive `Default`, and delete the impl

    warning: `Default` for `Attributed` builds each field from a constant
      --> tools/dylint/examples/manual_default_field_values.rs:92:28
       |
    92 |     fn default() -> Self { Self { attempts: 3 } }
       |                            ^^^^^^^^^^^^^^^^^^^^
       |
       = help: declare `attempts: u8 = 3` under `#![feature(default_field_values)]`, derive `Default`, and delete the impl

    warning: `Default` for `Custom` builds each field from a constant through `fresh`
       --> tools/dylint/examples/manual_default_field_values.rs:192:28
        |
    192 |     fn default() -> Self { Self::fresh() }
        |                            ^^^^^^^^^^^^^
        |
    note: fields built here
       --> tools/dylint/examples/manual_default_field_values.rs:188:26
        |
    188 |     fn fresh() -> Self { Self { number: Number::new() } }
        |                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: declare `number: Number = Number::new()` under `#![feature(default_field_values)]`, derive `Default`, and delete the impl

    warning: `Default` for `Nested` builds each field from a constant
       --> tools/dylint/examples/manual_default_field_values.rs:212:28
        |
    212 |     fn default() -> Self { Self { values: (Number::new(), 0) } }
        |                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: declare `values: (Number, u8) = (Number::new(), 0)` under `#![feature(default_field_values)]`, derive `Default`, and delete the impl

    warning: `Default` for `Constant` builds each field from a constant
       --> tools/dylint/examples/manual_default_field_values.rs:378:28
        |
    378 |     fn default() -> Self { Self { attempts: 3 } }
        |                            ^^^^^^^^^^^^^^^^^^^^
        |
        = help: declare `attempts: u8 = 3` under `#![feature(default_field_values)]`, add `#[derive_const(Default)]` under `#![feature(derive_const)]`, and delete the impl

    warning: `Default` for `Tuple` builds each field from a constant
       --> tools/dylint/examples/manual_default_field_values.rs:385:28
        |
    385 |     fn default() -> Self { Self { 0: 3 } }
        |                            ^^^^^^^^^^^^^
        |
        = help: declare `u8 = 3` under `#![feature(default_field_values)]`, derive `Default`, and delete the impl
    "#);
}

fn manual_deref_pattern(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `as_str` derefs `owned` for its patterns
      --> tools/dylint/examples/manual_deref_pattern.rs:15:11
       |
    15 |     match owned.as_str() {
       |           ^^^^^^^^^^^^^^ help: match `&owned` under `#![feature(deref_patterns)]`: `&owned`
       |
       = note: `#[warn(manual_deref_pattern)]` on by default
    applicability MaybeIncorrect: match `&owned` under `#![feature(deref_patterns)]`

    warning: `as_slice` derefs `owned` for its patterns
      --> tools/dylint/examples/manual_deref_pattern.rs:24:30
       |
    24 |     if let [first, second] = owned.as_slice() { first.strict_add(*second) } else { 0 }
       |                              ^^^^^^^^^^^^^^^^ help: match `&owned` under `#![feature(deref_patterns)]`: `&owned`
    applicability MaybeIncorrect: match `&owned` under `#![feature(deref_patterns)]`

    warning: `as_slice` derefs `owned` for its patterns
      --> tools/dylint/examples/manual_deref_pattern.rs:29:23
       |
    29 |     let [first, ..] = owned.as_slice() else { return 0 };
       |                       ^^^^^^^^^^^^^^^^ help: match `&owned` under `#![feature(deref_patterns)]`: `&owned`
    applicability MaybeIncorrect: match `&owned` under `#![feature(deref_patterns)]`

    warning: `as_deref` derefs `profile.name` for its patterns
      --> tools/dylint/examples/manual_deref_pattern.rs:34:11
       |
    34 |     match profile.name.as_deref() {
       |           ^^^^^^^^^^^^^^^^^^^^^^^ help: match `&profile.name` under `#![feature(deref_patterns)]`: `&profile.name`
    applicability MaybeIncorrect: match `&profile.name` under `#![feature(deref_patterns)]`

    warning: `&*` derefs `value` for its patterns
      --> tools/dylint/examples/manual_deref_pattern.rs:42:11
       |
    42 |     match &**value {
       |           ^^^^^^^^ help: match `value` under `#![feature(deref_patterns)]`: `value`
    applicability MaybeIncorrect: match `value` under `#![feature(deref_patterns)]`

    warning: `&*` derefs `held` for its patterns
      --> tools/dylint/examples/manual_deref_pattern.rs:51:11
       |
    51 |     match &*held {
       |           ^^^^^^ help: match `&held` under `#![feature(deref_patterns)]`: `&held`
    applicability MaybeIncorrect: match `&held` under `#![feature(deref_patterns)]`
    ");
}

fn manual_drop_guard(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `Drop` for `Guard` calls the closure `F` it holds
     --> tools/dylint/examples/manual_drop_guard.rs:5:1
      |
    5 | struct Guard<F>(Option<F>)
      | ^^^^^^^^^^^^^^^
      |
      = help: replace `Guard` with `core::mem::DropGuard::new((), |()| ..)` and call `DropGuard::dismiss` where code disarms the guard
      = note: `#[warn(manual_drop_guard)]` on by default

    warning: `Drop` for `Deferred` calls the closure `F` it holds
      --> tools/dylint/examples/manual_drop_guard.rs:18:1
       |
    18 | struct Deferred<F>(F)
       | ^^^^^^^^^^^^^^^^^^
       |
       = help: replace `Deferred` with `core::mem::DropGuard::new((), |()| ..)` and call `DropGuard::dismiss` where code disarms the guard

    warning: `Drop` for `Manual` calls the closure `F` it holds
      --> tools/dylint/examples/manual_drop_guard.rs:40:1
       |
    40 | struct Manual<F>(ManuallyDrop<F>)
       | ^^^^^^^^^^^^^^^^
       |
       = help: replace `Manual` with `core::mem::DropGuard::new((), |()| ..)` and call `DropGuard::dismiss` where code disarms the guard
    ");
}

fn manual_fmt_from_fn(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `Joined` is built at one site and implements only `Display`
      --> tools/dylint/examples/manual_fmt_from_fn.rs:5:8
       |
     5 | struct Joined<'items>(&'items [u8]);
       |        ^^^^^^
       |
    note: built here
      --> tools/dylint/examples/manual_fmt_from_fn.rs:97:32
       |
    97 |     let joined = format!("{}", Joined(&values));
       |                                ^^^^^^^^^^^^^^^
       = help: replace `Joined` with `fmt::from_fn(move |formatter| ..)` at its construction site
       = note: `#[warn(manual_fmt_from_fn)]` on by default
    "#);
}

fn manual_from_utf8_lossy_owned(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: lossy decoding copies `bytes`, which nothing reads afterwards
      --> tools/dylint/examples/manual_from_utf8_lossy_owned.rs:20:40
       |
    20 | fn decoded(bytes: Vec<u8>) -> String { String::from_utf8_lossy(&bytes).into_owned() }
       |                                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `String::from_utf8_lossy_owned`: `String::from_utf8_lossy_owned(bytes)`
       |
       = note: `#[warn(manual_from_utf8_lossy_owned)]` on by default
    applicability MachineApplicable: call `String::from_utf8_lossy_owned`

    warning: lossy decoding copies `output.stdout`, which nothing reads afterwards
      --> tools/dylint/examples/manual_from_utf8_lossy_owned.rs:22:47
       |
    22 | fn decoded_output(output: Output) -> String { String::from_utf8_lossy(&output.stdout).into_owned() }
       |                                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `String::from_utf8_lossy_owned`: `String::from_utf8_lossy_owned(output.stdout)`
    applicability MachineApplicable: call `String::from_utf8_lossy_owned`

    warning: lossy decoding copies `bytes`, which nothing reads afterwards
      --> tools/dylint/examples/manual_from_utf8_lossy_owned.rs:43:42
       |
    43 | fn qualified(bytes: Vec<u8>) -> String { std::borrow::Cow::into_owned(String::from_utf8_lossy(&bytes)) }
       |                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `String::from_utf8_lossy_owned`: `String::from_utf8_lossy_owned(bytes)`
    applicability MachineApplicable: call `String::from_utf8_lossy_owned`

    warning: lossy decoding copies `bytes`, which nothing reads afterwards
      --> tools/dylint/examples/manual_from_utf8_lossy_owned.rs:45:42
       |
    45 | fn converted(bytes: Vec<u8>) -> String { String::from_utf8_lossy(&bytes).into() }
       |                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `String::from_utf8_lossy_owned`: `String::from_utf8_lossy_owned(bytes)`
    applicability MachineApplicable: call `String::from_utf8_lossy_owned`

    warning: lossy decoding copies `bytes`, which nothing reads afterwards
      --> tools/dylint/examples/manual_from_utf8_lossy_owned.rs:47:42
       |
    47 | fn displayed(bytes: Vec<u8>) -> String { String::from_utf8_lossy(&bytes).to_string() }
       |                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `String::from_utf8_lossy_owned`: `String::from_utf8_lossy_owned(bytes)`
    applicability MachineApplicable: call `String::from_utf8_lossy_owned`

    warning: lossy decoding copies `bytes`, which nothing reads afterwards
      --> tools/dylint/examples/manual_from_utf8_lossy_owned.rs:76:9
       |
    76 |         String::from_utf8_lossy(&bytes).into_owned()
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `String::from_utf8_lossy_owned`: `String::from_utf8_lossy_owned(bytes)`
    applicability MachineApplicable: call `String::from_utf8_lossy_owned`

    warning: lossy decoding copies `bytes`, which nothing reads afterwards
      --> tools/dylint/examples/manual_from_utf8_lossy_owned.rs:84:5
       |
    84 |     std::string::String::from_utf8_lossy(&bytes).into_owned()
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `String::from_utf8_lossy_owned`: `std::string::String::from_utf8_lossy_owned(bytes)`
    applicability MachineApplicable: call `String::from_utf8_lossy_owned`
    ");
}

fn manual_gen_block(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `Countdown` is built at one site and implements only `Iterator`
       --> tools/dylint/examples/manual_gen_block.rs:9:8
        |
      9 | struct Countdown {
        |        ^^^^^^^^^
        |
    note: built here
       --> tools/dylint/examples/manual_gen_block.rs:153:23
        |
    153 |     let counted: u8 = Countdown { left: 3 }.sum();
        |                       ^^^^^^^^^^^^^^^^^^^^^
        = help: replace `Countdown` with a `gen move { .. }` block at its construction site under `#![feature(gen_blocks)]`
        = note: `#[warn(manual_gen_block)]` on by default

    warning: `Restart` is built at one site and implements only `Iterator`
       --> tools/dylint/examples/manual_gen_block.rs:18:8
        |
     18 | struct Restart {
        |        ^^^^^^^
        |
    note: built here
       --> tools/dylint/examples/manual_gen_block.rs:155:39
        |
    155 |     let restarted: u8 = Iterator::sum(Restart { left: 4 });
        |                                       ^^^^^^^^^^^^^^^^^^^
        = help: replace `Restart` with a `gen move { .. }` block at its construction site under `#![feature(gen_blocks)]`
    ");
}

fn manual_partition(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: two `filter` passes split `self.words` on one predicate and its negation
      --> tools/dylint/examples/manual_partition.rs:16:22
       |
    16 |         let filled = self.words.iter().filter(|word| !word.is_empty()).copied().collect();
       |                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: other pass here
      --> tools/dylint/examples/manual_partition.rs:15:21
       |
    15 |         let blank = self.words.iter().filter(|word| word.is_empty()).copied().collect();
       |                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: iterate `self.words` once with `Iterator::partition`
       = note: `#[warn(manual_partition)]` on by default

    warning: two `filter` passes split `words` on one predicate and its negation
      --> tools/dylint/examples/manual_partition.rs:51:18
       |
    51 |     let filled = words.iter().filter(|word| !str::is_empty(word)).copied().collect();
       |                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: other pass here
      --> tools/dylint/examples/manual_partition.rs:50:17
       |
    50 |     let blank = words.iter().filter(|word| str::is_empty(word)).copied().collect();
       |                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: iterate `words` once with `Iterator::partition`

    warning: two `filter` passes split `words` on one predicate and its negation
      --> tools/dylint/examples/manual_partition.rs:70:22
       |
    70 |         let filled = words.iter().filter(|word| !word.is_empty()).copied().collect();
       |                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: other pass here
      --> tools/dylint/examples/manual_partition.rs:69:21
       |
    69 |         let blank = words.iter().filter(|word| word.is_empty()).copied().collect();
       |                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: iterate `words` once with `Iterator::partition`

    warning: two `filter` passes split `items` on one predicate and its negation
      --> tools/dylint/examples/manual_partition.rs:84:20
       |
    84 |     let negative = items.iter().filter(|(value, _)| !(*value > 0)).copied().collect();
       |                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: other pass here
      --> tools/dylint/examples/manual_partition.rs:83:20
       |
    83 |     let positive = items.iter().filter(|(value, _)| *value > 0).copied().collect();
       |                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: iterate `items` once with `Iterator::partition`

    warning: two `filter` passes split `items` on one predicate and its negation
      --> tools/dylint/examples/manual_partition.rs:90:20
       |
    90 |     let rejected = items.iter().filter(|value| !(**value > 0)).collect();
       |                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: other pass here
      --> tools/dylint/examples/manual_partition.rs:89:20
       |
    89 |     let selected = items.iter().filter(|value| **value > 0).collect();
       |                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: iterate `items` once with `Iterator::partition`

    warning: two `filter` passes split `items` on one predicate and its negation
       --> tools/dylint/examples/manual_partition.rs:120:20
        |
    120 |     let rejected = Iterator::collect(Iterator::copied(Iterator::filter(<[u8]>::iter(items), |value| !(**value > 0))));
        |                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: other pass here
       --> tools/dylint/examples/manual_partition.rs:119:20
        |
    119 |     let selected = Iterator::collect(Iterator::copied(Iterator::filter(<[u8]>::iter(items), |value| **value > 0)));
        |                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: iterate `items` once with `Iterator::partition`
    ");
}

fn manual_path_is_empty(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: path emptiness is tested through `OsStr`
      --> tools/dylint/examples/manual_path_is_empty.rs:25:33
       |
    25 | fn blank(path: &Path) -> bool { path.as_os_str().is_empty() }
       |                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `Path::is_empty`: `<::std::path::Path>::is_empty(path)`
       |
       = note: `#[warn(manual_path_is_empty)]` on by default
    applicability MachineApplicable: call `Path::is_empty`

    warning: path emptiness is tested through `OsStr`
      --> tools/dylint/examples/manual_path_is_empty.rs:27:42
       |
    27 | fn blank_owned(path: &PathBuf) -> bool { path.as_os_str().is_empty() }
       |                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `Path::is_empty`: `<::std::path::Path>::is_empty(path)`
    applicability MachineApplicable: call `Path::is_empty`

    warning: path emptiness is tested through `OsStr`
      --> tools/dylint/examples/manual_path_is_empty.rs:29:47
       |
    29 | fn joined(root: &Path, child: &str) -> bool { root.join(child).as_os_str().is_empty() }
       |                                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `Path::is_empty`: `<::std::path::Path>::is_empty(&root.join(child))`
    applicability MachineApplicable: call `Path::is_empty`

    warning: path emptiness is tested through `OsStr`
      --> tools/dylint/examples/manual_path_is_empty.rs:35:37
       |
    35 | fn qualified(path: &Path) -> bool { std::ffi::OsStr::is_empty(Path::as_os_str(path)) }
       |                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `Path::is_empty`: `<::std::path::Path>::is_empty(path)`
    applicability MachineApplicable: call `Path::is_empty`

    warning: path emptiness is tested through `OsStr`
      --> tools/dylint/examples/manual_path_is_empty.rs:37:42
       |
    37 | fn projected(path: &Projected) -> bool { path.as_os_str().is_empty() }
       |                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `Path::is_empty`: `<::std::path::Path>::is_empty(path)`
    applicability MachineApplicable: call `Path::is_empty`

    warning: path emptiness is tested through `OsStr`
      --> tools/dylint/examples/manual_path_is_empty_no_std.rs:24:16
       |
    24 |     black_box((Path::new("").as_os_str().is_empty(), recent, values(2)));
       |                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call `Path::is_empty`
       = note: `#[warn(manual_path_is_empty)]` on by default
    "#);
}

fn manual_retain_back(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `recent` drops front elements by hand to keep its last `1`
      --> tools/dylint/examples/manual_path_is_empty_no_std.rs:12:5
       |
    12 | /     while recent.len() > 1 {
    13 | |         recent.pop_front();
    14 | |     }
       | |_____^
       |
       = help: call `VecDeque::retain_back`
       = note: `#[warn(manual_retain_back)]` on by default

    warning: `self.entries` drops front elements by hand to keep its last `self.limit`
      --> tools/dylint/examples/manual_retain_back.rs:53:9
       |
    53 | /         while self.entries.len() > self.limit {
    54 | |             self.entries.pop_front();
    55 | |         }
       | |_________^ help: call `VecDeque::retain_back`: `::std::collections::VecDeque::retain_back(&mut self.entries, self.limit);`
       |
       = note: `#[warn(manual_retain_back)]` on by default
    applicability MachineApplicable: call `VecDeque::retain_back`

    warning: `recent` drops front elements by hand to keep its last `kept`
      --> tools/dylint/examples/manual_retain_back.rs:62:5
       |
    62 | /     while recent.len() > kept {
    63 | |         let _ = recent.pop_front();
    64 | |     }
       | |_____^ help: call `VecDeque::retain_back`: `::std::collections::VecDeque::retain_back(&mut recent, kept);`
    applicability MachineApplicable: call `VecDeque::retain_back`

    warning: `recent` drops front elements by hand to keep its last `3`
      --> tools/dylint/examples/manual_retain_back.rs:69:5
       |
    69 | /     while 3 < recent.len() {
    70 | |         recent.pop_front();
    71 | |     }
       | |_____^ help: call `VecDeque::retain_back`: `::std::collections::VecDeque::retain_back(&mut recent, 3);`
    applicability MachineApplicable: call `VecDeque::retain_back`

    warning: `recent` drops front elements by hand to keep its last `kept`
      --> tools/dylint/examples/manual_retain_back.rs:76:5
       |
    76 |     recent.drain(..recent.len().saturating_sub(kept));
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `VecDeque::retain_back`: `::std::collections::VecDeque::retain_back(&mut recent, kept)`
    applicability MachineApplicable: call `VecDeque::retain_back`

    warning: `recent` drops front elements by hand to keep its last `kept`
      --> tools/dylint/examples/manual_retain_back.rs:83:5
       |
    83 | /     while recent.len() > kept {
    84 | |         drop(recent.pop_front());
    85 | |     }
       | |_____^ help: call `VecDeque::retain_back`: `::std::collections::VecDeque::retain_back(&mut recent, kept);`
    applicability MachineApplicable: call `VecDeque::retain_back`

    warning: `recent` drops front elements by hand to keep its last `kept`
      --> tools/dylint/examples/manual_retain_back.rs:90:5
       |
    90 | /     while recent.len() > kept {
    91 | |         _ = recent.pop_front();
    92 | |     }
       | |_____^ help: call `VecDeque::retain_back`: `::std::collections::VecDeque::retain_back(&mut recent, kept);`
    applicability MachineApplicable: call `VecDeque::retain_back`

    warning: `recent` drops front elements by hand to keep its last `kept`
       --> tools/dylint/examples/manual_retain_back.rs:107:5
        |
    107 | /     for _ in 0..recent.len().saturating_sub(kept) {
    108 | |         recent.pop_front();
    109 | |     }
        | |_____^
        |
    help: call `VecDeque::retain_back`
       --> tools/dylint/examples/manual_retain_back.rs:107:5
        |
    107 | /     for _ in 0..recent.len().saturating_sub(kept) {
    108 | |         recent.pop_front();
    109 | |     }
        | |_____^

    warning: `recent` drops front elements by hand to keep its last `kept`
       --> tools/dylint/examples/manual_retain_back.rs:114:5
        |
    114 | /     for _ in kept..recent.len() {
    115 | |         recent.pop_front();
    116 | |     }
        | |_____^
        |
    help: call `VecDeque::retain_back`
       --> tools/dylint/examples/manual_retain_back.rs:114:5
        |
    114 | /     for _ in kept..recent.len() {
    115 | |         recent.pop_front();
    116 | |     }
        | |_____^

    warning: `recent` drops front elements by hand to keep its last `kept`
       --> tools/dylint/examples/manual_retain_back.rs:121:5
        |
    121 |     while recent.len() > kept && recent.pop_front().is_some() {}
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `VecDeque::retain_back`: `::std::collections::VecDeque::retain_back(&mut recent, kept);`
    applicability MachineApplicable: call `VecDeque::retain_back`

    warning: `recent` drops front elements by hand to keep its last `kept`
       --> tools/dylint/examples/manual_retain_back.rs:132:9
        |
    130 |   macro_rules! trimmed {
    131 |       ($recent:expr, $kept:expr) => {
    132 | /         while $recent.len() > $kept {
    133 | |             $recent.pop_front();
    134 | |         }
        | |_________^
    ...
    139 |       trimmed!(recent, kept);
        |       ---------------------- in this macro invocation
        |
    help: call `VecDeque::retain_back`
       --> tools/dylint/examples/manual_retain_back.rs:132:9
        |
    132 | /         while $recent.len() > $kept {
    133 | |             $recent.pop_front();
    134 | |         }
        | |_________^
    ...
    139 |       trimmed!(recent, kept);
        |       ---------------------- in this macro invocation

    warning: `recent` drops front elements by hand to keep its last `kept`
       --> tools/dylint/examples/manual_retain_back.rs:177:5
        |
    177 | /     while recent.len() > kept {
    178 | |         {
    179 | |             {
    180 | |                 let _ = recent.pop_front();
    181 | |             };
    182 | |         };
    183 | |     }
        | |_____^ help: call `VecDeque::retain_back`: `::std::collections::VecDeque::retain_back(&mut recent, kept);`
    applicability MachineApplicable: call `VecDeque::retain_back`

    warning: `recent` drops front elements by hand to keep its last `kept`
       --> tools/dylint/examples/manual_retain_back.rs:195:5
        |
    195 | /     while VecDeque::len(&recent) > kept {
    196 | |         VecDeque::pop_front(&mut recent);
    197 | |     }
        | |_____^ help: call `VecDeque::retain_back`: `::std::collections::VecDeque::retain_back(&mut recent, kept);`
    applicability MachineApplicable: call `VecDeque::retain_back`

    warning: `recent` drops front elements by hand to keep its last `kept`
       --> tools/dylint/examples/manual_retain_back.rs:230:5
        |
    230 | /     while recent.len() > kept {
    231 | |         recent.pop_front();
    232 | |     }
        | |_____^ help: call `VecDeque::retain_back`: `::std::collections::VecDeque::retain_back(recent, kept);`
    applicability MachineApplicable: call `VecDeque::retain_back`
    ");
}

fn manual_set_operator(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `intersection` collects a set by hand
      --> tools/dylint/examples/manual_set_operator.rs:17:81
       |
    17 | fn intersected(held: &BTreeSet<u32>, stated: &BTreeSet<u32>) -> BTreeSet<u32> { held.intersection(stated).copied().collect() }
       |                                                                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: write the set operator: `held & stated`
       |
       = note: `#[warn(manual_set_operator)]` on by default
    applicability MachineApplicable: write the set operator

    warning: `union` collects a set by hand
      --> tools/dylint/examples/manual_set_operator.rs:19:74
       |
    19 | fn united(held: BTreeSet<u32>, stated: BTreeSet<u32>) -> BTreeSet<u32> { held.union(&stated).copied().collect() }
       |                                                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: write the set operator: `&held | &stated`
    applicability MachineApplicable: write the set operator

    warning: `difference` collects a set by hand
      --> tools/dylint/examples/manual_set_operator.rs:21:89
       |
    21 | fn subtracted(held: &BTreeSet<String>, stated: &BTreeSet<String>) -> BTreeSet<String> { held.difference(stated).cloned().collect() }
       |                                                                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: write the set operator: `held - stated`
    applicability MachineApplicable: write the set operator

    warning: `symmetric_difference` collects a set by hand
      --> tools/dylint/examples/manual_set_operator.rs:23:77
       |
    23 | fn toggled(held: &BTreeSet<u32>, stated: &BTreeSet<u32>) -> BTreeSet<u32> { held.symmetric_difference(stated).copied().collect() }
       |                                                                             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: write the set operator: `held ^ stated`
    applicability MachineApplicable: write the set operator

    warning: `intersection` collects a set by hand
      --> tools/dylint/examples/manual_set_operator.rs:25:73
       |
    25 | fn hashed(held: &HashSet<u32>, stated: &HashSet<u32>) -> HashSet<u32> { held.intersection(stated).copied().collect() }
       |                                                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: write the set operator: `held & stated`
    applicability MachineApplicable: write the set operator

    warning: `union` collects a set by hand
      --> tools/dylint/examples/manual_set_operator.rs:27:80
       |
    27 | fn joined(held: HashSet<String>, stated: HashSet<String>) -> HashSet<String> { held.union(&stated).cloned().collect() }
       |                                                                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: write the set operator: `&held | &stated`
    applicability MachineApplicable: write the set operator

    warning: `difference` collects a set by hand
      --> tools/dylint/examples/manual_set_operator.rs:29:74
       |
    29 | fn removed(held: &HashSet<u32>, stated: &HashSet<u32>) -> HashSet<u32> { held.difference(stated).copied().collect() }
       |                                                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: write the set operator: `held - stated`
    applicability MachineApplicable: write the set operator

    warning: `symmetric_difference` collects a set by hand
      --> tools/dylint/examples/manual_set_operator.rs:31:74
       |
    31 | fn swapped(held: &HashSet<u32>, stated: &HashSet<u32>) -> HashSet<u32> { held.symmetric_difference(stated).copied().collect() }
       |                                                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: write the set operator: `held ^ stated`
    applicability MachineApplicable: write the set operator

    warning: `intersection` collects a set by hand
      --> tools/dylint/examples/manual_set_operator.rs:33:78
       |
    33 | fn mutated(held: &mut HashSet<u32>, stated: &HashSet<u32>) -> HashSet<u32> { held.intersection(stated).copied().collect() }
       |                                                                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: write the set operator: `&*held & stated`
    applicability MachineApplicable: write the set operator

    warning: `intersection` collects a set by hand
      --> tools/dylint/examples/manual_set_operator.rs:35:81
       |
    35 | fn nested(held: &Rc<BTreeSet<u32>>, stated: &&BTreeSet<u32>) -> BTreeSet<u32> { held.intersection(stated).copied().collect() }
       |                                                                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: write the set operator: `&**held & *stated`
    applicability MachineApplicable: write the set operator

    warning: `intersection` collects a set by hand
      --> tools/dylint/examples/manual_set_operator.rs:37:78
       |
    37 | ...> { Iterator::collect(Iterator::copied(BTreeSet::intersection(&held, stated))) }
       |        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: write the set operator: `&held & stated`
    applicability MachineApplicable: write the set operator
    ");
}

fn manual_slice_as_array(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: fallible conversion checks the length of `slice` to view it as `&[u8; 4]`
      --> tools/dylint/examples/manual_slice_as_array.rs:59:47
       |
    59 | fn viewed(slice: &[u8]) -> Option<&[u8; 4]> { <&[u8; 4]>::try_from(slice).ok() }
       |                                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^-----
       |                                               |
       |                                               help: call `<[T]>::as_array`: `<[_]>::as_array::<4>(slice)`
       |
       = note: `#[warn(manual_slice_as_array)]` on by default
    applicability MachineApplicable: call `<[T]>::as_array`

    warning: fallible conversion checks the length of `slice` to view it as `&mut [u8; 2]`
      --> tools/dylint/examples/manual_slice_as_array.rs:61:59
       |
    61 | fn viewed_mut(slice: &mut [u8]) -> Option<&mut [u8; 2]> { slice.try_into().ok() }
       |                                                           ^^^^^^^^^^^^^^^^-----
       |                                                           |
       |                                                           help: call `<[T]>::as_mut_array`: `<[_]>::as_mut_array::<2>(slice)`
    applicability MachineApplicable: call `<[T]>::as_mut_array`

    warning: fallible conversion checks the length of `slice[..2]` to view it as `[u8; 2]`
      --> tools/dylint/examples/manual_slice_as_array.rs:63:46
       |
    63 | fn prefix(slice: &[u8]) -> Option<[u8; 2]> { slice[..2].try_into().ok() }
       |                                              ^^^^^^^^^^^^^^^^^^^^^-----
       |                                              |
       |                                              help: call `<[T]>::as_array`: `<[_]>::as_array::<2>(&slice[..2]).copied()`
    applicability MachineApplicable: call `<[T]>::as_array`

    warning: fallible conversion checks the length of `slice[slice.len() - 2..]` to view it as `[u8; 2]`
      --> tools/dylint/examples/manual_slice_as_array.rs:65:46
       |
    65 | fn suffix(slice: &[u8]) -> Option<[u8; 2]> { slice[slice.len() - 2..].try_into().ok() }
       |                                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^-----
       |                                              |
       |                                              help: call `<[T]>::as_array`: `<[_]>::as_array::<2>(&slice[slice.len() - 2..]).copied()`
    applicability MachineApplicable: call `<[T]>::as_array`

    warning: fallible conversion checks the length of `array[..2]` to view it as `[u8; 2]`
      --> tools/dylint/examples/manual_slice_as_array.rs:67:55
       |
    67 | fn array_prefix(array: &[u8; 4]) -> Option<[u8; 2]> { array[..2].try_into().ok() }
       |                                                       ^^^^^^^^^^^^^^^^^^^^^-----
       |                                                       |
       |                                                       help: call `<[T]>::first_chunk`: `<[_]>::first_chunk::<2>(array).copied()`
    applicability MachineApplicable: call `<[T]>::first_chunk`

    warning: fallible conversion checks the length of `array[array.len() - 2..]` to view it as `[u16; 2]`
      --> tools/dylint/examples/manual_slice_as_array.rs:69:57
       |
    69 | fn array_suffix(array: &[u16; 4]) -> Option<[u16; 2]> { array[array.len() - 2..].try_into().ok() }
       |                                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^-----
       |                                                         |
       |                                                         help: call `<[T]>::last_chunk`: `<[_]>::last_chunk::<2>(array).copied()`
    applicability MachineApplicable: call `<[T]>::last_chunk`

    warning: fallible conversion checks the length of `slice` to view it as `[u8; N]`
      --> tools/dylint/examples/manual_slice_as_array.rs:71:63
       |
    71 | fn generic<const N: usize>(slice: &[u8]) -> Option<[u8; N]> { slice.try_into().ok() }
       |                                                               ^^^^^^^^^^^^^^^^-----
       |                                                               |
       |                                                               help: call `<[T]>::as_array`: `<[_]>::as_array::<N>(slice).copied()`
    applicability MachineApplicable: call `<[T]>::as_array`

    warning: fallible conversion checks the length of `array[..r#N]` to view it as `[u8; N]`
      --> tools/dylint/examples/manual_slice_as_array.rs:73:79
       |
    73 | fn generic_prefix<const r#N: usize>(array: &[u8; r#N]) -> Option<[u8; r#N]> { array[..r#N].try_into().ok() }
       |                                                                               ^^^^^^^^^^^^^^^^^^^^^^^-----
       |                                                                               |
       |                                                                               help: call `<[T]>::first_chunk`: `<[_]>::first_chunk::<r#N>(array).copied()`
    applicability MachineApplicable: call `<[T]>::first_chunk`

    warning: fallible conversion checks the length of `slice` to view it as `[u8; 2]`
      --> tools/dylint/examples/manual_slice_as_array.rs:75:67
       |
    75 | fn required(slice: &[u8]) -> Result<[u8; 2], TryFromSliceError> { slice.try_into() }
       |                                                                   ^^^^^^^^^^^^^^^^
       |
       = help: call `<[T]>::as_array::<2>` and handle `None`

    warning: fallible conversion checks the length of `slice[..2]` to view it as `[u8; 4]`
      --> tools/dylint/examples/manual_slice_as_array.rs:81:57
       |
    81 | fn mismatched_prefix(slice: &[u8]) -> Option<[u8; 4]> { slice[..2].try_into().ok() }
       |                                                         ^^^^^^^^^^^^^^^^^^^^^-----
       |                                                         |
       |                                                         help: call `<[T]>::as_array`: `<[_]>::as_array::<4>(&slice[..2]).copied()`
    applicability MachineApplicable: call `<[T]>::as_array`

    warning: fallible conversion checks the length of `slice[slice.len() - 2..]` to view it as `[u8; 4]`
      --> tools/dylint/examples/manual_slice_as_array.rs:83:57
       |
    83 | fn mismatched_suffix(slice: &[u8]) -> Option<[u8; 4]> { slice[slice.len() - 2..].try_into().ok() }
       |                                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^-----
       |                                                         |
       |                                                         help: call `<[T]>::as_array`: `<[_]>::as_array::<4>(&slice[slice.len() - 2..]).copied()`
    applicability MachineApplicable: call `<[T]>::as_array`

    warning: fallible conversion checks the length of `slice[..length]` to view it as `[u8; 4]`
      --> tools/dylint/examples/manual_slice_as_array.rs:85:69
       |
    85 | fn dynamic_prefix(slice: &[u8], length: usize) -> Option<[u8; 4]> { slice[..length].try_into().ok() }
       |                                                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^-----
       |                                                                     |
       |                                                                     help: call `<[T]>::as_array`: `<[_]>::as_array::<4>(&slice[..length]).copied()`
    applicability MachineApplicable: call `<[T]>::as_array`

    warning: fallible conversion checks the length of `(*slice)` to view it as `[u8; 4]`
      --> tools/dylint/examples/manual_slice_as_array.rs:87:53
       |
    87 | fn dereferenced(slice: &&[u8]) -> Option<[u8; 4]> { (*slice).try_into().ok() }
       |                                                     ^^^^^^^^^^^^^^^^^^^-----
       |                                                     |
       |                                                     help: call `<[T]>::as_array`: `<[_]>::as_array::<4>((*slice)).copied()`
    applicability MachineApplicable: call `<[T]>::as_array`

    warning: fallible conversion checks the length of `(if flag { left } else { right })` to view it as `[u8; 4]`
      --> tools/dylint/examples/manual_slice_as_array.rs:89:73
       |
    89 | fn selected(left: &[u8], right: &[u8], flag: bool) -> Option<[u8; 4]> { (if flag { left } else { right }).try_into().ok() }
       |                                                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^-----
       |                                                                         |
       |                                                                         help: call `<[T]>::as_array`: `<[_]>::as_array::<4>((if flag { left } else { right })).copied()`
    applicability MachineApplicable: call `<[T]>::as_array`

    warning: fallible conversion checks the length of `(match flag {
                     true => left,
                     false => right,
                 })` to view it as `[u8; 4]`
      --> tools/dylint/examples/manual_slice_as_array.rs:92:5
       |
    92 | /     (match flag {
    93 | |         true => left,
    94 | |         false => right,
    95 | |     })
    96 | |     .try_into()
       | |_______________^
       |
    help: call `<[T]>::as_array`
       |
    92 ~     <[_]>::as_array::<4>((match flag {
    93 +         true => left,
    94 +         false => right,
    95 +     })).copied()
       |
    applicability MachineApplicable: call `<[T]>::as_array`

    warning: fallible conversion checks the length of `slice` to view it as `[u8; 4]`
       --> tools/dylint/examples/manual_slice_as_array.rs:100:49
        |
    100 | fn qualified(slice: &[u8]) -> Option<[u8; 4]> { <&[u8] as TryInto<[u8; 4]>>::try_into(slice).ok() }
        |                                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^-----
        |                                                 |
        |                                                 help: call `<[T]>::as_array`: `<[_]>::as_array::<4>(slice).copied()`
    applicability MachineApplicable: call `<[T]>::as_array`

    warning: fallible conversion checks the length of `slice` to view it as `[u8; 4]`
       --> tools/dylint/examples/manual_slice_as_array.rs:102:63
        |
    102 | fn qualified_ok(slice: &[u8]) -> Option<[u8; 4]> { Result::ok(<[u8; 4]>::try_from(slice)) }
        |                                                    -----------^^^^^^^^^^^^^^^^^^^^^^^^^^- help: call `<[T]>::as_array`: `<[_]>::as_array::<4>(slice).copied()`
    applicability MachineApplicable: call `<[T]>::as_array`

    warning: fallible conversion checks the length of `slice` to view it as `&[u8; 4]`
       --> tools/dylint/examples/manual_slice_as_array.rs:104:37
        |
    104 | fn isolated(slice: &[u8]) -> bool { <&[u8; 4]>::try_from(slice).ok().is_some() }
        |                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^-----
        |                                     |
        |                                     help: call `<[T]>::as_array`: `<[_]>::as_array::<4>(slice)`
    applicability MachineApplicable: call `<[T]>::as_array`

    warning: fallible conversion checks the length of `view` to view it as `&[u8; 4]`
       --> tools/dylint/examples/manual_slice_as_array.rs:106:54
        |
    106 | fn projected(view: &Projected) -> Option<&[u8; 4]> { <&[u8; 4] as TryFrom<&[u8]>>::try_from(view).ok() }
        |                                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^-----
        |                                                      |
        |                                                      help: call `<[T]>::as_array`: `<[_]>::as_array::<4>(view)`
    applicability MachineApplicable: call `<[T]>::as_array`

    warning: fallible conversion checks the length of `view` to view it as `[u8; 4]`
       --> tools/dylint/examples/manual_slice_as_array.rs:108:61
        |
    108 | fn projected_mut(view: &mut Projected) -> Option<[u8; 4]> { <[u8; 4] as TryFrom<&mut [u8]>>::try_from(view).ok() }
        |                                                             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^-----
        |                                                             |
        |                                                             help: call `<[T]>::as_mut_array`: `<[_]>::as_mut_array::<4>(view).copied()`
    applicability MachineApplicable: call `<[T]>::as_mut_array`

    warning: fallible conversion checks the length of `view.0[view.0.len() - 2..]` to view it as `[u16; 2]`
       --> tools/dylint/examples/manual_slice_as_array.rs:110:61
        |
    110 | fn projected_suffix(view: &FieldView) -> Option<[u16; 2]> { view.0[view.0.len() - 2..].try_into().ok() }
        |                                                             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^-----
        |                                                             |
        |                                                             help: call `<[T]>::as_array`: `<[_]>::as_array::<2>(&view.0[view.0.len() - 2..]).copied()`
    applicability MachineApplicable: call `<[T]>::as_array`

    warning: fallible conversion checks the length of `array[array.len() - 2..]` to view it as `[u8; 2]`
       --> tools/dylint/examples/manual_slice_as_array.rs:112:53
        |
    112 | fn overridden(array: &[u8; 4]) -> Option<[u8; 2]> { array[array.len() - 2..].try_into().ok() }
        |                                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^-----
        |                                                     |
        |                                                     help: call `<[T]>::as_array`: `<[_]>::as_array::<2>(&array[array.len() - 2..]).copied()`
    applicability MachineApplicable: call `<[T]>::as_array`
    ");
}

fn manual_type_changing_struct_update(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `Message` literal copies each unchanged field of `self` to change its type arguments
      --> tools/dylint/examples/manual_type_changing_struct_update.rs:25:38
       |
    25 |     fn send(self) -> Message<Sent> { Message { state: PhantomData, body: self.body, id: self.id } }
       |                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: write the changed fields and `..self` under `#![feature(type_changing_struct_update)]`: `Message { state: PhantomData, ..self }`
       |
       = note: `#[warn(manual_type_changing_struct_update)]` on by default
    applicability MaybeIncorrect: write the changed fields and `..self` under `#![feature(type_changing_struct_update)]`

    warning: `Message` literal copies each unchanged field of `self` to change its type arguments
      --> tools/dylint/examples/manual_type_changing_struct_update.rs:29:43
       |
    29 |     fn projected(self) -> Message<Sent> { Message { state: PhantomData, id: self.2, body: self.body } }
       |                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: write the changed fields and `..self` under `#![feature(type_changing_struct_update)]`: `Message { state: PhantomData, id: self.2, ..self }`
    applicability MaybeIncorrect: write the changed fields and `..self` under `#![feature(type_changing_struct_update)]`
    ");
}

fn manual_upcast(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `into_display` returns `self` as `dyn Display`, which trait upcasting converts from `dyn BoxedView`
       --> tools/dylint/examples/duplicate_body.rs:486:45
        |
    486 |         fn into_display(self: Box<Self>) -> Box<dyn Display>;
        |                                             ^^^^^^^^^^^^^^^^
        |
        = help: delete the method and coerce `dyn BoxedView` to `dyn Display` at each call site
        = note: `#[warn(manual_upcast)]` on by default

    warning: `as_display` returns `self` as `dyn Display`, which trait upcasting converts from `dyn Shape`
      --> tools/dylint/examples/manual_upcast.rs:11:29
       |
    11 |     fn as_display(&self) -> &dyn Display;
       |                             ^^^^^^^^^^^^
       |
       = help: delete the method and coerce `dyn Shape` to `dyn Display` at each call site
       = note: `#[warn(manual_upcast)]` on by default

    warning: `shared` returns `self` as `dyn Display`, which trait upcasting converts from `dyn Shape`
      --> tools/dylint/examples/manual_upcast.rs:13:25
       |
    13 |     fn shared(&self) -> &(dyn Display + Send)
       |                         ^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the method and coerce `dyn Shape` to `dyn Display` at each call site

    warning: `retained` returns `self` as `dyn Display`, which trait upcasting converts from `dyn Shape`
      --> tools/dylint/examples/manual_upcast.rs:16:27
       |
    16 |     fn retained(&self) -> &(dyn Display + 'static)
       |                           ^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the method and coerce `dyn Shape` to `dyn Display` at each call site

    warning: `as_named_display` returns `self` as `dyn Display`, which trait upcasting converts from `dyn Named`
      --> tools/dylint/examples/manual_upcast.rs:23:39
       |
    23 |     fn as_named_display(&mut self) -> &mut dyn Display;
       |                                       ^^^^^^^^^^^^^^^^
       |
       = help: delete the method and coerce `dyn Named` to `dyn Display` at each call site

    warning: `into_static` returns `self` as `dyn Display`, which trait upcasting converts from `dyn Boxed`
      --> tools/dylint/examples/manual_upcast.rs:32:40
       |
    32 |     fn into_static(self: Box<Self>) -> Box<dyn Display>
       |                                        ^^^^^^^^^^^^^^^^
       |
       = help: delete the method and coerce `dyn Boxed` to `dyn Display` at each call site

    warning: `as_shown` returns `self` as `dyn Display`, which an unsizing coercion converts
      --> tools/dylint/examples/manual_upcast.rs:54:27
       |
    54 |     fn as_shown(&self) -> &dyn Display
       |                           ^^^^^^^^^^^^
       |
       = help: delete the method and coerce the receiver to `dyn Display` at each call site

    warning: `sized_any` returns `self` as `dyn Any`, which an unsizing coercion converts
      --> tools/dylint/examples/manual_upcast.rs:59:28
       |
    59 |     fn sized_any(&self) -> &dyn Any
       |                            ^^^^^^^^
       |
       = help: delete the method and coerce the receiver to `dyn Any` at each call site

    warning: `narrow` returns `self` as `dyn Tagged`, which trait upcasting converts from `dyn TypedView`
      --> tools/dylint/examples/manual_upcast.rs:68:25
       |
    68 |     fn narrow(&self) -> &dyn Tagged<&'a u8>;
       |                         ^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the method and coerce `dyn TypedView` to `dyn Tagged` at each call site

    warning: `configured` returns `self` as `dyn Configured`, which trait upcasting converts from `dyn ConfiguredView`
      --> tools/dylint/examples/manual_upcast.rs:78:29
       |
    78 |     fn configured(&self) -> &dyn Configured<Item = Self::Item>;
       |                             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the method and coerce `dyn ConfiguredView` to `dyn Configured` at each call site

    warning: `display` returns `self` as `dyn Display`, which trait upcasting converts from `dyn Threaded`
      --> tools/dylint/examples/manual_upcast.rs:86:26
       |
    86 |     fn display(&self) -> &(dyn Display + Send);
       |                          ^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the method and coerce `dyn Threaded` to `dyn Display` at each call site

    warning: `reflected` returns `self` as `dyn Any`, which trait upcasting converts from `dyn Threaded`
      --> tools/dylint/examples/manual_upcast.rs:87:28
       |
    87 |     fn reflected(&self) -> &dyn Any;
       |                            ^^^^^^^^
       |
       = help: delete the method and coerce `dyn Threaded` to `dyn Any` at each call site

    warning: `projected` returns `self` as `dyn Display`, which trait upcasting converts from `dyn Projected`
      --> tools/dylint/examples/manual_upcast.rs:95:28
       |
    95 |     fn projected(&self) -> &dyn Display;
       |                            ^^^^^^^^^^^^
       |
       = help: delete the method and coerce `dyn Projected` to `dyn Display` at each call site
    ");
}

fn manual_vec_from_fn(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `(0..count).map(..).collect()` builds a `Vec` from each index
      --> tools/dylint/examples/manual_path_is_empty_no_std.rs:17:41
       |
    17 | fn values(count: usize) -> Vec<usize> { (0..count).map(|index| index.strict_mul(2)).collect() }
       |                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call `Vec::from_fn`
       = note: `#[warn(manual_vec_from_fn)]` on by default

    warning: `(0..count).map(..).collect()` builds a `Vec` from each index
      --> tools/dylint/examples/manual_vec_from_fn.rs:14:42
       |
    14 | fn squares(count: usize) -> Vec<usize> { (0..count).map(|index| index.strict_mul(index)).collect::<Vec<_>>() }
       |                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `Vec::from_fn`: `::std::vec::Vec::from_fn(count, |index| index.strict_mul(index))`
       |
       = note: `#[warn(manual_vec_from_fn)]` on by default
    applicability MachineApplicable: call `Vec::from_fn`

    warning: `(0..count).map(..).collect()` builds a `Vec` from each index
      --> tools/dylint/examples/manual_vec_from_fn.rs:16:42
       |
    16 | fn labels(count: usize) -> Vec<String> { (0..count).map(|index| index.to_string()).collect() }
       |                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `Vec::from_fn`: `::std::vec::Vec::from_fn(count, |index| index.to_string())`
    applicability MachineApplicable: call `Vec::from_fn`

    warning: `(0..count).map(..).collect()` builds a `Vec` from each index
      --> tools/dylint/examples/manual_vec_from_fn.rs:31:5
       |
    31 |     (0..count).map(|index| index.strict_mul(2)).collect()
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `Vec::from_fn`: `::std::vec::Vec::from_fn(count, |index| index.strict_mul(2))`
    applicability MachineApplicable: call `Vec::from_fn`
    ");
}

}
