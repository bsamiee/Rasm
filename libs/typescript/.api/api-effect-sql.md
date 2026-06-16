# [API_CATALOGUE] @effect/sql

Decompile-verified from the installed distribution at `node_modules/@effect/sql/dist/dts`
(version `0.51.1`). This is the dialect-agnostic SQL toolkit: the `SqlClient` query service and
template DSL (`Statement`), the connection acquirer contract (`SqlConnection`), the typed error
rail (`SqlError`), the schema-bridged query/resolver layers (`SqlSchema`, `SqlResolver`), the
`Model` schema-class family with its CRUD repository/data-loader builders, the generic `Migrator`,
and the durable `SqlEventJournal` / `SqlEventLogServer` / `SqlPersistedQueue` surfaces. Dialect
packages (`@effect/sql-pg`, `@effect/sql-mssql`, `@effect/sql-sqlite-*`, `@effect/sql-drizzle`)
specialize `SqlClient` and supply a `Statement.Compiler`; portable query logic depends on the
abstract symbols catalogued here. Peer surface: `effect ^3.21`, `@effect/experimental ^0.60`,
`@effect/platform ^0.96`.

The package root `.` re-exports twelve namespaces only — every symbol is reached through one of them.

```ts
// @effect/sql
export * as Migrator from "@effect/sql/Migrator"
export * as Model from "@effect/sql/Model"
export * as SqlClient from "@effect/sql/SqlClient"
export * as SqlConnection from "@effect/sql/SqlConnection"
export * as SqlError from "@effect/sql/SqlError"
export * as SqlEventJournal from "@effect/sql/SqlEventJournal"
export * as SqlEventLogServer from "@effect/sql/SqlEventLogServer"
export * as SqlPersistedQueue from "@effect/sql/SqlPersistedQueue"
export * as SqlResolver from "@effect/sql/SqlResolver"
export * as SqlSchema from "@effect/sql/SqlSchema"
export * as SqlStream from "@effect/sql/SqlStream"
export * as Statement from "@effect/sql/Statement"
```

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@effect/sql`
- package: `@effect/sql`
- entry: `@effect/sql` (namespace re-export) plus the twelve sub-paths `@effect/sql/SqlClient`, `@effect/sql/Statement`, `@effect/sql/SqlConnection`, `@effect/sql/SqlError`, `@effect/sql/SqlSchema`, `@effect/sql/SqlResolver`, `@effect/sql/Model`, `@effect/sql/Migrator`, `@effect/sql/SqlStream`, `@effect/sql/SqlEventJournal`, `@effect/sql/SqlEventLogServer`, `@effect/sql/SqlPersistedQueue`
- asset: `SqlClient` service + tag + `make`, the `Statement` template DSL / `Fragment` algebra / dialect `Compiler`, `Connection`/`Acquirer` driver contract, `SqlError`/`ResultLengthMismatch` rail, `SqlSchema` and `SqlResolver` schema-bridged query layers, the `Model` schema-class family with `makeRepository`/`makeDataLoaders`, the generic `Migrator` runner + loaders, the pause/resume stream adapter, and the durable journal/queue/event-log subsystems
- rail: persistence / sql-core

## [2]-[PUBLIC_TYPES]

### @effect/sql/SqlClient — query service family

[PUBLIC_TYPE_SCOPE]: type id, model, tag, options
- rail: persistence
- entry: `@effect/sql/SqlClient`

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]      | [RAIL]                                                         |
| :-----: | :---------------------- | :----------------- | :------------------------------------------------------------- |
|   [1]   | `TypeId`                | const + type alias | branded service id                                             |
|   [2]   | `SqlClient`             | interface          | extends `Statement.Constructor`; the query/transaction surface |
|   [3]   | `SqlClient` (tag)       | `Context.Tag`      | `Context.Tag<SqlClient, SqlClient>` — service accessor         |
|   [4]   | `SqlClient.MakeOptions` | interface          | acquirer + compiler + dialect SQL knobs for `make`             |
|   [5]   | `make`                  | constructor        | `MakeOptions` → `Effect<SqlClient, never, Reactivity>`         |
|   [6]   | `makeWithTransaction`   | constructor        | builds the `withTransaction` combinator from a transaction tag |
|   [7]   | `TransactionConnection` | interface + tag    | `Tag<…, readonly [conn: Connection, depth: number]>`           |
|   [8]   | `SafeIntegers`          | reference class    | `Context.Reference<boolean>` — 64-bit-as-bigint toggle         |

`SqlClient` IS the callable template tag: it extends `Statement.Constructor`, so the service value
is invoked as a tagged template (`` sql`SELECT …` ``) and also carries the `unsafe`/`insert`/
`update`/`in`/`and`/`or`/`csv`/`join`/`onDialect` fragment builders documented under `Statement`.

```ts contract
import * as Statement from "@effect/sql/Statement"
import type { Connection } from "@effect/sql/SqlConnection"
import { SqlError } from "@effect/sql/SqlError"
import * as Reactivity from "@effect/experimental/Reactivity"
import * as Context from "effect/Context"
import * as Effect from "effect/Effect"
import type { ReadonlyMailbox } from "effect/Mailbox"
import type { ReadonlyRecord } from "effect/Record"
import * as Scope from "effect/Scope"
import * as Stream from "effect/Stream"

const TypeId: unique symbol
type TypeId = typeof TypeId

interface SqlClient extends Statement.Constructor {
  readonly [TypeId]: TypeId
  /** Copy of the client for safeql etc. */
  readonly safe: this
  /** Copy of the client without transformations. */
  readonly withoutTransforms: () => this
  readonly reserve: Effect.Effect<Connection, SqlError, Scope.Scope>
  /** Ensure all sql queries inside `self` run in a transaction. */
  readonly withTransaction: <R, E, A>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, E | SqlError, R>
  /** Reactive query via the @effect/experimental Reactivity service. */
  readonly reactive: <A, E, R>(
    keys: ReadonlyArray<unknown> | ReadonlyRecord<string, ReadonlyArray<unknown>>,
    effect: Effect.Effect<A, E, R>
  ) => Stream.Stream<A, E, R>
  readonly reactiveMailbox: <A, E, R>(
    keys: ReadonlyArray<unknown> | ReadonlyRecord<string, ReadonlyArray<unknown>>,
    effect: Effect.Effect<A, E, R>
  ) => Effect.Effect<ReadonlyMailbox<A, E>, never, R | Scope.Scope>
}

const SqlClient: Context.Tag<SqlClient, SqlClient>

declare namespace SqlClient {
  interface MakeOptions {
    readonly acquirer: Connection.Acquirer
    readonly compiler: Statement.Compiler
    readonly transactionAcquirer?: Connection.Acquirer
    readonly spanAttributes: ReadonlyArray<readonly [string, unknown]>
    readonly beginTransaction?: string | undefined
    readonly rollback?: string | undefined
    readonly commit?: string | undefined
    readonly savepoint?: ((name: string) => string) | undefined
    readonly rollbackSavepoint?: ((name: string) => string) | undefined
    readonly transformRows?: (<A extends object>(row: ReadonlyArray<A>) => ReadonlyArray<A>) | undefined
    readonly reactiveMailbox?: <A, E, R>(
      keys: ReadonlyArray<unknown> | ReadonlyRecord<string, ReadonlyArray<unknown>>,
      effect: Effect.Effect<A, E, R>
    ) => Effect.Effect<ReadonlyMailbox<A, E>, never, R | Scope.Scope>
  }
}

