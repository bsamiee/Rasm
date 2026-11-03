# [INTERFACES]

Shared contract for the `libs/python/typography` packages and `tools/foundry`, read by every lander with `DECISION.md`, `ARCHITECTURE.md`, `docs/research/coding/python/font-engineering.md` and the `python-coding` skills. Where this file and `ARCHITECTURE.md` differ, this file wins; it exists so packages built in parallel meet. Names below are the public surface each package exports; a lander adds private members freely and renames nothing listed.

## [01]-[LAYOUT]

```text
libs/python/typography/           uv workspace member `typography`, import root `typography`
  faces/      faults.py names.py glyphs.py layout.py declaration.py coverage.py importing.py
  outlines/   paths.py measure.py nib.py tidy.py
  shaping/    runs.py expectations.py corpus.py
  rules/      scripts.py plans.py emission.py measures.py locale.py digits.py fits.py marks.py joins.py scale.py anchors.py spacing.py metrics.py ornaments.py
  verification/ requirements.py geometry.py
  builds/     pipeline.py
tools/foundry/                    uv workspace member `foundry`, import root `foundry`
  app.py commands.py
tests/python/libs/typography/<package>/test_*.py
tests/python/tools/foundry/test_*.py
assets/fonts/rasm.toml            the set declaration beside its faces
assets/fonts/corpus.toml          the shaping corpus the declaration names
```

- Imports point downward: `faces` imports no sibling package; `outlines` imports `faces`; `shaping` imports `faces` and `outlines`; `rules` imports the three; `verification` imports the three and nothing from `rules`; `builds` imports every package; `foundry` imports `builds` and `verification`.
- Every module: section dividers per `CLAUDE.md` [04], `lazy` imports for names no class statement or annotation reads, `__all__` under `[EXPORTS]`, docstrings per `python-document`, no comments.
- Every record is `msgspec.Struct, frozen=True, kw_only=True`; unions use one tagged base per package with `tag_field="kind", tag=str.lower`.
- Every fallible function returns `Outcome[T]` from `typography.faces.faults` (written, read it first); `Fault.of(Reason.X, subject, cause, member=)`; independent checks combine through `accumulated` and `paired`.
- `uharfbuzz` and `pathops` objects never appear in a signature, a record field or a module constant: they live inside function bodies, and functions exchange the records below.
- Line budgets (`ARCHITECTURE.md` [08]) are hard: faces 600–900, outlines 400–600, shaping 250–400, rules 850–1250, verification 300–450, builds 150–250, foundry 100–150. Count with `loc`.

## [02]-[FACES]

Landed surface (`loc` 696 lines, 7 modules). `faults`: `Reason`, `Fault`, `Faults`, `Outcome`, `accumulated`, `paired`. `names`: `Form`, `GlyphName` (`codes`, `form`, `variant`, `base`, `text`, `parse`, `LIMIT` the 63-character `post` and AGL bound). `glyphs`: `Point`, `Contour`, `Outline`, `Anchor`, `Component`, `Joining` (`of(code)` from `youseedee`), `Side` (`of(code)` from the combining class), roles `Letter`, `Mark`, `Ligature`, `Digit`, `Sign`, `Ornament`, `Space`, `Control` as `Roles`, `role(name)`, `Glyph` (`name`, `role`, `advance`, `contours`, `components`, `anchors`; `codes`, `category`, `anchor()`; read `glyph.name.text`, no `key`), `Info`, `Metrics`, `Source` (`face`, `upm`, `glyphs`, `info`, `metrics`, `lookups`; `named`, `encoded`, `glyph()`, `by_role()`, `replaced()`, `without()`). `layout`: `Adjustment`, `Single`, `Ligatures`, `Context`, `Contextual`, `Positions`, `Pairs`, `Lift`, `Lifts`, `Lookups`. `coverage`: `Coverage(required, excluded, blocks)` with `uncovered`, `unrecorded`, `exclusion`, defaults the Perso-Arabic text set, `Excluded(codes, reason)`. `declaration`: `Em`, `Ratio`, `Band`, genres `TextNaskh`, `BlackNaskh`, `Thuluth`, `Kufi`, `DotMatrix`, `Ruqah`, `Diwani` as `Genres` over one `Genre` record (bands, `alif`, `word_ratio`, `word_band`, `join_overlap`, `nuqta_floor`, `stack_gap`, `clearance`, `mark_floor`, `hinting`, `coverage`, `spurs`), treatments `Sheared`, `Ringed`, `Contour`, `Shadow`, `Segmented` as `Treatments` (descriptors of a finished face), `Values` (scalar overrides a rule reads), `Face` (`style`, `original`, `suffix`, `family`, `weight`, `width`, `treatments`, `values`, `genre`; `typographic`, `stem`), `OrnamentFace` (`first` alone), `Faces`, `Family`, `OrnamentSet`, `FontSet` (`name`, `corpus`, `output`, `families`, `ornaments`; `faces`, `folders`), `Declared` (`set`, `root`; `corpus`, `original()`, `source()`, `binary()`, `check()`, `approved()`), `declared(path)`. `importing`: `imported(path, face) -> Outcome[Source]` reading cmap, GSUB forms and ligatures, GPOS anchors by voted side, Unicode roles and joining types, exclusions from `genre.coverage`.

