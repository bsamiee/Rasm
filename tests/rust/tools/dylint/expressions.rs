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

fn borrowed_indexing(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: indexing `BTreeMap<u32, u16>` by `&1` panics when the entry is absent
     --> tools/dylint/examples/borrowed_indexing.rs:8:49
      |
    8 | fn ordered(ports: &BTreeMap<u32, u16>) -> u16 { ports[&1] }
      |                                                 ^^^^^^^^^
      |
      = help: call `ports.get(&1)` and handle `None`
      = note: `#[warn(borrowed_indexing)]` on by default

    warning: indexing `HashMap<&str, u16>` by `"http"` panics when the entry is absent
      --> tools/dylint/examples/borrowed_indexing.rs:10:48
       |
    10 | fn hashed(ports: &HashMap<&str, u16>) -> u16 { ports["http"] }
       |                                                ^^^^^^^^^^^^^
       |
       = help: call `ports.get("http")` and handle `None`

    warning: indexing `IndexMap<u32, u16>` by `&2` panics when the entry is absent
      --> tools/dylint/examples/borrowed_indexing.rs:12:50
       |
    12 | fn inserted(ports: &IndexMap<u32, u16>) -> u16 { ports[&2] }
       |                                                  ^^^^^^^^^
       |
       = help: call `ports.get(&2)` and handle `None`

    warning: indexing `BTreeMap<u32, u16>` by `&3` panics when the entry is absent
      --> tools/dylint/examples/borrowed_indexing.rs:28:9
       |
    26 | macro_rules! port {
    27 |     ($ports:expr) => {
    28 |         $ports[&3]
       |         ^^^^^^^^^^
    ...
    32 | fn expanded(ports: &BTreeMap<u32, u16>) -> u16 { port!(ports) }
       |                                                  ------------ in this macro invocation
       |
       = help: call `ports.get(&3)` and handle `None`

    warning: indexing `VecDeque<u16>` by `3` panics when the entry is absent
      --> tools/dylint/examples/borrowed_indexing.rs:51:43
       |
    51 | fn queued(queue: &VecDeque<u16>) -> u16 { queue[3] }
       |                                           ^^^^^^^^
       |
       = help: call `queue.get(3)` and handle `None`
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
       = help: call `try_get_matches_from_mut(arguments)` on a mutable borrow of `command`
       = note: `#[warn(cloned_command_matches)]` on by default

    warning: `try_get_matches_from` consumes a clone of `cli.command`
      --> tools/dylint/examples/cloned_command_matches.rs:16:80
       |
    16 | fn fielded(cli: &Cli, arguments: &[&str]) -> Result<ArgMatches, clap::Error> { cli.command.clone().try_get_matches_from(arguments) }
       |                                                                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call `try_get_matches_from_mut(arguments)` on a mutable borrow of `cli.command`

    warning: `try_get_matches_from` consumes a clone of `command`
      --> tools/dylint/examples/cloned_command_matches.rs:25:5
       |
    25 |     copy.try_get_matches_from(arguments)
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call `try_get_matches_from_mut(arguments)` on a mutable borrow of `command`
    ");
}

fn environment_override(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `with_timeout` overrides `OTEL_EXPORTER_OTLP_TRACES_TIMEOUT`, `OTEL_EXPORTER_OTLP_TIMEOUT`, which `opentelemetry_otlp` reads
      --> tools/dylint/examples/environment_override.rs:14:126
       |
    14 | .../traces").with_protocol(Protocol::HttpBinary).with_timeout(Duration::from_secs(5)).build()
       |                                                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the call and set `OTEL_EXPORTER_OTLP_TRACES_TIMEOUT` for traces or `OTEL_EXPORTER_OTLP_TIMEOUT` for every signal
       = note: `#[warn(environment_override)]` on by default

    warning: `with_protocol` overrides `OTEL_EXPORTER_OTLP_TRACES_PROTOCOL`, `OTEL_EXPORTER_OTLP_PROTOCOL`, which `opentelemetry_otlp` reads
      --> tools/dylint/examples/environment_override.rs:14:90
       |
    14 |     SpanExporter::builder().with_http().with_endpoint("http://127.0.0.1:4318/v1/traces").with_protocol(Protocol::HttpBinary).with_ti...
       |                                                                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the call and set `OTEL_EXPORTER_OTLP_TRACES_PROTOCOL` for traces or `OTEL_EXPORTER_OTLP_PROTOCOL` for every signal

    warning: `with_endpoint` overrides `OTEL_EXPORTER_OTLP_TRACES_ENDPOINT`, `OTEL_EXPORTER_OTLP_ENDPOINT`, which `opentelemetry_otlp` reads
      --> tools/dylint/examples/environment_override.rs:14:41
       |
    14 |     SpanExporter::builder().with_http().with_endpoint("http://127.0.0.1:4318/v1/traces").with_protocol(Protocol::HttpBinary).with_ti...
       |                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the call and set `OTEL_EXPORTER_OTLP_TRACES_ENDPOINT` for traces or `OTEL_EXPORTER_OTLP_ENDPOINT` for every signal

    warning: `with_endpoint` overrides `OTEL_EXPORTER_OTLP_LOGS_ENDPOINT`, `OTEL_EXPORTER_OTLP_ENDPOINT`, which `opentelemetry_otlp` reads
      --> tools/dylint/examples/environment_override.rs:17:61
       |
    17 | ...> { opentelemetry_otlp::WithExportConfig::with_endpoint(LogExporter::builder().with_http(), "http://127.0.0.1:4318/v1/logs").buil...
       |        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the call and set `OTEL_EXPORTER_OTLP_LOGS_ENDPOINT` for logs or `OTEL_EXPORTER_OTLP_ENDPOINT` for every signal

    warning: `with_temporality` overrides `OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE`, which `opentelemetry_otlp` reads
      --> tools/dylint/examples/environment_override.rs:19:89
       |
    19 | fn temporal() -> Result<MetricExporter, ExporterBuildError> { MetricExporter::builder().with_temporality(Temporality::Delta).build() }
       |                                                                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the call and set `OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE` in the process environment

    warning: `with_span_limits` overrides `OTEL_SPAN_ATTRIBUTE_COUNT_LIMIT`, `OTEL_SPAN_EVENT_COUNT_LIMIT`, `OTEL_SPAN_LINK_COUNT_LIMIT`, which `opentelemetry_sdk` reads
      --> tools/dylint/examples/environment_override.rs:21:127
       |
    21 | ...pler::AlwaysOn).with_max_events_per_span(16).with_span_limits(SpanLimits::default()).build() }
       |                                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the call and set `OTEL_SPAN_ATTRIBUTE_COUNT_LIMIT`, `OTEL_SPAN_EVENT_COUNT_LIMIT`, `OTEL_SPAN_LINK_COUNT_LIMIT` in the process environment

    warning: `with_max_events_per_span` overrides `OTEL_SPAN_EVENT_COUNT_LIMIT`, which `opentelemetry_sdk` reads
      --> tools/dylint/examples/environment_override.rs:21:98
       |
    21 | fn sampled() -> SdkTracerProvider { SdkTracerProvider::builder().with_sampler(Sampler::AlwaysOn).with_max_events_per_span(16).with_s...
       |                                                                                                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the call and set `OTEL_SPAN_EVENT_COUNT_LIMIT` in the process environment

    warning: `with_sampler` overrides `OTEL_TRACES_SAMPLER`, `OTEL_TRACES_SAMPLER_ARG`, which `opentelemetry_sdk` reads
      --> tools/dylint/examples/environment_override.rs:21:66
       |
    21 | fn sampled() -> SdkTracerProvider { SdkTracerProvider::builder().with_sampler(Sampler::AlwaysOn).with_max_events_per_span(16).with_s...
       |                                                                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the call and set `OTEL_TRACES_SAMPLER`, `OTEL_TRACES_SAMPLER_ARG` in the process environment

    warning: `with_scheduled_delay` overrides `OTEL_BSP_SCHEDULE_DELAY`, which `opentelemetry_sdk` reads
      --> tools/dylint/examples/environment_override.rs:24:68
       |
    24 |     (trace::BatchConfigBuilder::default().with_max_queue_size(512).with_scheduled_delay(Duration::from_secs(1)).build(), logs::Batch...
       |                                                                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the call and set `OTEL_BSP_SCHEDULE_DELAY` in the process environment

    warning: `with_max_queue_size` overrides `OTEL_BSP_MAX_QUEUE_SIZE`, which `opentelemetry_sdk` reads
      --> tools/dylint/examples/environment_override.rs:24:43
       |
    24 |     (trace::BatchConfigBuilder::default().with_max_queue_size(512).with_scheduled_delay(Duration::from_secs(1)).build(), logs::Batch...
       |                                           ^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the call and set `OTEL_BSP_MAX_QUEUE_SIZE` in the process environment

    warning: `with_max_export_batch_size` overrides `OTEL_BLRP_MAX_EXPORT_BATCH_SIZE`, which `opentelemetry_sdk` reads
      --> tools/dylint/examples/environment_override.rs:24:158
       |
    24 | ...1)).build(), logs::BatchConfigBuilder::default().with_max_export_batch_size(64).build())
       |                                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the call and set `OTEL_BLRP_MAX_EXPORT_BATCH_SIZE` in the process environment

    warning: `with_interval` overrides `OTEL_METRIC_EXPORT_INTERVAL`, which `opentelemetry_sdk` reads
      --> tools/dylint/examples/environment_override.rs:27:109
       |
    27 | ...Exporter> { PeriodicReader::builder(exporter).with_interval(Duration::from_secs(5)).build() }
       |                                                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete the call and set `OTEL_METRIC_EXPORT_INTERVAL` in the process environment

    warning: `with_detector` overrides `OTEL_SERVICE_NAME`, `OTEL_RESOURCE_ATTRIBUTES`, which `opentelemetry_sdk` reads
      --> tools/dylint/examples/environment_override.rs:29:49
       |
    29 | fn detected() -> Resource { Resource::builder().with_detector(Box::new(TelemetryResourceDetector)).build() }
       |                                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: merge `SdkProvidedResourceDetector`, `EnvResourceDetector` after every other detector, starting from `Resource::builder_empty()`

    warning: `with_detectors` overrides `OTEL_SERVICE_NAME`, `OTEL_RESOURCE_ATTRIBUTES`, which `opentelemetry_sdk` reads
      --> tools/dylint/examples/environment_override.rs:33:12
       |
    33 |     seeded.with_detectors(&[Box::new(TelemetryResourceDetector)]).build()
       |            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: merge `SdkProvidedResourceDetector`, `EnvResourceDetector` after every other detector, starting from `Resource::builder_empty()`

    warning: `with_detector` overrides `OTEL_RESOURCE_ATTRIBUTES`, which `opentelemetry_sdk` reads
      --> tools/dylint/examples/environment_override.rs:36:108
       |
    36 | ...(Box::new(EnvResourceDetector::new())).with_detector(Box::new(TelemetryResourceDetector)).build() }
       |                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: merge `EnvResourceDetector` after every other detector

    warning: `with_detectors` overrides `OTEL_RESOURCE_ATTRIBUTES`, which `opentelemetry_sdk` reads
      --> tools/dylint/examples/environment_override.rs:38:55
       |
    38 | ...::builder_empty().with_detectors(&[Box::new(EnvResourceDetector::new()), Box::new(TelemetryResourceDetector)]).build() }
       |                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: merge `EnvResourceDetector` after every other detector

    warning: `with_detector` overrides `OTEL_SERVICE_NAME`, which `opentelemetry_sdk` reads
      --> tools/dylint/examples/environment_override.rs:42:49
       |
    42 | fn restored() -> Resource { Resource::builder().with_detector(Box::new(TelemetryResourceDetector)).with_detector(Box::new(EnvResourc...
       |                                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: merge `SdkProvidedResourceDetector` after every other detector, starting from `Resource::builder_empty()`
    "#);
}

fn explicit_iter_chain_zip(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `iter` converts `left` into an iterator only to call `zip`
      --> tools/dylint/examples/assertions_outside_tests.rs:26:5
       |
    26 |     left.iter().zip(right).map(|(left, right)| left.strict_mul(*right)).sum()
       |     ^^^^^^^^^^^^^^^^^^^^^^ help: call `iter::zip` with `use core::iter;`: `iter::zip(left, right)`
       |
       = note: `#[warn(explicit_iter_chain_zip)]` on by default
    applicability MaybeIncorrect: call `iter::zip` with `use core::iter;`

    warning: `into_iter` converts `left` into an iterator only to call `chain`
      --> tools/dylint/examples/explicit_iter_chain_zip.rs:18:57
       |
    18 | fn owned(left: Vec<u32>, right: Vec<u32>) -> Vec<u32> { left.into_iter().chain(right).collect() }
       |                                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `iter::chain` with `use core::iter;`: `iter::chain(left, right)`
       |
       = note: `#[warn(explicit_iter_chain_zip)]` on by default
    applicability MaybeIncorrect: call `iter::chain` with `use core::iter;`

    warning: `iter` converts `values` into an iterator only to call `zip`
      --> tools/dylint/examples/explicit_iter_chain_zip.rs:20:55
       |
    20 | ...2]) -> u32 { values.iter().zip(weights).map(|(value, weight)| value.strict_mul(*weight)).sum() }
       |                 ^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `iter::zip` with `use core::iter;`: `iter::zip(values, weights)`
    applicability MaybeIncorrect: call `iter::zip` with `use core::iter;`

    warning: `iter` converts `values` into an iterator only to call `zip`
      --> tools/dylint/examples/explicit_iter_chain_zip.rs:24:5
       |
    24 |     values.iter().zip(weights).map(|(value, weight)| value.strict_add(*weight)).sum()
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `iter::zip` with `use core::iter;`: `iter::zip(&values, weights)`
    applicability MaybeIncorrect: call `iter::zip` with `use core::iter;`

    warning: `iter_mut` converts `values` into an iterator only to call `zip`
      --> tools/dylint/examples/explicit_iter_chain_zip.rs:29:5
       |
    29 |     values.iter_mut().zip(weights).for_each(|(value, weight)| *value = value.strict_mul(*weight));
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `iter::zip` with `use core::iter;`: `iter::zip(&mut values, weights)`
    applicability MaybeIncorrect: call `iter::zip` with `use core::iter;`

    warning: `iter` converts `map` into an iterator only to call `chain`
      --> tools/dylint/examples/explicit_iter_chain_zip.rs:33:69
       |
    33 | ... &[(u32, u32)]) -> usize { map.iter().chain(extra.iter().map(|(key, value)| (key, value))).count() }
       |                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: call `iter::chain` with `use core::iter;`: `iter::chain(map, extra.iter().map(|(key, value)| (key, value)))`
    applicability MaybeIncorrect: call `iter::chain` with `use core::iter;`
    ");
}

