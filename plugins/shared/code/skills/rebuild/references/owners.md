# [OWNERS]

Owner reads run in order, and each read decides what the next one opens:
1. Version: release a project resolves through its own link or lock row, as stale and differently patched copies sit beside it
2. Patches: local patches of a package with their reasons, as they change behavior its documentation describes
3. Guide: a package's own agent guide (`AGENTS.md`, `llms.txt`) read whole, and each `ai-docs/` topic it links for a job
4. Built-ins: standard library and runtime modules listed before any hand-written parse, process call, path, collection, text, or time code
5. Catalog: ecosystem catalog read whole with its group headings and comments
6. Index: each owner's module, namespace, or framework list read whole
7. Search: each job's words searched across every installed declaration of the ecosystem, ranked by package, store copies read for discovery alone
8. Inventory: each candidate's whole member set written to a file before any member is read by name
9. Member: one member read from its doc comment's first line to its declaration's end

Each read replaces one bad form:

| [INDEX] | [FOUND]                                                                                 | [READ]                                |
| :-----: | :-------------------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | Member or semantic (concurrency, limit, depth) recalled from memory or an older release | Version, then Member                  |
|  [02]   | Candidates limited to modules a target imports                                          | Index                                 |
|  [03]   | Empty search read as leave to hand-write                                                | Index above it, each group read whole |
|  [04]   | Catalog searched by one word                                                            | Catalog                               |
|  [05]   | Inventory capped, sampled, or listed from symbol names                                  | Inventory                             |
|  [06]   | Package skill standing for an inventory                                                 | Inventory, skill for forms alone      |

Commands name `<project>` for a project folder, `<scratch>` for a session scratch folder, and `<job words>` for a hand-written job's verb, object, and format.

## [01]-[TYPESCRIPT]

Owners: Effect and its runtime platform package, built-ins of the runtime consumers bundle into (Node, Bun under Claude Code hooks, UXP), host declaration files, `@rasm/*` libraries, then pnpm's catalog:
- Index: Effect's module summaries, run again per subpath the runtime reaches (`ai`, `http`, `process`, `sql`)
- Inventory: every Effect member with renamed exports (`try`, `catch`, `Array`), other packages followed through each `export *` and brace list

```bash
jq -r .version <project>/node_modules/<package>/package.json
yq '.patchedDependencies' pnpm-workspace.yaml
fd -d 1 '^(AGENTS\.md|llms.*\.txt|ai-docs)$' <project>/node_modules/<package>
fd -d 1 -e ts -E index.ts . <project>/node_modules/effect/src/<subpath> -X awk 'FNR == 1 { f = 0 } FNR == 1 && /^\/\*\*/ { f = 1; next } f && /^ \* [A-Z`]/ { sub(/^ \* /, ""); print FILENAME "\t" $0; f = 0 }'
rg -oN --glob '*.d.ts' --glob '!**/internal/**' '^export (?:declare )?(?:const|function|class|abstract class|interface|type|namespace|enum) ([\w$]+)|^\s*[\w$]+ as ([\w$]+) \}' -r '$1$2' <project>/node_modules/effect/dist | sed 's|.*/dist/||; s|\.d\.ts:|.|' | sort -u > <scratch>/effect.txt
rg -l -i '<job words>' node_modules/.pnpm --glob '*.d.ts' --glob '!**/internal/**' | sed -E 's|node_modules/.pnpm/[^/]+/node_modules/||' | awk -F/ '{ print ($1 ~ /^@/ ? $1 "/" $2 : $1) }' | sort | uniq -c | sort -rn
awk '/^catalog:/ { f = 1; next } f && /^[a-z]/ { exit } f && /# --- \[/ { s = $3; next } f && /^ +[^ #]/ { print s "\t" $0 }' pnpm-workspace.yaml
node -p "require('node:module').builtinModules.join('\n')"
```

Adobe hosts read Atelier's `atelier://object-model/<host>` resource in place of a declaration file.

## [02]-[PYTHON]

Owners: standard library, `expression`, `msgspec`, `anyio`, and dependency groups of the uv root holding a target, as host roots (`libs/python/blender`) resolve their own versions, `<uv-root>` naming that root and `<python>` its interpreter:
- Inventory: every module of a package with members and signatures, as `pydoc <package>` alone lists the top module
- Search: `--no-ignore` reaches installed sources and stubs, as `.venv/.gitignore` ignores every file

```bash
uv tree --frozen --project <uv-root> --package <package> --depth 0
<python> -I -c "import sys; print(*sorted(sys.stdlib_module_names), sep='\n')"
<python> -I -c "import pkgutil, importlib, sys; p = importlib.import_module(sys.argv[1]); print(p.__name__, *[m.name for m in pkgutil.walk_packages(getattr(p, '__path__', []), p.__name__ + '.') if '._' not in m.name], sep='\n')" <module> | xargs <python> -I -m pydoc > <scratch>/<module>.txt
sed -n '/^\[dependency-groups\]/,/^\[tool/p' <uv-root>/pyproject.toml
rg -l -i --no-ignore '<job words>' <uv-root>/.venv/lib/python*/site-packages --glob '*.py' --glob '*.pyi' | sed -E 's|.*/site-packages/||' | cut -d/ -f1 | sort | uniq -c | sort -rn
```

## [03]-[CSHARP]

