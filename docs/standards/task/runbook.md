---
description: Standard for operational runbooks
---

# Runbook standards

Runbooks are symptom-to-fix operational procedures. They help a responder move
from an observable alert, failure, or user-impact report to triage, mitigation,
rollback, escalation, verification, and evidence capture under pressure.

## Use when

Use a runbook when:

- the starting point is an observable operational symptom;
- a responder needs safe triage and mitigation;
- rollback, abort, or escalation criteria matter;
- response evidence must be captured.

Do not use a runbook for normal tasks, architecture background, incident-command
policy, postmortem templates, or broad reference catalogs.

## External basis

Use SRE and mature incident-response practice for clear roles, early impact
assessment, read-only triage before mutation, practiced response, rollback
readiness, evidence capture, and continuous refinement.

## Placement

- Shared operational runbook: `docs/runbooks/<symptom-or-system>.md`.
- Owner-local runbook: `{owner}/runbooks/<symptom-or-system>.md` or the nearest
  maintained operations corpus for that owner.
- Service-local emergency procedure: beside the service only when local
  credentials, dashboards, deployment tools, or ownership make a shared page
  less discoverable during response.

Use one canonical runbook for one operational trigger. Link related runbooks
instead of copying the same triage or mitigation path.

## Required structure

```markdown
# Recover from <observable symptom>

Owner: <owner role or team>
Last reviewed: YYYY-MM-DD
Review trigger: <alert, dependency, topology, tool, or owner change>

## Trigger
## Impact
## Safety and prerequisites
## Triage
## Mitigation
## Rollback or abort
## Escalation
## Verification
## Evidence to capture
## Follow-up cleanup
## Related
```

`Follow-up cleanup` keeps the system safe after recovery. It is not a postmortem
template. `Related` may be omitted when no adjacent maintained page adds value.

## Scope rules

- Start from an observable trigger: alert name, failed check, user-impact
  report, queue depth, latency budget breach, data integrity symptom, deployment
  failure, or security signal.
- Name affected surface and user or business impact before procedure.
- Keep the runbook focused on recovery or safe escalation.
- Include only prerequisites needed during response: access, permissions,
  break-glass path, dashboards, diagnostic tools, known-good backups, or safe
  execution context.
- Link background, architecture, API catalogs, contact directories, severity
  models, communications policy, and postmortem templates.

## Triage rules

- Put read-only observation before state-changing actions.
- Order checks by decision value: confirm trigger, estimate impact, isolate the
  failing component, identify recent changes, and choose mitigation, rollback,
  or escalation.
- Give exact commands, dashboards, queries, or UI paths when stable enough to
  execute under pressure.
- State expected signal after each material check.
- State branch conditions directly: `If <signal>, do <next action>`.
- Preserve evidence before actions that can destroy logs, counters, traces,
  screenshots, or volatile state.

## Mitigation, rollback, and abort

- Prefer reversible, bounded, low-blast-radius actions before broad changes.
- State who may perform risky actions when permission matters.
- Pair each state-changing action with expected result and verification.
- Mark automation as safe only when idempotent or guarded by explicit
  preconditions.
- Include fallback or escalation when primary tooling may be unavailable.
- Define rollback or abort criteria before actions that can worsen impact, hide
  evidence, increase load, change data, or remove capacity.
- If rollback is impossible, say so and require escalation before irreversible
  action.
- Stop when verification signals contradict the assumed cause; return to triage
  or escalate with captured evidence.

## Escalation

Escalation criteria must be observable: impact threshold, time threshold,
unclear ownership, failed rollback, missing access, suspected security issue,
data loss risk, customer-facing outage, or unavailable primary tooling.

Name the role or owning team, not an individual, unless the operational corpus
requires a specific duty contact. Include trigger, impact, hypothesis, actions
taken, evidence captured, and next unsafe or blocked step.

## Verification and evidence

- Verification must prove recovery or containment, not merely command
  completion.
- Include customer-visible, system-visible, and operator-visible signals when
  all three matter.
- Capture alert IDs, timeline anchors, dashboards, commands, logs, traces,
  deploy or change IDs, ticket or incident links, screenshots, and known gaps.
- State where evidence is stored when the corpus has a maintained incident
  record, ticket, object store, or audit path.
- Mark unavailable proof honestly.

## Maintenance

Review a runbook when trigger, service owner, dashboard, alert rule, command,
dependency, topology, rollback path, escalation path, or automation changes.
Update it from real incidents when responders had to invent a check, skip a
stale step, ask for hidden context, or perform an undocumented mitigation.
Delete or merge runbooks whose triggers no longer exist.

## Boundaries

- How-to guides cover normal work for competent readers.
- Runbooks cover operational symptoms, recovery, containment, rollback,
  escalation, and response evidence.
- Incident process documentation owns severity models, command roles,
  stakeholder communications, postmortems, and governance.
- Test strategy owns gate policy and flaky-test handling.
- Support matrix owns support status, limitations, deprecation, and
  end-of-support facts.
- Architecture and reference docs own background, topology, and lookup catalogs.

## Review checklist

- [ ] Title and trigger start from an observable symptom.
- [ ] Owner, `Last reviewed`, and `Review trigger` are present.
- [ ] Impact and affected surface are stated before action.
- [ ] Safety prerequisites are response-critical only.
- [ ] Triage is separated from mitigation and starts with read-only observation.
- [ ] State-changing steps include expected results and recovery verification.
- [ ] Risky actions have rollback or abort criteria.
- [ ] Escalation criteria and owner role are observable and actionable.
- [ ] Evidence capture is explicit and tied to a storage location or handoff.
- [ ] Incident governance, postmortem workflow, normal how-to content, and
      lookup catalogs are linked instead of embedded.
