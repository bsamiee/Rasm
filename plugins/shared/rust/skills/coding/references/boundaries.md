# [BOUNDARIES]

## [01]-[DECODE]

Untrusted input becomes domain values once, and each refusal names its field's path:

```rust
#[derive(Debug)]
pub struct Object<T>(PhantomData<T>);

impl<'de, T> Visitor<'de> for Object<T>
where T: Deserialize<'de>
{
    type Value = T;

    fn expecting(&self, formatter: &mut fmt::Formatter<'_>) -> fmt::Result { formatter.write_str("an object of named fields") }

    fn visit_map<A>(self, map: A) -> Result<T, A::Error>
    where A: MapAccess<'de> {
        T::deserialize(MapAccessDeserializer::new(map))
    }
}

impl<'de, T> DeserializeAs<'de, T> for Object<T>
where T: Deserialize<'de>
{
    fn deserialize_as<D>(deserializer: D) -> Result<T, D::Error>
    where D: Deserializer<'de> {
        deserializer.deserialize_map(Self(PhantomData))
    }
}

pub fn decoded<'de, T>(text: &'de [u8]) -> Result<T, Error<serde_json::Error>>
where T: Deserialize<'de> {
    let mut json = serde_json::Deserializer::from_slice(text);
    let mut track = Track::new();
    let tracked = serde_path_to_error::Deserializer::new(&mut json, &mut track);
    let value = Object::<T>::deserialize_as(tracked).and_then(|value| json.end().map(|()| value));
    value.map_err(|source| Error::new(track.path(), source))
}
```

Derived `Deserialize` accepts a record as a JSON list of its fields in order, and `Object` reads a map alone (`["a", []]` is refused):
- Fields holding a record take `#[serde_as(as = "Object<Line>")]`, a list of records `Vec<Object<Line>>`
- `Deserializer::end` refuses text after the document, which `serde_path_to_error::deserialize` alone accepts
- Documents decode into their record, and `serde_json::Value` stays inside a decoder in place of keys read through `Value::pointer`

Forms serde reads in one pass keep the path, and forms it buffers report an enclosing position:

| [INDEX] | [FORM]                                                  | [REFUSAL_PATH]                                                  |
| :-----: | :------------------------------------------------------ | :-------------------------------------------------------------- |
|  [01]   | Record field, list index, map key of text or number     | Whole path (`lines[0].sku`)                                     |
|  [02]   | Map key decoded through `FromStr`                       | Whole path                                                      |
|  [03]   | Map key of an enum type                                 | `?` in place of key                                             |
|  [04]   | Externally tagged enum, serde's default                 | Whole path through variant (`change.renamed.name`)              |
|  [05]   | Adjacently tagged enum, tag key ahead of content        | Whole path through content key                                  |
|  [06]   | Adjacently tagged enum, content key first               | Enum's field                                                    |
|  [07]   | Internally tagged enum (`tag = "type"`)                 | Enum's field                                                    |
|  [08]   | `untagged` enum, or an enum with one `untagged` variant | Enum's field, each variant's refusal lost                       |
|  [09]   | Field of a `flatten` member                             | Record holding member                                           |
|  [10]   | `#[serde(try_from = "Raw")]`                            | Whole path inside `Raw`, conversion refusal at field holding it |

`nutype`'s derived `Deserialize` appends `Expected valid <Name>` to a refusal, and a text type keeping its own error decodes through `FromStr`:

```rust
#[nutype(validate(with = labelled, error = LabelError), derive(Debug, Clone, PartialEq, Eq, FromStr), derive_unchecked(DeserializeFromStr))]
pub struct Label(String);
```

`labelled` returns `Result<(), LabelError>`, and a refused label decodes to `LabelError`'s message alone at its field's path:
- `derive_unchecked` takes nutype's `derive_unchecked` feature, and `DeserializeFromStr` comes from `serde_with`
- `DeserializeFromStr` reads a JSON string alone, and a numeric newtype keeps nutype's `Deserialize`

Domain types whose wire shape differs decode through a derived record of that shape, and the constructor goes in `TryFrom`:

```rust
#[derive(Debug, Deserialize)]
#[serde(try_from = "Bounds")]
pub struct Band {
    low: u16,
    high: u16,
}

impl TryFrom<Bounds> for Band {
    type Error = Inverted;

    fn try_from(Bounds { low, high }: Bounds) -> Result<Self, Inverted> { (low < high).then_some(Self { low, high }).ok_or(Inverted { low, high }) }
}
```

