---
description: Standard for README entry documents and hub indexes
---

# README standards

README files are entry documents. They orient readers at a repository,
directory, package, tool, or documentation corpus boundary, provide the smallest
usable adoption path, and route deeper content elsewhere.

## Use when

Use a README for:

- repository-level overview;
- package, tool, or directory-local entrypoint;
- documentation corpus hub;
- short local map to owner docs.

Do not use a README as an architecture document, API catalog, tutorial,
runbook, roadmap, or contribution workflow. Link those documents instead.

## External basis

Use Standard README for repository and library entries, hosting-platform rules
for rendering and discovery, and mature open-source practice for README scope.
The README is the front door and index, not the whole documentation site.

Repository source, manifests, generated contracts, and runnable tool output own
factual claims about names, commands, package status, and support.

## Profiles

- Root entry: repository-level overview for users, adopters, and contributors.
- Package entry: directory-local purpose, status, public entrypoints, and owner
  links.
- Hub index: documentation corpus map that helps readers choose the next page.
- Tool entry: command, operator, or local service entrypoint.

Use exactly one primary profile. Name it in the opening paragraph when the file
could be confused with another profile.

## README contract

- Name the file `README.md` unless a platform or localization standard requires
  a variant.
- Start with one H1 title matching the repository, package, directory, tool, or
  corpus name unless the mismatch is intentional and explained immediately.
- Put a one-paragraph description directly after title, badges, or banner.
- Prefer relative links for repository-local targets.
- Keep generated tables of contents out of short README files.
- Use badges only when they reflect real maintained automation, releases,
  security posture, or package metadata.
- Link security, contribution, support, governance, roadmap, architecture, and
  API documents when those concerns exist.
- Verify install, setup, usage, and command examples exactly as written or mark
  the limitation.

## Profile structures

Root entry:

1. Title.
2. Badges or banner when useful and truthful.
3. Short description.
4. Status, maturity, or support note when needed.
5. Install, setup, or quick start.
6. Minimal usage or first successful path.
7. Documentation map.
8. Help or support.
9. Security reporting link when applicable.
10. Contributing link when maintained.
11. Maintainers or governance link when useful.
12. License.

Package entry:

1. Purpose and boundary.
2. Current status.
3. Public entrypoints or supported import surface.
4. Local build, test, or usage command when directly runnable.
5. Related architecture, roadmap, design, API, or owner docs.
6. Constraints, non-goals, or known exclusions.
7. Owner, maintainer, or escalation link when durable.

Hub index:

1. Corpus purpose.
2. How to choose a child page.
3. Child links with one-line roles.
4. Maintenance or freshness trigger when the corpus can drift.

Tool entry:

1. Tool purpose and boundary.
2. Supported environment or prerequisites.
3. First successful command.
4. Common command map.
5. Output, artifact, or side effect locations.
6. Verification command or expected result.
7. Troubleshooting link or owner escalation.

## Boundaries

- README links to architecture for structure.
- README links to ADRs for durable decisions.
- README links to design documents for proposal history.
- README links to how-to or contributing docs for procedures.
- README links to API or reference docs for catalogs.
- README links to runbooks for operational recovery.
- README links to support matrices for support truth.
- README links to roadmap documents for sequence and future work.
- README links to tutorials for learning paths.

## Review checklist

- [ ] Profile is clear.
- [ ] Title, short description, and local link behavior follow the active
      platform baseline.
- [ ] First paragraph states purpose and boundary.
- [ ] Setup, usage, and commands are minimal and verified.
- [ ] Links route deeper content instead of duplicating it.
- [ ] Package, support, and status claims use current evidence.
- [ ] Badges, security links, contribution links, maintainers, and license
      sections are truthful for the profile.
