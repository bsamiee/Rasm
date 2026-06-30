# [RASM_MATERIALS_BIM_ELEMENT_REBUILD_SCOPE]

The single source every rebuild workflow prompt points to. A workflow prompt names the relevant
section here and does NOT restate scope. All work is `.planning/*.md` design-doc work: code fences ARE
the product (transcription-complete, decompile-verified, implementation-ready). Edits to manifests,
`tools/`, `tests/`, and `.api/` outside `libs/` are real.

SUPERSEDES `ELEMENT-REBUILD-PLAN.md` (now archived). That plan's standing-up of `Rasm.Element`,
the property-graph element, and the package admissions are LANDED; its live WATCH residuals are
captured verbatim in `[3]` below. Read `[3]` instead of the archived plan.

## [1]-[UNIFIED_MATERIAL_COMPONENT_ELEMENT_PARADIGM]

Three layers (IFC 4.3 + Revit agree; this rebuild makes them explicit and singular):
- **Material** = pure substance — homogeneous, no geometry/placement (`IfcMaterial` + properties /
  Revit Material). The seam `Material` node + `MaterialPropertySet`. Materials owns the substance
  catalog SOURCE.
- **Component** = the standardized, placement-free TYPE (`IfcElementType`/`IfcTypeProduct`, Revit
  Family Type) — marries a parametric cross-section/profile to a material and owns the COMPLETE
  parametric data + shared properties + predefined kind + classification. The cross-section is a
  FIELD of the Component, never a peer.
- **Element** = the sited occurrence — placement + value overrides only; inherits everything
  standardized from its Component. The seam `Bake`'d `Object` node.

Singular mechanism — one regime, one owner, one projection, one place. No dual paradigm, no
soft-aligning, no coupling. The Element seam is REBUILT where needed, fully `docs/stacks/csharp/`
conformant:

1. **One `Object` node, one rooted identity regime, `ObjectKind ∈ {Type, Occurrence}`.** No
   content-keyed-vs-rooted bifurcation. A Type Object's rooted `NodeId` is DETERMINISTICALLY derived
   from the Component's canonical content — a principled `NodeId` derivation added to the one identity
   owner (`Object.ToCanonicalBytes` excludes the volatile `Representations` for the Type seed, so a
   later geometry attach never re-keys the type), so identical Components dedup to one Type and IFC
   round-trip is stable. An Occurrence's `NodeId` is its unique placement identity. One regime, two
   seedings — never a second identity owner. (`ObjectKind.Type` lives at `Graph/element.md`
   `[02]-[NODE_MODEL]`, statics `Occurrence`/`Type`; reuse it.)
2. **Owner-mints-its-identity projection law** — the seam change replacing the fragile "minter stamps
   the aspect projector's egress" convention. The OWNER of a concept mints its `Object`. Materials
   owns Types, so the ONE Component projection mints the deterministic-rooted Type Object AND stamps
   its `Classification`/`PredefinedType` (neutral token; validity stays the Bim `AdmitPredefined`
   egress gate) AND lowers its complete parametric data AND binds occurrences via
   `Assign.TypeDefinition` — all in ONE projection. Model authors mint Occurrence Objects. Bim
   ingests `IfcElementType`→the SAME Type Object and `IfcElement`→Occurrence — one type
   representation, authored and ingested unified.
3. **One Component owner, one Component projection.** `Component` collapses `Profiles` + `Connection`
   into ONE polymorphic owner over a primary/minor discriminant: primary space-bounding members
   (`IfcBuiltElement`: beam/wall/plate — geometry-on-type, one occurrence = one piece) vs minor
   standardized parts (`IfcElementComponent`: bolt/rebar/stud/brick/CMU/IGU — one type, MANY pieces).
   Cross-section is a Component FIELD (the `ParametricSection` section machinery + the
   `ComputedSection` capacity rail become Component-internal). `MaterialProjector` +
   `ConnectionProjector` collapse into ONE Component projection whose `Project` fold discriminates
   primary/minor + type/occurrence. Construction (kept + hardened) consumes `Component`.
