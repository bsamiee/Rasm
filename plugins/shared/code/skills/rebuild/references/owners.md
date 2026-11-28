# [OWNERS]

Owner commands list a language's owners whole and rank them by a job's words, `<project>` naming the project folder, `<scratch>` the session scratch folder, and `<job words>` the verb, object, and format of a hand-written job.

## [01]-[TYPESCRIPT]

Owners: Effect and its runtime platform package, Node built-ins on the pinned Node, host declaration files, `@rasm/*` libraries, then pnpm's catalog:
1. Version through a project's own link, as the store holds older and differently patched copies
2. Local patches with their reasons
3. Agent guide of a package
4. Effect module index with each module's summary line, run again per subpath the runtime reaches (`ai`, `http`, `process`, `sql`)
5. Every Effect member written to a file, renamed exports (`try`, `catch`, `Array`) included
6. Every export of any other package, its declaration files followed through each `export *` and brace list to the file declaring the member
7. Installed declarations searched by job words, ranked by package
8. Catalog rows with the group each serves
9. Node built-ins before any shell-out or text parse

```bash
jq -r .version <project>/node_modules/<package>/package.json
yq '.patchedDependencies' pnpm-workspace.yaml
fd -d 1 '^(AGENTS\.md|llms.*\.txt|ai-docs)$' <project>/node_modules/<package>
fd -d 1 -e ts -E index.ts . <project>/node_modules/effect/src/<subpath> -X awk 'FNR == 1 { f = 0 } FNR == 1 && /^\/\*\*/ { f = 1; next } f && /^ \* [A-Z`]/ { sub(/^ \* /, ""); print FILENAME "\t" $0; f = 0 }'
rg -oN --glob '*.d.ts' --glob '!**/internal/**' '^export (?:declare )?(?:const|function|class|abstract class|interface|type|namespace|enum) ([\w$]+)|^\s*[\w$]+ as ([\w$]+) \}' -r '$1$2' <project>/node_modules/effect/dist | sed 's|.*/dist/||; s|\.d\.ts:|.|' | sort -u > <scratch>/inventory/effect.txt
rg -l -i '<job words>' node_modules/.pnpm --glob '*.d.ts' --glob '!**/internal/**' | sed -E 's|node_modules/.pnpm/[^/]+/node_modules/||' | awk -F/ '{ print ($1 ~ /^@/ ? $1 "/" $2 : $1) }' | sort | uniq -c | sort -rn
awk '/^catalog:/ { f = 1; next } f && /^[a-z]/ { exit } f && /# --- \[/ { s = $3; next } f && /^ +[^ #]/ { print s "\t" $0 }' pnpm-workspace.yaml
node -p "require('node:module').builtinModules.join('\n')"
```

Host declarations are read as every file a host writes, as later files augment the module a project types. Adobe hosts read Atelier's `atelier://object-model/<host>` resource in place of a declaration file.

## [02]-[PYTHON]

Owners: standard library, `expression`, `msgspec`, `anyio`, and dependency groups of the uv root holding a target, as host roots (`libs/python/blender`) resolve their own versions, `<python>` naming that root's interpreter:
1. Version that root locks
2. Standard library modules before any hand-written parse, process call, or path code
3. Every module of a package with members and signatures written to a file, as `pydoc <package>` alone lists the top module
4. Dependency groups read whole with their comments
5. Installed sources and stubs searched by job words, ranked by package, past `.venv/.gitignore`

```bash
uv tree --frozen --package <package> --depth 0
<python> -I -c "import sys; print(*sorted(sys.stdlib_module_names), sep='\n')"
<python> -I -c "import pkgutil, importlib, sys; p = importlib.import_module(sys.argv[1]); print(p.__name__, *[m.name for m in pkgutil.walk_packages(getattr(p, '__path__', []), p.__name__ + '.') if '._' not in m.name], sep='\n')" <module> | xargs <python> -I -m pydoc > <scratch>/inventory/<module>.txt
sed -n '/^\[dependency-groups\]/,/^\[tool/p' <uv-root>/pyproject.toml
rg -l -i --no-ignore '<job words>' <uv-root>/.venv/lib/python*/site-packages --glob '*.py' --glob '*.pyi' | sed -E 's|.*/site-packages/||' | cut -d/ -f1 | sort | uniq -c | sort -rn
```

## [03]-[CSHARP]

Owners: BCL, `LanguageExt.Core`, Thinktecture, Mapperly, and RhinoCommon, Grasshopper, and Eto for Rhino targets, then the central catalog:
1. Version the project restored, as catalog rows name MSBuild properties (`$(RhinoVersion)`) and conditional `Update` rows
2. Packages folder from NuGet, as `NuGet.config` moves it into the repository cache
3. Every public type, documented or not, as XML documentation omits a fifth of public types
4. Every public member written to a file from the decompiled assembly
5. BCL members from the reference pack's documentation ids before any hand-written collection, text, or time code
6. Members taking or returning the job's type, as extension and `Prelude` members sit outside that type
7. Catalog groups and rows with their comments, `Transitive Pinning` holding pins and no candidates
8. Package documentation searched by job words, ranked by package

