# [API_CATALOGUE] sax

`sax` is an event-driven streaming XML/HTML tokenizer. The `parser(strict, opt)` factory and the `SAXParser` class consume markup pushed through `write` and emit tokens through assignable `on*` handlers (`ontext`, `onopentag`, `onclosetag`, `onattribute`, `oncdata`, `oncomment`, `onprocessinginstruction`, `onerror`, `onend`, and the rest). The `createStream(strict, opt)` factory and `SAXStream` class wrap the same parser as a Node.js `Duplex` stream that re-emits each token as a typed `'event'` via `on(event, listener)`. The `xmlns` option promotes open-tag payloads from `Tag` to namespace-aware `QualifiedTag`; `strict` toggles XML strictness versus lenient HTML parsing. Types ship as `@types/sax`.

---

## [1]-[PACKAGE_SURFACE]

The push-parser surface (`parser`/`SAXParser`) and the stream surface (`createStream`/`SAXStream`) consume the same `SAXOptions` and emit the same token shapes.

```ts
// sax — @types/sax
export const EVENTS: string[]
export function parser(strict?: boolean, opt?: SAXOptions): SAXParser
export function createStream(strict?: boolean, opt?: SAXOptions): SAXStream
```

---

## [2]-[OPTIONS_AND_TOKENS]

`SAXOptions` toggles trimming, normalization, casing, namespace awareness, script handling, and position tracking. `Tag` is the default open-tag payload; with `xmlns: true`, open-tag payloads are `QualifiedTag` carrying resolved namespaces and per-attribute qualification.

```ts
// sax — token types
interface SAXOptions {
  trim?: boolean | undefined
  normalize?: boolean | undefined
  lowercase?: boolean | undefined
  xmlns?: boolean | undefined
  noscript?: boolean | undefined
  position?: boolean | undefined
}

interface QualifiedName { name: string; prefix: string; local: string; uri: string }
interface QualifiedAttribute extends QualifiedName { value: string }
interface BaseTag { name: string; isSelfClosing: boolean }
interface Tag extends BaseTag { attributes: { [key: string]: string } }
interface QualifiedTag extends QualifiedName, BaseTag {
  ns: { [key: string]: string }
  attributes: { [key: string]: QualifiedAttribute }
}
```

---

## [3]-[SAX_PARSER]

`SAXParser` is the push tokenizer. Feed markup with `write`, finish with `end`/`close`, and read parse state from its public members. Tokens arrive on the assignable `on*` callbacks rather than an event emitter.

```ts
// sax — SAXParser
class SAXParser {
  constructor(strict?: boolean, opt?: SAXOptions)

  // Methods
  write(s: string): SAXParser
  end(): void
  close(): SAXParser
  resume(): SAXParser
  flush(): void

  // Members
  line: number; column: number; position: number; startTagPosition: number
  error: Error; closed: boolean; strict: boolean
  opt: SAXOptions; tag: Tag
  ENTITIES: { [key: string]: string }

  // Token handlers (assignable)
  onerror(e: Error): void
  ontext(t: string): void
  ondoctype(doctype: string): void
  onprocessinginstruction(node: { name: string; body: string }): void
  onsgmldeclaration(sgmlDecl: string): void
  onopentagstart(tag: Tag | QualifiedTag): void
  onopentag(tag: Tag | QualifiedTag): void
  onclosetag(tagName: string): void
  onattribute(attr: { name: string; value: string }): void
  oncomment(comment: string): void
  onopencdata(): void
  oncdata(cdata: string): void
  onclosecdata(): void
  onopennamespace(ns: { prefix: string; uri: string }): void
  onclosenamespace(ns: { prefix: string; uri: string }): void
  onscript(script: string): void
  onready(): void
  onend(): void
}
```

---

## [4]-[SAX_STREAM]

`SAXStream` extends `stream.Duplex`, wrapping a `SAXParser` (`_parser`) and re-emitting each token as a typed event. The `on` overloads bind listeners to the exact token payload per event name.

```ts
// sax — SAXStream
class SAXStream extends stream.Duplex {
  constructor(strict?: boolean, opt?: SAXOptions)
  _parser: SAXParser

  on(event: "text" | "doctype" | "cdata" | "comment" | "script", listener: (this: this, value: string) => void): this
  on(event: "closetag", listener: (this: this, tagName: string) => void): this
  on(event: "opentag" | "opentagstart", listener: (this: this, tag: Tag | QualifiedTag) => void): this
  on(event: "attribute", listener: (this: this, attr: { name: string; value: string }) => void): this
  on(event: "processinginstruction", listener: (this: this, node: { name: string; body: string }) => void): this
  on(event: "sgmldeclaration", listener: (this: this, sgmlDecl: string) => void): this
  on(event: "opennamespace" | "closenamespace", listener: (this: this, ns: { prefix: string; uri: string }) => void): this
  on(event: "opencdata" | "closecdata" | "end" | "ready" | "close" | "readable" | "drain" | "finish", listener: (this: this) => void): this
  on(event: "data", listener: (this: this, chunk: any) => void): this
  on(event: "error", listener: (this: this, err: Error) => void): this
  on(event: "pipe" | "unpipe", listener: (this: this, src: stream.Readable) => void): this
  on(event: string | symbol, listener: (this: this, ...args: any[]) => void): this
}
```

---

## [5]-[IMPLEMENTATION_LAW]

[PARSER_MODE]:
- `SAXParser` is push-driven: `write` chunks in, `on*` callbacks fire synchronously per token, `end`/`close` finalize. It does not extend an event emitter; assign callbacks directly to the instance.
- `SAXStream` is the `Duplex` adapter over the same parser; bind via `on(event, listener)` and pipe Node streams through it. `_parser` exposes the underlying `SAXParser` for state inspection.

[STRICT_AND_NAMESPACE]:
- `strict` selects XML strictness (`true`) versus lenient HTML-tolerant parsing (`false`).
- `xmlns: true` resolves namespaces: `onopentag`/`opentag` payloads become `QualifiedTag` with `ns` and `QualifiedAttribute` values, and `opennamespace`/`closenamespace` tokens fire around scoped prefixes.

[STATE]:
- Position members (`line`, `column`, `position`, `startTagPosition`) populate when `position: true`. `error` holds the last parse error; `closed` reports finalization; `ENTITIES` is the mutable entity table for custom entity definitions.
- `EVENTS` enumerates every emitted event name shared by the parser callbacks and the stream events.
