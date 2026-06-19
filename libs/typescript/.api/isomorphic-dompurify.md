# [API_CATALOGUE] isomorphic-dompurify

Grounded from installed `node_modules` type declarations (`isomorphic-dompurify` 3.16.0, peer
`dompurify` 3.4.10, reflected from `dist/index.d.ts` and `dist/browser.d.ts`). Every surface below
is verbatim from the declaration files with exact spellings. `isomorphic-dompurify` wraps
`dompurify` to provide a single entry point that works in both Node.js (using a DOM emulation
environment) and browser environments. The package exports a pre-instantiated `DOMPurify` instance
as the default export plus all flat named exports from it, and re-exports all public type surfaces
from `dompurify`. The `sanitize`, `addHook`, `removeHook`, `removeHooks`, `removeAllHooks`,
`setConfig`, `clearConfig`, `isValidAttribute`, `isSupported`, `version`, and `removed` members are
bound directly to the default instance; `clearWindow` resets the internal DOM environment.

---

## [01]-[PACKAGE_SURFACE]

```ts
// isomorphic-dompurify — dist/index.d.ts (node/universal entry)
import { DOMPurify as DOMPurify$1 } from 'dompurify';
export {
  Config, DOMPurify, DocumentFragmentHook, ElementHook, HookName, NodeHook,
  RemovedAttribute, RemovedElement, UponSanitizeAttributeHook, UponSanitizeAttributeHookEvent,
  UponSanitizeElementHook, UponSanitizeElementHookEvent, WindowLike
} from 'dompurify';

declare const DOMPurify: DOMPurify$1;
declare const sanitize:        DOMPurify$1["sanitize"]
declare const isSupported:     boolean
declare const addHook:         DOMPurify$1["addHook"]
declare const removeHook:      DOMPurify$1["removeHook"]
declare const removeHooks:     DOMPurify$1["removeHooks"]
declare const removeAllHooks:  DOMPurify$1["removeAllHooks"]
declare const setConfig:       DOMPurify$1["setConfig"]
declare const clearConfig:     DOMPurify$1["clearConfig"]
declare const isValidAttribute: DOMPurify$1["isValidAttribute"]
declare const version:         string
declare const removed:         DOMPurify$1['removed']
declare function clearWindow(): void

export {
  addHook, clearConfig, clearWindow, DOMPurify as default,
  isSupported, isValidAttribute, removeAllHooks, removeHook, removeHooks,
  removed, sanitize, setConfig, version
}
```

The browser entry (`dist/browser.d.ts`) is identical in shape but omits the `DOMPurify` re-export namespace alias; it exports the same flat named set plus re-exports types from `dompurify`.

---

## [02]-[DOMPURIFY_INTERFACE]

The core sanitizer contract. `DOMPurify` is a callable interface — calling it with a `WindowLike` creates a new isolated instance; the default export is already instantiated.

```ts
// dompurify — DOMPurify interface
interface DOMPurify {
  (root?: WindowLike): DOMPurify

  version: string
  removed: Array<RemovedElement | RemovedAttribute>
  isSupported: boolean

  sanitize(dirty: string | Node, cfg: Config & { RETURN_TRUSTED_TYPE: true }): TrustedHTML
  sanitize(dirty: Node, cfg: Config & { IN_PLACE: true }): Node
  sanitize(dirty: string | Node, cfg: Config & { RETURN_DOM: true }): Node
  sanitize(dirty: string | Node, cfg: Config & { RETURN_DOM_FRAGMENT: true }): DocumentFragment
  sanitize(dirty: string | Node, cfg?: Config): string

  isValidAttribute(tag: string, attr: string, value: string): boolean

  setConfig(cfg?: Config): void
  clearConfig(): void

  addHook(entryPoint: "beforeSanitizeElements" | "afterSanitizeElements" | "uponSanitizeShadowNode", hookFunction: NodeHook): void
  addHook(entryPoint: "beforeSanitizeAttributes" | "afterSanitizeAttributes", hookFunction: ElementHook): void
  addHook(entryPoint: "beforeSanitizeShadowDOM" | "afterSanitizeShadowDOM", hookFunction: DocumentFragmentHook): void
  addHook(entryPoint: "uponSanitizeElement", hookFunction: UponSanitizeElementHook): void
  addHook(entryPoint: "uponSanitizeAttribute", hookFunction: UponSanitizeAttributeHook): void

  removeHook(entryPoint: "beforeSanitizeElements" | "afterSanitizeElements" | "uponSanitizeShadowNode", hookFunction?: NodeHook): NodeHook | undefined
  removeHook(entryPoint: "beforeSanitizeAttributes" | "afterSanitizeAttributes", hookFunction?: ElementHook): ElementHook | undefined
  removeHook(entryPoint: "beforeSanitizeShadowDOM" | "afterSanitizeShadowDOM", hookFunction?: DocumentFragmentHook): DocumentFragmentHook | undefined
  removeHook(entryPoint: "uponSanitizeElement", hookFunction?: UponSanitizeElementHook): UponSanitizeElementHook | undefined
  removeHook(entryPoint: "uponSanitizeAttribute", hookFunction?: UponSanitizeAttributeHook): UponSanitizeAttributeHook | undefined

  removeHooks(entryPoint: HookName): void
  removeAllHooks(): void
}
```

