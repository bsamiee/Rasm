/*
Vertical Value mode (row 14). FUN_00017184 is the vertical unit: with the checkbox on, T = the type-area height field 0x15d4a9
(H when blank), L_fit = T / Round(T / L), or (T + x) / Round((T + x) / L) with image-lines on, quantised to 0.001.
FUN_00166204 fills that field: T = H - m_t - m_b and the width W - m_i - m_o from the value fields 0x15d49b/0x15d49d and
0x15d49f/0x15d4a1. FUN_0012fc3c is the value-field observer that reapplies the fit and derives u_h = L_fit * W_t / T (+ x).
FUN_000e8428 is the apply routine after the leading observer; its branch at the vertical Value checkbox holds the same fit.

PMReal helpers: construct 0x13490, operator/ 0x157c8, operator* 0x15a7c, operator+ 0x17c4c, operator- 0x28080,
Round 0x15abc (floor(x + 0.5)), floor 0x37d70, a > b + 1e-8 0x14140, a >= b - 1e-8 0x15808, a <= b + 1e-8 0x31d38,
a < b - 1e-8 0x3208c, |a - b| < 1e-8 0x132fc, not equal 0x18504, quantise 0x152f0 (mode 3 three decimals, mode 4 four).
Widget access: read value 0x13338, read value in the panel unit 0x1544c, read checkbox 0x12ef4, read dropdown index 0x13034,
write value 0x13c20, set maximum 0x196d0, unit factor at object + 0x88.
*/

//==== FUNC @ 17184 -> 00017184

undefined1  [16] FUN_00017184(undefined8 param_1)

{
  bool bVar1;
  short sVar2;
  int iVar3;
  undefined8 uVar4;
  long *plVar5;
  undefined1 auVar6 [16];
  undefined1 auStack_2b4 [4];
  undefined8 local_2b0;
  undefined8 local_2a8;
  undefined8 local_2a0;
  undefined8 local_298;
  undefined1 auStack_290 [12];
  undefined1 auStack_284 [4];
  undefined8 local_280;
  undefined8 local_278;
  undefined8 local_270;
  undefined8 local_268;
  undefined1 auStack_260 [12];
  undefined1 auStack_254 [4];
  undefined8 local_250;
  byte local_241;
  undefined1 auStack_240 [72];
  undefined1 auStack_1f8 [76];
  undefined1 auStack_1ac [4];
  ulong local_1a8;
  undefined8 local_1a0;
  undefined8 local_198;
  ulong local_190;
  undefined1 auStack_188 [8];
  ulong local_180;
  undefined1 auStack_178 [12];
  undefined4 local_16c;
  undefined8 local_168;
  undefined8 local_160;
  ulong local_158;
  undefined8 local_150;
  undefined8 local_148;
  undefined8 local_140;
  undefined8 local_138;
  ulong local_130;
  undefined1 auStack_128 [8];
  undefined1 auStack_120 [6];
  short local_11a;
  undefined8 local_118;
  undefined1 auStack_110 [12];
  undefined1 auStack_104 [4];
  undefined8 local_100;
  undefined1 auStack_f8 [4];
  undefined1 auStack_f4 [4];
  int local_f0;
  undefined1 auStack_ec [6];
  short local_e6;
  undefined1 auStack_e4 [4];
  ulong local_e0;
  undefined1 auStack_d4 [4];
  undefined8 local_d0;
  undefined4 local_c8;
  undefined1 auStack_c4 [4];
  undefined1 auStack_c0 [79];
  undefined1 uStack_71;
  undefined1 auStack_70 [8];
  undefined1 local_68 [16];
  undefined1 uStack_51;
  undefined1 auStack_50 [8];
  undefined4 local_48;
  undefined1 auStack_38 [8];
  undefined8 local_30;
  ulong local_28;
  
  local_30 = param_1;
  FUN_00013490(0,&local_28);
  sVar2 = FUN_00002c30(local_30);
  if (sVar2 == 0) {
    FUN_001a7968();
    sVar2 = FUN_00016b78(auStack_38);
    if (sVar2 == 0) {
      uVar4 = __Z26GetExecutionContextSessionv();
      FUN_00002978(auStack_50,uVar4,&uStack_51);
      sVar2 = FUN_000029b4(auStack_50);
      if (sVar2 == 0) {
        plVar5 = (long *)FUN_000029d8(auStack_50);
        local_68 = (**(code **)(*plVar5 + 0x28))();
        FUN_00017bd4(auStack_70,local_68,&uStack_71);
        sVar2 = FUN_00017c10(auStack_70);
        uVar4 = local_30;
        if (sVar2 == 0) {
          FUN_00002cf8(auStack_c4,0x15d306);
          local_c8 = 0xffffffff;
          FUN_00012c10(auStack_c0,uVar4,auStack_c4,&local_c8);
          uVar4 = local_30;
          FUN_00002cf8(auStack_d4,&DAT_0015d31b);
          local_d0 = FUN_0001544c(uVar4,auStack_d4,auStack_c0);
          uVar4 = local_30;
          FUN_00002cf8(auStack_e4,0x15d31c);
          local_e0 = FUN_00013338(uVar4,auStack_e4);
          uVar4 = local_30;
          FUN_00002cf8(auStack_ec,0x15d323);
          local_e6 = FUN_00012ef4(uVar4,auStack_ec);
          uVar4 = local_30;
          FUN_00002cf8(auStack_f4,0x15d326);
          local_f0 = FUN_00013034(uVar4,auStack_f4);
          uVar4 = local_30;
          FUN_00002cf8(auStack_f8,0x15d4a3);
          sVar2 = FUN_00012ef4(uVar4,auStack_f8);
          uVar4 = local_30;
          if (sVar2 == 0) {
            FUN_00013490(0);
            sVar2 = FUN_00014140(&local_e0,auStack_178);
            uVar4 = local_30;
            if (sVar2 == 0) {
              local_241 = 0;
              __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1f8,"[GC] CGS");
              sVar2 = FUN_001a6604(auStack_1f8);
              bVar1 = true;
              if (sVar2 == 0) {
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_240,"[GC] Smart Setup",0);
                local_241 = 1;
                sVar2 = FUN_001a6604(auStack_240);
                bVar1 = sVar2 != 0;
              }
              if ((local_241 & 1) != 0) {
                __ZN8PMStringD1Ev(auStack_240);
              }
              __ZN8PMStringD1Ev(auStack_1f8);
              uVar4 = local_30;
              if (bVar1) {
                FUN_00002cf8(auStack_254,0x15d342);
                local_250 = FUN_00013338(uVar4,auStack_254);
                FUN_00013490(0);
                sVar2 = FUN_000132fc(&local_250,auStack_260);
                if (sVar2 != 0) {
                  plVar5 = (long *)FUN_00017c8c(auStack_38);
                  local_278 = (**(code **)(*plVar5 + 0x90))();
                  local_270 = FUN_000157c8(&local_d0,&local_278);
                  local_268 = FUN_00015abc(&local_270);
                  local_250 = local_268;
                }
                uVar4 = local_30;
                FUN_00002cf8(auStack_284,0x15d344);
                local_280 = FUN_00013338(uVar4,auStack_284);
                FUN_00013490(0);
                sVar2 = FUN_000132fc(&local_280,auStack_290);
                if (sVar2 != 0) {
                  plVar5 = (long *)FUN_00017c8c(auStack_38);
                  iVar3 = (**(code **)(*plVar5 + 0xa0))();
                  FUN_00013490((double)iVar3,&local_298);
                  local_280 = local_298;
                }
                local_2a8 = FUN_000157c8(&local_d0,&local_250);
                local_2a0 = FUN_000157c8(&local_2a8,&local_280);
                uVar4 = local_30;
                FUN_00002cf8(auStack_2b4,0x15d347);
                local_2b0 = FUN_00013338(uVar4,auStack_2b4);
                local_28 = FUN_00015a7c(&local_2a0,&local_2b0);
              }
            }
            else if (local_e6 == 0) {
              local_28 = local_e0;
            }
            else if (local_f0 < 5) {
              FUN_00002cf8(auStack_1ac,0x15d31d);
              local_1a8 = FUN_00013338(uVar4,auStack_1ac);
              local_28 = local_1a8;
            }
            else {
              FUN_00013490((double)(local_f0 + -3));
              local_180 = FUN_00015a7c(&local_e0,auStack_188);
              local_28 = local_180;
              local_1a0 = FUN_000157c8(&local_d0,&local_28);
              local_198 = FUN_00015abc(&local_1a0);
              local_190 = FUN_000157c8(&local_d0,&local_198);
              local_28 = local_190;
            }
          }
          else {
            FUN_00002cf8(auStack_104,0x15d4a9);
            local_100 = FUN_0001544c(uVar4,auStack_104,auStack_c0);
            FUN_00013490(0);
            sVar2 = FUN_000132fc(&local_100,auStack_110);
            if (sVar2 != 0) {
              local_100 = local_d0;
            }
            plVar5 = (long *)FUN_00017c34(auStack_70);
            local_118 = (**(code **)(*plVar5 + 0x2c0))();
            uVar4 = local_30;
            FUN_00002cf8(auStack_120,0x15d3a9);
            local_11a = FUN_00012ef4(uVar4,auStack_120);
            FUN_00013490(0);
            sVar2 = FUN_00014140(&local_118,auStack_128);
            if (sVar2 == 0 || local_11a == 0) {
              local_168 = FUN_000157c8(&local_100,&local_e0);
              local_160 = FUN_00015abc(&local_168);
              local_158 = FUN_000157c8(&local_100,&local_160);
              local_28 = local_158;
            }
            else {
              local_138 = FUN_00017c4c(&local_100,&local_118);
              local_150 = FUN_00017c4c(&local_100,&local_118);
              local_148 = FUN_000157c8(&local_150,&local_e0);
              local_140 = FUN_00015abc(&local_148);
              local_130 = FUN_000157c8(&local_138,&local_140);
              local_28 = local_130;
            }
            local_16c = 3;
            FUN_000152f0(&local_28,&local_16c);
          }
          __ZN8PMStringD1Ev(auStack_c0);
          local_48 = 0;
        }
        else {
          local_48 = 2;
        }
        FUN_00017ca4(auStack_70);
      }
      else {
        local_48 = 2;
      }
      FUN_00002b9c(auStack_50);
    }
    else {
      local_48 = 2;
    }
    FUN_00017158(auStack_38);
  }
  auVar6._8_8_ = 0;
  auVar6._0_8_ = local_28;
  return auVar6;
}

//==== FUNC @ 1665d4 -> 00166204

void FUN_00166204(undefined8 param_1)

{
  bool bVar1;
  short sVar2;
  undefined8 uVar3;
  undefined1 auStack_278 [4];
  undefined1 auStack_274 [4];
  undefined1 auStack_270 [72];
  undefined1 auStack_228 [4];
  undefined1 auStack_224 [4];
  undefined8 local_220;
  undefined1 auStack_218 [8];
  undefined1 auStack_210 [12];
  undefined1 auStack_204 [4];
  undefined8 local_200;
  undefined1 auStack_1f8 [8];
  undefined1 auStack_1f0 [8];
  undefined1 auStack_1e8 [76];
  undefined1 auStack_19c [4];
  undefined1 auStack_198 [76];
  undefined1 auStack_14c [4];
  undefined1 auStack_148 [76];
  undefined1 auStack_fc [4];
  undefined1 auStack_f8 [8];
  undefined1 auStack_f0 [8];
  undefined1 auStack_e8 [8];
  undefined1 auStack_e0 [8];
  undefined8 local_d8;
  undefined8 local_d0;
  undefined1 auStack_c4 [4];
  undefined8 local_c0;
  undefined8 local_b8;
  undefined8 local_b0;
  undefined1 auStack_a4 [4];
  undefined8 local_a0;
  undefined1 auStack_94 [4];
  undefined8 local_90;
  undefined1 auStack_84 [4];
  undefined8 local_80;
  undefined1 auStack_74 [4];
  undefined8 local_70;
  undefined1 auStack_64 [4];
  undefined8 local_60;
  undefined1 uStack_51;
  undefined1 auStack_50 [8];
  undefined4 local_48;
  undefined1 uStack_31;
  undefined1 auStack_30 [8];
  undefined8 local_28;
  
  local_28 = param_1;
  FUN_00002bf4(auStack_30,param_1,&uStack_31);
  sVar2 = FUN_00002c30(auStack_30);
  if (sVar2 == 0) {
    uVar3 = __Z26GetExecutionContextSessionv();
    FUN_00002978(auStack_50,uVar3,&uStack_51);
    sVar2 = FUN_000029b4(auStack_50);
    if (sVar2 == 0) {
      FUN_00002cf8(auStack_64,0x15d49b);
      local_60 = FUN_00013338(auStack_30,auStack_64);
      FUN_00002cf8(auStack_74,0x15d49d);
      local_70 = FUN_00013338(auStack_30,auStack_74);
      FUN_00002cf8(auStack_84,0x15d49f);
      local_80 = FUN_00013338(auStack_30,auStack_84);
      FUN_00002cf8(auStack_94,0x15d4a1);
      local_90 = FUN_00013338(auStack_30,auStack_94);
      FUN_00002cf8(auStack_a4,&DAT_0015d31b);
      local_a0 = FUN_00013338(auStack_30,auStack_a4);
      local_b8 = FUN_00017c4c(&local_60,&local_70);
      local_b0 = FUN_00028080(&local_a0,&local_b8);
      FUN_00002cf8(auStack_c4,&DAT_0015d31a);
      local_c0 = FUN_00013338(auStack_30,auStack_c4);
      local_d8 = FUN_00017c4c(&local_80,&local_90);
      local_d0 = FUN_00028080(&local_c0,&local_d8);
      FUN_00013490(0);
      sVar2 = FUN_000132fc(&local_60,auStack_e0);
      bVar1 = false;
      if (sVar2 != 0) {
        FUN_00013490(0);
        sVar2 = FUN_000132fc(&local_70,auStack_e8);
        bVar1 = false;
        if (sVar2 != 0) {
          FUN_00013490(0);
          sVar2 = FUN_000132fc(&local_80,auStack_f0);
          bVar1 = false;
          if (sVar2 != 0) {
            FUN_00013490(0);
            sVar2 = FUN_000132fc(&local_90,auStack_f8);
            bVar1 = sVar2 != 0;
          }
        }
      }
      if (bVar1) {
        FUN_00002cf8(auStack_fc,0x15d4a7);
        __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_148,"",0);
        FUN_00013588(auStack_30,auStack_fc,auStack_148);
        __ZN8PMStringD1Ev(auStack_148);
        FUN_00002cf8(auStack_14c,0x15d4a8);
        __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_198,"",0);
        FUN_00013588(auStack_30,auStack_14c,auStack_198);
        __ZN8PMStringD1Ev(auStack_198);
        FUN_00002cf8(auStack_19c,0x15d4a9);
        __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1e8,"",0);
        FUN_00013588(auStack_30,auStack_19c,auStack_1e8);
        __ZN8PMStringD1Ev(auStack_1e8);
        local_48 = 1;
      }
      else {
        FUN_00013490(0);
        sVar2 = FUN_00014140(&local_a0,auStack_1f0);
        bVar1 = false;
        if (sVar2 != 0) {
          FUN_00013490(0);
          sVar2 = FUN_00014140(&local_c0,auStack_1f8);
          bVar1 = sVar2 != 0;
        }
        if (bVar1) {
          local_200 = FUN_0001e3ac(auStack_30,&local_c0,&local_a0);
          FUN_00002cf8(auStack_204,0x15d4a5);
          FUN_00013c20(auStack_30,auStack_204,&local_200);
        }
        FUN_00013490(0);
        sVar2 = FUN_00014140(&local_b0,auStack_210);
        bVar1 = false;
        if (sVar2 != 0) {
          FUN_00013490(0);
          sVar2 = FUN_00014140(&local_d0,auStack_218);
          bVar1 = sVar2 != 0;
        }
        if (bVar1) {
          local_220 = FUN_0001e3ac(auStack_30,&local_d0,&local_b0);
          FUN_00002cf8(auStack_224,0x15d4a7);
          FUN_00013c20(auStack_30,auStack_224,&local_220);
        }
        else {
          FUN_00002cf8(auStack_228,0x15d4a7);
          __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_270,"",0);
          FUN_00013588(auStack_30,auStack_228,auStack_270);
          __ZN8PMStringD1Ev(auStack_270);
        }
        FUN_00002cf8(auStack_274,0x15d4a8);
        FUN_00013c20(auStack_30,auStack_274,&local_d0);
        FUN_00002cf8(auStack_278,0x15d4a9);
        FUN_00013c20(auStack_30,auStack_278,&local_b0);
        local_48 = 0;
      }
    }
    else {
      local_48 = 2;
    }
    FUN_00002b9c(auStack_50);
  }
  else {
    local_48 = 2;
  }
  FUN_00002c54(auStack_30);
  return;
}

