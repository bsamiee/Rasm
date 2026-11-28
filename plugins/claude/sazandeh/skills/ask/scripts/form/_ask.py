"""Asks turned into calls: forms, pages, conditions, and batches of up to four questions."""

from collections import Counter
from functools import partial
from itertools import accumulate, batched, chain, groupby
import math
lazy from collections.abc import Iterable

from expression import Error, Ok

from form._card import Asked, Block, Box, Call, Offer, rendered, row_form, Rows
from form._changes import changed, numbered, Step
from form._show import content, Shown
from form._spec import Ask, Asking, Changes, Consent, Content, Decide, Fault, gathered, Git, History, Loc, NEXT, Outcome, quoted, Reason, Review, Select

# --- [CONSTANTS] ------------------------------------------------------------------------

_QUESTIONS = 4
_OPTIONS = 4
_PAGE = 3


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PAGES]


def _spans(count: int, /) -> tuple[int, ...]:
    heads = math.ceil(max(0, count - _OPTIONS) / _PAGE)
    return (*(_PAGE,) * heads, count - _PAGE * heads)


def _span(start: int, end: int, /) -> str:
    return f"options {start} to {end}" if end > start else f"option {end}"


def _paged(question: str, header: str, offers: tuple[Offer, ...], shared: tuple[Block, ...], at: Loc, /) -> tuple[Step, ...]:
    spans = _spans(len(offers))
    ends = tuple(accumulate(spans))
    bounds = tuple(zip((end - span for span, end in zip(spans, ends, strict=True)), ends, (*ends[1:], ends[-1]), strict=True))
    return tuple(
        (
            Asked(
                question=question if not index else f"{question} ({_span(start + 1, end)} of {len(offers)})",
                header=header,
                offers=(*offers[start:end], *((Offer(name=NEXT, does=f"Shows {_span(end + 1, next_end)} of {len(offers)}"),) if end < len(offers) else ())),
                shared=shared,
                at=at,
            ),
            (index - 1, NEXT) if index else None,
        )
        for index, (start, end, next_end) in enumerate(bounds)
    )


