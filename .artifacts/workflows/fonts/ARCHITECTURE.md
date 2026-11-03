# [ARCHITECTURE]

`libs/python/fonts/` reconciles each face of a declared font set from its original binary through a plan of rules into a UFO source, compiles it through ufo2ft, verifies it against one set of requirements, and packages it. Records are frozen `msgspec.Struct`, results are `expression` `Outcome[T]` with one `Fault` record, and every module follows the `python-coding` skill.

## [01]-[MODULES]

Each package owns what fontTools, ufo2ft, HarfBuzz, OTS and fontspector leave undone:

| [INDEX] | [PACKAGE]      | [OWNS]                                                                   | [READS]                           |
| :-----: | :------------- | :----------------------------------------------------------------------- | :-------------------------------- |
|  [01]   | `faces`        | `FontSet`, `Family`, `Face`, `Glyph`, `Role`, `Coverage`, decode, import | `fontTools.ttLib`, ufoLib2        |
|  [02]   | `outlines`     | Union, difference, stroke measure, nib sweep, signed gap, bounds, IoU    | `skia-pathops`, fontTools pens    |
|  [03]   | `shaping`      | Shaped runs, `hb-shape` serialization, fontspector shaping JSON, corpus  | `uharfbuzz`                       |
|  [04]   | `rules`        | `Rule` union, `Plan` per genre and treatment, lookup records, emission   | `faces`, `outlines`, `shaping`    |
|  [05]   | `verification` | `Requirement` union, `Finding`, `verified`, OTS and fontspector decode   | `opentype-sanitizer`, fontspector |
|  [06]   | `foundry`      | `App` commands `source`, `build`, `check`, `approve`, `run`, `publish`   | Every package above, ufo2ft       |

Dependencies point downward in table order. `foundry` alone performs file and process effects: UFO write, `compileTTF` with the writer list, `ttfautohint`, `fontTools.ttLib.woff2` and the build hash, since a separate compile package would forward ufo2ft calls unchanged.

## [02]-[FACE_MODEL]

Set declarations (TOML, decoded by `msgspec.toml` into records) name families, their genre, their faces and per-face design values, validated whole at decode:

```python
class Face(Struct, frozen=True):
    family: FamilyName
    style: Style
    weight: Weight
    width: Width
    original: Path
    genre: Genre
    treatments: tuple[Treatment, ...]
```

- `Genre` is a tagged union over `TextNaskh`, `BlackNaskh`, `Thuluth`, `Kufi`, `DotMatrix`, `Ruqah`, `Diwani` (`PLAN.md` [01.3])
- Each `Genre` record holds the bands its class measures (mark band, pitch band, apparent size), and a face record overrides one by value
- `Treatment` is a tagged union over `Sheared(angle)`, `Ringed(width)`, `Contour(ring, channel)`, `Segmented`, effects and not weights
- Italic-only and contour-only faces are a genre with a treatment and no plain sibling, so no class exists per effect
- `OrnamentSet` is a second set kind with no letters: re-encoded art, merged duplicates and declared code points
- `Family` holds name, genre and `tuple[Face, ...]`, and shared values (scale, vertical metrics) derive once per family
- `Glyph` holds `GlyphName`, code points, `Role`, contours as point tuples, components, anchors, advance and OpenType category
- `Role` is a tagged union: `Letter` with form and joining type, `Mark` with side and class, `Ligature` with components, `Digit`, `Sign`, `Ornament`
- `Coverage` lists required code points per genre and `Excluded(code, reason)` records, and a mapped code point in neither is a finding
- Names derive from code point and role (`uni0644_uni0627.fina`), parsed once at import into `GlyphName`
- Import reads cmap, GSUB singles and ligatures for roles, GPOS mark lookups for anchors, GDEF for categories
- Import reads Unicode decomposition for hamza and madda components and `youseedee` `ArabicShaping.txt` for joining types
- Import drops `MarkAttachClassDef` where the GDEF `GlyphClassDef` offset points inside the 12-byte header, before fontTools decompiles it
- Outlines enter through `outlines.union` and cu2qu, so every glyph is overlap-free and quadratic from import

