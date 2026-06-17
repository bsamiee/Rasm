# [MATERIALS_TASKLOG]

Open work owned by this folder across the three sub-domains; closed items do not appear. `[STATUS]` is one of `QUEUED`, `ACTIVE`, `BLOCKED`, `SPIKE`; owner state is read at `ARCHITECTURE.md` `[OWNER_REGISTRY]`. Every `SPIKE` row names the probe that flips its owner registry cell to `FINALIZED`.

## [1]-[PROFILES]

The polymorphic profile owner, the masonry family realized, and the un-designed families queued as the growth axis.

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | Transcribe the `Profile`/`ProfileFamily` owner and the masonry vocabulary (`Coring`/`BondName`/`Orientation`/`Cut`/`ClosureRule`/`SpecialShape`) per `ARCHITECTURE.md` `[SOURCE_TREE]`; re-home the archived masonry catalogue (us/uk/din/au/is rows) as masonry-family `Profile` rows through `ProfileCatalogue.BuildMasonryRows`, re-expressing `Dim3`/`VerticalCoursing`/`AspectRatio` as kernel-`Dimension` `ProfileUnit` columns | profile#PROFILE_FAMILY | QUEUED |
| [2] | Typed generated-bond interpreter algebra owned by `BondName` (`BondKind.Generated` rows — herringbone/basket-weave/pinwheel/diaper/quetta — fail explicitly until it lands) | profile#PROFILE_FAMILY | SPIKE |
| [3] | CMU family depth-fill: design the `cmu` `ProfileFamily` unit vocabulary (cell/face-shell columns) as the second family | profile#PROFILE_OWNER | QUEUED |
| [4] | Steel family depth-fill: design the `steel` `ProfileFamily` section-property vocabulary (depth/flange/web/fillet) | profile#PROFILE_OWNER | QUEUED |
| [5] | Timber family depth-fill: design the `timber` `ProfileFamily` lamella/grade vocabulary (sawn/glulam/CLT) | profile#PROFILE_OWNER | QUEUED |
| [6] | Glazing family depth-fill: design the `glazing` `ProfileFamily` pane/spacer/frame vocabulary (IGU profiles) | profile#PROFILE_OWNER | QUEUED |

## [2]-[APPEARANCE_ENGINE]

Cross-page and cross-package seams holding an appearance-engine owner at SPIKE until ratified, plus the engine transcription.

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | Path-trace consumption split: reconcile the canonical owner name (`BsdfModel` vs `LayeredBsdf`) and ratify `LayeredBsdf.Evaluate`/`Sample` as the AppUi `viewport-pipeline#PATH_TRACE` shading contract in the suite ledger; the `thin-film` lobe's closed-set admission resolves with it | bsdf#LAYERED_COMPOSITION | SPIKE |
| [2] | Photometric quantity rows: admit `Luminance`/`LuminousFlux`/`LuminousIntensity` as composed Compute `QuantityFamily` rows (one each, per its Growth rule); until then those three ride author-kernel raw doubles while illuminance composes the existing row | texture-photometric#PHOTOMETRIC | SPIKE |
| [3] | Engine csproj references: the package `.csproj` carries no `Rasm` project reference and no Unicolour/Thinktecture/LanguageExt packages the engine and profile fences require; the package owner adds them when the source lands | bsdf#SHADING_FRAME | QUEUED |
| [4] | White-furnace energy-conservation harness: the BSDF directional-albedo integral asserts numerically against the closed-form Kulla-Conty term; gates no fence | bsdf#LOBE_FAMILY | SPIKE |
| [5] | Transcribe the appearance engine per `ARCHITECTURE.md` `[SOURCE_TREE]` build order (`Appearance/Faults.cs` through `MaterialLibrary.cs`); each file transcribes its page clusters verbatim and seeds the remaining ~70 library rows as data | appearance-graph#MATERIAL_LIBRARY | QUEUED |

## [3]-[CONSTRUCTION]

The host-neutral element→assembly→layout model and the layout fold stages folded from the former brick-layout roadmap, generalized to any profile family.

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | Transcribe the `Element`/`Assembly`/`Layout` model and the `ConstructionLayout.Resolve` straight-run course fold per `ARCHITECTURE.md` `[SOURCE_TREE]`; transcribe the archived `Layout.cs` `Repeat`/`Place` station-stepped placement projection generalized off the `Profile.Unit` | construction#ASSEMBLY_FOLD | QUEUED |
| [2] | Opening subtraction and opening-edge cuts as station/elevation interruptions plus edge-cut requests | construction#ASSEMBLY_FOLD | QUEUED |
| [3] | Corner endpoint conditions, turn metadata, and closure modifiers | construction#ASSEMBLY_FOLD | QUEUED |
| [4] | Arch placement: source-backed profile constraints plus a station-normalized path rule (TN31 gives detailing constraints, not a complete scalar placement algorithm) | construction#ASSEMBLY_FOLD | SPIKE |
| [5] | Pier layout and closure solving beyond existing bond offsets | construction#ASSEMBLY_FOLD | QUEUED |
| [6] | Layout warnings distinguishing source-backed rules from heuristics, separate from hard layout failure | construction#ASSEMBLY_FOLD | QUEUED |
| [7] | Host materialization seam: the future APP root turns each `Placement` scalar tuple into host geometry; the construction model holds no `Rhino.Geometry` type | construction#ELEMENT_MODEL | QUEUED |
