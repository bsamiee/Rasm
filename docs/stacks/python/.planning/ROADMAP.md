# [PYTHON_STACK_ROADMAP]

[ACTIVE] Feature build for `docs/stacks/python/`: first complete the core Python coding doctrine that prevents low-quality flat code, then promote testing proof rails, then platform graph truth. Current position: T-0020 is active in P-0010 under M-0010. Progress basis: 3/24 tasks. Proof route: source review and Markdown validation. Live planning route: Markdown-controlled.

## [1]-[CURRENT_POSITION]

State: [ACTIVE]
Sequence type: Feature build
Current focus: T-0020 in P-0010 under M-0010.
Progress basis: 3/24 tasks; counted only when task `Status` is `[COMPLETE]` and proof agrees.
Progress: [███░░░░░░░░░░░░░░░░░] 13%
Proof route: source review and Markdown validation.
Live planning route: Markdown-controlled.

## [2]-[STRUCTURAL_COMPONENTS]

Roadmap IDs are scope-local. Milestones use `M-0010`, phases use `P-0010`, and tasks use `T-0010`; only task rows carry lifecycle status.

Task completion requires the target file to exist, the README chooser to route to it when current, planned architecture to match, and proof to agree.

## [3]-[BOUNDARIES]

This roadmap owns the Python stack documentation buildout sequence only. Current language decisions live in [language](../language.md), stack orientation lives in [README](../README.md), local agent behavior lives in [AGENTS](../AGENTS.md), and planned topology lives in [architecture](ARCHITECTURE.md).

The first milestone is core coding doctrine only: language, PEP routing, dense value shapes, dispatch surfaces, rails, boundaries, runtime, and algorithms. Testing is the second milestone. Platform and package graph truth are the third milestone.

[SEQUENCING_RULES]:
- Repair `language.md` before creating `pep-standards.md`; the PEP index must map to stable owner boundaries instead of compensating for a confused language page.
- Create `pep-standards.md` as an index only. It lists PEP, Python version, owner, agent action, and superseded practice; it does not carry PEP prose or concept-page doctrine.
- Create `data-shapes.md` before `surfaces-and-dispatch.md` so decorators and registries can depend on stable object and payload law.
- Create `boundaries.md` before `runtime.md` so dynamic behavior and external protocols have an owner before runtime policy depends on them.
- Keep core coding doctrine as flat root concept pages until a category has multiple peer leaves and a chooser.
- Create the `testing/` folder only in M-0020, after core doctrine can define what proof must protect.
- Create the `platform/` folder only in M-0030, after core doctrine and testing have stable owner routes.
- Keep `.planning/SPEC.aot-first-decorator.md` as source material until all referenced rules have active owners.
- For every phase from P-0020 onward, complete the phase research/spec task before creating the phase's owner pages; page-creation tasks use the resulting spec sheet as their reference material.

## [4]-[ACTIVE_WORK]

