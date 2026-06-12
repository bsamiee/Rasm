# [05] Rule-Catalog Parity — CARRY / COLLAPSE / DROP + New-Rule Recipe

Per-rule disposition for all 87 carried descriptors, with the corrected ID policy and arithmetic (08 C1/C2/C3 forced three corrections — all resolved here). Evidence base: the existing catalog (`Kernel/RuleCatalog.cs`), the doctrine partition (doc 02), and the red-team ID-policy lane (doc 08 §C). The final shape: **57 active rules derived from the 87 carried descriptors (48 Law + 9 Pressure), of which 2 Law take new CSP09xx IDs (0910/0911); + 6 net-new rules (0901-0906) = 63 total active; 10 dropped; 4 reserved (never-shipped) IDs.**

## [1]-[ID_STABILITY_POLICY] (08 C — the strongest red-team lane, resolved)

CSP#### is immutable once SHIPPED. Agent memory and docs reference IDs (MEMORY.md cites CSP0705, CSP0742). The policy:

1. **CARRY** keeps its ID and message semantics.
2. **COLLAPSE — message-compatible, strictly-narrower-or-equal merge:** survivor takes the LOWEST constituent ID; constituents tombstoned. (08 C2 CORRECTION: the original design violated "lowest constituent" once — `0725←0718` was written with survivor ID 0725, but 0718 < 0725, so the survivor is **0718**. Re-verified in §3 and §6: every reuse-survivor ID is now the numeric MINIMUM of its constituent set. `0606←0704` was already correct, 0606 < 0704.)
3. **COLLAPSE — semantic BROADENING or remediation-POLARITY FLIP:** a NEW CSP09xx ID, ALL constituents tombstoned, old→new map published in agent-readable form (`analyzer.md`) + MEMORY.md update. Survivor-ID reuse is FORBIDDEN here because previously-compliant call sites would start firing under the same ID and any existing suppression would silently suppress the NEW semantics.
4. **New rules** allocate CSP09xx; **retired IDs never reused.**

