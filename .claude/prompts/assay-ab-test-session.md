# [PROMPT] Tool — A/B Parity, Concurrency Stress, and Test Suite

Use this for the next `<target-tool>` session. The job is to prove `<target-tool>` is at least as capable as `<predecessor-tool>` on every overlapping verb, prove resilience under heavy concurrent use, then build the dense algorithmic test suite. Work in this order: A/B parity, stress, non-parity exercise, adversarial hardening, tests.

## [1] WHERE / WHAT / STATE

- **New tool:** `<target-tool>` — the agent-first polyglot quality keychain. Claims `<capability-arms>`, root verbs `<root-verbs>`, and optional automation arms.
- **Non-parity capabilities:** polyglot `api`; dual-modality `code search`; `code query`; `code rewrite`; run history and `delta`; toolchain-doctor; automation watch/schedule; transparent remote execution.
- **A/B target:** `<predecessor-tool>` — predecessor or reference surface. Overlapping verbs: `<overlapping-verbs>`.
- **Test scaffold:** `<test-scaffold>` exists; the A/B matrix, wire laws, automation, stress, and adversarial specs are the missing suite.
- **Host boundary:** `<host-boundary>` remains the host-gated lane for live host behavior.

## [2] DOCTRINE

- **A/B is output-level and adversarial.** For every overlapping verb, run both tools against the same isolated tree and diff exact content: fields, shapes, exit codes, statuses, truncation, lock behavior, ordering, artifact pointers, and diagnostics.
- **No regression.** A capped list with `truncated=true` and an artifact is acceptable; a capped list with neither is a regression. Integrate only focused wins from the predecessor surface.
- **Agent ergonomics are internal behavior.** Commands self-route, self-describe, and self-correct; failures carry actionable diagnostics; hardening lands inside the command, not as new flags or ceremony.
- **Concurrency resilience is a primary deliverable.** Lease contention, dotnet artifacts, scratch space, mutation locks, rewrite locks, package staging, and read-only fans must behave cleanly under dozens to hundreds of simultaneous invocations.
- **Tests are dense, algorithmic, and law-driven.** Use parametrization, generators, and catalog/member sweeps over flat file spam. Keep the file set minimal and split only by real concern.
- **Live where reachable.** Use real tool invocations and real trees. Mock only unreachable host or remote boundaries, and record the honest ceiling.

## [3] SURFACE MAP

Map the current `<target-tool>` capability surface against `<predecessor-tool>` and produce:

1. The overlapping-verb matrix.
2. The non-parity capability list.
3. The existing assay test scaffold and missing law families.
4. The host-gated cases that cannot run headlessly.

## [4] PHASE A — A/B PARITY

Run a workflow that assigns each verb-scenario cell to an isolated comparison. Capture raw outputs, canonical field maps, exit-code/status correspondence, truncation behavior, lock behavior, ordering, old-tool wins worth integrating, new-tool bugs, missing diagnostics, and a better/worse verdict per dimension.

Do not move to stress/tests until every overlapping cell is proven non-regressive or the regression has been fixed.

## [5] PHASE B — CONCURRENCY STRESS

Build a stress harness that launches many concurrent invocations across lease-bearing and build-bearing paths: `<build-command>`, mutation runs, rewrite apply, package stage, and read-only fans. Assert no deadlock, nested run loop, corrupted lock, artifact collision, or opaque failure. Compare the same storm against `<predecessor-tool>` and record the resilience delta.

## [6] PHASE C — NON-PARITY EXERCISE

Exercise every capability without a `<predecessor-tool>` analog: polyglot API indexing/querying, code search/query/rewrite, run-history delta, retention pruning, automation triggers, schedule recovery, resource governance, and remote execution loopback. Each result should expose domain-appropriate structured evidence.

## [7] PHASE D — ADVERSARIAL HARDENING

Feed malformed, hostile, and boundary inputs through every command: bad patterns, missing paths, permission failures, corrupt locks, corrupt history, malformed SSH URIs, oversized output, Unicode/encoding edges, empty trees, concurrent steal races, and raising thunks. Confirm every reachable failure degrades to typed structured output instead of an uncaught traceback or silent wrong answer.

## [8] PHASE E — TEST SUITE

Build the minimal dense suite, split only by concern:

- Wire laws for `Envelope`, `Report`, `Detail`, `RailStatus`, one-envelope output, exit-code/status mapping, terse wire shape, and closed variant spaces.
- A/B parity laws for every overlapping verb, driven from the comparison fixture.
- Automation laws for triggers, missed-fire recovery, jitter, resource governance, and sequence halting.
- Concurrency laws for lease steal, fast-fail busy behavior, artifact isolation, and no nested run loop.
- Adversarial laws for degenerate inputs and typed degradation.

Refine `conftest.py` as shared laws emerge; shrink duplicate fixtures instead of spreading helper files.

## [9] ERGONOMICS HARDENING

As A/B and adversarial findings land, harden command behavior internally: primary-field positional acceptance, better ambiguity resolution, richer misses, faster primitives, domain-appropriate `Detail`, and no truncation without an artifact pointer.

## [10] START HERE

1. Build the surface map.
2. Run A/B parity and integrate focused old-tool wins.
3. Run concurrency stress on both tools.
4. Exercise non-parity capabilities and adversarial cases.
5. Build the dense suite and harden command ergonomics.
