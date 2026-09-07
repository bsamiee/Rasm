// --- [IMPORTS] -------------------------------------------------------------------------

import { describe, expect, it } from 'vitest';
import type { Decision } from '../composition/decision.ts';
import { ADVICE, gitGuard, gitPaths } from './git.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Verdict = 'deny' | 'rewrite';

interface Call {
    readonly tool: 'Bash';
    readonly command: string;
}

type Plain =
    | { readonly kind: 'rewrite'; readonly e: Call; readonly context: readonly string[] }
    | { readonly kind: 'deny'; readonly reason: string }
    | { readonly kind: 'answer'; readonly result: unknown };

// --- [CONSTANTS] -----------------------------------------------------------------------

const _KIB = 1024;
const _MAX_COMMAND_KIB = 128;
// The word ${IFS} through a template, with no template placeholder in a string literal
const _DOLLAR = '$';
// The verdicts hold with README.md present at the repository root and no-such-file-xyz and no-such-thing-xyz absent
const _EXISTING: ReadonlySet<string> = new Set(['README.md']);

// The frozen policy verdicts, ported from the deleted git-guard.py, its exit 2 as deny and exit 0 as rewrite
const _VERDICTS: readonly (readonly [string, Verdict, string])[] = [
    [
        'git stash',
        'deny',
        'git-guard: git stash hides uncommitted work other agents depend on. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    ['git stash list', 'rewrite', ''],
    ['git stash show', 'rewrite', ''],
    ['git log --oneline -3 | head -2', 'rewrite', ''],
    [
        "sh -c 'git checkout -- .'",
        'deny',
        'git-guard: git checkout with a pathspec overwrites working-tree files. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git rebase main',
        'deny',
        'git-guard: git rebase rewrites commits other agents can already hold. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    ['git status', 'rewrite', ''],
    [
        'git branch -d feature',
        'deny',
        'git-guard: git branch -d deletes or force-moves a branch. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git branch -D feature',
        'deny',
        'git-guard: git branch -D deletes or force-moves a branch. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git branch -M main',
        'deny',
        'git-guard: git branch -M deletes or force-moves a branch. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git branch --delete feature',
        'deny',
        'git-guard: git branch --delete deletes or force-moves a branch. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git branch --force main',
        'deny',
        'git-guard: git branch --force deletes or force-moves a branch. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    ['git branch feature', 'rewrite', ''],
    [
        'git checkout -f main',
        'deny',
        'git-guard: git checkout -f discards local changes. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git checkout -B main',
        'deny',
        'git-guard: git checkout -B discards local changes. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git checkout -p',
        'deny',
        'git-guard: git checkout -p discards local changes. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git checkout --patch',
        'deny',
        'git-guard: git checkout --patch discards local changes. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git checkout --ours file.txt',
        'deny',
        'git-guard: git checkout --ours discards local changes. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git checkout --theirs file.txt',
        'deny',
        'git-guard: git checkout --theirs discards local changes. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git checkout --force main',
        'deny',
        'git-guard: git checkout --force discards local changes. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    ['git checkout -b feature', 'rewrite', ''],
    ['git checkout --orphan blank', 'rewrite', ''],
    ['git checkout -t origin/main', 'rewrite', ''],
    ['git checkout --track origin/main', 'rewrite', ''],
    ['git checkout --detach', 'rewrite', ''],
    ['git checkout main', 'rewrite', ''],
    ['git checkout -', 'rewrite', ''],
    [
        'git checkout .',
        'deny',
        'git-guard: git checkout with a pathspec overwrites working-tree files. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git checkout :/',
        'deny',
        'git-guard: git checkout with a pathspec overwrites working-tree files. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git checkout -- README.md',
        'deny',
        'git-guard: git checkout with a pathspec overwrites working-tree files. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git checkout README.md',
        'deny',
        'git-guard: git checkout README.md names an existing path and would overwrite it. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git checkout main README.md',
        'deny',
        'git-guard: git checkout with a pathspec overwrites working-tree files. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    ['git checkout no-such-file-xyz', 'rewrite', ''],
    [
        'git clean',
        'deny',
        'git-guard: git clean deletes untracked files. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git clean -fd',
        'deny',
        'git-guard: git clean deletes untracked files. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git config alias.co checkout',
        'deny',
        'git-guard: git config alias.co defines a git alias that can hide a blocked subcommand. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    ['git config user.name x', 'rewrite', ''],
    [
        'git -c alias.x=stash x',
        'deny',
        'git-guard: an inline git alias (-c alias.*) can hide a blocked subcommand. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    ['git push', 'rewrite', ''],
    [
        'git push -f',
        'deny',
        'git-guard: git push -f rewrites or deletes remote history. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git push -d origin feature',
        'deny',
        'git-guard: git push -d rewrites or deletes remote history. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git push --delete origin feature',
        'deny',
        'git-guard: git push --delete rewrites or deletes remote history. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git push --mirror',
        'deny',
        'git-guard: git push --mirror rewrites or deletes remote history. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git push --prune',
        'deny',
        'git-guard: git push --prune rewrites or deletes remote history. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git push --force',
        'deny',
        'git-guard: git push --force rewrites or deletes remote history. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git push origin +main',
        'deny',
        'git-guard: git push +main rewrites or deletes remote history. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git push origin :feature',
        'deny',
        'git-guard: git push :feature rewrites or deletes remote history. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    ['git push origin main', 'rewrite', ''],
    [
        'git rebase',
        'deny',
        'git-guard: git rebase rewrites commits other agents can already hold. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git rebase -i HEAD~3',
        'deny',
        'git-guard: git rebase rewrites commits other agents can already hold. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git reflog delete HEAD@{1}',
        'deny',
        'git-guard: git reflog delete erases reflog entries, the last recovery path. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git reflog drop HEAD@{1}',
        'deny',
        'git-guard: git reflog drop erases reflog entries, the last recovery path. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git reflog expire --all',
        'deny',
        'git-guard: git reflog expire erases reflog entries, the last recovery path. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    ['git reflog', 'rewrite', ''],
    ['git reset', 'rewrite', ''],
    [
        'git reset --hard',
        'deny',
        'git-guard: git reset --hard wipes working-tree or index state. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git reset --merge',
        'deny',
        'git-guard: git reset --merge wipes working-tree or index state. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git reset --keep',
        'deny',
        'git-guard: git reset --keep wipes working-tree or index state. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git reset HEAD~1',
        'deny',
        'git-guard: git reset HEAD~1 moves HEAD and drops commits from the branch. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    ['git reset README.md', 'rewrite', ''],
    ['git reset -- README.md', 'rewrite', ''],
    [
        'git reset no-such-thing-xyz',
        'deny',
        'git-guard: git reset no-such-thing-xyz moves HEAD and drops commits from the branch. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git restore README.md',
        'deny',
        'git-guard: git restore overwrites working-tree files. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    ['git restore --staged README.md', 'rewrite', ''],
    ['git restore -S README.md', 'rewrite', ''],
    [
        'git restore -SW README.md',
        'deny',
        'git-guard: git restore overwrites working-tree files. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git restore --worktree README.md',
        'deny',
        'git-guard: git restore overwrites working-tree files. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git revert HEAD',
        'deny',
        'git-guard: git revert reverses committed history. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git revert',
        'deny',
        'git-guard: git revert reverses committed history. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git switch -f main',
        'deny',
        'git-guard: git switch -f discards local changes. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git switch -C main',
        'deny',
        'git-guard: git switch -C discards local changes. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git switch --discard-changes main',
        'deny',
        'git-guard: git switch --discard-changes discards local changes. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git switch --force main',
        'deny',
        'git-guard: git switch --force discards local changes. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    ['git switch main', 'rewrite', ''],
    ['git switch -c feature', 'rewrite', ''],
    ['git commit -n -m x', 'rewrite', ''],
    ['git commit -m x', 'rewrite', ''],
    [
        'git -C /tmp stash',
        'deny',
        'git-guard: git stash hides uncommitted work other agents depend on. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git --git-dir=.git stash',
        'deny',
        'git-guard: git stash hides uncommitted work other agents depend on. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    ['git -c core.pager=cat log', 'rewrite', ''],
    [
        "bash -c 'git stash'",
        'deny',
        'git-guard: git stash hides uncommitted work other agents depend on. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'zsh -c "git reset --hard"',
        'deny',
        'git-guard: git reset --hard wipes working-tree or index state. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'eval git stash',
        'deny',
        'git-guard: the command runs git or a script through a shell or an interpreter, run git directly and a script by its path. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    ['sh -c', 'rewrite', ''],
    [
        'sh script.sh',
        'deny',
        'git-guard: the command runs git or a script through a shell or an interpreter, run git directly and a script by its path. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'sh -s',
        'deny',
        'git-guard: the command runs git or a script through a shell or an interpreter, run git directly and a script by its path. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'python -c \'import subprocess; subprocess.run(["git", "stash"])\'',
        'deny',
        'git-guard: git stash hides uncommitted work other agents depend on. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    ['python -c \'print("git stash")\'', 'rewrite', ''],
    [
        'node -e \'require("child_process").execSync("git rebase main")\'',
        'deny',
        'git-guard: git rebase rewrites commits other agents can already hold. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'ruby -e \'system("git stash")\'',
        'deny',
        'git-guard: git stash hides uncommitted work other agents depend on. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    ['perl -e \'print "git"\'', 'rewrite', ''],
    [
        'echo $(git stash)',
        'deny',
        'git-guard: git stash hides uncommitted work other agents depend on. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'echo `git stash`',
        'deny',
        'git-guard: git stash hides uncommitted work other agents depend on. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    ['echo "$(git stash list)"', 'rewrite', ''],
    ["echo 'no $(git stash) here'", 'rewrite', ''],
    [
        'git stash && echo done',
        'deny',
        'git-guard: git stash hides uncommitted work other agents depend on. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'echo a; git rebase main',
        'deny',
        'git-guard: git rebase rewrites commits other agents can already hold. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'echo a || git clean -fd',
        'deny',
        'git-guard: git clean deletes untracked files. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'git log | git stash',
        'deny',
        'git-guard: git stash hides uncommitted work other agents depend on. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'sudo git stash',
        'deny',
        'git-guard: git stash hides uncommitted work other agents depend on. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'env FOO=bar git stash',
        'deny',
        'git-guard: git stash hides uncommitted work other agents depend on. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'FOO=bar git stash',
        'deny',
        'git-guard: git stash hides uncommitted work other agents depend on. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'uv run git stash',
        'deny',
        'git-guard: git stash hides uncommitted work other agents depend on. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'pnpm exec git stash',
        'deny',
        'git-guard: git stash hides uncommitted work other agents depend on. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'timeout 5 git stash',
        'deny',
        'git-guard: git stash hides uncommitted work other agents depend on. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'xargs git stash',
        'deny',
        'git-guard: git stash hides uncommitted work other agents depend on. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        'nice -n 10 git stash',
        'deny',
        'git-guard: git stash hides uncommitted work other agents depend on. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    [
        '/usr/bin/git stash',
        'deny',
        'git-guard: git stash hides uncommitted work other agents depend on. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    ['git add .', 'rewrite', ''],
    ["git commit -m 'git stash'", 'rewrite', ''],
    ['echo "git stash"', 'rewrite', ''],
    ["cat <<'EOF2'\ngit stash\nEOF2", 'rewrite', ''],
    [
        'cat <<EOF3\n$(git stash)\nEOF3',
        'deny',
        'git-guard: git stash hides uncommitted work other agents depend on. Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.',
    ],
    ['git stash \\\n  list', 'rewrite', ''],
    ["echo $'git stash'", 'rewrite', ''],
    [`git stash${_DOLLAR}{IFS}list`, 'rewrite', ''],
    ['mise x -- node --version', 'rewrite', ''],
    ['pnpm add effect@3', 'rewrite', ''],
    ['npm install', 'rewrite', ''],
];

// --- [OPERATIONS] ----------------------------------------------------------------------

const _plain = (decision: Decision<Call, unknown, string>): Plain =>
    decision.match<Plain>({
        rewrite: (e, context) => ({ kind: 'rewrite', e, context }),
        deny: (reason) => ({ kind: 'deny', reason }),
        answer: (result) => ({ kind: 'answer', result }),
    });

const _EXPECTED: Readonly<Record<Verdict, (command: string, reason: string) => Plain>> = {
    deny: (_command, reason): Plain => ({ kind: 'deny', reason }),
    rewrite: (command): Plain => ({ kind: 'rewrite', e: { tool: 'Bash', command }, context: [] }),
};

const _guard = gitGuard(_EXISTING);

// --- [TESTS] ---------------------------------------------------------------------------

describe('gitGuard', () => {
    it.each(_VERDICTS)('decides %j as the script did', (command, verdict, reason) => {
        expect(_plain(_guard({ tool: 'Bash', command }))).toStrictEqual(_EXPECTED[verdict](command, reason));
    });

    it('refuses a command longer than the lexing cap', () => {
        const command = `git status ${'#'.repeat(_MAX_COMMAND_KIB * _KIB)}`;
        expect(_plain(_guard({ tool: 'Bash', command }))).toStrictEqual({
            kind: 'deny',
            reason: `git-guard: the command is too long to lex within the hook deadline, it cannot be checked safely. ${ADVICE}`,
        });
    });
});

describe('gitPaths', () => {
    it('names the reset and checkout targets the refinements test', () => {
        expect(gitPaths('git reset HEAD~1')).toStrictEqual(['HEAD~1']);
        expect(gitPaths('git checkout main README.md')).toStrictEqual(['main', 'README.md']);
        expect(gitPaths('git -C /tmp reset --hard x && git checkout -b feature')).toStrictEqual(['x', 'feature']);
        expect(gitPaths('git status')).toStrictEqual([]);
    });
});
