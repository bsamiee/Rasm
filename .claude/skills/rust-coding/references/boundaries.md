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

`Window` round-trips `{"start": .., "end": ..}` and rejects an unknown key or an end at or before its start. `Deref` reaches `Range` members (`window.contains(&now)`). Domain enums spelled as flat wire fields decode through `#[serde(try_from = "<Wire>")]`. Records re-encoded with one field replaced take it as a type parameter, decode `IgnoredAny` there, and rebuild through `Record { field, ..decoded }`.

Decode errors name the failing path, held by `toml` errors and `axum::Json` rejections. `serde_json::Value` stays inside a decoder. Programs driving a `serde_json::Deserializer` decode through `serde_path_to_error::deserialize`, then reject trailing input with `Deserializer::end`. A primitive, `str`, or `String` holds no field path and reads through `Value::as_str` or another `as_*`. Lengths a peer sends size a buffer through `Vec::try_with_capacity`, refusing as `io::ErrorKind::OutOfMemory` in place of aborting.

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
|  [03]   | Bytes as base64 or hex text        | `#[serde_as(as = "Base64")]`, `Hex`                  |
|  [04]   | Instant as epoch seconds           | `#[serde_as(as = "TimestampSeconds<i64>")]`          |
|  [05]   | One value or a list                | `#[serde_as(as = "OneOrMany<_>")]`                   |
|  [06]   | Map rejecting a repeated key       | `#[serde_as(as = "MapPreventDuplicates<_, _>")]`     |
|  [07]   | List dropping a malformed item     | `#[serde_as(as = "VecSkipError<_>")]`                |
|  [08]   | Absent key for each `None`         | `#[skip_serializing_none]` on the type               |
|  [09]   | JSON forwarded unread              | Borrowed `&RawValue` or owned `Box<RawValue>`        |
|  [10]   | Undeclared keys kept               | `#[serde(flatten)] rest: Map<String, Value>`         |
|  [11]   | Undeclared keys rejected           | `#[serde(deny_unknown_fields)]`                      |
|  [12]   | Foreign type's own byte codec      | Marker with `SerializeAs` over `Bytes`               |

`#[serde_as]` and `#[skip_serializing_none]` precede the derive; one `#[serde_as]` enables field conversions throughout a type. Adapters compose inside containers (`Vec<DisplayFromStr>`, `Option<Base64>`), and an absent optional key decodes as `None`. Derived `JsonSchema` reads the adapter's wire form. `TimestampSeconds` accepts jiff `Timestamp` or `Zoned`, and `DurationSeconds` accepts `SignedDuration`.

Plain map decoding keeps the last repeated key; `MapPreventDuplicates` enforces a wire contract requiring unique keys. Closed contracts reject undeclared keys, while extension contracts can retain them in flattened `rest`. Serde does not support `deny_unknown_fields` with `flatten` on either record. `VecSkipError<_, I>` reports each discarded item's error to an `InspectError` implementor.

Foreign types with a byte form no adapter covers take one marker implementing both directions over `Bytes`:

```rust
#[derive(Debug)]
pub struct Key;

impl<K> SerializeAs<TypedUuid<K>> for Key
where K: TypedUuidKind
{
    fn serialize_as<S>(source: &TypedUuid<K>, serializer: S) -> Result<S::Ok, S::Error>
    where S: Serializer {
        Bytes::serialize_as(source.as_bytes(), serializer)
    }
}

impl<'de, K> DeserializeAs<'de, TypedUuid<K>> for Key
where K: TypedUuidKind
{
    fn deserialize_as<D>(deserializer: D) -> Result<TypedUuid<K>, D::Error>
    where D: Deserializer<'de> {
        Bytes::deserialize_as(deserializer).map(TypedUuid::from_bytes)
    }
}
```

Fields declare `#[serde_as(as = "Key")]` in place of a `serialize_with` and `deserialize_with` pair each.

