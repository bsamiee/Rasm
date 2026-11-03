import { HostError, Requests } from '@rasm/automation/requests';
import { Unmatched } from '@rasm/search/unmatched';
import { Link } from '@rasm/uxp/link';
import { Array, Cause, Console, Effect, flow, Layer, Match, Option, Predicate, Schema, SchemaGetter } from 'effect';
import type { Rpc, RpcGroup } from 'effect/rpc';

// --- [TYPES] ---------------------------------------------------------------------------

declare global {
    interface ImportMetaEnv {
        readonly LINK_ORIGIN: string;
    }
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const UNEVALUATED = 'EvalScript error.';

// --- [MODELS] --------------------------------------------------------------------------

const Cep = Schema.Struct({ evalScript: Schema.declare(Predicate.isFunction), getSystemPath: Schema.declare(Predicate.isFunction), getExtensionId: Schema.declare(Predicate.isFunction) });
const Panel = Schema.Struct({ __adobe_cep__: Schema.declare(Schema.is(Cep)) });
const Labeled = Schema.Struct({ label: Schema.String });
const Reply = Schema.Union([
    Schema.Literal(UNEVALUATED).pipe(Schema.decodeTo(Schema.toEncoded(Schema.TaggedStruct('Unevaluated', {})), { decode: SchemaGetter.succeed({ _tag: 'Unevaluated' as const }), encode: SchemaGetter.forbiddenEncoding })),
    Schema.fromJsonString(
        Schema.Union([
            Schema.TaggedStruct('Value', { value: Schema.optionalKey(Schema.Json) }),
            Schema.toCodecJson(Unmatched),
            Schema.TaggedStruct('IllegalArgument', { message: Schema.String }),
            Schema.TaggedStruct('Thrown', { message: Schema.String, number: Schema.OptionFromOptionalKey(Schema.Finite), line: Schema.OptionFromOptionalKey(Schema.Int) }),
        ]),
    ),
]);

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const { __adobe_cep__: cep } = yield* Schema.decodeUnknownEffect(Panel)(globalThis);
    const evaluated = (script: string) =>
        Effect.callback<unknown>((resume) => {
            cep.evalScript.call(cep, script, flow(Effect.succeed, resume));
        });
    const served =
        ({ _tag: tag, payloadSchema, successSchema }: RpcGroup.Rpcs<typeof Requests>) =>
        (payload: unknown) =>
            Schema.encodeUnknownEffect(Schema.fromJsonString(Schema.Struct({ tag: Schema.Literal(tag), label: Schema.String, payload: Schema.toCodecJson(payloadSchema) })))({
                tag,
                label: Option.match(Schema.decodeUnknownOption(Labeled)(payload), { onNone: () => tag, onSome: ({ label }) => label }),
                payload,
            }).pipe(
                Effect.flatMap((request) => evaluated(`handle(${request.replaceAll('\u2028', '\\u2028').replaceAll('\u2029', '\\u2029')})`)),
                Effect.flatMap(Schema.decodeUnknownEffect(Reply)),
                Effect.flatMap(
                    Match.valueTags({
                        Unevaluated: () => Effect.fail(HostError.make({ reason: { _tag: 'Unavailable', because: UNEVALUATED }, subject: tag })),
                        Value: ({ value }) => Schema.decodeUnknownEffect(Schema.toCodecJson(successSchema))(value),
                        Unmatched: Effect.fail,
                        IllegalArgument: ({ message }) => Effect.fail(new Cause.IllegalArgumentError(message)),
                        Thrown: (failed) => Effect.fail(HostError.thrown([tag, ...Option.toArray(Option.map(failed.number, (code) => `Error ${code}`)), failed.message].join(': '), failed.line)(failed)),
                    }),
                ),
            );
    const handlers = Object.fromEntries(Array.map(Array.fromIterable(Requests.requests.values()), (rpc) => [rpc._tag, served(rpc)]));
    const [extension, application] = yield* Effect.all([Schema.decodeUnknownEffect(Schema.String)(cep.getExtensionId.call(cep)), Schema.decodeUnknownEffect(Schema.URLFromString)(cep.getSystemPath.call(cep, 'hostApplication'))]);
    yield* Effect.filterOrFail(
        evaluated('typeof handle'),
        (reply) => reply === 'function',
        (reply) => HostError.make({ reason: { _tag: 'Unavailable', because: `typeof handle answered ${String(reply)}` }, subject: extension }),
    );
    // biome-ignore lint/nursery/noUnsafeTypeAssertion: each handler encodes its tag's payload and decodes its tag's success through that rpc's schemas, which a table built from the request map types as their union
    const layer = Requests.toLayer(handlers as unknown as { readonly [Current in RpcGroup.Rpcs<typeof Requests> as Current['_tag']]: Rpc.ToHandlerFn<Current, never> });
    return yield* Layer.launch(Link.dial(Requests, layer, { origin: import.meta.env.LINK_ORIGIN, extension, application: Effect.succeed(application) }));
}).pipe(Effect.tapCause(flow(Cause.pretty, Console.error)), Effect.runFork);
