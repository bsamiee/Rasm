import { tasks as graphed } from '@rasm/nx/graph';
import { Array, Effect, FileSystem, Match, Option, Path, Record, Schema } from 'effect';
import { walk } from 'sh-syntax';
import { type Call, command, type Editor, head, type Invocation, is, leaves, matches, named, nearest, operand, ran, said, span, staged, type Table, texts, type Verdict, type Word } from '#/rules/rule';

// --- [TYPES] ---------------------------------------------------------------------------

type Graph = Effect.Success<ReturnType<typeof graphed>> & { readonly root: string };
type Row = Graph['tasks'] extends ReadonlyMap<string, infer R> ? R : never;
type Steps = (typeof Hooks.Type)['steps'];

// --- [CONSTANTS] -----------------------------------------------------------------------

const shared = 'Leave it running; nx reset --only-cache empties the cache alone.';
const launchers: ReadonlySet<string> = new Set(['', 'pnpm exec', 'npx', 'uv run', 'mise exec', 'pixi run', 'npm exec']);
const plain = /^[\w./:@%+=,-]+$/u;
const ending: ReadonlySet<string> = new Set(['show', 'graph', 'sync', 'sync:check', 'conformance:check']);
const operations: Readonly<Record<string, { readonly aggregate: string; readonly args: readonly string[]; readonly applies: boolean }>> = {
    up: { aggregate: 'deploy', args: [], applies: true },
    preview: { aggregate: 'dry-run', args: [], applies: false },
    refresh: { aggregate: 'dry-run', args: [], applies: false },
    destroy: { aggregate: 'deploy', args: ['--operation=destroy'], applies: true },
};
const pulumied: ReadonlySet<string> = new Set(['--stack', '-s', '--yes', '-y', '--skip-preview', '--non-interactive', '--cwd', '-C']);

// --- [MODELS] --------------------------------------------------------------------------

