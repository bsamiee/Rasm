# [PY_ARTIFACTS_API_DETOOLS]

`detools` owns binary-delta for the artifacts DELTA_BUNDLE rail: `create_patch` diffs a from-image against a to-image into a compressed patch, `apply_patch` reconstructs the to-image, and `patch_info` peeks the self-describing header for per-kind metadata. Native `bsdiff`/`hdiffpatch`/`suffix_array` extensions own the diffing, in-place segmentation, and firmware data-format awareness; the patch-payload codecs are admitted siblings `detools` selects and frames, and `detools.Error` lifts to an `expression` `Result.Error` at the codec `async_boundary`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `detools`
- package: `detools`
- import: `detools`
- owner: `artifacts`
- rail: delta
- license: `BSD`
- marker: native `bsdiff`/`hdiffpatch`/`suffix_array` C extensions built from the sdist, interpreter-tagged (no abi3), rebuilt per Python minor
- depends: `humanfriendly`, `bitstruct`, `pyelftools` (ELF offset reader), `zstandard`, `lz4`, `heatshrink2` — the codec siblings `detools` selects
- namespaces: `detools`, `detools.create`, `detools.apply`, `detools.info`, `detools.compression`, `detools.data_format`, `detools.errors`
- public surface: no `__all__`; the library contract is the explicit `from … import …` re-export set the `[02]`/`[03]` rosters carry, `Error`, and `__version__`; the module's `_do_*` handlers, `data_format_args`, `add_data_format_args`, `find_data_offset_into_binfile`, and `parse_range` are `__main__` CLI plumbing, never library surface
- entry points: console script `detools`; library use is import-only
- capability: binary-delta patch creation across `sequential`/`in-place`/`bsdiff` patch types and `bsdiff`/`hdiffpatch`/`match-blocks` algorithms, `divsufsort`/`sais` suffix-array construction, `bz2`/`crle`/`lzma`/`zstd`/`lz4`/`heatshrink`/`none` compression, data-format-aware ELF/AArch64/Cortex-M4/Xtensa-LX106 segmentation, patch application from file-like or named-file inputs, and patch-container inspection returning a per-kind metadata record

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: failure root

`Error` (`class Error(Exception)`, `detools.errors`) is the sole public type and the common base for every `detools` failure — bad patch type, bad algorithm/patch-type combination, header read failure, compression mismatch, applied-image size mismatch, in-place dfpatch `NotImplementedError`. Patch type and algorithm are call-row strings on `create_patch`, never parallel patch classes.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :------- | :------------ | :----------------------------------------------- |
|  [01]   | `Error`  | error         | base failure for every `detools` patch operation |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: patch creation

`create_patch` is the single diff surface; `patch_type`, `algorithm`, `suffix_array_algorithm`, and `compression` are call rows whose `(algorithm, patch_type)` pair selects the create kernel. Valid pairs are `bsdiff`×`sequential`/`in-place`/`bsdiff`, `hdiffpatch`×`hdiffpatch`, and `match-blocks`×`sequential`/`hdiffpatch`; every other pair raises `Error`.

`use_mmap` is a boundary fact rather than a knob: a `BytesIO` ingress exposes no `fileno()`, so the `bsdiff`/`sequential` kernel falls back mmap->heap on `io.UnsupportedOperation` while the `hdiffpatch`/`match-blocks` kernels mmap with no heap fallback — an in-memory-buffer caller pins `use_mmap=False`.

```python signature
create_patch(
    ffrom, fto, fpatch,
    compression='lzma', patch_type='sequential', algorithm='bsdiff',
    suffix_array_algorithm='divsufsort',
    memory_size=None, segment_size=None, minimum_shift_size=None, data_format=None,
    from_data_offset_begin=0, from_data_offset_end=0, from_data_begin=0, from_data_end=0,
    from_code_begin=0, from_code_end=0,
    to_data_offset_begin=0, to_data_offset_end=0, to_data_begin=0, to_data_end=0,
    to_code_begin=0, to_code_end=0,
    match_score=6, match_block_size=64, use_mmap=True,
    heatshrink_window_sz2=8, heatshrink_lookahead_sz2=7,
)
```