```bash
jq -r '.libraries | keys[] | select(startswith("<Id>/"))' .artifacts/dotnet/obj/<Project>/project.assets.json
dotnet nuget locals global-packages --list
ilspycmd --dump-table TypeDef --json <dll> | jq -r '.rows[] | select(.Attributes | test("(^|, )(Public|NestedPublic)")) | .Namespace + "." + .Name'
ilspycmd -p -o .cache/ilspy/<Name>/<Version> <dll> && rg -n '^\s*public ' .cache/ilspy/<Name>/<Version> > <scratch>/inventory/<Name>.txt
rg -oI 'name="[TM]:[^"]+' "$(dirname "$(readlink -f "$(command -v dotnet)")")/packs/Microsoft.NETCore.App.Ref/<version>/ref/net<major>.0" --glob '*.xml'
rg -o 'M:[\w.`]+\.\w+\([^)]*<Type>[^)]*\)' <packages>/<id>/<version>/lib/<tfm>/<assembly>.xml | sort -u
rg -n 'Label=|Version=' Directory.Packages.props
rg -l -i '<job words>' <packages> --glob '*.xml' | sed -E 's|^<packages>/||' | cut -d/ -f1 | sort | uniq -c | sort -rn
```

Use `dotnet:codelens` for one namespace's members with signatures and summaries in place.

## [04]-[RUST]

Owners: `core`, `alloc`, and `std` at the pinned nightly, then `[workspace.dependencies]`:
1. Version the workspace pins, as `Cargo.lock` holds more than one version of a crate
2. Workspace dependency table read whole with its group headings and comments
3. Every item of a crate with re-exports inlined and the workspace's features applied, as facade crates declare nothing themselves
4. Standard library source searched by job words at the pinned commit

```bash
yq -p toml -oy '.workspace.dependencies.<crate>' Cargo.toml
sed -n '/^\[workspace.dependencies\]/,/^\[workspace.lints/p' Cargo.toml
cargo doc --locked --no-deps -p <crate>@<version> && rg -o 'href="[^"]+\.html">[^<]+' .artifacts/cargo/doc/<crate_name>/all.html > <scratch>/inventory/<crate>.txt
rg -l -i '<job words>' .cargo/rustc-source/library/core/src .cargo/rustc-source/library/alloc/src .cargo/rustc-source/library/std/src --glob '*.rs'
```

Trait methods sit on each trait's page or source, as `all.html` lists items alone.

## [05]-[SWIFT]

Owners: Swift's standard library and compiler, Foundation, AppKit, Darwin, other SDK frameworks, repository modules and `SPI`, then root `Package.swift` dependencies. macOS has no package catalog, and most jobs a Swift file hand-writes sit in the SDK:
1. SDK frameworks, Swift modules, and Darwin headers ranked by match counts of the job's words
2. Swift interface of a framework, as overlays and Swift-only modules hold API no Clang synthesis prints
3. Clang interface of a framework as Swift, submodules included
4. Repository modules, each a candidate before new code
5. Public declarations of a package dependency with modules it re-exports, from the checkout Xcode resolved
6. Exports of a private framework `SPI` links, compared with what its header declares

```bash
SDK="$(xcrun --show-sdk-path)"
rg -ic '<job words>' "$SDK"/System/Library/Frameworks/*/{Headers,Frameworks/*/Headers,Modules/*.swiftmodule} "$SDK"/usr/lib/swift "$SDK"/usr/include | sed -E 's#.*/([^/]+)\.(framework|swiftmodule)/[^:]*:#\1 #; s#.*/usr/include/([^/:]+)[^:]*:#\1 #' | awk '{ n[$1] += $2 } END { for (k in n) print n[k], k }' | sort -rn
fd -e swiftinterface arm64e-apple-macos "$SDK/System/Library/Frameworks/<Framework>.framework" "$SDK/usr/lib/swift/<Module>.swiftmodule"
xcrun swift-synthesize-interface -module-name <Framework> -sdk "$SDK" -target "arm64-apple-macos$(xcrun --show-sdk-version)" -include-submodules > <scratch>/inventory/<Framework>.swift
swift package dump-package | jq -r '.targets[] | select(.type != "test") | .name + " " + (.path // "")'
rg -n '^\s*(@_exported import|public |open )' .cache/nx/workspace-data/xcode-packages/Rasm.xcworkspace/checkouts/<package>/Sources
dyld_info -exports /System/Library/PrivateFrameworks/<Framework>.framework/<Framework>
```

Compiler synthesis owns `Equatable`, `Hashable`, `Codable`, and memberwise initializers, and a hand-written `==`, coding key set, or initializer restating them goes.

## [06]-[CPP]

Owners: libc++ at the LLVM row of mise, read through its `std` module, then `vcpkg.json` ports at its `builtin-baseline`:
1. Every standard library name the `std` module exports, written to a file
2. Headers of libc++ searched by job words
3. Ports the catalog declares, and ports matching a job before a hand-written library

```bash
rg -oN 'using std::[\w:]+' "$(dirname "$(command -v clang++)")/../share/libc++/v1/std" > <scratch>/inventory/std.txt
rg -l -i '<job words>' "$(dirname "$(command -v clang++)")/../include/c++/v1"
jq '.dependencies, .features' vcpkg.json
vcpkg search <job words>
```

Use `cpp:unreal` for engine members of code running inside Unreal Engine.
