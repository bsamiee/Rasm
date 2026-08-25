#!/usr/bin/osascript -l JavaScript
// Title    : osa-tool
// Contract : osascript -l JavaScript osa-tool.js <bundle-id> <payload-json>
//            stdout carries one JSON.stringify object; console.log rides stderr.
'use strict';

const POLICY = { tool: '<TOOL_NAME>', idleSeconds: 300 };

// osascript and a double-clicked applet both enter here; the applet arrives with an empty argv.
function run(argv) {
    const [bundleID, payloadJSON] = argv;
    if (!bundleID) {
        throw new Error('missing bundle identifier');
    }
    const payload = payloadJSON ? JSON.parse(payloadJSON) : {};
    return JSON.stringify({ tool: POLICY.tool, bundleID, payload });
}

// Droplet ingress: a Finder drop hands Path tokens, never strings, so each item converts explicitly.
function openDocuments(dropped) {
    return JSON.stringify(dropped.map((item) => item.toString()));
}

// Stay-open applet only (osacompile -s); the return value is the seconds until the next call.
function idle() {
    return POLICY.idleSeconds;
}

// Stay-open teardown: release retained state here before the host exits.
function quit() {
    return true;
}
