# [HARVEST_DISPATCH]

Per-round prompt for dispatching the reviewer-harvest agent: the agent's profile carries every standing law and the skill owns the process, so this prompt restates neither — it carries only what no standing surface can know. Fill every slot; a slot the round genuinely lacks is omitted whole, never left as a placeholder.

```markdown template
Round <N> distill. Feed: <round-dir>/harvest-feed.md. Lane reports: <round-dir>/lane-*-report.json.
Recurrence census: prior-round reports at <prior-round-dir>/lane-*-report.json [, <prior-round-dir>/...].
Reviewed surface: <corpus kind this round distilled — code | estate (prose, skills, agent defs) | mixed; selects the owner set and classification vocabulary>.
Estate propagation: <sensitive when a touched .claude/ surface byte-copies estate-wide, or a CLAUDE.md fact owes the global ~/.claude/ manifest rather than the project file; omitted for repo-local work>.
Owner set: <the estate owners this round may reach beyond the reviewer configs — the round's live subset of the agent's dispatch-named owner set>.
Round priorities: <classes, feed sections, or measurements this round elevates — a verdict-mix honesty sample, a user-directed analysis>.
Landing concurrently: <files closer or fixer agents are editing in parallel this round>.
User directives: <round-scoped instructions>.
```
