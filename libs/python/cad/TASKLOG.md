# [PY_CAD_TASKLOG]

`python/cad` tasks track landing-grain work across the exchange, B-rep, metrology, tessellation, and service sub-domains.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
- Atomic: <present only on a minor-scope task; names the small unit so a later session sizes its turn>.
Capability, Shape, Unlocks, and Anchors are required on every open card, Atomic included; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Tasks state landing-grain work decomposing an idea.
-->

[OCP_CATALOG_ROWS]-[ACTIVE]: every OCCT member a landed fence transcribes resolves to a catalogue row, so no page carries an unverified spelling.
- Capability: fence spellings become catalogue-backed rather than author-recalled, closing the gap where a page transcribes a member the catalogue never rostered.
- Shape: `libs/python/cad/.api/cadquery-ocp.md` entrypoint tables, one row per member, each verified against the installed distribution first.
- Unlocks: `IDEAS.md [DURABLE_SELECTION]` — the correspondence work cannot leave research state while the history members it reads carry no rows.
- Anchors: `docs/laws/topology.md` `[CATALOG_MEMBER]`, ruling that a verifying member lands its row in the same pass and that claimed absence proves by a failed live resolve.
- Route: import each member from the workspace interpreter, read its real signature, then seat the row under the scope that owns it.
- Atomic: one member roster across the sewing, fillet-diagnostic, topology-map, and Boolean-history families.

[LANE_CUSTODY_OWNER]-[QUEUED]: one owner decides native parallelism and both consumers read that decision rather than each spelling a constant.
- Capability: whole-lane custody becomes a value the lane mints and hands down, so no page asserts a right to the machine's cores on its own.
- Shape: `libs/python/cad/.planning/service/lane.md` mints the custody value; `brep/boolean.md` and `tessellation/mesh.md` take it as an argument instead of a module constant.
- Unlocks: honest concurrency under the one-slot lane, where two pages each hardcode a parallel flag only the lane can justify.
- Anchors: the catalogued law that the mesher's parallel control is a boolean rather than a thread count, so a bounded caller claims whole-lane custody before enabling it; the single-slot capacity limiter the lane already owns.
- Tension: OCCT's own internal parallelism is what the one-slot lane exists to permit, so the custody value must express a grant the lane makes rather than a preference a kernel page holds.

[PARTIAL_PRIMITIVES]-[QUEUED]: angle-bounded primitives reach the wire, so a caller asks for the wedge the kernel already builds.
- Capability: primitive construction spans the kernel's full parameter space rather than the subset the first request shape happened to carry.
- Shape: `cad/v1` primitive operation messages grow their angular bounds, and `libs/python/cad/.planning/brep/solid.md` widens its roster rows to admit them.
- Unlocks: `IDEAS.md [EXCHANGE_SYMMETRY]` — modeling reaches parity with the kernel, removing a class of shapes a caller builds by Boolean subtraction instead.
- Anchors: the catalogue's record that the sphere, cylinder, cone, and torus constructors take full or partial angular extents, against a wire that carries radius and height alone.
- Atomic: one bounded field set per primitive message with its matching roster rows.

[FACE_SELECTION]-[QUEUED]: faces are selectable the way edges already are, so face-scoped operations stop being unspellable.
- Capability: sub-topology selection generalizes across topology kinds instead of privileging the one kind the first feature needed.
- Shape: `cad/v1` grows the selection message beside the edge one, and `libs/python/cad/.planning/brep/feature.md` widens its selection owner to discriminate on kind.
- Unlocks: shelling, face-scoped offsets, and draft, none of which can name their target; the thickness arm thickens a profile rather than hollowing a solid.
- Anchors: the existing edge selection with its zero-based wire and one-based kernel-map regime, which the face case reproduces exactly over a different topology map.
- Ripple: precedes `IDEAS.md [EXACT_HEALING]` — repair operations select faces before they repair them.

[GLTF_METADATA]-[QUEUED]: emitted glTF carries the identity its STEP counterpart already carries, or the empty map retires with its loss stated.
- Capability: artifact identity holds across both emitted formats rather than stopping at the exact one, so a tessellated body is as reproducible as a sealed one.
- Shape: `libs/python/cad/.planning/tessellation/emission.md` fills the file-info map it passes empty, reading the canonical values `exchange/identity` already owns.
- Unlocks: `IDEAS.md [PART_IDENTITY]` — per-part identity needs a metadata channel on the emitted container before it can carry anything.
- Anchors: the empty string map the writer takes as a required positional argument; the canonical header policy the exchange band already applies to STEP.
- Route: write a file with a populated map, read the container back, and confirm whether the values survive into the asset record before designing the policy.
- Tension: per-run metadata reintroduces the byte instability STEP canonicalization exists to remove, so every written field stays canonical or absent.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[RAIL_REBUILD]-[COMPLETE]: every owner across the five sub-domains returns `CadRail` and the seventy-nine raise statements are gone, with a raise surviving only at the two seams that name themselves — the worker pickle crossing and the serve edge's terminal Connect collapse.
[SPLIT_ENGINE_ADMITTED]-[COMPLETE]: `networkx` landed as direct closure after the per-body closure verdict was proved dead — `Trimesh.split` dispatches to a graph engine and neither was installed, so the census raised `ImportError` inside a worker no caller observes; the split now runs and is proved on a two-body scene.
[HEADER_CANONICALIZATION]-[COMPLETE]: STEP identity canonicalization landed across the FILE_NAME slots after a live write proved the indexed setters take only once `Transfer` has sized the aggregate, and proved the preprocessor slot carries the OCCT version, so an unpinned slot moves emitted bytes on every kernel upgrade.
[DEAD_COMPOUND]-[DROPPED]: `_compound` retired rather than rehomed — no operation field asks for a heterogeneous assembly, every arm collapses to one body before sealing, and the fuse that merges abutting regions is the correct many-into-one where a compound leaves coincident faces and a false census.
