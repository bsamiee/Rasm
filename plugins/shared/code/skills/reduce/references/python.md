# [PYTHON]

Python targets hold `.py` and `.pyi` files under each uv root `git ls-files '*uv.lock'` lists and PEP 723 plugin scripts beside their `.py.lock`, `<uv-root>` naming the root holding a target, `<package>` a folder holding `__init__.py`, `<paths>` files a pass touches, `<roots>` the folders `libs products plugins tools tests`, `<main>` the repository root, and `<dir>` session scratch folder.

## [01]-[SKILLS]

Import roots of a target decide package rows, and path or host API decides host rows:

```bash
rg -oNI '^\s*(?:lazy )?(?:from|import) (\w+)' -r '$1' -g '*.py' <target> | sort | uniq -c | sort -rn
```

| [INDEX] | [TARGET]           | [SKILLS]             | [CONDITION]                                             |
| :-----: | :----------------- | :------------------- | :------------------------------------------------------ |
|  [01]   | Any Python         | `python:coding`      | `.py` or `.pyi` file                                    |
|  [02]   | Results and chains | `python:expression`  | Imports `expression`                                    |
|  [03]   | Records and codecs | `python:msgspec`     | Imports `msgspec`                                       |
|  [04]   | Boundary models    | `python:pydantic`    | Imports `pydantic` or `pydantic_settings`               |
|  [05]   | Command line       | `python:cyclopts`    | Imports `cyclopts`                                      |
|  [06]   | Telemetry          | `python:telemetry`   | Imports `structlog` or `opentelemetry`                  |
|  [07]   | Docstrings         | `python:document`    | Pass writing a declaration, ruff `D` or `DOC` finding   |
|  [08]   | Blender            | `blender:coding`     | Imports `bpy`, file under a Blender uv root             |
|  [09]   | Blender extension  | `blender:extensions` | `blender_manifest.toml`, `register`, `AddonPreferences` |
|  [10]   | Blender interface  | `blender:interface`  | `Panel`, `Menu`, `UIList`, `gpu`, `blf`, draw handlers  |
|  [11]   | Blender nodes      | `blender:nodes`      | `NodeTree`, node interface sockets                      |
|  [12]   | Blender output     | `blender:render`     | `RenderSettings`, compositor, sequencer, `RenderEngine` |
|  [13]   | Tests              | `code:test`          | Suite under `tests/`, as no Python testing skill exists |

## [02]-[SURVEY]

Signals over `<paths>`, one command per step:
1. Size: lines and branching per file, largest first
2. Clones: jscpd AST pairs across `<roots>` touching the target, ranked by lines times similarity
3. Dependents: projects depending on the target's Nx project, every uv root in one graph file
4. Declarations: module-level items with their class members per file, as a count skipping members passes a move into a class as a cut
5. Exports: names each `__all__` lists per file
6. Levels: package-internal names, listed by a private module's `__all__` and left out of its package entry, then file-private `_` names per file
7. Private readers: file-private module-level declarations with one reader in their file
8. Name pairs: declaration and member names differing by case or underscore, each group judged, as a class and its instance share a name by design
9. Nesting: block depth per site past one level, deepest first

