// --- [TYPES] ---------------------------------------------------------------------------

declare module 'claude-code' {
    interface PluginState {
        risman: { rows: RismanRows };
    }
}

export type RismanRows = Readonly<Record<string, Readonly<Record<string, string>>>>;
