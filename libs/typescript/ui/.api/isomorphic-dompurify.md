# [TS_UI_API_ISOMORPHIC_DOMPURIFY]

`isomorphic-dompurify` wraps DOMPurify 3.x with one dual-runtime resolution: on node it backs the sanitizer with a cached `jsdom` window (reset by `clearWindow()`), on the browser it uses the native `window` — the same `sanitize(dirty, cfg)` gate, both sides, so a server-prerendered string and a client-hydrated string pass identical policy. It re-exports the entire DOMPurify type surface. The sanitizer is ONE config-discriminated operation, not a family: the return type (`string` / `TrustedHTML` / `DocumentFragment` / `HTMLElement` / in-place `Node`) is selected by the `Config` flags, and the `Config` itself is the one allow/deny policy vocabulary. Hooks are the extension axis. In this stack it is the render-boundary ceiling: every DOM-bound wire string passes through it once before it reaches `dangerouslySetInnerHTML`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `isomorphic-dompurify`
- package: `isomorphic-dompurify`
- license: `MIT`
- deps: `dompurify ^catalog` (the sanitizer + full re-exported type surface), `jsdom catalog` (node `WindowLike` backing)
- catalog-verdict: KEEP
- runtime: isomorphic — conditional export: node (`dist/index`, jsdom window + `clearWindow`) vs browser (`dist/browser`, native window, explicitly-overloaded hooks); one import site, resolution by condition
- entry: `.` — default export is the `DOMPurify` instance; named exports are the bound methods + the re-exported `dompurify` types

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the re-exported DOMPurify type surface
- rail: sanitize
- Every type is re-exported from `dompurify`; `Config` is the policy vocabulary and the hook types are the extension surface. The default export is the `DOMPurify` instance (`sanitize`/`addHook`/`setConfig`/`isValidAttribute`/`removed`/`version`/`isSupported`).

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]      | [CONSUMER_BOUNDARY]                                                    |
| :-----: | :------------------------------------ | :----------------- | :--------------------------------------------------------------------- |
|  [01]   | `Config`                              | policy vocabulary  | the one allow/deny + return-mode record every `sanitize` parameterizes |
|  [02]   | `DOMPurify`                           | instance interface | the default-export instance shape; methods listed in the lead          |
|  [03]   | `HookName`                            | hook entry union   | the string union of the entry points keying every `addHook`            |
|  [04]   | `NodeHook`                            | hook fn            | per-element mutation callback, before/after the element pass           |
|  [05]   | `ElementHook`                         | hook fn            | per-element attribute-pass mutation callback                           |
|  [06]   | `DocumentFragmentHook`                | hook fn            | shadow-DOM fragment-pass mutation callback                             |
|  [07]   | `UponSanitizeElementHook`             | decision hook fn   | inspect or override the element allow decision                         |
|  [08]   | `UponSanitizeAttributeHook`           | decision hook fn   | inspect or override the attribute allow decision                       |
|  [09]   | `UponSanitizeElementHookEvent`        | hook event         | the `allowedTags`/`allowedAttributes` mutation payload                 |
|  [10]   | `UponSanitizeAttributeHookEvent`      | hook event         | the `attrName`/`attrValue` mutation payload                            |
|  [11]   | `RemovedElement` / `RemovedAttribute` | audit row          | the `removed[]` diagnostic — what the last sanitize stripped           |
|  [12]   | `WindowLike`                          | window shape       | the jsdom/native window shape (`DocumentFragment`, `Node`, `Element`)  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the sanitize gate — one config-discriminated operation
- rail: sanitize
- `sanitize` is overloaded on the `Config` return flags; the same call is the string gate, the Trusted Types gate, the DOM/fragment gate, or the in-place scrubber — one operation, mode as data.
- Every overload is `sanitize(dirty, cfg?)` with `dirty: string | Node`; the `Config` return flag selects the return type. `isValidAttribute` returns `boolean`, `removed` is `(RemovedElement | RemovedAttribute)[]`.

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                          |
| :-----: | :----------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `sanitize(dirty, cfg?)` → `string`               | string gate    | default DOM-bound wire text before `dangerouslySetInnerHTML` |
|  [02]   | `+ RETURN_TRUSTED_TYPE` → `TrustedHTML`          | CSP gate       | a `TrustedHTML` sink under a `TRUSTED_TYPES_POLICY`          |
|  [03]   | `+ RETURN_DOM` → `HTMLElement`                   | DOM gate       | a node, not a string, without a re-parse                     |
|  [04]   | `+ RETURN_DOM_FRAGMENT` → `DocumentFragment`     | DOM gate       | a detached fragment, without a re-parse                      |
|  [05]   | `+ IN_PLACE` (`dirty: Node`) → `Node`            | in-place scrub | scrub a detached node tree without serialization             |
|  [06]   | `setConfig(cfg?)` / `clearConfig()`              | global policy  | pin a project-wide allow-list; clear to defaults             |
|  [07]   | `isValidAttribute(tag, attr, value)` / `removed` | policy probe   | pre-check one attribute; read what the last call stripped    |
|  [08]   | `isSupported` / `version` / `clearWindow()`      | runtime state  | gate the browser path; reset the cached node jsdom window    |

[ENTRYPOINT_SCOPE]: hooks — the extension axis
- rail: sanitize
- `addHook(entryPoint, fn)` binds a hook by entry-point string; a hook mutates the node/attribute mid-sanitize. Teardown is `removeHook(entryPoint, fn?)` / `removeHooks(entryPoint)` / `removeAllHooks()`.