const make: (options: SqlClient.MakeOptions) => Effect.Effect<SqlClient, never, Reactivity.Reactivity>

const makeWithTransaction: <I, S>(options: {
  readonly transactionTag: Context.Tag<I, readonly [conn: S, counter: number]>
  readonly spanAttributes: ReadonlyArray<readonly [string, unknown]>
  readonly acquireConnection: Effect.Effect<readonly [Scope.CloseableScope | undefined, S], SqlError>
  readonly begin: (conn: NoInfer<S>) => Effect.Effect<void, SqlError>
  readonly savepoint: (conn: NoInfer<S>, id: number) => Effect.Effect<void, SqlError>
  readonly commit: (conn: NoInfer<S>) => Effect.Effect<void, SqlError>
  readonly rollback: (conn: NoInfer<S>) => Effect.Effect<void, SqlError>
  readonly rollbackSavepoint: (conn: NoInfer<S>, id: number) => Effect.Effect<void, SqlError>
}) => <R, E, A>(effect: Effect.Effect<A, E, R>) => Effect.Effect<A, E | SqlError, R>

interface TransactionConnection { readonly _: unique symbol }
const TransactionConnection: Context.Tag<TransactionConnection, readonly [conn: Connection, depth: number]>

// Context reference (default false). Set true to receive 64-bit columns as bigint.
class SafeIntegers extends Context.Reference<SafeIntegers>()("@effect/sql/SqlClient/SafeIntegers", boolean) {}
```

### @effect/sql/Statement — template DSL and fragment algebra

[PUBLIC_TYPE_SCOPE]: constructor, statement, fragment algebra
- rail: persistence
- entry: `@effect/sql/Statement`

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [RAIL]                                                                                                                                                       |
| :-----: | :------------------------- | :------------ | :----------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `Constructor`              | interface     | the callable template builder (what `SqlClient` extends)                                                                                                     |
|   [2]   | `Statement<A>`             | interface     | a `Fragment` that is also `Effect<ReadonlyArray<A>, SqlError>`                                                                                               |
|   [3]   | `Fragment`                 | interface     | `{ segments: ReadonlyArray<Segment> }` — the compile unit                                                                                                    |
|   [4]   | `Segment`                  | type union    | `Literal \| Identifier \| Parameter \| ArrayHelper \| RecordInsertHelper \| RecordUpdateHelper \| RecordUpdateHelperSingle \| Custom` (no `Fragment` member) |
|   [5]   | `Dialect`                  | string union  | `"sqlite" \| "pg" \| "mysql" \| "mssql" \| "clickhouse"`                                                                                                     |
|   [6]   | `Literal`                  | interface     | `_tag: "Literal"`; `{ value; params? }` raw-SQL leaf segment                                                                                                 |
|   [7]   | `Identifier` / `Parameter` | interface     | tagged leaf segments                                                                                                                                         |
|   [8]   | `ArrayHelper`              | interface     | `in (…)` value list                                                                                                                                          |
|   [9]   | `RecordInsertHelper`       | interface     | `insert(…)` builder with `.returning`                                                                                                                        |
|  [10]   | `RecordUpdateHelper`       | interface     | `updateValues(…, alias)` builder with `.returning`                                                                                                           |
|  [11]   | `RecordUpdateHelperSingle` | interface     | `update(…)` single-row builder with `.returning`                                                                                                             |
|  [12]   | `Custom<T,A,B,C>`          | interface     | `_tag: "Custom"`; `{ kind: T; i0; i1; i2 }` dialect extension point                                                                                          |
|  [13]   | `Helper`                   | type union    | `ArrayHelper \| RecordInsertHelper \| RecordUpdateHelper \| RecordUpdateHelperSingle \| Identifier \| Custom`                                                |
|  [14]   | `PrimitiveKind`            | string union  | runtime parameter classification                                                                                                                             |
|  [15]   | `Compiler`                 | interface     | `{ dialect; compile; withoutTransform }`                                                                                                                     |
|  [16]   | `Transformer`              | type alias    | `Statement.Transformer` fiber-ref transform hook                                                                                                             |

[PUBLIC_TYPE_SCOPE]: free functions
- rail: persistence

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [RAIL]                                                                             |
| :-----: | :------------------------ | :------------ | :--------------------------------------------------------------------------------- |
|   [1]   | `make`                    | constructor   | builds a `Constructor` from acquirer + compiler                                    |
|   [2]   | `custom`                  | constructor   | builds a custom-fragment factory for a `Custom` kind                               |
|   [3]   | `unsafeFragment`          | constructor   | raw sql + params → `Fragment`                                                      |
|   [4]   | `and` / `or`              | combinator    | `ReadonlyArray<string \| Fragment>` → `Fragment`                                   |
|   [5]   | `csv`                     | combinator    | comma-joined values (optional prefix) → `Fragment`                                 |
|   [6]   | `join`                    | combinator    | literal-joined fragment combinator factory                                         |
|   [7]   | `isFragment`              | guard         | `(u) => u is Fragment`                                                             |
|   [8]   | `isCustom`                | guard         | `(kind) => (u) => u is Custom` — kind-narrowing custom guard                       |
|   [9]   | `makeCompiler`            | constructor   | assemble a dialect `Compiler` from emit callbacks                                  |
|  [10]   | `makeCompilerSqlite`      | constructor   | prebuilt SQLite `Compiler`                                                         |
|  [11]   | `withTransformer`         | combinator    | run an effect with a statement transformer (dual)                                  |
|  [12]   | `withTransformerDisabled` | combinator    | run an effect with transforms suppressed                                           |
|  [13]   | `setTransformer`          | layer ctor    | install a `Transformer` as a `Layer`                                               |
|  [14]   | `currentTransformer`      | fiber ref     | `FiberRef<Option<Transformer>>`                                                    |
|  [15]   | `defaultEscape`           | function      | `(c) => (str) => string` identifier escaper builder                                |
|  [16]   | `defaultTransforms`       | function      | `(transformer, nested?)` → `{ value; object; array }` row/identifier transform set |
|  [17]   | `primitiveKind`           | function      | `(value) => PrimitiveKind`                                                         |

```ts contract
import type { Connection } from "@effect/sql/SqlConnection"
import { SqlError } from "@effect/sql/SqlError"
import type { Effect } from "effect/Effect"
import type * as FiberRef from "effect/FiberRef"
import type * as FiberRefs from "effect/FiberRefs"
import type * as Layer from "effect/Layer"
import type * as Option from "effect/Option"
import type { Pipeable } from "effect/Pipeable"
import * as Stream from "effect/Stream"
import type * as Tracer from "effect/Tracer"

type Dialect = "sqlite" | "pg" | "mysql" | "mssql" | "clickhouse"
type PrimitiveKind = "string" | "number" | "bigint" | "boolean" | "Date" | "null" | "Int8Array" | "Uint8Array" | "object"

declare const FragmentId: unique symbol
type FragmentId = typeof FragmentId

interface Fragment {
  readonly [FragmentId]: (_: never) => FragmentId
  readonly segments: ReadonlyArray<Segment>
}

