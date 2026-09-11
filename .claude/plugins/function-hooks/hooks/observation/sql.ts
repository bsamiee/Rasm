// --- [IMPORTS] -------------------------------------------------------------------------

import type { Cell, Row } from './row.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LOCATE: readonly string[] = ['mise', 'where', 'sqlite'];
const WAL = 'pragma journal_mode=wal;\n';
const _DDL = [
    'create table if not exists observation(event text not null, ts integer not null, session_id text not null, prompt_id text, agent_id text, tool text, tool_use_id text, payload text not null check (json_valid(payload)));',
    'create index if not exists observation_session_ts on observation(session_id, ts);',
    'create index if not exists observation_agent on observation(agent_id);',
    'create index if not exists observation_prompt on observation(prompt_id);',
].join('\n');

// --- [OPERATIONS] ----------------------------------------------------------------------

const _quoted = (text: string): string => `'${text.replaceAll("'", "''")}'`;

const _literal = (cell: Cell): string => (cell.kind === 'text' ? _quoted(cell.text) : 'null');

const _directory = (root: string): string => `${root}/.cache/observation`;

const keep = (root: string): string => `${_directory(root)}/.keep`;

const database = (root: string): string => `${_directory(root)}/observation.db`;

const argv = (install: string, file: string): readonly string[] => [`${install}/bin/sqlite3`, '-cmd', '.timeout 10000', file];

const script = (built: Row): string =>
    `${_DDL}\ninsert into observation(event, ts, session_id, prompt_id, agent_id, tool, tool_use_id, payload) values (${[
        _quoted(built.event),
        String(built.ts),
        _quoted(built.sessionId),
        _literal(built.promptId),
        _literal(built.agentId),
        _literal(built.tool),
        _literal(built.toolUseId),
        _quoted(built.payload),
    ].join(', ')});\n`;

// --- [EXPORTS] -------------------------------------------------------------------------

export { argv, database, keep, LOCATE, script, WAL };
