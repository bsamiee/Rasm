/*
Grid Width, Calculate (row 04). FUN_00110ffc: M_h = Round(W / g_desired), u_h = W / M_h, M_h * subdiv_h sent to the document grid.

PMReal helpers: construct 0x13490, operator/ 0x157c8, operator* 0x15a7c, operator+ 0x17c4c, operator- 0x28080,
Round 0x15abc (floor(x + 0.5)), floor 0x37d70, a > b + 1e-8 0x14140, a >= b - 1e-8 0x15808, a <= b + 1e-8 0x31d38,
a < b - 1e-8 0x3208c, |a - b| < 1e-8 0x132fc, not equal 0x18504, quantise 0x152f0 (mode 3 three decimals, mode 4 four).
Widget access: read value 0x13338, read value in the panel unit 0x1544c, read checkbox 0x12ef4, read dropdown index 0x13034,
write value 0x13c20, set maximum 0x196d0, unit factor at object + 0x88.
*/

//==== FUNC @ 1116b4 -> 00110ffc

void FUN_00110ffc(long param_1,short *param_2)

{
  bool bVar1;
  short sVar2;
  int iVar3;
  undefined8 uVar4;
  long *plVar5;
  undefined1 auStack_4b8 [4];
  undefined1 auStack_4b4 [4];
  undefined1 auStack_4b0 [12];
  undefined1 auStack_4a4 [4];
  undefined1 auStack_4a0 [8];
  undefined1 auStack_498 [4];
  undefined4 local_494;
  undefined1 auStack_490 [8];
  undefined8 local_488;
  undefined8 local_480;
  undefined8 local_478;
  undefined8 local_470;
  undefined8 local_468;
  undefined1 auStack_45c [4];
  undefined8 local_458;
  undefined8 local_450;
  undefined8 local_448;
  undefined8 local_440;
  undefined1 auStack_438 [8];
  undefined8 local_430;
  undefined8 local_428;
  undefined8 local_420;
  undefined8 local_418;
  undefined8 local_410;
  undefined1 auStack_408 [8];
  undefined1 auStack_400 [12];
  undefined1 auStack_3f4 [4];
  undefined1 auStack_3f0 [8];
  undefined1 auStack_3e8 [72];
  undefined1 auStack_3a0 [4];
  undefined1 auStack_39c [4];
  undefined8 local_398;
  undefined8 local_390;
  undefined8 local_388;
  undefined1 auStack_380 [76];
  undefined1 auStack_334 [4];
  int local_330;
  undefined1 auStack_32c [4];
  undefined8 local_328;
  undefined1 auStack_320 [72];
  undefined1 auStack_2d8 [76];
  undefined1 auStack_28c [4];
  undefined1 auStack_288 [72];
  undefined1 auStack_240 [72];
  undefined1 auStack_1f8 [76];
  undefined1 auStack_1ac [4];
  undefined1 auStack_1a8 [76];
  undefined1 auStack_15c [4];
  undefined8 local_158;
  undefined1 auStack_14c [4];
  undefined8 local_148;
  undefined1 auStack_13c [6];
  short local_136;
  undefined1 auStack_134 [6];
  short local_12e;
  undefined1 auStack_12c [6];
  short local_126;
  undefined1 auStack_124 [4];
  undefined8 local_120;
  undefined1 auStack_114 [4];
  undefined8 local_110;
  undefined1 auStack_104 [4];
  undefined8 local_100;
  undefined1 auStack_f4 [4];
  undefined8 local_f0;
  undefined4 local_e8;
  undefined1 auStack_e4 [4];
  undefined1 auStack_e0 [72];
  undefined8 local_98;
  undefined1 uStack_89;
  undefined1 auStack_88 [15];
  undefined1 uStack_79;
  undefined1 local_78 [16];
  undefined1 auStack_68 [15];
  undefined1 uStack_59;
  undefined1 auStack_58 [8];
  int local_50;
  undefined1 uStack_39;
  undefined1 auStack_38 [8];
  short *local_30;
  long local_28;
  
  local_30 = param_2;
  local_28 = param_1;
  FUN_00002bf4(auStack_38,param_1,&uStack_39);
  sVar2 = FUN_00002c30(auStack_38);
  if (sVar2 != 0) {
    local_50 = 2;
    goto LAB_00111d34;
  }
  uVar4 = __Z26GetExecutionContextSessionv();
  FUN_00002978(auStack_58,uVar4,&uStack_59);
  sVar2 = FUN_000029b4(auStack_58);
  if (sVar2 == 0) {
    plVar5 = (long *)FUN_000029d8(auStack_58);
    local_78 = (**(code **)(*plVar5 + 0x18))();
    FUN_0000df20(auStack_68,local_78,&uStack_79);
    sVar2 = FUN_0000df5c(auStack_68);
    if (sVar2 == 0) {
      uVar4 = FUN_0000df80(auStack_68);
      FUN_0000df98(auStack_88,uVar4,&uStack_89);
      sVar2 = FUN_0000dfd4(auStack_88);
      if (sVar2 == 0) {
        FUN_00013490(0,&local_98);
        FUN_00002cf8(auStack_e4,0x15d306);
        local_e8 = 0xffffffff;
        FUN_00012c10(auStack_e0,auStack_38,auStack_e4,&local_e8);
        FUN_00002cf8(auStack_f4,0x15d32e);
        local_f0 = FUN_0001544c(auStack_38,auStack_f4,auStack_e0);
        FUN_00002cf8(auStack_104,&DAT_0015d31a);
        local_100 = FUN_0001544c(auStack_38,auStack_104,auStack_e0);
        FUN_00002cf8(auStack_114,&DAT_0015d31b);
        local_110 = FUN_0001544c(auStack_38,auStack_114,auStack_e0);
        FUN_00002cf8(auStack_124,0x15d4a9);
        local_120 = FUN_0001544c(auStack_38,auStack_124,auStack_e0);
        FUN_00002cf8(auStack_12c,0x15d4a3);
        local_126 = FUN_00012ef4(auStack_38,auStack_12c);
        FUN_00002cf8(auStack_134,0x15d4a4);
        local_12e = FUN_00012ef4(auStack_38,auStack_134);
        FUN_00002cf8(auStack_13c,0x15d323);
        local_136 = FUN_00012ef4(auStack_38,auStack_13c);
        FUN_00002cf8(auStack_14c,0x15d31c);
        local_148 = FUN_00013338(auStack_38,auStack_14c);
        FUN_00002cf8(auStack_15c,0x15d31d);
        local_158 = FUN_00013338(auStack_38,auStack_15c);
        FUN_00002cf8(auStack_1ac,0x15d32f);
        FUN_000138a8(auStack_1a8,auStack_38,auStack_1ac);
        __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1f8,"[GC] Desired Grid Width: ",0);
        FUN_001cc9ac(auStack_240,auStack_1f8);
        FUN_001b4670(auStack_240);
        __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_288,"0x15d300kGCCalculateKey",0);
        sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_1a8,auStack_288,1,0);
        __ZN8PMStringD1Ev(auStack_288);
        if (sVar2 == 0) {
          FUN_00002cf8(auStack_39c,0x15d324);
          FUN_00011dc4(auStack_38,auStack_39c,&DAT_00208c52);
          FUN_00002cf8(auStack_3a0,0x15d32f);
          __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_3e8,"0x15d300kGCCalculateKey",0);
          FUN_00013588(auStack_38,auStack_3a0,auStack_3e8);
          __ZN8PMStringD1Ev(auStack_3e8);
          FUN_00013490(0);
          sVar2 = FUN_000132fc(&local_f0,auStack_3f0);
          if (sVar2 != 0) {
            FUN_00002cf8(auStack_3f4,0x15d32f);
            FUN_00011dc4(auStack_38,auStack_3f4,&DAT_00208c50);
          }
          FUN_00013490(0);
          sVar2 = FUN_000132fc(&local_158,auStack_400);
          bVar1 = true;
          if (sVar2 == 0) {
            FUN_00013490(0);
            sVar2 = FUN_000132fc(&local_148,auStack_408);
            bVar1 = sVar2 != 0;
          }
          if (!bVar1) {
            if (local_136 == 0) {
              local_428 = FUN_000157c8(&local_110,&local_148);
              local_420 = FUN_00015abc(&local_428);
              local_418 = FUN_000157c8(&local_110,&local_420);
              local_430 = FUN_000157c8(&local_100,&local_110);
              local_410 = FUN_00015a7c(&local_418,&local_430);
              local_98 = local_410;
            }
            else {
              bVar1 = false;
              if (local_126 != 0) {
                FUN_00013490(0);
                sVar2 = FUN_00014140(&local_120,auStack_438);
                bVar1 = sVar2 != 0;
              }
              if (bVar1) {
                local_448 = FUN_000157c8(&local_100,&local_120);
                local_440 = FUN_00015a7c(&local_158,&local_448);
                local_98 = local_440;
              }
              else {
                local_458 = FUN_000157c8(&local_100,&local_110);
                local_450 = FUN_00015a7c(&local_158,&local_458);
                local_98 = local_450;
              }
            }
            goto LAB_00111a30;
          }
          local_50 = 2;
        }
        else {
          FUN_00002cf8(auStack_28c,0x15d324);
          FUN_00011dc4(auStack_38,auStack_28c,&DAT_00208c50);
          __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                    (auStack_2d8,
                     "If you are trying to match the document grid width to the document leading, work in unit: Points and Pixels. This will avoid any kind of confusion between different values and units. Leading is always shown in points and the Grid Width is always shown in the selected unit. To apply your entered leading value, uncheck Fit Leading."
                     ,0);
          if (*local_30 != 0) {
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_320,"0x15d300kGCDialogTitleKey",0);
            __ZN6CAlert29WarningAlertWithDontShowAgainERK8PMStringihiS2_S2_S2_s
                      (auStack_2d8,0x15d383,0,1,auStack_320,PTR__kNullString_00234110);
            __ZN8PMStringD1Ev(auStack_320);
          }
          FUN_00002cf8(auStack_32c,0x15d32e);
          local_328 = FUN_00013338(auStack_38,auStack_32c);
          __ZN8PMString12AppendNumberERK6PMRealiss(auStack_1f8,&local_328,0xffffffff,0);
          sVar2 = FUN_001a6604(auStack_1f8);
          if (sVar2 == 0) {
            local_330 = FUN_001a6844(auStack_1f8,&DAT_00208c50);
            if (local_330 == 0) goto LAB_0011166c;
            local_50 = 2;
          }
          else {
LAB_0011166c:
            FUN_00002cf8(auStack_334,0x15d32f);
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_380,"0x15d300kGCUnDoKey",0);
            FUN_00013588(auStack_38,auStack_334,auStack_380);
            __ZN8PMStringD1Ev(auStack_380);
            local_390 = FUN_000157c8(&local_100,&local_f0);
            local_388 = FUN_00015abc(&local_390);
            local_398 = FUN_000157c8(&local_100,&local_388);
            local_50 = 0;
            local_98 = local_398;
          }
          __ZN8PMStringD1Ev(auStack_2d8);
          if (local_50 == 0) {
LAB_00111a30:
            FUN_00002cf8(auStack_45c,0x15d330);
            local_468 = FUN_00015a7c(&local_98,param_1 + 0x88);
            FUN_00013c20(auStack_38,auStack_45c,&local_468);
            plVar5 = (long *)FUN_000029d8(auStack_58);
            (**(code **)(*plVar5 + 0xa0))(plVar5,&local_98);
            plVar5 = (long *)FUN_000029d8(auStack_58);
            local_488 = (**(code **)(*plVar5 + 0x98))();
            local_480 = FUN_000157c8(&local_100,&local_488);
            local_478 = FUN_00015abc(&local_480);
            plVar5 = (long *)FUN_000029d8(auStack_58);
            iVar3 = (**(code **)(*plVar5 + 0xa8))();
            FUN_00013490((double)iVar3);
            local_470 = FUN_00015a7c(&local_478,auStack_490);
            plVar5 = (long *)FUN_000029d8(auStack_58);
            local_494 = FUN_00032070(&local_470);
            (**(code **)(*plVar5 + 0xf0))(plVar5,&local_494);
            FUN_001ad93c(auStack_38);
            if (local_12e == 0) {
              FUN_00167d6c(param_1,&DAT_00208c50,&DAT_00208c52);
              FUN_00002cf8(auStack_498,0x15d49e);
              FUN_00013490(0);
              FUN_00013c20(auStack_38,auStack_498,auStack_4a0);
              FUN_00002cf8(auStack_4a4,0x15d4a0);
              FUN_00013490(0);
              FUN_00013c20(auStack_38,auStack_4a4,auStack_4b0);
              FUN_00002cf8(auStack_4b4,0x15d49e);
              FUN_0012dd50(param_1,auStack_4b4,&DAT_00208c50);
              FUN_00002cf8(auStack_4b8,0x15d4a0);
              FUN_0012dd50(param_1,auStack_4b8,&DAT_00208c50);
            }
            if ((local_126 != 0) && (local_12e == 0)) {
              FUN_001b5050(auStack_38);
            }
            FUN_00138008(param_1);
            local_50 = 0;
          }
        }
        __ZN8PMStringD1Ev(auStack_240);
        __ZN8PMStringD1Ev(auStack_1f8);
        __ZN8PMStringD1Ev(auStack_1a8);
        __ZN8PMStringD1Ev(auStack_e0);
      }
      else {
        local_50 = 2;
      }
      FUN_0000e040(auStack_88);
    }
    else {
      local_50 = 2;
    }
    FUN_0000e06c(auStack_68);
  }
  else {
    local_50 = 2;
  }
  FUN_00002b9c(auStack_58);
LAB_00111d34:
  FUN_00002c54(auStack_38);
  return;
}
