// --- [OPERATIONS] ----------------------------------------------------------------------

fn manual_bool_toggle(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: assignment negates `flag` by hand
     --> tools/dylint/examples/manual_bool_toggle.rs:5:31
      |
    5 | fn flipped(flag: &mut bool) { *flag = !*flag; }
      |                               ^^^^^^^^^^^^^^ help: call `bool::toggle`: `flag.toggle()`
      |
      = note: `#[warn(manual_bool_toggle)]` on by default
    applicability MachineApplicable: call `bool::toggle`

    warning: assignment negates `flag` by hand
     --> tools/dylint/examples/manual_bool_toggle.rs:7:29
      |
    7 | fn xored(flag: &mut bool) { *flag ^= true; }
      |                             ^^^^^^^^^^^^^ help: call `bool::toggle`: `flag.toggle()`
    applicability MachineApplicable: call `bool::toggle`
    ");
}

fn manual_default_field_values(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `Default` for `Retry` builds each field from a constant
      --> tools/dylint/examples/manual_default_field_values.rs:30:28
       |
    30 |     fn default() -> Self { Self { attempts: 3, label: String::new() } }
       |                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: fields built here
      --> tools/dylint/examples/manual_default_field_values.rs:30:28
       |
    30 |     fn default() -> Self { Self { attempts: 3, label: String::new() } }
       |                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: declare each value on its field as `field: T = value` and derive `Default` under `#![feature(default_field_values)]`
       = note: `#[warn(manual_default_field_values)]` on by default

    warning: `Default` for `Config` calls `new`, which builds each field from its type's default
      --> tools/dylint/examples/manual_default_field_values.rs:55:28
       |
    55 |     fn default() -> Self { Self::new() }
       |                            ^^^^^^^^^^^
       |
    note: fields built here
      --> tools/dylint/examples/manual_default_field_values.rs:51:24
       |
    51 |     fn new() -> Self { Self { depth: 0, name: String::new(), enabled: false, label: "", parent: None } }
       |                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: derive `Default` and delete the hand-written impl

    warning: `Default` for `Limits` calls `new`, which builds each field from a constant
      --> tools/dylint/examples/manual_default_field_values.rs:72:28
       |
    72 |     fn default() -> Self { Self::new() }
       |                            ^^^^^^^^^^^
       |
    note: fields built here
      --> tools/dylint/examples/manual_default_field_values.rs:68:24
       |
    68 |     fn new() -> Self { Self { depth: 4, name: String::new(), enabled: false, label: "", parent: None } }
       |                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: declare each value on its field as `field: T = value` and derive `Default` under `#![feature(default_field_values)]`

    warning: `Default` for `Attributed` builds each field from a constant
      --> tools/dylint/examples/manual_default_field_values.rs:82:28
       |
    82 |     fn default() -> Self { Self { attempts: 3 } }
       |                            ^^^^^^^^^^^^^^^^^^^^
       |
    note: fields built here
      --> tools/dylint/examples/manual_default_field_values.rs:82:28
       |
    82 |     fn default() -> Self { Self { attempts: 3 } }
       |                            ^^^^^^^^^^^^^^^^^^^^
       = help: declare each value on its field as `field: T = value` and derive `Default` under `#![feature(default_field_values)]`
    "#);
}

