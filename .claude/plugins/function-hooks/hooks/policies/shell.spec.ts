// --- [IMPORTS] -------------------------------------------------------------------------

import { describe, expect, it } from 'vitest';
import { type Decision, fold } from '../composition/decision.ts';
import { gitGuard } from './git.ts';
import { commandCeiling, commandTimeout, type NxCaches, nxTargets, packageManager, shellHits, shellRule, shellSkills, skipNxCache } from './shell.ts';

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
const _RULE_CHECKS = '.claude/skills/ast-grep/scripts/rule-checks.sh';

// --- [CASES] ---------------------------------------------------------------------------

const _DENIED: readonly (readonly [string, readonly string[]])[] = [
    ['mise x', ['mise x names no command', 'run the command directly']],
    ['mise exec node@22', ['mise exec names no command']],
    ['mise x zizmor@latest --', ['mise x names no command']],
    [
        'mise x -C dir node@20 node -v',
        ['mise x holds an option before the command', 'mise x [<tool>@<version>...] -- <command>', '-C <dir> as cd <dir> &&'],
    ],
    ['mise x -C dir node@20 -- node app.js', ['mise x holds an option before the command']],
    ['mise exec node@20 --command "node -v"', ['mise exec holds an option before the command', '--command as the command']],
    ['mise x -j 4 -- node -v', ['mise x holds an option before the command']],
    ['eval "$(mise env -s bash)"', ['eval "$(mise env)" names no command', 'run the command directly']],
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
    // Every NX_DAEMON value but false outranks nx.json useDaemonProcess: false, the client's enabled() reads false and unset alone as off
    ['NX_DAEMON=true nx show projects', ['NX_DAEMON=true starts the Nx daemon', 'useDaemonProcess: false', 'never rotates, run nx show projects']],
    ['ls && NX_DAEMON=1 NO_COLOR=1 nx run rasm:lint python', ['NX_DAEMON=1 starts the Nx daemon', 'run ls && NO_COLOR=1 nx run rasm:lint python']],
    ['sleep 60', ['sleep 60 waits on nothing', 'run_in_background: true', 'until loop under the Monitor tool']],
    ['sleep', ['sleep waits on nothing']],
    ['sleep 5; sleep 10', ['sleep 5 waits on nothing']],
    ['sleep 2 &&\n  sleep 3', ['sleep 2 waits on nothing']],
    ['NO_COLOR=1 sleep 60', ['sleep 60 waits on nothing']],
    // The lint retry loop, do if holds two openers and fi closes one, so sleep 30 sits inside the loop body
    [
        'for i in $(seq 1 10); do if pnpm exec nx run function-hooks:lint --skip-nx-cache >/dev/null 2>&1; then echo "lint passed on try $i"; exit 0; fi; sleep 30; done; echo "lint still failing"; exit 1',
        ['sleep 30 inside a loop body', 'run_in_background: true', 'until loop under the Monitor tool', 'expect -c'],
    ],
    // The paced feed into script, the group's closing paren feeds a pipe
    [
        "( sleep 10; printf 'a\\n'; sleep 20; printf 'b\\n' ) | script -q /dev/null claude",
        ['sleep 10 inside a group that feeds a pipe', 'expect -c on a pseudo-terminal', 'run_in_background: true'],
    ],
    ['until [ -f x ]; do sleep 2; done; cat x', ['sleep 2 inside a loop body', 'until loop under the Monitor tool']],
    ['until [ -f x ]; do\n    sleep 2\ndone\ncat x', ['sleep 2 inside a loop body']],
    ['for i in 1 2; do sleep 1; done', ['sleep 1 inside a loop body']],
    ['if [ -f x ]; then\n    sleep 1\nfi', ['sleep 1 inside a conditional body']],
    ['if [ -f x ]; then sleep 1; fi', ['sleep 1 inside a conditional body']],
    ['while sleep 1; do curl -s x && break; done', ['sleep 1 inside a loop body']],
    ['{ sleep 1; echo ok; }', ['sleep 1 inside a brace group body']],
    ['case $x in a) sleep 1;; esac', ['sleep 1 inside a case body']],
];

