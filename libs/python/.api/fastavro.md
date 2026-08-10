# [PY_BRANCH_API_FASTAVRO]

`fastavro` is the Cython-accelerated Avro codec: container-file and schemaless read and write, schema parsing with named-schema resolution, JSON-encoded Avro, canonical-form fingerprinting, and a logical-type dispatch table keyed on `"<avro-type>-<logicalType>"`. Six of its modules ship as compiled extensions selected at import time with a pure-Python twin behind each, and the codec roster is a plain dict whose keys exist whether or not the compression dependency does. It is the payload codec beneath the registry Avro serializer and the reader of a standalone `.avsc` schema document.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `fastavro`
- package: `fastavro` (MIT)
- module: `fastavro`
- namespaces: `fastavro.{read,write,schema,validation,json_read,json_write,types,const,utils,repository,io,logical_readers,logical_writers}`
- target: compiled wheel — six `cpython-315-darwin.so` extensions beside their `_*_py` fallbacks; `py.typed` with `.pyi` stubs standing in for the compiled modules
- rail: payload-codec

## [02]-[PUBLIC_TYPES]

[TYPE_SCOPE]: `fastavro.types`

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [CAPABILITY]                                                                               |
| :-----: | :------------- | :------------ | :----------------------------------------------------------------------------------------- |
|  [01]   | `Schema`       | type alias    | `str \| list \| dict` — a bare type name, a union list, or a dict schema                   |
|  [02]   | `DictSchema`   | type alias    | `dict[Any, Any]`                                                                           |
|  [03]   | `NamedSchemas` | type alias    | `dict[str, dict]` — the mutable accumulator `parse_schema` threads                         |
|  [04]   | `AvroMessage`  | type alias    | the value union: `None`, `str`, `float`, `int`, `Decimal`, `bool`, `bytes`, `list`, `dict` |

[READER_WRITER_SCOPE]: the stateful codecs

| [INDEX] | [SYMBOL]                                              | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :---------------------------------------------------- | :------------ | :---------------------------------------------------------------------- |
|   [01]  | `fastavro.read.reader`                                | class         | container-file iterator; `metadata`, `codec`, `writer_schema`           |
|   [02]  | `fastavro.read.block_reader`                          | class         | the same file as `Block` values, each a lazy record run                 |
|   [03]  | `fastavro._read.Block`                                | class         | `num_records`/`offset`/`size`/`codec`; `read` exports it NOT            |
|   [04]  | `fastavro.write.Writer`                               | class         | incremental writer: `write`/`write_block`/`flush`/`dump`                |
|   [05]  | `fastavro._write_py.GenericWriter`                    | ABC           | `schema`/`metadata`/`validate_fn` base; `write` exports it NOT          |
|   [06]  | `io.binary_encoder` / `io.binary_decoder`             | class         | `BinaryEncoder`/`BinaryDecoder`, pure-Python primitives                 |
|   [07]  | `io.json_encoder` / `io.json_decoder`                 | class         | `AvroJSONEncoder`/`AvroJSONDecoder`, same submodule paths               |

[SCHEMA_SCOPE]: `fastavro.schema` and `fastavro.repository`

| [INDEX] | [SYMBOL]                                              | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :---------------------------------------------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `schema.SchemaParseException`         | exception     | a malformed or contradictory schema document                         |
|  [02]   | `schema.UnknownType`                  | exception     | a named reference no accumulator resolves                            |
|  [03]   | `schema.FINGERPRINT_ALGORITHMS`       | `set[str]`    | `hashlib.algorithms_guaranteed` plus `SHA-256`, `MD5`, `CRC-64-AVRO` |
|  [04]   | `repository.AbstractSchemaRepository` | ABC           | one `load(name)` member — the `load_schema` injection seam           |
|  [05]   | `repository.FlatDictRepository`       | class         | `.avsc` files under one directory, keyed by schema name              |
|  [06]   | `repository.SchemaRepositoryError`    | exception     | a failed load or parse, both re-raised as one class                  |
|  [07]   | `read.SchemaResolutionError`          | exception     | a writer schema the reader schema cannot resolve                     |
|  [08]   | `validation.ValidationError`          | exception     | carries a `ValidationErrorData(datum, schema, field)` list           |

[LOGICAL_SCOPE]: `LOGICAL_READERS` and `LOGICAL_WRITERS`, both plain module-level dicts keyed `"<avro-type>-<logicalType>"`

| [INDEX] | [KEY]                           | [READS_AS]              | [WRITES_FROM]       |
| :-----: | :------------------------------ | :---------------------- | :------------------ |
|  [01]   | `long-timestamp-millis`         | aware `datetime` in UTC | aware or naive      |
|  [02]   | `long-timestamp-micros`         | aware `datetime` in UTC | aware or naive      |
|  [03]   | `long-local-timestamp-millis`   | naive `datetime`        | naive stamped UTC   |
|  [04]   | `long-local-timestamp-micros`   | naive `datetime`        | naive stamped UTC   |
|  [05]   | `int-date`                      | `datetime.date`         | `date` or ISO `str` |
|  [06]   | `bytes-decimal` `fixed-decimal` | `decimal.Decimal`       | `Decimal`           |
|  [07]   | `string-uuid`                   | `uuid.UUID`             | `UUID` or `str`     |
|  [08]   | `int-time-millis`               | `datetime.time`         | `time`              |
|  [09]   | `long-time-micros`              | `datetime.time`         | `time`              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: container-file and schemaless codecs

