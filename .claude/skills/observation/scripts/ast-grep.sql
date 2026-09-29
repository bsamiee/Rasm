-- Confirms ast-grep hits as sites, :worktree the scanned tree, :out the JSON `ast-grep scan --json=compact` printed over paths relative to it, run from :worktree
.read .claude/skills/observation/scripts/site.sql
.read .claude/skills/observation/scripts/hit.sql
.param set :actor_id 'ast-grep'
insert into hit
select
    value ->> '$.ruleId',
    value ->> '$.file',
    value ->> '$.range.start.line' + 1,
    value ->> '$.range.start.column' + 1,
    value ->> '$.range.end.line' + 1,
    value ->> '$.range.end.column' + 1,
    value ->> '$.severity',
    value ->> '$.message',
    value ->> '$.replacement'
from json_each(:out);
.read .claude/skills/observation/scripts/span.sql
.read .claude/skills/observation/scripts/insert.sql
