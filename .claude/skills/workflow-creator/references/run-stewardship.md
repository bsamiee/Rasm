# [RUN_STEWARDSHIP]

Launching a durable workflow binds the launching agent to its stewardship: the run is observed, its artifacts graded, its lanes' behavior mined, and every finding lands as a durable improvement at the owning surface. A run that merely completes teaches nothing; the observation system is how a workflow earns its next, stronger version.

## [01]-[INSTRUMENTS]

Three instruments cover a run; each answers a question the others cannot.

[WATCHER]: a persistent monitor over the run's scratch directory and journal answering "where is the run and is it healthy". Wake taxonomy: phase-bucket first artifacts, per-lane stage transitions, failure signatures, stall windows, drain rounds, completion — never per-artifact pings. The watcher is a DISK SCRIPT run through explicit `bash`, shellcheck-clean, with marker-file state so a restart never replays old signals, and a one-shot test mode proven before arming; an inline eval'd monitor dies on the first rarely-taken branch and its death is silent. Silence is never health: the filter matches every terminal state — a watcher that only recognizes the happy path reads a crashed run as a running one. `scripts/watch-run.sh` is the owner; arm it with the run's scratch dir, journal, and a bucket spec.

[FORENSICS]: post-stage transcript analysis answering "how did the lane behave". Delegated, sampled, read-only; transcripts are large JSONL projected with `jq`/`grep`, never read whole. The turn census partitions every tool call: own-pass authoring, provided-artifact reads, targeted verification at anchors, authoring, re-derivation of provided facts, cold exploration, format and retry churn. Two derived ledgers carry the value: the UPSTREAM-GAP ledger — every cold lookup names what the lane needed and the upstream artifact obligated to carry it — and the RE-DERIVATION ledger — every re-derived fact names the artifact section the lane did not trust. Label joins ride the deterministic artifact grammar: a lane's transcript is found by grepping its embedded product path in the first message, and multiple lanes embed the same path (a writer's product rides its reviewers' prompts), so the join disambiguates by the first message's TASK line and the transcript's tool-call density before trusting it.

[GRADING]: adversarial artifact review answering "is the product fit for its consumer". Per artifact TYPE against the producing prompt as the contract, through the five-part battery: anchor audit (open a sample of anchors on current disk), consumer simulation (work the artifact as its downstream consumer and time the path to a decision), signal/bloat ledger, structure alternatives, enforcement placement. Graders are adversarial, read-only, report to run scratch, and return short summaries; a tournament dispatches one grader per completed lane and dedupes findings across returns before anything lands.

The verdict for an agent TYPE is three-railed, never one: artifact grade, behavior forensics, and DOWNSTREAM-CONSUMPTION PROOF — grep the consumers' transcripts for actual reads of the artifact (paths opened, index-row jumps, row IDs cited in fixlogs). An artifact nobody downstream opens is churn regardless of its grade; a downstream lane's re-derivation turns are hard evidence of a weak upstream artifact. The cross-check closes the loop: upstream artifact quality is MEASURED at the consumer, never asserted by the producer.

## [02]-[ARTIFACT_LAW]

