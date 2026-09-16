/*
Lock (row 21). FUN_00157dec handles the two Lock checkboxes 0x15d4c5 (rows) and 0x15d4c6 (columns): on, the line fields
are disabled and the margin sum k_t + k_b (or k_i + k_o) is stored through vtable + 0x1e0 (or + 0x1f0); off, 0 is stored.
FUN_00129a38 is the top and bottom line observer: with the lock on and sum X read at vtable + 0x1d8, editing one margin
writes the other as X - new, and an edit with X - new < 0 is reverted to X - other; the rows are then reapplied over the
moved type area with the same counts and gutters (FUN_0015d6f4, FUN_00163ee0).

PMReal helpers: construct 0x13490, operator/ 0x157c8, operator* 0x15a7c, operator+ 0x17c4c, operator- 0x28080,
Round 0x15abc (floor(x + 0.5)), floor 0x37d70, a > b + 1e-8 0x14140, a >= b - 1e-8 0x15808, a <= b + 1e-8 0x31d38,
a < b - 1e-8 0x3208c, |a - b| < 1e-8 0x132fc, not equal 0x18504, quantise 0x152f0 (mode 3 three decimals, mode 4 four).
Widget access: read value 0x13338, read value in the panel unit 0x1544c, read checkbox 0x12ef4, read dropdown index 0x13034,
write value 0x13c20, set maximum 0x196d0, unit factor at object + 0x88.
*/

//==== FUNC @ 157dec -> 00157dec

void FUN_00157dec(void)

