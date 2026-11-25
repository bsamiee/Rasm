import process from 'node:process';
import { Array, Clock, Effect, Function, Match, Option, Predicate, Result, Schema, String, Struct } from 'effect';
import manifest from '#/.claude-plugin/plugin.json' with { type: 'json' };
import { Host, HostError } from '#/host';
import type { Request } from '#/rules/decision';
import { all, commandOf, edited, folderOf, holds, inserted, perSubcommand, quote, type Rule, type Ruleset, refusal, refuse, relocated, respell, spelled, unresolved, when, whole } from '#/rules/rule';
import type { Argument } from '#/shell/arguments';
import type { Invocation } from '#/shell/invocation';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BRANCHES = ['changes branches', 'stay on this branch'] as const;
const MOVES = 'moves the HEAD every session shares';
const OVERWRITES = ['overwrites uncommitted edits another session may own', 'read git diff and undo only your own hunks with your file edit tool'] as const;
const FOREIGN = 'overwrites edits this session did not write in';
const LEDGER = '.jsonl';
const ABSENT = /^0+$/u;
const REFLOG_ACTION = 'GIT_REFLOG_ACTION';
const HOOKS = ['skips the commit hooks', 'fix what they report'] as const;
const HISTORY = ['rewrites shared history', 'commit forward'] as const;
const HOOKS_PATH = /^core\.hookspath=/iu;
const REMOTE = ['rewrites remote history', 'push plain refs'] as const;
const ALIASES = ['aliases hide subcommands', 'run them directly'] as const;
const TAG = /^[+:]?refs\/tags\//u;
const ALIAS = /^alias\./u;
const BRANCH_WRITES = ['--delete', '-D', '-M', '--force', '--copy', '-C', '--move'];
const RESET_DISCARDS = ['--hard', '--merge', '--keep'];
const UNWINDS = ['--abort', '--quit'];

// --- [MODELS] --------------------------------------------------------------------------

const Entry = Schema.TaggedUnion({
    Wrote: { agent: Schema.optionalKey(Schema.String), path: Schema.String, before: Schema.optionalKey(Schema.String), after: Schema.optionalKey(Schema.String) },
    Opened: { agent: Schema.optionalKey(Schema.String), call: Schema.String, at: Schema.Finite, pid: Schema.Int, tree: Schema.optionalKey(Schema.String), path: Schema.optionalKey(Schema.String), until: Schema.optionalKey(Schema.Finite) },
    Closed: { agent: Schema.optionalKey(Schema.String), call: Schema.String, at: Schema.Finite },
});
const Line = Schema.fromJsonString(Entry);
const Output = Schema.Union([Schema.Struct({ filePath: Schema.String, oldString: Schema.String, newString: Schema.String, originalFile: Schema.NullOr(Schema.String), replaceAll: Schema.Boolean }), Schema.Struct({ filePath: Schema.String, content: Schema.String, originalFile: Schema.NullOr(Schema.String) })]);
const Calling = Schema.Struct({ session: Schema.String, agent: Schema.optionalKey(Schema.String), call: Schema.String, cwd: Schema.String, file: Schema.optionalKey(Schema.String), until: Schema.optionalKey(Schema.Finite) }).annotate({
    description: "A tool call about to run: a file tool's file, or a shell call's session folder whose worktree is snapshot, and the latest instant a bounded call can still write",
});
const Writing = Schema.Struct({ session: Schema.String, agent: Schema.optionalKey(Schema.String), call: Schema.String, file: Schema.String, result: Schema.Unknown }).annotate({
    description: "An Edit or Write call's result after it ran, recorded as the writing agent's change of that file's blob and closing the call's window",
});

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [LEDGER]

const markerOf = ({ session, agent }: Pick<Request, 'session' | 'agent'>) => `${manifest.name} ${[session, ...Option.toArray(Option.fromUndefinedOr(agent))].join('/')}`;

const located = (host: Host['Service'], folder: string) =>
    Effect.map(host.run(['git', '-C', folder, 'rev-parse', '--absolute-git-dir', '--show-toplevel', '--show-prefix']), ({ code, stdout }) => {
        const [gitdir = '', toplevel = '', prefix = ''] = stdout.split('\n');
        return Option.liftPredicate({ gitdir, ledger: host.path.join(gitdir, manifest.name), toplevel, prefix }, () => code === 0);
    });

