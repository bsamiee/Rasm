# PANEL 1 — BRIEF-EXECUTION FIT — `rebuild.js` vs RASM-CS-GEOMETRY leg 4

Lens: can the CURRENT reconcile-free engine execute DECISION `[10]` row 4 as written. Verdict up front: **NO — not as a clean single invocation.** The 12-page author/rebuild body runs; the *riders* (GShark manifest+catalog deletion, `FAB`/`AppHost` counterpart edits, `ARCH:[02]` fan wiring, the four `[05]` acceptance dry-runs) have **no owning stage** and reach agents only through prose. Reconcile removal orphaned exactly the class of work leg 4 is dense in. Fixes below are engine-generic and brief-gated (no no-brief narrowing).

Line anchors: `rebuild.js` = `RB:<line>`; DECISION section = `[NN]`; disk verified this session.

---

## F0 — ROOT CAUSE: the DECISION was authored against a reconcile-bearing engine; reconcile is gone

DECISION `[10]` line 262 (verbatim): *"Each leg closes its own residuals in-run (**the engine's terminal reconcile carries whole-repository write authority**; no post-leg residual pass exists)."* The engine no longer has that stage — `meta.description` (`RB:4`): *"NO reconcile stage: every agent repairs the cross-file ripples its own work exposes IN ITS OWN PASS."* Composition ends at the review pool (`RB:724-728`); there is no terminal whole-corpus agent. The ledger leg-3 row already flags "RECONCILE-FREE engine … no residual machinery exists" (ledger:18), yet the DECISION prose still parks obligations on the removed stage.

Consequence: every leg-4 rider the DECISION implicitly assigned to "the terminal reconcile / whole-repository write authority" is now **unhomed** — manifest motion, cross-package `.planning/` ripple, `ARCH`/`README` fan wiring, and acceptance dry-runs. F1-F5 are all instances of this single root cause. **Generic weakness:** any demanding brief that stages whole-repo/cross-corpus motion "into the reconcile" now has no executor. The engine deleted its only cross-page-visible, whole-repo-write stage without replacing the *scheduling of brief-declared out-of-corpus motions* that used to land there.

---

## F1 — RIDER DELIVERY is prose-only and section-gated; cross-cutting-section riders may never reach an agent

Delivery channel, exhaustively: riders arrive only through two prose lines — `LAW` (`RB:455-459`, "READ {BRIEF} FIRST … the SHARED-LAW HEAD plus every section covering the targeted folder(s)") and `READ_MANDATE` (`RB:548-549`, "the campaign brief … the shared-law head + every section covering these folders"). **There is no structured rider input.** `PLAN_SCHEMA` (`RB:42-45`) carries only `packages` / `pages` / `deletePages`; nothing else crosses from brief to agent except what an agent chooses to read.

The leg-4 riders live in **cross-cutting sections that are not "a section covering the Parametric/Meshing/Drawing folder"**:
- GShark deletion → `[09]-[ROSTER_AND_API]` (roster table, keyed "Leg 4") + `[03]` line 113. A Parametric agent applying "every section covering these folders" has no folder reason to open `[09]`.
- `FAB:22/23/33/46/47/48`, `AppHost` verify, `Compute`/`Persistence` edits → `[04]-[SEAM_LEDGER]` counterpart table. Same problem: `[04]` is a ledger section, not folder-scoped.

Partial mitigation exists — each rider is *also* echoed on its owning page row's Seams cell (row 20 seam names `api-gshark.md` DELETE; row 27 seam names `FAB:33/46/47`; row 28 seam names AppHost signature-lock). So an agent reading its own page row catches the pointer. But the **exact anchors live only in the cross-cutting section**, and they diverge from the row (F8), so the page-row echo is a lossy copy.

**Proposal (parameterized):** add an optional `riders` arg parsed alongside `brief`/`targets` (`RB:33`), shaped as typed rows `{motion: 'manifest-drop'|'manifest-add'|'catalog-delete'|'counterpart-edit'|'verify', target, anchor, detail, page?}`. The launcher transcribes the brief's `[04]`/`[09]`/`[10]` rider cells into this array once; the engine routes each row to the stage that owns it (F3/F4/F5). Riders stop depending on an agent's section-reading discipline. **No-brief effect:** `riders` defaults `[]` (mirrors `BRIEF=''` and empty `deletePages`); the routing stages no-op. Zero narrowing.

