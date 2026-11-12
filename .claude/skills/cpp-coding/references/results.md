# [RESULTS]

## [01]-[PIPELINES]

Dependent steps chain through the members of `std::expected`, and one `value_or` at the composition root names what a failure means there:

```cpp
const auto total = ledger::Cents::make(250)
                       .and_then([](ledger::Cents cents) { return cents.plus(cents); })
                       .transform(&ledger::Cents::value)
                       .value_or(0);
```

`plus` returns `std::expected<Cents, Refusal>` and refuses an overflow. A failed `make` skips `plus` and `transform`.

## [02]-[ACCUMULATION]

Independent checks all run, and their refusals return together:

```cpp
auto parsed(std::string_view text) -> std::expected<Cents, std::vector<Refusal>> {
    std::int64_t value{};
    const auto [end, error] = std::from_chars(text.data(), text.data() + text.size(), value);
    const std::array checks{
        std::pair{error == std::errc::result_out_of_range, Refusal::Overflow},
        std::pair{error == std::errc{} && value < 0, Refusal::Negative},
        std::pair{error == std::errc::invalid_argument || end != text.data() + text.size(), Refusal::Unbalanced},
    };
    auto refusals = checks | std::views::filter(&std::pair<bool, Refusal>::first) | std::views::values | std::ranges::to<std::vector>();
    return refusals.empty() ? Cents::make(value).transform_error([](Refusal refusal) { return std::vector{refusal}; }) : std::unexpected{std::move(refusals)};
}
```

Checks are rows of one table, each a condition beside its refusal. A caller receives every refusal of one input in one outcome.

## [03]-[REFUSAL_POLICIES]

Responses to a refusal a library cannot choose are a policy type the caller names:

```cpp
template <typename Policy>
concept RefusalPolicy = requires(Refusal refusal) {
    { Policy::refused(refusal) } -> std::same_as<void>;
};

struct Halting {
    [[noreturn]] static auto refused(Refusal refusal) -> void {
        std::println(std::cerr, "refused: {}", label(refusal));
        std::terminate();
    }
};

template <RefusalPolicy Policy>
[[nodiscard]] constexpr auto settled(std::expected<Cents, Refusal> outcome) -> Cents {
    return outcome.or_else([](Refusal refusal) -> std::expected<Cents, Refusal> { Policy::refused(refusal); }).value();
}
```

`settled<Halting>(Cents::make(125))` yields the value or ends the process through the policy. `label` is a `switch` over `Refusal` with one `return` per enumerator.