Owners: BCL, `LanguageExt.Core`, Thinktecture, Mapperly, and RhinoCommon, Grasshopper, and Eto for Rhino targets, then the central catalog:
- Version: the one a project restored, as catalog rows name MSBuild properties (`$(RhinoVersion)`) and conditional `Update` rows
- Packages: folder NuGet reports, as `NuGet.config` moves it into the repository cache
- Types: every public type, documented or not, as XML documentation lists documented types alone
- Built-ins: BCL members from the reference pack's documentation ids
- Members: each member taking or returning a job's type, read from its inventory, as extension and `Prelude` members sit outside the type
- Catalog: `Transitive Pinning` holds pins and no candidates

```bash
jq -r '.libraries | keys[] | select(startswith("<Id>/"))' .artifacts/dotnet/obj/<Project>/project.assets.json
dotnet nuget locals global-packages --list
ilspycmd --dump-table TypeDef --json <dll> | jq -r '.rows[] | select(.Attributes | test("(^|, )(Public|NestedPublic)")) | .Namespace + "." + .Name'
ilspycmd -p -o .cache/ilspy/<Name>/<Version> <dll> && rg -n '^\s*public ' .cache/ilspy/<Name>/<Version> > <scratch>/<Name>.txt
rg -oI 'name="[TM]:[^"]+' "$(dirname "$(readlink -f "$(command -v dotnet)")")/packs/Microsoft.NETCore.App.Ref/<version>/ref/net<major>.0" --glob '*.xml'
rg -w '<Type>' <scratch>/<Name>.txt
rg -n 'Label=|Version=' Directory.Packages.props
rg -l -i '<job words>' <packages> --glob '*.xml' | sed -E 's|^<packages>/||' | cut -d/ -f1 | sort | uniq -c | sort -rn
```

Use `dotnet:codelens` for one namespace's members with signatures and summaries in place.

## [04]-[RUST]

Owners: `core`, `alloc`, and `std` at the pinned nightly, then `[workspace.dependencies]`:
- Version: the workspace pin, as `Cargo.lock` holds more than one version of a crate
- Inventory: every item of a crate with re-exports inlined and workspace features applied, as facade crates declare nothing themselves

```bash
yq -p toml -oy '.workspace.dependencies.<crate>' Cargo.toml
sed -n '/^\[workspace.dependencies\]/,/^\[workspace.lints/p' Cargo.toml
cargo doc --locked --no-deps -p <crate>@<version> && rg -o 'href="[^"]+\.html">[^<]+' .artifacts/cargo/doc/<crate_name>/all.html > <scratch>/<crate>.txt
rg -l -i '<job words>' .cargo/rustc-source/library/core/src .cargo/rustc-source/library/alloc/src .cargo/rustc-source/library/std/src --glob '*.rs'
```

Trait methods sit on each trait's page or source, as `all.html` lists items alone.

## [05]-[SWIFT]

Owners: Swift's standard library and compiler, Foundation, AppKit, Darwin, other SDK frameworks, repository modules and `SPI`, then root `Package.swift` dependencies. macOS has no package catalog, and most jobs a Swift file hand-writes sit in the SDK:
- Swift interface: a framework's own, as overlays and Swift-only modules hold API no Clang synthesis prints
- Clang interface: a framework's headers as Swift, submodules included
- Repository modules: each a candidate before new code
- Package dependency: public declarations with modules it re-exports, from the checkout Xcode resolved
- Private framework: exports `SPI` links, compared with what its header declares

```bash
SDK="$(xcrun --show-sdk-path)"
rg -ic '<job words>' "$SDK"/System/Library/Frameworks/*/{Headers,Frameworks/*/Headers,Modules/*.swiftmodule} "$SDK"/usr/lib/swift "$SDK"/usr/include | sed -E 's#.*/([^/]+)\.(framework|swiftmodule)/[^:]*:#\1 #; s#.*/usr/include/([^/:]+)[^:]*:#\1 #' | awk '{ n[$1] += $2 } END { for (k in n) print n[k], k }' | sort -rn
fd -e swiftinterface arm64e-apple-macos "$SDK/System/Library/Frameworks/<Framework>.framework" "$SDK/usr/lib/swift/<Module>.swiftmodule"
xcrun swift-synthesize-interface -module-name <Framework> -sdk "$SDK" -target "arm64-apple-macos$(xcrun --show-sdk-version)" -include-submodules > <scratch>/<Framework>.swift
swift package dump-package | jq -r '.targets[] | select(.type != "test") | .name + " " + (.path // "")'
rg -n '^\s*(@_exported import|public |open )' .cache/nx/workspace-data/xcode-packages/Rasm.xcworkspace/checkouts/<package>/Sources
dyld_info -exports /System/Library/PrivateFrameworks/<Framework>.framework/<Framework>
```

## [06]-[CPP]

Owners: libc++ at the LLVM row of mise, read through its `std` module, then `vcpkg.json` ports at its `builtin-baseline`, and `vcpkg search` lists ports outside it before any hand-written library:

```bash
rg -oIN 'using std::[\w:]+' "$(dirname "$(command -v clang++)")/../share/libc++/v1/std" > <scratch>/std.txt
rg -l -i '<job words>' "$(dirname "$(command -v clang++)")/../include/c++/v1"
jq '.dependencies, .features' vcpkg.json
vcpkg search <job words>
```

Use `cpp:unreal` for engine members of code running inside Unreal Engine.