fn redundant_clone_in_closure(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: closure clones `path` with no later read
      --> tools/dylint/examples/redundant_clone_in_closure.rs:24:113
       |
    24 | ...se().map_err(|source| ReadError { path: path.clone(), source }) }
       |                                      ------^^^^^^^^^^^^
       |                                      |
       |                                      help: move `path` into the closure: `path`
       |
       = note: `#[warn(redundant_clone_in_closure)]` on by default
    applicability MachineApplicable: move `path` into the closure

    warning: closure clones `name` with no later read
      --> tools/dylint/examples/redundant_clone_in_closure.rs:26:92
       |
    26 | fn fallback(name: String, given: Option<String>) -> Option<String> { given.or_else(|| Some(name.clone())) }
       |                                                                                            ^^^^^^^^^^^^ help: move `name` into the closure: `name`
    applicability MachineApplicable: move `name` into the closure

    warning: async block clones `name` with no later read
      --> tools/dylint/examples/redundant_clone_in_closure.rs:28:56
       |
    28 | fn deferred(name: String) -> String { block_on(async { name.clone() }) }
       |                                                        ^^^^^^^^^^^^ help: move `name` into the async block: `name`
    applicability MachineApplicable: move `name` into the async block

    warning: closure clones `name` with no later read
      --> tools/dylint/examples/redundant_clone_in_closure.rs:30:89
       |
    30 | fn owned(name: String, given: Option<String>) -> Option<String> { given.or_else(|| Some(name.to_owned())) }
       |                                                                                         ^^^^^^^^^^^^^^^ help: move `name` into the closure: `name`
    applicability MachineApplicable: move `name` into the closure

    warning: closure clones `name` with no later read
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
       |
       = note: `..` skips the hand-written `Default` impl of `Retry`
    applicability MaybeIncorrect: write `..` to take each field's declared default

    warning: `..Limits::default()` fills only `Limits` fields with declared default values
      --> tools/dylint/examples/redundant_default_base.rs:45:53
       |
    45 | fn named(name: String) -> Limits { Limits { name, ..Limits::default() } }
       |                                                     ^^^^^^^^^^^^^^^^^ help: write `..` to take each field's declared default
    applicability MachineApplicable: write `..` to take each field's declared default
    ");
}

