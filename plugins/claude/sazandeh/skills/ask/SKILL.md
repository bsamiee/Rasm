---
name: ask
description: "Use before asking the owner anything: a choice, permission for an action, review of content, a diff or commits, file changes in a git tree, or memory approval, covering specs, host layouts, previews, answers, and the commands approving answers run"
allowed-tools: Bash(uv run --locked --script ${CLAUDE_SKILL_DIR}/scripts/ask.py *)
---

# [SAZANDEH_ASK]

`scripts/ask.py` builds every `AskUserQuestion` input from a spec: option order, recommended marks, previews, cuts, pages, batches, row style, the form each host draws, and the command each approving answer runs.

[SCRIPTS]:
- [01]-[ASK](scripts/ask.py): Turns a YAML or JSON spec into one `AskUserQuestion` input per stdout line, with report lines on stderr

## [01]-[LAYERS]

Questions pass through these layers, and each layer decides what the next one receives:

| [INDEX] | [LAYER] | [HOLDS]                                                                                                    |
| :-----: | :------ | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | Spec    | What the agent knows: question texts naming subject and deciding fact, options, facts, content, conditions |
|  [02]   | Script  | Every input of the call, derived from the spec and the host: forms, previews, cuts, pages, runs commands   |
|  [03]   | Dialog  | Host drawing up to 4 questions as tabs, 2 to 4 options each, then returning one result text                |

Hosts draw the dialog in one layout each, which `CLAUDE_CODE_ENTRYPOINT` names and the script reads itself:

| [INDEX] | [LAYOUT] | [HOSTS]                                   | [DRAWS]                                                                   |
| :-----: | :------- | :---------------------------------------- | :------------------------------------------------------------------------ |
|  [01]   | Terminal | `cli`                                     | Labels beside a preview box and notes on `n`, else host `Type something.` |
|  [02]   | Webview  | `claude-vscode`                           | Labels beside a scrolling preview pane, host `Other` row with a text box  |
|  [03]   | Plain    | Desktop, SDK, and remote hosts, `--plain` | Labels and descriptions, content inside the question text, host typed row |

Terminal facts the script measures and states:
- Box: terminal columns minus 34 wide, previews wrap at columns minus 38, and height leaves the dialog's chrome, question, hint, and notes rows
- Questions over 80 columns or holding a newline draw with a 2-column gutter, which the height budget counts
- Cut bar: lines past the box height wait behind the host's `✂ N lines hidden` bar, and enlarging the pane draws them
- Previews: Markdown with `ini`, `diff`, and `text` fences highlighted, no background colors, no Mermaid or HTML, at most 2000 units
- Notes: typed on the focused row, returned after its label, the one free answer a preview question offers
- Rows: a question sending no preview, every multi-select included, draws descriptions under labels and the host's `Type something.` row
- Flow: a lone single-select question submits on Enter, and a call of more questions takes Tab between tabs and ends on a review screen
- Chat: every dialog draws `Chat about this`, which refuses the call with a clarify note

## [02]-[STEPS]

Each ask runs the same steps, from one question to a batch of gated calls:
1. Find: a fact a read, run, search, or agent supplies is found, never asked, and plan approval goes through `ExitPlanMode`
2. Choose: each open question takes the form of `[03]` naming what the owner decides
3. Write: one spec holds every question open now in asking order, each asked when it first arises, before work builds on a guess
4. Run: the command of `[06]`, then each stderr row read by its table before any call
5. Send: each stdout line as one `AskUserQuestion` call unchanged, a gated call only after the answer its `call` row names
6. Read: each result by `[07]`, one line stating each pick before acting, then each `runs` row of an approving answer
7. Follow: a typed answer or note raising a new question joins one new spec, and an answer changing a body reruns the spec

## [03]-[SPEC]

Specs are YAML sequences of asks, each opened by the key naming its form, its value the question text naming the subject and deciding fact, as chat text after a call can stay hidden while the dialog is open:

| [INDEX] | [KEY]     | [ASKS]                                       | [FIELDS]                                                           |
| :-----: | :-------- | :------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | `decide`  | One exclusive choice among 2 or more         | `header`, `recommended`, `options`, `show`, `when`                 |
|  [02]   | `select`  | Choices that combine                         | `header`, `options`, `when`                                        |
|  [03]   | `consent` | Permission for one named action              | `header`, `recommended`, `action`, `safe`, `facts`, `show`, `when` |
|  [04]   | `review`  | Content to accept, a diff, or a commit range | `header`, `show`, `actions`, `recommended`, `when`                 |
|  [05]   | `changes` | Files changed in a git tree or commit range  | `paths`, `range`, `into`, `noun`, `keys`, `group`                  |

