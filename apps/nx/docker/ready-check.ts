import { layer } from '@effect/platform-node/NodeServices';
import { runReadyCheck } from '@nx/devkit/internal';
import { Array, Effect, Formatter, Schedule, Schema, Stream, String } from 'effect';
import { ChildProcess, CommandLine } from 'effect/process';

// --- [MODELS] --------------------------------------------------------------------------

const Running = Schema.Array(Schema.fromJsonString(Schema.Struct({ Service: Schema.String, State: Schema.Literal('running'), Health: Schema.Literals(['', 'healthy']) }))).annotate({
    description: 'The lines of docker compose ps --format json where every listed container runs and passes its healthcheck or states none',
});
const Reason = Schema.TaggedUnion({ Exited: { code: Schema.Int, stderr: Schema.String }, Refused: { detail: Schema.String } });

// --- [ERRORS] --------------------------------------------------------------------------

class DockerError extends Schema.TaggedError<DockerError, { readonly brand: unique symbol }>()('DockerError', { reason: Reason, subject: Schema.String, cause: Schema.optionalKey(Schema.Defect()) }) {
    static readonly of = (reason: DockerError['reason'], subject: string) => (cause?: unknown) => DockerError.make({ reason, subject, ...(cause === undefined ? {} : { cause }) });

    override get message() {
        const said = Reason.match(this.reason, { Exited: ({ code, stderr }) => [`exits ${code}`, ...(stderr.trim() === '' ? [] : [stderr.trim()])].join(': '), Refused: ({ detail }) => detail });
        return [`${this.subject}: ${said}`, ...(this.cause === undefined ? [] : [Formatter.format(this.cause)])].join(': ');
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const printed = Effect.fnUntraced(function* (argv: Array.NonEmptyReadonlyArray<string>, options: Pick<ChildProcess.CommandOptions, 'cwd' | 'env'>) {
    const handle = yield* ChildProcess.make(Array.headNonEmpty(argv), Array.tailNonEmpty(argv), { ...options, extendEnv: true, detached: false, stdin: 'ignore' });
    const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? stdout : yield* DockerError.of({ _tag: 'Exited', code, stderr }, CommandLine.display(argv))();
}, Effect.scoped);

const waitForDevServer = (...services: readonly string[]) =>
    runReadyCheck(({ root }) =>
        printed(['docker', 'compose', 'ps', '--format', 'json', ...services], { cwd: root }).pipe(
            Effect.flatMap((stdout) => Schema.decodeEffect(Running)(stdout.split('\n').filter(String.isNonEmpty))),
            Effect.map((running) =>
                Array.difference(
                    services,
                    Array.map(running, ({ Service }) => Service),
                ),
            ),
            Effect.filterOrFail(Array.isReadonlyArrayEmpty, (absent) => DockerError.of({ _tag: 'Refused', detail: `lists no running container of ${absent.join(', ')}` }, root)()),
            Effect.retry(Schedule.spaced('250 millis')),
            Effect.as(null),
            Effect.provide(layer),
            Effect.runPromise,
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { DockerError, printed, waitForDevServer };
