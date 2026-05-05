export const renderModuleStatus = (ready: boolean) => {
    let status = 'blocked';

    if (ready) {
        status = 'ready';
    }

    return status;
};
