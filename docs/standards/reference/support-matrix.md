---
description: Standard for support matrices
---

# Support matrix standards

Support matrices state which runtime, platform, version, feature, or integration
surfaces are supported, limited, deprecated, retired, compatible, or
unsupported. They are policy-backed reference documents, not roadmaps, release
notes, or procedures.

## Use when

Use a support matrix when readers need to compare:

- product, runtime, platform, host, toolchain, browser, device, or deployment
  support;
- component version skew and compatibility bounds;
- feature availability by plan, edition, runtime, API version, region, or
  integration;
- deprecation, removal, retirement, or migration status.

Do not use a support matrix for future work sequence, release narrative,
step-by-step migration, or operational recovery.

## Source of truth

Use this order for support truth:

1. Current source, manifests, generated contracts, release metadata, package
   indexes, and runnable compatibility checks.
2. Official lifecycle, support, release, deprecation, migration, and
   compatibility policies from the owning vendor or upstream project.
3. Maintainer-controlled release notes, migration guides, known limitations,
   issue records, or support announcements.
4. Local support-matrix prose.

Do not invent local lifecycle semantics when upstream policy owns the phase,
date, support entitlement, or compatibility rule.

## Placement

- Shared support corpus: `docs/support/<surface>.md`.
- Reference-adjacent matrix: owner reference corpus when the matrix is lookup
  data.
- Package-local matrix: `{owner}/SUPPORT.md` only when support truth is local to
  that owner.

Keep the matrix near the owner that can refresh it.

## Profiles

- Product lifecycle: release lines, support phases, dates, retirement, and
  security posture.
- Runtime or platform support: operating systems, language runtimes, host
  versions, toolchains, browsers, devices, or deployment environments.
- Compatibility matrix: component version skew, supported combinations,
  minimum and maximum versions, and upgrade-order constraints.
- Feature availability: capabilities by plan, edition, runtime, platform, API
  version, region, or integration.
- API or feature deprecation: deprecated surfaces, replacements, warnings,
  removal dates, and migration paths.

Choose one primary profile. Split the page when it becomes both a support
policy and a task guide or release narrative.

## Required structure

```markdown
# <Surface> support matrix

## Scope
## Source of truth
## Status vocabulary
## Matrix
## Limitations
## Deprecations and removals
## Migration paths
## Evidence
## Review trigger
## Related
```

`Deprecations and removals`, `Migration paths`, and `Related` are optional when
the surface has no deprecated or adjacent content. Source, status vocabulary,
matrix, evidence, and review trigger are required for drift-prone support
claims.

## Status vocabulary

Define only statuses used by the matrix:

- `Supported`: actively tested, maintained, and eligible for stated channels.
- `General availability`: current stable release or feature line.
- `Maintenance`: receives the maintenance classes named by the owning policy.
- `Limited`: supported only for stated capabilities, environments,
  entitlements, or bounds.
- `Deprecated`: still present, discouraged, and scheduled or eligible for
  removal under a stated policy.
- `End of support`: no new fixes, security updates, assisted support, or content
  updates are expected unless an explicit program says otherwise.
- `Retired`: service, product, release, or feature is removed or no longer
  available.
- `Unsupported`: not intended to work.

Do not use color, icons, badges, or vague labels as the only status signal.

## Matrix rules

Use a table when row scanning is the clearest way to compare support facts. Use
subsections or grouped lists when rows need paragraphs, nested steps, or long
migration explanation.

Rows must be independently useful. Include the smallest stable field set:

- surface, product, component, feature, integration, runtime, platform, or API;
- version, release line, channel, edition, plan, region, or environment;
- lifecycle phase or status;
- support start, deprecation, maintenance start, removal, retirement, or
  end-of-life date;
- supported and unsupported capabilities;
- compatibility bounds and version-skew direction;
- dependency, entitlement, patch, or toolchain requirement;
- replacement surface or migration target;
- evidence source and last verification signal.

Do not copy a large generated or vendor-owned matrix when the official source is
stronger. Link the source and publish only the local subset that changes a
reader decision.

## Lifecycle and compatibility

Lifecycle rows must state what the phase means: security fixes, bug fixes,
feature work, documentation updates, assisted support, self-service support,
compatibility testing, and extended support options when those differ.

Compatibility matrices must state direction, supported skew, rolling-upgrade or
mixed-version limits, required upgrade order, and plausible unsupported
combinations.

Use exact dates when the owner publishes exact dates. Use month-level dates only
when the owner publishes month-level dates.

## Deprecations and migration

Deprecation entries must state deprecated surface, first deprecated version or
announcement date, current availability, warning signal when known, replacement,
removal version or policy window, behavior change, and source of truth.

Migration guidance in a support matrix is compact and decision-oriented. Name
source and target versions or features, direct or staged path, prerequisites,
known breaking changes, validation signal, and link to the owning how-to guide,
migration guide, release note, or API reference.

Move step-by-step migration work to a how-to guide. Move recovery to a runbook.

## Evidence and review trigger

Every support status needs evidence. Prefer local manifests, lockfiles,
generated contracts, compatibility test output, official lifecycle pages,
release notes, migration guides, security bulletins, and known-limitations pages
from the owner.

Use event-based review triggers whenever possible:

- upstream release, patch, or lifecycle-policy update;
- support announcement, retirement notice, or security advisory;
- package, runtime, host, platform, or generated-contract version change;
- compatibility test, certification, or conformance matrix change;
- migration guide or deprecation-policy update;
- local manifest, lockfile, toolchain, or supported-environment change.

## Boundaries

- Support matrices own support status, compatibility bounds, lifecycle dates,
  deprecations, removals, and migration targets.
- Reference documents own detailed command, field, API, package, and glossary
  lookup when support status is only one fact among many.
- API documentation owns generated or contract-backed API surface truth.
- Roadmaps own future work sequence and milestone proof.
- Release notes own what changed in a release.
- How-to guides own migration procedures.
- Runbooks own operational response, rollback, escalation, and recovery.

## Review checklist

- [ ] Scope defines the supported surface.
- [ ] Source of truth is explicit.
- [ ] Status terms are defined.
- [ ] Each status states maintenance, security, feature, support, and
      documentation implications when those differ.
- [ ] Tables are used only where row scanning improves support lookup.
- [ ] Lifecycle dates preserve the owner's precision.
- [ ] Compatibility bounds include direction, minimums, maximums, and skew when
      relevant.
- [ ] Deprecations distinguish deprecated, removed, retired, and unsupported
      states.
- [ ] Migration targets exist where needed.
- [ ] Step-by-step migration procedures are linked, not embedded.
- [ ] Every status has evidence.
- [ ] Review trigger is stated.
