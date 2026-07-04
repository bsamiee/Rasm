# PANEL 3 — THREADING / ARCHITECTURE dossier (`rebuild.js` redesign)

Lens: work-threading. Why a 5-page leg takes ~3h, and the threading redesign that fixes it without lowering the quality bar. All findings verified against `.claude/workflows/rebuild.js`, the workflow-creator harness reference (`api-reference.md` §5/§6/§9/§11, `patterns.md` §13/§14/§19), `libs/.planning/campaign-method.md`, `.claude/scratch/geometry-run-ledger.md`, and the live `RASM-CS-GEOMETRY-DECISION.md`.

---

## 1. WALL-CLOCK AUTOPSY

### 1.1 The current run shape (exact, from `[COMPOSITION]` lines 687-728)

```
Plan          1 sonnet/low  schema=PLAN         (thin enumerate)
 └ BARRIER-0  real: need the page list to chunk
Discover      pool(chunk(PAGES,4), CAP=10)   1 opus/max  per 4 pages   READ-ONLY
 └ BARRIER-1  await pool(...) → build global MAP_BY_PAGE (line 704)
Implement     pool(chunk(PAGES,4)+delete, CAP=10)  1 fable/max per 4 pages  WRITES
 └ BARRIER-2  await pool(...) (line 712)
Critique+Redteam  pool(chunk(PAGES,4), CAP=10)  per batch: crit(fable/xhigh) → await → redteam(fable/max)
```

For 5 pages: `chunk(PAGES,4)` = `[[4],[1]]` → 2 batches everywhere. Agent census = **Plan 1 + Discover 2 + Implement 2 + (Critique 2 → Redteam 2) = 9 giant agents**, matching the brief's `1+2+2+2+2`. Three global barriers (BARRIER-0/1/2). The critique→redteam pair is **already per-batch-pipelined inside one pool worker** — batch-1's redteam does not wait for batch-0's critique; `phase('Critique'); phase('Redteam')` are display groups only.

### 1.2 The true critical path

The longest dependency chain runs through the **heavy batch** (the 4-page chunk), one agent deep per stage, serialized by the two write-barriers:

```
Plan → Discover[b0/4pp] → ║ → Implement[b0/4pp] → ║ → Critique[b0/4pp] → Redteam[b0/4pp]
 ~5m       ~45m          B1      ~55m            B2      ~45m              ~55m
```

Critical path ≈ **5 + 45 + 55 + 45 + 55 = ~205 min ≈ 3.4 h** — matches the observed ~3 h. The path is **4 heavy sequential agents** (Discover/Implement/Critique/Redteam on the 4-page batch) plus a thin Plan. The batch-of-1 lane runs its whole chain concurrently in ~⅓ the wall-clock and then **idles at every barrier** — its idle time is pure waste, not path length.

### 1.3 The re-reading tax (measured, not estimated loosely)

Every one of the **8 post-Plan agents** is commanded by `READ_MANDATE` (impl/crit/redteam) and `discoverPrompt` to re-derive the identical grounding from scratch — "ULTRA-DEEP-READ the language doctrine … never a subset; the campaign brief in FULL; BOTH .api tiers … list both tier directories in full." Measured corpus for the C# geometry leg:

| Re-read corpus (fixed, page-independent) | Lines | ~Tokens |
|---|---|---|
| `docs/stacks/csharp` core (8 files) + ~2-3 relevant `domain/` shards | ~3,200 | ~38k |
| shared `libs/csharp/.api/` (31 catalogs, relevant subset ~half) | ~2,800 | ~34k |
| folder `libs/csharp/Rasm/.api/` (11 catalogs) | ~1,870 | ~22k |
| brief head + covering sections | ~269 | ~3k |
| **fixed tax per agent** | | **~95k tokens + ~40 ls/Read round-trips** |

Wall-clock of the fixed tax ≈ **15-25 min per agent before a single edit is written** (token ingestion + serial tool round-trips over both `.api` dirs and the doctrine tree). Paid **8×** across a 5-page leg = **~120-200 min of pure re-reading**, of which **~4× sits on the critical path** (Discover, Implement, Critique, Redteam each pay it once) ≈ **~60-100 min of the ~3 h wall-clock is the same doctrine + `.api` catalogs re-ingested four times.** The *page-body* reads (the target pages + siblings + index docs, ~4k lines) are genuine per-stage work and are **not** recoverable; the **fixed tax is**.

