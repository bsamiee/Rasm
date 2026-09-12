-- Confirms biome diagnostics as sites, :worktree the checked tree, :out the JSON `biome lint --reporter=json` wrote over paths relative to it, run from :worktree
.read .claude/skills/observation/scripts/site.sql
.read .claude/skills/observation/scripts/hit.sql
.param set :state 'confirmed'
.param set :by 'check:biome'
insert into hit
select
    'biome:' || (value ->> '$.category'),
    value ->> '$.location.path',
    value ->> '$.location.path',
    value ->> '$.location.start.line',
    value ->> '$.location.start.column',
    value ->> '$.location.end.line',
    value ->> '$.location.end.column',
    value ->> '$.severity',
    value ->> '$.message',
    'checker:biome'
from json_each(cast(readfile(:out) as text), '$.diagnostics');
.read .claude/skills/observation/scripts/span.sql
.read .claude/skills/observation/scripts/insert.sql
