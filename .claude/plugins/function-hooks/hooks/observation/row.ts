import type { ClassicHookEvent, EventName } from 'claude-code';
import { none, type Option, some } from '../composition.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Event = ClassicHookEvent | 'tool.call' | Extract<EventName, `turn.${string}`>;
type Payload = Readonly<Record<string, unknown>>;
type Ids = Readonly<Partial<Record<'sessionId' | 'promptId' | 'agentId' | 'tool' | 'toolUseId', string>>>;
type Row = Ids & {
    readonly event: Event;
    readonly ts: number;
    readonly sessionId: string;
    readonly payload: string;
};

interface Drops {
    readonly [key: string]: Drops | true;
}
interface Columns {
    readonly ids: Ids;
    readonly drops: Drops;
    readonly tools: Readonly<Partial<Record<string, Drops>>>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const _isRecord = (value: unknown): value is Payload => typeof value === 'object' && value !== null;

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

const session = (value: Payload, columns: Columns): Option<string> => {
    const cell = columns.ids.sessionId === undefined ? undefined : value[columns.ids.sessionId];
    return typeof cell === 'string' ? some(cell) : none;
};

const row = (event: Event, value: Payload, columns: Columns, sessionId: string, ts: number): Row => {
    const ids: Ids = Object.fromEntries(
        Object.entries(columns.ids).flatMap(([name, key]) => {
            const cell = value[key];
            return typeof cell === 'string' ? [[name, cell]] : [];
        }),
    );
    const columned = Object.fromEntries(Object.values(columns.ids).map((key) => [key, true] as const));
    return { ...ids, event, ts, sessionId, payload: JSON.stringify(_dropped(value, { ...columns.drops, ...(ids.tool === undefined ? {} : columns.tools[ids.tool]), ...columned })) };
};

// --- [COLUMNS] -------------------------------------------------------------------------

const CLASSIC: Columns = {
    ids: { sessionId: 'session_id', promptId: 'prompt_id', agentId: 'agent_id', tool: 'tool_name', toolUseId: 'tool_use_id' },
    drops: { ['hook_event_name']: true, ['tool_calls']: { ['tool_response']: true } },
    tools: {
        ['Read']: { ['tool_response']: { pages: true, file: { content: true, base64: true, cells: true } } },
        ['Write']: { ['tool_response']: { content: true } },
        ['Edit']: { ['tool_response']: { originalFile: true } },
    },
};
const CALL: Columns = { ids: { agentId: 'agentId', tool: 'tool', toolUseId: 'tool_use_id' }, drops: { trace: { received: true, returned: true } }, tools: {} };
const TURN: Columns = { ids: { agentId: 'agentId' }, drops: {}, tools: {} };

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Columns, Event, Payload };
export { CALL, CLASSIC, row, session, TURN };
