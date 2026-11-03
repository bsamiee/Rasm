# [DECISION]

Keep: the font tooling becomes a Python library under `libs/python/typography/`, rebuilt from its research findings and from none of its code.

## [01]-[PROBLEM]

Third-party and vendor fonts reach verified, standards-complete, script-correct deliverables. Each face is reconciled from its original binary through declared rules into a source, compiled, verified against one set of requirements, and packaged, and a rebuild from the same original and rules yields the same bytes.

InPage originals show what such fonts lack (`observations.toml` `[summary]`, `SPEC.md`, `PLAN.md` [03.8]):
- GDEF headers OTS rejects in 63 of 87 fonts, `GlyphClassDef` pointing at byte 10 of the 12-byte header
- Word space of 0.025 em in every kept face against 0.13 to 0.29 em in reference faces
- Joins abutting within 1 unit with no overlap in 47 of 78 text fonts, printing hairline seams
- Nonzero mark advances, blank harakat in 25 styles, one fatha outline reused across 11 faces
- Urdu forms on U+06F4 and U+06F7, no `FAR` language system, no pair kerning, metadata grouping nothing

Repairs split into design decisions per face class and engineering every face shares, and a library holds the engineering half.

## [02]-[DURABILITY]

One font set with a rebuild loop carries the decision, a second set carries none of it:
- InPage is the one set: 38 text families and 5 ornament sets kept, 11 families tossed (`PLAN.md` [01])
- Rule changes rebuild every face, as fix rounds rebuilt the built families after each shared-pipeline change
- Originals come from the plug-in's encrypted resources (`fonts/inpage/README.md`), so a plug-in release can carry new font versions
- `TODO.md` adds plug-in 2.2 for InDesign 2026 and its macOS build to the decompile when they arrive
- `@rasm/typography`, `@rasm/indesign` and atelier read each installed face as given, so only a build-time requirement catches a face defect
- Sulus shipped a لا without its lam through OTS, fontbakery and the shaping baseline (`PLAN.md` [04]), so requirements measure geometry

Gap: no planned work rebuilds another set, and the drive's premium font folders join Typeface alone (`TODO.md`). Root `[TYPOGRAPHY]` rows stand without an importer under the catalog rule, and ufo2ft, opentype-sanitizer and fontbakery joined them on 2026-11-02 with this rebuild, so they show no second consumer.

Upkeep: `docs/research` is git-ignored, so the tools hold no history and `rebuild.py` is the only copy of evidence `PLAN.md` cites. On 2026-11-02 `tools/` held 10,638 Python lines, 484 module constants and an 899-line `families.toml` behind an untyped `design: dict`, edited by concurrent agents. A library puts that logic under the repository's checkers, tests and version history.

## [03]-[SHAPE]

Library capability with a thin command project, holding only work no tool does:
- Packages under `libs/python/typography/`, each with `py.typed`: core `faces`, `outlines`, `shaping`, pipeline `rules`, `verification`, `builds`
- `builds` is the one pipeline over every face class, and every package is importable by any caller: the CLI, tests, a later host workflow
- Core packages import nothing from the pipeline packages, so checking and build tooling never couples into them
- `ufo2ft` compiles through its writers and filters, fontspector and OTS judge generic conformance, and the library supplies their inputs
- `tools/foundry/` is the CLI project: one cyclopts `App` holding commands, parameters and output alone, every behavior in the library
- `foundry build <set.toml>` and `foundry check <face>` run from PATH, never through `python -m`
- Font-set declarations decode through `msgspec.toml` into frozen `Struct` records, pydantic stays out with no untrusted network boundary
- Command parameters are the library's own scalar types, enums and paths, a `Struct` never binds a token (cyclopts leaves it unchecked)
- Font sets are inputs: one TOML declaration per set beside its originals, read by path argument
- InPage's licence allows no copying, so originals and declaration stay on the drive, and the library holds no family name, path or value
- Tests under `tests/python/libs/typography/<package>/` build their fonts with `fontTools.fontBuilder`, so the repository holds no licensed binary

Face roles, outline geometry, design rules and requirements are domain code with tests, outside the `eng/` machinery root targets call.

## [04]-[PLACE]

- `libs/python/typography/pyproject.toml` is one packaged uv workspace member on the `blender` pattern, joined to root `members` and `uv.lock` first
- Later typography work joins the same member as further top-level packages
- Manifest: hatchling, `dev-mode-dirs = [".."]`, `[project] dependencies` naming each import, `[tool.ruff] extend`, `[tool.deptry]`
- `tools/foundry/pyproject.toml` is a packaged member with `[project.scripts] foundry = "foundry.app:app"` depending on `typography` by workspace source
- `tools/foundry` takes an `install` target running `uv tool install` over the project, so `foundry` sits on PATH over its previous install
- Nx infers a project per `py.typed` directory with its `tests/python` folder, and ty, mypy and refurb run as root targets
- `follow_untyped_imports` rows for `ufo2ft.*`, `ots.*`, `ttfautohint.*` and `youseedee.*` join the `fontTools.*` and `pathops.*` row
- `untyped_calls_exclude` takes `fontTools.feaLib`, `fontTools.pens`, `ufo2ft` and `ots`, whose calls mypy strict reports as untyped
- `fontspector` takes a `mise.toml` `"github:fonttools/fontspector"` row with `version_prefix = "fontspector-v"`, the repository tagging every crate
- `uharfbuzz` 0.57.0 bundles HarfBuzz 15.0.0 with the `ot` shaper and, on macOS, `coretext`, so no HarfBuzz binary joins `mise.toml`
- Use `docs/research/coding/python/font-engineering.md` for ecosystem facts

