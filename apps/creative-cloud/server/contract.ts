// --- [IMPORTS] -------------------------------------------------------------------------

import { type Context, Effect, Layer, Schema } from 'effect';
import { McpServer, Tool, Toolkit } from 'effect/unstable/ai';
import { BridgeError } from './errors.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Services<Dependencies extends readonly Context.Key<unknown, unknown>[]> = Context.Service.Identifier<Dependencies[number]>;

type Row<Channel, Dependencies extends readonly Context.Key<unknown, unknown>[]> = <Input, Output>(
    name: string,
    description: string,
    parameters: Schema.Codec<Input, unknown, never, never>,
    member: Schema.Codec<Output, unknown, never, never>,
    readOnly: boolean,
    run: (channel: Channel, input: Input) => Effect.Effect<Output, BridgeError, Services<Dependencies>>,
) => Layer.Layer<never, never, Channel | Services<Dependencies>>;

// --- [MODELS] --------------------------------------------------------------------------

const Failure: Schema.Struct<{ readonly kind: Schema.Literal<'error'>; readonly error: typeof BridgeError }> = Schema.Struct({ kind: Schema.Literal('error'), error: BridgeError });

// --- [CONTRACT] ------------------------------------------------------------------------

const _handle = <Channel, Input, Output, Requirements>(
    run: (channel: Channel, input: Input) => Effect.Effect<Output, BridgeError, Requirements>,
    state: Channel,
    input: Input,
): Effect.Effect<{ readonly result: Output | (typeof Failure)['Type'] }, never, Requirements> =>
    Effect.match(run(state, input), { onSuccess: (value) => ({ result: value }), onFailure: (error) => ({ result: { kind: 'error' as const, error } }) });

const contract =
    <Channel, Dependencies extends readonly Context.Key<unknown, unknown>[]>(channel: Context.Service<Channel, Channel>, dependencies: Dependencies): Row<Channel, Dependencies> =>
    (name, description, parameters, member, readOnly, run): Layer.Layer<never, never, Channel | Services<Dependencies>> => {
        const toolkit = Toolkit.make(
            Tool.make(name, { description, parameters, success: Schema.Struct({ result: Schema.Union([member, Failure]) }), dependencies: [...dependencies] }).annotate(Tool.Readonly, readOnly),
        );
        return Layer.provide(McpServer.toolkit(toolkit), toolkit.toLayer(Effect.map(channel, (state) => toolkit.of({ [name]: (input) => _handle(run, state, input) }))));
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export { contract, Failure };
