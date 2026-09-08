// --- [IMPORTS] -------------------------------------------------------------------------

import { describe, expect, it } from 'vitest';
import { groups, INTERPRETER, type Leaf, leaves, strip } from './argv.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _GIT = ['git'];
const _SENTINEL = [['git', INTERPRETER]];
const _MAX_DEPTH = 8;
// The word ${IFS} through a template, with no template placeholder in a string literal
const _DOLLAR = '$';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _texts = (parsed: readonly Leaf[]): readonly (readonly string[])[] => parsed.map((leaf) => leaf.map((word) => word.text));

const _spans = (parsed: readonly Leaf[]): readonly (readonly (readonly [string, number, number])[])[] =>
    parsed.map((leaf) => leaf.map((word): readonly [string, number, number] => [word.text, word.start, word.end]));

// The span of the first occurrence of a word in a command, the expectation a span test compares against
const _at = (command: string, word: string): readonly [string, number, number] => [word, command.indexOf(word), command.indexOf(word) + word.length];

const _git = leaves(_GIT);

// Wraps a command in a double-quoted sh -c body the given number of times
const _nested = (depth: number): string =>
    Array.from({ length: depth }).reduce<string>((inner) => `sh -c "${inner.replace(/["\\]/gu, '\\$&')}"`, 'git stash');

// --- [CASES] ---------------------------------------------------------------------------

// Frozen expectations, the substitution rows expect no residue word from the blanked body
const _CASES: readonly (readonly [string, readonly (readonly string[])[]])[] = [
    ['git stash', [['git', 'stash']]],
    ['echo $(git stash)', [['git', 'stash'], ['echo']]],
    ['echo `git stash`', [['git', 'stash'], ['echo']]],
    [
        'echo run `git stash` later',
        [
            ['git', 'stash'],
            ['echo', 'run', 'later'],
        ],
    ],
    ["echo 'run `git stash` later'", [['echo', 'run `git stash` later']]],
    [
        'echo "run `git stash` later"',
        [
            ['git', 'stash'],
            ['echo', `run ${' '.repeat('`git stash`'.length)} later`],
        ],
    ],
    [
        'echo "$(git stash list)"',
        [
            ['git', 'stash', 'list'],
            ['echo', ' '.repeat('$(git stash list)'.length)],
        ],
    ],
    ["echo 'no $(git stash) here'", [['echo', 'no $(git stash) here']]],
    ["sh -c 'git checkout -- .'", [['git', 'checkout', '--', '.']]],
    ["bash -c 'git stash'", [['git', 'stash']]],
    ['zsh -c "git reset --hard"', [['git', 'reset', '--hard']]],
    ['eval git stash', _SENTINEL],
    ['sh script.sh', _SENTINEL],
    ['sh -s', _SENTINEL],
    ['sh -c', []],
    ['FOO=bar git stash', [['FOO=bar', 'git', 'stash']]],
    ['env FOO=bar git stash', [['env', 'FOO=bar', 'git', 'stash']]],
    ['sudo git stash', [['sudo', 'git', 'stash']]],
    ['nice -n 10 git stash', [['nice', '-n', '10', 'git', 'stash']]],
    ['uv run git stash', [['uv', 'run', 'git', 'stash']]],
    ['pnpm exec git stash', [['pnpm', 'exec', 'git', 'stash']]],
    ['timeout 5 git stash', [['timeout', '5', 'git', 'stash']]],
    [
        "timeout 5 sh -c 'git stash'",
        [
            ['git', 'stash'],
            ['timeout', '5', 'sh', '-c', 'git stash'],
        ],
    ],
    ['timeout 5', [['timeout', '5']]],
    ['xargs git stash', [['xargs', 'git', 'stash']]],
    ['/usr/bin/git stash', [['/usr/bin/git', 'stash']]],
    [
        'git stash && echo done',
        [
            ['git', 'stash'],
            ['echo', 'done'],
        ],
    ],
    [
        'echo a; git rebase main',
        [
            ['echo', 'a'],
            ['git', 'rebase', 'main'],
        ],
    ],
    [
        'echo a || git clean -fd',
        [
            ['echo', 'a'],
            ['git', 'clean', '-fd'],
        ],
    ],
    [
        'git log | git stash',
        [
            ['git', 'log'],
            ['git', 'stash'],
        ],
    ],
    ['python -c \'import subprocess; subprocess.run(["git", "stash"])\'', [['git', 'stash']]],
    ['python -c \'print("git stash")\'', []],
    ['node -e \'require("child_process").execSync("git rebase main")\'', [['git', 'rebase', 'main']]],
    ['ruby -e \'system("git stash")\'', [['git', 'stash']]],
    ['perl -e \'print "git"\'', []],
    ["cat <<'EOF2'\ngit stash\nEOF2", [['cat']]],
    ['cat <<EOF3\n$(git stash)\nEOF3', [['git', 'stash'], ['cat']]],
    ['git stash \\\n  list', [['git', 'stash', 'list']]],
    ["echo $'git stash'", [['echo', '$git stash']]],
    [`git stash${_DOLLAR}{IFS}list`, [['git', 'stash', 'list']]],
    ["git commit -m 'git stash'", [['git', 'commit', '-m', 'git stash']]],
    ['echo "git stash"', [['echo', 'git stash']]],
    ['git -c core.pager=cat log', [['git', '-c', 'core.pager=cat', 'log']]],
    ["cat > tmp-proof.md <<'EOF'\nop://x\nEOF", [['cat', '>', 'tmp-proof.md']]],
    ['a#b git stash', [['a']]],
    ['git stash # note\ngit log', [['git', 'stash', 'git', 'log']]],
];

