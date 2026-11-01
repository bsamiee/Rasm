# [BOUNDARIES]

Covers where untyped data meets the domain, from decoding text and bytes into constructed types to encoding output and composing the host.

## [01]-[DECODE]

Decoding is the one place untrusted data becomes domain values, and a record of constrained fields decodes valid with no check of its own. A rule across fields constrains the whole value, and `nutype` over a standard type states it in one declaration:

```rust
#[nutype(
    validate(predicate = |span| !span.is_empty()),
    derive(Debug, Clone, PartialEq, Eq, Deref, Serialize, Deserialize)
)]
pub struct Window(Range<Timestamp>);
```

`Window` decodes from `{"start": .., "end": ..}`, rejects an unknown key or an end at or before its start, encodes through the same shape, and reaches `Range` members (`window.contains(&now)`) through `Deref`. Wire shapes no standard type matches become a record declared as the inner type, and a domain enum spelled as flat wire fields decodes through `#[serde(try_from = "<Wire>")]`.

Decoders name the failing value: `toml` errors hold its key path and source excerpt, `axum::Json` rejections its field path. A `serde_json::Deserializer` a program drives decodes through `serde_path_to_error::deserialize`, and `Deserializer::end` then rejects trailing input.

## [02]-[TAGGED_UNIONS]

Enums cross a boundary with one tag declaration, and fields every variant shares sit on a record that flattens the enum beside them:

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
    Renamed { name: Label },
    Removed,
    #[serde(other)]
    Unknown,
}
```

`{"type": "renamed", "item": .., "at": .., "name": ..}` decodes into one `Event`, and a consumer matches `change` alone. `ItemId` is a `TypedUuid`, `Label` a `nutype` over `String`. Consumers of a stream that producers extend end the enum with a `#[serde(other)]` unit variant: an unknown tag decodes to `Unknown`, and a known tag with malformed fields fails. Closed protocols reject an unknown tag at decode. Errors a boundary encodes take the same form: the crate error derives `Serialize` under a tag with `#[serde(skip)]` on each `source`, and its reason enum serializes through `serde_with::SerializeDisplay` from the `Display` that names each reason.

## [03]-[FIELD_CODECS]

Field encodings sit on the field, and the field keeps its domain type:

| [INDEX] | [WIRE]                         | [DECLARATION]                                |
| :-----: | :----------------------------- | :------------------------------------------- |
|  [01]   | Text a type parses and renders | `#[serde_as(as = "DisplayFromStr")]`         |
|  [02]   | Bytes as base64 text           | `#[serde_as(as = "Base64")]`                 |
|  [03]   | Instant as epoch seconds       | `#[serde_as(as = "TimestampSeconds<i64>")]`  |
|  [04]   | One value or a list            | `#[serde_as(as = "OneOrMany<_>")]`           |
|  [05]   | Absent key for each `None`     | `#[skip_serializing_none]` on the type       |
|  [06]   | JSON forwarded unread          | `Box<RawValue>` field                        |
|  [07]   | Undeclared keys kept           | `#[serde(flatten)] rest: Map<String, Value>` |
|  [08]   | Undeclared keys rejected       | `#[serde(deny_unknown_fields)]`              |
|  [09]   | Field names in another case    | `#[serde(rename_all = "camelCase")]`         |

`#[serde_as]` and `#[skip_serializing_none]` precede the derive. One `#[serde_as]` enables every codec on the type, codecs compose inside containers (`Vec<DisplayFromStr>`, `Option<Base64>`), an `Option` field under `serde_as` decodes an absent key as `None`, and a derived `JsonSchema` describes each field's wire form through the `#[schemars(with)]` that `serde_as` writes. Configuration and requests a program owns reject undeclared keys, so a misspelled key fails at decode. Records a program passes through keep undeclared keys in `rest`, and an encode writes back what it read. `RawValue` copies a forwarded value's bytes unparsed inside a record whose fields decode without `flatten`.

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

`Frame::read(&mut Cursor::new(bytes))` returns `BinResult<Frame>`, and `binrw::Error` names the stream position of a bad magic or a rejected quantity. `Quantity` is a `nutype` over `u32`. Length prefixes are `temp` on read and computed from the collection on write, so the collection is the one record of its length. `#[binrw]` precedes the derives and removes temporary fields before they run.

## [05]-[CONFIGURATION]

Invocation values (settings path, secrets, verbosity) decode from arguments and environment through `clap`, service settings decode from one file, and each setting has one source:

```rust
#[derive(Debug, Parser)]
#[command(version, about)]
pub struct Args {
    #[arg(long, env)]
    pub settings: Utf8PathBuf,
    #[arg(long, env, hide_env_values = true)]
    pub token: SecretString,
    #[command(flatten)]
    pub verbosity: Verbosity<InfoLevel>,
}
```

Flags and variable names derive from field names (`--settings`, `SETTINGS`). `SecretString` redacts its `Debug` output, and code reads it through `expose_secret` at the call that sends it. `Verbosity` adds `-v` and `-q` flags that resolve to a tracing level filter. Arguments hold values the program reads, and a variable a dependency reads (`RUST_LOG`) reaches that dependency untouched. Settings declare each default on its field:

```rust
#[derive(Debug, Clone, Default, Deserialize)]
#[serde(default, deny_unknown_fields)]
pub struct Settings {
    pub listen: SocketAddr = SocketAddr::new(IpAddr::V4(Ipv4Addr::LOCALHOST), 8080),
    pub timeout: SignedDuration = SignedDuration::from_secs(30),
    pub windows: Vec<Window> = Vec::new(),
}

pub async fn settings(path: &Utf8Path) -> Result<Settings, ServiceError> {
    let text = fs::read_to_string(path).await.map_err(|source| ServiceError::Read { path: path.to_owned(), source })?;
    toml::from_str(&text).map_err(|source| ServiceError::Settings { path: path.to_owned(), source })
}
```

`#[serde(default)]` fills each absent key from its field default, `timeout = "30s"` decodes through `SignedDuration`'s friendly format, and every value decodes into its domain type. `ServiceError` is the crate error.

## [06]-[HOST]

`main` holds a program's only global effects: it parses arguments, installs a subscriber, runs the composed effect, and translates its result:

```rust
#[tokio::main]
async fn main() -> ExitCode {
    let args = Args::parse();
    let filter = EnvFilter::from_default_env().add_directive(args.verbosity.tracing_level_filter().into());
    tracing_subscriber::fmt().with_env_filter(filter).with_writer(io::stderr).init();
    run(args).await.inspect_err(|failure| tracing::error!({ EXCEPTION_MESSAGE } = %Report::new(failure), "service stopped")).err().as_ref().map_or(ExitCode::SUCCESS, exit_code)
}
```

`-v` and `-q` set the level of every target, and a `RUST_LOG` directive naming a target refines it. Logs write to stderr, stdout holds output another program reads through `Serialize`, and output a person reads renders through `Display`.
