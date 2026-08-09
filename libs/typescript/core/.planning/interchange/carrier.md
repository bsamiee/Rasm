# [CORE_CARRIER]

W3C propagation crosses the interchange plane as ONE typed `traceparent`/`tracestate`/`baggage` value, total parse/print folds, `rasm.tenant` promotion, a closed transport table, and the Connect `-bin` typed-metadata lane. HTTP, Connect, NATS, MQTT v5, CloudEvents, and Kafka inject and extract through one codec. Malformed input folds to absence under the restart posture, while ordered, bounded folds keep output byte-stable. Module `core/src/interchange/carrier.ts` admits a transport as one dialect row, a baggage axis as one member key, and a typed family as one name row.

`Carrier` composes only the `value` floor's `Identity.Tenant` and hands dialect frames to the runtime wave as data. Kafka, NATS, MQTT, and CloudEvents realize their rows, while `interchange/invoke` composes Connect. Frame values recover the dialect discriminant, so one mapped handler record owns dispatch.

## [01]-[INDEX]

- [02]-[CONTEXT_VALUE]: the typed triple, its brands, the total parse/print folds, the span lift; `Carrier`.
- [03]-[TENANT_BAGGAGE]: the `rasm.tenant` promotion and the scoped recovery decode; `Carrier`.
- [04]-[DIALECT_TABLE]: the closed frame rows, inject/extract dispatch, the `-bin` typed-metadata lane; `Carrier`.

## [02]-[CONTEXT_VALUE]

- Owner: `Traceparent`, `Carrier.State`, `Carrier.Member`, and `Carrier.Context` own parent, tracestate, baggage, and optionality.
- Law: Malformed parents restart; invalid state or baggage members drop independently.
- Law: Baggage properties prove delimiter-safe W3C grammar before entering context.
- Law: Baggage admits 64 members, 4096 encoded bytes per member, and 8192 encoded bytes total.
- Law: Version `ff`, invalid version-zero flags, extensions on version zero, and all-zero identities refuse.
- Law: Parent print emits the supported version-zero spelling and sampled flag.
- Law: `_stateRows` enforces grammar, first-key-wins uniqueness, member count, and aggregate text bounds before joining.
- Law: Baggage print uses Effect `Encoding` before member and aggregate encoded-byte admission.
- Law: `Carrier.span` lifts structural span fields; `Carrier.Current` scopes ingress; `Carrier.current` overlays the live parent and preserves lists.
- Growth: a new context list (a fourth W3C header) is one field on the triple with its parse/print row; a new parse bound is one `_CEILING` field.
- Boundary: Tracer owns the live span; data owns persisted contexts; `Carrier` owns pure context values and folds.
- Packages: `effect` (`Array`, `Effect`, `Either`, `Encoding`, `Option`, `Schema`, `String`, `pipe`); `../value/identity.ts` (`Identity.Tenant`).

