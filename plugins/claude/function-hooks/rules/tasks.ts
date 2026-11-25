import { tasks as graphed } from '@rasm/nx/graph';
import { Array, Effect, Equal, FileSystem, Match, Option, Path, Predicate, Record, Schema, String, Struct } from 'effect';
import { RedirOperator, type Stmt, walk } from 'sh-syntax';
import { type Call, type Editor, feeds, type Invocation, nearest, ran, type Script, type Table, type Verdict, type Word } from '#/rules/rule';

// --- [TYPES] ---------------------------------------------------------------------------

type Graph = Effect.Success<ReturnType<typeof graphed>> & { readonly root: string };
type Row = Graph['tasks'] extends ReadonlyMap<string, infer R> ? R : never;
type Steps = (typeof Hooks.Type)['steps'];

// --- [CONSTANTS] -----------------------------------------------------------------------

const shared = 'Leave it running; nx reset --only-cache empties the cache alone.';
const launchers: ReadonlySet<string> = new Set(['', 'pnpm exec', 'npx', 'uv run', 'mise exec', 'pixi run', 'npm exec']);
const wrapped: ReadonlySet<string> = new Set([...launchers].flatMap((launcher) => launcher.split(' ').slice(0, 1).filter(String.isNonEmpty)));
const bounded: ReadonlySet<string> = new Set([...wrapped, 'timeout']);
const plain = /^[\w./:@%+=,-]+$/u;
const ending: ReadonlySet<string> = new Set(['sync', 'sync:check', 'conformance:check']);
const operations: Readonly<Record<string, { readonly aggregate: string; readonly args: readonly string[]; readonly applies: boolean }>> = {
    up: { aggregate: 'deploy', args: [], applies: true },
    preview: { aggregate: 'dry-run', args: [], applies: false },
    refresh: { aggregate: 'dry-run', args: [], applies: false },
    destroy: { aggregate: 'deploy', args: ['--operation=destroy'], applies: true },
};
const pulumied: ReadonlySet<string> = new Set(['--stack', '-s', '--yes', '-y', '--skip-preview', '--non-interactive', '--cwd', '-C']);

// --- [MODELS] --------------------------------------------------------------------------

