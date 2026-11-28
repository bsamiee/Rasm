---
name: code
description: "Use when a task needs a fact about a dependency or a survey of many repositories, covering source selection by fact kind and GitHub quota"
---

# [SEARCH_CODE]

Dependency declarations come from installed files, usage from Context7, DeepWiki, and public code, source and wiki from the repository at its installed tag, and versions from registries.

- Version: Context7 indexes the default branch, an installed version's fact comes from its files, its tag, or a Context7 snapshot
- Path: package metadata names repository and commit, Context7 IDs are the repository's `/owner/repo` lowercased, a tag list spells each tag
- Prose: Context7 descriptions and DeepWiki answers are generated, code quotes come from their Source URL, member names from their declaration
- Ignore: an ignore file inside the searched tree (`.venv/.gitignore`) hides its files, `--no-ignore` reads them, a listed path needs no flag
- Size: `get_file_contents`, `get_package_context`, and DeepWiki `read_wiki_contents` return whole content, `rg` cites a line
- Index: `search_code` reads default branches in files under 384 KB, `get_repository_tree` finds a path at a tag
- Quota: `GH_TOKEN`, the GitHub MCP, and every other token of one GitHub account spend one pool, the next call past it answers HTTP 403
- Context7: `ctx7` sends `CONTEXT7_API_KEY`, each command spends one call of its monthly quota, an ID from a repository URL skips `ctx7 library`
- Wait: `ctx7` exits 75 while a tag or upgrade guide is indexed, the same command reruns after the wait it prints

## [01]-[DECLARATION]

One member's signature and doc, a type's members, a module's exports, or a configuration option, read from installed files at the pinned version.

Installed version of a direct or transitive package and the package that depends on it, one line per ecosystem:
- .NET: resolved version at each node of the graph from a restored project
- TypeScript: version per dependent, transitive packages sit under `node_modules/.pnpm/<pkg>@<version>*/node_modules/<pkg>`, scope `/` spelled `+`
- Python: version with every direct dependent from `uv.lock`, an extra or group in parentheses
- Rust: version with every direct dependent from `Cargo.lock`
- Swift: resolved tree of root `Package.swift`

```bash
dotnet nuget why <project>.csproj <id>
pnpm why <pkg> --depth 1
uv tree --frozen --invert --package <pkg> --depth 1
cargo tree --locked --invert <crate> --depth 1
swift package show-dependencies --format json
```

Swift interface of a Clang SDK module (`ApplicationServices`, `CoreFoundation`) at the selected Xcode's SDK:

```bash
xcrun swift-synthesize-interface -module-name <Module> -sdk "$(xcrun --show-sdk-path)" -target "arm64-apple-macos$(xcrun --show-sdk-version)" -include-submodules | rg -n '<Member>'
```

.NET types declaring a member as `.xml` doc ids, ` ``N ` follows a generic method name, without decompiling:
- Installed version, `<lib>` is `~/.nuget/packages/<id>/<version>/lib/<tfm>`
- Version missing from the packages folder, its nupkg streamed through `tar`, id lowercase

