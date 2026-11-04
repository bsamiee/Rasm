# [PERFORMANCE]

Scan cost combines process startup, rule compilation, file discovery, parsing, candidate matching, relation traversal, and output. Reduce repeated work at its owner before changing parallelism.

## [01]-[WORKLOAD]

Project scans compile configured utilities and rules before selecting active rule ids:
- Batch independent rules over shared files in one scan to share discovery and parsing
- Active rule selection reduces matching without avoiding project rule loading
- Unicode `\w` and `\s` multiply a regex's compile cost, every scan and outline run compiles each loaded regex
- Custom-language registration loads every configured library; consumers can use generated registration subsets without global utilities
- Stdin scans need `--min-severity hint` or higher to exclude rules with severity `off`
- `scan -r` and `--inline-rules` load project global utilities without loading project rule directories
- Pass known paths directly, directory scans use native discovery and ignore handling
- CLI `--globs` excludes files before reading and parsing, rule `files` and `ignores` apply after parsing
- Order broad CLI glob inclusions before exclusions; later matching overrides win
- Keep shared traversal policy in project configuration, invocation flags select one run's workload
- Cache inputs include source files, rules, utilities, grammar declarations, tool versions, and applicable ignore files
- Runtime-specific rule partitions scan their own source scopes, general rules run once across supported runtimes
- Route checks and updates by native execution mode before selecting cached tasks or forwarding options

## [02]-[CANDIDATES]

Candidate kind sets decide how many nodes attempt a matcher:
- Match the rarest kind that identifies each reported or replaced construct
- Frequent kinds take direct child or field predicates that decide their own match
- Positive `any` arms union kind sets, a kindless arm widens selection to every kind
- Positive `all` clauses intersect kind sets, binding order still decides capture behavior
- Restrict syntax structurally before reading text or resolving a related declaration, tests `kind`, `field`, `has`, or captures state need no `regex`
- Apply `regex` to a leaf field when only its name, operator, or literal matters, a class over a parsed token's characters repeats lexing
- Composite-node `regex` reads complete source text, nested declarations and bodies included
- Unicode `\b` makes lazy and full DFAs quit at a non-ASCII byte, the search reruns on a slower engine
- Ordered `all` tests capture-free text, fields, truth, and call context before captures and scope-resolving utilities
- Bind owners before capture reuse, an unbound `pattern: $NAME` binds any candidate instead of testing equality
- ESQuery selectors compile to existing matchers, shorter syntax changes no underlying traversal cost

Whole-file predicates repeated at frequent candidates multiply source-text reads. Match a declaration or owner once when its scope decides the correction. When a use itself needs correction, reject nonmatching callees or names before resolving ownership.

## [03]-[RELATIONS]

Relations reuse parsed trees, ancestor searches do not parse source again:
- Direct `field` and neighboring relations avoid whole-axis searches
- Positive relations add `secondary` captures whose complete text is repeated in JSON and diagnostic labels
- For predicates with no exported captures, retain explicit candidate kinds and use double `not` to discard new bindings
- `inside` with rule or `end` stopping allocates complete ancestry before evaluating ancestor predicates
- Ancestor stops reduce predicate tests, direct parents avoid ancestry arrays but still resolve paths from the root
- `has` with `end` uses iterative descendant traversal, a rule stop prunes matching subtrees
- `field` with a rule stop examines the field child and its direct children, deeper descent needs another relation
- Repeated grammar fields require sibling matching, a field lookup reads their first child alone
- Walk targets and stopping predicates use atomic kinds when a kind alone states their boundary
- Repeated atomic boundaries share a YAML anchor, a kind-only util adds a registry lookup at every visited node
- Scope joins start at the declaration and search once when all reported uses share that owner
- Consumers needing scope ownership can collect each boundary once and join source ranges instead of repeating ancestor text per match
- Bind source references once under distinct captures for identity and scope constraints, compare declaration names separately
- `nthChild` filters all named siblings on each evaluation, use direct fields for grammar roles
- `nthChild.ofRule` can supply candidate kinds, matching the same filter outside it repeats work unless captures require ordering

## [04]-[UTILITIES]