const _PASSED: readonly string[] = [
    'echo "the phrase mise x appears in prose"',
    'mise env --json',
    'mise install',
    'echo "$(mise env -s bash)"',
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
    `ast-grep run -c ${_SCRATCHPAD}/own-tree/sgconfig.yml -l typescript -p x .`,
    `ast-grep run --config=${_SCRATCHPAD}/own-tree/sgconfig.yml -l typescript -p x .`,
    'cd /tmp/x && ast-grep run -l typescript -p x .',
    'ast-grep run -l tsx -p typescript .',
    'ast-grep run -p x -r y .',
    `ast-grep scan -r ${_SCRATCHPAD}/own-tree/draft.yml .`,
    `ast-grep scan -c ${_SCRATCHPAD}/own-tree/sgconfig.yml -r tools/ast-grep/rules/a/b/c.yml .`,
    'echo "ast-grep scan -r tools/ast-grep/rules/a/b/c.yml"',
    "ast-grep test --include-off --filter '^no-json-parse$'",
    `ast-grep test --include-off -t ${_SCRATCHPAD}/own-tree/tests`,
    'echo "sleep 60 in prose"',
    'sleep 1 | cat',
    '(cd tools && ls) | cat',
    'NO_COLOR= rg leaves tools',
    'export NO_COLOR=1',
    'FORCE_COLOR=1 pnpm exec nx run function-hooks:test',
    'rg --color always leaves tools',
    'git status --no-color',
    'git blame --no-color README.md',
    'shellcheck --color never x.sh',
    'uv run pytest --color never',
    'dotnet test -tl:off',
    'nx-cloud status',
    'nx run rasm:lint python',
    'nx run rasm:lint README.md tools/nx',
    'nx run rasm:format libs/dotnet/interop/Rasm.Interop',
    'dotnet build -bl:.artifacts/binlog/build-{}.binlog',
    'mise which node',
    'mise which -t node@22 node',
    'echo ast-grep scan --json -U in prose',
    'git log --json -U',
    'echo "ast-grep scan --json -U"',
    'ast-grep scan --json .',
    'ast-grep scan --json . | jq length',
    'echo ast-grep scan -i -U in prose',
    'git rebase -i -U',
    'echo "ast-grep scan -i -U"',
    'ast-grep scan -i .',
    `ls ${_SCRATCHPAD}/env`,
    `${_RULE_CHECKS} pairing ts`,
    _RULE_CHECKS,
];

const _INCLUDE_OFF_LINE = 'Ran ast-grep test --include-off, the severity: off rewrite rules under rewrites/ run only under that flag';
const _TSX_LINE = 'Ran with -l tsx, sgconfig.yml languageGlobs maps every .ts file to tsx and typescript finds nothing';
const _JSON_LINE = 'Dropped --json beside -U, the two together write nothing';
const _INTERACTIVE_LINE = 'Dropped -i beside -U, -U accepts every diff and -i prompts on a terminal the Bash tool lacks';
const _STREAM_LINE = 'Ran --json=stream before wc -l, the array form counts one line';
const _WAIT_LINE =
    'run the command it waits for with run_in_background: true and read its completion notification, or watch the condition with an until loop under the Monitor tool';

const _sleepLine = (leaf: string): string => `Dropped ${leaf}, ${_WAIT_LINE}`;

const _ruleLine = (filter: string, flag: string): string =>
    `Ran ${filter} in place of ${flag}, scan -r loads no utilDirs and the root sgconfig.yml registers the rule`;

const _testIdLine = (pattern: string): string =>
    `Ran ast-grep test --filter '${pattern}', test takes no positional and --filter selects the cases by rule id`;

const _langLine = (words: string): string => `Dropped ${words}, scan takes no language flag and each rule's language field parses its files`;

const _miseLine = (rest: string, prefix: string): string =>
    `Ran ${rest} in place of ${prefix}, the SessionStart hook wrote the mise environment to CLAUDE_ENV_FILE, and a missing binary is a missing [tools] row in mise.toml followed by mise install`;

const _evalLine = (body: string): string => `Dropped eval "$(${body})", the SessionStart hook wrote the mise environment to CLAUDE_ENV_FILE`;

const _WHICH_LINE = 'Ran one mise which per name, which takes one BIN_NAME and refuses a second with unexpected argument';