### 1.4 Barrier verdict — real vs. habit

| Barrier | Real dependency? | Cost |
|---|---|---|
| **BARRIER-0** Plan→Discover | REAL — the page list drives `chunk` | ~5 min (cheap) |
| **BARRIER-1** Discover→Implement | **HABIT** — `implement(bX)` reads `MAP` for `bX`'s pages ONLY, never the sibling batch's map; the global `await pool` before building `MAP_BY_PAGE` is a convenience, not a data dependency | straggler idle: fast lane waits ~30-40 min for the heavy discover before ANY implement starts |
| **BARRIER-2** Implement→Critique | **HABIT for folder-disjoint batches**; PARTIALLY REAL only where two batches contend on one folder's index docs | same straggler idle |
| crit→redteam (intra-pool) | REAL — redteam reads `crit` result and re-verifies its repairs | correctly sequential, already pipelined per batch |

**BARRIER-1 and BARRIER-2 are the removable ones** — they serialize batches that have no cross-batch data dependency, converting per-batch dependencies into whole-stage waits.

---

## 2. PIPELINE VS BARRIER + WRITE-COLLISION

### 2.1 What can pipeline

`implement(bX)` depends only on `discover(bX)`; `critique(bX)` only on `implement(bX)`; `redteam(bX)` only on `critique(bX)`. This is the textbook `pipeline(batches, discover, implement, critiqueThenRedteam)` shape (`api-reference.md` §6, `patterns.md` §3): each batch flows its whole 4-stage chain independently, **no barrier between stages**, wall-clock = the slowest single **batch's chain**, not the sum of the slowest **stage** at each barrier. On the imbalanced 5-page leg the pipeline win is modest (~1× the fast-lane idle per barrier, ~30-60 min); it **grows with balanced, multi-folder waves** where today's straggler idle is largest.

### 2.2 The collision the reconcile-removal already created

The engine is **reconcile-free** by law (`campaign-method.md` [04]: "every agent … WRITES; the agent who sees the defect owns the fix; deferral is a defect"). So impl/crit/redteam agents edit **cross-file** — seam counterparts, consumer sites, and the folder **index docs** (`ARCHITECTURE.md`, `README.md`). Under the current pool, **batches within a stage already run concurrently** (CAP=10). Therefore **the collision hazard exists TODAY**, not only under a future higher-parallelism design:

- **Index-doc contention** — `implement(b0)` and `implement(b1)` from the same folder both append/rewrite that folder's `ARCHITECTURE.md`/`README.md` → last-write-wins clobber, or an anchored `Edit` failing because a concurrent edit moved its anchor.
- **Seam-counterpart contention** — `critique(pageA)` edits `pageA`'s seam counterpart `pageB` while `pageB`'s own agent rebuilds `pageB`. The live brief makes this concrete: the Wave-2 pre-motion "`Processing/repair.md` improve — `BooleanOp` … excised to `arrangement.md`" edits `repair.md` from `arrangement.md`'s pass.
- **Read-during-write** — a pipelined `discover(folderB)` reading "the folder at large" while `implement(folderA)` writes is only safe when A≠B; same-folder overlap makes discover map a half-rebuilt sibling.

Today this is masked by low batch count (2 batches / 5 pages) and the anchored-Edit convention — **not** by any structural guarantee. Shrinking batches or adding pipeline overlap **removes the mask**.

### 2.3 The partitioning rule that makes parallelism safe

**Folder = the single-writer boundary**, because the contended shared files (`ARCHITECTURE.md`/`README.md`) are folder-scoped. Two rules, layered:

1. **FOLDER-DISJOINT LANES.** Partition a wave's pages into per-folder lanes. Cross-folder lanes touch disjoint index docs → free concurrency (pipeline or pool) with zero index collision, and no read-during-write (discover of folder B never reads folder A's in-flight files). This is `patterns.md` §14's work-unit lever: make the coherence boundary (the folder's index docs) the concurrency unit.
2. **ANCHORED-EDIT-REAPPLY for the irreducible cross-folder seams.** A seam counterpart lives in another folder's index/page; that edit stays surgical (`patterns.md` §13 collision discipline): "edit pages a sibling may share with surgical anchored `Edit`s only, re-reading and re-applying on an edit conflict — never a whole-file rewrite." The brief already freezes wire-canonical names, so the seam edit is a bounded row insert, never a foreign-interior rebuild. This is mandated in-prompt, not enforced by a lock — it keeps the no-reconcile law intact (the agent that disturbs the seam fixes both ends in-pass) while making concurrent disturbance safe.

