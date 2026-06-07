# [TESTS_BRIDGE_REFINER]

## [TRANSCRIPT]

Role: refinement / Tests Bridge Refiner for Rasm `AGENTS.md` work.

Scope constraints followed:
- No active files edited.
- Built on context reports instead of re-running broad inventory.
- Read the requested context reports:
  - `.reports/agents-md-050626/track-source-scans/12-tests-bridge-context.md`
  - `.reports/agents-md-050626/track-source-scans/11-root-integration.md`
  - `.reports/agents-md-050626/track-source-scans/05-frontier-instruction-practice.md`
  - `.reports/agents-md-050626/track-source-scans/06-poly-csharp.md`
- Read current active files:
  - `CLAUDE.md`
  - root `AGENTS.md`
  - `docs/standards/README.md`
  - `docs/standards/AGENTS.md`
  - `docs/standards/agents-md.md`
  - `docs/standards/explanation/test-strategy.md`
  - `tests/csharp/AGENTS.md`
  - `tests/csharp/libs/AGENTS.md`
  - `tools/rhino-bridge/AGENTS.md`
- Used memory only as orientation for known Rasm test/bridge failure modes, then verified recommendations against current repo files and context reports.
- Did not run static, test, bridge, mutation, fuzz, benchmark, docs validation, or link checks because this is a read-only refinement report.
- Will write this same report to `.reports/agents-md-050626/track-owner-routing/05-tests-bridge-refiner.md` after posting it in chat.

## [BASELINE_FROM_WAVE_1]

context already established the facts this pass should not re-litigate:
- Current test inventory supports high-density adversarial tests and real bridge scenarios, not smoke-check minimalism.
- `tests/csharp/AGENTS.md` already carries the core doctrine: adversarial laws, independent oracles, `Spec` rail helpers, `_testkit` promotion discipline, and no scenario `#r` / `#load` / absolute build paths.
- `tests/csharp/libs/AGENTS.md` already carries source-mirrored ownership, law density, bridge boundaries, and no skipped xUnit for bridge-owned gaps.
- `tools/rhino-bridge/AGENTS.md` already carries source-only scenarios, structured marker evidence, ephemeral artifacts, and bridge setup stop rules.
- `docs/standards/agents-md.md` already says `AGENTS.md` is a scoped behavioral overlay, not a command catalog, validation ladder, provider manual, session record, or research summary.
- The remaining problem is precision: make test-owned overlays a first-class `AGENTS.md` profile, then sharpen root and local overlays at the exact points where future agents can blur static specs, bridge runtime proof, synthetic probes, and raw rail-state checks.

## [GOVERNING_CONSTRAINTS]

- `CLAUDE.md:20-21` routes `.spec.cs` and `.verify.csx` through `testing-cs`.
- `CLAUDE.md:116-129` keeps static analysis, unit tests, and Rhino runtime verification as separate rails.
- Root `AGENTS.md:15-23` already routes C# tests, `.verify.csx`, bridge scenarios, testkit, bridge runtime, and test-tool APIs to the right overlays.
- Root `AGENTS.md:79-82` rejects root command catalogs, subtree implementation facts, bridge transcripts, and C# proof claims for docs-only instruction edits.
- `docs/standards/agents-md.md:32-41` defines produced `AGENTS.md` functions and bans generic validation sections.
- `docs/standards/agents-md.md:52-58` already has conditional functions for boundaries, tool authorization, trust boundaries, delegation, local vocabulary, and stop rules, but not a specific test-owning overlay contract.
- `docs/standards/explanation/test-strategy.md:174-175` already separates static/build, unit/property, mutation, fuzzing, benchmark, snapshot/visual, and runtime scenario proof. No test-strategy edit is needed for this refinement scope.
- `tests/csharp/AGENTS.md:42` already says direct rail peeking is supplemental only; `tests/csharp/AGENTS.md:47` rejects `.IsSucc`, `.IsFail`, `.IsSome`, and `.IsNone` as primary proof. The nuance should be made explicit enough to avoid review friction.
- `tools/rhino-bridge/AGENTS.md:14-20` already says bridge commands are only for host runtime behavior and not synthetic unit tests. The wording should clarify that source-owned `*.verify.csx` scenarios are runtime laws, while artificial bridge probes are still rejected.

