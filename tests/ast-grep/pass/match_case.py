from typing import Literal, assert_never


type State = Literal["ready", "blocked"]


def render(state: State) -> str:
    match state:
        case "ready":
            return "ready"
        case "blocked":
            return "blocked"
        case _ as unreachable:
            assert_never(unreachable)
