# [PY_ARTIFACTS_API_DETOOLS]

`detools` supplies the binary-delta surface for the artifacts DELTA_BUNDLE rail: a `create_patch` factory that diffs a from-image against a to-image and writes a compressed patch, plus an `apply_patch` family that reconstructs the to-image from the from-image and a patch. The package owner composes `create_patch`, `apply_patch`, and `patch_info` into the DELTA_BUNDLE create/apply path; it removes any hand-rolled bsdiff or hdiffpatch suffix-array diffing because the native `bsdiff`/`hdiffpatch`/`suffix_array` extensions are in-package, and it never re-implements the patch container framing, compression selection, or in-place segmentation `detools` already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `detools`
- package: `detools`
- import: `detools`
- owner: `artifacts`
- rail: delta
- installed: `0.53.0` reflected via `assay api` on cp315
- entry points: console script `detools` (CLI); library use is import-only
- capability: binary-delta patch creation across `sequential`/`in-place`/`bsdiff` patch types and `bsdiff`/`hdiffpatch` algorithms, `divsufsort`/`sais` suffix-array construction, `bz2`/`crle`/`lzma`/`zstd`/`lz4`/`none` compression, data-format-aware ELF/AArch64/Cortex-M4/Xtensa segmentation, patch application from file-like or named-file inputs, and patch-container inspection

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: failure root
- rail: delta

The patch surface is function-centric; the one public type is `Error`, the common base for every `detools` failure (bad patch type, header mismatch, compression mismatch, applied-image size mismatch). Patch type and algorithm are call-row strings on `create_patch`, never parallel patch classes.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL]                                          |
| :-----: | :------- | :------------ | :---------------------------------------------- |
|  [01]   | `Error`  | error         | base failure for all `detools` patch operations |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: patch creation
- rail: delta

`create_patch` is the single diff surface; `patch_type`, `algorithm`, `suffix_array_algorithm`, and `compression` are call rows that select sequential/in-place/bsdiff framing and bsdiff/hdiffpatch diffing, never per-mode builder types. The `_filenames` row opens the named files and forwards the identical keyword axis. `ffrom`/`fto`/`fpatch` are file-like objects.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                                                                                                                                                                                                                                                                                                                          | [CAPABILITY]                                 |
| :-----: | :----------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :------------------------------------------- |
|  [01]   | `create_patch`           | `create_patch(ffrom, fto, fpatch, compression='lzma', patch_type='sequential', algorithm='bsdiff', suffix_array_algorithm='divsufsort', memory_size=None, segment_size=None, minimum_shift_size=None, data_format=None, ..., match_score=6, match_block_size=64, use_mmap=True, heatshrink_window_sz2=8, heatshrink_lookahead_sz2=7)` | diff `ffrom`->`fto`, write patch to `fpatch` |
|  [02]   | `create_patch_filenames` | `create_patch_filenames(fromfile, tofile, patchfile, compression='lzma', patch_type='sequential', algorithm='bsdiff', suffix_array_algorithm='divsufsort', ...)`                                                                                                                                                                      | named-file form of `create_patch`            |

[ENTRYPOINT_SCOPE]: patch application
- rail: delta

`apply_patch` peeks the patch header type and dispatches to the sequential or hdiffpatch reconstructor; `apply_patch_bsdiff` applies a raw bsdiff patch; `apply_patch_in_place` mutates the memory image in place. Each `_filenames` row opens the named files and forwards to the file-like form. `apply_patch`/`apply_patch_bsdiff` return the size of the created to-data.

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]                                                | [CAPABILITY]                                       |
| :-----: | :------------------------------- | :---------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `apply_patch`                    | `apply_patch(ffrom, fpatch, fto)` -> `int`                  | reconstruct `fto` (sequential/hdiffpatch dispatch) |
|  [02]   | `apply_patch_filenames`          | `apply_patch_filenames(fromfile, patchfile, tofile)`        | named-file form of `apply_patch`                   |
|  [03]   | `apply_patch_bsdiff`             | `apply_patch_bsdiff(ffrom, fpatch, fto)` -> `int`           | apply a raw bsdiff patch                           |
|  [04]   | `apply_patch_bsdiff_filenames`   | `apply_patch_bsdiff_filenames(fromfile, patchfile, tofile)` | named-file form of `apply_patch_bsdiff`            |
|  [05]   | `apply_patch_in_place`           | `apply_patch_in_place(fmem, fpatch)`                        | apply an in-place patch, mutating the memory image |
|  [06]   | `apply_patch_in_place_filenames` | `apply_patch_in_place_filenames(memfile, patchfile)`        | named-file form of `apply_patch_in_place`          |