const Argv = Schema.Struct({ command: Schema.Union([Schema.String, Schema.Struct({ argv: Schema.Array(Schema.String) })]) });
const Hooks = Schema.fromJsonString(Schema.Struct({ steps: Schema.Record(Schema.String, Schema.Struct({ check: Schema.optionalKey(Schema.NullOr(Argv)), fix: Schema.optionalKey(Schema.NullOr(Argv)) })) }));
const Plan = Schema.fromJsonString(Schema.Struct({ steps: Schema.Array(Schema.Struct({ status: Schema.String, fileCount: Schema.optionalKey(Schema.Finite) })) }));
const Metadata = Schema.fromJsonString(Schema.Struct({ workspace_root: Schema.String, packages: Schema.Array(Schema.Struct({ name: Schema.String, manifest_path: Schema.String })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const workspace = Effect.fnUntraced(function* (cwd: string) {
    const root = yield* nearest(cwd, 'nx.json');
    const graph = yield* Option.match(root, { onNone: () => Effect.succeedNone, onSome: (folder) => Effect.option(graphed(folder)) });
    return Option.map(Option.all([root, graph]), ([folder, held]): Graph => ({ ...held, root: folder }));
});
const addressed = (invocation: Invocation, graph: Graph): readonly Row[] => {
    const [first = '', second = ''] = [Option.getOrUndefined(operand(invocation)), Option.getOrUndefined(operand(invocation, 1))];
    const listed = (flag: string) =>
        invocation
            .values(flag)
            .flatMap((value) => Option.toArray(said(value)))
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
const placed = Effect.fnUntraced(function* (invocation: Invocation, cwd: string, root: string, words: readonly Invocation['words'][number][] = invocation.operands.map(({ word }) => word)) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const base = Option.getOrElse(invocation.place._tag === 'Local' ? invocation.place.cwd.text : Option.none(), () => cwd);
    const paths = words.map((word) => ({ word, text: said(word), path: Option.map(said(word), (text) => path.resolve(base, text)) }));
    const present = yield* Effect.forEach(paths, ({ path: file }) => Option.match(file, { onNone: () => Effect.succeed(false), onSome: (held) => Effect.orElseSucceed(fs.exists(held), () => false) }));
    return Array.zipWith(paths, present, ({ word, text, path: file }, exists) => ({ word, text, file: Option.filter(file, (held) => exists && (held === root || held.startsWith(`${root}/`))) }));
});
const helps = (graph: Graph) =>
    Array.fromIterable(graph.tasks.values()).flatMap((row) => {
        const words = Option.getOrElse(
            Option.filter(
                Option.map(row.help, (help) => help.split(' ')),
                (all) => all.every((word) => plain.test(word)),
            ),
            Array.empty<string>,
        );
        return row.cached && Option.isNone(row.configuration)
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
    });
const routable = (call: Call) => (invocation: Invocation) =>
    call.action.tool === 'Bash' &&
    invocation.inner.length === 0 &&
    Option.isNone(invocation.repeated) &&
    invocation.assigns.length === 0 &&
    invocation.rest.length === 0 &&
    invocation.flags.length === 0 &&
    !is(invocation, 'nx', 'hk') &&
    !Option.exists(invocation.stage, ({ index, count }) => index < count - 1) &&
    invocation.within.every((within) => within._tag === 'Wrapper' && [...launchers].some((launcher) => launcher.split(' ')[0] === within.name));
const routed = (graph: Graph, picks: Array.NonEmptyReadonlyArray<Row>): Array.NonEmptyReadonlyArray<string> => {
    const [first] = picks;
    const whole = Option.filter(aggregate(graph, first), ({ runs }) => runs._tag === 'Aggregate' && runs.members.length === 1);
    return picks.length > 1 ? Array.make('nx', 'run-many', '-t', first.target, '-p', picks.map(({ project }) => project).join(',')) : Option.match(whole, { onNone: () => Array.make('nx', 'run', `${first.project}:${first.target}`), onSome: ({ target }) => Array.make('nx', target, first.project) });
};
const route = Effect.fnUntraced(function* (invocation: Invocation, graph: Graph, helped: ReturnType<typeof helps>, cwd: string) {
    const operands = yield* placed(invocation, cwd, graph.root);
    const program = Option.getOrUndefined(named(invocation));
    const spoken = [...invocation.path, ...operands.flatMap(({ text, file }) => (Option.isSome(file) ? [] : Option.toArray(text)))].join(' ');
    const files = operands.flatMap(({ file }) => Option.toArray(file));
    const owners = Array.dedupe(Array.isReadonlyArrayEmpty(files) ? Option.toArray(graph.owner(cwd)) : files.flatMap((file) => Option.toArray(graph.owner(file))));
    const rows = helped.filter(({ row, program: helping, stated }) => helping === program && stated === spoken && owners.includes(row.project)).map(({ row }) => row);
    const picks = owners.flatMap((owner) => (rows.filter(({ project }) => project === owner).length === 1 ? rows.filter(({ project }) => project === owner) : []));
    const narrowed = files.some((file) => !picks.some(({ root }) => file === `${graph.root}/${root}`.replace(/\/\.$/u, '') || file === graph.root));
    const several = Array.findFirst(owners, (owner) => rows.filter(({ project }) => project === owner).length > 1);
    const whole = invocation.within.length === 0 ? span(invocation) : Option.all([Option.map(launched(invocation), ([first]) => first), Option.map(span(invocation), ([, last]) => last)]);
    const ready = Option.filter(Option.all([Array.match(picks, { onEmpty: Option.none, onNonEmpty: Option.some }), whole]), ([held]) => held.length === owners.length && held.every(({ target: name }) => name === held[0].target) && !narrowed);
    return Match.value({ several: Option.getOrUndefined(several), ready: Option.getOrUndefined(ready) }).pipe(
        Match.when({ several: Match.string }, ({ several: owner }) => [
            {
                refuse: `${command(invocation)} matches several targets of ${owner}.`,
                instruct: `Run one of: ${rows
                    .filter(({ project }) => project === owner)
                    .map((row) => `nx run ${row.project}:${row.target}`)
                    .join(', ')}.`,
            },
        ]),
        Match.when({ ready: Match.defined }, ({ ready: [chosen, words] }) => [{ edit: (editor: Editor) => [editor.replace(words, routed(graph, chosen))] }]),
        Match.orElse((): readonly Verdict[] => []),
    );
});
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
    Record.toEntries(configured).flatMap(([step, modes]) =>
        (['check', 'fix'] as const).flatMap((mode) => {
            const own = argv(modes[mode]);
            const other = argv(modes[mode === 'check' ? 'fix' : 'check']);
            const words = own.slice(1).filter((word) => word !== '{{files}}');
            const distinct = other.length === 0 ? [] : words.filter((word) => !other.includes(word));
            return own[0] === program && own.includes('{{files}}') && asked.every((word) => words.includes(word)) && distinct.every((word) => asked.includes(word)) ? [{ step, mode }] : [];
        }),
    );
const stepped = Effect.fnUntraced(function* (invocation: Invocation, configured: Steps, root: string, cwd: string) {
    const path = yield* Path.Path;
    const operands = yield* placed(invocation, cwd, root, invocation.words.slice(1));
    const files = operands.flatMap(({ file }) => Option.toArray(file));
    const asked = texts(invocation.words.slice(1).filter((written) => !operands.some(({ word, file }) => word === written && Option.isSome(file))));
    const fitting = Option.match(asked, {
        onNone: () => [],
        onSome: (words) =>
            fits(
                configured,
                Option.getOrElse(named(invocation), () => ''),
                words,
            ),
    });
    const relative = files.map((file) => path.relative(cwd, file) || '.');
    const fit = Option.filter(Array.head(fitting), () => fitting.length === 1 && Array.isReadonlyArrayNonEmpty(files));
    const plan = yield* Option.match(fit, { onNone: () => Effect.succeed({ printed: '', code: 1 }), onSome: ({ mode, step }) => ran(Array.make('hk', mode, '--step', step, '--plan', '--json', ...relative), cwd) });
    const included = Option.exists(Schema.decodeOption(Plan)(plan.printed), ({ steps: planned }) => planned.some(({ status, fileCount }) => status === 'included' && fileCount === files.length));
    return Option.match(
        Option.filter(Option.all([fit, span(invocation)]), () => included),
        {
            onNone: (): readonly Verdict[] => [],
            onSome: ([{ step, mode }, whole]) => [{ edit: (editor: Editor) => [editor.replace(whole, Array.make('hk', mode, '--step', step, ...relative))] }],
        },
    );
});
const crates = Effect.fnUntraced(function* (invocation: Invocation, cwd: string) {
    const path = yield* Path.Path;
    const packages = invocation.values('-p', '--package').flatMap((value) => Option.toArray(said(value)));
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
    'nx.publish': { program: 'nx', judge: (invocation) => (invocation.words.some((written) => Option.exists(said(written), (text) => /(?:^|[:,=])nx-release-publish(?:,|$)/u.test(text))) ? [{ refuse: 'nx-release-publish publishes dependencies for real.', instruct: 'Run nx release.' }] : []) },
    'nx.daemon': {
        program: 'nx',
        path: ['daemon'],
        judge: Effect.fnUntraced(function* (invocation, { script }) {
            const later = leaves(yield* script.invocations).some((other) => other.start > invocation.end && is(other, 'nx') && !Option.exists(operand(other), (verb) => verb === 'daemon' || verb === 'reset'));
            return Match.value({ stopped: invocation.has('--stop'), stmt: Option.getOrUndefined(Option.filter(invocation.node, () => later)) }).pipe(
                Match.when({ stopped: false }, () => []),
                Match.when({ stmt: Match.defined }, ({ stmt }) => [{ edit: (editor: Editor) => [editor.remove(stmt)] }, { note: 'nx daemon --stop left the call, as every session shares the daemon.', once: 'call' as const }]),
                Match.orElse(() => [{ refuse: 'nx daemon --stop stops the daemon every session shares.', instruct: shared }]),
            );
        }),
    },
    'nx.reset': {
        program: 'nx',
        path: ['reset'],
        judge: (invocation) => (invocation.has('--only-cache') && !invocation.has('--only-daemon', '--only-workspace-data') ? [] : [{ refuse: 'nx reset stops the daemon and deletes the workspace data every session shares.', instruct: 'Run nx reset --only-cache.' }]),
    },
    'nx.skip-sync': { program: 'nx', judge: (invocation) => (invocation.words.some((written) => Option.contains(said(written), '--skip-sync')) ? [{ refuse: 'nx --skip-sync leaves tsconfig references stale.', instruct: 'Run nx sync, then the command without --skip-sync.' }] : []) },
    'nx.daemonless': {
        program: 'nx',
        judge: (invocation) => {
            const assigns = Option.toArray(invocation.node).flatMap(({ cmd }) => (cmd?.type === 'CallExpr' ? cmd.assigns : []));
            return assigns.filter(({ name }) => name?.value === 'NX_DAEMON').map((assign) => ({ edit: (editor: Editor) => [editor.remove(assign)] }));
        },
    },
    'nx.launcher': {
        program: 'nx',
        judge: (invocation) => (Array.isReadonlyArrayNonEmpty(invocation.within) && invocation.within.every((within) => within._tag === 'Wrapper' && ['pnpm', 'npx', 'npm'].includes(within.name)) ? Option.toArray(launched(invocation)).map((pair) => ({ edit: (editor: Editor) => [editor.remove(pair)] })) : []),
    },
    'nx.quieted': {
        program: 'nx',
        judge: Effect.fnUntraced(function* (invocation, { script }) {
            const parsed = Option.exists(staged(yield* script.invocations, invocation, 1), (next) => is(next, 'jq'));
            const redirects = Option.getOrElse(
                Option.map(invocation.node, ({ redirs }) => redirs),
                Array.empty,
            );
            const targets = redirects.map((redirect) => {
                const expansion = Option.map(Option.fromNullishOr(redirect.word), (word) => script.expand(word)[0]);
                return { redirect, target: Option.flatMap(expansion, (held) => (held._tag === 'Expanded' ? held.text : Option.none())) };
            });
            const hidden = targets.filter(({ redirect, target: aimed }) => Option.exists(aimed, (text) => text === '/dev/null' || (parsed && text === '1' && redirect.n?.value === '2')));
            return Option.exists(operand(invocation), (verb) => verb === 'show' || verb === 'graph') && Array.isReadonlyArrayNonEmpty(hidden) ? [{ edit: (editor: Editor) => hidden.map(({ redirect }) => editor.remove(redirect)) }] : [];
        }),
    },
    'nx.bounded': {
        program: 'nx',
        judge: Effect.fnUntraced(function* (invocation, { cwd }) {
            const timed = invocation.within.length === 1 && invocation.within.every((within) => within._tag === 'Wrapper' && within.name === 'timeout');
            const graph = timed ? yield* workspace(cwd) : Option.none();
            const rows = Option.getOrElse(
                Option.map(graph, (held) => addressed(invocation, held).flatMap((row) => members(held, row))),
                Array.empty,
            );
            const finite = Option.exists(operand(invocation), (verb) => ending.has(verb)) || (Array.isReadonlyArrayNonEmpty(rows) && rows.every(({ continuous }) => !continuous));
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
        judge: Effect.fnUntraced(function* (invocation, { cwd }) {
            const graph = invocation.rest.length > 0 ? yield* workspace(cwd) : Option.none();
            const row = Option.flatMap(graph, (held) => Array.findFirst(addressed(invocation, held), ({ runs }) => runs._tag === 'Aggregate'));
            const tools = Option.getOrElse(
                Option.map(row, ({ runs }) => (runs._tag === 'Aggregate' ? runs.members : [])),
                Array.empty,
            );
            const address = Option.all([Option.flatMap(Array.get(invocation.operands, 0), ({ word }) => word.node), Option.flatMap(Array.get(invocation.operands, 1), ({ word }) => word.node)]);
            return Option.match(row, {
                onNone: () => [],
                onSome: ({ project, target: name }) =>
                    Option.match(
                        Option.filter(Option.all([Array.head(tools), address]), () => tools.length === 1),
                        {
                            onNone: () => [{ refuse: `nx ${name} runs ${tools.length} tools, so arguments after -- reach none of them.`, instruct: `Pass them to one of: ${tools.map((tool) => `nx run ${project}:${tool}`).join(', ')}.` }],
                            onSome: ([tool, words]) => [{ edit: (editor: Editor) => [editor.replace(words, Array.make('run', `${project}:${tool}`))] }],
                        },
                    ),
            });
        }),
    },
    'nx.loop': {
        program: 'nx',
        judge: Effect.fnUntraced(function* (invocation, { script }) {
            const loop = Option.getOrUndefined(invocation.repeated);
            const looped = leaves(yield* script.invocations).filter((other) => is(other, 'nx') && Option.exists(other.repeated, ({ start, source }) => start === loop?.start && source === loop.source));
            const spelled = Option.getOrElse(Option.all(looped.map((other) => texts(other.words))), (): readonly (readonly string[])[] => []);
            const [first = [], ...rest] = spelled;
            const varying = first.flatMap((word, at) => (rest.some((other) => other[at] !== word) ? [at] : []));
            const items = Array.dedupe(spelled.map((words) => words[varying[0] ?? 0] ?? ''));
            const plan = Match.value({ verb: first[1] ?? '', at: varying.length === 1 && rest.every((other) => other.length === first.length) ? varying[0] : undefined }).pipe(
                Match.when({ at: Match.undefined }, () => Option.none()),
                Match.when({ verb: 'run' }, () => Option.some({ targets: Array.dedupe(items.map((item) => item.split(':')[1] ?? '')), projects: Array.dedupe(items.map((item) => item.split(':')[0] ?? '')) })),
                Match.when({ at: 2 }, () => Option.some({ targets: [first[1] ?? ''], projects: items })),
                Match.when({ at: 1 }, () => Option.some({ targets: items, projects: [first[2] ?? ''] })),
                Match.orElse(() => Option.none()),
            );
            return invocation === looped[0]
                ? Option.toArray(plan).map(({ targets, projects }) => ({
                      refuse: 'A loop runs nx once per item, each without the cache and parallelism of one run.',
                      instruct: `Run nx run-many -t ${targets.join(',')} -p ${projects.join(',')}${first
                          .slice(3)
                          .map((word) => ` ${word}`)
                          .join('')}.`,
                  }))
                : [];
        }),
    },
    'vitest.loop': {
        program: 'vitest',
        judge: (invocation, { script }) => {
            const counts: number[] = [];
            const start = Option.getOrUndefined(Option.map(invocation.repeated, (loop) => loop.start));
            walk(script.syntax, (found) => {
                counts.push(...(found.type === 'ForClause' && found.loop?.type === 'WordIter' && script.located(found.start) === start ? [found.loop.items.length] : []));
            });
            return Option.toArray(Option.filter(Array.head(counts), (count) => count > 1 && !invocation.has('--repeats') && Option.isSome(texts(invocation.words)))).map((count) => ({
                refuse: `A loop runs vitest ${count} times, each paying its start again.`,
                instruct: `Run ${Option.getOrElse(texts(invocation.words), (): readonly string[] => []).join(' ')} --repeats ${count - 1} once.`,
            }));
        },
    },
    'nx.target': {
        script: true,
        judge: Effect.fnUntraced(function* (script, call) {
            const candidates = leaves(yield* script.invocations).filter(routable(call));
            const graph = Array.isReadonlyArrayNonEmpty(candidates) ? yield* workspace(call.cwd) : Option.none();
            const helped = Option.getOrElse(Option.map(graph, helps), Array.empty);
            const verdicts = yield* Effect.forEach(
                Option.toArray(graph).flatMap((held) => candidates.map((invocation) => [invocation, held] as const)),
                ([invocation, held]) => route(invocation, held, helped, call.cwd),
            );
            return verdicts.flat();
        }),
    },
    'hk.step': {
        script: true,
        judge: Effect.fnUntraced(function* (script, call) {
            const candidates = leaves(yield* script.invocations).filter(
                (invocation) => call.action.tool === 'Bash' && invocation.inner.length === 0 && Option.isNone(invocation.repeated) && invocation.assigns.length === 0 && Option.isNone(invocation.stage) && invocation.within.length === 0 && !is(invocation, 'hk', 'nx'),
            );
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const root = Array.isReadonlyArrayNonEmpty(candidates) ? yield* nearest(call.cwd, '.git') : Option.none();
            const declared = yield* Option.match(root, { onNone: () => Effect.succeed(''), onSome: (folder) => Effect.orElseSucceed(fs.readFileString(path.join(folder, '.config/hk.pkl')), () => '') });
            const listed = candidates.filter((invocation) => Option.exists(named(invocation), (name) => declared.includes(`"${name}"`)));
            const configured = Option.isSome(root) && Array.isReadonlyArrayNonEmpty(listed) ? yield* steps(Option.getOrElse(root, () => call.cwd)) : {};
            const verdicts = yield* Effect.forEach(
                Option.toArray(root).flatMap((folder) => listed.map((invocation) => [invocation, folder] as const)),
                ([invocation, folder]) => stepped(invocation, configured, folder, call.cwd),
            );
            return verdicts.flat();
        }),
    },
    'cargo.fmt': {
        program: 'cargo',
        aliases: ['cargo-fmt'],
        judge: Effect.fnUntraced(function* (invocation, { cwd }) {
            const formatting = is(invocation, 'cargo-fmt') || invocation.path[0] === 'fmt' || Option.contains(operand(invocation), 'fmt');
            const root = formatting ? yield* nearest(cwd, '.git') : Option.none();
            const mode = invocation.has('--check') ? 'check' : 'fix';
            const configured = Option.isSome(root) ? yield* steps(Option.getOrElse(root, () => cwd)) : {};
            const step = Array.findFirst(Record.toEntries(configured), ([, modes]) => argv(modes[mode])[0] === 'rustfmt');
            const folders = Option.isSome(step) ? yield* crates(invocation, cwd) : Option.none();
            const known = invocation.flags.every(({ flag }) => flag.spellings.some((spelling) => ['--check', '-p', '--package', '--all'].includes(spelling))) && invocation.rest.length === 0 && invocation.operands.length <= 1;
            return Match.value({ step: Option.getOrUndefined(step), routed: Option.getOrUndefined(Option.all([step, folders, span(invocation)])) }).pipe(
                Match.when({ routed: Match.defined }, ({ routed: [[name], targets, whole] }) =>
                    known ? [{ edit: (editor: Editor) => [editor.replace(whole, Array.make('hk', mode, '--step', name, ...targets))] }] : [{ refuse: 'cargo fmt option has no hk form.', instruct: `Run hk ${mode} --step ${name} ${targets.join(' ')}.` }],
                ),
                Match.when({ step: Match.defined }, () => [{ refuse: 'cargo fmt resolves no crate here.', instruct: 'Pass --package <crate>, or run it in the crate folder.' }]),
                Match.orElse((): readonly Verdict[] => []),
            );
        }),
    },
    'pulumi.deploy': {
        program: 'pulumi',
        judge: Effect.fnUntraced(function* (invocation, { cwd }) {
            const verb = Option.getOrElse(
                Option.orElse(Array.head(invocation.path), () => operand(invocation)),
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
                    const stack = Option.flatMap(Array.head(invocation.values('--stack')), said);
                    const foreign = invocation.flags.some(({ flag }) => !flag.spellings.some((spelling) => pulumied.has(spelling)));
                    const words = Array.isReadonlyArrayEmpty(args) && Option.isNone(stack) ? Array.make('nx', wanted, project) : Array.make('nx', 'run', `${project}:${stage}`, ...args, ...Option.toArray(Option.map(stack, (name) => `--stack=${name}`)));
                    const derived = !project.startsWith('<') && !foreign && (!applies || (invocation.has('--yes', '-y', '--skip-preview') && Option.isSome(stack)));
                    return derived ? Option.toArray(span(invocation)).map((whole) => ({ edit: (editor: Editor) => [editor.replace(whole, words)] })) : [{ refuse: `pulumi ${verb} skips the stack programs Nx builds first.`, instruct: `Run ${words.join(' ')}.` }];
                },
            });
        }),
    },
    'ruff.isolated': {
        program: 'ruff',
        judge: Effect.fnUntraced(function* (invocation, { cwd }) {
            const root = yield* nearest(cwd, '.git');
            return Option.isSome(root) && invocation.has('--isolated') ? [{ drop: ['--isolated'] }, { note: 'ruff --isolated left the call, as the project configuration holds its rules.', once: 'agent' as const }] : [];
        }),
    },
    'python.cache': {
        program: 'ruff',
        aliases: ['mypy'],
        judge: Effect.fnUntraced(function* (invocation, { cwd }) {
            const root = yield* nearest(cwd, '.git');
            const path = yield* Path.Path;
            const caches = invocation.values('--cache-dir').flatMap((value) => Option.toArray(said(value)).map((cache) => path.resolve(cwd, cache)));
            const kept =
                Option.isNone(root) ||
                !Array.isReadonlyArrayNonEmpty(caches) ||
                (yield* ran(
                    ['git', 'check-ignore', '--no-index', '-q', '--', ...caches],
                    Option.getOrElse(root, () => cwd),
                )).code === 0;
            const assigns = Option.toArray(invocation.node)
                .flatMap(({ cmd }) => (cmd?.type === 'CallExpr' ? cmd.assigns : []))
                .filter(({ name }) => /^(?:RUFF|MYPY)_CACHE_DIR$/u.test(name?.value ?? ''));
            return Option.isNone(root)
                ? []
                : [...(kept ? [] : [{ drop: ['--cache-dir'] }, { note: `${command(invocation)} --cache-dir left the call, as the cache belongs in the ignored folder the project configures.`, once: 'agent' as const }]), ...assigns.map((assign) => ({ edit: (editor: Editor) => [editor.remove(assign)] }))];
        }),
    },
    'ruff.config': {
        program: 'ruff',
        judge: (invocation) => (['check', 'format'].includes(invocation.path[0] ?? '') && invocation.values('--config').some((value) => Option.exists(said(value), (text) => !text.includes('='))) && !invocation.has('--no-cache', '-n') ? [{ ensure: Array.of('--no-cache') }] : []),
    },
    'stubgen.cache': {
        program: 'stubgen',
        judge: Effect.fnUntraced(function* (invocation, { cwd, script }) {
            const root = yield* nearest(cwd, '.git');
            const set = Object.hasOwn(script.dialect.environment, 'MYPY_CACHE_DIR') || invocation.assigns.some(({ name }) => name === 'MYPY_CACHE_DIR');
            return Option.isSome(root) && !set ? Option.toArray(invocation.node).map((stmt) => ({ edit: (editor: Editor) => [editor.assign(stmt, 'MYPY_CACHE_DIR', '/dev/null')] })) : [];
        }),
    },
    'pytest.config': {
        program: 'pytest',
        judge: Effect.fnUntraced(function* (invocation, { cwd }) {
            const root = yield* nearest(cwd, '.git');
            return Option.isSome(root) && invocation.has('--config-file', '-c', '--benchmark-storage') ? [{ refuse: 'pytest --config-file and --benchmark-storage leave the project configuration.', instruct: 'Run pytest with the project configuration and its benchmark storage.' }] : [];
        }),
    },
    'pytest.overrides': {
        program: 'pytest',
        judge: Effect.fnUntraced(function* (invocation, { cwd }) {
            const root = yield* nearest(cwd, '.git');
            const overrides = invocation.flags.filter(({ flag, values }) => (flag.spellings.includes('-o') || flag.spellings.includes('--override-ini')) && values.some((value) => matches(value, /^(?:addopts|cache_dir)=/u)));
            const moves = overrides.flatMap(({ spelled, values }) => {
                const last = Option.getOrUndefined(Option.flatMap(Array.last(values), ({ node }) => node));
                const moved = Option.getOrElse(texts(values), (): readonly string[] => [])
                    .flatMap((text) => (text.startsWith('addopts=') ? text.slice('addopts='.length).split(/\s+/u) : []))
                    .filter((word) => word !== '');
                return Option.toArray(spelled.node).map((first) => ({ range: [first, last ?? first] as const, moved }));
            });
            return Match.value({ inside: Option.isSome(root) && Array.isReadonlyArrayNonEmpty(overrides), unknown: overrides.some(({ values }) => Option.isNone(texts(values))) }).pipe(
                Match.when({ inside: false }, () => []),
                Match.when({ unknown: true }, () => [{ refuse: 'pytest -o addopts expands only when it runs.', instruct: 'Pass the test flags to pytest directly.' }]),
                Match.orElse(() => [{ edit: (editor: Editor) => moves.map(({ range, moved }) => (Array.isReadonlyArrayNonEmpty(moved) ? editor.replace(range, moved) : editor.remove(range))) }]),
            );
        }),
    },
    'pytest.addopts': {
        program: 'pytest',
        judge: Effect.fnUntraced(function* (invocation, { cwd }) {
            const root = yield* nearest(cwd, '.git');
            const assign = Array.findFirst(
                Option.toArray(invocation.node).flatMap(({ cmd }) => (cmd?.type === 'CallExpr' ? cmd.assigns : [])),
                ({ name }) => name?.value === 'PYTEST_ADDOPTS',
            );
            const value = Option.flatMap(
                Array.findFirst(invocation.assigns, ({ name }) => name === 'PYTEST_ADDOPTS'),
                ({ value: written }) => said(written),
            );
            const moved = Option.getOrElse(value, () => '')
                .split(/\s+/u)
                .filter((word) => word !== '');
            return Match.value({ set: Option.isSome(root) && Option.isSome(assign), known: Option.isSome(value), moved: Array.match(moved, { onEmpty: () => undefined, onNonEmpty: (words) => words }) }).pipe(
                Match.when({ set: false }, () => []),
                Match.when({ known: false }, () => [{ refuse: 'PYTEST_ADDOPTS expands only when pytest runs.', instruct: 'Pass the test flags to pytest directly.' }]),
                Match.orElse(({ moved: words }) => [{ edit: (editor: Editor) => [...Option.toArray(assign).map((held) => editor.remove(held)), ...Option.toArray(Option.all([head(invocation), Option.fromUndefinedOr(words)])).map(([word, given]) => editor.after(word, given))] }]),
            );
        }),
    },
    'python.outside': {
        program: 'python',
        aliases: ['python3'],
        judge: Effect.fnUntraced(function* (invocation, { cwd }) {
            const root = yield* nearest(cwd, '.git');
            const path = yield* Path.Path;
            const script = Option.flatMap(Array.head(invocation.reads), ({ text }) => text);
            const outside = Option.filter(Option.all([root, script]), ([folder, file]) => !file.startsWith(`${folder}/`) && invocation.within.length === 0 && !invocation.has('-c', '-m'));
            const operands = yield* placed(invocation, cwd, '');
            const absolute = operands.filter(({ text }) => Option.exists(text, (given) => !given.startsWith('/'))).flatMap(({ word, file }) => Option.toArray(Option.all([word.node, file])));
            return Option.match(Option.all([outside, Option.flatMap(Array.head(invocation.words), ({ node }) => node)]), {
                onNone: () => [],
                onSome: ([[, file], first]) => [{ edit: (editor: Editor) => [editor.before(first, Array.make('env', '-C', path.dirname(file))), ...absolute.map(([node, held]) => editor.replace(node, Array.of(held)))] }],
            });
        }),
    },
    'cores.counted': {
        program: 'xargs',
        aliases: ['parallel', 'make', 'gmake', 'ninja'],
        judge: Effect.fnUntraced(function* (invocation, { cwd }) {
            const unset = invocation.flags.filter(({ flag, spelled, values }) => {
                const next = Option.flatMap(Array.get(invocation.words, invocation.words.indexOf(spelled) + 1), said);
                const zero = values.every((value) => Option.contains(said(value), '0')) || Option.exists(said(spelled), (text) => /\D0$/u.test(text));
                return flag.spellings.some((spelling) => ['-P', '--max-procs', '-j', '--jobs'].includes(spelling)) && zero && !(values.length === 0 && Option.exists(next, (text) => /^\d+$/u.test(text)));
            });
            const cores = Array.isReadonlyArrayNonEmpty(unset) ? (yield* ran(['getconf', '_NPROCESSORS_ONLN'], cwd)).printed.trim() : '';
            const targets = unset.flatMap(({ spelled, values }) => {
                const value = Option.getOrUndefined(Option.flatMap(Array.head(values), ({ node }) => node));
                const written = Option.getOrUndefined(said(spelled)) ?? '';
                return value === undefined ? Option.toArray(spelled.node).map((word) => [word, `${written.replace(/(?:=)?0$/u, '')}${written.startsWith('--') ? '=' : ''}${cores}`] as const) : [[value, cores] as const];
            });
            return Array.isReadonlyArrayNonEmpty(targets) ? [{ edit: (editor: Editor) => targets.map(([word, text]) => editor.replace(word, Array.of(text))) }] : [];
        }),
    },
    'uv.sync': { program: 'uv', path: ['sync'], ensure: ['--all-packages'], unless: ['--package', '--script'] },
    'blender.environment': { program: 'Blender', judge: (invocation) => (invocation.has('--python', '--python-expr') && !invocation.has('--python-use-system-env') ? [{ ensure: Array.of('--python-use-system-env') }] : []) },
} satisfies Table;

// --- [EXPORTS] -------------------------------------------------------------------------

export { tasks };
