# [PYTHON_SYSTEM_APIS]

Standard-library APIs replace local machinery only when they own the concern. They do not replace `expression` rails, `pydantic`/`msgspec` admission, the numeric route owners, or the structured-concurrency owner. This page is the stdlib-owner-replacement law: the high-churn surface where a yearly stdlib delta retires a local helper, kept separate from the stable language-form law so the language page does not churn with each stdlib addition. A row names the owning stdlib surface and the local pattern, loop, or wrapper it deletes.

## [1]-[SMELL_LOOKUP]

This table is a lookup by repeated local smell; the owning section card states the placement law.

| [INDEX] | [SMELL]                            | [OWNER]                         |
| :-----: | :--------------------------------- | :------------------------------ |
|   [1]   | stringly `os.walk`/`os.path` flow  | `pathlib.Path` algebra          |
|   [2]   | manual chunked hash loop           | `hashlib.file_digest`           |
|   [3]   | local chunk/batch helper loop      | `itertools.batched`             |
|   [4]   | `zip` product fold                 | `math.sumprod`                  |
|   [5]   | timestamp-prefixed UUID wrapper    | `uuid.uuid7`                    |
|   [6]   | ambiguous `re.match` prefix check  | `re.prefixmatch`                |
|   [7]   | `datetime.strptime` slicing        | `date.strptime`/`time.strptime` |
|   [8]   | subprocess or bespoke zstd adapter | `compression.zstd`              |
|   [9]   | `bytes(buffer)` + `clear()`        | `bytearray.take_bytes`          |
|  [10]   | `os.cpu_count()` worker sizing     | `os.process_cpu_count`          |
|  [11]   | negated-priority heap wrapper      | max-heap `heapq` APIs           |

## [2]-[PATHS_AND_FILES]

[PATH_ALGEBRA]:
- Owner: `pathlib.Path` and `PurePath` — `Path.walk`, `Path.copy`, `Path.move`, `Path.info`, `Path.from_uri`, `PurePath.full_match`, `PurePath.with_segments`, `os.path.splitroot`, and `os.path.realpath(strict=os.path.ALLOW_MISSING)`.
- Replace: stringly `os.walk` flow, `shutil` transfer wrappers, repeated `stat()` probes, hand-parsed `file:` URLs, ad hoc recursive glob predicates, drive/root string slicing, and symlink-prefix resolution loops.
- Gate: `recurse_symlinks=`/`follow_symlinks=` carries symlink policy explicitly; `Path.info` caches the stat result so a type probe pays one syscall.
- Rule: `Path.walk` yields `(dirpath, dirnames, filenames)` as `Path`/`str`, replacing the `os.walk` string idiom end to end.

[FILE_IO]:
- Owner: `NamedTemporaryFile(delete_on_close=...)`, `shutil.rmtree(onexc=...)`, `os.readinto`, `importlib.resources.as_file`, `mimetypes.guess_file_type`, and `os.path.isreserved`.
- Replace: `mkstemp` unlink ladders, `onerror` tuple handlers, `os.read` copy slices, `__file__` extraction loops, path use of `guess_type`, and reserved-name tables.
- Gate: persisted text I/O states `encoding="utf-8"` (or `encoding="locale"` only at a genuine locale boundary), never relying on an implicit default at a durable seam.

## [3]-[TEXT_REGEX_TIME]

[REGEX]:
- Owner: `re` with `re.prefixmatch`, `re.PatternError`, and compiled module-level patterns.
- Replace: ambiguous `re.match` used as a prefix test and generic `re.error` catches.
- Rule: a structural grammar compiles to one module-level pattern; a parse-error path catches `re.PatternError`, not the broad `re.error`.

[DATETIME]:
- Owner: `datetime.date.strptime`, `datetime.time.strptime`, and timezone-aware `datetime`.
- Replace: `datetime.strptime` followed by `.date()`/`.time()` slicing and naive datetime arithmetic at boundaries.
- Boundary: a wire timestamp admits to an aware `datetime` at the seam; the interior carries the aware value, never a naive one.