def _checked(question: str, header: str, offers: tuple[Offer, ...], at: Loc, /) -> tuple[Step, ...]:
    count = math.ceil(len(offers) / _OPTIONS)
    sizes = tuple(len(offers) // count + (index < len(offers) % count) for index in range(count))
    starts = tuple(accumulate(sizes, initial=0))
    return tuple(
        (Asked(question=f"{question} ({index} of {count})" if count > 1 else question, header=numbered(header, index, count), offers=offers[start : start + size], multiple=True, at=at), None)
        for index, (start, size) in enumerate(zip(starts, sizes, strict=False), 1)
    )


def _ranked(offers: Iterable[Offer], /) -> tuple[Offer, ...]:
    return tuple(sorted(offers, key=lambda offer: not offer.recommended))


# --- [FORMS]


def _shown(show: Content | None, at: Loc, box: Box, /) -> Outcome[Shown]:
    match show:
        case None:
            return Ok(Shown(blocks=()))
        case _:
            return content(show, at, box)


def _decided(ask: Decide, at: Loc, box: Box, /) -> Outcome[tuple[Step, ...]]:
    shows = (_shown(ask.show, (*at, "show"), box), *(_shown(detail.show, (*at, "options", name, "show"), box) for name, detail in ask.options.items()))
    return gathered(shows).map(
        lambda found: _paged(
            ask.decide,
            ask.header,
            _ranked(
                Offer(name=name, does=detail.does, facts=tuple(detail.facts.items()), show=shown.blocks, recommended=name == ask.recommended)
                for (name, detail), shown in zip(ask.options.items(), found[1:], strict=True)
            ),
            found[0].blocks,
            at,
        )
    )


def _consented(ask: Consent, at: Loc, box: Box, /) -> Outcome[tuple[Step, ...]]:
    facts = (Rows.of(ask.facts.items()),) if ask.facts else ()
    offers = _ranked(Offer(name=name, does=does, recommended=name == ask.recommended) for name, does in (*ask.action.items(), *ask.safe.items()))
    return _shown(ask.show, (*at, "show"), box).map(lambda shown: _paged(ask.consent, ask.header, offers, (*facts, *shown.blocks), at))


def _reviewing(ask: Review, at: Loc, shown: Shown, /) -> tuple[Step, ...]:
    reverting = ask.choices[1][0] if ask.actions is None and isinstance(ask.show, Git) else None
    offers = _ranked(Offer(name=name, does=does, recommended=name == ask.pick, runs=(shown.reverse,) if name == reverting and shown.reverse is not None else ()) for name, does in ask.choices)
    return _paged(ask.review, ask.header, offers, shown.blocks, at)


def _reviewed(ask: Review, at: Loc, box: Box, /) -> Outcome[tuple[Step, ...]]:
    return content(ask.show, (*at, "show"), box).map(partial(_reviewing, ask, at))


# --- [ASKS]


def _steps(ask: Ask, at: Loc, box: Box, /) -> Outcome[tuple[tuple[Step, ...], tuple[str, ...]]]:
    match ask:
        case Decide():
            return _decided(ask, at, box).map(lambda steps: (steps, ()))
        case Select(select=question, header=header, options=options):
            return Ok((_checked(question, header, tuple(Offer(name=name, does=does) for name, does in options.items()), at), ()))
        case Consent():
            return _consented(ask, at, box).map(lambda steps: (steps, ()))
        case Review():
            return _reviewed(ask, at, box).map(lambda steps: (steps, ()))
        case _:
            return changed(ask, at, box)


def _form(ask: Ask, /) -> str:
    return type(ask).key()


type _Placed = tuple[Asked, tuple[int, str] | None]


def _condition(ask: Asking, index: int, headed: frozendict[str, tuple[int, tuple[int, ...]]], steps: tuple[_Placed, ...], failed: frozenset[int], /) -> Outcome[tuple[int, str] | None]:
    if ask.when is None:
        return Ok(None)
    ((header, label),) = ask.when.items()
    at = (index, _form(ask), "when")
    target, places = headed.get(header, (index, ()))
    found = next(((place, offer.label) for place in places if not steps[place][0].multiple for offer in steps[place][0].offers if offer.name == label), None)
    match target >= index, target in failed, found:
        case True, _, _:
            return Error(Fault.of(at, Reason.WHEN_UNKNOWN, header=header))
        case _, True, _:
            return Ok(None)
        case _, _, None if any(steps[place][0].multiple for place in places):
            return Error(Fault.of(at, Reason.WHEN_MULTIPLE, header=header))
        case _, _, None:
            return Error(Fault.of(at, Reason.WHEN_LABEL, header=header, label=label))
        case _:
            return Ok(found)


def _linked(asks: tuple[Ask, ...], outcomes: tuple[Outcome[tuple[Step, ...]], ...], /) -> Outcome[tuple[_Placed, ...]]:
    held = tuple(outcome.default_value(()) for outcome in outcomes)
    starts = tuple(accumulate(map(len, held), initial=0))
    placed = tuple(chain.from_iterable(tuple((asked, None if after is None else (start + after[0], after[1])) for asked, after in steps) for steps, start in zip(held, starts, strict=False)))
    failed = frozenset(index for index, outcome in enumerate(outcomes) if outcome.is_error())
    asking = tuple((index, ask) for index, ask in enumerate(asks) if not isinstance(ask, Changes | History))
    headed = frozendict({ask.header: (index, tuple(range(starts[index], starts[index + 1]))) for index, ask in reversed(asking)})
    headers = Counter(ask.header for _, ask in asking)
    repeated = (Error(Fault.of((index, _form(ask), "header"), Reason.HEADER_REPEATED, header=ask.header)) for index, ask in asking if headers[ask.header] > 1)
    conditions = gathered(_condition(ask, index, headed, placed, failed) for index, ask in asking)
    owners = tuple(chain.from_iterable((index,) * len(steps) for index, steps in enumerate(held)))
    attached = conditions.map(lambda found: frozendict(zip((index for index, _ in asking), found, strict=True))).map(
        lambda gate: tuple((asked, after or gate.get(owner)) for (asked, after), owner in zip(placed, owners, strict=True))
    )
    return gathered((*(outcome.map(lambda _: ()) for outcome in outcomes), *repeated, attached)).map(lambda parts: parts[-1])


def _batched(steps: tuple[_Placed, ...], /) -> tuple[tuple[tuple[int, str] | None, tuple[int, ...]], ...]:
    order = frozendict((condition, rank) for rank, condition in enumerate(dict.fromkeys((None, *(after for _, after in steps)))))
    ranked = sorted(range(len(steps)), key=lambda index: order[steps[index][1]])
    return tuple((condition, chunk) for condition, group in groupby(ranked, key=lambda index: steps[index][1]) for chunk in batched(group, _QUESTIONS))


def _called(steps: tuple[_Placed, ...], box: Box, batch: tuple[tuple[int, str] | None, tuple[int, ...]], /) -> Outcome[tuple[tuple[int, ...], Call, tuple[str, ...]]]:
    _, places = batch
    asked = tuple(steps[place][0] for place in places)
    form, why = row_form(
        row for question in asked for offer in question.offers for block in (*offer.blocks, *question.shared) if isinstance(block, Rows) for _, section in block.sections for row in section
    )
    texts, headers = Counter(question.question for question in asked), Counter(question.header for question in asked)
    repeated = (
        *(Error(Fault.of(question.at, Reason.QUESTION_REPEATED, question=question.question)) for question in asked if texts[question.question] > 1),
        *(Error(Fault.of(question.at, Reason.HEADER_REPEATED, header=question.header)) for question in asked if headers[question.header] > 1),
    )
    questions = gathered(rendered(question, form, box, multi=len(asked) > 1) for question in asked)
    return gathered((questions, *repeated)).map(lambda parts: (places, Call(questions=tuple(wire for wire, _ in parts[0])), (*why, *chain.from_iterable(rows for _, rows in parts[0]))))


def _reported(steps: tuple[_Placed, ...], calls: tuple[tuple[tuple[int, ...], Call, tuple[str, ...]], ...], /) -> tuple[tuple[Call, tuple[str, ...]], ...]:
    wired = frozendict(chain.from_iterable(zip(places, call.questions, strict=True) for places, call, _ in calls))
    return tuple(
        (call, (*((f"call {number}: after {quoted(wired[after[0]].question.partition('\n')[0])} answers {quoted(after[1])}",) if (after := steps[places[0]][1]) is not None else ()), *rows))
        for number, (places, call, rows) in enumerate(calls, 1)
    )


def built(asks: tuple[Ask, ...], box: Box, /) -> Outcome[tuple[tuple[tuple[Call, tuple[str, ...]], ...], tuple[str, ...]]]:
    outcomes = tuple(_steps(ask, (index, _form(ask)), box) for index, ask in enumerate(asks))
    notes = tuple(chain.from_iterable(outcome.map(lambda parts: parts[1]).default_value(()) for outcome in outcomes))
    planned = _linked(asks, tuple(outcome.map(lambda parts: parts[0]) for outcome in outcomes))
    return planned.bind(lambda steps: gathered(map(partial(_called, steps, box), _batched(steps))).map(lambda calls: (_reported(steps, calls), notes)))


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["built"]