// A Statement IS the query effect: yielding it runs the query and decodes rows.
interface Statement<A> extends Fragment, Effect<ReadonlyArray<A>, SqlError>, Pipeable {
  readonly raw: Effect<unknown, SqlError>
  readonly withoutTransform: Effect<ReadonlyArray<A>, SqlError>
  readonly stream: Stream.Stream<A, SqlError>
  readonly values: Effect<ReadonlyArray<ReadonlyArray<unknown>>, SqlError>
  readonly unprepared: Effect<ReadonlyArray<A>, SqlError>
  readonly compile: (withoutTransform?: boolean | undefined) => readonly [sql: string, params: ReadonlyArray<unknown>]
}

type Segment = Literal | Identifier | Parameter | ArrayHelper | RecordInsertHelper | RecordUpdateHelper | RecordUpdateHelperSingle | Custom
type Helper = ArrayHelper | RecordInsertHelper | RecordUpdateHelper | RecordUpdateHelperSingle | Identifier | Custom

interface Literal { readonly _tag: "Literal"; readonly value: string; readonly params?: ReadonlyArray<unknown> | undefined }
interface Identifier { readonly _tag: "Identifier"; readonly value: string }
interface Parameter { readonly _tag: "Parameter"; readonly value: unknown }
interface ArrayHelper { readonly _tag: "ArrayHelper"; readonly value: ReadonlyArray<unknown | Fragment> }
interface RecordInsertHelper {
  readonly _tag: "RecordInsertHelper"
  readonly value: ReadonlyArray<Record<string, unknown>>
  readonly returning: (sql: string | Identifier | Fragment) => RecordInsertHelper
}
interface RecordUpdateHelper {
  readonly _tag: "RecordUpdateHelper"
  readonly value: ReadonlyArray<Record<string, unknown>>
  readonly alias: string
  readonly returning: (sql: string | Identifier | Fragment) => RecordUpdateHelper
}
interface RecordUpdateHelperSingle {
  readonly _tag: "RecordUpdateHelperSingle"
  readonly value: Record<string, unknown>
  readonly omit: ReadonlyArray<string>
  readonly returning: (sql: string | Identifier | Fragment) => RecordUpdateHelperSingle
}
interface Custom<T extends string = string, A = void, B = void, C = void> {
  readonly _tag: "Custom"; readonly kind: T; readonly i0: A; readonly i1: B; readonly i2: C
}

// The callable template builder. SqlClient extends this; PgClient is a SqlClient.
interface Constructor {
  <A extends object = Connection.Row>(strings: TemplateStringsArray, ...args: Array<any>): Statement<A>
  (value: string): Identifier
  /** Create an unsafe SQL query. */
  readonly unsafe: <A extends object>(sql: string, params?: ReadonlyArray<any> | undefined) => Statement<A>
  readonly literal: (sql: string) => Fragment
  readonly in: {
    (value: ReadonlyArray<unknown>): ArrayHelper
    (column: string, value: ReadonlyArray<unknown>): Fragment
  }
  readonly insert: {
    (value: ReadonlyArray<Record<string, unknown>>): RecordInsertHelper
    (value: Record<string, unknown>): RecordInsertHelper
  }
  /** Update a single row. */
  readonly update: <A extends Record<string, unknown>>(value: A, omit?: ReadonlyArray<keyof A>) => RecordUpdateHelperSingle
  /** Update multiple rows. **Note:** not supported in sqlite. */
  readonly updateValues: (value: ReadonlyArray<Record<string, unknown>>, alias: string) => RecordUpdateHelper
  readonly and: (clauses: ReadonlyArray<string | Fragment>) => Fragment
  readonly or: (clauses: ReadonlyArray<string | Fragment>) => Fragment
  readonly csv: {
    (values: ReadonlyArray<string | Fragment>): Fragment
    (prefix: string, values: ReadonlyArray<string | Fragment>): Fragment
  }
  readonly join: (literal: string, addParens?: boolean, fallback?: string) => (clauses: ReadonlyArray<string | Fragment>) => Fragment
  readonly onDialect: <A, B, C, D, E>(options: {
    readonly sqlite: () => A; readonly pg: () => B; readonly mysql: () => C; readonly mssql: () => D; readonly clickhouse: () => E
  }) => A | B | C | D | E
  readonly onDialectOrElse: <A, B = never, C = never, D = never, E = never, F = never>(options: {
    readonly orElse: () => A
    readonly sqlite?: () => B; readonly pg?: () => C; readonly mysql?: () => D; readonly mssql?: () => E; readonly clickhouse?: () => F
  }) => A | B | C | D | E | F
}

interface Compiler {
  readonly dialect: Dialect
  readonly compile: (statement: Fragment, withoutTransform: boolean) => readonly [sql: string, params: ReadonlyArray<unknown>]
  readonly withoutTransform: this
}

const make: (
  acquirer: Connection.Acquirer,
  compiler: Compiler,
  spanAttributes: ReadonlyArray<readonly [string, unknown]>,
  transformRows: (<A extends object>(row: ReadonlyArray<A>) => ReadonlyArray<A>) | undefined
) => Constructor

const makeCompiler: <C extends Custom<any, any, any, any> = any>(options: {
  readonly dialect: Dialect
  readonly placeholder: (index: number, value: unknown) => string
  readonly onIdentifier: (value: string, withoutTransform: boolean) => string
  readonly onRecordUpdate: (placeholders: string, alias: string, columns: string, values: ReadonlyArray<ReadonlyArray<unknown>>, returning: readonly [sql: string, params: ReadonlyArray<unknown>] | undefined) => readonly [sql: string, params: ReadonlyArray<unknown>]
  readonly onCustom: (type: C, placeholder: (u: unknown) => string, withoutTransform: boolean) => readonly [sql: string, params: ReadonlyArray<unknown>]
  readonly onInsert?: (columns: ReadonlyArray<string>, placeholders: string, values: ReadonlyArray<ReadonlyArray<unknown>>, returning: readonly [sql: string, params: ReadonlyArray<unknown>] | undefined) => readonly [sql: string, binds: ReadonlyArray<unknown>]
  readonly onRecordUpdateSingle?: (columns: ReadonlyArray<string>, values: ReadonlyArray<unknown>, returning: readonly [sql: string, params: ReadonlyArray<unknown>] | undefined) => readonly [sql: string, params: ReadonlyArray<unknown>]
}) => Compiler
const makeCompilerSqlite: (transform?: ((_: string) => string) | undefined) => Compiler

const custom: <C extends Custom<any, any, any, any>>(kind: C["kind"]) => (i0: C["i0"], i1: C["i1"], i2: C["i2"]) => Fragment
const unsafeFragment: (sql: string, params?: ReadonlyArray<unknown> | undefined) => Fragment
const and: (clauses: ReadonlyArray<string | Fragment>) => Fragment
const or: (clauses: ReadonlyArray<string | Fragment>) => Fragment
const csv: { (values: ReadonlyArray<string | Fragment>): Fragment; (prefix: string, values: ReadonlyArray<string | Fragment>): Fragment }
const join: (literal: string, addParens?: boolean, fallback?: string) => (clauses: ReadonlyArray<string | Fragment>) => Fragment
const isFragment: (u: unknown) => u is Fragment
const isCustom: <A extends Custom<any, any, any, any>>(kind: A["kind"]) => (u: unknown) => u is A
const defaultEscape: (c: string) => (str: string) => string
const primitiveKind: (value: unknown) => PrimitiveKind
const defaultTransforms: (transformer: (str: string) => string, nested?: boolean) => {
  readonly value: (value: any) => any
  readonly object: (obj: Record<string, any>) => any
  readonly array: <A extends object>(rows: ReadonlyArray<A>) => ReadonlyArray<A>
}

