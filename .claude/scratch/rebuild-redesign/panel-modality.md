# panel-modality — rebuild.js redesign, lens 4/5 (MODALITY: brief vs no-brief)

Scope: `.claude/workflows/rebuild.js` (working-tree state, 735 LOC — the uncommitted delta that DELETED the Reconcile stage is the live design intent; committed HEAD `b24cbc6c3` still carries reconcile). All line anchors are against the working-tree file.

## [SUMMARY] — sharpest modality defects

1. **The engine is brief-FIRST with no-brief as a degenerate fallback, not a peer.** The brief modality owns a full kind/action vocabulary `{new, rebuild, improve}` + `absorb` + `delete` + scope-law + concept-source + frozen-invariant + ripple-rules; the no-brief modality collapses to a SINGLE sentence (`:576`) that admits one verb — `rebuild` — for every page and nothing else. 4 of 5 action semantics (`new`, `improve`, `absorb`, `delete`) are physically unreachable without a brief. That is brief-as-primary, no-brief-as-afterthought.
2. **The brief's frozen-invariant + scope law is injected at IMPLEMENT (`:618`) but NOT in the task bodies of CRITIQUE (`:631-652`) or REDTEAM (`:653-680`) — the two MOST aggressive rebuild passes.** They inherit the frozen law only through the long shared `LAW` head (`:455-460`) via `PRE`, never as a task-level guard. The passes most likely to churn a brief-frozen signature are the ones missing the task-level "these are byte-identical" reminder. This is backwards and is the single most dangerous leak.
3. **The no-brief "folder charter as scope authority" is a phantom.** The only folder-charter reference (`:583`) is gated on kind=`new`, which the no-brief plan can never emit — so the no-brief modality has NO self-derived scope/intent authority at all. It reads the charter as passing context (index docs at `:584`) but never as the intent law the brief is. No-brief cannot author a charter-demanded missing page and has no frozen-page mechanism.
4. **The brief carries a leg partition as first-class data (`RASM-CS-GEOMETRY-DECISION.md [10]`, `RASM-PY-DATA-BRIEF.md [05]`), but the engine cannot read it.** Leg-slicing is 100% external — the caller hand-copies a per-leg `targets` array into each invocation. The two live briefs disagree on the invocation shape (GEOMETRY passes per-leg page arrays; PY-DATA passes the whole folder and claims "one leg per run" — which the current engine CANNOT honor: a whole-folder brief run admits every brief-named page across ALL legs at once). This inconsistency is concrete proof the leg selector must become engine input.

## [HISTORY] — what hardened, what was lost, what was rightly fixed

The durable engine was brief-aware from its FIRST authoring (`2a5e49d20`, 07-01: 33 brief-mentions). There was never a "pure general-rebuild" era to recover — the modality tension is native. The relevant hardening happened in the plan's scope-classification branch and in the ripple/reconcile machinery:

