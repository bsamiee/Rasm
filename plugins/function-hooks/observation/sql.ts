import type { Invocation } from '../policies/invocation.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Column = (typeof _OBSERVATION)[number][0];

// --- [CONSTANTS] -----------------------------------------------------------------------

const _FOLDER = '.cache/observation';
const DELTA = `${_FOLDER}/delta.sql`;

// --- [OPERATIONS] ----------------------------------------------------------------------

const sqlite = (root: string): Invocation => ['sqlite3', '-bail', '-cmd', '.timeout 10000', `${root}/${_FOLDER}/observation.db`];
const _normalized = (text: string): string => `replace(replace(replace(replace(replace(replace(${text}, char(9), ' '), char(13), ' '), char(10), ' '), ' ', char(64976, 64977)), char(64977, 64976), ''), char(64976, 64977), ' ')`;
const _lineage = (main: string, worktree: string, branch: string): string => `iif(${worktree} = ${main}, '.', substr(${worktree}, length(rtrim(${worktree}, replace(${worktree}, '/', ''))) + 1)) || '/' || ${branch}`;
const bound = (values: object, statement: string): string => `.parameter init\ninsert into temp.sqlite_parameters(key, value) select ':' || key, value from json_each('${JSON.stringify(values).replaceAll("'", "''")}');\n${statement}`;

// --- [SCHEMA] --------------------------------------------------------------------------

const _OBSERVATION = [
    ['event', 'text not null'],
    ['ts', 'integer not null'],
    ['session_id', 'text not null'],
    ['prompt_id', 'text'],
    ['agent_id', 'text'],
    ['tool', 'text'],
    ['tool_use_id', 'text'],
    ['payload', 'text not null'],
] as const;
const _TABLES: readonly (readonly [name: string, body: string, rows?: readonly object[]])[] = [
    ['observation', `(${_OBSERVATION.map(([name, type]) => `${name} ${type}`).join(', ')})`],
    [
        'transition_state',
        '(state text primary key, live integer not null default 0 check (live in (0, 1))) strict',
        [
            { state: 'proposed', live: 1 },
            { state: 'confirmed', live: 1 },
            { state: 'wrong', live: 0 },
            { state: 'checker_owned', live: 1 },
            { state: 'checker_silent', live: 1 },
            { state: 'fixed', live: 0 },
            { state: 'vanished', live: 0 },
            { state: 'moved', live: 0 },
            { state: 'waived', live: 1 },
        ],
    ],
    ['transition_actor', '(actor text primary key) strict', [{ actor: 'user' }, { actor: 'agent' }, { actor: 'check' }]],
    ['checker', '(tool text primary key) strict', [{ tool: 'ast-grep' }, { tool: 'ruff' }, { tool: 'biome' }, { tool: 'roslyn' }]],
    ['delivery_channel', '(channel text primary key) strict', [{ channel: 'additionalContext' }, { channel: 'report' }]],
    ['range_kind', '(kind text primary key) strict', [{ kind: 'edit' }]],
    ['bar_verdict', '(verdict text primary key, earns integer not null check (earns in (0, 1))) strict'],
    [
        'finding',
        `(checker text references checker(tool), category text not null, path text not null, text text not null, ntext text generated always as (${_normalized('text')}) stored, text_hash text generated always as (lower(hex(sha3(ntext, 256)))) stored, occurrence integer not null, finding_id text generated always as (lower(hex(sha3(coalesce(checker || ':', '') || category || char(0) || path || char(0) || text_hash || char(0) || occurrence, 256)))) stored unique, start_line integer not null, start_column integer not null, end_line integer not null, end_column integer not null, byte_start integer, byte_end integer, subject_hash text not null, severity text, message text not null, replacement text, session_id text, prompt_id text, agent_id text, tool_use_id text, observed_at integer not null) strict`,
    ],
    [
        'finding_transition',
        "(finding_id text not null references finding(finding_id), state text not null references transition_state(state), subject_hash text not null, path text, start_line integer, start_column integer, end_line integer, end_column integer, byte_start integer, byte_end integer, occurrence integer, at integer not null, actor text not null references transition_actor(actor), actor_id text check ((actor = 'user') = (actor_id is null)), evidence text check (evidence is not null or state not in ('wrong', 'waived', 'checker_owned', 'checker_silent')), verdict text references bar_verdict(verdict)) strict",
    ],
    ['finding_delivery', '(finding_id text not null references finding(finding_id), lineage_key text not null, session_id text not null, agent_id text, channel text not null references delivery_channel(channel), delivered_at integer not null) strict'],
    [
        'judged_range',
        `(kind text not null references range_kind(kind), main_worktree text not null, worktree text not null, branch text not null, lineage_key text generated always as (${_lineage('main_worktree', 'worktree', 'branch')}) stored, from_ts integer not null, to_ts integer not null, agent_id text not null, at integer not null) strict`,
    ],
];
const _INDEXES: readonly string[] = [
    'create index if not exists observation_session_ts on observation(session_id, ts);',
    'create index if not exists observation_agent on observation(agent_id);',
    'create index if not exists observation_prompt on observation(prompt_id);',
    "create index if not exists observation_turn on observation(payload ->> '$.turnId');",
    'create index if not exists observation_event on observation(event, tool, ts);',
    'create index if not exists observation_tool_use on observation(tool_use_id);',
    'create index if not exists finding_category on finding(category);',
    'create index if not exists finding_path on finding(path);',
    'create index if not exists finding_transition_at on finding_transition(finding_id, at);',
    'create index if not exists finding_delivery_at on finding_delivery(finding_id, delivered_at);',
    'create index if not exists judged_range_to on judged_range(kind, lineage_key, to_ts);',
];
const _COST =
    "cost as (select session_id, ts, event, payload ->> '$.usage.cost.usd' as usd, max(ts) filter (where event = 'SessionStart') over (partition by session_id order by ts) as process_ts from (select session_id, ts, 'SessionStart' as event, null as payload from processes union all select session_id, ts, event, payload from observation where event in ('Stop', 'SessionEnd')))";
