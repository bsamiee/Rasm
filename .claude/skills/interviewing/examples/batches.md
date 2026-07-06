# [BATCH_CRAFT]

A round's question batch is the interview's payload; a batch that asks the abstract, courts agreement, surveys defaults, or hides a contradiction ships a confident wrong spec. Each entry names one batch defect and its fix under the fixed Detection / Rejected / Accepted / Reason / Reframe card; the Rejected and Accepted bodies are real batches, not descriptions of batches.

## [01]-[UNGROUNDED_ABSTRACTION]

A question asks the user to imagine a design in the air when the corpus already holds the surface the question is about.

- Detection: A round opens with an abstraction — a shape, a policy, a tradeoff — and cites no anchor, so the user answers from imagination instead of reacting to what exists.
- Rejected:
  ```markdown
  1. How should the graph carrier handle multiple backends?
     - a) One unified type
     - b) A separate type per backend
  ```
- Accepted:
  ```markdown
  1. `<unit>/planning/graph.md:55` types the carrier as `AnyGraph = "RxGraph | NxGraph | igraph.Graph"` — a string union no type-checker resolves, and the third arm is GPL. Which backend is the canonical owner the other two fold into?
     - a) `RxGraph` canonical, the rest as boundary adapters — drops the GPL surface from the core rail; costs the `igraph`-only algorithms until re-homed.
     - b) A real tagged union over all three — keeps every algorithm; costs a GPL-confined build lane and a resolvable tag per backend.
  ```
- Reason: The anchor turns imagination into reaction — the user confronts a named surface, a real weakness, a real string, and the answer carries a fact instead of a preference.
- Reframe: Run the corpus check first, embed the path and the observed fact in the question stem, and delete any question the confrontation set already answers.

## [02]-[APPROVAL_SEEKING]

A question invites the user to ratify the interviewer's framing instead of surfacing evidence, so agreement arrives wearing the mask of a ruling.

- Detection: A question a satisfied user answers "yes" to without adding a fact — a tag question, a "does that sound right", a mirror of the interviewer's own preference.
- Rejected:
  ```markdown
  1. The content-keyed scheduler looks like the right call, doesn't it?
  2. You're comfortable leaving impact search deferred, right?
  ```
- Accepted:
  ```markdown
  1. Walk me through the last time a consumer needed the scheduler: what did it request, what did the current design force it to do instead, and what did that cost?
  2. When a caller last reached for impact search, what happened — did the deferred owner serve it, or did the caller route around the gap?
  ```
- Reason: A mirrored question harvests compliance; the episode walk harvests what happened, and the fact the past supplies outranks the assent the framing invites.
- Reframe: Convert every ratification into an occurrence — the most recent time, the current workaround, what it cost — and re-anchor any agreement to a concrete instance before it becomes a ruling.

## [03]-[HYPOTHETICAL_FUTURE]

A question asks how the user hopes to handle an event that has not happened, harvesting optimism where the past holds a fact.

- Detection: A future-tense stem — "if X arrived", "how would you want" — that records aspiration, not evidence, and enters the record as a fact ruling.
- Rejected:
  ```markdown
  1. If a new component family arrived that didn't fit the closed ten, how would you want to handle it?
  ```
- Accepted:
  ```markdown
  1. `<unit>/architecture.md:79` closes `ComponentFamily` at ten. When did a family last press a slot the set refused — what was it, and what did the closed count force?
     - a) No family ever pressed the boundary — the closed count records as fact with its reopen condition.
     - b) A family was dropped or forced to fit — the count is a gap, not a design; name the missing slot.
  ```
- Reason: Past specifics outrank hypotheticals — the last occurrence yields what was tried, compared, and abandoned, where the future-tense answer yields only what the user hopes.
- Reframe: Anchor every generality to its most recent occurrence; when no answer exists, record the resulting ruling as an assumption carrying its invalidating condition, never as a fact.

## [04]-[STATEMENT_AS_QUESTION]

A question offers one sane answer, spending a round to extract a foregone conclusion instead of a real fork.

- Detection: A stem whose options do not lead to materially different work — strict typing over `Any`, a working seam over a broken one — where one branch is house law.
- Rejected:
  ```markdown
  1. Should the design use strict typing instead of `Any`?
  2. Do we want to avoid shipping a broken seam?
  ```
- Accepted:
  ```markdown
  [CONSTRAINT] Strict typing over `Any` is house law — recorded, not asked.

  1. The strict-typing law forces the carrier off the string union. Two typings survive it: a `Protocol` over the three backends, or a tagged union with a resolver. Which?
     - a) `Protocol` — structural, no tag; costs the backend-specific algorithms that need the concrete type.
     - b) Tagged union — explicit dispatch; costs a resolver per operation.
  ```
- Reason: A question with one sane answer is a statement wearing a question mark; recording it as a constraint frees the round to ask the fork the foregone conclusion opens.
- Reframe: State the settled branch as a constraint in the record, then spend the round on the real tradeoff the constraint exposes — two options changing the work differently.