## [03]-[OUTLINES]

Landed surface (`loc` 277 lines, 4 modules). `paths`: `Box` (with `of`, `band`, `strips`, `grown`, `&`, `|`, `contour`), `Matrix`, `Axis`, `Segment`, `Operation` (`combined(a, b, Operation)` for difference, intersection and xor), `union`, `area`, `bounds`, `iou`, `pieces`, `transformed(outline, matrix)`, `offset(outline, distance)` (grow or shrink, mitred), `strip(outline, low, high, axis)`, pens `trace`, `draw`, `traced`, `segments`. `measure`: `stroke`, `perimeter`, `counter`, `gap(a, b, axis=None)` (signed, overlap depth negative), `restroked`, `column_ink(outline, box, columns, axis)`, `moments`/`Moments`. `nib`: `swept`, `monoline`, `extruded`. `tidy`: `tidied`, `despiked`, `snapped`.

## [04]-[SHAPING]

Landed surface (`loc` 188 lines, 3 modules). `runs`: `Binary` (`of`, `read`; `data`, `upm`, `order`), `Shaper`, `Direction`, `Placed` (UTF-8 byte clusters as `hb-shape`), `Shaped` (`placed`, `width`, `names`, `clusters`, `serialized` without brackets, the fontspector form), `shaped(binary, text, *, language, features, shaper, direction)`, `drawn(binary, run)` (placed outlines; consumers union or bound them), `outline(binary, glyph)`, `encoded(binary)`, `marks(binary)`, `timed`. `expectations`: `Defaults`, `Expectation` (extends `Defaults`, options flat as fontspector reads them), `Configuration`, `Expectations`, `encoded`, `decoded`, `expected(binary, corpus)`. `corpus`: `Corpus` with `prose` keyed by BCP 47 language, `marked`, `annotated`, `deep`, `words`, `digits`, `punctuation`, `joins`, `seam`, `zwnj`, `ligatures`, `letters`, `vowel_stacks`, `stacks`, properties `languages`, `plain`, `lines`, `matrix`, `word_runs`, methods `forms(code, marks)`, `pairs(around, marks)`; `corpus(path)` decodes `assets/fonts/corpus.toml`.

## [05]-[RULES]

