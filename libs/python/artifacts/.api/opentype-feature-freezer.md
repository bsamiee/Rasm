# [PY_ARTIFACTS_API_OPENTYPE_FEATURE_FREEZER]

`opentype_feature_freezer` owns GSUB-into-`cmap` feature freezing for the artifacts font rail: the single `RemapByOTL` engine bakes chosen GSUB single (LookupType 1) and alternate (LookupType 3) substitutions into a font's default Unicode-to-glyph map so features such as `smcp`/`onum`/`ss01` render on-by-default in non-OpenType consumers, then rewrites the `name`/CFF naming tables. It reads and writes through `fontTools.ttLib.TTFont` and owns no font model. `typography/font` `FontEngineering` drives it as the `FREEZE` `FontOp` arm — freeze rebinds `cmap` where `SUBSET` prunes glyphs.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opentype-feature-freezer`
- package: `opentype-feature-freezer` (Apache-2.0)
- module: `opentype_feature_freezer`
- owner: `artifacts`
- rail: fonts — the `typography/font` `FREEZE` `FontOp` arm
- depends: `fonttools` — the binary-font model and table objects the remap reads and writes
- entry points: console script `pyftfeatfreeze`; library use is `RemapByOTL(options).run()` with the `cli.main`/`cli.parseOptions` argv helpers

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: feature-freeze engine and its state

`RemapByOTL` is the single owner, constructed over one `options` carrier whose attribute set is the freeze policy — the CLI builds it as an argparse `Namespace`, a library caller as any attribute carrier exposing the same field names. There is no `freeze(bytes) -> bytes` entry; the contract is construct with options, call `run()`, read `options.outpath`.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                                                  |
| :-----: | :-------------------------------- | :------------ | :---------------------------------------------------------------------------- |
|  [01]   | `RemapByOTL`                      | class         | `RemapByOTL(options)`; the open->remap->rename->save pipeline                 |
|  [02]   | `RemapByOTL.options`              | attribute     | the carrier holding the freeze-policy fields (the [02] field table)           |
|  [03]   | `RemapByOTL.ttx`                  | attribute     | `Optional[TTFont]`; `None` pre-`openFont()`, closed by `closeFont()`          |
|  [04]   | `RemapByOTL.success`              | attribute     | `bool`; each stage sets it, `run()`/`remapByOTL()` short-circuit when `False` |
|  [05]   | `RemapByOTL.substitution_mapping` | attribute     | old->new glyph-name `MutableMapping[str, str]` over every `cmap` subtable     |

[PUBLIC_TYPE_SCOPE]: the options field set (the freeze policy)

These are the argparse `dest=` names `RemapByOTL` reads directly; the design-side options struct spreads exactly this set.

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
|  [12]   | `names`        | `bool`                  | collect and print the substituted glyph names during processing                         |
|  [13]   | `verbose`      | `bool`                  | raise the module logger to `INFO` (consumed by `cli.main`, not by `RemapByOTL`)         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the freeze pipeline

`run()` is the canonical call. Each stage method is public and individually callable to drive a pre-opened `TTFont` or interleave stages; each returns `None` save `renameFont() -> bool`, mutates `self`, and is not pure.

| [INDEX] | [SURFACE]              | [SHAPE]  | [CAPABILITY]                                                                  |
| :-----: | :--------------------- | :------- | :---------------------------------------------------------------------------- |
|  [01]   | `RemapByOTL(options)`  | ctor     | construct over the options carrier; derives `outpath` if unset                |
|  [02]   | `run()`                | instance | the full open->remap->rename->save pipeline, gated on `.success`              |
|  [03]   | `openFont()`           | instance | open the font into `.ttx` via `TTFont(inpath, 0, recalcBBoxes=False)`         |
|  [04]   | `remapByOTL()`         | instance | the GSUB->`cmap` fold: `initSubs()` then rows [05]-[08]                       |
|  [05]   | `filterFeatureIndex()` | instance | resolve the `FeatureIndex` set from `GSUB.ScriptList` under `script`/`lang`   |
|  [06]   | `filterLookupList()`   | instance | resolve the `LookupList` indices for `features` over the `FeatureIndex`       |
|  [07]   | `applySubstitutions()` | instance | fold LookupType 1/3/7 subs to `.substitution_mapping`; warn no-`cmap` remap   |
|  [08]   | `remapCmaps()`         | instance | rewrite every `cmap` subtable entry through `.substitution_mapping`           |
|  [09]   | `renameFont() -> bool` | instance | RIBBI/OFL `name` + CFF top-dict rename; no-op without `suffix`/`replacenames` |
|  [10]   | `saveFont()`           | instance | `report` prints enumeration; else `zapnames` then `ttx.save(outpath)`         |
|  [11]   | `closeFont()`          | instance | `ttx.close()` the live font                                                   |

[ENTRYPOINT_SCOPE]: argv / CLI helpers

An in-process design bypasses these and builds the options carrier directly; `parseOptions` documents the canonical option vocabulary.

