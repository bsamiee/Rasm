import { availableParallelism } from 'node:os';
import { expect, layer } from '@effect/vitest';
import { Effect, FileSystem } from 'effect';
import { Decider, decided, refusing } from '#/engine';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CORES = availableParallelism();
const REFUSED = [
    ['nx daemon --stop', 'nx daemon --stop stops the daemon every session shares'],
    ['nx reset', 'nx reset stops the daemon and deletes the workspace data every session shares, run nx reset --only-cache'],
    ['nx reset --only-cloud', 'nx reset stops the daemon and deletes the workspace data every session shares, run nx reset --only-cache'],
    ['nx reset --only-workspace-data', 'nx reset deletes the workspace data every session shares, run nx reset --only-cache'],
    ['pnpm patch effect', 'pnpm patch copies a package into the tree'],
    ['pytest --config-file other.toml', 'Pytest must use project configuration and benchmark storage'],
] as const;
const ALLOWED = ['nx reset --only-cache', 'timeout 20 nx graph', 'make -j4', 'for i in 1 2; do nx run-many -t test -p web; done', 'for p in web api; do nx test $p > $p.log; done', 'for f in a.test.ts b.test.ts; do vitest run $f; done'] as const;
const REWRITTEN = [
    ['seq 3 | xargs -P 0 echo', `seq 3 | xargs -P ${CORES} echo`],
    ['make -j all', `make -j${CORES} all`],
    [String.raw`printf 'naïve → ✓\n' > n.txt; make -j all`, `printf 'naïve → ✓\\n' > n.txt; make -j${CORES} all`],
    ['for p in web api; do nx test $p; done', 'nx run-many -t test -p web,api'],
    ['for p in web api; do nx test $p --skip-nx-cache; done', 'nx run-many -t test -p web,api --skip-nx-cache'],
    ['for i in 1 2 3; do vitest run; done', 'vitest run --repeats 2'],
    ['nx daemon --stop; nx show projects', 'nx show projects'],
    ['timeout 120 nx show projects', 'nx show projects'],
    ['timeout 20 nx graph --file=graph.json', 'nx graph --file=graph.json'],
    ['nx run-many -t check -p web --skip-sync', 'nx sync && nx run-many -t check -p web'],
    ['nx build web --skipSync 2>&1 | tail -5', 'set -o pipefail; nx sync && nx build web 2>&1 | tail -5'],
    ['nx show project rasm --json 2>&1 | jq .name', 'set -o pipefail; nx show project rasm --json | jq .name'],
] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const inFolder = (command: string) =>
    Effect.flatMap(
        FileSystem.FileSystem.use((fs) => fs.makeTempDirectoryScoped()),
        (folder) => decided(command, folder),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(Decider, { excludeTestServices: true })('tasks', (it) => {
    it.effect.each(REFUSED)('%s is refused because %s', ([command, reason]) => Effect.map(inFolder(command), (got) => expect(got).toSatisfy(refusing(reason), `refused because ${reason}`)));

    it.effect.each(ALLOWED)('%s runs unchanged', (command) => Effect.map(inFolder(command), (got) => expect(got).toStrictEqual({ _tag: 'Allow' })));

    it.effect.each(REWRITTEN)(
        '%s becomes %s, which runs unchanged when decided again',
        Effect.fnUntraced(function* ([command, rewritten]) {
            expect(yield* inFolder(command)).toMatchObject({ _tag: 'Rewrite', command: rewritten, background: false });
            expect(yield* inFolder(rewritten)).toStrictEqual({ _tag: 'Allow' });
        }),
    );

    it.effect(
        'Skipped sync note reaches a session once, and again in another session',
        Effect.fnUntraced(function* () {
            const folder = yield* FileSystem.FileSystem.use((fs) => fs.makeTempDirectoryScoped());
            const decisions = yield* Effect.forEach(['first', 'first', 'second'], (session) => decided('nx build web --skip-sync', folder, { session }));
            const notes = decisions.map((decision) => (decision._tag === 'Rewrite' ? decision.note : decision._tag));
            expect(notes).toStrictEqual([notes[0], undefined, notes[0]]);
            expect(notes[0]).toBeTypeOf('string');
        }),
    );
});
