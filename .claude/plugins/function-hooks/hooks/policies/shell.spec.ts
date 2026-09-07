// --- [IMPORTS] -------------------------------------------------------------------------

import { describe, expect, it } from 'vitest';
import type { Decision } from '../composition/decision.ts';
import { commandTimeout, type NxCaches, nxTargets, packageManager, shellHits, shellRule, shellSkills, skipNxCache } from './shell.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Call {
    readonly tool: 'Bash';
    readonly command: string;
    readonly timeout?: number;
}

type Plain =
    | { readonly kind: 'rewrite'; readonly command: string; readonly timeout: number | undefined; readonly context: readonly string[] }
    | { readonly kind: 'deny'; readonly reason: string }
    | { readonly kind: 'answer' };

// --- [OPERATIONS] ----------------------------------------------------------------------

const _plain = (decision: Decision<Call, unknown, string>): Plain =>
    decision.match<Plain>({
        rewrite: (e, context) => ({ kind: 'rewrite', command: e.command, timeout: e.timeout, context }),
        deny: (reason) => ({ kind: 'deny', reason }),
        answer: () => ({ kind: 'answer' }),
    });

const _call = (command: string): Call => ({ tool: 'Bash', command });

const _fresh = (command: string): Plain => _plain(shellRule(new Set())(_call(command)));

const _seen = (command: string, seen: readonly string[]): Plain => _plain(shellRule(new Set(seen))(_call(command)));

const _pass = (command: string): Plain => ({ kind: 'rewrite', command, timeout: undefined, context: [] });

const _timed = (command: string, timeout?: number): Plain => _plain(commandTimeout({ tool: 'Bash', command, timeout }));

// Whether a reason holds every expected fragment
const _holdsAll =
    (fragments: readonly string[]) =>
    (reason: string): boolean =>
        fragments.every((fragment) => reason.includes(fragment));

const _skip = (command: string, caches: NxCaches): Plain => _plain(skipNxCache(caches)(_call(command)));

const _SCRATCHPAD = '/private/tmp/claude-501/slug/session/scratchpad';
const _GATE = '.claude/skills/ast-grep/scripts/rule-checks.sh';

// --- [CASES] ---------------------------------------------------------------------------

const _DENIED: readonly (readonly [string, readonly string[]])[] = [
    ['mise x -- node --version', ['mise x is refused', 'run node --version directly']],
    ['echo $(mise x -- node -v)', ['mise x is refused']],
    ["sh -c 'mise exec -- node -v'", ['mise x is refused', 'run node -v directly']],
    ['pnpm exec nx release --dry-run', ['Preview flags are refused', 'pnpm exec nx release']],
    ['git push -n', ['Preview flags are refused', 'git push']],
    ['pnpm add effect@3', ['Unpinned: pnpm add effect@catalog:']],
    ['pnpm add @effect/cli@3', ['Unpinned: pnpm add @effect/cli@catalog:']],
    ['uv add httpx==0.28', ['Unpinned: uv add httpx']],
    ['dotnet add package Foo --version 1.0', ['Unpinned: dotnet package add Foo', 'Directory.Packages.props']],
    ['yq r mise.toml tools', ['yq']],
    ['head -c 100 x.binlog', ['.binlog files are binary']],
    ["cat > tmp-proof.md <<'EOF'\nop://Tokens/x/credential\nEOF", ['Doppler']],
    [
        'ast-grep test -U',
        ['ast-grep test -U with no --filter rewrites every changed snapshot in the shared tree', "ast-grep test -U --filter '^<id>$'"],
    ],
    ['ast-grep test --update-all --include-off', ['ast-grep test -U with no --filter']],
    ['pnpm exec ast-grep test -U', ['ast-grep test -U with no --filter']],
    ['npx ast-grep test -U', ['ast-grep test -U with no --filter']],
    [
        `mkdir -p ${_SCRATCHPAD}/probe`,
        ['sits in the probe/ directory every agent of the session shares', `write under ${_SCRATCHPAD}/<label>-probe,`],
    ],
    [
        `cp a.ts ${_SCRATCHPAD}/probe/a.ts`,
        [`${_SCRATCHPAD}/probe/a.ts sits in the probe/ directory`, `write under ${_SCRATCHPAD}/<label>-probe/a.ts,`],
    ],
];