const ledgersOf = Effect.fnUntraced(function* (host: Host['Service'], ledger: string) {
    const listed = yield* Effect.orElseSucceed(host.fs.readDirectory(ledger), (): readonly string[] => []);
    const named = listed.filter((file) => host.path.extname(file) === LEDGER);
    const texts = yield* Effect.forEach(named, (file) => Effect.orElseSucceed(host.fs.readFileString(host.path.join(ledger, file)), () => ''));
    const files = Array.zip(named, texts).map(([file, text]) => ({ session: host.path.basename(file, LEDGER), path: host.path.join(ledger, file), text, entries: Array.getSomes(text.split('\n').map((line) => Schema.decodeOption(Line)(line))) }));
    return { files, entries: files.flatMap(({ session, entries }) => entries.map((entry) => [session, entry] as const)) };
});

const windowsOf = (entries: readonly (readonly [string, typeof Entry.Type])[], at: number) => {
    const ended = new Map(entries.flatMap(([owner, entry]) => (Predicate.isTagged(entry, 'Closed') ? [[`${owner} ${entry.call}`, entry.at] as const] : [])));
    return Effect.forEach(
        entries.flatMap(([owner, entry]) => (Predicate.isTagged(entry, 'Opened') ? [{ owner, entry }] : [])),
        ({ owner, entry }) => Effect.map(Effect.isSuccess(Effect.try({ try: () => process.kill(entry.pid, 0), catch: HostError.of('kill') })), (alive) => ({ owner, entry, end: ended.get(`${owner} ${entry.call}`) ?? Math.min(entry.until ?? Number.POSITIVE_INFINITY, alive ? Number.POSITIVE_INFINITY : at) })),
    );
};

const blobsOf = (listed: string, field: number) =>
    new Map(
        listed
            .split('\0')
            .filter(String.isNonEmpty)
            .map((record) => [record.slice(record.indexOf('\t') + 1), record.slice(0, record.indexOf('\t')).split(' ')[field] ?? ''] as const),
    );

const swept = Effect.fnUntraced(function* (host: Host['Service'], { ledger, toplevel }: { readonly ledger: string; readonly toplevel: string }) {
    const [{ files, entries }, at] = yield* Effect.all([ledgersOf(host, ledger), Clock.currentTimeMillis]);
    const windows = yield* windowsOf(entries, at);
    const opening = Math.min(...windows.filter(({ end }) => end > at).map(({ entry }) => entry.at));
    const paths = Array.dedupe(entries.flatMap(([, entry]) => (Predicate.isTagged(entry, 'Wrote') ? [entry.path] : [])));
    const git = (...args: readonly string[]) => host.output(['git', '--literal-pathspecs', '-C', toplevel, ...args, '--', ...paths]);
    const [head, index, kept] = paths.length === 0 ? ['', '', []] : yield* Effect.all([git('ls-tree', '-z', 'HEAD'), git('ls-files', '-s', '-z'), Effect.filter(paths, (path) => host.exists(host.path.join(toplevel, path)))]);
    const hashed = kept.length === 0 ? [] : (yield* host.output(['git', '-C', toplevel, 'hash-object', '--', ...kept])).split('\n');
    const [committed, staged, worktree] = [blobsOf(head, 2), blobsOf(index, 1), new Map(Array.zip(kept, hashed))];
    const grants = ({ entries: held }: (typeof files)[number]) => {
        const writes = held.flatMap((entry) => (Predicate.isTagged(entry, 'Wrote') ? [entry] : []));
        return writes.some(({ path, agent }) => {
            const links = writes.filter((link) => link.path === path && link.agent === agent);
            const [h, i, w] = [committed.get(path), staged.get(path), worktree.get(path)];
            return (w !== i && derived(links, i, w)) || (w !== h && derived(links, h, w));
        });
    };
    const live = (file: (typeof files)[number]) => windows.some(({ owner, end }) => owner === file.session && end > at) || file.entries.some((entry) => Predicate.isTagged(entry, 'Closed') && entry.at >= opening) || grants(file);
    yield* Effect.forEach(
        files.filter((file) => !live(file)),
        ({ path, text }) =>
            Effect.mapError(
                Effect.gen(function* () {
                    const tomb = `${path}.swept`;
                    yield* host.fs.rename(path, tomb);
                    const late = (yield* host.fs.readFileString(tomb)).slice(text.length);
                    yield* String.isNonEmpty(late) ? host.fs.writeFileString(path, late, { flag: 'a' }) : Effect.void;
                    yield* host.fs.remove(tomb);
                }),
                HostError.of(path),
            ),
        { discard: true },
    );
});

