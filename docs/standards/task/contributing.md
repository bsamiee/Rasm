# [CONTRIBUTING_STANDARDS]

A contributing guide tells a prospective contributor which contribution paths the project accepts, how normal changes move from intent to review, what evidence proves the work, and how pull requests converge. It is a contributor workflow document, not onboarding, architecture, gate taxonomy, incident response, repository policy, support policy, or security-disclosure policy. Keep adjacent concerns in their routes and link them only where the contributor must act on the boundary.

## [1][USE_WHEN]

Use a contributing guide when readers need accepted contribution paths, normal-change workflow, quality-gate evidence, pull-request evidence, review-collaboration rules, or security-report routing. Do not use it for readiness, incident response, gate taxonomy, repository policy, support status, or vulnerability-disclosure policy. Route those by topic to onboarding, runbook, test-strategy, support-matrix, and maintained repository-policy or security-policy sources when one exists; otherwise require a source-routing check before the contributor acts.

Authoring contract:
- Agent use: prove the project accepts the contribution path, name the first artifact and gates, and keep repository policy, security, support, and readiness in their routes.
- Required produced structure: `Scope`, `Ways to contribute`, `Setup workflow`, `Quality gates`, `Pull requests`, `Review`, `Security reports`, `Boundaries`, and `Checklist`.
- Section cardinality: one accepted-path table; one setup workflow; one quality-gate mapping only when maintained gates exist; conditional help, documentation, and maintenance sections appear only when their trigger changes contributor action.
- Adjacent checks: check onboarding, runbook, test strategy, support matrix, proof, README, API, code documentation, how-to, roadmap, architecture, and maintained security or repository-policy sources only when the contributor workflow or evidence changes.
- Maintenance triggers: update the guide when accepted paths, host discovery, templates, branch protection, required checks, command gates, sign-off, security route, review policy, help route, or documentation-travel rule changes.

## [2][CONTRIBUTION_BASELINES]

Anchor contribution conventions to current project policy first and primary platform conventions second. A guide may mention a host surface, commit format, sign-off, code of conduct, license, contribution location, review ladder, or security route only when repository configuration, host settings, or maintained policy proves the project uses it.

Use external contribution conventions as placement and syntax constraints, not as project obligations. GitHub contributing-guideline conventions allow `CONTRIBUTING` files in repository root, `.github/`, or `docs/`, surface the selected file during issue and pull-request creation plus repository contribution surfaces, and choose among multiple repository-local contributing files in `.github/`, root, then `docs` order. GitHub default community-health files are fallbacks for repositories that lack their own file of that type, using `.github/`, root, then `docs` precedence. Evidence: [GitHub contributing guidelines](https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/setting-guidelines-for-repository-contributors) and [GitHub default community-health files](https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/creating-a-default-community-health-file).

