import { NodeServices } from '@effect/platform-node';
import { describe, expect, layer } from '@effect/vitest';
import { Option } from '@rasm/function-hooks/composition';
import { commandDecision, type Request, type System } from '@rasm/function-hooks/policies';
import { Config, Duration, Effect, FileSystem, Path, Stream } from 'effect';
import { ChildProcess } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = new URL('../../../..', import.meta.url).pathname.replace(/\/$/u, '');

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
        decide: (command: string, request: Request) =>
            Effect.promise(() =>
                commandDecision(
                    { run: spawn, exists: (entry) => run(fs.exists(entry)), read: (entry) => run(fs.readFileString(entry)), environment: (name) => run(Effect.map(Config.String(name).pipe(Config.withDefault(undefined)), Option.given)), scanner, plugin, root: Option.some(ROOT), cwd: ROOT, home: Option.some(home) },
                    command,
                    'rasm',
                    request,
                ),
            ),
    };
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const decided = Effect.fn('decided')(function* (command: string, request: Request, installed: boolean, expected: object) {
    const { decide } = yield* hosted(installed);
    expect(yield* decide(command, request)).toMatchObject(expected);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(NodeServices.layer, { timeout: 60_000 })('function-hooks', (it) => {
    const deadline: Duration.Input = '600 seconds';
    const blind = 'Command waits blindly. Use Monitor or run_in_background';
    const foreground: Request = { kind: 'foreground', timeout: Option.none };
    const background: Request = { kind: 'background' };
    const monitor: Request = { kind: 'monitor' };
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
            ["open -a 'Rhino 8'", "watch-apps launch 'Rhino 8'"],
            [`osascript -e 'tell application "TextEdit" to quit saving no'`, 'watch-apps quit TextEdit'],
            ['open -a Rhino --args -nosplash', 'watch-apps launch Rhino --arguments=-nosplash'],
            ['open -a Rhino model.3dm', 'watch-apps launch Rhino --document model.3dm'],
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
            ['cd /tmp && W=/private/tmp/x; rm -rf $W/p "$W/q"', 'cd /tmp && W=/private/tmp/x; rm -rf /private/tmp/x/p "/private/tmp/x/q"'],
        ] as const)('%s', ([command, text]) => decided(command, foreground, false, { kind: 'rewrite', command: text, background: false }));
        it.effect.each([
            ['trailing & becomes run_in_background', 'nx run portfolio:serve &', foreground, false, { kind: 'rewrite', command: 'nx run portfolio:serve ', background: true, notice: Option.some('& backgrounded the call. Wait for its completion notice') }],
            ['trailing & drops in a background call', 'nx run portfolio:serve &', background, false, { kind: 'rewrite', command: 'nx run portfolio:serve ', background: false }],
            [
                'missing timeout becomes call timeout',
                `timeout ${Duration.toSeconds(deadline)} nx run rasm:check`,
                foreground,
                false,
                { kind: 'rewrite', command: 'nx run rasm:check', timeout: Option.some(Duration.toMillis(deadline)), notice: Option.some(`timeout is not installed. Call timeout set to ${Duration.toSeconds(deadline)}`) },
            ],
            ['missing timeout over the maximum caps the call timeout', 'timeout 2h nx run rasm:check', foreground, false, { kind: 'rewrite', timeout: Option.some(Duration.toMillis(deadline)), notice: Option.some('timeout is not installed. Call timeout capped at maximum') }],
            ['installed timeout stays', `timeout ${Duration.toSeconds(deadline)} nx run rasm:check`, foreground, true, { kind: 'allow' }],
            ['read -t leak becomes sleep in Monitor', 'until test -f done; do read -t 1 x < <(tail -f /dev/null); done', monitor, false, { kind: 'rewrite', command: 'until test -f done; do sleep 1; done' }],
            [
                'analyzeHeadless import runs in background, logs under .artifacts/logs, and drops heap overrides',
                'GHIDRA_HEADLESS_MAXMEM=16G "$GHIDRA_INSTALL_DIR/support/analyzeHeadless" "$GHIDRA_PROJECT_DIR" name -import x',
                foreground,
                false,
                { kind: 'rewrite', command: `"$GHIDRA_INSTALL_DIR/support/analyzeHeadless" "$GHIDRA_PROJECT_DIR" name -import x -log ${ROOT}/.artifacts/logs/ghidra.log -scriptlog ${ROOT}/.artifacts/logs/ghidra-script.log`, background: true },
            ],
            [
                'analyzeHeadless Serve.java runs in background',
                'analyzeHeadless "$GHIDRA_PROJECT_DIR" name -process x -postScript Serve.java',
                foreground,
                false,
                { kind: 'rewrite', command: `analyzeHeadless "$GHIDRA_PROJECT_DIR" name -process x -postScript Serve.java -log ${ROOT}/.artifacts/logs/ghidra.log -scriptlog ${ROOT}/.artifacts/logs/ghidra-script.log`, background: true },
            ],
            [
                'analyzeHeadless script runs stay in the foreground',
                'analyzeHeadless "$GHIDRA_PROJECT_DIR" name -process x -postScript Rtti.java',
                foreground,
                false,
                { kind: 'rewrite', command: `analyzeHeadless "$GHIDRA_PROJECT_DIR" name -process x -postScript Rtti.java -log ${ROOT}/.artifacts/logs/ghidra.log -scriptlog ${ROOT}/.artifacts/logs/ghidra-script.log`, background: false },
            ],
            ['tracked script files stay unparsed', 'bash .claude/skills/manage-repo/scripts/maintain.sh --help', foreground, false, { kind: 'allow' }],
        ] as const)('%s', ([, command, request, installed, expected]) => decided(command, request, installed, expected));
        it.effect(
            'analyzeHeadless literal project location becomes $GHIDRA_PROJECT_DIR',
            Effect.fn('located')(function* () {
                const [{ decide }, folder] = yield* Effect.all([hosted(false), Config.String('GHIDRA_PROJECT_DIR')]);
                expect(yield* decide(`analyzeHeadless ${folder} name -process x`, foreground)).toMatchObject({ kind: 'rewrite', command: `analyzeHeadless "$GHIDRA_PROJECT_DIR" name -process x -log ${ROOT}/.artifacts/logs/ghidra.log -scriptlog ${ROOT}/.artifacts/logs/ghidra-script.log`, background: false });
            }),
        );
    });
    describe('denials', () => {
        it.effect.each([
            ['sleep 1; echo hi', blind],
            ['ls; sleep 60; ls', blind],
            ['ls; sleep 60', blind],
            ['cd /tmp; /bin/sleep 60', blind],
            ['until test -f done; do sleep 3; done', blind],
            ['while pgrep -f x >/dev/null; do sleep 5; done', blind],
            ['while kill -0 123 2>/dev/null; do sleep 1; done', blind],
            ['until ! ps -p 123 >/dev/null; do sleep 1; done', blind],
            ['for i in 1 2 3; do curl -sf localhost:3000 && break; sleep 2; done', blind],
            ['while kill -0 123; do :; done', blind],
            ['until test -f done; do true; done', blind],
            ['caffeinate -i -t 300', blind],
            ['caffeinate -w 123', blind],
            ['ls; caffeinate -t 5; ls', blind],
            ['read -t 5 x < <(tail -f /dev/null)', blind],
            ['tail -n +1 -f build.log | grep -m 1 Finished', blind],
            ['wait4path /Volumes/Build', blind],
            ['watch -n 2 ls', blind],
            ['bash -c "sleep 30; ls"', blind],
            ["bash -c 'read -t 5 x < <(tail -f /dev/null)'", 'read -t leaks its input process. Use sleep 5'],
            ["bash -c 'pgrep -f foo'", "pgrep foo matches its own shell. Use '[f]oo'"],
            ["cat > /tmp/probe.py <<'EOF'\nprint(1)\nEOF\npython3 /tmp/probe.py", 'python3 runs inline code'],
            ["python3 - <<'EOF'\nprint(1)\nEOF", 'python3 runs inline code'],
            ['a || W=/private/tmp/x && rm -rf $W/p', 'rm $W/p expands at run time'],
            ['S=$(pwd); rm -rf $S/build', 'rm $S/build expands at run time'],
            ['rm -rf .', 'covers repository root'],
            [`osascript -e 'tell application "Finder" to make new folder'`, 'osascript drives Finder. Use watch-apps perform'],
            [`osascript -e 'tell application "System Events" to keystroke "a"'`, 'System Events posts to frontmost app'],
            [`osascript -e 'tell application "Adobe Illustrator" to do javascript "alert(1)"'`, "osascript scripts 'Adobe Illustrator'. Use Atelier execute_script"],
            ['osascript probe.applescript', 'osascript probe.applescript is opaque'],
            ['open -W -a Blender scene.blend', 'open -W waits on Blender'],
            ['pkill -f analyzeHeadless', 'pkill ends every match. Run kill <pid> on one pid pgrep lists'],
            ['killall Rhino', 'killall ends every match'],
            ['kill $(pgrep -x TextEdit)', 'pgrep feeds kill every match'],
            ['pgrep -x TextEdit | xargs kill', 'pgrep feeds kill every match'],
            ['pgrep -f Rhinoceros | xargs kill', 'pgrep feeds kill every match'],
            [`osascript -l JavaScript -e 'Application("TextEdit").activate()'`, 'JavaScript osascript drives apps. Run watch-apps <subcommand> <app>'],
            ['until watch-apps read TextEdit | grep -q responding; do sleep 5; done', 'Loop polls TextEdit. Run watch-apps watch TextEdit in Monitor'],
            ['watch-apps watch TextEdit', 'watch-apps watch never exits. Run watch-apps watch TextEdit in Monitor'],
            ['timeout 600 nx run rasm:check & wait; bash -c "timeout 5 ls"', 'timeout is not installed. Drop it'],
            ['for p in a b; do nx run $p:test; done', 'Loop reruns nx tasks'],
            ['ls libs | xargs -I{} nx run {}:check', 'Loop reruns nx tasks'],
            ['nx run portfolio:serve & echo started', '& job outlives the call'],
            ['"$GHIDRA_INSTALL_DIR/support/analyzeHeadless" /tmp/p name -process x', 'analyzeHeadless project is not $GHIDRA_PROJECT_DIR'],
            ['for b in a b; do analyzeHeadless "$GHIDRA_PROJECT_DIR" "$b" -process x; done', 'analyzeHeadless fans out per input. Send each run with printf'],
            ['for f in a b; do analyzeHeadless "$GHIDRA_PROJECT_DIR" p -import "$f"; done', 'analyzeHeadless fans out per input. Chain one -import per project with &&'],
            ["rsync -a --exclude '*.lock*' /private/tmp/x/p.rep/ /private/tmp/copy/", 'rsync copies Ghidra project p, its Serve.java serves every caller. Run printf'],
            ['export GHIDRA_HEADLESS_MAXMEM=4096M; node prove.ts', 'GHIDRA_HEADLESS_MAXMEM overrides the analyzeHeadless heap. Drop it'],
            ['GHIDRA_MAXMEM=4G bash -c "echo"; bash -c "GHIDRA_MAXMEM=4G echo"', 'GHIDRA_MAXMEM overrides the analyzeHeadless heap'],
            ["sd -F 'abc' 'abc' notes.md", 'sd replaces abc with itself'],
            ["sd '' x notes.md", 'sd empty pattern matches everywhere'],
            ['fd -e plist Info ~', 'is too wide'],
            ['git reset --hard', 'git reset --hard discards working-tree or index state'],
            ['git stash', 'git stash hides uncommitted work'],
            ['cat', 'cat waits on unfed stdin'],
        ] as const)('%s', ([command, reason]) => decided(command, foreground, false, { kind: 'deny', reason: expect.stringContaining(reason) }));
        it.effect.each([
            ['wait.sh', 'until ! pgrep -f analyzeHeadless >/dev/null; do /bin/sleep 5; done\n', blind],
            ['run.sh', 'project="$PWD/ghidra/project"\n"$GHIDRA_INSTALL_DIR/support/analyzeHeadless" "$project" color -process "$1"\n', 'analyzeHeadless project is not $GHIDRA_PROJECT_DIR'],
        ] as const)(
            'script file %s',
            Effect.fn('scripted')(function* ([name, text, reason]: readonly [string, string, string]) {
                const { decide, write } = yield* hosted(false);
                expect(yield* decide(`bash ${yield* write(name, text)}`, foreground)).toMatchObject({ kind: 'deny', reason: expect.stringContaining(reason) });
            }),
        );
        it.effect.each([
            ['rm -f {}/*.lock*', "rm *.lock* deletes a Ghidra lock, stale locks block nothing. Run lsof -t '{}/p.lock~'"],
            ['S={}; cd /tmp && rm -f $S/p.lock~', 'rm p.lock~ deletes a Ghidra lock'],
            ['find {} -maxdepth 1 -name p.lock -delete', 'find p.lock deletes a Ghidra lock'],
            ['cp -cR {} /private/tmp/copy', 'cp copies Ghidra project p, its Serve.java serves every caller. Run printf'],
        ] as const)(
            'project folder %s',
            Effect.fn('projected')(function* ([command, reason]: readonly [string, string]) {
                const { decide, write } = yield* hosted(false);
                const file = yield* write('p.gpr', '');
                const folder = file.slice(0, file.lastIndexOf('/'));
                expect(yield* decide(command.replaceAll('{}', folder), foreground)).toMatchObject({ kind: 'deny', reason: expect.stringContaining(reason.replaceAll('{}', folder)) });
            }),
        );
    });
    describe('allowances', () => {
        it.effect.each([
            ['S=/private/tmp/scratchpad; rm -rf $S/build', foreground],
            ['sed -n 1,3p <(git show HEAD:README.md)', foreground],
            ['echo a | grep x | tail -3 && ls', foreground],
            ['/Applications/Blender.app/Contents/MacOS/Blender -b scene.blend --python render.py', foreground],
            ['tsc -p docs/research/probe', foreground],
            ['ruff check --select F401 tools', foreground],
            ['ast-grep scan --json plugins', foreground],
            ['while read -r line; do echo "$line"; done < notes.md', foreground],
            ['for f in a b; do wc -l "$f"; done', foreground],
            ['nx show project rasm --json', foreground],
            ['nx run rasm:check 2>&1 | head -20', foreground],
            ['set -euo pipefail; nx run rasm:check | tail -5', foreground],
            ['git log --oneline | tail -3', foreground],
            ['tail -n 20 build.log', foreground],
            ['pgrep analyzeHeadless', foreground],
            ['pgrep -f "$pattern"', foreground],
            ['ps aux | grep -c java', foreground],
            ['nx run portfolio:serve & wait', foreground],
            ['fd -H foo plugins', foreground],
            ['git status', foreground],
            ['awk \'BEGIN{printf "%.3f\\n", 6.28/2}\'', foreground],
            ['cd /tmp && W=/private/tmp/x && rm -rf $W/p', foreground],
            ['S=/private/tmp/s; C=$S/c; rm -rf $C', foreground],
            ['GHIDRA_INSTALL_DIR=/opt/ghidra ls', foreground],
            ['nx run rasm-views:check', foreground],
            ['for f in a.md b.md; do wc -l "$f"; done; nx run rasm-views:check', foreground],
            ['sleep 1; echo hi', background],
            ['ls; sleep 60; ls', background],
            ['until test -f done; do sleep 3; done', background],
            ['until ! ps -p 123 >/dev/null; do sleep 1; done', background],
            ['tail -f build.log', background],
            ['until test -f done; do sleep 3; done', monitor],
            ['while kill -0 123 2>/dev/null; do sleep 1; done', monitor],
            ['tail -f build.log | grep --line-buffered error', monitor],
            ['watch -n 2 ls', monitor],
            ['watch-apps watch TextEdit', monitor],
            ['watch-apps watch --help', foreground],
            ['watch-apps launch Rhino --arguments -nosplash', foreground],
            ['watch-apps launch Rhino --document model.3dm', foreground],
            ['watch-apps launch TextEdit', foreground],
            ['watch-apps launch Rhino --new', foreground],
            ['/Applications/Blender.app/Contents/MacOS/Blender --command extension list', foreground],
        ] as const)('%s', ([command, request]) => decided(command, request, false, { kind: 'allow' }));
    });
});
