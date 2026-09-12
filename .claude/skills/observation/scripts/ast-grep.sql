-- Confirms ast-grep hits as sites, text and bytes from the checker, :worktree the scanned tree, :out the JSON `ast-grep scan --json=compact` wrote over paths relative to it, run from :worktree
.read .claude/skills/observation/scripts/site.sql
.param set :state 'confirmed'
.param set :by 'check:ast-grep'
with match as (
    select
        value ->> '$.ruleId' as rule_id,
        value ->> '$.file' as file,
        value ->> '$.text' as text,
        value ->> '$.range.byteOffset.start' as byte_start,
        value ->> '$.range.byteOffset.end' as byte_end,
        value ->> '$.range.start.line' as start_line,
        value ->> '$.range.start.column' as start_column,
        value ->> '$.range.end.line' as end_line,
        value ->> '$.range.end.column' as end_column,
        value ->> '$.severity' as severity,
        value ->> '$.message' as message,
        value ->> '$.replacement' as replacement
    from json_each(cast(readfile(:out) as text))
),
content as (select distinct file, readfile(file) as blob from match)
insert into site(category, path, text, occurrence, start_line, start_column, end_line, end_column, byte_start, byte_end, subject_hash, severity, message, replacement, source)
select
    'ast-grep:' || m.rule_id,
    m.file,
    m.text,
    (length(substr(c.blob, 1, m.byte_start)) - length(cast(replace(substr(c.blob, 1, m.byte_start), cast(m.text as blob), x'') as blob))) / length(cast(m.text as blob)) + 1,
    m.start_line + 1,
    m.start_column + 1,
    m.end_line + 1,
    m.end_column + 1,
    m.byte_start,
    m.byte_end,
    lower(hex(sha3(c.blob, 256))),
    m.severity,
    m.message,
    m.replacement,
    'checker:ast-grep'
from match m
join content c on c.file = m.file;
.read .claude/skills/observation/scripts/insert.sql
