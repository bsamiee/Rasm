// --- [IMPORTS] -------------------------------------------------------------------------

import type { Cell, Row } from './row.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Argv = readonly [string, ...string[]];

// --- [CONSTANTS] -----------------------------------------------------------------------

const LOCATE: Argv = ['mise', 'where', 'sqlite'];
const _TABLES: Readonly<Record<string, string>> = {
    ['observation']:
        '(event text not null, ts integer not null, session_id text not null, prompt_id text, agent_id text, tool text, tool_use_id text, payload text not null check (json_valid(payload)))',
    ['transition_state']: '(state text primary key) strict',
    ['delivery_channel']: '(channel text primary key) strict',
    ['range_kind']: '(kind text primary key) strict',
    ['bar_verdict']: '(verdict text primary key, earns integer not null check (earns in (0, 1))) strict',
    ['finding']:
        "(category text not null, path text not null, text text not null, text_hash text not null, occurrence integer not null, finding_id text generated always as (lower(hex(sha3(category || char(0) || path || char(0) || text_hash || char(0) || occurrence, 256)))) stored unique, start_line integer not null, start_column integer not null, end_line integer not null, end_column integer not null, byte_start integer, byte_end integer, subject_hash text not null, severity text, message text not null, replacement text, source text not null check (source like 'checker:%' or source like 'agent:%'), session_id text, prompt_id text, agent_id text, tool_use_id text, observed_at integer not null) strict",
    ['finding_transition']:
        "(finding_id text not null references finding(finding_id), state text not null references transition_state(state), subject_hash text not null, start_line integer, start_column integer, end_line integer, end_column integer, occurrence integer, at integer not null, by text not null check (by = 'user' or by like 'agent:%' or by like 'check:%'), evidence text check (evidence is not null or state not in ('wrong', 'waived', 'checker_owned', 'checker_silent')), verdict text references bar_verdict(verdict), successor text references finding(finding_id) check (successor is not null or state <> 'moved')) strict",
    ['finding_delivery']:
        '(finding_id text not null references finding(finding_id), lineage_key text not null, session_id text not null, agent_id text, channel text not null references delivery_channel(channel), delivered_at integer not null) strict',
    ['judged_range']:
        "(kind text not null references range_kind(kind), main_worktree text not null, worktree text not null, branch text not null, lineage_key text generated always as (main_worktree || '/' || worktree || '/' || branch) stored, from_ts integer not null, to_ts integer not null, agent_id text not null, at integer not null) strict",
};
const _INDEXES: readonly string[] = [
    'create index if not exists observation_session_ts on observation(session_id, ts);',
    'create index if not exists observation_agent on observation(agent_id);',
    'create index if not exists observation_prompt on observation(prompt_id);',
    "create index if not exists observation_turn on observation(json_extract(payload, '$.turnId'));",
    'create index if not exists finding_category on finding(category);',
    'create index if not exists finding_path on finding(path);',
    'create index if not exists finding_transition_at on finding_transition(finding_id, at);',
    'create index if not exists finding_delivery_at on finding_delivery(finding_id, delivered_at);',
    'create index if not exists judged_range_to on judged_range(kind, worktree, to_ts);',
];
const _ROWS: readonly string[] = [
    "delete from transition_state where state not in ('proposed', 'confirmed', 'wrong', 'checker_owned', 'checker_silent', 'fixed', 'vanished', 'moved', 'waived') and not exists (select 1 from finding_transition t where t.state = transition_state.state);",
    "insert or ignore into transition_state(state) values ('proposed'), ('confirmed'), ('wrong'), ('checker_owned'), ('checker_silent'), ('fixed'), ('vanished'), ('moved'), ('waived');",
    "delete from delivery_channel where channel not in ('additionalContext', 'prompt.submit', 'stop.block', 'report') and not exists (select 1 from finding_delivery d where d.channel = delivery_channel.channel);",
    "insert or ignore into delivery_channel(channel) values ('additionalContext'), ('prompt.submit'), ('stop.block'), ('report');",
    "delete from range_kind where kind <> 'edit' and not exists (select 1 from judged_range j where j.kind = range_kind.kind);",
    "insert or ignore into range_kind(kind) values ('edit');",
];
const _VIEWS: readonly string[] = [
    "create view edited_files as select session_id, prompt_id, agent_id, ts, tool, tool_use_id, coalesce(json_extract(payload, '$.tool_input.file_path'), json_extract(payload, '$.tool_input.notebook_path')) as file_path, json_extract(payload, '$.cwd') as cwd from observation where event = 'PostToolUse' and tool in ('Edit', 'Write', 'NotebookEdit');",
    "create view lineage as select g.session_id, g.prompt_id, g.agent_id, a.parent_id, g.agent_type, a.description, a.resolved_model, a.is_async, a.status, a.total_tokens, a.total_duration_ms, g.started_ts, g.stopped_ts from (select agent_id, min(session_id) as session_id, min(prompt_id) as prompt_id, min(json_extract(payload, '$.agent_type')) as agent_type, min(case when event = 'SubagentStart' then ts end) as started_ts, max(case when event in ('SubagentStop', 'turn.complete') then ts end) as stopped_ts from observation where event in ('SubagentStart', 'SubagentStop', 'turn.complete') and agent_id is not null group by agent_id) g left join (select json_extract(payload, '$.tool_response.agentId') as agent_id, min(agent_id) as parent_id, min(json_extract(payload, '$.tool_input.description')) as description, min(json_extract(payload, '$.tool_response.resolvedModel')) as resolved_model, min(json_extract(payload, '$.tool_response.isAsync')) as is_async, min(json_extract(payload, '$.tool_response.status')) as status, min(json_extract(payload, '$.tool_response.totalTokens')) as total_tokens, min(json_extract(payload, '$.tool_response.totalDurationMs')) as total_duration_ms from observation where event = 'PostToolUse' and tool = 'Agent' group by 1) a on a.agent_id = g.agent_id;",
    "create view processes as select session_id, ts, json_extract(payload, '$.source') as source from observation where event = 'SessionStart' and json_extract(payload, '$.source') <> 'compact';",
    "create view turn_cost as select x.session_id, x.prompt_id, x.turn_id, x.started_ts, x.model, x.input_tokens, x.output_tokens, x.cache_read_input_tokens, x.cache_creation_input_tokens, x.steps, x.duration_ms, x.reason, case when x.process_ts is not null then x.stop_usd - ifnull((select json_extract(q.payload, '$.usage.cost.usd') from observation q where q.session_id = x.session_id and q.event = 'Stop' and q.ts >= x.process_ts and q.ts < x.stop_ts order by q.ts desc limit 1), 0) end as usd from (select p.session_id, p.prompt_id, p.turn_id, p.started_ts, json_extract(c.payload, '$.usage.model') as model, json_extract(c.payload, '$.usage.input_tokens') as input_tokens, json_extract(c.payload, '$.usage.output_tokens') as output_tokens, json_extract(c.payload, '$.usage.cache_read_input_tokens') as cache_read_input_tokens, json_extract(c.payload, '$.usage.cache_creation_input_tokens') as cache_creation_input_tokens, (select count(1) from observation s where s.event = 'turn.step' and json_extract(s.payload, '$.turnId') = p.turn_id) as steps, json_extract(c.payload, '$.durationMs') as duration_ms, json_extract(c.payload, '$.reason') as reason, k.ts as stop_ts, json_extract(k.payload, '$.usage.cost.usd') as stop_usd, (select max(b.ts) from processes b where b.session_id = p.session_id and b.ts <= k.ts) as process_ts from (select t.session_id, json_extract(t.payload, '$.turnId') as turn_id, t.ts as started_ts, (select u.prompt_id from observation u where u.session_id = t.session_id and u.event = 'UserPromptSubmit' and u.ts <= t.ts order by u.ts desc limit 1) as prompt_id from observation t where t.event = 'turn.start') p left join observation c on c.event = 'turn.complete' and json_extract(c.payload, '$.turnId') = p.turn_id left join observation k on k.event = 'Stop' and k.session_id = p.session_id and k.ts between p.started_ts and c.ts and not exists (select 1 from observation k2 where k2.event = 'Stop' and k2.session_id = p.session_id and k2.ts > k.ts and k2.ts <= c.ts)) x;",
    "create view agent_cost as select l.session_id, l.prompt_id, l.agent_id, l.agent_type, l.stopped_ts - l.started_ts as span_ms, count(o.rowid) as tool_uses, sum(json_extract(o.payload, '$.duration_ms')) as tool_duration_ms, l.total_tokens, l.total_duration_ms from lineage l left join observation o on o.event = 'PostToolUse' and o.agent_id = l.agent_id group by l.agent_id;",
    'create view edit_churn as select session_id, prompt_id, file_path, count(1) as edits, count(distinct agent_id) + max(agent_id is null) as agents, min(ts) as first_ts, max(ts) as last_ts from edited_files group by session_id, prompt_id, file_path having count(1) > 1;',
    "create view denials as select ts, session_id, prompt_id, agent_id, tool, tool_use_id, 'policy' as kind, json_extract(payload, '$.deny') as reason, json_extract(payload, '$.command') as command, json_extract(payload, '$.file_path') as file_path from observation where event = 'tool.call' and json_extract(payload, '$.deny') is not null union all select ts, session_id, prompt_id, agent_id, tool, tool_use_id, 'permission', json_extract(payload, '$.reason'), json_extract(payload, '$.tool_input.command'), json_extract(payload, '$.tool_input.file_path') from observation where event = 'PermissionDenied' union all select ts, session_id, prompt_id, agent_id, tool, tool_use_id, 'failure', json_extract(payload, '$.error'), json_extract(payload, '$.tool_input.command'), json_extract(payload, '$.tool_input.file_path') from observation where event = 'PostToolUseFailure' union all select b.ts, b.session_id, b.prompt_id, b.agent_id, json_extract(c.value, '$.tool_name'), json_extract(c.value, '$.tool_use_id'), 'host', null, json_extract(c.value, '$.tool_input.command'), json_extract(c.value, '$.tool_input.file_path') from observation b, json_each(json_extract(b.payload, '$.tool_calls')) c where b.event = 'PostToolBatch' and not exists (select 1 from observation r where r.tool_use_id = json_extract(c.value, '$.tool_use_id') and r.event in ('PostToolUse', 'PostToolUseFailure', 'PermissionDenied', 'tool.call'));",
    "create view session_audit as select s.session_id, min(s.ts) as first_ts, max(s.ts) as last_ts, (select count(1) from processes b where b.session_id = s.session_id) as processes, sum(s.event = 'UserPromptSubmit') as prompts, sum(s.event = 'SubagentStart') as agents, sum(s.event = 'SubagentStop' and json_extract(s.payload, '$.agent_type') = '') as forks, sum(s.event = 'PostCompact') as compactions, sum(case when s.event = 'PostCompact' then length(cast(json_extract(s.payload, '$.compact_summary') as blob)) end) as compact_summary_bytes, (select json_extract(e.payload, '$.reason') from observation e where e.session_id = s.session_id and e.event = 'SessionEnd' order by e.ts desc limit 1) as end_reason, (select json_array_length(json_extract(p.payload, '$.background_tasks')) from observation p where p.session_id = s.session_id and p.event = 'Stop' order by p.ts desc limit 1) as background_tasks_at_end, (select sum((select max(json_extract(r.payload, '$.usage.cost.usd')) from observation r where r.session_id = b.session_id and r.event in ('Stop', 'SessionEnd') and r.ts >= b.ts and not exists (select 1 from processes n where n.session_id = b.session_id and n.ts > b.ts and n.ts <= r.ts))) from processes b where b.session_id = s.session_id and (select f.event from observation f where f.session_id = s.session_id order by f.ts limit 1) = 'SessionStart') as usd from observation s group by s.session_id;",
    "create view commits as select ts, session_id, prompt_id, agent_id, tool_use_id, json_extract(payload, '$.tool_response.gitOperation.commit.sha') as sha, json_extract(payload, '$.tool_response.gitOperation.commit.kind') as kind, json_extract(payload, '$.tool_response.gitOperation.commit.branch') as branch, json_extract(payload, '$.cwd') as cwd from observation where event = 'PostToolUse' and tool = 'Bash' and json_extract(payload, '$.tool_response.gitOperation.commit') is not null;",
    "create view agent_digest as select o.session_id, o.agent_id, l.agent_type, count(1) as tool_uses, min(o.ts) as first_ts, max(o.ts) as last_ts, sum(o.tool = 'Read') as reads, sum(o.tool = 'Edit') as edits, sum(o.tool = 'Write') as writes, sum(o.tool = 'Bash') as bash_calls, sum(o.tool = 'Agent') as agent_calls, (select count(1) from denials d where d.session_id = o.session_id and d.agent_id is o.agent_id) as denials, (select count(distinct f.file_path) from edited_files f where f.session_id = o.session_id and f.agent_id is o.agent_id) as files from observation o left join lineage l on l.agent_id = o.agent_id where o.event = 'PostToolUse' group by o.session_id, o.agent_id;",
    "create view repeated_calls as select session_id, agent_id, tool, json_extract(payload, '$.tool_input') as tool_input, count(1) as calls, min(ts) as first_ts, max(ts) as last_ts from observation where event = 'PostToolUse' group by session_id, agent_id, tool, json_extract(payload, '$.tool_input') having count(1) > 1;",
    "create view edits_outside_cwd as select ts, session_id, prompt_id, agent_id, tool, tool_use_id, file_path, cwd from edited_files where substr(file_path, 1, length(cwd) + 1) <> cwd || '/';",
    "create view running_agents as select s.session_id, s.prompt_id, s.agent_id, json_extract(s.payload, '$.agent_type') as agent_type, json_extract(s.payload, '$.cwd') as cwd, s.ts as started_ts from observation s where s.event = 'SubagentStart' and not exists (select 1 from observation x where x.session_id = s.session_id and x.ts > s.ts and (x.event = 'SessionEnd' or (x.event = 'turn.complete' and x.agent_id = s.agent_id) or (x.event in ('Stop', 'SubagentStop') and not exists (select 1 from json_each(x.payload, '$.background_tasks') t where json_extract(t.value, '$.id') = s.agent_id))));",
    "create view finding_state as select f.finding_id, f.category, f.path, f.text, f.text_hash, f.occurrence, coalesce(t.start_line, f.start_line) as start_line, coalesce(t.start_column, f.start_column) as start_column, coalesce(t.end_line, f.end_line) as end_line, coalesce(t.end_column, f.end_column) as end_column, f.byte_start, f.byte_end, f.severity, f.message, f.replacement, f.source, f.session_id, f.prompt_id, f.agent_id, f.tool_use_id, f.observed_at, t.state, t.subject_hash, t.at, t.by, t.evidence, t.verdict, t.successor, (select max(c.at) from finding_transition c where c.finding_id = f.finding_id and c.state in ('fixed', 'vanished')) as last_closed_at from finding f join finding_transition t on t.rowid = (select u.rowid from finding_transition u where u.finding_id = f.finding_id order by u.at desc, u.rowid desc limit 1);",
    "create view confirmed_findings as select finding_id, category, path, text, occurrence, start_line, start_column, end_line, end_column, message, replacement, source, session_id, prompt_id, agent_id, subject_hash, at as confirmed_at, evidence, verdict, last_closed_at from finding_state s where state = 'confirmed' and source like 'agent:%' and not exists (select 1 from finding_transition w where w.finding_id = s.finding_id and w.state = 'wrong' and w.subject_hash = s.subject_hash);",
    "create view open_findings as select c.*, (select json_group_array(distinct d.lineage_key) from finding_delivery d where d.finding_id in (with recursive predecessor(id) as (select c.finding_id union select t.finding_id from finding_transition t join predecessor on t.successor = predecessor.id where t.state = 'moved') select id from predecessor) and (c.last_closed_at is null or d.delivered_at > c.last_closed_at) and not exists (select 1 from observation b where b.session_id = d.session_id and b.ts > d.delivered_at and (b.event = 'SessionEnd' or (b.event = 'PostCompact' and d.channel <> 'report')))) as delivered_on from confirmed_findings c;",
    "create view recurring_categories as select c.category, count(1) as sites, json_group_array(c.path || ':' || c.start_line) as sites_at, min(c.confirmed_at) as first_at, max(c.confirmed_at) as last_at, (select json_group_array(distinct d.lineage_key) from finding_delivery d join confirmed_findings s on s.category = c.category and d.finding_id in (with recursive predecessor(id) as (select s.finding_id union select t.finding_id from finding_transition t join predecessor on t.successor = predecessor.id where t.state = 'moved') select id from predecessor) where d.channel = 'report' and (s.last_closed_at is null or d.delivered_at > s.last_closed_at) and not exists (select 1 from observation b where b.event = 'SessionEnd' and b.session_id = d.session_id and b.ts > d.delivered_at)) as reported_on from confirmed_findings c left join bar_verdict v on v.verdict = c.verdict group by c.category having count(1) >= 2 and total(v.earns = 0) = 0;",
    "create view judged_edits as select e.session_id, e.prompt_id, e.agent_id, e.ts, e.tool, e.tool_use_id, e.file_path, e.cwd, j.lineage_key from edited_files e join judged_range j on j.kind = 'edit' and e.ts between j.from_ts and j.to_ts and (e.cwd = j.worktree or substr(e.cwd, 1, length(j.worktree) + 1) = j.worktree || '/') where substr(e.file_path, 1, length(e.cwd) + 1) = e.cwd || '/' and not exists (select 1 from judged_range k where k.kind = 'edit' and length(k.worktree) > length(j.worktree) and (e.cwd = k.worktree or substr(e.cwd, 1, length(k.worktree) + 1) = k.worktree || '/'));",
    "create view unjudged_edits as select session_id, prompt_id, agent_id, ts, tool, tool_use_id, file_path, cwd from edited_files e where substr(file_path, 1, length(cwd) + 1) = cwd || '/' and not exists (select 1 from judged_edits x where x.tool_use_id = e.tool_use_id);",
    "create view category_fires as select f.category, count(distinct f.finding_id) as sites, count(t.rowid) as sightings, count(distinct f.prompt_id) as prompts_fired, (select count(distinct prompt_id) from judged_edits) as prompts_judged, min(t.at) as first_at, max(t.at) as last_at from finding f left join finding_transition t on t.finding_id = f.finding_id and t.by like 'check:%' where f.source like 'checker:%' group by f.category;",
    "create view missed_sites as select finding_id, category, path, start_line, start_column, evidence, at from finding_state where state = 'checker_silent';",
];