const _binlogLine = (stamped: string, fixed: string): string =>
    `Ran ${stamped} in place of ${fixed}, a fixed binlog name overwrites the last capture and collides across sessions, and {} stamps the name with the date, time, process id, and a random suffix`;

// The command, its rewrite, and the context lines, and the rewrite passes unchanged on a second read
const _REWRITTEN: readonly (readonly [string, string, readonly string[]])[] = [
    // Env prefixes, proven byte-identical under the Bash tool: NO_COLOR=1 over rg, fd, nx show projects, git log, ast-grep, dotnet build (SAME, esc 0/0),
    // CLICOLOR=0 over ls and rg (SAME), TERM=dumb over rg, nx, git log, dotnet build (SAME) and the test target (41/41 escapes)
    ['NO_COLOR=1 rg -n leaves tools', 'rg -n leaves tools', []],
    // FORCE_COLOR=0 sits in the settings env beside NO_COLOR, the test target prints 0 escapes under both exported against 41 under NO_COLOR alone
    ['FORCE_COLOR=0 pnpm exec nx run function-hooks:test', 'pnpm exec nx run function-hooks:test', []],
    ['NO_COLOR=true fd argv .claude/plugins', 'fd argv .claude/plugins', []],
    ['NO_COLOR=1 TERM=dumb pnpm exec nx show projects', 'pnpm exec nx show projects', []],
    ['CLICOLOR=0 ls -la tools', 'ls -la tools', []],
    ['TERM=dumb dotnet build -bl', 'dotnet build -bl', []],
    ['NX_TUI=false NO_COLOR=1 pnpm exec nx show projects', 'NX_TUI=false pnpm exec nx show projects', []],
    // NX_DAEMON=false restates nx.json useDaemonProcess: false, the client reads false and unset alone as off
    ['NX_DAEMON=false pnpm exec nx show projects', 'pnpm exec nx show projects', []],
    ['ls && NO_COLOR=1 git log -3 --oneline', 'ls && git log -3 --oneline', []],
    // Color flags, proven byte-identical under the Bash tool per tool and spelling (SAME, esc 0/0): rg, fd, ast-grep run|scan|test and the implicit run,
    // typos, difft, ruff check, uv tree, uv sync with both spellings, shellcheck --color=never, git log|diff|show|branch|grep --no-color,
    // dotnet build -tl:off with Time Elapsed filtered, tsc --pretty false over a failing file (exit 1 both)
    ['rg --color never -n leaves tools', 'rg -n leaves tools', []],
    ['rg -n --color=never leaves tools', 'rg -n leaves tools', []],
    ['fd --color never argv .claude/plugins', 'fd argv .claude/plugins', []],
    ['ast-grep scan --color=never tools/nx', 'ast-grep scan tools/nx', []],
    ['ast-grep --color=never -p x tools', 'ast-grep -p x tools', []],
    ['pnpm exec ast-grep run --color never -p x tools', 'pnpm exec ast-grep run -p x tools', []],
    ['git log -3 --oneline --no-color', 'git log -3 --oneline', []],
    ['git diff --no-color --stat', 'git diff --stat', []],
    ['uv tree --color never --depth 1', 'uv tree --depth 1', []],
    ['uv run ruff check --color=never eng/scripts', 'uv run ruff check eng/scripts', []],
    ['typos --color never tools/nx', 'typos tools/nx', []],
    ['shellcheck --color=never x.sh', 'shellcheck x.sh', []],
    ['difft --color never README.md CLAUDE.md', 'difft README.md CLAUDE.md', []],
    ['dotnet build Workspace.slnx -tl:off -bl', 'dotnet build Workspace.slnx -bl', []],
    ['pnpm exec tsc --noEmit --pretty false', 'pnpm exec tsc --noEmit', []],
    ['NO_COLOR=1 rg --color=never leaves tools', 'rg leaves tools', []],
    // The {} stamp expanded to 20260908-071941--51905--Uizs3j in a run
    [
        'dotnet build Workspace.slnx -bl:.artifacts/build.binlog',
        'dotnet build Workspace.slnx -bl:.artifacts/build-{}.binlog',
        [_binlogLine('-bl:.artifacts/build-{}.binlog', '-bl:.artifacts/build.binlog')],
    ],
    ['dotnet build -bl:x.binlog', 'dotnet build -bl:x-{}.binlog', [_binlogLine('-bl:x-{}.binlog', '-bl:x.binlog')]],
    [
        'dotnet msbuild x.csproj /bl:LogFile=out/x.binlog',
        'dotnet msbuild x.csproj /bl:LogFile=out/x-{}.binlog',
        [_binlogLine('/bl:LogFile=out/x-{}.binlog', '/bl:LogFile=out/x.binlog')],
    ],
    // mise which node pnpm exits 2 with unexpected argument 'pnpm' found
    ['mise which node pnpm', 'mise which node; mise which pnpm', [_WHICH_LINE]],
    ['mise which node pnpm uv | head -1', 'mise which node; mise which pnpm; mise which uv | head -1', [_WHICH_LINE]],
    ['mise which --plugin node pnpm', 'mise which --plugin node; mise which --plugin pnpm', [_WHICH_LINE]],
    ['eval "$(mise env -s bash)" && zizmor --version', 'zizmor --version', [_evalLine('mise env -s bash')]],
    ['eval "$(mise env)"; node -v', 'node -v', [_evalLine('mise env')]],
    ['eval $(mise env -s bash) && node -v', 'node -v', [_evalLine('mise env -s bash')]],
    ['eval "$(mise env -s bash)"\nnode -v', 'node -v', [_evalLine('mise env -s bash')]],
    ['eval "$(mise env -s bash)" && git push --force', 'git push --force', [_evalLine('mise env -s bash')]],
    ['eval "$(mise env -s bash)" && mise x -- node -v', 'node -v', [_evalLine('mise env -s bash'), _miseLine('node -v', 'mise x --')]],
    ['mise x zizmor@latest -- zizmor --version', 'zizmor --version', [_miseLine('zizmor --version', 'mise x zizmor@latest --')]],
    ['mise exec -- zizmor --version', 'zizmor --version', [_miseLine('zizmor --version', 'mise exec --')]],
    ['mise x zizmor@latest -- zizmor --help', 'zizmor --help', [_miseLine('zizmor --help', 'mise x zizmor@latest --')]],
    ['mise x zizmor@latest -- zizmor --help 2>&1', 'zizmor --help 2>&1', [_miseLine('zizmor --help 2>&1', 'mise x zizmor@latest --')]],
    ['mise exec -- node -v', 'node -v', [_miseLine('node -v', 'mise exec --')]],
    ['mise x node@22 node -v', 'node -v', [_miseLine('node -v', 'mise x node@22')]],
    ['mise x node@22 python@3.11 -- node -v', 'node -v', [_miseLine('node -v', 'mise x node@22 python@3.11 --')]],
    ['mise x -- echo "a b"', 'echo "a b"', [_miseLine('echo "a b"', 'mise x --')]],
    ['echo $(mise x -- node -v)', 'echo $(node -v)', [_miseLine('node -v', 'mise x --')]],
    ["sh -c 'mise exec -- node -v'", "sh -c 'node -v'", [_miseLine('node -v', 'mise exec --')]],
    ['mise x -- node -v | head -1', 'node -v | head -1', [_miseLine('node -v', 'mise x --')]],
    ['mise x -- node -v && ls tools', 'node -v && ls tools', [_miseLine('node -v', 'mise x --')]],
    ['ast-grep test', 'ast-grep test --include-off', [_INCLUDE_OFF_LINE]],
    ["ast-grep test -U --filter '^no-return-by-branch$'", "ast-grep test --include-off -U --filter '^no-return-by-branch$'", [_INCLUDE_OFF_LINE]],
    ["ast-grep test -U -f '^x$'", "ast-grep test --include-off -U -f '^x$'", [_INCLUDE_OFF_LINE]],
    ['ast-grep test -U --filter=^x$', 'ast-grep test --include-off -U --filter=^x$', [_INCLUDE_OFF_LINE]],
    [
        `ast-grep test -U -c ${_SCRATCHPAD}/own-tree/sgconfig.yml`,
        `ast-grep test --include-off -U -c ${_SCRATCHPAD}/own-tree/sgconfig.yml`,
        [_INCLUDE_OFF_LINE],
    ],
    [
        `ast-grep test -U --config ${_SCRATCHPAD}/own-tree/sgconfig.yml`,
        `ast-grep test --include-off -U --config ${_SCRATCHPAD}/own-tree/sgconfig.yml`,
        [_INCLUDE_OFF_LINE],
    ],
    [`ast-grep test -U -t ${_SCRATCHPAD}/own-tree/tests`, `ast-grep test --include-off -U -t ${_SCRATCHPAD}/own-tree/tests`, [_INCLUDE_OFF_LINE]],
    [
        `ast-grep test -U --test-dir ${_SCRATCHPAD}/own-tree/tests`,
        `ast-grep test --include-off -U --test-dir ${_SCRATCHPAD}/own-tree/tests`,
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
    [
        'ast-grep scan -r tools/ast-grep/rules/python/pydantic/no-os-environ.yml eng/scripts/provision.py',
        "ast-grep scan --filter '^no-os-environ$' eng/scripts/provision.py",
        [_ruleLine("--filter '^no-os-environ$'", '-r tools/ast-grep/rules/python/pydantic/no-os-environ.yml')],
    ],
    [
        'pnpm exec ast-grep scan --rule /Users/x/Rasm/tools/ast-grep/rules/typescript/claude-code/require-is-tool.yml --json=stream .claude/plugins',
        "pnpm exec ast-grep scan --filter '^require-is-tool$' --json=stream .claude/plugins",
        [_ruleLine("--filter '^require-is-tool$'", '--rule /Users/x/Rasm/tools/ast-grep/rules/typescript/claude-code/require-is-tool.yml')],
    ],
    [
        'ast-grep scan --rule=tools/ast-grep/rewrites/bash/tooling/npm-command-to-pnpm.yml .github',
        "ast-grep scan --filter '^npm-command-to-pnpm$' --error=npm-command-to-pnpm .github",
        [
            _ruleLine(
                "--filter '^npm-command-to-pnpm$' --error=npm-command-to-pnpm",
                '--rule=tools/ast-grep/rewrites/bash/tooling/npm-command-to-pnpm.yml',
            ),
        ],
    ],
    ['ast-grep test no-json-parse', "ast-grep test --include-off --filter '^no-json-parse$'", [_INCLUDE_OFF_LINE, _testIdLine('^no-json-parse$')]],
    [
        'ast-grep test -U no-json-parse',
        "ast-grep test --include-off -U --filter '^no-json-parse$'",
        [_INCLUDE_OFF_LINE, _testIdLine('^no-json-parse$')],
    ],
    [
        'ast-grep test --include-off no-json-parse no-never-arm',
        "ast-grep test --include-off --filter '^(no-json-parse|no-never-arm)$'",
        [_testIdLine('^(no-json-parse|no-never-arm)$')],
    ],
    ['ast-grep scan -l typescript .', 'ast-grep scan .', [_langLine('-l typescript')]],
    [
        'ast-grep scan --lang=tsx --stdin --inline-rules "$(cat x.yml)"',
        'ast-grep scan --stdin --inline-rules "$(cat x.yml)"',
        [_langLine('--lang=tsx')],
    ],
    ['ast-grep scan -ltsx tools/nx', 'ast-grep scan tools/nx', [_langLine('-ltsx')]],
    ['sleep 60 && ast-grep scan --filter x .', 'ast-grep scan --filter x .', [_sleepLine('sleep 60')]],
    ['sleep 5; echo ok', 'echo ok', [_sleepLine('sleep 5')]],
    ['echo ok; sleep 5', 'echo ok', [_sleepLine('sleep 5')]],
    ['sleep 2 &&\n  echo ok', 'echo ok', [_sleepLine('sleep 2')]],
    ['echo a; sleep 1; echo b', 'echo a; echo b', [_sleepLine('sleep 1')]],
    ['sleep 1; sleep 2; echo ok', 'echo ok', [_sleepLine('sleep 1'), _sleepLine('sleep 2')]],
    ['echo ok; sleep 1; sleep 2', 'echo ok', [_sleepLine('sleep 1'), _sleepLine('sleep 2')]],
    ["sh -c 'sleep 1; echo ok'", "sh -c 'echo ok'", [_sleepLine('sleep 1')]],
    ['sleep 1 2>/dev/null && echo ok', 'echo ok', [_sleepLine('sleep 1 2>/dev/null')]],
    ['sleep 3 & echo ok', 'echo ok', [_sleepLine('sleep 3')]],
    ['sleep 60 && ast-grep test', 'ast-grep test --include-off', [_sleepLine('sleep 60'), _INCLUDE_OFF_LINE]],
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
    [
        'timeout abc echo ok',
        ['The timeout duration abc is no number with an optional s, m, h, or d suffix', 'run echo ok with the Bash timeout parameter'],
    ],
    ['timeout 5x echo ok', ['The timeout duration 5x is no number']],
    ['timeout', ['timeout names no duration', 'run the command with the Bash timeout parameter']],
    ['timeout 30', ['timeout names no command']],
    ['ls && timeout 30', ['timeout names no command']],
    ['timeout -s KILL', ['timeout names no duration', 'run the command with the Bash timeout parameter']],
    ['timeout 5s', ['timeout names no command']],
    ['ls && timeout 5s', ['timeout names no command']],
];

const _TIMEOUT_PASSED: readonly string[] = [
    'echo "the word timeout in prose"',
    'git config --get http.timeout',
    'sleep 1',
    'echo "timeout 5 x" && ls',
    "echo 'run `timeout 30 x` later'",
];

// Durations in milliseconds, the Bash timeout parameter's unit and its maximum
const _HALF_SECOND = 500;
const _FIVE_SECONDS = 5000;
const _TEN_SECONDS = 10_000;
const _THIRTY_SECONDS = 30_000;
const _TWO_MINUTES = 120_000;
const _FIVE_MINUTES = 300_000;
const _TEN_MINUTES = 600_000;

const _bashTimeoutLine = (command: string, bound: number): string => `Ran ${command} under the Bash timeout parameter at ${bound} ms`;

const _cappedLine = (duration: string): string =>
    `timeout ${duration} exceeds the Bash timeout maximum of 600000 ms and ran capped, run_in_background: true runs past it`;

// The command, its rewrite, the parameter, and the context lines, the prefixes leave by span with the redirects and quoting byte for byte
const _TIMEOUT_REWRITTEN: readonly (readonly [string, string, number, readonly string[]])[] = [
    ['timeout 5s echo ok', 'echo ok', _FIVE_SECONDS, [_bashTimeoutLine('echo ok', _FIVE_SECONDS)]],
    ['timeout 0.5 echo ok', 'echo ok', _HALF_SECOND, [_bashTimeoutLine('echo ok', _HALF_SECOND)]],
    ['timeout 2m echo ok', 'echo ok', _TWO_MINUTES, [_bashTimeoutLine('echo ok', _TWO_MINUTES)]],
    ['timeout 1h echo ok', 'echo ok', _TEN_MINUTES, [_bashTimeoutLine('echo ok', _TEN_MINUTES), _cappedLine('1h')]],
    ['timeout 700 sleep 1', 'sleep 1', _TEN_MINUTES, [_bashTimeoutLine('sleep 1', _TEN_MINUTES), _cappedLine('700')]],
    ['timeout -s KILL 10 echo ok', 'echo ok', _TEN_SECONDS, [_bashTimeoutLine('echo ok', _TEN_SECONDS)]],
    ['timeout -k 5 10 echo ok', 'echo ok', _TEN_SECONDS, [_bashTimeoutLine('echo ok', _TEN_SECONDS)]],
    ['timeout --signal=KILL --foreground 10 echo ok', 'echo ok', _TEN_SECONDS, [_bashTimeoutLine('echo ok', _TEN_SECONDS)]],
    ["sh -c 'timeout 30 echo ok'", "sh -c 'echo ok'", _THIRTY_SECONDS, [_bashTimeoutLine("sh -c 'echo ok'", _THIRTY_SECONDS)]],
    ["timeout 30 sh -c 'git status'", "sh -c 'git status'", _THIRTY_SECONDS, [_bashTimeoutLine("sh -c 'git status'", _THIRTY_SECONDS)]],
    [
        "ls && timeout 30 sh -c 'git status'",
        "ls && sh -c 'git status'",
        _THIRTY_SECONDS,
        [_bashTimeoutLine("ls && sh -c 'git status'", _THIRTY_SECONDS)],
    ],
    ['echo $(timeout 5 echo ok)', 'echo $(echo ok)', _FIVE_SECONDS, [_bashTimeoutLine('echo $(echo ok)', _FIVE_SECONDS)]],
    ['echo ok | timeout 5 cat', 'echo ok | cat', _FIVE_SECONDS, [_bashTimeoutLine('echo ok | cat', _FIVE_SECONDS)]],
    ['timeout 30 echo ok && ls', 'echo ok && ls', _THIRTY_SECONDS, [_bashTimeoutLine('echo ok && ls', _THIRTY_SECONDS)]],
    [
        'timeout 300 uv sync --locked --all-groups 2>&1 | tail -40',
        'uv sync --locked --all-groups 2>&1 | tail -40',
        _FIVE_MINUTES,
        [_bashTimeoutLine('uv sync --locked --all-groups 2>&1 | tail -40', _FIVE_MINUTES)],
    ],
    [
        'ls && timeout 300 uv sync --locked 2>&1 | tail -40; timeout 1m echo "a  b" >out.txt',
        'ls && uv sync --locked 2>&1 | tail -40; echo "a  b" >out.txt',
        _FIVE_MINUTES,
        [_bashTimeoutLine('ls && uv sync --locked 2>&1 | tail -40; echo "a  b" >out.txt', _FIVE_MINUTES)],
    ],
];

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
});