4. **One named Bake type→occurrence inheritance.** A NAMED inheritance dimension — NOT an
   `InheritanceMode` extension (that is `PropertyBag`-only, at `Properties/property.md`
   `[03]-[PROPERTY_BAG]`) — with explicit per-field precedence: single fields =
   occurrence-overrides-type; `Seq` fields (materials/assessments/classifications) = union +
   dedup-by-key. Surface a `TypeId` reference on the baked `Element` so the generator recovers which
   Component a piece inherits. Rework occurrence-direct `SectionOf`/`CompositionOf`/`MaterialsOf` to
   resolve through the Component (or add type-resolved accessors) WITHOUT silently breaking Compute's
   Op-free `graph.SectionOf(member)`.
5. **One seam-declared NEUTRAL detail schema.** The realizing-detail bag + the material-property→Pset
   column shape become ONE schema declared in Element over `PropertyBag` + the canonical
   `PropertyName` vocabulary (NEUTRAL — Element carries no `Pset_*`/`Rasm_ConnectionRealization` IFC
   names). Both the Component projection (author) and Bim (egress/reader) target it. IFC names + the
   egress mapping + GlobalId assignment stay in Bim's `SemanticProjector`; any cross-peer invariant is
   a Bim-implemented `IGraphConstraint`, never IFC columns in the seam.
6. **Name collisions resolved.** Disambiguate the `MaterialBinding` collision — Element
   `Graph/element.md` `[03]` `MaterialBinding` vs Materials `Projection/material.md` `MaterialBinding`
   vs the new type-binding — into `BakedMaterial` (Element baked accessor) / `MaterialBinding`
   (Materials projection) / `TypeBinding` (new type→occurrence bind).

## [2]-[COMPLETE_GENERATIVE_DATA_PER_COMPONENT]

A generator must build real geometry from captured data — no scalar smears, no placeholders. Cited
standards ground each field; the fence carries the rows transcription-complete.

- **CMU** (ASTM C90/NCMA TEK; EN 771-3): per-cell positions/shapes (replace uniform-cell inference),
  end-web vs cross-web thickness split, cell taper/draft + face-shell flare, per-cell grout boolean +
  rebar-cell position (replace the homogeneous `GroutFraction` smear), special units
  (bond-beam/knockout/channel/lintel/sash/control-joint/open-end), finish geometry
  (split depth/score count+spacing/rib), multi-cell R, 3D head joint. FIX: fire = power-law not linear
  cap; grout density not hardcoded.
- **Brick/masonry** (ASTM C62/C216/C652; EN 771-1; BIA TN): frog geometry
  (depth/taper/single-vs-double/pocket), core/perforation geometry (hole count/positions/diameter —
  replace scalar void-fraction bands), per-unit packing transforms (basketweave/pinwheel/diaper),
  cut-plane position/orientation, voussoir taper, actual-vs-work size + tolerance, mortar 3D recess
  profile.
- **Glazing IGU** (EN 1279; ASTM E2190; EN 12543): pane face dimensions (replace the hardcoded 1200mm
  placeholder), spacer geometry (width/depth/edge-seal sealant/desiccant/corner-keys), interlayer type
  (PVB/SGP/EVA) + multi-ply, gas fill % / mixture, spectral optics (per-pane
  reflectance/absorptance), frame/muntin/grid, VIG. FIX: flat GWP/specific-heat/fire(Ei30) hardcodes;
  NaN-sentinel emissivity → `Option`.
- **Fastener** (ISO 261/724/4014/4017/4032/7089/898-1; ASTM F3125): thread form (α=60°, P/d2/d3/D1,
  class 6g/6H), head (`s` across-flats, `e`, `k` height, `b`/`u` thread length + run-out), nut
  (`s`/`m`), washer (`d1`/`d2`/`t` + HV), shank-vs-thread length. Bolt+nut+washer as a complete
  assembly.
- **Rebar** (ASTM A615/A706; ISO 6935-2; EN 10080; ACI 318 §25.3; EN 1992 §8.3): rib geometry
  (rib height `h`, spacing `c`, longitudinal-rib height, flank angle, relative-rib-area `fR` — the
  bond parameter), full bend/hook schedule (min inside bend dia, hook type/extension, stirrup/tie
  bends) + BS 8666 shape codes.
- **Headed stud** (ISO 13918 Type SD; AWS D1.1; AISC 360 §I8; EN 1994): shank dia `d1`, head dia `d5`
  + thickness `h3`, underhead neck `h4`, length-before-weld `l1` + weld burn-off → as-welded length
  (both exposed).
- **Weld/joint** (AWS D1.1; AISC 360): weld bead profile (face concave/convex/flat, reinforcement,
  toe/root), groove prep (root opening/face, bevel V/U/J angle, backing bar), plug-weld hole geometry,
  adhesive fillet/bite.
