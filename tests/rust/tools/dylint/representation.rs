// --- [OPERATIONS] ----------------------------------------------------------------------

#[rustfmt::skip]
snapshots! {
fn formatted_tracing_value(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: tracing message interpolates values into text
      --> tools/dylint/examples/formatted_tracing_value.rs:22:5
       |
    22 |     tracing::info!("loaded {} rows from {path}", rows);
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: record each value beside a constant message: directly for `tracing::Value`, `%value` for `Display` formatting, or `?value` for `Debug` formatting
       = note: `#[warn(formatted_tracing_value)]` on by default

    warning: tracing field records a value rendered to text
      --> tools/dylint/examples/formatted_tracing_value.rs:28:27
       |
    28 |     tracing::warn!(port = format!("{port}"), "listening");
       |                           ^^^^^^^^^^^^^^^^^
       |
       = help: record each value directly for `tracing::Value`, with `%value` for `Display` formatting or `?value` for `Debug` formatting

    warning: tracing field records a value rendered to text
      --> tools/dylint/examples/formatted_tracing_value.rs:29:28
       |
    29 |     tracing::debug!(port = port.to_string(), "listening");
       |                            ^^^^^^^^^^^^^^^^ help: record the `u16` as a typed field: `port`
    applicability MaybeIncorrect: record the `u16` as a typed field

    warning: tracing field records a value rendered to text
      --> tools/dylint/examples/formatted_tracing_value.rs:32:55
       |
    32 |     let _span = tracing::info_span!("request", port = format!("{port}"));
       |                                                       ^^^^^^^^^^^^^^^^^
       |
       = help: record each value directly for `tracing::Value`, with `%value` for `Display` formatting or `?value` for `Debug` formatting

    warning: tracing field records a value rendered to text
      --> tools/dylint/examples/formatted_tracing_value.rs:33:65
       |
    33 |     tracing::info_span!("request", port = Empty).record("port", port.to_string());
       |                                                                 ^^^^^^^^^^^^^^^^ help: record the `u16` as a typed field: `&port`
    applicability MaybeIncorrect: record the `u16` as a typed field

    warning: tracing field records a value rendered to text
      --> tools/dylint/examples/formatted_tracing_value.rs:34:28
       |
    34 |     tracing::debug!(port = ToString::to_string(&port), "listening");
       |                            ^^^^^^^^^^^^^^^^^^^^^^^^^^ help: record the `&u16` as a typed field: `&port`
    applicability MaybeIncorrect: record the `&u16` as a typed field

    warning: tracing field records a value rendered to text
      --> tools/dylint/examples/formatted_tracing_value.rs:35:65
       |
    35 |     tracing::info_span!("request", port = Empty).record("port", ToString::to_string(&port));
       |                                                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^ help: record the `&u16` as a typed field: `&&port`
    applicability MaybeIncorrect: record the `&u16` as a typed field

    warning: tracing field records a value rendered to text
      --> tools/dylint/examples/formatted_tracing_value.rs:37:43
       |
    37 |     tracing::Span::record(&span, "port", &port.to_string());
       |                                           ^^^^^^^^^^^^^^^^ help: record the `u16` as a typed field: `&port`
    applicability MaybeIncorrect: record the `u16` as a typed field

    warning: tracing field records a value rendered to text
      --> tools/dylint/examples/formatted_tracing_value.rs:38:42
       |
    38 |     tracing::Span::record(&span, "port", format!("{port}"));
       |                                          ^^^^^^^^^^^^^^^^^
       |
       = help: pass a native `tracing::Value` to `Span::record`, or use `tracing::field::display` for `Display` formatting and `tracing::field::debug` for `Debug` formatting

    warning: tracing field records a value rendered to text
      --> tools/dylint/examples/formatted_tracing_value.rs:42:19
       |
    42 |     let message = format!("handled {count} rows");
       |                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: record each value directly for `tracing::Value`, with `%value` for `Display` formatting or `?value` for `Debug` formatting

    warning: tracing field records a value rendered to text
      --> tools/dylint/examples/formatted_tracing_value.rs:44:31
       |
    44 |     tracing::info!(summary = %format_args!("handled {count}"), "done");
       |                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: record each value directly for `tracing::Value`, with `%value` for `Display` formatting or `?value` for `Debug` formatting

    warning: tracing message interpolates values into text
      --> tools/dylint/examples/formatted_tracing_value.rs:54:5
       |
    54 |     tracing::info!("state {status:?}");
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: record each value beside a constant message: directly for `tracing::Value`, `%value` for `Display` formatting, or `?value` for `Debug` formatting

    warning: tracing field records a value rendered to text
      --> tools/dylint/examples/formatted_tracing_value.rs:55:29
       |
    55 |     tracing::debug!(state = format!("{status:?}"), "state");
       |                             ^^^^^^^^^^^^^^^^^^^^^
       |
       = help: record each value directly for `tracing::Value`, with `%value` for `Display` formatting or `?value` for `Debug` formatting

    warning: tracing field records a value rendered to text
      --> tools/dylint/examples/formatted_tracing_value.rs:56:67
       |
    56 |     tracing::info_span!("request", state = Empty).record("state", format!("{status:?}"));
       |                                                                   ^^^^^^^^^^^^^^^^^^^^^
       |
       = help: pass a native `tracing::Value` to `Span::record`, or use `tracing::field::display` for `Display` formatting and `tracing::field::debug` for `Debug` formatting

    warning: tracing field records a value rendered to text
      --> tools/dylint/examples/formatted_tracing_value.rs:61:67
       |
    61 |     tracing::info_span!("request", error = Empty).record("error", error.to_string());
       |                                                                   ^^^^^^^^^^^^^^^^^ help: record the value through its `Display` impl: `tracing::field::display(&error)`
    applicability MaybeIncorrect: record the value through its `Display` impl

    warning: tracing field records a value rendered to text
      --> tools/dylint/examples/formatted_tracing_value.rs:64:67
       |
    64 |     tracing::info_span!("request", error = Empty).record("error", boxed.to_string());
       |                                                                   ^^^^^^^^^^^^^^^^^ help: record the `std::boxed::Box<dyn std::error::Error>` as a typed field: `&boxed`
    applicability MaybeIncorrect: record the `std::boxed::Box<dyn std::error::Error>` as a typed field
    "#);
}

fn manual_reference_impl(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `impl AsRef<String> for Title` returns the wrapped `String` by hand
      --> tools/dylint/examples/manual_reference_impl.rs:86:1
       |
    86 | / impl AsRef<String> for Title {
    87 | |     fn as_ref(&self) -> &String { &self.0 }
    88 | | }
       | |_^
       |
       = help: derive `derive_more::AsRef` on `Title` with `#[as_ref(String)]` and delete the impl
       = note: `#[warn(manual_reference_impl)]` on by default

    warning: `impl AsRef<str> for Title` returns the wrapped `String` by hand
      --> tools/dylint/examples/manual_reference_impl.rs:90:1
       |
    90 | / impl AsRef<str> for Title {
    91 | |     fn as_ref(&self) -> &str { self.0.as_ref() }
    92 | | }
       | |_^
       |
       = help: derive `derive_more::AsRef` on `Title` with `#[as_ref(str)]` and delete the impl

    warning: `impl AsMut<String> for Title` returns the wrapped `String` by hand
      --> tools/dylint/examples/manual_reference_impl.rs:94:1
       |
    94 | / impl AsMut<String> for Title {
    95 | |     fn as_mut(&mut self) -> &mut String { &mut self.0 }
    96 | | }
       | |_^
       |
       = help: derive `derive_more::AsMut` on `Title` with `#[as_mut(String)]` and delete the impl
    ");
}

fn manual_serialization(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `ContentBlock::text` receives text from `serde_json::to_string`
      --> tools/dylint/examples/manual_serialization.rs:15:71
       |
    15 | fn tried(port: &Port) -> Result<ContentBlock, serde_json::Error> { Ok(ContentBlock::text(serde_json::to_string(port)?)) }
       |                                                                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: serialized here
      --> tools/dylint/examples/manual_serialization.rs:15:90
       |
    15 | fn tried(port: &Port) -> Result<ContentBlock, serde_json::Error> { Ok(ContentBlock::text(serde_json::to_string(port)?)) }
       |                                                                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: pass `port` to `ContentBlock::json`
       = note: `ContentBlock::json` returns `Result<ContentBlock, ErrorData>`
       = note: `#[warn(manual_serialization)]` on by default

    warning: `ContentBlock::text` receives text from `serde_json::to_string`
      --> tools/dylint/examples/manual_serialization.rs:19:8
       |
    19 |     Ok(ContentBlock::text(text))
       |        ^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: serialized here
      --> tools/dylint/examples/manual_serialization.rs:18:16
       |
    18 |     let text = serde_json::to_string(port)?;
       |                ^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: pass `port` to `ContentBlock::json`
       = note: `ContentBlock::json` returns `Result<ContentBlock, ErrorData>`

    warning: `ContentBlock::text` receives text from `serde_json::to_string`
      --> tools/dylint/examples/manual_serialization.rs:22:45
       |
    22 | fn defaulted(port: &Port) -> ContentBlock { ContentBlock::text(serde_json::to_string(port).unwrap_or_default()) }
       |                                             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: serialized here
      --> tools/dylint/examples/manual_serialization.rs:22:64
       |
    22 | fn defaulted(port: &Port) -> ContentBlock { ContentBlock::text(serde_json::to_string(port).unwrap_or_default()) }
       |                                                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: pass `port` to `ContentBlock::json`
       = note: `ContentBlock::json` returns `Result<ContentBlock, ErrorData>`
    ");
}

fn manual_variant_array(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: borrowed slice lists every variant of `Mode` in declaration order
      --> tools/dylint/examples/manual_variant_array.rs:53:79
       |
    53 |     black_box(&([ALL.len(), HOT.len(), PRIORITY.len(), Mode::VARIANTS.len()], [Mode::Fast, Mode::Slow].contains(&Mode::Slow)));
       |                                                                               ^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: read `<Mode as strum::VariantArray>::VARIANTS`; add `#[derive(strum::VariantArray)]` where the trait is absent
       = note: `#[warn(manual_variant_array)]` on by default

    warning: borrowed slice lists every variant of `Measure<4>` in declaration order
      --> tools/dylint/examples/manual_variant_array.rs:54:15
       |
    54 |     black_box([Measure::<4>::Low, Measure::<4>::High].contains(&Measure::<4>::High));
       |               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: read `<Measure<4> as strum::VariantArray>::VARIANTS`; add `#[derive(strum::VariantArray)]` where the trait is absent

    warning: borrowed slice lists every variant of `Visible` in declaration order
      --> tools/dylint/examples/manual_variant_array.rs:58:15
       |
    58 |     black_box([Visible::Shown, Visible::Hidden].contains(&Visible::Hidden));
       |               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: read `<Visible as strum::VariantArray>::VARIANTS`; add `#[derive(strum::VariantArray)]` where the trait is absent
    ");
}

fn needless_reference_field(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `Walker` holds `cx` by reference only to copy `cx.tcx`
       --> tools/dylint/examples/needless_reference_field.rs:25:5
        |
     25 |     cx: &'cx Context<'tcx>,
        |     ^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `cx.tcx` copied here
       --> tools/dylint/examples/needless_reference_field.rs:130:55
        |
    130 |     const fn interner(&self) -> Interner<'tcx> { self.cx.tcx }
        |                                                       ^^^^^^
        = help: store the `Interner<'tcx>` of `cx.tcx` in place of `cx` and remove `'cx`
        = note: `#[warn(needless_reference_field)]` on by default

    warning: `Shared` holds `settings` by reference only to copy `settings.port`
       --> tools/dylint/examples/needless_reference_field.rs:29:5
        |
     29 |     settings: &'settings Settings,
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `settings.port` copied here
       --> tools/dylint/examples/needless_reference_field.rs:134:40
        |
    134 |     const fn port(&self) -> u16 { self.settings.port }
        |                                        ^^^^^^^^^^^^^
        = help: store the `u16` of `settings.port` in place of `settings`

    warning: `Exposed` holds `settings` by reference only to copy `settings.port`
       --> tools/dylint/examples/needless_reference_field.rs:39:5
        |
     39 |     pub settings: &'settings Settings,
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `settings.port` copied here
       --> tools/dylint/examples/needless_reference_field.rs:144:40
        |
    144 |     const fn port(&self) -> u16 { self.settings.port }
        |                                        ^^^^^^^^^^^^^
        = help: store the `u16` of `settings.port` in place of `settings` and remove `'settings`

    warning: `Derived` holds `settings` by reference only to copy `settings.port`
       --> tools/dylint/examples/needless_reference_field.rs:68:5
        |
     68 |     settings: &'settings Settings,
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `settings.port` copied here
       --> tools/dylint/examples/needless_reference_field.rs:177:40
        |
    177 |     const fn port(&self) -> u16 { self.settings.port }
        |                                        ^^^^^^^^^^^^^
        = help: store the `u16` of `settings.port` in place of `settings` and remove `'settings`

    warning: `Tagged` holds `0` by reference only to copy `0.retries`
       --> tools/dylint/examples/needless_reference_field.rs:88:26
        |
     88 | struct Tagged<'settings>(&'settings Settings);
        |                          ^^^^^^^^^^^^^^^^^^^
        |
    note: `0.retries` copied here
       --> tools/dylint/examples/needless_reference_field.rs:193:42
        |
    193 |     const fn retries(&self) -> u8 { self.0.retries }
        |                                          ^^^^^^^^^
        = help: store the `u8` of `0.retries` in place of `0` and remove `'settings`

    warning: `Scoped` holds `source` by reference only to copy `source.value`
       --> tools/dylint/examples/needless_reference_field.rs:111:5
        |
    111 |     source: &'value Projected<T>,
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `source.value` copied here
       --> tools/dylint/examples/needless_reference_field.rs:208:40
        |
    208 |     fn value(&self) -> T::Value { self.source.value }
        |                                        ^^^^^^^^^^^^
        = help: store the `<T as Width>::Value` of `source.value` in place of `source` and remove `'value`
    ");
}