```typescript signature
import { decodeBinaryHeader, encodeBinaryHeader } from "@connectrpc/connect"
import type { Wire } from "./codec.ts" // type-only: the census family union, carrying no runtime edge to the codec owner
import { Headers, HttpTraceContext } from "@effect/platform"
import { Array, Context, Effect, Either, Encoding, Option, Predicate, Record, Schema, String, pipe } from "effect"
import { Convention } from "../observe/convention.ts"
import { Identity } from "../value/identity.ts"

const _CEILING = { state: 32, stateText: 512, baggage: 64, baggageMember: 4096, baggageText: 8192 } as const

const _TraceId = Schema.String.pipe(Schema.pattern(/^(?!0{32})[0-9a-f]{32}$/), Schema.brand("TraceId"))
const _SpanId = Schema.String.pipe(Schema.pattern(/^(?!0{16})[0-9a-f]{16}$/), Schema.brand("SpanId"))

class Traceparent extends Schema.Class<Traceparent>("Traceparent")({
  traceId: _TraceId,
  spanId: _SpanId,
  sampled: Schema.Boolean,
}) {}

const _PARENT = /^([0-9a-f]{2})-([0-9a-f]{32})-([0-9a-f]{16})-([0-9a-f]{2})(-.+)?$/
const _STATE_KEY = /^[a-z][a-z0-9_\-*/]{0,255}$|^[a-z0-9][a-z0-9_\-*/]{0,240}@[a-z][a-z0-9_\-*/]{0,13}$/
const _STATE_VALUE = /^[\x20-\x2b\x2d-\x3c\x3e-\x7e]{0,255}[\x21-\x2b\x2d-\x3c\x3e-\x7e]$/
const _BAGGAGE_KEY = /^[!#$%&'*+\-.^_`|~0-9a-zA-Z]+$/
const _BAGGAGE_VALUE = /^[\x21\x23-\x2b\x2d-\x3a\x3c-\x5b\x5d-\x7e]*$/
// W3C baggage property grammar: token key, optional "=" then baggage-octets — every printable byte
// except the dquote, comma, semicolon, and backslash delimiters that would re-frame the header.
const _PROPERTY = /^[!#$%&'*+\-.^_`|~0-9a-zA-Z]+(?:=[\x21\x23-\x2b\x2d-\x3a\x3c-\x5b\x5d-\x7e]*)?$/

const _decodedParent = Schema.decodeUnknownOption(Traceparent)

// Effect Encoding owns the URI exception rail; carrier's restart boundary folds its typed Either to absence.
const _decodeUri = (value: string): Option.Option<string> => Either.getRight(Encoding.decodeUriComponent(value))
const _encodeUri = (value: string): Option.Option<string> => Either.getRight(Encoding.encodeUriComponent(value))

const _parent = (text: string): Option.Option<Traceparent> =>
  pipe(
    Option.fromNullable(_PARENT.exec(String.trim(text))),
    Option.filter(([, version, , , flags, extension]) =>
      version !== "ff" && (version !== "00" || (extension === undefined && (flags === "00" || flags === "01")))),
    Option.flatMap(([, , traceId, spanId, flags]) =>
      _decodedParent({ traceId, spanId, sampled: (Number.parseInt(flags ?? "00", 16) & 1) === 1 })),
  )

// Property tails re-print verbatim, so grammar is admission: a malformed tail drops here exactly
// as a malformed member drops, and the one fold serves parse and print identically.
const _properties = (properties: ReadonlyArray<string>): ReadonlyArray<string> =>
  pipe(
    properties,
    Array.map(String.trim),
    Array.filter((property) => _PROPERTY.test(property)),
  )

type _StateFold = { readonly rows: ReadonlyArray<Carrier.State>; readonly length: number }

const _stateMember = ({ key, value }: Carrier.State): boolean => _STATE_KEY.test(key) && _STATE_VALUE.test(value)

const _stateRows = (rows: ReadonlyArray<Carrier.State>): ReadonlyArray<Carrier.State> =>
  pipe(
    rows,
    Array.filter(_stateMember),
    (admitted) => Array.dedupeWith(admitted, (left, right) => left.key === right.key),
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

const _utf8 = { read: new TextDecoder(), write: new TextEncoder() } as const
const _bytes = (text: string): number => _utf8.write.encode(text).byteLength

const _baggageMember = (entry: string): Option.Option<Carrier.Member> =>
  pipe(
    String.trim(entry),
    Option.liftPredicate((held) => _bytes(held) <= _CEILING.baggageMember),
    Option.flatMap((held) => pipe(String.split(held, ";"), ([head, ...properties]) =>
      pipe(
        Option.fromNullable(/^([^=]+)=(.*)$/.exec(head ?? "")),
        Option.filter(([, key, value]) =>
          key !== undefined && _BAGGAGE_KEY.test(key) && _BAGGAGE_VALUE.test(value ?? "")),
        Option.flatMap(([, key, value]) =>
          Option.map(_decodeUri(value ?? ""), (decoded) => ({
            key: key ?? "",
            value: decoded,
            properties: _properties(properties),
          }))),
      ))),
  )

const _baggage = (text: string): ReadonlyArray<Carrier.Member> =>
  _bytes(text) > _CEILING.baggageText
    ? []
    : pipe(String.split(text, ","), Array.filterMap(_baggageMember), Array.take(_CEILING.baggage))

const _printedParent = (parent: Traceparent): string =>
  `00-${parent.traceId}-${parent.spanId}-${parent.sampled ? "01" : "00"}`

const _printedState = (rows: ReadonlyArray<Carrier.State>): string =>
  Array.join(Array.map(_stateRows(rows), (row) => `${row.key}=${row.value}`), ",")

const _printedMember = ({ key, value, properties }: Carrier.Member): Option.Option<string> =>
  pipe(
    Option.some(key),
    Option.filter((held) => _BAGGAGE_KEY.test(held)),
    Option.flatMap(() => _encodeUri(value)),
    Option.map((encoded) => Array.join([`${key}=${encoded}`, ..._properties(properties)], ";")),
    Option.filter((member) => _bytes(member) <= _CEILING.baggageMember),
  )

type _BaggageFold = { readonly members: ReadonlyArray<string>; readonly bytes: number }

const _printedMembers = (
  members: ReadonlyArray<Carrier.Member>,
  reserved = 0,
  limit = _CEILING.baggage,
): _BaggageFold =>
  pipe(
    members,
    Array.filterMap(_printedMember),
    Array.take(limit),
    Array.reduce({ members: [], bytes: reserved } satisfies _BaggageFold, (held, member) => {
      const bytes = held.bytes + (held.bytes > reserved || reserved > 0 ? 1 : 0) + _bytes(member)
      return bytes <= _CEILING.baggageText ? { members: [...held.members, member], bytes } : held
    }),
  )

const _printedBaggage = (members: ReadonlyArray<Carrier.Member>): string =>
  Array.join(_printedMembers(members).members, ",")
```

## [03]-[TENANT_BAGGAGE]

- Owner: `_TENANT` names the one tenancy baggage member.
- Owner: `promote` upserts `Identity.Tenant`; `tenant` decodes it through `Identity.Tenant.FromScope`.
- Law: `promote` replaces `rasm.tenant` with `Identity.Tenant.scope`, reserves its slot, and enforces member and byte ceilings.
- Law: `Identity.Tenant.FromScope` re-proves both key alphabets; malformed scope folds to `Option.none`.
- Law: Resource stamping and propagation carry the same identity value.
- Growth: a second promoted axis (a deployment ring, a request class) is one member-key constant with its promote/read pair beside this one.
- Boundary: `Dial.Ambient` holds fiber tenancy; security consumes the recovered `Identity.Tenant`.
- Packages: `effect` (`Array`, `Option`); `../observe/convention.ts` (`Convention`); `../value/identity.ts` (`Identity.Tenant`).

```typescript signature
// Observe owns this baggage key outright, so one spelling site serves the branch and a rename there lands here
// rather than orphaning a twin no consumer can see diverge.
const _TENANT = Convention.rasm.tenant

const _decodedTenant = Schema.decodeUnknownOption(Identity.Tenant.FromScope)

const _promote = (context: Carrier.Context, tenant: Identity.Tenant): Carrier.Context =>
  pipe(
    { key: _TENANT, value: tenant.scope, properties: [] } satisfies Carrier.Member,
    (promoted) => ({
      promoted,
      reserved: Option.match(_printedMember(promoted), { onNone: () => _CEILING.baggageText, onSome: _bytes }),
    }),
    ({ promoted, reserved }) => ({
      promoted,
      admitted: _printedMembers(
        Array.filter(context.baggage, (member) => member.key !== _TENANT),
        reserved,
        _CEILING.baggage - 1,
      ),
    }),
    ({ promoted, admitted }) => ({
      ...context,
      baggage: [...Array.filterMap(admitted.members, _baggageMember), promoted],
    }),
  )

const _tenant = (context: Carrier.Context): Option.Option<Identity.Tenant> =>
  Option.flatMap(
    Array.findFirst(context.baggage, (member) => member.key === _TENANT),
    (member) => _decodedTenant(member.value),
  )
```

## [04]-[DIALECT_TABLE]

- Owner: `Carrier.Frame` maps each transport to its actual header value shape.
- Owner: `_dialects` and `Carrier.record` share immutable record-header read and write rows.
- Owner: `_BIN` carries typed metadata as base64 octets through Connect `-bin` headers.
- Law: `_dialects` maps one W3C context to string, repeated-string, byte, or extension-record frames; consumers use only that row.
- Law: `inject` omits absent fields and prints `tracestate` beneath a printed parent alone; `extract` drops malformed members independently and always returns `Carrier.Context`.
- Law: Missing or malformed W3C parents fall through to platform `b3` then `xb3` decoders; injection remains W3C-only.
- Law: Restart discards `tracestate`, so vendor rows survive the W3C parent alone and never a `b3`-recovered one.
- Law: B3 projection reads through the selected dialect row once, so every transport shares the same fallback.
- Law: `Row.read` accepts W3C and B3 keys; `Row.write` accepts only `Carrier.Injected` W3C keys.
- Law: `_BIN` maps a metadata name to its `Wire` family; `bin.set/get` carry that family's own octets and fold decode failure to absence.
- Law: Dialect rows decide selection, injection, and `degrade` alone — tenancy realizes through `promote`, and a context's lifetime ends with the `Carrier.Current` scope its caller opened.
- Law: Kafka and `Carrier.record` share one `TextEncoder` and `TextDecoder` kernel.
- Law: `record.read` selects the first repeated value and detaches bytes.
- Law: `record.write` emits a fresh string record without importing a host byte type.
- Growth: Add a transport as one `Carrier.Frame` field and dialect row; add typed metadata or fallback as one table row.
- Boundary: Runtime owns clients and bindings; invoke composes `Carrier.current`, tenant promotion, Connect injection, and `_BIN`.
- Packages: `@connectrpc/connect`, `@effect/platform`, and `effect`.

```typescript signature
const _KEYS = ["traceparent", "tracestate", "baggage"] as const

// The Zipkin ingress names, READ-only: the single-header form and the multi-header form the platform decoders parse.
const _B3 = ["b3", "x-b3-traceid", "x-b3-spanid", "x-b3-sampled", "x-b3-parentspanid"] as const

declare namespace Carrier {
  type Key = (typeof _KEYS)[number] | (typeof _B3)[number] // every header name a row READS
  type Injected = (typeof _KEYS)[number] // the write half is the W3C three alone, so "inject prints nothing else" is a parameter type rather than a discipline
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
  type RecordHeader = Uint8Array | string | ReadonlyArray<Uint8Array | string> | undefined
  type RecordHeaders = Record.ReadonlyRecord<string, RecordHeader>
  type Dialect = keyof Frame
  type Row<F> = {
    readonly read: (frame: F, key: Key) => Option.Option<string>
    readonly write: (frame: F, key: Injected, value: string) => F
    readonly degrade: string // what this frame shape forfeits carrying a W3C context, stated per row and readable by consumers
  }
  type Bin = keyof typeof _BIN
  type Shape = {
    readonly Current: typeof _Current
    readonly Traceparent: typeof Traceparent
    readonly bin: {
      readonly names: typeof _BIN
      readonly get: (headers: Headers.Headers, name: Bin) => Option.Option<Uint8Array>
      readonly set: (headers: Headers.Headers, name: Bin, octets: Uint8Array) => Headers.Headers
    }
    readonly current: Effect.Effect<Context>
    readonly dialects: typeof _dialects // consumers read a row's `degrade` before selecting the transport that carries their context
    readonly empty: Context
    readonly extract: <K extends Dialect>(dialect: K, frame: Frame[K]) => Context
    readonly inject: <K extends Dialect>(dialect: K, context: Context, frame: Frame[K]) => Frame[K]
    readonly record: {
      readonly read: (headers: RecordHeaders) => Frame["kafka"]
      readonly write: (frame: Frame["kafka"]) => Record.ReadonlyRecord<string, string>
    }
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
    readonly promote: (context: Context, tenant: Identity.Tenant) => Context
    readonly span: (span: { readonly traceId: string; readonly spanId: string; readonly sampled: boolean }) => Context
    readonly tenant: (context: Context) => Option.Option<Identity.Tenant>
  }
  type _Rows<T extends { readonly [K in Dialect]: Row<Frame[K]> } = typeof _dialects> = T
}

// Values name CENSUS families, so the guard binds this table to the codec roster: a renamed family fails at this
// declaration rather than attaching a typed header whose message class no producer mints.
const _BIN = {
  "rasm-stamp-bin": "HlcStampWire",
  "rasm-tenant-bin": "TenantContextWire",
} as const satisfies Record.ReadonlyRecord<`rasm-${string}-bin`, Wire.Family>

const _recordRow = <Value>(
  degrade: string,
  read: (value: Value) => Option.Option<string>,
  write: (value: string) => Value,
): Carrier.Row<Record.ReadonlyRecord<string, Value>> => ({
  degrade,
  read: (frame, key) => Option.flatMap(Option.fromNullable(frame[key]), read),
  write: (frame, key, value) => ({ ...frame, [key]: write(value) }),
})

const _records = {
  text: _recordRow("<repeated-values-unrepresentable>", (value: string) => Option.some(value), (value) => value),
  bytes: _recordRow(
    "<header-octets-read-as-utf8>",
    (value: Uint8Array) => Option.some(_utf8.read.decode(value)),
    (value) => _utf8.write.encode(value),
  ),
  read: (headers: Carrier.RecordHeaders): Carrier.Frame["kafka"] =>
    Record.fromEntries(
      Object.entries(headers).flatMap(([key, value]) =>
        pipe(
          value === undefined || Predicate.isString(value) || value instanceof Uint8Array ? value : value[0],
          (head) => head === undefined
            ? []
            : [[key, Predicate.isString(head) ? _utf8.write.encode(head) : new Uint8Array(head)] as const],
        )),
    ),
  write: (frame: Carrier.Frame["kafka"]): Record.ReadonlyRecord<string, string> =>
    Record.map(frame, (value) => _utf8.read.decode(value)),
} as const

const _dialects: { readonly [K in Carrier.Dialect]: Carrier.Row<Carrier.Frame[K]> } = {
  // Events carry extension attributes UNPREFIXED, so this row reads an event's own attribute record; binary-mode
  // bindings prefix those names into a header frame the http row already serves, and that prefix stays theirs.
  cloudevents: {
    degrade: "<attribute-record-only>",
    read: (frame, key) => Option.filter(Option.fromNullable(frame[key]), Predicate.isString),
    write: (frame, key, value) => ({ ...frame, [key]: value }),
  },
  connect: {
    degrade: "<repeats-flattened-before-arrival>", // `Headers` indexes one string per name, so a repeat never reaches this row
    read: (frame, key) => Headers.get(frame, key),
    write: (frame, key, value) => Headers.set(frame, key, value),
  },
  fanout: _records.text,
  http: _records.text,
  kafka: _records.bytes,
  mqtt: {
    degrade: "<repeated-user-property-first-wins>",
    read: (frame, key) =>
      Option.flatMap(Option.fromNullable(frame[key]), (held) =>
        Predicate.isString(held) ? Option.some(held) : Array.head(held)),
    write: (frame, key, value) => ({ ...frame, [key]: value }),
  },
  nats: _records.text,
}

const _record = {
  read: _records.read,
  write: _records.write,
} as const

const _inject = <K extends Carrier.Dialect>(dialect: K, context: Carrier.Context, frame: Carrier.Frame[K]): Carrier.Frame[K] =>
  pipe(
    { row: _dialects[dialect], baggage: _printedBaggage(context.baggage) },
    ({ row, baggage }) =>
      pipe(
        Option.match(context.parent, {
          onNone: () => frame, // no parent prints no state: vendor rows anchor to the parent whose trace minted them
          onSome: (parent) =>
            pipe(
              row.write(frame, "traceparent", _printedParent(parent)),
              (held) => Array.isNonEmptyReadonlyArray(context.state)
                ? row.write(held, "tracestate", _printedState(context.state))
                : held,
            ),
        }),
        (held) => baggage.length > 0 ? row.write(held, "baggage", baggage) : held,
      ),
  )

// The platform B3 decoders read a header frame, so the Zipkin names project off the row's own value codec into one
// `Headers` map — the dialect stays a value codec, and the two Zipkin grammars stay the platform's own parse.
const _zipkin = <K extends Carrier.Dialect>(dialect: K, frame: Carrier.Frame[K]): Headers.Headers =>
  Headers.fromInput(
    Record.fromEntries(
      Array.filterMap(_B3, (key) => Option.map(_dialects[dialect].read(frame, key), (value) => [key, value] as const)),
    ),
  )

const _extract = <K extends Carrier.Dialect>(dialect: K, frame: Carrier.Frame[K]): Carrier.Context =>
  pipe(
    { row: _dialects[dialect], w3c: Option.flatMap(_dialects[dialect].read(frame, "traceparent"), _parent) },
    ({ row, w3c }) => ({
      // one projection feeds both Zipkin grammars: the fallback is already lazy, so re-projecting per decoder would
      // rebuild the same header map twice on every W3C-absent hop
      parent: Option.orElse(w3c, () =>
        pipe(_zipkin(dialect, frame), (zipkin) =>
          Option.flatMap(
            Option.orElse(HttpTraceContext.b3(zipkin), () => HttpTraceContext.xb3(zipkin)),
            (external) => _span(external).parent, // the recovered external span lifts through the one structural span fold
          ))),
      // Restart drops what it cannot anchor: vendor rows key to the W3C parent that carried them, so a refused parent
      // and a b3-recovered one both admit an empty list rather than lineage naming a trace this hop no longer continues.
      state: Option.isSome(w3c)
        ? Option.match(row.read(frame, "tracestate"), { onNone: () => [], onSome: _state })
        : [],
      // Baggage travels on its own W3C specification and survives a restarted parent, so its admission stays unconditional.
      baggage: Option.match(row.read(frame, "baggage"), { onNone: () => [], onSome: _baggage }),
    }),
  )

const _empty: Carrier.Context = { parent: Option.none(), state: [], baggage: [] }

class _Current extends Context.Reference<_Current>()("core/Carrier/Current", {
  defaultValue: () => _empty,
}) {}

const _current: Effect.Effect<Carrier.Context> = Effect.map(
  Effect.all({ carried: _Current, span: Effect.option(Effect.currentSpan) }),
  ({ carried, span }) =>
    pipe(Option.match(span, { onNone: () => _empty, onSome: _span }), (live) => ({
      parent: Option.orElse(live.parent, () => carried.parent),
      state: carried.state,
      baggage: carried.baggage,
    })),
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
    get: (headers, name) =>
      Option.flatMap(Headers.get(headers, name), (value) =>
        Either.getRight(Either.try(() => decodeBinaryHeader(value)))), // the codec's DataLoss throw folds to absence by the restart posture
    set: (headers, name, octets) => Headers.set(headers, name, encodeBinaryHeader(octets)),
  },
  current: _current,
  dialects: _dialects,
  empty: _empty,
  extract: _extract,
  inject: _inject,
  record: _record,
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
