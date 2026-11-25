import { assert, it } from '@effect/vitest';
import { Policy } from '@rasm/policy';
import { Effect, Formatter } from 'effect';
import { environment, services, shell } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const restore = 'Read the committed text with `git diff -- <path>` or `git show HEAD:./<path>` and put back only your own lines with your file edit tool.';
const policy = new Policy();
const rows = [
    { command: 'git stash', denied: true, restoring: false },
    { command: 'git stash list', denied: false, restoring: false },
    { command: 'git revert HEAD', denied: true, restoring: true },
    { command: 'git restore a.txt', denied: true, restoring: true },
    { command: 'git restore --staged a.txt', denied: false, restoring: false },
    { command: 'git restore -SW a.txt', denied: true, restoring: true },
    { command: 'git() { echo "$@"; }; git stash', denied: false, restoring: false },
    { command: 'git checkout -- a.txt', denied: true, restoring: true },
    { command: 'git checkout main', denied: false, restoring: false },
    { command: 'git checkout -f main', denied: true, restoring: true },
    { command: 'git switch --discard-changes main', denied: true, restoring: true },
    { command: 'git switch -c topic', denied: false, restoring: false },
    { command: 'git reset --hard HEAD~1', denied: true, restoring: true },
    { command: 'git reset --soft HEAD~1', denied: false, restoring: false },
    { command: 'git clean -fd', denied: true, restoring: false },
    { command: 'git clean -n -d', denied: false, restoring: false },
    { command: 'git checkout-index -a -f', denied: true, restoring: true },
    { command: 'git checkout-index -- a.txt', denied: false, restoring: false },
    { command: 'git update-index --assume-unchanged a.txt', denied: true, restoring: false },
    { command: 'git update-index --refresh', denied: false, restoring: false },
    { command: 'git status && git -C sub reset --hard', denied: true, restoring: true },
    { command: 'echo "$(git stash)"', denied: true, restoring: false },
    { command: 'git log --oneline', denied: false, restoring: false },
] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const restores = (decision: Effect.Success<ReturnType<Policy['decide']>>) => decision._tag === 'Deny' && decision.reasons.every((reason) => reason.endsWith(restore));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(services, { excludeTestServices: true })('git', (suite) => {
    suite.effect.each(rows)('$command', ({ command, denied, restoring }) =>
        Effect.gen(function* () {
            const decision = yield* policy.decide(shell(command), environment);
            assert.strictEqual(decision._tag, denied ? 'Deny' : 'Allow', Formatter.format(decision));
            assert.strictEqual(restores(decision), restoring, Formatter.format(decision));
        }),
    );
});
