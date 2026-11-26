import type { EngineInterface, On } from 'claude-code';

// --- [TYPES] ---------------------------------------------------------------------------

interface Folder {
    readonly index: string;
    readonly root: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const INDEX = 'MEMORY.md';
const EXTENSION = '.md';
const SCRIPT = 'skills/memory/scripts/index.py';
const LIMIT = 120_000;
const GENERATED = `After writing the file, stop: \`${INDEX}\` is generated from each memory file's \`description\` after every write, one line per file, and loaded into context each session, so never write or edit \`${INDEX}\` itself. Keep each \`description\` to one specific line of at most 150 characters, as it is that file's line in the index.`;
const REFUSAL = `\`${INDEX}\` is generated from each memory file's \`description\` after every write: edit the \`description\` of the memory file instead, and its index line follows.`;
const POINTER: readonly (readonly [RegExp, string])[] = [
    [new RegExp(`^After writing the file, add a one-line pointer in \`${RegExp.escape(INDEX)}\`.*$`, 'mu'), GENERATED],
    [/^Saving a memory is a two-step process:\n\n\*\*Step 1\*\* — write /mu, 'Write '],
    [/^\*\*Step 2\*\* — add a pointer to that file in .*$/mu, GENERATED],
    [new RegExp(`^- \`${RegExp.escape(INDEX)}\` is always loaded into your conversation context.*\\n`, 'mu'), ''],
];
const INDEXED = new RegExp(`${RegExp.escape(INDEX)}$`, 'u');
const MARKDOWN = new RegExp(`${RegExp.escape(EXTENSION)}$`, 'u');

// --- [OPERATIONS] ----------------------------------------------------------------------

function real($: EngineInterface, path: unknown) {
    return typeof path === 'string'
        ? $.fs
              .stat(path, { resolve: true })
              .then(({ realPath }) => realPath ?? path)
              .catch(() => path)
        : Promise.resolve(undefined);
}

function signature($: EngineInterface, root: string) {
    return $.fs.list(root).then((entries) =>
        entries
            .filter(({ kind, name }) => kind === 'file' && name.endsWith(EXTENSION) && name !== INDEX)
            .map(({ name, size, mtimeMs }) => `${name}:${size}:${mtimeMs}`)
            .join('\n'),
    );
}

async function regenerate($: EngineInterface, at: Folder) {
    const { exitCode, stdout, stderr } = await $.process.run(['uv', 'run', '--locked', '--script', `${$.plugin.root}/${SCRIPT}`, at.root], { env: await $.process.hookEnv(), timeoutMs: LIMIT });
    return { failed: exitCode !== 0, report: `${stdout}${stderr}`.trim(), seen: await signature($, at.root) };
}

// --- [COMPOSITION] ---------------------------------------------------------------------

function register(on: On) {
    const state: { folder?: Folder; seen: string; queue: Promise<unknown> } = { seen: '', queue: Promise.resolve() };
    const settle = (run: () => Promise<Awaited<ReturnType<typeof regenerate>>>) => {
        const done = state.queue.then(run).then((ran) => {
            state.seen = ran.seen;
            return ran;
        });
        state.queue = done.catch(() => undefined);
        return done;
    };
    on('prompt.section', { name: 'memory' }, (_, e, next) => next(e).then((answer) => ({ ...answer, text: answer.text === null ? null : POINTER.reduce((text, [pattern, replacement]) => text.replace(pattern, replacement), answer.text) })));
    on('prompt.context', async ($, e, next) => {
        const answer = await next(e);
        const files = answer.instructionFiles ?? e.instructionFiles ?? [];
        const file = files.find(({ kind }) => kind === 'memory');
        if (file === undefined) {
            return answer;
        }
        const index = (await real($, file.path)) ?? file.path;
        const at = { index, root: index.slice(0, -INDEX.length) };
        state.folder = at;
        const { failed, report } = await settle(() => regenerate($, at));
        const content = await $.fs.read(file.path);
        return { blocks: failed ? [...answer.blocks, { name: 'memoryIndex', text: report }] : answer.blocks, instructionFiles: files.map((row) => (row === file ? { ...row, content } : row)) };
    });
    on('tool.call', { tool: ['Write', 'Edit'], file_path: INDEXED }, async ($, e, next) => {
        const { file_path: path } = e;
        const target = await real($, path);
        return target !== undefined && target === state.folder?.index ? { deny: REFUSAL } : next(e);
    });
    on('tool.run', { tool: ['Write', 'Edit'], file_path: MARKDOWN }, async ($, e, next) => {
        const answer = await next(e);
        const { file_path: path } = e;
        const [target, at] = [await real($, path), state.folder];
        if (answer.deny !== undefined || at === undefined || target?.startsWith(at.root) !== true) {
            return answer;
        }
        const { report } = await settle(() => regenerate($, at));
        $.ui.invalidate('prompt.context');
        return { ...answer, context: [...(answer.context ?? []), report] };
    });
    on('turn.complete', async ($, e, next) => {
        const answer = await next(e);
        const at = state.folder;
        if (at === undefined || (await signature($, at.root)) === state.seen) {
            return answer;
        }
        const { report } = await settle(() => regenerate($, at));
        $.ui.invalidate('prompt.context');
        await $.session.append({ message: { type: 'user', content: [{ type: 'text', text: report }] } });
        return answer;
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
