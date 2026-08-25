# [PY_GEOMETRY_API_PILLOW]

`pillow` (`PIL`) is geometry's WebP decoder, admitted for one role the interpreter floor cannot serve: the SOG v2 gaussian-splat container stores each channel as a lossless-WebP plane, and stdlib ships no WebP codec. Geometry's scan owner binds the header-only probe (`mode`/`size`/`getbands` before any decode), the `convert` band narrowing, and the `__array_interface__` copy `numpy` reads — never a raster operation, an encode, or a colour transform, each of which belongs to `artifacts`.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: image owner, lazy decode subtype, and the fault family

`Image.Image` is the one mutable pixel-buffer owner and `mode` a string row (`RGB`/`RGBA`/`L`/`P`/…), never a per-format image type; `open` returns the `ImageFile` subtype whose decode defers until a pixel is read, so header facts cost no decode. `UnidentifiedImageError` subclasses `OSError`, so the narrower arm reads first wherever both are caught.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                                              |
| :-----: | :------------------------------ | :------------ | :------------------------------------------------------------------------ |
|  [01]   | `Image.Image`                   | class         | the mutable pixel buffer; `mode`/`size`/`convert`/`getbands` ride it      |
|  [02]   | `ImageFile.ImageFile`           | class         | `open`-returned lazy image; header parsed, pixels deferred until `load`   |
|  [03]   | `WebPImagePlugin.WebPImageFile` | class         | the `ImageFile` subtype a WebP payload resolves to; `format` reads `WEBP` |
|  [04]   | `Image.UnidentifiedImageError`  | fault         | payload no registered plugin claims; subclass of `OSError`                |
|  [05]   | `Image.DecompressionBombError`  | fault         | declared pixel count past `Image.MAX_IMAGE_PIXELS`; a bare `Exception`    |
|  [06]   | `Image.MAX_IMAGE_PIXELS`        | policy value  | the bomb ceiling the header check gates on; `None` disables it            |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: decode, header read, and the numeric hand-off

`open` is the single lazy decode factory across every plugin, keyed by header bytes; `formats` restricts the probe to a named plugin set, so an arbitrary payload never walks the whole registry. `mode`, `size`, `format`, and `getbands()` all answer off the parsed header with no pixel decode — the band admission runs before any cost is paid — while `convert`, `load`, and `np.asarray` each force it. `Image` is a context manager, and `convert` returns a NEW image, so the converted result must be taken inside the window.

| [INDEX] | [SURFACE]                     | [SHAPE]  | [CAPABILITY]                                                          |
| :-----: | :---------------------------- | :------- | :-------------------------------------------------------------------- |
|  [01]   | `Image.open(fp, mode, *, …)`  | static   | lazy decode factory over a path or binary stream; `formats` pins them |
|  [02]   | `img.mode` / `img.format`     | property | band-layout string / registry format key, both header-only            |
|  [03]   | `img.size` / `width`/`height` | property | `(width, height)` and its scalars, header-only                        |
|  [04]   | `img.getbands()`              | instance | band-name tuple (`('R','G','B','A')`), header-only                    |
|  [05]   | `img.convert(mode, …)`        | instance | mode conversion returning a NEW image; forces the decode              |
|  [06]   | `img.load()`                  | instance | force the decode and return the pixel accessor                        |
|  [07]   | `img.__array_interface__`     | property | the NumPy v3 interface `np.asarray` reads; `data` is owned `bytes`    |
|  [08]   | `img.info`                    | property | per-plugin decode metadata dict; carries NO losslessness flag         |
|  [09]   | `img.__enter__`/`__exit__`    | instance | context manager closing the underlying file                           |
|  [10]   | `features.check(name)`        | static   | probe the optional native surface; `None`/`False` for an unknown name |
|  [11]   | `features.check_module(name)` | static   | the exact module probe — `check_module("webp")` is the codec's own    |

