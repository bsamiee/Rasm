import { assert, it } from '@effect/vitest';
import { Execute, Installation, Launch, Launching, Perform, RunningInstance, Screenshot, State } from '@rasm/applications';
import { Effect, Option, Record, Result, Schema, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PID = 64_327;
const BLOCKING = 20_917;
const AX_ERROR = 3;
const MAIL = 'file:///Applications/Mail.app/';
const REPORT = 'file:///report.ips';
const LINE = `{"running":{"active":false,"process":"sleeping","status":"unresponsive","windows":[{"asking":true,"frame":[[653,277],[494,131]],"kind":"blocking","layer":8,"number":${BLOCKING},"onScreen":false,"title":"Adobe InDesign"},{"element":{"id":"AQID","role":"AXWindow","actions":[],"kind":"window","asking":false},"frame":[[0,0],[1800,1082]],"kind":"window","layer":0,"number":20900,"onScreen":false},{"frame":[[0,669],[500,500]],"kind":"undescribed","layer":0,"number":20901,"onScreen":false,"presented":false}]}}`;
const INSTANCE = `{"pid":${PID},"identifier":"com.adobe.InDesign","url":"file:///Applications/Adobe%20InDesign.app/","activationPolicy":"regular","state":${LINE}}`;
const FAILURE = `{"reason":"noProcess","subject":"${PID}","message":"Process ${PID} exited","error":${AX_ERROR}}`;
const EXITED = `{"exited":{"signal":6,"detail":["memory"],"reporting":true,"report":{"url":"${REPORT}","pid":${PID},"exception":"EXC_CRASH"}}}`;
const LAUNCHING = [`{"launched":{"pid":${PID},"created":true}}`, `{"state":${INSTANCE}}`, `{"prompts":[${INSTANCE}]}`, `{"failed":${FAILURE}}`];
const LISTING = `{"_tag":"Success","success":[{"url":"${MAIL}","identifier":"com.apple.mail","name":"Mail","version":"16.0","build":"3826","scriptable":true,"agent":false}]}`;
const SHOTS = `{"${BLOCKING}":{"_tag":"Success","success":"file:///tmp/${BLOCKING}.png"},"20900":{"_tag":"Failure","failure":{"name":"ApplicationFailure","message":"Window closed"}}}`;
const EXECUTED = '{"reply":null,"artifacts":[{"_tag":"Success","success":{"uri":"file:///tmp/a.pdf","mimeType":"application/pdf","data":"AQID"}}]}';
const ACTIONS = `{"actions":[{"perform":{"element":"AQID","action":"AXPress"}},{"set":{"element":"AQID","value":"A"}},{"key":{"process":${PID},"code":36}}]}`;

// --- [OPERATIONS] ----------------------------------------------------------------------

const wire = <T, E>(schema: Schema.Codec<T, E>) => Schema.fromJsonString(Schema.toCodecJson(schema));
const decoded =
    <T, E>(schema: Schema.Codec<T, E>) =>
    (line: string) =>
        Schema.decodeEffect(wire(schema))(line);
const windowsOf = (state: typeof State.Type) => State.match<readonly (typeof State.Window.Type)[]>(state, { running: Struct.get('windows'), failed: () => [], exited: () => [] });
const rowOf = ({ kind, asking, presented, element }: typeof State.Window.Type) => [kind, asking, presented, element?.actions];
const keptOf = (state: typeof State.Type) => State.match<unknown>(state, { running: ({ status }) => status, failed: ({ error }) => error, exited: ({ report }) => report?.pid });
const buildOf = ({ build }: Installation) => Option.getOrUndefined(build);

// --- [COMPOSITION] ---------------------------------------------------------------------

it.effect('decodes the server wire as written, keeping fields it declares nothing about', () =>
    Effect.gen(function* () {
        const [running, failed, exited] = yield* Effect.all([decoded(State)(LINE), decoded(State)(`{"failed":${FAILURE}}`), decoded(State)(EXITED)]);
        const [launching, shots, listing, executed] = yield* Effect.all([Effect.forEach(LAUNCHING, decoded(Launching)), decoded(Screenshot.Result)(SHOTS), decoded(Schema.Result(Schema.Array(Installation), Schema.ErrorInstance()))(LISTING), decoded(Execute.Result)(EXECUTED)]);
        assert.deepStrictEqual(State.asking(running).map(Struct.get('number')), [BLOCKING]);
        assert.deepStrictEqual(windowsOf(running).map(rowOf), [
            ['blocking', true, true, undefined],
            ['window', false, true, []],
            ['undescribed', false, false, undefined],
        ]);
        assert.deepStrictEqual([keptOf(running), keptOf(failed), keptOf(exited)], ['unresponsive', AX_ERROR, PID]);
        assert.deepStrictEqual(launching.map(Struct.keys), [['launched'], ['state'], ['prompts'], ['failed']]);
        assert.deepStrictEqual(Record.map(shots, Result.isSuccess), { [BLOCKING]: true, 20900: false });
        assert.deepStrictEqual((yield* Effect.fromResult(listing)).map(buildOf), ['3826']);
        assert.lengthOf(executed.artifacts, 1);
        assert.deepStrictEqual(yield* Schema.encodeEffect(Schema.toCodecJson(Launch))(Launch.make({ url: new URL(MAIL) })), { url: MAIL, documents: [], arguments: [], environment: {}, new: false });
        assert.strictEqual(yield* Schema.encodeEffect(wire(Perform))(yield* decoded(Perform)(ACTIONS)), ACTIONS);
        assert.include(yield* Schema.encodeEffect(wire(RunningInstance))(yield* decoded(RunningInstance)(INSTANCE)), '"actions":[]');
    }),
);
