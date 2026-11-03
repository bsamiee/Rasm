import { NodeServices } from '@effect/platform-node';
import { describe, expect, layer } from '@effect/vitest';
import { commandDecision, none, type Request, type System, some } from '@rasm/function-hooks';
import { Config, Duration, Effect, FileSystem, Path, Stream } from 'effect';
import { ChildProcess } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = new URL('../../../..', import.meta.url).pathname.replace(/\/$/u, '');
const DEADLINE = Duration.seconds(600);
const FOREGROUND: Request = { kind: 'foreground', timeout: none };
const BACKGROUND: Request = { kind: 'background' };
const MONITOR: Request = { kind: 'monitor' };
const PROJECTS: Readonly<Record<string, readonly string[]>> = { '.': ['rasm'], 'plugins/function-hooks': ['@rasm/function-hooks'], 'rasm-mcp': ['rasm-mcp'] };
const TARGETS: Readonly<Record<string, object>> = {
    rasm: {
        'lint:biome': { options: { command: 'biome check --error-on-warnings' } },
        'format:biome': { options: { command: 'biome check --write --error-on-warnings --no-errors-on-unmatched' } },
        'lint:ruff': { dependsOn: [{ target: 'lint:ruff:*', params: 'forward' }] },
        'lint:ruff:check': { options: { command: 'ruff check' } },
        'lint:ruff:format': { options: { command: 'ruff format --check' } },
        'format:ruff': { options: { commands: ['ruff check --fix', 'ruff format'] } },
        'lint:ast-grep': { dependsOn: [{ target: 'lint:ast-grep:tree', params: 'forward' }, 'lint:ast-grep:root'] },
        'lint:ast-grep:tree': { options: { command: "ast-grep scan --no-ignore hidden --globs '!pnpm-lock.yaml'" } },
        'lint:ast-grep:root': { options: { command: "ast-grep scan --filter '^(no-chain-flattening)$'" } },
        'typecheck:tsc': { options: { command: 'tsc --pretty false' } },
    },
    '@rasm/function-hooks': { 'typecheck:tsc': { options: { command: 'tsc --pretty false' } } },
    'rasm-mcp': { 'typecheck:clippy': { options: { command: 'cargo clippy --locked --no-deps --package rasm-mcp' } } },
};
const REWRITES = [
    ['timeout 600 cargo clippy -p rasm-mcp 2>&1 | tail -5', 'set -o pipefail; nx run rasm-mcp:typecheck:clippy 2>&1 | tail -5'],
    ['timeout -k 5 2m nx run rasm:check', 'nx run rasm:check'],
    ['biome check plugins/function-hooks', 'nx run rasm:lint:biome -- plugins/function-hooks'],
    ['biome check --write plugins/function-hooks', 'nx run rasm:format:biome -- plugins/function-hooks'],
    ['ruff check tools/refurb', 'nx run rasm:lint:ruff -- tools/refurb'],
    ['ruff format tools/refurb', 'nx run rasm:format:ruff -- tools/refurb'],
    ['ast-grep scan plugins', 'nx run rasm:lint:ast-grep -- plugins'],
    ['node_modules/.bin/tsc -p plugins/function-hooks/tsconfig.json --noEmit --pretty false', 'nx run @rasm/function-hooks:typecheck:tsc -- --noEmit'],
    ["sd -F '- text' 'new' notes.md", "sd -F -- '- text' 'new' notes.md"],
    ['open -n -g -a Blender scene.blend', 'watch-apps launch Blender --new --document scene.blend'],
    [`osascript -e 'tell application "Adobe Illustrator" to activate'`, "watch-apps launch 'Adobe Illustrator'"],
    ['tree -L 2 --dirsfirst --noreport plugins', 'tree -L 2 --group-directories-first plugins'],
    ['nx run rasm:typecheck:mypy --skip-nx-cache', 'nx run rasm:typecheck:mypy'],
    ['nx affected -t check --skipNxCache', 'nx affected -t check'],
    ['fd -e ts policies plugins', 'fd --hidden -e ts policies plugins'],
    ['rg -n Verdict plugins', 'rg --hidden -n Verdict plugins'],
    ['pgrep -f analyzeHeadless', "pgrep -f '[a]nalyzeHeadless'"],
    ['pgrep -lf "ghidra.app.util"', "pgrep -lf '[g]hidra.app.util'"],
    ['ps -axo pid,command | grep java', "ps -axo pid,command | grep '[j]ava'"],
    ['nx run rasm:check 2>&1 | tail -20', 'set -o pipefail; nx run rasm:check 2>&1 | tail -20'],
    ['cargo test -p rasm-cli | rg FAILED', 'set -o pipefail; cargo test -p rasm-cli | rg --hidden FAILED'],
    ['caffeinate -i nx run rasm:check', 'nx run rasm:check'],
    ['ls; caffeinate -t 5; ls', 'ls; sleep 5; ls'],
    ['ls; read -t 1 x < <(tail -f /dev/null); ls', 'ls; sleep 1; ls'],
] as const;
const DECISIONS = [
    ['trailing & becomes run_in_background', 'nx run portfolio:serve &', FOREGROUND, false, { kind: 'rewrite', command: 'nx run portfolio:serve ', background: true, notice: some('& backgrounded the call. Wait for its completion notice') }],
    ['trailing & drops in a background call', 'nx run portfolio:serve &', BACKGROUND, false, { kind: 'rewrite', command: 'nx run portfolio:serve ', background: false }],
    ['missing timeout becomes call timeout', `timeout ${Duration.toSeconds(DEADLINE)} nx run rasm:check`, FOREGROUND, false, { kind: 'rewrite', command: 'nx run rasm:check', timeout: some(Duration.toMillis(DEADLINE)), notice: some(`timeout is not installed. Call timeout set to ${Duration.toSeconds(DEADLINE)}`) }],
    ['installed timeout stays', `timeout ${Duration.toSeconds(DEADLINE)} nx run rasm:check`, FOREGROUND, true, { kind: 'allow' }],
    [
        'analyzeHeadless runs in background, logs under .artifacts/logs, and drops heap overrides',
        'GHIDRA_HEADLESS_MAXMEM=16G "$GHIDRA_INSTALL_DIR/support/analyzeHeadless" "$GHIDRA_PROJECT_DIR" name -process x',
        FOREGROUND,
        false,
        { kind: 'rewrite', command: `"$GHIDRA_INSTALL_DIR/support/analyzeHeadless" "$GHIDRA_PROJECT_DIR" name -process x -log ${ROOT}/.artifacts/logs/ghidra.log -scriptlog ${ROOT}/.artifacts/logs/ghidra-script.log`, background: true },
    ],
    ['tracked script files stay unparsed', 'bash .claude/skills/manage-repo/scripts/maintain.sh --help', FOREGROUND, false, { kind: 'allow' }],
] as const;
const DENIALS = [
    ["cat > /tmp/probe.py <<'EOF'\nprint(1)\nEOF\npython3 /tmp/probe.py", 'python3 runs inline code'],
    ["python3 - <<'EOF'\nprint(1)\nEOF", 'python3 runs inline code'],
    ['until test -f done; do sleep 3; done', 'Poll loop holds the turn. Run it in Monitor'],
    ['while pgrep -f x >/dev/null; do sleep 5; done', 'Poll loop holds the turn'],
    ['while kill -0 123; do read -t 5 v < <(tail -f /dev/null); done', 'Poll loop holds the turn'],
    ['while kill -0 123 2>/dev/null; do sleep 1; done', 'Poll loop holds the turn'],
    ['until ! ps -p 123 >/dev/null; do sleep 1; done', 'Poll loop holds the turn'],
    ['for i in 1 2 3; do curl -sf localhost:3000 && break; sleep 2; done', 'Poll loop holds the turn'],
    ['sleep 240; grep -c done build.log', 'sleep waits blindly. Use Monitor or task notification'],
    ['cd /tmp; /bin/sleep 60', 'sleep waits blindly'],
    ['caffeinate -i -t 300', 'caffeinate waits blindly'],
    ['read -t 5 x < <(tail -f /dev/null)', 'read waits blindly'],
    ['bash -c "sleep 30; ls"', 'sleep waits blindly'],
    ['tail -n +1 -f build.log | grep -m 1 Finished', 'tail -f can wait forever. Run it in Monitor'],
    ['wait4path /Volumes/Build', 'wait4path can wait forever. Run it in Monitor'],
    ['caffeinate -w 123', 'caffeinate can wait forever'],
    ['cd /tmp && W=/private/tmp/x; rm -rf $W/p', 'rm $W/p expands at run time'],
    ['a || W=/private/tmp/x && rm -rf $W/p', 'rm $W/p expands at run time'],
    [`osascript -e 'tell application "Finder" to make new folder'`, 'osascript drives Finder. Use watch-apps perform or atelier'],
    [`osascript -e 'tell application "System Events" to keystroke "a"'`, 'System Events hits frontmost app'],
    ['osascript probe.applescript', 'osascript probe.applescript is opaque'],
    ['timeout 600 nx run rasm:check & wait; bash -c "timeout 5 ls"', 'timeout is not installed. Drop it'],
    ['GHIDRA_MAXMEM=4G bash -c "echo"; bash -c "GHIDRA_MAXMEM=4G echo"', 'GHIDRA_MAXMEM overrides the analyzeHeadless heap'],
    ["bash -c 'read -t 5 x < <(tail -f /dev/null)'", 'read -t leaks its input process. Use sleep 5'],
    ["bash -c 'caffeinate -t 5'", 'caffeinate -t 5 waits. Use sleep 5'],
    ["bash -c 'pgrep -f foo'", "pgrep foo matches its own shell. Use '[f]oo'"],
    ["sd -F 'abc' 'abc' notes.md", 'sd replaces abc with itself'],
    ["sd '' x notes.md", 'sd empty pattern matches everywhere'],
    ['open -W -a Blender scene.blend', 'open -W launches Blender'],
    ['S=$(pwd); rm -rf $S/build', 'rm $S/build expands at run time'],
    ['rm -rf .', 'covers repository root'],
    ['pkill -f analyzeHeadless', 'pkill ends every analyzeHeadless process. Kill your own pid'],
    ['killall Rhino', 'killall ends every Rhino process'],
    ['for p in a b; do nx run $p:test; done', 'Loop reruns nx tasks'],
    ['ls libs | xargs -I{} nx run {}:check', 'Loop reruns nx tasks'],
    ['sleep 5 & echo started', '& job outlives the call'],
    ['"$GHIDRA_INSTALL_DIR/support/analyzeHeadless" /tmp/p name -process x', 'analyzeHeadless project is not $GHIDRA_PROJECT_DIR'],
    ['for b in a b; do analyzeHeadless "$GHIDRA_PROJECT_DIR" "$b" -process x; done', 'analyzeHeadless fans out per input'],
    ['export GHIDRA_HEADLESS_MAXMEM=4096M; node prove.ts', 'GHIDRA_HEADLESS_MAXMEM overrides the analyzeHeadless heap. Drop it'],
    ['fd -e plist Info ~', 'is too wide'],
    ['git reset --hard', 'git reset --hard discards working-tree or index state'],
    ['git stash', 'git stash hides uncommitted work'],
    ['cat', 'cat waits on unfed stdin'],
] as const;
const SCRIPTS = [
    ['wait.sh', 'until ! pgrep -f analyzeHeadless >/dev/null; do /bin/sleep 5; done\n', 'Poll loop holds the turn'],
    ['run.sh', 'project="$PWD/ghidra/project"\n"$GHIDRA_INSTALL_DIR/support/analyzeHeadless" "$project" color -process "$1"\n', 'analyzeHeadless project is not $GHIDRA_PROJECT_DIR'],
] as const;
const ALLOWANCES = [
    ['S=/private/tmp/scratchpad; rm -rf $S/build', FOREGROUND],
    ['sed -n 1,3p <(git show HEAD:README.md)', FOREGROUND],
    ['echo a | grep x | tail -3 && ls', FOREGROUND],
    ['/Applications/Blender.app/Contents/MacOS/Blender -b scene.blend --python render.py', FOREGROUND],
    ['tsc -p docs/research/probe', FOREGROUND],
    ['ruff check --select F401 tools', FOREGROUND],
    ['ast-grep scan --json plugins', FOREGROUND],
    ['while read -r line; do echo "$line"; done < notes.md', FOREGROUND],
    ['for f in a b; do wc -l "$f"; done', FOREGROUND],
    ['nx show project rasm --json', FOREGROUND],
    ['nx run rasm:check 2>&1 | head -20', FOREGROUND],
    ['set -euo pipefail; nx run rasm:check | tail -5', FOREGROUND],
    ['git log --oneline | tail -3', FOREGROUND],
    ['tail -n 20 build.log', FOREGROUND],
    ['pgrep analyzeHeadless', FOREGROUND],
    ['pgrep -f "$pattern"', FOREGROUND],
    ['ps aux | grep -c java', FOREGROUND],
    ['nx run portfolio:serve & wait', FOREGROUND],
    ['fd -H foo plugins', FOREGROUND],
    ['git status', FOREGROUND],
    ['cd /tmp; ls; sleep 2; ls', FOREGROUND],
    ['awk \'BEGIN{printf "%.3f\\n", 6.28/2}\'', FOREGROUND],
    ['cd /tmp && W=/private/tmp/x && rm -rf $W/p', FOREGROUND],
    ['S=/private/tmp/s; C=$S/c; rm -rf $C', FOREGROUND],
    ['GHIDRA_INSTALL_DIR=/opt/ghidra ls', FOREGROUND],
    ['nx run rasm-views:check', FOREGROUND],
    ['for f in a.md b.md; do wc -l "$f"; done; nx run rasm-views:check', FOREGROUND],
    ['until test -f done; do sleep 3; done', MONITOR],
    ['tail -f build.log | grep --line-buffered error', MONITOR],
    ['while kill -0 123 2>/dev/null; do sleep 1; done', MONITOR],
    ['until test -f done; do sleep 3; done', BACKGROUND],
    ['until ! ps -p 123 >/dev/null; do sleep 1; done', BACKGROUND],
    ['sleep 30; echo done', BACKGROUND],
] as const;

