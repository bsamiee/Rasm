import { Array, Effect, Function, Match, Option, Predicate, Result, Schema, String, Struct } from 'effect';
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
    Wrote: { agent: Schema.optionalKey(Schema.String), path: Schema.String, before: Schema.optionalKey(Schema.String), after: Schema.String },
});
const Line = Schema.fromJsonString(Entry);
const Output = Schema.Union([Schema.Struct({ filePath: Schema.String, oldString: Schema.String, newString: Schema.String, originalFile: Schema.NullOr(Schema.String), replaceAll: Schema.Boolean }), Schema.Struct({ filePath: Schema.String, content: Schema.String, originalFile: Schema.NullOr(Schema.String) })]);
const Writing = Schema.Struct({ session: Schema.String, agent: Schema.optionalKey(Schema.String), result: Schema.Unknown }).annotate({ description: "An Edit or Write call's structured result, recorded as the writing agent's change of that file's blob" });

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [LEDGER]

const markerOf = ({ session, agent }: Pick<Request, 'session' | 'agent'>) => `${manifest.name} ${[session, ...Option.toArray(Option.fromUndefinedOr(agent))].join('/')}`;

const located = (host: Host['Service'], folder: string) =>
    Effect.map(host.run(['git', '-C', folder, 'rev-parse', '--absolute-git-dir', '--show-toplevel', '--show-prefix']), ({ code, stdout }) => {
        const [gitdir = '', toplevel = '', prefix = ''] = stdout.split('\n');
        return Option.liftPredicate({ ledger: host.path.join(gitdir, manifest.name), toplevel, prefix }, () => code === 0);
    });

const entriesOf = Effect.fnUntraced(function* (host: Host['Service'], ledger: string) {
    const files = yield* Effect.orElseSucceed(host.fs.readDirectory(ledger), (): readonly string[] => []);
    const texts = yield* Effect.forEach(files, (file) => Effect.orElseSucceed(host.fs.readFileString(host.path.join(ledger, file)), () => ''));
    return Array.zip(files, texts).flatMap(([file, text]) => Array.getSomes(text.split('\n').map((line) => Schema.decodeOption(Line)(line))).map((entry) => [host.path.basename(file, LEDGER), entry] as const));
});

const afterOf = (output: typeof Output.Type): Option.Option<string> =>
    'content' in output
        ? Option.some(output.content)
        : Option.match(Option.fromNullOr(output.originalFile), {
              onNone: () => Option.some(output.newString),
              onSome: (original) =>
                  Option.map(
                      Option.liftPredicate(original.indexOf(output.oldString), (at) => at >= 0),
                      (at) => (output.replaceAll ? original.split(output.oldString).join(output.newString) : `${original.slice(0, at)}${output.newString}${original.slice(at + output.oldString.length)}`),
                  ),
          });

const blobOf = (oid: string) => Option.liftPredicate(oid, (held: string) => !ABSENT.test(held));

const changesOf = (listed: string) =>
    Array.chunksOf(listed.split('\0').filter(String.isNonEmpty), 2).map(([meta, path = '']) => {
        const [from = '', to = '', status = ''] = meta.split(' ').slice(2);
        return { path, from: blobOf(from), to: Option.filter(blobOf(to), () => status !== 'D'), kept: status !== 'D' };
    });

const appended = Effect.fnUntraced(function* (host: Host['Service'], ledger: string, session: string, entry: typeof Entry.Type) {
    const line = yield* Effect.orDie(Schema.encodeEffect(Line)(entry));
    yield* Effect.mapError(Effect.andThen(host.fs.makeDirectory(ledger, { recursive: true }), host.fs.writeFileString(host.path.join(ledger, `${session}${LEDGER}`), `${line}\n`, { flag: 'a' })), HostError.of(ledger));
});

