import { availableParallelism } from 'node:os';
import process from 'node:process';
import { fileURLToPath } from 'node:url';
import { layerUndiciOptions } from '@effect/platform-node/NodeHttpClient';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { Cause, Effect, FileSystem, Layer, Path, Schema, Stream } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { HttpClient, HttpClientRequest, HttpClientResponse } from 'effect/http';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';

// --- [TYPES] ---------------------------------------------------------------------------

type Decision = typeof Decision.Type;

// --- [CONSTANTS] -----------------------------------------------------------------------

const SERVER = fileURLToPath(import.meta.resolve('@rasm/function-hooks/server'));
const HOOK = fileURLToPath(new URL('../hook.ts', import.meta.resolve('@rasm/function-hooks/server')));
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
] as const;
const HOLDERS = {
    writer: (output: string) => ChildProcess.make('sh', ['-c', 'exec sleep 60 >> "$0"', output]),
    reader: (output: string) => ChildProcess.make('tail', ['-f', output], { stdout: 'ignore' }),
} as const;
const AGGREGATES = [
    ['nx test @rasm/test-function-hooks -- -t decide', { _tag: 'Rewrite', command: 'nx run @rasm/test-function-hooks:test--vitest -- -t decide', background: false, note: 'nx test became nx run @rasm/test-function-hooks:test--vitest -- -t decide' }],
    ['nx clean @rasm/test-function-hooks -- --verbose', { _tag: 'Rewrite', command: 'nx run @rasm/test-function-hooks:clean--vitest -- --verbose', background: false, note: 'nx clean became nx run @rasm/test-function-hooks:clean--vitest -- --verbose' }],
] as const;
const CORES = availableParallelism();
const REWRITTEN = (command: string, note?: string) => ({ _tag: 'Rewrite', command, background: false, ...(note === undefined ? {} : { note }) }) as const;
const SESSION_CALLS = [
    ['launchctl submit -l probe -- /bin/sleep 9', { _tag: 'Deny', reason: 'launchctl submit detached jobs outlive the session; use run_in_background and TaskStop, or a mise [daemons] row' }],
    ['claude -p hi', { _tag: 'Deny', reason: 'claude --print nests a session inside an agent, spawn an agent with the Agent tool' }],
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

const INLINE = (program: string, where = 'inline code') => ({ _tag: 'Deny', reason: `${program} runs ${where} no rule reads, answer it with a CLI (rg, ast-grep, jq, yq, duckdb) or an nx target, as a script file written for it is judged the same` }) as const;
const UNCOMMITTED = (file: string) => ({ _tag: 'Deny', reason: `${file} holds uncommitted edits another session may own, change it with your file edit tool` }) as const;
const FIXTURES = {
    'probe.py': 'print(open("a.txt").read())\n',
    'probe.mjs': 'console.log(1);\n',
    'probe.swift': 'print(1)\n',
    'probe.sh': "python3 -c 'print(1)'\n",
    'version.js': "console.log(require('effect/package.json').version)\n",
    'plain.py': 'print(1)\n',
    'scene.py': 'import bpy\nprint(bpy.app.version)\n',
} as const;
const SCRIPTED = (scratch: string, scratchpad = '/private/tmp/claude-501/-Users-u-Developer-Rasm/0d9c0b52-46aa-4e5f-9a3c-1f6f0c2b7e11/scratchpad') =>
    [
        [`python3 ${scratch}/probe.py`, INLINE('python3', `${scratch}/probe.py`)],
        [`uv run --no-project ${scratch}/probe.py`, INLINE('python', `${scratch}/probe.py`)],
        [`uv run --with httpx ${scratch}/probe.py`, INLINE('python', `${scratch}/probe.py`)],
        [`uv run --script ${scratch}/probe.py`, INLINE('python', `${scratch}/probe.py`)],
        [`uvx --from httpx python ${scratch}/probe.py`, INLINE('python', `${scratch}/probe.py`)],
        [`${scratch}/probe.py`, INLINE('python', `${scratch}/probe.py`)],
        [`node ${scratch}/probe.mjs`, INLINE('node', `${scratch}/probe.mjs`)],
        [`bun ${scratch}/probe.mjs`, INLINE('bun', `${scratch}/probe.mjs`)],
        [`bun run ${scratch}/probe.mjs`, INLINE('bun', `${scratch}/probe.mjs`)],
        [`tsx ${scratch}/probe.mjs`, INLINE('tsx', `${scratch}/probe.mjs`)],
        [`deno run ${scratch}/probe.mjs`, INLINE('deno', `${scratch}/probe.mjs`)],
        [`swift ${scratch}/probe.swift`, INLINE('swift', `${scratch}/probe.swift`)],
        [`swift -e 'print(1)'`, INLINE('swift')],
        [`bash ${scratch}/probe.sh`, INLINE('python3')],
        [`sh ${scratch}/probe.sh`, INLINE('python3')],
        [`source ${scratch}/probe.sh`, INLINE('python3')],
        [`. ${scratch}/probe.sh`, INLINE('python3')],
        [`cat > ${scratch}/written.sh <<'EOF'\npython3 -c 'print(2)'\nEOF\nbash ${scratch}/written.sh`, INLINE('python3')],
        [`blender --background --python ${scratch}/plain.py`, INLINE('blender', `${scratch}/plain.py`)],
        [`node ${scratch}/version.js`, { _tag: 'Rewrite', command: 'jq -r .version node_modules/effect/package.json', background: false }],
        [`osascript -e 'delay 240'`, BARE_TIMER],
        [`swift -e 'Thread.sleep(forTimeInterval: 240)'`, BARE_TIMER],
        [`node ${scratch}/repo/probe.mjs`, INLINE('node', `${scratch}/repo/probe.mjs`)],
        [`node ${scratch}/repo/tool.mjs`, ALLOWED],
        [`node ${scratch}/repo/ignored/generated.mjs`, ALLOWED],
        [`cp ${scratch}/note.txt ${scratch}/repo/dirty.txt`, UNCOMMITTED(`${scratch}/repo/dirty.txt`)],
        [`ditto ${scratch}/note.txt ${scratch}/repo/dirty.txt`, UNCOMMITTED(`${scratch}/repo/dirty.txt`)],
        [`echo hi > ${scratch}/repo/dirty.txt`, UNCOMMITTED(`${scratch}/repo/dirty.txt`)],
        [`cat ${scratch}/note.txt | tee ${scratch}/repo/dirty.txt`, UNCOMMITTED(`${scratch}/repo/dirty.txt`)],
        [`cp ${scratch}/note.txt ${scratch}/repo/sub`, UNCOMMITTED(`${scratch}/repo/sub/note.txt`)],
        [`echo hi >> ${scratch}/repo/dirty.txt`, ALLOWED],
        [`cp ${scratch}/note.txt ${scratch}/repo/clean.txt`, ALLOWED],
        [`cp ${scratch}/note.txt ${scratch}/repo/fresh.txt`, ALLOWED],
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
        [`cat ${scratch}/note.txt | tee -a ${scratch}/repo/dirty.txt`, ALLOWED],
        [`rm -rf ${scratchpad}/*`, { _tag: 'Deny', reason: `rm ${scratchpad}/* empties the scratchpad the session and every subagent share, delete only the subfolder you made by name` }],
        [`trash ${scratchpad}`, { _tag: 'Deny', reason: `trash ${scratchpad} empties the scratchpad the session and every subagent share, delete only the subfolder you made by name` }],
        [`fd --hidden -t f . ${scratchpad} -X rm`, { _tag: 'Deny', reason: `rm ${scratchpad} empties the scratchpad the session and every subagent share, delete only the subfolder you made by name` }],
        [`rm -rf ${scratchpad}/rebuild`, ALLOWED],
        ['fd --hidden -d 1 react node_modules/.pnpm', { _tag: 'Rewrite', command: 'fd --no-ignore --hidden -d 1 react node_modules/.pnpm', background: false }],
        [`rm -rf ${scratchpad.slice(0, scratchpad.indexOf('/-'))}/tmp.Xk2v`, ALLOWED],
        ['fd --hidden -d 2 react /usr', ALLOWED],
        ['fd --hidden -d 3 react /usr', { _tag: 'Deny', reason: 'fd root /usr too wide, pass a package folder or a depth of at most 2' }],
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
const Json = Schema.fromJsonString(Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

const served = Effect.fnUntraced(function* (env: Readonly<Record<string, string>> = {}) {
    const [fs, path, spawner] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
    const folder = yield* fs.makeTempDirectoryScoped();
    const socket = path.join(folder, 'decide.sock');
    const server = yield* spawner.spawn(ChildProcess.make(process.execPath, [SERVER, socket], { env, extendEnv: true }));
    yield* Effect.raceFirst(
        Stream.runHead(Stream.filter(Stream.splitLines(Stream.decodeText(server.stdout)), (line) => line === socket)),
        Effect.flatMap(server.exitCode, (code) => Effect.fail(new Cause.UnknownError(code, `${SERVER} exits ${code} before it listens on ${socket}`))),
    );
    const decided = (command: string, kind: 'foreground' | 'background' | 'monitor', stated: { readonly cwd?: string; readonly agent?: string } = {}) =>
        SecureRandom.SecureRandom.use((random) => random.uuidV4()).pipe(
            Effect.flatMap((call) => HttpClient.execute(HttpClientRequest.bodyJsonUnsafe(HttpClientRequest.post('http://function-hooks/decide'), { command, cwd: folder, kind, session: 'session', call, ...stated }))),
            Effect.flatMap(HttpClientResponse.matchStatus({ '2xx': ({ json }) => json, orElse: ({ status, text }) => Effect.flatMap(text, (body) => Effect.fail(new Cause.UnknownError(body, `decide answers ${status}, ${body}`))) })),
            Effect.flatMap(Schema.decodeUnknownEffect(Decision)),
            Effect.provide(layerUndiciOptions({ connect: { socketPath: socket } })),
        );
    return { folder, decided };
});

const held = Effect.fnUntraced(function* (holder: keyof typeof HOLDERS) {
    const [fs, path, spawner, { folder, decided }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner, served()]);
    const output = path.join(folder, 'tasks', 'b7k2.output');
    yield* fs.makeDirectory(path.dirname(output));
    yield* fs.writeFileString(output, '');
    return { output, holding: yield* spawner.spawn(HOLDERS[holder](output)), decided };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.live(
    'Monitor tail of a written task output waits on its writer, and the rewrite decides to itself',
    Effect.fnUntraced(
        function* () {
            const [fs, spawner, { output, holding, decided }] = yield* Effect.all([FileSystem.FileSystem, ChildProcessSpawner.ChildProcessSpawner, held('writer')]);
            const bounded = yield* decided(`tail -f ${output}`, 'monitor');
            assert(bounded._tag === 'Rewrite', `tail -f ${output} decides ${bounded._tag}`);
            assert.deepStrictEqual<readonly Decision[]>([bounded, yield* decided(bounded.command, 'monitor')], [{ _tag: 'Rewrite', command: bounded.command, background: false }, ALLOWED]);
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
            assert.deepStrictEqual<readonly Decision[]>([yield* decided(`cat ${output}`, 'foreground'), yield* decided(`cat ${output}`, 'foreground')], decisions(output));
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
            assert.deepStrictEqual<readonly Decision[]>(
                decisions,
                WAITS.map((wait) => wait[2]),
            );
        },
        Effect.scoped,
        Effect.provide(Layer.merge(layer, SecureRandom.layerWebCrypto)),
    ),
    60_000,
);

it.live.each(AGGREGATES)(
    'Nx aggregate call %s with one-run arguments runs the one target its <verb>--* glob matches, %j',
    Effect.fnUntraced(
        function* ([command, decision]) {
            const { decided } = yield* served();
            assert.deepStrictEqual<Decision>(yield* decided(command, 'foreground', { cwd: import.meta.dirname }), decision);
        },
        Effect.scoped,
        Effect.provide(Layer.merge(layer, SecureRandom.layerWebCrypto)),
    ),
    30_000,
);

it.live(
    'Nx show piped into jq keeps its stderr out of the JSON',
    Effect.fnUntraced(
        function* () {
            const { decided } = yield* served();
            assert.deepStrictEqual<Decision>(yield* decided('nx show project rasm --json 2>&1 | jq .name', 'foreground'), { _tag: 'Rewrite', command: 'set -o pipefail; nx show project rasm --json | jq .name', background: false });
        },
        Effect.scoped,
        Effect.provide(Layer.merge(layer, SecureRandom.layerWebCrypto)),
    ),
    30_000,
);

it.live(
    'Scripts an agent wrote, writes over uncommitted work, scratchpad wipes, awk forms, and listing depths decide alike on every route',
    Effect.fnUntraced(
        function* () {
            const [fs, path, spawner, { folder, decided }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner, served()]);
            const written = (files: Readonly<Record<string, string>>) => Effect.forEach(Object.entries(files), ([name, text]) => fs.writeFileString(path.join(folder, name), text));
            const git = (...args: readonly string[]) => spawner.exitCode(ChildProcess.make('git', ['-c', 'user.name=hooks', '-c', 'user.email=hooks@localhost', '-c', 'commit.gpgsign=false', ...args], { cwd: path.join(folder, 'repo') }));
            yield* Effect.forEach(['repo/sub', 'repo/ignored'], (name) => fs.makeDirectory(path.join(folder, name), { recursive: true }));
            yield* written({ ...FIXTURES, 'repo/.gitignore': 'ignored/\n', 'repo/dirty.txt': 'base\n', 'repo/clean.txt': 'base\n', 'repo/tool.mjs': 'console.log(1);\n', 'repo/sub/note.txt': 'base\n' });
            yield* Effect.forEach(
                [
                    ['init', '-q'],
                    ['add', '.'],
                    ['commit', '-q', '--no-verify', '-m', 'base'],
                ],
                (args) => git(...args),
            );
            yield* written({ 'repo/dirty.txt': 'edited\n', 'repo/sub/note.txt': 'edited\n', 'repo/probe.mjs': 'console.log(1);\n', 'repo/ignored/generated.mjs': 'console.log(1);\n' });
            const cases = SCRIPTED(folder);
            assert.deepStrictEqual<readonly Decision[]>(
                yield* Effect.forEach(cases, ([command]) => decided(command, 'foreground')),
                cases.map(([, decision]) => decision),
            );
        },
        Effect.scoped,
        Effect.provide(Layer.merge(layer, SecureRandom.layerWebCrypto)),
    ),
    60_000,
);

it.live(
    'Detached jobs, nested sessions, hidden browser heads, the shared Nx daemon, and unbounded parallelism of an agent take their own form',
    Effect.fnUntraced(
        function* () {
            const { decided } = yield* served();
            assert.deepStrictEqual<readonly Decision[]>(
                yield* Effect.forEach(SESSION_CALLS, ([command]) => decided(command, 'foreground', { agent: 'a1' })),
                SESSION_CALLS.map(([, decision]) => decision),
            );
            assert.deepStrictEqual<Decision>(yield* decided('claude -p hi', 'foreground'), ALLOWED);
        },
        Effect.scoped,
        Effect.provide(Layer.merge(layer, SecureRandom.layerWebCrypto)),
    ),
    60_000,
);

it.live(
    'Codex apply_patch of a script outside the root that a rule rewrites is refused with the rule reason, never the whole file as a command',
    Effect.fnUntraced(
        function* () {
            const [fs, path, spawner] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
            const [cwd, elsewhere] = yield* Effect.all([fs.makeTempDirectoryScoped(), fs.makeTempDirectoryScoped()]);
            const file = path.join(elsewhere, 'version.js');
            const call = { session_id: 'session', cwd, tool_name: 'apply_patch', tool_input: { command: `*** Begin Patch\n*** Add File: ${file}\n+console.log(require('effect/package.json').version)\n*** End Patch\n` } };
            const hook = spawner.spawn(ChildProcess.make(process.execPath, [HOOK, 'codex'], { stdin: Stream.encodeText(Stream.make(yield* Schema.encodeEffect(Json)(call))) }));
            const printed = yield* Effect.flatMap(hook, (running) => Stream.mkString(Stream.decodeText(running.stdout)));
            assert.deepStrictEqual(yield* Schema.decodeEffect(Json)(printed), { hookSpecificOutput: { hookEventName: 'PreToolUse', permissionDecision: 'deny', permissionDecisionReason: `${file}: Nested node inline code, run jq -r .version node_modules/effect/package.json` } });
        },
        Effect.scoped,
        Effect.provide(layer),
    ),
    30_000,
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
                [`git -C ${repo} show HEAD:f > ${file}`, UNCOMMITTED(file)],
                [`git -C ${repo} show HEAD:f > ${path.join(folder, 'f')}`, ALLOWED],
                [`git -C ${repo} branch -r`, ALLOWED],
                [`git -C ${repo} checkout -b probe`, ALLOWED],
                [`git -C ${folder} branch -r`, ALLOWED],
                [`git -C ${folder} branch -D probe`, { _tag: 'Deny', reason: 'git branch -D changes branches, stay on this branch' }],
                [`git -C ${repo} checkout-index -f -a`, { _tag: 'Deny', reason: 'git checkout-index -f overwrites uncommitted edits another session may own, read git diff and undo only your own hunks with your file edit tool' }],
                [`for f in a b; do git show HEAD:$f > ${folder}/$f; done`, { _tag: 'Deny', reason: `${folder}/$f expands later, so no check can see whether it holds uncommitted edits, write each file by its literal path` }],
                [`git -C ${folder} commit --amend --no-edit`, { _tag: 'Deny', reason: 'git commit --amend rewrites the last commit another session may own, commit forward' }],
                [`git -C ${folder} -c core.hooksPath=/dev/null commit -m probe`, { _tag: 'Deny', reason: 'git commit -c skips the commit hooks, fix what they report' }],
            ] as const;
            assert.deepStrictEqual<readonly Decision[]>(
                yield* Effect.forEach(cases, ([command]) => decided(command, 'foreground')),
                cases.map(([, decision]) => decision),
            );
        },
        Effect.scoped,
        Effect.provide(Layer.merge(layer, SecureRandom.layerWebCrypto)),
    ),
    30_000,
);
