-- Binds :state confirmed and :actor check, then creates the hit table of one-based lines and character columns each checker script fills before span.sql
.param set :state 'confirmed'
.param set :actor 'check'
create temp table hit(
    category text not null,
    path text not null,
    start_line integer not null,
    start_column integer not null,
    end_line integer not null,
    end_column integer not null,
    severity text,
    message text not null,
    replacement text
) strict;