```bash
jscpd --config .config/jscpd.json --complexity --reporters json --output <dir>/jscpd <paths> &&
    jq -r --arg r "$PWD/" '.summary.files[] | [.lines, .complexity, (.path | ltrimstr($r))] | @tsv' <dir>/jscpd/jscpd-complexity.json | sort -rn
jscpd --config .config/jscpd.json --similarity 0.6 --min-nodes 60 --format python --reporters json --output <dir>/jscpd <roots>;
    jq -r --arg t "$PWD/<target>/" '[.duplicates[] | select((.firstFile.name | startswith($t)) or (.secondFile.name | startswith($t)))] | sort_by(-(.lines * .similarity))[]
    | "\(.lines)\t\(.similarity)\t\(.firstFile.name):\(.firstFile.start)\t\(.secondFile.name):\(.secondFile.start)"' <dir>/jscpd/jscpd-report.json
nx graph --file=<dir>/graph.json &&
    jq -r --arg p '<nx-project>' '.graph.dependencies | to_entries[] | select(any(.value[]; .target == $p)) | .key' <dir>/graph.json
ast-grep outline <paths> --items structure --view expanded --json=stream | jq -r '"\([.items[] | select(.isImport | not) | 1 + (.members // [] | length)] | add // 0)\t\(.path)"'
rg -U --json '^__all__ = \[[^\]]*\]' <paths> | jq -r 'select(.type == "match") | [(.data.lines.text | [scan("\"[^\"]+\"")] | length), .data.path.text] | @tsv'
comm -13 <(rg -oUIN '^__all__ = \[[^\]]*\]' <package>/__init__.py | rg -o '"\w+"' | sort -u) \
    <(rg -oUIN '^__all__ = \[[^\]]*\]' -g '*.py' -g '!__init__.py' <package> | rg -o '"\w+"' | sort -u) &&
    rg -cN '^(?:(?:async )?def|class|type) _[A-Za-z0-9]|^_[A-Za-z0-9]\w*(?:: [^=]+)? = ' <paths>
comm -12 <(rg -oN --no-heading -r '$1$2' '^(?:(?:async )?def|class|type) (_[A-Za-z0-9]\w*)|^(_[A-Za-z0-9]\w*)(?:: [^=]+)? = ' <paths> | sort -u) \
    <(rg -owN --no-heading '_[A-Za-z0-9]\w*' <paths> | sort | uniq -c | awk '$1 == 2 { print $2 }' | sort -u)
ast-grep outline <paths> --items structure --view expanded --json=stream | jq -r '.items[] | .name, (.members[]?.name) | select(startswith("__") | not)' | sort -u |
    awk '{ k = tolower($0); gsub(/_/, "", k); g[k] = g[k] " " $0; n[k]++ } END { for (k in g) if (n[k] > 1) print g[k] }'
ruff check --select PLR1702 --config 'lint.pylint.max-nested-blocks=1' --output-format json <paths> |
    jq -r --arg r "$PWD/" '.[] | [(.message | capture("\\((?<n>[0-9]+) > ").n), (.filename | ltrimstr($r)), .location.row] | @tsv' | sort -rn
```

Commands exit by their own rules:
- Clones: jscpd exits 1 whenever a pair prints, and the ranking reads its report regardless
- Levels: one run per package entry, a package holding public subpackages running once per subpackage entry
- Nesting: ruff `PLR1702` counts `if`, `for`, `while`, `try`, `with`, and `match` blocks, exiting 1 on a site past `max-nested-blocks`
- Limits: ast-grep `no-fourth-nesting-level-python` holds committed code to 3 levels of control flow, conditional expressions, and closures inside control flow
- Closures: a multi-statement local `def` or a conditional `lambda` counts a level neither counter reads outside control flow, found by a whole read

## [03]-[SOURCES]

Package reads, one command per step, `<python>` being `<uv-root>/.venv/bin/python -I`:
1. Version: release each uv root locks, as roots resolve one package apart (`pydantic`, `numpy`), and an empty tree means the root lacks it
2. Guide: documentation URL of the installed distribution, read through `search:web`, as wheels hold no agent guide
3. Index: import name of a distribution, then its public modules with their summary lines from step 4
4. Inventory: `pydoc` of each chosen module into one file under `### <module>` headings, counted by member lines, inherited members included
5. Member: source of one member, its typed signature from a `.pyi` stub through `search:code` where a compiled module declares none
6. Standard library: `builtins` and each module `sys.stdlib_module_names` lists through steps 3 and 4, built-in type members (`str`, `dict`, `tuple`) included
7. Host: every RNA property and function of `bpy.types` from the installed Blender, as a Blender root's `bpy` wheel can trail it
8. Workspace: each `rasm.*` module the target imports, resolved from its uv root to a source file read whole
9. Catalog: Blender root candidates in order of group `blender` (what Blender bundles), member `dependencies`, then a root catalog row with a 3.13 wheel on every `required-environments` platform

