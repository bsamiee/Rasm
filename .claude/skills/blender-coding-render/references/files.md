# [FILES]

## [01]-[PARTS]

Multilayer EXR parts follow the stage that produced the image:

| [INDEX] | [CASE]                                   | [PARTS]                                                            |
| :-----: | :--------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | No enabled effect returns a color image  | `<layer>.<pass>` for each layer with `use` on                      |
|  [02]   | Last effect returns a color image        | `Composite.Combined`, then view layer parts in `view_layers` order |
|  [03]   | Strips in the editor, `use_sequencer` on | One unnamed part with channels `R`, `G`, `B`, `A`                  |

Parts take pass display names, and channels take component suffixes in file order:
- Light passes write `<Light> Direct`, `<Light> Indirect`, and `<Light> Color` (`Diffuse Color`)
- `use_pass_z` writes `Depth`, and `use_pass_emit` writes `Emission`
- Color passes write `R`, `G`, `B`, and `Combined` adds `A`
- `Depth`, `Mist`, and Cycles `Volume Majorant` write `Z`, `Position` and `Normal` `X` to `Z`, `Vector` `X` to `W`, and `UV` `A`, `U`, `V`
- Index passes and `VALUE` AOVs write `X`, `COLOR` AOVs `R`, `G`, `B`, `A`
- Light group parts `Combined_<group>` and Cycles `Shadow Catcher` write `R`, `G`, `B`
- Multi-view parts are `<layer>.<pass>-<view>` with channels keeping `<layer>.<pass>.<channel>`, and `MULTIVIEW` parts hold a `view` attribute
- `use_exr_interleave` writes one part named `""` holding every `<layer>.<pass>.<channel>` sorted by part name
- Interleaved reads pick channels by `spec().channelnames` index

`parts` keys each part's `ImageBuf` by its name in file order, an unreadable file failing once:

```python
def parts(path: Path, /) -> Outcome[dict[str, OpenImageIO.ImageBuf]]:
    match OpenImageIO.ImageBuf(str(path)):
        case OpenImageIO.ImageBuf(has_error=True) as failed:
            return Error(Fault.of(Reason.UNREADABLE, str(path), failed.geterror()))
        case opened:
            held = (OpenImageIO.ImageBuf(str(path), index, 0) for index in range(opened.nsubimages))
            return Ok({part.spec().get_string_attribute("name"): part for part in held})
```

`parts(path).bind(lambda held: Option.of_optional(held.get(name)).to_result_with(partial(Fault.of, Reason.ABSENT, str(path), member=name)))` reads a pass render settings can skip, each buffer reading pixels at `get_pixels`:
- `get_pixels(OpenImageIO.FLOAT)` returns `(height, width, channels)` top row first, and `np.flipud` gives `Image.pixels` row order
- Missing files open with `has_error` set and `nsubimages` 0, `geterror()` reading the cause OpenImageIO otherwise prints at teardown

## [02]-[CRYPTOMATTE]

Cryptomatte parts `<layer>.CryptoObjectNN`, `CryptoMaterialNN`, and `CryptoAssetNN` hold channels `r`, `g`, `b`, `a`:
- Parts hold ranks as (id, coverage) pairs in `r`, `g` and `b`, `a`, depth 6 filling `CryptoObject00` to `02`
- First part headers hold `cryptomatte/<key>/name` (`<layer>.<Kind>`), `hash` (`MurmurHash3_32`), and `conversion` (`uint32_to_float32`)
- `cryptomatte/<key>/manifest` holds a JSON map of names to hex hashes
- Cycles manifests list lights, the world, and default materials, EEVEE manifests items it drew

Manifests hold raw hashes, and pixels hold float ids keeping sign and mantissa bits with the exponent clamped to 1 through 254. `matte` checks the layer and every name against the first part's header before reading a rank:

```python
def matte(path: Path, layer: str, names: frozenset[str], /) -> Outcome[NDArray[np.float32]]:
    def summed(held: dict[str, OpenImageIO.ImageBuf], manifest: dict[str, str], /) -> NDArray[np.float32]:
        hashes = np.array([int(manifest[name], 16) for name in names], np.uint32)
        ranks = np.concatenate([part.get_pixels(OpenImageIO.FLOAT) for key, part in held.items() if re.fullmatch(rf"{re.escape(layer)}\d+", key)], axis=-1)
        return np.where(np.isin(ranks.view(np.uint32)[..., ::2], hashes & 0x807FFFFF | np.clip(hashes >> 23 & 0xFF, 1, 254) << 23), ranks[..., 1::2], 0).sum(axis=-1)

    def manifested(held: dict[str, OpenImageIO.ImageBuf], /) -> Outcome[NDArray[np.float32]]:
        header = next(iter(held.values())).spec()
        return (
            Option.of_optional(next((attribute.name for attribute in header.extra_attribs if attribute.name.startswith("cryptomatte/") and attribute.value == layer), None))
            .to_result_with(partial(Fault.of, Reason.ABSENT, str(path), member=layer))
            .map(lambda key: msgspec.json.decode(header.get_string_attribute(key.replace("/name", "/manifest")), type=dict[str, str]))
            .filter_with(lambda manifest: names <= manifest.keys(), lambda manifest: Fault.of(Reason.ABSENT, layer, member=", ".join(sorted(names - manifest.keys()))))
            .map(partial(summed, held))
        )

    return parts(path).bind(manifested)
```

`matte(path, "ViewLayer.CryptoObject", frozenset({"Cube"}))` returns the cube's coverage top row first. Layers the file lacks fail naming the file, and names the manifest lacks fail naming the layer.

## [03]-[FILE_OUTPUT]

File Output parts and channels take item names and socket types:
- Multilayer parts take the item name with no layer prefix (`beauty`), channels `<item>.<channel>`
- `RGBA` items write `R`, `G`, `B`, `A`, `FLOAT` items `V`, and `VECTOR` items `X` to the item's `vector_socket_dimensions`
- Items fed a cryptomatte result write lowercase `r`, `g`, `b`, `a`
- Multilayer files fill an unlinked single-value input across the frame, and single-layer files skip it

## [04]-[METADATA]

Render metadata sits in the first part's header alone, every later part holding format attributes:
- `use_stamp_*` flags write header keys whether or not `use_stamp` burns them into pixels
- Default flags write `Camera`, `Date`, `File`, `Frame`, `RenderTime`, `Scene`, and `Time`
- `use_stamp_note` writes `stamp_note_text` as `Note`
- Cycles adds `cycles.<layer>.samples`, `render_time`, `synchronization_time`, and `total_time`
- EXR headers name the written linear space in `colorInteropID` (`lin_rec709_scene`, `lin_ap1_scene` under an `ACEScg` override)
- OpenImageIO reads `colorInteropID` and PNG `CICP` as `oiio:ColorSpace` (`srgb_rec709_scene` under `sRGB`, `pq_rec2020_display` under `Rec.2100-PQ`)

## [05]-[WRITES]

Bundled `OpenImageIO` writes images through file paths, its Python module holding no `IOProxy`:
- `OpenImageIO.get_string_attribute("output_format_list")` lists the build's writers, Blender's build holding no JPEG XL, PSD, DDS, or raw writer
- `ImageOutput.supports("cicp")` holds for PNG and HEIF writers alone
- JPEG writes read `CompressionQuality` and `jpeg:subsampling` (`4:4:4`, `4:2:2`, `4:2:0`, `4:1:1`) from the spec, `4:2:1` writing `4:2:0`
- JPEG reads report no `CompressionQuality`
