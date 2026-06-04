---
description: Standard for contribution guide documentation
---

# Contributing standards

Contributing guides explain who can contribute, which contribution paths are
accepted, how to prepare a change, how to prove it, and what review expects.
They are workflow documents for contributors, not architecture catalogs,
maintainer runbooks, onboarding curricula, or security-policy templates.

## Use when

Use a contributing guide when readers need:

- accepted contribution paths;
- setup and workflow for normal changes;
- quality-gate and pull-request evidence expectations;
- review collaboration rules;
- security-report routing.

Do not use a contributing guide for role readiness, incident response, test
taxonomy, governance, or security disclosure policy.

## External basis

Use hosting-platform contributor guidance for discoverable placement, open
source contributor practice for issue and pull-request boundaries, and security
reporting guidance for vulnerability-routing boundaries. Repository truth,
maintained automation, branch rules, review settings, and current commands own
factual workflow claims.

## Placement

Use one discoverable contribution guide per repository or documentation corpus.
Preferred locations are `.github/CONTRIBUTING.md`, `CONTRIBUTING.md`, or
`docs/CONTRIBUTING.md`, following hosting-platform discovery behavior.

Use owner-local contribution guides only when a subproject has materially
different contribution rules. Link the owner-local guide from the root guide
instead of duplicating shared setup, conduct, review, or security-reporting
rules.

## Required structure

```markdown
# Contributing

## Scope
## Ways to contribute
## Before you start
## Setup
## Workflow
## Quality gates
## Documentation
## Pull requests
## Review
## Security reports
## Getting help
```

Omit a section only when it is irrelevant or replaced by a directly linked owner
document. Do not keep placeholder sections, template instructions, or maintainer
notes in the published guide.

## Contributor paths

State accepted contribution paths with enough detail for a reader to choose the
right first action:

- issues for bug reports, proposals, reproduction details, and questions that
  need project discussion;
- pull requests for requested, discussed, or small self-contained changes;
- discussions, forums, chat, mailing lists, or support channels only when they
  are maintained;
- maintainer consultation before broad, expensive, compatibility-breaking,
  governance, security-sensitive, or cross-owner work.

Make non-code contributions first-class when the project accepts them. Docs,
tests, examples, triage, reproduction cases, design feedback, accessibility
fixes, translations, reviews, and release notes may all be valid paths when
maintainers can review them.

## Setup and workflow

Setup must be short, current, and sufficient for the normal contribution path.
Link deeper onboarding, architecture, build, or platform docs instead of
embedding them.

Workflow rules must state:

- how to find an issue or propose work;
- whether to fork, branch, or request access;
- expected branch, commit, or changeset discipline;
- how to keep a change scoped and reviewable;
- when generated files, dependency updates, screenshots, artifacts, or docs must
  be included;
- how to recover or ask for help when setup or validation fails.

Do not claim continuous integration, required checks, branch protection,
maintainer response times, or automation behavior unless the repository or host
configuration proves it.

## Quality gates and documentation

Quality gates must point to current runnable commands or maintained quality
documentation. For each change family, state:

- what command or review gate proves the change;
- what evidence to include in the pull request;
- when a broader gate is required.

Route new docs through [documentation-system.md](../documentation-system.md),
then the matching document-type standard. Use [proof.md](../proof.md) for
evidence rules.

The guide should state when docs must change with code, configuration,
generated contracts, user-visible behavior, support status, or operational
procedure. It should not carry the documentation style guide, architecture map,
API catalog, or release history.

## Pull requests and review

Pull request guidance must require:

- concise summary;
- paths, owners, or areas touched when that helps route review;
- commands, checks, status checks, or review gates used as proof;
- known gaps, unrun checks, follow-up work, or reviewer access needs;
- screenshots, recordings, logs, generated artifacts, or reproduction steps
  when visual, runtime, operational, or user-facing behavior changed;
- links to related issues, discussions, designs, or decisions when they govern
  the change.

Review guidance should explain how contributors and maintainers collaborate:

- keep technical discussion public unless sensitive information is involved;
- answer review comments with either a change, a question, or a reasoned
  disagreement;
- keep unrelated changes out of a contribution;
- update the pull request body when proof, scope, or risk changes;
- describe review or triage ladders when the project supports them.

## Security reports

Contributing guides must route vulnerability reports to the project's security
policy or private vulnerability reporting channel. If no private path is known,
tell reporters to ask maintainers for the preferred security contact without
publishing exploit details.

Keep security reporting separate from normal issue and pull request workflow.
Do not embed a full `SECURITY.md` template, disclosure timeline, bounty promise,
supported-version policy, or advisory workflow in the contribution guide.

## Boundaries

- README files welcome users and route contributors to the contribution guide.
- Onboarding guides prepare new contributors, maintainers, or operators for a
  role.
- Test strategies define gate taxonomy, ownership, cost, and flake policy.
- Runbooks handle operational incidents and recovery.
- Security policies define private reporting and advisory handling.
- Codes of conduct define behavior standards and enforcement contacts.
- Governance documents define roles, authority, voting, and maintainer
  lifecycle.

## Review checklist

- [ ] Placement is discoverable by the hosting platform.
- [ ] Scope and accepted contribution paths are clear.
- [ ] Setup is current, bounded, and linked to deeper docs when needed.
- [ ] Workflow explains when to open an issue, pull request, or discussion.
- [ ] Quality gates are current, runnable, or linked to maintained automation.
- [ ] Pull request evidence expectations are explicit.
- [ ] Review expectations keep normal collaboration public.
- [ ] Documentation changes route through this standards corpus.
- [ ] Security reporting points to a private or coordinated process without
      becoming a security-policy template.
