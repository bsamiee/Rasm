# [PYTHON_STACK_ROADMAP]

[ACTIVE] Package build for `docs/stacks/python`: seed the Python stack decision atlas and promote planned concept pages as they become durable owners. Current position: T-0040 is active in P-0020 under M-0010. Proof route: documentation review plus Markdown validation. Live planning route: Markdown-controlled.

## [1]-[CURRENT_POSITION]

State: [ACTIVE]
Sequence type: Package build
Current focus: T-0040 in P-0020 under M-0010.
Progress basis: 3/10 tasks; counted only when task `Status` is `[COMPLETE]` and proof agrees.
Progress: [██████░░░░░░░░░░░░░░] 30%
Proof route: documentation review plus Markdown validation.
Live planning route: Markdown-controlled.

## [2]-[STRUCTURAL_COMPONENTS]

Roadmap IDs are scope-local. Milestones use `M-0010`, phases use `P-0010`, and tasks use `T-0010`; only task rows carry lifecycle status.

Task completion requires the target file to exist, the README chooser to route to it when current, planned architecture to match, and proof to agree.

## [3]-[BOUNDARIES]

This roadmap owns the Python stack documentation buildout sequence only. Current language decisions live in [language](../language.md), stack orientation lives in [README](../README.md), local agent behavior lives in [AGENTS](../AGENTS.md), and planned topology lives in [architecture](ARCHITECTURE.md).

Do not create external-library-specific pages. Planned pages are promoted by domain, category, or concept; avoid operational buckets copied from skills.

## [4]-[ACTIVE_WORK]

Milestone:
- ID: M-0010
- Outcome: Python stack decision atlas exists with durable language guidance, stack topology, and planned concept owners.
- Completion rule: all child tasks are `[COMPLETE]`, and README plus planned architecture agree with promoted pages.
- Progress basis: complete child tasks over all tasks in child phases.
- Progress: [██████░░░░░░░░░░░░░░] 30%
- Phases:
    - P-0010: Seed the stack folder.
        - Scope: `docs/stacks/python/`
        - Completion rule: all child tasks are `[COMPLETE]`.
        - Progress basis: complete child tasks over this phase's tasks.
        - Progress: [████████████████████] 100%
        - Tasks:
            - [x] T-0010 [COMPLETE] Create the stack chooser and leaf overlay
                - Work: create `README.md` and `AGENTS.md` with stack-local route and rejection rules.
                - Target: `docs/stacks/python/README.md`, `docs/stacks/python/AGENTS.md`.
                - Exit: chooser and overlay exist with local deltas only.
                - Proof required: Markdown validation.
            - [x] T-0020 [COMPLETE] Create the language decision page
                - Work: write canonical Python `>=3.15` language guidance by concern rather than version.
                - Target: `docs/stacks/python/language.md`.
                - Exit: page contains active surface, decision chooser, replacements, and rejections.
                - Proof required: Markdown validation and source review.
            - [x] T-0030 [COMPLETE] Create planning topology
                - Work: create this roadmap and the planning architecture.
                - Target: `docs/stacks/python/.planning/ROADMAP.md`, `docs/stacks/python/.planning/ARCHITECTURE.md`.
                - Exit: planning files use roadmap and planning-architecture structures.
                - Proof required: Markdown validation.
    - P-0020: Promote concept decision pages.
        - Scope: `docs/stacks/python/**`
        - Depends on: P-0010.
        - Completion rule: all child tasks are `[COMPLETE]`.
        - Progress basis: complete child tasks over this phase's tasks.
        - Progress: [░░░░░░░░░░░░░░░░░░░░] 0%
        - Tasks:
            - [ ] T-0040 [ACTIVE] Promote domain shape guidance
                - Work: create the concept page for dense values, classes, variants, protocols, DTO boundaries, admission, and shape collapse.
                - Target: `docs/stacks/python/domain-shapes.md`.
                - Exit: page owns shape decisions that prevent primitive obsession, dict bags, dataclass spam, optional payload bags, and wrapper-only classes.
                - Proof required: source review and Markdown validation.
            - [ ] T-0050 [QUEUED] Promote surface and dispatch guidance
                - Work: create the concept page for public APIs, descriptors, decorators, protocols, algebraic dispatch, pattern routing, and polymorphic collapse.
                - Target: `docs/stacks/python/surfaces-and-dispatch.md`.
                - Exit: page owns advanced surface decisions without helper-function or switch-table topology.
                - Proof required: source review and Markdown validation.
            - [ ] T-0060 [QUEUED] Promote result and effect guidance
                - Work: create the concept page for rails, effects, typed failures, resources, retries, recovery, and boundary collapse.
                - Target: `docs/stacks/python/rails-and-effects.md`.
                - Exit: page owns result and effect decisions without package-shaped topology.
                - Proof required: source review and Markdown validation.
            - [ ] T-0070 [QUEUED] Promote boundary and runtime guidance
                - Work: create concept pages for codecs, validation, parsing, transport boundaries, runtime concurrency, interpreters, instrumentation, and resource ownership.
                - Target: `docs/stacks/python/boundaries-and-codecs.md`, `docs/stacks/python/runtime-and-concurrency.md`.
                - Exit: pages own boundary and runtime decisions without operational storage or observability buckets.
                - Proof required: source review and Markdown validation.
            - [ ] T-0080 [QUEUED] Promote algorithm, platform, and proof guidance
                - Work: create concept pages for algorithms/data flow, package graph/platform ownership, and proof rails.
                - Target: `docs/stacks/python/algorithms-and-data.md`, `docs/stacks/python/platform/build-and-packages.md`, `docs/stacks/python/testing/README.md`.
                - Exit: pages own package-backed capability, tool graph facts, and proof decisions with no command catalogs.
                - Proof required: source review and Markdown validation.
    - P-0030: Retire planning state.
        - Scope: `docs/stacks/python/.planning/`
        - Depends on: P-0020.
        - Completion rule: all child tasks are `[COMPLETE]`.
        - Progress basis: complete child tasks over this phase's tasks.
        - Progress: [░░░░░░░░░░░░░░░░░░░░] 0%
        - Tasks:
            - [ ] T-0090 [QUEUED] Update chooser and planned architecture after promotion
                - Work: replace future chooser rows with links and close planned-view records.
                - Target: `docs/stacks/python/README.md`, `docs/stacks/python/.planning/ARCHITECTURE.md`.
                - Exit: README links all promoted concept pages, and planning architecture has no stale planned records.
                - Proof required: link and Markdown validation.
            - [ ] T-0100 [QUEUED] Archive or delete completed planning material
                - Work: remove planning files when they no longer change agent action, or mark terminal state if they still preserve buildout context.
                - Target: `docs/stacks/python/.planning/`.
                - Exit: no completed planning state remains as active instruction or current docs truth.
                - Proof required: source review.

## [5]-[VALIDATION]

- [ ] Current position matches the first active task.
- [ ] Task progress derives only from `[COMPLETE]` task rows.
- [ ] Every active task has `Work`, `Target`, `Exit`, and `Proof required`.
- [ ] No milestone or phase status field is present.
- [ ] No task creates an external-library-specific page or folder.
