# [CONTRIBUTING_STANDARDS]

A contributing guide tells a prospective contributor which contribution paths the project accepts, how normal changes move from intent to review, what confirmation proves the work, and how pull requests converge. It is a contributor workflow document, not onboarding, architecture, gate taxonomy, roadmap, incident response, repository policy, support policy, or vulnerability-disclosure policy. Keep adjacent concerns in their routes and link them only where the contributor must act on the boundary.

## [01]-[USE_WHEN]

Use a contributing guide when readers need accepted contribution paths, normal-change workflow, contributor-facing quality-gate confirmation, pull-request confirmation, review-collaboration rules, or security-report routing.

Route away adjacent concerns before authoring the workflow:

[WORKFLOW_ROUTES]:
- readiness, first safe tasks, and ramp sequence go to onboarding.
- normal procedures go to how-to guides.
- implementation sequence, active milestone bodies, terminal work, and task exit confirmation go to roadmaps.
- operational symptoms and recovery go to runbooks.
- gate taxonomy, flake policy, and portfolio confirmation go to test strategy.
- supported versions, platforms, and compatibility windows go to support matrices.

[POLICY_ROUTES]:
- disclosure timelines, support windows, advisory flow, and bounty promises go to the maintained security-policy route.
- branch protection, merge permissions, repository governance, CODEOWNERS policy, and response-time promises go to maintained repository-policy sources.

[AUTHORING_CONTRACT]:
- Agent use: prove the project accepts the contribution path, name the first artifact and contributor-facing gates, and route repository policy, security, support, readiness, and roadmap task state away.
- Required produced structure: `Scope`, `Ways to contribute`, `Setup workflow`, `Quality gates`, `Pull requests`, `Review`, `Security reports`, `Boundaries`, and `Result check`.
- Section cardinality: one accepted-path selector; one setup workflow; one quality-gate mapping only when maintained gates exist; conditional `Before you start`, `Getting help`, `Documentation changes`, and `Maintenance` sections only when their trigger changes contributor action.
- Adjacent checks: check onboarding, how-to, runbook, test strategy, support matrix, confirmation, README, API, code documentation, roadmap, architecture, and maintained security or repository-policy sources only when the contributor workflow or confirmation changes.
- Maintenance triggers: update the guide when accepted paths, host discovery, templates, branch protection, required checks, command gates, sign-off, security route, review policy, help route, or documentation-travel rule changes.

Opening order is fixed for task standards: route and use contract first, produced structure second, cardinality third, then baselines, examples, and local patterns. Do not let contribution-path examples define section order implicitly.

## [02]-[SOURCE_PRECEDENCE]

Anchor contribution obligations to project truth first. A guide may mention a host surface, commit format, sign-off, code of conduct, license, contribution location, review route, or security route only when repository configuration, host settings, workflow files, templates, CODEOWNERS, or maintained policy proves the project uses it.

Use this source order for contributor-facing claims:
1. Repository truth: host settings, branch-protection rules, required checks, workflow files, commit linting, sign-off checks, pull-request templates, issue templates, CODEOWNERS, and maintained policy documents.
2. Local runnable confirmation: the exact setup, quality, generation, docs, or review command run during maintenance.
3. Maintained route documents: README, onboarding, how-to, test strategy, support matrix, security policy, repository policy, API, reference, code documentation, architecture, and roadmap.
4. Maintained host or convention source used only for placement, syntax, or fallback behavior.

Do not import a `feat:` or `fix:` prefix rule, `Signed-off-by` line, contribution location, branch-protection claim, required-check claim, response-time promise, or security-reporting path from external examples alone. If host or policy confirmation is unavailable during authoring, state the missing configuration and omit the obligation from the contributor workflow.

## [03]-[REQUIRED_STRUCTURE]

The published guide uses a required base plus conditional additions. Each `##` heading is a standalone retrieval unit; renumber headings in document order after inserting or omitting conditional sections.

