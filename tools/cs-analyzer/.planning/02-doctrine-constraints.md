# [02] Doctrine Constraints — Binding Table + Enforceable-Law Candidates

Distilled from the finalized 8-file doctrine corpus `docs/stacks/csharp/` (read in full): `README.md` (the atlas — 15 laws in 5 groups, the 9-row `COLLAPSE_SCAN`, `PAGE_CRAFT`, `CORPUS_LAW`, the planned `domain/` roadmap), `language.md` (C# 14 / net10 canonical-form choosers), `shapes.md` (owner chooser + ValueObject/SmartEnum/Union law), `surfaces-and-dispatch.md` (request-union collapse, modal arity, 5 dispatch forms, carrier-polymorphic dispatch), `rails-and-effects.md` (LanguageExt rail chooser, terminal collapse, traversal, schedule, receipts), `boundaries.md` (seam chooser, admission/sentinel projection, lifetime capsules, state cells), `algorithms.md` (route spine, admission gates, receipt law), `system-apis.md` (BCL ownership table). Cross-referenced against the existing 87-CSP surface to ground enforceability and surface drift. Evidence base: doctrine-file line cites below; existing-rule cites against `Kernel/RuleCatalog.cs`.

## [1]-[CRITICAL_DOCTRINE_DRIFT_THE_REBUILD_RESOLVES]

The existing catalog encodes OLD doctrine in two load-bearing places. Both resolutions are settled into doc 05's parity map.

- **TIME drift.** CSP0007 `WallClock` and CSP0714 `DateTimeFieldInDomain` mandate `NodaTime.IClock`/`NodaTime.Instant` (`RuleCatalog.cs:45,120`). Finalized doctrine mandates `TimeProvider`: `system-apis.md:148-152` names `TimeProvider`/`PeriodicTimer`/`Stopwatch.GetTimestamp` as the TIME owner; `rails-and-effects.md:161` puts `TimeProvider Clock` in the runtime record; `boundaries.md:136-138` HOST_MARSHAL uses the injected provider seam. The rebuilt time rule (CSP0007 survivor) bans `DateTime.Now`/`DateTimeOffset.Now`/`Stopwatch.StartNew`/`new Timer` and steers to `TimeProvider`/`PeriodicTimer`/`Stopwatch.GetTimestamp`, NOT NodaTime. NOTE (08 C1): because this FLIPS remediation polarity, the survivor cannot reuse the 0007 ID semantics for previously-compliant NodaTime sites — see doc 05 ID policy and doc 07.
- **Un-finalized framework rules.** CSP0008 `HttpClientConstruction`→`IHttpClientFactory`, CSP0402/0403 (FluentValidation), CSP0406 (Scrutor), CSP0604 (telemetry-identity), CSP0605 (hardcoded OTLP endpoint) cite frameworks/policy whose owning pages are PLANNED not finalized (`README.md` ATLAS rows 9-15 + ROADMAP). These encode un-finalized policy and are DROPPED until `domain/resilience.md`, `domain/validation.md`, `domain/diagnostics.md` finalize (doc 05 DROP set). 0605 additionally is the `Uri.TryCreate`-per-string-literal perf hazard (01 §6).

## [2]-[BINDING_CONSTRAINT_TABLE]

Each row: constraint → owning doc:section → enforceability verdict → existing rule(s). "ENFORCEABLE" = syntactic/symbol-graph decidable (the rule core); "JUDGMENT" = concept-density/semantic (advisory or no rule).

| [CONSTRAINT] | [OWNER] | [VERDICT] | [EXISTING] |
| ------------ | ------- | --------- | ---------- |
| `[ValueObject<T>]` owns invariant scalars; private key + `Validate` factory; no public ctor / `with`-bypass / init-only bypass | `shapes.md:9-22,58-62` | ENFORCEABLE | CSP0203/0713/0717/0720 |
| `[SmartEnum<TKey>]`: bounded vocab, `public static readonly` items, `[UseDelegateFromConstructor]` columns, generated total `Switch`; never language `switch` over items; `[Flags]`-with-no-bitwise → SmartEnum | `shapes.md:135-188`, ReplaceFlags:35 | ENFORCEABLE (Flags, repeated-Switch); JUDGMENT (vocab-owns-policy) | CSP0724/0734/0745 |
| `[Union]`: record/class root family-global; closure via private base ctor + sealed cases + `ConversionFromValue=None`; total Switch/Map default | `shapes.md:190-264`, surfaces:33 | ENFORCEABLE (closure, ops-qual, same-payload, manual-override); JUDGMENT (root choice, StopAt) | CSP0702/0802/0737/0740 |
| LanguageExt rails: narrowest carrier — `Option<T>` absence, `Fin<T>` sync fallibility, `Validation<E,T>` accumulation, `Eff<RT,T>` runtime, `IO<T>` deferred | `rails-and-effects.md:5-22` | ENFORCEABLE (effect-return, `Validation<Seq>` ban, Match-terminal, Run-in-transform); JUDGMENT (which carrier is narrowest-correct) | CSP0504/0703/0002/0705/0303 |
| EXPRESSION_SPINE: all domain logic expression-shaped; no `if`/`else`/`for`/`while`/statement-`switch`/`try`/`catch`; statements survive only in named ref-struct/span kernels with explicit Exemption | `README.md:34`; `language.md:241-246` | ENFORCEABLE | CSP0001/0009/0725/0706/0707 |
| BOUNDARY_ADMISSION: raw material admitted once into evidence-carrying owner; interior never re-validates / sees nulls/sentinels/provider shapes | `README.md:35`; `boundaries.md:22-53` | ENFORCEABLE (null-sentinel, ambient-state leak); JUDGMENT (absence-taxonomy cause-bearing→Union vs None) | CSP0104/0709/0723 |
| MODAL_ARITY: one entrypoint owns all modalities; discriminate on input shape; never name suffix / bool knob; `params ReadOnlySpan<T>` + request `[Union]` absorb | `README.md:40`; `surfaces:71-108` | ENFORCEABLE | CSP0005/0708/0729/0502/0726 |
| Primitive/BCL-collection/array leak in public domain signatures | `system-apis` SMELL rows 4-6; `shapes` OWNER_CHOOSER | ENFORCEABLE | CSP0003/0004/0201 |
| Mutable field / auto-property setter in domain types | `rails` REPRESENTATION; `language` IMMUTABLE_CARRIER | ENFORCEABLE | CSP0012/0202/0715/0720 |
| Factory return-type: `Create`/`CreateK`/`Validate` returns `Fin<T>`/`Validation<Error,T>`/`K<F,T>`, not raw T | `shapes:307`; `surfaces` KEY_ADMISSION | ENFORCEABLE | CSP0713/0504 |
| Single-use private helper / one-hop forwarding / single-impl interface | `README.md:54-56` | ENFORCEABLE (single-use, single-impl, alias-collapse); JUDGMENT (COMPOSED_IMPLEMENTATION "missing case") | CSP0503/0501/0733 |
| `Validation<Seq<Error>,T>` / `Validation<string,T>` monoid | `rails:123-128` | ENFORCEABLE | CSP0703 |
| `.Filter().Map()` → `.Choose`; mapped-then-`TraverseM(identity)` fusion | `rails:95,84` | ENFORCEABLE | CSP0710/0735 |
| `Atom<T>`/`Ref<T>` as property not static field | `rails` ATOM_STATE; state CELL_AND_THREAD | ENFORCEABLE | CSP0712 |
| Static `Regex.X()` / runtime `new Regex` → `[GeneratedRegex]`; regex charset → `SearchValues<T>` | `system-apis:24-28` | ENFORCEABLE (regex); HEURISTIC (charset shape) | CSP0606/0607/0704 |
| `Channel.CreateUnbounded` / bounded-without-FullMode in domain | `system-apis:182-185` | ENFORCEABLE | CSP0404/0405 |
| Same-payload union cases ≥3 → SmartEnum/kind-record | `shapes` MergeSamePayload:34 | ENFORCEABLE | CSP0737 |
| Switch-expression-as-arithmetic-operand precedence trap (C#14 parse hazard) | C#14 parse | ENFORCEABLE (zero FP, real bug provenance) | CSP0727 |
| `MapFail` discards `Try.lift` exception via `_` | `rails:39-44` | ENFORCEABLE | CSP0728 |
| TIME owner is `TimeProvider`/`PeriodicTimer`/`Stopwatch.GetTimestamp` (NOT NodaTime, NOT `DateTime.Now`) | `system-apis:148-152`; `rails:161`; `boundaries:136-138` | ENFORCEABLE (retargeted) | CSP0007/0714 (retired→09xx) |
| State-threaded dispatch / closed-union plan fusion / generic-math `IXxxOperators` | `README.md:43-47`; surfaces DISPATCH_AND_ROWS | ENFORCEABLE (state-threaded, plan-fusion, type-class); JUDGMENT (cases-share-generative-structure) | CSP0734/0744/0505 |

## [3]-[ENFORCEABILITY_PARTITION]

The single most important authoring law: a rule ships only if its predicate is syntactically/symbol-graph DECIDABLE. Judgment-class doctrine ships as Pressure/Info (with a decidable proxy) or gets NO rule.

MECHANICALLY ENFORCEABLE (the rule core) — the full set is the ENFORCEABLE rows above.

JUDGMENT / HEURISTIC (advisory/info or no rule):
- `ANTICIPATORY_COLLAPSE` / "shape for the family it will absorb" (`README:41`) — cannot detect "a second case is conceivable"; only the realized 3+-instance signal is decidable.
- `DEEP_SURFACES` "prefer one rich surface over four fragments" (`README:39`) — concept-density; the ≥3-parallel-types proxy is the only enforceable face (→ new CSP0906).
- `DERIVED_LOGIC` "cases share generative structure → fold/table" (`README:45`) — repeated-full-coverage-Switch (CSP0734/0744) is the decidable proxy; "shares generative structure" is judgment.
- `POLICY_VALUES` "boolean param selects between two bodies" (`surfaces:113-117`) — bool-param detection is syntactic but "selects between two bodies" needs body analysis; heuristic.
- `LIBRARY_DEPTH` "deepest operator the package reaches for" / "wrapper renames a package API" (`README:50`) — rename-wrapper is partially decidable (one-call forwarding); "deepest operator" is judgment.
- Carrier-narrowness "narrowest carrier that states the real outcome" (`rails:5-22`) — which carrier is "right" is semantic; only gross violations (raw exception escaping, `Either<Error,_>` where `Fin` fits) are decidable.
- Receipt-typing "keep typed receipt when fields carry solver/route/status evidence vs collapse to fact stream" (`rails:300-304`; CLAUDE.md) — CSP0730/0731/0732 encode the 3+-bucket proxy; the keep-vs-collapse DECISION is judgment and a known FP source (MEMORY.md: C4 "P0 bugs" verified false). Ships Pressure, structure-only (doc 05).
- Algorithm receipt provenance, witness-residual gates, ULP/epsilon policy (`algorithms.md` whole) — semantic numeric correctness, NOT analyzer-decidable. CLAUDE.md: "NEVER replace algorithm-specific proof receipts with generic `IReceipt`/ledger abstractions"; "Keep typed algorithm receipts when fields carry route/status/sampling/solver/spectral/mesh/extraction evidence." NO rules target this surface beyond the generic effect-return/imperative-flow rules.

## [4]-[PAGE_CRAFT_GAP_NEW_PAGE_MANDATED]

The atlas has NO page owning analyzer-authoring law. `README` PAGE_CRAFT:74-91 governs DOC authoring; CORPUS_LAW:93-102 governs corpus accretion; neither legislates how analyzer RULES are authored. `README` DOCTRINE:31 states "the doctrine authors the tool, never the reverse" and "analyzer findings are architecture pressure"; CLAUDE.md [4] says "Treat CSP analyzer diagnostics as hypotheses; refine the analyzer for false positives" and "rules describe semantic shapes and include positive AND negative tests for valid compact code." This rule-authoring contract lives ONLY in CLAUDE.md, not in the finalized corpus.

MANDATE (companion deliverable, the rebuild's first build step): author `docs/stacks/csharp/analyzer.md` owning (1) the enforceable-vs-judgment partition as a standing law; (2) the positive+negative-test mandate (every rule ships valid-compact-code negative tests); (3) the false-positive-is-a-rule-bug doctrine; (4) the severity tier (hard-error for decidable structural law, Pressure/Info for density heuristics); (5) the rule-is-not-coupled-to-namespaces constraint. The analyzer's `helpLinkUri` anchors target `analyzer.md#cspNNNN`; without the page the rebuilt analyzer re-derives authoring policy ad hoc — the exact anti-pattern the atlas exists to prevent.

## [5]-[REBUILD_POSTURE_NOTES]

- The `COLLAPSE_SCAN` (`README:58-72`) is the single most rule-mappable artifact: 9 rows, r1/r2/r5/r7/r8 decidable, r3/r4/r6/r9 heuristic; they map nearly 1:1 to the surface-area rule family (CSP05xx/07xx). Make it the analyzer's spine (doc 05).
- Severity calibration is the headline lesson from the old catalog: every rule is `Error` (`RuleCatalog.cs:26`). Doctrine treats findings as architecture pressure/hypotheses; MEMORY.md records multiple FP episodes (CSP0705 mid-pipeline, receipt rules). The rebuild splits hard-error (decidable structural) vs Pressure/Info (density heuristic); a uniform-Error catalog mis-signals to agents (doc 04 §TIERS).
- `system-apis.md` ownership: BCL replacements `Convert.ToHexString`, `Regex.Count`, `SearchValues<T>`, `TimeProvider`, throw-helper statics, `MemoryExtensions.Split`, `u8` literals, `FrozenDictionary`, `TensorPrimitives`; explicitly NON-replacing: LanguageExt rails, Thinktecture shapes, MathNet algorithms, Rhino geometry (`:3`). ENFORCEABLE there: regex (CSP0606/0607/0704), channels (CSP0404/0405); the rest are advisory smell-lookups — NO rules (judgment to apply a replacement vs not).
