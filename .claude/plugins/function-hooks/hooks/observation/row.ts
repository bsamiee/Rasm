// --- [IMPORTS] -------------------------------------------------------------------------

import type { ClassicHookEvent, EventName } from 'claude-code';
import { fromNullable, none, type Option, some } from '../composition.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Event = ClassicHookEvent | 'tool.call' | Extract<EventName, `turn.${string}`>;

interface Columns {
    readonly session?: string;
    readonly prompt?: string;
    readonly agent?: string;
    readonly tool?: string;
    readonly toolUse?: string;
    readonly drops: readonly string[];
    readonly trims: readonly ((value: Readonly<Record<string, unknown>>, tool: Option<string>) => Readonly<Record<string, unknown>>)[];
}

interface Row {
    readonly event: Event;
    readonly ts: number;
    readonly sessionId: string;
    readonly promptId: Option<string>;
    readonly agentId: Option<string>;
    readonly tool: Option<string>;
    readonly toolUseId: Option<string>;
    readonly payload: string;
}

// --- [REFINEMENTS] ---------------------------------------------------------------------

const _isRecord = (value: unknown): value is Readonly<Record<string, unknown>> => typeof value === 'object' && value !== null;

const _isText = (value: unknown): value is string => typeof value === 'string';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _text = (value: Readonly<Record<string, unknown>>, key: string | undefined): Option<string> => {
    const cell = key === undefined ? undefined : value[key];
    return _isText(cell) ? some(cell) : none;
};

const _without = (value: Readonly<Record<string, unknown>>, keys: readonly string[]): Readonly<Record<string, unknown>> =>
    Object.fromEntries(Object.entries(value).filter(([key]) => !keys.includes(key)));

const _read: Columns['trims'][number] = (value, tool) => {
    const response = value['tool_response'];
    return tool.kind === 'some' && tool.value === 'Read' && _isRecord(response) && _isRecord(response['file'])
        ? { ...value, ['tool_response']: { ...response, file: _without(response['file'], ['content', 'base64', 'cells']) } }
        : value;
};

const _response: Columns['trims'][number] = (value, tool) => {
    const drops = tool.kind === 'some' ? fromNullable({ ['Read']: ['pages'], ['Write']: ['content'], ['Edit']: ['originalFile'] }[tool.value]) : none;
    const response = value['tool_response'];
    return drops.kind === 'some' && _isRecord(response) ? { ...value, ['tool_response']: _without(response, drops.value) } : value;
};

const _dropped =
    (key: string, drops: readonly string[]): Columns['trims'][number] =>
    (value): Readonly<Record<string, unknown>> => {
        const items = value[key];
        return Array.isArray(items) ? { ...value, [key]: items.filter(_isRecord).map((item) => _without(item, drops)) } : value;
    };

const session = (value: Readonly<Record<string, unknown>>, columns: Columns): Option<string> => _text(value, columns.session);

const row = (event: Event, value: Readonly<Record<string, unknown>>, columns: Columns, sessionId: string, ts: number): Row => {
    const tool = _text(value, columns.tool);
    return {
        event,
        ts,
        sessionId,
        promptId: _text(value, columns.prompt),
        agentId: _text(value, columns.agent),
        tool,
        toolUseId: _text(value, columns.toolUse),
        payload: JSON.stringify(
            _without(
                columns.trims.reduce((trimmed, trim) => trim(trimmed, tool), value),
                [columns.session, columns.prompt, columns.agent, columns.tool, columns.toolUse].filter(_isText).concat(columns.drops),
            ),
        ),
    };
};

// --- [REGISTRATIONS] -------------------------------------------------------------------

const CLASSIC: Columns = {
    session: 'session_id',
    prompt: 'prompt_id',
    agent: 'agent_id',
    tool: 'tool_name',
    toolUse: 'tool_use_id',
    drops: ['hook_event_name'],
    trims: [_read, _response, _dropped('tool_calls', ['tool_response'])],
};
const CALL: Columns = { agent: 'agentId', tool: 'tool', toolUse: 'tool_use_id', drops: [], trims: [_dropped('trace', ['received', 'returned'])] };
const TURN: Columns = { agent: 'agentId', drops: [], trims: [] };

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Columns, Event, Row };
export { CALL, CLASSIC, row, session, TURN };