Utility calls share definitions without caching match results:
- Retain shared predicates where independent consumers need the same structural fact
- Inline one-use fixed predicates and delete utilities without consumers
- Distinct argument rules or private capture scopes are separate parameterized-util consumers even within one file
- Preserve matching and exported captures independently when collapsing utility calls
- Place declaration-independent leaf tests before imported-name or owner resolution
- Kind inference and dependency ordering need separate attention, relational references can hide load-order edges
- Parameterized calls under relations require supported load ordering, adding an unused dependency solely to force loading changes no model

## [05]-[RECURSION]

Every recursive utility cycle must move strictly along one finite tree order:
- Descendant recursion consumes child syntax, ancestor recursion consumes enclosing syntax
- Sibling recursion keeps one direction and moves past its previous declaration
- Mixed `has` and `inside`, or `follows` and `precedes`, can revisit the same node
- Alias lookup can return its own initializer, structural wrapper recursion alone does not make alias recursion terminate
- Resolve a declaration once, then match its initializer with a separately terminating structural predicate
- Load-time cycle checks follow direct and argument dependencies, skipping relations and descent hidden inside called templates
- Arbitrary depth caps hide a nonterminating relation or reject accepted syntax, fix traversal direction or ownership
- Recursive grammar productions express nested syntax without enumerating a maximum nesting depth

Language injections parse each matching injected region once per configured entry. Injection processing does not recursively reinject resulting documents. Overlapping entries or nested host regions can still parse the same source repeatedly, choose disjoint ownership or outer regions only when the injected grammar preserves required coverage.

## [06]-[PROCESSES]

Each CLI process chooses workers independently and shares no concurrency budget with other scans:
- Compare `--threads` values under expected concurrent workloads before setting a process budget
- Measure group completion time, total CPU, and aggregate memory alongside individual latency
- `scan --max-results` bounds returned matches after per-file scans, one large file still completes matching
- Parallel file completion makes limited result order unsuitable for a stable global selection
- Python server ast-grep-mcp starts a fresh CLI per tool request, config and grammar content reload without restarting
- Restart ast-grep-mcp to refresh inherited executable paths or advertised language metadata
- MCP `max_results` limits returned matches after subprocess completion, full CLI output remains buffered
- `--json=stream` changes output framing, queued file results and consumer buffering still consume memory
- Use `scan --inline-rules` with `--max-results` for bounded searches
- NAPI `findAll` compiles and traverses per call, `pattern` constructs configuration without a compiled-query cache
- NAPI `findInFiles` compiles once for its parallel file walk, sharing discovery and parsing
- `findInFiles` can resolve before queued callbacks, finish aggregation when processed callbacks reach its returned file count
- `parseAsync` uses libuv workers, `findInFiles` runs a parallel Rust walker inside one asynchronous task
- NAPI nodes retain complete source trees, aggregate text and ranges when later work needs values alone
- NAPI ranges and edits use UTF-16 code units, CLI byte offsets use UTF-8 and columns count Unicode scalars
- NAPI `fieldChildren` returns repeated field children, `field` returns their first node
- Register dynamic NAPI languages together, later `registerDynamicLanguage` calls are ignored
- Derive numeric NAPI kind ids with `kind(language, name)`, grammar versions own their values
- NAPI queries do not implement project rule directories, global utility loading, or the complete CLI scan pipeline

## [07]-[MEASUREMENTS]

Compare identical files, parser versions, rule selection, output, cache state, and concurrency. Separate rule changes from toolchain changes when either can explain the result. Record wall time, user and system CPU, peak resident memory, exit status, file counts, and changed findings.

Benchmark selected rules without inherited `--off`, severity overrides do not reactivate disabled rules.

Use native timing tools over the command being changed:

```bash
hyperfine -N --ignore-failure=1 --metrics all -r <runs> --export-json <timings> \
    "ast-grep scan --custom-languages allow --threads <threads> --json=stream <path>"
```

`--ignore-failure=1` accepts finding exits while load and usage errors fail benchmarks. Native `time` reports resource use when a benchmark tool omits it. Measure the production command separately from its test runner; a runner's peak memory includes its own work. Parsing baselines use an active kind-only rule over explicit language globs, disabling every rule removes language filtering.

Compare rule ids, paths, matched text and ranges, captures, replacement text, and replacement ranges on identical inputs. Explain changed findings and fixes through corrected coverage or duplicate removal. Keep durations and machine values in run artifacts, durable guidance states the shape causing repeated work.

```bash
jq -cS '[.ruleId, .file, .range, .text, .metaVariables, .replacement, .replacementOffsets]' <state>.jsonl | sort > <state>.keys
```
