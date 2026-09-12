# [SQLITE]

Facts of SQLite, its shell, and DuckDB over the sink, each the principle that decides a script's or view's form, one section per documentation page at sqlite.org.

## [01]-[GENERATED_COLUMNS]

Page `gencol.html`, stored generated columns hold the normalization and the hashes:

| [INDEX] | [FACT]                                                         | [DECIDES]                                                          |
| :-----: | :------------------------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | Stored column computes at write and reads as a column          | `ntext` and `text_hash` on `finding` and `site`, spelled once each |
|  [02]   | Expression reads other generated columns short of a cycle      | `text_hash` over `ntext`, `finding_id` over `text_hash`            |
|  [03]   | Expression takes deterministic scalar functions alone          | `sha3` and `replace` qualify, a file body stays a runtime column   |
|  [04]   | `table_info` omits generated columns, `table_xinfo` lists them | Rebuild copies `table_info` columns, generated ones recompute      |
|  [05]   | Stored column cannot join by `alter table add column`          | Declaration change rebuilds the table through the delta            |
|  [06]   | Column datatype applies affinity to the expression's result    | `text` on every hash and normalized column                         |

## [02]-[UPSERT]

Page `lang_upsert.html`, repeat inserts and the bar row:

| [INDEX] | [FACT]                                                 | [DECIDES]                                                                |
| :-----: | :----------------------------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `on` after `insert ... select` parses as a join clause | `where` before `on conflict`, `where true` where no predicate exists     |
|  [02]   | Upsert fires on a uniqueness constraint alone          | `finding_id` `unique` drives `do nothing`, a check or key failure aborts |
|  [03]   | `excluded.` names the value the insert carried         | `bar.sql` `do update set earns = excluded.earns`                         |

## [03]-[RETURNING]

Page `lang_returning.html`, batch and lifecycle outputs:

| [INDEX] | [FACT]                                                          | [DECIDES]                                                        |
| :-----: | :-------------------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | Rows come in arbitrary order                                    | Readers match ids, never positions                               |
|  [02]   | Output serves no subquery or CTE                                | `insert.sql` transition joins `site` again, not the first output |
|  [03]   | Rows are the ones the statement wrote, `do nothing` writes none | First `insert.sql` array lists sites new to the table            |

## [04]-[WITH]

Page `lang_with.html`, span location and file reads:

| [INDEX] | [FACT]                                                         | [DECIDES]                                                        |
| :-----: | :------------------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | Recursive CTE runs the recursive select per queued row         | `walk` steps through occurrences bounded by `occurrence`         |
|  [02]   | `union` keeps every earlier row to dedupe, `union all` streams | `union all` in `walk`, no walk over predecessors since ids stay  |
|  [03]   | `with` clauses sit beside each other at one depth              | `command`, `renamed`, and `destination` in place of nested froms |

## [05]-[EXPRESSIONS]

Page `lang_expr.html`, operators and subqueries:

| [INDEX] | [FACT]                                                          | [DECIDES]                                                         |
| :-----: | :-------------------------------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | `\|\|`, `->`, and `->>` share one precedence and associate left | Parentheses around `value ->> '$.code'` beside a concatenation    |
|  [02]   | Correlated subquery re-evaluates per outer row                  | Anti-join in `unjudged_edits`, temp tables in `lifecycle.sql`     |
|  [03]   | `not in` answers null when the subquery holds a null            | `not in` over `finding_id` alone, a not-null column               |
|  [04]   | `is` compares null-safe                                         | `d.agent_id is o.agent_id` in `agent_digest`                      |
|  [05]   | `''` inside a literal spells one quote                          | Bound text holding `'` goes as `"'<text>'"` with each `'` doubled |
|  [06]   | `iif(x, y, z)` equals `case when x then y else z end`           | `iif` where a case has one arm and an else                        |

## [06]-[JSON]

Page `json1.html`, payload reads and file slicing:

| [INDEX] | [FACT]                                                 | [DECIDES]                                                           |
| :-----: | :----------------------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | `->>` answers an SQL value, `->` JSON text             | `->>` in mapping and lifecycle scripts, `strict` converts digits    |
|  [02]   | `json_each` walks an array or object as rows           | Patch lines, `tool_calls`, `delivered_on`, `:sites`, `:ids`         |
|  [03]   | Blob that reads as JSON text is accepted as JSON       | `cast(readfile(:out) as text)` states the intent before `json_each` |
|  [04]   | `json_quote` escapes text, `\n` and `\\` in the result | `line.sql` splits a file into one array element per line            |
|  [05]   | `json_group_array(distinct x)` aggregates one array    | `delivered_on` and `reported_on`                                    |

## [07]-[AGGREGATES]

Page `lang_aggfunc.html`, counts and last rows:

| [INDEX] | [FACT]                                                       | [DECIDES]                                                      |
| :-----: | :----------------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `count(1) filter (where x)` counts matches in the one pass   | Per-event counts in `session_audit` and `agent_digest`         |
|  [02]   | `total()` answers 0.0 over no rows, `sum()` null             | `total(v.earns = 0) = 0` in `recurring_categories`             |
|  [03]   | Bare columns beside one `max()` come from the row holding it | `insert.sql` reads the ids of a file's last `edited_files` row |

## [08]-[WINDOWS]

Page `windowfunctions.html`, positions, ranks, and neighbors:

| [INDEX] | [FACT]                                                                     | [DECIDES]                                           |
| :-----: | :------------------------------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `sum() over (order by x rows between unbounded preceding and 1 preceding)` | Line start offsets in `line.sql`                    |
|  [02]   | `row_number() over (partition by ... order by ...)`                        | One finding per site in the `insert.sql` transition |
|  [03]   | `max() over (partition by ... order by ...)` is a running maximum          | `process_ts` in `turn_cost` and `session_audit`     |
|  [04]   | `lag()` and `lead()` read the neighbor row of the partition                | `previous_usd` and `next_ts` in `turn_cost`         |
|  [05]   | One window pass reads the partition once                                   | Replaces a correlated subquery over the same rows   |

