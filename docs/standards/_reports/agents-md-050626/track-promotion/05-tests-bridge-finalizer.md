# [TESTS_BRIDGE_FINALIZER]

Files read: `CLAUDE.md`, `docs/standards/README.md`, `docs/standards/AGENTS.md`, `docs/standards/agents-md.md`, `tests/csharp/AGENTS.md`, `tests/csharp/libs/AGENTS.md`, `tools/rhino-bridge/AGENTS.md`, `docs/standards/_reports/agents-md-050626/track-source-scans/12-tests-bridge-context.md`, `docs/standards/_reports/agents-md-050626/track-owner-routing/05-tests-bridge-refiner.md`, and `docs/standards/_reports/agents-md-050626/track-owner-routing/03-internalization-taxonomy.md`.

## [1][FINAL_POSITION]

Make proof classification the invariant that all four edits serve. The stronger rule is not "tests use bridge sometimes"; it is "no assertion, scenario fact, mutation reaction, or proof claim is valid until its rail is classified."

Final axis:
- Static specs own pure managed laws, guards, deterministic algorithms, failure categories, typed receipts, generated metadata, rail contracts, and pre-native rejection.
- Bridge scenarios own successful Rhino/GH host runtime behavior, document/canvas/UI state, native geometry/topology, capture, resolver freshness, and real assembly/package behavior.
- Mutation survivors are not a demand for more assertions; they are triage items.
- Raw rail state is not forbidden everywhere; it is forbidden as primary proof when a richer proof surface exists.
- Bridge output is not text. The durable scenario contract is marker-scanned JSON facts emitted through the scenario harness.

## [2][DOCS_STANDARDS_AGENTS_MD]

Target: `docs/standards/agents-md.md`.

Edit only the test-owning conditional rule; do not broaden `agents-md.md` into testing policy.

Add this bullet to `[5][SECTION_CARDINALITY]` under `[CONDITIONAL_FUNCTIONS]`, after `Tool authorization`:

```markdown
- Test contract: required when the folder owns specs, scenarios, testkit, fuzz, benchmark, architecture checks, mutation, or host/runtime proof. It classifies local proof rails before assertion, names acceptable oracle sources, separates static-managed laws from bridge-owned runtime proof, defines helper-promotion routes, rejects fake-proof shapes, and states mutation-survivor and unavailable-runtime stop behavior without listing runner commands.
```

Why this wording is final:
- "classifies local proof rails before assertion" is the parent standard hook.
- "without listing runner commands" prevents `AGENTS.md` from becoming `docs/testing-libs` or a tool README.
- "helper-promotion routes" keeps `_testkit` discipline in the AGENTS profile instead of letting "no helpers" become a slogan.

Do not add a separate test section, test taxonomy table, command ladder, or examples block here. The local overlays own the concrete grammar.

## [3][TESTS_CSHARP_AGENTS]

Target: `tests/csharp/AGENTS.md`.

### [3.1][READ_ORDER]

Replace the `[REQUIRED]` line with:

```markdown
[REQUIRED]: Follow `CLAUDE.md`, `testing-cs`, and `coding-csharp` for every `.spec.cs`, `.verify.csx`, and testkit change.
```

Add this read-order bullet after "When changing any C# test, read this file first.":

```markdown
- Before changing an assertion, scenario fact, expected-value path, or mutation response, classify the behavior as static-managed law, bridge-owned runtime behavior, architecture/tooling/fuzz/benchmark rail, mutation-survivor triage, or proof gap.
```

### [3.2][TEST_CONTRACT]

Add this paragraph after the existing expected-values paragraph:

```markdown
Proof classification comes before assertion. Static specs prove pure managed contracts, guards, deterministic algorithms, generated metadata, typed receipts, rail categories, and pre-native rejection; source-owned bridge scenarios prove successful Rhino/GH host runtime behavior, document/canvas/UI state, native geometry or topology, capture, resolver freshness, and assembly/package behavior that static xUnit cannot execute.
```

Add this paragraph immediately after it:

```markdown
Mutation survivors are triage input. Classify each survivor as missing oracle, equivalent mutant, bridge-owned path, or product bug before editing tests; strengthen a spec only after that classification says the surviving behavior is static-managed and missing a real oracle.
```

### [3.3][ORACLE_RULES]

Replace the current rail-helper bullet with:

```markdown
- Use `Spec` rail helpers for `Fin`, `Validation`, `Option`, success, failure category, and diagnostics. Raw `.IsSucc`, `.IsFail`, `.IsSome`, and `.IsNone` checks are allowed only as secondary invariants after value, category, oracle, diagnostic, receipt, or bridge-fact proof, or when rail-state policy is the subject under test.
```

Add this oracle bullet after the distinct-payload rule:

```markdown
- Treat bridge facts as oracle material only when they come from source-owned scenario evidence, not from human-readable stdout, run-local artifact paths, or a static imitation of the host.
```

### [3.4][REJECTIONS]

Replace the raw rail rejection with:

```markdown
- No raw rail-state checks as primary proof when `Spec` helpers, expected values, failure categories, diagnostics, receipt invariants, or bridge facts can prove the behavior.
```

Add these rejection bullets near the bridge-scenario rejection:

```markdown
- No skipped xUnit, mock host, headless substitute, shape-only host assertion, or documentation caveat as bridge-owned proof.
- No mutation-response assertion until the survivor is classified as missing oracle, equivalent mutant, bridge-owned path, or product bug.
```

### [3.5][STOP_RULES]

Replace the current stop-rule paragraph with:

```markdown
If a bridge run reports host assembly identity, staged-reference, marker parsing, fact envelope, or LanguageExt bootstrap failures, verify bridge setup before weakening the scenario or static spec. If a static spec would only pretend to execute native runtime behavior, route the claim to a source-owned bridge scenario or record a proof gap instead.
```

## [4][TESTS_CSHARP_LIBS_AGENTS]

Target: `tests/csharp/libs/AGENTS.md`.

### [4.1][READ_ORDER]

Add this bullet after "When changing a target spec, read the full spec first.":

```markdown
- When a spec marks behavior `BRIDGE-DEFERRED`, touches RhinoCommon/GH2 host behavior, or reacts to a bridge-owned mutation survivor, read sibling `scenarios/` where present before deciding whether the runtime gap is already owned.
```

### [4.2][OWNERSHIP]

Add this bullet after "Keep module-local generators in the spec until multiple specs need the same shape.":

```markdown
- Keep one-spec generators, fixtures, and assertion data local; promote only law, oracle, generator, serializer, or scenario capability that has multiple real consumers or removes repeated owner-local proof logic.
```

### [4.3][LAW_DENSITY]

Replace the final paragraph in `[3][LAW_DENSITY]` with:

```markdown
Prefer one owner-local law matrix or generated sweep over several narrow facts when the same sample can attack construction, closure, dispatch, unsupported outputs, failure categories, receipts, invariants, and an independent oracle together. Split only when the oracle statement, source owner, or triage identity changes.
```

Add this bullet under `[CRITICAL]`:

```markdown
- Classify bridge-deferred behavior before writing static assertions; static specs may prove guards, admission, unsupported outputs, receipts, and failure categories around a native path, but successful host execution belongs in a scenario.
```

### [4.4][EXTENSION_GRAMMAR]

Replace the bridge-owned behavior bullet with:

```markdown
- New bridge-owned behavior: add or update source-only `*.verify.csx` scenarios under the relevant mirrored source path, paired with the owning source file or tight source concern through bridge routing; do not add manifests, scenario catalogs, `#r`, `#load`, absolute build-output paths, skipped xUnit, or mock-host substitutes.
```

### [4.5][BRIDGE_BOUNDARIES]

Replace the section with:

```markdown
## [5][BRIDGE_BOUNDARIES]

- RhinoCommon or GH2 APIs that require RhinoWIP host state belong in bridge scenarios under the relevant mirrored source path.
- Static specs may classify bridge-owned behavior and prove managed guards, categories, receipts, or unsupported-output rails, but must not pretend to execute successful native runtime paths.
- Pair each bridge scenario with an owning source file or tight source concern so bridge routing can rebuild the real project and prove the runtime fact.
- Record bridge-owned gaps as executable scenario work or explicit proof gaps, not skipped xUnit tests, mock-host assertions, shape-only host checks, or documentation caveats.
- Bridge scenario facts must be emitted through the scenario harness and fact bag; do not parse raw stdout, duplicate human-readable lines, or run-local artifact paths as proof.
```

### [4.6][STOP_RULES]

Add this sentence to the end of `[7][STOP_RULES]`:

```markdown
Do not weaken a production contract or add static fake proof until the matching source owner and any sibling scenario evidence have been inspected.
```

## [5][TOOLS_RHINO_BRIDGE_AGENTS]

Target: `tools/rhino-bridge/AGENTS.md`.

### [5.1][WHEN_TO_INVOKE]

Add this paragraph after the opening sentence:

```markdown
The bridge is not a synthetic unit-test framework; it is the runtime proof rail for source-owned host behavior that static managed gates cannot execute.
```

Replace the final sentence in `[2][WHEN_TO_INVOKE]` with:

```markdown
Do not invoke bridge rails for synthetic unit tests, coverage probes, static-managed assertions, normal managed compile cleanup, Rhino settings automation, or long-running UI-thread experiments that the host cannot cancel safely.
```

### [5.2][SCENARIO_KIT]

Replace the whole section with:

```markdown
## [4][SCENARIO_KIT]

