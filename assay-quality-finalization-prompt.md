# Assay Quality Finalization Prompt

## Goal

Fully finalize `tools/assay` as the agent-facing replacement for `tools/quality`.

The top goal is to ensure no planned or desired functionality is missing. Assay must preserve the useful capabilities of the old tool while improving integration, output quality, resilience, concurrency behavior, diagnostics, and code quality. Functionality such as `fsspec`, SSH/cloud artifact handling, automation, `psutil`, OpenTelemetry, and `structlog` must be automatic and integrated from the root of the tool, not exposed as weak, tacked-on modes or agent overhead.

The final result must identify and fix gaps in parity, output shape, runtime behavior, performance, artifacts, diagnostics, test coverage, external-library usage, and low-quality implementation patterns.

## Required Sub-Agents

Use 2 independent sub-agents to read all of `tools/quality` while you do the main task. They must read every Python file under `tools/quality` and the entire `tools/quality/README.md`.

Each `tools/quality` sub-agent must map:

- all functions, classes, modules, and code patterns
- all command capabilities and parity candidates
- the exact output and return structure optimized for agents
- diagnostics, errors, artifacts, and failure behavior
- lock/concurrency behavior, including multi-agent lock-file usage
- optimization and resilience features that must not be lost

Use the two reports independently as a cross-check so nothing valuable from `tools/quality` is missed.

After the initial inventory, send 1 sub-agent per external library imported by `tools/assay`. Each library agent must return:

- every import site
- every current capability used from that library
- advanced library capabilities that could strengthen Assay
- whether Assay fully leverages the library or only uses it superficially
- exact implementation recommendations with file and line references

Send 1 sub-agent to each declared external library in `pyproject.toml` that Assay does not currently use. Each unused-library agent must read `tools/assay` and identify whether that library can add real value, where it belongs, and how it should integrate without adding surface spam.

## Initial Evidence Capture

Run:

- `tree tools/assay`
- `forge-loc.sh tools/assay`

Capture the top 100 lines of each `tools/assay` file. Note every import and identify all external libraries in use.

Read `pyproject.toml` and compare declared Python dependencies against what `tools/assay` actually imports or should import.

## Source Reading Order

After dispatching the required agents, read every file under `tools/assay` line by line.

After that source read, read:

- `tools/assay/AGENTS.md`
- `tools/assay/README.md`

Use those files to understand the folder purpose, local refactoring mandate, tool behavior, and intended relationship between Assay and Quality.

## Command And Parity Audit

Run the commands for both tools.

Start with commands that have parity between `tools/quality` and `tools/assay`. For each parity command, compare:

- execution logic
- runtime and performance behavior
- output shape and token size
- diagnostics and error behavior
- artifact production
- truncation behavior
- resilience, locking, and multi-agent behavior
- fragility, hard-coding, or hanging risk

After all parity commands are checked, run the remaining Assay-only commands and record behavior, failures, output quality, and runtime risk.

Identify the exact output differences between both tools for every parity command. Determine which output is better for agents and whether Assay should change.

## Capability Integration Audit

Identify all functionality Assay has below the command surface. Focus on integrated value, not command names.

Trace and grade:

- `fsspec` and SSH/cloud artifact storage
- automation and scheduling
- agent-facing JSON envelopes and truncation behavior
- `.NET` command behavior under heavy concurrent agent usage
- OpenTelemetry integration
- `structlog` integration
- `psutil` diagnostics
- artifact storage, reads, writes, globs, and cleanup
- resilience, cancellation, timeout, and resource behavior
- registry and command output consistency

For every integrated capability, answer:

- Is it fully implemented where it belongs?
- Is it automatic and always-on where valuable?
- Does it maximize the approved external library?
- Does it produce useful agent-facing evidence without noise?
- Does it fail safely and consistently?
- What exact refactor or integration would improve it?

## Code Quality Audit

Identify low-quality code and refactor opportunities across `tools/assay`.

Find:

- unnecessary definitions, functions, classes, constants, enums, strings, and types
- repeated shapes that should collapse into one polymorphic owner
- stringly typed command, path, mode, or result handling
- rat-nesting and excessive branching
- flat code spam and weak single-use indirection
- functionality bolted on beside the root design instead of integrated into it
- dead or underused functionality
- low-density logic that can be reduced with Python 3.14+ features, functional pipelines, or `expression` patterns

Do not remove capability to reduce LOC. Preserve behavior through denser polymorphic owners, better object shapes, stronger folds, and integrated root-level surfaces.

## Tests And Proof Gaps

`tests/tools/assay` should already be partially done. Verify whether tests cover the intended capability surface.

Pay special attention to:

- SSH/cloud behavior for `fsspec`
- deterministic JSON envelope shape
- command parity contracts
- output truncation behavior
- automation behavior
- artifact-store reads, writes, globs, and cleanup
- failure envelopes and diagnostics
- concurrency and resource behavior

Identify missing tests, stale tests, weak mocks, and executable proof gaps.

## Final Deliverable

Produce a truthful, source-backed finalization plan for Assay.

The plan must identify:

- Full statement of understanding of `tools/assay/AGENTS.md` and all engineering rules
- Detailed tasks with file code changes necessary; truthful accounting of what/why is changing and validated logic flow to ensure correct location of change and optimized integration. 
- all behavior and functionality differences between `tools/quality` and `tools/assay`
- which old-tool capabilities are still valuable and must be preserved
- which Assay capabilities are real, partial, fragile, or missing
- exact output differences across parity commands
- external-library usage gaps and integration recommendations
- concrete code-quality refactors that reduce surface while preserving capability
- command fragility, hang risk, performance risk, and concurrency risk
- test and proof gaps that must be closed

