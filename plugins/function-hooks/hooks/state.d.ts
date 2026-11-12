// --- [TYPES] ---------------------------------------------------------------------------

declare module 'claude-code' {
    interface PluginState {
        'function-hooks': { rows: Shaped<Row[]> };
    }
}

export interface Row {
    pid: number;
    instance: string;
    bundle: string;
    agent?: string;
}