```bash
rg -oI 'M:[\w.`]+\.<Member>(``\d)?\([^"]*' <lib>/*.xml
xh -F "https://api.nuget.org/v3-flatcontainer/<id>/<version>/<id>.<version>.nupkg" | tar -xOf - 'lib/*.xml' | rg -oI 'M:[\w.`]+\.<Member>(``\d)?\([^"]*'
```

.NET decompiled source on stdout with doc comments and nested types, `<dll>` is `<lib>/<assembly>.dll` of an installed version:
1. Full name of a type with its `` `N `` arity, entity types `c`, `i`, `s`, `d`, `e` as one word
2. Type line and public members of the type step 1 named
3. Doc comment, attributes, and signature per overload with line, `-A<n>` on its `rg` reads the body

```bash
ilspycmd -l cisde <dll> | rg '<Type>'
ilspycmd -t '<Namespace.Type`N>' <dll> | rg -n '^\s*public '
ilspycmd -t '<Namespace.Type`N>' <dll> | rg -nU '(^[ \t]*///.*\n)*([ \t]*\[.*\]\n)*[ \t]*public .*\b<Member>(<[^>]*>)?\(.*'
```

.NET decompiled files under `<main>/.cache/ilspy`, `<main>` the repository root:
1. Assembly name and version as `<Name>/<Version>`, an existing `<main>/.cache/ilspy/<Name>/<Version>` skips step 2
2. Project with one file per type, without `-p` the whole assembly as one `<Name>.decompiled.cs`

```bash
ilspycmd --dump-table Assembly --json <dll> | jq -r '.rows[0] | "\(.Name)/\(.Version)"'
ilspycmd -p -o <main>/.cache/ilspy/<Name>/<Version> <dll>
```

TypeScript exports from the native compiler's export list, run from `<main>`, `<project>` the folder whose `package.json` declares the package:
1. Index: groups with member counts and module summaries, a package name adding its other entry points (`effect/ai`), exit 1 when empty
2. Group: `<folder>/<group>.tsv` read whole, a row per member with name, kind, `<file>:<start>-<end>` span, and first doc line
3. Member: its row found by name across groups, then its doc comment, overloads, and declaration read at the span

```bash
node plugins/shared/search/skills/code/scripts/exports.ts <project> <specifier>
cat <folder>/<group>.tsv
rg '^<member>\t' <folder>
sed -n '<start>,<end>p' <file>
```

TypeScript type and enum values of one configuration schema definition:

```bash
jq -c '(.["$defs"] // .definitions).<Def>' <main>/node_modules/<pkg>/<schema>.json
```

TypeScript catalog package no project declares, its tarball at the catalog release laid out as a `<project>` the export listing reads:

```bash
mkdir -p .artifacts/search/<pkg>@<version>/node_modules/<pkg> && xh -F "$(pnpm view <pkg>@<version> dist.tarball)" | tar -xz --strip-components 1 -C .artifacts/search/<pkg>@<version>/node_modules/<pkg> && echo '{"type": "module"}' > .artifacts/search/<pkg>@<version>/package.json
```

Python package in `uv.lock`, `uv run --no-sync --with <pkg> python` resolves one outside the lock:
1. Docs, signatures, and visible members; dotted names select members, module source holds declared `__all__` and imports
2. Module source with `<mod>`, member source with `<mod>:<qualname>`; `-d` replaces source with origin and cache details

```bash
pixi run --locked python -m pydoc <mod>
pixi run --locked python -m inspect <mod>:<qualname>
```

Python typed signature per overload from a package's `.pyi` or its `-stubs` package where the runtime declares no types:

```bash
rg -nU --no-ignore 'def <fn>\([^)]*\)[^:]*:' <main>/.venv/lib/python*/site-packages/<pkg>* --glob '*.pyi'
```

Rust public declaration with file and line from crate source under `$CARGO_HOME/registry/src`:

```bash
rg -n -A8 '^\s*pub (\w+ )*(fn|struct|enum|trait|type|const|static|macro) <Symbol>\b' \
    $CARGO_HOME/registry/src/index.crates.io-*/<crate>-<version> --glob '*.rs'
```

Rust compiler, standard library, Cargo, Clippy, rustfmt, and rust-analyzer declaration at the pinned nightly's commit, from the source archive mise links at `.cargo/rustc-source`, `<folder>` being `compiler`, `library`, or `src/tools/<tool>`:

```bash
rg -n -A8 '^\s*pub (\w+ )*(fn|struct|enum|trait|type|const|static|macro) <Symbol>\b' \
    <main>/.cargo/rustc-source/<folder> --glob '*.rs'
```

MSBuild target with `Condition`, `Inputs`, and `DependsOnTargets` across the SDK `global.json` resolves, package targets under `build/` or `buildTransitive/`:

```bash
rg -n -A3 '<Target Name="<target>"' "$(dotnet msbuild <project>.csproj -getProperty:MSBuildToolsPath)"
```

Java members and signatures of classes from jars on a classpath, more than one class per call:

```bash
javap -cp '<jar>:<jar>' <package.Class> <package.Class>
```

CLI flags of each subcommand of one name from the tool's carapace completer as JSON (`Longhand`, `Shorthand`, `Usage`), `Short` telling same-named subcommands apart and a tool without a completer printing nothing:

```bash
carapace <tool> export | jq -c '.. | objects | select(.Name? == "<subcommand>") | {Short, LocalFlags}'
```

## [02]-[USAGE]

How a member composes, from the repository's code examples, source, wiki, and README and from public code, one concept per query naming its symbol.

Context7 reads:
- Answer across libraries in one step, up to four `--library` names or IDs, `--library-version` reads that version's snapshot
- Candidates with snippet count, reputation, benchmark score, Versions, and `Source code` on IDs `ctx7 code` reads
- Code examples and source with a Source URL per snippet, `--language` ranks one language first, wiki pages can describe an older major
- Snapshot of the version a project writes (`4.1.0`, `^4.1.0`, `3`) through `--tag`, else the closest one indexed, a major alone its newest release
- Rendered docs site with API reference pages, Source URL names the docs version, a code fence can lose line breaks
- Declaration of one symbol in repository source at the commit its docs describe, `/websites/` and `/llmstxt/` IDs hold no source
- Source lines matching a regex at a `/<owner>/<repo>/<tag>` ID, `-F` reads an error message literally, `-l` names files alone, `-c` counts per file
- Callers of a symbol, symbols of one file, a file's lines, and repository paths through `--op references`, `outline`, `read`, and `list`
- Next page, read, or count from an output's `Next:` line, `offset` as `--offset`, `version` as `--tag`, other fields by their flag name

```bash
ctx7 search '<question naming the symbol>' --library /<owner>/<repo> --language <language>
ctx7 library '<repository name>' '<task sentence naming the symbol>'
ctx7 docs /<owner>/<repo> '<one concept naming the symbol>' --language <language>
ctx7 docs /<owner>/<repo> '<one concept>' --tag <version>
ctx7 docs /websites/<site> '<one concept>'
ctx7 code /<owner>/<repo> '<Symbol>' --op definition
ctx7 code /<owner>/<repo>/<tag> '<regex>' -g '<glob>' -g '!<glob>' -C 3
ctx7 code /<owner>/<repo> '<regex>' -c
ctx7 code /<owner>/<repo>/<tag> --op read --path '<path>#L<n>-L<n>'
```

Context7 stars, trust score, and last update per candidate:

```bash
ctx7 library '<repository name>' '<task sentence>' --json | jq -r '.[] | [.id, .benchmarkScore, .trustScore, .stars, .totalSnippets, .lastUpdateDate[:10], (.versions // [] | join(","))] | @tsv'
```

DeepWiki reads one default-branch commit that can trail the branch by months, one stateless JSON-RPC call each to `https://mcp.deepwiki.com/mcp`:
1. Page index per repository, what its parts are, `Repository not found` routes the repository to its tree and files
2. Answer with quoted signatures and wiki page links over repositories step 1 indexed, one `repoName[]` per repository, up to 10 per call
3. Whole wiki under `.artifacts/search/deepwiki`, a `# Page: <title>` heading per page and `**Sources:**` lines naming `path:start-end`

```bash
xh -b POST https://mcp.deepwiki.com/mcp 'Accept:application/json, text/event-stream' jsonrpc=2.0 id:=1 method=tools/call 'params[name]=read_wiki_structure' 'params[arguments][repoName]=<owner>/<repo>' | rg -r '$1' '^data: (.*)' | jq -r '.result.content[0].text'
xh -b POST https://mcp.deepwiki.com/mcp 'Accept:application/json, text/event-stream' jsonrpc=2.0 id:=1 method=tools/call 'params[name]=ask_wiki_question' 'params[arguments][repoName][]=<owner>/<repo>' 'params[arguments][repoName][]=<owner>/<repo>' 'params[arguments][question]=<question naming the members>' | rg -r '$1' '^data: (.*)' | jq -r '.result.content[0].text'
mkdir -p .artifacts/search/deepwiki/<owner> && xh -b POST https://mcp.deepwiki.com/mcp 'Accept:application/json, text/event-stream' jsonrpc=2.0 id:=1 method=tools/call 'params[name]=read_wiki_contents' 'params[arguments][repoName]=<owner>/<repo>' | rg -r '$1' '^data: (.*)' | jq -r '.result.content[0].text' > .artifacts/search/deepwiki/<owner>/<repo>.md
rg -n '^# Page: |<term>' .artifacts/search/deepwiki/<owner>/<repo>.md
```

Public code composing members, repository and path per fragment:
- Sourcegraph stream search answers without a token, matches spaced terms as one string and `AND` terms in one file, and counts `lineNumber` from 0
- Endpoint `mcp.grep.app` answers literal or `useRegexp` patterns, as grep.app `/api/search` refuses scripts with a Vercel challenge
- Sourcegraph skips forks and archived repositories until the query adds `fork:yes` and `archived:yes`
- `search_code` checks one repository or qualifier both lack, spending the GitHub account's code search quota

```bash
xh -I -b 'https://sourcegraph.com/.api/search/stream' q=='<Member> AND <Member> lang:<language> count:50' | rg -r '$1' '^data: (\[.*)' | jq -c '.[] | select(.type == "content") | {repository, commit, path, line: (.lineMatches[0].lineNumber + 1), text: .lineMatches[0].line}'
xh -I -b POST https://mcp.grep.app 'Accept:application/json, text/event-stream' jsonrpc=2.0 id:=1 method=tools/call 'params[name]=searchGitHub' 'params[arguments][query]=<code literal>' 'params[arguments][language][]=<Language>' | rg -r '$1' '^data: (.*)' | jq -r '.result.content[].text'
```

```text
search_code {"query": "\"<Member>\" \"<Member>\" language:<language>", "perPage": 5, "fields": ["repository", "path", "text_matches"]}
```

.NET `AGENTS.md` of an installed NuGet package, its README otherwise, from the packages folder or source:

```text
get_package_context {"solutionDirectory": "<main>", "packageName": "<id>", "packageVersion": "<version>"}
```

Cited README lines of an installed .NET or TypeScript package:

```bash
rg -n -A6 '<term>' ~/.nuget/packages/<id>/<version> --glob '*.md'
rg -n --no-ignore -A6 '<term>' <main>/<project>/node_modules/<pkg> --glob '*.md'
```

Python cited lines of a package's long description at one version, case sensitive:

```bash
xh https://pypi.org/pypi/<pkg>/<version>/json | jq -r '.info.description' | rg -n -A6 '<term>'
```

## [03]-[REPOSITORY]

Repository, commit, and tag behind an installed version, then its tree, files, blame, and wiki at the tag:
1. Repository URL and build commit from package metadata, one line per ecosystem:
    - .NET: `projectUrl` holds the repository when a `repository` element holds a commit alone
    - TypeScript: repository URL, package directory in a monorepo, and commit when the publisher recorded one
    - Python: source, documentation, and changelog URLs of the installed version
    - Rust: repository URL, then commit and package directory from the crate's `.cargo_vcs_info.json`
2. Tag spelling for a version (`v1.5.0`, `8.7.0`, `effect@3.22.1`, `4.0.0a6`), a nuspec commit matches column one, no filter lists every tag
3. Cited lines of a file at the tag, any size

```bash
rg -o '<(repository|projectUrl)[^<]*' ~/.nuget/packages/<id>/<version>/<id>.nuspec
pnpm view <pkg>@<version> repository.url repository.directory gitHead --json
xh https://pypi.org/pypi/<pkg>/<version>/json | jq -c '.info.project_urls'
cargo info <crate>@<version> | rg '^repository:'
jq -c '{sha1: .git.sha1, path_in_vcs}' $CARGO_HOME/registry/src/index.crates.io-*/<crate>-<version>/.cargo_vcs_info.json
git ls-remote --tags https://github.com/<owner>/<repo> | rg 'refs/tags/\S*<version>$'
gh api "repos/<owner>/<repo>/contents/<path>?ref=<tag>" -H "Accept: application/vnd.github.raw+json" | rg -n -A3 '\b<term>\b'
```

GitHub wiki pages served raw, a repository without a wiki prints no page name and answers 404 to a page:
1. Page names from the wiki's page list, Home stays unlisted
2. Cited lines of one page

```bash
xh -F https://github.com/<owner>/<repo>/wiki/_pages | rg -o 'href="/<owner>/<repo>/wiki/[^"#/_][^"#/]*"' | sort -u
xh -F https://raw.githubusercontent.com/wiki/<owner>/<repo>/<Page>.md | rg -n -A8 '\b<term>\b'
```

GitHub reads at the tag:
1. Paths with sizes under one directory
2. Whole file into the window, `fields` applies to a directory listing alone
3. Commit, author, and date that last changed each line range

```text
get_repository_tree {"owner": "<owner>", "repo": "<repo>", "tree_sha": "<tag>", "recursive": true, "path_filter": "<dir>/"}
get_file_contents {"owner": "<owner>", "repo": "<repo>", "path": "<file>", "ref": "refs/tags/<tag>"}
get_file_blame {"owner": "<owner>", "repo": "<repo>", "path": "<file>", "ref": "<tag>", "start_line": <n>, "end_line": <n>}
```

GitHub paths and fragments in one repository:

```text
search_code {"query": "\"<phrase>\" repo:<owner>/<repo> path:<dir>", "perPage": 5, "fields": ["path", "text_matches"]}
```

## [04]-[RELEASE]

Newest version and its date come from the registry, notes from each tag's release, changes across versions from a Context7 guide, and advisories name patched versions.

Context7 breaking changes, deprecations, codemods, and public API diff across a version range, a library without a guide answers `not prepared yet`:
1. Page into a file, page 1's header names each page a migration needs, `fromVersion` exact, a major alone reads as its newest release
2. Section and note titles with line numbers
3. One section by its title

```bash
mkdir -p .artifacts/search && ctx7 get-upgrade-guide /<owner>/<repo> <installed version> <target version> --page <n> > .artifacts/search/<repo>@<installed>-<target>.<n>.md
rg -n '^#{2,3} ' .artifacts/search/<repo>@<installed>-<target>.<n>.md
mq -A 'import "section" | section::section(self, "<section title>", 2) | map(section::all_nodes) | flatten' .artifacts/search/<repo>@<installed>-<target>.<n>.md
```

.NET newest NuGet version, prereleases included, with its publish date, `<id>` lowercase:

```bash
version=$(xh -b https://api.nuget.org/v3-flatcontainer/<id>/index.json | jq -r '.versions[-1]')
xh -b "https://api.nuget.org/v3/registration5-gz-semver2/<id>/$version.json" | jq -c --arg version "$version" '{version: $version, published: .published[:10]}'
```

GitHub advisories on pinned versions with each patched version, one call across ecosystems, owner and repo name the current repository:

```text
check_dependency_vulnerabilities {"owner": "<owner>", "repo": "<repo>", "dependencies": [{"ecosystem": "nuget", "name": "<id>", "version": "<version>"}, {"ecosystem": "npm", "name": "<pkg>", "version": "<version>"}, {"ecosystem": "pip", "name": "<pkg>", "version": "<version>"}]}
```

TypeScript newest version per dist-tag (`latest`, `next`, `beta`, `rc`) and publish date of one version:

```bash
pnpm view <pkg> dist-tags time --json | jq -c '{tags: .["dist-tags"], published: .time["<version>"][:10]}'
```

Python versions:
- Lock version beside the newest, prereleases per `pyproject`
- Newest stable version, its upload time, and its requirements from the registry, a versionless URL describes newest stable

```bash
uv tree --frozen --outdated --package <pkg> --depth 0
xh https://pypi.org/pypi/<pkg>/json | jq -r '.info.version, .urls[0].upload_time, .info.requires_dist[]?'
```

Rust newest version on crates.io:

```bash
cargo search <crate> --limit 1
```

GitHub release notes body alone, `Not Found` means no release exists for the tag and its changelog holds release notes:

```bash
gh api "repos/<owner>/<repo>/releases/tags/<tag>" --jq '.body'
```

## [05]-[SURVEY]

Reads across many repositories run under an installation token of the owner's research App, which holds a quota apart from the account's:
- Token: `gh-token` issues one from `GITHUB_APP_ID`, `GITHUB_APP_INSTALLATION_ID`, and `GITHUB_APP_PRIVATE_KEY`, valid one hour
- Reach: contents and metadata of public repositories and the owner's own, read-only
- Quota: 5,000 REST calls and 5,000 GraphQL points an hour, 30 searches and 10 code searches a minute, the sizes of the account pool
- Batch: one GraphQL query aliases up to 100 `repository` fields for 1 point, where REST spends a call per repository and endpoint
- Account: writes, projects, notifications, and the GitHub MCP stay on `GH_TOKEN`

```bash
# Remaining quota of each resource under the token
GH_TOKEN="$(gh-token generate --app-id "$GITHUB_APP_ID" --installation-id "$GITHUB_APP_INSTALLATION_ID" --key <(printf '%s' "$GITHUB_APP_PRIVATE_KEY") --token-only)" \
    gh api rate_limit --jq '.resources | map_values(.remaining)'

# Fields of up to 100 repositories, one `<owner>/<repo>` per line of <file>
jq -Rrn '[inputs | split("/") as [$o, $n] | "r\(input_line_number): repository(owner: \($o | tojson), name: \($n | tojson)) { ...R }"]
    | "query { \(join(" ")) } fragment R on Repository { nameWithOwner stargazerCount pushedAt isArchived licenseInfo { spdxId } latestRelease { tagName publishedAt } }"' <file> \
    | GH_TOKEN="$(gh-token generate --app-id "$GITHUB_APP_ID" --installation-id "$GITHUB_APP_INSTALLATION_ID" --key <(printf '%s' "$GITHUB_APP_PRIVATE_KEY") --token-only)" \
    gh api graphql -F query=@- --jq '.data[]'
```
