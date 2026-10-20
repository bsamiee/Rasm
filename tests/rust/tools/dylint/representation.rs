// --- [OPERATIONS] ----------------------------------------------------------------------

fn fn_pointer_iterator_type(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `IntoIter` names `std::iter::Map<std::vec::IntoIter<u8>, fn(u8) -> u16>` through a `fn` pointer in place of its closure
      --> tools/dylint/examples/fn_pointer_iterator_type.rs:18:21
       |
    18 |     type IntoIter = Map<vec::IntoIter<u8>, fn(u8) -> u16>;
       |                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: declare `type IntoIter = impl Iterator<Item = ..>` and build it with a closure under `#![feature(impl_trait_in_assoc_type)]`
       = note: `#[warn(fn_pointer_iterator_type)]` on by default
    ");
}

fn formatted_tracing_value(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: tracing message interpolates values into text
     --> tools/dylint/examples/formatted_tracing_value.rs:6:5
      |
    6 |     tracing::info!("loaded {} rows from {path}", rows);
      |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
      |
      = help: record each value as its own field (`name = %value` or `name = ?value`) beside a constant message
      = note: `#[warn(formatted_tracing_value)]` on by default

    warning: tracing field records a value rendered to text
      --> tools/dylint/examples/formatted_tracing_value.rs:12:27
       |
    12 |     tracing::warn!(port = format!("{port}"), "listening");
       |                           ^^^^^^^^^^^^^^^^^
       |
       = help: record the value itself with `%value` or `?value`, the subscriber formats it

    warning: tracing field records a value rendered to text
      --> tools/dylint/examples/formatted_tracing_value.rs:13:28
       |
    13 |     tracing::debug!(port = port.to_string(), "listening");
       |                            ^^^^^^^^^^^^^^^^
       |
       = help: record the value itself with `%value` or `?value`, the subscriber formats it

    warning: tracing field records a value rendered to text
      --> tools/dylint/examples/formatted_tracing_value.rs:16:55
       |
    16 |     let _span = tracing::info_span!("request", port = format!("{port}"));
       |                                                       ^^^^^^^^^^^^^^^^^
       |
       = help: record the value itself with `%value` or `?value`, the subscriber formats it

    warning: tracing field records a value rendered to text
      --> tools/dylint/examples/formatted_tracing_value.rs:20:19
       |
    20 |     let message = format!("handled {count} rows");
       |                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: record the value itself with `%value` or `?value`, the subscriber formats it

    warning: tracing field records a value rendered to text
      --> tools/dylint/examples/formatted_tracing_value.rs:22:31
       |
    22 |     tracing::info!(summary = %format_args!("handled {count}"), "done");
       |                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: record the value itself with `%value` or `?value`, the subscriber formats it
    "#);
}

