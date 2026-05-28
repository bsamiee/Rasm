const _label = {
    blocked: 'Blocked',
    ready: 'Ready',
} as const;

export const StatusLabel = ({ state }: { readonly state: keyof typeof _label }) => <span>{_label[state]}</span>;
