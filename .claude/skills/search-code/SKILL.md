---
name: search-code
description: "Use when a task needs a dependency's signature, API shape, usage, docs, wiki, source, tags, or releases from the installed package or its repository."
---

# [SEARCH_CODE]

Dependency declarations read from installed files, usage from Context7, DeepWiki, and public code, source and wiki from the repository at the installed tag, versions from the registry.

- Version: Context7 and DeepWiki index the default branch, an installed version's fact comes from its files, its tag, or a Context7 snapshot
- Path: package metadata names the repository and commit, the Context7 ID is that `/owner/repo` lowercased, the tag list spells the tag
- Prose: Context7 descriptions and DeepWiki answers are generated, the code is quoted from the Source URL, a named member confirms in the declaration
- Ignore: an ignore file inside the searched tree (`.venv/.gitignore`) hides its files, `--no-ignore` reads them, a listed path needs no flag
- Size: `get_file_contents`, `get_package_context`, and `read_wiki_contents` return the whole document, a cited line reads through `rg`
- Index: `search_code` reads default branches in files under 384 KB, the tree call finds a path at a tag
- Caps: Context7 tool descriptions cap each tool at 3 calls per question, an ID from the repository URL skips the resolve

Numbered steps chain, each consuming the step before, bulleted cases are alternatives, one per command line in order.

## [01]-[DECLARATION]

One member's signature and doc, a type's members, a module's exports, or a configuration option, read from the installed files at the pinned version.

Installed version of a direct or transitive package and the package that depends on it, one line per ecosystem:
- .NET: resolved version at each node of the graph from a restored project
- TypeScript: version per dependent, transitive packages sit under `node_modules/.pnpm/<pkg>@<version>*/node_modules/<pkg>`, scope `/` spelled `+`
- Python: version with every direct dependent from `uv.lock`, an extra or group in parentheses

```bash
dotnet nuget why <project>.csproj <id>
pnpm why <pkg> --depth 1
uv tree --frozen --invert --package <pkg> --depth 1
```

.NET types declaring a member as `.xml` doc ids, ` ``N ` follows a generic method name, no decompile:
- Installed version, `<lib>` is `.cache/nuget/packages/<id>/<version>/lib/<tfm>`
- Version the packages folder lacks, the nupkg streamed through `tar`, id lowercase