fn forwarding_newtype(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `impl AsRef<[String]> for Names` returns the wrapped `Vec<String>` by hand
      --> tools/dylint/examples/forwarding_newtype.rs:15:1
       |
    15 | impl AsRef<[String]> for Names {
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: derive `derive_more::AsRef` on `Names` with `#[as_ref(forward)]` and delete the impl
       = note: `#[warn(forwarding_newtype)]` on by default

    warning: newtype `Queue` wraps `Vec<u32>`, and every inherent method other than a constructor forwards to it
      --> tools/dylint/examples/forwarding_newtype.rs:53:1
       |
    53 | struct Queue(Vec<u32>);
       | ^^^^^^^^^^^^
       |
    note: forwards to `Vec<u32>`
      --> tools/dylint/examples/forwarding_newtype.rs:56:5
       |
    56 |     fn push(&mut self, value: u32) { self.0.push(value); }
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    57 |
    58 |     const fn len(&self) -> usize { self.0.len() }
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: use `Vec<u32>` in place of `Queue`

    warning: newtype `Public` wraps `Vec<u32>`, and every inherent method other than a constructor forwards to it
      --> tools/dylint/examples/forwarding_newtype.rs:62:1
       |
    62 | pub struct Public(Vec<u32>);
       | ^^^^^^^^^^^^^^^^^
       |
    note: forwards to `Vec<u32>`
      --> tools/dylint/examples/forwarding_newtype.rs:66:5
       |
    66 |     pub const fn len(&self) -> usize { self.0.len() }
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: use `Vec<u32>` in place of `Public`

    warning: `impl AsRef<String> for Title` returns the wrapped `String` by hand
      --> tools/dylint/examples/forwarding_newtype.rs:85:1
       |
    85 | impl AsRef<String> for Title {
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: derive `derive_more::AsRef` on `Title` and delete the impl

    warning: `impl AsRef<str> for Title` returns the wrapped `String` by hand
      --> tools/dylint/examples/forwarding_newtype.rs:89:1
       |
    89 | impl AsRef<str> for Title {
       | ^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: derive `derive_more::AsRef` on `Title` with `#[as_ref(forward)]` and delete the impl

    warning: `impl AsMut<String> for Title` returns the wrapped `String` by hand
      --> tools/dylint/examples/forwarding_newtype.rs:93:1
       |
    93 | impl AsMut<String> for Title {
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: derive `derive_more::AsMut` on `Title` and delete the impl

    warning: `impl Deref for Names` exposes every method of the wrapped `Vec<String>` on `Names`
      --> tools/dylint/examples/forwarding_newtype_deref.rs:12:1
       |
    12 | impl Deref for Names {
       | ^^^^^^^^^^^^^^^^^^^^
       |
       = help: use `Vec<String>` in place of `Names`, or give `Names` methods that keep an invariant of its own
       = note: `#[warn(forwarding_newtype)]` on by default

    warning: `impl DerefMut for Names` exposes every method of the wrapped `Vec<String>` on `Names`
      --> tools/dylint/examples/forwarding_newtype_deref.rs:18:1
       |
    18 | impl DerefMut for Names {
       | ^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: use `Vec<String>` in place of `Names`, or give `Names` methods that keep an invariant of its own

    warning: `impl Deref for Scores` exposes every method of the wrapped `Vec<u32>` on `Scores`
      --> tools/dylint/examples/forwarding_newtype_deref.rs:22:26
       |
    22 | #[derive(Debug, Default, derive_more::Deref)]
       |                          ^^^^^^^^^^^^^^^^^^
       |
       = help: use `Vec<u32>` in place of `Scores`, or give `Scores` methods that keep an invariant of its own

    warning: `impl Deref for Exported` exposes every method of the wrapped `Vec<u8>` on `Exported`
      --> tools/dylint/examples/forwarding_newtype_deref.rs:50:1
       |
    50 | impl Deref for Exported {
       | ^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: use `Vec<u8>` in place of `Exported`, or give `Exported` methods that keep an invariant of its own
    ");
}

fn manual_enum_map(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `BTreeMap` built with one entry per variant of `Unit`
      --> tools/dylint/examples/manual_enum_map.rs:21:18
       |
    21 |     let counts = BTreeMap::from([(Unit::Meter, 0_u32), (Unit::Foot, 0)]);
       |                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: build the map with `enum_map::enum_map!` over `#[derive(enum_map::Enum)]` on `Unit`
       = note: `#[warn(manual_enum_map)]` on by default

    warning: `BTreeMap` built with one entry per variant of `Unit`
      --> tools/dylint/examples/manual_enum_map.rs:22:21
       |
    22 |     let collected = [(Unit::Meter, 1_u32), (Unit::Foot, 2)].into_iter().collect::<BTreeMap<_, _>>();
       |                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: build the map with `enum_map::enum_map!` over `#[derive(enum_map::Enum)]` on `Unit`

    warning: `HashMap` built with one entry per variant of `Unit`
      --> tools/dylint/examples/manual_enum_map.rs:27:37
       |
    27 | fn hashed() -> HashMap<Unit, u32> { HashMap::from([(Unit::Meter, 0_u32), (Unit::Foot, 0)]) }
       |                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: build the map with `enum_map::enum_map!` over `#[derive(enum_map::Enum)]` on `Unit`

    warning: `BTreeMap` built with one entry per variant of `Unit`
      --> tools/dylint/examples/manual_enum_map_collect.rs:32:20
       |
    32 |     let iterated = Unit::iter().map(|unit| (unit, weight(unit))).collect::<BTreeMap<_, _>>();
       |                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: build the map with `enum_map::enum_map!` over `#[derive(enum_map::Enum)]` on `Unit`
       = note: `#[warn(manual_enum_map)]` on by default

    warning: `BTreeMap` built with one entry per variant of `Unit`
      --> tools/dylint/examples/manual_enum_map_collect.rs:33:18
       |
    33 |     let listed = Unit::VARIANTS.iter().map(|unit| (*unit, weight(*unit))).collect::<BTreeMap<_, _>>();
       |                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: build the map with `enum_map::enum_map!` over `#[derive(enum_map::Enum)]` on `Unit`

    warning: `BTreeMap` built with one entry per variant of `Unit`
      --> tools/dylint/examples/manual_enum_map_collect.rs:34:18
       |
    34 |     let copied = Unit::VARIANTS.iter().copied().map(|unit| (unit, 0_u32)).collect::<BTreeMap<_, _>>();
       |                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: build the map with `enum_map::enum_map!` over `#[derive(enum_map::Enum)]` on `Unit`

    warning: `BTreeMap` built with one entry per variant of `Unit`
      --> tools/dylint/examples/manual_enum_map_collect.rs:37:18
       |
    37 |     let zipped = Unit::iter().zip([1_u32, 3]).collect::<BTreeMap<_, _>>();
       |                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: build the map with `enum_map::enum_map!` over `#[derive(enum_map::Enum)]` on `Unit`

    warning: `BTreeMap` built with one entry per variant of `Unit`
       --> tools/dylint/examples/variant_constant_functions.rs:106:18
        |
    106 |     let counts = BTreeMap::from([(Unit::Meter, 0_u32), (Unit::Foot, 0)]);
        |                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: build the map with `enum_map::enum_map!` over `#[derive(enum_map::Enum)]` on `Unit`
        = note: `#[warn(manual_enum_map)]` on by default
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
       = help: pass the value to `ContentBlock::json`, which serializes it and calls `ContentBlock::text`
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
       = help: pass the value to `ContentBlock::json`, which serializes it and calls `ContentBlock::text`

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
       = help: pass the value to `ContentBlock::json`, which serializes it and calls `ContentBlock::text`
    ");
}

fn manual_slot_map(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `Store` tracks reusable indexes of `items` in free list `next` by hand
     --> tools/dylint/examples/manual_slot_map.rs:7:1
      |
    7 | struct Store<T> {
      | ^^^^^^^^^^^^^^^
      |
      = help: hold `slotmap::SlotMap<K, T>`, `insert` returns a generational key and `remove` frees its slot
      = note: `#[warn(manual_slot_map)]` on by default

    warning: `Pool` tracks reusable indexes of `entries` in free list `vacant` by hand
      --> tools/dylint/examples/manual_slot_map.rs:36:1
       |
    36 | struct Pool {
       | ^^^^^^^^^^^
       |
       = help: hold `slotmap::SlotMap<K, T>`, `insert` returns a generational key and `remove` frees its slot
    ");
}

fn manual_variant_array(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: array lists every variant of `Mode` in declaration order
      --> tools/dylint/examples/manual_variant_array.rs:13:24
       |
    13 | const ALL: [Mode; 2] = [Mode::Fast, Mode::Slow];
       |                        ^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: derive `strum::VariantArray` on `Mode` and read `Mode::VARIANTS`
       = note: `#[warn(manual_variant_array)]` on by default

    warning: array lists every variant of `Mode` in declaration order
      --> tools/dylint/examples/manual_variant_array.rs:19:65
       |
    19 | fn main() { black_box(&([ALL.len(), HOT.len(), PRIORITY.len()], [Mode::Fast, Mode::Slow].contains(&Mode::Slow))); }
       |                                                                 ^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: derive `strum::VariantArray` on `Mode` and read `Mode::VARIANTS`
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
      --> tools/dylint/examples/needless_reference_field.rs:95:55
       |
    95 |     const fn interner(&self) -> Interner<'tcx> { self.cx.tcx }
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
      --> tools/dylint/examples/needless_reference_field.rs:99:40
       |
    99 |     const fn port(&self) -> u16 { self.settings.port }
       |                                        ^^^^^^^^^^^^^
       = help: store the `u16` of `settings.port` in place of `settings`

    warning: `Exposed` holds `settings` by reference only to copy `settings.port`
       --> tools/dylint/examples/needless_reference_field.rs:39:5
        |
     39 |     pub settings: &'settings Settings,
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `settings.port` copied here
       --> tools/dylint/examples/needless_reference_field.rs:109:40
        |
    109 |     const fn port(&self) -> u16 { self.settings.port }
        |                                        ^^^^^^^^^^^^^
        = help: store the `u16` of `settings.port` in place of `settings` and remove `'settings`

    warning: `Derived` holds `settings` by reference only to copy `settings.port`
       --> tools/dylint/examples/needless_reference_field.rs:68:5
        |
     68 |     settings: &'settings Settings,
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `settings.port` copied here
       --> tools/dylint/examples/needless_reference_field.rs:142:40
        |
    142 |     const fn port(&self) -> u16 { self.settings.port }
        |                                        ^^^^^^^^^^^^^
        = help: store the `u16` of `settings.port` in place of `settings` and remove `'settings`

    warning: `Tagged` holds `0` by reference only to copy `0.retries`
       --> tools/dylint/examples/needless_reference_field.rs:88:26
        |
     88 | struct Tagged<'settings>(&'settings Settings);
        |                          ^^^^^^^^^^^^^^^^^^^
        |
    note: `0.retries` copied here
       --> tools/dylint/examples/needless_reference_field.rs:158:42
        |
    158 |     const fn retries(&self) -> u8 { self.0.retries }
        |                                          ^^^^^^^^^
        = help: store the `u8` of `0.retries` in place of `0` and remove `'settings`
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
       = help: replace `Label` with its field type `std::string::String`
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
       = help: replace `port` and `failure` with one `Result`
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
       = help: replace `value` and `refusal` with one `Result`

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
       = help: replace `value` and `error` with one `Result`
    ");
}

fn primitive_domain_binding(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `wait_millis` holds a duration as `u64`
      --> tools/dylint/examples/primitive_domain_binding.rs:54:9
       |
    54 |     let wait_millis = base.saturating_mul(2);
       |         ^^^^^^^^^^^
       |
       = help: declare a `core::time::Duration` and convert where a protocol or API takes the number
       = note: `#[warn(primitive_domain_binding)]` on by default

    warning: `retry_delay` holds a duration as `u64`
      --> tools/dylint/examples/primitive_domain_binding.rs:55:9
       |
    55 |     let retry_delay = 500_u64;
       |         ^^^^^^^^^^^
       |
       = help: declare a `core::time::Duration` and convert where a protocol or API takes the number

    warning: `cache_dir` holds a path as `std::string::String`
      --> tools/dylint/examples/primitive_domain_binding.rs:60:9
       |
    60 |     let cache_dir = format!("{root}/cache");
       |         ^^^^^^^^^
       |
       = help: declare a `PathBuf` or `&Path`

    warning: `data_file` holds a path as `std::string::String`
      --> tools/dylint/examples/primitive_domain_binding.rs:61:9
       |
    61 |     let data_file = format!("{root}/data.json");
       |         ^^^^^^^^^
       |
       = help: declare a `PathBuf` or `&Path`

    warning: `log_file` holds a path as `std::string::String`
      --> tools/dylint/examples/primitive_domain_binding.rs:62:9
       |
    62 |     let log_file = format!("{root}/run.log");
       |         ^^^^^^^^
       |
       = help: declare a `PathBuf` or `&Path`

    warning: `created_at` holds an instant as `i64`
      --> tools/dylint/examples/primitive_domain_binding.rs:72:9
       |
    72 |     let created_at = base.saturating_add(1);
       |         ^^^^^^^^^^
       |
       = help: declare a `jiff::Timestamp`

    warning: `since_epoch` holds an instant as `i64`
      --> tools/dylint/examples/primitive_domain_binding.rs:73:9
       |
    73 |     let since_epoch = base.saturating_add(2);
       |         ^^^^^^^^^^^
       |
       = help: declare a `jiff::Timestamp`

    warning: `epoch_ms` holds an instant as `i64`
      --> tools/dylint/examples/primitive_domain_binding.rs:74:9
       |
    74 |     let epoch_ms = base.saturating_add(3);
       |         ^^^^^^^^
       |
       = help: declare a `jiff::Timestamp`

    warning: `timestamp` holds an instant as `i64`
      --> tools/dylint/examples/primitive_domain_binding.rs:75:9
       |
    75 |     let timestamp = base.saturating_add(4);
       |         ^^^^^^^^^
       |
       = help: declare a `jiff::Timestamp`

    warning: `start_date` holds a calendar value as `std::string::String`
      --> tools/dylint/examples/primitive_domain_binding.rs:80:9
       |
    80 |     let start_date = format!("2026-10-{day:02}");
       |         ^^^^^^^^^^
       |
       = help: declare a `jiff::civil::Date`, `Time`, or `DateTime`

    warning: `length_mm` holds a physical quantity as `f64`
      --> tools/dylint/examples/primitive_domain_binding.rs:85:9
       |
    85 |     let length_mm = base * 2.0;
       |         ^^^^^^^^^
       |
       = help: declare a `uom::si::f64` quantity (`Length`, `Mass`, `Angle`, `Pressure`, `Power`, `Energy`, `ThermodynamicTemperature`, `Frequency`, `Ratio`)

    warning: `angle_degrees` holds a physical quantity as `f64`
      --> tools/dylint/examples/primitive_domain_binding.rs:86:9
       |
    86 |     let angle_degrees = length_mm / 3.0;
       |         ^^^^^^^^^^^^^
       |
       = help: declare a `uom::si::f64` quantity (`Length`, `Mass`, `Angle`, `Pressure`, `Power`, `Energy`, `ThermodynamicTemperature`, `Frequency`, `Ratio`)

    warning: `declared_ms` holds a duration as `u64`
      --> tools/dylint/examples/primitive_domain_binding.rs:92:9
       |
    92 |     let declared_ms: u64 = base;
       |         ^^^^^^^^^^^
       |
       = help: declare a `core::time::Duration` and convert where a protocol or API takes the number

    warning: `total_bytes` holds a byte size as `u64`
       --> tools/dylint/examples/primitive_domain_binding.rs:103:9
        |
    103 |     let total_bytes = base.saturating_mul(2);
        |         ^^^^^^^^^^^
        |
        = help: declare a `bytesize::ByteSize`

    warning: `load_percent` holds a physical quantity as `f64`
       --> tools/dylint/examples/primitive_domain_binding.rs:109:9
        |
    109 |     let load_percent = base / 2.0;
        |         ^^^^^^^^^^^^
        |
        = help: declare a `uom::si::f64` quantity (`Length`, `Mass`, `Angle`, `Pressure`, `Power`, `Energy`, `ThermodynamicTemperature`, `Frequency`, `Ratio`)

    warning: `retry_delay` holds a duration as `u64`
       --> tools/dylint/examples/primitive_domain_binding.rs:114:9
        |
    114 |     let retry_delay: u64 = 500;
        |         ^^^^^^^^^^^
        |
        = help: declare a `core::time::Duration` and convert where a protocol or API takes the number

    warning: `timeout_ms` holds a duration as `u64`
       --> tools/dylint/examples/primitive_domain_binding.rs:133:23
        |
    133 |     let waited_for = |timeout_ms: u64| timeout_ms.saturating_add(1);
        |                       ^^^^^^^^^^
        |
        = help: declare a `core::time::Duration` and convert where a protocol or API takes the number

    warning: `POLL_INTERVAL` holds a duration as `u64`
      --> tools/dylint/examples/primitive_domain_binding.rs:12:7
       |
    12 | const POLL_INTERVAL: u64 = 250;
       |       ^^^^^^^^^^^^^
       |
       = help: declare a `core::time::Duration` and convert where a protocol or API takes the number

    warning: `CONFIG_PATH` holds a path as `&'static str`
      --> tools/dylint/examples/primitive_domain_binding.rs:13:8
       |
    13 | static CONFIG_PATH: &str = "config.toml";
       |        ^^^^^^^^^^^
       |
       = help: declare a `&Path` through `Path::new` under `#![feature(const_convert)]`

    warning: `timeout_ms` holds a duration as `u64`
      --> tools/dylint/examples/primitive_domain_binding.rs:19:5
       |
    19 |     timeout_ms: u64,
       |     ^^^^^^^^^^
       |
       = help: declare a `core::time::Duration` and convert where a protocol or API takes the number

    warning: `output_dir` holds a path as `std::string::String`
      --> tools/dylint/examples/primitive_domain_binding.rs:20:5
       |
    20 |     output_dir: String,
       |     ^^^^^^^^^^
       |
       = help: declare a `PathBuf` or `&Path`

    warning: `started_at` holds an instant as `i64`
      --> tools/dylint/examples/primitive_domain_binding.rs:22:5
       |
    22 |     started_at: i64,
       |     ^^^^^^^^^^
       |
       = help: declare a `jiff::Timestamp`

    warning: `width_mm` holds a physical quantity as `f64`
      --> tools/dylint/examples/primitive_domain_binding.rs:23:5
       |
    23 |     width_mm: f64,
       |     ^^^^^^^^
       |
       = help: declare a `uom::si::f64` quantity (`Length`, `Mass`, `Angle`, `Pressure`, `Power`, `Energy`, `ThermodynamicTemperature`, `Frequency`, `Ratio`)

    warning: `sample_bytes` holds a byte size as `u64`
      --> tools/dylint/examples/primitive_domain_binding.rs:33:5
       |
    33 |     sample_bytes: u64,
       |     ^^^^^^^^^^^^
       |
       = help: declare a `bytesize::ByteSize`

    warning: `created_at` holds an instant as `i64`
      --> tools/dylint/examples/primitive_domain_binding.rs:34:5
       |
    34 |     created_at: i64,
       |     ^^^^^^^^^^
       |
       = help: declare a `jiff::Timestamp` with `#[serde_as(as = "TimestampSeconds<i64>")]`

    warning: `window_secs` holds a duration as `u64`
      --> tools/dylint/examples/primitive_domain_binding.rs:35:5
       |
    35 |     window_secs: u64,
       |     ^^^^^^^^^^^
       |
       = help: declare a `core::time::Duration` with `#[serde_as(as = "DurationSeconds<u64>")]`

    warning: `payload_bytes` holds a byte size as `usize`
      --> tools/dylint/examples/primitive_domain_binding.rs:48:13
       |
    48 |     Sized { payload_bytes: usize },
       |             ^^^^^^^^^^^^^
       |
       = help: declare a `bytesize::ByteSize`

    warning: `retry_secs` holds a duration as `u64`
       --> tools/dylint/examples/primitive_domain_binding.rs:118:25
        |
    118 | fn scheduled(job: &Job, retry_secs: u64, cache_file: &str, note_file: &str) -> io::Result<String> {
        |                         ^^^^^^^^^^
        |
        = help: declare a `core::time::Duration` and convert where a protocol or API takes the number

    warning: `cache_file` holds a path as `&str`
       --> tools/dylint/examples/primitive_domain_binding.rs:118:42
        |
    118 | fn scheduled(job: &Job, retry_secs: u64, cache_file: &str, note_file: &str) -> io::Result<String> {
        |                                          ^^^^^^^^^^
        |
        = help: declare a `PathBuf` or `&Path`

    warning: `delay_ms` holds a duration as `u64`
     --> tools/dylint/examples/primitive_domain_binding.rs:7:20
      |
    7 |     fn wait(&self, delay_ms: u64);
      |                    ^^^^^^^^
      |
      = help: declare a `core::time::Duration` and convert where a protocol or API takes the number

    warning: `cache_dir` holds a path as `std::string::String`
     --> tools/dylint/examples/primitive_domain_binding_exported.rs:5:9
      |
    5 |     pub cache_dir: String,
      |         ^^^^^^^^^
      |
      = help: declare a `PathBuf` or `&Path`
      = note: `#[warn(primitive_domain_binding)]` on by default

    warning: `log_file` holds a path as `&str`
      --> tools/dylint/examples/primitive_domain_binding_exported.rs:11:15
       |
    11 | pub fn opened(log_file: &str, settings: &Settings) -> usize { log_file.len().saturating_add(settings.label_path.len()) }
       |               ^^^^^^^^
       |
       = help: declare a `PathBuf` or `&Path`
    "#);
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

fn sentinel_default(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `unwrap_or` turns `None` into a `u32` default that a later test reads as absence
      --> tools/dylint/examples/sentinel_default.rs:47:23
       |
    47 |     let count = value.unwrap_or(0);
       |                       ^^^^^^^^^^^^
       |
    note: tested here
      --> tools/dylint/examples/sentinel_default.rs:48:8
       |
    48 |     if count == 0 { "none" } else { "some" }
       |        ^^^^^^^^^^
       = help: keep the `Option` and match `None` where the test acts on absence
       = note: `#[warn(sentinel_default)]` on by default

    warning: `unwrap_or_default` turns `None` into a `Vec<u32>` default that a later test reads as absence
      --> tools/dylint/examples/sentinel_default.rs:51:58
       |
    51 | fn listed_empty(value: Option<Vec<u32>>) -> bool { value.unwrap_or_default().is_empty() }
       |                                                          ^^^^^^^^^^^^^^^^^^^
       |
    note: tested here
      --> tools/dylint/examples/sentinel_default.rs:51:52
       |
    51 | fn listed_empty(value: Option<Vec<u32>>) -> bool { value.unwrap_or_default().is_empty() }
       |                                                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: keep the `Option` and match `None` where the test acts on absence

    warning: `unwrap_or` turns `None` into a `&str` default that a later test reads as absence
      --> tools/dylint/examples/sentinel_default.rs:55:16
       |
    53 | macro_rules! tested {
    54 |     ($value:expr) => {
    55 |         $value.unwrap_or("") == ""
       |                ^^^^^^^^^^^^^
    ...
    59 | fn both(left: Option<&str>, right: Option<&str>) -> bool { tested!(left) && tested!(right) }
       |                                                            ------------- in this macro invocation
       |
    note: tested here
      --> tools/dylint/examples/sentinel_default.rs:55:9
       |
    55 |         $value.unwrap_or("") == ""
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^
    ...
    59 | fn both(left: Option<&str>, right: Option<&str>) -> bool { tested!(left) && tested!(right) }
       |                                                            ------------- in this macro invocation
       = help: keep the `Option` and match `None` where the test acts on absence

    warning: `unwrap_or` turns `None` into a `&str` default that a later test reads as absence
      --> tools/dylint/examples/sentinel_default.rs:55:16
       |
    53 | macro_rules! tested {
    54 |     ($value:expr) => {
    55 |         $value.unwrap_or("") == ""
       |                ^^^^^^^^^^^^^
    ...
    59 | fn both(left: Option<&str>, right: Option<&str>) -> bool { tested!(left) && tested!(right) }
       |                                                                             -------------- in this macro invocation
       |
    note: tested here
      --> tools/dylint/examples/sentinel_default.rs:55:9
       |
    55 |         $value.unwrap_or("") == ""
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^
    ...
    59 | fn both(left: Option<&str>, right: Option<&str>) -> bool { tested!(left) && tested!(right) }
       |                                                                             -------------- in this macro invocation
       = help: keep the `Option` and match `None` where the test acts on absence
    "#);
}

fn single_impl_trait(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: trait `Small` is implemented only for `Result<u8, ParseIntError>`
      --> tools/dylint/examples/partition_by_result.rs:10:1
       |
    10 | trait Small {
       | ^^^^^^^^^^^
       |
       = help: delete the trait and declare each method as a function taking the `Result<u8, ParseIntError>`
       = note: `#[warn(single_impl_trait)]` on by default

    warning: trait `Store` is implemented only for `Memory`
       --> tools/dylint/examples/single_impl_trait.rs:6:1
        |
      6 | pub(crate) trait Store: Send + Sync {
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: `dyn Store` always holds `Memory`
       --> tools/dylint/examples/single_impl_trait.rs:113:16
        |
    113 |     store: Arc<dyn Send + Store>,
        |                ^^^^^^^^^^^^^^^^
        = help: replace the trait with an inherent `impl Memory` block
        = note: `#[warn(single_impl_trait)]` on by default

    warning: trait `Clock` is implemented only for `Fixed`
      --> tools/dylint/examples/single_impl_trait.rs:10:1
       |
    10 | trait Clock {
       | ^^^^^^^^^^^
       |
       = help: replace the trait with an inherent `impl Fixed` block

    warning: trait `Lookup` is implemented only for `Borrowed<'_>`
       --> tools/dylint/examples/single_impl_trait.rs:38:1
        |
     38 | trait Lookup {
        | ^^^^^^^^^^^^
        |
    note: `dyn Lookup` always holds `Borrowed<'_>`
       --> tools/dylint/examples/single_impl_trait.rs:144:19
        |
    144 | fn first(lookup: &dyn Lookup) -> Option<u32> { lookup.lookup(0) }
        |                   ^^^^^^^^^^
        = help: replace the trait with an inherent `impl Borrowed<'_>` block

    warning: trait `Doubled` is implemented only for `u32`
      --> tools/dylint/examples/single_impl_trait.rs:46:1
       |
    46 | trait Doubled {
       | ^^^^^^^^^^^^^
       |
       = help: delete the trait and declare each method as a function taking the `u32`

    warning: trait `Handler` is implemented only for `F`
       --> tools/dylint/examples/single_impl_trait.rs:158:1
        |
    158 | trait Handler {
        | ^^^^^^^^^^^^^
        |
        = help: replace `Handler` with the bound on `F`, named as `trait Handler = <bound>;` under `#![feature(trait_alias)]`

    warning: trait `Source` is implemented only for `Port`
       --> tools/dylint/examples/untyped_error.rs:28:1
        |
     28 | pub trait Source {
        | ^^^^^^^^^^^^^^^^
        |
    note: `dyn Source` always holds `Port`
       --> tools/dylint/examples/untyped_error.rs:343:18
        |
    343 |     let source: &dyn Source = &Port;
        |                  ^^^^^^^^^^
        = help: replace the trait with an inherent `impl Port` block
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
       = help: declare `user_id` as `newtype_uuid::TypedUuid<<Kind>>` for an entity identifier or `uuid::Uuid` otherwise, `Display` and serde render either
       = note: `#[warn(stringified_uuid)]` on by default

    warning: field `key` holds a `Uuid` rendered to text
      --> tools/dylint/examples/stringified_uuid.rs:28:50
       |
    28 | fn session(id: Uuid) -> Session { Session { key: format!("session-{id}") } }
       |                                                  ^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: declare `key` as `newtype_uuid::TypedUuid<<Kind>>` for an entity identifier or `uuid::Uuid` otherwise, `Display` and serde render either

    warning: field `key` holds a `Uuid` rendered to text
      --> tools/dylint/examples/stringified_uuid.rs:32:19
       |
    32 |     session.key = fresh.hyphenated().to_string();
       |                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: declare `key` as `newtype_uuid::TypedUuid<<Kind>>` for an entity identifier or `uuid::Uuid` otherwise, `Display` and serde render either
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
       = help: declare an enum deriving `strum::EnumString` with a variant per value, parse the text once where it enters, and match the variants
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
       = help: declare an enum deriving `strum::EnumString` with a variant per value, parse the text once where it enters, and match the variants
       = note: `#[warn(stringly_typed_value)]` on by default

    warning: field `kind` holds one of `"build"`, `"test"` as text
      --> tools/dylint/examples/stringly_typed_value.rs:29:5
       |
    29 |     kind: String,
       |     ^^^^^^^^^^^^
       |
    note: branched on here
      --> tools/dylint/examples/stringly_typed_value.rs:65:5
       |
    65 | /     match &job.kind {
    66 | |         "build" => build(job),
    67 | |         "test" => test(job),
    68 | |         _ => 0,
    69 | |     }
       | |_____^
       = help: declare an enum deriving `strum::EnumString` with a variant per value, parse the text once where it enters, and match the variants

    warning: field `state` holds one of `"done"`, `"pending"` as text
      --> tools/dylint/examples/stringly_typed_value.rs:39:5
       |
    39 |     state: &'static str,
       |     ^^^^^^^^^^^^^^^^^^^
       |
    note: branched on here
      --> tools/dylint/examples/stringly_typed_value.rs:74:67
       |
    74 | fn finished(steps: &[Step]) -> usize { steps.iter().filter(|step| step.state == DONE).count() }
       |                                                                   ^^^^^^^^^^^^^^^^^^
       = help: declare an enum deriving `strum::EnumString` with a variant per value, parse the text once where it enters, and match the variants

    warning: `level` holds one of `"warn"`, `"info"` as text
      --> tools/dylint/examples/stringly_typed_value.rs:78:11
       |
    78 | fn logged(level: &str, text: &str) -> String { if level == "warn" { text.to_uppercase() } else { text.to_owned() } }
       |           ^^^^^
       |
    note: branched on here
      --> tools/dylint/examples/stringly_typed_value.rs:78:51
       |
    78 | fn logged(level: &str, text: &str) -> String { if level == "warn" { text.to_uppercase() } else { text.to_owned() } }
       |                                                   ^^^^^^^^^^^^^^^
       = help: declare an enum deriving `strum::EnumString` with a variant per value, parse the text once where it enters, and match the variants

    warning: `mode` holds one of `"fast"`, `"slow"`, `"pause"` as text
      --> tools/dylint/examples/stringly_typed_value.rs:91:9
       |
    91 |     let mode = "fast";
       |         ^^^^
       |
    note: branched on here
      --> tools/dylint/examples/stringly_typed_value.rs:92:5
       |
    92 | /     match mode {
    93 | |         "fast" => input.len(),
    94 | |         "slow" => input.len().strict_mul(2),
    95 | |         PAUSE => input.len().strict_add(1),
    96 | |         _ => 0,
    97 | |     }
       | |_____^
       = help: declare an enum deriving `strum::EnumString` with a variant per value, parse the text once where it enters, and match the variants

    warning: `level` holds one of `"warn"`, `"info"` as text
       --> tools/dylint/examples/stringly_typed_value.rs:131:12
        |
    131 | fn leveled(level: &str, text: &str) -> usize {
        |            ^^^^^
        |
    note: branched on here
       --> tools/dylint/examples/stringly_typed_value.rs:132:8
        |
    132 |     if level == "warn" {
        |        ^^^^^^^^^^^^^^^
    133 |         text.len()
    134 |     } else if level == "info" {
        |               ^^^^^^^^^^^^^^^
        = help: declare an enum deriving `strum::EnumString` with a variant per value, parse the text once where it enters, and match the variants

    warning: `mode` holds one of `"fast"`, `"slow"` as text
       --> tools/dylint/examples/stringly_typed_value.rs:154:10
        |
    154 | fn speed(mode: &str) -> u32 {
        |          ^^^^
        |
    note: branched on here
       --> tools/dylint/examples/stringly_typed_value.rs:155:5
        |
    155 | /     match mode {
    156 | |         "fast" => 1,
    157 | |         "slow" => 2,
    158 | |         _ => 3,
    159 | |     }
        | |_____^
        = help: declare an enum deriving `strum::EnumString` with a variant per value, parse the text once where it enters, and match the variants

    warning: field `kind` holds one of `"json"`, `"text"` as text
       --> tools/dylint/examples/stringly_typed_value.rs:172:5
        |
    172 |     pub kind: String,
        |     ^^^^^^^^^^^^^^^^
        |
    note: branched on here
       --> tools/dylint/examples/stringly_typed_value.rs:176:8
        |
    176 |     if request.kind == "json" {
        |        ^^^^^^^^^^^^^^^^^^^^^^
    177 |         1
    178 |     } else if request.kind == "text" {
        |               ^^^^^^^^^^^^^^^^^^^^^^
        = help: declare an enum deriving `strum::EnumString` with a variant per value, parse the text once where it enters, and match the variants

    warning: text-keyed `tally` is addressed by fixed keys `"ok"`, `"err"`
       --> tools/dylint/examples/stringly_typed_value.rs:148:9
        |
    148 |     let mut tally = BTreeMap::new();
        |         ^^^^^^^^^
        |
    note: addressed here
       --> tools/dylint/examples/stringly_typed_value.rs:149:18
        |
    149 |     tally.insert("ok", words.len());
        |                  ^^^^
    150 |     tally.insert("err", 0);
        |                  ^^^^^
    151 |     tally.get("ok").copied().unwrap_or_default().strict_add(tally["err"])
        |               ^^^^                                                ^^^^^
        = help: key the collection by an enum with a variant per key, or hold a struct with a field per key
    "#);
}

fn unchecked_construction(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: field `0` lets code outside the module set `Port` without the `u16` check of `Port::try_from`
      --> tools/dylint/examples/unchecked_construction.rs:12:28
       |
    12 |     pub(crate) struct Port(pub u16);
       |                            ^^^^^^^
       |
    note: `Port::try_from` declared here
      --> tools/dylint/examples/unchecked_construction.rs:17:9
       |
    17 | ...   fn try_from(value: u16) -> Result<Self, Self::Error> { if value < 1024 { Err(RangeError(u32::from(value))) } else { Ok(Self(va...
       |       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: make `0` private and add an accessor for code outside the module
       = note: `#[warn(unchecked_construction)]` on by default

    warning: field `width` lets code outside the module set `Window` without the `u32` check of `Window::new`
      --> tools/dylint/examples/unchecked_construction.rs:22:9
       |
    22 |         pub width: u32,
       |         ^^^^^^^^^^^^^^
       |
    note: `Window::new` declared here
      --> tools/dylint/examples/unchecked_construction.rs:27:9
       |
    27 | ...   pub(crate) const fn new(width: u32, height: u32) -> Option<Self> { if width == 0 || height == 0 { None } else { Some(Self { wi...
       |       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: make `width` private and add an accessor for code outside the module

    warning: field `height` lets code outside the module set `Window` without the `u32` check of `Window::new`
      --> tools/dylint/examples/unchecked_construction.rs:23:9
       |
    23 |         pub height: u32,
       |         ^^^^^^^^^^^^^^^
       |
    note: `Window::new` declared here
      --> tools/dylint/examples/unchecked_construction.rs:27:9
       |
    27 | ...   pub(crate) const fn new(width: u32, height: u32) -> Option<Self> { if width == 0 || height == 0 { None } else { Some(Self { wi...
       |       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: make `height` private and add an accessor for code outside the module

    warning: `Weight` built from a `u32` parameter skips `Weight::try_from`
      --> tools/dylint/examples/unchecked_construction.rs:42:62
       |
    42 |         pub(crate) const fn unchecked(weight: u32) -> Self { Self(weight) }
       |                                                              ^^^^^^^^^^^^
       |
    note: `Weight::try_from` declared here
      --> tools/dylint/examples/unchecked_construction.rs:38:9
       |
    38 |         fn try_from(value: u32) -> Result<Self, Self::Error> { if value > 500 { Err(RangeError(value)) } else { Ok(Self(value)) } }
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: build `Weight` through `Weight::try_from`

    warning: field `value` lets code outside the module set `Level` without the `u8` check of `Level::new`
      --> tools/dylint/examples/unchecked_construction.rs:67:9
       |
    67 |         pub value: u8,
       |         ^^^^^^^^^^^^^
       |
    note: `Level::new` declared here
      --> tools/dylint/examples/unchecked_construction.rs:72:9
       |
    72 |         pub(crate) fn new(value: u8) -> Option<Self> { (value <= 10).then_some(Self { value, label: "level" }) }
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: make `value` private and add an accessor for code outside the module

    warning: `Weight` built from a `u32` parameter skips `Weight::try_from`
      --> tools/dylint/examples/unchecked_construction.rs:85:57
       |
    85 |     pub(crate) const fn weighed(value: u32) -> Weight { Weight(value) }
       |                                                         ^^^^^^^^^^^^^
       |
    note: `Weight::try_from` declared here
      --> tools/dylint/examples/unchecked_construction.rs:38:9
       |
    38 |         fn try_from(value: u32) -> Result<Self, Self::Error> { if value > 500 { Err(RangeError(value)) } else { Ok(Self(value)) } }
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: build `Weight` through `Weight::try_from`

    warning: derived `Deserialize` builds `Decoded` without the `u16` check of `Decoded::try_from`
      --> tools/dylint/examples/unchecked_construction.rs:88:23
       |
    88 |     pub(crate) struct Decoded(u16);
       |                       ^^^^^^^
       |
    note: `Decoded::try_from` declared here
      --> tools/dylint/examples/unchecked_construction.rs:93:9
       |
    93 | ...   fn try_from(value: u16) -> Result<Self, Self::Error> { if value < 1024 { Err(RangeError(u32::from(value))) } else { Ok(Self(va...
       |       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       = help: decode through `#[serde(try_from = "u16")]` and a `TryFrom<u16>` impl that runs `Decoded::try_from`
    "#);
}

fn unparsed_field(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: field `level` holds text every read parses into `u8`
      --> tools/dylint/examples/unparsed_field.rs:15:5
       |
    15 |     level: String,
       |     ^^^^^^^^^^^^^
       |
    note: parsed here
      --> tools/dylint/examples/unparsed_field.rs:31:64
       |
    31 | fn verbosity(options: &Options) -> Result<u8, ParseIntError> { options.level.parse() }
       |                                                                ^^^^^^^^^^^^^^^^^^^^^
       = help: declare `level` as `u8` and parse the text where it enters, through clap's `value_parser` or `serde_with::DisplayFromStr`
       = note: `#[warn(unparsed_field)]` on by default

    warning: field `level` holds text every read parses into `u8`
      --> tools/dylint/examples/unparsed_field.rs:26:5
       |
    26 |     pub level: String,
       |     ^^^^^^^^^^^^^^^^^
       |
    note: parsed here
      --> tools/dylint/examples/unparsed_field.rs:43:69
       |
    43 | pub fn exported(exported: &Exported) -> Result<u8, ParseIntError> { exported.level.parse() }
       |                                                                     ^^^^^^^^^^^^^^^^^^^^^^
       = help: declare `level` as `u8` and parse the text where it enters, through clap's `value_parser` or `serde_with::DisplayFromStr`
    ");
}

fn untyped_json_access(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `Value::get` accesses an untyped JSON document by a fixed key
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

    warning: indexing accesses an untyped JSON document by a fixed key
      --> tools/dylint/examples/untyped_json_access.rs:54:8
       |
    54 |     Ok(document["size"]["bytes"].as_u64().unwrap_or_default())
       |        ^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: decode the document into a struct deriving `serde::Deserialize`

    warning: `Map::get` accesses an untyped JSON document by a fixed key
      --> tools/dylint/examples/untyped_json_access.rs:62:72
       |
    62 | fn object_name(object: &Map<String, Value>) -> Option<&Value> { object.get("name") }
       |                                                                        ^^^^^^^^^^^
       |
       = help: decode the document into a struct deriving `serde::Deserialize`

    warning: `Map::get` accesses an untyped JSON document by a fixed key
      --> tools/dylint/examples/untyped_json_access.rs:64:67
       |
    64 | fn constant_name(object: &Map<String, Value>) -> Option<&Value> { Map::get(object, NAME) }
       |                                                                   ^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: decode the document into a struct deriving `serde::Deserialize`

    warning: `Value::pointer` accesses an untyped JSON document by a fixed key
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

    warning: `Value::get` accesses an untyped JSON document by a fixed key
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
       = help: return every value from one match on `Unit`
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

// --- [COMPOSITION] ---------------------------------------------------------------------

snapshots!(
    fn_pointer_iterator_type,
    formatted_tracing_value,
    forwarding_newtype,
    manual_enum_map,
    manual_serialization,
    manual_slot_map,
    manual_variant_array,
    needless_reference_field,
    needless_struct,
    option_fields_as_result,
    primitive_domain_binding,
    redundant_option_flag,
    sentinel_default,
    single_impl_trait,
    stringified_uuid,
    stringly_typed_value,
    unchecked_construction,
    unparsed_field,
    untyped_json_access,
    variant_constant_functions
);