Within a big folder (Processing has 11 pages), keep its sub-batches **in one lane serialized** so the folder's index docs have a single writer at a time. Page **bodies** are disjoint files and could run parallel; the only contention is the index rows — the refinement is to let page-bodies fan out and funnel index-row edits to the lane's last sub-batch, but the safe baseline is a serialized folder lane.

**Net:** never let two concurrent agents write one folder's index docs. Folder lanes give that by construction; anchored-reapply handles the residual cross-folder seam edits.

---

## 3. THE MULTI-LEG WAVE LIST (the big one)

### 3.1 The finding that reframes this: the brief ALREADY IS a wave list

`RASM-CS-GEOMETRY-DECISION.md` declares **`### Wave 1` / `Wave 2` / `Wave 3` / `Wave 4`**, each with "listed order = dependency order," explicit **intra-wave backward edges** and **equal-or-earlier cross-wave edges** ("every edge lands equal-or-earlier wave; intra-wave edges point backward"). The run ledger's "Leg 1..5" map **one-to-one** onto the brief's waves. So the current engine takes an already-partitioned, already-dependency-ordered wave list and forces a **human to slice it into N separate `Workflow` runs** — each paying its own Plan + Discover, each needing a manual relaunch, a fresh run-ledger, and a wait-for-notification handoff. **The wave loop imposes no new authoring burden on the brief; it consumes the partition the brief already expresses.**

The per-leg-run tax the loop removes:
- **N−1 human relaunch cycles** — the dominant hidden cost. Between legs a human must notice the completion notification, review, commit, and relaunch — routinely **30 min to hours** of dead wall-clock per handoff. Leg 2 shows the worst case: it died on credit exhaustion and required a **hand-authored salvage agent + a manual, user-signal-gated leg-3 relaunch** (`geometry-run-ledger.md` lines 10-22).
- **N−1 redundant Plan agents** — one Plan emits all waves.
- The cognitive load of tracking N run IDs / N resume commands.

### 3.2 Concrete design (matches the real harness API)

**Schema change — the wave is a page discriminant, not a parallel structure** (one page list, discriminated by an integer, per the density law):

```js
// --- [MODELS] ---
const PLAN_SCHEMA = { type: 'object', additionalProperties: false, required: ['packages', 'pages'], properties: {
  packages: { /* unchanged */ },
  pages: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['page', 'kind', 'wave'], properties: {
    page: { type: 'string' }, kind: { type: 'string', enum: ['new', 'rebuild', 'improve'] },
    absorb: { type: 'string' }, wave: { type: 'integer' } } } },
  deletePages: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['page', 'capturedIn', 'wave'],
    properties: { page: { type: 'string' }, capturedIn: { type: 'string' }, wave: { type: 'integer' } } } } } }
```

`planPrompt` gains one clause: *"Assign each in-scope page its `wave` index from the brief's `### Wave N` partition (a page under `Wave K` → `wave: K-1`); a page with no declared wave → `wave: 0`. A page carrying BOTH an earlier-wave `improve` pre-motion AND a later-wave main motion emits TWO rows — one per (wave, kind)."* An optional `args.waves` (an array-of-page-arrays) lets a caller pin the partition without a brief.

**Composition — the inter-wave barrier is the JS `for` loop; intra-wave is folder-lane parallelism:**

