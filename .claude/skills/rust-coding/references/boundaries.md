# [BOUNDARIES]

## [01]-[DECODE]

Decoding is the one place untrusted data becomes domain values, and `nutype` states a rule across fields with its wire form in one declaration:

```rust
#[nutype(
    validate(predicate = |span| !span.is_empty()),
    derive(Debug, Clone, PartialEq, Eq, Deref, Serialize, Deserialize)
)]
pub struct Window(Range<Timestamp>);
```

`Window` round-trips `{"start": .., "end": ..}` and rejects an unknown key or an end at or before its start. `Deref` reaches `Range` members (`window.contains(&now)`). Domain enums spelled as flat wire fields decode through `#[serde(try_from = "<Wire>")]`.

Decode errors name the failing path, held by `toml` errors and `axum::Json` rejections. Programs driving a `serde_json::Deserializer` decode through `serde_path_to_error::deserialize`, then reject trailing input with `Deserializer::end`.

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

`{"type": "renamed", "item": .., "at": .., "name": ..}` decodes into one `Event`, and a consumer matches `change` alone. `ItemId` is a `TypedUuid`, `Label` a `nutype` over `String`. Streams producers extend end the enum with a `#[serde(other)]` unit variant: an unknown tag decodes to `Unknown`, a malformed known tag fails. Closed protocols reject an unknown tag at decode. Enums a consumer extends end with a generic `#[serde(untagged)]` variant (`Extended(K)`) that decodes consumer cases. Encoded outcomes are a tagged enum of their cases in place of `Result`'s `{"Ok": ..}`. Crate errors derive `Serialize` under a tag with `#[serde(skip)]` on each `source`. Reason enums serialize through `serde_with::SerializeDisplay`.

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

`#[serde_as]` and `#[skip_serializing_none]` precede the derive, and one `#[serde_as]` enables every conversion on the type. Conversions compose inside containers (`Vec<DisplayFromStr>`, `Option<Base64>`), and an `Option` conversion decodes an absent key as `None`. Derived `JsonSchema` reads each conversion's wire form. `TimestampSeconds` takes a jiff `Timestamp` or `Zoned`, and `DurationSeconds` a `SignedDuration`. A plain map decode keeps the last value of a repeated key, and a map from untrusted input rejects the repeat. `VecSkipError<_, I>` reports each dropped item to `I`, an `InspectError` implementor. Records a program owns reject undeclared keys. Pass-through records keep them in `rest`, their one `Value` field, and encode back what they read. `RawValue` copies a forwarded value's bytes unparsed inside a record that decodes without `flatten`.

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

`Frame::read(&mut Cursor::new(bytes))` returns `BinResult<Frame>`, and `binrw::Error` names the stream position of a bad magic or a rejected quantity. `Quantity` is a `nutype` over `u32`. Length prefixes are `temp` on read and computed from the collection on write. `#[binrw]` precedes the derives, removing temporary fields before they run.

## [05]-[CONFIGURATION]

Each setting has one source: `clap` decodes invocation values (settings paths, secrets, process flags) from arguments and environment, settings files the rest:

```rust
#[derive(Debug, Parser)]
#[command(version, about)]
pub struct Args {
    #[arg(long, global = true)]
    pub settings: Vec<Utf8PathBuf>,
    #[arg(long, env, hide_env_values = true)]
    pub token: SecretString,
    #[arg(long, value_enum, default_value_t)]
    pub retention: Retention,
}
```

Flags and variables derive from field names (`--token`, `TOKEN`), a variable a package or specification names takes its constant or literal (`env = EnvFilter::DEFAULT_ENV`, `env = "OTEL_PROPAGATORS"`), and each repeated `--settings` layers over earlier files and field defaults. `SecretString` redacts its `Debug` output, and code reads it through `expose_secret` at the call sending it. Choice enums derive `ValueEnum` with a `#[value(help)]` per variant, and `#[default]` beside `default_value_t` states the default once. Flags every program shares (log directory, man pages, color, telemetry with its `RUST_LOG` and `OTEL_*` variables) sit on a workspace record, and a record holding it beside each program's `Args` implements `CommandFactory` and `FromArgMatches` and restores the program's `about` through `Resettable::from`. Default `_mut` methods of `FromArgMatches` route to the `&ArgMatches` pair and each derived part clones, and the record writes all four methods with the `&` pair cloning once into the `_mut` pair. Builder commands read clap command metadata from `clap::command!()`. Flattened records take `#[group(skip)]` and `#[command(next_help_heading = ..)]`, and records sharing a type name keep apart arg groups and help sections. Variables the program reads are arguments, and a variable a dependency reads itself (`OTEL_EXPORTER_OTLP_ENDPOINT`) reaches it untouched. Settings declare each default on its field, and one generic function decodes every layer stack:

