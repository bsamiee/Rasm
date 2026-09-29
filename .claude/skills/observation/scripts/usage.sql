-- Messages, models, and tokens per transcript file, each API message read from its content block holding the most output tokens
create macro usage(transcripts) as table
select
    filename as transcript,
    count(*) as messages,
    list(distinct message.model order by message.model) as models,
    sum(message.usage.input_tokens)::bigint as input_tokens,
    sum(message.usage.output_tokens)::bigint as output_tokens,
    sum(message.usage.cache_read_input_tokens)::bigint as cache_read_tokens,
    sum(message.usage.cache_creation_input_tokens)::bigint as cache_creation_tokens
from (
    select filename, arg_max(message, message.usage.output_tokens) as message
    from read_json(
        transcripts,
        filename = true,
        columns = {
            type: 'VARCHAR',
            isApiErrorMessage: 'BOOLEAN',
            message: 'STRUCT(id VARCHAR, model VARCHAR, usage STRUCT(input_tokens BIGINT, output_tokens BIGINT, cache_read_input_tokens BIGINT, cache_creation_input_tokens BIGINT))'
        }
    )
    where type = 'assistant' and isApiErrorMessage is not true
    group by filename, message.id
)
group by filename;