## [03]-[RULES]

Rules are data, a plan is a validated sequence of them, and each rule repairs a defect class `observations.toml` records:

```python
type Rule = Tables | Locale | Digits | Outlines | Marks | QuranicMarks | Joins | Scale | Anchors | Spacing | Metrics | Records | Hinting | WordSigns

class Plan(Struct, frozen=True):
    genre: Genre
    treatments: tuple[Treatment, ...]
    rules: tuple[Rule, ...]
```

- Each `Rule` record holds its values (join overlap in em, word gap ratio, pitch band)
- `Plan` construction holds `PLAN.md` [02] order, each rule measuring what earlier ones wrote, and refuses `Anchors` before `Scale` or `Marks`
- `applied(face: Face, rule: Rule) -> Outcome[Face]` matches exhaustively, and `planned(face, plan)` binds the sequence, so one fault names its rule
- Treatments transform a planned face (`Sheared` slants outlines and sets the italic angle, `Ringed` strokes inward), and genres set rule values
- Rules return a new `Face`, and measurements (scale factor, clearance from the original's shaping) are pure functions over `Face` and `Shaped` values
- Layout rules return `Lookup` records (`SinglePos`, `PairPos`, `Contextual`, `Ligature`, `Alternate`) with feature, language system and order
- One emitter writes lookup records as `feaLib.ast` into `ufo.features.text` once
- `ufo2ft` writers in append mode derive `mark`, `mkmk`, `curs`, `kern` and GDEF from `top`, `_top`, `top_1`, `entry`, `exit` and `caret_1`
- Contextual lifts sit in the `mark` feature before the `# Automatic Code` marker, so their lookups precede the writers' `mkmk` lookups
- Contextual inputs start at the letter, so a mark never opens a context
- Pairs come through `KernFeatureWriter` groups, which set placement and advance on the first glyph of an RTL pair
- Family rules (`Scale`, `Metrics`, `Records`) run over `Family`, and `Hinting` runs where a face's values ask for it
- `OrnamentSet` takes its own plan: PUA re-encoding, duplicate art merged, code page and Unicode range bits cleared

## [04]-[VERIFICATION]

Verification is one relation, `verified(built: Built, requirements: tuple[Requirement, ...]) -> tuple[Finding, ...]`. Independent requirements accumulate, and a build passes when the tuple is empty. Requirements follow `PLAN.md` [04], and each geometric one reads outlines through uharfbuzz `draw_glyph_with_pen` and skia-pathops:

| [INDEX] | [REQUIREMENT]   | [MEASURES]                                                           | [THROUGH]                              |
| :-----: | :-------------- | :------------------------------------------------------------------- | :------------------------------------- |
|  [01]   | `Sanitized`     | OTS accepts the binary                                               | `ots.sanitize`                         |
|  [02]   | `Profile`       | `universal` and `adobe` profiles with excluded ids, no FAIL or ERROR | `fontspector -x <id> --json <file>`    |
|  [03]   | `Expectations`  | Shaped runs equal approved shaping JSON                              | `fontspector` `shaping/regression`     |
|  [04]   | `Reachable`     | No `.notdef` for corpus texts under `ot` and `coretext` shapers      | `uharfbuzz.shape` with `shapers`       |
|  [05]   | `Coverage`      | Every required code point mapped, every excluded one recorded        | `faces.Coverage`                       |
|  [06]   | `MarkClearance` | Signed gap of each mark to base, neighbours and stacks, whole words  | `outlines.gap` over shaped runs        |
|  [07]   | `MarkLine`      | Above marks share one line within a stated tolerance                 | `outlines.bounds`                      |
|  [08]   | `LigatureInk`   | Ligature ink at least a stated ratio of its components' ink          | `outlines.area`                        |
|  [09]   | `JoinParity`    | Joined pairs do not move when `kern` toggles                         | `uharfbuzz.shape`, features on and off |
|  [10]   | `Seam`          | Each shaped join overlaps its neighbour by the declared overlap      | `outlines.intersection` over a run     |
|  [11]   | `Distinct`      | IoU between digit pairs below a bound                                | `outlines.iou`                         |
|  [12]   | `Metrics`       | Pitch inside its band, win covers every stacked mark of the corpus   | `outlines.bounds`                      |
|  [13]   | `Identity`      | Dots and hamzas match the original by IoU                            | `outlines.iou`                         |
|  [14]   | `Speed`         | Corpus shaping time within a stated ratio of the original            | `uharfbuzz.shape` timing               |
|  [15]   | `Reproducible`  | Rebuild hash equals the declaration's recorded hash                  | `Built.hash`                           |

- `Finding` records requirement, face, subject (glyph, pair or word), measured value and bound, and the report serializes the tuple as JSON
- `approve` writes shaping JSON from a named build hash, and a missing file is a finding, so no run seeds its own baseline
- `fontspector` runs `shaping/regression` under `--profile googlefonts -c shaping/regression` with `--configuration` naming `test_directory`
- Host checks through `tools/harnesses/indesign` stay outside the library, and `check` names the harness command to run

## [05]-[PACKAGES]

Versions read from PyPI, crates.io and GitHub releases on 2026-11-02, pinned in `uv.lock` alone:

| [INDEX] | [PACKAGE]            | [VERSION] | [3_15]                        | [MEMBERS]                                                           |
| :-----: | :------------------- | :-------- | :---------------------------- | :------------------------------------------------------------------ |
|  [01]   | `fonttools`          | 4.68.0    | `cp315` and `cp315t` wheels   | `ttLib`, `woff2`, `feaLib.ast`, `subset`, `fontBuilder`, `cu2qu`    |
|  [02]   | `ufo2ft`             | 3.11.0    | Pure, classifiers to 3.14     | `compileTTF`, `compileOTF`, Mark, Curs, Kern, Gdef writers, filters |
|  [03]   | `ufoLib2`            | 0.19.1    | Pure Python, `py.typed`       | `Font`, `Glyph`, `Anchor`, `Component`, `Contour`, `Point`          |
|  [04]   | `uharfbuzz`          | 0.57.0    | `cp311-abi3`, HarfBuzz 15.0.0 | `Face`, `Font`, `Buffer`, `shape`, `Font.draw_glyph_with_pen`       |
|  [05]   | `skia-pathops`       | 0.9.2     | `cp310-abi3` wheels           | `Path`, `PathPen`, `op`, `simplify`, `union`, `difference`          |
|  [06]   | `opentype-sanitizer` | 9.2.0     | No Linux arm64 wheel          | `ots.sanitize` over the bundled `ots-sanitize`                      |
|  [07]   | `ttfautohint-py`     | 0.6.1     | `py3-none` platform wheels    | `ttfautohint.ttfautohint`                                           |
|  [08]   | `youseedee`          | 0.7.0     | Pure, downloads UCD           | `ucd_data` for `Joining_Type` and `Joining_Group`                   |
|  [09]   | `msgspec`            | 0.22.0    | `cp315` and `cp315t` wheels   | `Struct`, `convert`, `toml.decode`, `json.encode`                   |
|  [10]   | `expression`         | 5.8.0     | Pure Python                   | `Result`, `Ok`, `Error`, `Option`, `extra.result.catch`, `pipe`     |
|  [11]   | `cyclopts`           | 5.3.3     | Pure Python                   | `App`, `Parameter`, `config.Env`                                    |
|  [12]   | `fontspector`        | 1.9.0     | GitHub release, `mise` row    | `--profile`, `-x`, `--configuration`, `--json <file>`               |

- `youseedee` downloads the latest `UCD.zip` into a user cache on first call, so the build hash covers the UCD version it read
- `fonttools` members: `ttLib.TTFont`, `subset.Subsetter`, `fontBuilder.FontBuilder`, pens `RecordingPen`, `BoundsPen`, `AreaPen`, `TTGlyphPen`
- `ufo2ft` filters: `RemoveOverlapsFilter`, `CubicToQuadraticFilter`, `DottedCircleFilter`

Considered and not taken:
- `fontbakery` 1.1.0: released 2025-10, CI 3.9 to 3.12, superseded by fontspector in gftools `qa`
- `fontmake` 3.12.2: a CLI over ufo2ft with CI at 3.10, 3.11 and 3.13, so the library calls ufo2ft directly
- `fontFeatures` 1.9.0 and fez: repository quiet since 2025-09 and fez archived, lookup records and `feaLib.ast` cover the need
- `ufo-extractor` 0.8.1: recovers anchors from GPOS alone and features through fontFeatures, import reads tables through `ttLib`
- `fontc` 1.0.0: TTF output alone, no overlap removal, no custom writers, no binary import
- `write-fonts` 0.55.0: `RUST.md` [03] lists it under work no crate states

## [06]-[LOGIC]

Algorithms the research proved, kept as pure functions with their correction:

| [INDEX] | [FUNCTION]           | [COMPUTES]                                                            | [CORRECTION]                             |
| :-----: | :------------------- | :-------------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | `faces.anchors`      | Named anchors from GPOS MarkBase and MarkLig, classes voted to a side | Ligature arms keep one anchor each       |
|  [02]   | `faces.names`        | AGL or `uniXXXX` names with form suffixes from GSUB singles           | Parsed once into `GlyphName`             |
|  [03]   | `outlines.stroke`    | Mean stroke width as 2·area/perimeter, counter width, signed gap      | Unchanged                                |
|  [04]   | `outlines.nib`       | Broad-nib stroke from a cubic skeleton split at nib-parallel tangents | Unchanged                                |
|  [05]   | `outlines.tidy`      | Spike, sliver and speck removal, retraced segment repair              | Unchanged                                |
|  [06]   | `rules.marks`        | Vowels from the face's own alif, waw and medial seen on one mark pen  | Own marks first, band from `Genre`       |
|  [07]   | `rules.anchors`      | Base anchors from ink with clearance from the original's shaping      | Reference face as a declaration value    |
|  [08]   | `rules.nuqta`        | Single, double and triple dot groups reseated on the canonical nuqta  | Membership by role, hamza never a dot    |
|  [09]   | `rules.clashes`      | `calt` alternates lifting or sliding a nuqta group clear              | Closed-form lift from signed distance    |
|  [10]   | `rules.neighbours`   | Contextual GPOS lifts from a column-span ink model                    | Letter-first inputs, lifts before `mkmk` |
|  [11]   | `rules.joins`        | Join band with foot at y = 0, overlap bars, tatweel cut to 0.25 em    | Band and overlap from `Genre`            |
|  [12]   | `rules.digits`       | Persian and Arabic digit forms from skeletons on the face's nib       | Distinctness as a requirement            |
|  [13]   | `rules.locale`       | ٫ ٬ from reh and comma, ؉ ؊ from ٪, ۀ from ه and hamza, `FAR` `locl`  | Unchanged                                |
|  [14]   | `rules.spacing`      | Word space from gap ratio over shaped profiles, class kerning         | `KernFeatureWriter` groups               |
|  [15]   | `rules.metrics`      | `typo` and hhea from shaped corpus ink inside the pitch band          | Cap as a `Metrics` requirement           |
|  [16]   | `rules.matrix`       | Dot diameter and pitch measured from a dot-matrix face's glyphs       | `DotMatrix` genre rule, not a script     |
|  [17]   | `rules.ornaments`    | Ornament re-encoding to contiguous code points, duplicate art merged  | Code points from declaration             |
|  [18]   | `verification.seam`  | Join overlap as intersection area of neighbours in a shaped run       | Geometry in place of column darkness     |
|  [19]   | `verification.marks` | Exhaustive mark on letter and stack runs with signed gaps             | Neighbours and whole words included      |

## [07]-[BOUNDARIES]

- Originals, set declarations, approved shaping JSON and built fonts sit together outside the repository, named by relative paths
- `publish` copies built files to a destination argument and compares hashes, deleting nothing
- Private Use Area ornaments have bidi class L, and the typesetting library that places them inserts the mark that keeps line order
- Reference faces for Perso-Arabic text are unhinted with `prep` and `gasp` alone, so `Hinting` stays out of every text plan by default
