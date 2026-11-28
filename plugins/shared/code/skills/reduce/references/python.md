# [PYTHON]

Python targets hold `.py` and `.pyi` files under each uv root `git ls-files ':!:.config/**' '*uv.lock'` lists and PEP 723 plugin scripts beside their `.py.lock`, `<uv-root>` naming the root holding a target, `<project>` the uv member folder holding its `pyproject.toml`, `<package>` a folder holding `__init__.py`, `<python>` the root interpreter `<uv-root>/.venv/bin/python -I -W ignore`, `<paths>` files a pass touches, and `<dir>` session scratch folder.

## [01]-[SKILLS]

| [INDEX] | [TARGET]            | [SKILLS]                                               | [CONDITION]                                     |
| :-----: | :------------------ | :----------------------------------------------------- | :---------------------------------------------- |
|  [01]   | Any Python          | `python:coding`, `python:expression`, `python:msgspec` | Every session, whatever the target imports      |
|  [02]   | Boundary validation | `python:pydantic`                                      | Hand checks of untrusted input or settings      |
|  [03]   | Command line        | `python:cyclopts`                                      | Hand `sys.argv` or `argparse` parsing           |
|  [04]   | Telemetry           | `python:telemetry`                                     | Hand logging setup, `print` diagnostics, timers |
|  [05]   | Docstrings          | `python:document`                                      | Pass writing a declaration                      |
|  [06]   | Blender             | `blender:coding`                                       | Imports `bpy`, file under a Blender uv root     |
|  [07]   | Blender add-on      | `blender:extensions`, `blender:interface`              | `register`, `Panel`, `Menu`, draw handlers      |
|  [08]   | Blender output      | `blender:nodes`, `blender:render`                      | `NodeTree`, `RenderSettings`, compositor        |
|  [09]   | Tests               | `code:test`                                            | Suite under `tests/`, no Python testing skill   |

`python:coding` states anyio forms in its concurrency reference and numpy forms in its numerics reference, both read whole with it. Members take the forms their `pyproject.toml` `requires-python` lowest bound accepts, so Blender roots and root libraries a Blender root installs (`>=3.13`) take no 3.14 or 3.15 form.

## [02]-[SOURCES]

Python targets read core packages `expression`, `msgspec`, `anyio`, and `numpy` every session, the standard library of their root's interpreter, and Blender's RNA, as most hand-rolled code in this tree restates one of them. Package reads, one command per step:
1. Version: release each uv root locks of each core package and each package step 11 adds, as roots resolve one package apart (`anyio`, `numpy`)
2. Guide: README each distribution's metadata holds, read whole, as wheels hold no agent guide, then its documentation URL through `search:web`
3. `expression`: inventory of every public module read whole, as its results, options, collections, and effects replace hand control flow
4. `msgspec`: inventory of every public module and each `.pyi` stub, as compiled `_core` states its signatures in stubs alone
5. `anyio`: inventory of every public module, for tasks, cancellation, streams, subprocesses, and file I/O a coroutine does by hand
6. `numpy`: inventory of every public module, each module naming the job's field (`numpy.linalg`, `numpy.random`, `numpy.fft`) read whole
7. Member: source of one member, a compiled member's signature from its `.pyi` stub through `search:code`
8. Standard library: `builtins`, `functools`, `itertools`, `operator`, `collections`, `enum`, `typing`, and a job's module as inventory `<module>`
9. Host: each RNA property and function of `bpy.types` in the installed Blender with enum identifiers, as a Blender root's `bpy` wheel can trail it
10. Workspace: first-party files the target imports, each read whole
11. Central manifest: dependency groups of each uv root whole, each package fitting a target's job added to its project, its API read through step 3

