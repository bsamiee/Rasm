// --- [MODELS] --------------------------------------------------------------------------

declare module 'claude-code' {
    interface BuiltinToolInputs {
        AskUserQuestion: {
            questions: { question: string; header: string; options: { label: string; description?: string }[]; multiSelect: boolean }[];
            metadata?: { source?: string };
        };
    }
    interface PluginState {
        varasi: { holds: Shaped<Holds> };
    }
}

export interface Holds {
    readonly held: readonly string[];
    readonly rows: Readonly<Record<string, string>>;
}
