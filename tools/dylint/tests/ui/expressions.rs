// --- [OPERATIONS] ----------------------------------------------------------------------

fn absurd_nonzero_comparison(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `count.get() == 0` is always `false`, `std::num::NonZero<u32>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:11:41
       |
    11 | fn empty(count: NonZero<u32>) -> bool { count.get() == 0 }
       |                                         ^^^^^^^^^^^^^^^^ help: replace the comparison with `false`: `false`
       |
       = note: `#[warn(absurd_nonzero_comparison)]` on by default
    applicability MachineApplicable: replace the comparison with `false`

    warning: `count.get() != 0` is always `true`, `std::num::NonZero<u32>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:13:43
       |
    13 | fn present(count: NonZero<u32>) -> bool { count.get() != 0 }
       |                                           ^^^^^^^^^^^^^^^^ help: replace the comparison with `true`: `true`
    applicability MachineApplicable: replace the comparison with `true`

    warning: `count.get() > 0` is always `true`, `std::num::NonZero<u64>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:15:44
       |
    15 | fn positive(count: NonZero<u64>) -> bool { count.get() > 0 }
       |                                            ^^^^^^^^^^^^^^^ help: replace the comparison with `true`: `true`
    applicability MachineApplicable: replace the comparison with `true`

    warning: `1 <= count.get()` is always `true`, `std::num::NonZero<usize>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:17:43
       |
    17 | fn least(count: NonZero<usize>) -> bool { 1 <= count.get() }
       |                                           ^^^^^^^^^^^^^^^^ help: replace the comparison with `true`: `true`
    applicability MachineApplicable: replace the comparison with `true`

    warning: `count.get() < 1` is always `false`, `std::num::NonZero<u8>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:19:40
       |
    19 | fn below(count: NonZero<u8>) -> bool { count.get() < 1 }
       |                                        ^^^^^^^^^^^^^^^ help: replace the comparison with `false`: `false`
    applicability MachineApplicable: replace the comparison with `false`

    warning: `count.get() <= 0` is always `false`, `std::num::NonZero<u8>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:21:39
       |
    21 | fn most(count: NonZero<u8>) -> bool { count.get() <= 0 }
       |                                       ^^^^^^^^^^^^^^^^ help: replace the comparison with `false`: `false`
    applicability MachineApplicable: replace the comparison with `false`

    warning: `NonZero::<u32>::MIN.get() == 0` is always `false`, `std::num::NonZero<u32>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:23:24
       |
    23 | fn minimum() -> bool { NonZero::<u32>::MIN.get() == 0 }
       |                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: replace the comparison with `false`: `false`
    applicability MaybeIncorrect: replace the comparison with `false`

    warning: `0 < limits.retries.get()` is always `true`, `std::num::NonZero<u32>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:25:39
       |
    25 | fn retried(limits: &Limits) -> bool { 0 < limits.retries.get() }
       |                                       ^^^^^^^^^^^^^^^^^^^^^^^^ help: replace the comparison with `true`: `true`
    applicability MachineApplicable: replace the comparison with `true`

    warning: `NonZero::get(count) == 0` is always `false`, `std::num::NonZero<u16>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:27:42
       |
    27 | fn called(count: NonZero<u16>) -> bool { NonZero::get(count) == 0 }
       |                                          ^^^^^^^^^^^^^^^^^^^^^^^^ help: replace the comparison with `false`: `false`
    applicability MachineApplicable: replace the comparison with `false`

    warning: `delta.get() != 0` is always `true`, `std::num::NonZero<i32>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:31:47
       |
    31 | fn signed_zero(delta: NonZero<i32>) -> bool { delta.get() != 0 }
       |                                               ^^^^^^^^^^^^^^^^ help: replace the comparison with `true`: `true`
    applicability MachineApplicable: replace the comparison with `true`

    warning: `u32::from(count) == 0` is always `false`, `std::num::NonZero<u32>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:37:45
       |
    37 | fn converted(count: NonZero<u32>) -> bool { u32::from(count) == 0 }
       |                                             ^^^^^^^^^^^^^^^^^^^^^ help: replace the comparison with `false`: `false`
    applicability MachineApplicable: replace the comparison with `false`

    warning: `0 < Into::<u64>::into(count)` is always `true`, `std::num::NonZero<u64>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:39:50
       |
    39 | fn converted_into(count: NonZero<u64>) -> bool { 0 < Into::<u64>::into(count) }
       |                                                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: replace the comparison with `true`: `true`
    applicability MachineApplicable: replace the comparison with `true`

    warning: `count.get() == NONE` is always `false`, `std::num::NonZero<u32>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:43:47
       |
    43 | fn named_bound(count: NonZero<u32>) -> bool { count.get() == NONE }
       |                                               ^^^^^^^^^^^^^^^^^^^ help: replace the comparison with `false`: `false`
    applicability MachineApplicable: replace the comparison with `false`

    warning: `$count.get() == 0` is always `false`, `std::num::NonZero<u32>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:47:9
       |
    45 | macro_rules! empty_count {
    46 |     ($count:expr) => {
    47 |         $count.get() == 0
       |         ^^^^^^^^^^^^^^^^^
    ...
    51 | fn expanded(count: NonZero<u32>) -> bool { empty_count!(count) }
       |                                            ------------------- in this macro invocation
       |
    help: replace the comparison with `false`
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:47:9
       |
    47 |         $count.get() == 0
       |         ^^^^^^^^^^^^^^^^^
    ...
    51 | fn expanded(count: NonZero<u32>) -> bool { empty_count!(count) }
       |                                            ------------------- in this macro invocation
    ");
}

fn assertions_outside_tests(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `assert!` asserts at run time outside a test
      --> tools/dylint/examples/assertions_outside_tests.rs:20:5
       |
    20 |     assert!(values.len() <= LIMIT, "too many values");
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return a typed error for a failure the input causes, state the invariant in the types, or move the check into a test or a `const` block
       = note: `#[warn(assertions_outside_tests)]` on by default

    warning: `debug_assert_eq!` asserts at run time outside a test
      --> tools/dylint/examples/assertions_outside_tests.rs:25:5
       |
    25 |     debug_assert_eq!(left.len(), right.len());
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return a typed error for a failure the input causes, state the invariant in the types, or move the check into a test or a `const` block

    warning: `assert_ne!` asserts at run time outside a test
      --> tools/dylint/examples/assertions_outside_tests.rs:30:5
       |
    30 |     assert_ne!(left, right);
       |     ^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return a typed error for a failure the input causes, state the invariant in the types, or move the check into a test or a `const` block

    warning: `assert_matches!` asserts at run time outside a test
      --> tools/dylint/examples/assertions_outside_tests.rs:35:5
       |
    35 |     core::assert_matches!(reply, Reply::Accepted(_));
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return a typed error for a failure the input causes, state the invariant in the types, or move the check into a test or a `const` block

    warning: `assert_eq!` asserts at run time outside a test
      --> tools/dylint/examples/assertions_outside_tests.rs:43:5
       |
    43 |     assert_eq!(left, right, "lengths differ");
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return a typed error for a failure the input causes, state the invariant in the types, or move the check into a test or a `const` block

    warning: `debug_assert!` asserts at run time outside a test
      --> tools/dylint/examples/assertions_outside_tests.rs:48:5
       |
    48 |     debug_assert!(!values.is_empty(), "no values");
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return a typed error for a failure the input causes, state the invariant in the types, or move the check into a test or a `const` block

    warning: `debug_assert_ne!` asserts at run time outside a test
      --> tools/dylint/examples/assertions_outside_tests.rs:49:5
       |
    49 |     debug_assert_ne!(values.len(), LIMIT, "values fill the limit");
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return a typed error for a failure the input causes, state the invariant in the types, or move the check into a test or a `const` block

    warning: `debug_assert_matches!` asserts at run time outside a test
      --> tools/dylint/examples/assertions_outside_tests.rs:54:5
       |
    54 |     core::debug_assert_matches!(reply, Reply::Rejected);
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return a typed error for a failure the input causes, state the invariant in the types, or move the check into a test or a `const` block

    warning: `assert!` asserts at run time outside a test
      --> tools/dylint/examples/assertions_outside_tests.rs:65:9
       |
    63 | macro_rules! ensured {
    64 |     ($condition:expr) => {
    65 |         assert!($condition, "broken")
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    ...
    70 |     ensured!(count < LIMIT);
       |     ----------------------- in this macro invocation
       |
       = help: return a typed error for a failure the input causes, state the invariant in the types, or move the check into a test or a `const` block
    "#);
}

fn cloned_command_matches(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `try_get_matches_from` consumes a clone of `command`
      --> tools/dylint/examples/cloned_command_matches.rs:14:87
       |
    14 | fn parsed(command: &Command, arguments: &[&str]) -> Result<ArgMatches, clap::Error> { command.clone().try_get_matches_from(arguments) }
       |                                                                                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: take the command by `&mut` and call `get_matches_mut()`, or `try_get_matches_from_mut(<arguments>)` for the `_from` methods
       = note: `#[warn(cloned_command_matches)]` on by default

    warning: `try_get_matches_from` consumes a clone of `cli.command`
      --> tools/dylint/examples/cloned_command_matches.rs:16:80
       |
    16 | fn fielded(cli: &Cli, arguments: &[&str]) -> Result<ArgMatches, clap::Error> { cli.command.clone().try_get_matches_from(arguments) }
       |                                                                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: take the command by `&mut` and call `get_matches_mut()`, or `try_get_matches_from_mut(<arguments>)` for the `_from` methods
    ");
}

fn environment_override(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `with_timeout` overrides `OTEL_EXPORTER_OTLP_TIMEOUT`, which `opentelemetry_otlp` reads
     --> tools/dylint/examples/environment_override.rs:8:126
      |
    8 | ...1/traces").with_protocol(Protocol::HttpBinary).with_timeout(Duration::from_secs(5)).build()
      |                                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
      |
      = help: delete the call and set `OTEL_EXPORTER_OTLP_TIMEOUT` in the process environment
      = note: `#[warn(environment_override)]` on by default

    warning: `with_protocol` overrides `OTEL_EXPORTER_OTLP_PROTOCOL`, which `opentelemetry_otlp` reads
     --> tools/dylint/examples/environment_override.rs:8:90
      |
    8 |     SpanExporter::builder().with_http().with_endpoint("http://127.0.0.1:4318/v1/traces").with_protocol(Protocol::HttpBinary).with_tim...
      |                                                                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
      |
      = help: delete the call and set `OTEL_EXPORTER_OTLP_PROTOCOL` in the process environment

    warning: `with_endpoint` overrides `OTEL_EXPORTER_OTLP_ENDPOINT`, which `opentelemetry_otlp` reads
     --> tools/dylint/examples/environment_override.rs:8:41
      |
    8 |     SpanExporter::builder().with_http().with_endpoint("http://127.0.0.1:4318/v1/traces").with_protocol(Protocol::HttpBinary).with_tim...
      |                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
      |
      = help: delete the call and set `OTEL_EXPORTER_OTLP_ENDPOINT` in the process environment

    warning: `with_endpoint` overrides `OTEL_EXPORTER_OTLP_ENDPOINT`, which `opentelemetry_otlp` reads
      --> tools/dylint/examples/environment_override.rs:11:61
       |
    11 | ...> { opentelemetry_otlp::WithExportConfig::with_endpoint(LogExporter::builder().with_http(), "http://127.0.0.1:4318/v1/logs").buil...
       |        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the call and set `OTEL_EXPORTER_OTLP_ENDPOINT` in the process environment
    "#);
}

fn map_indexing(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: indexing `BTreeMap<u32, u16>` by `&1` panics when the key is absent
     --> tools/dylint/examples/map_indexing.rs:8:49
      |
    8 | fn ordered(ports: &BTreeMap<u32, u16>) -> u16 { ports[&1] }
      |                                                 ^^^^^^^^^
      |
      = help: call `get` and handle `None`
      = note: `#[warn(map_indexing)]` on by default

    warning: indexing `HashMap<&str, u16>` by `"http"` panics when the key is absent
      --> tools/dylint/examples/map_indexing.rs:10:48
       |
    10 | fn hashed(ports: &HashMap<&str, u16>) -> u16 { ports["http"] }
       |                                                ^^^^^^^^^^^^^
       |
       = help: call `get` and handle `None`

    warning: indexing `IndexMap<u32, u16>` by `&2` panics when the key is absent
      --> tools/dylint/examples/map_indexing.rs:12:50
       |
    12 | fn inserted(ports: &IndexMap<u32, u16>) -> u16 { ports[&2] }
       |                                                  ^^^^^^^^^
       |
       = help: call `get` and handle `None`

    warning: indexing `BTreeMap<u32, u16>` by `&3` panics when the key is absent
      --> tools/dylint/examples/map_indexing.rs:20:9
       |
    18 | macro_rules! port {
    19 |     ($ports:expr) => {
    20 |         $ports[&3]
       |         ^^^^^^^^^^
    ...
    24 | fn expanded(ports: &BTreeMap<u32, u16>) -> u16 { port!(ports) }
       |                                                  ------------ in this macro invocation
       |
       = help: call `get` and handle `None`

    warning: indexing `BTreeMap<&str, usize>` by `"err"` panics when the key is absent
       --> tools/dylint/examples/stringly_typed_value.rs:151:61
        |
    151 |     tally.get("ok").copied().unwrap_or_default().strict_add(tally["err"])
        |                                                             ^^^^^^^^^^^^
        |
        = help: call `get` and handle `None`
        = note: `#[warn(map_indexing)]` on by default
    "#);
}

fn redundant_clone_in_closure(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: closure clones `path`, which nothing reads after the closure
      --> tools/dylint/examples/redundant_clone_in_closure.rs:24:113
       |
    24 | ...se().map_err(|source| ReadError { path: path.clone(), source }) }
       |                                      ------^^^^^^^^^^^^
       |                                      |
       |                                      help: move `path` into the closure: `path`
       |
       = note: `#[warn(redundant_clone_in_closure)]` on by default
    applicability MachineApplicable: move `path` into the closure

    warning: closure clones `name`, which nothing reads after the closure
      --> tools/dylint/examples/redundant_clone_in_closure.rs:26:92
       |
    26 | fn fallback(name: String, given: Option<String>) -> Option<String> { given.or_else(|| Some(name.clone())) }
       |                                                                                            ^^^^^^^^^^^^ help: move `name` into the closure: `name`
    applicability MachineApplicable: move `name` into the closure

    warning: closure clones `name`, which nothing reads after the closure
      --> tools/dylint/examples/redundant_clone_in_closure.rs:28:56
       |
    28 | fn deferred(name: String) -> String { block_on(async { name.clone() }) }
       |                                                        ^^^^^^^^^^^^ help: move `name` into the closure: `name`
    applicability MachineApplicable: move `name` into the closure

    warning: closure clones `name`, which nothing reads after the closure
      --> tools/dylint/examples/redundant_clone_in_closure.rs:30:89
       |
    30 | fn owned(name: String, given: Option<String>) -> Option<String> { given.or_else(|| Some(name.to_owned())) }
       |                                                                                         ^^^^^^^^^^^^^^^ help: move `name` into the closure: `name`
    applicability MachineApplicable: move `name` into the closure

    warning: closure clones `name`, which nothing reads after the closure
      --> tools/dylint/examples/redundant_clone_in_closure.rs:32:90
       |
    32 | fn called(name: String, given: Option<String>) -> Option<String> { given.or_else(|| Some(Clone::clone(&name))) }
       |                                                                                          ^^^^^^^^^^^^^^^^^^^ help: move `name` into the closure: `name`
    applicability MachineApplicable: move `name` into the closure
    ");
}

fn redundant_default_base(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `..Default::default()` fills only `Limits` fields with declared default values
      --> tools/dylint/examples/redundant_default_base.rs:32:54
       |
    32 | fn limits(name: String) -> Limits { Limits { name, ..Default::default() } }
       |                                                      ^^^^^^^^^^^^^^^^^^ help: write `..` to take each field's declared default
       |
       = note: `#[warn(redundant_default_base)]` on by default
    applicability MachineApplicable: write `..` to take each field's declared default

    warning: `..Default::default()` fills only `Retry` fields with declared default values
      --> tools/dylint/examples/redundant_default_base.rs:36:46
       |
    36 | fn retry() -> Retry { Retry { attempts: 4, ..Default::default() } }
       |                                              ^^^^^^^^^^^^^^^^^^ help: write `..` to take each field's declared default
    applicability MaybeIncorrect: write `..` to take each field's declared default
    ");
}

fn wrapping_arithmetic(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `wrapping_add` wraps on overflow
     --> tools/dylint/examples/wrapping_arithmetic.rs:5:45
      |
    5 | const fn stepped(value: u32) -> u32 { value.wrapping_add(1) }
      |                                             ^^^^^^^^^^^^^^^
      |
      = help: call `checked_*` and return overflow as an error, or `saturating_*` where the type limit bounds the domain
      = note: `#[warn(wrapping_arithmetic)]` on by default

    warning: `overflowing_mul` wraps on overflow
     --> tools/dylint/examples/wrapping_arithmetic.rs:7:53
      |
    7 | const fn flagged(value: i64) -> (i64, bool) { value.overflowing_mul(3) }
      |                                                     ^^^^^^^^^^^^^^^^^^
      |
      = help: call `checked_*` and return overflow as an error, or `saturating_*` where the type limit bounds the domain

    warning: `Wrapping` arithmetic wraps on overflow
     --> tools/dylint/examples/wrapping_arithmetic.rs:9:41
      |
    9 | fn wrapped(value: u8) -> Wrapping<u8> { Wrapping(value) + Wrapping(1) }
      |                                         ^^^^^^^^^^^^^^^
      |
      = help: call `checked_*` and return overflow as an error, or `saturating_*` where the type limit bounds the domain

    warning: `Wrapping` arithmetic wraps on overflow
     --> tools/dylint/examples/wrapping_arithmetic.rs:9:59
      |
    9 | fn wrapped(value: u8) -> Wrapping<u8> { Wrapping(value) + Wrapping(1) }
      |                                                           ^^^^^^^^^^^
      |
      = help: call `checked_*` and return overflow as an error, or `saturating_*` where the type limit bounds the domain

    warning: `wrapping_sub` wraps on overflow
      --> tools/dylint/examples/wrapping_arithmetic.rs:19:16
       |
    17 | macro_rules! bumped {
    18 |     ($value:expr) => {
    19 |         $value.wrapping_sub(1)
       |                ^^^^^^^^^^^^^^^
    ...
    23 | const fn expanded(value: u16) -> u16 { bumped!(value) }
       |                                        -------------- in this macro invocation
       |
       = help: call `checked_*` and return overflow as an error, or `saturating_*` where the type limit bounds the domain
    ");
}

// --- [COMPOSITION] ---------------------------------------------------------------------

snapshots!(absurd_nonzero_comparison, assertions_outside_tests, cloned_command_matches, environment_override, map_indexing, redundant_clone_in_closure, redundant_default_base, wrapping_arithmetic);