const _VIEWS: readonly string[] = [
    "create view edited_files as select session_id, prompt_id, agent_id, ts, tool, tool_use_id, coalesce(payload ->> '$.tool_input.file_path', payload ->> '$.tool_input.notebook_path') as file_path, payload ->> '$.cwd' as cwd from observation where event = 'PostToolUse' and tool in ('Edit', 'Write', 'NotebookEdit');",
    "create view lineage as select g.session_id, g.prompt_id, g.agent_id, a.parent_id, g.agent_type, a.description, a.resolved_model, a.is_async, a.status, a.total_tokens, a.total_duration_ms, g.started_ts, g.stopped_ts from (select agent_id, min(session_id) as session_id, min(prompt_id) as prompt_id, min(payload ->> '$.agent_type') as agent_type, min(ts) filter (where event = 'SubagentStart') as started_ts, max(ts) filter (where event in ('SubagentStop', 'turn.complete')) as stopped_ts from observation where event in ('SubagentStart', 'SubagentStop', 'turn.complete') and agent_id is not null group by agent_id) g left join (select payload ->> '$.tool_response.agentId' as agent_id, min(agent_id) as parent_id, min(payload ->> '$.tool_input.description') as description, min(payload ->> '$.tool_response.resolvedModel') as resolved_model, min(payload ->> '$.tool_response.isAsync') as is_async, min(payload ->> '$.tool_response.status') as status, min(payload ->> '$.tool_response.totalTokens') as total_tokens, min(payload ->> '$.tool_response.totalDurationMs') as total_duration_ms from observation where event = 'PostToolUse' and tool = 'Agent' group by 1) a on a.agent_id = g.agent_id;",
    "create view processes as select session_id, ts, payload ->> '$.source' as source from observation where event = 'SessionStart' and payload ->> '$.source' <> 'compact';",
    `create view turn_cost as with ${_COST}, priced as (select session_id, ts, usd, process_ts, lag(usd) over (partition by session_id, process_ts order by ts) as previous_usd, lead(ts) over (partition by session_id order by ts) as next_ts from cost where event = 'Stop') select p.session_id, p.prompt_id, p.turn_id, p.started_ts, c.payload ->> '$.usage.model' as model, c.payload ->> '$.usage.input_tokens' as input_tokens, c.payload ->> '$.usage.output_tokens' as output_tokens, c.payload ->> '$.usage.cache_read_input_tokens' as cache_read_input_tokens, c.payload ->> '$.usage.cache_creation_input_tokens' as cache_creation_input_tokens, (select count(*) from observation s where s.event = 'turn.step' and s.payload ->> '$.turnId' = p.turn_id) as steps, c.payload ->> '$.durationMs' as duration_ms, c.payload ->> '$.reason' as reason, (select x.payload ->> '$.last_assistant_message' from observation x where x.event = 'StopFailure' and x.session_id = p.session_id and x.agent_id is null and x.ts between p.started_ts and c.ts order by x.ts desc limit 1) as cause, iif(k.process_ts is not null, k.usd - coalesce(k.previous_usd, 0), null) as usd from (select t.session_id, t.payload ->> '$.turnId' as turn_id, t.ts as started_ts, (select u.prompt_id from observation u where u.session_id = t.session_id and u.event = 'UserPromptSubmit' and u.ts <= t.ts order by u.ts desc limit 1) as prompt_id from observation t where t.event = 'turn.start') p left join observation c on c.event = 'turn.complete' and c.payload ->> '$.turnId' = p.turn_id left join priced k on k.session_id = p.session_id and k.ts between p.started_ts and c.ts and (k.next_ts is null or k.next_ts > c.ts);`,
    "create view agent_cost as select l.session_id, l.prompt_id, l.agent_id, l.agent_type, l.stopped_ts - l.started_ts as span_ms, count(o.rowid) as tool_uses, sum(o.payload ->> '$.duration_ms') as tool_duration_ms, coalesce(l.total_tokens, t.tokens) as total_tokens, coalesce(l.total_duration_ms, t.duration_ms) as total_duration_ms, (select x.payload ->> '$.last_assistant_message' from observation x where x.event = 'StopFailure' and x.agent_id = l.agent_id order by x.ts desc limit 1) as cause from lineage l left join observation o on o.event = 'PostToolUse' and o.agent_id = l.agent_id left join (select c.agent_id, sum(c.payload ->> '$.usage.input_tokens' + c.payload ->> '$.usage.output_tokens' + c.payload ->> '$.usage.cache_read_input_tokens' + c.payload ->> '$.usage.cache_creation_input_tokens') as tokens, sum(c.payload ->> '$.durationMs') as duration_ms from observation c where c.event = 'turn.complete' and c.agent_id is not null group by c.agent_id) t on t.agent_id = l.agent_id group by l.agent_id;",
    'create view edit_churn as select session_id, prompt_id, file_path, count(*) as edits, count(distinct agent_id) + max(agent_id is null) as agents, min(ts) as first_ts, max(ts) as last_ts from edited_files group by session_id, prompt_id, file_path having count(*) > 1;',
    "create view denials as select ts, session_id, prompt_id, agent_id, tool, tool_use_id, 'policy' as kind, payload ->> '$.deny' as reason, payload ->> '$.command' as command, payload ->> '$.file_path' as file_path from observation where event = 'tool.call' and payload ->> '$.deny' is not null union all select ts, session_id, prompt_id, agent_id, tool, tool_use_id, 'permission', payload ->> '$.reason', payload ->> '$.tool_input.command', payload ->> '$.tool_input.file_path' from observation where event = 'PermissionDenied' union all select ts, session_id, prompt_id, agent_id, tool, tool_use_id, 'failure', payload ->> '$.error', payload ->> '$.tool_input.command', payload ->> '$.tool_input.file_path' from observation where event = 'PostToolUseFailure' union all select b.ts, b.session_id, b.prompt_id, b.agent_id, c.value ->> '$.tool_name', c.value ->> '$.tool_use_id', 'host', null, c.value ->> '$.tool_input.command', c.value ->> '$.tool_input.file_path' from observation b, json_each(b.payload, '$.tool_calls') c where b.event = 'PostToolBatch' and not exists (select 1 from observation r where r.tool_use_id = c.value ->> '$.tool_use_id' and r.event in ('PostToolUse', 'PostToolUseFailure', 'PermissionDenied', 'tool.call'));",
    `create view session_audit as with ${_COST}, spent as (select session_id, sum(usd) as usd from (select session_id, max(usd) as usd from cost where event <> 'SessionStart' and process_ts is not null group by session_id, process_ts) group by session_id), base as (select session_id, min(ts) as first_ts, max(ts) as last_ts, count(*) filter (where event = 'UserPromptSubmit') as prompts, count(*) filter (where event = 'SubagentStart') as agents, count(*) filter (where event = 'SubagentStop' and payload ->> '$.agent_type' = '') as forks, count(*) filter (where event = 'PostCompact') as compactions, sum(length(cast(payload ->> '$.compact_summary' as blob))) filter (where event = 'PostCompact') as compact_summary_bytes, min(ts) = min(ts) filter (where event = 'SessionStart') as recorded from observation group by session_id) select b.session_id, b.first_ts, b.last_ts, coalesce(n.processes, 0) as processes, b.prompts, b.agents, b.forks, b.compactions, b.compact_summary_bytes, (select e.payload ->> '$.reason' from observation e where e.session_id = b.session_id and e.event = 'SessionEnd' order by e.ts desc limit 1) as end_reason, (select json_array_length(p.payload, '$.background_tasks') from observation p where p.session_id = b.session_id and p.event = 'Stop' order by p.ts desc limit 1) as background_tasks_at_end, iif(b.recorded, x.usd, null) as usd from base b left join (select session_id, count(*) as processes from processes group by session_id) n on n.session_id = b.session_id left join spent x on x.session_id = b.session_id;`,
    "create view commits as select ts, session_id, prompt_id, agent_id, tool_use_id, payload ->> '$.tool_response.gitOperation.commit.sha' as sha, payload ->> '$.tool_response.gitOperation.commit.kind' as kind, payload ->> '$.tool_response.gitOperation.commit.branch' as branch, payload ->> '$.cwd' as cwd from observation where event = 'PostToolUse' and tool = 'Bash' and payload ->> '$.tool_response.gitOperation.commit' is not null;",
    "create view agent_digest as select o.session_id, o.agent_id, l.agent_type, count(*) as tool_uses, min(o.ts) as first_ts, max(o.ts) as last_ts, count(*) filter (where o.tool = 'Read') as reads, count(*) filter (where o.tool = 'Edit') as edits, count(*) filter (where o.tool = 'Write') as writes, count(*) filter (where o.tool = 'Bash') as bash_calls, count(*) filter (where o.tool = 'Agent') as agent_calls, coalesce(d.denials, 0) as denials, coalesce(f.files, 0) as files from observation o left join lineage l on l.agent_id = o.agent_id left join (select session_id, agent_id, count(*) as denials from denials group by session_id, agent_id) d on d.session_id = o.session_id and d.agent_id is o.agent_id left join (select session_id, agent_id, count(distinct file_path) as files from edited_files group by session_id, agent_id) f on f.session_id = o.session_id and f.agent_id is o.agent_id where o.event = 'PostToolUse' group by o.session_id, o.agent_id;",
    "create view repeated_calls as select session_id, agent_id, tool, payload ->> '$.tool_input' as tool_input, count(*) as calls, min(ts) as first_ts, max(ts) as last_ts from observation where event = 'PostToolUse' group by session_id, agent_id, tool, payload ->> '$.tool_input' having count(*) > 1;",
    "create view edits_outside_cwd as select ts, session_id, prompt_id, agent_id, tool, tool_use_id, file_path, cwd from edited_files where instr(file_path, cwd || '/') <> 1;",
    "create view running_agents as with parented as materialized (select payload ->> '$.tool_response.agentId' as agent_id from observation where event = 'PostToolUse' and tool = 'Agent') select s.session_id, s.prompt_id, s.agent_id, s.payload ->> '$.agent_type' as agent_type, s.payload ->> '$.cwd' as cwd, s.ts as started_ts from observation s where s.event = 'SubagentStart' and (select max(m.ts) from observation m where m.session_id = s.session_id) > unixepoch() * 1000 - 172800000 and not exists (select 1 from observation x where x.session_id = s.session_id and x.ts > s.ts and (x.event = 'SessionEnd' or (x.event = 'SessionStart' and x.payload ->> '$.source' <> 'compact') or (x.event = 'turn.complete' and x.agent_id = s.agent_id) or (x.event in ('Stop', 'SubagentStop') and s.agent_id in (select agent_id from parented) and not exists (select 1 from json_each(x.payload, '$.background_tasks') t where t.value ->> '$.id' = s.agent_id))));",
    "create view finding_state as select f.finding_id, f.category, coalesce(t.path, f.path) as path, f.text, f.ntext, f.text_hash, f.occurrence, coalesce(t.start_line, f.start_line) as start_line, coalesce(t.start_column, f.start_column) as start_column, coalesce(t.end_line, f.end_line) as end_line, coalesce(t.end_column, f.end_column) as end_column, coalesce(t.byte_start, f.byte_start) as byte_start, coalesce(t.byte_end, f.byte_end) as byte_end, f.severity, f.message, f.replacement, f.checker, f.session_id, f.prompt_id, f.agent_id, f.tool_use_id, f.observed_at, j.state, t.subject_hash, t.at, t.actor, t.actor_id, j.evidence, j.verdict, (select max(c.at) from finding_transition c where c.finding_id = f.finding_id and c.state in ('fixed', 'vanished')) as last_closed_at from finding f join finding_transition t on t.rowid = (select u.rowid from finding_transition u where u.finding_id = f.finding_id order by u.at desc, u.rowid desc limit 1) join finding_transition j on j.rowid = (select u.rowid from finding_transition u where u.finding_id = f.finding_id and u.state <> 'moved' order by u.at desc, u.rowid desc limit 1);",
    "create view confirmed_findings as select finding_id, category, path, text, ntext, occurrence, start_line, start_column, end_line, end_column, message, replacement, session_id, prompt_id, agent_id, subject_hash, at as confirmed_at, evidence, verdict, last_closed_at from finding_state s where state = 'confirmed' and checker is null and not exists (select 1 from finding_transition w where w.finding_id = s.finding_id and w.state = 'wrong' and w.subject_hash = s.subject_hash);",
    'create view open_findings as select c.*, (select json_group_array(distinct d.lineage_key) from finding_delivery d where d.finding_id = c.finding_id and (c.last_closed_at is null or d.delivered_at > c.last_closed_at)) as delivered_on from confirmed_findings c;',
    "create view recurring_categories as select c.category, count(*) as sites, json_group_array(c.path || ':' || c.start_line) as sites_at, min(c.confirmed_at) as first_at, max(c.confirmed_at) as last_at, (select json_group_array(distinct d.lineage_key) from finding_delivery d join confirmed_findings s on s.category = c.category and s.finding_id = d.finding_id where d.channel = 'report' and (s.last_closed_at is null or d.delivered_at > s.last_closed_at)) as reported_on from confirmed_findings c left join bar_verdict v on v.verdict = c.verdict group by c.category having count(*) >= 2 and count(*) filter (where v.earns = 0) = 0;",
    "create view judged_edits as select e.session_id, e.prompt_id, e.agent_id, e.ts, e.tool, e.tool_use_id, e.file_path, e.cwd, j.lineage_key from edited_files e join judged_range j on j.kind = 'edit' and e.ts between j.from_ts and j.to_ts and instr(e.cwd || '/', j.worktree || '/') = 1 where instr(e.file_path, e.cwd || '/') = 1 and not exists (select 1 from judged_range k where k.kind = 'edit' and length(k.worktree) > length(j.worktree) and instr(e.cwd || '/', k.worktree || '/') = 1);",
    "create view unjudged_edits as select e.session_id, e.prompt_id, e.agent_id, e.ts, e.tool, e.tool_use_id, e.file_path, e.cwd from edited_files e left join judged_edits x on x.tool_use_id = e.tool_use_id where instr(e.file_path, e.cwd || '/') = 1 and x.tool_use_id is null;",
    "create view category_fires as select f.checker, f.category, count(distinct f.finding_id) as sites, count(t.rowid) as sightings, count(distinct f.prompt_id) as prompts_fired, min(t.at) as first_at, max(t.at) as last_at from finding f left join finding_transition t on t.finding_id = f.finding_id and t.actor = 'check' where f.checker is not null group by f.checker, f.category;",
    "create view missed_sites as select finding_id, category, path, start_line, start_column, evidence, at from finding_state where state = 'checker_silent';",
];
const OPEN = bound(
    { rows: Object.fromEntries(_TABLES.flatMap(([name, _body, rows]) => (rows === undefined ? [] : [[name, rows]]))) },
    [
        'pragma journal_mode=wal;',
        ..._TABLES.map(([name, body]) => `create temp table ${name}${body};`),
        ..._INDEXES,
        'begin immediate;',
        `.output ${DELTA}`,
        "select 'drop ' || m.type || ' ' || m.name || ';' from sqlite_master m left join sqlite_temp_master w on w.type = m.type and lower(w.name) = lower(m.name) where m.type = 'view' or (m.type = 'index' and m.sql <> w.sql);",
        "select 'create table ' || w.name || '__delta' || substr(w.sql, instr(w.sql, '(')) || ';' || char(10) || 'insert into ' || w.name || '__delta(' || coalesce(c.cols, '') || ') select ' || coalesce(c.cols, '') || ' from ' || w.name || ';' || char(10) || 'drop table ' || w.name || ';' || char(10) || 'alter table ' || w.name || '__delta rename to ' || w.name || ';' from sqlite_temp_master w join sqlite_master m on m.type = 'table' and lower(m.name) = lower(w.name) and substr(m.sql, instr(m.sql, '(')) <> substr(w.sql, instr(w.sql, '(')) left join (select t.name as tbl, group_concat(p.name, ', ' order by p.cid) as cols from sqlite_temp_master t, pragma_table_info(t.name, 'temp') p join pragma_table_info(t.name, 'main') q on q.name = p.name group by t.name) c on c.tbl = w.name;",
        '.output',
        ..._TABLES.map(([name]) => `drop table temp.${name};`),
        `.read ${DELTA}`,
        ..._TABLES.map(([name, body]) => `create table if not exists ${name}${body};`),
        ..._INDEXES,
        `.output ${DELTA}`,
        `select 'delete from ' || l.key || ' where ' || k.name || ' not in (select value ->> ' || quote('$.' || k.name) || ' from json_each(' || quote(l.value) || '))' || coalesce((select group_concat(' and not exists (select 1 from ' || m.name || ' r where r.' || f."from" || ' = ' || l.key || '.' || k.name || ')', '') from sqlite_master m, pragma_foreign_key_list(m.name) f where m.type = 'table' and f."table" = l.key), '') || ';' || char(10) || 'insert into ' || l.key || '(' || (select group_concat(c.name, ', ') from pragma_table_info(l.key) c) || ') select ' || (select group_concat('value ->> ' || quote('$.' || c.name), ', ') from pragma_table_info(l.key) c) || ' from json_each(' || quote(l.value) || ') where true on conflict do ' || coalesce('update set ' || (select group_concat(c.name || ' = excluded.' || c.name, ', ') from pragma_table_info(l.key) c where c.pk = 0), 'nothing') || ';' from json_each(:rows) l, pragma_table_info(l.key) k where k.pk = 1;`,
        '.output',
        `.read ${DELTA}`,
        ..._VIEWS,
        'commit;',
    ].join('\n'),
);