```bash
for d in expression msgspec anyio numpy <dist>; do (cd <uv-root> && uv tree --frozen --invert --package "$d" --depth 1 | head -1); done
<python> -c "import sys; from importlib.metadata import metadata; m = metadata(sys.argv[1]); print(m['Version'], *m.get_all('Project-URL') or (), m.json.get('description', ''), sep='\n')" <dist>
for root in expression msgspec anyio numpy <module>; do <python> -c "import importlib, pkgutil, sys; root = importlib.import_module(sys.argv[1]); print(root.__name__, *(m.name for m in pkgutil.walk_packages(getattr(root, '__path__', ()), f'{root.__name__}.') if not any(p.startswith('_') or p == 'tests' for p in m.name.split('.'))), sep='\n')" "$root" | while read -r m; do printf '### %s\n' "$m"; <python> -m pydoc "$m"; done > "<dir>/inventory-$root.txt"; done; rg -c '^    (class )?\w+\(|^ \|  \w+\(' <dir>/inventory-*.txt
rg -n '^(class|def) |^    def ' -g '*.pyi' "$(<python> -c 'import msgspec, os; print(os.path.dirname(msgspec.__file__))')"
<python> -m inspect <module>:<qualname>
<python> -c "import sys; print(*sorted(sys.stdlib_module_names), sep='\n')"
blender --background --factory-startup --python-exit-code 1 --python-expr "import bpy; print(*(f'{n}\t{kind}\t{m.identifier}\t{\" \".join(i.identifier for i in getattr(m, \"enum_items\", ()))}\t{m.description}' for n in dir(bpy.types) for rna in [getattr(getattr(bpy.types, n), 'bl_rna', None)] if rna for kind, members in (('property', rna.properties), ('function', rna.functions)) for m in members), sep='\n')" | rg '\t(property|function)\t' > <dir>/inventory-bpy.tsv
ruff analyze graph <target> | jq -r '[.[]] | add | unique[]'
git ls-files ':!:.config/**' '*uv.lock' && yq -p toml -oy '."dependency-groups"' <uv-root>/pyproject.toml && yq -p toml -oy '.project.dependencies' <project>/pyproject.toml
```

Reads hold by root:
- Interpreter: `<python>` reads the release its root installs, as `uv pip list` reads `VIRTUAL_ENV`, which mise points at the root `.venv`
- Installs: root `default-groups = "all"` installs every root catalog row, so step 3's command reads any root package with no install
- Imports: inventories import each module, and a module whose optional dependency is missing prints its error in the inventory and stays unread
- Members: `<python> -m inspect` reads a class member (`expression.core.option:Some.map`), as a `type` alias holds no members
- Blender: modules importing `bpy` load inside Blender alone, read through step 9 and the Blender root's `bpy-stubs`, `rna_enums` literals included

## [03]-[MAPPING]

Maps list what each file of the target declares and exports and who reads it, one command per step, each run once over `<paths>`:
1. Exports: `__all__` size per file and module-level names neither `_` names nor listed, the level each name leaves unstated
2. Levels: names a private module's `__all__` lists and its package entry omits, with the files naming each, 1 meaning its own file alone
3. Name families: module-level names differing by case, underscore, plural, or a joined builder word, each family read whole before judging it
4. Constants: upper-case module names with the lines naming each in its file, 2 meaning one reader
5. Classes: instance fields, static methods, and methods per class, the leads for namespace classes and data classes
6. Operations: module functions whose first parameter takes a class their own file declares, each a lead for a member of that class
7. Importers: files importing each module of the unit, a package entry's importers standing for its private modules' readers
8. Text: sites spelling a name as a string (`__all__`, `bl_idname`, `bpy.ops.<category>.<name>`, wire tags, entry points, `importlib`) and mentions
9. Like work: clone pairs touching the target across `<roots>` (`libs products plugins tools`), ranked by lines times similarity
10. Nesting: block depth per site past one level, deepest first, then local `def` depth per function

