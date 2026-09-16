/*
Apply Square Grid (row 07). FUN_0012335c: k = Round(W / u_v), W' = k * u_v, u_h := u_v.

PMReal helpers: construct 0x13490, operator/ 0x157c8, operator* 0x15a7c, operator+ 0x17c4c, operator- 0x28080,
Round 0x15abc (floor(x + 0.5)), floor 0x37d70, a > b + 1e-8 0x14140, a >= b - 1e-8 0x15808, a <= b + 1e-8 0x31d38,
a < b - 1e-8 0x3208c, |a - b| < 1e-8 0x132fc, not equal 0x18504, quantise 0x152f0 (mode 3 three decimals, mode 4 four).
Widget access: read value 0x13338, read value in the panel unit 0x1544c, read checkbox 0x12ef4, read dropdown index 0x13034,
write value 0x13c20, set maximum 0x196d0, unit factor at object + 0x88.
*/

//==== FUNC @ 123500 -> 0012335c

void FUN_0012335c(long param_1)

{
  long lVar1;
  bool bVar2;
  short sVar3;
  int iVar4;
  undefined8 uVar5;
  long *plVar6;
  undefined1 auStack_804 [4];
  undefined1 auStack_800 [4];
  undefined1 auStack_7fc [4];
  undefined1 auStack_7f8 [72];
  undefined1 auStack_7b0 [76];
  undefined1 auStack_764 [4];
  undefined1 auStack_760 [4];
  undefined1 auStack_75c [4];
  undefined1 auStack_758 [12];
  undefined1 auStack_74c [4];
  undefined8 local_748;
  undefined1 auStack_740 [76];
  undefined1 auStack_6f4 [4];
  undefined8 local_6f0;
  undefined8 local_6e8;
  undefined1 auStack_6dc [4];
  undefined1 auStack_6d8 [4];
  undefined1 auStack_6d4 [4];
  undefined8 local_6d0;
  undefined4 local_6c8;
  undefined1 auStack_6c4 [4];
  undefined1 auStack_6c0 [72];
  undefined4 local_678;
  undefined4 local_674;
  undefined1 auStack_670 [76];
  undefined4 local_624;
  undefined1 auStack_620 [72];
  undefined4 local_5d8;
  undefined1 auStack_5d4 [4];
  undefined1 auStack_5d0 [8];
  undefined1 auStack_5c8 [8];
  undefined8 local_5c0;
  undefined1 auStack_5b8 [76];
  undefined1 auStack_56c [4];
  undefined4 local_568;
  undefined1 auStack_564 [4];
  undefined1 auStack_560 [8];
  undefined1 auStack_558 [8];
  undefined8 local_550;
  undefined1 auStack_548 [76];
  undefined1 auStack_4fc [4];
  undefined1 local_4f8 [16];
  undefined1 auStack_4e8 [8];
  undefined1 auStack_4e0 [24];
  undefined1 auStack_4c8 [12];
  undefined1 auStack_4bc [4];
  undefined1 auStack_4b8 [8];
  undefined1 auStack_4b0 [4];
  undefined1 auStack_4ac [4];
  undefined1 auStack_4a8 [8];
  undefined8 local_4a0;
  undefined1 auStack_494 [4];
  undefined1 auStack_490 [72];
  undefined1 auStack_448 [8];
  undefined8 local_440;
  undefined1 auStack_434 [4];
  undefined1 auStack_430 [72];
  undefined1 auStack_3e8 [8];
  undefined1 auStack_3e0 [8];
  undefined1 auStack_3d8 [32];
  undefined1 local_3b8 [16];
  undefined1 auStack_3a8 [76];
  undefined1 auStack_35c [4];
  undefined4 local_358;
  undefined1 auStack_354 [4];
  undefined8 local_350;
  undefined8 local_348;
  undefined8 local_340;
  undefined8 local_338;
  undefined8 local_330;
  undefined8 local_328;
  undefined8 local_320;
  undefined1 auStack_318 [8];
  undefined8 local_310;
  undefined8 local_308;
  undefined8 local_300;
  undefined1 auStack_2f8 [12];
  int local_2ec;
  undefined1 auStack_2e8 [76];
  undefined1 auStack_29c [4];
  undefined4 local_298;
  undefined1 auStack_294 [4];
  undefined8 local_290;
  undefined1 auStack_288 [8];
  undefined8 local_280;
  undefined8 local_278;
  undefined8 local_270;
  undefined1 auStack_264 [4];
  undefined8 local_260;
  undefined1 auStack_258 [76];
  undefined1 auStack_20c [4];
  undefined1 auStack_208 [72];
  undefined8 local_1c0;
  long local_1b8;
  undefined1 auStack_1b0 [72];
  undefined1 auStack_168 [8];
  undefined1 auStack_160 [12];
  undefined1 auStack_154 [4];
  undefined8 local_150;
  undefined1 auStack_144 [4];
  undefined8 local_140;
  undefined1 auStack_134 [4];
  undefined8 local_130;
  undefined4 local_128;
  undefined1 auStack_124 [4];
  undefined1 auStack_120 [72];
  undefined1 auStack_d8 [78];
  short local_8a;
  undefined1 auStack_88 [15];
  undefined1 uStack_79;
  undefined1 auStack_78 [8];
  undefined4 local_70;
  undefined1 uStack_59;
  undefined1 auStack_58 [8];
  undefined8 local_50;
  undefined8 local_48;
  undefined8 local_40;
  undefined8 local_38;
  long local_30;
  long local_28;
  
  local_30 = param_1;
  FUN_00013490(&local_38);
  FUN_00013490(0,&local_40);
  FUN_00013490(0,&local_48);
  FUN_00013490(0,&local_50);
  FUN_00002bf4(auStack_58,param_1,&uStack_59);
  sVar3 = FUN_00002c30(auStack_58);
  if (sVar3 != 0) {
    local_70 = 2;
    goto LAB_00124958;
  }
  uVar5 = __Z26GetExecutionContextSessionv();
  FUN_00002978(auStack_78,uVar5,&uStack_79);
  sVar3 = FUN_000029b4(auStack_78);
  if (sVar3 == 0) {
    FUN_001a7968(auStack_88);
    sVar3 = FUN_00016b78(auStack_88);
    if (sVar3 == 0) {
      local_8a = 0;
      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_d8,"[GC] Square: ",0);
      FUN_00002cf8(auStack_124,0x15d306);
      local_128 = 0xffffffff;
      FUN_00012c10(auStack_120,auStack_58,auStack_124,&local_128);
      FUN_00002cf8(auStack_134,0x15d31c);
      local_130 = FUN_00013338(auStack_58,auStack_134);
      FUN_00002cf8(auStack_144,0x15d31d);
      local_140 = FUN_00013338(auStack_58,auStack_144);
      FUN_00002cf8(auStack_154,&DAT_0015d31b);
      local_150 = FUN_0001544c(auStack_58,auStack_154,auStack_120);
      FUN_00013490(0);
      sVar3 = FUN_000132fc(&local_130,auStack_160);
      bVar2 = true;
      if (sVar3 == 0) {
        FUN_00013490(0);
        sVar3 = FUN_000132fc(&local_140,auStack_168);
        bVar2 = true;
        if (sVar3 == 0) {
          sVar3 = FUN_00015808(&local_140,&local_150);
          bVar2 = sVar3 != 0;
        }
      }
      if (bVar2) {
        local_70 = 2;
      }
      else {
        FUN_001cc9ac(auStack_1b0,auStack_d8);
        sVar3 = __ZNK8PMString7IsEmptyEv(auStack_1b0);
        if (sVar3 == 0) {
          iVar4 = FUN_00030b98(auStack_1b0);
          local_1b8 = __ZNK8PMString9SubstringEii(auStack_1b0,0xd,iVar4 + -0xd);
          if (local_1b8 != 0) {
            uVar5 = __ZNK8PMString11GetAsDoubleEPNS_15ConversionErrorEPi(local_1b8,0);
            FUN_00013490(uVar5,&local_1c0);
            lVar1 = local_1b8;
            local_38 = local_1c0;
            if (local_1b8 != 0) {
              __ZN8PMStringD1Ev(local_1b8);
              local_28 = lVar1;
              __ZN8K2Memory27RTLCompatibleDeleteDelegateEPv(lVar1);
            }
            goto LAB_001237c0;
          }
          local_70 = 2;
        }
        else {
LAB_001237c0:
          FUN_001b4670(auStack_1b0);
          FUN_00002cf8(auStack_20c,0x15d324);
          FUN_000138a8(auStack_208,auStack_58,auStack_20c);
          __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_258,"0x15d300kGCSquareKey",0);
          sVar3 = __ZNK8PMString7IsEqualERKS_hh(auStack_208,auStack_258,1,0);
          __ZN8PMStringD1Ev(auStack_258);
          if (sVar3 == 0) {
            local_8a = 1;
            FUN_00013490(0);
            sVar3 = FUN_000132fc(&local_38,auStack_2f8);
            if (sVar3 != 0) {
              plVar6 = (long *)FUN_000029d8(auStack_78);
              iVar4 = (**(code **)(*plVar6 + 0xe8))();
              FUN_00013490((double)iVar4,&local_300);
              local_48 = local_300;
              FUN_00013490(0x3ff0000000000000);
              local_310 = FUN_00028080(&local_48,auStack_318);
              local_308 = FUN_00015a7c(&local_310,&local_140);
              local_38 = local_308;
            }
            local_40 = local_38;
            local_338 = FUN_000157c8(&local_150,&local_130);
            local_330 = FUN_00015abc(&local_338);
            local_328 = FUN_000157c8(&local_150,&local_330);
            local_340 = FUN_000157c8(&local_40,&local_150);
            local_320 = FUN_00015a7c(&local_328,&local_340);
            local_50 = local_320;
            local_350 = FUN_000157c8(&local_40,&local_50);
            local_348 = FUN_00015abc(&local_350);
            local_48 = local_348;
            plVar6 = (long *)FUN_00017c8c(auStack_88);
            (**(code **)(*plVar6 + 0x68))(plVar6,&local_50);
            FUN_00002cf8(auStack_354,0x15d326);
            iVar4 = FUN_00013034(auStack_58,auStack_354);
            if (iVar4 < 1) {
              plVar6 = (long *)FUN_00017c8c(auStack_88);
              local_358 = 1;
              (**(code **)(*plVar6 + 0x78))(plVar6,&local_358);
            }
            FUN_00002cf8(auStack_35c,0x15d324);
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_3a8,"0x15d300kGCSquareKey",0);
            FUN_00013588(auStack_58,auStack_35c,auStack_3a8);
            __ZN8PMStringD1Ev(auStack_3a8);
LAB_00123dcc:
            plVar6 = (long *)FUN_000029d8(auStack_78);
            local_3b8 = (**(code **)(*plVar6 + 0x18))();
            FUN_00013490();
            FUN_00013490(0);
            __ZN6PMRectC1ERK6PMRealS2_S2_S2_
                      (auStack_3d8,auStack_3e0,auStack_3e8,&local_40,&local_150);
            FUN_001b0acc(local_3b8,auStack_3d8);
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_430,"0x15d300kGCMillimetersKey",0);
            sVar3 = __ZNK8PMString7IsEqualERKS_hh(auStack_120,auStack_430,1,0);
            __ZN8PMStringD1Ev(auStack_430);
            if (sVar3 == 0) {
              __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_490,"0x15d300kGCInchesKey",0);
              sVar3 = __ZNK8PMString7IsEqualERKS_hh(auStack_120,auStack_490,1,0);
              __ZN8PMStringD1Ev(auStack_490);
              if (sVar3 == 0) {
                FUN_00002cf8(auStack_4ac,0x15d31a);
                FUN_00013c20(auStack_58,auStack_4ac,&local_40);
              }
              else {
                FUN_00002cf8(auStack_494,0x15d31a);
                FUN_00013490(0x4052000000000000);
                local_4a0 = FUN_000157c8(&local_40,auStack_4a8);
                FUN_00013c20(auStack_58,auStack_494,&local_4a0);
              }
            }
            else {
              FUN_00002cf8(auStack_434,0x15d31a);
              FUN_00013490(0x4006ad5b202b0759);
              local_440 = FUN_000157c8(&local_40,auStack_448);
              FUN_00013c20(auStack_58,auStack_434,&local_440);
            }
            FUN_0001b96c(auStack_58);
            FUN_00002cf8(auStack_4b0,0x15d49e);
            FUN_00013490(0);
            FUN_00013c20(auStack_58,auStack_4b0,auStack_4b8);
            FUN_00002cf8(auStack_4bc,0x15d4a0);
            FUN_00013490(0);
            FUN_00013c20(auStack_58,auStack_4bc,auStack_4c8);
            FUN_00163ee0(param_1,&DAT_00208c52);
            FUN_0016599c(param_1);
            FUN_00166204(param_1);
            FUN_0005c474();
            FUN_00013490(0);
            FUN_0005c4a0(auStack_4e0,auStack_4e8);
            plVar6 = (long *)FUN_000029d8(auStack_78);
            local_4f8 = (**(code **)(*plVar6 + 0x28))();
            FUN_001afeec(local_4f8,auStack_4e0);
            FUN_00002cf8(auStack_4fc,0x15d4c7);
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_548,"columns",0);
            local_550 = FUN_00015a7c(&local_48,&local_140);
            FUN_00013490();
            FUN_00013490(0);
            FUN_001668f4(param_1,auStack_4fc,auStack_548,&local_550,&local_140,auStack_558,
                         auStack_560);
            __ZN8PMStringD1Ev(auStack_548);
            FUN_00002cf8(auStack_564,0x15d4c7);
            local_568 = 0;
            FUN_00015b04(auStack_58,auStack_564,&local_568,1);
            FUN_00002cf8(auStack_56c,0x15d4cc);
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_5b8,"columns",0);
            local_5c0 = FUN_00015a7c(&local_48,&local_140);
            FUN_00013490();
            FUN_00013490(0);
            FUN_001668f4(param_1,auStack_56c,auStack_5b8,&local_5c0,&local_140,auStack_5c8,
                         auStack_5d0);
            __ZN8PMStringD1Ev(auStack_5b8);
            FUN_00002cf8(auStack_5d4,0x15d4cc);
            local_5d8 = 0;
            FUN_00015b04(auStack_58,auStack_5d4,&local_5d8,1);
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_620,"[GC-1] Secondary Columns",0);
            local_624 = 0;
            FUN_001b775c(auStack_58,auStack_620,&local_624);
            __ZN8PMStringD1Ev(auStack_620);
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_670,"[GC-1] Subcolumns",0);
            local_674 = 0;
            FUN_001b775c(auStack_58,auStack_670,&local_674);
            __ZN8PMStringD1Ev(auStack_670);
            plVar6 = (long *)FUN_000029d8(auStack_78);
            local_678 = FUN_00032070(&local_48);
            (**(code **)(*plVar6 + 0xf0))(plVar6,&local_678);
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_6c0,"0x15d300kGCSquareKey",0);
            sVar3 = __ZNK8PMString7IsEqualERKS_hh(auStack_208,auStack_6c0,1,0);
            __ZN8PMStringD1Ev(auStack_6c0);
            if (sVar3 == 0) {
              plVar6 = (long *)FUN_000029d8(auStack_78);
              (**(code **)(*plVar6 + 0xa0))(plVar6,&local_50);
            }
            else {
              plVar6 = (long *)FUN_000029d8(auStack_78);
              (**(code **)(*plVar6 + 0xa0))(plVar6,&local_140);
            }
            FUN_00002cf8(auStack_6c4,0x15d326);
            iVar4 = FUN_00013034(auStack_58,auStack_6c4);
            if (iVar4 < 1) {
              plVar6 = (long *)FUN_000029d8(auStack_78);
              local_6c8 = 1;
              (**(code **)(*plVar6 + 0xb0))(plVar6,&local_6c8);
            }
            local_6d0 = FUN_0001e3ac(auStack_58,&local_40,&local_150);
            FUN_00002cf8(auStack_6d4,0x15d320);
            FUN_00013c20(auStack_58,auStack_6d4,&local_6d0);
            FUN_00002cf8(auStack_6d8,0x15d4a5);
            FUN_00013c20(auStack_58,auStack_6d8,&local_6d0);
            FUN_00002cf8(auStack_6dc,0x15d330);
            plVar6 = (long *)FUN_000029d8(auStack_78);
            local_6f0 = (**(code **)(*plVar6 + 0x98))();
            local_6e8 = FUN_00015a7c(&local_6f0,param_1 + 0x88);
            FUN_00013c20(auStack_58,auStack_6dc,&local_6e8);
            FUN_00002cf8(auStack_6f4,0x15d32f);
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_740,"0x15d300kGCCalculateKey",0);
            FUN_00013588(auStack_58,auStack_6f4,auStack_740);
            __ZN8PMStringD1Ev(auStack_740);
            bVar2 = false;
            if (local_8a != 0) {
              FUN_00002cf8(auStack_74c,0x15d32e);
              local_748 = FUN_00013338(auStack_58,auStack_74c);
              FUN_00013490(0);
              sVar3 = FUN_00018504(&local_748,auStack_758);
              bVar2 = sVar3 != 0;
            }
            if (bVar2) {
              FUN_00002cf8(auStack_75c,0x15d32f);
              FUN_00011dc4(auStack_58,auStack_75c,&local_8a);
            }
            else {
              FUN_00002cf8(auStack_760,0x15d32f);
              FUN_00011dc4(auStack_58,auStack_760,&DAT_00208c50);
            }
            FUN_00002cf8(auStack_764,0x15d32e);
            FUN_00011dc4(auStack_58,auStack_764,&local_8a);
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_7f8,"[GC] Desired Grid Width: ",0);
            FUN_001cc9ac(auStack_7b0,auStack_7f8);
            FUN_0000dd8c();
            __ZN8PMStringD1Ev(auStack_7b0);
            __ZN8PMStringD1Ev(auStack_7f8);
            FUN_001b4670(auStack_1b0);
            FUN_00138008(param_1);
            FUN_00002cf8(auStack_7fc,0x15d4a4);
            sVar3 = FUN_00012ef4(auStack_58,auStack_7fc);
            bVar2 = false;
            if (sVar3 == 0) {
              FUN_00002cf8(auStack_800,0x15d4a3);
              sVar3 = FUN_00012ef4(auStack_58,auStack_800);
              bVar2 = sVar3 != 0;
            }
            if (bVar2) {
              FUN_00002cf8(auStack_804,0x15d4e3);
              FUN_00013c20(auStack_58,auStack_804,&local_48);
            }
            FUN_001b5050(auStack_58);
            FUN_0005c530(auStack_4e0);
            local_70 = 0;
          }
          else {
            local_8a = 0;
            FUN_00002cf8(auStack_264,0x15d31a);
            local_260 = FUN_0001544c(auStack_58,auStack_264,auStack_120);
            local_38 = local_260;
            local_278 = FUN_000157c8(&local_38,&local_140);
            local_270 = FUN_00015abc(&local_278);
            local_48 = local_270;
            local_280 = FUN_00015a7c(&local_48,&local_140);
            local_40 = local_280;
            sVar3 = FUN_0003208c(&local_40,&local_38);
            if (sVar3 != 0) {
              FUN_00013490(0x3ff0000000000000);
              FUN_0004ef2c(&local_48,auStack_288);
              local_290 = FUN_00015a7c(&local_48,&local_140);
              local_40 = local_290;
            }
            plVar6 = (long *)FUN_00017c8c(auStack_88);
            (**(code **)(*plVar6 + 0x68))(plVar6,&local_140);
            FUN_00002cf8(auStack_294,0x15d326);
            iVar4 = FUN_00013034(auStack_58,auStack_294);
            if (iVar4 < 1) {
              plVar6 = (long *)FUN_00017c8c(auStack_88);
              local_298 = 1;
              (**(code **)(*plVar6 + 0x78))(plVar6,&local_298);
            }
            FUN_00002cf8(auStack_29c,0x15d324);
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_2e8,"0x15d300kGCUnDoKey",0);
            FUN_00013588(auStack_58,auStack_29c,auStack_2e8);
            __ZN8PMStringD1Ev(auStack_2e8);
            __ZN8PMString12AppendNumberERK6PMRealiss(auStack_d8,&local_38,0xffffffff,0);
            sVar3 = FUN_001a6604(auStack_d8);
            if (sVar3 != 0) goto LAB_00123dcc;
            local_2ec = FUN_001a6844(auStack_d8,&DAT_00208c50);
            if (local_2ec == 0) goto LAB_00123dcc;
            local_70 = 2;
          }
          __ZN8PMStringD1Ev(auStack_208);
        }
        __ZN8PMStringD1Ev(auStack_1b0);
      }
      __ZN8PMStringD1Ev(auStack_120);
      __ZN8PMStringD1Ev(auStack_d8);
    }
    else {
      local_70 = 2;
    }
    FUN_00017158(auStack_88);
  }
  else {
    local_70 = 2;
  }
  FUN_00002b9c(auStack_78);
LAB_00124958:
  FUN_00002c54(auStack_58);
  return;
}