What grading enforces — the creation laws a lane's product answers to:
- [ANCHORED_ENTRY]: every fact carries a typed `path:line` anchor in inline form; a bare symbol is not an anchor, and one canonical inline exemplar in the prompt kills format literalization at source.
- [ABSENCE_RANGE]: an absence claim states the range actually read; a phantom born of a partial read is the producer's defect, not the consumer's.
- [RAIL_HONESTY]: a verification claim names the rail actually run; a rail unavailable in the lane's sandbox degrades the claim to its true grade (catalog-anchored, prose-confirmed), never to "verified".
- [EARNED_EMPTY]: every section admitting emptiness carries the checks that earned it; a silent empty is a lazy miss.
- [DISPOSITION_BY_ID]: a producer mints a stable key on every claim row at creation, and a consumer obligated to land upstream rows dispositions each row by that key — landed, refuted with the disk fact, or deferred to its named owner — so silence is impossible; a disposition law whose upstream mints no keys degrades to topic-prose and is a producer schema defect, never consumer laxity.
- [LEAD_LINE]: judgment-tier entries open with a typed triage lead (owner, ground, anchors, size, counterpart state) so the consumer decides at a glance; the WHY stays as prose behind it.
- [FACT_LADDER]: mapping lanes emit facts, ideation lanes emit judgments, writer lanes emit changes; the micro entry form unifies across tiers, but a judgment field never moves UP the ladder — a map artifact carrying triage verdicts pre-judges and anchors every consumer.
- [SWEEP_DISCIPLINE]: a cross-cutting section holds only claims true across the whole scope; an entry anchored at a specific site lives in that site's section, so a consumer's scoped read is COMPLETE for its slice; a cross-site entry is ONE row listing every site, never a copy per section.
- [CHANNEL_NAMING]: content lives on disk, the wire carries indexes — and the consuming prompt names the DISK artifact as the channel explicitly; "the reports" ambiguity sends consumers to an index that does not carry the content.
- [ENGINE_DATA]: a roster, census, or path set the orchestrator already holds is embedded in the prompt; a lane re-deriving engine-held data with tree and glob calls is an engine defect, not lane misbehavior.
- [NO_PROCESS]: no self-verification narration, no self-counts in headers, no session or process ledger inside a product artifact.

And the consumption laws a lane's behavior answers to: own-pass first where the stage is a writer or reviewer (the blind pass is authored before any provided navigation is read — the anti-anchoring dividend is real and measured); navigation by index jump, never a linear walk of every artifact; verification at the anchor with one targeted read, never a whole-file re-read of quoted material; declines and prior claims consumed as refutation targets, never settled verdicts.

## [03]-[IMPROVEMENT_LAW]

Findings become improvements under a placement discipline; "the agent should try harder" is never a finding.

- [PLACEMENT]: every defect lands at a named surface — a schema field, a prompt law sentence, a prompt exemplar, engine data, or register variance — chosen by where enforcement is mechanical rather than hortatory. A format defect wants an exemplar; a completeness defect wants a schema field or an earned-empty law; a behavior defect wants a prompt law or the upstream artifact that removes the behavior's cause.
- [ROOT_FIX]: a claimed capability or timer is verified against official documentation and live reproduction before any guidance rides it; constants and comments that rationalize behavior that does not occur are noise to collapse, never defensive layers to keep.
- [WORKING_COPY]: a workflow with a resumable live run is byte-frozen; improvements accrete in a byte-exact working copy the stewarding agent edits personally, validated on every edit, and fold back only when the run is no longer resumable-relevant.
- [BATCH]: improvements land batched per proven pattern, never as mini-update spam; each batch re-validates the file and re-runs the dry-run gate.
- [FLEET]: a proven change-set propagates through scoped aligners — one per target workflow, bounded concurrency, each loading this skill, diffing the reference pair, reading its target in full, applying at the target's own grain with principled skips, and validating; aligners never touch the reference pair or any sibling. Every barrier they meet is audited, merged only when provably illusory — a genuine data edge stays.
- [ALTITUDE]: each finding routes to the surface that owns it — the workflow file for lane-local law, this skill for orchestration-general law, the external-lane skill for external-model law, the method corpus for campaign roles, the machine owner for environment facts — stated once at its owner, composed by pointer everywhere else.

## [04]-[EXTERNAL_LANES]

External-model lanes get the deeper look; the codex skill owns their general law, and the external-lanes reference owns the in-workflow overlay. Stewardship facts that bind observation:
- A wrapper's transcript holds the external call's result only after the call returns; a live wrapper transcript ending at the dispatch is a lane in flight, not an observability hole. The external side's actions live in its own session store, joined by the thread identifier the receipt carries.
- Effort pins are explicit: the operator default is read from its config owner, never assumed — an unpinned lane inherits whatever the default is today, and a lane that needs a tier states it.
- Taming is measured, never vibed: premature stop is checked against the product's coverage of its contract; circular churn is exec-command count against unique files touched, repeated identical queries, and wall time against the peer median.
- Stall detection never observes a live blocking call; wall-clock watchdogs are the binding bound on a wedged external call, and the watchdog's dead-lane receipt frees the slot while the session store keeps the thread recoverable.
