# --- [OPERATIONS] -----------------------------------------------------------------------

include("${TOOLCHAIN}")
foreach(language IN ITEMS C CXX OBJCXX)
    if(CMAKE_${language}_COMPILER)
        execute_process(COMMAND "${CMAKE_${language}_COMPILER}" --version OUTPUT_VARIABLE reported COMMAND_ERROR_IS_FATAL ANY)
        string(REGEX MATCH "^[^\n]*" release "${reported}")
        message(STATUS "${language} ${release}")
    endif()
endforeach()
