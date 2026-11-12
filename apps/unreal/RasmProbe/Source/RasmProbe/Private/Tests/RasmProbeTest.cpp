// Copyright 2026 Bardia Samiee. All Rights Reserved.

#include "Misc/AutomationTest.h"
#include "RasmProbe.h"

#include <rasm/tolerance.hpp>

#if WITH_AUTOMATION_WORKER

IMPLEMENT_SIMPLE_AUTOMATION_TEST(FRasmProbeDoubledTest, "RasmProbe.Doubled", EAutomationTestFlags_ApplicationContextMask | EAutomationTestFlags::EngineFilter)

bool FRasmProbeDoubledTest::RunTest(const FString& Parameters)
{
	if (FRasmProbeModule::Doubled(21) != 42)
	{
		AddError(TEXT("Doubled(21) is not 42."));
	}
	return true;
}

IMPLEMENT_SIMPLE_AUTOMATION_TEST(FRasmProbeToleranceTest, "RasmProbe.Tolerance", EAutomationTestFlags_ApplicationContextMask | EAutomationTestFlags::EngineFilter)

bool FRasmProbeToleranceTest::RunTest(const FString& Parameters)
{
	if (!rasm::within(1.0, 1.0, rasm::default_ulps))
	{
		AddError(TEXT("within(1.0, 1.0) is false."));
	}
	if (rasm::labelled(rasm::default_ulps) != std::string("4 ulps"))
	{
		AddError(TEXT("labelled(4) is not \"4 ulps\"."));
	}
	return true;
}

#endif
