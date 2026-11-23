import { availableParallelism } from 'node:os';
import process from 'node:process';
import { fileURLToPath } from 'node:url';
import { layerUndiciOptions } from '@effect/platform-node/NodeHttpClient';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it, makeMethods } from '@effect/vitest';
import { type Workspace, it as workspaceTest } from '@rasm/testing/nx';
import { Array, Cause, Effect, FileSystem, Layer, Path, Schema, Stream } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { HttpClient, HttpClientRequest, HttpClientResponse } from 'effect/http';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SERVER = fileURLToPath(import.meta.resolve('@rasm/function-hooks/claude/server.ts'));
const ALLOWED = { _tag: 'Allow' } as const;
const BARE_TIMER = {
    _tag: 'Deny',
    reason: 'A bare timer waits on nothing and wakes you with nothing new: background tasks and agents notify when they end, Monitor `until <check>; do sleep 5; done` waits on a condition with timeout_ms as its bound, and a turn with nothing to do ends until a notice arrives',
} as const;
const BACKGROUND = (command: string) => ({ _tag: 'Rewrite', command, background: true, note: 'Call runs in the background and notifies on completion, never poll it' }) as const;
const WAITS = [
    ['until rg -q done b.log; do sleep 5; done', 'foreground', { _tag: 'Monitor', command: 'until rg -q done b.log; do sleep 5; done' }],
    ['until ! pgrep -f fetch.sh; do sleep 2; done', 'background', { _tag: 'Monitor', command: "until ! pgrep -f '[f]etch.sh'; do sleep 2; done" }],
    ['for i in $(seq 1 57); do [ -s b.log ] && break; sleep 10; done; cat b.log', 'foreground', { _tag: 'Monitor', command: 'for i in $(seq 1 57); do [ -s b.log ] && break; sleep 10; done; cat b.log' }],
    ['timeout 420 tail -n +1 -f b.log | rg -m1 done', 'foreground', { _tag: 'Monitor', command: 'timeout 420 tail -n +1 -f b.log | rg -m1 done' }],
    ['until rg -q done b.log; do sleep 5; done', 'monitor', { _tag: 'Allow' }],
    ['sleep 240', 'foreground', BARE_TIMER],
    ['sleep 240; echo woke', 'background', BARE_TIMER],
    ['while true; do sleep 60; done', 'monitor', BARE_TIMER],
    ['ffmpeg -v error -re -f lavfi -i anullsrc=r=8000:cl=mono -t 165 -f null -', 'foreground', BARE_TIMER],
    ['ping -c 30 -i 1 127.0.0.1', 'foreground', BARE_TIMER],
    ['wait4path /tmp/flag', 'foreground', BACKGROUND('wait4path /tmp/flag')],
    ['until curl -sf localhost:3000; do sleep 1; done; pnpm test', 'foreground', BACKGROUND('until curl -sf localhost:3000; do sleep 1; done; pnpm test')],
    ['for f in *.log; do tail -n 5 $f; sleep 1; done', 'foreground', BACKGROUND('for f in *.log; do tail -n 5 $f; sleep 1; done')],
    ['while IFS= read -r u; do curl -s "$u"; sleep 1; done < urls.txt', 'foreground', BACKGROUND('while IFS= read -r u; do curl --fail-with-body -s "$u"; sleep 1; done < urls.txt')],
    ['ffmpeg -f lavfi -i anullsrc=r=44100:cl=stereo -t 3 -q:a 9 silence.mp3', 'foreground', ALLOWED],
    ['echo $(( 1794861903 - $(date +%s) ))', 'foreground', { _tag: 'Allow' }],
    [
        'jq -rs last /tmp/claude-501/x/tasks/a46c9759cea20d51a.output',
        'foreground',
        {
            _tag: 'Deny',
            reason: "/tmp/claude-501/x/tasks/a46c9759cea20d51a.output is a running agent's output that the agent reports when it ends, SendMessage asks it for status",
        },
    ],
    ['rg -c error /Users/u/.claude/projects/-x/5f1e.jsonl', 'foreground', { _tag: 'Deny', reason: '/Users/u/.claude/projects/-x/5f1e.jsonl is a session transcript, atuin threads show <session id> --style ndjson reads it' }],
    ['tail -n 50 /Users/u/.zsh_history', 'foreground', { _tag: 'Deny', reason: "/Users/u/.zsh_history is shell history, atuin search --filter-mode global --search-mode fulltext '<words>' reads it" }],
    ['rg -l error /Users/u/.claude/projects', 'foreground', { _tag: 'Deny', reason: '/Users/u/.claude/projects is a session transcript, atuin threads show <session id> --style ndjson reads it' }],
    ['cat /Users/u/.claude/projects/-x/memory/MEMORY.md', 'foreground', ALLOWED],
] as const;
const HOLDERS = {
    writer: (output: string) => ChildProcess.make('sh', ['-c', 'exec sleep 60 >> "$0"', output]),
    reader: (output: string) => ChildProcess.make('tail', ['-f', output], { stdout: 'ignore' }),
} as const;
const CORES = availableParallelism();
const REWRITTEN = (command: string, note?: string) => ({ _tag: 'Rewrite', command, background: false, ...(note === undefined ? {} : { note }) }) as const;
const WORKSPACE = {
    'nx.json': '{}',
    'notes/plan.md': '',
    'suite/project.json': JSON.stringify({
        name: 'suite',
        targets: {
            test: { executor: 'nx:noop', dependsOn: [{ target: 'test--*', params: 'forward' }] },
            'test--vitest': { command: 'vitest run', cache: true, metadata: { help: { command: 'pnpm exec vitest run --help', example: {} } } },
            clean: { executor: 'nx:noop', dependsOn: [{ target: 'clean--*', params: 'forward' }] },
            'clean--tsc': { command: 'tsc --build --clean', metadata: { help: { command: 'pnpm exec tsc --build --help', example: {} } } },
            'clean--vitest': { command: 'vitest --clearCache', metadata: { help: { command: 'pnpm exec vitest --help', example: {} } } },
            serve: { executor: 'nx:noop', dependsOn: [{ target: 'serve--*', params: 'forward' }] },
            'serve--vite': { command: 'vite', continuous: true },
        },
    }),
} as const;
const UNBOUND = 'timeout before nx removed, as its kill loses unfinished tasks of a run that ends on its own: a foreground call past its timeout moves to the background and notifies, and TaskStop ends a hung run';
const WORKSPACE_CALLS = [
    ['nx test suite -- -t decide', 'suite', REWRITTEN('nx run suite:test--vitest -- -t decide', 'nx test became nx run suite:test--vitest -- -t decide')],
    ['nx clean suite -- --verbose', 'suite', { _tag: 'Deny', reason: 'pass one-run arguments to one of: clean--tsc,clean--vitest' }],
    ['vitest run', 'suite', REWRITTEN('nx test suite', 'vitest became nx test suite')],
    ['vitest run', 'notes', ALLOWED],
    ['timeout 600 nx test suite', 'suite', REWRITTEN('nx test suite', UNBOUND)],
    ['timeout 120 nx show projects', 'suite', REWRITTEN('nx show projects', UNBOUND)],
    ['timeout 600 vitest run', 'suite', REWRITTEN('nx test suite', `vitest became nx test suite. ${UNBOUND}`)],
    ['timeout 30 nx serve suite', 'suite', ALLOWED],
    ['timeout 30 nx run suite:serve--vite', 'suite', ALLOWED],
    ['timeout 20 nx graph', 'suite', ALLOWED],
    ['timeout 20 nx graph --file=graph.json', 'suite', REWRITTEN('nx graph --file=graph.json', UNBOUND)],
] as const;
const SESSION_CALLS = [
    ['launchctl submit -l probe -- /bin/sleep 9', { _tag: 'Deny', reason: 'launchctl submit detached jobs outlive the session; use run_in_background and TaskStop, or a mise [daemons] row' }],
    ['claude -p hi', ALLOWED],
    ['seq 3 | xargs -P 0 echo', REWRITTEN(`seq 3 | xargs -P ${CORES} echo`, `xargs -P 0 became ${CORES}`)],
    ['parallel -j0 echo ::: a', REWRITTEN(`parallel -j${CORES} echo ::: a`, `parallel -j0 became -j${CORES}`)],
    ['parallel -j 100% echo ::: a', ALLOWED],
    ['parallel -j -1 echo ::: a', ALLOWED],
    ['make -j all', REWRITTEN(`make -j${CORES} all`, `make -j all became -j${CORES}`)],
    ['make -j4', ALLOWED],
    ['ninja -j0', REWRITTEN(`ninja -j${CORES}`, `ninja -j0 became -j${CORES}`)],
    ['nx daemon --stop; nx show projects', REWRITTEN('nx show projects', 'nx daemon --stop removed, the daemon is shared by every session')],
    ['nx reset --onlyDaemon && nx show projects', REWRITTEN('nx show projects', 'nx reset --onlyDaemon removed, the daemon is shared by every session')],
    ['nx daemon --stop', { _tag: 'Deny', reason: 'nx daemon --stop stops the daemon every session shares, leave it running' }],
] as const;

