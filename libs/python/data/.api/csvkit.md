# [PY_DATA_API_CSVKIT]

`csvkit` is the tabular subprocess-CLI boundary of the data rail: a suite of 14 agate-backed console scripts that ingest, clean, slice, join, sort, stack, reformat, query, and inspect delimited tabular data as a Unix-style pipeline. It is the boundary complement to the in-process Arrow readers (`fastexcel`/`python-calamine`) and the in-process query engine (`duckdb`/`polars`): `in2csv` converts foreign tabular formats (DBF, fixed-width, GeoJSON, JSON, NDJSON, XLS, XLSX) to CSV, `csvsql`/`sql2csv` bridge CSV to any SQLAlchemy database, and the `csv*` filter family transforms CSV-in/CSV-out under a shared dialect-and-inference option vocabulary. The owner invokes the binaries through the runtime `anyio.run_process` boundary (the manifest bans direct `subprocess.*`), reads the agate type-inference (numbers/dates/booleans/nulls) the tools apply, and never re-implements a CSV parser, a fixed-width/DBF/Excel decoder, or a cross-database SQL emitter. A thin importable surface (`csvkit.reader`/`writer`/`DictReader`/`DictWriter`, `csvkit.agate`) re-exports the agate CSV reader/writer for in-process use, but the boundary role is the subprocess CLI.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `csvkit`
- package: `csvkit`
- import: `csvkit` (importable reader/writer surface); the boundary use is the console scripts invoked as a subprocess
- owner: `data`
- rail: tabular-cli
- installed: `2.2.0` (agate `1.14.2`, agate-excel `0.4.2`, agate-dbf `0.2.4`, agate-sql `0.7.3`)
- entry points: 14 console scripts — `in2csv`, `sql2csv`, `csvclean`, `csvcut`, `csvgrep`, `csvjoin`, `csvsort`, `csvstack`, `csvformat`, `csvsql`, `csvstat`, `csvlook`, `csvpy`, `csvjson`
- capability: foreign-format-to-CSV conversion (DBF/fixed-width/GeoJSON/JSON/NDJSON/XLS/XLSX via `in2csv`); structural error report/repair (`csvclean`); column projection (`csvcut`), row search by literal/regex/match-file (`csvgrep`), multi-key sort (`csvsort`), SQL-like inner/outer/left/right join (`csvjoin`), and multi-file row stacking with grouping (`csvstack`); output dialect reformatting incl. ASV unit/record separators (`csvformat`); cross-dialect SQL DDL/DML generation and direct execution (`csvsql`) and database-query-to-CSV (`sql2csv`) over SQLAlchemy; per-column descriptive statistics in text/CSV/JSON (`csvstat`); Markdown-table console rendering (`csvlook`); CSV-to-JSON/GeoJSON serialization (`csvjson`); and a Python shell preloaded with a CSV reader / `DictReader` / agate `Table` (`csvpy`). Every tool applies agate type inference (numbers, dates, datetimes, booleans, common null tokens), disable-able with `-I`/`--no-inference`.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: importable Python surface
- rail: tabular-cli

The package module re-exports the agate CSV reader/writer for in-process use; the deeper table model is `agate` (`agate.Table.from_csv`/`to_csv`, `agate.TypeTester` inference). The console scripts are the boundary surface and live under `[03]-[ENTRYPOINTS]`.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [RAIL]                                                              |
| :-----: | :------------------ | :------------ | :----------------------------------------------------------------- |
|  [01]   | `csvkit.reader`     | reader        | unicode-aware CSV row reader (agate `csv` reader) over a file/stream |
|  [02]   | `csvkit.writer`     | writer        | unicode-aware CSV row writer (agate `csv` writer)                   |
|  [03]   | `csvkit.DictReader` | reader        | header-keyed dict-row CSV reader                                    |
|  [04]   | `csvkit.DictWriter` | writer        | header-keyed dict-row CSV writer                                    |
|  [05]   | `csvkit.agate`      | module        | the re-exported `agate` table engine (`Table.from_csv`, type inference, aggregations) backing every tool |

