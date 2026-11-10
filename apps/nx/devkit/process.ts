import process from 'node:process';
import { Effect, Formatter, Option, Schema, Stream } from 'effect';
import { ChildProcess } from 'effect/process';

// --- [TYPES] ---------------------------------------------------------------------------

interface Run extends ChildProcess.CommandOptions {
    readonly input?: string;
    readonly echo?: { readonly stdout: NodeJS.WritableStream; readonly stderr: NodeJS.WritableStream };
}

// --- [ERRORS] --------------------------------------------------------------------------

class ToolError extends Schema.TaggedError<ToolError, { readonly brand: unique symbol }>()('ToolError', { subject: Schema.String, detail: Schema.String, cause: Schema.optionalKey(Schema.Defect()) }) {
    static readonly of = (subject: string, detail: string) => (cause?: unknown) => (Schema.is(ToolError)(cause) ? cause : ToolError.make({ subject, detail, ...(cause === undefined ? {} : { cause }) }));

    override get message() {
        return [`${this.subject}: ${this.detail}`, ...(this.cause === undefined ? [] : [Formatter.format(this.cause)])].join(': ');
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const children = new Set<number>();

const ended = () => children.forEach((pid) => Option.liftThrowable(process.kill)(pid, 'SIGTERM'));

const spawned = (command: string, args: readonly string[], options: ChildProcess.CommandOptions = {}) =>
    Effect.tap(ChildProcess.make(command, args, { extendEnv: true, stdin: 'ignore', ...options, detached: false }), ({ pid }) =>
        Effect.acquireRelease(
            Effect.sync(() => {
                if (children.size === 0) {
                    process.once('exit', ended);
                }
                children.add(pid);
            }),
            () =>
                Effect.sync(() => {
                    children.delete(pid);
                    if (children.size === 0) {
                        process.off('exit', ended);
                    }
                }),
        ),
    );

const ran = Effect.fnUntraced(function* (command: string, args: readonly string[], { input, echo, ...options }: Run = {}) {
    const handle = yield* spawned(command, args, input === undefined ? options : { ...options, stdin: Stream.encodeText(Stream.succeed(input)) });
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
                      subject: [command, ...args].join(' '),
                      detail: [`exited ${code}`, ...(options.cwd === undefined ? [] : [` in ${options.cwd}`]), ...(options.echo === undefined ? [`: ${[stdout, stderr].join('\n').trim()}`] : [])].join(''),
                  }),
              ),
    );

const present = (command: string, args: readonly string[]) => Effect.map(ran(command, args), ({ code }) => code === 0).pipe(Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(false)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { present, printed, type Run, ran, spawned, ToolError };
