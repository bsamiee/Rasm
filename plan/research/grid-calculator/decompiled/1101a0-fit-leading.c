/*
Fit Leading, Quick (typography.md [01] row 01, README [08] row 01).
FUN_001101a0 is the leading field observer: N = Round(H / L) at 0x157c8 then 0x15abc, L_fit = H / N at 0x157c8,
quantised at 0x152f0 mode 3, guarded by N != 0 (0x132fc), L_fit > 1.0 (0x3208c) and L_fit < H (0x15808).

PMReal helpers: construct 0x13490, operator/ 0x157c8, operator* 0x15a7c, operator+ 0x17c4c, operator- 0x28080,
Round 0x15abc (floor(x + 0.5)), floor 0x37d70, a > b + 1e-8 0x14140, a >= b - 1e-8 0x15808, a <= b + 1e-8 0x31d38,
a < b - 1e-8 0x3208c, |a - b| < 1e-8 0x132fc, not equal 0x18504, quantise 0x152f0 (mode 3 three decimals, mode 4 four).
Widget access: read value 0x13338, read value in the panel unit 0x1544c, read checkbox 0x12ef4, read dropdown index 0x13034,
write value 0x13c20, set maximum 0x196d0, unit factor at object + 0x88.
*/

//==== FUNC @ 110574 -> 001101a0

/* WARNING: Restarted to delay deadcode elimination for space: stack */

void FUN_001101a0(undefined8 param_1)

