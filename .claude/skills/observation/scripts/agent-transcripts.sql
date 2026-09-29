-- Messages, models, and tokens per subagent from its transcript, one row per agent_transcript_path of SubagentStop rows in database s
.read .claude/skills/observation/scripts/usage.sql
select r.agent_id, r.agent_type, u.* exclude (transcript)
from usage('~/.claude/projects/*/*/subagents/*.jsonl') u
join (
    select distinct agent_id, payload ->> '$.agent_type' as agent_type, payload ->> '$.agent_transcript_path' as transcript
    from s.observation
    where event = 'SubagentStop'
) r using (transcript)
order by u.output_tokens desc;
