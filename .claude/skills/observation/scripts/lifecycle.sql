-- Closes, moves, and reconfirms every live finding against disk and the edits since its transition in one transaction, :worktree the tree the paths are relative to, run from it
-- Live states are proposed, confirmed, checker_owned, checker_silent, and waived, the site on disk at its last transition
-- Removed line holding the text, or a write with content lacking it, is fixed, an edit or `git mv` since the transition placing it elsewhere is moved, text gone otherwise is vanished, a confirmed row present at a new hash is reconfirmed, every other live row keeps its state
-- Spans locate the raw text's nth occurrence, null where whitespace alone differs, a site another finding holds at the new path closes as vanished, a zero-width site moves by `git mv` alone
pragma foreign_keys = on;
begin immediate;
create temp table content(
    path text primary key,
    blob blob,
    ntext text generated always as (replace(replace(replace(replace(replace(replace(cast(blob as text), char(9), ' '), char(13), ' '), char(10), ' '), ' ', char(64976, 64977)), char(64977, 64976), ''), char(64976, 64977), ' ')) stored
);
insert into content(path, blob) select distinct path, readfile(path) from finding_state where state in ('proposed', 'confirmed', 'checker_owned', 'checker_silent', 'waived');
create temp table head as
select
    s.finding_id, s.category, s.path, s.text, s.ntext, s.text_hash, s.occurrence, s.at, s.state, s.evidence, s.verdict,
    s.subject_hash as last_hash,
    lower(hex(sha3(c.blob, 256))) as hash,
    coalesce(instr(c.ntext, s.ntext) > 0, 0) as present
from finding_state s
join content c on c.path = s.path
where s.state in ('proposed', 'confirmed', 'checker_owned', 'checker_silent', 'waived');
create temp table edit as
select o.ts, o.tool_use_id, e.file_path, o.payload
from edited_files e
join observation o on o.tool_use_id = e.tool_use_id and o.event = 'PostToolUse'
where e.ts > (select min(at) from head) and substr(e.file_path, 1, length(:worktree) + 1) = :worktree || '/';
create temp table removal as
select h.finding_id, e.tool_use_id, max(e.ts) as ts
from head h
join edit e on e.file_path = :worktree || '/' || h.path and e.ts > h.at
where h.present = 0
    and (
        exists (
            select 1 from json_each(e.payload, '$.tool_response.structuredPatch') p
            where instr((select group_concat(substr(l.value, 2), char(10)) from json_each(p.value, '$.lines') l where substr(l.value, 1, 1) = '-'), h.text) > 0
        )
        or instr(e.payload ->> '$.tool_input.content', h.text) = 0
    )
group by h.finding_id;
create temp table move as
select h.finding_id, substr(e.file_path, length(:worktree) + 2) as new_path, max(e.ts) as ts
from head h
join edit e on e.ts > h.at and e.file_path <> :worktree || '/' || h.path
where h.present = 0
    and h.text <> ''
    and h.finding_id not in (select finding_id from removal)
    and (
        instr(e.payload ->> '$.tool_input.content', h.text) > 0
        or exists (
            select 1 from json_each(e.payload, '$.tool_response.structuredPatch') p
            where instr((select group_concat(substr(l.value, 2), char(10)) from json_each(p.value, '$.lines') l where substr(l.value, 1, 1) = '+'), h.text) > 0
        )
    )
group by h.finding_id;
with
command as (
    select ts, substr(payload ->> '$.tool_input.command', instr(payload ->> '$.tool_input.command', 'git mv ') + 7) as rest
    from observation
    where event = 'PostToolUse' and tool = 'Bash' and ts > (select min(at) from head) and payload ->> '$.tool_input.command' like '%git mv %'
),
renamed as (
    select h.finding_id, o.ts, substr(o.rest, length(h.path) + 2) as after
    from head h
    join command o on o.ts > h.at and substr(o.rest, 1, length(h.path) + 1) = h.path || ' '
    where h.present = 0 and h.finding_id not in (select finding_id from removal union select finding_id from move)
),
destination as (select finding_id, ts, rtrim(substr(after, 1, instr(after || ' ', ' ') - 1), ';') as new_path from renamed)
insert into move(finding_id, new_path, ts) select finding_id, new_path, max(ts) from destination group by finding_id;
insert into content(path, blob) select new_path, readfile(:worktree || '/' || new_path) from move where true on conflict do nothing;
delete from move
where exists (
        select 1 from finding_state x
        join head h on h.finding_id = move.finding_id
        where x.finding_id <> h.finding_id and x.path = move.new_path and x.category = h.category and x.text_hash = h.text_hash and x.occurrence = h.occurrence
    )
    or (select blob from content where path = move.new_path) is null
    or (select (length(c.ntext) - length(replace(c.ntext, h.ntext, ''))) / length(h.ntext) from content c join head h on h.finding_id = move.finding_id where c.path = move.new_path) < (select occurrence from head where finding_id = move.finding_id);
