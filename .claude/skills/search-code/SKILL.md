---
name: search-code
description: "Use when a task needs a dependency's signature, API shape, usage, docs, wiki, source, tags, or releases from the installed package or its repository."
---

# [SEARCH_CODE]

A dependency's declarations, usage, source, and releases read from the installed files, Context7, DeepWiki, and its repository. Use `dotnet-roslyn-codelens` for a referenced assembly's members, extension methods, and IL. Use `search-web` for the open web, the issue that explains a symptom, a product release without a repository, and a composition of two packages with no recipe in either's docs. Greptile knowledge bases cover the organization's own repositories, never a dependency.

- Currency: Context7 indexes the default branch and GitHub wiki, DeepWiki its generated wiki, an installed version's fact comes from its file or tag
- Sources: each Context7 snippet's Source URL names its ref, a version snapshot returns `blob/main` sources beside `blob/<tag>` ones
- Size: `get_file_contents` returns a whole file and ignores `fields`, `read_wiki_contents` returns the whole wiki, cited lines read through `rg`
- Ignore: `rg` under `node_modules` or `.venv` takes `--no-ignore`, the repository's `dist/` rule and `.venv/.gitignore` hide the files
- Caps: the tool descriptions cap `resolve-library-id` and `query-docs` at three calls each per question, a known ID skips the resolve
- IDs: a package resolves under its repository's name, a binding or repackaging (`Rasm.Native.*`) under its upstream's name
- IDs: `/org/project` holds recipes with source, `/websites/*` holds concept prose, a version snapshot lists in the resolve result
- Misses: `search_code` indexes the default branch and files under 384 KB, a file at a tag or over the limit reads through `gh api` raw and `rg`
- Checks: a snippet whose heading or Source URL names another type or version confirms against the installed declaration, a repeated query repeats
- Quota: a Context7 error naming the quota ends its use for the session, the installed file and the repository answer, an answer from memory says so

## [01]-[SIGNATURE]

An installed package states a declaration at the version the manifest pins, one call per member:

```bash
rg -n -o 'M:<Type>.<Member>(``[0-9]+)?\([^"]*' "$(dotnet nuget locals global-packages -l | cut -d' ' -f2)/<lowercase-id>/<version>/lib/<tfm>/<assembly>.xml"   # Parameter types per overload of a package the solution does not reference, no return type, generic arity after two backticks
curl -sL "https://api.nuget.org/v3-flatcontainer/<lowercase-id>/<version>/<lowercase-id>.<version>.nupkg" | tar -xf - -C <dir> 'lib/*'   # XML doc and assembly at the central version when the global-packages folder holds another
dotnet dnx ilspycmd -y -- -l cise <dll>                                                                              # Classes, interfaces, structs, and enums of an assembly with no XML doc, the assembly name differs from the package id
dotnet dnx ilspycmd -y -- -t <Namespace.Type> <dll> | rg -n 'public .*<Member>\('                                    # Full signature per overload, decompiled
pnpm why <pkg>                                                                                                       # Consumers per version, a direct dependency under node_modules/<pkg>, a transitive one under node_modules/.pnpm/node_modules/<pkg>
rg -n --no-ignore -A8 '^export (type|interface|declare (const|function|class)) <Symbol>\b' node_modules/<pkg> --glob '*.d.ts'   # Declaration with its doc comment, --glob '*.d.ts' skips the compiled .js
jq -c '.["$defs"].<Def>' node_modules/<pkg>/<schema>.json                                                            # Enum values of a configuration schema definition
uv run --frozen python -c "import inspect, <mod>; print(inspect.signature(<mod>.<fn>))"                             # Signature of a locked dependency, --with <pkg> resolves the newest release of one outside the lock
rg -n -B1 --no-ignore 'def <fn>\b' .venv/lib/python*/site-packages/<pkg>-stubs/ --glob '*.pyi'                      # Typed overloads a stub package declares, one @overload line per signature
uv run python -c "import inspect, <mod>; print(inspect.getsourcefile(<mod>.<cls>))"                                  # Source file under .venv for the body
uv run ruff rule <code>                                                                                              # Rule text with its options at the installed ruff
rg -n -A3 'Name="<target>"' "$(dotnet msbuild <project>.csproj -getProperty:MSBuildToolsPath)" --glob '*.targets'   # Target with its Inputs and Outputs at the SDK global.json resolves, a package target under its build/ folder
```

## [02]-[USAGE]

Context7 states how a member composes, one concept per query in a task sentence naming the symbol:

