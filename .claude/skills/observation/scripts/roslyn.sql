-- Confirms Roslyn results with a location as sites, :worktree the built tree, :out the SARIF 2.1 log `dotnet build -p:ErrorLog=<out>%2Cversion=2.1` wrote, run from :worktree
.read .claude/skills/observation/scripts/site.sql
.read .claude/skills/observation/scripts/hit.sql
.param set :state 'confirmed'
.param set :by 'check:roslyn'
insert into hit
select
    'roslyn:' || (value ->> '$.ruleId'),
    substr(value ->> '$.locations[0].physicalLocation.artifactLocation.uri', length('file://' || :worktree) + 2),
    substr(value ->> '$.locations[0].physicalLocation.artifactLocation.uri', 8),
    value ->> '$.locations[0].physicalLocation.region.startLine',
    value ->> '$.locations[0].physicalLocation.region.startColumn',
    value ->> '$.locations[0].physicalLocation.region.endLine',
    value ->> '$.locations[0].physicalLocation.region.endColumn',
    value ->> '$.level',
    value ->> '$.message.text',
    'checker:roslyn'
from json_each(cast(readfile(:out) as text), '$.runs[0].results')
where value ->> '$.locations[0].physicalLocation.artifactLocation.uri' is not null;
.read .claude/skills/observation/scripts/span.sql
.read .claude/skills/observation/scripts/insert.sql
