# [PY_ARTIFACTS_API_JINJA2]

`jinja2` supplies the report-templating composition surface for the artifacts report-templating owner: a sandboxed template engine, a loader axis, an undefined-handling axis, autoescape policy, and the sync/async render path that drives report generation from a `VisualSpec`/`ExportPlan` and a runtime `ContentIdentity`. The package owner composes `Environment`, the loader family, and the undefined family into one report-templating owner; it never re-implements lexing, parsing, or rendering jinja2 already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jinja2`
- package: `jinja2`
- import: `import jinja2` (lint alias `j2`)
- owner: `artifacts`
- rail: report-templating
- installed: `3.1.6` reflected via `python -c "import jinja2"` on cp315
- entry points: none (library only)
- capability: text/markup template engine; lexer, parser, compiler, sandboxed execution, loader hierarchy, autoescape policy, undefined-value algebra, sync and async rendering, bytecode caching, extension hooks

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: engine and template roots
- rail: report-templating

Environment rows carry delimiter, whitespace, extension, undefined, finalize, autoescape, loader, cache, reload, bytecode, and async policy.

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]    | [CAPABILITY]                       |
| :-----: | :-------------------------------------- | :---------------- | :--------------------------------- |
|   [1]   | `Environment`                           | engine root       | configured compilation/render root |
|   [2]   | `Template`                              | compiled unit     | sync/async render entrypoints      |
|   [3]   | `sandbox.SandboxedEnvironment`          | restricted engine | filtered untrusted-template engine |
|   [4]   | `sandbox.ImmutableSandboxedEnvironment` | hardened engine   | sandbox blocking object mutation   |
|   [5]   | `sandbox.SecurityError`                 | sandbox fault     | forbidden sandbox access           |

[PUBLIC_TYPE_SCOPE]: loader axis
- rail: report-templating

| [INDEX] | [SYMBOL]           | [PACKAGE_ROLE]    | [CAPABILITY]                                                            |
| :-----: | :----------------- | :---------------- | :---------------------------------------------------------------------- |
|   [1]   | `BaseLoader`       | loader root       | abstract source provider; subclass overrides `get_source`               |
|   [2]   | `FileSystemLoader` | disk loader       | loads templates from a search path list                                 |
|   [3]   | `PackageLoader`    | package loader    | loads templates from an installed package's resource tree               |
|   [4]   | `DictLoader`       | in-memory loader  | loads templates from a `name -> source` mapping                         |
|   [5]   | `FunctionLoader`   | callable loader   | delegates source resolution to a callable                               |
|   [6]   | `PrefixLoader`     | namespaced loader | routes by prefix to a child loader                                      |
|   [7]   | `ChoiceLoader`     | fallback loader   | tries an ordered loader sequence                                        |
|   [8]   | `ModuleLoader`     | compiled loader   | loads pre-compiled templates emitted by `ModuleLoader`-targeted compile |

[PUBLIC_TYPE_SCOPE]: undefined axis and bytecode cache
- rail: report-templating

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]      | [CAPABILITY]                                                         |
| :-----: | :------------------------ | :------------------ | :------------------------------------------------------------------- |
|   [1]   | `Undefined`               | lenient undefined   | default missing-value placeholder; renders empty, raises on most ops |
|   [2]   | `ChainableUndefined`      | chainable undefined | permits attribute/index chaining on missing values                   |
|   [3]   | `DebugUndefined`          | debug undefined     | renders a debug marker for missing values                            |
|   [4]   | `StrictUndefined`         | strict undefined    | raises `UndefinedError` on any access                                |
|   [5]   | `BytecodeCache`           | cache root          | abstract compiled-bytecode cache                                     |
|   [6]   | `FileSystemBytecodeCache` | disk cache          | persists compiled bytecode to a directory                            |
|   [7]   | `MemcachedBytecodeCache`  | remote cache        | persists compiled bytecode to a memcached client                     |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: report-templating

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE]   | [CAPABILITY]                               |
| :-----: | :----------------------- | :--------------- | :----------------------------------------- |
|   [1]   | `TemplateError`          | fault root       | base of every template fault               |
|   [2]   | `TemplateNotFound`       | resolution fault | a named template is absent from the loader |
|   [3]   | `TemplatesNotFound`      | selection fault  | no template in a select list resolved      |
|   [4]   | `TemplateSyntaxError`    | parse fault      | template source failed to lex/parse        |
|   [5]   | `TemplateAssertionError` | compile fault    | a compile-time assertion failed            |
|   [6]   | `TemplateRuntimeError`   | runtime fault    | a runtime error raised during render       |
|   [7]   | `UndefinedError`         | undefined fault  | a strict/undefined access raised           |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: engine construction and resolution
- rail: report-templating

The `Environment` row carries delimiter, whitespace, extension, undefined, finalize, autoescape, loader, cache, reload, bytecode, and async policy.

| [INDEX] | [SURFACE]                            | [CALL_SHAPE]                     | [CAPABILITY]                                        |
| :-----: | :----------------------------------- | :------------------------------- | :-------------------------------------------------- |
|   [1]   | `Environment`                        | engine configuration policy      | full engine configuration                           |
|   [2]   | `Environment.get_template`           | name plus globals policy         | resolve and compile a named template via the loader |
|   [3]   | `Environment.select_template`        | name list plus globals policy    | resolve the first available of a name list          |
|   [4]   | `Environment.get_or_select_template` | name or name-list input          | polymorphic resolve over a name or name list        |
|   [5]   | `Environment.from_string`            | source plus template policy      | compile an in-memory source string                  |
|   [6]   | `Environment.compile_expression`     | expression plus undefined policy | compile a single expression to a callable           |
|   [7]   | `Environment.overlay`                | partial config override          | derive a reconfigured child environment             |
|   [8]   | `Environment.add_extension`          | extension identifier             | register an extension after construction            |

[ENTRYPOINT_SCOPE]: render path (sync and async)
- rail: report-templating

Render rows share args/kwargs context; async rows require an async-enabled environment.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                  | [CAPABILITY]                                                 |
| :-----: | :---------------------- | :---------------------------- | :----------------------------------------------------------- |
|   [1]   | `Template.render`       | render context -> `str`       | render to a full string                                      |
|   [2]   | `Template.render_async` | async render context -> `str` | async render; requires `enable_async=True`                   |
|   [3]   | `Template.generate`     | render context -> iterator    | lazy chunked render                                          |
|   [4]   | `Template.stream`       | render context -> stream      | streamed render with `dump`/`disable_buffering`              |
|   [5]   | `Template.make_module`  | vars/shared/locals policy     | materialize exported names/blocks                            |
|   [6]   | `Template`              | source plus engine knobs      | construct a standalone template with an implicit environment |

[ENTRYPOINT_SCOPE]: policy, undefined, and module functions
- rail: report-templating

Policy rows carry extension defaults, logging base class, object probe, cache state, and context-passing decorator targets.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]               | [CAPABILITY]                                       |
| :-----: | :----------------------- | :------------------------- | :------------------------------------------------- |
|   [1]   | `select_autoescape`      | extension/default policy   | extension-driven autoescape selector               |
|   [2]   | `make_logging_undefined` | logger plus undefined base | undefined subclass that logs every access          |
|   [3]   | `is_undefined`           | object probe               | undefined-instance predicate                       |
|   [4]   | `clear_caches`           | no-arg cache flush         | flush global template/lexer caches                 |
|   [5]   | `pass_context`           | callable decorator         | mark a filter/test to receive the render `Context` |
|   [6]   | `pass_environment`       | callable decorator         | mark a callable to receive the `Environment`       |
|   [7]   | `pass_eval_context`      | callable decorator         | mark a callable to receive the `EvalContext`       |

[ENTRYPOINT_SCOPE]: loader and bytecode-cache construction
- rail: report-templating

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                                                              | [CAPABILITY]                    |
| :-----: | :------------------------ | :------------------------------------------------------------------------ | :------------------------------ |
|   [1]   | `FileSystemLoader`        | `FileSystemLoader(searchpath, encoding='utf-8', followlinks=False)`       | disk template root(s)           |
|   [2]   | `PackageLoader`           | `PackageLoader(package_name, package_path='templates', encoding='utf-8')` | package-resource template root  |
|   [3]   | `DictLoader`              | `DictLoader(mapping)`                                                     | in-memory `name -> source` map  |
|   [4]   | `FunctionLoader`          | `FunctionLoader(load_func)`                                               | callable source provider        |
|   [5]   | `PrefixLoader`            | `PrefixLoader(mapping, delimiter='/')`                                    | prefix-routed child loaders     |
|   [6]   | `ChoiceLoader`            | `ChoiceLoader(loaders)`                                                   | ordered loader fallback chain   |
|   [7]   | `ModuleLoader`            | `ModuleLoader(path)`                                                      | pre-compiled-template loader    |
|   [8]   | `FileSystemBytecodeCache` | `FileSystemBytecodeCache(directory=None, pattern='__jinja2_%s.cache')`    | on-disk compiled-bytecode cache |

## [4]-[IMPLEMENTATION_LAW]

[REPORT_TEMPLATING]:
- import: `import jinja2 as j2` at boundary scope only; module-level import is banned by the manifest import policy.
- engine construction: one `Environment` (or `SandboxedEnvironment` for untrusted report sources) is the single owner; `autoescape=select_autoescape(...)`, `undefined=StrictUndefined`, and `enable_async=True` are the default report-templating policy — a missing variable is a fault, not a silent blank.
- loader axis: the loader is a row value (`FileSystemLoader`, `PackageLoader`, `DictLoader`, `PrefixLoader`, `ChoiceLoader`), never a parallel engine per source; report template roots are loader rows.
- render axis: `render` and `render_async` are the one sync/async pair; `generate`/`stream` own chunked output for large reports; the render context carries the `VisualSpec`/`ExportPlan` projection and the runtime `ContentIdentity`, never a re-minted identity.
- evidence: each render captures the resolved template name, the loader row, the autoescape decision, the undefined policy, and the output byte length as a templating receipt.
- boundary: jinja2 renders the report body offline; the rendered bytes feed the document/PDF owner; live UI templating and browser state stay outside this package.

[SANDBOX_POLICY]:
- untrusted source: report templates from outside the package use `ImmutableSandboxedEnvironment`; a `SecurityError` is a typed fault on the templating rail, never an escape.
- attribute policy: the sandbox attribute/operator filters are the security boundary; the owner never disables them to expose host internals.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `jinja2`
- Owns: report-templating composition — sandboxed template engine, loader axis, undefined-handling axis, autoescape policy, and the sync/async render path that drives report generation
- Accept: report-body rendering from a `VisualSpec`/`ExportPlan` and runtime `ContentIdentity` into bytes the document/PDF owner consumes
- Reject: wrapper-renames of `render`/`get_template`; a second engine per template source where a loader row suffices; live UI templating; identity minting the runtime already owns