//==== FUNC @ 12fc3c -> 0012fc3c

/* WARNING: Restarted to delay deadcode elimination for space: stack */

void FUN_0012fc3c(long param_1,undefined8 param_2,short *param_3)

{
  bool bVar1;
  short sVar2;
  int iVar3;
  undefined8 uVar4;
  long *plVar5;
  ulong uVar6;
  long *plVar7;
  long lVar8;
  undefined1 auStack_734 [4];
  undefined4 local_730;
  undefined1 auStack_72c [4];
  undefined1 auStack_728 [76];
  undefined4 local_6dc;
  undefined1 auStack_6d8 [28];
  undefined4 local_6bc;
  undefined1 auStack_6b8 [8];
  undefined1 auStack_6b0 [8];
  undefined1 auStack_6a8 [76];
  undefined1 auStack_65c [4];
  undefined8 local_658;
  undefined4 local_650;
  undefined1 auStack_64c [4];
  undefined1 auStack_648 [72];
  undefined1 auStack_600 [4];
  undefined1 auStack_5fc [4];
  undefined1 auStack_5f8 [12];
  undefined1 auStack_5ec [4];
  undefined1 auStack_5e8 [4];
  undefined1 auStack_5e4 [4];
  undefined1 auStack_5e0 [12];
  undefined1 auStack_5d4 [4];
  undefined1 auStack_5d0 [12];
  undefined1 auStack_5c4 [4];
  undefined4 local_5c0;
  undefined1 auStack_5bc [4];
  undefined4 local_5b8;
  undefined1 auStack_5b4 [4];
  undefined1 auStack_5b0 [4];
  undefined1 auStack_5ac [4];
  undefined8 local_5a8;
  undefined1 auStack_5a0 [8];
  undefined8 local_598;
  undefined8 local_590;
  undefined8 local_588;
  undefined8 local_580;
  undefined1 auStack_574 [4];
  undefined8 local_570;
  undefined1 auStack_568 [8];
  undefined8 local_560;
  undefined8 local_558;
  undefined1 auStack_54c [4];
  undefined8 local_548;
  undefined1 auStack_53c [4];
  undefined8 local_538;
  undefined8 local_530;
  undefined1 auStack_528 [8];
  undefined1 auStack_520 [4];
  undefined1 auStack_51c [4];
  undefined8 local_518;
  undefined1 auStack_50c [4];
  undefined8 local_508;
  undefined4 local_500;
  undefined4 local_4fc;
  undefined4 local_4f8;
  undefined4 local_4f4;
  undefined8 local_4f0;
  undefined1 auStack_4e8 [4];
  undefined4 local_4e4;
  undefined8 local_4e0;
  undefined8 local_4d8;
  undefined8 local_4d0;
  undefined8 local_4c8;
  undefined8 local_4c0;
  undefined8 local_4b8;
  undefined8 local_4b0;
  undefined1 auStack_4a4 [4];
  undefined1 auStack_4a0 [76];
  undefined1 auStack_454 [4];
  undefined1 auStack_450 [78];
  short local_402;
  undefined8 local_400;
  undefined8 local_3f8;
  undefined1 auStack_3ec [4];
  undefined8 local_3e8;
  undefined1 auStack_3dc [4];
  undefined8 local_3d8;
  undefined2 local_3ca;
  undefined1 auStack_3c8 [76];
  undefined1 auStack_37c [4];
  undefined1 auStack_378 [72];
  undefined1 auStack_330 [4];
  undefined4 local_32c;
  undefined8 local_328;
  undefined8 local_320;
  undefined8 local_318;
  undefined8 local_310;
  undefined8 local_308;
  undefined8 local_300;
  undefined8 local_2f8;
  undefined8 local_2f0;
  undefined1 auStack_2e4 [4];
  undefined1 auStack_2e0 [6];
  short local_2da;
  undefined8 local_2d8;
  undefined1 auStack_2cc [4];
  undefined8 local_2c8;
  undefined1 auStack_2bc [4];
  undefined8 local_2b8;
  undefined1 auStack_2b0 [12];
  undefined1 auStack_2a4 [4];
  undefined8 local_2a0;
  undefined1 auStack_294 [4];
  undefined8 local_290;
  undefined1 auStack_288 [12];
  undefined1 auStack_27c [4];
  undefined8 local_278;
  undefined1 auStack_270 [8];
  undefined8 local_268;
  undefined8 local_260;
  int local_258;
  byte local_251;
  undefined1 auStack_250 [79];
  byte local_201;
  undefined1 auStack_200 [76];
  undefined1 auStack_1b4 [4];
  undefined1 auStack_1b0 [8];
  undefined1 auStack_1a8 [8];
  undefined1 auStack_1a0 [8];
  undefined1 auStack_198 [4];
  undefined1 auStack_194 [4];
  undefined8 local_190;
  undefined8 local_188;
  undefined1 auStack_17c [4];
  undefined8 local_178;
  undefined8 local_170;
  undefined1 auStack_168 [8];
  undefined1 auStack_160 [8];
  undefined8 local_158;
  undefined8 local_150;
  undefined8 local_148;
  undefined1 auStack_140 [12];
  undefined1 auStack_134 [4];
  undefined4 local_130;
  undefined1 auStack_12c [4];
  undefined1 auStack_128 [72];
  undefined8 local_e0;
  undefined1 auStack_d8 [8];
  undefined8 local_d0;
  undefined8 local_c8;
  undefined1 auStack_c0 [8];
  int local_b8;
  undefined1 auStack_b4 [4];
  undefined8 local_b0;
  undefined8 local_a8;
  undefined8 local_a0;
  undefined1 auStack_94 [4];
  undefined8 local_90;
  undefined1 uStack_81;
  undefined1 local_80 [16];
  undefined1 auStack_70 [15];
  undefined1 uStack_61;
  undefined1 auStack_60 [8];
  undefined4 local_58;
  undefined1 uStack_41;
  undefined1 auStack_40 [8];
  short *local_38;
  undefined8 local_30;
  long local_28;
  
  local_38 = param_3;
  local_30 = param_2;
  local_28 = param_1;
  FUN_00002bf4(auStack_40,param_1,&uStack_41);
  sVar2 = FUN_00002c30(auStack_40);
  if (sVar2 == 0) {
    uVar4 = __Z26GetExecutionContextSessionv();
    FUN_00002978(auStack_60,uVar4,&uStack_61);
    sVar2 = FUN_000029b4(auStack_60);
    if (sVar2 == 0) {
      plVar5 = (long *)FUN_000029d8(auStack_60);
      local_80 = (**(code **)(*plVar5 + 0x28))();
      FUN_00017bd4(auStack_70,local_80,&uStack_81);
      sVar2 = FUN_00017c10(auStack_70);
      if (sVar2 == 0) {
        FUN_00002cf8(auStack_94,0x15d31b);
        local_90 = FUN_00013338(auStack_40,auStack_94);
        plVar5 = (long *)FUN_00017c34(auStack_70);
        local_a0 = (**(code **)(*plVar5 + 0x2c0))();
        FUN_0015d6f4(param_1);
        local_a8 = FUN_00013338(auStack_40,local_30);
        FUN_00002cf8(auStack_b4,0x15d31d);
        local_b0 = FUN_00013338(auStack_40,auStack_b4);
        plVar5 = (long *)FUN_000029d8(auStack_60);
        local_b8 = (**(code **)(*plVar5 + 0xf8))();
        FUN_00013490(0);
        sVar2 = FUN_00014140(&local_b0,auStack_c0);
        if (sVar2 == 0 || local_b8 != 0) {
          FUN_00013490((double)(long)local_b8);
          local_d0 = FUN_00015a7c(auStack_d8,&local_b0);
          local_e0 = FUN_000157c8(&local_a8,param_1 + 0x88);
          local_c8 = FUN_00028080(&local_d0,&local_e0);
          FUN_00068460(&local_c8,param_1 + 0x88);
          FUN_00002cf8(auStack_12c,0x15d309);
          local_130 = 0xffffffff;
          FUN_00012c10(auStack_128,auStack_40,auStack_12c,&local_130);
          __ZN8PMString6AppendEPKciNS_14StringEncodingE
                    (auStack_128," Image-lines",0x7fffffff,0xffffffff);
          uVar4 = local_30;
          FUN_00002cf8(auStack_134,0x15d49b);
          uVar6 = FUN_0001417c(uVar4,auStack_134);
          uVar4 = local_30;
          if ((uVar6 & 1) == 0) {
            FUN_00002cf8(auStack_194,0x15d49d);
            uVar6 = FUN_0001417c(uVar4,auStack_194);
            if ((uVar6 & 1) != 0) {
              FUN_00002cf8(auStack_198,0x15d49b);
              FUN_000196d0(auStack_40,auStack_198,&local_c8);
            }
          }
          else {
            sVar2 = FUN_001a6604(auStack_128);
            uVar4 = local_30;
            if (sVar2 == 0) {
              local_178 = FUN_00015a7c(&local_a0,param_1 + 0x88);
              local_170 = FUN_00017c4c(&local_a8,&local_178);
              FUN_00013c20(auStack_40,uVar4,&local_170);
            }
            else {
              FUN_00013490(0);
              sVar2 = FUN_00014140(&local_a8,auStack_140);
              bVar1 = false;
              if (sVar2 != 0) {
                bVar1 = *local_38 != 0;
              }
              if (bVar1) {
                plVar5 = (long *)FUN_00017c34(auStack_70);
                local_148 = (**(code **)(*plVar5 + 0x2d0))();
                local_158 = FUN_00015a7c(&local_148,param_1 + 0x88);
                local_150 = FUN_00028080(&local_a8,&local_158);
                FUN_00013490(0);
                sVar2 = FUN_00015808(&local_150,auStack_160);
                uVar4 = local_30;
                if (sVar2 == 0) {
                  FUN_00013490(0);
                  FUN_00013c20(auStack_40,uVar4,auStack_168);
                }
                else {
                  FUN_00013c20(auStack_40,local_30,&local_150);
                }
              }
            }
            FUN_00002cf8(auStack_17c,0x15d49d);
            local_190 = FUN_00015a7c(&local_a0,param_1 + 0x88);
            local_188 = FUN_00017c4c(&local_c8,&local_190);
            FUN_000196d0(auStack_40,auStack_17c,&local_188);
          }
          FUN_001a7968(auStack_1a0);
          sVar2 = FUN_00016b78(auStack_1a0);
          if (sVar2 == 0) {
            FUN_001a7c08(auStack_1a8);
            sVar2 = FUN_0000e280(auStack_1a8);
            if (sVar2 == 0) {
              FUN_001a264c(auStack_1b0);
              sVar2 = FUN_00030ab8(auStack_1b0);
              if (sVar2 == 0) {
                FUN_0015d6f4(param_1);
                FUN_00166204(param_1);
                FUN_00002cf8(auStack_1b4,0x15d323);
                local_201 = 0;
                local_251 = 0;
                sVar2 = FUN_00012ef4(auStack_40,auStack_1b4);
                bVar1 = false;
                if (sVar2 != 0) {
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_200,"[GC] Smart Setup",0);
                  local_201 = 1;
                  sVar2 = FUN_001a6604(auStack_200);
                  bVar1 = false;
                  if (sVar2 == 0) {
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_250,"[GC] CGS",0);
                    local_251 = 1;
                    sVar2 = FUN_001a6604(auStack_250);
                    bVar1 = sVar2 == 0;
                  }
                }
                if ((local_251 & 1) != 0) {
                  __ZN8PMStringD1Ev(auStack_250);
                }
                if ((local_201 & 1) != 0) {
                  __ZN8PMStringD1Ev(auStack_200);
                }
                if (bVar1) {
                  plVar5 = (long *)FUN_000029d8(auStack_60);
                  local_258 = (**(code **)(*plVar5 + 0xe8))();
                  plVar5 = (long *)FUN_000029d8(auStack_60);
                  local_268 = (**(code **)(*plVar5 + 0x98))();
                  plVar5 = (long *)FUN_000029d8(auStack_60);
                  iVar3 = (**(code **)(*plVar5 + 0xa8))();
                  FUN_00013490((double)iVar3);
                  local_260 = FUN_000157c8(&local_268,auStack_270);
                  FUN_00002cf8(auStack_27c,0x15d4a9);
                  local_278 = FUN_00013338(auStack_40,auStack_27c);
                  FUN_00013490(0);
                  sVar2 = FUN_000132fc(&local_278,auStack_288);
                  if (sVar2 != 0) {
                    FUN_00002cf8(auStack_294,0x15d31b);
                    local_290 = FUN_00013338(auStack_40,auStack_294);
                    local_278 = local_290;
                  }
                  FUN_00002cf8(auStack_2a4,0x15d4a8);
                  local_2a0 = FUN_00013338(auStack_40,auStack_2a4);
                  FUN_00013490(0);
                  sVar2 = FUN_000132fc(&local_2a0,auStack_2b0);
                  if (sVar2 != 0) {
                    FUN_00002cf8(auStack_2bc,&DAT_0015d31a);
                    local_2b8 = FUN_00013338(auStack_40,auStack_2bc);
                    local_2a0 = local_2b8;
                  }
                  FUN_000afc88(&local_278,param_1 + 0x88);
                  FUN_000afc88(&local_2a0,param_1 + 0x88);
                  FUN_00002cf8(auStack_2cc,0x15d31c);
                  local_2c8 = FUN_00013338(auStack_40,auStack_2cc);
                  FUN_00013490(0,&local_2d8);
                  FUN_00002cf8(auStack_2e0,0x15d4a3);
                  local_2da = FUN_00012ef4(auStack_40,auStack_2e0);
                  bVar1 = false;
                  if (local_2da != 0) {
                    FUN_00002cf8(auStack_2e4,0x15d3a9);
                    sVar2 = FUN_00012ef4(auStack_40,auStack_2e4);
                    bVar1 = sVar2 != 0;
                  }
                  if (bVar1) {
                    local_2f8 = FUN_00017c4c(&local_278,&local_a0);
                    local_310 = FUN_00017c4c(&local_278,&local_a0);
                    local_308 = FUN_000157c8(&local_310,&local_2c8);
                    local_300 = FUN_00015abc(&local_308);
                    local_2f0 = FUN_000157c8(&local_2f8,&local_300);
                    local_2d8 = local_2f0;
                  }
                  else {
                    local_328 = FUN_000157c8(&local_278,&local_2c8);
                    local_320 = FUN_00015abc(&local_328);
                    local_318 = FUN_000157c8(&local_278,&local_320);
                    local_2d8 = local_318;
                  }
                  FUN_00015c98(auStack_40,&local_2d8);
                  local_32c = 3;
                  FUN_000152f0(&local_2d8,&local_32c);
                  FUN_00002cf8(auStack_330,0x15d31d);
                  FUN_00013c20(auStack_40,auStack_330,&local_2d8);
                  FUN_00002cf8(auStack_37c,0x15d30a);
                  FUN_000138a8(auStack_378,auStack_40,auStack_37c);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                            (auStack_3c8,"0x15d300kGCHideGridKey",0);
                  sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_378,auStack_3c8,1,0);
                  __ZN8PMStringD1Ev(auStack_3c8);
                  __ZN8PMStringD1Ev(auStack_378);
                  if (sVar2 == 0) {
                    FUN_001ae290(auStack_40,&local_2d8,&DAT_00208c52);
                  }
                  else {
                    FUN_001ae290(auStack_40,&local_2d8,&DAT_00208c50);
                  }
                  plVar5 = (long *)FUN_000029d8(auStack_60);
                  plVar7 = (long *)FUN_0000e2e0(auStack_1a8);
                  local_3ca = (**(code **)(*plVar7 + 0x20))();
                  (**(code **)(*plVar5 + 0x110))(plVar5,&local_3ca);
                  plVar5 = (long *)FUN_0000e2e0(auStack_1a8);
                  sVar2 = (**(code **)(*plVar5 + 0x20))();
                  if (sVar2 == 0) {
                    plVar5 = (long *)FUN_000029d8(auStack_60);
                    (**(code **)(*plVar5 + 0x120))(plVar5,&DAT_00208c50);
                  }
                  plVar5 = (long *)FUN_0000e2e0(auStack_1a8);
                  (**(code **)(*plVar5 + 0x28))(plVar5,&local_2d8);
                  FUN_00002cf8(auStack_3dc,0x15d31b);
                  local_3d8 = FUN_00013338(auStack_40,auStack_3dc);
                  FUN_00002cf8(auStack_3ec,&DAT_0015d31a);
                  local_3e8 = FUN_00013338(auStack_40,auStack_3ec);
                  FUN_000afc88(&local_3d8,param_1 + 0x88);
                  FUN_000afc88(&local_3e8,param_1 + 0x88);
                  local_400 = FUN_000157c8(&local_278,&local_2d8);
                  local_3f8 = FUN_00015abc(&local_400);
                  local_b8 = FUN_00032070(&local_3f8);
                  local_402 = 0;
                  FUN_00002cf8(auStack_454,0x15d32f);
                  FUN_000138a8(auStack_450,auStack_40,auStack_454);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                            (auStack_4a0,"0x15d300kGCCalculateKey",0);
                  sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_450,auStack_4a0,1,0);
                  __ZN8PMStringD1Ev(auStack_4a0);
                  __ZN8PMStringD1Ev(auStack_450);
                  if (sVar2 != 0) {
                    bVar1 = false;
                    if (local_2da != 0) {
                      FUN_00002cf8(auStack_4a4,0x15d3a9);
                      sVar2 = FUN_00012ef4(auStack_40,auStack_4a4);
                      bVar1 = sVar2 != 0;
                    }
                    if (bVar1) {
                      local_4c0 = FUN_00017c4c(&local_278,&local_a0);
                      local_4b8 = FUN_000157c8(&local_3e8,&local_4c0);
                      local_4b0 = FUN_00015a7c(&local_2d8,&local_4b8);
                      local_260 = local_4b0;
                    }
                    else {
                      local_4d0 = FUN_000157c8(&local_3e8,&local_278);
                      local_4c8 = FUN_00015a7c(&local_2d8,&local_4d0);
                      local_260 = local_4c8;
                    }
                    local_4e0 = FUN_000157c8(&local_3e8,&local_260);
                    local_4d8 = FUN_00015abc(&local_4e0);
                    local_258 = FUN_00032070(&local_4d8);
                    local_4e4 = 3;
                    FUN_000152f0(&local_260,&local_4e4);
                    plVar5 = (long *)FUN_000029d8(auStack_60);
                    (**(code **)(*plVar5 + 0xf0))(plVar5,&local_258);
                    plVar5 = (long *)FUN_000029d8(auStack_60);
                    (**(code **)(*plVar5 + 0xa0))(plVar5,&local_260);
                    plVar5 = (long *)FUN_00017c8c(auStack_1a0);
                    (**(code **)(*plVar5 + 0x68))(plVar5,&local_260);
                    FUN_00002cf8(auStack_4e8,0x15d330);
                    local_4f0 = FUN_00015a7c(&local_260,param_1 + 0x88);
                    FUN_00013c20(auStack_40,auStack_4e8,&local_4f0);
                    local_402 = 1;
                  }
                  plVar5 = (long *)FUN_000029d8(auStack_60);
                  (**(code **)(*plVar5 + 0x100))(plVar5,&local_b8);
                  plVar5 = (long *)FUN_000029d8(auStack_60);
                  local_4f4 = 1;
                  (**(code **)(*plVar5 + 0xb0))(plVar5,&local_4f4);
                  plVar5 = (long *)FUN_000029d8(auStack_60);
                  (**(code **)(*plVar5 + 0xd0))(plVar5,&local_2d8);
                  plVar5 = (long *)FUN_000029d8(auStack_60);
                  local_4f8 = 1;
                  (**(code **)(*plVar5 + 0xe0))(plVar5,&local_4f8);
                  plVar5 = (long *)FUN_00017c8c(auStack_1a0);
                  local_4fc = 1;
                  (**(code **)(*plVar5 + 0x78))(plVar5,&local_4fc);
                  plVar5 = (long *)FUN_00017c8c(auStack_1a0);
                  (**(code **)(*plVar5 + 0x88))(plVar5,&local_2d8);
                  plVar5 = (long *)FUN_00017c8c(auStack_1a0);
                  local_500 = 1;
                  (**(code **)(*plVar5 + 0x98))(plVar5,&local_500);
                  plVar5 = (long *)FUN_00030cd4(auStack_1b0);
                  FUN_00002cf8(auStack_50c,0x15d31c);
                  local_508 = FUN_00013338(auStack_40,auStack_50c);
                  (**(code **)(*plVar5 + 0x38))(plVar5,&local_508);
                  plVar5 = (long *)FUN_00030cd4(auStack_1b0);
                  FUN_00002cf8(auStack_51c,0x15d31d);
                  local_518 = FUN_00013338(auStack_40,auStack_51c);
                  (**(code **)(*plVar5 + 0x48))(plVar5,&local_518);
                  FUN_00002cf8(auStack_520,0x15d4e6);
                  local_530 = FUN_00015abc(&local_2d8);
                  iVar3 = FUN_00032070(&local_530);
                  FUN_00013490((double)iVar3);
                  FUN_000196d0(auStack_40,auStack_520,auStack_528);
                  FUN_00002cf8(auStack_53c,0x15d49b);
                  local_538 = FUN_00013338(auStack_40,auStack_53c);
                  FUN_00002cf8(auStack_54c,0x15d49d);
                  local_548 = FUN_00013338(auStack_40,auStack_54c);
                  FUN_00013490((double)(long)local_b8);
                  local_560 = FUN_00015a7c(auStack_568,&local_2d8);
                  local_570 = FUN_000157c8(&local_538,param_1 + 0x88);
                  local_558 = FUN_00028080(&local_560,&local_570);
                  FUN_00068460(&local_558,param_1 + 0x88);
                  FUN_00002cf8(auStack_574,0x15d49d);
                  local_588 = FUN_00015a7c(&local_a0,param_1 + 0x88);
                  local_580 = FUN_00017c4c(&local_558,&local_588);
                  FUN_000196d0(auStack_40,auStack_574,&local_580);
                  FUN_00013490((double)(long)local_b8);
                  local_598 = FUN_00015a7c(auStack_5a0,&local_2d8);
                  local_5a8 = FUN_000157c8(&local_548,param_1 + 0x88);
                  local_590 = FUN_00028080(&local_598,&local_5a8);
                  local_558 = local_590;
                  FUN_00068460(&local_558,param_1 + 0x88);
                  FUN_00002cf8(auStack_5ac,0x15d49b);
                  FUN_000196d0(auStack_40,auStack_5ac,&local_558);
                  FUN_001cd3e4(auStack_40);
                  FUN_00167d6c(param_1,&DAT_00208c52,&DAT_00208c50);
                  if (local_402 != 0) {
                    FUN_00167d6c(param_1,&DAT_00208c50,&DAT_00208c52);
                  }
                  FUN_00002cf8(auStack_5b0,0x15d4a4);
                  sVar2 = FUN_00012ef4(auStack_40,auStack_5b0);
                  if (sVar2 == 0 && local_402 != 0) {
                    FUN_00002cf8(auStack_5b4,0x15d4c7);
                    local_5b8 = 0;
                    FUN_00015b04(auStack_40,auStack_5b4,&local_5b8,1);
                    FUN_00002cf8(auStack_5bc,0x15d4cc);
                    local_5c0 = 0;
                    FUN_00015b04(auStack_40,auStack_5bc,&local_5c0,1);
                    FUN_00002cf8(auStack_5c4,0x15d49e);
                    FUN_00013490(0);
                    FUN_00013c20(auStack_40,auStack_5c4,auStack_5d0);
                    FUN_00002cf8(auStack_5d4,0x15d4a0);
                    FUN_00013490(0);
                    FUN_00013c20(auStack_40,auStack_5d4,auStack_5e0);
                    FUN_00002cf8(auStack_5e4,0x15d49e);
                    FUN_0012dd50(param_1,auStack_5e4,&DAT_00208c50);
                    FUN_00002cf8(auStack_5e8,0x15d4a0);
                    FUN_0012dd50(param_1,auStack_5e8,&DAT_00208c50);
                    FUN_00002cf8(auStack_5ec,0x15d4e3);
                    FUN_00013490((double)(long)local_258);
                    FUN_00013c20(auStack_40,auStack_5ec,auStack_5f8);
                  }
                  else if (local_402 != 0) {
                    FUN_00131edc(param_1,&DAT_00208c50);
                    FUN_00136464(param_1);
                    FUN_00137230(param_1);
                  }
                  FUN_001b5050(auStack_40);
                  FUN_00138008(param_1);
                }
                FUN_00002cf8(auStack_5fc,0x15d49a);
                FUN_00129a38(param_1,auStack_5fc,&DAT_00208c50,&DAT_00208c52);
                FUN_00002cf8(auStack_600,0x15d49c);
                FUN_00129a38(param_1,auStack_600,&DAT_00208c50,&DAT_00208c52);
                FUN_001c0ee4(auStack_40);
                FUN_00002cf8(auStack_64c,0x15d39c);
                local_650 = 0xffffffff;
                FUN_00012c10(auStack_648,auStack_40,auStack_64c,&local_650);
                FUN_00002cf8(auStack_65c,0x15d39d);
                local_658 = FUN_00013338(auStack_40,auStack_65c);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_6a8,"Custom Leading (pt)",0);
                sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_648,auStack_6a8,1,0);
                bVar1 = true;
                if (sVar2 == 0) {
                  FUN_00013490(0);
                  sVar2 = FUN_00014140(&local_658,auStack_6b0);
                  bVar1 = sVar2 != 0;
                }
                __ZN8PMStringD1Ev(auStack_6a8);
                if (bVar1) {
                  FUN_000069c0(&local_6bc,&DAT_00004265);
                  uVar4 = FUN_0001703c(local_6bc);
                  FUN_00017094(auStack_6b8,uVar4);
                  lVar8 = FUN_00017114(auStack_6b8);
                  if (lVar8 != 0) {
                    __ZN17AttributeBossListC1Ev(auStack_6d8);
                    uVar4 = FUN_000170c8(auStack_6b8);
                    FUN_000170e0(uVar4,1);
                    uVar4 = FUN_00017114(auStack_6b8);
                    local_6dc = 0;
                    __ZN17AttributeBossList14ApplyAttributeEPK10IPMUnknown6IDTypeI11ClassID_tagE
                              (auStack_6d8,uVar4,0);
                    FUN_00002cf8(auStack_72c,0x15d3ad);
                    local_730 = 0xffffffff;
                    FUN_00012c10(auStack_728,auStack_40,auStack_72c,&local_730);
                    FUN_00002f44(auStack_734,0xca0c);
                    FUN_001aed08(auStack_40,auStack_728,auStack_6d8,auStack_734);
                    __ZN8PMStringD1Ev(auStack_728);
                    __ZN17AttributeBossListD1Ev(auStack_6d8);
                  }
                  FUN_0001712c(auStack_6b8);
                }
                __ZN8PMStringD1Ev(auStack_648);
                local_58 = 0;
              }
              else {
                local_58 = 2;
              }
              FUN_000325e4(auStack_1b0);
            }
            else {
              local_58 = 2;
            }
            FUN_0000e324(auStack_1a8);
          }
          else {
            local_58 = 2;
          }
          FUN_00017158(auStack_1a0);
          __ZN8PMStringD1Ev(auStack_128);
        }
        else {
          local_58 = 2;
        }
      }
      else {
        local_58 = 2;
      }
      FUN_00017ca4(auStack_70);
    }
    else {
      local_58 = 2;
    }
    FUN_00002b9c(auStack_60);
  }
  else {
    local_58 = 2;
  }
  FUN_00002c54(auStack_40);
  return;
}

