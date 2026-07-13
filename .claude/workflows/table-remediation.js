export const meta = {
  name: 'table-remediation',
  description: 'Repair every Markdown table defect across damaged docs with the derived in-place relief playbook, gate-verified per file',
  phases: [
    { title: 'Discover', detail: 'one agent runs the gate and returns every file carrying a table defect' },
    { title: 'Repair', detail: 'one agent per 6-file batch; relieve tables in place, verify each file table-clean' },
  ],
}

// --- [CONSTANTS] ------------------------------------------------------------------------

// args optionally pins the file set (array of absolute paths, or {files}); absent, a discovery agent computes it fresh from the gate.
const PROVIDED = Array.isArray(args) ? args : Array.isArray(args?.files) ? args.files : null
const BATCH = 6

const DISCOVER = `Compute the fresh set of Markdown files carrying a table defect. Run, from the docgen skill dir:
cd /Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/docgen && uv run scripts/prose_gate.py --json $(fd -e md . /Users/bardiasamiee/Documents/99.Github/Rasm) 2>/dev/null | jq -rs '[.[] | select(.check | startswith("table")) | .file] | unique | sort_by((. | test("\\\\.api/") | not), .) | .[]'
Each output line is one absolute path (".api" catalogs sorted first, then planning specs). Return every path as {files:[...]}, verbatim and complete — do not sample, truncate, or edit the list.`
const DISCOVER_SCHEMA = { type: 'object', additionalProperties: false, required: ['files'], properties: { files: { type: 'array', items: { type: 'string' } } } }