| [INDEX] | [SURFACE]                                                                 | [SHAPE] | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------ | :------ | :--------------------------------------- |
|  [01]   | `writer(fo, schema, records, codec="null", sync_interval=16000, …)`       | static  | whole container file                     |
|  [02]   | `Writer(fo, schema, codec="null", sync_interval=16000, …)`                | ctor    | incremental writer                       |
|  [03]   | `schemaless_writer(fo, schema, record, strict=False, …)`                 | static  | one record, no header; compiled drops the `*` |
|  [04]   | `reader(fo, reader_schema=None, return_record_name=False, …)`             | ctor    | container-file iterator                  |
|  [05]   | `block_reader(fo, reader_schema=None, …)`                                 | ctor    | the same file, block-granular            |
|  [06]   | `schemaless_reader(fo, writer_schema, reader_schema=None, …)`             | static  | one record against a known writer schema |
|  [07]   | `is_avro(path_or_buffer)`                                                 | static  | magic-byte probe                         |
|  [08]   | `json_writer(fo, schema, records, *, write_union_type=True, …)`           | static  | JSON-encoded Avro                        |
|  [09]   | `json_reader(fo, schema, reader_schema=None, *, decoder=AvroJSONDecoder)` | static  | the JSON inverse                         |

`writer` continues `metadata=None`, `validator=None`, `sync_marker=None`, `codec_compression_level=None` on the COMPILED path — the pure `_write_py` twin alone spells `validator=False`/`sync_marker=b""` — then the keyword-only `strict=False`, `strict_allow_default=False`, `disable_tuple_notation=False`. `Writer` continues `metadata=None`, `validator=False`, `sync_marker=b""`, `compression_level=None`, `options={}`. `schemaless_writer` closes on `strict_allow_default=False`, `disable_tuple_notation=False`, and `json_writer` on `validator=False`, `encoder=AvroJSONEncoder`, `strict=False`, `strict_allow_default=False`, `disable_tuple_notation=False`.

`reader` continues `return_record_name_override=False`, `handle_unicode_errors="strict"`, `return_named_type=False`, `return_named_type_override=False`.

[ENTRYPOINT_SCOPE]: schema surface

| [INDEX] | [SURFACE]                                                            | [SHAPE] | [CAPABILITY]                                        |
| :-----: | :------------------------------------------------------------------- | :------ | :-------------------------------------------------- |
|  [01]   | `schema.parse_schema(schema, named_schemas=None, *, expand=False)`   | static  | resolve names, normalize, and hint the writer       |
|  [02]   | `schema.load_schema(schema_path, *, repo=None, named_schemas=None)`  | static  | one file, or a repository NAME under a bound `repo` |
|  [03]   | `schema.load_schema_ordered(ordered_schemas)`                        | static  | dependency-ordered multi-file resolution            |
|  [04]   | `schema.expand_schema(schema)` / `schema.fullname(schema)`           | static  | inline every named reference; the qualified name    |
|  [05]   | `schema.to_parsing_canonical_form(schema)`                           | static  | the specification's canonical string                |
|  [06]   | `schema.fingerprint(parsing_canonical_form, algorithm)`              | static  | the schema digest under one named algorithm         |
|  [07]   | `validation.validate(datum, schema, field="", raise_errors=True, …)` | static  | one record                                          |
|  [08]   | `validation.validate_many(records, schema, raise_errors=True, …)`    | static  | a sequence                                          |

`validation.validate` and `validation.validate_many` each continue `strict=False`, `disable_tuple_notation=False`.

[CODEC_ROSTER]: `read.BLOCK_READERS` keys seven names and `_write.BLOCK_WRITERS` mirrors them under the private module alone; a key present with its dependency absent installs a thunk that raises `ValueError` on first use

