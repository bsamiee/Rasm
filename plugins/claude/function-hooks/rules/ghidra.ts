import { Array, Effect, FileSystem, Glob, Option, Path, Record } from 'effect';
import type { Assign, Stmt } from 'sh-syntax';
import { type Call, findUp, type Invocation, type Table } from '#/rules/rule';
import { moves } from '#/rules/waits';

// --- [CONSTANTS] -----------------------------------------------------------------------

const headless = 'analyzeHeadless';
const heapSetting = /^GHIDRA_(?:HEADLESS_)?MAXMEM(?:=|$)/u;
const projectFile = /\.(?:gpr|rep)(?:\/|$)/u;
const logs = [
    ['-log', 'ghidra.log'],
    ['-scriptlog', 'ghidra-script.log'],
] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const setsHeap = ({ name }: Assign) => heapSetting.test(name?.value ?? '');
const findHeapSettings = (stmt: Stmt) => {
    const declared = stmt.cmd?.type === 'DeclClause' ? stmt.cmd.args : [];
    const [prefixed, exported] = [(stmt.cmd?.type === 'CallExpr' ? stmt.cmd.assigns : []).filter(setsHeap), declared.filter(setsHeap)];
    const whole = Array.isReadonlyArrayNonEmpty(declared) && exported.length === declared.length;
    return { removed: whole ? [stmt] : prefixed, kept: whole ? [] : exported.flatMap(({ name }) => Array.fromNullishOr(name?.value)) };
};
const overlaps = (invocations: readonly Invocation[]): boolean => {
    const runs = invocations.filter((invocation) => invocation.runs(headless));
    return runs.some(({ repeated }) => Option.isSome(repeated)) || (runs.length > 1 && invocations.some(({ background }) => background));
};
const listProjects = Effect.fnUntraced(function* (folder: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const entries = yield* Effect.orElseSucceed(fs.readDirectory(folder), Array.empty<string>);
    return entries.filter((entry) => entry.endsWith('.gpr')).map((entry) => path.join(folder, entry.slice(0, -'.gpr'.length)));
});
const chain = Effect.fnUntraced(function* ({ shell, script }: Call, lines: readonly (readonly string[])[]) {
    const [first, ...others] = yield* Effect.forEach(lines, (line) => shell.script(script.dialect)`${line}`);
    const start = yield* Effect.fromNullishOr(first);
    const stmt = yield* Effect.fromOption(Array.head(start.syntax.stmts));
    return yield* shell.edit(start, (editor) => others.map((next) => editor.follow(stmt, '&&', next)));
}, Effect.catchNoSuchElement);

// --- [RULES] ---------------------------------------------------------------------------