- **Steel sections**: complete via VividOrange typed-designation classes + `SteelStiffness` for the
  `Wpl`/`It`/`Iw`/`Av` gap; timber via hand-rolled EN/APA grades. The section capacity rail covers
  steel/RC/timber/masonry (glazing + connection capacity cases are noted as future `[Union]` growth,
  not this campaign).

## [3]-[CAPTURED_WATCH_RESIDUALS] (from the superseded plan)

Resolve only those TRIGGERED by the rebuild; the rest stay logged here, not re-investigated.

- **H8** — per-node `IntroducedIn`/`RemovedIn` schema-span stamped by Bim at ingress; validate
  `code`+`predefined` against `Header.ReleaseVersion` at `Emit`; sniff `FILE_SCHEMA`/JSON
  `schema_identifier` before constructing `DatabaseIfc` (never hardcode 4x3). RESOLVE only if the
  rebuild touches the Bim ingress/emit site.
- **M4** (WATCH) — `ElementGraph`→`Ara3D.BimOpenSchema` egress: if BimOpenSchema is EAV-generic,
  Persistence owns a structural map; if BIM-typed, it is a Bim-implemented seam projection. Surface as
  a Marten FLAT-TABLE projection (co-transactional), not daemon-lagged. RESOLVE only if Type-node
  persistence forces the egress-owner decision.
- **M6** (WATCH) — no schedule/task/sequence node for 4D; add explicit schedule/task nodes only if 4D
  is a real target (schedule/cost already in Bim via MPXJ). DEFER unless a real target needs it.
- **M7** (WATCH) — settle `ProfileRef`→section-property (VividOrange) as a ONE-HOP resolution so
  structural consumers do not re-resolve per call. This resolution spans Bim/Compute/python/ts wire;
  `ProfileRef`/`ProfileSet`/`ComputedSection` STAY seam-canonical (see `[5]`).
- **C4** (smoke-only) — `NREL.OpenStudio.macOS-arm64` 3.11.0 restores; KEEP in-process. Smoke-check
  the `OpenStudioCSharp` dylib P/Invoke-loads on net10/arm64 at admission. DEFER unless a real target
  needs it.
- **L2** (do-NOT-re-add) — DuckDB spatial/vss "unused" is NOT a defect: spatial→PG GiST, ANN→pgvector
  is valid; DuckDB as a columnar aggregator is fine. Never re-surface this as a finding.

## [4]-[CATALOG_LAW]

All three at once, per catalog: schema + polymorphic loader rail (parameterization) + DENSE IN-FENCE
authoritative seed tables (the fence carries the rows, transcription-complete) + maximal external-lib
leverage.

- **VividOrange owns** steel section CATALOGUES (12 nations, ~2914 typed designation classes) + EN
  grade factories + elastic/concrete section properties. Compose the typed-designation classes; do not
  re-tabulate steel sections by hand.
- **Hand-roll in-fence** everything VividOrange does not own: the `Wpl`/`It`/`Iw`/`Av` gap via
  `SteelStiffness`; CMU; brick; bolt+nut+washer; rebar RIB geometry; headed stud; IGU stacks; timber +
  AISI grades.
- **GeometryGym** = IFC schema mirror only — no catalogue/grade/parametric-solid. The solid is
  hand-authored from the captured data.
- No new NuGet package is required (generative data is hand-rolled in-fence). Package work is the
  LanguageExt.Core `.api` gap + the `libs/csharp/.api/` README roster + the
  protobuf-net/MessagePack/Nerdbank.MessagePack redundancy.

## [5]-[SEMANTIC_RENAME_LAW_AND_BOUNDARIES]

