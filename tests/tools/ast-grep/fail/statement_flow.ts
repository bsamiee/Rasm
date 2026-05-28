export const renderStatus = (ready: boolean) => {
    if (ready) {
        return 'ready';
    }

    return 'blocked';
};