`Bounds` derives `Deserialize` over `low` and `high`, and `Inverted` is the crate error naming both.

Records borrow text from a buffer that outlives them:
- `&'a str` refuses a JSON string holding an escape (`\n`, `\"`), and `#[serde(borrow)] Cow<'a, str>` borrows unescaped text and owns the rest
- `#[serde(borrow)]` marks a `Cow` field and a field of a record holding a lifetime, a `Cow` without it owning every value

## [02]-[TAGGED_UNIONS]

Enums crossing a boundary keep serde's external tag, the one tag form keeping every path, and shared fields go in a record holding the enum:

```rust
#[derive(Debug, Clone, PartialEq, Eq, Deserialize)]
#[serde(deny_unknown_fields)]
pub struct Event {
    pub item: Label,
    pub at: Timestamp,
    pub change: Change,
}

#[derive(Debug, Clone, PartialEq, Eq, Deserialize)]
#[serde(rename_all = "snake_case", deny_unknown_fields)]
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

`{"item": "a", "at": "<time>", "change": {"renamed": {"name": "b"}}}` decodes into one `Event`, and a refused name reports `change.renamed.name`:
- Unit variants are a string (`"change": "created"`), and a variant with fields is one key holding them
- `#[serde(other)]` on a unit variant takes a string no variant names, and an unknown key holding fields stays refused
- Closed protocols omit `#[serde(other)]`, refusing an unknown string at decode
- Tag forms another party fixes take serde's matching attribute
- Enums holding an `untagged` variant report a refused known variant as `data did not match any variant`
- Types a program encodes and never decodes take the tag form their consumer reads, path loss being a cost of decode alone
- Encoded outcomes are an enum of their cases in place of `Result`, which encodes as `{"Ok": ..}`

## [03]-[FIELD_CONVERSIONS]

Encodings belong to a field, and the field keeps its domain type. Adapters go in `#[serde_as(as = "<adapter>")]` under one `#[serde_as]` on the type:

| [INDEX] | [WIRE]                             | [DECLARATION]                                     |
| :-----: | :--------------------------------- | :------------------------------------------------ |
|  [01]   | Text a type parses and renders     | `DisplayFromStr`                                  |
|  [02]   | Number or its text, number written | `PickFirst<(_, DisplayFromStr)>`                  |
|  [03]   | Bytes as base64 or hex text        | `Base64`, `Hex`                                   |
|  [04]   | Instant or duration in seconds     | `TimestampSeconds<i64>`, `DurationSeconds<i64>`   |
|  [05]   | One value or a list                | `OneOrMany<_>`                                    |
|  [06]   | Map refusing a repeated key        | `MapPreventDuplicates<_, _>`                      |
|  [07]   | Absent key for each `None`         | `#[skip_serializing_none]` on the type            |
|  [08]   | JSON forwarded unread              | `#[serde(borrow)] &'a RawValue`, `Box<RawValue>`  |
|  [09]   | Undeclared keys kept               | `#[serde(flatten)] rest: BTreeMap<String, Value>` |
|  [10]   | Undeclared keys refused            | `#[serde(deny_unknown_fields)]`                   |

`#[serde_as]` and `#[skip_serializing_none]` precede the derive, and adapters nest inside containers (`Vec<DisplayFromStr>`, `Option<Base64>`):
- `Option` adapters decode an absent key as `None`
- Plain maps keep the last of equal keys, and `MapPreventDuplicates` refuses a second at the map's path
- `deny_unknown_fields` beside `flatten` refuses a key no member reads, naming its record in place of the key
- `Base64` and `Hex` take serde_with's `base64` and `hex` features, jiff types its `jiff_1` feature, and `RawValue` serde_json's `raw_value` feature
- Foreign types no adapter covers take one marker implementing `SerializeAs` and `DeserializeAs` in place of a `with` function per field

## [04]-[BINARY]

One `binrw` declaration decodes and encodes a binary format, with byte order and magic on the type and each domain field built by its constructor:

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

