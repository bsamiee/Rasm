/*
Image-line heights, the top margin with image-lines, and size matching (rows 17, 18, 19). FUN_001ca224 measures a glyph: it
creates a text frame on the '[GC] Text' layer, applies the font (kTextAttrFontUID 0x1b2b), style (0x1b02) and size (0x1b03),
inserts the glyph string, converts it to outlines, reads the outline bounding box through IGeometry (vtable + 0x20) and returns
bottom - top (PMRect fields at +0x18 and +0x8, subtracted at 0x28080) quantised to 0.001. FUN_00163ee0 writes the top margin as
k_t * L + x when x != 0. FUN_0014f5bc matches the image-line height to the grid width: sizes climb from 4 pt in 1 pt steps
until the measured height reaches u_h, then a bisection to 0.001 pt between the last two sizes, else the closest size and an alert.

PMReal helpers: construct 0x13490, operator/ 0x157c8, operator* 0x15a7c, operator+ 0x17c4c, operator- 0x28080,
Round 0x15abc (floor(x + 0.5)), floor 0x37d70, a > b + 1e-8 0x14140, a >= b - 1e-8 0x15808, a <= b + 1e-8 0x31d38,
a < b - 1e-8 0x3208c, |a - b| < 1e-8 0x132fc, not equal 0x18504, quantise 0x152f0 (mode 3 three decimals, mode 4 four).
Widget access: read value 0x13338, read value in the panel unit 0x1544c, read checkbox 0x12ef4, read dropdown index 0x13034,
write value 0x13c20, set maximum 0x196d0, unit factor at object + 0x88.
*/

//==== FUNC @ 1ca224 -> 001ca224

/* WARNING: Restarted to delay deadcode elimination for space: stack */

undefined1  [16]
FUN_001ca224(undefined1 param_1 [16],undefined8 param_2,undefined8 param_3,undefined8 param_4,
            undefined8 param_5,undefined4 *param_6,undefined8 param_7,undefined8 param_8,
            undefined8 param_9)