[PUBLIC_TYPE_SCOPE]: shared dialect-and-inference option vocabulary
- rail: tabular-cli

Most tools inherit one common CSVKit option base; documenting it once avoids per-tool repetition. Input-dialect flags describe the source CSV, inference flags govern agate typing, and structural flags govern headers/rows.

| [INDEX] | [OPTION]                                 | [CAPABILITY]                                                          |
| :-----: | :--------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `-d/--delimiter`, `-t/--tabs`            | input field delimiter; `-t` forces tab and overrides `-d`            |
|  [02]   | `-q/--quotechar`, `-b/--no-doublequote`, `-p/--escapechar` | input quote char, doubled-quote toggle, escape char |
|  [03]   | `-u/--quoting {0,1,2,3,4,5}`             | input quoting style (0 minimal, 1 all, 2 non-numeric, 3 none)        |
|  [04]   | `-z/--maxfieldsize`, `-e/--encoding`     | per-field byte cap and source encoding                              |
|  [05]   | `-S/--skipinitialspace`                  | ignore whitespace immediately after the delimiter                   |
|  [06]   | `-H/--no-header-row`, `-K/--skip-lines`  | treat input as header-less (synthesize `a,b,c,…`); skip N leading lines |
|  [07]   | `-L/--locale`, `--date-format`, `--datetime-format`, `--no-leading-zeroes` | agate numeric locale and strptime date/datetime parsing; keep leading-zero strings |
|  [08]   | `--blanks`, `--null-value`               | keep `""`/`na`/`n/a`/`none`/`null`/`.` literal; add extra NULL tokens |
|  [09]   | `-y/--snifflimit`, `-I/--no-inference`   | dialect-sniff byte budget (`0` off, `-1` whole file); disable agate type inference |
|  [10]   | `-l/--linenumbers`, `--add-bom`, `--zero`, `-v/--verbose`, `-V/--version` | prepend a line-number column; emit UTF-8 BOM; zero-based column numbering; full tracebacks; version |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: ingest and convert
- rail: tabular-cli

`in2csv` is the universal foreign-format-to-CSV converter; `-f` pins the source format (else inferred), with Excel/JSON/fixed-width sub-options. `sql2csv` runs a SQL query against a SQLAlchemy database and emits the result as CSV.

| [INDEX] | [SURFACE]  | [CALL_SHAPE]                                                                                                            | [CAPABILITY]                                              |
| :-----: | :--------- | :-------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `in2csv`   | `in2csv [-f {csv,dbf,fixed,geojson,json,ndjson,xls,xlsx}] [-s SCHEMA] [-k KEY] [-n] [--sheet S] [--write-sheets …] [--use-sheet-names] [--reset-dimensions] [--encoding-xls E] [FILE]` | convert DBF/fixed-width/GeoJSON/JSON/NDJSON/XLS/XLSX to CSV; `-s` is the fixed-width schema, `-k` the JSON list key, `-n`/`--sheet`/`--write-sheets` the Excel sheet controls |
|  [02]   | `sql2csv`  | `sql2csv [--db CONNECTION_STRING] [--query QUERY] [--engine-option K V] [--execution-option K V] [-e ENC] [-H] [FILE]` | execute a SQL query (from `--query`, FILE, or STDIN) on a database and write the rows as CSV; `-H` omits column names |

[ENTRYPOINT_SCOPE]: transform CSV (CSV-in, CSV-out)
- rail: tabular-cli

The filter family shares the common option vocabulary and adds its own selection/transform flags. Column arguments accept a comma-separated mix of 1-based indices, names, and ranges (`"1,id,3-5"`).