```markdown template
# [CONTRIBUTING]

<Lead: accepted contribution surface and the highest-risk contribution boundary.>

## [1]-[SCOPE]

## [2]-[WAYS_CONTRIBUTE]

## [3]-[SETUP_WORKFLOW]

## [4]-[QUALITY_GATES]

## [5]-[PULL_REQUESTS]

## [6]-[REVIEW]

## [7]-[SECURITY_REPORTS]

## [8]-[BOUNDARIES]

## [N]-[BEFORE_YOU_START]

## [N]-[GETTING_HELP]

## [N]-[DOCUMENTATION_CHANGES]

## [N]-[MAINTENANCE]
```

Place conditional sections where the contributor needs them: `Before you start` before setup, `Getting help` after quality gates, `Documentation changes` before PR confirmation, and `Maintenance` before or after `Boundaries` based on whether it changes contribution workflow or only stewardship.

[SECTION_CARDINALITY]:
- `Lead`, `Scope`, `Ways to contribute`, `Setup workflow`, `Quality gates`, `Pull requests`, `Review`, `Security reports`, `Boundaries`, and `Result check`: required, single.
- `Before you start`: conditional; include only for conduct, license, sign-off, discussion, account, permission, or agreement prerequisites.
- `Getting help`: conditional; include immediately after `Quality gates` only when setup, permission, route, or gate blockers interrupt a reviewable path.
- `Documentation changes`: conditional; include only when documentation must travel with changed truth.
- `Maintenance`: conditional in produced guides; include only when the guide itself is an actively maintained contributor contract.

Required sections carry one purpose each. `Scope` states accepted surfaces and route-away topics; `Ways to contribute` selects the first artifact; `Setup workflow` reaches a first-gate-passing tree and states enforced workflow facts; `Quality gates` names contributor-facing results; `Pull requests` records review confirmation; `Review` makes threads converge; `Security reports` routes private vulnerability detail; `Boundaries` names adjacent routes; `Result check` verifies the published guide. Omit placeholders, template instructions, maintenance route notes, and speculative routes.

## [04]-[SCOPE]

`Scope` states what the project accepts before it teaches any workflow. It carries the accepted contribution surface, the public or private entry route for each surface, topics routed away, and the highest-risk contribution boundary, such as security-sensitive, breaking, or cross-scope work that requires direction first.

Do not publish broad encouragement without a reviewable path. A contribution guide that invites work the project cannot review creates contributor waste and route ambiguity. When no maintained route exists for an excluded topic, route the contributor to source-routing consultation instead of inventing policy in `CONTRIBUTING`.

## [05]-[WAYS_CONTRIBUTE]

`Ways to contribute` is a selector for the first reviewable artifact. Each accepted path names the contributor intent, entry artifact, gated prerequisite, and scope bound, so a contributor can choose the first action without reading the whole guide.

Use a table only when the project accepts two or more paths that readers compare. Keep the selector narrow:

```markdown template
| [INDEX] | [PATH]   | [INTENT]   | [ENTRY_ARTIFACT] | [PREREQUISITE] | [SCOPE_BOUND]   |
| :-----: | :------- | :--------- | :--------------- | :------------- | :-------------- |
|   [1]   | <path-a> | <intent-a> | <artifact-a>     | <gate-a>       | <scope-bound-a> |
|   [2]   | <path-b> | <intent-b> | <artifact-b>     | <gate-b>       | <scope-bound-b> |
```

Use a definition block for one accepted path:

```markdown template
Path: <path name>
Intent: <contributor intent>
Entry artifact: <entry artifact>
Prerequisite: <gate or `—`>
Scope bound: <accepted-scope bound>
```

Required path fields are `Intent`, `Entry artifact`, and `Scope bound`; `Prerequisite` is required when an action is gated and marked `—` only when no prerequisite exists. `Non-code` is a path only when maintained routes can review the work; it may include reproduction cases, design feedback, accessibility fixes, translations, reviews, examples, and release notes when the project maintains those routes.

