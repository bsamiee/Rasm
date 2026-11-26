import { assert, it } from '@effect/vitest';
import { Policy } from '@rasm/policy/policy';
import { Effect, Equal, Formatter } from 'effect';
import { environment, folder, printed, services, shell } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const prepare = 'git init -q -b main . && echo one > tracked && git add tracked && git commit -qm one && echo two >> tracked && git commit -qam two && git branch side && git tag v1 && echo mine >> tracked && echo new > untracked';
const work = "cat tracked untracked; git for-each-ref --format='%(refname) %(objectname)'; git stash list";
const local = [
    'git checkout .',
    'git checkout tracked',
    'git checkout -- tracked',
    'git checkout HEAD tracked',
    'git checkout side',
    'git checkout -h .',
    'git checkout -f -h',
    'git restore tracked',
    'git restore -h tracked',
    'git reset --hard',
    'git reset --hard -h',
    'git clean -f',
    'git clean -f -h',
    'git clean -n',
    'git stash',
    'git stash -h',
    'git revert --quit',
    'git rm -f tracked',
    'git rm -f -n tracked',
    'git rm -f --dry-run tracked',
    'git branch -D side',
    'git branch -D -h side',
    'git branch -f side HEAD~1',
    'git tag -d v1',
    'git tag -d -h v1',
    'git tag -f v1 HEAD~1',
    'git update-ref -d refs/heads/side',
    'git push -f -n origin main',
] as const;
const remote = [
    { command: 'git push origin main', denied: false },
    { command: 'git push --repo=origin +main', denied: true },
    { command: 'git push origin :"$B"', denied: true },
    { command: 'git push --delete origin refs/tags/v1', denied: true },
    { command: 'git push origin +refs/tags/v1', denied: true },
    { command: 'git update-ref --stdin', denied: true },
] as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(services, { excludeTestServices: true })('git', (suite) => {
    suite.effect.each(local)('%s is refused exactly when git itself loses work in a checkout', (command) =>
        Effect.gen(function* () {
            const cwd = yield* folder({});
            const [, before, , after, decision] = [yield* printed(prepare, cwd), yield* printed(work, cwd), yield* printed(command, cwd), yield* printed(work, cwd), yield* new Policy().decide(shell(command), environment(cwd))];
            assert.strictEqual(decision._tag, Equal.equals(before, after) ? 'Allow' : 'Deny', Formatter.format({ decision, before, after }));
        }).pipe(Effect.scoped),
    );
    suite.effect.each(remote)('$command', ({ command, denied }) => Effect.map(new Policy().decide(shell(command), environment()), (decision) => assert.strictEqual(decision._tag, denied ? 'Deny' : 'Allow', Formatter.format(decision))));
});