---

## F2 — OUT-OF-CORPUS MOTION AUTHORITY: no stage owns the GShark manifest deletion

The GShark vendor-and-own ruling (R1, `[00]`) requires, staged to **wave 4** (`[09]` GShark VENDOR row; `[10]` row 4 rider): drop `GShark` from `Directory.Packages.props` (disk-confirmed `PROPS:71`) and from `libs/csharp/Rasm/Rasm.csproj` (disk-confirmed `:21`). These are **manifest files — not `.planning/` pages, not `.api/` catalogs**. Per CLAUDE.md `[04]`/`[07]`, manifest motion is a deliberate centralized action applied by hand-editing the grouped props.

The engine models exactly **one** class of non-page motion: `deletePages` → the `delete:brief` executor (`RB:711-715`, `deletePrompt` `RB:624-630`). That executor is **page-only** and requires a `capturedIn` content destination (`PLAN_SCHEMA.deletePages` requires `capturedIn`, `RB:45`) — a package pin has no `capturedIn`. So manifest motion cannot enter through `deletePages`. Its only remaining path is the implement ripple clause (`RB:620-622`, "the consumer site … wherever the fix genuinely lives"): the `nurbs.md` fable author *might* also edit two manifest files. That is fragile on three counts — (a) it is one of 3 concurrent `fable` implement agents (`RB:712-714`), (b) it fires only if the agent connects row 20 → the manifest, and (c) **nothing verifies the pin was actually dropped.** The wave-1 roster motion was executed by the *main session* pre-leg (`[09]` "EXECUTED PRE-LEG … legs VERIFY"), but the GShark drop is explicitly deferred to wave 4 and is *not* pre-executed — so there is no main-session safety net either.

**Proposal:** a dedicated post-implement, pre-critique **MOTION stage** consuming `riders` rows of kind `manifest-*`/`counterpart-edit`, run as one serialized agent (manifest edits are inherently serial — never concurrent fable authors racing `Directory.Packages.props`). It hand-edits the grouped props / csproj per the anchored row and returns a receipt (dropped/added/verified). **Temptation to resist:** hardcoding `"drop GShark at PROPS:71"` into the engine. **Parameterized form:** the engine never names GShark or a line number — it applies `{motion:'manifest-drop', target:'Directory.Packages.props', anchor:'GShark'}` supplied by the launcher from `[09]`. **No-brief effect:** no brief ⇒ no `riders` ⇒ stage no-ops. Zero narrowing.

---

## F3 — `api-gshark.md` catalog deletion is explicitly excluded from every stage

`[03]` line 113 (verbatim): *"kernel `api-gshark.md` deletes with the WAVE-4 `nurbs.md` landing … never in wave 1"* and *".api deletions ride the … roster motion, **not deletePages**."* Disk-confirmed the file exists (`libs/csharp/Rasm/.api/api-gshark.md`, 25KB). So the DECISION forbids the one channel (`deletePages`) that could delete it, and the roster motion it names has no engine stage (F2). Residual path: the `nurbs.md` author deletes it as a "consumer-site" ripple. Same fragility as F2, plus a **removal-discipline tripwire**: `RB:445-450` forbids removing an admission "ONLY where the campaign brief explicitly rules that removal" — the brief *does* rule it (R1/`[09]`), so it is sanctioned, but a cautious fable agent reading `RB:445-450` in isolation may *decline* to delete a catalog it was told to preserve-by-default and instead defer — and deferral now has no reconcile to catch it.

**Proposal:** fold catalog deletion into the F2 MOTION stage as `{motion:'catalog-delete', target, guardPage}` where `guardPage` (nurbs) must have landed first — the stage checks the vendored owner exists before deleting the vendored-package catalog (encodes the one-engine law mechanically instead of trusting an agent to sequence it). **No-brief effect:** brief-gated; no-op without `riders`.