Route broad, expensive, compatibility-breaking, security-sensitive, or cross-scope work to a source-routing check before the contributor spends implementation effort. Use prose for a single route-away condition. Use a decision table only when two or more independent conditions jointly choose the entry artifact. Use a route-selection flowchart only when normal contribution, security-sensitive work, and consultation-before-implementation all exist and the branch-and-rejoin path would be hard to follow as prose.

Show a near-miss only when authors tend to publish vague path bounds:

[CONTRIBUTOR_PATH]:
- Accepted intent: land fix
- Accepted prerequisite: local gate
- Accepted scope bound: self-contained; no breaking API or cross-scope migration
- Rejected intent: quick fixes
- Rejected prerequisite: unspecified
- Rejected scope bound: broad and unbounded
- Reason: the accepted fields constrain the contributor path; the rejected fields leave the path broad and unbounded.

Use the same compact contrast labels in every contributing example: `Accepted`, `Rejected`, `Near miss`, and `Reason`. Do not substitute `Good`, `Bad`, or project-local taste labels; the contrast record exists to show the action difference.

## [06]-[BEFORE_YOU_START]

Include `Before you start` only when a contributor must accept or complete a prerequisite before opening a contribution path. Carry the route and acceptance action, not the policy body.

- Link the code of conduct and state that contribution implies acceptance, when the project enforces one.
- Name the contribution license or sign-off requirement, when one governs the change: inbound license, `Signed-off-by` line, or contributor license agreement route.
- State how to add a sign-off only when the project requires sign-off.
- Name required accounts, permissions, or contributor agreements only when they gate contribution.
- Link the discussion or source-routing contact channel for questions that are not yet a contribution.

Do not embed prerequisite knowledge, first-task guidance, or readiness here; route those to onboarding. Do not embed the full code-of-conduct text, license body, sign-off certification body, or contributor-license-agreement. Route each to its maintained material.

## [07]-[SETUP_WORKFLOW]

`Setup workflow` states the commands a contributor on the normal path runs to reach a working tree that passes the first gate. Link deeper onboarding, architecture, build, platform, or reference material instead of embedding it. A setup command that was not checked during guide maintenance stays provisional beside the command under [proof.md](../proof.md).

Use ordered steps for commands that must run in order. Use bullets or definition fields for enforced workflow facts that are unordered:
- how to find an issue to take or how to propose new work.
- whether to fork, branch from the default branch, or request write access.
- the branch, commit, changeset, or pull-request-title discipline the project enforces.
- any commit-message convention only when the project enforces it.
- how to keep one change to one concern so review stays tractable.
- when generated files, dependency-manifest updates, screenshots, artifacts, or docs must travel with the code.
- how to recover or ask for help when setup or a gate fails.

State each branch condition before its action: `If <signal>, do <action>`. Do not claim continuous integration, required status checks, branch protection, source-routing response time, commit-convention enforcement, sign-off enforcement, or automation behavior unless repository or host configuration proves it.

## [08]-[QUALITY_GATES]

`Quality gates` names the runnable command or maintained review gate that proves a contributor's change and the result to attach. Publish a gate mapping only when the repository proves concrete commands or maintained review gates; otherwise, delete the section's table or record and state the missing route.

Use a table when contributors must choose between two or more change families:

```markdown template
| [INDEX] | [CHANGE]   | [GATE]   | [REPORT]   | [GAP]   |
| :-----: | :--------- | :------- | :--------- | :------ |
|   [1]   | <change-a> | <gate-a> | <report-a> | <gap-a> |
|   [2]   | <change-b> | <gate-b> | <report-b> | <gap-b> |
```

Use a definition block for one gate:

```markdown template
Change: <change family>
Gate: <runnable command or maintained review gate>
Report: <status and result>
Gap: <gap route or `—`>
```

