from typing import assert_never


type State = str


def render(state: State) -> str:
    match state:
        case "ready":
            return "ready"
        case "blocked":
            return "blocked"
        case _ as unreachable:
            assert_never(unreachable)