{
  bool bVar1;
  bool bVar2;
  short sVar3;
  undefined8 uVar4;
  long *plVar5;
  undefined1 auStack_630 [79];
  byte local_5e1;
  undefined1 auStack_5e0 [72];
  undefined1 auStack_598 [72];
  undefined1 auStack_550 [72];
  undefined1 auStack_508 [72];
  undefined1 auStack_4c0 [4];
  undefined1 auStack_4bc [4];
  undefined1 auStack_4b8 [76];
  undefined1 auStack_46c [4];
  undefined1 auStack_468 [76];
  undefined1 auStack_41c [4];
  undefined1 auStack_418 [4];
  undefined1 auStack_414 [4];
  undefined1 auStack_410 [4];
  undefined1 auStack_40c [4];
  undefined1 auStack_408 [76];
  undefined1 auStack_3bc [4];
  undefined1 auStack_3b8 [72];
  undefined1 auStack_370 [4];
  undefined1 auStack_36c [4];
  undefined1 auStack_368 [4];
  undefined1 auStack_364 [4];
  undefined1 auStack_360 [4];
  undefined1 auStack_35c [4];
  undefined1 auStack_358 [76];
  undefined1 auStack_30c [4];
  undefined1 auStack_308 [72];
  undefined1 auStack_2c0 [8];
  undefined4 local_2b8;
  undefined4 local_2b4;
  undefined8 local_2b0;
  undefined8 local_2a8;
  undefined8 local_2a0;
  undefined1 auStack_294 [4];
  undefined1 auStack_290 [4];
  undefined1 auStack_28c [4];
  undefined1 auStack_288 [4];
  undefined1 auStack_284 [4];
  undefined1 auStack_280 [76];
  undefined1 auStack_234 [4];
  undefined1 auStack_230 [72];
  undefined1 auStack_1e8 [8];
  undefined8 local_1e0;
  undefined8 local_1d8;
  undefined1 auStack_1cc [4];
  undefined8 local_1c8;
  undefined4 local_1c0;
  undefined1 auStack_1bc [4];
  undefined1 auStack_1b8 [76];
  undefined1 auStack_16c [4];
  undefined8 local_168;
  undefined1 auStack_160 [4];
  undefined1 auStack_15c [4];
  undefined1 auStack_158 [4];
  undefined1 auStack_154 [4];
  undefined1 auStack_150 [4];
  undefined1 auStack_14c [4];
  undefined1 auStack_148 [72];
  undefined1 auStack_100 [4];
  undefined1 auStack_fc [4];
  undefined1 auStack_f8 [72];
  undefined1 auStack_b0 [15];
  undefined1 uStack_a1;
  undefined1 auStack_a0 [8];
  undefined4 local_98;
  undefined1 uStack_81;
  undefined1 auStack_80 [8];
  undefined1 auStack_78 [78];
  short local_2a;
  undefined8 local_28;
  
  local_2a = 0;
  local_28 = param_1;
  __ZN8PMStringC1Ev(auStack_78);
  FUN_00002bf4(auStack_80,param_1,&uStack_81);
  sVar3 = FUN_00002c30(auStack_80);
  if (sVar3 == 0) {
    uVar4 = __Z26GetExecutionContextSessionv();
    FUN_00002978(auStack_a0,uVar4,&uStack_a1);
    sVar3 = FUN_000029b4(auStack_a0);
    if (sVar3 == 0) {
      FUN_001a2484(auStack_b0);
      sVar3 = FUN_0000e2a4(auStack_b0);
      if (sVar3 == 0) {
        FUN_00002cf8(auStack_fc,0x15d32f);
        FUN_000138a8(auStack_f8,auStack_80,auStack_fc);
        FUN_0000dd8c(auStack_78);
        __ZN8PMStringD1Ev(auStack_f8);
        FUN_00002cf8(auStack_100,0x15d323);
        local_2a = FUN_00012ef4(auStack_80,auStack_100);
        FUN_00002cf8(auStack_14c,0x15d31c);
        FUN_000138a8(auStack_148,auStack_80,auStack_14c);
        sVar3 = __ZNK8PMString7IsEmptyEv(auStack_148);
        if (sVar3 == 0) {
          FUN_00002cf8(auStack_16c,0x15d31c);
          local_168 = FUN_00013338(auStack_80,auStack_16c);
          FUN_00002cf8(auStack_1bc,0x15d306);
          local_1c0 = 0xffffffff;
          FUN_00012c10(auStack_1b8,auStack_80,auStack_1bc,&local_1c0);
          FUN_00002cf8(auStack_1cc,&DAT_0015d31b);
          local_1c8 = FUN_0001544c(auStack_80,auStack_1cc,auStack_1b8);
          local_1e0 = FUN_000157c8(&local_1c8,&local_168);
          local_1d8 = FUN_00015abc(&local_1e0);
          FUN_00013490(0);
          sVar3 = FUN_000132fc(&local_1d8,auStack_1e8);
          if (sVar3 == 0) {
            local_2b0 = FUN_000157c8(&local_1c8,&local_168);
            local_2a8 = FUN_00015abc(&local_2b0);
            local_2a0 = FUN_000157c8(&local_1c8,&local_2a8);
            local_2b4 = 3;
            FUN_000152f0(&local_2a0,&local_2b4);
            local_2b8 = 3;
            FUN_000152f0(&local_1c8,&local_2b8);
            FUN_00013490(0x3ff0000000000000);
            sVar3 = FUN_0003208c(&local_2a0,auStack_2c0);
            if (sVar3 == 0) {
              sVar3 = FUN_00015808(&local_2a0,&local_1c8);
              if (sVar3 == 0) {
                FUN_00002cf8(auStack_46c,0x15d32f);
                FUN_000138a8(auStack_468,auStack_80,auStack_46c);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                          (auStack_4b8,"0x15d300kGCCalculateKey",0);
                sVar3 = __ZNK8PMString7IsEqualERKS_hh(auStack_468,auStack_4b8,1,0);
                __ZN8PMStringD1Ev(auStack_4b8);
                __ZN8PMStringD1Ev(auStack_468);
                if (sVar3 != 0) {
                  FUN_00002cf8(auStack_4bc,0x15d324);
                  FUN_00011dc4(auStack_80,auStack_4bc,&DAT_00208c52);
                }
                FUN_00002cf8(auStack_4c0,0x15d323);
                FUN_00011dc4(auStack_80,auStack_4c0,&DAT_00208c52);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_508,"[GC] CGS",0);
                FUN_001b4670(auStack_508);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_550,"[GC] Smart Setup",0);
                FUN_001b4670(auStack_550);
                __ZN8PMStringD1Ev(auStack_550);
                plVar5 = (long *)FUN_000029d8(auStack_a0);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_598,"BDS",0);
                (**(code **)(*plVar5 + 0x140))(plVar5,auStack_598);
                __ZN8PMStringD1Ev(auStack_598);
                __ZN8PMStringD1Ev(auStack_508);
                local_98 = 0;
              }
              else {
                FUN_00002cf8(auStack_370,0x15d31d);
                FUN_00013c20(auStack_80,auStack_370,&local_2a0);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                          (auStack_3b8,
                           "The output value for the leading is either the same as the document height or greater, please reduce the value."
                           ,0);
                __ZN6CAlert16InformationAlertERK8PMString(auStack_3b8);
                FUN_00002cf8(auStack_3bc,0x15d31c);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_408,"",0);
                FUN_00013588(auStack_80,auStack_3bc,auStack_408);
                __ZN8PMStringD1Ev(auStack_408);
                FUN_00002cf8(auStack_40c,0x15d324);
                FUN_00011dc4(auStack_80,auStack_40c,&DAT_00208c50);
                FUN_00002cf8(auStack_410,0x15d323);
                FUN_00011dc4(auStack_80,auStack_410,&DAT_00208c50);
                FUN_00002cf8(auStack_414,0x15d323);
                FUN_00013178(auStack_80,auStack_414,&DAT_00208c50);
                FUN_00002cf8(auStack_418,0x15d326);
                FUN_000193a8(auStack_80,auStack_418);
                FUN_00002cf8(auStack_41c,0x15d326);
                FUN_00011dc4(auStack_80,auStack_41c,&DAT_00208c50);
                local_98 = 2;
                __ZN8PMStringD1Ev(auStack_3b8);
              }
            }
            else {
              __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                        (auStack_308,
                         "Please enter a minimum value of 1 pt or smaller than document height.",0);
              __ZN6CAlert16InformationAlertERK8PMString(auStack_308);
              FUN_00002cf8(auStack_30c,0x15d31c);
              __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_358,"",0);
              FUN_00013588(auStack_80,auStack_30c,auStack_358);
              __ZN8PMStringD1Ev(auStack_358);
              FUN_00002cf8(auStack_35c,0x15d324);
              FUN_00011dc4(auStack_80,auStack_35c,&DAT_00208c50);
              FUN_00002cf8(auStack_360,0x15d323);
              FUN_00011dc4(auStack_80,auStack_360,&DAT_00208c50);
              FUN_00002cf8(auStack_364,0x15d323);
              FUN_00013178(auStack_80,auStack_364,&DAT_00208c50);
              FUN_00002cf8(auStack_368,0x15d326);
              FUN_000193a8(auStack_80,auStack_368);
              FUN_00002cf8(auStack_36c,0x15d326);
              FUN_00011dc4(auStack_80,auStack_36c,&DAT_00208c50);
              local_98 = 2;
              __ZN8PMStringD1Ev(auStack_308);
            }
          }
          else {
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                      (auStack_230,
                       "The value for the leading is very large than the document height, please reduce the value."
                       ,0);
            __ZN6CAlert16InformationAlertERK8PMString(auStack_230);
            FUN_00002cf8(auStack_234,0x15d31c);
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_280,"",0);
            FUN_00013588(auStack_80,auStack_234,auStack_280);
            __ZN8PMStringD1Ev(auStack_280);
            FUN_00002cf8(auStack_284,0x15d324);
            FUN_00011dc4(auStack_80,auStack_284,&DAT_00208c50);
            FUN_00002cf8(auStack_288,0x15d323);
            FUN_00011dc4(auStack_80,auStack_288,&DAT_00208c50);
            FUN_00002cf8(auStack_28c,0x15d323);
            FUN_00013178(auStack_80,auStack_28c,&DAT_00208c50);
            FUN_00002cf8(auStack_290,0x15d326);
            FUN_000193a8(auStack_80,auStack_290);
            FUN_00002cf8(auStack_294,0x15d326);
            FUN_00011dc4(auStack_80,auStack_294,&DAT_00208c50);
            local_98 = 2;
            __ZN8PMStringD1Ev(auStack_230);
          }
          __ZN8PMStringD1Ev(auStack_1b8);
        }
        else {
          FUN_00002cf8(auStack_150,0x15d324);
          FUN_00011dc4(auStack_80,auStack_150,&DAT_00208c50);
          FUN_00002cf8(auStack_154,0x15d323);
          FUN_00011dc4(auStack_80,auStack_154,&DAT_00208c50);
          FUN_00002cf8(auStack_158,0x15d323);
          FUN_00013178(auStack_80,auStack_158,&DAT_00208c50);
          FUN_00002cf8(auStack_15c,0x15d326);
          FUN_000193a8(auStack_80,auStack_15c);
          FUN_00002cf8(auStack_160,0x15d326);
          FUN_00011dc4(auStack_80,auStack_160,&DAT_00208c50);
          local_98 = 2;
        }
        __ZN8PMStringD1Ev(auStack_148);
      }
      else {
        local_98 = 2;
      }
      FUN_0000e2f8(auStack_b0);
    }
    else {
      local_98 = 2;
    }
    FUN_00002b9c(auStack_a0);
  }
  else {
    local_98 = 2;
  }
  FUN_00002c54(auStack_80);
  local_5e1 = 0;
  bVar1 = false;
  bVar2 = false;
  if (local_2a == 0) {
    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_5e0,"[GC] Set Leading",0);
    local_5e1 = 1;
    sVar3 = FUN_001a6604(auStack_5e0);
    bVar2 = false;
    if (sVar3 != 0) {
      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_630,"0x15d300kGCUnDoKey",0);
      bVar1 = true;
      sVar3 = __ZNK8PMString7IsEqualERKS_hh(auStack_78,auStack_630,1,0);
      bVar2 = sVar3 != 0;
    }
  }
  if (bVar1) {
    __ZN8PMStringD1Ev(auStack_630);
  }
  if ((local_5e1 & 1) != 0) {
    __ZN8PMStringD1Ev(auStack_5e0);
  }
  if (bVar2) {
    FUN_00128118(param_1);
  }
  else {
    FUN_000e8428(param_1);
  }
  FUN_0014b630(param_1,&DAT_00208c50);
  __ZN8PMStringD1Ev(auStack_78);
  return;
}
