import { describe, expect, it } from 'vitest';
import { type Argv, hasGroup, INTERPRETER, parse, strip } from './argv.ts';

const _SENTINEL = [['git', INTERPRETER]];
const _MAX_DEPTH = 8;
const _DOLLAR = '$';

const _texts = (argvs: readonly Argv[]): readonly (readonly string[])[] => argvs.map((argv) => argv.map((word) => word.text));

const _git = parse(['git']);

// Each word beside the source text its span covers
const _sliced = (argv: Argv, command: string): readonly (readonly [string, string])[] =>
    argv.map((word) => [word.text, command.slice(word.start, word.end)]);

// A double-quoted sh -c body nested the given number of times
const _nested = (depth: number): string =>
    Array.from({ length: depth }).reduce<string>((inner) => `sh -c "${inner.replace(/["\\]/gu, '\\$&')}"`, 'git stash');

// One case per lexer rule: separators, substitutions, quoting, shells, interpreters, wrappers, heredocs, continuations, and comments
const _CASES: readonly (readonly [string, readonly (readonly string[])[]])[] = [
    ['git stash && echo done; ls | wc', [['git', 'stash'], ['echo', 'done'], ['ls'], ['wc']]],
    ['echo $(git stash)', [['git', 'stash'], ['echo']]],
    [
        'echo "run `git stash` later"',
        [
            ['git', 'stash'],
            ['echo', `run ${' '.repeat('`git stash`'.length)} later`],
        ],
    ],
    ["echo 'no $(git stash) `here`'", [['echo', 'no $(git stash) `here`']]],
    ["sh -c 'git checkout -- .'", [['git', 'checkout', '--', '.']]],
    [
        "timeout 5 sh -c 'git stash'",
        [
            ['git', 'stash'],
            ['timeout', '5', 'sh', '-c', 'git stash'],
        ],
    ],
    ['eval git stash', _SENTINEL],
    ['sh script.sh', _SENTINEL],
    ['sh -c', []],
    ['FOO=bar sudo git stash', [['FOO=bar', 'sudo', 'git', 'stash']]],
    ['python -c \'import subprocess; subprocess.run(["git", "stash"])\'', [['git', 'stash']]],
    ['python -c \'print("git stash")\'', []],
    ['node -e \'require("child_process").execSync("git rebase main")\'', [['git', 'rebase', 'main']]],
    ["cat <<'EOF'\ngit stash\nEOF", [['cat']]],
    ['cat <<EOF\n$(git stash)\nEOF', [['git', 'stash'], ['cat']]],
    ['git stash \\\n  list', [['git', 'stash', 'list']]],
    [`git stash${_DOLLAR}{IFS}list`, [['git', 'stash', 'list']]],
    ["git commit -m 'git stash'", [['git', 'commit', '-m', 'git stash']]],
    ['git stash # note\ngit log', [['git', 'stash', 'git', 'log']]],
];

describe('parse', () => {
    it.each(_CASES)('reads %j', (command, expected) => {
        expect(_texts(_git(command))).toStrictEqual(expected);
    });

    it('strips env assignments, wrappers, and runners with their options', () => {
        for (const command of ['FOO=bar git stash', 'env FOO=bar git stash', 'nice -n 10 git stash', 'uv run git stash', 'npm exec -- git stash']) {
            expect(_texts(_git(command).map(strip))).toStrictEqual([['git', 'stash']]);
        }
    });

    it('slices an interpreter body at every guarded word', () => {
        expect(_texts(parse(['git', 'mise'])('python -c \'subprocess.run(["mise", "x", "--", "git", "stash"])\''))).toStrictEqual([
            ['mise', 'x', '--', 'git', 'stash'],
            ['git', 'stash'],
        ]);
    });

    it('resolves eight nested shells and answers the sentinel at the ninth', () => {
        expect(_texts(_git(_nested(_MAX_DEPTH)))).toStrictEqual([['git', 'stash']]);
        expect(_texts(_git(_nested(_MAX_DEPTH + 1)))).toStrictEqual(_SENTINEL);
    });

    it('keeps each word span relative to the source through a substitution and a quoted body', () => {
        const command = "echo $(git stash) && bash -c 'git log'";
        const spans = _git(command).map((argv) => _sliced(argv, command));
        expect(spans).toStrictEqual([
            [
                ['git', 'git'],
                ['stash', 'stash'],
            ],
            [['echo', 'echo']],
            [
                ['git', 'git'],
                ['log', 'log'],
            ],
        ]);
    });

    it('reads a group from a paren outside quotes and substitutions alone', () => {
        expect(hasGroup('( sleep 1; echo a ) | cat')).toBe(true);
        expect(hasGroup("echo $(ls) '(a)' | cat")).toBe(false);
    });
});
