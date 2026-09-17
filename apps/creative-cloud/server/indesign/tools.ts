// --- [IMPORTS] -------------------------------------------------------------------------

import { Context, Crypto, Duration, Effect, Layer, Option, Schema } from 'effect';
import { contract, Failure } from '../contract.ts';
import { Execute } from '../frames.ts';
import { Jobs, run } from '../jobs.ts';
import { dispatch, type Endpoint, Links } from '../socket.ts';
import { TIMEOUT_MS, TimeoutMs, Undo } from '../values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Channel {
    readonly link: Endpoint;
    readonly host: Jobs['indesign'];
}

// --- [MODELS] --------------------------------------------------------------------------

const Channel: Context.Service<Channel, Channel> = Context.Service<Channel>('InDesignChannel');

const Reply = Schema.Union([Schema.Struct({ kind: Schema.Literal('value'), value: Schema.Json, autocorrections: Schema.Array(Schema.String), undo: Undo, tookMs: Schema.Number }), Failure]).pipe(
    Schema.toTaggedUnion('kind'),
);

const _row = contract(Channel, [Crypto.Crypto]);

// --- [LAYER] ---------------------------------------------------------------------------

const layer: Layer.Layer<never, never, Links | Jobs | Crypto.Crypto> = Layer.provide(
    _row(
        'indesign_execute',
        'Runs `code` as a function body in InDesign UXP and returns its value as JSON. With `undoName` the body is synchronous and runs as one undo step, without it the body can `await`',
        Schema.Struct({ code: Schema.String, undoName: Schema.OptionFromOptionalKey(Schema.String), timeoutMs: Schema.OptionFromOptionalKey(TimeoutMs) }),
        Reply.cases.value,
        false,
        (channel, { code, undoName, timeoutMs }) =>
            Effect.map(
                Effect.timed(
                    run(
                        channel.host,
                        Option.getOrElse(timeoutMs, () => TIMEOUT_MS),
                        (jobId) =>
                            dispatch(channel.link, {
                                jobId,
                                kind: 'execute',
                                body: Schema.encodeSync(Schema.toCodecJson(Execute))({ code, undoName }),
                                suspendHistory: Option.none(),
                                commandName: Option.none(),
                            }),
                    ),
                ),
                ([took, done]) => ({
                    kind: 'value' as const,
                    value: done.value,
                    autocorrections: Option.getOrElse(done.autocorrections, () => []),
                    undo: Option.match(undoName, { onNone: () => 'none' as const, onSome: () => 'single' as const }),
                    tookMs: Duration.toMillis(took),
                }),
            ),
    ),
    Layer.effect(
        Channel,
        Effect.map(Effect.all([Links, Jobs]), ([links, jobs]) => ({ link: links.indesign, host: jobs.indesign })),
    ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { layer };
