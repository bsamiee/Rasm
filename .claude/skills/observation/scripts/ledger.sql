-- Ledger row of one range run, :main, :worktree, and :branch the lineage, :from_ts and :to_ts the range prompt's bounds, :id the agent, the next range opens at the lineage's last to_ts
pragma foreign_keys = on;
insert into judged_range(kind, main_worktree, worktree, branch, from_ts, to_ts, agent_id, at)
values ('edit', :main, :worktree, :branch, :from_ts, :to_ts, :id, cast(unixepoch('subsec') * 1000 as integer));