---

## [03]-[CONFIG]

The sanitizer configuration passed to `sanitize` / `setConfig`.

```ts
// dompurify — Config
interface Config {
  ADD_ATTR?:                 string[] | ((attributeName: string, tagName: string) => boolean) | undefined
  ADD_DATA_URI_TAGS?:        string[] | undefined
  ADD_TAGS?:                 string[] | ((tagName: string) => boolean) | undefined
  ADD_URI_SAFE_ATTR?:        string[] | undefined
  ALLOW_ARIA_ATTR?:          boolean | undefined
  ALLOW_DATA_ATTR?:          boolean | undefined
  ALLOW_UNKNOWN_PROTOCOLS?:  boolean | undefined
  ALLOW_SELF_CLOSE_IN_ATTR?: boolean | undefined
  ALLOWED_ATTR?:             string[] | undefined
  ALLOWED_TAGS?:             string[] | undefined
  ALLOWED_NAMESPACES?:       string[] | undefined
  ALLOWED_URI_REGEXP?:       RegExp | undefined
  CUSTOM_ELEMENT_HANDLING?: {
    tagNameCheck?:               RegExp | ((tagName: string) => boolean) | null | undefined
    attributeNameCheck?:         RegExp | ((attributeName: string, tagName?: string) => boolean) | null | undefined
    allowCustomizedBuiltInElements?: boolean | undefined
  }
  FORBID_ATTR?:         string[] | undefined
  FORBID_CONTENTS?:     string[] | undefined
  ADD_FORBID_CONTENTS?: string[] | undefined
  FORBID_TAGS?:         string[] | undefined
  FORCE_BODY?:          boolean | undefined
  HTML_INTEGRATION_POINTS?: Record<string, boolean> | undefined
  IN_PLACE?:            boolean | undefined
  KEEP_CONTENT?:        boolean | undefined
  MATHML_TEXT_INTEGRATION_POINTS?: Record<string, boolean> | undefined
  NAMESPACE?:           string | undefined
  PARSER_MEDIA_TYPE?:   DOMParserSupportedType | undefined
  RETURN_DOM_FRAGMENT?: boolean | undefined
  RETURN_DOM?:          boolean | undefined
  RETURN_TRUSTED_TYPE?: boolean | undefined
  SAFE_FOR_TEMPLATES?:  boolean | undefined
  SAFE_FOR_XML?:        boolean | undefined
  SANITIZE_DOM?:        boolean | undefined
  SANITIZE_NAMED_PROPS?: boolean | undefined
  TRUSTED_TYPES_POLICY?: TrustedTypePolicy | null | undefined
  USE_PROFILES?: false | UseProfilesConfig | undefined
  WHOLE_DOCUMENT?: boolean | undefined
}

interface UseProfilesConfig {
  mathMl?:    boolean | undefined
  svg?:       boolean | undefined
  svgFilters?: boolean | undefined
  html?:      boolean | undefined
}
```

---

## [04]-[HOOKS]

