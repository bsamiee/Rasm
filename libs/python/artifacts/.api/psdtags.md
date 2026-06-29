# psdtags

## Owned Surface

| Capability | API | Artifact Owner |
|---|---|---|
| Photoshop TIFF image resources | `psdtags.TiffImageResources`, `TiffImageResources.fromtiff`, `TiffImageResources.tifftag` | `.planning/export/layered.md` |
| Photoshop TIFF layer and mask source data | `psdtags.TiffImageSourceData`, `TiffImageSourceData.fromtiff`, `TiffImageSourceData.tifftag` | `.planning/export/layered.md` |
| Photoshop layer records | `psdtags.PsdLayer`, `psdtags.PsdLayers` | `.planning/export/layered.md` |
| Photoshop layer geometry | `psdtags.PsdRectangle` | `.planning/export/layered.md` |
| Photoshop layer channel payloads | `psdtags.PsdChannel`, `psdtags.PsdChannelId.CHANNEL0`, `CHANNEL1`, `CHANNEL2`, `TRANSPARENCY_MASK` | `.planning/export/layered.md` |
| Photoshop blend and layer flags | `psdtags.PsdBlendMode`, `psdtags.PsdLayerFlag` | `.planning/export/layered.md` |
| Photoshop TIFF container enums | `psdtags.PsdFormat.BE32BIT`, `psdtags.PsdKey.LAYER`, `psdtags.PsdUserMask` | `.planning/export/layered.md` |

## Integration Rules

- `export/layered` owns `psdtags`; graphic, composition, document, and receipt layers consume only local `Layer`, `LayerFact`, and `ArtifactReceipt` projections.
- Photoshop-compatible TIFF output writes `TiffImageSourceData.tifftag(...)` and `TiffImageResources.tifftag()` through the `tifffile` extratag boundary.
- The `TiffImageSourceData.byteorder` and photometric contract must stay aligned with the TIFF writer call; mismatched provider state never crosses into local facts.
