# [CSHARP_SHAPES]

A concept owns exactly one generated type, and five discriminants select it before any attribute is written: admission (does raw material cross a trust boundary here), identity regime (key, structural, case, or reference), variant arity (one shape or N alternatives), payload timing (case data fixed at declaration or constructed per occurrence), and openness (closed vocabulary or foreign extension). The selection fixes where change detonates, what equality means, and which capabilities derive — every misplaced shape traces to one mis-answered discriminant. Raw material is admitted once into the owner through its generated factory; everything interior holds evidence and re-validates nothing.

## [1]-[OWNER_CHOOSER]

This table routes a concept signature to its owner; the cards below legislate each owner's law.

| [INDEX] | [CONCEPT_SIGNATURE]                            | [OWNER]                          | [IDENTITY]      |
| :-----: | :--------------------------------------------- | :------------------------------- | :-------------- |
|   [1]   | invariant-bearing scalar                       | `[ValueObject<TKey>]`            | key             |
|   [2]   | N fields, one concept, no discriminant         | `[ComplexValueObject]`           | structural      |
|   [3]   | bounded vocabulary, wire-keyed identity        | `[SmartEnum<TKey>]`              | key             |
|   [4]   | bounded vocabulary, process-local behavior     | `[SmartEnum]` keyless            | reference       |
|   [5]   | closed alternatives, per-occurrence payload    | `[Union]`                        | case            |
|   [6]   | one value over 2-5 unrelated types             | `[Union<T1,...>]` ad-hoc         | slot then value |
|   [7]   | interior product, no invariant, no admission   | record or readonly record struct | structural      |
|   [8]   | combinable capability set                      | vocabulary items in a frozen set | key             |
|   [9]   | runtime-sourced vocabulary                     | keyed owner plus frozen registry | key             |
|  [10]   | cross-product or externally sourced policy key | frozen table                     | composite key   |
|  [11]   | foreign wire enum or ABI bit layout            | language enum at the seam only   | ordinal         |
|  [12]   | foreign code must add cases                    | manual interface or hierarchy    | declared        |

## [2]-[DECISION_LAW]

[OWNER_SELECTION]:
- Law: singleton-versus-instance is the sharpest vocabulary discriminant — a smart enum is a fixed set of named instances dispatched by identity, a union a fixed set of shapes dispatched by case; vocabulary that sprouts constructor arguments at use sites has already become a union.
- Accept: the complex value object only when every field is meaningful under every value; when any field is meaningful only under some discriminant, the concept is a union regardless of field count — a sum encoded as a product.
- Reject: the regular/ad-hoc union split decided by arity or convenience — the split is relatedness: cases that carry meaning outside the union are ad-hoc members, cases that exist only as this concept's alternatives are a regular family. Value object versus ad-hoc union over the same data resolves on invariant presence; the ad-hoc form validates nothing.
- Boundary: a generated owner that validates nothing, applies default comparer policy, and never crosses a boundary decides nothing — the plain record was already correct; the inverse is equal spam: an anemic record at a boundary scatters its missing validation across call sites.

[SHAPE_INVERSIONS]:
- Law: three or more sibling types sharing fields for one concept collapse into one closed family; analyzer-enforced closure prevents regrowth.
- Reject: the nullable payload bag — one record, N nullable fields, a kind discriminator — re-derives at every consumer the discriminant-field correspondence the type system already knew; the union's exhaustive dispatch deletes every null check and inverts the failure mode from silent to loud.
- Reject: the enum-plus-dictionary pair — the language enum admits undefined values by cast, the dictionary lookup is partial, and the two drift independently; a smart enum closes admission and stores the policy as columns in one declaration.
- Reject: owner-per-protocol duplication — a domain type shadowed by JSON, persistence, and binding types collapses into one owner plus object factories partitioned by framework, the partition analyzer-policed.
- Boundary: same-payload case collapse targets passive, non-generic, non-fault unions — marker cases, behavior-owning cases, fault families, and semantically distinct member names stay distinct, because equal payload shape alone never merges meaning.