```js
// --- [COMPOSITION] ---
phase('Plan')
const plan = await agent(planPrompt(''), { label: 'plan', phase: 'Plan', model: 'sonnet', effort: 'low', schema: PLAN_SCHEMA, stallMs: STALL })
const PAGES = normalizePages(plan)                                   // each row carries .wave (default 0), keyed (page|wave)
const deletePages = (plan?.deletePages || []).filter((d) => d && d.page)
const WAVES = [...new Set(PAGES.map((p) => p.wave))].sort((a, b) => a - b)

const landed = []
for (const w of WAVES) {                                            // ── INTER-WAVE BARRIER = this await boundary ──
  const wp = PAGES.filter((p) => p.wave === w)
  const wd = deletePages.filter((d) => d.wave === w)
  if (!wp.length && !wd.length) continue
  const lanes = groupByFolder(wp)                                   // folder = single-writer boundary (§2.3)

  phase('W' + w + ' Discover'); phase('W' + w + ' Build')
  MAP_BY_PAGE = new Map()                                           // map is WAVE-LOCAL — re-derived from landed disk
  const out = (await pool(lanes, CAP, async (lane) => {
    const disc = await agent(discoverPrompt(lane.pages, w), { label: 'discover:w' + w + ':' + lane.folder,
      phase: 'W' + w + ' Discover', model: 'opus', effort: 'max', schema: DISCOVER_SCHEMA, stallMs: STALL })
    for (const m of (disc?.worklist || [])) if (m?.page) MAP_BY_PAGE.set(m.page, m)   // lane-local, disjoint keys
    for (const batch of chunk(lane.pages, IMPL_BATCH)) {           // one folder lane = one index-doc writer, serialized
      await agent(implementPrompt(batch, w), { label: 'impl:w' + w + ':' + lane.folder, phase: 'W' + w + ' Build', model: 'fable', effort: 'max', schema: FIXLOG_SCHEMA, stallMs: STALL })
      const crit = await agent(critiquePrompt(batch, 0), { label: 'crit:w' + w + ':' + lane.folder, phase: 'W' + w + ' Build', model: 'fable', effort: 'xhigh', schema: REVIEW_SCHEMA, stallMs: STALL })
      await agent(redteamPrompt(batch, 0, crit), { label: 'rt:w' + w + ':' + lane.folder, phase: 'W' + w + ' Build', model: 'fable', effort: 'max', schema: REVIEW_SCHEMA, stallMs: STALL })
    }
    return { folder: lane.folder, pages: lane.pages.length }
  })).filter(Boolean)
  if (wd.length) await agent(deletePrompt(wd), { label: 'delete:w' + w, phase: 'W' + w + ' Build', model: 'fable', effort: 'high', schema: DELETE_SCHEMA, stallMs: STALL })

  const built = out.reduce((n, o) => n + o.pages, 0)
  log('Wave ' + w + ': ' + out.length + '/' + lanes.length + ' folder-lanes landed, ' + built + ' pages')
  if (out.length === 0) { log('Wave ' + w + ' produced nothing — HALTING before dependent waves'); break }  // failure isolation
  landed.push({ wave: w, folders: out.length, pages: built })
}
return { targets: TARGETS, language: LANG_KEY, brief: BRIEF, waves: landed }
```

Cross-folder concurrency runs disjoint index docs; each folder lane serializes its own discover→build chain, so its index docs have exactly one live writer. The whole discover→impl→crit→redteam chain runs **inside** the lane worker, giving the pipeline overlap **across folders** (folder A in redteam while folder B is in implement) with **no shared-file collision** — the §2.3 rule satisfied by construction.

### 3.3 The mandate's sub-questions, answered