## [05]-[SURVEY_BATCH]

A batch grows past the point where every option is a real tradeoff, so the user picks defaults down a checklist instead of ruling.

- Detection: A round with more than a handful of questions, each a menu of parameters the domain default already settles, forcing the user to rubber-stamp.
- Rejected:
  ```markdown
  1. What log format? a) JSON b) logfmt c) plain
  2. What retention window? a) 7d b) 30d c) 90d
  3. What serializer? a) msgspec b) orjson c) stdlib
  4. What test runner? a) built-in b) external
  5. What lint profile? a) strict b) default
  6. What cache backend? a) memory b) disk
  7. What CI trigger? a) push b) tag
  8. What doc format? a) markdown b) rst
  ```
- Accepted:
  ```markdown
  1. The acquisition surface carries three entrypoints — `read`, `survey`, `acquire` — over one stated aspect. Collapse to one polymorphic entry discriminating on input shape, or keep three named arms?
     - a) One entry — object-store inventory and presign become input shapes; costs a wider input discriminant.
     - b) Three arms — each name self-documents; costs the entry proliferation the collapse law forbids.
  ```
- Reason: A survey-sized batch makes the user pick defaults and buries the one load-bearing question in noise; a narrow batch keeps every option a real tradeoff the user must rule on.
- Reframe: Default every nonessential gap and name the default in the record; keep the batch narrow enough that each option changes the work, and rank the survivors by the decisions their answers invalidate.

## [06]-[MENU_NOT_PROBE]

Options list forms without their costs, so the user chooses by preference instead of by consequence.

- Detection: A question whose options are bare labels — no cost, no consequence, no anchor — reducing the choice to taste.
- Rejected:
  ```markdown
  1. How should the viewer load geometry?
     - a) wasm decoder
     - b) server-side tessellation
     - c) native codec
  ```
- Accepted:
  ```markdown
  1. `<unit>/architecture.md:54` marks the viewer codec-absent until an upstream wasm decoder identity is admitted. Which decode owner does the viewer bind?
     - a) Upstream wasm decoder — keeps the viewer thin; costs a hard dependency on an unadmitted serving row that blocks render until it lands.
     - b) Server-side tessellation to a mesh wire — unblocks render now; costs a round-trip per view and a server the viewer did not need.
  ```
- Reason: Options are probes, not menus — each names its cost and cites its anchor, so the answer reveals which consequence the user accepts, and the lead marker points only where one option dominates.
- Reframe: Attach a cost or foreclosure to every option, cite the confrontation-set anchor where one exists, and let free-text remain the escape valve, never the plan.

## [07]-[DEPENDENT_CASCADE]

A batch stacks questions whose later members are meaningless until the first answers, wasting every dependent the gating answer invalidates.

- Detection: Questions 2 through N presuppose a specific answer to question 1, so a different answer to 1 discards the rest of the batch.
- Rejected:
  ```markdown
  1. Should the graph carrier be a single owner or split per backend?
  2. For the split, what are the three backend type names?
  3. For those types, which shares the traversal API?
  4. Given that API, where does the GPL backend get isolated?
  ```
- Accepted:
  ```markdown
  1. The carrier owns three backends behind a string union. One decision gates the rest: is the carrier one polymorphic owner, or one owner per backend?
     - a) One owner — the backend becomes a policy value; the next round designs the discriminant and the GPL confinement.
     - b) Per backend — each is its own surface; the next round designs the shared traversal contract.
  ```
- Reason: A cascade of dependents signals the round runs one question deep; batching them spends four slots to learn one answer and re-asks three of them whichever way the gate falls.
- Reframe: Ask the gating question alone, hold its dependents for the next round, and batch only questions that stand independent of each other's answers.

## [08]-[SILENT_RECONCILIATION]

Two rulings collide and the interviewer picks one quietly, burying the conflict the user alone owns the authority to resolve.

- Detection: A round records a ruling that contradicts an earlier one — or an index claim its own pages deny — without ever surfacing the collision as a question.
- Rejected:
  ```markdown
  1. Since the README already calls impact a standing domain owner, I'll record impact search as in-scope and move on.
  ```
- Accepted:
  ```markdown
  1. Two rulings collide: `<unit>/README.md:3` presents impact as a standing domain owner, while `<unit>/planning/impact.md:15` gates impact search and wide sweeps behind an unnamed future consumer. Both cannot hold. Which is law?
     - a) Owner is law — pull search and sweeps into current scope; costs the build now, closes the claim-to-carriage gap.
     - b) Deferral is law — demote the README claim to "impact interchange owner"; costs the broad promise, keeps scope honest.
  ```
- Reason: A silently reconciled conflict hard-codes the interviewer's guess as the user's ruling; surfacing it with each resolution's consequence hands the authority back where it belongs.
- Reframe: Give every contradiction its own question — the two rulings, the surfacing observation, and the consequence of each resolution — and record the outcome as a conflict entry, never as a quiet overwrite.