| [INDEX] | [CODEC]     | [DEPENDS_ON]                                          | [STATE]                                        |
| :-----: | :---------- | :---------------------------------------------------- | :--------------------------------------------- |
|  [01]   | `null`      | nothing                                               | live                                           |
|  [02]   | `deflate`   | stdlib `zlib`, raw window                             | live                                           |
|  [03]   | `bzip2`     | stdlib `bz2`                                          | live                                           |
|  [04]   | `xz`        | stdlib `lzma`                                         | live                                           |
|  [05]   | `zstandard` | stdlib `compression.zstd`                             | live — no PyPI package is needed at this floor |
|  [06]   | `lz4`       | `lz4.block`                                           | live                                           |
|  [07]   | `snappy`    | `cramjam`, or the deprecated `python-snappy` fallback | the thunk — neither is installed               |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Six modules ship compiled and are selected at IMPORT time by a `try`/`except ImportError` at each shim, with no runtime toggle and no environment switch — `read`, `write`, `schema`, `validation`, `logical_readers`, `logical_writers`. Each pure-Python twin is reachable only by importing `fastavro._read_py` and its siblings directly. `json_read` and `json_write` import the pure-Python halves UNCONDITIONALLY, so the JSON codec path never runs compiled.
- Compiled encode and decode hold the GIL, so a thread hop buys nothing; only the surrounding file I/O is worth offloading.
- `fastavro.__all__` is computed by walking `locals()`, so thirteen submodule names and `__version__` leak into it beside the ten bound functions. `validate_many`, `Writer`, `load_schema`, and `fingerprint` are NOT on the top-level namespace and resolve through `fastavro.validation`, `fastavro.write`, and `fastavro.schema`.
- Two stub-versus-runtime divergences: `_validation.pyi` omits the real trailing `disable_tuple_notation` on both `validate` and `validate_many`, and `_read.pyi`'s `Block.__init__` omits the real `named_schemas` parameter. `schemaless_writer`'s keyword-only marker is lost in the compiled build and its three trailing flags accept positionally there — the keyword form is written regardless, since the pure-Python twin refuses it.
- Codec keys present in `BLOCK_READERS` are not capability claims: a missing dependency installs a thunk raising `ValueError` naming the libraries at first use, so codec availability probes the thunk rather than the key.
- Every logical WRITER is pass-through on a non-matching Python type — `prepare_date` on an `int`, `prepare_uuid` on a `str`, `prepare_bytes_decimal` on a non-`Decimal` each return the value unchanged and it reaches the raw encoder. There is no coercion error path, so a wrong type is a silently wrong record rather than a refusal.
- Timezone handling is ASYMMETRIC: `prepare_timestamp_millis`/`_micros` on a NAIVE datetime run through `time.mktime`, which reads the machine's local zone, while an aware datetime differences against the UTC epoch. `local-timestamp-*` writers instead stamp UTC onto the naive value, and a naive datetime crossing `timestamp-micros` is machine-dependent.
- `read_decimal` mutates a MODULE-LEVEL shared `decimal.Context` precision per call, so concurrent reads of schemas with differing precision race on process-global state.
- `LOGICAL_READERS`, `LOGICAL_WRITERS`, `BLOCK_READERS`, and `BLOCK_WRITERS` are plain module-level dicts, so an override is process-global and permanent for the interpreter; `write.py` re-exports no writer roster at all, so the block-writer table reaches only through `fastavro._write`.
- `Writer.__init__` on an appendable file re-reads the existing header and ADOPTS the file's codec and sync marker, ignoring the codec it was handed; on a fresh file an unknown codec raises `ValueError`. `writer` spells the same knob `codec_compression_level` where `Writer` spells it `compression_level`.
- Unknown `logicalType` values never error — dispatch misses and the raw Avro value passes through.
- `file_reader.schema` is deprecated in favour of `writer_schema`; the legacy `python-snappy` fallback warns toward `cramjam`. Those are the only deprecations, and the package's own CLI trips the first.

[STACKING]:
- `confluent-kafka`(`.api/confluent-kafka.md`): `AvroSerializer`/`AvroDeserializer` compose this codec beneath the registry's magic-byte frame, so a registry-fronted payload never reaches `schemaless_writer` directly and the writer schema resolves off the frame's schema id.
- `cloudevents`(`.api/cloudevents.md`): the Avro `EventFormat` row's `write_data`/`read_data` pair runs `schemaless_writer`/`schemaless_reader` over the resolved schema, since the CloudEvents payload carries its own framing and needs no Avro container header.
- `obstore`(`.api/obstore.md`): a container file read through `reader` streams off an object-store byte range, so `block_reader`'s per-block offsets align with the ranged fetch rather than a whole-object download.

[LOCAL_ADMISSION]:
- `parse_schema` runs ONCE per schema at admission and its result is the value every codec call takes, so no leg re-parses a document per record.
- `strict=True` is the admitted write posture — a record must carry exactly the schema's fields — and `strict_allow_default` relaxes it only where a schema default genuinely stands in.
- Logical values cross as aware `datetime`, `decimal.Decimal`, `uuid.UUID`, and `datetime.date`; a naive datetime never reaches a `timestamp-*` slot, since the writer reads the machine's zone.
- Codec availability probes the thunk at composition rather than reading a key, and `snappy` refuses by name at this floor.
- Compiled modules are the only admitted path; a direct `fastavro._read_py` import is the deleted form.

[RAIL_LAW]:
- Package: `fastavro`
- Owns: Avro container and schemaless encode/decode, schema parse and resolution, canonical form and fingerprinting, record validation, logical-type dispatch
- Accept: `parse_schema`, `schemaless_writer`/`schemaless_reader`, `writer`/`reader`/`block_reader`, `Writer`, `schema.load_schema` with a bound repository, `to_parsing_canonical_form`/`fingerprint`, `validation.validate`/`validate_many`
- Reject: a naive datetime on a `timestamp-*` slot; a per-record `parse_schema`; a direct `_*_py` import; a codec selected off dict-key presence; a mutation of the process-global logical or block dicts
