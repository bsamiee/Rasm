"""Flatten the Illustrator Prefs tree into `section/key = value` rows."""

import re
import sys

src = open(sys.argv[1], encoding="utf-8", errors="replace").read().split("\n")

rows = []
stack = []
i = 0
while i < len(src):
    line = src[i].strip()
    i += 1
    if not line:
        continue
    if line == "}":
        if stack:
            stack.pop()
        continue
    m = re.match(r"^/(\S+)\s*(.*)$", line)
    if not m:
        continue
    key, rest = m.group(1), m.group(2).strip()
    path = "/".join(stack + [key])
    if rest == "{":
        stack.append(key)
        continue
    if rest.startswith("["):
        # hex blob: [ <len> <hex lines...> ]
        hexlen = rest[1:].strip()
        blob = []
        while i < len(src):
            l2 = src[i].strip()
            i += 1
            if l2 == "]" or l2.endswith("]"):
                blob.append(l2.rstrip("]").strip())
                break
            blob.append(l2)
        hx = "".join(blob)
        try:
            txt = bytes.fromhex(hx).decode("utf-8", errors="replace")
        except ValueError:
            txt = "<hex:%s>" % hx[:64]
        rows.append((path, "[len=%s] %s" % (hexlen, txt)))
        continue
    rows.append((path, rest))

for p, v in rows:
    print("%s = %s" % (p, v))