Raw fields decode directly through the JSON tokenizer: `#[serde(borrow)] &'a RawValue` borrows a string or slice, while `Box<RawValue>` owns reader input or data outliving its source. Buffering through Serde `Content` or `Value` loses raw-source capture and formatting; a named raw field beside flattened siblings can still decode directly. Borrowed fields retain their input owner, making a copy useful when a small retained value otherwise holds a large input alive.

Protocol codecs can use an existing transport when replacing its client would lose authentication, headers, TLS, proxy policy, or connection reuse. Fallible serialization can stage an item before committing destination writes, preserving refusal and partial-write contracts.

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

Each setting has one source: `clap` decodes invocation values (settings paths, secrets, process flags) from arguments and environment into the type each consumer reads, a `value_parser` refusing what its `FromStr` accepts and the consumer refuses (an authority without a port), settings files the rest:

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

Flags and variables derive from field names (`--token`, `TOKEN`); package or specification names use their constant or declared literal (`EnvFilter::DEFAULT_ENV`, `OTEL_PROPAGATORS`). Repeated `--settings` files layer over earlier files and field defaults. `SecretString` redacts `Debug`, with `expose_secret` at the sending call. Choice enums derive `ValueEnum`, per-variant `#[value(help)]`, and `#[default]` beside `default_value_t`. Repeatable paths omit `value_delimiter` so separators remain part of each path.

Shared flags (logs, man pages, color, telemetry with `RUST_LOG` and `OTEL_*`) belong to a record generic over application `Args`. Native `CommandFactory`, `FromArgMatches`, and `Parser` compose that record. `Resettable::from` restores application `about` and `long_about`; flattened package records clear both through `#[command(about = None, long_about = None)]`. Default `_mut` methods delegate to their borrowed pair, cloning each derived part; composed records can implement all four methods with one clone into the consuming pair. Builder commands read metadata from `clap::command!()`.

Child commands keep executable and child tokens in one trailing `Vec<OsString>` with required arity, `num_args = 1..`, `trailing_var_arg`, `allow_hyphen_values`, and `ValueHint::CommandWithArguments`. Splitting executable and arguments into separate positionals lets known parent flags outrank the next positional. Derive parsing when it expresses the complete contract; supported typed `CommandFactory` and `FromArgMatches` conversion remains available otherwise. Slice pattern `[program, args @ ..]` separates the captured command after native arity enforcement.

