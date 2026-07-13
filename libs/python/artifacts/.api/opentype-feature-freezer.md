# [PY_ARTIFACTS_API_OPENTYPE_FEATURE_FREEZER]

`opentype_feature_freezer` supplies the GSUB-into-`cmap` feature-freezing engine for the artifacts font rail: the single `RemapByOTL` class drives a font through a fixed open->remap->rename->save pipeline that "bakes" a chosen set of OpenType GSUB single (LookupType 1) and alternate (LookupType 3) substitutions into the font's default Unicode-to-glyph mapping, so features such as `smcp`/`c2sc`/`onum`/`ss01` become on-by-default even in non-OpenType-aware consumers, and rewrites the `name`/CFF naming tables (RIBBI family rename, OFL reserved-name replacement, version stamping). It composes `fontTools.ttLib.TTFont` for all binary IO and table access and owns no font model of its own. This is the categorical-best feature-freeze owner; the `typography/font#FONT` `FontEngineering` owner reaches it as a `FREEZE` `FontOp` arm — the inverse-direction transform from `SUBSET` (subset prunes the glyph set; freeze rewrites `cmap` to point default codepoints at the substituted glyphs). The owner never re-walks GSUB `ScriptList`/`FeatureList`/`LookupList` by hand, never re-derives the alternate-class first-pick, and never hand-edits the `name`/CFF name records — `RemapByOTL` owns the whole remap-and-rename fold.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentype_feature_freezer`
- package: `opentype-feature-freezer`
- import: `opentype_feature_freezer`
- owner: `artifacts`
- rail: fonts
- license: Apache-2.0
- installed: `1.32.2` (pure-Python; abi-agnostic, present on cp315)
- depends: `fonttools (>=4.0,<5.0)` — the binary-font model and table objects the remap reads/writes
- entry points: `pyftfeatfreeze` console script (`opentype_feature_freezer.cli:main`); library use is `RemapByOTL(options)` + the `cli.main`/`cli.parseOptions` argv helpers
- capability: freeze a comma-separated GSUB feature set into a font's `cmap` by applying single (LookupType 1) and alternate (LookupType 3, first alternate; including extension LookupType 7) substitutions, optionally script/language filtered; RIBBI family rename with feature-derived or custom suffix; OFL `search/replace` name mutation; `name`-ID 3 (unique-ID stamp) and ID 5 (version stamp) feature annotation; `post` v3 glyph-name zapping; a report mode enumerating the font's scripts/languages/features

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: feature-freeze engine and its options carrier
- rail: fonts

`RemapByOTL` is the single owner; it is constructed with one `options` object whose attribute set IS the freeze policy (the CLI builds it as an argparse `Namespace`, a library caller builds it as any attribute carrier — a `types.SimpleNamespace` or a typed struct exposing the same field names). The engine holds the live `fontTools.ttLib.TTFont` as `.ttx`, the glyph-substitution mapping as `.substitution_mapping`, and a `.success` boolean the pipeline short-circuits on. There is no functional `freeze(bytes) -> bytes` entry — the public contract is "construct with options, call `run()`, the rewritten font is written to `options.outpath`".

| [INDEX] | [SYMBOL]                               | [PACKAGE_ROLE] | [CAPABILITY]                                                                  |
| :-----: | :------------------------------------- | :------------- | :---------------------------------------------------------------------------- |
|  [01]   | `opentype_feature_freezer.RemapByOTL`  | freeze engine  | `RemapByOTL(options)`; the open->remap->rename->save pipeline                 |
|  [02]   | `RemapByOTL.options`                   | freeze policy  | the attribute carrier holding the freeze-policy fields (the [02] table)       |
|  [03]   | `RemapByOTL.ttx`                       | live font      | `Optional[TTFont]`; `None` pre-`openFont()`, closed by `closeFont()`          |
|  [04]   | `RemapByOTL.success`                   | pipeline gate  | `bool`; each stage sets it, `run()`/`remapByOTL()` short-circuit when `False` |
|  [05]   | `RemapByOTL.substitution_mapping`      | remap table    | old->new glyph-name `MutableMapping[str,str]` over every `cmap` subtable      |
|  [06]   | `opentype_feature_freezer.__version__` | version anchor | the installed package version string (`"1.32.2"`)                             |

[PUBLIC_TYPE_SCOPE]: the options field set (the freeze policy surface)
- rail: fonts

