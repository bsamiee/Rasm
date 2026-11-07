# [HK]

## [01]-[HOOKS]

- `hk check` runs top-level steps and `hooks.check` steps, `hk fix` runs top-level steps alone
- Steps with a `check` and no `fix` run their check under `hk fix`, a checker kept out of fix runs sits in `hooks.check`
- `hk check --fix` runs each step's `fix`, `fix = null` keeps a writer another entry owns out of that run
- Top-level `steps` turn on a `pre-commit` hook stashing with Git, `enabled = false` keeps `hk install` from installing it
- `hk install` installs each enabled hook except `check` and `fix`, as `hook.hk-<event>.command` Git config rows where Git reads hooks from config
- `hk install --mise` runs hk through `mise x`, and `HK=0` makes the installed launcher skip its hook

## [02]-[SELECTION]

- `--all` refuses file arguments, `--glob='**/*'` selects every file and forwarded paths replace that selection
- `--step=<step>` runs the named steps alone, a step its `depends` names stays unrun
- `match_any` takes no `glob` or `types` beside it, a builtin declaring `match_any` takes `match_any = null` with its new `glob`
- `types` match shebangs and special file names, a hash input listing globs misses the files a type adds
- File walks skip what ignore files exclude unless `walk_ignore = false`

## [03]-[SCHEDULING]

- Steps of one hook run at once under per-file locks, writers sharing a file take turns and readers share
- `depends` orders steps inside one group, a dependency in a later group has no effect
- `exclusive = true` waits for earlier steps and starts no step until it ends, `--plan --json` splits its groups around that step
- `batch = true` splits a step's files across `--jobs` with at least `batch_min_files` per batch, batched steps running together share the jobs
- Batching speeds a single-threaded checker over many files, a tool using every core or paying a slow start per batch gains little
- `fail_fast` defaults to `true` and cancels pending steps at the first failure, `fail_fast = false` in configuration reports every step on every call

## [04]-[FIXES]

- `check_diff` prints a patch fix runs apply in place of `fix`, an exit 0 skips the fixer
- Patches `git apply` rejects send hk to the fixer after the check, so the tool runs twice
- Steps with `check_diff` and no `fix` write through the patch alone
- `hk util format-diff <files> -- <command>` builds a patch from a formatter reading stdin, `{}` in the command names each file
- `hk util sarif-diff -- <command>` builds a patch from the fixes in a SARIF report
- `check_after_diff = true` reruns `check` after the patch, for a fixer leaving findings it cannot fix
- `check_list_files` names the files needing a fix, `check_first = true` checks before fixing a file another step also writes

## [05]-[CONFIGURATION]

- Configuration amends one release's package URI, Pkl takes no interpolation in a module URI
- Each hk release preloads its own package alone, a URI naming another release downloads it
- `hk validate --deny-warnings` fails on unknown keys, steps with no command, `depends` naming an unknown or later step, and globs opening with `./`, `/`, or `!`

## [06]-[STATE]

- `HK_STATE_DIR` moves the run log, failed-command output, and stash patches together, without it the logs sit under `<git-dir>/hk/`
- `HK_LOG_FILE` and `HK_OUTPUT_FILE` each move one file and leave the other in the default folder
- `HK_CACHE_DIR` holds evaluated configurations and `HK_PKL_CACHE_DIR` downloaded packages, both default to the platform cache folder
- `HK_PKL_CACHE_DIR` reads the process environment alone, `hk.pkl` cannot set it