{
  int iVar1;
  short sVar2;
  int iVar3;
  undefined8 uVar4;
  undefined8 uVar5;
  long *plVar6;
  undefined1 auVar7 [16];
  undefined1 auStack_2328 [76];
  undefined1 auStack_22dc [4];
  undefined1 auStack_22d8 [76];
  undefined1 auStack_228c [4];
  undefined1 auStack_2288 [4];
  undefined1 auStack_2284 [4];
  undefined1 auStack_2280 [8];
  undefined1 auStack_2278 [8];
  undefined1 auStack_2270 [8];
  undefined1 auStack_2268 [8];
  undefined1 auStack_2260 [8];
  undefined1 auStack_2258 [8];
  undefined1 auStack_2250 [8];
  undefined1 auStack_2248 [12];
  undefined1 auStack_223c [4];
  undefined1 auStack_2238 [76];
  undefined1 auStack_21ec [4];
  undefined1 auStack_21e8 [76];
  undefined1 auStack_219c [4];
  undefined1 auStack_2198 [76];
  undefined1 auStack_214c [4];
  undefined1 auStack_2148 [76];
  undefined1 auStack_20fc [4];
  undefined1 auStack_20f8 [76];
  undefined1 auStack_20ac [4];
  undefined1 auStack_20a8 [76];
  undefined1 auStack_205c [4];
  undefined1 auStack_2058 [76];
  undefined1 auStack_200c [4];
  undefined1 auStack_2008 [76];
  undefined1 auStack_1fbc [4];
  undefined1 auStack_1fb8 [76];
  undefined1 auStack_1f6c [4];
  undefined1 auStack_1f68 [76];
  undefined1 auStack_1f1c [4];
  undefined1 auStack_1f18 [76];
  undefined1 auStack_1ecc [4];
  undefined1 auStack_1ec8 [76];
  undefined1 auStack_1e7c [4];
  undefined1 auStack_1e78 [4];
  undefined1 auStack_1e74 [4];
  undefined1 auStack_1e70 [76];
  undefined1 auStack_1e24 [4];
  undefined1 auStack_1e20 [76];
  undefined1 auStack_1dd4 [4];
  undefined1 auStack_1dd0 [76];
  undefined1 auStack_1d84 [4];
  undefined1 auStack_1d80 [76];
  undefined1 auStack_1d34 [4];
  undefined1 auStack_1d30 [76];
  undefined1 auStack_1ce4 [4];
  undefined1 auStack_1ce0 [76];
  undefined1 auStack_1c94 [4];
  undefined1 auStack_1c90 [76];
  undefined1 auStack_1c44 [4];
  undefined1 auStack_1c40 [76];
  undefined1 auStack_1bf4 [4];
  undefined1 auStack_1bf0 [76];
  undefined1 auStack_1ba4 [4];
  undefined1 auStack_1ba0 [76];
  undefined1 auStack_1b54 [4];
  undefined1 auStack_1b50 [76];
  undefined1 auStack_1b04 [4];
  undefined1 auStack_1b00 [76];
  undefined1 auStack_1ab4 [4];
  undefined1 auStack_1ab0 [76];
  undefined1 auStack_1a64 [4];
  undefined1 auStack_1a60 [4];
  undefined1 auStack_1a5c [4];
  undefined1 auStack_1a58 [4];
  undefined1 auStack_1a54 [4];
  undefined1 auStack_1a50 [4];
  undefined1 auStack_1a4c [4];
  undefined1 auStack_1a48 [4];
  undefined1 auStack_1a44 [4];
  undefined1 auStack_1a40 [4];
  undefined1 auStack_1a3c [4];
  undefined1 auStack_1a38 [4];
  undefined1 auStack_1a34 [4];
  undefined1 auStack_1a30 [4];
  undefined1 auStack_1a2c [4];
  undefined1 auStack_1a28 [4];
  undefined1 auStack_1a24 [4];
  undefined1 auStack_1a20 [4];
  undefined1 auStack_1a1c [4];
  undefined1 auStack_1a18 [4];
  undefined1 auStack_1a14 [4];
  undefined1 auStack_1a10 [72];
  undefined1 auStack_19c8 [76];
  undefined1 auStack_197c [4];
  undefined1 auStack_1978 [76];
  undefined1 auStack_192c [4];
  undefined1 auStack_1928 [76];
  undefined1 auStack_18dc [4];
  undefined1 auStack_18d8 [76];
  undefined1 auStack_188c [4];
  undefined1 auStack_1888 [76];
  undefined1 auStack_183c [4];
  undefined1 auStack_1838 [76];
  undefined1 auStack_17ec [4];
  undefined1 auStack_17e8 [76];
  undefined1 auStack_179c [4];
  undefined1 auStack_1798 [76];
  undefined1 auStack_174c [4];
  undefined1 auStack_1748 [76];
  undefined1 auStack_16fc [4];
  undefined1 auStack_16f8 [76];
  undefined1 auStack_16ac [4];
  undefined1 auStack_16a8 [76];
  undefined1 auStack_165c [4];
  undefined1 auStack_1658 [76];
  undefined1 auStack_160c [4];
  undefined1 auStack_1608 [76];
  undefined1 auStack_15bc [4];
  undefined1 auStack_15b8 [76];
  undefined1 auStack_156c [4];
  undefined1 auStack_1568 [72];
  undefined1 auStack_1520 [4];
  undefined1 auStack_151c [4];
  undefined1 auStack_1518 [4];
  undefined1 auStack_1514 [4];
  undefined1 auStack_1510 [76];
  undefined1 auStack_14c4 [4];
  undefined1 auStack_14c0 [4];
  undefined1 auStack_14bc [4];
  undefined1 auStack_14b8 [8];
  undefined1 auStack_14b0 [8];
  undefined1 auStack_14a8 [8];
  undefined1 auStack_14a0 [8];
  undefined1 auStack_1498 [8];
  undefined1 auStack_1490 [76];
  undefined1 auStack_1444 [4];
  undefined1 auStack_1440 [76];
  undefined1 auStack_13f4 [4];
  undefined1 auStack_13f0 [76];
  undefined1 auStack_13a4 [4];
  undefined1 auStack_13a0 [76];
  undefined1 auStack_1354 [4];
  undefined1 auStack_1350 [76];
  undefined1 auStack_1304 [4];
  undefined1 auStack_1300 [76];
  undefined1 auStack_12b4 [4];
  undefined1 auStack_12b0 [76];
  undefined1 auStack_1264 [4];
  undefined1 auStack_1260 [76];
  undefined1 auStack_1214 [4];
  undefined1 auStack_1210 [76];
  undefined1 auStack_11c4 [4];
  undefined1 auStack_11c0 [76];
  undefined1 auStack_1174 [4];
  undefined1 auStack_1170 [76];
  undefined1 auStack_1124 [4];
  undefined1 auStack_1120 [76];
  undefined1 auStack_10d4 [4];
  undefined1 auStack_10d0 [76];
  undefined1 auStack_1084 [4];
  undefined1 auStack_1080 [76];
  undefined1 auStack_1034 [4];
  undefined1 auStack_1030 [72];
  undefined1 auStack_fe8 [76];
  undefined1 auStack_f9c [4];
  undefined1 auStack_f98 [76];
  undefined1 auStack_f4c [4];
  undefined1 auStack_f48 [76];
  undefined1 auStack_efc [4];
  undefined1 auStack_ef8 [76];
  undefined1 auStack_eac [4];
  undefined1 auStack_ea8 [72];
  undefined1 auStack_e60 [76];
  undefined1 auStack_e14 [4];
  undefined1 auStack_e10 [76];
  undefined1 auStack_dc4 [4];
  undefined1 auStack_dc0 [76];
  undefined1 auStack_d74 [4];
  undefined1 auStack_d70 [76];
  undefined1 auStack_d24 [4];
  undefined1 auStack_d20 [72];
  undefined1 auStack_cd8 [4];
  undefined1 auStack_cd4 [4];
  undefined1 auStack_cd0 [4];
  undefined1 auStack_ccc [4];
  undefined1 auStack_cc8 [4];
  undefined1 auStack_cc4 [4];
  undefined1 auStack_cc0 [4];
  undefined1 auStack_cbc [4];
  undefined1 auStack_cb8 [4];
  undefined1 auStack_cb4 [4];
  undefined1 auStack_cb0 [4];
  undefined1 auStack_cac [4];
  undefined1 auStack_ca8 [4];
  undefined1 auStack_ca4 [4];
  undefined1 auStack_ca0 [4];
  undefined1 auStack_c9c [4];
  undefined1 auStack_c98 [4];
  undefined1 auStack_c94 [4];
  undefined1 auStack_c90 [4];
  undefined1 auStack_c8c [4];
  undefined1 auStack_c88 [4];
  undefined1 auStack_c84 [4];
  undefined1 auStack_c80 [4];
  undefined1 auStack_c7c [4];
  undefined1 auStack_c78 [4];
  undefined1 auStack_c74 [4];
  undefined1 auStack_c70 [4];
  undefined1 auStack_c6c [4];
  undefined1 auStack_c68 [76];
  undefined1 auStack_c1c [4];
  undefined1 auStack_c18 [76];
  undefined1 auStack_bcc [4];
  undefined1 auStack_bc8 [76];
  undefined1 auStack_b7c [4];
  undefined1 auStack_b78 [76];
  undefined1 auStack_b2c [4];
  undefined1 auStack_b28 [76];
  undefined1 auStack_adc [4];
  undefined1 auStack_ad8 [12];
  undefined1 auStack_acc [4];
  undefined1 auStack_ac8 [12];
  undefined1 auStack_abc [4];
  undefined1 auStack_ab8 [12];
  undefined1 auStack_aac [4];
  undefined1 auStack_aa8 [12];
  undefined1 auStack_a9c [4];
  undefined1 auStack_a98 [4];
  undefined1 auStack_a94 [4];
  undefined1 auStack_a90 [4];
  undefined1 auStack_a8c [4];
  undefined1 auStack_a88 [4];
  undefined1 auStack_a84 [4];
  undefined1 auStack_a80 [72];
  undefined1 auStack_a38 [72];
  undefined4 local_9f0;
  undefined1 auStack_9ec [4];
  undefined1 auStack_9e8 [72];
  undefined1 auStack_9a0 [72];
  undefined1 auStack_958 [72];
  undefined1 auStack_910 [75];
  undefined1 uStack_8c5;
  undefined4 local_8c4;
  undefined1 auStack_8c0 [12];
  int local_8b4;
  undefined1 auStack_8b0 [12];
  undefined1 auStack_8a4 [4];
  undefined1 auStack_8a0 [4];
  undefined1 auStack_89c [4];
  undefined1 auStack_898 [4];
  undefined4 local_894;
  undefined1 auStack_890 [4];
  undefined1 auStack_88c [4];
  undefined1 auStack_888 [72];
  undefined1 auStack_840 [4];
  undefined4 local_83c;
  undefined1 auStack_838 [4];
  undefined1 auStack_834 [4];
  undefined1 auStack_830 [72];
  undefined1 auStack_7e8 [72];
  undefined1 auStack_7a0 [72];
  undefined1 auStack_758 [72];
  undefined1 auStack_710 [72];
  undefined1 auStack_6c8 [72];
  undefined1 auStack_680 [72];
  undefined1 auStack_638 [72];
  undefined4 local_5f0;
  undefined4 local_5ec;
  undefined1 auStack_5e8 [4];
  undefined1 auStack_5e4 [4];
  undefined4 local_5e0;
  undefined1 auStack_5dc [4];
  undefined1 auStack_5d8 [4];
  undefined1 auStack_5d4 [4];
  undefined1 auStack_5d0 [76];
  undefined1 auStack_584 [4];
  undefined1 auStack_580 [76];
  undefined1 auStack_534 [4];
  undefined1 auStack_530 [76];
  undefined1 auStack_4e4 [4];
  undefined1 auStack_4e0 [76];
  undefined1 auStack_494 [4];
  undefined1 auStack_490 [76];
  undefined1 auStack_444 [4];
  undefined1 auStack_440 [4];
  undefined1 auStack_43c [4];
  undefined1 auStack_438 [4];
  undefined1 auStack_434 [4];
  undefined1 auStack_430 [4];
  undefined1 auStack_42c [4];
  undefined1 auStack_428 [8];
  undefined8 local_420;
  undefined1 auStack_414 [4];
  undefined1 auStack_410 [8];
  undefined8 local_408;
  undefined1 auStack_3fc [4];
  undefined1 auStack_3f8 [8];
  undefined8 local_3f0;
  undefined1 auStack_3e4 [4];
  undefined1 auStack_3e0 [8];
  undefined8 local_3d8;
  undefined1 auStack_3cc [4];
  undefined1 auStack_3c8 [8];
  undefined8 local_3c0;
  undefined1 auStack_3b4 [4];
  undefined1 auStack_3b0 [8];
  undefined8 local_3a8;
  undefined1 auStack_39c [4];
  undefined1 auStack_398 [76];
  undefined1 auStack_34c [4];
  undefined1 auStack_348 [8];
  undefined8 local_340;
  undefined1 auStack_338 [4];
  undefined1 auStack_334 [4];
  undefined1 auStack_330 [8];
  undefined8 local_328;
  undefined1 auStack_320 [4];
  undefined1 auStack_31c [4];
  undefined1 auStack_318 [8];
  undefined8 local_310;
  undefined1 auStack_308 [4];
  undefined4 local_304;
  undefined1 auStack_300 [8];
  undefined8 local_2f8;
  undefined1 auStack_2f0 [72];
  undefined4 local_2a8;
  undefined1 auStack_2a4 [4];
  undefined1 auStack_2a0 [72];
  undefined8 local_258;
  undefined8 local_250;
  undefined1 auStack_248 [76];
  undefined1 auStack_1fc [4];
  undefined1 auStack_1f8 [76];
  undefined1 auStack_1ac [4];
  undefined1 auStack_1a8 [4];
  undefined1 auStack_1a4 [4];
  undefined1 auStack_1a0 [7];
  undefined1 uStack_199;
  undefined1 auStack_198 [15];
  undefined1 uStack_189;
  undefined1 auStack_188 [15];
  undefined1 uStack_179;
  undefined1 auStack_178 [8];
  undefined1 local_170 [16];
  undefined1 uStack_159;
  undefined1 auStack_158 [8];
  undefined1 local_150 [16];
  undefined1 auStack_140 [15];
  undefined1 uStack_131;
  undefined1 auStack_130 [12];
  undefined4 local_124;
  undefined1 auStack_120 [88];
  undefined1 auStack_c8 [78];
  short local_7a;
  undefined1 auStack_78 [72];
  undefined8 local_30;
  undefined8 local_28;
  
  auVar7 = (*(code *)PTR____chkstk_darwin_002340e0)();
  local_30 = auVar7._8_8_;
  uVar4 = auVar7._0_8_;
  local_28 = uVar4;
  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
            (auStack_78,"Do you really want to reset the entire document?");
  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_c8,"Yes",0);
  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_120,"No",0);
  sVar2 = __ZN6CAlert10ModalAlertERK8PMStringS2_S2_S2_ss
                    (auStack_78,auStack_c8,auStack_120,PTR__kNullString_00234110,1,4);
  __ZN8PMStringD1Ev(auStack_120);
  __ZN8PMStringD1Ev(auStack_c8);
  local_7a = sVar2;
  if (sVar2 == 1) {
    uVar5 = __Z26GetExecutionContextSessionv();
    FUN_00002978(auStack_130,uVar5,&uStack_131);
    sVar2 = FUN_000029b4(auStack_130);
    if (sVar2 == 0) {
      FUN_001a2484(auStack_140);
      sVar2 = FUN_0000e2a4(auStack_140);
      if (sVar2 == 0) {
        plVar6 = (long *)FUN_000029d8(auStack_130);
        local_150 = (**(code **)(*plVar6 + 0x18))();
        FUN_0000df20(auStack_158,local_150,&uStack_159);
        sVar2 = FUN_0000df5c(auStack_158);
        if (sVar2 == 0) {
          plVar6 = (long *)FUN_000029d8(auStack_130);
          local_170 = (**(code **)(*plVar6 + 0x28))();
          FUN_00017bd4(auStack_178,local_170,&uStack_179);
          sVar2 = FUN_00017c10(auStack_178);
          if (sVar2 == 0) {
            uVar5 = FUN_0000df80(auStack_158);
            FUN_0000df98(auStack_188,uVar5,&uStack_189);
            sVar2 = FUN_0000dfd4(auStack_188);
            if (sVar2 == 0) {
              FUN_00032b68(auStack_198,local_150,&uStack_199);
              sVar2 = FUN_00032ba4(auStack_198);
              uVar5 = local_30;
              if (sVar2 == 0) {
                FUN_00002cf8(auStack_1a0,&DAT_0015d318);
                FUN_00027534(uVar5,auStack_1a0);
                uVar5 = local_30;
                FUN_00002cf8(auStack_1a4,&DAT_0015d317);
                FUN_000136f4(uVar5,auStack_1a4,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_1a8,&DAT_0015d312);
                FUN_00013178(uVar5,auStack_1a8,&DAT_00208c52);
                FUN_00011e90(local_30,&DAT_00208c52);
                uVar5 = local_30;
                FUN_00002cf8(auStack_1ac,0x15d30b);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1f8,"",0);
                FUN_00013588(uVar5,auStack_1ac,auStack_1f8);
                __ZN8PMStringD1Ev(auStack_1f8);
                uVar5 = local_30;
                FUN_00002cf8(auStack_1fc,&DAT_0015d30c);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_248,"",0);
                FUN_00013588(uVar5,auStack_1fc,auStack_248);
                __ZN8PMStringD1Ev(auStack_248);
                plVar6 = (long *)FUN_000029d8(auStack_130);
                local_250 = (**(code **)(*plVar6 + 0x88))();
                plVar6 = (long *)FUN_000029d8(auStack_130);
                local_258 = (**(code **)(*plVar6 + 0xb8))();
                uVar5 = local_30;
                FUN_00002cf8(auStack_2a4,0x15d306);
                local_2a8 = 0xffffffff;
                FUN_00012c10(auStack_2a0,uVar5,auStack_2a4,&local_2a8);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                          (auStack_2f0,"0x15d300kGCMillimetersKey",0);
                sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_2a0,auStack_2f0,1,0);
                __ZN8PMStringD1Ev(auStack_2f0);
                if (sVar2 == 0) {
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_398,"0x15d300kGCInchesKey",0)
                  ;
                  sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_2a0,auStack_398,1,0);
                  __ZN8PMStringD1Ev(auStack_398);
                  uVar5 = local_30;
                  if (sVar2 == 0) {
                    FUN_00002cf8(auStack_42c,0x15d31a);
                    FUN_00013c20(uVar5,auStack_42c,&local_250);
                    uVar5 = local_30;
                    FUN_00002cf8(auStack_430,0x15d31b);
                    FUN_00013c20(uVar5,auStack_430,&local_258);
                    uVar5 = local_30;
                    FUN_00002cf8(auStack_434,0x15d338);
                    FUN_00013c20(uVar5,auStack_434,&local_250);
                    uVar5 = local_30;
                    FUN_00002cf8(auStack_438,0x15d341);
                    FUN_00013c20(uVar5,auStack_438,&local_258);
                    uVar5 = local_30;
                    FUN_00002cf8(auStack_43c,0x15d358);
                    FUN_00013c20(uVar5,auStack_43c,&local_250);
                    uVar5 = local_30;
                    FUN_00002cf8(auStack_440,0x15d364);
                    FUN_00013c20(uVar5,auStack_440,&local_258);
                  }
                  else {
                    FUN_00002cf8(auStack_39c,0x15d31a);
                    FUN_00013490(0x4052000000000000);
                    local_3a8 = FUN_000157c8(&local_250,auStack_3b0);
                    FUN_00013c20(uVar5,auStack_39c,&local_3a8);
                    uVar5 = local_30;
                    FUN_00002cf8(auStack_3b4,0x15d31b);
                    FUN_00013490(0x4052000000000000);
                    local_3c0 = FUN_000157c8(&local_258,auStack_3c8);
                    FUN_00013c20(uVar5,auStack_3b4,&local_3c0);
                    uVar5 = local_30;
                    FUN_00002cf8(auStack_3cc,0x15d338);
                    FUN_00013490(0x4052000000000000);
                    local_3d8 = FUN_000157c8(&local_250,auStack_3e0);
                    FUN_00013c20(uVar5,auStack_3cc,&local_3d8);
                    uVar5 = local_30;
                    FUN_00002cf8(auStack_3e4,0x15d341);
                    FUN_00013490(0x4052000000000000);
                    local_3f0 = FUN_000157c8(&local_258,auStack_3f8);
                    FUN_00013c20(uVar5,auStack_3e4,&local_3f0);
                    uVar5 = local_30;
                    FUN_00002cf8(auStack_3fc,0x15d358);
                    FUN_00013490(0x4052000000000000);
                    local_408 = FUN_000157c8(&local_250,auStack_410);
                    FUN_00013c20(uVar5,auStack_3fc,&local_408);
                    uVar5 = local_30;
                    FUN_00002cf8(auStack_414,0x15d364);
                    FUN_00013490(0x4052000000000000);
                    local_420 = FUN_000157c8(&local_258,auStack_428);
                    FUN_00013c20(uVar5,auStack_414,&local_420);
                  }
                }
                else {
                  FUN_00013490(0x4006ad5b202b0759);
                  local_2f8 = FUN_000157c8(&local_258,auStack_300);
                  local_304 = 3;
                  FUN_000152f0(&local_2f8,&local_304);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_308,0x15d31a);
                  FUN_00013490(0x4006ad5b202b0759);
                  local_310 = FUN_000157c8(&local_250,auStack_318);
                  FUN_00013c20(uVar5,auStack_308,&local_310);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_31c,0x15d31b);
                  FUN_00013c20(uVar5,auStack_31c,&local_2f8);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_320,0x15d338);
                  FUN_00013490(0x4006ad5b202b0759);
                  local_328 = FUN_000157c8(&local_250,auStack_330);
                  FUN_00013c20(uVar5,auStack_320,&local_328);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_334,0x15d341);
                  FUN_00013c20(uVar5,auStack_334,&local_2f8);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_338,0x15d358);
                  FUN_00013490(0x4006ad5b202b0759);
                  local_340 = FUN_000157c8(&local_250,auStack_348);
                  FUN_00013c20(uVar5,auStack_338,&local_340);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_34c,0x15d364);
                  FUN_00013c20(uVar5,auStack_34c,&local_2f8);
                }
                uVar5 = local_30;
                FUN_00002cf8(auStack_444,0x15d31c);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_490,"",0);
                FUN_00013588(uVar5,auStack_444,auStack_490);
                __ZN8PMStringD1Ev(auStack_490);
                uVar5 = local_30;
                FUN_00002cf8(auStack_494,0x15d32e);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_4e0,"",0);
                FUN_00013588(uVar5,auStack_494,auStack_4e0);
                __ZN8PMStringD1Ev(auStack_4e0);
                uVar5 = local_30;
                FUN_00002cf8(auStack_4e4,0x15d330);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_530,"",0);
                FUN_00013588(uVar5,auStack_4e4,auStack_530);
                __ZN8PMStringD1Ev(auStack_530);
                uVar5 = local_30;
                FUN_00002cf8(auStack_534,0x15d324);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_580,"0x15d300kGCSquareKey",0);
                FUN_00013588(uVar5,auStack_534,auStack_580);
                __ZN8PMStringD1Ev(auStack_580);
                uVar5 = local_30;
                FUN_00002cf8(auStack_584,0x15d32f);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                          (auStack_5d0,"0x15d300kGCCalculateKey",0);
                FUN_00013588(uVar5,auStack_584,auStack_5d0);
                __ZN8PMStringD1Ev(auStack_5d0);
                uVar5 = local_30;
                FUN_00002cf8(auStack_5d4,0x15d326);
                FUN_000193a8(uVar5,auStack_5d4);
                uVar5 = local_30;
                FUN_00002cf8(auStack_5d8,0x15d326);
                FUN_00011dc4(uVar5,auStack_5d8,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_5dc,0x15d326);
                local_5e0 = 0;
                FUN_00015b04(uVar5,auStack_5dc,&local_5e0,1);
                uVar5 = local_30;
                FUN_00002cf8(auStack_5e4,0x15d32f);
                FUN_00011dc4(uVar5,auStack_5e4,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_5e8,0x15d32e);
                FUN_00011dc4(uVar5,auStack_5e8,&DAT_00208c52);
                plVar6 = (long *)FUN_000029d8(auStack_130);
                local_5ec = 1;
                (**(code **)(*plVar6 + 0xb0))(plVar6,&local_5ec);
                plVar6 = (long *)FUN_000029d8(auStack_130);
                local_5f0 = 1;
                (**(code **)(*plVar6 + 0xe0))(plVar6,&local_5f0);
                plVar6 = (long *)FUN_000029d8(auStack_130);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_638,"",0);
                (**(code **)(*plVar6 + 0x140))(plVar6,auStack_638);
                __ZN8PMStringD1Ev(auStack_638);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_680,"[GC] Set Leading",0);
                FUN_001b4670(auStack_680);
                __ZN8PMStringD1Ev(auStack_680);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_710,"[GC] Subdivision: ",0);
                FUN_001cc9ac(auStack_6c8,auStack_710);
                __ZN8PMStringD1Ev(auStack_710);
                FUN_001b4670(auStack_6c8);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                          (auStack_7a0,"[GC] Desired Grid Width: ",0);
                FUN_001cc9ac(auStack_758,auStack_7a0);
                FUN_0000dd8c();
                __ZN8PMStringD1Ev(auStack_758);
                __ZN8PMStringD1Ev(auStack_7a0);
                FUN_001b4670(auStack_6c8);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_830,"[GC] Square: ",0);
                FUN_001cc9ac(auStack_7e8,auStack_830);
                FUN_0000dd8c();
                __ZN8PMStringD1Ev(auStack_7e8);
                __ZN8PMStringD1Ev(auStack_830);
                FUN_001b4670(auStack_6c8);
                uVar5 = local_30;
                FUN_00002cf8(auStack_834,0x15d3a9);
                FUN_00011dc4(uVar5,auStack_834,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_838,0x15d3aa);
                local_83c = 0;
                FUN_00015b04(uVar5,auStack_838,&local_83c,1);
                uVar5 = local_30;
                FUN_00002cf8(auStack_840,0x15d3a8);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_888,"",0);
                FUN_00013588(uVar5,auStack_840,auStack_888);
                __ZN8PMStringD1Ev(auStack_888);
                uVar5 = local_30;
                FUN_00002cf8(auStack_88c,0x15d3a8);
                FUN_00011dc4(uVar5,auStack_88c,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_890,0x15d3aa);
                local_894 = 2;
                FUN_00015844(uVar5,auStack_890,&local_894,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_898,0x15d3a1);
                FUN_000187cc(uVar5,auStack_898,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_89c,0x15d3a2);
                FUN_000187cc(uVar5,auStack_89c,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_8a0,0x15d3d7);
                FUN_000187cc(uVar5,auStack_8a0,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_8a4,0x15d3e7);
                FUN_000187cc(uVar5,auStack_8a4,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00013490(0);
                FUN_00015c98(uVar5,auStack_8b0);
                local_8b4 = 0;
                while( true ) {
                  iVar1 = local_8b4;
                  plVar6 = (long *)FUN_00032bc8(auStack_198);
                  iVar3 = (**(code **)(*plVar6 + 0x38))();
                  if (iVar3 <= iVar1) break;
                  uVar5 = FUN_0000310c(local_150);
                  plVar6 = (long *)FUN_00032bc8(auStack_198);
                  local_8c4 = (**(code **)(*plVar6 + 0x30))(plVar6,local_8b4);
                  FUN_00032be0(auStack_8c0,uVar5,local_8c4,&uStack_8c5);
                  sVar2 = FUN_0001b8dc(auStack_8c0);
                  if (sVar2 == 0) {
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_910,"",0);
                    plVar6 = (long *)FUN_0001d0f4(auStack_8c0);
                    (**(code **)(*plVar6 + 0x18))(plVar6,auStack_910);
                    __ZN8PMStringC1ERKS_(auStack_958,auStack_910);
                    __ZN8PMString6AppendEPKciNS_14StringEncodingE
                              (auStack_958," Break Horizontal",0x7fffffff,0xffffffff);
                    FUN_001b4670(auStack_958);
                    __ZN8PMStringD1Ev(auStack_958);
                    __ZN8PMStringD1Ev(auStack_910);
                    local_124 = 0;
                  }
                  else {
                    local_124 = 6;
                  }
                  FUN_0001d10c(auStack_8c0);
                  local_8b4 = local_8b4 + 1;
                }
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_9a0,"[GC] Break Vertical",0);
                FUN_001b4670(auStack_9a0);
                __ZN8PMStringD1Ev(auStack_9a0);
                uVar5 = local_30;
                FUN_00002cf8(auStack_9ec,0x15d309);
                local_9f0 = 0xffffffff;
                FUN_00012c10(auStack_9e8,uVar5,auStack_9ec,&local_9f0);
                __ZN8PMStringC1ERKS_(auStack_a38,auStack_9e8);
                __ZN8PMString6AppendEPKciNS_14StringEncodingE
                          (auStack_a38," Horizontal Grid",0x7fffffff,0xffffffff);
                __ZN8PMStringC1ERKS_(auStack_a80,auStack_9e8);
                __ZN8PMString6AppendEPKciNS_14StringEncodingE
                          (auStack_a80," Subleading",0x7fffffff,0xffffffff);
                FUN_001b4670(auStack_a38);
                FUN_001b4670(auStack_a80);
                uVar5 = local_30;
                FUN_00002cf8(auStack_a84,0x15d4cd);
                FUN_00013178(uVar5,auStack_a84,&DAT_00208c52);
                FUN_001387f4(uVar4);
                uVar5 = local_30;
                FUN_00002cf8(auStack_a88,0x15d4be);
                FUN_00013178(uVar5,auStack_a88,&DAT_00208c52);
                FUN_00138ecc(uVar4);
                uVar5 = local_30;
                FUN_00002cf8(auStack_a8c,0x15d4ca);
                FUN_00013178(uVar5,auStack_a8c,&DAT_00208c52);
                FUN_00139400(uVar4);
                uVar5 = local_30;
                FUN_00002cf8(auStack_a90,0x15d4d0);
                FUN_00013178(uVar5,auStack_a90,&DAT_00208c52);
                FUN_0013a18c(uVar4);
                uVar5 = local_30;
                FUN_00002cf8(auStack_a94,0x15d4c0);
                FUN_00013178(uVar5,auStack_a94,&DAT_00208c52);
                FUN_0013af6c(uVar4);
                uVar5 = local_30;
                FUN_00002cf8(auStack_a98,0x15d4e0);
                FUN_00013178(uVar5,auStack_a98,&DAT_00208c52);
                FUN_0013b4ec(uVar4);
                uVar5 = local_30;
                FUN_00002cf8(auStack_a9c,0x15d49b);
                FUN_00013490(0);
                FUN_00013c20(uVar5,auStack_a9c,auStack_aa8);
                uVar5 = local_30;
                FUN_00002cf8(auStack_aac,0x15d49d);
                FUN_00013490(0);
                FUN_00013c20(uVar5,auStack_aac,auStack_ab8);
                uVar5 = local_30;
                FUN_00002cf8(auStack_abc,0x15d49f);
                FUN_00013490(0);
                FUN_00013c20(uVar5,auStack_abc,auStack_ac8);
                uVar5 = local_30;
                FUN_00002cf8(auStack_acc,0x15d4a1);
                FUN_00013490(0);
                FUN_00013c20(uVar5,auStack_acc,auStack_ad8);
                uVar5 = local_30;
                FUN_00002cf8(auStack_adc,0x15d4a7);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_b28,"",0);
                FUN_00013588(uVar5,auStack_adc,auStack_b28);
                __ZN8PMStringD1Ev(auStack_b28);
                uVar5 = local_30;
                FUN_00002cf8(auStack_b2c,0x15d4a8);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_b78,"",0);
                FUN_00013588(uVar5,auStack_b2c,auStack_b78);
                __ZN8PMStringD1Ev(auStack_b78);
                uVar5 = local_30;
                FUN_00002cf8(auStack_b7c,0x15d4a9);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_bc8,"",0);
                FUN_00013588(uVar5,auStack_b7c,auStack_bc8);
                __ZN8PMStringD1Ev(auStack_bc8);
                uVar5 = local_30;
                FUN_00002cf8(auStack_bcc,0x15d494);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                          (auStack_c18,"0x15d300kGCWidthColonKey",0);
                FUN_00013588(uVar5,auStack_bcc,auStack_c18);
                __ZN8PMStringD1Ev(auStack_c18);
                uVar5 = local_30;
                FUN_00002cf8(auStack_c1c,0x15d495);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                          (auStack_c68,"0x15d300kGCHeightColonKey",0);
                FUN_00013588(uVar5,auStack_c1c,auStack_c68);
                __ZN8PMStringD1Ev(auStack_c68);
                uVar5 = local_30;
                FUN_00002cf8(auStack_c6c,0x15d4a3);
                FUN_00013178(uVar5,auStack_c6c,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_c70,0x15d4a4);
                FUN_00013178(uVar5,auStack_c70,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_c74,0x15d4c5);
                FUN_00013178(uVar5,auStack_c74,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_c78,0x15d4c6);
                FUN_00013178(uVar5,auStack_c78,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_c7c,0x15d4c5);
                FUN_00011dc4(uVar5,auStack_c7c,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_c80,0x15d4c6);
                FUN_00011dc4(uVar5,auStack_c80,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_c84,0x15d4a3);
                FUN_00011dc4(uVar5,auStack_c84,&DAT_00208c52);
                uVar5 = local_30;
                FUN_00002cf8(auStack_c88,0x15d4a4);
                FUN_00011dc4(uVar5,auStack_c88,&DAT_00208c52);
                uVar5 = local_30;
                FUN_00002cf8(auStack_c8c,0x15d49b);
                FUN_00011dc4(uVar5,auStack_c8c,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_c90,0x15d49d);
                FUN_00011dc4(uVar5,auStack_c90,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_c94,0x15d49f);
                FUN_00011dc4(uVar5,auStack_c94,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_c98,0x15d4a1);
                FUN_00011dc4(uVar5,auStack_c98,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_c9c,0x15d4ab);
                FUN_00011dc4(uVar5,auStack_c9c,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_ca0,0x15d4ad);
                FUN_00011dc4(uVar5,auStack_ca0,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_ca4,0x15d4af);
                FUN_00011dc4(uVar5,auStack_ca4,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_ca8,0x15d4b1);
                FUN_00011dc4(uVar5,auStack_ca8,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_cac,0x15d4b7);
                FUN_00011dc4(uVar5,auStack_cac,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_cb0,0x15d4b9);
                FUN_00011dc4(uVar5,auStack_cb0,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_cb4,0x15d4b3);
                FUN_00011dc4(uVar5,auStack_cb4,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_cb8,0x15d4b5);
                FUN_00011dc4(uVar5,auStack_cb8,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_cbc,0x15d3fb);
                FUN_00011dc4(uVar5,auStack_cbc,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_cc0,0x15d3fd);
                FUN_00011dc4(uVar5,auStack_cc0,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_cc4,0x15d4bb);
                FUN_00011dc4(uVar5,auStack_cc4,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_cc8,0x15d4bd);
                FUN_00011dc4(uVar5,auStack_cc8,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_ccc,0x15d4e2);
                FUN_00011dc4(uVar5,auStack_ccc,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_cd0,0x15d4e3);
                FUN_00011dc4(uVar5,auStack_cd0,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_cd4,0x15d4e5);
                FUN_00011dc4(uVar5,auStack_cd4,&DAT_00208c50);
                uVar5 = local_30;
                FUN_00002cf8(auStack_cd8,0x15d4e6);
                FUN_00011dc4(uVar5,auStack_cd8,&DAT_00208c50);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                          (auStack_d20,"0x15d300kGCMillimetersKey",0);
                sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_2a0,auStack_d20,1,0);
                __ZN8PMStringD1Ev(auStack_d20);
                uVar5 = local_30;
                if (sVar2 == 0) {
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_ea8,"0x15d300kGCInchesKey",0)
                  ;
                  sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_2a0,auStack_ea8,1,0);
                  __ZN8PMStringD1Ev(auStack_ea8);
                  uVar5 = local_30;
                  if (sVar2 == 0) {
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                              (auStack_1030,"0x15d300kGCPointsPixelsKey",0);
                    sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_2a0,auStack_1030,1,0);
                    __ZN8PMStringD1Ev(auStack_1030);
                    uVar5 = local_30;
                    if (sVar2 != 0) {
                      FUN_00002cf8(auStack_1034,0x15d4ed);
                      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                                (auStack_1080,"0x15d300kGCPtKey",0);
                      FUN_00013588(uVar5,auStack_1034,auStack_1080);
                      __ZN8PMStringD1Ev(auStack_1080);
                      uVar5 = local_30;
                      FUN_00002cf8(auStack_1084,0x15d4ef);
                      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                                (auStack_10d0,"0x15d300kGCPtKey",0);
                      FUN_00013588(uVar5,auStack_1084,auStack_10d0);
                      __ZN8PMStringD1Ev(auStack_10d0);
                      uVar5 = local_30;
                      FUN_00002cf8(auStack_10d4,0x15d4ee);
                      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                                (auStack_1120,"0x15d300kGCPtKey",0);
                      FUN_00013588(uVar5,auStack_10d4,auStack_1120);
                      __ZN8PMStringD1Ev(auStack_1120);
                      uVar5 = local_30;
                      FUN_00002cf8(auStack_1124,0x15d4f0);
                      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                                (auStack_1170,"0x15d300kGCPtKey",0);
                      FUN_00013588(uVar5,auStack_1124,auStack_1170);
                      __ZN8PMStringD1Ev(auStack_1170);
                    }
                  }
                  else {
                    FUN_00002cf8(auStack_eac,0x15d4ed);
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_ef8,"0x15d300kGCInKey",0);
                    FUN_00013588(uVar5,auStack_eac,auStack_ef8);
                    __ZN8PMStringD1Ev(auStack_ef8);
                    uVar5 = local_30;
                    FUN_00002cf8(auStack_efc,0x15d4ef);
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_f48,"0x15d300kGCInKey",0);
                    FUN_00013588(uVar5,auStack_efc,auStack_f48);
                    __ZN8PMStringD1Ev(auStack_f48);
                    uVar5 = local_30;
                    FUN_00002cf8(auStack_f4c,0x15d4ee);
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_f98,"0x15d300kGCInKey",0);
                    FUN_00013588(uVar5,auStack_f4c,auStack_f98);
                    __ZN8PMStringD1Ev(auStack_f98);
                    uVar5 = local_30;
                    FUN_00002cf8(auStack_f9c,0x15d4f0);
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_fe8,"0x15d300kGCInKey",0);
                    FUN_00013588(uVar5,auStack_f9c,auStack_fe8);
                    __ZN8PMStringD1Ev(auStack_fe8);
                  }
                }
                else {
                  FUN_00002cf8(auStack_d24,0x15d4ed);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_d70,"0x15d300kGCMmKey",0);
                  FUN_00013588(uVar5,auStack_d24,auStack_d70);
                  __ZN8PMStringD1Ev(auStack_d70);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_d74,0x15d4ef);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_dc0,"0x15d300kGCMmKey",0);
                  FUN_00013588(uVar5,auStack_d74,auStack_dc0);
                  __ZN8PMStringD1Ev(auStack_dc0);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_dc4,0x15d4ee);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_e10,"0x15d300kGCMmKey",0);
                  FUN_00013588(uVar5,auStack_dc4,auStack_e10);
                  __ZN8PMStringD1Ev(auStack_e10);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_e14,0x15d4f0);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_e60,"0x15d300kGCMmKey",0);
                  FUN_00013588(uVar5,auStack_e14,auStack_e60);
                  __ZN8PMStringD1Ev(auStack_e60);
                }
                uVar5 = local_30;
                FUN_00002cf8(auStack_1174,0x15d4d3);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_11c0,"0x15d300kGCGreaterKey",0)
                ;
                FUN_00013588(uVar5,auStack_1174,auStack_11c0);
                __ZN8PMStringD1Ev(auStack_11c0);
                uVar5 = local_30;
                FUN_00002cf8(auStack_11c4,0x15d4d4);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1210,"0x15d300kGCGreaterKey",0)
                ;
                FUN_00013588(uVar5,auStack_11c4,auStack_1210);
                __ZN8PMStringD1Ev(auStack_1210);
                uVar5 = local_30;
                FUN_00002cf8(auStack_1214,0x15d4d6);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1260,"0x15d300kGCGreaterKey",0)
                ;
                FUN_00013588(uVar5,auStack_1214,auStack_1260);
                __ZN8PMStringD1Ev(auStack_1260);
                uVar5 = local_30;
                FUN_00002cf8(auStack_1264,0x15d4d2);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_12b0,"0x15d300kGCGreaterKey",0)
                ;
                FUN_00013588(uVar5,auStack_1264,auStack_12b0);
                __ZN8PMStringD1Ev(auStack_12b0);
                uVar5 = local_30;
                FUN_00002cf8(auStack_12b4,0x15d4d5);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1300,"0x15d300kGCGreaterKey",0)
                ;
                FUN_00013588(uVar5,auStack_12b4,auStack_1300);
                __ZN8PMStringD1Ev(auStack_1300);
                uVar5 = local_30;
                FUN_00002cf8(auStack_1304,0x15d3ff);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1350,"0x15d300kGCGreaterKey",0)
                ;
                FUN_00013588(uVar5,auStack_1304,auStack_1350);
                __ZN8PMStringD1Ev(auStack_1350);
                uVar5 = local_30;
                FUN_00002cf8(auStack_1354,0x15d4e3);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_13a0,"",0);
                FUN_00013588(uVar5,auStack_1354,auStack_13a0);
                __ZN8PMStringD1Ev(auStack_13a0);
                uVar5 = local_30;
                FUN_00002cf8(auStack_13a4,0x15d4e4);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_13f0,"",0);
                FUN_00013588(uVar5,auStack_13a4,auStack_13f0);
                __ZN8PMStringD1Ev(auStack_13f0);
                uVar5 = local_30;
                FUN_00002cf8(auStack_13f4,0x15d4e6);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1440,"",0);
                FUN_00013588(uVar5,auStack_13f4,auStack_1440);
                __ZN8PMStringD1Ev(auStack_1440);
                uVar5 = local_30;
                FUN_00002cf8(auStack_1444,0x15d4e7);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1490,"",0);
                FUN_00013588(uVar5,auStack_1444,auStack_1490);
                __ZN8PMStringD1Ev(auStack_1490);
                plVar6 = (long *)FUN_000029d8(auStack_130);
                FUN_00013490(0);
                (**(code **)(*plVar6 + 0x130))(plVar6,auStack_1498);
                plVar6 = (long *)FUN_000029d8(auStack_130);
                FUN_00013490(0);
                (**(code **)(*plVar6 + 0x1d0))(plVar6,auStack_14a0);
                plVar6 = (long *)FUN_000029d8(auStack_130);
                FUN_00013490(0);
                (**(code **)(*plVar6 + 0x1e0))(plVar6,auStack_14a8);
                plVar6 = (long *)FUN_000029d8(auStack_130);
                FUN_00013490(0);
                (**(code **)(*plVar6 + 0x1f0))(plVar6,auStack_14b0);
                plVar6 = (long *)FUN_000029d8(auStack_130);
                (**(code **)(*plVar6 + 0x210))(plVar6,&DAT_00208c50);
                plVar6 = (long *)FUN_000029d8(auStack_130);
                (**(code **)(*plVar6 + 0x200))(plVar6,&DAT_00208c50);
                FUN_001a7c08(auStack_14b8);
                sVar2 = FUN_0000e280(auStack_14b8);
                if (sVar2 == 0) {
                  plVar6 = (long *)FUN_0000e2c8(auStack_140);
                  (**(code **)(*plVar6 + 0x20))(plVar6,&DAT_00208c52);
                  FUN_001ad93c(local_30);
                  FUN_0015d0a0(uVar4,1);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_14bc,0x15d324);
                  FUN_00011dc4(uVar5,auStack_14bc,&DAT_00208c50);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_14c0,0x15d323);
                  FUN_00011dc4(uVar5,auStack_14c0,&DAT_00208c50);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_14c4,0x15d323);
                  FUN_00013178(uVar5,auStack_14c4,&DAT_00208c50);
                  FUN_000e8428(uVar4);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1510,"[GC] CGS",0);
                  FUN_001b4670(auStack_1510);
                  __ZN8PMStringD1Ev(auStack_1510);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1514,0x15d34c);
                  FUN_00011dc4(uVar5,auStack_1514,&DAT_00208c52);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1518,0x15d33e);
                  FUN_00011dc4(uVar5,auStack_1518,&DAT_00208c50);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_151c,0x15d34a);
                  FUN_00011dc4(uVar5,auStack_151c,&DAT_00208c50);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1520,0x15d339);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1568,"",0);
                  FUN_00013588(uVar5,auStack_1520,auStack_1568);
                  __ZN8PMStringD1Ev(auStack_1568);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_156c,0x15d33a);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_15b8,"",0);
                  FUN_00013588(uVar5,auStack_156c,auStack_15b8);
                  __ZN8PMStringD1Ev(auStack_15b8);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_15bc,0x15d33b);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1608,"",0);
                  FUN_00013588(uVar5,auStack_15bc,auStack_1608);
                  __ZN8PMStringD1Ev(auStack_1608);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_160c,0x15d33c);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1658,"",0);
                  FUN_00013588(uVar5,auStack_160c,auStack_1658);
                  __ZN8PMStringD1Ev(auStack_1658);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_165c,0x15d342);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_16a8,"",0);
                  FUN_00013588(uVar5,auStack_165c,auStack_16a8);
                  __ZN8PMStringD1Ev(auStack_16a8);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_16ac,0x15d343);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_16f8,"",0);
                  FUN_00013588(uVar5,auStack_16ac,auStack_16f8);
                  __ZN8PMStringD1Ev(auStack_16f8);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_16fc,0x15d344);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1748,"",0);
                  FUN_00013588(uVar5,auStack_16fc,auStack_1748);
                  __ZN8PMStringD1Ev(auStack_1748);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_174c,0x15d345);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1798,"",0);
                  FUN_00013588(uVar5,auStack_174c,auStack_1798);
                  __ZN8PMStringD1Ev(auStack_1798);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_179c,0x15d346);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_17e8,"",0);
                  FUN_00013588(uVar5,auStack_179c,auStack_17e8);
                  __ZN8PMStringD1Ev(auStack_17e8);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_17ec,0x15d347);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1838,"",0);
                  FUN_00013588(uVar5,auStack_17ec,auStack_1838);
                  __ZN8PMStringD1Ev(auStack_1838);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_183c,0x15d348);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1888,"",0);
                  FUN_00013588(uVar5,auStack_183c,auStack_1888);
                  __ZN8PMStringD1Ev(auStack_1888);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_188c,0x15d34f);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_18d8,"",0);
                  FUN_00013588(uVar5,auStack_188c,auStack_18d8);
                  __ZN8PMStringD1Ev(auStack_18d8);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_18dc,0x15d350);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1928,"",0);
                  FUN_00013588(uVar5,auStack_18dc,auStack_1928);
                  __ZN8PMStringD1Ev(auStack_1928);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_192c,0x15d33d);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1978,"",0);
                  FUN_00013588(uVar5,auStack_192c,auStack_1978);
                  __ZN8PMStringD1Ev(auStack_1978);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_197c,0x15d349);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_19c8,"",0);
                  FUN_00013588(uVar5,auStack_197c,auStack_19c8);
                  __ZN8PMStringD1Ev(auStack_19c8);
                  plVar6 = (long *)FUN_000029d8(auStack_130);
                  (**(code **)(*plVar6 + 0x1b0))(plVar6,&DAT_00208c50);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1a10,"[GC] Smart Setup",0);
                  FUN_001b4670(auStack_1a10);
                  __ZN8PMStringD1Ev(auStack_1a10);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1a14,0x15d364);
                  FUN_00011dc4(uVar5,auStack_1a14,&DAT_00208c52);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1a18,0x15d366);
                  FUN_00011dc4(uVar5,auStack_1a18,&DAT_00208c52);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1a1c,0x15d357);
                  FUN_00011dc4(uVar5,auStack_1a1c,&DAT_00208c52);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1a20,0x15d369);
                  FUN_00011dc4(uVar5,auStack_1a20,&DAT_00208c52);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1a24,&DAT_0015d370);
                  FUN_00011dc4(uVar5,auStack_1a24,&DAT_00208c50);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1a28,&DAT_0015d372);
                  FUN_00011dc4(uVar5,auStack_1a28,&DAT_00208c50);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1a2c,&DAT_0015d374);
                  FUN_00011dc4(uVar5,auStack_1a2c,&DAT_00208c50);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1a30,&DAT_0015d375);
                  FUN_00011dc4(uVar5,auStack_1a30,&DAT_00208c50);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1a34,&DAT_0015d37d);
                  FUN_00011dc4(uVar5,auStack_1a34,&DAT_00208c50);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1a38,&DAT_0015d37e);
                  FUN_00011dc4(uVar5,auStack_1a38,&DAT_00208c50);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1a3c,&DAT_0015d376);
                  FUN_00011dc4(uVar5,auStack_1a3c,&DAT_00208c50);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1a40,&DAT_0015d378);
                  FUN_00011dc4(uVar5,auStack_1a40,&DAT_00208c50);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1a44,&DAT_0015d37a);
                  FUN_00011dc4(uVar5,auStack_1a44,&DAT_00208c50);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1a48,&DAT_0015d37b);
                  FUN_00011dc4(uVar5,auStack_1a48,&DAT_00208c50);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1a4c,0x15d360);
                  FUN_000136f4(uVar5,auStack_1a4c,&DAT_00208c52);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1a50,0x15d362);
                  FUN_000136f4(uVar5,auStack_1a50,&DAT_00208c52);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1a54,0x15d36d);
                  FUN_000136f4(uVar5,auStack_1a54,&DAT_00208c52);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1a58,0x15d36f);
                  FUN_000136f4(uVar5,auStack_1a58,&DAT_00208c52);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1a5c,0x15d36c);
                  FUN_000136f4(uVar5,auStack_1a5c,&DAT_00208c52);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1a60,0x15d36e);
                  FUN_000136f4(uVar5,auStack_1a60,&DAT_00208c52);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1a64,0x15d35a);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1ab0,"",0);
                  FUN_00013588(uVar5,auStack_1a64,auStack_1ab0);
                  __ZN8PMStringD1Ev(auStack_1ab0);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1ab4,0x15d356);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1b00,"",0);
                  FUN_00013588(uVar5,auStack_1ab4,auStack_1b00);
                  __ZN8PMStringD1Ev(auStack_1b00);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1b04,0x15d35d);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1b50,"",0);
                  FUN_00013588(uVar5,auStack_1b04,auStack_1b50);
                  __ZN8PMStringD1Ev(auStack_1b50);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1b54,0x15d359);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1ba0,"",0);
                  FUN_00013588(uVar5,auStack_1b54,auStack_1ba0);
                  __ZN8PMStringD1Ev(auStack_1ba0);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1ba4,0x15d35c);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1bf0,"",0);
                  FUN_00013588(uVar5,auStack_1ba4,auStack_1bf0);
                  __ZN8PMStringD1Ev(auStack_1bf0);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1bf4,0x15d35f);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1c40,"",0);
                  FUN_00013588(uVar5,auStack_1bf4,auStack_1c40);
                  __ZN8PMStringD1Ev(auStack_1c40);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1c44,0x15d361);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1c90,"",0);
                  FUN_00013588(uVar5,auStack_1c44,auStack_1c90);
                  __ZN8PMStringD1Ev(auStack_1c90);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1c94,0x15d363);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1ce0,"",0);
                  FUN_00013588(uVar5,auStack_1c94,auStack_1ce0);
                  __ZN8PMStringD1Ev(auStack_1ce0);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1ce4,&DAT_0015d370);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1d30,"",0);
                  FUN_00013588(uVar5,auStack_1ce4,auStack_1d30);
                  __ZN8PMStringD1Ev(auStack_1d30);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1d34,&DAT_0015d371);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1d80,"",0);
                  FUN_00013588(uVar5,auStack_1d34,auStack_1d80);
                  __ZN8PMStringD1Ev(auStack_1d80);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1d84,&DAT_0015d372);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1dd0,"",0);
                  FUN_00013588(uVar5,auStack_1d84,auStack_1dd0);
                  __ZN8PMStringD1Ev(auStack_1dd0);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1dd4,&DAT_0015d373);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1e20,"",0);
                  FUN_00013588(uVar5,auStack_1dd4,auStack_1e20);
                  __ZN8PMStringD1Ev(auStack_1e20);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1e24,&DAT_0015d37c);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1e70,"",0);
                  FUN_00013588(uVar5,auStack_1e24,auStack_1e70);
                  __ZN8PMStringD1Ev(auStack_1e70);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1e74,&DAT_0015d37d);
                  FUN_000193a8(uVar5,auStack_1e74);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1e78,&DAT_0015d374);
                  FUN_000193a8(uVar5,auStack_1e78);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1e7c,0x15d366);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1ec8,"",0);
                  FUN_00013588(uVar5,auStack_1e7c,auStack_1ec8);
                  __ZN8PMStringD1Ev(auStack_1ec8);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1ecc,0x15d357);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1f18,"",0);
                  FUN_00013588(uVar5,auStack_1ecc,auStack_1f18);
                  __ZN8PMStringD1Ev(auStack_1f18);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1f1c,0x15d369);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1f68,"",0);
                  FUN_00013588(uVar5,auStack_1f1c,auStack_1f68);
                  __ZN8PMStringD1Ev(auStack_1f68);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1f6c,0x15d365);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1fb8,"",0);
                  FUN_00013588(uVar5,auStack_1f6c,auStack_1fb8);
                  __ZN8PMStringD1Ev(auStack_1fb8);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_1fbc,0x15d368);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_2008,"",0);
                  FUN_00013588(uVar5,auStack_1fbc,auStack_2008);
                  __ZN8PMStringD1Ev(auStack_2008);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_200c,0x15d36b);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_2058,"",0);
                  FUN_00013588(uVar5,auStack_200c,auStack_2058);
                  __ZN8PMStringD1Ev(auStack_2058);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_205c,0x15d36d);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_20a8,"",0);
                  FUN_00013588(uVar5,auStack_205c,auStack_20a8);
                  __ZN8PMStringD1Ev(auStack_20a8);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_20ac,0x15d36f);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_20f8,"",0);
                  FUN_00013588(uVar5,auStack_20ac,auStack_20f8);
                  __ZN8PMStringD1Ev(auStack_20f8);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_20fc,&DAT_0015d376);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_2148,"",0);
                  FUN_00013588(uVar5,auStack_20fc,auStack_2148);
                  __ZN8PMStringD1Ev(auStack_2148);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_214c,&DAT_0015d377);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_2198,"",0);
                  FUN_00013588(uVar5,auStack_214c,auStack_2198);
                  __ZN8PMStringD1Ev(auStack_2198);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_219c,&DAT_0015d378);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_21e8,"",0);
                  FUN_00013588(uVar5,auStack_219c,auStack_21e8);
                  __ZN8PMStringD1Ev(auStack_21e8);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_21ec,&DAT_0015d379);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_2238,"",0);
                  FUN_00013588(uVar5,auStack_21ec,auStack_2238);
                  __ZN8PMStringD1Ev(auStack_2238);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_223c,&DAT_0015d37a);
                  FUN_000193a8(uVar5,auStack_223c);
                  plVar6 = (long *)FUN_00017c34(auStack_178);
                  (**(code **)(*plVar6 + 0x38))(plVar6,&DAT_00208c50);
                  plVar6 = (long *)FUN_00017c34(auStack_178);
                  (**(code **)(*plVar6 + 0x48))(plVar6,&DAT_00208c50);
                  plVar6 = (long *)FUN_000029d8(auStack_130);
                  FUN_00013490(0);
                  (**(code **)(*plVar6 + 0x240))(plVar6,auStack_2248);
                  plVar6 = (long *)FUN_000029d8(auStack_130);
                  FUN_00013490(0);
                  (**(code **)(*plVar6 + 0x250))(plVar6,auStack_2250);
                  plVar6 = (long *)FUN_000029d8(auStack_130);
                  FUN_00013490(0);
                  (**(code **)(*plVar6 + 0x260))(plVar6,auStack_2258);
                  plVar6 = (long *)FUN_000029d8(auStack_130);
                  FUN_00013490(0);
                  (**(code **)(*plVar6 + 0x270))(plVar6,auStack_2260);
                  plVar6 = (long *)FUN_000029d8(auStack_130);
                  FUN_00013490(0);
                  (**(code **)(*plVar6 + 0x280))(plVar6,auStack_2268);
                  plVar6 = (long *)FUN_000029d8(auStack_130);
                  FUN_00013490(0);
                  (**(code **)(*plVar6 + 0x290))(plVar6,auStack_2270);
                  plVar6 = (long *)FUN_000029d8(auStack_130);
                  FUN_00013490(0);
                  (**(code **)(*plVar6 + 0x2a0))(plVar6,auStack_2278);
                  plVar6 = (long *)FUN_000029d8(auStack_130);
                  FUN_00013490(0);
                  (**(code **)(*plVar6 + 0x2b0))(plVar6,auStack_2280);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_2284,0x15d303);
                  FUN_00011dc4(uVar5,auStack_2284,&DAT_00208c50);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_2288,0x15d30a);
                  FUN_00011dc4(uVar5,auStack_2288,&DAT_00208c52);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_228c,0x15d30a);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                            (auStack_22d8,"0x15d300kGCHideGridKey",0);
                  FUN_00013588(uVar5,auStack_228c,auStack_22d8);
                  __ZN8PMStringD1Ev(auStack_22d8);
                  uVar5 = local_30;
                  FUN_00002cf8(auStack_22dc,0x15d308);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_2328,"0x15d300kGCSaveKey",0);
                  FUN_00013588(uVar5,auStack_22dc,auStack_2328);
                  __ZN8PMStringD1Ev(auStack_2328);
                  FUN_000d20f0(uVar4,0);
                  local_124 = 0;
                }
                else {
                  local_124 = 2;
                }
                FUN_0000e324(auStack_14b8);
                __ZN8PMStringD1Ev(auStack_a80);
                __ZN8PMStringD1Ev(auStack_a38);
                __ZN8PMStringD1Ev(auStack_9e8);
                __ZN8PMStringD1Ev(auStack_6c8);
                __ZN8PMStringD1Ev(auStack_2a0);
              }
              else {
                local_124 = 2;
              }
              FUN_00032cd4(auStack_198);
            }
            else {
              local_124 = 2;
            }
            FUN_0000e040(auStack_188);
          }
          else {
            local_124 = 2;
          }
          FUN_00017ca4(auStack_178);
        }
        else {
          local_124 = 2;
        }
        FUN_0000e06c(auStack_158);
      }
      else {
        local_124 = 2;
      }
      FUN_0000e2f8(auStack_140);
    }
    else {
      local_124 = 2;
    }
    FUN_00002b9c(auStack_130);
  }
  else {
    local_124 = 2;
  }
  __ZN8PMStringD1Ev(auStack_78);
  return;
}