Landed surface (`loc` 1236 lines, 14 modules). `scripts`: `Rule` (tagged base), `Script` (the code points and texts one writing system's rules read, stated once as data), `Recipe`, `Derived`. `plans`: `Plan` (order invariant), `Rules`, `Hinting`, `planned(face) -> Plan` resolving every value from the genre and `face.values`, `applied(source, rule, *, original, corpus)`, `executed(source, plan, *, original, corpus)`; treatments describe a finished face (angle, ring, channel, shadow, segmentation) and are read by measurements and records, never applied. `emission`: `emitted(source) -> Emitted(features, groups, kerning)` through `feaLib.ast`, lifts before the `# Automatic Code` marker, kern pairs as `KernFeatureWriter` groups. Rule modules, each one record and one operation: `locale`/`localized` (coverage fill from the face's own glyphs, controls, presentation forms, `FAR` `locl`, `rlig` lam-alef), `digits`/`figured` (`pnum` and `tnum`, digit forms), `fits`/`fitted` (union, tidy, nuqta groups and floor, spurs by `genre.spurs`), `marks`/`penned`, `quranic`, `signed` (one pen, Quranic marks, word signs, `ccmp`), `joins`/`joined`, `scale`/`scaled`, `anchors`/`seated` (anchors from ink, `calt` dot clearance, lifts), `spacing`/`spaced`, `metrics`/`measured`, `recorded`, `ornaments`/`encoded` (Private Use run from `first`, duplicate art folded by outline equality, tiles by negative bearings, ink-tight metrics), `measures` shared measurement helpers.

## [06]-[VERIFICATION]

Landed surface (`loc` 390 lines, 2 modules). `requirements`: the requirement records `Sanitized`, `Profile` (fontspector profiles, `universal` default), `Expectations`, `Reachable`, `Coverage`, `MarkClearance`, `MarkLine`, `LigatureInk`, `JoinParity`, `Seam`, `Distinct`, `Metrics`, `Identity`, `Reproducible` (`Speed` dropped: no source states its ratio), `Requirements`, `Finding(requirement, subject, measured, bound)`, `requirements(face, *, recorded)`, `verified(binary, requirements, *, source, original, corpus, path, approved) -> tuple[Finding, ...]`, `approved(binary, corpus, path)`; OTS and fontspector run inside it (JSON configuration, one temporary folder per run). `geometry`: `clearance`, `mark_line`, `ligature_ink`, `join_parity`, `seam`, `distinct`, `metrics`, `identity`, `reachable`, `Breach`; each run shaped once and cached across requirements.

## [07]-[BUILDS]

Landed surface (`loc` 234 lines, 1 module). `Action` (source, build, check, approve, run, publish), `Built(face, path, hash, inputs, findings)` with `inputs` over the face's original bytes, its declaration record, its plan record, the corpus and the source text of the modules its plan's rules and the compile dispatch to, `Report` (one JSON line per face, empty fields omitted), `sourced`, `written` (ufoLib2 `info` and the `public.*` lib keys ufo2ft reads), `compiled` (ufo2ft `compileTTF` with the four writers; STAT and `head.modified` the two hand edits ufo2ft lacks), `built -> (Built, Source)`, `current`, `checked` (reads the recorded build; `Reproducible` compares the file hash with the record), `published` (TTF and WOFF2), `performed(action, declaration, *, name, destination)`.

## [08]-[FOUNDRY]

Landed surface (`loc` 30 lines, 2 modules). `app.py`: one `App` with `exited` (JSON on stdout, each fault once on stderr, sysexits codes by reason). `commands.py`: one command `foundry ACTION DECLARATION [NAME] [DESTINATION]`, `NAME` the set, a family, an ornament set or a face stem, no other flag; installed through `nx run foundry:install` (`uv tool install --python "$(mise which python)"`).

## [09]-[DECLARATION_AND_PROOF]

- `assets/fonts/rasm.toml` declares the set (`name = "Rasm"`, `corpus = "corpus.toml"`, `output = "../../.artifacts/fonts/rasm"`): every family with its asset folder as `original`, each face its asset file, genre, weight, width, treatments as descriptors of the finished face, and the scalar `values` a rule reads. Ornament sets state `first` alone.
- Proof families: `Satr` (`assets/fonts/Satr/Satr-Regular.ttf`, text Naskh) and `Tazhib` (`assets/fonts/Tazhib/Tazhib-Regular.ttf`, ornament set), through `foundry run assets/fonts/rasm.toml <name>`; the build of a finished face keeps its cmap, feature tags, metrics and corpus shaping and passes verification.
- Tests build their fonts with `fontTools.fontBuilder.FontBuilder` and read neither `assets/fonts` nor `docs/research`.

