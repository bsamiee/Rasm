-- Hit table of one-based lines and character columns the ruff, biome, and roslyn scripts fill before span.sql, no parameter
create temp table hit(
    category text not null,
    path text not null,
    file text not null,
    start_line integer not null,
    start_column integer not null,
    end_line integer not null,
    end_column integer not null,
    severity text,
    message text not null,
    source text not null
) strict;
