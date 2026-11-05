// biome-ignore-all lint/suspicious/noTemplateCurlyInString: Bash substitutions are literal test input.
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { expect, it } from '@effect/vitest';
import { type CommandRequest, commandDecision, Maybe, type System } from '@rasm/function-hooks';
import { Config, Duration, Effect, FileSystem, Option, Path, Stream } from 'effect';
import { ChildProcess } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = new URL('../../../../..', import.meta.url).pathname.replace(/\/$/u, '');
const foreground: CommandRequest = { kind: 'foreground', timeout: Maybe.none };

// --- [SERVICES] ------------------------------------------------------------------------

const hosted = Effect.fn('hosted')(function* (installed: boolean, overrides: Readonly<Record<string, object>> = {}) {
    const [fs, path, home, context] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String('HOME'), Effect.context<NodeServices>()]);
    const run = Effect.runPromiseWith(context);
    const plugin = path.join(ROOT, 'plugins', 'function-hooks');
    const roots: Readonly<Record<string, string>> = { '.': 'rasm', 'plugins/function-hooks': '@rasm/function-hooks' };
    const targets: Readonly<Record<string, object>> = {
        rasm: {
            'lint:biome': { options: { command: 'biome check --error-on-warnings' } },
            'format:biome': { options: { command: 'biome check --write --error-on-warnings --no-errors-on-unmatched' } },
            'format:biome:unsafe': { options: { command: 'biome check --write --unsafe --error-on-warnings --no-errors-on-unmatched' } },
            'lint:ruff': { dependsOn: [{ target: 'lint:ruff:*', params: 'forward' }] },
            'lint:ruff:check': { options: { command: 'ruff check' } },
            'lint:ruff:format': { options: { command: 'ruff format --check' } },
            'format:ruff': { options: { commands: ['ruff check --fix', 'ruff format'] } },
            'lint:ast-grep': { dependsOn: [{ target: 'lint:ast-grep:tree', params: 'forward' }, 'lint:ast-grep:root'] },
            'lint:ast-grep:tree': { options: { command: "ast-grep scan --custom-languages allow --no-ignore hidden --globs '!pnpm-lock.yaml'" } },
            'lint:ast-grep:root': { options: { command: "ast-grep scan --custom-languages allow --filter '^(no-chain-flattening)$'" } },
            rewrite: { dependsOn: [{ target: 'rewrite:tree', params: 'forward' }, 'rewrite:root'] },
            'rewrite:tree': { options: { command: "flock .cache/ast-grep/rewrite.lock ast-grep scan --custom-languages allow --no-ignore hidden --globs '!pnpm-lock.yaml' --update-all" } },
            'rewrite:root': { options: { command: "flock .cache/ast-grep/rewrite.lock ast-grep scan --custom-languages allow --filter '^(no-chain-flattening)$' --update-all" } },
            outline: { options: { command: 'ast-grep outline --custom-languages allow --color never' } },
            'typecheck:tsc': { options: { command: 'effect-tsgo tsc@next --pretty false' } },
            'typecheck:mypy': { dependsOn: [{ target: 'typecheck:mypy:root', params: 'forward' }, 'typecheck:mypy:3.13'] },
            'typecheck:mypy:root': { options: { command: "mypy --num-workers auto --exclude '^libs/python/blender/'" } },
            'typecheck:mypy:3.13': { options: { command: 'export VIRTUAL_ENV=.cache/python/3.13 && mypy --num-workers auto --python-version 3.13 --python-executable .cache/python/3.13/bin/python "libs/python/blender"' } },
            'lint:refurb': { dependsOn: [{ target: 'lint:refurb:root', params: 'forward' }, 'lint:refurb:3.13'] },
            'lint:refurb:root': { options: { command: "python -m tools.refurb --quiet -- --exclude '^libs/python/blender/'" } },
            'lint:refurb:3.13': { options: { command: 'export VIRTUAL_ENV=.cache/python/3.13 && python -m tools.refurb --quiet --python-version 3.13 "libs/python/blender" -- --python-executable .cache/python/3.13/bin/python' } },
            ...overrides,
        },
        '@rasm/function-hooks': { 'typecheck:tsc': { options: { command: 'effect-tsgo tsc@next --pretty false' } } },
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
        const [file, , , tool, , , value] = argv;
        const name = roots[String(value)] ?? String(value);
        const shown = targets[name];
        const stubs: Readonly<Record<string, boolean>> = { timeout: true, mise: tool === 'nx' };
        const output = file === 'timeout' ? '' : JSON.stringify({ name, targets: shown });
        return stubs[file] === true ? run(Effect.succeed({ exitCode: Number(file === 'timeout' ? !installed : shown === undefined), stderr: '', stdout: output })) : spawned(argv, init);
    };
    const scanner = (yield* Effect.promise(() => spawned(['mise', 'which', 'ast-grep'], { cwd: plugin, stdin: '' }))).stdout.trim();
    const folder = yield* fs.makeTempDirectoryScoped();
    return {
        write: (name: string, text: string) => Effect.as(fs.writeFileString(path.join(folder, name), text), path.join(folder, name)),
        decide: (command: string, request: CommandRequest) =>
            Effect.promise(() =>
                commandDecision(
                    {
                        run: spawn,
                        exists: (entry) => run(fs.exists(entry)),
                        read: (entry) => run(fs.readFileString(entry)),
                        environment: (name) => run(Effect.map(Config.option(Config.String(name)), Option.match({ onNone: () => Maybe.none, onSome: Maybe.some }))),
                        scanner,
                        plugin,
                        root: Maybe.some(ROOT),
                        cwd: ROOT,
                        home: Maybe.some(home),
                    },
                    command,
                    request,
                ),
            ),
    };
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const decided = Effect.fn('decided')(function* (command: string, request: CommandRequest, installed: boolean, expected: object) {
    const { decide } = yield* hosted(installed);
    expect(yield* decide(command, request)).toMatchObject(expected);
});
const located = Effect.fn('located')(function* () {
    const [{ decide }, folder] = yield* Effect.all([hosted(false), Config.String('GHIDRA_PROJECT_DIR')]);
    expect(yield* decide(`analyzeHeadless ${folder} name -process x`, foreground)).toMatchObject({ kind: 'rewrite', command: `analyzeHeadless "$GHIDRA_PROJECT_DIR" name -process x -log ${ROOT}/.artifacts/logs/ghidra.log -scriptlog ${ROOT}/.artifacts/logs/ghidra-script.log`, background: false });
});
const scripted = Effect.fn('scripted')(function* ([name, text, flags, expected]: readonly [string, string, readonly string[], object]) {
    const { decide, write } = yield* hosted(false);
    expect(yield* decide(['bash', ...flags, yield* write(name, text)].join(' '), foreground)).toMatchObject(expected);
});
const projected = Effect.fn('projected')(function* ([command, reason]: readonly [string, string]) {
    const { decide, write } = yield* hosted(false);
    const file = yield* write('p.gpr', '');
    const folder = file.slice(0, file.lastIndexOf('/'));
    expect(yield* decide(command.replaceAll('{}', folder), foreground)).toMatchObject({ kind: 'deny', reason: expect.stringContaining(reason.replaceAll('{}', folder)) });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { timeout: 60_000 })('function-hooks', (methods) => {
    const deadline: Duration.Input = '600 seconds';
    const blind = 'Command waits in the foreground, use Monitor or run_in_background';
    const background: CommandRequest = { kind: 'background' };
    const monitor: CommandRequest = { kind: 'monitor' };
    methods.effect.each([
        ['timeout 600 cargo clippy -p rasm-mcp 2>&1 | tail -5', 'set -o pipefail; nx run rasm-mcp:typecheck:clippy 2>&1 | tail -5'],
        ['timeout -k 5 2m nx run rasm:check', 'nx run rasm:check'],
        ['biome check plugins/function-hooks', 'nx run rasm:lint:biome --args=plugins/function-hooks'],
        ['biome check --write plugins/function-hooks', 'nx run rasm:format:biome --args=plugins/function-hooks'],
        ['ruff check tools/refurb', 'nx run rasm:lint:ruff --args=tools/refurb'],
        ['ruff format tools/refurb', 'nx run rasm:format:ruff --args=tools/refurb'],
        ['ast-grep scan plugins', 'nx run rasm:lint:ast-grep --args=plugins'],
        ['ast-grep scan --custom-languages allow plugins', 'nx run rasm:lint:ast-grep --args=plugins'],
        ['ast-grep scan --update-all', 'nx run rasm:rewrite'],
        ['ast-grep scan --json plugins', 'ast-grep scan --custom-languages allow --json plugins'],
        ['ast-grep scan -U plugins', 'ast-grep scan --custom-languages allow -U plugins'],
        ["ast-grep run -p 'Option.some($A)' -l tsxlang plugins", "ast-grep run --custom-languages allow -p 'Option.some($A)' -l tsxlang plugins"],
        ["ast-grep -p 'Option.some($A)' plugins", "ast-grep --custom-languages allow -p 'Option.some($A)' plugins"],
        ['ast-grep outline plugins/function-hooks/monads.ts', 'ast-grep outline --custom-languages allow plugins/function-hooks/monads.ts'],
        ['ast-grep test --skip-snapshot-tests', 'ast-grep test --custom-languages allow --skip-snapshot-tests'],
        ['nx run rasm:outline -- --custom-languages allow plugins', 'nx run rasm:outline -- plugins'],
        ['nx run rasm:outline -- --color never --custom-languages=allow', 'nx run rasm:outline'],
        ['node_modules/.bin/tsc -p plugins/function-hooks/tsconfig.json --noEmit --pretty false', 'nx run @rasm/function-hooks:typecheck:tsc --args=--noEmit'],
        ['effect-tsgo tsc -p plugins/function-hooks/tsconfig.json --noEmit --pretty false', 'nx run @rasm/function-hooks:typecheck:tsc --args=--noEmit'],
        ['effect-tsgo tsc --noEmit --pretty false', 'nx run rasm:typecheck:tsc --args=--noEmit'],
        ['effect-tsgo tsc@next -p plugins/function-hooks/tsconfig.json --noEmit --pretty false', 'nx run @rasm/function-hooks:typecheck:tsc --args=--noEmit'],
        ['.venv/bin/mypy --config-file pyproject.toml --no-pretty tools/refurb', 'nx run rasm:typecheck:mypy --args=tools/refurb'],
        ['mypy --python-version 3.13 --python-executable .cache/python/3.13/bin/python --cache-dir /private/tmp/mypy libs/python/blender/bpy', 'nx run rasm:typecheck:mypy:3.13'],
        ['uv run mypy', 'nx run rasm:typecheck:mypy'],
        ['refurb tools/refurb', 'nx run rasm:lint:refurb --args=tools/refurb'],
        ['python -m tools.refurb tools/refurb', 'nx run rasm:lint:refurb --args=tools/refurb'],
        ['python -m tools.refurb --quiet --python-version 3.13 libs/python/blender -- --python-executable .cache/python/3.13/bin/python', 'nx run rasm:lint:refurb:3.13'],
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
    ] as const)('rewrites %s', ([command, text]) => decided(command, foreground, false, { kind: 'rewrite', command: text, background: false }));
    methods.effect.each([
        ['trailing & becomes run_in_background', 'nx run portfolio:serve &', foreground, false, { kind: 'rewrite', command: 'nx run portfolio:serve ', background: true, notice: Maybe.some('Call runs in the background for &, wait for its completion notice') }],
        ['trailing & drops in a background call', 'nx run portfolio:serve &', background, false, { kind: 'rewrite', command: 'nx run portfolio:serve ', background: false }],
        [
            'missing timeout becomes call timeout',
            `timeout ${Duration.toSeconds(deadline)} nx run rasm:check`,
            foreground,
            false,
            { kind: 'rewrite', command: 'nx run rasm:check', timeout: Maybe.some(Duration.toMillis(deadline)), notice: Maybe.some(`timeout is not installed, call timeout set to ${Duration.toSeconds(deadline)}`) },
        ],
        ['missing timeout over the maximum caps the call timeout', 'timeout 2h nx run rasm:check', foreground, false, { kind: 'rewrite', timeout: Maybe.some(Duration.toMillis(deadline)), notice: Maybe.some(`timeout is not installed, call timeout capped at ${Duration.toSeconds(deadline)}s`) }],
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
        ['named descriptor redirect opens no program', 'exec {fd}>f', foreground, false, { kind: 'allow' }],
        ['timed command reads its redirect', 'time cat < f', foreground, false, { kind: 'allow' }],
        ['coprocess reads its pipe', 'printf x | coproc cat', foreground, false, { kind: 'allow' }],
    ] as const)('rewrites: %s', ([, command, request, installed, expected]) => decided(command, request, installed, expected));
    methods.effect.each([
        ['biome check --error-on-warnings --no-errors-on-unmatched nx.json', { kind: 'rewrite', command: "nx run rasm:lint:biome --args='--no-errors-on-unmatched nx.json'" }],
        ['biome check --no-errors-on-unmatched --reporter=json nx.json', { kind: 'rewrite', command: "nx run rasm:lint:biome --args='--no-errors-on-unmatched --reporter=json nx.json'" }],
        ['biome check --write --no-errors-on-unmatched nx.json', { kind: 'rewrite', command: 'nx run rasm:format:biome --args=nx.json' }],
        ['biome check --fix --no-errors-on-unmatched nx.json', { kind: 'rewrite', command: 'nx run rasm:format:biome --args=nx.json' }],
        ['biome check --write --unsafe nx.json', { kind: 'rewrite', command: 'nx run rasm:format:biome:unsafe --args=nx.json' }],
        ['biome check --fix --unsafe nx.json', { kind: 'rewrite', command: 'nx run rasm:format:biome:unsafe --args=nx.json' }],
        ['biome check --write --reporter json nx.json', { kind: 'rewrite', command: "nx run rasm:format:biome --args='--reporter json nx.json'" }],
        ['biome check --unsafe nx.json', { kind: 'allow' }],
        ['biome check --write=false nx.json', { kind: 'allow' }],
        ['biome check --fix=false nx.json', { kind: 'allow' }],
        ['ruff check --fix tools', { kind: 'rewrite', command: 'nx run rasm:format:ruff --args=tools' }],
        ['ruff check --fix-only tools', { kind: 'allow' }],
        ['ruff check --no-fix tools', { kind: 'allow' }],
        ['ruff check --fix --no-fix tools', { kind: 'allow' }],
        ['ruff check --fix --unsafe-fixes tools', { kind: 'allow' }],
        ['ruff check --diff tools', { kind: 'allow' }],
        ['ruff format --check tools', { kind: 'allow' }],
        ['ruff format --diff tools', { kind: 'allow' }],
        ['ast-grep scan --update-all tools', { kind: 'rewrite', command: 'ast-grep scan --custom-languages allow --update-all tools' }],
        ['ast-grep scan -U --filter ^x$ tools', { kind: 'rewrite', command: 'ast-grep scan --custom-languages allow -U --filter ^x$ tools' }],
        ['ast-grep scan --interactive tools', { kind: 'rewrite', command: 'ast-grep scan --custom-languages allow --interactive tools' }],
        ['ast-grep scan -i tools', { kind: 'rewrite', command: 'ast-grep scan --custom-languages allow -i tools' }],
        ['cargo clippy -p rasm-mcp --fix', { kind: 'allow' }],
    ] as const)('preserves checker execution mode for %s', ([command, expected]) => decided(command, foreground, false, expected));
    methods.effect.each([
        ["biome check '$$'", { kind: 'rewrite', command: "nx run rasm:lint:biome --args=''\\''$$'\\'''" }],
        ["biome check '$&'", { kind: 'rewrite', command: "nx run rasm:lint:biome --args=''\\''$&'\\'''" }],
        ['biome check "$\'"', { kind: 'rewrite', command: "nx run rasm:lint:biome --args='\"$'\\''\"'" }],
        ["biome check '$`'", { kind: 'rewrite', command: "nx run rasm:lint:biome --args=''\\''$`'\\'''" }],
        ['path=plugins; biome check "$path/file.ts"', { kind: 'allow' }],
        ['biome check "$unknown/file.ts"', { kind: 'allow' }],
        ['biome check $(printf plugins)', { kind: 'allow' }],
        ['biome check *.ts', { kind: 'allow' }],
        ['biome check .', { kind: 'rewrite', command: 'nx run rasm:lint:biome --args=.' }],
    ] as const)('preserves caller arguments for %s', ([command, expected]) => decided(command, foreground, false, expected));
    methods.effect.each([
        ['biome check nx.json', 'lint:biome', { command: 'biome check --error-on-warnings', args: '--no-errors-on-unmatched' }, { kind: 'rewrite', command: "nx run rasm:lint:biome --args='--no-errors-on-unmatched nx.json'" }],
        ['biome check nx.json', 'lint:biome', { command: 'biome check --error-on-warnings', args: ['--reporter', 'json'] }, { kind: 'rewrite', command: "nx run rasm:lint:biome --args='--reporter json nx.json'" }],
        ['biome check tools', 'lint:biome', { command: 'biome check --error-on-warnings', args: 'tools' }, { kind: 'rewrite', command: 'nx run rasm:lint:biome' }],
        ['biome check plugins', 'lint:biome', { command: 'biome check --error-on-warnings', args: 'tools' }, { kind: 'allow' }],
        ['biome check nx.json', 'lint:biome', { command: 'biome check --error-on-warnings', args: '--write' }, { kind: 'allow' }],
        ['biome check --write nx.json', 'lint:biome', { command: 'biome check --error-on-warnings', args: '--write' }, { kind: 'rewrite', command: "nx run rasm:lint:biome --args='--write nx.json'" }],
        ['biome check nx.json', 'lint:biome', { command: 'biome check --error-on-warnings', args: 'ignored.ts', forwardAllArgs: false }, { kind: 'allow' }],
        ['cd tools && biome check', 'lint:biome', { command: 'biome check --error-on-warnings', forwardAllArgs: false }, { kind: 'allow' }],
        ['biome check nx.json', 'lint:biome', { commands: [{ command: 'biome check --error-on-warnings', forwardAllArgs: false }], forwardAllArgs: true }, { kind: 'allow' }],
        ['biome check nx.json', 'lint:biome', { commands: [{ command: 'biome check --error-on-warnings', forwardAllArgs: true }], args: '--no-errors-on-unmatched', forwardAllArgs: false }, { kind: 'rewrite', command: "nx run rasm:lint:biome --args='--no-errors-on-unmatched nx.json'" }],
        ['ruff check tools', 'lint:ruff:check', { command: 'ruff check', args: '--quiet' }, { kind: 'rewrite', command: "nx run rasm:lint:ruff:check --args='--quiet tools'" }],
        ['biome check nx.json', 'lint:biome', { command: 'biome check {args}' }, { kind: 'allow' }],
        ['biome check nx.json', 'lint:biome', { command: 'biome check', args: '--no-errors-on-unmatched; printf done' }, { kind: 'allow' }],
    ] as const)(
        'preserves configured arguments for %s',
        Effect.fn('configuredArguments')(function* ([command, target, options, expected]) {
            const { decide } = yield* hosted(false, { [target]: { options } });
            expect(yield* decide(command, foreground)).toMatchObject(expected);
        }),
    );
    methods.effect('rewrites analyzeHeadless literal project location becomes $GHIDRA_PROJECT_DIR', located);
    methods.effect.each([
        ['dir=/; answer=${ local dir=/safe; }; rm -rf "$dir"', 'deny'],
        ['dir=/safe; answer=${ local dir=/; }; rm -rf "$dir"', 'allow'],
        ['dir=/; answer=${| local dir=/safe; }; rm -rf "$dir"', 'deny'],
        ['dir=/safe; answer=${| local dir=/; }; rm -rf "$dir"', 'allow'],
        ['dir=/; answer=${ declare dir=/safe; }; rm -rf "$dir"', 'deny'],
        ['dir=/safe; answer=${ typeset dir=/; }; rm -rf "$dir"', 'allow'],
        ['dir=/safe; answer=${ local dir; }; rm -rf "$dir"', 'allow'],
        ['dir=/safe; answer=${ local dir=/; rm -rf "$dir"; }', 'deny'],
        ['dir=/; answer=${ local dir=/safe; rm -rf "$dir"; }', 'allow'],
        ['dir=/safe; answer=${ local dir=/safe; dir=/; }; rm -rf "$dir"', 'allow'],
        ['dir=/; answer=${ local dir=/safe; declare -g dir=/changed; }; rm -rf "$dir"', 'allow'],
        ['dir=/safe; answer=${ local dir=/safe; declare -g dir=/; }; rm -rf "$dir"', 'deny'],
        ['answer=${ local dir=/safe; }; rm -rf "$dir/p"', 'deny'],
        ['false && dir=/safe; rm -rf "$dir/p"', 'deny'],
        ['cd /tmp && W=/private/tmp/x; rm -rf $W/p "$W/q"', 'deny'],
        ['f() { cd /safe; }; rm -rf .', 'deny'],
        ['if false; then cd /safe; fi; rm -rf .', 'deny'],
        ['f() { cd /safe; }; f; rm -rf .', 'deny'],
        ['cd /does-not-exist; rm -rf .', 'deny'],
        ['cd /safe && rm -rf .', 'allow'],
        ['value=${ cd /tmp; }; rm -rf .', 'deny'],
        ['value=${| cd /tmp; REPLY=done; }; rm -rf .', 'deny'],
        ['value=${ cd /safe && rm -rf .; }', 'allow'],
        ['builtin cd /safe && rm -rf .', 'allow'],
        ['dir=/safe; answer=${ builtin local dir=/; }; rm -rf "$dir"', 'allow'],
        ['dir=/safe; answer=${ builtin local dir=/; rm -rf "$dir"; }', 'deny'],
        ['dir=/; answer=${ builtin local dir=/safe; rm -rf "$dir"; }', 'allow'],
        ['dir=/; local dir=/safe; rm -rf "$dir"', 'deny'],
        ['dir=/safe; local dir=/; rm -rf "$dir"', 'allow'],
        ['dir=/safe; answer=${ if false; then local dir; fi; dir=/; }; rm -rf "$dir"', 'deny'],
        ['dir=/safe; answer=${ if false; then local dir; else dir=/; fi; }; rm -rf "$dir"', 'deny'],
        ['dir=/safe; coproc { dir=/; }; rm -rf "$dir"', 'allow'],
        ['dir=/; coproc { dir=/safe; }; rm -rf "$dir"', 'deny'],
        ['dir=/safe; time dir=/; rm -rf "$dir"', 'deny'],
    ] as const)('resolves Bash scope for %s', ([command, kind]) => decided(command, foreground, false, { kind }));
    methods.effect.each([
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
        ["bash -c 'sleep 30' -n", blind],
        ["bash -c 'rm -rf .' -n", 'covers the repository root'],
        ["bash -n +n -c 'sleep 30'", blind],
        ["bash -n +o noexec -c 'rm -rf .'", 'covers the repository root'],
        ["bash -oc errexit 'rm -rf .'", 'covers the repository root'],
        ["bash -c 'read -t 5 x < <(tail -f /dev/null)'", 'read -t 5 leaks its input process, use sleep 5'],
        ["bash -c 'pgrep -f foo'", "pgrep foo matches its own shell, use '[f]oo'"],
        ["cat > /tmp/probe.py <<'EOF'\nprint(1)\nEOF\npython3 /tmp/probe.py", 'python3 runs inline code'],
        ["python3 - <<'EOF'\nprint(1)\nEOF", 'python3 runs inline code'],
        ['a || W=/private/tmp/x && rm -rf $W/p', 'rm $W/p expands at run time'],
        ['S=$(pwd); rm -rf $S/build', 'rm $S/build expands at run time'],
        ['rm -rf .', 'covers the repository root'],
        ['value=${ rm -rf .; }', 'covers the repository root'],
        ['value=${| rm -rf .; }', 'covers the repository root'],
        ['value=${ W=.; }; rm -rf $W', 'covers the repository root'],
        ['value=${| W=.; REPLY=done; }; rm -rf $W', 'covers the repository root'],
        ['value=${| REPLY=.; rm -rf $REPLY; }', 'covers the repository root'],
        ['REPLY=.; value=${| REPLY=/private/tmp/x; }; rm -rf $REPLY', 'covers the repository root'],
        ['value=$(cd /tmp); rm -rf .', 'covers the repository root'],
        ["value=${| node -e 'process.exit(0)'; }", 'node runs inline code'],
        ['"${ printf /Applications/Blender.app; }/Contents/MacOS/Blender"', 'path expands at run time'],
        [`osascript -e 'tell application "Finder" to make new folder'`, 'osascript drives Finder, use watch-apps perform'],
        [`osascript -e 'tell application "System Events" to keystroke "a"'`, 'System Events posts to the frontmost app'],
        [`osascript -e 'tell application "Adobe Illustrator" to do javascript "alert(1)"'`, "osascript scripts 'Adobe Illustrator', use Atelier execute_script"],
        ['osascript probe.applescript', 'osascript probe.applescript hides its statements'],
        ['open -W -a Blender scene.blend', 'open -W waits on Blender'],
        ['pkill -f analyzeHeadless', 'pkill ends every match, run kill <pid> on one pid pgrep lists'],
        ['killall Rhino', 'killall ends every match'],
        ['kill $(pgrep -x TextEdit)', 'pgrep feeds kill every match'],
        ['pgrep -x TextEdit | xargs kill', 'pgrep feeds kill every match'],
        ['pgrep -f Rhinoceros | xargs kill', 'pgrep feeds kill every match'],
        [`osascript -l JavaScript -e 'Application("TextEdit").activate()'`, 'JavaScript osascript drives apps, run watch-apps <subcommand> <app>'],
        ['until watch-apps read TextEdit | grep -q responding; do sleep 5; done', 'Loop polls TextEdit, run watch-apps watch TextEdit in Monitor'],
        ['watch-apps watch TextEdit', 'watch-apps watch never exits, run watch-apps watch TextEdit in Monitor'],
        ['timeout 600 nx run rasm:check & wait; bash -c "timeout 5 ls"', 'timeout is not installed, remove it'],
        ['for p in a b; do nx run $p:test; done', 'Loop reruns nx tasks'],
        ['ls libs | xargs -I{} nx run {}:check', 'Loop reruns nx tasks'],
        ['nx run portfolio:serve & echo started', '& job outlives the call'],
        ['"$GHIDRA_INSTALL_DIR/support/analyzeHeadless" /tmp/p name -process x', 'analyzeHeadless project /tmp/p is not $GHIDRA_PROJECT_DIR'],
        ['for b in a b; do analyzeHeadless "$GHIDRA_PROJECT_DIR" "$b" -process x; done', 'analyzeHeadless fans out per input, send each run with printf'],
        ['for f in a b; do analyzeHeadless "$GHIDRA_PROJECT_DIR" p -import "$f"; done', 'analyzeHeadless fans out per input, chain one -import per project with &&'],
        ["rsync -a --exclude '*.lock*' /private/tmp/x/p.rep/ /private/tmp/copy/", 'rsync copies Ghidra project p, send requests to its Serve.java with printf'],
        ['export GHIDRA_HEADLESS_MAXMEM=4096M; node prove.ts', 'GHIDRA_HEADLESS_MAXMEM overrides the analyzeHeadless heap, remove it'],
        ['GHIDRA_MAXMEM=4G bash -c "echo"; bash -c "GHIDRA_MAXMEM=4G echo"', 'GHIDRA_MAXMEM overrides the analyzeHeadless heap'],
        ["sd -F 'abc' 'abc' notes.md", 'sd replaces abc with itself'],
        ["sd '' x notes.md", 'sd empty pattern matches everywhere'],
        ['fd -e plist Info ~', 'is too wide'],
        ['git reset --hard', 'git reset --hard discards working-tree or index state'],
        ['git stash', 'git stash hides uncommitted work'],
        ['cat', 'cat waits on stdin with no input'],
        ['bash -n', 'bash waits on stdin with no input'],
        ['time cat', 'cat waits on stdin with no input'],
        ['time git push -f', 'git push -f rewrites or deletes remote history'],
        ['time -p git push -f | tee x', 'git push -f rewrites or deletes remote history'],
        ['coproc { rm -rf .; }', 'covers the repository root'],
        ['coproc NAME ( rm -rf . )', 'covers the repository root'],
    ] as const)('denies %s', ([command, reason]) => decided(command, foreground, false, { kind: 'deny', reason: expect.stringContaining(reason) }));
    methods.effect.each([
        ['wait.sh', 'until ! pgrep -f analyzeHeadless >/dev/null; do /bin/sleep 5; done\n', [], { kind: 'deny', reason: expect.stringContaining(blind) }],
        ['run.sh', 'project="$PWD/ghidra/project"\n"$GHIDRA_INSTALL_DIR/support/analyzeHeadless" "$project" color -process "$1"\n', [], { kind: 'deny', reason: expect.stringContaining('analyzeHeadless project "$project" is not $GHIDRA_PROJECT_DIR') }],
    ] as const)('denies script file %s', scripted);
    methods.effect.each([['-n'], ['-o', 'noexec']] as const)('allows shell syntax check %s', (flags) => scripted(['syntax.sh', 'sleep 30 &\nrm -rf .\n', flags, { kind: 'allow' }]));
    methods.effect.each([
        ['rm -f {}/*.lock*', "rm *.lock* deletes a Ghidra lock that blocks only while held, run lsof -t '{}/p.lock~'"],
        ['S={}; cd /tmp && rm -f $S/p.lock~', 'rm p.lock~ deletes a Ghidra lock'],
        ['find {} -maxdepth 1 -name p.lock -delete', 'find p.lock deletes a Ghidra lock'],
        ['cp -cR {} /private/tmp/copy', 'cp copies Ghidra project p, send requests to its Serve.java with printf'],
    ] as const)('denies project folder %s', projected);
    methods.effect.each([
        ['S=/private/tmp/scratchpad; rm -rf $S/build', foreground],
        ['sed -n 1,3p <(git show HEAD:README.md)', foreground],
        ['echo a | grep x | tail -3 && ls', foreground],
        ['/Applications/Blender.app/Contents/MacOS/Blender -b scene.blend --python render.py', foreground],
        ['tsc -p docs/research/probe', foreground],
        ['ruff check --select F401 tools', foreground],
        ['ast-grep scan --custom-languages ignore --json plugins', foreground],
        ['ast-grep outline --custom-languages=allow plugins', foreground],
        ['ast-grep --version', foreground],
        ['ast-grep scan -h', foreground],
        ['nx run rasm:outline -- --color always plugins', foreground],
        ['nx run rasm:outline --args=plugins', foreground],
        ['effect-tsgo tsc@7.0.2 --noEmit --pretty false', foreground],
        ['command -v bash', foreground],
        ['command -V bash', foreground],
        ["bash -n -c 'sleep 30 & rm -rf .'", foreground],
        ["bash -o noexec -c 'sleep 30 & rm -rf .'", foreground],
        ["bash +n -n -c 'sleep 30 & rm -rf .'", foreground],
        ["bash -cn 'sleep 30 & rm -rf .'", foreground],
        ['mypy /private/tmp/probe.py', foreground],
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
        ['value=${ W=/private/tmp/x; printf done; }; rm -rf $W/p', foreground],
        ['value=${| W=/private/tmp/x; REPLY=done; }; rm -rf $W/p', foreground],
        ['REPLY=/private/tmp/x; value=${| REPLY=.; }; rm -rf $REPLY/p', foreground],
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
    ] as const)('allows %s', ([command, request]) => decided(command, request, false, { kind: 'allow' }));
});
