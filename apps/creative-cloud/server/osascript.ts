// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, type Crypto, Duration, Effect, Option, type Ref, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/unstable/process';
import { type BridgeError, classify, type Probe } from './errors.ts';
import { type Activity, probe as probed } from './jobs.ts';
import { type HostId, PROBE_MS } from './values.ts';

// --- [TEMPLATE] ------------------------------------------------------------------------

const _literal = (text: string): string => `"${text.replaceAll('\\', '\\\\').replaceAll('"', '\\"')}"`;

const doScript = (javascript: string): string => `do script ${_literal(javascript)}`;

// --- [BOUNDARY] ------------------------------------------------------------------------

const reply: (host: HostId, command: ChildProcess.Command) => Effect.Effect<string, BridgeError, ChildProcessSpawner.ChildProcessSpawner> = Effect.fnUntraced(
    function* (host: HostId, command: ChildProcess.Command) {
        const spawner = yield* ChildProcessSpawner.ChildProcessSpawner;
        const handle = yield* spawner.spawn(command);
        const [stdout, stderr, exitCode] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], {
            concurrency: 'unbounded',
        });
        return classify(host, { exitCode, stdout, stderr });
    },
    Effect.scoped,
    Effect.orDie,
    Effect.flatMap(Effect.fromResult),
);

const read = (host: HostId, bundleId: string, timeoutMs: number, statement: string, file: Option.Option<string>): Effect.Effect<string, BridgeError, ChildProcessSpawner.ChildProcessSpawner> => {
    const application = `application id ${_literal(bundleId)}`;
    const script = [
        `if not running of ${application} then error number -600`,
        ...Array.fromOption(Option.map(file, (path) => `set f to POSIX file ${_literal(path)}`)),
        `with timeout of ${Math.ceil(Duration.toSeconds(Duration.millis(timeoutMs)))} seconds`,
        `tell ${application}`,
        statement,
        'end tell',
        'end timeout',
    ];
    return reply(host, ChildProcess.make('osascript', ['-'], { stdin: Stream.encodeText(Stream.make(script.join('\n'))) }));
};

const probe = (host: HostId, bundleId: string, jobs: Ref.Ref<Activity>): Effect.Effect<Probe, never, ChildProcessSpawner.ChildProcessSpawner | Crypto.Crypto> =>
    probed(host, jobs, () => read(host, bundleId, PROBE_MS, 'get version', Option.none()));

// --- [EXPORTS] -------------------------------------------------------------------------

export { doScript, probe, read, reply };