[BOUNDARY_AND_CHURN]:
- Law: a value rewritten per frame or per solver iteration does not belong in a validated owner — accumulation lives in a plain record or struct and admission happens once where the result becomes domain material; the decision variable is reads-of-evidence per write. Generated owners delete the external update surface structurally, so every derived state is a new admission.
- Accept: the language enum in exactly three places, all seams — mirroring a wire schema a foreign protocol owns, bit-flag interop where a foreign ABI owns the layout, ordinal indexing inside measured kernels; its open admission by integral cast is the disqualifier everywhere else, and conversion to the closed vocabulary at the seam is the re-closing move.
- Reject: flags enums for combinable capability — a flags value has nowhere to put per-flag behavior, so flag-test chains re-derive policy at every consumer; vocabulary items in a frozen set carry behavior as columns, membership is set algebra, policy is a fold.

[BEHAVIOR_PLACEMENT]:
- Law: behavior placement follows ownership of the key that selects it — call-site-varying behavior rides generated dispatch, case-intrinsic behavior with a uniform signature rides behavior columns or case members, and behavior keyed by something the family does not own rides a frozen table.
- Accept: frozen tables in exactly three places — cross-product keys no single family owns, externally sourced policy admitted at startup, and keys outside any generated family; a table keyed by generated vocabulary proves totality at composition time by comparing cardinality against the item-count product, converting lookup risk into a startup invariant.
- Reject: external dictionaries keyed by items — they re-derive a correspondence a column or dispatch already owns and miss new items silently where generated dispatch arity-breaks loudly; repeated full-coverage dispatch with identical arms across consumers is the fold-to-column signal.

[CHANGE_DETONATION]:
- Law: owner selection is selection of where change detonates — a new union case lands at every exhaustive dispatch site, a new smart-enum item lands at its own declaration because the generated constructor demands every column, a new complex member lands at every factory call, and a key-type migration lands only at conversion and wire seams because interior code holds the owner, never the key.
- Law: growth absorption is tunable to totality — a vocabulary with column-held behavior and dispatch generation deleted absorbs item growth with zero consumer breaks, while the same vocabulary with generated dispatch pushes every addition to all call sites; the setting is a declared stance on who pays for growth.
- Boundary: invariant tightening is the one change with zero compile signal — the factory narrows silently while stored values persist under the old rule; tightening is a data-migration decision with a code rider, and the proof obligation lives entirely in tests.

[MANUAL_EXEMPTIONS]:
- Law: generated families close the case axis and leave the operation axis open — extension members attach operations from outside, so the manual-owner exemption triggers on case extension alone; an extension point foreign code must populate with new cases is an interface or abstract hierarchy with the closure disciplines applied by hand.
- Accept: manual routes for owners that must be interfaces, generic case payloads (lift the parameter to the union root instead, or hand-roll the hierarchy), ad-hoc arity past five members, and runtime-sourced vocabulary — items are compile-time static fields, so configuration-sourced sets become a keyed owner plus a frozen registry admitted once at startup.
- Law: per-item behavior needing generic type parameters takes derived cases — one private generic case class closed at several type arguments yields type-indexed items while the vocabulary stays non-generic.

## [3]-[VALUE_OBJECTS]

[ADMISSION_FACTORY]:
- Law: `Validate` is the primal factory — the only signature returning the typed fault and the instance in one call; `Create` throws with the fault string-flattened, `TryCreate` discards or merely annotates it, and both hard-code a null format provider, so culture-sensitive admission enters through `Validate` alone.
- Law: the two hooks form an ordered pipeline — `ValidateFactoryArguments` receives every argument by `ref` (normalization mutates pre-storage, so the stored key is the canonical form and call sites cannot admit unnormalized values), then `ValidateConstructorArguments` runs on every construction path including rehydration but can only throw.
- Law: hook placement decides invariant-drift failure mode — the factory hook owns rejections legitimate on fresh input and exempts stored data; the constructor hook owns the invariant-of-record and converts drift into a load-time fault. An invariant living only in the factory hook is a deliberate statement that stored data is exempt.
- Law: a non-`void` hook return threads admission evidence the frozen members cannot hold into `FactoryPostInit` — transactional advice reached only on genuine admission, only on the factory path; rehydration runs neither hook nor post-init, so anything derived from the threaded value must be re-derivable or it silently does not exist on stored material.
- Reject: per-call-site error translation — the validation-error contract is one static `Create(string)`, and a fault family satisfying both that contract and the rail's expected-error base is simultaneously the generated owner's error and a first-class rail value.

