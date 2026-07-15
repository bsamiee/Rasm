# Review charter — Rasm

Repo law rides the files.json reads: `CLAUDE.md` owns execution and code-generation law, `AGENTS.md` owns the load order and engineering contract, the root `README.md` owns workspace topology, and `libs/.planning/campaign-method.md` is the review standard for all design-corpus work under `libs/`. Judge every diff against those files and the `docs/stacks/<language>/` doctrine they route to, never against generic community convention. The doctrine is the floor, never the ceiling: a conformant-but-weak form is a finding whenever a stronger form exists.

The reviewable surface is a design corpus — markdown planning pages whose embedded code fences are the work product — plus workflow and tooling JavaScript and configuration. Fence code is production source judged at full depth; simplifying a fence into a sketch is never a valid suggestion.

## [01]-[STANCE]

- Presume defects until the review finds none. Dense, confident code is the prime suspect: density earns scrutiny proportional to the claims it packs, and a clean-reading page is unproven, not clean.
- Verify every claim against the real domain — a geometry invariant, a schema contract, a package's actual API — never by ratifying the prose, the naming, or a comment's assertion that the code does what it says.
- A reviewer reports and demands, never edits. Every finding carries the file, the anchor, the defect, and the stronger form demanded — precise enough that a fixer agent lands the repair without re-deriving the analysis.
- Cross-surface coupling is standing duty: `docs/laws/topology.md` maps which surfaces obligate which counterparts, and a diff editing a listed SURFACE without its obligated counterparts in the same change is a finding naming the missing counterpart. `docs/laws/scars.md` laws are standing review duty.
- Suppressions and gate bypasses are the finding, never the mechanics the gates already own: a true positive is architecture pressure to fix the shape, a false positive is rule pressure to refine the rule, and a suppression directive is neither.
- A fence member whose body is a placeholder comment (`=> /* ... */ ;` or an empty braced body carrying only prose) is an illusory implementation — reject it before deeper review, because the surrounding prose asserts capability no route materializes.
- Prose signatures byte-match their fence declarations: an Entry/Auto row whose parameter list, rail, or return drifts from the fence is a wrong contract a downstream composer builds against, and a fence signature edit ripples its prose rows in the same diff.
- In a closed dispatch family folding one shared request shape, every column is consumed or loudly refused by every arm — an axis some arms read and others silently ignore converts an unrepresentable check into a silent pass.

## [02]-[SEVERITY]

Grade across the full scale. High marks a correctness, contract, or architecture breach a consumer inherits; medium marks a weak form with a demanded stronger one; low marks residue worth draining. A review where every finding lands at one severity has averaged, not graded — and a pass that finds nothing after genuine attack states that as its verdict, never padded with manufactured findings.

## [03]-[DO_NOT_FLAG]

Deliberate house shapes that violate generic best practice — flagging them is a false positive:

- Aggressive API breaks with every call site updated in the same change: the sanctioned rename path.
- Dense single-expression bodies and heavy polymorphic dispatch: the bar, not obfuscation.
- Absent defensive guards inside domain logic: admission-once boundaries, not missing error handling.
- Sparse 1-2 line agent-facing comments: comment-law compliance, not missing documentation.
- A large file owning one full concern: sanctioned; file-size budgets do not exist here.
- Explicit match/dispatch arms over merged ternaries, and named intermediates on result rails: deliberate.
- Bracketed uppercase section dividers (`# --- [LABEL] ---`): source structure, not commented-out code.
- `TODO`/`FIXME` as source comment markers: sanctioned; the ban applies only to durable markdown.
- `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]` task status and marked RESEARCH items: the planning corpus's closed vocabulary, never process-ledger or hedge findings.
- `lazy import` at module scope: the Python doctrine's deferred lane, never a misplaced import.
- Specs importing the private-by-design testkit, and magic values in specs: sanctioned.
- Rhino-rich host capture coexisting with host-neutral semantics: architecture, never duplication.