declare namespace Statement {
  type Transformer = (self: Statement<unknown>, sql: Constructor, context: FiberRefs.FiberRefs, span: Tracer.Span) => Effect<Statement<unknown>>
}
const currentTransformer: FiberRef.FiberRef<Option.Option<Statement.Transformer>>
const withTransformer: {
  (f: Statement.Transformer): <A, E, R>(effect: Effect<A, E, R>) => Effect<A, E, R>
  <A, E, R>(effect: Effect<A, E, R>, f: Statement.Transformer): Effect<A, E, R>
}
const withTransformerDisabled: <A, E, R>(effect: Effect<A, E, R>) => Effect<A, E, R>
const setTransformer: (f: Statement.Transformer) => Layer.Layer<never, never, never>
```

### @effect/sql/SqlConnection — driver contract

[PUBLIC_TYPE_SCOPE]: connection + acquirer
- rail: persistence
- entry: `@effect/sql/SqlConnection`

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                                                                                                    |
| :-----: | :-------------------- | :------------ | :-------------------------------------------------------------------------------------------------------- |
|   [1]   | `Connection`          | interface     | the low-level execute/stream/values driver seam                                                           |
|   [2]   | `Connection.Acquirer` | type alias    | `Effect<Connection, SqlError, Scope>` — scoped connection get (namespace member, not a standalone export) |
|   [3]   | `Row`                 | type alias    | `Record<string, unknown>` — the default decoded-row shape                                                 |

```ts contract
import { SqlError } from "@effect/sql/SqlError"
import type { Effect } from "effect/Effect"
import type { Scope } from "effect/Scope"
import type { Stream } from "effect/Stream"

interface Connection {
  readonly execute: (
    sql: string, params: ReadonlyArray<unknown>,
    transformRows: (<A extends object>(row: ReadonlyArray<A>) => ReadonlyArray<A>) | undefined
  ) => Effect<ReadonlyArray<any>, SqlError>
  /** Raw results straight from the underlying client. */
  readonly executeRaw: (sql: string, params: ReadonlyArray<unknown>) => Effect<unknown, SqlError>
  readonly executeStream: (
    sql: string, params: ReadonlyArray<unknown>,
    transformRows: (<A extends object>(row: ReadonlyArray<A>) => ReadonlyArray<A>) | undefined
  ) => Stream<any, SqlError>
  readonly executeValues: (sql: string, params: ReadonlyArray<unknown>) => Effect<ReadonlyArray<ReadonlyArray<unknown>>, SqlError>
  readonly executeUnprepared: (
    sql: string, params: ReadonlyArray<unknown>,
    transformRows: (<A extends object>(row: ReadonlyArray<A>) => ReadonlyArray<A>) | undefined
  ) => Effect<ReadonlyArray<any>, SqlError>
}

declare namespace Connection {
  type Acquirer = Effect<Connection, SqlError, Scope>
}
type Row = { readonly [column: string]: unknown }
```

### @effect/sql/SqlError — typed error rail

[PUBLIC_TYPE_SCOPE]: errors
- rail: persistence
- entry: `@effect/sql/SqlError`

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]      | [RAIL]                                                 |
| :-----: | :--------------------- | :----------------- | :----------------------------------------------------- |
|   [1]   | `SqlErrorTypeId`       | const + type alias | branded error id (shared by both errors)               |
|   [2]   | `SqlError`             | tagged error       | `_tag: "SqlError"`; `{ cause?; message? }`             |
|   [3]   | `ResultLengthMismatch` | tagged error       | `_tag: "ResultLengthMismatch"`; `{ expected; actual }` |

```ts contract
import type { YieldableError } from "effect/Cause"

const SqlErrorTypeId: unique symbol
type SqlErrorTypeId = typeof SqlErrorTypeId

class SqlError extends YieldableError {
  readonly [SqlErrorTypeId]: SqlErrorTypeId
  readonly _tag: "SqlError"
  readonly cause?: unknown
  readonly message?: string
}

class ResultLengthMismatch extends YieldableError {
  readonly [SqlErrorTypeId]: SqlErrorTypeId
  readonly _tag: "ResultLengthMismatch"
  readonly expected: number
  readonly actual: number
  get message(): string
}
```

### @effect/sql/SqlSchema — schema-bridged query layer

[PUBLIC_TYPE_SCOPE]: request/result codecs
- rail: persistence
- entry: `@effect/sql/SqlSchema`

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY] | [RAIL]                                                                              |
| :-----: | :-------- | :------------ | :---------------------------------------------------------------------------------- |
|   [1]   | `findAll` | constructor   | request → `Effect<ReadonlyArray<A>, E \| ParseError, …>`                            |
|   [2]   | `findOne` | constructor   | request → `Effect<Option<A>, E \| ParseError, …>`                                   |
|   [3]   | `single`  | constructor   | request → `Effect<A, E \| ParseError \| NoSuchElementException, …>`                 |
|   [4]   | `void`    | constructor   | request → `Effect<void, E \| ParseError, …>` (exported as `void`, internal `void_`) |

```ts contract
import type * as Cause from "effect/Cause"
import type * as Effect from "effect/Effect"
import type * as Option from "effect/Option"
import type { ParseError } from "effect/ParseResult"
import type * as Schema from "effect/Schema"

const findAll: <IR, II, IA, AR, AI, A, R, E>(options: {
  readonly Request: Schema.Schema<IA, II, IR>
  readonly Result: Schema.Schema<A, AI, AR>
  readonly execute: (request: II) => Effect.Effect<ReadonlyArray<unknown>, E, R>
}) => (request: IA) => Effect.Effect<ReadonlyArray<A>, E | ParseError, R | IR | AR>

const findOne: <IR, II, IA, AR, AI, A, R, E>(options: {
  readonly Request: Schema.Schema<IA, II, IR>
  readonly Result: Schema.Schema<A, AI, AR>
  readonly execute: (request: II) => Effect.Effect<ReadonlyArray<unknown>, E, R>
}) => (request: IA) => Effect.Effect<Option.Option<A>, E | ParseError, R | IR | AR>

const single: <IR, II, IA, AR, AI, A, R, E>(options: {
  readonly Request: Schema.Schema<IA, II, IR>
  readonly Result: Schema.Schema<A, AI, AR>
  readonly execute: (request: II) => Effect.Effect<ReadonlyArray<unknown>, E, R>
}) => (request: IA) => Effect.Effect<A, E | ParseError | Cause.NoSuchElementException, R | IR | AR>