```ts
// dompurify — hook types
type BasicHookName = "beforeSanitizeElements" | "afterSanitizeElements" | "uponSanitizeShadowNode"
type ElementHookName = "beforeSanitizeAttributes" | "afterSanitizeAttributes"
type DocumentFragmentHookName = "beforeSanitizeShadowDOM" | "afterSanitizeShadowDOM"
type UponSanitizeElementHookName = "uponSanitizeElement"
type UponSanitizeAttributeHookName = "uponSanitizeAttribute"
type HookName = BasicHookName | ElementHookName | DocumentFragmentHookName | UponSanitizeElementHookName | UponSanitizeAttributeHookName

type NodeHook = (this: DOMPurify, currentNode: Node, hookEvent: null, config: Config) => void
type ElementHook = (this: DOMPurify, currentNode: Element, hookEvent: null, config: Config) => void
type DocumentFragmentHook = (this: DOMPurify, currentNode: DocumentFragment, hookEvent: null, config: Config) => void
type UponSanitizeElementHook = (this: DOMPurify, currentNode: Node, hookEvent: UponSanitizeElementHookEvent, config: Config) => void
type UponSanitizeAttributeHook = (this: DOMPurify, currentNode: Element, hookEvent: UponSanitizeAttributeHookEvent, config: Config) => void

interface UponSanitizeElementHookEvent {
  tagName:     string
  allowedTags: Record<string, boolean>
}
interface UponSanitizeAttributeHookEvent {
  attrName:          string
  attrValue:         string
  keepAttr:          boolean
  allowedAttributes: Record<string, boolean>
  forceKeepAttr:     boolean | undefined
}
```

---

## [05]-[REMOVAL_TYPES]

```ts
// dompurify — removal and environment types
interface RemovedElement  { element: Node }
interface RemovedAttribute { attribute: Attr | null; from: Node }

type WindowLike = Pick<
  typeof globalThis,
  'DocumentFragment' | 'HTMLTemplateElement' | 'Node' | 'Element' |
  'NodeFilter' | 'NamedNodeMap' | 'HTMLFormElement' | 'DOMParser'
> & {
  document?: Document
  MozNamedAttrMap?: typeof window.NamedNodeMap
} & Pick<TrustedTypesWindow, 'trustedTypes'>
```

---

## [06]-[IMPLEMENTATION_LAW]

[ENTRY_TOPOLOGY]:
- `isomorphic-dompurify` has two compilation entries: the default `dist/index.d.ts` (node/universal,
  sets up a DOM environment via JSDOM or similar at import time) and `dist/browser.d.ts` (pure
  browser, identical API surface, no DOM emulation overhead). Package `exports` routes `browser`
  condition to the browser entry; Node.js bundlers and runtimes receive the universal entry.
- The default export is a pre-instantiated `DOMPurify` instance bound to the relevant environment.
  Creating additional instances via `DOMPurify(customWindow)` is valid but the default instance
  covers the standard sanitization path.

[SANITIZE_DISPATCH]:
- `sanitize` is overloaded on `cfg` flags: `RETURN_TRUSTED_TYPE: true` → `TrustedHTML`;
  `IN_PLACE: true` → mutates and returns the input `Node`; `RETURN_DOM: true` → `Node`;
  `RETURN_DOM_FRAGMENT: true` → `DocumentFragment`; default → `string`.
- `ALLOWED_TAGS` / `ALLOWED_ATTR` replace the defaults entirely; `ADD_TAGS` / `ADD_ATTR` extend
  them. `USE_PROFILES` overrides `ALLOWED_TAGS` — do not combine the two.
- `SAFE_FOR_TEMPLATES` strips template syntax (`{{ }}`, `${ }`, `<% %>`); use only when the output
  feeds a template engine and there is no alternative; not recommended for general production use.

[HOOK_LAW]:
- `addHook` dispatches on `entryPoint` literal to the correct hook function overload; each hook type
  receives a different `currentNode` type (`Node`, `Element`, `DocumentFragment`).
- `removeHook` (singular, pops the specific function if supplied or the top of the stack) vs
  `removeHooks` (plural, removes ALL hooks at the entry point) vs `removeAllHooks` (clears every
  entry point).

[CLEARWINDOW]:
- `clearWindow()` tears down the internally held DOM environment reference. Call only when the DOM
  emulation environment must be released (e.g., worker teardown). After calling `clearWindow`, the
  instance is in an undefined state until the module is re-imported or a new instance is constructed
  via `DOMPurify(newWindow)`.