Author scenarios through the testkit scenario harness. Emit evidence through `Scenario.Run` and `FactBag`; parse structured bridge facts markers, not human-readable duplicate lines or run-local artifact paths.

Use marker scanning for returned envelopes and evidence facts. Do not parse individual legacy `key=value` lines; human-readable duplicates are noise, not the contract.
```

### [5.3][REJECTIONS]

Add these rejection bullets:

```markdown
- No artificial bridge probes without a real production source, host API, assembly freshness, document/canvas, native geometry, capture, resolver, package, or protocol fact to prove.
- No claim that a bridge run proved behavior unless the scenario emitted structured facts through the marker contract.
```

### [5.4][STOP_RULES]

Replace the first sentence with:

```markdown
If bridge bootstrap, host assembly identity, staged reference, lock drift, marker parsing, fact-envelope shape, or LanguageExt loading fails, fix setup or report the proof gap before changing scenario semantics.
```

Keep the package/deploy/publish sentence as-is.

## [6][WEAK_FORMULATIONS_TO_REJECT]

Reject these because they leave the wrong proof rail available:

| [WEAK_FORMULATION]                         | [REPLACEMENT]                                                                                                                                                                                                      |
| :----------------------------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| "Bridge scenarios cover runtime behavior." | "Classify behavior first; successful Rhino/GH host execution belongs in source-owned `*.verify.csx` scenarios, while xUnit stays on managed guards, categories, receipts, and deterministic laws."                 |
| "Do not use `.IsSucc` or `.IsSome`."       | "Raw rail-state checks are secondary invariants unless rail state is the contract under test; primary proof uses `Spec` helpers, value/category oracles, diagnostics, receipt invariants, or bridge facts."        |
| "Add more tests for mutation survivors."   | "Classify each survivor as missing oracle, equivalent mutant, bridge-owned path, or product bug before editing tests."                                                                                             |
| "Use bridge evidence."                     | "Use facts emitted through `Scenario.Run` and `FactBag`, parsed from structured bridge markers; do not parse human-readable stdout or run-local artifact paths."                                                   |
| "No helpers."                              | "Keep one-spec data local; promote only law, oracle, generator, serializer, or scenario capability with multiple real consumers or repeated proof logic."                                                          |
| "Static tests should not call Rhino."      | "Static specs may classify bridge-owned behavior and prove managed guards, admission, unsupported outputs, receipts, and categories around native paths; successful native execution belongs in bridge scenarios." |
| "The bridge is not a unit-test framework." | "The bridge rejects synthetic probes, but it is the runtime proof rail for source-owned host behavior static managed gates cannot execute."                                                                        |
| "Document the bridge gap."                 | "Create executable source-owned scenario work or record an explicit proof gap; never use skipped xUnit, mock-host assertions, shape-only checks, or documentation caveats as proof."                               |

## [7][FINAL_EDIT_ORDER]

Apply in this order:
1. `docs/standards/agents-md.md`: add the test contract conditional function.
2. `tests/csharp/AGENTS.md`: add classification, mutation triage, rail-peeking nuance, and no fake bridge proof.
3. `tests/csharp/libs/AGENTS.md`: tighten owner-local density, sibling scenario inspection, bridge boundaries, and helper promotion.
4. `tools/rhino-bridge/AGENTS.md`: tighten bridge invocation, marker/fact parsing, artificial-probe rejection, and fact-envelope stop rules.

This avoids duplicating command syntax and keeps the global standard abstract while local overlays carry the actionable rails.