[TEMPLATE_RENDER]:
- Owner: t-string processors and `string.templatelib.Template` consumed at the render boundary.
- Replace: f-string pre-parsing, rendered-string reparsing, and string concatenation hiding template policy.
- Boundary: template structure (segments, interpolations, conversions) is a language-form concern owned by `language.md`; this row owns only the render-time stdlib consumption.

## [4]-[NUMERIC_PRIMITIVES]

[SCALAR_MATH]:
- Owner: `math.integer`, `math.fma`, `math.fmax`/`math.fmin`, `math.isnormal`/`math.issubnormal`/`math.signbit`, `math.sumprod`, `fractions.Fraction.from_number`, and `statistics.kde`.
- Replace: float math on integer algorithms, rounded multiply-add, NaN-aware min/max wrappers, bit-level float probes, `zip` product folds, constructor branch ladders, and local kernel-density estimators.
- Boundary: array and matrix algorithms are not scalar-replacement concerns — they route to `algorithms.md`; this card owns scalar invariants on stdlib numeric primitives only.

[IDENTITY_AND_HEAP]:
- Owner: `uuid.uuid7`, `uuid.NIL`, `uuid.MAX`, `Counter` XOR, max-heap `heapq` APIs, and `operator.is_none`.
- Replace: timestamp-prefixed UUID wrappers, magic UUID boundary literals, manual count-difference folds, negated-priority heap wrappers, and one-off `lambda x: x is None`.

## [5]-[BINARY_AND_INTEGRITY]

[BUFFERS]:
- Owner: `collections.abc.Buffer`, `inspect.BufferFlags`, `array`/`memoryview` complex codes, `os.readinto`, `bytearray.take_bytes`, `ctypes.memoryview_at`, and pickle protocol-5 out-of-band buffers.
- Replace: `ByteString` or bytes prose, magic integer buffer flags, struct-packed numeric buffers, `os.read` copy slices, `bytes(buffer)` plus `clear()`, `string_at` copy scaffolds, and copy-heavy pickle blobs.
- Boundary: zero-copy buffer ownership across an `await` is a boundary concern owned by `boundaries.md`; this card owns the stdlib buffer-surface selection.

[CODECS_AND_DIGEST]:
- Owner: `compression.zstd`, `base64.z85encode`/`z85decode`, base-N `canonical=`/`padded=`/`wrapcol=`/`ignorechars=` decoders, `hashlib.file_digest`, and `zlib.adler32_combine`/`zlib.crc32_combine`.
- Replace: subprocess or bespoke zstd adapters, local Z85 codecs, padding-bit postchecks, pre/post encode formatting, manual chunked hash loops, and recompress-to-checksum loops.
- Gate: a wire digest streams through `hashlib.file_digest` (file) or one `hashlib` one-shot (in-memory); a non-cryptographic cache key uses a fast non-cryptographic hash, never SHA for a cache key.

## [6]-[ITERATION_AND_FUNCTIONAL]

[ITERATION]:
- Owner: `itertools.batched`, `functools.Placeholder`, `zip(strict=True)`, and `map(strict=True)`.
- Replace: local chunk/batch helper loops, lambda wrappers for partial gaps, and post-truncation length asserts.
- Rule: `zip(strict=True)`/`map(strict=True)` proves the arity invariant at the call, replacing a downstream length check.

[RESOURCE_MATERIALIZATION]:
- Owner: `importlib.resources.as_file`, `tomllib` (TOML 1.1.0), and `ContextVar.set` token context manager.
- Replace: `__file__` extraction loops, `tomli`/parser shims, and `reset(token)` `finally` blocks.

## [7]-[RESEARCH]

- [STDLIB_FLOOR]: every row assumes the interpreter floor in `language.md`'s ACTIVE_SURFACE; a row whose owning API regresses below the floor moves to the language page's replacement column with the older spelling. The floor is the single fact this page reads from the language owner.