// Every env assignment, wrapper, and runner leaves with its options
const _STRIPPED: readonly string[] = [
    'FOO=bar git stash',
    'env FOO=bar git stash',
    'sudo git stash',
    'nice -n 10 git stash',
    'uv run git stash',
    'pnpm exec git stash',
    'timeout 5 git stash',
    'xargs git stash',
    'npm exec -- git stash',
];

// --- [TESTS] ---------------------------------------------------------------------------

describe('leaves', () => {
    it.each(_CASES)('parses %j', (command, expected) => {
        expect(_texts(_git(command))).toStrictEqual(expected);
    });

    it.each(_STRIPPED)('strips the wrappers of %j down to git', (command) => {
        expect(_texts(_git(command).map(strip))).toStrictEqual([['git', 'stash']]);
    });

    it('slices an interpreter body at every guarded word', () => {
        expect(_texts(leaves(['git', 'mise'])('python -c \'import subprocess; subprocess.run(["mise", "x", "--", "git", "stash"])\''))).toStrictEqual(
            [
                ['mise', 'x', '--', 'git', 'stash'],
                ['git', 'stash'],
            ],
        );
    });

    it('resolves eight nested shells and returns the sentinel at the ninth', () => {
        expect(_texts(_git(_nested(_MAX_DEPTH)))).toStrictEqual([['git', 'stash']]);
        expect(_texts(_git(_nested(_MAX_DEPTH + 1)))).toStrictEqual(_SENTINEL);
    });

    it('keeps the span of each word relative to the original text', () => {
        const plain = 'grep -rn x eng/';
        expect(_spans(_git(plain))).toStrictEqual([[_at(plain, 'grep'), _at(plain, '-rn'), _at(plain, 'x'), _at(plain, 'eng/')]]);
        const joined = 'echo a && grep x';
        expect(_spans(_git(joined))).toStrictEqual([
            [_at(joined, 'echo'), _at(joined, 'a')],
            [_at(joined, 'grep'), _at(joined, 'x')],
        ]);
    });

    it('keeps exact spans through a substitution body and a quoted shell body', () => {
        const substituted = 'echo $(git stash)';
        expect(_spans(_git(substituted))).toStrictEqual([[_at(substituted, 'git'), _at(substituted, 'stash')], [_at(substituted, 'echo')]]);
        const quoted = "bash -c 'git stash'";
        expect(_spans(_git(quoted))).toStrictEqual([[_at(quoted, 'git'), _at(quoted, 'stash')]]);
    });

    it('puts the shell word span on both words of the sentinel', () => {
        const script = 'sh script.sh';
        const [word, start, end] = _at(script, 'sh');
        expect(_spans(_git(script))).toStrictEqual([
            [
                ['git', start, end],
                [INTERPRETER, start, end],
            ],
        ]);
        expect(word).toBe('sh');
    });
});

describe('groups', () => {
    it('spans each parenthesized group and marks the one whose closing paren feeds a pipe', () => {
        const piped = "( sleep 10; printf 'a\\n'; sleep 20 ) | script -q /dev/null cmd";
        expect(groups(piped)).toStrictEqual([{ start: 1, end: piped.indexOf(')'), piped: true }]);
        const joined = '(cd x && ls) && (echo a || echo b) || true';
        expect(groups(joined)).toStrictEqual([
            { start: 1, end: joined.indexOf(')'), piped: false },
            { start: joined.lastIndexOf('(') + 1, end: joined.lastIndexOf(')'), piped: false },
        ]);
    });

    it('reads no group from a substitution, a quoted paren, or a command without parens', () => {
        expect(groups('echo $(ls) | cat')).toStrictEqual([]);
        expect(groups("echo '(a)' | cat")).toStrictEqual([]);
        expect(groups('ls | cat')).toStrictEqual([]);
    });

    it('runs an unclosed group to the end without a pipe', () => {
        expect(groups('( sleep 1; echo')).toStrictEqual([{ start: 1, end: '( sleep 1; echo'.length, piped: false }]);
    });
});