// exported under the name `void`
const void_: <IR, II, IA, R, E>(options: {
  readonly Request: Schema.Schema<IA, II, IR>
  readonly execute: (request: II) => Effect.Effect<unknown, E, R>
}) => (request: IA) => Effect.Effect<void, E | ParseError, R | IR>
export { void_ as void }
```

### @effect/sql/SqlResolver — batched request resolver layer

[PUBLIC_TYPE_SCOPE]: resolver + builders
- rail: persistence
- entry: `@effect/sql/SqlResolver`

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [RAIL]                                                                                                           |
| :-----: | :------------------ | :------------ | :--------------------------------------------------------------------------------------------------------------- |
|   [1]   | `SqlResolver<T,…>`  | interface     | extends `RequestResolver`; carries `execute`/`makeExecute`/`cachePopulate`/`cacheInvalidate`/`request` accessors |
|   [2]   | `SqlRequest<T,A,E>` | interface     | the tagged `Request` shape backing a resolver                                                                    |
|   [3]   | `ordered`           | constructor   | one-to-one request↔result ordering; `ResultLengthMismatch` on mismatch                                           |
|   [4]   | `grouped`           | constructor   | many-results-per-request keyed by request/result group key                                                       |
|   [5]   | `findById`          | constructor   | id-keyed resolver yielding `Option<A>` per id                                                                    |
|   [6]   | `void`              | constructor   | side-effecting resolver (exported as `void`, internal `void_`); result `void`                                    |

Each builder has a `withContext: false` (default) and a `withContext: true` overload; the latter
threads the result-schema context `RA`/`R` into the requirement channel.

```ts contract
import type * as Effect from "effect/Effect"
import type * as Option from "effect/Option"
import type { ParseError } from "effect/ParseResult"
import type * as Request from "effect/Request"
import type * as Schema from "effect/Schema"
import type * as Tracer from "effect/Tracer"
import type * as RequestResolver from "effect/RequestResolver"
import type * as Types from "effect/Types"
import type { ResultLengthMismatch } from "@effect/sql/SqlError"

interface SqlRequest<T extends string, A, E> extends Request.Request<A, E | ParseError> {
  readonly _tag: T
  readonly spanLink: Tracer.SpanLink
  readonly input: unknown
  readonly encoded: unknown
}

interface SqlResolver<T extends string, I, A, E, R> extends RequestResolver.RequestResolver<SqlRequest<T, A, E>> {
  readonly execute: (input: I) => Effect.Effect<A, E | ParseError, R>
  readonly makeExecute: (resolver: RequestResolver.RequestResolver<SqlRequest<T, A, E>>) => (input: I) => Effect.Effect<A, E | ParseError, R>
  readonly cachePopulate: (id: I, result: A) => Effect.Effect<void>
  readonly cacheInvalidate: (id: I) => Effect.Effect<void>
  readonly request: (input: I) => Effect.Effect<SqlRequest<T, A, E>, ParseError, R>
}

const ordered: <T extends string, I, II, RI, A, IA, _, E, RA = never, R = never>(tag: T, options:
  | { readonly Request: Schema.Schema<I, II, RI>; readonly Result: Schema.Schema<A, IA>; readonly execute: (requests: Array<Types.NoInfer<II>>) => Effect.Effect<ReadonlyArray<_>, E>; readonly withContext?: false }
  | { readonly Request: Schema.Schema<I, II, RI>; readonly Result: Schema.Schema<A, IA, RA>; readonly execute: (requests: Array<Types.NoInfer<II>>) => Effect.Effect<ReadonlyArray<_>, E, R>; readonly withContext: true }
) => Effect.Effect<SqlResolver<T, I, A, E | ResultLengthMismatch, RI>, never, RA | R>

const grouped: <T extends string, I, II, K, RI, A, IA, Row, E, RA = never, R = never>(tag: T, options:
  | { readonly Request: Schema.Schema<I, II, RI>; readonly RequestGroupKey: (request: Types.NoInfer<I>) => K; readonly Result: Schema.Schema<A, IA>; readonly ResultGroupKey: (result: Types.NoInfer<A>, row: Types.NoInfer<Row>) => K; readonly execute: (requests: Array<Types.NoInfer<II>>) => Effect.Effect<ReadonlyArray<Row>, E>; readonly withContext?: false }
  | { readonly Request: Schema.Schema<I, II, RI>; readonly RequestGroupKey: (request: Types.NoInfer<I>) => K; readonly Result: Schema.Schema<A, IA, RA>; readonly ResultGroupKey: (result: Types.NoInfer<A>, row: Types.NoInfer<Row>) => K; readonly execute: (requests: Array<Types.NoInfer<II>>) => Effect.Effect<ReadonlyArray<Row>, E, R>; readonly withContext: true }
) => Effect.Effect<SqlResolver<T, I, Array<A>, E, RI>, never, RA | R>

const findById: <T extends string, I, II, RI, A, IA, Row, E, RA = never, R = never>(tag: T, options:
  | { readonly Id: Schema.Schema<I, II, RI>; readonly Result: Schema.Schema<A, IA>; readonly ResultId: (result: Types.NoInfer<A>, row: Types.NoInfer<Row>) => I; readonly execute: (requests: Array<Types.NoInfer<II>>) => Effect.Effect<ReadonlyArray<Row>, E>; readonly withContext?: false }
  | { readonly Id: Schema.Schema<I, II, RI>; readonly Result: Schema.Schema<A, IA, RA>; readonly ResultId: (result: Types.NoInfer<A>, row: Types.NoInfer<Row>) => I; readonly execute: (requests: Array<Types.NoInfer<II>>) => Effect.Effect<ReadonlyArray<Row>, E, R>; readonly withContext: true }
) => Effect.Effect<SqlResolver<T, I, Option.Option<A>, E, RI>, never, RA | R>

