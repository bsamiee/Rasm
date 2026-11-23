import { expect, layer } from '@effect/vitest';
import { Array, Effect, FileSystem, Order, Path, Record, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Decider, decided } from '#/engine';

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = {
    'a.txt': ['alpha', 'start and end on one line', 'beta needle', 'gamma', 'start here', 'delta', 'epsilon', 'end here', 'zeta', 'eta needle', 'theta', 'iota'].join('\n').concat('\n'),
    'a.csv': ['x,y,z', '1,2,3', 'nocomma', '4,,6'].join('\n').concat('\n'),
    '.ignore': 'ignored.log\nignored.txt\n',
    '.hidden.txt': 'hidden needle\n',
    '.hidden.log': 'hidden\n',
    'ignored.txt': 'ignored needle\n',
    'one.log': 'one\n',
    'ignored.log': 'ignored\n',
    'sub/two.log': 'two two\n',
    'sub/deeper/three.log': 'three three three\n',
    'sub/deeper/deepest/four.log': 'four\n',
    'version.js': "console.log(require('effect/package.json').version)\n",
    'node_modules/effect/package.json': '{ "name": "effect", "version": "9.8.7" }\n',
} as const;
const SAME_OUTPUT = [
    ["awk 'NR>=3 && NR<=9' a.txt", 'sed'],
    ["awk '{print}' a.txt", 'cat'],
    ["awk -F',' '{print $1}' a.csv", 'cut'],
    ['node version.js', 'jq'],
] as const;
const SAME_LINES = [
    ["find . -name '*.log' -maxdepth 3", 'fd'],
    [String.raw`find . -name '*.log' -exec du -sk {} \;`, 'find'],
    ['fd --hidden -e log -x du -sk', 'fd'],
    ['grep -rn needle .', 'rg'],
] as const;
const RELOCATED = ['printf moved > {file} && cat {file}', 'printf moved > {file} && cat "{file}"', "printf moved > '{file}' && cat {file}"] as const;
const LEADING_FOLDER = /^\.\//u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const seeded = Effect.fnUntraced(function* () {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folder = yield* Effect.flatMap(fs.makeTempDirectoryScoped(), fs.realPath);
    yield* Effect.forEach(Record.toEntries(FILES), ([file, text]) => Effect.andThen(fs.makeDirectory(path.dirname(path.join(folder, file)), { recursive: true }), fs.writeFileString(path.join(folder, file), text)), { discard: true });
    return folder;
});

const ran = Effect.fnUntraced(function* (command: string, folder: string) {
    const spawner = yield* ChildProcessSpawner.ChildProcessSpawner;
    const handle = yield* spawner.spawn(ChildProcess.make('bash', ['-c', command], { cwd: folder, env: { RIPGREP_CONFIG_PATH: '' }, extendEnv: true }));
    return yield* Effect.all({ stdout: Stream.mkString(Stream.decodeText(handle.stdout)), stderr: Stream.mkString(Stream.decodeText(handle.stderr)), code: handle.exitCode }, { concurrency: 'unbounded' });
});

const lines = ({ stdout, stderr, code }: { readonly stdout: string; readonly stderr: string; readonly code: number }) => ({
    code,
    stderr,
    lines: Array.sort(
        stdout
            .trimEnd()
            .split('\n')
            .map((line) => line.replace(LEADING_FOLDER, '')),
        Order.String,
    ),
});

const rewritten = Effect.fnUntraced(function* (command: string, folder: string) {
    const decision = yield* decided(command, folder);
    const replaced = decision._tag === 'Rewrite' ? decision.command : command;
    const after = yield* ran(replaced, folder);
    return { decision, replaced, again: yield* decided(replaced, folder), before: yield* ran(command, folder), after };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(Decider, { excludeTestServices: true })('rewrites', (it) => {
    it.effect.each(SAME_OUTPUT)(
        '%s becomes a %s call printing the same output with the same status, which runs unchanged when decided again',
        Effect.fnUntraced(function* ([command, program]) {
            const { decision, replaced, again, before, after } = yield* rewritten(command, yield* seeded());
            expect([decision, again]).toMatchObject([{ _tag: 'Rewrite', background: false }, { _tag: 'Allow' }]);
            expect(replaced.split(' ', 1)).toStrictEqual([program]);
            expect(before.stdout).not.toBe('');
            expect(after).toStrictEqual(before);
        }),
        30_000,
    );

    it.effect.each(SAME_LINES)(
        '%s becomes a %s call printing the same lines in any order with the same status, which runs unchanged when decided again',
        Effect.fnUntraced(function* ([command, program]) {
            const { decision, replaced, again, before, after } = yield* rewritten(command, yield* seeded());
            expect([decision, again]).toMatchObject([{ _tag: 'Rewrite', background: false }, { _tag: 'Allow' }]);
            expect(replaced.split(' ', 1)).toStrictEqual([program]);
            expect(before.stdout).not.toBe('');
            expect(lines(after)).toStrictEqual(lines(before));
        }),
        30_000,
    );

    it.effect.each(RELOCATED)(
        '%s writes and reads one relocated file outside /tmp',
        Effect.fnUntraced(function* (template) {
            const [fs, path, folder] = yield* Effect.all([FileSystem.FileSystem, Path.Path, seeded()]);
            const file = `/tmp/${path.basename(folder)}.txt`;
            const decision = yield* decided(template.replaceAll('{file}', file), folder);
            const replaced = decision._tag === 'Rewrite' ? decision.command : '';
            expect([decision, yield* decided(replaced, folder)]).toMatchObject([{ _tag: 'Rewrite', background: false }, { _tag: 'Allow' }]);
            expect([yield* ran(replaced, folder), yield* fs.exists(file)]).toStrictEqual([{ stdout: 'moved', stderr: '', code: 0 }, false]);
        }),
        30_000,
    );
});