`Quantity` is a `nutype` over `u32`, and `Frame::read(&mut Cursor::new(bytes))` names the stream position of a bad magic or a refused quantity:
- Length prefixes are `temp` on read and computed from the collection on write, `#[binrw]` preceding the derives to remove each `temp` field
- `count` grows a list of wider items as they arrive, `Vec<u8>` reserves the whole count at once, and a count past the stream's end fails there
- `try_map` and `try_calc` box their error through `as _`, and a crate denying `clippy::as_underscore` expects it on the module, never the type
- Hand-written readers take a peer's length through `reader.take(length).read_to_end(&mut buffer)`, which grows as bytes arrive
- `read_to_end` over `take` returns fewer bytes than `length` at the stream's end, and the reader refuses that count as a truncation

## [05]-[CONFIGURATION]

Each setting has one source, `clap` decoding arguments and environment variables into the type each consumer reads and settings files the rest:

```rust
#[derive(Debug, Parser)]
#[command(version, about)]
pub struct Args {
    #[arg(long)]
    pub settings: Vec<PathBuf>,
    #[arg(long, env, hide_env_values = true)]
    pub token: SecretString,
    #[arg(long, value_enum, default_value_t)]
    pub retention: Retention,
    #[arg(required = true, num_args = 1.., trailing_var_arg = true, allow_hyphen_values = true)]
    pub command: Vec<OsString>,
}
```

`Retention` derives `ValueEnum` and `Default`, and `TOKEN=<token> <program> --settings a.toml run -x` decodes every field:
- Flags and variables take the field's name (`--token`, `TOKEN`), and a dependency's variable takes its constant (`env = EnvFilter::DEFAULT_ENV`)
- Help states a field's purpose alone, as clap prints each default from `default_value_t`
- `hide_env_values` keeps a secret out of `--help`, `SecretString` redacts `Debug`, and `expose_secret` reads it at the call sending it
- `value_parser` refuses at parse a value the type's `FromStr` accepts and its consumer refuses (an address without a port)
- Repeatable paths omit `value_delimiter`, a comma staying part of a path
- Child command lines are one trailing `Vec<OsString>`, and every word after the first positional reaches the child, flags included
- Variables a dependency reads itself (`OTEL_EXPORTER_OTLP_ENDPOINT`) stay out of `Args` and reach it untouched

Settings files layer over field defaults, a later file replacing a value and merging a table by key, and one decode of the merge names a refused key:

```rust
#[derive(Debug, Clone, PartialEq, Eq, Default, Deserialize)]
#[serde(default, deny_unknown_fields)]
pub struct Settings {
    pub listen: SocketAddr = SocketAddr::new(IpAddr::V4(Ipv4Addr::LOCALHOST), 8080),
    pub timeout: SignedDuration = SignedDuration::from_secs(30),
}

#[must_use]
pub fn layered(lower: Table, upper: Table) -> Table {
    upper.into_iter().fold(lower, |mut table, (key, above)| {
        let merged = match (table.remove(&key), above) {
            (Some(Value::Table(below)), Value::Table(above)) => Value::Table(layered(below, above)),
            (_, above) => above,
        };
        table.insert(key, merged);
        table
    })
}

pub fn settings(tables: Vec<Table>) -> Result<Settings, serde_path_to_error::Error<toml::de::Error>> {
    let merged = tables.into_iter().fold(Table::new(), layered);
    serde_path_to_error::deserialize(merged)
}
```

Each file parses through `toml::from_str::<Table>`, and a syntax error names its line and column:
- `settings` over no file returns each field's default
- `timeout = "5m"` decodes through `SignedDuration`'s own format, and `timeout = 5` reports `timeout`

## [06]-[ENTRY_POINT]

`main` parses arguments, builds a runtime, runs the composed effect once, and reports its failure:

```rust
fn main() -> ExitCode {
    let args = Args::parse();
    let runtime = Builder::new_current_thread().enable_all().build().map_err(Failure::Runtime);
    let ran = runtime.and_then(|runtime| runtime.block_on(run(args)));
    ran.map_or_else(reported, |()| ExitCode::Ok)
}

fn reported(error: Failure) -> ExitCode {
    let printed = writeln!(io::stderr().lock(), "{}", Report::new(&error));
    printed.map_or_else(ExitCode::from, |()| error.into())
}
```

