# [REVIEW_STANCE]

Rasm is a design-first polyglot monorepo: the reviewable surface is a planning corpus — markdown pages whose embedded code fences are the work product — plus workflow and tooling JavaScript, Python operators, and configuration. Fence code is production source judged at full depth; simplifying a fence into a sketch is never a valid suggestion. Repo law rides `CLAUDE.md`, `AGENTS.md`, the root `README.md`, and `libs/.planning/campaign-method.md`; every diff is judged against those files and the `docs/stacks/<language>/` doctrine they route to, never against generic community convention. Doctrine is the floor, never the ceiling: a conformant-but-weak form is a finding whenever a stronger form exists.

## [01]-[STANCE]

- Presume defects until the pass finds none; dense, confident code is the prime suspect, and a clean-reading page is unproven, not clean.
- Verify every claim against the real domain — a geometry invariant, a schema contract, a package's actual API — never by ratifying prose, naming, or a comment's assertion. A claimed absence of a member verifies at the same bar as a positive citation: a false no-such-member claim steers every future rebuild into the inferior spelling.
- Settled corpus law outranks reviewer intuition: a finding contradicting a contract the owning page rules — a factory's declared parameters, a ruled composition form, a seam's admission placement — is wrong until that page is read and the claim verified against its stated contract; re-litigating a ruled design is a false positive, not caution.
- Every finding names the file, the anchor, the defect, and the stronger form demanded — precise enough that a fixer agent lands the repair without re-deriving the analysis.
- Demand root-cause depth: when a finding admits a literal patch and a deeper collapse, the collapse is the demanded form.
- Cross-surface coupling is standing duty: `docs/laws/topology.md` maps which surfaces obligate which counterparts, and a diff editing a listed SURFACE without its obligated counterparts in the same change is a finding naming the missing counterpart. `docs/laws/patterns.md` and `docs/laws/scars.md` are standing review law.
- Suppressions and gate bypasses are the finding, never the mechanics: a true positive is architecture pressure to fix the shape, a false positive is rule pressure to refine the rule, and a suppression directive is neither.
- Grade across the full severity scale; a review where every finding lands at one severity has averaged, not graded, and a pass that finds nothing after genuine attack states that as its verdict, never padded with manufactured findings.

## [02]-[ARCHITECTURE_LAW]

- One polymorphic entry per rail, discriminating on input shape. Parallel entry points, modality-named siblings, `get`/`getMany`/`getById` families, and direction-named twins of one correspondence are defects; demand absorption into the one owner, never a new sibling.
- Judge every surface from its consumers: a parameter a policy value or input shape reconstructs is knob spam; configuration ceremony pushed onto callers, or orchestration a consumer must hand-write, is a defect. Demand internalized lifecycle, routing, and policy with zero capability sacrifice.
- A long imperative body is a defect wherever a denser expression-shaped form exists: demand fold, combinator, table-driven, or generator forms over hand-rolled loops and branching, and demand surfaces and object types collapse into fewer, denser, richer owners.
- Domain logic rides typed result/effect rails; raw exceptions in domain flow, or dual error paradigms crossing one boundary, are defects.
- No compat shims, obsolete aliases, backwards-compat wrappers, or migration helpers anywhere: aggressive API breaking with every call site updated in the same change is the sanctioned path, and a preserved stale surface is the finding.
- Admission happens once at the boundary: a typed field's evidence is settled at its factory, so demanding re-validation of what the type already carries is the defect, never the finding.
- Dispatch is closed-world by default; an extensibility hook, open registry, or plugin seam is a defect unless extension must happen outside Rasm and a present consumer exists. A decorative operator overload without a proven algebra (monoid, semigroup, lattice) is rejected.

## [03]-[DEFECT_CLASSES]

Proven classes worth hunting at root-cause depth: required-field threading dropped by signature migrations, finiteness admission before range checks, identity preimages omitting output-affecting inputs, match/dispatch totality, frozen-payload discipline — mutable defaults, unsorted iteration reaching egress — per-stream timestamp re-basing, resource custody on every exit path, archive and metadata bomb gates, deterministic egress ordering. A multi-field hash preimage length-frames every variable-width field and count-frames every adjacent collection; separator-joined concatenation is rejected outright. A derived artifact keys on the content hash of its source, never path or mtime. Root discovery walks upward to a sentinel file, never a fixed parent depth. A wire token admits only its exact emission spelling; a tolerant parse re-emitting a normalized form forks the key.

## [04]-[DO_NOT_FLAG]

Deliberate house shapes that violate generic best practice — flagging them is a false positive:

- Aggressive API breaks with every call site updated in the same change: the sanctioned rename path.
- Dense single-expression bodies and heavy polymorphic dispatch: the bar, not obfuscation.
- Absent defensive guards inside domain logic: admission-once boundaries, not missing error handling.
- Sparse 1-2 line agent-facing comments: comment-law compliance, not missing documentation.
- A large file owning one full concern: sanctioned; file-size budgets do not exist here.
- Bracketed uppercase section dividers (`# --- [LABEL] ---`): source structure, not commented-out code.
- `TODO`/`FIXME` as source comment markers: sanctioned; the ban applies only to durable markdown.
- Rhino-rich host capture coexisting with host-neutral semantics: architecture, never duplication.
- Explicit match/dispatch arms over merged ternaries, and named intermediates on result rails: deliberate.
