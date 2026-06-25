# [csvkit]

`csvkit` (2.2.0, MIT, pure-Python wheel `py3-none-any`, no ABI floor; runs on cp315) is a fourteen-tool tabular CSV suite over the `agate` data engine. Every tool is both a `console_scripts` executable (`in2csv`, `csvcut`, `csvgrep`, `csvjoin`, `csvsort`, `csvstack`, `csvjson`, `csvsql`, `sql2csv`, `csvstat`, `csvlook`, `csvclean`, `csvformat`, `csvpy`) AND an importable `csvkit.utilities.<name>` module exporting a `CSVKitUtility` subclass plus a `launch_new_instance` entry. The class face owns the CLI argument grammar through one shared `_init_common_parser` base, reads agate-inferred column types (`TypeTester` over `agate.Text`/`agate.Number`/`agate.Date`/`agate.Boolean`), transparently decompresses `.gz`/`.bz2`/`.xz`/`.zst` input via a `LazyFile`, and writes to an injectable `output_file`. The suite has two distinct boundary modes: a STREAM-PIPE mode where each tool is launched as an `anyio.run_process` subprocess composing a Unix CSV pipeline (`in2csv | csvgrep | csvsort | csvjson`), and an IN-PROCESS mode where a `CSVKitUtility(args=[...], output_file=buf).run()` runs inside the event loop's worker thread with a captured buffer. It is the single tabular-CSV-transform owner; no hand-rolled CSV dialect sniffing, JSON/GeoJSON emit, SQL-load loop, or format-conversion shim survives where a csvkit tool already owns it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `csvkit`
- package: `csvkit`
- module: `csvkit`
- version: `2.2.0`
- license: MIT
- wheel: pure-Python (`py3-none-any`), no native ABI floor; runs on cp315
- import: `csvkit` (legacy agate-CSV shim), `csvkit.cli` (utility base + library helpers), `csvkit.utilities.<tool>` (one module per tool)
- owner: `data`
- rail: tabular-csv
- requires-python: resolved and installed in the cp315 default venv (pure-Python; no compiled extension)
- depends-on: `agate` (1.14.2 — the Table/CSV/type-inference engine; csvkit composes its readers, `Table.from_csv`/`to_json`/`to_csv`, and `TypeTester`), `agate-excel` (0.4.2 — `xls`/`xlsx` ingestion via `openpyxl`/`xlrd`), `agate-sql` (0.7.3 — SQLAlchemy round-trip backing `csvsql`/`sql2csv`), `agate-dbf` (0.2.4 — `dbf` ingestion), `sqlalchemy` (2.0.51), `openpyxl`, `xlrd`
- console_scripts: 14 entries, each `csvkit.utilities.<tool>:launch_new_instance` — the subprocess boundary names
- namespaces: `csvkit` (top-level agate re-export), `csvkit.cli` (`CSVKitUtility` base + parsing/codec helpers + error rails), `csvkit.utilities` (the 14 tool modules), `csvkit.convert` (`guess_format`), `csvkit.cleanup` (`RowChecker`/`join_rows`), `csvkit.grep` (`FilteringCSVReader`), `csvkit.exceptions` (typed error rails)
- capability: format conversion into CSV (`csv`/`dbf`/`fixed`/`geojson`/`json`/`ndjson`/`xls`/`xlsx`), column selection/grep/sort/join/stack, JSON/GeoJSON/NDJSON emit, SQL table creation + bulk load + query round-trip, typed per-column statistics, fixed-width/dialect reformatting, structural cleaning, and an interactive agate `Table` REPL handoff — all over one shared dialect/encoding/null/locale argument grammar
- version-note: `csvkit` exposes no `__version__` attribute; resolve via `importlib.metadata.version("csvkit")`. The top-level `csvkit` module is a documented legacy shim (`reader`/`writer`/`DictReader`/`DictWriter` = `agate.csv.*`); new in-process code imports `agate` directly for the engine and `csvkit.cli`/`csvkit.utilities` for the tools.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: utility base class (`csvkit.cli`)
- rail: tabular-csv
- `CSVKitUtility` is the abstract base every tool subclasses. `__init__(args=None, output_file=None, error_file=None)` parses `args` (an argv list, or `sys.argv[1:]` when `None`) through the shared common parser plus the subclass's `add_arguments`; `output_file`/`error_file` default to `sys.stdout`/`sys.stderr` but are injectable for in-process capture. `run()` opens the input `LazyFile` (honoring `.gz`/`.bz2`/`.xz`/`.zst`), optionally writes a UTF-8 BOM, then calls the subclass `main()`. The base owns dialect/encoding extraction (`_extract_csv_reader_kwargs`/`_extract_csv_writer_kwargs`), agate type inference (`get_column_types`), column-id resolution (`get_column_offset`/`get_rows_and_column_names_and_column_ids`), header listing (`print_column_names`), leading-line skipping (`skip_lines`), STDIN-tty detection (`additional_input_expected`), and a SIGPIPE-tolerant exception handler.

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]    | [RAIL]                                                                          |
| :-----: | :--------------------------------------- | :--------------- | :----------------------------------------------------------------------------- |
|  [01]   | `cli.CSVKitUtility`                       | abstract base    | utility base; `run()` drives `main()`; injectable `output_file`/`error_file`   |
|  [02]   | `cli.LazyFile`                           | file proxy       | deferred-open input wrapper; `.close()`; opens only on first read              |
|  [03]   | `cli.DEFAULT_NULL_VALUES`                | tuple constant   | `('', 'na', 'n/a', 'none', 'null', '.')` — null-coercion vocabulary            |
|  [04]   | `cli.QUOTING_CHOICES`                     | list constant    | `[0,1,2,3,4,5]` — `csv` module quoting modes accepted by `--quoting`           |

