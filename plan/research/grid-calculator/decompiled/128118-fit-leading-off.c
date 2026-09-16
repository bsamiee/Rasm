/*
Fit Leading off (row 15). FUN_00128118 runs when the checkbox is off: L_fit = L (quantised, 0x152f0), N = Round(H / L),
N * L > H drops one line, r = H - N * L is stored through the model setter at vtable + 0x130 and shown as the bottom margin,
the horizontal unit still uses Fit(H, L) * W / H. FUN_00014a5c lists exact multiples L * k in the subdivision dropdown when the
checkbox is off. FUN_00163ee0 writes the applied margins: top = k_t * L (+ x with image-lines), bottom = k_b * L + r, the
remainder read back through vtable + 0x128.

PMReal helpers: construct 0x13490, operator/ 0x157c8, operator* 0x15a7c, operator+ 0x17c4c, operator- 0x28080,
Round 0x15abc (floor(x + 0.5)), floor 0x37d70, a > b + 1e-8 0x14140, a >= b - 1e-8 0x15808, a <= b + 1e-8 0x31d38,
a < b - 1e-8 0x3208c, |a - b| < 1e-8 0x132fc, not equal 0x18504, quantise 0x152f0 (mode 3 three decimals, mode 4 four).
Widget access: read value 0x13338, read value in the panel unit 0x1544c, read checkbox 0x12ef4, read dropdown index 0x13034,
write value 0x13c20, set maximum 0x196d0, unit factor at object + 0x88.
*/

//==== FUNC @ 128118 -> 00128118

void FUN_00128118(long param_1)

