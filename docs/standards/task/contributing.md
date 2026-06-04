# [CONTRIBUTING_STANDARDS]

A contributing guide tells a prospective contributor which paths the project accepts, how to prepare a change, what evidence proves it, and what review expects. It is a workflow document for contributors. Keep architecture catalogs, operational recovery, role curricula, gate taxonomy, and security-disclosure policy in their own owners, named here by topic and linked only at the boundary.

## [1][USE_WHEN]

Use a contributing guide when readers need accepted contribution paths, normal-change workflow, quality-gate evidence, pull-request evidence, review-collaboration rules, or security-report routing. Do not use it for role readiness, incident response, gate taxonomy, governance authority, or security-disclosure policy. Route those by topic to onboarding, runbook, test-strategy, and the maintained project policy when one exists; otherwise require maintainer consultation.

## [2][CANONICAL_SOURCE]

Anchor the published guide to the canonical contribution conventions, and record the proof beside the claim rather than asserting from memory.

- `Source of truth:` GitHub contribution-guideline and community-profile conventions; Conventional Commits 1.0.0 when the project enforces `<type>(<scope>): <description>`; Developer Certificate of Origin when the project requires per-commit `Signed-off-by`.
- `GitHub convention:` a `CONTRIBUTING` file in the repository root, `.github/`, or `docs/`, surfaced on issue and pull-request creation views and the repository contribute tab.
- `Guide contents:` steps for good issues and pull requests, links to external docs and the code of conduct, and community and behavioral expectations.
- `Last verified:` 2026-06-04.
- `Review trigger:` GitHub community-health-file guidance, the Conventional Commits specification, or the project's enforced commit or sign-off policy changes.

Claim a convention only when the repository enforces it. Do not import a `feat:`/`fix:` prefix rule, a sign-off requirement, or a community-health-file location into the guide unless project configuration or policy proves the project uses it.

## [3][WAYS_CONTRIBUTE]

A contributing guide carries one or more contribution paths, each with a distinct contributor intent, entry artifact, prerequisite, and accepted-scope bound. State each path the project accepts so a reader picks the correct first action without guessing. Keep the path facts together; shorten cells before splitting the structure. A small library may publish only the first two paths, while a multi-owner monorepo publishes all five.

| [INDEX] | [PATH]              | [INTENT]        | [ENTRY]          | [PREREQUISITE]       | [ACCEPTED_SCOPE]                    |
| :-----: | :------------------ | :-------------- | :--------------- | :------------------- | :---------------------------------- |
|   [1]   | Report              | defect evidence | Issue            | duplicate search     | repro, environment, expectation     |
|   [2]   | Propose             | agree direction | Issue or discussion | maintained path    | problem, direction, owners          |
|   [3]   | Patch (small)       | land fix        | Pull request     | local gates          | one concern, no compatibility break |
|   [4]   | Patch (coordinated) | land broad work | Issue then PR    | maintainer agreement | cross-owner or breaking work        |
|   [5]   | Non-code            | improve corpus  | Issue or PR      | reviewable path      | docs, tests, examples, triage       |

`Non-code` scope includes reproduction cases, design feedback, accessibility fixes, translations, reviews, and release notes when those paths are maintained and reviewable.

Required path fields per row: contributor intent, entry artifact, and accepted scope. The prerequisite field is required when an action is gated and omitted otherwise. Carry a non-code path as its own row when maintainers can review it; demote a path to "not accepted" by removing its row, not by leaving an empty cell. Route a contributor to maintainer consultation before broad, expensive, compatibility-breaking, governance, security-sensitive, or cross-owner work.

State the path-selection rule as a decision table when more than one signal routes a contributor, so a reader reaches the correct entry artifact without reading every row:

| [INDEX] | [CHANGE_SIGNAL]             | [COMPATIBILITY_BREAK] | [ENTRY_ARTIFACT]               |
| :-----: | :-------------------------- | :-------------------- | :----------------------------- |
|   [1]   | Defect or reproduction only | —                     | Issue                          |
|   [2]   | Self-contained fix          | No                    | Pull request                   |
|   [3]   | Cross-owner or expensive    | No                    | Issue first, then pull request |
|   [4]   | Any source change           | Yes                   | Issue first, then pull request |
|   [5]   | Security-sensitive          | Any                   | Private security route         |

