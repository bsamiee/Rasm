-- Confirms Roslyn results with a location as sites, :worktree the built tree, :out the text of the SARIF 2.1 log `dotnet build -p:ErrorLog=<log>%2Cversion=2.1` wrote, run from :worktree
.read .claude/skills/observation/scripts/site.sql
.read .claude/skills/observation/scripts/hit.sql
.param set :actor_id 'roslyn'
insert into hit
select
    value ->> '$.ruleId',
    substr(value ->> '$.locations[0].physicalLocation.artifactLocation.uri', length('file://' || :worktree) + 2),
    value ->> '$.locations[0].physicalLocation.region.startLine',
    value ->> '$.locations[0].physicalLocation.region.startColumn',
    value ->> '$.locations[0].physicalLocation.region.endLine',
    value ->> '$.locations[0].physicalLocation.region.endColumn',
    value ->> '$.level',
    value ->> '$.message.text',
    null
from json_each(:out, '$.runs[0].results')
where value ->> '$.locations[0].physicalLocation.artifactLocation.uri' is not null;
.read .claude/skills/observation/scripts/span.sql
.read .claude/skills/observation/scripts/insert.sql
