---
name: manage-repo-rules
description: "Use when writing, placing, or organizing a .claude/rules/ file, covering placement against CLAUDE.md, checks, and skills."
user-invocable: false
paths:
  - ".claude/rules/**"
---

# [MANAGE_REPO_RULES]

A rule holds one concern under one `paths` set in 1 to 25 body lines, under 10 as the target, and a rule that holds two concerns splits.

Use the guidance skill for the parts table, the landing moves, and the cleaning pass.

| [INDEX] | [OWNER]     | [FIELD]                                          | [EFFECT]                                                              |
| :-----: | :---------- | :----------------------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | Rule        | `paths`                                          | YAML list of globs, the one rule field                                |
|  [02]   | Skill       | `paths`                                          | Same glob format, loads the skill on a matching read in main          |
|  [03]   | Agent       | `memory`                                         | `user`, `project`, or `local`, the subagent's own auto memory         |
|  [04]   | Agent       | none                                             | `Explore` and `Plan` skip `CLAUDE.md` and every rule, by no field     |
|  [05]   | Hook        | `InstructionsLoaded`                             | `file_path`, `memory_type`, `load_reason`, `trigger_file_path`        |
|  [06]   | Settings    | `claudeMdExcludes`                               | Absolute-path globs that skip a rule or its symlink target, any layer |
|  [07]   | Settings    | `autoMemoryDirectory`, `autoMemoryEnabled`       | Auto memory location and toggle                                       |
|  [08]   | Memory      | `type`, `modified`                               | `user`, `feedback`, `project`, or `reference`, and the write stamp    |
|  [09]   | Environment | `CLAUDE_CODE_ADDITIONAL_DIRECTORIES_CLAUDE_MD=1` | Loads `CLAUDE.md` and `.claude/rules/*.md` from each `--add-dir`      |
|  [10]   | Environment | `CLAUDE_CODE_DISABLE_AUTO_MEMORY=1`              | Turns auto memory off                                                 |
|  [11]   | CLI         | `--setting-sources` without `project`            | Skips project rules                                                   |

## [01]-[PLACEMENT]

Each kind of fact has one owner, and a rule holds what no other owner takes:

| [INDEX] | [FACT]                                                   | [OWNER]                                                               |
| :-----: | :------------------------------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | Fact with no path set                                    | `CLAUDE.md` line                                                      |
|  [02]   | Fact a linter, analyzer, `ast-grep` rule, or hook checks | That check, and the rule states nothing a check enforces              |
|  [03]   | Procedure a skill owns                                   | That skill, and the rule points once with `Use <skill> for <purpose>` |
|  [04]   | Route to a skill keyed by a file kind                    | Rule with `paths` over that kind                                      |
|  [05]   | Route to a skill with no file key                        | `CLAUDE.md` line                                                      |

A fact that fits two rules goes to the rule with the narrower `paths`.

## [02]-[LOADING]

The harness discovers every `.md` under `.claude/rules/` recursively, symlinked files and directories included, and loads each by its frontmatter:
- Rules with `paths` load once per agent on the first matching read, in the main session, a subagent, and a fork
- Rules without `paths` load at launch beside `CLAUDE.md` in every session, and a rule without `paths` is a finding
- Rules under `~/.claude/rules/` load before project rules, and project rules take priority
- After compaction the harness reloads the rules for up to five of the files read or edited most recently
- Globs take `**`, brace expansion, and `\[` for a literal bracket, and no negation
- One rule's `paths` list expands to at most 1,000 patterns
- `load_reason` is `session_start`, `nested_traversal`, `path_glob_match`, `include`, or `compact`, and `path_glob_match` proves a rule loaded

## [03]-[ORGANIZATION]

Level 1 of `.claude/rules/` is one of four kinds, named in the repository's own tokens, and no name is invented:

```text
.claude/rules/
├── languages/<language>/          # Rules for every file of one language, paths **/*.<ext> and the language manifests
│   ├── <concern>.md               # One concern of the language under one paths set
│   └── <category>/<concern>.md    # One category of the language's concerns (code, testing, documentation)
├── <directory>/<concern>.md       # One directory of the repository layout, nested as deep as the paths go
├── general/<concern>.md           # Behavioral guidance bound to no language and no directory, any paths set
└── <root-file>.md                 # One root file of the repository, paths that file alone
```

- Every language sits under `languages/`, and a layout directory or `general/` is a directory from its first rule
- A dot-prefixed layout directory takes its name without the dot, `github/` for `.github/`
- A level-1 file binds one root file and takes its name
- Files under a directory are named for their concern in the tool's or the field's own word
- A language gains a `<category>/` level when its files hold a second category of concern, and the category binds by the language's own notation
- A `languages/<language>/testing/` rule binds every test of the language by its test notation, and a `tests/` rule binds that tree alone
- One file holds one concern under one `paths` set, and the `paths` are exactly the files the facts hold under, never wider
- A file splits when it passes 25 body lines or when two of its facts hold under different path sets, and a move renames nothing a `paths` glob binds
- A rule retires when its `paths` match no file on disk, when `CLAUDE.md` or a skill states its facts, or when a check enforces them

## [04]-[SHAPE]

A rule file holds the `paths` list as its whole frontmatter and a body stating what holds under those paths:
- The body opens with one sentence stating what holds under the `paths`, then entries
- Each entry states the category and the criterion an agent applies to the next case
- A procedure a skill owns appears as one `Use <skill> for <purpose>` line
- A body holds no heading
- No example runs longer than one line

## [05]-[ANTI_PATTERNS]

Smells a rule file shows sit beside their correct form:

| [INDEX] | [SMELL]                                                 | [CORRECT_FORM]                        |
| :-----: | :------------------------------------------------------ | :------------------------------------ |
|  [01]   | `globs`, `alwaysApply`, `applyTo`, or `description` key | `paths` list alone                    |
|  [02]   | README or index of the rules                            | Nothing, the frontmatter is the index |

## [06]-[CHECKS]

The check over the file and the loading probe over its `paths` prove a new rule:
- `.claude/skills/guidance/scripts/check.sh <file>` prints nothing over a rule
- An `InstructionsLoaded` hook runs `jq -r '[.load_reason, .file_path, .trigger_file_path] | join(" | ")' >> events.log`
- `claude -p --model haiku --max-turns 8 --allowedTools Read -- 'Read <file>. Then reply Done.'`, and `rg -c path_glob_match events.log` prints 1