const _PASSED: readonly string[] = [
    'echo "the phrase mise x appears in prose"',
    'git commit -n -m x',
    'uv run pytest -n 4',
    'pnpm add effect@catalog:',
    'pnpm add @effect/cli',
    'uv add httpx',
    'ls tools',
    'dotnet build -bl',
    'gh pr checks',
    'echo "ast-grep test -U in prose"',
    'echo ast-grep test in prose',
    'make test',
    'echo "ast-grep test"',
    'echo ast-grep run -l typescript in prose',
    'biome check -l typescript',
    'echo "ast-grep run -l typescript"',
    'ast-grep run -l typescript -p x --stdin',
    `ast-grep run -c ${_SCRATCHPAD}/x-probe/sgconfig.yml -l typescript -p x .`,
    `ast-grep run --config=${_SCRATCHPAD}/x-probe/sgconfig.yml -l typescript -p x .`,
    'cd /tmp/x && ast-grep run -l typescript -p x .',
    'ast-grep run -l tsx -p typescript .',
    'ast-grep scan -l typescript .',
    'echo ast-grep scan --json -U in prose',
    'git log --json -U',
    'echo "ast-grep scan --json -U"',
    'ast-grep scan --json .',
    'ast-grep scan --json . | jq length',
    'echo ast-grep scan -i -U in prose',
    'git rebase -i -U',
    'echo "ast-grep scan -i -U"',
    'ast-grep scan -i .',
    `ls ${_SCRATCHPAD}/probe-env`,
    `${_GATE} pairing ts`,
    `${_GATE} gate xml`,
    _GATE,
    'rule-checks.sh gate',
    `${_GATE} gate ts '^no-filter-over-lifted-value$'`,
    `${_GATE} gate ts > gate.log`,
];

const _INCLUDE_OFF_LINE = 'Ran ast-grep test --include-off, the severity: off rewrite rules under rewrites/ run only under that flag';
const _TSX_LINE = 'Ran with -l tsx, sgconfig.yml languageGlobs maps every .ts file to tsx and typescript finds nothing';
const _JSON_LINE = 'Dropped --json beside -U, the two together write nothing';
const _INTERACTIVE_LINE = 'Dropped -i beside -U, -U accepts every diff and -i prompts on a terminal the Bash tool lacks';
const _STREAM_LINE = 'Ran --json=stream before wc -l, the array form counts one line';