// --- [OPERATIONS] ----------------------------------------------------------------------

const quoted = (text: string): string => `'${text.replaceAll("'", "''")}'`;

const _literal = (cell: Cell): string => (cell.kind === 'text' ? quoted(cell.text) : 'null');

const _directory = (root: string): string => `${root}/.cache/observation`;

const _delta = (root: string): string => `${_directory(root)}/delta.sql`;

const _argument = (path: string): string => `"${path.replaceAll('\\', '\\\\').replaceAll('"', '\\"')}"`;

const keep = (root: string): string => `${_directory(root)}/.keep`;

const database = (root: string): string => `${_directory(root)}/observation.db`;

const sqlite3 = (install: string, file: string): Argv => [`${install}/bin/sqlite3`, '-bail', '-cmd', '.timeout 10000', file];

const script = (built: Row): string =>
    `insert into observation(event, ts, session_id, prompt_id, agent_id, tool, tool_use_id, payload) values (${[
        quoted(built.event),
        String(built.ts),
        quoted(built.sessionId),
        _literal(built.promptId),
        _literal(built.agentId),
        _literal(built.tool),
        _literal(built.toolUseId),
        quoted(built.payload),
    ].join(', ')});\n`;

// --- [SCRIPTS] -------------------------------------------------------------------------

