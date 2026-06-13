#!/usr/bin/env python3
"""One protocol-conformant Execute over the live bridge pipe with HostPlugins=[RhinoCodePlugin].

Rhino 9.0.26160 no longer pre-registers RhinoCode languages at startup and the frozen plugin's
Registrar.StartScriptingLanguages call gates vacuously on an empty registry. Loading McNeel's
RhinoCodePlugin (c9cba87a-23ce-4f15-a918-97645c05cde7) through the bridge's own HostPlugins
pre-load mechanism constructs ScriptEditorCommand, whose ctor runs Registrar.StartScripting(true)
and registers mcneel.roslyn.csharp. Run under the assay bridge flock lease.
"""

import json
import os
import socket
import struct
import sys

RHINOCODE_PLUGIN_ID = "c9cba87a-23ce-4f15-a918-97645c05cde7"

endpoint = json.load(open(os.path.expanduser("~/.rasm/rhino-bridge.json"), encoding="utf-8-sig"))
pipe_path = os.path.join(os.environ["TMPDIR"], f"CoreFxPipe_{endpoint['pipeName']}")
print(f"pipe={pipe_path}", file=sys.stderr)

request = {
    "command": "execute",
    "timeoutMs": 120000,
    "payload": {
        "script": 'System.Console.WriteLine("language-bootstrap-ok");',
        "scriptPath": None,
        "references": [],
        "hostPlugins": [RHINOCODE_PLUGIN_ID],
    },
}

with socket.socket(socket.AF_UNIX, socket.SOCK_STREAM) as sock:
    sock.settimeout(120.0)
    sock.connect(pipe_path)
    body = json.dumps(request).encode()
    sock.sendall(struct.pack("<i", len(body)) + body)
    prefix = b""
    while len(prefix) < 4:
        prefix += sock.recv(4 - len(prefix))
    (length,) = struct.unpack("<i", prefix)
    payload = b""
    while len(payload) < length:
        chunk = sock.recv(min(65536, length - len(payload)))
        if not chunk:
            break
        payload += chunk

reply = json.loads(payload)
print(json.dumps(reply, indent=1)[:4000])
status = reply.get("status", "")
sys.exit(0 if status == "ok" else 1)
