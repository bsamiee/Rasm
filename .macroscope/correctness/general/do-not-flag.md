# [DO_NOT_FLAG]

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