{
  bool bVar1;
  short sVar2;
  int iVar3;
  undefined8 uVar4;
  long *plVar5;
  undefined4 local_6bc;
  undefined1 auStack_6b8 [4];
  undefined4 local_6b4;
  undefined1 auStack_6b0 [4];
  undefined4 local_6ac;
  undefined1 auStack_6a8 [4];
  undefined4 local_6a4;
  undefined1 auStack_6a0 [4];
  undefined1 auStack_69c [4];
  int local_698;
  undefined1 auStack_694 [4];
  undefined1 auStack_690 [4];
  undefined4 local_68c;
  undefined1 auStack_688 [4];
  undefined4 local_684;
  undefined1 auStack_680 [4];
  undefined1 auStack_67c [4];
  undefined1 auStack_678 [8];
  undefined8 local_670;
  undefined1 auStack_668 [72];
  undefined1 auStack_620 [8];
  undefined8 local_618;
  undefined1 auStack_610 [72];
  undefined8 local_5c8;
  undefined1 auStack_5c0 [8];
  undefined1 auStack_5b8 [4];
  undefined4 local_5b4;
  undefined4 local_5b0;
  undefined4 local_5ac;
  undefined4 local_5a8;
  undefined4 local_5a4;
  undefined1 auStack_5a0 [4];
  undefined1 auStack_59c [4];
  undefined1 auStack_598 [4];
  undefined1 auStack_594 [4];
  undefined4 local_590;
  undefined1 auStack_58c [4];
  undefined4 local_588;
  undefined1 auStack_584 [4];
  undefined8 local_580;
  undefined1 auStack_578 [4];
  undefined4 local_574;
  undefined8 local_570;
  undefined8 local_568;
  undefined4 local_55c;
  undefined8 local_558;
  undefined8 local_550;
  undefined8 local_548;
  undefined8 local_540;
  undefined8 local_538;
  undefined1 auStack_530 [76];
  undefined1 auStack_4e4 [4];
  undefined1 auStack_4e0 [72];
  undefined8 local_498;
  undefined8 local_490;
  undefined8 local_488;
  undefined8 local_480;
  undefined8 local_478;
  undefined8 local_470;
  undefined8 local_468;
  undefined8 local_460;
  undefined8 local_458;
  undefined8 local_450;
  undefined8 local_448;
  undefined1 auStack_440 [4];
  undefined1 auStack_43c [4];
  undefined1 auStack_438 [72];
  undefined4 local_3f0;
  undefined1 auStack_3ec [4];
  undefined1 auStack_3e8 [72];
  undefined1 auStack_3a0 [76];
  undefined1 auStack_354 [4];
  undefined1 auStack_350 [72];
  undefined1 auStack_308 [4];
  undefined4 local_304;
  undefined1 auStack_300 [12];
  undefined1 auStack_2f4 [4];
  undefined8 local_2f0;
  undefined1 auStack_2e8 [4];
  undefined1 auStack_2e4 [4];
  undefined1 auStack_2e0 [12];
  undefined1 auStack_2d4 [4];
  undefined1 auStack_2d0 [12];
  undefined1 auStack_2c4 [4];
  undefined1 auStack_2c0 [4];
  undefined1 auStack_2bc [4];
  undefined1 auStack_2b8 [4];
  undefined1 auStack_2b4 [4];
  undefined1 auStack_2b0 [4];
  undefined1 auStack_2ac [4];
  undefined1 auStack_2a8 [8];
  undefined1 auStack_2a0 [8];
  undefined8 local_298;
  undefined1 auStack_290 [8];
  undefined1 auStack_288 [8];
  undefined1 auStack_280 [32];
  undefined1 auStack_260 [76];
  undefined1 auStack_214 [4];
  undefined1 auStack_210 [72];
  undefined4 local_1c8;
  undefined1 auStack_1c4 [4];
  undefined1 auStack_1c0 [8];
  undefined1 auStack_1b8 [72];
  undefined1 auStack_170 [4];
  undefined1 auStack_16c [4];
  undefined8 local_168;
  undefined1 auStack_15c [4];
  undefined8 local_158;
  undefined4 local_150;
  undefined1 auStack_14c [4];
  undefined1 auStack_148 [72];
  undefined1 auStack_100 [6];
  short local_fa;
  undefined1 auStack_f8 [6];
  short local_f2;
  undefined1 auStack_f0 [6];
  short local_ea;
  undefined1 auStack_e8 [72];
  undefined1 auStack_a0 [8];
  undefined1 auStack_98 [8];
  undefined1 auStack_90 [15];
  undefined1 uStack_81;
  undefined1 auStack_80 [15];
  undefined1 uStack_71;
  undefined1 auStack_70 [8];
  undefined1 local_68 [16];
  undefined1 uStack_51;
  undefined1 auStack_50 [8];
  undefined4 local_48;
  undefined1 uStack_31;
  undefined1 auStack_30 [8];
  long local_28;
  
  local_28 = param_1;
  FUN_00002bf4(auStack_30,param_1,&uStack_31);
  sVar2 = FUN_00002c30(auStack_30);
  if (sVar2 != 0) {
    local_48 = 2;
    goto LAB_0012998c;
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
      uVar4 = FUN_0000df80(auStack_70);
      FUN_0000df98(auStack_80,uVar4,&uStack_81);
      sVar2 = FUN_0000dfd4(auStack_80);
      if (sVar2 == 0) {
        FUN_001a7968(auStack_90);
        sVar2 = FUN_00016b78(auStack_90);
        if (sVar2 == 0) {
          FUN_001a2484(auStack_98);
          sVar2 = FUN_0000e2a4(auStack_98);
          if (sVar2 == 0) {
            FUN_001a7c08(auStack_a0);
            sVar2 = FUN_0000e280(auStack_a0);
            if (sVar2 == 0) {
              __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_e8,"[GC] Set Leading",0);
              FUN_00002cf8(auStack_f0,0x15d4a3);
              local_ea = FUN_00012ef4(auStack_30,auStack_f0);
              FUN_00002cf8(auStack_f8,0x15d4a4);
              local_f2 = FUN_00012ef4(auStack_30,auStack_f8);
              FUN_00002cf8(auStack_100,0x15d323);
              local_fa = FUN_00012ef4(auStack_30,auStack_100);
              FUN_00002cf8(auStack_14c,0x15d306);
              local_150 = 0xffffffff;
              FUN_00012c10(auStack_148,auStack_30,auStack_14c,&local_150);
              FUN_00002cf8(auStack_15c,&DAT_0015d31a);
              local_158 = FUN_0001544c(auStack_30,auStack_15c,auStack_148);
              FUN_00002cf8(auStack_16c,&DAT_0015d31b);
              local_168 = FUN_0001544c(auStack_30,auStack_16c,auStack_148);
              FUN_00002cf8(auStack_170,0x15d4e6);
              __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1b8,"",0);
              FUN_00013588(auStack_30,auStack_170,auStack_1b8);
              __ZN8PMStringD1Ev(auStack_1b8);
              plVar5 = (long *)FUN_000029d8(auStack_50);
              FUN_00013490(0);
              (**(code **)(*plVar5 + 0x130))(plVar5,auStack_1c0);
              FUN_00002cf8(auStack_1c4,0x15d326);
              local_1c8 = 0;
              FUN_00015b04(auStack_30,auStack_1c4,&local_1c8,1);
              FUN_0010772c(param_1);
              FUN_00002cf8(auStack_214,0x15d324);
              FUN_000138a8(auStack_210,auStack_30,auStack_214);
              __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_260,"0x15d300kGCUnDoKey",0);
              sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_210,auStack_260,1,0);
              __ZN8PMStringD1Ev(auStack_260);
              __ZN8PMStringD1Ev(auStack_210);
              if (sVar2 != 0) {
                FUN_0012335c(param_1);
              }
              if (local_fa == 0) {
                FUN_0001b96c(auStack_30);
                plVar5 = (long *)FUN_000029d8(auStack_50);
                sVar2 = (**(code **)(*plVar5 + 0x58))();
                if (sVar2 == 0) {
LAB_001287c0:
                  FUN_00013490();
                  FUN_00013490(0);
                  __ZN6PMRectC1ERK6PMRealS2_S2_S2_
                            (auStack_280,auStack_288,auStack_290,&local_158,&local_168);
                  FUN_001b0acc(local_68,auStack_280);
                }
                else {
                  plVar5 = (long *)FUN_000029d8(auStack_50);
                  sVar2 = (**(code **)(*plVar5 + 0x68))();
                  if (sVar2 == 0) goto LAB_001287c0;
                }
                plVar5 = (long *)FUN_000029d8(auStack_50);
                local_298 = (**(code **)(*plVar5 + 0x1d8))();
                FUN_00013490(0);
                sVar2 = FUN_00014140(&local_298,auStack_2a0);
                if (sVar2 != 0 || local_ea != 0) {
                  plVar5 = (long *)FUN_000029d8(auStack_50);
                  FUN_00013490(0);
                  (**(code **)(*plVar5 + 0x1e0))(plVar5,auStack_2a8);
                  FUN_00002cf8(auStack_2ac,0x15d4c5);
                  FUN_00013178(auStack_30,auStack_2ac,&DAT_00208c50);
                  FUN_00002cf8(auStack_2b0,0x15d4c5);
                  FUN_00011dc4(auStack_30,auStack_2b0,&DAT_00208c50);
                  FUN_00002cf8(auStack_2b4,0x15d49a);
                  FUN_00011dc4(auStack_30,auStack_2b4,&DAT_00208c52);
                  FUN_00002cf8(auStack_2b8,0x15d49c);
                  FUN_00011dc4(auStack_30,auStack_2b8,&DAT_00208c52);
                  FUN_00002cf8(auStack_2bc,0x15d49b);
                  FUN_00011dc4(auStack_30,auStack_2bc,&DAT_00208c50);
                  FUN_00002cf8(auStack_2c0,0x15d49d);
                  FUN_00011dc4(auStack_30,auStack_2c0,&DAT_00208c50);
                  FUN_00002cf8(auStack_2c4,0x15d49a);
                  FUN_00013490(0);
                  FUN_00013c20(auStack_30,auStack_2c4,auStack_2d0);
                  FUN_00002cf8(auStack_2d4,0x15d49c);
                  FUN_00013490(0);
                  FUN_00013c20(auStack_30,auStack_2d4,auStack_2e0);
                }
                FUN_00167d6c(param_1,&DAT_00208c52);
                FUN_0014b630(param_1,&DAT_00208c50);
                FUN_001b5050(auStack_30);
                if (local_ea != 0) {
                  FUN_00002cf8(auStack_2e4,0x15d4a3);
                  FUN_00013178(auStack_30,auStack_2e4,&DAT_00208c50);
                  FUN_00002cf8(auStack_2e8,0x15d4a3);
                  FUN_0013bc94(param_1,auStack_2e8);
                }
                FUN_00002cf8(auStack_2f4,0x15d31c);
                local_2f0 = FUN_00013338(auStack_30,auStack_2f4);
                FUN_00013490(0);
                sVar2 = FUN_00018504(&local_2f0,auStack_300);
                if (sVar2 == 0) {
                  FUN_000e8428(param_1);
                  goto LAB_00129868;
                }
                FUN_00015c98(auStack_30,&local_2f0);
                local_304 = 3;
                FUN_000152f0(&local_2f0,&local_304);
                FUN_00002cf8(auStack_308,0x15d31d);
                FUN_00013c20(auStack_30,auStack_308,&local_2f0);
                FUN_00002cf8(auStack_354,0x15d3b3);
                FUN_000138a8(auStack_350,auStack_30,auStack_354);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_3a0,"0x15d300kGCLockKey",0);
                sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_350,auStack_3a0,1,0);
                __ZN8PMStringD1Ev(auStack_3a0);
                __ZN8PMStringD1Ev(auStack_350);
                if (sVar2 != 0) {
                  FUN_00002cf8(auStack_3ec,0x15d309);
                  local_3f0 = 0xffffffff;
                  FUN_00012c10(auStack_3e8,auStack_30,auStack_3ec,&local_3f0);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_438,"[GC-1]",0);
                  sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_3e8,auStack_438,1,0);
                  bVar1 = local_ea != 0;
                  __ZN8PMStringD1Ev(auStack_438);
                  if (sVar2 == 0 && bVar1) {
                    FUN_00002cf8(auStack_440,0x15d3a9);
                    FUN_00011dc4(auStack_30,auStack_440,&DAT_00208c50);
                  }
                  else {
                    FUN_00002cf8(auStack_43c,0x15d3a9);
                    FUN_00011dc4(auStack_30,auStack_43c,&DAT_00208c52);
                  }
                  __ZN8PMStringD1Ev(auStack_3e8);
                }
                local_450 = FUN_000157c8(&local_168,&local_2f0);
                local_448 = FUN_00015abc(&local_450);
                local_458 = FUN_00015a7c(&local_2f0,&local_448);
                FUN_00013490(0,&local_460);
                sVar2 = FUN_00014140(&local_458,&local_168);
                if (sVar2 != 0) {
                  local_468 = FUN_00028080(&local_458,&local_2f0);
                  local_458 = local_468;
                }
                sVar2 = FUN_00014140(&local_168,&local_458);
                if (sVar2 == 0) {
                  FUN_00013490(0,&local_478);
                  local_460 = local_478;
                }
                else {
                  local_470 = FUN_00028080(&local_168,&local_458);
                  local_460 = local_470;
                }
                local_480 = local_460;
                local_498 = FUN_00028080(&local_168,&local_480);
                local_490 = FUN_000157c8(&local_498,&local_2f0);
                local_488 = FUN_00015abc(&local_490);
                FUN_00002cf8(auStack_4e4,0x15d32f);
                FUN_000138a8(auStack_4e0,auStack_30,auStack_4e4);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                          (auStack_530,"0x15d300kGCCalculateKey",0);
                sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_4e0,auStack_530,1,0);
                __ZN8PMStringD1Ev(auStack_530);
                __ZN8PMStringD1Ev(auStack_4e0);
                if (sVar2 != 0) {
                  FUN_00167d6c(param_1,&DAT_00208c50,&DAT_00208c52);
                  local_550 = FUN_000157c8(&local_168,&local_2f0);
                  local_548 = FUN_00015abc(&local_550);
                  local_540 = FUN_000157c8(&local_168,&local_548);
                  local_558 = FUN_000157c8(&local_158,&local_168);
                  local_538 = FUN_00015a7c(&local_540,&local_558);
                  local_55c = 3;
                  FUN_000152f0(&local_538,&local_55c);
                  plVar5 = (long *)FUN_000029d8(auStack_50);
                  (**(code **)(*plVar5 + 0xa0))(plVar5,&local_538);
                  plVar5 = (long *)FUN_00017c8c(auStack_90);
                  (**(code **)(*plVar5 + 0x68))(plVar5,&local_538);
                  local_570 = FUN_000157c8(&local_158,&local_538);
                  local_568 = FUN_00015abc(&local_570);
                  plVar5 = (long *)FUN_000029d8(auStack_50);
                  local_574 = FUN_00032070(&local_568);
                  (**(code **)(*plVar5 + 0xf0))(plVar5,&local_574);
                  FUN_00002cf8(auStack_578,0x15d330);
                  local_580 = FUN_00015a7c(&local_538,param_1 + 0x88);
                  FUN_00013c20(auStack_30,auStack_578,&local_580);
                  FUN_00002cf8(auStack_584,0x15d4c7);
                  local_588 = 0;
                  FUN_00015b04(auStack_30,auStack_584,&local_588,1);
                  FUN_00002cf8(auStack_58c,0x15d4cc);
                  local_590 = 0;
                  FUN_00015b04(auStack_30,auStack_58c,&local_590,1);
                  if (local_f2 == 0) {
                    FUN_00002cf8(auStack_59c,0x15d49e);
                    FUN_0012dd50(param_1,auStack_59c,&DAT_00208c50);
                    FUN_00002cf8(auStack_5a0,0x15d4a0);
                    FUN_0012dd50(param_1,auStack_5a0,&DAT_00208c50);
                  }
                  else {
                    FUN_00002cf8(auStack_594,0x15d49f);
                    FUN_00131b38(param_1,auStack_594);
                    FUN_00002cf8(auStack_598,0x15d4a1);
                    FUN_00131b38(param_1,auStack_598);
                  }
                }
                plVar5 = (long *)FUN_000029d8(auStack_50);
                local_5a4 = FUN_00032070(&local_488);
                (**(code **)(*plVar5 + 0x100))(plVar5,&local_5a4);
                plVar5 = (long *)FUN_000029d8(auStack_50);
                local_5a8 = 1;
                (**(code **)(*plVar5 + 0xb0))(plVar5,&local_5a8);
                plVar5 = (long *)FUN_000029d8(auStack_50);
                (**(code **)(*plVar5 + 0xd0))(plVar5,&local_2f0);
                plVar5 = (long *)FUN_000029d8(auStack_50);
                local_5ac = 1;
                (**(code **)(*plVar5 + 0xe0))(plVar5,&local_5ac);
                plVar5 = (long *)FUN_00017c8c(auStack_90);
                local_5b0 = 1;
                (**(code **)(*plVar5 + 0x78))(plVar5,&local_5b0);
                plVar5 = (long *)FUN_00017c8c(auStack_90);
                (**(code **)(*plVar5 + 0x88))(plVar5,&local_2f0);
                plVar5 = (long *)FUN_00017c8c(auStack_90);
                local_5b4 = 1;
                (**(code **)(*plVar5 + 0x98))(plVar5,&local_5b4);
                FUN_00002cf8(auStack_5b8,0x15d4e6);
                local_5c8 = FUN_00015abc(&local_2f0);
                iVar3 = FUN_00032070(&local_5c8);
                FUN_00013490((double)iVar3);
                FUN_000196d0(auStack_30,auStack_5b8,auStack_5c0);
                FUN_00014a5c(auStack_30,&local_2f0);
                FUN_001cd3e4(auStack_30);
                plVar5 = (long *)FUN_000029d8(auStack_50);
                (**(code **)(*plVar5 + 0x130))(plVar5,&local_460);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                          (auStack_610,"0x15d300kGCMillimetersKey",0);
                sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_148,auStack_610,1,0);
                __ZN8PMStringD1Ev(auStack_610);
                if (sVar2 == 0) {
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_668,"0x15d300kGCInchesKey",0)
                  ;
                  sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_148,auStack_668,1,0);
                  __ZN8PMStringD1Ev(auStack_668);
                  if (sVar2 != 0) {
                    FUN_00013490(0x4052000000000000);
                    local_670 = FUN_000157c8(&local_460,auStack_678);
                    local_460 = local_670;
                  }
                }
                else {
                  FUN_00013490(0x4006ad5b202b0759);
                  local_618 = FUN_000157c8(&local_460,auStack_620);
                  local_460 = local_618;
                }
                FUN_00002cf8(auStack_67c,0x15d49d);
                FUN_00013c20(auStack_30,auStack_67c,&local_460);
                FUN_0016599c(param_1);
                FUN_00166204(param_1);
                plVar5 = (long *)FUN_0000e2e0(auStack_a0);
                (**(code **)(*plVar5 + 0x28))(plVar5,&local_2f0);
                FUN_00138008(param_1);
                FUN_00002cf8(auStack_680,0x15d4c9);
                local_684 = 0;
                FUN_00015b04(auStack_30,auStack_680,&local_684,1);
                FUN_00002cf8(auStack_688,0x15d4cf);
                local_68c = 0;
                FUN_00015b04(auStack_30,auStack_688,&local_68c,1);
                FUN_00002cf8(auStack_690,0x15d49b);
                FUN_0012fc3c(param_1,auStack_690,&DAT_00208c50);
                FUN_00002cf8(auStack_694,0x15d49d);
                FUN_0012fc3c(param_1,auStack_694,&DAT_00208c50);
                sVar2 = FUN_001a6604(auStack_e8);
                if (sVar2 != 0) goto LAB_00129868;
                local_698 = FUN_001a6844(auStack_e8,&DAT_00208c50);
                if (local_698 == 0) goto LAB_00129868;
                local_48 = 2;
              }
              else {
                FUN_00002cf8(auStack_69c,0x15d323);
                FUN_00013178(auStack_30,auStack_69c,&DAT_00208c50);
                FUN_000e8428(param_1);
LAB_00129868:
                FUN_00002cf8(auStack_6a0,0x15d326);
                local_6a4 = 5;
                FUN_00015844(auStack_30,auStack_6a0,&local_6a4,&local_fa);
                FUN_00002cf8(auStack_6a8,0x15d326);
                local_6ac = 6;
                FUN_00015844(auStack_30,auStack_6a8,&local_6ac,&local_fa);
                FUN_00002cf8(auStack_6b0,0x15d326);
                local_6b4 = 7;
                FUN_00015844(auStack_30,auStack_6b0,&local_6b4,&local_fa);
                FUN_00002cf8(auStack_6b8,0x15d326);
                local_6bc = 8;
                FUN_00015844(auStack_30,auStack_6b8,&local_6bc,&local_fa);
                local_48 = 0;
              }
              __ZN8PMStringD1Ev(auStack_148);
              __ZN8PMStringD1Ev(auStack_e8);
            }
            else {
              local_48 = 2;
            }
            FUN_0000e324(auStack_a0);
          }
          else {
            local_48 = 2;
          }
          FUN_0000e2f8(auStack_98);
        }
        else {
          local_48 = 2;
        }
        FUN_00017158(auStack_90);
      }
      else {
        local_48 = 2;
      }
      FUN_0000e040(auStack_80);
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
LAB_0012998c:
  FUN_00002c54(auStack_30);
  return;
}

