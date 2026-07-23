# [PY_ARTIFACTS_API_JINJA2]

`jinja2` owns report-templating for the artifacts `document` rail: one sandboxed `Environment` composes a loader axis, an undefined-handling axis, autoescape policy, native-type rendering, the filter/test/global/extension registries, bytecode/precompile caching, and the sync/async render path that drives report generation from a `VisualSpec`/`ExportPlan` and runtime `ContentIdentity`. Its owner re-implements no lexing, parsing, compilation, or rendering jinja2 already carries.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jinja2`
- package: `jinja2` (BSD-3-Clause)
- module: `jinja2`
- namespaces: `jinja2`, `jinja2.sandbox`, `jinja2.nativetypes`, `jinja2.ext`, `jinja2.bccache`
- owner: `artifacts`
- rail: report-templating
- depends: `MarkupSafe` runtime dependency; `Babel` only under the `i18n` extra

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: engine and template roots

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]     | [CAPABILITY]                                                             |
| :-----: | :-------------------------------------- | :---------------- | :----------------------------------------------------------------------- |
|  [01]   | `Environment`                           | engine root       | configured compilation/render root                                       |
|  [02]   | `Template`                              | compiled unit     | sync/async render entrypoints                                            |
|  [03]   | `sandbox.SandboxedEnvironment`          | restricted engine | filtered untrusted-template engine                                       |
|  [04]   | `sandbox.ImmutableSandboxedEnvironment` | hardened engine   | sandbox blocking object mutation                                         |
|  [05]   | `sandbox.SecurityError`                 | sandbox fault     | forbidden sandbox access                                                 |
|  [06]   | `nativetypes.NativeEnvironment`         | native engine     | renders to the native Python object of one expression, not a string      |
|  [07]   | `nativetypes.NativeTemplate`            | native unit       | `render` yields the evaluated Python value (int/list/dict), not a string |

[PUBLIC_TYPE_SCOPE]: loader axis

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]     | [CAPABILITY]                                                              |
| :-----: | :----------------- | :---------------- | :------------------------------------------------------------------------ |
|  [01]   | `BaseLoader`       | loader root       | abstract source provider; subclass overrides `get_source`                 |
|  [02]   | `FileSystemLoader` | disk loader       | loads templates from a search path list                                   |
|  [03]   | `PackageLoader`    | package loader    | loads templates from an installed package's resource tree                 |
|  [04]   | `DictLoader`       | in-memory loader  | loads templates from a `name -> source` mapping                           |
|  [05]   | `FunctionLoader`   | callable loader   | delegates source resolution to a callable                                 |
|  [06]   | `PrefixLoader`     | namespaced loader | routes by prefix to a child loader                                        |
|  [07]   | `ChoiceLoader`     | fallback loader   | tries an ordered loader sequence                                          |
|  [08]   | `ModuleLoader`     | compiled loader   | loads pre-compiled templates emitted by a `ModuleLoader`-targeted compile |

[PUBLIC_TYPE_SCOPE]: undefined axis and bytecode cache

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]       | [CAPABILITY]                                                         |
| :-----: | :------------------------ | :------------------ | :------------------------------------------------------------------- |
|  [01]   | `Undefined`               | lenient undefined   | default missing-value placeholder; renders empty, raises on most ops |
|  [02]   | `ChainableUndefined`      | chainable undefined | permits attribute/index chaining on missing values                   |
|  [03]   | `DebugUndefined`          | debug undefined     | renders a debug marker for missing values                            |
|  [04]   | `StrictUndefined`         | strict undefined    | raises `UndefinedError` on any access                                |
|  [05]   | `BytecodeCache`           | cache root          | abstract compiled-bytecode cache                                     |
|  [06]   | `FileSystemBytecodeCache` | disk cache          | persists compiled bytecode to a directory                            |
|  [07]   | `MemcachedBytecodeCache`  | remote cache        | persists compiled bytecode to a memcached client                     |

[PUBLIC_TYPE_SCOPE]: fault family

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]    | [CAPABILITY]                               |
| :-----: | :----------------------- | :--------------- | :----------------------------------------- |
|  [01]   | `TemplateError`          | fault root       | base of every template fault               |
|  [02]   | `TemplateNotFound`       | resolution fault | a named template is absent from the loader |
|  [03]   | `TemplatesNotFound`      | selection fault  | no template in a select list resolved      |
|  [04]   | `TemplateSyntaxError`    | parse fault      | template source failed to lex/parse        |
|  [05]   | `TemplateAssertionError` | compile fault    | a compile-time assertion failed            |
|  [06]   | `TemplateRuntimeError`   | runtime fault    | a runtime error raised during render       |
|  [07]   | `UndefinedError`         | undefined fault  | a strict/undefined access raised           |

[PUBLIC_TYPE_SCOPE]: extension family (`jinja2.ext`)

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]       | [CAPABILITY]                                                                |
| :-----: | :---------------------------------- | :------------------ | :-------------------------------------------------------------------------- |
|  [01]   | `ext.Extension`                     | extension base      | subclass to add custom `{% %}` tags, filters, and parse hooks               |
|  [02]   | `ext.InternationalizationExtension` | i18n extension      | `{% trans %}` / `gettext`/`ngettext` translation (`"jinja2.ext.i18n"`)      |
|  [03]   | `ext.ExprStmtExtension`             | expr-stmt extension | `{% do %}` side-effecting expression statement (`"jinja2.ext.do"`)          |
|  [04]   | `ext.LoopControlExtension`          | loop-control        | `{% break %}` / `{% continue %}` inside loops (`"jinja2.ext.loopcontrols"`) |
|  [05]   | `ext.DebugExtension`                | debug extension     | `{% debug %}` dumps the render context (`"jinja2.ext.debug"`)               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: engine construction and resolution

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                                            |
| :-----: | :------------------------------------------------- | :------- | :------------------------------------------------------ |
|  [01]   | `Environment(**policy)`                            | ctor     | configured compilation/render root                      |
|  [02]   | `get_template(name, parent, globals)`              | instance | resolve+compile a named template via the loader         |
|  [03]   | `select_template(names, parent, globals)`          | instance | resolve the first available of a name list              |
|  [04]   | `get_or_select_template(name_or_list, ...)`        | instance | polymorphic resolve over a name or name list            |
|  [05]   | `from_string(source, globals, template_class)`     | factory  | compile an in-memory source string                      |
|  [06]   | `compile_expression(source, undefined_to_none)`    | instance | compile a single expression to a callable               |
|  [07]   | `overlay(**changes)`                               | instance | derive a reconfigured child environment                 |
|  [08]   | `add_extension(extension)`                         | instance | register an extension after construction                |
|  [09]   | `compile(source, name, filename, raw, defer_init)` | instance | compile to a code object or raw module string           |
|  [10]   | `compile_templates(target, *, zip, ignore_errors)` | instance | precompile every template to the `ModuleLoader` archive |
|  [11]   | `filters`/`tests`/`globals`/`policies`             | property | mutable registries for custom filters/tests/globals     |

[ENTRYPOINT_SCOPE]: render path (sync and async)
- render family carry: `*args`, `**kwargs` (the render context)

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `Template.render(...) -> str`                | instance | render to a full string                          |
|  [02]   | `Template.render_async(...) -> str`          | instance | async render; requires `enable_async=True`       |
|  [03]   | `Template.generate(...) -> Iterator`         | instance | lazy chunked render                              |
|  [04]   | `Template.stream(...) -> TemplateStream`     | instance | streamed render with `dump`/`disable_buffering`  |
|  [05]   | `Template.make_module(vars, shared, locals)` | instance | materialize exported names/blocks                |
|  [06]   | `Template(source, **knobs)`                  | ctor     | standalone template with an implicit environment |

[ENTRYPOINT_SCOPE]: policy, undefined, and module functions

| [INDEX] | [SURFACE]                                    | [SHAPE]   | [CAPABILITY]                                    |
| :-----: | :------------------------------------------- | :-------- | :---------------------------------------------- |
|  [01]   | `select_autoescape(enabled_extensions, ...)` | factory   | extension-driven autoescape selector            |
|  [02]   | `make_logging_undefined(logger, base)`       | factory   | undefined subclass that logs every access       |
|  [03]   | `is_undefined(obj) -> bool`                  | function  | undefined-instance predicate                    |
|  [04]   | `clear_caches()`                             | function  | flush global template/lexer caches              |
|  [05]   | `pass_context(f)`                            | decorator | mark a callable to receive the render `Context` |
|  [06]   | `pass_environment(f)`                        | decorator | mark a callable to receive the `Environment`    |
|  [07]   | `pass_eval_context(f)`                       | decorator | mark a callable to receive the `EvalContext`    |

[ENTRYPOINT_SCOPE]: loader and bytecode-cache construction

| [INDEX] | [SURFACE]                                                                 | [SHAPE] | [CAPABILITY]                    |
| :-----: | :------------------------------------------------------------------------ | :------ | :------------------------------ |
|  [01]   | `FileSystemLoader(searchpath, encoding='utf-8', followlinks=False)`       | ctor    | disk template root(s)           |
|  [02]   | `PackageLoader(package_name, package_path='templates', encoding='utf-8')` | ctor    | package-resource template root  |
|  [03]   | `DictLoader(mapping)`                                                     | ctor    | in-memory `name -> source` map  |
|  [04]   | `FunctionLoader(load_func)`                                               | ctor    | callable source provider        |
|  [05]   | `PrefixLoader(mapping, delimiter='/')`                                    | ctor    | prefix-routed child loaders     |
|  [06]   | `ChoiceLoader(loaders)`                                                   | ctor    | ordered loader fallback chain   |
|  [07]   | `ModuleLoader(path)`                                                      | ctor    | pre-compiled-template loader    |
|  [08]   | `FileSystemBytecodeCache(directory=None, pattern='__jinja2_%s.cache')`    | ctor    | on-disk compiled-bytecode cache |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `Environment` is the single engine owner; report policy defaults to `autoescape=select_autoescape(...)`, `undefined=StrictUndefined`, and `enable_async=True`, so a missing variable faults rather than rendering blank.
- Every template source is a loader row on that one engine, never a parallel engine.
- `render`/`render_async` is the one sync/async pair; `generate`/`stream` own chunked output for large reports; the render context carries the `VisualSpec`/`ExportPlan` projection and the runtime `ContentIdentity`, never a re-minted identity.
- A template yielding a typed Python value binds `NativeEnvironment`/`NativeTemplate`, never `ast.literal_eval` over a string render; string reports stay on `Environment`.
- Custom tags, filters, tests, and globals are configuration rows on the mutable `Environment` registries via `add_extension`/`.filters`/`.tests`/`.globals` and the `pass_context`/`pass_environment`/`pass_eval_context` decorators, never a second engine.
- Untrusted report source binds `ImmutableSandboxedEnvironment`; the sandbox attribute/operator filters are the security boundary and `SecurityError` is a typed fault on the rail.
- `compile_templates(target)` feeds a parse-free `ModuleLoader` archive for sealed report sets; `FileSystemBytecodeCache`/`MemcachedBytecodeCache` caches a live engine for hot reload.
- jinja2 renders the report body offline into bytes the document/PDF owner consumes; live UI templating and browser state stay outside this package.

[STACKING]:
- `great-tables`(`.api/great-tables.md`): an `as_raw_html(inline_css=True)` fragment embeds into the report body via `{{ table_html | safe }}` or an autoescape-off block.
- `weasyprint`(`.api/weasyprint.md`) / `pymupdf`(`.api/pymupdf.md`) / `pikepdf`(`.api/pikepdf.md`): the assembled HTML body prints to PDF or stitches into a container; a `typst`(`.api/typst.md`)/LaTeX target consumes a `\(...\)`-delimited variant.
- `docxtpl`(`.api/docxtpl.md`): a shared `Environment` passed to `DocxTemplate.render(context, jinja_env=...)` keeps filters, globals, and the autoescape/undefined policy identical across the HTML report body and the `.docx` body.
- `InternationalizationExtension` stacks with `Babel` (the `i18n` extra) for locale-aware report strings beside the locale-aware `great-tables` cell formatting.
- within-lib: `document/report` composes the one `Environment` — loader rows, registries, and the sync/async render path — into the `DocumentNode` tree.
- universal rail: the render receipt is a `msgspec.Struct` admitted through `pydantic`, the boundary is `@beartype`-validated, and a `TemplateNotFound`/`UndefinedError`/`SecurityError` folds onto the `expression.Result` rail at the capsule while the `@receipted` weave carries the `structlog` event and `opentelemetry` span capturing template name, loader row, autoescape decision, undefined policy, and output byte length (`libs/python/.api`).

[LOCAL_ADMISSION]:
- BSD-3-Clause pure-Python wheel; admitted for the `document` report-templating rail. `MarkupSafe` is the runtime dependency; `Babel` enters only under the `i18n` extra.
- Import lazily at boundary scope (`import jinja2`); module-level import is banned by the manifest import policy.
- Sandbox attribute/operator filters stay enabled for every untrusted source; the owner never disables them to expose host internals.

[RAIL_LAW]:
- Package: `jinja2`
- Owns: report-templating composition — sandboxed engine, loader axis, undefined axis, autoescape policy, native-type rendering, the filter/test/global/extension registries, bytecode/precompile caching, and the sync/async render path.
- Accept: report-body rendering from a `VisualSpec`/`ExportPlan` and runtime `ContentIdentity` into bytes the document/PDF owner consumes; a shared `Environment` passed into `docxtpl`.
- Reject: wrapper-renames of `render`/`get_template`; a second engine per template source where a loader row suffices; a parallel engine for native output where `NativeEnvironment` is the row; a custom-tag hack where an `ext.Extension` belongs; live UI templating; identity minting the runtime owns.
