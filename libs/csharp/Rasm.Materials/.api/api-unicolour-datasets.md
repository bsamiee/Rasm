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

| [INDEX] | [SYMBOL]         | [PACKAGE_ROLE]    | [CAPABILITY]                                                                                |
| :-----: | :--------------- | :---------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `Macbeth`        | reference set     | 24-patch ColorChecker / Macbeth `Unicolour` table                                           |
|  [02]   | `MacAdam`        | reference set     | MacAdam optimal-colour limit loci as `Unicolour`                                            |
|  [03]   | `EbnerFairchild` | reference set     | Ebner-Fairchild constant-hue `Unicolour` loci                                               |
|  [04]   | `HungBerns`      | reference set     | Hung-Berns constant-hue `Unicolour` loci                                                    |
|  [05]   | `IsccNbs`        | reference set     | ISCC-NBS centroid `Unicolour` colour names                                                  |
|  [06]   | `Css`            | named-colour list | `Css.All` holds 148 named `Unicolour` colours; `Css.Transparent` is a separate public field |
|  [07]   | `Xkcd`           | named-colour list | 949 xkcd survey named `Unicolour` colours                                                   |
|  [08]   | `Nord`           | named-colour list | Nord 16-swatch palette as `Unicolour`                                                       |
|  [09]   | `ArtistPaint`    | pigment set       | Golden artist-paint `Pigment` reflectance set                                               |

[PUBLIC_TYPE_SCOPE]: perceptual colourmap carriers
- rail: colour-datasets

