# [PY_BRANCH_API_PYTHON_DATEUTIL]

`python-dateutil` reads a timestamp string wider than any RFC-3339 grammar admits, and `dateutil.parser.isoparse` is the member every CloudEvents binding decodes its `time` attribute through. It returns a `dateutil.tz` zone object rather than the stdlib `timezone.utc` singleton and raises a bare `ValueError` on a string it cannot read, so conformance and zone identity both prove at the admitting consumer.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `dateutil.parser` readers and their fault

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :----------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `isoparser`              | class         | ISO-8601 reader bound to one date-time separator                   |
|  [02]   | `parser`                 | class         | heuristic reader over ambiguous and natural-language strings       |
|  [03]   | `parserinfo`             | class         | token roster and two-digit-year pivot `parser` reads               |
|  [04]   | `ParserError`            | exception     | `parser` refusal deriving `ValueError`; `isoparse` never raises it |
|  [05]   | `UnknownTimezoneWarning` | warning       | `parser` dropped a zone abbreviation, deriving `RuntimeWarning`    |

[PUBLIC_TYPE_SCOPE]: `dateutil.tz` zones the ISO reader returns

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :--------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `tzutc`    | tzinfo        | zero-offset zone every `Z`, `z`, `+00`, and `+00:00` suffix resolves to |
|  [02]   | `tzoffset` | tzinfo        | fixed-offset zone a signed numeric suffix resolves to, name `None`      |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: ISO-8601 admission

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :----------------------------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `isoparse(dt_str)`                               | static   | read one ISO-8601 string into a `datetime`           |
|  [02]   | `isoparser(sep=None)`                            | ctor     | bind a reader to one date-time separator character   |
|  [03]   | `isoparser.isoparse(dt_str)`                     | instance | read a `datetime` under this reader's separator      |
|  [04]   | `isoparser.parse_isodate(datestr)`               | instance | read the date half alone into a `date`               |
|  [05]   | `isoparser.parse_isotime(timestr)`               | instance | read the time half alone into a zone-carrying `time` |
|  [06]   | `isoparser.parse_tzstr(tzstr, zero_as_utc=True)` | instance | read an offset suffix into a `tzutc` or `tzoffset`   |

- `isoparse`: raises bare `ValueError` on every malformed string, never `ParserError`, and reports an empty string as `ISO string too short`.
- `isoparser(sep=None)`: admits ANY single character between the date and time halves, so `2018-01-01 00:00:00Z` reads exactly as the `T` form does.
- `parse_tzstr`: `zero_as_utc=True` folds a zero offset to `tzutc`, so `+00:00` and `Z` return one zone object.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `isoparse` admits the ISO-8601 superset, so a truncated `2018`, a `2018-W01-1` week date, a `2018-001` ordinal date, and a `20180101T000000Z` basic-format string all read where RFC-3339 refuses; wire conformance proves at the consumer, never at the reader.
- `isoparse` yields a NAIVE `datetime` for every string carrying no offset, so RFC-3339's mandatory offset proves on a `tzinfo is None` read at admission.
- Zone objects are `tzutc` and `tzoffset`, comparing unequal to `datetime.timezone.utc`, so offset equality proves through `utcoffset()` and identity comparison against the stdlib singleton always fails.
- Fractional seconds past six digits truncate toward zero with no refusal, `24:00:00` rolls into the next midnight, and `23:59:60` raises — so the leap second RFC-3339 permits is exactly the value this reader rejects.
- `ParserError` and `UnknownTimezoneWarning` belong to `parser`, so a catch set naming `ParserError` alone lets every malformed ISO string escape the boundary.

[STACKING]:
- `cloudevents`(`.api/cloudevents.md`): the one `time` reader across every binding — `core.bindings.common.decode_header_value`, `core.formats.json.JSONFormat.read`, and the `amqp`, `http`, `kafka`, and `rabbitmq` header decoders each hand the raw attribute to `isoparse` and catch nothing, so its `ValueError` crosses the binding whole and reaches the branch seam untyped.
- `transport/event`(`runtime/.planning/transport/event.md`): `_FORMAT_RAISES` carries `ValueError`, so `boundary` folds that escaping raise onto the `EVENT_DECODE` rail; branch-minted extension timestamps read through `datetime.fromisoformat` on the same page, keeping the stdlib `timezone.utc` singleton on every value the branch itself mints.

[LOCAL_ADMISSION]:
- `isoparse` serves the vendor bindings' `time` decode alone; every branch-minted timestamp reads through `datetime.fromisoformat`, which refuses the truncated forms and returns the `timezone.utc` singleton.
- Normalize an `isoparse` result to `datetime.timezone.utc` at the admitting boundary, so one zone representation reaches the interior and no comparison straddles two `tzinfo` families.
- `parser`, `parserinfo`, and `UnknownTimezoneWarning` stay out — a heuristic reader guessing day-month order over a wire value fabricates a timestamp no producer sent.
- `relativedelta`, `rrule`, `easter`, `utils`, and `zoneinfo` stay out — `apscheduler` owns cron and interval recurrence, and the stdlib `zoneinfo` module owns the tz database.
