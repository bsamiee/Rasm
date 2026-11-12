# [RANGES]

## [01]-[PIPELINES]

Sequences transform through views and collect once with `std::ranges::to`:

```cpp
auto largest(std::span<const Posting> postings, std::size_t count) -> std::vector<std::int64_t> {
    auto values = postings | std::views::transform(signed_value) | std::ranges::to<std::vector>();
    std::ranges::partial_sort(values, values.begin() + static_cast<std::ptrdiff_t>(std::min(count, values.size())), std::ranges::greater{});
    return values | std::views::take(count) | std::ranges::to<std::vector>();
}
```

Views run no work until a consumer iterates them, and `partial_sort` orders the leading `count` elements alone.

## [02]-[FOLDS]

Reductions name their fold, `std::ranges::fold_left` taking the seed and the operation:

```cpp
const auto total = std::ranges::fold_left(postings | std::views::transform(signed_value), std::int64_t{0}, std::plus{});
```

`std::ranges::max`, `min`, `count_if`, `any_of`, `all_of`, and `contains` state the reductions they name, a fold restating one of them goes.

## [03]-[PROJECTIONS]

Algorithms take a projection in place of a comparator lambda that reads one member:

```cpp
std::ranges::sort(sessions, std::ranges::greater{}, &Session::started);
const auto shortest = std::ranges::min_element(paths, {}, &std::string::size);
```

`Session` is a record with a `started` field, and `paths` a range of `std::string`.

## [04]-[INDEXED_AND_PAIRED]

Positions come from `std::views::enumerate` and parallel sequences from `std::views::zip`, an index variable beside a range restating them:

```cpp
for (const auto [index, argument] : std::views::enumerate(arguments)) {
    std::println("{}: {}", index, argument);
}
```