- Collapse `Profiles/*` + `Connection/*` into ONE `Component/` sub-domain. Key renames (the full
  ~428-anchor verbatim old→new symbol/file/anchor map is WF-1's DECIDE output): `hanger`→`connector`;
  `JointSection`/`NominalMm` disambiguation; cmu `WebThicknessMm`→`CrossWebMm`/`EndWebMm`;
  `GroutFraction`→`GroutedCellFraction`; resolve the `Coring` cross-family leak; the `MaterialBinding`
  trio of `[1].6`.
- **`ProfileRef`/`ProfileSet`/`ComputedSection` STAY seam-canonical** (the
  `MaterialComposition.ProfileSet(ProfileRef)` case + the M7 resolution spanning
  Bim/Compute/python/ts). `Component` composes them unchanged; the semantic-rename STOPS at the
  Materials folder boundary.
- **Appearance (9 pages) is OUT OF SCOPE for signatures.** `Profile`/`ConnectionItem` carry a stable
  `MaterialId AppearanceId`; reconcile is at the owner-agnostic `AppearanceKey`. Its 5 cross-page
  anchors into the collapsing pages get ANCHOR-ONLY fixups. `AppearanceKey`/`AppearanceSummary.Of`
  (tolerance 0.0, full IEEE precision) is a FROZEN invariant the rebuild must not perturb.
- **Bim renames** (WF-2): `Model/structure.md`→`spatial.md` (breaks the `structure`/`structural`
  homonym); split `Projection/semantic.md` (984L) into `semantic`/`relations`/`egress`.
- **Fabrication seam rows** (WF-3): `Construction/nesting`→Fabrication/Nesting; `Properties`→
  Fabrication/Process.

## [6]-[DUAL_AXIS_REVIEW_MANDATE]

Every page is read on BOTH axes. A page finalizes only when a cold read against both surfaces nothing.

- **CODE doctrine** — `docs/stacks/csharp/` every page by name: `README.md`, `language.md`,
  `shapes.md`, `surfaces-and-dispatch.md`, `rails-and-effects.md`, `boundaries.md`, `algorithms.md`,
  `system-apis.md`; plus `docs/stacks/csharp/domain/` (`README.md` router +
  runtime/concurrency/diagnostics/validation/resilience/transport/persistence/durability/postgres/
  data-interchange/compute/visuals/interaction). Strategic emphasis for this rebuild:
  `data-interchange`, `diagnostics`, `interaction`, `transport`, `validation`.
- **DOC-CRAFT** — `libs/.planning/README.md` (`[PLANNING_STANDARD]`: doc-set per tier, the four
  index-doc templates, the design-page grammar, the `page#CLUSTER` integration-point notation, the
  `[06]` cold-grade REVIEW gate, the `[07]` `.api` tiering) + `libs/.planning/campaign-method.md`; the
  THREE `docs/standards/` form standards `information-structure.md`, `formatting.md`,
  `style-guide.md`; the `docs/standards/proof.md` claim discipline.
- **Banned hedges** (word-boundary, page-wide; from `libs/.planning/README.md` `[05]`): should, could,
  would, might, maybe, perhaps, likely, probably, propose, consider, recommended, ideally, TBD, TODO,
  FIXME, we, our, you, and the synonym forms — is expected to, can be, aims to, is designed to, in the
  future, eventually, as needed, if necessary. Future tense is legal only on a card growth line and a
  RESEARCH item.
- **Zero-provenance** (owned by `docs/standards/style-guide.md`): no reader address, narration,
  process, source provenance, source-mining history, freshness disclaimers, checklist tails — and on a
  design page no links, URLs, versions, dates, or session context.
- **The README cold-grade is the doc-finalization gate.**

## [7]-[TWO_TIER_API_MANDATE]

Every page cites the `.api/` catalogs its concept composes — and ONLY those, never noise.

- **Substrate tier** `libs/csharp/.api/` — the universally-used external sources (the cross-cutting
  substrate catalogue); used whenever any folder composes a shared dependency. Treated equally with
  the folder tier.
- **Folder tier** `libs/csharp/<package>/.api/` — domain catalogues + folder-specific overlays
  (Materials' tier lives at the package root `Rasm.Materials/.api/`, 19 catalogs incl. the
  VividOrange family).
- Members are verified-local truth via `uv run python -m tools.assay api` (decompile/reflection over
  the restored assembly). Live NuGet feed intelligence routes through the nuget MCP; `assay api` wins
  on conflict for "does this member exist".

## [8]-[MANIFEST_RULE]

- Apply a package version change by HAND-EDITING the grouped `Directory.Packages.props` — NEVER
  `dotnet add`. Confirm the NEWEST stable via the nuget MCP (`get_latest_package_version` /
  `get_package_context`); confirm the graph with `dotnet restore` and `dotnet nuget why`.
- `.md` legs have no executable gate. Only manifest edits gate (on `dotnet restore`/`dotnet nuget
  why`). Keep `Directory.Packages.props` label-grouped by owner, sorted within clusters, one-line
  maintenance comments.