const JSON_READ = (program: string) => ({ _tag: 'Deny', reason: `${program} inline code parses JSON, use jq` }) as const;
const FILE_WRITE = (program: string) => ({ _tag: 'Deny', reason: `${program} inline code writes files or runs commands, run each command directly and edit files with sd or your file edit tool` }) as const;
const UNRESOLVED_DELETE = (subject: string, raw: string) => ({ _tag: 'Deny', reason: `${subject} ${raw} expands later, pass a literal path` }) as const;
const FIXTURES = {
    'probe.py': 'print(open("a.txt").read())\n',
    'probe.mjs': 'console.log(1);\n',
    'probe.sh': "python3 -c 'import json,sys; print(json.load(sys.stdin))'\n",
    'version.js': "console.log(require('effect/package.json').version)\n",
} as const;
const SCRIPTED = (scratch: string, scratchpad = '/private/tmp/claude-501/-Users-u-Developer-Rasm/0d9c0b52-46aa-4e5f-9a3c-1f6f0c2b7e11/scratchpad') =>
    [
        [`python3 ${scratch}/probe.py`, ALLOWED],
        [`node ${scratch}/probe.mjs`, ALLOWED],
        [`swift -e 'print(1)'`, ALLOWED],
        [`uv run --with materialyoucolor python -I -c "$(cat ${scratch}/probe.py)"`, ALLOWED],
        [`python3 -c 'import json,sys; print(json.load(sys.stdin)["a"])' < a.json`, JSON_READ('python3')],
        [`python3 - <<'EOF'\nopen("o.txt", "w").write("x")\nEOF`, FILE_WRITE('python3')],
        [`python3 -c 'import subprocess; subprocess.run(["rm", "-rf", "x"])'`, FILE_WRITE('python3')],
        [`node -e "require('fs').writeFileSync('o.txt', 'x')"`, FILE_WRITE('node')],
        [`bash ${scratch}/probe.sh`, JSON_READ('python3')],
        [`sh ${scratch}/probe.sh`, JSON_READ('python3')],
        [`source ${scratch}/probe.sh`, JSON_READ('python3')],
        [`. ${scratch}/probe.sh`, JSON_READ('python3')],
        [`cat > ${scratch}/written.sh <<'EOF'\npython3 -c 'import json,sys; print(json.loads(sys.argv[1]))' '{}'\nEOF\nbash ${scratch}/written.sh`, JSON_READ('python3')],
        [`node ${scratch}/version.js`, { _tag: 'Rewrite', command: 'jq -r .version node_modules/effect/package.json', background: false }],
        [`osascript -e 'delay 240'`, BARE_TIMER],
        [`swift -e 'Thread.sleep(forTimeInterval: 240)'`, BARE_TIMER],
        [`awk 'NR>=3 && NR<=9' a.txt`, { _tag: 'Rewrite', command: "sed -n '3,9p' a.txt", background: false }],
        [`awk 'NR==5' a.txt`, { _tag: 'Rewrite', command: "sed -n '5p' a.txt", background: false }],
        [`awk '{print}' a.txt`, { _tag: 'Rewrite', command: 'cat a.txt', background: false }],
        [`awk 'length > 80' a.txt`, { _tag: 'Rewrite', command: "rg -n '^.{81,}$' a.txt", background: false }],
        [`awk -F',' '{print $2}' a.csv`, { _tag: 'Rewrite', command: 'cut -d, -f2 a.csv', background: false }],
        [`awk -F: '{print $1,$3}' a.txt`, { _tag: 'Rewrite', command: 'cut -d: -f1,3 a.txt', background: false }],
        [`awk '/start/,/end/' a.txt`, { _tag: 'Rewrite', command: 'sed -En /start/,/end/p a.txt', background: false }],
        [`awk '{print $2}' a.txt`, ALLOWED],
        [`sysctl -n hw.memsize | awk '{print $1/1073741824" GB"}'`, ALLOWED],
        [`awk '{s+=$1} END {print s}' sizes.txt`, ALLOWED],
        [`ps -axo pid,rss | awk '$2 > 100000'`, ALLOWED],
        [`awk '{print > "out.txt"}' a.txt`, { _tag: 'Deny', reason: 'awk inline code writes files or runs commands, run each command directly and edit files with sd or your file edit tool' }],
        [`rm -rf ${scratchpad}/*`, { _tag: 'Deny', reason: `rm ${scratchpad}/* empties the scratchpad the session and every subagent share, delete only the subfolder you made by name` }],
        [`trash ${scratchpad}`, { _tag: 'Deny', reason: `trash ${scratchpad} empties the scratchpad the session and every subagent share, delete only the subfolder you made by name` }],
        [`fd --hidden -t f . ${scratchpad} -X rm`, { _tag: 'Deny', reason: `rm ${scratchpad} empties the scratchpad the session and every subagent share, delete only the subfolder you made by name` }],
        [`rm -rf ${scratchpad}/rebuild`, ALLOWED],
        ['fd --hidden -d 1 react node_modules/.pnpm', { _tag: 'Rewrite', command: 'fd --no-ignore --hidden -d 1 react node_modules/.pnpm', background: false }],
        [`rm -rf ${scratchpad.slice(0, scratchpad.indexOf('/-'))}/tmp.Xk2v`, ALLOWED],
        ['fd --hidden -d 2 react /usr', ALLOWED],
        ['fd --hidden -d 3 react /usr', ALLOWED],
        ['fd --hidden -d 3 react /', { _tag: 'Deny', reason: 'fd root / too wide, pass a package folder or a depth of at most 2' }],
        ['fd --hidden react node_modules/.pnpm', { _tag: 'Rewrite', command: 'fd --no-ignore --hidden react node_modules/.pnpm', background: false }],
        ['K=/private/tmp/claude-501/x/scratchpad/nvkeys.toml; printf x > "$K"; rm -f "$K"', ALLOWED],
        ['K=$(mktemp); printf x > "$K"; rm -f "$K"', ALLOWED],
        ['T=$(mktemp -d); rm -rf "$T"', ALLOWED],
        ['for f in a b; do rm -f $f.txt; done', ALLOWED],
        ['while IFS= read -r p; do rm -f -- "$p"; done < list.txt', ALLOWED],
        [String.raw`printf '%s\n' a b | xargs rm -f`, ALLOWED],
        ['d=$(dirname a/b); rm -rf "$d"', UNRESOLVED_DELETE('rm', '"$d"')],
        ['rm -r $UNSET_BUILD_ROOT/build', UNRESOLVED_DELETE('rm', '$UNSET_BUILD_ROOT/build')],
        ['trash "$stale"', UNRESOLVED_DELETE('trash', '"$stale"')],
        [String.raw`printf '%s\n' a b | xargs rm -rf`, { _tag: 'Deny', reason: 'xargs rm reads printf output, name each path to rm' }],
        ['cd /tmp && ls', ALLOWED],
        [`find . -name '*.log' -maxdepth 3`, { _tag: 'Rewrite', command: "fd --hidden --no-ignore -g '*.log' -d 3", background: false, note: "find became fd --hidden --no-ignore -g '*.log' -d 3" }],
        ['python3 --help-xoptions', ALLOWED],
        [
            `trash ${scratchpad.slice(0, scratchpad.lastIndexOf('/', scratchpad.lastIndexOf('/') - 1))}/*/scratchpad`,
            { _tag: 'Deny', reason: `trash ${scratchpad.slice(0, scratchpad.lastIndexOf('/', scratchpad.lastIndexOf('/') - 1))}/*/scratchpad empties the scratchpad the session and every subagent share, delete only the subfolder you made by name` },
        ],
        ['timeout 280 tail -f /dev/null', BARE_TIMER],
        ['(cat req.jsonl; timeout 15 tail -f /dev/null) | cat', ALLOWED],
        ['while :; do date; done', { _tag: 'Deny', reason: 'Loop on : never ends, as nothing changes its condition and no break or exit leaves it, wait on a real condition with Monitor `until <check>; do sleep <seconds>; done`' }],
    ] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Decision = Schema.Union([
    Schema.TaggedStruct('Allow', { note: Schema.optionalKey(Schema.String) }),
    Schema.TaggedStruct('Deny', { reason: Schema.String }),
    Schema.TaggedStruct('Rewrite', { command: Schema.String, background: Schema.Boolean, note: Schema.optionalKey(Schema.String) }),
    Schema.TaggedStruct('Monitor', { command: Schema.String, note: Schema.optionalKey(Schema.String) }),
]);
const GraphFile = Schema.fromJsonString(Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

const served = Effect.fnUntraced(function* (started: Pick<ChildProcess.CommandOptions, 'cwd' | 'env'> = {}) {
    const [fs, path, spawner] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
    const folder = yield* fs.makeTempDirectoryScoped();
    const socket = path.join(folder, 'decide.sock');
    const server = yield* spawner.spawn(ChildProcess.make(process.execPath, [SERVER, socket], { ...started, extendEnv: true }));
    yield* Effect.raceFirst(
        Stream.runHead(Stream.filter(Stream.splitLines(Stream.decodeText(server.stdout)), (line) => line === socket)),
        Effect.flatMap(server.exitCode, (code) => Effect.fail(new Cause.UnknownError(code, `${SERVER} exits ${code} before it listens on ${socket}`))),
    );
    const asked = (route: 'decide' | 'read', body: object) =>
        HttpClient.execute(HttpClientRequest.bodyJsonUnsafe(HttpClientRequest.post(`http://function-hooks/${route}`), body)).pipe(
            Effect.flatMap(HttpClientResponse.matchStatus({ '2xx': ({ json }) => json, orElse: ({ status, text }) => Effect.flatMap(text, (answer) => Effect.fail(new Cause.UnknownError(answer, `${route} answers ${status}, ${answer}`))) })),
            Effect.flatMap(Schema.decodeUnknownEffect(Decision)),
            Effect.provide(layerUndiciOptions({ connect: { socketPath: socket } })),
        );
    const decided = (command: string, kind: 'foreground' | 'background' | 'monitor', stated: { readonly cwd?: string; readonly agent?: string } = {}) =>
        Effect.flatMap(
            SecureRandom.SecureRandom.use((random) => random.uuidV4()),
            (call) => asked('decide', { command, cwd: folder, kind, session: 'session', call, ...stated }),
        );
    return { folder, decided, asked };
});

const held = Effect.fnUntraced(function* (holder: keyof typeof HOLDERS) {
    const [fs, path, spawner, { folder, decided }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner, served()]);
    const output = path.join(folder, 'tasks', 'b7k2.output');
    yield* fs.makeDirectory(path.dirname(output));
    yield* fs.writeFileString(output, '');
    return { output, holding: yield* spawner.spawn(HOLDERS[holder](output)), decided };
});

