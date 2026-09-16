/*
Subdivision and the Quick horizontal unit (rows 02 and 03).
FUN_0010772c reads the subdivision dropdown index, fits L or L * k at 0x157c8/0x15abc/0x157c8 and derives u_h = L_fit * (W / H).

PMReal helpers: construct 0x13490, operator/ 0x157c8, operator* 0x15a7c, operator+ 0x17c4c, operator- 0x28080,
Round 0x15abc (floor(x + 0.5)), floor 0x37d70, a > b + 1e-8 0x14140, a >= b - 1e-8 0x15808, a <= b + 1e-8 0x31d38,
a < b - 1e-8 0x3208c, |a - b| < 1e-8 0x132fc, not equal 0x18504, quantise 0x152f0 (mode 3 three decimals, mode 4 four).
Widget access: read value 0x13338, read value in the panel unit 0x1544c, read checkbox 0x12ef4, read dropdown index 0x13034,
write value 0x13c20, set maximum 0x196d0, unit factor at object + 0x88.
*/

//==== FUNC @ 107cc8 -> 0010772c

void FUN_0010772c(undefined8 param_1)

{
  bool bVar1;
  short sVar2;
  int iVar3;
  undefined8 uVar4;
  long *plVar5;
  long *plVar6;
  undefined1 auStack_5d8 [76];
  undefined1 auStack_58c [4];
  undefined1 auStack_588 [79];
  byte local_539;
  undefined1 auStack_538 [79];
  byte local_4e9;
  undefined1 auStack_4e8 [72];
  undefined1 auStack_4a0 [4];
  undefined1 auStack_49c [6];
  undefined2 local_496;
  undefined1 auStack_494 [4];
  undefined1 auStack_490 [76];
  int local_444;
  undefined1 auStack_440 [4];
  undefined1 auStack_43c [4];
  undefined1 auStack_438 [12];
  undefined1 auStack_42c [4];
  undefined1 auStack_428 [12];
  undefined1 auStack_41c [4];
  undefined1 auStack_418 [4];
  undefined1 auStack_414 [4];
  undefined1 auStack_410 [12];
  undefined1 auStack_404 [4];
  undefined1 auStack_400 [12];
  undefined1 auStack_3f4 [4];
  undefined1 auStack_3f0 [4];
  undefined4 local_3ec;
  undefined4 local_3e8;
  undefined4 local_3e4;
  undefined8 local_3e0;
  undefined4 local_3d4;
  undefined1 auStack_3d0 [8];
  undefined8 local_3c8;
  undefined8 local_3c0;
  undefined8 local_3b8;
  undefined8 local_3b0;
  undefined1 auStack_3a8 [76];
  undefined1 auStack_35c [4];
  undefined1 auStack_358 [72];
  undefined8 local_310;
  undefined8 local_308;
  undefined8 local_300;
  undefined8 local_2f8;
  undefined8 local_2f0;
  undefined8 local_2e8;
  undefined8 local_2e0;
  undefined1 auStack_2d8 [8];
  undefined8 local_2d0;
  undefined8 local_2c8;
  undefined8 local_2c0;
  undefined8 local_2b8;
  undefined8 local_2b0;
  undefined8 local_2a8;
  undefined8 local_2a0;
  undefined8 local_298;
  undefined8 local_290;
  undefined1 auStack_288 [76];
  int local_23c;
  undefined4 local_238;
  undefined1 auStack_234 [4];
  undefined1 auStack_230 [76];
  undefined1 auStack_1e4 [4];
  undefined8 local_1e0;
  undefined1 auStack_1d4 [4];
  undefined8 local_1d0;
  undefined1 auStack_1c4 [4];
  undefined8 local_1c0;
  undefined1 auStack_1b4 [4];
  undefined8 local_1b0;
  undefined1 auStack_1a4 [4];
  short local_1a0;
  short local_19e;
  int local_19c;
  undefined8 local_198;
  undefined8 local_190;
  undefined1 auStack_188 [72];
  undefined1 auStack_140 [72];
  undefined4 local_f8;
  undefined1 auStack_f4 [4];
  undefined1 auStack_f0 [76];
  undefined1 auStack_a4 [4];
  int local_a0;
  undefined1 uStack_99;
  undefined1 auStack_98 [8];
  undefined1 auStack_90 [8];
  undefined1 auStack_88 [8];
  undefined1 auStack_80 [15];
  undefined1 uStack_71;
  undefined1 auStack_70 [8];
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
    goto LAB_001089f8;
  }
  uVar4 = __Z26GetExecutionContextSessionv();
  FUN_00002978(auStack_50,uVar4,&uStack_51);
  sVar2 = FUN_000029b4(auStack_50);
  if (sVar2 == 0) {
    plVar5 = (long *)FUN_000029d8(auStack_50);
    local_68 = (**(code **)(*plVar5 + 0x18))();
    FUN_0000df20(auStack_70,local_68,&uStack_71);
    sVar2 = FUN_0000df5c(auStack_70);
    if (sVar2 == 0) {
      FUN_001a7968(auStack_80);
      sVar2 = FUN_00016b78(auStack_80);
      if (sVar2 == 0) {
        FUN_001a7c08(auStack_88);
        sVar2 = FUN_0000e280(auStack_88);
        if (sVar2 == 0) {
          FUN_001a2484(auStack_90);
          sVar2 = FUN_0000e2a4(auStack_90);
          if (sVar2 == 0) {
            uVar4 = FUN_0000df80(auStack_70);
            FUN_0000df98(auStack_98,uVar4,&uStack_99);
            sVar2 = FUN_0000dfd4(auStack_98);
            if (sVar2 == 0) {
              FUN_00002cf8(auStack_a4,0x15d326);
              local_a0 = FUN_00013034(auStack_30,auStack_a4);
              FUN_00002cf8(auStack_f4,0x15d306);
              local_f8 = 0xffffffff;
              FUN_00012c10(auStack_f0,auStack_30,auStack_f4,&local_f8);
              __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_140,"[GC] Subdivision: ",0);
              FUN_001cc9ac(auStack_188,auStack_140);
              FUN_001b4670(auStack_188);
              FUN_00015a50(&local_190);
              FUN_00015a50(&local_198);
              local_19c = 1;
              local_19e = 0;
              FUN_00002cf8(auStack_1a4,0x15d323);
              local_1a0 = FUN_00012ef4(auStack_30,auStack_1a4);
              FUN_00002cf8(auStack_1b4,0x15d31c);
              local_1b0 = FUN_00013338(auStack_30,auStack_1b4);
              FUN_00002cf8(auStack_1c4,0x15d31d);
              local_1c0 = FUN_00013338(auStack_30,auStack_1c4);
              FUN_00002cf8(auStack_1d4,&DAT_0015d31a);
              local_1d0 = FUN_0001544c(auStack_30,auStack_1d4,auStack_f0);
              FUN_00002cf8(auStack_1e4,&DAT_0015d31b);
              local_1e0 = FUN_0001544c(auStack_30,auStack_1e4,auStack_f0);
              FUN_00002cf8(auStack_234,0x15d326);
              local_238 = 0xffffffff;
              FUN_00012c10(auStack_230,auStack_30,auStack_234,&local_238);
              __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_288," pt",0);
              iVar3 = __ZNK8PMString13IndexOfStringERKS_i(auStack_230,auStack_288,0);
              __ZN8PMStringD1Ev(auStack_288);
              local_23c = iVar3;
              if (iVar3 < 0) {
                local_48 = 2;
              }
              else {
                if ((local_a0 < 1) || (4 < local_a0)) {
                  if ((4 < local_a0) || (local_a0 == 0)) {
                    __ZN8PMString6AppendEPKciNS_14StringEncodingE
                              (auStack_140," Multiplication",0x7fffffff,0xffffffff);
                    if (4 < local_a0) {
                      local_19c = local_a0 + -3;
                    }
                    FUN_00013490((double)(long)local_19c);
                    local_2d0 = FUN_00015a7c(&local_1b0,auStack_2d8);
                    local_198 = local_2d0;
                    local_2f0 = FUN_000157c8(&local_1e0,&local_198);
                    local_2e8 = FUN_00015abc(&local_2f0);
                    local_2e0 = FUN_000157c8(&local_1e0,&local_2e8);
                    local_198 = local_2e0;
                    local_300 = FUN_000157c8(&local_1d0,&local_1e0);
                    local_2f8 = FUN_00015a7c(&local_198,&local_300);
                    local_190 = local_2f8;
                    sVar2 = FUN_00014140(&local_198,&local_1e0);
                    if (sVar2 != 0) {
                      local_48 = 2;
                      goto LAB_0010898c;
                    }
                  }
                }
                else {
                  __ZN8PMString6AppendEPKciNS_14StringEncodingE
                            (auStack_140," Division",0x7fffffff,0xffffffff);
                  local_19c = local_a0 + 1;
                  local_2a0 = FUN_000157c8(&local_1e0,&local_1b0);
                  local_298 = FUN_00015abc(&local_2a0);
                  local_290 = FUN_000157c8(&local_1e0,&local_298);
                  local_198 = local_290;
                  local_2c0 = FUN_000157c8(&local_1e0,&local_1b0);
                  local_2b8 = FUN_00015abc(&local_2c0);
                  local_2b0 = FUN_000157c8(&local_1e0,&local_2b8);
                  local_2c8 = FUN_000157c8(&local_1d0,&local_1e0);
                  local_2a8 = FUN_00015a7c(&local_2b0,&local_2c8);
                  local_190 = local_2a8;
                }
                plVar5 = (long *)FUN_000029d8(auStack_50);
                (**(code **)(*plVar5 + 0xb0))(plVar5,&local_19c);
                plVar5 = (long *)FUN_000029d8(auStack_50);
                (**(code **)(*plVar5 + 0xe0))(plVar5,&local_19c);
                if (local_1a0 == 0) {
                  local_198 = local_1b0;
                }
                else {
                  local_310 = FUN_000157c8(&local_1e0,&local_198);
                  local_308 = FUN_00015abc(&local_310);
                  FUN_00002cf8(auStack_35c,0x15d32f);
                  FUN_000138a8(auStack_358,auStack_30,auStack_35c);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                            (auStack_3a8,"0x15d300kGCCalculateKey",0);
                  sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_358,auStack_3a8,1,0);
                  __ZN8PMStringD1Ev(auStack_3a8);
                  __ZN8PMStringD1Ev(auStack_358);
                  if (sVar2 != 0) {
                    plVar5 = (long *)FUN_000029d8(auStack_50);
                    (**(code **)(*plVar5 + 0xa0))(plVar5,&local_190);
                    plVar5 = (long *)FUN_000029d8(auStack_50);
                    local_3c8 = (**(code **)(*plVar5 + 0x98))();
                    local_3c0 = FUN_000157c8(&local_1d0,&local_3c8);
                    local_3b8 = FUN_00015abc(&local_3c0);
                    plVar5 = (long *)FUN_000029d8(auStack_50);
                    iVar3 = (**(code **)(*plVar5 + 0xa8))();
                    FUN_00013490((double)iVar3);
                    local_3b0 = FUN_00015a7c(&local_3b8,auStack_3d0);
                    plVar5 = (long *)FUN_000029d8(auStack_50);
                    local_3d4 = FUN_00032070(&local_3b0);
                    (**(code **)(*plVar5 + 0xf0))(plVar5,&local_3d4);
                    local_19e = 1;
                  }
                  local_3e0 = local_198;
                  local_3e4 = 3;
                  FUN_000152f0(&local_3e0,&local_3e4);
                  plVar5 = (long *)FUN_000029d8(auStack_50);
                  (**(code **)(*plVar5 + 0xd0))(plVar5,&local_3e0);
                  plVar5 = (long *)FUN_000029d8(auStack_50);
                  local_3e8 = FUN_00032070(&local_308);
                  (**(code **)(*plVar5 + 0x100))(plVar5,&local_3e8);
                  plVar5 = (long *)FUN_0000e2e0(auStack_88);
                  (**(code **)(*plVar5 + 0x28))(plVar5,&local_3e0);
                }
                FUN_001ad93c(auStack_30);
                FUN_00015c98(auStack_30,&local_198);
                local_3ec = 3;
                FUN_000152f0(&local_198,&local_3ec);
                FUN_00002cf8(auStack_3f0,0x15d31d);
                FUN_00013c20(auStack_30,auStack_3f0,&local_198);
                FUN_001cd3e4(auStack_30);
                FUN_00167d6c(param_1,&DAT_00208c52);
                FUN_0014b630(param_1,&DAT_00208c50);
                FUN_00002cf8(auStack_3f4,0x15d49a);
                FUN_00013490(0);
                FUN_00013c20(auStack_30,auStack_3f4,auStack_400);
                FUN_00002cf8(auStack_404,0x15d49c);
                FUN_00013490(0);
                FUN_00013c20(auStack_30,auStack_404,auStack_410);
                FUN_00002cf8(auStack_414,0x15d49a);
                FUN_00129a38(param_1,auStack_414,&DAT_00208c50,&DAT_00208c52);
                FUN_00002cf8(auStack_418,0x15d49c);
                FUN_00129a38(param_1,auStack_418,&DAT_00208c50,&DAT_00208c52);
                if (local_19e != 0) {
                  FUN_00167d6c(param_1,&DAT_00208c50,&DAT_00208c52);
                  FUN_00002cf8(auStack_41c,0x15d49e);
                  FUN_00013490(0);
                  FUN_00013c20(auStack_30,auStack_41c,auStack_428);
                  FUN_00002cf8(auStack_42c,0x15d4a0);
                  FUN_00013490(0);
                  FUN_00013c20(auStack_30,auStack_42c,auStack_438);
                  FUN_00002cf8(auStack_43c,0x15d49e);
                  FUN_0012dd50(param_1,auStack_43c,&DAT_00208c50);
                  FUN_00002cf8(auStack_440,0x15d4a0);
                  FUN_0012dd50(param_1,auStack_440,&DAT_00208c50);
                }
                sVar2 = FUN_001a6604(auStack_140);
                if ((sVar2 == 0) && (0 < local_a0)) {
                  local_444 = FUN_001a6844(auStack_140,&DAT_00208c50);
                  if (local_444 != 0) {
                    local_48 = 2;
                    goto LAB_0010898c;
                  }
                }
                FUN_00002cf8(auStack_494,0x15d30a);
                FUN_000138a8(auStack_490,auStack_30,auStack_494);
                if (local_a0 == 0) {
                  local_4e9 = 0;
                  local_539 = 0;
                  FUN_00002cf8(auStack_49c,0x15d4a3);
                  sVar2 = FUN_00012ef4(auStack_30,auStack_49c);
                  bVar1 = false;
                  if (sVar2 == 0) {
                    FUN_00002cf8(auStack_4a0,0x15d4a4);
                    sVar2 = FUN_00012ef4(auStack_30,auStack_4a0);
                    bVar1 = false;
                    if (sVar2 == 0) {
                      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                                (auStack_4e8,"0x15d300kGCHideGridKey",0);
                      local_4e9 = 1;
                      sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_490,auStack_4e8,1,0);
                      bVar1 = true;
                      if (sVar2 == 0) {
                        __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                                  (auStack_538,"0x15d300kGCBaselineKey",0);
                        local_539 = 1;
                        sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_490,auStack_538,1,0);
                        bVar1 = sVar2 != 0;
                      }
                    }
                  }
                  if ((local_539 & 1) != 0) {
                    __ZN8PMStringD1Ev(auStack_538);
                  }
                  if ((local_4e9 & 1) != 0) {
                    __ZN8PMStringD1Ev(auStack_4e8);
                  }
                  if (bVar1) {
                    FUN_001ae290(auStack_30,&local_198,&DAT_00208c50);
                    plVar5 = (long *)FUN_000029d8(auStack_50);
                    (**(code **)(*plVar5 + 0x110))(plVar5,&DAT_00208c50);
                    plVar5 = (long *)FUN_000029d8(auStack_50);
                    (**(code **)(*plVar5 + 0x120))(plVar5,&DAT_00208c50);
                  }
                }
                else {
                  FUN_001ae290(auStack_30,&local_198,&DAT_00208c52);
                  plVar5 = (long *)FUN_000029d8(auStack_50);
                  plVar6 = (long *)FUN_0000e2e0(auStack_88);
                  local_496 = (**(code **)(*plVar6 + 0x20))();
                  (**(code **)(*plVar5 + 0x110))(plVar5,&local_496);
                }
                FUN_00138008(param_1);
                FUN_00002cf8(auStack_58c,0x15d324);
                FUN_000138a8(auStack_588,auStack_30,auStack_58c);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_5d8,"0x15d300kGCUnDoKey",0);
                sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_588,auStack_5d8,1,0);
                __ZN8PMStringD1Ev(auStack_5d8);
                __ZN8PMStringD1Ev(auStack_588);
                if (sVar2 != 0) {
                  FUN_0012335c(param_1);
                  FUN_0012335c(param_1);
                }
                __ZN8PMStringD1Ev(auStack_490);
                local_48 = 0;
              }
LAB_0010898c:
              __ZN8PMStringD1Ev(auStack_230);
              __ZN8PMStringD1Ev(auStack_188);
              __ZN8PMStringD1Ev(auStack_140);
              __ZN8PMStringD1Ev(auStack_f0);
            }
            else {
              local_48 = 2;
            }
            FUN_0000e040(auStack_98);
          }
          else {
            local_48 = 2;
          }
          FUN_0000e2f8(auStack_90);
        }
        else {
          local_48 = 2;
        }
        FUN_0000e324(auStack_88);
      }
      else {
        local_48 = 2;
      }
      FUN_00017158(auStack_80);
    }
    else {
      local_48 = 2;
    }
    FUN_0000e06c(auStack_70);
  }
  else {
    local_48 = 2;
  }
  FUN_00002b9c(auStack_50);
LAB_001089f8:
  FUN_00002c54(auStack_30);
  return;
}
