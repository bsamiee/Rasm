# --- [OPERATIONS] -----------------------------------------------------------------------

file(GLOB profiles "${PROFILES}/*.profraw")
if(NOT profiles)
    return()
endif()
execute_process(
    COMMAND llvm-profdata merge --sparse "--output=${MERGED}" ${profiles}
    COMMAND_ERROR_IS_FATAL ANY
)
execute_process(COMMAND "${CMAKE_CTEST_COMMAND}" --test-dir "${TESTS}" --show-only=json-v1 OUTPUT_VARIABLE listing COMMAND_ERROR_IS_FATAL ANY)
cmake_path(ABSOLUTE_PATH TESTS NORMALIZE OUTPUT_VARIABLE folder)
string(JSON count LENGTH "${listing}" tests)
math(EXPR last "${count} - 1")
foreach(at RANGE ${last})
    string(JSON command ERROR_VARIABLE absent GET "${listing}" tests ${at} command 0)
    cmake_path(IS_PREFIX folder "${command}" NORMALIZE built)
    if(built)
        list(APPEND objects "--object=${command}")
    endif()
endforeach()
list(REMOVE_DUPLICATES objects)
execute_process(
    COMMAND llvm-cov export --format=lcov "--instr-profile=${MERGED}" ${objects}
    OUTPUT_FILE "${REPORT}"
    COMMAND_ERROR_IS_FATAL ANY
)
