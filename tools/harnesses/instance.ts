import { tmpdir } from 'node:os';
import process from 'node:process';
import { fileURLToPath } from 'node:url';
import { State } from '@rasm/automation/applications';
import { Array, Effect, FileSystem, Option, Path, Record, Schema, Stream } from 'effect';
import { ChildProcess } from 'effect/process';
import { HarnessError } from '#/error';
import type { Crash } from '#/results';

// --- [TYPES] ---------------------------------------------------------------------------

type Exited = Extract<typeof State.Type, { readonly _tag: 'Exited' }>;
interface Launch {
    readonly app: string;
    readonly fresh?: boolean;
    readonly documents?: readonly string[];
    readonly arguments?: readonly string[];
    readonly environment?: Record.ReadonlyRecord<string, string>;
}

// --- [MODELS] --------------------------------------------------------------------------

const Instances = Schema.Record(Schema.String, Schema.Record(Schema.String, State));
const Failure = Schema.Record(Schema.String, Schema.Struct({ fail: Schema.Struct({ reason: Schema.String }) }));
const Line = Schema.fromJsonString(Schema.toCodecJson(Schema.Union([Instances, Failure])));
const Ips = Schema.fromJsonString(
    Schema.Struct({
        exception: Schema.optionalKey(Schema.Struct({ type: Schema.String, signal: Schema.optionalKey(Schema.String) })),
        faultingThread: Schema.optionalKey(Schema.Int),
        threads: Schema.optionalKey(Schema.Array(Schema.Struct({ frames: Schema.Array(Schema.Struct({ imageIndex: Schema.Int, imageOffset: Schema.Int, symbol: Schema.optionalKey(Schema.String) })) }))),
        usedImages: Schema.optionalKey(Schema.Array(Schema.Struct({ name: Schema.optionalKey(Schema.String) }))),
    }),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const decoded = (text: string): Effect.Effect<readonly { readonly identifier: string; readonly pid: number; readonly state: typeof State.Type }[], HarnessError> =>
    Option.match(Schema.decodeOption(Line)(text), {
        onNone: () => Effect.succeed([]),
        onSome: (line) =>
            Schema.is(Failure)(line)
                ? Effect.flatMap(
                      Effect.fromOption(Array.head(Record.toEntries(line)), () => HarnessError.make({ reason: { _tag: 'Unavailable', because: text }, subject: 'watch-apps' })),
                      ([key, { fail }]) => HarnessError.make({ reason: { _tag: 'Unavailable', because: fail.reason }, subject: key }),
                  )
                : Effect.succeed(Record.collect(line, (identifier, instances) => Record.collect(instances, (pid, state) => ({ identifier, pid: Number(pid), state }))).flat()),
    });
const spawned = Effect.fn('spawned')(function* (command: string, args: readonly string[]) {
    const handle = yield* ChildProcess.make(command, args, { stdin: 'ignore', stderr: 'inherit' });
    const [lines, code] = yield* Effect.all([Stream.runCollect(Stream.splitLines(Stream.decodeText(handle.stdout))), handle.exitCode], { concurrency: 'unbounded' });
    return { lines, code };
}, Effect.scoped);
const launch = Effect.fn('launch')(function* (spec: Launch) {
    const flags = [...(spec.fresh === true ? ['--new'] : []), ...(spec.documents ?? []).flatMap((file) => ['--document', file]), ...(spec.arguments ?? []).flatMap((value) => ['--arguments', value]), ...Record.collect(spec.environment ?? {}, (name, value) => ['--environment', `${name}=${value}`]).flat()];
    const { lines, code } = yield* spawned('watch-apps', ['launch', spec.app, '--hides', ...flags]);
    const states = (yield* Effect.forEach(lines, decoded)).flat();
    const last = Array.last(states.filter(({ state }) => !State.isAnyOf(['Exited'])(state)));
    return yield* Option.match(
        Option.filter(last, () => code === 0),
        {
            onNone: () => HarnessError.make({ reason: { _tag: 'Unavailable', because: lines.join('\n') }, subject: spec.app }),
            onSome: ({ pid, identifier }) => Effect.succeed({ pid, identifier, launched: states.some(({ state }) => State.isAnyOf(['Launching'])(state)) }),
        },
    );
});
const lock = Effect.fn('lock')(function* (identifier: string, pid: number) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folder = path.join(tmpdir(), 'rasm-harnesses');
    const file = path.join(folder, `${identifier}.${pid}.lock`);
    yield* fs.makeDirectory(folder, { recursive: true });
    const acquired = Effect.gen(function* () {
        const { code } = yield* spawned('shlock', ['-f', file, '-p', `${process.pid}`]);
        yield* code === 0 ? Effect.void : Effect.flatMap(fs.readFileString(file), (holder) => HarnessError.make({ reason: { _tag: 'Unavailable', because: `held by harness process ${holder.trim()}` }, subject: `${identifier} process ${pid}` }));
    });
    yield* Effect.acquireRelease(acquired, () => Effect.orDie(fs.remove(file)));
});
const watch = (names: Array.NonEmptyReadonlyArray<string>, pid: number) =>
    Stream.unwrap(
        Effect.map(ChildProcess.make('watch-apps', ['watch', ...names], { stdin: 'ignore', stderr: 'inherit' }), (handle) =>
            handle.stdout.pipe(
                Stream.decodeText,
                Stream.splitLines,
                Stream.mapEffect(decoded),
                Stream.flatMap((rows) => Stream.fromIterable(rows.filter((row) => row.identifier !== names[0] || row.pid === pid))),
            ),
        ),
    );