// --- [STATEMENTS] ----------------------------------------------------------------------

const INSERT = `insert into observation(${_OBSERVATION.map(([name]) => name).join(', ')}) values (${_OBSERVATION.map(([name]) => `:${name}`).join(', ')});`;
const BOUNDARY = `insert into temp.sqlite_parameters(key, value) values (':key', ${_lineage(':main', ':worktree', ':branch')});
create temp table decision as with r(f) as (select coalesce(max(to_ts), 0) from judged_range where kind = 'edit' and lineage_key = :key), e as materialized (select v.session_id, v.file_path, v.agent_id from unjudged_edits v, r where v.ts > r.f and v.ts <= :to and instr(v.cwd || '/', :worktree || '/') = 1), a as materialized (select count(*) filter (where agent_id in (select agent_id from e)) = 0 as quiet, count(*) filter (where agent_type = :editAgent and instr(cwd || '/', :worktree || '/') = 1) = 0 as rangeIdle, count(*) filter (where agent_type = :categoryAgent and instr(cwd || '/', :worktree || '/') = 1) = 0 as categoryIdle from running_agents), c(category) as (select category from recurring_categories where sites >= :categoryThreshold and not exists (select 1 from json_each(reported_on) where value = :key) order by sites desc, category limit 1), spawn(value) as (select json_object('kind', 'range', 'agent', :editAgent, 'from', r.f, 'to', :to) from r, a where :editThreshold > 0 and a.quiet and a.rangeIdle and exists (select 1 from e where session_id = :session) and (select count(distinct file_path) from e) >= :editThreshold union all select json_object('kind', 'category', 'agent', :categoryAgent, 'category', c.category) from c, a where :categoryThreshold > 0 and a.quiet and a.categoryIdle) select a.quiet and a.rangeIdle as idle, (select json_group_array(json(value)) from spawn) as spawns from a;
begin immediate;
create temp table told as select finding_id from open_findings where (select idle from decision) and instr(${_normalized('cast(readfile(path) as text)')}, ntext) > 0 and not exists (select 1 from json_each(delivered_on) where value = :key);
insert into finding_delivery(finding_id, lineage_key, session_id, channel, delivered_at) select finding_id, :key, :session, 'additionalContext', :to from told;
select json_object('key', :key, 'spawns', json(spawns), 'findings', (select json_group_array(finding_id) from told)) from decision;
commit;`;
const JUDGE = "insert into judged_range(kind, main_worktree, worktree, branch, from_ts, to_ts, agent_id, at) values ('edit', :main, :worktree, :branch, :from, :to, :id, :at);";
const REPORT = "insert into finding_delivery(finding_id, lineage_key, session_id, agent_id, channel, delivered_at) select finding_id, :key, :session, :id, 'report', :at from confirmed_findings where category = :category;";

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Column };
export { BOUNDARY, bound, DELTA, INSERT, JUDGE, OPEN, REPORT, sqlite };