```bash
<python> -c "import ast, sys; [print(p, len(a), ' '.join(sorted(d - a)), sep='\t') for p in sys.argv[1:] for b in [ast.parse(open(p, encoding='utf-8').read()).body] for a in [{n for s in b if isinstance(s, ast.Assign) and ast.unparse(s.targets[0]) == '__all__' for n in ast.literal_eval(s.value)}] for d in [{ast.unparse(getattr(s, 'name', None) or s.target) if isinstance(s, (ast.TypeAlias, ast.AnnAssign)) else s.name for s in b if isinstance(s, (ast.FunctionDef, ast.AsyncFunctionDef, ast.ClassDef, ast.TypeAlias, ast.AnnAssign))} | {ast.unparse(t) for s in b if isinstance(s, ast.Assign) for t in s.targets} - {'__all__'}] for d in [{n for n in d if not n.startswith('_')}]]" <paths>
comm -13 <(rg -oUIN '^__all__ = \[[^\]]*\]' <package>/__init__.py | rg -o '"\w+"' | tr -d '"' | sort -u) <(rg -oUIN '^__all__ = \[[^\]]*\]' -g '*.py' -g '!__init__.py' <package> | rg -o '"\w+"' | tr -d '"' | sort -u) |
    while read -r n; do printf '%s\t%s\n' "$(rg -lw -g '*.py' -- "$n" . | wc -l | tr -d ' ')" "$n"; done | sort -n
ast-grep outline <paths> --items structure --json=stream | jq -r '.path as $p | .items[] | select(.name != "__all__") | "\(.name)\t\($p | split("/") | last)"' | sort -u |
    awk -F'\t' '{ k = tolower($1); sub(/^_+/, "", k); sub(/^(make|run|to|from|new|create|build|get|with|as)_/, "", k); sub(/(_of|_for|_from|s)$/, "", k); gsub(/_/, "", k)
        g[k] = g[k] " " $1 "@" $2; n[k]++ } END { for (k in g) if (n[k] > 1) print n[k] "\t" g[k] }' | sort -rn
ast-grep outline <paths> --items structure --json=stream | jq -r '.path as $p | .items[] | select(.name | test("^_?[A-Z][A-Z0-9_]+$")) | "\($p)\t\(.name)"' |
    while IFS=$'\t' read -r p n; do printf '%s\t%s\t%s\n' "$(rg -cw -- "$n" "$p")" "$p" "$n"; done | sort -n
ast-grep outline <paths> --items structure --view expanded --json=stream | jq -r '.path as $p | .items[] | select(.symbolType == "class")
    | [.members[]? | select(.name | contains(".") | not)] as $m | ($m | map(select(.symbolType == "method"))) as $f
    | [($m | map(select(.symbolType == "field" and (.signature | test("ClassVar") | not))) | length), ($f | map(select(.signature | test("^@staticmethod"))) | length), ($f | length), $p, .name] | @tsv'
ast-grep outline <paths> --items structure --json=stream | jq -r '.path as $p | [.items[] | select(.symbolType == "class") | .name] as $c | .items[] | select(.symbolType == "function")
    | (.signature | capture("^(async )?def \\w+(\\[[^]]*\\])?\\(\\*?\\w+: (?<t>\\w+)").t // "") as $t | select($c | index($t)) | "\($p)\t\(.name)\t\($t)"'
ruff analyze graph --direction dependents <roots> tests | jq -r '[.[$ARGS.positional[]][]?] - $ARGS.positional | unique[]' --args <paths>
rg -nw '<name>' .
jscpd --config .config/jscpd.json --similarity 0.6 --min-nodes 60 --format python --reporters json --output <dir>/jscpd <roots>;
    jq -r --arg t "$PWD/<target>/" '[.duplicates[] | select((.firstFile.name | startswith($t)) or (.secondFile.name | startswith($t)))] | sort_by(-(.lines * .similarity))[]
    | "\(.lines)\t\(.similarity)\t\(.firstFile.name):\(.firstFile.start)\t\(.secondFile.name):\(.secondFile.start)"' <dir>/jscpd/jscpd-report.json
ruff check --select PLR1702 --config 'lint.pylint.max-nested-blocks=1' --output-format json <paths> |
    jq -r --arg r "$PWD/" '.[] | [(.message | capture("\\((?<n>[0-9]+) > ").n), (.filename | ltrimstr($r)), .location.row] | @tsv' | sort -rn
ast-grep outline <paths> --items structure --view expanded --json=stream | jq -r '.path as $p | .items[] | (.name, .members[]?.name) | select(contains(".")) | "\(split(".") | length - 1)\t\($p)\t\(.)"' | sort -rn
```

