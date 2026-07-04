# PANEL 2 — PROMPT EFFICACY DOSSIER (`rebuild.js`)

Lens question for every sentence: does this change agent behavior, and is it the cheapest sentence that does? The assembled `PRE` preamble is ~3.35k tokens (cs) / ~4.7k tokens (py) BEFORE the per-page reading map and the stage task. Most of that budget restates doctrine the agent is separately mandated to read in full — a drift-prone second source of truth that also weakens the read pressure it means to reinforce.

## [01]-[HEADLINE]

- The single biggest waste: `L.ultra` (the "16 named laws held as fact"), the inline COLLAPSE_SCAN 12-signal enumeration in `critiquePrompt`, `L.ownerChooser`, `L.patlaw`'s pattern content, `L.boundaries`, and the `L.aspects`/`L.rails`/`L.modernity` interpolations are PARAPHRASES of the doctrine README's `[02]-[DOCTRINE]` and `[03]-[COLLAPSE_SCAN]` sections. The agent already hard-reads those under `L.readLaw`. Two sources of truth for one law; the paraphrase has already drifted (e.g., the critique task hardcodes a 12-signal collapse list while the cs README ships 9 rows, py 12, ts 25).
- The most hamfisted repetition: the adversarial re-opener. Every task block (`discover`, `implement`, `critique`, `redteam`) re-derives "disbelieve / dense-code-is-the-prime-suspect / burden-on-the-code / reject good-enough" in fresh prose, on top of the full `ADVERSARIAL` block already in `PRE`. Five statements of one stance.
- The engine law that MUST stay inline (it is nowhere in the doctrine): the adversarial stance, the two naivety axes (COVERAGE/APPROACH, roster→seed-data), REMOVAL DISCIPLINE / buildout-over-removal, WRITE-FULLY + cross-file-ripple ownership, the member-verification rail (`assay api`, phantom=delete), the kind semantics (new/rebuild/improve, absorb-then-delete), and the reading-map contract.
- Proposed shape: replace doctrine paraphrase with a hard READING CHAIN (near-first, one required `ls` act, explicit atlas order, "this prompt does not restate the doctrine — read it at source"), keep the engine law as SIX merged blocks stated ONCE, and reduce every stage task to its own action referencing the shared checks by name. Net: `PRE` cs ~3.35k → ~1.52k, py ~4.7k → ~1.72k; assembled critique cs ~5.33k → ~2.1k.

## [02]-[REDUNDANCY CENSUS]

Occurrence counts are across ONE assembled critique/redteam prompt (`PRE` + `READ_MANDATE` + task); the idea is stated in full or by near-verbatim restatement unless marked "ptr" (a bare pointer, cheap).