// The command, its rewrite, and the context lines, and the rewrite passes unchanged on a second read
const _REWRITTEN: readonly (readonly [string, string, readonly string[]])[] = [
    ['ast-grep test', 'ast-grep test --include-off', [_INCLUDE_OFF_LINE]],
    ["ast-grep test -U --filter '^no-return-by-branch$'", "ast-grep test --include-off -U --filter '^no-return-by-branch$'", [_INCLUDE_OFF_LINE]],
    ["ast-grep test -U -f '^x$'", "ast-grep test --include-off -U -f '^x$'", [_INCLUDE_OFF_LINE]],
    ['ast-grep test -U --filter=^x$', 'ast-grep test --include-off -U --filter=^x$', [_INCLUDE_OFF_LINE]],
    [
        `ast-grep test -U -c ${_SCRATCHPAD}/x-probe/sgconfig.yml`,
        `ast-grep test --include-off -U -c ${_SCRATCHPAD}/x-probe/sgconfig.yml`,
        [_INCLUDE_OFF_LINE],
    ],
    [
        `ast-grep test -U --config ${_SCRATCHPAD}/x-probe/sgconfig.yml`,
        `ast-grep test --include-off -U --config ${_SCRATCHPAD}/x-probe/sgconfig.yml`,
        [_INCLUDE_OFF_LINE],
    ],
    [`ast-grep test -U -t ${_SCRATCHPAD}/x-probe/tests`, `ast-grep test --include-off -U -t ${_SCRATCHPAD}/x-probe/tests`, [_INCLUDE_OFF_LINE]],
    [
        `ast-grep test -U --test-dir ${_SCRATCHPAD}/x-probe/tests`,
        `ast-grep test --include-off -U --test-dir ${_SCRATCHPAD}/x-probe/tests`,
        [_INCLUDE_OFF_LINE],
    ],
    ['pnpm exec ast-grep test', 'pnpm exec ast-grep test --include-off', [_INCLUDE_OFF_LINE]],
    ["npx ast-grep test --filter '^x$'", "npx ast-grep test --include-off --filter '^x$'", [_INCLUDE_OFF_LINE]],
    ["ast-grep run -l typescript -p 'const $A = $B' tools/nx", "ast-grep run -l tsx -p 'const $A = $B' tools/nx", [_TSX_LINE]],
    ['ast-grep run --lang=typescript -p x tools', 'ast-grep run --lang=tsx -p x tools', [_TSX_LINE]],
    ['ast-grep run -l=typescript -p x tools', 'ast-grep run -l=tsx -p x tools', [_TSX_LINE]],
    ['ast-grep run -ltypescript -p x tools', 'ast-grep run -ltsx -p x tools', [_TSX_LINE]],
    ['ast-grep -p x -l typescript tools', 'ast-grep -p x -l tsx tools', [_TSX_LINE]],
    ['pnpm exec ast-grep run --lang typescript -p x tools', 'pnpm exec ast-grep run --lang tsx -p x tools', [_TSX_LINE]],
    [
        "ast-grep run -l tsx -p 'console.log($A)' -r 'console.info($A)' --json -U x.ts",
        "ast-grep run -l tsx -p 'console.log($A)' -r 'console.info($A)' -U x.ts",
        [_JSON_LINE],
    ],
    ['ast-grep scan --json=compact --update-all .', 'ast-grep scan --update-all .', [_JSON_LINE]],
    ['pnpm exec ast-grep scan -U --json .', 'pnpm exec ast-grep scan -U .', [_JSON_LINE]],
    ['ast-grep scan -i -U y.ts', 'ast-grep scan -U y.ts', [_INTERACTIVE_LINE]],
    ['npx ast-grep run -p x -r y --interactive -U .', 'npx ast-grep run -p x -r y -U .', [_INTERACTIVE_LINE]],
    ["ast-grep test -i -U --filter '^x$'", "ast-grep test --include-off -U --filter '^x$'", [_INCLUDE_OFF_LINE, _INTERACTIVE_LINE]],
];

// Commands with a wc -l leaf, read with the loc key seen so the wc row adds no line
const _COUNTED: readonly (readonly [string, string])[] = [
    [
        "ast-grep scan --filter '^no-catch-around-next$' --json .claude/plugins | wc -l",
        "ast-grep scan --filter '^no-catch-around-next$' --json=stream .claude/plugins | wc -l",
    ],
    ['ast-grep run -p x --json=compact tools | wc -l', 'ast-grep run -p x --json=stream tools | wc -l'],
    ['pnpm exec ast-grep scan --json=pretty . | wc -l', 'pnpm exec ast-grep scan --json=stream . | wc -l'],
    [
        'ast-grep scan --json . | wc -l && ast-grep scan --json . | jq length',
        'ast-grep scan --json=stream . | wc -l && ast-grep scan --json . | jq length',
    ],
];

const _COUNTED_PASSED: readonly string[] = [
    'echo ast-grep scan --json in prose | wc -l',
    'git log --json | wc -l',
    'echo "ast-grep scan --json" | wc -l',
    'ast-grep scan --json=stream . | wc -l',
    'ast-grep scan --json . ; wc -l x.txt',
    'ast-grep scan --json . > out.json; wc -l other.txt',
    'ast-grep scan . | wc -l',
];