// Declared tables and indexes stand in temp, where an unqualified name resolves first, and the two selects compare them with sqlite_master into the delta file
// Delta drops views first, a rename parses every view, and is read once the temp tables are dropped
const open = (root: string): string => {
    const delta = _argument(_delta(root));
    return [
        'pragma foreign_keys = off;',
        ...Object.entries(_TABLES).map(([name, body]) => `create temp table ${name}${body};`),
        ..._INDEXES,
        'begin immediate;',
        `.output ${delta}`,
        "select 'drop view if exists ' || name || ';' from sqlite_master where type = 'view' union all select 'drop index if exists ' || m.name || ';' from sqlite_master m join sqlite_temp_master w on w.type = 'index' and lower(w.name) = lower(m.name) where m.type = 'index' and m.sql is not w.sql;",
        "select 'create table ' || w.name || '__delta' || substr(w.sql, instr(w.sql, '(')) || ';' || char(10) || 'insert into ' || w.name || '__delta(' || ifnull(c.cols, '') || ') select ' || ifnull(c.cols, '') || ' from ' || w.name || ';' || char(10) || 'drop table ' || w.name || ';' || char(10) || 'alter table ' || w.name || '__delta rename to ' || w.name || ';' from sqlite_temp_master w join sqlite_master m on m.type = 'table' and lower(m.name) = lower(w.name) and substr(m.sql, instr(m.sql, '(')) <> substr(w.sql, instr(w.sql, '(')) left join (select t.name as tbl, group_concat(p.name, ', ' order by p.cid) as cols from sqlite_temp_master t join pragma_table_xinfo(t.name, 'temp') p join pragma_table_info(t.name, 'main') q on q.name = p.name where p.hidden = 0 group by t.name) c on c.tbl = w.name;",
        '.output',
        ...Object.keys(_TABLES).map((name) => `drop table temp.${name};`),
        `.read ${delta}`,
        ...Object.entries(_TABLES).map(([name, body]) => `create table if not exists ${name}${body};`),
        ..._INDEXES,
        ..._ROWS,
        ..._VIEWS,
        'commit;',
    ].join('\n');
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Argv };
export { database, keep, LOCATE, open, quoted, script, sqlite3 };
