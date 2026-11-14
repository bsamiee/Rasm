---
name: review-change
description: "Use when judging, challenging, or codifying a change to code, covering diff reading, hunk verdicts, owner routing per fact and checker, review roles and briefs, and the principles and checks a change leaves behind"
---

# [REVIEW_CHANGE]

Reviews judge each hunk of a change against its file whole, its consumers, package and host source at the pinned release, and every check the language's owners list, rebuild each wrong hunk in place, and leave each settled fact at the skill or checker owning it. Code, configuration, build, rule, and checker files take the same review. Use `refactor-code` for the rework, its removal and structure tables, and its adversary passes, `<language>-coding` skills for language form, `use-ast-grep` for rule shape and cost, `clean-prose` for guidance text.

Scope names how the roles of `[04]` run:

| [INDEX] | [SCOPE]  | [RUN]                                                                                                               |
| :-----: | :------- | :------------------------------------------------------------------------------------------------------------------ |
|  [01]   | File     | `refactor-code` adversary passes where the change had none, then one judge holding every `[04]` role in order       |
|  [02]   | Folder   | `refactor-code` folder scope: reader, editor, adversaries, and verdict per file group                               |
|  [03]   | Range    | `<base>..HEAD`: a reader per unit, a writer per checker and rule folder, per skill a writer and adversary           |
|  [04]   | Pass     | One reader per tool-call log in order, naming each wasted round and skipped check with the step catching it earlier |
|  [05]   | Guidance | Guidance adversary of `[04]` over `git diff HEAD -- <folder>` hunk by hunk with its passage, then its prose pass    |

## [01]-[READING]

Verdicts rest on the file whole, each consumer, and the checks at base, one drafted from a diff, a search over one file, or a pass's report is withdrawn. Each read runs its command:

| [INDEX] | [READ]         | [COMMAND]                                                                                                              |
| :-----: | :------------- | :--------------------------------------------------------------------------------------------------------------------- |
|  [01]   | Base and tree  | `git log -5 --stat`, `git status --short`, base is the named or last clean commit, the live tree is the subject        |
|  [02]   | Standards      | `CLAUDE.md` standards, the language skill, memory entries                                                              |
|  [03]   | Checks at base | Every checker `[03]` names (`nx run <project>:check`, `hk check <paths>`), before an edit                              |
|  [04]   | Rules and rows | Rule folders, utils, lint rows, and the `tools/` lint list of the language, before a stronger form is proposed         |
|  [05]   | File           | `ast-grep outline <path>` to choose, each file whole, consumers by `--items exports`, `rg -F '<name>'`, `ast-grep run` |
|  [06]   | Diff           | `GIT_EXTERNAL_DIFF=difft git diff <base> -- <paths>` for reshaped against moved code, `git diff --numstat <base>`      |
|  [07]   | Counts         | `loc` and `ast-grep outline` at `HEAD` and in the tree, the counts `refactor-code` reading lists                       |
|  [08]   | Cause          | `git log -S '<literal>' -- <path>` for each guard, flag, or form a hunk removes or keeps                               |
|  [09]   | Sources        | `search-code` at the installed release, generated files found by `fd <name> .cache/<tool>` before a read               |
|  [10]   | Entry points   | Each entry point `refactor-code` reading lists, run once on repository input at base and new                           |
|  [11]   | Input          | Repository files per entry point covering each site a hunk changes, run at base and new                                |
|  [12]   | Timing         | `hyperfine -r <runs> --metrics time_wall_clock,instructions '<command>'` at base and after, build profile stated       |
|  [13]   | Logs           | Each pass's tool-call log in order with its instruction verbatim, for a `Pass` scope                                   |

Reading conduct:
- Source claims quote lines read whole at the pinned tag, a grep for a flag list misses a stage before it (a substitution ahead of a parse)
- Saved research in the scratchpad is read by absolute path before any fetch, every path is absolute, shell state resets between calls
- Checks run through their Nx target, a raw tool call (`pnpm exec tsc`, `cargo clippy --package`) lacks the configuration the target holds
- Manifest edits take one unlocked build updating the lock before a locked check, build output sits where the tool's own setting states
- Substring edits (`sd`) are reread as the whole expression they sit in before a check runs

## [02]-[JUDGING]

Each hunk takes one verdict from what it removes or adds, named with the `CLAUDE.md` line, skill line, or check deciding it, and a hunk doing neither is a shuffle:

