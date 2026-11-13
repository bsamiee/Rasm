---
name: crawl-github
description: "Use when finding, comparing, grading, or collecting files from public GitHub repositories on any topic, covering discovery, ecosystem baseline, judging, tree mapping, selective fetch, and delegation to scout, take, and audit agents"
argument-hint: "[request | scout <brief folder> <slice> | take <brief folder> <batch> | audit <brief folder> <batch>]"
allowed-tools: Bash(${CLAUDE_SKILL_DIR}/scripts/discover.sh *) Bash(${CLAUDE_SKILL_DIR}/scripts/fetch.sh *)
---

# [CRAWL_GITHUB]

Agents find repositories by measured facts, grade each on one stated scale from files read, rank all of them across slices before any fetch, and collect chosen paths at one commit. Requests name a subject and kinds of file, each repository and file name comes from command output.

[SCRIPTS]:
- [01]-[DISCOVER](scripts/discover.sh): Repository facts from a search, a code search, or named repositories, their baseline, and a ranked slate
- [02]-[FETCH](scripts/fetch.sh): Tree, survey, chosen paths, and remainder of one repository at one commit

Arguments choose a role:

| [INDEX] | [ARGUMENTS]                    | [ROLE]      | [WORK]                                                             |
| :-----: | :----------------------------- | :---------- | :----------------------------------------------------------------- |
|  [01]   | Request text                   | Lead        | `[01]`, baseline of `[02]`, `[06]`, and `[08]`                     |
|  [02]   | `scout <brief folder> <slice>` | Scout       | `[02]` and `[03]` for one slice, a candidates file, no fetch       |
|  [03]   | `take <brief folder> <batch>`  | Take agent  | `[03]` to `[05]` for rows of its `ASSIGNED.tsv`, outputs of `[06]` |
|  [04]   | `audit <brief folder> <batch>` | Audit agent | `[07]` over one batch                                              |

Scouts, take agents, and audit agents read `REQUEST.md` and `BRIEF.md` of their brief folder whole before any command.

## [01]-[SCOPE]

