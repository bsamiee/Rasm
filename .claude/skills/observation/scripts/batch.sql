-- Proposes an agent's sites in one batch, :worktree the tree the paths are relative to, :sites a JSON array keyed by site columns, :id the writing agent, run from :worktree
.read .claude/skills/observation/scripts/site.sql
.param set :state 'proposed'
.param set :by "'agent:' || :id"
insert into site(category, path, text, occurrence, start_line, start_column, end_line, end_column, subject_hash, message, replacement, source)
select
    value ->> '$.category',
    value ->> '$.path',
    value ->> '$.text',
    value ->> '$.occurrence',
    value ->> '$.start_line',
    value ->> '$.start_column',
    value ->> '$.end_line',
    value ->> '$.end_column',
    lower(hex(sha3(readfile(value ->> '$.path'), 256))),
    value ->> '$.message',
    value ->> '$.replacement',
    'agent:' || :id
from json_each(cast(readfile(:sites) as text));
.read .claude/skills/observation/scripts/insert.sql