## [PRIMARY_RECOMMENDATION]

Make the standard change first in `docs/standards/agents-md.md`, then let root and local overlay edits become small translations.

Recommended order:
1. Add a test-owning overlay conditional function to `docs/standards/agents-md.md`.
2. Add root-level test assertion classification triggers to root `AGENTS.md`.
3. Tighten `tests/csharp/AGENTS.md` around `.verify.csx`, classification before assertion, mutation-survivor triage, and raw rail-state checks.
4. Tighten `tests/csharp/libs/AGENTS.md` around sibling scenarios, bridge-owned runtime gaps, and owner-local law matrices.
5. Tighten `tools/rhino-bridge/AGENTS.md` around the bridge README tension: not synthetic unit tests, but source-owned host runtime laws.

This keeps command syntax in `CLAUDE.md`, tool READMEs, and `docs/testing-libs`; it keeps `AGENTS.md` focused on local selectors, owners, rejection replacements, and stop behavior.

## [SECTION_BY_SECTION_RECOMMENDATIONS]

### [1][DOCS_STANDARDS_AGENTS_MD]

File: `docs/standards/agents-md.md`

Placement 1: add one drafting question after `docs/standards/agents-md.md:15`.

Recommended wording:
```markdown
- If this folder owns tests, specs, scenarios, testkit, mutation, fuzz, benchmark, architecture checks, or runtime proof, which local proof rail owns each behavior and which fake proof shape must be rejected?
```

Reason:
- The current drafting questions ask about owner rails and route-away generally, but test overlays have a repeated special hazard: fake proof. A question here makes test proof classification part of authoring before section shaping.

Placement 2: add a conditional function after `docs/standards/agents-md.md:53`.

Recommended wording:
```markdown
- Test contract: required when the folder owns specs, scenarios, testkit, fuzz, benchmark, architecture checks, mutation, or host/runtime proof. It names the local proof rail, oracle source, runtime boundary, helper-promotion route, rejected fake-proof shapes, and stop behavior without listing runner commands.
```

Reason:
- This is the most important standard-level edit. It gives test-owning overlays a first-class slot while preserving the existing route-away rule that command syntax belongs outside `AGENTS.md`.

Placement 3: add a test-specific local extension grammar paragraph after `docs/standards/agents-md.md:89`.

Recommended wording:
```markdown
For test-owning overlays, write trigger-action-owner rules around proof classification. Accepted: Before adding a Rhino-facing assertion, classify the behavior as static-managed law or host-runtime scenario proof; put native success in a source-owned scenario and keep xUnit on guards, categories, receipts, and pure managed contracts. Rejected: Add a skipped xUnit test for Rhino behavior.
```

Reason:
- `agents-md.md` already has a code-owning accepted/rejected example. Tests need the equivalent because the wrong edit is not only "helper sprawl"; it is "proof in the wrong rail."

Placement 4: do not add a new generic validation section.

Reason:
- `docs/standards/agents-md.md:41`, `docs/standards/agents-md.md:60-63`, and `docs/standards/agents-md.md:104-107` already reject generic validation ladders and route tool command syntax away. The new test contract must not become a command catalog.

### [2][ROOT_AGENTS]

File: root `AGENTS.md`

Placement: add two bullets immediately after `AGENTS.md:16`.

