"""Gives a session LaunchServices started what it withholds from a launched app: output streams in its profile's `blender.log` and the launching folder as working folder."""

import os
from pathlib import Path

# --- [COMPOSITION] ----------------------------------------------------------------------


def main() -> None:
    """Sends both output streams to the log `run` prints and enters the folder `run` names."""
    with (Path(os.environ["BLENDER_USER_RESOURCES"]) / "blender.log").open("ab", buffering=0) as sink:
        os.dup2(sink.fileno(), 1)
        os.dup2(1, 2)
    os.chdir(os.environ["PWD"])


if __name__ == "__main__":
    main()
