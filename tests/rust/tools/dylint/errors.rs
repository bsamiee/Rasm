// --- [OPERATIONS] ----------------------------------------------------------------------

snapshots! {
fn debug_main_error(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `Termination` prints `Failure` from `main` through its derived `Debug`
      --> tools/dylint/examples/debug_main_error.rs:13:14
       |
    13 | fn main() -> Result<(), Failure> {
       |              ^^^^^^^^^^^^^^^^^^^
       |
       = help: return `Result<(), std::error::Report<Failure>>` under `#![feature(error_reporter)]`
       = note: `#[warn(debug_main_error)]` on by default

    warning: `Termination` prints `Error` from `main` through its derived `Debug`
      --> tools/dylint/examples/macro_formatting.rs:86:14
       |
    86 | fn main() -> fmt::Result {
       |              ^^^^^^^^^^^
       |
       = help: return `Result<(), std::error::Report<Error>>` under `#![feature(error_reporter)]`
       = note: `#[warn(debug_main_error)]` on by default
    ");
}

fn decode_without_path(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `Deserialize::deserialize` decodes `&mut Deserializer<StrRead<'_>>` without tracking the field path
      --> tools/dylint/examples/decode_without_path.rs:44:61
       |
    44 | fn decoded(text: &str) -> Result<Port, serde_json::Error> { Port::deserialize(&mut serde_json::Deserializer::from_str(text)) }
       |                                                             ^^^^^^^^^^^^^^^^^
       |
       = help: decode through `serde_path_to_error::deserialize`
       = note: `#[warn(decode_without_path)]` on by default

    warning: `Deserialize::deserialize` decodes `&mut Deserializer<StrRead<'_>>` without tracking the field path
      --> tools/dylint/examples/decode_without_path.rs:46:64
       |
    46 | fn qualified(text: &str) -> Result<Reply, serde_json::Error> { <Reply as Deserialize>::deserialize(&mut serde_json::Deserializer::fr...
       |                                                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: decode through `serde_path_to_error::deserialize`

    warning: `Deserialize::deserialize` decodes `Value` without tracking the field path
      --> tools/dylint/examples/decode_without_path.rs:48:77
       |
    48 | fn converted(value: serde_json::Value) -> Result<Port, serde_json::Error> { Port::deserialize(value) }
       |                                                                             ^^^^^^^^^^^^^^^^^
       |
       = help: decode through `serde_path_to_error::deserialize`

    warning: `Deserialize::deserialize` decodes `&mut Deserializer<StrRead<'_>>` without tracking the field path
      --> tools/dylint/examples/decode_without_path.rs:63:69
       |
    63 | fn keyword_json(text: &str) -> Result<Keyword, serde_json::Error> { Keyword::deserialize(&mut serde_json::Deserializer::from_str(tex...
       |                                                                     ^^^^^^^^^^^^^^^^^^^^
       |
       = help: decode through `serde_path_to_error::deserialize`

    warning: `Deserialize::deserialize` decodes `StrDeserializer<'_, Error>` without tracking the field path
      --> tools/dylint/examples/decode_without_path.rs:65:62
       |
    65 | fn scalar_record(text: &str) -> Result<Port, value::Error> { Port::deserialize(value::StrDeserializer::new(text)) }
       |                                                              ^^^^^^^^^^^^^^^^^
       |
       = help: decode through `serde_path_to_error::deserialize`

    warning: `Deserialize::deserialize` decodes `StrDeserializer<'_, Error>` without tracking the field path
      --> tools/dylint/examples/decode_without_path.rs:69:65
       |
    69 | fn custom_unit(text: &str) -> Result<Delegated, value::Error> { Delegated::deserialize(value::StrDeserializer::new(text)) }
       |                                                                 ^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: decode through `serde_path_to_error::deserialize`
    ");
}

fn discarded_error(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `unwrap_or` replaces `ParseIntError` with a fallback value
      --> tools/dylint/examples/discarded_error.rs:83:21
       |
    83 |         parse(text).unwrap_or(0),
       |                     ^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`
       = note: `#[warn(discarded_error)]` on by default

    warning: `unwrap_or_default` replaces `ParseIntError` with a fallback value
      --> tools/dylint/examples/discarded_error.rs:84:21
       |
    84 |         parse(text).unwrap_or_default(),
       |                     ^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `map_or` replaces `ParseIntError` with a fallback value
      --> tools/dylint/examples/discarded_error.rs:85:21
       |
    85 |         parse(text).map_or(0, |value| value.strict_add(1)),
       |                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `map_or_default` replaces `ParseIntError` with a fallback value
      --> tools/dylint/examples/discarded_error.rs:86:21
       |
    86 |         parse(text).map_or_default(|value| value.strict_add(1)),
       |                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_else` replaces `ParseIntError` with a fallback value
      --> tools/dylint/examples/discarded_error.rs:87:21
       |
    87 |         parse(text).unwrap_or_else(|_| 0),
       |                     ^^^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_else` replaces `ParseIntError` with a fallback value
      --> tools/dylint/examples/discarded_error.rs:88:21
       |
    88 |         parse(text).unwrap_or_else(|_ignored| 0),
       |                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `map_or_else` replaces `ParseIntError` with a fallback value
      --> tools/dylint/examples/discarded_error.rs:89:21
       |
    89 |         parse(text).map_or_else(|_| 0, |value| value.strict_add(1)),
       |                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `or` turns `ParseIntError` into success
      --> tools/dylint/examples/discarded_error.rs:93:76
       |
    93 | fn successes(text: &str) -> [Result<(), ParseIntError>; 2] { [effect(text).or(Ok(())), effect(text).or_else(|_| Ok(()))] }
       |                                                                            ^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `or_else` turns `ParseIntError` into success
      --> tools/dylint/examples/discarded_error.rs:93:101
       |
    93 | fn successes(text: &str) -> [Result<(), ParseIntError>; 2] { [effect(text).or(Ok(())), effect(text).or_else(|_| Ok(()))] }
       |                                                                                                     ^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `ok` turns `ParseIntError` into `None`
      --> tools/dylint/examples/discarded_error.rs:95:54
       |
    95 | fn absences(text: &str) -> Option<u32> { parse(text).ok()?.checked_add(parse(text).ok().map_or(0, |value| value)) }
       |                                                      ^^^^
       |
       = help: return the `Result` with `?`, or convert its error into a caller variant with `map_err`

    warning: `ok` turns `ParseIntError` into `None`
      --> tools/dylint/examples/discarded_error.rs:95:84
       |
    95 | fn absences(text: &str) -> Option<u32> { parse(text).ok()?.checked_add(parse(text).ok().map_or(0, |value| value)) }
       |                                                                                    ^^^^
       |
       = help: return the `Result` with `?`, or convert its error into a caller variant with `map_err`

    warning: `ok` turns `ParseIntError` into `None`
      --> tools/dylint/examples/discarded_error.rs:97:54
       |
    97 | fn returned(text: &str) -> Option<u32> { parse(text).ok() }
       |                                                      ^^^^
       |
       = help: return the `Result` with `?`, or convert its error into a caller variant with `map_err`

    warning: `is_ok` tests the `Result` and drops `RecvError`
       --> tools/dylint/examples/discarded_error.rs:101:75
        |
    101 | fn closed(receiver: &mpsc::Receiver<u32>) -> [bool; 1] { [receiver.recv().is_ok()] }
        |                                                                           ^^^^^^^
        |
        = help: match the `Result` and handle `Err` in place of the test

    warning: `Result::ok` turns `ParseIntError` into `None`
       --> tools/dylint/examples/discarded_error.rs:107:57
        |
    107 |         texts.iter().map(|text| parse(text)).filter_map(Result::ok).collect(),
        |                                                         ^^^^^^^^^^
        |
        = help: return the `Result` with `?`, or convert its error into a caller variant with `map_err`

    warning: `is_ok_and` tests the `Result` and drops `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:120:21
        |
    120 |         parse(text).is_ok_and(|value| value > 1),
        |                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result` and handle `Err` in place of the test

    warning: `is_ok` tests the `Result` and drops `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:121:21
        |
    121 |         parse(text).is_ok(),
        |                     ^^^^^^^
        |
        = help: match the `Result` and handle `Err` in place of the test

    warning: `is_err` tests the `Result` and drops `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:122:21
        |
    122 |         parse(text).is_err(),
        |                     ^^^^^^^^
        |
        = help: match the `Result` and handle `Err` in place of the test

    warning: `is_err_and` tests the `Result` and drops `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:123:21
        |
    123 |         parse(text).is_err_and(|_| true),
        |                     ^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result` and handle `Err` in place of the test

    warning: `err().is_none` tests the `Result` and drops `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:124:20
        |
    124 |         parse(text).err().is_none(),
        |                    ^^^^^^^^^^^^^^^^
        |
        = help: match the `Result` and handle `Err` in place of the test

    warning: `err().is_some` tests the `Result` and drops `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:125:20
        |
    125 |         parse(text).err().is_some(),
        |                    ^^^^^^^^^^^^^^^^
        |
        = help: match the `Result` and handle `Err` in place of the test

    warning: `matches!` tests the `Result` and drops `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:126:9
        |
    126 |         matches!(parse(text), Ok(1)),
        |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result` and handle `Err` in place of the test

    warning: pattern of `if let` leaves `ParseIntError` unmatched
       --> tools/dylint/examples/discarded_error.rs:131:8
        |
    131 |     if let Ok(value) = parse(texts[0]) {
        |        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `if let` drops a `Result` and its unhandled `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:134:8
        |
    134 |     if let Err(_) = parse(texts[0]) {}
        |        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, or return it with `?`

    warning: `if let` replaces `ParseIntError` with a fallback value
       --> tools/dylint/examples/discarded_error.rs:135:23
        |
    135 |     let fallback = if let Ok(value) = parse(texts[0]) { value } else { 0 };
        |                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: pattern of `let Ok(..) else` leaves `ParseIntError` unmatched
       --> tools/dylint/examples/discarded_error.rs:136:5
        |
    136 |     let Ok(start) = parse(texts[0]) else { return fallback };
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `Err(_) =>` replaces `ParseIntError` with a fallback value
       --> tools/dylint/examples/discarded_error.rs:139:9
        |
    139 |         Err(_) => 0,
        |         ^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `Err(_) =>` drops a `Result` and its unhandled `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:160:13
        |
    160 |             Err(_) => continue,
        |             ^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, or return it with `?`

    warning: `unwrap_or_default` replaces `Rejected<Record>` with a fallback value
       --> tools/dylint/examples/discarded_error.rs:176:55
        |
    176 | fn rejected(record: Record) -> Record { record.step().unwrap_or_default() }
        |                                                       ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `ok` turns `Rejected<Record>` into `None`
       --> tools/dylint/examples/discarded_error.rs:178:71
        |
    178 | fn rejected_absence(record: Record) -> Option<Record> { record.step().ok() }
        |                                                                       ^^^^
        |
        = help: return the `Result` with `?`, or convert its error into a caller variant with `map_err`

    warning: `map_or` replaces `PoisonError<MutexGuard<'_, u32>>` with a fallback value
       --> tools/dylint/examples/discarded_error.rs:180:61
        |
    180 | fn locked(shared: &Mutex<u32>) -> [u32; 2] { [shared.lock().map_or(0, |guard| *guard), shared.try_lock().map_or(0, |guard| *guard)] }
        |                                                             ^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `map_or` replaces `TryLockError<MutexGuard<'_, u32>>` with a fallback value
       --> tools/dylint/examples/discarded_error.rs:180:106
        |
    180 | fn locked(shared: &Mutex<u32>) -> [u32; 2] { [shared.lock().map_or(0, |guard| *guard), shared.try_lock().map_or(0, |guard| *guard)] }
        |                                                                                                          ^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `flatten` iterates results and skips each `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:185:43
        |
    185 |     (texts.iter().map(|text| parse(text)).flatten().collect(), parse(texts[0]).into_iter().collect(), parse(texts[0]).iter().count(...
        |                                           ^^^^^^^^^
        |
        = help: collect the results into `validated::Validated<Vec<_>, ParseIntError>` and call `.ok()`

    warning: `into_iter` iterates the `Result` and skips its `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:185:80
        |
    185 |     (texts.iter().map(|text| parse(text)).flatten().collect(), parse(texts[0]).into_iter().collect(), parse(texts[0]).iter().count(...
        |                                                                                ^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `iter` iterates the `Result` and skips its `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:185:119
        |
    185 |     (texts.iter().map(|text| parse(text)).flatten().collect(), parse(texts[0]).into_iter().collect(), parse(texts[0]).iter().count(...
        |                                                                                                                       ^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `iter_mut` iterates the `Result` and skips its `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:185:151
        |
    185 | ...collect(), parse(texts[0]).iter().count(), parse(texts[0]).iter_mut().count())
        |                                                               ^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `is_ok_and` tests the `Result` and drops `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:188:71
        |
    188 | fn tested(text: &str) -> Result<bool, ParseIntError> { Ok(parse(text).is_ok_and(|value| value > 1)) }
        |                                                                       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result` and handle `Err` in place of the test

    warning: `drop` drops a `Result` and its unhandled `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:193:5
        |
    193 |     drop(effect(text));
        |     ^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, or return it with `?`

    warning: `_ =` drops a `Result` and its unhandled `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:194:5
        |
    194 |     _ = effect(text);
        |     ^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, or return it with `?`

    warning: `let _ignored` drops a `Result` and its unhandled `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:195:5
        |
    195 |     let _ignored = effect(text);
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, or return it with `?`

    warning: `unwrap_or_default` replaces `ParseIntError` with a fallback value
       --> tools/dylint/examples/discarded_error.rs:200:22
        |
    198 | macro_rules! defaulted {
    199 |     ($text:expr) => {
    200 |         parse($text).unwrap_or_default()
        |                      ^^^^^^^^^^^^^^^^^^^
    ...
    204 | fn expanded(text: &str) -> u32 { defaulted!(text) }
        |                                  ---------------- in this macro invocation
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or` replaces `TryFromIntError` with a fallback value
       --> tools/dylint/examples/discarded_error.rs:212:61
        |
    212 | fn saturated(value: i64) -> (u8, i8) { (u8::try_from(value).unwrap_or(u8::MAX), i8::try_from(value).unwrap_or_else(|_| if value < 0...
        |                                                             ^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_else` replaces `TryFromIntError` with a fallback value
       --> tools/dylint/examples/discarded_error.rs:212:101
        |
    212 | ...or(u8::MAX), i8::try_from(value).unwrap_or_else(|_| if value < 0 { i8::MIN } else { i8::MAX })) }
        |                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or` replaces `TryFromIntError` with a fallback value
       --> tools/dylint/examples/discarded_error.rs:216:89
        |
    216 | fn opaque_saturation(read: impl FnOnce() -> Result<u8, TryFromIntError>) -> u8 { read().unwrap_or(u8::MAX) }
        |                                                                                         ^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_else` replaces `TryFromIntError` with a fallback value
       --> tools/dylint/examples/discarded_error.rs:218:64
        |
    218 | fn reversed_saturation(value: i64) -> i8 { i8::try_from(value).unwrap_or_else(|_| if value < 0 { i8::MAX } else { i8::MIN }) }
        |                                                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or` replaces `TryFromIntError` with a fallback value
       --> tools/dylint/examples/discarded_error.rs:220:52
        |
    220 | fn clamped(value: i64) -> u8 { u8::try_from(value).unwrap_or(1) }
        |                                                    ^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `map_or_default` replaces `TryFromIntError` with a fallback value
       --> tools/dylint/examples/discarded_error.rs:222:53
        |
    222 | fn narrowed(value: i64) -> u8 { u8::try_from(value).map_or_default(|_| u8::MAX) }
        |                                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `let (.. | Err(_))` drops a `Result` and its unhandled `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:230:5
        |
    230 |     let (Ok(()) | Err(_)) = effect(text);
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, or return it with `?`

    warning: `let (.. | Err(_))` drops a `Result` and its unhandled `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:231:5
        |
    231 |     let (Ok(()) | Err(ParseIntError { .. })) = effect(text);
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, or return it with `?`

    warning: `let (.. | Err(_))` drops `Vec<u32>` without naming its type
       --> tools/dylint/examples/discarded_error.rs:232:5
        |
    232 |     let (Ok(()) | Err(_)) = returned_back(vec![1]);
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Err` value by its type, `Err(<Type> { .. })`

    warning: `Err(_) =>` drops a `Result` and its unhandled `ParseIntError`
       --> tools/dylint/examples/discarded_error.rs:235:9
        |
    235 |         Ok(()) | Err(_) => (),
        |         ^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, or return it with `?`

    warning: `ok` turns `ParseIntError` into `None`
       --> tools/dylint/examples/discarded_error.rs:239:139
        |
    239 | ....ok().expect("radix 10 parses decimal digits") }, parse(text).ok().unwrap()] }
        |                                                                  ^^^^
        |
        = help: return the `Result` with `?`, or convert its error into a caller variant with `map_err`

    warning: `drop` drops a `Result` and its unhandled `TryFromIntError`
       --> tools/dylint/examples/discarded_error.rs:257:5
        |
    257 |     drop(u16::try_from(value));
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, or return it with `?`

    warning: `is_ok` tests the `Result` and drops `TryFromIntError`
       --> tools/dylint/examples/discarded_error.rs:258:27
        |
    258 |     (u16::try_from(value).is_ok(), u16::try_from(value).unwrap_or_default(), u16::try_from(value).or_else(|_| Ok(0)))
        |                           ^^^^^^^
        |
        = help: match the `Result` and handle `Err` in place of the test

    warning: `unwrap_or_default` replaces `TryFromIntError` with a fallback value
       --> tools/dylint/examples/discarded_error.rs:258:57
        |
    258 |     (u16::try_from(value).is_ok(), u16::try_from(value).unwrap_or_default(), u16::try_from(value).or_else(|_| Ok(0)))
        |                                                         ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `or_else` turns `TryFromIntError` into success
       --> tools/dylint/examples/discarded_error.rs:258:99
        |
    258 |     (u16::try_from(value).is_ok(), u16::try_from(value).unwrap_or_default(), u16::try_from(value).or_else(|_| Ok(0)))
        |                                                                                                   ^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `ok` turns `OpenError` into `None`
       --> tools/dylint/examples/discarded_error.rs:269:69
        |
    269 | fn open_error(value: Result<u32, OpenError>) -> Option<u32> { value.ok() }
        |                                                                     ^^^^
        |
        = help: return the `Result` with `?`, or convert its error into a caller variant with `map_err`

    warning: `ok` turns `DroppedError` into `None`
       --> tools/dylint/examples/discarded_error.rs:271:75
        |
    271 | fn dropped_error(value: Result<u32, DroppedError>) -> Option<u32> { value.ok() }
        |                                                                           ^^^^
        |
        = help: return the `Result` with `?`, or convert its error into a caller variant with `map_err`

    warning: `ok` turns `Reason` into `None`
       --> tools/dylint/examples/discarded_error.rs:273:71
        |
    273 | fn multiple_causes(value: Result<u32, Reason>) -> Option<u32> { value.ok() }
        |                                                                       ^^^^
        |
        = help: return the `Result` with `?`, or convert its error into a caller variant with `map_err`

    warning: `drop` drops a `Result` and its unhandled `SingleError`
       --> tools/dylint/examples/discarded_error.rs:276:5
        |
    276 |     drop(read());
        |     ^^^^^^^^^^^^
        |
        = help: match the `Result`, or return it with `?`

    warning: `is_ok` tests the `Result` and drops `SingleError`
       --> tools/dylint/examples/discarded_error.rs:277:13
        |
    277 |     (read().is_ok(), read().unwrap_or_default(), read().or(Ok(0)))
        |             ^^^^^^^
        |
        = help: match the `Result` and handle `Err` in place of the test

    warning: `unwrap_or_default` replaces `SingleError` with a fallback value
       --> tools/dylint/examples/discarded_error.rs:277:29
        |
    277 |     (read().is_ok(), read().unwrap_or_default(), read().or(Ok(0)))
        |                             ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `or` turns `SingleError` into success
       --> tools/dylint/examples/discarded_error.rs:277:57
        |
    277 |     (read().is_ok(), read().unwrap_or_default(), read().or(Ok(0)))
        |                                                         ^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `SingleError` with a fallback value
       --> tools/dylint/examples/discarded_error.rs:280:75
        |
    280 | fn optional_fallback(value: Result<u32, SingleError>) -> u32 { value.ok().unwrap_or_default() }
        |                                                                           ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `is_some` tests the `Result` and drops `SingleError`
       --> tools/dylint/examples/discarded_error.rs:282:78
        |
    282 | fn optional_view(value: Result<u32, SingleError>) -> [bool; 1] { [value.ok().is_some()] }
        |                                                                              ^^^^^^^^^
        |
        = help: match the `Result` and handle `Err` in place of the test

    warning: `or` turns `SingleError` into success
       --> tools/dylint/examples/discarded_error.rs:284:82
        |
    284 | fn optional_success(value: Result<u32, SingleError>) -> Option<u32> { value.ok().or(Some(0)) }
        |                                                                                  ^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `SingleError` with a fallback value
       --> tools/dylint/examples/discarded_error.rs:288:105
        |
    288 | fn optional_relays(value: Result<u32, SingleError>) -> u32 { value.ok().map(u32::reverse_bits).or(None).unwrap_or_default() }
        |                                                                                                         ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `or_else` turns `SingleError` into success
       --> tools/dylint/examples/discarded_error.rs:292:90
        |
    292 | fn optional_success_closure(value: Result<u32, SingleError>) -> Option<u32> { value.ok().or_else(|| Some(0)) }
        |                                                                                          ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `into_iter` iterates the `Result` and skips its `SingleError`
       --> tools/dylint/examples/discarded_error.rs:296:81
        |
    296 | fn optional_iteration(value: Result<u32, SingleError>) -> Vec<u32> { value.ok().into_iter().collect() }
        |                                                                                 ^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `or_else` turns `SingleError` into success
       --> tools/dylint/examples/discarded_error.rs:300:91
        |
    300 | fn optional_returned_success(value: Result<u32, SingleError>) -> Option<u32> { value.ok().or_else(|| return Some(0)) }
        |                                                                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `ok` turns `SingleError` into `None`
       --> tools/dylint/examples/discarded_error.rs:304:81
        |
    304 | fn optional_untyped_translation(value: Result<u32, SingleError>) -> u32 { value.ok().ok_or(()).unwrap_or_default() }
        |                                                                                 ^^^^
        |
        = help: return the `Result` with `?`, or convert its error into a caller variant with `map_err`

    warning: `xor` turns `SingleError` into success
       --> tools/dylint/examples/discarded_error.rs:306:92
        |
    306 | fn optional_exclusive_success(value: Result<u32, SingleError>) -> Option<u32> { value.ok().xor(Some(0)) }
        |                                                                                            ^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `reduce` turns `SingleError` into success
       --> tools/dylint/examples/discarded_error.rs:310:90
        |
    310 | fn optional_reduced_success(value: Result<u32, SingleError>) -> Option<u32> { value.ok().reduce(Some(0), u32::strict_add) }
        |                                                                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `get_or_try_insert_with` turns `SingleError` into success
       --> tools/dylint/examples/discarded_error.rs:314:93
        |
    314 | fn optional_try_insert_success(value: Result<u32, SingleError>) -> Option<u32> { value.ok().get_or_try_insert_with(|| Some(0)).copi...
        |                                                                                             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `get_or_try_insert_with` turns `SingleError` into success
       --> tools/dylint/examples/discarded_error.rs:323:10
        |
    323 |           .get_or_try_insert_with(|| {
        |  __________^
    324 | |             Err({
    325 | |                 if choose {
    326 | |                     return Ok(0);
    ...   |
    329 | |             })
    330 | |         })
        | |__________^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `get_or_try_insert_with` turns `SingleError` into success
       --> tools/dylint/examples/discarded_error.rs:337:10
        |
    337 |           .get_or_try_insert_with(|| 'reply: {
        |  __________^
    338 | |             Err({
    339 | |                 if choose {
    340 | |                     break 'reply Ok(0);
    ...   |
    343 | |             })
    344 | |         })
        | |__________^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `ok` turns `SendError<String>` into `None`
       --> tools/dylint/examples/discarded_error.rs:364:106
        |
    364 | fn publication_absence(sender: &broadcast::Sender<String>) -> Option<usize> { sender.send(String::new()).ok() }
        |                                                                                                          ^^^^
        |
        = help: return the `Result` with `?`, or convert its error into a caller variant with `map_err`

    warning: `unwrap_or_default` replaces `SendError<String>` with a fallback value
       --> tools/dylint/examples/discarded_error.rs:366:99
        |
    366 | fn publication_fallback(sender: &broadcast::Sender<String>) -> usize { sender.send(String::new()).unwrap_or_default() }
        |                                                                                                   ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `is_ok` tests the `Result` and drops `SendError<String>`
       --> tools/dylint/examples/discarded_error.rs:368:100
        |
    368 | fn publication_view(sender: &broadcast::Sender<String>) -> [bool; 1] { [sender.send(String::new()).is_ok()] }
        |                                                                                                    ^^^^^^^
        |
        = help: match the `Result` and handle `Err` in place of the test

    warning: `drop` drops a `Result` and its unhandled `SendError<String>`
       --> tools/dylint/examples/discarded_error.rs:370:85
        |
    370 | fn opaque_publication(result: Result<usize, broadcast::error::SendError<String>>) { drop(result); }
        |                                                                                     ^^^^^^^^^^^^
        |
        = help: match the `Result`, or return it with `?`

    warning: `drop` drops a `Result` and its unhandled `SendError<String>`
       --> tools/dylint/examples/discarded_error.rs:373:5
        |
    373 |     drop('publish: { sender.send(if choose { break 'publish incoming } else { String::new() }) });
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, or return it with `?`

    warning: `drop` drops a `Result` and its unhandled `SendError<String>`
       --> tools/dylint/examples/discarded_error.rs:379:5
        |
    379 |     drop(sender.send(String::new()));
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, or return it with `?`

    warning: `drop` drops a `Result` and its unhandled `String`
       --> tools/dylint/examples/discarded_error.rs:382:5
        |
    382 |     drop(sender.send(String::new()));
        |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, or return it with `?`

    warning: `or_else` turns `ParseIntError` into success
      --> tools/dylint/examples/discarded_error_source.rs:55:64
       |
    55 | fn recovered(text: &str) -> Result<u16, Failure> { parse(text).or_else(|_| Ok(0)) }
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
       --> tools/dylint/examples/duplicate_type.rs:416:52
        |
    416 |     shim.port.strict_add(u16::try_from(shim.limit).unwrap_or_default())
        |                                                    ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`
        = note: `#[warn(discarded_error)]` on by default

    warning: `unwrap_or_default` replaces `PoisonError<MutexGuard<'_, Vec<u8>>>` with a fallback value
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:62:50
       |
    62 |     SHARED.lock().map(|mut bytes| bytes.push(1)).unwrap_or_default();
       |                                                  ^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`
       = note: `#[warn(discarded_error)]` on by default

    warning: `unwrap_or_default` replaces `PoisonError<MutexGuard<'_, Vec<u8>>>` with a fallback value
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:62:50
       |
    62 |     SHARED.lock().map(|mut bytes| bytes.push(1)).unwrap_or_default();
       |                                                  ^^^^^^^^^^^^^^^^^^^
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

    warning: `unwrap_or_default` replaces `Error` with a fallback value
      --> tools/dylint/examples/manual_serialization.rs:34:121
       |
    34 | ...e_json::to_string(port).map(|_| String::from("other")).unwrap_or_default()) }
       |                                                           ^^^^^^^^^^^^^^^^^^^
       |
       = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `map_or` replaces `Error` with a fallback value
       --> tools/dylint/examples/needless_import_rename.rs:137:42
        |
    137 |     let rendered = rendered(&Map::new()).map_or(0, |text| text.len());
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
       --> tools/dylint/examples/redundant_error_conversion.rs:215:26
        |
    215 |         wrapped(b"8083").map_or(Ok(0), parse)?;
        |                          ^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`
        = note: `#[warn(discarded_error)]` on by default

    warning: `map_or` replaces `OuterError` with a fallback value
       --> tools/dylint/examples/redundant_error_conversion.rs:216:26
        |
    216 |         located(b"8084").map_or(Ok(0), inferred)?;
        |                          ^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `map_or` replaces `AttributedError` with a fallback value
       --> tools/dylint/examples/redundant_error_conversion.rs:228:29
        |
    228 |         attributed(b"8093").map_or(Ok(0), parse)?;
        |                             ^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `PortError` with a fallback value
       --> tools/dylint/examples/redundant_error_conversion.rs:231:74
        |
    231 |     black_box((run(), read(Path::new("port")).map(|text| text.len()))).0.unwrap_or_default();
        |                                                                          ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `ParseIntError` with a fallback value
       --> tools/dylint/examples/stringified_error.rs:343:44
        |
    343 |     let mut attempt = counted(parsed(text).unwrap_or_default());
        |                                            ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`
        = note: `#[warn(discarded_error)]` on by default

    warning: `unwrap_or_default` replaces `Error` with a fallback value
       --> tools/dylint/examples/stringly_typed_value.rs:159:34
        |
    159 |     input.read_line(&mut answer).unwrap_or_default();
        |                                  ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`
        = note: `#[warn(discarded_error)]` on by default

    warning: `ok` turns `PortError` into `None`
       --> tools/dylint/examples/untyped_error.rs:241:76
        |
    241 |     if failures.is_empty() { Ok(texts.iter().filter_map(|text| typed(text).ok()).collect()) } else { Err(failures) }
        |                                                                            ^^^^
        |
        = help: return the `Result` with `?`, or convert its error into a caller variant with `map_err`
        = note: `#[warn(discarded_error)]` on by default

    warning: `ok` turns `PortError` into `None`
       --> tools/dylint/examples/untyped_error.rs:246:100
        |
    246 | fn surveyed(texts: &[&str]) -> Survey { Survey { ports: texts.iter().filter_map(|text| typed(text).ok()).collect(), failures: audit...
        |                                                                                                    ^^^^
        |
        = help: return the `Result` with `?`, or convert its error into a caller variant with `map_err`

    warning: `map_or` replaces `&str` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:272:13
        |
    272 |     present.map_or(Ok(0), |digits| spelled(digits).or_else(|_text| typed(digits)))
        |             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `Result::ok` turns `Error` into `None`
       --> tools/dylint/examples/untyped_error.rs:295:154
        |
    295 | ....map(|report| report.downcast::<ParseIntError>()).find_map(Result::ok) }
        |                                                               ^^^^^^^^^^
        |
        = help: return the `Result` with `?`, or convert its error into a caller variant with `map_err`

    warning: `Err(_) =>` replaces `Error` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:361:9
        |
    361 |         Err(anyhow::Error { .. }) => None,
        |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `Code` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:473:21
        |
    473 |         coded(text).unwrap_or_default(),
        |                     ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `Code` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:474:38
        |
    474 |         retried(text, Code::Invalid).unwrap_or_default(),
        |                                      ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `Relocated` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:475:25
        |
    475 |         relocated(text).unwrap_or_default(),
        |                         ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `Box<str>` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:476:26
        |
    476 |         boxed_text(text).unwrap_or_default(),
        |                          ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `Arc<str>` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:477:27
        |
    477 |         shared_text(text).unwrap_or_default(),
        |                           ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `Rc<str>` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:478:28
        |
    478 |         counted_text(text).unwrap_or_default(),
        |                            ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `Arc<dyn Error + Send + Sync>` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:480:28
        |
    480 |         shared_error(text).unwrap_or_default(),
        |                            ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `Rc<dyn Error>` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:481:29
        |
    481 |         counted_error(text).unwrap_or_default(),
        |                             ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `map_or` replaces `Vec<PortError>` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:499:27
        |
    499 |         [planned(&[text]).map_or(0, |found| found.len()), audited(&[text]).len(), survey.ports.len(), survey.failures.len()],
        |                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `RenderError` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:515:46
        |
    515 |         answered(oneshot::channel().1).await.unwrap_or_default(),
        |                                              ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `RenderError` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:516:30
        |
    516 |         infallible(Ok(8080)).unwrap_or_default(),
        |                              ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `ErrorData` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:517:26
        |
    517 |         served(text, 80).unwrap_or_default(),
        |                          ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `ErrorData` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:518:22
        |
    518 |         mapped(text).unwrap_or_default(),
        |                      ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `ErrorData` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:519:40
        |
    519 |         dispatched(text, "ports/read").unwrap_or_default(),
        |                                        ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`

    warning: `unwrap_or_default` replaces `ErrorData` with a fallback value
       --> tools/dylint/examples/untyped_error.rs:520:31
        |
    520 |         extracted(Some(text)).unwrap_or_default(),
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

    warning: `Err(_) =>` replaces `Signal` with a fallback value
       --> tools/dylint/examples/wildcard_workspace_enum_match_arm.rs:162:9
        |
    162 |         Ok(_) | Err(_) => 1,
        |         ^^^^^^^^^^^^^^^^^^^
        |
        = help: match the `Result`, handle each error the caller acts on, and return the rest with `?`
        = note: `#[warn(discarded_error)]` on by default
    "#);
}

fn discarded_error_source(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: `map_err` replaces `ParseIntError` without reading it
      --> tools/dylint/examples/discarded_error_source.rs:33:60
       |
    33 | fn named(text: &str) -> Result<u16, Failure> { parse(text).map_err(|_error| Failure::Missing) }
       |                                                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: hold `ParseIntError` as `#[source]` of the error the closure builds
       = note: `#[warn(discarded_error_source)]` on by default

    warning: `map_err` replaces `ParseIntError` without reading it
      --> tools/dylint/examples/discarded_error_source.rs:35:62
       |
    35 | fn ignored(text: &str) -> Result<u16, Failure> { parse(text).map_err(|_| Failure::Missing) }
       |                                                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `or_else` replaces `ParseIntError` without reading it
      --> tools/dylint/examples/discarded_error_source.rs:37:62
       |
    37 | fn retried(text: &str) -> Result<u16, Failure> { parse(text).or_else(|_| Err(Failure::Missing)) }
       |                                                              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `or_else` replaces `ParseIntError` without reading it
      --> tools/dylint/examples/discarded_error_source.rs:39:68
       |
    39 | fn retried_named(text: &str) -> Result<u16, Failure> { parse(text).or_else(|_ignored| Err(Failure::Missing)) }
       |                                                                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `Error` without reading it
      --> tools/dylint/examples/discarded_error_source.rs:41:104
       |
    41 | ...e::<u16>().map_err(anyhow::Error::from).map_err(|anyhow::Error { .. }| Failure::Missing) }
       |                                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: hold `Error` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `String` without reading it
      --> tools/dylint/examples/discarded_error_source.rs:49:76
       |
    49 | fn text_dropped(name: &str) -> Result<ColorChoice, Failure> { name.parse().map_err(|_: String| Failure::Missing) }
       |                                                                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: hold the text or the subject it names as a field the message prints

    warning: `map_err` replaces `TryFromIntError` without reading it
      --> tools/dylint/examples/discarded_error_source.rs:81:10
       |
    81 |         .map_err(|_| Failure::Missing)
       |          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: hold `TryFromIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `TryFromIntError` without reading it
      --> tools/dylint/examples/discarded_error_source.rs:92:10
       |
    92 |         .map_err(|_| Failure::Missing)
       |          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: hold `TryFromIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `TryFromIntError` without reading it
       --> tools/dylint/examples/discarded_error_source.rs:105:10
        |
    105 |         .map_err(|_| Failure::Missing)
        |          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `TryFromIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `TryFromIntError` without reading it
       --> tools/dylint/examples/discarded_error_source.rs:108:73
        |
    108 | fn two_sided(value: i32) -> Result<u16, Failure> { u16::try_from(value).map_err(|_| Failure::Missing) }
        |                                                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `TryFromIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `TryFromIntError` without reading it
       --> tools/dylint/examples/discarded_error_source.rs:110:93
        |
    110 | fn nonzero(value: NonZero<i32>) -> Result<NonZero<u16>, Failure> { NonZero::try_from(value).map_err(|_| Failure::Missing) }
        |                                                                                             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `TryFromIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `TryFromIntError` without reading it
       --> tools/dylint/examples/discarded_error_source.rs:112:97
        |
    112 | fn opaque(read: impl FnOnce() -> Result<u16, TryFromIntError>) -> Result<u16, Failure> { read().map_err(|_| Failure::Missing) }
        |                                                                                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `TryFromIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `Reserved` without reading it
       --> tools/dylint/examples/discarded_error_source.rs:114:75
        |
    114 | fn reserved(value: Result<u16, Reserved>) -> Result<u16, Failure> { value.map_err(|_| Failure::Missing) }
        |                                                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `Reserved` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `LayoutError` without reading it
       --> tools/dylint/examples/discarded_error_source.rs:118:112
        |
    118 | ...> { Layout::from_size_align(size, align).map_err(|LayoutError { .. }| Failure::Missing) }
        |                                             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `LayoutError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `LayoutError` without reading it
       --> tools/dylint/examples/discarded_error_source.rs:120:106
        |
    120 | ...or>) -> Result<Layout, Failure> { read().map_err(|LayoutError { .. }| Failure::Missing) }
        |                                             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `LayoutError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `OpenError` without reading it
       --> tools/dylint/examples/discarded_error_source.rs:126:35
        |
    126 |         Err(OpenError::default()).map_err(|_| Failure::Missing),
        |                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `OpenError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `DroppedError` without reading it
       --> tools/dylint/examples/discarded_error_source.rs:127:27
        |
    127 |         Err(DroppedError).map_err(|_| Failure::Missing),
        |                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `DroppedError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
      --> tools/dylint/examples/untyped_error.rs:55:85
       |
    55 | ...ror> { value.parse().map(Self).map_err(|_error: ParseIntError| String::from("text is not a port")) }
       |                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: hold `ParseIntError` as `#[source]` of the error the closure builds
       = note: `#[warn(discarded_error_source)]` on by default

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:202:61
        |
    202 | pub fn probed(text: &str) -> Result<u16, ()> { text.parse().map_err(|_error: ParseIntError| ()) }
        |                                                             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:204:69
        |
    204 | fn borrowed(text: &str) -> Result<u16, &'static str> { text.parse().map_err(|_error: ParseIntError| "port is not a number") }
        |                                                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:210:64
        |
    210 | fn generic_default(text: &str) -> Fallible<u16> { text.parse().map_err(|_error: ParseIntError| String::from("port is not a number")) }
        |                                                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:217:58
        |
    217 | fn coded(text: &str) -> Result<u16, Code> { text.parse().map_err(|_error: ParseIntError| Code::Invalid) }
        |                                                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:219:72
        |
    219 | fn retried(text: &str, code: Code) -> Result<u16, Code> { text.parse().map_err(|_error: ParseIntError| code) }
        |                                                                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:221:67
        |
    221 | fn relocated(text: &str) -> Result<u16, Relocated> { text.parse().map_err(|_error: ParseIntError| Relocated::Moved) }
        |                                                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:223:67
        |
    223 | ...6, Box<str>> { text.parse().map_err(|_error: ParseIntError| Box::from("boxed port is not a number")) }
        |                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:225:68
        |
    225 | ..., Arc<str>> { text.parse().map_err(|_error: ParseIntError| Arc::from("shared port is not a number")) }
        |                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:227:68
        |
    227 | ...6, Rc<str>> { text.parse().map_err(|_error: ParseIntError| Rc::from("counted port is not a number")) }
        |                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:229:79
        |
    229 | ...static, str>> { text.parse().map_err(|_error: ParseIntError| Cow::Borrowed("port is not a number")) }
        |                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:235:65
        |
    235 | fn returned(text: String) -> Result<u16, String> { text.parse().map_err(|_error: ParseIntError| text) }
        |                                                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:237:73
        |
    237 | fn routed(text: &str) -> Result<Reply, Reply> { text.parse().map(Reply).map_err(|_error: ParseIntError| Reply(400)) }
        |                                                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:270:128
        |
    270 | ...else { digits.parse::<u16>().map_err(|_error: ParseIntError| String::from("port is not a number")) };
        |                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `ParseIntError` as `#[source]` of the error the closure builds

    warning: `map_err` replaces `ParseIntError` without reading it
       --> tools/dylint/examples/untyped_error.rs:342:60
        |
    342 | fn numbered(text: &str) -> Result<u16, i32> { text.parse().map_err(|_error: ParseIntError| -1) }
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
    warning: tracing field records error `Unread` outside `Report`
      --> tools/dylint/examples/error_field_without_sources.rs:22:124
       |
    22 | fn displayed(text: &str) -> Result<u16, Unread> { parsed(text).inspect_err(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %error, "p...
       |                                                                                                                            ^^^^^
       |
       = help: record `std::error::Report::new(error)` with `%` or `field::display` under `#![feature(error_reporter)]`
       = note: `#[warn(error_field_without_sources)]` on by default

    warning: tracing field records error `Unread` outside `Report`
      --> tools/dylint/examples/error_field_without_sources.rs:24:123
       |
    24 | fn debugged(text: &str) -> Result<u16, Unread> { parsed(text).inspect_err(|error| tracing::warn!({ EXCEPTION_MESSAGE } = ?error, "po...
       |                                                                                                                           ^^^^^
       |
       = help: record `std::error::Report::new(error)` with `%` or `field::display` under `#![feature(error_reporter)]`

    warning: tracing field records error `dyn std::error::Error` outside `Report`
      --> tools/dylint/examples/error_field_without_sources.rs:26:120
       |
    26 | ...r(|error| tracing::warn!({ EXCEPTION_MESSAGE } = error as &(dyn Error + 'static), "port unread")) }
       |                                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: record `std::error::Report::new(error)` with `%` or `field::display` under `#![feature(error_reporter)]`

    warning: tracing field records error `std::num::ParseIntError` outside `Report`
      --> tools/dylint/examples/error_field_without_sources.rs:28:122
       |
    28 | ...spect_err(|error| tracing::warn!({ EXCEPTION_MESSAGE } = %error.source, "port unread")) }
       |                                                              ^^^^^^^^^^^^
       |
       = help: record `std::error::Report::new(error)` with `%` or `field::display` under `#![feature(error_reporter)]`

    warning: tracing field records error `Unread` outside `Report`
      --> tools/dylint/examples/error_field_without_sources.rs:30:140
       |
    30 | ...or| drop(tracing::info_span!("port", { EXCEPTION_MESSAGE } = %error))) }
       |                                                                  ^^^^^
       |
       = help: record `std::error::Report::new(error)` with `%` or `field::display` under `#![feature(error_reporter)]`

    warning: tracing field records error `Unread` outside `Report`
      --> tools/dylint/examples/error_field_without_sources.rs:34:102
       |
    34 |         tracing::info_span!("port", { EXCEPTION_MESSAGE } = Empty).record(EXCEPTION_MESSAGE, display(error));
       |                                                                                                      ^^^^^
       |
       = help: record `std::error::Report::new(error)` with `%` or `field::display` under `#![feature(error_reporter)]`
    "#);
}




fn manual_error_impl(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `Manual` implements `core::error::Error` by hand
      --> tools/dylint/examples/manual_error_impl.rs:32:1
       |
    32 | impl Error for Manual {}
       | ^^^^^^^^^^^^^^^^^^^^^
       |
       = help: derive `derive_more::Error`
       = note: `#[warn(manual_error_impl)]` on by default

    warning: `Sourced<E>` implements `core::error::Error` by hand
      --> tools/dylint/examples/manual_error_impl.rs:38:1
       |
    38 | impl<E: Error + 'static> Error for Sourced<E> {
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: derive `derive_more::Error` with `#[error(ignore)]` and mark field `0` `#[error(source, not(backtrace))]`

    warning: `Hidden` implements `core::error::Error` by hand
      --> tools/dylint/examples/manual_error_impl.rs:81:1
       |
    81 | impl Error for Hidden {}
       | ^^^^^^^^^^^^^^^^^^^^^
       |
       = help: derive `derive_more::Error` with `#[error(ignore)]`

    warning: `ManualError` implements `core::error::Error` by hand
       --> tools/dylint/examples/redundant_error_conversion.rs:116:1
        |
    116 | impl error::Error for ManualError {
        | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: derive `derive_more::Error` with `#[error(ignore)]` and mark field `0` `#[error(source, not(backtrace))]`
        = note: `#[warn(manual_error_impl)]` on by default
    ");
}

fn partition_by_result(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `partition` leaves each half of the results wrapped in `Ok` or `Err`
      --> tools/dylint/examples/partition_by_result.rs:29:84
       |
    29 | fn named(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(passed) }
       |                                                                                    ^^^^^^^^^^^^^^^^^
       |
       = help: call `Itertools::partition_result` and delete the unwraps after it
       = note: `#[warn(partition_by_result)]` on by default

    warning: `partition` leaves each half of the results wrapped in `Ok` or `Err`
      --> tools/dylint/examples/partition_by_result.rs:31:85
       |
    31 | fn pathed(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(Result::is_err) }
       |                                                                                     ^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call `Itertools::partition_result`, swap the halves, and delete the unwraps after it

    warning: `partition` leaves each half of the results wrapped in `Ok` or `Err`
      --> tools/dylint/examples/partition_by_result.rs:33:85
       |
    33 | fn closed(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(|result| result.is_ok()) }
       |                                                                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call `Itertools::partition_result` and delete the unwraps after it

    warning: `partition` leaves each half of the results wrapped in `Ok` or `Err`
      --> tools/dylint/examples/partition_by_result.rs:35:86
       |
    35 | fn negated(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(|result| !result.as_ref().is_err()) }
       |                                                                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call `Itertools::partition_result` and delete the unwraps after it

    warning: `partition` leaves each half of the results wrapped in `Ok` or `Err`
      --> tools/dylint/examples/partition_by_result.rs:37:85
       |
    37 | fn called(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(|result| Result::is_ok(result)) }
       |                                                                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call `Itertools::partition_result` and delete the unwraps after it

    warning: `partition` leaves each half of the results wrapped in `Ok` or `Err`
      --> tools/dylint/examples/partition_by_result.rs:39:68
       |
    39 | fn qualified(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { Iterator::partition(results.into_iter(), Result::is_ok) }
       |                                                                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call `Itertools::partition_result` and delete the unwraps after it

    warning: `partition` leaves each half of the results wrapped in `Ok` or `Err`
      --> tools/dylint/examples/partition_by_result.rs:41:82
       |
    41 | fn borrowed(results: &[Parsed]) -> (Vec<&Parsed>, Vec<&Parsed>) { results.iter().partition(|result| result.is_ok()) }
       |                                                                                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call `Itertools::partition_result` after mapping each borrowed result with `Result::as_ref` and delete the unwraps after it

    warning: `partition` leaves each half of the results wrapped in `Ok` or `Err`
      --> tools/dylint/examples/partition_by_result.rs:45:86
       |
    45 | fn matched(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(|result| matches!(result, Ok(_))) }
       |                                                                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call `Itertools::partition_result` and delete the unwraps after it

    warning: `partition` leaves each half of the results wrapped in `Ok` or `Err`
      --> tools/dylint/examples/partition_by_result.rs:47:91
       |
    47 | fn dereferenced(results: Vec<Parsed>) -> (Vec<Parsed>, Vec<Parsed>) { results.into_iter().partition(|result| (*result).is_err()) }
       |                                                                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call `Itertools::partition_result`, swap the halves, and delete the unwraps after it

    warning: `partition` leaves each half of the results wrapped in `Ok` or `Err`
      --> tools/dylint/examples/partition_by_result.rs:49:148
       |
    49 | ...t<u8, TryFromIntError>>) { results.into_iter().partition(|&result| result.is_ok()) }
       |                                                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: call `Itertools::partition_result` and delete the unwraps after it

    warning: `partition` leaves each half of the results wrapped in `Ok` or `Err`
      --> tools/dylint/examples/partition_by_result.rs:52:25
       |
    52 |       results.into_iter().partition(|result| match result {
       |  _________________________^
    53 | |         Ok(_) => true,
    54 | |         Err(_) => false,
    55 | |     })
       | |______^
       |
       = help: call `Itertools::partition_result` and delete the unwraps after it
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

    warning: `impl From<Error> for ReadError` wraps the source in `Io` by hand
      --> tools/dylint/examples/redundant_error_conversion.rs:94:1
       |
    94 | impl From<io::Error> for ReadError {
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
    note: source field
      --> tools/dylint/examples/redundant_error_conversion.rs:91:18
       |
    91 |     Io(#[source] io::Error),
       |                  ^^^^^^^^^
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
        |                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: delete the `map_err` call
    applicability MachineApplicable: delete the `map_err` call

    warning: `map_err` repeats the `From` conversion `?` applies
       --> tools/dylint/examples/redundant_error_conversion.rs:133:37
        |
    133 |     let text = str::from_utf8(bytes).map_err(PortError::Text)?;
        |                                     ^^^^^^^^^^^^^^^^^^^^^^^^^ help: delete the `map_err` call
    applicability MachineApplicable: delete the `map_err` call

    warning: `map_err` repeats the `From` conversion `?` applies
       --> tools/dylint/examples/redundant_error_conversion.rs:143:37
        |
    143 |     let text = str::from_utf8(bytes).map_err(LocatedError)?;
        |                                     ^^^^^^^^^^^^^^^^^^^^^^ help: delete the `map_err` call
    applicability MachineApplicable: delete the `map_err` call

    warning: `map_err` repeats the `From` conversion `?` applies
       --> tools/dylint/examples/redundant_error_conversion.rs:153:28
        |
    153 |     let text = text!(bytes).map_err(PortError::from)?;
        |                            ^^^^^^^^^^^^^^^^^^^^^^^^^ help: delete the `map_err` call
    applicability MachineApplicable: delete the `map_err` call

    warning: `map_err` repeats the `From` conversion `?` applies
       --> tools/dylint/examples/redundant_error_conversion.rs:159:37
        |
    159 |     let text = str::from_utf8(bytes).map_err(E::from)?;
        |                                     ^^^^^^^^^^^^^^^^^ help: delete the `map_err` call
    applicability MachineApplicable: delete the `map_err` call

    warning: `map_err` repeats the `From` conversion `?` applies
       --> tools/dylint/examples/redundant_error_conversion.rs:189:37
        |
    189 |     let text = str::from_utf8(bytes).map_err(|error| PortError::Text(error))?;
        |                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ help: delete the `map_err` call
    applicability MachineApplicable: delete the `map_err` call
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
      = help: delete field `source` from the message
      = note: `#[warn(source_in_error_message)]` on by default

    warning: `Parse` message prints its source field `0`
      --> tools/dylint/examples/source_in_error_message.rs:10:5
       |
    10 |     #[error("cannot parse the port: {0}")]
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete field `0` from the message

    warning: `Wrapped` message prints its source field `0`
      --> tools/dylint/examples/source_in_error_message.rs:12:5
       |
    12 |     #[error("{0}")]
       |     ^^^^^^^^^^^^^^^
       |
       = help: state the failure in the message in place of field `0`

    warning: `Write` message prints its source field `cause`
      --> tools/dylint/examples/source_in_error_message.rs:20:5
       |
    20 |     #[error("cannot write {cause:?}")]
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete field `cause` from the message

    warning: `Sync` message prints its source field `source`
      --> tools/dylint/examples/source_in_error_message.rs:25:5
       |
    25 |     #[error("cannot sync {}: {}", .path, .source)]
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete field `source` from the message

    warning: `Flush` message prints its source field `0`
      --> tools/dylint/examples/source_in_error_message.rs:27:5
       |
    27 |     #[error("cannot flush {reason}", reason = .0)]
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete field `0` from the message

    warning: `Receive` message prints its source field `source`
      --> tools/dylint/examples/source_in_error_message.rs:33:5
       |
    33 |     #[error(r"cannot receive: {source}")]
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete field `source` from the message

    warning: `Accept` message prints its source field `source`
      --> tools/dylint/examples/source_in_error_message.rs:35:5
       |
    35 |     #[error("cannot accept: \u{7b}source\u{7d}")]
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete field `source` from the message

    warning: `LoadError` message prints its source field `source`
      --> tools/dylint/examples/source_in_error_message.rs:40:1
       |
    40 | #[error("cannot load the settings: {source}")]
       | ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: delete field `source` from the message
    "#);
}

fn stringified_error(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: text of error `std::fmt::Error` is compared
      --> tools/dylint/../../tests/rust/tools/dylint/expansions.rs:83:12
       |
    83 |         if error.to_string().contains("argument") {
       |            ^^^^^^^^^^^^^^^^^
       |
       = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text
       = note: `#[warn(stringified_error)]` on by default

    warning: text of error `std::fmt::Error` is compared
      --> tools/dylint/../../tests/rust/tools/dylint/expansions.rs:86:25
       |
    86 |         black_box(match error.to_string().contains("formatting") {
       |                         ^^^^^^^^^^^^^^^^^
       |
       = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

    warning: text of error `std::fmt::Error` is compared
      --> tools/dylint/../../tests/rust/tools/dylint/expansions.rs:90:20
       |
    90 |         assert!(if error.to_string().contains("argument") { true } else { false });
       |                    ^^^^^^^^^^^^^^^^^
       |
       = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

    warning: text of error `std::fmt::Error` is compared
      --> tools/dylint/../../tests/rust/tools/dylint/expansions.rs:91:23
       |
    91 |         let message = error.to_string();
       |                       ^^^^^^^^^^^^^^^^^
       |
       = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

    warning: text of error `std::fmt::Error` is compared
       --> tools/dylint/../../tests/rust/tools/dylint/expansions.rs:106:17
        |
    106 |         assert!(error.to_string().contains("formatting"));
        |                 ^^^^^^^^^^^^^^^^^
        |
        = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

    warning: text of `error_assertions::Mode` is compared
       --> tools/dylint/../../tests/rust/tools/dylint/expansions.rs:135:20
        |
    135 |                 if mode.to_string().contains("Ready") {
        |                    ^^^^^^^^^^^^^^^^
        |
        = help: compare or match `error_assertions::Mode` in place of its text

    warning: text of error `std::fmt::Error` is compared
       --> tools/dylint/../../tests/rust/tools/dylint/expansions.rs:139:27
        |
    139 |                 black_box(format!("{error:?}").contains("Error"));
        |                           ^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

    warning: text of error `std::num::ParseIntError` is stored
      --> tools/dylint/examples/option_fields_as_result.rs:90:59
       |
    90 |         Err(error) => Exchange { value: None, error: Some(error.to_string()) },
       |                                                           ^^^^^^^^^^^^^^^^^
       |
       = help: hold `std::num::ParseIntError` as `#[source]` of a `thiserror` variant
       = note: `#[warn(stringified_error)]` on by default

    warning: text of error `std::num::ParseIntError` is stored
      --> tools/dylint/examples/stringified_error.rs:64:96
       |
    64 | fn field(text: &str) -> Result<u16, Failure> { parsed(text).map_err(|error| Failure { message: error.to_string() }) }
       |                                                                                                ^^^^^^^^^^^^^^^^^
       |
       = help: hold `std::num::ParseIntError` as `#[source]` of a `thiserror` variant
       = note: `#[warn(stringified_error)]` on by default

    warning: text of error `std::num::ParseIntError` is stored
      --> tools/dylint/examples/stringified_error.rs:66:87
       |
    66 | fn wrapped(text: &str) -> Result<u16, Wrapped> { parsed(text).map_err(|error| Wrapped(format!("port: {error}"))) }
       |                                                                                       ^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: hold `std::num::ParseIntError` as `#[source]` of a `thiserror` variant

    warning: text of error `std::num::ParseIntError` is stored
      --> tools/dylint/examples/stringified_error.rs:68:102
       |
    68 | fn converted(text: &str) -> Result<u16, Wrapped> { parsed(text).map_err(|error| Wrapped(String::from(error.to_string().as_str()))) }
       |                                                                                                      ^^^^^^^^^^^^^^^^^
       |
       = help: hold `std::num::ParseIntError` as `#[source]` of a `thiserror` variant

    warning: text of error `std::num::ParseIntError` is stored
      --> tools/dylint/examples/stringified_error.rs:70:59
       |
    70 | fn rerendered(error: &ParseIntError) -> Wrapped { Wrapped(error.to_string().to_string()) }
       |                                                           ^^^^^^^^^^^^^^^^^
       |
       = help: hold `std::num::ParseIntError` as `#[source]` of a `thiserror` variant

    warning: text of error `std::num::ParseIntError` is stored
      --> tools/dylint/examples/stringified_error.rs:72:60
       |
    72 | fn transferred(error: &ParseIntError) -> Wrapped { Wrapped(error.to_string().into()) }
       |                                                            ^^^^^^^^^^^^^^^^^
       |
       = help: hold `std::num::ParseIntError` as `#[source]` of a `thiserror` variant

    warning: text of error `std::num::ParseIntError` is stored
      --> tools/dylint/examples/stringified_error.rs:74:97
       |
    74 | fn constructed(text: &str) -> Result<u16, Failure> { parsed(text).map_err(|error| Failure::new(&error.to_string())) }
       |                                                                                                 ^^^^^^^^^^^^^^^^^
       |
       = help: hold `std::num::ParseIntError` as `#[source]` of a `thiserror` variant

    warning: text of error `std::num::ParseIntError` is stored
      --> tools/dylint/examples/stringified_error.rs:83:25
       |
    83 |         attempt.label = error.to_string();
       |                         ^^^^^^^^^^^^^^^^^
       |
       = help: hold `std::num::ParseIntError` as `#[source]` of a `thiserror` variant

    warning: text of error `std::num::ParseIntError` is stored
      --> tools/dylint/examples/stringified_error.rs:90:23
       |
    90 |         let message = format!("{error:?}");
       |                       ^^^^^^^^^^^^^^^^^^^^
       |
       = help: hold `std::num::ParseIntError` as `#[source]` of a `thiserror` variant

    warning: text of error `std::num::ParseIntError` is stored
      --> tools/dylint/examples/stringified_error.rs:95:78
       |
    95 | fn listed(error: &ParseIntError, messages: &mut Vec<String>) { messages.push(error.to_string()); }
       |                                                                              ^^^^^^^^^^^^^^^^^
       |
       = help: hold `std::num::ParseIntError` as `#[source]` of a `thiserror` variant

    warning: text of error `std::num::ParseIntError` is returned
       --> tools/dylint/examples/stringified_error.rs:101:129
        |
    101 | ...::ExactlyOneError<Box<dyn Iterator<Item = u16>>>> { Ok(error.to_string()) }
        |                                                           ^^^^^^^^^^^^^^^^^
        |
        = help: hold `std::num::ParseIntError` as `#[source]` of a `thiserror` variant

    warning: text of error `std::num::ParseIntError` is returned
       --> tools/dylint/examples/stringified_error.rs:115:75
        |
    115 | fn collected(errors: &[ParseIntError]) -> Vec<String> { errors.iter().map(ToString::to_string).collect() }
        |                                                                           ^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `std::num::ParseIntError` as `#[source]` of a `thiserror` variant

    warning: text of error `std::num::ParseIntError` is returned
       --> tools/dylint/examples/stringified_error.rs:117:49
        |
    117 | fn described(error: &ParseIntError) -> String { error.to_string() }
        |                                                 ^^^^^^^^^^^^^^^^^
        |
        = help: hold `std::num::ParseIntError` as `#[source]` of a `thiserror` variant

    warning: text of error `std::num::ParseIntError` is returned
       --> tools/dylint/examples/stringified_error.rs:122:19
        |
    122 |         message = error.to_string();
        |                   ^^^^^^^^^^^^^^^^^
        |
        = help: hold `std::num::ParseIntError` as `#[source]` of a `thiserror` variant

    warning: text of error `std::num::ParseIntError` is compared
       --> tools/dylint/examples/stringified_error.rs:127:43
        |
    127 | fn equal(error: &ParseIntError) -> bool { error.to_string() == "invalid digit found in string" }
        |                                           ^^^^^^^^^^^^^^^^^
        |
        = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

    warning: text of error `std::io::Error` is compared
       --> tools/dylint/examples/stringified_error.rs:129:43
        |
    129 | fn contained(error: &io::Error) -> bool { ToString::to_string(error).contains("denied") }
        |                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

    warning: text of error `std::num::ParseIntError` is compared
       --> tools/dylint/examples/stringified_error.rs:132:11
        |
    132 |     match format!("{error}") {
        |           ^^^^^^^^^^^^^^^^^^
        |
        = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

    warning: text of error `std::num::ParseIntError` is compared
       --> tools/dylint/examples/stringified_error.rs:138:77
        |
    138 | fn tested(error: &ParseIntError) -> usize { if let Some(("cannot", rest)) = error.to_string().split_once(' ') { rest.len() } else {...
        |                                                                             ^^^^^^^^^^^^^^^^^
        |
        = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

    warning: text of error `std::num::ParseIntError` is compared
       --> tools/dylint/examples/stringified_error.rs:141:19
        |
    141 |     let message = error.to_string();
        |                   ^^^^^^^^^^^^^^^^^
        |
        = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

    warning: text of error `std::num::ParseIntError` is compared
       --> tools/dylint/examples/stringified_error.rs:145:84
        |
    145 | fn seen(error: &ParseIntError, known: &BTreeSet<String>) -> bool { known.contains(&error.to_string()) }
        |                                                                                    ^^^^^^^^^^^^^^^^^
        |
        = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

    warning: text of error `std::num::ParseIntError` is returned
       --> tools/dylint/examples/stringified_error.rs:166:72
        |
    166 | fn joined_text(errors: &[ParseIntError]) -> String { errors.iter().map(ToString::to_string).collect::<Vec<_>>().join(", ") }
        |                                                                        ^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `std::num::ParseIntError` as `#[source]` of a `thiserror` variant

    warning: text of `Mode` is compared
       --> tools/dylint/examples/stringified_error.rs:199:31
        |
    199 | fn fast(mode: Mode) -> bool { mode.to_string() == "fast" }
        |                               ^^^^^^^^^^^^^^^^
        |
        = help: compare or match `Mode` in place of its text

    warning: text of error `std::num::ParseIntError` is stored
       --> tools/dylint/examples/stringified_error.rs:205:62
        |
    205 | fn rejected(error: &ParseIntError) -> RefusedError { refused(error.to_string()) }
        |                                                              ^^^^^^^^^^^^^^^^^
        |
        = help: hold `std::num::ParseIntError` as `#[source]` of a `thiserror` variant

    warning: text of error `std::num::ParseIntError` is stored
       --> tools/dylint/examples/stringified_error.rs:209:67
        |
    209 | fn owned_io(error: ParseIntError) -> io::Error { io::Error::other(error.to_string()) }
        |                                                                   ^^^^^^^^^^^^^^^^^
        |
        = help: preserve `std::num::ParseIntError` as the constructor's typed source

    warning: text of error `std::num::ParseIntError` is stored
       --> tools/dylint/examples/stringified_error.rs:211:96
        |
    211 | fn formatted_io(kind: io::ErrorKind, error: ParseIntError) -> io::Error { io::Error::new(kind, format!("port: {error}")) }
        |                                                                                                ^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: preserve `std::num::ParseIntError` as the constructor's typed source

    warning: text of error `E` is stored
       --> tools/dylint/examples/stringified_error.rs:213:104
        |
    213 | fn bounded_io<E: core::error::Error + Send + Sync + 'static>(error: E) -> io::Error { io::Error::other(error.to_string()) }
        |                                                                                                        ^^^^^^^^^^^^^^^^^
        |
        = help: preserve `E` as the constructor's typed source

    warning: text of error `std::num::ParseIntError` is stored
       --> tools/dylint/examples/stringified_error.rs:219:86
        |
    219 | fn reused_io(error: ParseIntError) -> (io::Error, ParseIntError) { (io::Error::other(error.to_string()), error) }
        |                                                                                      ^^^^^^^^^^^^^^^^^
        |
        = help: preserve `std::num::ParseIntError` as the constructor's typed source

    warning: text of error `std::num::ParseIntError` is stored
       --> tools/dylint/examples/stringified_error.rs:221:101
        |
    221 | fn closure_io(text: &str) -> Result<u16, io::Error> { parsed(text).map_err(|error| io::Error::other(error.to_string())) }
        |                                                                                                     ^^^^^^^^^^^^^^^^^
        |
        = help: preserve `std::num::ParseIntError` as the constructor's typed source

    warning: text of error `std::num::ParseIntError` is compared
       --> tools/dylint/examples/stringified_error.rs:230:17
        |
    230 |     let first = error.to_string();
        |                 ^^^^^^^^^^^^^^^^^
        |
        = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

    warning: text of error `std::num::ParseIntError` is stored
       --> tools/dylint/examples/stringified_error.rs:246:18
        |
    246 |     let render = ToString::to_string;
        |                  ^^^^^^^^^^^^^^^^^^^
        |
        = help: hold `std::num::ParseIntError` as `#[source]` of a `thiserror` variant

    warning: text of `Mode` is compared
       --> tools/dylint/examples/stringified_error.rs:251:17
        |
    251 |     let first = mode.to_string();
        |                 ^^^^^^^^^^^^^^^^
        |
        = help: compare or match `Mode` in place of its text

    warning: text of error `std::num::ParseIntError` is compared
       --> tools/dylint/examples/stringified_error.rs:259:17
        |
    259 |     let first = error.to_string();
        |                 ^^^^^^^^^^^^^^^^^
        |
        = help: match the error's variant, `kind()`, or `downcast_ref` in place of its text

    warning: text of error `std::num::ParseIntError` is stored
       --> tools/dylint/examples/stringified_error.rs:269:21
        |
    269 |     let render = || error.to_string();
        |                     ^^^^^^^^^^^^^^^^^
        |
        = help: hold `std::num::ParseIntError` as `#[source]` of a `thiserror` variant

    warning: text of error `std::num::ParseIntError` is stored
       --> tools/dylint/examples/stringified_error.rs:280:26
        |
    280 |     let render = || Some(error.to_string());
        |                          ^^^^^^^^^^^^^^^^^
        |
        = help: hold `std::num::ParseIntError` as `#[source]` of a `thiserror` variant

    warning: text of error `std::num::ParseIntError` is stored
       --> tools/dylint/examples/stringified_error.rs:323:30
        |
    323 |     let factory = || move || error.to_string();
        |                              ^^^^^^^^^^^^^^^^^
        |
        = help: hold `std::num::ParseIntError` as `#[source]` of a `thiserror` variant

    warning: text of error `std::num::ParseIntError` is returned
       --> tools/dylint/examples/stringified_error.rs:328:19
        |
    328 |     let message = error.to_string();
        |                   ^^^^^^^^^^^^^^^^^
        |
        = help: hold `std::num::ParseIntError` as `#[source]` of a `thiserror` variant

    warning: text of error `uuid::Error` is stored
      --> tools/dylint/examples/stringified_uuid.rs:42:59
       |
    42 | fn failed(error: uuid::Error) -> Session { Session { key: error.to_string() } }
       |                                                           ^^^^^^^^^^^^^^^^^
       |
       = help: hold `uuid::Error` as `#[source]` of a `thiserror` variant
       = note: `#[warn(stringified_error)]` on by default

    warning: text of error `std::num::ParseIntError` is stored
       --> tools/dylint/examples/untyped_error.rs:344:133
        |
    344 | ...p_err(|error: ParseIntError| serde_json::Value::String(error.to_string())) }
        |                                                           ^^^^^^^^^^^^^^^^^
        |
        = help: hold `std::num::ParseIntError` as `#[source]` of a `thiserror` variant
        = note: `#[warn(stringified_error)]` on by default
    "#);
}

fn untyped_error(found: &str) {
    insta::assert_snapshot!(found, @r#"
    warning: error `anyhow::Error` is a report that erases each failure's type
      --> tools/dylint/examples/discarded_error_source.rs:41:83
       |
    41 | fn destructured(text: &str) -> Result<u16, Failure> { text.parse::<u16>().map_err(anyhow::Error::from).map_err(|anyhow::Error { .. }...
       |                                                                                   ^^^^^^^^^^^^^^^^^^^
       |
       = help: return the concrete source error, or add a domain error for context or distinct failures
       = note: `#[warn(untyped_error)]` on by default

    warning: error `u16` is a primitive that names no failure
       --> tools/dylint/examples/duplicate_type.rs:467:25
        |
    467 | const fn outcome(value: Result<u8, u16>) -> Outcome<u8, u16> {
        |                         ^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures
        = note: `#[warn(untyped_error)]` on by default

    warning: error `Box<dyn Error>` is an erased `dyn Error` callers cannot match
      --> tools/dylint/../../tests/rust/tools/dylint/expansions.rs:49:26
       |
    49 |     fn boxed_helper() -> Result<(), Box<dyn Error>> { Ok(()) }
       |                          ^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return the concrete source error, or add a domain error for context or distinct failures
       = note: `#[warn(untyped_error)]` on by default

    warning: error `anyhow::Error` is a report that erases each failure's type
      --> tools/dylint/../../tests/rust/tools/dylint/expansions.rs:51:27
       |
    51 |     fn report_helper() -> anyhow::Result<()> { Ok(()) }
       |                           ^^^^^^^^^^^^^^^^^^
       |
       = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `anyhow::Error` is a report that erases each failure's type
       --> tools/dylint/../../tests/rust/tools/dylint/expansions.rs:160:38
        |
    160 |         fn nested_report_helper() -> anyhow::Result<()> { Ok(()) }
        |                                      ^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `String` is text callers cannot match
       --> tools/dylint/../../tests/rust/tools/dylint/expansions.rs:165:31
        |
    165 |     fn string_test_error() -> Result<(), String> { Ok(()) }
        |                               ^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `anyhow::Error` is a report that erases each failure's type
       --> tools/dylint/../../tests/rust/tools/dylint/expansions.rs:169:19
        |
    169 |         black_box(anyhow::Error::from(fmt::Error));
        |                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `anyhow::Error` is a report that erases each failure's type
       --> tools/dylint/../../tests/rust/tools/dylint/expansions.rs:176:19
        |
    176 |         black_box(anyhow::Error::from(fmt::Error));
        |                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `anyhow::Error` is a report that erases each failure's type
       --> tools/dylint/../../tests/rust/tools/dylint/expansions.rs:182:9
        |
    182 |         Err::<(), _>(fmt::Error).context("rendering")?;
        |         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `u8` is a primitive that names no failure
      --> tools/dylint/examples/short_circuited_join.rs:66:35
       |
    66 | async fn separate_callbacks() -> (Result<u32, u8>, Result<u32, u8>) {
       |                                   ^^^^^^^^^^^^^^^
       |
       = help: return the concrete source error, or add a domain error for context or distinct failures
       = note: `#[warn(untyped_error)]` on by default

    warning: error `u8` is a primitive that names no failure
      --> tools/dylint/examples/short_circuited_join.rs:66:52
       |
    66 | async fn separate_callbacks() -> (Result<u32, u8>, Result<u32, u8>) {
       |                                                    ^^^^^^^^^^^^^^^
       |
       = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `u8` is a primitive that names no failure
      --> tools/dylint/examples/short_circuited_join.rs:68:7
       |
    68 |     ((|| Ok(left?))(), (|| Ok(right?))())
       |       ^^
       |
       = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `u8` is a primitive that names no failure
      --> tools/dylint/examples/short_circuited_join.rs:68:25
       |
    68 |     ((|| Ok(left?))(), (|| Ok(right?))())
       |                         ^^
       |
       = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `u8` is a primitive that names no failure
      --> tools/dylint/examples/short_circuited_join.rs:71:31
       |
    71 | async fn repeated_result() -> Result<u32, u8> {
       |                               ^^^^^^^^^^^^^^^
       |
       = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `u8` is a primitive that names no failure
      --> tools/dylint/examples/short_circuited_join.rs:77:35
       |
    77 | async fn exclusive(flag: bool) -> Result<u32, u8> {
       |                                   ^^^^^^^^^^^^^^^
       |
       = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `u8` is a primitive that names no failure
      --> tools/dylint/examples/short_circuited_join.rs:82:56
       |
    82 | async fn nested_exclusive(outer: bool, inner: bool) -> Result<u32, u8> {
       |                                                        ^^^^^^^^^^^^^^^
       |
       = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `u8` is a primitive that names no failure
      --> tools/dylint/examples/short_circuited_join.rs:87:39
       |
    87 | async fn complementary(flag: bool) -> Result<(), u8> {
       |                                       ^^^^^^^^^^^^^^
       |
       = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `u8` is a primitive that names no failure
      --> tools/dylint/examples/short_circuited_join.rs:98:23
       |
    98 | async fn grouped() -> Result<u32, u8> {
       |                       ^^^^^^^^^^^^^^^
       |
       = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `u8` is a primitive that names no failure
       --> tools/dylint/examples/short_circuited_join.rs:104:33
        |
    104 | async fn infinite_buffered() -> Result<Vec<u32>, u8> { stream::repeat_with(|| future::ready(Err::<u32, u8>(1))).buffered(4).try_col...
        |                                 ^^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `u8` is a primitive that names no failure
       --> tools/dylint/examples/short_circuited_join.rs:104:93
        |
    104 | async fn infinite_buffered() -> Result<Vec<u32>, u8> { stream::repeat_with(|| future::ready(Err::<u32, u8>(1))).buffered(4).try_col...
        |                                                                                             ^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `u8` is a primitive that names no failure
       --> tools/dylint/examples/short_circuited_join.rs:106:23
        |
    106 | async fn cycling() -> Result<Vec<u32>, u8> { future::join_all([future::ready(Err::<u32, u8>(1))]).await.into_iter().cycle().collect...
        |                       ^^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `u8` is a primitive that names no failure
       --> tools/dylint/examples/short_circuited_join.rs:110:38
        |
    110 | async fn dependent_continuation() -> Result<u32, u8> {
        |                                      ^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `u8` is a primitive that names no failure
       --> tools/dylint/examples/short_circuited_join.rs:118:32
        |
    118 | async fn selected_operand() -> Result<bool, u8> {
        |                                ^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `String` is text callers cannot match
      --> tools/dylint/examples/stringified_error.rs:77:37
       |
    77 |     let port = parsed(text).map_err(|error| error.to_string())?;
       |                                     ^^^^^^^
       |
       = help: return the concrete source error, or add a domain error for context or distinct failures
       = note: `#[warn(untyped_error)]` on by default

    warning: error `String` is text callers cannot match
      --> tools/dylint/examples/stringified_error.rs:97:65
       |
    97 | fn closure(text: &str) -> Option<String> { parsed(text).map_err(|error| error.to_string()).err() }
       |                                                                 ^^^^^^^
       |
       = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `String` is text callers cannot match
      --> tools/dylint/examples/stringified_error.rs:99:38
       |
    99 | fn errored(error: &ParseIntError) -> Result<u16, String> { Err(error.to_string()) }
       |                                      ^^^^^^^^^^^^^^^^^^^
       |
       = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `String` is text callers cannot match
       --> tools/dylint/examples/stringified_error.rs:103:41
        |
    103 | const fn error_text(message: String) -> Result<u16, String> { Err(message) }
        |                                         ^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `String` is text callers cannot match
       --> tools/dylint/examples/stringified_error.rs:105:46
        |
    105 | fn factory_errored(error: &ParseIntError) -> Result<u16, String> { error_text(error.to_string()) }
        |                                              ^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `u8` is a primitive that names no failure
       --> tools/dylint/examples/unending_join.rs:209:96
        |
    209 |     let drained = WatchStream::new(sender.subscribe()).map(Ok::<_, u8>).try_for_each(|_| ready(Err::<(), _>(1)));
        |                                                                                                ^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures
        = note: `#[warn(untyped_error)]` on by default

    warning: error `Box<dyn Error + Send + Sync>` is an erased `dyn Error` callers cannot match
      --> tools/dylint/examples/untyped_error.rs:29:17
       |
    29 | type Boxed<T> = Result<T, Box<dyn Error + Send + Sync>>;
       |                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: return the concrete source error, or add a domain error for context or distinct failures
       = note: `#[warn(untyped_error)]` on by default

    warning: error `String` is text callers cannot match
      --> tools/dylint/examples/untyped_error.rs:33:23
       |
    33 |     fn read(&self) -> Result<u16, String>;
       |                       ^^^^^^^^^^^^^^^^^^^
       |
       = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `()` is a unit type that names no failure
      --> tools/dylint/examples/untyped_error.rs:46:14
       |
    46 |     outcome: Result<u16, ()>,
       |              ^^^^^^^^^^^^^^^
       |
       = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `String` is text callers cannot match
      --> tools/dylint/examples/untyped_error.rs:53:18
       |
    53 |     type Error = String;
       |                  ^^^^^^
       |
       = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `String` is text callers cannot match
      --> tools/dylint/examples/untyped_error.rs:69:18
       |
    69 |     type Error = String;
       |                  ^^^^^^
       |
       = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `Invalid` holds a collection of errors that can be empty
       --> tools/dylint/examples/untyped_error.rs:111:5
        |
    111 |     Invalid(Vec<PortError>),
        |     ^^^^^^^
        |
        = help: hold the errors as `nonempty_collections::NEVec<PortError>`

    warning: error `Unreachable` holds an erased `dyn Error` callers cannot match
       --> tools/dylint/examples/untyped_error.rs:149:5
        |
    149 |     Unreachable(#[source] Box<dyn Error + Send + Sync>),
        |     ^^^^^^^^^^^
        |
        = help: hold the wrapped error's concrete type as `#[source]`

    warning: error `String` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:200:27
        |
    200 | fn spelled(text: &str) -> Result<u16, String> { if text.is_empty() { Err(String::from("port text is empty")) } else { Digits::try_f...
        |                           ^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `()` is a unit type that names no failure
       --> tools/dylint/examples/untyped_error.rs:202:30
        |
    202 | pub fn probed(text: &str) -> Result<u16, ()> { text.parse().map_err(|_error: ParseIntError| ()) }
        |                              ^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `&str` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:204:28
        |
    204 | fn borrowed(text: &str) -> Result<u16, &'static str> { text.parse().map_err(|_error: ParseIntError| "port is not a number") }
        |                            ^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `Box<dyn Error + Send + Sync>` is an erased `dyn Error` callers cannot match
       --> tools/dylint/examples/untyped_error.rs:206:25
        |
    206 | fn boxed(text: &str) -> Result<u16, Box<dyn Error + Send + Sync>> { Ok(text.trim().parse()?) }
        |                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `String` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:210:35
        |
    210 | fn generic_default(text: &str) -> Fallible<u16> { text.parse().map_err(|_error: ParseIntError| String::from("port is not a number")) }
        |                                   ^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `String` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:212:33
        |
    212 | async fn awaited(text: &str) -> Result<u16, String> {
        |                                 ^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `Code` is a `Display` type that implements no `core::error::Error`
       --> tools/dylint/examples/untyped_error.rs:217:25
        |
    217 | fn coded(text: &str) -> Result<u16, Code> { text.parse().map_err(|_error: ParseIntError| Code::Invalid) }
        |                         ^^^^^^^^^^^^^^^^^
        |
    note: declared here
       --> tools/dylint/examples/untyped_error.rs:89:1
        |
     89 | enum Code {
        | ^^^^^^^^^
        = help: implement `core::error::Error` for `Code`

    warning: error `Code` is a `Display` type that implements no `core::error::Error`
       --> tools/dylint/examples/untyped_error.rs:219:39
        |
    219 | fn retried(text: &str, code: Code) -> Result<u16, Code> { text.parse().map_err(|_error: ParseIntError| code) }
        |                                       ^^^^^^^^^^^^^^^^^
        |
    note: declared here
       --> tools/dylint/examples/untyped_error.rs:89:1
        |
     89 | enum Code {
        | ^^^^^^^^^
        = help: implement `core::error::Error` for `Code`

    warning: error `Box<str>` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:223:30
        |
    223 | fn boxed_text(text: &str) -> Result<u16, Box<str>> { text.parse().map_err(|_error: ParseIntError| Box::from("boxed port is not a nu...
        |                              ^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `Arc<str>` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:225:31
        |
    225 | fn shared_text(text: &str) -> Result<u16, Arc<str>> { text.parse().map_err(|_error: ParseIntError| Arc::from("shared port is not a ...
        |                               ^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `Rc<str>` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:227:32
        |
    227 | fn counted_text(text: &str) -> Result<u16, Rc<str>> { text.parse().map_err(|_error: ParseIntError| Rc::from("counted port is not a ...
        |                                ^^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `Cow<'static, str>` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:229:33
        |
    229 | fn borrowed_text(text: &str) -> Result<u16, Cow<'static, str>> { text.parse().map_err(|_error: ParseIntError| Cow::Borrowed("port i...
        |                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `Arc<dyn Error + Send + Sync>` is an erased `dyn Error` callers cannot match
       --> tools/dylint/examples/untyped_error.rs:231:32
        |
    231 | fn shared_error(text: &str) -> Result<u16, Arc<dyn Error + Send + Sync>> { text.parse().map_err(|error: ParseIntError| -> Arc<dyn E...
        |                                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `Rc<dyn Error>` is an erased `dyn Error` callers cannot match
       --> tools/dylint/examples/untyped_error.rs:233:33
        |
    233 | fn counted_error(text: &str) -> Result<u16, Rc<dyn Error>> { text.parse().map_err(|error: ParseIntError| -> Rc<dyn Error> { Rc::new...
        |                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `String` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:235:30
        |
    235 | fn returned(text: String) -> Result<u16, String> { text.parse().map_err(|_error: ParseIntError| text) }
        |                              ^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `Vec<PortError>` is a collection of errors that can be empty
       --> tools/dylint/examples/untyped_error.rs:239:31
        |
    239 | fn planned(texts: &[&str]) -> Result<Vec<u16>, Vec<PortError>> {
        |                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: collect the results into `validated::Validated` and return `Result<_, NEVec<PortError>>` from `.ok()`

    warning: function signals success with an empty `Vec<PortError>`
       --> tools/dylint/examples/untyped_error.rs:244:31
        |
    244 | fn audited(texts: &[&str]) -> Vec<PortError> { texts.iter().filter_map(|text| typed(text).err()).collect() }
        |                               ^^^^^^^^^^^^^^
        |
        = help: collect the results into `validated::Validated` and return `Result<(), NEVec<PortError>>` from `.ok()`

    warning: error `String` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:254:77
        |
    254 | fn defaulted(port: Option<u16>) -> Result<u16, PortError> { port.ok_or_else(|| String::from("port is missing")).or_else(|_text| typ...
        |                                                                             ^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `&str` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:269:40
        |
    269 |     let present = if text.is_empty() { Err("port text is empty") } else { Ok(text) };
        |                                        ^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `String` is text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:270:19
        |
    270 |     let spelled = |digits: &str| if digits.is_empty() { Err(String::from("port text is empty")) } else { digits.parse::<u16>().map_...
        |                   ^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `anyhow::Error` is a report that erases each failure's type
       --> tools/dylint/examples/untyped_error.rs:293:29
        |
    293 | fn reported(path: &Path) -> anyhow::Result<String> { Ok(fs_err::read_to_string(path)?) }
        |                             ^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `anyhow::Error` is a report that erases each failure's type
       --> tools/dylint/examples/untyped_error.rs:299:56
        |
    299 | fn contexted(text: &str) -> Result<u16, ReportError> { text.parse::<u16>().context("port").map_err(ReportError::Project) }
        |                                                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: `ErrorData` is built from written text while local `Unserved` converts into it
       --> tools/dylint/examples/untyped_error.rs:325:31
        |
    325 |     if port == reserved { Err(ErrorData::invalid_params(format!("{name} names a reserved port"), None)) } else { Ok(port) }
        |                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: text written here
       --> tools/dylint/examples/untyped_error.rs:325:57
        |
    325 |     if port == reserved { Err(ErrorData::invalid_params(format!("{name} names a reserved port"), None)) } else { Ok(port) }
        |                                                         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: add a variant holding the subject to `Unserved` and convert it into `ErrorData`

    warning: `ErrorData` is built from written text while local `Unserved` converts into it
       --> tools/dylint/examples/untyped_error.rs:329:78
        |
    329 | ...d_then(|port| if port == 0 { Err(ErrorData::internal_error("port zero cannot be served", None)) } else { Ok(port) })
        |                                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
    note: text written here
       --> tools/dylint/examples/untyped_error.rs:329:104
        |
    329 | ...ort| if port == 0 { Err(ErrorData::internal_error("port zero cannot be served", None)) } else { Ok(port) })
        |                                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: add a variant holding the subject to `Unserved` and convert it into `ErrorData`

    warning: error `i32` is a primitive that names no failure
       --> tools/dylint/examples/untyped_error.rs:342:28
        |
    342 | fn numbered(text: &str) -> Result<u16, i32> { text.parse().map_err(|_error: ParseIntError| -1) }
        |                            ^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `Vec<String>` is a collection of text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:346:28
        |
    346 | fn messages(text: &str) -> Result<u16, Vec<String>> { text.parse().map_err(|error: ParseIntError| vec![error.to_string()]) }
        |                            ^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `(u16, String)` is a tuple that names no failure
       --> tools/dylint/examples/untyped_error.rs:348:26
        |
    348 | fn paired(text: &str) -> Result<u16, (u16, String)> { text.parse().map_err(|error: ParseIntError| (1, error.to_string())) }
        |                          ^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `anyhow::Error` is a report that erases each failure's type
       --> tools/dylint/examples/untyped_error.rs:356:86
        |
    356 | fn wrapped(text: &str) -> Result<u16, WrappedError> { Ok(text.parse::<u16>().map_err(anyhow::Error::from)?) }
        |                                                                                      ^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `anyhow::Error` is a report that erases each failure's type
       --> tools/dylint/examples/untyped_error.rs:384:55
        |
    384 | fn mixed(settings: &DiagNutsSettings, local: bool) -> anyhow::Result<()> {
        |                                                       ^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `anyhow::Error` is a report that erases each failure's type
       --> tools/dylint/examples/untyped_error.rs:391:48
        |
    391 | fn chosen_report<E>(outcome: Result<(), E>) -> anyhow::Result<()>
        |                                                ^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `anyhow::Error` is a report that erases each failure's type
       --> tools/dylint/examples/untyped_error.rs:396:81
        |
    396 | fn mixed_propagation<E>(settings: &DiagNutsSettings, outcome: Result<(), E>) -> anyhow::Result<()>
        |                                                                                 ^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `anyhow::Error` is a report that erases each failure's type
       --> tools/dylint/examples/untyped_error.rs:416:82
        |
    416 | fn field_report(settings: &DiagNutsSettings, held: (anyhow::Result<()>, u16)) -> anyhow::Result<()> {
        |                                                                                  ^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `anyhow::Error` is a report that erases each failure's type
       --> tools/dylint/examples/untyped_error.rs:421:79
        |
    421 | fn joined_report(settings: &DiagNutsSettings, outcome: anyhow::Result<()>) -> anyhow::Result<()> { settings.validate().and(outcome) }
        |                                                                               ^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `anyhow::Error` is a report that erases each failure's type
       --> tools/dylint/examples/untyped_error.rs:423:79
        |
    423 | fn argument_report<E>(settings: &DiagNutsSettings, outcome: Result<(), E>) -> anyhow::Result<()>
        |                                                                               ^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `anyhow::Error` is a report that erases each failure's type
       --> tools/dylint/examples/untyped_error.rs:429:82
        |
    429 | fn tried_local_report<E>(settings: &DiagNutsSettings, outcome: Result<(), E>) -> anyhow::Result<()>
        |                                                                                  ^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `Box<dyn Error + Send + Sync>` is an erased `dyn Error` callers cannot match
       --> tools/dylint/examples/untyped_error.rs:438:20
        |
    438 | async fn main() -> Result<(), Box<dyn Error + Send + Sync>> {
        |                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `anyhow::Error` is a report that erases each failure's type
       --> tools/dylint/examples/untyped_error.rs:462:19
        |
    462 |         annotated(anyhow::Error::from(fmt::Error)),
        |                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `()` is a unit type that names no failure
       --> tools/dylint/examples/untyped_error.rs:495:49
        |
    495 |         Pending { outcome: text.parse().map_err(drop) }.outcome.unwrap_or_default(),
        |                                                 ^^^^
        |
        = help: return the concrete source error, or add a domain error for context or distinct failures

    warning: error `Message` holds text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:121:5
        |
    121 |     Message(String),
        |     ^^^^^^^
        |
    note: text written here
       --> tools/dylint/examples/untyped_error.rs:278:60
        |
    278 |         "refused" => Err(ConfigError::Message(String::from("configuration refused the port"))),
        |                                                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: move the text into `#[error(..)]` with each interpolated value as a typed field

    warning: error `Detail` holds text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:125:5
        |
    125 |     Detail { detail: Box<str>, context: Cow<'static, str> },
        |     ^^^^^^
        |
    note: text written here
       --> tools/dylint/examples/untyped_error.rs:279:65
        |
    279 |         "detail" => Err(ConfigError::Detail { detail: Box::from("port detail"), context: Cow::Borrowed("port context") }),
        |                                                                 ^^^^^^^^^^^^^
        = help: move the text into `#[error(..)]` with each interpolated value as a typed field

    warning: error `Reserved` holds text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:135:5
        |
    135 |     Reserved { port: String },
        |     ^^^^^^^^
        |
    note: text written here
       --> tools/dylint/examples/untyped_error.rs:280:57
        |
    280 |         "reserved" => Err(ConfigError::Reserved { port: 80_u16.to_string() }),
        |                                                         ^^^^^^^^^^^^^^^^^^
        = help: move the text into `#[error(..)]` with each interpolated value as a typed field

    warning: error `Composed` holds text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:137:5
        |
    137 |     Composed(Box<str>),
        |     ^^^^^^^^
        |
    note: text written here
       --> tools/dylint/examples/untyped_error.rs:281:49
        |
    281 |         "composed" => Err(ConfigError::Composed(format!("{text} refused the port").into())),
        |                                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        = help: move the text into `#[error(..)]` with each interpolated value as a typed field

    warning: error `Refusal` holds text callers cannot match
       --> tools/dylint/examples/untyped_error.rs:144:1
        |
    144 | struct Refusal(Arc<str>);
        | ^^^^^^^^^^^^^^
        |
    note: text written here
       --> tools/dylint/examples/untyped_error.rs:287:62
        |
    287 | fn refused() -> Result<u16, Refusal> { Err(Refusal(Arc::from("port refused"))) }
        |                                                              ^^^^^^^^^^^^^^
        = help: move the text into `#[error(..)]` with each interpolated value as a typed field

    warning: error `String` is text callers cannot match
     --> tools/dylint/examples/untyped_error_exported.rs:7:33
      |
    7 | pub fn described(text: &str) -> Result<u16, String> { text.parse().map_err(|error: ParseIntError| error.to_string()) }
      |                                 ^^^^^^^^^^^^^^^^^^^
      |
      = help: return the concrete source error, or add a domain error for context or distinct failures
      = note: `#[warn(untyped_error)]` on by default
    "#);
}

}
