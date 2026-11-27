import { tasks as projectGraph } from '@rasm/nx/graph';
import { Array, Effect, Equal, FileSystem, Function, Match, Option, Path, Predicate, Record, Schema, String } from 'effect';
import { RedirOperator, type Stmt, type Word } from 'sh-syntax';
import { covers, execute, findUp, type Invocation, type Script, type Table, type Verdict } from '#/rules/rule';

// --- [TYPES] ---------------------------------------------------------------------------

type Graph = Effect.Success<ReturnType<typeof projectGraph>> & { readonly root: string };
type Row = NonNullable<ReturnType<Graph['tasks']['get']>>;
type Steps = (typeof Hooks.Type)['steps'];

// --- [CONSTANTS] -----------------------------------------------------------------------

const daemonAdvice = 'Leave it running; nx reset --only-cache empties the cache alone.';
const launchers: ReadonlySet<string> = new Set(['', 'pnpm exec', 'npx', 'uv run', 'mise exec', 'pixi run', 'npm exec']);
const launcherPrograms: ReadonlySet<string> = new Set([...launchers].flatMap((launcher) => launcher.split(' ').slice(0, 1).filter(String.isNonEmpty)));
const boundedPrograms: ReadonlySet<string> = new Set([...launcherPrograms, 'timeout']);
const plainWord = /^[\w./:@%+=,-]+$/u;
const finiteVerbs: ReadonlySet<string> = new Set(['sync', 'sync:check', 'conformance:check']);
const stackOperations: Readonly<Record<string, { readonly aggregate: string; readonly args: readonly string[]; readonly applies: boolean }>> = {
    up: { aggregate: 'deploy', args: [], applies: true },
    preview: { aggregate: 'dry-run', args: [], applies: false },
    refresh: { aggregate: 'dry-run', args: [], applies: false },
    destroy: { aggregate: 'deploy', args: ['--operation=destroy'], applies: true },
};
const stackFlags = ['--stack', '-s', '--yes', '-y', '--skip-preview', '--non-interactive', '--cwd', '-C'];

// --- [MODELS] --------------------------------------------------------------------------