| [INDEX] | [SURFACE]   | [CALL_SHAPE]                                                                                          | [CAPABILITY]                                                                 |
| :-----: | :---------- | :--------------------------------------------------------------------------------------------------- | :-------------------------------------------------------------------------- |
|  [01]   | `csvcut`    | `csvcut [-n] [-c COLUMNS] [-C NOT_COLUMNS] [-x] [FILE]`                                              | project/reorder columns (`-c`) or exclude them (`-C`); `-n` lists columns+indices; `-x` drops fully-empty rows |
|  [02]   | `csvgrep`   | `csvgrep [-c COLUMNS] (-m PATTERN | -r REGEX | -f MATCHFILE) [-i] [-a] [FILE]`                        | keep rows where the named columns match a literal (`-m`), regex (`-r`), or any line of a match-file (`-f`); `-i` inverts, `-a` matches in any column |
|  [03]   | `csvsort`   | `csvsort [-n] [-c COLUMNS] [-r] [-i] [-y SNIFF] [-I] [FILE]`                                         | multi-key type-aware sort by `-c`; `-r` descending, `-i` case-insensitive    |
|  [04]   | `csvjoin`   | `csvjoin [-c COLUMNS] [--outer] [--left] [--right] [-y SNIFF] [-I] [FILE ...]`                       | SQL-like join of two-or-more files on `-c` keys (one key per file); default inner, with full-outer/left/right; reads all inputs into memory |
|  [05]   | `csvstack`  | `csvstack [-g GROUPS] [-n GROUP_NAME] [--filenames] [FILE ...]`                                      | concatenate rows of multiple same-shape files; `-g`/`-n` add a grouping column, `--filenames` uses each source filename as the group value |
|  [06]   | `csvformat` | `csvformat [-E] [-D OUT_DELIMITER] [-T] [-A] [-Q OUT_QUOTECHAR] [-U {0..5}] [-B] [-P OUT_ESCAPECHAR] [-M OUT_LINETERMINATOR] [FILE]` | rewrite to a custom output dialect; `-E` drops the header, `-A` emits ASCII unit/record separators, the `-D/-T/-Q/-U/-B/-P/-M` set mirrors the input dialect flags for output |
|  [07]   | `csvclean`  | `csvclean [--length-mismatch] [--empty-columns] [-a] [--omit-error-rows] [--label LABEL] [--header-normalize-space] [--join-short-rows] [--separator SEP] [--fill-short-rows] [--fillvalue V] [FILE]` | report (STDERR) and fix (STDOUT) structural errors — ragged-length and empty-column detection (`-a` enables all checks), short-row join/fill, header whitespace normalization, error-row omission |

[ENTRYPOINT_SCOPE]: database SQL bridge
- rail: tabular-cli

`csvsql` is the bidirectional CSV-SQL bridge: with no `--db` it emits `CREATE TABLE` DDL in a chosen dialect; with `--db` it executes the generated SQL and optionally loads the rows.

| [INDEX] | [SURFACE] | [CALL_SHAPE]                                                                                                                                                                   | [CAPABILITY]                                                          |
| :-----: | :-------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `csvsql`  | `csvsql [-i {mssql,mysql,oracle,postgresql,sqlite}] [--db CONNECTION_STRING] [--query QUERIES] [--insert] [--prefix P] [--before-insert SQL] [--after-insert SQL] [--sql-delimiter D] [--tables NAMES] [--no-constraints] [--unique-constraint COLS] [--no-create] [--create-if-not-exists] [--overwrite] [--db-schema S] [--chunk-size N] [--min-col-len N] [--col-len-multiplier M] [FILE ...]` | generate dialect-specific `CREATE TABLE` DDL (`-i`, no `--db`) or execute against a database (`--db`): `--query` runs SQL and emits the last result as CSV, `--insert` loads rows with `--no-create`/`--create-if-not-exists`/`--overwrite`/`--chunk-size` lifecycle control |

[ENTRYPOINT_SCOPE]: inspect and serialize
- rail: tabular-cli

`csvstat`/`csvlook` are read-only inspectors; `csvjson` serializes to JSON/GeoJSON; `csvpy` opens an interactive shell over the loaded data.