| Axis | Original (`2a5e49d20`, 07-01) | Current (working tree) | Verdict |
|---|---|---|---|
| Brief scope | Named pages → new/rebuild; **every OTHER page under targets → `improve` (the cold-pass set)** — a brief run swept the WHOLE folder | Named pages only; **every unnamed page OUT OF SCOPE** (`:571-573`) — "no cold-pass set, no general-improve tangents" | **Hardening was RIGHT.** The old brief branch would blanket-`improve` the landed/frozen pages a DECISION marks "standing law, never re-litigated" (GEOMETRY's 34 kernel pages). Precise scope protects frozen work. |
| No-brief scope | `every page gets kind rebuild` (`:576`) | Identical (`:576`) — untouched | **Unchanged — and that's the defect.** No-brief never gained the frozen/settled distinction the brief scope forced. Point a no-brief run at a folder with landed kernel pages and it hostile-rebuilds them with no freeze mechanism. |
| Ripple repair | Deferred to `residual_high` → union-find clusters → ≤6 opus fixers → 1 verify (a whole RECONCILE stage) | Reconcile DELETED; **every pass repairs its own cross-file ripple in-pass** (`:451-454`, `:63-65`, `:167-168`, `:177-178`); "deferral is a defect" | Modality-neutral change, but it RAISES the brief-frozen-churn risk of defect #2: in-pass ripple repair means critique/redteam now edit seam counterparts and sibling pages directly (`:167` "repair the seam counterpart... yourself"), yet still without the brief's frozen-name guard in their task body. More write authority, same missing guard. |
| Cold-pass breadth | Brief runs covered the whole folder (named + improve) | Brief runs cover only named pages | Breadth MOVED from brief-mode to no-brief-mode. No-brief is now the ONLY every-page-hostile modality; brief is surgical. This is a coherent split — but only no-brief kept breadth, and no-brief kept it WITHOUT any settled-page protection. |

Net: the hardening correctly made the brief modality SURGICAL (scope = brief-named set) and correctly pushed ripple repair in-pass. It never touched the no-brief modality, which still means "expand targets, rebuild every page, no skip, no author, no delete." The strength the general modality retains — unconditional every-page hostility — is real and worth keeping as no-brief's identity. The weakness it never lost — zero frozen/settled awareness, zero charter-driven authoring — is the no-brief work item.

## [MODALITY-NEEDS] — what each modality genuinely requires as a first-class citizen

| Need | BRIEF modality | NO-BRIEF modality |
|---|---|---|
| Scope authority | Brief-named pages ∩ passed targets; unnamed = out of scope (`:571-573`). CORRECT today. | Full-target expansion: every existing page under the target → in scope (`:562-565` + `:576`). CORRECT today, but MISSING a settled/frozen skip and a charter-demanded `new` admission. |
| Kind vocabulary | `{new, rebuild, improve}` + `absorb` + `delete`, each ruled per-page by the brief against disk (`:567-575`). Full and correct. | Today: `{rebuild}` only (`:576`). SHOULD gain: `new` (charter-demanded missing page) and an explicit skip for charter-frozen pages. `improve` genuinely does not apply (no brief KEEP-with-extension rows exist to drive it) — leave it brief-only. |
| Concept source (for a `new` page) | "in the brief" (`:583`) — the brief section is the spec. | "from the folder charter" (`:583`) — but DEAD because no-brief emits no `new`. Must become live: the folder `ARCHITECTURE.md`/`README.md`/`IDEAS.md` charter IS the no-brief intent authority. |
| Intent / per-page concern | Brief head + covering sections, read at discover (`:581`) and implement. | Folder charter (index docs), currently read only as CONTEXT (`:584`), never elevated to intent law. Must become the no-brief peer of the brief's per-page concern. |
| Frozen invariants | Named by brief, "byte-identical law" (`:457-458`), restated at implement (`:618-619`). MISSING at critique/redteam task bodies. | No mechanism. A first-class no-brief needs a folder freeze marker (a charter row / IDEAS "landed, do not re-litigate" list) so it does not churn settled pages — OR an explicit contract that no-brief is only ever pointed at genuinely unsettled folders (weaker, caller-discipline-dependent). |
| Consumer-ripple rules | Brief consumer-ripple rules govern seam-name and counterpart edits (`:64`, `:459`). | Falls back to the generic BOUNDARY LAW seam rules (`:133-138`) — surgical seam repair, wire-canonical names frozen. Coherent; no gap. |
| Deletion authority | `deletePages` (brief-declared, verify-then-delete, `:624-630`) + `absorb`-then-delete. Only phantom deletion otherwise (`:449-450`). | Zero deletion authority — `deletePages` always empty (`:576`), REMOVAL DISCIPLINE forbids concept deletion absent a brief ruling (`:445-450`). CORRECT: a standing cold-pass must never delete pages. Keep. |
| Wave/leg declaration | The brief ALREADY carries it (`[10]`/`[05]`). Engine must READ it (see [LEG] below). | No leg concept — a flat single-pass every-page run. Leg is strictly a brief injection. |

## [LEAK-CENSUS] — modality leaks with file:line anchors

Two failure directions: (A) brief assumptions degrading no-brief runs; (B) no-brief generality diluting brief precision.

Substantive brief conditionals total SIX sites: `:455`, `:549`, `:566`, `:581`, `:583`, `:618` (plus one cosmetic log label `:695`).

### (A) Brief-shaped language reaching / degrading no-brief runs

- **`:583` — DEAD no-brief branch.** `(BRIEF ? 'in the brief' : 'from the folder charter')` is gated inside the kind=`new` clause of discover. No-brief never produces `new` (plan `:576`), so the `'from the folder charter'` arm is unreachable. The intended no-brief scope authority exists in the prompt but is wired to a branch the plan can't reach. LEAK: latent capability disconnected.
- **`:581` → empty in no-brief.** The per-page-concern read is `(BRIEF ? 'Read ' + BRIEF ... : '')`. In no-brief this injects NOTHING — no-brief discover gets no intent/concern authority, only page + folder + `.api`. LEAK: no-brief has no analogue of the brief's per-page concern.
- **`:445-450` REMOVAL DISCIPLINE — brief-shaped clause in the shared head.** "removed ONLY where the campaign brief explicitly rules that removal." In no-brief there is no brief, so the clause silently collapses to "phantoms only." Semantically coherent, but it is written as a brief-conditional statement delivered unconditionally to no-brief agents. MILD LEAK: reads as if a brief exists; a no-brief agent must infer the collapse.
- **`:618-619` implement frozen clause — absent in no-brief (correctly).** `(BRIEF ? 'Every paradigm, rename, absorb, and delete decision follows the brief; frozen invariants ... byte-identical. ' : '')`. Clean conditional; no no-brief degradation. NOT a leak — cited as the correct pattern the other prompts should mirror.

### (B) No-brief generality diluting brief precision

- **`:631-652` critiquePrompt + `:653-680` redteamPrompt — NO task-body brief guard.** Both carry the FULL no-brief hostility ("rebuild to the strongest form the doctrine admits", counterfactual-collapse, `:658-661`) with zero task-level reference to brief scope or frozen invariants. They receive the frozen law only through the shared `LAW` head (`:455-460`) buried in `PRE`. The two most aggressive passes — the ones with in-pass write authority over seam counterparts (`:167`, `:177-178`) — are the ones missing the task-body "byte-identical / follow the brief" reminder that the LEAST aggressive pass (implement, `:618`) has. SHARPEST LEAK: brief precision is diluted exactly where churn risk peaks.
- **`:576` no-brief plan fallback — blunt, one-verb.** `'No campaign brief: every page gets kind rebuild; deletePages is empty; no absorb pairs.'` This is the entire no-brief scope law — one sentence against the brief branch's nine-line paragraph (`:567-575`). It cannot express skip, author, or improve. Under a brief the plan is a precise classifier; without one it is a rubber stamp. The asymmetry is the structural root of defect #1.
- **`:549-550` READ_MANDATE renumber.** `(BRIEF ? '(2) the campaign brief ...' : '')` + `'(' + (BRIEF ? '3' : '2') + ')'`. Clean, correct — the only fully-symmetric conditional in the file. Cited as the pattern to generalize.

### Prompt-fragment classification (shared law vs modality injection)

- SHARED (identical both modes, no conditional): `LAW` head corpus/strata/stackLaw/apiTiers (`:441`), FUNDAMENTAL-REBUILD (`:442-444`), WRITE-FULLY (`:451-454`), `ADVERSARIAL` (`:461-483`), `EXTEND` (`:484-508`), `PROSE`/`COMMENTS` (`:509-524`), `L.ultra`/`L.mechanics`/`L.patlaw`/`L.boundaries` (doctrine), READ_MANDATE doctrine + `.api`-tier read (`:547-556` minus the brief insert). These are correct as shared.
- BRIEF-ONLY injections (today, scattered as inline ternaries): the CAMPAIGN-BRIEF `LAW` concat (`:455-460`), READ_MANDATE brief read (`:549`), plan brief-classification (`:566-575`), discover brief-concern (`:581`) + brief new-concept arm (`:583`), implement frozen clause (`:618-619`), the entire `deletePrompt` (`:624-630`, only ever invoked when `deletePages` is non-empty, i.e. under a brief). MISSING brief injections: a frozen/scope guard in critique (`:631`) and redteam (`:653`) task bodies; a leg selector.
- NO-BRIEF-specific injections (today, near-absent): plan fallback (`:576`) and the unreachable discover charter arm (`:583`). BOTH under-built — they must be RAISED to a real folder-charter scope law (full-target expansion + charter-demanded `new` + settled-page skip + charter-as-per-page-concern).

## [LEG] — should the brief modality accept the wave/leg declaration directly? YES.

Evidence. `RASM-CS-GEOMETRY-DECISION.md [10]` (`:255-260`) declares each leg as a literal `Workflow(rebuild.js, {targets: [Numerics/predicates.md, ...9 pages...], brief: "RASM-CS-GEOMETRY-DECISION.md"})` with per-row kinds already ruled ("rebuild ×5, new ×1 (edit), improve ×3 (repair = donor excision ONLY; pack = phantom kill ONLY; ...)") plus a rider column (roster motions, ripple sweeps, counterpart edits). The caller hand-copies the per-leg page array into `targets`. `RASM-PY-DATA-BRIEF.md [EXECUTION]` (`:5-11`) instead passes `{targets: "libs/python/data", brief}` — the WHOLE folder — and claims "One `[05]-[BUILD_LEGS]` leg landed per run" and "loops once per leg in partition order." The current engine CANNOT honor that: a whole-folder brief run admits EVERY brief-named page across all three legs in one run (plan `:567-575` admits all brief-named pages under the target). So the two live briefs contradict each other on the invocation contract, and one of them is already impossible against the engine as written.

The leg partition ALSO already declares, per row, exactly what the plan step re-derives from scratch: which pages, which kind, which absorb pairs, which deletePages, which riders. The plan agent (`:689`) re-classifies "each page FROM THE BRIEF against REAL DISK STATE" — duplicating a ruling the DECISION `[10]` table already made. That is the classic re-derivation defect the campaign law forbids.

**Modality-correct shape.** Add an optional `leg` selector to the brief modality only (no-brief has no legs):

```
args = {targets?, brief, leg?}   // leg ∈ number | number[] | 'all'
```

- When `leg` is present, the plan step READS the brief's leg-partition table (`[10]` / `[05]-[BUILD_LEGS]`) and admits ONLY the selected leg's page rows, consuming their DECLARED kind / absorb / deletePages / riders VERBATIM (verified against disk for absence/presence, never re-invented). `targets` becomes optional and, when omitted, is DERIVED from the leg rows — the leg rows ARE the targets. This collapses GEOMETRY's hand-sliced arrays and PY-DATA's whole-folder-one-leg framing into one contract: `{brief, leg: N}`.
- `leg: [1,2]` or `leg: 'all'` enables one-run multi-leg (the dependency-ordered waterfall) when the caller wants it — the plan unions the selected legs' rows in partition order. Single-leg-per-run stays the default the briefs assume.
- No-brief runs never carry `leg`; passing one with no brief is a no-op (nothing to read a partition from).
- The `[10]` acyclicity guarantee (`:253`) means an engine reading the partition can also VERIFY the requested leg's upstream legs are landed (every edge lands equal-or-earlier wave), turning a silent out-of-order invocation into a plan-time refusal.

This makes the leg partition a FIRST-CLASS engine input rather than caller-hand-sliced `targets`, kills the plan-step re-derivation, and resolves the GEOMETRY-vs-PY-DATA inconsistency. It is a pure brief-modality injection — the no-brief every-page pass is unaffected.

## [DESIGN] — proposed dual-modality prompt assembly

Replace the six scattered `(BRIEF ? … : …)` inline ternaries with ONE modality discriminant resolved once, exactly as `L = LANG[LANG_KEY]` resolves the language once (`:438`). Both modalities become symmetric field tables — the CLAUDE.md "one polymorphic owner, discriminate on input shape" law applied to the workflow's own modality dispatch.

```js
// --- [MODALITY] ---
const MODE = {
  brief: {
    scopeLaw:      /* plan: admit brief-named pages ∩ targets (or leg rows); unnamed out of scope */,
    conceptSource: 'in the brief',
    intentRead:    'Read ' + BRIEF + ' (head + covering sections) for the per-page concern. ',
    readInsert:    '(2) the campaign brief ' + BRIEF + ' in FULL; ',   // renumbers .api tier to (3)
    decisionClause:'Every paradigm, rename, absorb, and delete decision follows the brief; frozen invariants the brief names stay byte-identical. ',
    frozenGuard:   'BRIEF-FROZEN: a signature/name/invariant the brief freezes is byte-identical law — never rename, re-shape, or re-collapse it, even to a "stronger" form; the brief scope is the page set, no page outside it is touched. ',
    legRead:       /* when leg present: read [10]/[05], admit the selected leg's rows verbatim */,
    deleteAuthority: true,
  },
  folder: {   // no-brief, first-classed
    scopeLaw:      'full-target expansion: EVERY existing page under the target enters the run as kind `rebuild`; a page the folder CHARTER (ARCHITECTURE.md/README.md/IDEAS.md) demands but disk lacks enters as kind `new`; a page the charter marks LANDED/settled is skipped. deletePages empty; deletion only for phantoms. ',
    conceptSource: 'from the folder charter (ARCHITECTURE.md/README.md + nearest siblings)',
    intentRead:    'Read the folder charter (ARCHITECTURE.md + README.md + IDEAS.md) as the INTENT authority for what each page owns and which pages are settled. ',
    readInsert:    '',                                   // .api tier stays (2)
    decisionClause:'',
    frozenGuard:   'CHARTER-SETTLED: a page the charter marks landed is not re-litigated; every other page is hostile-rebuilt to the strongest form. ',
    legRead:       '',
    deleteAuthority: false,
  },
}
const M = BRIEF ? MODE.brief : MODE.folder
```

Assembly switch: each prompt composes `M.<field>` instead of an inline `BRIEF ?`. Concretely:

- `LAW` (`:455`): `.concat(BRIEF ? [CAMPAIGN BRIEF block] : [folder-charter block])` — BOTH modalities get a scope-authority head, not brief-or-nothing.
- `READ_MANDATE` (`:549`): `M.readInsert` + the renumber derived from `M` (brief → tier is (3), folder → (2)); add `M.intentRead` so no-brief reads the charter as intent, not just context.
- `planPrompt` (`:566`): `M.scopeLaw` — the folder branch is now a real paragraph (full expansion + charter `new` + settled skip), not a one-liner; the brief branch optionally `M.legRead`.
- `discoverPrompt` (`:581`,`:583`): `M.intentRead` + `M.conceptSource` — the folder-charter arm becomes reachable and first-class.
- `implementPrompt` (`:618`): `M.decisionClause`.
- **`critiquePrompt` (`:631`) + `redteamPrompt` (`:653`): ADD `M.frozenGuard` to the task body** — closes the sharpest leak (defect #2). Brief runs get the byte-identical guard at the aggressive passes; folder runs get the charter-settled guard.
- `deletePrompt` fires only when `M.deleteAuthority` and `deletePages` is non-empty — unchanged behavior, now explicitly modality-gated.

Result: two symmetric first-class modalities. Brief = surgical (scope law = brief-named/leg rows, frozen invariants enforced at EVERY pass, leg-aware, full delete/absorb authority). Folder = broad (scope law = full-target expansion + charter-demanded `new` + charter-settled skip, charter as intent authority, phantom-only deletion, no legs). The single `M` discriminant replaces six scattered ternaries; adding a third modality later is one row on `MODE`, not another ternary in every prompt.