const afterOf = (output: typeof Output.Type): Option.Option<string> =>
    'content' in output
        ? Option.some(output.content)
        : Option.match(Option.fromNullOr(output.originalFile), {
              onNone: () => Option.some(output.newString),
              onSome: (original) => Option.liftPredicate(output.replaceAll ? original.split(output.oldString).join(output.newString) : original.replace(output.oldString, () => output.newString), () => original.includes(output.oldString)),
          });

const blobOf = (oid: string) => Option.liftPredicate(oid, (held: string) => !ABSENT.test(held));

const changesOf = (listed: string) =>
    Array.chunksOf(listed.split('\0').filter(String.isNonEmpty), 2).map(([meta, path = '']) => {
        const [from = '', to = '', status = ''] = meta.split(' ').slice(2);
        return { path, from: blobOf(from), to: Option.filter(blobOf(to), () => status !== 'D'), kept: status !== 'D' };
    });

const appended = Effect.fnUntraced(function* (host: Host['Service'], ledger: string, session: string, entries: readonly (typeof Entry.Type)[]) {
    const lines = yield* Effect.orDie(Effect.forEach(entries, (entry) => Schema.encodeEffect(Line)(entry)));
    yield* Effect.mapError(Effect.andThen(host.fs.makeDirectory(ledger, { recursive: true }), host.fs.writeFileString(host.path.join(ledger, `${session}${LEDGER}`), lines.map((line) => `${line}\n`).join(''), { flag: 'a' })), HostError.of(ledger));
});

const snapshot = Effect.fnUntraced(function* (host: Host['Service'], { gitdir, toplevel }: { readonly gitdir: string; readonly toplevel: string }) {
    const index = yield* Effect.mapError(host.fs.makeTempFileScoped(), HostError.of('index'));
    yield* Effect.ignore(host.fs.copyFile(host.path.join(gitdir, 'index'), index));
    yield* host.run(['git', '-C', toplevel, 'add', '-A'], { env: { GIT_INDEX_FILE: index } });
    return (yield* host.run(['git', '-C', toplevel, 'write-tree'], { env: { GIT_INDEX_FILE: index } })).stdout.trim();
}, Effect.scoped);

const derived = (links: readonly { readonly before?: string; readonly after?: string }[], from: string | undefined, to: string | undefined, seen: ReadonlySet<string | undefined> = new Set()): boolean =>
    from === to || (!seen.has(to) && links.some(({ before, after }) => after === to && derived(links, from, before, seen.union(new Set([to])))));

const foreign = Effect.fnUntraced(function* (host: Host['Service'], repository: string, { session, agent }: Pick<Request, 'session' | 'agent'>, pathspecs: readonly string[], source: Option.Option<string>, indexed: boolean) {
    const place = yield* located(host, repository);
    if (Option.isNone(place)) {
        return pathspecs;
    }
    const { ledger, toplevel } = place.value;
    const diffed = (...against: readonly string[]) => Effect.map(host.output(['git', '-C', repository, 'diff', '--raw', '-z', '--no-abbrev', '--no-renames', '--no-relative', ...against, '--', ...pathspecs]), changesOf);
    const [worktree, index, entries] = yield* Effect.all([diffed(...Option.toArray(source)), indexed ? diffed('--cached', ...Option.toArray(source)) : Effect.succeed([]), Effect.map(ledgersOf(host, ledger), Struct.get('entries'))]);
    const present = worktree.filter(({ kept }) => kept);
    const hashed = present.length === 0 ? [] : (yield* host.output(['git', '-C', toplevel, 'hash-object', '--', ...present.map(({ path }) => path)])).split('\n');
    const blobs = new Map(present.map(({ path }, at) => [path, hashed[at]] as const));
    const links = Array.groupBy(
        entries.flatMap(([owner, entry]) => (Predicate.isTagged(entry, 'Wrote') && owner === session && entry.agent === agent ? [entry] : [])),
        Struct.get('path'),
    );
    const unowned = [...worktree.map(({ path, from }) => ({ path, from, to: Option.fromUndefinedOr(blobs.get(path)) })), ...index].filter(({ path, from, to }) => !derived(links[path] ?? [], Option.getOrUndefined(from), Option.getOrUndefined(to)));
    return Array.dedupe(unowned.map(Struct.get('path')));
});

