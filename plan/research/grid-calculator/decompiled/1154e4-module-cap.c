/*
Module cap (row 22). FUN_001154e4 is the horizontal module count observer: s = W / modules at 0x157c8, and the module size
must satisfy 1 pt <= s <= 1000 pt (0.352778 mm to 352.778 mm, 0.0138889 in to 13.8889 in, the literals at 0x3208c and
0x14140); outside the range the module size field 0x15d33a is blanked and nothing applies; inside, modules * subdivisions is
sent to the grid division through vtable + 0xf0.

PMReal helpers: construct 0x13490, operator/ 0x157c8, operator* 0x15a7c, operator+ 0x17c4c, operator- 0x28080,
Round 0x15abc (floor(x + 0.5)), floor 0x37d70, a > b + 1e-8 0x14140, a >= b - 1e-8 0x15808, a <= b + 1e-8 0x31d38,
a < b - 1e-8 0x3208c, |a - b| < 1e-8 0x132fc, not equal 0x18504, quantise 0x152f0 (mode 3 three decimals, mode 4 four).
Widget access: read value 0x13338, read value in the panel unit 0x1544c, read checkbox 0x12ef4, read dropdown index 0x13034,
write value 0x13c20, set maximum 0x196d0, unit factor at object + 0x88.
*/

//==== FUNC @ 1154e4 -> 001154e4

void FUN_001154e4(undefined8 param_1)

