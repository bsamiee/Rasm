# [PY_CAD_IDEAS]

`python/cad` ideas track exact-modeling and exchange capability the OCCT kernel already reaches but the wire does not yet carry.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[DURABLE_SELECTION]-[QUEUED]: a sub-shape keeps one name across the reseal that ends every operation, so feature work composes rather than racing topology order.
- Capability: identity of a sub-shape becomes a first-class value the producer mints and the consumer replays, independent of the traversal order a fresh decode happens to yield.
- Shape: `libs/python/cad/.planning/brep/provenance.md` grows the correspondence owner, `brep/feature.md` accepts it beside its ordinal selection, and `cad/v1` grows the carrier the reply returns and the request replays.
- Unlocks: chained modeling across calls — a fillet naming an edge the preceding fuse produced, which today is unspellable because `Execute` returns a resealed body whose ordinals no longer match the request that made it.
- Anchors: OCCT `BRepAlgoAPI_*.History()` with its per-shape `Generated`/`Modified`/`IsDeleted` correspondence and `SectionEdges`; the `BooleanProvenance` message collapsing that correspondence to two booleans; the proto's own warning that indices are unstable after resealing.
- Tension: a durable name is either a content-derived key, which survives a reseal but not a geometric edit, or a producer-issued token, which survives an edit but obliges the provider to carry state across calls it currently holds none of.
- Ripple: precedes `geometry` — the mesh band composes chained CAD operations only once a name survives the hop.

[PART_IDENTITY]-[QUEUED]: an emitted assembly carries per-part identity, so a CAD element census joins the way the IFC path's already does.
- Capability: element identity crosses as a value beside the count, making the emitted scene addressable rather than merely measurable.
- Shape: `libs/python/cad/.planning/exchange/assembly.md` grows the label walk, `tessellation/emission.md` carries the identity onto the emitted nodes, and `cad/v1` grows the per-part roster the tessellation reply returns.
- Unlocks: per-element metadata joins across the wire for CAD bodies, closing the asymmetry where the IFC path surfaces element identity and the CAD path surfaces one anonymous integer the peer cannot join on.
- Anchors: `XCAFDoc_ShapeTool.GetReferredShape_s`/`GetLocation_s` walking instances and locations; the XCAF name and colour channels the emitter already reads back; `Rasm.Bim`'s element-identity join at the peer end.
- Tension: OCCT returns component locations already applied through `GetShape_s`, so a label walk is a second authority over the same geometry unless it carries identity ALONE and never re-derives a placement.

[NATIVE_MESH_HANDOFF]-[QUEUED]: triangulation crosses as arrays rather than as a container a consumer must decode a second time.
- Capability: discrete geometry becomes a first-class payload of the tessellation call, so the exact kernel's own triangulation reaches a consumer without a lossy container round-trip in between.
- Shape: `libs/python/cad/.planning/tessellation/mesh.md` grows the triangulation read, and the wire grows a buffer-carrying arm beside the artifact reference the reply carries today.
- Unlocks: mesh spatial queries, repair, and quality grading over CAD bodies without re-decoding an emitted GLB, which the geometry client's own boundary forbids it from doing.
- Anchors: catalogued `BRep_Tool.Triangulation_s`, `Poly_Triangulation`, `Poly_Triangle`, and `TopLoc_Location`, whose stacking row already states they become vertex and face arrays for a mesh owner.
- Tension: a buffer on the wire competes with the reference-only receipt this package rules everywhere else, so the arm earns its seat only where the round-trip cost is proven to dominate the transfer cost.

[EXACT_HEALING]-[BLOCKED]: exact B-rep repair lands under its own typed admission rather than as an alias of an unrelated tolerance.
- Capability: repair of an imported exact body becomes an admitted operation carrying its own evidence, distinct from the tessellation deflection and the IFC precision it must never borrow.
- Shape: `libs/python/cad/.planning/brep/tolerance.md` as the numeric regime owner, with the healing admission and its receipt seated there before any operation arm consumes it.
- Unlocks: recovery of imported bodies that fail validity today, which currently refuse with no repair path because sewing is the only repair arm the wire carries.
- Anchors: the catalogue's own deferral, which rules that a healing policy requires its own typed admission contract and receipt and cannot alias tessellation deflection or IFC precision; the branch ruling that IFC and CAD precision stay source-owned.
- Arms: live proof that a candidate healing operator changes topology monotonically, since the catalogue records that forced healing changed topology and tessellation non-monotonically on real IGES input.
- Route: probe the OCCT healing operators against a corpus of failing bodies, record which change topology, then design the receipt from what varies.
- Tension: healing trades exactness for admissibility, so the receipt must state what moved rather than reporting a repaired body as though it were the admitted one.

[EXCHANGE_SYMMETRY]-[QUEUED]: every neutral format this package reads it can also write, and every format it writes it can also model from.
- Capability: exchange becomes symmetric across formats rather than STEP-complete and IGES-partial, so a caller's format choice stops silently narrowing which operations remain reachable.
- Shape: `libs/python/cad/.planning/exchange/iges.md` as the format owner, and the `cad/v1` operation oneof widened to admit the sources it can already read.
- Unlocks: modeling directly on an IGES body, which tessellates but never executes, and emission back to a format a peer toolchain requires.
- Anchors: catalogued `IGESControl_Writer` and `IGESCAFControl_Writer` beside the reader this package already composes; the absent `FILE_SCHEMA` analogue that keeps IGES protocol evidence honestly empty.
- Tension: IGES admits free-form surfaces rather than solids, so widening the operation oneof obliges every arm to state its behaviour on a body carrying no volume.

[INERTIA_EVIDENCE]-[QUEUED]: mass properties reach the receipt whole rather than stopping at volume and centroid.
- Capability: the measured shape publishes its full inertial description, so a structural consumer reads evidence instead of re-deriving it from geometry it does not hold.
- Shape: `libs/python/cad/.planning/metrology/properties.md` grows the tensor read, and `BrepKernelReceipt` grows the fields carrying it.
- Unlocks: section-property and structural analysis over exact CAD bodies at the geometry band, which obtains neither principal axes nor radii of gyration from a CAD receipt.
- Anchors: `GProp_GProps.MatrixOfInertia`, catalogued and unread, beside the `Mass` and `CentreOfMass` reads the receipt already composes.
- Ripple: mirrors `geometry` structural work — the section-property owner is the standing consumer for exactly these values.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[ONE_FAULT_FAMILY]-[COMPLETE]: landed as `.planning/faults.md` — one `CadFault` value over a `FaultRow` roster keyed on independent leg and case columns, replacing three parallel frozen-dataclass Exception families whose nested `Literal` kind sets paid a re-wrap tax at every leg boundary and reconstructed the wire ordinal again at the serve edge.
[SUBDOMAIN_STRUCTURE]-[COMPLETE]: landed as five sub-domain folders over one root spine — `exchange`, `brep`, `metrology`, `tessellation`, and `service` — replacing the flat page set that made this the only Python package with no sub-domain tier; `metrology` seating below both native owners dissolved the import edge that had the exchange page composing the B-rep page for its receipt.