[KEY_AND_IDENTITY_POLICY]:
- Law: the key defaults to a private field escaping only through conversion and explicit-interface egress — consumers compare and dispatch on the owner, never the raw key, and a key-type migration becomes a boundary break instead of a pervasive one; a public key member is an opt-in.
- Law: comparer policy is a type, not a value — accessor classes carry static-abstract comparer properties threaded as type arguments, so one attribute swap changes equality, hashing, ordering, and operators at once, with the accessor never instantiated; the accessor stores its comparer in a static field, because a fresh comparer per property read allocates inside every dictionary probe.
- Law: string keys default to ordinal-ignore-case across every surface at once; the stance is declared per owner and never inherits, so every string-bearing layer declares its policy from one accessor type — repetition of the declaration is mandatory, divergence of the policy is the defect.
- Law: equality and ordering pair or diverge as one policy — an ordering accessor without its equality counterpart lets hash containers and sorted containers disagree on identity; `CompareTo` and the relational operators are independent derivations that agree only through a configured accessor.
- Accept: complex-owner equality membership as opt-in via member comparers — unmarked members vanish from equality, hashing, and the diagnostic string while remaining factory parameters; collection-typed members compare by reference identity unless a sequence-semantics accessor is attached, the only sanctioned fix.

[OPERATOR_ALGEBRA]:
- Law: the six operator axes are a per-operation grant set declaring the quantity's group structure — the enabled axes are exactly the operations under which the quantity is closed, every generated operator is homogeneous, and cross-dimension operations are hand-written against the foreign result type: the declaration is the algebra, recoverable without reading a method body.
- Law: every operator body routes its raw result back through the throwing factory — arithmetic re-enters admission, so subtraction on validated operands can throw when the result violates the invariant; the `checked` variants add overflow trapping on the key math, giving one operator two failure species selected by the call site's checked context.
- Law: no identity element is ever synthesized — a unit is a constant minted outside admission, so the advertised structure is a magma and every seed, zero, or bound enters a generic kernel as an admitted parameter; an algorithm constrained on the aggregate number interface excludes every owner by construction, and the exact binary families it uses are the owner-compatible spelling.
- Law: key-typed operator overloads compare and compute against an unadmitted raw operand under the owner's comparer policy without validating it — a membership and threshold gate at the boundary, never a proof of admissibility.
- Boundary: narrow integral keys have language promotion erased by a generated cast back to the key — unchecked computes wide, truncates silently, and re-admits the residue, while checked throws before narrowing; full-range owners on narrow keys carry silent modular arithmetic in unchecked contexts.

[ABSENCE_AND_DEFAULT]:
- Law: null-yield modes convert blank input from rejection into success-with-null, and the empty-string mode is the only one that also removes the `NotNullWhen` annotation — under the null-only mode flow analysis trusts an annotation the generator left unsound, so bridges audit the generated attributes, not the documentation; the honest projection is option-of-owner with absence decided at the seam.
- Law: struct owners deny `default` structurally and the analyzer escalates declaration-site zero-init to errors, but array allocation, unconstrained generic `default`, and field zero-init mint ghosts invisibly — the runtime gate belongs once at the outermost storage seam, reading the key member directly rather than comparing against a default instance.
- Law: default-hostility propagates transitively through every nested member and the computed state overrides the requested permission — an aggregate stays default-safe down its whole spine or takes the class form at the first hostile member; a struct with a legitimate default names it as a canonical instance.
- Boundary: class owners admit null at the factory edge with declared null-yielding policies; struct owners trade absence for layout — the choice relocates the absence discipline between a nameable, policed null and an unnameable, free-to-construct poison.

