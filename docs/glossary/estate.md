# [ESTATE_GLOSSARY]

Structural vocabulary binds the whole repo: rank, boundary, unit, and corpus-surface names mean one thing at the cross-libs core and inside a leaf page alike.

## [01]-[TIERS_AND_UNITS]

- `platform tier`: Holds the independently adoptable library estates every app, plugin, and service composes exactly as it takes an external package.
- `product tier`: Declares intent, binds host edges, and emits output over platform capability it never re-owns.
- `estate`: Bundles one language's packages into an independently adoptable whole that resolves its own graph with no peer branch present.
- `branch`: Names one language's estate together with the doc-set, manifests, toolchain, and gates that estate owns.
- `package`: Closes one bounded context — own nouns, own invariants, and a published boundary an unrelated application adopts alone.
- `bounded context`: Fences one model whose nouns and invariants hold inside it and translate at its boundary.
    - [NOT]: Evans' team-and-subsystem reading; only the package boundary fences a model here.
- `cross-cutting concern`: Names what all owners need and none owns — identity, telemetry, faults, policy — woven at definition or composition time.
    - [NOT]: Aspect weaving, which each branch doctrine owns as an attachment mechanism.
- `capability`: Names what an owner does for a consumer, stated as present-tense owned fact and never gated on consumer arrival.
- `tier`: Ranks one level of ownership breadth — cross-libs core, branch, folder, page — and the narrowest tier holding a fact owns it.
- `grain`: Fixes the resolution a fact is stated at: concept grain for an idea, landing grain for a task, member grain for a catalog.
    - [NOT]: Orleans virtual actors and warehouse fact-table grain; neither names a runtime entity here.
- `altitude`: Names one level of a layered surface a fact can sit at, from an interior body up through the published wire.
    - [NOT]: Solar elevation, which rides `solar altitude` in full and never the bare word.

## [02]-[RANK_AND_PLANE]

- `stratum`: Ranks one dependency layer inside a branch, and every edge leaving it runs strictly upward.
- `strata`: Orders a branch's whole rank set, seating shared machinery at the lowest rank every consumer reaches.
    - [NOT]: Wave and band, neither of which ranks anything, and `tier`, which ranks ownership breadth rather than dependency.
- `plane`: Names one horizontal concern band cutting across strata rank, whose members seat at a rank yet stay outside the runtime graph.
    - [NOT]: Network control and data planes, geometric `Plane` values, and texel-raster `texture plane`, each owned by its own domain.
- `type plane`: Carries the compile-time half of a declaration, where a name exists for the checker alone.
- `value plane`: Carries the runtime half of a declaration, where a name exists as a live value.
- `deploy plane`: Carries deployment-time data an infrastructure program feeds into a workload environment.

## [03]-[BOUNDARIES]

- `seam`: Declares one crossing between two owners, recorded at both endpoint folders under identical kind and direction.
    - [NOT]: Feathers' test seam, an incision cut into legacy code for substitution; published contracts carry every crossing here.
- `wire`: Fixes the byte-level shape crossing a boundary, spelled once at its codec owner and compared byte-wise at every peer.
- `asset address`: Locates each file of one content-keyed product under that product's own key, so publisher and reader join on it, deriving no path.
    - [NOT]: Per-file digest paths, which re-key each leaf and fork one product's identity across its own members.
- `contract`: Defines one shape both ends conform to independently, carrying data and obligating no build order in either direction.
- `coupling`: Binds an edit obligation between two surfaces, so touching one obligates its counterpart in the same change.
- `drift`: Measures the gap between a surface and its declared truth after one end moves alone.
- `ripple`: Names every counterpart edit one change obligates elsewhere, landed at each end in the same pass.
- `fence`: Encloses transcription-complete code inside a design page, and that block is the work product a pass measures.
    - [NOT]: Namespace and network fences; only the fenced markdown block carries this word.

## [04]-[CORPUS_SURFACES]

- `corpus`: Gathers under one root the pages one authoring standard governs, and that standard is the corpus's own.
- `manifest`: Centralizes package, version, and tool ownership for one language at one root file.
- `card`: Carries one owner's earned fields under a bracketed leader a grep filters on, and an unearned field is omitted.
    - [NOT]: UI cards and hardware cards; only the leader-and-fields record carries this word.
- `page`: Owns one eventual source file's design and nothing above or beside it.
- `router`: Keys one reader decision to one owning page per choice, so a duplicate route forks that choice.
- `doctrine`: Legislates how one language is written, graded at every fence its branch admits.
- `law`: States one resolved timeless obligation an owner carries, phrased so no rebuild satisfies it accidentally.
- `ruling`: Registers one settled decision at its narrowest owning tier, foreclosing re-litigation rather than teaching mechanism.
- `charter`: Opens a surface on the capability it owns and the boundary it holds.
- `scar`: Records regression-proven law whose only justification is a failure the estate paid for, beside the trigger re-arming it.
- `pick receipt`: Names one host boundary's own hit evidence, and the host spellings stay PLURAL — their frames and providers share zero column.
    - [NOT]: One cross-host pick receipt; Rhino and Grasshopper `PickReceipt` types stand allowlisted DISTINCT owners, never halves of a shared shape.
