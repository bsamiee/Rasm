/*
Based On (row 20). FUN_0001d1bc fills the dropdown with the document's paragraph styles after '[No Paragraph Style]'.
FUN_001049ec applies the choice: it reads the style's leading (kTextAttrLead 0x1b1b) and point size (0x1b03); a leading of
-1 (auto) becomes autoLeading (0x1b1a) * size at 0x15a7c; the value is written into the leading field 0x15d31c, the size into
0x15d3a7, and FUN_001101a0 runs the ordinary fit.

PMReal helpers: construct 0x13490, operator/ 0x157c8, operator* 0x15a7c, operator+ 0x17c4c, operator- 0x28080,
Round 0x15abc (floor(x + 0.5)), floor 0x37d70, a > b + 1e-8 0x14140, a >= b - 1e-8 0x15808, a <= b + 1e-8 0x31d38,
a < b - 1e-8 0x3208c, |a - b| < 1e-8 0x132fc, not equal 0x18504, quantise 0x152f0 (mode 3 three decimals, mode 4 four).
Widget access: read value 0x13338, read value in the panel unit 0x1544c, read checkbox 0x12ef4, read dropdown index 0x13034,
write value 0x13c20, set maximum 0x196d0, unit factor at object + 0x88.
*/

//==== FUNC @ 1049ec -> 001049ec

void FUN_001049ec(void)

