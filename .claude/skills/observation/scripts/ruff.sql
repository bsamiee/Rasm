-- Confirms ruff diagnostics as sites, :worktree the checked tree, :out the JSON `ruff check --output-format json` wrote, run from :worktree
.read .claude/skills/observation/scripts/site.sql
.read .claude/skills/observation/scripts/hit.sql
.param set :state 'confirmed'
.param set :by 'check:ruff'
insert into hit
select
    'ruff:' || (value ->> '$.code'),
    substr(value ->> '$.filename', length(:worktree) + 2),
    value ->> '$.filename',
    value ->> '$.location.row',
    value ->> '$.location.column',
    value ->> '$.end_location.row',
    value ->> '$.end_location.column',
    value ->> '$.severity',
    value ->> '$.message',
    'checker:ruff'
from json_each(cast(readfile(:out) as text));
.read .claude/skills/observation/scripts/span.sql
.read .claude/skills/observation/scripts/insert.sql
