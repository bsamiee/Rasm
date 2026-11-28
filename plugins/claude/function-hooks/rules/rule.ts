import type { Invocation, Verdict } from '@rasm/policy/rule';
import { run } from '@rasm/process';
import { Array, Effect, FileSystem, Function, Option, Path } from 'effect';

// --- [OPERATIONS] ----------------------------------------------------------------------

const covers = (folder: string, path: string) => {
    const base = folder.replace(/\/+$/u, '');
    return base === '' || path === base || path.startsWith(`${base}/`);
};
const patternOf = (pieces: Invocation['reads'][number]['pieces']) => Option.map(Option.all(pieces.map((piece) => (piece._tag === 'Text' || piece._tag === 'Wild' ? Option.some(piece.text) : Option.none()))), Array.join(''));
const roots = (walker: Invocation, cwd: string): readonly string[] => Array.match(Array.getSomes(walker.reads.map(({ text }) => text)), { onEmpty: () => [cwd], onNonEmpty: Function.identity });
const rewrite = (invocation: Invocation, words: Array.NonEmptyReadonlyArray<string>): readonly Verdict<'Shell'>[] => Option.toArray(invocation.span).map((span) => ({ edit: (editor) => [editor.replace(span, words)] }));
const findUp = Effect.fnUntraced(function* (cwd: string, marker: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const ancestors = Array.unfold(
        Option.some(path.resolve(cwd)),
        Option.map((folder) => [folder, Option.liftPredicate(path.dirname(folder), (parent) => parent !== folder)] as const),
    );
    return yield* Effect.findFirst(ancestors, (folder) => Effect.orElseSucceed(fs.exists(path.join(folder, marker)), () => false));
});
const execute = (cwd: string) => (argv: Array.NonEmptyReadonlyArray<string>) => run(argv, { cwd }).pipe(Effect.catchTag('ExitError', (exited) => Effect.succeed(exited)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { covers, execute, findUp, patternOf, rewrite, roots };