{
  bool bVar1;
  short sVar2;
  int iVar3;
  uint uVar4;
  int iVar5;
  long lVar6;
  undefined8 uVar7;
  long *plVar8;
  ulong uVar9;
  long lVar10;
  undefined8 *puVar11;
  ulong uVar12;
  undefined1 auVar13 [16];
  undefined4 local_8b0;
  undefined1 auStack_8ac [4];
  undefined4 local_8a8;
  undefined1 auStack_8a4 [4];
  undefined1 auStack_8a0 [8];
  undefined1 auStack_898 [8];
  undefined8 local_890;
  undefined4 local_888;
  undefined1 auStack_884 [4];
  undefined1 auStack_880 [8];
  undefined8 local_878;
  undefined4 local_870;
  undefined1 auStack_86c [4];
  undefined1 auStack_868 [8];
  undefined8 local_860;
  undefined4 local_858;
  undefined1 auStack_854 [4];
  undefined1 auStack_850 [8];
  undefined1 auStack_848 [8];
  undefined8 local_840;
  undefined4 local_834;
  undefined8 local_830;
  undefined4 local_824;
  undefined1 auStack_820 [8];
  undefined8 local_818;
  undefined8 local_810;
  undefined1 auStack_808 [8];
  undefined8 local_800;
  undefined4 local_7f8;
  undefined1 auStack_7f4 [4];
  undefined1 auStack_7f0 [8];
  undefined8 local_7e8;
  undefined4 local_7e0;
  undefined1 auStack_7dc [4];
  undefined1 auStack_7d8 [8];
  undefined8 local_7d0;
  undefined4 local_7c8;
  undefined1 auStack_7c4 [4];
  undefined1 auStack_7c0 [8];
  undefined1 auStack_7b8 [8];
  undefined8 local_7b0;
  undefined4 local_7a4;
  undefined8 local_7a0;
  undefined8 local_798;
  int local_78c;
  undefined4 local_788;
  undefined1 auStack_784 [4];
  ulong local_780;
  undefined1 auStack_778 [72];
  undefined4 local_730;
  undefined4 local_72c;
  undefined1 auStack_728 [8];
  undefined4 local_720;
  undefined1 auStack_71c [4];
  undefined1 auStack_718 [8];
  undefined8 local_710;
  undefined1 auStack_704 [4];
  undefined1 auStack_700 [4];
  undefined1 auStack_6fc [4];
  undefined4 local_6f8;
  undefined1 auStack_6f4 [4];
  undefined1 auStack_6f0 [8];
  undefined8 local_6e8;
  undefined4 local_6e0;
  undefined1 auStack_6dc [4];
  undefined1 auStack_6d8 [8];
  undefined1 auStack_6d0 [8];
  undefined8 local_6c8;
  undefined4 local_6bc;
  undefined1 auStack_6b8 [8];
  undefined8 local_6b0;
  undefined8 local_6a8;
  undefined1 auStack_6a0 [8];
  undefined8 local_698;
  undefined4 local_690;
  undefined1 auStack_68c [4];
  undefined1 auStack_688 [8];
  undefined1 auStack_680 [8];
  undefined8 local_678;
  undefined4 local_66c;
  undefined1 auStack_668 [8];
  undefined8 local_660;
  undefined8 local_658;
  undefined1 auStack_650 [12];
  undefined1 auStack_644 [4];
  undefined1 auStack_640 [4];
  undefined1 auStack_63c [4];
  undefined4 local_638;
  undefined1 auStack_634 [4];
  undefined1 auStack_630 [8];
  undefined8 local_628;
  undefined4 local_620;
  undefined1 auStack_61c [4];
  undefined1 auStack_618 [8];
  undefined1 auStack_610 [8];
  undefined8 local_608;
  undefined4 local_5fc;
  undefined8 local_5f8;
  undefined1 auStack_5f0 [8];
  undefined8 local_5e8;
  undefined4 local_5e0;
  undefined1 auStack_5dc [4];
  undefined1 auStack_5d8 [8];
  undefined1 auStack_5d0 [8];
  undefined8 local_5c8;
  undefined4 local_5bc;
  undefined8 local_5b8;
  undefined1 auStack_5b0 [8];
  undefined1 auStack_5a8 [8];
  undefined8 local_5a0;
  undefined1 auStack_598 [72];
  undefined1 auStack_550 [8];
  undefined8 local_548;
  undefined1 auStack_540 [72];
  undefined4 local_4f8;
  undefined1 auStack_4f4 [4];
  undefined1 auStack_4f0 [72];
  undefined8 local_4a8;
  undefined8 local_4a0;
  undefined4 local_498;
  undefined4 local_494;
  undefined1 auStack_490 [8];
  undefined8 local_488;
  undefined4 local_47c;
  undefined1 auStack_478 [8];
  undefined8 local_470;
  undefined8 local_468;
  undefined1 auStack_45c [4];
  undefined8 local_458;
  undefined1 auStack_450 [8];
  undefined8 local_448;
  undefined1 auStack_43c [4];
  undefined1 auStack_438 [76];
  undefined1 auStack_3ec [4];
  undefined1 auStack_3e8 [8];
  undefined8 local_3e0;
  undefined4 local_3d8;
  undefined4 local_3d4;
  undefined1 auStack_3d0 [12];
  undefined4 local_3c4;
  undefined1 auStack_3c0 [4];
  undefined4 local_3bc;
  undefined1 auStack_3b8 [4];
  undefined4 local_3b4;
  undefined1 auStack_3b0 [4];
  undefined4 local_3ac;
  undefined1 auStack_3a8 [4];
  undefined4 local_3a4;
  undefined1 auStack_3a0 [4];
  int local_39c;
  undefined4 local_398;
  undefined4 local_394;
  undefined1 auStack_390 [12];
  undefined4 local_384;
  undefined1 auStack_380 [4];
  undefined4 local_37c;
  undefined1 auStack_378 [4];
  undefined1 auStack_374 [4];
  undefined4 local_370;
  undefined1 auStack_36c [4];
  undefined1 auStack_368 [4];
  undefined4 local_364;
  undefined4 local_360;
  undefined4 local_35c;
  undefined1 auStack_358 [12];
  undefined4 local_34c;
  undefined1 auStack_348 [4];
  undefined4 local_344;
  undefined1 auStack_340 [4];
  undefined4 local_33c;
  undefined1 auStack_338 [4];
  undefined4 local_334;
  undefined1 auStack_330 [4];
  undefined4 local_32c;
  undefined1 auStack_328 [4];
  undefined4 local_324;
  undefined1 auStack_320 [4];
  undefined4 local_31c;
  undefined1 auStack_318 [4];
  undefined4 local_314;
  undefined1 auStack_310 [4];
  undefined4 local_30c;
  undefined1 auStack_308 [4];
  int local_304;
  undefined4 local_300;
  undefined4 local_2fc;
  undefined1 auStack_2f8 [8];
  undefined1 auStack_2f0 [4];
  undefined1 auStack_2ec [4];
  undefined1 auStack_2e8 [4];
  undefined1 auStack_2e4 [4];
  undefined1 auStack_2e0 [4];
  undefined1 auStack_2dc [4];
  undefined1 auStack_2d8 [4];
  undefined1 auStack_2d4 [4];
  undefined1 auStack_2d0 [72];
  undefined8 local_288;
  undefined1 auStack_280 [11];
  undefined1 uStack_275;
  undefined4 local_274;
  undefined1 auStack_270 [12];
  undefined4 local_264;
  undefined4 local_260;
  undefined4 local_25c;
  undefined1 auStack_258 [8];
  undefined4 local_250;
  undefined4 local_24c;
  undefined1 auStack_248 [15];
  byte local_239;
  undefined1 auStack_238 [75];
  byte local_1ed;
  undefined1 auStack_1ec [4];
  undefined1 auStack_1e8 [76];
  undefined1 auStack_19c [4];
  undefined1 auStack_198 [4];
  undefined1 auStack_194 [4];
  undefined1 auStack_190 [4];
  undefined1 auStack_18c [4];
  undefined8 local_188;
  undefined4 local_180;
  undefined4 local_17c;
  undefined1 auStack_178 [8];
  undefined1 auStack_170 [8];
  undefined8 local_168;
  undefined4 local_160;
  undefined4 local_15c;
  undefined1 auStack_158 [8];
  undefined4 local_150;
  undefined4 local_14c;
  undefined1 auStack_148 [12];
  undefined4 local_13c;
  undefined1 auStack_138 [15];
  undefined1 uStack_129;
  undefined1 auStack_128 [12];
  undefined4 local_11c;
  undefined4 local_118;
  undefined4 local_114;
  int local_110;
  undefined1 auStack_10c [4];
  undefined1 auStack_108 [12];
  undefined4 local_fc;
  undefined1 auStack_f8 [72];
  undefined4 local_b0;
  undefined4 local_ac;
  undefined1 auStack_a8 [15];
  undefined1 uStack_99;
  undefined1 local_98 [16];
  undefined1 auStack_88 [15];
  undefined1 uStack_79;
  undefined1 auStack_78 [8];
  undefined1 local_70 [16];
  undefined1 uStack_59;
  undefined1 auStack_58 [8];
  undefined4 local_50;
  undefined1 uStack_39;
  undefined1 auStack_38 [8];
  undefined8 local_30;
  long local_28;
  
  auVar13 = (*(code *)PTR____chkstk_darwin_002340e0)();
  local_30 = auVar13._8_8_;
  lVar6 = auVar13._0_8_;
  local_28 = lVar6;
  FUN_00002bf4(auStack_38,lVar6,&uStack_39);
  sVar2 = FUN_00002c30(auStack_38);
  if (sVar2 == 0) {
    uVar7 = __Z26GetExecutionContextSessionv();
    FUN_00002978(auStack_58,uVar7,&uStack_59);
    sVar2 = FUN_000029b4(auStack_58);
    if (sVar2 == 0) {
      plVar8 = (long *)FUN_000029d8(auStack_58);
      local_70 = (**(code **)(*plVar8 + 0x18))();
      FUN_0000df20(auStack_78,local_70,&uStack_79);
      sVar2 = FUN_0000df5c(auStack_78);
      if (sVar2 == 0) {
        plVar8 = (long *)FUN_0001b750(auStack_78);
        local_98 = (**(code **)(*plVar8 + 0x88))();
        FUN_0001b768(auStack_88,local_98,&uStack_99);
        sVar2 = FUN_0001b7a4(auStack_88);
        if (sVar2 == 0) {
          uVar7 = FUN_0001b7c8(auStack_88);
          FUN_00002f44(&local_ac,0xca0c);
          FUN_0001de4c(auStack_a8,uVar7,local_ac);
          sVar2 = FUN_0001de90(auStack_a8);
          if (sVar2 == 0) {
            FUN_00030b3c(&local_b0);
            uVar7 = local_30;
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                      (auStack_f8,"0x15d300kGCNoParagraphStyleKey",0);
            sVar2 = __ZNK8PMString7IsEqualERKS_hh(uVar7,auStack_f8,1,0);
            __ZN8PMStringD1Ev(auStack_f8);
            if (sVar2 == 0) {
              plVar8 = (long *)FUN_0001dee4(auStack_a8);
              local_fc = (**(code **)(*plVar8 + 0x38))(plVar8,local_30,0);
              local_b0 = local_fc;
              uVar9 = FUN_00002e24(&local_b0,&DAT_00208c58);
              if ((uVar9 & 1) != 0) {
                uVar7 = FUN_0000310c(local_70);
                __ZN7UIDListC1EP9IDataBase(auStack_108,uVar7);
                plVar8 = (long *)FUN_0001dee4(auStack_a8);
                plVar8 = (long *)(**(code **)(*plVar8 + 0x18))();
                FUN_00002f44(auStack_10c,0xca0b);
                (**(code **)(*plVar8 + 0x80))(plVar8,auStack_108,auStack_10c);
                local_110 = 0;
                while( true ) {
                  iVar5 = local_110;
                  iVar3 = FUN_0001defc(auStack_108);
                  if (iVar3 < iVar5) break;
                  local_114 = FUN_0016a370(auStack_108,local_110);
                  plVar8 = (long *)FUN_0001dee4(auStack_a8);
                  local_11c = local_114;
                  local_118 = (**(code **)(*plVar8 + 0x40))(plVar8,local_114,local_30);
                  local_b0 = local_118;
                  uVar9 = FUN_0000e010(&local_b0,&DAT_00208c58);
                  if ((uVar9 & 1) != 0) break;
                  local_110 = local_110 + 1;
                }
                __ZN7UIDListD1Ev(auStack_108);
              }
              uVar7 = FUN_0000df80(auStack_78);
              FUN_00030adc(auStack_128,uVar7,&uStack_129);
              sVar2 = FUN_00030b18(auStack_128);
              if (sVar2 == 0) {
                plVar8 = (long *)FUN_00030bb0(auStack_128);
                local_13c = local_b0;
                uVar7 = (**(code **)(*plVar8 + 0x18))(plVar8,local_b0);
                FUN_00030bc8(auStack_138,uVar7);
                uVar4 = FUN_00030bfc(auStack_138);
                if ((uVar4 & 1) == 0) {
                  uVar7 = FUN_00030c2c(auStack_138);
                  FUN_000069c0(&local_14c,&DAT_00001b1b);
                  FUN_00002f44(&local_150,&DAT_00001b06);
                  uVar7 = __ZNK17AttributeBossList14QueryByClassIDE6IDTypeI11ClassID_tagES0_I9PMIID_tagE
                                    (uVar7,local_14c,local_150);
                  FUN_00030c44(auStack_148,uVar7);
                  uVar7 = FUN_00030c2c(auStack_138);
                  FUN_000069c0(&local_15c,&DAT_00001b03);
                  FUN_00002f44(&local_160,&DAT_00001b06);
                  uVar7 = __ZNK17AttributeBossList14QueryByClassIDE6IDTypeI11ClassID_tagES0_I9PMIID_tagE
                                    (uVar7,local_15c,local_160);
                  FUN_00030c44(auStack_158,uVar7);
                  lVar10 = FUN_00030c78(auStack_148);
                  if (lVar10 != 0) {
                    lVar10 = FUN_00030c78(auStack_158);
                    if (lVar10 != 0) {
                      plVar8 = (long *)FUN_00030c90(auStack_148);
                      puVar11 = (undefined8 *)(**(code **)(*plVar8 + 0x18))();
                      local_168 = *puVar11;
                      plVar8 = (long *)FUN_00030c90(auStack_158);
                      puVar11 = (undefined8 *)(**(code **)(*plVar8 + 0x18))();
                      *(undefined8 *)(lVar6 + 0x80) = *puVar11;
                      FUN_00013490(0xbff0000000000000);
                      sVar2 = FUN_000132fc(&local_168,auStack_170);
                      if (sVar2 != 0) {
                        uVar7 = FUN_00030c2c(auStack_138);
                        FUN_000069c0(&local_17c,&DAT_00001b1a);
                        FUN_00002f44(&local_180,&DAT_00001b06);
                        uVar7 = __ZNK17AttributeBossList14QueryByClassIDE6IDTypeI11ClassID_tagES0_I9PMIID_tagE
                                          (uVar7,local_17c,local_180);
                        FUN_00030c44(auStack_178,uVar7);
                        lVar10 = FUN_00030c78(auStack_178);
                        if (lVar10 != 0) {
                          plVar8 = (long *)FUN_00030c90(auStack_178);
                          uVar7 = (**(code **)(*plVar8 + 0x18))();
                          local_188 = FUN_00015a7c(uVar7,lVar6 + 0x80);
                          local_168 = local_188;
                        }
                        FUN_00030ca8(auStack_178);
                      }
                      FUN_00002cf8(auStack_18c,0x15d31c);
                      FUN_00013c20(auStack_38,auStack_18c,&local_168);
                      FUN_001101a0(lVar6);
                      FUN_00002cf8(auStack_190,0x15d3a7);
                      FUN_00013c20(auStack_38,auStack_190,lVar6 + 0x80);
                      FUN_00002cf8(auStack_194,0x15d3a7);
                      FUN_0014c76c(lVar6,auStack_194,&DAT_00208c52);
                      FUN_00002cf8(auStack_198,0x15d4a3);
                      local_1ed = 0;
                      local_239 = 0;
                      sVar2 = FUN_00012ef4(auStack_38,auStack_198);
                      bVar1 = false;
                      if (sVar2 == 0) {
                        FUN_00002cf8(auStack_19c,0x15d4a4);
                        sVar2 = FUN_00012ef4(auStack_38,auStack_19c);
                        bVar1 = false;
                        if (sVar2 == 0) {
                          FUN_00002cf8(auStack_1ec,0x15d30a);
                          FUN_000138a8(auStack_1e8,auStack_38,auStack_1ec);
                          local_1ed = 1;
                          __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                                    (auStack_238,"0x15d300kGCHideGridKey",0);
                          local_239 = 1;
                          sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_1e8,auStack_238,1,0);
                          bVar1 = sVar2 != 0;
                        }
                      }
                      if ((local_239 & 1) != 0) {
                        __ZN8PMStringD1Ev(auStack_238);
                      }
                      if ((local_1ed & 1) != 0) {
                        __ZN8PMStringD1Ev(auStack_1e8);
                      }
                      if (bVar1) {
                        FUN_001ae290(auStack_38,&local_168,&DAT_00208c50);
                        plVar8 = (long *)FUN_000029d8(auStack_58);
                        (**(code **)(*plVar8 + 0x110))(plVar8,&DAT_00208c50);
                        plVar8 = (long *)FUN_000029d8(auStack_58);
                        (**(code **)(*plVar8 + 0x120))(plVar8,&DAT_00208c50);
                      }
                    }
                  }
                  uVar7 = FUN_00030c2c(auStack_138);
                  FUN_000069c0(&local_24c,&DAT_00001b2b);
                  FUN_00002f44(&local_250,&DAT_00001b01);
                  uVar7 = __ZNK17AttributeBossList14QueryByClassIDE6IDTypeI11ClassID_tagES0_I9PMIID_tagE
                                    (uVar7,local_24c,local_250);
                  FUN_00031da8(auStack_248,uVar7);
                  uVar7 = FUN_00030c2c(auStack_138);
                  FUN_000069c0(&local_25c,&DAT_00001b02);
                  FUN_00002f44(&local_260,&DAT_00001b0d);
                  uVar7 = __ZNK17AttributeBossList14QueryByClassIDE6IDTypeI11ClassID_tagES0_I9PMIID_tagE
                                    (uVar7,local_25c,local_260);
                  FUN_00031ddc(auStack_258,uVar7);
                  lVar10 = FUN_00031e10(auStack_248);
                  if (lVar10 != 0) {
                    lVar10 = FUN_00031e28(auStack_258);
                    if (lVar10 != 0) {
                      uVar7 = FUN_00031e58(auStack_248);
                      local_264 = FUN_00031e70(uVar7);
                      *(undefined4 *)(lVar6 + 0x34) = local_264;
                      plVar8 = (long *)FUN_00031e40(auStack_258);
                      uVar7 = (**(code **)(*plVar8 + 0x18))();
                      FUN_00013b00(lVar6 + 0x38,uVar7);
                      uVar7 = FUN_0000310c(local_70);
                      local_274 = *(undefined4 *)(lVar6 + 0x34);
                      FUN_00031ea4(auStack_270,uVar7,local_274,&uStack_275);
                      lVar10 = FUN_00031ef0(auStack_270);
                      if (lVar10 != 0) {
                        plVar8 = (long *)FUN_00031f08(auStack_270);
                        FUN_00013490(0x3fe6666666666666,&local_288);
                        uVar7 = (**(code **)(*plVar8 + 0x78))(local_288,plVar8,lVar6 + 0x38,0);
                        FUN_00014948(auStack_280,uVar7);
                        lVar10 = FUN_0001497c(auStack_280);
                        if (lVar10 != 0) {
                          __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_2d0,"",0);
                          plVar8 = (long *)FUN_00014994(auStack_280);
                          (**(code **)(*plVar8 + 0x40))(plVar8,auStack_2d0);
                          FUN_00002cf8(auStack_2d4,0x15d3a5);
                          FUN_000191b0(auStack_38,auStack_2d4,auStack_2d0,1);
                          FUN_00002cf8(auStack_2d8,0x15d3a5);
                          FUN_00013588(auStack_38,auStack_2d8,auStack_2d0);
                          FUN_00002cf8(auStack_2dc,0x15d3a5);
                          FUN_00002cf8(auStack_2e0,0x15d3a6);
                          FUN_001431b0(lVar6,auStack_2dc,auStack_2e0,&DAT_00208c50);
                          FUN_00002cf8(auStack_2e4,0x15d3a6);
                          FUN_000191b0(auStack_38,auStack_2e4,lVar6 + 0x38,1);
                          FUN_00002cf8(auStack_2e8,0x15d3a6);
                          FUN_00013588(auStack_38,auStack_2e8,lVar6 + 0x38);
                          FUN_00002cf8(auStack_2ec,0x15d3a5);
                          FUN_00002cf8(auStack_2f0,0x15d3a6);
                          FUN_001431b0(lVar6,auStack_2ec,auStack_2f0,&DAT_00208c52);
                          __ZN8PMStringD1Ev(auStack_2d0);
                        }
                        FUN_000149ac(auStack_280);
                      }
                      FUN_00031f20(auStack_270);
                    }
                  }
                  uVar7 = FUN_00030c2c(auStack_138);
                  FUN_000069c0(&local_2fc,&DAT_00001b7e);
                  FUN_00002f44(&local_300,&DAT_00001b09);
                  uVar7 = __ZNK17AttributeBossList14QueryByClassIDE6IDTypeI11ClassID_tagES0_I9PMIID_tagE
                                    (uVar7,local_2fc,local_300);
                  FUN_00032260(auStack_2f8,uVar7);
                  plVar8 = (long *)FUN_000322ac(auStack_2f8);
                  local_304 = (**(code **)(*plVar8 + 0x20))();
                  if (local_304 == 0) {
                    FUN_00002cf8(auStack_308,0x15d39e);
                    local_30c = 0;
                    FUN_00015b04(auStack_38,auStack_308,&local_30c,1);
                  }
                  else if (local_304 == 1) {
                    FUN_00002cf8(auStack_310,0x15d39e);
                    local_314 = 1;
                    FUN_00015b04(auStack_38,auStack_310,&local_314,1);
                  }
                  else if (local_304 == 2) {
                    FUN_00002cf8(auStack_318,0x15d39e);
                    local_31c = 2;
                    FUN_00015b04(auStack_38,auStack_318,&local_31c,1);
                  }
                  else if (local_304 == 4) {
                    FUN_00002cf8(auStack_320,0x15d39e);
                    local_324 = 3;
                    FUN_00015b04(auStack_38,auStack_320,&local_324,1);
                  }
                  else if (local_304 == 5) {
                    FUN_00002cf8(auStack_328,0x15d39e);
                    local_32c = 4;
                    FUN_00015b04(auStack_38,auStack_328,&local_32c,1);
                  }
                  else if (local_304 == 6) {
                    FUN_00002cf8(auStack_330,0x15d39e);
                    local_334 = 5;
                    FUN_00015b04(auStack_38,auStack_330,&local_334,1);
                  }
                  else if (local_304 == 3) {
                    FUN_00002cf8(auStack_338,0x15d39e);
                    local_33c = 6;
                    FUN_00015b04(auStack_38,auStack_338,&local_33c,1);
                  }
                  else if (local_304 == 8) {
                    FUN_00002cf8(auStack_340,0x15d39e);
                    local_344 = 7;
                    FUN_00015b04(auStack_38,auStack_340,&local_344,1);
                  }
                  else if (local_304 == 9) {
                    FUN_00002cf8(auStack_348,0x15d39e);
                    local_34c = 8;
                    FUN_00015b04(auStack_38,auStack_348,&local_34c,1);
                  }
                  uVar7 = FUN_00030c2c(auStack_138);
                  FUN_000069c0(&local_35c,&DAT_00001b07);
                  FUN_00002f44(&local_360,&DAT_00001b03);
                  uVar7 = __ZNK17AttributeBossList14QueryByClassIDE6IDTypeI11ClassID_tagES0_I9PMIID_tagE
                                    (uVar7,local_35c,local_360);
                  FUN_00031f4c(auStack_358,uVar7);
                  plVar8 = (long *)FUN_00031f98(auStack_358);
                  local_364 = (**(code **)(*plVar8 + 0x20))();
                  FUN_000069c0(auStack_368,0x13803);
                  uVar9 = FUN_00031fb0(&local_364,auStack_368);
                  if ((uVar9 & 1) == 0) {
                    FUN_000069c0(auStack_374,0x3e64);
                    uVar9 = FUN_00031fb0(&local_364,auStack_374);
                    if ((uVar9 & 1) == 0) {
                      FUN_00002cf8(auStack_380,0x15d3a4);
                      local_384 = 4;
                      FUN_00015b04(auStack_38,auStack_380,&local_384,1);
                    }
                    else {
                      FUN_00002cf8(auStack_378,0x15d3a4);
                      local_37c = 3;
                      FUN_00015b04(auStack_38,auStack_378,&local_37c,1);
                    }
                  }
                  else {
                    FUN_00002cf8(auStack_36c,0x15d3a4);
                    local_370 = 2;
                    FUN_00015b04(auStack_38,auStack_36c,&local_370,1);
                  }
                  uVar7 = FUN_00030c2c(auStack_138);
                  FUN_000069c0(&local_394,&DAT_00001b2c);
                  FUN_00002f44(&local_398,&DAT_00001b16);
                  uVar7 = __ZNK17AttributeBossList14QueryByClassIDE6IDTypeI11ClassID_tagES0_I9PMIID_tagE
                                    (uVar7,local_394,local_398);
                  FUN_00031fe0(auStack_390,uVar7);
                  plVar8 = (long *)FUN_0003202c(auStack_390);
                  local_39c = (**(code **)(*plVar8 + 0x18))();
                  if (local_39c == 0) {
                    FUN_00002cf8(auStack_3a0,0x15d3ae);
                    local_3a4 = 0;
                    FUN_00015b04(auStack_38,auStack_3a0,&local_3a4,1);
                  }
                  else if (local_39c == 1) {
                    FUN_00002cf8(auStack_3a8,0x15d3ae);
                    local_3ac = 1;
                    FUN_00015b04(auStack_38,auStack_3a8,&local_3ac,1);
                  }
                  else if (local_39c == 2) {
                    FUN_00002cf8(auStack_3b0,0x15d3ae);
                    local_3b4 = 2;
                    FUN_00015b04(auStack_38,auStack_3b0,&local_3b4,1);
                  }
                  else if (local_39c == 3) {
                    FUN_00002cf8(auStack_3b8,0x15d3ae);
                    local_3bc = 3;
                    FUN_00015b04(auStack_38,auStack_3b8,&local_3bc,1);
                  }
                  else if (local_39c == 4) {
                    FUN_00002cf8(auStack_3c0,0x15d3ae);
                    local_3c4 = 4;
                    FUN_00015b04(auStack_38,auStack_3c0,&local_3c4,1);
                  }
                  uVar7 = FUN_00030c2c(auStack_138);
                  FUN_000069c0(&local_3d4,&DAT_00001b0b);
                  FUN_00002f44(&local_3d8,&DAT_00001b06);
                  uVar7 = __ZNK17AttributeBossList14QueryByClassIDE6IDTypeI11ClassID_tagES0_I9PMIID_tagE
                                    (uVar7,local_3d4,local_3d8);
                  FUN_00030c44(auStack_3d0,uVar7);
                  uVar7 = FUN_00030c90(auStack_3d0);
                  puVar11 = (undefined8 *)FUN_00032044(uVar7);
                  local_3e0 = *puVar11;
                  FUN_00013490(0);
                  sVar2 = FUN_000132fc(&local_3e0,auStack_3e8);
                  if (sVar2 == 0) {
                    FUN_00002cf8(auStack_43c,0x15d3a0);
                    FUN_00013490(0x408f400000000000);
                    local_448 = FUN_00015a7c(&local_3e0,auStack_450);
                    FUN_00013c20(auStack_38,auStack_43c,&local_448);
                  }
                  else {
                    FUN_00002cf8(auStack_3ec,0x15d3a0);
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_438,"",0);
                    FUN_00013588(auStack_38,auStack_3ec,auStack_438);
                    __ZN8PMStringD1Ev(auStack_438);
                  }
                  FUN_00002cf8(auStack_45c,0x15d3a7);
                  local_458 = FUN_00013338(auStack_38,auStack_45c);
                  plVar8 = (long *)FUN_000029d8(auStack_58);
                  local_470 = (**(code **)(*plVar8 + 0x98))();
                  plVar8 = (long *)FUN_000029d8(auStack_58);
                  iVar5 = (**(code **)(*plVar8 + 0xa8))();
                  FUN_00013490((double)iVar5);
                  local_468 = FUN_000157c8(&local_470,auStack_478);
                  local_47c = 3;
                  FUN_000152f0(&local_468,&local_47c);
                  FUN_00015a50(&local_488);
                  uVar7 = FUN_00030c2c(auStack_138);
                  FUN_000069c0(&local_494,&DAT_00001b18);
                  FUN_00002f44(&local_498,&DAT_00001b06);
                  uVar7 = __ZNK17AttributeBossList14QueryByClassIDE6IDTypeI11ClassID_tagES0_I9PMIID_tagE
                                    (uVar7,local_494,local_498);
                  FUN_00030c44(auStack_490,uVar7);
                  uVar7 = FUN_00030c90(auStack_490);
                  puVar11 = (undefined8 *)FUN_00032044(uVar7);
                  local_4a8 = *puVar11;
                  local_4a0 = local_4a8;
                  FUN_00002cf8(auStack_4f4,0x15d3d2);
                  local_4f8 = 0xffffffff;
                  FUN_00012c10(auStack_4f0,auStack_38,auStack_4f4,&local_4f8);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                            (auStack_540,"0x15d300kGCMillimetersKey",0);
                  sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_4f0,auStack_540,1,0);
                  __ZN8PMStringD1Ev(auStack_540);
                  if (sVar2 == 0) {
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                              (auStack_598,"0x15d300kGCInchesKey",0);
                    sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_4f0,auStack_598,1,0);
                    __ZN8PMStringD1Ev(auStack_598);
                    if (sVar2 != 0) {
                      FUN_00013490(0x4052000000000000);
                      local_5a0 = FUN_000157c8(&local_4a8,auStack_5a8);
                      local_4a8 = local_5a0;
                    }
                  }
                  else {
                    FUN_00013490(0x4006ad5b202b0759);
                    local_548 = FUN_000157c8(&local_4a8,auStack_550);
                    local_4a8 = local_548;
                  }
                  FUN_00013490(0);
                  sVar2 = FUN_00014140(&local_4a0,auStack_5b0);
                  if (sVar2 == 0) {
                    FUN_00013490(0);
                    sVar2 = FUN_0003208c(&local_4a0,auStack_650);
                    if (sVar2 == 0) {
                      FUN_00002cf8(auStack_71c,0x15d3a2);
                      local_720 = 0;
                      FUN_00015b04(auStack_38,auStack_71c,&local_720,1);
                    }
                    else {
                      FUN_00013490(0xbff0000000000000);
                      local_660 = FUN_00015a7c(auStack_668,&local_4a0);
                      local_658 = FUN_000157c8(&local_660,&local_458);
                      local_66c = 3;
                      local_488 = local_658;
                      FUN_000152f0(&local_488,&local_66c);
                      local_678 = FUN_00015abc(&local_488);
                      sVar2 = FUN_000132fc(&local_488,&local_678);
                      bVar1 = false;
                      if (sVar2 != 0) {
                        FUN_00013490(0x3ff0000000000000);
                        sVar2 = FUN_00015808(&local_488,auStack_680);
                        bVar1 = false;
                        if (sVar2 != 0) {
                          FUN_00013490(0x4028000000000000);
                          sVar2 = FUN_00031d38(&local_488,auStack_688);
                          bVar1 = sVar2 != 0;
                        }
                      }
                      if (bVar1) {
                        FUN_00002cf8(auStack_68c,0x15d3a2);
                        FUN_00013490(0x402c000000000000);
                        local_698 = FUN_00017c4c(&local_488,auStack_6a0);
                        local_690 = FUN_00032070(&local_698);
                        FUN_00015b04(auStack_38,auStack_68c,&local_690,1);
                      }
                      else {
                        FUN_00013490(0xbff0000000000000);
                        local_6b0 = FUN_00015a7c(auStack_6b8,&local_4a0);
                        local_6a8 = FUN_000157c8(&local_6b0,&local_468);
                        local_6bc = 3;
                        local_488 = local_6a8;
                        FUN_000152f0(&local_488,&local_6bc);
                        local_6c8 = FUN_00015abc(&local_488);
                        sVar2 = FUN_000132fc(&local_488,&local_6c8);
                        bVar1 = false;
                        if (sVar2 != 0) {
                          FUN_00013490(0x3ff0000000000000);
                          sVar2 = FUN_00015808(&local_488,auStack_6d0);
                          bVar1 = false;
                          if (sVar2 != 0) {
                            FUN_00013490(0x4014000000000000);
                            sVar2 = FUN_00031d38(&local_488,auStack_6d8);
                            bVar1 = sVar2 != 0;
                          }
                        }
                        if (bVar1) {
                          FUN_00002cf8(auStack_6dc,0x15d3a2);
                          FUN_00013490(0x403c000000000000);
                          local_6e8 = FUN_00017c4c(&local_488,auStack_6f0);
                          local_6e0 = FUN_00032070(&local_6e8);
                          FUN_00015b04(auStack_38,auStack_6dc,&local_6e0,1);
                        }
                        else {
                          FUN_00002cf8(auStack_6f4,0x15d3a2);
                          local_6f8 = 0x1b;
                          FUN_00015b04(auStack_38,auStack_6f4,&local_6f8,1);
                          FUN_00002cf8(auStack_6fc,0x15d3d3);
                          FUN_00011dc4(auStack_38,auStack_6fc,&DAT_00208c52);
                          FUN_00002cf8(auStack_700,0x15d3d2);
                          FUN_00011dc4(auStack_38,auStack_700,&DAT_00208c52);
                        }
                      }
                      FUN_00002cf8(auStack_704,0x15d3d3);
                      FUN_00013490(0xbff0000000000000);
                      local_710 = FUN_00015a7c(&local_4a8,auStack_718);
                      FUN_00013c20(auStack_38,auStack_704,&local_710);
                    }
                  }
                  else {
                    local_5b8 = FUN_000157c8(&local_4a0,&local_458);
                    local_5bc = 3;
                    local_488 = local_5b8;
                    FUN_000152f0(&local_488,&local_5bc);
                    local_5c8 = FUN_00015abc(&local_488);
                    sVar2 = FUN_000132fc(&local_488,&local_5c8);
                    bVar1 = false;
                    if (sVar2 != 0) {
                      FUN_00013490(0x3ff0000000000000);
                      sVar2 = FUN_00015808(&local_488,auStack_5d0);
                      bVar1 = false;
                      if (sVar2 != 0) {
                        FUN_00013490(0x4008000000000000);
                        sVar2 = FUN_00031d38(&local_488,auStack_5d8);
                        bVar1 = sVar2 != 0;
                      }
                    }
                    if (bVar1) {
                      FUN_00002cf8(auStack_5dc,0x15d3a2);
                      FUN_00013490(0x4008000000000000);
                      local_5e8 = FUN_00017c4c(&local_488,auStack_5f0);
                      local_5e0 = FUN_00032070(&local_5e8);
                      FUN_00015b04(auStack_38,auStack_5dc,&local_5e0,1);
                    }
                    else {
                      local_5f8 = FUN_000157c8(&local_4a0,&local_468);
                      local_5fc = 3;
                      local_488 = local_5f8;
                      FUN_000152f0(&local_488,&local_5fc);
                      local_608 = FUN_00015abc(&local_488);
                      sVar2 = FUN_000132fc(&local_488,&local_608);
                      bVar1 = false;
                      if (sVar2 != 0) {
                        FUN_00013490(0x3ff0000000000000);
                        sVar2 = FUN_00015808(&local_488,auStack_610);
                        bVar1 = false;
                        if (sVar2 != 0) {
                          FUN_00013490(0x4014000000000000);
                          sVar2 = FUN_00031d38(&local_488,auStack_618);
                          bVar1 = sVar2 != 0;
                        }
                      }
                      if (bVar1) {
                        FUN_00002cf8(auStack_61c,0x15d3a2);
                        FUN_00013490(0x4020000000000000);
                        local_628 = FUN_00017c4c(&local_488,auStack_630);
                        local_620 = FUN_00032070(&local_628);
                        FUN_00015b04(auStack_38,auStack_61c,&local_620,1);
                      }
                      else {
                        FUN_00002cf8(auStack_634,0x15d3a2);
                        local_638 = 7;
                        FUN_00015b04(auStack_38,auStack_634,&local_638,1);
                        FUN_00002cf8(auStack_63c,0x15d3d3);
                        FUN_00011dc4(auStack_38,auStack_63c,&DAT_00208c52);
                        FUN_00002cf8(auStack_640,0x15d3d2);
                        FUN_00011dc4(auStack_38,auStack_640,&DAT_00208c52);
                      }
                    }
                    FUN_00002cf8(auStack_644,0x15d3d3);
                    FUN_00013c20(auStack_38,auStack_644,&local_4a8);
                  }
                  uVar7 = FUN_00030c2c(auStack_138);
                  FUN_000069c0(&local_72c,&DAT_00001b29);
                  FUN_00002f44(&local_730,&DAT_00001b05);
                  uVar7 = __ZNK17AttributeBossList14QueryByClassIDE6IDTypeI11ClassID_tagES0_I9PMIID_tagE
                                    (uVar7,local_72c,local_730);
                  FUN_000320c8(auStack_728,uVar7);
                  plVar8 = (long *)FUN_00032114(auStack_728);
                  uVar7 = (**(code **)(*plVar8 + 0x20))();
                  FUN_0003212c(auStack_778,uVar7);
                  uVar9 = FUN_00032160(auStack_778);
                  local_780 = uVar9;
                  uVar12 = FUN_00032184(auStack_778);
                  if (uVar9 == uVar12) {
                    FUN_00002cf8(auStack_784,0x15d3a1);
                    local_788 = 0;
                    FUN_00015b04(auStack_38,auStack_784,&local_788,1);
                  }
                  while( true ) {
                    uVar9 = local_780;
                    uVar12 = FUN_00032184(auStack_778);
                    if (uVar12 <= uVar9) break;
                    local_78c = FUN_000321d0(local_780);
                    local_798 = FUN_000321e8(local_780);
                    local_7a0 = FUN_000157c8(&local_798,&local_458);
                    local_7a4 = 3;
                    local_488 = local_7a0;
                    FUN_000152f0(&local_488,&local_7a4);
                    local_7b0 = FUN_00015abc(&local_488);
                    sVar2 = FUN_000132fc(&local_488,&local_7b0);
                    bVar1 = false;
                    if (sVar2 != 0) {
                      FUN_00013490(0x3ff0000000000000);
                      sVar2 = FUN_00015808(&local_488,auStack_7b8);
                      bVar1 = false;
                      if (sVar2 != 0) {
                        FUN_00013490(0x4014000000000000);
                        sVar2 = FUN_00031d38(&local_488,auStack_7c0);
                        bVar1 = sVar2 != 0;
                      }
                    }
                    if (bVar1) {
                      if (local_78c == 0) {
                        FUN_00002cf8(auStack_7c4,0x15d3a1);
                        FUN_00013490(0x4008000000000000);
                        local_7d0 = FUN_00017c4c(&local_488,auStack_7d8);
                        local_7c8 = FUN_00032070(&local_7d0);
                        FUN_00015b04(auStack_38,auStack_7c4,&local_7c8,1);
                      }
                      else if (local_78c == 1) {
                        FUN_00002cf8(auStack_7dc,0x15d3a1);
                        FUN_00013490(0x402e000000000000);
                        local_7e8 = FUN_00017c4c(&local_488,auStack_7f0);
                        local_7e0 = FUN_00032070(&local_7e8);
                        FUN_00015b04(auStack_38,auStack_7dc,&local_7e0,1);
                      }
                      else if (local_78c == 2) {
                        FUN_00002cf8(auStack_7f4,0x15d3a1);
                        FUN_00013490(0x403b000000000000);
                        local_800 = FUN_00017c4c(&local_488,auStack_808);
                        local_7f8 = FUN_00032070(&local_800);
                        FUN_00015b04(auStack_38,auStack_7f4,&local_7f8,1);
                      }
                    }
                    else {
                      plVar8 = (long *)FUN_000029d8(auStack_58);
                      local_818 = (**(code **)(*plVar8 + 0x98))();
                      plVar8 = (long *)FUN_000029d8(auStack_58);
                      iVar5 = (**(code **)(*plVar8 + 0xa8))();
                      FUN_00013490((double)iVar5);
                      local_810 = FUN_000157c8(&local_818,auStack_820);
                      local_824 = 3;
                      FUN_000152f0(&local_810,&local_824);
                      local_830 = FUN_000157c8(&local_798,&local_810);
                      local_834 = 3;
                      local_488 = local_830;
                      FUN_000152f0(&local_488,&local_834);
                      local_840 = FUN_00015abc(&local_488);
                      sVar2 = FUN_000132fc(&local_488,&local_840);
                      bVar1 = false;
                      if (sVar2 != 0) {
                        FUN_00013490(0x3ff0000000000000);
                        sVar2 = FUN_00015808(&local_488,auStack_848);
                        bVar1 = false;
                        if (sVar2 != 0) {
                          FUN_00013490(0x4014000000000000);
                          sVar2 = FUN_00031d38(&local_488,auStack_850);
                          bVar1 = sVar2 != 0;
                        }
                      }
                      if (bVar1) {
                        if (local_78c == 0) {
                          FUN_00002cf8(auStack_854,0x15d3a1);
                          FUN_00013490(0x4022000000000000);
                          local_860 = FUN_00017c4c(&local_488,auStack_868);
                          local_858 = FUN_00032070(&local_860);
                          FUN_00015b04(auStack_38,auStack_854,&local_858,1);
                        }
                        else if (local_78c == 1) {
                          FUN_00002cf8(auStack_86c,0x15d3a1);
                          FUN_00013490(0x4035000000000000);
                          local_878 = FUN_00017c4c(&local_488,auStack_880);
                          local_870 = FUN_00032070(&local_878);
                          FUN_00015b04(auStack_38,auStack_86c,&local_870,1);
                        }
                        else if (local_78c == 2) {
                          FUN_00002cf8(auStack_884,0x15d3a1);
                          FUN_00013490(0x4040800000000000);
                          local_890 = FUN_00017c4c(&local_488,auStack_898);
                          local_888 = FUN_00032070(&local_890);
                          FUN_00015b04(auStack_38,auStack_884,&local_888,1);
                        }
                      }
                      else {
                        FUN_00013490(0);
                        sVar2 = FUN_00014140(&local_798,auStack_8a0);
                        if (sVar2 == 0) {
                          FUN_00002cf8(auStack_8ac,0x15d3a1);
                          local_8b0 = 0;
                          FUN_00015b04(auStack_38,auStack_8ac,&local_8b0,1);
                        }
                        else {
                          FUN_00002cf8(auStack_8a4,0x15d3a1);
                          local_8a8 = 0x28;
                          FUN_00015b04(auStack_38,auStack_8a4,&local_8a8,1);
                        }
                      }
                    }
                    local_780 = local_780 + 0x28;
                  }
                  FUN_00032208(auStack_778);
                  FUN_00032234(auStack_728);
                  __ZN8PMStringD1Ev(auStack_4f0);
                  FUN_00030ca8(auStack_490);
                  FUN_00030ca8(auStack_3d0);
                  FUN_000324dc(auStack_390);
                  FUN_00032508(auStack_358);
                  FUN_000322c4(auStack_2f8);
                  FUN_00032534(auStack_258);
                  FUN_00032560(auStack_248);
                  FUN_00030ca8(auStack_158);
                  FUN_00030ca8(auStack_148);
                  local_50 = 0;
                }
                else {
                  local_50 = 2;
                }
                FUN_0003258c(auStack_138);
              }
              else {
                local_50 = 2;
              }
              FUN_000325b8(auStack_128);
            }
            else {
              FUN_00157dec(lVar6,auStack_38);
              local_50 = 2;
            }
          }
          else {
            local_50 = 2;
          }
          FUN_0001dfd8(auStack_a8);
        }
        else {
          local_50 = 2;
        }
        FUN_0001d190(auStack_88);
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