The two ID reuses the original design got wrong (08 C1), now fixed:
- **0002 ← 0705+0303** was a semantic BROADENING (today's 0002 is narrowly "Match is boundary-only"; absorbing 0705+0303 makes it a broad terminality rule). RESOLVED: the broad terminality rule is a NEW ID **CSP0910 TerminalCollapse**; 0002/0303/0705 all tombstoned. MEMORY.md's CSP0705 reference is updated to point at 0910.
- **0007 ← 0714+0401** FLIPS remediation polarity (NodaTime → TimeProvider). RESOLVED: the TimeProvider rule is a NEW ID **CSP0911 TimeDiscipline**; 0007/0401/0714 tombstoned. The TimeProvider retarget is doctrine-correct (doc 02 §1); only the ID reuse was wrong.

All other COLLAPSE survivors are message-compatible narrower-or-equal merges and keep the lowest constituent ID.

## [2]-[LAW_TIER_CARRY] (34 rules — verified count, 08 C2 CORRECTION)

(08 C2 flagged "33 listed, 34 IDs". Recount against the enumerated list below: it IS **34** distinct IDs. The original design's "33" was the double-count error; the corrected count is 34, and the §6 ledger balances on it.)

`0001` ImperativeControlFlow (syntax-kind only; partition 0001=if/else/loops/switch-stmt, 0009=throw/try/catch); `0005` OverloadSpam (preserve full exemption algebra `ShapeRules.cs:148-202` + XML doc); `0006` AsyncBlocking; `0009` ExceptionControlFlow; `0010` AsyncVoid; `0015` VarInference; `0301` FireAndForgetTask; `0503` SingleUsePrivateHelper (FIX counting: include `IMethodReferenceOperation` method-group refs, exclude recursion self-calls — `AnalyzerState.cs:74-82` flaws); `0504` EffectReturnPolicy; `0505` TypeClassStaticAbstract; `0506` ExtensionProjectionRequired (shape-only predicate, no name heuristics); `0601` HotPathLinq (gated on declared HotPath scope, killing the `PerformanceReport.cs` FP); `0701` PrimitiveShape; `0702` DuShape; `0703` ValidationType; `0706` EarlyReturnGuardChain (most-specific-wins: suppresses 0001 on same nodes); `0707` VariableReassignment; `0708` ApiSurfaceInflationByPrefix (prefix vocab from AdditionalFiles); `0710` FilterMapChainOnSeq; `0711` AsyncAwaitInEff; `0712` AtomRefAsProperty; `0713` CreateFactoryReturnType; `0723` AmbientHostStateLeak (RENAMED from RhinoActiveDocLeak — rule names carry no host/project tokens, same ID and message intent; ambient-state list from AdditionalFiles, project-agnostic); `0724` FlagsEnumOveruse (CompilationEnd — 08 B3, NOT SymbolStart); `0727` SwitchExpressionPrecedence (verbatim — zero FPs, real production-bug provenance); `0728` MapFailDiscardsException (keep strict `Try.lift→Run→MapFail` chain); `0729` OverloadAdjacency (FIX asymmetric seed exemption `ShapeRules.cs:269-280`; record-struct key replaces hand 397-prime hash; SymbolStart per-type); `0733` GeneratedCaseAliasCollapse; `0734` StateThreadedDispatch; `0735` TraverseFusion; `0736` FoldAppendAccumulator; `0737` SamePayloadUnionCases; `0739` GuardableFinConditional; `0740` ManualClosedUnionOverride (per-compilation union-case index built once, killing the O(types × namespace-tree) triple walk; 08 B5).

## [3]-[LAW_TIER_COLLAPSE] (survivors absorbing constituents)

Each survivor ID is the numeric MIN of its constituent set unless marked NEW-ID.

| [SURVIVOR] | [ABSORBS] | [PREDICATE] |
| ---------- | --------- | ----------- |
| `CSP0910` TerminalCollapse **(NEW ID — broadening, 08 C1)** | 0002+0705+0303 | one Match/IfFail/Run/RunAsync/.Value terminality rule on the `SymbolFacts.cs:469-486` terminal analysis; deletes dead `IsRegexMatchCall` (`FlowRules.cs:44-51`, 08 D3) and the split-brain double SwitchExpression pipeline |
| `CSP0003` SignatureLeak | 0003+0004+0201 | primitive\|BCL-collection\|array in public domain signature; offending kind in args |
| `CSP0911` TimeDiscipline **(NEW ID — polarity flip, 08 C1)** | 0007+0714+0401 | RETARGETED NodaTime→TimeProvider (doc 02 §1); bans `DateTime.Now`/`DateTimeOffset.Now`/`Stopwatch.StartNew`/`new Timer`, steers `TimeProvider`/`PeriodicTimer`/`Stopwatch.GetTimestamp` |
| `CSP0011` MutableCollectionConstruction | 0011+0204 | type set from AdditionalFiles |
| `CSP0012` MutableDomainState | 0012+0202 | setter\|field triggers |
| `CSP0014` UnboundedConcurrency | 0014+0302 | `Task.Run` \| unbounded `WhenAll` |
| `CSP0104` NullSentinel | 0104+0709 | binary comparison \| null pattern |
| `CSP0203` AdmissionBypass | 0203+0717+0720 | public-ctor \| with-expr/direct-construction \| init-only bypass on validated owner |
| `CSP0404` ChannelTopology | 0404+0405 | unbounded \| bounded-sans-FullMode |
| `CSP0502` NamedArguments | 0502+0726 | ≥3-param methods AND record primary ctors |
| `CSP0606` RegexConstruction | 0606+0704 | static `Regex.X` \| runtime `new Regex` (0606<0704, so 0606 survivor is correct; SYSLIB1045 covers only the constant-pattern subset, keep ours) |
| `CSP0718` ImperativeAccumulator | 0718+0725 | outer-var assignment \| `.Add` in loop; remediation arg Fold vs Choose (08 C2 CORRECTION: 0718<0725, so the survivor ID is **0718**, NOT 0725 as the original design wrote — original violated lowest-constituent here) |
| `CSP0742` ManualAdmissionGate | (rebuilt) | data-driven; fires ONLY when `CspBannedSymbols` admission section configured; silent otherwise |
| `CSP0802` UnionOpsQualification | (rebuilt) | attribute metadata names from `build_property`, decoupled from `Rasm.Domain`; generator extracted (doc 04 §8) |

## [4]-[PRESSURE_TIER] (9 rules — Warning, density heuristics)

`0013` ClosureCapture ← 0017+0602 (one rule; Warning default; HotPath projects escalate to error via their own `.editorconfig` — severity-by-scope falls out of standard config once `NotConfigurable` dies); `0501` InterfacePollution (per-compilation blindness to sibling-assembly implementors is unfixable in-process — demote to Warning, document; CompilationEnd); `0607` GeneratedRegexCharsetValidation (replace the char-by-char regex mini-parser `SymbolFacts.cs:338-380` with a bounded charset-shape recognizer; heuristic ⇒ Warning); `0719` UnsafeNumericConversion (narrowing is sometimes intended in geometry code); `0730` ReceiptDensity ← 0731+0732, REBUILT structure-only (≥3 sibling collection-typed buckets sharing element construction; ALL name-fragment classification — `OperationalReceiptNames`/`ProofReceiptFragments` `:231-240`, `MutationReceipt`/`DocumentReceipt` wrappers `:979-1012` — deleted; documented FP source, MEMORY.md C4 "P0 bugs" false); `0738` ExclusiveOptionalPayloadBag (same FP family); `0741` ForwardingRequestCaseFamily (threshold config `csp.CSP0741.case_threshold`, default 10 per Op+Result break-even memory); `0744` ClosedUnionPlanFusion (CompilationEnd); `0745` PassiveSiblingSurfaceFamily (COLLAPSE_SCAN r6/r9 are doctrine-classified heuristics).

## [5]-[DROP] (10 rules — owner page not finalized OR SDK/compiler owns it OR pure duplicate)

`0008` HttpClientConstruction, `0402`/`0403` FluentValidation, `0406` Scrutor — encode `domain/resilience.md`, `domain/validation.md` policy that is PLANNED not finalized (re-admit only when the owning page finalizes); `0604` TelemetryIdentity + `0605` HardcodedOtlpEndpoint — same un-finalized diagnostics page, and 0605 is the `Uri.TryCreate`-per-string-literal perf hazard (`RuntimeRules.cs:211-225`); `0603` LibraryImportRequired — SDK owns it: escalate SYSLIB1054 to error via `.globalconfig`; `0608` EnumeratorCancellationMissing — compiler owns it: escalate CS8425 via `WarningsAsErrors`; `0715` AnemicEntityDetection — every finding is a guaranteed co-fire of the 0012-survivor + 0203/0701, pure duplicate noise for agents; `0743` ManualGenericProjectionGate — predicate was never crisply statable, implemented via project-coupled owner names (`SymbolFacts.cs:651-654`), judgment-class per doctrine partition.

## [6]-[ARITHMETIC] (08 C2 — the corrected ledger)

The 87 carried descriptors partition EXHAUSTIVELY into seven mutually-disjoint buckets (verified: sum = 87, zero overlap, zero omission):

| [BUCKET] | [N] | [IDs] |
| -------- | :-: | ----- |
| Law CARRY (keeps own ID) | 34 | 0001 0005 0006 0009 0010 0015 0301 0503 0504 0505 0506 0601 0701 0702 0703 0706 0707 0708 0710 0711 0712 0713 0723 0724 0727 0728 0729 0733 0734 0735 0736 0737 0739 0740 |
| Law COLLAPSE survivor — reuses lowest constituent ID | 12 | 0003 0011 0012 0014 0104 0203 0404 0502 0606 0718 0742 0802 |
| Old narrow IDs whose semantics move to a NEW survivor ID (retired) | 6 | 0002 0303 0705 (→ CSP0910); 0007 0401 0714 (→ CSP0911) |
| Constituents ABSORBED into a reuse-survivor (retired) | 12 | 0004 0201 (→0003); 0204 (→0011); 0202 (→0012); 0302 (→0014); 0709 (→0104); 0717 0720 (→0203); 0405 (→0404); 0726 (→0502); 0704 (→0606); 0725 (→0718) |
| Pressure tier ACTIVE | 9 | 0013 0501 0607 0719 0730 0738 0741 0744 0745 |
| Constituents ABSORBED into a Pressure survivor (retired) | 4 | 0017 0602 (→0013); 0731 0732 (→0730) |
| DROP | 10 | 0008 0402 0403 0406 0603 0604 0605 0608 0715 0743 |
| **TOTAL** | **87** | exhaustive, disjoint |

Bucket-2 note: `0742`/`0802` sit in the reuse-survivor bucket with ZERO absorbed constituents — their predicates are REBUILT (data-driven / build-property-decoupled, §3) while ID and message intent are kept; do not hunt for absorbed IDs that do not exist.

**Active-rule tally:** Law active = 34 CARRY + 12 reuse-survivors + 2 new-id survivors (0910, 0911) = **48**. Pressure active = **9**. Net-new rules (0901-0906) = **6**. **Total active = 48 + 9 + 6 = 63.** Of the 63, **57 derive from the 87 carried descriptors** (48 Law + 9 Pressure) and **6 are net-new**; 2 of the 57 (0910/0911) carry new IDs.

**Retired (carried IDs that do NOT survive as themselves) = 32:** the 6 old-narrow-IDs + 12 reuse-absorbed + 4 pressure-absorbed + 10 DROP. All 32 were in the shipped `RuleCatalog.All`, so all 32 are genuinely-shipped retirees → Removed Rules table in Shipped.md (§7).

## [7]-[TOMBSTONES_AND_RESERVED_IDS] (08 C3 — the correction)

- **Tombstone (Removed Rules table in Shipped.md):** only genuinely-SHIPPED retirees — the **32** IDs in §6's retired set (6 old-narrow + 12 reuse-absorbed + 4 pressure-absorbed + 10 DROP). A Removed Rules tombstone REQUIRES a prior shipped entry; all 32 were in the shipped `RuleCatalog.All`, so the full Shipped history must first be authored (the empty 2-line `Shipped.md` is replaced with real release history).
- **Reserved (NOT tombstoned):** CSP0016, CSP0716, CSP0721, CSP0722 — verified NEVER shipped (0 source hits, empty Shipped.md; 08 C3). Tombstoning them would FABRICATE release history. They go in a `Catalog.Reserved` list + a meta-test asserting they are never reused, never emitted.
- Retired and reserved IDs are NEVER reused.

## [8]-[NEW_RULE_AUTHORING_RECIPE]

To add a rule (the corrected add-a-rule contract, 08 A1):
1. **Decidability gate** — confirm the predicate is syntactically/symbol-graph decidable (doc 02 §3). If judgment-class, it ships Pressure/Info with a decidable proxy, or not at all.
2. **Catalog row** — add `<ID>.Row` to `Catalog.All` (or a per-category trivial-row table for keyword bans).
3. **Rule file** — `Rules/<Category>/<ID>.<Name>.cs` exposing `internal static RuleRow Row { get; }`: descriptor (message in the §7-of-doc-04 grammar, `Description`=doctrine anchor, `helpLinkUri`=`analyzer.md#cspNNNN`), `Tier`, `ScopeGate`, `Bindings` (trigger + total `Check` delegate, `CancellationToken` threaded, no throw).
4. **Test file** — `tests/.../Rules/<ID>.<Name>Tests.cs`: ≥1 positive with `{|CSP####:span|}` markup, ≥1 negative valid-compact-code, one case per documented exemption clause.
5. **Unshipped row** — `AnalyzerReleases.Unshipped.md` (RS2000 fails the build otherwise; meta-test asserts it exists).
6. **Doc anchor** — `docs/stacks/csharp/analyzer.md#cspNNNN`.
7. **Data file** (if data-driven) — ship the `CspBannedSymbols.txt`/`CspPrefixVocabulary.txt` section IN the same change (08 F4).

False-positive doctrine: a false positive is a RULE BUG. Fix production code for true positives; refine the analyzer (predicate or scope gate) for false positives or fixes that add ceremony without improving correctness (CLAUDE.md [4]).

## [9]-[NEW_RULES_AT_REBUILD] (6 — mechanically enforceable, doctrine-grounded)

| [ID] | [NAME] | [TIER] | [TRIGGER] | [PREDICATE] | [GROUNDING] |
| :--: | ------ | :----: | --------- | ----------- | ----------- |
| CSP0901 | ScopeDeclarationIntegrity | Law | CompilationStart read; report at CompilationEnd, `Location.None` (DOCUMENTED span-doctrine deviation — a missing build property has no syntax node) | compilation lacking `build_property.CspScope` AND assembly `[CspScope]` ⇒ error; ALSO a data-driven rule enabled in non-tooling scope without its AdditionalFiles section ⇒ error (08 F4) | foundation of the scope model (doc 04 §4) |
| CSP0902 | SuppressionJustification | Law | CompilationStart config scan (`GlobalOptions` + `GetOptions(tree)` over `compilation.SyntaxTrees` for `dotnet_diagnostic.CSP*` keys); SymbolAction for attribute sites | `[SuppressMessage]` targeting CSP* without non-empty Justification ⇒ error; ALSO a Law-tier `dotnet_diagnostic.CSP*.severity` downgrade (none/suggestion) in any `.editorconfig` ⇒ error (08 F3). `#pragma` for CSP is BANNED (no Justification field; F3) — suppression is `[SuppressMessage]`-only | replaces NotConfigurable; the audited escape hatch |
| CSP0903 | BoundaryExemptionInventory | Info | SymbolAction on attributed symbols | one info per `[BoundaryAdapter]`/`[CspExempt]` site (non-console emission path, 08 F1) | deferred auditability rule (`RuleCatalog.cs:14-16`); spoofing countermeasure |
| CSP0904 | ExtensionMethodNaming | Law | SymbolAction (`SymbolKind.Method`, `IsExtensionMethod`) | extension method named `Get[A-Z]…` (projections are noun-named, complementing 0506) OR declared in a static class whose name ends `Ext` (canonical container suffix is `Extensions`) ⇒ error; message names the noun-projection rename | deferred backlog verbatim "forbid GetXyz / XyzExt prefix patterns on extension methods" (`RuleCatalog.cs:12-13`) |
| CSP0905 | ParallelEffectVariants | Pressure | SymbolStart per containing type (same-name member comparison is type-local) | ≥4 same-name members differing only in rail carrier (Eff/Fin/IO/Validation) ⇒ `K<F,A>` collapse | deferred backlog (`RuleCatalog.cs:8-11`); decidable signature comparison |
| CSP0906 | ParallelTypeFamily | Pressure | CompilationEnd (cross-TYPE accumulation; documented batch-only, same band as 0501/0724 — doc 04 §2.2) | ≥3 sibling types with identical member-shape sets modeling one concept | COLLAPSE_SCAN r7 decidable proxy; CLAUDE.md ≥3-parallel-types signal |

Everything else doctrine marks judgment-class (anticipatory collapse, deep-surfaces, carrier-narrowness, algorithm receipt provenance/epsilon) gets NO rule — `algorithms.md` surfaces are explicitly out of analyzer reach (doc 02 §3).

## [10]-[SCOPE_GATE_ASSIGNMENT] (the published Rule×Scope matrix — doc 04 §4's data table, settled here)

Default-gate algebra, then the EXHAUSTIVE exception list. The meta-test (doc 06 §5f) asserts every active rule carries exactly one gate; deviations discovered during the port are argued in the rule's `analyzer.md` anchor, never changed silently.

- **DEFAULT: `DomainOrApplication`** for every Law and Pressure rule not listed below — the bulk flow/shape/surface/rail band (doc 04 §4 names this the canonical gate). The default gate INCLUDES HotPath trees (`DomainOrApplication = Domain | Application | HotPath`, doc 04 §3): HotPath ADDS the perf band, it never exits domain discipline — otherwise marking `Rasm/Analysis` HotPath would strip it of the very enforcement 08 D2 preserved. This includes `0104` NullSentinel and `0723` ambient-host-state: both are INTERIOR rules — boundary code legitimately compares null/sentinels to project them (`boundaries.md` admission law) and legitimately touches host ambient state, so neither fires in `Boundary` scope.
- **`Everywhere` (all scopes incl. Test/Composition/Tooling-with-analyzer):** `0901` `0902` `0903` (integrity must hold wherever the analyzer runs) + `0727` SwitchExpressionPrecedence (a compiler parse trap is a real bug wherever it parses; zero-FP provenance).
- **`HotPath` only:** `0601` (kills the `PerformanceReport.cs` FP). `0013` ClosureCapture stays Pressure under the default gate; HotPath projects escalate it to error via their own `.editorconfig` (severity-by-scope, §4), NOT via gate.
- **`Boundary` only:** `0742` ManualAdmissionGate — admission gates live AT the boundary (doc 04 §4 "admission rules fire at the boundary"); interior code never re-validates (that is 0203's job under the default gate).
- **`Test`/`Composition` scopes** run ONLY the `Everywhere` band: test and wiring code is exempt from domain discipline by doctrine (test detection is now a DECLARED scope, not a path substring). `Tooling` projects normally skip the analyzer reference entirely (doc 04 §4 note, 08 D1).
- Per-tree `csp.scope` overrides re-assert the default band inside Boundary projects' pure kernels (`Rasm.Rhino` case, 08 D2) — the gate table needs no special rows for that; the override changes the SCOPE, not the gate.