Conventional Commits 1.0.0 requires a type and description in `<type>[optional scope][!]: <description>`; body and footers are optional. A project may enforce that convention or a stricter subset only when repository configuration or maintained policy proves it. DCO supplies certification text, `git commit --signoff` supplies a project-defined `Signed-off-by` trailer behavior, and the project must prove whether every commit requires that trailer. Evidence: [Conventional Commits 1.0.0](https://www.conventionalcommits.org/en/v1.0.0/), [Developer Certificate of Origin 1.1](https://developercertificate.org/), and [Git `commit --signoff`](https://git-scm.com/docs/git-commit).

A normal contributing guide routes vulnerability reports to the maintained security policy, enabled private vulnerability-reporting channel, or coordinated disclosure route. If no route is documented, it tells reporters to ask maintenance routes for the preferred security contact without publishing exploit details. Do not publish a disclosure timeline, bounty promise, support-window rule, or advisory workflow in `CONTRIBUTING`; those belong to the security-policy route.

Claim an enforced convention only from repository truth: host settings, branch-protection rules, required checks, workflow files, commit linting, sign-off checks, pull-request templates, issue templates, CODEOWNERS, or maintained policy documents. Do not import a `feat:` or `fix:` prefix rule, a `Signed-off-by` line, a contribution location, response-time promise, branch-protection claim, or security-reporting path from external examples alone. If host or policy confirmation is unavailable during authoring, state the missing configuration and omit the obligation from the contributor workflow.

## [3][WAYS_CONTRIBUTE]

A contributing guide carries one row per accepted, reviewable contribution path. Each row names the contributor intent, entry artifact, prerequisite when one gates action, and scope bound, so a contributor picks the correct first action without reading the whole guide. Remove paths the project does not accept or cannot review; do not leave empty rows, speculative routes, or maturity-based variants.

Use this path-row shape only for project-accepted paths:

```markdown template
| [INDEX] | [PATH]      | [INTENT]             | [ENTRY_ARTIFACT] | [PREREQUISITE]                 | [SCOPE_BOUND]          |
| :-----: | :---------- | :------------------- | :--------------- | :----------------------------- | :--------------------- |
|   [1]   | <path name> | <contributor intent> | <entry artifact> | <prerequisite, or `—` if none> | <accepted-scope bound> |
```

Required path fields per row: contributor intent, entry artifact, and scope bound. The prerequisite field is required when an action is gated and marked `—` only when no prerequisite exists. `Non-code` is a path only when maintenance routes can review the work; it may include reproduction cases, design feedback, accessibility fixes, translations, reviews, examples, and release notes when the project maintains those routes.

Route broad, expensive, compatibility-breaking, security-sensitive, or cross-scope work to a source-routing check before the contributor spends implementation effort. Use prose for a single route-away condition. Use a decision table only when two or more independent conditions jointly choose the entry artifact. Use a route-selection flowchart only when normal contribution, security-sensitive work, and consultation-before-implementation all exist and the branch-and-rejoin path would be hard to follow as prose.

Show a near-miss only when authors tend to publish vague path bounds:

```markdown conceptual
INTENT: land fix
PREREQUISITE: local gate
SCOPE_BOUND: self-contained; no breaking API or cross-scope migration
```

```markdown rejected
INTENT: quick fixes
PREREQUISITE: unspecified
SCOPE_BOUND: broad and unbounded
```

## [4][REQUIRED_STRUCTURE]

The published guide uses a required base plus conditional additions. Each `##` heading is a standalone retrieval unit; renumber headings in document order after inserting or omitting conditional sections.

```markdown template
# [CONTRIBUTING]

<Lead: accepted contribution surface and the highest-risk contribution boundary.>

## [1][SCOPE]

## [2][WAYS_CONTRIBUTE]

## [3][SETUP_WORKFLOW]

## [4][QUALITY_GATES]

## [5][PULL_REQUESTS]

## [6][REVIEW]

## [7][SECURITY_REPORTS]

## [8][BOUNDARIES]

## [9][CHECKLIST]
```

Add these conditional sections only when their trigger applies:

```markdown template
## [N][BEFORE_YOU_START]

## [N][GETTING_HELP]

## [N][DOCUMENTATION_CHANGES]

## [N][MAINTENANCE]
```

Required sections carry one purpose each: `Scope` states accepted surfaces and route-away topics; `Ways to contribute` selects the first artifact; `Setup workflow` reaches a first-gate-passing tree and states enforced workflow facts; `Quality gates` names contributor-facing results; `Pull requests` records review evidence; `Review` makes threads converge; `Security reports` routes private vulnerability detail; `Boundaries` names adjacent routes; `Checklist` verifies the published guide.

Conditional sections appear only when their trigger holds: `Before you start` for conduct, license, sign-off, discussion, account, or agreement prerequisites; `Getting help` immediately after quality gates when setup, permission, route, or gate blockers interrupt the path; `Documentation changes` when docs travel with changed truth; `Maintenance` when the guide itself is an actively maintained contributor contract. Omit a placeholder rather than publishing it empty. Do not keep template instructions, maintenance route notes, or speculative routes in the published guide.

## [5][SCOPE_RULES]

`Scope` states what the project accepts before it teaches any workflow. It carries the accepted contribution surface, the public or private entry route for each surface, topics routed away, and the highest-risk boundary, such as security-sensitive, breaking, or cross-scope work requiring direction first.

Do not publish broad encouragement without a reviewable path. A contribution guide that invites work the project cannot review creates contributor waste and maintenance route ambiguity. When no maintained route exists for an excluded topic, route the contributor to maintenance route consultation instead of inventing a policy in `CONTRIBUTING`.

## [6][BEFORE_YOU_START]

State the conduct, license, sign-off, and prerequisite facts a contributor must accept before opening any path. Carry the route and acceptance action, not the policy body.

- Link the code of conduct and state that contribution implies acceptance, when the project enforces one.
- Name the contribution license or sign-off requirement, when one governs the change: inbound license, `Signed-off-by` Developer Certificate of Origin line, or contributor-license-agreement route.
- State how to add a sign-off with `git commit --signoff` only when the project requires DCO sign-off.
- Name required accounts, permissions, or contributor agreements only when they gate contribution.
- Link the discussion or maintenance route-contact channel for questions that are not yet a contribution.

Do not embed prerequisite knowledge, first-task guidance, or readiness here; route those to onboarding. Do not embed the full code-of-conduct text, license body, DCO body, or contributor-license-agreement. Route each to its maintained source.

## [7][SETUP_WORKFLOW]

Setup states the commands a contributor on the normal path runs to reach a working tree that passes the first gate; it carries nothing else. Link deeper onboarding, architecture, build, platform, or reference material instead of embedding it. A setup command that was not checked during guide maintenance stays provisional beside the command, using the claim-level rules in [proof.md](../proof.md).

Workflow states each enforced fact the contributor must follow:
- How to find an issue to take or how to propose new work.
- Whether to fork, branch from the default branch, or request write access.
- The branch, commit, changeset, or pull-request-title discipline the project enforces.
- Any commit-message convention, such as Conventional Commits `<type>[optional scope][!]: <description>`, only when the project enforces it.
- How to keep one change to one concern so review stays tractable.
- When generated files, dependency-manifest updates, screenshots, artifacts, or docs must travel with the code.
- How to recover or ask for help when setup or a gate fails.

State each branch condition before its action: `If <signal>, do <action>`. Do not claim continuous integration, required status checks, branch protection, maintenance route response time, commit-convention enforcement, sign-off enforcement, or automation behavior unless repository or host configuration proves it.

## [8][QUALITY_GATES]

Quality gates name the runnable command or review gate that proves a contributor's change and the result to attach. State the contributor-facing result per change family so a contributor can run the maintained gate and report gaps without learning the whole gate taxonomy.

The change-family-to-gate mapping is repository truth, not prose to invent here. Publish the mapping only when the repository proves concrete commands or maintained review gates:

```markdown template
| [INDEX] | [CHANGE]        | [GATE]                                       | [RESULT_TO_REPORT]          | [KNOWN_GAP]             |
| :-----: | :-------------- | :------------------------------------------- | :-------------------------- | :---------------------- |
|   [1]   | <change family> | <runnable command or maintained review gate> | <command/status and result> | <unrun or failing gate> |
```

Required gate fields per row: change family, gate, result to report, and known gap. Point every gate at a current runnable command or maintained quality document; state honestly when a gate is a human review rather than an executable command, and never assert a gate passed unless it ran in the change or a current status check proves it. Route gate taxonomy, gate routing, flake policy, escalation thresholds, and portfolio-level proof to [test-strategy.md](../explanation/test-strategy.md) or the maintained quality route.

Show a gap in the row where the contributor reports a result, not in a separate apology paragraph:

```markdown conceptual
| [INDEX] | [CHANGE]        | [GATE]                        | [RESULT_TO_REPORT]             | [KNOWN_GAP]                                                       |
| :-----: | :-------------- | :---------------------------- | :----------------------------- | :---------------------------------------------------------------- |
|   [1]   | Markdown update | `git diff --check -- <paths>` | [PASS] no whitespace drift     | none                                                              |
|   [2]   | Runtime change  | maintained integration review | [SKIP] validator access needed | unrun: protected scenario requires the runtime verification route |
```

Delete the gate table when no maintained gate or review route exists. A polished table of unproved commands is filler, not contributor guidance.

## [9][GETTING_HELP]

Include `Getting help` only for contribution blockers that prevent a contributor from completing a reviewable path: setup command failure, missing permission, unclear route, inaccessible required gate, or a review question that needs maintained source direction. Link the maintained discussion or maintenance contact route only when the project publishes one. Do not use this section for user support, onboarding, repository-policy questions, incident response, or vulnerability reports; route those topics to their routes.

## [10][DOCUMENTATION_CHANGES]

Include `Documentation changes` when contributions can alter documentation truth: code behavior, configuration, generated contracts, user-visible behavior, support status, gate policy, route practice, contribution workflow, operational procedure, or public entry route. Route a new or changed document through the standards index by topic, then through the matching document-type standard; this guide carries the trigger, not the adjacent document.

When documentation travels with a change, use a short handoff record instead of a loose link or copied section:

```markdown template
Changed fact: <code behavior, configuration, generated contract, procedure, support fact, or workflow>
Consumed by: <API, code-documentation, README, support matrix, test strategy, roadmap, architecture, how-to, runbook, tutorial, onboarding, contributing, or reference>
Use in this document: <contributor action: update existing doc, create routed doc, or state no maintained route exists>
Evidence: <command, generated contract, reviewed artifact, status check, or stated gap>
Update when: <changed fact, generated artifact, procedure, support row, gate, route, or workflow changes>
Close when: <the consuming document updates, the guide states no maintained route exists, or the fact routes away>
Route-away: <adjacent document body that remains outside the contributing guide>
```

Use richer integration only when the adjacent document changes contributor action or review evidence. API contract changes usually update API documentation and any migration how-to; public symbol intent changes route to code documentation; support status changes route to the support matrix and any README entry route; new contributor gates update both contributing and test strategy; onboarding updates only when a workflow change affects readiness or first safe tasks. A link is enough when the adjacent document only provides background.

## [11][PULL_REQUESTS]

A pull request is accepted for review when its body carries the change summary, run results, known gaps, scope, and required self-checks. Missing required results are a request-changes condition, not a validator nicety. Use the host template when one exists; otherwise use one body shape with checklist items embedded, not a separate checklist plus a duplicate template:

```markdown template
Summary: <one to three sentences naming the change and motivation>

Results:
    - <command, status check, or review gate>: <result>

Known gaps: <none, or unrun/failing gate with reason and resolution route>

Scope: <paths, areas, or routes touched; one concern only>

Conditional evidence: <screenshots, recordings, generated artifacts, reproduction steps, governing issue, discussion, design, or decision; omit when absent>

Required self-check:
    - [ ] Run results are attached or the gap is stated.
    - [ ] Unrelated changes are excluded.
    - [ ] Enforced title, commit, or sign-off policy is satisfied, when repository policy requires it.
```

State an unrun gate as unrun rather than implying it passed. Update the pull-request body when run results, scope, or risk change during review. Add paths, routes, visual evidence, runtime evidence, generated artifacts, or governing decisions only when those facts route review.

## [12][REVIEW]

Review guidance defines how contributors and maintenance routes collaborate so a thread converges. Apply these rules to every review:
- Keep technical discussion public unless it carries sensitive information.
- Answer each review comment with a change, a question, or a reasoned disagreement; silence is not a response.
- Keep unrelated changes out of the contribution.
- Re-state run results, scope, or risk in the pull-request body when any of them changes.

When the project runs a maintained review or triage ladder, describe only the contributor-facing profile signal and policy link. Branch protection, merge conditions, and repository policy belong to the maintained repository-policy route. Omit the profile table entirely when the project runs a single flat review path or when no maintained policy proves the profiles.

```markdown template
| [INDEX] | [PROFILE]      | [TRIGGER]               | [POLICY_LINK]                   | [CONTRIBUTOR_ACTION]        |
| :-----: | :------------- | :---------------------- | :------------------------------ | :-------------------------- |
|   [1]   | <profile name> | <review-routing signal> | <maintained review-policy link> | <contributor-facing action> |
```

Required profile fields per row: trigger, policy link, and contributor action. Do not create profile rows for validator convenience alone; a maintained policy must consume them.

## [13][SECURITY_REPORTS]

Route every vulnerability report to the project's maintained security policy, enabled private vulnerability-reporting channel, or coordinated vulnerability-reporting channel, and keep that route out of the normal issue and pull-request flow. When no coordinated route is documented, instruct reporters to ask maintenance routes for the preferred security contact without publishing exploit details.

Carry only the private route and no policy body. Do not embed a `SECURITY.md` template, disclosure timeline, bounty promise, supported-version policy, advisory workflow, embargo rule, or response-time claim; those belong to the security-policy route.

## [14][FORMAT_CHOICES]

Choose the smallest form that preserves the contribution workflow fact:
- Use the path table only for accepted contribution paths the project can review.
- Use the quality-gate table only when each row has a maintained command or review gate and a known-gap column.
- Use the pull-request body template for asserted completion, run results, known gaps, and self-checks.
- Use a decision table only when independent conditions jointly choose the entry artifact.
- Use a route-selection flowchart only when normal, security-sensitive, and consultation routes all exist and branch visually.
- Use a documentation handoff record only when a contribution changes adjacent documentation truth.
- Keep security routing as prose or a short route record, not a public vulnerability-disclosure policy.

## [15][MAINTENANCE]

Review a contributing guide when accepted contribution paths, host discovery behavior, issue or pull-request templates, branch protection, required checks, gate commands, DCO, CLA, sign-off, commit-title convention, security-report route, review policy, contributor help route, or documentation-travel rule changes. Update the guide from real review failures when contributors repeatedly choose the wrong entry artifact, omit required evidence, report gates ambiguously, publish security details publicly, or ask for hidden maintenance route policy. Delete or route dead contribution paths instead of preserving them as legacy invitations.

## [16][BOUNDARIES]

These adjacent standards own routed material:
- [README.md](../README.md) carries document-type routing, the reader-need map, placement, and lifecycle for new and changed docs.
- [runbook.md](runbook.md) carries operational symptom-to-fix procedures, rollback, escalation, communication, and evidence capture.
- [onboarding.md](../learning/onboarding.md) carries agent ramp, read-first path, constraints, first safe action, validation, and stop rules.
- [proof.md](../proof.md) carries evidence strength and claim-level reporting for workflows, enforced conventions, sign-off, pull requests, and unrun gates.
- [test-strategy.md](../explanation/test-strategy.md) carries gate taxonomy, gate routing, gate cost, flake policy, and portfolio-level evidence.
- [support-matrix.md](../reference/support-matrix.md) carries supported versions, platforms, runtimes, deprecation, and end-of-support facts.
- Maintained repository-policy and security-policy documents carry disclosure, support-window, and advisory policy when they exist; otherwise this guide routes those topics to source-routing checks without becoming the policy body.

## [17][CHECKLIST]

Use this checklist by group:

Scope and policy:
- [ ] Placement is discoverable by the hosting platform or explicitly routed from the repository entrypoint.
- [ ] No CI, status-check, branch-protection, merge-condition, response-time, sign-off, security, merge-permission, CODEOWNERS, template, DCO, or commit-convention claim appears without host, repository, or maintained-policy confirmation.
- [ ] `Scope` states accepted contribution surfaces and route-away topics before workflow details.

Workflow and results:
- [ ] Every accepted contribution path has intent, `ENTRY_ARTIFACT`, and `SCOPE_BOUND`.
- [ ] Conduct, license, and sign-off prerequisites are linked, not embedded, when the project enforces them.
- [ ] Setup commands either have current run results beside the claim or state the unrun gate.
- [ ] Workflow states each enforced fact, including any commit or title convention, and writes conditions before actions.
- [ ] Each quality-gate row names a runnable command or maintained review gate, the result to report, and how to state gaps.
- [ ] Pull-request evidence uses one body shape and marks unrun gates honestly.

Structure and examples:
- [ ] Review rules keep normal collaboration public and require every comment to receive a response.
- [ ] Documentation-change triggers route new docs through the standards corpus by topic and use the handoff record only when docs travel with the change.
- [ ] `Getting help` appears only for contribution blockers and routes support, onboarding, incidents, repository policy, and security reports elsewhere.
- [ ] Security reporting points to a private or coordinated route without becoming a security-policy template.
- [ ] Format choices match their purpose: path and gate tables are proved, PR completion is a body template, and security routing is not a policy body.
- [ ] Maintenance triggers cover contribution paths, host templates, gates, sign-off policy, security route, review policy, and contributor help routes.