Milestone:
- ID: M-0010
- Outcome: Core Python coding doctrine exists for advanced values, surfaces, rails, boundaries, runtime, and algorithms without category folders or package-shaped topology.
- Completion rule: all child tasks are `[COMPLETE]`, and required handoffs are closed or routed away.
- Progress basis: complete child tasks over all tasks in child phases.
- Progress: [█████░░░░░░░░░░░░░░░] 23%
- Phases:
    - P-0010: Foundation decision surfaces.
        - Scope: `docs/stacks/python/README.md`, `docs/stacks/python/AGENTS.md`, `docs/stacks/python/language.md`, `docs/stacks/python/.planning/`.
        - Completion rule: all child tasks are `[COMPLETE]`.
        - Progress basis: complete child tasks over this phase's tasks.
        - Progress: [████████████░░░░░░░░] 60%
        - Tasks:
            - [x] T-0010 [COMPLETE] Establish Python stack chooser and overlay routing
                - Work: establish Python stack chooser and overlay routing.
                - Target: `docs/stacks/python/README.md`, `docs/stacks/python/AGENTS.md`.
                - Exit: stack chooser and overlay routing are established.
                - Proof required: source review and Markdown validation.
            - [ ] T-0020 [ACTIVE] Rewrite `language.md` as the version-feature document for Python 3.10 through 3.15
                - Work: rewrite `language.md` as a concise Python 3.10-3.15 feature surface, prioritizing Python 3.15 and then Python 3.14.
                - Target: `docs/stacks/python/language.md`.
                - Exit: `language.md` is stable enough for `pep-standards.md` to map to owner boundaries.
                - Proof required: source review and Markdown validation.
            - [x] T-0030 [COMPLETE] Align planning topology to the flat core concept-page set
                - Work: align planning topology to the flat core concept-page set.
                - Target: `docs/stacks/python/.planning/ARCHITECTURE.md`.
                - Exit: planned core files match the planned view in `.planning/ARCHITECTURE.md`.
                - Proof required: source review and Markdown validation.
            - [x] T-0040 [COMPLETE] Capture AOT-first and decorator-first planning
                - Work: capture AOT-first and decorator-first planning in `.planning/SPEC.aot-first-decorator.md`.
                - Target: `docs/stacks/python/.planning/SPEC.aot-first-decorator.md`.
                - Exit: `.planning/SPEC.aot-first-decorator.md` exists as source material until all referenced rules have active owners.
                - Proof required: source review and Markdown validation.
            - [ ] T-0050 [QUEUED] Create `pep-standards.md` as a compact PEP-to-owner decision index
                - Work: create `pep-standards.md` as a compact PEP-to-owner decision index.
                - Target: `docs/stacks/python/pep-standards.md`.
                - Depends on: T-0020.
                - Exit: `pep-standards.md` exists, and the README chooser reaches it.
                - Proof required: source review and Markdown validation.
    - P-0020: Core modeling and composition doctrine.
        - Scope: `docs/stacks/python/.planning/SPEC.core-modeling-composition.md`, `docs/stacks/python/data-shapes.md`, `docs/stacks/python/surfaces-and-dispatch.md`, `docs/stacks/python/rails-and-effects.md`, `docs/stacks/python/boundaries.md`.
        - Depends on: P-0010.
        - Completion rule: all child tasks are `[COMPLETE]`.
        - Progress basis: complete child tasks over this phase's tasks.
        - Progress: [░░░░░░░░░░░░░░░░░░░░] 0%
        - Tasks:
            - [ ] T-0060 [QUEUED] Produce the core modeling and composition spec sheet
                - Work: run ten bounded sub-agent research lanes for data shapes, dispatch surfaces, rails, and boundaries; fold research, critique, dry-run content, and candidate wording into one focused spec sheet.
                - Target: `docs/stacks/python/.planning/SPEC.core-modeling-composition.md`.
                - Depends on: T-0050.
                - Exit: the spec sheet defines the handoff for data shapes, surfaces and dispatch, rails and effects, and boundaries.
                - Proof required: source review and Markdown validation.
                - Handoff: phase owner-page tasks use the spec sheet as reference material.
            - [ ] T-0070 [QUEUED] Create `data-shapes.md`
                - Work: create `data-shapes.md` for objects, records, enums, immutable data, sentinels, payloads, and chained shape nesting.
                - Target: `docs/stacks/python/data-shapes.md`.
                - Reference material: `SPEC.core-modeling-composition.md#data-shapes`.
                - Depends on: T-0060.
                - Exit: `data-shapes.md` owns the planned data-shape decisions.
                - Proof required: source review and Markdown validation.
            - [ ] T-0080 [QUEUED] Create `surfaces-and-dispatch.md`
                - Work: create `surfaces-and-dispatch.md` for decorators, typed registries, pattern dispatch, and polymorphic surfaces.
                - Target: `docs/stacks/python/surfaces-and-dispatch.md`.
                - Reference material: `SPEC.core-modeling-composition.md#surfaces-and-dispatch`.
                - Depends on: T-0070.
                - Exit: `surfaces-and-dispatch.md` owns the planned surface and dispatch decisions.
                - Proof required: source review and Markdown validation.
            - [ ] T-0090 [QUEUED] Create `rails-and-effects.md`
                - Work: create `rails-and-effects.md` for `Option` / `Result`, error transport, effect composition, recovery, retry, and resource rails.
                - Target: `docs/stacks/python/rails-and-effects.md`.
                - Reference material: `SPEC.core-modeling-composition.md#rails-and-effects`.
                - Depends on: T-0080.
                - Exit: `rails-and-effects.md` owns the planned result and effect decisions.
                - Proof required: source review and Markdown validation.
            - [ ] T-0100 [QUEUED] Create `boundaries.md`
                - Work: create `boundaries.md` for validation, codecs, structured templates, external text, subprocess/sql/shell sinks, dynamic import, and dynamic execution.
                - Target: `docs/stacks/python/boundaries.md`.
                - Reference material: `SPEC.core-modeling-composition.md#boundaries`.
                - Depends on: T-0060.
                - Exit: `boundaries.md` owns the planned boundary decisions.
                - Proof required: source review and Markdown validation.
    - P-0030: Core runtime and algorithm doctrine.
        - Scope: `docs/stacks/python/.planning/SPEC.runtime-algorithms.md`, `docs/stacks/python/runtime.md`, `docs/stacks/python/algorithms.md`.
        - Depends on: P-0020.
        - Completion rule: all child tasks are `[COMPLETE]`.
        - Progress basis: complete child tasks over this phase's tasks.
        - Progress: [░░░░░░░░░░░░░░░░░░░░] 0%
        - Tasks:
            - [ ] T-0110 [QUEUED] Produce the runtime and algorithms spec sheet
                - Work: run ten bounded sub-agent research lanes for runtime behavior, concurrency, scheduling, import cost, algorithms, traversal, parsing, and scientific-array boundaries; fold research, critique, dry-run content, and candidate wording into one focused spec sheet.
                - Target: `docs/stacks/python/.planning/SPEC.runtime-algorithms.md`.
                - Depends on: P-0020.
                - Exit: the spec sheet defines the handoff for runtime and algorithms.
                - Proof required: source review and Markdown validation.
                - Handoff: phase owner-page tasks use the spec sheet as reference material.
            - [ ] T-0120 [QUEUED] Create `runtime.md`
                - Work: create `runtime.md` for import-time evaluation, annotation cost, startup, concurrency, free-threading, JIT, scheduling, process lifetime, and resource ownership.
                - Target: `docs/stacks/python/runtime.md`.
                - Reference material: `SPEC.runtime-algorithms.md#runtime`.
                - Depends on: T-0110.
                - Exit: `runtime.md` owns the planned runtime decisions.
                - Proof required: source review and Markdown validation.
            - [ ] T-0130 [QUEUED] Create `algorithms.md`
                - Work: create `algorithms.md` for traversal, folds, vectorized work, complexity, ordering, data-structure strategy, parsing trees, and scientific-array boundaries.
                - Target: `docs/stacks/python/algorithms.md`.
                - Reference material: `SPEC.runtime-algorithms.md#algorithms`.
                - Depends on: T-0110.
                - Exit: `algorithms.md` owns the planned algorithm and data-structure decisions.
                - Proof required: source review and Markdown validation.