```bash
rg -oI 'M:[\w.`]+\.<Member>(``\d)?\([^"]*' <lib>/*.xml
curl -sL "https://api.nuget.org/v3-flatcontainer/<id>/<version>/<id>.<version>.nupkg" | tar -xOf - 'lib/*.xml' | rg -oI 'M:[\w.`]+\.<Member>(``\d)?\([^"]*'
```

.NET decompiled source on stdout with doc comments and nested types, `<dll>` is `<lib>/<assembly>.dll` of an installed version:
1. Full name of a type with its `` `N `` arity, entity types `c`, `i`, `s`, `d`, `e` as one word
2. Type line and public members of the type step 1 named
3. Doc comment, attributes, and signature per overload with line, `-A<n>` on the same `rg` reads the body

```bash
dotnet dnx ilspycmd -y -- -l cisde <dll> | rg '<Type>'
dotnet dnx ilspycmd -y -- -t '<Namespace.Type`N>' <dll> | rg -n '^\s*public '
dotnet dnx ilspycmd -y -- -t '<Namespace.Type`N>' <dll> | rg -nU '(^[ \t]*///.*\n)*([ \t]*\[.*\]\n)*[ \t]*public .*\b<Member>(<[^>]*>)?\(.*'
```

.NET decompiled files under `<main>/.artifacts/ilspy`, `<main>` is the first worktree line of `git worktree list --porcelain`:
1. Assembly name and version as `<Name>/<Version>`, an existing `<main>/.artifacts/ilspy/<Name>/<Version>` skips step 2
2. Project with one file per type, without `-p` the whole assembly as one `<Name>.decompiled.cs`

```bash
dotnet dnx ilspycmd -y -- --dump-table Assembly --json <dll> | jq -r '.rows[0] | "\(.Name)/\(.Version)"'
dotnet dnx ilspycmd -y -- -p -o <main>/.artifacts/ilspy/<Name>/<Version> <dll>
```

TypeScript package under `node_modules/<pkg>`, `package.json` `types` names the entry file:
1. Exports of one declaration file, `export * as <Module>` names the module file step 2 searches
2. Declaration with file and line, overloads continue below, the doc comment above reads through `Read` at that line

```bash
rg -n '^export ' node_modules/<pkg>/<file>.d.ts
rg -n --no-ignore -A8 '^export (declare )?(abstract )?(const|function|class|interface|type|enum|namespace) <Symbol>\b' node_modules/<pkg> --glob '*.d.ts'
```

TypeScript type and enum values of one configuration schema definition:

```bash
jq -c '(.["$defs"] // .definitions).<Def>' node_modules/<pkg>/<schema>.json
```

Python package in `uv.lock`, `--with <pkg>` before `python` resolves one outside the lock:
1. Declared API from `__all__`, public names of `dir` without one
2. Signature per parameter with module and docstring, a class lists its members
3. Body of a function or class

```bash
uv run --frozen python -c "import <mod>; print(getattr(<mod>, '__all__', None) or [n for n in dir(<mod>) if not n.startswith('_')])"
uv run --frozen python -c "import <mod>; help(<mod>.<Obj>)"
uv run --frozen python -c "import inspect, <mod>; print(inspect.getsource(<mod>.<Obj>))"
```

Python typed signature per overload from the package's `.pyi` or its `-stubs` package where the runtime declares no types:

```bash
rg -nU --no-ignore 'def <fn>\([^)]*\)[^:]*:' .venv/lib/python*/site-packages/<pkg>* --glob '*.pyi'
```

MSBuild target with `Condition`, `Inputs`, and `DependsOnTargets` across the SDK `global.json` resolves, package targets under `build/` or `buildTransitive/`:

```bash
rg -n -A3 '<Target Name="<target>"' "$(dotnet msbuild <project>.csproj -getProperty:MSBuildToolsPath)"
```

Java members and signatures of classes from the jars on a classpath, more than one class per call:

```bash
javap -cp '<jar>:<jar>' <package.Class> <package.Class>
```

## [02]-[USAGE]

How a member composes, from the repository's code examples, source, wiki, and README and from public code, one concept per query naming the symbol.

Context7 ID is the repository URL's `/owner/repo` lowercased, resolve when metadata names no URL:
- Candidates with snippet count, reputation, benchmark score, and Versions
- Code examples and source of the default branch with a Source URL per snippet, wiki pages can describe an older major, `not found` means unindexed
- Snapshot from the Versions list, `__branch__<name>` snapshots an older major
- Rendered docs site with API reference pages, Source URL names the docs version, a code fence can lose line breaks

```text
mcp__context7__resolve-library-id {"libraryName": "<repository name>", "query": "<task sentence naming the symbol>"}
mcp__context7__query-docs {"libraryId": "/<owner>/<repo>", "query": "<one concept naming the symbol>"}
mcp__context7__query-docs {"libraryId": "/<owner>/<repo>/<version>", "query": "<one concept>"}
mcp__context7__query-docs {"libraryId": "/websites/<site>", "query": "<one concept>"}
```

DeepWiki generated wiki of the default branch:
1. Page index per repository, what its parts are, `Repository not found` routes that repository to its tree and files
2. Answer with quoted signatures over repositories step 1 indexed, up to 10 per call

```text
mcp__deepwiki__read_wiki_structure {"repoName": "<owner>/<repo>"}
mcp__deepwiki__ask_question {"repoName": ["<owner>/<repo>", "<owner>/<repo>"], "question": "<question naming the members>"}
```

GitHub public code composing members, repository and commit per fragment:

```text
mcp__github__search_code {"query": "\"<Member>\" \"<Member>\" language:<language>", "perPage": 5, "fields": ["repository", "path", "text_matches"]}
```

.NET `AGENTS.md` of an installed NuGet package, its README otherwise, from the packages folder or the source:

```text
mcp__nuget__get_package_context {"solutionDirectory": "<repo>", "packageName": "<id>", "packageVersion": "<version>"}
```

Context7 stars, trust score, and last update per candidate, the MCP result omits them, `libraryName` is the repository name:

```bash
curl -s "https://context7.com/api/v2/libs/search?libraryName=<repository name>" -H "Authorization: Bearer $CONTEXT7_API_KEY" | jq -r '.results[:8][] | [.id, .benchmarkScore, .trustScore, .stars, .totalSnippets, .lastUpdateDate[:10], (.versions|join(","))] | @tsv'
```

Cited README lines of an installed .NET or TypeScript package:

```bash
rg -n --no-ignore -A6 '<term>' .cache/nuget/packages/<id>/<version> --glob '*.md'
rg -n --no-ignore -A6 '<term>' node_modules/<pkg> --glob '*.md'
```

Python cited lines of a package's long description at one version, case sensitive:

```bash
curl -s "https://pypi.org/pypi/<pkg>/<version>/json" | jq -r '.info.description' | rg -n -A6 '<term>'
```

## [03]-[REPOSITORY]

Repository, commit, and tag behind an installed version, then its tree, files, blame, and wiki at that tag:
1. Repository URL and build commit from package metadata, one line per ecosystem:
    - .NET: `projectUrl` holds the repository when the `repository` element holds a commit alone
    - TypeScript: repository URL, package directory in a monorepo, and commit when the publisher recorded one
    - Python: source, documentation, and changelog URLs of the installed version
2. Tag spelling for a version (`v1.5.0`, `8.7.0`, `effect@3.22.1`, `4.0.0a6`), a nuspec commit matches column one, no filter lists every tag
3. Cited lines of a file at the tag, any size

```bash
rg -o '<(repository|projectUrl)[^<]*' .cache/nuget/packages/<id>/<version>/<id>.nuspec
pnpm view <pkg>@<version> repository.url repository.directory gitHead --json
curl -s "https://pypi.org/pypi/<pkg>/<version>/json" | jq -c '.info.project_urls'
git ls-remote --tags https://github.com/<owner>/<repo> | rg 'refs/tags/\S*<version>$'
gh api "repos/<owner>/<repo>/contents/<path>?ref=<tag>" -H "Accept: application/vnd.github.raw+json" | rg -n -A3 '\b<term>\b'
```

GitHub wiki pages served raw, a repository without a wiki prints no page name and answers 404 to a page:
1. Page names from the wiki's page list, Home stays unlisted
2. Cited lines of one page

```bash
curl -sL https://github.com/<owner>/<repo>/wiki/_pages | rg -o 'href="/<owner>/<repo>/wiki/[^"#/_][^"#/]*"' | sort -u
curl -sL https://raw.githubusercontent.com/wiki/<owner>/<repo>/<Page>.md | rg -n -A8 '\b<term>\b'
```

GitHub reads at the tag:
1. Paths with sizes under one directory
2. Whole file into the window, `fields` applies to a directory listing alone
3. Commit, author, and date that last changed each line range

```text
mcp__github__get_repository_tree {"owner": "<owner>", "repo": "<repo>", "tree_sha": "<tag>", "recursive": true, "path_filter": "<dir>/"}
mcp__github__get_file_contents {"owner": "<owner>", "repo": "<repo>", "path": "<file>", "ref": "<tag>"}
mcp__github__get_file_blame {"owner": "<owner>", "repo": "<repo>", "path": "<file>", "ref": "<tag>", "start_line": <n>, "end_line": <n>}
```

GitHub paths and fragments in one repository:

```text
mcp__github__search_code {"query": "\"<phrase>\" repo:<owner>/<repo> path:<dir>", "perPage": 5, "fields": ["path", "text_matches"]}
```

## [04]-[RELEASE]

Newest version and its date come from the registry, notes come from the release at the tag, an advisory names the patched version.

.NET newest NuGet version with its publish date:

```text
mcp__nuget__get_latest_package_version {"solutionDirectory": "<repo>", "packageName": "<id>", "includePrerelease": true}
```

GitHub advisories on the pinned versions with the patched version each, one call across ecosystems, owner and repo name this repository:

```text
mcp__github__check_dependency_vulnerabilities {"owner": "<owner>", "repo": "<repo>", "dependencies": [{"ecosystem": "nuget", "name": "<id>", "version": "<version>"}, {"ecosystem": "npm", "name": "<pkg>", "version": "<version>"}, {"ecosystem": "pip", "name": "<pkg>", "version": "<version>"}]}
```

TypeScript newest version per dist-tag (`latest`, `next`, `beta`, `rc`) and publish date of one version:

```bash
pnpm view <pkg> dist-tags time --json | jq -c '{tags: .["dist-tags"], published: .time["<version>"][:10]}'
```

Python versions:
- Lock version beside the newest, prereleases per `pyproject`
- Newest stable version, its upload time, and its requirements from the registry, the versionless URL describes newest stable

```bash
uv tree --frozen --outdated --package <pkg> --depth 0
curl -s https://pypi.org/pypi/<pkg>/json | jq -r '.info.version, .urls[0].upload_time, .info.requires_dist[]?'
```

GitHub release notes body alone, `Not Found` means the tag has no release and the changelog at the tag holds the notes:

```bash
gh api "repos/<owner>/<repo>/releases/tags/<tag>" --jq '.body'
```
