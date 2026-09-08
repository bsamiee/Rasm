# [TOOL_FORMS]

Forms and facts per tool an agent step names, each proven by a run from repository root, with the exit code or line the run printed where a step reads it.

## [01]-[GIT]

- `git diff --name-only --diff-filter=ACMR <commit>` lists changed files on disk, `git ls-files --others --exclude-standard` adds untracked ones
- `git diff --name-only <commit>` lists a deleted path
- `git status --porcelain | cut -c4-` lists `old -> new` for a rename
- `git diff --numstat <commit> -- <scope>` prints `added deleted path` whole, `--stat` truncates a long path
- `git diff --name-only <commit> -- . ':(exclude)<path>'` lists the complement of a scope in one command with every exclude
- `git diff | shasum` before and after a formatter proves it rewrote nothing
- `git diff --exit-code` fails on the run's own edits
- `git log -p --follow -- <file>` holds facts a rewrite dropped across a rename
- `git rev-parse --show-toplevel` is the absolute root a tool outside the shell resolves against
- `git rev-parse HEAD` is the default commit
- `git show HEAD:<file> > <file>` restores a file
- `git mv` renames a file

## [02]-[FILES]

- `fd -e <ext> . <dir>` names a pattern, `-H` includes hidden paths, `-I` ignored ones
- `fd -g '<id>.yml' <dir>` finds a rule's siblings
- `fd` refuses a file as search path with `Search path '<file>' is not a directory`
- `fd --strip-cwd-prefix` refuses a positional path
- `fd -e csproj . <scope> -x <cmd> {}` runs one command per hit, `-X <cmd>` one command over the batch
- `fd -e yml . <dir> -x basename {} .yml | paste -sd'|' -` derives an id alternation from the files that own it
- `fd -t f --changed-within <span>` lists every writer's files
- `rg -n -F '<text>'` proves an old spelling absent at exit 1
- `rg -l -F` lists files for `sd`
- `rg -c` counts before and after a rewrite
- `rg` refuses look-around without `--pcre2`
- `rg --pcre2` refuses an unknown escape (`\y`) with `unrecognized character follows \`
- `rg -nU --pcre2 -e '^[ \t]*(#(?!!)|//|<!--|/\*)' -e '^[ \t]*(message|note):' -e '"""[\s\S]*?"""' <files>` extracts comments and rule text
- `sd -F '<old>' '<new>' <files>` rewrites a fixed string, `sd` has no `-s` flag
- `sd` takes `--` before a replacement opening with `-`
- `jq` and `yq` refuse `.vscode/settings.json` over its `//` comments, `Read` reads it
- `yq -r '[.id, .message] | join(" | ")' <rules>/*.yml` maps a rule family
- `yq '.snapshots | map_values(.fixed)' <snapshot>` reads a snapshot
- `K='<case>' yq '.snapshots[strenv(K)]' <snapshot>` reads one case
- `stat -f '%m %N' <file>` prints the modification time an artifact proof compares
- `mktemp -d <dir>/scratch-XXXXXX` makes a private directory under a tree other sessions write

## [03]-[MISE]

- `mise which <name>` takes one name per call, a second positional prints `error: unexpected argument`
- `mise which <package binary>` prints `mise ERROR <name> is not a mise bin`, `command -v <name>` resolves the binary
- `mise ls --current` prints the `global.json` SDK as `dotnet <version> (symlink)`, `mise which dotnet` prints the machine SDK
- `mise env -s bash` is the environment the `SessionStart` hook writes
- `mise env --json-extended | jq -r '.[].source'` names each row's file
- `mise doctor` prints the `config_files:` list
- `file $(mise which <binary>)` proves the architecture of a `ubi:<owner>/<repo>` binary

## [04]-[NX]