## [4]-[SMART_ENUMS]

[VOCABULARY_DECLARATION]:
- Law: the declaration list is the vocabulary — public static readonly fields typed exactly as the enum fix item membership, dispatch indices, callback order, and metadata identifiers; a static property or a case-typed field silently vanishes from items and dispatch at warning severity, so vocabulary-heavy code promotes those two warnings to errors.
- Law: a keyed vocabulary carries two total orders that agree only by accident — items enumerate in declaration order while comparison ranks by key under the configured comparer; domain rank divergent from key order is an item column sorted by projection, never a bent comparer, because key policy is one point swinging lookup, hash, comparison, and operators together.
- Law: items initialize in declaration order, so a row referencing a later item captures null silently and the materialization guard never catches it — cross-row references defer behind a delegate evaluated at call time, the only resolution for cyclic graphs.
- Accept: keyless vocabularies for behavior-only rows — items, dispatch, reference identity, no lookup, no conversions, no parsing; wire identity on a keyless form is a declared object-factory exception, never ambient.

[LOOKUP_LIFECYCLE]:
- Law: validity is a property of keys, not instances — no invalid item is constructible, and code holding a key chooses exactly one verb: `Get` throws typed and backs the explicit conversion, `TryGet` is the bool form, `Validate` returns the typed fault; a vocabulary whose call sites catch exceptions is using the wrong verb.
- Law: the whole runtime state funnels through one publication-locked lazy whose single pass assigns indices, builds the frozen lookup, and fail-fasts on duplicate keys — and it caches a thrown exception for the life of the process, so forcing items in a startup probe is the only place this defect class is cheap; the poisoning transits the metadata plane, so serializers walking items trip it identically.
- Law: a derived index keyed on an item column projects from the item accessor, never from an eager static initializer — reading through the accessor inserts the happens-before edge through materialization, and an eager initializer can observe sentinel indices and unresolved cross-references.
- Law: string-keyed vocabularies admit spans end-to-end through the frozen alternate lookup with zero allocation; a custom comparer built from a delegate factory satisfies the build but not the span plane and poisons the first lookup at runtime — custom comparers implement the span-alternate interface or stay BCL.
- Boundary: items are static state per load context — vocabulary values cross isolation seams as keys re-admitted on the far side, never as instances.

[DISPATCH_AND_ROWS]:
- Law: generated dispatch compiles to an integer jump table over a write-once index cell — one field read and a branch on the hot path — and totality is enforced by method arity: adding an item adds a parameter to every total overload, so a stale exhaustive dispatch refuses to compile; unnamed callback arguments are an analyzer error because the argument name is the reorder shield.
- Law: generated dispatch is the only total dispatch the type system can hold — items are singletons, not constants, so a language `switch` cannot be total over a vocabulary, a key-pattern probe re-enters string literals and drops future items, and a guard chain re-derives the correspondence one dispatch call owns.
- Law: behavior rows ladder by tier — delegate columns injected per item row for policy the vocabulary owns (constructor arity forces the column, so a new item cannot omit it), generated dispatch for single-consumer logic where compiler-checked totality is the point, and abstract members with private nested derived cases only where a behavior needs generic type parameters or coherent multi-member overrides.
- Law: partial dispatch is a presence test — the action form's omitted arm and explicitly-null arm are indistinguishable and an omitted default makes every unhandled case a silent no-op, while the partial map's set-flag carries every legal value including default; state-threaded forms admit span-shaped state and results, and static lambdas with threaded state are the analyzer-encoded posture.

## [5]-[UNIONS]

