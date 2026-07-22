# [CORE_CARRIER]

W3C propagation crosses the interchange plane as ONE typed `traceparent`/`tracestate`/`baggage` value, total parse/print folds, `rasm.tenant` promotion, a closed transport table, and the Connect `-bin` typed-metadata lane. HTTP, Connect, NATS, MQTT v5, CloudEvents, and Kafka inject and extract through one codec. Malformed input folds to absence under the restart posture, while ordered, bounded folds keep output byte-stable. Module `core/src/interchange/carrier.ts` admits a transport as one dialect row, a baggage axis as one member key, and a typed family as one name row.

`Carrier` composes only the `value` floor's `TenantContext` and hands dialect frames to the runtime wave as data. Kafka, NATS, MQTT, and CloudEvents realize their rows, while `interchange/invoke` composes Connect. Frame values recover the dialect discriminant, so one mapped handler record owns dispatch.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                         | [PUBLIC]  |
| :-----: | :--------------- | :----------------------------------------------------------------------------- | :-------- |
|  [01]   | `CONTEXT_VALUE`  | the typed triple, its brands, the total parse/print folds, the span lift       | `Carrier` |
|  [02]   | `TENANT_BAGGAGE` | the `rasm.tenant` promotion and the scoped recovery decode                     | `Carrier` |
|  [03]   | `DIALECT_TABLE`  | the closed frame rows, inject/extract dispatch, the `-bin` typed-metadata lane | `Carrier` |

## [02]-[CONTEXT_VALUE]

- Owner: the context anchors — `Traceparent`, the parent-span identity class over the non-zero `TraceId`/`SpanId` hex brands with the sampled flag; `Carrier.State`, the ordered `tracestate` member rows under the vendor-key grammar; `Carrier.Member`, the baggage rows carrying percent-decoded values with their property tails; and `Carrier.Context`, the assembled triple whose every field is honestly absent-or-present — `parent` as `Option`, the two lists empty when unclaimed.
- Law: parse is total by the restart posture — a malformed `traceparent` folds to `Option.none` so the receiver restarts the trace instead of failing the call, a malformed `tracestate` or baggage member drops while its well-formed siblings survive, each baggage property tail proves the W3C property grammar or drops — a tail carrying a delimiter never re-frames a printed header — and context lists, serialized tracestate, and property tails truncate at their declared ceilings, so no downstream consumer meets an unbounded list; version `ff` refuses, version `00` admits only flags `00` or `01` and refuses extension fields, a future version parses its known four fields, and the all-zero trace or span id refuses through the brand pattern.
- Law: print is the inverse total fold — the parent renders the supported `00-<trace>-<span>-<flags>` spelling with the sampled bit as the whole flags byte, one `_stateRows` fold applies member grammar, row count, and aggregate text bounds before state members join in held order, and baggage members percent-encode through the boundary-owned URI fold with unencodable members and malformed or overlong properties dropped at print exactly as malformed input drops at parse, so print-then-parse is identity over every surviving supported-version value.
- Law: the span lift is structural — `Carrier.span({ traceId, spanId, sampled })` accepts any value carrying the three fields, so an Effect `Tracer.Span` and a recovered `ExternalSpan` lift identically with zero adapters. `Carrier.Current` scopes an admitted ingress context; `Carrier.current` overlays the live `Effect.currentSpan` parent while retaining carried tracestate and baggage, so core Connect egress and every runtime transport preserve foreign context without importing the runtime adapter.
- Growth: a new context list (a fourth W3C header) is one field on the triple with its parse/print row; a new parse bound is one `_CEILING` field.
- Boundary: which span is current is the rail's tracer; store-and-forward of a context beside a durable frame is the data wave's — this owner is the pure value and its folds.
- Packages: `effect` (`Array`, `Effect`, `Either`, `Option`, `Schema`, `String`, `pipe`); `../value/identity.ts` (`TenantContext`).