const derived = (links: readonly { readonly before?: string; readonly after: string }[], from: Option.Option<string>, to: Option.Option<string>, seen: ReadonlySet<string> = new Set()): boolean =>
    Option.getOrUndefined(from) === Option.getOrUndefined(to) || Option.exists(to, (blob) => !seen.has(blob) && links.some(({ before, after }) => after === blob && derived(links, from, Option.fromUndefinedOr(before), seen.union(new Set([blob])))));

const foreign = Effect.fnUntraced(function* (host: Host['Service'], repository: string, { session, agent }: Pick<Request, 'session' | 'agent'>, pathspecs: readonly string[], source: Option.Option<string>, indexed: boolean) {
    const place = yield* located(host, repository);
    if (Option.isNone(place)) {
        return pathspecs;
    }
    const { ledger, toplevel } = place.value;
    const diffed = (...against: readonly string[]) => Effect.map(host.output(['git', '-C', repository, 'diff', '--raw', '-z', '--no-abbrev', '--no-renames', '--no-relative', ...against, '--', ...pathspecs]), changesOf);
    const [worktree, index, entries] = yield* Effect.all([diffed(...Option.toArray(source)), indexed ? diffed('--cached', ...Option.toArray(source)) : Effect.succeed([]), entriesOf(host, ledger)]);
    const present = worktree.filter(({ kept }) => kept);
    const hashed = present.length === 0 ? [] : (yield* host.output(['git', '-C', toplevel, 'hash-object', '--', ...present.map(({ path }) => path)])).split('\n');
    const blobs = new Map(present.map(({ path }, at) => [path, hashed[at]] as const));
    const links = Array.groupBy(
        entries.flatMap(([owner, entry]) => (Predicate.isTagged(entry, 'Wrote') && owner === session && entry.agent === agent ? [entry] : [])),
        Struct.get('path'),
    );
    const unowned = [...worktree.map(({ path, from }) => ({ path, from, to: Option.fromUndefinedOr(blobs.get(path)) })), ...index].filter(({ path, from, to }) => !derived(links[path] ?? [], from, to));
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

const inSession =
    (...rules: readonly Rule[]): Rule =>
    (invocation, call) =>
        Option.exists(repositoryOf(call.host, invocation), (repository) => !call.host.inside(repository, call.host.root)) ? [] : all(...rules)(invocation, call);

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

const wrote = Effect.fn('wrote')(function* ({ session, agent, result }: typeof Writing.Type) {
    const host = yield* Host;
    const output = Schema.decodeUnknownOption(Output)(result);
    const held = Option.all({ decoded: output, after: Option.flatMap(output, afterOf) });
    if (Option.isNone(held)) {
        return;
    }
    const { decoded, after } = held.value;
    const place = yield* located(host, host.path.dirname(decoded.filePath));
    if (Option.isNone(place)) {
        return;
    }
    const { ledger, toplevel, prefix } = place.value;
    const path = `${prefix}${host.path.basename(decoded.filePath)}`;
    const hashed = (stdin: string) => Effect.map(host.run(['git', '-C', toplevel, 'hash-object', '--stdin', `--path=${path}`], { stdin }), ({ stdout }) => stdout.trim());
    const [before, blob] = yield* Effect.all([Effect.transposeOption(Option.map(Option.fromNullOr(decoded.originalFile), hashed)), hashed(after)]);
    yield* appended(host, ledger, session, Entry.cases.Wrote.make({ path, after: blob, ...Option.match(before, { onNone: () => ({}), onSome: (hash) => ({ before: hash }) }), ...(agent === undefined ? {} : { agent }) }));
});

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
                        switch: [UNFORCED, refuse(...OVERWRITES, '-C')],
                        'checkout-index': [refuse(...OVERWRITES, '-f', '--force', '-a', '--all')],
                        'update-index': [refuse('hides edits from git status', 'leave the index flags alone', '--assume-unchanged', '--skip-worktree')],
                    }),
                    inSession(
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

export { GIT, Writing, wrote };
