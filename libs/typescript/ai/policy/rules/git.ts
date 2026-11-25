import { Option } from 'effect';
import type { Table } from '#/judgment/rule';

// --- [CONSTANTS] -----------------------------------------------------------------------

const restore = 'Read the committed text with `git diff -- <path>` or `git show HEAD:./<path>` and put back only your own lines with your file edit tool.';
const discards = 'discards uncommitted edits other sessions in this checkout may hold';
const strands = 'drops commits other sessions in this checkout may still need';
const force = ['-f', '--force', '--discard-changes'];
const tags = /^[+:]?refs\/tags\//u;

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
    'git.checkout.reset': { program: 'git', path: ['checkout'], when: ['-B'], refuse: `resets an existing branch and ${strands}`, instruct: 'Create a branch under a new name with `git switch -c <name>`.' },
    'git.switch.force': { program: 'git', path: ['switch'], when: force, refuse: discards, instruct: restore },
    'git.switch.reset': { program: 'git', path: ['switch'], when: ['-C', '--force-create'], refuse: `resets an existing branch and ${strands}`, instruct: 'Create a branch under a new name with `git switch -c <name>`.' },
    'git.reset': { program: 'git', path: ['reset'], when: ['--hard', '--merge', '--keep'], refuse: discards, instruct: restore },
    'git.clean': { program: 'git', path: ['clean'], unless: ['-n', '--dry-run'], refuse: 'deletes untracked files other sessions in this checkout may hold', instruct: 'Remove the files you created by name.' },
    'git.rm.force': { program: 'git', path: ['rm'], when: ['-f', '--force'], unless: ['--cached'], refuse: discards, instruct: 'Delete only files you created, by name, and leave edited files to their sessions.' },
    'git.checkout-index': { program: 'git', path: ['checkout-index'], when: ['-f', '--force', '-a', '--all'], refuse: discards, instruct: restore },
    'git.update-index': { program: 'git', path: ['update-index'], when: ['--assume-unchanged', '--skip-worktree'], refuse: 'hides edits from git status for every session in this checkout', instruct: 'Leave the index flags alone.' },
    'git.branch.delete': {
        program: 'git',
        path: ['branch'],
        judge: (invocation) => (invocation.has('-D') || (invocation.has('-d', '--delete') && invocation.has('-f', '--force')) ? [{ refuse: `git branch -D ${strands}.`, instruct: 'Delete it with `git branch -d`, which refuses a branch whose commits are not merged.' }] : []),
    },
    'git.branch.overwrite': { program: 'git', path: ['branch'], when: ['-M', '-C'], refuse: `overwrites an existing branch and ${strands}`, instruct: 'Rename or copy to a name no branch holds, without the forcing flag.' },
    'git.push': {
        program: 'git',
        path: ['push'],
        judge: (invocation) => {
            const refs = invocation.operands.slice(1).flatMap(({ word }) => Option.toArray(word.text));
            const tagged = refs.length > 0 && refs.every((ref) => tags.test(ref));
            const forced = invocation.has('-f', '--force', '--force-with-lease', '--force-if-includes') || refs.some((ref) => ref.startsWith('+'));
            const deleted = invocation.has('-d', '--delete', '--mirror', '--prune') || refs.some((ref) => ref.startsWith(':'));
            return tagged
                ? []
                : [
                      ...(forced ? [{ refuse: 'git push with force overwrites commits on the remote that other sessions may have pushed.', instruct: 'Pull, integrate their commits, and push without force.' }] : []),
                      ...(deleted ? [{ refuse: 'git push that deletes remote refs drops commits other sessions may still need.', instruct: 'Leave remote branches in place.' }] : []),
                  ];
        },
    },
    'git.tag.delete': { program: 'git', path: ['tag'], when: ['-d', '--delete'], refuse: 'deletes a tag that marks work other sessions and releases point at', instruct: 'Leave tags in place.' },
    'git.update-ref.delete': { program: 'git', path: ['update-ref'], when: ['-d'], refuse: `deletes a ref and ${strands}`, instruct: 'Leave the ref in place.' },
    'git.reflog.delete': {
        program: 'git',
        path: ['reflog'],
        judge: (invocation) => (['delete', 'drop', 'expire'].includes(invocation.path[1] ?? Option.getOrElse(invocation.operand(), () => '')) ? [{ refuse: 'git reflog delete or expire erases the record that recovers lost commits.', instruct: 'Leave the reflog intact.' }] : []),
    },
    'git.filter-branch': { program: 'git', path: ['filter-branch'], refuse: 'rewrites every commit and drops the history other sessions build on', instruct: 'Commit the change forward.' },
    'git.filter-repo': { program: 'git', path: ['filter-repo'], refuse: 'rewrites every commit and drops the history other sessions build on', instruct: 'Commit the change forward.' },
    'git.worktree.remove': { program: 'git', path: ['worktree', 'remove'], when: ['-f', '--force'], refuse: 'deletes a worktree with its uncommitted edits', instruct: 'Commit or move its edits, then run `git worktree remove` without --force.' },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { git };
