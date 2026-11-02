import { NodeServices } from '@effect/platform-node';
import { describe, expect, layer } from '@effect/vitest';
import { commandDecision, none, type Request, type System, some } from '@rasm/function-hooks';
import { Config, Effect, FileSystem, Path, Stream } from 'effect';
import { ChildProcess } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

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
const BASH: Request = { tool: 'Bash', background: false, timeout: none };
const MONITOR: Request = { tool: 'Monitor', background: true, timeout: some(300_000) };

// --- [SERVICES] ------------------------------------------------------------------------

const host = Effect.gen(function* () {
    const [fs, path, home, context] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String('HOME'), Effect.context<NodeServices.NodeServices>()]);
    const run = Effect.runPromiseWith(context);
    const root = path.resolve(new URL('../../../..', import.meta.url).pathname);
    const spawn: System['run'] = ([file, ...rest], { cwd, stdin }) => {
        const [, , , , subject, value = ''] = rest;
        if (file === 'mise' && rest[2] === 'nx') {
            return Promise.resolve({ exitCode: 0, stderr: '', stdout: JSON.stringify(subject === 'projects' ? (PROJECTS[rest.at(-1) ?? ''] ?? []) : { targets: TARGETS[value] ?? {} }) });
        }
        return run(
            ChildProcess.make(file, rest, { cwd, stdin: Stream.encodeText(Stream.make(stdin)) }).pipe(
                Effect.flatMap((handle) => Effect.all({ stdout: Stream.mkString(Stream.decodeText(handle.stdout)), stderr: Stream.mkString(Stream.decodeText(handle.stderr)), exitCode: handle.exitCode }, { concurrency: 'unbounded' })),
                Effect.scoped,
            ),
        );
    };
    return {
        root,
        decide: (command: string, request: Request = BASH, cwd = root) =>
            Effect.promise(() => commandDecision({ run: spawn, exists: (entry) => run(fs.exists(entry)), plugin: path.join(root, 'plugins', 'function-hooks'), root: some(root), cwd, home: some(home) }, command, 'rasm', request)),
    };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(NodeServices.layer, { timeout: 60_000 })('function-hooks', (it) => {
    describe('rewrites', () => {
        it.effect.each([
            ['timeout 600 cargo clippy -p rasm-mcp 2>&1 | tail -5', 'nx run rasm-mcp:typecheck:clippy 2>&1 | tail -5'],
            ['timeout -k 5 2m nx run rasm:check', 'nx run rasm:check'],
            ['biome check plugins/function-hooks', 'nx run rasm:lint:biome -- plugins/function-hooks'],
            ['biome check --write plugins/function-hooks', 'nx run rasm:format:biome -- plugins/function-hooks'],
            ['ruff check tools/refurb', 'nx run rasm:lint:ruff -- tools/refurb'],
            ['ruff format tools/refurb', 'nx run rasm:format:ruff -- tools/refurb'],
            ['ast-grep scan plugins', 'nx run rasm:lint:ast-grep -- plugins'],
            ['node_modules/.bin/tsc -p plugins/function-hooks/tsconfig.json --noEmit --pretty false', 'nx run @rasm/function-hooks:typecheck:tsc -- --noEmit'],
            ["sd -F '- text' 'new' notes.md", "sd -F -- '- text' 'new' notes.md"],
            ['watch-apps launch org.blenderfoundation.blender', 'watch-apps launch --hides org.blenderfoundation.blender'],
            ['open -n -g -a Blender scene.blend', 'watch-apps launch Blender --hides --new --document=scene.blend'],
            ['/Applications/RhinoBETA.app/Contents/MacOS/Rhinoceros -nosplash', 'watch-apps launch RhinoBETA --hides --arguments=-nosplash'],
            [`osascript -e 'tell application "Adobe Illustrator" to activate'`, "watch-apps launch 'Adobe Illustrator' --hides"],
            ['tree -L 2 --dirsfirst --noreport plugins', 'tree -L 2 --group-directories-first plugins'],
            ['nx run rasm:typecheck:mypy --skip-nx-cache', 'nx run rasm:typecheck:mypy'],
            ['nx affected -t check --skipNxCache', 'nx affected -t check'],
        ] as const)('%s', ([command, rewritten]) =>
            Effect.gen(function* () {
                const { decide } = yield* host;
                expect(yield* decide(command)).toMatchObject({ kind: 'rewrite', command: rewritten });
            }),
        );
        it.effect('timeout prefix becomes call timeout', () =>
            Effect.gen(function* () {
                const { decide } = yield* host;
                expect(yield* decide('timeout 600 nx run rasm:check')).toMatchObject({ kind: 'rewrite', timeout: some(600_000), background: false });
            }),
        );
        it.effect('analyzeHeadless runs in background and logs under .artifacts/logs', () =>
            Effect.gen(function* () {
                const { decide, root } = yield* host;
                expect(yield* decide('"$GHIDRA_INSTALL_DIR/support/analyzeHeadless" /tmp/p name -process x')).toMatchObject({
                    kind: 'rewrite',
                    command: `"$GHIDRA_INSTALL_DIR/support/analyzeHeadless" /tmp/p name -process x -log ${root}/.artifacts/logs/ghidra.log -scriptlog ${root}/.artifacts/logs/ghidra-script.log`,
                    background: true,
                });
            }),
        );
    });
    describe('denials', () => {
        it.effect.each([
            ["cat > /tmp/probe.py <<'EOF'\nprint(1)\nEOF\npython3 /tmp/probe.py", 'python3 runs inline program'],
            ['until test -f done; do sleep 3; done', 'Loop polls with sleep'],
            ['caffeinate -t 590; echo waited', 'caffeinate -t holds call open'],
            ["sd -F 'abc' 'abc' notes.md", 'sd replaces abc with identical text in notes.md'],
            ['open -W -a Blender scene.blend', 'open -W launches Blender outside watch-apps'],
            ['S=$(pwd); rm -rf $S/build', 'expanded at run time'],
        ] as const)('%s', ([command, reason]) =>
            Effect.gen(function* () {
                const { decide } = yield* host;
                expect(yield* decide(command)).toMatchObject({ kind: 'deny', reason: expect.stringContaining(reason) });
            }),
        );
    });
    describe('allowances', () => {
        it.effect.each([
            'S=/private/tmp/scratchpad; rm -rf $S/build',
            'sed -n 1,3p <(git show HEAD:README.md)',
            'echo a | grep x | tail -3 && ls',
            '/Applications/Blender.app/Contents/MacOS/Blender -b scene.blend --python render.py',
            'tsc -p docs/research/probe',
            'ruff check --select F401 tools',
            'ast-grep scan --json plugins',
        ])('%s', (command) =>
            Effect.gen(function* () {
                const { decide } = yield* host;
                expect(yield* decide(command)).toEqual({ kind: 'allow' });
            }),
        );
        it.effect('Monitor polls with until-loop', () =>
            Effect.gen(function* () {
                const { decide } = yield* host;
                expect(yield* decide('until test -f done; do sleep 3; done', MONITOR)).toEqual({ kind: 'allow' });
            }),
        );
    });
});
