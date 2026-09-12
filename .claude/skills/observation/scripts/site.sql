-- Opens a batch transaction with the site table every source fills, read by batch.sql and each checker script, no parameter
pragma foreign_keys = on;
begin immediate;
create temp table site(
    category text not null,
    path text not null,
    text text not null,
    ntext text generated always as (replace(replace(replace(replace(replace(replace(text, char(9), ' '), char(13), ' '), char(10), ' '), ' ', char(64976, 64977)), char(64977, 64976), ''), char(64976, 64977), ' ')) stored,
    text_hash text generated always as (lower(hex(sha3(ntext, 256)))) stored,
    occurrence integer not null,
    start_line integer not null,
    start_column integer not null,
    end_line integer not null,
    end_column integer not null,
    byte_start integer,
    byte_end integer,
    subject_hash text not null,
    severity text,
    message text not null,
    replacement text,
    source text not null
) strict;