[FAMILY_SELECTION]:
- Law: the root declaration kind is family-global — a record root buys whole-family structural equality with cross-case comparison constant-false and stays flat by analyzer law; depth (intermediate cases, nested dispatch, stop-at boundaries) requires a class root and surrenders generated equality. The trade is exactly evidence family against intent tree.
- Law: closure is constructor reachability, not attribute law — owner constructors private, cases sealed or private-constructed, and case discovery recursing only through class-kind containers, so a case nested beneath a struct or interface intermediate is a phantom: legal to derive, invisible to dispatch, landing in the default arm's runtime throw.
- Law: a generic union root is a phantom-typestate carrier — cases close over the root's parameters without declaring their own, one declaration yields a family per marker instantiation, and dispatch stays total within each phase; the cost is invariance, paid at storage and transport with an erasing projection.
- Law: cases admit their own payloads — the union composes already-admitted material and carries no validation partial; closed custom admission is non-public construction plus suppressed conversions plus one hand-written factory returning a rail value.
- Reject: an ad-hoc union spelling exactly success-or-failure — rails own outcome transport; unions own domain variant vocabularies whose cases demand distinct continuations.
- Accept: duplicate member types in an ad-hoc union as deliberate modeling — the same representation under two names yields slots whose only ingress is the named factories and whose equality discriminates the slot before the payload.

[DISPATCH_AND_GROWTH]:
- Law: `Map` evaluates every arm before dispatch — preallocated verdict tables and cold carriers only; any allocating or task-shaped arm belongs in func-form `Switch`, where selection and execution stay separate phases. Eager task arms run all branches, abandon the losers mid-flight, and surface their failures as unobserved faults far from the call.
- Law: the case list is part of every dispatch signature, so case addition is a binary break at every consumer assembly — loud, early, impossible to misroute; that propagation is the contract. Case rename breaks on two axes at once, because generated parameter names are public API under the named-argument law and the case type name is embedded in consumer signatures.
- Law: stop-at overloads dispatch a case subtree as one named arm while sibling leaves stay exhaustive — two granularities over one tree, each total, each breaking only on growth in the axis it names; the coarse handler is also the seam where a fold re-enters a different carrier.
- Law: operation families attach to the closed owner as extension members whose bodies are total dispatch — case addition still breaks them at compile time — and combination operators are extension territory while conversion operators stay generator territory; neither family generates ordering, so rank is a `Map` to preallocated ordinals composed into a comparer.
- Reject: partial dispatch for routing — a new case routes to the default silently at every partial site; partial forms are for semantically total defaults, and carrier-polymorphic folds stay on the total forms because a partial fold defaults the new case in every specialization at once.

[OPERATION_SURFACE_POLICY]:
- Use: `GenerateUnionOps` on operation and intent unions that own generated per-case operation identity; `SkipUnionOps` on result, evidence, and wire-shaped unions so the absence of an operation surface is declared, never accidental.
- Reject: separate operation tables that drift from generated cases, and unqualified unions in functional surfaces where the operation stance is undeclared.

[AD_HOC_FORM]:
- Law: storage is computed, not declared — one typed field per stateful unique member until the second stateful reference member collapses every reference member into one shared object slot; struct members keep inline fields, and a stateless member is zero-width with its identity carried entirely by the discriminator.
- Law: `default` of a struct ad-hoc union is poison, not value — index zero throws from every observing surface, the type system cannot name the state, and the crash site is the first observation, never the minting site; the case probes are the one surface total over poison, and the disjunction of probes reconstructs the typestate test the language refuses to express, so every rehydration, pooling, and array-scan seam guards on probes before touching values.
- Law: equality is discriminator-gated then member-dispatched under the declared string-comparison default, and the hash is the active member's hash without discriminator mixing; `ToString` forwards the raw member and erases the active case from every log line — identity-bearing rendering projects through `Map` to labeled text.
- Law: generated implicit conversions make the union a parameter-side absorber — one parameter replaces an overload per member and collection expressions lift mixed batches element-wise — until a member is an interface, `object`, a type parameter, or a duplicate, where admission flips to named factories; a non-public constructor modifier without suppressing value conversions leaves the public operators as an ingress bypass, so closing admission sets both.