const refusedPaths = (invocation: Invocation, paths: readonly string[]) => (paths.length === 0 ? [] : refusal(`${commandOf(invocation)} ${FOREIGN} ${paths.join(', ')}, ${OVERWRITES[1]}`));

// --- [RULES]

const nested =
    (...names: readonly string[]): Predicate.Predicate<Invocation> =>
    ({ path, operands }) =>
        Option.exists(Option.firstSomeOf([Array.get(path, 1), Option.map(Array.head(operands), Struct.get('text'))]), (name) => names.includes(name));

const scoped = ({ args, words, path: [subcommand] }: Invocation) => {
    const at = words.find((word) => word.text === subcommand && !args.some(({ values }) => values.includes(word)))?.start ?? Number.POSITIVE_INFINITY;
    return Array.partition(
        args,
        Result.liftPredicate(({ words: [flag] }: Argument) => flag.start > at, Function.identity),
    );
};

const repositoryOf = (host: Host['Service'], invocation: Invocation) =>
    scoped(invocation)[1]
        .filter(({ names }) => names.includes('-C'))
        .flatMap(({ values }) => values)
        .reduce((folder, { text }) => host.resolve(text, folder), invocation.command.cwd);

const present = (host: Host['Service'], invocation: Invocation, word: string) =>
    Option.match(host.resolve(word, repositoryOf(host, invocation)), {
        onNone: () => Effect.succeed(false),
        onSome: host.exists,
    });

const inRepository =
    (...rules: readonly Rule[]): Rule =>
    (invocation, call) =>
        Effect.map(all(...rules)(invocation, call), (verdicts) => (verdicts.length > 0 && Option.isNone(repositoryOf(call.host, invocation)) ? unresolved(invocation) : verdicts));

const dirty = (host: Host['Service'], invocation: Invocation, paths: readonly string[]) =>
    Effect.orElseSucceed(
        Effect.map(host.output(['git', '-C', Option.getOrElse(repositoryOf(host, invocation), () => host.root), 'status', '--porcelain', '--', ...paths]), (listed) => listed.trim() !== ''),
        () => true,
    );

const namesPaths = (host: Host['Service'], invocation: Invocation) =>
    Option.match(Array.head(invocation.operands), {
        onNone: () => Effect.succeed(false),
        onSome: (first) => (invocation.tail.length > 0 || invocation.operands.length > 1 || first.text === '.' || first.text.startsWith(':') ? Effect.succeed(true) : Effect.map(present(host, invocation, first.text), (found) => found && first.text !== '-')),
    });

const movedHead: Rule = Effect.fnUntraced(function* (invocation, { host }) {
    const detached = invocation.has('--detach');
    const target = Array.head(invocation.operands);
    if ((!detached && Option.isNone(target)) || (yield* namesPaths(host, invocation))) {
        return [];
    }
    const repository = Option.getOrElse(repositoryOf(host, invocation), () => host.root);
    const branch = Option.liftPredicate((yield* host.output(['git', '-C', repository, 'symbolic-ref', '--short', '-q', 'HEAD'])).trim(), String.isNonEmpty);
    const staying = !detached && Option.exists(target, ({ known, text }) => known && Option.contains(branch, text));
    const held = Option.getOrElse(branch, () => 'HEAD');
    const subject = detached ? '--detach' : Option.match(target, { onNone: () => '--detach', onSome: ({ text }) => text });
    return staying ? [] : refusal(`${commandOf(invocation)} ${subject} ${MOVES} off ${held}, stay on ${held} and read other commits through git show <commit>:<path>`);
});