Milestone:
- ID: M-0020
- Outcome: Python testing proof rails exist after core doctrine defines the behavior to protect.
- Completion rule: all child tasks are `[COMPLETE]`, and required handoffs are closed or routed away.
- Progress basis: complete child tasks over all tasks in child phases.
- Progress: [░░░░░░░░░░░░░░░░░░░░] 0%
- Phases:
    - P-0040: Testing folder promotion.
        - Scope: `docs/stacks/python/.planning/SPEC.testing-proof-rails.md`, `docs/stacks/python/testing/`.
        - Depends on: M-0010.
        - Completion rule: all child tasks are `[COMPLETE]`.
        - Progress basis: complete child tasks over this phase's tasks.
        - Progress: [░░░░░░░░░░░░░░░░░░░░] 0%
        - Tasks:
            - [ ] T-0140 [QUEUED] Produce the testing proof-rails spec sheet
                - Work: run ten bounded sub-agent research lanes for managed laws, property proof, model checks, coverage, mutation, snapshots, benchmarks, async proof, process proof, and parser proof; fold research, critique, dry-run content, and candidate wording into one focused spec sheet.
                - Target: `docs/stacks/python/.planning/SPEC.testing-proof-rails.md`.
                - Depends on: M-0010.
                - Exit: the spec sheet defines the handoff for the testing chooser and proof-rail leaves.
                - Proof required: source review and Markdown validation.
                - Handoff: phase owner-page tasks use the spec sheet as reference material.
            - [ ] T-0150 [QUEUED] Create `testing/README.md`
                - Work: create the Python proof-rail chooser for managed laws, evidence rails, and specialized rails.
                - Target: `docs/stacks/python/testing/README.md`.
                - Reference material: `SPEC.testing-proof-rails.md#testing-chooser`.
                - Depends on: T-0140.
                - Exit: `testing/README.md` routes Python proof questions without copying command catalogs.
                - Proof required: source review and Markdown validation.
            - [ ] T-0160 [QUEUED] Create `testing/managed-laws.md`
                - Work: create `managed-laws.md` for pytest, Hypothesis, property laws, model-based checks, fixtures, and assertion shape.
                - Target: `docs/stacks/python/testing/managed-laws.md`.
                - Reference material: `SPEC.testing-proof-rails.md#managed-laws`.
                - Depends on: T-0150.
                - Exit: `managed-laws.md` owns Python static law and property proof choices.
                - Proof required: source review and Markdown validation.
            - [ ] T-0170 [QUEUED] Create `testing/evidence-rails.md`
                - Work: create `evidence-rails.md` for coverage, mutation, snapshots, benchmark evidence, and deterministic artifact proof.
                - Target: `docs/stacks/python/testing/evidence-rails.md`.
                - Reference material: `SPEC.testing-proof-rails.md#evidence-rails`.
                - Depends on: T-0150.
                - Exit: `evidence-rails.md` owns Python evidence rail choices.
                - Proof required: source review and Markdown validation.
            - [ ] T-0180 [QUEUED] Create `testing/specialized-rails.md`
                - Work: create `specialized-rails.md` for async, socket, filesystem, process, parser, and benchmark-specialized proof rails.
                - Target: `docs/stacks/python/testing/specialized-rails.md`.
                - Reference material: `SPEC.testing-proof-rails.md#specialized-rails`.
                - Depends on: T-0150.
                - Exit: `specialized-rails.md` owns Python specialized proof rail choices.
                - Proof required: source review and Markdown validation.