// exported under the name `void` — side-effecting resolver
const void_: <T extends string, I, II, RI, E, R = never>(tag: T, options:
  | { readonly Request: Schema.Schema<I, II, RI>; readonly execute: (requests: Array<Types.NoInfer<II>>) => Effect.Effect<ReadonlyArray<unknown>, E>; readonly withContext?: false }
  | { readonly Request: Schema.Schema<I, II, RI>; readonly execute: (requests: Array<Types.NoInfer<II>>) => Effect.Effect<unknown, E, R>; readonly withContext: true }
) => Effect.Effect<SqlResolver<T, I, void, E, RI>, never, R>
export { void_ as void }
```

### @effect/sql/Model — schema-class family + CRUD builders

[PUBLIC_TYPE_SCOPE]: model class + variant fields
- rail: persistence
- entry: `@effect/sql/Model`

| [INDEX] | [SYMBOL]                                                                                                                                                                          | [TYPE_FAMILY]       | [RAIL]                                                                      |
| :-----: | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------ | :-------------------------------------------------------------------------- |
|   [1]   | `Class<Self>`                                                                                                                                                                     | class factory       | `Model.Class<Self>("Id")({ …fields })` — the variant schema class           |
|   [2]   | `Any` / `AnyNoContext`                                                                                                                                                            | type alias          | model upper bounds for builder generics                                     |
|   [3]   | `VariantsDatabase`                                                                                                                                                                | string union        | `"select" \| "insert" \| "update"`                                          |
|   [4]   | `VariantsJson`                                                                                                                                                                    | string union        | `"json" \| "jsonCreate" \| "jsonUpdate"`                                    |
|   [5]   | `fields`                                                                                                                                                                          | accessor            | extract the raw field map from a model                                      |
|   [6]   | `Field`/`FieldOnly`/`FieldExcept`/`fieldEvolve`/`fieldFromKey`                                                                                                                    | field combinators   | per-variant field shaping (returned alongside `Class` from the same module) |
|   [7]   | `extract`                                                                                                                                                                         | combinator          | extract one variant schema (`extract(variant)` or `extract(self, variant)`) |
|   [8]   | `Struct` / `Union`                                                                                                                                                                | constructors        | variant-aware struct / tagged-union schema builders (mirror `Model.Class`)  |
|   [9]   | `Override`                                                                                                                                                                        | brand ctor          | `<A>(value) => A & Brand<"Override">`                                       |
|  [10]   | `Generated`                                                                                                                                                                       | field ctor          | DB-generated: select+update+json, not insert                                |
|  [11]   | `GeneratedByApp`                                                                                                                                                                  | field ctor          | app-generated: required by DB, not by JSON variants                         |
|  [12]   | `Sensitive`                                                                                                                                                                       | field ctor          | select+insert+update, excluded from JSON variants                           |
|  [13]   | `FieldOption`                                                                                                                                                                     | field ctor          | nullable in DB variants, optional in JSON variants                          |
|  [14]   | `JsonFromString`                                                                                                                                                                  | field ctor          | parse/stringify JSON column                                                 |
|  [15]   | `Date`/`DateTimeFromDate`/`DateWithNow`/`DateTimeWithNow`/`DateTimeInsert`/`DateTimeUpdate` (+`DateTime{Insert,Update}{FromDate,FromNumber}`, `DateTimeFrom{Date,Number}WithNow`) | overrideable fields | timestamp columns                                                           |
|  [16]   | `UuidV4Insert`/`UuidV4WithGenerate`                                                                                                                                               | overrideable fields | generated UUID columns                                                      |
|  [17]   | `BooleanFromNumber`                                                                                                                                                               | field schema        | 0/1 boolean column                                                          |

[PUBLIC_TYPE_SCOPE]: repository builders
- rail: persistence

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [RAIL]                                                               |
| :-----: | :---------------- | :------------ | :------------------------------------------------------------------- |
|   [1]   | `makeRepository`  | constructor   | model → CRUD effect record (`insert`/`update`/`findById`/`delete`/…) |
|   [2]   | `makeDataLoaders` | constructor   | model → batched data-loader record (windowed; needs `Scope`)         |

```ts contract
import type { Brand } from "effect/Brand"
import type * as DateTime from "effect/DateTime"
import type { DurationInput } from "effect/Duration"
import type * as Effect from "effect/Effect"
import type * as Option from "effect/Option"
import type * as Scope from "effect/Scope"
import type * as Schema from "effect/Schema"
import type * as VariantSchema from "@effect/experimental/VariantSchema"
import type { SqlClient } from "@effect/sql/SqlClient"

// Self-typed model class: `class Group extends Model.Class<Group>("Group")({ id: Model.Generated(...), ... }) {}`
// The class exposes per-variant schemas: `.insert`, `.update`, `.json`, `.jsonCreate`, `.jsonUpdate`, plus `.fields`.
const Class: <Self = never>(identifier: string) => <const Fields extends VariantSchema.Struct.Fields>(
  fields: Fields & VariantSchema.Struct.Validate<Fields, "select" | "insert" | "update" | "json" | "jsonCreate" | "jsonUpdate">,
  annotations?: Schema.Annotations.Schema<Self, readonly []> | undefined
) => /* VariantSchema.Class<Self, Fields, …> — requires the Self generic */ unknown

type VariantsDatabase = "select" | "insert" | "update"
type VariantsJson = "json" | "jsonCreate" | "jsonUpdate"

const fields: <A extends VariantSchema.Struct<any>>(self: A) => A[VariantSchema.TypeId]
const Override: <A>(value: A) => A & Brand<"Override">

interface Generated<S extends Schema.Schema.All | Schema.PropertySignature.All>
  extends VariantSchema.Field<{ readonly select: S; readonly update: S; readonly json: S }> {}
const Generated: <S extends Schema.Schema.All | Schema.PropertySignature.All>(schema: S) => Generated<S>

interface GeneratedByApp<S extends Schema.Schema.All | Schema.PropertySignature.All>
  extends VariantSchema.Field<{ readonly select: S; readonly insert: S; readonly update: S; readonly json: S }> {}
const GeneratedByApp: <S extends Schema.Schema.All | Schema.PropertySignature.All>(schema: S) => GeneratedByApp<S>

interface Sensitive<S extends Schema.Schema.All | Schema.PropertySignature.All>
  extends VariantSchema.Field<{ readonly select: S; readonly insert: S; readonly update: S }> {}
const Sensitive: <S extends Schema.Schema.All | Schema.PropertySignature.All>(schema: S) => Sensitive<S>

const FieldOption: <Field extends VariantSchema.Field<any> | Schema.Schema.Any>(self: Field) => /* nullable-in-db, optional-in-json */ unknown
const JsonFromString: <S extends Schema.Schema.All | Schema.PropertySignature.All>(schema: S) => JsonFromString<S>

const DateWithNow: VariantSchema.Overrideable<DateTime.Utc, string, never>
const DateTimeFromDateWithNow: VariantSchema.Overrideable<DateTime.Utc, globalThis.Date, never>
const DateTimeFromNumberWithNow: VariantSchema.Overrideable<DateTime.Utc, number, never>
const UuidV4WithGenerate: <B extends string | symbol>(schema: Schema.brand<typeof Schema.Uint8ArrayFromSelf, B>) => VariantSchema.Overrideable<Uint8Array & Brand<B>, Uint8Array>

const makeRepository: <S extends Any, Id extends (keyof S["Type"]) & (keyof S["update"]["Type"]) & (keyof S["fields"])>(
  Model: S, options: { readonly tableName: string; readonly spanPrefix: string; readonly idColumn: Id }
) => Effect.Effect<{
  readonly insert: (insert: S["insert"]["Type"]) => Effect.Effect<S["Type"], never, S["Context"] | S["insert"]["Context"]>
  readonly insertVoid: (insert: S["insert"]["Type"]) => Effect.Effect<void, never, S["Context"] | S["insert"]["Context"]>
  readonly update: (update: S["update"]["Type"]) => Effect.Effect<S["Type"], never, S["Context"] | S["update"]["Context"]>
  readonly updateVoid: (update: S["update"]["Type"]) => Effect.Effect<void, never, S["Context"] | S["update"]["Context"]>
  readonly findById: (id: Schema.Schema.Type<S["fields"][Id]>) => Effect.Effect<Option.Option<S["Type"]>, never, S["Context"] | Schema.Schema.Context<S["fields"][Id]>>
  readonly delete: (id: Schema.Schema.Type<S["fields"][Id]>) => Effect.Effect<void, never, Schema.Schema.Context<S["fields"][Id]>>
}, never, SqlClient>

