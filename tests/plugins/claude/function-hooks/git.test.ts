import { expect, layer } from '@effect/vitest';
import { Cause, Context, Effect, FileSystem, Layer, Path } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Decider, decided, refusing } from '#/engine';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ISOLATED = { GIT_CONFIG_GLOBAL: '/dev/null', GIT_CONFIG_NOSYSTEM: '1' } as const;
const OVERWRITES = 'overwrites uncommitted edits another session may own';
const BRANCHES = 'changes branches, stay on this branch';
const REFUSED = [
    ['git -C {repo} checkout -- dirty', `git checkout dirty ${OVERWRITES}`],
    ['cd {repo} && git restore dirty', `git restore ${OVERWRITES}`],
    ['git -C {repo} reset --hard', `git reset --hard ${OVERWRITES}`],
    ['git -C {repo} stash', 'git stash hides work'],
    ['git -C {repo} clean -fd', "git clean deletes other sessions' untracked files"],
    ['git -C {session} reset -q --soft HEAD~1', 'git reset HEAD~1 moves HEAD, commit forward'],
    ['git -C {session} branch -D probe', `git branch -D ${BRANCHES}`],
    ['git -C {session} checkout -b probe', `git checkout -b ${BRANCHES}`],
    ['git -C {session} commit --amend --no-edit', 'git commit --amend rewrites the last commit another session may own'],
    ['git -C {session} -c core.hooksPath=/dev/null commit -m probe', 'git commit -c skips the commit hooks'],
    ['git -C {session} push origin :main', 'git push :main rewrites remote history'],
    ['git -C {session} rebase main', 'git rebase rewrites shared commits'],
    ['cd {session} && patch -p1 < fix.diff', 'patch applies a diff outside git, run git apply'],
    ['git -C {repo} worktree add -b probe {session}/tree', '-b adds a branch and a second checkout, work in place, or drop -b to export the commit into the folder'],
] as const;
const ALLOWED = [
    'git -C {repo} checkout -- clean',
    'git -C {repo} restore --staged dirty',
    'git -C {repo} stash list',
    'git -C {repo} clean --dry-run',
    'git -C {repo} checkout -b probe',
    'git -C {repo} reset --soft HEAD~1',
    'git -C {session} branch -r',
    'git -C {session} rebase --abort',
    'git -C {session} revert --no-edit HEAD',
    'git -C {session} branch probe',
    'patch -d {repo} -p1 < fix.diff',
] as const;
const MOVES = 'moves the HEAD every session shares';
const SHARED_REFUSED = [
    ['git switch topic', `git switch topic ${MOVES} off main`],
    ['git switch -', `git switch - ${MOVES} off main`],
    ['git switch --detach HEAD', `git switch --detach ${MOVES} off main`],
    ['git checkout topic', `git checkout topic ${MOVES} off main`],
    ['git checkout --detach', `git checkout --detach ${MOVES} off main`],
    ['git checkout HEAD~1', `git checkout HEAD~1 ${MOVES} off main`],
    ['git bisect start', `git bisect start ${MOVES}`],
    ['git pull', 'git pull merges or rebases onto the HEAD every session shares, run git pull --ff-only'],
    ['git pull --rebase', 'git pull merges or rebases onto the HEAD every session shares, run git pull --ff-only'],
    ['git pull --ff-only --no-ff', 'git pull merges or rebases onto the HEAD every session shares, run git pull --ff-only'],
    ['git merge clash', 'git merge clash conflicts with HEAD'],
    ['git merge topic clash', 'git merge topic clash names more than one branch'],
] as const;
const SHARED_ALLOWED = ['git switch main', 'git checkout main', 'git checkout', 'git bisect log', 'git pull --ff-only', 'git merge topic', 'git merge --abort'] as const;
const REWRITTEN = [
    ['git -C {session} commit --no-verify -m probe', 'git -C {session} commit -m probe'],
    ['git -C {session} commit -nm probe', 'git -C {session} commit -m probe'],
    ['git -C {session} push --force origin main', 'git -C {session} push origin main'],
    ['git -C {session} push origin +main', 'git -C {session} push origin main'],
] as const;

// --- [SERVICES] ------------------------------------------------------------------------

