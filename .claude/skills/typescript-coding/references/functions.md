# [FUNCTIONS]

Covers functions as values, from pipelines and dual signatures to partial application, keyed tables, and `Effect.fn`.

## [01]-[PIPELINES]

Generic data-last steps (`Array.map`, `Option.map`) infer their type parameter from the value before them, so a pipeline through one starts from a typed value in `pipe`, and `flow` composes steps with a concrete first input:

```ts
const slug = flow(String.toLowerCase, String.replaceAll(/[^a-z0-9]+/g, ' '), String.trim, String.replaceAll(' ', '-'));

const slugs = (items: ReadonlyArray<Item>) => pipe(items, Array.filter((item) => item.published), Array.map((item) => slug(item.title)), Array.dedupe);
```

`slugs` annotates its parameter alone, and each later step infers from it:
- Package functions enter as steps by reference (`Array.dedupe`, `String.trim`), a lambda reads a field or combines arguments
- Declarations whose body forwards one package call are that call at each use

## [02]-[DUAL_SIGNATURES]

`Function.dual` builds one function dispatching on argument count: a full call runs its body data-first, and a call missing its subject returns a data-last step:

```ts
const share = Function.dual<(part: BigDecimal.BigDecimal) => (line: Line) => BigDecimal.BigDecimal, (line: Line, part: BigDecimal.BigDecimal) => BigDecimal.BigDecimal>(2, (line, part) => BigDecimal.multiply(line.total, part));
```

`share(line, part)` and `pipe(line, share(part))` run one body:
- Type arguments state both signatures once, and body parameters take their types from the data-first one
- Arity counts data-first parameters, a guard (`(args) => Schema.is(Line)(args[0])`) replaces it for an optional trailing parameter
- Callbacks passing an element index as their second argument take the applied form, `Array.map(lines, share(part))`
- Exported functions consumers call both directly and in a pipeline are duals, a module-private step stays data-last

## [03]-[PARTIAL_APPLICATION]

Partial application runs each stage of a curried function as its arguments arrive, so work reading fixed arguments alone sits in an outer stage and per-value work in the returned function:

```ts
const converter = (rates: HashMap.HashMap<Currency, BigDecimal.BigDecimal>) => (to: Currency) => Option.map(HashMap.get(rates, to), (rate) => BigDecimal.multiply(rate));
```

`converter(rates)(to)` looks the rate up once and answers `Option.none()` for an unknown currency before any amount arrives, and each amount reaches `BigDecimal.multiply(rate)`.
- Package data-last forms are applied functions already: `BigDecimal.multiply(rate)`, `Number.between({ minimum, maximum })`, `Array.map(f)`
- Failures fixed arguments decide answer at their application as `Option` or `Result`, and the returned function stays total
- Closures capture immutable values, a `Ref` or service a function reads is yielded inside its effect

## [04]-[KEYED_TABLES]

Tables whose rows are package modules select a whole capability by key, and every member the modules share reads through one lookup:

```ts
const encodings = { hex: Hex, base64: Base64, base64url: Base64Url } as const;

const Encoding = Schema.Literals(Struct.keys(encodings));
const decoded = (encoding: typeof Encoding.Type, text: string) => encodings[encoding].decode(text);
```

`encodings[encoding]` types as a union of its rows, so `decode` keeps each module's signature and answers `Result<Uint8Array, EncodingError>`. A new encoding is one row, and `encode` reads through the same row.
- Rows hold a package module or function where behavior differs between cases, and data one shared function reads where a value differs

## [05]-[EFFECTFUL_FUNCTIONS]

`Effect.fn('<name>')` takes a body returning an effect or a generator, and runs it with every trailing step of each call inside one span named for it. A retry or timeout among trailing steps runs every attempt inside the span, and `Effect.annotateCurrentSpan` there tags it:

```ts
const order = Effect.fn('order')(
    (id: OrderId) => HttpClient.get(`/orders/${id}`).pipe(Effect.flatMap(HttpClientResponse.schemaBodyJson(Order))),
    (effect, id) => Effect.catchTag(effect, 'SchemaError', (cause) => OrderError.make({ reason: 'rejected', subject: id, cause })),
    (effect, id) => Effect.andThen(Effect.annotateCurrentSpan({ id }), effect),
);
```

Responses failing `Order` decoding become an `OrderError` with their id as subject, `HttpClientError` passes on unchanged, and the span records each id. `Order` is a response schema and `OrderId` its brand.
- `Effect.fnUntraced` declares a generator function called inside a traced one, recording no stack frame and no span
- Span names follow the binding, `<Class>.<member>` for a class field and `<Service>.<key>` for a service member
- Attributes every call shares sit in `Effect.fn('<name>', { attributes })`
