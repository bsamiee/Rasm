import { Cause, Effect, Stream } from 'effect';
import { ChildProcess } from 'effect/process';

// --- [OPERATIONS] ----------------------------------------------------------------------

const run = Effect.fnUntraced(function* (command: string, args: readonly string[], env: Readonly<Record<string, string>> = {}) {
    const handle = yield* ChildProcess.make(command, args, { env, extendEnv: true, stdin: 'ignore' });
    const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? stdout : yield* new Cause.UnknownError(stderr, `${command} ${args.join(' ')} exited ${code}: ${stderr.trim()}`);
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { run };