class Folders extends Context.Service<Folders>()('@rasm/test-function-hooks/git.test/Folders', {
    make: Effect.gen(function* () {
        const [fs, path, spawner] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
        const [repo, session] = yield* Effect.all([Effect.flatMap(fs.makeTempDirectoryScoped(), fs.realPath), Effect.flatMap(fs.makeTempDirectoryScoped(), fs.realPath)]);
        const git = (...args: readonly string[]) =>
            Effect.filterOrFail(
                spawner.exitCode(ChildProcess.make('git', ['-c', 'user.name=hooks', '-c', 'user.email=hooks@localhost', ...args], { cwd: repo, env: ISOLATED, extendEnv: true })),
                (code) => code === 0,
                (code) => new Cause.UnknownError(code, `git ${args.join(' ')} exits ${code}`),
            );
        const written = (file: string, text: string) => fs.writeFileString(path.join(repo, file), text);
        const committed = (branch: string, file: string, text: string) => Effect.andThen(Effect.andThen(git('switch', '-q', branch), written(file, text)), git('commit', '-q', '--no-verify', '-am', branch));
        yield* Effect.forEach(['dirty', 'clean', 'lines'], (file) => written(file, 'base\n'), { discard: true });
        yield* Effect.forEach(
            [
                ['init', '-q', '-b', 'main'],
                ['add', '.'],
                ['commit', '-q', '--no-verify', '-m', 'base'],
                ['branch', 'topic'],
                ['branch', 'clash'],
            ],
            (args) => git(...args),
            { discard: true },
        );
        yield* Effect.all([committed('topic', 'clean', 'base\ntopic\n'), committed('clash', 'lines', 'clash\n'), committed('main', 'lines', 'main\n')], { discard: true });
        yield* written('dirty', 'edited\n');
        const branches = spawner.string(ChildProcess.make('git', ['-C', repo, 'branch', '--format=%(refname:short)'], { env: ISOLATED, extendEnv: true }));
        return { repo, session, branches, filled: (template: string) => template.replaceAll('{repo}', repo).replaceAll('{session}', session) };
    }),
}) {
    static readonly layer = Layer.effect(this, this.make);
}

// --- [COMPOSITION] ---------------------------------------------------------------------

layer(Layer.provideMerge(Folders.layer, Decider), { excludeTestServices: true })('git', (it) => {
    it.effect.each(REFUSED)(
        '%s is refused because %s',
        ([command, reason]) =>
            Effect.gen(function* () {
                const { session, filled } = yield* Folders;
                expect(yield* decided(filled(command), session)).toSatisfy(refusing(reason), `refused because ${reason}`);
            }),
        30_000,
    );

    it.effect.each(ALLOWED)(
        '%s runs unchanged',
        (command) =>
            Effect.gen(function* () {
                const { session, filled } = yield* Folders;
                expect(yield* decided(filled(command), session)).toStrictEqual({ _tag: 'Allow' });
            }),
        30_000,
    );

    it.effect(
        'Merge-tree reports topic clean and clash conflicting against main, the partitions the merge rows stand on',
        Effect.fnUntraced(function* () {
            const [spawner, { repo }] = yield* Effect.all([ChildProcessSpawner.ChildProcessSpawner, Folders]);
            const merged = (branch: string) => spawner.exitCode(ChildProcess.make('git', ['-C', repo, 'merge-tree', '--write-tree', '--quiet', 'main', branch], { env: ISOLATED, extendEnv: true }));
            expect(yield* Effect.all([merged('topic'), merged('clash')])).toStrictEqual([0, 1]);
        }),
        30_000,
    );

    it.effect.each(SHARED_REFUSED)(
        '%s in the session tree is refused because %s',
        ([command, reason]) =>
            Effect.gen(function* () {
                const { repo } = yield* Folders;
                expect(yield* decided(command, repo)).toSatisfy(refusing(reason), `refused because ${reason}`);
            }),
        30_000,
    );

    it.effect.each(SHARED_ALLOWED)(
        '%s in the session tree runs unchanged',
        (command) =>
            Effect.gen(function* () {
                const { repo } = yield* Folders;
                expect(yield* decided(command, repo)).toStrictEqual({ _tag: 'Allow' });
            }),
        30_000,
    );

    it.effect.each(REWRITTEN)(
        '%s becomes %s, which runs unchanged when decided again',
        Effect.fnUntraced(function* ([command, rewritten]) {
            const { session, filled } = yield* Folders;
            expect(yield* decided(filled(command), session)).toMatchObject({ _tag: 'Rewrite', command: filled(rewritten), background: false });
            expect(yield* decided(filled(rewritten), session)).toStrictEqual({ _tag: 'Allow' });
        }),
        30_000,
    );

    it.effect(
        'Worktree add of a repository becomes an export that writes the committed tree into the new folder and adds no branch',
        Effect.fnUntraced(function* () {
            const [fs, path, spawner, { repo, session, branches }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner, Folders]);
            const target = path.join(yield* Effect.flatMap(fs.makeTempDirectoryScoped(), fs.realPath), 'exported');
            const before = yield* branches;
            const decision = yield* decided(`git -C ${repo} worktree add ${target}`, session);
            const exported = decision._tag === 'Rewrite' ? decision.command : '';
            expect(decision).toMatchObject({ _tag: 'Rewrite', background: false });
            expect(yield* decided(exported, session)).toStrictEqual({ _tag: 'Allow' });
            expect(yield* spawner.exitCode(ChildProcess.make('bash', ['-c', exported], { cwd: session }))).toBe(0);
            expect(yield* Effect.all([fs.readFileString(path.join(target, 'dirty')), fs.readFileString(path.join(target, 'clean')), branches])).toStrictEqual(['base\n', 'base\n', before]);
            expect(before).not.toBe('');
        }),
        30_000,
    );
});
