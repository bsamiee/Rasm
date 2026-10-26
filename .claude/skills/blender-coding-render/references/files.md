# [FILES]

Covers written render files, from multilayer parts and channels to cryptomatte layouts, multi-view parts, File Output files, header metadata, and reads.

## [01]-[PARTS]

Multilayer EXR parts follow which stage produced the image:

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
- Parts hold ranks as (id, coverage) pairs in `r`, `g` and `b`, `a`
- `pass_cryptomatte_depth` ranks fill `CryptoObject00` to `02` at the default 6
- Headers hold `cryptomatte/<key>/name` naming `<layer>.<Kind>`, `manifest` as a JSON map of names to hex hashes, `hash` `MurmurHash3_32`, and `conversion` `uint32_to_float32`
- Cycles manifests list lights, the world, and default materials, EEVEE manifests items it drew
- Hashes become ids by keeping sign and mantissa bits and clamping the exponent to 1 through 254

```python
from string import digits

from expression import Error, Option, Result
import msgspec
import numpy as np
from numpy.typing import NDArray
import OpenImageIO


class Unreadable(msgspec.Struct, frozen=True, kw_only=True):
    path: str
    reason: str


class Unlayered(msgspec.Struct, frozen=True, kw_only=True):
    layer: str


class Unlisted(msgspec.Struct, frozen=True, kw_only=True):
    names: frozenset[str]


def coverage(path: str, layer: str, names: frozenset[str]) -> Result[NDArray[np.float32], Unreadable | Unlayered | Unlisted]:
    def matted(opened: OpenImageIO.ImageBuf, manifest: dict[str, str]) -> NDArray[np.float32]:
        hashes = np.array([int(manifest[name], 16) for name in names], np.uint32)
        parts = (OpenImageIO.ImageBuf(path, index, 0) for index in range(opened.nsubimages))
        ranks = np.concatenate([part.get_pixels(OpenImageIO.FLOAT) for part in parts if part.spec().getattribute("name").rstrip(digits) == layer], axis=-1)
        return np.where(np.isin(ranks.view(np.uint32)[..., ::2], hashes & 0x807FFFFF | np.clip(hashes >> 23 & 0xFF, 1, 254) << 23), ranks[..., 1::2], 0).sum(axis=-1)

    def manifested(header: OpenImageIO.ImageSpec) -> Result[dict[str, str], Unlayered]:
        return (
            Option
            .of_optional(next((held.name.removesuffix("/name") for held in header.extra_attribs if held.name.endswith("/name") and held.value == layer), None))
            .to_result(Unlayered(layer=layer))
            .map(lambda key: msgspec.json.decode(header.getattribute(f"{key}/manifest"), type=dict[str, str]))
        )

    match OpenImageIO.ImageBuf(path):
        case OpenImageIO.ImageBuf(has_error=True) as failed:
            return Error(Unreadable(path=path, reason=failed.geterror()))
        case opened:
            return (
                Result[OpenImageIO.ImageSpec, Unlayered | Unlisted](ok=opened.spec())
                .bind(manifested)
                .filter_with(lambda manifest: names <= manifest.keys(), lambda manifest: Unlisted(names=names.difference(manifest)))
                .map(lambda manifest: matted(opened, manifest))
            )
```

## [03]-[MULTI_VIEW]

Multi-view EXR files place views by `image_settings.views_format`:
- `MULTIVIEW` parts hold a `view` attribute
- Multilayer multi-view parts are `<layer>.<pass>-<view>`, channels keeping `<layer>.<pass>.<channel>`

## [04]-[FILE_OUTPUT]

File Output nodes write one part or file per item:
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
- EXR headers name the written linear space in `colorInteropID` (`lin_rec709_scene`, `lin_ap1_scene` under an `ACEScg` override), which OpenImageIO reads as `oiio:ColorSpace`
- PNG files hold `CICP`, which OpenImageIO reads as `oiio:ColorSpace` (`srgb_rec709_scene` under `sRGB`, `pq_rec2020_display` under `Rec.2100-PQ`)

## [06]-[READS]

Bundled `OpenImageIO` reads written files:
- `OpenImageIO.ImageBuf(path, part, 0)` opens one multilayer part, and `OpenImageIO.ImageBuf(path).nsubimages` counts parts
- Missing or unreadable files open with `nsubimages` 0 and `has_error` set, `geterror()` reading the cause OpenImageIO otherwise prints at teardown
- `spec().getattribute("name")` names a part `<layer>.<pass>`, split at its last dot, and `spec().channelnames` hold `<layer>.<pass>.<channel>`
- `get_pixels(OpenImageIO.FLOAT)` returns a `(height, width, channels)` array, top row first
