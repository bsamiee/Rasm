// --- [OPERATIONS] ----------------------------------------------------------------------

fn debug_main_error(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `Termination` prints `Failure` from `main` through its derived `Debug`
      --> tools/dylint/examples/debug_main_error.rs:13:14
       |
    13 | fn main() -> Result<(), Failure> {
       |              ^^^^^^^^^^^^^^^^^^^
       |
       = help: return `Result<_, std::error::Report<Failure>>` under `#![feature(error_reporter)]` to print `Display` and the `sources()` chain
       = note: `#[warn(debug_main_error)]` on by default
    ");
}

fn decode_without_path(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `Deserialize::deserialize` decodes `&mut Deserializer<StrRead<'_>>` without tracking the field path
      --> tools/dylint/examples/decode_without_path.rs:30:61
       |
    30 | fn decoded(text: &str) -> Result<Port, serde_json::Error> { Port::deserialize(&mut serde_json::Deserializer::from_str(text)) }
       |                                                             ^^^^^^^^^^^^^^^^^
       |
       = help: decode through `serde_path_to_error::deserialize`, whose error names the failing field path
       = note: `#[warn(decode_without_path)]` on by default

    warning: `Deserialize::deserialize` decodes `&mut Deserializer<StrRead<'_>>` without tracking the field path
      --> tools/dylint/examples/decode_without_path.rs:32:64
       |
    32 | fn qualified(text: &str) -> Result<Reply, serde_json::Error> { <Reply as Deserialize>::deserialize(&mut serde_json::Deserializer::fr...
       |                                                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: decode through `serde_path_to_error::deserialize`, whose error names the failing field path

    warning: `Deserialize::deserialize` decodes `Value` without tracking the field path
      --> tools/dylint/examples/decode_without_path.rs:34:77
       |
    34 | fn converted(value: serde_json::Value) -> Result<Port, serde_json::Error> { Port::deserialize(value) }
       |                                                                             ^^^^^^^^^^^^^^^^^
       |
       = help: decode through `serde_path_to_error::deserialize`, whose error names the failing field path
    ");
}

fn discarded_error(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `unwrap_or` replaces `ParseIntError` with a fallback value
      --> tools/dylint/examples/discarded_error.rs:21:21
       |
    21 |         parse(text).unwrap_or(0),
       |                     ^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`
       = note: `#[warn(discarded_error)]` on by default

    warning: `unwrap_or_default` replaces `ParseIntError` with a fallback value
      --> tools/dylint/examples/discarded_error.rs:22:21
       |
    22 |         parse(text).unwrap_or_default(),
       |                     ^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `map_or` replaces `ParseIntError` with a fallback value
      --> tools/dylint/examples/discarded_error.rs:23:21
       |
    23 |         parse(text).map_or(0, |value| value.strict_add(1)),
       |                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_else` replaces `ParseIntError` with a fallback value
      --> tools/dylint/examples/discarded_error.rs:24:21
       |
    24 |         parse(text).unwrap_or_else(|_| 0),
       |                     ^^^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_else` replaces `ParseIntError` with a fallback value
      --> tools/dylint/examples/discarded_error.rs:25:21
       |
    25 |         parse(text).unwrap_or_else(|_ignored| 0),
       |                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `map_or_else` replaces `ParseIntError` with a fallback value
      --> tools/dylint/examples/discarded_error.rs:26:21
       |
    26 |         parse(text).map_or_else(|_| 0, |value| value.strict_add(1)),
       |                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `or` turns `ParseIntError` into success
      --> tools/dylint/examples/discarded_error.rs:30:76
       |
    30 | fn successes(text: &str) -> [Result<(), ParseIntError>; 2] { [effect(text).or(Ok(())), effect(text).or_else(|_| Ok(()))] }
       |                                                                            ^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `or_else` turns `ParseIntError` into success
      --> tools/dylint/examples/discarded_error.rs:30:101
       |
    30 | fn successes(text: &str) -> [Result<(), ParseIntError>; 2] { [effect(text).or(Ok(())), effect(text).or_else(|_| Ok(()))] }
       |                                                                                                     ^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `ok` turns `ParseIntError` into `None`
      --> tools/dylint/examples/discarded_error.rs:32:54
       |
    32 | fn absences(text: &str) -> Option<u32> { parse(text).ok()?.checked_add(parse(text).ok().map_or(0, |value| value)) }
       |                                                      ^^^^
       |
       = help: return the `Result` with `?`, or convert its error into a caller variant with `map_err`

    warning: `ok` turns `ParseIntError` into `None`
      --> tools/dylint/examples/discarded_error.rs:32:84
       |
    32 | fn absences(text: &str) -> Option<u32> { parse(text).ok()?.checked_add(parse(text).ok().map_or(0, |value| value)) }
       |                                                                                    ^^^^
       |
       = help: return the `Result` with `?`, or convert its error into a caller variant with `map_err`

    warning: `ok` turns `ParseIntError` into `None`
      --> tools/dylint/examples/discarded_error.rs:34:54
       |
    34 | fn returned(text: &str) -> Option<u32> { parse(text).ok() }
       |                                                      ^^^^
       |
       = help: return the `Result` with `?`, or convert its error into a caller variant with `map_err`

    warning: `Result::ok` turns `ParseIntError` into `None`
      --> tools/dylint/examples/discarded_error.rs:44:57
       |
    44 |         texts.iter().map(|text| parse(text)).filter_map(Result::ok).collect(),
       |                                                         ^^^^^^^^^^
       |
       = help: return the `Result` with `?`, or convert its error into a caller variant with `map_err`

    warning: `is_ok_and` tests the `Result` and drops `ParseIntError`
      --> tools/dylint/examples/discarded_error.rs:57:21
       |
    57 |         parse(text).is_ok_and(|value| value > 1),
       |                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result` and handle `Err` in place of the test

    warning: `is_ok` tests the `Result` and drops `ParseIntError`
      --> tools/dylint/examples/discarded_error.rs:58:21
       |
    58 |         parse(text).is_ok(),
       |                     ^^^^^^^
       |
       = help: match the `Result` and handle `Err` in place of the test

    warning: `is_err` tests the `Result` and drops `ParseIntError`
      --> tools/dylint/examples/discarded_error.rs:59:21
       |
    59 |         parse(text).is_err(),
       |                     ^^^^^^^^
       |
       = help: match the `Result` and handle `Err` in place of the test

    warning: `is_err_and` tests the `Result` and drops `ParseIntError`
      --> tools/dylint/examples/discarded_error.rs:60:21
       |
    60 |         parse(text).is_err_and(|_| true),
       |                     ^^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result` and handle `Err` in place of the test

    warning: `err().is_none` tests the `Result` and drops `ParseIntError`
      --> tools/dylint/examples/discarded_error.rs:61:20
       |
    61 |         parse(text).err().is_none(),
       |                    ^^^^^^^^^^^^^^^^
       |
       = help: match the `Result` and handle `Err` in place of the test

    warning: `err().is_some` tests the `Result` and drops `ParseIntError`
      --> tools/dylint/examples/discarded_error.rs:62:20
       |
    62 |         parse(text).err().is_some(),
       |                    ^^^^^^^^^^^^^^^^
       |
       = help: match the `Result` and handle `Err` in place of the test

    warning: `matches!` tests the `Result` and drops `ParseIntError`
      --> tools/dylint/examples/discarded_error.rs:63:9
       |
    63 |         matches!(parse(text), Ok(1)),
       |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result` and handle `Err` in place of the test

    warning: pattern of `if let` leaves `ParseIntError` unmatched
      --> tools/dylint/examples/discarded_error.rs:68:8
       |
    68 |     if let Ok(value) = parse(texts[0]) {
       |        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `if let` drops a `Result` and its unhandled `ParseIntError`
      --> tools/dylint/examples/discarded_error.rs:71:8
       |
    71 |     if let Err(_) = parse(texts[0]) {}
       |        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, or return it with `?`

    warning: `if let` replaces `ParseIntError` with a fallback value
      --> tools/dylint/examples/discarded_error.rs:72:23
       |
    72 |     let fallback = if let Ok(value) = parse(texts[0]) { value } else { 0 };
       |                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: pattern of `let Ok(..) else` leaves `ParseIntError` unmatched
      --> tools/dylint/examples/discarded_error.rs:73:5
       |
    73 |     let Ok(start) = parse(texts[0]) else { return fallback };
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `Err(_) =>` replaces `ParseIntError` with a fallback value
      --> tools/dylint/examples/discarded_error.rs:76:9
       |
    76 |         Err(_) => 0,
       |         ^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `Err(_) =>` drops a `Result` and its unhandled `ParseIntError`
      --> tools/dylint/examples/discarded_error.rs:97:13
       |
    97 |             Err(_) => continue,
       |             ^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, or return it with `?`

    warning: `map_or` replaces `PoisonError<MutexGuard<'_, u32>>` with a fallback value
       --> tools/dylint/examples/discarded_error.rs:105:61
        |
    105 | fn locked(shared: &Mutex<u32>) -> [u32; 2] { [shared.lock().map_or(0, |guard| *guard), shared.try_lock().map_or(0, |guard| *guard)] }
        |                                                             ^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `map_or` replaces `TryLockError<MutexGuard<'_, u32>>` with a fallback value
       --> tools/dylint/examples/discarded_error.rs:105:106
        |
    105 | fn locked(shared: &Mutex<u32>) -> [u32; 2] { [shared.lock().map_or(0, |guard| *guard), shared.try_lock().map_or(0, |guard| *guard)] }
        |                                                                                                          ^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `flatten` iterates results and skips each `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:110:43
        |
    110 |     (texts.iter().map(|text| parse(text)).flatten().collect(), parse(texts[0]).into_iter().collect(), parse(texts[0]).iter().count(...
        |                                           ^^^^^^^^^
        |
        = help: collect into `validated::Validated<Vec<_>, _>` and call `.ok()` to keep every error

    warning: `into_iter` iterates the `Result` and skips its `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:110:80
        |
    110 |     (texts.iter().map(|text| parse(text)).flatten().collect(), parse(texts[0]).into_iter().collect(), parse(texts[0]).iter().count(...
        |                                                                                ^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `iter` iterates the `Result` and skips its `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:110:119
        |
    110 |     (texts.iter().map(|text| parse(text)).flatten().collect(), parse(texts[0]).into_iter().collect(), parse(texts[0]).iter().count(...
        |                                                                                                                       ^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `iter_mut` iterates the `Result` and skips its `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:110:151
        |
    110 | ...collect(), parse(texts[0]).iter().count(), parse(texts[0]).iter_mut().count())
        |                                                               ^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `is_ok_and` tests the `Result` and drops `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:113:71
        |
    113 | fn tested(text: &str) -> Result<bool, ParseIntError> { Ok(parse(text).is_ok_and(|value| value > 1)) }
        |                                                                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result` and handle `Err` in place of the test

    warning: `drop` drops a `Result` and its unhandled `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:118:5
        |
    118 |     drop(effect(text));
        |     ^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, or return it with `?`

    warning: `_ =` drops a `Result` and its unhandled `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:119:5
        |
    119 |     _ = effect(text);
        |     ^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, or return it with `?`

    warning: `let _ignored` drops a `Result` and its unhandled `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:120:5
        |
    120 |     let _ignored = effect(text);
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, or return it with `?`

    warning: `unwrap_or_default` replaces `ParseIntError` with a fallback value
       --> tools/dylint/examples/discarded_error.rs:125:22
        |
    123 | macro_rules! defaulted {
    124 |     ($text:expr) => {
    125 |         parse($text).unwrap_or_default()
        |                      ^^^^^^^^^^^^^^^^^^^
    ...
    129 | fn expanded(text: &str) -> u32 { defaulted!(text) }
        |                                  ---------------- in this macro invocation
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or` replaces `TryFromIntError` with a fallback value
       --> tools/dylint/examples/discarded_error.rs:139:52
        |
    139 | fn clamped(value: i64) -> u8 { u8::try_from(value).unwrap_or(1) }
        |                                                    ^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `or_else` turns `ParseIntError` into success
      --> tools/dylint/examples/discarded_error_source.rs:35:64
       |
    35 | fn recovered(text: &str) -> Result<u16, Failure> { parse(text).or_else(|_| Ok(0)) }
       |                                                                ^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`
       = note: `#[warn(discarded_error)]` on by default

    warning: `unwrap_or_default` replaces `ParseIntError` with a fallback value
       --> tools/dylint/examples/duplicate_body.rs:208:59
        |
    208 |     pub(super) fn parse(text: &str) -> u32 { text.parse().unwrap_or_default() }
        |                                                           ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`
        = note: `#[warn(discarded_error)]` on by default

    warning: `unwrap_or_default` replaces `ParseIntError` with a fallback value
       --> tools/dylint/examples/duplicate_body.rs:214:76
        |
    214 |     pub(super) fn parse(text: &str) -> u32 { u32::from_str_radix(text, 16).unwrap_or_default() }
        |                                                                            ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `TryFromIntError` with a fallback value
       --> tools/dylint/examples/duplicate_type.rs:408:52
        |
    408 |     shim.port.strict_add(u16::try_from(shim.limit).unwrap_or_default())
        |                                                    ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`
        = note: `#[warn(discarded_error)]` on by default

    warning: `unwrap_or_default` replaces `PoisonError<MutexGuard<'_, Vec<u8>>>` with a fallback value
      --> tools/dylint/examples/interior_mutable_static.rs:70:50
       |
    70 |     SHARED.lock().map(|mut bytes| bytes.push(1)).unwrap_or_default();
       |                                                  ^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`
       = note: `#[warn(discarded_error)]` on by default

    warning: `unwrap_or_default` replaces `SendError<u32>` with a fallback value
      --> tools/dylint/examples/manual_partition.rs:83:20
       |
    83 |     sender.send(4).unwrap_or_default();
       |                    ^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`
       = note: `#[warn(discarded_error)]` on by default

    warning: `unwrap_or_default` replaces `Error` with a fallback value
      --> tools/dylint/examples/manual_serialization.rs:22:92
       |
    22 | fn defaulted(port: &Port) -> ContentBlock { ContentBlock::text(serde_json::to_string(port).unwrap_or_default()) }
       |                                                                                            ^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`
       = note: `#[warn(discarded_error)]` on by default

    warning: `map_or` replaces `Error` with a fallback value
      --> tools/dylint/examples/needless_import_rename.rs:78:42
       |
    78 |     let rendered = rendered(&Map::new()).map_or(0, |text| text.len());
       |                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`
       = note: `#[warn(discarded_error)]` on by default

    warning: `map_or` replaces `&ParseIntError` with a fallback value
      --> tools/dylint/examples/option_fields_as_result.rs:68:64
       |
    68 | fn outcome(outcome: &Outcome) -> u16 { outcome.result.as_ref().map_or(0, |port| *port) }
       |                                                                ^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`
       = note: `#[warn(discarded_error)]` on by default

    warning: `unwrap_or_default` replaces `ReadError` with a fallback value
       --> tools/dylint/examples/option_fields_as_result.rs:107:56
        |
    107 |         read(Path::new("port")).map(|text| text.len()).unwrap_or_default(),
        |                                                        ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `map_or` replaces `LocatedError` with a fallback value
       --> tools/dylint/examples/redundant_error_conversion.rs:211:26
        |
    211 |         wrapped(b"8083").map_or(Ok(0), parse)?;
        |                          ^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`
        = note: `#[warn(discarded_error)]` on by default

    warning: `map_or` replaces `OuterError` with a fallback value
       --> tools/dylint/examples/redundant_error_conversion.rs:212:26
        |
    212 |         located(b"8084").map_or(Ok(0), inferred)?;
        |                          ^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `map_or` replaces `AttributedError` with a fallback value
       --> tools/dylint/examples/redundant_error_conversion.rs:222:29
        |
    222 |         attributed(b"8093").map_or(Ok(0), parse)?;
        |                             ^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `PortError` with a fallback value
       --> tools/dylint/examples/redundant_error_conversion.rs:225:74
        |
    225 |     black_box((run(), read(Path::new("port")).map(|text| text.len()))).0.unwrap_or_default();
        |                                                                          ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `ParseIntError` with a fallback value
       --> tools/dylint/examples/stringified_error.rs:160:44
        |
    160 |     let mut attempt = counted(parsed(text).unwrap_or_default());
        |                                            ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`
        = note: `#[warn(discarded_error)]` on by default

    warning: `unwrap_or_default` replaces `Error` with a fallback value
       --> tools/dylint/examples/stringly_typed_value.rs:143:34
        |
    143 |     input.read_line(&mut answer).unwrap_or_default();
        |                                  ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`
        = note: `#[warn(discarded_error)]` on by default

    warning: `map_or` replaces `RangeError` with a fallback value
       --> tools/dylint/examples/unchecked_construction.rs:118:37
        |
    118 |     let port = Port::try_from(8080).map_or(0, |port| port.0);
        |                                     ^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`
        = note: `#[warn(discarded_error)]` on by default

    warning: `map_or` replaces `RangeError` with a fallback value
       --> tools/dylint/examples/unchecked_construction.rs:120:41
        |
    120 |     let weights = [Weight::try_from(10).map_or(0, |weight| weight.get()), Weight::unchecked(1).halved().get(), weighed(3).get(), Me...
        |                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `map_or` replaces `Error` with a fallback value
       --> tools/dylint/examples/unchecked_construction.rs:123:64
        |
    123 |     black_box(&(serde_json::from_str::<units::Decoded>("8080").map_or(0, |decoded| decoded.get()), serde_json::from_str::<units::Gu...
        |                                                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `map_or` replaces `Error` with a fallback value
       --> tools/dylint/examples/unchecked_construction.rs:123:147
        |
    123 | ...serde_json::from_str::<units::Guarded>("8080").map_or(0, |guarded| guarded.get())));
        |                                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `ok` turns `PortError` into `None`
       --> tools/dylint/examples/untyped_error.rs:231:76
        |
    231 |     if failures.is_empty() { Ok(texts.iter().filter_map(|text| typed(text).ok()).collect()) } else { Err(failures) }
        |                                                                            ^^^^
        |
        = help: return the `Result` with `?`, or convert its error into a caller variant with `map_err`
        = note: `#[warn(discarded_error)]` on by default

    warning: `ok` turns `PortError` into `None`
       --> tools/dylint/examples/untyped_error.rs:236:100
        |
    236 | fn surveyed(texts: &[&str]) -> Survey { Survey { ports: texts.iter().filter_map(|text| typed(text).ok()).collect(), failures: audit...
        |                                                                                                    ^^^^
        |
        = help: return the `Result` with `?`, or convert its error into a caller variant with `map_err`

    warning: `map_or` replaces `&str` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:262:13
        |
    262 |     present.map_or(Ok(0), |digits| spelled(digits).or_else(|_text| typed(digits)))
        |             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `Code` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:353:21
        |
    353 |         coded(text).unwrap_or_default(),
        |                     ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `Code` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:354:38
        |
    354 |         retried(text, Code::Invalid).unwrap_or_default(),
        |                                      ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `Relocated` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:355:25
        |
    355 |         relocated(text).unwrap_or_default(),
        |                         ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `Box<str>` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:356:26
        |
    356 |         boxed_text(text).unwrap_or_default(),
        |                          ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `Arc<str>` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:357:27
        |
    357 |         shared_text(text).unwrap_or_default(),
        |                           ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `Rc<str>` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:358:28
        |
    358 |         counted_text(text).unwrap_or_default(),
        |                            ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `Arc<dyn Error + Send + Sync>` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:360:28
        |
    360 |         shared_error(text).unwrap_or_default(),
        |                            ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `Rc<dyn Error>` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:361:29
        |
    361 |         counted_error(text).unwrap_or_default(),
        |                             ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `map_or` replaces `Vec<PortError>` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:379:27
        |
    379 |         [planned(&[text]).map_or(0, |found| found.len()), audited(&[text]).len(), survey.ports.len(), survey.failures.len()],
        |                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `RenderError` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:394:46
        |
    394 |         answered(oneshot::channel().1).await.unwrap_or_default(),
        |                                              ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `RenderError` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:395:30
        |
    395 |         infallible(Ok(8080)).unwrap_or_default(),
        |                              ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `ErrorData` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:396:26
        |
    396 |         served(text, 80).unwrap_or_default(),
        |                          ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `ErrorData` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:397:22
        |
    397 |         mapped(text).unwrap_or_default(),
        |                      ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `ErrorData` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:398:40
        |
    398 |         dispatched(text, "ports/read").unwrap_or_default(),
        |                                        ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `ErrorData` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:399:31
        |
    399 |         extracted(Some(text)).unwrap_or_default(),
        |                               ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `TryFromIntError` with a fallback value
       --> tools/dylint/examples/variant_constant_functions.rs:107:141
        |
    107 | ...fset() * f64::from(u32::try_from(unit.symbol().len()).unwrap_or_default())).sum::<f64>();
        |                                                          ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`
        = note: `#[warn(discarded_error)]` on by default
    "#);
}

fn discarded_error_source(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `map_err` replaces `ParseIntError` without reading it
      --> tools/dylint/examples/discarded_error_source.rs:21:60
       |
    21 | fn named(text: &str) -> Result<u16, Failure> { parse(text).map_err(|_error| Failure::Missing) }
       |                                                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: hold `ParseIntError` as `#[source]` of the error the closure builds
       = note: `#[warn(discarded_error_source)]` on by default

    warning: `or_else` replaces `ParseIntError` without reading it
      --> tools/dylint/examples/discarded_error_source.rs:25:62
       |
    25 | fn retried(text: &str) -> Result<u16, Failure> { parse(text).or_else(|_| Err(Failure::Missing)) }
       |                                                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `or_else` replaces `ParseIntError` without reading it
      --> tools/dylint/examples/discarded_error_source.rs:27:68
       |
    27 | fn retried_named(text: &str) -> Result<u16, Failure> { parse(text).or_else(|_ignored| Err(Failure::Missing)) }
       |                                                                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
      --> tools/dylint/examples/untyped_error.rs:51:85
       |
    51 | ...ror> { value.parse().map(Self).map_err(|_error: ParseIntError| String::from("text is not a port")) }
       |                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: hold `ParseIntError` as `#[source]` of the error the closure builds
       = note: `#[warn(discarded_error_source)]` on by default

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:192:61
        |
    192 | pub fn probed(text: &str) -> Result<u16, ()> { text.parse().map_err(|_error: ParseIntError| ()) }
        |                                                             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:194:69
        |
    194 | fn borrowed(text: &str) -> Result<u16, &'static str> { text.parse().map_err(|_error: ParseIntError| "port is not a number") }
        |                                                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:200:64
        |
    200 | fn generic_default(text: &str) -> Fallible<u16> { text.parse().map_err(|_error: ParseIntError| String::from("port is not a number")) }
        |                                                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:207:58
        |
    207 | fn coded(text: &str) -> Result<u16, Code> { text.parse().map_err(|_error: ParseIntError| Code::Invalid) }
        |                                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:209:72
        |
    209 | fn retried(text: &str, code: Code) -> Result<u16, Code> { text.parse().map_err(|_error: ParseIntError| code) }
        |                                                                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:211:67
        |
    211 | fn relocated(text: &str) -> Result<u16, Relocated> { text.parse().map_err(|_error: ParseIntError| Relocated::Moved) }
        |                                                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:213:67
        |
    213 | ...6, Box<str>> { text.parse().map_err(|_error: ParseIntError| Box::from("boxed port is not a number")) }
        |                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:215:68
        |
    215 | ..., Arc<str>> { text.parse().map_err(|_error: ParseIntError| Arc::from("shared port is not a number")) }
        |                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:217:68
        |
    217 | ...6, Rc<str>> { text.parse().map_err(|_error: ParseIntError| Rc::from("counted port is not a number")) }
        |                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:219:79
        |
    219 | ...static, str>> { text.parse().map_err(|_error: ParseIntError| Cow::Borrowed("port is not a number")) }
        |                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:225:65
        |
    225 | fn returned(text: String) -> Result<u16, String> { text.parse().map_err(|_error: ParseIntError| text) }
        |                                                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:227:73
        |
    227 | fn routed(text: &str) -> Result<Reply, Reply> { text.parse().map(Reply).map_err(|_error: ParseIntError| Reply(400)) }
        |                                                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:260:128
        |
    260 | ...else { digits.parse::<u16>().map_err(|_error: ParseIntError| String::from("port is not a number")) };
        |                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:322:60
        |
    322 | fn numbered(text: &str) -> Result<u16, i32> { text.parse().map_err(|_error: ParseIntError| -1) }
        |                                                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
     --> tools/dylint/examples/untyped_error_exported.rs:5:61
      |
    5 | pub fn probed(text: &str) -> Result<u16, ()> { text.parse().map_err(|_error: ParseIntError| ()) }
      |                                                             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
      |
      = help: hold `ParseIntError` as `#[source]` of the error the closure builds
      = note: `#[warn(discarded_error_source)]` on by default
    "#);
}

fn error_field_without_sources(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: tracing field records `Unread` without its sources
      --> tools/dylint/examples/error_field_without_sources.rs:22:124
       |
    22 | fn displayed(text: &str) -> Result<u16, Unread> { parsed(text).inspect_err(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %error, "p...
       |                                                                                                                            ^^^^^
       |
       = help: record `Report::new(error)` under `%` or `field::display`, its text names the error and each source
       = note: `#[warn(error_field_without_sources)]` on by default

    warning: tracing field records `Unread` without its sources
      --> tools/dylint/examples/error_field_without_sources.rs:24:123
       |
    24 | fn debugged(text: &str) -> Result<u16, Unread> { parsed(text).inspect_err(|error| tracing::warn!({ EXCEPTION_MESSAGE } = ?error, "po...
       |                                                                                                                           ^^^^^
       |
       = help: record `Report::new(error)` under `%` or `field::display`, its text names the error and each source

    warning: tracing field records `dyn std::error::Error` without its sources
      --> tools/dylint/examples/error_field_without_sources.rs:26:120
       |
    26 | ...r(|error| tracing::warn!({ EXCEPTION_MESSAGE } = error as &(dyn Error + 'static), "port unread")) }
       |                                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: record `Report::new(error)` under `%` or `field::display`, its text names the error and each source

    warning: tracing field records `std::num::ParseIntError` without its sources
      --> tools/dylint/examples/error_field_without_sources.rs:28:122
       |
    28 | ...spect_err(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %error.source, "port unread")) }
       |                                                              ^^^^^^^^^^^^
       |
       = help: record `Report::new(error)` under `%` or `field::display`, its text names the error and each source

    warning: tracing field records `Unread` without its sources
      --> tools/dylint/examples/error_field_without_sources.rs:30:140
       |
    30 | ...or| drop(tracing::info_span!("port", { EXCEPTION_MESSAGE } = %error))) }
       |                                                                  ^^^^^
       |
       = help: record `Report::new(error)` under `%` or `field::display`, its text names the error and each source

    warning: tracing field records `Unread` without its sources
      --> tools/dylint/examples/error_field_without_sources.rs:34:102
       |
    34 |         tracing::info_span!("port", { EXCEPTION_MESSAGE } = Empty).record(EXCEPTION_MESSAGE, display(error));
       |                                                                                                      ^^^^^
       |
       = help: record `Report::new(error)` under `%` or `field::display`, its text names the error and each source
    "#);
}

fn foreign_error_return(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: exported `load` returns `std::io::Error` from another crate
      --> tools/dylint/examples/foreign_error_return.rs:28:23
       |
    28 |     fn load(&self) -> Result<u16, io::Error>;
       |                       ^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return a local error enum with a variant holding `std::io::Error` as `#[source]`
       = note: `#[warn(foreign_error_return)]` on by default

    warning: exported `open` returns `std::io::Error` from another crate
      --> tools/dylint/examples/foreign_error_return.rs:70:26
       |
    70 |     pub fn open(self) -> io::Result<u16> { self.load() }
       |                          ^^^^^^^^^^^^^^^
       |
       = help: return a local error enum with a variant holding `std::io::Error` as `#[source]`

    warning: exported `parse` returns `std::num::ParseIntError` from another crate
      --> tools/dylint/examples/foreign_error_return.rs:85:29
       |
    85 | pub fn parse(text: &str) -> Result<u16, ParseIntError> { text.parse() }
       |                             ^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return a local error enum with a variant holding `std::num::ParseIntError` as `#[source]`

    warning: exported `fetch` returns `std::num::ParseIntError` from another crate
      --> tools/dylint/examples/foreign_error_return.rs:87:35
       |
    87 | pub async fn fetch(text: &str) -> Result<u16, ParseIntError> {
       |                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return a local error enum with a variant holding `std::num::ParseIntError` as `#[source]`

    warning: exported `shared` returns `std::num::ParseIntError` from another crate
      --> tools/dylint/examples/foreign_error_return.rs:92:30
       |
    92 | pub fn shared(text: &str) -> Result<u16, Arc<ParseIntError>> { parse(text).map_err(Arc::new) }
       |                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return a local error enum with a variant holding `std::num::ParseIntError` as `#[source]`

    warning: exported `expanded` returns `std::num::ParseIntError` from another crate
       --> tools/dylint/examples/foreign_error_return.rs:101:1
        |
    101 | reader!(expanded);
        | ^^^^^^^^^^^^^^^^^
        |
        = help: return a local error enum with a variant holding `std::num::ParseIntError` as `#[source]`

    warning: exported `narrowed` returns `std::num::TryFromIntError` from another crate
       --> tools/dylint/examples/foreign_error_return.rs:109:32
        |
    109 | pub fn narrowed(value: u32) -> Result<Port, TryFromIntError> { Port::try_from(value) }
        |                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return a local error enum with a variant holding `std::num::TryFromIntError` as `#[source]`

    warning: exported `joined` returns `tokio::task::JoinError` from another crate
       --> tools/dylint/examples/foreign_error_return.rs:111:35
        |
    111 | pub async fn joined(port: u16) -> Result<u16, JoinError> { tokio::spawn(async move { port }).await }
        |                                   ^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return a local error enum with a variant holding `tokio::task::JoinError` as `#[source]`

    warning: exported `load` returns `std::io::Error` from another crate
       --> tools/dylint/examples/foreign_error_return.rs:113:29
        |
    113 | pub fn load(path: &Path) -> io::Result<String> { fs_err::read_to_string(path) }
        |                             ^^^^^^^^^^^^^^^^^^
        |
        = help: return a local error enum with a variant holding `std::io::Error` as `#[source]`

    warning: exported `configured` returns `clap::error::Error` from another crate
       --> tools/dylint/examples/foreign_error_return.rs:115:42
        |
    115 | pub fn configured(arguments: &[&str]) -> Result<Options, clap::Error> { Options::try_parse_from(arguments) }
        |                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return a local error enum with a variant holding `clap::error::Error` as `#[source]`
    ");
}

fn independent_question_marks(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `?` on `first` returns before independent `second` is computed
     --> tools/dylint/examples/independent_question_marks.rs:8:5
      |
    8 | /     let first = parse(left)?;
    9 | |     let second = parse(right)?;
      | |_______________________________^
      |
      = help: collect each result into `validated::Validated`, combine them through `map2`..`map4`, and call `.ok()`
      = note: `#[warn(independent_question_marks)]` on by default

    warning: `?` on `first` returns before independent `second` is computed
      --> tools/dylint/examples/independent_question_marks.rs:14:5
       |
    14 | /     let first = parse(left)?;
    15 | |     let second = parse(right)?;
       | |_______________________________^
       |
       = help: collect each result into `validated::Validated`, combine them through `map2`..`map4`, and call `.ok()`

    warning: `?` on `first` returns before independent `second` is computed
      --> tools/dylint/examples/independent_question_marks.rs:20:5
       |
    20 | /     let first = parse(left)?;
    21 | |     let second = parse(right)?;
       | |_______________________________^
       |
       = help: collect each result into `validated::Validated`, combine them through `map2`..`map4`, and call `.ok()`

    warning: `?` on `start` returns before independent `end` is computed
      --> tools/dylint/examples/independent_question_marks.rs:26:5
       |
    26 | /     let start = parse(left)?;
    27 | |     let end = parse(right)?;
       | |____________________________^
       |
       = help: collect each result into `validated::Validated`, combine them through `map2`..`map4`, and call `.ok()`

    warning: `?` on `parse(left)` returns before independent `parse(right)` is computed
      --> tools/dylint/examples/independent_question_marks.rs:62:78
       |
    62 | fn summed_inline(left: &str, right: &str) -> Result<u32, ParseIntError> { Ok(parse(left)?.strict_add(parse(right)?)) }
       |                                                                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: collect each result into `validated::Validated`, combine them through `map2`..`map4`, and call `.ok()`

    warning: `?` on `parse(left)` returns before independent `parse(right)` is computed
      --> tools/dylint/examples/independent_question_marks.rs:64:99
       |
    64 | fn built_inline(left: &str, right: &str) -> Result<Range<u32>, ParseIntError> { Ok(Range { start: parse(left)?, end: parse(right)? }) }
       |                                                                                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: collect each result into `validated::Validated`, combine them through `map2`..`map4`, and call `.ok()`

    warning: `?` on `parse(left.trim())` returns before independent `parse(right)` is computed
      --> tools/dylint/examples/independent_question_marks.rs:66:81
       |
    66 | ...ult<(usize, u32), ParseIntError> { Ok((parse(left.trim())?.to_string().len(), parse(right)?.strict_add(1))) }
       |                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: collect each result into `validated::Validated`, combine them through `map2`..`map4`, and call `.ok()`

    warning: `?` on `spelled(text)` returns before independent `borrowed(text)` is computed
       --> tools/dylint/examples/untyped_error.rs:346:9
        |
    346 | /         spelled(text)?,
    347 | |         borrowed(text)?,
        | |______________________^
        |
        = help: collect each result into `validated::Validated`, combine them through `map2`..`map4`, and call `.ok()`
        = note: `#[warn(independent_question_marks)]` on by default

    warning: `?` on `loaded(Path::new("port"))` returns before independent `reported(Path::new("port")).map_err(projected)` is computed
       --> tools/dylint/examples/untyped_error.rs:384:9
        |
    384 | /         loaded(Path::new("port"))?,
    385 | |         reported(Path::new("port")).map_err(projected)?,
        | |______________________________________________________^
        |
        = help: collect each result into `validated::Validated`, combine them through `map2`..`map4`, and call `.ok()`
    "#);
}

fn io_error_without_context(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `io::Error` built from text holds a failure callers cannot match
      --> tools/dylint/examples/io_error_without_context.rs:13:108
       |
    13 | ...ffer.is_empty() { Ok(0) } else { Err(io::Error::new(io::ErrorKind::InvalidData, "bad magic")) } }
       |                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: build it from a typed error: `io::Error::new(kind, <thiserror value>)`
       = note: `#[warn(io_error_without_context)]` on by default

    warning: `StoreError` takes every `io::Error` through `From` and keeps no operation or subject
      --> tools/dylint/examples/io_error_without_context.rs:21:5
       |
    21 |     Read(#[from] io::Error),
       |     ^^^^
       |
       = help: declare a variant holding an operation enum, the subject, and the `io::Error` source, and build it with `map_err` at each call

    warning: `JournalError` takes every `io::Error` through `From` and keeps no operation or subject
      --> tools/dylint/examples/io_error_without_context.rs:26:1
       |
    26 | struct JournalError {
       | ^^^^^^^^^^^^^^^^^^^
       |
       = help: declare a variant holding an operation enum, the subject, and the `io::Error` source, and build it with `map_err` at each call

    warning: `io::Error` built from text holds a failure callers cannot match
      --> tools/dylint/examples/io_error_without_context.rs:57:19
       |
    57 |         "" => Err(io::Error::new(io::ErrorKind::InvalidData, "port text is empty")),
       |                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: build it from a typed error: `io::Error::new(kind, <thiserror value>)`

    warning: `io::Error` built from text holds a failure callers cannot match
      --> tools/dylint/examples/io_error_without_context.rs:58:20
       |
    58 |         "-" => Err(io::Error::other(format!("port text {text} is a dash"))),
       |                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: build it from a typed error: `io::Error::new(kind, <thiserror value>)`

    warning: `io::Error` built from text holds a failure callers cannot match
      --> tools/dylint/examples/io_error_without_context.rs:68:62
       |
    68 |         Err(LoadError::Read { path: path.to_owned(), source: io::Error::other("path is empty") })
       |                                                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return a `thiserror` variant holding the subject and its typed source

    warning: `ReadError` takes every `io::Error` through `From` and keeps no operation or subject
      --> tools/dylint/examples/redundant_error_conversion.rs:91:5
       |
    91 |     Io(#[source] io::Error),
       |     ^^
       |
       = help: declare a variant holding an operation enum, the subject, and the `io::Error` source, and build it with `map_err` at each call
       = note: `#[warn(io_error_without_context)]` on by default

    warning: `io::Error` built from text holds a failure callers cannot match
       --> tools/dylint/examples/stringified_error.rs:148:49
        |
    148 | fn handed(error: &ParseIntError) -> io::Error { io::Error::other(error.to_string()) }
        |                                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` variant holding the subject and its typed source
        = note: `#[warn(io_error_without_context)]` on by default
    "#);
}

fn manual_error_impl(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `Manual` implements `core::error::Error` by hand
      --> tools/dylint/examples/manual_error_impl.rs:12:1
       |
    12 | impl Error for Manual {}
       | ^^^^^^^^^^^^^^^^^^^^^
       |
       = help: derive `thiserror::Error` with an `#[error(..)]` message and `#[source]` on the wrapped error
       = note: `#[warn(manual_error_impl)]` on by default

    warning: `ManualError` implements `core::error::Error` by hand
       --> tools/dylint/examples/redundant_error_conversion.rs:116:1
        |
    116 | impl error::Error for ManualError {
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: derive `thiserror::Error` with an `#[error(..)]` message and `#[source]` on the wrapped error
        = note: `#[warn(manual_error_impl)]` on by default
    ");
}

fn partition_by_result(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `partition` splits results by case and leaves each half wrapped in `Ok` or `Err`
      --> tools/dylint/examples/partition_by_result.rs:26:84
       |
    26 | fn named(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(passed) }
       |                                                                                    ^^^^^^^^^^^^^^^^^
       |
       = help: call `partition_result::<Vec<_>, Vec<_>, _, _>()` (`use itertools::Itertools as _;`) on owned results and delete the unwraps after it
       = note: `#[warn(partition_by_result)]` on by default

    warning: `partition` splits results by case and leaves each half wrapped in `Ok` or `Err`
      --> tools/dylint/examples/partition_by_result.rs:28:85
       |
    28 | fn pathed(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(Result::is_err) }
       |                                                                                     ^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call `partition_result::<Vec<_>, Vec<_>, _, _>()` (`use itertools::Itertools as _;`) on owned results and delete the unwraps after it

    warning: `partition` splits results by case and leaves each half wrapped in `Ok` or `Err`
      --> tools/dylint/examples/partition_by_result.rs:30:85
       |
    30 | fn closed(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(|result| result.is_ok()) }
       |                                                                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call `partition_result::<Vec<_>, Vec<_>, _, _>()` (`use itertools::Itertools as _;`) on owned results and delete the unwraps after it

    warning: `partition` splits results by case and leaves each half wrapped in `Ok` or `Err`
      --> tools/dylint/examples/partition_by_result.rs:32:86
       |
    32 | fn negated(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(|result| !result.as_ref().is_err()) }
       |                                                                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call `partition_result::<Vec<_>, Vec<_>, _, _>()` (`use itertools::Itertools as _;`) on owned results and delete the unwraps after it

    warning: `partition` splits results by case and leaves each half wrapped in `Ok` or `Err`
      --> tools/dylint/examples/partition_by_result.rs:34:85
       |
    34 | fn called(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(|result| Result::is_ok(result)) }
       |                                                                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call `partition_result::<Vec<_>, Vec<_>, _, _>()` (`use itertools::Itertools as _;`) on owned results and delete the unwraps after it

    warning: `partition` splits results by case and leaves each half wrapped in `Ok` or `Err`
      --> tools/dylint/examples/partition_by_result.rs:36:86
       |
    36 | fn matched(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(|result| matches!(result, Ok(_))) }
       |                                                                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call `partition_result::<Vec<_>, Vec<_>, _, _>()` (`use itertools::Itertools as _;`) on owned results and delete the unwraps after it

    warning: `partition` splits results by case and leaves each half wrapped in `Ok` or `Err`
      --> tools/dylint/examples/partition_by_result.rs:38:91
       |
    38 | fn dereferenced(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(|result| (*result).is_err()) }
       |                                                                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call `partition_result::<Vec<_>, Vec<_>, _, _>()` (`use itertools::Itertools as _;`) on owned results and delete the unwraps after it

    warning: `partition` splits results by case and leaves each half wrapped in `Ok` or `Err`
      --> tools/dylint/examples/partition_by_result.rs:40:148
       |
    40 | ...t<u8, TryFromIntError>>) { results.into_iter().partition(|&result| result.is_ok()) }
       |                                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call `partition_result::<Vec<_>, Vec<_>, _, _>()` (`use itertools::Itertools as _;`) on owned results and delete the unwraps after it

    warning: `partition` splits results by case and leaves each half wrapped in `Ok` or `Err`
      --> tools/dylint/examples/partition_by_result.rs:43:25
       |
    43 |       results.into_iter().partition(|result| match result {
       |  _________________________^
    44 | |         Ok(_) => true,
    45 | |         Err(_) => false,
    46 | |     })
       | |______^
       |
       = help: call `partition_result::<Vec<_>, Vec<_>, _, _>()` (`use itertools::Itertools as _;`) on owned results and delete the unwraps after it
    ");
}

fn redundant_error_conversion(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `impl From<ParseFloatError> for ScaleError` wraps the source in `Parse` by hand
      --> tools/dylint/examples/redundant_error_conversion.rs:68:1
       |
    68 | impl From<ParseFloatError> for ScaleError {
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: source field
      --> tools/dylint/examples/redundant_error_conversion.rs:50:21
       |
    50 |     Parse(#[source] ParseFloatError),
       |                     ^^^^^^^^^^^^^^^
       = help: mark the field `#[from]` and delete the impl
       = note: `#[warn(redundant_error_conversion)]` on by default

    warning: `impl From<Utf8Error> for ScaleError` wraps the source in `Text` by hand
      --> tools/dylint/examples/redundant_error_conversion.rs:72:1
       |
    72 | impl From<Utf8Error> for ScaleError {
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: source field
      --> tools/dylint/examples/redundant_error_conversion.rs:54:9
       |
    54 |         source: Utf8Error,
       |         ^^^^^^^^^^^^^^^^^
       = help: mark the field `#[from]` and delete the impl

    warning: `impl From<Utf8Error> for AttributedError` wraps the source in `Text` by hand
       --> tools/dylint/examples/redundant_error_conversion.rs:105:1
        |
    105 | impl From<Utf8Error> for AttributedError {
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: source field
       --> tools/dylint/examples/redundant_error_conversion.rs:101:20
        |
    101 |     Text(#[source] Utf8Error),
        |                    ^^^^^^^^^
        = help: mark the field `#[from]` and delete the impl

    warning: `map_err` repeats the `From` conversion `?` applies
       --> tools/dylint/examples/redundant_error_conversion.rs:127:37
        |
    127 |     let text = str::from_utf8(bytes).map_err(Into::<PortError>::into)?;
        |                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
    applicability MachineApplicable: remove the `map_err` call

    warning: `map_err` repeats the `From` conversion `?` applies
       --> tools/dylint/examples/redundant_error_conversion.rs:128:35
        |
    128 |     let port = text.trim().parse().map_err(PortError::from)?;
        |                                   ^^^^^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
    applicability MaybeIncorrect: remove the `map_err` call

    warning: `map_err` repeats the `From` conversion `?` applies
       --> tools/dylint/examples/redundant_error_conversion.rs:133:37
        |
    133 |     let text = str::from_utf8(bytes).map_err(PortError::Text)?;
        |                                     ^^^^^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
    applicability MachineApplicable: remove the `map_err` call

    warning: `map_err` repeats the `From` conversion `?` applies
       --> tools/dylint/examples/redundant_error_conversion.rs:143:37
        |
    143 |     let text = str::from_utf8(bytes).map_err(LocatedError)?;
        |                                     ^^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
    applicability MachineApplicable: remove the `map_err` call

    warning: `map_err` repeats the `From` conversion `?` applies
       --> tools/dylint/examples/redundant_error_conversion.rs:153:28
        |
    153 |     let text = text!(bytes).map_err(PortError::from)?;
        |                            ^^^^^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
    applicability MachineApplicable: remove the `map_err` call

    warning: `map_err` repeats the `From` conversion `?` applies
       --> tools/dylint/examples/redundant_error_conversion.rs:159:37
        |
    159 |     let text = str::from_utf8(bytes).map_err(E::from)?;
        |                                     ^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
    applicability MachineApplicable: remove the `map_err` call

    warning: `map_err` repeats the `From` conversion `?` applies
       --> tools/dylint/examples/redundant_error_conversion.rs:164:85
        |
    164 |     let ports = lines.iter().map(|line| line.parse()).collect::<Result<Vec<_>, _>>().map_err(PortError::Parse)?;
        |                                                                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
    applicability MaybeIncorrect: remove the `map_err` call

    warning: `map_err` repeats the `From` conversion `?` applies
       --> tools/dylint/examples/redundant_error_conversion.rs:169:35
        |
    169 |     let port = text.parse::<u16>().map_err(ManualError)?;
        |                                   ^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
    applicability MaybeIncorrect: remove the `map_err` call

    warning: `map_err` repeats the `From` conversion `?` applies
       --> tools/dylint/examples/redundant_error_conversion.rs:175:117
        |
    175 | ...Error> { text.trim().parse::<u16>().map_err(PortError::from)? } }
        |                                       ^^^^^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
    applicability MaybeIncorrect: remove the `map_err` call

    warning: `map_err` repeats the `From` conversion `?` applies
       --> tools/dylint/examples/redundant_error_conversion.rs:180:35
        |
    180 |     let port = text.trim().parse().map_err(|error| PortError::from(error))?;
        |                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
    applicability MaybeIncorrect: remove the `map_err` call

    warning: `map_err` repeats the `From` conversion `?` applies
       --> tools/dylint/examples/redundant_error_conversion.rs:185:37
        |
    185 |     let text = str::from_utf8(bytes).map_err(|error| PortError::Text(error))?;
        |                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
    applicability MachineApplicable: remove the `map_err` call

    warning: `map_err` repeats the `From` conversion `?` applies
       --> tools/dylint/examples/redundant_error_conversion.rs:192:36
        |
    192 |     let scale = text.trim().parse().map_err(ScaleError::Parse)?;
        |                                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
    applicability MaybeIncorrect: remove the `map_err` call

    warning: `map_err` repeats the `From` conversion `?` applies
       --> tools/dylint/examples/redundant_error_conversion.rs:201:43
        |
    201 |             let port = text.trim().parse().map_err(<PortError as From<_>>::from)?;
        |                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
    applicability MaybeIncorrect: remove the `map_err` call

    warning: `map_err` repeats the `From` conversion `?` applies
       --> tools/dylint/examples/redundant_error_conversion.rs:205:48
        |
    205 |             let port: u16 = text.trim().parse().map_err(PortError::Parse)?;
        |                                                ^^^^^^^^^^^^^^^^^^^^^^^^^^ help: remove the `map_err` call
    applicability MaybeIncorrect: remove the `map_err` call
    ");
}

fn source_in_error_message(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `Read` message prints its source field `source`
     --> tools/dylint/examples/source_in_error_message.rs:8:5
      |
    8 |     #[error("cannot read {path}: {source}")]
      |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
      |
      = help: delete field `source` from the message, reporters print it through `source()`
      = note: `#[warn(source_in_error_message)]` on by default

    warning: `Parse` message prints its source field `0`
      --> tools/dylint/examples/source_in_error_message.rs:10:5
       |
    10 |     #[error("cannot parse the port: {0}")]
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete field `0` from the message, reporters print it through `source()`

    warning: `Wrapped` message prints its source field `0`
      --> tools/dylint/examples/source_in_error_message.rs:12:5
       |
    12 |     #[error("{0}")]
       |     ^^^^^^^^^^^^^^^
       |
       = help: state the failure in the message in place of field `0`, reporters print the source through `source()`

    warning: `Write` message prints its source field `cause`
      --> tools/dylint/examples/source_in_error_message.rs:20:5
       |
    20 |     #[error("cannot write {cause:?}")]
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete field `cause` from the message, reporters print it through `source()`

    warning: `Sync` message prints its source field `source`
      --> tools/dylint/examples/source_in_error_message.rs:25:5
       |
    25 |     #[error("cannot sync {}: {}", .path, .source)]
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete field `source` from the message, reporters print it through `source()`

    warning: `Flush` message prints its source field `0`
      --> tools/dylint/examples/source_in_error_message.rs:27:5
       |
    27 |     #[error("cannot flush {reason}", reason = .0)]
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete field `0` from the message, reporters print it through `source()`

    warning: `LoadError` message prints its source field `source`
      --> tools/dylint/examples/source_in_error_message.rs:36:1
       |
    36 | #[error("cannot load the settings: {source}")]
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete field `source` from the message, reporters print it through `source()`
    "#);
}

fn stringified_error(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: text of error `std::num::ParseIntError` is stored where callers cannot match its cause
      --> tools/dylint/examples/option_fields_as_result.rs:90:59
       |
    90 |         Err(error) => Exchange { value: None, error: Some(error.to_string()) },
       |                                                           ^^^^^^^^^^^^^^^^^
       |
       = help: hold the error as `#[source]` of a `thiserror` variant and render it where a log or view prints it
       = note: `#[warn(stringified_error)]` on by default

    warning: text of error `std::num::ParseIntError` is stored where callers cannot match its cause
      --> tools/dylint/examples/stringified_error.rs:43:96
       |
    43 | fn field(text: &str) -> Result<u16, Failure> { parsed(text).map_err(|error| Failure { message: error.to_string() }) }
       |                                                                                                ^^^^^^^^^^^^^^^^^
       |
       = help: hold the error as `#[source]` of a `thiserror` variant and render it where a log or view prints it
       = note: `#[warn(stringified_error)]` on by default

    warning: text of error `std::num::ParseIntError` is stored where callers cannot match its cause
      --> tools/dylint/examples/stringified_error.rs:45:87
       |
    45 | fn wrapped(text: &str) -> Result<u16, Wrapped> { parsed(text).map_err(|error| Wrapped(format!("port: {error}"))) }
       |                                                                                       ^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: hold the error as `#[source]` of a `thiserror` variant and render it where a log or view prints it

    warning: text of error `std::num::ParseIntError` is stored where callers cannot match its cause
      --> tools/dylint/examples/stringified_error.rs:47:102
       |
    47 | fn converted(text: &str) -> Result<u16, Wrapped> { parsed(text).map_err(|error| Wrapped(String::from(error.to_string().as_str()))) }
       |                                                                                                      ^^^^^^^^^^^^^^^^^
       |
       = help: hold the error as `#[source]` of a `thiserror` variant and render it where a log or view prints it

    warning: text of error `std::num::ParseIntError` is stored where callers cannot match its cause
      --> tools/dylint/examples/stringified_error.rs:49:97
       |
    49 | fn constructed(text: &str) -> Result<u16, Failure> { parsed(text).map_err(|error| Failure::new(&error.to_string())) }
       |                                                                                                 ^^^^^^^^^^^^^^^^^
       |
       = help: hold the error as `#[source]` of a `thiserror` variant and render it where a log or view prints it

    warning: text of error `std::num::ParseIntError` is stored where callers cannot match its cause
      --> tools/dylint/examples/stringified_error.rs:58:25
       |
    58 |         attempt.label = error.to_string();
       |                         ^^^^^^^^^^^^^^^^^
       |
       = help: hold the error as `#[source]` of a `thiserror` variant and render it where a log or view prints it

    warning: text of error `std::num::ParseIntError` is stored where callers cannot match its cause
      --> tools/dylint/examples/stringified_error.rs:65:23
       |
    65 |         let message = format!("{error:?}");
       |                       ^^^^^^^^^^^^^^^^^^^^
       |
       = help: hold the error as `#[source]` of a `thiserror` variant and render it where a log or view prints it

    warning: text of error `std::num::ParseIntError` is stored where callers cannot match its cause
      --> tools/dylint/examples/stringified_error.rs:70:78
       |
    70 | fn listed(error: &ParseIntError, messages: &mut Vec<String>) { messages.push(error.to_string()); }
       |                                                                              ^^^^^^^^^^^^^^^^^
       |
       = help: hold the error as `#[source]` of a `thiserror` variant and render it where a log or view prints it

    warning: text of error `std::num::ParseIntError` is returned where callers cannot match its cause
      --> tools/dylint/examples/stringified_error.rs:76:75
       |
    76 | fn collected(errors: &[ParseIntError]) -> Vec<String> { errors.iter().map(ToString::to_string).collect() }
       |                                                                           ^^^^^^^^^^^^^^^^^^^
       |
       = help: hold the error as `#[source]` of a `thiserror` variant and render it where a log or view prints it

    warning: text of error `std::num::ParseIntError` is returned where callers cannot match its cause
      --> tools/dylint/examples/stringified_error.rs:78:49
       |
    78 | fn described(error: &ParseIntError) -> String { error.to_string() }
       |                                                 ^^^^^^^^^^^^^^^^^
       |
       = help: hold the error as `#[source]` of a `thiserror` variant and render it where a log or view prints it

    warning: text of error `std::num::ParseIntError` is returned where callers cannot match its cause
      --> tools/dylint/examples/stringified_error.rs:83:19
       |
    83 |         message = error.to_string();
       |                   ^^^^^^^^^^^^^^^^^
       |
       = help: hold the error as `#[source]` of a `thiserror` variant and render it where a log or view prints it

    warning: text of error `std::num::ParseIntError` is compared
      --> tools/dylint/examples/stringified_error.rs:88:43
       |
    88 | fn equal(error: &ParseIntError) -> bool { error.to_string() == "invalid digit found in string" }
       |                                           ^^^^^^^^^^^^^^^^^
       |
       = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

    warning: text of error `std::io::Error` is compared
      --> tools/dylint/examples/stringified_error.rs:90:43
       |
    90 | fn contained(error: &io::Error) -> bool { ToString::to_string(error).contains("denied") }
       |                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

    warning: text of error `std::num::ParseIntError` is compared
      --> tools/dylint/examples/stringified_error.rs:93:11
       |
    93 |     match format!("{error}") {
       |           ^^^^^^^^^^^^^^^^^^
       |
       = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

    warning: text of error `std::num::ParseIntError` is compared
      --> tools/dylint/examples/stringified_error.rs:99:77
       |
    99 | fn tested(error: &ParseIntError) -> usize { if let Some(("cannot", rest)) = error.to_string().split_once(' ') { rest.len() } else { ...
       |                                                                             ^^^^^^^^^^^^^^^^^
       |
       = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

    warning: text of error `std::num::ParseIntError` is compared
       --> tools/dylint/examples/stringified_error.rs:102:19
        |
    102 |     let message = error.to_string();
        |                   ^^^^^^^^^^^^^^^^^
        |
        = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

    warning: text of error `std::num::ParseIntError` is compared
       --> tools/dylint/examples/stringified_error.rs:106:84
        |
    106 | fn seen(error: &ParseIntError, known: &BTreeSet<String>) -> bool { known.contains(&error.to_string()) }
        |                                                                                    ^^^^^^^^^^^^^^^^^
        |
        = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

    warning: text of `Mode` is compared
       --> tools/dylint/examples/stringified_error.rs:140:31
        |
    140 | fn fast(mode: Mode) -> bool { mode.to_string() == "fast" }
        |                               ^^^^^^^^^^^^^^^^
        |
        = help: compare or match the value itself in place of its text

    warning: text of error `std::num::ParseIntError` is stored where callers cannot match its cause
       --> tools/dylint/examples/stringified_error.rs:146:62
        |
    146 | fn rejected(error: &ParseIntError) -> RefusedError { refused(error.to_string()) }
        |                                                              ^^^^^^^^^^^^^^^^^
        |
        = help: hold the error as `#[source]` of a `thiserror` variant and render it where a log or view prints it

    warning: text of error `std::num::ParseIntError` is stored where callers cannot match its cause
       --> tools/dylint/examples/untyped_error.rs:324:133
        |
    324 | ...p_err(|error: ParseIntError| serde_json::Value::String(error.to_string())) }
        |                                                           ^^^^^^^^^^^^^^^^^
        |
        = help: hold the error as `#[source]` of a `thiserror` variant and render it where a log or view prints it
        = note: `#[warn(stringified_error)]` on by default
    "#);
}

fn untyped_error(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `Result` error `u16` is a primitive that names no failure
       --> tools/dylint/examples/duplicate_type.rs:459:25
        |
    459 | const fn outcome(value: Result<u8, u16>) -> Outcome<u8, u16> {
        |                         ^^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source
        = note: `#[warn(untyped_error)]` on by default

    warning: `Result` error `String` is text callers cannot match
      --> tools/dylint/examples/stringified_error.rs:52:37
       |
    52 |     let port = parsed(text).map_err(|error| error.to_string())?;
       |                                     ^^^^^^^
       |
       = help: return a `thiserror` enum with a variant per failure and its typed source
       = note: `#[warn(untyped_error)]` on by default

    warning: `Result` error `String` is text callers cannot match
      --> tools/dylint/examples/stringified_error.rs:72:65
       |
    72 | fn closure(text: &str) -> Option<String> { parsed(text).map_err(|error| error.to_string()).err() }
       |                                                                 ^^^^^^^
       |
       = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `String` is text callers cannot match
      --> tools/dylint/examples/stringified_error.rs:74:38
       |
    74 | fn errored(error: &ParseIntError) -> Result<u16, String> { Err(error.to_string()) }
       |                                      ^^^^^^^^^^^^^^^^^^^
       |
       = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `Box<dyn Error + Send + Sync>` is an erased `dyn Error` callers cannot match
      --> tools/dylint/examples/untyped_error.rs:25:17
       |
    25 | type Boxed<T> = Result<T, Box<dyn Error + Send + Sync>>;
       |                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return a `thiserror` enum with a variant per failure and its typed source
       = note: `#[warn(untyped_error)]` on by default

    warning: `Result` error `String` is text callers cannot match
      --> tools/dylint/examples/untyped_error.rs:29:23
       |
    29 |     fn read(&self) -> Result<u16, String>;
       |                       ^^^^^^^^^^^^^^^^^^^
       |
       = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `()` is a unit type that names no failure
      --> tools/dylint/examples/untyped_error.rs:42:14
       |
    42 |     outcome: Result<u16, ()>,
       |              ^^^^^^^^^^^^^^^
       |
       = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `String` is text callers cannot match
      --> tools/dylint/examples/untyped_error.rs:49:18
       |
    49 |     type Error = String;
       |                  ^^^^^^
       |
       = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `String` is text callers cannot match
      --> tools/dylint/examples/untyped_error.rs:65:18
       |
    65 |     type Error = String;
       |                  ^^^^^^
       |
       = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: error `Invalid` holds a collection of errors that can be empty
       --> tools/dylint/examples/untyped_error.rs:107:5
        |
    107 |     Invalid(Vec<PortError>),
        |     ^^^^^^^
        |
        = help: hold the errors in a `nonempty_collections::NEVec`

    warning: error `Unreachable` holds an erased `dyn Error` callers cannot match
       --> tools/dylint/examples/untyped_error.rs:143:5
        |
    143 |     Unreachable(#[source] Box<dyn Error + Send + Sync>),
        |     ^^^^^^^^^^^
        |
        = help: give the variant typed fields, with the wrapped error's own type as `#[source]`

    warning: error `Project` holds a report that erases each failure's type
       --> tools/dylint/examples/untyped_error.rs:161:5
        |
    161 |     Project(#[source] anyhow::Error),
        |     ^^^^^^^
        |
        = help: give the variant typed fields, with the wrapped error's own type as `#[source]`

    warning: error `Format` holds a one-value source error that adds nothing to the variant
       --> tools/dylint/examples/untyped_error.rs:167:5
        |
    167 |     Format(#[from] fmt::Error),
        |     ^^^^^^
        |
        = help: delete the field, state the source's cause in the variant's message, and convert with a `map_err` closure matching the source's one value

    warning: error `Abandoned` holds a one-value source error that adds nothing to the variant
       --> tools/dylint/examples/untyped_error.rs:171:5
        |
    171 |     Abandoned(#[from] oneshot::error::RecvError),
        |     ^^^^^^^^^
        |
        = help: delete the field, state the source's cause in the variant's message, and convert with a `map_err` closure matching the source's one value

    warning: `Result` error `String` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:190:27
        |
    190 | fn spelled(text: &str) -> Result<u16, String> { if text.is_empty() { Err(String::from("port text is empty")) } else { Digits::try_f...
        |                           ^^^^^^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `()` is a unit type that names no failure
       --> tools/dylint/examples/untyped_error.rs:192:30
        |
    192 | pub fn probed(text: &str) -> Result<u16, ()> { text.parse().map_err(|_error: ParseIntError| ()) }
        |                              ^^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `&str` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:194:28
        |
    194 | fn borrowed(text: &str) -> Result<u16, &'static str> { text.parse().map_err(|_error: ParseIntError| "port is not a number") }
        |                            ^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `Box<dyn Error + Send + Sync>` is an erased `dyn Error` callers cannot match
       --> tools/dylint/examples/untyped_error.rs:196:25
        |
    196 | fn boxed(text: &str) -> Result<u16, Box<dyn Error + Send + Sync>> { Ok(text.trim().parse()?) }
        |                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `String` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:200:35
        |
    200 | fn generic_default(text: &str) -> Fallible<u16> { text.parse().map_err(|_error: ParseIntError| String::from("port is not a number")) }
        |                                   ^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `String` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:202:33
        |
    202 | async fn awaited(text: &str) -> Result<u16, String> {
        |                                 ^^^^^^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `Code` is a `Display` type that implements no `core::error::Error`
       --> tools/dylint/examples/untyped_error.rs:207:25
        |
    207 | fn coded(text: &str) -> Result<u16, Code> { text.parse().map_err(|_error: ParseIntError| Code::Invalid) }
        |                         ^^^^^^^^^^^^^^^^^
        |
    note: declared here
       --> tools/dylint/examples/untyped_error.rs:85:1
        |
     85 | enum Code {
        | ^^^^^^^^^
        = help: derive `thiserror::Error` on the type in place of its hand-written `Display`

    warning: `Result` error `Code` is a `Display` type that implements no `core::error::Error`
       --> tools/dylint/examples/untyped_error.rs:209:39
        |
    209 | fn retried(text: &str, code: Code) -> Result<u16, Code> { text.parse().map_err(|_error: ParseIntError| code) }
        |                                       ^^^^^^^^^^^^^^^^^
        |
    note: declared here
       --> tools/dylint/examples/untyped_error.rs:85:1
        |
     85 | enum Code {
        | ^^^^^^^^^
        = help: derive `thiserror::Error` on the type in place of its hand-written `Display`

    warning: `Result` error `Box<str>` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:213:30
        |
    213 | fn boxed_text(text: &str) -> Result<u16, Box<str>> { text.parse().map_err(|_error: ParseIntError| Box::from("boxed port is not a nu...
        |                              ^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `Arc<str>` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:215:31
        |
    215 | fn shared_text(text: &str) -> Result<u16, Arc<str>> { text.parse().map_err(|_error: ParseIntError| Arc::from("shared port is not a ...
        |                               ^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `Rc<str>` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:217:32
        |
    217 | fn counted_text(text: &str) -> Result<u16, Rc<str>> { text.parse().map_err(|_error: ParseIntError| Rc::from("counted port is not a ...
        |                                ^^^^^^^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `Cow<'static, str>` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:219:33
        |
    219 | fn borrowed_text(text: &str) -> Result<u16, Cow<'static, str>> { text.parse().map_err(|_error: ParseIntError| Cow::Borrowed("port i...
        |                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `Arc<dyn Error + Send + Sync>` is an erased `dyn Error` callers cannot match
       --> tools/dylint/examples/untyped_error.rs:221:32
        |
    221 | fn shared_error(text: &str) -> Result<u16, Arc<dyn Error + Send + Sync>> { text.parse().map_err(|error: ParseIntError| -> Arc<dyn E...
        |                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `Rc<dyn Error>` is an erased `dyn Error` callers cannot match
       --> tools/dylint/examples/untyped_error.rs:223:33
        |
    223 | fn counted_error(text: &str) -> Result<u16, Rc<dyn Error>> { text.parse().map_err(|error: ParseIntError| -> Rc<dyn Error> { Rc::new...
        |                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `String` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:225:30
        |
    225 | fn returned(text: String) -> Result<u16, String> { text.parse().map_err(|_error: ParseIntError| text) }
        |                              ^^^^^^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `Vec<PortError>` is a collection of errors that can be empty
       --> tools/dylint/examples/untyped_error.rs:229:31
        |
    229 | fn planned(texts: &[&str]) -> Result<Vec<u16>, Vec<PortError>> {
        |                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return `Result<T, NEVec<E>>` built by collecting into `validated::Validated`

    warning: function returns `Vec<PortError>`, where an empty collection means success
       --> tools/dylint/examples/untyped_error.rs:234:31
        |
    234 | fn audited(texts: &[&str]) -> Vec<PortError> { texts.iter().filter_map(|text| typed(text).err()).collect() }
        |                               ^^^^^^^^^^^^^^
        |
        = help: return `Result<(), NEVec<E>>` built by collecting into `validated::Validated`

    warning: `Result` error `String` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:244:77
        |
    244 | fn defaulted(port: Option<u16>) -> Result<u16, PortError> { port.ok_or_else(|| String::from("port is missing")).or_else(|_text| typ...
        |                                                                             ^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `&str` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:259:40
        |
    259 |     let present = if text.is_empty() { Err("port text is empty") } else { Ok(text) };
        |                                        ^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `String` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:260:19
        |
    260 |     let spelled = |digits: &str| if digits.is_empty() { Err(String::from("port text is empty")) } else { digits.parse::<u16>().map_...
        |                   ^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `anyhow::Error` is a report that erases each failure's type
       --> tools/dylint/examples/untyped_error.rs:281:29
        |
    281 | fn reported(path: &Path) -> anyhow::Result<String> { Ok(fs_err::read_to_string(path)?) }
        |                             ^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `anyhow::Error` is a report that erases each failure's type
       --> tools/dylint/examples/untyped_error.rs:285:56
        |
    285 | fn contexted(text: &str) -> Result<u16, ReportError> { text.parse::<u16>().context("port").map_err(ReportError::Project) }
        |                                                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `ErrorData` is built from written text while local `Unserved` converts into it
       --> tools/dylint/examples/untyped_error.rs:311:31
        |
    311 |     if port == reserved { Err(ErrorData::invalid_params(format!("{name} names a reserved port"), None)) } else { Ok(port) }
        |                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: text written here
       --> tools/dylint/examples/untyped_error.rs:311:57
        |
    311 |     if port == reserved { Err(ErrorData::invalid_params(format!("{name} names a reserved port"), None)) } else { Ok(port) }
        |                                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: add a variant holding the subject to `Unserved` and convert it into `ErrorData`

    warning: `ErrorData` is built from written text while local `Unserved` converts into it
       --> tools/dylint/examples/untyped_error.rs:315:78
        |
    315 | ...d_then(|port| if port == 0 { Err(ErrorData::internal_error("port zero cannot be served", None)) } else { Ok(port) })
        |                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: text written here
       --> tools/dylint/examples/untyped_error.rs:315:104
        |
    315 | ...ort| if port == 0 { Err(ErrorData::internal_error("port zero cannot be served", None)) } else { Ok(port) })
        |                                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: add a variant holding the subject to `Unserved` and convert it into `ErrorData`

    warning: `Result` error `i32` is a primitive that names no failure
       --> tools/dylint/examples/untyped_error.rs:322:28
        |
    322 | fn numbered(text: &str) -> Result<u16, i32> { text.parse().map_err(|_error: ParseIntError| -1) }
        |                            ^^^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `Value` is a `Display` type that implements no `core::error::Error`
       --> tools/dylint/examples/untyped_error.rs:324:30
        |
    324 | fn documented(text: &str) -> Result<u16, serde_json::Value> { text.parse().map_err(|error: ParseIntError| serde_json::Value::String...
        |                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `Vec<String>` is a collection of text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:326:28
        |
    326 | fn messages(text: &str) -> Result<u16, Vec<String>> { text.parse().map_err(|error: ParseIntError| vec![error.to_string()]) }
        |                            ^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `(u16, String)` is a tuple that names no failure
       --> tools/dylint/examples/untyped_error.rs:328:26
        |
    328 | fn paired(text: &str) -> Result<u16, (u16, String)> { text.parse().map_err(|error: ParseIntError| (1, error.to_string())) }
        |                          ^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: error `Other` holds a report that erases each failure's type
       --> tools/dylint/examples/untyped_error.rs:333:5
        |
    333 |     Other(#[from] anyhow::Error),
        |     ^^^^^
        |
        = help: give the variant typed fields, with the wrapped error's own type as `#[source]`

    warning: `Result` error `anyhow::Error` is a report that erases each failure's type
       --> tools/dylint/examples/untyped_error.rs:336:86
        |
    336 | fn wrapped(text: &str) -> Result<u16, WrappedError> { Ok(text.parse::<u16>().map_err(anyhow::Error::from)?) }
        |                                                                                      ^^^^^^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `Box<dyn Error + Send + Sync>` is an erased `dyn Error` callers cannot match
       --> tools/dylint/examples/untyped_error.rs:341:20
        |
    341 | async fn main() -> Result<(), Box<dyn Error + Send + Sync>> {
        |                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: `Result` error `()` is a unit type that names no failure
       --> tools/dylint/examples/untyped_error.rs:375:49
        |
    375 |         Pending { outcome: text.parse().map_err(drop) }.outcome.unwrap_or_default(),
        |                                                 ^^^^
        |
        = help: return a `thiserror` enum with a variant per failure and its typed source

    warning: error `Message` holds text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:117:5
        |
    117 |     Message(String),
        |     ^^^^^^^
        |
    note: text written here
       --> tools/dylint/examples/untyped_error.rs:268:60
        |
    268 |         "refused" => Err(ConfigError::Message(String::from("configuration refused the port"))),
        |                                                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: give the variant typed fields, with the wrapped error's own type as `#[source]`

    warning: error `Detail` holds text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:119:5
        |
    119 |     Detail { detail: Box<str>, context: Cow<'static, str> },
        |     ^^^^^^
        |
    note: text written here
       --> tools/dylint/examples/untyped_error.rs:269:65
        |
    269 |         "detail" => Err(ConfigError::Detail { detail: Box::from("port detail"), context: Cow::Borrowed("port context") }),
        |                                                                 ^^^^^^^^^^^^^
        = help: give the variant typed fields, with the wrapped error's own type as `#[source]`

    warning: error `Reserved` holds text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:129:5
        |
    129 |     Reserved { port: String },
        |     ^^^^^^^^
        |
    note: text written here
       --> tools/dylint/examples/untyped_error.rs:270:57
        |
    270 |         "reserved" => Err(ConfigError::Reserved { port: 80_u16.to_string() }),
        |                                                         ^^^^^^^^^^^^^^^^^^
        = help: give the variant typed fields, with the wrapped error's own type as `#[source]`

    warning: error `Composed` holds text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:131:5
        |
    131 |     Composed(Box<str>),
        |     ^^^^^^^^
        |
    note: text written here
       --> tools/dylint/examples/untyped_error.rs:271:49
        |
    271 |         "composed" => Err(ConfigError::Composed(format!("{text} refused the port").into())),
        |                                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: give the variant typed fields, with the wrapped error's own type as `#[source]`

    warning: error `Refusal` holds text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:138:1
        |
    138 | struct Refusal(Arc<str>);
        | ^^^^^^^^^^^^^^
        |
    note: text written here
       --> tools/dylint/examples/untyped_error.rs:277:62
        |
    277 | fn refused() -> Result<u16, Refusal> { Err(Refusal(Arc::from("port refused"))) }
        |                                                              ^^^^^^^^^^^^^^
        = help: give the variant typed fields, with the wrapped error's own type as `#[source]`

    warning: `Result` error `String` is text callers cannot match
     --> tools/dylint/examples/untyped_error_exported.rs:7:33
      |
    7 | pub fn described(text: &str) -> Result<u16, String> { text.parse().map_err(|error: ParseIntError| error.to_string()) }
      |                                 ^^^^^^^^^^^^^^^^^^^
      |
      = help: return a `thiserror` enum with a variant per failure and its typed source
      = note: `#[warn(untyped_error)]` on by default
    "#);
}

// --- [COMPOSITION] ---------------------------------------------------------------------

snapshots!(
    debug_main_error,
    decode_without_path,
    discarded_error,
    discarded_error_source,
    error_field_without_sources,
    foreign_error_return,
    independent_question_marks,
    io_error_without_context,
    manual_error_impl,
    partition_by_result,
    redundant_error_conversion,
    source_in_error_message,
    stringified_error,
    untyped_error
);
