"""TCP rule of the `hosting` plugin a suite loads."""

import pytest

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_tcp_opens_for_a_test_requesting_the_event_loop_and_stays_refused_for_others(pytester: pytest.Pytester) -> None:
    """Under `--disable-socket` with warnings as errors, a test requesting `portal` creates a socket and a test requesting no fixture of the plugin is refused one."""
    pytester.makepyfile("""
        import socket

        import pytest


        def test_reaching(portal):
            socket.socket().close()


        def test_refused():
            with pytest.raises(UserWarning, match="tried to use socket"):
                socket.socket()
    """)
    pytester.runpytest_subprocess("-p", "rasm.testing.hosting", "--disable-socket", "-W", "error").assert_outcomes(passed=2)
