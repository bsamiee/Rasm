import { describe, expect, it } from 'vitest';
import { ADVICE, gitGuard, gitPaths } from './git.ts';

const _EXISTING: ReadonlySet<string> = new Set(['README.md']);
const _KIB = 1024;
const _MAX_COMMAND_KIB = 128;

const _guard = gitGuard(_EXISTING);

const _reason = (command: string): string | undefined => {
    const decision = _guard({ command });
    return decision.kind === 'deny' ? decision.reason : undefined;
};

// One row per table entry and refinement arm, the reason as the guard spells it
const _DENIED: readonly (readonly [string, string])[] = [
    ['git stash', 'git stash hides uncommitted work other agents depend on'],
    ['git branch -D feature', 'git branch -D deletes or force-moves a branch'],
    ['git checkout --force main', 'git checkout --force discards local changes'],
    ['git checkout .', 'git checkout with a pathspec overwrites working-tree files'],
    ['git checkout README.md', 'git checkout README.md names an existing path and would overwrite it'],
    ['git clean -fd', 'git clean deletes untracked files'],
    ['git config alias.co checkout', 'git config alias.co defines a git alias that can hide a refused subcommand'],
    ['git -c alias.x=stash x', 'inline git alias can hide a refused subcommand'],
    ['git push --force', 'git push --force rewrites or deletes remote history'],
    ['git push origin +main', 'git push +main rewrites or deletes remote history'],
    ['git rebase main', 'git rebase rewrites commits other agents can already hold'],
    ['git reflog expire --all', 'git reflog expire erases reflog entries, the last recovery path'],
    ['git reset --hard', 'git reset --hard wipes working-tree or index state'],
    ['git reset HEAD~1', 'git reset HEAD~1 moves HEAD and drops commits from the branch'],
    ['git restore file.txt', 'git restore overwrites working-tree files'],
    ['git revert HEAD', 'git revert reverses committed history'],
    ['git switch -C main', 'git switch -C discards local changes'],
    ["sh -c 'git stash'", 'git stash hides uncommitted work other agents depend on'],
    ['sh script.sh', 'run git directly and a script by its path, not through a shell or an interpreter'],
    ['echo ok && git stash', 'git stash hides uncommitted work other agents depend on'],
    ['uv run -- git stash', 'git stash hides uncommitted work other agents depend on'],
];

const _ALLOWED: readonly string[] = [
    'git status',
    'git stash list',
    'git stash show',
    'git branch feature',
    'git checkout -b feature',
    'git checkout main',
    'git checkout -',
    'git checkout no-such-file-xyz',
    'git push',
    'git reset README.md',
    'git reset -- README.md',
    'git restore --staged file.txt',
    'git -c core.pager=cat log',
    'git -C dir status',
    'echo "git stash"',
    'python -c \'print("git stash")\'',
];

describe('gitGuard', () => {
    it.each(_DENIED)('refuses %j', (command, reason) => {
        expect(_reason(command)).toBe(`${reason}, ${ADVICE}`);
    });

    it.each(_ALLOWED)('passes %j', (command) => {
        expect(_guard({ command })).toStrictEqual({ kind: 'rewrite', e: { command }, context: [] });
    });

    it('refuses a command past the lexing bound and strips control characters from a reason', () => {
        expect(_reason('x'.repeat(_MAX_COMMAND_KIB * _KIB + 1))).toContain('exceeds the 128 KiB check limit');
        expect(_reason('git reset ab')).toBe(`git reset a b moves HEAD and drops commits from the branch, ${ADVICE}`);
    });

    it('names the paths the reset and checkout refinements test', () => {
        expect(gitPaths('git reset a b && git checkout -f c && git status')).toStrictEqual(['a', 'b', 'c']);
    });
});