```bash
(cd <uv-root> && uv tree --frozen --invert --package <dist> --depth 1)
<python> -c "import sys; from importlib.metadata import metadata; m = metadata(sys.argv[1]); print(m['Version'], *m.get_all('Project-URL') or ())" <dist>
<python> -c "import sys, re; from importlib.metadata import packages_distributions as p; n = lambda s: re.sub(r'[-_.]+', '-', s).lower(); print(*sorted(m for m, d in p().items() if n(sys.argv[1]) in map(n, d)))" <dist>
<python> -c "import importlib, pkgutil, sys; root = importlib.import_module(sys.argv[1]); print(root.__name__, *(m.name for m in pkgutil.walk_packages(getattr(root, '__path__', ()), f'{root.__name__}.') if '._' not in m.name), sep='\n')" <module> > <dir>/modules-<module>.txt
while read -r m; do printf '### %s\n' "$m"; <python> -m pydoc "$m"; done < <dir>/modules-<module>.txt > <dir>/inventory-<module>.txt && rg -c '^    (class )?\w+\(|^ \|  \w+\(' <dir>/inventory-<module>.txt
<python> -m inspect <module>:<qualname>
<python> -c "import sys; print(*sorted(sys.stdlib_module_names), sep='\n')"
blender --background --factory-startup --python-exit-code 1 --python-expr "import bpy; print(*(f'{n}\t{kind}\t{m.identifier}\t{m.description}' for n in dir(bpy.types) for rna in [getattr(getattr(bpy.types, n), 'bl_rna', None)] if rna for kind, members in (('property', rna.properties), ('function', rna.functions)) for m in members), sep='\n')" | rg '\t(property|function)\t' > <dir>/inventory-bpy.tsv
rg -oNI '^\s*(?:lazy )?from (rasm(?:\.\w+)+) import' -r '$1' -g '*.py' <target> | sort -u | xargs <python> -c "import importlib.util, sys; print(*(f'{n}\t{importlib.util.find_spec(n).origin}' for n in sys.argv[1:]), sep='\n')"
```

Reads hold by root:
- Interpreter: `<python>` reads the exact release its root installs, as `uv pip list` reads `VIRTUAL_ENV`, which mise points at the root `.venv` in every shell
- Catalog: root `default-groups = "all"` installs every root catalog row, so step 4 reads any root candidate with no install
- Imports: steps 3, 4, and 6 import each module, and a module whose optional dependency is missing prints its error in the inventory and stays unread
- Blender: modules importing `bpy` load inside Blender alone, read through step 7 and the Blender root's `bpy` stubs

## [04]-[CONSUMERS]

Readers and leads across `<roots>`, one command per step:
1. Readers: reader sites by symbol identity from `ty server`, one run from each uv root a reader sits in, as root ty excludes both Blender roots
2. Importers: files importing each module of the unit, a package entry's importers standing for its private modules' readers
3. Text: sites ty never reports, spelling a name as a string (`__all__`, `bl_idname`, `bpy.ops.<category>.<name>`, wire tags, entry points, `importlib`)
4. Duplicates by name: class and `type` names declared in more than one project, each pair read whole before a merge
5. Mentions: removed or renamed name across the repository with no type filter, docs, skills, and manifests included
6. Leads: `unread:` declarations step 1 prints for a module run with no symbol, read as leads alone, as a reader can sit in another uv root

```bash
(cd <uv-root> && node <main>/plugins/shared/code/skills/reduce/scripts/readers.ts <file> [<symbol>...])
ruff analyze graph --direction dependents <roots> | jq -r '[.[$ARGS.positional[]][]?] - $ARGS.positional | unique[]' --args <paths>
rg -nF '<name>' -g '*.py' -g '*.toml' <roots>
rg -oN --no-heading -r '$1' '^(?:class|type) ([A-Z]\w*)' -g '*.py' <roots> | awk -F: -v roots="$(git ls-files '*pyproject.toml' | xargs -n1 dirname | tr '\n' ' ')" '
    BEGIN { n = split(roots, r, " ") } { p = "."; for (i = 1; i <= n; i++) if (index($1, r[i] "/") == 1 && length(r[i]) > length(p)) p = r[i]; held[$2 "\t" p] = 1 }
    END { for (k in held) { split(k, f, "\t"); c[f[1]]++; g[f[1]] = g[f[1]] " " f[2] } for (k in c) if (c[k] > 1) print c[k] "\t" k "\t" g[k] }' | sort -rn
rg -nw '<name>' .
```

Commands read by their own rules:
- Roots: `uv tree --invert` of a library's distribution names each root holding it, and a root library (`rasm.records`) takes a run from each Blender root
- Rows: readers writes rows under `.artifacts/code/readers/` of its working folder, and a file outside that uv root writes them along its `..` path, deleted after the read
- Entries: a package entry holds `__all__` and imports alone, so readers runs over the private module declaring a name
- Graph: ruff resolves every uv root and plugin script by module identity, `lazy` imports included

## [05]-[FORMS]

