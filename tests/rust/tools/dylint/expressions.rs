// --- [OPERATIONS] ----------------------------------------------------------------------

#[rustfmt::skip]
snapshots! {
fn absurd_nonzero_comparison(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `count.get() == 0` is always `false`, `std::num::NonZero<u32>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:22:41
       |
    22 | fn empty(count: NonZero<u32>) -> bool { count.get() == 0 }
       |                                         ^^^^^^^^^^^^^^^^ help: replace the comparison with `false`: `false`
       |
       = note: `#[warn(absurd_nonzero_comparison)]` on by default
    applicability MachineApplicable: replace the comparison with `false`

    warning: `count.get() != 0` is always `true`, `std::num::NonZero<u32>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:24:43
       |
    24 | fn present(count: NonZero<u32>) -> bool { count.get() != 0 }
       |                                           ^^^^^^^^^^^^^^^^ help: replace the comparison with `true`: `true`
    applicability MachineApplicable: replace the comparison with `true`

    warning: `count.get() > 0` is always `true`, `std::num::NonZero<u64>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:26:44
       |
    26 | fn positive(count: NonZero<u64>) -> bool { count.get() > 0 }
       |                                            ^^^^^^^^^^^^^^^ help: replace the comparison with `true`: `true`
    applicability MachineApplicable: replace the comparison with `true`

    warning: `1 <= count.get()` is always `true`, `std::num::NonZero<usize>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:28:43
       |
    28 | fn least(count: NonZero<usize>) -> bool { 1 <= count.get() }
       |                                           ^^^^^^^^^^^^^^^^ help: replace the comparison with `true`: `true`
    applicability MachineApplicable: replace the comparison with `true`

    warning: `count.get() < 1` is always `false`, `std::num::NonZero<u8>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:30:40
       |
    30 | fn below(count: NonZero<u8>) -> bool { count.get() < 1 }
       |                                        ^^^^^^^^^^^^^^^ help: replace the comparison with `false`: `false`
    applicability MachineApplicable: replace the comparison with `false`

    warning: `count.get() <= 0` is always `false`, `std::num::NonZero<u8>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:32:39
       |
    32 | fn most(count: NonZero<u8>) -> bool { count.get() <= 0 }
       |                                       ^^^^^^^^^^^^^^^^ help: replace the comparison with `false`: `false`
    applicability MachineApplicable: replace the comparison with `false`

    warning: `NonZero::<u32>::MIN.get() == 0` is always `false`, `std::num::NonZero<u32>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:34:24
       |
    34 | fn minimum() -> bool { NonZero::<u32>::MIN.get() == 0 }
       |                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: replace the comparison with `false`: `false`
    applicability MachineApplicable: replace the comparison with `false`

    warning: `0 < limits.retries.get()` is always `true`, `std::num::NonZero<u32>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:36:39
       |
    36 | fn retried(limits: &Limits) -> bool { 0 < limits.retries.get() }
       |                                       ^^^^^^^^^^^^^^^^^^^^^^^^ help: replace the comparison with `true`: `true`
    applicability MachineApplicable: replace the comparison with `true`

    warning: `NonZero::get(count) == 0` is always `false`, `std::num::NonZero<u16>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:38:42
       |
    38 | fn called(count: NonZero<u16>) -> bool { NonZero::get(count) == 0 }
       |                                          ^^^^^^^^^^^^^^^^^^^^^^^^ help: replace the comparison with `false`: `false`
    applicability MachineApplicable: replace the comparison with `false`

    warning: `delta.get() != 0` is always `true`, `std::num::NonZero<i32>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:42:47
       |
    42 | fn signed_zero(delta: NonZero<i32>) -> bool { delta.get() != 0 }
       |                                               ^^^^^^^^^^^^^^^^ help: replace the comparison with `true`: `true`
    applicability MachineApplicable: replace the comparison with `true`

    warning: `u32::from(count) == 0` is always `false`, `std::num::NonZero<u32>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:48:45
       |
    48 | fn converted(count: NonZero<u32>) -> bool { u32::from(count) == 0 }
       |                                             ^^^^^^^^^^^^^^^^^^^^^ help: replace the comparison with `false`: `false`
    applicability MachineApplicable: replace the comparison with `false`

    warning: `0 < Into::<u64>::into(count)` is always `true`, `std::num::NonZero<u64>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:50:50
       |
    50 | fn converted_into(count: NonZero<u64>) -> bool { 0 < Into::<u64>::into(count) }
       |                                                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: replace the comparison with `true`: `true`
    applicability MachineApplicable: replace the comparison with `true`

    warning: `count.get() == NONE` is always `false`, `std::num::NonZero<u32>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:54:47
       |
    54 | fn named_bound(count: NonZero<u32>) -> bool { count.get() == NONE }
       |                                               ^^^^^^^^^^^^^^^^^^^ help: replace the comparison with `false`: `false`
    applicability MachineApplicable: replace the comparison with `false`

    warning: `$count.get() == 0` is always `false`, `std::num::NonZero<u32>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:58:9
       |
    56 | macro_rules! empty_count {
    57 |     ($count:expr) => {
    58 |         $count.get() == 0
       |         ^^^^^^^^^^^^^^^^^
    ...
    62 | fn expanded(count: NonZero<u32>) -> bool { empty_count!(count) }
       |                                            ------------------- in this macro invocation
       |
    help: replace the comparison with `false`
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:58:9
       |
    58 |         $count.get() == 0
       |         ^^^^^^^^^^^^^^^^^
    ...
    62 | fn expanded(count: NonZero<u32>) -> bool { empty_count!(count) }
       |                                            ------------------- in this macro invocation

    warning: `values[index()].get() != 0` is always `true`, `std::num::NonZero<u32>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:66:74
       |
    66 | fn indexed(values: &[NonZero<u32>], index: impl Fn() -> usize) -> bool { values[index()].get() != 0 }
       |                                                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: retain operand evaluation and replace comparison with `true`

    warning: `value.retries.get() > 0` is always `true`, `std::num::NonZero<u32>` holds no zero
      --> tools/dylint/examples/absurd_nonzero_comparison.rs:68:46
       |
    68 | fn projected(value: &Dereferenced) -> bool { value.retries.get() > 0 }
       |                                              ^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: retain operand evaluation and replace comparison with `true`
    ");
}

fn assertions_outside_tests(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `assert!` asserts at run time outside a test
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:20:5
       |
    20 |     assert!(values.len() <= LIMIT, "too many values");
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return a typed error for a failure the input causes, state the invariant in the types, or move the check into a test or a `const` block
       = note: `#[warn(assertions_outside_tests)]` on by default

    warning: `debug_assert_eq!` asserts at run time outside a test
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:25:5
       |
    25 |     debug_assert_eq!(left.len(), right.len());
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return a typed error for a failure the input causes, state the invariant in the types, or move the check into a test or a `const` block

    warning: `assert_ne!` asserts at run time outside a test
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:30:5
       |
    30 |     assert_ne!(left, right);
       |     ^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return a typed error for a failure the input causes, state the invariant in the types, or move the check into a test or a `const` block

    warning: `assert_matches!` asserts at run time outside a test
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:35:5
       |
    35 |     core::assert_matches!(reply, Reply::Accepted(_));
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return a typed error for a failure the input causes, state the invariant in the types, or move the check into a test or a `const` block

    warning: `assert_eq!` asserts at run time outside a test
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:43:5
       |
    43 |     assert_eq!(left, right, "lengths differ");
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return a typed error for a failure the input causes, state the invariant in the types, or move the check into a test or a `const` block

    warning: `debug_assert!` asserts at run time outside a test
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:48:5
       |
    48 |     debug_assert!(!values.is_empty(), "no values");
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return a typed error for a failure the input causes, state the invariant in the types, or move the check into a test or a `const` block

    warning: `debug_assert_ne!` asserts at run time outside a test
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:49:5
       |
    49 |     debug_assert_ne!(values.len(), LIMIT, "values fill the limit");
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return a typed error for a failure the input causes, state the invariant in the types, or move the check into a test or a `const` block

    warning: `debug_assert_matches!` asserts at run time outside a test
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:54:5
       |
    54 |     core::debug_assert_matches!(reply, Reply::Rejected);
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return a typed error for a failure the input causes, state the invariant in the types, or move the check into a test or a `const` block

    warning: `assert!` asserts at run time outside a test
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:65:9
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

fn environment_override(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `with_protocol` overrides `OTEL_EXPORTER_OTLP_TRACES_PROTOCOL`, `OTEL_EXPORTER_OTLP_PROTOCOL`, which `opentelemetry_otlp` reads
      --> tools/dylint/examples/environment_override.rs:13:99
       |
    13 | ...ldError> { SpanExporter::builder().with_http().with_protocol(Protocol::HttpBinary).build() }
       |                                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the call and set `OTEL_EXPORTER_OTLP_TRACES_PROTOCOL` for traces or `OTEL_EXPORTER_OTLP_PROTOCOL` for every signal
       = note: `#[warn(environment_override)]` on by default

    warning: `with_compression` overrides `OTEL_EXPORTER_OTLP_LOGS_COMPRESSION`, `OTEL_EXPORTER_OTLP_COMPRESSION`, which `opentelemetry_otlp` reads
      --> tools/dylint/examples/environment_override.rs:15:62
       |
    15 | ...ldError> { opentelemetry_otlp::WithHttpConfig::with_compression(LogExporter::builder().with_http(), Compression::Gzip).build() }
       |               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the call and set `OTEL_EXPORTER_OTLP_LOGS_COMPRESSION` for logs or `OTEL_EXPORTER_OTLP_COMPRESSION` for every signal

    warning: `with_temporality` overrides `OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE`, which `opentelemetry_otlp` reads
      --> tools/dylint/examples/environment_override.rs:21:89
       |
    21 | fn temporal() -> Result<MetricExporter, ExporterBuildError> { MetricExporter::builder().with_temporality(Temporality::Delta).build() }
       |                                                                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the call and set `OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE` in the process environment

    warning: `with_max_links_per_span` overrides `OTEL_SPAN_LINK_COUNT_LIMIT`, which `opentelemetry_sdk` reads
      --> tools/dylint/examples/environment_override.rs:27:127
       |
    27 | ...ttributes_per_span(9).with_max_events_per_span(16).with_max_links_per_span(8).build() }
       |                                                       ^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the call and set `OTEL_SPAN_LINK_COUNT_LIMIT` in the process environment

    warning: `with_max_events_per_span` overrides `OTEL_SPAN_EVENT_COUNT_LIMIT`, which `opentelemetry_sdk` reads
      --> tools/dylint/examples/environment_override.rs:27:98
       |
    27 | fn limited() -> SdkTracerProvider { SdkTracerProvider::builder().with_max_attributes_per_span(9).with_max_events_per_span(16).with_m...
       |                                                                                                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the call and set `OTEL_SPAN_EVENT_COUNT_LIMIT` in the process environment

    warning: `with_max_attributes_per_span` overrides `OTEL_SPAN_ATTRIBUTE_COUNT_LIMIT`, which `opentelemetry_sdk` reads
      --> tools/dylint/examples/environment_override.rs:27:66
       |
    27 | fn limited() -> SdkTracerProvider { SdkTracerProvider::builder().with_max_attributes_per_span(9).with_max_events_per_span(16).with_m...
       |                                                                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the call and set `OTEL_SPAN_ATTRIBUTE_COUNT_LIMIT` in the process environment

    warning: `with_max_export_batch_size` overrides `OTEL_BLRP_MAX_EXPORT_BATCH_SIZE`, which `opentelemetry_sdk` reads
      --> tools/dylint/examples/environment_override.rs:32:70
       |
    32 | ...   logs::BatchConfigBuilder::default().with_max_queue_size(512).with_max_export_batch_size(64).with_scheduled_delay(Duration::fro...
       |                                                                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the call and set `OTEL_BLRP_MAX_EXPORT_BATCH_SIZE` in the process environment

    warning: `with_max_queue_size` overrides `OTEL_BLRP_MAX_QUEUE_SIZE`, which `opentelemetry_sdk` reads
      --> tools/dylint/examples/environment_override.rs:32:45
       |
    32 | ...   logs::BatchConfigBuilder::default().with_max_queue_size(512).with_max_export_batch_size(64).with_scheduled_delay(Duration::fro...
       |                                           ^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the call and set `OTEL_BLRP_MAX_QUEUE_SIZE` in the process environment
    ");
}

fn explicit_iter_chain_zip(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `iter` converts `left` into an iterator only to call `zip`
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:26:5
       |
    26 |     left.iter().zip(right).map(|(left, right)| left.strict_mul(*right)).sum()
       |     ^^^^^^^^^^^^^^^^^^^^^^ help: call `iter::zip` with `use core::iter;`: `iter::zip(left, right)`
       |
       = note: `#[warn(explicit_iter_chain_zip)]` on by default
    applicability MaybeIncorrect: call `iter::zip` with `use core::iter;`

    warning: `into_iter` converts `left` into an iterator only to call `chain`
      --> tools/dylint/examples/explicit_iter_chain_zip.rs:79:57
       |
    79 | fn owned(left: Vec<u32>, right: Vec<u32>) -> Vec<u32> { left.into_iter().chain(right).collect() }
       |                                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `iter::chain` with `use core::iter;`: `iter::chain(left, right)`
       |
       = note: `#[warn(explicit_iter_chain_zip)]` on by default
    applicability MaybeIncorrect: call `iter::chain` with `use core::iter;`

    warning: `iter` converts `values` into an iterator only to call `zip`
      --> tools/dylint/examples/explicit_iter_chain_zip.rs:81:55
       |
    81 | ...2]) -> u32 { values.iter().zip(weights).map(|(value, weight)| value.strict_mul(*weight)).sum() }
       |                 ^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `iter::zip` with `use core::iter;`: `iter::zip(values, weights)`
    applicability MaybeIncorrect: call `iter::zip` with `use core::iter;`

    warning: `iter` converts `values` into an iterator only to call `zip`
      --> tools/dylint/examples/explicit_iter_chain_zip.rs:85:5
       |
    85 |     values.iter().zip(weights).map(|(value, weight)| value.strict_add(*weight)).sum()
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `iter::zip` with `use core::iter;`: `iter::zip(&values, weights)`
    applicability MaybeIncorrect: call `iter::zip` with `use core::iter;`

    warning: `iter_mut` converts `values` into an iterator only to call `zip`
      --> tools/dylint/examples/explicit_iter_chain_zip.rs:90:5
       |
    90 |     values.iter_mut().zip(weights).for_each(|(value, weight)| *value = value.strict_mul(*weight));
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `iter::zip` with `use core::iter;`: `iter::zip(&mut values, weights)`
    applicability MaybeIncorrect: call `iter::zip` with `use core::iter;`

    warning: `iter` converts `map` into an iterator only to call `chain`
      --> tools/dylint/examples/explicit_iter_chain_zip.rs:94:69
       |
    94 | ... &[(u32, u32)]) -> usize { map.iter().chain(extra.iter().map(|(key, value)| (key, value))).count() }
       |                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `iter::chain` with `use core::iter;`: `iter::chain(map, extra.iter().map(|(key, value)| (key, value)))`
    applicability MaybeIncorrect: call `iter::chain` with `use core::iter;`

    warning: `iter` converts `left` into an iterator only to call `zip`
      --> tools/dylint/../../tests/rust/tools/dylint/assertions_outside_tests.rs:26:5
       |
    26 |     left.iter().zip(right).map(|(left, right)| left.strict_mul(*right)).sum()
       |     ^^^^^^^^^^^^^^^^^^^^^^ help: call `iter::zip` with `use core::iter;`: `iter::zip(left, right)`
       |
       = note: `#[warn(explicit_iter_chain_zip)]` on by default
    applicability MaybeIncorrect: call `iter::zip` with `use core::iter;`
    ");
}

fn redundant_clone_in_closure(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: closure clones `path` with no later read
      --> tools/dylint/examples/redundant_clone_in_closure.rs:38:113
       |
    38 | ...se().map_err(|source| ReadError { path: path.clone(), source }) }
       |                                      ------^^^^^^^^^^^^
       |                                      |
       |                                      help: move `path` into the closure: `path`
       |
       = note: `#[warn(redundant_clone_in_closure)]` on by default
    applicability MachineApplicable: move `path` into the closure

    warning: closure clones `name` with no later read
      --> tools/dylint/examples/redundant_clone_in_closure.rs:40:92
       |
    40 | fn fallback(name: String, given: Option<String>) -> Option<String> { given.or_else(|| Some(name.clone())) }
       |                                                                                            ^^^^^^^^^^^^ help: move `name` into the closure: `name`
    applicability MachineApplicable: move `name` into the closure

    warning: async block clones `name` with no later read
      --> tools/dylint/examples/redundant_clone_in_closure.rs:42:56
       |
    42 | fn deferred(name: String) -> String { block_on(async { name.clone() }) }
       |                                                        ^^^^^^^^^^^^ help: move `name` into the async block: `name`
    applicability MachineApplicable: move `name` into the async block

    warning: closure clones `name` with no later read
      --> tools/dylint/examples/redundant_clone_in_closure.rs:44:89
       |
    44 | fn owned(name: String, given: Option<String>) -> Option<String> { given.or_else(|| Some(name.to_owned())) }
       |                                                                                         ^^^^^^^^^^^^^^^ help: move `name` into the closure: `name`
    applicability MachineApplicable: move `name` into the closure

    warning: closure clones `name` with no later read
      --> tools/dylint/examples/redundant_clone_in_closure.rs:46:90
       |
    46 | fn called(name: String, given: Option<String>) -> Option<String> { given.or_else(|| Some(Clone::clone(&name))) }
       |                                                                                          ^^^^^^^^^^^^^^^^^^^ help: move `name` into the closure: `name`
    applicability MachineApplicable: move `name` into the closure
    ");
}

fn wrapping_arithmetic(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `u32::wrapping_mul` wraps on overflow
      --> tools/dylint/examples/renamed_function_params_in_macro.rs:21:52
       |
    19 |   macro_rules! scaled {
    20 |       ($($method:ident($argument:ident) => $body:expr;)*) => {$(
    21 |           fn $method(&self, $argument: u32) -> u32 { $body(self.0, $argument) }
       |                                                      ^^^^^^^^^^^^^^^^^^^^^^^^
    ...
    32 | /     scaled! {
    33 | |         scale(by) => u32::wrapping_mul;
    34 | |         clamp(limit) => u32::min;
    35 | |     }
       | |_____- in this macro invocation
       |
       = help: call `checked_mul` and return its `None` as an error, or `saturating_mul` to clamp at the `u32` bounds
       = note: `#[warn(wrapping_arithmetic)]` on by default

    warning: `u32::wrapping_mul` wraps on overflow
      --> tools/dylint/examples/renamed_function_params_in_macro.rs:21:52
       |
    19 |   macro_rules! scaled {
    20 |       ($($method:ident($argument:ident) => $body:expr;)*) => {$(
    21 |           fn $method(&self, $argument: u32) -> u32 { $body(self.0, $argument) }
       |                                                      ^^^^^^^^^^^^^^^^^^^^^^^^
    ...
    41 | /     scaled! {
    42 | |         scale(factor) => u32::wrapping_mul;
    43 | |         shift(_offset) => u32::saturating_sub;
    44 | |         clamp(limit) => u32::min;
    45 | |     }
       | |_____- in this macro invocation
       |
       = help: call `checked_mul` and return its `None` as an error, or `saturating_mul` to clamp at the `u32` bounds

    warning: `u32::wrapping_add` wraps on overflow
     --> tools/dylint/examples/wrapping_arithmetic.rs:5:45
      |
    5 | const fn stepped(value: u32) -> u32 { value.wrapping_add(1) }
      |                                             ^^^^^^^^^^^^^^^
      |
      = help: call `checked_add` and return its `None` as an error, or `saturating_add` to clamp at the `u32` bounds
      = note: `#[warn(wrapping_arithmetic)]` on by default

    warning: `i64::overflowing_mul` wraps on overflow
     --> tools/dylint/examples/wrapping_arithmetic.rs:7:53
      |
    7 | const fn flagged(value: i64) -> (i64, bool) { value.overflowing_mul(3) }
      |                                                     ^^^^^^^^^^^^^^^^^^
      |
      = help: call `checked_mul` and return its `None` as an error, or `saturating_mul` to clamp at the `i64` bounds

    warning: `u32::wrapping_add` wraps on overflow
     --> tools/dylint/examples/wrapping_arithmetic.rs:9:41
      |
    9 | const fn qualified(value: u32) -> u32 { u32::wrapping_add(value, 1) }
      |                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^
      |
      = help: call `checked_add` and return its `None` as an error, or `saturating_add` to clamp at the `u32` bounds

    warning: `i64::overflowing_mul` wraps on overflow
      --> tools/dylint/examples/wrapping_arithmetic.rs:11:58
       |
    11 | const fn qualified_overflow(value: i64) -> (i64, bool) { i64::overflowing_mul(value, 3) }
       |                                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call `checked_mul` and return its `None` as an error, or `saturating_mul` to clamp at the `i64` bounds

    warning: `+` on `Wrapping<u8>` wraps on overflow
      --> tools/dylint/examples/wrapping_arithmetic.rs:13:41
       |
    13 | fn wrapped(value: u8) -> Wrapping<u8> { Wrapping(value) + Wrapping(1) }
       |                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: hold `u8` in place of `Wrapping<u8>`, call `checked_add` and return its `None` as an error, or `saturating_add` to clamp at the `u8` bounds

    warning: `+` on `Wrapping<i16>` wraps on overflow
      --> tools/dylint/examples/wrapping_arithmetic.rs:15:79
       |
    15 | fn referenced(left: &Wrapping<i16>, right: &Wrapping<i16>) -> Wrapping<i16> { left + right }
       |                                                                               ^^^^^^^^^^^^
       |
       = help: hold `i16` in place of `Wrapping<i16>`, call `checked_add` and return its `None` as an error, or `saturating_add` to clamp at the `i16` bounds

    warning: `-` on `Wrapping<i16>` wraps on overflow
      --> tools/dylint/examples/wrapping_arithmetic.rs:17:66
       |
    17 | fn referenced_negative(value: &Wrapping<i16>) -> Wrapping<i16> { -value }
       |                                                                  ^^^^^^
       |
       = help: hold `i16` in place of `Wrapping<i16>`, call `checked_neg` and return its `None` as an error, or `saturating_neg` to clamp at the `i16` bounds

    warning: `u16::wrapping_sub` wraps on overflow
      --> tools/dylint/examples/wrapping_arithmetic.rs:27:16
       |
    25 | macro_rules! bumped {
    26 |     ($value:expr) => {
    27 |         $value.wrapping_sub(1)
       |                ^^^^^^^^^^^^^^^
    ...
    31 | const fn expanded(value: u16) -> u16 { bumped!(value) }
       |                                        -------------- in this macro invocation
       |
       = help: call `checked_sub` and return its `None` as an error, or `saturating_sub` to clamp at the `u16` bounds

    warning: `u32::wrapping_add_signed` wraps on overflow
      --> tools/dylint/examples/wrapping_arithmetic.rs:41:44
       |
    41 | const fn signed(value: u32) -> u32 { value.wrapping_add_signed(-1) }
       |                                            ^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call `checked_add_signed` and return its `None` as an error, or `saturating_add_signed` to clamp at the `u32` bounds

    warning: `+=` on `Wrapping<i8>` wraps on overflow
      --> tools/dylint/examples/wrapping_arithmetic.rs:44:5
       |
    44 |     total += Wrapping(1);
       |     ^^^^^^^^^^^^^^^^^^^^
       |
       = help: hold `i8` in place of `Wrapping<i8>`, call `checked_add` and return its `None` as an error, or `saturating_add` to clamp at the `i8` bounds

    warning: `-` on `Wrapping<i8>` wraps on overflow
      --> tools/dylint/examples/wrapping_arithmetic.rs:45:5
       |
    45 |     -total
       |     ^^^^^^
       |
       = help: hold `i8` in place of `Wrapping<i8>`, call `checked_neg` and return its `None` as an error, or `saturating_neg` to clamp at the `i8` bounds
    ");
}

}