- **Resume semantics across waves.** The loop is deterministic (`WAVES` derives from the cached Plan output; every agent prompt embeds the page list + wave index, all stable). On `Workflow({ scriptPath, resumeFromRunId })` the script replays: completed waves' agents hit the journal cache instantly (`api-reference.md` §11 — key = `v2:sha256(prompt + schema/model/isolation/agentType)`), the in-flight/unstarted run live. A **mid-wave death resumes in place and auto-continues into dependent waves** — the exact failure that forced leg-2's manual salvage now needs only a resume. Soundness of the disk dependency: a resumed wave-N+1 discover reads the disk that wave-N's agents (cached, but their edits are already on disk from the original run) left → consistent.
- **Failure isolation (a dead wave must not orphan the run).** Two layers: (1) agent-level — the harness aborts a stalled agent after 5 retries and its `agent()` resolves; `.filter(Boolean)` lets the lane continue. (2) wave-level — a **health gate**: `if (out.length === 0) break` halts before dependent waves rather than cascading a broken foundation forward. Budget/credit exhaustion (`WorkflowBudgetExceededError`, §9) lets in-flight agents finish and keeps them journaled, starts no new ones — the partial wave is checkpointed on disk; resume in a fresh turn continues. The loop deliberately does **not** self-guard budget beyond this natural behavior — the journal + resume is the recovery, not a budget branch.
- **Kinds / absorbs spanning waves.** `kind` is per-(page,wave) row — a page's Wave-1 `improve` pre-motion and Wave-3 `rebuild` main motion are two rows, keyed distinctly, so the wave-local `MAP_BY_PAGE` never collides them (it is rebuilt per wave). **An absorb pair is atomic in the absorber's wave** — the absorber moves content then deletes the absorbed page in its own pass, so absorber and absorbed travel in one wave; Plan collapses any brief split by homing the pair to the absorber's (later) wave. `deletePages` are wave-tagged to the wave whose destination content lands; a page deleted in wave K referenced by a wave >K page is repaired in-pass by that page's discover (it reads landed disk). Cross-wave content moves (the brief's Wave-2 excision into a Wave-3 page) are safe precisely because the inter-wave barrier serializes them — this is **why** waves barrier and do not pipeline.
- **Leg-boundary residual-cleanliness checkpoint — survives, simplified.** The reconcile-free engine has no cross-wave residual machinery by design; the checkpoint is a `log` + the barrier, not an agent. Wave-N redteam already "VERIFY[s] the critique's cross-file repairs landed on BOTH ends of every touched seam" (line 678) — the cleanliness is enforced *inside* the wave, and the barrier guarantees wave N+1's discover re-derives drift from wave N's **landed** disk (the ledger's "each leg's Discover re-derives the drift from disk"). No git commit runs inside the loop (the orchestrator has no filesystem, §10); the launching turn commits after the run, or an optional per-wave sonnet `git add -A && commit` micro-agent can checkpoint — but mid-run commits risk partial-campaign commits if a later wave halts, so **commit-at-end is the recommendation**, with an optional `args.commitPerWave` valve.

### 3.4 Phase-count discipline

Naively `phase()`-ing five stages × N waves = spam. The **real** structural boundaries are: Plan, and per wave a **Discover barrier** (the one intra-wave read-gate) and a **Build lane** (impl→crit→redteam are one continuous per-folder chain, so grouping them as one "Build" phase is honest — the critique/redteam split is display sugar the pool already collapses). That is **`Plan` + 2×N phases**: 5 phases for a 2-wave leg, 9 for a 4-wave campaign — structural, never spam. The wave index rides the agent **label** (`impl:w2:Processing`), which is not in the resume key (§5), so re-labelling never invalidates cache. Do **not** mint a phase per (wave, stage); do **not** keep the standalone `Critique`/`Redteam` display phases — the fewest phases that show the wave-dependency structure are `Plan` + `W{k} Discover` + `W{k} Build`.

---

## 4. SPEED VS QUALITY LEVERS

| Lever | Speed effect | Quality risk | Recommendation |
|---|---|---|---|
| **Grounding-dossier hand-off** — Discover writes an ANCHORED verified-primary dossier; impl/crit/redteam read it + spot-verify instead of re-ingesting the full doctrine + both `.api` tiers | removes ~3 of 4 fixed re-reads on the critical path ≈ **45-100 min/leg** | the standing distrust law: agents are agreeable and will TRIM/trust a passed artifact, so a dossier that becomes a **substitute** for verification propagates phantoms and violates the "pointer, not ceiling" law | **ADOPT with honesty rails** (below). Highest wall-clock ROI. |
| **Wave loop** (§3) | kills N−1 relaunch cycles + N−1 Plans; collapses inter-leg human latency (hours) into one unattended run; resume in-place | a poisoned early wave cascades to dependent waves before a human reviews (the per-leg model has a human checkpoint) | **ADOPT.** Guard with the wave health-gate + inter-wave barrier; keep an **optional** `args.pauseAfterWave` for human-in-loop campaigns (a `/workflows` pause, not a separate run). |
| **Folder-partitioned pipeline within a wave** (§2.3) | removes BARRIER-1/BARRIER-2 straggler idle (~30-60 min on balanced waves); overlaps folder A's build with folder B's discover | higher concurrency multiplies index-doc + seam collision | **ADOPT with the folder-single-writer partition + anchored-Edit-reapply.** Never pipeline without the partition. |
| **Smaller batches** (`IMPL_BATCH` 4→2/1) | more concurrency (up to CAP=16), shorter per-agent chains | (a) loses cross-page seam coherence a 4-page agent unifies in one head → more cross-file ripple + collision; (b) **multiplies the fixed re-read tax** (per-agent, not per-page); (c) more index-doc writers | **HOLD until the dossier lands.** Batch size is coupled to the reading tax — shrinking batches before the dossier removes the re-read multiplies the tax. After the dossier: batch 2-3 for build stages is viable; keep **batch 4 for discover** (coherence-heavy, tax amortizes over 4 pages). |

### 4.1 The dossier honesty rails (keeping the distrust law intact)

The dossier **replaces the redundant doctrine/`.api` RE-READ, never the page-specific adversarial attack.** Three structural checks keep it honest, mirroring how redteam already re-checks critique:

1. **Anchored citations.** The dossier carries `apiUsed: [{catalog, member, anchor}]` with a `file:line` anchor (verified by discover via `assay api` / the `.api` catalog). It quotes the **specific** doctrine owner-chooser rows and `.api` member blocks relevant to these pages — verified **primary material**, not a lossy doctrine summary. Reading the dossier is reading confirmed primary extracts, not trusting a digest.
2. **Successor-verifies-predecessor schema.** The dossier's first consumer (Implement) re-verifies a **sample** of the dossier anchors and returns `dossierPhantoms: string[]` in its fix-log (one new optional `FIXLOG_SCHEMA` field). A phantom in the dossier is a **loud discover defect**, surfaced exactly as redteam surfaces a critique miss — discover is audited by its consumer, never self-certified.
3. **"Pointer, verify, exceed" framing preserved.** `READ_MANDATE` already states "the map POINTS; you VERIFY and EXCEED it." The dossier keeps that verbatim and **downgrades** the blanket "re-read the full doctrine + both full `.api` tiers" to "read the dossier's verified extracts + SPOT-VERIFY its anchors + read the full doctrine ONLY for the shards the dossier flags." The adversarial attack on the **code** stays fully intact — it never depended on re-ingesting the whole doctrine, only on attacking the fence against the verified domain + package surface, which the dossier supplies pre-verified.

**Channel.** The thin, journaled pointers (anchors, `weak`, stacking) stay in the Discover **return value** (`DISCOVER_SCHEMA` worklist — resume-safe, already the existing channel). The heavy verified-primary extracts (doctrine row quotes, `.api` member blocks) that would bloat every downstream prompt go to a **per-folder scratch dossier** `.claude/scratch/<campaign>/grounding-w{K}-<folder>.md`; the downstream prompt carries the **path** with a fallback clause ("if absent, fall back to the full doctrine/`.api` read") — the two sanctioned handoff channels (script-carried returns + scratch files), resume-robust because the scratch file is on disk from the original run and the fallback covers a cleaned scratch dir.

---

## TOP 3 STRUCTURAL CHANGES (ranked)

1. **WAVE LOOP — one run executes the brief's already-declared dependency-ordered wave list.** The geometry DECISION already carries `Wave 1..4` with intra-wave backward edges and equal-or-earlier cross-wave edges; the engine currently forces a human to slice it into N separate runs, each with its own Plan/Discover and a manual relaunch. A deterministic `for (w of WAVES)` loop with the barrier at the loop boundary and folder-lane parallelism inside kills the per-leg workflow-spam, collapses hours of inter-leg human handoff latency into one unattended run, and makes a mid-wave death (leg-2's credit exhaustion) resume in place instead of needing a hand-authored salvage. Schema cost: one `wave: integer` discriminant on `PLAN_SCHEMA.pages`/`deletePages`; the wave is a page field, not a parallel structure.

2. **GROUNDING-DOSSIER HAND-OFF — Discover verifies once, downstream reads + spot-verifies.** The measured fixed re-read tax is ~95k tokens + ~40 tool round-trips (~15-25 min) paid by all 8 post-Plan agents, ~4× of it on the critical path. Discover writes an anchored (`catalog:line`) verified-primary dossier; impl/crit/redteam read it and spot-verify instead of re-ingesting the full doctrine + both `.api` tiers — ~45-100 min off each leg's critical path. Honesty is held by anchored citations, a `dossierPhantoms` successor-verify field, and the preserved "pointer, verify, exceed" law: the dossier replaces the redundant re-read, never the adversarial attack on the code.

3. **FOLDER-PARTITIONED PIPELINE + SINGLE-WRITER SAFETY — remove BARRIER-1/BARRIER-2, make concurrency safe.** BARRIER-1 (Discover→Implement) and BARRIER-2 (Implement→Critique) are habit, not data dependencies — they convert per-batch dependencies into whole-stage straggler waits. Removing them via a per-folder pipeline lane recovers the straggler idle; the reconcile-free engine's cross-file writes make that safe **only** under the rule that the folder's index docs (`ARCHITECTURE.md`/`README.md`) have a single live writer — guaranteed by folder-disjoint lanes plus the anchored-Edit-reapply discipline for the irreducible cross-folder seam counterparts. This collision hazard already exists in today's concurrent pool (masked by low batch count); the partition is what lets batch size shrink or overlap grow without corruption.