| [INDEX] | [VERDICT] | [MEANS]                                                                    | [ACTION]                                 |
| :-----: | :-------- | :------------------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | Right     | Shape removed, value derived, member called, family folded, pass cut       | Kept, principle landed through `[05]`    |
|  [02]   | Naive     | Right in shape, wrong in a detail a check, source, or skill line names     | Detail rebuilt in place, check recorded  |
|  [03]   | Shuffle   | `refactor-code` shuffle row, or a merge re-matching a narrowed value       | Undone in place or finished into removal |
|  [04]   | Wrong     | Other `refactor-code` row, `CLAUDE.md` or memory form, owner check finding | Rebuilt to the stronger form             |

Judges fix what they judge: each wrong, naive, and shuffle verdict is applied in place in the pass that reached it, a verdict list handed to a later pass is a deferred finding, and a verdict is settled by the read, a source, or one run, never asked. Criteria beyond the `refactor-code` tables:
- Count: a count cut by an erased error, an untyped read, a discarded message, a dropped case, or a permissive decode is a regression
- Trade: a hunk fixing one checker's finding by a form another owner bans (an alias dodging a limit) is naive, one stronger shape resolves both
- Credit: a regression at base belongs to base, one the change introduced to the change, both fixed in this pass
- Output: when the wire shape changed, base and new outputs compare through one normalizer, never through a gate on old output
- Logic: a defect found by reading is proven by one discriminating input on base and new, chosen from repository data, before its verdict
- Cost: a hunk with a linear-scan lookup, repeated walk or parse, unbounded recursion, per-element copy, or resource held past use is wrong
- Triage: mixed work is judged from the last clean commit, a hunk naming no fact or capability undone in place, no baseline is a target
- Suppression: each kept suppression's reason is traced to the row it names, a reason a catalog or manifest row makes false goes with it
- Consumer: a behavior change a checker intends reaches every consumer site in the same change, each rewritten site proven to still report
- API: reviewers write one consumer call per library hunk, a hunk taking a caller's choice or exposing more than its signature types is wrong

## [03]-[ROUTING]

Each fact a review settles has one owner, read before a verdict states it and edited by the review changing it:

| [INDEX] | [FACT]                                               | [OWNER]                                                                           |
| :-----: | :--------------------------------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | Level, option, threshold, or ban of an included rule | Language's checker configuration, its ban rows included                           |
|  [02]   | Form syntax decides alone, in any parsed language    | `tools/ast-grep/rules/<language>`, a capture in `utils/<language>`, outline row   |
|  [03]   | Form resolved names, types, or attributes decide     | `tools/dylint`, `apps/roslyn`, `tools/refurb`, a `tsconfig.base.json` row         |
|  [04]   | Fact a checker computes from a grammar or engine     | That checker's code under `tools/`, consumers rebuilt to its finding              |
|  [05]   | Fact no checker decides soundly for every instance   | Language skill section whose heading covers it, else the family skill covering it |
|  [06]   | Placement, target, entry point, or catalog feature   | README `[OWNERS]` row, the catalog or manifest row                                |
|  [07]   | Divider, section order, or wording                   | `clean-prose` and `prose.py`                                                      |

Owner criteria:
- Soundness: the first owner in rank deciding the form for every instance owns it, an owner reporting a subset of another's instances owns nothing
- Form: `use-ast-grep` rules decide the shown form, widened category, and shared site, the near rule or util named before a new one is written
- Conflict: a lint's help contradicting a skill line or a package refusal narrows the lint by one predicate or changes the line, never a suppression
- Off rows: a row turning a check off stays while its reason holds, the reason reread each review
- Outside causes: a machine setting or host build is named with its owner, a workaround inside the tree is a hidden cause

Check shape, kept by every rule or lint written or widened:
- Rules take the `use-ast-grep` template order, utils, and `files` rules, its `performance.md` holds cost per form and the `hyperfine` baseline form
- Lints read the resolved result (type, definition path, attribute, trait) inside their existing pass, a walk added per lint fails

## [04]-[ROLES]

Reviewers, a combiner, fixers, and adversaries run as delegated agents when a subject spans more than one file group, language owner, or pass, else one judge holds every role in order. A lead keeps the merge and spawns each round in one message, disjoint scopes at once and scopes sharing a file in order, never one agent over the whole.