- `pnpm exec nx run rasm:<target> -- <path>...` scopes a root target
- Unqualified `nx` is ungranted
- `pnpm exec nx affected -t <target> --files=<path>[,<path>]` on a root file affects the tree
- `pnpm exec nx show projects --affected --files=<paths> --json` names the owners, prints `[]` on a committed tree without `--files`
- `pnpm exec nx show project <project> --json | jq '.targets|keys'` lists targets, `jq '.targets.<target>.inputs'` the hashed inputs
- `pnpm exec nx run rasm:lint <dir>` hashes the tree in `inputs`, a planted defect misses the cache
- `pnpm exec nx run rasm:outline -- <path> --items structure` maps a file, `--view names`, `signatures`, `expanded`, or `digest` set depth
- `--match <Name>` selects one member, `-l <lang>` outlines a directory of one language, `--json=compact` prints one array per file
- `--color never` fails the outline target's schema
- `NO_COLOR=1` sits in `.claude/settings.json` `env` for every tool subprocess
- Outline prints `nothing found` for a TOML file or a `.slnx`, maps `package.json`, a workflow, and a `.props`, whole read serves the rest
- `pnpm exec nx run rasm:coverage -- --language <lang>` prints `merged files=N language=<lang>`, with or without `--`
- `pnpm exec nx run rasm:workflow -- --list` lists jobs, `-- --job=<job>` runs past 600000 ms under `run_in_background: true`
- `TaskStop` on the job run leaves `act-*` containers, `docker ps -a --filter name=act- -q` with `docker rm --force --volumes` clears them
- `pnpm exec nx run rasm:harness` reinstalls the plugin under the running session
- `pnpm exec nx run rasm:rules` runs `ast-grep test --include-off`
- `pnpm exec nx graph --file=<path>.json` writes the graph, `ProjectReference` edges sit beside the plugin's static edges
- `pnpm exec nx run rasm:format <files>` then `git diff --exit-code` proves layout on a committed tree

## [05]-[PACKAGE_MANAGERS]

- `pnpm exec biome rage --linter` prints enabled rules, `pnpm exec biome explain <rule>` one rule
- `pnpm why <pkg>` prints the installed version
- `pnpm exec biome check --error-on-warnings <file>` scans the tree until `files.includes` excludes the caches, `--stdin-file-path` checks text
- `pnpm exec tsc --showConfig -p <tsconfig>` prints effective options, `pnpm exec tsc --strict --noEmit --ignoreConfig <case>` proves a case
- `node --test <file>` runs one test file
- `pnpm exec nx run function-hooks:check` runs `lint` and `test`
- `uv sync --locked` checks every group under `default-groups = "all"`
- `uv lock --check` proves the lock
- `uv tree --frozen --package <name> --depth 1` prints a dependency, `--invert` its consumers, `--only-group <g>` one group
- `uv run --only-group eng python -m eng.scripts.<module> --help` prints a repository script's parameters
- `uv run pytest --co -q -p no:cov` drops a plugin by entry point name (`pytest_cov`) or suffix, the distribution name matches nothing
- `pytest -p no:<plugin>` under `required_plugins` answers `Missing required plugins` at exit 4, the drop proves the name alone
- `uv run ruff check --select ALL --isolated --preview <file>` reads the full rule set
- `UV_PROJECT_ENVIRONMENT=<dir>` makes a private venv

## [06]-[DOTNET]

- `dotnet msbuild <project> -getProperty:<A>,<B>` evaluates one project, `Workspace.slnx` answers `MSB1063`
- `dotnet msbuild Directory.Build.props -getProperty:ArtifactsPath` evaluates the root value without a project, `eng/native/` evaluates its own
- `ArtifactsPath` prints no trailing separator, `<logs>` is `$(...)/binlog/` with the slash written
- `dotnet msbuild <project> -getItem:PackageReference` lists every reference with `DefiningProjectFullPath`, `get_nuget_dependencies` the project's own
- `fd -e csproj . <scope> -x dotnet msbuild {} -getItem:PackageReference` evaluates a scope in one call, `PackageVersion` reads once
- `dotnet msbuild <file> -getProperty:MSBuildProjectFile` is the parse check, a broken file answers `MSB4025`
- `dotnet build <build> --no-restore -t:Rebuild -check -bl:<logs>/<purpose>-{}.binlog` names each capture uniquely, a fixed name collides across sessions
- `MSBuildTreatWarningsAsErrors=true` prints a `BC` report as `error BC0106`, fails the build, and lists it in `binlog_errors`
- `dotnet sln Workspace.slnx list` decides `<build>`, a project the list lacks takes its own `-check` build under its own `<logs>`
- `dotnet restore <slnx> --artifacts-path <scratch>` then two builds under it isolate a no-change pair from other sessions
- `-p:LangVersion=7.3` produces a failing build without a repository edit
- `Csc` execution count in `binlog_expensive_tasks` or `stat` on the assembly proves a compile, a duration proves nothing

