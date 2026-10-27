# [FILES]

## [01]-[PARTS]

Multilayer EXR parts follow the stage that produced the image:

| [INDEX] | [CASE]                                   | [PARTS]                                                            |
| :-----: | :--------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | No enabled effect returns a color image  | `<layer>.<pass>` for each layer with `use` on                      |
|  [02]   | Last effect returns a color image        | `Composite.Combined`, then view layer parts in `view_layers` order |
|  [03]   | Strips in the editor, `use_sequencer` on | One unnamed part with channels `R`, `G`, `B`, `A`                  |

Interleaved files (`use_exr_interleave`) hold every channel in one part sorted by name, and reads pick a layer by name

Parts take pass display names, and channels take component suffixes in file order:
- Light passes write `<Light> Direct`, `<Light> Indirect`, and `<Light> Color` (`Diffuse Color`)
- `use_pass_z` writes `Depth`, and `use_pass_emit` writes `Emission`
- Color passes write `R`, `G`, `B`, and `Combined` adds `A`
- `Depth`, `Mist`, and Cycles `Volume Majorant` write `Z`, `Position` and `Normal` `X` to `Z`, `Vector` `X` to `W`, and `UV` `A`, `U`, `V`
- Index passes and `VALUE` AOVs write `X`, `COLOR` AOVs `R`, `G`, `B`, `A`
- Light group parts `Combined_<group>` and Cycles `Shadow Catcher` write `R`, `G`, `B`

## [02]-[CRYPTOMATTE]

Cryptomatte parts `<layer>.CryptoObjectNN`, `CryptoMaterialNN`, and `CryptoAssetNN` hold channels `r`, `g`, `b`, `a`:
- Parts hold ranks as (id, coverage) pairs in `r`, `g` and `b`, `a`, depth 6 filling `CryptoObject00` to `02`
- Headers hold `cryptomatte/<key>/name` naming `<layer>.<Kind>`, `manifest` as a JSON map of names to hex hashes, `hash` `MurmurHash3_32`, and `conversion` `uint32_to_float32`
- Cycles manifests list lights, the world, and default materials, EEVEE manifests items it drew

Mattes match manifest hashes as float ids that keep sign and mantissa bits and clamp the exponent to 1 through 254:

```python
from string import digits

from expression import Error, Option, Result
import msgspec
import numpy as np
from numpy.typing import NDArray
import OpenImageIO


def matted(path: str, layer: str, names: frozenset[str]) -> Result[NDArray[np.float32], OSError | KeyError]:
    def summed(opened: OpenImageIO.ImageBuf, manifest: dict[str, str]) -> NDArray[np.float32]:
        hashes = np.array([int(manifest[name], 16) for name in names], np.uint32)
        parts = (OpenImageIO.ImageBuf(path, index, 0) for index in range(opened.nsubimages))
        ranks = np.concatenate([part.get_pixels(OpenImageIO.FLOAT) for part in parts if part.spec().getattribute("name").rstrip(digits) == layer], axis=-1)
        matte: NDArray[np.float32] = np.where(np.isin(ranks.view(np.uint32)[..., ::2], hashes & 0x807FFFFF | np.clip(hashes >> 23 & 0xFF, 1, 254) << 23), ranks[..., 1::2], 0).sum(axis=-1)
        return matte

    def manifested(header: OpenImageIO.ImageSpec) -> Result[dict[str, str], OSError | KeyError]:
        return (
            Option
            .of_optional(next((attribute.name.removesuffix("/name") for attribute in header.extra_attribs if attribute.name.endswith("/name") and attribute.value == layer), None))
            .to_result(KeyError(layer))
            .map(lambda key: msgspec.json.decode(header.getattribute(f"{key}/manifest"), type=dict[str, str]))
        )

    match OpenImageIO.ImageBuf(path):
        case OpenImageIO.ImageBuf(has_error=True) as failed:
            return Error(OSError(failed.geterror()))
        case opened:
            return (
                Result[OpenImageIO.ImageSpec, OSError | KeyError](ok=opened.spec())
                .bind(manifested)
                .filter_with(lambda manifest: names <= manifest.keys(), lambda manifest: KeyError(*sorted(names.difference(manifest))))
                .map(lambda manifest: summed(opened, manifest))
            )
```

## [03]-[MULTI_VIEW]

Multi-view EXR files place views by `image_settings.views_format`:
- `MULTIVIEW` parts hold a `view` attribute
- Multilayer multi-view parts are `<layer>.<pass>-<view>`, channels keeping `<layer>.<pass>.<channel>`

## [04]-[FILE_OUTPUT]

File Output parts and channels take item names and socket types:
- Multilayer parts take the item name with no layer prefix (`beauty`), channels `<item>.<channel>`
- `RGBA` items write `R`, `G`, `B`, `A`, `FLOAT` items `V`, and `VECTOR` items `X` to the item's `vector_socket_dimensions`
- Items fed a cryptomatte result write lowercase `r`, `g`, `b`, `a`
- Multilayer files fill an unlinked single-value input across the frame, and single-layer files skip it

## [05]-[METADATA]

Headers hold render metadata:
- `use_stamp_*` flags write header keys whether or not `use_stamp` burns them into pixels
- Default flags write `Camera`, `Date`, `File`, `Frame`, `RenderTime`, `Scene`, and `Time`
- `use_stamp_note` writes `stamp_note_text` as `Note`
- Cycles adds `cycles.<layer>.samples`, `render_time`, `synchronization_time`, and `total_time`
- EXR headers name the written linear space in `colorInteropID` (`lin_rec709_scene`, `lin_ap1_scene` under an `ACEScg` override)
- OpenImageIO reads EXR `colorInteropID` and PNG `CICP` as `oiio:ColorSpace` (`srgb_rec709_scene` under `sRGB`, `pq_rec2020_display` under `Rec.2100-PQ`)

## [06]-[OPENIMAGEIO]

Bundled `OpenImageIO` reads written files and writes images through file paths, its Python module holding no `IOProxy`:
- `OpenImageIO.ImageBuf(path, part, 0)` opens one multilayer part, and `OpenImageIO.ImageBuf(path).nsubimages` counts parts
- Missing or unreadable files open with `nsubimages` 0 and `has_error` set, `geterror()` reading the cause OpenImageIO otherwise prints at teardown
- `spec().getattribute("name")` names a part `<layer>.<pass>`, split at its last dot, and `spec().channelnames` hold `<layer>.<pass>.<channel>`
- `get_pixels(OpenImageIO.FLOAT)` returns a `(height, width, channels)` array, top row first
- `OpenImageIO.get_string_attribute("output_format_list")` lists the build's writers, Blender's build holding no JPEG XL, PSD, DDS, or raw writer
- `ImageOutput.supports("cicp")` holds for PNG and HEIF writers alone
- JPEG writes read `CompressionQuality` and `jpeg:subsampling` (`4:4:4`, `4:2:2`, `4:2:0`, `4:1:1`) from the spec, `4:2:1` writing `4:2:0`
- JPEG reads report no `CompressionQuality`
