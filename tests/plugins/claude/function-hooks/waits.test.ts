import { expect, layer } from '@effect/vitest';
import { Effect, FileSystem, Path, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Decider, decided, refusing } from '#/engine';

// --- [CONSTANTS] -----------------------------------------------------------------------

const TIMER = 'A bare timer waits on nothing';
const REFUSED = [
    ['sleep 240', 'foreground', TIMER],
    ['sleep 240; echo woke', 'background', TIMER],
    ['while true; do sleep 60; done', 'monitor', TIMER],
    ['timeout 280 tail -f /dev/null', 'foreground', TIMER],
    ['ffmpeg -v error -re -f lavfi -i anullsrc=r=8000:cl=mono -t 165 -f null -', 'foreground', TIMER],
    ['ping -c 30 -i 1 127.0.0.1', 'foreground', TIMER],
    ['echo hello', 'foreground', 'Call prints literal text and runs nothing'],
    ['while :; do date; done', 'foreground', 'Loop on : never ends'],
    ['until false; do date; done', 'foreground', 'Loop on false never ends'],
    ['read line < /dev/zero', 'foreground', 'read < /dev/zero never returns'],
    ['watch -n 5 ls', 'foreground', 'watch repaints a screen'],
    ['launchctl submit -l probe -- /bin/sleep 9', 'foreground', 'launchctl submit detached jobs outlive the session'],
    ['(sleep 9; touch done) & echo started', 'foreground', '& job outlives the call'],
    ['jq -rs last /tmp/claude-501/x/tasks/a46c9759cea20d51a.output', 'foreground', "is a running agent's output"],
] as const;
const ALLOWED = [
    ['ffmpeg -v error -re -f lavfi -i anullsrc=r=8000:cl=mono -t 165 silence.mp3', 'foreground'],
    ['ffmpeg -v error -f lavfi -i anullsrc=r=8000:cl=mono -t 165 -f null -', 'foreground'],
    ['watch -n 5 ls', 'monitor'],
    ['launchctl list', 'foreground'],
    ['ping -c 1 127.0.0.1', 'foreground'],
    ['(cat req.jsonl; timeout 15 tail -f /dev/null) | cat', 'foreground'],
    ['echo $(( 1794861903 - $(date +%s) ))', 'foreground'],
    ["read -d '' line < /dev/zero", 'foreground'],
    ['until true; do date; done', 'foreground'],
    ['while false; do date; done', 'foreground'],
    ['until rg -q done b.log; do sleep 5; done', 'monitor'],
] as const;
const MONITORED = [
    ['until rg -q done b.log; do sleep 5; done', 'foreground', 'until rg -q done b.log; do sleep 5; done'],
    ['until ! pgrep -f fetch.sh; do sleep 2; done', 'background', "until ! pgrep -f '[f]etch.sh'; do sleep 2; done"],
    ['for i in $(seq 1 57); do [ -s b.log ] && break; sleep 10; done; cat b.log', 'foreground', 'for i in $(seq 1 57); do [ -s b.log ] && break; sleep 10; done; cat b.log'],
    ['timeout 420 tail -n +1 -f b.log | rg -m1 done', 'foreground', 'timeout 420 tail -n +1 -f b.log | rg -m1 done'],
    ['while true; do date; for f in a b; do [ -f "$f" ] && exit 0; done; sleep 1; done', 'foreground', 'while true; do date; for f in a b; do [ -f "$f" ] && exit 0; done; sleep 1; done'],
] as const;
const BACKGROUNDED = [
    ['wait4path /tmp/flag', 'wait4path /tmp/flag'],
    ['until curl -sf localhost:3000; do sleep 1; done; pnpm test', 'until curl -sf localhost:3000; do sleep 1; done; pnpm test'],
    ['until gh run view 45196768148 --json status; do sleep 30; done', 'gh run watch 45196768148 --exit-status'],
] as const;
const HOLDERS = { writer: 'exec 3>>"$0"', reader: 'exec 3<"$0"' } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const held = Effect.fnUntraced(function* (holder: keyof typeof HOLDERS) {
    const [fs, path, spawner] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
    const folder = yield* fs.makeTempDirectoryScoped();
    const output = path.join(folder, 'tasks', 'b7k2.output');
    yield* fs.makeDirectory(path.dirname(output));
    yield* fs.writeFileString(output, '');
    const holding = yield* spawner.spawn(ChildProcess.make('sh', ['-c', `${HOLDERS[holder]}; echo held; exec sleep 60`, output]));
    yield* Stream.runHead(Stream.splitLines(Stream.decodeText(holding.stdout)));
    return { folder, output, holding };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(Decider, { excludeTestServices: true })('waits', (it) => {
    it.effect.each(REFUSED)('%s as a %s call is refused because %s', ([command, kind, reason]) => Effect.map(decided(command, '/tmp', { kind }), (got) => expect(got).toSatisfy(refusing(reason), `refused because ${reason}`)), 30_000);

    it.effect.each(ALLOWED)('%s as a %s call runs unchanged', ([command, kind]) => Effect.map(decided(command, '/tmp', { kind }), (got) => expect(got).toStrictEqual({ _tag: 'Allow' })), 30_000);

    it.effect.each(MONITORED)('%s as a %s call waits on its condition as Monitor %s', ([command, kind, monitored]) => Effect.map(decided(command, '/tmp', { kind }), (got) => expect(got).toMatchObject({ _tag: 'Monitor', command: monitored })), 30_000);

    it.effect.each(BACKGROUNDED)('%s in the foreground moves to the background as %s', ([command, rewritten]) => Effect.map(decided(command, '/tmp'), (got) => expect(got).toMatchObject({ _tag: 'Rewrite', command: rewritten, background: true })), 30_000);

    it.effect(
        'Monitor tail of a task output its writer holds ends when the writer exits, and the rewrite decides to itself',
        Effect.fnUntraced(function* () {
            const [fs, spawner, { folder, output, holding }] = yield* Effect.all([FileSystem.FileSystem, ChildProcessSpawner.ChildProcessSpawner, held('writer')]);
            const bounded = yield* decided(`tail -f ${output}`, folder, { kind: 'monitor' });
            const command = bounded._tag === 'Rewrite' ? bounded.command : '';
            expect([bounded, yield* decided(command, folder, { kind: 'monitor' })]).toStrictEqual([{ _tag: 'Rewrite', command, background: false }, { _tag: 'Allow' }]);
            yield* fs.writeFileString(output, 'first\n', { flag: 'a' });
            const monitor = yield* spawner.spawn(ChildProcess.make('sh', ['-c', command]));
            const lines = Stream.runCollect(Stream.tap(Stream.splitLines(Stream.decodeText(monitor.stdout)), (line) => (line === 'first' ? fs.writeFileString(output, 'second\n', { flag: 'a' }) : holding.kill())));
            expect(yield* Effect.all([lines, monitor.exitCode])).toStrictEqual([['first', 'second'], 0]);
        }),
        30_000,
    );

    it.effect(
        'Second read of a task output a live writer holds runs, noting the task still runs',
        Effect.fnUntraced(function* () {
            const { folder, output } = yield* held('writer');
            const [first, second] = yield* Effect.all([decided(`cat ${output}`, folder, { session: 'writer' }), decided(`cat ${output}`, folder, { session: 'writer' })]);
            expect(first).toStrictEqual({ _tag: 'Allow' });
            expect(second).toSatisfy((decision: typeof first) => decision._tag === 'Allow' && decision.note?.includes('Task b7k2 still runs and notifies when it ends') === true, 'Allow noting Task b7k2 still runs');
        }),
        30_000,
    );

    it.effect(
        'Second read of a task output only a reader holds runs without a note',
        Effect.fnUntraced(function* () {
            const { folder, output } = yield* held('reader');
            const reads = yield* Effect.all([decided(`cat ${output}`, folder, { session: 'reader' }), decided(`cat ${output}`, folder, { session: 'reader' })]);
            expect(reads).toStrictEqual([{ _tag: 'Allow' }, { _tag: 'Allow' }]);
        }),
        30_000,
    );
});
