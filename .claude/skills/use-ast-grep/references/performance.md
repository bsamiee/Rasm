# [PERFORMANCE]

Scan cost adds process startup, rule compilation, file discovery, parsing, injection, candidate matching, relation traversal, and output. Repeated work comes out at its owner at full coverage before parallelism changes.

## [01]-[WORKLOAD]

Commands pay a fixed load before they walk:
- `scan` compiles every configured util and rule before active rule ids select, selection cuts matching alone, injections compile for every command
- Batch independent rules over shared files in one scan to share discovery and parsing
- Custom-language registration loads every configured library, thirteen libraries cost less than one catalog

Parsing dominates matching over a large file, and a relation at a frequent kind is the one rule form that competes with it:
- Rare `kind` with `regex` adds under 1% of a parse, a frequent kind deciding on a field `has` and `regex` about 1%, a frequent leaf with `regex` about 4%
- Frequent leaf with `inside` adds a fifth to a third of a parse whatever its stop, a `regex` clause ahead of the `inside` returns it to about 4%
- Owner kind with `has` to `end` adds about an eighth of a parse, a rule stop on that `has` a twentieth

## [02]-[COMPILE]

YAML node count and per-document regex compiles set load cost, bytes, formatting, and file layout move none:
- Unknown keys pay full deserialization per node
- `any:` over kinds costs a multiple of a comma list of those kinds
- `kind` selector scalars (`a > b`) load below the nested `inside` chain they state
- Presence tests on a field `node-types.json` gives one kind take `kind` in place of `pattern: $_`

Pattern sites cost what parsing their snippet costs, a bare `$X` or literal `L` that triggers parse recovery loads above `{context: <valid code>, selector: <kind>}` and above the 0.41 M of `regex: '^L$'`, per site on an empty file, with the capture and literal form each language takes:

| [INDEX] | [LANGUAGE] | [BARE_M] | [LITERAL_M] | [SELECTOR_M] | [FORMS]           |
| :-----: | :--------- | -------: | ----------: | -----------: | :---------------- |
|  [01]   | C#         |     0.96 |        0.94 |         0.12 | Selector, `regex` |
|  [02]   | Java       |     0.29 |        0.28 |         0.08 | Selector, `regex` |
|  [03]   | SQL        |     0.07 |        0.05 |         0.08 | Bare, `pattern`   |
|  [04]   | Postgres   |     0.06 |        0.05 |         0.09 | Bare, `pattern`   |
|  [05]   | Python     |     0.04 |        0.03 |         0.08 | Bare, `pattern`   |
|  [06]   | TypeScript |     0.04 |        0.02 |         0.08 | Bare, `pattern`   |
|  [07]   | TOML       |     0.04 |        0.02 |         0.07 | Bare, `pattern`   |
|  [08]   | XML        |     0.04 |        0.02 |         0.07 | Bare, `pattern`   |
|  [09]   | Bash       |     0.04 |        0.02 |         0.07 | Bare, `pattern`   |
|  [10]   | Swift      |     0.04 |        0.02 |         0.08 | Bare, `pattern`   |
|  [11]   | Rust       |     0.03 |        0.02 |         0.05 | Bare, `pattern`   |

- Selector forms fit a kind a lone metavariable parses as and stay where a `kind` beside a bare capture costs more walk than its load saving
- Composite and multi-kind sites stay bare unless an `any:` of selectors loads cheaper
- Anchored literals (`regex: '^L$'`) take `pattern: L` with no sibling `kind` where `L` parses clean to a named terminal of the one kind its position holds
- Literal patterns match about 500 fewer instructions per node, anonymous tokens keep `regex` since a pattern compares their kind alone
- Alternations (`^(?:a|b|c)$`) keep `regex`, an `any:` of patterns loads cheaper and matches dearer per node from a third member on
- Capture sites repeated across arms bind once in a `utils` entry the arms read, the site compiling once per document
- Each language and grammar change measures its own sites, a `kind` alone against each form on one empty file