Flattened records use `#[group(skip)]` and `#[command(next_help_heading = ..)]`; reused type names need distinct groups and headings. Optional flattening retains the group that determines presence and keeps required fields required. Values requiring another flag use `requires = "<id>"`. Application-read variables become arguments; dependency-read variables (`OTEL_EXPORTER_OTLP_ENDPOINT`) pass through untouched. Settings declare field defaults, and one generic function decodes layered files:

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
    fn merged<'text>(lower: DeTable<'text>, upper: DeTable<'text>) -> DeTable<'text> {
        upper.into_iter().fold(lower, |mut table, (key, entry)| {
            let span = entry.span();
            match (table.get_mut(&key).map(Spanned::get_mut), entry.into_inner()) {
                (Some(DeValue::Table(earlier)), DeValue::Table(later)) => *earlier = merged(mem::take(earlier), later),
                (_, later) => {
                    table.insert(key, Spanned::new(span, later));
                }
            }
            table
        })
    }
    let tables = layers
        .iter()
        .map(|layer| {
            let (table, errors) = DeTable::parse_recoverable(&layer.text);
            NEVec::try_from_vec(errors).map_or(Validated::Good(table.into_inner()), |failures| {
                Validated::Fail(failures.into_nonempty_iter().map(|mut source| {
                    source.set_input(None);
                    ServiceError::Syntax { origin: Origin { layer: Arc::clone(layer), span: source.span() }, source: Box::new(source) }
                }).collect())
            })
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()?;
    serde_path_to_error::deserialize(Deserializer::from(Spanned::new(0..0, tables.into_iter().fold(DeTable::new(), merged))))
        .map_err(|error| NEVec::new(ServiceError::Settings { origin: origin(layers, error.path()), source: Box::new(error) }))
}

#[must_use]
pub fn origin(layers: &[Arc<Layer>], path: &Path) -> Option<Origin> {
    layers
        .iter()
        .filter_map(|layer| {
            let (table, _) = DeTable::parse_recoverable(&layer.text);
            let root = Spanned::new(table.span(), DeValue::Table(table.into_inner()));
            let spans = path.iter().scan(&root, |value, segment| {
                *value = match segment {
                    Segment::Map { key } | Segment::Enum { variant: key } => value.get_ref().get(key),
                    Segment::Seq { index } => value.get_ref().get(*index),
                    Segment::Unknown => None,
                }?;
                Some(value.span())
            });
            spans.enumerate().last().map(|deepest| (layer, deepest))
        })
        .max_by_key(|(_, (depth, _))| *depth)
        .map(|(layer, (_, span))| Origin { layer: Arc::clone(layer), span: Some(span) })
}
```

`layered::<Settings>(&layers)` decodes `timeout = "30s"` through `SignedDuration`'s friendly format and every value into its domain type. `ServiceError` is the crate error, a `Layer` holds one file's `path` and `text`, and an `Origin` holds an `Arc<Layer>` and `span: Option<Range<usize>>`. `parse_recoverable` reports every syntax error of every layer, and `set_input(None)` drops each error's input copy, its excerpt rendering from the layer through `annotate_snippets::Renderer::plain` under `Level::ERROR.no_name()`, and a `None` span renders the file path alone through `annotate_snippets::Origin::path`. Later layers replace values and merge tables key by key in place, an overridden key keeping its position under `preserve_order`. Tables move into the merge, and `origin` re-parses the layers once a decode or later validation fails at a `serde_path_to_error::Path`, returning the layer holding its deepest prefix (the later one on a tie), and a nested key no layer sets points at the table missing it. `Syntax` holds an `Origin`, and `Settings` an `Option<Origin>` that is `None` for a path no layer holds a prefix of (a missing top-level key, an empty stack).

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
- `CompleteEnv::with_factory(<Record>::command).complete()` runs before the parse over the one `fn` item building every later command
- `Parser::try_parse` parses once, exiting `Usage` on an error or `Ok` on stdout where `use_stderr()` is false (help, version)
- Failed parses read `--manual` through `ignore_errors(true)`, an `exclusive` flag conflicting with any set `env` variable
- JSON lines write to `(move || OptionalWriter::from(appender.clone())).or_else(stderr)`, a daily file under a log directory or the passed writer
- Writers a subscriber shares with other code pass as `Arc<W>` beside `impl Write for &W`, tracing-subscriber's `MakeWriter for Arc<W>` cloning nothing per write
- Failures before the subscriber starts are crate error variants (`Appender { directory, source }`), printed through `clap::Error::raw(<kind>, ..).with_cmd(&command)`
- `.print().map_or_else(sysexits::ExitCode::from, |()| error.into()).into()` exits with the variant's status or the print failure's
- Startup failures report through `subscriber::with_default(<subscriber>, ..)`, a scoped `WARN` subscriber ahead of the global one with no `Dispatch` built
- `DropGuard::new(Runtime::new()?, Runtime::shutdown_background)` holds the runtime `block_on` reads, every path returning without waiting for blocked tasks
- `panic::set_hook` force-flushes each provider, then calls the previous hook, `panic::take_hook()` held as an `Arc<dyn Fn(&PanicHookInfo) + Send + Sync>` the guard restores
- Hooks record `info.payload_as_str()` as `EXCEPTION_MESSAGE` and `info.location()` as `CODE_FILE_PATH` and `CODE_LINE_NUMBER` fields
- Provider guards shut down on unwind without replacing hooks; normal exit restores the previous hook before provider shutdown
- `panic = "abort"` runs no unwind release, and a hook held for process lifetime chains through `panic::update_hook` to drop the `WorkerGuard` an `Arc<nonpoison::Mutex<Option<WorkerGuard>>>` holds
- `clap_mangen::generate_to` writes man pages into a directory `create_dir_all` made
- Work returning `T: Termination` exits through `report()`

One release serves suspend, the panic hook, and exit:

```rust
pub fn released(modes: &[Mode]) -> Result<(), NEVec<io::Error>> {
    let mut out = io::stdout();
    let restored = try {
        #[cfg(unix)]
        queue!(out, PopKeyboardEnhancementFlags)?;
        modes.iter().try_for_each(|mode| match mode {
            Mode::Mouse => queue!(out, DisableMouseCapture),
            Mode::Focus => queue!(out, DisableFocusChange),
        })?;
        execute!(out, DisableBracketedPaste, Show, LeaveAlternateScreen)?
    };
    Validated::from(restored).map2(Validated::from(terminal::disable_raw_mode()), |(), ()| ()).ok()
}

#[cfg(unix)]
pub fn suspended(terminal: &mut DefaultTerminal, modes: &[Mode]) -> Result<(), NEVec<io::Error>> {
    released(modes).and_then(|()| {
        (try bikeshed io::Result<()> {
            kill_current_process_group(Signal::TSTP)?;
            entered(modes)?;
            terminal.clear()?
        })
        .map_err(NEVec::new)
    })
}
```

Escapes leave the alternate screen before raw mode ends. Writers gated on raw mode resume stderr once it ends, and lines written while the alternate screen is up vanish with it. Raw mode refusals leave screen restores applied, where `ratatui::try_restore` stops at its first `?` with the alternate screen up. `entered` reapplies the modes `released` undoes.

Stdout holds output another program reads through `Serialize`, and output a person reads renders through `Display`. Encoder failures classify where they enter, an I/O cause converting into the one variant over `io::Error` (serde_json `classify() == Category::Io` through `io::Error::from`, csv `into_kind()` yielding `ErrorKind::Io`), and the exit match holds one arm per variant, a `BrokenPipe` from a closed reader ending output as success. Streamed encoders keep a refused item out of their state and output:

```rust
#[derive(Debug)]
pub struct JsonArray<W> {
    out: W,
    scratch: Vec<u8>,
    formatter: PrettyFormatter<'static>,
    first: bool,
}

impl<W> JsonArray<W>
where W: io::Write
{
    pub fn opened(out: W) -> io::Result<Self> {
        let (mut scratch, mut formatter) = (Vec::new(), PrettyFormatter::new());
        formatter.begin_array(&mut scratch).map(|()| Self { out, scratch, formatter, first: true })
    }

    pub fn appended<T>(&mut self, value: &T) -> io::Result<Result<(), serde_json::Error>>
    where T: Serialize {
        let (mark, mut next) = (self.scratch.len(), self.formatter.clone());
        let encoded = try {
            next.begin_array_value(&mut self.scratch, self.first).map_err(serde_json::Error::io)?;
            value.serialize(&mut Serializer::with_formatter(&mut self.scratch, next.clone()))?;
            next.end_array_value(&mut self.scratch).map_err(serde_json::Error::io)?
        };
        match encoded {
            Ok(()) => {
                (self.formatter, self.first) = (next, false);
                let lines = self.scratch.iter().rposition(|byte| *byte == b'\n').map_or_default(|end| end.strict_add(1));
                self.out.write_all(self.scratch.drain(..lines).as_slice()).map(Ok)
            }
            Err(source) => {
                self.scratch.truncate(mark);
                Ok(Err(source))
            }
        }
    }

    pub fn closed(mut self) -> io::Result<()> {
        self.formatter.end_array(&mut self.scratch)?;
        self.scratch.push(b'\n');
        self.out.write_all(&self.scratch)
    }
}
```

`opened` writes `begin_array` once, and an empty run closes as `[]`. Write failures ending the stream sit outside each item's refusal, and callers fold refusals per row with no variant test sorting out the fatal one. `Color::write_global` sets the one color choice every `anstream` writer reads where its consumer writes, no public record holding a standard stream, and `AutoStream::choice(&stream)` decides color for a consumer writing outside `anstream` (`enforce_styling`, `Styles::plain`).