[PUBLIC_TYPE_SCOPE]: the fourteen tool classes (`csvkit.utilities.<tool>`)
- rail: tabular-csv
- One `CSVKitUtility` subclass per tool; the class name is its `prog`. Each carries a `description`, `override_flags` (which common flags to suppress — `f` drops the file positional for the STDIN-only `csvpy`), and an `add_arguments` declaring tool-specific flags atop the shared base. `launch_new_instance()` is the `console_scripts` thunk (`<Class>().run()`).

| [INDEX] | [SYMBOL]                          | [TOOL]      | [RAIL]                                                                       |
| :-----: | :-------------------------------- | :---------- | :--------------------------------------------------------------------------- |
|  [01]   | `utilities.in2csv.In2CSV`         | `in2csv`    | convert `dbf`/`fixed`/`geojson`/`json`/`ndjson`/`xls`/`xlsx`/`csv` -> CSV    |
|  [02]   | `utilities.csvcut.CSVCut`         | `csvcut`    | select/reorder/drop columns; `--names` lists headers                        |
|  [03]   | `utilities.csvgrep.CSVGrep`       | `csvgrep`   | row filter by literal/`--regex`/`--match`/`--file` over `--columns`          |
|  [04]   | `utilities.csvsort.CSVSort`       | `csvsort`   | sort rows by `--columns`; `--reverse`/`--ignore-case`                        |
|  [05]   | `utilities.csvjoin.CSVJoin`       | `csvjoin`   | relational join (`--columns`, `--outer`/`--left`/`--right`)                  |
|  [06]   | `utilities.csvstack.CSVStack`     | `csvstack`  | vertical concat of like-schema files; `--groups`/`--filenames`              |
|  [07]   | `utilities.csvjson.CSVJSON`       | `csvjson`   | CSV -> JSON / GeoJSON (`--lat`/`--lon`/`--geometry`/`--crs`) / NDJSON (`--stream`) |
|  [08]   | `utilities.csvsql.CSVSQL`         | `csvsql`    | generate DDL + bulk-load to a SQLAlchemy `--db`, or run `--query`           |
|  [09]   | `utilities.sql2csv.SQL2CSV`       | `sql2csv`   | run SQL against `--db`, stream the result set out as CSV                    |
|  [10]   | `utilities.csvstat.CSVStat`       | `csvstat`   | typed per-column stats (`--mean`/`--median`/`--stdev`/`--freq`/...); `--json` |
|  [11]   | `utilities.csvlook.CSVLook`       | `csvlook`   | render a Markdown-style aligned table; `--max-rows`/`--max-columns`         |
|  [12]   | `utilities.csvclean.CSVClean`     | `csvclean`  | structural validation/repair (`--length-mismatch`, `--join-short-rows`)     |
|  [13]   | `utilities.csvformat.CSVFormat`   | `csvformat` | re-emit with a different dialect (`--out-delimiter`/`--out-quoting`/`--out-asv`) |
|  [14]   | `utilities.csvpy.CSVPy`           | `csvpy`     | drop into a Python shell with `reader`/`table` bound; `--agate`/`--dict`    |

