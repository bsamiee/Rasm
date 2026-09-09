import { describe, expect, it } from 'vitest';
import type { Decision } from '../composition/decision.ts';
import { OP_LINE } from './paths.ts';
import { type Bash, commandCeiling, commandTimeout, packageManager, shellOnce, shellRule } from './shell.ts';

const _CEILING = 600_000;

const _fresh = (command: string): Decision<Bash> => shellRule(new Set())({ command });

const _reason = (command: string): string => {
    const decision = _fresh(command);
    return decision.kind === 'deny' ? decision.reason : '';
};

const _rewritten = (command: string): readonly [string, readonly string[]] => {
    const decision = _fresh(command);
    return decision.kind === 'rewrite' ? [decision.e.command, decision.context] : ['', []];
};

// One case per deny row, the fragments the reason must hold
const _DENIED: readonly (readonly [string, readonly string[]])[] = [['head -c 100 x.binlog', ['mcp__binlog__binlog_overview']]];

// One case per rewrite row, the command that runs and a fragment of its line
const _REWRITTEN: readonly (readonly [string, string, string])[] = [
    ['NX_DAEMON=true nx show projects', 'nx show projects', 'Dropped NX_DAEMON=true'],
    ['mise x node@22 -- node -v', 'node -v', 'Ran node -v without mise x node@22 --'],
    ['eval "$(mise env -s bash)" && pnpm install', 'pnpm install', 'Dropped eval'],
    ['uv add httpx==0.28 rich', 'uv add httpx rich', 'uv.lock alone pins versions'],
    ['pnpm add effect@3 @effect/cli', 'pnpm add effect@catalog: @effect/cli', 'pnpm-workspace.yaml'],
    ['dotnet add package Foo --version 1.0', 'dotnet add package Foo', 'Directory.Packages.props'],
    ['sleep 1; ls; sleep 2', 'ls', 'Dropped sleep 1'],
];

// One case per line row over a command that runs as written, the fragment its line holds
const _LINED: readonly (readonly [string, string])[] = [
    ['mise x -C dir node@20 -- node app.js', 'Run the command directly'],
    ['eval "$(mise env -s bash)"', 'changes nothing'],
    ['git push -n', 'Dry run proves nothing, run git push'],
    ['sleep 60', 'sleep 60 holds the turn'],
    ['until [ -f x ]; do sleep 2; done; cat x', 'sleep 2 holds the turn'],
    ['gh issue create -t x', 'Use the github MCP for gh issue'],
];

const _PASSED: readonly string[] = [
    'echo "the phrase mise x appears in prose"',
    'mise env --json',
    'git commit -n -m x',
    'uv run pytest -n 4',
    'pnpm add effect@catalog:',
    'ls tools',
    "ast-grep test --include-off --filter '^no-json-parse$'",
    'echo "sleep 60 in prose"',
    'tail -f log | sleep 5',
    'NX_DAEMON=false nx show projects',
    'gh pr create --fill',
];

describe('shellRule', () => {
    it.each(_DENIED)('refuses %j', (command, fragments) => {
        const reason = _reason(command);
        for (const fragment of fragments) {
            expect(reason).toContain(fragment);
        }
    });

    it.each(_REWRITTEN)('rewrites %j', (command, expected, fragment) => {
        const [rewritten, context] = _rewritten(command);
        expect(rewritten).toBe(expected);
        expect(context.join('\n')).toContain(fragment);
    });

    it.each(_LINED)('runs %j as written with its line', (command, fragment) => {
        const [rewritten, context] = _rewritten(command);
        expect(rewritten).toBe(command);
        expect(context.join('\n')).toContain(fragment);
    });

    it.each(_PASSED)('passes %j', (command) => {
        expect(_fresh(command)).toStrictEqual({ kind: 'rewrite', e: { command }, context: [] });
    });

    it('adds each once line one time, none the session has seen, and the secret line on every redirect', () => {
        const command = 'grep -rn x a.cs b.cs && find . -name y';
        expect(shellOnce(command).map((line) => line.key)).toStrictEqual(['ast-grep', 'dotnet-roslyn-codelens', 'fd']);
        const seen = shellRule(new Set(['ast-grep']))({ command });
        expect(seen.kind === 'rewrite' ? seen.context.length : 0).toBe(2);
        expect(_rewritten("cat > x.md <<'EOF'\nop://Tokens/x\nEOF")[1]).toStrictEqual([OP_LINE]);
    });
});

describe('command bounds', () => {
    it('moves a timeout prefix to the parameter, the smaller bound for a whole command and the larger for a partial one', () => {
        expect(commandTimeout({ command: 'timeout 5 sleep 100', timeout: 120_000 })).toStrictEqual({
            kind: 'rewrite',
            e: { command: 'sleep 100', timeout: 5000 },
            context: ['Ran sleep 100 with the Bash timeout at 5000 ms'],
        });
        const partial = commandTimeout({ command: 'timeout 5m x && timeout -s KILL 2h y', timeout: 1000 });
        expect(partial.kind === 'rewrite' ? [partial.e.command, partial.e.timeout] : []).toStrictEqual(['x && y', _CEILING]);
        expect(partial.kind === 'rewrite' ? partial.context.at(-1) : '').toContain('Capped');
        expect(commandTimeout({ command: 'timeout x cmd' })).toStrictEqual({ kind: 'rewrite', e: { command: 'timeout x cmd' }, context: [] });
    });

    it('raises a slow foreground call to the ceiling silently and backgrounds a workflow job', () => {
        expect(commandCeiling({ command: 'pnpm exec nx run rasm:lint', timeout: 1000 })).toStrictEqual({
            kind: 'rewrite',
            e: { command: 'pnpm exec nx run rasm:lint', timeout: _CEILING },
            context: [],
        });
        expect(commandCeiling({ command: 'ls' })).toStrictEqual({ kind: 'rewrite', e: { command: 'ls' }, context: [] });
        const job: Bash = { command: 'nx run rasm:workflow' };
        const workflow = commandCeiling(job);
        expect(workflow.kind === 'rewrite' ? workflow.e.run_in_background : false).toBe(true);
    });

    it('replaces every npm word with the package manager', () => {
        expect(packageManager('pnpm')({ command: 'npm install && npm test' })).toStrictEqual({
            kind: 'rewrite',
            e: { command: 'pnpm install && pnpm test' },
            context: ['Ran pnpm in place of npm'],
        });
    });
});