`ExitCode` is `sysexits::ExitCode`, `run` returns `Result<(), Failure>`, and `Failure` is the crate error converting into `ExitCode`:
- `Args::parse` exits 2 on a usage error and 0 after `--help` or `--version`, before any thread starts
- `Builder` builds the runtime from parsed arguments, where `#[tokio::main]` starts one before the parse
- Failed report writes exit with their own status through `sysexits`'s `From<io::Error>`
- Raw mode and alternate screen end in a `Drop` guard and in a panic hook, the hook alone running under `panic = "abort"`
- `panic::update_hook(|previous, info| { <release>; previous(info) })` (`panic_update_hook`) runs `<release>` ahead of the previous hook

## [07]-[OUTPUT]

Stdout holds what another program reads through `Serialize`, text a person reads renders through `Display`, and records stream as JSON Lines:

```rust
pub fn written<W, T>(out: W, records: impl IntoIterator<Item = T>) -> io::Result<()>
where
    W: Write,
    T: Serialize,
{
    let mut out = BufWriter::new(out);
    let lines = records.into_iter().try_for_each(|record| {
        let mut line = serde_json::to_vec(&record)?;
        line.push(b'\n');
        out.write_all(&line)
    });
    match lines.and_then(|()| out.flush()) {
        Err(closed) if closed.kind() == io::ErrorKind::BrokenPipe => Ok(()),
        written => written,
    }
}
```

`written(io::stdout().lock(), records)` writes one line per record, and a reader closing the pipe ends output as success:
- `print!`, `println!`, and `eprintln!` panic once a reader closes the stream, and `writeln!` on a locked handle returns `io::ErrorKind::BrokenPipe`
- `serde_json::to_writer` writes a refused record's prefix before it fails, and `to_vec` before `write_all` writes each record whole or not at all
- `Stdout` flushes at each newline even into a pipe, and bulk output wraps its lock in `BufWriter` and calls `flush`, a dropped one losing its error
- `serde_json::Error` converts through `io::Error::from`, an I/O cause returning its own `io::Error` and a refused value `InvalidData`

## [08]-[FILES]

Files replaced whole are written beside their target and renamed over it, and a reader sees one whole file, old or new:

```rust
pub fn replaced(path: &Path, bytes: &[u8]) -> io::Result<()> {
    let folder = path.parent().ok_or(io::ErrorKind::InvalidInput)?;
    let mut staged = NamedTempFile::new_in(folder)?;
    staged.write_all(bytes)?;
    staged.as_file().sync_all()?;
    staged.persist(path)?;
    cfg_select! {
        unix => fs_err::File::open(folder)?.sync_all(),
        _ => Ok(()),
    }
}
```

`new_in(folder)` keeps the rename on one filesystem, and a target names its folder (`./a.json`), `a.json` holding an empty parent `new_in` refuses:
- `sync_all` before `persist` keeps a crash from leaving an empty file under the target's name
- Folder syncs keep the rename on Unix, and `File::open` refuses a folder on Windows
- `fs_err` names the path in each `io::Error`, where `std::fs` names none
- Temporary files hold mode `0o600`, and a target others read takes `as_file().set_permissions` before `persist`
- `persist_noclobber` refuses an existing target, and `PersistError` returns its file beside its `io::Error` for a retry, `?` keeping the error alone
- Async callers run the whole replacement in one `spawn_blocking` closure

Locks belong to the open file that took them, and `File::try_lock` reports another holder as `TryLockError::WouldBlock`:

```rust
pub fn locked(path: &Path) -> io::Result<Option<fs_err::File>> {
    let file = fs_err::OpenOptions::new().write(true).create(true).truncate(false).open(path)?;
    match file.try_lock() {
        Ok(()) => Ok(Some(file)),
        Err(TryLockError::WouldBlock) => Ok(None),
        Err(TryLockError::Error(source)) => Err(source),
    }
}
```

Dropping a file releases its lock, and `truncate(false)` keeps a second opener from emptying a file the holder wrote:
- Each open `File` holds its own lock, and a second open in one process meets `WouldBlock` as another process does
- `lock` blocks until its holder releases, and async code runs it in `spawn_blocking` or retries `try_lock` after a sleep
- `lock_shared` and `try_lock_shared` let readers share a lock no writer holds
- Locks are advisory on Unix (`flock`), and a process opening the file without a lock reads and writes it freely
