import type { EngineInterface, On } from 'claude-code';

// --- [CONSTANTS] -----------------------------------------------------------------------

const INDEX = 'MEMORY.md';
const MARKDOWN = /\.md$/u;
const GENERATED = `After writing the file, stop: \`${INDEX}\` is generated from each memory file's \`description\` after every write, one line per file, and loaded into context each session, so never write or edit \`${INDEX}\` itself. Keep each \`description\` to one specific line of at most 150 characters, as it is that file's line in the index.`;
const POINTER: readonly (readonly [RegExp, string])[] = [
    [new RegExp(`^(?:After writing the file, add a one-line pointer in \`${RegExp.escape(INDEX)}\`|\\*\\*Step 2\\*\\* — add a pointer to that file in ).*$`, 'mu'), GENERATED],
    [/^Saving a memory is a two-step process:\n\n\*\*Step 1\*\* — write /mu, 'Write '],
    [new RegExp(`^- \`${RegExp.escape(INDEX)}\` is always loaded into your conversation context.*\\n`, 'mu'), ''],
];

// --- [OPERATIONS] ----------------------------------------------------------------------

function real($: EngineInterface, path: string) {
    return $.fs
        .stat(path, { resolve: true })
        .then(({ realPath }) => realPath ?? path)
        .catch(() => path);
}

function dirty($: EngineInterface, root: string) {
    return $.process.run(['git', '-C', root, 'status', '--porcelain']).then(({ exitCode, stdout }) => exitCode !== 0 || stdout.trim() !== '');
}

async function regenerate($: EngineInterface, root: string) {
    const { exitCode, stdout, stderr } = await $.process.run(['uv', 'run', '--locked', '--script', `${$.plugin.root}/skills/memory/scripts/index.py`, root], { env: await $.process.hookEnv(), timeoutMs: 120_000 });
    return { failed: exitCode !== 0, report: `${stdout}${stderr}`.trim() };
}

// --- [COMPOSITION] ---------------------------------------------------------------------

function register(on: On) {
    const state: { root?: string; queue: Promise<unknown> } = { queue: Promise.resolve() };
    const settle = <A>(run: () => Promise<A>) => {
        const done = state.queue.then(run);
        state.queue = done.catch(() => undefined);
        return done;
    };
    on('prompt.section', { name: 'memory' }, (_, e, next) =>
        next(e).then((answer) => {
            const text = answer.text === null ? null : POINTER.reduce((swapped, [pattern, replacement]) => swapped.replace(pattern, replacement), answer.text);
            return { ...answer, text: text !== null && text === answer.text ? `${text}\n\nThis Claude Code release words the memory section above in a form the dreamweave plugin does not recognize, so its steps may still name \`${INDEX}\`. ${GENERATED}` : text };
        }),
    );
    on('prompt.context', async ($, e, next) => {
        const answer = await next(e);
        const files = answer.instructionFiles ?? e.instructionFiles ?? [];
        const file = files.find(({ kind }) => kind === 'memory');
        if (file === undefined) {
            return answer;
        }
        const index = await real($, file.path);
        const root = index.slice(0, -INDEX.length);
        state.root = root;
        const { failed, report } = await settle(() => regenerate($, root));
        const content = await $.fs.read(file.path);
        return { blocks: failed ? [...answer.blocks, { name: 'memoryIndex', text: report }] : answer.blocks, instructionFiles: files.map((row) => (row === file ? { ...row, content } : row)) };
    });
    on('tool.call', { tool: ['Write', 'Edit'], file_path: new RegExp(`${RegExp.escape(INDEX)}$`, 'u') }, async ($, e, next) => {
        const path = e['file_path'];
        const target = typeof path === 'string' ? await real($, path) : undefined;
        return state.root !== undefined && target === `${state.root}${INDEX}` ? { deny: `\`${INDEX}\` is generated from each memory file's \`description\` after every write: edit the \`description\` of the memory file instead, and its index line follows.` } : next(e);
    });
    on('tool.run', { tool: ['Write', 'Edit'], file_path: MARKDOWN }, async ($, e, next) => {
        const [answer, root, path] = [await next(e), state.root, e['file_path']];
        const target = typeof path === 'string' ? await real($, path) : undefined;
        if (answer.deny !== undefined || root === undefined || target?.startsWith(root) !== true) {
            return answer;
        }
        const { report } = await settle(() => regenerate($, root));
        $.ui.invalidate('prompt.context');
        return { ...answer, context: [...(answer.context ?? []), report] };
    });
    on('turn.complete', async ($, e, next) => {
        const [answer, root] = [await next(e), state.root];
        if (root === undefined || !(await settle(() => dirty($, root)))) {
            return answer;
        }
        const { report } = await settle(() => regenerate($, root));
        $.ui.invalidate('prompt.context');
        await $.session.append({ message: { type: 'user', content: [{ type: 'text', text: report }] } });
        return answer;
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
