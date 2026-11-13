// Copyright 2026 Bardia Samiee. All Rights Reserved.

#pragma once

#include "Kismet/BlueprintFunctionLibrary.h"

#include "ToleranceLibrary.generated.h"

UCLASS()
class TOLERANCE_API UToleranceLibrary : public UBlueprintFunctionLibrary
{
	GENERATED_BODY()

public:
	UFUNCTION(BlueprintPure, Category = "Math|Tolerance", meta = (DisplayName = "Within Units In The Last Place"))
	static bool Within(double Left, double Right, int32 Ulps);
};