//==== FUNC @ 129a38 -> 00129a38

void FUN_00129a38(undefined8 param_1,undefined8 param_2,short *param_3,undefined8 param_4)

{
  bool bVar1;
  short sVar2;
  int iVar3;
  long lVar4;
  undefined8 uVar5;
  long *plVar6;
  ulong uVar7;
  undefined1 auVar8 [16];
  undefined1 auStack_db8 [12];
  undefined1 auStack_dac [4];
  undefined1 auStack_da8 [12];
  undefined1 auStack_d9c [4];
  undefined8 local_d98;
  undefined1 auStack_d8c [4];
  undefined1 auStack_d88 [76];
  undefined1 auStack_d3c [4];
  undefined8 local_d38;
  undefined1 auStack_d30 [4];
  undefined1 auStack_d2c [4];
  undefined1 auStack_d28 [12];
  undefined1 auStack_d1c [4];
  undefined8 local_d18;
  undefined1 auStack_d10 [12];
  undefined1 auStack_d04 [4];
  undefined8 local_d00;
  undefined1 auStack_cf8 [12];
  undefined1 auStack_cec [4];
  undefined8 local_ce8;
  byte local_cd9;
  undefined1 auStack_cd8 [72];
  undefined1 auStack_c90 [72];
  undefined1 auStack_c48 [4];
  undefined1 auStack_c44 [4];
  undefined1 auStack_c40 [4];
  undefined1 auStack_c3c [4];
  undefined1 auStack_c38 [4];
  undefined1 auStack_c34 [4];
  undefined1 auStack_c30 [4];
  undefined1 auStack_c2c [4];
  undefined1 auStack_c28 [4];
  undefined4 local_c24;
  undefined1 auStack_c20 [4];
  undefined1 auStack_c1c [4];
  undefined4 local_c18;
  undefined1 auStack_c14 [4];
  undefined1 auStack_c10 [12];
  undefined1 auStack_c04 [4];
  undefined8 local_c00;
  undefined1 auStack_bf4 [4];
  undefined4 local_bf0;
  undefined1 auStack_bec [4];
  undefined1 auStack_be8 [7];
  byte local_be1;
  undefined1 auStack_be0 [72];
  undefined1 auStack_b98 [12];
  undefined1 auStack_b8c [4];
  undefined8 local_b88;
  undefined1 auStack_b80 [12];
  undefined1 auStack_b74 [4];
  undefined8 local_b70;
  undefined1 auStack_b68 [4];
  int local_b64;
  undefined1 auStack_b60 [8];
  undefined1 auStack_b58 [8];
  undefined1 auStack_b50 [76];
  undefined1 auStack_b04 [4];
  undefined1 auStack_b00 [8];
  undefined8 local_af8;
  undefined8 local_af0;
  undefined8 local_ae8;
  undefined8 local_ae0;
  undefined8 local_ad8;
  undefined8 local_ad0;
  undefined1 auStack_ac8 [76];
  undefined1 auStack_a7c [4];
  undefined8 local_a78;
  undefined1 auStack_a70 [8];
  undefined1 auStack_a68 [76];
  undefined1 auStack_a1c [4];
  undefined8 local_a18;
  undefined1 auStack_a10 [8];
  undefined8 local_a08;
  undefined8 local_a00;
  undefined8 local_9f8;
  undefined8 local_9f0;
  undefined8 local_9e8;
  undefined8 local_9e0;
  undefined8 local_9d8;
  undefined8 local_9d0;
  undefined8 local_9c8;
  undefined1 auStack_9c0 [76];
  undefined1 auStack_974 [4];
  undefined8 local_970;
  undefined1 auStack_968 [8];
  undefined4 local_960;
  undefined1 auStack_95c [4];
  undefined1 auStack_958 [72];
  undefined1 auStack_910 [4];
  int local_90c;
  undefined1 auStack_908 [4];
  undefined1 auStack_904 [4];
  undefined1 auStack_900 [4];
  undefined1 auStack_8fc [4];
  undefined1 auStack_8f8 [4];
  undefined1 auStack_8f4 [4];
  undefined1 auStack_8f0 [4];
  undefined1 auStack_8ec [4];
  undefined1 auStack_8e8 [4];
  undefined4 local_8e4;
  undefined1 auStack_8e0 [4];
  undefined1 auStack_8dc [4];
  undefined4 local_8d8;
  undefined1 auStack_8d4 [4];
  undefined1 auStack_8d0 [12];
  undefined1 auStack_8c4 [4];
  undefined8 local_8c0;
  undefined1 auStack_8b4 [4];
  undefined4 local_8b0;
  undefined1 auStack_8ac [4];
  undefined1 auStack_8a8 [4];
  undefined1 auStack_8a4 [4];
  undefined1 auStack_8a0 [4];
  undefined1 auStack_89c [4];
  undefined1 auStack_898 [4];
  short local_894;
  byte local_891;
  undefined1 auStack_890 [72];
  undefined1 auStack_848 [12];
  undefined1 auStack_83c [4];
  undefined8 local_838;
  undefined1 auStack_830 [12];
  undefined1 auStack_824 [4];
  undefined8 local_820;
  undefined1 auStack_814 [4];
  int local_810;
  undefined1 auStack_80c [4];
  undefined1 auStack_808 [4];
  undefined4 local_804;
  undefined1 auStack_800 [4];
  undefined1 auStack_7fc [4];
  undefined1 auStack_7f8 [8];
  undefined1 auStack_7f0 [8];
  undefined1 auStack_7e8 [72];
  undefined1 auStack_7a0 [7];
  byte local_799;
  undefined1 auStack_798 [72];
  undefined1 auStack_750 [72];
  undefined1 auStack_708 [8];
  undefined1 auStack_700 [8];
  undefined1 auStack_6f8 [76];
  undefined1 auStack_6ac [4];
  undefined1 auStack_6a8 [8];
  undefined8 local_6a0;
  undefined8 local_698;
  undefined8 local_690;
  undefined1 auStack_684 [4];
  undefined8 local_680;
  undefined1 auStack_678 [8];
  undefined8 local_670;
  undefined8 local_668;
  undefined8 local_660;
  undefined1 auStack_658 [76];
  undefined1 auStack_60c [4];
  undefined8 local_608;
  undefined1 auStack_5fc [4];
  undefined1 auStack_5f8 [4];
  undefined4 local_5f4;
  undefined1 auStack_5f0 [4];
  undefined1 auStack_5ec [4];
  undefined1 auStack_5e8 [8];
  undefined1 auStack_5e0 [72];
  undefined1 auStack_598 [7];
  byte local_591;
  undefined1 auStack_590 [72];
  undefined1 auStack_548 [72];
  undefined1 auStack_500 [8];
  undefined1 auStack_4f8 [76];
  undefined1 auStack_4ac [4];
  undefined8 local_4a8;
  undefined1 auStack_4a0 [8];
  undefined8 local_498;
  undefined8 local_490;
  undefined8 local_488;
  undefined8 local_480;
  undefined8 local_478;
  undefined8 local_470;
  undefined8 local_468;
  undefined8 local_460;
  undefined8 local_458;
  undefined1 auStack_450 [76];
  undefined1 auStack_404 [4];
  undefined8 local_400;
  undefined1 auStack_3f8 [8];
  undefined4 local_3f0;
  undefined1 auStack_3ec [4];
  undefined1 auStack_3e8 [72];
  undefined1 auStack_3a0 [4];
  int local_39c;
  undefined8 local_398;
  undefined8 local_390;
  undefined8 local_388;
  undefined8 local_380;
  undefined8 local_378;
  undefined8 local_370;
  undefined8 local_368;
  undefined8 local_360;
  undefined8 local_358;
  byte local_349;
  undefined1 auStack_348 [76];
  undefined1 auStack_2fc [4];
  undefined1 auStack_2f8 [8];
  undefined1 auStack_2f0 [12];
  undefined1 auStack_2e4 [4];
  undefined1 auStack_2e0 [8];
  undefined1 auStack_2d8 [12];
  undefined1 auStack_2cc [4];
  undefined8 local_2c8;
  undefined1 auStack_2bc [4];
  undefined8 local_2b8;
  undefined1 auStack_2ac [4];
  undefined8 local_2a8;
  undefined1 auStack_29c [4];
  undefined8 local_298;
  undefined4 local_290;
  undefined1 auStack_28c [4];
  undefined1 auStack_288 [72];
  undefined1 auStack_240 [4];
  undefined1 auStack_23c [4];
  undefined1 auStack_238 [4];
  undefined1 auStack_234 [4];
  undefined1 auStack_230 [8];
  undefined8 local_228;
  undefined1 auStack_220 [4];
  undefined1 auStack_21c [4];
  undefined8 local_218;
  undefined8 local_210;
  undefined1 auStack_204 [4];
  undefined1 auStack_200 [8];
  undefined8 local_1f8;
  undefined1 auStack_1ec [4];
  undefined8 local_1e8;
  undefined1 auStack_1e0 [4];
  undefined1 auStack_1dc [4];
  undefined8 local_1d8;
  undefined8 local_1d0;
  undefined1 auStack_1c4 [4];
  undefined1 auStack_1c0 [8];
  undefined8 local_1b8;
  undefined1 auStack_1ac [4];
  undefined8 local_1a8;
  undefined1 auStack_1a0 [8];
  undefined8 local_198;
  undefined8 local_190;
  undefined1 auStack_184 [4];
  undefined8 local_180;
  undefined8 local_178;
  undefined8 local_170;
  undefined1 auStack_168 [8];
  int local_160;
  undefined1 auStack_15c [4];
  undefined8 local_158;
  undefined1 auStack_150 [6];
  short local_14a;
  undefined1 auStack_148 [6];
  short local_142;
  undefined1 auStack_140 [6];
  undefined2 local_13a;
  undefined1 auStack_138 [72];
  undefined4 local_f0;
  undefined1 auStack_ec [4];
  undefined1 auStack_e8 [72];
  undefined1 auStack_a0 [4];
  short local_9c;
  undefined1 uStack_99;
  undefined1 auStack_98 [8];
  undefined1 local_90 [16];
  undefined1 uStack_79;
  undefined1 auStack_78 [8];
  undefined1 auStack_70 [8];
  undefined1 auStack_68 [8];
  undefined4 local_60;
  undefined1 uStack_49;
  undefined1 auStack_48 [8];
  undefined8 local_40;
  short *local_38;
  undefined8 local_30;
  long local_28;
  
  auVar8 = (*(code *)PTR____chkstk_darwin_002340e0)();
  local_30 = auVar8._8_8_;
  lVar4 = auVar8._0_8_;
  local_40 = param_4;
  local_38 = param_3;
  local_28 = lVar4;
  FUN_00002bf4(auStack_48,lVar4,&uStack_49);
  sVar2 = FUN_00002c30(auStack_48);
  if (sVar2 != 0) {
    local_60 = 2;
    goto LAB_0012cb84;
  }
  FUN_001a2484(auStack_68);
  sVar2 = FUN_0000e2a4(auStack_68);
  if (sVar2 == 0) {
    FUN_001a7c08(auStack_70);
    sVar2 = FUN_0000e280(auStack_70);
    if (sVar2 == 0) {
      uVar5 = __Z26GetExecutionContextSessionv();
      FUN_00002978(auStack_78,uVar5,&uStack_79);
      sVar2 = FUN_000029b4(auStack_78);
      if (sVar2 == 0) {
        plVar6 = (long *)FUN_000029d8(auStack_78);
        local_90 = (**(code **)(*plVar6 + 0x28))();
        FUN_00017bd4(auStack_98,local_90,&uStack_99);
        sVar2 = FUN_00017c10(auStack_98);
        if (sVar2 == 0) {
          FUN_00002cf8(auStack_a0,0x15d4c5);
          local_9c = FUN_00012ef4(auStack_48,auStack_a0);
          FUN_00002cf8(auStack_ec,0x15d309);
          local_f0 = 0xffffffff;
          FUN_00012c10(auStack_e8,auStack_48,auStack_ec,&local_f0);
          __ZN8PMStringC1ERKS_(auStack_138,auStack_e8);
          __ZN8PMString6AppendEPKciNS_14StringEncodingE
                    (auStack_138," Image-lines",0x7fffffff,0xffffffff);
          FUN_00002cf8(auStack_140,0x15d4a4);
          local_13a = FUN_00012ef4(auStack_48,auStack_140);
          FUN_00002cf8(auStack_148,0x15d4a3);
          local_142 = FUN_00012ef4(auStack_48,auStack_148);
          FUN_00002cf8(auStack_150,0x15d323);
          local_14a = FUN_00012ef4(auStack_48,auStack_150);
          FUN_00002cf8(auStack_15c,0x15d31d);
          local_158 = FUN_00013338(auStack_48,auStack_15c);
          plVar6 = (long *)FUN_000029d8(auStack_78);
          local_160 = (**(code **)(*plVar6 + 0xf8))();
          FUN_00013490(0);
          sVar2 = FUN_00014140(&local_158,auStack_168);
          if (sVar2 == 0 || local_160 < 1) {
            FUN_00002cf8(auStack_d9c,0x15d49a);
            FUN_00013490(0);
            FUN_00013c20(auStack_48,auStack_d9c,auStack_da8);
            FUN_00002cf8(auStack_dac,0x15d49c);
            FUN_00013490(0);
            FUN_00013c20(auStack_48,auStack_dac,auStack_db8);
          }
          else {
            local_170 = local_158;
            plVar6 = (long *)FUN_00017c34(auStack_98);
            local_178 = (**(code **)(*plVar6 + 0x2c0))();
            FUN_00002cf8(auStack_184,0x15d3a8);
            local_180 = FUN_00013338(auStack_48,auStack_184);
            local_190 = FUN_00013338(auStack_48,local_30);
            FUN_00013490((double)(long)local_160);
            local_198 = FUN_00028080(auStack_1a0,&local_190);
            if (local_9c != 0) {
              plVar6 = (long *)FUN_000029d8(auStack_78);
              local_1a8 = (**(code **)(*plVar6 + 0x1d8))();
              uVar5 = local_30;
              FUN_00002cf8(auStack_1ac,0x15d49a);
              uVar7 = FUN_0001417c(uVar5,auStack_1ac);
              uVar5 = local_30;
              if ((uVar7 & 1) == 0) {
                FUN_00002cf8(auStack_1ec,0x15d49c);
                uVar7 = FUN_0001417c(uVar5,auStack_1ec);
                if ((uVar7 & 1) != 0) {
                  local_1f8 = FUN_00028080(&local_1a8,&local_190);
                  FUN_00013490(0);
                  sVar2 = FUN_00015808(&local_1f8,auStack_200);
                  if (sVar2 == 0) {
                    FUN_00002cf8(auStack_21c,0x15d49a);
                    local_218 = FUN_00013338(auStack_48,auStack_21c);
                    FUN_00002cf8(auStack_220,0x15d49c);
                    local_228 = FUN_00028080(&local_1a8,&local_218);
                    FUN_00013c20(auStack_48,auStack_220,&local_228);
                  }
                  else {
                    FUN_00002cf8(auStack_204,0x15d49a);
                    local_210 = FUN_00028080(&local_1a8,&local_190);
                    FUN_00013c20(auStack_48,auStack_204,&local_210);
                  }
                }
              }
              else {
                local_1b8 = FUN_00028080(&local_1a8,&local_190);
                FUN_00013490(0);
                sVar2 = FUN_00015808(&local_1b8,auStack_1c0);
                if (sVar2 == 0) {
                  FUN_00002cf8(auStack_1dc,0x15d49c);
                  local_1d8 = FUN_00013338(auStack_48,auStack_1dc);
                  FUN_00002cf8(auStack_1e0,0x15d49a);
                  local_1e8 = FUN_00028080(&local_1a8,&local_1d8);
                  FUN_00013c20(auStack_48,auStack_1e0,&local_1e8);
                }
                else {
                  FUN_00002cf8(auStack_1c4,0x15d49c);
                  local_1d0 = FUN_00028080(&local_1a8,&local_190);
                  FUN_00013c20(auStack_48,auStack_1c4,&local_1d0);
                }
              }
            }
            FUN_00013490(0);
            sVar2 = FUN_00015808(&local_198,auStack_230);
            uVar5 = local_30;
            if (sVar2 != 0) {
              FUN_00002cf8(auStack_234,0x15d49a);
              uVar7 = FUN_0001417c(uVar5,auStack_234);
              uVar5 = local_30;
              if ((uVar7 & 1) == 0) {
                FUN_00002cf8(auStack_23c,0x15d49c);
                uVar7 = FUN_0001417c(uVar5,auStack_23c);
                if ((uVar7 & 1) != 0) {
                  FUN_00002cf8(auStack_240,0x15d49a);
                  FUN_000196d0(auStack_48,auStack_240,&local_198);
                }
              }
              else {
                FUN_00002cf8(auStack_238,0x15d49c);
                FUN_000196d0(auStack_48,auStack_238,&local_198);
              }
            }
            FUN_0015d6f4(lVar4);
            FUN_00163ee0(lVar4,local_40);
            FUN_0016599c(lVar4);
            FUN_00166204(lVar4);
            FUN_00002cf8(auStack_28c,0x15d306);
            local_290 = 0xffffffff;
            FUN_00012c10(auStack_288,auStack_48,auStack_28c,&local_290);
            FUN_00002cf8(auStack_29c,0x15d49a);
            local_298 = FUN_00013338(auStack_48,auStack_29c);
            FUN_00002cf8(auStack_2ac,0x15d49c);
            local_2a8 = FUN_00013338(auStack_48,auStack_2ac);
            FUN_00002cf8(auStack_2bc,0x15d49b);
            local_2b8 = FUN_00013338(auStack_48,auStack_2bc);
            FUN_00002cf8(auStack_2cc,0x15d49d);
            local_2c8 = FUN_00013338(auStack_48,auStack_2cc);
            FUN_00013490(0);
            sVar2 = FUN_00014140(&local_298,auStack_2d8);
            if (sVar2 == 0) {
              FUN_00013490(0);
              sVar2 = FUN_00014140(&local_2a8,auStack_2e0);
              bVar1 = false;
              if (sVar2 != 0) goto LAB_0012a564;
            }
            else {
LAB_0012a564:
              bVar1 = local_9c == 0;
            }
            if (bVar1) {
              FUN_00002cf8(auStack_2e4,0x15d4c5);
              FUN_00011dc4(auStack_48,auStack_2e4,&DAT_00208c52);
            }
            else {
              FUN_00013490(0);
              sVar2 = FUN_000132fc(&local_298,auStack_2f0);
              bVar1 = false;
              if (sVar2 != 0) {
                FUN_00013490(0);
                sVar2 = FUN_000132fc(&local_2a8,auStack_2f8);
                bVar1 = sVar2 != 0;
              }
              if (bVar1) {
                FUN_00002cf8(auStack_2fc,0x15d4c5);
                FUN_00011dc4(auStack_48,auStack_2fc,&DAT_00208c50);
              }
            }
            local_349 = 0;
            bVar1 = false;
            if ((local_142 != 0) && (bVar1 = false, local_14a != 0)) {
              __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_348,"[GC-1]",0);
              local_349 = 1;
              sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_e8,auStack_348,1,0);
              bVar1 = sVar2 != 0;
            }
            if ((local_349 & 1) != 0) {
              __ZN8PMStringD1Ev(auStack_348);
            }
            if (bVar1) {
              FUN_0015d6f4(lVar4);
              plVar6 = (long *)FUN_000029d8(auStack_78);
              local_358 = (**(code **)(*plVar6 + 0x1b8))();
              local_378 = FUN_000157c8(&local_2b8,lVar4 + 0x88);
              local_370 = FUN_00028080(&local_378,&local_358);
              local_368 = FUN_000157c8(&local_370,&local_170);
              local_360 = FUN_00015abc(&local_368);
              local_298 = local_360;
              local_390 = FUN_000157c8(&local_2c8,lVar4 + 0x88);
              local_388 = FUN_000157c8(&local_390,&local_170);
              local_380 = FUN_00015abc(&local_388);
              local_2a8 = local_380;
            }
            FUN_00015a50(&local_398);
            if (*(short *)(lVar4 + 0x90) != 0) {
              FUN_00002cf8(auStack_3a0,0x15d4c9);
              local_39c = FUN_00013034(auStack_48,auStack_3a0);
              FUN_00002cf8(auStack_3ec,0x15d4c9);
              local_3f0 = 0xffffffff;
              FUN_00012c10(auStack_3e8,auStack_48,auStack_3ec,&local_3f0);
              FUN_00013490(0);
              sVar2 = FUN_00018504(&local_178,auStack_3f8);
              if (sVar2 == 0) {
                if ((local_142 == 0) || (local_14a == 0)) {
                  FUN_00013490((double)(long)local_160);
                  local_6a0 = FUN_00028080(auStack_6a8,&local_298);
                  local_698 = FUN_00028080(&local_6a0,&local_2a8);
                  local_690 = FUN_00015a7c(&local_698,&local_170);
                  local_398 = local_690;
                }
                else {
                  FUN_00002cf8(auStack_60c,0x15d4a9);
                  local_608 = FUN_0001544c(auStack_48,auStack_60c,auStack_288);
                  local_398 = local_608;
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_658,"[GC-1]",0);
                  sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_e8,auStack_658,1,0);
                  __ZN8PMStringD1Ev(auStack_658);
                  if (sVar2 != 0) {
                    local_668 = FUN_000157c8(&local_398,&local_170);
                    local_660 = FUN_00015abc(&local_668);
                    local_670 = FUN_00015a7c(&local_660,&local_170);
                    local_398 = local_670;
                  }
                  FUN_00013490(0x3f50624dd2f1a9fc);
                  sVar2 = FUN_00031d38(&local_398,auStack_678);
                  if (sVar2 != 0) {
                    FUN_00002cf8(auStack_684,&DAT_0015d31b);
                    local_680 = FUN_0001544c(auStack_48,auStack_684,auStack_288);
                    local_398 = local_680;
                  }
                }
                FUN_00002cf8(auStack_6ac,0x15d4c9);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_6f8,"rows",0);
                FUN_00013490();
                FUN_00013490(0);
                FUN_001668f4(lVar4,auStack_6ac,auStack_6f8,&local_398,&local_170,auStack_700,
                             auStack_708);
                __ZN8PMStringD1Ev(auStack_6f8);
                local_799 = 0;
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_750,"[GC] Smart Setup");
                sVar2 = FUN_001a6604(auStack_750);
                bVar1 = true;
                if (sVar2 == 0) {
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_798,"[GC] CGS",0);
                  local_799 = 1;
                  sVar2 = FUN_001a6604(auStack_798);
                  bVar1 = sVar2 != 0;
                }
                if ((local_799 & 1) != 0) {
                  __ZN8PMStringD1Ev(auStack_798);
                }
                __ZN8PMStringD1Ev(auStack_750);
                if (bVar1) {
                  FUN_00002cf8(auStack_7a0,0x15d37a);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_7e8,"rows",0);
                  FUN_00013490();
                  FUN_00013490(0);
                  FUN_001668f4(lVar4,auStack_7a0,auStack_7e8,&local_398,&local_170,auStack_7f0,
                               auStack_7f8);
                  __ZN8PMStringD1Ev(auStack_7e8);
                  FUN_00002cf8(auStack_7fc,0x15d37a);
                  iVar3 = FUN_0001853c(auStack_48,auStack_7fc);
                  if (iVar3 < 1) {
                    FUN_00002cf8(auStack_808,0x15d37a);
                    FUN_00011dc4(auStack_48,auStack_808,&DAT_00208c50);
                    FUN_00002cf8(auStack_80c,&DAT_0015d37b);
                    FUN_00011dc4(auStack_48,auStack_80c,&DAT_00208c50);
                  }
                  else {
                    FUN_00002cf8(auStack_800,0x15d37a);
                    local_804 = 0;
                    FUN_00015b04(auStack_48,auStack_800,&local_804,1);
                  }
                }
              }
              else {
                if ((local_142 == 0) || (local_14a == 0)) {
                  FUN_00013490((double)(long)local_160);
                  local_498 = FUN_00028080(auStack_4a0,&local_298);
                  local_490 = FUN_00028080(&local_498,&local_2a8);
                  local_488 = FUN_00015a7c(&local_490,&local_170);
                  local_480 = FUN_00028080(&local_488,&local_178);
                  local_398 = local_480;
                }
                else {
                  FUN_00002cf8(auStack_404,0x15d4a9);
                  local_400 = FUN_0001544c(auStack_48,auStack_404,auStack_288);
                  local_398 = local_400;
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_450,"[GC-1]",0);
                  sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_e8,auStack_450,1,0);
                  __ZN8PMStringD1Ev(auStack_450);
                  if (sVar2 != 0) {
                    local_468 = FUN_00017c4c(&local_398,&local_178);
                    local_460 = FUN_000157c8(&local_468,&local_170);
                    local_458 = FUN_00015abc(&local_460);
                    local_478 = FUN_00015a7c(&local_458,&local_170);
                    local_470 = FUN_00028080(&local_478,&local_178);
                    local_398 = local_470;
                  }
                }
                local_4a8 = FUN_00028080(&local_170,&local_178);
                FUN_00002cf8(auStack_4ac,0x15d4c9);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_4f8,"rows",0);
                FUN_00013490(0);
                FUN_001668f4(lVar4,auStack_4ac,auStack_4f8,&local_398,&local_170,&local_4a8,
                             auStack_500);
                __ZN8PMStringD1Ev(auStack_4f8);
                local_591 = 0;
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_548,"[GC] Smart Setup");
                sVar2 = FUN_001a6604(auStack_548);
                bVar1 = true;
                if (sVar2 == 0) {
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_590,"[GC] CGS",0);
                  local_591 = 1;
                  sVar2 = FUN_001a6604(auStack_590);
                  bVar1 = sVar2 != 0;
                }
                if ((local_591 & 1) != 0) {
                  __ZN8PMStringD1Ev(auStack_590);
                }
                __ZN8PMStringD1Ev(auStack_548);
                if (bVar1) {
                  FUN_00002cf8(auStack_598,0x15d37a);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_5e0,"rows",0);
                  FUN_00013490(0);
                  FUN_001668f4(lVar4,auStack_598,auStack_5e0,&local_398,&local_170,&local_4a8,
                               auStack_5e8);
                  __ZN8PMStringD1Ev(auStack_5e0);
                  FUN_00002cf8(auStack_5ec,0x15d37a);
                  iVar3 = FUN_0001853c(auStack_48,auStack_5ec);
                  if (iVar3 < 1) {
                    FUN_00002cf8(auStack_5f8,0x15d37a);
                    FUN_00011dc4(auStack_48,auStack_5f8,&DAT_00208c50);
                    FUN_00002cf8(auStack_5fc,&DAT_0015d37b);
                    FUN_00011dc4(auStack_48,auStack_5fc,&DAT_00208c50);
                  }
                  else {
                    FUN_00002cf8(auStack_5f0,0x15d37a);
                    local_5f4 = 0;
                    FUN_00015b04(auStack_48,auStack_5f0,&local_5f4,1);
                  }
                }
              }
              FUN_00002cf8(auStack_814,0x15d4c9);
              local_810 = FUN_0001853c(auStack_48,auStack_814);
              if (local_810 < 1) {
                FUN_00002cf8(auStack_8fc,0x15d4c9);
                FUN_00011dc4(auStack_48,auStack_8fc,&DAT_00208c50);
                FUN_00002cf8(auStack_900,0x15d4d5);
                FUN_00011dc4(auStack_48,auStack_900,&DAT_00208c50);
                FUN_00002cf8(auStack_904,0x15d4ca);
                FUN_00011dc4(auStack_48,auStack_904,&DAT_00208c50);
                FUN_00002cf8(auStack_908,0x15d4cb);
                FUN_00011dc4(auStack_48,auStack_908,&DAT_00208c50);
              }
              else {
                FUN_00002cf8(auStack_824,0x15d369);
                local_820 = FUN_00013338(auStack_48,auStack_824);
                FUN_00013490(0);
                sVar2 = FUN_00014140(&local_820,auStack_830);
                bVar1 = true;
                if (sVar2 == 0) {
                  FUN_00002cf8(auStack_83c,0x15d4b3);
                  local_838 = FUN_00013338(auStack_48,auStack_83c);
                  FUN_00013490(0);
                  sVar2 = FUN_00014140(&local_838,auStack_848);
                  bVar1 = sVar2 != 0;
                }
                if (bVar1) {
                  local_891 = 0;
                  bVar1 = false;
                  if (-1 < local_39c) {
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                              (auStack_890,"Custom Setting Applied",0);
                    local_891 = 1;
                    sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_3e8,auStack_890,1,0);
                    bVar1 = sVar2 == 0;
                  }
                  if ((local_891 & 1) != 0) {
                    __ZN8PMStringD1Ev(auStack_890);
                  }
                  if (bVar1) {
                    FUN_00002cf8(auStack_898,&DAT_0015d312);
                    local_894 = FUN_00012ef4(auStack_48,auStack_898);
                    if ((local_9c == 0) || (*local_38 != 0)) {
                      if ((local_9c == 0) && ((*local_38 == 0 && (local_894 != 0)))) {
                        sVar2 = FUN_001a6604(auStack_138);
                        if (sVar2 != 0) {
                          FUN_00002cf8(auStack_8a4,0x15d4c9);
                          FUN_00015b04(auStack_48,auStack_8a4,&local_39c,1);
                          FUN_00002cf8(auStack_8a8,0x15d4c9);
                          FUN_0010b924(lVar4,auStack_8a8,&DAT_00208c52);
                          goto LAB_0012b9f8;
                        }
                      }
                      FUN_00002cf8(auStack_8ac,0x15d4c9);
                      local_8b0 = 0;
                      FUN_00015b04(auStack_48,auStack_8ac,&local_8b0,1);
                      FUN_00002cf8(auStack_8b4,0x15d4c9);
                      FUN_0010b924(lVar4,auStack_8b4,&DAT_00208c52);
                      FUN_0010df8c(lVar4);
                    }
                    else {
                      FUN_00002cf8(auStack_89c,0x15d4c9);
                      FUN_00015b04(auStack_48,auStack_89c,&local_39c,1);
                      FUN_00002cf8(auStack_8a0,0x15d4c9);
                      FUN_0010b924(lVar4,auStack_8a0,&DAT_00208c52);
                    }
                  }
                  else {
                    FUN_00002cf8(auStack_8c4,0x15d4b3);
                    local_8c0 = FUN_00013338(auStack_48,auStack_8c4);
                    FUN_00013490(0);
                    sVar2 = FUN_00014140(&local_8c0,auStack_8d0);
                    if (sVar2 == 0) {
                      FUN_00002cf8(auStack_8d4,0x15d4c9);
                      local_8d8 = 0;
                      FUN_00015b04(auStack_48,auStack_8d4,&local_8d8,1);
                      FUN_00002cf8(auStack_8dc,0x15d4c9);
                      FUN_0010b924(lVar4,auStack_8dc,&DAT_00208c50);
                    }
                    else {
                      FUN_0013354c(lVar4,&DAT_00208c52);
                      FUN_0013491c(lVar4);
                    }
                  }
                }
                else {
                  FUN_00002cf8(auStack_8e0,0x15d4c9);
                  local_8e4 = 0;
                  FUN_00015b04(auStack_48,auStack_8e0,&local_8e4,1);
                  FUN_00002cf8(auStack_8e8,0x15d4c9);
                  FUN_0010b924(lVar4,auStack_8e8,&DAT_00208c50);
                  FUN_0010df8c(lVar4);
                }
