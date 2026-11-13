// Copyright 2026 Bardia Samiee. All Rights Reserved.

#include "Misc/AutomationTest.h"
#include "ToleranceLibrary.h"

#if WITH_AUTOMATION_WORKER

IMPLEMENT_SIMPLE_AUTOMATION_TEST(FToleranceWithinTest, "Tolerance.Within", EAutomationTestFlags_ApplicationContextMask | EAutomationTestFlags::EngineFilter)

bool FToleranceWithinTest::RunTest(const FString& Parameters)
{
	TestTrue(TEXT("A value is within zero units of itself"), UToleranceLibrary::Within(1.0, 1.0, 0));
	TestTrue(TEXT("The next double is within one unit"), UToleranceLibrary::Within(1.0, 1.0 + DBL_EPSILON, 1));
	TestFalse(TEXT("A value one billionth away is outside four units"), UToleranceLibrary::Within(1.0, 1.0 + 1.0e-9, 4));
	TestFalse(TEXT("A negative count is refused"), UToleranceLibrary::Within(1.0, 1.0, -1));
	return true;
}

#endif
