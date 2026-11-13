import process, { env } from 'node:process';
import { Boolean, Effect, Formatter, Layer, ManagedRuntime, Option, Predicate, pipe, Schema, Semaphore, Stream } from 'effect';
import { ChildProcess, CommandLine } from 'effect/process';
import { onExit } from 'signal-exit';

// --- [TYPES] ---------------------------------------------------------------------------

interface Run extends ChildProcess.CommandOptions {
    readonly input?: string;
    readonly echo?: { readonly stdout: NodeJS.WritableStream; readonly stderr: NodeJS.WritableStream };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLAIN = { NO_COLOR: '1', FORCE_COLOR: undefined } as const;

// --- [ERRORS] --------------------------------------------------------------------------

class ToolError extends Schema.TaggedError<ToolError, { readonly brand: unique symbol }>()('ToolError', { subject: Schema.String, detail: Schema.String, cause: Schema.optionalKey(Schema.Defect()) }) {
    static readonly of = (subject: string, detail: string) => (cause?: unknown) => (Schema.is(ToolError)(cause) ? cause : ToolError.make({ subject, detail, ...(cause === undefined ? {} : { cause }) }));

    override get message() {
        return [`${this.subject}: ${this.detail}`, ...(this.cause === undefined ? [] : [Formatter.format(this.cause)])].join(': ');
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

const alone = Semaphore.makeUnsafe(1);

// --- [OPERATIONS] ----------------------------------------------------------------------

const secluded =
    (variables: Readonly<Record<string, string>>) =>
    <A, E, R>(effect: Effect.Effect<A, E, R>) =>
        Semaphore.withPermit(
            alone,
            Effect.acquireUseRelease(
                Effect.sync(() => {
                    const before = { ...env };
                    Object.assign(env, variables);
                    return before;
                }),
                () => effect,
                (before) =>
                    Effect.sync(() => {
                        Object.keys(env)
                            .filter((key) => !(key in before))
                            .forEach((key) => Reflect.deleteProperty(env, key));
                        Object.assign(env, before);
                    }),
            ),
        );

const spawned = Effect.fnUntraced(function* (command: string, args: readonly string[], options: ChildProcess.CommandOptions = {}) {
    const handle = yield* ChildProcess.make(command, args, { extendEnv: true, stdin: 'ignore', ...options });
    const succeeded = Effect.map(
        Effect.when(handle.exitCode, Effect.map(handle.isRunning, Boolean.not)),
        Option.exists((code: number) => code === 0),
    );
    yield* Effect.acquireRelease(
        Effect.sync(() =>
            onExit(() => {
                Option.liftThrowable(process.kill)(process.platform === 'win32' ? handle.pid : -handle.pid, 'SIGTERM');
            }),
        ),
        (unhooked) => Effect.ignore(Effect.andThen(Effect.sync(unhooked), Effect.when(handle.unref, succeeded))),
    );
    return handle;
});

const ran = Effect.fnUntraced(function* (command: string, args: readonly string[], { input, echo, ...options }: Run = {}) {
    const handle = yield* spawned(command, args, { ...options, ...(echo === undefined && { env: { ...PLAIN, ...options.env } }), ...(input !== undefined && { stdin: Stream.encodeText(Stream.succeed(input)) }) });
    const read = (stream: typeof handle.stdout, sink: NodeJS.WritableStream | undefined) => Stream.mkString(sink === undefined ? Stream.decodeText(stream) : Stream.tap(Stream.decodeText(stream), (chunk) => Effect.sync(() => sink.write(chunk))));
    const [stdout, stderr, code] = yield* Effect.all([read(handle.stdout, echo?.stdout), read(handle.stderr, echo?.stderr), handle.exitCode], { concurrency: 'unbounded' });
    return { code, stdout, stderr };
}, Effect.scoped);

const printed = (command: string, args: readonly string[], options: Run = {}) =>
    Effect.flatMap(ran(command, args, options), ({ code, stdout, stderr }) =>
        code === 0
            ? Effect.succeed(stdout)
            : Effect.fail(
                  ToolError.make({
                      subject: CommandLine.display([command, ...args]),
                      detail: [`exited ${code}`, ...(options.cwd === undefined ? [] : [` in ${options.cwd}`]), ...(options.echo === undefined ? [`: ${[stdout, stderr].join('\n').trim()}`] : [])].join(''),
                  }),
              ),
    );

const present = (command: string, args: readonly string[], options: Run = {}) => Effect.map(ran(command, args, options), ({ code, stdout }) => Option.liftPredicate(stdout, () => code === 0)).pipe(Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeedNone));

// --- [COMPOSITION] ---------------------------------------------------------------------

const platform = pipe(
    Effect.promise(() => import('@effect/platform-node/NodeServices')),
    Effect.map(({ layer }) => layer),
    Layer.unwrap,
);

const runtime = ManagedRuntime.make(platform);

const messaged = <E>(error: E): E => (Predicate.isError(error) ? Object.defineProperty(error, 'message', { value: error.message }) : error);

const promised = <A, E>(effect: Effect.Effect<A, E, ManagedRuntime.ManagedRuntime.Services<typeof runtime>>) => runtime.runPromise(Effect.mapError(effect, messaged));

const provided = <R, F>(layer: Layer.Layer<R, F, ManagedRuntime.ManagedRuntime.Services<typeof runtime>>) => {
    const managed = ManagedRuntime.make(Layer.provideMerge(layer, platform));
    return <A, E>(effect: Effect.Effect<A, E, R | ManagedRuntime.ManagedRuntime.Services<typeof runtime>>) => managed.runPromise(Effect.mapError(effect, messaged));
};

const iterated =
    <Args extends readonly unknown[], A, E>(results: (...args: Args) => Stream.Stream<A, E, ManagedRuntime.ManagedRuntime.Services<typeof runtime>>) =>
    (...args: Args): AsyncIterableIterator<A> => {
        const pulled = runtime.runPromise(Effect.map(Stream.toAsyncIterableEffect(results(...args)), (iterable) => iterable[Symbol.asyncIterator]()));
        const iterator: AsyncIterableIterator<A> = { [Symbol.asyncIterator]: () => iterator, next: () => pulled.then((held) => held.next()) };
        return iterator;
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export { iterated, present, printed, promised, provided, ran, runtime, secluded, spawned, ToolError };
