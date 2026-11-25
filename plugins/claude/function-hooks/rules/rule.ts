import type { Policy } from '@rasm/policy';
import { Array, Effect, FileSystem, Option, Path, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';

// --- [TYPES] ---------------------------------------------------------------------------

type Table = typeof Policy extends new (...tables: infer T extends readonly unknown[]) => unknown ? T[number] : never;
type Whole = Extract<NonNullable<Table[string]>, { readonly script: true }>;
type Script = Parameters<Whole['judge']>[0];
type Call = Parameters<Whole['judge']>[1];
type Invocation = Effect.Success<Script['invocations']>[number];
type Written = Invocation['words'][number];
type Verdict = Extract<ReturnType<Whole['judge']>, readonly unknown[]>[number];
type Editor = Parameters<Extract<Verdict, { readonly edit: unknown }>['edit']>[0];
type Word = Parameters<Editor['before']>[0];

// --- [OPERATIONS] ----------------------------------------------------------------------

const said = (written: Written): Option.Option<string> => Option.filter(written.field.text, () => written.exact);
const matches = (written: Written, pattern: RegExp): boolean => Option.exists(said(written), (text) => pattern.test(text));
const spells = (written: Written, ...words: readonly string[]): boolean => Option.exists(said(written), (text) => words.includes(text));
const texts = (words: readonly Written[]): Option.Option<readonly string[]> => Option.all(words.map(said));
const named = (invocation: Invocation): Option.Option<string> => (invocation.program._tag === 'Unknown' ? Option.none() : Option.some(invocation.program.name.slice(invocation.program.name.lastIndexOf('/') + 1)));
const is = (invocation: Invocation, ...names: readonly string[]): boolean => Option.exists(named(invocation), (name) => names.includes(name));
const operand = (invocation: Invocation, at = 0): Option.Option<string> => Option.flatMap(Array.get(invocation.operands, at), ({ word }) => said(word));
const head = (invocation: Invocation): Option.Option<Word> => Option.flatMap(Array.head(invocation.words), ({ node }) => node);
const foreground = ({ action }: Call): boolean => action.tool === 'Bash' && !('run_in_background' in action && action.run_in_background);
const leaves = (invocations: readonly Invocation[]): readonly Invocation[] => invocations.flatMap((invocation) => [invocation, ...leaves(invocation.inner)]);
const span = (invocation: Invocation): Option.Option<readonly [Word, Word]> => Option.all([head(invocation), Option.flatMap(Array.last(invocation.words), ({ node }) => node)]);
const command = (invocation: Invocation): string => [...Option.toArray(named(invocation)), ...invocation.path].join(' ');
const staged = (invocations: readonly Invocation[], invocation: Invocation, offset: number): Option.Option<Invocation> =>
    Option.flatMap(invocation.stage, ({ pipeline, index }) => Array.findFirst(invocations, (other) => other.source === invocation.source && other.within.length === invocation.within.length && Option.exists(other.stage, (stage) => stage.pipeline === pipeline && stage.index === index + offset)));
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

export { type Call, command, type Editor, foreground, head, type Invocation, is, leaves, matches, named, nearest, operand, ran, said, type Script, span, spells, staged, type Table, texts, type Verdict, type Word, type Written };