const Words = Schema.Array(Schema.String);
const Command = Schema.Struct({ command: Schema.Union([Schema.String, Schema.Struct({ argv: Words })]) }).pipe(Schema.NullOr, Schema.optionalKey);
const Hooks = Schema.fromJsonString(Schema.Struct({ steps: Schema.Record(Schema.String, Schema.Struct({ check: Command, check_diff: Command, check_list_files: Command, fix: Command })) }));
const Plan = Schema.fromJsonString(Schema.Struct({ steps: Schema.Array(Schema.Struct({ status: Schema.String, fileCount: Schema.optionalKey(Schema.Finite) })) }));
const Metadata = Schema.fromJsonString(Schema.Struct({ workspace_root: Schema.String, packages: Schema.Array(Schema.Struct({ name: Schema.String, manifest_path: Schema.String })) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const cwdOf = (invocation: Invocation, cwd: string): string => Option.getOrElse(invocation.place._tag === 'Local' ? invocation.place.cwd.text : Option.none(), () => cwd);
const launcherOf = (invocation: Invocation): Option.Option<readonly [Word, Word]> =>
    Option.flatMap(Option.all([invocation.node, invocation.head]), ([stmt, word]) => {
        const args = stmt.cmd?.type === 'CallExpr' ? stmt.cmd.args : [];
        return Option.all([Array.head(args), Array.get(args, args.indexOf(word) - 1)]);
    });
const extentOf = (invocation: Invocation): Option.Option<readonly [Word, Word]> => (invocation.within.length === 0 ? invocation.span : Option.all([Option.map(launcherOf(invocation), ([first]) => first), Option.map(invocation.span, ([, last]) => last)]));
const isLaunchedBy = (invocation: Invocation, programs: ReadonlySet<string>) => invocation.within.every((within) => within._tag === 'Wrapper' && programs.has(within.name));
const isRaw = (invocation: Invocation, programs: ReadonlySet<string>) => invocation.inner.length === 0 && Option.isNone(invocation.repeated) && invocation.assigns.length === 0 && !invocation.runs('nx', 'hk') && isLaunchedBy(invocation, programs);
const loopOf = ({ tree }: Script, invocation: Invocation): Option.Option<Stmt> =>
    Option.flatMap(invocation.node, (body) => {
        const [stmt, clause] = Array.takeRight(tree.path(body), 3).map(({ node }) => node);
        const sole = clause?.type === 'ForClause' && clause.loop?.type === 'WordIter' && clause.do.length === 1 && clause.do[0] === body;
        const bare = body.redirs.length === 0 && body.cmd?.type === 'CallExpr' && body.cmd.assigns.length === 0 && !body.background && !body.negated;
        return stmt?.type === 'Stmt' && stmt.redirs.length === 0 && sole && bare ? Option.some(stmt) : Option.none();
    });
const readGraph = Effect.fnUntraced(function* (cwd: string) {
    const root = yield* Effect.fromOption(yield* findUp(cwd, 'nx.json'));
    return { ...(yield* projectGraph(root)), root };
}, Effect.option);
const resolveRows = (nx: Invocation, { tasks: rows }: Graph): readonly Row[] => {
    const [verb, subject] = [Option.getOrElse(nx.operand(), () => ''), Option.getOrElse(nx.operand(1), () => '')];
    const list = (flag: string) => Array.getSomes(nx.values(flag).map(({ text }) => text)).flatMap((value) => value.split(','));
    const projects = Array.match(list('-p'), { onEmpty: () => Array.dedupe(rows.values().map(({ project }) => project)), onNonEmpty: Function.identity });
    const keys = Match.value(verb).pipe(
        Match.when('run', () => [subject.split(':').slice(0, 2).join(':')]),
        Match.when(Match.is('run-many', 'affected'), () => Array.cartesianWith(projects, list('-t'), (project, target) => `${project}:${target}`)),
        Match.orElse(() => [`${subject}:${verb}`]),
    );
    return keys.flatMap((key) => Array.fromNullishOr(rows.get(key)));
};
const expandRow = (graph: Graph, row: Row): readonly Row[] => [row, ...(row.runs._tag === 'Aggregate' ? row.runs.members.flatMap((member) => Array.fromNullishOr(graph.tasks.get(`${row.project}:${member}`))).flatMap((member) => expandRow(graph, member)) : [])];
const folderOf = (graph: Graph, path: Path.Path, row: Row): string => path.join(graph.root, row.runs._tag === 'Commands' ? row.runs.cwd : Option.getOrElse(Option.liftPredicate(row.options['cwd'], Predicate.isString), () => row.root));
const collectLines = (options: unknown): readonly (readonly string[])[] =>
    Match.value(options).pipe(
        Match.when(Predicate.isString, (text) => [text.split(/\s+/u)]),
        Match.when(Schema.is(Words), (words) => [words]),
        Match.when(Predicate.isObjectOrArray, (nested) => Object.values(nested).flatMap(collectLines)),
        Match.orElse(() => []),
    );
const helpsOf = (row: Row) => {
    const commands = collectLines(row.options);
    const help = Option.filter(
        Option.map(row.help, (command) => command.split(' ')),
        (words) => Option.isNone(row.configuration) && words.every((word) => plainWord.test(word)),
    );
    return Option.getOrElse(help, Array.empty<string>).flatMap((program, at, words) => {
        const stated = words.slice(at + 1).filter((word) => !word.startsWith('-'));
        const found = commands.flatMap((line) => {
            const index = line.findIndex((word) => word === program || word === stated[0]);
            return index === -1 ? [] : [new Set(line.slice(line[index] === program ? index + 1 : index))];
        });
        const lines = Array.isReadonlyArrayEmpty(found) ? [new Set<string>()] : found;
        const configured = lines.reduce((all, line) => all.union(line), new Set<string>());
        return launchers.has(words.slice(0, at).join(' ')) ? [{ row, program, stated: stated.join(' '), lines, configured }] : [];
    });
};
const listHelps = Function.memoize((rows: Graph['tasks']) => Array.fromIterable(rows.values()).flatMap(helpsOf));
const locate = Effect.fnUntraced(function* (words: Invocation['words'], cwd: string, root: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    return yield* Effect.forEach(
        words,
        Effect.fnUntraced(function* (word) {
            const inside = Option.filter(
                Option.map(word.text, (text) => path.resolve(cwd, text)),
                (file) => covers(root, file),
            );
            const exists = yield* Option.match(inside, { onNone: () => Effect.succeed(false), onSome: (file) => Effect.orElseSucceed(fs.exists(file), () => false) });
            return { word, file: Option.filter(inside, () => exists) };
        }),
    );
});
const restate = (graph: Graph, picks: Array.NonEmptyReadonlyArray<Row>, forwarded: readonly string[], everywhere: boolean): Array.NonEmptyReadonlyArray<string> => {
    const [first] = picks;
    const aggregate = Array.findFirst(graph.tasks.values(), ({ project, target, configuration, runs }) => (project === first.project && Option.isNone(configuration) && runs._tag === 'Aggregate' && runs.members.includes(first.target) ? Option.some({ target, members: runs.members }) : Option.none()));
    const projects = everywhere ? [] : ['-p', picks.map(({ project }) => project).join(',')];
    const forward = Array.isReadonlyArrayEmpty(forwarded) ? [] : ['--', ...forwarded];
    return picks.length > 1
        ? Array.make('nx', 'run-many', '-t', first.target, ...projects)
        : Option.match(
              Option.filter(aggregate, ({ members }) => members.length === 1 && Array.isReadonlyArrayEmpty(forwarded)),
              { onNone: () => Array.make('nx', 'run', `${first.project}:${first.target}`, ...forward), onSome: ({ target }) => Array.make('nx', target, first.project) },
          );
};
const route = Effect.fnUntraced(function* (invocation: Invocation, graph: Graph, cwd: string) {
    const path = yield* Path.Path;
    const operands = yield* locate(
        invocation.operands.map(({ word }) => word),
        cwd,
        graph.root,
    );
    const values = yield* Effect.forEach(invocation.flags, (flag) => locate(flag.values, cwd, graph.root));
    const flags = Array.zipWith(invocation.flags, values, ({ spelled, values: given }, located) => {
        const written = Option.all([spelled, ...given.filter(({ node }) => Option.isSome(node))].map(({ text }) => text));
        return { spelling: spelled.text, known: Option.isSome(written), words: Option.getOrElse(written, Array.empty<string>), paths: Array.getSomes(located.map(({ file }) => file)) };
    });
    const files = Array.getSomes(operands.map(({ file }) => file));
    const paths = [...files, ...flags.flatMap(({ paths: given }) => given)];
    const stated = [...invocation.path, ...operands.flatMap(({ word, file }) => (Option.isSome(file) ? [] : Option.toArray(word.text)))].join(' ');
    const helps = listHelps(graph.tasks).filter((help) => invocation.runs(help.program) && help.stated === stated);
    const direct = Array.dedupe((Array.isReadonlyArrayEmpty(paths) ? [cwd] : paths).flatMap((at) => Option.toArray(graph.owner(at))));
    const below = Array.isReadonlyArrayEmpty(paths) && !helps.some(({ row }) => row.cached && direct.includes(row.project)) ? Array.dedupe(helps.filter(({ row }) => row.cached && covers(cwd, path.join(graph.root, row.root))).map(({ row }) => row.project)) : [];
    const widened = Array.isReadonlyArrayNonEmpty(below);
    const owners = widened ? below : direct;
    const peers = helps.filter(({ row }) => owners.includes(row.project));
    const asked = new Set(flags.flatMap(({ spelling }) => Option.toArray(spelling)));
    const written = new Set(flags.flatMap(({ words }) => words));
    const fits = (help: (typeof peers)[number]) => {
        const others = peers.filter(({ row }) => row.project === help.row.project && row.target !== help.row.target);
        const uncached = others.filter(({ row }) => !row.cached);
        const loose = uncached.reduce((all, { configured }) => all.union(configured), new Set<string>());
        const folder = folderOf(graph, path, help.row);
        const consumed = flags.filter(({ words }) => !words.every((word) => help.configured.has(word))).every(({ paths: given }) => given.every((file) => file === folder || file === path.join(graph.root, help.row.root) || help.configured.has(path.relative(folder, file))));
        const excluded = [...asked].some((word) => !help.configured.has(word) && others.some(({ configured }) => configured.has(word)));
        const same = help.lines.some((line) => [...line].every((word) => !word.startsWith('-') || written.has(word) || Array.isReadonlyArrayEmpty(uncached) || loose.has(word)));
        return help.row.cached && flags.every(({ known }) => known) && consumed && !excluded && same;
    };
    const fit = peers.filter(fits);
    const rows = fit.map(({ row }) => row);
    const absorbed = Array.match(fit, { onEmpty: () => new Set<string>(), onNonEmpty: ([first, ...others]) => others.reduce((common, { configured }) => common.intersection(configured), first.configured) });
    const owned = owners.map((owner) => [owner, rows.filter(({ project }) => project === owner)] as const);
    const picks = owned.flatMap(([, own]) => (own.length === 1 ? own : []));
    const inner = files.filter((file) => !picks.some(({ root }) => file === path.join(graph.root, root)));
    const forwarded = [...flags.flatMap(({ words, paths: given }) => (Array.isReadonlyArrayNonEmpty(given) ? [] : words)).filter((word) => !absorbed.has(word)), ...Array.take(picks, 1).flatMap((pick) => inner.map((file) => path.relative(folderOf(graph, path, pick), file) || '.'))];
    const narrowed = Array.isReadonlyArrayNonEmpty(inner) && (picks.length !== 1 || widened);
    const ready = Option.filter(
        Option.all([Option.liftPredicate(picks, Array.isArrayNonEmpty), extentOf(invocation)]),
        ([chosen]) => (widened || chosen.length === owners.length) && chosen.every(({ target }) => target === chosen[0].target) && !narrowed && ((chosen.length === 1 && !widened) || Array.isReadonlyArrayEmpty(forwarded)),
    );
    return Match.value({ several: Option.getOrUndefined(Array.findFirst(owned, ([, own]) => own.length > 1)), ready: Option.getOrUndefined(ready) }).pipe(
        Match.when({ several: Match.defined }, ({ several: [owner, own] }): readonly Verdict[] => [{ deny: `${invocation.label} matches several targets of ${owner}.`, advice: `Run one of: ${own.map((row) => `nx run ${row.project}:${row.target}`).join(', ')}.` }]),
        Match.when({ ready: Match.defined }, ({ ready: [chosen, span] }): readonly Verdict[] => [{ edit: (editor) => [editor.replace(span, restate(graph, chosen, forwarded, widened && cwd === graph.root))] }]),
        Match.orElse((): readonly Verdict[] => []),
    );
});
const readSteps = Effect.fnUntraced(function* (root: string) {
    const path = yield* Path.Path;
    const { stdout, code } = yield* execute(root)(['pkl', 'eval', '--format', 'json', path.join(root, '.config/hk.pkl')]);
    return Option.match(
        Option.filter(Schema.decodeOption(Hooks)(stdout), () => code === 0),
        { onNone: (): Steps => ({}), onSome: ({ steps }) => steps },
    );
});
const argvOf = (mode: Steps[string]['check']): readonly string[] => (mode === null || mode === undefined || Predicate.isString(mode.command) ? [] : mode.command.argv);
const matchSteps = (steps: Steps, invocation: Invocation, asked: readonly string[]) =>
    Array.dedupeWith(
        Record.toEntries(steps).flatMap(([step, modes]) => {
            const checks = [argvOf(modes.check), argvOf(modes.check_diff), argvOf(modes.check_list_files)];
            const sides = [...checks.map((own) => ({ mode: 'check' as const, own, other: argvOf(modes.fix) })), { mode: 'fix' as const, own: argvOf(modes.fix), other: checks.flat() }];
            return sides.flatMap(({ mode, own, other }) => {
                const [program = '', ...given] = own;
                const words = given.filter((word) => word !== '{{files}}');
                const distinct = other.length === 0 ? [] : words.filter((word) => !other.includes(word));
                return invocation.runs(program) && own.includes('{{files}}') && asked.every((word) => words.includes(word)) && distinct.every((word) => asked.includes(word)) ? [{ step, mode }] : [];
            });
        }),
        (left, right) => left.step === right.step && left.mode === right.mode,
    );
const routeStep = Effect.fnUntraced(function* (invocation: Invocation, steps: Steps, root: string, cwd: string) {
    const path = yield* Path.Path;
    const words = yield* locate(invocation.words.slice(1), cwd, root);
    const files = Array.getSomes(words.map(({ file }) => file)).map((file) => path.relative(cwd, file) || '.');
    const asked = Option.all(words.filter(({ file }) => Option.isNone(file)).map(({ word }) => word.text));
    const matches = Option.match(asked, { onNone: () => [], onSome: (given) => matchSteps(steps, invocation, given) });
    const match = Option.filter(Array.head(matches), () => matches.length === 1 && Array.isReadonlyArrayNonEmpty(files));
    const plan = yield* Option.match(match, { onNone: () => Effect.succeed(''), onSome: ({ mode, step }) => Effect.map(execute(cwd)(['hk', mode, '--step', step, '--plan', '--json', ...files]), ({ stdout }) => stdout) });
    const included = Option.exists(Schema.decodeOption(Plan)(plan), ({ steps: planned }) => planned.some(({ status, fileCount }) => status === 'included' && fileCount === files.length));
    return Option.toArray(Option.filter(Option.all([match, extentOf(invocation)]), () => included)).map(([{ step, mode }, span]): Verdict => ({ edit: (editor) => [editor.replace(span, Array.make('hk', mode, '--step', step, ...files))] }));
});
const resolveCrates = Effect.fnUntraced(function* (cargo: Invocation, cwd: string) {
    const path = yield* Path.Path;
    const named = Array.getSomes(cargo.values('-p', '--package').map(({ text }) => text));
    const metadata = Schema.decodeOption(Metadata)((yield* execute(cwd)(['cargo', 'metadata', '--no-deps', '--format-version', '1'])).stdout);
    const crates = Option.match(metadata, { onNone: () => [], onSome: ({ packages }) => packages.map(({ name, manifest_path }) => ({ name, folder: path.dirname(manifest_path) })) });
    const owning = Array.findFirst(
        crates.toSorted((left, right) => right.folder.length - left.folder.length),
        ({ folder }) => covers(folder, cwd),
    );
    const chosen = Option.all(
        named.map((name) =>
            Option.map(
                Array.findFirst(crates, (crate) => crate.name === name),
                ({ folder }) => path.relative(cwd, folder) || '.',
            ),
        ),
    );
    return Match.value({ all: cargo.has('--all') || Option.exists(metadata, ({ workspace_root }) => cwd === workspace_root && Array.isReadonlyArrayEmpty(named)), named: Array.isReadonlyArrayNonEmpty(named) }).pipe(
        Match.when({ all: true }, () => Option.some(['--all'])),
        Match.when({ named: true }, () => chosen),
        Match.orElse(() => Option.map(owning, ({ folder }) => [path.relative(cwd, folder) || '.'])),
    );
});

// --- [RULES] ---------------------------------------------------------------------------

const tasks: Table = {
    'worktree.isolated': {
        action: 'Isolate',
        decide: ({ worker }) => Option.match(worker, { onSome: () => [{ apart: false as const }], onNone: () => [{ deny: 'A worktree is a second checkout that every build repeats and the guards of this checkout never see.', advice: 'Work in place.' }] }),
    },
    'nx.publish': { program: 'nx', decide: (nx) => (nx.words.some((word) => word.test(/(?:^|[:,=])nx-release-publish(?:,|$)/u)) ? [{ deny: 'nx-release-publish publishes dependencies for real.', advice: 'Run nx release.' }] : []) },
    'nx.daemon': {
        program: 'nx',
        decide: (nx, { invocations }) => {
            const verb = Option.getOrElse(nx.operand(), () => '');
            const stops = (verb === 'daemon' && nx.has('--stop')) || (verb === 'reset' && nx.has('--only-daemon'));
            const later = invocations.some((other) => other.start > nx.end && other.runs('nx') && !Option.exists(other.operand(), (next) => next === 'daemon' || next === 'reset'));
            const spoken = Option.match(nx.argv, { onNone: () => `nx ${verb}`, onSome: Array.join(' ') });
            return Match.value({ stops, stmt: Option.getOrUndefined(Option.filter(nx.node, () => later)) }).pipe(
                Match.when({ stops: false }, () => []),
                Match.when({ stmt: Match.defined }, ({ stmt }): readonly Verdict[] => [{ edit: (editor) => [editor.remove(stmt)] }, { note: `${spoken} left the call, as every session shares the daemon.`, once: 'call' as const }]),
                Match.orElse(() => [{ deny: `${spoken} stops the daemon every session shares.`, advice: daemonAdvice }]),
            );
        },
    },
    'nx.reset': {
        program: 'nx',
        path: ['reset'],
        decide: (nx) => (nx.has('--only-workspace-data') || !nx.has('--only-cache', '--only-daemon') ? [{ deny: 'nx reset stops the daemon and deletes the workspace data every session shares.', advice: 'Run nx reset --only-cache.' }] : []),
    },
    'nx.skip-sync': {
        program: 'nx',
        decide: Effect.fnUntraced(function* (nx, { shell, script }) {
            const stmt = Option.filter(nx.node, () => nx.has('--skip-sync'));
            const sync = yield* Effect.transposeOption(Option.map(stmt, () => shell.script(script.dialect)`nx sync`));
            return Option.toArray(Option.all([sync, stmt])).flatMap(([command, skipped]) => [
                { drop: ['--skip-sync'] },
                { edit: (editor) => [editor.precede(command, '&&', skipped)] },
                { note: '--skip-sync became nx sync before the command, as a skipped sync leaves tsconfig references stale.', once: 'agent' as const },
            ]);
        }),
    },
    'nx.daemonless': {
        program: 'nx',
        decide: (nx) =>
            Option.toArray(nx.node)
                .flatMap(({ cmd }) => (cmd?.type === 'CallExpr' ? cmd.assigns : []))
                .filter(({ name }) => name?.value === 'NX_DAEMON')
                .map((assign) => ({ edit: (editor) => [editor.remove(assign)] })),
    },
    'nx.launcher': {
        program: 'nx',
        decide: (nx) => (Array.isReadonlyArrayNonEmpty(nx.within) && isLaunchedBy(nx, launcherPrograms) ? nx.pipe(launcherOf, Option.toArray).map((launcher) => ({ edit: (editor) => [editor.remove(launcher)] })) : []),
    },
    'nx.quieted': {
        program: 'nx',
        decide: (nx, { invocations, script }) => {
            const parsed = invocations.some((next) => next.runs('jq') && nx.feeds(next));
            const redirects = Option.toArray(nx.node).flatMap(({ redirs }) => redirs.map((redirect) => ({ redirect, target: Option.flatMap(Option.fromNullOr(redirect.word), (word) => script.expanded(word)) })));
            const discarded = redirects.filter(({ target }) => Option.contains(target, '/dev/null'));
            const silent = discarded.some(({ redirect }) => redirect.n === null && redirect.op === RedirOperator.RdrOut);
            const merged = redirects.filter(({ redirect, target }) => (parsed || silent) && Option.contains(target, '1') && redirect.n?.value === '2' && redirect.op === RedirOperator.DplOut);
            const hidden = [...discarded, ...merged];
            return Option.exists(nx.operand(), (verb) => verb === 'show' || verb === 'graph') && Array.isReadonlyArrayNonEmpty(hidden) ? [{ edit: (editor) => hidden.map(({ redirect }) => editor.remove(redirect)) }] : [];
        },
    },
    'nx.bounded': {
        program: 'nx',
        decide: Effect.fnUntraced(function* (nx, { cwd }) {
            const timed = nx.within.length === 1 && nx.within.every((within) => within._tag === 'Wrapper' && within.name === 'timeout');
            const graph = timed ? yield* readGraph(cwdOf(nx, cwd)) : Option.none();
            const rows = Option.toArray(graph).flatMap((found) => resolveRows(nx, found).flatMap((row) => expandRow(found, row)));
            const verb = Option.getOrElse(nx.operand(), () => '');
            const prints = (verb === 'show' && !nx.has('--web')) || (verb === 'graph' && nx.words.some((word) => word.test(/^--(?:file|print)(?:=|$)/u)));
            const finite = prints || finiteVerbs.has(verb) || (Array.isReadonlyArrayNonEmpty(rows) && rows.every(({ continuous }) => !continuous));
            return Option.toArray(Option.filter(launcherOf(nx), () => timed && finite)).flatMap((launcher) => [
                { edit: (editor) => [editor.remove(launcher)] },
                { note: 'timeout before nx left the call, as its kill loses unfinished tasks of a run that ends by itself; a foreground call past its timeout moves to the background and notifies, and TaskStop ends a hung run.', once: 'agent' as const },
            ]);
        }),
    },
    'nx.aggregate': {
        program: 'nx',
        decide: Effect.fnUntraced(function* (nx, { cwd }) {
            const graph = nx.rest.length > 0 ? yield* readGraph(cwdOf(nx, cwd)) : Option.none();
            const aggregates = Option.toArray(graph).flatMap((found) => resolveRows(nx, found).flatMap(({ project, target, runs }) => (runs._tag === 'Aggregate' ? [{ project, target, tools: runs.members }] : [])));
            const tools = Array.dedupe(aggregates.flatMap(({ tools: members }) => members));
            const many = Option.exists(nx.operand(), (verb) => verb === 'run-many' || verb === 'affected');
            const sole = Option.filter(Array.head(tools), () => aggregates.every(({ tools: members }) => members.length === 1));
            const target = Option.flatMap(many ? Array.head(nx.values('-t')) : Option.none(), ({ node }) => node);
            const address = Option.all([Option.flatMap(Array.get(nx.operands, 0), ({ word }) => word.node), Option.flatMap(Array.get(nx.operands, 1), ({ word }) => word.node)]);
            return Option.match(Array.head(aggregates), {
                onNone: () => [],
                onSome: ({ project, target: name }) =>
                    Match.value({ tool: Option.getOrUndefined(sole), target: Option.getOrUndefined(target), address: Option.getOrUndefined(address) }).pipe(
                        Match.when({ tool: Match.string, target: Match.defined }, ({ tool, target: word }): readonly Verdict[] => [{ edit: (editor) => [editor.replace(word, [tool])] }]),
                        Match.when({ tool: Match.string, address: Match.defined }, ({ tool, address: span }): readonly Verdict[] => (many ? [] : [{ edit: (editor) => [editor.replace(span, ['run', `${project}:${tool}`])] }])),
                        Match.orElse((): readonly Verdict[] => [{ deny: `nx ${name} runs ${tools.length} tools, so arguments after -- reach none of them.`, advice: `Pass them to one of: ${tools.map((tool) => `nx run ${project}:${tool}`).join(', ')}.` }]),
                    ),
            });
        }),
    },
    'nx.loop': {
        program: 'nx',
        decide: Effect.fnUntraced(function* (nx, { invocations, script, shell }) {
            const runs = invocations.filter((other) => other.runs('nx') && Option.isSome(nx.repeated) && Equal.equals(other.repeated, nx.repeated));
            const spelled = Option.getOrElse(Option.all(runs.map(({ argv }) => argv)), Array.empty<readonly string[]>);
            const [first = [], ...rest] = spelled;
            const varying = first.flatMap((word, at) => (rest.some((other) => other[at] !== word) ? [at] : []));
            const items = Array.dedupe(spelled.map((words) => words[varying[0] ?? 0] ?? ''));
            const verb = first[1] ?? '';
            const single = varying.length === 1 && rest.every((other) => other.length === first.length) && verb !== 'show' && verb !== 'graph' && !first.includes('--graph');
            const plan = Match.value({ verb, at: single ? varying[0] : undefined }).pipe(
                Match.when({ at: Match.undefined }, () => Option.none()),
                Match.when({ verb: 'run', at: 2 }, () => Option.some({ targets: Array.dedupe(items.map((item) => item.split(':')[1] ?? '')), projects: Array.dedupe(items.map((item) => item.split(':')[0] ?? '')) })),
                Match.when({ verb: 'run' }, () => Option.none()),
                Match.when({ at: 2 }, () => Option.some({ targets: [verb], projects: items })),
                Match.when({ at: 1 }, () => Option.some({ targets: items, projects: [first[2] ?? ''] })),
                Match.orElse(() => Option.none()),
            );
            const folded = Option.filter(Option.all([plan, loopOf(script, nx)]), ([{ targets, projects }]) => nx === runs[0] && targets.length * projects.length === items.length);
            const command = yield* Effect.transposeOption(Option.map(folded, ([{ targets, projects }]) => shell.script(script.dialect)`nx run-many -t ${targets.join(',')} -p ${projects.join(',')} ${first.slice(3)}`));
            return Option.toArray(Option.all([command, folded])).flatMap(([many, [, stmt]]) => [{ edit: (editor) => [editor.command(stmt, many)] }, { note: 'A loop over nx became one nx run-many, which shares the cache and parallelism of one run.', once: 'agent' as const }]);
        }),
    },
    'nx.target': {
        action: 'Shell',
        decide: Effect.fnUntraced(function* (_, { invocations, action, cwd }) {
            const raw = invocations.filter((invocation) => action.run !== 'monitored' && isRaw(invocation, boundedPrograms) && invocation.rest.length === 0 && !Option.exists(invocation.stage, ({ index, count }) => index < count - 1));
            const verdicts = yield* Effect.forEach(
                raw.map((invocation) => [invocation, cwdOf(invocation, cwd)] as const),
                ([invocation, folder]) => Effect.flatMap(readGraph(folder), Option.match({ onNone: () => Effect.succeed<readonly Verdict[]>([]), onSome: (graph) => route(invocation, graph, folder) })),
            );
            return verdicts.flat();
        }),
    },
    'hk.step': {
        action: 'Shell',
        decide: Effect.fnUntraced(function* (_, { invocations, action, cwd }) {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const raw = invocations.filter((invocation) => action.run !== 'monitored' && isRaw(invocation, launcherPrograms) && Option.isNone(invocation.stage));
            const root = Array.isReadonlyArrayNonEmpty(raw) ? yield* findUp(cwd, '.git') : Option.none();
            const declared = yield* Option.match(root, { onNone: () => Effect.succeed(''), onSome: (folder) => Effect.orElseSucceed(fs.readFileString(path.join(folder, '.config/hk.pkl')), () => '') });
            const named = raw.filter((invocation) => Option.exists(invocation.name, (name) => declared.includes(`"${name}"`)));
            const steps = yield* Option.match(
                Option.filter(root, () => Array.isReadonlyArrayNonEmpty(named)),
                { onNone: () => Effect.succeed<Steps>({}), onSome: readSteps },
            );
            const verdicts = yield* Effect.forEach(Array.cartesian(Option.toArray(root), named), ([folder, invocation]) => routeStep(invocation, steps, folder, cwdOf(invocation, cwd)));
            return verdicts.flat();
        }),
    },
    'cargo.fmt': {
        program: 'cargo',
        aliases: ['cargo-fmt'],
        decide: Effect.fnUntraced(function* (cargo, call) {
            const cwd = cwdOf(cargo, call.cwd);
            const formats = cargo.runs('cargo-fmt') || cargo.path[0] === 'fmt' || Option.contains(cargo.operand(), 'fmt');
            const mode = cargo.has('--check') ? 'check' : 'fix';
            const root = formats ? yield* findUp(cwd, '.git') : Option.none();
            const steps = yield* Option.match(root, { onNone: () => Effect.succeed<Steps>({}), onSome: readSteps });
            const step = Array.findFirst(Record.toEntries(steps), ([, modes]) => argvOf(modes[mode])[0] === 'rustfmt');
            const folders = Option.isSome(step) ? yield* resolveCrates(cargo, cwd) : Option.none();
            const known = cargo.only('--check', '-p', '--package', '--all') && cargo.rest.length === 0 && cargo.operands.length <= 1;
            return Match.value({ step: Option.getOrUndefined(step), routed: Option.getOrUndefined(Option.all([step, folders, extentOf(cargo)])) }).pipe(
                Match.when({ routed: Match.defined }, ({ routed: [[name], crates, span] }): readonly Verdict[] =>
                    known ? [{ edit: (editor) => [editor.replace(span, Array.make('hk', mode, '--step', name, ...crates))] }] : [{ deny: 'cargo fmt option has no hk form.', advice: `Run hk ${mode} --step ${name} ${crates.join(' ')}.` }],
                ),
                Match.when({ step: Match.defined }, (): readonly Verdict[] => [{ deny: 'cargo fmt resolves no crate here.', advice: 'Pass --package <crate>, or run it in the crate folder.' }]),
                Match.orElse((): readonly Verdict[] => []),
            );
        }),
    },
    'pulumi.deploy': {
        program: 'pulumi',
        decide: Effect.fnUntraced(function* (pulumi, call) {
            const cwd = cwdOf(pulumi, call.cwd);
            const verb = Option.getOrElse(
                Option.orElse(Array.head(pulumi.path), () => pulumi.operand()),
                () => '',
            );
            const operation = Record.get(stackOperations, verb);
            const graph = Option.isSome(operation) ? yield* readGraph(cwd) : Option.none();
            return Option.toArray(Option.all([operation, graph])).flatMap(([{ aggregate, args, applies }, found]): readonly Verdict[] => {
                const target = `${aggregate}--pulumi`;
                const deployable = Array.dedupe(
                    found.tasks
                        .values()
                        .filter((row) => row.target === target)
                        .map(({ project }) => project),
                );
                const owner = Option.firstSomeOf([Option.filter(found.owner(cwd), (project) => deployable.includes(project)), Option.filter(Array.head(deployable), () => deployable.length === 1)]);
                const project = Option.getOrElse(owner, () => `<${deployable.join('|')}>`);
                const stack = Option.flatMap(Array.head(pulumi.values('--stack')), ({ text }) => text);
                const words = Array.isReadonlyArrayEmpty(args) && Option.isNone(stack) ? Array.make('nx', aggregate, project) : Array.make('nx', 'run', `${project}:${target}`, ...args, ...Option.toArray(Option.map(stack, (name) => `--stack=${name}`)));
                const derived = Option.isSome(owner) && pulumi.only(...stackFlags) && (!applies || (pulumi.has('--yes', '-y', '--skip-preview') && Option.isSome(stack)));
                return derived ? Option.toArray(pulumi.span).map((span) => ({ edit: (editor) => [editor.replace(span, words)] })) : [{ deny: `pulumi ${verb} skips the stack programs Nx builds first.`, advice: `Run ${words.join(' ')}.` }];
            });
        }),
    },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { tasks };
