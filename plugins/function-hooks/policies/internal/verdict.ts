import { Option } from '../../monads.ts';
import type { CommandRequest, System } from '../command-decision.ts';
import type { Command, parse, Script, Word } from './command.ts';
import { type Invocation, programName, unquote } from './invocation.ts';
import type { Target } from './nx-target.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Refusal {
    readonly kind: 'refuse';
    readonly reason: string;
}
type Verdict = Refusal | { readonly kind: 'edit'; readonly start: number; readonly end: number; readonly text: string; readonly notice: Option<string>; readonly timeout: Option<number> } | { readonly kind: 'background'; readonly notice: string };
type Route = Refusal | { readonly kind: 'rewrite'; readonly program: string; readonly start: Word; readonly text: string };

interface Call {
    readonly system: System;
    readonly source: Uint8Array;
    readonly script: Script;
    readonly project: string;
    readonly request: CommandRequest;
    readonly apps: ReadonlyMap<string, string>;
    readonly writers: ReadonlyMap<string, string>;
    readonly routes: ReadonlyMap<Command, Route>;
    readonly projects: Map<string, Promise<Option<{ readonly name: string; readonly targets: Readonly<Record<string, Target>> }>>>;
    readonly parses: Map<string, ReturnType<typeof parse>>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const UNRESOLVED_FOLDER = 'runs in an unresolved folder, cd to a literal path';
const MAX_TIMEOUT = 600_000;
const EXPANSION = /[$`]/u;
const SHELL_SAFE = /^[\w@%+=:,./-]+$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [VERDICTS]

const refuse = (reason: string): Refusal => ({ kind: 'refuse', reason });

const edit = (start: number, end: number, text: string, notice: Option<string> = Option.none, timeout: Option<number> = Option.none): Verdict => ({ kind: 'edit', start, end, text, notice, timeout });

const background = (cause: string): Verdict => ({ kind: 'background', notice: `Call runs in the background for ${cause}, wait for its completion notice` });

const editOrRefuse = (command: Command, change: Verdict, reason: string): Verdict => (command.origin === 'call' ? change : refuse(reason));

// --- [WORDS]

const quote = (word: string): string => (SHELL_SAFE.test(word) ? word : `'${word.replaceAll("'", String.raw`'\''`)}'`);

const literalArgument = ({ expands, raw }: Word): boolean => !expands || SHELL_SAFE.test(raw);

const expanded = (words: readonly Word[], text: string): string => {
    const [spoken] = words.filter((word) => word.expands && word.text === text);
    return spoken === undefined || !EXPANSION.test(text) ? text : unquote(spoken.literal);
};

// --- [COMMANDS]

const invocationsOf = (commands: readonly Command[], ...programs: readonly string[]): readonly Invocation[] => commands.flatMap(({ invocations: chain }) => chain.filter(([program]) => programs.includes(program) || programs.includes(programName(program))));

const invocationSites = (commands: readonly Command[]): readonly { readonly command: Command; readonly index: number; readonly invocation: Invocation; readonly at: number }[] => commands.flatMap((command, index) => command.invocations.map((invocation, at) => ({ command, index, invocation, at })));

const nestedIn = (commands: readonly Command[], command: Command): readonly Command[] => commands.filter((other) => other !== command && other.extent.start >= command.extent.start && other.extent.end <= command.extent.end);

const pipelineNeighbors = (commands: readonly Command[], index: number, step: 1 | -1): readonly Command[] => {
    const next = commands[index + step];
    return (step > 0 ? commands[index] : next)?.pipesOutput === true && next !== undefined ? [next, ...pipelineNeighbors(commands, index + step, step)] : [];
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { background, type Call, EXPANSION, edit, editOrRefuse, expanded, invocationSites, invocationsOf, literalArgument, MAX_TIMEOUT, nestedIn, pipelineNeighbors, quote, type Route, refuse, UNRESOLVED_FOLDER, type Verdict };