Python forms of each category, each rebuild holding under its stated condition alone, each source the skill stating the form, a survey signal, or the commit of a past pass. Lints denied by `hk fix` and `check--refurb` guide fixes inside a pass and find no committed site: ruff `PLW0108` (`lambda x: f(x)`) and `PERF401` (loop `append`), ast-grep `no-forwarding-property`, `no-alias-declaration`, and `no-forwarding-module`, and refurb `RASM102` (restated defaults), `RASM107` (`chain` flattening), and `RASM108` (`MappingProxyType` on 3.15). The largest cuts replaced a copy of host data with the host's own type (`40c61c310` removed 19,639 lines against 17,767 added), so records restating RNA, sockets, or package types rank first:

| [INDEX] | [CATEGORY]   | [FOUND]                                        | [REBUILD]                                       | [SOURCE]            |
| :-----: | :----------- | :--------------------------------------------- | :---------------------------------------------- | :------------------ |
|  [01]   | Redundant    | Record restating host settings or sockets      | Host type read through RNA                      | `40c61c310`         |
|  [02]   | Redundant    | Result bridge or fault type per layer          | One `Fault`, `Reason` enum per package          | `148e59670`         |
|  [03]   | Redundant    | Record one producer fills, one reader formats  | Values at its reader, record deleted            | `3eb6d39c1`         |
|  [04]   | Redundant    | Parameter no implementation reads              | Removed from every implementation and caller    | `3eb6d39c1`         |
|  [05]   | Redundant    | Guard restating a decoded `Meta` bound         | Guard removed                                   | `python:msgspec`    |
|  [06]   | Redundant    | Variant a host cannot write                    | Variant and its readers removed                 | `a242ab1d7`         |
|  [07]   | Redundant    | `TYPE_CHECKING` block in a 3.15 root           | `lazy` import                                   | `python:coding`     |
|  [08]   | Duplicated   | Generator trampoline over `send` and a stack   | `reached` walk and one comprehension of changes | `3eb6d39c1`         |
|  [09]   | Duplicated   | `next(...)` linear search per lookup           | One `dict` built once, read by key              | `3eb6d39c1`         |
|  [10]   | Duplicated   | Registration state a registry member holds     | Registry member (`Classes(build=...)`)          | `3eb6d39c1`         |
|  [11]   | Duplicated   | `catch` of `AttributeError` per attribute      | `getattr` under `hasattr` in one comprehension  | `3eb6d39c1`         |
|  [12]   | Duplicated   | Regex word splitter per package                | `rasm.records.words`                            | `ced9b02e7`         |
|  [13]   | Duplicated   | Bodies near-identical across products          | One member of their field's library             | Clone survey        |
|  [14]   | Duplicated   | Value type or domain declared per project      | Package type, else one in its field's library   | Name survey         |
|  [15]   | Duplicated   | `try`/`except` per call site                   | `catch(f, exception=E)` with `map_error`        | `python:coding`     |
|  [16]   | Duplicated   | Loop appending values and faults to lists      | `accumulated`, `traverse`, `Block.choose`       | `python:expression` |
|  [17]   | Duplicated   | `match` per case restating a combinator        | `.map`, `.bind`, `.map_error`, `.merge()`       | `python:expression` |
|  [18]   | Duplicated   | First-match loop over sources                  | `Seq.of_iterable(...).try_pick`                 | `python:expression` |
|  [19]   | Types        | Static-method class with one module instance   | Members of its one consumer                     | `3eb6d39c1`         |
|  [20]   | Types        | Enum `__init__` storing fields                 | Member value, `_value_` typed                   | `a242ab1d7`         |
|  [21]   | Types        | Enum beside a tagged union naming each case    | `ClassVar` on each case                         | `a242ab1d7`         |
|  [22]   | Types        | Records per variant with a `kind: str` field   | Cases of one tagged `msgspec.Struct` base       | `python:coding`     |
|  [23]   | Types        | `@dataclass` holding a domain value            | Frozen `msgspec.Struct`                         | `python:coding`     |
|  [24]   | Types        | `Protocol` with one in-tree implementation     | That class                                      | `python:coding`     |
|  [25]   | Types        | `""` meaning absent                            | `T \| None` read through `option.of_optional`   | `ced9b02e7`         |
|  [26]   | Literals     | Property `X if self is Member else Y`          | Member value holding its datum                  | `a242ab1d7`         |
|  [27]   | Literals     | `StrEnum` restating Blender identifiers        | `rna_enums` literal                             | `blender:coding`    |
|  [28]   | Literals     | `name=` label restating its identifier         | `bpy.path.display_name(<key>)`                  | `python:coding`     |
|  [29]   | Literals     | Sentence spelled at a raise or fault site      | `Reason` member template, `Fault.of`            | `148e59670`         |
|  [30]   | Literals     | `str()` or `repr()` around a fault subject     | Value as it is, `shown` rendering it            | `python:coding`     |
|  [31]   | Literals     | Seconds, bytes, or degrees as a bare number    | `timedelta`, `pint` quantity                    | `python:coding`     |
|  [32]   | Pass-through | Private helper one function reads              | Body at its reader                              | `3eb6d39c1`         |
|  [33]   | Exports      | Private-module `__all__` name no sibling reads | `_` prefix, out of every `__all__`              | `python:coding`     |
|  [34]   | Nesting      | `if`/`elif` chain over `isinstance`            | `match` with class patterns                     | `python:coding`     |
|  [35]   | Nesting      | `match` over a property, then its value        | One tuple `match`, class patterns and guards    | `3eb6d39c1`         |
|  [36]   | Nesting      | Conditional `lambda` in a chain step           | `filter_with`, `or_else_with`, `default_with`   | `python:expression` |
|  [37]   | Nesting      | Local multi-statement `def` per chain step     | Flat `bind` chain, `pipeline`                   | `python:expression` |