//==== FUNC @ 150a0 -> 00014a5c

/* WARNING: Restarted to delay deadcode elimination for space: stack */

void FUN_00014a5c(undefined8 param_1,undefined8 *param_2)

{
  undefined8 uVar1;
  bool bVar2;
  short sVar3;
  long *plVar4;
  undefined4 local_1c4;
  undefined1 auStack_1c0 [4];
  int local_1bc;
  undefined1 auStack_1b8 [4];
  undefined4 local_1b4;
  undefined8 local_1b0;
  undefined8 local_1a8;
  undefined8 local_1a0;
  undefined1 auStack_198 [8];
  undefined8 local_190;
  undefined1 auStack_188 [8];
  undefined8 local_180;
  undefined1 auStack_178 [8];
  undefined8 local_170;
  undefined1 auStack_164 [4];
  undefined8 local_160;
  int local_158;
  int local_154;
  undefined1 auStack_150 [4];
  undefined4 local_14c;
  undefined1 auStack_148 [8];
  undefined8 local_140;
  int local_134;
  undefined1 auStack_130 [72];
  undefined1 auStack_e8 [12];
  undefined1 auStack_dc [4];
  int local_d8;
  undefined1 auStack_d4 [6];
  short local_ce;
  undefined1 auStack_cc [4];
  undefined8 local_c8;
  undefined4 local_c0;
  undefined1 auStack_bc [4];
  undefined1 auStack_b8 [76];
  undefined4 local_6c;
  undefined8 local_68;
  undefined4 local_60;
  undefined1 uStack_49;
  undefined1 auStack_48 [12];
  undefined1 auStack_3c [4];
  undefined8 local_38;
  undefined8 *local_30;
  undefined8 local_28;
  
  local_30 = param_2;
  local_28 = param_1;
  sVar3 = FUN_00002c30(param_1);
  if (sVar3 == 0) {
    plVar4 = (long *)FUN_00003ca8(local_28);
    FUN_00002cf8(auStack_3c,0x15d326);
    local_38 = (**(code **)(*plVar4 + 0x48))(plVar4,auStack_3c,9999);
    FUN_000147f4(auStack_48,local_38,&uStack_49);
    sVar3 = FUN_00014830(auStack_48);
    if (sVar3 == 0) {
      plVar4 = (long *)FUN_00014854(auStack_48);
      (**(code **)(*plVar4 + 0x28))(plVar4,1);
      local_68 = *local_30;
      local_6c = 3;
      FUN_000152f0(&local_68,&local_6c);
      uVar1 = local_28;
      FUN_00002cf8(auStack_bc,0x15d306);
      local_c0 = 0xffffffff;
      FUN_00012c10(auStack_b8,uVar1,auStack_bc,&local_c0);
      uVar1 = local_28;
      FUN_00002cf8(auStack_cc,&DAT_0015d31b);
      local_c8 = FUN_0001544c(uVar1,auStack_cc,auStack_b8);
      uVar1 = local_28;
      FUN_00002cf8(auStack_d4,0x15d323);
      local_ce = FUN_00012ef4(uVar1,auStack_d4);
      uVar1 = local_28;
      FUN_00002cf8(auStack_dc,0x15d326);
      local_d8 = FUN_00013034(uVar1,auStack_dc);
      FUN_00013490(0);
      sVar3 = FUN_00014140(&local_68,auStack_e8);
      if (sVar3 != 0) {
        __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_130,"Applied Leading: ",0);
        __ZN8PMString12AppendNumberERK6PMRealiss(auStack_130,&local_68,0xffffffff,0);
        __ZN8PMString6AppendEPKciNS_14StringEncodingE(auStack_130," pt",0x7fffffff,0xffffffff);
        plVar4 = (long *)FUN_00014854(auStack_48);
        (**(code **)(*plVar4 + 0x18))(plVar4,auStack_130,0xfffffffe,1);
        FUN_000157a4(auStack_130);
        for (local_134 = 2; local_134 < 6; local_134 = local_134 + 1) {
          FUN_00013490((double)(long)local_134);
          local_140 = FUN_000157c8(&local_68,auStack_148);
          local_14c = 3;
          FUN_000152f0(&local_140,&local_14c);
          __ZN8PMString6AppendEPKciNS_14StringEncodingE(auStack_130,"/",0x7fffffff,0xffffffff);
          __ZN8PMString12AppendNumberEi(auStack_130,local_134);
          __ZN8PMString6AppendEPKciNS_14StringEncodingE
                    (auStack_130," - V. Subdiv: ",0x7fffffff,0xffffffff);
          __ZN8PMString12AppendNumberERK6PMRealiss(auStack_130,&local_140,0xffffffff,0);
          __ZN8PMString6AppendEPKciNS_14StringEncodingE(auStack_130," pt",0x7fffffff,0xffffffff);
          plVar4 = (long *)FUN_00014854(auStack_48);
          (**(code **)(*plVar4 + 0x18))(plVar4,auStack_130,0xfffffffe,1);
          FUN_000157a4(auStack_130);
          sVar3 = FUN_00015808(&local_140,&local_c8);
          uVar1 = local_28;
          if (sVar3 != 0) {
            FUN_00002cf8(auStack_150,0x15d326);
            local_154 = local_134 + -1;
            FUN_00015844(uVar1,auStack_150,&local_154,&DAT_002084b8);
          }
        }
        for (local_158 = 2; uVar1 = local_28, local_158 < 6; local_158 = local_158 + 1) {
          FUN_00002cf8(auStack_164,0x15d31c);
          local_160 = FUN_00013338(uVar1,auStack_164);
          FUN_00015a50(&local_170);
          FUN_00013490(0);
          sVar3 = FUN_000132fc(&local_160,auStack_178);
          bVar2 = true;
          if ((sVar3 == 0) && (bVar2 = true, local_d8 < 1)) {
            bVar2 = local_ce == 0;
          }
          if (bVar2) {
            local_160 = local_68;
            FUN_00013490((double)(long)local_158);
            local_180 = FUN_00015a7c(&local_160,auStack_188);
            local_170 = local_180;
          }
          else {
            FUN_00013490((double)(long)local_158);
            local_190 = FUN_00015a7c(&local_160,auStack_198);
            local_170 = local_190;
            local_1b0 = FUN_000157c8(&local_c8,&local_170);
            local_1a8 = FUN_00015abc(&local_1b0);
            local_1a0 = FUN_000157c8(&local_c8,&local_1a8);
            local_170 = local_1a0;
          }
          local_1b4 = 3;
          FUN_000152f0(&local_170,&local_1b4);
          __ZN8PMString6SetKeyEPKcNS_19TranslateDuringCallE
                    (auStack_130,"0x15d300kGCMultiplicationKey",1);
          __ZN8PMString12AppendNumberEi(auStack_130,local_158);
          __ZN8PMString6AppendEPKciNS_14StringEncodingE
                    (auStack_130," - V. Subdiv: ",0x7fffffff,0xffffffff);
          __ZN8PMString12AppendNumberERK6PMRealiss(auStack_130,&local_170,0xffffffff,0);
          __ZN8PMString6AppendEPKciNS_14StringEncodingE(auStack_130," pt",0x7fffffff,0xffffffff);
          plVar4 = (long *)FUN_00014854(auStack_48);
          (**(code **)(*plVar4 + 0x18))(plVar4,auStack_130,0xfffffffe,1);
          FUN_000157a4(auStack_130);
          sVar3 = FUN_00015808(&local_170,&local_c8);
          uVar1 = local_28;
          if (sVar3 != 0) {
            FUN_00002cf8(auStack_1b8,0x15d326);
            local_1bc = local_158 + 3;
            FUN_00015844(uVar1,auStack_1b8,&local_1bc,&DAT_002084b8);
          }
        }
        __ZN8PMStringD1Ev(auStack_130);
      }
      uVar1 = local_28;
      FUN_00002cf8(auStack_1c0,0x15d326);
      local_1c4 = 0;
      FUN_00015b04(uVar1,auStack_1c0,&local_1c4,1);
      __ZN8PMStringD1Ev(auStack_b8);
      local_60 = 0;
    }
    else {
      local_60 = 2;
    }
    FUN_00014a30(auStack_48);
  }
  return;
}