[RAIL_ARMS]:
- Law: every non-abstract case is a Kleisli point — the payload is the input, the arm body is the lift, and generated total dispatch is the only place the family's continuation is selected; arm-set uniformity is load-bearing, so a constant arm writes the carrier's pure lift, which keeps the arm portable across every carrier the fold specializes to.
- Law: a recursive case's arm is a traversal, not a sequencer — child folds combine through the applicative tuple combinator, naming the pure combine and nothing about failure policy; the carrier's trait constraint is the only edit that flips a whole fold between fail-fast and accumulating.
- Law: state and result channels are orthogonal — environment rides the threaded state, results ride the carrier, and a second monad is earned only when the environment itself is effectful; the ref-struct result channel and the kinded heap carrier are chosen per fold and never mixed in one dispatch.
- Boundary: generated dispatch is depth-honest recursion — one type-test ladder and two frames per level — so unbounded or hostile input takes an explicit-stack kernel at the admission boundary, the named statement exemption.

[WIRE_SURFACE]:
- Law: neither union family is wire-capable alone — no discriminator is emitted, so a boundary-crossing family either admits a single-scalar grammar through an object factory (one parser, one printer, every consumer routed through the same `Validate`), re-shapes as a keyed owner whose key is the discriminator, or accepts an edge DTO; a union whose cases cannot fold into one grammar is interior-only by construction, and that constraint runs backward into owner selection.
- Law: polymorphic JSON on a regular union is the platform's own per-leaf derived-type registration with exact-runtime-type resolution — an unregistered leaf fails rather than degrades — and discriminator strings derive from `nameof` or the published member list so a rename breaks compilation, not production decoding.
- Law: factory declaration order is load-bearing — conversion resolution selects the last factory matching a consumer's filter, so appending a factory can silently re-route an existing consumer; serialization, persistence, and binding claims are analyzer-deduplicated to exactly one owner each.
- Boundary: deserialization is admission — every generated converter routes the wire value through `Validate`, hostile payloads fail into the serializer's error channel, and a half-built union cannot exist in memory; converter code materializes only when the framework's integration assembly is present, so wire flags without the assembly are dormant metadata.

## [6]-[ADMISSION_RAILS]

[RAIL_BRIDGE]:
- Law: the projection from generated outcome to rail is one expression — `Validate` discriminated by a property pattern, with the carrier's implicit conversions lifting both fault and instance; one generic static extension bridge over the factory contract serves every owner, the receiver position being the only place the owner type infers, and call sites read owner-dotted with zero type arguments.
- Law: the bridge's constraints mirror the contract exactly — the raw-type parameter carries the byref-like anti-constraint or silently excludes every span-keyed admission, and fixing the fault base in the constraint lets covariance serve a whole fault lattice while each owner declares its most precise case.
- Law: the success-arm null suppression is justified only by the generated contract; owners with null-yield modes break it deliberately and their bridge is three-valued — fault, absence, instance — projecting absence to the option, never the bang.
- Reject: bridging through `Create`, `TryCreate`, or the parse surface — the throw path string-flattens the typed fault, the try forms discard or merely re-annotate what `Validate` already returned, and the parse surface exists for framework binding; entering it from domain code voluntarily downgrades evidence.

[FAULT_FAMILIES]:
- Law: a fault family designs two tiers — one string-bearing case manufactured by the family's static `Create` for generator-authored text, and N structured cases hooks construct directly; a closed union-shaped family whose base derives from the expected-error record is simultaneously validation error, rail error, and exhaustively dispatchable recovery vocabulary, with stop-at overloads splitting severity tiers at the boundary.
- Law: fault identity has two relations — structural equality and code-keyed matching — and catch-style recovery goes through the code and type predicates, never equality; a family leaving codes at zero has silently opted into message-string matching, and domain codes allocate outside the runtime's reserved negative control band or impersonate cancellations to every generic handler upstream.
- Law: expectedness is conjunctive and exceptionality disjunctive across an aggregate — one exceptional infiltrator flips the whole disposition, so triage gates on exceptionality before reading fault cases; typed faults survive accumulation and union with foreign errors, the property that licenses the error base as universal failure currency.
- Law: typed faults round-trip exception-only channels with reference fidelity through the capture constructor, which also normalizes host cancellation and timeout into the reserved vocabulary; the wire contract serializes message and code only — structured payloads and inner-chain provenance are process-local, so codes must be self-sufficient for remote triage.
- Law: hooks own their exceptions — no factory path guards them, so a throwing hook detonates every admission surface including the nominally non-throwing forms; the error channel is the only safe failure path, and foreign factories that throw take the lazy capture monad as their one-hop projection.

