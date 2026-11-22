// --- [MODELS] --------------------------------------------------------------------------

declare module 'claude-code' {
    interface PluginState {
        'function-hooks': { tasks: Shaped<Record<string, Task>> };
    }
}

export interface Task {
    readonly file: string;
    readonly agent?: string;
    readonly told?: string;
}
