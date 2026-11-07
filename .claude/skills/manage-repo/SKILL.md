---
name: manage-repo
description: "Use when changing how the repository is structured, built, or checked, covering each concern's owner and placement"
---

# [MANAGE_REPO]

Rules here hold for every language, each reference holds tool behavior deciding one toolchain's rows.

[REFERENCES]:
- [01]-[NX](references/nx.md): Task graph, how Nx composes inferred targets and decides what a task hashes and runs
- [02]-[TOOLING](references/tooling.md): Toolchain no language owns, from what installs a tool to what keeps a service running
- [03]-[HK](references/hk.md): File checkers and writers under one runner, how hooks, selection, scheduling, and patches decide each step row
- [04]-[TYPESCRIPT](references/typescript.md): One catalog resolving packages, each host runtime deciding a program's types and build
- [05]-[PYTHON](references/python.md): Project resolution under uv, each embedded interpreter bounding what resolves, builds, and bundles
- [06]-[DOTNET](references/dotnet.md): Shared MSBuild declarations and central versions, installed Rhino deciding what a host project builds against
- [07]-[SWIFT](references/swift.md): SwiftPM modules and Xcode apps linking them, each setting and lock at the system that reads it
- [08]-[JAVA](references/java.md): Scripts a tool loads, each checker resolving them against that tool's jars
- [09]-[RUST](references/rust.md): Cargo workspace, one pinned nightly and root rows holding policy every member inherits
- [10]-[DERIVATION](references/derivation.md): Mechanisms each configuration system provides to discover members, inherit policy, and select modes

## [01]-[VARIATION]

Repository work varies by operation, subject (project, package, stack, file), and mode (configuration, platform, interpreter, environment). Names state operations alone, subject and mode come from the declaration owning them and reach a tool through that tool's own mechanism:
- Facts sit once at their owner (manifest, lock, catalog, host), each list, path, version, or name another file needs derives from it
- Members come from tool discovery (workspace globs, inferred projects, item includes), a member list goes unless it is the owner
- Shared policy sits in the file every member inherits, a member file states what differs from it alone
- Project kinds (host, platform, product) join a root file as a row a tag or condition selects, or a kind file passing values to its general form
- Root files compute per-project values (cache, output path) from the project directory their tool supplies or a project file passes
- Modes take the tool's mode mechanism (configuration, condition, marker, matrix, platform file), never a name or row per value
- Splits a tool forces (one interpreter per run) derive runs and scope from member declarations, a tool reading each member's version runs once
- Adding a member, version, or platform edits its own declaration alone, an edit anywhere else marks a hand-enumerated variant
- Derivation no tool mechanism covers sits where declarations become that tool's model: `tools/nx` inference, root MSBuild targets, `infra/`
- `tools/`, `infra/`, `plugins/`, and `eng/` code reads project names, paths, and values from arguments and project declarations alone
- `eng/` programs perform operations (upgrade, release, install) and hold no member list

## [02]-[PLACEMENT]

- Concerns with no owner take a new file named for the tool that reads it
- Structure joins with its first consumer, a row, file, target, or value nothing reads goes
- Row consumers are importers (`package.json` included), reading tools, targets, MCP rows, skills, `.gitattributes` filters, and `[CLI_TOOLING]` rows
- Catalog rows and their overrides stay without an importer, catalogs hold packages for future projects
- Build chains and project kinds of a declared host stay while no project uses them
- Rows naming a package, binary, or server point at one a project file or tool row installs
- Root files hold policy every project shares, a row one project consumes sits in the project file
- Project files are written by hand as the file set their language's init command produces
- Project files extending a root file exist for a plugin discovering projects by file name, a `--config` tool reads the root file from any `cwd`
- Rows changing no tool output or lock go, documented defaults included, a row canceling another goes with the canceled row
- Lists replacing a default list keep its entries, a schema, release notes, or installed source states each default
- Skills name the tools they drive, their scripts, and output paths, and hold no target or project row
- Scripts join their subject's skill, a script an app, target, or workflow consumes joins the repository
- Capabilities come from tool configuration and extension points, then another tool, then a script or `eng/` project for work no tool or package does
- Root targets unify format, build, install, and release of the repository's own code
- Wrappers over one mise tool are targets only as a prerequisite other targets name in `dependsOn`