[PUBLIC_TYPE_SCOPE]: library helpers + error rails
- rail: tabular-csv
- These are the importable building blocks the tools compose; use them directly for in-process column-id resolution, format guessing, row-shape repair, and predicate-filtered reading without spawning a subprocess. The error types are the typed failure rail to map at the boundary.

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY]   | [RAIL]                                                                  |
| :-----: | :----------------------------------------- | :-------------- | :--------------------------------------------------------------------- |
|  [01]   | `cli.parse_column_identifiers`             | function        | resolve a `1,name,3-5` spec -> zero-based column index list            |
|  [02]   | `cli.match_column_identifier`              | function        | resolve one identifier (1-based ordinal or name) -> index              |
|  [03]   | `cli.make_default_headers`                 | function        | `n` -> `('a','b',...)` synthetic headers for `--no-header-row`         |
|  [04]   | `convert.guess_format`                     | function        | infer the `in2csv` format key from a filename extension               |
|  [05]   | `cleanup.RowChecker`                       | class           | streaming row-shape validator/repairer backing `csvclean`             |
|  [06]   | `cleanup.join_rows(rows, separator)`       | function        | merge short rows into one (the `--join-short-rows` primitive)         |
|  [07]   | `grep.FilteringCSVReader(reader, patterns, header=True, any_match=False, inverse=False)` | iterator class | predicate-driven row-filtering reader backing `csvgrep` |
|  [08]   | `exceptions.ColumnIdentifierError`         | exception       | a `--columns` spec referenced a missing/invalid column                |
|  [09]   | `exceptions.RequiredHeaderError`           | exception       | a header was required but `--no-header-row` was set                   |
|  [10]   | `exceptions.InvalidValueForTypeException`  | exception       | a cell failed agate type coercion under inference                     |
|  [11]   | `exceptions.CustomException`               | base exception  | csvkit error root carrying a formatted message                        |
|  [12]   | `cleanup.Error`                            | record          | one structural defect row emitted by `RowChecker`                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: subprocess boundary (STREAM-PIPE mode)
- rail: tabular-csv
- The dominant consumption mode: each tool is a `console_scripts` executable invoked through `anyio.run_process([tool, *flags], input=csv_bytes, check=True, cwd=..., env=...) -> CompletedProcess` (`.api/anyio.md`), or chained as a real OS pipeline via `anyio.open_process` connecting `stdout`->`stdin`. The argv head is the tool name; the body is the flag grammar below. CSV flows on stdin/stdout as bytes, so a pipeline (`in2csv data.xlsx | csvgrep -c status -m active | csvsort -c date | csvjson`) keeps the whole transform off-heap and streaming. Tools set `SIGPIPE` to default so `... | head` truncation never raises.

