# [TOOL_FORMS]

Forms and facts per tool an agent step names, each one proven by a run from the repository root, with the exit code or the line the run printed where a step reads it.

## [01]-[GIT]

- `git diff --name-only --diff-filter=ACMR <commit>` lists changed files on disk, and `git ls-files --others --exclude-standard` adds untracked ones
- `git diff --name-only <commit>` lists a deleted path, and `git status --porcelain | cut -c4-` lists `old -> new` for a rename
- `git diff --numstat <commit> -- <scope>` prints `added deleted path` whole, and `--stat` truncates a long path
- `git diff --name-only <commit> -- . ':(exclude)<path>'` lists the complement of a scope in one command with every exclude
- `git diff | shasum` before and after a formatter proves it rewrote nothing, and `git diff --exit-code` fails on the run's own edits
- `git log -p --follow -- <file>` holds the facts a rewrite dropped across a rename
- `git rev-parse --show-toplevel` is the absolute root a tool outside the shell resolves against, and `git rev-parse HEAD` the default commit
- `git show HEAD:<file> > <file>` restores a file, because the hook refuses `git checkout -- <path>`
- `git mv` renames a file, and `rg -n -F '<old path>'` at exit 1 proves every reference followed

## [02]-[FILES]

- `fd -e <ext> . <dir>` names a shape, `fd -H` includes hidden paths, `fd -I` ignored ones, and `fd -g '<id>.yml' <dir>` finds a rule's siblings
- `fd` refuses a file as its search path, `Search path '<file>' is not a directory`, and `--strip-cwd-prefix` refuses a positional path
- `fd -e csproj . <scope> -x <cmd> {}` runs one command per hit, and `-X <cmd>` runs one command over the batch
- `fd -e yml . <dir> -x basename {} .yml | paste -sd'|' -` derives an id alternation from the files that own it
- `fd -t f --changed-within <span>` lists every writer's files, and a marker file attributes nothing
- `rg -n -F '<text>'` proves an old spelling absent at exit 1, `rg -l -F` lists the files `sd` rewrites, and `rg -c` counts before and after
- `rg` refuses look-around without `--pcre2`, and `--pcre2` refuses an unknown escape (`\y`) with `unrecognized character follows \`
- `rg -nU --pcre2 -e '^[ \t]*(#(?!!)|//|<!--|/\*)' -e '^[ \t]*(message|note):' -e '"""[\s\S]*?"""' <files>` extracts comments and rule text
- `sd -F '<old>' '<new>' <files>` rewrites a fixed string, `--` precedes a replacement that opens with `-`, and `sd` has no `-s` flag
- `jq -r` prints a shell value, `[]?` reads an optional array, and `yq '.expr' <file>` is the one `yq` form
- `jq` and `yq` refuse `.vscode/settings.json`, because the file holds `//` comments, and `Read` reads it
- `yq -r '[.id, .message] | join(" | ")' <rules>/*.yml` maps a rule family, and `yq '.snapshots | map_values(.fixed)' <snapshot>` reads a snapshot
- `K='<case>' yq '.snapshots[strenv(K)]' <snapshot>` reads one case, and a snapshot read whole costs its every line
- `tree <dir>` lists a tree and `-D` its directories, and `loc <dir>` counts lines with a complexity score per file
- `stat -f '%m %N' <file>` prints the modification time an artifact proof compares
- `mktemp -d <dir>/scratch-XXXXXX` makes a private directory under a tree other sessions write

## [03]-[MISE]

- `mise which <name>` takes one name per call, and a second positional prints `error: unexpected argument`
- `mise which biome` prints `mise ERROR biome is not a mise bin`, and `command -v <name>` resolves a package binary
- `mise ls --current` prints `dotnet 10.0.400 (symlink)` from `global.json`, and `mise which dotnet` prints the machine SDK
- `mise env -s bash` is the environment the `SessionStart` hook writes, and `mise env --json-extended | jq -r '.[].source'` names each row's file
- `mise doctor` prints the `config_files:` list, and `mise exec -- <cmd>` is rewritten to `<cmd>`
- `file $(mise which <binary>)` proves the architecture of a `ubi:<owner>/<repo>` binary

## [04]-[NX]

