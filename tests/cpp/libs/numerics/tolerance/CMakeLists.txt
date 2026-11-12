cmake_minimum_required(VERSION 4.5)

project(rasm-tolerance-tests LANGUAGES CXX)

# --- [DEPENDENCIES] ---------------------------------------------------------------------

find_package(Catch2 3 CONFIG REQUIRED)
include(Catch)

# --- [TARGETS] --------------------------------------------------------------------------

add_executable(rasm-tolerance-tests tolerance.cpp)
target_compile_features(rasm-tolerance-tests PRIVATE cxx_std_26)
target_link_libraries(rasm-tolerance-tests PRIVATE rasm-tolerance::tolerance Catch2::Catch2WithMain)
catch_discover_tests(rasm-tolerance-tests ADD_TAGS_AS_LABELS)
