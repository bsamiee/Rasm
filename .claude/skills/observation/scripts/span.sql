-- Hit rows to site rows, text and bytes sliced from each file read once, a zero-width hit writes text '' at occurrence 1, :actor_id the checker
create temp table content as select path, readfile(path) as blob from hit group by path;
.read .claude/skills/observation/scripts/line.sql
with span as (
    select
        h.*,
        s.pos + length(cast(substr(s.text, 1, h.start_column - 1) as blob)) as byte_start,
        e.pos + length(cast(substr(e.text, 1, h.end_column - 1) as blob)) as byte_end,
        c.blob
    from hit h
    join content c on c.path = h.path
    join line s on s.path = h.path and s.n = h.start_line
    join line e on e.path = h.path and e.n = h.end_line
)
insert into site(checker, category, path, text, occurrence, start_line, start_column, end_line, end_column, byte_start, byte_end, subject_hash, severity, message, replacement)
select
    :actor_id,
    category,
    path,
    cast(substr(blob, byte_start, byte_end - byte_start) as text),
    (length(substr(blob, 1, byte_start - 1)) - length(cast(replace(substr(blob, 1, byte_start - 1), substr(blob, byte_start, byte_end - byte_start), x'') as blob))) / max(byte_end - byte_start, 1) + 1,
    start_line,
    start_column,
    end_line,
    end_column,
    byte_start - 1,
    byte_end - 1,
    lower(hex(sha3(blob, 256))),
    severity,
    message,
    replacement
from span;
