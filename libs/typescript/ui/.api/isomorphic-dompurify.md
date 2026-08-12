# [TS_UI_API_ISOMORPHIC_DOMPURIFY]

`isomorphic-dompurify` wraps DOMPurify with one dual-runtime resolution: node backs the sanitizer with a cached `jsdom` window, the browser binds the native `window`, and one `sanitize(dirty, cfg)` gate runs identical policy both sides so a prerendered string and a hydrated string clear the same allow-list.

`sanitize` is one `Config`-discriminated operation, not a family — the return flag selects the return type, hooks extend it, and it is the render-boundary ceiling every DOM-bound wire string clears once before `dangerouslySetInnerHTML`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `isomorphic-dompurify`
- package: `isomorphic-dompurify` (MIT)
- module: `.` conditional export — node backs a `jsdom` window (`clearWindow` disposes it), browser binds native `window`; one import, resolution by export condition
- runtime: isomorphic — `isSupported` reads `false` where neither window exists; the node condition builds its window at import, so resolving this module costs one document
- depends: `dompurify` re-exports the full type surface and the sanitizer; `jsdom` backs the node window
- rail: sanitize — the render-boundary ceiling every DOM-bound wire string clears once

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `isomorphic-dompurify` re-exports the DOMPurify type surface whole from `dompurify` — `Config` is the policy vocabulary, the hook types the extension surface, `DOMPurify` the default-export instance.

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]      | [CAPABILITY]                                     |
| :-----: | :------------------------------------ | :----------------- | :----------------------------------------------- |
|  [01]   | `Config`                              | policy vocabulary  | allow/deny/return-mode record every gate reads   |
|  [02]   | `DOMPurify`                           | instance interface | default-export instance shape                    |
|  [03]   | `HookName`                            | hook entry union   | string union keying every `addHook`              |
|  [04]   | `NodeHook`                            | hook fn            | element-pass mutation callback                   |
|  [05]   | `ElementHook`                         | hook fn            | attribute-pass mutation callback                 |
|  [06]   | `DocumentFragmentHook`                | hook fn            | shadow-DOM fragment-pass callback                |
|  [07]   | `UponSanitizeElementHook`             | decision hook fn   | override the element allow decision              |
|  [08]   | `UponSanitizeAttributeHook`           | decision hook fn   | override the attribute allow decision            |
|  [09]   | `UponSanitizeElementHookEvent`        | hook event         | `allowedTags`/`allowedAttributes` payload        |
|  [10]   | `UponSanitizeAttributeHookEvent`      | hook event         | `attrName`/`attrValue` payload                   |
|  [11]   | `RemovedElement` / `RemovedAttribute` | audit row          | `removed[]` strip diagnostic                     |
|  [12]   | `WindowLike`                          | window shape       | jsdom/native window (`DocumentFragment`, `Node`) |

[CONFIG_POLICY]: `Config` is data, not branching — one record parameterizes every gate, return-mode flags in the entrypoint table:
- allow/deny/extend: `ALLOWED_TAGS` `ALLOWED_ATTR` `ALLOWED_URI_REGEXP` / `FORBID_TAGS` `FORBID_ATTR` / `ADD_TAGS` `ADD_ATTR`
- profile: `USE_PROFILES` (`html`/`svg`/`svgFilters`/`mathMl`, overrides `ALLOWED_TAGS`)
- defense: `SANITIZE_DOM` `SANITIZE_NAMED_PROPS` `WHOLE_DOCUMENT` `CUSTOM_ELEMENT_HANDLING` `TRUSTED_TYPES_POLICY`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `sanitize` is one overloaded gate — `dirty: string | Node`, the `Config` return flag selecting the return type.

| [INDEX] | [SURFACE]                                            | [SHAPE]  | [CAPABILITY]                |
| :-----: | :--------------------------------------------------- | :------- | :-------------------------- |
|  [01]   | `sanitize(dirty, cfg?)` -> `string`                  | instance | boundary string gate        |
|  [02]   | `+RETURN_TRUSTED_TYPE` -> `TrustedHTML`              | instance | Trusted Types CSP gate      |
|  [03]   | `+RETURN_DOM` -> `HTMLElement`                       | instance | DOM node, no re-parse       |
|  [04]   | `+RETURN_DOM_FRAGMENT` -> `DocumentFragment`         | instance | detached fragment           |
|  [05]   | `+IN_PLACE (dirty: Node)` -> `Node`                  | instance | in-place node scrub         |
|  [06]   | `setConfig(cfg?)` / `clearConfig()`                  | instance | pin or clear project policy |
|  [07]   | `isValidAttribute(string, string, string)` -> `bool` | instance | pre-check one attribute     |
|  [08]   | `removed`                                            | property | last-call strip audit       |
|  [09]   | `isSupported` / `version`                            | property | import-time state snapshot  |
|  [10]   | `clearWindow()`                                      | function | reset cached node window    |
|  [11]   | `DOMPurify(root)` -> `DOMPurify`                     | factory  | a sanitizer over one window |

- `clearWindow()`: closes the node window and rebuilds the sanitizer over a fresh one, so every `addHook` registration and any `setConfig` policy dies with it and the caller re-pins both before the next `sanitize`.
- `isSupported` / `version`: plain values captured when the module resolved, never live getters — `clearWindow()` leaves both reading the state of the window they were taken from.
- `DOMPurify(root)`: calling the default export mints a sanitizer bound to a caller-supplied window, so a host needing the node document's own construction options — a quiet virtual console, a resource policy — builds that window itself and mints against it.