[ACCUMULATION_ALGEBRA]:
- Law: applicative composition states independence and surfaces every fault; monadic composition states data dependency and short-circuits — choosing between them is a statement about the field dependency graph, not style. The generated spine never accumulates: independence between faults is reified as separate owners before the applicative bridge can surface them together.
- Law: accumulation lives in the failure type's algebra, not the traversal verb — applicative traversal over a fallible carrier with a non-semigroup failure type type-checks and silently keeps the first fault, and the miss is invisible until the first multi-failure input; the sound designs are the error base as currency or a semigroup on the fault family with an aggregate case.
- Law: layered admission is two-phase — leaf owners admit applicatively, the composite binds monadically on top because it can only fail on cross-member relations, the carrier's own join flattens between phases, and refinement threads as a guard with an explicit fault; filter and `where` manufacture the monoid identity as an evidence-free rejection no keyed recovery will ever match.
- Law: batch admission is one traversal whose verb and carrier select the semantics — the applicative verb under an accumulating carrier surfaces all faults, the monadic verb aborts at the first — derived from one fold, never two code paths.
- Law: the carrier lattice is lossless in both directions — collapsing the accumulator packs the aggregate intact into the sequential carrier's error slot, and lifting back resumes accumulation; direction states where accumulation continues, not what evidence survives.

[FALLBACK_AND_RECOVERY]:
- Law: alternative grammars compose with three distinct semantics — eager choice runs both admissions and keeps the left fault on total failure, the short-circuit operator pair defers the fallback grammar entirely, and semigroup combination is failure-dominant accumulation across independent grammars; deferred fallback is a trait-level lazy overload, never a hand-rolled branch.
- Law: failure-side rewrites stay on the rail — re-code in place, substitute a recovery admission, or fold both sides; recovery that pattern-matches mid-pipeline to re-wrap errors reimplements combinators the carrier already owns, and the predicate-gated catch vocabulary substitutes recovery admissions for matching faults without leaving the rail.
- Boundary: the sequential carrier's terminal spelling at the outermost host seam rethrows with original stacks for exceptional faults and code-bearing wrappers for expected ones — full fidelity in one line, only at the boundary.

[CONSTRAINT_PLANE]:
- Law: every generated owner implements one static-abstract factory contract, and vocabularies layer enumeration above it — one generic constraint owns admission sweeps, cache hydration, and startup probes across every owner kind, with the owner type itself as the typeclass instance and zero runtime witness; the relays shipped for expression trees and boxed dispatch are reused, never re-derived.
- Law: the minimal tier is collapse discipline applied to constraints — a projection-only algorithm constrained on the full vocabulary interface over-specifies and silently rejects every keyed value object that would have satisfied it; widening a bridge to the factory tier is what turns a vocabulary-specific bridge program-wide.
- Law: an admission arrow typed over the kinded carrier is the primary form — one arrow drives accumulating, short-circuiting, effectful, and transformer-stacked admission by the caller's carrier choice; the weakest sufficient trait is the correct constraint, rejection demands the fallible conjunction because a bare applicative cannot fail, and carrier migration is a named transformation, never unwrap-and-rewrap.
- Boundary: span admission is a separately constrained entry pinned to the span factory shape — the general key path's nullable form cannot host a byref-like type, so an algorithm spanning both declares two overloads by mechanics, not policy.

## [7]-[COMPOSITION]

