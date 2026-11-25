import { Array, Data, type Effect, Formatter, Inspectable, Option, Redactable, Redacted, Schema } from 'effect';
import type { CommandLine } from 'effect/process';
import type { BaseNode, File, Word, WordPart } from 'sh-syntax';
import type { Invocation, Place } from '#/analysis/invocation';
import { expanded } from '#/evaluation/expansion';
import { Role, type Expansion, type Scope } from '#/evaluation/value';
import type { Dialect } from '#/syntax/dialect';

// --- [MODELS] --------------------------------------------------------------------------

class Script extends Data.Class<{
    readonly source: string;
    readonly dialect: Dialect;
    readonly place: Place;
    readonly scope: Scope;
    readonly syntax: File;
    readonly located: (offset: number) => number;
    readonly sites: ReadonlyMap<Word, { readonly role: Role; readonly braces: Option.Option<Word> }>;
    readonly redacted: readonly { readonly start: number; readonly end: number; readonly shown: Redacted.Redacted<unknown> }[];
    readonly invocations: Effect.Effect<readonly Invocation[], ShellError>;
}> {
    argv(...operands: readonly CommandLine.Word[]): Array.NonEmptyReadonlyArray<CommandLine.Word> {
        return Array.make(this.dialect.binary, '-c', Array.isReadonlyArrayNonEmpty(this.redacted) ? Redacted.make(this.source) : this.source, this.dialect.binary, ...operands);
    }

    expand(word: Word, scope: Scope = this.scope, dialect: Dialect = this.dialect, role?: Role): readonly [Expansion, Scope] {
        const site = Option.fromUndefinedOr(this.sites.get(word));
        const { expansion, scope: next } = expanded(word, scope, dialect, {
            braces: Option.flatMap(site, ({ braces }) => braces),
            role:
                role ??
                Option.getOrElse(
                    Option.map(site, ({ role: placed }) => placed),
                    Role.Argument,
                ),
            secret: (part: WordPart) => this.redacted.some(({ start, end }) => this.located(part.start) < end && this.located(part.end) > start),
        });
        return [expansion, next];
    }

    text(node: BaseNode): string {
        return this.source.slice(this.located(node.start), this.located(node.end));
    }

    override toString() {
        const [shown, rest] = this.redacted.reduce<readonly [string, number]>(([text, at], { start, end, shown: hidden }) => [`${text}${this.source.slice(at, start)}${Formatter.format(hidden)}`, end], ['', 0]);
        return `${shown}${this.source.slice(rest)}`;
    }

    toJSON() {
        return this.toString();
    }

    [Inspectable.NodeInspectSymbol]() {
        return this.toString();
    }

    [Redactable.symbolRedactable]() {
        return this.toString();
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

class ShellError extends Schema.TaggedError<ShellError, { readonly brand: unique symbol }>()('ShellError', {
    reason: Schema.TaggedUnion({
        Syntax: { offset: Schema.Option(Schema.Int), incomplete: Schema.Boolean },
        Dialect: {},
        Construction: { hole: Schema.Int, kind: Schema.Literals(['Value', 'Escape', 'Position', 'Delimiter']) },
        Edit: { offset: Schema.Int, kind: Schema.Literals(['Value', 'Position', 'Delimiter', 'Overlap']) },
        Host: {},
    }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
    message: Schema.String,
}) {
    static readonly of = (reason: ShellError['reason'], subject: string) => (cause?: unknown) => ShellError.make({ reason, subject, ...(cause === undefined ? {} : { cause }), message: [`${reason._tag}: ${subject}`, ...(cause === undefined ? [] : [Formatter.format(cause)])].join(': ') });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Script, ShellError };