const _CONTEXT: readonly (readonly [string, string, string])[] = [
    ['ls -R tools', 'tree', 'Use tree <dir>'],
    ["find tools -name '*.ts'", 'fd', 'Use fd for filesystem queries'],
    ['wc -l tools/nx/*.ts', 'loc', 'Use loc <dir>'],
    ['rg Foo libs/dotnet/Bar.cs', 'dotnet-roslyn-codelens', 'dotnet-roslyn-codelens'],
    ['rg Foo Directory.Build.props', 'dotnet-msbuild-evaluation', 'dotnet-msbuild-evaluation'],
    ["rg Foo 'tools/**/*.ts'", 'ast-grep', 'ast-grep'],
    ['grep -rn foo x', 'ast-grep', 'rg is for literals and comments'],
    ['grep -r foo x', 'ast-grep', 'rg is for literals and comments'],
    ['grep -P foo x', 'ast-grep', 'rg is for literals and comments'],
    ["grep 'a+' values.txt", 'ast-grep', 'rg is for literals and comments'],
    ["grep -E 'a+' values.txt", 'ast-grep', 'rg is for literals and comments'],
    ["grep -r --include='*.txt' needle .", 'ast-grep', 'rg is for literals and comments'],
    ["grep -R --exclude='*.log' needle .", 'ast-grep', 'rg is for literals and comments'],
    ['gh pr merge 1', 'github', 'github MCP for gh pr merge'],
    ['dotnet build Workspace.slnx', 'dotnet-msbuild-diagnostics', 'Add -bl to dotnet build'],
];

const _TIMEOUT_DENIED: readonly (readonly [string, readonly string[]])[] = [
    ['timeout 5s echo ok', ['The timeout duration 5s is not whole seconds', 'run echo ok with the Bash timeout parameter']],
    ['timeout 0.5 echo ok', ['The timeout duration 0.5 is not whole seconds']],
    ['timeout -s KILL 10 echo ok', ['The timeout options -s KILL have no Bash form', 'run echo ok with the Bash timeout parameter']],
    ['timeout -k 5 10 echo ok', ['The timeout options -k 5 have no Bash form']],
    ['timeout --signal=KILL --foreground 10 echo ok', ['The timeout options --signal=KILL --foreground have no Bash form']],
    [
        "timeout 30 sh -c 'echo ok'",
        ['The timeout prefix wraps a shell or nothing', 'run what it wraps with the Bash timeout parameter in milliseconds (max 600000)'],
    ],
    [
        "sh -c 'timeout 30 echo ok'",
        ['timeout inside a compound command has no exact Bash form', "run sh -c 'echo ok' with the Bash timeout parameter"],
    ],
    ['echo $(timeout 5 echo ok)', ['timeout inside a compound command', 'run echo $(echo ok) with the Bash timeout parameter']],
    ['echo ok | timeout 5 cat', ['timeout inside a compound command', 'run echo ok | cat with the Bash timeout parameter']],
    ['timeout 30 echo ok && ls', ['timeout inside a compound command', 'run echo ok && ls with the Bash timeout parameter']],
    [
        'timeout 700 sleep 1',
        ['timeout 700 exceeds the Bash timeout maximum of 600000 ms', 'run sleep 1 with timeout: 600000 or run_in_background: true'],
    ],
    ['timeout', ['The timeout prefix wraps a shell or nothing']],
    ['timeout 30', ['The timeout prefix wraps a shell or nothing']],
    ['timeout -s KILL', ['timeout names no duration', 'run the command with the Bash timeout parameter']],
    ['timeout 5s', ['timeout names no command']],
];

const _TIMEOUT_PASSED: readonly string[] = ['echo "the word timeout in prose"', 'git config --get http.timeout', 'sleep 1'];

// Durations in milliseconds, the Bash timeout parameter's unit and its maximum
const _FIVE_SECONDS = 5000;
const _TEN_SECONDS = 10_000;
const _THIRTY_SECONDS = 30_000;
const _TWO_MINUTES = 120_000;
const _TEN_MINUTES = 600_000;

// --- [TESTS] ---------------------------------------------------------------------------