// --- [SERVICES] ------------------------------------------------------------------------

const hosted = (installed: boolean) =>
    Effect.gen(function* () {
        const [fs, path, home, context] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String('HOME'), Effect.context<NodeServices.NodeServices>()]);
        const run = Effect.runPromiseWith(context);
        const plugin = path.join(ROOT, 'plugins', 'function-hooks');
        const spawned: System['run'] = ([file, ...rest], { cwd, stdin }) =>
            run(
                ChildProcess.make(file, rest, { cwd, stdin: Stream.encodeText(Stream.make(stdin)) }).pipe(
                    Effect.flatMap((handle) => Effect.all({ stdout: Stream.mkString(Stream.decodeText(handle.stdout)), stderr: Stream.mkString(Stream.decodeText(handle.stderr)), exitCode: handle.exitCode }, { concurrency: 'unbounded' })),
                    Effect.scoped,
                ),
            );
        const spawn: System['run'] = (argv, init) => {
            const [file, , , tool, , subject, value] = argv;
            const listed = subject === 'projects' ? (PROJECTS[String(argv.at(-1))] ?? []) : { targets: TARGETS[String(value)] ?? {} };
            const stubs: Readonly<Record<string, boolean>> = { timeout: true, mise: tool === 'nx' };
            const output = file === 'timeout' ? '' : JSON.stringify(listed);
            return stubs[file] === true ? run(Effect.succeed({ exitCode: Number(file === 'timeout' && !installed), stderr: '', stdout: output })) : spawned(argv, init);
        };
        const scanner = (yield* Effect.promise(() => spawned(['mise', 'which', 'ast-grep'], { cwd: plugin, stdin: '' }))).stdout.trim();
        const folder = yield* fs.makeTempDirectoryScoped();
        return {
            write: (name: string, text: string) => Effect.as(fs.writeFileString(path.join(folder, name), text), path.join(folder, name)),
            decide: (command: string, request: Request) => Effect.promise(() => commandDecision({ run: spawn, exists: (entry) => run(fs.exists(entry)), read: (entry) => run(fs.readFileString(entry)), scanner, plugin, root: some(ROOT), cwd: ROOT, home: some(home) }, command, 'rasm', some(request))),
        };
    });