[ENTRYPOINT_SCOPE]: `addHook(entryPoint, fn)` binds a hook by entry-point string; a bound hook mutates the node or attribute mid-sanitize. Teardown is `removeHook(entryPoint, fn?)` / `removeHooks(entryPoint)` / `removeAllHooks()`.

| [INDEX] | [ENTRY_POINT]              | [HOOK_TYPE]                 | [ROLE]                           |
| :-----: | :------------------------- | :-------------------------- | :------------------------------- |
|  [01]   | `beforeSanitizeElements`   | `NodeHook`                  | element pass, before             |
|  [02]   | `afterSanitizeElements`    | `NodeHook`                  | element pass, after              |
|  [03]   | `uponSanitizeShadowNode`   | `NodeHook`                  | shadow-node pass                 |
|  [04]   | `beforeSanitizeAttributes` | `ElementHook`               | attribute pass, before           |
|  [05]   | `afterSanitizeAttributes`  | `ElementHook`               | attribute pass, after            |
|  [06]   | `beforeSanitizeShadowDOM`  | `DocumentFragmentHook`      | shadow-DOM fragment pass, before |
|  [07]   | `afterSanitizeShadowDOM`   | `DocumentFragmentHook`      | shadow-DOM fragment pass, after  |
|  [08]   | `uponSanitizeElement`      | `UponSanitizeElementHook`   | override the element decision    |
|  [09]   | `uponSanitizeAttribute`    | `UponSanitizeAttributeHook` | override the attribute decision  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Pass the return flag for a return mode; four separate functions is the deleted form.
- node caches one bare doctype-only `jsdom` window under the same sanitizer — no scripts, no resource loading, no styles the parse reaches — and that emptiness carries the isolation the gate rests on; the browser path binds the native `window`.
- window and sanitizer share one lifetime: replacing the window replaces the sanitizer, so hook registrations and pinned policy belong to a window generation and re-establish whenever it turns over.
- jsdom's CSSOM backs the node window, computing every length in pixels and serializing CSS functions canonically, so a hook reading a computed style reads resolved px and a spelling the author's own units never wrote.
- errors the document raises — a style block that will not parse, a resource the policy declines — reach the process console through jsdom's own error channel rather than the sanitize return, so untrusted markup is a log-volume surface as well as a policy one.
- sanitize runs once at the render boundary — re-sanitizing per render is waste, re-parsing a sanitized string a defect.

[STACKING]:
- `effect`(`libs/typescript/.api/effect.md`): `sanitize` folds once at the ingress boundary over every DOM-bound field of a `Schema`-decoded value; `removed[]` and a refused policy surface as a typed `Data.TaggedError` on the fault channel `catchTag` recovers, never a throw.
- `react`(`.api/react.md`): the sanitized `string` feeds `dangerouslySetInnerHTML`, or a `TrustedHTML` return feeds a Trusted-Types sink under strict CSP.
- `effect-platform-browser`(`libs/typescript/.api/effect-platform-browser.md`): the node prerender sanitizes over the jsdom window and browser hydration over the native window — one policy both sides, `clearWindow()` between server renders, `isSupported` gating the browser fast-path.
- within-lib: `ui` receives already-clean strings because the ingress charter owns the sanitize ceiling, so no `view` row re-sanitizes.

[LOCAL_ADMISSION]:
- Sanitize every untrusted HTML-bearing string once at the ingress/render boundary; a per-component `DOMPurify.sanitize` call is the deleted form.
- Pin the allow-list through `setConfig` or an explicit per-call `Config`, `USE_PROFILES` for the common HTML profile; widening `ADD_TAGS`/`ADD_ATTR` carries a stated policy reason.
- node SSR reuses the one cached window across renders, since `sanitize` leaves no per-render document state; `clearWindow()` serves a long-lived process reclaiming the document, and the same pass that calls it re-registers every hook and re-pins the policy.
- Own the node document's construction — its console, its resource policy — by building the window and minting through `DOMPurify(root)`; the browser path gates on `isSupported`.
- Strict CSP returns `TrustedHTML` (`RETURN_TRUSTED_TYPE`) into a Trusted-Types sink.

[RAIL_LAW]:
- Package: `isomorphic-dompurify`
- Owns: the dual-runtime DOMPurify sanitize gate, the `Config` allow/deny/return-mode vocabulary, the hook extension axis, the `removed[]` audit, Trusted-Types output, and the node/browser window resolution (`clearWindow`/`isSupported`/`DOMPurify(root)`)
- Accept: one boundary `sanitize` parameterized by `Config`, project policy via `setConfig`, `USE_PROFILES` for common HTML, `RETURN_TRUSTED_TYPE` under CSP, hooks for decision overrides, one long-lived node window, `DOMPurify(root)` where the host owns the document's construction, invocation once at the ingress/render boundary
- Reject: per-render or per-component sanitize, re-sanitizing a clean string, a hand-rolled tag/attr allowlist beside `Config`, a hook or pinned policy assumed to survive `clearWindow()`, unsanitized markup reaching `dangerouslySetInnerHTML`