| [INDEX] | [ENTRY_POINT]              | [HOOK_TYPE]                 | [ROLE]                                        |
| :-----: | :------------------------- | :-------------------------- | :-------------------------------------------- |
|  [01]   | `beforeSanitizeElements`   | `NodeHook`                  | element pass, before                          |
|  [02]   | `afterSanitizeElements`    | `NodeHook`                  | element pass, after                           |
|  [03]   | `uponSanitizeShadowNode`   | `NodeHook`                  | shadow-node pass                              |
|  [04]   | `beforeSanitizeAttributes` | `ElementHook`               | attribute pass, before                        |
|  [05]   | `afterSanitizeAttributes`  | `ElementHook`               | attribute pass, after                         |
|  [06]   | `beforeSanitizeShadowDOM`  | `DocumentFragmentHook`      | shadow-DOM fragment pass, before              |
|  [07]   | `afterSanitizeShadowDOM`   | `DocumentFragmentHook`      | shadow-DOM fragment pass, after               |
|  [08]   | `uponSanitizeElement`      | `UponSanitizeElementHook`   | inspect/override the element allow decision   |
|  [09]   | `uponSanitizeAttribute`    | `UponSanitizeAttributeHook` | inspect/override the attribute allow decision |

## [04]-[IMPLEMENTATION_LAW]

[SANITIZE_TOPOLOGY]:
- one gate, mode as data: `sanitize` is a single overloaded operation; `RETURN_TRUSTED_TYPE`/`RETURN_DOM`/`RETURN_DOM_FRAGMENT`/`IN_PLACE` on the `Config` select the return type. Never call four functions — pass the flag.
- `Config` is the one policy vocabulary: `ALLOWED_TAGS`/`ALLOWED_ATTR`/`ALLOWED_URI_REGEXP` (allow), `FORBID_TAGS`/`FORBID_ATTR` (deny), `ADD_TAGS`/`ADD_ATTR` (extend), `USE_PROFILES` (`{ html, svg, svgFilters, mathMl }`, overrides `ALLOWED_TAGS`), `WHOLE_DOCUMENT`, `SANITIZE_DOM`/`SANITIZE_NAMED_PROPS` (clobbering defense), `CUSTOM_ELEMENT_HANDLING`, `TRUSTED_TYPES_POLICY`. Policy is data, not branching.
- isomorphic resolution: node caches one `jsdom` window and runs the same DOMPurify over it; `clearWindow()` disposes it so a long-lived SSR process does not leak the window between renders. The browser path uses the native `window`; `isSupported` is `false` where neither exists.
- the render-boundary ceiling: a mXSS payload in a decoded wire string never reaches the DOM because sanitize runs at the boundary once; re-sanitizing per render is waste and re-parsing an already-sanitized string is a defect.

[INTEGRATION_LAW]:
- Stack with `effect` (`libs/typescript/.api/effect.md`): `sanitize` is a pure boundary function folded at the render seam — the `wire`/`interchange` `QuarantineFold.sanitize` row runs it ONCE at the tolerance terminal over every DOM-bound field of a `Schema`-decoded value, so the sanitizer is the rendered-text ceiling the ingress charter owns and `ui` receives already-clean strings. `removed[]` and a `Breaking` policy hit surface as a typed `FaultDetail`, never a thrown exception.
- Stack with `react` (`.api/react.md`): the sanitized `string` feeds `dangerouslySetInnerHTML`, or the `TrustedHTML` return feeds a Trusted-Types sink under strict CSP. The HTML-bearing rows are `viewer/mark/bcf.md` BCF topic-comment markup, `intl/message.md` rich message HTML, and any `view/compose.md` content surface binding decoded markup.
- Stack with the platform SSR/hydration split (`libs/typescript/.api/effect-platform-browser.md` / node runtime): the node prerender sanitizes over the jsdom window and the browser hydration over the native window — one policy both sides, `clearWindow()` between server renders. `isSupported` gates whether the browser fast-path or a no-op degrade applies.
- Stack with Trusted Types + CSP: `RETURN_TRUSTED_TYPE: true` + `TRUSTED_TYPES_POLICY` make sanitize the single `TrustedHTML` producer, so a `require-trusted-types-for 'script'` CSP is satisfiable without disabling the policy at the sink.

[LOCAL_ADMISSION]:
- Sanitize every untrusted HTML-bearing string once at the ingress/render boundary through the `QuarantineFold.sanitize` row; never inline `DOMPurify.sanitize` per component or re-sanitize an already-clean value.
- Pin the allow-list via `setConfig` or an explicit per-call `Config` (prefer `USE_PROFILES` for the common HTML profile); never widen `ADD_TAGS`/`ADD_ATTR` without a policy reason.
- On node SSR, call `clearWindow()` between independent renders to bound the jsdom window; on the browser, gate on `isSupported`.
- Under strict CSP, return `TrustedHTML` (`RETURN_TRUSTED_TYPE`) rather than a raw string into a Trusted-Types sink.

[RAIL_LAW]:
- Package: `isomorphic-dompurify`
- Owns: the dual-runtime DOMPurify sanitize gate, the `Config` allow/deny/return-mode policy vocabulary, the six-entry-point hook axis, the `removed[]` audit, Trusted-Types output, and the node/browser window resolution (`clearWindow`/`isSupported`)
- Accept: one boundary `sanitize` call parameterized by `Config`, project policy via `setConfig`, `USE_PROFILES` for common HTML, `RETURN_TRUSTED_TYPE` under CSP, hooks for decision overrides, `clearWindow` between SSR renders, invocation at the `QuarantineFold` terminal
- Reject: per-render or per-component sanitize calls, re-sanitizing a clean string, a hand-rolled tag/attr allowlist beside `Config`, unsanitized markup reaching `dangerouslySetInnerHTML`, widening the allow-list without cause
