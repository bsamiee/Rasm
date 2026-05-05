export const StatusLabel = ({ ready }: { readonly ready: boolean }) => {
    if (ready) {
        return <span>Ready</span>;
    }

    return <span>Blocked</span>;
};