Forms and counts follow what the owner decides:
- `decide` holds 2 to 4 options, and 5 or more page behind `More options`
- `select` serves picks that combine, at most 4 checkboxes per question, more splitting into numbered questions
- `consent` serves one action against holding off, `review` content the owner accepts or rejects
- `show` and `facts` go only on content labels and descriptions cannot hold, as a previewed question drops the host's typed row

Fields:
- `header`: at most 12 characters, unique in the spec, as conditions and results name a question by it
- `options`: label to `does`, one line on what picking it leads to, or in `decide` to `{does, facts, show}`
- `facts`: short values every `decide` option holds under the same keys, compared side by side in each preview
- Labels: at most 25 columns and 11 when recommended, no `, `, none of `Other`, `More options`, `Let me pick`
- `recommended`: required on `decide` and `consent`, `null` on a `decide` of the owner's taste, which keeps option order and marks none
- `review` marks its first action recommended unless `recommended` names another
- `action` and `safe`: one-entry mappings of label to `does`, `safe` defaulting to `Not yet: Leaves everything as it is`
- `actions`: label to `does`, by default `Apply edit` and `Leave as is` for `diff`, `Keep` and `Revert` for `git`, `Save` and `Skip` for `file`, else `Accept` and `Reject`
- `when`: `{<header>: <label>}`, the ask sent only after that question returns that label
- YAML: scalars read as their source text, so numbers and dates need no quotes, and values holding `, ` go in block mappings

`show` takes one key naming its kind, on the ask for every option or on one `decide` option:

| [INDEX] | [KIND]    | [HOLDS]                                                        | [DRAWN_AS]                                           |
| :-----: | :-------- | :------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `text`    | Markdown up to 2000 units, mockups in ```` ```text ```` fences | Markdown as written                                  |
|  [02]   | `fields`  | Mapping of short facts                                         | Rows by `[04]`                                       |
|  [03]   | `records` | Mappings, the first field naming each                          | Aligned table when keys match, else one section each |
|  [04]   | `diff`    | `path`, `before`, `after`                                      | Hunks, small prose changes merged into one line      |
|  [05]   | `git`     | `repo`, `range` of commits, `paths`                            | Hunks of the range, `Revert` running its reverse     |
|  [06]   | `file`    | Path on disk                                                   | Front matter rows, then the body                     |
|  [07]   | `steps`   | Names in order                                                 | Boxes joined by arrows, stacked when too wide        |

HTML, images, Mermaid, and emoji refuse, and content over 2000 units goes on an artifact page the question links.

```yaml
- decide: Which date library should @rasm/time wrap? It replaces three hand-rolled parsers.
  header: Library
  recommended: date-fns
  options:
    date-fns:
      does: Pure functions, tree-shakes to the calls used
      facts: {size: 3 kB for the calls used, undo: swap imports}
    Luxon:
      does: Zone-aware objects in a single bundle
      facts: {size: 23 kB min, undo: rewrite every call site}
- consent: Push 3 commits to origin/main? CI runs on push.
  header: Push
  recommended: Push now
  when: {Library: date-fns}
  action: {Push now: Pushes main and starts CI}
```

## [04]-[PREVIEWS]

Previews put deciding lines first, then content, and every row of one call takes one style per key:
- Order: the option's heading, its own `show`, its `facts`, then the content every option shares (consent facts, review content, file body)
- Rows: a key whose every value in the call the host's `ini` highlighter leaves plain draws in an `ini` fence, cyan key and plain value
- Code rows: a key the `ini` grammar refuses, or a value holding `'`, `"`, `;`, `#`, or `on`, `off`, `yes`, `no`, `true`, `false` inside a word, draws as a code span key
- Facts kept short and plain stay in the `ini` fence, and the `code rows` stderr row names the value and token that moved a key
- Cuts: a preview over 2000 units keeps its first pieces (blocks, lines, rows, records, hunks) and states `✂ k of n <unit> cut · full: <source>`
- Height: the script never cuts for height, the host cut bar hides the rest, and the question then names the hidden line count and the route
- Wide: a mockup or table wider than the box wraps, and the `wide` row asks for shorter lines in that mockup alone
- Content: a question sends previews only when an option holds `show` or `facts` or the ask holds shared content, else none
- Pages: 5 or more options show 3 per page and a `More options` row, the last page holding up to 4
- Plain layout: each question's shared content follows its text, option content follows under `<label>:`, and descriptions carry `facts`

## [05]-[CHANGES]

`changes` asks one question per changed file, the diff for an edit and the body for a new or deleted file, with `keys` front matter rows above:
- `paths`: pathspecs, `":!MEMORY.md"` excluding one
- `range`: commits, else the working tree against its upstream or `HEAD`
- `into`: folder approved files copy into, refused beside `range`
- `noun`: word naming each file in questions and headers
- `keys`: front matter keys shown in their order, a mapping key showing every leaf under it, a key no file holds stated on stderr
- `group`: new files joined by `[[name]]` or Markdown links share one question, and a group whose preview needs a cut or hides lines splits with a stated row

