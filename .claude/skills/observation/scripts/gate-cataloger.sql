-- Gate counts of one shape-cataloger run, :id its agent id, one named array per select, every count 0 but ranges, 1 on a range prompt, run from the worktree
select count(*) as stale from finding_state where state in ('proposed', 'confirmed') and subject_hash <> lower(hex(sha3(readfile(path), 256)));
select count(*) as proposed from finding_state where by = 'agent:' || :id and state = 'proposed';
select count(*) as oversized from finding_state where finding_id in (select finding_id from finding_transition where by = 'agent:' || :id) and length(replacement) >= length(text);
select count(*) as uncovered from finding_state s where finding_id in (select finding_id from finding_transition where by = 'agent:' || :id and state = 'checker_owned') and evidence not in (select category from finding where path = s.path);
select count(*) as ranges from judged_range where agent_id = :id;
select count(*) as edits from edited_files where agent_id = :id;
