-- Messages, model, and tokens of one transcript under DuckDB, variable transcript a Stop transcript_path or a SubagentStop agent_transcript_path
select
    count(*) filter (type = 'assistant') as messages,
    any_value(message.model) as model,
    sum(message.usage.output_tokens)::bigint as output_tokens,
    sum(message.usage.cache_read_input_tokens)::bigint as cache_read_tokens,
    sum(message.usage.cache_creation_input_tokens)::bigint as cache_creation_tokens
from read_json_auto(getvariable('transcript'), union_by_name = true, ignore_errors = true);