Placement decides labels and which answer runs a command:

| [INDEX] | [PLACEMENT]  | [SPEC]  | [LABELS]                                          | [RUNS_ON]                                       |
| :-----: | :----------- | :------ | :------------------------------------------------ | :---------------------------------------------- |
|  [01]   | Copy         | `into`  | `Save`, `Apply edit`, `Delete` against a decline  | Approve: copy, `git apply`, or `rm` in `into`   |
|  [02]   | Working tree | Neither | `Keep` against `Undo`, `Delete` against `Restore` | Decline: file restored from the base or removed |
|  [03]   | Commit range | `range` | `Keep` against `Revert`                           | Decline: reverse diff staged for a commit       |

Linked groups offer `<approve> all`, `Let me pick`, and `<decline> all`, and `Let me pick` opens one checkbox per file, each picked file running its own row.

Memory approval comes before any write to the memory folder:
1. Clone: `git clone -q <memory-dir> <scratch>/memory`, then each new or edited memory file written there
2. Ask: `- {changes: <scratch>/memory, into: <memory-dir>, noun: memory, keys: [description, metadata.type, triggers], group: true, paths: ["*.md", ":!MEMORY.md"]}`
3. Write: the `runs` row of each approving answer and each picked file, general memories first
4. Check: `scripts/index.py <memory-dir> <project-root>` of `dreamweave:memory`, each refusal it reports fixed in the clone and asked again
5. Refusal: a refused `git apply` means another session wrote, so the clone takes `git -C <scratch>/memory pull --rebase` and the ask reruns
6. Close: the clone deleted

## [06]-[RUN]

```bash
uv run --locked --script ${CLAUDE_SKILL_DIR}/scripts/ask.py - <<'YAML'
<spec>
YAML
```

Stderr rows:
- `call <k>: after "<question>" answers "<label>"` -> call `k` runs only when that question returns that exact label
- `<header> "<label>": runs <command>` -> the command that answer runs, once per approving answer or picked file
- `<header>: cut ...` -> the preview states its cut and its source, and the owner reads the rest there
- `<header>: hidden <n> of <m> lines` -> the question names the route, and a pick still counts
- `<header>: wide <n> cells` -> the named mockup is wider than the box in terminal columns and takes shorter lines before the next run, other previews wrap
- `code rows "<key>": ...` -> a shorter or plainer fact value keeps that key in the `ini` fence
- `"<hub>": group split, <reason>` -> each member of that group asks alone
- `skipped`, `no changes`, `keys: ... names no front matter leaf` -> that file, tree, or key gets nothing
- `previews: none, ...` -> the host or flag draws the plain layout
- `ask: refused` with exit 65, 66, or 69 -> each row's field fixed in the spec before one more run, which can report checks the first held back
- Owner's next message saying the dialog took no keys -> the same spec again with `--plain`

## [07]-[ANSWERS]

Results join questions with `, `, each `"<q>"="<label>"`, and a terminal preview question adds ` selected preview:` with the picked preview as sent, so notes start at the first ` notes: ` after that preview's last line:

| [INDEX] | [RESULT]                                                    | [NEXT]                                                                 |
| :-----: | :---------------------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `"<q>"="<label>"`                                           | Act on the label, ` (Recommended)` included in its text                |
|  [02]   | ` notes: <text>` after a label                              | Label stands, notes qualify it and win where they conflict             |
|  [03]   | `(no option selected)` with notes                           | Notes typed unmoved, qualifying the first row unless they name another |
|  [04]   | `More options`, `Let me pick`                               | Call whose `call` row names that label                                 |
|  [05]   | Label the question never offered                            | Text typed in the host's row, which is the answer                      |
|  [06]   | Multi-select `<a>, <b>`                                     | Labels split on `, `, as no label holds one                            |
|  [07]   | `select` tab absent while another tab of its call answered  | No option picked from that tab                                         |
|  [08]   | `The user wants to clarify these questions.`                | Prose answer to what the owner asks, and the rest in a new spec        |
|  [09]   | `To tell you how to proceed, the user said:` and other text | That message followed                                                  |
|  [10]   | `The user doesn't want to proceed` ending in `STOP`         | Stop and wait                                                          |
|  [11]   | `did not answer`, `(No answer provided)`, a question absent | Ungated work goes on, consent does nothing, asked again next           |
|  [12]   | `The user asked for more questions`                         | Follow-up spec before any work                                         |

Clarify arrives inside the `To tell you how to proceed` form, so row `[08]` is read before row `[09]`.