const servedIn = Effect.fnUntraced(function* (workspace: Workspace) {
    const [path, spawner] = yield* Effect.all([Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
    const data = '.nx/workspace-data';
    const repository = yield* spawner.exitCode(ChildProcess.make('git', ['init', '--quiet'], { cwd: workspace.root }));
    const graph = yield* Effect.promise(() => workspace.write(WORKSPACE).graph({}));
    assert.deepStrictEqual<readonly [number, readonly string[]]>([repository, Object.keys(graph.nodes)], [0, ['suite']], 'workspace is a git repository whose graph holds its one project');
    workspace.write({ [path.join(data, 'project-graph.json')]: yield* Effect.orDie(Schema.encodeEffect(GraphFile)(graph)) });
    const { decided } = yield* served({ cwd: workspace.root, env: { NX_WORKSPACE_DATA_DIRECTORY: data } });
    return (command: string, folder: string) => decided(command, 'foreground', { cwd: path.join(workspace.root, folder) });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.live(
    'Monitor tail of a written task output waits on its writer, and the rewrite decides to itself',
    Effect.fnUntraced(
        function* () {
            const [fs, spawner, { output, holding, decided }] = yield* Effect.all([FileSystem.FileSystem, ChildProcessSpawner.ChildProcessSpawner, held('writer')]);
            const bounded = yield* decided(`tail -f ${output}`, 'monitor');
            assert(bounded._tag === 'Rewrite', `tail -f ${output} decides ${bounded._tag}`);
            assert.deepStrictEqual([bounded, yield* decided(bounded.command, 'monitor')], [{ _tag: 'Rewrite', command: bounded.command, background: false }, ALLOWED]);
            yield* fs.writeFileString(output, 'first\n', { flag: 'a' });
            const monitor = yield* spawner.spawn(ChildProcess.make('sh', ['-c', bounded.command]));
            const tailed = Stream.runCollect(Stream.tap(Stream.splitLines(Stream.decodeText(monitor.stdout)), (line) => (line === 'first' ? fs.writeFileString(output, 'second\n', { flag: 'a' }) : holding.kill())));
            assert.deepStrictEqual<readonly [readonly string[], number]>(yield* Effect.all([tailed, monitor.exitCode]), [['first', 'second'], 0]);
        },
        Effect.scoped,
        Effect.provide(Layer.merge(layer, SecureRandom.layerWebCrypto)),
    ),
    30_000,
);

it.live.each([
    ['writer', (output: string) => [ALLOWED, { ...ALLOWED, note: `Task b7k2 still runs and notifies when it ends, and Monitor \`tail -n +1 -F ${output} | grep --line-buffered -E '<pattern>'\` streams the lines you wait for` }]],
    ['reader', () => [ALLOWED, ALLOWED]],
] as const)(
    'Repeated read of a task output its %s holds runs, a live writer adding where its lines stream',
    Effect.fnUntraced(
        function* ([holder, decisions]) {
            const { output, decided } = yield* held(holder);
            assert.deepStrictEqual([yield* decided(`cat ${output}`, 'foreground'), yield* decided(`cat ${output}`, 'foreground')], [...decisions(output)]);
        },
        Effect.scoped,
        Effect.provide(Layer.merge(layer, SecureRandom.layerWebCrypto)),
    ),
    30_000,
);

it.live(
    'Leading cd into the folder the request names keeps a later redirect in that folder wherever the shell starts',
    Effect.fnUntraced(
        function* () {
            const [fs, path, spawner, { folder, decided }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner, served()]);
            const elsewhere = yield* fs.makeTempDirectoryScoped();
            const command = `cd ${folder} && echo decided > decided.txt`;
            const decision = yield* decided(command, 'foreground');
            assert.notStrictEqual(decision._tag, 'Deny');
            yield* spawner.exitCode(ChildProcess.make('sh', ['-c', decision._tag === 'Rewrite' ? decision.command : command], { cwd: elsewhere }));
            assert.deepStrictEqual(yield* Effect.all([fs.exists(path.join(folder, 'decided.txt')), fs.exists(path.join(elsewhere, 'decided.txt'))]), [true, false]);
        },
        Effect.scoped,
        Effect.provide(Layer.merge(layer, SecureRandom.layerWebCrypto)),
    ),
    30_000,
);

it.live(
    'Waits become a Monitor, a bounded background task, or a refusal naming the notice that ends them',
    Effect.fnUntraced(
        function* () {
            const { decided } = yield* served();
            const decisions = yield* Effect.forEach(WAITS, ([command, kind]) => decided(command, kind));
            assert.deepStrictEqual(
                decisions,
                Array.map(WAITS, (wait) => wait[2]),
            );
        },
        Effect.scoped,
        Effect.provide(Layer.merge(layer, SecureRandom.layerWebCrypto)),
    ),
    60_000,
);

it.live(
    'Read of a running agent output, a session transcript, the projects folder, or shell history is refused with its route, and a shell task output or a project memory file is allowed',
    Effect.fnUntraced(
        function* () {
            const { asked } = yield* served();
            const transcript = 'is a session transcript, atuin threads show <session id> --style ndjson reads it';
            const cases = [
                ['/tmp/claude-501/x/tasks/b7k2.output', ALLOWED],
                ['/Users/u/.claude/projects/-x/memory/MEMORY.md', ALLOWED],
                ['/tmp/claude-501/x/tasks/a46c9759cea20d51a.output', { _tag: 'Deny', reason: "/tmp/claude-501/x/tasks/a46c9759cea20d51a.output is a running agent's output that the agent reports when it ends, SendMessage asks it for status" }],
                ['/Users/u/.claude/projects/-x/5f1e.jsonl', { _tag: 'Deny', reason: `/Users/u/.claude/projects/-x/5f1e.jsonl ${transcript}` }],
                ['/Users/u/.claude/projects/-x', { _tag: 'Deny', reason: `/Users/u/.claude/projects/-x ${transcript}` }],
                ['/Users/u/.claude/projects', { _tag: 'Deny', reason: `/Users/u/.claude/projects ${transcript}` }],
                ['/Users/u/.zsh_history', { _tag: 'Deny', reason: "/Users/u/.zsh_history is shell history, atuin search --filter-mode global --search-mode fulltext '<words>' reads it" }],
            ] as const;
            assert.deepStrictEqual(
                yield* Effect.forEach(cases, ([file]) => asked('read', { file })),
                Array.map(cases, ([, decision]) => decision),
            );
        },
        Effect.scoped,
        Effect.provide(Layer.merge(layer, SecureRandom.layerWebCrypto)),
    ),
    30_000,
);

it.live(
    'Nx call that skips the workspace sync runs nx sync first without the flag, in either spelling, noted once',
    Effect.fnUntraced(
        function* () {
            const { decided } = yield* served();
            const cases = [
                ['nx run-many -t check -p web --skip-sync', REWRITTEN('nx sync && nx run-many -t check -p web', '--skip-sync became nx sync before the command, as a skipped sync leaves tsconfig references stale')],
                ['nx build web --skipSync 2>&1 | tail -5', REWRITTEN('set -o pipefail; nx sync && nx build web 2>&1 | tail -5')],
                ['nx build web', ALLOWED],
            ] as const;
            assert.deepStrictEqual(
                yield* Effect.forEach(cases, ([command]) => decided(command, 'foreground')),
                Array.map(cases, ([, decision]) => decision),
            );
        },
        Effect.scoped,
        Effect.provide(Layer.merge(layer, SecureRandom.layerWebCrypto)),
    ),
    30_000,
);

makeMethods(workspaceTest).live.each(WORKSPACE_CALLS)(
    'Call %s in folder %s of an Nx workspace whose only project, in folder suite, holds one test--* target, two clean--* targets, and one continuous serve--* target is decided %j',
    ([command, folder, decision], { workspace }) =>
        servedIn(workspace).pipe(
            Effect.flatMap((decided) => decided(command, folder)),
            Effect.map((got) => assert.deepStrictEqual(got, decision)),
            Effect.scoped,
            Effect.provide(Layer.merge(layer, SecureRandom.layerWebCrypto)),
        ),
    30_000,
);

it.live(
    'Engine whose carapace exports nothing and whose path holds no rg reads flags from the usage another engine stored in its data folder, and judges a fixed sd pattern from the file',
    Effect.fnUntraced(
        function* () {
            const [fs, path, spawner] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
            const [data, cwd, bin] = yield* Effect.all([fs.makeTempDirectoryScoped(), fs.makeTempDirectoryScoped(), fs.makeTempDirectoryScoped()]);
            const carapace = yield* spawner.string(ChildProcess.make('sh', ['-c', 'command -v carapace']));
            yield* fs.writeFileString(path.join(cwd, 'notes.txt'), 'alpha\nbeta\n');
            yield* fs.writeFileString(path.join(bin, 'carapace'), `#!/bin/sh\n[ "$1" = --version ] && exec ${carapace.trim()} --version\nexit 9\n`, { mode: 0o755 });
            const cases = [
                ['grep -rn needle .', REWRITTEN('rg --no-ignore -n needle .', 'grep became rg --no-ignore')],
                ['sd -F gamma delta notes.txt', { _tag: 'Deny', reason: 'sd pattern matches nothing in notes.txt, read the current text' }],
                ['sd -F beta delta notes.txt', ALLOWED],
            ] as const;
            const storing = yield* served({ env: { CLAUDE_PLUGIN_DATA: data } });
            const stored = yield* Effect.forEach(cases, ([command]) => storing.decided(command, 'foreground', { cwd }));
            const reading = yield* served({ env: { CLAUDE_PLUGIN_DATA: data, PATH: `${bin}:/usr/bin:/bin` } });
            const read = yield* Effect.forEach(cases, ([command]) => reading.decided(command, 'foreground', { cwd }));
            const wanted = Array.map(cases, ([, decision]) => decision);
            assert.deepStrictEqual([stored, read], [wanted, wanted]);
        },
        Effect.scoped,
        Effect.provide(Layer.merge(layer, SecureRandom.layerWebCrypto)),
    ),
    60_000,
);

it.live(
    'Nx show piped into jq keeps its stderr out of the JSON',
    Effect.fnUntraced(
        function* () {
            const { decided } = yield* served();
            assert.deepStrictEqual(yield* decided('nx show project rasm --json 2>&1 | jq .name', 'foreground'), { _tag: 'Rewrite', command: 'set -o pipefail; nx show project rasm --json | jq .name', background: false });
        },
        Effect.scoped,
        Effect.provide(Layer.merge(layer, SecureRandom.layerWebCrypto)),
    ),
    30_000,
);

it.live(
    'Agent scripts run, inline code routes JSON reads and file writes, unresolved recursive deletions, scratchpad wipes, awk forms, and listing depths take their own form',
    Effect.fnUntraced(
        function* () {
            const [fs, path, { folder, decided }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, served()]);
            yield* Effect.forEach(Object.entries(FIXTURES), ([name, text]) => fs.writeFileString(path.join(folder, name), text));
            const cases = SCRIPTED(folder);
            assert.deepStrictEqual(
                yield* Effect.forEach(cases, ([command]) => decided(command, 'foreground')),
                Array.map(cases, ([, decision]) => decision),
            );
        },
        Effect.scoped,
        Effect.provide(Layer.merge(layer, SecureRandom.layerWebCrypto)),
    ),
    60_000,
);

it.live(
    'Detached jobs, the shared Nx daemon, and unbounded parallelism of an agent take their own form, and a nested claude --print runs',
    Effect.fnUntraced(
        function* () {
            const { decided } = yield* served();
            assert.deepStrictEqual(
                yield* Effect.forEach(SESSION_CALLS, ([command]) => decided(command, 'foreground', { agent: 'a1' })),
                Array.map(SESSION_CALLS, ([, decision]) => decision),
            );
        },
        Effect.scoped,
        Effect.provide(Layer.merge(layer, SecureRandom.layerWebCrypto)),
    ),
    60_000,
);

it.live(
    'Data-loss git guards hold in a repository outside the session root, where branch reads and branch changes pass',
    Effect.fnUntraced(
        function* () {
            const [fs, path, spawner, { folder, decided }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner, served()]);
            const repo = yield* fs.makeTempDirectoryScoped();
            const file = path.join(repo, 'f');
            const git = (...args: readonly string[]) => spawner.exitCode(ChildProcess.make('git', ['-c', 'user.name=hooks', '-c', 'user.email=hooks@localhost', '-c', 'commit.gpgsign=false', ...args], { cwd: repo }));
            yield* fs.writeFileString(file, 'base\n');
            yield* Effect.forEach(
                [
                    ['init', '-q'],
                    ['add', '.'],
                    ['commit', '-q', '--no-verify', '-m', 'base'],
                ],
                (args) => git(...args),
            );
            yield* fs.writeFileString(file, 'edited\n');
            const cases = [
                [`git -C ${repo} checkout -- f`, { _tag: 'Deny', reason: 'git checkout f overwrites uncommitted edits another session may own, read git diff and undo only your own hunks with your file edit tool' }],
                [`cd ${repo} && git restore f`, { _tag: 'Deny', reason: 'git restore overwrites uncommitted edits another session may own, read git diff and undo only your own hunks with your file edit tool' }],
                [`git -C ${repo} stash`, { _tag: 'Deny', reason: 'git stash hides work, leave the work in the tree' }],
                [`git -C ${repo} reset --hard`, { _tag: 'Deny', reason: 'git reset --hard overwrites uncommitted edits another session may own, read git diff and undo only your own hunks with your file edit tool' }],
                [`git -C ${repo} branch -r`, ALLOWED],
                [`git -C ${repo} checkout -b probe`, ALLOWED],
                [`git -C ${folder} branch -r`, ALLOWED],
                [`git -C ${folder} branch -D probe`, { _tag: 'Deny', reason: 'git branch -D changes branches, stay on this branch' }],
                [`git -C ${repo} checkout-index -f -a`, { _tag: 'Deny', reason: 'git checkout-index -f overwrites uncommitted edits another session may own, read git diff and undo only your own hunks with your file edit tool' }],
                [`git -C ${folder} commit --amend --no-edit`, { _tag: 'Deny', reason: 'git commit --amend rewrites the last commit another session may own, commit forward' }],
                [`git -C ${folder} -c core.hooksPath=/dev/null commit -m probe`, { _tag: 'Deny', reason: 'git commit -c skips the commit hooks, fix what they report' }],
                [`git -C ${folder} commit --no-verify -m probe`, { _tag: 'Rewrite', command: `git -C ${folder} commit -m probe`, background: false, note: 'git commit --no-verify removed' }],
                [`git -C ${folder} commit -nm probe`, { _tag: 'Rewrite', command: `git -C ${folder} commit -m probe`, background: false, note: 'git commit -nm became -m' }],
            ] as const;
            assert.deepStrictEqual(
                yield* Effect.forEach(cases, ([command]) => decided(command, 'foreground')),
                Array.map(cases, ([, decision]) => decision),
            );
        },
        Effect.scoped,
        Effect.provide(Layer.merge(layer, SecureRandom.layerWebCrypto)),
    ),
    30_000,
);
