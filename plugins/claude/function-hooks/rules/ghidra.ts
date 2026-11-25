import { Array, Effect, FileSystem, Glob, Option, Path, Struct } from 'effect';
import { type Editor, type Invocation, nearest, type Script, type Table } from '#/rules/rule';
import { unattended } from '#/rules/waits';

// --- [CONSTANTS] -----------------------------------------------------------------------

const headless = 'analyzeHeadless';
const heap = /^GHIDRA_(?:HEADLESS_)?MAXMEM$/u;
const project = /\.(?:gpr|rep)(?:\/|$)/u;
const logs = [
    ['-log', 'ghidra.log'],
    ['-scriptlog', 'ghidra-script.log'],
] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const heaped = (stmt: Option.Option.Value<Effect.Success<Script['invocations']>[number]['node']>): readonly Parameters<Editor['remove']>[0][] => {
    const assigns = stmt.cmd?.type === 'CallExpr' ? stmt.cmd.assigns : [];
    const declared = stmt.cmd?.type === 'DeclClause' ? stmt.cmd.args : [];
    const held = [...assigns, ...declared].filter(({ name }) => heap.test(name?.value ?? ''));
    return Array.isReadonlyArrayNonEmpty(declared) && held.length === declared.length ? [stmt] : held;
};
const concurrent = (all: readonly Invocation[]): boolean => {
    const runs = all.filter((invocation) => invocation.runs(headless));
    return runs.some(({ repeated }) => Option.isSome(repeated)) || (runs.length > 1 && all.some(({ background }) => background));
};
const projects = Effect.fnUntraced(function* (folder: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const entries = yield* Effect.orElseSucceed(fs.readDirectory(folder), (): readonly string[] => []);
    return entries.filter((entry) => entry.endsWith('.gpr')).map((entry) => path.join(folder, entry.slice(0, -'.gpr'.length)));
});