| [INDEX] | [SURFACE] | [CALL_SHAPE]                                                                                                                       | [CAPABILITY]                                                            |
| :-----: | :-------- | :-------------------------------------------------------------------------------------------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `csvstat` | `csvstat [--csv] [--json] [-i INDENT] [-n] [-c COLUMNS] [--type|--nulls|--non-nulls|--unique|--min|--max|--sum|--mean|--median|--stdev|--len|--max-precision|--freq|--count] [--freq-count N] [--decimal-format F] [-G] [FILE]` | per-column descriptive statistics (type, nulls, unique, min/max, sum, mean, median, stdev, max length/precision, frequent values, row count); plain-text, `--csv`, or `--json`; a single-stat flag narrows output to that measure |
|  [02]   | `csvlook` | `csvlook [--max-rows N] [--max-columns N] [--max-column-width N] [--max-precision N] [--no-number-ellipsis] [FILE]`                | render a Markdown-compatible fixed-width table to the console with row/column/width/precision truncation |
|  [03]   | `csvjson` | `csvjson [-i INDENT] [-k KEY] [--lat LAT] [--lon LON] [--type TYPE] [--geometry GEOMETRY] [--crs CRS] [--no-bbox] [--stream] [FILE]` | serialize CSV to JSON (array, or `-k`-keyed object) or GeoJSON FeatureCollection (`--lat`/`--lon`/`--geometry`/`--crs`, bbox unless `--no-bbox`); `--stream` emits newline-delimited objects |
|  [04]   | `csvpy`   | `csvpy [--dict] [--agate] [--no-number-ellipsis] [FILE]`                                                                          | drop into a Python shell with the file preloaded as a `csvkit.reader` (default), a `DictReader` (`--dict`), or an `agate.Table` (`--agate`) |

## [04]-[IMPLEMENTATION_LAW]

[TABULAR_CLI]:
- invocation: csvkit is a subprocess boundary, not an in-process library on the hot path; the owner spawns each binary through the runtime `anyio.run_process` adapter (the manifest bans `subprocess.*`), pipes CSV/JSON over STDIN/STDOUT, and captures STDERR for `csvclean` error reports. The importable `csvkit.reader`/`writer`/`DictReader`/`DictWriter` exist for in-process use but the canonical role is the CLI.
- dialect axis: one shared input-dialect vocabulary (`-d`/`-t`/`-q`/`-u`/`-b`/`-p`/`-z`/`-e`/`-S`) describes the source CSV across every tool; `csvformat` adds the mirrored output set (`-D`/`-T`/`-A`/`-Q`/`-U`/`-B`/`-P`/`-M`). Dialect is a per-invocation option, never a per-tool variant.
- inference axis: agate type inference (numbers, dates/datetimes via `-L`/`--date-format`/`--datetime-format`, booleans, null tokens) runs by default; `-I`/`--no-inference` forces raw-string passthrough and `-y`/`--snifflimit` bounds dialect sniffing. The owner reads inference results, never re-implements typing.
- conversion axis: `in2csv` is the single foreign-format ingress (`-f` discriminates DBF/fixed/GeoJSON/JSON/NDJSON/XLS/XLSX) backed by `agate-excel`/`agate-dbf`; it is the CSV-producing inverse of `csvjson`/`csvformat`. Excel-to-CSV through `in2csv` is the subprocess counterpart to the in-process `fastexcel`/`python-calamine` readers — boundary-shell versus zero-copy Arrow.
- transform axis: the `csv*` filter family is CSV-in/CSV-out and composes as a pipeline — `csvcut` (column projection), `csvgrep` (row search), `csvsort`, `csvjoin` (in-memory SQL-like join), `csvstack` (vertical concat with grouping), `csvclean` (structural repair). Column arguments are the shared index/name/range mini-language (`"1,id,3-5"`), 1-based unless `--zero`.
- sql axis: `csvsql` and `sql2csv` bridge CSV and any SQLAlchemy-reachable database — `csvsql -i <dialect>` emits portable DDL, `csvsql --db --insert` loads rows with create/overwrite/chunk lifecycle, `sql2csv --db --query` exports query results. This is the heterogeneous-database bridge distinct from the in-process `duckdb`/`adbc`/`connectorx` query rails; it composes those engines' SQLAlchemy URLs rather than replacing them.
- json axis: `csvjson` is the CSV-to-JSON/GeoJSON serializer (array, `-k`-keyed object, or GeoJSON FeatureCollection with `--lat`/`--lon`/`--geometry`/`--crs`); `--stream` emits NDJSON. The geospatial GeoJSON output complements the in-process `geopandas`/`pyogrio` vector IO at the shell boundary.
- evidence: each invocation's receipt is the tool's own output channel — `csvstat --json` for column statistics, `csvclean` STDERR for the structural-error report (with `--label` row provenance), and exit code for success/failure; the owner threads these into the data quality/profile rail rather than re-deriving them.
- boundary: csvkit owns the subprocess CSV pipeline — foreign-format conversion, structural cleaning, shell-style transforms, cross-database SQL generation/execution, descriptive stats, and JSON/GeoJSON serialization. In-process Arrow ingestion (`fastexcel`/`python-calamine`), the columnar query engine (`duckdb`/`polars`/`datafusion`), and the schema-contract gate (`dataframely`/`pandera`/`pointblank`) stay outside it; csvkit is the shell-boundary tool, not the in-memory dataframe owner.
- stacking: the tools chain over pipes (`in2csv data.xlsx | csvcut -c 1,3 | csvgrep -c 1 -m foo | csvsort -c 3 | csvjson`), so a multi-step boundary transform is one composed subprocess pipeline; the final CSV/JSON feeds the in-process dataset/frame/lakehouse owner, which picks the materialization.

