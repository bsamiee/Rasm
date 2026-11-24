import { expect, layer } from '@effect/vitest';
import { closed, opened, wrote } from '@rasm/function-hooks/rules/git';
import { Cause, Context, Effect, FileSystem, Layer, Match, Path } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Decider, decided, refusing } from '#/engine';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ISOLATED = { GIT_CONFIG_GLOBAL: '/dev/null', GIT_CONFIG_NOSYSTEM: '1' } as const;
const IDENTITY = { GIT_AUTHOR_NAME: 'hooks', GIT_AUTHOR_EMAIL: 'hooks@localhost', GIT_COMMITTER_NAME: 'hooks', GIT_COMMITTER_EMAIL: 'hooks@localhost' } as const;
const OVERWRITES = 'overwrites uncommitted edits another session may own';
const BRANCHES = 'changes branches, stay on this branch';
const FOREIGN = 'overwrites edits this session did not write in';
const REFUSED = [
    ['git -C {repo} checkout -- dirty', `git checkout ${FOREIGN} dirty`],
    ['cd {repo} && git restore dirty', `git restore ${FOREIGN} dirty`],
    ['git -C {repo} reset --hard', `git reset --hard ${OVERWRITES}`],
    ['git -C {repo} stash', 'git stash hides work'],
    ['git -C {repo} clean -fd', "git clean deletes other sessions' untracked files"],
    ['git -C {session} reset -q --soft HEAD~1', 'git reset HEAD~1 moves HEAD, commit forward'],
    ['git -C {session} branch -D probe', `git branch -D ${BRANCHES}`],
    ['git -C {session} checkout -b probe', `git checkout -b ${BRANCHES}`],
    ['git -C {session} commit --amend --no-edit', 'git commit --amend rewrites HEAD, which this session did not commit'],
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
const OWNERS = { a: { session: 'session-a' }, a1: { session: 'session-a', agent: 'a1' }, b: { session: 'session-b' } } as const;
const STEPS = {
    mine: { edit: 'a', file: 'f', old: 'base', new: 'mine', text: 'mine\n' },
    more: { edit: 'a', file: 'f', old: 'mine', new: 'more', text: 'more\n' },
    theirs: { edit: 'b', file: 'f', old: 'base', new: 'theirs', text: 'theirs\n' },
} as const;
const RESTORED = [
    ['an unchanged path', [], 'git restore f', 'a'],
    ['its own edit', [STEPS.mine], 'git restore f', 'a'],
    ['its own edit across the tree', [STEPS.mine], 'git restore .', 'a'],
    [
        'its own write and its own edit of every occurrence',
        [
            { write: 'a', file: 'f', text: 'base\nbase\n' },
            { replace: 'a', file: 'f', old: 'base', new: 'mine', text: 'mine\nmine\n' },
        ],
        'git restore f',
        'a',
    ],
    ['its own edit and its own later write', [STEPS.mine, { write: 'a', file: 'f', text: 'whole\n' }], 'git checkout -- f', 'a'],
    ['its own edits with one staged between them, back to the index', [STEPS.mine, { stage: 'f' }, STEPS.more], 'git restore f', 'a'],
    ['its own edits with one staged between them, back to HEAD', [STEPS.mine, { stage: 'f' }, STEPS.more], 'git checkout HEAD -- f', 'a'],
    ['its own edit over content a shell staged, back to the index', [{ shell: 'f', text: 'staged\n' }, { stage: 'f' }, { edit: 'a', file: 'f', old: 'staged', new: 'mine', text: 'mine\n' }], 'git restore f', 'a'],
    ['its own edit that its own shell call rewrote in place', [STEPS.mine, { runs: 'a', command: "sed -i.orig 's/mine/sed/' f && rm f.orig" }], 'git restore f', 'a'],
    ['a file its own shell call formatted by redirect', [{ runs: 'a', command: String.raw`printf 'formatted\n' > f` }], 'git checkout -- f', 'a'],
    ['a file its own shell call deleted', [{ runs: 'a', command: 'rm f' }], 'git restore f', 'a'],
] as const;
const KEPT = [
    ['its own edit over content a shell staged, back to HEAD with the index', [{ shell: 'f', text: 'staged\n' }, { stage: 'f' }, { edit: 'a', file: 'f', old: 'staged', new: 'mine', text: 'mine\n' }], 'git restore --source=HEAD --staged --worktree f', 'a', `git restore ${FOREIGN} f,`],
    ['its own edit over the HEAD content while a shell staged other content, back to HEAD with the index', [{ shell: 'f', text: 'staged\n' }, { stage: 'f' }, { shell: 'f', text: 'base\n' }, STEPS.mine], 'git checkout HEAD -- f', 'a', `git checkout ${FOREIGN} f,`],
    ['an edit by another session', [STEPS.theirs], 'git restore f', 'a', `git restore ${FOREIGN} f,`],
    ['its own edit that another session edited since', [STEPS.mine, { edit: 'b', file: 'f', old: 'mine', new: 'theirs', text: 'theirs\n' }], 'git restore f', 'a', `git restore ${FOREIGN} f,`],
    ['its own edit that a shell command rewrote since', [STEPS.mine, { shell: 'f', text: 'shell\n' }], 'git checkout -- f', 'a', `git checkout ${FOREIGN} f,`],
    ['its own edit that a shell command deleted since', [STEPS.mine, { remove: 'f' }], 'git restore f', 'a', `git restore ${FOREIGN} f,`],
    ['its own edit beside a path a shell command deleted, naming only the deleted path', [{ edit: 'a', file: 'g', old: 'base', new: 'mine', text: 'mine\n' }, { remove: 'f' }], 'git restore f g', 'a', `git restore ${FOREIGN} f, read`],
    ['an edit by the main agent of its own session, from a subagent', [STEPS.mine], 'git restore f', 'a1', `git restore ${FOREIGN} f,`],
    ['its own shell call over a path another session edited inside the call', [STEPS.mine, { runs: 'a', command: "sed -i.orig 's/$/!/' f && rm f.orig", during: { edit: 'b', file: 'f', old: 'mine', new: 'theirs', text: 'theirs\n' } }], 'git restore f', 'a', `git restore ${FOREIGN} f,`],
    ['its own shell call while a shell call of another session was open', [STEPS.mine, { runs: 'a', command: "sed -i.orig 's/mine/sed/' f && rm f.orig", during: { opens: 'b' } }], 'git restore f', 'a', `git restore ${FOREIGN} f,`],
    [
        'a hand write to a path its own shell call left alone',
        [
            { shell: 'f', text: 'hand\n' },
            { runs: 'a', command: "sed -i.orig 's/base/sed/' g && rm g.orig" },
        ],
        'git restore f',
        'a',
        `git restore ${FOREIGN} f,`,
    ],
    ['its own path beside a path another session edited, naming only that path', [STEPS.mine, { edit: 'b', file: 'g', old: 'base', new: 'theirs', text: 'theirs\n' }], 'git restore f g', 'a', `git restore ${FOREIGN} g,`],
] as const;
const NOT_COMMITTED = 'git commit --amend rewrites HEAD, which this session did not commit';
const AMENDED = [
    ['its own commit', [{ hand: 'echo 1 > c && git add c' }, { by: 'a', command: 'git commit -q -m one' }]],
    ['its own commit it amended before', [{ hand: 'echo 1 > c && git add c' }, { by: 'a', command: 'git commit -q -m one' }, { by: 'a', command: 'git commit --amend -q --no-edit' }]],
    ['its own commit inside a chain that ends in success', [{ hand: 'echo 1 > c && git add c' }, { by: 'a', command: 'false; git commit -q -m one; true' }]],
] as const;
const AMEND_KEPT = [
    ['a commit by another session', [{ hand: 'echo 1 > c && git add c' }, { by: 'b', command: 'git commit -q -m one' }], NOT_COMMITTED],
    ['a commit by a subagent of its own session', [{ hand: 'echo 1 > c && git add c' }, { by: 'a1', command: 'git commit -q -m one' }], NOT_COMMITTED],
    ['a commit made outside any session', [{ hand: 'echo 1 > c && git add c && git commit -q -m one' }], NOT_COMMITTED],
    ['a commit another session made on top of its own', [{ hand: 'echo 1 > c && git add c' }, { by: 'a', command: 'git commit -q -m one' }, { hand: 'echo 2 > c && git add c' }, { by: 'b', command: 'git commit -q -m two' }], NOT_COMMITTED],
    ['a hand commit on the same parent while its own commit failed in a chain that exits 0', [{ by: 'a', command: 'false; git commit -q -m race; true', meanwhile: 'echo 1 > c && git add c && git commit -q -m hand' }], NOT_COMMITTED],
    ['its own revert', [{ hand: 'echo 1 > c && git add c && git commit -q -m one' }, { by: 'a', command: 'git revert --no-edit HEAD' }], NOT_COMMITTED],
    ['its own cherry-pick', [{ hand: 'git switch -q -c side && echo 1 > c && git add c && git commit -q -m side && git switch -q main' }, { by: 'a', command: 'git cherry-pick side' }], NOT_COMMITTED],
    ['its own merge', [{ hand: 'git switch -q -c side && echo 1 > c && git add c && git commit -q -m side && git switch -q main' }, { by: 'a', command: 'git merge --no-ff --no-edit side' }], NOT_COMMITTED],
    ['its own commit once the reflog expired', [{ hand: 'echo 1 > c && git add c' }, { by: 'a', command: 'git commit -q -m one' }, { hand: 'git reflog expire --expire=now --all' }], NOT_COMMITTED],
    ['its own commit once pushed', [{ hand: 'echo 1 > c && git add c' }, { by: 'a', command: 'git commit -q -m one' }, { push: true }], 'git commit --amend rewrites HEAD, which origin/main already holds'],
] as const;
const STAMP = "GIT_REFLOG_ACTION='function-hooks session-a'";
const REWRITTEN = [
    ['git -C {session} commit -m probe', `${STAMP} git -C {session} commit -m probe`],
    ['false; git -C {session} commit -m probe; true', `false; ${STAMP} git -C {session} commit -m probe; true`],
    ['GIT_REFLOG_ACTION=borrowed git -C {session} commit -m probe', `${STAMP} git -C {session} commit -m probe`],
    ['git -C {session} commit --no-verify -m probe', `${STAMP} git -C {session} commit -m probe`],
    ['git -C {session} commit -nm probe', `${STAMP} git -C {session} commit -m probe`],
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

// --- [OPERATIONS] ----------------------------------------------------------------------

const reached = <A, E, R>(effect: Effect.Effect<A, E, R>, holds: (value: A) => boolean, partition: string) => Effect.filterOrFail(effect, holds, (value) => new Cause.UnknownError(value, `setup did not reach ${partition}, got ${JSON.stringify(value)}`));

const scratch = Effect.fnUntraced(function* () {
    const [fs, path, spawner] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
    const [repo, remote] = yield* Effect.all([Effect.flatMap(fs.makeTempDirectoryScoped(), fs.realPath), Effect.flatMap(fs.makeTempDirectoryScoped(), fs.realPath)]);
    const shell = (command: string) => spawner.exitCode(ChildProcess.make('bash', ['-c', command], { cwd: repo, env: { ...ISOLATED, ...IDENTITY }, extendEnv: true }));
    const succeeded = (command: string) => reached(shell(command), (code) => code === 0, `a passing ${command}`);
    const file = (name: string) => path.join(repo, name);
    const decidedAs = (command: string, by: keyof typeof OWNERS, call?: string) => decided(command, repo, { ...OWNERS[by], ...(call === undefined ? {} : { call }) });
    yield* Effect.forEach(['f', 'g'], (name) => fs.writeFileString(file(name), 'base\n'), { discard: true });
    yield* succeeded('git init -q -b main && git add . && git commit -q -m base');
    return {
        repo,
        file,
        shell,
        succeeded,
        decidedAs,
        written: (name: string, text: string) => fs.writeFileString(file(name), text),
        recorded: Effect.fnUntraced(function* (by: keyof typeof OWNERS, name: string, text: string, call: string, result: object) {
            yield* opened({ ...OWNERS[by], call, cwd: repo, file: file(name) });
            const originalFile = (yield* fs.exists(file(name))) ? yield* fs.readFileString(file(name)) : null;
            yield* fs.writeFileString(file(name), text);
            yield* wrote({ ...OWNERS[by], call, file: file(name), result: { ...result, filePath: file(name), originalFile } });
        }),
        shelled: Effect.fnUntraced(function* <E, R>(by: keyof typeof OWNERS, command: string, call: string, during: Effect.Effect<void, E, R>) {
            const decision = yield* reached(decidedAs(command, by, call), ({ _tag }) => _tag === 'Allow' || _tag === 'Rewrite', `a ${command} that runs`);
            yield* opened({ ...OWNERS[by], call, cwd: repo });
            yield* during;
            yield* succeeded('command' in decision ? decision.command : command);
            yield* closed({ ...OWNERS[by], call, cwd: repo });
        }),
        pushed: Effect.andThen(
            succeeded(`git init -q --bare ${remote} && git remote add origin ${remote} && git push -q origin main`),
            reached(spawner.string(ChildProcess.make('git', ['-C', repo, 'for-each-ref', '--contains', 'HEAD', '--format=%(refname:short)', 'refs/remotes'], { env: ISOLATED, extendEnv: true })), (held) => held === 'origin/main\n', 'a HEAD origin/main holds'),
        ),
    };
});

type Scratch = Effect.Success<ReturnType<typeof scratch>>;

const duringStep = (held: Scratch, step: { readonly opens: keyof typeof OWNERS } | { readonly edit: keyof typeof OWNERS; readonly file: string; readonly old: string; readonly new: string; readonly text: string }, at: number) =>
    'opens' in step ? opened({ ...OWNERS[step.opens], call: `open-${at}`, cwd: held.repo }) : held.recorded(step.edit, step.file, step.text, `edit-${at}`, { oldString: step.old, newString: step.new, replaceAll: false, structuredPatch: [], userModified: false });

const restoreStep = (held: Scratch) => (step: (typeof RESTORED | typeof KEPT)[number][1][number], at: number) =>
    Match.value(step).pipe(
        Match.when({ runs: Match.string }, (call) => held.shelled(call.runs, call.command, `call-${at}`, 'during' in call ? duringStep(held, call.during, at + 100) : Effect.void)),
        Match.when({ edit: Match.string }, ({ edit, file, old, new: replacement, text }) => held.recorded(edit, file, text, `edit-${at}`, { oldString: old, newString: replacement, replaceAll: false, structuredPatch: [], userModified: false })),
        Match.when({ replace: Match.string }, ({ replace, file, old, new: replacement, text }) => held.recorded(replace, file, text, `edit-${at}`, { oldString: old, newString: replacement, replaceAll: true, structuredPatch: [], userModified: false })),
        Match.when({ write: Match.string }, ({ write, file, text }) => held.recorded(write, file, text, `edit-${at}`, { type: 'update', content: text, structuredPatch: [] })),
        Match.when({ shell: Match.string }, ({ shell, text }) => held.written(shell, text)),
        Match.when({ remove: Match.string }, ({ remove }) => held.succeeded(`rm ${remove}`)),
        Match.orElse(({ stage }) => held.succeeded(`git add ${stage}`)),
    );

const amendStep = (held: Scratch) => (step: (typeof AMENDED | typeof AMEND_KEPT)[number][1][number], at: number) =>
    Match.value(step).pipe(
        Match.when({ by: Match.string }, (call) =>
            Effect.gen(function* () {
                const decision = yield* reached(held.decidedAs(call.command, call.by, `call-${at}`), ({ _tag }) => _tag === 'Allow' || _tag === 'Rewrite', `a ${call.command} that runs`);
                yield* 'meanwhile' in call ? held.succeeded(call.meanwhile) : Effect.void;
                yield* held.shell('command' in decision ? decision.command : call.command);
            }),
        ),
        Match.when({ hand: Match.string }, ({ hand }) => held.succeeded(hand)),
        Match.orElse(() => held.pushed),
    );

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

    it.effect.each(RESTORED)(
        'Restore after %s runs unchanged',
        ([, steps, command, by]) =>
            Effect.scoped(
                Effect.gen(function* () {
                    const held = yield* scratch();
                    yield* Effect.forEach(steps, restoreStep(held), { discard: true });
                    expect(yield* held.decidedAs(command, by)).toStrictEqual({ _tag: 'Allow' });
                }),
            ),
        30_000,
    );

    it.effect.each(KEPT)(
        'Restore after %s is refused',
        ([, steps, command, by, reason]) =>
            Effect.scoped(
                Effect.gen(function* () {
                    const held = yield* scratch();
                    yield* Effect.forEach(steps, restoreStep(held), { discard: true });
                    expect(yield* held.decidedAs(command, by)).toSatisfy(refusing(reason), `refused because ${reason}`);
                }),
            ),
        30_000,
    );

    it.effect.each(AMENDED)(
        'Amend after %s runs under the session stamp',
        ([, steps]) =>
            Effect.scoped(
                Effect.gen(function* () {
                    const held = yield* scratch();
                    yield* Effect.forEach(steps, amendStep(held), { discard: true });
                    expect(yield* held.decidedAs('git commit --amend -q --no-edit', 'a')).toStrictEqual({ _tag: 'Rewrite', command: `${STAMP} git commit --amend -q --no-edit`, background: false });
                }),
            ),
        30_000,
    );

    it.effect.each(AMEND_KEPT)(
        'Amend after %s is refused',
        ([, steps, reason]) =>
            Effect.scoped(
                Effect.gen(function* () {
                    const held = yield* scratch();
                    yield* Effect.forEach(steps, amendStep(held), { discard: true });
                    expect(yield* held.decidedAs('git commit --amend -q --no-edit', 'a')).toSatisfy(refusing(reason), `refused because ${reason}`);
                }),
            ),
        30_000,
    );

    it.effect.each(REWRITTEN)(
        '%s becomes %s, which runs unchanged when decided again',
        Effect.fnUntraced(function* ([command, rewritten]) {
            const { session, filled } = yield* Folders;
            expect(yield* decided(filled(command), session, OWNERS.a)).toMatchObject({ _tag: 'Rewrite', command: filled(rewritten), background: false });
            expect(yield* decided(filled(rewritten), session, OWNERS.a)).toStrictEqual({ _tag: 'Allow' });
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
