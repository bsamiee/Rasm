# [RASM_MATERIALS_API_UNICOLOUR_DATASETS]

`Wacton.Unicolour.Datasets` supplies named-colour lists, ColorChecker and Macbeth reference sets, perceptual colourmaps, and academic reference datasets as static `Unicolour` and `Pigment` tables for material colour validation. It carries no observer CMFs, illuminant SPDs, or generic reflectance; spectral upsampling and measured-spectral construction stay on the main `Wacton.Unicolour` colour owner.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Wacton.Unicolour.Datasets`
- package: `Wacton.Unicolour.Datasets`
- license: MIT (`github.com/waacton/Unicolour`)
- assembly: `Wacton.Unicolour.Datasets`
- namespace: `Wacton.Unicolour.Datasets`
- asset: runtime library (`netstandard2.0` single-TFM; consumer `net10.0` binds the netstandard2.0 asset). Transitively depends on `Wacton.Unicolour` (the `Unicolour`/`Pigment`/`Configuration` carriers) — pure-managed, ALC-safe, no native or extra transitive assets.
- rail: colour-datasets

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: reference and named-colour set carriers
- rail: colour-datasets

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
|:-----: |:--------------- |:---------------- |:------------------------------------------------ |
| [01] | `Macbeth` | reference set | 24-patch ColorChecker / Macbeth `Unicolour` table |
| [02] | `MacAdam` | reference set | MacAdam optimal-colour limit loci as `Unicolour` |
| [03] | `EbnerFairchild` | reference set | Ebner-Fairchild constant-hue `Unicolour` loci |
| [04] | `HungBerns` | reference set | Hung-Berns constant-hue `Unicolour` loci |
| [05] | `IsccNbs` | reference set | ISCC-NBS centroid `Unicolour` colour names |
| [06] | `Css` | named-colour list | `Css.All` holds 148 named `Unicolour` colours; `Css.Transparent` is a separate public field |
| [07] | `Xkcd` | named-colour list | 949 xkcd survey named `Unicolour` colours |
| [08] | `Nord` | named-colour list | Nord 16-swatch palette as `Unicolour` |
| [09] | `ArtistPaint` | pigment set | Golden artist-paint `Pigment` reflectance set |

[PUBLIC_TYPE_SCOPE]: perceptual colourmap carriers
- rail: colour-datasets

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
|:-----: |:---------------- |:----------------- |:--------------------------------------------- |
| [01] | `Colourmap` | colourmap base | `abstract` `Map`, `MapWithClipping`, `Palette` |
| [02] | `Colourmaps` | colourmap registry | static handles to all 15 `Colourmap` instances |
| [03] | `Viridis` | colourmap | matplotlib viridis perceptual map |
| [04] | `Plasma` | colourmap | matplotlib plasma perceptual map |
| [05] | `Inferno` | colourmap | matplotlib inferno perceptual map |
| [06] | `Magma` | colourmap | matplotlib magma perceptual map |
| [07] | `Cividis` | colourmap | matplotlib cividis perceptual map |
| [08] | `Mako` | colourmap | seaborn mako perceptual map |
| [09] | `Rocket` | colourmap | seaborn rocket perceptual map |
| [10] | `Crest` | colourmap | seaborn crest perceptual map |
| [11] | `Flare` | colourmap | seaborn flare perceptual map |
| [12] | `Vlag` | colourmap | seaborn vlag diverging map |
| [13] | `Icefire` | colourmap | seaborn icefire diverging map |
| [14] | `Twilight` | colourmap | matplotlib twilight cyclic map |
| [15] | `TwilightShifted` | colourmap | matplotlib twilight-shifted cyclic map |
| [16] | `Turbo` | colourmap | Google turbo perceptual map |
| [17] | `Cubehelix` | colourmap | Green cubehelix monotonic-luminance map |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Colourmap` sampling
- rail: colour-datasets

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:-------------------------------------------------------------------------------------------- |:-------------- |:------------------------------------------- |
| [01] | `Colourmap.Map(double x)` | abstract sample | maps `x` in `[0, 1]` to a `Unicolour` |
| [02] | `Unicolour MapWithClipping(double x, Unicolour? lowerClipColour = null, Unicolour? upperClipColour = null)` | clamped sample | clamps out-of-range `x` to clip colours |
| [03] | `Colourmap.Palette(int count)` | sequence call | evenly spaced `IEnumerable<Unicolour>` |
| [04] | `Colourmap.Config` | static field | sRGB, D65 working `Configuration` |
| [05] | `Colourmap.Black` / `Colourmap.White` | static field | default lower and upper clip `Unicolour` |
| [06] | `<Colourmap>.Lookup` | static field | source `IEnumerable<Unicolour>` lookup table on the 14 lookup-backed maps |
| [07] | `Cubehelix.Map(double x, double start, double rotations, double hue, double gamma)` | static call | procedural cubehelix sample |