Commands read by their own rules:
- Exports: package entry lists names it imports, so step 1 prints its `__all__` size with no unlisted names
- Levels: one run per package entry, its count read across the repository, as products and other members read a library's names
- Namespaces: entry `__all__` naming a private module (`"grades"`) publishes each of its names as `grades.<name>`, which `rg -w` counts
- Families: class, its default instance, and its builder print as one family (`Clock`, `CLOCK`, `clock`), one concept under each name
- Classes: columns count fields (`ClassVar` none, `__slots__` one), static methods, and methods, `0` fields beside static methods marking a namespace
- Clones: jscpd exits 1 whenever a pair prints, and the ranking reads its report regardless
- Nesting: ruff `PLR1702` counts `if`, `for`, `while`, `try`, `with`, and `match` blocks, exiting 1 on a site past `max-nested-blocks`
- Limits: hk rule `no-fourth-nesting-level-python` holds code to 3 levels of `if`, `for`, `while`, `match`, conditional expressions, and closures
- Closures: outline names a local `def` by its enclosing path (`completed.held.kept`), a depth neither checker reads outside control flow

## [04]-[MECHANISMS]

Python binds every name at runtime in the namespace of its module or class, and a class is at once type, constructor, match pattern, and namespace, so most reductions here remove a declaration by letting an object already present state it.

[NAMES]:
- Classes: class holds its constructors (`X.of`), defaults (`X()`), shared values (`ClassVar`), and derived facts (`@property`) under one name
- Modules: module is a namespace of functions and values, `history.folder` reading as one concept where a static-method class restates it
- Entries: entry `__all__` row naming a private module (`"grades"`) exports its whole `__all__` under one name, read as `grades.cdl`
- Levels: `_` makes a name file-private, `_` module's `__all__` package-internal, and entry `__init__.py` `__all__` public to type checkers
- Keys: `__name__`, `__qualname__`, enum `.name`, `msgspec.structs.fields`, `__set_name__`, and `__init_subclass__` turn declared names into keys
- Enums: member value holds any object (data tuple `__init__` unpacks, `enum.member` callable), and iteration lists every member
- Decoding: `auto()` on a `StrEnum` spells each member's lowercase name, and `X[name]` or `X(value)` reads a member from a boundary string
- Verbs: functions name the value they return by their operation's past participle (`accumulated`, `relinked`), a form names passes keep

[RECORDS]:
- Generated: frozen `msgspec.Struct` generates `__init__`, equality, hash, `__match_args__`, `__replace__`, codec, and schema from its fields
- Copies: `msgspec.structs.replace` changes fields of one record, `msgspec.convert(<object>, T, from_attributes=True)` reads same-named fields
- Builtins: `msgspec.to_builtins` and `msgspec.structs.asdict` write a record's mapping, in place of a dict literal spelling each field
- Shapes: `msgspec.inspect.type_info` lists fields with wire names, defaults, and bounds, so flag, column, and key lists derive from a record
- Unions: `type Shape = Circle | Rect` over cases of one tagged base, folded by one `match` with class patterns, `@case` where no codec reads it
- Types: `typing.get_args` over an alias's `__value__` lists a `Literal` or union at runtime, `Self` and `type(self)` name the class
- Signatures: `**P` with `Concatenate`, `*Ts`, and `Unpack[TypedDict]` state a wrapper's parameters from its callee, `TypeForm` a type argument

