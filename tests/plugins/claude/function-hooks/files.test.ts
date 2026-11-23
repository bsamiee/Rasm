import { expect, layer } from '@effect/vitest';
import { Effect, FileSystem, Path, Record } from 'effect';
import { Decider, decided, refusing } from '#/engine';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SCRATCHPAD = '/private/tmp/claude-501/-Users-u-Developer-Rasm/0d9c0b52-46aa-4e5f-9a3c-1f6f0c2b7e11/scratchpad';
const FILES = {
    'probe.py': 'print(open("a.txt").read())\n',
    'probe.sh': "python3 -c 'import json,sys; print(json.load(sys.stdin))'\n",
    'notes.txt': 'alpha\nbeta\n',
} as const;
const WIPE = 'empties the scratchpad the session and every subagent share';
const PER_RESULT = 'starts one process per search result';
const REFUSED = [
    [`python3 -c 'import json,sys; print(json.load(sys.stdin)["a"])' < a.json`, 'python3 inline code parses JSON'],
    [`python3 - <<'EOF'\nopen("o.txt", "w").write("x")\nEOF`, 'python3 inline code writes files or runs commands'],
    [`node -e "require('fs').writeFileSync('o.txt', 'x')"`, 'node inline code writes files or runs commands'],
    ['bash probe.sh', 'python3 inline code parses JSON'],
    [`cat > written.sh <<'EOF'\npython3 -c 'import json,sys; print(json.loads(sys.argv[1]))' '{}'\nEOF\nbash written.sh`, 'python3 inline code parses JSON'],
    [`awk '{print > "out.txt"}' a.txt`, 'awk inline code writes files or runs commands'],
    [`osascript -e 'delay 240'`, 'A bare timer waits on nothing'],
    [`rm -rf ${SCRATCHPAD}/*`, WIPE],
    [`fd --hidden -t f . ${SCRATCHPAD} -X rm`, WIPE],
    ['trash /private/tmp/claude-501/-Users-u-Developer-Rasm/*/scratchpad', WIPE],
    ['cd /private/tmp/claude-501/-Users-u-Developer-Rasm && rm -rf 7fd799b0-df54-4140-ac59-37d55a3c703a', WIPE],
    ['d=$(dirname a/b); rm -rf "$d"', 'rm "$d" expands later, pass a literal path'],
    [String.raw`printf '%s\n' a b | xargs rm -rf`, 'xargs rm reads printf output'],
    ['fd --hidden -e log -x wc -l', `fd -x wc ${PER_RESULT}`],
    ['fd --hidden -e log --threads=1 -x wc -l', `fd -x wc ${PER_RESULT}`],
    [String.raw`find . -name '*.log' -exec wc -l {} \;`, `find -exec wc ${PER_RESULT}`],
    ['fd --hidden -d 3 react /', 'fd root / too wide'],
    ['sd -F gamma delta notes.txt', 'sd pattern matches nothing in notes.txt'],
    ['sd -s beta delta notes.txt', 'sd -s is no sd option'],
] as const;
const ALLOWED = [
    'python3 probe.py',
    "awk -F',' '{print $2}' a.csv",
    "awk -F: '{print $1,$3}' a.colon",
    "awk '/start/,/end/' a.txt",
    "awk 'length > 80' a.txt",
    `rm -rf ${SCRATCHPAD}/rebuild`,
    'rm -rf /private/tmp/claude-501/tmp.Xk2v',
    'T=$(mktemp -d); rm -rf "$T"',
    'for f in a b; do rm -f $f.txt; done',
    String.raw`printf '%s\n' a b | xargs rm -f`,
    'fd --hidden -d 2 react /',
    'fd --hidden -e log --threads=1 --max-results 9 -x wc -l',
    "find . -name '*.log' -exec du -sk {} +",
    'sd -F beta delta notes.txt',
] as const;
const REWRITTEN = [
    ['fd --hidden -d 1 react node_modules/.pnpm', 'fd --no-ignore --hidden -d 1 react node_modules/.pnpm'],
    ['fd -e ts libs', 'fd --hidden -e ts libs'],
] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const seeded = Effect.fnUntraced(function* () {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folder = yield* Effect.flatMap(fs.makeTempDirectoryScoped(), fs.realPath);
    yield* Effect.forEach(Record.toEntries(FILES), ([file, text]) => fs.writeFileString(path.join(folder, file), text), { discard: true });
    return folder;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(Decider, { excludeTestServices: true })('files', (it) => {
    it.effect.each(REFUSED)(
        '%s is refused because %s',
        ([command, reason]) =>
            Effect.map(
                Effect.flatMap(seeded(), (folder) => decided(command, folder)),
                (got) => expect(got).toSatisfy(refusing(reason), `refused because ${reason}`),
            ),
        30_000,
    );

    it.effect.each(ALLOWED)(
        '%s runs unchanged',
        (command) =>
            Effect.map(
                Effect.flatMap(seeded(), (folder) => decided(command, folder)),
                (got) => expect(got).toStrictEqual({ _tag: 'Allow' }),
            ),
        30_000,
    );

    it.effect.each(REWRITTEN)(
        '%s becomes %s, which runs unchanged when decided again',
        Effect.fnUntraced(function* ([command, rewritten]) {
            const folder = yield* seeded();
            expect(yield* decided(command, folder)).toMatchObject({ _tag: 'Rewrite', command: rewritten, background: false });
            expect(yield* decided(rewritten, folder)).toStrictEqual({ _tag: 'Allow' });
        }),
        30_000,
    );
});