const PLAYBOOK = `Repair every Markdown table defect in the files listed below. These are Rasm durable docs — mostly ".api" capability catalogs (a symbol/entrypoint table plus signature fences) and planning specs. The defect is cells cramming full signatures and multi-clause behavior, blowing the 150-column rendered table-width cap. RELIEVE TABLES IN PLACE. Never tear a table down to mega-prose or a bare unlabeled list, and never drop a verified member, signature, or law.

FIRST, invoke the docgen skill (Skill tool, skill "docgen") to load the durable-prose register; consult its references/structure.md [03] and examples/tables.md only if a table's shape is unclear after the playbook below. The playbook is the distilled nuance — follow it directly; do not re-derive from the full standards.

GOVERNING PRINCIPLE — you are RELIEVING width, not adding content. A correct fix holds the file roughly flat or SHRINKS it; the ONLY content it may legitimately add is one genuine cohesive-type signature fence (a real multi-field DTO or options struct). Any other growth means you duplicated or fence-dumped instead of relieving, and it is wrong. The verified members, enum values, defaults, and signatures already in the cells are the payload — keep them SCANNABLE in the grid; never vaporize them into a vague phrase and never bury them in a fence.

RELIEF MOVES — try in this order; reach for a later move only when the earlier ones cannot fix the width:
1. Tighten the cell, keep its specifics. Compress phrasing, not facts: drop filler words, keep the load-bearing tokens as code spans. An enum's values ("ORT_ENABLE_ALL=99"), a struct's key fields, a method's name — these ARE the capability; a cell trimmed to "graph optimization level" that merely restates [SYMBOL]+[ROLE] is a DEAD cell and a defect you introduced.
2. Hoist to the header or the lead. A word every cell repeats moves into the header once. A cross-row invariant, or a rule the whole table obeys, moves to the lead sentence before the table (the prose around a table is a relief surface). When several overloads or rows share one signature tail, state the shared form ONCE in the lead ("every Write overload takes (target, uint minLinealExtent, ..., string idAttributeName = \\"id\\")") and let the table carry only what differs — this is LEANER than a fence and is preferred over one whenever the signatures rhyme.
3. Split an overloaded row into multiple atomic rows. When one cell crams several distinct members, overloads, or values, give each its own row (one member per row stays greppable); renumber [INDEX] sequentially. This is usually BETTER than a fence.
4. Split a heterogeneous table by concern. A grab-bag table (enums + handles + structs + options in one grid) becomes right-sized sub-tables — an enums sub-table with a compact values column, a handles sub-table, an options sub-table — each column sized to its own rows.
5. Lean on the prose that already exists. The crammed behavior is often already stated in the file's own [IMPLEMENTATION_LAW]/[STACKING]/[RAIL_LAW]/lead — then the cell was a duplicate and simply shrinks to an identity phrase. A genuine per-row clause that nothing else carries relieves to a keyed indexed list below the table ("- [NN]-[LABEL]: prose." matching the row), never mega-prose.
6. Structural fixes: a universal column (every cell identical, usually already in the scope block) is deleted; a "·"/"/"/conjunction multi-value cell fans to one row per value; a sub-lettered [1a]/[1b] index renumbers to sequential [02]/[03] with any parent relation in the lead or an owner column.

SIGNATURE FENCE — LAST RESORT, NARROW, RARELY THE ANSWER:
- Earned ONLY by a single cohesive type whose many real fields/methods an implementer copies verbatim (e.g. one options struct with 8 fields), AND only when moves 1-5 cannot carry it. Most .api tables need ZERO new fences.
- FORBIDDEN fence forms (a prior pass did all of these and bloated four files 20-75%): dumping a whole [CALL_SHAPE]/[SIGNATURE] column into a fence; a fence of many distinct unrelated symbols; empty-body declarations ("enum X { }") with the description in a trailing comment; a fence that restates what a cell already says. If you are adding more than one fence to a file, or a fence longer than the table it relieves, stop — you are bloating, not relieving.
- The label MUST be "signature" (with the language, e.g. csharp signature); "contract" is not a sanctioned intent and fails the gate — correct any existing "<lang> contract" fence you touch to "<lang> signature". This label fix is in scope even where you add no fence.

ANTI-PATTERNS (forbidden — these are exactly what the reverted bad pass did):
- Gutting a specific cell into a phrase that just restates its [SYMBOL] or [ROLE].
- Converting a signature/call-shape column into a giant fence.
- Growing the file by any means other than one genuine cohesive-type fence — fence-dumps, gutted-then-redumped cells, and duplicated content are all bloat.
- Tearing a table down to mega-prose or an unkeyed list.

HARD RULES:
- Preserve ALL information — every verified member, enum value, default, and signature, kept scannable. Dropping or vaporizing one is a failure.
- Every table keeps its leading centered [INDEX] column and bracketed UPPERCASE rubric headers; keep every name/signature/type spelled EXACTLY as written (relieve layout, never re-author the API; keep an ambiguous signature verbatim).
- SCOPE: fix only table defects (table-width, table-cell, table-index, table-prose, table-links) plus the coupled "contract"->"signature" fence-intent label. Do NOT touch unrelated findings (hard-wrapped intros, sibling pointers, H1 prose tails, hedges, version anchors).

THE TWO TESTS before you mark a file done: (A) CELL TEST — every capability/description cell carries information beyond its [SYMBOL] and [ROLE]; a cell you could delete without losing meaning is a dead restatement, and gutting a specific cell to produce one is the worst failure. (B) BLOAT TEST — the discriminator is the QUALITY of any growth, not a raw number. A relief shrinks the file when the crammed cells were duplicating prose the file already states; it grows only by the lines of a genuine cohesive-type signature fence (a real multi-field DTO or options struct whose fields ARE the type's API). Treat noticeable growth as a prompt to check WHY: if the added lines are a real type shape an implementer copies verbatim, that is earned; if they are gutted cells re-dumped as a fenced signature column, empty-body declarations, or many unrelated symbols, you bloated — lean it by hoisting shared signatures into the lead and keeping enum/value lists as compact code-span cells. If either test fails, the relief was naive — redo it.

REFERENCE EXEMPLARS (already fixed to the target form — read one if a table's shape is unclear):
- /Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm/.planning/Parametric/nurbs.md ([03]-[DENSITY_BAR]: the leanest pattern — sub-lettered index renumbered, two fat prose columns relieved to a keyed indexed list, NO fence, ~flat line count).
- /Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Bim/.api/api-nts-vectortiles-mapbox.md (atomic capability cells; a universal [RAIL] column dropped; shared Write/Read signatures HOISTED into the scope card; ONE earned signature fence for the genuine multi-field TileJSON DTOs — the only content added).
- /Users/bardiasamiee/Documents/99.Github/Rasm/tests/python/.api/mutmut.md (multi-value "·"-joined cells fanned to rows; a closed status vocabulary relieved into one compact fence tuple).

TOOLS (run from the docgen skill dir):
- Pad/normalize after editing: cd /Users/bardiasamiee/Documents/99.Github/Rasm/.claude/skills/docgen && uv run scripts/prose_gate.py fix --write <ABS_FILE>
- Check findings: uv run scripts/prose_gate.py --json <ABS_FILE>  (each line is one finding: {check,status,line,detail}; a file with no line whose check starts with "table" and no "fence-intent" fail is done)

FINALIZATION (mandatory, per file, in this order): (1) make your content edits; (2) run "fix --write <ABS_FILE>" as the LAST edit so every table is re-padded to its canonical EXPANDED column-aligned form — never leave a table in tight, collapsed, hand-narrowed, or uneven-padding state; the committed file must show fully expanded grids; (3) run "--json <ABS_FILE>" and confirm zero findings whose check starts with "table" and zero "fence-intent" failures. If the gate still flags the file, edit again and repeat (2) and (3). A file is done only after step (2) has run and step (3) is clean.

ACCEPTANCE per file: the final on-disk state has every table canonically expanded by "fix --write", "uv run scripts/prose_gate.py --json <ABS_FILE>" shows ZERO findings whose check starts with "table" and zero "fence-intent" failures, and every other pre-existing finding is untouched.`

