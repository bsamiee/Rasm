# [BOUNDARIES]

Covers untyped data entering and leaving domain types.

## [01]-[DECODE]

Decoding is the one place untrusted data becomes domain values, and `nutype` states a rule across fields with its wire form in one declaration:

```rust
#[nutype(
    validate(predicate = |span| !span.is_empty()),
    derive(Debug, Clone, PartialEq, Eq, Deref, Serialize, Deserialize)
)]
pub struct Window(Range<Timestamp>);
```

`Window` decodes from and encodes to `{"start": .., "end": ..}`, rejects an unknown key or an end at or before its start, and reaches `Range` members (`window.contains(&now)`) through `Deref`. Domain enums spelled as flat wire fields decode through `#[serde(try_from = "<Wire>")]`.

Decode errors name the failing path: `toml` errors and `axum::Json` rejections hold it, and a `serde_json::Deserializer` a program drives decodes through `serde_path_to_error::deserialize`, then `Deserializer::end` rejects trailing input.

## [02]-[TAGGED_UNIONS]

Enums cross a boundary with one tag declaration, and fields every variant shares sit on a record flattening the enum beside them:

```rust
#[derive(Debug, Clone, PartialEq, Eq, Deserialize)]
pub struct Event {
    pub item: ItemId,
    pub at: Timestamp,
    #[serde(flatten)]
    pub change: Change,
}

#[derive(Debug, Clone, PartialEq, Eq, Deserialize)]
#[serde(tag = "type", rename_all = "snake_case")]
pub enum Change {
    Created,
    Renamed {
        name: Label,
    },
    Removed,
    #[serde(other)]
    Unknown,
}
```

`{"type": "renamed", "item": .., "at": .., "name": ..}` decodes into one `Event`, and a consumer matches `change` alone. `ItemId` is a `TypedUuid`, `Label` a `nutype` over `String`. Consumers of a stream that producers extend end the enum with a `#[serde(other)]` unit variant: an unknown tag decodes to `Unknown`, and a known tag with malformed fields fails. Closed protocols reject an unknown tag at decode. Encoded outcomes and errors take the same form: an outcome is a tagged enum of its cases in place of `Result`'s `{"Ok": ..}`, and the crate error derives `Serialize` under a tag with `#[serde(skip)]` on each `source`, its reason enum serializing through `serde_with::SerializeDisplay` from its `Display`.

## [03]-[FIELD_CONVERSIONS]

Encodings sit on the field, and the field keeps its domain type:

| [INDEX] | [WIRE]                             | [DECLARATION]                                        |
| :-----: | :--------------------------------- | :--------------------------------------------------- |
|  [01]   | Text a type parses and renders     | `#[serde_as(as = "DisplayFromStr")]`                 |
|  [02]   | Number or its text, number written | `#[serde_as(as = "PickFirst<(_, DisplayFromStr)>")]` |
|  [03]   | Bytes as base64 text               | `#[serde_as(as = "Base64")]`                         |
|  [04]   | Instant as epoch seconds           | `#[serde_as(as = "TimestampSeconds<i64>")]`          |
|  [05]   | One value or a list                | `#[serde_as(as = "OneOrMany<_>")]`                   |
|  [06]   | Map rejecting a repeated key       | `#[serde_as(as = "MapPreventDuplicates<_, _>")]`     |
|  [07]   | List dropping a malformed item     | `#[serde_as(as = "VecSkipError<_>")]`                |
|  [08]   | Absent key for each `None`         | `#[skip_serializing_none]` on the type               |
|  [09]   | JSON forwarded unread              | `Box<RawValue>` field                                |
|  [10]   | Undeclared keys kept               | `#[serde(flatten)] rest: Map<String, Value>`         |
|  [11]   | Undeclared keys rejected           | `#[serde(deny_unknown_fields)]`                      |