- `pnpm exec nx run rasm:<target> -- <path>...` scopes a root target, the tree form runs without paths, and the bare `nx` spelling is ungranted
- `pnpm exec nx affected -t <target> --files=<path>[,<path>]` runs the owning projects with their dependents, and a root file affects the tree
- `pnpm exec nx show projects --affected --files=<paths> --json` names the owners, and prints `[]` on a committed tree without `--files`
- `pnpm exec nx show project <project> --json | jq '.targets|keys'` lists targets, and `jq '.targets.<target>.inputs'` the hashed inputs
- `nx run rasm:lint <dir>` misses the cache on a planted defect and hits on no change, because the `inputs` hash the tree
- `pnpm exec nx run-many -t <target> -p tag:language:<lang>` runs one language, and `check` replaces the `lint` and `typecheck` pair on the root
- `pnpm exec nx run rasm:outline -- <path> --items structure` maps a file, `--view names`, `signatures`, `expanded`, or `digest` set the depth
- `--match <Name>` selects one member, `-l <lang>` outlines a directory of one language, `--json=compact` prints one array per file
- `--color never` fails the outline target's schema, and the output holds the same bytes with and without `NO_COLOR=1`
- `NO_COLOR=1` is set in the `env` block of `.claude/settings.json` for every tool subprocess of a session, and the hook drops the prefix from a Bash call, because `function-hooks:test` printed 730 escape lines plain against 41 under it
- The outline prints `nothing found` for a TOML file or a `.slnx`, maps `package.json`, a workflow, and a `.props`, and the whole read serves those
- `pnpm exec nx run rasm:coverage -- --language <lang>` prints `merged files=N language=<lang>`, with or without the `--`
- `pnpm exec nx run rasm:workflow -- --list` lists the jobs, and `-- --job=<job>` runs past 600000 ms under `run_in_background: true`
- `TaskStop` on the job run leaves `act-*` containers, and `docker ps -a --filter name=act- -q` with `docker rm --force --volumes` clears them
- `pnpm exec nx run rasm:harness` reinstalls the plugin under the running session, and `rasm:rules` runs `ast-grep test --include-off`
- `pnpm exec nx run rasm:rewrite -- --filter='^<id>$' --error=<id> <path>` applies one rewrite rule
- `pnpm exec nx graph --file=<path>.json` writes the graph, and `ProjectReference` edges sit beside the plugin's static edges

## [05]-[PACKAGE_MANAGERS]

- `pnpm exec biome rage --linter` prints the enabled rules, `pnpm exec biome explain <rule>` one rule, and `pnpm why <pkg>` the installed version
- `pnpm exec biome check --error-on-warnings <file>` scans the tree until `files.includes` excludes the caches, and `--stdin-file-path` probes text
- `pnpm exec tsc --showConfig -p <tsconfig>` prints the effective options, and `pnpm exec tsc --strict --noEmit --ignoreConfig <case>` proves a case
- `node --test <file>` runs one test file, and `pnpm exec nx run function-hooks:check` runs `lint` and `test`
- `uv sync --locked` checks every group under `default-groups = "all"`, and `uv lock --check` proves the lock
- `uv tree --frozen --package <name> --depth 1` prints a dependency, `--invert` its consumers, and `--only-group <g>` one group
- `uv run --only-group eng python -m eng.scripts.<module> --help` prints a repository script's parameters
- `uv run pytest --co -q -p no:cov` drops a plugin by its entry point name (`pytest_cov`) or its suffix, and the distribution name matches nothing
- `pytest -p no:<plugin>` under `required_plugins` answers `Missing required plugins` at exit 4, and the drop proves the name alone
- `uv run ruff check --select ALL --isolated --preview <file>` reads the full rule set, and `UV_PROJECT_ENVIRONMENT=<dir>` makes a private venv

## [06]-[DOTNET]

- `dotnet msbuild <project> -getProperty:<A>,<B>` evaluates one project, and `Workspace.slnx` answers `MSB1063`
- `dotnet msbuild Directory.Build.props -getProperty:ArtifactsPath` evaluates the root value without a project, and `eng/native/` evaluates its own
- `ArtifactsPath` prints no trailing separator, and `<logs>` is `$(...)/binlog/` with the slash written
- `dotnet msbuild <project> -getItem:PackageReference` lists every reference with `DefiningProjectFullPath`, `get_nuget_dependencies` the project's own
- `fd -e csproj . <scope> -x dotnet msbuild {} -getItem:PackageReference` evaluates a scope in one call, and `PackageVersion` reads once
- `dotnet msbuild <file> -getProperty:MSBuildProjectFile` is the parse check, and a broken file answers `MSB4025`
- `dotnet build <build> --no-restore -t:Rebuild -check -bl:<logs>/<purpose>-{}.binlog` stamps each capture, and a fixed name collides across sessions
- `MSBuildTreatWarningsAsErrors=true` prints a `BC` report as `error BC0106`, fails the build, and lands it in `binlog_errors`
- `dotnet sln Workspace.slnx list` decides `<build>`, and a project the list lacks takes its own `-check` build under its own `<logs>`
- `dotnet restore <slnx> --artifacts-path <scratch>` then two builds under it isolate a no-change pair from other sessions
- `-p:LangVersion=7.3` produces a failing build without a repository edit, the symptom a debugger route proves on
- `Csc` execution count in `binlog_expensive_tasks` or `stat` on the assembly proves a compile, and a duration proves nothing
- `dotnet build` without `-bl` gets a context line, and `-tl:off` with `-v:q` are stage inputs the tool's configuration settles