fn needless_struct(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `Label` has no impl beyond `Debug`, and every use builds or destructures it whole
      --> tools/dylint/examples/needless_struct.rs:11:1
       |
    11 | struct Label {
       | ^^^^^^^^^^^^
       |
       = help: replace `Label` with its field type `String`
       = note: `#[warn(needless_struct)]` on by default
    ");
}

fn option_fields_as_result(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: every construction of `Parsed` sets exactly one of success `port` and failure `failure`
      --> tools/dylint/examples/option_fields_as_result.rs:35:5
       |
    35 |     failure: Option<ParseIntError>,
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: `port` declared here
      --> tools/dylint/examples/option_fields_as_result.rs:34:5
       |
    34 |     port: Option<u16>,
       |     ^^^^^^^^^^^^^^^^^
       = help: replace `port` and `failure` with one `Result<u16, ParseIntError>` field
       = note: `#[warn(option_fields_as_result)]` on by default

    warning: every construction of `Reply` sets exactly one of success `value` and failure `refusal`
      --> tools/dylint/examples/option_fields_as_result.rs:41:5
       |
    41 |     refusal: Option<Closed>,
       |     ^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: `value` declared here
      --> tools/dylint/examples/option_fields_as_result.rs:40:5
       |
    40 |     value: Option<u16>,
       |     ^^^^^^^^^^^^^^^^^^
       = help: replace `value` and `refusal` with one `Result<u16, Closed>` field

    warning: every construction of `Exchange` sets exactly one of success `value` and failure `error`
      --> tools/dylint/examples/option_fields_as_result.rs:84:5
       |
    84 |     pub error: Option<String>,
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: `value` declared here
      --> tools/dylint/examples/option_fields_as_result.rs:83:5
       |
    83 |     pub value: Option<u32>,
       |     ^^^^^^^^^^^^^^^^^^^^^^
       = help: replace `value` and `error` with one `Result<u32, String>` field
    ");
}

fn redundant_option_flag(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: every construction sets `Outcome::ok` to `value.is_some()`
      --> tools/dylint/examples/redundant_option_flag.rs:53:5
       |
    53 |     pub ok: bool,
       |     ^^^^^^^^^^^^
       |
    note: `value` declared here
      --> tools/dylint/examples/redundant_option_flag.rs:54:5
       |
    54 |     pub value: Option<String>,
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: delete `ok` and read `value.is_some()` or match `value`
       = note: `#[warn(redundant_option_flag)]` on by default

    warning: every construction sets `Parsed::parsed` to `port.is_some()`
      --> tools/dylint/examples/redundant_option_flag.rs:59:5
       |
    59 |     parsed: bool,
       |     ^^^^^^^^^^^^
       |
    note: `port` declared here
      --> tools/dylint/examples/redundant_option_flag.rs:60:5
       |
    60 |     port: Option<u16>,
       |     ^^^^^^^^^^^^^^^^^
       = help: delete `parsed` and read `port.is_some()` or match `port`

    warning: every construction sets `Lookup::found` to `value.is_some()`
      --> tools/dylint/examples/redundant_option_flag.rs:29:5
       |
    29 |     found: bool,
       |     ^^^^^^^^^^^
       |
    note: `value` declared here
      --> tools/dylint/examples/redundant_option_flag.rs:30:5
       |
    30 |     value: Option<u16>,
       |     ^^^^^^^^^^^^^^^^^^
       = help: delete `found` and read `value.is_some()` or match `value`

    warning: every construction sets `Cached::missing` to `entry.is_none()`
      --> tools/dylint/examples/redundant_option_flag.rs:35:5
       |
    35 |     missing: bool,
       |     ^^^^^^^^^^^^^
       |
    note: `entry` declared here
      --> tools/dylint/examples/redundant_option_flag.rs:36:5
       |
    36 |     entry: Option<String>,
       |     ^^^^^^^^^^^^^^^^^^^^^
       = help: delete `missing` and read `entry.is_none()` or match `entry`
    ");
}

fn restated_constant(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `"OTEL_EXPORTER_OTLP_ENDPOINT"` spells `opentelemetry_otlp::OTEL_EXPORTER_OTLP_ENDPOINT`
     --> tools/dylint/examples/restated_constant.rs:9:24
      |
    9 | const ENDPOINT: &str = "OTEL_EXPORTER_OTLP_ENDPOINT";
      |                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: replace the literal with the constant: `opentelemetry_otlp::OTEL_EXPORTER_OTLP_ENDPOINT`
      |
      = note: `#[warn(restated_constant)]` on by default
    applicability MaybeIncorrect: replace the literal with the constant

    warning: `"service.name"` spells `opentelemetry_semantic_conventions::attribute::SERVICE_NAME`
      --> tools/dylint/examples/restated_constant.rs:21:61
       |
    21 | const fn spelled() -> [(&'static str, &'static str); 2] { [("service.name", "bridge"), ("http.server.request.duration", "1.5")] }
       |                                                             ^^^^^^^^^^^^^^ help: replace the literal with the constant: `opentelemetry_semantic_conventions::attribute::SERVICE_NAME`
    applicability MaybeIncorrect: replace the literal with the constant

    warning: `"http.server.request.duration"` spells `opentelemetry_semantic_conventions::metric::HTTP_SERVER_REQUEST_DURATION`
      --> tools/dylint/examples/restated_constant.rs:21:89
       |
    21 | const fn spelled() -> [(&'static str, &'static str); 2] { [("service.name", "bridge"), ("http.server.request.duration", "1.5")] }
       |                                                                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: replace the literal with the constant: `opentelemetry_semantic_conventions::metric::HTTP_SERVER_REQUEST_DURATION`
    applicability MaybeIncorrect: replace the literal with the constant

    warning: `"code.file.path"` spells `opentelemetry_semantic_conventions::attribute::CODE_FILE_PATH`
      --> tools/dylint/examples/restated_constant.rs:23:47
       |
    23 | fn located(key: &str) -> bool { matches!(key, "code.file.path") }
       |                                               ^^^^^^^^^^^^^^^^ help: replace the literal with the constant: `opentelemetry_semantic_conventions::attribute::CODE_FILE_PATH`
    applicability MaybeIncorrect: replace the literal with the constant

    warning: `"service.namespace"` spells `opentelemetry_semantic_conventions::attribute::SERVICE_NAMESPACE`
      --> tools/dylint/examples/restated_constant.rs:27:9
       |
    25 | macro_rules! namespaced {
    26 |     () => {
    27 |         "service.namespace"
       |         ^^^^^^^^^^^^^^^^^^^
    ...
    31 | const fn expanded() -> &'static str { namespaced!() }
       |                                       ------------- in this macro invocation
       |
    help: replace the literal with the constant
      --> tools/dylint/examples/restated_constant.rs:27:9
       |
    27 |         "service.namespace"
       |         ^^^^^^^^^^^^^^^^^^^
    ...
    31 | const fn expanded() -> &'static str { namespaced!() }
       |                                       ------------- in this macro invocation
    "#);
}

fn sentinel_default(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `unwrap_or_default` replaces `None` with a `u32` default tested immediately
      --> tools/dylint/examples/sentinel_default.rs:90:48
       |
    90 | fn numeric(value: Option<u32>) -> bool { value.unwrap_or_default() == 0 }
       |                                                ^^^^^^^^^^^^^^^^^^^
       |
    note: tested here
      --> tools/dylint/examples/sentinel_default.rs:90:42
       |
    90 | fn numeric(value: Option<u32>) -> bool { value.unwrap_or_default() == 0 }
       |                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: test the `Option` directly with `is_none_or`, `is_some_and`, or a match; preserve mapping and `Some` values equal to the default
       = note: `#[warn(sentinel_default)]` on by default

    warning: `unwrap_or` replaces `None` with a `&str` default tested immediately
      --> tools/dylint/examples/sentinel_default.rs:92:49
       |
    92 | fn textual(value: Option<&str>) -> bool { value.unwrap_or("").is_empty() }
       |                                                 ^^^^^^^^^^^^^
       |
    note: tested here
      --> tools/dylint/examples/sentinel_default.rs:92:43
       |
    92 | fn textual(value: Option<&str>) -> bool { value.unwrap_or("").is_empty() }
       |                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: test the `Option` directly with `is_none_or`, `is_some_and`, or a match; preserve mapping and `Some` values equal to the default

    warning: `unwrap_or_default` replaces `None` with a `&[u8]` default tested immediately
      --> tools/dylint/examples/sentinel_default.rs:94:51
       |
    94 | fn borrowed(value: Option<&[u8]>) -> bool { value.unwrap_or_default().is_empty() }
       |                                                   ^^^^^^^^^^^^^^^^^^^
       |
    note: tested here
      --> tools/dylint/examples/sentinel_default.rs:94:45
       |
    94 | fn borrowed(value: Option<&[u8]>) -> bool { value.unwrap_or_default().is_empty() }
       |                                             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: test the `Option` directly with `is_none_or`, `is_some_and`, or a match; preserve mapping and `Some` values equal to the default

    warning: `map_or` replaces `None` with a `u32` default tested immediately
      --> tools/dylint/examples/sentinel_default.rs:96:52
       |
    96 | fn mapped_test(value: Option<u32>) -> bool { value.map_or(0, |value| value.saturating_mul(2)) == 0 }
       |                                                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: tested here
      --> tools/dylint/examples/sentinel_default.rs:96:46
       |
    96 | fn mapped_test(value: Option<u32>) -> bool { value.map_or(0, |value| value.saturating_mul(2)) == 0 }
       |                                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: test the `Option` directly with `is_none_or`, `is_some_and`, or a match; preserve mapping and `Some` values equal to the default

    warning: `unwrap_or_default` replaces `None` with a `char` default tested immediately
       --> tools/dylint/examples/sentinel_default.rs:99:17
        |
     99 |     match value.unwrap_or_default() {
        |                 ^^^^^^^^^^^^^^^^^^^
        |
    note: tested here
       --> tools/dylint/examples/sentinel_default.rs:99:5
        |
     99 | /     match value.unwrap_or_default() {
    100 | |         '\0' => false,
    101 | |         _ => true,
    102 | |     }
        | |_____^
        = help: test the `Option` directly with `is_none_or`, `is_some_and`, or a match; preserve mapping and `Some` values equal to the default
    "#);
}

fn single_impl_trait(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: trait `Toggle` is implemented only for `&mut bool`
     --> tools/dylint/examples/manual_bool_toggle.rs:9:1
      |
    9 | trait Toggle {
      | ^^^^^^^^^^^^
      |
      = help: replace `Toggle` with functions and constants for `&mut bool`
      = note: `#[warn(single_impl_trait)]` on by default

    warning: trait `New` is implemented only for `Vec<u8, System>`
      --> tools/dylint/examples/manual_default_field_values.rs:10:1
       |
    10 | trait New {
       | ^^^^^^^^^
       |
       = help: replace `New` with functions and constants for `Vec<u8, System>`
       = note: `#[warn(single_impl_trait)]` on by default

    warning: trait `converted::From` is implemented only for `String`
       --> tools/dylint/examples/manual_default_field_values.rs:415:5
        |
    415 |     trait From {
        |     ^^^^^^^^^^
        |
        = help: replace `converted::From` with functions and constants for `String`

    warning: trait `Measured` is implemented only for `VecDeque<u32>`
     --> tools/dylint/examples/manual_retain_back.rs:6:1
      |
    6 | trait Measured {
      | ^^^^^^^^^^^^^^
      |
      = help: replace `Measured` with functions and constants for `VecDeque<u32>`
      = note: `#[warn(single_impl_trait)]` on by default

    warning: trait `Retained` is implemented only for `&mut VecDeque<u32>`
      --> tools/dylint/examples/manual_retain_back.rs:14:1
       |
    14 | trait Retained {
       | ^^^^^^^^^^^^^^
       |
       = help: replace `Retained` with functions and constants for `&mut VecDeque<u32>`

    warning: trait `shadowed_option::Presence` is implemented only for `Option<T>`
       --> tools/dylint/examples/manual_retain_back.rs:284:5
        |
    284 |     trait Presence {
        |     ^^^^^^^^^^^^^^
        |
        = help: replace `shadowed_option::Presence` with functions and constants for `Option<T>`

    warning: trait `Measured` is implemented only for `[u8; 4]`
      --> tools/dylint/examples/manual_slice_as_array.rs:10:1
       |
    10 | trait Measured {
       | ^^^^^^^^^^^^^^
       |
       = help: replace `Measured` with functions and constants for `[u8; 4]`
       = note: `#[warn(single_impl_trait)]` on by default

    warning: trait `View` is implemented only for `Borrowed<'_>`
      --> tools/dylint/examples/manual_upcast.rs:48:1
       |
    48 | trait View {
       | ^^^^^^^^^^
       |
       = help: replace `View` with an inherent `impl Borrowed<'_>` block
       = note: `#[warn(single_impl_trait)]` on by default

    warning: trait `FixedView` is implemented only for `Square`
       --> tools/dylint/examples/manual_upcast.rs:81:1
        |
     81 | trait FixedView: Configured {
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `dyn FixedView` always holds `Square`
       --> tools/dylint/examples/manual_upcast.rs:315:17
        |
    315 |     let fixed: &dyn FixedView<Item = u8> = &square;
        |                 ^^^^^^^^^^^^^^^^^^^^^^^^
        = help: replace `FixedView` with an inherent `impl Square` block

    warning: trait `Projected` is implemented only for `Square`
       --> tools/dylint/examples/manual_upcast.rs:94:1
        |
     94 | trait Projected<T: Witness<Value = Self>>: Display {
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `dyn Projected` always holds `Square`
       --> tools/dylint/examples/manual_upcast.rs:313:21
        |
    313 |     let projected: &dyn Projected<()> = &square;
        |                     ^^^^^^^^^^^^^^^^^
        = help: replace `Projected` with an inherent `impl Square` block

    warning: trait `Small` is implemented only for `Result<u8, ParseIntError>`
      --> tools/dylint/examples/partition_by_result.rs:13:1
       |
    13 | trait Small {
       | ^^^^^^^^^^^
       |
       = help: replace `Small` with functions and constants for `Result<u8, ParseIntError>`
       = note: `#[warn(single_impl_trait)]` on by default

    warning: trait `Store` is implemented only for `Memory`
       --> tools/dylint/examples/single_impl_trait.rs:6:1
        |
      6 | pub(crate) trait Store: Send + Sync {
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `dyn Store` always holds `Memory`
       --> tools/dylint/examples/single_impl_trait.rs:127:16
        |
    127 |     store: Arc<dyn Send + Store>,
        |                ^^^^^^^^^^^^^^^^
        = help: replace `Store` with an inherent `impl Memory` block
        = note: `#[warn(single_impl_trait)]` on by default

    warning: trait `Clock` is implemented only for `Fixed`
      --> tools/dylint/examples/single_impl_trait.rs:10:1
       |
    10 | trait Clock {
       | ^^^^^^^^^^^
       |
       = help: replace `Clock` with an inherent `impl Fixed` block

    warning: trait `Lookup` is implemented only for `Borrowed<'_>`
       --> tools/dylint/examples/single_impl_trait.rs:38:1
        |
     38 | trait Lookup {
        | ^^^^^^^^^^^^
        |
    note: `dyn Lookup` always holds `Borrowed<'_>`
       --> tools/dylint/examples/single_impl_trait.rs:158:19
        |
    158 | fn first(lookup: &dyn Lookup) -> Option<u32> { lookup.lookup(0) }
        |                   ^^^^^^^^^^
        = help: replace `Lookup` with an inherent `impl Borrowed<'_>` block

    warning: trait `Adjusted` is implemented only for `F`
      --> tools/dylint/examples/single_impl_trait.rs:46:1
       |
    46 | trait Adjusted {
       | ^^^^^^^^^^^^^^
       |
       = help: replace `Adjusted` with the `Fn(u32) -> u32` bound and retain behavior in generic functions

    warning: trait `Doubled` is implemented only for `u32`
      --> tools/dylint/examples/single_impl_trait.rs:56:1
       |
    56 | trait Doubled {
       | ^^^^^^^^^^^^^
       |
       = help: replace `Doubled` with functions and constants for `u32`

    warning: trait `Handler` is implemented only for `F`
       --> tools/dylint/examples/single_impl_trait.rs:179:1
        |
    179 | trait Handler {
        | ^^^^^^^^^^^^^
        |
        = help: replace `Handler` with the `Fn(u32) -> u32` bound and retain behavior in generic functions

    warning: trait `Source` is implemented only for `Port`
       --> tools/dylint/examples/untyped_error.rs:32:1
        |
     32 | pub trait Source {
        | ^^^^^^^^^^^^^^^^
        |
    note: `dyn Source` always holds `Port`
       --> tools/dylint/examples/untyped_error.rs:440:18
        |
    440 |     let source: &dyn Source = &Port;
        |                  ^^^^^^^^^^
        = help: replace `Source` with an inherent `impl Port` block
        = note: `#[warn(single_impl_trait)]` on by default
    ");
}

fn stringified_uuid(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: field `user_id` holds a `Uuid` rendered to text
      --> tools/dylint/examples/stringified_uuid.rs:26:56
       |
    26 | fn account(name: &str) -> Account { Account { user_id: Uuid::new_v4().to_string(), name: name.to_owned() } }
       |                                                        ^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: declare `user_id` as `newtype_uuid::TypedUuid<K>` for an entity identifier, or as `uuid::Uuid`
       = note: `#[warn(stringified_uuid)]` on by default

    warning: field `key` holds a `Uuid` rendered to text
      --> tools/dylint/examples/stringified_uuid.rs:32:19
       |
    32 |     session.key = fresh.hyphenated().to_string();
       |                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: declare `key` as `newtype_uuid::TypedUuid<K>` for an entity identifier, or as `uuid::Uuid`

    warning: field `key` holds a `Uuid` rendered to text
      --> tools/dylint/examples/stringified_uuid.rs:44:52
       |
    44 | fn formatted(id: Uuid) -> Session { Session { key: format!("{id}") } }
       |                                                    ^^^^^^^^^^^^^^^
       |
       = help: declare `key` as `newtype_uuid::TypedUuid<K>` for an entity identifier, or as `uuid::Uuid`

    warning: field `key` holds a `Uuid` rendered to text
      --> tools/dylint/examples/stringified_uuid.rs:48:52
       |
    48 | fn qualified(id: Uuid) -> Session { Session { key: ToString::to_string(&id.hyphenated()) } }
       |                                                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: declare `key` as `newtype_uuid::TypedUuid<K>` for an entity identifier, or as `uuid::Uuid`
    "#);
}

fn stringly_typed_value(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `owned` holds one of `"a"`, `"b"` as text
      --> tools/dylint/examples/manual_deref_pattern.rs:14:9
       |
    14 |     let owned = name.to_owned();
       |         ^^^^^
       |
    note: branched on here
      --> tools/dylint/examples/manual_deref_pattern.rs:15:5
       |
    15 | /     match owned.as_str() {
    16 | |         "a" => 1,
    17 | |         "b" => 2,
    18 | |         _ => 0,
    19 | |     }
       | |_____^
       = help: declare `owned` as a `strum::EnumString` enum with a variant per value and parse the text where it enters
       = note: `#[warn(stringly_typed_value)]` on by default

    warning: `level` holds one of `"warn"`, `"info"` as text
      --> tools/dylint/examples/stringly_typed_value.rs:14:22
       |
    14 |     fn weight(&self, level: &str) -> usize {
       |                      ^^^^^
       |
    note: branched on here
      --> tools/dylint/examples/stringly_typed_value.rs:15:12
       |
    15 |         if level == "warn" {
       |            ^^^^^^^^^^^^^^^
    16 |             self.base().strict_mul(2)
    17 |         } else if level == "info" {
       |                   ^^^^^^^^^^^^^^^
       = help: declare `level` as a `strum::EnumString` enum with a variant per value and parse the text where it enters
       = note: `#[warn(stringly_typed_value)]` on by default

    warning: field `kind` holds one of `"build"`, `"test"` as text
      --> tools/dylint/examples/stringly_typed_value.rs:34:5
       |
    34 |     kind: String,
       |     ^^^^^^^^^^^^
       |
    note: branched on here
      --> tools/dylint/examples/stringly_typed_value.rs:73:5
       |
    73 | /     match &job.kind {
    74 | |         "build" => build(job),
    75 | |         "test" => test(job),
    76 | |         _ => 0,
    77 | |     }
       | |_____^
       = help: declare `kind` as a `strum::EnumString` enum with a variant per value and parse the text where it enters

    warning: field `state` holds one of `"done"`, `"pending"` as text
      --> tools/dylint/examples/stringly_typed_value.rs:52:5
       |
    52 |     state: &'static str,
       |     ^^^^^^^^^^^^^^^^^^^
       |
    note: branched on here
      --> tools/dylint/examples/stringly_typed_value.rs:82:67
       |
    82 | fn finished(steps: &[Step]) -> usize { steps.iter().filter(|step| step.state == DONE).count() }
       |                                                                   ^^^^^^^^^^^^^^^^^^
       = help: declare `state` as a `strum::EnumString` enum with a variant per value and parse the text where it enters

    warning: `level` holds one of `"warn"`, `"info"` as text
      --> tools/dylint/examples/stringly_typed_value.rs:86:11
       |
    86 | fn logged(level: &str, text: &str) -> String { if level == "warn" { text.to_uppercase() } else { text.to_owned() } }
       |           ^^^^^
       |
    note: branched on here
      --> tools/dylint/examples/stringly_typed_value.rs:86:51
       |
    86 | fn logged(level: &str, text: &str) -> String { if level == "warn" { text.to_uppercase() } else { text.to_owned() } }
       |                                                   ^^^^^^^^^^^^^^^
       = help: declare `level` as a `strum::EnumString` enum with a variant per value and parse the text where it enters

    warning: `mode` holds one of `"fast"`, `"slow"`, `"pause"` as text
       --> tools/dylint/examples/stringly_typed_value.rs:107:9
        |
    107 |     let mode = "fast";
        |         ^^^^
        |
    note: branched on here
       --> tools/dylint/examples/stringly_typed_value.rs:108:5
        |
    108 | /     match mode {
    109 | |         "fast" => input.len(),
    110 | |         "slow" => input.len().strict_mul(2),
    111 | |         PAUSE => input.len().strict_add(1),
    112 | |         _ => 0,
    113 | |     }
        | |_____^
        = help: declare `mode` as a `strum::EnumString` enum with a variant per value and parse the text where it enters

    warning: `level` holds one of `"warn"`, `"info"` as text
       --> tools/dylint/examples/stringly_typed_value.rs:147:12
        |
    147 | fn leveled(level: &str, text: &str) -> usize {
        |            ^^^^^
        |
    note: branched on here
       --> tools/dylint/examples/stringly_typed_value.rs:148:8
        |
    148 |     if level == "warn" {
        |        ^^^^^^^^^^^^^^^
    149 |         text.len()
    150 |     } else if level == "info" {
        |               ^^^^^^^^^^^^^^^
        = help: declare `level` as a `strum::EnumString` enum with a variant per value and parse the text where it enters

    warning: `mode` holds one of `"fast"`, `"slow"` as text
       --> tools/dylint/examples/stringly_typed_value.rs:170:10
        |
    170 | fn speed(mode: &str) -> u32 {
        |          ^^^^
        |
    note: branched on here
       --> tools/dylint/examples/stringly_typed_value.rs:171:5
        |
    171 | /     match mode {
    172 | |         "fast" => 1,
    173 | |         "slow" => 2,
    174 | |         _ => 3,
    175 | |     }
        | |_____^
        = help: declare `mode` as a `strum::EnumString` enum with a variant per value and parse the text where it enters

    warning: field `kind` holds one of `"json"`, `"text"` as text
       --> tools/dylint/examples/stringly_typed_value.rs:188:5
        |
    188 |     pub kind: String,
        |     ^^^^^^^^^^^^^^^^
        |
    note: branched on here
       --> tools/dylint/examples/stringly_typed_value.rs:192:8
        |
    192 |     if request.kind == "json" {
        |        ^^^^^^^^^^^^^^^^^^^^^^
    193 |         1
    194 |     } else if request.kind == "text" {
        |               ^^^^^^^^^^^^^^^^^^^^^^
        = help: declare `kind` as a `strum::EnumString` enum with a variant per value and parse the text where it enters

    warning: text-keyed `tally` is addressed by fixed keys `"ok"`, `"err"`
       --> tools/dylint/examples/stringly_typed_value.rs:164:9
        |
    164 |     let mut tally = BTreeMap::new();
        |         ^^^^^^^^^
        |
    note: addressed here
       --> tools/dylint/examples/stringly_typed_value.rs:165:18
        |
    165 |     tally.insert("ok", words.len());
        |                  ^^^^
    166 |     tally.insert("err", 0);
        |                  ^^^^^
    167 |     tally.get("ok").copied().unwrap_or_default().strict_add(tally["err"])
        |               ^^^^                                                ^^^^^
        = help: key `tally` by an enum with a variant per key, or replace it with a struct holding a field per key
    "#);
}

fn untyped_json_access(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `Value::get` reads `"name"` from an untyped JSON document
      --> tools/dylint/examples/untyped_json_access.rs:49:17
       |
    49 |     Ok(document.get("name").and_then(Value::as_str).map(str::to_owned))
       |                 ^^^^^^^^^^^
       |
       = help: decode the document into a struct deriving `serde::Deserialize`
       = note: `#[warn(untyped_json_access)]` on by default

    warning: `Value::as_str` reads an untyped JSON document by variant
      --> tools/dylint/examples/untyped_json_access.rs:49:38
       |
    49 |     Ok(document.get("name").and_then(Value::as_str).map(str::to_owned))
       |                                      ^^^^^^^^^^^^^
       |
       = help: decode the document into a struct deriving `serde::Deserialize`

    warning: `Value::as_u64` reads an untyped JSON document by variant
      --> tools/dylint/examples/untyped_json_access.rs:54:34
       |
    54 |     Ok(document["size"]["bytes"].as_u64().unwrap_or_default())
       |                                  ^^^^^^^^
       |
       = help: decode the document into a struct deriving `serde::Deserialize`

    warning: indexing reads `["size"]["bytes"]` from an untyped JSON document
      --> tools/dylint/examples/untyped_json_access.rs:54:8
       |
    54 |     Ok(document["size"]["bytes"].as_u64().unwrap_or_default())
       |        ^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: decode the document into a struct deriving `serde::Deserialize`

    warning: `Map::get` reads `"name"` from an untyped JSON document
      --> tools/dylint/examples/untyped_json_access.rs:62:72
       |
    62 | fn object_name(object: &Map<String, Value>) -> Option<&Value> { object.get("name") }
       |                                                                        ^^^^^^^^^^^
       |
       = help: decode the document into a struct deriving `serde::Deserialize`

    warning: `Map::get` reads `"name"` from an untyped JSON document
      --> tools/dylint/examples/untyped_json_access.rs:64:67
       |
    64 | fn constant_name(object: &Map<String, Value>) -> Option<&Value> { Map::get(object, NAME) }
       |                                                                   ^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: decode the document into a struct deriving `serde::Deserialize`

    warning: `Value::pointer` reads `"/items/0"` from an untyped JSON document
      --> tools/dylint/examples/untyped_json_access.rs:68:57
       |
    68 | fn first(document: &Value) -> Option<&Value> { document.pointer("/items/0") }
       |                                                         ^^^^^^^^^^^^^^^^^^^
       |
       = help: decode the document into a struct deriving `serde::Deserialize`

    warning: `Value::String` pattern reads an untyped JSON document by variant
      --> tools/dylint/examples/untyped_json_access.rs:70:55
       |
    70 | fn textual(document: &Value) -> Option<&str> { if let Value::String(text) = document { Some(text) } else { None } }
       |                                                       ^^^^^^^^^^^^^^^^^^^
       |
       = help: decode the document into a struct deriving `serde::Deserialize`

    warning: `Value::as_str` reads an untyped JSON document by variant
      --> tools/dylint/examples/untyped_json_access.rs:76:124
       |
    76 | ...str) -> Option<&'document str> { object.get(key).and_then(Value::as_str) }
       |                                                              ^^^^^^^^^^^^^
       |
       = help: decode the document into a struct deriving `serde::Deserialize`

    warning: `Value::as_str` reads an untyped JSON document by variant
      --> tools/dylint/examples/untyped_json_access.rs:78:164
       |
    78 | ...iter().copied().filter_map(|key| object.get(key).and_then(Value::as_str)).collect() }
       |                                                              ^^^^^^^^^^^^^
       |
       = help: decode the document into a struct deriving `serde::Deserialize`

    warning: `Value::get` reads `"name"` from an untyped JSON document
       --> tools/dylint/examples/untyped_json_access.rs:102:14
        |
    102 |     document.get(key)
        |              ^^^^^^^^
        |
        = help: decode the document into a struct deriving `serde::Deserialize`
    "#);
}

fn variant_constant_functions(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `factor` and `offset` each match every `Unit` variant to one constant
      --> tools/dylint/examples/variant_constant_functions.rs:7:1
       |
     7 | enum Unit {
       | ^^^^^^^^^
       |
    note: matched here
      --> tools/dylint/examples/variant_constant_functions.rs:33:5
       |
    33 |     const fn factor(self) -> f64 {
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    ...
    40 |     const fn offset(self) -> f64 {
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: return the values of `factor` and `offset` from one match on `Unit`
       = note: `#[warn(variant_constant_functions)]` on by default

    warning: `interval` and `timeout` each match every `Pace` variant to one constant
      --> tools/dylint/examples/variant_constant_functions.rs:19:1
       |
    19 | enum Pace {
       | ^^^^^^^^^
       |
    note: matched here
      --> tools/dylint/examples/variant_constant_functions.rs:72:5
       |
    72 |     const fn interval(self) -> Duration {
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    ...
    79 |     const fn timeout(self) -> Duration {
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: replace `Pace` with a struct holding `interval` and `timeout` and one `const` per variant
    ");
}

}
