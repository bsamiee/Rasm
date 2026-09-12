-- Cost per subagent from its transcript joined to SubagentStop rows under DuckDB, background agents included, the sink attached as s on the command line since attach takes a literal path alone
select
    r.agent_id,
    any_value(r.agent_type) as agent_type,
    count(*) filter (t.type = 'assistant') as messages,
    any_value(t.message.model) as model,
    sum(t.message.usage.output_tokens)::bigint as output_tokens,
    sum(t.message.usage.cache_read_input_tokens)::bigint as cache_read_tokens,
    sum(t.message.usage.cache_creation_input_tokens)::bigint as cache_creation_tokens
from read_json_auto('~/.claude/projects/*/*/subagents/*.jsonl', union_by_name = true, ignore_errors = true) t
join (
    select agent_id, json_extract_string(payload, '$.agent_transcript_path') as path, json_extract_string(payload, '$.agent_type') as agent_type
    from s.observation
    where event = 'SubagentStop'
) r on r.path = t.filename
group by r.agent_id
order by output_tokens desc;
