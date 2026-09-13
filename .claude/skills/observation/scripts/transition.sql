-- One transition on a finding at its current path and hash, :finding_id the row, :state and :by from the states table, :evidence its evidence or null, :verdict a bar_verdict row on the rule builder's confirmed or null, run from the worktree
pragma foreign_keys = on;
insert into finding_transition(finding_id, state, subject_hash, path, at, by, evidence, verdict)
select finding_id, :state, lower(hex(sha3(readfile(path), 256))), path, cast(unixepoch('subsec') * 1000 as integer), :by, :evidence, :verdict
from finding_state
where finding_id = :finding_id
returning finding_id, state;