```rust
#[derive(Debug, Clone, Default, Deserialize)]
#[serde(default, deny_unknown_fields)]
pub struct Settings {
    pub listen: SocketAddr = SocketAddr::new(IpAddr::V4(Ipv4Addr::LOCALHOST), 8080),
    pub timeout: SignedDuration = SignedDuration::from_secs(30),
    pub windows: Vec<Window> = Vec::new(),
}

pub fn layered<S>(layers: &[Arc<Layer>]) -> Result<S, NEVec<ServiceError>>
where S: DeserializeOwned {
    fn merged<'text>(lower: Spanned<DeValue<'text>>, upper: Spanned<DeValue<'text>>) -> Spanned<DeValue<'text>> {
        let span = upper.span();
        Spanned::new(span, match (lower.into_inner(), upper.into_inner()) {
            (DeValue::Table(earlier), DeValue::Table(later)) => DeValue::Table(later.into_iter().fold(earlier, |mut table, (key, entry)| {
                let value = table.remove(&key).into_iter().fold(entry, |upper, lower| merged(lower, upper));
                table.insert(key, value);
                table
            })),
            (_, later) => later,
        })
    }
    let roots = layers
        .iter()
        .map(|layer| {
            let (table, errors) = DeTable::parse_recoverable(&layer.text);
            NEVec::try_from_vec(errors).map_or(Validated::Good(Spanned::new(table.span(), DeValue::Table(table.into_inner()))), |failures| {
                Validated::Fail(failures.into_nonempty_iter().map(|mut source| {
                    source.set_input(None);
                    ServiceError::Syntax { layer: Arc::clone(layer), span: source.span(), source: Box::new(source) }
                }).collect())
            })
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()?;
    serde_path_to_error::deserialize(ValueDeserializer::from(roots.iter().cloned().fold(Spanned::new(0..0, DeValue::Table(DeTable::new())), merged))).map_err(|error| {
        let located = iter::zip(layers, &roots).rev().find_map(|(layer, root)| {
            error
                .path()
                .iter()
                .try_fold(root, |value, segment| match segment {
                    Segment::Map { key } | Segment::Enum { variant: key } => value.get_ref().get(key),
                    Segment::Seq { index } => value.get_ref().get(*index),
                    Segment::Unknown => None,
                })
                .map(|value| (Arc::clone(layer), value.span()))
        });
        NEVec::new(ServiceError::Settings { located, source: Box::new(error) })
    })
}
```

`layered::<Settings>(&layers)` decodes `timeout = "30s"` through `SignedDuration`'s friendly format and every value into its domain type. `ServiceError` is the crate error, and a `Layer` holds one file's `path` and `text`. `parse_recoverable` reports every syntax error of every layer, and `set_input(None)` drops each error's input copy, its excerpt rendering from the layer through `annotate_snippets::Renderer::plain`. Later layers replace values and merge tables key by key. `located` holds the last layer defining the failing path and its span, an empty path (a missing root field) reads the last layer's root, and `None` means no layer defines it. `Syntax` holds `span: Option<Range<usize>>`, and `Settings` holds `located: Option<(Arc<Layer>, Range<usize>)>`.

## [06]-[ENTRY_POINT]

`main` holds a program's only global effects, one `command::run` call that parses arguments, installs telemetry, starts runtime and signal streams, and logs and translates each failure:

```rust
fn main() -> ExitCode {
    command::run(Defaults { .. }, io::stderr, Identity::new(), async |Process { args: Args { settings, token, retention }, signals, filter: _ }| {
        serve(layered::<Settings>(&read(settings).await?)?, token, retention, signals).await
    })
}
```

`read` joins every path's `Arc<Layer>` read through `Validated`, and `Defaults { .. }` keeps the `info` filter. `command::run` holds each process effect around the work:
- `get_matches_mut` under `subcommand_required(false)` runs an `exclusive` process flag (`--manual`) before a required subcommand is checked
- JSON lines write to `(move || OptionalWriter::from(journal.clone())).or_else(stderr)`, a daily file under a log directory or the passed writer
- Failures before the subscriber starts are crate error variants (`Journal { directory, source }`), printed through `Command::error(..).print()`
- `.print().map_or_else(sysexits::ExitCode::from, |()| error.into()).into()` exits with the variant's status or the print failure's
- Startup failures report through `dispatcher::with_default(&early, ..)`, a scoped `WARN` dispatch ahead of the global subscriber
- `DropGuard::new(Runtime::new()?, Runtime::shutdown_background)` holds the runtime `block_on` reads, every path returning without waiting for blocked tasks
- `panic::update_hook` ahead of `ratatui::try_init` restores terminal state after the init's restore and before the previous hook
- `panic::set_hook` under `DropGuard::new(panic::take_hook(), panic::set_hook)` force-flushes each provider, and `panic = "abort"` runs no other flush
- Hooks record `info.payload_as_str()` and `info.location()`, a `%info` field renders both as text and layer `code.*` fields name the hook
- `DropGuard` over the providers shuts them down on unwind, and `DropGuard::dismiss` returns them once the work commits for a reported shutdown
- Work returning `T: Termination` exits through `report()`

Stdout holds output another program reads through `Serialize`, and output a person reads renders through `Display`. Writer, encoder, and pager failures fold into one `Option<io::ErrorKind>`, and a `BrokenPipe` from a closed reader ends output as success. `AutoStream::choice(&stream)` decides color for a consumer writing outside `anstream` (`enforce_styling`, `Styles::plain`).