Required gate fields are `Change`, `Gate`, `Report`, and `Gap`. Point every gate at a current runnable command or maintained quality document, state honestly when a gate is human review rather than an executable command, and never assert a gate passed unless it ran in the change or a current status check proves it. Route gate taxonomy, gate routing, flake policy, escalation thresholds, and portfolio-level confirmation to [test-strategy.md](../explanation/test-strategy.md) or the maintained quality route.

Show a gap in the row or record where the contributor reports a result, not in a separate apology paragraph:

```markdown conceptual
| [INDEX] | [CHANGE]     | [GATE]                  | [REPORT]                       | [GAP]                            |
| :-----: | :----------- | :---------------------- | :----------------------------- | :------------------------------- |
|   [1]   | `<change-a>` | `<maintained-gate-a>`   | [PASS] `<reported-signal-a>`   | —                                |
|   [2]   | `<change-b>` | maintained review route | [SKIP] validator access needed | unrun: runtime route unavailable |
```

Delete the gate mapping when no maintained gate or review route exists. A polished list of unproved commands is filler, not contributor guidance.

## [09]-[GETTING_HELP]

Include `Getting help` only for contribution blockers that prevent a contributor from completing a reviewable path: setup command failure, missing permission, unclear route, inaccessible required gate, or a review question that needs maintained material direction. Link the maintained discussion or source-routing contact route only when the project publishes one. Do not use this section for user support, onboarding, repository-policy questions, incident response, roadmap task negotiation, or vulnerability reports; route those topics elsewhere.

## [10]-[DOCUMENTATION_CHANGES]

Include `Documentation changes` when contributions can alter documentation truth: code behavior, configuration, generated contracts, user-visible behavior, support status, gate policy, route practice, contribution workflow, operational procedure, or public entry route. Route a new or changed document through the standards index by topic, then through the matching document-type standard; this guide carries the trigger, not the adjacent document.

Use the shared adjacent-document relation record only when documentation must travel with the change:

```markdown template
Changed fact: <code behavior, configuration, generated contract, procedure, support fact, or workflow>
Consumed by: <API, code documentation, README, support matrix, test strategy, roadmap, architecture, how-to, runbook, tutorial, onboarding, contributing, or reference>
Use in this document: <contributor action: update existing doc, create routed doc, or state no maintained route exists>
Update when: <changed fact, generated artifact, procedure, support row, gate, route, or workflow changes>
Close when: <the consuming document updates, the guide states no maintained route exists, or the fact routes away>
Route-away: <adjacent document body that remains outside the contributing guide>
```

Use richer integration only when the adjacent document changes contributor action or review confirmation. API contract changes usually update API documentation and any migration how-to; public symbol intent changes route to code documentation; support status changes route to the support matrix and any README entry route; new contributor gates update both contributing and test strategy; onboarding updates only when a workflow change affects readiness or first safe tasks; roadmap updates only when task sequence or milestone confirmation changes contributor action. A link is enough when the adjacent document only provides background.

## [11]-[PULL_REQUESTS]

A pull request is accepted for review when its body carries the change summary, run results, gaps, scope, and required self-checks. Missing required results are a request-changes condition, not a validator nicety. Use the host template when one exists; otherwise use one body shape with checklist items embedded, not a separate checklist plus a duplicate template:

```markdown template
Summary: <one to three sentences naming the change and motivation>

Results:
    - <command, status check, or review gate>: <result>

Gap: <none, or unrun/failing gate with reason and resolution route>

Scope: <paths, areas, or routes touched; one concern only>

Observed result: <screenshots, recordings, generated artifacts, reproduction steps, governing issue, discussion, design, decision, or roadmap milestone; omit when absent>

Required self-check:
    - [ ] Run results are attached or the gap is stated.
    - [ ] Unrelated changes are excluded.
    - [ ] Enforced title, commit, or sign-off policy is satisfied, when repository policy requires it.
```