## [4][REQUIRED_STRUCTURE]

The published guide uses the section set below. Each `##` heading is a standalone retrieval unit; order them so a reader meets scope before workflow and proof before review.

```markdown template
# [CONTRIBUTING]

## [1][SCOPE]

## [2][WAYS_CONTRIBUTE]

## [3][BEFORE_YOU_START]

## [4][SETUP_WORKFLOW]

## [5][QUALITY_GATES]

## [6][DOCUMENTATION_CHANGES]

## [7][PULL_REQUESTS]

## [8][REVIEW]

## [9][SECURITY_REPORTS]

## [10][GETTING_HELP]

## [11][BOUNDARIES]

## [12][EXAMPLE_CONTRIBUTION_PATH]

## [13][REVIEW_CHECKLIST]

```

Section cardinality:

| [INDEX] | [SECTION]                 | [CARDINALITY]         | [CARRIES]                                                                     |
| :-----: | :------------------------ | :-------------------- | :---------------------------------------------------------------------------- |
|   [1]   | Scope                     | Required, one         | Accepted paths and topics routed elsewhere                                    |
|   [2]   | Ways to contribute        | Required, one         | One path table row per accepted path                                          |
|   [3]   | Before you start          | Optional, zero or one | Prerequisite knowledge, conduct link, license note                            |
|   [4]   | Setup and workflow        | Required, one         | Commands to reach a first-gate-passing tree, then the enforced workflow facts |
|   [5]   | Quality gates             | Required, one         | One gate row per change family                                                |
|   [6]   | Documentation changes     | Optional, zero or one | When docs travel with the change                                              |
|   [7]   | Pull requests             | Required, one         | Required and conditional proof fields                                         |
|   [8]   | Review                    | Required, one         | Collaboration rules and review profiles                                       |
|   [9]   | Security reports          | Required, one         | Private vulnerability route                                                   |
|  [10]   | Getting help              | Optional, zero or one | Where to ask when blocked                                                     |
|  [11]   | Boundaries                | Required, one         | Adjacent owners for routed-away content                                       |
|  [12]   | Example contribution path | Optional, zero or one | One accepted and one rejected path shape when scope misuse is likely          |
|  [13]   | Review checklist          | Required, one         | Verification gates for the published guide                                    |

Repeatable unit: one path table row inside `Ways to contribute`, one per accepted path. Copy the row shape rather than reconstructing it:

```markdown template
| [INDEX] | [PATH]      | [INTENT]             | [ENTRY_ARTIFACT] | [PREREQUISITE]                 | [SCOPE_BOUND]          |
| :-----: | :---------- | :------------------- | :--------------- | :----------------------------- | :--------------------- |
|   [1]   | <path name> | <contributor intent> | <entry artifact> | <prerequisite, or `—` if none> | <accepted-scope bound> |
```

Omit an optional section only when it is irrelevant or fully replaced by one directly linked owner document. Omit a placeholder rather than publishing it empty. Do not keep template instructions or maintainer notes in the published guide.

## [5][BEFORE_YOU_START]

State the conduct, license, and prerequisite facts a contributor must accept before opening any path. Carry the route, not the policy text.

- Link the code of conduct and state that contribution implies acceptance, when the project enforces one.
- Name the contribution license or sign-off requirement, when one governs the change: the inbound license, the `Signed-off-by` Developer Certificate of Origin line, or the contributor-license-agreement route. State how to add a sign-off (`git commit --signoff`) when the project requires it.
- Link the support or discussion channel for questions that are not yet a contribution.

Do not embed the full code-of-conduct text, the license body, or the contributor-license-agreement; route each to its owner.

## [6][SETUP_WORKFLOW]

Setup states the commands a contributor on the normal path runs to reach a working tree that passes the first gate; it carries nothing else. Link deeper onboarding, architecture, build, or platform material instead of embedding it. Setup is current when every command in it ran against the repository at or after `Last verified`.