{
  bool bVar1;
  short sVar2;
  uint uVar3;
  ulong uVar4;
  undefined8 uVar5;
  long *plVar6;
  long *plVar7;
  long lVar8;
  undefined8 *puVar9;
  undefined8 uVar10;
  undefined1 auVar11 [16];
  undefined4 local_3cc;
  undefined1 auStack_3c8 [15];
  undefined1 uStack_3b9;
  undefined1 local_3b8 [16];
  undefined1 auStack_3a8 [8];
  undefined1 auStack_3a0 [12];
  undefined4 local_394;
  undefined1 auStack_390 [12];
  undefined4 local_384;
  ulong local_380;
  undefined8 local_378;
  undefined8 local_370;
  undefined8 local_368;
  undefined8 local_360;
  undefined1 uStack_351;
  undefined1 local_350 [16];
  undefined1 auStack_340 [8];
  undefined1 auStack_338 [8];
  undefined1 auStack_330 [8];
  undefined1 auStack_328 [8];
  undefined1 auStack_320 [12];
  undefined4 local_314;
  undefined1 auStack_310 [8];
  undefined1 auStack_308 [23];
  undefined1 uStack_2f1;
  undefined1 auStack_2f0 [15];
  undefined1 uStack_2e1;
  undefined1 auStack_2e0 [8];
  undefined1 local_2d8 [16];
  undefined1 local_2c8 [16];
  undefined1 auStack_2b8 [16];
  undefined1 auStack_2a8 [8];
  undefined4 local_2a0;
  undefined4 local_29c;
  undefined1 auStack_298 [8];
  undefined4 local_290;
  undefined4 local_28c;
  undefined1 auStack_288 [12];
  undefined4 local_27c;
  undefined4 local_278;
  undefined4 local_274;
  undefined1 auStack_270 [8];
  undefined1 auStack_268 [24];
  undefined1 auStack_250 [8];
  undefined8 local_248;
  undefined8 local_240;
  undefined8 local_238;
  undefined1 auStack_230 [8];
  undefined1 auStack_228 [8];
  undefined1 auStack_220 [32];
  undefined4 local_200;
  undefined4 local_1fc;
  undefined1 auStack_1f8 [8];
  undefined8 local_1f0;
  undefined4 local_1e4;
  undefined1 auStack_1e0 [15];
  undefined1 uStack_1d1;
  undefined1 auStack_1d0 [12];
  undefined4 local_1c4;
  undefined1 auStack_1c0 [12];
  undefined4 local_1b4;
  undefined1 local_1b0 [16];
  undefined1 auStack_1a0 [20];
  undefined4 local_18c;
  undefined1 auStack_188 [15];
  undefined1 uStack_179;
  undefined1 local_178 [16];
  undefined1 auStack_168 [15];
  undefined1 uStack_159;
  undefined1 auStack_158 [12];
  undefined4 local_14c;
  undefined1 auStack_148 [23];
  undefined1 uStack_131;
  undefined1 auStack_130 [8];
  undefined1 local_128 [16];
  undefined1 auStack_118 [8];
  undefined4 local_110;
  undefined4 local_10c;
  undefined1 auStack_108 [72];
  undefined4 local_c0;
  undefined1 uStack_b9;
  undefined1 auStack_b8 [15];
  undefined1 uStack_a9;
  undefined1 auStack_a8 [8];
  undefined1 local_a0 [16];
  undefined4 local_90;
  undefined1 uStack_79;
  undefined1 auStack_78 [8];
  undefined1 auStack_70 [8];
  undefined1 auStack_68 [8];
  undefined8 local_60;
  undefined8 local_58;
  undefined8 local_50;
  undefined4 *local_48;
  undefined8 local_40;
  ulong local_38 [2];
  undefined8 local_28;
  
  local_60 = param_9;
  local_58 = param_8;
  local_50 = param_7;
  local_48 = param_6;
  local_40 = param_5;
  FUN_00013490(0,local_38);
  __ZN6CAlert13SetShowAlertsEs(0);
  uVar4 = FUN_00002e24(local_48,&DAT_00208e3c);
  bVar1 = true;
  if ((uVar4 & 1) == 0) {
    sVar2 = __ZNK8PMString7IsEmptyEv(local_50);
    uVar5 = local_58;
    bVar1 = true;
    if (sVar2 == 0) {
      FUN_00013490(0);
      sVar2 = FUN_000132fc(uVar5,auStack_68);
      uVar5 = local_60;
      bVar1 = true;
      if (sVar2 == 0) {
        FUN_00013490(0);
        sVar2 = FUN_000132fc(uVar5,auStack_70);
        bVar1 = sVar2 != 0;
      }
    }
  }
  if (!bVar1) {
    uVar5 = __Z26GetExecutionContextSessionv();
    FUN_00002978(auStack_78,uVar5,&uStack_79);
    sVar2 = FUN_000029b4(auStack_78);
    if (sVar2 == 0) {
      plVar6 = (long *)FUN_000029d8(auStack_78);
      local_a0 = (**(code **)(*plVar6 + 0x18))();
      FUN_0000df20(auStack_a8,local_a0,&uStack_a9);
      sVar2 = FUN_0000df5c(auStack_a8);
      if (sVar2 == 0) {
        uVar5 = FUN_0000df80(auStack_a8);
        FUN_0000df98(auStack_b8,uVar5,&uStack_b9);
        sVar2 = FUN_0000dfd4(auStack_b8);
        if (sVar2 == 0) {
          plVar6 = (long *)FUN_0000dff8(auStack_b8);
          __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_108,"[GC] Text",0);
          local_c0 = (**(code **)(*plVar6 + 0x20))(plVar6,auStack_108);
          __ZN8PMStringD1Ev(auStack_108);
          plVar6 = (long *)FUN_0000dff8(auStack_b8);
          local_110 = local_c0;
          local_10c = (**(code **)(*plVar6 + 0x40))(plVar6,local_c0);
          plVar6 = (long *)FUN_0000dff8(auStack_b8);
          uVar5 = (**(code **)(*plVar6 + 0x28))(plVar6,local_10c);
          FUN_000ca9c4(auStack_118,uVar5);
          sVar2 = FUN_000caa24(auStack_118);
          if (sVar2 == 0) {
            plVar6 = (long *)FUN_000029d8(auStack_78);
            local_128 = (**(code **)(*plVar6 + 0x28))();
            FUN_0001b840(auStack_130,local_128,&uStack_131);
            sVar2 = FUN_0001b87c(auStack_130);
            if (sVar2 == 0) {
              uVar5 = FUN_0000310c(local_a0);
              plVar6 = (long *)FUN_0001c5a8(auStack_130);
              local_14c = (**(code **)(*plVar6 + 0x50))(plVar6,0);
              FUN_00032d2c(auStack_148,uVar5,local_14c);
              FUN_000cab28(auStack_158,auStack_148,&uStack_159);
              sVar2 = FUN_000cab64(auStack_158);
              if (sVar2 == 0) {
                plVar6 = (long *)FUN_0001b750(auStack_a8);
                local_178 = (**(code **)(*plVar6 + 0x88))();
                FUN_0001b768(auStack_168,local_178,&uStack_179);
                sVar2 = FUN_0001b7a4(auStack_168);
                if (sVar2 == 0) {
                  uVar5 = FUN_0001b7c8(auStack_168);
                  FUN_00002f44(&local_18c,0xca0c);
                  FUN_0001de4c(auStack_188,uVar5,local_18c);
                  sVar2 = FUN_0001de90(auStack_188);
                  if (sVar2 == 0) {
                    plVar6 = (long *)FUN_000029d8(auStack_78);
                    local_1b0 = (**(code **)(*plVar6 + 0x18))();
                    uVar5 = FUN_0000310c(local_1b0);
                    plVar6 = (long *)FUN_0001dee4(auStack_188);
                    local_1b4 = (**(code **)(*plVar6 + 0x20))();
                    FUN_00032d2c(auStack_1a0,uVar5,local_1b4);
                    FUN_00002f44(&local_1c4,0x218);
                    FUN_0016a534(auStack_1c0,auStack_1a0,local_1c4);
                    sVar2 = FUN_0016a578(auStack_1c0);
                    if (sVar2 == 0) {
                      uVar5 = FUN_0000df80(auStack_a8);
                      FUN_00030adc(auStack_1d0,uVar5,&uStack_1d1);
                      sVar2 = FUN_00030b18(auStack_1d0);
                      if (sVar2 == 0) {
                        plVar6 = (long *)FUN_00030bb0(auStack_1d0);
                        plVar7 = (long *)FUN_0001dee4(auStack_188);
                        local_1e4 = (**(code **)(*plVar7 + 0x20))();
                        uVar5 = (**(code **)(*plVar6 + 0x18))(plVar6,local_1e4);
                        FUN_00030bc8(auStack_1e0,uVar5);
                        uVar3 = FUN_00030bfc(auStack_1e0);
                        if ((uVar3 & 1) == 0) {
                          FUN_00013490(0,&local_1f0);
                          uVar5 = FUN_00030c2c(auStack_1e0);
                          FUN_000069c0(&local_1fc,&DAT_00001b18);
                          FUN_00002f44(&local_200,&DAT_00001b06);
                          uVar5 = __ZNK17AttributeBossList14QueryByClassIDE6IDTypeI11ClassID_tagES0_I9PMIID_tagE
                                            (uVar5,local_1fc,local_200);
                          FUN_00030c44(auStack_1f8,uVar5);
                          lVar8 = FUN_00030c78(auStack_1f8);
                          if (lVar8 != 0) {
                            uVar5 = FUN_00030c90(auStack_1f8);
                            puVar9 = (undefined8 *)FUN_00032044(uVar5);
                            local_1f0 = *puVar9;
                          }
                          FUN_00013490(auStack_228);
                          FUN_00013490(0,auStack_230);
                          local_240 = FUN_00017c4c(local_60,local_58);
                          local_238 = FUN_00017c4c(&local_240,&local_1f0);
                          local_248 = FUN_00017c4c(local_60,local_58);
                          __ZN6PMRectC1ERK6PMRealS2_S2_S2_
                                    (auStack_220,auStack_228,auStack_230,&local_238,&local_248);
                          plVar6 = (long *)FUN_0001c5a8(auStack_130);
                          uVar5 = FUN_000caa74(auStack_118);
                          uVar5 = (**(code **)(*plVar6 + 0x18))(plVar6,uVar5,0,0);
                          FUN_000caa8c(auStack_250,uVar5);
                          sVar2 = FUN_000cab04(auStack_250);
                          if (sVar2 == 0) {
                            __ZN17AttributeBossListC1Ev(auStack_268);
                            FUN_000069c0(&local_274,&DAT_00001b2b);
                            uVar5 = FUN_0016a3fc(local_274);
                            FUN_00031da8(auStack_270,uVar5);
                            lVar8 = FUN_00031e10(auStack_270);
                            if (lVar8 != 0) {
                              uVar5 = FUN_00031e58(auStack_270);
                              local_278 = *local_48;
                              FUN_0016a454(uVar5,local_278);
                              uVar5 = FUN_00031e10(auStack_270);
                              local_27c = 0;
                              __ZN17AttributeBossList14ApplyAttributeEPK10IPMUnknown6IDTypeI11ClassID_tagE
                                        (auStack_268,uVar5,0);
                            }
                            FUN_000069c0(&local_28c,&DAT_00001b02);
                            uVar5 = FUN_0016a498(local_28c);
                            FUN_00031ddc(auStack_288,uVar5);
                            lVar8 = FUN_00031e28(auStack_288);
                            if (lVar8 != 0) {
                              plVar6 = (long *)FUN_00031e40(auStack_288);
                              (**(code **)(*plVar6 + 0x20))(plVar6,local_50,1);
                              uVar5 = FUN_00031e28(auStack_288);
                              local_290 = 0;
                              __ZN17AttributeBossList14ApplyAttributeEPK10IPMUnknown6IDTypeI11ClassID_tagE
                                        (auStack_268,uVar5,0);
                            }
                            FUN_000069c0(&local_29c,&DAT_00001b03);
                            uVar5 = FUN_00032adc(local_29c);
                            FUN_00030c44(auStack_298,uVar5);
                            lVar8 = FUN_00030c78(auStack_298);
                            if (lVar8 != 0) {
                              uVar5 = FUN_00030c90(auStack_298);
                              FUN_00032b34(uVar5,local_58);
                              uVar5 = FUN_00030c78(auStack_298);
                              local_2a0 = 0;
                              __ZN17AttributeBossList14ApplyAttributeEPK10IPMUnknown6IDTypeI11ClassID_tagE
                                        (auStack_268,uVar5,0);
                            }
                            FUN_000c9fbc(auStack_2a8);
                            FUN_00084ed0(auStack_2b8);
                            uVar5 = FUN_000caac0(auStack_250);
                            local_2d8 = __Z9GetUIDRefPK10IPMUnknown(uVar5);
                            local_2c8 = FUN_000cadf4(auStack_2a8,local_2d8,auStack_220,0,0,
                                                     auStack_2b8,1);
                            sVar2 = FUN_000029f0(local_2c8,PTR___ZN6UIDRef5gNullE_00234018);
                            if (sVar2 == 0) {
                              FUN_001cb830(auStack_2e0,auStack_2b8,&uStack_2e1);
                              uVar5 = FUN_001cb86c(auStack_2e0);
                              FUN_001cb884(auStack_2f0,uVar5,&uStack_2f1);
                              sVar2 = FUN_001cb8c0(auStack_2f0);
                              if (sVar2 == 0) {
                                local_28 = 0x40;
                                uVar5 = __ZN8K2Memory24RTLCompatibleNewDelegateEm(0x40);
                                __ZN10WideStringC1ERK8PMString(uVar5,local_40);
                                FUN_001cb8e4(auStack_308,uVar5);
                                plVar6 = (long *)FUN_001cb918(auStack_2f0);
                                uVar5 = (**(code **)(*plVar6 + 0x28))(plVar6,0,auStack_308,0);
                                FUN_0000de90(auStack_310,uVar5);
                                sVar2 = FUN_00084d6c(auStack_310);
                                if (sVar2 == 0) {
                                  uVar5 = FUN_0000dec4(auStack_310);
                                  __ZN8CmdUtils14ProcessCommandEP8ICommand(uVar5);
                                  plVar6 = (long *)FUN_001cb930(auStack_2e0);
                                  FUN_000069c0(&local_314,0x235);
                                  (**(code **)(*plVar6 + 0x110))(plVar6,0,1,auStack_268,local_314);
                                  FUN_001cb948(auStack_328);
                                  plVar6 = (long *)FUN_001cb974(auStack_328);
                                  __ZN7UIDListC1ERK6UIDRef(auStack_330,local_2c8);
                                  uVar5 = (**(code **)(*plVar6 + 0x28))(plVar6,auStack_330,1);
                                  FUN_0000de90(auStack_320,uVar5);
                                  __ZN7UIDListD1Ev(auStack_330);
                                  FUN_001cb98c(auStack_328);
                                  uVar5 = FUN_0000dec4(auStack_320);
                                  __ZN8CmdUtils14ProcessCommandEP8ICommand(uVar5);
                                  plVar6 = (long *)FUN_0000dedc(auStack_320);
                                  uVar5 = (**(code **)(*plVar6 + 0x38))();
                                  __ZN7UIDListC1ERKS_(auStack_338,uVar5);
                                  local_350 = __ZNK7UIDList6GetRefEi(auStack_338,0);
                                  FUN_000cb444(auStack_340,local_350,&uStack_351);
                                  plVar6 = (long *)FUN_00037364(auStack_340);
                                  local_378 = (**(code **)(*plVar6 + 0x20))();
                                  local_370 = param_2;
                                  local_368 = param_3;
                                  local_360 = param_4;
                                  uVar5 = FUN_0001ea60();
                                  uVar10 = FUN_0001ea48(&local_378);
                                  local_380 = FUN_00028080(uVar5,uVar10);
                                  local_384 = 3;
                                  local_38[0] = local_380;
                                  FUN_000152f0(local_38,&local_384);
                                  FUN_000069c0(&local_394,0xf02);
                                  uVar5 = __ZN8CmdUtils13CreateCommandE6IDTypeI11ClassID_tagE
                                                    (local_394);
                                  FUN_0000de90(auStack_390,uVar5);
                                  plVar6 = (long *)FUN_0000dedc(auStack_390);
                                  (**(code **)(*plVar6 + 0x40))(plVar6,auStack_338);
                                  uVar5 = FUN_0000dec4(auStack_390);
                                  __ZN8CmdUtils14ProcessCommandEP8ICommand(uVar5);
                                  __ZN7UIDListC1ERK6UIDRef(auStack_3a0,local_2c8);
                                  local_3b8 = __ZNK7UIDList6GetRefEi(auStack_3a0,0);
                                  FUN_000cb444(auStack_3a8,local_3b8,&uStack_3b9);
                                  lVar8 = FUN_00037420(auStack_3a8);
                                  if (lVar8 != 0) {
                                    FUN_000069c0(&local_3cc,0xf02);
                                    uVar5 = __ZN8CmdUtils13CreateCommandE6IDTypeI11ClassID_tagE
                                                      (local_3cc);
                                    FUN_0000de90(auStack_3c8,uVar5);
                                    plVar6 = (long *)FUN_0000dedc(auStack_3c8);
                                    (**(code **)(*plVar6 + 0x40))(plVar6,auStack_3a0);
                                    uVar5 = FUN_0000dec4(auStack_3c8);
                                    __ZN8CmdUtils14ProcessCommandEP8ICommand(uVar5);
                                    FUN_0000def4(auStack_3c8);
                                  }
                                  FUN_0003737c(auStack_3a8);
                                  __ZN7UIDListD1Ev(auStack_3a0);
                                  FUN_0000def4(auStack_390);
                                  FUN_0003737c(auStack_340);
                                  __ZN7UIDListD1Ev(auStack_338);
                                  FUN_0000def4(auStack_320);
                                  local_90 = 0;
                                }
                                else {
                                  local_90 = 2;
                                }
                                FUN_0000def4(auStack_310);
                                FUN_001cb9b8(auStack_308);
                              }
                              else {
                                local_90 = 2;
                              }
                              FUN_001cb9e4(auStack_2f0);
                              FUN_001cba10(auStack_2e0);
                            }
                            else {
                              local_90 = 2;
                            }
                            FUN_000c9ffc(auStack_2a8);
                            FUN_00030ca8(auStack_298);
                            FUN_00032534(auStack_288);
                            FUN_00032560(auStack_270);
                            __ZN17AttributeBossListD1Ev(auStack_268);
                          }
                          else {
                            local_90 = 2;
                          }
                          FUN_000caad8(auStack_250);
                          FUN_00030ca8(auStack_1f8);
                        }
                        else {
                          local_90 = 2;
                        }
                        FUN_0003258c(auStack_1e0);
                      }
                      else {
                        local_90 = 2;
                      }
                      FUN_000325b8(auStack_1d0);
                    }
                    else {
                      local_90 = 2;
                    }
                    FUN_0016a740(auStack_1c0);
                  }
                  else {
                    local_90 = 2;
                  }
                  FUN_0001dfd8(auStack_188);
                }
                else {
                  local_90 = 2;
                }
                FUN_0001d190(auStack_168);
              }
              else {
                local_90 = 2;
              }
              FUN_000cabe4(auStack_158);
            }
            else {
              local_90 = 2;
            }
            FUN_0001d138(auStack_130);
          }
          else {
            local_90 = 2;
          }
          FUN_000caa48(auStack_118);
        }
        else {
          local_90 = 2;
        }
        FUN_0000e040(auStack_b8);
      }
      else {
        local_90 = 2;
      }
      FUN_0000e06c(auStack_a8);
    }
    else {
      local_90 = 2;
    }
    FUN_00002b9c(auStack_78);
  }
  __ZN6CAlert13SetShowAlertsEs(1);
  auVar11._8_8_ = 0;
  auVar11._0_8_ = local_38[0];
  return auVar11;
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

