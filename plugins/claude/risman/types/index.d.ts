// --- [TYPES] ---------------------------------------------------------------------------

declare module 'claude-code' {
    interface PluginState {
        risman: { waiting: RismanWaiting };
    }
}

export type RismanWaiting = Readonly<Record<string, Readonly<Record<string, string>>>>;
