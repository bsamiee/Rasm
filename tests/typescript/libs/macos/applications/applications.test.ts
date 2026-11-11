import { assert, it } from '@effect/vitest';
import { Execute, Installation, Launch, Launching, Perform, RunningInstance, Screenshot, State } from '@rasm/applications';
import { Array, Effect, Option, Record, Result, Schema, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PID = 64_327;
const PROCESS = `${PID}@8589769593254`;
const BLOCKING = 20_917;
const AX_ERROR = 3;
const MAIL = 'file:///Applications/Mail.app/';
const LINE = `{"running":{"active":false,"process":"sleeping","status":"unresponsive","windows":[{"asking":true,"frame":[[653,277],[494,131]],"kind":"blocking","layer":8,"number":${BLOCKING},"onScreen":false,"title":"Adobe InDesign"},{"element":{"id":"AQID","role":"AXWindow","actions":[],"kind":"window","asking":false},"frame":[[0,0],[1800,1082]],"kind":"window","layer":0,"number":20900,"onScreen":false},{"frame":[[0,669],[500,500]],"kind":"undescribed","layer":0,"number":20901,"onScreen":false,"presented":false}]}}`;
const INSTANCE = `{"pid":${PID},"instance":"${PROCESS}","identifier":"com.adobe.InDesign","url":"file:///Applications/Adobe%20InDesign.app/","activationPolicy":"regular","state":${LINE}}`;
const FAILURE = `{"reason":"refused","subject":"AXPress on AQID","message":"AXPress on AQID refused with AXError ${AX_ERROR}","error":${AX_ERROR}}`;
const EXITED = `{"exited":{"signal":6,"detail":["memory"],"report":{"url":"file:///report.ips","pid":${PID},"instance":"${PROCESS}","service":"app.rasm.atelier.server","exception":"EXC_CRASH","signal":"SIGKILL (Code Signature Invalid)","termination":{"code":4,"indicator":"Launch Constraint Violation","namespace":"CODESIGNING"}}}}`;
const LAUNCHING = [`{"launched":{"pid":${PID},"instance":"${PROCESS}","identifier":"com.adobe.InDesign","created":false,"replaced":true}}`, `{"state":${INSTANCE}}`, `{"prompts":[${INSTANCE}]}`, `{"failed":${FAILURE}}`];
const LISTING = `{"_tag":"Success","success":[{"url":"${MAIL}","identifier":"com.apple.mail","name":"Mail","version":"16.0","build":"3826","scriptable":true,"activationPolicy":"regular"}]}`;
const SHOTS = `{"${BLOCKING}":{"_tag":"Success","success":"file:///tmp/${BLOCKING}.png"},"20900":{"_tag":"Failure","failure":{"name":"ApplicationFailure","message":"Window closed"}}}`;
const ACTIONS = `{"actions":[{"perform":{"element":"AQID","action":"AXPress"}},{"set":{"element":"AQID","value":"A"}},{"key":{"process":${PID},"code":36}}]}`;

// --- [MODELS] --------------------------------------------------------------------------

const Listing = Schema.Result(Schema.Array(Installation), Schema.ErrorInstance());

// --- [OPERATIONS] ----------------------------------------------------------------------

const wire = <T, E>(schema: Schema.Codec<T, E>) => Schema.fromJsonString(Schema.toCodecJson(schema));
const decoded =
    <T, E>(schema: Schema.Codec<T, E>) =>
    (line: string) =>
        Schema.decodeEffect(wire(schema))(line);
const windowsOf = State.matchOrElse({ running: ({ running }) => running.windows }, Array.empty<typeof State.Window.Type>);
const rowOf = ({ kind, asking, presented, element }: typeof State.Window.Type) => [kind, asking, presented, Option.flatMap(Option.fromUndefinedOr(element), Record.get('actions'))];
const keptOf = State.match({ running: ({ running }) => running.status, failed: ({ failed: { error } }) => error, exited: ({ exited }) => Option.map(Option.fromUndefinedOr(exited.report), ({ instance, termination }) => [instance, termination?.indicator]) });
const buildOf = ({ build }: Installation) => Option.getOrUndefined(build);

// --- [COMPOSITION] ---------------------------------------------------------------------

it.effect(
    'decodes the server wire as written, keeping fields it declares nothing about',
    Effect.fnUntraced(function* () {
        const [running, failed, exited] = yield* Effect.all([decoded(State)(LINE), decoded(State)(`{"failed":${FAILURE}}`), decoded(State)(EXITED)]);
        const [launching, shots, listing, executed] = yield* Effect.all([
            Effect.forEach(LAUNCHING, decoded(Launching)),
            decoded(Screenshot.Result)(SHOTS),
            decoded(Listing)(LISTING),
            decoded(Execute.Result)('{"reply":null,"artifacts":[{"_tag":"Success","success":{"uri":"file:///tmp/a.pdf","mimeType":"application/pdf","data":"AQID"}}]}'),
        ]);
        assert.deepStrictEqual(State.asking(running).map(Struct.get('number')), [BLOCKING]);
        assert.deepStrictEqual(windowsOf(running).map(rowOf), [
            ['blocking', true, true, Option.none()],
            ['window', false, true, Option.some([])],
            ['undescribed', false, false, Option.none()],
        ]);
        assert.deepStrictEqual([keptOf(running), keptOf(failed), keptOf(exited)], ['unresponsive', AX_ERROR, Option.some([PROCESS, 'Launch Constraint Violation'])]);
        assert.deepStrictEqual(launching.map(Struct.get('_tag')), ['launched', 'state', 'prompts', 'failed']);
        assert.deepStrictEqual(Record.map(shots, Result.isSuccess), { [BLOCKING]: true, 20900: false });
        assert.deepStrictEqual((yield* Effect.fromResult(listing)).map(buildOf), ['3826']);
        assert.lengthOf(executed.artifacts, 1);
        const encodeLaunch = Schema.encodeEffect(Schema.toCodecJson(Launch));
        assert.deepStrictEqual(yield* encodeLaunch(Launch.make({ url: new URL(MAIL) })), { url: MAIL, documents: [], arguments: [], environment: {}, new: false });
        assert.strictEqual(yield* Schema.encodeEffect(wire(Perform))(yield* decoded(Perform)(ACTIONS)), ACTIONS);
        assert.include(yield* Schema.encodeEffect(wire(RunningInstance))(yield* decoded(RunningInstance)(INSTANCE)), '"actions":[]');
    }),
);