const makeDataLoaders: <S extends AnyNoContext, Id extends (keyof S["Type"]) & (keyof S["update"]["Type"]) & (keyof S["fields"])>(
  Model: S, options: { readonly tableName: string; readonly spanPrefix: string; readonly idColumn: Id; readonly window: DurationInput; readonly maxBatchSize?: number | undefined }
) => Effect.Effect<{
  readonly insert: (insert: S["insert"]["Type"]) => Effect.Effect<S["Type"]>
  readonly insertVoid: (insert: S["insert"]["Type"]) => Effect.Effect<void>
  readonly findById: (id: Schema.Schema.Type<S["fields"][Id]>) => Effect.Effect<Option.Option<S["Type"]>>
  readonly delete: (id: Schema.Schema.Type<S["fields"][Id]>) => Effect.Effect<void>
}, never, SqlClient | Scope.Scope>
```

### @effect/sql/Migrator — generic migration runner

[PUBLIC_TYPE_SCOPE]: options, loaders, runner
- rail: migration
- entry: `@effect/sql/Migrator`

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                                                        |
| :-----: | :------------------- | :------------ | :------------------------------------------------------------ |
|   [1]   | `MigratorOptions<R>` | interface     | `{ loader; schemaDirectory?; table? }`                        |
|   [2]   | `Loader<R>`          | type alias    | effect yielding `ReadonlyArray<ResolvedMigration>`            |
|   [3]   | `ResolvedMigration`  | tuple alias   | `[id: number, name: string, load: Effect<…, …, SqlClient>]`   |
|   [4]   | `Migration`          | interface     | `{ id; name; createdAt }` applied-row record                  |
|   [5]   | `MigrationError`     | tagged error  | `_tag: "MigrationError"`; closed `reason` enum                |
|   [6]   | `make`               | constructor   | dialect-parameterized runner builder (`dumpSchema` hook)      |
|   [7]   | `fromGlob`           | loader        | `Record<string, () => Promise<any>>` → `Loader`               |
|   [8]   | `fromBabelGlob`      | loader        | `Record<string, any>` → `Loader`                              |
|   [9]   | `fromRecord`         | loader        | `Record<string, Effect<void, unknown, SqlClient>>` → `Loader` |

```ts contract
import type * as Effect from "effect/Effect"
import type { SqlClient } from "@effect/sql/SqlClient"
import type { SqlError } from "@effect/sql/SqlError"
import type { YieldableError } from "effect/Cause"

interface MigratorOptions<R = never> {
  readonly loader: Loader<R>
  readonly schemaDirectory?: string
  readonly table?: string
}
type Loader<R = never> = Effect.Effect<ReadonlyArray<ResolvedMigration>, MigrationError, R>
type ResolvedMigration = readonly [id: number, name: string, load: Effect.Effect<any, any, SqlClient>]
interface Migration { readonly id: number; readonly name: string; readonly createdAt: Date }

class MigrationError extends YieldableError {
  readonly _tag: "MigrationError"
  readonly cause?: unknown
  readonly reason: "bad-state" | "import-error" | "failed" | "duplicates" | "locked"
  readonly message: string
}

const make: <RD = never>(opts: {
  dumpSchema?: (path: string, migrationsTable: string) => Effect.Effect<void, MigrationError, RD>
}) => <R2 = never>(options: MigratorOptions<R2>) =>
  Effect.Effect<ReadonlyArray<readonly [id: number, name: string]>, MigrationError | SqlError, SqlClient | RD | R2>

const fromGlob: (migrations: Record<string, () => Promise<any>>) => Loader
const fromBabelGlob: (migrations: Record<string, any>) => Loader
const fromRecord: (migrations: Record<string, Effect.Effect<void, unknown, SqlClient>>) => Loader
```

### @effect/sql/SqlStream — pause/resume stream adapter

[PUBLIC_TYPE_SCOPE]: stream constructor
- rail: persistence
- entry: `@effect/sql/SqlStream`

```ts contract
import type * as Chunk from "effect/Chunk"
import type * as Effect from "effect/Effect"
import type * as Stream from "effect/Stream"

// Adapt a backpressured cursor into a Stream; the register callback receives push emitters
// and returns lifecycle hooks the runtime drives for flow control.
const asyncPauseResume: <A, E = never, R = never>(
  register: (emit: {
    readonly single: (item: A) => void
    readonly chunk: (chunk: Chunk.Chunk<A>) => void
    readonly array: (chunk: ReadonlyArray<A>) => void
    readonly fail: (error: E) => void
    readonly end: () => void
  }) => {
    readonly onInterrupt: Effect.Effect<void, never, R>
    readonly onPause: Effect.Effect<void>
    readonly onResume: Effect.Effect<void>
  },
  bufferSize?: number
) => Stream.Stream<A, E, R>
```

### @effect/sql/SqlEventJournal · SqlEventLogServer · SqlPersistedQueue — durable subsystems

[PUBLIC_TYPE_SCOPE]: durable persistence layers
- rail: durable
- entry: `@effect/sql/SqlEventJournal`, `@effect/sql/SqlEventLogServer`, `@effect/sql/SqlPersistedQueue`

| [INDEX] | [MODULE]            | [SYMBOLS]                                           | [RAIL]                                               |
| :-----: | :------------------ | :-------------------------------------------------- | :--------------------------------------------------- |
|   [1]   | `SqlEventJournal`   | `make`, `layer`                                     | SQL-backed `@effect/experimental` EventJournal store |
|   [2]   | `SqlEventLogServer` | `makeStorage`, `layerStorage`, `layerStorageSubtle` | SQL-backed EventLog server storage layers            |
|   [3]   | `SqlPersistedQueue` | `make`, `layerStore`                                | SQL-backed durable work queue store                  |

These provide `Layer`/effect constructors that back `@effect/experimental` EventJournal, EventLog,
and persisted-queue services with a `SqlClient`. They are reached only when the durable backplane
is provisioned on SQL; the load-bearing entry points are the `layer*`/`make*` constructors below.
Each is a non-generic option-bag constructor returning a concrete `Layer`/`Effect` over the named
`@effect/experimental` service — they do not carry call-site generics.

```ts contract
import type * as Duration from "effect/Duration"
import type * as Effect from "effect/Effect"
import type * as Layer from "effect/Layer"
import type { Scope } from "effect/Scope"
import type * as EventJournal from "@effect/experimental/EventJournal"
import type * as EventLogServer from "@effect/experimental/EventLogServer"
import type { EventLogEncryption } from "@effect/experimental/EventLogEncryption"
import type * as PersistedQueue from "@effect/experimental/PersistedQueue"
import type { SqlClient } from "@effect/sql/SqlClient"
import type { SqlError } from "@effect/sql/SqlError"

// SqlEventJournal
const make: (options?: { readonly entryTable?: string; readonly remotesTable?: string })
  => Effect.Effect<typeof EventJournal.EventJournal.Service, SqlError, SqlClient>
const layer: (options?: { readonly eventLogTable?: string; readonly remotesTable?: string })
  => Layer.Layer<EventJournal.EventJournal, SqlError, SqlClient>

// SqlEventLogServer  (entryTablePrefix / remoteIdTable / insertBatchSize)
const makeStorage: (options?: { readonly entryTablePrefix?: string; readonly remoteIdTable?: string; readonly insertBatchSize?: number })
  => Effect.Effect<typeof EventLogServer.Storage.Service, SqlError, SqlClient | EventLogEncryption | Scope>
const layerStorage: (options?: { readonly entryTablePrefix?: string; readonly remoteIdTable?: string; readonly insertBatchSize?: number })
  => Layer.Layer<EventLogServer.Storage, SqlError, SqlClient | EventLogEncryption>
