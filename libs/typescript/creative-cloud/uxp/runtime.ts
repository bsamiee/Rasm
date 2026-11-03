import { HostError } from '@rasm/automation/requests';
import { Array, Cause, Clock, Console, Context, Deferred, Duration, Effect, Exit, flow, Layer, Option, Path, Predicate, Record, Result, Schema, Sink, Stdio, Stream, Struct } from 'effect';
import { RpcMessage, RpcSerialization, RpcServer } from 'effect/rpc';

// --- [CONSTANTS] -----------------------------------------------------------------------

const EVALUATED = 'evaluated.js';
const IDENTIFIER = /^[$_\p{ID_Start}](?:[$\p{ID_Continue}]|\u200C|\u200D)*$/u;

// --- [MODELS] --------------------------------------------------------------------------

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
            const line = Predicate.hasProperty(cause, 'stack') && Predicate.isString(cause.stack) ? Array.head(lines) : Option.none();
            return HostError.thrown(String(Predicate.hasProperty(cause, 'message') ? cause.message : cause), line)(cause);
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
        const above = (each: unknown) => (Predicate.isObject(each) && each !== Object.prototype ? Option.some([declared(each), Object.getPrototypeOf(each)] as const) : Option.none());
        return Array.prepend(Array.unfold(Object.getPrototypeOf(prototype), above), declared(prototype));
    };
    static readonly script = <E>(modules: Uxp['Service'], server: Layer.Layer<never, E, RpcServer.Protocol | Uxp>): void => {
        const decoder = new TextDecoder();
        Effect.gen(function* () {
            const [{ script }, context, done] = yield* Effect.all([Schema.decodeUnknownEffect(Script)(modules.uxp), Effect.context<Uxp>(), Deferred.make<void>()]);
            const answered = (chunk: string | Uint8Array) => Effect.andThen(Effect.sync(() => script.setResult(Predicate.isString(chunk) ? chunk : decoder.decode(chunk))), Deferred.succeed(done, undefined));
            const failed = Effect.fnUntraced(function* (cause: Cause.Cause<unknown>) {
                yield* Uxp.logged(cause);
                const defect = yield* Effect.orDie(Schema.encodeEffect(Schema.Defect())(Cause.squash(cause)));
                const encoded = RpcSerialization.json.makeUnsafe().encode(RpcMessage.ResponseDefectEncoded(defect));
                return yield* encoded === undefined ? Effect.void : answered(encoded);
            });
            const stdio = Layer.succeed(Stdio.Stdio)(Stdio.make({ args: Effect.succeed(script.args), stdin: Stream.encodeText(Stream.fromIterable(script.args)), stdout: () => Sink.forEach(answered), stderr: () => Sink.forEach(Console.error) }));
            const exit = Effect.runSyncExitWith(context)(Layer.launch(server).pipe(Effect.provide(RpcServer.layerProtocolStdio.pipe(Layer.provide([stdio, RpcSerialization.layerJson]))), Effect.catchCauseIf(Predicate.not(Cause.hasInterruptsOnly), failed)));
            const pending = Option.filter(Result.getSuccess(Exit.findDefect(exit)), Cause.isAsyncFiberError);
            yield* Effect.sync(() => Option.getOrUndefined(pending)?.fiber.interruptUnsafe());
            return yield* Option.isSome(pending) && !(yield* Deferred.isDone(done)) ? failed(Cause.die(pending.value)) : Effect.void;
        }).pipe(Effect.provide(Uxp.layer(modules)), Effect.tapCause(Uxp.logged), Effect.runSyncExit);
    };
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Uxp };