// The adapter's order over a Bash call, the timeout pass before and after the shell rewrite
const _ordered = (command: string): Plain =>
    _plain(fold<Call, unknown, string>([commandTimeout, shellRule(new Set()), commandTimeout])(_call(command)));

describe('fold order', () => {
    it('moves a timeout prefix the mise rewrite exposes to the Bash parameter', () => {
        expect(_ordered('mise x -- timeout 30 node -v')).toStrictEqual({
            kind: 'rewrite',
            command: 'node -v',
            timeout: _THIRTY_SECONDS,
            context: [_miseLine('timeout 30 node -v', 'mise x --'), 'Ran node -v under the Bash timeout parameter at 30000 ms'],
        });
    });

    it('rewrites the mise leaf a timeout prefix wrapped', () => {
        expect(_ordered('timeout 30 mise x -- node -v')).toStrictEqual({
            kind: 'rewrite',
            command: 'node -v',
            timeout: _THIRTY_SECONDS,
            context: ['Ran mise x -- node -v under the Bash timeout parameter at 30000 ms', _miseLine('node -v', 'mise x --')],
        });
    });
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

    it.each(_TIMEOUT_REWRITTEN)(
        'rewrites %j to %j under the parameter at %d ms and passes the rewrite unchanged',
        (command, rewritten, bound, context) => {
            expect(_timed(command)).toStrictEqual({ kind: 'rewrite', command: rewritten, timeout: bound, context });
            expect(_timed(rewritten, bound)).toStrictEqual({ kind: 'rewrite', command: rewritten, timeout: bound, context: [] });
        },
    );

    it('moves the prefix of a shell body to the parameter and leaves the body to the git rows', () => {
        expect(_plain(fold([commandTimeout, gitGuard(new Set())])(_call("timeout 30 sh -c 'git reset --hard'")))).toStrictEqual({
            kind: 'deny',
            reason: expect.toSatisfy(_holdsAll(['git-guard: git reset --hard wipes working-tree or index state'])),
        });
        expect(_timed("timeout 30 sh -c 'git status'", _TWO_MINUTES)).toMatchObject({ command: "sh -c 'git status'", timeout: _THIRTY_SECONDS });
    });

    it('keeps a larger parameter over the leaves of a compound command and raises a smaller one to the largest duration', () => {
        expect(_timed('timeout 30 echo ok && ls', _TWO_MINUTES)).toMatchObject({ kind: 'rewrite', command: 'echo ok && ls', timeout: _TWO_MINUTES });
        expect(_timed('timeout 30 echo ok && ls', _TEN_SECONDS)).toMatchObject({
            kind: 'rewrite',
            command: 'echo ok && ls',
            timeout: _THIRTY_SECONDS,
        });
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

// The call with its background flag, read through the ceiling rule into the fields it decides
interface Ceiling {
    readonly kind: 'rewrite' | 'deny' | 'answer';
    readonly timeout?: number;
    readonly background?: boolean;
    readonly context: readonly string[];
}

// The Bash tool's own field name for its background flag
const _BACKGROUND = 'run_in_background';

const _ceiling = (command: string, timeout?: number, background?: boolean): Ceiling =>
    commandCeiling({ tool: 'Bash' as const, command, timeout, [_BACKGROUND]: background }).match<Ceiling>({
        rewrite: (e, context) => ({ kind: 'rewrite', timeout: e.timeout, background: e[_BACKGROUND], context }),
        deny: () => ({ kind: 'deny', context: [] }),
        answer: () => ({ kind: 'answer', context: [] }),
    });

const _BACKGROUND_LINE =
    'Ran with run_in_background: true, rasm:workflow runs a workflow job past the Bash timeout maximum of 600000 ms, and its completion notification carries the result';

// One leaf per head of the table, each takes the ceiling with no line
const _SLOW_LEAVES: readonly string[] = [
    'pnpm exec nx run function-hooks:typecheck',
    'nx run-many -t typecheck',
    'NX_TUI=false pnpm exec nx affected -t check --files=README.md',
    'dotnet build Workspace.slnx -bl',
    'dotnet test Workspace.slnx --no-build',
    'ast-grep test --include-off',
    "claude -p 'reply ok' --output-format stream-json",
    'act -j lint',
    'uv sync --locked',
    'pnpm install --frozen-lockfile',
    'ls && dotnet build Workspace.slnx 2>&1 | tail -20',
];

const _QUICK_LEAVES: readonly string[] = [
    'ls tools',
    'pnpm exec nx show projects',
    'dotnet --version',
    'ast-grep scan tools/nx',
    'uv tree --depth 1',
    'pnpm list',
    'claude plugin validate x',
    'echo "dotnet build in prose"',
];

describe('commandCeiling', () => {
    it.each(_SLOW_LEAVES)('raises %j to the ceiling with no line when the call sets no bound', (command) => {
        expect(_ceiling(command)).toStrictEqual({ kind: 'rewrite', timeout: _TEN_MINUTES, background: undefined, context: [] });
    });

    it('raises a smaller bound and keeps the ceiling', () => {
        expect(_ceiling('dotnet build Workspace.slnx', _TWO_MINUTES)).toMatchObject({ timeout: _TEN_MINUTES, context: [] });
        expect(_ceiling('dotnet build Workspace.slnx', _TEN_MINUTES)).toMatchObject({ timeout: _TEN_MINUTES, context: [] });
    });

    it.each(_QUICK_LEAVES)('leaves %j and its bound alone', (command) => {
        expect(_ceiling(command, _FIVE_SECONDS)).toStrictEqual({ kind: 'rewrite', timeout: _FIVE_SECONDS, background: undefined, context: [] });
        expect(_ceiling(command)).toStrictEqual({ kind: 'rewrite', timeout: undefined, background: undefined, context: [] });
    });

    it('passes a call that already runs in the background', () => {
        expect(_ceiling('dotnet build Workspace.slnx', undefined, true)).toStrictEqual({
            kind: 'rewrite',
            timeout: undefined,
            background: true,
            context: [],
        });
        expect(_ceiling('pnpm exec nx run rasm:workflow -- --job=lint', undefined, true)).toStrictEqual({
            kind: 'rewrite',
            timeout: undefined,
            background: true,
            context: [],
        });
    });

    it('moves the workflow job to the background with its line', () => {
        expect(_ceiling('pnpm exec nx run rasm:workflow -- --job=lint')).toStrictEqual({
            kind: 'rewrite',
            timeout: undefined,
            background: true,
            context: [_BACKGROUND_LINE],
        });
        expect(_ceiling('nx run rasm:workflow -- --job=dotnet', _TEN_MINUTES)).toMatchObject({ background: true, context: [_BACKGROUND_LINE] });
    });
});