```text
mcp__context7__resolve-library-id {"libraryName": "<repository name>", "query": "<task sentence naming the symbol>"}   # Title, ID, snippet count, benchmark score, and version snapshots per candidate
mcp__context7__query-docs {"libraryId": "/<org>/<project>", "query": "<one concept naming the symbol>"}              # Snippets from the repository docs, source, and wiki, a Source URL each
mcp__context7__query-docs {"libraryId": "/<org>/<project>/<version>", "query": "<one concept>"}                       # Snapshot from the resolve list, an installed version with no snapshot reads its installed declaration
mcp__deepwiki__ask_question {"repoName": "<owner>/<repo>", "question": "<question>"}                                  # Synthesized lead over the default branch, each member it names confirms through a signature call
```

```bash
curl -s "https://context7.com/api/v1/search?query=<name>" -H "Authorization: Bearer $CONTEXT7_API_KEY" | jq -r '.results[:8][] | [.id, .benchmarkScore, .trustScore, .totalTokens, .lastUpdateDate[:10], .verified, (.versions|join(","))] | @tsv'   # Trust score, token count, and update date the MCP strips
```

## [03]-[REPOSITORY]

A repository's docs, specs, source, and wiki read at a tag, search for the path first, size next, then the file:

```text
mcp__github__search_code {"query": "<term> extension:<ext> path:<dir> repo:<o>/<r>", "perPage": 5, "fields": ["path", "text_matches"]}   # Paths with matching fragments on the default branch
mcp__github__get_file_contents {"owner": "<o>", "repo": "<r>", "path": "<dir>", "ref": "refs/tags/<tag>", "fields": ["type", "name", "size"]}   # Directory listing with a size per file, type tells a subdirectory from an empty file
mcp__github__get_file_contents {"owner": "<o>", "repo": "<r>", "path": "<file>", "ref": "refs/tags/<tag>"}          # Whole file at a tag into the window
mcp__nuget__get_package_context {"solutionDirectory": "<repo>", "packageName": "<id>", "packageVersion": "<version>"}   # README or AGENTS.md of a package, installed or not
```

```bash
gh api "repos/<o>/<r>/contents/<path>?ref=<tag>" --jq '.size'                                                         # Size of a known path before a whole-file read
gh api "repos/<o>/<r>/contents/<path>?ref=<tag>" -H "Accept: application/vnd.github.raw+json" | rg -n -A3 '\b<term>\b'   # Large file at a tag, cited lines alone
git clone -q --depth 1 https://github.com/<o>/<r>.wiki.git <dir>                                                       # Wiki page names, the wiki is its own repository outside code search and the MCP
curl -sL "https://raw.githubusercontent.com/wiki/<o>/<r>/<Page>.md" | rg -n -A8 '\b<term>\b'                            # One wiki page by the name a Context7 Source URL ends with
```

## [04]-[RELEASE]

The newest version, its tag spelling, and its notes:

```text
mcp__github__list_releases {"owner": "<o>", "repo": "<r>", "perPage": 3, "fields": ["tag_name", "published_at", "prerelease"]}   # Newest releases with the tag spelling, prerelease flagged
mcp__github__list_tags {"owner": "<o>", "repo": "<r>", "perPage": 3}                                                    # Repository without releases, order is by name not date
mcp__nuget__get_latest_package_version {"solutionDirectory": "<repo>", "packageName": "<id>", "includePrerelease": true}           # Newest NuGet version with its publish date
```

```bash
gh api repos/<o>/<r>/releases/latest --jq '.tag_name, .published_at'   # Newest stable release, a repository on prereleases reports an older tag than list_releases
gh api "repos/<o>/<r>/releases/tags/<tag>" --jq '.body'                # Release notes body alone
```

## [05]-[FAILURES]

Each envelope names the next call:

| [INDEX] | [OUTPUT]                                                          | [NEXT]                                                      |
| :-----: | :---------------------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | Title or description from `resolve-library-id` of another product | `resolve-library-id` with the upstream name, installed file |
|  [02]   | `Library ... not found` on `query-docs`                           | `resolve-library-id` with the repository name               |
|  [03]   | `Repository not found` from DeepWiki                              | `query-docs` on the repository ID, `search_code`            |
|  [04]   | `SymbolNotFound` or `is not referenced` from Roslyn               | XML doc or `ilspycmd` under the global-packages folder      |
|  [05]   | Empty README resource from `get_package_context`                  | `.nuspec` and `lib/` under the global-packages folder       |
|  [06]   | `total_count: 0` for a symbol on a tag or in a file over 384 KB   | `gh api` raw at the tag, `rg`                               |
