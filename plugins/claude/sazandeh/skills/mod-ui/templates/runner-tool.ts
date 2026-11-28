import type { Element } from '@rasm/claude-code/element';
import { Engine } from '@rasm/claude-code/engine';
import { Plugin, Tool } from '@rasm/claude-code/events';
import { host } from '@rasm/claude-code/host';
import type { On } from 'claude-code';
import { Effect, Layer, Option, Record, Schema, Stream } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const NAMES = { tool: 'survey', agent: 'surveyor' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Dispatch = Schema.Struct({ subagent_type: Schema.String });
const Survey = Schema.Struct({
    question: Schema.String.annotate({ description: 'What the surveyor answers from the files of the working directory' }),
});

// --- [COMPOSITION] ---------------------------------------------------------------------

function register(on: On) {
    host(on, {
        layer: Layer.empty,
        hooks: {
            'tool.check': (e, next) =>
                Plugin.use(({ tools, agents }) =>
                    Option.contains(Record.get(tools, NAMES.tool), e.tool) || (e.tool === 'Agent' && Option.exists(Schema.decodeUnknownOption(Dispatch)(e.input), ({ subagent_type }) => Option.contains(Record.get(agents, NAMES.agent), subagent_type))) ? Effect.succeed({ decision: 'allow' as const }) : next(e),
                ),
        },
        tools: {
            [NAMES.tool]: Tool(Survey, {
                description: 'Answer a question about the working directory in a read-only surveyor agent, returning its report',
                isDeferred: false,
                run: Effect.fnUntraced(function* ({ question }) {
                    const [engine, { agents }] = yield* Effect.all([Engine, Plugin]);
                    return yield* Option.match(Record.get(agents, NAMES.agent), {
                        onNone: () => Effect.succeed({ deny: `${NAMES.tool}: refused, agent type ${NAMES.agent} is not registered` }),
                        onSome: (agent) => Effect.map(engine.callTool({ tool: 'Agent', input: { subagent_type: agent, description: question.slice(0, 40), prompt: question } }), (called) => (called.deny === undefined ? { result: called.text ?? `${NAMES.agent}: started` } : { deny: called.deny })),
                    });
                }),
            }),
        },
        agents: {
            [NAMES.agent]: {
                description: 'Answers one question about the working directory from its files',
                prompt: 'Answer the question from the files of the working directory. Read, never write. Reply in report lines: subject line, then lowercase label: value facts.',
                tools: ['Read', 'Grep', 'Glob'],
                isOffered: false,
            },
        },
        render: {
            state: Stream.unwrap(Plugin.use(({ tools }) => Record.get(tools, NAMES.tool).pipe(Stream.make, Effect.succeed))),
            ToolUse: (e, tool, next) => (Option.contains(tool, e.props.tool) && e.props.isRunning && e.props.isExpanded !== true ? Effect.map(next(), (row): Element => ({ _tag: 'Box', flexDirection: 'column', children: [row, { _tag: 'Progress', label: `${NAMES.agent} reads the working directory` }] })) : next()),
        },
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
