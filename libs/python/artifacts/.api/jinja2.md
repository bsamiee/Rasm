# [PY_ARTIFACTS_API_JINJA2]

`jinja2` supplies the report-templating composition surface for the artifacts report-templating owner: a sandboxed template engine, a loader axis, an undefined-handling axis, autoescape policy, and the sync/async render path that drives report generation from a `VisualSpec`/`ExportPlan` and a runtime `ContentIdentity`. The package owner composes `Environment`, the loader family, and the undefined family into one report-templating owner; it never re-implements lexing, parsing, or rendering jinja2 already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jinja2`
- package: `jinja2`
- import: `import jinja2` (lint alias `j2`)
- owner: `artifacts`
- rail: report-templating
- license: BSD-3-Clause (runtime dep `MarkupSafe>=2.0`; `Babel>=2.7` only under the `i18n` extra)
- asset: runtime library; pure Python (`py3-none-any`, flit-built), no ABI gate, cp315-clean (manifest floor `jinja2>=3.1.6`, no `python_version` marker)
- installed: `3.1.6` reflected via `assay api resolve jinja2`
- entry points: none (library only)
- capability: text/markup template engine; lexer, parser, compiler, sandboxed execution, loader hierarchy, autoescape policy, undefined-value algebra, sync and async rendering, native-type rendering, bytecode caching, mutable filter/test/global/policy registries, and the extension-hook family (i18n, expression-statement, loop-control, debug)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: engine and template roots
- rail: report-templating

Environment rows carry delimiter, whitespace, extension, undefined, finalize, autoescape, loader, cache, reload, bytecode, and async policy.

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]    | [CAPABILITY]                       |
| :-----: | :-------------------------------------- | :---------------- | :--------------------------------- |
|  [01]   | `Environment`                           | engine root       | configured compilation/render root; mutable `.filters`/`.tests`/`.globals`/`.policies` registries |
|  [02]   | `Template`                              | compiled unit     | sync/async render entrypoints      |
|  [03]   | `sandbox.SandboxedEnvironment`          | restricted engine | filtered untrusted-template engine |
|  [04]   | `sandbox.ImmutableSandboxedEnvironment` | hardened engine   | sandbox blocking object mutation   |
|  [05]   | `sandbox.SecurityError`                 | sandbox fault     | forbidden sandbox access           |
|  [06]   | `nativetypes.NativeEnvironment`         | native engine     | renders to the native Python object of a single expression, not a string |
|  [07]   | `nativetypes.NativeTemplate`            | native unit       | `render(...)` yields the evaluated Python value (int/list/dict) for typed report-value extraction |

[PUBLIC_TYPE_SCOPE]: loader axis
- rail: report-templating

| [INDEX] | [SYMBOL]           | [PACKAGE_ROLE]    | [CAPABILITY]                                                            |
| :-----: | :----------------- | :---------------- | :---------------------------------------------------------------------- |
|  [01]   | `BaseLoader`       | loader root       | abstract source provider; subclass overrides `get_source`               |
|  [02]   | `FileSystemLoader` | disk loader       | loads templates from a search path list                                 |
|  [03]   | `PackageLoader`    | package loader    | loads templates from an installed package's resource tree               |
|  [04]   | `DictLoader`       | in-memory loader  | loads templates from a `name -> source` mapping                         |
|  [05]   | `FunctionLoader`   | callable loader   | delegates source resolution to a callable                               |
|  [06]   | `PrefixLoader`     | namespaced loader | routes by prefix to a child loader                                      |
|  [07]   | `ChoiceLoader`     | fallback loader   | tries an ordered loader sequence                                        |
|  [08]   | `ModuleLoader`     | compiled loader   | loads pre-compiled templates emitted by `ModuleLoader`-targeted compile |

[PUBLIC_TYPE_SCOPE]: undefined axis and bytecode cache
- rail: report-templating

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]      | [CAPABILITY]                                                         |
| :-----: | :------------------------ | :------------------ | :------------------------------------------------------------------- |
|  [01]   | `Undefined`               | lenient undefined   | default missing-value placeholder; renders empty, raises on most ops |
|  [02]   | `ChainableUndefined`      | chainable undefined | permits attribute/index chaining on missing values                   |
|  [03]   | `DebugUndefined`          | debug undefined     | renders a debug marker for missing values                            |
|  [04]   | `StrictUndefined`         | strict undefined    | raises `UndefinedError` on any access                                |
|  [05]   | `BytecodeCache`           | cache root          | abstract compiled-bytecode cache                                     |
|  [06]   | `FileSystemBytecodeCache` | disk cache          | persists compiled bytecode to a directory                            |
|  [07]   | `MemcachedBytecodeCache`  | remote cache        | persists compiled bytecode to a memcached client                     |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: report-templating

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE]   | [CAPABILITY]                               |
| :-----: | :----------------------- | :--------------- | :----------------------------------------- |
|  [01]   | `TemplateError`          | fault root       | base of every template fault               |
|  [02]   | `TemplateNotFound`       | resolution fault | a named template is absent from the loader |
|  [03]   | `TemplatesNotFound`      | selection fault  | no template in a select list resolved      |
|  [04]   | `TemplateSyntaxError`    | parse fault      | template source failed to lex/parse        |
|  [05]   | `TemplateAssertionError` | compile fault    | a compile-time assertion failed            |
|  [06]   | `TemplateRuntimeError`   | runtime fault    | a runtime error raised during render       |
|  [07]   | `UndefinedError`         | undefined fault  | a strict/undefined access raised           |

[PUBLIC_TYPE_SCOPE]: extension family (`jinja2.ext`)
- rail: report-templating

Extensions register via `Environment(extensions=[...])` or `add_extension`; they add tags, filters, and parser hooks. `Extension` is the subclass base for custom `{% %}` tags. The shorthand identifier strings (`"jinja2.ext.i18n"`, `"jinja2.ext.do"`, `"jinja2.ext.loopcontrols"`, `"jinja2.ext.debug"`) resolve to these classes.

| [INDEX] | [SYMBOL]                            | [PACKAGE_ROLE]      | [CAPABILITY]                                                       |
| :-----: | :---------------------------------- | :------------------ | :----------------------------------------------------------------- |
|  [01]   | `ext.Extension`                     | extension base      | subclass to add custom `{% %}` tags, filters, and parse hooks      |
|  [02]   | `ext.InternationalizationExtension` | i18n extension      | `{% trans %}` / `gettext`/`ngettext` translation (`"jinja2.ext.i18n"`) |
|  [03]   | `ext.ExprStmtExtension`             | expr-stmt extension | `{% do %}` side-effecting expression statement (`"jinja2.ext.do"`) |
|  [04]   | `ext.LoopControlExtension`          | loop-control        | `{% break %}` / `{% continue %}` inside loops (`"jinja2.ext.loopcontrols"`) |
|  [05]   | `ext.DebugExtension`                | debug extension     | `{% debug %}` dumps the render context (`"jinja2.ext.debug"`)      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: engine construction and resolution
- rail: report-templating

The `Environment` row carries delimiter, whitespace, extension, undefined, finalize, autoescape, loader, cache, reload, bytecode, and async policy.

| [INDEX] | [SURFACE]                            | [CALL_SHAPE]                     | [CAPABILITY]                                        |
| :-----: | :----------------------------------- | :------------------------------- | :-------------------------------------------------- |
|  [01]   | `Environment`                        | engine configuration policy      | full engine configuration                           |
|  [02]   | `Environment.get_template`           | name plus globals policy         | resolve and compile a named template via the loader |
|  [03]   | `Environment.select_template`        | name list plus globals policy    | resolve the first available of a name list          |
|  [04]   | `Environment.get_or_select_template` | name or name-list input          | polymorphic resolve over a name or name list        |
|  [05]   | `Environment.from_string`            | source plus template policy      | compile an in-memory source string                  |
|  [06]   | `Environment.compile_expression`     | expression plus undefined policy | compile a single expression to a callable           |
|  [07]   | `Environment.overlay`                | partial config override          | derive a reconfigured child environment             |
|  [08]   | `Environment.add_extension`          | extension identifier             | register an extension after construction            |
|  [09]   | `Environment.compile`                | source/AST -> code               | `compile(source, name, filename, raw=False, defer_init=False)` — compile to a code object or (raw) Python module string for `ModuleLoader` |
|  [10]   | `Environment.compile_templates`      | loader -> compiled archive       | precompile every loader template to a zip/dir the `ModuleLoader` then serves |
|  [11]   | `Environment.filters` / `.tests` / `.globals` / `.policies` | mutable registries  | inject custom filters/tests/globals after construction (the surface `pass_context`/`pass_environment` decorate) |

[ENTRYPOINT_SCOPE]: render path (sync and async)
- rail: report-templating

Render rows share args/kwargs context; async rows require an async-enabled environment.

| [INDEX] | [SURFACE]               | [CALL_SHAPE]                  | [CAPABILITY]                                                 |
| :-----: | :---------------------- | :---------------------------- | :----------------------------------------------------------- |
|  [01]   | `Template.render`       | render context -> `str`       | render to a full string                                      |
|  [02]   | `Template.render_async` | async render context -> `str` | async render; requires `enable_async=True`                   |
|  [03]   | `Template.generate`     | render context -> iterator    | lazy chunked render                                          |
|  [04]   | `Template.stream`       | render context -> stream      | streamed render with `dump`/`disable_buffering`              |
|  [05]   | `Template.make_module`  | vars/shared/locals policy     | materialize exported names/blocks                            |
|  [06]   | `Template`              | source plus engine knobs      | construct a standalone template with an implicit environment |

[ENTRYPOINT_SCOPE]: policy, undefined, and module functions
- rail: report-templating

Policy rows carry extension defaults, logging base class, object probe, cache state, and context-passing decorator targets.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]               | [CAPABILITY]                                       |
| :-----: | :----------------------- | :------------------------- | :------------------------------------------------- |
|  [01]   | `select_autoescape`      | extension/default policy   | extension-driven autoescape selector               |
|  [02]   | `make_logging_undefined` | logger plus undefined base | undefined subclass that logs every access          |
|  [03]   | `is_undefined`           | object probe               | undefined-instance predicate                       |
|  [04]   | `clear_caches`           | no-arg cache flush         | flush global template/lexer caches                 |
|  [05]   | `pass_context`           | callable decorator         | mark a filter/test to receive the render `Context` |
|  [06]   | `pass_environment`       | callable decorator         | mark a callable to receive the `Environment`       |
|  [07]   | `pass_eval_context`      | callable decorator         | mark a callable to receive the `EvalContext`       |

[ENTRYPOINT_SCOPE]: loader and bytecode-cache construction
- rail: report-templating

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                                                              | [CAPABILITY]                    |
| :-----: | :------------------------ | :------------------------------------------------------------------------ | :------------------------------ |
|  [01]   | `FileSystemLoader`        | `FileSystemLoader(searchpath, encoding='utf-8', followlinks=False)`       | disk template root(s)           |
|  [02]   | `PackageLoader`           | `PackageLoader(package_name, package_path='templates', encoding='utf-8')` | package-resource template root  |
|  [03]   | `DictLoader`              | `DictLoader(mapping)`                                                     | in-memory `name -> source` map  |
|  [04]   | `FunctionLoader`          | `FunctionLoader(load_func)`                                               | callable source provider        |
|  [05]   | `PrefixLoader`            | `PrefixLoader(mapping, delimiter='/')`                                    | prefix-routed child loaders     |
|  [06]   | `ChoiceLoader`            | `ChoiceLoader(loaders)`                                                   | ordered loader fallback chain   |
|  [07]   | `ModuleLoader`            | `ModuleLoader(path)`                                                      | pre-compiled-template loader    |
|  [08]   | `FileSystemBytecodeCache` | `FileSystemBytecodeCache(directory=None, pattern='__jinja2_%s.cache')`    | on-disk compiled-bytecode cache |

## [04]-[IMPLEMENTATION_LAW]

[REPORT_TEMPLATING]:
- import: `import jinja2 as j2` at boundary scope only; module-level import is banned by the manifest import policy.
- engine construction: one `Environment` (or `SandboxedEnvironment` for untrusted report sources) is the single owner; `autoescape=select_autoescape(...)`, `undefined=StrictUndefined`, and `enable_async=True` are the default report-templating policy — a missing variable is a fault, not a silent blank.
- loader axis: the loader is a row value (`FileSystemLoader`, `PackageLoader`, `DictLoader`, `PrefixLoader`, `ChoiceLoader`), never a parallel engine per source; report template roots are loader rows.
- render axis: `render` and `render_async` are the one sync/async pair; `generate`/`stream` own chunked output for large reports; the render context carries the `VisualSpec`/`ExportPlan` projection and the runtime `ContentIdentity`, never a re-minted identity.
- native axis: when a template must yield a typed Python value (a computed number, a list, a dict) rather than a string, `NativeEnvironment`/`NativeTemplate.render` is the row — never `ast.literal_eval` over a string render; string reports stay on `Environment`.
- extension and registry axis: custom report tags register through `Environment(extensions=[...])` / `add_extension` (`ext.Extension` subclass or the `"jinja2.ext.*"` identifiers); custom filters/tests/globals attach to the mutable `Environment.filters`/`.tests`/`.globals` registries and use `pass_context`/`pass_environment`/`pass_eval_context` to receive engine state — these are configuration rows on the one engine, never a second engine.
- precompile axis: `compile_templates(target)` emits a zip/dir archive that `ModuleLoader(path)` serves with no parse cost; `FileSystemBytecodeCache`/`MemcachedBytecodeCache` is the per-template cache for the live `Environment` — choose precompiled archive for sealed report sets, bytecode cache for hot reload.
- runtime seam: jinja2 is markerless and cp315-clean (pure Python over `MarkupSafe`, which is cp315-wheeled), so the engine runs ON the cp315 core — no subprocess seam; `render_async`/`generate_async` integrate directly with the runtime `anyio` (`.api/anyio.md`) task scope rather than crossing a process boundary.
- evidence: each render captures the resolved template name, the loader row, the autoescape decision, the undefined policy, and the output byte length as a `msgspec.Struct` (`.api/msgspec.md`) templating receipt — emitted under one `structlog` (`.api/structlog.md`) event inside an OpenTelemetry (`.api/opentelemetry-api.md`) span; a `TemplateNotFound`/`UndefinedError`/`SecurityError` folds onto the `expression.Result` (`.api/expression.md`) rail rather than raising into the report producer.
- boundary: jinja2 renders the report body offline; the rendered bytes feed the document/PDF owner; live UI templating and browser state stay outside this package.

[STACKING]:
- the rendered HTML/markup feeds the document/PDF owners: a `great_tables` (`.api/great-tables.md`) `as_raw_html()` fragment embeds via `{{ table_html | safe }}` (or an autoescape-off block), the assembled body prints through `weasyprint` (`.api/weasyprint.md`) to PDF or stitches via `pymupdf`/`pikepdf`; a Typst/LaTeX target consumes a `\(...\)`-delimited variant
- the `docxtpl` (`.api/docxtpl.md`) DOCX rail uses jinja2 as its in-document engine — a shared `Environment` (passed to `DocxTemplate.render(context, jinja_env=...)`) keeps custom filters/globals identical across the HTML report body and the `.docx` body
- `select_autoescape` is the autoescape policy for the HTML/markup branch; the `i18n` extension (`InternationalizationExtension`) stacks with `Babel` (the declared `i18n` extra) for locale-aware report strings beside the locale-aware `great_tables` cell formatting

[SANDBOX_POLICY]:
- untrusted source: report templates from outside the package use `ImmutableSandboxedEnvironment`; a `SecurityError` is a typed fault on the templating rail, never an escape.
- attribute policy: the sandbox attribute/operator filters are the security boundary; the owner never disables them to expose host internals.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `jinja2`
- Owns: report-templating composition — sandboxed template engine, loader axis, undefined-handling axis, autoescape policy, native-type rendering, the extension/filter/test/global registries, bytecode/precompile caching, and the sync/async render path that drives report generation
- Accept: report-body rendering from a `VisualSpec`/`ExportPlan` and runtime `ContentIdentity` into bytes the document/PDF owner consumes; a shared `Environment` passed into `docxtpl`'s render
- Reject: wrapper-renames of `render`/`get_template`; a second engine per template source where a loader row suffices; a parallel engine for native-value output where `NativeEnvironment` is the row; a custom-tag hack where an `ext.Extension` belongs; live UI templating; identity minting the runtime already owns
