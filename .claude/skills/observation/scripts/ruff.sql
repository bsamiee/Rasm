-- Confirms ruff diagnostics as sites, :worktree the checked tree, :out the JSON `ruff check --output-format json` printed, run from :worktree
.read .claude/skills/observation/scripts/site.sql
.read .claude/skills/observation/scripts/hit.sql
.param set :actor_id 'ruff'
insert into hit
select
    value ->> '$.code',
    substr(value ->> '$.filename', length(:worktree) + 2),
    value ->> '$.location.row',
    value ->> '$.location.column',
    value ->> '$.end_location.row',
    value ->> '$.end_location.column',
    value ->> '$.severity',
    value ->> '$.message',
    null
from json_each(:out);
.read .claude/skills/observation/scripts/span.sql
.read .claude/skills/observation/scripts/insert.sql