const ghidra: Table = {
    'ghidra.heap': {
        action: 'Shell',
        judge: Effect.fnUntraced(function* (script, { shell }) {
            const all = (yield* script.invocations).flatMap(Struct.get('leaves'));
            const stmts = Array.dedupe(all.flatMap(({ node }) => Option.toArray(node)));
            const removed = stmts.flatMap(heaped);
            const words = all.filter((invocation) => invocation.runs('env')).flatMap(({ operands }) => operands.filter(({ word, roles }) => roles.includes('assignment') && word.test(/^GHIDRA_(?:HEADLESS_)?MAXMEM=/u)).flatMap(({ word }) => Option.toArray(word.node)));
            const cleared = new Set(all.filter((invocation) => invocation.runs('unset')).flatMap(({ words: written }) => Option.getOrElse(Option.all(written.map(Struct.get('text'))), (): readonly string[] => [])));
            const inherited = Object.keys(script.dialect.environment).filter((name) => heap.test(name) && !cleared.has(name));
            const first = Array.findFirst(all, (invocation) => invocation.runs(headless) && invocation.within.length === 0);
            const unset = Array.isReadonlyArrayNonEmpty(inherited) ? Option.some(yield* shell.script(script.dialect)`unset ${inherited}`) : Option.none();
            const preceded = Option.all([unset, Option.flatMap(first, ({ node }) => node)]);
            return Option.isSome(first) && (Array.isReadonlyArrayNonEmpty(removed) || Array.isReadonlyArrayNonEmpty(words) || Option.isSome(preceded))
                ? [
                      { edit: (editor: Editor) => [...removed.map((target) => editor.remove(target)), ...words.map((word) => editor.remove(word)), ...Option.toArray(preceded).map(([built, stmt]) => editor.precede(built, ';', stmt))] },
                      { note: `GHIDRA_MAXMEM left the call, as ${headless} sizes its own heap.`, once: 'agent' as const },
                  ]
                : [];
        }),
    },
    'ghidra.project': {
        program: headless,
        judge: Effect.fnUntraced(function* (invocation, { script, cwd }) {
            const path = yield* Path.Path;
            const folder = Option.map(Option.fromUndefinedOr(script.dialect.environment['GHIDRA_PROJECT_DIR']), (given) => path.resolve(given));
            const location = Option.all([Option.flatMap(Array.head(invocation.operands), ({ word }) => word.node), invocation.operand()]);
            const chained = invocation.background && concurrent((yield* script.invocations).flatMap(Struct.get('leaves')));
            return Option.match(
                Option.filter(Option.all([folder, location]), ([shared, [, text]]) => !chained && path.resolve(cwd, text) !== shared),
                {
                    onNone: () => [],
                    onSome: ([shared, [word, text]]) => [{ edit: (editor: Editor) => [editor.replace(word, Array.of(shared))] }, { note: `${headless} ${text} became ${shared}, the project folder every session shares.`, once: 'agent' as const }],
                },
            );
        }),
    },
    'ghidra.logs': {
        program: headless,
        judge: Effect.fnUntraced(function* (invocation, { cwd, script }) {
            const root = yield* nearest(cwd, '.git');
            const chained = invocation.background && concurrent((yield* script.invocations).flatMap(Struct.get('leaves')));
            const words = Option.getOrElse(Option.all(invocation.words.map(Struct.get('text'))), (): readonly string[] => []);
            const last = Option.flatMap(Array.last(invocation.words), ({ node }) => node);
            const wanted = Option.toArray(root).flatMap((folder) => logs.map(([flag, file]) => ({ flag, log: `${folder}/.artifacts/logs/${file}`, at: words.indexOf(flag) })));
            const added = wanted.filter(({ at }) => at === -1).flatMap(({ flag, log }) => [flag, log]);
            const moved = wanted.filter(({ at, log }) => at >= 0 && words[at + 1] !== log).flatMap(({ at, log }) => Option.toArray(Option.flatMap(Array.get(invocation.words, at + 1), ({ node }) => node)).map((word) => [word, log] as const));
            return Option.match(
                Option.filter(last, () => !chained && invocation.operands.length > 0 && (Array.isReadonlyArrayNonEmpty(added) || Array.isReadonlyArrayNonEmpty(moved))),
                {
                    onNone: () => [],
                    onSome: (word) => [{ edit: (editor: Editor) => [...moved.map(([target, log]) => editor.replace(target, Array.of(log))), ...Array.match(added, { onEmpty: () => [], onNonEmpty: (given) => [editor.after(word, given)] })] }],
                },
            );
        }),
    },
    'ghidra.concurrent': {
        action: 'Shell',
        judge: Effect.fnUntraced(function* (script, { shell }) {
            const invocations = yield* script.invocations;
            const all = invocations.flatMap(Struct.get('leaves'));
            const runs = all.filter((invocation) => invocation.runs(headless));
            const waits = all.filter((invocation) => invocation.runs('wait') && invocation.operands.length === 0);
            const importing = runs.every((run) => run.has('-import'));
            const chainable = importing && runs.every(({ background, repeated, within, assigns, node }) => background && Option.isNone(repeated) && within.length === 0 && assigns.length === 0 && Option.exists(node, ({ redirs }) => redirs.length === 0));
            const spelled = Option.all(runs.map((run) => Option.all(run.words.map(Struct.get('text')))));
            const plan = Option.filter(Option.all([spelled, Option.flatMap(Array.last(waits), ({ node }) => node)]), () => chainable && waits.length === 1 && invocations.filter(({ background }) => background).length === runs.length);
            const [first, ...others] = Option.isSome(plan) ? yield* Effect.forEach(plan.value[0], (line) => shell.script(script.dialect)`${line}`) : [];
            const opening = Option.flatMap(Option.fromUndefinedOr(first), (start) => Option.map(Array.head(start.syntax.stmts), (stmt) => [start, stmt] as const));
            const built = Option.isSome(opening) ? Option.some(yield* shell.edit(opening.value[0], (editor) => others.map((next) => editor.follow(opening.value[1], '&&', next)))) : Option.none();
            const socket = Option.getOrElse(
                Option.flatMap(Array.head(runs), (run) => run.operand(1)),
                () => '<name>',
            );
            return Option.match(Option.all([built, plan]), {
                onSome: ([chain, [, wait]]) => [
                    { edit: (editor: Editor) => [...runs.flatMap(({ node }) => Option.toArray(node)).map((stmt) => editor.remove(stmt)), editor.command(wait, chain)] },
                    { note: `${headless} runs in the background on one project became one chain of -import runs, as Ghidra locks a project to one run.`, once: 'agent' as const },
                ],
                onNone: () => (concurrent(all) ? [{ refuse: `${headless} runs concurrently on one project, and Ghidra locks a project to one run.`, instruct: importing ? 'Chain each -import with &&.' : `Send the runs to the project's Serve.java with nc -U "$GHIDRA_PROJECT_DIR/${socket}.sock".` }] : []),
            });
        }),
    },
    'ghidra.long': {
        program: headless,
        judge: (invocation) => {
            const words = Option.getOrElse(Option.all(invocation.words.map(Struct.get('text'))), (): readonly string[] => []);
            return invocation.has('-import') || words.some((word, at) => words[at - 1] === '-postScript' && word.endsWith('Serve.java')) ? [unattended] : [];
        },
    },
    'ghidra.copy': {
        program: 'cp',
        aliases: ['rsync', 'ditto'],
        judge: Effect.fnUntraced(function* (invocation) {
            const path = yield* Path.Path;
            const sources = Array.dedupe(invocation.reads.flatMap(({ text }) => Option.toArray(text)));
            const found = yield* Effect.forEach(sources, projects);
            const held = Array.zipWith(sources, found, (source, bases) => [...(project.test(source) ? [source] : []), ...bases.map((base) => `${base}.gpr`)]);
            return Array.dedupe(held.flat()).map((file) => ({
                refuse: `${Option.getOrElse(invocation.name, () => 'cp')} copies the Ghidra project ${path.basename(file)}, which a running Ghidra rewrites under its lock.`,
                instruct: "Read it through the project's Serve.java.",
            }));
        }),
    },
    'ghidra.lock': {
        action: 'Shell',
        judge: Effect.fnUntraced(function* (script) {
            const path = yield* Path.Path;
            const all = (yield* script.invocations).flatMap(Struct.get('leaves'));
            const fields = all.flatMap((invocation) => {
                const globs = invocation.runs('find') ? Array.getSomes(invocation.values('-name', '-iname').map(({ field }) => field.text)).map((name) => Glob.make(name)) : [];
                return invocation.deletes.map((field) => ({ field, globs }));
            });
            const folders = Array.dedupe(fields.map(({ field }) => Option.match(field.text, { onSome: (text) => path.dirname(text), onNone: () => (field.prefix.endsWith('/') ? path.resolve(field.prefix) : path.dirname(field.prefix)) })));
            const bases = (yield* Effect.forEach(folders, projects)).flat();
            const locks = bases.flatMap((base) => [`${base}.lock`, `${base}.lock~`]);
            const deleted = Array.dedupe(
                Array.cartesian(locks, fields)
                    .filter(([lock, { field, globs }]) => Option.getOrElse(field.matches(lock), () => false) && globs.every((glob) => Glob.match(glob, path.basename(lock))))
                    .map(([lock]) => lock),
            );
            return deleted.map((file) => ({ refuse: `Deleting ${file} drops the lock of the Ghidra project ${file.replace(/\.lock~?$/u, '.gpr')} while a run may hold it.`, instruct: `Run lsof -t ${file} to find the run that holds it.` }));
        }),
    },
} satisfies Table;

// --- [EXPORTS] -------------------------------------------------------------------------

export { ghidra };
