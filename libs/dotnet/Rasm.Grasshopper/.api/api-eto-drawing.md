# [RASM_GRASSHOPPER_API_ETO_DRAWING]

`Eto.Drawing` paints every Grasshopper2 canvas painter, wire renderer, icon projector, and tooltip painter. Immediate `Graphics` stream, `GraphicsPath` geometry, the brush/pen family, measured text, and `Bitmap` staging are the branch algebra this boundary composes unchanged; the rows below are the indexed-raster, hex-egress, and font-enumeration carriers the GH2 chrome adds beyond it.

## [01]-[PUBLIC_TYPES]

- Registers the `Eto.Drawing` paint algebra (`libs/dotnet/.api/api-eto-drawing.md`): `Graphics` with its transform and clip stacks, `GraphicsPath`/`IGraphicsPath`, the brush, pen, and dash family, `Color` and its space projections, `Font`/`FormattedText` layout, `Matrix` composition, `Bitmap`/`BitmapData`/`Image`, and the `Drawable` paint hook carry their algebra there and this boundary draws through that spelling; the rows below are the carriers this partition adds beyond it.

[PUBLIC_TYPE_SCOPE]: composite clip and palette-indexed raster

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                       |
| :-----: | :-------------- | :------------ | :------------------------------------------------- |
|  [01]   | `Region`        | class         | composite clip region beyond a single rect or path |
|  [02]   | `IndexedBitmap` | class         | palette-indexed raster                             |
|  [03]   | `Palette`       | class         | indexed-colour table                               |

[PUBLIC_TYPE_SCOPE]: hex egress and font enumeration

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :------------- | :------------ | :---------------------------------------- |
|  [01]   | `ColorStyles`  | flags enum    | hex-render policy, never a palette source |
|  [02]   | `SystemFont`   | enum          | host semantic font role                   |
|  [03]   | `FontFamily`   | class         | family over its `FontTypeface` set        |
|  [04]   | `FontTypeface` | class         | concrete weight and slant face            |

- `ColorStyles` is `[Flags]` over `None=0`, `ExcludeAlpha=1`, `AlphaLast=4`, `ShortHex=8`, `All=0xD` — it shapes colour-to-hex-string rendering and supplies no colour value, so it is a text-egress policy row and never a chrome-palette source.
- `FontFamily` enumerates its `FontTypeface` set, so a weight or slant the canvas requests resolves against the installed family rather than a hand-spelled face name.

## [02]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Region` is the one composite clip: a canvas clipping to a union or difference of areas builds the region and hands it to the branch clip stack, never nesting `SetClip` calls to emulate the composition.
- `IndexedBitmap` and `Palette` are the palette-indexed staging path beside the branch `Format32bppRgba` raster; a palette swap re-colours a whole sprite without touching pixel memory.
- `ColorStyles` governs hex egress only — a swatch value reads from the branch colour surface or `SystemColors`, never from a hex round trip.

[STACKING]:
- `api-eto-drawing`(`libs/dotnet/.api/api-eto-drawing.md`): the paint algebra every row here decorates — a `Region` enters the branch clip stack, and an `IndexedBitmap` projects to the branch raster at the blit edge; the `Icon`/`IconFrame` projection pair carries its rows there, composed through the kernel asset module rather than this boundary.
- `api-eto-forms`(`libs/dotnet/Rasm.Grasshopper/.api/api-eto-forms.md`): the control lifecycle raises the paint event whose `Graphics` this boundary draws through.
- `api-eto-platform`(`libs/dotnet/Rasm.Grasshopper/.api/api-eto-platform.md`): the managed paint objects back onto CoreGraphics through the `Eto.Mac.Drawing` handler set; a curved-stroke or text-state operation the managed path leaves ambiguous resolves on the `api-macos-native` `CGPath`/`CATextLayer` branch under platform gate.
- `api-thinktecture-runtime-extensions`(`libs/dotnet/.api/api-thinktecture-runtime-extensions.md`): `ColorStyles` and `SystemFont` map onto `[SmartEnum]` and flag owners so an egress policy or a semantic font role carries behaviour rather than a bare host enum switch.
- `api-languageext`(`libs/dotnet/.api/api-languageext.md`): an unresolved `FontFamily` face lookup lowers onto `Option<T>` at the folder boundary.

[LOCAL_ADMISSION]:
- Composite clipping enters through `Region`, icon projection through `Icon`/`IconFrame`, and palette staging through `IndexedBitmap`/`Palette`; a hand-rolled clip union, a per-scale icon field, or a manual palette walk is the deleted form.
- Every paint operation past these carriers runs on the registered branch algebra — a painter never re-derives a `Graphics` command, a path figure, or a colour projection here.