| [INDEX] | [SYMBOL]          | [PACKAGE_ROLE]     | [CAPABILITY]                                   |
| :-----: | :---------------- | :----------------- | :--------------------------------------------- |
|  [01]   | `Colourmap`       | colourmap base     | `abstract` `Map`, `MapWithClipping`, `Palette` |
|  [02]   | `Colourmaps`      | colourmap registry | static handles to all 15 `Colourmap` instances |
|  [03]   | `Viridis`         | colourmap          | matplotlib viridis perceptual map              |
|  [04]   | `Plasma`          | colourmap          | matplotlib plasma perceptual map               |
|  [05]   | `Inferno`         | colourmap          | matplotlib inferno perceptual map              |
|  [06]   | `Magma`           | colourmap          | matplotlib magma perceptual map                |
|  [07]   | `Cividis`         | colourmap          | matplotlib cividis perceptual map              |
|  [08]   | `Mako`            | colourmap          | seaborn mako perceptual map                    |
|  [09]   | `Rocket`          | colourmap          | seaborn rocket perceptual map                  |
|  [10]   | `Crest`           | colourmap          | seaborn crest perceptual map                   |
|  [11]   | `Flare`           | colourmap          | seaborn flare perceptual map                   |
|  [12]   | `Vlag`            | colourmap          | seaborn vlag diverging map                     |
|  [13]   | `Icefire`         | colourmap          | seaborn icefire diverging map                  |
|  [14]   | `Twilight`        | colourmap          | matplotlib twilight cyclic map                 |
|  [15]   | `TwilightShifted` | colourmap          | matplotlib twilight-shifted cyclic map         |
|  [16]   | `Turbo`           | colourmap          | Google turbo perceptual map                    |
|  [17]   | `Cubehelix`       | colourmap          | Green cubehelix monotonic-luminance map        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Colourmap` sampling
- rail: colour-datasets

`Colourmap.Map` samples `[0, 1]`, and `MapWithClipping` substitutes boundary colours outside that interval.

`Palette` emits evenly spaced samples. `Black` and `White` supply the default lower and upper clip colours.

Lookup-backed maps expose their source tables, while `Cubehelix.Map` computes samples procedurally.

| [INDEX] | [SURFACE]                   | [KIND]          | [RESULT]                 |
| :-----: | :-------------------------- | :-------------- | :----------------------- |
|  [01]   | `Colourmap.Map`             | abstract method | `Unicolour`              |
|  [02]   | `Colourmap.MapWithClipping` | method          | `Unicolour`              |
|  [03]   | `Colourmap.Palette`         | method          | `IEnumerable<Unicolour>` |
|  [04]   | `Colourmap.Config`          | static field    | sRGB D65 `Configuration` |
|  [05]   | `Colourmap.Black`           | static field    | lower clip `Unicolour`   |
|  [06]   | `Colourmap.White`           | static field    | upper clip `Unicolour`   |
|  [07]   | `<Colourmap>.Lookup`        | static field    | source lookup table      |
|  [08]   | `Cubehelix.Map`             | static method   | procedural colour sample |

[PARAMETERS]:
- `Colourmap.Map`: `double x`
- `Colourmap.MapWithClipping`: `double x`; `Unicolour? lowerClipColour = null`; `Unicolour? upperClipColour = null`
- `Colourmap.Palette`: `int count`
- `Cubehelix.Map`: `double x`; `double start`; `double rotations`; `double hue`; `double gamma`

[ENTRYPOINT_SCOPE]: `Colourmaps` registry handles
- rail: colour-datasets

| [INDEX] | [SURFACE]                                     | [CALL_SHAPE] | [CAPABILITY]                      |
| :-----: | :-------------------------------------------- | :----------- | :-------------------------------- |
|  [01]   | `Colourmaps.Viridis`.. `Colourmaps.Cubehelix` | static field | named `Colourmap` instance handle |

[ENTRYPOINT_SCOPE]: reference and named-colour set members
- rail: colour-datasets

Every reference and named-colour member below is static.

| [INDEX] | [SURFACE]                               | [KIND]   | [CAPABILITY]                                      |
| :-----: | :-------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `Macbeth.DarkSkin`.. `Macbeth.Black`    | field    | 24 named ColorChecker patch `Unicolour` values    |
|  [02]   | `Macbeth.Natural`                       | field    | six-patch ColorChecker `IEnumerable<Unicolour>`   |
|  [03]   | `Miscellaneous`                         | field    | six-patch ColorChecker `IEnumerable<Unicolour>`   |
|  [04]   | `PrimaryAndSecondary`                   | field    | six-patch ColorChecker `IEnumerable<Unicolour>`   |
|  [05]   | `Greyscale`                             | field    | six-patch ColorChecker `IEnumerable<Unicolour>`   |
|  [06]   | `Macbeth.All`                           | property | all 24 patches as `IEnumerable<Unicolour>`        |
|  [07]   | `MacAdam.Limits10`.. `MacAdam.Limits95` | field    | per-luminance limit `IEnumerable<Unicolour>`      |
|  [08]   | `MacAdam.All`                           | property | all MacAdam loci as `IEnumerable<Unicolour>`      |
|  [09]   | `EbnerFairchild.AllHue0`.. `AllHue336`  | field    | per-hue constant-hue `IEnumerable<Unicolour>`     |
|  [10]   | `EbnerFairchild.All`                    | property | all Ebner-Fairchild `IEnumerable<Unicolour>` loci |
|  [11]   | `HungBerns.AllRed`.. `AllMagentaRed`    | field    | per-hue `IEnumerable<Unicolour>` rows             |
|  [12]   | `HungBerns.All25`.. `AllRef`            | field    | per-chroma `IEnumerable<Unicolour>` rows          |
|  [13]   | `HungBerns.All`                         | property | all Hung-Berns loci as `IEnumerable<Unicolour>`   |
|  [14]   | `IsccNbs.VividPink`.. (267 centroids)   | field    | named ISCC-NBS centroid `Unicolour` values        |
|  [15]   | `IsccNbs.All`                           | property | all 267 centroids as `IEnumerable<Unicolour>`     |
|  [16]   | `IsccNbs.FromName(string)`              | lookup   | named ISCC-NBS lookup returning `Unicolour?`      |
|  [17]   | `Css.AliceBlue`.. CSS named colours     | field    | named CSS `Unicolour` values                      |
|  [18]   | `Css.Transparent`                       | field    | transparent CSS `Unicolour` value                 |
|  [19]   | `Css.All`                               | property | 148 non-transparent `IEnumerable<Unicolour>`      |
|  [20]   | `Css.FromName(string)`                  | lookup   | named CSS lookup returning `Unicolour?`           |
|  [21]   | `Xkcd.AcidGreen`.. (949 colours)        | field    | named xkcd `Unicolour` values                     |
|  [22]   | `Xkcd.All`                              | property | all 949 colours as `IEnumerable<Unicolour>`       |
|  [23]   | `Xkcd.FromName(string)`                 | lookup   | named xkcd lookup returning `Unicolour?`          |
|  [24]   | `Nord.Nord0`.. `Nord.Nord15`            | field    | Nord swatch `Unicolour` values                    |
|  [25]   | `Nord.PolarNight`                       | field    | named group `IEnumerable<Unicolour>`              |
|  [26]   | `SnowStorm`                             | field    | named group `IEnumerable<Unicolour>`              |
|  [27]   | `Frost`                                 | field    | named group `IEnumerable<Unicolour>`              |
|  [28]   | `Aurora`                                | field    | named group `IEnumerable<Unicolour>`              |
|  [29]   | `Nord.All`                              | property | all 16 swatches as `IEnumerable<Unicolour>`       |
|  [30]   | `ArtistPaint.BoneBlack`.. (19 pigments) | field    | Kubelka-Munk `Pigment` reflectance values         |
|  [31]   | `ArtistPaint.All`                       | property | all 19 pigments as `IEnumerable<Pigment>`         |
|  [32]   | `ArtistPaint.Configuration`             | field    | sRGB D50 pigment `Configuration`                  |

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