const cleanMerge: Rule = Effect.fnUntraced(function* (invocation, { host }) {
    const { operands } = invocation;
    if (operands.length > 1 || !operands.every(({ known }) => known)) {
        return refusal(`${commandOf(invocation)} ${spelled(operands)} names more than one branch or one that expands later, merge one literal branch at a time`);
    }
    const target = Option.getOrElse(Option.map(Array.head(operands), Struct.get('text')), () => '@{upstream}');
    const { code, stderr } = yield* host.run(['git', '-C', Option.getOrElse(repositoryOf(host, invocation), () => host.root), 'merge-tree', '--write-tree', '--quiet', 'HEAD', target]);
    return Match.value(code).pipe(
        Match.when(0, () => []),
        Match.when(1, () => refusal(`${commandOf(invocation)} ${target} conflicts with HEAD, which leaves conflict markers in the tree every session shares, merge it in a clone under "$TMPDIR"`)),
        Match.orElse(() => refusal(`${commandOf(invocation)} ${target} is no merge git merge-tree can check, ${stderr.trim()}`)),
    );
});

const checkoutPaths: Rule = Effect.fnUntraced(function* (invocation, { host, request }) {
    const { operands, tail } = invocation;
    const [first] = operands;
    if (first === undefined || invocation.has('--detach') || !(yield* namesPaths(host, invocation))) {
        return [];
    }
    if (!operands.every(({ known }) => known)) {
        return refusal(`${commandOf(invocation)} ${first.text} ${OVERWRITES.join(', ')}`);
    }
    const tree = tail.length > 0 ? operands.filter((word) => !tail.includes(word)) : operands.slice(0, operands.length > 1 && !(yield* present(host, invocation, first.text)) ? 1 : 0);
    const source = Option.map(Array.head(tree), Struct.get('text'));
    const paths = operands.filter((word) => !tree.includes(word)).map(Struct.get('text'));
    return refusedPaths(
        invocation,
        yield* foreign(
            host,
            Option.getOrElse(repositoryOf(host, invocation), () => host.root),
            request,
            paths,
            source,
            Option.isSome(source),
        ),
    );
});

const restoredPaths: Rule = Effect.fnUntraced(function* (invocation, { host, request }) {
    const { operands } = invocation;
    if (!operands.every(({ known }) => known)) {
        return refusal(`${commandOf(invocation)} ${OVERWRITES.join(', ')}`);
    }
    const staged = invocation.has('--staged');
    const source = Option.orElse(Array.head(invocation.values('--source', '-s')), () => Option.liftPredicate('HEAD', () => staged));
    return refusedPaths(
        invocation,
        yield* foreign(
            host,
            Option.getOrElse(repositoryOf(host, invocation), () => host.root),
            request,
            operands.map(Struct.get('text')),
            source,
            staged,
        ),
    );
});

const stamped: Rule = (invocation, { request }) => {
    const marker = `${REFLOG_ACTION}=${markerOf(request)}`;
    const held = [...invocation.outer, invocation].flatMap(({ assigned }) => assigned).find(({ text }) => text.startsWith(`${REFLOG_ACTION}=`));
    return Option.match(Option.fromUndefinedOr(held), {
        onNone: () => inserted(invocation, invocation.command.start, `${REFLOG_ACTION}=${quote(markerOf(request))} `),
        onSome: (word) => (word.text === marker ? [] : edited(invocation, word, `${REFLOG_ACTION}=${quote(markerOf(request))}`)),
    });
};

const ownedHead: Rule = Effect.fnUntraced(function* (invocation, { host, request }) {
    const repository = Option.getOrElse(repositoryOf(host, invocation), () => host.root);
    const [head, reflog, held] = yield* Effect.all([
        host.output(['git', '-C', repository, 'log', '-1', '--format=%H%n%B', 'HEAD']),
        host.output(['git', '-C', repository, 'reflog', 'show', '--format=%H%x00%gs', 'HEAD']),
        host.output(['git', '-C', repository, 'for-each-ref', '--contains', 'HEAD', '--format=%(refname:short)', 'refs/remotes']),
    ]);
    const [commit = '', subject = ''] = head.split('\n');
    const created = Array.findLast(reflog.split('\n'), (line) => line.startsWith(`${commit}\0`));
    const mine = String.isNonEmpty(commit) && Option.contains(created, `${commit}\0${markerOf(request)}: ${subject}`);
    const remotes = held.split('\n').filter(String.isNonEmpty);
    return Match.value({ mine, pushed: remotes.length > 0 }).pipe(
        Match.when({ mine: false }, () => refusal(`${commandOf(invocation)} --amend rewrites HEAD, which this session did not commit, commit forward`)),
        Match.when({ pushed: true }, () => refusal(`${commandOf(invocation)} --amend rewrites HEAD, which ${remotes.join(', ')} already holds, commit forward`)),
        Match.orElse(() => []),
    );
});

