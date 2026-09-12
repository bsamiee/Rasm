-- One bar_verdict row, :verdict its name from the judging rubric, :earns 1 where the verdict earns a rule, updated in place on a repeat
insert into bar_verdict(verdict, earns)
values (:verdict, :earns)
on conflict(verdict) do update set earns = excluded.earns;
