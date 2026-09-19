// --- [IMPORTS] -------------------------------------------------------------------------

import { Context, Crypto, Effect, type FileSystem, flow, Layer, Option, Schema, Stream, Struct } from 'effect';
import { AiError, McpSchema, McpServer, Tool, type Toolkit } from 'effect/unstable/ai';
import { BridgeError, faulted } from './errors.ts';
import { rendered } from './media.ts';
import { prepared, type Scope, type Session } from './socket.ts';
import { Autocorrections, type SocketHost, TIMEOUT_MS, Undo } from './values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Services<Dependencies extends readonly Context.Key<unknown, unknown>[]> = Context.Service.Identifier<Dependencies[number]>;

type Reply<S extends Schema.Constraint> = Schema.toCodecJson<Schema.Struct<{ readonly result: Schema.Union<readonly [S, typeof Failure]> }>>;

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
const _failed = Schema.is(Schema.Struct({ result: Failure }));

const Value: Schema.Struct<{
    readonly kind: Schema.Literal<'value'>;
    readonly value: Schema.Codec<Schema.Json>;
    readonly tookMs: Schema.Number;
    readonly autocorrections: typeof Autocorrections;
    readonly undo: Schema.OptionFromOptionalKey<typeof Undo>;
}> = Schema.Struct({ kind: Schema.Literal('value'), value: Schema.Json, tookMs: Schema.Number, autocorrections: Autocorrections, undo: Schema.OptionFromOptionalKey(Undo) });

// --- [CONTRACT] ------------------------------------------------------------------------

const tool =
    <const Dependencies extends readonly Context.Key<unknown, unknown>[]>(dependencies: Dependencies) =>
    <const Name extends string, Parameters extends Schema.Codec<unknown, unknown, never, never>, Success extends Schema.Codec<unknown, unknown, never, never>>(
        name: Name,
        description: string,
        parameters: Parameters,
        success: Success,
        readOnly: boolean,
    ): Tool.Tool<
        Name,
        { readonly parameters: Schema.toCodecJson<Parameters>; readonly success: Reply<Success>; readonly failure: Schema.Never; readonly failureMode: 'error' },
        Services<Dependencies>
    > =>
        Tool.make(name, {
            description,
            parameters: Schema.toCodecJson(parameters),
            success: Schema.toCodecJson(Schema.Struct({ result: Schema.Union([success, Failure]) })),
            dependencies: [...dependencies],
        }).annotate(Tool.Readonly, readOnly);

const plain =
    <const Kinds extends Bodies, const Results extends Replies<NoInfer<Kinds>>>(bodies: Schema.Struct<Kinds>, results: Schema.Struct<Results>) =>
    <const Name extends `${SocketHost}_${Snake<K>}`, const K extends keyof Kinds & string>(
        name: Name,
        kind: K,
        description: string,
        readOnly: boolean,
    ): Tool.Tool<Name, { readonly parameters: Schema.toCodecJson<Kinds[K]>; readonly success: Reply<Results[K]>; readonly failure: Schema.Never; readonly failureMode: 'error' }, Crypto.Crypto> =>
        tool([Crypto.Crypto])(name, description, Struct.get(bodies.fields, kind), Struct.get(results.fields, kind), readOnly);

const forward =
    <const Kinds extends Bodies, const Results extends Replies<NoInfer<Kinds>>>(bodies: Schema.Struct<Kinds>, results: Schema.Struct<Results>) =>
    <const K extends keyof Kinds & string>(channel: Session, kind: K, scope: Scope) =>
    (input: Kinds[K]['Type']): Effect.Effect<Results[K]['Type'], BridgeError, Crypto.Crypto> =>
        Effect.map(
            prepared(bodies)(channel, TIMEOUT_MS, kind, () => Effect.succeed(input), Struct.get(results.fields, kind), scope),
            Struct.get('value'),
        );

const succeeded = <S extends Schema.Constraint>(row: { readonly successSchema: Reply<S> }): S => row.successSchema.schema.fields.result.members[0];

const answering: Answering = Struct.lambda<Answering>(
    (handler) =>
        (input): ReturnType<ReturnType<Answering>> =>
            Effect.match(handler(input), { onSuccess: (value) => ({ result: value }), onFailure: (error) => ({ result: Failure.make({ kind: 'error', error }) }) }),
);

const _protocolError = (error: unknown): McpSchema.InvalidParams | McpSchema.InternalError =>
    AiError.isAiError(error) && error.reason._tag === 'ToolParameterValidationError'
        ? new McpSchema.InvalidParams({ message: error.reason.message })
        : new McpSchema.InternalError({ message: 'Tool execution failed', data: Schema.encodeSync(Schema.Defect())(error) });

const _register = Effect.fnUntraced(function* <Tools extends Record<string, Tool.Any>>(
    built: Toolkit.WithHandler<Tools>,
    services: Context.Context<Tool.HandlerServices<Tools[keyof Tools]> | FileSystem.FileSystem>,
    name: keyof Tools,
) {
    const registry = yield* McpServer.McpServer;
    const row = Struct.get(built.tools, name);
    const [inputSchema, outputSchema] = yield* Effect.all([
        Schema.decodeUnknownEffect(McpSchema.ToolJson)({ ...Tool.getJsonSchema(row), type: 'object' }),
        Schema.decodeUnknownEffect(McpSchema.ToolOutputJson)(Tool.getJsonSchemaFromSchema(row.successSchema)),
    ]).pipe(Effect.orDie);
    yield* registry.addTool({
        annotations: row.annotations,
        tool: new McpSchema.Tool({
            name: row.name,
            description: Tool.getDescription(row),
            inputSchema,
            outputSchema,
            _meta: Option.getOrUndefined(Context.getOption(row.annotations, Tool.Meta)),
            annotations: {
                title: Option.getOrUndefined(Context.getOption(row.annotations, Tool.Title)),
                readOnlyHint: Context.get(row.annotations, Tool.Readonly),
                destructiveHint: Context.get(row.annotations, Tool.Destructive),
                idempotentHint: Context.get(row.annotations, Tool.Idempotent),
                openWorldHint: Context.get(row.annotations, Tool.OpenWorld),
            },
        }),
        handle: (payload) =>
            built.handle(name, payload).pipe(
                Stream.unwrap,
                Stream.runLast,
                Effect.flatMap(Effect.fromOption),
                Effect.flatMap((reply) => rendered(reply.encodedResult, reply.isFailure || _failed(reply.result))),
                Effect.provideContext(services),
                Effect.tapCause(faulted),
                Effect.mapError(_protocolError),
                Effect.catchDefect(flow(_protocolError, Effect.fail)),
            ),
    });
});

const register = <Tools extends Record<string, Tool.Any>>(
    toolkit: Toolkit.Toolkit<Tools>,
): Layer.Layer<never, never, Tool.HandlersFor<Tools> | Tool.HandlerServices<Tools[keyof Tools]> | FileSystem.FileSystem> =>
    Layer.effectDiscard(
        Effect.gen(function* () {
            const built = yield* toolkit;
            const services = yield* Effect.context<Tool.HandlerServices<Tools[keyof Tools]> | FileSystem.FileSystem>();
            yield* Effect.forEach(Struct.keys(built.tools), (name) => _register(built, services, name), { discard: true });
        }),
    ).pipe(Layer.provide(McpServer.McpServer.layer));

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Handlers, Member, Reply, Snake };
export { answering, Failure, forward, plain, register, succeeded, tool, Value };
