-- Finding rows for sites no current path holds, one transition per site on the finding current at its path, then commit, :worktree prefixes edited_files paths, :state and :by come from the calling script
-- Ids of a finding come from the last edited_files row of its file, a site matches the live finding by category, text_hash, occurrence, and current path, the first returning lists the ids new to finding, the second every site's id
insert into finding(category, path, text, occurrence, start_line, start_column, end_line, end_column, byte_start, byte_end, subject_hash, severity, message, replacement, source, session_id, prompt_id, agent_id, tool_use_id, observed_at)
select
    s.category, s.path, s.text, s.occurrence, s.start_line, s.start_column, s.end_line, s.end_column, s.byte_start, s.byte_end,
    s.subject_hash, s.severity, s.message, s.replacement, s.source,
    e.session_id, e.prompt_id, e.agent_id, e.tool_use_id,
    cast(unixepoch('subsec') * 1000 as integer)
from site s
left join (select file_path, session_id, prompt_id, agent_id, tool_use_id, max(ts) as ts from edited_files group by file_path) e on e.file_path = :worktree || '/' || s.path
where not exists (select 1 from finding_state x where x.category = s.category and x.text_hash = s.text_hash and x.occurrence = s.occurrence and x.path = s.path)
on conflict do nothing
returning finding_id;
insert into finding_transition(finding_id, state, subject_hash, path, start_line, start_column, end_line, end_column, byte_start, byte_end, occurrence, at, by, evidence)
select
    x.finding_id, :state, s.subject_hash, s.path,
    s.start_line, s.start_column, s.end_line, s.end_column, s.byte_start, s.byte_end, s.occurrence,
    cast(unixepoch('subsec') * 1000 as integer), :by, s.message
from site s
join (
    select
        f.finding_id, f.category, f.text_hash, f.occurrence, coalesce(t.path, f.path) as path,
        row_number() over (partition by f.category, f.text_hash, f.occurrence, coalesce(t.path, f.path) order by t.state in ('proposed', 'confirmed', 'checker_owned', 'checker_silent', 'waived') desc, t.at desc) as rank
    from finding f
    left join finding_state t on t.finding_id = f.finding_id
) x on x.category = s.category and x.text_hash = s.text_hash and x.occurrence = s.occurrence and x.path = s.path and x.rank = 1
returning finding_id;
commit;