## [07]-[AST_GREP]

- `ast-grep scan <missing>` prints `ERROR: <path>: No such file or directory` at exit 0, and `ast-grep run -p '<p>' -l <lang> <missing>` exits 1
- `printf '%s' '<code>' | ast-grep scan --inline-rules '<yaml>' --json --stdin; echo $?` prints `1` on an `error` match, `0` with `[]` on a miss, `8` rejected
- `--stdin` takes one rule and refuses `--filter`, and a rule below `severity: error` prints `help[<id>]` at exit 0 on a match
- `ast-grep scan --inspect entity <file> 2>&1 >/dev/null | rg '\|<id>:'` proves a rule registered, and `--inspect summary` prints the file count
- `ast-grep test --include-off` runs every case in 0.15 s, and `--filter '^<id>$'` serves `-U` alone after `rm` of the snapshot
- `ast-grep test -U` keeps an orphan key for a changed case, and `Rule not found` exits 3 for an absent id
- `ast-grep scan --no-ignore hidden <scope>` reaches the hidden trees, and a duplicate id under `sgconfig.yml` exits 8
- `ast-grep run -p '<old>' -r '<new>' -l tsx -U <dir>` rewrites from the root, and the run changes nothing outside it
- `ast-grep scan --filter '^(<ids>)$' --json=stream . | jq -r .ruleId | sort | uniq -c` counts hits per rule in one scan
- `ast-grep outline --help` lists `-l`, `--items`, `--view`, `--match`, `--pub-members`, `--no-ignore`, and `--color`
- `mcp__ast-grep__find_code` with `pattern` the identifier finds declarations with references, and a call shape finds calls alone
- `mcp__ast-grep__find_code` reads no yaml fixture or markdown, and `rg -l -F` lists the fixture and markdown files
- `mcp__ast-grep__find_code_by_rule` with `output_format: text` and `max_results` prints `showing first N of M`, and the JSON form lands in a file
- `hyperfine -N -r 8 '<scan>'` measures one rule, and `difft --display inline <a> <b>` diffs two sorted maps with no escape byte under a pipe

## [08]-[SCRIPTS]

- `.claude/skills/ast-grep/scripts/rule-checks.sh <pairing|width|arms|parse|measure>` runs by its path, and an interpreter prefix is refused
- `rule-checks.sh measure <ext> <path>...` reads `ts` or `py` from its `elements` table, exits 1 on another, and `arms` and `parse` run per language
- `uv run --only-group eng python -m eng.scripts.<coverage|mutation|workflow|harness|provision|publish|stage>` runs a repository script
- `eng.scripts.workflow` removes its containers in a `finally` a `TaskStop` never reaches

## [09]-[CLAUDE]

- `claude plugin validate <dir>` prints `Validation passed` for an agents directory, and `claude plugin validate <plugin>` counts hooks and `$` calls
- `claude --plugin-dir <plugin> --debug-file <path> -p '<prompt>' --output-format stream-json --verbose > <transcript>` proves a hook
- `claude --settings '{"pluginConfigs":{"<plugin>":{"options":{...}}}}'` sets a plugin option for one run
- `expect -c 'spawn claude ...; expect -re {<marker>}; send ...'` drives an interactive session, and piped input into `script -q` is lost
- `claude --help` lists `--debug-file` and `--plugin-dir` and no `--max-turns`
- `-p` transcripts hold no hook context line, the `result` event restates it, and `jq 'select(.type=="attachment")'` reads them from the JSONL
- `Preloaded skill '<name>'` lines in the debug file prove the `skills` entries, and `agent.spawn settled` prints the spawn

## [10]-[OTHER]

- `gh repo view --json nameWithOwner -q .nameWithOwner` prints the owner pair, and remote work goes through the `github` MCP tools
- `mcp__github__actions_list` with `method`, `resource_id`, and `perPage` lists runs and jobs, then `get_job_logs` with `job_id` and `tail_lines`
- `mcp__github__get_file_contents` with `ref: refs/tags/<tag>` reads an action's manifest at its tag
- `mcp__nuget__get_latest_package_version` with `includePrerelease: true` and `solutionDirectory` prints the newest release
- `typos --force-exclude <path>` exits 2 on a hit and honors `[tool.typos.files] extend-exclude` on an explicit path, and `typos --diff` prints the proposed spelling
- `nx run rasm:lint <files>` runs every checker of the files' kinds, and `nx run rasm:format <files>` then `git diff --exit-code` proves their layout
- `act --bug-report` prints the container architecture and the socket, and `docker volume ls --filter name=act- -q` lists the cache volume