[ENTRYPOINT_SCOPE]: `Colourmaps` registry handles
- rail: colour-datasets

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:--------------------------------------------- |:----------- |:-------------------------------- |
| [01] | `Colourmaps.Viridis`.. `Colourmaps.Cubehelix` | static field | named `Colourmap` instance handle |

[ENTRYPOINT_SCOPE]: reference and named-colour set members
- rail: colour-datasets

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:------------------------------------------------------------------------ |:-------------- |:--------------------------------------------------- |
| [01] | `Macbeth.DarkSkin`.. `Macbeth.Black` | static field | 24 named ColorChecker patch `Unicolour` values |
| [02] | `Macbeth.Natural` / `Miscellaneous` / `PrimaryAndSecondary` / `Greyscale` | static field | six-patch ColorChecker row `IEnumerable<Unicolour>` |
| [03] | `Macbeth.All` | static property | all 24 patches as `IEnumerable<Unicolour>` |
| [04] | `MacAdam.Limits10`.. `MacAdam.Limits95` | static field | per-luminance MacAdam limit `IEnumerable<Unicolour>` |
| [05] | `MacAdam.All` | static property | all MacAdam limit loci as `IEnumerable<Unicolour>` |
| [06] | `EbnerFairchild.AllHue0`.. `AllHue336` | static field | per-hue constant-hue `IEnumerable<Unicolour>` loci |
| [07] | `EbnerFairchild.All` | static property | all Ebner-Fairchild loci as `IEnumerable<Unicolour>` |
| [08] | `HungBerns.AllRed`.. `AllMagentaRed`, `All25`.. `AllRef` | static field | per-hue and per-chroma `IEnumerable<Unicolour>` rows |
| [09] | `HungBerns.All` | static property | all Hung-Berns loci as `IEnumerable<Unicolour>` |
| [10] | `IsccNbs.VividPink`.. (267 centroids) | static field | named ISCC-NBS centroid `Unicolour` values |
| [11] | `IsccNbs.All` | static property | all 267 centroids as `IEnumerable<Unicolour>` |
| [12] | `IsccNbs.FromName(string)` | static lookup | named ISCC-NBS lookup returning `Unicolour?` |
| [13] | `Css.AliceBlue`.. CSS named colours plus `Css.Transparent` | static field | named CSS `Unicolour` values |
| [14] | `Css.All` | static property | all 148 non-transparent CSS colours as `IEnumerable<Unicolour>` |
| [15] | `Css.FromName(string)` | static lookup | named CSS lookup returning `Unicolour?` |
| [16] | `Xkcd.AcidGreen`.. (949 colours) | static field | named xkcd `Unicolour` values |
| [17] | `Xkcd.All` | static property | all 949 xkcd colours as `IEnumerable<Unicolour>` |
| [18] | `Xkcd.FromName(string)` | static lookup | named xkcd lookup returning `Unicolour?` |
| [19] | `Nord.Nord0`.. `Nord.Nord15` | static field | Nord swatch `Unicolour` values |
| [20] | `Nord.PolarNight` / `SnowStorm` / `Frost` / `Aurora` | static field | named Nord group `IEnumerable<Unicolour>` |
| [21] | `Nord.All` | static property | all 16 Nord swatches as `IEnumerable<Unicolour>` |
| [22] | `ArtistPaint.BoneBlack`.. (19 pigments) | static field | Kubelka-Munk artist `Pigment` reflectance values |
| [23] | `ArtistPaint.All` | static property | all 19 pigments as `IEnumerable<Pigment>` |
| [24] | `ArtistPaint.Configuration` | static field | sRGB, D50 working `Configuration` for the pigments |

## [04]-[IMPLEMENTATION_LAW]