[RAIL_LAW]:
- Package: `csvkit`
- Owns: the 14-tool agate-backed CSV subprocess pipeline — foreign-format-to-CSV conversion (`in2csv`), structural error report/repair (`csvclean`), column/row/sort/join/stack/reformat transforms (`csvcut`/`csvgrep`/`csvsort`/`csvjoin`/`csvstack`/`csvformat`), cross-dialect SQL generation/execution and query export (`csvsql`/`sql2csv`), per-column statistics (`csvstat`), Markdown-table rendering (`csvlook`), CSV-to-JSON/GeoJSON serialization (`csvjson`), and an interactive CSV/`DictReader`/agate-`Table` shell (`csvpy`), all under one shared dialect-and-inference option vocabulary
- Accept: shell-boundary tabular work invoked through `anyio.run_process` — converting foreign tabular files to CSV, cleaning and reshaping CSV in a pipeline, bridging CSV to a SQLAlchemy database, and serializing CSV to JSON/GeoJSON
- Reject: a hand-rolled CSV parser, fixed-width/DBF/Excel decoder, or cross-database SQL emitter where a csvkit tool owns it; direct `subprocess.*` invocation bypassing the `anyio.run_process` boundary; using csvkit on the in-process hot path where `fastexcel`/`python-calamine` zero-copy Arrow reads or the `duckdb`/`polars` query engine belong; re-implementing the agate type inference the tools apply

## [05]-[LOCAL_ADMISSION]

[CAPTURE_GAP]:
- the suite is agate-backed (verified against `csvkit 2.2.0` with `agate 1.14.2`, `agate-excel 0.4.2`, `agate-dbf 0.2.4`, `agate-sql 0.7.3`): the CLI tools delegate parsing, type inference, and aggregation to `agate`, Excel/DBF decode to `agate-excel`/`agate-dbf`, and database access to `agate-sql` over SQLAlchemy. The owner composes the binaries; it never re-implements agate.
- `csvsql -i` emits exactly `mssql`/`mysql`/`oracle`/`postgresql`/`sqlite`, so a DDL-generation consumer targets that set and routes DuckDB and every other engine through the in-process query rail (`duckdb`/`adbc`/`connectorx`).
- the boundary role over the in-process readers: `in2csv` Excel-to-CSV is the subprocess counterpart to the zero-copy `fastexcel`/`python-calamine` Arrow readers, and `csvsql`/`sql2csv` are the SQLAlchemy heterogeneous-database bridge beside the in-process `duckdb`/`adbc`/`connectorx` rails — csvkit is admitted for the shell-pipeline boundary, not as a replacement for the in-memory dataframe/query owners.