//==== FUNC @ e8428 -> 000e8428

void FUN_000e8428(void)

{
  bool bVar1;
  short sVar2;
  int iVar3;
  long lVar4;
  undefined8 uVar5;
  long *plVar6;
  undefined1 auStack_1464 [4];
  undefined1 auStack_1460 [4];
  undefined1 auStack_145c [4];
  undefined1 auStack_1458 [4];
  undefined1 auStack_1454 [4];
  undefined1 auStack_1450 [4];
  undefined1 auStack_144c [4];
  undefined1 auStack_1448 [4];
  undefined1 auStack_1444 [4];
  undefined1 auStack_1440 [4];
  undefined1 auStack_143c [4];
  undefined1 auStack_1438 [4];
  undefined1 auStack_1434 [4];
  undefined1 auStack_1430 [4];
  undefined1 auStack_142c [4];
  undefined1 auStack_1428 [4];
  undefined1 auStack_1424 [4];
  undefined1 auStack_1420 [4];
  undefined1 auStack_141c [4];
  undefined1 auStack_1418 [4];
  undefined1 auStack_1414 [4];
  undefined1 auStack_1410 [4];
  undefined1 auStack_140c [4];
  undefined1 auStack_1408 [4];
  undefined1 auStack_1404 [4];
  undefined1 auStack_1400 [4];
  undefined1 auStack_13fc [4];
  undefined1 auStack_13f8 [4];
  undefined1 auStack_13f4 [4];
  undefined1 auStack_13f0 [4];
  undefined1 auStack_13ec [4];
  undefined1 auStack_13e8 [4];
  undefined1 auStack_13e4 [4];
  undefined1 auStack_13e0 [4];
  undefined1 auStack_13dc [4];
  undefined1 auStack_13d8 [4];
  undefined1 auStack_13d4 [4];
  undefined1 auStack_13d0 [4];
  undefined1 auStack_13cc [4];
  undefined1 auStack_13c8 [4];
  undefined1 auStack_13c4 [4];
  undefined1 auStack_13c0 [4];
  undefined1 auStack_13bc [4];
  undefined1 auStack_13b8 [4];
  undefined1 auStack_13b4 [4];
  undefined1 auStack_13b0 [4];
  undefined4 local_13ac;
  undefined1 auStack_13a8 [4];
  undefined4 local_13a4;
  undefined1 auStack_13a0 [4];
  undefined1 auStack_139c [4];
  undefined1 auStack_1398 [8];
  undefined1 auStack_1390 [4];
  undefined1 auStack_138c [4];
  undefined1 auStack_1388 [4];
  undefined1 auStack_1384 [4];
  undefined1 auStack_1380 [4];
  undefined1 auStack_137c [4];
  undefined1 auStack_1378 [8];
  undefined1 auStack_1370 [8];
  undefined1 auStack_1368 [8];
  undefined1 auStack_1360 [76];
  undefined4 local_1314;
  undefined1 auStack_1310 [12];
  undefined4 local_1304;
  undefined1 auStack_1300 [8];
  undefined4 local_12f8;
  undefined4 local_12f4;
  undefined1 auStack_12f0 [76];
  undefined1 auStack_12a4 [4];
  undefined1 auStack_12a0 [72];
  undefined1 auStack_1258 [4];
  undefined1 auStack_1254 [4];
  undefined1 auStack_1250 [4];
  undefined1 auStack_124c [4];
  undefined1 auStack_1248 [4];
  undefined1 auStack_1244 [4];
  undefined1 auStack_1240 [4];
  undefined1 auStack_123c [4];
  undefined1 auStack_1238 [4];
  undefined1 auStack_1234 [4];
  undefined1 auStack_1230 [8];
  undefined1 auStack_1228 [4];
  undefined1 auStack_1224 [4];
  undefined1 auStack_1220 [8];
  undefined1 auStack_1218 [4];
  undefined1 auStack_1214 [4];
  undefined1 auStack_1210 [8];
  undefined1 auStack_1208 [4];
  undefined1 auStack_1204 [4];
  undefined1 auStack_1200 [12];
  undefined1 auStack_11f4 [4];
  undefined1 auStack_11f0 [4];
  undefined1 auStack_11ec [4];
  undefined1 auStack_11e8 [4];
  undefined1 auStack_11e4 [4];
  undefined1 auStack_11e0 [76];
  undefined1 auStack_1194 [4];
  undefined1 auStack_1190 [76];
  undefined1 auStack_1144 [4];
  undefined1 auStack_1140 [76];
  undefined1 auStack_10f4 [4];
  undefined1 auStack_10f0 [76];
  undefined1 auStack_10a4 [4];
  undefined1 auStack_10a0 [72];
  undefined1 auStack_1058 [4];
  undefined1 auStack_1054 [4];
  undefined1 auStack_1050 [4];
  undefined1 auStack_104c [4];
  undefined1 auStack_1048 [4];
  undefined4 local_1044;
  undefined1 auStack_1040 [4];
  undefined1 auStack_103c [4];
  undefined1 auStack_1038 [4];
  undefined1 auStack_1034 [4];
  undefined1 auStack_1030 [4];
  undefined1 auStack_102c [4];
  undefined1 auStack_1028 [4];
  int local_1024;
  undefined1 auStack_1020 [8];
  undefined1 auStack_1018 [8];
  undefined8 local_1010;
  undefined1 auStack_1008 [72];
  undefined1 auStack_fc0 [4];
  undefined1 auStack_fbc [4];
  undefined1 auStack_fb8 [4];
  undefined1 auStack_fb4 [4];
  undefined1 auStack_fb0 [4];
  undefined1 auStack_fac [4];
  undefined4 local_fa8;
  undefined1 auStack_fa4 [4];
  undefined1 auStack_fa0 [4];
  undefined1 auStack_f9c [4];
  undefined1 auStack_f98 [4];
  undefined1 auStack_f94 [4];
  undefined1 auStack_f90 [4];
  int local_f8c;
  undefined1 auStack_f88 [8];
  undefined1 auStack_f80 [8];
  undefined8 local_f78;
  undefined1 auStack_f70 [72];
  undefined1 auStack_f28 [4];
  undefined1 auStack_f24 [4];
  undefined1 auStack_f20 [4];
  undefined1 auStack_f1c [4];
  undefined1 auStack_f18 [4];
  undefined1 auStack_f14 [4];
  undefined1 auStack_f10 [4];
  undefined1 auStack_f0c [4];
  undefined4 local_f08;
  undefined1 auStack_f04 [4];
  undefined1 auStack_f00 [4];
  undefined1 auStack_efc [4];
  undefined1 auStack_ef8 [4];
  int local_ef4;
  undefined1 auStack_ef0 [8];
  undefined1 auStack_ee8 [8];
  undefined8 local_ee0;
  undefined1 auStack_ed8 [72];
  undefined1 auStack_e90 [4];
  undefined1 auStack_e8c [4];
  undefined1 auStack_e88 [4];
  undefined1 auStack_e84 [4];
  undefined1 auStack_e80 [4];
  undefined1 auStack_e7c [4];
  undefined1 auStack_e78 [4];
  undefined1 auStack_e74 [4];
  undefined1 auStack_e70 [4];
  undefined1 auStack_e6c [4];
  undefined1 auStack_e68 [4];
  undefined1 auStack_e64 [4];
  undefined1 auStack_e60 [4];
  undefined1 auStack_e5c [4];
  undefined1 auStack_e58 [4];
  undefined1 auStack_e54 [4];
  undefined1 auStack_e50 [4];
  undefined1 auStack_e4c [4];
  undefined1 auStack_e48 [4];
  undefined1 auStack_e44 [4];
  undefined1 auStack_e40 [4];
  undefined1 auStack_e3c [4];
  undefined1 auStack_e38 [4];
  undefined1 auStack_e34 [4];
  undefined1 auStack_e30 [12];
  undefined1 auStack_e24 [4];
  undefined1 auStack_e20 [4];
  undefined1 auStack_e1c [4];
  undefined4 local_e18;
  undefined1 auStack_e14 [4];
  undefined1 auStack_e10 [72];
  undefined4 local_dc8;
  undefined1 auStack_dc4 [4];
  undefined1 auStack_dc0 [72];
  undefined4 local_d78;
  undefined1 auStack_d74 [4];
  undefined4 local_d70;
  undefined1 auStack_d6c [4];
  undefined1 auStack_d68 [72];
  undefined4 local_d20;
  undefined1 auStack_d1c [4];
  undefined1 auStack_d18 [76];
  undefined1 auStack_ccc [4];
  undefined1 auStack_cc8 [4];
  undefined1 auStack_cc4 [4];
  undefined1 auStack_cc0 [8];
  undefined1 auStack_cb8 [4];
  undefined1 auStack_cb4 [4];
  undefined1 auStack_cb0 [4];
  undefined1 auStack_cac [4];
  undefined1 auStack_ca8 [4];
  undefined1 auStack_ca4 [4];
  undefined1 auStack_ca0 [4];
  undefined1 auStack_c9c [4];
  undefined4 local_c98;
  undefined1 auStack_c94 [4];
  undefined1 auStack_c90 [72];
  undefined4 local_c48;
  undefined1 auStack_c44 [4];
  undefined1 auStack_c40 [72];
  undefined4 local_bf8;
  undefined1 auStack_bf4 [4];
  undefined4 local_bf0;
  undefined1 auStack_bec [4];
  undefined1 auStack_be8 [72];
  undefined4 local_ba0;
  undefined1 auStack_b9c [4];
  undefined1 auStack_b98 [76];
  undefined1 auStack_b4c [4];
  undefined1 auStack_b48 [4];
  undefined4 local_b44;
  undefined1 auStack_b40 [7];
  byte local_b39;
  undefined1 auStack_b38 [75];
  byte local_aed;
  undefined1 auStack_aec [4];
  undefined1 auStack_ae8 [72];
  undefined1 auStack_aa0 [76];
  undefined1 auStack_a54 [4];
  undefined1 auStack_a50 [78];
  ushort local_a02;
  undefined1 auStack_a00 [4];
  undefined1 auStack_9fc [4];
  undefined1 auStack_9f8 [6];
  ushort local_9f2;
  undefined8 local_9f0;
  undefined1 auStack_9e8 [8];
  undefined1 auStack_9e0 [4];
  undefined1 auStack_9dc [4];
  undefined8 local_9d8;
  undefined1 auStack_9cc [4];
  undefined8 local_9c8;
  undefined4 local_9bc;
  undefined4 local_9b8;
  undefined4 local_9b4;
  undefined1 auStack_9b0 [4];
  undefined4 local_9ac;
  undefined4 local_9a8;
  undefined4 local_9a4;
  undefined8 local_9a0;
  undefined1 auStack_994 [4];
  undefined4 local_990;
  undefined4 local_98c;
  undefined8 local_988;
  undefined8 local_980;
  undefined1 auStack_978 [76];
  undefined1 auStack_92c [4];
  undefined1 auStack_928 [78];
  short local_8da;
  undefined8 local_8d8;
  undefined8 local_8d0;
  undefined8 local_8c8;
  undefined8 local_8c0;
  undefined8 local_8b8;
  undefined8 local_8b0;
  undefined8 local_8a8;
  undefined8 local_8a0;
  undefined1 auStack_894 [4];
  undefined8 local_890;
  undefined1 auStack_888 [12];
  undefined1 auStack_87c [4];
  undefined8 local_878;
  undefined1 auStack_86c [4];
  undefined1 auStack_868 [4];
  undefined1 auStack_864 [4];
  undefined1 auStack_860 [72];
  undefined1 auStack_818 [4];
  undefined1 auStack_814 [4];
  undefined1 auStack_810 [76];
  undefined1 auStack_7c4 [4];
  undefined1 auStack_7c0 [72];
  undefined1 auStack_778 [12];
  undefined1 auStack_76c [4];
  undefined8 local_768;
  undefined1 auStack_760 [12];
  undefined1 auStack_754 [4];
  undefined1 auStack_750 [12];
  undefined1 auStack_744 [4];
  undefined1 auStack_740 [12];
  undefined1 auStack_734 [4];
  undefined1 auStack_730 [12];
  undefined1 auStack_724 [4];
  undefined1 auStack_720 [8];
  undefined1 auStack_718 [8];
  undefined1 auStack_710 [32];
  undefined1 local_6f0 [16];
  undefined1 auStack_6dc [4];
  undefined1 auStack_6d8 [8];
  undefined1 auStack_6d0 [4];
  undefined1 auStack_6cc [4];
  undefined1 auStack_6c8 [12];
  undefined1 auStack_6bc [4];
  undefined8 local_6b8;
  undefined1 auStack_6ac [4];
  undefined8 local_6a8;
  undefined1 auStack_69c [4];
  undefined8 local_698;
  undefined1 auStack_68c [4];
  undefined1 auStack_688 [8];
  undefined1 auStack_680 [4];
  undefined1 auStack_67c [4];
  undefined1 auStack_678 [12];
  undefined1 auStack_66c [4];
  undefined8 local_668;
  undefined8 local_660;
  undefined8 local_658;
  undefined1 auStack_64c [4];
  undefined8 local_648;
  undefined1 auStack_63c [4];
  undefined8 local_638;
  undefined1 auStack_62c [4];
  undefined8 local_628;
  undefined8 local_620;
  undefined8 local_618;
  undefined1 auStack_610 [12];
  undefined1 auStack_604 [4];
  undefined1 auStack_600 [8];
  undefined1 auStack_5f8 [4];
  undefined1 auStack_5f4 [4];
  undefined1 auStack_5f0 [12];
  undefined1 auStack_5e4 [4];
  undefined8 local_5e0;
  undefined1 auStack_5d4 [4];
  undefined8 local_5d0;
  undefined1 auStack_5c4 [4];
  undefined8 local_5c0;
  undefined1 auStack_5b4 [4];
  undefined1 auStack_5b0 [8];
  undefined1 auStack_5a8 [4];
  undefined1 auStack_5a4 [4];
  undefined1 auStack_5a0 [12];
  undefined1 auStack_594 [4];
  undefined8 local_590;
  undefined8 local_588;
  undefined8 local_580;
  undefined1 auStack_574 [4];
  undefined8 local_570;
  undefined1 auStack_564 [4];
  undefined8 local_560;
  undefined1 auStack_554 [4];
  undefined1 auStack_550 [8];
  undefined1 auStack_548 [8];
  undefined8 local_540;
  undefined8 local_538;
  undefined1 auStack_530 [4];
  undefined1 auStack_52c [4];
  undefined8 local_528;
  undefined1 auStack_51c [4];
  undefined8 local_518;
  undefined1 auStack_50c [4];
  undefined8 local_508;
  undefined4 local_500;
  undefined1 auStack_4fc [4];
  undefined1 auStack_4f8 [76];
  undefined1 auStack_4ac [4];
  undefined1 auStack_4a8 [4];
  undefined1 auStack_4a4 [4];
  undefined1 auStack_4a0 [8];
  undefined8 local_498;
  undefined8 local_490;
  undefined1 auStack_484 [4];
  undefined8 local_480;
  undefined1 auStack_478 [12];
  undefined1 auStack_46c [4];
  undefined1 auStack_468 [4];
  undefined1 auStack_464 [6];
  ushort local_45e;
  undefined1 auStack_45c [6];
  ushort local_456;
  undefined1 auStack_454 [4];
  undefined1 auStack_450 [78];
  ushort local_402;
  undefined1 auStack_400 [12];
  undefined1 auStack_3f4 [4];
  undefined1 auStack_3f0 [72];
  undefined1 auStack_3a8 [76];
  undefined1 auStack_35c [4];
  undefined1 auStack_358 [72];
  undefined1 auStack_310 [4];
  undefined1 auStack_30c [4];
  undefined1 auStack_308 [72];
  undefined1 auStack_2c0 [12];
  undefined1 auStack_2b4 [4];
  undefined8 local_2b0;
  undefined1 auStack_2a8 [76];
  undefined1 auStack_25c [4];
  undefined1 auStack_258 [72];
  undefined1 auStack_210 [4];
  undefined1 auStack_20c [4];
  undefined1 auStack_208 [76];
  undefined1 auStack_1bc [4];
  undefined4 local_1b8;
  undefined1 auStack_1b4 [4];
  undefined1 auStack_1b0 [8];
  undefined4 local_1a8;
  undefined1 auStack_1a4 [4];
  undefined1 auStack_1a0 [76];
  int local_154;
  undefined1 auStack_150 [6];
  short local_14a;
  undefined1 auStack_148 [6];
  undefined2 local_142;
  undefined1 auStack_140 [6];
  short local_13a;
  undefined4 local_138;
  undefined1 auStack_134 [4];
  undefined1 auStack_130 [76];
  undefined1 auStack_e4 [4];
  undefined8 local_e0;
  undefined1 auStack_d8 [15];
  undefined1 uStack_c9;
  undefined1 local_c8 [16];
  undefined1 auStack_b8 [8];
  undefined1 auStack_b0 [8];
  undefined1 auStack_a8 [8];
  undefined1 auStack_a0 [15];
  undefined1 uStack_91;
  undefined1 local_90 [16];
  undefined1 auStack_80 [15];
  undefined1 uStack_71;
  undefined1 local_70 [16];
  undefined1 auStack_60 [15];
  undefined1 uStack_51;
  undefined1 auStack_50 [8];
  undefined4 local_48;
  undefined1 uStack_31;
  undefined1 auStack_30 [8];
  long local_28;
  
  lVar4 = (*(code *)PTR____chkstk_darwin_002340e0)();
  local_28 = lVar4;
  FUN_00002bf4(auStack_30,lVar4,&uStack_31);
  sVar2 = FUN_00002c30(auStack_30);
  if (sVar2 != 0) {
    local_48 = 2;
    goto LAB_000ed938;
  }
  uVar5 = __Z26GetExecutionContextSessionv();
  FUN_00002978(auStack_50,uVar5,&uStack_51);
  sVar2 = FUN_000029b4(auStack_50);
  if (sVar2 == 0) {
    plVar6 = (long *)FUN_000029d8(auStack_50);
    local_70 = (**(code **)(*plVar6 + 0x18))();
    FUN_0000df20(auStack_60,local_70,&uStack_71);
    sVar2 = FUN_0000df5c(auStack_60);
    if (sVar2 == 0) {
      plVar6 = (long *)FUN_0001b750(auStack_60);
      local_90 = (**(code **)(*plVar6 + 0x88))();
      FUN_00084d30(auStack_80,local_90,&uStack_91);
      sVar2 = FUN_0001b81c(auStack_80);
      if (sVar2 == 0) {
        FUN_001a264c(auStack_a0);
        sVar2 = FUN_00030ab8(auStack_a0);
        if (sVar2 == 0) {
          FUN_001a7968(auStack_a8);
          sVar2 = FUN_00016b78(auStack_a8);
          if (sVar2 == 0) {
            FUN_001a2484(auStack_b0);
            sVar2 = FUN_0000e2a4(auStack_b0);
            if (sVar2 == 0) {
              plVar6 = (long *)FUN_000029d8(auStack_50);
              local_c8 = (**(code **)(*plVar6 + 0x28))();
              FUN_00017bd4(auStack_b8,local_c8,&uStack_c9);
              sVar2 = FUN_00017c10(auStack_b8);
              if (sVar2 == 0) {
                FUN_001a7c08(auStack_d8);
                sVar2 = FUN_0000e280(auStack_d8);
                if (sVar2 == 0) {
                  FUN_00002cf8(auStack_e4,0x15d31d);
                  local_e0 = FUN_00013338(auStack_30,auStack_e4);
                  FUN_00002cf8(auStack_134,0x15d309);
                  local_138 = 0xffffffff;
                  FUN_00012c10(auStack_130,auStack_30,auStack_134,&local_138);
                  FUN_00002cf8(auStack_140,0x15d4a3);
                  local_13a = FUN_00012ef4(auStack_30,auStack_140);
                  FUN_00002cf8(auStack_148,0x15d4a4);
                  local_142 = FUN_00012ef4(auStack_30,auStack_148);
                  FUN_00002cf8(auStack_150,0x15d3a9);
                  local_14a = FUN_00012ef4(auStack_30,auStack_150);
                  if (local_14a == 1) {
                    local_154 = 1;
                    FUN_00002cf8(auStack_1a4,0x15d309);
                    local_1a8 = 0xffffffff;
                    FUN_00012c10(auStack_1a0,auStack_30,auStack_1a4,&local_1a8);
                    __ZN8PMString6AppendEPKciNS_14StringEncodingE
                              (auStack_1a0," Image-lines",0x7fffffff,0xffffffff);
                    sVar2 = FUN_001a6604(auStack_1a0);
                    if (sVar2 != 0) {
                      local_154 = FUN_001b41d8(auStack_30);
                      if (local_154 == 0) {
                        plVar6 = (long *)FUN_00017c34(auStack_b8);
                        FUN_00013490(0);
                        (**(code **)(*plVar6 + 0x2c8))(plVar6,auStack_1b0);
                      }
                    }
                    __ZN8PMStringD1Ev(auStack_1a0);
                  }
                  FUN_00002cf8(auStack_1b4,0x15d3aa);
                  local_1b8 = 0;
                  FUN_00015b04(auStack_30,auStack_1b4,&local_1b8,1);
                  FUN_00002cf8(auStack_1bc,0x15d3a8);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_208,"",0);
                  FUN_00013588(auStack_30,auStack_1bc,auStack_208);
                  __ZN8PMStringD1Ev(auStack_208);
                  FUN_00002cf8(auStack_20c,0x15d3a8);
                  FUN_00011dc4(auStack_30,auStack_20c,&DAT_00208c50);
                  FUN_00002cf8(auStack_210,0x15d3a9);
                  FUN_00013178(auStack_30,auStack_210,&DAT_00208c50);
                  FUN_00002cf8(auStack_25c,0x15d3b3);
                  FUN_000138a8(auStack_258,auStack_30,auStack_25c);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_2a8,"0x15d300kGCLockKey",0);
                  sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_258,auStack_2a8,1,0);
                  bVar1 = false;
                  if (sVar2 != 0) {
                    FUN_00002cf8(auStack_2b4,0x15d31d);
                    local_2b0 = FUN_00013338(auStack_30,auStack_2b4);
                    FUN_00013490(0);
                    sVar2 = FUN_00018504(&local_2b0,auStack_2c0);
                    bVar1 = sVar2 != 0;
                  }
                  __ZN8PMStringD1Ev(auStack_2a8);
                  __ZN8PMStringD1Ev(auStack_258);
                  if (bVar1) {
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_308,"[GC-1]",0);
                    sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_130,auStack_308,1,0);
                    bVar1 = local_13a != 0;
                    __ZN8PMStringD1Ev(auStack_308);
                    if (sVar2 == 0 && bVar1) {
                      FUN_00002cf8(auStack_310,0x15d3a9);
                      FUN_00011dc4(auStack_30,auStack_310,&DAT_00208c50);
                    }
                    else {
                      FUN_00002cf8(auStack_30c,0x15d3a9);
                      FUN_00011dc4(auStack_30,auStack_30c,&DAT_00208c52);
                    }
                  }
                  FUN_00002cf8(auStack_35c,0x15d324);
                  FUN_000138a8(auStack_358,auStack_30,auStack_35c);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_3a8,"0x15d300kGCUnDoKey",0);
                  sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_358,auStack_3a8,1,0);
                  __ZN8PMStringD1Ev(auStack_3a8);
                  __ZN8PMStringD1Ev(auStack_358);
                  if (sVar2 != 0) {
                    FUN_0012335c(lVar4);
                  }
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_3f0,"[GC] Set Leading",0);
                  FUN_00002cf8(auStack_3f4,0x15d323);
                  sVar2 = FUN_00012ef4(auStack_30,auStack_3f4);
                  bVar1 = false;
                  if (sVar2 == 0) {
                    sVar2 = FUN_001a6604(auStack_3f0);
                    bVar1 = sVar2 != 0;
                  }
                  if (bVar1) {
                    FUN_001b4670(auStack_3f0);
                    plVar6 = (long *)FUN_000029d8(auStack_50);
                    FUN_00013490(0);
                    (**(code **)(*plVar6 + 0x130))(plVar6,auStack_400);
                    if (local_13a != 0) {
                      local_402 = 0;
                      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_450,"[GC-1]",0);
                      sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_130,auStack_450,1,0);
                      __ZN8PMStringD1Ev(auStack_450);
                      local_402 = (ushort)(sVar2 != 0);
                      FUN_00002cf8(auStack_454,0x15d49a);
                      local_456 = (ushort)(local_402 == 0);
                      FUN_00011dc4(auStack_30,auStack_454,&local_456);
                      FUN_00002cf8(auStack_45c,0x15d49c);
                      local_45e = (ushort)(local_402 == 0);
                      FUN_00011dc4(auStack_30,auStack_45c,&local_45e);
                      FUN_00002cf8(auStack_464,0x15d49b);
                      FUN_00011dc4(auStack_30,auStack_464,&local_402);
                      FUN_00002cf8(auStack_468,0x15d49d);
                      FUN_00011dc4(auStack_30,auStack_468,&local_402);
                      FUN_00002cf8(auStack_46c,0x15d4c5);
                      FUN_00011dc4(auStack_30,auStack_46c,&DAT_00208c50);
                      plVar6 = (long *)FUN_000029d8(auStack_50);
                      FUN_00013490(0);
                      (**(code **)(*plVar6 + 0x1e0))(plVar6,auStack_478);
                      FUN_00002cf8(auStack_484,0x15d49c);
                      local_480 = FUN_00013338(auStack_30,auStack_484);
                      local_498 = FUN_00015a7c(&local_480,&local_e0);
                      local_490 = FUN_00015a7c(&local_498,lVar4 + 0x88);
                      FUN_00013490(0);
                      sVar2 = FUN_00015808(&local_490,auStack_4a0);
                      if (sVar2 != 0) {
                        FUN_00002cf8(auStack_4a4,0x15d49d);
                        FUN_00013c20(auStack_30,auStack_4a4,&local_490);
                      }
                      FUN_00002cf8(auStack_4a8,0x15d49d);
                      FUN_0012fc3c(lVar4,auStack_4a8,&DAT_00208c50);
                    }
                    FUN_00002cf8(auStack_4ac,0x15d323);
                    FUN_00013178(auStack_30,auStack_4ac,&DAT_00208c52);
                  }
                  FUN_00002cf8(auStack_4fc,0x15d306);
                  local_500 = 0xffffffff;
                  FUN_00012c10(auStack_4f8,auStack_30,auStack_4fc,&local_500);
                  FUN_00002cf8(auStack_50c,&DAT_0015d31a);
                  local_508 = FUN_0001544c(auStack_30,auStack_50c,auStack_4f8);
                  FUN_00002cf8(auStack_51c,&DAT_0015d31b);
                  local_518 = FUN_0001544c(auStack_30,auStack_51c,auStack_4f8);
                  FUN_0001b96c(auStack_30);
                  local_528 = FUN_0001e3ac(auStack_30,&local_508,&local_518);
                  FUN_00002cf8(auStack_52c,0x15d320);
                  FUN_00013c20(auStack_30,auStack_52c,&local_528);
                  FUN_00002cf8(auStack_530,0x15d4a5);
                  FUN_00013c20(auStack_30,auStack_530,&local_528);
                  plVar6 = (long *)FUN_000029d8(auStack_50);
                  local_540 = (**(code **)(*plVar6 + 0x98))();
                  plVar6 = (long *)FUN_000029d8(auStack_50);
                  iVar3 = (**(code **)(*plVar6 + 0xa8))();
                  FUN_00013490((double)iVar3);
                  local_538 = FUN_000157c8(&local_540,auStack_548);
                  FUN_00013490(0);
                  sVar2 = FUN_00014140(&local_538,auStack_550);
                  if (sVar2 != 0) {
                    FUN_00002cf8(auStack_554,0x15d4a4);
                    sVar2 = FUN_00012ef4(auStack_30,auStack_554);
                    if (sVar2 == 0) {
                      FUN_00002cf8(auStack_564,0x15d49e);
                      local_560 = FUN_00013338(auStack_30,auStack_564);
                      FUN_00002cf8(auStack_574,0x15d4a0);
                      local_570 = FUN_00013338(auStack_30,auStack_574);
                      local_588 = FUN_00015a7c(&local_560,&local_538);
                      local_590 = FUN_00015a7c(&local_570,&local_538);
                      local_580 = FUN_00017c4c(&local_588,&local_590);
                      sVar2 = FUN_00031d38(&local_508,&local_580);
                      if (sVar2 != 0) {
                        FUN_00002cf8(auStack_594,0x15d49e);
                        FUN_00013490(0);
                        FUN_00013c20(auStack_30,auStack_594,auStack_5a0);
                        FUN_00002cf8(auStack_5a4,0x15d49e);
                        FUN_0012dd50(lVar4,auStack_5a4,&DAT_00208c50);
                        FUN_00002cf8(auStack_5a8,0x15d4a0);
                        FUN_00013490(0);
                        FUN_00013c20(auStack_30,auStack_5a8,auStack_5b0);
                        FUN_00002cf8(auStack_5b4,0x15d4a0);
                        FUN_0012dd50(lVar4,auStack_5b4,&DAT_00208c50);
                      }
                    }
                    else {
                      FUN_00002cf8(auStack_5c4,0x15d49f);
                      local_5c0 = FUN_00013338(auStack_30,auStack_5c4);
                      FUN_00002cf8(auStack_5d4,0x15d4a1);
                      local_5d0 = FUN_00013338(auStack_30,auStack_5d4);
                      local_5e0 = FUN_00017c4c(&local_5c0,&local_5d0);
                      sVar2 = FUN_00031d38(&local_508,&local_5e0);
                      if (sVar2 != 0) {
                        FUN_00002cf8(auStack_5e4,0x15d49f);
                        FUN_00013490(0);
                        FUN_00013c20(auStack_30,auStack_5e4,auStack_5f0);
                        FUN_00002cf8(auStack_5f4,0x15d49f);
                        FUN_00131b38(lVar4,auStack_5f4);
                        FUN_00002cf8(auStack_5f8,0x15d4a1);
                        FUN_00013490(0);
                        FUN_00013c20(auStack_30,auStack_5f8,auStack_600);
                        FUN_00002cf8(auStack_604,0x15d4a1);
                        FUN_00131b38(lVar4,auStack_604);
                      }
                    }
                  }
                  FUN_00013490(0);
                  sVar2 = FUN_00014140(&local_e0,auStack_610);
                  if (sVar2 != 0) {
                    local_618 = local_e0;
                    FUN_00013490(&local_620);
                    FUN_00013490(0,&local_628);
                    FUN_00002cf8(auStack_62c,0x15d4a3);
                    sVar2 = FUN_00012ef4(auStack_30,auStack_62c);
                    if (sVar2 == 0) {
                      FUN_00002cf8(auStack_63c,0x15d49a);
                      local_638 = FUN_00013338(auStack_30,auStack_63c);
                      local_620 = local_638;
                      FUN_00002cf8(auStack_64c,0x15d49c);
                      local_648 = FUN_00013338(auStack_30,auStack_64c);
                      local_628 = local_648;
                      local_660 = FUN_00015a7c(&local_620,&local_618);
                      local_668 = FUN_00015a7c(&local_628,&local_618);
                      local_658 = FUN_00017c4c(&local_660,&local_668);
                      sVar2 = FUN_00031d38(&local_518,&local_658);
                      if (sVar2 != 0) {
                        FUN_00002cf8(auStack_66c,0x15d49a);
                        FUN_00013490(0);
                        FUN_00013c20(auStack_30,auStack_66c,auStack_678);
                        FUN_00002cf8(auStack_67c,0x15d49a);
                        FUN_00129a38(lVar4,auStack_67c,&DAT_00208c50,&DAT_00208c52);
                        FUN_00002cf8(auStack_680,0x15d49c);
                        FUN_00013490(0);
                        FUN_00013c20(auStack_30,auStack_680,auStack_688);
                        FUN_00002cf8(auStack_68c,0x15d49c);
                        FUN_00129a38(lVar4,auStack_68c,&DAT_00208c50,&DAT_00208c52);
                      }
                    }
                    else {
                      FUN_00002cf8(auStack_69c,0x15d49b);
                      local_698 = FUN_00013338(auStack_30,auStack_69c);
                      local_620 = local_698;
                      FUN_00002cf8(auStack_6ac,0x15d49d);
                      local_6a8 = FUN_00013338(auStack_30,auStack_6ac);
                      local_628 = local_6a8;
                      local_6b8 = FUN_00017c4c(&local_620,&local_628);
                      sVar2 = FUN_00031d38(&local_518,&local_6b8);
                      if (sVar2 != 0) {
                        FUN_00002cf8(auStack_6bc,0x15d49b);
                        FUN_00013490(0);
                        FUN_00013c20(auStack_30,auStack_6bc,auStack_6c8);
                        FUN_00002cf8(auStack_6cc,0x15d49b);
                        FUN_0012fc3c(lVar4,auStack_6cc,&DAT_00208c50);
                        FUN_00002cf8(auStack_6d0,0x15d49d);
                        FUN_00013490(0);
                        FUN_00013c20(auStack_30,auStack_6d0,auStack_6d8);
                        FUN_00002cf8(auStack_6dc,0x15d49d);
                        FUN_0012fc3c(lVar4,auStack_6dc,&DAT_00208c50);
                      }
                    }
                  }
                  plVar6 = (long *)FUN_000029d8(auStack_50);
                  sVar2 = (**(code **)(*plVar6 + 0x58))();
                  if (sVar2 == 0) {
LAB_000e9fa8:
                    plVar6 = (long *)FUN_000029d8(auStack_50);
                    local_6f0 = (**(code **)(*plVar6 + 0x18))();
                    FUN_00013490();
                    FUN_00013490(0);
                    __ZN6PMRectC1ERK6PMRealS2_S2_S2_
                              (auStack_710,auStack_718,auStack_720,&local_508,&local_518);
                    FUN_001b0acc(local_6f0,auStack_710);
                  }
                  else {
                    plVar6 = (long *)FUN_000029d8(auStack_50);
                    sVar2 = (**(code **)(*plVar6 + 0x68))();
                    if (sVar2 == 0) goto LAB_000e9fa8;
                  }
                  FUN_00002cf8(auStack_724,0x15d49a);
                  FUN_00013490(0x4024000000000000);
                  FUN_00019938(auStack_30,auStack_724,auStack_730);
                  FUN_00002cf8(auStack_734,0x15d49c);
                  FUN_00013490(0x4024000000000000);
                  FUN_00019938(auStack_30,auStack_734,auStack_740);
                  FUN_00002cf8(auStack_744,0x15d49e);
                  FUN_00013490(0x4024000000000000);
                  FUN_00019938(auStack_30,auStack_744,auStack_750);
                  FUN_00002cf8(auStack_754,0x15d4a0);
                  FUN_00013490(0x4024000000000000);
                  FUN_00019938(auStack_30,auStack_754,auStack_760);
                  FUN_00002cf8(auStack_76c,0x15d31c);
                  local_768 = FUN_00013338(auStack_30,auStack_76c);
                  FUN_00013490(0);
                  sVar2 = FUN_00018504(&local_768,auStack_778);
                  if (sVar2 == 0) {
                    FUN_00002cf8(auStack_1058,0x15d31c);
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_10a0,"",0);
                    FUN_00013588(auStack_30,auStack_1058,auStack_10a0);
                    __ZN8PMStringD1Ev(auStack_10a0);
                    FUN_00002cf8(auStack_10a4,0x15d31d);
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_10f0,"",0);
                    FUN_00013588(auStack_30,auStack_10a4,auStack_10f0);
                    __ZN8PMStringD1Ev(auStack_10f0);
                    FUN_00002cf8(auStack_10f4,0x15d330);
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1140,"",0);
                    FUN_00013588(auStack_30,auStack_10f4,auStack_1140);
                    __ZN8PMStringD1Ev(auStack_1140);
                    FUN_00002cf8(auStack_1144,0x15d324);
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                              (auStack_1190,"0x15d300kGCSquareKey",0);
                    FUN_00013588(auStack_30,auStack_1144,auStack_1190);
                    __ZN8PMStringD1Ev(auStack_1190);
                    FUN_00002cf8(auStack_1194,0x15d32f);
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                              (auStack_11e0,"0x15d300kGCCalculateKey",0);
                    FUN_00013588(auStack_30,auStack_1194,auStack_11e0);
                    __ZN8PMStringD1Ev(auStack_11e0);
                    FUN_00002cf8(auStack_11e4,0x15d4a3);
                    FUN_00013178(auStack_30,auStack_11e4,&DAT_00208c50);
                    FUN_00002cf8(auStack_11e8,0x15d4a4);
                    FUN_00013178(auStack_30,auStack_11e8,&DAT_00208c50);
                    FUN_00002cf8(auStack_11ec,0x15d4a3);
                    FUN_0013bc94(lVar4,auStack_11ec);
                    FUN_00002cf8(auStack_11f0,0x15d4a4);
                    FUN_0013bc94(lVar4,auStack_11f0);
                    FUN_00167d6c(lVar4,&DAT_00208c50,&DAT_00208c52);
                    FUN_00002cf8(auStack_11f4,0x15d49a);
                    FUN_00013490(0);
                    FUN_00013c20(auStack_30,auStack_11f4,auStack_1200);
                    FUN_00002cf8(auStack_1204,0x15d49a);
                    FUN_00129a38(lVar4,auStack_1204,&DAT_00208c50,&DAT_00208c52);
                    FUN_00002cf8(auStack_1208,0x15d49c);
                    FUN_00013490(0);
                    FUN_00013c20(auStack_30,auStack_1208,auStack_1210);
                    FUN_00002cf8(auStack_1214,0x15d49c);
                    FUN_00129a38(lVar4,auStack_1214,&DAT_00208c50,&DAT_00208c52);
                    FUN_00002cf8(auStack_1218,0x15d49e);
                    FUN_00013490(0);
                    FUN_00013c20(auStack_30,auStack_1218,auStack_1220);
                    FUN_00002cf8(auStack_1224,0x15d49e);
                    FUN_0012dd50(lVar4,auStack_1224,&DAT_00208c50);
                    FUN_00002cf8(auStack_1228,0x15d4a0);
                    FUN_00013490(0);
                    FUN_00013c20(auStack_30,auStack_1228,auStack_1230);
                    FUN_00002cf8(auStack_1234,0x15d4a0);
                    FUN_0012dd50(lVar4,auStack_1234,&DAT_00208c50);
                    FUN_00002cf8(auStack_1238,0x15d49a);
                    FUN_00011dc4(auStack_30,auStack_1238,&DAT_00208c50);
                    FUN_00002cf8(auStack_123c,0x15d49c);
                    FUN_00011dc4(auStack_30,auStack_123c,&DAT_00208c50);
                    FUN_00002cf8(auStack_1240,0x15d49e);
                    FUN_00011dc4(auStack_30,auStack_1240,&DAT_00208c50);
                    FUN_00002cf8(auStack_1244,0x15d4a0);
                    FUN_00011dc4(auStack_30,auStack_1244,&DAT_00208c50);
                    FUN_00002cf8(auStack_1248,0x15d4c5);
                    FUN_00011dc4(auStack_30,auStack_1248,&DAT_00208c50);
                    FUN_00002cf8(auStack_124c,0x15d4c6);
                    FUN_00011dc4(auStack_30,auStack_124c,&DAT_00208c50);
                    FUN_00002cf8(auStack_1250,0x15d4c5);
                    FUN_00013178(auStack_30,auStack_1250,&DAT_00208c50);
                    FUN_00002cf8(auStack_1254,0x15d4c6);
                    FUN_00013178(auStack_30,auStack_1254,&DAT_00208c50);
                    FUN_00002cf8(auStack_1258,0x15d30b);
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_12a0,"",0);
                    FUN_00013588(auStack_30,auStack_1258,auStack_12a0);
                    __ZN8PMStringD1Ev(auStack_12a0);
                    FUN_00002cf8(auStack_12a4,&DAT_0015d30c);
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_12f0,"",0);
                    FUN_00013588(auStack_30,auStack_12a4,auStack_12f0);
                    __ZN8PMStringD1Ev(auStack_12f0);
                    plVar6 = (long *)FUN_000029d8(auStack_50);
                    local_12f4 = 0;
                    (**(code **)(*plVar6 + 0xf0))(plVar6,&local_12f4);
                    plVar6 = (long *)FUN_000029d8(auStack_50);
                    local_12f8 = 0;
                    (**(code **)(*plVar6 + 0x100))(plVar6,&local_12f8);
                    plVar6 = (long *)FUN_000029d8(auStack_50);
                    FUN_00013490(0);
                    (**(code **)(*plVar6 + 0xa0))(plVar6,auStack_1300);
                    plVar6 = (long *)FUN_000029d8(auStack_50);
                    local_1304 = 1;
                    (**(code **)(*plVar6 + 0xb0))(plVar6,&local_1304);
                    plVar6 = (long *)FUN_000029d8(auStack_50);
                    FUN_00013490(0);
                    (**(code **)(*plVar6 + 0xd0))(plVar6,auStack_1310);
                    plVar6 = (long *)FUN_000029d8(auStack_50);
                    local_1314 = 1;
                    (**(code **)(*plVar6 + 0xe0))(plVar6,&local_1314);
                    plVar6 = (long *)FUN_000029d8(auStack_50);
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1360,"",0);
                    (**(code **)(*plVar6 + 0x140))(plVar6,auStack_1360);
                    __ZN8PMStringD1Ev(auStack_1360);
                    plVar6 = (long *)FUN_000029d8(auStack_50);
                    FUN_00013490(0);
                    (**(code **)(*plVar6 + 0x1d0))(plVar6,auStack_1368);
                    plVar6 = (long *)FUN_000029d8(auStack_50);
                    FUN_00013490(0);
                    (**(code **)(*plVar6 + 0x1e0))(plVar6,auStack_1370);
                    plVar6 = (long *)FUN_000029d8(auStack_50);
                    FUN_00013490(0);
                    (**(code **)(*plVar6 + 0x1f0))(plVar6,auStack_1378);
                    *(undefined2 *)(lVar4 + 0x9c) = 0;
                    FUN_00002cf8(auStack_137c,0x15d4c7);
                    FUN_000193a8(auStack_30,auStack_137c);
                    FUN_00002cf8(auStack_1380,0x15d4c8);
                    FUN_000193a8(auStack_30,auStack_1380);
                    FUN_00002cf8(auStack_1384,0x15d4cc);
                    FUN_000193a8(auStack_30,auStack_1384);
                    FUN_00002cf8(auStack_1388,0x15d4c9);
                    FUN_000193a8(auStack_30,auStack_1388);
                    FUN_00002cf8(auStack_138c,0x15d3fe);
                    FUN_000193a8(auStack_30,auStack_138c);
                    FUN_00002cf8(auStack_1390,0x15d4cf);
                    FUN_000193a8(auStack_30,auStack_1390);
                    FUN_00013490(0);
                    FUN_00015c98(auStack_30,auStack_1398);
                    FUN_00002cf8(auStack_139c,0x15d3a9);
                    FUN_00011dc4(auStack_30,auStack_139c,&DAT_00208c50);
                    FUN_00002cf8(auStack_13a0,0x15d3aa);
                    local_13a4 = 0;
                    FUN_00015b04(auStack_30,auStack_13a0,&local_13a4,1);
                    FUN_00002cf8(auStack_13a8,0x15d3aa);
                    local_13ac = 2;
                    FUN_00015844(auStack_30,auStack_13a8,&local_13ac,&DAT_00208c50);
                    FUN_00002cf8(auStack_13b0,0x15d3a1);
                    FUN_000187cc(auStack_30,auStack_13b0,&DAT_00208c50);
                    FUN_00002cf8(auStack_13b4,0x15d3a2);
                    FUN_000187cc(auStack_30,auStack_13b4,&DAT_00208c50);
                    FUN_00002cf8(auStack_13b8,0x15d4c7);
                    FUN_00011dc4(auStack_30,auStack_13b8,&DAT_00208c50);
                    FUN_00002cf8(auStack_13bc,0x15d4c8);
                    FUN_00011dc4(auStack_30,auStack_13bc,&DAT_00208c50);
                    FUN_00002cf8(auStack_13c0,0x15d4cc);
                    FUN_00011dc4(auStack_30,auStack_13c0,&DAT_00208c50);
                    FUN_00002cf8(auStack_13c4,0x15d4cd);
                    FUN_00011dc4(auStack_30,auStack_13c4,&DAT_00208c50);
                    FUN_00002cf8(auStack_13c8,0x15d4ce);
                    FUN_00011dc4(auStack_30,auStack_13c8,&DAT_00208c50);
                    FUN_00002cf8(auStack_13cc,0x15d4c9);
                    FUN_00011dc4(auStack_30,auStack_13cc,&DAT_00208c50);
                    FUN_00002cf8(auStack_13d0,0x15d3fe);
                    FUN_00011dc4(auStack_30,auStack_13d0,&DAT_00208c50);
                    FUN_00002cf8(auStack_13d4,0x15d4ca);
                    FUN_00011dc4(auStack_30,auStack_13d4,&DAT_00208c50);
                    FUN_00002cf8(auStack_13d8,0x15d4cb);
                    FUN_00011dc4(auStack_30,auStack_13d8,&DAT_00208c50);
                    FUN_00002cf8(auStack_13dc,0x15d4cf);
                    FUN_00011dc4(auStack_30,auStack_13dc,&DAT_00208c50);
                    FUN_00002cf8(auStack_13e0,0x15d4d0);
                    FUN_00011dc4(auStack_30,auStack_13e0,&DAT_00208c50);
                    FUN_00002cf8(auStack_13e4,0x15d4d1);
                    FUN_00011dc4(auStack_30,auStack_13e4,&DAT_00208c50);
                    FUN_00002cf8(auStack_13e8,0x15d4d2);
                    FUN_00011dc4(auStack_30,auStack_13e8,&DAT_00208c50);
                    FUN_00002cf8(auStack_13ec,0x15d4d3);
                    FUN_00011dc4(auStack_30,auStack_13ec,&DAT_00208c50);
                    FUN_00002cf8(auStack_13f0,0x15d4d4);
                    FUN_00011dc4(auStack_30,auStack_13f0,&DAT_00208c50);
                    FUN_00002cf8(auStack_13f4,0x15d4d5);
                    FUN_00011dc4(auStack_30,auStack_13f4,&DAT_00208c50);
                    FUN_00002cf8(auStack_13f8,0x15d3ff);
                    FUN_00011dc4(auStack_30,auStack_13f8,&DAT_00208c50);
                    FUN_00002cf8(auStack_13fc,0x15d4d6);
                    FUN_00011dc4(auStack_30,auStack_13fc,&DAT_00208c50);
                    FUN_00002cf8(auStack_1400,0x15d4b3);
                    FUN_00011dc4(auStack_30,auStack_1400,&DAT_00208c50);
                    FUN_00002cf8(auStack_1404,0x15d4b5);
                    FUN_00011dc4(auStack_30,auStack_1404,&DAT_00208c50);
                    FUN_00002cf8(auStack_1408,0x15d3fb);
                    FUN_00011dc4(auStack_30,auStack_1408,&DAT_00208c50);
                    FUN_00002cf8(auStack_140c,0x15d3fd);
                    FUN_00011dc4(auStack_30,auStack_140c,&DAT_00208c50);
                    FUN_00002cf8(auStack_1410,0x15d4e0);
                    FUN_00011dc4(auStack_30,auStack_1410,&DAT_00208c50);
                    FUN_00002cf8(auStack_1414,0x15d4e1);
                    FUN_00011dc4(auStack_30,auStack_1414,&DAT_00208c50);
                    FUN_00002cf8(auStack_1418,0x15d4e0);
                    FUN_00013178(auStack_30,auStack_1418,&DAT_00208c50);
                    FUN_00002cf8(auStack_141c,0x15d4bb);
                    FUN_00011dc4(auStack_30,auStack_141c,&DAT_00208c50);
                    FUN_00002cf8(auStack_1420,0x15d4bd);
                    FUN_00011dc4(auStack_30,auStack_1420,&DAT_00208c50);
                    FUN_00002cf8(auStack_1424,0x15d4c0);
                    FUN_00011dc4(auStack_30,auStack_1424,&DAT_00208c50);
                    FUN_00002cf8(auStack_1428,0x15d4c1);
                    FUN_00011dc4(auStack_30,auStack_1428,&DAT_00208c50);
                    FUN_00002cf8(auStack_142c,0x15d4c0);
                    FUN_00013178(auStack_30,auStack_142c,&DAT_00208c50);
                    FUN_00002cf8(auStack_1430,0x15d4ab);
                    FUN_00011dc4(auStack_30,auStack_1430,&DAT_00208c50);
                    FUN_00002cf8(auStack_1434,0x15d4ad);
                    FUN_00011dc4(auStack_30,auStack_1434,&DAT_00208c50);
                    FUN_00002cf8(auStack_1438,0x15d4b7);
                    FUN_00011dc4(auStack_30,auStack_1438,&DAT_00208c50);
                    FUN_00002cf8(auStack_143c,0x15d4b9);
                    FUN_00011dc4(auStack_30,auStack_143c,&DAT_00208c50);
                    FUN_00002cf8(auStack_1440,0x15d4be);
                    FUN_00011dc4(auStack_30,auStack_1440,&DAT_00208c50);
                    FUN_00002cf8(auStack_1444,0x15d4bf);
                    FUN_00011dc4(auStack_30,auStack_1444,&DAT_00208c50);
                    FUN_00002cf8(auStack_1448,0x15d4be);
                    FUN_00013178(auStack_30,auStack_1448,&DAT_00208c50);
                    FUN_00002cf8(auStack_144c,0x15d4af);
                    FUN_00011dc4(auStack_30,auStack_144c,&DAT_00208c50);
                    FUN_00002cf8(auStack_1450,0x15d4b1);
                    FUN_00011dc4(auStack_30,auStack_1450,&DAT_00208c50);
                    FUN_00002cf8(auStack_1454,0x15d324);
                    FUN_00011dc4(auStack_30,auStack_1454,&DAT_00208c50);
                    FUN_00002cf8(auStack_1458,0x15d323);
                    FUN_00011dc4(auStack_30,auStack_1458,&DAT_00208c50);
                    FUN_00002cf8(auStack_145c,0x15d323);
                    FUN_00013178(auStack_30,auStack_145c,&DAT_00208c50);
                    FUN_00002cf8(auStack_1460,0x15d326);
                    FUN_000193a8(auStack_30,auStack_1460);
                    FUN_00002cf8(auStack_1464,0x15d326);
                    FUN_00011dc4(auStack_30,auStack_1464,&DAT_00208c50);
                  }
                  else {
                    FUN_00002cf8(auStack_7c4,0x15d32f);
                    FUN_000138a8(auStack_7c0,auStack_30,auStack_7c4);
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                              (auStack_810,"0x15d300kGCCalculateKey",0);
                    sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_7c0,auStack_810,1,0);
                    __ZN8PMStringD1Ev(auStack_810);
                    __ZN8PMStringD1Ev(auStack_7c0);
                    if (sVar2 != 0) {
                      FUN_00002cf8(auStack_814,0x15d324);
                      FUN_00011dc4(auStack_30,auStack_814,&DAT_00208c52);
                    }
                    FUN_00002cf8(auStack_818,0x15d324);
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                              (auStack_860,"0x15d300kGCSquareKey",0);
                    FUN_00013588(auStack_30,auStack_818,auStack_860);
                    __ZN8PMStringD1Ev(auStack_860);
                    FUN_00002cf8(auStack_864,0x15d323);
                    FUN_00011dc4(auStack_30,auStack_864,&DAT_00208c52);
                    FUN_00002cf8(auStack_868,0x15d323);
                    FUN_00013178(auStack_30,auStack_868,&DAT_00208c52);
                    FUN_00002cf8(auStack_86c,0x15d4a3);
                    sVar2 = FUN_00012ef4(auStack_30,auStack_86c);
                    if (sVar2 == 0) {
                      local_8c8 = FUN_000157c8(&local_518,&local_768);
                      local_8c0 = FUN_00015abc(&local_8c8);
                      local_8b8 = FUN_000157c8(&local_518,&local_8c0);
                      local_e0 = local_8b8;
                    }
                    else {
                      FUN_00002cf8(auStack_87c,0x15d4a9);
                      local_878 = FUN_0001544c(auStack_30,auStack_87c,auStack_4f8);
                      FUN_00013490(0);
                      sVar2 = FUN_000132fc(&local_878,auStack_888);
                      if (sVar2 != 0) {
                        FUN_00002cf8(auStack_894,&DAT_0015d31b);
                        local_890 = FUN_0001544c(auStack_30,auStack_894,auStack_4f8);
                        local_878 = local_890;
                      }
                      local_8b0 = FUN_000157c8(&local_878,&local_768);
                      local_8a8 = FUN_00015abc(&local_8b0);
                      local_8a0 = FUN_000157c8(&local_878,&local_8a8);
                      local_e0 = local_8a0;
                    }
                    plVar6 = (long *)FUN_0000e2e0(auStack_d8);
                    (**(code **)(*plVar6 + 0x28))(plVar6,&local_e0);
                    local_8d8 = FUN_000157c8(&local_518,&local_768);
                    local_8d0 = FUN_00015abc(&local_8d8);
                    local_8da = 0;
                    FUN_00002cf8(auStack_92c,0x15d32f);
                    FUN_000138a8(auStack_928,auStack_30,auStack_92c);
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                              (auStack_978,"0x15d300kGCCalculateKey",0);
                    sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_928,auStack_978,1,0);
                    __ZN8PMStringD1Ev(auStack_978);
                    __ZN8PMStringD1Ev(auStack_928);
                    if (sVar2 != 0) {
                      local_988 = FUN_000157c8(&local_508,&local_518);
                      local_980 = FUN_00015a7c(&local_e0,&local_988);
                      local_538 = local_980;
                      plVar6 = (long *)FUN_00017c8c(auStack_a8);
                      (**(code **)(*plVar6 + 0x68))(plVar6,&local_538);
                      local_98c = 3;
                      FUN_000152f0(&local_538,&local_98c);
                      plVar6 = (long *)FUN_000029d8(auStack_50);
                      (**(code **)(*plVar6 + 0xa0))(plVar6,&local_538);
                      plVar6 = (long *)FUN_000029d8(auStack_50);
                      local_990 = FUN_00032070(&local_8d0);
                      (**(code **)(*plVar6 + 0xf0))(plVar6,&local_990);
                      FUN_00002cf8(auStack_994,0x15d330);
                      local_9a0 = FUN_00015a7c(&local_538,lVar4 + 0x88);
                      FUN_00013c20(auStack_30,auStack_994,&local_9a0);
                      local_8da = 1;
                    }
                    plVar6 = (long *)FUN_00017c8c(auStack_a8);
                    local_9a4 = 1;
                    (**(code **)(*plVar6 + 0x78))(plVar6,&local_9a4);
                    plVar6 = (long *)FUN_00017c8c(auStack_a8);
                    local_9a8 = 1;
                    (**(code **)(*plVar6 + 0x98))(plVar6,&local_9a8);
                    FUN_00015c98(auStack_30,&local_e0);
                    local_9ac = 4;
                    FUN_000152f0(&local_e0,&local_9ac);
                    FUN_00002cf8(auStack_9b0,0x15d31d);
                    FUN_00013c20(auStack_30,auStack_9b0,&local_e0);
                    plVar6 = (long *)FUN_00017c8c(auStack_a8);
                    (**(code **)(*plVar6 + 0x88))(plVar6,&local_e0);
                    plVar6 = (long *)FUN_000029d8(auStack_50);
                    local_9b4 = FUN_00032070(&local_8d0);
                    (**(code **)(*plVar6 + 0x100))(plVar6,&local_9b4);
                    plVar6 = (long *)FUN_000029d8(auStack_50);
                    local_9b8 = 1;
                    (**(code **)(*plVar6 + 0xb0))(plVar6,&local_9b8);
                    plVar6 = (long *)FUN_000029d8(auStack_50);
                    (**(code **)(*plVar6 + 0xd0))(plVar6,&local_e0);
                    plVar6 = (long *)FUN_000029d8(auStack_50);
                    local_9bc = 1;
                    (**(code **)(*plVar6 + 0xe0))(plVar6,&local_9bc);
                    plVar6 = (long *)FUN_00030cd4(auStack_a0);
                    FUN_00002cf8(auStack_9cc,0x15d31c);
                    local_9c8 = FUN_00013338(auStack_30,auStack_9cc);
                    (**(code **)(*plVar6 + 0x38))(plVar6,&local_9c8);
                    plVar6 = (long *)FUN_00030cd4(auStack_a0);
                    FUN_00002cf8(auStack_9dc,0x15d31d);
                    local_9d8 = FUN_00013338(auStack_30,auStack_9dc);
                    (**(code **)(*plVar6 + 0x48))(plVar6,&local_9d8);
                    FUN_00002cf8(auStack_9e0,0x15d4e6);
                    local_9f0 = FUN_00015abc(&local_e0);
                    iVar3 = FUN_00032070(&local_9f0);
                    FUN_00013490((double)iVar3);
                    FUN_000196d0(auStack_30,auStack_9e0,auStack_9e8);
                    FUN_001cd3e4(auStack_30);
                    FUN_00138008(lVar4);
                    FUN_00014a5c(auStack_30,&local_e0);
                    local_9f2 = 0;
                    FUN_00002cf8(auStack_9f8,0x15d4a3);
                    sVar2 = FUN_00012ef4(auStack_30,auStack_9f8);
                    bVar1 = true;
                    if (sVar2 == 0) {
                      FUN_00002cf8(auStack_9fc,0x15d4a4);
                      sVar2 = FUN_00012ef4(auStack_30,auStack_9fc);
                      bVar1 = sVar2 != 0;
                    }
                    if (bVar1) {
                      local_9f2 = 1;
                    }
                    FUN_00002cf8(auStack_a00,0x15d326);
                    local_a02 = (ushort)(local_9f2 == 0);
                    FUN_00011dc4(auStack_30,auStack_a00,&local_a02);
                    FUN_00002cf8(auStack_a54,0x15d30a);
                    local_aed = 0;
                    local_b39 = 0;
                    FUN_000138a8(auStack_a50,auStack_30,auStack_a54);
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                              (auStack_aa0,"0x15d300kGCHideGridKey",0);
                    sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_a50,auStack_aa0,1,0);
                    bVar1 = true;
                    if (sVar2 == 0) {
                      FUN_00002cf8(auStack_aec,0x15d30a);
                      FUN_000138a8(auStack_ae8,auStack_30,auStack_aec);
                      local_aed = 1;
                      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                                (auStack_b38,"0x15d300kGCBaselineKey",0);
                      local_b39 = 1;
                      sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_ae8,auStack_b38,1,0);
                      bVar1 = sVar2 != 0;
                    }
                    if ((local_b39 & 1) != 0) {
                      __ZN8PMStringD1Ev(auStack_b38);
                    }
                    if ((local_aed & 1) != 0) {
                      __ZN8PMStringD1Ev(auStack_ae8);
                    }
                    __ZN8PMStringD1Ev(auStack_aa0);
                    __ZN8PMStringD1Ev(auStack_a50);
                    local_9f2 = (ushort)!bVar1;
                    FUN_001ae290(auStack_30,&local_e0,&local_9f2);
                    plVar6 = (long *)FUN_000029d8(auStack_50);
                    (**(code **)(*plVar6 + 0x110))(plVar6,&local_9f2);
                    if (local_9f2 == 0) {
                      plVar6 = (long *)FUN_000029d8(auStack_50);
                      (**(code **)(*plVar6 + 0x120))(plVar6,&DAT_00208c50);
                    }
                    FUN_00002cf8(auStack_b40,0x15d3aa);
                    local_b44 = 2;
                    FUN_00015844(auStack_30,auStack_b40,&local_b44,&DAT_00208c52);
                    FUN_00002cf8(auStack_b48,0x15d3a1);
                    FUN_000187cc(auStack_30,auStack_b48,&DAT_00208c50,&DAT_00208c52);
                    FUN_00002cf8(auStack_b4c,0x15d3a2);
                    FUN_000187cc(auStack_30,auStack_b4c,&DAT_00208c50,&DAT_00208c52);
                    FUN_00167d6c(lVar4,&DAT_00208c52,&DAT_00208c50);
                    if (local_8da != 0) {
                      FUN_00167d6c(lVar4,&DAT_00208c50);
                    }
                    FUN_00002cf8(auStack_b9c,0x15d4c9);
                    local_ba0 = 0xffffffff;
                    FUN_00012c10(auStack_b98,auStack_30,auStack_b9c,&local_ba0);
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                              (auStack_be8,"Custom Setting Applied",0);
                    sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_b98,auStack_be8,1,0);
                    __ZN8PMStringD1Ev(auStack_be8);
                    __ZN8PMStringD1Ev(auStack_b98);
                    if (sVar2 == 0) {
                      FUN_00002cf8(auStack_bec,0x15d4c9);
                      local_bf0 = 0;
                      FUN_00015b04(auStack_30,auStack_bec,&local_bf0,1);
                      FUN_00002cf8(auStack_bf4,0x15d3fe);
                      local_bf8 = 0;
                      FUN_00015b04(auStack_30,auStack_bf4,&local_bf8,1);
                    }
                    FUN_00002cf8(auStack_c44,0x15d4cf);
                    local_c48 = 0xffffffff;
                    FUN_00012c10(auStack_c40,auStack_30,auStack_c44,&local_c48);
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                              (auStack_c90,"Custom Setting Applied",0);
                    sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_c40,auStack_c90,1,0);
                    __ZN8PMStringD1Ev(auStack_c90);
                    __ZN8PMStringD1Ev(auStack_c40);
                    if (sVar2 == 0) {
                      FUN_00002cf8(auStack_c94,0x15d4cf);
                      local_c98 = 0;
                      FUN_00015b04(auStack_30,auStack_c94,&local_c98,1);
                    }
                    FUN_00002cf8(auStack_c9c,0x15d49a);
                    FUN_000196d0(auStack_30,auStack_c9c,&local_8d0);
                    FUN_00002cf8(auStack_ca0,0x15d49a);
                    FUN_00129a38(lVar4,auStack_ca0,&DAT_00208c50,&DAT_00208c52);
                    FUN_00002cf8(auStack_ca4,0x15d49c);
                    FUN_000196d0(auStack_30,auStack_ca4,&local_8d0);
                    FUN_00002cf8(auStack_ca8,0x15d49c);
                    FUN_00129a38(lVar4,auStack_ca8,&DAT_00208c50,&DAT_00208c52);
                    FUN_00002cf8(auStack_cac,0x15d4a3);
                    sVar2 = FUN_00012ef4(auStack_30,auStack_cac);
                    if (sVar2 == 0) {
                      FUN_00002cf8(auStack_cb0,0x15d49a);
                      FUN_00011dc4(auStack_30,auStack_cb0,&DAT_00208c52);
                      FUN_00002cf8(auStack_cb4,0x15d49c);
                      FUN_00011dc4(auStack_30,auStack_cb4,&DAT_00208c52);
                      FUN_00002cf8(auStack_cb8,0x15d4c5);
                      FUN_00013178(auStack_30,auStack_cb8,&DAT_00208c50);
                      plVar6 = (long *)FUN_000029d8(auStack_50);
                      FUN_00013490(0);
                      (**(code **)(*plVar6 + 0x1e0))(plVar6,auStack_cc0);
                    }
                    else {
                      FUN_00002cf8(auStack_cc4,0x15d49b);
                      FUN_0012fc3c(lVar4,auStack_cc4,&DAT_00208c50);
                      FUN_00002cf8(auStack_cc8,0x15d49d);
                      FUN_0012fc3c(lVar4,auStack_cc8,&DAT_00208c50);
                    }
                    FUN_00002cf8(auStack_ccc,0x15d4a4);
                    sVar2 = FUN_00012ef4(auStack_30,auStack_ccc);
                    if (sVar2 == 0 && local_8da != 0) {
                      FUN_00002cf8(auStack_d1c,0x15d4c7);
                      local_d20 = 0xffffffff;
                      FUN_00012c10(auStack_d18,auStack_30,auStack_d1c,&local_d20);
                      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                                (auStack_d68,"Custom Setting Applied",0);
                      sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_d18,auStack_d68,1,0);
                      __ZN8PMStringD1Ev(auStack_d68);
                      __ZN8PMStringD1Ev(auStack_d18);
                      if (sVar2 == 0) {
                        FUN_00002cf8(auStack_d6c,0x15d4c7);
                        local_d70 = 0;
                        FUN_00015b04(auStack_30,auStack_d6c,&local_d70,1);
                        FUN_00002cf8(auStack_d74,0x15d4c8);
                        local_d78 = 0;
                        FUN_00015b04(auStack_30,auStack_d74,&local_d78,1);
                      }
                      FUN_00002cf8(auStack_dc4,0x15d4cc);
                      local_dc8 = 0xffffffff;
                      FUN_00012c10(auStack_dc0,auStack_30,auStack_dc4,&local_dc8);
                      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                                (auStack_e10,"Custom Setting Applied",0);
                      sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_dc0,auStack_e10,1,0);
                      __ZN8PMStringD1Ev(auStack_e10);
                      __ZN8PMStringD1Ev(auStack_dc0);
                      if (sVar2 == 0) {
                        FUN_00002cf8(auStack_e14,0x15d4cc);
                        local_e18 = 0;
                        FUN_00015b04(auStack_30,auStack_e14,&local_e18,1);
                      }
                      FUN_00002cf8(auStack_e1c,0x15d49e);
                      FUN_00011dc4(auStack_30,auStack_e1c,&DAT_00208c52);
                      FUN_00002cf8(auStack_e20,0x15d4a0);
                      FUN_00011dc4(auStack_30,auStack_e20,&DAT_00208c52);
                      FUN_00002cf8(auStack_e24,0x15d4c6);
                      FUN_00013178(auStack_30,auStack_e24,&DAT_00208c50);
                      plVar6 = (long *)FUN_000029d8(auStack_50);
                      FUN_00013490(0);
                      (**(code **)(*plVar6 + 0x1f0))(plVar6,auStack_e30);
                      FUN_00002cf8(auStack_e34,0x15d49e);
                      FUN_000196d0(auStack_30,auStack_e34,&local_8d0);
                      FUN_00002cf8(auStack_e38,0x15d49e);
                      FUN_0012dd50(lVar4,auStack_e38,&DAT_00208c50);
                      FUN_00002cf8(auStack_e3c,0x15d4a0);
                      FUN_000196d0(auStack_30,auStack_e3c,&local_8d0);
                      FUN_00002cf8(auStack_e40,0x15d4a0);
                      FUN_0012dd50(lVar4,auStack_e40,&DAT_00208c50);
                    }
                    else if (local_8da != 0) {
                      FUN_00002cf8(auStack_e44,0x15d49f);
                      FUN_0012dd50(lVar4,auStack_e44,&DAT_00208c50);
                      FUN_00002cf8(auStack_e48,0x15d4a1);
                      FUN_0012dd50(lVar4,auStack_e48,&DAT_00208c50);
                    }
                    FUN_00002cf8(auStack_e4c,0x15d4b3);
                    FUN_00011dc4(auStack_30,auStack_e4c,&DAT_00208c52);
                    FUN_00002cf8(auStack_e50,0x15d4b5);
                    FUN_00011dc4(auStack_30,auStack_e50,&DAT_00208c52);
                    FUN_00002cf8(auStack_e54,0x15d4e0);
                    FUN_00011dc4(auStack_30,auStack_e54,&DAT_00208c52);
                    FUN_00002cf8(auStack_e58,0x15d4e1);
                    FUN_00011dc4(auStack_30,auStack_e58,&DAT_00208c52);
                    FUN_00002cf8(auStack_e5c,0x15d4e0);
                    FUN_00013178(auStack_30,auStack_e5c,&DAT_00208c52);
                    FUN_00002cf8(auStack_e60,0x15d4bb);
                    FUN_00011dc4(auStack_30,auStack_e60,&DAT_00208c52);
                    FUN_00002cf8(auStack_e64,0x15d4bd);
                    FUN_00011dc4(auStack_30,auStack_e64,&DAT_00208c52);
                    FUN_00002cf8(auStack_e68,0x15d4c0);
                    FUN_00011dc4(auStack_30,auStack_e68,&DAT_00208c52);
                    FUN_00002cf8(auStack_e6c,0x15d4c1);
                    FUN_00011dc4(auStack_30,auStack_e6c,&DAT_00208c52);
                    FUN_00002cf8(auStack_e70,0x15d4c0);
                    FUN_00013178(auStack_30,auStack_e70,&DAT_00208c52);
                    FUN_00002cf8(auStack_e74,0x15d4ab);
                    FUN_00011dc4(auStack_30,auStack_e74,&DAT_00208c52);
                    FUN_00002cf8(auStack_e78,0x15d4ad);
                    FUN_00011dc4(auStack_30,auStack_e78,&DAT_00208c52);
                    FUN_00002cf8(auStack_e7c,0x15d4b7);
                    FUN_00011dc4(auStack_30,auStack_e7c,&DAT_00208c52);
                    FUN_00002cf8(auStack_e80,0x15d4b9);
                    FUN_00011dc4(auStack_30,auStack_e80,&DAT_00208c52);
                    FUN_00002cf8(auStack_e84,0x15d4be);
                    FUN_00011dc4(auStack_30,auStack_e84,&DAT_00208c52);
                    FUN_00002cf8(auStack_e88,0x15d4bf);
                    FUN_00011dc4(auStack_30,auStack_e88,&DAT_00208c52);
                    FUN_00002cf8(auStack_e8c,0x15d4be);
                    FUN_00013178(auStack_30,auStack_e8c,&DAT_00208c52);
                    if (*(short *)(lVar4 + 0x90) == 0) {
                      FUN_00002cf8(auStack_e90,0x15d4c9);
                      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_ed8,"rows",0);
                      local_ee0 = FUN_00015a7c(&local_8d0,&local_e0);
                      FUN_00013490();
                      FUN_00013490(0);
                      FUN_001668f4(lVar4,auStack_e90,auStack_ed8,&local_ee0,&local_e0,auStack_ee8,
                                   auStack_ef0);
                      __ZN8PMStringD1Ev(auStack_ed8);
                      FUN_00002cf8(auStack_ef8,0x15d4c9);
                      local_ef4 = FUN_0001853c(auStack_30,auStack_ef8);
                      if (local_ef4 < 1) {
                        FUN_00002cf8(auStack_f18,0x15d4c9);
                        FUN_00011dc4(auStack_30,auStack_f18,&DAT_00208c50);
                        FUN_00002cf8(auStack_f1c,0x15d4d5);
                        FUN_00011dc4(auStack_30,auStack_f1c,&DAT_00208c50);
                        FUN_00002cf8(auStack_f20,0x15d4ca);
                        FUN_00011dc4(auStack_30,auStack_f20,&DAT_00208c50);
                        FUN_00002cf8(auStack_f24,0x15d4cb);
                        FUN_00011dc4(auStack_30,auStack_f24,&DAT_00208c50);
                      }
                      else {
                        FUN_00002cf8(auStack_efc,0x15d4c9);
                        FUN_00011dc4(auStack_30,auStack_efc,&DAT_00208c52);
                        FUN_00002cf8(auStack_f00,0x15d4d5);
                        FUN_00011dc4(auStack_30,auStack_f00,&DAT_00208c52);
                        FUN_00002cf8(auStack_f04,0x15d4c9);
                        local_f08 = 0;
                        FUN_00015b04(auStack_30,auStack_f04,&local_f08,1);
                        FUN_00002cf8(auStack_f0c,0x15d4ca);
                        FUN_00011dc4(auStack_30,auStack_f0c,&DAT_00208c52);
                        FUN_00002cf8(auStack_f10,0x15d4cb);
                        FUN_00011dc4(auStack_30,auStack_f10,&DAT_00208c52);
                        FUN_00002cf8(auStack_f14,0x15d4c9);
                        FUN_0010b924(lVar4,auStack_f14,&DAT_00208c50);
                      }
                    }
                    if (*(short *)(lVar4 + 0x94) == 0) {
                      FUN_00002cf8(auStack_f28,0x15d4cf);
                      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_f70,"rows",0);
                      local_f78 = FUN_00015a7c(&local_8d0,&local_e0);
                      FUN_00013490();
                      FUN_00013490(0);
                      FUN_001668f4(lVar4,auStack_f28,auStack_f70,&local_f78,&local_e0,auStack_f80,
                                   auStack_f88);
                      __ZN8PMStringD1Ev(auStack_f70);
                      FUN_00002cf8(auStack_f90,0x15d4cf);
                      local_f8c = FUN_0001853c(auStack_30,auStack_f90);
                      if (local_f8c < 1) {
                        FUN_00002cf8(auStack_fb0,0x15d4cf);
                        FUN_00011dc4(auStack_30,auStack_fb0,&DAT_00208c50);
                        FUN_00002cf8(auStack_fb4,0x15d4d6);
                        FUN_00011dc4(auStack_30,auStack_fb4,&DAT_00208c50);
                        FUN_00002cf8(auStack_fb8,0x15d4d0);
                        FUN_00011dc4(auStack_30,auStack_fb8,&DAT_00208c50);
                        FUN_00002cf8(auStack_fbc,0x15d4d1);
                        FUN_00011dc4(auStack_30,auStack_fbc,&DAT_00208c50);
                      }
                      else {
                        FUN_00002cf8(auStack_f94,0x15d4cf);
                        FUN_00011dc4(auStack_30,auStack_f94,&DAT_00208c52);
                        FUN_00002cf8(auStack_f98,0x15d4d6);
                        FUN_00011dc4(auStack_30,auStack_f98,&DAT_00208c52);
                        FUN_00002cf8(auStack_f9c,0x15d4d0);
                        FUN_00011dc4(auStack_30,auStack_f9c,&DAT_00208c52);
                        FUN_00002cf8(auStack_fa0,0x15d4d1);
                        FUN_00011dc4(auStack_30,auStack_fa0,&DAT_00208c52);
                        FUN_00002cf8(auStack_fa4,0x15d4cf);
                        local_fa8 = 0;
                        FUN_00015b04(auStack_30,auStack_fa4,&local_fa8,1);
                        FUN_00002cf8(auStack_fac,0x15d4cf);
                        FUN_0010b924(lVar4,auStack_fac,&DAT_00208c50);
                      }
                    }
                    if ((*(short *)(lVar4 + 0x98) == 0) && (local_8da != 0)) {
                      FUN_00002cf8(auStack_fc0,0x15d4cc);
                      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1008,"columns",0);
                      local_1010 = FUN_00015a7c(&local_8d0,&local_538);
                      FUN_00013490();
                      FUN_00013490(0);
                      FUN_001668f4(lVar4,auStack_fc0,auStack_1008,&local_1010,&local_538,
                                   auStack_1018,auStack_1020);
                      __ZN8PMStringD1Ev(auStack_1008);
                      FUN_00002cf8(auStack_1028,0x15d4cc);
                      local_1024 = FUN_0001853c(auStack_30,auStack_1028);
                      bVar1 = false;
                      if (0 < local_1024) {
                        FUN_00002cf8(auStack_102c,0x15d4a4);
                        sVar2 = FUN_00012ef4(auStack_30,auStack_102c);
                        bVar1 = sVar2 == 0;
                      }
                      if (bVar1) {
                        FUN_00002cf8(auStack_1030,0x15d4cc);
                        FUN_00011dc4(auStack_30,auStack_1030,&DAT_00208c52);
                        FUN_00002cf8(auStack_1034,0x15d4d4);
                        FUN_00011dc4(auStack_30,auStack_1034,&DAT_00208c52);
                        FUN_00002cf8(auStack_1038,0x15d4cd);
                        FUN_00011dc4(auStack_30,auStack_1038,&DAT_00208c52);
                        FUN_00002cf8(auStack_103c,0x15d4ce);
                        FUN_00011dc4(auStack_30,auStack_103c,&DAT_00208c52);
                        FUN_00002cf8(auStack_1040,0x15d4cc);
                        local_1044 = 0;
                        FUN_00015b04(auStack_30,auStack_1040,&local_1044,1);
                        FUN_0010ae20(lVar4);
                      }
                      else {
                        FUN_00002cf8(auStack_1048,0x15d4cc);
                        FUN_00011dc4(auStack_30,auStack_1048,&DAT_00208c50);
                        FUN_00002cf8(auStack_104c,0x15d4d4);
                        FUN_00011dc4(auStack_30,auStack_104c,&DAT_00208c50);
                        FUN_00002cf8(auStack_1050,0x15d4cd);
                        FUN_00011dc4(auStack_30,auStack_1050,&DAT_00208c50);
                        FUN_00002cf8(auStack_1054,0x15d4ce);
                        FUN_00011dc4(auStack_30,auStack_1054,&DAT_00208c50);
                      }
                    }
                  }
                  FUN_0014b630(lVar4,&DAT_00208c50);
                  __ZN8PMStringD1Ev(auStack_4f8);
                  __ZN8PMStringD1Ev(auStack_3f0);
                  __ZN8PMStringD1Ev(auStack_130);
                  local_48 = 0;
                }
                else {
                  local_48 = 2;
                }
                FUN_0000e324(auStack_d8);
              }
              else {
                local_48 = 2;
              }
              FUN_00017ca4(auStack_b8);
            }
            else {
              local_48 = 2;
            }
            FUN_0000e2f8(auStack_b0);
          }
          else {
            local_48 = 2;
          }
          FUN_00017158(auStack_a8);
        }
        else {
          local_48 = 2;
        }
        FUN_000325e4(auStack_a0);
      }
      else {
        local_48 = 2;
      }
      FUN_0001d164(auStack_80);
    }
    else {
      local_48 = 2;
    }
    FUN_0000e06c(auStack_60);
  }
  else {
    local_48 = 2;
  }
  FUN_00002b9c(auStack_50);
LAB_000ed938:
  FUN_00002c54(auStack_30);
  return;
}