## [09]-[SHELL]

Page `cli.html`, the `sqlite3` process every reader and writer runs:

| [INDEX] | [FACT]                                                          | [DECIDES]                                                              |
| :-----: | :-------------------------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `readfile`, `writefile`, and `sha3` are shell functions         | Hashing writes through the shell, `views.test.ts` throws in its `sha3` |
|  [02]   | `readfile` is null for a missing path and raises on a directory | `subject_hash` `''` for a gone file, `cast` to text before `instr`     |
|  [03]   | `-bail` stops at the first error                                | Writers run under `-bail`, a failed statement never reaches `commit`   |
|  [04]   | `.timeout` waits on a locked database                           | `begin immediate` waits ten seconds before failing                     |
|  [05]   | `-json` prints one array per select with rows, nothing for none | Readers read an absent array as zero rows                              |
|  [06]   | `.mode tabs` writes raw tabs and newlines inside values         | State script cells hold counts and flags alone                         |
|  [07]   | `.output <file>` and `.read <file>` redirect and replay         | Open writes the delta file, scripts share files through `.read`        |
|  [08]   | `.read <file>` as the argument runs after every `-cmd`          | Script commands bind through `-cmd`, then name the script              |
|  [09]   | `.parameter set` evaluates SQL, unparsable text binds as text   | `'<text>'` binds text, a bare number an integer, `null` null           |
|  [10]   | `.parameter set` inside a script binds the statements after it  | `batch.sql` and each checker script bind `:state` and `:by`            |
|  [11]   | Unbound parameter reads null                                    | Missing `:state` or `:by` fails its `not null` column                  |

## [10]-[FOREIGN_KEYS]

Page `foreignkeys.html`, the transition's reference to its finding:

| [INDEX] | [FACT]                                                        | [DECIDES]                                                 |
| :-----: | :------------------------------------------------------------ | :-------------------------------------------------------- |
|  [01]   | Enforcement is off per connection until `pragma foreign_keys` | Every writer script opens with the pragma                 |
|  [02]   | Pragma inside a transaction changes nothing                   | Pragma before `begin`, an orphan `finding_id` writes at 0 |

## [11]-[STRICT]

Page `stricttables.html`, every finding table:

| [INDEX] | [FACT]                                                        | [DECIDES]                                                        |
| :-----: | :------------------------------------------------------------ | :--------------------------------------------------------------- |
|  [01]   | Value coerces losslessly to the declared type or raises       | `->>` digits fill integer columns, an integer fills text as text |
|  [02]   | Generated columns, checks, and foreign keys work as elsewhere | `strict` on every finding table changes no constraint            |

## [12]-[WAL]

Page `wal.html`, the sink's journal:

| [INDEX] | [FACT]                                   | [DECIDES]                                                  |
| :-----: | :--------------------------------------- | :--------------------------------------------------------- |
|  [01]   | `journal_mode=wal` persists in the file  | Open switches once, later opens read `wal`                 |
|  [02]   | One writer at a time, readers block none | Writers wait under `.timeout`, readers wait on nothing     |
|  [03]   | Every process shares one host            | Sink sits under the main worktree, never on a network path |

## [13]-[OPTIMIZER]

Pages `optoverview.html` and `queryplanner.html`, view bodies and indexes:

| [INDEX] | [FACT]                                                          | [DECIDES]                                                           |
| :-----: | :-------------------------------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | Subquery in `from` flattens into the outer query                | Views join views, a correlated subquery over a view never flattens  |
|  [02]   | Index serves a prefix of its columns with `=` then one range    | `(event, tool, ts)` serves `event = x and tool in (...) and ts > y` |
|  [03]   | Two indexes where one is a prefix of the other waste one        | No index on `event` alone beside `(event, tool, ts)`                |
|  [04]   | `like` with a literal prefix uses an index, a leading `%` scans | `like '%git mv %'` over Bash rows reads every row of its ts range   |
|  [05]   | Correlated `not exists` on an unindexed column scans per row    | `tool_use_id` index serves `denials` and the lifecycle's `edit`     |

## [14]-[DUCKDB]

DuckDB over the sink through `attach ... (type sqlite, read_only)`, measured on this machine:

| [INDEX] | [FACT]                                                            | [DECIDES]                                                            |
| :-----: | :---------------------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `s.<table>` omits generated columns and types by affinity         | `finding_id`, `text_hash`, `lineage_key` read through `sqlite_query` |
|  [02]   | `sqlite_query` types every column `VARCHAR`                       | `cast` under an aggregate                                            |
|  [03]   | `s.<view>` re-parses the body in DuckDB's dialect                 | Views read through `sqlite_query`, DuckDB joins its own data alone   |
|  [04]   | No busy timeout, a write against a held lock fails at once        | DuckDB reads alone, every write goes through `sqlite3`               |
|  [05]   | First `attach (type sqlite)` after an upgrade loads the extension | Network at that first attach                                         |
|  [06]   | `getvariable` serves a table function, `attach` takes a literal   | `transcript.sql` reads a variable, the sink attaches through `-cmd`  |
|  [07]   | `-cmd` runs before `-f`, `-c` exits before stdin                  | Script commands take `-cmd` for the binding and `-f` for the file    |
|  [08]   | Glob skips a deleted transcript, a name raises `IO Error`         | `agent-transcripts.sql` reads the subagent glob                      |
