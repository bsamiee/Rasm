import { NodeServices } from '@effect/platform-node';
import { describe, expect, layer } from '@effect/vitest';
import { commandDecision, none, type Request, type System, some } from '@rasm/function-hooks';
import { Config, Duration, Effect, FileSystem, Path, Stream } from 'effect';
import { ChildProcess } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = new URL('../../../..', import.meta.url).pathname.replace(/\/$/u, '');
const DEADLINE: Duration.Input = '600 seconds';
const BLIND = 'Command waits blindly. Use Monitor or run_in_background';
const FOREGROUND: Request = { kind: 'foreground', timeout: none };
const BACKGROUND: Request = { kind: 'background' };
const MONITOR: Request = { kind: 'monitor' };

// --- [SERVICES] ------------------------------------------------------------------------

const hosted = Effect.fn('hosted')(function* (installed: boolean) {
    const [fs, path, home, context] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String('HOME'), Effect.context<NodeServices.NodeServices>()]);
    const run = Effect.runPromiseWith(context);
    const plugin = path.join(ROOT, 'plugins', 'function-hooks');
    const projects: Readonly<Record<string, readonly string[]>> = { '.': ['rasm'], 'plugins/function-hooks': ['@rasm/function-hooks'], 'rasm-mcp': ['rasm-mcp'] };
    const targets: Readonly<Record<string, object>> = {
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
    const spawned: System['run'] = ([file, ...rest], { cwd, stdin }) =>
        run(
            ChildProcess.make(file, rest, { cwd, stdin: Stream.encodeText(Stream.make(stdin)) }).pipe(
                Effect.flatMap((handle) => Effect.all({ stdout: Stream.mkString(Stream.decodeText(handle.stdout)), stderr: Stream.mkString(Stream.decodeText(handle.stderr)), exitCode: handle.exitCode }, { concurrency: 'unbounded' })),
                Effect.scoped,
            ),
        );
    const spawn: System['run'] = (argv, init) => {
        const [file, , , tool, , subject, value] = argv;
        const listed = subject === 'projects' ? (projects[String(argv.at(-1))] ?? []) : { targets: targets[String(value)] ?? {} };
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

const decided = Effect.fn('decided')(function* (command: string, request: Request, installed: boolean, expected: object) {
    const { decide } = yield* hosted(installed);
    expect(yield* decide(command, request)).toMatchObject(expected);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(NodeServices.layer, { timeout: 60_000 })('function-hooks', (it) => {
    describe('rewrites', () => {
        it.effect.each([
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
            [`osascript -e 'tell application "Adobe InDesign 2026" to quit'`, "watch-apps quit 'Adobe InDesign 2026'"],
            [`osascript -e 'quit app "TextEdit"'`, 'watch-apps quit TextEdit'],
            [`osascript -e 'activate application "TextEdit"'`, 'watch-apps launch TextEdit'],
            ['/System/Applications/TextEdit.app/Contents/MacOS/TextEdit', 'watch-apps launch com.apple.TextEdit'],
            ['watch-apps perform AAEC AXPress; sleep 2; watch-apps read TextEdit', 'watch-apps perform AAEC AXPress; watch-apps read TextEdit'],
            ['watch-apps perform AAEC AXPress && sleep 2', 'watch-apps perform AAEC AXPress'],
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
        ] as const)('%s', ([command, text]) => decided(command, FOREGROUND, false, { kind: 'rewrite', command: text, background: false }));
        it.effect.each([
            ['trailing & becomes run_in_background', 'nx run portfolio:serve &', FOREGROUND, false, { kind: 'rewrite', command: 'nx run portfolio:serve ', background: true, notice: some('& backgrounded the call. Wait for its completion notice') }],
            ['trailing & drops in a background call', 'nx run portfolio:serve &', BACKGROUND, false, { kind: 'rewrite', command: 'nx run portfolio:serve ', background: false }],
            [
                'missing timeout becomes call timeout',
                `timeout ${Duration.toSeconds(DEADLINE)} nx run rasm:check`,
                FOREGROUND,
                false,
                { kind: 'rewrite', command: 'nx run rasm:check', timeout: some(Duration.toMillis(DEADLINE)), notice: some(`timeout is not installed. Call timeout set to ${Duration.toSeconds(DEADLINE)}`) },
            ],
            ['missing timeout over the maximum caps the call timeout', 'timeout 2h nx run rasm:check', FOREGROUND, false, { kind: 'rewrite', timeout: some(Duration.toMillis(DEADLINE)), notice: some('timeout is not installed. Call timeout capped at maximum') }],
            ['installed timeout stays', `timeout ${Duration.toSeconds(DEADLINE)} nx run rasm:check`, FOREGROUND, true, { kind: 'allow' }],
            ['read -t leak becomes sleep in Monitor', 'until test -f done; do read -t 1 x < <(tail -f /dev/null); done', MONITOR, false, { kind: 'rewrite', command: 'until test -f done; do sleep 1; done' }],
            [
                'analyzeHeadless runs in background, logs under .artifacts/logs, and drops heap overrides',
                'GHIDRA_HEADLESS_MAXMEM=16G "$GHIDRA_INSTALL_DIR/support/analyzeHeadless" "$GHIDRA_PROJECT_DIR" name -process x',
                FOREGROUND,
                false,
                { kind: 'rewrite', command: `"$GHIDRA_INSTALL_DIR/support/analyzeHeadless" "$GHIDRA_PROJECT_DIR" name -process x -log ${ROOT}/.artifacts/logs/ghidra.log -scriptlog ${ROOT}/.artifacts/logs/ghidra-script.log`, background: true },
            ],
            ['tracked script files stay unparsed', 'bash .claude/skills/manage-repo/scripts/maintain.sh --help', FOREGROUND, false, { kind: 'allow' }],
        ] as const)('%s', ([, command, request, installed, expected]) => decided(command, request, installed, expected));
    });
    describe('denials', () => {
        it.effect.each([
            ['sleep 1; echo hi', BLIND],
            ['ls; sleep 60; ls', BLIND],
            ['ls; sleep 60', BLIND],
            ['cd /tmp; /bin/sleep 60', BLIND],
            ['until test -f done; do sleep 3; done', BLIND],
            ['while pgrep -f x >/dev/null; do sleep 5; done', BLIND],
            ['while kill -0 123 2>/dev/null; do sleep 1; done', BLIND],
            ['until ! ps -p 123 >/dev/null; do sleep 1; done', BLIND],
            ['for i in 1 2 3; do curl -sf localhost:3000 && break; sleep 2; done', BLIND],
            ['while kill -0 123; do :; done', BLIND],
            ['until test -f done; do true; done', BLIND],
            ['caffeinate -i -t 300', BLIND],
            ['caffeinate -w 123', BLIND],
            ['ls; caffeinate -t 5; ls', BLIND],
            ['read -t 5 x < <(tail -f /dev/null)', BLIND],
            ['tail -n +1 -f build.log | grep -m 1 Finished', BLIND],
            ['wait4path /Volumes/Build', BLIND],
            ['watch -n 2 ls', BLIND],
            ['bash -c "sleep 30; ls"', BLIND],
            ["bash -c 'read -t 5 x < <(tail -f /dev/null)'", 'read -t leaks its input process. Use sleep 5'],
            ["bash -c 'pgrep -f foo'", "pgrep foo matches its own shell. Use '[f]oo'"],
            ["cat > /tmp/probe.py <<'EOF'\nprint(1)\nEOF\npython3 /tmp/probe.py", 'python3 runs inline code'],
            ["python3 - <<'EOF'\nprint(1)\nEOF", 'python3 runs inline code'],
            ['cd /tmp && W=/private/tmp/x; rm -rf $W/p', 'rm $W/p expands at run time'],
            ['a || W=/private/tmp/x && rm -rf $W/p', 'rm $W/p expands at run time'],
            ['S=$(pwd); rm -rf $S/build', 'rm $S/build expands at run time'],
            ['rm -rf .', 'covers repository root'],
            [`osascript -e 'tell application "Finder" to make new folder'`, 'osascript drives Finder. Use watch-apps perform or atelier'],
            [`osascript -e 'tell application "System Events" to keystroke "a"'`, 'System Events hits frontmost app'],
            ['osascript probe.applescript', 'osascript probe.applescript is opaque'],
            ['open -W -a Blender scene.blend', 'open -W launches Blender'],
            ['pkill -f analyzeHeadless', 'pkill ends every match. Kill pids or watch-apps quit'],
            ['killall Rhino', 'killall ends every match'],
            ['kill $(pgrep -x TextEdit)', 'pgrep feeds kill every match'],
            ['pgrep -x TextEdit | xargs kill', 'pgrep feeds kill every match'],
            [`osascript -e 'tell application "TextEdit" to quit saving no'`, 'osascript drives TextEdit'],
            [`osascript -l JavaScript -e 'Application("TextEdit").activate()'`, 'JavaScript osascript reaches apps. Use watch-apps or atelier'],
            ['until watch-apps read TextEdit | grep -q responding; do sleep 5; done', 'Loop polls watch-apps. Run watch-apps watch in Monitor'],
            ['watch-apps watch TextEdit', 'watch-apps watch never exits. Run it in Monitor'],
            ['timeout 600 nx run rasm:check & wait; bash -c "timeout 5 ls"', 'timeout is not installed. Drop it'],
            ['for p in a b; do nx run $p:test; done', 'Loop reruns nx tasks'],
            ['ls libs | xargs -I{} nx run {}:check', 'Loop reruns nx tasks'],
            ['nx run portfolio:serve & echo started', '& job outlives the call'],
            ['"$GHIDRA_INSTALL_DIR/support/analyzeHeadless" /tmp/p name -process x', 'analyzeHeadless project is not $GHIDRA_PROJECT_DIR'],
            ['for b in a b; do analyzeHeadless "$GHIDRA_PROJECT_DIR" "$b" -process x; done', 'analyzeHeadless fans out per input'],
            ['export GHIDRA_HEADLESS_MAXMEM=4096M; node prove.ts', 'GHIDRA_HEADLESS_MAXMEM overrides the analyzeHeadless heap. Drop it'],
            ['GHIDRA_MAXMEM=4G bash -c "echo"; bash -c "GHIDRA_MAXMEM=4G echo"', 'GHIDRA_MAXMEM overrides the analyzeHeadless heap'],
            ["sd -F 'abc' 'abc' notes.md", 'sd replaces abc with itself'],
            ["sd '' x notes.md", 'sd empty pattern matches everywhere'],
            ['fd -e plist Info ~', 'is too wide'],
            ['git reset --hard', 'git reset --hard discards working-tree or index state'],
            ['git stash', 'git stash hides uncommitted work'],
            ['cat', 'cat waits on unfed stdin'],
        ] as const)('%s', ([command, reason]) => decided(command, FOREGROUND, false, { kind: 'deny', reason: expect.stringContaining(reason) }));
        it.effect.each([
            ['wait.sh', 'until ! pgrep -f analyzeHeadless >/dev/null; do /bin/sleep 5; done\n', BLIND],
            ['run.sh', 'project="$PWD/ghidra/project"\n"$GHIDRA_INSTALL_DIR/support/analyzeHeadless" "$project" color -process "$1"\n', 'analyzeHeadless project is not $GHIDRA_PROJECT_DIR'],
        ] as const)(
            'script file %s',
            Effect.fn('scripted')(function* ([name, text, reason]: readonly [string, string, string]) {
                const { decide, write } = yield* hosted(false);
                expect(yield* decide(`bash ${yield* write(name, text)}`, FOREGROUND)).toMatchObject({ kind: 'deny', reason: expect.stringContaining(reason) });
            }),
        );
    });
    describe('allowances', () => {
        it.effect.each([
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
            ['awk \'BEGIN{printf "%.3f\\n", 6.28/2}\'', FOREGROUND],
            ['cd /tmp && W=/private/tmp/x && rm -rf $W/p', FOREGROUND],
            ['S=/private/tmp/s; C=$S/c; rm -rf $C', FOREGROUND],
            ['GHIDRA_INSTALL_DIR=/opt/ghidra ls', FOREGROUND],
            ['nx run rasm-views:check', FOREGROUND],
            ['for f in a.md b.md; do wc -l "$f"; done; nx run rasm-views:check', FOREGROUND],
            ['sleep 1; echo hi', BACKGROUND],
            ['ls; sleep 60; ls', BACKGROUND],
            ['until test -f done; do sleep 3; done', BACKGROUND],
            ['until ! ps -p 123 >/dev/null; do sleep 1; done', BACKGROUND],
            ['tail -f build.log', BACKGROUND],
            ['until test -f done; do sleep 3; done', MONITOR],
            ['while kill -0 123 2>/dev/null; do sleep 1; done', MONITOR],
            ['tail -f build.log | grep --line-buffered error', MONITOR],
            ['watch -n 2 ls', MONITOR],
            ['watch-apps watch TextEdit', MONITOR],
            ['watch-apps watch --help', FOREGROUND],
            ['/Applications/Blender.app/Contents/MacOS/Blender --command extension list', FOREGROUND],
        ] as const)('%s', ([command, request]) => decided(command, request, false, { kind: 'allow' }));
    });
});
