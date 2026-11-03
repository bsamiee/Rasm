import { HostError } from '@rasm/automation/requests';
import { Array, Cause, Clock, Console, Context, Duration, Effect, flow, Layer, Option, Path, Predicate, Record, Result, Schema, Sink, Stdio, Stream, Struct, UndefinedOr } from 'effect';
import { RpcMessage, RpcSerialization, RpcServer } from 'effect/rpc';

// --- [CONSTANTS] -----------------------------------------------------------------------

const EVALUATED = 'evaluated.js';
const IDENTIFIER = /^[$_\p{ID_Start}](?:[$\p{ID_Continue}]|\u200C|\u200D)*$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Described = Schema.Struct({ message: Schema.Unknown });
const Script = Schema.Struct({ script: Schema.declare(Schema.is(Schema.Struct({ args: Schema.Array(Schema.String), setResult: Schema.declare(Predicate.isFunction) }))) });

// --- [SERVICES] ------------------------------------------------------------------------

class Uxp extends Context.Service<Uxp, { readonly uxp: typeof import('adobe:uxp').default; readonly fs: typeof import('adobe:fs') }>()('@rasm/uxp/runtime/Uxp') {
    static readonly Entry = Schema.Struct({ isEntry: Schema.Literal(true), url: Schema.Union([Schema.String, Schema.URL]) });
    static readonly logged = flow(Cause.pretty, Console.error);
    static readonly layer = (modules: Uxp['Service']) =>
        Layer.mergeAll(
            Layer.effect(Clock.Clock)(
                Effect.map(Clock.Clock, (base) => {
                    const monotonicTimeNanosUnsafe = (): bigint => Duration.toNanosUnsafe(performance.now());
                    const currentTimeNanosUnsafe = (): bigint => Duration.toNanosUnsafe(base.currentTimeMillisUnsafe());
                    return {
                        currentTimeMillisUnsafe: base.currentTimeMillisUnsafe.bind(base),
                        currentTimeMillis: base.currentTimeMillis,
                        sleep: base.sleep.bind(base),
                        currentTimeNanosUnsafe,
                        currentTimeNanos: Effect.sync(currentTimeNanosUnsafe),
                        monotonicTimeNanosUnsafe,
                        monotonicTimeNanos: Effect.sync(monotonicTimeNanosUnsafe),
                    };
                }),
            ),
            Layer.succeed(Uxp)(modules),
            Path.layer,
        );
    static readonly thrown = (cause: unknown): HostError => {
        const prepare = Error.prepareStackTrace;
        const lines: number[] = [];
        Error.prepareStackTrace = (error, frames) => {
            lines.push(...frames.filter((frame) => frame.getScriptNameOrSourceURL() === EVALUATED).flatMap((frame) => frame.getLineNumber() ?? []));
            return prepare?.(error, frames) ?? [String(error), ...frames.map((frame) => `    at ${frame}`)].join('\n');
        };
        try {
            const message = Option.match(Schema.decodeUnknownOption(Described)(cause), { onNone: () => String(cause), onSome: (described) => String(described.message) });
            const line = Predicate.hasProperty(cause, 'stack') && Predicate.isString(cause.stack) ? Array.head(lines) : Option.none();
            return HostError.thrown(message, line)(cause);
        } finally {
            Error.prepareStackTrace = prepare;
        }
    };
    static readonly evaluate = (scope: Record.ReadonlyRecord<string, unknown>, code: string) =>
        Effect.try({
            // biome-ignore lint/nursery/noImpliedEval: execute_script evaluates caller source in the host
            try: (): unknown => new Function(`{ ${Record.keys(Record.filter(scope, (_, key) => IDENTIFIER.test(key))).join(', ')} }`, 'return eval(arguments[1])')(scope, `${code}\n//# sourceURL=${EVALUATED}`),
            catch: Uxp.thrown,
        }).pipe(
            Effect.filterOrFail(Predicate.isFunction, (value) => new Cause.IllegalArgumentError(`code evaluates to ${typeof value}, not a body function`)),
            Effect.map((body) => Effect.try({ try: (): unknown => body(), catch: Uxp.thrown })),
        );
    static readonly restore = (target: object, saved: Record.ReadonlyRecord<string, unknown>) => Effect.validate(Record.toEntries(saved), ([key, value]) => Effect.try({ try: () => Object.assign(target, { [key]: value }), catch: HostError.refused(`restore of ${key}`) }), { discard: true }).pipe(Effect.orDie);
    static readonly preserve = <T extends object>(target: T, replacements: Partial<T>, held: readonly string[] = Struct.keys(replacements)) => {
        const refused = HostError.refused(`preferences ${held.join(', ')}`);
        return Effect.acquireRelease(Effect.try({ try: () => Record.fromIterableWith(held, (key): readonly [string, unknown] => [key, Reflect.get(target, key)]), catch: refused }), (saved) => Uxp.restore(target, saved)).pipe(
            Effect.tap(() => Effect.try({ try: () => Object.assign(target, replacements), catch: refused })),
        );
    };
    static readonly members = (prototype: object) => {
        const declared = (each: object) => {
            const descriptors = Record.toEntries(Object.getOwnPropertyDescriptors(each));
            return {
                name: each.constructor.name,
                fields: Array.filterMap(descriptors, ([name, { get, set }]) => (get === undefined ? Result.failVoid : Result.succeed({ name, settable: set !== undefined }))),
                methods: Array.filterMap(descriptors, ([name, { value }]) => (Predicate.isFunction(value) && name !== 'constructor' ? Result.succeed(name) : Result.failVoid)),
            };
        };
        const above = (each: unknown) => Option.liftPredicate(each, (parent): parent is object => Predicate.isObject(parent) && parent !== Object.prototype).pipe(Option.map((parent) => [declared(parent), Object.getPrototypeOf(parent)] as const));
        return Array.prepend(Array.unfold(Object.getPrototypeOf(prototype), above), declared(prototype));
    };
    static readonly script = <E>(modules: Uxp['Service'], server: Layer.Layer<never, E, RpcServer.Protocol | Uxp>): void => {
        const decoder = new TextDecoder();
        Effect.gen(function* () {
            const [{ script }, serialization] = yield* Effect.all([Schema.decodeUnknownEffect(Script)(modules.uxp), RpcSerialization.RpcSerialization]);
            const answered = (chunk: string | Uint8Array) => Effect.sync(() => script.setResult(Predicate.isString(chunk) ? chunk : decoder.decode(chunk)));
            const stdio = Layer.succeed(Stdio.Stdio)(Stdio.make({ args: Effect.succeed(script.args), stdin: Stream.encodeText(Stream.fromIterable(script.args)), stdout: () => Sink.forEach(answered), stderr: () => Sink.forEach(Console.error) }));
            return yield* Layer.launch(server).pipe(
                Effect.provide(RpcServer.layerProtocolStdio.pipe(Layer.provide([stdio, RpcSerialization.layerJson]))),
                Effect.catchCauseIf(
                    Predicate.not(Cause.hasInterruptsOnly),
                    Effect.fnUntraced(function* (cause: Cause.Cause<unknown>) {
                        yield* Uxp.logged(cause);
                        const defect = yield* Effect.orDie(Schema.encodeEffect(Schema.Defect())(Cause.squash(cause)));
                        return yield* UndefinedOr.match(serialization.makeUnsafe().encode(RpcMessage.ResponseDefectEncoded(defect)), { onUndefined: () => Effect.void, onDefined: answered });
                    }),
                ),
            );
        }).pipe(Effect.provide([Uxp.layer(modules), RpcSerialization.layerJson]), Effect.tapCause(Uxp.logged), Effect.runSyncExit);
    };
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Uxp };
