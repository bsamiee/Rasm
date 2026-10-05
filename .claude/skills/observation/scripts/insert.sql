-- Finding rows for sites no current path holds, one transition per site on the live or newest finding at its path, then commit, :worktree prefixes edited_files paths, calling script binds :state, :actor, and :actor_id
insert into finding(checker, category, path, text, occurrence, start_line, start_column, end_line, end_column, byte_start, byte_end, subject_hash, severity, message, replacement, session_id, prompt_id, agent_id, tool_use_id, observed_at)
select
    s.checker, s.category, s.path, s.text, s.occurrence, s.start_line, s.start_column, s.end_line, s.end_column, s.byte_start, s.byte_end,
    s.subject_hash, s.severity, s.message, s.replacement,
    e.session_id, e.prompt_id, e.agent_id, e.tool_use_id,
    cast(unixepoch('subsec') * 1000 as integer)
from site s
left join (select file_path, session_id, prompt_id, agent_id, tool_use_id, max(ts) as ts from edited_files group by file_path) e on e.file_path = :worktree || '/' || s.path
where not exists (select 1 from finding_state x where x.checker is not distinct from s.checker and x.category = s.category and x.text_hash = s.text_hash and x.occurrence = s.occurrence and x.path = s.path)
on conflict do nothing
returning finding_id;
insert into finding_transition(finding_id, state, subject_hash, path, start_line, start_column, end_line, end_column, byte_start, byte_end, occurrence, at, actor, actor_id, evidence)
select
    x.finding_id, :state, s.subject_hash, s.path,
    s.start_line, s.start_column, s.end_line, s.end_column, s.byte_start, s.byte_end, s.occurrence,
    cast(unixepoch('subsec') * 1000 as integer), :actor, :actor_id, s.message
from site s
join (
    select
        f.finding_id, f.checker, f.category, f.text_hash, f.occurrence, coalesce(t.path, f.path) as path,
        row_number() over (partition by f.checker, f.category, f.text_hash, f.occurrence, coalesce(t.path, f.path) order by k.live desc, t.at desc) as rank
    from finding f
    left join finding_state t on t.finding_id = f.finding_id
    left join transition_state k on k.state = t.state
) x on x.checker is not distinct from s.checker and x.category = s.category and x.text_hash = s.text_hash and x.occurrence = s.occurrence and x.path = s.path and x.rank = 1
returning finding_id;
commit;
