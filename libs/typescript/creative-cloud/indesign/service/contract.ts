import { Completion, resource, tool } from '@rasm/mcp/link';
import { Image, Instructions, READING } from '@rasm/mcp/server';
import { Unmatched } from '@rasm/search';
import { Schema, Struct } from 'effect';
import { Tool } from 'effect/ai';
import { Rpc, RpcGroup, RpcSerialization } from 'effect/rpc';
import { Render } from '../export.ts';
import { FontView } from '../fonts.ts';
import { Inspect } from '../inspection.ts';
import { Phase } from '../objects/transactions.ts';
import { Change, Create, Remove } from '../objects/values.ts';
import { Preflight } from '../preflight.ts';
import { Script } from '../scripts.ts';
import { FindChange, Query } from '../search.ts';
import { Invoke, MenuAction } from '../session.ts';

// --- [MODELS] --------------------------------------------------------------------------

const PATH = '/indesign';
const WIRE = RpcSerialization.layerSchemaBinary({ fingerprintPayloads: true });
const Peer = Schema.Struct({ application: Schema.URLFromString });
const Failure = Schema.Union([Unmatched, Schema.ErrorInstance()]);

// --- [COMPOSITION] ---------------------------------------------------------------------

const Font = Rpc.make('font', {
    payload: { name: Schema.String.annotate({ description: 'Native font name, PostScript name, full name, or font specifier; a family selects its Regular face' }), text: Schema.String },
    success: Schema.Struct({
        ...FontView.fields,
        installed: Schema.OptionFromOptionalKey(
            FontView.fields.installed.from.schema.pipe(Schema.fieldsAssign({ source: Schema.Struct({ data: Schema.Uint8Array, subset: Schema.Boolean.annotate({ description: 'The original file was unavailable: bytes are a native subset at the default instance holding the requested characters alone' }) }) })),
        ),
    }),
    error: Failure,
});
const Scripted = Rpc.make('execute_script', { payload: Script, success: Script.Result, error: Failure });
const Invocation = Rpc.make('invoke', { payload: Invoke, success: Invoke.Result, error: Failure });
const Enter = Rpc.make('enter', { payload: {}, success: Phase });
const Complete = Rpc.make('complete', { payload: { operation: Schema.String, parameter: Schema.String }, success: Schema.Array(Schema.String), error: Failure });
const InDesign = RpcGroup.make(
    Rpc.make('inspect', { payload: Inspect, success: Inspect.Result, error: Failure }).annotateMerge(READING).pipe(tool),
    Rpc.make('change', { payload: Change, success: Change.Result, error: Failure }).pipe(tool),
    Rpc.make('create', { payload: Create, success: Create.Result, error: Failure }).pipe(tool),
    Rpc.make('remove', { payload: Remove, success: Remove.Result, error: Failure }).pipe(tool),
    Scripted,
    Rpc.make('find_change', { payload: FindChange, success: FindChange.Result, error: Failure }).annotate(Tool.OpenWorld, false).pipe(tool),
    Font,
    Rpc.make('render', { payload: Render, success: Render.Result.mapFields(Struct.assign({ image: Image('image/png') })), error: Failure })
        .annotateMerge(READING)
        .pipe(tool),
    Rpc.make('menu', { payload: { area: Schema.OptionFromOptionalKey(Schema.String) }, success: Schema.Array(MenuAction), error: Failure }).pipe(resource('indesign://menu')),
    Invocation,
    Rpc.make('preflight', { payload: { profile: Schema.OptionFromOptionalKey(Schema.String) }, success: Preflight, error: Failure }).pipe(resource('indesign://preflight')),
    Rpc.make('queries', { payload: { name: Schema.OptionFromOptionalKey(Schema.String) }, success: Schema.Array(Query), error: Failure }).pipe(resource('indesign://queries')),
    Enter,
    Complete,
)
    .annotate(Completion, (rpc, parameter, call) => call(Complete, { operation: rpc._tag, parameter }))
    .annotate(Instructions, [
        'InDesign is served through its panel, which links when InDesign starts. Measurements are numbers in points. Every mutation runs as one undo step named by its verb and target, and its result carries the preflight of each document it touched: issue count and the issues it introduced. Resource templates select an application; base resource URIs use the single linked application.',
    ]);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Complete, Enter, Font, InDesign, Invocation, PATH, Peer, Scripted, WIRE };