Assay must end with ultra-advanced and powerful polymorphism, no fragile logic, no dead functionality, no underutilized approved external libraries, no unnecessary strings/types/constants, and no weak feature surfaces bolted onto the side of the tool.

---

# Follow-Up Refinement Prompt

Unnecessary LOC was added during the implementation pass. Critically judge that growth and refine the tool holistically to reduce LOC and surface area through real root-level refactoring, not by inlining blindly or removing whitespace/comments. Do not change code for the sake of churn; understand the full tool and improve logic, flow, ownership, and future extensibility.

The area of biggest STRUCTURAL refactoring is `tools/assay/composition` and often the location of LOC bloat and incorrect addition of new code (flat code spam) and a symptom pressure point; functionality need sto integrate properly prior at the right location, not the most convenient/immediatatly found critcally re-examine all content added + existing to restructure properly to ensure we stop going straight to the composition folder with flat code spam.

This is a full refinement and optimization pass across the tool and tests, not only the last implementation diff. Re-read the whole surface and identify where duplicated branches, flat code, constants, literals, enums, classes, loose models, and one-off shapes can collapse into singular rails, singular shapes, nested enums, richer stdlib typing capabilities, or advanced polymorphic/functionality owners. Preserve capability while reducing surface.

Critically review the plan, find gaps or missing implementations, and verify every remaining item truthfully. Rate code quality with a structured 1-10 scale after re-reading the files. Be critical but fair, and aim for 9.2+ quality across the full tool and test surface based on integrations, logic, flow, capability preservation, density, and maintainability.

IMPORTANT: After grading the files, identifying bad code, and finalizing corrections/improvements, distill high-value durable guidance into [AGENTS.md](tools/assay/AGENTS.md). Add only guidance that should govern future work in this folder.

Use `git diff` to see recently changed code, but do not limit the refinement to changed lines. The target is the best whole-folder design.

Read [README.md](tools/assay/README.md) and ensure the logic flow is true and accurate. Update it only if behavior, routing, or command truth changed.
Read [AGENTS.md](tools/assay/AGENTS.md), understand every local requirement, and apply this rule everywhere:

"- Refactor as if the capability was present from the first design: collapse duplicated branches into the owner rail, update tests at that owner boundary, and delete obsolete wrong-placement code in the same change."

## Required Sub-Agents

Use 10 sub-agents focused on `tools/assay` tool code. All 10 must load and follow [$coding-python](/Users/bardiasamiee/.codex/skills/coding-python/SKILL.md), plus:

- [effects.md](.claude/skills/coding-python/references/effects.md)
- [decorators.md](.claude/skills/coding-python/references/decorators.md)
- [types.md](.claude/skills/coding-python/references/types.md)

Tool-code agents must re-read every file under `tools/assay` and identify objective code-quality improvements:

- unified rails and owner boundaries
- singular polymorphic shapes replacing loose classes/types/constants/enums/literals
- advanced stdlib typing and Python 3.14+ capabilities replacing ad-hoc type plumbing
- nested enums or richer owner-local vocabulary where they reduce loose string handling
- algorithmically driven code over numeric or branch-heavy control flow
- removal of rat-nesting, stringiness spam, flat code, and single-use indirection
- deeper root-level integration of functionality currently tacked onto composition or rail edges
- LOC reduction through stronger design, not capability loss

Of those 10 tool-code agents, 5 must focus specifically on implementation finalization. They must identify every incomplete, partial, fragile, or incorrectly placed feature and give exact A-to-Z guidance for how to finish it properly from the root owner. They must also identify high-potential areas for new capability or extensions to existing capability, with guidance that preserves minimal surface, minimal new types, and root-level integration.

Use 10 sub-agents focused on test code. They must read [pyproject.toml](pyproject.toml), [tests/conftest.py](tests/conftest.py), [tests/tools/assay/conftest.py](tests/tools/assay/conftest.py), and every file in `tests/tools/assay`.

Test-code agents must pursue the same quality goal as the tool-code agents:

- denser, more adversarial tests
- fewer loose one-off tests with equivalent or stronger coverage
- shared fixtures and conftest owners that reduce repetition without hiding behavior
- algorithmically driven parametrization and law-style coverage
- no hand-rolled behavior already owned by pytest, Hypothesis, inline-snapshot, dirty-equals, time-machine, coverage, or other approved test tooling
- singular shapes and polymorphic assertions instead of repeated literal expected values
- reduced LOC and stronger proof through better test architecture

Of those 10 test-code agents, 5 must focus specifically on implementation finalization from the test side. They must identify missing proof for every partial feature, every weak or stale test, every place coverage can be made more adversarial, and every place a new or extended capability requires executable proof.

Send 4 additional sub-agents to aggressively research the external testing libraries actually used in [tests/conftest.py](tests/conftest.py), [tests/tools/assay/conftest.py](tests/tools/assay/conftest.py), and `tests/tools/assay`. They must identify how those libraries can heavily reduce LOC and strengthen proof through advanced APIs, fixtures, strategies, matchers, snapshot normalization, time control, coverage integration, or parametrization. Their recommendations must reduce ad-hoc later-added tests into fewer, better, algorithmically driven tests with singular shapes and polymorphic assertions.

## Refinement Deliverable

Produce and then execute a holistic refinement plan that:

- finalizes remaining implementation gaps
- reduces unnecessary LOC and surface area across tool and tests
- improves polymorphism, singular rails, and singular shapes
- removes loose constants, literals, enums, classes, and types where a better owner exists
- integrates advanced Python and stdlib type capabilities where they improve correctness and density
- extends valuable existing functionality without adding weak new surfaces
- adds new capability only when it fits the root design and has executable proof
- updates [AGENTS.md](tools/assay/AGENTS.md) with durable folder guidance after the work
- updates [README.md](tools/assay/README.md) only if source-backed behavior changed