Reviewers read one lens each, edit nothing, and return principles (form replaced, stronger form, criterion, evidence, standing in guidance), codifiable rules before and after, checker rows (fact kind, owner, near rule, shape, message), and mistakes per pass with what catches each earlier:

| [INDEX] | [LENS]             | [READS]                                                           | [RETURNS]                                      |
| :-----: | :----------------- | :---------------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | Diff forward       | Each hunk against the file whole, consumers, and pinned source    | Verdict per hunk with its principle            |
|  [02]   | Standards backward | Final file against skill, lint rows, rules, utils, `tools/` lints | Each line contradicted, weak, or missing       |
|  [03]   | Process            | Each pass's tool-call log in order                                | Each wasted round and skipped check, its catch |

Combiner, after every reviewer returns:
1. Overlaps merge under the stronger statement, complements integrate, contradictions resolve by the source read whole at its tag or one run
2. Each withdrawn claim is listed with its evidence, a gap present at base that no pass or reviewer named takes its owner
3. Owner checks a pass skipped and timings a pass omitted run, their output entering the result
4. Returns hunk verdicts per file, principles with owner file and section, codifiable rows with owners, checker rows, and mistakes to the fixers

Fixers, at once over disjoint owners and in order over a shared file:
- One per file group over its hunk verdicts, undoing or rebuilding each wrong, naive, and shuffle hunk in place with its owner checks rerun
- One per skill, editing the owning section alone as `[05]` states, each fact once across the language's skill set
- One per checker owner and per rule folder, proving each check as `[05]` states, a change another owner's file needs reroutes in one later round
- One close moving renamed ids to every reference across the tree and deleting each mention of a removed id

Adversaries:
- Code: the `refactor-code` adversary passes in sequence per file group, then a verdict in place
- Guidance: one per edited skill over `git diff HEAD -- .claude/skills/<skill>` and its untracked files, hunk by hunk with the passage around each
- Guidance verdict: each line weaker than code in the tree, wrong against package source, or stating a fact another skill states is rebuilt in place
- Prose: a clean-prose pass over the same lines, ending with `prose.py check`

Briefs carry the `refactor-code` brief contents with scope (files owned, read, excluded), standards (sections and skills judging the scope, memory entries), and saved research by absolute scratchpad path. Each role returns structured: files edited, counts before and after, changes reaching other files, facts verified with sources, errors met, and `Unverified:` claims with the command settling them.

Lead conduct:
- Waits for completion notices, polling an agent's output reads partial results
- Merges findings under their file keeping the strongest evidence, reruns the deciding check on a conflicting verdict
- Reads each change against live text before it enters the result, re-derives no verified fact

## [05]-[CODIFYING]

Facts a change settled leave it as principles and checks, mined after the verdict and landed in the same review:
1. Principles hold for any file of the language, naming the form replaced, the stronger form, its category, and its criterion on placeholder names
2. Principles name no path, session, run, count, or term the field lacks, a one-file move is none, an exception one instance needs takes no line
3. Each lands at the skill section whose heading covers it, the `[PRINCIPLES]` list when it spans constructs, a reference when its topic sits there
4. Weak lines are replaced or split, a snippet rewritten whole, a wrong line deleted with every mention and nothing standing in its place
5. `rg -F` over the language's skill folders shows where another file states the fact, the owner line stays and nothing is added beside it
6. Each routes through `[03]` to its check, its message short and agent-facing
7. Rules written or rewritten prove once: one scan of a scratch file of violating forms and near misses, one hit per form, then the project at zero
8. Rules are timed against their kind baseline per `use-ast-grep` `performance.md`, lints and rows run over the finding sites before and after
9. Scratch files are deleted after their scan, zero findings alone prove nothing
10. Adversary and prose passes of `[04]` read the modified lines alone

Reviews are finished when every wrong, naive, and shuffle hunk is rebuilt or undone in place, each principle sits at its owner and each check is proved, `hk fix <paths>` ran over edited paths, and `nx run <project>:check` of each edited and consumer project (`hk check <paths>` for a file-level checker, `prose.py check` for guidance) passes with zero findings. Results state verdicts per hunk with their principle, counts and timings before and after, principles with owner file and section, checks with id and hits, mistakes with the step catching each earlier, and `Unverified:` claims with the command settling them.