const Argv = Schema.Struct({ command: Schema.Union([Schema.String, Schema.Struct({ argv: Schema.Array(Schema.String) })]) });
const Hooks = Schema.fromJsonString(Schema.Struct({ steps: Schema.Record(Schema.String, Schema.Struct({ check: Schema.optionalKey(Schema.NullOr(Argv)), check_diff: Schema.optionalKey(Schema.NullOr(Argv)), check_list_files: Schema.optionalKey(Schema.NullOr(Argv)), fix: Schema.optionalKey(Schema.NullOr(Argv)) })) }));
const Plan = Schema.fromJsonString(Schema.Struct({ steps: Schema.Array(Schema.Struct({ status: Schema.String, fileCount: Schema.optionalKey(Schema.Finite) })) }));
const Metadata = Schema.fromJsonString(Schema.Struct({ workspace_root: Schema.String, packages: Schema.Array(Schema.Struct({ name: Schema.String, manifest_path: Schema.String })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const workspace = Effect.fnUntraced(function* (cwd: string) {
    const root = yield* nearest(cwd, 'nx.json');
    const graph = yield* Option.match(root, { onNone: () => Effect.succeedNone, onSome: (folder) => Effect.option(graphed(folder)) });
    return Option.map(Option.all([root, graph]), ([folder, held]): Graph => ({ ...held, root: folder }));
});
const addressed = (invocation: Invocation, graph: Graph): readonly Row[] => {
    const [first = '', second = ''] = [Option.getOrUndefined(invocation.operand()), Option.getOrUndefined(invocation.operand(1))];
    const listed = (flag: string) =>
        invocation
            .values(flag)
            .flatMap((value) => Option.toArray(value.text))
            .flatMap((value) => value.split(','));
    const asked = listed('-p');
    const projects = Array.isReadonlyArrayEmpty(asked) ? Array.dedupe(Array.fromIterable(graph.tasks.values()).map(({ project }) => project)) : asked;
    const keys = Match.value(first).pipe(
        Match.when('run', () => [second.split(':').slice(0, 2).join(':')]),
        Match.when(Match.is('run-many', 'affected'), () => Array.cartesianWith(projects, listed('-t'), (project, target) => `${project}:${target}`)),
        Match.orElse(() => [`${second}:${first}`]),
    );
    return keys.flatMap((key) => Array.fromNullishOr(graph.tasks.get(key)));
};
const members = (graph: Graph, row: Row): readonly Row[] => [row, ...(row.runs._tag === 'Aggregate' ? row.runs.members.flatMap((member) => Array.fromNullishOr(graph.tasks.get(`${row.project}:${member}`))).flatMap((held) => members(graph, held)) : [])];
const aggregate = (graph: Graph, row: Row) => Array.findFirst(Array.fromIterable(graph.tasks.values()), (held) => held.project === row.project && Option.isNone(held.configuration) && held.runs._tag === 'Aggregate' && held.runs.members.includes(row.target));
const launched = (invocation: Invocation): Option.Option<readonly [Word, Word]> =>
    Option.flatMap(Option.all([invocation.node, Option.flatMap(Array.head(invocation.words), ({ node }) => node)]), ([stmt, word]) => {
        const args = stmt.cmd?.type === 'CallExpr' ? stmt.cmd.args : [];
        return Option.all([Array.head(args), Array.get(args, args.indexOf(word) - 1)]);
    });
const here = (invocation: Invocation, cwd: string): string => Option.getOrElse(invocation.place._tag === 'Local' ? invocation.place.cwd.text : Option.none(), () => cwd);
const placed = Effect.fnUntraced(function* (invocation: Invocation, cwd: string, root: string, words: readonly Invocation['words'][number][] = invocation.operands.map(({ word }) => word)) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const base = here(invocation, cwd);
    const paths = words.map((word) => ({ word, text: word.text, path: Option.map(word.text, (text) => path.resolve(base, text)) }));
    const present = yield* Effect.forEach(paths, ({ path: file }) => Option.match(file, { onNone: () => Effect.succeed(false), onSome: (held) => Effect.orElseSucceed(fs.exists(held), () => false) }));
    return Array.zipWith(paths, present, ({ word, text, path: file }, exists) => ({ word, text, file: Option.filter(file, (held) => exists && (held === root || held.startsWith(`${root}/`))) }));
});
const helps = (graph: Graph) =>
    Array.fromIterable(graph.tasks.values())
        .flatMap((row) => {
            const words = Option.getOrElse(
                Option.filter(
                    Option.map(row.help, (help) => help.split(' ')),
                    (all) => all.every((word) => plain.test(word)),
                ),
                Array.empty<string>,
            );
            return Option.isNone(row.configuration)
                ? words.flatMap((program, at) =>
                      launchers.has(words.slice(0, at).join(' '))
                          ? [
                                {
                                    row,
                                    program,
                                    stated: words
                                        .slice(at + 1)
                                        .filter((word) => !word.startsWith('-'))
                                        .join(' '),
                                },
                            ]
                          : [],
                  )
                : [];
        })
        .map((entry) => {
            const lines = commanded(entry);
            return { ...entry, lines, configured: new Set(lines.flatMap((line) => [...line])) };
        });
const arrays = (value: unknown): readonly (readonly string[])[] =>
    Match.value(value).pipe(
        Match.when(Predicate.isString, (text) => [text.split(/\s+/u)]),
        Match.when(
            (held: unknown): held is readonly string[] => Array.isArray(held) && held.every(Predicate.isString),
            (held) => [held],
        ),
        Match.when(Predicate.isObjectOrArray, (held) => Object.values(held).flatMap(arrays)),
        Match.orElse(() => []),
    );
const commanded = ({ row, program, stated }: { readonly row: Row; readonly program: string; readonly stated: string }): readonly ReadonlySet<string>[] => {
    const anchor = stated.split(' ')[0] ?? '';
    const lines = arrays(row.options).flatMap((line) => {
        const at = line.findIndex((word) => word === program || (anchor !== '' && word === anchor));
        return at === -1 ? [] : [new Set(line.slice(line[at] === program ? at + 1 : at))];
    });
    return Array.isReadonlyArrayEmpty(lines) ? [new Set<string>()] : lines;
};
const based = (graph: Graph, path: Path.Path, row: Row): string => path.join(graph.root, row.runs._tag === 'Commands' ? row.runs.cwd : Option.getOrElse(Option.filter(Option.some(row.options['cwd']), Predicate.isString), () => row.root));
const extent = (invocation: Invocation): Option.Option<readonly [Word, Word]> => (invocation.within.length === 0 ? invocation.span : Option.all([Option.map(launched(invocation), ([first]) => first), Option.map(invocation.span, ([, last]) => last)]));
const routable = (call: Call, wrappers: ReadonlySet<string>) => (invocation: Invocation) =>
    call.action.run !== 'monitored' &&
    invocation.inner.length === 0 &&
    Option.isNone(invocation.repeated) &&
    invocation.assigns.length === 0 &&
    invocation.rest.length === 0 &&
    !invocation.runs('nx', 'hk') &&
    !Option.exists(invocation.stage, ({ index, count }) => index < count - 1) &&
    invocation.within.every((within) => within._tag === 'Wrapper' && wrappers.has(within.name));
const routed = (graph: Graph, picks: Array.NonEmptyReadonlyArray<Row>, forwarded: readonly string[], everywhere: boolean): Array.NonEmptyReadonlyArray<string> => {
    const [first] = picks;
    const projects = everywhere ? [] : ['-p', picks.map(({ project }) => project).join(',')];
    const sole = Option.filter(aggregate(graph, first), ({ runs }) => runs._tag === 'Aggregate' && runs.members.length === 1 && Array.isReadonlyArrayEmpty(forwarded));
    const forward = Array.isReadonlyArrayEmpty(forwarded) ? [] : ['--', ...forwarded];
    return picks.length > 1 ? Array.make('nx', 'run-many', '-t', first.target, ...projects) : Option.match(sole, { onNone: () => Array.make('nx', 'run', `${first.project}:${first.target}`, ...forward), onSome: ({ target }) => Array.make('nx', target, first.project) });
};
const route = Effect.fnUntraced(function* (invocation: Invocation, graph: Graph, helped: ReturnType<typeof helps>, cwd: string) {
    const path = yield* Path.Path;
    const operands = yield* placed(invocation, cwd, graph.root);
    const program = Option.getOrUndefined(invocation.name);
    const spoken = [...invocation.path, ...operands.flatMap(({ text, file }) => (Option.isSome(file) ? [] : Option.toArray(text)))].join(' ');
    const files = operands.flatMap(({ file }) => Option.toArray(file));
    const valued = yield* Effect.forEach(invocation.flags, (bound) => placed(invocation, cwd, graph.root, bound.values));
    const flags = Array.zipWith(invocation.flags, valued, (bound, values) => ({ spelling: bound.spelled.text, words: Option.all([bound.spelled, ...bound.values.filter(({ node }) => Option.isSome(node))].map(Struct.get('text'))), paths: Array.getSomes(values.map(({ file }) => file)) }));
    const located = flags.flatMap(({ paths }) => paths);
    const unplaced = Array.isReadonlyArrayEmpty(files) && Array.isReadonlyArrayEmpty(located);
    const matching = helped.filter(({ program: helping, stated }) => helping === program && stated === spoken);
    const direct = Array.dedupe(unplaced ? Option.toArray(graph.owner(cwd)) : [...files, ...located].flatMap((file) => Option.toArray(graph.owner(file))));
    const nested = unplaced && !matching.some(({ row }) => row.cached && direct.includes(row.project)) ? Array.dedupe(matching.filter(({ row }) => row.cached && `${path.join(graph.root, row.root)}/`.startsWith(`${cwd}/`)).map(({ row }) => row.project)) : [];
    const widened = Array.isReadonlyArrayNonEmpty(nested);
    const owners = widened ? nested : direct;
    const peers = matching.filter(({ row }) => owners.includes(row.project));
    const asked = new Set(flags.flatMap(({ spelling }) => Option.toArray(spelling)));
    const written = new Set(flags.flatMap(({ words }) => Option.getOrElse(words, Array.empty<string>)));
    const fitting = (held: (typeof peers)[number]) => {
        const others = peers.filter(({ row }) => row.project === held.row.project && row.target !== held.row.target);
        const foreign = flags.filter(({ words }) => !Option.getOrElse(words, Array.empty<string>).every((word) => held.configured.has(word)));
        const base = based(graph, path, held.row);
        const consumed = foreign.every(({ paths }) => paths.every((file) => file === base || file === path.join(graph.root, held.row.root) || held.configured.has(path.relative(base, file))));
        const excluded = [...asked].some((word) => !held.configured.has(word) && others.some(({ configured }) => configured.has(word)));
        const writers = others.filter(({ row }) => !row.cached);
        const loose = new Set(writers.flatMap(({ configured }) => [...configured]));
        const same = held.lines.some((line) => [...line].every((word) => !word.startsWith('-') || written.has(word) || Array.isReadonlyArrayEmpty(writers) || loose.has(word)));
        return held.row.cached && flags.every(({ words }) => Option.isSome(words)) && consumed && !excluded && same;
    };
    const fit = peers.filter(fitting);
    const rows = fit.map(({ row }) => row);
    const absorbed = Array.match(fit, { onEmpty: () => new Set<string>(), onNonEmpty: ([first, ...others]) => others.reduce((common, { configured }) => common.intersection(configured), first.configured) });
    const picks = owners.flatMap((owner) => (rows.filter(({ project }) => project === owner).length === 1 ? rows.filter(({ project }) => project === owner) : []));
    const inner = files.filter((file) => !picks.some(({ root }) => file === path.join(graph.root, root)));
    const narrowed = Array.isReadonlyArrayNonEmpty(inner) && (picks.length !== 1 || widened);
    const forwarded = [
        ...flags.flatMap(({ words, paths }) => (Array.isReadonlyArrayNonEmpty(paths) ? [] : Option.getOrElse(words, Array.empty<string>))).filter((word) => !absorbed.has(word)),
        ...Option.toArray(Array.head(picks)).flatMap((pick) => inner.map((file) => path.relative(based(graph, path, pick), file) || '.')),
    ];
    const several = Array.findFirst(owners, (owner) => rows.filter(({ project }) => project === owner).length > 1);
    const ready = Option.filter(
        Option.all([Array.match(picks, { onEmpty: Option.none, onNonEmpty: Option.some }), extent(invocation)]),
        ([held]) => (widened || held.length === owners.length) && held.every(({ target: name }) => name === held[0].target) && !narrowed && ((held.length === 1 && !widened) || Array.isReadonlyArrayEmpty(forwarded)),
    );
    return Match.value({ several: Option.getOrUndefined(several), ready: Option.getOrUndefined(ready) }).pipe(
        Match.when({ several: Match.string }, ({ several: owner }) => [
            {
                deny: `${invocation.label} matches several targets of ${owner}.`,
                advice: `Run one of: ${rows
                    .filter(({ project }) => project === owner)
                    .map((row) => `nx run ${row.project}:${row.target}`)
                    .join(', ')}.`,
            },
        ]),
        Match.when({ ready: Match.defined }, ({ ready: [chosen, words] }) => [{ edit: (editor: Editor) => [editor.replace(words, routed(graph, chosen, forwarded, widened && cwd === graph.root))] }]),
        Match.orElse((): readonly Verdict[] => []),
    );
});
const looping = (script: Script, invocation: Invocation): Option.Option<Stmt> => {
    const loops: Stmt[] = [];
    walk(script.syntax, (node) => {
        loops.push(...(node.type === 'Stmt' && node.cmd?.type === 'ForClause' && node.cmd.loop?.type === 'WordIter' && node.redirs.length === 0 && Option.exists(invocation.repeated, ({ start }) => node.cmd !== null && script.located(node.cmd.start) === start) ? [node] : []));
    });
    return Option.filter(Array.head(loops), ({ cmd }) => {
        const [body, ...others] = cmd?.type === 'ForClause' ? cmd.do : [];
        return others.length === 0 && body?.redirs.length === 0 && body.cmd?.type === 'CallExpr' && body.cmd.assigns.length === 0 && !body.background && !body.negated && Option.exists(invocation.node, (stmt) => stmt === body);
    });
};
const steps = Effect.fnUntraced(function* (root: string) {
    const path = yield* Path.Path;
    const { printed, code } = yield* ran(['pkl', 'eval', '--format', 'json', path.join(root, '.config/hk.pkl')], root);
    return code === 0
        ? Option.getOrElse(
              Option.map(Schema.decodeOption(Hooks)(printed), ({ steps: held }) => held),
              (): Steps => ({}),
          )
        : {};
});
const argv = (mode: typeof Argv.Type | null | undefined): readonly string[] => (mode === null || mode === undefined || typeof mode.command === 'string' ? [] : mode.command.argv);
const fits = (configured: Steps, program: string, asked: readonly string[]) =>
    Array.dedupeWith(
        Record.toEntries(configured).flatMap(([step, modes]) => {
            const checking = [argv(modes.check), argv(modes.check_diff), argv(modes.check_list_files)];
            const sides = [...checking.map((own) => ({ mode: 'check' as const, own, other: argv(modes.fix) })), { mode: 'fix' as const, own: argv(modes.fix), other: checking.flat() }];
            return sides.flatMap(({ mode, own, other }) => {
                const words = own.slice(1).filter((word) => word !== '{{files}}');
                const distinct = other.length === 0 ? [] : words.filter((word) => !other.includes(word));
                return own[0] === program && own.includes('{{files}}') && asked.every((word) => words.includes(word)) && distinct.every((word) => asked.includes(word)) ? [{ step, mode }] : [];
            });
        }),
        (left, right) => left.step === right.step && left.mode === right.mode,
    );
const stepped = Effect.fnUntraced(function* (invocation: Invocation, configured: Steps, root: string, cwd: string) {
    const path = yield* Path.Path;
    const operands = yield* placed(invocation, cwd, root, invocation.words.slice(1));
    const files = operands.flatMap(({ file }) => Option.toArray(file));
    const asked = Option.all(
        invocation.words
            .slice(1)
            .filter((written) => !operands.some(({ word, file }) => word === written && Option.isSome(file)))
            .map(Struct.get('text')),
    );
    const fitting = Option.match(asked, {
        onNone: () => [],
        onSome: (words) =>
            fits(
                configured,
                Option.getOrElse(invocation.name, () => ''),
                words,
            ),
    });
    const relative = files.map((file) => path.relative(cwd, file) || '.');
    const fit = Option.filter(Array.head(fitting), () => fitting.length === 1 && Array.isReadonlyArrayNonEmpty(files));
    const plan = yield* Option.match(fit, { onNone: () => Effect.succeed({ printed: '', code: 1 }), onSome: ({ mode, step }) => ran(Array.make('hk', mode, '--step', step, '--plan', '--json', ...relative), cwd) });
    const included = Option.exists(Schema.decodeOption(Plan)(plan.printed), ({ steps: planned }) => planned.some(({ status, fileCount }) => status === 'included' && fileCount === files.length));
    return Option.match(
        Option.filter(Option.all([fit, extent(invocation)]), () => included),
        {
            onNone: (): readonly Verdict[] => [],
            onSome: ([{ step, mode }, words]) => [{ edit: (editor: Editor) => [editor.replace(words, Array.make('hk', mode, '--step', step, ...relative))] }],
        },
    );
});
const crates = Effect.fnUntraced(function* (invocation: Invocation, cwd: string) {
    const path = yield* Path.Path;
    const packages = invocation.values('-p', '--package').flatMap((value) => Option.toArray(value.text));
    const metadata = Schema.decodeOption(Metadata)((yield* ran(['cargo', 'metadata', '--no-deps', '--format-version', '1'], cwd)).printed);
    const listed = Option.getOrElse(
        Option.map(metadata, ({ packages: held }) => held.map(({ name, manifest_path }) => ({ name, folder: path.dirname(manifest_path) }))),
        Array.empty<{ readonly name: string; readonly folder: string }>,
    );
    const owning = Array.findFirst(
        listed.toSorted((left, right) => right.folder.length - left.folder.length),
        ({ folder }) => cwd === folder || cwd.startsWith(`${folder}/`),
    );
    const chosen = Option.all(
        packages.map((name) =>
            Option.map(
                Array.findFirst(listed, (held) => held.name === name),
                ({ folder }) => path.relative(cwd, folder) || '.',
            ),
        ),
    );
    return Match.value({ all: invocation.has('--all') || Option.exists(metadata, ({ workspace_root }) => cwd === workspace_root && Array.isReadonlyArrayEmpty(packages)), named: Array.isReadonlyArrayNonEmpty(packages) }).pipe(
        Match.when({ all: true }, () => Option.some(['--all'])),
        Match.when({ named: true }, () => chosen),
        Match.orElse(() => Option.map(owning, ({ folder }) => [path.relative(cwd, folder) || '.'])),
    );
});

const tasks: Table = {
    'worktree.isolated': {
        action: 'Isolate',
        decide: ({ worker }) => Option.match(worker, { onSome: () => [{ apart: false as const }], onNone: () => [{ deny: 'A worktree is a second checkout that every build repeats and the guards of this checkout never see.', advice: 'Work in place.' }] }),
    },
    'nx.publish': { program: 'nx', decide: (invocation) => (invocation.words.some((written) => written.test(/(?:^|[:,=])nx-release-publish(?:,|$)/u)) ? [{ deny: 'nx-release-publish publishes dependencies for real.', advice: 'Run nx release.' }] : []) },
    'nx.daemon': {
        program: 'nx',
        decide: Effect.fnUntraced(function* (invocation, { script }) {
            const verb = Option.getOrElse(invocation.operand(), () => '');
            const stops = (verb === 'daemon' && invocation.has('--stop')) || (verb === 'reset' && invocation.has('--only-daemon'));
            const later = (yield* script.invocations).flatMap(Struct.get('leaves')).some((other) => other.start > invocation.end && other.runs('nx') && !Option.exists(other.operand(), (next) => next === 'daemon' || next === 'reset'));
            const spoken = Option.getOrElse(
                Option.map(Option.all(invocation.words.map(Struct.get('text'))), (words) => words.join(' ')),
                () => `nx ${verb}`,
            );
            return Match.value({ stops, stmt: Option.getOrUndefined(Option.filter(invocation.node, () => later)) }).pipe(
                Match.when({ stops: false }, () => []),
                Match.when({ stmt: Match.defined }, ({ stmt }) => [{ edit: (editor: Editor) => [editor.remove(stmt)] }, { note: `${spoken} left the call, as every session shares the daemon.`, once: 'call' as const }]),
                Match.orElse(() => [{ deny: `${spoken} stops the daemon every session shares.`, advice: shared }]),
            );
        }),
    },
    'nx.reset': {
        program: 'nx',
        path: ['reset'],
        decide: (invocation) => (invocation.has('--only-workspace-data') || !invocation.has('--only-cache', '--only-daemon') ? [{ deny: 'nx reset stops the daemon and deletes the workspace data every session shares.', advice: 'Run nx reset --only-cache.' }] : []),
    },
    'nx.skip-sync': {
        program: 'nx',
        decide: Effect.fnUntraced(function* (invocation, { shell, script }) {
            const sync = invocation.has('--skip-sync') ? Option.some(yield* shell.script(script.dialect)`nx sync`) : Option.none();
            return Option.match(Option.all([sync, invocation.node]), {
                onNone: () => [],
                onSome: ([synced, stmt]) => [{ drop: ['--skip-sync'] }, { edit: (editor: Editor) => [editor.precede(synced, '&&', stmt)] }, { note: '--skip-sync became nx sync before the command, as a skipped sync leaves tsconfig references stale.', once: 'agent' as const }],
            });
        }),
    },
    'nx.daemonless': {
        program: 'nx',
        decide: (invocation) => {
            const assigns = Option.toArray(invocation.node).flatMap(({ cmd }) => (cmd?.type === 'CallExpr' ? cmd.assigns : []));
            return assigns.filter(({ name }) => name?.value === 'NX_DAEMON').map((assign) => ({ edit: (editor: Editor) => [editor.remove(assign)] }));
        },
    },
    'nx.launcher': {
        program: 'nx',
        decide: (invocation) => (Array.isReadonlyArrayNonEmpty(invocation.within) && invocation.within.every((within) => within._tag === 'Wrapper' && wrapped.has(within.name)) ? Option.toArray(launched(invocation)).map((pair) => ({ edit: (editor: Editor) => [editor.remove(pair)] })) : []),
    },
    'nx.quieted': {
        program: 'nx',
        decide: Effect.fnUntraced(function* (invocation, { script }) {
            const parsed = (yield* script.invocations).flatMap(Struct.get('leaves')).some((next) => next.runs('jq') && feeds(invocation, next));
            const redirects = Option.getOrElse(
                Option.map(invocation.node, ({ redirs }) => redirs),
                Array.empty,
            );
            const targets = redirects.map((redirect) => {
                const expansion = Option.map(Option.fromNullishOr(redirect.word), (word) => script.expand(word)[0]);
                return { redirect, target: Option.flatMap(expansion, (held) => (held._tag === 'Expanded' ? held.text : Option.none())) };
            });
            const nulled = targets.filter(({ target: aimed }) => Option.contains(aimed, '/dev/null'));
            const silenced = nulled.some(({ redirect }) => redirect.n === null && redirect.op === RedirOperator.RdrOut);
            const merged = targets.filter(({ redirect, target: aimed }) => (parsed || silenced) && Option.contains(aimed, '1') && redirect.n?.value === '2' && redirect.op === RedirOperator.DplOut);
            const hidden = [...nulled, ...merged];
            return Option.exists(invocation.operand(), (verb) => verb === 'show' || verb === 'graph') && Array.isReadonlyArrayNonEmpty(hidden) ? [{ edit: (editor: Editor) => hidden.map(({ redirect }) => editor.remove(redirect)) }] : [];
        }),
    },
    'nx.bounded': {
        program: 'nx',
        decide: Effect.fnUntraced(function* (invocation, { cwd: session }) {
            const cwd = here(invocation, session);
            const timed = invocation.within.length === 1 && invocation.within.every((within) => within._tag === 'Wrapper' && within.name === 'timeout');
            const graph = timed ? yield* workspace(cwd) : Option.none();
            const rows = Option.getOrElse(
                Option.map(graph, (held) => addressed(invocation, held).flatMap((row) => members(held, row))),
                Array.empty,
            );
            const verb = Option.getOrElse(invocation.operand(), () => '');
            const printed = (verb === 'show' && !invocation.has('--web')) || (verb === 'graph' && invocation.words.some((written) => written.test(/^--(?:file|print)(?:=|$)/u)));
            const finite = printed || ending.has(verb) || (Array.isReadonlyArrayNonEmpty(rows) && rows.every(({ continuous }) => !continuous));
            return timed && finite
                ? Option.toArray(launched(invocation)).flatMap((pair) => [
                      { edit: (editor: Editor) => [editor.remove(pair)] },
                      { note: 'timeout before nx left the call, as its kill loses unfinished tasks of a run that ends by itself; a foreground call past its timeout moves to the background and notifies, and TaskStop ends a hung run.', once: 'agent' as const },
                  ])
                : [];
        }),
    },
    'nx.aggregate': {
        program: 'nx',
        decide: Effect.fnUntraced(function* (invocation, { cwd: session }) {
            const cwd = here(invocation, session);
            const graph = invocation.rest.length > 0 ? yield* workspace(cwd) : Option.none();
            const rows = Option.getOrElse(
                Option.map(graph, (held) => addressed(invocation, held).filter(({ runs }) => runs._tag === 'Aggregate')),
                Array.empty,
            );
            const tools = Array.dedupe(rows.flatMap(({ runs }) => (runs._tag === 'Aggregate' ? runs.members : [])));
            const many = Option.contains(invocation.operand(), 'run-many') || Option.contains(invocation.operand(), 'affected');
            const pointed = Option.flatMap(many ? Array.head(invocation.values('-t')) : Option.none(), ({ node }) => node);
            const address = Option.all([Option.flatMap(Array.get(invocation.operands, 0), ({ word }) => word.node), Option.flatMap(Array.get(invocation.operands, 1), ({ word }) => word.node)]);
            const sole = rows.every(({ runs }) => runs._tag === 'Aggregate' && runs.members.length === 1) ? Array.head(tools) : Option.none();
            return Option.match(Array.head(rows), {
                onNone: () => [],
                onSome: ({ project, target: name }) =>
                    Match.value({ tool: Option.getOrUndefined(sole), named: Option.getOrUndefined(pointed), address: Option.getOrUndefined(address) }).pipe(
                        Match.when({ tool: Match.string, named: Match.defined }, ({ tool, named: word }) => [{ edit: (editor: Editor) => [editor.replace(word, Array.of(tool))] }]),
                        Match.when({ tool: Match.string, address: Match.defined }, ({ tool, address: words }) => (many ? [] : [{ edit: (editor: Editor) => [editor.replace(words, Array.make('run', `${project}:${tool}`))] }])),
                        Match.orElse(() => [{ deny: `nx ${name} runs ${tools.length} tools, so arguments after -- reach none of them.`, advice: `Pass them to one of: ${tools.map((tool) => `nx run ${project}:${tool}`).join(', ')}.` }]),
                    ),
            });
        }),
    },
    'nx.loop': {
        program: 'nx',
        decide: Effect.fnUntraced(function* (invocation, { script, shell }) {
            const found = yield* script.invocations;
            const cycle = looping(script, invocation);
            const looped = found.flatMap(Struct.get('leaves')).filter((other) => other.runs('nx') && Option.isSome(invocation.repeated) && Equal.equals(other.repeated, invocation.repeated));
            const spelled = Option.getOrElse(Option.all(looped.map((other) => Option.all(other.words.map(Struct.get('text'))))), (): readonly (readonly string[])[] => []);
            const [first = [], ...rest] = spelled;
            const varying = first.flatMap((word, at) => (rest.some((other) => other[at] !== word) ? [at] : []));
            const items = Array.dedupe(spelled.map((words) => words[varying[0] ?? 0] ?? ''));
            const verb = first[1] ?? '';
            const plan = Match.value({ verb, at: varying.length === 1 && rest.every((other) => other.length === first.length) && !['show', 'graph'].includes(verb) && !first.includes('--graph') ? varying[0] : undefined }).pipe(
                Match.when({ at: Match.undefined }, () => Option.none()),
                Match.when({ verb: 'run', at: 2 }, () => Option.some({ targets: Array.dedupe(items.map((item) => item.split(':')[1] ?? '')), projects: Array.dedupe(items.map((item) => item.split(':')[0] ?? '')) })),
                Match.when({ verb: 'run' }, () => Option.none()),
                Match.when({ at: 2 }, () => Option.some({ targets: [verb], projects: items })),
                Match.when({ at: 1 }, () => Option.some({ targets: items, projects: [first[2] ?? ''] })),
                Match.orElse(() => Option.none()),
            );
            const product = Option.filter(Option.all([plan, cycle]), ([{ targets, projects }]) => invocation === looped[0] && targets.length * projects.length === items.length);
            const written = yield* Effect.forEach(Option.toArray(product), ([{ targets, projects }, stmt]) => Effect.map(shell.script(script.dialect)`nx run-many -t ${targets.join(',')} -p ${projects.join(',')} ${first.slice(3)}`, (built) => [built, stmt] as const));
            return written.flatMap(([built, stmt]) => [{ edit: (editor: Editor) => [editor.command(stmt, built)] }, { note: 'A loop over nx became one nx run-many, which shares the cache and parallelism of one run.', once: 'agent' as const }]);
        }),
    },
    'nx.target': {
        action: 'Shell',
        decide: Effect.fnUntraced(function* (script, call) {
            const candidates = (yield* script.invocations).flatMap(Struct.get('leaves')).filter(routable(call, bounded));
            const verdicts = yield* Effect.forEach(candidates, (invocation) => Effect.flatMap(workspace(here(invocation, call.cwd)), Option.match({ onNone: () => Effect.succeed([]), onSome: (graph) => route(invocation, graph, helps(graph), here(invocation, call.cwd)) })));
            return verdicts.flat();
        }),
    },
    'hk.step': {
        action: 'Shell',
        decide: Effect.fnUntraced(function* (script, call) {
            const candidates = (yield* script.invocations)
                .flatMap(Struct.get('leaves'))
                .filter(
                    (invocation) =>
                        call.action.run !== 'monitored' &&
                        invocation.inner.length === 0 &&
                        Option.isNone(invocation.repeated) &&
                        invocation.assigns.length === 0 &&
                        Option.isNone(invocation.stage) &&
                        invocation.within.every((within) => within._tag === 'Wrapper' && wrapped.has(within.name)) &&
                        !invocation.runs('hk', 'nx'),
                );
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const root = Array.isReadonlyArrayNonEmpty(candidates) ? yield* nearest(call.cwd, '.git') : Option.none();
            const declared = yield* Option.match(root, { onNone: () => Effect.succeed(''), onSome: (top) => Effect.orElseSucceed(fs.readFileString(path.join(top, '.config/hk.pkl')), () => '') });
            const listed = candidates.filter((invocation) => Option.exists(invocation.name, (name) => declared.includes(`"${name}"`)));
            const configured = Option.isSome(root) && Array.isReadonlyArrayNonEmpty(listed) ? yield* steps(Option.getOrElse(root, () => call.cwd)) : {};
            const verdicts = yield* Effect.forEach(
                Option.toArray(root).flatMap((held) => listed.map((invocation) => [invocation, held] as const)),
                ([invocation, top]) => stepped(invocation, configured, top, here(invocation, call.cwd)),
            );
            return verdicts.flat();
        }),
    },
    'cargo.fmt': {
        program: 'cargo',
        aliases: ['cargo-fmt'],
        decide: Effect.fnUntraced(function* (invocation, { cwd: session }) {
            const cwd = here(invocation, session);
            const formatting = invocation.runs('cargo-fmt') || invocation.path[0] === 'fmt' || Option.contains(invocation.operand(), 'fmt');
            const root = formatting ? yield* nearest(cwd, '.git') : Option.none();
            const mode = invocation.has('--check') ? 'check' : 'fix';
            const configured = Option.isSome(root) ? yield* steps(Option.getOrElse(root, () => cwd)) : {};
            const step = Array.findFirst(Record.toEntries(configured), ([, modes]) => argv(modes[mode])[0] === 'rustfmt');
            const folders = Option.isSome(step) ? yield* crates(invocation, cwd) : Option.none();
            const known = invocation.flags.every(({ flag }) => flag.spellings.some((spelling) => ['--check', '-p', '--package', '--all'].includes(spelling))) && invocation.rest.length === 0 && invocation.operands.length <= 1;
            return Match.value({ step: Option.getOrUndefined(step), routed: Option.getOrUndefined(Option.all([step, folders, extent(invocation)])) }).pipe(
                Match.when({ routed: Match.defined }, ({ routed: [[name], targets, words] }) =>
                    known ? [{ edit: (editor: Editor) => [editor.replace(words, Array.make('hk', mode, '--step', name, ...targets))] }] : [{ deny: 'cargo fmt option has no hk form.', advice: `Run hk ${mode} --step ${name} ${targets.join(' ')}.` }],
                ),
                Match.when({ step: Match.defined }, () => [{ deny: 'cargo fmt resolves no crate here.', advice: 'Pass --package <crate>, or run it in the crate folder.' }]),
                Match.orElse((): readonly Verdict[] => []),
            );
        }),
    },
    'pulumi.deploy': {
        program: 'pulumi',
        decide: Effect.fnUntraced(function* (invocation, { cwd: session }) {
            const cwd = here(invocation, session);
            const verb = Option.getOrElse(
                Option.orElse(Array.head(invocation.path), () => invocation.operand()),
                () => '',
            );
            const operation = Option.fromUndefinedOr(operations[verb]);
            const graph = Option.isSome(operation) ? yield* workspace(cwd) : Option.none();
            return Option.match(Option.all([operation, graph]), {
                onNone: () => [],
                onSome: ([{ aggregate: wanted, args, applies }, held]) => {
                    const stage = `${wanted}--pulumi`;
                    const deployable = Array.dedupe(
                        Array.fromIterable(held.tasks.values())
                            .filter((row) => row.target === stage)
                            .map(({ project }) => project),
                    );
                    const project = Option.getOrElse(
                        Option.orElse(
                            Option.filter(held.owner(cwd), (owner) => deployable.includes(owner)),
                            () => Option.filter(Array.head(deployable), () => deployable.length === 1),
                        ),
                        () => `<${deployable.join('|')}>`,
                    );
                    const stack = Option.flatMap(Array.head(invocation.values('--stack')), Struct.get('text'));
                    const foreign = invocation.flags.some(({ flag }) => !flag.spellings.some((spelling) => pulumied.has(spelling)));
                    const words = Array.isReadonlyArrayEmpty(args) && Option.isNone(stack) ? Array.make('nx', wanted, project) : Array.make('nx', 'run', `${project}:${stage}`, ...args, ...Option.toArray(Option.map(stack, (name) => `--stack=${name}`)));
                    const derived = !project.startsWith('<') && !foreign && (!applies || (invocation.has('--yes', '-y', '--skip-preview') && Option.isSome(stack)));
                    return derived ? Option.toArray(invocation.span).map((written) => ({ edit: (editor: Editor) => [editor.replace(written, words)] })) : [{ deny: `pulumi ${verb} skips the stack programs Nx builds first.`, advice: `Run ${words.join(' ')}.` }];
                },
            });
        }),
    },
} satisfies Table;

// --- [EXPORTS] -------------------------------------------------------------------------

export { tasks };
