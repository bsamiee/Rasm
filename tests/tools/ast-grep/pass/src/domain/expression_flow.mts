const _status = {
    blocked: 'blocked',
    ready: 'ready',
} as const;

export const renderModuleStatus = (state: keyof typeof _status) => _status[state];