Leads fix each value before any agent starts and report every one a request left open:
- Subject: every kind of repository the request's words admit, each kind in a slice, a lead narrows none
- Gates: tests the request states as required, each decided by a discovery row or a file read
- Preferences: every other test (a request's "ideally", each filter a lead adds), ranking candidates and removing none
- Criteria: what the request wants learned from a repository, each named with the evidence showing it (a file, a recorded run, source read)
- Scale: one meaning per grade for every agent, with the request's bar and floor, else 8.0 and 5.0
- Files: kinds the request names, by role, names come from each tree
- Bounds: byte cap per file, data kept out (corpora, media, generated output)
- Sources: source files read whole per repository for a criterion about code, 5 unless the request sets one
- Counts: repositories per slice as a cap, a slice with fewer at its bar stays short
- Pool: candidates with files opened per repository kept, 3 unless the request sets one
- Layout: `<out>/<slice>/<owner>-<repo>/<repository path>`, `<out>` a folder the request names, else wherever the harness keeps session working files
- Dates: today and each window start as `YYYY-MM-DD`
- Write-up: fields per repository as the request words them

Scale, unless the request sets one:

| [INDEX] | [GRADE]    | [MEANING]                                                               |
| :-----: | :--------- | :---------------------------------------------------------------------- |
|  [01]   | 9.0 to 10  | Every criterion met as well as its slice's best, each with its evidence |
|  [02]   | 8.0 to 8.9 | Every criterion met, one unproven or short of that best                 |
|  [03]   | 7.0 to 7.9 | Most criteria met, one absent                                           |
|  [04]   | 5.0 to 6.9 | Few criteria shown well, others absent or unproven                      |
|  [05]   | Under 5.0  | Nothing the request wants learned                                       |

- Grades place a repository by criteria met, a listed weakness outside every criterion moves no grade
- Criteria no repository of a slice's baseline meets and its ecosystem offers no means for are `n/a` there and leave its grades
- Use `search-web` for what a slice's ecosystem offers for a criterion, before that criterion counts against its repositories

`<work>` folders hold one repository's tree and fetched objects each, under a scratch folder named for one agent's role and slice or batch, outside every project tree: build tools read manifests they find, and agents sharing a folder read each other's trees.

## [02]-[DISCOVERY]

Rows are JSON lines with `repo`, `stars`, `forks`, `kb`, `created`, `pushed`, `branch`, `commit`, `committed`, `commits_since`, `ci`, `merged_prs`, `open_issues`, `releases`, `release`, `released`, `license`, `archived`, `fork_of`, `langs`, `topics`, `submodules`, `root`, `paths`, `homepage`, and `description`:
- `langs` is percent of bytes per language, `root` every root entry with `/` after a folder
- `committed` is the default branch head's date and `commits_since` its commits since `<since>`, `pushed` moves on a push to any branch
- Activity tests read `commits_since`, a `pushed:` qualifier alone passes repositories with no default-branch commit
- `paths` answers each `<probe>` (a repository path): a folder's entries, a file's bytes, or `null` for no entry at that exact path
- `ci` is the head commit's check state, `fork_of` the parent of a fork
- Rows go to a file, `jq` prints the columns a decision reads

```bash
# Repository search: <max> rows in query order, matches=<total> on stderr
"${CLAUDE_SKILL_DIR}/scripts/discover.sh" search '<qualifiers> sort:<stars-desc|updated-desc>' <since> <max> [<probe>...]

# File content search: rows of repository and path
"${CLAUDE_SKILL_DIR}/scripts/discover.sh" code '<literal> filename:<name>' <max>

# Rows for named repositories: lines opening with <owner>/<repo>, or any text holding github.com links
"${CLAUDE_SKILL_DIR}/scripts/discover.sh" repos <since> [<probe>...] <'<names file>'
"${CLAUDE_SKILL_DIR}/scripts/discover.sh" code '<query>' <max> | "${CLAUDE_SKILL_DIR}/scripts/discover.sh" repos <since> [<probe>...]
gh api 'repos/<owner>/<repo>/readme' -H 'Accept: application/vnd.github.raw+json' | "${CLAUDE_SKILL_DIR}/scripts/discover.sh" repos <since>

# Share of rows holding each root entry, probe, release, and head-commit check
"${CLAUDE_SKILL_DIR}/scripts/discover.sh" baseline <'<rows>.jsonl'

# Rank kept rows
jq -r 'select(<test over row fields>) | [.repo, .stars, .commits_since, .merged_prs, (.root | join(" "))] | @tsv' '<rows>.jsonl'
```

- Search qualifiers: `language:`, `topic:`, `stars:>N`, `pushed:>YYYY-MM-DD`, `created:`, `size:>KB`, `archived:false`, `fork:false`, `org:`, `in:name,description,readme <words>`
- Search serves 1000 repositories per query, `matches` above 1000 needs a split by star range, topic, or creation date
- `matches` above `<max>` and under 1000 needs a larger `<max>`
- Code search qualifiers: `filename:`, `extension:`, `path:`, `language:`, `repo:`, `org:`, beside at least one literal
- Code search reads default branches and files under 384 KB and serves 1000 rows per query
- Code search can miss archived repositories, forks, and repositories inactive for a year
- Code search allows 10 requests a minute across one account, `code` waits for each reset
- `repos` follows a renamed repository to its current name and prints each unresolved name on stderr
- `repos` and `search` fetch facts for 10 repositories per request in sequence, hundreds of names run for minutes in the background
- Rate: 1 GraphQL point per 100 searched names and 1 per 10 repositories, `gh api rate_limit --jq .resources` prints what remains

Baselines state what a slice's ecosystem holds before anything is graded against it:
- Leads run `search` over each slice's population with the criteria's paths as probes, then `baseline`, and put each slice's shares in its brief
- Shares near zero across a population mark what that ecosystem lacks, a repository is graded against what its slice shows

Candidates start from a slice's whole population and widen from there, each source ending in `repos`:
- Search by topic and language, ranked by `stars` and `commits_since`, then by `topics` and owners (`org:`) of its best rows
- Code search for a file name or literal only the subject's tools write
- Curated lists and comparison pages, their links piped to `repos`
- Use `search-web` for what practitioners name and why, a web page ranks nothing until `repos` returns its row

## [03]-[JUDGING]

Rows rank candidates, files decide. Read a candidate's deciding files before grading it:

```bash
# One file as raw text, a folder path returns its JSON listing
gh api 'repos/<owner>/<repo>/contents/<path>' -H 'Accept: application/vnd.github.raw+json'

# Recorded runs by trigger, for a criterion a run proves: push runs of a branch, tag and release runs under their own events
gh api 'repos/<owner>/<repo>/actions/runs?event=<push|release|schedule>&per_page=30' --jq '.workflow_runs[] | [.name, .head_branch, .conclusion, .created_at[:10]] | @tsv'
```

```text
ToolSearch {"query": "select:mcp__deepwiki__read_wiki_structure,mcp__deepwiki__ask_wiki_question", "max_results": 2}
mcp__deepwiki__read_wiki_structure {"repoName": "<owner>/<repo>"}
```

- Grades take each criterion in turn: met, part, absent, or `n/a`, with its evidence, then one grade from the brief's scale
- Criteria about code are graded from source files read whole, never from configuration around them
- Source files come from different packages, largest first by `survey` bytes, up to the brief's count
- Judged files are files opened whole, a row, a listing, or a wiki answer is no reading
- DeepWiki pages are generated, they map a repository and source no claim
- Use `search-code` for what an unfamiliar tool's option does, a grade over a setting needs its documented meaning
- Candidates failing one gate while grading at the bar are near misses, written to the candidates file with that gate named
- Repositories below the bar that alone show a mechanism the request names are exhibits of it, reported outside every count
- Pools short of the brief's count per kept repository widen through another source before a scout returns

Scouts write a candidates file, `<out>/_scout/<slice>.tsv`, one line per candidate with files opened, kept or not:

```text
<owner>/<repo><TAB><grade><TAB><failed gate, or -><TAB><slice><TAB><one-clause reason>
```

## [04]-[MAPPING]

```bash
# Tree at the default branch head, or at a branch, tag, or full commit
"${CLAUDE_SKILL_DIR}/scripts/fetch.sh" map <owner>/<repo> <work> [<ref>]

# What the tree holds, whole or under one folder
"${CLAUDE_SKILL_DIR}/scripts/fetch.sh" survey <work> [<folder>]

# Paths of files matching a pattern, under a byte cap when one is given
"${CLAUDE_SKILL_DIR}/scripts/fetch.sh" pick <work> '<pattern>' [<bytes>]
```

- `map` writes `<work>/tree.tsv` (type, bytes, path) and `<work>/COMMIT`, types are `blob`, `link`, `tree`, and `commit`
- `survey` prints a folder's files, its folders with file counts and bytes, extensions, names that recur or have no extension, submodules, and largest files
- Unexpected file names show in `survey` before any pattern is written, a pattern written first finds only what it names
- `commit` rows are submodules with their pinned commit in a fourth column, `.gitmodules` names each one's repository
- Submodules holding wanted files are mapped as their own repository at that commit
- `link` rows are symbolic links, a taken link needs its target path taken too
- Folders can hold tens of thousands of data files, extension counts under a folder decide whether it is taken whole

## [05]-[TAKING]

Path lists hold one repository path per line, a file or a folder, a folder brings every file under it:

```bash
# Picked paths appended to a list
"${CLAUDE_SKILL_DIR}/scripts/fetch.sh" pick <work> '<pattern>' <bytes> >>'<paths file>'

# Listed files in one request, each at its repository path under <dest>, with <dest>/COMMIT
"${CLAUDE_SKILL_DIR}/scripts/fetch.sh" take <work> <dest> '<paths file>'

# Tree paths matching a pattern that <dest> lacks
"${CLAUDE_SKILL_DIR}/scripts/fetch.sh" left <work> <dest> ['<pattern>']
```

- `take` prints `files=<n> links=<n>`, a line matching no file or naming a submodule fails the call before any fetch
- Patterns of `pick` and `left` are `rg` regular expressions over repository paths
- Folder lines bring every file whatever its size or kind, a byte cap holds only for lines `pick` wrote
- `left` runs once per wanted kind after `take`, one repository at a time, its output is what a list missed
- Test files follow their ecosystem's convention (folder, prefix, suffix, or beside sources), `survey` shows which one a tree uses
- Git LFS files arrive as pointer text
- Wanted files a repository calls from another repository (shared workflows, build modules) go beside it in a folder of their own, outside every count
- Each batch keeps one script that maps every repository at its taken commit and takes its path list, an audit agent reruns it

## [06]-[DELEGATION]

Leads write the request and the brief as files before any spawn:
- `<out>/_brief/REQUEST.md`: every user message, verbatim and whole
- `<out>/_brief/BRIEF.md`: values of `[01]` with the scale, each slice with its baseline shares, quota shared by all agents, and each role's return

Work runs in stages, each stage's agents spawned in one message:
1. Scouts, one per slice and at least 2, each return a candidates file
2. Leads rank every scout's candidates at once and write the slate to `<out>/_brief/SLATE.tsv`
3. Leads assign `keep` rows in rank order up to each slice's cap, as slate rows in `<out>/_take/<batch>/ASSIGNED.tsv`
4. Take agents, one per batch, map, read, take, check, and write up their rows
5. Audit agents, one fresh agent per batch, follow `[07]`

```bash
# One row per repository, best grade first: rank, keep|near|below, grade, repository, slice, failed gate, reason
"${CLAUDE_SKILL_DIR}/scripts/discover.sh" slate <bar> '<out>'/_scout/*.tsv >'<out>/_brief/SLATE.tsv'
```

```text
Skill {"skill": "crawl-github", "args": "scout <out>/_brief <slice>"}
Skill {"skill": "crawl-github", "args": "take <out>/_brief <batch>"}
Skill {"skill": "crawl-github", "args": "audit <out>/_brief <batch>"}
```

Brief and spawn rules:
- Agents read the request itself, a lead's summary of it loses requirements
- Briefs hold values, paths, and measured shares alone: no candidate repository, no expected file name, no ranking
- Spawn prompts hold one line of purpose and the skill call, nothing of what a lead expects back
- Slices differ by kind of repository the request admits, a split by star range or page gives agents one ranking cut in pieces
- Scout count is the request's agent count, else one per slice
- Batches hold 2 repositories at most, a take agent reads each repository's files and sources whole in one context
- Spawns state a model when the request names one
- Scouts, take agents, and audit agents spawn no agents
- `near` rows and exhibits go to the user in the report, a lead takes one only with its failed gate stated in the write-up

Take agents leave, per batch:
- `<out>/<slice>/<owner>-<repo>/`: taken files at repository paths
- `<out>/_take/<batch>/take.sh` and `paths/<owner>-<repo>.txt`: what brought them
- `<out>/_take/<batch>/REPOS.md`: one write-up per repository in the request's fields

Write-ups add rows to the request's fields:
- `criteria`: each criterion as met, part, absent, or `n/a`, with the path or run proving it
- `read`: every file read whole, sources included

Returns name each repository with its grade, files opened per repository, and what stayed unsettled. A take agent whose reading puts a repository below the bar says so and returns it short.

## [07]-[AUDIT]

One fresh audit agent follows each take agent, spawned with nothing of that agent's return. Per repository of a batch:
1. Run the batch's `take.sh`, map each repository again, and read its deciding files and sources
2. Grade each criterion on the brief's scale
3. Run `survey` and `left` per wanted kind, read what `left` prints, and take each file the request wants
4. Delete each taken file outside the request's kinds
5. Apply the request's thresholds: a repository below its floor goes, one below its bar gets a replacement
6. Replacements are the slate's first `keep` row of that slice no `ASSIGNED.tsv` lists, appended to the batch's own before mapping
7. Write each replacement up in full, with its `criteria` and `read` rows
8. Add a grade, its `criteria` row, and reason under the take agent's in `REPOS.md`, strike through a removed repository's entry in place

Grades come from files read in a repository, a take agent's write-up is a claim to test. Returns name each repository with both grades, files added and deleted, replacements, and removals.

## [08]-[REPORT]

Leads stay in their turn until each stage's completion notices arrive, then read every slice folder themselves before any synthesis:
- Notices of agents whose spawner ended its turn go to the session above it, a lead that stops waiting loses them
- `left` over one repository per batch confirms what a return claims
- Grades of one repository more than a band apart are settled by reading the evidence both `criteria` rows cite
- Reports state each scope value a lead chose, the scale, and each slice's baseline
- Reports list repositories with both grades, short slices, near misses, and exhibits
- Unsettled items carry the command that settles each
