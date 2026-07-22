# [RASM_MATERIALS_API_UNICOLOUR_DATASETS]

`Wacton.Unicolour.Datasets` owns named-colour lists, the ColorChecker/Macbeth and academic reference sets, perceptual colourmaps, and the Golden pigment table as static `Unicolour` and `Pigment` tables built on the main `Wacton.Unicolour` colour owner. Observer CMFs, illuminant SPDs, generic reflectance, and every mix, difference, and spectral-upsampling transform stay on that main owner; this package supplies validation and named-reference colour alone.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Wacton.Unicolour.Datasets`
- package: `Wacton.Unicolour.Datasets` (MIT, Wacton)
- assembly: `Wacton.Unicolour.Datasets`
- namespace: `Wacton.Unicolour.Datasets`
- asset: pure-managed `netstandard2.0` runtime library, ALC-safe, no native or extra transitive asset; a `net10.0` consumer binds the netstandard2.0 asset and pulls `Wacton.Unicolour` (the `Unicolour`/`Pigment`/`Configuration` carriers).
- rail: colour-datasets

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: reference and named-colour set carriers

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]     | [CAPABILITY]                                    |
| :-----: | :--------------- | :---------------- | :---------------------------------------------- |
|  [01]   | `Macbeth`        | reference set     | 24-patch ColorChecker/Macbeth `Unicolour` table |
|  [02]   | `MacAdam`        | reference set     | MacAdam optimal-colour limit loci               |
|  [03]   | `EbnerFairchild` | reference set     | Ebner-Fairchild constant-hue loci               |
|  [04]   | `HungBerns`      | reference set     | Hung-Berns constant-hue loci                    |
|  [05]   | `IsccNbs`        | reference set     | ISCC-NBS centroid colour names                  |
|  [06]   | `Css`            | named-colour list | CSS named `Unicolour` colours                   |
|  [07]   | `Xkcd`           | named-colour list | xkcd survey named `Unicolour` colours           |
|  [08]   | `Nord`           | named-colour list | Nord swatch palette                             |
|  [09]   | `ArtistPaint`    | pigment set       | Golden artist-paint `Pigment` reflectance set   |

[PUBLIC_TYPE_SCOPE]: perceptual colourmap carriers

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]      | [CAPABILITY]                                 |
| :-----: | :----------- | :----------------- | :------------------------------------------- |
|  [01]   | `Colourmap`  | colourmap base     | `abstract Map`, `MapWithClipping`, `Palette` |
|  [02]   | `Colourmaps` | colourmap registry | one static handle per concrete `Colourmap`   |

Concrete `Colourmap` instances, reached through `Colourmaps` handles:
[MATPLOTLIB_SEQUENTIAL]: `Viridis` `Plasma` `Inferno` `Magma` `Cividis`
[MATPLOTLIB_CYCLIC]: `Twilight` `TwilightShifted`
[SEABORN_SEQUENTIAL]: `Mako` `Rocket` `Crest` `Flare`
[SEABORN_DIVERGING]: `Vlag` `Icefire`
[STANDALONE]: `Turbo` (Google perceptual) `Cubehelix` (Green procedural monotonic-luminance)

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Colourmap` sampling

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                                             |
| :-----: | :---------------------------------------------------------- | :------- | :------------------------------------------------------- |
|  [01]   | `Colourmap.Map(double)`                                     | instance | `abstract` sample -> `Unicolour`                         |
|  [02]   | `Colourmap.MapWithClipping(double, Unicolour?, Unicolour?)` | instance | substitutes clip colour outside `[0,1]`                  |
|  [03]   | `Colourmap.Palette(int)`                                    | instance | evenly spaced `IEnumerable<Unicolour>`                   |
|  [04]   | `Colourmap.Config`                                          | static   | sRGB D65 `Configuration`                                 |
|  [05]   | `Colourmap.Black`, `Colourmap.White`                        | static   | default lower/upper clip `Unicolour`                     |
|  [06]   | `<Colourmap>.Lookup`                                        | static   | source `IEnumerable<Unicolour>` table                    |
|  [07]   | `Cubehelix.Map(double, double, double, double, double)`     | static   | procedural sample (`start`, `rotations`, `hue`, `gamma`) |

