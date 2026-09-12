-- Gate counts of one shape-verifier run, :id its agent id, :start its SubagentStart ts, :ids a JSON array of the scope's finding ids, one named array per select, touched equals the id count and every other count is 0, run from the worktree
select count(*) as touched from finding_state where finding_id in (select value from json_each(:ids)) and at >= :start;
select count(*) as proposed from finding_state where finding_id in (select value from json_each(:ids)) and state = 'proposed';
select count(*) as unevidenced from finding_transition where by = 'agent:' || :id and state in ('wrong', 'checker_owned', 'checker_silent') and evidence = '';
select count(*) as unjudged from finding_transition where by = 'agent:' || :id and state = 'confirmed' and verdict is null;
select count(*) as oversized from finding_state where by = 'agent:' || :id and state = 'confirmed' and length(replacement) >= length(text);
select count(*) as stale from finding_state where by = 'agent:' || :id and subject_hash <> lower(hex(sha3(readfile(path), 256)));
select count(*) as stale_closed from finding_state where finding_id in (select value from json_each(:ids)) and by = 'check:sqlite3' and subject_hash <> lower(hex(sha3(readfile(path), 256)));
select count(*) as edits from edited_files where agent_id = :id;
