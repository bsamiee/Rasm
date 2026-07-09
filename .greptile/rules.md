# Review context — Rasm

C#/.NET-primary monorepo with TypeScript and Python lanes. Language doctrine is codified under docs/stacks/{csharp,typescript,python} — review against those documents, not generic community convention. Where a finding and the doctrine disagree, the doctrine wins.

## Design paradigms

- Functional programming exclusively; monadic railway-oriented flows unify error handling — never dual paradigms in one surface.
- The unit of design is the polymorphic dispatch surface, not the file. Fewer deep surfaces beat many shallow ones; a new capability is a case, row, or dispatch arm on the owning surface.
- Boundary kernels may use language-native control flow only where it preserves correctness, typing, performance, or interop clarity; domain logic never does.
- Central manifests own versions: Directory.Packages.props for NuGet, per-package package.json + committed pnpm lockfile for TS, pyproject.toml + uv for Python.

## Universal bar

Anticipate 10x functionality growth: surfaces absorb new modalities as rows, cases, or dispatch arms — never as new files, flags, or knobs. Defects: knob/param/flag spam, hardcoded values, fragile string plumbing, naive happy-path logic, hand-rolled reimplementations of capability the ecosystem already provides. External packages are first-class implementation material at full power, newest stable versions. Everything ships agent-first: composable, receipt-bearing, self-describing. Collapse spam relentlessly.

## Review priorities

1. Doctrine regressions (rails, dispatch, package custody) outrank style and naming.
2. New public surfaces demand justification against extending an existing owner.
3. Generated or lock content is never review substrate.