These attribute names are the load-bearing knob set the owning design page must populate on the options carrier; they are the argparse `dest=` names and are read directly by `RemapByOTL`. A typed options struct in the design composes exactly this field set.

| [INDEX] | [FIELD]        | [TYPE]                  | [CAPABILITY]                                                                            |
| :-----: | :------------- | :---------------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `inpath`       | `os.PathLike`           | input `.otf`/`.ttf` path opened via `TTFont(inpath, 0, recalcBBoxes=False)`             |
|  [02]   | `outpath`      | `os.PathLike` \| `None` | output path; `None` derives `<inpath>.featfreeze.otf`                                   |
|  [03]   | `features`     | `str`                   | comma-separated GSUB feature tags to freeze, e.g. `"smcp,c2sc,onum"` (split on `,`)     |
|  [04]   | `script`       | `str` \| `None`         | OpenType script tag filter, e.g. `"cyrl"`; `None`/falsy matches every `ScriptRecord`    |
|  [05]   | `lang`         | `str` \| `None`         | OpenType language tag filter, e.g. `"SRB "`; `None` uses each script's `DefaultLangSys` |
|  [06]   | `zapnames`     | `bool`                  | set `post.formatType = 3.0` (drop glyph names; `.ttf` only) before save                 |
|  [07]   | `suffix`       | `bool`                  | append a family-name suffix; default suffix is the sorted frozen feature tags           |
|  [08]   | `usesuffix`    | `str`                   | explicit suffix string used when `suffix` is set (overrides the feature-derived one)    |
|  [09]   | `replacenames` | `str`                   | OFL name mutation, `"search1/replace1,search2/replace2"` applied to the family name     |
|  [10]   | `info`         | `bool`                  | stamp the freeze feature list into the `name`-ID 5 version string                       |
|  [11]   | `report`       | `bool`                  | report-only: print scripts/languages and features (`-s`/`-l`/`-f` form), write nothing  |
|  [12]   | `names`        | `bool`                  | collect+print the substituted (remapped) glyph names during processing                  |
|  [13]   | `verbose`      | `bool`                  | raise the module logger to `INFO` (consumed by `cli.main`, not by `RemapByOTL`)         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the freeze pipeline
- rail: fonts

`run()` is the single orchestration entry and the canonical call. It opens the font, runs `remapByOTL()` (the GSUB->`cmap` fold), renames, saves to `outpath`, and closes — short-circuiting on `.success`. The constituent methods are public and individually callable for a caller that needs to drive a pre-opened `TTFont` or interleave the stages, but `run()` is the contract; each stage method returns `None` save `renameFont() -> bool`, mutates `self`, and is not pure.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]           | [CAPABILITY]                                                                  |
| :-----: | :------------------------------ | :--------------------- | :---------------------------------------------------------------------------- |
|  [01]   | `RemapByOTL(options)`           | `RemapByOTL(options)`  | construct the engine over the options carrier; derives `outpath` if unset     |
|  [02]   | `RemapByOTL.run`                | `run()`                | the full open->remap->rename->save pipeline, gated on `.success`              |
|  [03]   | `RemapByOTL.openFont`           | `openFont()`           | open the font into `.ttx` via `TTFont(inpath, 0, recalcBBoxes=False)`         |
|  [04]   | `RemapByOTL.remapByOTL`         | `remapByOTL()`         | the GSUB->`cmap` fold: `initSubs` then rows [05]-[08]                         |
|  [05]   | `RemapByOTL.filterFeatureIndex` | `filterFeatureIndex()` | resolve the `FeatureIndex` set from `GSUB.ScriptList` under `script`/`lang`   |
|  [06]   | `RemapByOTL.filterLookupList`   | `filterLookupList()`   | resolve the `LookupList` indices for `features` over the `FeatureIndex`       |
|  [07]   | `RemapByOTL.applySubstitutions` | `applySubstitutions()` | fold LookupType 1/3/7 subs to `.substitution_mapping`; warn no-`cmap` remap   |
|  [08]   | `RemapByOTL.remapCmaps`         | `remapCmaps()`         | rewrite every `cmap` subtable entry through `.substitution_mapping`           |
|  [09]   | `RemapByOTL.renameFont`         | `renameFont() -> bool` | RIBBI/OFL `name` + CFF top-dict rename; no-op without `suffix`/`replacenames` |
|  [10]   | `RemapByOTL.saveFont`           | `saveFont()`           | `report` prints enumeration; else `zapnames` then `ttx.save(outpath)`         |
|  [11]   | `RemapByOTL.closeFont`          | `closeFont()`          | `ttx.close()` the live font                                                   |

