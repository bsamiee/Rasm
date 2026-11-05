# [PERFORMANCE]

Scan cost combines process startup, rule compilation, file discovery, parsing, injection, candidate matching, relation traversal, and output. Reduce repeated work at its owner at full coverage before changing parallelism.

## [01]-[WORKLOAD]

Every command compiles configured utilities and rules, a fixed load paid before it walks and before active rule ids select:
- Batch independent rules over shared files in one scan to share discovery and parsing
- Active rule selection reduces matching without avoiding project rule loading
- YAML node count and per-document regex compiles set load cost, bytes, formatting, and file layout move none
- Unicode `\w`, `\s`, `\d`, and `\b` multiply a regex's compile cost, every scan and outline run compiles each loaded regex
- Custom-language registration loads every configured library; consumers can use generated registration subsets without global utilities
- Stdin scans need `--min-severity hint` or higher to exclude rules with severity `off`
- Pass known paths directly, directory scans use native discovery and ignore handling
- Order broad CLI glob inclusions before exclusions; later matching overrides win
- Keep shared traversal policy in project configuration, invocation flags select one run's workload

## [02]-[CANDIDATES]

Candidate kind sets decide how many nodes attempt a matcher:
- Match the rarest kind that identifies each reported or replaced construct
- Frequent kinds take direct child or field predicates that decide their own match
- Restrict syntax structurally before reading text or resolving a related declaration, tests `kind`, `field`, `has`, or captures state need no `regex`
- Apply `regex` to a leaf field when only its name, operator, or literal matters, a class over a parsed token's characters repeats lexing
- Composite-node `regex` reads complete source text, nested declarations and bodies included
- Unicode `\b` makes lazy and full DFAs quit at a non-ASCII byte, the search reruns on a slower engine
- Ordered `all` tests capture-free text, fields, truth, and call context before captures and scope-resolving utilities
- Bind owners before capture reuse, an unbound `pattern: $NAME` binds any candidate instead of testing equality
- ESQuery selectors compile to existing matchers, shorter syntax changes no underlying traversal cost
- `transform` runs on every match, `rewrite` walks every descendant of its source per rewriter, a kindless rewriter tries each node
- Utility calls share definitions without caching match results, a util at frequent candidates reruns per call

Whole-file predicates repeated at frequent candidates multiply source-text reads. Match a declaration or owner once when its scope decides the correction. When a use itself needs correction, reject nonmatching callees or names before resolving ownership.

## [03]-[RELATIONS]

Relations reuse parsed trees, ancestor searches do not parse source again:
- Direct `field` and neighboring relations avoid whole-axis searches
- Positive relations add `secondary` captures whose complete text is repeated in JSON and diagnostic labels
- For predicates with no exported captures, retain explicit candidate kinds and use double `not` to discard new bindings
- `inside` with rule or `end` stopping allocates complete ancestry before evaluating ancestor predicates
- Ancestor stops reduce predicate tests, direct parents avoid ancestry arrays but still resolve paths from the root
- `has` with `end` uses iterative descendant traversal, a rule stop prunes matching subtrees
- Walk targets and stopping predicates use atomic kinds when a kind alone states their boundary
- Repeated atomic boundaries share a YAML anchor, a kind-only util adds a registry lookup at every visited node
- Scope joins start at the declaration and search once when all reported uses share that owner
- Consumers needing scope ownership can collect each boundary once and join source ranges instead of repeating ancestor text per match
- Bind source references once under distinct captures for identity and scope constraints, compare declaration names separately
- `nthChild` filters all named siblings on each evaluation, use direct fields for grammar roles
- `nthChild.ofRule` can supply candidate kinds, matching the same filter outside it repeats work unless captures require ordering

## [04]-[INJECTIONS]

Injection entries run wherever a command walks a host file:
- Each entry runs its rule over every host file, whether or not a rule or catalog reads the injected language
- Entry rules pay candidate and relation cost like scan rules, a rare `kind` and a capture-free callee test precede scope predicates
- Entry `utils` compile per entry, YAML anchors share source text across entries and compile at each use
- Each match builds one document over its range and parses it once, injected documents never reinject
- Overlapping entries or nested host regions parse the same source repeatedly
- Choose disjoint ownership, or outer regions only when the injected grammar preserves required coverage
- Sites a host states no dialect for inject under every dialect entry, each adding a parse and its matches

## [05]-[PROCESSES]

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

## [06]-[MEASUREMENTS]

Measurements report one command under stated files, parser versions, rule selection, output, cache state, and concurrency, and gate nothing.

- Retired instructions hold steady under concurrent load where wall time drifts, `hyperfine --metrics instructions` counts them
- Record wall time, user and system CPU, peak resident memory, exit status, and file counts beside them
- `--ignore-failure=1` accepts finding exits while load and usage errors fail benchmarks
- One-line input isolates fixed load, a corpus run less that load isolates parsing and matching
- Parsing floors use an active kind-only rule over explicit language globs, disabling every rule removes language filtering
- Benchmark selected rules without inherited `--off`, severity overrides do not reactivate disabled rules
- Measure the production command separately from its test runner; a runner's peak memory includes its own work
- Separate rule changes from toolchain changes when either can explain a reading
- Keep durations and machine values in run artifacts, durable guidance states the shape causing repeated work

Command cost:

```bash
hyperfine -N --ignore-failure=1 --warmup 3 --metrics all -r <runs> --export-json <timings> \
    "ast-grep scan --custom-languages allow --threads <threads> --json=stream <path>"
```

Rule cost is single-thread instruction count beside a parse-error rule over the same files, the gap is what the matcher adds, `<ids>` joining the parse-error rule and each measured rule with `,`, `<globs>` joining the rule's language extensions, its `languageGlobs` entries, and each host injecting it in one brace glob:

```bash
hyperfine -N --ignore-failure=1 -r 3 --metrics instructions -L id '<ids>' --export-json <timings> \
    "ast-grep scan --threads 1 --custom-languages allow --filter '^{id}$' --globs '<globs>' --json=stream <path>"
jq -r '.results[] | [.parameters.id.value, .summary.instructions.mean] | @tsv' <timings>
```

Catalog cost is `ast-grep outline` time on a one-line file under a config keeping every language and library, with that catalog in place and every other catalog a one-document stub:

```bash
hyperfine -N --warmup 3 'ast-grep outline -c <config> --custom-languages allow <one-line file>'
```