| [INDEX] | [SURFACE]                                                                                   | [ENTRY_FAMILY] | [RAIL]                                                                  |
| :-----: | :------------------------------------------------------------------------------------------ | :------------- | :--------------------------------------------------------------------- |
|  [01]   | `in2csv -f {csv,dbf,fixed,geojson,json,ndjson,xls,xlsx} [--sheet S] [--no-inference] FILE`  | convert        | normalize any supported source into CSV on stdout                      |
|  [02]   | `csvcut -c COLS [--not-columns COLS] [--names] [--delete-empty-rows]`                        | project        | column selection/reorder; `--names` prints the header roster           |
|  [03]   | `csvgrep -c COLS (-m STR \| -r REGEX \| -f FILE) [--invert-match] [--any-match]`              | filter         | row filter; literal/regex/value-file predicate over named columns      |
|  [04]   | `csvsort -c COLS [--reverse] [--ignore-case] [--no-inference]`                               | sort           | type-aware row sort by column spec                                     |
|  [05]   | `csvjoin -c COLS [--outer \| --left \| --right] FILE...`                                      | join           | relational join across files on shared key columns                     |
|  [06]   | `csvstack [--groups G] [--group-name N] [--filenames] FILE...`                               | concat         | stack like-schema files vertically with optional provenance column     |
|  [07]   | `csvjson [--lat C --lon C [--geometry] [--crs S]] [--stream] [--indent N] [--key K]`         | json-emit      | CSV -> JSON; GeoJSON FeatureCollection with lat/lon; NDJSON via `--stream` |
|  [08]   | `csvsql --db URL [--insert] [--no-create] [--overwrite] [--tables T] [--query Q] FILE...`    | sql-load       | DDL generation + bulk insert to any SQLAlchemy DB; or ad-hoc `--query` |
|  [09]   | `sql2csv --db URL (--query Q \| FILE) [--encoding E]`                                         | sql-read       | execute SQL, stream the result set out as CSV                          |
|  [10]   | `csvstat [--json [--indent N]] [--columns C] [--mean \| --freq \| ...] [--no-inference]`      | stat           | typed per-column summary; one statistic flag narrows output           |
|  [11]   | `csvlook [--max-rows N] [--max-columns N] [--max-column-width N] [--max-precision N]`        | render         | aligned Markdown table for human/diff consumption                     |
|  [12]   | `csvclean [--length-mismatch] [--empty-columns] [--enable-all-checks] [--omit-error-rows]`  | clean          | structural validation/repair; error rows split out                    |
|  [13]   | `csvformat [--out-delimiter D] [--out-quoting Q] [--out-asv] [--skip-header]`                | reformat       | re-emit with a target dialect; `--out-asv` uses ASCII unit/record separators |
|  [14]   | `csvpy [--agate \| --dict] [--no-number-ellipsis] FILE`                                       | repl           | interactive shell with `table`/`reader` bound (no STDIN)               |