State an unrun gate as unrun rather than implying it passed. Update the pull-request body when run results, scope, or risk change during review. Add paths, routes, visual confirmation, runtime confirmation, generated artifacts, governing decisions, or roadmap milestones only when those facts route review.

## [12]-[REVIEW]

`Review` defines how contributors resolve review threads without widening the contribution. Apply these rules to every review:
- Keep technical discussion public unless it carries sensitive information.
- Answer each review comment with a change, a question, or a reasoned disagreement; silence is not a response.
- Keep unrelated changes out of the contribution.
- Re-state run results, scope, or risk in the pull-request body when any of them changes.

When the project runs a maintained review or triage ladder, describe only the contributor-facing profile signal and policy link. Use a table only when two or more profiles are maintained:

```markdown template
| [INDEX] | [PROFILE]   | [TRIGGER]  | [POLICY_LINK]   | [CONTRIBUTOR_ACTION] |
| :-----: | :---------- | :--------- | :-------------- | :------------------- |
|   [1]   | <profile-a> | <signal-a> | <policy-link-a> | <action-a>           |
|   [2]   | <profile-b> | <signal-b> | <policy-link-b> | <action-b>           |
```

Required profile fields are `Trigger`, `Policy link`, and `Contributor action`. Omit the profile mapping when the project runs a single flat review path or when no maintained policy proves the profiles. Branch protection, merge conditions, and repository policy belong to the maintained repository-policy route.

## [13]-[SECURITY_REPORTS]

Route every vulnerability report to the project's maintained security policy, enabled private vulnerability-reporting channel, or coordinated vulnerability-reporting channel, and keep that route out of the normal issue and pull-request flow. When no coordinated route is documented, instruct reporters to ask through the published source-routing channel for the preferred security contact without publishing exploit details.

Carry only the private route and no policy body. Do not embed a `SECURITY.md` template, disclosure timeline, bounty promise, supported-version policy, advisory workflow, embargo rule, or response-time claim; those belong to the security-policy route.

## [14]-[MAINTENANCE]

Review a contributing guide when accepted contribution paths, host discovery behavior, issue or pull-request templates, branch protection, required checks, gate commands, sign-off, contributor agreement, commit-title convention, security-report route, review policy, contributor help route, or documentation-travel rule changes. Update the guide from real review failures when contributors repeatedly choose the wrong entry artifact, omit required confirmation, report gates ambiguously, publish security details publicly, or ask for hidden repository-policy rules. Delete or route dead contribution paths instead of preserving them as legacy invitations.

## [15]-[BOUNDARIES]

These adjacent standards own routed material:

[TASK_ROUTES]:
- [README.md](../README.md) carries document-type routing, the reader-need map, placement, and lifecycle for new and changed docs.
- [how-to.md](how-to.md) carries normal repeatable procedures.
- [runbook.md](runbook.md) carries operational symptom-to-fix procedures, rollback, escalation, communication, and confirmation capture.
- [onboarding.md](../learning/onboarding.md) carries agent ramp, read-first path, constraints, first safe action, confirmation, and stop rules.
- [roadmap.md](../explanation/roadmap.md) carries implementation sequence, active milestone bodies, phase/task state, terminal work, documentation handoffs, progress, and task exit confirmation.

[POLICY_CONFIRMATION]:
- [proof.md](../proof.md) carries confirmation strength and claim-level reporting for workflows, enforced conventions, sign-off, pull requests, and unrun gates.
- [test-strategy.md](../explanation/test-strategy.md) carries gate taxonomy, gate routing, gate cost, flake policy, and portfolio-level confirmation.
- [support-matrix.md](../reference/support-matrix.md) carries supported versions, platforms, runtimes, deprecation, and end-of-support facts.
- Maintained repository-policy and security-policy documents carry branch protection, merge permissions, disclosure, support-window, and advisory policy when they exist; otherwise this guide routes those topics to source-routing checks without becoming the policy body.
