import type { Element } from '@rasm/claude-code/element';
import { Engine } from '@rasm/claude-code/engine';
import { Plugin, Tool } from '@rasm/claude-code/events';
import { host } from '@rasm/claude-code/host';
import type { AgentReportInput, On } from 'claude-code';
import { Context, Effect, Layer, Option, Record, Schema, Stream, SubscriptionRef } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Stance = 'running' | AgentReportInput['status'];

// --- [CONSTANTS] -----------------------------------------------------------------------

const NAMES = { tool: 'survey', agent: 'surveyor' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Dispatch = Schema.Struct({ subagent_type: Schema.String });
const Launched = Schema.Struct({ result: Schema.Struct({ agentId: Schema.String }) });
const Survey = Schema.Struct({
    question: Schema.String.annotate({ description: 'What the surveyor answers from the files of the working directory' }),
});

// --- [SERVICES] ------------------------------------------------------------------------

class Runs extends Context.Service<Runs, { readonly held: SubscriptionRef.SubscriptionRef<Record.ReadonlyRecord<string, { readonly agent: string; readonly stance: Stance }>> }>()('@rasm/sazandeh/skills/mod-ui/templates/runner-tool/Runs') {
    static readonly layer = Layer.effect(
        Runs,
        Effect.map(SubscriptionRef.make<Record.ReadonlyRecord<string, { readonly agent: string; readonly stance: Stance }>>({}), (held) => ({ held })),
    );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const launch = Effect.fnUntraced(function* (question: string, call: string, loop: Option.Option<string>) {
    const [engine, { agents }, { held }] = yield* Effect.all([Engine, Plugin, Runs]);
    const agent = yield* Effect.fromOption(Record.get(agents, NAMES.agent));
    const called = yield* engine.callTool({ tool: 'Agent', input: { subagent_type: agent, description: question.slice(0, 40), prompt: question }, ...Option.match(loop, { onNone: () => ({}), onSome: (agentId) => ({ agentId }) }) });
    return yield* Option.match(Schema.decodeUnknownOption(Launched)(called), {
        onNone: () => Effect.succeed({ deny: `${NAMES.tool}: refused, ${called.deny ?? called.text ?? 'the Agent tool started no agent'}` }),
        onSome: ({ result: { agentId } }) => Effect.as(SubscriptionRef.update(held, Record.set(call, { agent: agentId, stance: 'running' as const })), { result: `started ${NAMES.agent}\n  agent: ${agentId}\n  report: reaches this loop when the agent ends` }),
    });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

function register(on: On) {
    host(on, {
        layer: Runs.layer,
        hooks: {
            'tool.check': (e, next) =>
                Plugin.use(({ tools, agents }) =>
                    Option.contains(Record.get(tools, NAMES.tool), e.tool) || (e.tool === 'Agent' && Option.exists(Schema.decodeUnknownOption(Dispatch)(e.input), ({ subagent_type }) => Option.contains(Record.get(agents, NAMES.agent), subagent_type))) ? Effect.succeed({ decision: 'allow' as const }) : next(e),
                ),
            'agent.report': (e, next) =>
                Effect.andThen(
                    Runs.use(({ held }) =>
                        SubscriptionRef.update(
                            held,
                            Record.map((run) => (run.agent === e.agentId ? { ...run, stance: e.status } : run)),
                        ),
                    ),
                    next(e),
                ),
        },
        tools: {
            [NAMES.tool]: Tool(Survey, {
                description: 'Start a read-only surveyor agent on a question about the working directory: answers at once with its agent id, and its report reaches this loop when it ends',
                isDeferred: false,
                run: ({ question }, e) => Effect.catchNoSuchElement(launch(question, e.tool_use_id, Option.fromUndefinedOr(e.agentId))).pipe(Effect.map(Option.getOrElse(() => ({ deny: `${NAMES.tool}: refused, agent type ${NAMES.agent} is not registered` })))),
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
            state: Stream.unwrap(Runs.use(({ held }) => Effect.succeed(held.pipe(SubscriptionRef.changes)))),
            ToolUse: (e, runs, next) =>
                Option.match(
                    Option.filter(Record.get(runs, e.props.tool_use_id), () => e.props.isExpanded !== true),
                    {
                        onNone: () => next(),
                        onSome: ({ agent, stance }) => Effect.map(next(), (row): Element => ({ _tag: 'Box', flexDirection: 'column', children: [row, { _tag: 'Text', dimColor: true, children: `  ⎿ ${NAMES.agent} ${agent} ${stance}` }] })),
                    },
                ),
        },
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