const layerStorageSubtle: (options?: { readonly entryTablePrefix?: string; readonly remoteIdTable?: string; readonly insertBatchSize?: number })
  => Layer.Layer<EventLogServer.Storage, SqlError, SqlClient>

// SqlPersistedQueue  (tableName / pollInterval / lockRefreshInterval / lockExpiration)
const makeQueue: (options?: { readonly tableName?: string; readonly pollInterval?: Duration.DurationInput; readonly lockRefreshInterval?: Duration.DurationInput; readonly lockExpiration?: Duration.DurationInput })
  => Effect.Effect<PersistedQueue.PersistedQueueStore["Type"], SqlError, SqlClient | Scope>
const layerStore: (options?: { readonly tableName?: string; readonly pollInterval?: Duration.DurationInput; readonly lockRefreshInterval?: Duration.DurationInput; readonly lockExpiration?: Duration.DurationInput })
  => Layer.Layer<PersistedQueue.PersistedQueueStore, SqlError, SqlClient>
```

`layerStorageSubtle` drops the `EventLogEncryption` requirement (SubtleCrypto-free variant);
`layerStorage` requires it. `SqlPersistedQueue` exports its constructor as `make` (aliased `makeQueue`
above to avoid colliding with the `SqlEventJournal.make` row).

## [3]-[IMPLEMENTATION_LAW]

[CLIENT_TOPOLOGY]:
- `SqlClient` is the abstract query service and the callable template tag in one: it extends
  `Statement.Constructor`, so the tag value is invoked as `` sql`SELECT * FROM t WHERE id = ${id}` ``
  yielding a `Statement<Row>`, and also exposes `unsafe`, `literal`, `in`, `insert`, `update`,
  `updateValues`, `and`, `or`, `csv`, `join`, `onDialect`, `onDialectOrElse` as fragment builders.
- Portable persistence logic depends on the `SqlClient` tag from this package; dialect packages
  (`@effect/sql-pg` `PgClient`, etc.) provision it. Never construct `SqlClient` with bare `make` in
  application code — `make` is the dialect-package authoring seam (it takes a `Connection.Acquirer`
  and a `Statement.Compiler`); a dialect `layer` is the consumer entry.
- A `Statement<A>` IS an `Effect<ReadonlyArray<A>, SqlError>`: yield it to run and decode all rows.
  Use `.stream` for a row `Stream`, `.values` for positional arrays, `.raw` for the driver-native
  result, `.unprepared` to skip statement preparation, `.withoutTransform` to bypass row transforms,
  and `.compile()` to extract `[sql, params]` without executing.
- `withTransaction(self)` wraps `self` in a transaction with nested-savepoint semantics; `reserve`
  hands out a scoped `Connection` for manual control. `SafeIntegers` is a context reference
  (default `false`) toggling bigint decoding of 64-bit columns.
- `make` carries `Reactivity` in its requirement channel (the reactive-query backbone from
  `@effect/experimental`); dialect layers discharge it via the bundled default.

[STATEMENT_TOPOLOGY]:
- The `Fragment` is the compile unit: a `ReadonlyArray<Segment>` where each `Segment` is a
  `Literal`, `Identifier`, `Parameter`, `ArrayHelper`, `RecordInsertHelper`,
  `RecordUpdateHelper(Single)`, `Custom`, or nested `Fragment`. `and`/`or`/`csv`/`join` compose
  fragments for WHERE/ORDER BY/GROUP BY; `unsafeFragment` injects raw SQL+params.
- `onDialect`/`onDialectOrElse` are the in-query dialect switches; the closed `Dialect` union is
  `"sqlite" | "pg" | "mysql" | "mssql" | "clickhouse"`. `updateValues` (multi-row update) is
  explicitly unsupported on sqlite.
- A `Compiler` (`{ dialect; compile; withoutTransform }`) turns a `Fragment` into `[sql, params]`.
  Dialect packages build theirs via `makeCompiler` (emit-callback driven) or `makeCompilerSqlite`.
  The `Transformer` fiber-ref (`withTransformer`/`withTransformerDisabled`/`setTransformer`) rewrites
  statements per-fiber for cross-cutting concerns (e.g. identifier casing) without touching call sites.

[SCHEMA_AND_RESOLVER_TOPOLOGY]:
- `SqlSchema` bridges a raw row array into decoded domain values via `Request`/`Result` schemas:
  `findAll` → array, `findOne` → `Option`, `single` → value or `NoSuchElementException`, `void`
  → discard. Every variant adds `ParseError` to the error channel and threads schema contexts
  (`IR`/`AR`) into the requirement channel — this is the single decode owner; never hand-map rows.
- `SqlResolver` builds batched `RequestResolver`s for the Effect request/dedup machinery:
  `ordered` (1:1 positional, fails `ResultLengthMismatch` on count drift), `grouped` (N results per
  request keyed by a group key), `findById` (id-keyed `Option`). Use the `withContext: true`
  overload only when result schemas carry context.

[MODEL_TOPOLOGY]:
- `Model.Class<Self>("Id")({ …fields })` declares one schema with per-operation variants reachable
  as `.insert`/`.update`/`.json`/`.jsonCreate`/`.jsonUpdate` plus base select; the `Self` generic is
  mandatory (`class X extends Model.Class<X>("X")({…}) {}`). Choose field markers by lifecycle:
  `Generated` (DB-set: select/update/json, not insert), `GeneratedByApp` (app-set: DB-required, not
  JSON), `Sensitive` (DB variants only, never JSON), `FieldOption` (nullable in DB, optional in JSON),
  the `DateTime*`/`UuidV4*` overrideables for timestamps and generated UUIDs.
- `makeRepository(Model, { tableName, spanPrefix, idColumn })` yields the canonical CRUD record
  (`insert`/`insertVoid`/`update`/`updateVoid`/`findById`/`delete`) requiring `SqlClient`;
  `makeDataLoaders` yields the batched/windowed variant additionally requiring `Scope`. These are the
  repository owners — do not hand-roll per-entity insert/update/find functions.

[MIGRATOR_TOPOLOGY]:
- `Migrator.make({ dumpSchema? })` is the dialect-parameterized runner factory; dialect packages
  (e.g. `@effect/sql-pg/PgMigrator`) wrap it into `run`/`layer`. Pick a loader by migration source:
  `fromGlob` (bundler lazy-import map), `fromBabelGlob` (eager import map), `fromRecord` (inline
  programmatic effects); dialect/platform packages add filesystem loaders.
- `MigrationError.reason` is a closed five-member enum (`"bad-state" | "import-error" | "failed" |
  "duplicates" | "locked"`) — match on `reason`, never on `message` text. `MigratorOptions.table`
  names the bookkeeping table; `schemaDirectory` targets the `dumpSchema` hook.

[RAIL_LAW]:
- `@effect/sql`: persistence / sql-core rail; dialect-agnostic and tier-agnostic at the type level,
  but every concrete `SqlClient` provider (the `@effect/sql-*` dialect packages) is node/bun-tier
  with a host driver dependency. Browser bundles never import a dialect package; query results cross
  the wire/transport tier as decoded domain values, not `SqlClient`/`Statement` handles.
- The `SqlError` rail is the single failure channel for all execution; `ParseError` rides alongside
  it on the `SqlSchema`/`SqlResolver`/`Model` layers; `MigrationError` is the migration-only rail.