const RETURN = `RETURN as structured output: for each file, {path, tableBefore, tableAfter, fenceIntentFixed, converted, note}. tableBefore/tableAfter are the counts of table-* findings; converted is true only if you promoted a table to records/list (name why in note); note is two-to-four words on the moves applied. Report honestly — if a file still has a residual table finding, set tableAfter to the real count and say why in note.`

const FILE_SCHEMA = {
  type: 'object',
  additionalProperties: false,
  required: ['path', 'tableBefore', 'tableAfter', 'note'],
  properties: {
    path: { type: 'string' },
    tableBefore: { type: 'integer' },
    tableAfter: { type: 'integer' },
    fenceIntentFixed: { type: 'integer' },
    converted: { type: 'boolean' },
    note: { type: 'string' },
  },
}
const BATCH_SCHEMA = {
  type: 'object',
  additionalProperties: false,
  required: ['files'],
  properties: { files: { type: 'array', items: FILE_SCHEMA } },
}

// --- [COMPOSITION] ----------------------------------------------------------------------

phase('Discover')
let FILES = PROVIDED
if (!FILES) {
  const found = await agent(DISCOVER, { label: 'discover', phase: 'Discover', schema: DISCOVER_SCHEMA })
  FILES = found?.files ?? []
}
log(`${FILES.length} damaged files to repair`)

phase('Repair')
const batches = []
for (let i = 0; i < FILES.length; i += BATCH) batches.push(FILES.slice(i, i + BATCH))
log(`${FILES.length} damaged files in ${batches.length} batches of up to ${BATCH}`)

const results = await parallel(
  batches.map((batch, index) => () =>
    agent(`${PLAYBOOK}\n\nFILES TO FIX (batch ${index + 1} of ${batches.length}):\n${batch.map((f) => `- ${f}`).join('\n')}\n\n${RETURN}`, {
      label: `batch-${index + 1}`,
      phase: 'Repair',
      schema: BATCH_SCHEMA,
    }),
  ),
)

const reports = results.filter(Boolean).flatMap((r) => r.files ?? [])
const residual = reports.filter((f) => f.tableAfter > 0)
const converted = reports.filter((f) => f.converted)
log(`reported ${reports.length}/${FILES.length} files; ${residual.length} with residual table findings; ${converted.length} converted`)
return { files: FILES.length, batches: batches.length, reported: reports.length, residual, converted, reports }