| [INDEX] | [SURFACE]                | [CAPABILITY]                                            |
| :-----: | :----------------------- | :------------------------------------------------------ |
|  [01]   | `create_patch`           | diff `ffrom`->`fto`, write patch to `fpatch`            |
|  [02]   | `create_patch_filenames` | named-file form of `create_patch`, identical kwarg axis |

[ENTRYPOINT_SCOPE]: patch application

`apply_patch` peeks the header type and dispatches to the sequential or hdiffpatch reconstructor, so DELTA_BUNDLE apply routes through it for the self-describing `sequential`/`hdiffpatch` headers; `apply_patch_bsdiff` applies a raw headerless `BSDIFF40` patch and `apply_patch_in_place` mutates the memory image in place. Each returns the `int` size of the created to-data.

`apply_patch_in_place` writes INTO `fmem`, so the recovered to-image is the `to_size`-prefix `fmem.getvalue()[:to_size]`, never the full `memory_size`-padded buffer, and `to_size` binds before `getvalue()` because the call mutates in place.

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]                    | [CAPABILITY]                                       |
| :-----: | :------------------------------- | :------------------------------ | :------------------------------------------------- |
|  [01]   | `apply_patch`                    | `(ffrom, fpatch, fto) -> int`   | reconstruct `fto` (sequential/hdiffpatch dispatch) |
|  [02]   | `apply_patch_filenames`          | `(fromfile, patchfile, tofile)` | named-file form of `apply_patch`                   |
|  [03]   | `apply_patch_bsdiff`             | `(ffrom, fpatch, fto) -> int`   | apply a raw bsdiff patch                           |
|  [04]   | `apply_patch_bsdiff_filenames`   | `(fromfile, patchfile, tofile)` | named-file form of `apply_patch_bsdiff`            |
|  [05]   | `apply_patch_in_place`           | `(fmem, fpatch) -> int`         | apply an in-place patch, mutating `fmem`           |
|  [06]   | `apply_patch_in_place_filenames` | `(memfile, patchfile)`          | named-file form of `apply_patch_in_place`          |

[ENTRYPOINT_SCOPE]: patch inspection

`patch_info` peeks the header type and returns the patch-kind string paired with a per-kind info tuple; `fsize` is an optional size formatter (`humanfriendly.format_size`) the in-place segment summary applies.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                          | [CAPABILITY]                            |
| :-----: | :-------------------- | :------------------------------------ | :-------------------------------------- |
|  [01]   | `patch_info`          | `(fpatch, fsize=None) -> (str, info)` | report patch kind and per-type metadata |
|  [02]   | `patch_info_filename` | `(patchfile, fsize=None)`             | named-file form of `patch_info`         |

Per-kind `patch_info` info tuple (`detools.info`), the second element of the `(kind, info)` return:

- [01]-[SEQUENTIAL]: `(patch_size, compression, compression_info, dfpatch_size, data_format, dfpatch_info, *(to_size, diff_sizes, extra_sizes, …, number_of_size_bytes))`
- [02]-[IN_PLACE]: `(patch_size, compression, compression_info, memory_size, segment_size, shift_size, from_size, to_size, segments[(dfpatch_size, data_format, info)])` — `shift_size` is `detools.create.calc_shift`'s computed slide `max((memory_segments - from_segments) * segment_size, minimum_shift_size)`, where `memory_segments = memory_size // segment_size` (a `memory_size` not a multiple of `segment_size` raises `Error`), `from_segments = div_ceil(from_size, segment_size)`, and `minimum_shift_size` defaults to `2 * segment_size` and itself divides by `segment_size`; an audit re-derives the slide from the echoed sizes, never equality-checking the configured `minimum_shift_size`
- [03]-[HDIFFPATCH]: `(patch_size, compression, compression_info, to_size)`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `create_patch` owns diffing and one `apply_patch` owns reconstruction (peeking the header, dispatching sequential vs hdiffpatch); `apply_patch_bsdiff` and `apply_patch_in_place` are the headerless-bsdiff and in-place header KINDS, not parallel owners.
- `compression` selects the patch-payload codec at create time (`bz2`/`crle`/`lzma`/`zstd`/`lz4`/`heatshrink`/`none`); `apply_patch` resolves it from the header, never a parallel container.
- `memory_size`/`segment_size`/`minimum_shift_size` parameterize in-place segmentation as creation rows bound only under `patch_type='in-place'`; `data_format` selects firmware segmentation over the `from_*`/`to_*` offset rows.
- Every `_filenames`/`_filename` row is a thin named-file form over its file-like surface; one concept never carries both shapes.
- `import detools` binds at boundary scope only.

