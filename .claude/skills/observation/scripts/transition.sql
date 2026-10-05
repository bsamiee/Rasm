-- One transition on a finding at its current path and hash, :finding_id the row, :state a transition_state row, :actor a transition_actor row, :actor_id null for user, else agent_id, session_id, or tool, :evidence text or null, :verdict a bar_verdict row on a category agent's confirmed or null, run from worktree
pragma foreign_keys = on;
insert into finding_transition(finding_id, state, subject_hash, path, at, actor, actor_id, evidence, verdict)
select finding_id, :state, lower(hex(sha3(readfile(path), 256))), path, cast(unixepoch('subsec') * 1000 as integer), :actor, :actor_id, :evidence, :verdict
from finding_state
where finding_id = :finding_id
returning finding_id, state;