Rows hold under these conditions:
- Versions: row [07] and lints `RASM107` and `RASM108` hold in the root uv root alone, as Blender roots run Blender's bundled 3.13, and a stub-only name (`rna_enums`) keeps `TYPE_CHECKING`
- Row [14] -> names one tool repeats by design (refurb checks, per-package `Reason` enums) are no finding, and a pair joins only where both state one value
- Row [20] -> a member holding more than one fact takes a tuple value or a property over its value, as `a242ab1d7` left `0.0 if operation is MaskOperation.ADD else 1.0` at a reader
- Row [23] -> handle owners needing `field(compare=False, repr=False)` keep a frozen `dataclass`
- Row [24] -> a `Protocol` typing host objects (`bpy` structs), a module passed as a value, or a callable `Callable` cannot spell stays
- Row [32] -> a `partial` target, a typed `def` under `@catch`, a `singledispatch` registration, a callback Blender calls by name, and a `[T]` relating arguments stay
- Row [37] -> a local `def` holding a type parameter, a `try` around an awaited call, or a closure over a loop value stays, as `catch` and `bind` span no `await`
- Host reads: a call with a host side effect (`animation_data_create()`, depsgraph tags) is a write, and a read before a create stays (`3eb6d39c1`)
- Constants: a literal more than one site spells stays one declaration, as `b43f9faa9` folded a keymap name into separate spellings
- Names: class, function, and member names a host or codec derives from (`tag=str.lower`, `bl_idname`, operator idnames, `__name__` keys) carry their wire or host key into a rename
- Names: past participle function names (`accumulated`, `relinked`) are the repository's Python form and stay through a names pass
- Docstrings: ruff `D` and `DOC` rules require them and `msgspec.json.schema` reads them as `description`, so none is redundant code

## [06]-[CHECK]

Check per pass, each command its own background call, `--files` taking `<paths>` joined by commas:

```bash
hk fix <paths>
nx affected -t check --files=<paths> --exclude=rasm
nx run rasm:check--ty
ty check --python "$(uv python find --script <script>)" <script>
```

Warning: check targets run `uv run --exact --package <member>` under a `UV_PROJECT_ENVIRONMENT` the target sets, and the same command run by hand syncs the root `.venv` to one member, uninstalling every other package. Checks run through `nx` alone, and a removed environment returns through the `run` command of its `.config/mise.toml` `[deps.<row>]` row (`uv`, `blender`, `darkroom`):
- Scope: root project `rasm` runs repository-wide knip, jscpd, and cargo-deny that fail on other work in a shared tree, and a pass excludes it
- Root: `<paths>` in the root uv root -> `rasm:check--ty`, the root project's type check of that root
- Manifest: `<paths>` holding `pyproject.toml` or `uv.lock` -> same `nx affected` line without `--exclude=rasm`, `check--uv` running `uv lock --check`
- Blender: Darkroom's `check--blender` validates its extension with Blender's own validator inside the `nx affected` run
- Plugin script: PEP 723 script under `plugins/` sits in no uv root -> `ty` line over it, `hk fix` running ruff
