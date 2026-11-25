import type { Policy } from '@rasm/policy';
import { Array, Effect, FileSystem, Function, Option, Path, Stream } from 'effect';
import { ChildProcess, type CommandLine } from 'effect/process';

// --- [TYPES] ---------------------------------------------------------------------------

type Table = ConstructorParameters<typeof Policy>[number];
type ScriptRow = Extract<Table[string], { readonly action: 'Shell'; readonly decide: unknown }>;
type Script = Parameters<ScriptRow['decide']>[0];
type Call = Parameters<ScriptRow['decide']>[1];
type Invocation = Call['invocations'][number];
type Verdict = Extract<ReturnType<ScriptRow['decide']>, readonly unknown[]>[number];
type Piece = Invocation['reads'][number]['pieces'][number];

// --- [OPERATIONS] ----------------------------------------------------------------------

const covers = (folder: string, path: string) => {
    const base = folder.replace(/\/+$/u, '');
    return base === '' || path === base || path.startsWith(`${base}/`);
};
const patternOf = (pieces: readonly Piece[]) => Option.map(Option.all(pieces.map((piece) => (piece._tag === 'Text' || piece._tag === 'Wild' ? Option.some(piece.text) : Option.none()))), Array.join(''));
const roots = (walker: Invocation, cwd: string): readonly string[] => Array.match(Array.getSomes(walker.reads.map(({ text }) => text)), { onEmpty: () => [cwd], onNonEmpty: Function.identity });
const rewrite = (invocation: Invocation, words: Array.NonEmptyReadonlyArray<string>): readonly Verdict[] => Option.toArray(invocation.span).map((span) => ({ edit: (editor) => [editor.replace(span, words)] }));
const findUp = Effect.fnUntraced(function* (cwd: string, marker: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const ancestors = Array.unfold(
        Option.some(path.resolve(cwd)),
        Option.map((folder) => [folder, Option.liftPredicate(path.dirname(folder), (parent) => parent !== folder)] as const),
    );
    return yield* Effect.findFirst(ancestors, (folder) => Effect.orElseSucceed(fs.exists(path.join(folder, marker)), () => false));
});
const run =
    (cwd: string) =>
    (strings: TemplateStringsArray, ...words: readonly CommandLine.TemplateExpression[]) =>
        Effect.scoped(
            Effect.gen(function* () {
                const handle = yield* ChildProcess.make({ cwd, stdin: 'ignore' })(strings, ...words);
                const [printed, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), handle.exitCode], { concurrency: 'unbounded' });
                return { printed, code };
            }),
        );

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Call, covers, findUp, type Invocation, patternOf, rewrite, roots, run, type Script, type Table, type Verdict };