const appliedPatch: Rule = Effect.fnUntraced(function* (invocation, { host }) {
    const { operands } = invocation;
    if (operands.length === 0 || !operands.every(({ known }) => known)) {
        return refusal(`${commandOf(invocation)} reads a patch no check can list before it runs, pass the patch file`);
    }
    const listed = yield* Effect.orElseSucceed(host.output(['git', '-C', Option.getOrElse(repositoryOf(host, invocation), () => host.root), 'apply', '--numstat', ...operands.map(({ text }) => text)]), () => '');
    const paths = listed.split('\n').flatMap((line) => Option.toArray(Array.get(line.split('\t'), 2)));
    return paths.length === 0 || (yield* dirty(host, invocation, paths)) ? refusal(`${commandOf(invocation)} ${OVERWRITES.join(', ')}`) : [];
});

const resetTarget: Rule = Effect.fnUntraced(function* (invocation, { host }) {
    const [target] = invocation.operands;
    const paths = yield* Effect.forEach(invocation.operands, ({ text }) => present(host, invocation, text));
    if (target === undefined || target.text === 'HEAD' || target.text === '@' || invocation.tail.length > 0 || paths.some(Boolean)) {
        return [];
    }
    return !target.known && invocation.command.origin === 'call' ? inserted(invocation, target.start, '-- ') : refusal(`${commandOf(invocation)} ${target.text} moves HEAD, commit forward`);
});

const exportedTree: Rule = Effect.fnUntraced(function* (invocation, { host }) {
    const { path, operands } = invocation;
    const [folder, revision] = path[1] === undefined ? operands.slice(1) : operands;
    if (!nested('add')(invocation) || folder === undefined) {
        return [];
    }
    const branched = ['-b', '-B', '--orphan'].find((flag) => invocation.has(flag));
    if (branched !== undefined) {
        return refusal(`${commandOf(invocation)} ${branched} adds a branch and a second checkout, work in place, or drop ${branched} to export the commit into the folder`);
    }
    const temporary = Option.getOrElse(yield* host.environment('TMPDIR'), () => '/tmp');
    const target = Option.getOrUndefined(Option.filter(host.resolve(relocated(folder.text, temporary), repositoryOf(host, invocation)), () => folder.known));
    const found = target !== undefined && (yield* host.exists(target));
    const occupied = target === undefined || (found && (yield* Effect.match(host.fs.readDirectory(target), { onFailure: () => true, onSuccess: Array.isReadonlyArrayNonEmpty })));
    if (occupied) {
        return refusal(`${commandOf(invocation)} ${folder.raw} ${folder.known ? 'holds files the export overwrites, pass a new folder' : 'expands later, pass a literal path'}`);
    }
    const git = ['git', ...invocation.supplied('-C').flatMap(({ raw }) => ['-C', raw])].join(' ');
    const exported = relocated(folder.raw);
    return whole(invocation, `mkdir -p ${exported} && ${git} archive ${revision?.raw ?? 'HEAD'} | tar -x -C ${exported}`, 'git worktree add');
});

const branchWrites: Rule = (invocation) => {
    const named = new Set(scoped(invocation)[0].flatMap(({ names }) => names));
    return BRANCH_WRITES.filter((name) => named.has(name))
        .slice(0, 1)
        .flatMap((written) => refusal(`${commandOf(invocation)} ${written} ${BRANCHES.join(', ')}`));
};

const tagsAlone: Predicate.Predicate<Invocation> = ({ operands: [, ...refs] }) => Array.isReadonlyArrayNonEmpty(refs) && refs.every(({ text }) => TAG.test(text));

const forcedRefs: Rule = (invocation) =>
    invocation.operands
        .filter(({ text }) => !TAG.test(text))
        .flatMap((word) => {
            if (word.text.startsWith(':')) {
                return refusal(`${commandOf(invocation)} ${word.text} ${REMOTE.join(', ')}`);
            }
            return word.text.startsWith('+') && word.known ? edited(invocation, word, quote(word.text.slice(1)), `git push ${word.text}`) : [];
        });

// --- [ROUTES]

