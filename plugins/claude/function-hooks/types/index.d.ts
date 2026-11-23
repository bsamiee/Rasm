// --- [MODELS] --------------------------------------------------------------------------

declare module 'claude-code' {
    interface BuiltinToolInputs {
        Bash: { command: string; timeout?: number; description?: string; run_in_background?: boolean };
        Monitor: { description: string; timeout_ms: number; command?: string };
        Read: { file_path: string };
    }
    interface PluginState {
        'function-hooks': { tasks: Shaped<Record<string, Task>> };
    }
}

export interface Task {
    readonly file: string;
    readonly agent?: string;
    readonly told?: string;
}
