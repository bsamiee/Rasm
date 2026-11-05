import { Schema } from 'effect';

// --- [ERRORS] --------------------------------------------------------------------------

class WorkspaceError extends Schema.TaggedError<WorkspaceError, { readonly brand: unique symbol }>()('WorkspaceError', {
    reason: Schema.TaggedUnion({ Missing: { value: Schema.String }, Shadowed: { folder: Schema.String }, Single: { module: Schema.String }, Unparsed: { error: Schema.String } }),
    subject: Schema.String,
}) {
    static readonly missing = (subject: string, value: string) => WorkspaceError.make({ reason: { _tag: 'Missing', value }, subject });
    static readonly shadowed = (subject: string, folder: string) => WorkspaceError.make({ reason: { _tag: 'Shadowed', folder }, subject });
    static readonly single = (subject: string, module: string) => WorkspaceError.make({ reason: { _tag: 'Single', module }, subject });
    static readonly unparsed = (subject: string, error: string) => WorkspaceError.make({ reason: { _tag: 'Unparsed', error }, subject });

    override get message(): string {
        return WorkspaceError.fields.reason.match(this.reason, {
            Missing: ({ value }) => `${this.subject} has no ${value}`,
            Shadowed: ({ folder }) => `${this.subject} shares its name with folder ${folder}, move it into the folder`,
            Single: ({ module }) => `${this.subject} re-exports only ${module}, point its exports entry at that module`,
            Unparsed: ({ error }) => `${this.subject} does not parse, its barrel names no export: ${error}`,
        });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { WorkspaceError };
