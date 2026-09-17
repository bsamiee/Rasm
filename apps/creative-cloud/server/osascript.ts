// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Duration, Effect, Option, Stream, String } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/unstable/process';
import { type BridgeError, classify, NonZeroExit } from './errors.ts';
import type { HostId } from './values.ts';

// --- [TEMPLATE] ------------------------------------------------------------------------

const literal = (text: string): string => `"${text.replaceAll('\\', '\\\\').replaceAll('"', '\\"')}"`;

const doScript = (javascript: string): string => `do script ${literal(javascript)}`;

const doJavascript = (javascript: string): string => `do javascript ${literal(javascript)}`;

// --- [BOUNDARY] ------------------------------------------------------------------------

const reply: (command: ChildProcess.StandardCommand) => Effect.Effect<string, NonZeroExit, ChildProcessSpawner.ChildProcessSpawner> = Effect.fnUntraced(
    function* (command: ChildProcess.StandardCommand) {
        const spawner = yield* ChildProcessSpawner.ChildProcessSpawner;
        const handle = yield* spawner.spawn(command);
        const [stdout, stderr, exitCode] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], {
            concurrency: 'unbounded',
        });
        return { exitCode, stdout, stderr };
    },
    Effect.scoped,
    Effect.orDie,
    Effect.filterOrFail(
        (output) => output.exitCode === 0,
        (output) => NonZeroExit.make({ exitCode: output.exitCode, stderr: output.stderr }),
    ),
    Effect.map((output) => String.trim(output.stdout)),
);

const read = (host: HostId, bundleId: string, timeoutMs: number, statement: string, file: Option.Option<string>): Effect.Effect<string, BridgeError, ChildProcessSpawner.ChildProcessSpawner> => {
    const application = `application id ${literal(bundleId)}`;
    const script = [
        `if not running of ${application} then error number -600`,
        ...Array.fromOption(Option.map(file, (path) => `set f to POSIX file ${literal(path)}`)),
        `with timeout of ${Math.ceil(Duration.toSeconds(Duration.millis(timeoutMs)))} seconds`,
        `tell ${application}`,
        statement,
        'end tell',
        'end timeout',
    ];
    return Effect.mapError(reply(ChildProcess.make('osascript', ['-'], { stdin: Stream.encodeText(Stream.make(script.join('\n'))) })), (exit) => classify(host, exit));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { doJavascript, doScript, literal, read, reply };