[EXPRESSIONS]:
- Comprehensions: more than one `for` clause, a walrus, and unpacking (`*hit.groups() for hit in hits`) build what a loop appends
- Folds: `functools.reduce` folds a carried state, `itertools.accumulate` yields each state, `sum(start=)` and `math.prod` reduce numbers
- Totals: `max(default=)`, `min(key=)`, `any`, `all`, and `next(<iterator>, <default>)` read a flag or extreme a loop sets
- Iteration: `itertools` (`groupby`, `pairwise`, `batched`, `product`, `starmap`) over index loops, `dict.fromkeys` for ordered unique keys
- Sets: mapping views compare as sets (`a.keys() - b.keys()`), and CPython folds a set literal in an `in` test into one `frozenset` constant
- Callables: unbound methods (`str.casefold`), `operator` functions, and `partial` pass as themselves, `Placeholder` skipping a positional
- Getters: ty types `attrgetter` and `methodcaller` results as `Any`, so `lambda item: item.member` stays
- Dispatch: `match` with class, mapping, and sequence patterns and guards, `singledispatch` where another module registers a class
- Memoization: `cache` on a zero-argument builder and `cached_property` hold a value built once, in place of a module global a first call fills

[RESULTS]:
- Branches: `found.map(f).default_value(y)` replaces `f(...) if found.is_some() else y`, `merge()` a two-arm `match` over `Result[T, T]`
- Absence: `option.of_optional(table.get(key))` and `option.of_optional(try_downcast(kind, value))` replace `Some(...) if ... else NOTHING`
- Raises: `catch(f, exception=E)(arguments)` replaces a `try` around one synchronous call returning `Error` from its handler
- Batches: `accumulated` and `paired` combine independent results, `Block.partition` splits and `choose` filters and maps in one pass
- Chains: `bind`, `pipeline`, and direct-style `@effect.result` generators flatten dependent steps a tower of local `def` closures nests
- State: `Block.unfold` grows a chain from a seed, `Map.change` threads keyed state, and `Seq.try_pick` reads a first match lazily

[VERSIONS]:
- Releases: 3.15 adds `lazy` imports, `frozendict`, comprehension unpacking, and `TypeForm`, 3.14 `Placeholder` and `operator.is_not_none`

Host, checker, and codec rules bind where they read the code, each read from its source or proved by one probe:
- Blender classes: classes Blender registers by type (`Operator`, `Panel`, `PropertyGroup`) stay classes holding `bpy.props` annotations
- Blender enums: identifiers come from `rna_enums` literals of `bpy-stubs`, a module with no runtime object, imported under `TYPE_CHECKING`
- Blender writes: call with a host side effect (`animation_data_create()`, depsgraph tags) is a write, so a read guarding it stays
- Bounds: msgspec runs `Meta` constraints at decode and `convert` alone, so a `__post_init__` check of a record code constructs stays
- Wire names: `tag=str.lower` and `tag=True` derive wire tags from class names, so a class rename changes the wire unless `tag=` keeps it
- Variance: ty infers a PEP 695 parameter of a frozen `msgspec.Struct` invariant, so a record read covariantly keeps `TypeVar(..., covariant=True)`
- Generators: `yield` and `yield from` inside a comprehension raise `SyntaxError`, so a step yielding per item keeps its `for` statement
- Docstrings: ruff `D` and `DOC` rules require one per declaration and `msgspec.json.schema` reads a record's as `description`, so none is redundant

## [05]-[PATTERNS]

Patterns common in this tree's Python, each a lead the map confirms before any rebuild, its rebuild holding under the condition beside it:

