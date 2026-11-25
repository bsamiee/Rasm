import { Array, Effect, FileSystem, Option, Path } from 'effect';
import { type Editor, foreground, is, leaves, matches, named, nearest, operand, type Script, type Table, texts } from '#/rules/rule';

// --- [CONSTANTS] -----------------------------------------------------------------------

const headless = 'analyzeHeadless';
const heap = /^GHIDRA_(?:HEADLESS_)?MAXMEM$/u;
const project = /\.(?:gpr|rep)(?:\/|$)/u;
const lock = /^(?<stem>[^/]+?)\.?lock[*~]*$/u;
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

const ghidra: Table = {
    'ghidra.heap': {
        script: true,
        judge: Effect.fnUntraced(function* (script) {
            const all = leaves(yield* script.invocations);
            const stmts = Array.dedupe(all.flatMap(({ node }) => Option.toArray(node)));
            const removed = stmts.flatMap(heaped);
            const words = all.filter((invocation) => is(invocation, 'env')).flatMap(({ operands }) => operands.filter(({ word, roles }) => roles.includes('assignment') && matches(word, /^GHIDRA_(?:HEADLESS_)?MAXMEM=/u)).flatMap(({ word }) => Option.toArray(word.node)));
            const runs = all.some((invocation) => is(invocation, headless));
            return runs && (Array.isReadonlyArrayNonEmpty(removed) || Array.isReadonlyArrayNonEmpty(words))
                ? [{ edit: (editor: Editor) => [...removed.map((target) => editor.remove(target)), ...words.map((word) => editor.remove(word))] }, { note: `GHIDRA_MAXMEM left the call, as ${headless} sizes its own heap.`, once: 'agent' as const }]
                : [];
        }),
    },
    'ghidra.project': {
        program: headless,
        judge: Effect.fnUntraced(function* (invocation, { script, cwd }) {
            const path = yield* Path.Path;
            const folder = Option.map(Option.fromUndefinedOr(script.dialect.environment['GHIDRA_PROJECT_DIR']), (given) => path.resolve(given));
            const location = Option.all([Option.flatMap(Array.head(invocation.operands), ({ word }) => word.node), operand(invocation)]);
            return Option.match(
                Option.filter(Option.all([folder, location]), ([shared, [, text]]) => path.resolve(cwd, text) !== shared),
                {
                    onNone: () => [],
                    onSome: ([shared, [word, text]]) => [{ edit: (editor: Editor) => [editor.replace(word, Array.of(shared))] }, { note: `${headless} ${text} became ${shared}, the project folder every session shares.`, once: 'agent' as const }],
                },
            );
        }),
    },
    'ghidra.logs': {
        program: headless,
        judge: Effect.fnUntraced(function* (invocation, { cwd }) {
            const root = yield* nearest(cwd, '.git');
            const words = Option.getOrElse(texts(invocation.words), (): readonly string[] => []);
            const last = Option.flatMap(Array.last(invocation.words), ({ node }) => node);
            const wanted = Option.toArray(root).flatMap((folder) => logs.map(([flag, file]) => ({ flag, log: `${folder}/.artifacts/logs/${file}`, at: words.indexOf(flag) })));
            const added = wanted.filter(({ at }) => at === -1).flatMap(({ flag, log }) => [flag, log]);
            const moved = wanted.filter(({ at, log }) => at >= 0 && words[at + 1] !== log).flatMap(({ at, log }) => Option.toArray(Option.flatMap(Array.get(invocation.words, at + 1), ({ node }) => node)).map((word) => [word, log] as const));
            return Option.match(
                Option.filter(last, () => invocation.operands.length > 0 && (Array.isReadonlyArrayNonEmpty(added) || Array.isReadonlyArrayNonEmpty(moved))),
                {
                    onNone: () => [],
                    onSome: (word) => [{ edit: (editor: Editor) => [...moved.map(([target, log]) => editor.replace(target, Array.of(log))), ...Array.match(added, { onEmpty: () => [], onNonEmpty: (given) => [editor.after(word, given)] })] }],
                },
            );
        }),
    },
    'ghidra.concurrent': {
        script: true,
        judge: Effect.fnUntraced(function* (script) {
            const invocations = yield* script.invocations;
            const runs = leaves(invocations).filter((invocation) => is(invocation, headless));
            const concurrent = runs.some(({ repeated }) => Option.isSome(repeated)) || (runs.length > 1 && invocations.some(({ background }) => background));
            const importing = runs.some((invocation) => Option.exists(texts(invocation.words), (words) => words.includes('-import')));
            const socket = Option.getOrElse(
                Option.flatMap(Array.head(runs), (run) => operand(run, 1)),
                () => '<name>',
            );
            return concurrent ? [{ refuse: `${headless} runs concurrently on one project, and Ghidra locks a project to one run.`, instruct: importing ? 'Chain each -import with &&.' : `Send the runs to the project's Serve.java with nc -U "$GHIDRA_PROJECT_DIR/${socket}.sock".` }] : [];
        }),
    },
    'ghidra.long': {
        program: headless,
        judge: (invocation, call) => {
            const words = Option.getOrElse(texts(invocation.words), (): readonly string[] => []);
            const long = words.includes('-import') || words.some((word, at) => words[at - 1] === '-postScript' && word.endsWith('Serve.java'));
            return foreground(call) && long ? [{ run: 'background' as const }] : [];
        },
    },
    'ghidra.copy': {
        program: 'cp',
        aliases: ['rsync', 'ditto'],
        judge: (invocation) =>
            Array.dedupe(invocation.reads.flatMap(({ text }) => Option.toArray(text)).filter((file) => project.test(file))).map((file) => ({
                refuse: `${Option.getOrElse(named(invocation), () => 'cp')} copies the Ghidra project ${file}, which a running Ghidra rewrites under its lock.`,
                instruct: "Read it through the project's Serve.java.",
            })),
    },
    'ghidra.lock': {
        script: true,
        judge: Effect.fnUntraced(function* (script) {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const deleted = Array.dedupe(leaves(yield* script.invocations).flatMap(({ deletes }) => deletes.flatMap(({ text }) => Option.toArray(text))));
            const locks = deleted.flatMap((file) => Array.fromNullishOr(lock.exec(path.basename(file))?.groups?.['stem']).map((stem) => ({ file, gpr: path.join(path.dirname(file), `${stem.replace(/\.gpr$/u, '')}.gpr`) })));
            const held = yield* Effect.filter(locks, ({ gpr }) => Effect.orElseSucceed(fs.exists(gpr), () => false));
            return held.map(({ file, gpr }) => ({ refuse: `Deleting ${file} drops the lock of the Ghidra project ${gpr} while a run may hold it.`, instruct: `Run lsof -t ${file} to find the run that holds it.` }));
        }),
    },
} satisfies Table;

// --- [EXPORTS] -------------------------------------------------------------------------

export { ghidra };