Recommended wording:
```markdown
- Before adding or changing a test assertion, classify the behavior as static-managed law, bridge-owned runtime scenario, architecture/tooling/fuzz/benchmark rail, mutation triage, or proof gap.
- When behavior is bridge-owned, route proof to source-owned `*.verify.csx` work under `tests/csharp/libs/<Project>/<MirrorPath>/scenarios/` and read `tools/rhino-bridge/AGENTS.md`; do not replace it with skipped xUnit, mock-host proof, shape-only assertions, or documentation-only caveats.
```

Reason:
- Root already routes tests and bridge scenarios, but it does not yet trigger classification before assertion work. This is root-level because root-started agents may touch tests without starting inside `tests/csharp`.
- The wording is not a command catalog. It is a selector and rejection rule.

Do not add:
- `uv run python -m tools.quality test ...` syntax.
- Bridge command details.
- Scenario artifact paths.
- Test inventory counts.

Those belong to `CLAUDE.md`, `tools/quality/README.md`, `tools/rhino-bridge/README.md`, `docs/testing-libs`, or current repo proof.

### [3][TESTS_CSHARP_AGENTS]

File: `tests/csharp/AGENTS.md`

Placement 1: replace `tests/csharp/AGENTS.md:7`.

Current:
```markdown
[REQUIRED]: Follow `CLAUDE.md`, `testing-cs`, and `coding-csharp` for every `.spec.cs` change.
```

Recommended:
```markdown
[REQUIRED]: Follow `CLAUDE.md`, `testing-cs`, and `coding-csharp` for every `.spec.cs`, `.verify.csx`, and testkit change.
```

Reason:
- Root and `CLAUDE.md` already route `.verify.csx` through `testing-cs`; the local overlay should mirror that explicitly.

Placement 2: add one bullet after `tests/csharp/AGENTS.md:10`.

Recommended wording:
```markdown
- Before changing an assertion, scenario fact, or expected-value path, classify the behavior as static-managed law, bridge-owned runtime behavior, architecture/tooling/fuzz/benchmark rail, mutation-survivor triage, or proof gap.
```

Reason:
- This is the practical rule that preserves static-vs-bridge ownership before any test edit.

Placement 3: add one paragraph after `tests/csharp/AGENTS.md:23`.

Recommended wording:
```markdown
Mutation survivors are triage input. Classify each survivor as missing oracle, equivalent mutant, bridge-owned path, or product bug before editing tests.
```

Reason:
- This preserves adversarial density without turning mutation into blind assertion spam.

Placement 4: replace `tests/csharp/AGENTS.md:42`.

Current:
```markdown
- Use `Spec` rail helpers for `Fin`, `Validation`, `Option`, success, failure category, and diagnostics; direct rail peeking is supplemental only.
```

Recommended:
```markdown
- Use `Spec` rail helpers for `Fin`, `Validation`, `Option`, success, failure category, and diagnostics. Raw `.IsSucc`, `.IsFail`, `.IsSome`, and `.IsNone` checks are allowed only as secondary invariants after value, category, oracle, or diagnostic proof, or when rail-state policy is the subject under test.
```

Reason:
- This keeps the direct rail-peeking nuance: not banned in every context, but not allowed as primary proof.

Placement 5: replace `tests/csharp/AGENTS.md:47`.

Current:
```markdown
- No `.IsSucc`, `.IsFail`, `.IsSome`, or `.IsNone` as primary proof.
```

Recommended:
```markdown
- No raw rail-state checks as primary proof when `Spec` helpers, expected values, failure categories, diagnostics, or bridge facts can prove the behavior.
```

Reason:
- This aligns the rejection with the nuance above and avoids fighting legitimate secondary invariants.

### [4][TESTS_CSHARP_LIBS_AGENTS]

File: `tests/csharp/libs/AGENTS.md`

Placement 1: add one read-order bullet after `tests/csharp/libs/AGENTS.md:11`.

Recommended wording:
```markdown
- When a spec marks behavior `BRIDGE-DEFERRED` or touches RhinoCommon/GH2 host behavior, read sibling `scenarios/` where present before deciding whether the runtime gap is already owned.
```