//==== FUNC @ 1d1bc -> 0001d1bc

void FUN_0001d1bc(undefined8 param_1)

{
  int iVar1;
  short sVar2;
  int iVar3;
  undefined8 uVar4;
  long *plVar5;
  long lVar6;
  undefined1 auVar7 [16];
  ushort local_470;
  undefined4 local_3f0;
  undefined1 auStack_3ec [4];
  undefined1 auStack_3e8 [72];
  undefined1 auStack_3a0 [79];
  undefined1 uStack_351;
  undefined1 local_350 [16];
  undefined1 auStack_340 [8];
  int local_338;
  undefined1 auStack_334 [4];
  undefined1 auStack_330 [8];
  undefined1 auStack_328 [79];
  undefined1 uStack_2d9;
  undefined1 auStack_2d8 [15];
  undefined1 uStack_2c9;
  undefined1 auStack_2c8 [12];
  undefined1 auStack_2bc [4];
  undefined8 local_2b8;
  undefined1 auStack_2b0 [4];
  undefined1 auStack_2ac [4];
  undefined1 auStack_2a8 [4];
  undefined1 auStack_2a4 [4];
  undefined1 auStack_2a0 [4];
  undefined1 auStack_29c [4];
  undefined1 auStack_298 [76];
  undefined1 auStack_24c [4];
  undefined1 auStack_248 [76];
  undefined1 auStack_1fc [4];
  undefined1 auStack_1f8 [76];
  undefined1 auStack_1ac [4];
  undefined1 auStack_1a8 [76];
  undefined1 auStack_15c [4];
  undefined1 auStack_158 [76];
  undefined1 auStack_10c [4];
  undefined1 auStack_108 [72];
  undefined1 auStack_c0 [4];
  undefined1 auStack_bc [4];
  undefined1 auStack_b8 [4];
  undefined1 auStack_b4 [4];
  undefined1 auStack_b0 [4];
  undefined1 auStack_ac [6];
  ushort local_a6;
  undefined1 auStack_a4 [4];
  undefined1 auStack_a0 [12];
  undefined4 local_94;
  undefined1 auStack_90 [15];
  undefined1 uStack_81;
  undefined1 local_80 [16];
  undefined1 auStack_70 [15];
  undefined1 uStack_61;
  undefined1 auStack_60 [8];
  undefined1 local_58 [16];
  undefined4 local_48;
  undefined1 uStack_31;
  undefined1 auStack_30 [8];
  undefined8 local_28;
  
  local_28 = param_1;
  uVar4 = __Z26GetExecutionContextSessionv();
  FUN_00002978(auStack_30,uVar4,&uStack_31);
  sVar2 = FUN_000029b4(auStack_30);
  if (sVar2 == 0) {
    plVar5 = (long *)FUN_000029d8(auStack_30);
    local_58 = (**(code **)(*plVar5 + 0x18))();
    FUN_0000df20(auStack_60,local_58,&uStack_61);
    sVar2 = FUN_0000df5c(auStack_60);
    if (sVar2 == 0) {
      plVar5 = (long *)FUN_0001b750(auStack_60);
      local_80 = (**(code **)(*plVar5 + 0x88))();
      FUN_0001b768(auStack_70,local_80,&uStack_81);
      sVar2 = FUN_0001b7a4(auStack_70);
      if (sVar2 == 0) {
        uVar4 = FUN_0001b7c8(auStack_70);
        FUN_00002f44(&local_94,0xca0c);
        FUN_0001de4c(auStack_90,uVar4,local_94);
        sVar2 = FUN_0001de90(auStack_90);
        if (sVar2 == 0) {
          FUN_001a2484(auStack_a0);
          sVar2 = FUN_0000e2a4(auStack_a0);
          uVar4 = local_28;
          if (sVar2 == 0) {
            FUN_00002cf8(auStack_a4,&DAT_0015d318);
            FUN_00011dc4(uVar4,auStack_a4,&DAT_002084ba);
            plVar5 = (long *)FUN_000029d8(auStack_30);
            sVar2 = (**(code **)(*plVar5 + 0x58))();
            local_470 = 0;
            if (sVar2 != 0) {
              plVar5 = (long *)FUN_000029d8(auStack_30);
              sVar2 = (**(code **)(*plVar5 + 0x68))();
              local_470 = (ushort)(sVar2 != 0);
            }
            uVar4 = local_28;
            local_a6 = local_470 ^ 1;
            FUN_00002cf8(auStack_ac,&DAT_0015d31a);
            FUN_00011dc4(uVar4,auStack_ac,&local_a6);
            uVar4 = local_28;
            FUN_00002cf8(auStack_b0,&DAT_0015d31b);
            FUN_00011dc4(uVar4,auStack_b0,&local_a6);
            uVar4 = local_28;
            FUN_00002cf8(auStack_b4,0x15d31e);
            FUN_00011dc4(uVar4,auStack_b4,&local_a6);
            uVar4 = local_28;
            FUN_00002cf8(auStack_b8,0x15d31c);
            FUN_00011dc4(uVar4,auStack_b8,&DAT_002084ba);
            uVar4 = local_28;
            FUN_00002cf8(auStack_bc,0x15d32e);
            FUN_00011dc4(uVar4,auStack_bc,&DAT_002084ba);
            uVar4 = local_28;
            FUN_00002cf8(auStack_c0,0x15d31c);
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_108,"",0);
            FUN_00013588(uVar4,auStack_c0,auStack_108);
            __ZN8PMStringD1Ev(auStack_108);
            uVar4 = local_28;
            FUN_00002cf8(auStack_10c,0x15d31d);
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_158,"",0);
            FUN_00013588(uVar4,auStack_10c,auStack_158);
            __ZN8PMStringD1Ev(auStack_158);
            uVar4 = local_28;
            FUN_00002cf8(auStack_15c,0x15d32e);
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1a8,"",0);
            FUN_00013588(uVar4,auStack_15c,auStack_1a8);
            __ZN8PMStringD1Ev(auStack_1a8);
            uVar4 = local_28;
            FUN_00002cf8(auStack_1ac,0x15d330);
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1f8,"",0);
            FUN_00013588(uVar4,auStack_1ac,auStack_1f8);
            __ZN8PMStringD1Ev(auStack_1f8);
            uVar4 = local_28;
            FUN_00002cf8(auStack_1fc,0x15d324);
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_248,"0x15d300kGCSquareKey",0);
            FUN_00013588(uVar4,auStack_1fc,auStack_248);
            __ZN8PMStringD1Ev(auStack_248);
            uVar4 = local_28;
            FUN_00002cf8(auStack_24c,0x15d32f);
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_298,"0x15d300kGCCalculateKey",0);
            FUN_00013588(uVar4,auStack_24c,auStack_298);
            __ZN8PMStringD1Ev(auStack_298);
            uVar4 = local_28;
            FUN_00002cf8(auStack_29c,0x15d326);
            FUN_000193a8(uVar4,auStack_29c);
            uVar4 = local_28;
            FUN_00002cf8(auStack_2a0,0x15d326);
            FUN_00011dc4(uVar4,auStack_2a0,&DAT_002084b8);
            uVar4 = local_28;
            FUN_00002cf8(auStack_2a4,0x15d324);
            FUN_00011dc4(uVar4,auStack_2a4,&DAT_002084b8);
            uVar4 = local_28;
            FUN_00002cf8(auStack_2a8,0x15d323);
            FUN_00011dc4(uVar4,auStack_2a8,&DAT_002084b8);
            uVar4 = local_28;
            FUN_00002cf8(auStack_2ac,0x15d323);
            FUN_00013178(uVar4,auStack_2ac,&DAT_002084b8);
            uVar4 = local_28;
            FUN_00002cf8(auStack_2b0,0x15d32f);
            FUN_00011dc4(uVar4,auStack_2b0,&DAT_002084b8);
            plVar5 = (long *)FUN_00003ca8(local_28);
            FUN_00002cf8(auStack_2bc,0x15d31f);
            local_2b8 = (**(code **)(*plVar5 + 0x48))(plVar5,auStack_2bc,9999);
            FUN_000147f4(auStack_2c8,local_2b8,&uStack_2c9);
            FUN_0001910c(auStack_2d8,local_2b8,&uStack_2d9);
            lVar6 = FUN_0001deb4(auStack_2c8);
            if (lVar6 != 0) {
              lVar6 = FUN_0001decc(auStack_2d8);
              if (lVar6 != 0) {
                plVar5 = (long *)FUN_00014854(auStack_2c8);
                (**(code **)(*plVar5 + 0x28))(plVar5,1);
                plVar5 = (long *)FUN_00014854(auStack_2c8);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                          (auStack_328,"0x15d300kGCNoParagraphStyleKey",0);
                (**(code **)(*plVar5 + 0x18))(plVar5,auStack_328,0xfffffffe,1);
                __ZN8PMStringD1Ev(auStack_328);
                uVar4 = FUN_0000310c(local_58);
                __ZN7UIDListC1EP9IDataBase(auStack_330,uVar4);
                plVar5 = (long *)FUN_0001dee4(auStack_90);
                plVar5 = (long *)(**(code **)(*plVar5 + 0x18))();
                FUN_00002f44(auStack_334,0x218);
                (**(code **)(*plVar5 + 0x80))(plVar5,auStack_330,auStack_334);
                local_338 = 1;
                while( true ) {
                  iVar1 = local_338;
                  iVar3 = FUN_0001defc(auStack_330);
                  uVar4 = local_28;
                  if (iVar3 <= iVar1) break;
                  auVar7 = __ZNK7UIDList6GetRefEi(auStack_330,local_338);
                  local_350 = auVar7;
                  FUN_0001df58(auStack_340,local_350,&uStack_351);
                  plVar5 = (long *)FUN_0001df94(auStack_340);
                  (**(code **)(*plVar5 + 0xa0))(auStack_3a0,plVar5,0);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_3e8,"[Pro Ed.",0);
                  sVar2 = __ZNK8PMString8ContainsERKS_i(auStack_3a0,auStack_3e8,0);
                  __ZN8PMStringD1Ev(auStack_3e8);
                  if (sVar2 == 0) {
                    plVar5 = (long *)FUN_00014854(auStack_2c8);
                    (**(code **)(*plVar5 + 0x18))(plVar5,auStack_3a0,0xfffffffe,1);
                    local_48 = 0;
                  }
                  else {
                    local_48 = 6;
                  }
                  __ZN8PMStringD1Ev(auStack_3a0);
                  FUN_0001dfac(auStack_340);
                  local_338 = local_338 + 1;
                }
                FUN_00002cf8(auStack_3ec,0x15d31f);
                local_3f0 = 0;
                FUN_00015b04(uVar4,auStack_3ec,&local_3f0,1);
                __ZN7UIDListD1Ev(auStack_330);
              }
            }
            FUN_00019184(auStack_2d8);
            FUN_00014a30(auStack_2c8);
            local_48 = 0;
          }
          else {
            local_48 = 2;
          }
          FUN_0000e2f8(auStack_a0);
        }
        else {
          local_48 = 2;
        }
        FUN_0001dfd8(auStack_90);
      }
      else {
        local_48 = 2;
      }
      FUN_0001d190(auStack_70);
    }
    else {
      local_48 = 2;
    }
    FUN_0000e06c(auStack_60);
  }
  else {
    local_48 = 2;
  }
  FUN_00002b9c(auStack_30);
  return;
}