const opened = Effect.fn('opened')(
    function* ({ session, agent, call, file, until }: typeof Calling.Type) {
        const host = yield* Host;
        const place = yield* Effect.fromOption(yield* located(host, file === undefined ? host.root : host.path.dirname(file)));
        const [tree, at] = yield* Effect.all([file === undefined ? Effect.asSome(snapshot(host, place)) : Effect.succeedNone, Clock.currentTimeMillis]);
        yield* appended(host, place.ledger, session, [
            Entry.cases.Opened.make({ call, at, pid: process.ppid, ...Option.getOrUndefined(Option.bindTo(tree, 'tree')), ...(file === undefined ? {} : { path: `${place.prefix}${host.path.basename(file)}` }), ...(until === undefined ? {} : { until }), ...(agent === undefined ? {} : { agent }) }),
        ]);
    },
    Effect.catchNoSuchElement,
    (effect, { cwd }) => Effect.provideServiceEffect(effect, Host, Host.make(cwd)),
);

const closed = Effect.fn('closed')(
    function* ({ session, agent, call }: typeof Calling.Type) {
        const host = yield* Host;
        const place = yield* Effect.fromOption(yield* located(host, host.root));
        const { ledger, toplevel } = place;
        const start = yield* Effect.fromOption(Array.findLast((yield* ledgersOf(host, ledger)).entries, ([owner, entry]) => (owner === session && entry._tag === 'Opened' && entry.call === call ? Option.some(entry) : Option.none())));
        const before = yield* Effect.fromNullishOr(start.tree);
        const tree = yield* snapshot(host, place);
        const [at, { entries }] = yield* Effect.all([Clock.currentTimeMillis, ledgersOf(host, ledger)]);
        const living = (yield* windowsOf(entries, at)).filter(({ owner, entry }) => owner !== session || entry.agent !== agent);
        const overlapping = living.filter(({ entry, end }) => entry.at <= at && end >= start.at);
        const blocked = new Set(overlapping.map(({ entry }) => entry.path));
        const changes = blocked.has(undefined) ? [] : changesOf(yield* host.output(['git', '-C', toplevel, 'diff-tree', '-r', '-z', '--no-renames', before, tree])).filter(({ path }) => !blocked.has(path));
        const held = agent === undefined ? {} : { agent };
        const dead = living.filter(({ end }) => end === at);
        yield* Effect.forEach(dead, ({ owner, entry }) => appended(host, ledger, owner, [Entry.cases.Closed.make({ call: entry.call, at, ...(entry.agent === undefined ? {} : { agent: entry.agent }) })]), { discard: true });
        yield* appended(host, ledger, session, [...changes.map(({ path, from, to }) => Entry.cases.Wrote.make({ path, ...held, ...Option.getOrUndefined(Option.bindTo(from, 'before')), ...Option.getOrUndefined(Option.bindTo(to, 'after')) })), Entry.cases.Closed.make({ call, at, ...held })]);
        yield* swept(host, place);
    },
    Effect.catchNoSuchElement,
    (effect, { cwd }) => Effect.provideServiceEffect(effect, Host, Host.make(cwd)),
);

const wrote = Effect.fn('wrote')(function* ({ session, agent, call, file, result }: typeof Writing.Type) {
    const host = yield* Host;
    const place = yield* Effect.fromOption(yield* located(host, host.path.dirname(file)));
    const { ledger, toplevel, prefix } = place;
    const path = `${prefix}${host.path.basename(file)}`;
    const output = Schema.decodeUnknownOption(Output)(result);
    const held = agent === undefined ? {} : { agent };
    const hashed = (stdin: string) => Effect.map(host.run(['git', '-C', toplevel, 'hash-object', '--stdin', `--path=${path}`], { stdin }), ({ stdout }) => stdout.trim());
    const written = yield* Effect.transposeOption(Option.map(Option.all({ decoded: output, after: Option.flatMap(output, afterOf) }), ({ decoded, after }) => Effect.all({ before: Effect.transposeOption(Option.map(Option.fromNullOr(decoded.originalFile), hashed)), after: hashed(after) })));
    const at = yield* Clock.currentTimeMillis;
    yield* appended(host, ledger, session, [...Option.toArray(Option.map(written, ({ before, after }) => Entry.cases.Wrote.make({ path, after, ...held, ...Option.getOrUndefined(Option.bindTo(before, 'before')) }))), Entry.cases.Closed.make({ call, at, ...held })]);
    yield* swept(host, place);
}, Effect.catchNoSuchElement);

