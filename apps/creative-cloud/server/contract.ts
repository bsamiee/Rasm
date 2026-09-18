// --- [IMPORTS] -------------------------------------------------------------------------

import { type Context, Crypto, Effect, Layer, Schema, Struct } from 'effect';
import { McpServer, Tool, type Toolkit } from 'effect/unstable/ai';
import { BridgeError } from './errors.ts';
import { answer, type Scope, type Session } from './socket.ts';
import { Autocorrections, type SocketHost, TIMEOUT_MS, Undo } from './values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Services<Dependencies extends readonly Context.Key<unknown, unknown>[]> = Context.Service.Identifier<Dependencies[number]>;

type Reply<S extends Schema.Constraint> = Schema.Struct<{ readonly result: Schema.Union<readonly [S, typeof Failure]> }>;

type Member<T> = Tool.SuccessSchema<T> extends Reply<infer S> ? S['Type'] : never;

type Handlers<Tools extends Record<string, Tool.Any>> = {
    readonly [Name in keyof Tools]: (input: Tool.Parameters<Tools[Name]>) => Effect.Effect<Member<Tools[Name]>, BridgeError, Tool.HandlerServices<Tools[Name]>>;
};

type Snake<S extends string> = S extends `${infer Head}${infer Tail}` ? `${Head extends Lowercase<Head> ? Head : `_${Lowercase<Head>}`}${Snake<Tail>}` : S;

type Bodies = Record<string, Schema.Codec<unknown, unknown, never, never>>;

type Replies<Kinds> = { readonly [K in keyof Kinds]: Schema.Codec<unknown, unknown, never, never> };

interface Answering extends Struct.Lambda {
    <Input, Output, Requirements>(
        handler: (input: Input) => Effect.Effect<Output, BridgeError, Requirements>,
    ): (input: Input) => Effect.Effect<{ readonly result: Output | (typeof Failure)['Type'] }, never, Requirements>;
    readonly '~lambda.out': this['~lambda.in'] extends (input: infer Input) => Effect.Effect<infer Output, BridgeError, infer Requirements>
        ? (input: Input) => Effect.Effect<{ readonly result: Output | (typeof Failure)['Type'] }, never, Requirements>
        : never;
}

// --- [MODELS] --------------------------------------------------------------------------

const Failure: Schema.Struct<{ readonly kind: Schema.Literal<'error'>; readonly error: typeof BridgeError }> = Schema.Struct({ kind: Schema.Literal('error'), error: BridgeError });

const Value: Schema.Struct<{
    readonly kind: Schema.Literal<'value'>;
    readonly value: Schema.Codec<Schema.Json>;
    readonly tookMs: Schema.Number;
    readonly autocorrections: typeof Autocorrections;
    readonly undo: Schema.OptionFromOptionalKey<typeof Undo>;
}> = Schema.Struct({ kind: Schema.Literal('value'), value: Schema.Json, tookMs: Schema.Number, autocorrections: Autocorrections, undo: Schema.OptionFromOptionalKey(Undo) });

// --- [CONTRACT] ------------------------------------------------------------------------

const _answering = Struct.lambda<Answering>(
    (handler) =>
        (input): ReturnType<ReturnType<Answering>> =>
            Effect.match(handler(input), { onSuccess: (value) => ({ result: value }), onFailure: (error) => ({ result: Failure.make({ kind: 'error', error }) }) }),
);

const tool =
    <const Dependencies extends readonly Context.Key<unknown, unknown>[]>(dependencies: Dependencies) =>
    <const Name extends string, Parameters extends Schema.Codec<unknown, unknown, never, never>, Success extends Schema.Codec<unknown, unknown, never, never>>(
        name: Name,
        description: string,
        parameters: Parameters,
        success: Success,
        readOnly: boolean,
    ): Tool.Tool<Name, { readonly parameters: Parameters; readonly success: Reply<Success>; readonly failure: Schema.Never; readonly failureMode: 'error' }, Services<Dependencies>> =>
        Tool.make(name, { description, parameters, success: Schema.Struct({ result: Schema.Union([success, Failure]) }), dependencies: [...dependencies] }).annotate(Tool.Readonly, readOnly);

const plain =
    <const Kinds extends Bodies, const Results extends Replies<NoInfer<Kinds>>>(bodies: Schema.Struct<Kinds>, results: Schema.Struct<Results>) =>
    <const Name extends `${SocketHost}_${Snake<K>}`, const K extends keyof Kinds & string>(
        name: Name,
        kind: K,
        description: string,
        readOnly: boolean,
    ): Tool.Tool<Name, { readonly parameters: Kinds[K]; readonly success: Reply<Results[K]>; readonly failure: Schema.Never; readonly failureMode: 'error' }, Crypto.Crypto> =>
        tool([Crypto.Crypto])(name, description, Struct.get(bodies.fields, kind), Struct.get(results.fields, kind), readOnly);

const forward =
    <const Kinds extends Bodies, const Results extends Replies<NoInfer<Kinds>>>(bodies: Schema.Struct<Kinds>, results: Schema.Struct<Results>) =>
    <const K extends keyof Kinds & string>(channel: Session, kind: K, scope: Scope) =>
    (input: Kinds[K]['Type']): Effect.Effect<Results[K]['Type'], BridgeError, Crypto.Crypto> =>
        Effect.map(answer(bodies)(channel, TIMEOUT_MS, kind, input, Struct.get(results.fields, kind), scope), Struct.get('value'));

const succeeded = <S extends Schema.Constraint>(row: { readonly successSchema: Reply<S> }): S => row.successSchema.fields.result.members[0];

const answering = <Tools extends Record<string, Tool.Any>>(
    _toolkit: Toolkit.Toolkit<Tools>,
    handlers: Handlers<Tools>,
): { readonly [Name in keyof Tools]: Struct.Apply<Answering, Handlers<Tools>[Name]> } => Struct.map(handlers, _answering);

const host = <Tools extends Record<string, Tool.Any>, Channel>(
    toolkit: Toolkit.Toolkit<Tools>,
    channel: Context.Service<Channel, Channel>,
    handlers: (channel: Channel) => Toolkit.HandlersFrom<Tools>,
): Layer.Layer<never, never, Channel | Tool.HandlerServices<Tools>> => Layer.provide(McpServer.toolkit(toolkit), toolkit.toLayer(Effect.map(channel, handlers)));

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Snake };
export { answering, Failure, forward, host, plain, succeeded, tool, Value };