```typescript signature
import { decodeBinaryHeader, encodeBinaryHeader } from "@connectrpc/connect"
import type { DescMessage, Message, MessageShape } from "@bufbuild/protobuf"
import { Headers } from "@effect/platform"
import { Array, Context, Effect, Either, Option, Predicate, Record, Schema, String, pipe } from "effect"
import { TenantContext } from "../value/identity.ts"

const _CEILING = { state: 32, stateText: 512, baggage: 32, key: 256, value: 256, properties: 16, property: 256 } as const

const _TraceId = Schema.String.pipe(Schema.pattern(/^(?!0{32})[0-9a-f]{32}$/), Schema.brand("TraceId"))
const _SpanId = Schema.String.pipe(Schema.pattern(/^(?!0{16})[0-9a-f]{16}$/), Schema.brand("SpanId"))

class Traceparent extends Schema.Class<Traceparent>("Traceparent")({
  traceId: _TraceId,
  spanId: _SpanId,
  sampled: Schema.Boolean,
}) {}

const _PARENT = /^([0-9a-f]{2})-([0-9a-f]{32})-([0-9a-f]{16})-([0-9a-f]{2})(-.+)?$/
const _STATE_KEY = /^[a-z0-9][a-z0-9_\-*/]{0,255}$|^[a-z0-9][a-z0-9_\-*/]{0,240}@[a-z][a-z0-9_\-*/]{0,13}$/
const _STATE_VALUE = /^[\x20-\x2b\x2d-\x3c\x3e-\x7e]{0,255}[\x21-\x2b\x2d-\x3c\x3e-\x7e]$/
const _BAGGAGE_KEY = /^[!#$%&'*+\-.^_`|~0-9a-zA-Z]+$/
// W3C baggage property grammar: token key, optional "=" then baggage-octets — every printable byte
// except the dquote, comma, semicolon, and backslash delimiters that would re-frame the header.
const _PROPERTY = /^[!#$%&'*+\-.^_`|~0-9a-zA-Z]+(?:=[\x21\x23-\x2b\x2d-\x3a\x3c-\x5b\x5d-\x7e]*)?$/

const _decodedParent = Schema.decodeUnknownOption(Traceparent)

const _uri = (transform: (value: string) => string) => (value: string): Option.Option<string> => {
  // BOUNDARY ADAPTER: URI codecs throw on malformed escapes and lone surrogates; restart posture turns either into absence.
  try {
    return Option.some(transform(value))
  } catch {
    return Option.none()
  }
}

const _decodeUri = _uri(decodeURIComponent)
const _encodeUri = _uri(encodeURIComponent)

const _parent = (text: string): Option.Option<Traceparent> =>
  pipe(
    Option.fromNullable(_PARENT.exec(String.trim(text))),
    Option.filter(([, version, , , flags, extension]) =>
      version !== "ff" && (version !== "00" || (extension === undefined && (flags === "00" || flags === "01")))),
    Option.flatMap(([, , traceId, spanId, flags]) =>
      _decodedParent({ traceId, spanId, sampled: (Number.parseInt(flags ?? "00", 16) & 1) === 1 })),
  )

// A property tail re-prints verbatim, so grammar is admission: a malformed tail drops here exactly
// as a malformed member drops, and the one fold serves parse and print identically.
const _properties = (properties: ReadonlyArray<string>): ReadonlyArray<string> =>
  pipe(
    properties,
    Array.take(_CEILING.properties),
    Array.map(String.trim),
    Array.filter((property) => property.length <= _CEILING.property && _PROPERTY.test(property)),
  )

type _StateFold = { readonly rows: ReadonlyArray<Carrier.State>; readonly length: number }

const _stateMember = ({ key, value }: Carrier.State): boolean => _STATE_KEY.test(key) && _STATE_VALUE.test(value)

const _stateRows = (rows: ReadonlyArray<Carrier.State>): ReadonlyArray<Carrier.State> =>
  pipe(
    rows,
    Array.filter(_stateMember),
    Array.take(_CEILING.state),
    Array.reduce({ rows: [], length: 0 } satisfies _StateFold, (held, row) => {
      const length = held.length + (Array.isEmptyReadonlyArray(held.rows) ? 0 : 1) + row.key.length + row.value.length + 1
      return length <= _CEILING.stateText ? { rows: [...held.rows, row], length } : held
    }),
  ).rows

const _state = (text: string): ReadonlyArray<Carrier.State> =>
  pipe(
    String.split(text, ","),
    Array.filterMap((entry) =>
      pipe(
        Option.fromNullable(/^([^=]+)=(.*)$/.exec(String.trim(entry))),
        Option.map(([, key, value]) => ({ key: key ?? "", value: value ?? "" })),
      )),
    _stateRows,
  )

const _baggage = (text: string): ReadonlyArray<Carrier.Member> =>
  pipe(
    String.split(text, ","),
    Array.filterMap((entry) =>
      pipe(String.split(String.trim(entry), ";"), ([head, ...properties]) =>
        pipe(
          Option.fromNullable(/^([^=]+)=(.*)$/.exec(head ?? "")),
          Option.filter(([, key]) => key !== undefined && _BAGGAGE_KEY.test(key) && key.length <= _CEILING.key),
          Option.flatMap(([, key, value]) =>
            Option.map(
              Option.filter(_decodeUri(value ?? ""), (decoded) => decoded.length <= _CEILING.value),
              (decoded) => ({ key: key ?? "", value: decoded, properties: _properties(properties) }),
            )),
        ))),
    Array.take(_CEILING.baggage),
  )

const _printedParent = (parent: Traceparent): string =>
  `00-${parent.traceId}-${parent.spanId}-${parent.sampled ? "01" : "00"}`

const _printedState = (rows: ReadonlyArray<Carrier.State>): string =>
  Array.join(Array.map(_stateRows(rows), (row) => `${row.key}=${row.value}`), ",")

const _printedBaggage = (members: ReadonlyArray<Carrier.Member>): string =>
  Array.join(
    Array.filterMap(Array.take(members, _CEILING.baggage), (member) =>
      pipe(
        Option.some(member),
        Option.filter(({ key, value }) =>
          _BAGGAGE_KEY.test(key) && key.length <= _CEILING.key && value.length <= _CEILING.value),
        Option.flatMap(({ key, value, properties }) =>
          Option.map(_encodeUri(value), (encoded) => Array.join([`${key}=${encoded}`, ..._properties(properties)], ";"))),
      )),
    ",",
  )
```

## [03]-[TENANT_BAGGAGE]

- Owner: the tenancy member — `_TENANT`, the one `rasm.tenant` baggage key; `promote`, the upserting fold landing a `TenantContext` scope onto the baggage list; and `tenant`, the recovery decode reading the member back through `TenantContext.FromScope`.
- Law: promotion is inject-side law — every outbound hop that holds a tenancy promotes it before the dialect row prints, the member value is the branded `scope` spelling so the one partition key crosses brokers verbatim, and cost attribution survives every broker boundary because the member rides the same baggage header every dialect carries; the upsert replaces an incumbent `rasm.tenant` member, reserves its slot before truncation, and returns at most the declared baggage ceiling, so a relayed context never loses or duplicates tenancy.
- Law: recovery is a decode, never a split — the member value re-proves both key alphabets through `TenantContext.FromScope`, so a forged or truncated scope folds to `Option.none` and tenancy never enters domain flow as a bare string; resource-level tenant stamping stays `observe/convention`'s identity projection, and this member is the per-hop transport of the same value.
- Growth: a second promoted axis (a deployment ring, a request class) is one member-key constant with its promote/read pair beside this one.
- Boundary: which fiber holds the tenancy is `invoke`'s `Dial.Ambient` reference; security's claim alignment consumes the recovered `TenantContext` and owns everything past it.
- Packages: `effect` (`Array`, `Option`); `../value/identity.ts` (`TenantContext`).

```typescript signature
const _TENANT = "rasm.tenant"

const _decodedTenant = Schema.decodeUnknownOption(TenantContext.FromScope)

const _promote = (context: Carrier.Context, tenant: TenantContext): Carrier.Context => ({
  ...context,
  baggage: [
    ...Array.take(Array.filter(context.baggage, (member) => member.key !== _TENANT), _CEILING.baggage - 1),
    { key: _TENANT, value: tenant.scope, properties: [] },
  ],
})

const _tenant = (context: Carrier.Context): Option.Option<TenantContext> =>
  Option.flatMap(
    Array.findFirst(context.baggage, (member) => member.key === _TENANT),
    (member) => _decodedTenant(member.value),
  )
```

## [04]-[DIALECT_TABLE]

- Owner: the transport table — `_KEYS`, the three W3C header names every dialect shares; `Carrier.Frame`, the mapped per-dialect frame vocabulary (`fanout`/`http`/`nats` over string header records, `connect` over platform `Headers`, `kafka` over the byte-valued record-header map, `mqtt` over the v5 `userProperties` map, `cloudevents` over the extension-attribute record); `_dialects`, the mapped read/write handler record; the generic indexed `inject`/`extract` pair; and `_BIN` with the `bin` lane — the Connect `-bin` typed-metadata rows riding `encodeBinaryHeader`/`decodeBinaryHeader`.
- Law: one value, many frames — every row carries the same three keys, variation is exactly the frame's value codec (string on `http`/`connect`/`nats`, first-of-array on a repeated `mqtt` user property, UTF-8 bytes on `kafka`, a string-filtered `unknown` on `cloudevents`) and nothing else, so the table is data and a per-transport propagation fork has no authoring surface; the runtime clients realize each row against their live surfaces — `http` reads the normalized request-header record, `mqtt` prints into `opts.properties.userProperties` and reads the delivered packet's property block, `cloudevents` sets extension attributes the binary `Binding` emits as `ce-` headers, `kafka` lands record headers, `nats` lands `MsgHdrs` rows — and a raw header read past a row is the untyped-leak defect.
- Law: inject writes only what the context claims — an absent parent writes no `traceparent`, empty lists write nothing, so a hop never mints an empty header; extract composes the total folds, so a partially-malformed frame yields the surviving members and the context is always constructible.
- Law: the `-bin` lane is the TYPED metadata dialect beside the W3C rows, never a second context spelling — `_BIN` names the protobuf metadata families the invoke lanes attach (`rasm-tenant-bin` carrying `TenantContextWire`, `rasm-stamp-bin` carrying `HlcStampWire`), `bin.set` encodes a message through `encodeBinaryHeader(message, desc)` onto the `-bin`-suffixed name, and `bin.get` decodes through `decodeBinaryHeader(value, type)` with the codec's `DataLoss` throw folded to absence by the same restart posture the text folds hold; a new typed family is one `_BIN` row and its wire class, and `appendHeaders` remains the kit merge where a hand-assembled lane joins carrier headers with call headers.
- Law: the byte crossing is one marked seam — the `kafka` row's UTF-8 pair is the module's `TextEncoder`/`TextDecoder` kernel, constructed once and never escaping, so every dialect stays runtime-neutral under node, bun, and the browser.
- Growth: a new transport is one `Frame` field with its `_dialects` row; a new typed `-bin` family is one `_BIN` row.
- Boundary: live clients, broker connections, and binding serialization are the runtime wave's; the Connect lanes' per-call composition is `invoke#DIAL_AXIS`'s — its `_stamped` fold reads `Carrier.current`, promotes tenancy, prints the `connect` row, and leaves typed `_BIN` attachment to its interceptors.
- Packages: `@connectrpc/connect` (`encodeBinaryHeader`, `decodeBinaryHeader`); `@bufbuild/protobuf` (`DescMessage`, `Message`, `MessageShape`); `@effect/platform` (`Headers`); `effect` (`Array`, `Either`, `Option`, `Predicate`, `Record`).

```typescript signature
const _KEYS = ["traceparent", "tracestate", "baggage"] as const

const _utf8 = { read: new TextDecoder(), write: new TextEncoder() } as const // BOUNDARY ADAPTER: the kafka row's byte-to-text kernel; neither instance escapes

declare namespace Carrier {
  type Key = (typeof _KEYS)[number]
  type State = { readonly key: string; readonly value: string }
  type Member = { readonly key: string; readonly value: string; readonly properties: ReadonlyArray<string> }
  type Context = {
    readonly parent: Option.Option<Traceparent>
    readonly state: ReadonlyArray<State>
    readonly baggage: ReadonlyArray<Member>
  }
  type Frame = {
    readonly cloudevents: Record.ReadonlyRecord<string, unknown>
    readonly connect: Headers.Headers
    readonly fanout: Record.ReadonlyRecord<string, string>
    readonly http: Record.ReadonlyRecord<string, string>
    readonly kafka: Record.ReadonlyRecord<string, Uint8Array>
    readonly mqtt: Record.ReadonlyRecord<string, string | ReadonlyArray<string>>
    readonly nats: Record.ReadonlyRecord<string, string>
  }
  type Dialect = keyof Frame
  type Row<F> = {
    readonly read: (frame: F, key: Key) => Option.Option<string>
    readonly write: (frame: F, key: Key, value: string) => F
  }
  type Bin = keyof typeof _BIN
  type Shape = {
    readonly Current: typeof _Current
    readonly Traceparent: typeof Traceparent
    readonly bin: {
      readonly names: typeof _BIN
      readonly get: <Desc extends DescMessage>(headers: Headers.Headers, name: Bin, desc: Desc) => Option.Option<MessageShape<Desc>>
      readonly set: (headers: Headers.Headers, name: Bin, message: Message, desc: DescMessage) => Headers.Headers
    }
    readonly current: Effect.Effect<Context>
    readonly empty: Context
    readonly extract: <K extends Dialect>(dialect: K, frame: Frame[K]) => Context
    readonly inject: <K extends Dialect>(dialect: K, context: Context, frame: Frame[K]) => Frame[K]
    readonly parse: {
      readonly baggage: (text: string) => ReadonlyArray<Member>
      readonly traceparent: (text: string) => Option.Option<Traceparent>
      readonly tracestate: (text: string) => ReadonlyArray<State>
    }
    readonly print: {
      readonly baggage: (members: ReadonlyArray<Member>) => string
      readonly traceparent: (parent: Traceparent) => string
      readonly tracestate: (rows: ReadonlyArray<State>) => string
    }
    readonly promote: (context: Context, tenant: TenantContext) => Context
    readonly span: (span: { readonly traceId: string; readonly spanId: string; readonly sampled: boolean }) => Context
    readonly tenant: (context: Context) => Option.Option<TenantContext>
  }
  type _Rows<T extends { readonly [K in Dialect]: Row<Frame[K]> } = typeof _dialects> = T
}

const _BIN = {
  "rasm-stamp-bin": "HlcStampWire",
  "rasm-tenant-bin": "TenantContextWire",
} as const

const _dialects: { readonly [K in Carrier.Dialect]: Carrier.Row<Carrier.Frame[K]> } = {
  cloudevents: {
    read: (frame, key) => Option.filter(Option.fromNullable(frame[key]), Predicate.isString),
    write: (frame, key, value) => ({ ...frame, [key]: value }),
  },
  connect: {
    read: (frame, key) => Headers.get(frame, key),
    write: (frame, key, value) => Headers.set(frame, key, value),
  },
  fanout: {
    read: (frame, key) => Option.fromNullable(frame[key]),
    write: (frame, key, value) => ({ ...frame, [key]: value }),
  },
  http: {
    read: (frame, key) => Option.fromNullable(frame[key]),
    write: (frame, key, value) => ({ ...frame, [key]: value }),
  },
  kafka: {
    read: (frame, key) => Option.map(Option.fromNullable(frame[key]), (bytes) => _utf8.read.decode(bytes)),
    write: (frame, key, value) => ({ ...frame, [key]: _utf8.write.encode(value) }),
  },
  mqtt: {
    read: (frame, key) =>
      Option.flatMap(Option.fromNullable(frame[key]), (held) =>
        Predicate.isString(held) ? Option.some(held) : Array.head(held)), // a repeated v5 user property reads first-wins
    write: (frame, key, value) => ({ ...frame, [key]: value }),
  },
  nats: {
    read: (frame, key) => Option.fromNullable(frame[key]),
    write: (frame, key, value) => ({ ...frame, [key]: value }),
  },
}

const _inject = <K extends Carrier.Dialect>(dialect: K, context: Carrier.Context, frame: Carrier.Frame[K]): Carrier.Frame[K] => {
  const row = _dialects[dialect]
  const baggage = _printedBaggage(context.baggage)
  return pipe(
    Option.match(context.parent, {
      onNone: () => frame,
      onSome: (parent) => row.write(frame, "traceparent", _printedParent(parent)),
    }),
    (held) => (Array.isNonEmptyReadonlyArray(context.state) ? row.write(held, "tracestate", _printedState(context.state)) : held),
    (held) => (baggage.length > 0 ? row.write(held, "baggage", baggage) : held),
  )
}

const _extract = <K extends Carrier.Dialect>(dialect: K, frame: Carrier.Frame[K]): Carrier.Context => {
  const row = _dialects[dialect]
  return {
    parent: Option.flatMap(row.read(frame, "traceparent"), _parent),
    state: Option.match(row.read(frame, "tracestate"), { onNone: () => [], onSome: _state }),
    baggage: Option.match(row.read(frame, "baggage"), { onNone: () => [], onSome: _baggage }),
  }
}

const _empty: Carrier.Context = { parent: Option.none(), state: [], baggage: [] }

class _Current extends Context.Reference<_Current>()("core/Carrier/Current", {
  defaultValue: () => _empty,
}) {}

const _current: Effect.Effect<Carrier.Context> = Effect.flatMap(
  Effect.all([Effect.option(Effect.currentSpan), _Current]),
  ([span, carried]) => {
    const live = Option.match(span, { onNone: () => _empty, onSome: _span })
    return Effect.succeed({
      parent: Option.orElse(live.parent, () => carried.parent),
      state: carried.state,
      baggage: carried.baggage,
    })
  },
)

const _span = (span: { readonly traceId: string; readonly spanId: string; readonly sampled: boolean }): Carrier.Context => ({
  ..._empty,
  parent: _decodedParent({ traceId: span.traceId, spanId: span.spanId, sampled: span.sampled }),
})

const Carrier: Carrier.Shape = {
  Current: _Current,
  Traceparent,
  bin: {
    names: _BIN,
    get: (headers, name, desc) =>
      Option.flatMap(Headers.get(headers, name), (value) =>
        Either.getRight(Either.try(() => decodeBinaryHeader(value, desc)))), // the codec's DataLoss throw folds to absence by the restart posture
    set: (headers, name, message, desc) => Headers.set(headers, name, encodeBinaryHeader(message, desc)),
  },
  current: _current,
  empty: _empty,
  extract: _extract,
  inject: _inject,
  parse: { baggage: _baggage, traceparent: _parent, tracestate: _state },
  print: { baggage: _printedBaggage, traceparent: _printedParent, tracestate: _printedState },
  promote: _promote,
  span: _span,
  tenant: _tenant,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Carrier }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
