import type { ClassicHookEvent, EventName } from 'claude-code';
import type { Column } from './sql.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Event = ClassicHookEvent | 'tool.call' | Extract<EventName, `turn.${string}`>;
type Payload = Readonly<Record<string, unknown>>;
type Id = Exclude<Column, 'event' | 'ts' | 'session_id' | 'payload'>;
type Row = Readonly<Partial<Record<Id, string>>> & { readonly event: Event; readonly ts: number; readonly session_id: string; readonly payload: string };

interface Drops {
    readonly [key: string]: Drops | true;
}
interface Columns {
    readonly ids: Readonly<Partial<Record<Id, string>>>;
    readonly drops: Drops;
    readonly tools: Readonly<Partial<Record<string, Drops>>>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const CLASSIC: Columns = {
    ids: { prompt_id: 'prompt_id', agent_id: 'agent_id', tool: 'tool_name', tool_use_id: 'tool_use_id' },
    drops: { session_id: true, hook_event_name: true, tool_calls: { tool_response: true } },
    tools: {
        Read: { tool_response: { pages: true, file: { content: true, base64: true, cells: true } } },
        Write: { tool_response: { content: true } },
        Edit: { tool_response: { originalFile: true } },
    },
};
const CALL: Columns = { ids: { agent_id: 'agentId', tool: 'tool', tool_use_id: 'tool_use_id' }, drops: { trace: { received: true, returned: true } }, tools: {} };
const TURN: Columns = { ids: { agent_id: 'agentId' }, drops: {}, tools: {} };
const USAGE: readonly Event[] = ['Stop', 'SessionEnd'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const _dropped = (value: unknown, drops: Drops): unknown => {
    if (Array.isArray(value)) {
        return value.map((item) => _dropped(item, drops));
    }
    return typeof value === 'object' && value !== null ? Object.fromEntries(Object.entries(value).flatMap(([key, item]) => (drops[key] === true ? [] : [[key, drops[key] === undefined ? item : _dropped(item, drops[key])]]))) : value;
};

const row = (event: Event, value: Payload, columns: Columns, session: string, ts: number): Row => {
    const ids: Readonly<Partial<Record<Id, string>>> = Object.fromEntries(Object.entries(columns.ids).flatMap(([name, key]) => (typeof value[key] === 'string' ? [[name, value[key]]] : [])));
    const columned = Object.fromEntries(Object.values(columns.ids).map((key) => [key, true] as const));
    return { ...ids, event, ts, session_id: session, payload: JSON.stringify(_dropped(value, { ...columns.drops, ...(ids.tool === undefined ? {} : columns.tools[ids.tool]), ...columned })) };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Columns, Event, Payload };
export { CALL, CLASSIC, row, TURN, USAGE };
