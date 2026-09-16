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

Numbered lines chain, each consuming the line before, unnumbered lines are alternatives, one per case its comment names.

## [01]-[DECLARATION]

One member's signature and doc, a type's members, a module's exports, or a configuration option, read from the installed files at the pinned version:

```bash
# [VERSION] Installed version of a direct or transitive package and the package that depends on it, one line per ecosystem
# [DOTNET] Resolved version at each node of the graph from a restored project
dotnet nuget why <project>.csproj <id>
# [TYPESCRIPT] Version per dependent, a transitive package's directory is node_modules/.pnpm/<pkg>@<version>*/node_modules/<pkg>, scope / spelled +
pnpm why <pkg> --depth 1
# [PYTHON] Version with every direct dependent from uv.lock, an extra or group in parentheses
uv tree --frozen --invert --package <pkg> --depth 1

# [DOTNET] Types declaring a member as .xml doc ids, ``N follows a generic method name, no decompile
# Installed version, <lib> is .cache/nuget/packages/<id>/<version>/lib/<tfm>
rg -oI 'M:[\w.`]+\.<Member>(``\d)?\([^"]*' <lib>/*.xml
# Version the packages folder lacks, the nupkg streamed through tar, id lowercase
curl -sL "https://api.nuget.org/v3-flatcontainer/<id>/<version>/<id>.<version>.nupkg" | tar -xOf - 'lib/*.xml' | rg -oI 'M:[\w.`]+\.<Member>(``\d)?\([^"]*'

# [DOTNET] Decompiled source on stdout with doc comments and nested types, <dll> is <lib>/<assembly>.dll of an installed version
# 1. Full name of a type with its `N arity, entity types c, i, s, d, e as one word
dotnet dnx ilspycmd -y -- -l cisde <dll> | rg '<Type>'
# 2. Type line and public members of the type step 1 named
dotnet dnx ilspycmd -y -- -t '<Namespace.Type`N>' <dll> | rg -n '^\s*public '
# 3. Doc comment, attributes, and signature per overload with line, `-A<n>` on the same rg reads the body
dotnet dnx ilspycmd -y -- -t '<Namespace.Type`N>' <dll> | rg -nU '(^[ \t]*///.*\n)*([ \t]*\[.*\]\n)*[ \t]*public .*\b<Member>(<[^>]*>)?\(.*'

# [TYPESCRIPT] Package under node_modules/<pkg>, package.json types names the entry file
# 1. Exports of one declaration file, `export * as <Module>` names the module file step 2 searches
rg -n '^export ' node_modules/<pkg>/<file>.d.ts
# 2. Declaration with file and line, overloads continue below, the doc comment above reads through Read at that line
rg -n --no-ignore -A8 '^export (declare )?(abstract )?(const|function|class|interface|type|enum|namespace) <Symbol>\b' node_modules/<pkg> --glob '*.d.ts'

# [TYPESCRIPT] Type and enum values of one configuration schema definition
jq -c '(.["$defs"] // .definitions).<Def>' node_modules/<pkg>/<schema>.json

# [PYTHON] Package in uv.lock, --with <pkg> before python resolves one outside the lock
# 1. Declared API from __all__, public names of dir without one
uv run --frozen python -c "import <mod>; print(getattr(<mod>, '__all__', None) or [n for n in dir(<mod>) if not n.startswith('_')])"
# 2. Signature per parameter with module and docstring, a class lists its members
uv run --frozen python -c "import <mod>; help(<mod>.<Obj>)"
# 3. Body of a function or class
uv run --frozen python -c "import inspect, <mod>; print(inspect.getsource(<mod>.<Obj>))"

# [PYTHON] Typed signature per overload from the package's .pyi or its -stubs package where the runtime declares no types
rg -nU --no-ignore 'def <fn>\([^)]*\)[^:]*:' .venv/lib/python*/site-packages/<pkg>* --glob '*.pyi'

# [MSBUILD] Target with Condition, Inputs, and DependsOnTargets across the SDK global.json resolves, package targets under build/ or buildTransitive/
rg -n -A3 '<Target Name="<target>"' "$(dotnet msbuild <project>.csproj -getProperty:MSBuildToolsPath)"

# [JAVA] Members and signatures of classes from the jars on a classpath, several classes per call
javap -cp '<jar>:<jar>' <package.Class> <package.Class>
```

## [02]-[USAGE]

How a member composes, from the repository's code examples, source, wiki, and README and from public code, one concept per query naming the symbol:

```text
# [CONTEXT7] ID is the repository URL's /owner/repo lowercased, resolve when metadata names no URL
# Candidates with snippet count, reputation, benchmark score, and Versions
mcp__context7__resolve-library-id {"libraryName": "<repository name>", "query": "<task sentence naming the symbol>"}
# Code examples and source of the default branch with a Source URL per snippet, wiki pages can describe an older major, `not found` means unindexed
mcp__context7__query-docs {"libraryId": "/<owner>/<repo>", "query": "<one concept naming the symbol>"}
# Snapshot from the Versions list, __branch__<name> snapshots an older major
mcp__context7__query-docs {"libraryId": "/<owner>/<repo>/<version>", "query": "<one concept>"}
# Rendered docs site with API reference pages, Source URL names the docs version, a code fence can lose line breaks
mcp__context7__query-docs {"libraryId": "/websites/<site>", "query": "<one concept>"}

# [DEEPWIKI] Generated wiki of the default branch
# 1. Page index per repository, what its parts are, `Repository not found` routes that repository to its tree and files
mcp__deepwiki__read_wiki_structure {"repoName": "<owner>/<repo>"}
# 2. Answer with quoted signatures over repositories step 1 indexed, up to ten per call
mcp__deepwiki__ask_question {"repoName": ["<owner>/<repo>", "<owner>/<repo>"], "question": "<question naming the members>"}

# [GITHUB] Public code composing members, repository and commit per fragment
mcp__github__search_code {"query": "\"<Member>\" \"<Member>\" language:<language>", "perPage": 5, "fields": ["repository", "path", "text_matches"]}

# [DOTNET] AGENTS.md of an installed NuGet package, its README otherwise, from the packages folder or the source
mcp__nuget__get_package_context {"solutionDirectory": "<repo>", "packageName": "<id>", "packageVersion": "<version>"}
```

```bash
# [CONTEXT7] Stars, trust score, and last update per candidate, the MCP result omits them, libraryName is the repository name
curl -s "https://context7.com/api/v2/libs/search?libraryName=<repository name>" -H "Authorization: Bearer $CONTEXT7_API_KEY" | jq -r '.results[:8][] | [.id, .benchmarkScore, .trustScore, .stars, .totalSnippets, .lastUpdateDate[:10], (.versions|join(","))] | @tsv'

# [DOTNET] Cited README lines of an installed package
rg -n --no-ignore -A6 '<term>' .cache/nuget/packages/<id>/<version> --glob '*.md'

# [TYPESCRIPT] Cited README lines of an installed package
rg -n --no-ignore -A6 '<term>' node_modules/<pkg> --glob '*.md'

# [PYTHON] Cited lines of a package's long description at one version, case sensitive
curl -s "https://pypi.org/pypi/<pkg>/<version>/json" | jq -r '.info.description' | rg -n -A6 '<term>'
```

## [03]-[REPOSITORY]

Repository, commit, and tag behind an installed version, then its tree, files, blame, and wiki at that tag:

```bash
# 1. Repository URL and build commit from package metadata, one line per ecosystem
# [DOTNET] projectUrl holds the repository when the repository element holds a commit alone
rg -o '<(repository|projectUrl)[^<]*' .cache/nuget/packages/<id>/<version>/<id>.nuspec
# [TYPESCRIPT] Repository URL, package directory in a monorepo, and commit when the publisher recorded one
pnpm view <pkg>@<version> repository.url repository.directory gitHead --json
# [PYTHON] Source, documentation, and changelog URLs of the installed version
curl -s "https://pypi.org/pypi/<pkg>/<version>/json" | jq -c '.info.project_urls'
# 2. Tag spelling for a version (v1.5.0, 8.7.0, effect@3.22.1, 4.0.0a6), a nuspec commit matches column one, no filter lists every tag
git ls-remote --tags https://github.com/<owner>/<repo> | rg 'refs/tags/\S*<version>$'
# 3. Cited lines of a file at the tag, any size
gh api "repos/<owner>/<repo>/contents/<path>?ref=<tag>" -H "Accept: application/vnd.github.raw+json" | rg -n -A3 '\b<term>\b'

# [GITHUB] Wiki pages served raw, a repository without a wiki prints no page name and answers 404 to a page
# 1. Page names from the wiki's page list, Home stays unlisted
curl -sL https://github.com/<owner>/<repo>/wiki/_pages | rg -o 'href="/<owner>/<repo>/wiki/[^"#/_][^"#/]*"' | sort -u
# 2. Cited lines of one page
curl -sL https://raw.githubusercontent.com/wiki/<owner>/<repo>/<Page>.md | rg -n -A8 '\b<term>\b'
```

```text
# [GITHUB] Reads at the tag
# 1. Paths with sizes under one directory
mcp__github__get_repository_tree {"owner": "<owner>", "repo": "<repo>", "tree_sha": "<tag>", "recursive": true, "path_filter": "<dir>/"}
# 2. Whole file into the window, fields applies to a directory listing alone
mcp__github__get_file_contents {"owner": "<owner>", "repo": "<repo>", "path": "<file>", "ref": "<tag>"}
# 3. Commit, author, and date that last changed each line range
mcp__github__get_file_blame {"owner": "<owner>", "repo": "<repo>", "path": "<file>", "ref": "<tag>", "start_line": <n>, "end_line": <n>}

# [GITHUB] Paths and fragments in one repository
mcp__github__search_code {"query": "\"<phrase>\" repo:<owner>/<repo> path:<dir>", "perPage": 5, "fields": ["path", "text_matches"]}
```

## [04]-[RELEASE]

Newest version and its date come from the registry, notes come from the release at the tag, an advisory names the patched version:

```text
# [DOTNET] Newest NuGet version with its publish date
mcp__nuget__get_latest_package_version {"solutionDirectory": "<repo>", "packageName": "<id>", "includePrerelease": true}

# [GITHUB] Advisories on the pinned versions with the patched version each, one call across ecosystems, owner and repo name this repository
mcp__github__check_dependency_vulnerabilities {"owner": "<owner>", "repo": "<repo>", "dependencies": [{"ecosystem": "nuget", "name": "<id>", "version": "<version>"}, {"ecosystem": "npm", "name": "<pkg>", "version": "<version>"}, {"ecosystem": "pip", "name": "<pkg>", "version": "<version>"}]}
```

```bash
# [TYPESCRIPT] Newest version per dist-tag (latest, next, beta, rc) and publish date of one version
pnpm view <pkg> dist-tags time --json | jq -c '{tags: .["dist-tags"], published: .time["<version>"][:10]}'

# [PYTHON] Lock version beside the newest, prereleases per pyproject
uv tree --frozen --outdated --package <pkg> --depth 0
# [PYTHON] Newest stable version, its upload time, and its requirements from the registry, the versionless URL describes newest stable
curl -s https://pypi.org/pypi/<pkg>/json | jq -r '.info.version, .urls[0].upload_time, .info.requires_dist[]?'

# [GITHUB] Release notes body alone, Not Found means the tag has no release and the changelog at the tag holds the notes
gh api "repos/<owner>/<repo>/releases/tags/<tag>" --jq '.body'
```