Workflow states six facts, each required when the project enforces it:

1. How to find an issue to take or how to propose new work.
2. Whether to fork, branch from the default branch, or request write access.
3. The branch, commit, or changeset discipline the project enforces, including any commit-message or pull-request-title convention (such as Conventional Commits `<type>(<scope>): <description>`) when the project enforces one.
4. How to keep one change to one concern so review stays tractable.
5. When generated files, dependency-manifest updates, screenshots, artifacts, or docs must travel with the code.
6. How to recover or ask for help when setup or a gate fails.

State each branch condition before its action: `If <signal>, do <action>`. Do not claim continuous integration, required status checks, branch protection, maintainer response time, commit-convention enforcement, or automation behavior unless repository or host configuration proves it.

`Evidence:` repository host config (branch-protection rules, required-checks list), the workflow files under `.github/`, and any commit-lint or sign-off-check configuration. `Review trigger:` default-branch name, branch-protection, required-check, fork-policy, or commit-convention change.

## [7][QUALITY_GATES]

Quality gates name the runnable command or review gate that proves a change, the evidence to attach, and the threshold that escalates to a broader gate. State the gate per change family so a contributor runs the minimal proof and no more.

The change-family-to-gate mapping is repository truth, not prose to invent here. A guide for a multi-rail repository states one record per change family:

| [INDEX] | [CHANGE] | [PROOF_GATE]                  | [EVIDENCE]          | [ESCALATES_ON]              |
| :-----: | :------- | :---------------------------- | :------------------ | :-------------------------- |
|   [1]   | Source   | owning build or analyzer      | command and result  | shared build, package, or config |
|   [2]   | Test     | targeted affected test        | command and count   | flaky or cross-target fail  |
|   [3]   | Docs     | `git diff --check` + doc gate | gate run and result | generated/navigation change |

Required gate fields per row: change family, proof gate, evidence to attach. The escalation-threshold field is required when a broader gate exists and omitted when no escalation applies. Point every gate at a current runnable command or maintained quality document; state honestly when a gate is a human review rather than an executable command, and never assert a gate passed unless it ran in the change or a current status check proves it.

## [8][DOCUMENTATION_CHANGES]

State when docs must change in the same contribution that alters their source truth: code behavior, configuration, generated contracts, user-visible behavior, support status, or operational procedure. Route a new or changed document through the standards index by topic, then through the matching document-type standard; this guide carries the trigger, not the style guide, architecture map, API catalog, or release history.

## [9][PULL_REQUESTS]

A pull request is accepted for review when its body carries the proof fields below. Missing required proof is a request-changes condition, not a reviewer nicety. Render the always-required self-check as a checklist so the definition of done is asserted, not implied:

- [ ] Summary of one to three sentences states the change and its motivation.
- [ ] Proof is attached: the commands, checks, status checks, or review gates run, with results.
- [ ] Known gaps are stated plainly or marked "none": unrun checks, follow-up work, reviewer-access needs.
- [ ] Single concern; unrelated changes are excluded.
- [ ] Commit-message or title convention is satisfied, when the project enforces one.
- [ ] Sign-off (`Signed-off-by`) is present on every commit, when the project requires it.

Attach these fields when their trigger applies:

| [INDEX] | [TRIGGER]                                           | [FIELD]                                                         |
| :-----: | :-------------------------------------------------- | :-------------------------------------------------------------- |
|   [1]   | Review routes by area                               | Paths, owners, or areas touched                                 |
|   [2]   | Visual, runtime, operational, or user-facing change | Screenshots, recordings, logs, generated artifacts, or repro steps |
|   [3]   | Existing direction governs the change               | Link to the governing issue, discussion, design, or decision    |

State an unrun gate as unrun rather than implying it passed. Update the pull request body when proof, scope, or risk changes during review.

## [10][REVIEW]

Review guidance defines how contributors and maintainers collaborate so a thread converges. Apply these rules to every review:

- Keep technical discussion public unless it carries sensitive information.
- Answer each review comment with a change, a question, or a reasoned disagreement; silence is not a response.
- Keep unrelated changes out of the contribution.
- Re-state proof, scope, or risk in the pull request body when any of them changes.