describe('shellRule', () => {
    it.each(_DENIED)('denies %j', (command, fragments) => {
        expect(_fresh(command)).toStrictEqual({
            kind: 'deny',
            reason: expect.toSatisfy(_holdsAll(fragments)),
        });
    });

    it.each(_PASSED)('passes %j', (command) => {
        expect(_fresh(command)).toStrictEqual(_pass(command));
    });

    it.each(_REWRITTEN)('rewrites %j to %j and passes the rewrite unchanged', (command, rewritten, context) => {
        expect(_fresh(command)).toStrictEqual({ kind: 'rewrite', command: rewritten, timeout: undefined, context });
        expect(_fresh(rewritten)).toStrictEqual(_pass(rewritten));
    });

    it.each(_COUNTED)('rewrites %j to %j before wc -l and passes the rewrite unchanged', (command, rewritten) => {
        expect(_seen(command, ['loc'])).toStrictEqual({ kind: 'rewrite', command: rewritten, timeout: undefined, context: [_STREAM_LINE] });
        expect(_seen(rewritten, ['loc'])).toStrictEqual(_pass(rewritten));
    });

    it.each(_COUNTED_PASSED)('passes %j beside wc -l', (command) => {
        expect(_seen(command, ['loc'])).toStrictEqual(_pass(command));
    });

    it.each(_CONTEXT)('adds context for %j once under the key %j', (command, once, fragment) => {
        const plain = _fresh(command);
        expect(plain).toMatchObject({ kind: 'rewrite', command });
        expect(plain).toMatchObject({ context: [expect.stringContaining(fragment)] });
        expect(_seen(command, [once])).toStrictEqual(_pass(command));
    });

    it('keeps the first line of a once key shared by two rows', () => {
        expect(_fresh("grep -P Foo 'tools/**/*.ts'")).toMatchObject({ context: ['Load the ast-grep skill, rg is for literals and comments'] });
    });

    it.each(['ts', 'py', 'sh', 'yml', 'csproj', 'cs', 'json'])(
        'rewrites a direct gate %s run to the rasm:rules configuration of its language',
        (ext) => {
            expect(_fresh(`${_GATE} gate ${ext}`)).toStrictEqual({
                kind: 'rewrite',
                command: `pnpm exec nx run rasm:rules:${ext}`,
                timeout: undefined,
                context: [`Ran the gate through nx run rasm:rules:${ext}, a rerun with no change under tools/ast-grep/ replays the cached result`],
            });
        },
    );
});

describe('skipNxCache', () => {
    it('drops the flag with the context line on a target whose manifest states cache: false', () => {
        expect(_skip('nx run rasm:rewrite --id=x --paths=y --skip-nx-cache', { 'rasm:rewrite': false })).toStrictEqual({
            kind: 'rewrite',
            command: 'nx run rasm:rewrite --id=x --paths=y',
            timeout: undefined,
            context: ['Dropped --skip-nx-cache, rasm:rewrite declares cache: false and never reads the cache'],
        });
        expect(_skip('pnpm exec nx run rasm:rewrite --skipNxCache --id=x', { 'rasm:rewrite': false })).toStrictEqual({
            kind: 'rewrite',
            command: 'pnpm exec nx run rasm:rewrite --id=x',
            timeout: undefined,
            context: ['Dropped --skipNxCache, rasm:rewrite declares cache: false and never reads the cache'],
        });
    });

    it('passes a cached target, a target with no flag read, a run-many, and a run without the flag', () => {
        expect(_skip('nx run rasm:lint --skip-nx-cache', { 'rasm:lint': true })).toStrictEqual(_pass('nx run rasm:lint --skip-nx-cache'));
        expect(_skip('nx run rasm:check --skip-nx-cache', {})).toStrictEqual(_pass('nx run rasm:check --skip-nx-cache'));
        expect(_skip('nx run-many -t check --skip-nx-cache', {})).toStrictEqual(_pass('nx run-many -t check --skip-nx-cache'));
        expect(_skip('nx run rasm:rewrite --id=x --paths=y', { 'rasm:rewrite': false })).toStrictEqual(_pass('nx run rasm:rewrite --id=x --paths=y'));
    });
});