[ENTRYPOINT_SCOPE]: patch inspection
- rail: delta

`patch_info` peeks the header type and returns the patch kind string with a type-specific info record; `fsize` formats reported sizes. The `_filename` row opens the named file and forwards.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                      | [CAPABILITY]                            |
| :-----: | :-------------------- | :------------------------------------------------ | :-------------------------------------- |
|  [01]   | `patch_info`          | `patch_info(fpatch, fsize=None)` -> `(str, info)` | report patch kind and per-type metadata |
|  [02]   | `patch_info_filename` | `patch_info_filename(patchfile, fsize=None)`      | named-file form of `patch_info`         |

## [04]-[IMPLEMENTATION_LAW]

[DELTA_PATCH]:
- import: `import detools` at boundary scope only; module-level import is banned by the manifest import policy.
- create axis: one `create_patch` owns diffing; `patch_type` (`sequential`/`in-place`/`bsdiff`), `algorithm` (`sequential`/`hdiffpatch`), `suffix_array_algorithm` (`divsufsort`/`sais`), and `compression` (`bz2`/`crle`/`lzma`/`zstd`/`lz4`/`none`) are call rows, never a per-mode patch type — `create_patch_filenames` is the named-file row, not a parallel API.
- apply axis: `apply_patch` is the single reconstruction surface; it peeks the header type and dispatches sequential vs hdiffpatch internally, so callers route DELTA_BUNDLE apply through it rather than the algorithm-specific reconstructors; `apply_patch_bsdiff` is the bsdiff-only row and `apply_patch_in_place` is the in-place row, selected only when the patch kind forces it.
- filename axis: every `_filenames`/`_filename` variant is a thin named-file row over its file-like form; callers pass open file-like objects unless the named-file convenience is required, never both shapes for one concept.
- compression axis: `compression` selects the patch-payload codec row at create time; bz2/crle/lzma/zstd/lz4/none is a `compression` row, never a parallel patch container — `apply_patch` resolves the codec from the patch header.
- in-place axis: `memory_size`, `segment_size`, and `minimum_shift_size` parameterize in-place patch segmentation; they are creation rows used only when `patch_type='in-place'`, never a separate segmenting type.
- data-format axis: `data_format` selects ELF/AArch64/Cortex-M4/Xtensa-LX106 segmentation with the `from_*`/`to_*` offset rows; firmware-aware diffing is a `data_format` row, never a per-architecture patch surface.
- evidence: each operation captures patch type, algorithm, compression, suffix-array algorithm, created to-data size (from `apply_patch`), and patch-container metadata (from `patch_info`) as a delta receipt.
- boundary: detools owns binary-delta create/apply, patch-container framing, and patch inspection with the native `bsdiff`/`hdiffpatch`/`suffix_array` extensions; `Error` is the single failure rail mapped at the boundary; compression backends route to their owning libraries through detools, never re-implemented; the patch bytes feed the DELTA_BUNDLE persistence owner directly.

[RAIL_LAW]:
- Package: `detools`
- Owns: binary-delta patch creation (bsdiff/hdiffpatch, sequential/in-place/bsdiff framing), patch application from file-like or named-file inputs, and patch-container inspection
- Accept: DELTA_BUNDLE create/apply and patch metadata feeding the artifacts persistence owner
- Reject: wrapper-renames of `create_patch`/`apply_patch`; a hand-rolled bsdiff or hdiffpatch suffix-array diff; a parallel patch type per `patch_type`/`algorithm`/`compression`; a separate apply entrypoint per patch kind where `apply_patch` already dispatches on the header; carrying both file-like and named-file shapes for one operation