[DATASET_TOPOLOGY]:
- namespace: `Wacton.Unicolour.Datasets`; every dataset is a static table of `Unicolour` or `Pigment` values built on the main `Wacton.Unicolour` colour owner.
- colourmap: `Colourmap` is abstract; the 14 lookup-backed concrete maps carry a static `Lookup` `IEnumerable<Unicolour>` and override `Map(double x)` by interpolating the lookup table in `Rgb` with `HueSpan.Shorter`; `Cubehelix` computes procedurally through `Cubehelix.Map(double x, double start, double rotations, double hue, double gamma)`.
- sampling: `Map` accepts `x` in `[0, 1]`; `MapWithClipping` clamps below to `Black` (or `lowerClipColour`) and above to `White` (or `upperClipColour`); `Palette(count)` emits evenly spaced samples.
- registry: `Colourmaps` exposes one static handle per concrete `Colourmap`.
- reference sets: `Macbeth`, `MacAdam`, `EbnerFairchild`, `HungBerns`, and `IsccNbs` expose named static `Unicolour` members plus grouping `IEnumerable<Unicolour>` rows and an `All` aggregate.
- working space: each set fixes its own `Configuration` for the source measurement — `Colourmap.Config` is sRGB/`XyzConfiguration.D65`, `Macbeth` is sRGB/`Illuminant.D50`/`Observer.Degree2`, `ArtistPaint.Configuration` is sRGB/`XyzConfiguration.D50`; a consumer reads the set's own `Configuration` rather than assuming the global `Configuration.Default`.

[BOUNDARY]:
- This package carries named-colour lists, ColorChecker / Macbeth reference sets, perceptual colourmaps, and academic reference datasets only.
- Observer colour-matching functions, illuminant spectral power distributions, and generic reflectance live on the main `Wacton.Unicolour` owner (`Spd`, `SpectralCoefficients`, `Pigment`, `Illuminant`, `Observer`), not here.
- Spectral upsampling and measured-spectral construction consume the main `Wacton.Unicolour` colour owner; this package supplies validation and named-reference colour tables.

[LOCAL_ADMISSION]:
- Material colour validation reads a reference `Unicolour` from `Macbeth`, `MacAdam`, or an academic set rather than re-encoding patch coordinates.
- Library material rows resolve named colours through `Css`, `Xkcd`, or `Nord` instead of hand-keyed hex literals.
- Perceptual ramps sample a `Colourmap` through `Map` or `Palette`; lookup-backed maps read the lookup table, and `Cubehelix` uses its public procedural `Map`.
- Pigment reflectance for paint mixing reads `ArtistPaint` `Pigment` values and mixes through the main `Wacton.Unicolour` Kubelka-Munk constructor.

[STACK]:
- finish seam: `finish#FINISH` `FinishPigment` builds a `FrozenDictionary<string, Pigment>` from `ArtistPaint.All`, then `finish#FINISH` mixes the selected `Pigment[]` through the MAIN owner's `new Unicolour(pigments, weights)` Kubelka-Munk constructor under `ArtistPaint.Configuration` (sRGB/D50) — this package supplies the measured Golden pigment table, the main `Wacton.Unicolour` owner runs the mix.
- graph seam: `graph#MATERIAL_LIBRARY` `NearestChecker` measures a candidate `MaterialParameters` base colour against `Macbeth.All` patches through the main owner's `Difference(patch, DeltaE.Ciede2000)`, returning the `(Patch, DeltaE)` drift pair — the reference patches live here, the metric runs on the main owner.
- colourmap seam: a perceptual ramp samples a `Colourmap` through `Map(x)`/`Palette(count)` (each lookup-backed map interpolates its static `Lookup` `IEnumerable<Unicolour>` in `Rgb` with `HueSpan.Shorter`, while `Cubehelix` computes procedurally under `Colourmap.Config`) — the lookup is read, never re-derived; the produced `Unicolour` values flow to the main owner for any further conversion or gamut check.

[RAIL_LAW]:
- Package: `Wacton.Unicolour.Datasets` (MIT, `netstandard2.0`, depends on `Wacton.Unicolour`)
- Owns: named-colour lists (`Css.All` 148 plus `Css.Transparent`, `Xkcd` 949, `Nord` 16), ColorChecker / Macbeth reference set (24 patches), academic reference sets (`MacAdam`, `EbnerFairchild`, `HungBerns`, `IsccNbs` 267), the 15 perceptual `Colourmap` instances, and the Golden `ArtistPaint` 19-pigment table — all as static `Unicolour`/`Pigment` tables built on the main owner
- Accept: a reference patch, named colour, colourmap sample position, or pigment lookup, each read with its set's own `Configuration`
- Reject: observer CMFs, illuminant SPDs, generic reflectance, the Kubelka-Munk mix math, delta-E, and spectral upsampling, which stay on the main `Wacton.Unicolour` owner
