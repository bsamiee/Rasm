# [TOOL_FORMS]

Forms and facts per tool an agent step names, run from repository root.

Use `use-ast-grep` for ast-grep forms and exits

## [01]-[GIT]

- `git diff --name-only --diff-filter=ACMR <commit>` lists changed files on disk, `git ls-files --others --exclude-standard` adds untracked ones
- `git diff --name-only <commit>` lists a deleted path
- `git diff --numstat <commit> -- <scope>` prints `added deleted path` whole
- `git diff --name-only <commit> -- . ':(exclude)<path>'` lists the complement of a scope in one command with every exclude
- `git diff --exit-code` fails on the run's own edits, a writer's check form over the run's files reports what it rewrites
- `git log -p --follow -- <file>` holds facts a rewrite dropped across a rename
- `git rev-parse --show-toplevel` is the absolute root a tool outside the shell resolves against
- `git rev-parse HEAD` is the default commit
- `git show HEAD:<file> > <file>` restores a file

## [02]-[FILES]

- `fd` refuses a file as search path with `Search path '<file>' is not a directory`
- `fd --strip-cwd-prefix` refuses a positional path
- `fd -e yml . <dir> -x basename {} .yml | paste -sd'|' -` derives an id alternation from file names
- `rg -n -F '<text>'` exits 1 on an absent spelling, `rg -l -F` lists files for `sd`
- `rg` refuses look-around without `--pcre2`, `--pcre2` refuses an unknown escape (`\y`)
- `sd -F '<old>' '<new>' <files>` rewrites a fixed string line by line, a find spanning lines takes `-A`, without it `sd` matches nothing and exits 0
- `sd` takes `--` before a find or replacement opening with `-`, a `$1` capture reads as `${1}` before a word character
- `jq` and `yq` refuse a JSON file with `//` comments, `Read` reads it
- `stat -f '%m %N' <file>` prints the modification time
- `mktemp -d <dir>/tmp.XXXXXX` makes a private directory under a tree other sessions write

## [03]-[MISE]

- `mise which <name>` takes one name per call, a second positional prints `error: unexpected argument`
- `mise which <package binary>` prints `mise ERROR <name> is not a mise bin`, `command -v <name>` resolves the binary
- `mise ls --current` prints the `global.json` SDK as `dotnet <version> (symlink)`
- `mise env --json-extended | jq -r '.[].source'` names each row's file
- `mise doctor` prints the `config_files:` list
- `file $(mise which <binary>)` prints a binary's architecture

## [04]-[NX]

- `nx show project <project> --json | jq '.targets|keys'` lists targets, `jq '.targets.<target>.inputs'` the hashed inputs

## [05]-[PACKAGE_MANAGERS]

- `pytest -p no:<plugin>` under `required_plugins` answers `Missing required plugins` at exit 4
- `ruff check --select ALL --isolated --preview <file>` reads the full rule set
- `biome check --error-on-warnings`, `ruff check`, and `ruff format --check` over files are the writers' check forms, a missing path exits 1 or 2

## [06]-[DOTNET]

Use `dotnet-msbuild-diagnostics` for binlog capture

- `dotnet msbuild <project> -getProperty:<A>,<B>` evaluates one project, a solution path answers `MSB1063`
- `dotnet msbuild <project> -getItem:PackageReference` lists every reference with `DefiningProjectFullPath`
- `fd -e csproj . <scope> -x dotnet msbuild {} -getItem:PackageReference` evaluates a scope in one call
- `dotnet msbuild <file> -getProperty:MSBuildProjectFile` is the parse check, a broken file answers `MSB4025`
- `nx run rasm:lint:dotnet-format` is the writer's solution-wide check form

## [07]-[CLAUDE]

- `claude plugin validate <dir>` prints `Validation passed` for an agents directory or a plugin

## [08]-[OTHER]

- `difft --display inline <a> <b>` diffs files with no escape byte under a pipe
- `gh repo view --json nameWithOwner -q .nameWithOwner` prints the owner pair
- `mcp__github__actions_list` with `method`, `resource_id`, and `perPage` lists runs and jobs
- `mcp__github__get_job_logs` with a listed `job_id` and `tail_lines` prints the job's log tail
- `mcp__nuget__get_latest_package_version` with `includePrerelease: true` and an absolute `solutionDirectory` prints the newest release
- `yamlfmt -lint <files>` prints a diff at exit 1, a missing path is silent at exit 0
