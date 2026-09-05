# [DEEP_RESEARCH]

The sequence that gathers the whole domain behind a skill, a reference, an agent file, a tool, or an integration, judges every fact against the installed release, and rebuilds the sections the findings change. Use `domain-researcher` for the command forms, the gatherer brief, and the checks each run repeats, and `clean-prose` for every line written.

## [01]-[PLACEMENT]

The archive sits at the root of what owns the subject: a skill's research goes under `.claude/skills/<skill>/.archive/`, and a subject with no owning skill goes under the `.archive/` of the folder or category that owns it. Gatherers write under `<scratchpad>/<subject>-research/`, and the close copies the folders into the archive after `git check-ignore` matches its path.

One folder per source kind, each with the findings file of every topic its brief names:

| [INDEX] | [FOLDER]         | [CONTENT]                                                 | [COVERAGE]                                       |
| :-----: | :--------------- | :-------------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `docs/`          | Page files, the URL list, the full-text export            | URL list minus the files on disk is empty        |
|  [02]   | `repo/`          | Schemas, release bodies, threads, source files at the tag | Every schema key, release, and named source file |
|  [03]   | `wild/`          | Configurations, rules, workflows, pages, shallow clones   | At least 12 repositories with rules on disk      |
|  [04]   | `binary-probes/` | Scratch project holding the inputs of every probe         | Every disagreement, default, limit, exit code    |

## [02]-[BASELINE]

State the subject as the tool at its installed version, from `<tool> --version` or the lock file, and name the targets the research feeds. Read those targets whole with the help output of every subcommand, then list each fact they state with its section and the source that decides it. A fact with no source is a probe candidate, and the list is what the findings are judged against.

An archive that already covers the subject is the starting base:
- Read its findings files before any source, and take the sources they cite as gathered
- Probe each fact the task rests on against the installed binary or the source at the installed tag
- Aim the new gathering at the advanced capabilities the earlier pass left thin, named as the gap in the baseline list
- Correct a fact the probe disproves in the findings file that holds it, with the command and the output that settled it

## [03]-[GATHERING]

Dispatch one `opus` general-purpose gatherer per source kind in one message, each owning one folder and writing the findings file of each topic its brief names, and judge every finding. A brief passes when it states the output folder, the installed version, the method in order with the tool and the output path at each step, the coverage criterion, the capabilities the findings file covers, a report of at most 15 lines, and the rule that bulk content lands on disk while the gatherer reads pages as it writes findings.

A gatherer that cannot hold its source kind in one context dispatches sub-gatherers by topic group into its folder while it gathers the topics it keeps. A thread sub-gatherer saves one file per thread and returns the list its gatherer merges into the findings file, and a field sub-gatherer writes the findings file of its topic. The sub-gatherer brief names the topic group, the folder, the tool names to load, the file shape with the answer verbatim, and the size of the return.

Method per source kind:

[DOCUMENTATION]: Enumerate the URLs from the sitemap and the crawler's map subcommand, and crawl the site to one file per page. Diff the URL list against the files on disk, extract the misses in batches, and retry each failure with its URL recorded in `failed.txt`. The site's full-text export, split per `url:` block into the page files, is the authority for exact key names over the crawled HTML.

[REPOSITORY]: Save each schema, the body of every release back to the one before the installed version, the changelog, and the source files the brief names from the tarball at the installed tag, and re-run each extracted source claim against the installed binary. Save each thread a topic search returns as one file with its question and the maintainer's answer verbatim.

[FIELD]: Search code by configuration file name and key, by command in a workflow path, and by import of the library, and pick at least 12 repositories with rules on disk. Save every configuration, rule, utility, test, workflow, and hook file under `code/<owner>-<repo>/<path>`, a repository read whole as a shallow clone under `repos/`, and every page under `pages/`, with one findings file per topic: rule sets and configurations, CI and hooks, editor and library integrations, rewrite and codemod workflows, and guides and talks.

[GUIDES]: Discover through neural search, extract each page to disk, and record the technique a source teaches with its snippet and its URL.

Probe the facts the baseline marked while the gatherers run, and apply the findings peer agents send as they arrive.

## [04]-[FINDINGS]

A findings file holds one heading per capability, thread, repository, or workflow as its brief names, and each fact with the key, flag, or default quoted and its source beside it: a page path, a file path with the function, a release tag, a thread number, or a repository path. A fact with no source returns to its gatherer. Read each findings file whole as its gatherer reports, take its citation as the source of each fact, and probe a fact that changes a target against the binary when it disagrees with a page, another finding, or the target.

## [05]-[PROBES]

The installed binary decides every disagreement between a page, a thread, a gathering report, and a target, and a probe settles each default, order, limit, and exit code a target states. A probe is a scratch project with its configuration, rule files or `--inline-rules`, an input file, the command, and `echo $?`, and its record in the report is the command with the output line beside the decision. A disagreement decided by reading stays open.

## [06]-[INTEGRATION]

Integrate each findings file as it lands, through the sections its facts change, and ownership of a section passes to you with the first edit. A rebuilt section states the category with its criterion, and an enumeration, a manual paragraph, and appended content fail it:
1. Compare the section against the findings, and class each gap as a missing capability, a thin or wrong claim, or a coupling
2. Rebuild the owning section around its criterion, with one placeholder snippet where a shape must be shown
3. Apply each edit as an exact-string replacement that asserts one match, then read the section again
4. Send a finding that belongs to a file another agent owns to that agent, or to `main` when none is active, with the source that decides it
5. Measure every entry against 150 columns, read the target whole, run `git diff`, and delete what adds no criterion
6. Read `git log -p -- <file>` and restore each criterion, capability, flag, and purpose statement an earlier revision stated with more precision

## [07]-[CLOSE]

Copy the source folders without the tag tarball tree, the probe project as `binary-probes/`, the gatherer briefs under `briefs/`, and the brief the run received into the archive. Report each source kind with its path and counts, each section changed with the source that decided it, each probe with its command and exit code, each finding sent with its recipient, and every finding outside the scope, named for `main` in the same report.