//==== FUNC @ 163ee0 -> 00163ee0

void FUN_00163ee0(long param_1,short *param_2)

{
  bool bVar1;
  short sVar2;
  int iVar3;
  undefined8 uVar4;
  long *plVar5;
  undefined1 auStack_648 [12];
  undefined1 auStack_63c [4];
  undefined1 auStack_638 [12];
  undefined1 auStack_62c [4];
  undefined1 auStack_628 [4];
  undefined1 auStack_624 [4];
  undefined4 local_620;
  undefined1 auStack_61c [4];
  undefined8 local_618;
  undefined4 local_60c;
  undefined1 auStack_608 [4];
  undefined1 auStack_604 [4];
  undefined4 local_600;
  undefined1 auStack_5fc [4];
  undefined8 local_5f8;
  undefined4 local_5ec;
  undefined1 auStack_5e8 [12];
  undefined1 auStack_5dc [4];
  undefined8 local_5d8;
  undefined1 auStack_5d0 [12];
  undefined1 auStack_5c4 [4];
  undefined8 local_5c0;
  undefined1 auStack_5b4 [4];
  undefined8 local_5b0;
  undefined8 local_5a8;
  undefined1 auStack_59c [4];
  undefined8 local_598;
  undefined8 local_590;
  undefined1 auStack_588 [8];
  undefined1 auStack_580 [12];
  undefined1 auStack_574 [4];
  undefined1 auStack_570 [12];
  undefined1 auStack_564 [4];
  undefined1 auStack_560 [4];
  undefined1 auStack_55c [4];
  undefined1 auStack_558 [8];
  undefined8 local_550;
  undefined8 local_548;
  undefined8 local_540;
  undefined4 local_538;
  undefined1 auStack_534 [4];
  undefined8 local_530;
  undefined4 local_524;
  undefined1 auStack_520 [4];
  undefined1 auStack_51c [4];
  undefined4 local_518;
  undefined1 auStack_514 [4];
  undefined8 local_510;
  undefined4 local_504;
  undefined1 auStack_500 [12];
  undefined1 auStack_4f4 [4];
  undefined8 local_4f0;
  undefined1 auStack_4e8 [12];
  undefined1 auStack_4dc [4];
  undefined8 local_4d8;
  undefined1 auStack_4cc [4];
  undefined1 auStack_4c8 [8];
  undefined8 local_4c0;
  undefined8 local_4b8;
  undefined8 local_4b0;
  undefined8 local_4a8;
  undefined8 local_4a0;
  undefined8 local_498;
  undefined1 auStack_490 [8];
  undefined8 local_488;
  undefined8 local_480;
  undefined8 local_478;
  undefined8 local_470;
  undefined1 auStack_468 [8];
  undefined8 local_460;
  undefined8 local_458;
  undefined8 local_450;
  undefined8 local_448;
  undefined1 auStack_43c [4];
  undefined8 local_438;
  undefined4 local_430;
  undefined1 auStack_42c [4];
  undefined1 auStack_428 [72];
  undefined1 auStack_3e0 [72];
  undefined8 local_398;
  undefined8 local_390;
  undefined1 auStack_388 [8];
  undefined8 local_380;
  undefined1 auStack_378 [8];
  undefined8 local_370;
  undefined1 auStack_368 [8];
  undefined8 local_360;
  undefined8 local_358;
  undefined8 local_350;
  undefined8 local_348;
  undefined1 auStack_340 [12];
  undefined1 auStack_334 [4];
  undefined8 local_330;
  undefined1 auStack_324 [4];
  undefined8 local_320;
  undefined1 auStack_314 [4];
  undefined8 local_310;
  undefined1 auStack_308 [72];
  int local_2c0;
  int local_2bc;
  int local_2b8;
  int local_2b4;
  undefined8 local_2b0;
  undefined8 local_2a8;
  undefined4 local_2a0;
  undefined1 auStack_29c [4];
  undefined8 local_298;
  undefined1 auStack_290 [72];
  undefined8 local_248;
  undefined8 local_240;
  undefined8 local_238;
  undefined8 local_230;
  undefined8 local_228;
  undefined8 local_220;
  undefined1 auStack_218 [8];
  undefined8 local_210;
  byte local_201;
  undefined1 auStack_200 [72];
  undefined1 auStack_1b8 [12];
  undefined1 auStack_1ac [6];
  short local_1a6;
  undefined1 auStack_1a4 [6];
  short local_19e;
  undefined1 auStack_19c [6];
  short local_196;
  undefined1 auStack_194 [4];
  undefined8 local_190;
  undefined1 auStack_184 [4];
  undefined8 local_180;
  undefined1 auStack_174 [4];
  undefined8 local_170;
  undefined1 auStack_164 [4];
  undefined8 local_160;
  undefined8 local_158;
  undefined1 auStack_14c [4];
  undefined8 local_148;
  undefined1 auStack_140 [8];
  undefined8 local_138;
  undefined8 local_130;
  undefined1 auStack_124 [4];
  undefined8 local_120;
  undefined8 local_118;
  undefined4 local_110;
  undefined1 auStack_10c [4];
  undefined1 auStack_108 [79];
  undefined1 uStack_b9;
  undefined1 auStack_b8 [8];
  undefined1 local_b0 [16];
  undefined1 auStack_a0 [15];
  undefined1 uStack_91;
  undefined1 auStack_90 [8];
  undefined1 auStack_88 [15];
  undefined1 uStack_79;
  undefined1 auStack_78 [8];
  undefined1 local_70 [16];
  undefined1 uStack_59;
  undefined1 auStack_58 [8];
  undefined4 local_50;
  undefined1 uStack_39;
  undefined1 auStack_38 [8];
  short *local_30;
  long local_28;
  
  local_30 = param_2;
  local_28 = param_1;
  FUN_00002bf4(auStack_38,param_1,&uStack_39);
  sVar2 = FUN_00002c30(auStack_38);
  if (sVar2 == 0) {
    uVar4 = __Z26GetExecutionContextSessionv();
    FUN_00002978(auStack_58,uVar4,&uStack_59);
    sVar2 = FUN_000029b4(auStack_58);
    if (sVar2 == 0) {
      plVar5 = (long *)FUN_000029d8(auStack_58);
      local_70 = (**(code **)(*plVar5 + 0x18))();
      FUN_0000df20(auStack_78,local_70,&uStack_79);
      sVar2 = FUN_0000df5c(auStack_78);
      if (sVar2 == 0) {
        FUN_001a7968(auStack_88);
        sVar2 = FUN_00016b78(auStack_88);
        if (sVar2 == 0) {
          uVar4 = FUN_0000df80(auStack_78);
          FUN_0000df98(auStack_90,uVar4,&uStack_91);
          sVar2 = FUN_0000dfd4(auStack_90);
          if (sVar2 == 0) {
            FUN_001a2484(auStack_a0);
            sVar2 = FUN_0000e2a4(auStack_a0);
            if (sVar2 == 0) {
              plVar5 = (long *)FUN_000029d8(auStack_58);
              local_b0 = (**(code **)(*plVar5 + 0x28))();
              FUN_00017bd4(auStack_b8,local_b0,&uStack_b9);
              sVar2 = FUN_00017c10(auStack_b8);
              if (sVar2 == 0) {
                FUN_00002cf8(auStack_10c,0x15d309);
                local_110 = 0xffffffff;
                FUN_00012c10(auStack_108,auStack_38,auStack_10c,&local_110);
                plVar5 = (long *)FUN_00017c34(auStack_b8);
                local_118 = (**(code **)(*plVar5 + 0x2c0))();
                FUN_00002cf8(auStack_124,0x15d3a8);
                local_120 = FUN_00013338(auStack_38,auStack_124);
                sVar2 = __Z19IsCommandKeyPressedv();
                if (sVar2 != 0) {
                  FUN_00068460(&local_120,param_1 + 0x88);
                }
                plVar5 = (long *)FUN_000029d8(auStack_58);
                local_138 = (**(code **)(*plVar5 + 0x98))();
                plVar5 = (long *)FUN_000029d8(auStack_58);
                iVar3 = (**(code **)(*plVar5 + 0xa8))();
                FUN_00013490((double)iVar3);
                local_130 = FUN_000157c8(&local_138,auStack_140);
                FUN_00002cf8(auStack_14c,0x15d31d);
                local_158 = FUN_00013338(auStack_38,auStack_14c);
                local_148 = local_158;
                FUN_00002cf8(auStack_164,0x15d49a);
                local_160 = FUN_00013338(auStack_38,auStack_164);
                FUN_00002cf8(auStack_174,0x15d49c);
                local_170 = FUN_00013338(auStack_38,auStack_174);
                FUN_00002cf8(auStack_184,0x15d49e);
                local_180 = FUN_00013338(auStack_38,auStack_184);
                FUN_00002cf8(auStack_194,0x15d4a0);
                local_190 = FUN_00013338(auStack_38,auStack_194);
                FUN_00002cf8(auStack_19c,0x15d4a3);
                local_196 = FUN_00012ef4(auStack_38,auStack_19c);
                FUN_00002cf8(auStack_1a4,0x15d4a4);
                local_19e = FUN_00012ef4(auStack_38,auStack_1a4);
                FUN_00002cf8(auStack_1ac,0x15d323);
                local_1a6 = FUN_00012ef4(auStack_38,auStack_1ac);
                FUN_00013490(0);
                sVar2 = FUN_00014140(&local_158,auStack_1b8);
                local_201 = 0;
                bVar1 = false;
                if (((sVar2 != 0) && (bVar1 = true, local_196 != 0)) &&
                   (bVar1 = true, local_1a6 != 0)) {
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_200,"[GC-1]",0);
                  local_201 = 1;
                  sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_108,auStack_200,1,0);
                  bVar1 = sVar2 == 0;
                }
                if ((local_201 & 1) != 0) {
                  __ZN8PMStringD1Ev(auStack_200);
                }
                if (bVar1) {
                  FUN_00013490(&local_210);
                  FUN_00013490(0);
                  sVar2 = FUN_00018504(&local_118,auStack_218);
                  if (sVar2 == 0) {
                    local_240 = FUN_00015a7c(&local_160,&local_158);
                    local_238 = FUN_00015a7c(&local_240,param_1 + 0x88);
                    local_210 = local_238;
                  }
                  else {
                    local_230 = FUN_00015a7c(&local_160,&local_158);
                    local_228 = FUN_00017c4c(&local_230,&local_118);
                    local_220 = FUN_00015a7c(&local_228,param_1 + 0x88);
                    local_210 = local_220;
                  }
                  plVar5 = (long *)FUN_000029d8(auStack_58);
                  local_248 = (**(code **)(*plVar5 + 0x1b8))();
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_290,"[GC-1]",0);
                  sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_108,auStack_290,1,0);
                  bVar1 = false;
                  if ((sVar2 == 0) && (bVar1 = false, local_196 != 0)) {
                    bVar1 = local_1a6 != 0;
                  }
                  __ZN8PMStringD1Ev(auStack_290);
                  if (bVar1) {
                    local_298 = FUN_00015a7c(&local_248,param_1 + 0x88);
                    FUN_0004ef2c(&local_210,&local_298);
                  }
                  sVar2 = __Z19IsCommandKeyPressedv();
                  if (sVar2 != 0) {
                    FUN_000afc88(&local_210,param_1 + 0x88);
                  }
                  FUN_00002cf8(auStack_29c,0x15d49b);
                  FUN_00013c20(auStack_38,auStack_29c,&local_210);
                  plVar5 = (long *)FUN_000029d8(auStack_58);
                  local_2a0 = (**(code **)(*plVar5 + 0xf8))();
                  FUN_00013490(0,&local_2a8);
                  plVar5 = (long *)FUN_000029d8(auStack_58);
                  local_2b0 = (**(code **)(*plVar5 + 0x128))();
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_308,"[GC] CGS",0);
                  sVar2 = FUN_001a6604(auStack_308);
                  __ZN8PMStringD1Ev(auStack_308);
                  if (sVar2 == 0) {
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_3e0,"[GC-1]",0);
                    sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_108,auStack_3e0,1,0);
                    bVar1 = false;
                    if ((sVar2 == 0) && (bVar1 = false, local_196 != 0)) {
                      bVar1 = local_1a6 != 0;
                    }
                    __ZN8PMStringD1Ev(auStack_3e0);
                    if (bVar1) {
                      FUN_00002cf8(auStack_42c,0x15d306);
                      local_430 = 0xffffffff;
                      FUN_00012c10(auStack_428,auStack_38,auStack_42c,&local_430);
                      FUN_00002cf8(auStack_43c,&DAT_0015d31b);
                      local_438 = FUN_0001544c(auStack_38,auStack_43c,auStack_428);
                      FUN_0004ef9c(&local_438,&local_248);
                      local_460 = FUN_000157c8(&local_438,&local_158);
                      local_458 = FUN_00015abc(&local_460);
                      local_450 = FUN_00015a7c(&local_458,&local_158);
                      local_448 = FUN_00028080(&local_438,&local_450);
                      FUN_00013490(0);
                      sVar2 = FUN_0003208c(&local_448,auStack_468);
                      if (sVar2 != 0) {
                        local_470 = FUN_00017c4c(&local_158,&local_448);
                        local_448 = local_470;
                      }
                      local_488 = FUN_00015a7c(&local_170,&local_158);
                      local_480 = FUN_00017c4c(&local_448,&local_488);
                      local_478 = FUN_00015a7c(&local_480,param_1 + 0x88);
                      local_2a8 = local_478;
                      plVar5 = (long *)FUN_000029d8(auStack_58);
                      (**(code **)(*plVar5 + 0x1d0))(plVar5,&local_448);
                      __ZN8PMStringD1Ev(auStack_428);
                    }
                    else {
                      FUN_00013490(0);
                      sVar2 = FUN_00014140(&local_118,auStack_490);
                      if (sVar2 == 0 || local_19e == 0) {
                        local_4c0 = FUN_00015a7c(&local_170,&local_158);
                        local_4b8 = FUN_00017c4c(&local_2b0,&local_4c0);
                        local_4b0 = FUN_00015a7c(&local_4b8,param_1 + 0x88);
                        local_2a8 = local_4b0;
                      }
                      else {
                        local_4a8 = FUN_00015a7c(&local_170,&local_158);
                        local_4a0 = FUN_00017c4c(&local_2b0,&local_4a8);
                        local_498 = FUN_00015a7c(&local_4a0,param_1 + 0x88);
                        local_2a8 = local_498;
                      }
                    }
                  }
                  else {
                    FUN_00002cf8(auStack_314,0x15d347);
                    local_310 = FUN_00013338(auStack_38,auStack_314);
                    local_2b4 = FUN_00032070(&local_310);
                    FUN_00002cf8(auStack_324,0x15d342);
                    local_320 = FUN_00013338(auStack_38,auStack_324);
                    local_2b8 = FUN_00032070(&local_320);
                    FUN_00002cf8(auStack_334,0x15d344);
                    local_330 = FUN_00013338(auStack_38,auStack_334);
                    local_2bc = FUN_00032070(&local_330);
                    if (0 < local_2b4) {
                      iVar3 = 0;
                      if (local_2b4 != 0) {
                        iVar3 = (local_2b8 * local_2bc) / local_2b4;
                      }
                      local_2c0 = local_2b8 * local_2bc - iVar3 * local_2b4;
                    }
                    bVar1 = false;
                    if (0 < local_2b4) {
                      FUN_00013490(0);
                      sVar2 = FUN_00014140(&local_170,auStack_340);
                      bVar1 = false;
                      if (sVar2 != 0) {
                        bVar1 = 0 < local_2c0;
                      }
                    }
                    if (bVar1) {
                      FUN_00013490(0x3ff0000000000000);
                      local_360 = FUN_00028080(&local_170,auStack_368);
                      local_358 = FUN_00015a7c(&local_360,&local_158);
                      FUN_00013490((double)(long)local_2c0,auStack_378);
                      FUN_00013490((double)(long)local_2b4);
                      local_380 = FUN_000157c8(&local_158,auStack_388);
                      local_370 = FUN_00015a7c(auStack_378,&local_380);
                      local_350 = FUN_00017c4c(&local_358,&local_370);
                      local_348 = FUN_00015a7c(&local_350,param_1 + 0x88);
                      local_2a8 = local_348;
                    }
                    else {
                      local_398 = FUN_00015a7c(&local_170,&local_158);
                      local_390 = FUN_00015a7c(&local_398,param_1 + 0x88);
                      local_2a8 = local_390;
                    }
                  }
                  sVar2 = __Z19IsCommandKeyPressedv();
                  if (sVar2 != 0) {
                    FUN_000afc88(&local_2a8,param_1 + 0x88);
                  }
                  FUN_00013490(0);
                  sVar2 = FUN_00015808(&local_2a8,auStack_4c8);
                  if (sVar2 != 0) {
                    FUN_00002cf8(auStack_4cc,0x15d49d);
                    FUN_00013c20(auStack_38,auStack_4cc,&local_2a8);
                  }
                  FUN_00002cf8(auStack_4dc,&DAT_0015d376);
                  local_4d8 = FUN_00013338(auStack_38,auStack_4dc);
                  FUN_00013490(0);
                  sVar2 = FUN_00014140(&local_4d8,auStack_4e8);
                  bVar1 = true;
                  if (sVar2 == 0) {
                    FUN_00002cf8(auStack_4f4,&DAT_0015d378);
                    local_4f0 = FUN_00013338(auStack_38,auStack_4f4);
                    FUN_00013490(0);
                    sVar2 = FUN_00014140(&local_4f0,auStack_500);
                    bVar1 = sVar2 != 0;
                  }
                  if (bVar1) {
                    local_504 = 3;
                    FUN_000152f0(&local_210,&local_504);
                    FUN_00002cf8(auStack_514,&DAT_0015d377);
                    local_510 = FUN_00013338(auStack_38,auStack_514);
                    local_518 = 3;
                    FUN_000152f0(&local_510,&local_518);
                    sVar2 = FUN_00018504(&local_510,&local_210);
                    if ((sVar2 != 0) || (*local_30 != 0)) {
                      if (*local_30 != 0) {
                        FUN_00002cf8(auStack_51c,&DAT_0015d376);
                        FUN_00013c20(auStack_38,auStack_51c,&local_210);
                      }
                      FUN_00002cf8(auStack_520,&DAT_0015d377);
                      FUN_00013c20(auStack_38,auStack_520,&local_210);
                    }
                    local_524 = 3;
                    FUN_000152f0(&local_2a8,&local_524);
                    FUN_00002cf8(auStack_534,&DAT_0015d379);
                    local_530 = FUN_00013338(auStack_38,auStack_534);
                    local_538 = 3;
                    FUN_000152f0(&local_530,&local_538);
                    FUN_00013490(0,&local_540);
                    sVar2 = FUN_00014140(&local_530,&local_2a8);
                    if (sVar2 == 0) {
                      local_550 = FUN_00028080(&local_2a8,&local_530);
                      local_540 = local_550;
                    }
                    else {
                      local_548 = FUN_00028080(&local_530,&local_2a8);
                      local_540 = local_548;
                    }
                    FUN_00013490(0x3f50624dd2f1a9fc);
                    sVar2 = FUN_00014140(&local_540,auStack_558);
                    if (sVar2 != 0) {
                      FUN_00002cf8(auStack_55c,&DAT_0015d378);
                      FUN_00013c20(auStack_38,auStack_55c,&local_2a8);
                      FUN_00002cf8(auStack_560,&DAT_0015d379);
                      FUN_00013c20(auStack_38,auStack_560,&local_2a8);
                    }
                  }
                }
                else if (local_196 == 0) {
                  FUN_00002cf8(auStack_564,0x15d49b);
                  FUN_00013490(0);
                  FUN_00013c20(auStack_38,auStack_564,auStack_570);
                  FUN_00002cf8(auStack_574,0x15d49d);
                  FUN_00013490(0);
                  FUN_00013c20(auStack_38,auStack_574,auStack_580);
                }
                FUN_00013490(0);
                sVar2 = FUN_00014140(&local_130,auStack_588);
                if (sVar2 == 0 || local_19e != 0) {
                  if (local_19e == 0) {
                    FUN_00002cf8(auStack_62c,0x15d49f);
                    FUN_00013490(0);
                    FUN_00013c20(auStack_38,auStack_62c,auStack_638);
                    FUN_00002cf8(auStack_63c,0x15d4a1);
                    FUN_00013490(0);
                    FUN_00013c20(auStack_38,auStack_63c,auStack_648);
                  }
                }
                else {
                  local_598 = FUN_00015a7c(&local_180,&local_130);
                  local_590 = FUN_00015a7c(&local_598,param_1 + 0x88);
                  sVar2 = __Z19IsCommandKeyPressedv();
                  if (sVar2 != 0) {
                    FUN_000afc88(&local_590,param_1 + 0x88);
                  }
                  FUN_00002cf8(auStack_59c,0x15d49f);
                  FUN_00013c20(auStack_38,auStack_59c,&local_590);
                  local_5b0 = FUN_00015a7c(&local_190,&local_130);
                  local_5a8 = FUN_00015a7c(&local_5b0,param_1 + 0x88);
                  sVar2 = __Z19IsCommandKeyPressedv();
                  if (sVar2 != 0) {
                    FUN_000afc88(&local_5a8,param_1 + 0x88);
                  }
                  FUN_00002cf8(auStack_5b4,0x15d4a1);
                  FUN_00013c20(auStack_38,auStack_5b4,&local_5a8);
                  FUN_00002cf8(auStack_5c4,&DAT_0015d370);
                  local_5c0 = FUN_00013338(auStack_38,auStack_5c4);
                  FUN_00013490(0);
                  sVar2 = FUN_00014140(&local_5c0,auStack_5d0);
                  bVar1 = true;
                  if (sVar2 == 0) {
                    FUN_00002cf8(auStack_5dc,&DAT_0015d372);
                    local_5d8 = FUN_00013338(auStack_38,auStack_5dc);
                    FUN_00013490(0);
                    sVar2 = FUN_00014140(&local_5d8,auStack_5e8);
                    bVar1 = sVar2 != 0;
                  }
                  if (bVar1) {
                    local_5ec = 3;
                    FUN_000152f0(&local_590,&local_5ec);
                    FUN_00002cf8(auStack_5fc,&DAT_0015d371);
                    local_5f8 = FUN_00013338(auStack_38,auStack_5fc);
                    local_600 = 3;
                    FUN_000152f0(&local_5f8,&local_600);
                    sVar2 = FUN_00018504(&local_5f8,&local_590);
                    if (sVar2 != 0) {
                      FUN_00002cf8(auStack_604,&DAT_0015d370);
                      FUN_00013c20(auStack_38,auStack_604,&local_590);
                      FUN_00002cf8(auStack_608,&DAT_0015d371);
                      FUN_00013c20(auStack_38,auStack_608,&local_590);
                    }
                    local_60c = 3;
                    FUN_000152f0(&local_5a8,&local_60c);
                    FUN_00002cf8(auStack_61c,&DAT_0015d373);
                    local_618 = FUN_00013338(auStack_38,auStack_61c);
                    local_620 = 3;
                    FUN_000152f0(&local_618,&local_620);
                    sVar2 = FUN_00018504(&local_618,&local_5a8);
                    if (sVar2 != 0) {
                      FUN_00002cf8(auStack_624,&DAT_0015d372);
                      FUN_00013c20(auStack_38,auStack_624,&local_5a8);
                      FUN_00002cf8(auStack_628,&DAT_0015d373);
                      FUN_00013c20(auStack_38,auStack_628,&local_5a8);
                    }
                  }
                }
                __ZN8PMStringD1Ev(auStack_108);
                local_50 = 0;
              }
              else {
                local_50 = 2;
              }
              FUN_00017ca4(auStack_b8);
            }
            else {
              local_50 = 2;
            }
            FUN_0000e2f8(auStack_a0);
          }
          else {
            local_50 = 2;
          }
          FUN_0000e040(auStack_90);
        }
        else {
          local_50 = 2;
        }
        FUN_00017158(auStack_88);
      }
      else {
        local_50 = 2;
      }
      FUN_0000e06c(auStack_78);
    }
    else {
      local_50 = 2;
    }
    FUN_00002b9c(auStack_58);
  }
  else {
    local_50 = 2;
  }
  FUN_00002c54(auStack_38);
  return;
}
