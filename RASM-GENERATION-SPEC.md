# [RASM_GENERATION_SPEC] — future orchestration lib (transient seed)

A transient seed for `Rasm.Generation`, the future app-level layout/generation/assembly orchestration
lib. NOT built this campaign. Assumes the rebuilt Materials (`Component`), Element (the unified
type/occurrence seam), and Bim already landed per `RASM-REBUILD-SCOPE.md`. This seed exists only to
register the planned strata member and record the generative contract the rebuild must satisfy; the
permanent fixture is the registration in `libs/.planning/{architecture.md, planning-targets.md}` + a
branch IDEAS card. Delete this seed once the lib's `.planning` folder is stood up.

## [1]-[WHY_IT_EXISTS]

The rebuild captures COMPLETE generative parametric data per `Component` (`RASM-REBUILD-SCOPE.md`
`[2]`) so that "build the real geometry" is a downstream concern, not a Materials concern. Materials
owns the DATA; Element owns the type/occurrence GRAPH; this lib owns the ORCHESTRATION that turns a
sited occurrence + its inherited Component data + Construction primitives + bond/layout into host
geometry. Without it, every app re-implements solid authoring from the captured fields. With it, an
app wires a graph and receives geometry.

## [2]-[STRATA_PLACEMENT]

APP-PLATFORM. It composes MULTIPLE AEC-DOMAIN owners — `Component` generative data (Materials),
occurrences + `Bake` inheritance (Element seam), and Construction assembly/layout/nesting primitives
(Materials Construction / Fabrication) — which an AEC-DOMAIN peer cannot do (peers never reference
peers; alignment travels only through the shared lower stratum). Composing several AEC-DOMAIN packages
plus the `Rasm` kernel is the defining shape of APP-PLATFORM. Depends UP on
`{Rasm, Rasm.Element, Rasm.Materials, Rasm.Bim, Rasm.Fabrication}`; the host-specific bake to a live
Rhino document stays at HOST-BOUNDARY (`Rasm.Rhino`/`Rasm.Grasshopper`), so this lib produces
host-neutral kernel geometry, not plugin-bound output.

## [3]-[GENERATIVE_CONTRACT_IT_CONSUMES]

The rebuild must leave these consumable without this lib re-learning provider surfaces:
- **`Component` complete parametric data** — the per-family field sets of `RASM-REBUILD-SCOPE.md`
  `[2]` (CMU cell geometry, brick frog/core, IGU pane/spacer/optics, fastener thread/head/nut/washer,
  rebar rib + bend schedule, headed stud, weld bead/groove, steel/timber section). A
  parametric-solid authoring surface reads these fields and emits kernel geometry — GeometryGym is the
  IFC schema mirror only, so the solid is authored from the captured data, never from a provider
  parametric-solid.
- **Element `Bake` + `TypeId`** — a baked occurrence carries placement + overrides and a `TypeId` back
  to its `Component`; this lib recovers the Component generative data through that reference and
  applies the named type→occurrence inheritance precedence already resolved by `Bake`.
- **Construction primitives** — assembly composition, layout, and nesting (kept + hardened in the
  rebuild) supply the arrangement algebra (course/bond/coursing for masonry, spacing/edge-distance for
  fasteners, lap/development for rebar, mullion/transom grids for glazing) this lib orchestrates over
  many occurrences.
- **Neutral detail schema** — the realizing-detail bag (`RASM-REBUILD-SCOPE.md` `[1].5`) carries the
  connection/realization detail this lib reads to place hardware between primary members.

## [4]-[SURFACES_IT_OWNS] (signature-level, not built)

- **Generation pipeline** — one polymorphic entry that discriminates on occurrence kind and dispatches
  to the parametric-solid author, returning kernel geometry + a typed generation receipt (route /
  source fields / solver evidence).
- **Parametric-solid authoring** — per-family solid construction folded from the captured `Component`
  fields (the only place the captured geometry data becomes a mesh/brep).
- **Layout / bond resolvers** — Construction primitives lifted into placement transforms over an
  occurrence set (bond patterns, fastener grids, rebar schedules, glazing grids).
- **Assembly orchestration** — composes primary members + minor parts + realizing details into a
  coherent assembly, honoring the type/occurrence inheritance.

## [5]-[REGISTRATION] (the permanent fixtures this seed leaves behind)

- `libs/.planning/architecture.md` — `Rasm.Generation` added to the APP-PLATFORM stratum
  `Folder(s):` line with its dependency rule, and reflected in `[02]-[DEPENDENCY_DIRECTION]`.
- `libs/.planning/planning-targets.md` — `libs/csharp/Rasm.Generation` inserted into the `[CSHARP]`
  `Planning Folders:` line (alphabetical).
- `libs/csharp/.planning/IDEAS.md` — a branch C# IDEAS card (the cross-libs `libs/.planning/IDEAS.md`
  is cross-language-only; this lib is C#-only, so it homes in the branch pool). The card states the
  thesis (a Component's complete generative data + Construction + bond/layout orchestrated into host
  geometry), the unlocked capability (apps wire a graph and receive geometry), and the anchors
  (`Component` data, Element `Bake`/`TypeId`, Construction, the neutral detail schema).