| # | Idea | Count | Sites |
| :-: | :--- | :--: | :--- |
| 1 | Collapse ≥3 parallel shapes → one owner, same file, never new file | ~9 | `ultra` item2, `ultra` SHAPE_BUDGET, `EXTEND`, `patlaw` item1, `implement` task, `critique` inline COLLAPSE_SCAN (full 12-signal), `redteam` (A) + cold, `ADVERSARIAL` COLLAPSE FREEDOM; `L.collapseInto` interpolated 3×. Full COLLAPSE_SCAN table ALSO in README `[03]`. |
| 2 | Buildout over removal / never delete capability | ~6 | `LAW` REMOVAL DISCIPLINE (110w), `ultra` PRESERVE, `ultra` item2 tail, `EXTEND` JUSTIFIED, `discover` BUILDOUT FRAMING (90w), `READ_MANDATE` |
| 3 | Verify members / cite only real / phantom=delete | ~8 | `stackLaw`, `ultra` STACK CAPABILITY, `modernity`(6), `READ_MANDATE`, `EXTEND` GAP, `ADVERSARIAL` `L.illusion`, `discover` task, `redteam`(E); `L.verify` interpolated ~7× |
| 4 | Both `.api` tiers, mine fully, layer universals onto folder pkgs | ~8 | `LAW` `apiTiers`, `ultra` STACK CAPABILITY, `ultra` STACK .api (py), `READ_MANDATE`, `discover` task, `implement` task, `critique`(7), `redteam` cold; `L.apiTiers` interpolated 3× |
| 5 | Adversarial stance (disbelieve, dense=prime-suspect, burden-on-code, reject "good enough") | ~6 | `ADVERSARIAL` STANCE, `ADVERSARIAL` ILLUSORY, `discover` task, `implement` task, `critique` task, `redteam` task — each re-derived in fresh prose |
| 6 | Two naivety axes (COVERAGE + APPROACH, roster→seed-data) | ~7 | `ADVERSARIAL` NAIVETY, `ADVERSARIAL` ILLUSORY, `EXTEND` item1, `EXTEND` COVERAGE-OVER-SIZE, `critique`(7), `redteam`(A), `redteam`(F) |
| 7 | Capability completeness ⊥ collapse; own the domain concept; extend in place | ~4 | `EXTEND` (4 items = the concept), `critique`(7), `redteam`(F), partial in `ultra` |
| 8 | LIFECYCLE SPINE / BOUNDARY_ADMISSION, admit once, parameterize ingress+egress | ~4 (py ~5) | `ultra` OPERATIVE-DOCTRINE, `ultra` LIFECYCLE SPINE (near-verbatim of README law), `implement` task, `redteam`(C); py `ultra` leads with it + `mechanics`. Duplicates README `[02]` BOUNDARY_ADMISSION. |
| 9 | Cross-file ripple repair is YOURS, both ends, surgical, wire-frozen | ~5 | `LAW` WRITE-FULLY, `boundaries`, `implement` task, `critique` task, `redteam` VERIFY. Phrase "surgical; wire-canonical names frozen; never a foreign-interior rebuild" appears verbatim 3×. |
| 10 | Write-fully / fix-log is a report, not a to-do | ~4 | `LAW` WRITE-FULLY, `implement` task, `critique` "a fix, never a ledger note", `redteam` |
| 11 | OWNER_CHOOSER / 5 discriminants | ~3 | `ownerChooser` (critique, full 200w table cs), `ultra` item2 (py), `redteam` cold ptr. Duplicates README `shapes.md`. |
| 12 | KNOB_TEST | ~3 | `knob` (critique), `redteam` cold ptr, `ultra`/`mechanics` refs |
| 13 | ASPECTS two-weave | ~4 | `aspects` (critique), `ultra` DEFINITION_TIME_ASPECTS, py `ultra` ASPECT-FIRST (full), `redteam` cold ptr. Duplicates README `[02]`. |
| 14 | RAILS / closed-fault / accumulate-vs-abort | ~5 | `rails` (critique, 130w), py `ultra` RAILS (full), `patlaw` item1, `redteam`(C), `redteam` cold ptr |
| 15 | Modernity / latest language version | ~5 | `patlaw` item2 (full C#14 feature list 130w), `modernity`(6), `redteam` cold; `L.modern` interpolated 4× |
| 16 | Boundary/strata law (in-lane, upward, no host leak, seam repair) | ~5 | `boundaries` (full), `patlaw` item3, `strata`, `modernity`(6), `redteam`(D). Duplicates README `[02]` INTERFACE_SEAM + `boundaries.md`. |
| 17 | Prose quality / style guide / backtick-all / high-signal | ~3 | `PROSE` (full), `critique` "prose+comment hygiene" ptr, `redteam` ptr |
| 18 | Prove completeness by finding nothing / no churn | ~4 | `ADVERSARIAL` STANCE, `ADVERSARIAL` ILLUSORY, `EXTEND` JUSTIFIED, `redteam` |
| 19 | Comment hygiene | 1 full + 2 ptr | `COMMENTS` (full), `critique`/`redteam` ptr |
| 20 | Brief is binding scope (head + own section) | ~4 | `LAW` brief block, `READ_MANDATE`, `discover`, `implement`, `plan` |

Restatements 1, 8, 11, 13, 14, 15, 16 are the ones that DUPLICATE the doctrine README the agent already reads. Restatements 5, 6, 9, 10 are the ones re-derived in every task on top of a full statement in `PRE`.

## [03]-[TOKEN ESTIMATES]

Per block (cs path unless noted; ~1.4 tok/word for backtick-dense technical prose):

| Block | Lines | Tokens (cs) | Tokens (py) |
| :--- | :--- | :--: | :--: |
| `LAW` (no brief) | 440-460 | ~700 | ~720 |
| `ADVERSARIAL` | 461-483 | ~520 | ~520 |
| `L.ultra` | 81-110 / 206-246 | ~670 | ~980 |
| `EXTEND` | 484-508 | ~490 | ~490 |
| `L.mechanics` | — / 247-298 | 0 | ~1000 |
| `L.patlaw` | 112-132 | ~440 | ~470 |
| `L.boundaries` | 133-138 | ~180 | ~170 |
| `PROSE` | 509-518 | ~180 | ~180 |
| `COMMENTS` | 519-524 | ~170 | ~170 |
| **`PRE` total** | 525-526 | **~3350** | **~4700** |
| `READ_MANDATE` (+4-page map JSON) | 547-556 | ~640 | ~660 |
| `implement` task | 606-623 | ~460 | ~490 |
| `critique` task (incl. inline COLLAPSE_SCAN + owner/knob/aspects/rails/modernity) | 631-652 | ~1340 | ~1450 |
| `redteam` task (+crit JSON) | 653-680 | ~730 | ~780 |
| `discover` task | 578-605 | ~430 | ~450 |

Assembled stage prompt totals (cs):

| Stage | Composition | Tokens |
| :--- | :--- | :--: |
| discover | LAW + ADVERSARIAL + task | ~1650 |
| implement | PRE + READ_MANDATE + task | ~4450 |
| critique | PRE + READ_MANDATE + task | ~5330 |
| redteam | PRE + READ_MANDATE + task | ~4720 |

Per 40-page campaign (10 agents/stage): ~40 fresh-context agents each pay the fixed preamble. The preamble is the same for all 10 agents in a stage, so the doctrine-paraphrase cost is multiplied by agent count with zero marginal value over a single canonical read.

## [04]-[PER-BLOCK RULING]

| Block / sub-idea | Ruling | Justification |
| :--- | :--- | :--- |
| `L.readLaw` | READ-CHAIN (promote) | Already the seed of the reading chain. The py form is excellent (STEP 0 real `ls`, atlas order, "never a partial/skim/grep-jump/section-sample", on-disk-but-unlisted-is-still-law). Promote that rigor to cs+ts, add the `.api` two-tier `ls` act and the cs `domain/` shard nuance. Becomes block READ_FIRST. |
| `L.ultra` OPERATIVE-DOCTRINE (16 laws) | DELETE → READ-CHAIN | Verbatim-in-spirit copy of README `[02]-[DOCTRINE]`. The agent reads the source; a paraphrase is drift + a read-substitute. |
| `L.ultra` COLLAPSE MANDATE | MERGE → STANCE (one line) | The ">=3 parallel → one owner, same file" trigger is the README COLLAPSE_SCAN; keep only the engine floor-not-ceiling clause in STANCE. |
| `L.ultra` LIFECYCLE SPINE | DELETE → READ-CHAIN | Near-verbatim README BOUNDARY_ADMISSION. Keep the "parameterize ingress AND egress" clause once, in BUILD/READING chain pointer. |
| `L.ultra` STACK CAPABILITY / STACK .api (py) | MERGE → VERIFY | The both-tier `.api` law; collapse to one statement + the `ls` act in READ_FIRST. |
| `L.ultra` PRESERVE | MERGE → BUILD_LAW | One line: "preserve all capability; extension only deepens." |
| `L.mechanics` (py, 13 items) | KEEP-INLINE but COMPRESS to checklist | The mechanical-executability sweep is genuinely engine (fence-parses, model-coherence, loop-offload, etc.) AND cites owning doctrine sites. Compress each 1-3 line prose defect-definition to `name (owning-page site)`; the agent reads the site. ~1000 → ~200 tok. |
| `L.patlaw` pattern content | DELETE → READ-CHAIN | Duplicates README `[02]` (EXPRESSION_SPINE, POLICY_VALUES, SEMANTIC_NAMING, ONE_HOP_RESOLUTION) and `language.md`. |
| `L.patlaw` C#14 / py3.15 / TS feature enumeration | MERGE → one `modern` line + READ-CHAIN | `language.md` owns the feature surface. Keep only the version anchor (`L.modern`); the feature list is the doctrine page. |
| `L.boundaries` | DELETE → READ-CHAIN + WRITE_FULLY | Doctrine content duplicates README INTERFACE_SEAM + `boundaries.md`. The seam-repair engine rule (fix both ends, wire-frozen) moves to WRITE_FULLY, stated once. |
| `L.ownerChooser` (full discriminant→owner table) | MERGE → CRITIQUE task pointer + keep 5-discriminant mnemonic | The row-by-row owner mapping is `shapes.md`. Keep the compact 5-discriminant checklist (admission/identity/arity/timing/openness) as the engine audit trigger; route the mapping to the page. |
| `L.knob` | MERGE → CRITIQUE task (one line) | Compact engine check; keep the "delete param → does the value reconstruct it?" test, drop prose. |
| `L.aspects` | MERGE → CRITIQUE ptr | README `[02]` DEFINITION_TIME_ASPECTS + `rails-and-effects.md` own the two-weave taxonomy. |
| `L.rails` | MERGE → CRITIQUE ptr | `rails-and-effects.md` owns RAIL_CHOOSER. Keep the closed-fault + accumulate-vs-abort trigger by pointer. |
| `L.modernity` (6) | DELETE → folded into task pointers | Assembles strata+members+modern+doctrine-conformance already stated elsewhere; each becomes a named check. |
| `LAW` REMOVAL DISCIPLINE | KEEP-INLINE → BUILD_LAW | Engine law (not in doctrine): "underutilization is never removal evidence." Keep, tighten. |
| `LAW` WRITE-FULLY | KEEP-INLINE → WRITE_FULLY | Pure engine mechanics. Keep, merge with seam-repair. |
| `LAW` FUNDAMENTAL-REBUILD sentence | MERGE → per-task (implement owns "rebuild", critique/redteam own "audit") | The kind framing belongs in the stage task, not the shared preamble. |
| `ADVERSARIAL` (3 paragraphs) | KEEP-INLINE → STANCE, compress ~520→~330 | Engine stance + two naivety axes + illusory-target. Cut the 3rd-paragraph overlap; state each axis once. |
| `EXTEND` (4 items) | KEEP-INLINE → BUILD_LAW, compress ~490→~300 | Engine gap-source discipline (cite package/domain/consumer). Keep gap sources + owner-grammar; drop the coverage-over-size re-derivation (one clause). |
| `PROSE` + `COMMENTS` | MERGE → PROSE_AND_COMMENTS, ~350→~200 | Overlapping. One block: style-guide prose + backtick-all + comment hygiene + divider preservation. |
| `READ_MANDATE` | KEEP-INLINE → READING_MAP, trim | The per-page map contract is engine. Trim the `.api`-stacking restatement (now in VERIFY). |
| `discoverPrompt` re-inlining `[LAW, ADVERSARIAL]` | RESTRUCTURE | Compose the shared-block SUBSET each stage needs, not a hand-picked re-inline. Discover: no WRITE_FULLY, no PROSE/COMMENTS. |
| Per-task adversarial re-openers | DELETE | STANCE covers it once; each task re-derivation is pure waste. |

Blocks that are ENGINE and stay inline regardless: STANCE, the two naivety axes, REMOVAL/BUILD_LAW, WRITE_FULLY, the member-verification rail, the kind semantics, the reading-map contract, and the py mechanical-executability checklist. These carry law the doctrine does NOT own; the reading chain cannot replace them.

## [05]-[PROPOSED BLOCK ARCHITECTURE]

Shared blocks, authored once, composed per stage as a SUBSET (each stage picks what it needs — discover drops WRITE_FULLY + PROSE/COMMENTS; the delete executor takes only CONTEXT + boundary + task):

1. **CONTEXT** (~30 tok) — `Rasm monorepo, <corpus>. <strata>.` One line.
2. **READ_FIRST** (~380 tok) — the hard reading chain (doctrine README → all root pages in atlas order via real `ls`, cs `domain/` shard nuance; both `.api` tiers via `ls`; brief when present). Placed near-first: highest efficacy is the instruction that gates every downstream judgment. Ends with "this prompt does not restate the doctrine — read it at source; a summary is not a substitute."
3. **STANCE** (~330 tok) — merged adversarial: disbelieve + dense=prime-suspect + burden-on-code + reject good-enough + two naivety axes + illusory-primary-target + prove-by-finding-nothing + collapse-lists-are-a-floor.
4. **BUILD_LAW** (~300 tok) — buildout-over-removal + gap sources (cite one: package/domain/consumer) + owner-grammar landing + orthogonality + preserve-all + phantom/brief-only deletion.
5. **VERIFY** (~150 tok) — member-verification rail (`<verify>`, fallback), phantom=delete, both-tier `.api` maximization + universals-onto-folder.
6. **WRITE_FULLY** (~130 tok) — write now, fix-log is a report, cross-file ripple ownership both ends (surgical, wire-frozen, never foreign-interior), keep index docs truthful.
7. **PROSE_AND_COMMENTS** (~200 tok) — style-guide prose + backtick-all + comment hygiene + divider preservation.
8. **READING_MAP** (~500 tok, variable) — `READ_MANDATE`, trimmed: the Discover per-page map is a pointer you verify and EXCEED, not a ceiling.
9. **TASK** (per stage) — the stage action only, referencing shared checks by name.
   - discover: CONTEXT+READ_FIRST+STANCE+BUILD_LAW(framing)+VERIFY+map-contract; produce the map, zero write authority.
   - implement: full preamble + kind-aware author/rebuild/improve.
   - critique: full preamble + the audit-by-pointer task (below).
   - redteam: full preamble + six lenses + cold re-review by name.
   - delete: CONTEXT + boundary-repair + verify-then-delete.

Assembled targets (cs): discover ~1250, implement ~2050, critique ~2100, redteam ~2150. py: +~200 (mechanics checklist) per PRE-bearing stage.

## [06]-[DRAFT TEXT — LOAD-BEARING BLOCKS]

Neutral `<...>` placeholders map to the existing `L.*` interpolations.

### READ_FIRST (reading chain — replaces `readLaw` scatter + `ultra`-law-recitation + `ownerChooser`/`patlaw`/`boundaries`/`aspects`/`rails`/`modernity` doctrine restatements)

```
READ FIRST, IN ORDER, BEFORE EDITING — no fence is judged before this read lands.
(1) DOCTRINE — `docs/stacks/<lang>/README.md`, then EVERY root page it routes IN FULL, in the
    README [01]-[ATLAS] order. Enumerate the root set with a real `ls docs/stacks/<lang>/` (never
    from memory); a root page on disk but absent from the atlas is still mandatory law. Read each
    top-to-bottom — never a partial, skim, grep-jump, or section-sample.
    [cs] Then read the `domain/` shard(s) the page concern touches: the shards are enumerated in
    the README atlas — read the task-relevant ones truthfully, never guessed, never skipped;
    conformance to the shard is a hard gate.
(2) .API — both catalog tiers: `ls` the shared substrate `<api-root>/.api/` AND the folder
    `<package>/.api/`, then read every catalog relevant to these pages. Layer the shared/universal
    rails (<deepPkgs>) ON TOP OF the folder domain packages, never the folder set alone.
[brief] (3) BRIEF — `<BRIEF>`: the shared-law head plus every section covering these folders, in full.
This prompt does NOT restate the doctrine. The README [02]-[DOCTRINE] laws, the [03]-[COLLAPSE_SCAN]
table, the [05]-[PAGE_CRAFT] grammar, and [06]-[CORPUS_LAW] are the binding law — read them at the
source, hold them as fact, conform every fence to them. A summary is never a substitute for the read.
```

### STANCE (merged adversarial — replaces `ADVERSARIAL` + every per-task re-opener)

```
STANCE — every pass (author, critique, red-team) is hostile. Assume the fence is naive, shallow, or
illusory until it survives attack; the burden of proof is on the code, never on you. "Mature",
"already strong", "good enough", and a prior `clean` verdict are rejected self-assessments — most of
this corpus is <slur>. Dense, confident, package-fluent code is the PRIME suspect for hollowness, not
proof of depth: disbelieve every claim a fence makes about itself and verify it against the real
domain and the catalogued package surface. Reach a no-edit verdict only after a real attack finds
nothing; prove completeness by adding nothing, and never invent churn to look busy.
NAIVETY is a defect on two axes. COVERAGE — the owner models a thin slice of its concept: a 2-case
union for a 20-case domain, three fields where the concept carries fifteen. APPROACH — an enumerated
roster of styles, variants, or arms where one parameterized generator should GENERATE the space; demote
the roster to seed DATA over named parameters. Illusory code is the primary target — doctrine vocabulary
<vocab>, cited packages, confident prose, hollow body: a phantom (<illusion>), a name that promises
capability the body omits, decorative density carrying nothing, a stub dressed as a finished design.
Every collapse-signal list here is a FLOOR, never the complete set.
```

### BUILD_LAW (merged removal-discipline + capability-extension)

```
BUILD LAW — buildout over removal, always. Extend, deepen, and connect; never steer toward deletion.
An underutilized package, an orphan-looking admission, or a weak fence is an INTEGRATION target: the
capability lands as <ownerGrammar> — a case, row, field, operation, or policy value INSIDE the existing
owner, reshaped as if it had always carried it — or is wired into its owning sibling page in the same
pass. Never a parallel type, a new file, a sibling shape, or flat appended code; never extract a file
to cut LOC; never regress existing capability. The only deletion sanctioned absent a brief ruling is a
phantom (a cited member that does not exist); a brief-ruled removal is the other. Structural collapse
and CAPABILITY completeness are orthogonal — a fully collapsed owner can still model a naive slice.
Every extension cites exactly one gap source, never speculative. PACKAGE — a member the admitted
surface exposes that the concept admits but the page ignores (<gapPkg>; verify via <verify>). DOMAIN —
an attribute, metric, sub-kind, relationship, state, or operation the real concept demands (<gapDomain>).
CONSUMER — a contract a sibling or downstream owner will require. Byte-count is a weak proxy: assess each
owner against its full domain and both-tier package surface independently of size.
```

### CRITIQUE TASK (audit-by-pointer — replaces the inline COLLAPSE_SCAN + `ownerChooser`/`knob`/`aspects`/`rails`/`modernity` ~900-tok interpolation)

```
TASK — hostile doctrinal-conformance + capability audit; fix EACH page in place: <pages>. Audit every
fence against the doctrine you read, not a summary. Repair every hit now (a fix, never a ledger note);
a cross-file hit is yours per WRITE LAW.
- Run the README [03]-[COLLAPSE_SCAN] on every fence — any signal triggers the move, 3+ makes it
  mandatory — and hunt collapse targets past the listed rows.
- OWNER_CHOOSER (shapes.md): re-derive every shape from the 5 discriminants — admission, identity
  regime, variant arity, payload timing, openness — and replace any non-discriminant-correct owner;
  kill every parallel DTO, one-field wrapper, field-rename shape, and null/default ghost.
- KNOB_TEST: delete each parameter; where the value reconstructs it, collapse the knob to a policy
  value or input-shape discriminant; move every timeout/retry/deadline off the signature onto the
  carrier or a composition-time aspect.
- ASPECTS, RAILS + closed-fault discipline, STRATA/MEMBER conformance (<modern>; both .api tiers
  maximized; the <stack> file-organization + section-order law): audit each at its owning page.
- CAPABILITY-COMPLETENESS + ILLUSION: verify the body implements what names and prose promise; close
  any admitted capability the owner omits by growing it, citing the source; attack both naivety axes.
Return the batched fix-log (files = every file touched) + extended + summary.
```

## [07]-[STYLE-GUIDE VIOLATIONS]

The prompts largely honor the style guide's hedge ban (`PROSE` explicitly forbids may/might/probably/generally) — a genuine strength. Remaining issues, by the guide's own rules:

- Emphasis inflation. Pervasive ALL-CAPS (`HOSTILE`, `DISBELIEVE`, `NEVER`, `ALWAYS`, `PRIME`, `ULTRA-HARSH`, `MOST AGGRESSIVE`) flattens signal — when every clause shouts, none does. The guide bars bolding whole sentences; caps are the same emphasis inflation. Cut to the 2-3 caps per block that mark a true trigger. A model does not attack harder because the adjective is `MOST AGGRESSIVE`; it changes behavior from the concrete check.
- Run-on sentences. `LAW` REMOVAL DISCIPLINE, `discover` BUILDOUT FRAMING, and `redteam` (A) are single sentences of 90+ words chained with em-dashes and semicolons — violating "front-and-close paragraph shape, one controlling idea per paragraph." Split each into rule + exception.
- Redundant intensifiers that decide nothing: "genuinely aggressive attack", "ULTRA-HARSH, UNAGREEABLE auditor", "LAST and MOST AGGRESSIVE pass". Motivational, not behavior-changing — delete; keep the checklist that actually directs the attack.
- Soft hedge slip: `EXTEND` COVERAGE-OVER-SIZE "a SMALL page ... is almost always under-built" — "almost always" is a scope-free hedge the guide bans; state it as the rule ("assess against the domain, not size").
- Passive openers where active is available: several task blocks open "a no-edit verdict is reached ONLY after...", "cross-language wire seams are recorded as ALIGNED seams". The guide permits result-focused passive, but these name an actor — prefer "reach a no-edit verdict only after...".

## [08]-[BEFORE/AFTER TOKENS]

| Surface | Before (cs) | After (cs) | Before (py) | After (py) |
| :--- | :--: | :--: | :--: | :--: |
| PRE / shared preamble | ~3350 | ~1520 | ~4700 | ~1720 |
| critique task block | ~1340 | ~350 | ~1450 | ~380 |
| assembled discover | ~1650 | ~1250 | ~1700 | ~1350 |
| assembled implement | ~4450 | ~2050 | ~5800 | ~2400 |
| assembled critique | ~5330 | ~2100 | ~6700 | ~2450 |
| assembled redteam | ~4720 | ~2150 | ~6070 | ~2500 |

Per 40-page cs campaign (10 agents/stage), preamble+task savings ≈ (4450−2050 + 5330−2100 + 4720−2150 + 1650−1250) × 10 ≈ **~860k tokens**. py runs larger. The token cut is secondary to the two structural wins: one canonical source of law (kills drift between the prompt's paraphrase and the evolving doctrine), and a hard reading chain with a required `ls` act that makes the doctrine read actually happen instead of being pre-empted by a plausible in-prompt summary.