[ENTRYPOINT_SCOPE]: `Colourmaps` registry handles

| [INDEX] | [SURFACE]                                     | [SHAPE] | [CAPABILITY]                      |
| :-----: | :-------------------------------------------- | :------ | :-------------------------------- |
|  [01]   | `Colourmaps.Viridis`.. `Colourmaps.Cubehelix` | static  | named `Colourmap` instance handle |

[ENTRYPOINT_SCOPE]: reference and named-colour set members — every member static, each `FromName` returning `Unicolour?`

| [INDEX] | [SURFACE]                               | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :-------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `Macbeth.DarkSkin`.. `Macbeth.Black`    | static   | 24 named ColorChecker patch values            |
|  [02]   | `Macbeth.Natural`                       | static   | six-patch `IEnumerable<Unicolour>` group      |
|  [03]   | `Macbeth.Miscellaneous`                 | static   | six-patch `IEnumerable<Unicolour>` group      |
|  [04]   | `Macbeth.PrimaryAndSecondary`           | static   | six-patch `IEnumerable<Unicolour>` group      |
|  [05]   | `Macbeth.Greyscale`                     | static   | six-patch `IEnumerable<Unicolour>` group      |
|  [06]   | `Macbeth.All`                           | property | all 24 patches as `IEnumerable<Unicolour>`    |
|  [07]   | `MacAdam.Limits10`.. `MacAdam.Limits95` | static   | per-luminance limit `IEnumerable<Unicolour>`  |
|  [08]   | `MacAdam.All`                           | property | all MacAdam loci as `IEnumerable<Unicolour>`  |
|  [09]   | `EbnerFairchild.AllHue0`.. `AllHue336`  | static   | per-hue constant-hue `IEnumerable<Unicolour>` |
|  [10]   | `EbnerFairchild.All`                    | property | all Ebner-Fairchild loci                      |
|  [11]   | `HungBerns.AllRed`.. `AllMagentaRed`    | static   | per-hue `IEnumerable<Unicolour>` rows         |
|  [12]   | `HungBerns.All25`.. `AllRef`            | static   | per-chroma `IEnumerable<Unicolour>` rows      |
|  [13]   | `HungBerns.All`                         | property | all Hung-Berns loci                           |
|  [14]   | `IsccNbs.VividPink`.. centroids         | static   | named ISCC-NBS centroid values                |
|  [15]   | `IsccNbs.All`                           | property | all 267 centroids as `IEnumerable<Unicolour>` |
|  [16]   | `IsccNbs.FromName(string)`              | static   | name lookup -> `Unicolour?`                   |
|  [17]   | `Css.AliceBlue`.. named colours         | static   | named CSS `Unicolour` values                  |
|  [18]   | `Css.Transparent`                       | static   | transparent CSS value, outside `Css.All`      |
|  [19]   | `Css.All`                               | property | 148 non-transparent `IEnumerable<Unicolour>`  |
|  [20]   | `Css.FromName(string)`                  | static   | name lookup -> `Unicolour?`                   |
|  [21]   | `Xkcd.AcidGreen`.. named colours        | static   | named xkcd `Unicolour` values                 |
|  [22]   | `Xkcd.All`                              | property | all 949 colours as `IEnumerable<Unicolour>`   |
|  [23]   | `Xkcd.FromName(string)`                 | static   | name lookup -> `Unicolour?`                   |
|  [24]   | `Nord.Nord0`.. `Nord.Nord15`            | static   | Nord swatch `Unicolour` values                |
|  [25]   | `Nord.PolarNight`                       | static   | named group `IEnumerable<Unicolour>`          |
|  [26]   | `Nord.SnowStorm`                        | static   | named group `IEnumerable<Unicolour>`          |
|  [27]   | `Nord.Frost`                            | static   | named group `IEnumerable<Unicolour>`          |
|  [28]   | `Nord.Aurora`                           | static   | named group `IEnumerable<Unicolour>`          |
|  [29]   | `Nord.All`                              | property | all 16 swatches as `IEnumerable<Unicolour>`   |
|  [30]   | `ArtistPaint.BoneBlack`.. pigments      | static   | Kubelka-Munk `Pigment` reflectance values     |
|  [31]   | `ArtistPaint.All`                       | property | all 19 pigments as `IEnumerable<Pigment>`     |
|  [32]   | `ArtistPaint.Configuration`             | static   | sRGB D50 pigment `Configuration`              |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every dataset is a static table of `Unicolour` or `Pigment` values on the main `Wacton.Unicolour` owner; a new colourmap, reference set, or named-colour list is one static carrier type registered on the surface it joins.
- `Colourmap` is abstract: a lookup-backed map carries a static `Lookup` `IEnumerable<Unicolour>` and overrides `Map(double)` by interpolating that table in `Rgb` under `HueSpan.Shorter`; `Cubehelix` overrides `Map` procedurally through its static `Cubehelix.Map(double, double, double, double, double)`.
- `Map` accepts `x` in `[0, 1]`; `MapWithClipping` substitutes `Black`/`White` or an explicit clip colour outside it; `Palette(int)` emits evenly spaced samples.
- Each set fixes its own source-measurement `Configuration` a consumer reads rather than assuming `Configuration.Default`: `Colourmap.Config` is sRGB/`XyzConfiguration.D65`, `Macbeth` is sRGB/`Illuminant.D50`/`Observer.Degree2`, `ArtistPaint.Configuration` is sRGB/`XyzConfiguration.D50`.
- Observer CMFs, illuminant SPDs, and generic reflectance stay on the main `Wacton.Unicolour` owner (`Spd`, `SpectralCoefficients`, `Illuminant`, `Observer`); this package holds only validation and named-reference tables.

