# [DESIGN_DOCUMENT_STANDARDS]

A design document is a collaborative pre-implementation proposal for a change with real design ambiguity. It frames the pressure, compares viable options, records trade-offs, gathers owner feedback, splits the selected direction into reviewable slices, and states the validation plan before durable decision or implementation work lands. It owns proposal and review history; it does not own the accepted decision, current architecture, milestone sequence, operational response, or generated contract truth.

The entry gate is ambiguity: write a design document only when at least two plausible approaches, cross-boundary consequences, or unresolved trade-offs need review. Owner consensus, review slices, Last Call, and proof planning raise the review profile; they do not justify a design document by themselves.

## [1][USE_WHEN]

Use a design document when the change has design ambiguity and needs any of these before code lands:

- pre-code consensus across two or more owners or boundaries;
- comparison of viable options with recorded trade-offs;
- a change split into independently reviewable, revertible slices;
- a bounded final-objection window after discussion converges;
- a proof plan that names commands, contracts, checks, reviews, or risks before merge or release.

Do not use a design document for one obvious approach with no meaningful trade-off. Route accepted durable decisions to ADRs, current structure to architecture documents, dated build sequence to roadmaps, operational symptom response to runbooks, lookup catalogs to reference, generated contracts to API documentation, and contributor workflow to contributing guides.

## [2][SOURCE_ORDER]

This standard derives its shape from collaborative software design-doc practice and RFC-style final review, but it applies them as local documentation rules. Google's design-doc discussion supports collaborative pre-code review, goals, alternatives, trade-offs, cross-cutting concerns, and archival value. RFC 2026 supports open review, consensus, and Last Call as a final-objection pattern; local Last Call borrows only the bounded final-review pattern and does not claim to run the IETF process.