## [07]-[AST_GREP]

- `ast-grep scan <missing>` prints `ERROR: <path>: No such file or directory` at exit 0, `ast-grep run -p '<p>' -l <lang> <missing>` exits 1
- `printf '%s' '<code>' | ast-grep scan --inline-rules '<yaml>' --json --stdin; echo $?` prints `1` on an `error` match, `0` with `[]` on a miss, `8` rejected
- `--stdin` takes one rule and refuses `--filter`
- Rules below `severity: error` print `help[<id>]` at exit 0 on a match
- `ast-grep scan --inspect entity <file> 2>&1 >/dev/null | rg '\|<id>:'` proves a rule registered, `--inspect summary` prints the file count
- `ast-grep test --include-off` runs every case, `--filter '^<id>$'` serves `-U` alone after `rm` of the snapshot
- `ast-grep test -U` keeps an orphan key for a changed case
- `ast-grep test` prints `Rule not found` at exit 3 for an absent id
- `ast-grep scan --no-ignore hidden <scope>` reaches hidden trees
- Duplicate ids under `sgconfig.yml` exit 8
- `ast-grep run -p '<old>' -r '<new>' -l tsx -U <dir>` rewrites from the root, the run changes nothing outside it
- `ast-grep scan --filter '^(<ids>)$' --json=stream . | jq -r .ruleId | sort | uniq -c` counts hits per rule in one scan
- `ast-grep outline --help` lists `-l`, `--items`, `--view`, `--match`, `--pub-members`, `--no-ignore`, and `--color`
- `mcp__ast-grep__find_code` with `pattern` the identifier finds declarations with references, a call pattern finds calls alone
- Syntax tools read no yaml fixture or markdown, `rg -l -F` lists fixture and markdown files
- `mcp__ast-grep__find_code_by_rule` with `output_format: text` and `max_results` prints `showing first N of M`, the JSON form goes to a file
- `hyperfine -N -r 8 '<scan>'` measures one rule
- `difft --display inline <a> <b>` diffs two sorted maps with no escape byte under a pipe

## [08]-[SCRIPTS]

- `.claude/skills/ast-grep/scripts/rule-checks.sh <pairing|width|arms|parse|measure>` runs by its path, an interpreter prefix is refused
- `rule-checks.sh measure <ext> <path>...` reads `ts` or `py` from its `elements` table and exits 1 on another, `arms` and `parse` run per language
- `uv run --only-group eng python -m eng.scripts.<coverage|mutation|workflow|harness|provision|publish|stage>` runs a repository script

## [09]-[CLAUDE]

- `claude plugin validate <dir>` prints `Validation passed` for an agents directory, `claude plugin validate <plugin>` counts hooks and `$` calls
- `claude --plugin-dir <plugin> --debug-file <path> -p '<prompt>' --output-format stream-json --verbose > <transcript>` proves a hook
- `claude --settings '{"pluginConfigs":{"<plugin>":{"options":{...}}}}'` sets a plugin option for one run
- `expect -c 'spawn claude ...; expect -re {<marker>}; send ...'` drives an interactive session, a rendered marker precedes each `send`
- Piped input into `script -q` is lost
- `claude --help` lists `--debug-file` and `--plugin-dir` and no `--max-turns`
- `-p` transcripts hold no hook context line, the `result` event restates it
- `jq 'select(.type=="attachment")'` reads hook context lines from the JSONL
- `Preloaded skill '<name>'` lines in the debug file prove the `skills` entries
- `agent.spawn settled` in the debug file prints the spawn

## [10]-[OTHER]

- `gh repo view --json nameWithOwner -q .nameWithOwner` prints the owner pair
- `mcp__github__actions_list` with `method`, `resource_id`, and `perPage` lists runs and jobs, then `get_job_logs` with `job_id` and `tail_lines`
- `mcp__github__get_file_contents` with `ref: refs/tags/<tag>` reads an action's manifest at its tag
- `mcp__nuget__get_latest_package_version` with `includePrerelease: true` and `solutionDirectory` prints the newest release
- `typos --force-exclude <path>` exits 2 on a hit and honors `[tool.typos.files] extend-exclude` on an explicit path
- `typos --diff` prints the proposed spelling
- `act --bug-report` prints the container architecture and the socket
- `docker volume ls --filter name=act- -q` lists the cache volume