// --- [COMPOSITION] ---------------------------------------------------------------------

const UNFORCED = respell({ drops: ['--force', '--force-with-lease', '--force-if-includes', '--discard-changes'], notify: true });

const GIT: Ruleset = {
    programs: {
        git: {
            stopsAtOperand: ['stash', 'grep', 'config'],
            rules: [
                inRepository(
                    perSubcommand({
                        checkout: [UNFORCED, refuse(...OVERWRITES, '-B', '--patch', '--ours', '--theirs'), checkoutPaths],
                        clean: [when(({ has }) => !has('--dry-run'), refuse("deletes other sessions' untracked files", 'rm the paths you created by name'))],
                        reset: [refuse(...OVERWRITES, ...RESET_DISCARDS)],
                        apply: [when(({ has }) => !has('--cached') && (has('--apply') || !has('--check', '--stat', '--numstat', '--summary')), appliedPatch)],
                        restore: [when(({ has }) => !has('--staged') || has('--worktree'), restoredPaths)],
                        stash: [when(Predicate.not(nested('list', 'show')), refuse('hides work', 'leave the work in the tree'))],
                        switch: [UNFORCED, when((invocation) => scoped(invocation)[0].some(({ names }) => names.includes('-C') || names.includes('--force-create')), refuse(...OVERWRITES, '-C'))],
                        'checkout-index': [refuse(...OVERWRITES, '-f', '--force', '-a', '--all')],
                        'update-index': [refuse('hides edits from git status', 'leave the index flags alone', '--assume-unchanged', '--skip-worktree')],
                    }),
                    when(
                        (invocation, { host }) => !Option.exists(repositoryOf(host, invocation), (repository) => !host.inside(repository, host.root)),
                        when(holds(ALIAS, '-c'), refuse(...ALIASES, '-c')),
                        when(holds(HOOKS_PATH, '-c'), refuse(...HOOKS, '-c')),
                        perSubcommand({
                            branch: [branchWrites],
                            commit: [respell({ drops: ['--no-verify'], notify: true }), when(({ has }) => has('--amend'), ownedHead), stamped],
                            'filter-branch': [refuse(...HISTORY)],
                            'filter-repo': [refuse(...HISTORY)],
                            tag: [refuse('deletes a tag', 'leave published tags in place', '-d', '--delete')],
                            'update-ref': [refuse('deletes a ref', 'leave the ref in place', '-d')],
                            bisect: [when(Predicate.not(nested('log', 'view', 'visualize', 'help')), refuse(MOVES, 'bisect in a clone under "$TMPDIR"'))],
                            checkout: [refuse(...BRANCHES, '-b', '--orphan', '--track'), movedHead],
                            config: [when(({ operands }) => operands.slice(0, -1).some(({ text }) => text.startsWith('alias.')), refuse(...ALIASES))],
                            merge: [when(({ has }) => !has(...UNWINDS, '--continue'), cleanMerge)],
                            pull: [when(({ has }) => !has('--ff-only') || has('--rebase', '-r', '--no-ff', '--ff'), refuse('merges or rebases onto the HEAD every session shares', 'run git pull --ff-only'))],
                            push: [when(Predicate.not(tagsAlone), UNFORCED, refuse(...REMOTE, '--delete', '--mirror', '--prune')), forcedRefs],
                            rebase: [when(({ has }) => !has(...UNWINDS), refuse('rewrites shared commits', 'merge or commit forward'))],
                            reflog: [when(nested('delete', 'drop', 'expire'), refuse('deletes entries', 'leave the reflog intact'))],
                            reset: [when(({ has }) => !has(...RESET_DISCARDS), resetTarget)],
                            switch: [refuse(...BRANCHES, '--create', '--orphan'), movedHead],
                        }),
                    ),
                ),
                perSubcommand({ worktree: [exportedTree] }),
            ],
        },
        patch: {
            flags: ['-d=', '--directory='],
            rules: [(invocation, { host }) => (Option.exists(folderOf(host, invocation, '-d', '--directory'), (folder) => host.inside(folder, host.root)) ? refusal('patch applies a diff outside git, run git apply') : [])],
        },
    },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Calling, closed, GIT, opened, Writing, wrote };