---

## F4 — CROSS-PACKAGE counterpart edits reach into sibling packages the engine never scoped

`[04]` + `[10]` row 4 demand edits to files **outside `libs/csharp/Rasm/` entirely**: `Rasm.Fabrication/ARCHITECTURE.md` (`FAB:22/23/33/44/46/47/48` — disk-confirmed the file exists), `Rasm.AppHost/.planning/Sandbox/solver.md:54-55` (verify — disk-confirmed exists), `Rasm.Compute/ARCHITECTURE.md`, `Rasm.Persistence/…/Element/codec.md`. The engine's page set is 12 files under one package's `.planning/`; `folderOf` (`RB:541`) and the plan expansion (`RB:558-565`) never see the sibling packages. These land only via the boundaries seam clause (`RB:133-138`: "repair the counterpart mirror … surgical seam repair, wire-canonical names frozen, never a foreign-interior rebuild"). That clause is correct policy but is (a) agent-discretion-gated, (b) section-read-gated (the anchors are in `[04]`, not folder-scoped), and (c) unverified. The `AppHost` rider is worse: it is a **verify-only** action ("signature-locked … (verify)") — an author/review engine has no "assert X matches on disk, report drift" work-item type; a verify with no divergence produces no edit and thus no evidence it ran.

**Proposal:** route `counterpart-edit` and `verify` rider kinds through the F2 MOTION stage (or a read-only VERIFY sub-pass for the latter). The stage carries whole-repo path authority explicitly (the sibling-package paths are data in the rider row), applies the surgical wire edit, and for `verify` returns a drift report fail-open. **Temptation:** hardcoding the `FAB`/`AppHost` paths. **Parameterized form:** paths are rider-row `target` fields sourced from `[04]`. **No-brief effect:** brief-gated no-op.

---

## F5 — the four `[05]` acceptance dry-runs have no executor and cannot run in any per-batch stage

`[10]` row 4 closes with "the four brief `[05]` acceptance dry-runs compose"; `[08]` asserts they "compose from these owners plus landed fences; none reaches for a missing owner." In planning phase a "dry-run" = a **whole-corpus trace** confirming every owner a dry-run needs exists on disk with the right `Apply` entry signature and seam (e.g. the Fabrication unroll dry-run touches slice+skeleton+offset+develop+`[08]` rows). No engine stage does this: Plan/Discover are pre-write; Implement authors; Critique/Redteam each see **only their 4-page batch** (`RB:632-633`, `RB:653-654`) and cannot trace a cross-page, cross-folder acceptance path. There is no terminal cross-corpus reader. So the campaign's own closing acceptance criterion is unexecuted — the leg "closes" with the DECISION's central gate unverified.

**Proposal:** an optional terminal **ACCEPTANCE stage** after the review pool, gated on an `acceptance` arg = `[{name, needs: ['<page>#<entry>', '<seam-anchor>', …]}]` traces. One read-only agent per trace (pooled) confirms each `needs` resolves on disk, fail-open, returns a pass/miss ledger — never blocks, never writes. **No-brief effect:** `acceptance` defaults `[]`; stage no-ops. **Bonus (brief-agnostic):** even absent an `acceptance` list, this stage can run a generic "every cross-page symbol a landed fence reads resolves on a sibling owner" sweep — which *helps* no-brief mode catch the exact concurrent-batch seam divergence F6 introduces. Net positive for no-brief, zero narrowing.

---

## F6 — ALPHABETICAL batching severs the NURBS dependency spine across concurrent batches

`PAGES` is sorted alphabetically by full path (`RB:692`) then chunked 4-wide (`RB:710`, `IMPL_BATCH=4`). Leg-4's 12 sorted paths batch as:
- **B0**: `Drawing/pack`, `Drawing/view`, `Meshing/skeleton`, `Meshing/slice`
- **B1**: `Parametric/curve`, `Parametric/develop`, `Parametric/nurbs`, `Parametric/panelize`
- **B2**: `Parametric/patternmap`, `Parametric/subdivide`, `Parametric/surface`, `Processing/remesh`