## [10]-[BAR]

Owner's additions, binding for every lander and adversary:
- Relentless depth and capability in typography, fonts, shaping and the CLI; each dependency (fontTools, ufo2ft, uharfbuzz, skia-pathops, ots, ttfautohint-py, youseedee, msgspec, cyclopts, expression) used through its own members to its full capability, read from its installed source under `.venv/lib/python3.15/site-packages`, never hand-written over.
- Values derive from the font, OpenType and Unicode data or the package that states them; related quantities sit in one algebraic relation; one parameterized operation replaces variants; nothing hardcoded.
- Constants, string sets, small records and single-use helpers fold into fewer, richer objects; public names are only what a consumer composes, listed in `__all__`, everything else `_`-private.
- Newest Python forms (3.15 and before: `lazy` imports, type parameter syntax, `type` aliases, pattern matching, comprehension unpacking, deferred annotations) and PEP standards; flat expression-oriented control flow; fewer lines, files and exports. Nothing deferred, nothing kept for compatibility. Any naive shape carried in from the research or the plan is fixed, not inherited.
- The CLI follows the repository's tool rule: one command answers with no flag beyond the question; work is incremental by content (a face rebuilds only when its inputs or rules changed); each command returns concise, agent-facing results and failures with their cause (shaping, outlines, verification findings), never a wall of output.
- Checks each lander runs on its files: `ruff check`, `ruff format --check`, `ast-grep scan <paths>`, `mypy <paths>`, `ty check <paths>`, `pytest <its tests> -p no:randomly -q`, all at zero findings. The member manifests state `requires-python = ">=3.13"` (a lock fact keeping the 3.13 forks for Blender hosts); root Ruff `per-file-target-version` sets the library and `tools/foundry` to py315.
- Fontspector 1.9.0 on this machine holds the profiles `universal`, `opentype`, `googlefonts`, `fontwerk`, `iso15008`, `monotype` and no `adobe` profile; `Profile` defaults to `("universal",)`. Its JSON report is `{"results": {"<path>": {"<section>": [{"check_id", "worst_status", "subresults": [{"message", ...}], ...}]}}, "summary": {...}}`.
- The library and `foundry` are a general tool for building, refining and verifying any font; Perso-Arabic is one domain handled in depth, never the frame the code is shaped around: script facts (joining, coverage, mark sides, language systems) are data the declaration and Unicode state, genres and rules are parameterized records any script can declare, and no module name, constant or branch assumes Arabic.
- Nothing from the old tooling carries over. A change rebuilds only what it reaches: a face's `inputs` hash covers its original bytes, its declaration record, the corpus and the source text of the modules its plan's rules and the compile dispatch to, never the whole library, so an edit to one rule module rebuilds the faces whose plans hold that rule and no other.
- Control is fine-grained through the domain's own objects (a face, a family, a rule, a requirement), and the CLI takes no flag a question does not need; capability integrates into one operation rather than splitting into variants.
- Adversary pass per package: the packages carry type, class, constant and string spam; fold each into fewer, richer objects holding their operations, raise logic to higher-order operations for fonts in general (Perso-Arabic one domain of it), and cut lines, files and exports wherever the stronger form states the same capability. Each package reports lines, files and exports after the pass.
- Owner's reframing: the old original files are of no interest. The library works from the current faces in `assets/fonts` (each family folder holds its faces) and states universal Perso-Arabic font principles, not Urdu or Desi forms and not any one font. The set declaration sits beside those faces as `assets/fonts/rasm.toml` with `corpus.toml` beside it, each family's `original` folder its asset folder and each face's `original` its asset file, and `output` under `.artifacts/fonts/rasm`. No rule, requirement or pipeline step reads another face's binary, a `beside` set, a reference face or a per-font glyph list: a capability stands only as general logic every face gets, derived from the face's own data and Unicode.
