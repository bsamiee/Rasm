# [TOOL_FORMS]

Forms and facts per tool an agent step names, each proven by a run from repository root.

## [01]-[GIT]

- `git diff --name-only --diff-filter=ACMR <commit>` lists changed files on disk, `git ls-files --others --exclude-standard` adds untracked ones
- `git diff --name-only <commit>` lists a deleted path
- `git diff --numstat <commit> -- <scope>` prints `added deleted path` whole
- `git diff --name-only <commit> -- . ':(exclude)<path>'` lists the complement of a scope in one command with every exclude
- `git diff --exit-code` fails on the run's own edits, a writer's check form over the run's files proves it rewrites nothing
- `git log -p --follow -- <file>` holds facts a rewrite dropped across a rename
- `git rev-parse --show-toplevel` is the absolute root a tool outside the shell resolves against
- `git rev-parse HEAD` is the default commit
- `git show HEAD:<file> > <file>` restores a file

## [02]-[FILES]

- `fd` refuses a file as search path with `Search path '<file>' is not a directory`
- `fd --strip-cwd-prefix` refuses a positional path
- `fd -e yml . <dir> -x basename {} .yml | paste -sd'|' -` derives an id alternation from file names
- `rg -n -F '<text>'` proves an old spelling absent at exit 1, `rg -l -F` lists files for `sd`, `rg -c` counts before and after a rewrite
- `rg` refuses look-around without `--pcre2`, `--pcre2` refuses an unknown escape (`\y`)
- `rg -nU --pcre2 -e '^[ \t]*(#(?!!)|//|<!--|/\*)' -e '^[ \t]*(message|note):' -e '"""[\s\S]*?"""' <files>` extracts comments and rule text
- `sd -F '<old>' '<new>' <files>` rewrites a fixed string, `sd` has no `-s` flag
- `sd` takes `--` before a replacement opening with `-`
- `jq` and `yq` refuse a JSON file with `//` comments, `Read` reads it
- `yq -r '[.id, .message] | join(" | ")' <rules>/*.yml` maps a rule family
- `stat -f '%m %N' <file>` prints the modification time
- `mktemp -d <dir>/scratch-XXXXXX` makes a private directory under a tree other sessions write

## [03]-[MISE]

- `mise which <name>` takes one name per call, a second positional prints `error: unexpected argument`
- `mise which <package binary>` prints `mise ERROR <name> is not a mise bin`, `command -v <name>` resolves the binary
- `mise ls --current` prints the `global.json` SDK as `dotnet <version> (symlink)`
- `mise env -s bash` is the environment the `SessionStart` hook writes
- `mise env --json-extended | jq -r '.[].source'` names each row's file
- `mise doctor` prints the `config_files:` list
- `file $(mise which <binary>)` proves the architecture of a binary

## [04]-[NX]

- `nx affected -t <target> --files=<path>[,<path>]` runs each file's project with its dependents, a plugin file of `nx.json` every project
- `nx show project <project> --json | jq '.targets|keys'` lists targets, `jq '.targets.<target>.inputs'` the hashed inputs
- `nx run rasm:outline -- <path> --items structure` maps a file, `--view names`, `signatures`, `expanded`, or `digest` set depth
- `--match <Name>` selects one member, `-l <lang>` outlines a directory of one language, `--json=compact` prints one array
- `--color never` fails the outline target's schema, `NO_COLOR=1` in the settings `env` reaches every tool subprocess
- Outline maps the bundled languages, markdown included, and the custom languages `sgconfig.yml` names, prints `nothing found` for the rest

## [05]-[PACKAGE_MANAGERS]

- `pytest -p no:<plugin>` under `required_plugins` answers `Missing required plugins` at exit 4
- `ruff check --select ALL --isolated --preview <file>` reads the full rule set
- `biome check --error-on-warnings`, `ruff check`, and `ruff format --check` over files are the writers' check forms, a missing path exits 1 or 2

## [06]-[DOTNET]

- `dotnet msbuild <project> -getProperty:<A>,<B>` evaluates one project, a solution path answers `MSB1063`
- `dotnet msbuild Directory.Build.props -getProperty:ArtifactsPath` evaluates the root value without a project
- `ArtifactsPath` prints no trailing separator, `<logs>` is `$(...)/binlog/` with the slash written
- `dotnet msbuild <project> -getItem:PackageReference` lists every reference with `DefiningProjectFullPath`
- `fd -e csproj . <scope> -x dotnet msbuild {} -getItem:PackageReference` evaluates a scope in one call
- `dotnet msbuild <file> -getProperty:MSBuildProjectFile` is the parse check, a broken file answers `MSB4025`
- `dotnet format <project> --no-restore --verify-no-changes --include <files>` is the writer's check form, a missing path prints nothing at exit 0
- `dotnet build <build> --no-restore -t:Rebuild -check -bl:<logs><purpose>-{}.binlog` names a capture, `{}` expands to date, time, pid, and a suffix
- `dotnet restore <solution> --artifacts-path <scratch>` then builds under it isolate a pair from other sessions
- `-p:LangVersion=7.3` produces a failing build without a repository edit
- `Csc` execution count in `binlog_expensive_tasks` proves a compile, a duration proves nothing

## [07]-[AST_GREP]

- `ast-grep scan <missing>` prints `ERROR: <path>: No such file or directory` at exit 0, `ast-grep run -p '<p>' -l <lang> <missing>` exits 1
- `printf '%s' '<code>' | ast-grep scan --inline-rules '<yaml>' --json --stdin` exits 1 on an `error` match, 0 with `[]` on a miss, 8 on a parse error
- `--stdin` reports every inline rule, `--filter` selects among them
- Rules below `severity: error` print `warning[<id>]` or `help[<id>]` at exit 0 on a match
- `ast-grep scan --inspect entity <file> 2>&1 >/dev/null | rg '\|<id>:'` proves a rule registered, `--inspect summary` prints `scannedFileCount`
- `ast-grep scan --no-ignore hidden <scope>` reaches hidden trees
- `ast-grep run -p '<old>' -r '<new>' -l tsx -U <dir>` rewrites under one directory
- `ast-grep scan --filter '^(<ids>)$' --json=stream . | jq -r .ruleId | sort | uniq -c` counts hits per rule in one scan
- `mcp__ast-grep__find_code_by_rule` with `output_format: text` and `max_results` prints `showing first N of M`, the JSON form goes to a file
- `hyperfine -N -r 8 '<scan>'` measures one rule
- `difft --display inline <a> <b>` diffs two files with no escape byte under a pipe

## [08]-[CLAUDE]

- `claude plugin validate <dir>` prints `Validation passed` for an agents directory or a plugin

## [09]-[OTHER]

- `gh repo view --json nameWithOwner -q .nameWithOwner` prints the owner pair
- `mcp__github__actions_list` with `method`, `resource_id`, and `perPage` lists runs and jobs, then `get_job_logs` with `job_id` and `tail_lines`
- `mcp__nuget__get_latest_package_version` with `includePrerelease: true` and an absolute `solutionDirectory` prints the newest release
- `yamlfmt -lint <files>` prints a diff at exit 1, a missing path is silent at exit 0
