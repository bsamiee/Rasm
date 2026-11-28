const drawn = new Map();
const dispatch = { count: 0 };
const CHUNK = 9000;
const HELD = 60_000;

function plain(key, value) {
    return key === 'press' || key === 'part' ? undefined : value;
}

function same(left, right) {
    return JSON.stringify(left) === JSON.stringify(right);
}

/**
 * @param {unknown} before
 * @param {unknown} after
 */
function changed(before, after) {
    const [held, made] = [before, after].map((value) => new Map(Object.entries(typeof value === 'object' && value !== null ? value : {})));
    const keys = new Set([...held.keys(), ...made.keys()]);
    return Object.fromEntries([...keys].filter((key) => !same(held.get(key), made.get(key))).map((key) => [key, made.get(key) ?? null]));
}

async function observed($, e, next) {
    dispatch.count += 1;
    const order = dispatch.count;
    const answer = await next(next.event === 'ui.toast' ? { ...e, timeoutMs: HELD } : e);
    const engine = next.trace.find((link) => link.plugin === 'engine');
    const links = next.trace.filter((link) => link.plugin !== 'engine').map(({ plugin, outcome, reason }) => ({ plugin, outcome, reason }));
    const isRender = next.event === 'ui.render';
    const rewrote = isRender && engine !== undefined ? changed(e.props, engine.received.props) : {};
    const isAnswered = engine === undefined || !same(engine.returned, answer);
    const isFailed = links.some((link) => link.outcome !== 'returned' && link.outcome !== 'passed');
    const isChanged = [isAnswered, isFailed, Object.keys(rewrote).length > 0].includes(true);
    if (next.origin.plugin === 'engine' && !isChanged && (isRender || e.plugin === undefined)) {
        return answer;
    }
    const site = isRender ? { component: e.component, plugin: e.plugin, requestId: e.requestId, viewport: e.viewport, props: e.props } : e;
    const body = JSON.stringify({ event: next.event, origin: next.origin.plugin, args: site, answer: isAnswered ? answer : undefined, rewrote, links }, plain);
    if (isRender) {
        const key = `${e.component}\u0000${e.plugin ?? ''}\u0000${e.requestId}`;
        if (drawn.get(key) === body) {
            return answer;
        }
        drawn.set(key, body);
    }
    const text = `{"order":${order},${body.slice(1)}`;
    for (let at = 0; at < text.length; at += CHUNK) {
        $.ui.log(`PREVIEW ${order} ${at + CHUNK < text.length ? 1 : 0} ${text.slice(at, at + CHUNK)}`, { to: 'debug' });
    }
    return answer;
}

function passed(_$, e, next) {
    return next(e);
}

/** @type {import('claude-code').Register} */
export function register(on) {
    on('ui.render', { plugin: '*' }, observed).catch(passed);
    on('ui.press', { plugin: '*' }, observed).catch(passed);
    on('ui.input', { plugin: '*' }, observed).catch(passed);
    on('ui.select', { plugin: '*' }, observed).catch(passed);
    on('ui.toast', observed).catch(passed);
    on('ui.status', observed).catch(passed);
    on('ui.notice', observed).catch(passed);
    on('ui.notify', observed).catch(passed);
    on('ui.open', observed).catch(passed);
    on('ui.close', observed).catch(passed);
    on('ui.place', observed).catch(passed);
    on('theme.set', observed).catch(passed);
    on('audio.play', observed).catch(passed);
    on('audio.speak', observed).catch(passed);
    on('session.append', observed).catch(passed);
    on('prompt.context', observed).catch(passed);
}