Batches run **concurrently** (`pool`, `CAP=10`, 3 items, staggered 1.5s — `RB:531-537`, `RB:702`/`RB:712`). The DECISION's `[04]` seam spine is `nurbs ← curve·surface·develop` and `surface ← develop·panelize·patternmap`, `remesh ← panelize`. So the vendored NURBS engine (`nurbs`, B1) is authored **simultaneously with `surface` (B2)** which must compose `Nurbs.Of(NurbsWire)` + `RationalDerivatives` + fundamental forms; `develop` (B1) composes `surface` (B2); `panelize` (B1) composes `remesh` (B2). The concurrent authors cannot read each other's not-yet-written fences — they have only the Discover reading map (a pointer, `RB:547`), not the authored signatures. Result: cross-batch **seam divergence** exactly where the DECISION is most demanding (the vendored engine's public surface). The removed reconcile stage was the mechanism that used to heal this; "each agent repairs its own ripple in-pass" cannot, because at implement time the sibling fence does not exist yet.

Critique/redteam *can* catch it (the implement pool fully completes before the review pool starts — `RB:712` awaited, then `RB:724`), so `nurbs.md` exists when `surface`'s reviewer reads it. But the review batches use the **same alphabetical split** (`RB:724`), so `surface`'s redteam (B2) fixing a divergence against `nurbs` (B1) must **write `nurbs.md` while B1's own redteam is also writing it** — concurrent write on the foundation page.

**Proposal:** batch in **brief-declared dependency order**, not alphabetical. The DECISION already supplies it — `[02]`/`[10]` listed order *is* dependency order (`[10]` "listed order = dependency order"). When the brief's Plan output preserves page order (or a `pageOrder` rider is supplied), chunk in that order so a foundation page and its immediate consumers co-batch under one agent (nurbs+curve+surface+develop together). Absent order info, **fall back to folder-grouped-then-alphabetical** (sort by folder first) so same-folder siblings co-batch and cross-batch index-doc contention drops. **Temptation:** hardcoding the Parametric order. **Parameterized form:** consume the Plan's emitted page order (stop the `.sort()` at `RB:692` from destroying brief order; sort only within a folder/wave key). **No-brief effect:** no brief ⇒ no declared order ⇒ folder-grouped-then-alphabetical, which is *strictly better* than today's flat alphabetical for any multi-folder target set. Zero narrowing; a general improvement.

---

## F7 — NEW-FOLDER / MULTI-PAGE index-doc wiring has no single owner and races on shared files

Leg 4 adds 6 new pages into the (existing, disk-confirmed) `Parametric/` folder plus 3 new Meshing/Processing pages, and demands rows added to **one shared `ARCHITECTURE.md` `[02]`** (Parametric fan + slice/skeleton seam rows — `[04]` final row, row 21 seam) and **`README.md`** (`README:65` re-scope, row 27). The implement prompt tells *every* new/rebuild agent to wire "the owning-folder ARCHITECTURE.md + README.md maps" (`RB:609`) and "Keep the owning-folder index docs truthful" (`RB:619`). With B0 (slice, skeleton), B1 (curve), and B2 (surface, patternmap) all running concurrently and all instructed to edit the same `ARCHITECTURE.md`, the shared index docs take **concurrent `Edit`/`Write` from ≥3 agents** — last-writer-wins or stale-match Edit failures, with no reconcile to reconstruct the dropped rows. Two secondary facts sharpen this: (a) `ARCHITECTURE.md`/`README.md` live at `libs/csharp/Rasm/ARCHITECTURE.md` — the *package root, one level ABOVE `.planning/`* (disk-confirmed; the `.planning/` dir has none) — so an agent that looks inside `.planning/` first finds nothing and may skip wiring; (b) the "owning-folder" for a `Parametric/` page resolves via the plan-prompt rule "the path BEFORE `/.planning/`" = `libs/csharp/Rasm`, which *does* point at the right file, but only if the agent applies that rule rather than the literal folder of the page.

**Proposal:** designate index-doc wiring a **serialized terminal motion** — one agent, after the implement pool, holding all page fix-logs, writes the `ARCHITECTURE.md`/`README.md` rows for the whole leg in a single pass (the F2 MOTION stage or a sibling INDEX stage). Individual page agents stop editing the shared docs; they *report* the rows they need in their fix-log, the index agent applies them once. **No-brief effect:** brief-agnostic — concurrent index-doc writes are a hazard in any multi-page rebuild; serializing helps both modes. Zero narrowing.

---

## F8 — DECISION-internal defects the engine will faithfully surface (and one that misdirects the ripple agent)

These are brief defects, but they change leg-4 execution because the engine derives from disk while the operator eyeballs the DECISION:
1. **Kind census wrong.** `[10]` row 4 says "new ×8, rebuild ×4". Disk proves **9 new / 3 rebuild** (absent: slice, skeleton, remesh, nurbs, surface, subdivide, develop, panelize, patternmap = 9; present+rebuild: curve, view, pack = 3), consistent with `[02]` rows 17-28 and `[03]` ("new ×12 = rows 6,17-20,22-26,11-12" ⇒ 9 in wave 4). The Plan agent classifies FROM disk (`RB:568-571`) and will correctly emit 9/3 — but the operator sanity-checking against `[10]`'s "8/4" sees a mismatch and may wrongly "correct" a correct run. Fix the census in the DECISION before launch; do **not** teach the engine to trust the `[10]` count over disk.
2. **Stale csproj anchor.** Row 20 seam and `[00]` R1 cite `Rasm.csproj:18` for the GShark reference; disk is `:21` (and `[09]`/`[10]` correctly say `:21`). A ripple agent (F2) trusting row 20's `:18` edits the wrong line or misses the drop. The rider row (F1 proposal) should carry the symbol anchor `GShark`, not a line number, so line drift is irrelevant.

**Proposal:** none engine-side beyond F1's symbol-not-line anchoring; flag both to the DECISION owner pre-launch.

---

## VERDICT

**The current engine cannot execute leg 4 cleanly as a single `Workflow(rebuild.js, {targets:[12], brief})` invocation.** The 12-page author/rebuild body is well within the engine's competence: Plan correctly classifies 9 new / 3 rebuild from disk (the `Parametric/` folder and `curve`/`view`/`pack` exist; the other 9 are absent), the `new`-kind path authors + creates files, and the concurrent-batch seam divergence in the NURBS spine (F6) is recoverable at critique/redteam because the implement pool fully drains first. What the engine **cannot** do is the rider layer that leg 4 is dense in, because reconcile removal (F0) deleted the only stage that owned whole-repo/cross-corpus motion and nothing replaced the *scheduling* of it: the GShark pin (`PROPS:71`) + csproj (`:21`) drop and `api-gshark.md` deletion have no owning stage (F2/F3), the `FAB`/`AppHost`/`Compute`/`Persistence` counterpart edits reach into unscoped sibling packages on agent discretion alone (F4), the four `[05]` acceptance dry-runs have no executor at all (F5), and the shared `ARCHITECTURE.md`/`README.md` fan wiring races across ≥3 concurrent agents with no reconciler (F7). Every one of these was implicitly parked on the "terminal reconcile / whole-repository write authority" the DECISION still names at `[10]:262` and that no longer exists.

**Minimum fix set to make leg 4 executable:** (1) a `riders` structured arg (F1) + a serialized post-implement **MOTION stage** owning `manifest-*`/`catalog-delete`/`counterpart-edit`/`verify` rows (F2/F3/F4); (2) an optional terminal **ACCEPTANCE stage** gated on `acceptance` traces, read-only fail-open (F5); (3) **dependency/folder-ordered batching** replacing flat alphabetical (F6) and **serialized index-doc wiring** (F7). All four are brief-gated or brief-agnostic improvements — each no-ops or degrades gracefully to today's behavior when no brief/riders are supplied, so none narrows the engine to brief-mode; F6 and the generic-acceptance sweep are net positives for the no-brief modality. With those, leg 4 is a clean single invocation; without them it requires the launching session to hand-execute the manifest/catalog/counterpart/acceptance riders around the engine — which is exactly the manual out-of-band work the durable engine exists to eliminate.