| [INDEX] | [SURFACE]          | [SHAPE] | [CAPABILITY]                                                            |
| :-----: | :----------------- | :------ | :---------------------------------------------------------------------- |
|  [01]   | `cli.main`         | static  | `main(args=None, parser=None) -> int`: exists-check, log, `run()`       |
|  [02]   | `cli.parseOptions` | static  | `parseOptions(args=None) -> Namespace`; `-f -s -l -S -U -R -z -i -r -n` |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `RemapByOTL` is the whole library — no module functions, no `freeze(bytes) -> bytes` entry; the unit of composition is build an options carrier, `run()`, read `outpath`. State (`.ttx`/`.success`/`.substitution_mapping`) mutates across stages, so the engine is single-use per font.
- `options` carries the freeze policy (the [02] field table); `RemapByOTL` reads `options.features.split(",")`, `options.script`, `options.lang`, the rename knobs, and the `report`/`names` flags directly.
- Freeze fold — `filterFeatureIndex()` walks `GSUB.table.ScriptList.ScriptRecord` intersecting `options.script` (else all) and `options.lang` (else `DefaultLangSys`) to a `FeatureIndex` set; `filterLookupList()` maps `features` over `FeatureList.FeatureRecord[fi].FeatureTag` to a `LookupList` set; `applySubstitutions()` folds `LookupType in {1, 3, 7}` — single via `Subtable.mapping`, alternate via `alternates[g][0]`, extension recursing into `ExtSubTable` — into `.substitution_mapping`; `remapCmaps()` rewrites every `cmap` entry through it.
- Rename fold — `renameFont()` resolves the family from `name.getName(16, 3, 1) or name.getName(1, 3, 1)`, applies `replacenames` and the suffix (custom `usesuffix` or sorted feature tags), rewrites `name` records by ID — {1,4,16,18,21} family replace, ID 3 a `;featfreeze:` unique-ID stamp, ID 5 a version stamp under `info`, {6,20} the no-space PostScript replace — and mirrors `FamilyName`/`FullName`/PostScript name into the CFF top-dict, raising on multi-font CFF.
- `options.report` makes `saveFont()` print the script/language enumeration (`.reportLangSys`) and feature enumeration (`.reportFeature`) instead of writing — a discovery pass with no output font.

[STACKING]:
- `fontTools`(`.api/fonttools.md`): shares the `TTFont` binary-font model the `typography/font` `SUBSET`/`INSTANCE`/`OUTLINE`/`EMBED_AUDIT` arms operate on; a `FREEZE -> SUBSET` chain freezes `cmap` to the default-glyph set, then `Subsetter.populate(unicodes=...)` prunes to the now-default glyphs so frozen smallcaps survive. fontTools owns the model; this package owns the GSUB->`cmap` remap fontTools exposes as no one-call op.
- `msgspec`(`.api/msgspec.md`): the design-side options carrier is a frozen `Struct` spreading the [02] field names.
- `beartype`(`.api/beartype.md`): validates the feature-tag/script-tag inputs at the boundary before the native remap.
- `expression`(`.api/expression.md`): the `FREEZE` arm returns the `Result`/`RuntimeRail[ContentKey]` every `FontOp` arm rides; because the engine sets `.success` `False` on an unopenable/unsavable font instead of raising, the arm reads `engine.success` after `run()` and lifts a `False` into the rail failure rather than trusting a silent return.
- within-lib: `openFont`/`saveFont` are path-only, so the arm brackets a temp-path round-trip — write the input `font` bytes to a `NamedTemporaryFile`, point `options.inpath`/`options.outpath` at temp paths, `RemapByOTL(options).run()`, read the output back to `bytes`, `unlink` both in `finally` — exactly as `typography/shape` composes blackrenderer's path-only `saveImage`.
- `typography/shape` (uharfbuzz) applies features per-run and reversibly at shaping time while `FREEZE` bakes them permanently into `cmap`, so the two are complementary and a make-smallcaps-default request routes to `FREEZE`, never a hand-rolled `cmap` rewrite or a uharfbuzz reshape.

[LOCAL_ADMISSION]:
- Freeze step is `RemapByOTL(options).run()` over a temp-path round-trip, never a hand-walked `GSUB` traversal nor a hand-built `cmap` rewrite — the engine owns the LookupType 1/3/7 fold and the alternate first-pick.
- Freeze policy is the typed options struct spreading the [02] field names, never a loose `dict[str, object]` and never a parallel rename helper — `renameFont` owns the `name`/CFF rename and the OFL replace.
- Font model stays `fontTools.ttLib.TTFont`, so a `FREEZE -> SUBSET -> INSTANCE` chain runs on one model.
- a freeze fault reads off `engine.success` into the `RuntimeRail` failure; a `report`-mode run writes no font and yields a discovery receipt.

[RAIL_LAW]:
- Package: `opentype-feature-freezer`
- Owns: freezing a chosen GSUB single/alternate feature set into a font's default `cmap` for non-OpenType consumers; the attendant RIBBI/OFL `name`+CFF rename and `post` glyph-name zapping
- Accept: a `FREEZE` `FontOp` arm driving `RemapByOTL` over a temp-path round-trip on the shared `TTFont` bytes; the typed options struct as the freeze policy; the `report` mode as a font-feature/script discovery pass
- Reject: a hand-walked `GSUB` traversal or hand-built `cmap` rewrite where `RemapByOTL` owns the fold; a loose option `dict` or parallel rename helper where the options struct + `renameFont` suffice; a uharfbuzz reshape or a `Subsetter` pass mistaken for a freeze; trusting a silent return over reading `engine.success`