const ghidra: Table = {
    'ghidra.heap': {
        action: 'Shell',
        decide: Effect.fnUntraced(function* (script, { invocations, shell }) {
            const settings = Array.dedupe(invocations.flatMap(({ node }) => Option.toArray(node))).map(findHeapSettings);
            const removed = settings.flatMap((setting): readonly (Stmt | Assign)[] => setting.removed);
            const passed = invocations.filter((invocation) => invocation.runs('env')).flatMap(({ operands }) => operands.filter(({ word, roles }) => roles.includes('assignment') && word.test(heapSetting)).flatMap(({ word }) => Option.toArray(word.node)));
            const cleared = new Set(invocations.filter((invocation) => invocation.runs('unset')).flatMap(({ argv }) => Option.getOrElse(argv, Array.empty<string>)));
            const names = Array.dedupe([...Record.keys(script.dialect.environment).filter((name) => heapSetting.test(name)), ...settings.flatMap(({ kept }) => kept)]).filter((name) => !cleared.has(name));
            const first = Array.findFirst(invocations, (invocation) => invocation.runs(headless) && invocation.within.length === 0);
            const unset = yield* Effect.transposeOption(Option.map(Option.liftPredicate(names, Array.isArrayNonEmpty), (set) => shell.script(script.dialect)`unset ${set}`));
            const preceded = Option.all([unset, Option.flatMap(first, ({ node }) => node)]);
            return Option.isSome(first) && (Array.isReadonlyArrayNonEmpty(removed) || Array.isReadonlyArrayNonEmpty(passed) || Option.isSome(preceded))
                ? [{ edit: (editor) => [...[...removed, ...passed].map((setting) => editor.remove(setting)), ...Option.toArray(preceded).map(([command, stmt]) => editor.precede(command, ';', stmt))] }, { note: `GHIDRA_MAXMEM left the call, as ${headless} sizes its own heap.`, once: 'agent' as const }]
                : [];
        }),
    },
    'ghidra.project': {
        program: headless,
        decide: Effect.fnUntraced(function* (run, { invocations, script, cwd }) {
            const path = yield* Path.Path;
            const shared = Option.map(Record.get(script.dialect.environment, 'GHIDRA_PROJECT_DIR'), (folder) => path.resolve(folder));
            const location = Option.all([Option.flatMap(Array.head(run.operands), ({ word }) => word.node), run.operand()]);
            const moved = Option.filter(Option.all([shared, location]), ([folder, [, given]]) => !(run.background && overlaps(invocations)) && path.resolve(cwd, given) !== folder);
            return Option.toArray(moved).flatMap(([folder, [word, given]]) => [{ edit: (editor) => [editor.replace(word, [folder])] }, { note: `${headless} ${given} became ${folder}, the project folder every session shares.`, once: 'agent' as const }]);
        }),
    },
    'ghidra.logs': {
        program: headless,
        decide: Effect.fnUntraced(function* (run, { invocations, cwd }) {
            const files = Array.cartesian(Option.toArray(yield* findUp(cwd, '.git')), logs).map(([root, [flag, file]]) => ({ flag, file: `${root}/.artifacts/logs/${file}`, at: run.words.findIndex((word) => word.spells(flag)) }));
            const added = files.filter(({ at }) => at === -1).flatMap(({ flag, file }) => [flag, file]);
            const moved = files.flatMap(({ at, file }) =>
                Option.toArray(
                    Option.flatMap(
                        Option.filter(Array.get(run.words, at + 1), (value) => at >= 0 && !value.spells(file)),
                        ({ node }) => node,
                    ),
                ).map((word) => [word, file] as const),
            );
            const last = Option.filter(
                Option.map(run.span, ([, word]) => word),
                () => !(run.background && overlaps(invocations)) && run.operands.length > 0 && (Array.isReadonlyArrayNonEmpty(added) || Array.isReadonlyArrayNonEmpty(moved)),
            );
            return Option.toArray(last).map((word) => ({ edit: (editor) => [...moved.map(([value, file]) => editor.replace(value, [file])), ...Array.match(added, { onEmpty: () => [], onNonEmpty: (flags) => [editor.after(word, flags)] })] }));
        }),
    },
    'ghidra.concurrent': {
        action: 'Shell',
        decide: Effect.fnUntraced(function* (_, call) {
            const { invocations } = call;
            const runs = invocations.filter((invocation) => invocation.runs(headless));
            const waits = invocations.filter((invocation) => invocation.runs('wait') && invocation.operands.length === 0);
            const imports = runs.every((run) => run.has('-import'));
            const chainable = imports && runs.every(({ background, repeated, within, assigns, node }) => background && Option.isNone(repeated) && within.length === 0 && assigns.length === 0 && Option.exists(node, ({ redirs }) => redirs.length === 0));
            const lines = Option.filter(Option.all(runs.map(({ argv }) => argv)), () => chainable && waits.length === 1 && invocations.filter(({ background }) => background).length === runs.length);
            const commands = Option.flatten(yield* Effect.transposeOption(Option.map(lines, (given) => chain(call, given))));
            const wait = Option.flatMap(Array.last(waits), ({ node }) => node);
            const socket = Option.getOrElse(
                Option.flatMap(Array.head(runs), (run) => run.operand(1)),
                () => '<name>',
            );
            return Option.match(Option.all([commands, wait]), {
                onSome: ([chained, waited]) => [
                    { edit: (editor) => [...runs.flatMap(({ node }) => Option.toArray(node)).map((stmt) => editor.remove(stmt)), editor.command(waited, chained)] },
                    { note: `${headless} runs in the background on one project became one chain of -import runs, as Ghidra locks a project to one run.`, once: 'agent' as const },
                ],
                onNone: () => (overlaps(invocations) ? [{ deny: `${headless} runs concurrently on one project, and Ghidra locks a project to one run.`, advice: imports ? 'Chain each -import with &&.' : `Send the runs to the project's Serve.java with nc -U "$GHIDRA_PROJECT_DIR/${socket}.sock".` }] : []),
            });
        }),
    },
    'ghidra.long': {
        program: headless,
        decide: (run) => (run.has('-import') || run.words.some((word, at) => word.test(/Serve\.java$/u) && Option.exists(Array.get(run.words, at - 1), (flag) => flag.spells('-postScript'))) ? [moves.background] : []),
    },
    'ghidra.copy': {
        program: 'cp',
        aliases: ['rsync', 'ditto'],
        decide: Effect.fnUntraced(function* (copy) {
            const path = yield* Path.Path;
            const sources = Array.dedupe(Array.getSomes(copy.reads.map(({ text }) => text)));
            const inside = yield* Effect.forEach(sources, listProjects);
            const copied = Array.zipWith(sources, inside, (source, projects) => [...(projectFile.test(source) ? [source] : []), ...projects.map((project) => `${project}.gpr`)]);
            return Array.dedupe(copied.flat()).map((file) => ({ deny: `${copy.label} copies the Ghidra project ${path.basename(file)}, which a running Ghidra rewrites under its lock.`, advice: "Read it through the project's Serve.java." }));
        }),
    },
    'ghidra.lock': {
        action: 'Shell',
        decide: Effect.fnUntraced(function* (_, { invocations }) {
            const path = yield* Path.Path;
            const removals = invocations.flatMap((invocation) => {
                const names = invocation.runs('find') ? Array.getSomes(invocation.values('-name', '-iname').map(({ text }) => text)).map((name) => Glob.make(name)) : [];
                return invocation.deletes.map((field) => ({ field, names }));
            });
            const folders = Array.dedupe(removals.map(({ field }) => Option.match(field.text, { onSome: (text) => path.dirname(text), onNone: () => (field.prefix.endsWith('/') ? path.resolve(field.prefix) : path.dirname(field.prefix)) })));
            const projects = (yield* Effect.forEach(folders, listProjects)).flat();
            const locks = projects.flatMap((project) => [`${project}.lock`, `${project}.lock~`].map((lock) => ({ project, lock })));
            const deleted = Array.cartesian(locks, removals).filter(([{ lock }, { field, names }]) => Option.contains(field.matches(lock), true) && names.every((name) => Glob.match(name, path.basename(lock))));
            return Array.dedupeWith(deleted, ([left], [right]) => left.lock === right.lock).map(([{ project, lock }]) => ({ deny: `Deleting ${lock} drops the lock of the Ghidra project ${project}.gpr while a run may hold it.`, advice: `Run lsof -t ${lock} to find the run that holds it.` }));
        }),
    },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { ghidra };
