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
- Literal leaf `pattern`s (`Project`) test text with no regex compile, XML loads one at a tenth of an anchored `regex`

Capture sites cost what parsing their snippet costs, a bare `$X` that triggers parse recovery loads above `{context: <valid code>, selector: <kind>}`, per site over 100 rules on an empty file:

| [INDEX] | [LANGUAGE] | [BARE_M] | [SELECTOR_M] | [CHEAPER] |
| :-----: | :--------- | -------: | -----------: | :-------- |
|  [01]   | C#         |     0.99 |         0.14 | Selector  |
|  [02]   | Rust       |     0.47 |         0.08 | Selector  |
|  [03]   | Java       |     0.31 |         0.08 | Selector  |
|  [04]   | SQL        |     0.07 |         0.10 | Bare      |
|  [05]   | pgsql      |     0.06 |         0.11 | Bare      |
|  [06]   | TOML       |     0.03 |         0.08 | Bare      |
|  [07]   | XML        |     0.03 |         0.07 | Bare      |
|  [08]   | Python     |     0.02 |         0.05 | Bare      |
|  [09]   | Bash       |     0.02 |         0.05 | Bare      |
|  [10]   | Swift      |     0.01 |         0.08 | Bare      |
|  [11]   | TypeScript |    <0.01 |         0.03 | Bare      |

- Selector forms bind their kind alone, a site over several kinds weighs an `any:` of selectors against the bare form
- Captures of a whole declaration bind once in a `utils` entry every arm reads, one site per document
- Each language and grammar change measures its own sites, a `kind` alone against each form on one empty file

Regexes compile eagerly per document with no sharing across documents or rules, a `replace` costing about 0.5 M and 1.2 M with grouped alternation. Compile cost rises in order: unanchored literal, `$`, ASCII class and literal prefix (`^foo`), `\b` under either mode, Unicode `\d` at twice `[0-9]`, Unicode `\s` at six times an ASCII class, Unicode `\w` at ten times `[0-9A-Za-z_]`, `\p{XID_Start}\p{XID_Continue}*`:
- Only an ASCII class saves over `\s`, an explicit `\p{White_Space}` class costs the same
- `.` costs as much as `^\s`
- Unicode `\b` makes lazy and full DFAs quit at a non-ASCII byte, the search reruns on a slower engine

## [03]-[CANDIDATES]

Candidate kind sets decide how many nodes attempt a matcher:
- Match the rarest kind that identifies each reported or replaced construct
- Frequent kinds take direct child or field predicates that decide their own match
- `regex` reads a leaf field's name, operator, or literal, over a composite node it reads whole source text with nested bodies, a fact `kind`, `field`, or `has` states takes none
- Ordered `all` lists test the candidate's own name or text, then its structure, then relations and scope-resolving utils, and bind captures last
- Climbing to ancestors or scanning a scope before the candidate's own name test is the most frequent cost defect
- ESQuery selectors compile to existing matchers, shorter syntax changes no underlying traversal cost
- `transform` runs on every match, `rewrite` walks its whole source per rewriter, below nodes already replaced included, a kindless rewriter tries each node
- Rewriters take a `kind`, a signature's source holds no body, built from header captures or cut at the body
- Scans size each document's kind table to its highest candidate kind id, `ERROR` (65,535) in a rule's top-level kind set costs about 1.1 M instructions per file
- `ERROR` matched through a parent's `has` adds nothing, a rule reporting a file whose root is `ERROR` pays the table
- Outline item documents naming `ERROR` grow that table, literal false flags skip their compile under `--items exports`

No match result caches across candidates, each candidate reruns every relation and util, and a finding is the node that matched:
- Membership joins (a name in `__all__`, a later `export { x }`) cost linear work per candidate, quadratic over a file's items
- No rule resolves a scope's import set once for its uses, each use resolves it again
- Cheap forms narrow candidates before resolving, a name-text gate first, a renamed import reported once at its `use … as` line
- Scopes deciding the correction report their declaration once, a use needing correction rejects nonmatching callees or names before resolving ownership
- Parameterized utils run on every candidate of the caller's kind set, a `kind` beside the call keeps wrong-kind nodes out