describe('nxTargets', () => {
    it('lists the project, target, and flag word of each nx run leaf that carries a skip-cache flag', () => {
        expect(
            nxTargets('pnpm exec nx run rasm:rewrite --skipNxCache --id=x && nx run a:b && nx run c:d --skip-nx-cache').map((named) => [
                named.project,
                named.target,
                named.flag.text,
            ]),
        ).toStrictEqual([
            ['rasm', 'rewrite', '--skipNxCache'],
            ['c', 'd', '--skip-nx-cache'],
        ]);
    });
});

describe('packageManager', () => {
    it('rewrites the npm word to the configured package manager', () => {
        expect(_plain(packageManager('pnpm')(_call('npm install')))).toStrictEqual({
            kind: 'rewrite',
            command: 'pnpm install',
            timeout: undefined,
            context: ['Ran pnpm in place of npm'],
        });
    });

    it('rewrites an npm leaf after a separator', () => {
        expect(_plain(packageManager('pnpm')(_call('cd x && npm install')))).toStrictEqual({
            kind: 'rewrite',
            command: 'cd x && pnpm install',
            timeout: undefined,
            context: ['Ran pnpm in place of npm'],
        });
    });

    it('passes a command without npm', () => {
        expect(_plain(packageManager('pnpm')(_call('pnpm install')))).toStrictEqual(_pass('pnpm install'));
    });
});

describe('shellSkills', () => {
    it('names the once key of each context hit in table order', () => {
        expect(shellSkills('ls -R tools && find tools && grep -P foo x')).toStrictEqual(['ast-grep', 'tree', 'fd']);
    });
});

describe('shellHits', () => {
    it('lists every row and leaf pair in table order', () => {
        const hits = shellHits('mise x -- node -v && ls -R .');
        expect(hits.map((hit) => [hit.row.word[0], hit.leaf[0]?.text])).toStrictEqual([
            ['mise', 'mise'],
            ['ls', 'ls'],
        ]);
    });
});

describe('commandTimeout', () => {
    it.each(_TIMEOUT_DENIED)('denies %j', (command, fragments) => {
        expect(_timed(command)).toStrictEqual({
            kind: 'deny',
            reason: expect.toSatisfy(_holdsAll(fragments)),
        });
    });

    it.each(_TIMEOUT_PASSED)('passes %j', (command) => {
        expect(_timed(command)).toStrictEqual(_pass(command));
    });

    it.each(['timeout', 'gtimeout'])('moves %s duration to the Bash parameter', (command) => {
        expect(_timed(`${command} 30 sleep 1`)).toStrictEqual({
            kind: 'rewrite',
            command: 'sleep 1',
            timeout: _THIRTY_SECONDS,
            context: ['Ran sleep 1 under the Bash timeout parameter at 30000 ms'],
        });
    });

    it('reads the timeout word past an env assignment and keeps the assignment', () => {
        expect(_timed('FOO=1 timeout 30 sleep 1')).toMatchObject({ kind: 'rewrite', command: 'FOO=1 sleep 1', timeout: _THIRTY_SECONDS });
    });

    it('keeps the quoting of the wrapped command and the maximum duration', () => {
        expect(_timed('timeout 600 echo "a b"')).toMatchObject({ kind: 'rewrite', command: 'echo "a b"', timeout: _TEN_MINUTES });
    });

    it('takes the smaller of the prefix and the parameter already on the input', () => {
        expect(_timed('timeout 30 sleep 1', _TWO_MINUTES)).toMatchObject({ kind: 'rewrite', command: 'sleep 1', timeout: _THIRTY_SECONDS });
        expect(_timed('timeout 30 sleep 1', _TEN_SECONDS)).toMatchObject({
            kind: 'rewrite',
            command: 'sleep 1',
            timeout: _TEN_SECONDS,
            context: ['Ran sleep 1 under the Bash timeout parameter at 10000 ms'],
        });
    });

    it('leaves the parameter of a command without the prefix', () => {
        expect(_timed('sleep 1', _FIVE_SECONDS)).toStrictEqual({ kind: 'rewrite', command: 'sleep 1', timeout: _FIVE_SECONDS, context: [] });
    });
});
