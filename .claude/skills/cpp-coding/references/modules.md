# [MODULES]

## [01]-[PRIMARY_INTERFACE]

Primary interface units re-export their partitions and declare nothing of their own:

```cpp
export module ledger;

export import :amount;
export import :posting;
```

`ledger:amount` and `ledger:posting` are interface partitions, each `export module ledger:<part>;`. Consumers write `import ledger;` and never name a partition.

## [02]-[IMPLEMENTATION_UNITS]

Implementation units define what an interface declares and import the internal partitions they read:

```cpp
module ledger;

import std;
import :clock;

namespace ledger {

auto balance(std::span<const Posting> postings) -> std::expected<Cents, Refusal> {
    const auto total = std::ranges::fold_left(postings | std::views::transform(signed_value), std::int64_t{0}, std::plus{});
    return Cents::make(total).transform_error([](Refusal) { return Refusal::Unbalanced; });
}

}
```

`module ledger;` imports the primary interface implicitly. Every implementation unit declares `module ledger;`, a `module ledger:part;` declaration without `export` names an internal partition and belongs in `FILE_SET CXX_MODULES`.

## [03]-[GLOBAL_MODULE_FRAGMENT]

Headers that supply macros or C declarations enter through the global module fragment of the one partition calling them:

```cpp
module;

#include <ctime>

module ledger:clock;

import std;

namespace ledger {

[[nodiscard]] auto stamped(std::time_t at) -> std::string {
    std::tm parts{};
    ::gmtime_r(&at, &parts);
    return std::format("{:04}-{:02}-{:02}", parts.tm_year + 1900, parts.tm_mon + 1, parts.tm_mday);
}

}
```

`gmtime_r` is a POSIX declaration `import std;` does not provide. Fragments hold preprocessor directives alone, and a fragment with no directive goes.

## [04]-[HEADER_FACE]

Libraries a consumer outside the toolchain reads (a host building at its own standard, a C++ compiler without module scanning) declare those functions in `FILE_SET HEADERS`, define them in an ordinary source, and re-export them from the module:

```cpp
module;

#include <ledger/balance.hpp>

export module ledger:balance;

export namespace ledger {

using ledger::closing;

}
```

`ledger/balance.hpp` declares `closing` in namespace `ledger` under an include guard, and a source including that header defines it. Functions a header declares attach to the global module, so one definition serves `import ledger;` and `#include <ledger/balance.hpp>`. Headers of the face compile at the consumer's standard, name no module, and pass arithmetic types, `std::string`, `std::string_view`, and `std::span` alone across the boundary.
