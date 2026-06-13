#!/usr/bin/env python3
"""Run a command while holding one of assay's POSIX flock leases (non-blocking; busy -> exit 5)."""

import fcntl
import os
import subprocess
import sys

lock_path, *cmd = sys.argv[1:]
fd = os.open(lock_path, os.O_RDWR | os.O_CREAT, 0o644)
try:
    fcntl.flock(fd, fcntl.LOCK_EX | fcntl.LOCK_NB)
except BlockingIOError:
    print(f"lease busy: {lock_path}", file=sys.stderr)
    sys.exit(5)
try:
    rc = subprocess.run(cmd, check=False).returncode
finally:
    os.ftruncate(fd, 0)
    fcntl.flock(fd, fcntl.LOCK_UN)
    os.close(fd)
sys.exit(rc)