.read .claude/skills/observation/scripts/line.sql
create temp table site_at as
with
target as (
    select h.finding_id, h.path, c.blob, cast(h.text as blob) as text, h.occurrence
    from head h
    join content c on c.path = h.path
    where h.present = 1 and h.hash <> h.last_hash
    union all
    select m.finding_id, m.new_path, c.blob, cast(h.text as blob), h.occurrence
    from move m
    join head h on h.finding_id = m.finding_id
    join content c on c.path = m.new_path
),
walk as (
    select finding_id, 0 as n, 0 as pos from target
    union all
    select t.finding_id, walk.n + 1, walk.pos + instr(substr(t.blob, walk.pos + 1), t.text)
    from walk
    join target t on t.finding_id = walk.finding_id
    where walk.n < t.occurrence and instr(substr(t.blob, walk.pos + 1), t.text) > 0
),
found as (
    select t.finding_id, t.path, t.blob, walk.pos as byte_start, walk.pos + length(t.text) as byte_end
    from walk
    join target t on t.finding_id = walk.finding_id and walk.n = t.occurrence
)
select
    f.finding_id,
    s.n as start_line,
    length(cast(substr(f.blob, s.pos, f.byte_start - s.pos) as text)) + 1 as start_column,
    e.n as end_line,
    length(cast(substr(f.blob, e.pos, f.byte_end - e.pos) as text)) + 1 as end_column,
    f.byte_start - 1 as byte_start,
    f.byte_end - 1 as byte_end
from found f
join line s on s.path = f.path and s.pos = (select max(pos) from line where path = f.path and pos <= f.byte_start)
join line e on e.path = f.path and e.pos = (select max(pos) from line where path = f.path and pos <= f.byte_end);
insert into finding_transition(finding_id, state, subject_hash, path, at, by, evidence)
select r.finding_id, 'fixed', h.hash, h.path, cast(unixepoch('subsec') * 1000 as integer), 'check:sqlite3', r.tool_use_id
from removal r
join head h on h.finding_id = r.finding_id
returning finding_id, state, evidence;
insert into finding_transition(finding_id, state, subject_hash, path, start_line, start_column, end_line, end_column, byte_start, byte_end, occurrence, at, by)
select m.finding_id, 'moved', lower(hex(sha3(c.blob, 256))), m.new_path, a.start_line, a.start_column, a.end_line, a.end_column, a.byte_start, a.byte_end, h.occurrence, cast(unixepoch('subsec') * 1000 as integer), 'check:sqlite3'
from move m
join head h on h.finding_id = m.finding_id
join content c on c.path = m.new_path
left join site_at a on a.finding_id = m.finding_id
returning finding_id, state, path, start_line, end_column;
insert into finding_transition(finding_id, state, subject_hash, path, at, by)
select h.finding_id, 'vanished', h.hash, h.path, cast(unixepoch('subsec') * 1000 as integer), 'check:sqlite3'
from head h
where h.present = 0 and h.finding_id not in (select finding_id from removal union select finding_id from move)
returning finding_id, state, subject_hash;
insert into finding_transition(finding_id, state, subject_hash, path, start_line, start_column, end_line, end_column, byte_start, byte_end, occurrence, at, by, evidence, verdict)
select h.finding_id, 'confirmed', h.hash, h.path, a.start_line, a.start_column, a.end_line, a.end_column, a.byte_start, a.byte_end, h.occurrence, cast(unixepoch('subsec') * 1000 as integer), 'check:sqlite3', h.evidence, h.verdict
from head h
left join site_at a on a.finding_id = h.finding_id
where h.present = 1 and h.hash <> h.last_hash and h.state = 'confirmed'
returning finding_id, state, start_line, start_column, end_line, end_column;
commit;
