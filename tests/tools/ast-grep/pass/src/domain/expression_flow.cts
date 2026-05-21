const _commonStatus = {
    blocked: 'blocked',
    ready: 'ready',
} as const;

const renderCommonStatus = (state: keyof typeof _commonStatus) => _commonStatus[state];

module.exports = { renderCommonStatus };
