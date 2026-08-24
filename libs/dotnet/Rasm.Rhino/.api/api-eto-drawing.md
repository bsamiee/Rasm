# [RASM_RHINO_API_ETO_DRAWING]

`Eto.Drawing` is the immediate-mode paint surface behind every `Drawable` host and owner-drawn cell this boundary raises inside Rhino. The command stream, path geometry, brush and pen vocabulary, text layout, and raster staging are the branch algebra composed unchanged; this partition holds the projection law that keeps canonical geometry and perceptual colour inside the boundary and `Eto.Drawing` values at the render edge alone.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Eto.Drawing` — Rhino host-boundary partition
- package: `Eto` (host-provided; bound in-place from the Rhino-loaded `Eto.dll`, never a second NuGet admission) (BSD-3-Clause)
- assembly: `Eto.dll` (Rhino `RhCore` framework)
- namespace: `Eto.Drawing`
- asset: the same `Eto.dll` the `Eto.Forms` surface binds; `Graphics` handles render against the host's native canvas
- rail: paint

## [02]-[BOUNDARY_REACH]

- Registers the `Eto.Drawing` paint algebra (`libs/dotnet/.api/api-eto-drawing.md`): `Graphics` with its transform and clip stacks, `GraphicsPath`/`IGraphicsPath`, the brush, pen, and dash family, `Color` and its sRGB projections, `Font`/`FormattedText` layout, `Matrix` composition, `Bitmap`/`BitmapData`/`Image`, and the `Drawable` paint seam carry their algebra there. This partition adds no carrier of its own and states the boundary's composition law over the registered surface.

| [INDEX] | [BOUNDARY_CONCERN]         | [REGISTERED_MEMBERS]                                                                               |
| :-----: | :------------------------- | :------------------------------------------------------------------------------------------------- |
|  [01]   | owner-drawn cell and panel | `Drawable.Paint`, `PaintEventArgs.Graphics`, `PaintEventArgs.ClipRectangle`                        |
|  [02]   | off-event acquisition      | `Drawable.SupportsCreateGraphics`, `Drawable.CreateGraphics()`, `Drawable.Update(Rectangle)`       |
|  [03]   | in-place text editing      | `Drawable.TextComposition`, `CancelTextComposition()`, `CommitTextComposition()`                   |
|  [04]   | render-edge projection     | `Graphics` draw and fill commands, `GraphicsPath` construction and hit tests, `Matrix` composition |
|  [05]   | resource egress            | `Bitmap.Lock()`, `Bitmap.ToByteArray(ImageFormat)`, `Bitmap.Clone(Rectangle?)`                     |

## [03]-[IMPLEMENTATION_LAW]

[DRAWING_TOPOLOGY]:
- No retained scene exists at this boundary: a `Drawable` paint event hands a live handle, `CreateGraphics` acquires one off-event under its support flag, and the host re-issues the whole paint on invalidation, so a paint body is a pure function of boundary state and never a mutable draw-list the host replays.
- Hit-testing composes the same `GraphicsPath` the paint drew, so pointer resolution and rendering never diverge into two geometries.
- Canonical geometry and perceptual colour live inside this boundary; the paint owner projects to `Eto.Drawing` primitives at the render edge alone and `Eto.Drawing.*` types never leak past it.

[STACKING]:
- `libs/dotnet/.api/api-eto-drawing.md`: the registered algebra every paint body composes; this boundary adds no drawing carrier and re-tables none.
- `Wacton.Unicolour`(`libs/dotnet/.api/api-unicolour.md`): owns the perceptual colour model — perceptual blending, delta-E distance, gamut-mapped fills, and theme ramps route through `Unicolour`, and the registered sRGB colour survives only at the paint edge feeding a `Brush` or `Pen`.
- `LanguageExt.Core`(`libs/dotnet/.api/api-languageext.md`): a pixel lock rides an `Eff<A>`/`use` resource scope, `Fin<A>` rails an encode or decode, and `Seq<PointF>` is the vertex carrier a polyline or curve folds over.
- `Thinktecture.Runtime.Extensions`(`libs/dotnet/.api/api-thinktecture-runtime-extensions.md`): a `[ValueObject]` owns a validated stroke-style, dash-preset, or gradient-stop value; a `[SmartEnum]` owns the closed brush-kind and system-font-role vocabularies a generator-shaped paint layer folds to rows.
- `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-display.md`: viewport drawing through the Rhino display pipeline is the separate in-model surface; a `Drawable` paint never substitutes for a conduit and a conduit never substitutes for panel chrome.
- Kernel unification: easing, spring, and interpolation math positioning or animating a paint composes the `Rasm` kernel, never a second in-boundary derivation.

[LOCAL_ADMISSION]:
- `Eto.Drawing` binds the same Rhino-loaded `Eto.dll` as the forms surface, never a second NuGet copy; a `Graphics` handle comes only from a paint event or a support-gated `CreateGraphics`.
- Paint code holds canonical geometry and `Unicolour` colour internally and projects at the render edge; a domain signature carries neither an `Eto.Drawing` value nor a `Graphics` handle.

[RAIL_LAW]:
- Partition: `Eto.Drawing` Rhino host boundary — owner-drawn cell and panel painting over the registered branch algebra
- Owns: the projection law placing canonical geometry and perceptual colour inside the boundary and `Eto.Drawing` values at the render edge, and the no-retained-scene paint contract
- Accept: custom 2D painting behind a `Drawable`, path construction and hit-testing, text measurement and layout, image blit and pixel access, transform and clip state
- Reject: a re-tabling of the branch paint algebra, perceptual colour math (`Unicolour` owns it), widget construction and layout (`libs/dotnet/Rasm.Rhino/.api/api-eto-forms.md`), platform-handler selection (`libs/dotnet/Rasm.Rhino/.api/api-eto-platform.md`), host viewport drawing through the display pipeline (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-display.md`), and leaking `Eto.Drawing.*` types past the paint owner
