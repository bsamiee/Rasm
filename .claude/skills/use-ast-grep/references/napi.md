# [NAPI]

`@ast-grep/napi` parses and queries one source inside a Node process, the CLI remains the route to everything a project configures.

## [01]-[SELECTION]

CLI subprocesses over `--json` fit project rules and many files, napi fits a tool holding a parse across calls:
- CLI alone reads `sgconfig.yml`, rule and util directories, `fix`, rewriters, global and parameterized utils, injections, and `ast-grep-ignore`
- Napi reads no `sgconfig.yml`, one pattern can diverge by working directory where the CLI maps an extension to a custom language
- CLI spawns cost a process floor, small parse-and-find calls under napi cost a small fraction of one spawn
- `findInFiles` over one tree undercuts a CLI run with JSON output, `--json` stream reads add the parse of its output
- Napi and CLI each hold their grammars and rule parser in their binary, unknown rule keys throw, a rule using a newer CLI key fails in an older napi

## [02]-[RULES]

`NapiConfig` reads `rule`, `constraints`, `utils`, and `transform`:
- `fix`, `rewriters`, `id`, `message`, `severity`, and `files` drop silently at the top level, unknown keys inside `rule` throw
- `language` on a config is ignored, the node's language or `findInFiles`' first argument wins
- `rewrite` transforms are a silent identity, the CLI's `replacement` differs on one rule
- `arguments` under `utils` throws as an unknown field, a `matches: {<util>: {...}}` call throws `does not accept arguments`, `UtilityCall` stays typed and no config satisfies it
- `kind(language, name)` returns 0 for an unknown name with no throw and `findAll(0)` returns `[]`, a rule `kind` throws
- Built-in names fold case and accept `js`, custom names match exactly
- `findAll` compiles and traverses per call, `pattern` constructs configuration without a compiled-query cache

Node methods read differently from rule relations:
- `has`, `inside`, `precedes`, `follows` methods scan every level, rule relations default to `stopBy: neighbor`, `has` skips self and `find` includes it
- `getMatch` takes `A`, not `$A`, a `$$$ARGS` capture returns null, `getMultipleMatches` returns separators, `isNamed()` filters them, a single metavariable gives `[]`
- Metavariables and transforms belong to the matched wrapper alone, `parent()` loses them and `matches()` binds nothing
- `next`, `prev`, `children`, `child` include anonymous tokens, `ancestors` runs nearest-first and excludes self
- `id()` is a pointer cast stable across wrappers, `===` between wrappers is false
- Syntax errors do not throw, they appear as `ERROR` kinds, `kindToRefine` is a getter
- `fieldChildren` returns repeated field children, `field` returns their first node

Types derive from a language's generated map:
- `import type TS from '@ast-grep/napi/lang/TypeScript'` or `@ast-grep/lang-<name>/type`, then `parse<TS>(…)`
- Required fields are non-null, optional fields nullable, bad field names and rule kinds fail compilation
- `is()` narrows, a `kindToRefine` switch narrows only on `find<'a' | 'b'>`

## [03]-[EDITS]

`replace` is literal and `commitEdits` returns a string with the tree unchanged:
- `replace('bar($A)')` stays `bar($A)`, text builds from `getMatch` and `getTransformed`
- Edits sort by `startPos`, the earliest start wins whatever the array order, at an equal start the first listed wins, overlaps drop silently
- Inserts at one position keep array order, plain `{startPos, endPos, insertedText}` objects are accepted
- Offsets are source-absolute, a subnode receiver included
- Warning: an edit outside the receiver's range or `endPos` past the end aborts the process with SIGABRT, `endPos < startPos` silently corrupts text
- `findAll` returns nested matches outer-first, nested rewrites take an inner `commitEdits` or a new parse

```js
const root = parse(Lang.JavaScript, 'old_name(first_arg); old_name(second_arg); other(third_arg);').root();
const config = {rule: {pattern: 'old_name($ARG)'}, transform: {UPPER: {convert: {source: '$ARG', toCase: 'upperCase'}}}};
const edits = root.findAll(config).map((node) => node.replace(`new_name(${node.getTransformed('UPPER')})`));
root.commitEdits(edits); // new_name(FIRST_ARG); new_name(SECOND_ARG); other(third_arg);
root.text(); // old_name(first_arg); old_name(second_arg); other(third_arg);
```

## [04]-[FILES]

`findInFiles` compiles once for its parallel file walk and shares discovery and parsing:
- One callback per file, its returned count is files with at least one hit, `parseFiles` counts every parsed file and infers language by extension
- Promises can resolve before queued callbacks, aggregation finishes when processed callbacks reach the returned count
- Non-UTF-8 and missing paths give 0 and no callback, `err` is never set
- Bad rules, bad languages, and empty `paths` throw synchronously, a throwing callback becomes `uncaughtException` while the promise resolves
- Ignore files apply as under the CLI, hidden directories skip, hidden files with a matching extension are searched
- Explicit file paths parse as the declared language
- `languageGlobs` adds to the defaults

```js
const collect = (paths, matcher) => new Promise((resolve, reject) => {
    const rows = [];
    let total;
    const settle = () => rows.length === total && resolve(rows);
    findInFiles(Lang.JavaScript, {paths, matcher}, (error, nodes) => {
        if (error) return reject(error);
        rows.push(nodes.map((node) => node.text()));
        settle();
    }).then((count) => { total = count; settle(); }, reject);
});
```

## [05]-[RESOURCES]

- `parseAsync` runs on libuv workers and scales with `UV_THREADPOOL_SIZE`, `parseAsync('nope')` throws synchronously
- `findInFiles` adds a walker thread pool with no backpressure, files parse and match on walker threads, `find*` stays synchronous on the JS thread
- Native memory stays invisible to V8, each root costs tens of MB per MB of source, `external` shows no change
- Dropped roots hold memory until a collection, a loop over large sources without a tick reaches gigabytes
- Nodes retain complete source trees, aggregate text and ranges when later work needs values alone
- Ranges and edits use UTF-16 code units, `index` is one (`sgnode.d.ts` calls it a byte offset)

## [06]-[LANGUAGES]

`Lang` holds `Html`, `JavaScript`, `TypeScript`, `Tsx`, and `Css` alone, `@ast-grep/lang-<name>` packages export a registration for most other grammars:
- `hcl`, `nix`, `solidity`, and `zig` packages do not exist, `toml` and `sql` exist as extras, each registration falls back to `parser.so` when no prebuild matches
- `registerDynamicLanguage` takes every language in one call, later calls drop their registration and still `dlopen`
- Warning: a failing `registerDynamicLanguage` on a missing library or symbol aborts the process, only missing `extensions` throws a catchable error
- Registration takes `libraryPath`, `extensions`, optional `languageSymbol` (default `tree_sitter_<name>`), `metaVarChar`, and `expandoChar` (default `metaVarChar`)
- Relative `libraryPath` resolves against the working directory at call time, a platform map works beside the typed string
- Built-in names win, registering `JavaScript` parses the built-in
- `metaVarChar: '%'` makes `%A` a capture and `$A` text
- Registered languages are process-global, a Worker parses them without registering
- Numeric kind ids belong to a grammar version, ABI range 13 to 15 loads