## [03]-[ENTRY_POINTS]

Targets, tasks, and repository tools serve agents, and each call answers its caller's question whole:
- Concerns keep one entry system, a concern a target, a mise task, and a script each reach keeps one of them
- Operations take one name, a subject or mode reaches it as an argument or derives from its context, a name per subject or mode goes
- Entry points derive their items from declarations, a configuration, script, or README line per item goes
- Inputs a tool derives (working directory, declarations, live state, installed versions) take no flag
- Arguments after `--` select an operation's subject (path, app, filter) or pass a one-run switch, checker policy stays in target `options.args`
- Command prefixes shared across targets (`doppler run`) state no duplicated fact
- Defaults run the common case, a repeated call reaches the same state
- Operations on external state (deploy, publish, delete) report the live difference from declarations and apply on one explicit argument
- Calls return their result: an action prints the state it produced, a failure names its subject and cause and one available next action
- Output is machine-readable, exit status matches the printed outcome, and help or a call with nothing to do exits 0
- Help derives from the parser's declarations, logs and cleanup run automatically into the repository log and cache folders

## [04]-[CHECKS]

[TARGETS]:
- Projects with their own check hold one `check` target with no per-language name, dry-run variant, or script, a root-checked project holds none
- `check` and its prerequisites start no container
- Checkers and type checkers run as `lint:<tool>` and `typecheck:<tool>`, a project `lint` depends on its `lint:*`
- `hk` holds writers and each file checker's file set, a writer needing a restore or generator holds `format:<tool>`
- `<tool>` names the program implementing command: `dotnet format` is `dotnet-format`, `cargo fmt` is `rustfmt`, `cargo dylint` is `dylint`
- Checkers take one `<kind>:<tool>` name, a subcommand, language, package, or runtime is an argument, a configuration, or a run derived from member declarations
- Checker command lists hold dependent steps alone, a failing step hides findings of the rest
- Runners take continue-on-failure from their configuration, a flag per call leaves a direct call stopping at its first failure
- Writers sharing files take `parallelism: false`, `dependsOn` names producer prerequisites alone
- Checkers fail on each finding they print, a tool passing warnings takes its own switch (`--strict`, `--error-on-warnings`, a config deny row)
- Inputs and cache settings checker targets share sit once in a `targetDefaults` key matching their names
- Writers take no CI format-and-diff step, each writer's check form or a build diagnostic fails on every change it writes

[OWNERS]:
- Defect forms take the first owner able to report them: checker configuration, then ast-grep, then a `tools/` checker
- Checker configuration holds levels, options, thresholds, and bans for rules a checker includes
- Forms wrong whatever their types (shape, nesting, banned construct, hand loop) take ast-grep rules across languages
- Compiler extensions hold rules that resolved names, types, expansions, or other files decide
- `tools/` checker folders take the tool or compiler name they extend
- Forms code cannot write or no checker can see take no check
- Sites more than one rule, parse, or checker reports take one owner by proven correctness and coverage, others exclude them by one shared predicate
- Checker rules contradicting a required form or another checker stay off with their reason

[RULES]:
- Rules derive names, types, and contracts from host APIs and packages, a hand list of what a resolver or package states goes
- Rules state a defect form code showed or a host contract states, a rule for a form neither shows goes
- Tightening checker rows (ban, required form, lowered threshold) state policy and stay with no violating file
- Rules use the owner resolving each supported spelling
- New cases widen the rule holding their category, one category holds one rule per checker
- Rule, checker, or file extension rows join with a tracked file they read and the tree passing them
- Languages built into ast-grep take no `sgconfig.yml` row until a newer grammar release parses a form the bundled grammar fails
- Grammars ast-grep lacks or bundles behind a newer release build from a catalog package through project `grammars` into a `customLanguages` row
- Host syntax a catalog grammar lacks (`${…}` template substitutions) joins `tools/ast-grep/grammars/<language>/grammar.js` extending that grammar
- Embedded SQL injects into `sql` only where its dialect is SQLite, templates under a `pg` key or in a `Pg`-named declaration stay out of the parse
