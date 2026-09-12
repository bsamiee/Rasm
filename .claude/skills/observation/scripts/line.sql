-- Line table over the content table, one row per line of each file with its one-based byte position and text, no parameter
-- Backslashes read as slashes, one byte each, so an escaped newline in the text splits no line
create temp table line as
select
    c.path,
    l.key + 1 as n,
    1 + coalesce(sum(length(cast(l.value as blob)) + 1) over (partition by c.path order by l.key rows between unbounded preceding and 1 preceding), 0) as pos,
    l.value as text
from content c, json_each('[' || replace(replace(json_quote(cast(c.blob as text)), '\\', '\/'), '\n', '","') || ']') l
where c.blob is not null;