[ENTRYPOINT_SCOPE]: argv / CLI helpers
- rail: fonts

`cli.main` is the console-script body; `cli.parseOptions` builds the argparse `Namespace` whose attribute set the engine consumes. A design that drives the tool in-process bypasses these and builds the options carrier directly, but `parseOptions` documents the canonical option vocabulary.

| [INDEX] | [SURFACE]          | [CALL_SHAPE]                                               | [CAPABILITY]                                         |
| :-----: | :----------------- | :--------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `cli.main`         | `main(args: list[str] \| None = None, parser=None) -> int` | CLI: exists-check, log level, `run()`, exit code     |
|  [02]   | `cli.parseOptions` | `parseOptions(args=None) -> argparse.Namespace`            | options `Namespace`; `-f -s -l -S -U -R -z -i -r -n` |

## [04]-[IMPLEMENTATION_LAW]

[FREEZE_TOPOLOGY]:
- one class: `RemapByOTL` is the entire library; no module functions, no `freeze(bytes)->bytes` entry. The unit of composition is "build an options carrier, call `run()`, read `outpath`". The engine is stateful — `.ttx`/`.success`/`.substitution_mapping` are mutated across stages — so it is single-use per font.
- options-as-policy: the freeze policy is the attribute set on `options` ([02] field table). The engine reads `options.features.split(",")`, `options.script`, `options.lang`, the rename knobs, and the report/names flags directly. A `types.SimpleNamespace` is the minimal carrier; a typed `msgspec.Struct` exposing the same field names is the design-side owner.
- the freeze mechanism: `filterFeatureIndex()` walks `font["GSUB"].table.ScriptList.ScriptRecord`, intersecting on `options.script` (or all scripts) and `options.lang` (or each script's `DefaultLangSys`) to a `FeatureIndex` set; `filterLookupList()` maps the requested `features` over `FeatureList.FeatureRecord[fi].FeatureTag` to a `LookupList` index set; `applySubstitutions()` walks each `LookupList.Lookup[id].SubTable`, handling only `LookupType in {1, 3, 7}` (single, alternate, extension) — single uses `Subtable.mapping`, alternate takes `alternates[g][0]` (the first alternate), extension recurses into `ExtSubTable` — folding old->new into `.substitution_mapping`; `remapCmaps()` rewrites every `cmap` subtable entry through that mapping. A glyph whose old AND new name carry no `cmap` value is skipped with a warning (nothing to remap).
- the rename mechanism: `renameFont()` resolves the canonical family from `name.getName(16, 3, 1) or name.getName(1, 3, 1)`, applies the OFL `replacenames` search/replace and the suffix (custom `usesuffix` or sorted feature tags), then rewrites `name` records by ID — {1,4,16,18,21} get family search/replace, ID 3 gets a `;featfreeze:<features>` unique-ID stamp, ID 5 gets a `; featfreeze: <features>` version stamp only under `info`, {6,20} get the no-space PostScript-name replace — and mirrors `FamilyName`/`FullName`/PostScript name into the CFF top-dict for CFF fonts (raising on multi-font CFF).
- report vs write: `options.report` makes `saveFont()` print the script/language enumeration (gathered into `.reportLangSys` during `filterFeatureIndex`) and the feature enumeration (`.reportFeature` during `filterLookupList`) instead of writing — a discovery pass that answers "what features/scripts does this font carry" with no output font.

[STACKING]:
- fonttools-model seam: `opentype_feature_freezer` reads and writes through `fontTools.ttLib.TTFont` (`.api/fonttools.md` `TTFont` container row) — it shares the exact binary-font model the `typography/font#FONT` `SUBSET`/`INSTANCE`/`OUTLINE`/`EMBED_AUDIT` arms operate on. The freeze arm and the subset arm are two transforms over one `TTFont`: in a `FREEZE -> SUBSET` chain the freeze rewrites `cmap` to the default-glyph set, then `Subsetter.populate(unicodes=...)` prunes to the now-default glyphs, so the frozen smallcaps/oldstyle glyphs survive subsetting. fontTools owns the model; this package owns the GSUB->`cmap` remap fontTools does not provide as a one-call op.
- in-process boundary (path-only IO): `RemapByOTL.openFont` opens a path and `saveFont`->`ttx.save(outpath)` writes a path — there is no bytes-in/bytes-out entry. The owning `FREEZE` arm composes it exactly as `typography/shape#SHAPE` composes blackrenderer's path-only `saveImage`: write the input `font` bytes to a `tempfile.NamedTemporaryFile`, point `options.inpath`/`options.outpath` at temp paths, `RemapByOTL(options).run()`, read the output path back to `bytes`, `unlink` both in a `finally`. The freeze step thus returns the same `bytes` the `_FONT_TABLE` acceptor family produces, keyed by the runtime content key.
- universal-tier rails: the design-side options carrier is a `msgspec.Struct` (`libs/python/.api/msgspec.md`) frozen knob set spreading the [02] field names; `beartype` (`libs/python/.api/beartype.md`) validates the feature-tag/script-tag inputs at the boundary; the freeze step rides the same `expression`-`Result`/`RuntimeRail[ContentKey]` rail (`libs/python/.api/expression.md`) every `FontOp` arm returns, and the temp-path round-trip is bracketed so a freeze fault surfaces as a typed boundary failure, never a leaked file. Because the engine sets `.success` instead of raising on an unopenable/unsavable font, the arm reads `engine.success` after `run()` and lifts a `False` into the rail failure rather than trusting a silent return.
- shaping-vs-freeze seam: `typography/shape#SHAPE` (uharfbuzz) applies GSUB features at shaping time on a per-run basis (live, reversible); `FREEZE` bakes them permanently into `cmap` (one-time, irreversible, consumer-agnostic). They are complementary, not redundant — shaping serves the in-engine text run, freeze serves a deliverable font for non-OpenType consumers (LibreOffice, MS Office smallcaps). A design must not route a "make smallcaps default" request to a hand-rolled cmap rewrite when `FREEZE` owns it, nor to a uharfbuzz reshape when the goal is a frozen output font.

[LOCAL_ADMISSION]:
- the freeze step is `RemapByOTL(options).run()` over a temp-path round-trip, never a hand-walked `GSUB.ScriptList`/`FeatureList`/`LookupList` traversal nor a hand-built `cmap` rewrite — the engine owns the LookupType 1/3/7 fold and the alternate first-pick.
- the freeze policy is a typed options struct spreading the [02] field names (`features`/`script`/`lang`/`suffix`/`usesuffix`/`replacenames`/`zapnames`/`info`/`report`/`names`), never a loose `dict[str, object]` and never a parallel rename helper — `renameFont` owns the `name`/CFF rename and the OFL replace.
- the font model is `fontTools.ttLib.TTFont`; this package never re-parses sfnt tables — it consumes the same `TTFont` the rest of the font rail uses, so a `FREEZE -> SUBSET -> INSTANCE` chain stays on one model.
- a freeze fault is read off `engine.success` (the engine sets it `False` on open/save failure rather than raising) and lifted into the `RuntimeRail` failure; a `report`-mode run produces no output font and contributes a discovery receipt, not a deliverable one.

[RAIL_LAW]:
- Package: `opentype-feature-freezer`
- Owns: freezing a chosen GSUB single/alternate feature set into a font's default `cmap` (on-by-default features for non-OpenType consumers) plus the attendant RIBBI/OFL `name`+CFF rename and `post` glyph-name zapping
- Accept: a `FREEZE` `FontOp` arm on `typography/font#FONT` `FontEngineering`, driving `RemapByOTL` over a temp-path round-trip on the shared `TTFont` bytes; the typed options struct as the freeze policy; the `report` mode as a font-feature/script discovery pass
- Reject: a hand-walked GSUB `ScriptList`/`FeatureList`/`LookupList` traversal or a hand-built `cmap` rewrite where `RemapByOTL` owns the fold; a loose option `dict` or a parallel rename helper where the typed options struct + `renameFont` suffice; a uharfbuzz reshape or a fontTools `Subsetter` pass mistaken for a freeze — shaping is per-run and reversible, subsetting prunes, freeze permanently rebinds `cmap`; trusting a silent return instead of reading `engine.success`