| [INDEX] | [PATTERN]         | [LOOK_FOR]                                                          | [REBUILD]                                     |
| :-----: | :---------------- | :------------------------------------------------------------------ | :-------------------------------------------- |
|  [01]   | Statement loops   | Loop appending, counting, or flagging for a later read, index loops | Comprehension, fold, builtin, `itertools`     |
|  [02]   | Hand results      | `.is_some()` arms, `Some(v) if c else NOTHING`, one-call `try`      | Combinator, `of_optional`, `catch`            |
|  [03]   | Result batches    | Results appended before `accumulated`, `values, faults` lists       | `accumulated` over a generator, `partition`   |
|  [04]   | Namespace classes | No instance fields, `@staticmethod` members, `ClassVar` state       | Module functions and values                   |
|  [05]   | Loose operations  | Functions taking their file's own record first (`lattice_of`)       | Method or property of that record             |
|  [06]   | Hand records      | `__init__` storing fields, `@dataclass`, `dict`, `X(a=y.a, b=y.b)`  | Frozen `msgspec.Struct`, `replace`, `convert` |
|  [07]   | Name triads       | `X` beside `X_DEFAULT`, `x`, `x_of`, `make_x`, `x_` prefix          | One class with defaults and `X.of`            |
|  [08]   | Keyed constants   | `_WIDTH = Socket(key="width")`, name restating its string key       | Fields or members whose names are keys        |
|  [09]   | Case branches     | `X if self is M.A else Y`, `case "MIX":` arms, `isinstance` chains  | Member value, tagged union, class patterns    |
|  [10]   | Restated sources  | `Literal` of host identifiers, record copying host or package type  | `rna_enums` literal, source type              |
|  [11]   | Local def towers  | Local `def` holding another (`a.b.c`), each passed to `bind`        | Flat chain, tuple `match`, `effect.result`    |
|  [12]   | Export drift      | Public names outside `__all__`, `__all__` names one file reads      | `_` prefix, one `__all__` per level           |

Patterns hold under these conditions:
- Row [01] -> loop of host writes (`layout` calls, `operator.report`, `bpy` writes) stays a statement, its body one call per item
- Row [01] -> local list a fold would rebuild by tuple concatenation stays local, as mutating a local is no side effect
- Row [02] -> entry point reading a result once through `match` stays, and so does a `try` around an awaited call
- Row [03] -> step yielding to its driver per item keeps its loop and list, as a comprehension refuses `yield`
- Row [04] -> class Blender registers, or class its readers pass as a value (`place=(RenderHistory, key)`), stays a class
- Row [04] -> fieldless cases overriding static methods of their base (`MirrorBall` under `Panorama`) are union cases and stay
- Row [05] -> function a host or registry calls by name, or one reading each of its records alike, stays a module function
- Row [06] -> handle holder needing `field(compare=False, repr=False)` keeps a frozen `dataclass`, and service owning mutable state its class
- Row [07] -> value of another type built from `X` (`InterfacePanel.of("clock", Clock())`) and function doing another job keep their own names
- Row [08] -> key a host spells outside identifier rules (`"Standard Modal Map"`) stays one literal its readers share
- Row [09] -> identifiers Blender or a wire spells keep their value, and arms computing different results over shared data stay one `match`
- Row [10] -> wire record differing from its domain record in names, units, or nesting stays
- Row [11] -> local `def` holding a type parameter, a `try` around an awaited call, or a closure over a loop value stays
- Row [12] -> names Blender or a codec reads by name (`register`, operator functions) stay public

## [06]-[CHECK]

Check per pass, each command its own background call, `--files` taking `<paths>` joined by commas:

```bash
hk fix <paths>
nx affected -t check --files=<paths> --exclude=rasm
nx run <root-project>:check--ty
ty check --python "$(uv python find --script <script>)" <script>
```

Warning: check targets run `uv run --exact --package <member>` under a `UV_PROJECT_ENVIRONMENT` the target sets, and the same command run by hand syncs the root `.venv` to one member, uninstalling every other package. Checks run through `nx` alone, and a removed environment returns through the `run` command of its `.config/mise.toml` `[deps.<row>]` row (`uv`, `blender`, `darkroom`):
- Scope: root project `rasm` runs repository-wide knip, jscpd, and cargo-deny, which fail on other work in a shared tree, and a pass excludes it
- Types: `<paths>` in a root or Blender root member -> `rasm:check--ty` or `rasm-blender:check--ty`, which `nx affected` above never selects
- Manifest: `<paths>` holding `pyproject.toml` or `uv.lock` -> `nx affected` line without `--exclude=rasm`, `check--uv` running `uv lock --check`
- Blender: `check--blender` of an extension project validates it with Blender's own validator inside the `nx affected` run
- Plugin script: PEP 723 script under `plugins/` sits in no uv root -> `ty` line over it, `hk fix` running ruff
