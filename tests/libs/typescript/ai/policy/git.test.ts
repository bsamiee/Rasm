import { assert, it } from '@effect/vitest';
import { Policy } from '@rasm/policy';
import { Effect, Formatter } from 'effect';
import { call, dialect, judging } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const restore = 'Read the committed text with `git diff -- <path>` or `git show HEAD:./<path>` and put back only your own lines with your file edit tool.';
const policy = new Policy();
const rows = [
    { command: 'git stash', refused: true, restoring: false },
    { command: 'git stash push -m wip', refused: true, restoring: false },
    { command: 'git stash list', refused: false, restoring: false },
    { command: 'git stash show -p', refused: false, restoring: false },
    { command: 'git revert HEAD', refused: true, restoring: true },
    { command: 'git restore a.txt', refused: true, restoring: true },
    { command: 'git restore --staged a.txt', refused: false, restoring: false },
    { command: 'git restore -SW a.txt', refused: true, restoring: true },
    { command: 'git() { echo "$@"; }; git stash', refused: false, restoring: false },
    { command: 'git checkout -- a.txt', refused: true, restoring: true },
    { command: 'git checkout main', refused: false, restoring: false },
    { command: 'git checkout -f main', refused: true, restoring: true },
    { command: 'git switch --discard-changes main', refused: true, restoring: true },
    { command: 'git switch -c topic', refused: false, restoring: false },
    { command: 'git reset --hard HEAD~1', refused: true, restoring: true },
    { command: 'git reset --merge', refused: true, restoring: true },
    { command: 'git reset --keep HEAD', refused: true, restoring: true },
    { command: 'git reset --soft HEAD~1', refused: false, restoring: false },
    { command: 'git clean -fd', refused: true, restoring: false },
    { command: 'git clean -n -d', refused: false, restoring: false },
    { command: 'git clean --dry-run', refused: false, restoring: false },
    { command: 'git checkout-index -a -f', refused: true, restoring: true },
    { command: 'git checkout-index -- a.txt', refused: false, restoring: false },
    { command: 'git update-index --assume-unchanged a.txt', refused: true, restoring: false },
    { command: 'git update-index --skip-worktree a.txt', refused: true, restoring: false },
    { command: 'git update-index --refresh', refused: false, restoring: false },
    { command: 'git status && git -C sub reset --hard', refused: true, restoring: true },
    { command: 'echo "$(git stash)"', refused: true, restoring: false },
    { command: 'git log --oneline', refused: false, restoring: false },
] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const restores = (judgment: Effect.Success<ReturnType<Policy['judge']>>) => judgment._tag === 'Refuse' && judgment.reasons.every((reason) => reason.endsWith(restore));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(judging, { excludeTestServices: true })('git', (suite) => {
    suite.effect.each(rows.map((row, index) => ({ ...row, index })))('$command', ({ command, index, refused, restoring }) =>
        Effect.gen(function* () {
            const judgment = yield* policy.judge(call(command, index), dialect, import.meta.dirname);
            assert.strictEqual(judgment._tag, refused ? 'Refuse' : 'Pass', Formatter.format(judgment));
            assert.strictEqual(restores(judgment), restoring, Formatter.format(judgment));
        }),
    );
});