- `Image.open`: `formats` accepts a list or tuple of registry keys; an unmatched payload raises `UnidentifiedImageError` rather than returning a sentinel.
- `img.__array_interface__`: `data` is a `bytes` copy the array's base holds, not a view onto the decoder buffer, so the array outlives the closed image.
- `features.check_codec("webp")`: raises `ValueError` — WebP registers as a MODULE, not a codec, so the codec probe is the wrong seam.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- import: `lazy from PIL import Image, UnidentifiedImageError` at module scope — a `lazy` statement inside a function body is a SyntaxError — behind the owner's `find_spec` floor gate, so an unprovisioned host refuses by module name ahead of the offload instead of dying as an import inside a worker.
- open: one `Image.open(BytesIO(payload), formats=…)` per plane; the payload is already-resident octets, never a path, so no filesystem lifetime crosses the seam and the archive handle that produced them is closed before the decode runs.
- admission: `getbands()` proves the band count a consumer needs BEFORE the decode, because a lossless WebP written without alpha opens `RGB` and a fixed four-band read indexes past its last band; `convert` then narrows a wider source to the declared mode, so one canonical band regime crosses regardless of what the encoder wrote.
- fault: `UnidentifiedImageError`, `DecompressionBombError`, and the bare `OSError` a truncated payload raises map to distinct closed cases at the incurring read, the `OSError` subclass reading before its base; a bare `except Exception` around a decode is the deleted form.
- bomb: `open` raises `DecompressionBombError` off the DECLARED dimensions, before any pixel decodes and past twice `MAX_IMAGE_PIXELS`, and it descends from `Exception` rather than `OSError` — an except chain built on `OSError` alone lets a hostile header cross unconverted; between one and two times the ceiling `open` emits `DecompressionBombWarning` and returns normally, so only an escalating warning filter turns that band into a raise.
- lossless: `info` carries no losslessness flag and the RIFF chunk tag is unreliable under the extended container, so a lossy plane decodes silently into wrong quantization indices — losslessness is the CONTAINER's encoder contract, admitted as an assumption and stated as one, never probed.
- numeric: `np.asarray(img)` is the one hand-off, yielding `(height, width, bands)` `uint8`; the interface hands over an owned `bytes` copy, so the array survives the closed image and no borrowed decoder buffer escapes the window.
- provision: `features.check_module("webp")` is the exact native-build probe; `features.check` spans features, modules, and codecs and answers `False` with a `UserWarning` for a name none carries, while `features.check_codec("webp")` raises outright.

[STACKING]:
- `numpy`(`python/.api/numpy.md`): the decoded plane crosses as `np.asarray(image)` `(H, W, bands)` `uint8`, then `reshape(-1, bands)` gives the container's own `i = x + y * W` splat order directly; per-channel codebook lookup is one fancy index of the plane's `uint8` values into a `(256,)` `float32` table, so no per-pixel Python touches the decode.
- `expression`(`python/.api/expression.md`): the archive's member map is a `Map[str, bytes]` and each plane read is a `try_find` whose absent case lands on the owner's typed `malformed` refusal, never a `KeyError` crossing the worker seam.
- geometry scan owner: `scan/ingestion#INGESTION` `_plane` is the sole binding site — it opens the member octets, admits the band count, narrows the mode, and returns the array; `_rows` flattens it to the per-splat block the SOG channel fold consumes. Stdlib `zipfile` is the pairing that produces those octets: `ZipFile(BytesIO(raw))` reads every declared member inside one window into the member map, so no archive handle and no decoder buffer outlives its seam.

[LOCAL_ADMISSION]:
- pillow enters geometry for WebP plane decode alone, because the SOG v2 container has no other reader and stdlib carries no WebP codec; raster processing, encode, drawing, text, colour management, and every other pillow surface belong to `artifacts` (`python/artifacts/.api/pillow.md`) and never cross into this branch.
- geometry's floor gate resolves this module per CONTAINER rather than per verb: an SPZ payload decodes on the interpreter floor alone, so the image module is demanded only where the payload's leading signature names a planar container.
