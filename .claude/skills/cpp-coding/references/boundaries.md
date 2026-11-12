# [BOUNDARIES]

## [01]-[FORMATTING]

Types print through a `std::formatter` specialization, declared in the global namespace beside the type's module:

```cpp
template <>
struct std::formatter<ledger::Cents> : std::formatter<std::string_view> {
    auto format(ledger::Cents cents, std::format_context& context) const {
        return std::format_to(context.out(), "{}.{:02}", cents.value() / 100, cents.value() % 100);
    }
};
```

`std::println("{}", cents)` and `std::format` read the specialization. Diagnostics go to `std::cerr` through `std::println(std::cerr, ...)`.

## [02]-[ENTRY_POINT]

Entry points decode their arguments once into values and return the exit status of the outcome:

```cpp
auto main(int argc, char** argv) -> int {
    const auto arguments = std::span(argv, static_cast<std::size_t>(argc)).subspan(1);
    return ledger::parsed(arguments.empty() ? "" : arguments.front()).has_value() ? 0 : 2;
}
```

Libraries declare no `main` and read no argument, environment variable, or working directory.

## [03]-[TESTS]

Catch2 headers come before every `import`, a test case names one behavior, and a result a constant expression decides is checked at compile time:

```cpp
#include <catch2/catch_test_macros.hpp>

import std;
import ledger;

TEST_CASE("cents refuse a negative amount", "[ledger][cents]") {
    STATIC_REQUIRE(ledger::Cents::make(-1).error() == ledger::Refusal::Negative);

    SECTION("a sum past the largest amount is an overflow") {
        const auto largest = ledger::Cents::make(std::numeric_limits<std::int64_t>::max());
        REQUIRE(largest.and_then([](ledger::Cents cents) { return cents.plus(cents); }).error() == ledger::Refusal::Overflow);
    }
}
```

Test executables link `Catch2::Catch2WithMain` and register through `catch_discover_tests(<target> ADD_TAGS_AS_LABELS)`, each tag becoming a CTest label. Tests sit at `libs/cpp/<category>/tests/<package>/` as their own CMake project, suites spanning packages at `tests/cpp/<kind>/<suite>/`.
