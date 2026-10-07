import type { ClassicHookEvent, EventName } from 'claude-code';
import { none, type Option, some } from '../composition.ts';
import type { Column } from './sql.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Event = ClassicHookEvent | 'tool.call' | Extract<EventName, `turn.${string}`>;
type Payload = Readonly<Record<string, unknown>>;
type Id = Exclude<Column, 'event' | 'ts' | 'payload'>;
type Row = Readonly<Partial<Record<Id, string>> & Record<'session_id' | 'payload', string>> & { readonly event: Event; readonly ts: number };

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
    ids: { session_id: 'session_id', prompt_id: 'prompt_id', agent_id: 'agent_id', tool: 'tool_name', tool_use_id: 'tool_use_id' },
    drops: { hook_event_name: true, tool_calls: { tool_response: true } },
    tools: {
        Read: { tool_response: { pages: true, file: { content: true, base64: true, cells: true } } },
        Write: { tool_response: { content: true } },
        Edit: { tool_response: { originalFile: true } },
    },
};
const CALL: Columns = { ids: { agent_id: 'agentId', tool: 'tool', tool_use_id: 'tool_use_id' }, drops: { trace: { received: true, returned: true } }, tools: {} };
const TURN: Columns = { ids: { agent_id: 'agentId' }, drops: {}, tools: {} };

// --- [OPERATIONS] ----------------------------------------------------------------------

const _isRecord = (value: unknown): value is Payload => typeof value === 'object' && value !== null;

const _cell = (value: Payload, key: string | undefined): Option<string> => {
    const cell = key === undefined ? undefined : value[key];
    return typeof cell === 'string' ? some(cell) : none;
};

const _dropped = (value: unknown, drops: Drops): unknown => {
    if (Array.isArray(value)) {
        return value.map((item) => _dropped(item, drops));
    }
    return _isRecord(value)
        ? Object.fromEntries(
              Object.entries(value).flatMap(([key, item]) => {
                  const cut = drops[key];
                  return cut === true ? [] : [[key, cut === undefined ? item : _dropped(item, cut)]];
              }),
          )
        : value;
};

const session = (value: Payload, columns: Columns): Option<string> => _cell(value, columns.ids.session_id);

const row = (event: Event, value: Payload, columns: Columns, sessionId: string, ts: number): Row => {
    const ids: Readonly<Partial<Record<Id, string>>> = Object.fromEntries(
        Object.entries(columns.ids).flatMap(([name, key]) => {
            const cell = _cell(value, key);
            return cell.kind === 'some' ? [[name, cell.value]] : [];
        }),
    );
    const columned = Object.fromEntries(Object.values(columns.ids).map((key) => [key, true] as const));
    return { ...ids, event, ts, session_id: sessionId, payload: JSON.stringify(_dropped(value, { ...columns.drops, ...(ids.tool === undefined ? {} : columns.tools[ids.tool]), ...columned })) };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Columns, Event, Payload };
export { CALL, CLASSIC, row, session, TURN };