[STACKING]:
- `Wacton.Unicolour`(`libs/csharp/.api/api-unicolour.md`): `ArtistPaint.All` pigment reflectance feeds the main owner's `Unicolour(Pigment[], double[])` Kubelka-Munk mix under `ArtistPaint.Configuration`, and `Macbeth.All` patches feed `Difference(Unicolour, DeltaE.Ciede2000)` — this package supplies the measured tables, the main owner runs every mix, difference, and conversion.
- `finish#FINISH`: `FinishPigment` builds a `FrozenDictionary<string, Pigment>` from `ArtistPaint.All`, then mixes the selected `Pigment[]` through the main owner's Kubelka-Munk constructor.
- `graph#MATERIAL_LIBRARY`: `NearestChecker` ranks a candidate `MaterialParameters` base colour against `Macbeth.All` through the main owner's `Difference`, returning the `(Patch, DeltaE)` drift.
- colourmap ramp: a perceptual ramp samples a `Colourmap` through `Map(x)`/`Palette(count)`, and the produced `Unicolour` values flow to the main owner for further conversion or gamut check.

[LOCAL_ADMISSION]:
- Material colour validation reads a reference `Unicolour` from `Macbeth`, `MacAdam`, or an academic set, never re-encoded patch coordinates.
- Library material rows resolve named colours through `Css`, `Xkcd`, or `Nord`, never hand-keyed hex literals.
- Perceptual ramps sample a `Colourmap`: lookup-backed maps read their `Lookup`, `Cubehelix` uses its procedural `Map`.
- Paint-mixing pigment reflectance reads `ArtistPaint` `Pigment` values and mixes through the main owner's Kubelka-Munk constructor.

[RAIL_LAW]:
- Package: `Wacton.Unicolour.Datasets`
- Owns: named-colour lists (`Css`, `Xkcd`, `Nord`), the ColorChecker/Macbeth and academic reference sets (`MacAdam`, `EbnerFairchild`, `HungBerns`, `IsccNbs`), the perceptual `Colourmap` family, and the Golden `ArtistPaint` pigment table — all static `Unicolour`/`Pigment` tables on the main owner
- Accept: a reference patch, named colour, colourmap sample position, or pigment lookup, each read under its set's own `Configuration`
- Reject: observer CMFs, illuminant SPDs, generic reflectance, the Kubelka-Munk mix, delta-E, and spectral upsampling, which stay on the main `Wacton.Unicolour` owner
