import type { Table } from '#/judgment/rule';

// --- [CONSTANTS] -----------------------------------------------------------------------

const restore = 'Read the committed text with `git diff -- <path>` or `git show HEAD:./<path>` and put back only your own lines with your file edit tool.';
const discards = 'discards uncommitted edits other sessions in this checkout may hold';
const force = ['-f', '--force', '--discard-changes'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const git: Table = {
    'git.stash': {
        program: 'git',
        path: ['stash'],
        judge: (invocation) => (['list', 'show'].includes(invocation.path[1] ?? '') ? [] : [{ refuse: `git ${invocation.path.join(' ')} hides work other sessions in this checkout may hold.`, instruct: 'Leave the work in the tree and commit your own files by path.' }]),
    },
    'git.revert': { program: 'git', path: ['revert'], refuse: 'undoes committed work other sessions in this checkout build on', instruct: restore },
    'git.restore': { program: 'git', path: ['restore'], judge: (invocation) => (invocation.has('--staged') && !invocation.has('--worktree') ? [] : [{ refuse: `git restore ${discards}.`, instruct: restore }]) },
    'git.checkout.paths': {
        program: 'git',
        path: ['checkout'],
        judge: (invocation) => (invocation.rest.length > 0 || invocation.writes.length > 0 ? [{ refuse: `git checkout of paths ${discards}.`, instruct: restore }] : []),
    },
    'git.checkout.force': { program: 'git', path: ['checkout'], when: force, refuse: discards, instruct: restore },
    'git.switch.force': { program: 'git', path: ['switch'], when: force, refuse: discards, instruct: restore },
    'git.reset': { program: 'git', path: ['reset'], when: ['--hard', '--merge', '--keep'], refuse: discards, instruct: restore },
    'git.clean': { program: 'git', path: ['clean'], unless: ['-n', '--dry-run'], refuse: 'deletes untracked files other sessions in this checkout may hold', instruct: 'Remove the files you created by name.' },
    'git.checkout-index': { program: 'git', path: ['checkout-index'], when: ['-f', '--force', '-a', '--all'], refuse: discards, instruct: restore },
    'git.update-index': { program: 'git', path: ['update-index'], when: ['--assume-unchanged', '--skip-worktree'], refuse: 'hides edits from git status for every session in this checkout', instruct: 'Leave the index flags alone.' },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { git };