fn wrapping_arithmetic(found: &str) {
    insta::assert_snapshot!(found, @"
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

    warning: `+` on `Wrapping<u8>` wraps on overflow
     --> tools/dylint/examples/wrapping_arithmetic.rs:9:41
      |
    9 | fn wrapped(value: u8) -> Wrapping<u8> { Wrapping(value) + Wrapping(1) }
      |                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
      |
      = help: hold `u8` in place of `Wrapping<u8>`, call `checked_add` and return its `None` as an error, or `saturating_add` to clamp at the `u8` bounds

    warning: `u16::wrapping_sub` wraps on overflow
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
       = help: call `checked_sub` and return its `None` as an error, or `saturating_sub` to clamp at the `u16` bounds

    warning: `u32::wrapping_add_signed` wraps on overflow
      --> tools/dylint/examples/wrapping_arithmetic.rs:29:44
       |
    29 | const fn signed(value: u32) -> u32 { value.wrapping_add_signed(-1) }
       |                                            ^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call `checked_add_signed` and return its `None` as an error, or `saturating_add_signed` to clamp at the `u32` bounds

    warning: `+=` on `Wrapping<i8>` wraps on overflow
      --> tools/dylint/examples/wrapping_arithmetic.rs:32:5
       |
    32 |     total += Wrapping(1);
       |     ^^^^^^^^^^^^^^^^^^^^
       |
       = help: hold `i8` in place of `Wrapping<i8>`, call `checked_add` and return its `None` as an error, or `saturating_add` to clamp at the `i8` bounds

    warning: `-` on `Wrapping<i8>` wraps on overflow
      --> tools/dylint/examples/wrapping_arithmetic.rs:33:5
       |
    33 |     -total
       |     ^^^^^^
       |
       = help: hold `i8` in place of `Wrapping<i8>`, call `checked_neg` and return its `None` as an error, or `saturating_neg` to clamp at the `i8` bounds
    ");
}

// --- [COMPOSITION] ---------------------------------------------------------------------

snapshots!(
    absurd_nonzero_comparison,
    assertions_outside_tests,
    borrowed_indexing,
    cloned_command_matches,
    environment_override,
    explicit_iter_chain_zip,
    redundant_clone_in_closure,
    redundant_default_base,
    wrapping_arithmetic
);