fn manual_deref_pattern(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `as_str` derefs `&owned` for its patterns
      --> tools/dylint/examples/manual_deref_pattern.rs:15:11
       |
    15 |     match owned.as_str() {
       |           ^^^^^^^^^^^^^^ help: match a reference to the value under `#![feature(deref_patterns)]`: `&owned`
       |
       = note: `#[warn(manual_deref_pattern)]` on by default
    applicability MaybeIncorrect: match a reference to the value under `#![feature(deref_patterns)]`

    warning: `as_slice` derefs `&owned` for its patterns
      --> tools/dylint/examples/manual_deref_pattern.rs:24:30
       |
    24 |     if let [first, second] = owned.as_slice() { first.strict_add(*second) } else { 0 }
       |                              ^^^^^^^^^^^^^^^^ help: match a reference to the value under `#![feature(deref_patterns)]`: `&owned`
    applicability MaybeIncorrect: match a reference to the value under `#![feature(deref_patterns)]`

    warning: `as_slice` derefs `&owned` for its patterns
      --> tools/dylint/examples/manual_deref_pattern.rs:29:23
       |
    29 |     let [first, ..] = owned.as_slice() else { return 0 };
       |                       ^^^^^^^^^^^^^^^^ help: match a reference to the value under `#![feature(deref_patterns)]`: `&owned`
    applicability MaybeIncorrect: match a reference to the value under `#![feature(deref_patterns)]`

    warning: `as_deref` derefs `&profile.name` for its patterns
      --> tools/dylint/examples/manual_deref_pattern.rs:34:11
       |
    34 |     match profile.name.as_deref() {
       |           ^^^^^^^^^^^^^^^^^^^^^^^ help: match a reference to the value under `#![feature(deref_patterns)]`: `&profile.name`
    applicability MaybeIncorrect: match a reference to the value under `#![feature(deref_patterns)]`

    warning: `&*` derefs `value` for its patterns
      --> tools/dylint/examples/manual_deref_pattern.rs:42:11
       |
    42 |     match &**value {
       |           ^^^^^^^^ help: match a reference to the value under `#![feature(deref_patterns)]`: `value`
    applicability MaybeIncorrect: match a reference to the value under `#![feature(deref_patterns)]`

    warning: `&*` derefs `&held` for its patterns
      --> tools/dylint/examples/manual_deref_pattern.rs:51:11
       |
    51 |     match &*held {
       |           ^^^^^^ help: match a reference to the value under `#![feature(deref_patterns)]`: `&held`
    applicability MaybeIncorrect: match a reference to the value under `#![feature(deref_patterns)]`
    ");
}

fn manual_drop_guard(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `Guard` runs its closure field `F` on drop
     --> tools/dylint/examples/manual_drop_guard.rs:5:1
      |
    5 | struct Guard<F>(Option<F>)
      | ^^^^^^^^^^^^^^^
      |
      = help: hold `core::mem::DropGuard::new((), |()| ..)` and call `DropGuard::dismiss` where the guard is disarmed
      = note: `#[warn(manual_drop_guard)]` on by default

    warning: `Deferred` runs its closure field `F` on drop
      --> tools/dylint/examples/manual_drop_guard.rs:18:1
       |
    18 | struct Deferred<F>(F)
       | ^^^^^^^^^^^^^^^^^^
       |
       = help: hold `core::mem::DropGuard::new((), |()| ..)` and call `DropGuard::dismiss` where the guard is disarmed

    warning: `Manual` runs its closure field `F` on drop
      --> tools/dylint/examples/manual_drop_guard.rs:40:1
       |
    40 | struct Manual<F>(ManuallyDrop<F>)
       | ^^^^^^^^^^^^^^^^
       |
       = help: hold `core::mem::DropGuard::new((), |()| ..)` and call `DropGuard::dismiss` where the guard is disarmed
    ");
}

fn manual_fmt_from_fn(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `Joined` only formats a value at its one construction site
      --> tools/dylint/examples/manual_fmt_from_fn.rs:5:8
       |
     5 | struct Joined<'items>(&'items [u8]);
       |        ^^^^^^
       |
    note: built here
      --> tools/dylint/examples/manual_fmt_from_fn.rs:44:32
       |
    44 |     let joined = format!("{}", Joined(&values));
       |                                ^^^^^^
       = help: format at that site with `fmt::from_fn(move |formatter| ..)`
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
    ");
}

fn manual_gen_block(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `Countdown` only holds iterator state for its one construction site
      --> tools/dylint/examples/manual_gen_block.rs:5:8
       |
     5 | struct Countdown {
       |        ^^^^^^^^^
       |
    note: built here
      --> tools/dylint/examples/manual_gen_block.rs:48:23
       |
    48 |     let counted: u8 = Countdown { left: 3 }.sum();
       |                       ^^^^^^^^^^^^^^^^^^^^^
       = help: write the iteration at that site as a `gen move { .. }` block under `#![feature(gen_blocks)]`
       = note: `#[warn(manual_gen_block)]` on by default

    warning: `Restart` only holds iterator state for its one construction site
      --> tools/dylint/examples/manual_gen_block.rs:14:8
       |
    14 | struct Restart {
       |        ^^^^^^^
       |
    note: built here
      --> tools/dylint/examples/manual_gen_block.rs:50:25
       |
    50 |     let restarted: u8 = Restart { left: 4 }.sum();
       |                         ^^^^^^^^^^^^^^^^^^^
       = help: write the iteration at that site as a `gen move { .. }` block under `#![feature(gen_blocks)]`
    ");
}

fn manual_partition(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: two `filter` passes split `words.iter()` on one predicate and its negation
      --> tools/dylint/examples/manual_partition.rs:46:31
       |
    46 |     let filled = words.iter().filter(|text| !text.trim().is_empty()).copied().collect::<Vec<_>>();
       |                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: other pass here
      --> tools/dylint/examples/manual_partition.rs:45:30
       |
    45 |     let blank = words.iter().filter(|word| word.trim().is_empty()).copied().collect::<Vec<_>>();
       |                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: split once with `Iterator::partition` and read each side from its half
       = note: `#[warn(manual_partition)]` on by default
    ");
}

fn manual_path_is_empty(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `path.as_os_str().is_empty()` tests the path for emptiness through `OsStr`
     --> tools/dylint/examples/manual_path_is_empty.rs:6:33
      |
    6 | fn blank(path: &Path) -> bool { path.as_os_str().is_empty() }
      |                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `Path::is_empty`: `path.is_empty()`
      |
      = note: `#[warn(manual_path_is_empty)]` on by default
    applicability MachineApplicable: call `Path::is_empty`

    warning: `path.as_os_str().is_empty()` tests the path for emptiness through `OsStr`
     --> tools/dylint/examples/manual_path_is_empty.rs:8:42
      |
    8 | fn blank_owned(path: &PathBuf) -> bool { path.as_os_str().is_empty() }
      |                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `Path::is_empty`: `path.is_empty()`
    applicability MachineApplicable: call `Path::is_empty`

    warning: `root.join(child).as_os_str().is_empty()` tests the path for emptiness through `OsStr`
      --> tools/dylint/examples/manual_path_is_empty.rs:10:47
       |
    10 | fn joined(root: &Path, child: &str) -> bool { root.join(child).as_os_str().is_empty() }
       |                                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `Path::is_empty`: `root.join(child).is_empty()`
    applicability MachineApplicable: call `Path::is_empty`
    ");
}

fn manual_retain_back(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `self.entries` drops front elements by hand to keep its last `self.limit`
      --> tools/dylint/examples/manual_retain_back.rs:13:9
       |
    13 | /         while self.entries.len() > self.limit {
    14 | |             self.entries.pop_front();
    15 | |         }
       | |_________^ help: call `VecDeque::retain_back`: `self.entries.retain_back(self.limit);`
       |
       = note: `#[warn(manual_retain_back)]` on by default
    applicability MachineApplicable: call `VecDeque::retain_back`

    warning: `recent` drops front elements by hand to keep its last `kept`
      --> tools/dylint/examples/manual_retain_back.rs:22:5
       |
    22 | /     while recent.len() > kept {
    23 | |         let _ = recent.pop_front();
    24 | |     }
       | |_____^ help: call `VecDeque::retain_back`: `recent.retain_back(kept);`
    applicability MachineApplicable: call `VecDeque::retain_back`

    warning: `recent` drops front elements by hand to keep its last `3`
      --> tools/dylint/examples/manual_retain_back.rs:29:5
       |
    29 | /     while 3 < recent.len() {
    30 | |         recent.pop_front();
    31 | |     }
       | |_____^ help: call `VecDeque::retain_back`: `recent.retain_back(3);`
    applicability MachineApplicable: call `VecDeque::retain_back`

    warning: `recent` drops front elements by hand to keep its last `kept`
      --> tools/dylint/examples/manual_retain_back.rs:36:5
       |
    36 |     recent.drain(..recent.len().saturating_sub(kept));
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `VecDeque::retain_back`: `recent.retain_back(kept)`
    applicability MachineApplicable: call `VecDeque::retain_back`

    warning: `recent` drops front elements by hand to keep its last `kept`
      --> tools/dylint/examples/manual_retain_back.rs:43:5
       |
    43 | /     while recent.len() > kept {
    44 | |         drop(recent.pop_front());
    45 | |     }
       | |_____^ help: call `VecDeque::retain_back`: `recent.retain_back(kept);`
    applicability MachineApplicable: call `VecDeque::retain_back`

    warning: `recent` drops front elements by hand to keep its last `kept`
      --> tools/dylint/examples/manual_retain_back.rs:50:5
       |
    50 | /     while recent.len() > kept {
    51 | |         _ = recent.pop_front();
    52 | |     }
       | |_____^ help: call `VecDeque::retain_back`: `recent.retain_back(kept);`
    applicability MachineApplicable: call `VecDeque::retain_back`

    warning: `recent` drops front elements by hand to keep its last `kept`
      --> tools/dylint/examples/manual_retain_back.rs:67:5
       |
    67 | /     for _ in 0..recent.len().saturating_sub(kept) {
    68 | |         recent.pop_front();
    69 | |     }
       | |_____^
       |
    help: call `VecDeque::retain_back`
      --> tools/dylint/examples/manual_retain_back.rs:67:5
       |
    67 | /     for _ in 0..recent.len().saturating_sub(kept) {
    68 | |         recent.pop_front();
    69 | |     }
       | |_____^

    warning: `recent` drops front elements by hand to keep its last `kept`
      --> tools/dylint/examples/manual_retain_back.rs:74:5
       |
    74 | /     for _ in kept..recent.len() {
    75 | |         recent.pop_front();
    76 | |     }
       | |_____^
       |
    help: call `VecDeque::retain_back`
      --> tools/dylint/examples/manual_retain_back.rs:74:5
       |
    74 | /     for _ in kept..recent.len() {
    75 | |         recent.pop_front();
    76 | |     }
       | |_____^

    warning: `recent` drops front elements by hand to keep its last `kept`
      --> tools/dylint/examples/manual_retain_back.rs:81:5
       |
    81 |     while recent.len() > kept && recent.pop_front().is_some() {}
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `VecDeque::retain_back`: `recent.retain_back(kept);`
    applicability MachineApplicable: call `VecDeque::retain_back`

    warning: `recent` drops front elements by hand to keep its last `kept`
      --> tools/dylint/examples/manual_retain_back.rs:86:5
       |
    86 |     recent.drain(..recent.len() - kept);
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `VecDeque::retain_back`: `recent.retain_back(kept)`
    applicability MachineApplicable: call `VecDeque::retain_back`

    warning: `recent` drops front elements by hand to keep its last `kept`
      --> tools/dylint/examples/manual_retain_back.rs:92:9
       |
    90 |   macro_rules! trimmed {
    91 |       ($recent:expr, $kept:expr) => {
    92 | /         while $recent.len() > $kept {
    93 | |             $recent.pop_front();
    94 | |         }
       | |_________^
    ...
    99 |       trimmed!(recent, kept);
       |       ---------------------- in this macro invocation
       |
    help: call `VecDeque::retain_back`
      --> tools/dylint/examples/manual_retain_back.rs:92:9
       |
    92 | /         while $recent.len() > $kept {
    93 | |             $recent.pop_front();
    94 | |         }
       | |_________^
    ...
    99 |       trimmed!(recent, kept);
       |       ---------------------- in this macro invocation
    ");
}

fn manual_slice_as_array(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: fallible conversion checks the length of `slice` to view it as `&[u8; 4]`
     --> tools/dylint/examples/manual_slice_as_array.rs:5:47
      |
    5 | fn viewed(slice: &[u8]) -> Option<&[u8; 4]> { <&[u8; 4]>::try_from(slice).ok() }
      |                                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^-----
      |                                               |
      |                                               help: view the slice with the `Option`-returning method: `slice.as_array()`
      |
      = note: `#[warn(manual_slice_as_array)]` on by default
    applicability MachineApplicable: view the slice with the `Option`-returning method

    warning: fallible conversion checks the length of `slice` to view it as `&mut [u8; 2]`
     --> tools/dylint/examples/manual_slice_as_array.rs:7:59
      |
    7 | fn viewed_mut(slice: &mut [u8]) -> Option<&mut [u8; 2]> { slice.try_into().ok() }
      |                                                           ^^^^^^^^^^^^^^^^-----
      |                                                           |
      |                                                           help: view the slice with the `Option`-returning method: `slice.as_mut_array()`
    applicability MachineApplicable: view the slice with the `Option`-returning method

    warning: fallible conversion checks the length of `slice[..2]` to view it as `[u8; 2]`
     --> tools/dylint/examples/manual_slice_as_array.rs:9:46
      |
    9 | fn prefix(slice: &[u8]) -> Option<[u8; 2]> { slice[..2].try_into().ok() }
      |                                              ^^^^^^^^^^^^^^^^^^^^^-----
      |                                              |
      |                                              help: view the slice with the `Option`-returning method: `slice.first_chunk().copied()`
    applicability MaybeIncorrect: view the slice with the `Option`-returning method

    warning: fallible conversion checks the length of `slice[slice.len() - 2..]` to view it as `[u8; 2]`
      --> tools/dylint/examples/manual_slice_as_array.rs:11:46
       |
    11 | fn suffix(slice: &[u8]) -> Option<[u8; 2]> { slice[slice.len() - 2..].try_into().ok() }
       |                                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^-----
       |                                              |
       |                                              help: view the slice with the `Option`-returning method: `slice.last_chunk().copied()`
    applicability MaybeIncorrect: view the slice with the `Option`-returning method

    warning: fallible conversion checks the length of `slice` to view it as `[u8; 2]`
      --> tools/dylint/examples/manual_slice_as_array.rs:13:67
       |
    13 | fn required(slice: &[u8]) -> Result<[u8; 2], TryFromSliceError> { slice.try_into() }
       |                                                                   ^^^^^^^^^^^^^^^^ help: view the slice with the `Option`-returning method: `slice.as_array().copied()`
    applicability MaybeIncorrect: view the slice with the `Option`-returning method
    ");
}

fn manual_type_changing_struct_update(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `Message` copies each field of `self` to change its type arguments
      --> tools/dylint/examples/manual_type_changing_struct_update.rs:19:38
       |
    19 |     fn send(self) -> Message<Sent> { Message { state: PhantomData, body: self.body, id: self.id } }
       |                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: write the changed fields and `..base` under `#![feature(type_changing_struct_update)]`: `Message { state: PhantomData, ..self }`
       |
       = note: `#[warn(manual_type_changing_struct_update)]` on by default
    applicability MaybeIncorrect: write the changed fields and `..base` under `#![feature(type_changing_struct_update)]`
    ");
}

fn manual_upcast(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `as_display` returns `self` as `dyn Display`, which trait upcasting converts from `dyn Shape`
      --> tools/dylint/examples/manual_upcast.rs:11:29
       |
    11 |     fn as_display(&self) -> &dyn Display;
       |                             ^^^^^^^^^^^^
       |
       = help: delete the method and coerce `dyn Shape` to `dyn Display` at each call site
       = note: `#[warn(manual_upcast)]` on by default

    warning: `as_named_display` returns `self` as `dyn Display`, which trait upcasting converts from `dyn Named`
      --> tools/dylint/examples/manual_upcast.rs:16:39
       |
    16 |     fn as_named_display(&mut self) -> &mut dyn Display;
       |                                       ^^^^^^^^^^^^^^^^
       |
       = help: delete the method and coerce `dyn Named` to `dyn Display` at each call site

    warning: `into_display` returns `self` as `dyn Display`, which trait upcasting converts from `dyn Boxed`
      --> tools/dylint/examples/manual_upcast.rs:24:41
       |
    24 |     fn into_display(self: Box<Self>) -> Box<dyn Display>;
       |                                         ^^^^^^^^^^^^^^^^
       |
       = help: delete the method and coerce `dyn Boxed` to `dyn Display` at each call site

    warning: `as_any` returns `self` as `dyn Any`, which trait upcasting converts from `dyn Plugin` once `Any` is a supertrait
      --> tools/dylint/examples/manual_upcast.rs:36:25
       |
    36 |     fn as_any(&self) -> &dyn Any;
       |                         ^^^^^^^^
       |
       = help: add `Any` to the supertraits of `Plugin`, delete the method, and coerce `dyn Plugin` to `dyn Any` at each call site

    warning: `as_shown` returns `self` as `dyn Display`, which an unsizing coercion converts
      --> tools/dylint/examples/manual_upcast.rs:40:27
       |
    40 |     fn as_shown(&self) -> &dyn Display
       |                           ^^^^^^^^^^^^
       |
       = help: delete the method and coerce each receiver to `dyn Display` at each call site
    ");
}

fn manual_vec_from_fn(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `(0..count).map(..).collect()` builds a `Vec` from each index
     --> tools/dylint/examples/manual_vec_from_fn.rs:6:42
      |
    6 | fn squares(count: usize) -> Vec<usize> { (0..count).map(|index| index.strict_mul(index)).collect::<Vec<_>>() }
      |                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `Vec::from_fn`: `Vec::from_fn(count, |index| index.strict_mul(index))`
      |
      = note: `#[warn(manual_vec_from_fn)]` on by default
    applicability MachineApplicable: call `Vec::from_fn`

    warning: `(0..count).map(..).collect()` builds a `Vec` from each index
     --> tools/dylint/examples/manual_vec_from_fn.rs:8:42
      |
    8 | fn labels(count: usize) -> Vec<String> { (0..count).map(|index| index.to_string()).collect() }
      |                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `Vec::from_fn`: `Vec::from_fn(count, |index| index.to_string())`
    applicability MachineApplicable: call `Vec::from_fn`
    ");
}

// --- [COMPOSITION] ---------------------------------------------------------------------

snapshots!(
    manual_bool_toggle,
    manual_default_field_values,
    manual_deref_pattern,
    manual_drop_guard,
    manual_fmt_from_fn,
    manual_from_utf8_lossy_owned,
    manual_gen_block,
    manual_partition,
    manual_path_is_empty,
    manual_retain_back,
    manual_slice_as_array,
    manual_type_changing_struct_update,
    manual_upcast,
    manual_vec_from_fn
);