Milestone:
- ID: M-0030
- Outcome: Python platform and package graph truth exists without leaking package catalogs into core coding doctrine.
- Completion rule: all child tasks are `[COMPLETE]`, and required handoffs are closed or routed away.
- Progress basis: complete child tasks over all tasks in child phases.
- Progress: [░░░░░░░░░░░░░░░░░░░░] 0%
- Phases:
    - P-0050: Platform folder promotion.
        - Scope: `docs/stacks/python/.planning/SPEC.platform-package-graph.md`, `docs/stacks/python/platform/`.
        - Depends on: M-0020.
        - Completion rule: all child tasks are `[COMPLETE]`.
        - Progress basis: complete child tasks over this phase's tasks.
        - Progress: [░░░░░░░░░░░░░░░░░░░░] 0%
        - Tasks:
            - [ ] T-0190 [QUEUED] Produce the platform package-graph spec sheet
                - Work: run ten bounded sub-agent research lanes for package graph truth, tool graph truth, build metadata, dependency groups, package adoption gates, and concept-owner routing; fold research, critique, dry-run content, and candidate wording into one focused spec sheet.
                - Target: `docs/stacks/python/.planning/SPEC.platform-package-graph.md`.
                - Depends on: M-0020.
                - Exit: the spec sheet defines the handoff for the platform chooser and build/package page.
                - Proof required: source review and Markdown validation.
                - Handoff: phase owner-page tasks use the spec sheet as reference material.
            - [ ] T-0200 [QUEUED] Create `platform/README.md`
                - Work: create the Python platform chooser for package graph, tool graph, build truth, and cross-stack routing.
                - Target: `docs/stacks/python/platform/README.md`.
                - Reference material: `SPEC.platform-package-graph.md#platform-chooser`.
                - Depends on: T-0190.
                - Exit: `platform/README.md` routes platform questions without duplicating concept-page policy.
                - Proof required: source review and Markdown validation.
            - [ ] T-0210 [QUEUED] Create `platform/build-and-packages.md`
                - Work: create `build-and-packages.md` for `pyproject.toml`, dependency groups, tool graph, package adoption gates, and package-to-concept policy owners.
                - Target: `docs/stacks/python/platform/build-and-packages.md`.
                - Reference material: `SPEC.platform-package-graph.md#build-and-packages`.
                - Depends on: T-0200.
                - Exit: `build-and-packages.md` owns package graph and build truth, while core concept pages own coding decisions.
                - Proof required: source review and Markdown validation.
    - P-0060: Planning route closure.
        - Scope: `docs/stacks/python/README.md`, `docs/stacks/python/AGENTS.md`, `docs/stacks/python/.planning/ARCHITECTURE.md`, `docs/stacks/python/.planning/SPEC.planning-route-closure.md`.
        - Depends on: P-0050.
        - Completion rule: all child tasks are `[COMPLETE]`.
        - Progress basis: complete child tasks over this phase's tasks.
        - Progress: [░░░░░░░░░░░░░░░░░░░░] 0%
        - Tasks:
            - [ ] T-0220 [QUEUED] Produce the planning route closure spec sheet
                - Work: run ten bounded sub-agent research lanes for README chooser closure, overlay cleanup, planning architecture retirement, spec-sheet promotion, stale report boundaries, and residual planned-route gaps; fold research, critique, dry-run content, and candidate wording into one focused spec sheet.
                - Target: `docs/stacks/python/.planning/SPEC.planning-route-closure.md`.
                - Depends on: P-0050.
                - Exit: the spec sheet defines the handoff for reconciliation and planning-only detail retirement.
                - Proof required: source review and Markdown validation.
                - Handoff: phase closure tasks use the spec sheet as reference material.
            - [ ] T-0230 [QUEUED] Reconcile `README.md`, `AGENTS.md`, and `.planning/ARCHITECTURE.md`
                - Work: reconcile `README.md`, `AGENTS.md`, and `.planning/ARCHITECTURE.md` after core, testing, and platform pages exist.
                - Target: `docs/stacks/python/README.md`, `docs/stacks/python/AGENTS.md`, `docs/stacks/python/.planning/ARCHITECTURE.md`.
                - Reference material: `SPEC.planning-route-closure.md#reconciliation`.
                - Depends on: T-0220.
                - Exit: chooser, overlay, and planning architecture agree with promoted pages.
                - Proof required: source review and Markdown validation.
            - [ ] T-0240 [QUEUED] Retire planning-only detail
                - Work: retire planning-only detail that has been promoted to active owners.
                - Target: `docs/stacks/python/.planning/`.
                - Reference material: `SPEC.planning-route-closure.md#retirement`.
                - Depends on: T-0230.
                - Exit: no promoted planning-only detail remains active as durable coding law.
                - Proof required: source review and Markdown validation.

## [5]-[VALIDATION]

- [ ] Current position matches the first active task.
- [ ] Progress counts derive from completed task rows in this file.
- [ ] M-0010 contains only core Python coding doctrine.
- [ ] M-0020 owns testing and proof rails.
- [ ] M-0030 owns platform graph truth and planning closure.
- [ ] Every phase from P-0020 onward starts with a research/spec task.
- [ ] Every page-creation or closure task after a phase research/spec task names that spec sheet as reference material.
- [ ] Planned files match the planned view in `.planning/ARCHITECTURE.md`.
- [ ] `pep-standards.md` remains planned until the file exists and the README chooser reaches it.
- [ ] This roadmap may name planned files, but it does not carry the doctrine that belongs in those files.