Regexes compile eagerly per document with no sharing across documents or rules, `replace` included. Compile cost follows the engine a pattern selects and not its length, rising in order: unanchored literal or literal alternation, `$`, ASCII class and literal prefix (`^foo`), `\b` under either mode, Unicode `\d` at twice `[0-9]`, Unicode `\s` at six times an ASCII class, Unicode `\w` at ten times `[0-9A-Za-z_]`, `\p{XID_Start}\p{XID_Continue}*`:
- Only an ASCII class saves over `\s`, an explicit `\p{White_Space}` class costs the same
- `.` costs as much as `^\s`
- Always-true arms (an optional capture's last, a lineage end) take `{not: {any: []}}`, `regex: ''` compiling one regex per site
- Gates that only skip work take the cheapest literal superset (`concat|All|prepend`), over-matching costs walk and never findings or rows
- Text a capture's presence decides (a separator, a constant) comes from a rewriter with a literal fix, a `replace` over `(.)\z` compiling an anchored regex per site
- Unicode `\b` makes lazy and full DFAs quit at a non-ASCII byte, the search reruns on a slower engine

## [03]-[CANDIDATES]

Candidate kind sets decide how many nodes attempt a matcher:
- Match the rarest kind that identifies each reported or replaced construct
- Frequent kinds take direct child or field predicates that decide their own match
- `regex` reads a leaf field's name, operator, or literal, a fact `kind`, `field`, or `has` states takes none
- `regex` and `replace` UTF-8 check a node's whole span, inside a `has` walk to `end` each byte per enclosing node, a kind prefilter costing more
- Ordered `all` lists test the candidate's own name or text, then its structure, then relations and scope-resolving utils, and bind captures last
- Each `any` or `all` success copies the whole capture set, a rewriter's set holding every document capture, so captures cost per optional arm
- Kind tests leading an ordered `all` repeat the candidate kind filter, a map's `regex` already runs on those kinds alone
- Climbing to ancestors or scanning a scope before the candidate's own name test is the most frequent cost defect
- ESQuery selectors compile to existing matchers, shorter syntax changes no underlying traversal cost
- `transform` runs on every match whether its source is bound or not, fixed text belongs in the `fix` or template
- `rewrite` walks its whole source per rewriter, below nodes already replaced included, a kindless rewriter tries each node
- Rewriters take a `kind`, a signature's header captures and a kinded body walk each measured, a body cut still walking the body
- Every listed rewriter costs a walk where it never matches, one scoped to a construct nests in that construct's rewriter `transform`
- Kind tables size to a document's highest candidate kind id, a top-level `ERROR` (65,535) costing about 1.1 M instructions per file
- `ERROR` matched through a parent's `has` adds nothing, a rule reporting a file whose root is `ERROR` pays the table
- Outline item documents naming `ERROR` grow that table, literal false flags skip their compile under `--items exports`

No match result caches across candidates, each candidate reruns every relation and util, and a finding is the node that matched:
- Membership joins (a name in `__all__`, a later `export { x }`) cost linear work per candidate, quadratic over a file's items
- No rule resolves a scope's import set once for its uses, each use resolves it again
- Cheap tests of a necessary fact (name text, scope header, a kind-only climb as deep as a util chain's) precede a resolving util
- Scopes deciding the correction report their declaration once, uses needing correction reject other callees or names before resolving ownership
- Parameterized utils run at every candidate of the caller's kind set, each parameter reference naming no kind and building a fresh environment
- Param-free tests (kind, field, text) precede the call in a caller and the first parameter reference in a util

## [04]-[RELATIONS]

Relations reuse parsed trees, ancestor searches do not parse source again:
- Direct `field` and neighboring relations avoid whole-axis searches
- `inside`, `follows`, and `field` cost per visited ancestor or earlier sibling at each candidate, `has` near free, a rule anchors on the rarer parent
- `inside` with rule or `end` stopping allocates complete ancestry from the root before its first test
- Ancestor stops reduce predicate tests and add one stop test per ancestor, a rule stop over a frequent candidate can cost more than `end`
- Direct parents resolve from the root per evaluation, one kind-set `inside` gates the arms reading a parent, arms sharing its kind and field merge
- File-text gates (`inside: {kind: <root>, stopBy: end, regex: <word>}`) rescan the file per evaluation, they pay at rare candidates or words
- Rule stops on `has` prune matching subtrees
- Rule stops repeating a target's kinds compile a second list, `end` replaces them only where the target accepts every node of those kinds
- Sibling scans run first toward the nearer target, test `has` before `kind`, and join targets in one kindless `any:`
- `regex` on a literal a candidate's text must hold gates a `has` (`{regex: '^async', has: …}`, root `__all__`), one text scan in place of the walk it skips
- Positive relations add `secondary` captures whose complete text is repeated in JSON and diagnostic labels
- For predicates with no exported captures, retain explicit candidate kinds and use double `not` to discard new bindings
- Walk targets and stopping predicates use atomic kinds when a kind alone states their boundary
- Repeated atomic boundaries share a YAML anchor, a kind-only util adds a registry lookup at every visited node
- Consumers needing scope ownership can collect each boundary once and join source ranges instead of repeating ancestor text per match
- Bind source references once under distinct captures for identity and scope constraints, compare declaration names separately
- `nthChild` filters all named siblings on each evaluation, quadratic over a wide parent, direct fields state grammar roles and a list pattern arity (`{context: 'f($A, $B)', selector: <list>}`)
- First-of-kind tests run `not: {follows: {kind: <k>, stopBy: end}}`, stopping at the nearest earlier match, then `nthChild: {position: 1, ofRule: {kind: <k>}}` for siblings an error-recovered parent hides from that walk
- `nthChild.ofRule` can supply candidate kinds, matching the same filter outside it repeats work unless captures require ordering

## [05]-[OUTLINE]

Each process pays for every loaded catalog before it reads its input, one 15.7 KB TypeScript file under thirteen custom languages spends 1,488 M instructions and 95 ms:

| [INDEX] | [COST]                                       | [M_INSTRUCTIONS] | [SKIPPED_BY]                   |
| :-----: | :------------------------------------------- | :--------------- | :----------------------------- |
|  [01]   | Repository catalogs, 691 of it deserializing | 834              | Config holding fewer catalogs  |
|  [02]   | Walk filter over every catalog's language    | 238              | `-l`, `--stdin`                |
|  [03]   | Bundled catalog                              | 150              | `--no-default-outline-rules`   |
|  [04]   | Parse and extract                            | 116              | Nothing                        |
|  [05]   | `languageInjections`                         | 103              | Config holding fewer rows      |
|  [06]   | Thirteen libraries                           | 31               | Config holding fewer libraries |
|  [07]   | Process start                                | 24               | Nothing                        |

Nothing selects catalogs by input, these forms move the cost:
- One process over every known path pays load once at flat peak memory, a process per file pays it per file
- Narrowed configs cut a one-file run to under half and `--no-default-outline-rules` to a quarter, rows unchanged
- Cheaper documents stop short, a whole config with empty repository catalogs still loads above a narrowed one

Narrowed configs serve a reader that cannot batch, one process per blob or per edit:
- Each holds its host's `customLanguages` row, one row per language the host injects, and the `languageInjections` rows of that host
- Injection rows naming an unregistered host fail config load, an unregistered injected language skips
- Paths outside the config lose their catalog silently, a `.rs` file under a TypeScript config prints bundled `Rust` rows
- Trees and agent commands hold no such reader, one command answers any language and a tree pays load once

Load falls in order to YAML deserialization, a per-language walk filter, item compile, and member compile, a one-file outline pays it whole and a tree barely sees it:
- Walk filters build a type set per document group, `-l` and `--stdin` compile the same documents and skip most of it
- `--type` matching no document compiles nothing, `--stdin -l <lang> --view names --type null` isolates deserialization
- `--no-default-outline-rules -l <lang> <tree>` isolates read, parse, and injection-rule cost
- `/usr/bin/time -l` counts its direct process alone, a shell wrapper hides the child

Extraction adds over parse:
- Plain items add nothing, field captures a little, a regex cut more, a rewrite of a whole declaration an order of magnitude more
- Member documents that never match still cost their item's full subtree walk
- Members matching a name token walk the owner's body, a declaration kind adds under 1% of a parse, a rare header child a quarter, `identifier` a whole parse
- Each member row pays its captures, transforms, owner climb, and qualifier rewrite, extraction cost grows with rows and not nodes
- Items a filter drops pay member walk, name and signature render, and flag evaluation before removal
- Injected parse cost per site scales with host bytes, each site clones the host source
- Views decide output volume and little CPU, `expanded` costs near a tenth more instructions than `names` for a multiple of its bytes

## [06]-[INJECTIONS]

Injection entries run wherever a command walks a host file:
- Entries walk every node of every host file whatever their kind or readers of the injected language, an entry joins where hosts hold sites
- Entry rules pay candidate and relation cost as scan rules do
- Entry `utils` compile per entry, YAML anchors share source text across entries and compile at each use
- `scan` and `run` parse every resolvable document, read by a rule or not, `outline` skips languages holding no catalog before parsing, unresolved names build no document
- Overlapping entries or nested host regions parse the same source repeatedly
- Choose disjoint ownership, or outer regions only when the injected grammar preserves required coverage

## [07]-[PROCESSES]

Each CLI process chooses workers independently and shares no concurrency budget with other scans:
- Instructions stay flat as `--threads` grows while wall time falls
- `scan --max-results` bounds returned matches after per-file scans, one large file still completes matching
- Parallel file completion makes limited result order unsuitable for a stable global selection
- `--json=stream` changes output framing, queued file results and consumer buffering still consume memory
- Use `scan --inline-rules` with `--max-results` for bounded searches

## [08]-[MEASUREMENT]

Instruction counts hold steady between runs of one load order where wall time moves, lazily resolved utils giving a rule one cost per load order, comparisons take one thread, one output mode, and the minimum and median of repeated runs per side:

```bash
hyperfine -N --ignore-failure=1 --metrics instructions,time_wall_clock -r <runs> -L id '<baseline>,<rule>' --export-json <timings> "ast-grep scan --threads 1 --filter '^{id}\$' --json=stream <path>"
jq -r '.results[] | [.parameters.id, .summary.instructions.min, .summary.instructions.median] | @tsv' <timings>
```

- `--ignore-failure=1` accepts the finding exit and fails the benchmark on a load exit (`8`), `-i` reports a failed load's cost as the rule's
- Baselines match the root kind over the same files, the gap to one is the rule's matching with load, discovery, and parsing subtracted
- Mixed trees pass `--globs` for each extension the language and its injecting hosts read
- Load cost alone reads from one empty file under `--filter` of one rule, a filter matching none exits 3 and hyperfine fails it
- Changes report findings or rows and instructions, each gained and lost
- Cuts losing a fact a reader needs take a form moving the anchor, the pruned subtree, or where the fact is read
