import type { Policy } from '@rasm/policy';
import { Array, Effect, FileSystem, Option, Path, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';

// --- [TYPES] ---------------------------------------------------------------------------

type Table = typeof Policy extends new (...tables: infer T extends readonly unknown[]) => unknown ? T[number] : never;
type Whole = Extract<NonNullable<Table[string]>, { readonly action: 'Shell'; readonly decide: unknown }>;
type Script = Parameters<Whole['decide']>[0];
type Call = Parameters<Whole['decide']>[1];
type Invocation = Effect.Success<Script['invocations']>[number];
type Written = Invocation['words'][number];
type Verdict = Extract<ReturnType<Whole['decide']>, readonly unknown[]>[number];
type Editor = Parameters<Extract<Verdict, { readonly edit: unknown }>['edit']>[0];
type Word = Parameters<Editor['before']>[0];

// --- [OPERATIONS] ----------------------------------------------------------------------

const feeds = (producer: Invocation, { stdin, source }: Invocation): boolean => stdin._tag === 'Piped' && producer.source === source && Option.exists(producer.stage, ({ pipeline, index }) => stdin.pipeline === pipeline && stdin.index === index);
const ancestors = (path: Path.Path, folder: string): readonly string[] => (path.dirname(folder) === folder ? [folder] : [folder, ...ancestors(path, path.dirname(folder))]);
const nearest = Effect.fnUntraced(function* (cwd: string, marker: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    return yield* Effect.findFirst(ancestors(path, path.resolve(cwd)), (folder) => Effect.orElseSucceed(fs.exists(path.join(folder, marker)), () => false));
});
const ran = Effect.fnUntraced(function* (argv: Array.NonEmptyReadonlyArray<string>, cwd: string) {
    const spawner = yield* ChildProcessSpawner.ChildProcessSpawner;
    const handle = yield* spawner.spawn(ChildProcess.make(Array.headNonEmpty(argv), Array.tailNonEmpty(argv), { cwd, stdin: 'ignore' }));
    const [printed, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), handle.exitCode], { concurrency: 'unbounded' });
    return { printed, code };
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Call, type Editor, feeds, type Invocation, nearest, ran, type Script, type Table, type Verdict, type Word, type Written };