[NESTED_ADMISSION]:
- Law: admission composes depth-first with a monotonic trust gradient — an aggregate of N nested owners admits at exactly N seams, each owning its slice; the outer factory validates only cross-member relations, because re-validating a nested member duplicates a proof the type already carries and couples the outer invariant to the inner rule.
- Law: the enclosing owner inherits member capabilities by structural composition — generated equality delegates to member equality, the factory initializes through admitted instances, and admission, comparer policy, and default-safety arrive free at the outer layer.
- Law: an owner keyed by another generated owner reaches its full derived surface only across an assembly boundary — a generator cannot observe another generator's output, so the same-compilation composition carries the raw key with a separate vocabulary owner and one two-hop admission expression: a priced trade against an unpriced capability downgrade whose fingerprint is the nullable-annotated generated `ToString`.
- Reject: flattening a nested aggregate into one wide owner — it discards every inner admission, comparer, and default-safety and re-scatters them as ad-hoc outer checks; the flattening pressure is usually a wire shape, and the move is one object factory projecting the aggregate to its grammar while the interior keeps the composed spine.

[INGRESS_AND_EGRESS]:
- Law: conversion direction is a typed gradient — owner-to-key implicit because erasing evidence is free, key-to-owner explicit because creating evidence validates, throw-capable conversions segregated under explicit or unsafe spellings; reconstructing an aggregate upward is a chain of explicit admissions that does not compile when spelled implicitly.
- Law: the key-to-owner cast is a hidden throwing admission — boundary-only spelling, erasable codebase-wide by configuration, and forced off when factories are skipped; on class owners with reference keys it null-propagates before admission, accepting an absence the factory would reject.
- Law: one accessibility knob token-gates the whole ingress surface — constructors, conversions, and factories together — and composes under nesting into a funnel private at every layer except the deliberately exposed verb; factory verbs rename to domain vocabulary on the owner, deleting the last excuse for a renaming wrapper.
- Law: skipping factories cascades through every derived capability — converter, factory contract, inbound conversion, parsing, arithmetic — leaving a pure interior value born only from other admitted values; the surviving equality, comparison, and egress surfaces are exactly the ones that never construct.

[IDENTITY_STACKING]:
- Law: identity has three regimes fixed by owner choice — key, structural, and case — and regimes stack without blending: structural equality at an outer layer dispatches into key equality beneath it through the member's own equality, and case identity compares runtime case before payload; no owner reaches through a member to its internals.
- Law: a mixed-kind case hierarchy mixes identity regimes per case silently — record cases value-equal, class cases reference-equal — so case-declaration form is an identity decision, not a syntax preference.
- Boundary: the equality-and-ordering pairing does not telescope — an under-specified inner member lets hashing and sorting disagree at that layer alone, invisible until exactly that member enters a sorted container; identity-without-order is a legal stance only declared at the layer that holds it.

[WIRE_OWNERSHIP]:
- Law: a wire contract earns a protocol DTO only when its topology diverges from the domain owner; a single-scalar wire shape deletes the DTO through an object factory, and a complex owner with a string factory collapses to a parse-format micro-grammar owned by the type. Wire capability defaults on for keyed owners — severing hands the wire to a DTO, leaving it deletes the DTO — and the decision is made at the owner declaration, never discovered at the edge.
- Law: the DTO that survives never grows a `Validate` — it is a record of raw values whose admission is the applicative composition of per-field bridges, keeping validation in exactly one place per field and the DTO disposable at the seam.
- Law: trusted rehydration is the single sanctioned no-validation path — a declared constructor for persistence materialization of already-admitted truth, rejected on vocabularies whose identity map cannot be bypassed, ignored by wire deserialization and binding, and scoped precisely to data that passed admission before storage.
- Boundary: the metadata plane — reified owner taxonomy, key projections as compiled delegates and expression trees, conversion routes filtered on delegate presence — belongs to serializer and persistence adapters; a domain-layer appearance of metadata dispatch marks a missing typed surface, and the weakly-typed bridge degenerates wrong-typed keys into zero-value or null admission, so only the typed plane makes mismatch unrepresentable.
