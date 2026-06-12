# [BEDROCK] validation / seam-ownership

## seam-matrix

- One ownership matrix assigns every external-input seam exactly one validator owner, one carrier policy, and one trigger; a seam absent from the matrix is an unguarded ingress, and a validator no matrix row claims is dead admission code.
- The row schema is total — owner, carrier, trigger, gating-severity usage, override policy — and every row fills every column; a blank column is the shortcut detector (see rejected-shortcuts).
- The matrix is declared once as policy rows at the composition root and consumed by the root build — rows are data, so seam governance is recoverable from the declaration alone.
- The matrix driver is row-generic: rows run heterogeneously through the untyped validator contract with the payload type selecting the row — per-seam driver code is the rejected form.
- Triggers are a closed three-member set — at-boot, per-message, per-batch; per-read and per-render triggers are rejected (they are validate-on-read wearing a trigger's name), so trigger choice is a total dispatch, not a free text column.
- Row: configuration/options — owner: one options-shape validator adapted into the options-validation contract; carrier: accumulate (all binding faults at once); trigger: once at host start, never per read.
- Row: wire ingress (request/message/event payloads) — owner: one boundary validator per wire shape; carrier: accumulate across independent fields, abort across dependent steps within the validator via cascade policy; trigger: per message, exactly once, before any typed construction.
- Row: command-line arguments — owner: the parser's own grammar admits syntax, then the bound shape flows through the options row; argument validation never forks into a third mechanism beside parse and options.
- Row: interactive field input — owner: the same wire-shape validator run under property selection for per-field feedback; carrier: accumulate with severity tiers; trigger: per edit burst, with the full-shape run repeated once at submit.
- Row: bulk import / record streams — owner: the element-shape validator driven through collection rules so failure paths carry element indices; carrier: accumulate per record, with record-skip versus batch-abort as a declared policy row, never code; trigger: per batch.
- Under record-skip policy, skipped records are per-record outcomes on the rail — recorded evidence with their faults attached, never silently dropped rows; the batch receipt carries admitted, skipped, and aborted counts as one value.
- Row: I/O-backed checks (uniqueness, referential existence) — owner: async rules on the boundary validator of the seam that owns the I/O; carrier: abort after structural accumulation — structure first, I/O only over structurally sound input via dependent-rule ordering; trigger: per message. Domain interiors never perform these checks.
- Row: domain construction — owner: generated value-object and union admission partials; not a boundary-validator seam: the boundary validator validates raw shapes exclusively, the generated owner admits domain semantics, and the two never share a fact.
- Negative row — egress: no validation row exists for outputs; interior totality makes egress valid by construction, so an egress validator is not defense in depth but a symptom report that some interior path constructs unvalidated values — the repair is the leaking constructor, never the egress check.
- Negative row — cross-message facts: idempotency, replay, ordering, and quota are not validation; they belong to the transport and persistence layers' own laws, and a `MustAsync` reaching into message history is a boundary violation wearing a rule's clothes.
- Interactive rows declare their severity usage explicitly: advisory tiers flow during editing, the gating tier decides only at submit — severity selection is rule law, but WHICH tiers a row exercises is a matrix column.
- Per-field interactive runs and whole-shape admission runs are the same validator under different selectors — two rule graphs for one shape (a UI validator beside an API validator) is the rejected form; variant needs are ruleset rows on the one owner.
- The matrix is closed data at the composition root: rows pair a payload type to a validator registration, which makes the pairing checkable instead of conventional.

## admission-order

- One admission order at every seam, no reordering, no skips:
  1. raw-shape materialization, fail-closed — bytes and keys parse into the boundary DTO with unknown members rejected and parse faults captured;
  2. boundary validator — structural and cross-field facts over the DTO accumulate into one outcome;
  3. rail projection — the outcome crosses onto the typed rail exactly once through the one bridge;
  4. domain admission — generated owners mint domain values from validated raw parts, their own admission the final arbiter.
- Evidence flows forward only: each stage consumes the prior stage's success type and cannot observe raw input from an earlier stage.
- Stage outputs are types — stage 1 yields the DTO, stage 2 the outcome, stage 3 the carrier, stage 4 the domain value — so the order is enforced by signatures: a stage cannot be skipped without a type error, and discipline is never the enforcement mechanism.
- Stage responsibilities are exclusive: the boundary validator never constructs domain types and never duplicates a generated owner's invariant — a range the value object enforces is checked once, in the owner.
- The domain owner never re-checks structural facts (presence, length, format) that stage 2 settled.
- The split rule: facts statable over the raw DTO alone belong to stage 2; facts that define the domain value belong to stage 4.
- Stage 2 is the last point where raw input is observable — diagnostic capture of raw values happens here under the classification policy or not at all; later stages see only admitted types.
- Stage 2 runs on raw optionals: built-in shape rules are null-permissive by design, so presence is declared explicitly where required, and legal absence flows as success into stage 4 where option-typed admission decides — sentinel and absence projection mechanics stay boundary-capsule law; this page fixes only WHERE in the order they fire (during 1-to-2, never later).
- Validation is an explicit pipeline step invoked by the seam's composition — ambient framework auto-validation is rejected: it hides the admission point, runs validators synchronously regardless of their async character, and detaches the outcome from the rail.
- The order is the page-level instantiation of the one admission seam: stages 1-2 are the definition-time weave at the boundary shape, stage 3 is the single point where outcomes lift into rails, stage 4 is generated-owner admission — policy pushed across any of these joints stops being recoverable from its declaration.

## rejected-shortcuts

- validate-after-construct — building the domain object from raw input and then validating it forecloses interior totality: every consumer must re-suspect every instance; the admission order makes invalid domain values unrepresentable instead.
- throw-as-flow — `ValidateAndThrow` / `ValidateAndThrowAsync` / strategy `ThrowOnFailures` raising `ValidationException` (its `Errors` property carries the failures; one constructor appends the default message text to a custom message) is rejected as a seam mechanism: the exception erases the accumulate/abort distinction and crosses layers untyped.
- The only legal `ValidationException` seat is the final adapter to a foreign surface that speaks exceptions — and there it is the provider-exception arm, not validation flow.
- re-validation in the interior — calling any validator on an admitted value marks a leaked boundary; the repair moves the missed fact into stage 2 or stage 4, never adds a second checkpoint.
- dual admission — the same fact in both the boundary validator and a generated owner produces divergent messages, divergent codes, and a maintenance fork; one fact, one stage.
- partial-as-admission — accepting a payload because a property-selected or ruleset-selected run passed; selectors exist for feedback and variant routing, and only the seam row's full declared run admits.
- per-seam bespoke outcome handling — each seam folding failures its own way; the outcome leaves every seam through the same bridge (bridge law is projection-lane territory).
- silent-skip composition — child and polymorphic adaptors skip null values and unregistered runtime types by design; relying on that skip as an admission decision is rejected: required children and closed type families are declared, so silence can only ever mean legally absent.
- shared DTO across seams — one boundary shape serving two seam rows couples their validation evolution; seam variants on ONE shape are ruleset rows on its one validator, while genuinely distinct seams get distinct shapes and distinct rows.
- environment-swapped validators — a relaxed validator substituted per environment forks the rule graph; environmental leniency is a ruleset or severity-policy variant on the one validator, so what production enforces is always a superset-visible diff of one declaration.

## admission-once

- A value admitted at a seam carries its evidence in its type; downstream code recovers validity from the type alone.
- The corollary defect detector: any interior `IsValid` check, defensive null test on an admitted value, or second validator invocation is proof the seam leaked — and the fix is upstream.
- Admission produces evidence even on success when the seam is audited: the admission record is three fields — seam row identity, executed-variant receipt, override policy in force — and answers "what was checked, under what policy" without re-running anything.
- Admission records are ordinary facts on the operational stream; their export and retention ride the telemetry pages' governance, and no validation-specific audit channel exists.
- Cross-process, admission-once is per trust boundary, not per object lifetime: a payload admitted in one process and forwarded re-enters admission in the receiving process.
- When both ends compile from one shared contract, the receiving seam still runs stage 1 (fail-closed decode) while stages 2-4 collapse to the contract's guarantees — trust shrinks the order, never bypasses it.

## provider-exception-arm

- Each seam owns exactly one conversion arm where provider exceptions (deserializer, binder, transport) become the same fault stream stage 2 feeds — parse faults and shape faults converge into one outcome before the rail.
- The arm absorbs all three parse-fault classes — malformed syntax, unknown member, type mismatch — into one vocabulary; distinguishing them is a code concern on the fault, never a second channel.
- The arm is per-seam, never global: a global exception-to-fault translator erases seam identity, while the per-seam arm mints faults carrying the seam row's identity — which provider failed where is recoverable from the fault alone.
- Consumers therefore see one rejection vocabulary per seam, never an exception channel beside a validation channel.
- Two package exceptions are deliberately NOT seam outcomes: the null-root `InvalidOperationException` and `AsyncValidatorInvokedSynchronouslyException` are composition defects — a seam wired to pass null, a sync path wired to an async graph.
- Composition defects fail fast and loud at the seam's first exercise and must not convert into validation faults: converting them disguises a wiring bug as a data bug.

## carrier-selection

- Accumulating rail versus fail-fast admission rail is selected by the seam row's structure, never by a flag.
- Structurally independent facts — field shapes, parallel options sections, per-record checks — accumulate: the applicative validation carrier collects every fault in one pass.
- Sequenced mounts — parse-then-check-then-bind chains where step N+1 is meaningless after step N fails — abort: the monadic carrier stops at the first fault.
- Carrier selection is visible in the row's types: the accumulate row's bridge targets the accumulating carrier, the abort row's the fail-fast carrier — reading the matrix shows each seam's failure semantics without opening a validator.
- Within one boundary validator the same selection appears as cascade policy per chain; across the seam it appears as the carrier the bridge targets; carrier choice and accumulation algebra are settled rail law — the seam's only decision is which row gets which.
- Mixed seams sequence the two: accumulate structure fully, then abort into the I/O-or-domain step only on accumulated success — the one-line composition of both carriers, and the reason structural validators must not hide I/O rules mid-graph.
- A dependent chain deeper than two steps inside an accumulating boundary validator is mis-homed work: that depth signals a mount or workflow sequence wearing validation's clothes, and it moves to the owning lifecycle path where abort semantics are native.

## options-seam

- Options validation is the configuration instance of the same order: bind fail-closed, validate the bound shape, project, freeze.
- The options-shape validator is the single shape authority; the options-validation contract (`IValidateOptions<TOptions>` returning `ValidateOptionsResult`, with a result builder for accumulated failures) is a thin projection adapter over it.
- The one outcome serves two consumers: the adapter projects messages into the host's start gate, and the bridge projects the same outcome onto the rail — the adapter never becomes a second validation path, it is a second projection of the first.
- The eager-start hook (`ValidateOnStart` / `AddOptionsWithValidateOnStart`) is the trigger — boot refuses to complete over invalid configuration.
- The combined registration form (`AddOptionsWithValidateOnStart`) binds registration and eager trigger in one call — split registration where the trigger is attached separately is the drift form, because a forgotten trigger silently demotes the options row from at-boot to first-resolution.
- The options-validation contract is synchronous; an async rule in an options validator throws on first resolution — options validators are sync-rule-only by law, self-enforcing at boot when eager validation is on.
- Binding is fail-closed-on-unknown (`ErrorOnUnknownConfiguration` on the binder policy): an unrecognized configuration key is a boot rejection, not an ignored line — typo'd keys surface as faults instead of silently-default values that pass validation.
- Validate-once-then-freeze-and-publish: after the start gate passes, validated options become frozen policy values published to consumers.
- Re-validation on read, change-token-driven re-binding into mutable policy, and per-request options resolution at validated seams are rejected — a changed source is a new admission (restart or an explicit re-admission seam), never a silent in-place mutation of published policy; freeze and publish mechanics are runtime law, this seam owns only the gate that precedes them.
- Named options validate per name through the contract's name parameter — one validator covers every named instance; a name-specific fact is a condition on the context, not a second validator.
- Options validators run against the bound mutable instance before freezing — the validator observes, never repairs: a rule that normalizes or defaults a bound value has smuggled configuration logic into validation, and the repair belongs to the binding or defaulting layer.

## capability-pairing

- Capability pairing is a typed rejection at composition time: a seam row whose payload type resolves no validator, an options registration without a paired validator, or a validator registered for a type no seam row claims — each fails the root build with a typed composition fault naming the unpaired half.
- Pairing failures at composition cost one boot; pairing failures discovered at message time cost a silent unvalidated ingress — the asymmetry is the law's entire justification.
- The pairing check is a fold over two enumerations both available at the root — declared seam rows and validator registrations; set-difference in both directions must be empty. The check is the seam matrix made executable.
- The driver and the proofs share the same pairing enumerations — the seam driver resolves through exactly the pairings the proofs verified, so a passing proof pass is the driver's correctness precondition, not a parallel assurance.
- Pairing extends one level down: ruleset names forwarded into child validators are stringly surfaces too, and the forwarding names on a parent's child adaptors sweep against the child validator's declared ruleset metadata — a forwarded name with no matching child ruleset silently selects nothing, which the sweep converts into a composition rejection.

## closure-proofs

- One proof law covers every open registration surface at a validation seam: anything keyed by string or by runtime type is proven total over a generated or declared source enumeration at composition time, by a fold whose failure is a typed composition rejection.
- Instance — polymorphic totality: the per-derived-type validator table is swept against the closed family's case enumeration (generated union cases, smart-enum items); exact-type lookup passes unknown subtypes silently, so the proof is the only barrier between "new case added" and "new case unvalidated" — it converts a silent pass into a boot failure.
- The polymorphic proof's existence condition is itself a law: the sweep needs a closed enumerable family, so polymorphic validation over an open hierarchy — no generated owner, no case enumeration — is rejected outright; closing the family precedes validating it.
- Instance — ruleset vocabulary closure: `CreateDescriptor().GetRulesByRuleset()` (ruleset metadata: name plus rules) is swept against the declared seam-variant vocabulary in both directions — a ruleset name outside the vocabulary is drift (typo'd strings validate nothing), and a vocabulary entry with no rules is a phantom variant.
- The ruleset sweep also closes the default-selector hole: a rule moved into a ruleset leaves plain validation silently, and the sweep is where that movement becomes visible.
- Instance — member coverage closure: `GetMembersWithValidators()` / `GetValidatorsForMember(name)` against the DTO's reflected members — every member carries at least one rule or appears on a declared exemption list; a new DTO property without a rule or exemption fails the sweep, extending fail-closed-on-unknown to the validator's own coverage so shape growth cannot outrun rule growth.
- Instance — registration closure: the capability-pairing fold, over scan results and seam rows.
- Instance — code vocabulary closure: the projection's frozen code-to-case map sweeps against the declared code enumeration; the projection lane consumes the map, this fold proves it.
- Proofs run where the enumerations live: at the composition root during boot, or on the proof rail as architecture facts; both consume only public descriptor and registration surfaces, never rule internals.
- Proofs are deterministic folds over already-materialized metadata — descriptor metadata is constructed from the rule graph, scan output from the type system — so the proof pass costs one enumeration at boot and never touches a payload; proof cost is structurally incapable of scaling with traffic.
- A proof failure is itself a typed fault in the same vocabulary — composition rejections carry codes from the capability band, so boot failures and message-time rejections fold into one fault stream with their origin recoverable from the code.

## fail-closed-defaults

- One default across every admission surface: unknown input is rejected, and every opt-out is a recorded policy row naming the seam and the reason.
- Per-surface instances this page legislates: unknown configuration keys (binder policy row); unknown wire members (the wire codec's rejection channel — converter mechanics are boundary law, this page fixes the default's direction); undefined enum values including undefined flag combinations (enum admission is defined-bits-closed); unregistered polymorphic cases and uncovered members (closed by proof rather than by knob).
- Interactive surfaces get no stage-1 discount: edit surfaces construct the same DTOs through the same fail-closed materialization — a UI that assembles domain values directly has bypassed the matrix, not optimized it.
- Classification-admission rejections at the telemetry seam follow the same direction under their own page's mechanics — stated here only as the direction's reach.
- Fail-open surfaces that cannot be re-defaulted are closed by proof instead: where the package skips silently — null children, unknown runtime types, ruleset-displaced rules — the closure proofs convert each silent path into a composition-time failure, making the system-level default fail-closed even where the package-level default is permissive.
- The opt-out registry is itself swept: a permissive policy row whose seam no longer exists is dead policy, and the sweep that proves opt-outs are enumerable also proves they are live.

## forced-override

- A forced override — an operator accepting a payload that fails admission — re-runs the same rule graph under an override policy that downgrades the gating tier to a recorded tier.
- The identical failure values are still minted, still land in the outcome, still cross the bridge — but no longer block; the override decision, its principal, and the downgraded failures travel together as rail values.
- Severity-provider mechanics are validator law; the seam law is: override changes classification, never execution.
- Override permission is a conjunction of two declarations: the seam row's override-policy column AND the fault's band-level eligibility — both must permit, so neither a permissive seam nor a permissive code alone opens the gate.
- Partial runs produce no admission record — only the row's full declared run mints one, which is what makes "was this admitted" a one-field query instead of a forensic reconstruction.
- Skipping the validator under override is the rejected form — silent override, no evidence of what was waved through; post-hoc deletion of failures from an outcome is equally rejected as evidence tampering — the partition belongs to the projection, not the seam.
- Override receipts compose the executed-variant evidence: the admission record under override shows the full run occurred plus the policy that reclassified it — auditable without any extra logging seam.

## divergent

- seam-matrix-admission-order — the matrix as the page's absorbing owner: a new ingress (new transport, new import format, new interactive surface) is one new row binding payload type, validator owner, carrier, and trigger — zero new admission machinery, because stages 1-4 are seam-invariant and only the row's bindings vary.
- seam-matrix-admission-order — the rejected-shortcuts catalogue is the matrix's negative space: each shortcut is a row missing one column — no owner is ambient validation, no carrier is throw-as-flow, no trigger is validate-on-read, no stage-4 is validate-after-construct — which makes shortcut detection mechanical: name the missing column.
- seam-matrix-admission-order — the order's deepest property is cost-boundedness and idempotence per message: stage 1 is O(payload), stage 2 is one pass over one rule graph, stage 3 is one fold, stage 4 is per-constructed-value; nothing re-enters an earlier stage, so admission cost is a static function of the seam row — observed re-validation cost is a leak detector firing.
- seam-matrix-admission-order — the matrix doubles as the threat-surface inventory: review reads rows, not code, and the registration closure makes "unguarded ingress" a provable property rather than a review hope — the security audit of the validation layer is the matrix plus the proof results, both data.
- closure-proofs-fail-closed — the unified proof law is this page's instance of closure-over-source-enumeration: a frozen pairing proven total over a generated item set so a new case fails statically or at boot instead of at the unlucky message. All four instances share one four-step shape:
  - enumerate the source — generated cases, declared vocabulary, reflected members, seam rows.
  - enumerate the registration — type table, descriptor metadata, rule coverage, scan output.
  - assert bijection or declared exemption between the two.
  - reject typed on any difference.
  New proof candidates land as new instance rows of the same fold; the proof family is itself closed and absorbing.
- closure-proofs-fail-closed — fail-closed economics: every fail-open default trades a cheap visible boot failure for an expensive invisible production pass. The quantitative rule: an admission surface earns a permissive row only when the unknown input is provably non-actionable (logged-and-dropped telemetry, forward-compatible fields under a versioned contract) AND the permissive row is enumerable by the proofs — an opt-out the sweep cannot see is not policy, it is a hole.
- closure-proofs-fail-closed — proofs double as the seam documentation: the swept vocabularies, exemption lists, and matrix rows ARE the document, and documentation drift is structurally impossible because the document is the very data the proof reads — a reviewer answering "what does this seam check" reads the row and the sweep output, never the rule bodies.
