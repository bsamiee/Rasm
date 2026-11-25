import { atom, type BuiltinToolInputs, type EngineInterface, type Register, read, type SessionMessage, type ToolUseSummary, update } from 'claude-code';

// --- [TYPES] ---------------------------------------------------------------------------

type Question = BuiltinToolInputs['AskUserQuestion']['questions'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const KIND = /^[ \t]*(?<at>\d+)[^\w\n]+(?<kind>CONSENT|TASTE|WORK)\b/gmu;
const PICK = /\b(?:I (?:would |'d )?recommend|\(recommended\)|recommendation:|I(?: would|'d) (?:take|pick|go with|choose|keep)|my (?:pick|recommendation)|I suggest|the better (?:option|choice|form))\b/iu;
const LAST = 200;
const LEAD = 'varasi kept';
const UNCHECKED = 'varasi: the turn ends with nothing run since a question was kept from the person, so it is still unchecked. Asking it in prose skips the check. Do what the hold said, then ask it with the question tool.';
const HOLDS = atom({ plugin: 'varasi', key: 'holds' } as const, { held: [], rows: {} }, { shape: 'holds-1' });
const SORT = {
    model: 'sonnet',
    maxTokens: 200,
    timeoutMs: 20_000,
    system: `You sort questions an AI agent is about to put to the person it works for. The input is a JSON list of questions, each with its options. Label each one.

CONSENT: it asks leave to act on the person's own files, machines, accounts, money, data, or repository, or to keep a note or rule for later (delete, commit, push, publish, install, sign up, spend, save a memory).
TASTE: the answer is the person's own preference with no technical consequence to find out: a name, a wording, a look, a setting on their own devices, or a cost they pay.
WORK: anything else, however it is phrased, "do you want" included: how something should be built, where it should live, which mechanism, tool, or form to use, how much of it to do, or when (now, later, a later phase).

Write one line per question and nothing else: its number and its label.`,
} as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const keyOf = ({ header, question }: Question): string => (header.trim() === '' ? question : header.trim().toLowerCase());

const worked = async ($: EngineInterface, questions: readonly Question[], signal: AbortSignal): Promise<ReadonlySet<number>> => {
    const prompt = JSON.stringify(questions.map(({ question, options }) => ({ question, options: options.map(({ label, description }) => ({ label, description })) })));
    const reply = await $.model.complete({ ...SORT, prompt }, { signal });
    return new Set(reply.isAnswered ? [...reply.text.matchAll(KIND)].flatMap(({ groups }) => (groups?.['kind'] === 'WORK' ? [Number(groups['at']) - 1] : [])) : []);
};

const isHold = ({ tool, text = '' }: ToolUseSummary): boolean => tool === 'AskUserQuestion' && text.includes(LEAD);

const since = (uses: readonly ToolUseSummary[]): string | undefined => {
    const at = uses.findLastIndex(isHold);
    const ran = [...new Set(uses.slice(at + 1).flatMap(({ tool }) => (tool === 'AskUserQuestion' ? [] : [tool])))];
    return at === -1 ? undefined : `Kept from you once by varasi, asked again ${ran.length === 0 ? 'with nothing run since' : `after ${ran.join(', ')}`}`;
};

const pending = (messages: readonly SessionMessage[], rows: Readonly<Record<string, string>>): readonly ToolUseSummary[] | undefined => {
    const after = messages.slice(messages.findLastIndex(({ toolUses }) => toolUses.some(isHold)));
    const uses = after.flatMap(({ toolUses }) => toolUses);
    return after[0]?.toolUses.some(isHold) === true && after.every(({ role, text }, at) => at === 0 || role !== 'user' || text.trim() === '') && uses.every(({ tool_use_id }) => rows[tool_use_id] === undefined) ? uses : undefined;
};

const noting = (agentId: string): string =>
    `varasi: the report of agent ${agentId} picks, and one agent's pick is a first pass. Before it stands or reaches the person, brief a fresh agent with the question and the pick, none of the report's reasons, told to assume the pick naive and break it. Carry every limit the report states.`;

const holding = (held: readonly Question[], rest: boolean): string =>
    [
        `${LEAD} ${held.map(({ header, question }) => `"${header.trim() === '' ? question.split(/\s+/u).slice(0, 5).join(' ') : header}"`).join(', ')} from the person, who has not seen it.`,
        ...(rest ? ['Ask the other questions now.'] : []),
        'Ask each again with this tool, under its header, only once its question names the fact that would change its answer and what checked it: a trial, a measurement, a source read, or an agent that made no pick.',
        'With no such check, brief a fresh agent with the question, its facts, and its options as claims to break, none of your reasons or leaning.',
    ].join(' ');

// --- [COMPOSITION] ---------------------------------------------------------------------

const register: Register = (on) => {
    on('tool.call', { tool: 'AskUserQuestion' }, async ($, e, next) => {
        if (next.origin.plugin !== 'engine' || e.metadata?.source !== undefined) {
            return next(e);
        }
        const { held } = await read($, HOLDS);
        const known = e.questions.map((question) => held.includes(keyOf(question)));
        const work = known.every(Boolean) ? new Set<number>() : await worked($, e.questions, next.signal);
        const fresh = e.questions.filter((_, at) => work.has(at) && known[at] !== true);
        if (fresh.length > 0) {
            await update($, HOLDS, (now) => ({ ...now, held: [...now.held, ...fresh.map(keyOf)] }));
            return { deny: holding(fresh, fresh.length < e.questions.length) };
        }
        const messages = known.some(Boolean) ? await $.session.messages({ last: LAST, ...(e.agentId !== undefined && { agentId: e.agentId }) }) : [];
        const row = Array.isArray(messages) ? since(messages.flatMap(({ toolUses }) => toolUses)) : undefined;
        await (row === undefined ? undefined : update($, HOLDS, (now) => ({ ...now, rows: { ...now.rows, [e.tool_use_id]: row } })));
        return next(e);
    });
    on('classic.Stop', async ($, e, next) => {
        const answer = await next(e);
        if (e.stop_hook_active || answer.block !== undefined) {
            return answer;
        }
        const [messages, { rows }] = await Promise.all([$.session.messages({ last: LAST }), read($, HOLDS)]);
        const uses = Array.isArray(messages) ? pending(messages, rows) : undefined;
        return uses?.every(({ tool }) => tool === 'AskUserQuestion') === true ? { ...answer, block: UNCHECKED } : answer;
    });
    on('agent.report', async ($, e, next) => {
        const report = await next(e);
        if (report.consumed !== undefined || e.parentAgentId !== undefined || !PICK.test(report.text)) {
            return report;
        }
        const [messages, { rows }] = await Promise.all([$.session.messages({ last: LAST }), read($, HOLDS)]);
        if (!Array.isArray(messages) || pending(messages, rows) !== undefined) {
            return report;
        }
        await $.session.append({ message: { type: 'user', content: [{ type: 'text', text: noting(e.agentId) }] } });
        $.ui.toast(`varasi asked for a fresh check of the pick in a ${e.subagentType} report`);
        return report;
    });
    on('ui.render', { component: 'AskUserQuestion' }, async ($, e, next) => {
        const { rows } = await read($, HOLDS);
        const row = rows[e.requestId];
        if (row === undefined) {
            return next(e);
        }
        const { Box, Text } = $.ui.resolve(e);
        return (
            <Box flexDirection="column">
                <Text dimColor>{row}</Text>
                {await next(e)}
            </Box>
        );
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