`#[serde_as]` and `#[skip_serializing_none]` precede the derive, and one `#[serde_as]` enables every conversion on the type. Conversions compose inside containers (`Vec<DisplayFromStr>`, `Option<Base64>`), an `Option` conversion decodes an absent key as `None`, and a derived `JsonSchema` reads each conversion's wire form. `TimestampSeconds` takes a jiff `Timestamp` or `Zoned`, and `DurationSeconds` a `SignedDuration`. A plain map decode keeps the last value of a repeated key, and a map from untrusted input rejects the repeat. `VecSkipError<_, I>` reports each dropped item to `I`, an `InspectError` implementor. Records a program owns reject undeclared keys, and records it passes through keep them in `rest`, the one `Value` field, and an encode writes back what it read. `RawValue` copies a forwarded value's bytes unparsed inside a record whose fields decode without `flatten`.

## [04]-[BINARY]

Binary formats decode and encode from one `binrw` declaration. Byte order and magic sit on the type, and a raw field enters its domain type through the constructor:

```rust
#[binrw]
#[brw(little, magic = b"ITEM")]
#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Frame {
    #[br(try_map = Quantity::try_new)]
    #[bw(map = |quantity| quantity.into_inner())]
    pub quantity: Quantity,
    #[br(temp)]
    #[bw(try_calc = u32::try_from(readings.len()))]
    count: u32,
    #[br(count = count)]
    pub readings: Vec<i16>,
}
```

`Frame::read(&mut Cursor::new(bytes))` returns `BinResult<Frame>`, and `binrw::Error` names the stream position of a bad magic or a rejected quantity. `Quantity` is a `nutype` over `u32`. Length prefixes are `temp` on read and computed from the collection on write, and `#[binrw]` precedes the derives to remove temporary fields before they run.

## [05]-[CONFIGURATION]

Invocation values (settings path, secrets, telemetry) decode from arguments and environment through `clap`, service settings from one file, each setting from one source:

```rust
#[derive(Debug, Parser)]
#[command(version, about)]
pub struct Args {
    #[arg(long, env)]
    pub settings: Utf8PathBuf,
    #[arg(long, env, hide_env_values = true)]
    pub token: SecretString,
    #[command(flatten)]
    pub telemetry: Options,
}
```

Flags and variables derive from field names (`--settings`, `SETTINGS`). `SecretString` redacts its `Debug` output, and code reads it through `expose_secret` at the call sending it. `Options` from the workspace telemetry crate adds the log filter, service name, propagator, and exporter flags with their `RUST_LOG` and `OTEL_*` variables. Variables the program reads are arguments, and a variable a dependency reads itself (`OTEL_EXPORTER_OTLP_ENDPOINT`) reaches it untouched. Settings declare each default on its field, and one generic function decodes every file:

```rust
#[derive(Debug, Clone, Default, Deserialize)]
#[serde(default, deny_unknown_fields)]
pub struct Settings {
    pub listen: SocketAddr = SocketAddr::new(IpAddr::V4(Ipv4Addr::LOCALHOST), 8080),
    pub timeout: SignedDuration = SignedDuration::from_secs(30),
    pub windows: Vec<Window> = Vec::new(),
}

pub async fn decoded<T>(path: &Utf8Path) -> Result<T, ServiceError>
where T: DeserializeOwned {
    toml::from_str(&fs::read_to_string(path).await.map_err(|source| ServiceError::Read { path: path.to_owned(), source })?).map_err(|source| ServiceError::Decode { path: path.to_owned(), source })
}
```

`decoded::<Settings>(&args.settings)` decodes `timeout = "30s"` through `SignedDuration`'s friendly format and every value into its domain type. `ServiceError` is the crate error. `Read` and `Decode` hold the file path as a field beside each source, and `Runtime` holds the `io::Error` a runtime start returns.

## [06]-[ENTRY_POINT]

`main` holds a program's only global effects:

```rust
fn main() -> ExitCode {
    let Args { settings, token, telemetry } = Args::parse();
    telemetry.run(&Args::command(), [], "info", Identity::new(), |_| Runtime::new().map_err(ServiceError::Runtime).and_then(|runtime| runtime.block_on(serve(settings, token))).map_err(NEVec::new))
}
```

`Options::run` installs the subscriber, exporters, propagators, and panic hook around the work, and logs and translates each failure. Logs write to stderr, stdout holds output another program reads through `Serialize`, and output a person reads renders through `Display`.