Source of truth: [Software Engineering at Google, Design Docs](https://abseil.io/resources/swe-book/html/ch10.html), [RFC 2026 Internet Standards Process](https://www.rfc-editor.org/rfc/rfc2026), [RFC 2119](https://www.rfc-editor.org/rfc/rfc2119.html), and [RFC 8174](https://www.rfc-editor.org/rfc/rfc8174.html).
Last verified: 2026-06-04
Review trigger: design-doc practice, RFC Last Call process, or BCP 14 keyword guidance changes.

## [3][MODAL_LANGUAGE]

Use lowercase `must`, `should`, and `may` with the local craft standard's ordinary prose meanings: requirement, recommendation, and permission. Do not use bracketed all-caps tags such as `[MUST]`, `[SHOULD]`, or `[ALWAYS]` in a design document. RFC 2119 and RFC 8174 normative keywords apply only when a document explicitly invokes them with uppercase keyword semantics; this corpus does not use that notation for design docs.

## [4][PROFILES]

Pick one profile from blast radius. The profile raises review obligations; it never removes the ambiguity gate.

| [INDEX] | [PROFILE]        | [TRIGGER]                 | [REVIEW]            | [LAST_CALL]            | [ADR_HANDOFF] |
| :-----: | :--------------- | :------------------------ | :------------------ | :--------------------- | :------------ |
|   [1]   | Lightweight      | one owner or package      | owning reviewer     | optional               | no            |
|   [2]   | Standard         | 2+ owners or packages     | each affected owner | local Last Call        | if policy     |
|   [3]   | Public-contract  | runtime, contract, public | owners plus driver  | audience + channel     | yes           |

`If policy` means acceptance binds durable architecture policy. `Public-contract` means the change has enough public, runtime, or contract blast radius that every concern gets an accountable owner or a stated `n/a` reason.

Local Last Call is a review state inside this document type, not a separate RFC artifact and not the IETF process. It must define audience, channel, deadline, accountable approvals, objection handling, and final disposition. Fork a parallel RFC only when an external hosting or governance process demands it; when that happens, name the process and link the canonical copy.

## [5][AUTHORITY]

Source order decides a wording or scope question when sources disagree:

1. Current repository source, manifests, generated contracts, and accepted decisions the proposal must respect.
2. This design-document standard for proposal shape, lifecycle, review slices, and Last Call.
3. The five shared standards for position, form, craft, evidence, and notation.
4. External design-doc or RFC practice for background only.

A design document proposes; it never overrides an accepted decision. When a slice contradicts an accepted ADR, the design must either narrow scope or plan the ADR supersession handoff.

## [6][OPENING_METADATA]

Every design document opens with one definition block, one `label: value` per line. Copy only the always-required fields first; add conditional metadata only when its trigger holds.

```markdown template
Status: Draft | Discussion | Last Call | Accepted | Implemented | Abandoned
Profile: Lightweight | Standard | Public-contract
Date: YYYY-MM-DD
Authors: <names or owner roles>
```

Conditional metadata additions:

```markdown template
Reviewers: <consulted owners or review groups>
Last Call deadline: YYYY-MM-DD
Last Call channel: <notification channel or review surface>
Supersedes design: <path to earlier design>
Implemented evidence: <PR, ADR, roadmap milestone, release note, or validation receipt>
```

Metadata cardinality:

- `Status`, `Profile`, `Date`, and `Authors` are required.
- `Reviewers` is required at `Discussion` and later.
- `Last Call deadline` and `Last Call channel` are required at `Last Call` and later for `Standard` and `Public-contract`.
- `Supersedes design` appears only when this proposal replaces an earlier design.
- `Implemented evidence` is required only at `Implemented`; it links the owning implementation, ADR, roadmap, release, or validation receipt.

`Status` and `Profile` are discriminants: an agent reads them to route lifecycle obligations and conditional sections. Keep both to one value from the closed set.

## [7][REQUIRED_STRUCTURE]

Use the `Draft` skeleton only while the problem is still being shaped. Use the `Discussion and later` skeleton when reviewers must evaluate the proposal. Conditional sections are omitted until their trigger holds; when they appear, insert them at the named position and renumber headings in document order.

Draft skeleton:

```markdown template
# [CHANGE_NAMED_OUTCOME]

<Opening metadata.>

## [1][PROBLEM]

## [2][ROUGH_GOALS]

## [3][GAPS]

## [4][BOUNDARIES]

## [5][REVIEW_CHECKLIST]
```

Discussion and later skeleton:

```markdown template
# [CHANGE_NAMED_OUTCOME]

<Opening metadata.>

## [1][PROBLEM]

## [2][GOALS]

## [3][NON_GOALS]

## [4][CONTEXT]

## [5][PROPOSED_APPROACH]

## [6][ALTERNATIVES_CONSIDERED]

## [7][REVIEW_SLICES]

## [8][RISKS_OPEN_QUESTIONS]

## [9][VALIDATION_PROOF_PLAN]

## [10][BOUNDARIES]

## [11][REVIEW_CHECKLIST]
```

Conditional additions:

```markdown template
## [N][CROSS_CUTTING_IMPLICATIONS]

<Insert after `Review slices` for `Standard` and `Public-contract`.>

## [N][LAST_CALL_RECORD]

<Insert after `Risks and open questions` at `Last Call` and later.>

## [N][DECISION_RECORD_HANDOFF]

<Insert after `Validation and proof plan` when acceptance binds durable policy or supersedes an ADR.>
```

Lifecycle/profile decision table:

| [INDEX] | [CONDITION]                              | [REQUIRED_OUTPUT]                                           |
| :-----: | :--------------------------------------- | :---------------------------------------------------------- |
|   [1]   | `Status: Draft`                          | draft skeleton; visible gap note                            |
|   [2]   | `Status: Discussion` or later            | discussion skeleton; reviewers metadata                     |
|   [3]   | `Profile: Standard` or `Public-contract` | cross-cutting implications                                  |
|   [4]   | `Status: Last Call` or later             | Last Call record, deadline, channel, approval disposition   |
|   [5]   | acceptance binds durable policy          | decision-record handoff                                     |
|   [6]   | `Status: Implemented`                    | implemented evidence linking the owning implementation path |

Section cardinality:

- Opening metadata, `Boundaries`, and `Review checklist` are required for every status.
- `Problem`, `Rough goals`, and `Gaps` are enough for `Draft`.
- `Goals`, `Non-goals`, `Context`, `Proposed approach`, `Alternatives considered`, `Risks and open questions`, and `Validation and proof plan` are required at `Discussion` and later.
- `Review slices` is required for `Standard` and `Public-contract`; it is optional for `Lightweight`.
- Conditional sections appear only when their trigger row applies.

## [8][SECTION_RULES]

State each section's controlling content first and its boundary last. Where a section names a finite set of trackable items, render that set as the mandated structure.

- `Problem`: name the specific user, product, operational, or engineering pressure and who feels it. One controlling pressure per paragraph.
- `Goals`: write each goal as a checklist item with an observable metric, threshold, or pass/fail signal.
- `Non-goals`: name tempting scopes the proposal declines and the reason each is out of scope.
- `Context`: link current source paths, prior accepted decisions, issues, and standards as live links. Drift-prone context claims with source, API, tool, or contract truth carry nearby `Source of truth:`, `Evidence:`, `Last verified:`, or `Review trigger:` fields.
- `Proposed approach`: lead with the selected shape and close with the constraint a reviewer must accept to approve.
- `Alternatives considered`: record the chosen option, strongest rejected option, and do-nothing baseline unless inaction was impossible. Every option states its deciding trade-off.
- `Review slices`: define self-contained, ordered, revertible changes. Each slice has one reviewer focus and a rollback boundary.
- `Cross-cutting implications`: cover security, privacy, accessibility, internationalization, data, operational, compatibility, and runtime concerns; mark non-applicable concerns `n/a` with a reason.
- `Risks and open questions`: render one record per item, each with an owner and disposition from `open | assigned | deferred (owner) | accepted-as-risk | resolved`.
- `Last Call record`: summarize the selected direction, deadline, notification channel, accountable approvals, open objections, and final disposition.
- `Validation and proof plan`: name exact commands, contracts, runtime checks, review gates, and acceptance criteria. Mark a gate `enforced` only when a command or status check runs it; otherwise mark it `review-only`.

## [9][GOALS_CHECKLIST]

Render `Goals` as a checklist of measurable conditions. A bare prose goal with no pass condition is the primary low-value failure mode.

```markdown template
## [2][GOALS]

- [ ] Cold profile-view P95 under 1 s - proven by `<exact benchmark command or status check>`.
- [ ] Zero write-amplification regression - proven by `<contract diff or storage check>`.
- [ ] Rollback in one slice - proven by `<rollback boundary or feature flag check>`.
```

Each item pairs outcome with the metric, threshold, or signal that proves it. Carry the same scope boundary into `Non-goals`: a declined scope is a plausible candidate the reader might expect.

## [10][ALTERNATIVES_CONSIDERED]

Use a comparison table when two or more options survive triage. The baseline row is mandatory when inaction was plausible.

| [INDEX] | [OPTION]            | [GOOD]           | [NEUTRAL] | [BAD]           | [VERDICT]              |
| :-----: | :------------------ | :--------------- | :-------- | :-------------- | :--------------------- |
|   [1]   | Sharded writers     | Meets target     | rebalance | shard-loss mode | Selected: throughput   |
|   [2]   | Single-writer queue | Simple ownership | one core  | caps throughput | Rejected: throughput   |
|   [3]   | Do nothing          | No new code      | none      | misses pressure | Rejected: cost remains |

When only one option survived and the trade-off is asymmetric, a `Lightweight` design may fold the section into labeled prose under `Proposed approach`; the deciding trade-off and baseline still appear.

The rejected shape below records options without the deciding trade-off:

```markdown rejected
## [6][ALTERNATIVES_CONSIDERED]

- We looked at a single-writer queue and a sharded design.
- The sharded one seemed better.
```

## [11][REVIEW_SLICES]

A review slice is one self-contained change that a reviewer can understand, validate, and revert without the rest of the proposal landing first. Do not copy a fixed sequence; create only the slices the design actually needs.

| [INDEX] | [SLICE]             | [KIND]                   | [DEPENDS]            | [REVIEWER_FOCUS]          | [ROLLBACK_BOUNDARY]       |
| :-----: | :------------------ | :----------------------- | :------------------- | :------------------------ | :------------------------ |
|   [1]   | SLICE_NAME          | CHANGE_KIND              | none                 | SINGLE_REVIEW_CONCERN     | REVERT_OR_DISABLE_PATH    |
|   [2]   | NEXT_SLICE_NAME     | CONTRACT_OR_BEHAVIOR     | PRIOR_SLICE_NAME     | SINGLE_REVIEW_CONCERN     | ROLLBACK_BOUNDARY         |

Slice kinds are local labels, not a closed global sequence. Keep dependency order honest and leave no blank rollback boundary. When slices become dated milestones with exit gates, move the sequence to the roadmap owner.

The rejected shape below invites fixed-sequence copying:

```markdown rejected
| [INDEX] | [SLICE] | [KIND]         | [DEPENDS] |
| :-----: | :------ | :------------- | :-------- |
|   [1]   | S1      | refactor       | none      |
|   [2]   | S2      | implementation | S1        |
|   [3]   | S3      | tests          | S2        |
```

## [12][TRACKABLE_RECORDS]

`Risks and open questions` carries one record per item. A live item is `open`, `assigned`, or `deferred (owner)`; a settled item is `resolved` or `accepted-as-risk`.

```markdown template
### [N.M][SHARD_REBALANCE]

Owner: runtime-maintainers
Disposition: accepted-as-risk
Tracking: issues/482

### [N.M][WRITER_FANOUT]

Owner: storage-owner
Disposition: resolved
Tracking: issues/487 - benchmark settled per-key contention.
```

`Validation and proof plan` carries one row per gate, with an enforcement flag that separates a real gate from review intent:

| [INDEX] | [GATE]              | [COMMAND_CONTRACT]        | [ACCEPTANCE_SIGNAL] | [ENFORCEMENT] |
| :-----: | :------------------ | :------------------------ | :------------------ | :------------ |
|   [1]   | Unit laws           | EXACT_TEST_COMMAND        | suite green         | enforced      |
|   [2]   | Storage contract    | SCHEMA_CONTRACT_PATH diff | no breaking change  | enforced      |
|   [3]   | Owner design review | none                      | two owner approvals | review-only   |

At `Accepted` and `Implemented`, add proof receipt fields beside completed gates rather than rewriting planned gates as if they already ran:

```markdown template
Gate: <gate name>
Owner: <owner role>
When run: <status check, PR, local command, or not run>
Proof receipt: <artifact, link, or proof gap>
Last verified: YYYY-MM-DD
```

`Last Call record` carries sign-off readiness:

| [INDEX] | [OWNER]             | [APPROVAL] |     [DATE] |
| :-----: | :------------------ | :--------- | ---------: |
|   [1]   | runtime-maintainers | approved   | 2026-06-01 |
|   [2]   | storage-owner       | pending    |       none |

The design is ready to accept only when every accountable owner reads `approved`, every objection reads `resolved` or `accepted-as-risk`, and no live risk remains `open`.

## [13][LIFECYCLE]

Statuses advance in one direction except for a documented return to `Discussion`. Each transition has an observable entry condition.

```mermaid conceptual
stateDiagram-v2
    [*] --> Draft
    Draft --> Discussion: problem, goals, and viable options drafted
    Discussion --> LastCall: one direction selected, no open blocking objection
    LastCall --> Discussion: new evidence changes selected direction
    LastCall --> Accepted: approvals complete; objections resolved or accepted
    Accepted --> Implemented: owning implementation and proof links recorded
    Draft --> Abandoned: proposal dropped, reason recorded
    Discussion --> Abandoned: proposal dropped, reason recorded
    Accepted --> [*]
    Implemented --> [*]
    Abandoned --> [*]
```

Enter `Last Call` only when a reviewer can evaluate the final direction without rediscovering the discussion. If evidence changes the selected direction after `Last Call` opens, return to `Discussion`, update the trade-off summary, and open a new `Last Call`.

`Implemented` is a handoff status, not implementation ownership. It records that the owning implementation, ADR, roadmap milestone, release note, or validation receipt is linked; ongoing implementation state belongs to those owners.

## [14][DECISION_RECORD_HANDOFF]

After acceptance, create an ADR when the design binds two or more owners, packages, runtime boundaries, or durable contracts, or when it supersedes a prior durable decision. The design names the target ADR path and the accepted direction; the ADR standard owns derivation mechanics and confirmation evidence.

## [15][BOUNDARIES]

- [adr.md](adr.md) owns the accepted durable decision and its confirmation evidence after this proposal is approved.
- [roadmap.md](roadmap.md) owns build sequence, milestones, and exit proof when slices grow into a dated plan.
- [architecture.md](architecture.md) owns current structure and invariants the proposal must respect.
- [README.md](../README.md) routes document-type choice, placement, and lifecycle questions.

## [16][REVIEW_CHECKLIST]

- [ ] The document has real ambiguity: at least two plausible approaches, cross-boundary consequences, or unresolved trade-offs.
- [ ] `Status` and `Profile` are single closed-set values, and profile obligations are met.
- [ ] Opening metadata cardinality holds; conditional fields appear only when their trigger holds.
- [ ] A `Draft` uses the draft skeleton, and `Discussion` or later uses the full proposal skeleton.
- [ ] Conditional sections appear only when their trigger holds.
- [ ] `Problem` names a specific pressure and who feels it.
- [ ] Each goal is a checklist item naming a metric, threshold, or pass condition.
- [ ] Each non-goal is a declined candidate scope, not a restated failure mode.
- [ ] `Context` links live source paths, decisions, issues, and standards, with freshness fields for drift-prone claims.
- [ ] Reviewers or consulted owners are listed at `Discussion` and later.
- [ ] Each alternative records its deciding trade-off, and a do-nothing baseline appears when plausible.
- [ ] Alternatives use table form when two or more options survive.
- [ ] Review slices are self-contained, ordered, and carry a non-blank rollback boundary.
- [ ] Cross-cutting concerns are covered at `Standard` and `Public-contract`, with each concern owned or marked `n/a` with a reason.
- [ ] Each risk carries an owner and disposition, and no acceptance-ready record remains `open`.
- [ ] The proof plan names exact commands, contracts, gates, and acceptance criteria, marks review-only gates as unenforced, and adds proof receipts only when gates have run or landed.
- [ ] `Last Call` records audience, channel, deadline, every accountable owner's approval, and each objection's disposition.
- [ ] `Implemented` links the owning implementation, ADR, roadmap, release, or validation receipt rather than owning implementation state.
- [ ] The decision-record handoff names the target ADR when acceptance binds durable policy.
