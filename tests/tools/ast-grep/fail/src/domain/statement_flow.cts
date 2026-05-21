const renderCommonStatus = (value: unknown) => {
    try {
        return String(value);
    } catch {
        throw new Error('unreachable');
    }
};

module.exports = { renderCommonStatus };
