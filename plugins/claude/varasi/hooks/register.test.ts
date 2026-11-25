import type { AgentReportInput, BuiltinToolInputs, On, SessionMessage, ToolCallResult } from 'claude-code';
import { type Engine, expect, mock, test } from 'claude-code/testing';

// --- [TYPES] ---------------------------------------------------------------------------

type Question = BuiltinToolInputs['AskUserQuestion']['questions'][number];
interface Seen {
    readonly sorted: string[];
    readonly asked: string[];
    readonly toasts: string[];
    readonly notes: () => readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const USAGE = { input_tokens: 1, output_tokens: 1, cache_read_input_tokens: 0, cache_creation_input_tokens: 0 } as const;
const ANSWERED = { result: { questions: [], answers: {} }, text: 'The person answered.' } as const;
const ROW = /varasi/u;
const QUEUE: Question = {
    question: 'Research found the send cannot join the app transaction from its own database. Where should a queue live?',
    header: 'Queue home',
    multiSelect: false,
    options: [
        { label: 'Optional per queue (Recommended)', description: 'Each queue says where it lives' },
        { label: 'Separate database', description: 'Keep today' },
    ],
};
const SCRATCH: Question = {
    question: 'A finished session left 16 GiB of scratch in /tmp. Delete it?',
    header: 'Scratch',
    multiSelect: false,
    options: [
        { label: 'Delete it', description: 'Frees 16 GiB' },
        { label: 'Keep it', description: 'Until reboot' },
    ],
};
const FONT: Question = {
    question: 'Which font should the reading apps use?',
    header: 'Font',
    multiSelect: false,
    options: [
        { label: 'Geist', description: 'Proportional' },
        { label: 'Geist Mono', description: 'Fixed width' },
    ],
};
const SURFACES = ['terminal', 'desktop'] as const;
const REPORT: AgentReportInput = { agentId: 'a1', subagentType: 'general-purpose', provider: { plugin: 'engine', tier: 'core' }, status: 'completed', text: "I'd pick form [f]: each app keeps its queues in its own database. Whether pgmq runs on 19.0 is unverified." };
const FACTS = 'pgmq 1.9 builds on PostgreSQL 19.0; the trial ran 40 sends in one transaction.';

// --- [OPERATIONS] ----------------------------------------------------------------------

const worldOf = (on: On, labels: readonly string[], history: readonly SessionMessage[] = []): Seen => {
    const session = mock.session(on);
    const seen: Seen = { sorted: [], asked: [], toasts: [], notes: () => session.appended().flatMap(({ message }) => message.content.flatMap(({ text }) => (typeof text === 'string' ? [text] : []))) };
    const replies = labels.values();
    on('model.complete', (_, e) => {
        seen.sorted.push(e.prompt);
        return { value: { isAnswered: true, text: replies.next().value ?? '', usage: USAGE } };
    });
    on('session.messages', () => ({ value: [...history] }));
    on('agent.report', (_, e) => ({ text: e.text, review: 'off' }));
    on('classic.Stop', () => ({}));
    on('ui.toast', (_, e) => {
        seen.toasts.push(e.text);
        return { value: undefined };
    });
    on('tool.call', (_, e): ToolCallResult => {
        seen.asked.push(e.tool_use_id);
        return ANSWERED;
    });
    return seen;
};

const heldThen = (after: readonly string[]): readonly SessionMessage[] => [
    { role: 'assistant', text: '', toolUses: [{ tool_use_id: 'q0', tool: 'AskUserQuestion', input: {}, text: '<tool_use_error>varasi kept "Queue home" from the person, who has not seen it.</tool_use_error>', isError: true }] },
    { role: 'assistant', text: '', toolUses: after.map((tool, at) => ({ tool_use_id: `t${at}`, tool, input: {}, text: 'ran' })) },
];

const asked = ($: Engine, times: number): Promise<readonly (string | undefined)[]> =>
    Array.from({ length: times }).reduce<Promise<readonly (string | undefined)[]>>(async (denied) => {
        const before = await denied;
        const { deny } = await $.tool.call({ tool: 'AskUserQuestion', questions: [QUEUE] });
        return [...before, deny];
    }, Promise.resolve([]));

const askedTwice = async ($: Engine): Promise<void> => {
    await asked($, 2);
};

const rowOf = async ($: Engine, requestId = '', surface: (typeof SURFACES)[number] = 'terminal'): Promise<string | undefined> => {
    const ui = await $.ui.mount({ plugin: 'varasi', surface, component: 'AskUserQuestion', props: { tool: 'AskUserQuestion', questions: [QUEUE] }, requestId });
    const row = await ui.find({ type: 'Text', text: ROW });
    await ui.unmount();
    return row?.text;
};

const stopped = async ($: Engine, stop_hook_active = false): Promise<string | undefined> => {
    const { block } = await $.classic.Stop({ stop_hook_active });
    return block;
};

const words = (text = ''): number => text.split(/\s+/u).filter(Boolean).length;

// --- [COMPOSITION] ---------------------------------------------------------------------

test('a question the agent can settle by checking is held before the person sees it, naming it, in under 100 words', async ($, on) => {
    const world = worldOf(on, ['1 WORK']);
    const { deny } = await $.tool.call({ tool: 'AskUserQuestion', questions: [QUEUE] });
    expect(world.asked).toEqual([]);
    expect(deny).toStartWith('varasi kept "Queue home" from the person, who has not seen it. Ask each again with this tool, under its header, only once its question names the fact that would change its answer');
    expect(deny).toContain('Who must pay or sign in is no such fact.');
    expect(deny).toContain('brief a fresh agent with the question, its facts, and its options as claims to break, none of your reasons or leaning');
    expect(words(deny)).toBeLessThanOrEqual(100);
    expect(world.sorted).toEqual([expect.stringContaining('"question":"Research found the send')]);
});

test('a call holding consent and work is held whole, naming only the work, and tells the agent to ask the rest now', async ($, on) => {
    const world = worldOf(on, ['1 CONSENT\n2 WORK']);
    const { deny } = await $.tool.call({ tool: 'AskUserQuestion', questions: [SCRATCH, QUEUE] });
    expect(deny).toStartWith('varasi kept "Queue home" from the person, who has not seen it. Ask the other questions now.');
    expect(deny).not.toContain('Scratch');
    expect(world.asked).toEqual([]);
});

test('four held questions are each named once and keep the text within 100 words', async ($, on) => {
    worldOf(on, ['1 WORK\n2 WORK\n3 WORK\n4 WORK']);
    const { deny } = await $.tool.call({ tool: 'AskUserQuestion', questions: ['Queue home', 'Probe path', 'Image host', 'Shape crate'].map((header) => ({ ...QUEUE, header })) });
    expect(deny).toStartWith('varasi kept "Queue home", "Probe path", "Image host", "Shape crate" from the person, who has not seen it. Ask each again');
    expect(words(deny)).toBeLessThanOrEqual(100);
});

[
    { name: 'consent', labels: '1 CONSENT', questions: [SCRATCH] },
    { name: 'taste', labels: '1 TASTE', questions: [FONT] },
    { name: 'consent and taste', labels: '1: CONSENT\n2: TASTE', questions: [SCRATCH, FONT] },
    { name: 'an out-of-form sort', labels: 'I would hold the first one.', questions: [QUEUE] },
].forEach(({ name, labels, questions }) => {
    test(`a call of ${name} reaches the person untouched`, async ($, on) => {
        const world = worldOf(on, [labels]);
        expect(await $.tool.call({ tool: 'AskUserQuestion', questions })).toEqual(ANSWERED);
        expect(world.asked).toHaveLength(1);
    });
});

test('a question a command asks is never sorted', async ($, on) => {
    const world = worldOf(on, ['1 WORK']);
    expect(await $.tool.call({ tool: 'AskUserQuestion', questions: [QUEUE], metadata: { source: 'remember' } })).toEqual(ANSWERED);
    expect(world.sorted).toEqual([]);
});

test('a held question asked again under its header reaches the person unsorted, and a new one beside it is held', async ($, on) => {
    const world = worldOf(on, ['1 WORK', '1 WORK\n2 WORK']);
    await $.tool.call({ tool: 'AskUserQuestion', questions: [QUEUE] });
    const again = await $.tool.call({ tool: 'AskUserQuestion', questions: [{ ...QUEUE, question: 'A trial joined the send to the app transaction. Where should a queue live?' }] });
    const mixed = await $.tool.call({ tool: 'AskUserQuestion', questions: [QUEUE, { ...QUEUE, header: 'Probe path' }] });
    expect([again.deny, mixed.deny?.split('.', 1)[0]]).toEqual([undefined, 'varasi kept "Probe path" from the person, who has not seen it']);
    expect(world.sorted).toHaveLength(2);
    expect(world.asked).toHaveLength(1);
});

test('a held question asked again with nothing run since is held once more in under 100 words, then reaches the person told so above the dialog', async ($, on) => {
    const world = worldOf(on, ['1 WORK'], heldThen([]));
    const [, second, third] = await asked($, 3);
    expect(second).toStartWith('varasi kept "Queue home" again: nothing has run since the first hold, and a reworded question is no check.');
    expect(words(second)).toBeLessThanOrEqual(100);
    expect(third).toBeUndefined();
    expect(world.asked).toHaveLength(1);
    expect(await Promise.all(SURFACES.map((surface) => rowOf($, world.asked[0], surface)))).toEqual(SURFACES.map(() => 'Kept from you by varasi, asked again with nothing run since'));
});

test('a held question asked again after tools ran names each tool once, and a question never held shows no row', async ($, on) => {
    const world = worldOf(on, ['1 WORK', '1 CONSENT'], heldThen(['Agent', 'Read', 'Agent']));
    await askedTwice($);
    await $.tool.call({ tool: 'AskUserQuestion', questions: [SCRATCH] });
    expect(await Promise.all(world.asked.map((requestId) => rowOf($, requestId)))).toEqual(['Kept from you by varasi, asked again after Agent, Read', undefined]);
});

test('a held question asked again once its hold is past the messages read shows the person no row', async ($, on) => {
    const world = worldOf(on, ['1 WORK'], [{ role: 'assistant', text: '', toolUses: [{ tool_use_id: 'b1', tool: 'Bash', input: {}, text: 'ran' }] }]);
    await askedTwice($);
    expect(await rowOf($, world.asked[0])).toBeUndefined();
});

test('a report to the main conversation that picks arrives as written, a note beside it asks for a fresh check in under 100 words, and the person is told', async ($, on) => {
    const world = worldOf(on, []);
    const { text } = await $.agent.report(REPORT);
    expect(text).toBe(REPORT.text);
    expect(world.notes()).toHaveLength(1);
    expect(world.notes()[0]).toStartWith("varasi: the report of agent a1 picks, and one agent's pick is a first pass.");
    expect(world.notes()[0]).toEndWith('told to assume the pick naive and break it. Carry every limit the report states.');
    expect(words(world.notes()[0])).toBeLessThanOrEqual(100);
    expect(world.toasts).toEqual(['varasi asked for a fresh check of the pick in a general-purpose report']);
});

[
    { name: 'that picks nothing', report: { ...REPORT, text: FACTS } },
    { name: 'for another subagent', report: { ...REPORT, parentAgentId: 'a2' } },
    { name: "that a hold's check sent back while the person has not spoken", report: { ...REPORT, text: "I'd pick the app database: only it enqueues in the app's own transaction." }, history: heldThen(['Agent']) },
].forEach(({ name, report, history = [] }) => {
    test(`a report ${name} arrives as written with nothing beside it`, async ($, on) => {
        const world = worldOf(on, [], history);
        const { text } = await $.agent.report(report);
        expect([text, world.notes(), world.toasts]).toEqual([report.text, [], []]);
    });
});

test('a turn that ends with nothing run since a hold, the question moved to prose, is sent back once in under 100 words', async ($, on) => {
    worldOf(on, [], [...heldThen([]), { role: 'assistant', text: 'Which do you want, App database or Separate Redis?', toolUses: [] }]);
    const block = await stopped($);
    expect(block).toStartWith('varasi: the turn ends with nothing run since a question was kept from the person');
    expect(words(block)).toBeLessThanOrEqual(100);
    expect(await stopped($, true)).toBeUndefined();
});

[
    { name: 'a check ran since the hold', history: heldThen(['Agent']) },
    { name: 'the person spoke since the hold', history: [...heldThen([]), { role: 'user', text: 'Separate Redis', toolUses: [] }] satisfies SessionMessage[] },
    { name: 'no question was held', history: [{ role: 'assistant', text: 'Done.', toolUses: [] }] satisfies SessionMessage[] },
].forEach(({ name, history }) => {
    test(`a turn ends untouched when ${name}`, async ($, on) => {
        worldOf(on, [], history);
        expect(await stopped($)).toBeUndefined();
    });
});

test('a turn ends untouched when the held question was asked again with the question tool', async ($, on) => {
    const history = [...heldThen([])];
    const world = worldOf(on, ['1 WORK'], history);
    await asked($, 3);
    history.push({ role: 'assistant', text: '', toolUses: [{ tool_use_id: world.asked[0] ?? '', tool: 'AskUserQuestion', input: {}, text: 'The person answered.' }] });
    expect(await stopped($)).toBeUndefined();
});

test('a report that picks after the held question went back to the person by the question tool asks for a fresh check', async ($, on) => {
    const history = [...heldThen(['Agent'])];
    const world = worldOf(on, ['1 WORK'], history);
    await askedTwice($);
    history.push({ role: 'assistant', text: '', toolUses: [{ tool_use_id: world.asked[0] ?? '', tool: 'AskUserQuestion', input: {}, text: 'The person answered.' }] });
    await $.agent.report(REPORT);
    expect(world.notes()).toHaveLength(1);
});