{
  bool bVar1;
  short sVar2;
  undefined8 uVar3;
  long *plVar4;
  undefined2 local_3ea;
  undefined1 auStack_3e8 [10];
  undefined2 local_3de;
  undefined1 auStack_3dc [4];
  undefined8 local_3d8;
  undefined1 auStack_3cc [4];
  undefined8 local_3c8;
  undefined1 auStack_3c0 [12];
  undefined1 auStack_3b4 [4];
  undefined1 auStack_3b0 [76];
  undefined1 auStack_364 [4];
  undefined1 auStack_360 [8];
  undefined1 auStack_358 [15];
  byte local_349;
  undefined1 auStack_348 [72];
  undefined1 auStack_300 [8];
  undefined1 auStack_2f8 [15];
  byte local_2e9;
  undefined1 auStack_2e8 [72];
  undefined1 auStack_2a0 [8];
  undefined1 auStack_298 [8];
  undefined1 auStack_290 [72];
  undefined4 local_248;
  undefined1 auStack_244 [4];
  undefined1 auStack_240 [72];
  undefined8 local_1f8;
  undefined1 auStack_1ec [4];
  undefined8 local_1e8;
  undefined8 local_1e0;
  undefined4 local_1d4;
  undefined1 auStack_1d0 [12];
  undefined1 auStack_1c4 [4];
  undefined8 local_1c0;
  undefined1 auStack_1b4 [4];
  undefined8 local_1b0;
  undefined1 auStack_1a4 [4];
  undefined1 auStack_1a0 [76];
  undefined1 auStack_154 [4];
  undefined1 auStack_150 [76];
  undefined1 auStack_104 [4];
  undefined1 auStack_100 [72];
  undefined1 auStack_b8 [4];
  undefined1 auStack_b4 [4];
  undefined1 auStack_b0 [72];
  undefined1 local_68 [16];
  undefined1 uStack_51;
  undefined1 auStack_50 [8];
  undefined4 local_48;
  undefined1 uStack_31;
  undefined1 auStack_30 [8];
  undefined8 local_28;
  
  local_28 = param_1;
  FUN_00002bf4(auStack_30,param_1,&uStack_31);
  sVar2 = FUN_00002c30(auStack_30);
  if (sVar2 != 0) {
    local_48 = 2;
    goto LAB_00115e6c;
  }
  uVar3 = __Z26GetExecutionContextSessionv();
  FUN_00002978(auStack_50,uVar3,&uStack_51);
  sVar2 = FUN_000029b4(auStack_50);
  if (sVar2 == 0) {
    plVar4 = (long *)FUN_000029d8(auStack_50);
    local_68 = (**(code **)(*plVar4 + 0x18))();
    FUN_00002cf8(auStack_b4,0x15d339);
    FUN_000138a8(auStack_b0,auStack_30,auStack_b4);
    sVar2 = __ZNK8PMString7IsEmptyEv(auStack_b0);
    if (sVar2 == 0) {
      FUN_00002cf8(auStack_1b4,0x15d339);
      local_1b0 = FUN_00013338(auStack_30,auStack_1b4);
      FUN_00002cf8(auStack_1c4,0x15d33b);
      local_1c0 = FUN_00013338(auStack_30,auStack_1c4);
      FUN_00013490(0);
      sVar2 = FUN_00014140(&local_1c0,auStack_1d0);
      if (sVar2 != 0) {
        plVar4 = (long *)FUN_000029d8(auStack_50);
        local_1e0 = FUN_00015a7c(&local_1b0,&local_1c0);
        local_1d4 = FUN_00032070(&local_1e0);
        (**(code **)(*plVar4 + 0xf0))(plVar4,&local_1d4);
      }
      FUN_00002cf8(auStack_1ec,0x15d338);
      local_1e8 = FUN_00013338(auStack_30,auStack_1ec);
      local_1f8 = FUN_000157c8(&local_1e8,&local_1b0);
      FUN_00002cf8(auStack_244,0x15d306);
      local_248 = 0xffffffff;
      FUN_00012c10(auStack_240,auStack_30,auStack_244,&local_248);
      local_2e9 = 0;
      local_349 = 0;
      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_290,"0x15d300kGCMillimetersKey");
      sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_240,auStack_290,1,0);
      if (sVar2 == 0) {
LAB_00115a38:
        __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_2e8,"0x15d300kGCInchesKey",0);
        local_2e9 = 1;
        sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_240,auStack_2e8,1,0);
        if (sVar2 == 0) {
LAB_00115afc:
          __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_348,"0x15d300kGCPointsPixelsKey",0);
          local_349 = 1;
          sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_240,auStack_348,1,0);
          bVar1 = false;
          if (sVar2 != 0) {
            FUN_00013490(0x3ff0000000000000);
            sVar2 = FUN_0003208c(&local_1f8,auStack_358);
            bVar1 = true;
            if (sVar2 == 0) {
              FUN_00013490(0x408f400000000000);
              sVar2 = FUN_00014140(&local_1f8,auStack_360);
              bVar1 = sVar2 != 0;
            }
          }
        }
        else {
          FUN_00013490(0x3f8c779a6b50b0f2);
          sVar2 = FUN_0003208c(&local_1f8,auStack_2f8);
          bVar1 = true;
          if (sVar2 == 0) {
            FUN_00013490(0x402bc71de69ad42c);
            sVar2 = FUN_00014140(&local_1f8,auStack_300);
            bVar1 = true;
            if (sVar2 == 0) goto LAB_00115afc;
          }
        }
      }
      else {
        FUN_00013490(0x3fd6978d4fdf3b64);
        sVar2 = FUN_0003208c(&local_1f8,auStack_298);
        bVar1 = true;
        if (sVar2 == 0) {
          FUN_00013490(0x40760c72b020c49c);
          sVar2 = FUN_00014140(&local_1f8,auStack_2a0);
          bVar1 = true;
          if (sVar2 == 0) goto LAB_00115a38;
        }
      }
      if ((local_349 & 1) != 0) {
        __ZN8PMStringD1Ev(auStack_348);
      }
      if ((local_2e9 & 1) != 0) {
        __ZN8PMStringD1Ev(auStack_2e8);
      }
      __ZN8PMStringD1Ev(auStack_290);
      if (bVar1) {
        FUN_00002cf8(auStack_364,0x15d33a);
        __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_3b0,"",0);
        FUN_00013588(auStack_30,auStack_364,auStack_3b0);
        __ZN8PMStringD1Ev(auStack_3b0);
      }
      else {
        FUN_00002cf8(auStack_3b4,0x15d33a);
        FUN_00013c20(auStack_30,auStack_3b4,&local_1f8);
        FUN_00013490(0);
        sVar2 = FUN_00014140(&local_1c0,auStack_3c0);
        if (sVar2 != 0) {
          local_3c8 = FUN_000157c8(&local_1f8,&local_1c0);
          FUN_00002cf8(auStack_3cc,0x15d33c);
          FUN_00013c20(auStack_30,auStack_3cc,&local_3c8);
        }
      }
      FUN_00002cf8(auStack_3dc,0x15d345);
      local_3d8 = FUN_00013338(auStack_30,auStack_3dc);
      local_3de = 0;
      FUN_0015d72c(param_1,&local_3de);
      FUN_00013490(0);
      sVar2 = FUN_000132fc(&local_3d8,auStack_3e8);
      if (sVar2 != 0) {
        local_3ea = 1;
        FUN_0015d72c(param_1,&local_3ea);
      }
      __ZN8PMStringD1Ev(auStack_240);
      local_48 = 0;
    }
    else {
      FUN_00002cf8(auStack_b8,0x15d33a);
      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_100,"",0);
      FUN_00013588(auStack_30,auStack_b8,auStack_100);
      __ZN8PMStringD1Ev(auStack_100);
      FUN_00002cf8(auStack_104,0x15d33c);
      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_150,"",0);
      FUN_00013588(auStack_30,auStack_104,auStack_150);
      __ZN8PMStringD1Ev(auStack_150);
      FUN_00002cf8(auStack_154,0x15d30b);
      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1a0,"",0);
      FUN_00013588(auStack_30,auStack_154,auStack_1a0);
      __ZN8PMStringD1Ev(auStack_1a0);
      FUN_00002cf8(auStack_1a4,0x15d33e);
      FUN_00011dc4(auStack_30,auStack_1a4,&DAT_00208c50);
      local_48 = 2;
    }
    __ZN8PMStringD1Ev(auStack_b0);
  }
  else {
    local_48 = 2;
  }
  FUN_00002b9c(auStack_50);
LAB_00115e6c:
  FUN_00002c54(auStack_30);
  return;
}
