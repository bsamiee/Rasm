import { Completed } from '@rasm/process';
import { Array, Effect, FileSystem, Layer, MutableRef, Option, Path, PlatformError, Record, Schema, Sink, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner, CommandLine } from 'effect/process';
import { hashWithWorkspaceContext } from 'nx/src/devkit-internals';
import { TestRunner } from 'vitest';

// --- [TYPES] ---------------------------------------------------------------------------

interface Script {
    readonly root: string;
    readonly faked: readonly string[];
    readonly missing: readonly string[];
    readonly replies: typeof Recording.Type;
    readonly record: boolean;
    readonly renew: boolean;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = '<root>';
const OUTSIDE = /(?<=^|\s)\/(?:[^\s/]+\/)+/gu;

// --- [MODELS] --------------------------------------------------------------------------

const Reply = Schema.Struct({ code: Schema.Int, stdout: Schema.String, stderr: Schema.String });
const Recording = Schema.fromJsonString(Schema.Record(Schema.String, Reply), { space: 4 }).annotate({
    description: 'Exit code and output each tool run printed, keyed by `<workspace hash> <folder>$ <command line>`: the hash Nx gives the workspace files, the folder relative to the workspace root, that root written as <root>, and a path outside it by its file name',
});

// --- [SERVICES] ------------------------------------------------------------------------

const script = MutableRef.make(Option.none<Script>());
const runs = MutableRef.make<readonly string[]>([]);
const taken = MutableRef.make<typeof Recording.Type>({});
const unrecorded = MutableRef.make<readonly string[]>([]);

const standard = (command: ChildProcess.Command): ChildProcess.StandardCommand => (ChildProcess.isStandardCommand(command) ? command : standard(command.left));

const absent = (tool: string) => PlatformError.systemError({ _tag: 'NotFound', module: 'ChildProcess', method: 'spawn', pathOrDescriptor: tool });

const recorded = Effect.fnUntraced(function* (real: ChildProcessSpawner.ChildProcessSpawner['Service'], command: ChildProcess.Command, root: string, key: string) {
    const { code, stdout, stderr } = yield* Effect.provideService(Completed.run(command), ChildProcessSpawner.ChildProcessSpawner, real);
    const reply = { code, stdout: stdout.replaceAll(root, ROOT), stderr: stderr.replaceAll(root, ROOT) };
    MutableRef.update(taken, Record.set(key, reply));
    return reply;
});

const replied = ({ code, stdout, stderr }: typeof Reply.Type, root: string) => {
    const printed = (output: string) => Stream.encodeText(Stream.succeed(output.replaceAll(ROOT, root)));
    return ChildProcessSpawner.makeHandle({
        pid: ChildProcessSpawner.ProcessId(0),
        stdin: Sink.drain,
        stdout: printed(stdout),
        stderr: printed(stderr),
        all: printed(`${stdout}${stderr}`),
        exitCode: Effect.succeed(ChildProcessSpawner.ExitCode(code)),
        isRunning: Effect.succeed(false),
        kill: () => Effect.void,
        getInputFd: () => Sink.drain,
        getOutputFd: () => Stream.empty,
        unref: Effect.succeed(Effect.void),
    });
};

const answered = Effect.fnUntraced(function* (real: ChildProcessSpawner.ChildProcessSpawner['Service'], path: Path.Path, command: ChildProcess.Command, { root, missing, replies, record, renew }: Script) {
    const { command: called, args, options } = standard(command);
    const tool = path.basename(called);
    const state = yield* Effect.promise(() => hashWithWorkspaceContext(root, ['**']));
    const key = `${state} ${path.relative(root, options.cwd ?? root) || '.'}$ ${CommandLine.display([called, ...args])
        .replaceAll(root, ROOT)
        .replaceAll(OUTSIDE, '')}`;
    const unread = Effect.andThen(
        Effect.sync(() => MutableRef.update(unrecorded, Array.append(key))),
        Effect.fail(absent(tool)),
    );
    const fresh = record ? recorded(real, command, root, key) : unread;
    const held = renew ? Option.none() : Record.get(replies, key);
    return replied(yield* missing.includes(tool) ? Effect.fail(absent(tool)) : Option.match(held, { onSome: Effect.succeed, onNone: () => fresh }), root);
});

const layer = <E, R>(services: Layer.Layer<ChildProcessSpawner.ChildProcessSpawner | Path.Path, E, R>) =>
    Layer.effect(
        ChildProcessSpawner.ChildProcessSpawner,
        Effect.gen(function* () {
            const [real, path] = yield* Effect.all([ChildProcessSpawner.ChildProcessSpawner, Path.Path]);
            return ChildProcessSpawner.make((command) => {
                const { command: called, args } = standard(command);
                MutableRef.update(runs, Array.append(CommandLine.display([called, ...args])));
                return Option.match(
                    Option.filter(MutableRef.get(script), ({ faked, missing }) => [...faked, ...missing].includes(path.basename(called))),
                    { onNone: () => real.spawn(command), onSome: (stated) => answered(real, path, command, stated) },
                );
            });
        }),
    ).pipe(Layer.provideMerge(services));

const replayed = Effect.fnUntraced(function* (recording: string, root: string, faked: readonly string[], missing: readonly string[] = []) {
    const fs = yield* FileSystem.FileSystem;
    const replies = yield* fs.readFileString(recording).pipe(
        Effect.flatMap(Schema.decodeEffect(Recording)),
        Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed({})),
    );
    const update = Option.map(Option.fromNullishOr(TestRunner.getCurrentTest()), ({ context }) => context.expect.getState().snapshotState.snapshotUpdateState);
    yield* Effect.acquireRelease(
        Effect.sync(() => {
            MutableRef.set(script, Option.some({ root, faked, missing, replies, record: Option.exists(update, (state) => state !== 'none'), renew: Option.contains(update, 'all') }));
            MutableRef.set(taken, {});
            MutableRef.set(unrecorded, []);
        }),
        Effect.fnUntraced(function* () {
            MutableRef.set(script, Option.none());
            const merged = Record.fromEntries(Record.toEntries({ ...replies, ...MutableRef.get(taken) }).toSorted(([left], [right]) => left.localeCompare(right)));
            yield* Record.isEmptyRecord(MutableRef.get(taken)) ? Effect.void : Effect.flatMap(Schema.encodeEffect(Recording)(merged), (text) => fs.writeFileString(recording, `${text}\n`));
            return yield* Array.match(MutableRef.get(unrecorded), {
                onEmpty: () => Effect.void,
                onNonEmpty: (keys) => Effect.die(new Error(`${recording} holds no reply for ${keys.join(', ')}, run vitest with --update where each tool is installed to record them`)),
            });
        }, Effect.orDie),
    );
});

const spawned = Effect.fnUntraced(function* <A, E, R>(effect: Effect.Effect<A, E, R>) {
    const before = MutableRef.get(runs).length;
    const value = yield* effect;
    return [value, MutableRef.get(runs).slice(before)] as const;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { layer, replayed, spawned };