LAB_0012b9f8:
                FUN_00002cf8(auStack_8ec,0x15d4c9);
                FUN_00011dc4(auStack_48,auStack_8ec,&DAT_00208c52);
                FUN_00002cf8(auStack_8f0,0x15d4d5);
                FUN_00011dc4(auStack_48,auStack_8f0,&DAT_00208c52);
                FUN_00002cf8(auStack_8f4,0x15d4ca);
                FUN_00011dc4(auStack_48,auStack_8f4,&DAT_00208c52);
                FUN_00002cf8(auStack_8f8,0x15d4cb);
                FUN_00011dc4(auStack_48,auStack_8f8,&DAT_00208c52);
              }
              __ZN8PMStringD1Ev(auStack_3e8);
            }
            if (*(short *)(lVar4 + 0x94) != 0) {
              FUN_00002cf8(auStack_910,0x15d4cf);
              local_90c = FUN_00013034(auStack_48,auStack_910);
              FUN_00002cf8(auStack_95c,0x15d4cf);
              local_960 = 0xffffffff;
              FUN_00012c10(auStack_958,auStack_48,auStack_95c,&local_960);
              FUN_00013490(0);
              sVar2 = FUN_00018504(&local_178,auStack_968);
              if (sVar2 == 0) {
                if ((local_142 == 0) || (local_14a == 0)) {
                  FUN_00013490((double)(long)local_160);
                  local_af8 = FUN_00028080(auStack_b00,&local_298);
                  local_af0 = FUN_00028080(&local_af8,&local_2a8);
                  local_ae8 = FUN_00015a7c(&local_af0,&local_170);
                  local_398 = local_ae8;
                }
                else {
                  FUN_00002cf8(auStack_a7c,0x15d4a9);
                  local_a78 = FUN_0001544c(auStack_48,auStack_a7c,auStack_288);
                  local_398 = local_a78;
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_ac8,"[GC-1]",0);
                  sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_e8,auStack_ac8,1,0);
                  __ZN8PMStringD1Ev(auStack_ac8);
                  if (sVar2 != 0) {
                    local_ad8 = FUN_000157c8(&local_398,&local_170);
                    local_ad0 = FUN_00015abc(&local_ad8);
                    local_ae0 = FUN_00015a7c(&local_ad0,&local_170);
                    local_398 = local_ae0;
                  }
                }
                FUN_00002cf8(auStack_b04,0x15d4cf);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_b50,"rows",0);
                FUN_00013490();
                FUN_00013490(0);
                FUN_001668f4(lVar4,auStack_b04,auStack_b50,&local_398,&local_170,auStack_b58,
                             auStack_b60);
                __ZN8PMStringD1Ev(auStack_b50);
              }
              else {
                if ((local_142 == 0) || (local_14a == 0)) {
                  FUN_00013490((double)(long)local_160);
                  local_a08 = FUN_00028080(auStack_a10,&local_298);
                  local_a00 = FUN_00028080(&local_a08,&local_2a8);
                  local_9f8 = FUN_00015a7c(&local_a00,&local_170);
                  local_9f0 = FUN_00028080(&local_9f8,&local_178);
                  local_398 = local_9f0;
                }
                else {
                  FUN_00002cf8(auStack_974,0x15d4a9);
                  local_970 = FUN_0001544c(auStack_48,auStack_974,auStack_288);
                  local_398 = local_970;
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_9c0,"[GC-1]",0);
                  sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_e8,auStack_9c0,1,0);
                  __ZN8PMStringD1Ev(auStack_9c0);
                  if (sVar2 != 0) {
                    local_9d8 = FUN_00017c4c(&local_398,&local_178);
                    local_9d0 = FUN_000157c8(&local_9d8,&local_170);
                    local_9c8 = FUN_00015abc(&local_9d0);
                    local_9e8 = FUN_00015a7c(&local_9c8,&local_170);
                    local_9e0 = FUN_00028080(&local_9e8,&local_178);
                    local_398 = local_9e0;
                  }
                }
                local_a18 = FUN_00028080(&local_170,&local_178);
                FUN_00002cf8(auStack_a1c,0x15d4cf);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_a68,"rows",0);
                FUN_00013490(0);
                FUN_001668f4(lVar4,auStack_a1c,auStack_a68,&local_398,&local_170,&local_a18,
                             auStack_a70);
                __ZN8PMStringD1Ev(auStack_a68);
              }
              FUN_00002cf8(auStack_b68,0x15d4cf);
              local_b64 = FUN_0001853c(auStack_48,auStack_b68);
              if (local_b64 < 1) {
                FUN_00002cf8(auStack_c3c,0x15d4cf);
                FUN_00011dc4(auStack_48,auStack_c3c,&DAT_00208c50);
                FUN_00002cf8(auStack_c40,0x15d4d6);
                FUN_00011dc4(auStack_48,auStack_c40,&DAT_00208c50);
                FUN_00002cf8(auStack_c44,0x15d4d0);
                FUN_00011dc4(auStack_48,auStack_c44,&DAT_00208c50);
                FUN_00002cf8(auStack_c48,0x15d4d1);
                FUN_00011dc4(auStack_48,auStack_c48,&DAT_00208c50);
              }
              else {
                FUN_00002cf8(auStack_b74,0x15d369);
                local_b70 = FUN_00013338(auStack_48,auStack_b74);
                FUN_00013490(0);
                sVar2 = FUN_00014140(&local_b70,auStack_b80);
                bVar1 = true;
                if (sVar2 == 0) {
                  FUN_00002cf8(auStack_b8c,0x15d4bb);
                  local_b88 = FUN_00013338(auStack_48,auStack_b8c);
                  FUN_00013490(0);
                  sVar2 = FUN_00014140(&local_b88,auStack_b98);
                  bVar1 = sVar2 != 0;
                }
                if (bVar1) {
                  local_be1 = 0;
                  bVar1 = false;
                  if (-1 < local_90c) {
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                              (auStack_be0,"Custom Setting Applied",0);
                    local_be1 = 1;
                    sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_958,auStack_be0,1,0);
                    bVar1 = sVar2 == 0;
                  }
                  if ((local_be1 & 1) != 0) {
                    __ZN8PMStringD1Ev(auStack_be0);
                  }
                  if (bVar1) {
                    if (local_9c == 0) {
                      FUN_00002cf8(auStack_bec,0x15d4cf);
                      local_bf0 = 0;
                      FUN_00015b04(auStack_48,auStack_bec,&local_bf0,1);
                    }
                    else {
                      FUN_00002cf8(auStack_be8,0x15d4cf);
                      FUN_00015b04(auStack_48,auStack_be8,&local_90c,1);
                    }
                    FUN_00002cf8(auStack_bf4,0x15d4cf);
                    FUN_0010b924(lVar4,auStack_bf4,&DAT_00208c50);
                  }
                  else {
                    FUN_00002cf8(auStack_c04,0x15d4bb);
                    local_c00 = FUN_00013338(auStack_48,auStack_c04);
                    FUN_00013490(0);
                    sVar2 = FUN_00014140(&local_c00,auStack_c10);
                    if (sVar2 == 0) {
                      FUN_00002cf8(auStack_c14,0x15d4cf);
                      local_c18 = 0;
                      FUN_00015b04(auStack_48,auStack_c14,&local_c18,1);
                      FUN_00002cf8(auStack_c1c,0x15d4cf);
                      FUN_0010b924(lVar4,auStack_c1c,&DAT_00208c50);
                    }
                    else {
                      FUN_00135674(lVar4);
                    }
                  }
                }
                else {
                  FUN_00002cf8(auStack_c20,0x15d4cf);
                  local_c24 = 0;
                  FUN_00015b04(auStack_48,auStack_c20,&local_c24,1);
                  FUN_00002cf8(auStack_c28,0x15d4cf);
                  FUN_0010b924(lVar4,auStack_c28,&DAT_00208c50);
                }
                FUN_00002cf8(auStack_c2c,0x15d4cf);
                FUN_00011dc4(auStack_48,auStack_c2c,&DAT_00208c52);
                FUN_00002cf8(auStack_c30,0x15d4d6);
                FUN_00011dc4(auStack_48,auStack_c30,&DAT_00208c52);
                FUN_00002cf8(auStack_c34,0x15d4d0);
                FUN_00011dc4(auStack_48,auStack_c34,&DAT_00208c52);
                FUN_00002cf8(auStack_c38,0x15d4d1);
                FUN_00011dc4(auStack_48,auStack_c38,&DAT_00208c52);
              }
              __ZN8PMStringD1Ev(auStack_958);
            }
            local_cd9 = 0;
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_c90,"[GC] Smart Setup");
            sVar2 = FUN_001a6604(auStack_c90);
            if (sVar2 == 0) {
              __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_cd8,"[GC] CGS",0);
              local_cd9 = 1;
              sVar2 = FUN_001a6604(auStack_cd8);
              bVar1 = false;
              if (sVar2 != 0) goto LAB_0012c738;
            }
            else {
LAB_0012c738:
              FUN_00002cf8(auStack_cec,&DAT_0015d376);
              local_ce8 = FUN_00013338(auStack_48,auStack_cec);
              FUN_00013490(0);
              sVar2 = FUN_00031d38(&local_ce8,auStack_cf8);
              bVar1 = false;
              if (sVar2 != 0) {
                FUN_00002cf8(auStack_d04,&DAT_0015d378);
                local_d00 = FUN_00013338(auStack_48,auStack_d04);
                FUN_00013490(0);
                sVar2 = FUN_00031d38(&local_d00,auStack_d10);
                bVar1 = sVar2 != 0;
              }
            }
            if ((local_cd9 & 1) != 0) {
              __ZN8PMStringD1Ev(auStack_cd8);
            }
            __ZN8PMStringD1Ev(auStack_c90);
            if (bVar1) {
              FUN_00002cf8(auStack_d1c,0x15d365);
              local_d18 = FUN_00013338(auStack_48,auStack_d1c);
              FUN_00013490(0);
              sVar2 = FUN_00018504(&local_178,auStack_d28);
              if (sVar2 == 0) {
                FUN_00002cf8(auStack_d30,0x15d36d);
                local_d38 = FUN_00015a7c(&local_298,&local_d18);
                FUN_00013c20(auStack_48,auStack_d30,&local_d38);
              }
              else {
                FUN_00002cf8(auStack_d2c,0x15d36d);
                FUN_00013c20(auStack_48,auStack_d2c,&local_2b8);
              }
              FUN_00002cf8(auStack_d3c,&DAT_0015d377);
              __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_d88,"",0);
              FUN_00013588(auStack_48,auStack_d3c,auStack_d88);
              __ZN8PMStringD1Ev(auStack_d88);
              FUN_00002cf8(auStack_d8c,0x15d36f);
              local_d98 = FUN_00015a7c(&local_2a8,&local_d18);
              FUN_00013c20(auStack_48,auStack_d8c,&local_d98);
              plVar6 = (long *)FUN_000029d8(auStack_78);
              (**(code **)(*plVar6 + 0x2a0))(plVar6,&local_298);
              plVar6 = (long *)FUN_000029d8(auStack_78);
              (**(code **)(*plVar6 + 0x2b0))(plVar6,&local_2a8);
            }
            __ZN8PMStringD1Ev(auStack_288);
          }
          FUN_001c0ee4(auStack_48);
          plVar6 = (long *)FUN_0000e2c8(auStack_68);
          sVar2 = (**(code **)(*plVar6 + 0xe8))();
          if (sVar2 != 0) {
            FUN_001c4978(auStack_48);
          }
          __ZN8PMStringD1Ev(auStack_138);
          __ZN8PMStringD1Ev(auStack_e8);
          local_60 = 0;
        }
        else {
          local_60 = 2;
        }
        FUN_00017ca4(auStack_98);
      }
      else {
        local_60 = 2;
      }
      FUN_00002b9c(auStack_78);
    }
    else {
      local_60 = 2;
    }
    FUN_0000e324(auStack_70);
  }
  else {
    local_60 = 2;
  }
  FUN_0000e2f8(auStack_68);
LAB_0012cb84:
  FUN_00002c54(auStack_48);
  return;
}
