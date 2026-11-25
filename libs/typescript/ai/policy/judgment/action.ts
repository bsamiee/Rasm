import { Schema, SchemaTransformation } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Action = typeof Action.Type;
type Kind = keyof typeof kinds;

// --- [MODELS] --------------------------------------------------------------------------

const call = { tool_use_id: Schema.String, agentId: Schema.OptionFromOptionalKey(Schema.String) };
const rest = [Schema.Record(Schema.String, Schema.Unknown)] as const;
const background = Schema.Union([Schema.Boolean, Schema.Literal('true')]).pipe(Schema.decodeTo(Schema.Boolean, SchemaTransformation.transform({ decode: (flag) => flag === true || flag === 'true', encode: (flag) => flag })));
const kinds = {
    Shell: Schema.StructWithRest(Schema.Struct({ tool: Schema.Literals(['Bash', 'Monitor']), ...call, command: Schema.String, run_in_background: Schema.optionalKey(background) }), rest),
    Read: Schema.StructWithRest(Schema.Struct({ tool: Schema.Literal('Read'), ...call, file_path: Schema.String }), rest),
    Write: Schema.StructWithRest(Schema.Struct({ tool: Schema.Literal('Write'), ...call, file_path: Schema.String, content: Schema.String }), rest),
    Edit: Schema.StructWithRest(Schema.Struct({ tool: Schema.Literal('Edit'), ...call, file_path: Schema.String, old_string: Schema.String, new_string: Schema.String }), rest),
    Fetch: Schema.Union([
        Schema.StructWithRest(Schema.Struct({ tool: Schema.Literal('WebFetch'), ...call, url: Schema.String, prompt: Schema.String }), rest),
        Schema.StructWithRest(Schema.Struct({ tool: Schema.Literal('Monitor'), ...call, ws: Schema.Struct({ url: Schema.String, protocols: Schema.optionalKey(Schema.Array(Schema.String)) }) }), rest),
    ]),
    Mcp: Schema.StructWithRest(Schema.Struct({ tool: Schema.TemplateLiteral(['mcp__', Schema.NonEmptyString, '__', Schema.NonEmptyString]), ...call }), rest),
    Spawn: Schema.StructWithRest(Schema.Struct({ tool: Schema.Literal('Agent'), ...call, prompt: Schema.String, model: Schema.OptionFromOptionalKey(Schema.String) }), rest),
};
const Action = Object.assign(Schema.Union([kinds.Shell, kinds.Read, kinds.Write, kinds.Edit, kinds.Fetch, kinds.Mcp, kinds.Spawn]), kinds);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Action, type Kind };
