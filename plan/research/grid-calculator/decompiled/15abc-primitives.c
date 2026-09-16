/*
The arithmetic primitives every row relies on: Round, quantise, and the six tolerance comparisons, plus FUN_0005c3d4 (ceil for
positive non-integers) and the std::sort and std::reverse wrappers the Smart list uses.

PMReal helpers: construct 0x13490, operator/ 0x157c8, operator* 0x15a7c, operator+ 0x17c4c, operator- 0x28080,
Round 0x15abc (floor(x + 0.5)), floor 0x37d70, a > b + 1e-8 0x14140, a >= b - 1e-8 0x15808, a <= b + 1e-8 0x31d38,
a < b - 1e-8 0x3208c, |a - b| < 1e-8 0x132fc, not equal 0x18504, quantise 0x152f0 (mode 3 three decimals, mode 4 four).
Widget access: read value 0x13338, read value in the panel unit 0x1544c, read checkbox 0x12ef4, read dropdown index 0x13034,
write value 0x13c20, set maximum 0x196d0, unit factor at object + 0x88.
*/

//==== FUNC @ 15abc -> 00015abc

undefined1  [16] FUN_00015abc(double *param_1)

{
  undefined1 auVar1 [16];
  undefined1 auStack_28 [8];
  double *local_20;
  
  local_20 = param_1;
  FUN_00013490(*param_1 + 0.5);
  auVar1._0_8_ = FUN_00037d70(auStack_28);
  auVar1._8_8_ = 0;
  return auVar1;
}

//==== FUNC @ 152f0 -> 000152f0

void FUN_000152f0(undefined8 *param_1,int *param_2)

{
  undefined8 uVar1;
  undefined1 auStack_70 [8];
  undefined8 local_68 [2];
  undefined1 auStack_58 [8];
  undefined8 local_50;
  undefined1 auStack_48 [8];
  undefined8 local_40 [2];
  undefined1 auStack_30 [8];
  undefined8 local_28;
  int *local_20;
  undefined8 *local_18;
  
  local_20 = param_2;
  local_18 = param_1;
  if (*param_2 == 3) {
    FUN_00013490();
    local_28 = FUN_00015a7c(param_1,auStack_30);
    *local_18 = local_28;
    local_40[0] = FUN_00015abc(local_18);
    FUN_00013490(0x408f400000000000);
    uVar1 = FUN_000157c8(local_40,auStack_48);
    *local_18 = uVar1;
  }
  else if (*param_2 == 4) {
    FUN_00013490();
    local_50 = FUN_00015a7c(param_1,auStack_58);
    *local_18 = local_50;
    local_68[0] = FUN_00015abc(local_18);
    FUN_00013490(0x40c3880000000000);
    uVar1 = FUN_000157c8(local_68,auStack_70);
    *local_18 = uVar1;
  }
  return;
}

//==== FUNC @ 132fc -> 000132fc

bool FUN_000132fc(double *param_1,double *param_2)

{
  return ABS(*param_1 - *param_2) < 1e-08;
}

//==== FUNC @ 18504 -> 00018504

bool FUN_00018504(undefined8 param_1,undefined8 param_2)

{
  short sVar1;
  
  sVar1 = FUN_000132fc(param_1,param_2);
  return sVar1 == 0;
}

//==== FUNC @ 14140 -> 00014140

bool FUN_00014140(double *param_1,double *param_2)

{
  return *param_2 + 1e-08 < *param_1;
}

//==== FUNC @ 15808 -> 00015808

bool FUN_00015808(double *param_1,double *param_2)

{
  return *param_2 - 1e-08 <= *param_1;
}

//==== FUNC @ 31d38 -> 00031d38

bool FUN_00031d38(double *param_1,double *param_2)

{
  return *param_1 <= *param_2 + 1e-08;
}

//==== FUNC @ 3208c -> 0003208c

bool FUN_0003208c(double *param_1,double *param_2)

{
  return *param_1 < *param_2 - 1e-08;
}

//==== FUNC @ 5c3d4 -> 0005c3d4

undefined1  [16] FUN_0005c3d4(double *param_1)

{
  double dVar1;
  undefined1 auVar2 [16];
  int local_24;
  ulong local_18;
  
  if ((*param_1 <= 0.0) || (*param_1 == (double)(int)*param_1)) {
    dVar1 = *param_1;
  }
  else {
    dVar1 = *param_1 + 1.0;
  }
  local_24 = (int)dVar1;
  FUN_00013490((double)local_24,&local_18);
  auVar2._8_8_ = 0;
  auVar2._0_8_ = local_18;
  return auVar2;
}

//==== FUNC @ c54f8 -> 000c54f8

void FUN_000c54f8(undefined8 param_1,undefined8 param_2,undefined8 param_3)

{
  undefined8 local_28;
  undefined8 local_20;
  undefined8 local_18;
  
  local_28 = param_3;
  local_20 = param_2;
  local_18 = param_1;
  FUN_000c5ce0(param_1,param_2,&local_28);
  return;
}

//==== FUNC @ c5540 -> 000c5540

void FUN_000c5540(undefined8 param_1,undefined8 param_2)

{
  FUN_000c9df8(param_1,param_2);
  return;
}