// --- [OPERATIONS] ----------------------------------------------------------------------

const decided = (command: string, request: Request, installed: boolean, expected: object) =>
    Effect.gen(function* () {
        const { decide } = yield* hosted(installed);
        expect(yield* decide(command, request)).toMatchObject(expected);
    });

const rewritten = ([command, text]: readonly [string, string]) => decided(command, FOREGROUND, false, { kind: 'rewrite', command: text, background: false });

const refused = ([command, reason]: readonly [string, string]) => decided(command, FOREGROUND, false, { kind: 'deny', reason: expect.stringContaining(reason) });

const allowed = ([command, request]: readonly [string, Request]) => decided(command, request, false, { kind: 'allow' });

const scripted = ([name, text, reason]: readonly [string, string, string]) =>
    Effect.gen(function* () {
        const { decide, write } = yield* hosted(false);
        expect(yield* decide(`bash ${yield* write(name, text)}`, FOREGROUND)).toMatchObject({ kind: 'deny', reason: expect.stringContaining(reason) });
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(NodeServices.layer, { timeout: 60_000 })('function-hooks', (it) => {
    describe('rewrites', () => {
        it.effect.each(REWRITES)('%s', rewritten);
        it.effect.each(DECISIONS)('%s', ([, command, request, installed, expected]) => decided(command, request, installed, expected));
    });
    describe('denials', () => {
        it.effect.each(DENIALS)('%s', refused);
        it.effect.each(SCRIPTS)('script file %s', scripted);
    });
    describe('allowances', () => {
        it.effect.each(ALLOWANCES)('%s', allowed);
    });
});
