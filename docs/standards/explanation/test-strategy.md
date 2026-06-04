---
description: Standard for test strategy documentation
---

# Test strategy standards

Test strategy documents define the test portfolio, gate placement, ownership,
evidence expectations, and flaky-test policy for a maintained scope. They are
testing-risk policy documents, not contributor workflow guides, proof catalogs,
tool references, or implementation histories.

## Use when

Use a test strategy when a scope needs to state:

- which test levels exist and what risks they cover;
- where gates run and which changes require them;
- who owns failures, noisy tests, and quarantine decisions;
- which evidence proves behavior by change type;
- how cost, speed, fidelity, and reliability trade off.

Do not use it to tell contributors every command to run, catalog framework APIs,
or record milestone sequence.

## External basis

Use external testing guidance for strategy semantics:

- Test size describes resource and isolation boundary.
- Test scope describes the behavior or system surface being verified.
- Portfolio guidance prefers many small deterministic tests, enough integration
  tests for seams, and a small high-fidelity set for critical journeys.
- Gate-placement guidance puts fast deterministic checks closest to presubmit
  and moves expensive or less deterministic checks later.
- Flaky-test guidance requires identification, measurement, mitigation,
  owner-backed quarantine, and repair before trust erodes.

Repository truth owns actual gate names, commands, runners, status checks,
artifacts, and owner roles.

## Placement

- Shared strategy: `docs/test-strategy.md`.
- Test corpus strategy: `docs/testing-strategy.md` or a maintained test docs
  hub.
- Owner-local strategy: `{owner}/TEST_STRATEGY.md` when policy applies only
  inside one owner boundary.

Keep one strategy per scope. Link lower-level owner strategies instead of
copying their gate maps into a shared document.

## Required structure

```markdown
# <Scope> test strategy

## Scope
## Strategy principles
## Test levels
## Gate mapping
## Required proof by change type
## Ownership
## Flaky-test policy
## Metrics and evidence
## Review trigger
## Related
```

`Related` is optional when adjacent docs are already linked by the owner README.

## Strategy principles

- Prefer the smallest test that proves behavior with acceptable fidelity.
- Separate size from scope.
- Keep the portfolio pyramid-shaped unless the scope documents why another
  distribution is more reliable and cheaper to maintain.
- Replace duplicated end-to-end coverage with integration tests when the smaller
  gate catches the same failure.
- Reserve end-to-end gates for critical journeys and cross-boundary behavior.
- Treat coverage percentages as signals, not proof of correctness.
- Add nonfunctional levels only when the scope owns that risk.

## Test levels

Define only levels that the scope runs or reviews. For each level, state:

- purpose and risk covered;
- size boundary, such as process, machine, network, data store, external
  service, host runtime, or production-like system;
- scope boundary, such as function, module, component seam, workflow, system, or
  user journey;
- owner role responsible for maintenance and failure triage;
- expected runtime cost and resource class;
- isolation and data policy;
- failure artifacts needed for diagnosis;
- when the level must run.

Do not use runner directories, filenames, or framework names as the taxonomy
unless they also describe risk and isolation boundary.

## Gate mapping

Gate maps connect strategy to automation without becoming a tool manual. For
each gate, state:

- trigger, such as presubmit, post-submit, nightly, release, manual approval, or
  incident follow-up;
- selection rule, such as changed-path impact, dependency impact, owner tag,
  release target, risk label, or full-suite cadence;
- blocking behavior;
- timeout, retry, shard, or resource policy when it affects signal quality;
- required artifact or status-check location;
- escalation owner when the gate fails or becomes noisy.

Fast deterministic gates should block early. Slow or less hermetic gates may
run later, but the strategy must state what risk remains before those gates pass.

## Required proof by change type

Map change families to the smallest sufficient proof surface:

- behavior or algorithm changes and their narrow test level;
- integration or contract changes and their seam-level gate;
- user-journey, deployment, or host-runtime changes and their high-fidelity
  gate;
- nonfunctional risk changes and the specialized gate that owns the risk;
- documentation-only or configuration-only changes and their review or generated
  evidence;
- conditions that escalate from scoped gates to broader gates.

The strategy names which proof is required; [proof.md](../proof.md) decides
whether the evidence is strong enough.

## Ownership and flakiness

Every test level, gate, and quarantine path needs an accountable owner. Define
who maintains tests, diagnoses cross-owner failures, approves quarantine or
deletion, pays fixture and environment cost, and updates ownership when
boundaries move.

Define flakiness as a test that can pass and fail against the same relevant code
and environment state. State detection signal, severity, rerun policy,
quarantine criteria, owner, maximum duration, remaining signal, re-enable
criteria, and deletion criteria when a flaky test duplicates stronger coverage.
Quarantine is not a fix.

## Metrics and review trigger

Use metrics to improve signal, not replace judgment. Useful metrics include
pass, fail, flake, retry, quarantine, and re-enable rates; gate duration; queue
time; failure localization quality; critical journey coverage; defect escape
evidence; and behavior-level coverage where it matches risk.

Refresh the strategy when gates, test size or scope, ownership, quarantine
policy, architecture, runtime, deployment, flake rates, gate duration, release
escapes, or external testing guidance changes.

## Boundaries

- Contributing docs tell contributors what to run in a workflow.
- Proof standards define evidence strength and freshness.
- Test strategy defines gate taxonomy, ownership, escalation, and flake policy.
- Reference docs catalog test tools and framework APIs.
- Runbooks handle operational recovery.
- Roadmaps handle phase exit and delivery sequencing.

## Review checklist

- [ ] Scope and owner boundaries are clear.
- [ ] Test levels map to maintained gates or review gates.
- [ ] Gate triggers, selection rules, blocking behavior, and artifacts are
      stated.
- [ ] Change types map to the smallest sufficient proof surface.
- [ ] Large and cross-owner tests have diagnosis owners.
- [ ] Flaky-test handling includes detection, quarantine, owner, duration,
      remaining signal, and re-enable criteria.
- [ ] Metrics measure signal quality, cost, and escaped risk.
- [ ] Boundaries with contributing, proof, reference, runbook, and roadmap docs
      are explicit.