## [05]-[LANGUAGE]

Python at the mise 3.15.0 release, decided by stage coverage:

| [INDEX] | [STAGE]         | [PYTHON]                                  | [RUST]                                      |
| :-----: | :-------------- | :---------------------------------------- | :------------------------------------------ |
|  [01]   | Binary import   | `fontTools.ttLib`, ufoLib2                | `read-fonts` reads, `norad` writes, no path |
|  [02]   | Feature writing | `ufo2ft` writers, `feaLib.ast`            | `fontc` writers closed, `fea-rs` parses     |
|  [03]   | Compile         | `ufo2ft` TTF and OTF, overlap removal     | `fontc` TTF alone, no overlap removal       |
|  [04]   | Hint            | `ttfautohint-py`                          | None                                        |
|  [05]   | Verify          | `opentype-sanitizer`, fontspector process | `fontspector`, harfrust                     |
|  [06]   | Shape           | `uharfbuzz` on HarfBuzz 15.0.0            | `harfrust` 0.15.0                           |
|  [07]   | Repository rule | Catalog rows present                      | `write-fonts` excluded by `RUST.md` [03]    |

TypeScript reads fonts through fontkit and harfbuzzjs and compiles none, so it holds the runtime side alone. Typing is Python's cost: uharfbuzz and skia-pathops types stay out of signatures under the root mypy settings (`font-engineering.md`), so `shaping` and `outlines` keep those values inside function bodies and return msgspec records.

## [06]-[DEFECTS]

Findings of the code study and the journal that a rebuild leaves behind:

| [INDEX] | [DEFECT]                                                                | [REPLACEMENT]                                          |
| :-----: | :---------------------------------------------------------------------- | :----------------------------------------------------- |
|  [01]   | GSUB and GPOS as `.fea` strings, a lib string reparsed in `AfterMarks`  | Lookup records emitted once through `feaLib.ast`       |
|  [02]   | `design: dict` with over 80 keys read by string, `klass: str`           | Genre and treatment records per face                   |
|  [03]   | `importer.LEFT_OUT` drops assigned code points without record           | Coverage with an `Excluded` record per code            |
|  [04]   | `ibteda.py`, `xebec.py` each holding `sourced`, `design.py` run by path | One plan per genre and treatment, values declared      |
|  [05]   | Checks as `tuple[str, bool]`, each mark tested on its own base alone    | `Finding` records from one relation                    |
|  [06]   | Shaping baseline written when missing or under `--update`               | Expectations approved from a named build hash          |
|  [07]   | Nuqta recognized by area and box tolerance, hamza became a dot          | Roles from Unicode decomposition and GSUB components   |
|  [08]   | `body_and_dots` keeps a ligature's largest contour, Sulus lost its lam  | Ligature components by GSUB decomposition              |
|  [09]   | `while` loops stepping a shift until a gap clears                       | Closed-form gap and lift from signed distance          |
|  [10]   | `provisional_span` mutating the real UFO, `cache` on a mutable font     | Immutable face records, pure measurement               |
|  [11]   | Reem Kufi under `Path.home()`, Drive path holding the owner's email     | Reference faces and outputs as declaration values      |
|  [12]   | `PathOpsError` to 0.0, fontbakery `check=False` with ERROR as FAIL      | `Outcome` with a `Fault` per cause                     |
|  [13]   | Metrics from `texts.py` lines, pitch past the 2.04 em cap               | Metrics from the shaped corpus, bounds as requirements |
|  [14]   | `kern_pairs` as XAdvance on the first glyph, lifts after `mkmk`         | `KernFeatureWriter`, lifts before the writer marker    |
|  [15]   | Builders locked out of `rules.py`, PLAN prose as the rule store         | Rules in code and declarations, checked and versioned  |
|  [16]   | `fontbakery` 1.1.0 at CI 3.9 to 3.12                                    | `fontspector` 1.9.0 with its JSON report               |
|  [17]   | Private `Subsetter._closure_glyphs`                                     | `Subsetter.subset` over a copy, retained glyph order   |

## [07]-[BUILD_BAR]

Total size is 2,500 to 4,000 lines against the 10,638 lines of the current tooling, and the build reads that tooling's functionality and findings, never its code:
- Each capability takes its higher-order form: genres, treatments and rules as data, verification as one relation, one pipeline over every face class
- Each form builds on the chosen packages' own members, so nothing fontTools, ufo2ft, uharfbuzz, skia-pathops, OTS or fontspector states is restated
- Each module holds one concept at its natural size, and no package or folder holds a single small file
- Use `ARCHITECTURE.md` for each package's line budget and the current surface the rebuild deletes