Reason:
- context found static specs that deliberately classify native behavior as bridge-deferred. The overlay should make sibling scenario proof part of the edit path without requiring a new inventory.

Placement 2: replace `tests/csharp/libs/AGENTS.md:28`.

Current:
```markdown
Prefer one law-packed fact over several narrow facts when the same generated sample can attack closure, dispatch, unsupported outputs, failure categories, receipts, and invariants together.
```

Recommended:
```markdown
Prefer one owner-local law matrix or generated sweep over several narrow facts when the same sample can attack construction, closure, dispatch, unsupported outputs, failure categories, receipts, invariants, and an independent oracle together.
```

Reason:
- This keeps adversarial law density explicit and aligns with context's "dense laws beat fact sprawl" recommendation.

Placement 3: replace `tests/csharp/libs/AGENTS.md:36`.

Current:
```markdown
- New bridge-owned behavior: place source-only `*.verify.csx` scenarios under the relevant mirrored source path.
```

Recommended:
```markdown
- New bridge-owned behavior: add or update source-only `*.verify.csx` scenarios under the relevant mirrored source path, paired with the owning source/project through bridge routing; do not add manifests, scenario catalogs, `#r`, `#load`, or absolute build-output paths.
```

Reason:
- Strengthens scenario ownership without introducing command syntax or catalogs.

Placement 4: replace `tests/csharp/libs/AGENTS.md:42`.

Current:
```markdown
- Pair each bridge scenario with an owning source file for bridge checks.
```

Recommended:
```markdown
- Pair each bridge scenario with an owning source file or tight source concern so bridge routing can rebuild the real project and prove the runtime fact.
```

Reason:
- Avoids false precision when one scenario covers a tight source concern rather than exactly one file, while still preventing artificial probes.

Placement 5: add one sentence to `tests/csharp/libs/AGENTS.md:56`.

Recommended wording:
```markdown
Do not add a skipped xUnit test, mock-host assertion, or documentation-only caveat for behavior that belongs in a bridge scenario.
```

Reason:
- This is the local stop rule that preserves "no fake host proof."

### [5][TOOLS_RHINO_BRIDGE_AGENTS]

File: `tools/rhino-bridge/AGENTS.md`

Placement 1: add one paragraph after `tools/rhino-bridge/AGENTS.md:14`.

Recommended wording:
```markdown
The bridge is not a synthetic unit-test framework; it is the runtime proof rail for source-owned host behavior that static managed gates cannot execute.
```

Reason:
- This resolves the tension context identified: "not a unit-test framework" must not be misread as "do not write source-owned scenarios."

Placement 2: add one rejection after `tools/rhino-bridge/AGENTS.md:41`.

Recommended wording:
```markdown
- No artificial bridge probes without a real production source, host API, assembly freshness, document/canvas, native geometry, capture, resolver, or protocol fact to prove.
```

Reason:
- This is the bridge-side counterpart to "no fake host proof." It prevents using bridge as a high-cost substitute for unit tests.

Placement 3: replace `tools/rhino-bridge/AGENTS.md:31`.

Current:
```markdown
Author scenarios through the testkit scenario harness. Emit facts through the provided fact bag and rely on bridge markers as the structured wire contract.
```

Recommended:
```markdown
Author scenarios through the testkit scenario harness. Emit evidence through `Scenario.Run` and `FactBag`; parse structured bridge facts markers, not human-readable duplicate lines or run-local artifact paths.
```

Reason:
- This keeps evidence structured and prevents legacy stdout parsing or durable artifact-path claims.

Placement 4: keep `tools/rhino-bridge/AGENTS.md:37` as-is.

Reason:
- The current artifact rule is already good: bridge verification artifacts are run-local evidence and must not become durable documentation truth.

### [6][TEST_STRATEGY_STANDARD]

File: `docs/standards/explanation/test-strategy.md`

Recommendation: no active edit needed for this refinement scope.

Reason:
- `docs/standards/explanation/test-strategy.md:174-175` already states the key rail separation rule: static/build, unit/property, mutation, fuzzing, benchmark, snapshot/visual, and runtime scenario proof stay separate unless local policy proves one consumes another.
- `docs/standards/explanation/test-strategy.md:216-228` already names `host/runtime scenario` as a distinct rail class.
- The requested refinements belong in `agents-md.md` and local overlays, not the test-strategy type standard.

## [WORDING_PACKETS]

Use these as copy-ready packets during active edits.

### [AGENTS_MD_TEST_CONTRACT_PACKET]

```markdown
- Test contract: required when the folder owns specs, scenarios, testkit, fuzz, benchmark, architecture checks, mutation, or host/runtime proof. It names the local proof rail, oracle source, runtime boundary, helper-promotion route, rejected fake-proof shapes, and stop behavior without listing runner commands.
```

### [ROOT_TEST_TRIGGER_PACKET]

```markdown
- Before adding or changing a test assertion, classify the behavior as static-managed law, bridge-owned runtime scenario, architecture/tooling/fuzz/benchmark rail, mutation triage, or proof gap.
- When behavior is bridge-owned, route proof to source-owned `*.verify.csx` work under `tests/csharp/libs/<Project>/<MirrorPath>/scenarios/` and read `tools/rhino-bridge/AGENTS.md`; do not replace it with skipped xUnit, mock-host proof, shape-only assertions, or documentation-only caveats.
```

### [DIRECT_RAIL_PEEKING_PACKET]

```markdown
- Use `Spec` rail helpers for `Fin`, `Validation`, `Option`, success, failure category, and diagnostics. Raw `.IsSucc`, `.IsFail`, `.IsSome`, and `.IsNone` checks are allowed only as secondary invariants after value, category, oracle, or diagnostic proof, or when rail-state policy is the subject under test.
```

### [BRIDGE_RUNTIME_TRUTH_PACKET]

```markdown
The bridge is not a synthetic unit-test framework; it is the runtime proof rail for source-owned host behavior that static managed gates cannot execute.
```

### [NO_ARTIFICIAL_BRIDGE_PROBE_PACKET]

```markdown
- No artificial bridge probes without a real production source, host API, assembly freshness, document/canvas, native geometry, capture, resolver, or protocol fact to prove.
```

## [PLACEMENT_SUMMARY]

| [INDEX] | [FILE]                         | [SECTION]                      | [PLACEMENT]              | [ACTION]                                                         |
| :-----: | :----------------------------- | :----------------------------- | :----------------------- | :--------------------------------------------------------------- |
|   [1]   | `docs/standards/agents-md.md`  | `[1][USE_WHEN]`                | after line 15            | Add test-proof drafting question                                 |
|   [2]   | `docs/standards/agents-md.md`  | `[5][SECTION_CARDINALITY]`     | after line 53            | Add `Test contract` conditional function                         |
|   [3]   | `docs/standards/agents-md.md`  | `[7][LOCAL_EXTENSION_GRAMMAR]` | after line 89            | Add accepted/rejected test-proof example                         |
|   [4]   | root `AGENTS.md`               | `[1][READ_ORDER]`              | after line 16            | Add root assertion classification and bridge-owned proof trigger |
|   [5]   | `tests/csharp/AGENTS.md`       | `[1][READ_ORDER]`              | line 7 and after line 10 | Include `.verify.csx` and add classification before assertion    |
|   [6]   | `tests/csharp/AGENTS.md`       | `[2][TEST_CONTRACT]`           | after line 23            | Add mutation survivor triage                                     |
|   [7]   | `tests/csharp/AGENTS.md`       | `[4][ORACLE_RULES]`            | replace line 42          | Add direct rail-peeking nuance                                   |
|   [8]   | `tests/csharp/AGENTS.md`       | `[5][REJECTIONS]`              | replace line 47          | Align rejection with rail-peeking nuance                         |
|   [9]   | `tests/csharp/libs/AGENTS.md`  | `[1][READ_ORDER]`              | after line 11            | Read sibling scenarios for `BRIDGE-DEFERRED` or host behavior    |
|  [10]   | `tests/csharp/libs/AGENTS.md`  | `[3][LAW_DENSITY]`             | replace line 28          | Strengthen owner-local law matrix wording                        |
|  [11]   | `tests/csharp/libs/AGENTS.md`  | `[4][EXTENSION_GRAMMAR]`       | replace line 36          | Pair source-only scenario with owning source/project route       |
|  [12]   | `tests/csharp/libs/AGENTS.md`  | `[5][BRIDGE_BOUNDARIES]`       | replace line 42          | Let tight source concern count, but require real project proof   |
|  [13]   | `tests/csharp/libs/AGENTS.md`  | `[7][STOP_RULES]`              | after line 56            | Reject skipped xUnit, mock-host assertion, docs-only caveat      |
|  [14]   | `tools/rhino-bridge/AGENTS.md` | `[2][WHEN_TO_INVOKE]`          | after line 14            | Clarify source-owned runtime law role                            |
|  [15]   | `tools/rhino-bridge/AGENTS.md` | `[4][SCENARIO_KIT]`            | replace line 31          | Name `Scenario.Run`, `FactBag`, and structured facts markers     |
|  [16]   | `tools/rhino-bridge/AGENTS.md` | `[6][REJECTIONS]`              | after line 41            | Reject artificial bridge probes                                  |

## [RISKS_AND_GUARDS]

Risk: Root becomes a second test overlay.
Guard: root gets only classification triggers and route-away rules. Keep oracle details, mutation triage, direct rail peeking, and scenario ownership in test overlays.

Risk: `agents-md.md` grows test doctrine instead of authoring standard.
Guard: add only a conditional function and one accepted/rejected pattern. Do not copy Rasm-specific `_testkit` names into the standard except as examples if the active edit chooses to keep examples local.

Risk: Bridge wording could discourage valid scenarios.
Guard: use the two-part wording: bridge is not synthetic unit testing, but it is the runtime proof rail for source-owned host behavior.

Risk: Direct rail-peeking wording becomes too permissive.
Guard: allow raw `.Is*` checks only as secondary invariants after a real value/category/oracle/diagnostic proof, or when rail-state policy itself is under test.

Risk: Scenario pairing wording becomes too rigid.
Guard: allow one owning source file or one tight source concern. Reject manifests and catalogs, but do not force an exact one-file mapping when a scenario proves a cohesive owner.

## [NOT_RECOMMENDED]

Do not add:
- test command catalogs to root, `tests/csharp/AGENTS.md`, or `tools/rhino-bridge/AGENTS.md`;
- runtime artifact path examples as durable proof;
- current inventory counts to active overlays;
- provider manual summaries;
- fixed sub-agent counts;
- broad "run all gates" validation sections;
- compatibility language for skipped xUnit or fake host proof;
- a test-strategy edit for this scope.

## [CONFIDENCE]

High for placement and wording direction. The active files and context reports consistently support the same split: `agents-md.md` defines the test-owning overlay function, root triggers classification for root-started work, test overlays preserve adversarial law density, library overlays preserve source-owned scenario placement, and bridge overlay preserves real runtime proof without synthetic probes.

High for "no command catalogs" and "no fake host proof." These are directly supported by root rejections, `agents-md.md` route-away rules, test overlay rejections, and bridge overlay invariants.

Medium-high for direct rail-peeking wording. The current overlay already says direct rail peeking is supplemental only; the recommended wording makes the exception explicit. I did not line-review every existing `.IsSome` / `.IsNone` usage, so the wording is intentionally policy-level rather than a finding against specific specs.

No runtime confidence is claimed. This pass did not run static, unit, bridge, mutation, fuzz, benchmark, or docs validation gates.