[ENTRYPOINT_SCOPE]: common argument grammar (shared base — every file-consuming tool)
- rail: tabular-csv
- `_init_common_parser` installs these on all tools (suppressed only where a tool's `override_flags` drops one). They are the canonical dialect/encoding/null/locale knobs; pass them once per pipeline stage. The first positional `FILE` (omitted for `csvpy`, repeated for join/stack) accepts `-`/omission for STDIN.

| [INDEX] | [FLAG]                                          | [DEST]              | [MEANING]                                                              |
| :-----: | :---------------------------------------------- | :------------------ | :-------------------------------------------------------------------- |
|  [01]   | `-d/--delimiter` / `-t/--tabs`                   | `delimiter`/`tabs`  | input field delimiter; `--tabs` is the TSV shorthand                  |
|  [02]   | `-q/--quotechar` / `-u/--quoting`               | `quotechar`/`quoting` | quote char; `--quoting` selects a `QUOTING_CHOICES` mode             |
|  [03]   | `-b/--no-doublequote` / `-p/--escapechar`        | `doublequote`/`escapechar` | escaping discipline for embedded quotes                       |
|  [04]   | `-z/--maxfieldsize`                             | `field_size_limit`  | raise the `csv` field-size ceiling for very wide cells               |
|  [05]   | `-e/--encoding`                                 | `encoding`          | input text encoding (default `utf-8`); drives `LazyFile` reconfigure |
|  [06]   | `-L/--locale`                                   | `locale`            | numeric locale for agate `Number` parsing (grouping/decimal)         |
|  [07]   | `-S/--skipinitialspace`                         | `skipinitialspace`  | trim whitespace after each delimiter                                 |
|  [08]   | `--blanks` / `--null-value`                     | `blanks`/`null_values` | keep blanks literal, or extend the null-coercion vocabulary       |
|  [09]   | `--date-format` / `--datetime-format`            | `date_format`/`datetime_format` | strptime patterns overriding agate date inference        |
|  [10]   | `--no-leading-zeroes`                           | `no_leading_zeroes` | treat leading-zero numerics as numbers, not text                     |
|  [11]   | `-H/--no-header-row`                            | `no_header_row`     | synthesize `a,b,c...` headers (`make_default_headers`)               |
|  [12]   | `-K/--skip-lines`                              | `skip_lines`        | skip N leading lines before the header                               |
|  [13]   | `--zero` / `-l/--linenumbers`                    | `zero_based`/`line_numbers` | zero-based column ids; prepend a line-number column          |
|  [14]   | `--add-bom`                                     | `add_bom`           | prefix output with a UTF-8 BOM (Excel interop)                       |

[ENTRYPOINT_SCOPE]: in-process boundary (IN-PROCESS mode)
- rail: tabular-csv
- When the subprocess fork cost is not warranted (small payloads, an already-loaded buffer, or a need to avoid argv shell quoting), instantiate the tool class with an argv list and a capture buffer and run it inside an `anyio.to_thread.run_sync` worker — the tool's `run()` is blocking I/O, so the thread hop keeps the event loop live. The library helpers and the `agate.Table` engine are the lower-level in-process path when only the transform (not the CLI) is needed.

| [INDEX] | [SURFACE]                                                                       | [ENTRY_FAMILY] | [RAIL]                                                                |
| :-----: | :------------------------------------------------------------------------------ | :------------- | :------------------------------------------------------------------- |
|  [01]   | `CSVKitUtility(args=[...], output_file=buf, error_file=err)`                      | construct      | build a tool with captured output (`io.StringIO`) and explicit argv  |
|  [02]   | `<tool>.run()`                                                                   | execute        | open input + drive `main()`; blocking — wrap in `to_thread.run_sync` |
|  [03]   | `<tool>.print_column_names()`                                                    | introspect     | emit the header roster (the `--names` path) without a full transform |
|  [04]   | `cli.parse_column_identifiers(ids, column_names, column_offset, excluded_columns)` | resolve     | `1,name,3-5` spec -> zero-based index list (raises `ColumnIdentifierError`) |
|  [05]   | `cli.make_default_headers(n)`                                                    | headers        | synthetic header tuple for headerless input                          |
|  [06]   | `convert.guess_format(filename)`                                                 | infer          | extension -> `in2csv` format key (drives auto-detection)             |
|  [07]   | `grep.FilteringCSVReader(reader, patterns, *, header, any_match, inverse)`        | filter-iter    | predicate-filtered row iterator (the `csvgrep` core, no subprocess)  |
|  [08]   | `cleanup.RowChecker(reader, *, length_mismatch, join_short_rows, ...)` / `cleanup.join_rows(rows, separator)` | clean | streaming shape validation/repair (the `csvclean` core)  |
|  [09]   | `agate.Table.from_csv(path_or_file, *, column_types, sniff_limit, skip_lines)`    | engine-read    | the underlying typed Table; csvpy `--agate` binds exactly this       |
|  [10]   | `agate.Table.to_json(...)` / `agate.Table.to_csv(...)`                            | engine-write   | the emit side csvjson/in2csv compose                                 |

## [04]-[IMPLEMENTATION_LAW]

[CSVKIT_TOPOLOGY]:
- engine: every tool reads through `agate`'s sniffing CSV reader and (unless `--no-inference`) an `agate.TypeTester` building `agate.Text`/`agate.Number`/`agate.Date`/`agate.DateTime`/`agate.Boolean` columns; `get_column_types` folds `--blanks`/`--null-value`/`--locale`/`--date-format`/`--no-leading-zeroes` into the tester. Inference is the default and is what makes `csvstat`/`csvsort`/`csvjson` type-aware; `--no-inference` collapses every column to `agate.Text` for raw passthrough.
- two boundary modes: STREAM-PIPE (subprocess per tool, CSV bytes on stdin/stdout, composable as an OS pipeline) is the default for large/streaming data and shell-shaped transforms; IN-PROCESS (`CSVKitUtility(args=..., output_file=buf).run()` under a thread hop, or the library helpers/`agate.Table` directly) is for small payloads, loaded buffers, and avoiding argv quoting. Never re-implement a tool's transform in pure Python when the in-process class or its helper already owns it.
- format gateway: `in2csv` is the single entry that normalizes `dbf`/`fixed`/`geojson`/`json`/`ndjson`/`xls`/`xlsx` into CSV; `guess_format` does extension-based auto-detection. Every other tool assumes CSV in, so a heterogeneous-source pipeline always begins with `in2csv` (or `agate-excel`/`agate-dbf` in-process).
- emit discriminator: `csvjson` is one tool discriminating output shape by flag — plain JSON array (default), GeoJSON `FeatureCollection` when `--lat`/`--lon` (+ optional `--geometry`/`--crs`) are present, or NDJSON when `--stream` is set. One tool, three wire shapes; do not add parallel JSON emitters.
- column spec: `--columns`/`--not-columns` everywhere accept the same `1,name,3-5` grammar resolved by `parse_column_identifiers`; `--zero` switches to zero-based ids. A bad spec raises `ColumnIdentifierError` — the typed boundary signal.
- compression: input paths ending `.gz`/`.bz2`/`.xz`/`.zst` are opened transparently (gzip/bz2/lzma/zstandard) through `LazyFile`; csvkit never needs an external decompress stage for these.
- SQL bridge: `csvsql --db <SQLAlchemy-URL>` and `sql2csv --db <URL>` are the agate-sql round trip — `csvsql` generates dialect-correct DDL and bulk-inserts (chunked via `--chunk-size`, with `--no-create`/`--overwrite`/`--insert`/constraint flags), `sql2csv` executes a query and streams the result set back as CSV. The DB URL is any SQLAlchemy dialect; `--engine-option`/`--execution-option` pass through.

[STACKS_WITH]:
- `anyio` subprocess + structured concurrency (`.api/anyio.md`): a multi-tool CSV pipeline is built by `open_process` per stage wiring `stdout`->`stdin` inside one `create_task_group`, or as `run_process([tool, *flags], input=bytes, check=True)` per stage; the task group cancels the whole pipeline on first failure, and a `move_on_after` deadline preempts a wedged stage. The in-process class path runs under `to_thread.run_sync` so the blocking `run()` never stalls the loop.
- `stamina` retry (`.api/stamina.md`): a flaky source-fetch + `in2csv` conversion or a `csvsql` load against a contended DB is wrapped in `stamina.retry_context(on=BoundaryFault, attempts=..., timeout=...)`; the retry sleep is an `anyio` checkpoint, so the enclosing deadline still bounds a retry storm. A non-zero subprocess exit (raised by `check=True`) is the retryable signal.
- `expression` rails (`.api/expression.md`): the subprocess boundary returns a `CompletedProcess`; lift it with `Result.of_value`/a try-builder so a non-zero exit or a `ColumnIdentifierError`/`RequiredHeaderError` from the in-process path becomes `Error(BoundaryFault(...))` rather than a raw exception escaping the adapter. csvkit's typed exceptions map straight onto the error rail.
- `msgspec` models (`.api/msgspec.md`): `csvjson --stream` emits NDJSON, decoded line-by-line by `msgspec.json.Decoder(type=Row).decode_lines(buf)` into typed `Struct` records — csvkit owns the CSV->NDJSON shaping, msgspec owns the typed decode, with one column/field vocabulary across the wire. `csvstat --json` similarly feeds a stats `Struct`.
- `pydantic` validation (`.api/pydantic.md`): when the decoded CSV rows must satisfy a richer constraint set than agate inference, `csvjson`/`csvstat --json` output flows into `TypeAdapter(list[RowModel]).validate_json(bytes)`; csvkit's agate type inference is the cheap first pass, pydantic the strict gate — no parallel CSV parser.
- `beartype` contracts (`.api/beartype.md`): the in-process adapter that wraps `CSVKitUtility(args=..., output_file=buf).run()` carries a `@beartype` signature pinning the argv list and the returned buffer type, so a malformed flag list is a contract failure at the boundary, not a deep argparse `SystemExit`.
- `structlog` + OpenTelemetry (`.api/structlog.md`, `.api/opentelemetry-api.md`): each pipeline stage runs inside a `tracer.start_as_current_span("csvkit." + tool)` with the resolved argv and exit code bound as span attributes and structlog events; a `--verbose` tool's stderr is captured as the span's diagnostic context. One span per stage gives a traced CSV pipeline with no bespoke timing.
- `polars`/`pyarrow` handoff (`.api/polars.md`, `.api/pyarrow.md`): csvkit owns the messy-source normalization (dialect sniffing, Excel/DBF/GeoJSON ingestion, structural cleaning) that the columnar engines do not; the clean CSV `in2csv`/`csvclean` emit is then read by `polars.read_csv`/`pyarrow.csv.read_csv` for the heavy analytical pass. csvkit is the ingestion/repair front, the columnar engine the compute back — no overlap.

[LOCAL_ADMISSION]:
- The tabular-CSV lane composes csvkit tools as `anyio` subprocesses for streaming/shell-shaped transforms and as in-process `CSVKitUtility` instances (under `to_thread.run_sync`) for loaded buffers; it owns no hand-rolled CSV dialect sniffer, JSON/GeoJSON emitter, fixed-width parser, or SQL-load loop where a tool already owns it.
- `in2csv` is the single front door for non-CSV tabular sources (`xls`/`xlsx`/`dbf`/`fixed`/`json`/`ndjson`/`geojson`); the lane never imports a second Excel/DBF reader when `agate-excel`/`agate-dbf` already back it.
- Column selection/filter/sort/join/stack are expressed as the corresponding tool with a `parse_column_identifiers` spec; a missing column surfaces as `ColumnIdentifierError` mapped to the boundary error rail, never a silent skip.
- `csvjson --stream` + `msgspec` `Decoder.decode_lines` is the canonical CSV->typed-record path; `csvsql`/`sql2csv` is the canonical CSV<->SQL path; neither is re-implemented with a manual `csv` loop.
- Deterministic specs run tools in IN-PROCESS mode with a captured `io.StringIO` `output_file`, asserting on the exact emitted CSV/JSON rather than spawning a subprocess.

[RAIL_LAW]:
- Package: `csvkit`
- Owns: format conversion into CSV, column projection/filter/sort/join/stack, JSON/GeoJSON/NDJSON emit, SQL DDL+load+query round-trip, typed per-column statistics, dialect reformatting, structural cleaning, and an interactive agate `Table` handoff — over one shared dialect/encoding/null/locale grammar, in both subprocess and in-process modes
- Accept: `anyio.run_process`/`open_process` over the 14 `console_scripts`, in-process `CSVKitUtility(args=..., output_file=...).run()` under a thread hop, the `csvkit.cli`/`convert`/`cleanup`/`grep` library helpers, `agate.Table` as the engine, the typed `csvkit.exceptions` rail
- Reject: hand-rolled CSV dialect sniffing, bespoke JSON/GeoJSON/NDJSON emitters, manual fixed-width/Excel/DBF parsing, a second SQL-load loop, raw `subprocess`/`os.system` for tool invocation, swallowing `ColumnIdentifierError`/`RequiredHeaderError` instead of mapping them to the boundary rail
