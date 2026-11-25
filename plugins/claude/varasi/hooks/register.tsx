import { atom, type BuiltinToolInputs, type EngineInterface, type Register, read, type SessionMessage, type ToolUseSummary, update } from 'claude-code';

// --- [TYPES] ---------------------------------------------------------------------------

type Question = BuiltinToolInputs['AskUserQuestion']['questions'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const KIND = /^[ \t]*(?<at>\d+)[^\w\n]+(?<kind>CONSENT|TASTE|WORK)\b/gmu;
const PICK = /\b(?:I (?:would |'d )?recommend|\(recommended\)|recommendation:|I(?: would|'d) (?:take|pick|go with|choose|keep)|my (?:pick|recommendation)|I suggest|the better (?:option|choice|form))\b/iu;
const LAST = 200;
const LEAD = 'varasi kept';
const HOLDS = atom({ plugin: 'varasi', key: 'holds' } as const, { held: [], rows: {} }, { shape: 'holds-1' });
const SORT = {
    model: 'sonnet',
    maxTokens: 200,
    timeoutMs: 20_000,
    system: `You sort questions an AI agent is about to put to the person it works for. The input is a JSON list of questions, each with its options. Label each one.

CONSENT: it asks leave for one named act on the person's own files, machines, data, or repository, or to keep a note or rule for later (delete, commit, push, publish, install, save a memory).
TASTE: the answer is the person's own preference with no technical consequence to find out: a name, a wording, a look, a setting on their own devices.
WORK: anything else, however it is phrased, "do you want" included: how something should be built, where it should live, which mechanism, tool, vendor, account, plan, or form to use, what to pay for, how much of it to do, or when (now, later, a later phase). A choice among alternatives is WORK even where one of them needs the person to pay or sign in.

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

const ranSince = (uses: readonly ToolUseSummary[]): readonly string[] | undefined => {
    const at = uses.findLastIndex(isHold);
    return at === -1 ? undefined : [...new Set(uses.slice(at + 1).flatMap(({ tool }) => (tool === 'AskUserQuestion' ? [] : [tool])))];
};

const pending = (messages: readonly SessionMessage[], rows: Readonly<Record<string, string>>): readonly ToolUseSummary[] | undefined => {
    const after = messages.slice(messages.findLastIndex(({ toolUses }) => toolUses.some(isHold)));
    const uses = after.flatMap(({ toolUses }) => toolUses);
    return after[0]?.toolUses.some(isHold) === true && after.every(({ role, text }, at) => at === 0 || role !== 'user' || text.trim() === '') && uses.every(({ tool_use_id }) => rows[tool_use_id] === undefined) ? uses : undefined;
};

const noting = (agentId: string): string =>
    `varasi: the report of agent ${agentId} picks, and one agent's pick is a first pass. Before it stands or reaches the person, brief a fresh agent with the question and the pick, none of the report's reasons, told to assume the pick naive and break it. Carry every limit the report states.`;

const named = (held: readonly Question[]): string => held.map(({ header, question }) => `"${header.trim() === '' ? question.split(/\s+/u).slice(0, 5).join(' ') : header}"`).join(', ');

const holding = (held: readonly Question[], rest: boolean): string =>
    [
        `${LEAD} ${named(held)} from the person, who has not seen it.`,
        ...(rest ? ['Ask the other questions now.'] : []),
        'Ask each again with this tool, under its header, only once its question names the fact that would change its answer and what checked it: a trial, a measurement, a source read, or an agent that made no pick.',
        'Who must pay or sign in is no such fact.',
        'With no such check, brief a fresh agent with the question, its facts, and its options as claims to break, none of your reasons or leaning.',
    ].join(' ');

const again = (held: readonly Question[]): string => `${LEAD} ${named(held)} again: nothing has run since the first hold, and a reworded question is no check. Run the check or brief the fresh agent that hold named, then ask.`;

// --- [COMPOSITION] ---------------------------------------------------------------------

const register: Register = (on) => {
    on('tool.call', { tool: 'AskUserQuestion' }, async ($, e, next) => {
        if (next.origin.plugin !== 'engine' || e.metadata?.source !== undefined) {
            return next(e);
        }
        const { held } = await read($, HOLDS);
        const times = e.questions.map((question) => held.filter((key) => key === keyOf(question)).length);
        const work = times.every((count) => count > 0) ? new Set<number>() : await worked($, e.questions, next.signal);
        const fresh = e.questions.filter((_, at) => work.has(at) && times[at] === 0);
        if (fresh.length > 0) {
            await update($, HOLDS, (now) => ({ ...now, held: [...now.held, ...fresh.map(keyOf)] }));
            return { deny: holding(fresh, fresh.length < e.questions.length) };
        }
        const messages = times.some((count) => count > 0) ? await $.session.messages({ last: LAST, ...(e.agentId !== undefined && { agentId: e.agentId }) }) : [];
        const ran = Array.isArray(messages) ? ranSince(messages.flatMap(({ toolUses }) => toolUses)) : undefined;
        const idle = ran?.length === 0 ? e.questions.filter((_, at) => times[at] === 1) : [];
        if (idle.length > 0) {
            await update($, HOLDS, (now) => ({ ...now, held: [...now.held, ...idle.map(keyOf)] }));
            return { deny: again(idle) };
        }
        const row = ran === undefined ? undefined : `Kept from you by varasi, asked again ${ran.length === 0 ? 'with nothing run since' : `after ${ran.join(', ')}`}`;
        await (row === undefined ? undefined : update($, HOLDS, (now) => ({ ...now, rows: { ...now.rows, [e.tool_use_id]: row } })));
        return next(e);
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