const owner = Effect.fn('owner')(function* (pid: number) {
    const { lines } = yield* spawned('ps', ['-o', 'command=', '-p', String(pid)]);
    return lines.join(' ').trim();
});
const crashed = Effect.fn('crashed')(function* (exit: Exited) {
    const fs = yield* FileSystem.FileSystem;
    const report = Option.map(Option.fromUndefinedOr(exit.report), ({ url }) => fileURLToPath(url));
    const ips = yield* Option.match(report, {
        onNone: () => Effect.succeedNone,
        onSome: (file) => Effect.option(Effect.flatMap(fs.readFileString(file), (text) => Schema.decodeEffect(Ips)(text.slice(text.indexOf('\n') + 1)))),
    });
    const thread = Option.flatMap(ips, ({ faultingThread, threads }) => Array.get(threads ?? [], faultingThread ?? 0));
    return {
        ...(exit.status === undefined ? {} : { status: exit.status }),
        ...(exit.signal === undefined ? {} : { signal: exit.signal }),
        ...Option.match(report, { onNone: () => ({}), onSome: (held) => ({ report: held }) }),
        ...Option.match(
            Option.flatMap(ips, ({ exception }) => Option.fromUndefinedOr(exception)),
            { onNone: () => ({}), onSome: ({ type, signal }) => ({ type: [type, signal].filter((part) => part !== undefined).join(' ') }) },
        ),
        ...Option.match(
            Option.zipWith(thread, ips, ({ frames }, { usedImages }) => frames.map(({ imageIndex, imageOffset, symbol }) => `${usedImages?.[imageIndex]?.name ?? imageIndex} ${symbol ?? `+${imageOffset}`}`)),
            { onNone: () => ({}), onSome: (frames) => ({ frames }) },
        ),
    } satisfies typeof Crash.Type;
});
const sampled = Effect.fn('sampled')(function* (pid: number, file: string) {
    const { code } = yield* spawned('sample', [String(pid), '1', '-file', file]);
    return yield* code === 0 ? Effect.succeed(file) : HarnessError.make({ reason: { _tag: 'Unavailable', because: `sample exited with code ${code}` }, subject: `process ${pid}` });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { crashed, type Exited, type Launch, launch, lock, owner, sampled, watch };