[STACKING]:
- `lz4`/`zstandard`(`.api/lz4.md`, `.api/zstandard.md`): the delta `compression` axis selects the SAME codec cores the `package/codec#CODEC` single-blob arms own — `detools` frames the bytes through its `detools.compression.*` adapter, the codec library does the work.
- `package/delta#DELTA`+`package/bundle#BUNDLE`+`core/receipt#RECEIPT`: the patch bytes are the single blob the `Bundle` `DELTA` row carries; the per-kind `patch_info` tuple and returned to-data size fold onto the `ArtifactReceipt.Bundle` case (`frame_size` <- `to_size`, `level`/`dict_id` zero because the tuple carries no single integer level) through the runtime `ReceiptContributor` port, never a parallel delta receipt.
- `runtime` `xxhash` (`python:runtime [CONTENT_KEY]` seam): `DeltaKnobs.parent_key: ContentKey` is the from-image content key; the recovered to-image keys `f"from-{parent_key.hex}"`, decoded not re-minted.
- firmware seam (`pyelftools`): for an ELF image, `data_format_from_files(option, elffile, binfile, offset)` reads the code/data ranges via `detools.data_format.elf.from_file` and feeds the `from_*`/`to_*` offsets into `create_patch(data_format=…)`; the dfpatch is self-describing in the sequential header, so apply re-supplies no offsets. Bind `data_format_from_files`, never the `add_data_format_args`/`data_format_args` CLI helpers.
- within-lib (`msgspec`/`anyio`): `DeltaKnobs`/`InPlaceSegments`/`FirmwareLayout` are `msgspec.Struct(frozen=True)` bands whose `kwargs()` project dense `tuple[int, int]` ranges onto the flat `detools` kwarg names at the edge (closed axes are `Literal`), keeping the profile content-key-foldable; the CPU-bound native diff runs through `anyio.to_thread.run_sync` (optionally under a `CapacityLimiter`) off the event loop.
- observability (`structlog`+`opentelemetry`): each delta pack opens an OTel span and binds `patch_type`/`algorithm`/`compression`/`verified`/`frame_size` from the `patch_info` record, the round-trip verdict `verified = int(recovered == payload)` a structured receipt fact.

[LOCAL_ADMISSION]:
- `detools` is the superset diff engine: its `bsdiff` algorithm and `apply_patch_bsdiff` subsume `bsdiff4`, and it adds `hdiffpatch`, in-place segmentation, and firmware data-format awareness — admit it as the single diff owner, a sibling codec only the payload compressor it selects.

[RAIL_LAW]:
- Package: `detools`
- Owns: binary-delta patch creation (bsdiff/hdiffpatch, sequential/in-place/bsdiff framing), patch application from file-like or named-file inputs, and patch-container inspection
- Accept: DELTA_BUNDLE create/apply and patch metadata feeding the artifacts persistence owner
- Reject: a hand-rolled bsdiff/hdiffpatch suffix-array diff or admitting `bsdiff4` where `detools` owns the superset; re-implementing the `lz4`/`zstd`/`heatshrink` codec `detools` selects; a parallel patch type or apply entrypoint per `patch_type`/`algorithm`/`compression` where the call rows and `apply_patch` header-dispatch already discriminate; binding `add_data_format_args`/`data_format_args` where `data_format_from_files` is the offset resolver; `use_mmap=True` on a `BytesIO` ingress; raising `detools.Error` across the codec boundary instead of lifting to `Result.Error`; running the native diff on the event loop; a parallel delta receipt instead of the `ArtifactReceipt.Bundle` case; re-minting the parent content key