## [04]-[RELATIONS]

Relations reuse parsed trees, ancestor searches do not parse source again:
- Direct `field` and neighboring relations avoid whole-axis searches
- `inside` and `follows` cost per visited ancestor or sibling at each candidate, `has` walks down iteratively near free, a rule anchors on the rarer parent and looks down
- `inside` with rule or `end` stopping allocates complete ancestry from the root before its first test
- Ancestor stops reduce predicate tests and add one stop test per ancestor, a rule stop over a frequent candidate can cost more than `end`
- Direct parents avoid ancestry arrays but still resolve paths from the root
- Rule stops on `has` prune matching subtrees
- Sibling scans run first toward the nearer target, test `has` before `kind`, and join targets in one kindless `any:`
- Root `regex` on a sibling target's literal (`__all__`) ahead of its `has` costs one text scan where the target is absent
- Positive relations add `secondary` captures whose complete text is repeated in JSON and diagnostic labels
- For predicates with no exported captures, retain explicit candidate kinds and use double `not` to discard new bindings
- Walk targets and stopping predicates use atomic kinds when a kind alone states their boundary
- Repeated atomic boundaries share a YAML anchor, a kind-only util adds a registry lookup at every visited node
- Consumers needing scope ownership can collect each boundary once and join source ranges instead of repeating ancestor text per match
- Bind source references once under distinct captures for identity and scope constraints, compare declaration names separately
- `nthChild` filters all named siblings on each evaluation, use direct fields for grammar roles
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

The engine holds no switch selecting catalogs by input, three forms move this cost:
- Passing every known path to one process pays load once at flat peak memory, 40 TypeScript files take 131 ms in one process against 1,121 ms per file narrowed and 3,631 ms whole
- A narrowed config takes one file from 93-101 ms to 33-47 ms, `--no-default-outline-rules` to 20-33 ms, rows stayed equal over every TypeScript, Rust, Python, C#, and Swift file
- Cheaper documents stop short of it, zero repository catalog volume leaves a whole config above a narrowed one, whose own catalogs cost 187 M for TypeScript

Narrowed configs serve a reader that cannot batch, one process per blob or per edit:
- Each holds its host's `customLanguages` row, one row per language the host injects, and the `languageInjections` rows of that host
- An injection row naming an unregistered host fails config load, an unregistered injected language skips
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
- Items a filter drops pay member walk, name and signature render, and flag evaluation before removal
- Injected parse cost per site scales with host bytes, each site clones the host source
- Views decide output volume and little CPU, `expanded` costs near a tenth more instructions than `names` for a multiple of its bytes

## [06]-[INJECTIONS]

Injection entries run wherever a command walks a host file:
- Each entry walks every node of every host file whatever its rule's kind or whether a rule or catalog reads the injected language, an entry joins where hosts hold sites
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

Instruction counts hold steady between runs where wall time moves, one thread and one output mode keep the comparison fair:

```bash
hyperfine -N --ignore-failure=1 --metrics instructions,time_wall_clock -r <runs> -L id '<baseline>,<rule>' --export-json <timings> "ast-grep scan --threads 1 --filter '^{id}\$' --json=stream <path>"
jq -r '.results[] | [.parameters.id, .summary.instructions.mean] | @tsv' <timings>
```

- `--ignore-failure=1` accepts the finding exit and fails the benchmark on a load exit (`8`), `-i` reports a failed load's cost as the rule's
- Baselines match the root kind over the same files, the gap to one is the rule's matching with load, discovery, and parsing subtracted
- Mixed trees pass `--globs` for each extension the language and its injecting hosts read
- Load cost alone reads from one empty file under `--filter` of one rule, a filter matching none exits 3 and hyperfine fails it
- Changes report findings or rows gained and lost beside instructions gained and lost, a cut losing a fact a reader needs moves the anchor, the pruned subtree, or the fact's source