//==== FUNC @ 14f5bc -> 0014f5bc

void FUN_0014f5bc(long param_1)

{
  bool bVar1;
  short sVar2;
  int iVar3;
  undefined8 uVar4;
  long *plVar5;
  undefined1 auStack_780 [4];
  undefined1 auStack_77c [4];
  undefined1 auStack_778 [4];
  undefined1 auStack_774 [4];
  undefined1 auStack_770 [72];
  undefined4 local_728;
  undefined4 local_724;
  undefined1 auStack_720 [12];
  undefined1 auStack_714 [4];
  undefined8 local_710;
  undefined1 auStack_708 [24];
  undefined1 auStack_6f0 [76];
  undefined1 auStack_6a4 [4];
  undefined1 auStack_6a0 [72];
  undefined8 local_658;
  undefined1 auStack_64c [4];
  undefined1 auStack_648 [4];
  undefined4 local_644;
  undefined1 auStack_640 [8];
  undefined8 local_638;
  undefined8 local_630;
  undefined1 auStack_628 [8];
  undefined8 local_620;
  undefined8 local_618;
  undefined1 auStack_610 [4];
  undefined1 auStack_60c [4];
  undefined1 auStack_608 [8];
  undefined8 local_600;
  undefined8 local_5f8;
  undefined8 local_5f0;
  undefined4 local_5e4;
  undefined1 auStack_5e0 [8];
  undefined8 local_5d8;
  undefined8 local_5d0;
  undefined1 auStack_5c8 [8];
  undefined8 local_5c0;
  undefined8 local_5b8;
  undefined1 auStack_5b0 [8];
  undefined8 local_5a8;
  undefined1 auStack_59c [4];
  undefined1 auStack_598 [8];
  undefined8 local_590;
  undefined1 auStack_584 [4];
  undefined1 auStack_580 [8];
  undefined8 local_578;
  undefined1 auStack_570 [8];
  undefined8 local_568;
  undefined8 local_560;
  undefined1 auStack_558 [72];
  undefined1 auStack_510 [72];
  undefined1 auStack_4c8 [76];
  undefined1 auStack_47c [4];
  undefined1 auStack_478 [72];
  undefined1 auStack_430 [4];
  int local_42c;
  undefined1 auStack_428 [72];
  undefined1 auStack_3e0 [76];
  undefined1 auStack_394 [4];
  undefined1 auStack_390 [72];
  undefined1 auStack_348 [76];
  undefined1 auStack_2fc [4];
  undefined1 auStack_2f8 [72];
  undefined1 auStack_2b0 [12];
  undefined4 local_2a4;
  undefined8 local_2a0;
  undefined1 auStack_294 [4];
  undefined8 local_290;
  undefined4 local_288;
  undefined1 auStack_284 [4];
  undefined1 auStack_280 [72];
  undefined1 auStack_238 [76];
  undefined1 auStack_1ec [4];
  undefined1 auStack_1e8 [76];
  undefined1 auStack_19c [4];
  undefined1 auStack_198 [76];
  undefined1 auStack_14c [4];
  undefined1 auStack_148 [76];
  undefined1 auStack_fc [4];
  undefined1 auStack_f8 [76];
  undefined1 auStack_ac [4];
  undefined1 auStack_a8 [76];
  undefined1 auStack_5c [4];
  undefined1 auStack_58 [7];
  undefined1 uStack_51;
  undefined1 auStack_50 [8];
  undefined4 local_48;
  undefined1 uStack_31;
  undefined1 auStack_30 [8];
  long local_28;
  
  local_28 = param_1;
  FUN_00002bf4(auStack_30,param_1,&uStack_31);
  sVar2 = FUN_00002c30(auStack_30);
  if (sVar2 == 0) {
    uVar4 = __Z26GetExecutionContextSessionv();
    FUN_00002978(auStack_50,uVar4,&uStack_51);
    sVar2 = FUN_000029b4(auStack_50);
    if (sVar2 == 0) {
      FUN_00002cf8(auStack_58,&DAT_0015d313);
      sVar2 = FUN_00012ef4(auStack_30,auStack_58);
      bVar1 = true;
      if (sVar2 == 0) {
        FUN_00002cf8(auStack_5c,&DAT_0015d314);
        sVar2 = FUN_00012ef4(auStack_30,auStack_5c);
        bVar1 = sVar2 != 0;
      }
      if (bVar1) {
        __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                  (auStack_a8,
                   "This feature is not available in Modular or Smart Mode. It\'s only available in Quick mode. This feature matches the image-line height to the document grid width, by setting the font size accordingly."
                   ,0);
        __ZN6CAlert16InformationAlertERK8PMString(auStack_a8);
        FUN_00002cf8(auStack_ac,0x15d3a3);
        __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_f8,"0x15d300kGCOptionsKey",0);
        FUN_00013588(auStack_30,auStack_ac,auStack_f8);
        __ZN8PMStringD1Ev(auStack_f8);
        local_48 = 2;
        __ZN8PMStringD1Ev(auStack_a8);
      }
      else {
        FUN_00002cf8(auStack_fc,0x15d4a4);
        sVar2 = FUN_00012ef4(auStack_30,auStack_fc);
        if (sVar2 == 0) {
          FUN_00002cf8(auStack_19c,0x15d326);
          iVar3 = FUN_00013034(auStack_30,auStack_19c);
          if (iVar3 < 1) {
            FUN_00002cf8(auStack_284,0x15d306);
            local_288 = 0xffffffff;
            FUN_00012c10(auStack_280,auStack_30,auStack_284,&local_288);
            FUN_00002cf8(auStack_294,0x15d31d);
            local_290 = FUN_00013338(auStack_30,auStack_294);
            plVar5 = (long *)FUN_000029d8(auStack_50);
            local_2a0 = (**(code **)(*plVar5 + 0x98))();
            local_2a4 = 3;
            FUN_000152f0(&local_2a0,&local_2a4);
            FUN_00013490(0);
            sVar2 = FUN_000132fc(&local_290,auStack_2b0);
            if (sVar2 == 0) {
              sVar2 = FUN_00014140(&local_2a0,&local_290);
              if (sVar2 == 0) {
                __ZN8PMStringC1Ev(auStack_428);
                FUN_00002cf8(auStack_430,0x15d3aa);
                local_42c = FUN_00013034(auStack_30,auStack_430);
                if (local_42c == 2) {
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                            (auStack_478,
                             "This feature is not available when image-lines setting is set to \'Custom\'. Please select x or H and try again."
                             ,0);
                  __ZN6CAlert16InformationAlertERK8PMString(auStack_478);
                  FUN_00002cf8(auStack_47c,0x15d3a3);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                            (auStack_4c8,"0x15d300kGCOptionsKey",0);
                  FUN_00013588(auStack_30,auStack_47c,auStack_4c8);
                  __ZN8PMStringD1Ev(auStack_4c8);
                  local_48 = 2;
                  __ZN8PMStringD1Ev(auStack_478);
                }
                else {
                  if (local_42c == 1) {
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_510,"H",0);
                    FUN_0000dd8c(auStack_428);
                    __ZN8PMStringD1Ev(auStack_510);
                  }
                  else if (local_42c == 0) {
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_558,"x",0);
                    FUN_0000dd8c(auStack_428);
                    __ZN8PMStringD1Ev(auStack_558);
                  }
                  FUN_00013490(0x4010000000000000,&local_560);
                  FUN_00013490(&local_568);
                  FUN_00013490(0,auStack_570);
                  while (sVar2 = FUN_0003208c(&local_568,&local_2a0), sVar2 != 0) {
                    local_578 = FUN_001ca224(auStack_428,param_1 + 0x34,param_1 + 0x38,&local_560,
                                             &local_290);
                    local_568 = local_578;
                    FUN_00013490(0x3ff0000000000000);
                    FUN_0004ef2c(&local_560,auStack_580);
                  }
                  sVar2 = FUN_000132fc(&local_568,&local_2a0);
                  if (sVar2 == 0) {
                    sVar2 = FUN_00014140(&local_568,&local_2a0);
                    if (sVar2 != 0) {
                      FUN_00013490(0x4000000000000000);
                      FUN_0004ef9c(&local_560,auStack_5b0);
                      local_5b8 = local_560;
                      FUN_00013490(0x3ff0000000000000);
                      local_5c0 = FUN_00017c4c(&local_560,auStack_5c8);
                      local_5d8 = FUN_00017c4c(&local_5b8,&local_5c0);
                      FUN_00013490(0x4000000000000000);
                      local_5d0 = FUN_000157c8(&local_5d8,auStack_5e0);
                      local_5e4 = 3;
                      FUN_000152f0(&local_5d0,&local_5e4);
                      FUN_00015a50(&local_5f0);
                      while (sVar2 = FUN_00031d38(&local_5b8,&local_5c0), sVar2 != 0) {
                        local_560 = local_5d0;
                        local_5f8 = FUN_001ca224(auStack_428,param_1 + 0x34,param_1 + 0x38,
                                                 &local_560,&local_290);
                        local_5f0 = local_5f8;
                        sVar2 = FUN_0003208c(&local_5f0,&local_2a0);
                        if (sVar2 == 0) {
                          sVar2 = FUN_000132fc(&local_5f0,&local_2a0);
                          if (sVar2 != 0) {
                            FUN_00002cf8(auStack_60c,0x15d3a7);
                            FUN_00013c20(auStack_30,auStack_60c,&local_5d0);
                            FUN_00002cf8(auStack_610,0x15d3a8);
                            local_618 = FUN_00015a7c(&local_5f0,param_1 + 0x88);
                            FUN_00013c20(auStack_30,auStack_610,&local_618);
                            break;
                          }
                          FUN_00013490(0x3f50624dd2f1a9fc);
                          local_620 = FUN_00028080(&local_5d0,auStack_628);
                          local_5c0 = local_620;
                        }
                        else {
                          FUN_00013490(0x3f50624dd2f1a9fc);
                          local_600 = FUN_00017c4c(&local_5d0,auStack_608);
                          local_5b8 = local_600;
                        }
                        local_638 = FUN_00017c4c(&local_5b8,&local_5c0);
                        FUN_00013490(0x4000000000000000);
                        local_630 = FUN_000157c8(&local_638,auStack_640);
                        local_644 = 3;
                        local_5d0 = local_630;
                        FUN_000152f0(&local_5d0,&local_644);
                      }
                      sVar2 = FUN_00014140(&local_5b8,&local_5c0);
                      if (sVar2 != 0) {
                        FUN_00002cf8(auStack_648,0x15d3a7);
                        FUN_00013c20(auStack_30,auStack_648,&local_560);
                        FUN_00002cf8(auStack_64c,0x15d3a8);
                        local_658 = FUN_00015a7c(&local_5f0,param_1 + 0x88);
                        FUN_00013c20(auStack_30,auStack_64c,&local_658);
                        __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                                  (auStack_6a0,
                                   "The image-line height could not be matched to the document grid width, the closest font size has been set. For an exact match, please try to edit the document grid width to image-line height and then try again."
                                   ,0);
                        __ZN6CAlert16InformationAlertERK8PMString(auStack_6a0);
                        FUN_00002cf8(auStack_6a4,0x15d3a3);
                        __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                                  (auStack_6f0,"0x15d300kGCOptionsKey",0);
                        FUN_00013588(auStack_30,auStack_6a4,auStack_6f0);
                        __ZN8PMStringD1Ev(auStack_6f0);
                        __ZN8PMStringD1Ev(auStack_6a0);
                      }
                    }
                  }
                  else {
                    FUN_00002cf8(auStack_584,0x15d3a7);
                    FUN_00013490(0x3ff0000000000000);
                    local_590 = FUN_00028080(&local_560,auStack_598);
                    FUN_00013c20(auStack_30,auStack_584,&local_590);
                    FUN_00002cf8(auStack_59c,0x15d3a8);
                    local_5a8 = FUN_00015a7c(&local_568,param_1 + 0x88);
                    FUN_00013c20(auStack_30,auStack_59c,&local_5a8);
                  }
                  __ZN17AttributeBossListC1Ev(auStack_708);
                  FUN_00002cf8(auStack_714,0x15d3a7);
                  local_710 = FUN_00013338(auStack_30,auStack_714);
                  local_560 = local_710;
                  FUN_000069c0(&local_724,&DAT_00001b03);
                  uVar4 = FUN_00032adc(local_724);
                  FUN_00030c44(auStack_720,uVar4);
                  uVar4 = FUN_00030c90(auStack_720);
                  FUN_00032b34(uVar4,&local_560);
                  uVar4 = FUN_00030c78(auStack_720);
                  local_728 = 0;
                  __ZN17AttributeBossList14ApplyAttributeEPK10IPMUnknown6IDTypeI11ClassID_tagE
                            (auStack_708,uVar4,0);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_770,"[GC] Body Copy #1",0);
                  FUN_00002f44(auStack_774,0xca0c);
                  FUN_001aed08(auStack_30,auStack_770,auStack_708,auStack_774);
                  __ZN8PMStringD1Ev(auStack_770);
                  FUN_00002cf8(auStack_778,0x15d3a9);
                  sVar2 = FUN_00012ef4(auStack_30,auStack_778);
                  if (sVar2 != 0) {
                    FUN_00002cf8(auStack_77c,0x15d3a9);
                    FUN_00013178(auStack_30,auStack_77c,&DAT_00208c50);
                    FUN_0012cc1c(param_1,&DAT_00208c50);
                    FUN_00002cf8(auStack_780,0x15d3a9);
                    FUN_00013178(auStack_30,auStack_780,&DAT_00208c52);
                    FUN_0012cc1c(param_1,&DAT_00208c52);
                  }
                  *(undefined8 *)(param_1 + 0x80) = local_560;
                  FUN_001cba3c(auStack_30);
                  FUN_00030ca8(auStack_720);
                  __ZN17AttributeBossListD1Ev(auStack_708);
                  local_48 = 0;
                }
                __ZN8PMStringD1Ev(auStack_428);
              }
              else {
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                          (auStack_390,
                           "The grid width is greater than the leading, therefore this feature can not be applied. Please edit the grid width to a smaller value."
                           ,0);
                __ZN6CAlert16InformationAlertERK8PMString(auStack_390);
                FUN_00002cf8(auStack_394,0x15d3a3);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_3e0,"0x15d300kGCOptionsKey",0);
                FUN_00013588(auStack_30,auStack_394,auStack_3e0);
                __ZN8PMStringD1Ev(auStack_3e0);
                local_48 = 2;
                __ZN8PMStringD1Ev(auStack_390);
              }
            }
            else {
              __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                        (auStack_2f8,"Please enter leading and try again.",0);
              __ZN6CAlert16InformationAlertERK8PMString(auStack_2f8);
              FUN_00002cf8(auStack_2fc,0x15d3a3);
              __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_348,"0x15d300kGCOptionsKey",0);
              FUN_00013588(auStack_30,auStack_2fc,auStack_348);
              __ZN8PMStringD1Ev(auStack_348);
              local_48 = 2;
              __ZN8PMStringD1Ev(auStack_2f8);
            }
            __ZN8PMStringD1Ev(auStack_280);
          }
          else {
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                      (auStack_1e8,
                       "This feature is not available in combination with the subdivision option in Quick Document Setup. Please select the Applied Leading (top entry) in the subdivision dropdown and try again."
                       ,0);
            __ZN6CAlert16InformationAlertERK8PMString(auStack_1e8);
            __ZN8PMStringD1Ev(auStack_1e8);
            FUN_00002cf8(auStack_1ec,0x15d3a3);
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_238,"0x15d300kGCOptionsKey",0);
            FUN_00013588(auStack_30,auStack_1ec,auStack_238);
            __ZN8PMStringD1Ev(auStack_238);
            local_48 = 2;
          }
        }
        else {
          __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                    (auStack_148,
                     "This feature does not work when horizontal value option is applied.",0);
          __ZN6CAlert16InformationAlertERK8PMString(auStack_148);
          __ZN8PMStringD1Ev(auStack_148);
          FUN_00002cf8(auStack_14c,0x15d3a3);
          __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_198,"0x15d300kGCOptionsKey",0);
          FUN_00013588(auStack_30,auStack_14c,auStack_198);
          __ZN8PMStringD1Ev(auStack_198);
          local_48 = 2;
        }
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