- `wire pass`: Names the Grasshopper wire-drawing plan producer over the kernel paint program.
    - [NOT]: Electrical and network wire passes, or a drawing loop — `wire pass` PRODUCES marks and the kernel executor draws them.
- `drag facts`: Names the Grasshopper host residue of a drag gesture beside the kernel's own drag evidence.
    - [NOT]: Kernel `DragEvidence`, which keeps the evidence name while the boundary renames to facts at its own edge.
- `paint receipt`: Tallies one canvas paint run — marks drawn against culled under a gauged span.
    - [NOT]: Plotter and print receipts; only the canvas paint tally carries this word.
- `retry`: Re-drives one failed effect under the kernel `RedrivePolicy`/`Retriability` vocabulary alone.
    - [NOT]: `Reopenable` close-state, naming lifecycle and never a re-drive, so a method shadowing its same-named field reads as retry machinery.

## [05]-[PASS_VERBS]

- `census`: Enumerates a live surface mechanically, so a rule names the enumerator rather than a transcribed roster.
    - [NOT]: Population counting; enumeration here answers a gate.
- `harvest`: Nominates one pass's generalizable lessons for adjudication at the laws corpus, and an empty harvest closes the pass cleanly.
- `mint`: Creates a value, key, row, or identity at its one owning site, so no second site produces it.
- `homing`: Decides which tier owns a fact, catalog, or registry, and relocation strips the losing surface in the same pass.
    - [NOT]: Consistent-hashing re-homing, where membership change moves keys between ring members.

## [06]-[QUALIFIED_ONLY]

Words carrying live senses no context disambiguates refuse bare use, and every site spells one qualified form.

- `lane`: Refuses bare use.
- `dispatch lane`: Routes one class of agent work through a workflow under its own model, authority, and read scope.
- `offload lane`: Keys one isolation arm a blocking or CPU-bound body runs on, each bounded by its own capacity limiter.
- `transaction lane`: Separates a serialized writing path from an unwrapped analytical read path over one engine.
- `cache lane`: Keys one cache topology a resolver resolves by lane key, so each lane reads its own second-level store.
- `signal lane`: Carries one telemetry signal's exporter and pipeline rows through egress.
- `descriptor`: Refuses bare use.
- `capability descriptor`: Rows an open consumption axis — key, supplied capability, reached isolation — filled by its supplying branch alone.
- `descriptor set`: Snapshots a proto source's compiled form beside it as the drift gate's per-source baseline.
- `frame budget`: Refuses bare use.
- `viewport frame budget`: Bounds one rendered frame's spend — time, draws, residency — so a breach names its axis and degrades the frame alone.
- `solver frame budget`: Stops a coarse solve at one frame deadline between iteration floor and ceiling, forking refinement onto a background lane.
- `time travel`: Refuses bare use, and sibling packages each declare a legitimate `TimeTravel` owner.
- `collab time travel`: Reverts a live collaborative document onto a prior intent-ledger frontier by appending inverse intents.
- `store time travel`: Reads the system of record as it stood at one commit or instant through the store's as-of session, never mutating it.
- `landing`: Refuses bare use.
- `wire landing`: Declares a decoded wire family's one branch-side shape at the codec owner, so a consumer imports it and re-derives no field.
- `landing grain`: States a fact at task resolution, naming the exact file or sub-domain a card's work lands in.
- `plane landing`: Settles an infrastructure plane's row onto its realized backend, so readers provision off the armed set.
- `slot`: Refuses bare use.
- `identity slot`: Holds the key an evidence record projects for grouping, and that key is the pre-run source key a hit test compares.
- `carrier slot`: Names one position inside a rail or carrier where exactly one value lives — failure, variance, effect, converter.
- `host slot`: Seats one host-owned member the boundary writes under that host's own convention, and the domain never reads it back.
    - [NOT]: Rhino session slots, which the `rhino-mcp` tooling names and defines at its own site.
- `layout slot`: Fixes an object's attribute table at declaration, replacing per-instance dictionary storage.
- `envelope`: Refuses bare use.
- `message envelope`: Wraps a domain fact in the attributes a transport routes on, so a consumer routes without opening the payload.
- `building envelope`: Separates a building's conditioned interior from its exterior, and its tightness bounds the air a zone leaks.
- `operating envelope`: Bounds the conditions one machine or process admits, so a demand outside it refuses rather than degrades.
- `swept envelope`: Encloses the volume a moving body occupies across its whole motion, and a clearance test runs against that solid.
- `bounding envelope`: Brackets a geometry inside the axis-aligned extent an index compares first, so an exact predicate runs on candidates alone.
- `typed envelope`: Carries an operational rail's whole outcome — value beside failure — so no sentinel rides inside a data row.
