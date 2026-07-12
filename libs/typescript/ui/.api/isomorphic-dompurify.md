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
- Every type is re-exported from `dompurify`; `Config` is the policy vocabulary and the hook types are the extension surface.

| [INDEX] | [SYMBOL]                                                                                                                   | [TYPE_FAMILY]      | [CONSUMER_BOUNDARY]                                                                                                               |
| :-----: | :------------------------------------------------------------------------------------------------------------------------- | :----------------- | :-------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Config`                                                                                                                   | policy vocabulary  | the one allow/deny + return-mode record every `sanitize` call parameterizes                                                       |
|  [02]   | `DOMPurify`                                                                                                                | instance interface | the default export shape: `sanitize`/`addHook`/`setConfig`/`isValidAttribute`/`removed`/`version`/`isSupported`                   |
|  [03]   | `HookName` / `NodeHook` / `ElementHook` / `DocumentFragmentHook` / `UponSanitizeElementHook` / `UponSanitizeAttributeHook` | hook types         | the six-entry-point extension axis + their hook-function signatures                                                               |
|  [04]   | `UponSanitizeElementHookEvent` / `UponSanitizeAttributeHookEvent`                                                          | hook event         | the `allowedTags`/`allowedAttributes`/`attrName`/`attrValue` mutation payload passed to `upon*` hooks                             |
|  [05]   | `RemovedElement` / `RemovedAttribute`                                                                                      | audit row          | the `removed[]` diagnostic — what the last sanitize stripped, for a telemetry/quarantine receipt                                  |
|  [06]   | `WindowLike`                                                                                                               | window shape       | `Pick<globalThis, 'DocumentFragment'\|'HTMLTemplateElement'\|'Node'\|'Element'\|…>` — the jsdom/native window the sanitizer needs |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the sanitize gate — one config-discriminated operation
- rail: sanitize
- `sanitize` is overloaded on the `Config` return flags; the same call is the string gate, the Trusted Types gate, the DOM/fragment gate, or the in-place scrubber — one operation, mode as data.

| [INDEX] | [SURFACE]                                                                                                   | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                                                                                                 |
| :-----: | :---------------------------------------------------------------------------------------------------------- | :------------- | :---------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `sanitize(dirty: string \| Node, cfg?: Config): string`                                                     | string gate    | the default — DOM-bound wire text before `dangerouslySetInnerHTML`; `view/compose.md` rich content, `intl/message.md` HTML messages |
|  [02]   | `sanitize(dirty, cfg & { RETURN_TRUSTED_TYPE: true }): TrustedHTML`                                         | CSP gate       | strict-CSP surfaces — feeds a `TrustedHTML` sink under a `TRUSTED_TYPES_POLICY`                                                     |
|  [03]   | `sanitize(dirty, cfg & { RETURN_DOM: true \| RETURN_DOM_FRAGMENT: true }): HTMLElement \| DocumentFragment` | DOM gate       | when the caller needs a node, not a string, without a re-parse                                                                      |
|  [04]   | `sanitize(dirty: Node, cfg & { IN_PLACE: true }): Node`                                                     | in-place scrub | scrub an existing detached node tree without serialization                                                                          |
|  [05]   | `setConfig(cfg?: Config)` / `clearConfig()`                                                                 | global policy  | pin a project-wide allow-list once (`ALLOWED_TAGS`/`USE_PROFILES`); clear to defaults                                               |
|  [06]   | `isValidAttribute(tag, attr, value): boolean` / `removed: (RemovedElement \| RemovedAttribute)[]`           | policy probe   | pre-check one attribute; read what the last call stripped for an audit row                                                          |
|  [07]   | `isSupported: boolean` / `version: string` / `clearWindow(): void`                                          | runtime state  | `isSupported` gates the browser path; `clearWindow` resets the cached node jsdom window between SSR renders                         |

[ENTRYPOINT_SCOPE]: hooks — the extension axis (six entry points)
- rail: sanitize
- `addHook`/`removeHook` are overloaded by entry-point group; a hook mutates the node/attribute mid-sanitize. `removeHooks(entryPoint)` clears one point; `removeAllHooks()` clears all.

| [INDEX] | [SURFACE]                                                                                                                 | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                                                                    |
| :-----: | :------------------------------------------------------------------------------------------------------------------------ | :------------- | :----------------------------------------------------------------------------------------------------- |
|  [01]   | `addHook('beforeSanitizeElements'\|'afterSanitizeElements'\|'uponSanitizeShadowNode', NodeHook)`                          | node hook      | per-element mutation before/after the element pass                                                     |
|  [02]   | `addHook('beforeSanitizeAttributes'\|'afterSanitizeAttributes', ElementHook)`                                             | element hook   | per-element attribute-pass mutation                                                                    |
|  [03]   | `addHook('beforeSanitizeShadowDOM'\|'afterSanitizeShadowDOM', DocumentFragmentHook)`                                      | fragment hook  | shadow-DOM fragment pass                                                                               |
|  [04]   | `addHook('uponSanitizeElement', UponSanitizeElementHook)` / `addHook('uponSanitizeAttribute', UponSanitizeAttributeHook)` | decision hook  | inspect/override the allow decision with the hook-event payload (e.g. enforce a link-target allowlist) |
|  [05]   | `removeHook(entryPoint, fn?)` / `removeHooks(entryPoint)` / `removeAllHooks()`                                            | hook teardown  | remove one hook, all hooks at a point, or every hook                                                   |

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