When the project runs a review or triage ladder, describe it as a profile so a contributor knows which review depth a change attracts:

| [INDEX] | [PROFILE]   | [TRIGGER]                     | [APPROVER]        | [MERGE_CONDITION]                  |
| :-----: | :---------- | :---------------------------- | :---------------- | :--------------------------------- |
|   [1]   | Light       | docs, tests, single-owner fix | owning maintainer | required checks green              |
|   [2]   | Standard    | multi-file source change      | owning maintainer | approval plus green checks         |
|   [3]   | Coordinated | cross-owner/breaking/security | each owner        | owner approvals, checks, direction |

Required profile fields per row: trigger, approver, merge condition. Omit the table entirely when the project runs a single flat review path.

## [11][SECURITY_REPORTS]

Route every vulnerability report to the project's maintained security policy or coordinated vulnerability-reporting channel, and keep that route out of the normal issue and pull-request flow. When no coordinated route is documented, instruct reporters to ask maintainers for the preferred security contact without publishing exploit details.

Carry only the route here. Do not embed a `SECURITY.md` template, a disclosure timeline, a bounty promise, a supported-version policy, or an advisory workflow; those belong to the security-policy owner.

## [12][EXAMPLE_CONTRIBUTION_PATH]

Show one accepted shape and one rejected shape so scope becomes a bound, not a mood.

Accepted — scope is a testable bound:

```markdown template
| [INDEX] | [PATH]        | [INTENT] | [ENTRY]      | [PREREQ]   | [ACCEPTED_SCOPE]         |
| :-----: | :------------ | :------- | :----------- | :--------- | :----------------------- |
|   [1]   | Patch (small) | land fix | pull request | local gate | self-contained; no break |
```

Rejected — scope is a vague qualifier a reviewer cannot apply:

```markdown rejected
| [INDEX] | [PATH]        | [INTENT]    | [ENTRY]      | [PREREQ]      | [ACCEPTED_SCOPE]    |
| :-----: | :------------ | :---------- | :----------- | :------------ | :------------------ |
|   [1]   | Patch (small) | quick fixes | pull request | a little prep | comfortable changes |
```

## [13][BOUNDARIES]

- [README.md](../README.md) owns document-type routing, the reader-need map, the placement decision, and the evidence-standard route for new and changed docs.
- [runbook.md](runbook.md) owns operational symptom-to-fix procedures, rollback, and escalation.
- [onboarding.md](../learning/onboarding.md) owns role readiness, first safe tasks, shadowing, and sign-off for contributors, maintainers, and operators.
- [test-strategy.md](../explanation/test-strategy.md) owns gate taxonomy, gate ownership, gate cost, and flake policy.
- [support-matrix.md](../reference/support-matrix.md) owns supported versions, platforms, runtimes, and end-of-support facts.
- Maintained governance and security-policy documents carry authority, disclosure, support-window, and advisory policy when they exist; otherwise this guide routes those topics to maintainer consultation without becoming the policy body.

## [14][REVIEW_CHECKLIST]

- [ ] Placement is discoverable by the hosting platform (root, `.github/`, or `docs/`).
- [ ] The guide anchors its conventions to a current source with `Last verified`, and claims no convention the repository does not enforce.
- [ ] Scope and every accepted contribution path are stated, each with intent, entry artifact, and accepted scope.
- [ ] Conduct, license, and sign-off prerequisites are linked, not embedded, when the project enforces them.
- [ ] Setup commands ran at or after `Last verified` and link deeper docs.
- [ ] Workflow states each enforced fact, including any commit or title convention, and writes conditions before actions.
- [ ] Each quality-gate row names a runnable command or maintained gate and its evidence.
- [ ] Pull-request required fields use the checklist form and unrun gates are marked unrun.
- [ ] Review rules keep normal collaboration public and each comment answered.
- [ ] Documentation-change triggers route new docs through the standards corpus by topic.
- [ ] Security reporting points to a private or coordinated route without becoming a security-policy template.
- [ ] No CI, status-check, branch-protection, response-time, or commit-convention claim appears without host or repository proof.
