/*
Margins in line mode (row 06). FUN_00109e04, FUN_001773a4 and FUN_0017e768: k = Round(m / u), applied m' = k * u,
the paired form Round((m_a + m_b) / u) * u - m_b; FUN_001773a4 also builds the type-area span T for the Smart list.

PMReal helpers: construct 0x13490, operator/ 0x157c8, operator* 0x15a7c, operator+ 0x17c4c, operator- 0x28080,
Round 0x15abc (floor(x + 0.5)), floor 0x37d70, a > b + 1e-8 0x14140, a >= b - 1e-8 0x15808, a <= b + 1e-8 0x31d38,
a < b - 1e-8 0x3208c, |a - b| < 1e-8 0x132fc, not equal 0x18504, quantise 0x152f0 (mode 3 three decimals, mode 4 four).
Widget access: read value 0x13338, read value in the panel unit 0x1544c, read checkbox 0x12ef4, read dropdown index 0x13034,
write value 0x13c20, set maximum 0x196d0, unit factor at object + 0x88.
*/

//==== FUNC @ 10a010 -> 00109e04

void FUN_00109e04(undefined8 param_1)

{
  bool bVar1;
  short sVar2;
  undefined8 uVar3;
  long *plVar4;
  undefined1 auStack_1e8 [4];
  undefined1 auStack_1e4 [4];
  undefined1 auStack_1e0 [8];
  undefined1 auStack_1d8 [12];
  undefined1 auStack_1cc [4];
  undefined4 local_1c8;
  undefined1 auStack_1c4 [4];
  undefined1 auStack_1c0 [12];
  undefined1 auStack_1b4 [4];
  undefined4 local_1b0;
  undefined1 auStack_1ac [4];
  undefined1 auStack_1a8 [8];
  undefined1 auStack_1a0 [4];
  undefined1 auStack_19c [4];
  undefined8 local_198;
  undefined8 local_190;
  undefined8 local_188;
  undefined1 auStack_180 [72];
  undefined1 auStack_138 [4];
  undefined1 auStack_134 [4];
  undefined8 local_130;
  undefined1 auStack_128 [72];
  undefined1 auStack_e0 [4];
  undefined1 auStack_dc [4];
  undefined8 local_d8;
  undefined8 local_d0;
  undefined8 local_c8;
  undefined8 local_c0;
  undefined8 local_b8;
  undefined8 local_b0;
  undefined1 auStack_a8 [8];
  undefined1 auStack_a0 [12];
  undefined1 auStack_94 [4];
  undefined8 local_90;
  undefined1 auStack_84 [4];
  undefined8 local_80;
  undefined1 auStack_74 [4];
  undefined8 local_70;
  undefined8 local_68;
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
  if (sVar2 != 0) {
    local_48 = 2;
    goto LAB_0010a580;
  }
  uVar3 = __Z26GetExecutionContextSessionv();
  FUN_00002978(auStack_50,uVar3,&uStack_51);
  sVar2 = FUN_000029b4(auStack_50);
  if (sVar2 == 0) {
    FUN_00013490(&local_60);
    FUN_00013490(0,&local_68);
    FUN_00002cf8(auStack_74,0x15d359);
    local_70 = FUN_00013338(auStack_30,auStack_74);
    FUN_00002cf8(auStack_84,&DAT_0015d371);
    local_80 = FUN_00013338(auStack_30,auStack_84);
    FUN_00002cf8(auStack_94,&DAT_0015d373);
    local_90 = FUN_00013338(auStack_30,auStack_94);
    FUN_00013490(0);
    sVar2 = FUN_00014140(&local_80,auStack_a0);
    bVar1 = true;
    if (sVar2 == 0) {
      FUN_00013490(0);
      sVar2 = FUN_00014140(&local_90,auStack_a8);
      bVar1 = sVar2 != 0;
    }
    if (bVar1) {
      local_b8 = FUN_000157c8(&local_80,&local_70);
      local_b0 = FUN_00015abc(&local_b8);
      local_60 = local_b0;
      local_c8 = FUN_000157c8(&local_90,&local_70);
      local_c0 = FUN_00015abc(&local_c8);
      local_68 = local_c0;
      plVar4 = (long *)FUN_000029d8(auStack_50);
      (**(code **)(*plVar4 + 0x260))(plVar4,&local_60);
      plVar4 = (long *)FUN_000029d8(auStack_50);
      (**(code **)(*plVar4 + 0x270))(plVar4,&local_68);
      plVar4 = (long *)FUN_000029d8(auStack_50);
      local_d0 = FUN_00017c4c(&local_60,&local_68);
      (**(code **)(*plVar4 + 0x1f0))(plVar4,&local_d0);
      FUN_00002cf8(auStack_dc,0x15d49e);
      local_d8 = FUN_000194dc(auStack_30,auStack_dc);
      sVar2 = FUN_0003208c(&local_d8,&local_60);
      if (sVar2 == 0) {
        FUN_00002cf8(auStack_134,0x15d4a0);
        local_130 = FUN_000194dc(auStack_30,auStack_134);
        sVar2 = FUN_0003208c(&local_130,&local_68);
        if (sVar2 == 0) goto LAB_0010a330;
        FUN_00002cf8(auStack_138,&DAT_0015d373);
        __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_180,"",0);
        FUN_00013588(auStack_30,auStack_138,auStack_180);
        __ZN8PMStringD1Ev(auStack_180);
        local_48 = 2;
      }
      else {
        FUN_00002cf8(auStack_e0,&DAT_0015d371);
        __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_128,"",0);
        FUN_00013588(auStack_30,auStack_e0,auStack_128);
        __ZN8PMStringD1Ev(auStack_128);
        local_48 = 2;
      }
    }
    else {
      plVar4 = (long *)FUN_000029d8(auStack_50);
      local_188 = (**(code **)(*plVar4 + 600))();
      local_60 = local_188;
      plVar4 = (long *)FUN_000029d8(auStack_50);
      local_190 = (**(code **)(*plVar4 + 0x268))();
      local_68 = local_190;
      plVar4 = (long *)FUN_000029d8(auStack_50);
      local_198 = FUN_00017c4c(&local_60,&local_68);
      (**(code **)(*plVar4 + 0x1f0))(plVar4,&local_198);
LAB_0010a330:
      FUN_00002cf8(auStack_19c,0x15d49e);
      FUN_00013c20(auStack_30,auStack_19c,&local_60);
      FUN_00002cf8(auStack_1a0,0x15d4a0);
      FUN_00013c20(auStack_30,auStack_1a0,&local_68);
      FUN_00013490(0);
      sVar2 = FUN_00014140(&local_80,auStack_1a8);
      if (sVar2 != 0) {
        FUN_00002cf8(auStack_1ac,0x15d4cc);
        local_1b0 = 0;
        FUN_00015b04(auStack_30,auStack_1ac,&local_1b0,1);
        FUN_00002cf8(auStack_1b4,0x15d49e);
        FUN_0012dd50(param_1,auStack_1b4,&DAT_00208c52);
        FUN_00017cd0(auStack_30);
      }
      FUN_00013490(0);
      sVar2 = FUN_00014140(&local_90,auStack_1c0);
      if (sVar2 == 0) {
        FUN_00013490(0);
        sVar2 = FUN_000132fc(&local_80,auStack_1d8);
        bVar1 = false;
        if (sVar2 != 0) {
          FUN_00013490(0);
          sVar2 = FUN_000132fc(&local_90,auStack_1e0);
          bVar1 = sVar2 != 0;
        }
        if (bVar1) {
          FUN_00002cf8(auStack_1e4,0x15d49e);
          FUN_0012dd50(param_1,auStack_1e4,&DAT_00208c50);
          FUN_00002cf8(auStack_1e8,0x15d4a0);
          FUN_0012dd50(param_1,auStack_1e8,&DAT_00208c50);
        }
      }
      else {
        FUN_00002cf8(auStack_1c4,0x15d4cc);
        local_1c8 = 0;
        FUN_00015b04(auStack_30,auStack_1c4,&local_1c8,1);
        FUN_00002cf8(auStack_1cc,0x15d4a0);
        FUN_0012dd50(param_1,auStack_1cc,&DAT_00208c52);
        FUN_00017cd0(auStack_30);
      }
      local_48 = 0;
    }
  }
  else {
    local_48 = 2;
  }
  FUN_00002b9c(auStack_50);
LAB_0010a580:
  FUN_00002c54(auStack_30);
  return;
}

//==== FUNC @ 177ab0 -> 001773a4

void FUN_001773a4(void)

{
  bool bVar1;
  short sVar2;
  undefined4 uVar3;
  int iVar4;
  long lVar5;
  undefined8 uVar6;
  long *plVar7;
  ulong uVar8;
  long lVar9;
  undefined8 local_16a8;
  undefined8 local_16a0;
  undefined1 auStack_1694 [4];
  undefined8 local_1690;
  undefined8 local_1688;
  undefined1 auStack_167c [4];
  undefined8 local_1678;
  undefined1 auStack_166c [4];
  undefined8 local_1668;
  undefined1 auStack_165c [4];
  undefined1 auStack_1658 [12];
  undefined1 auStack_164c [4];
  undefined1 auStack_1648 [7];
  byte local_1641;
  undefined1 auStack_1640 [72];
  undefined1 auStack_15f8 [72];
  undefined1 auStack_15b0 [4];
  undefined1 auStack_15ac [4];
  undefined8 local_15a8;
  undefined8 local_15a0;
  undefined8 local_1598;
  undefined8 local_1590;
  undefined8 local_1588;
  undefined8 local_1580;
  undefined8 local_1578;
  undefined8 local_1570;
  undefined8 local_1568;
  undefined1 auStack_155c [4];
  undefined1 auStack_1558 [12];
  undefined1 auStack_154c [4];
  undefined1 auStack_1548 [4];
  undefined1 auStack_1544 [4];
  undefined1 auStack_1540 [4];
  undefined1 auStack_153c [4];
  undefined1 auStack_1538 [4];
  undefined1 auStack_1534 [4];
  undefined1 auStack_1530 [4];
  undefined1 auStack_152c [4];
  undefined1 auStack_1528 [76];
  undefined1 auStack_14dc [4];
  undefined1 auStack_14d8 [4];
  undefined1 auStack_14d4 [4];
  undefined1 auStack_14d0 [12];
  undefined1 auStack_14c4 [4];
  undefined1 auStack_14c0 [4];
  undefined1 auStack_14bc [4];
  undefined1 auStack_14b8 [4];
  undefined4 local_14b4;
  undefined1 auStack_14b0 [4];
  undefined1 auStack_14ac [4];
  undefined1 auStack_14a8 [4];
  undefined1 auStack_14a4 [4];
  undefined1 auStack_14a0 [4];
  undefined1 auStack_149c [4];
  undefined1 auStack_1498 [4];
  undefined1 auStack_1494 [4];
  undefined1 auStack_1490 [8];
  undefined8 local_1488;
  undefined1 auStack_147c [4];
  undefined8 local_1478;
  undefined8 local_1470;
  undefined8 local_1468;
  undefined1 auStack_145c [4];
  undefined1 auStack_1458 [8];
  undefined1 auStack_1450 [4];
  undefined1 auStack_144c [6];
  short local_1446;
  undefined1 auStack_1444 [4];
  undefined1 auStack_1440 [8];
  undefined8 local_1438;
  undefined1 auStack_1430 [8];
  undefined8 local_1428;
  undefined8 local_1420;
  undefined8 local_1418;
  undefined8 local_1410;
  undefined8 local_1408;
  undefined8 local_1400;
  undefined8 local_13f8;
  undefined1 auStack_13ec [4];
  undefined8 local_13e8;
  undefined8 local_13e0;
  undefined8 local_13d8;
  undefined1 auStack_13d0 [4];
  undefined1 auStack_13cc [4];
  undefined1 auStack_13c8 [8];
  undefined4 local_13c0;
  undefined1 auStack_13bc [4];
  undefined1 auStack_13b8 [4];
  undefined1 auStack_13b4 [4];
  undefined1 auStack_13b0 [8];
  undefined1 auStack_13a8 [8];
  undefined1 auStack_13a0 [8];
  undefined8 local_1398;
  undefined8 local_1390;
  undefined8 local_1388;
  undefined1 auStack_1380 [76];
  undefined1 auStack_1334 [4];
  undefined1 auStack_1330 [8];
  undefined8 local_1328;
  undefined1 auStack_1320 [8];
  undefined8 local_1318;
  undefined8 local_1310;
  undefined8 local_1308;
  undefined8 local_1300;
  undefined1 auStack_12f8 [76];
  undefined1 auStack_12ac [4];
  undefined1 auStack_12a8 [15];
  byte local_1299;
  undefined1 auStack_1298 [72];
  undefined1 auStack_1250 [76];
  undefined1 auStack_1204 [4];
  undefined8 local_1200;
  undefined2 local_11f6;
  undefined1 auStack_11f4 [4];
  undefined1 auStack_11f0 [8];
  undefined8 local_11e8;
  undefined1 auStack_11e0 [8];
  undefined8 local_11d8;
  undefined8 local_11d0;
  undefined8 local_11c8;
  undefined8 local_11c0;
  undefined1 auStack_11b8 [72];
  undefined4 local_1170;
  undefined4 local_116c;
  undefined8 local_1168;
  undefined8 local_1160;
  undefined4 local_1154;
  undefined1 auStack_1150 [8];
  undefined4 local_1148;
  undefined4 local_1144;
  undefined4 local_1140;
  undefined1 auStack_113c [4];
  undefined1 auStack_1138 [8];
  undefined1 auStack_1130 [8];
  undefined1 auStack_1128 [8];
  undefined8 local_1120;
  undefined1 auStack_1118 [76];
  undefined1 auStack_10cc [4];
  undefined1 auStack_10c8 [8];
  undefined8 local_10c0;
  undefined8 local_10b8;
  undefined1 auStack_10b0 [8];
  undefined8 local_10a8;
  undefined8 local_10a0;
  undefined8 local_1098;
  undefined8 local_1090;
  undefined8 local_1088;
  undefined8 local_1080;
  undefined8 local_1078;
  undefined8 local_1070;
  undefined8 local_1068;
  undefined8 local_1060;
  undefined8 local_1058;
  undefined8 local_1050;
  undefined1 auStack_1048 [8];
  undefined8 local_1040;
  undefined8 local_1038;
  undefined1 auStack_1030 [8];
  undefined1 auStack_1028 [8];
  undefined8 local_1020;
  undefined8 local_1018;
  undefined8 local_1010;
  undefined8 local_1008;
  undefined8 local_1000;
  undefined1 auStack_ff8 [8];
  undefined8 local_ff0;
  undefined8 local_fe8;
  undefined8 local_fe0;
  undefined8 local_fd8;
  undefined8 local_fd0;
  undefined8 local_fc8;
  undefined8 local_fc0;
  undefined8 local_fb8;
  undefined1 auStack_fb0 [8];
  undefined8 local_fa8;
  undefined8 local_fa0;
  undefined8 local_f98;
  undefined8 local_f90;
  undefined8 local_f88;
  undefined8 local_f80;
  undefined8 local_f78;
  undefined8 local_f70;
  undefined8 local_f68;
  undefined1 auStack_f60 [8];
  undefined8 local_f58;
  undefined1 auStack_f50 [8];
  undefined8 local_f48;
  undefined8 local_f40;
  undefined8 local_f38;
  undefined8 local_f30;
  undefined8 local_f28;
  undefined8 local_f20;
  undefined8 local_f18;
  undefined8 local_f10;
  undefined8 local_f08;
  undefined8 local_f00;
  undefined1 auStack_ef8 [8];
  undefined8 local_ef0;
  undefined8 local_ee8;
  undefined1 auStack_ee0 [8];
  undefined1 auStack_ed8 [8];
  undefined8 local_ed0;
  undefined8 local_ec8;
  undefined8 local_ec0;
  undefined8 local_eb8;
  undefined8 local_eb0;
  undefined1 auStack_ea8 [8];
  undefined8 local_ea0;
  undefined8 local_e98;
  undefined8 local_e90;
  undefined1 auStack_e88 [8];
  undefined8 local_e80;
  undefined1 auStack_e78 [8];
  undefined8 local_e70;
  undefined8 local_e68;
  undefined8 local_e60;
  undefined8 local_e58;
  undefined1 auStack_e50 [8];
  undefined1 auStack_e48 [8];
  undefined8 local_e40;
  undefined8 local_e38;
  undefined8 local_e30;
  undefined8 local_e28;
  undefined1 auStack_e20 [8];
  undefined8 local_e18;
  undefined1 auStack_e10 [8];
  undefined8 local_e08;
  undefined8 local_e00;
  undefined8 local_df8;
  undefined1 auStack_df0 [8];
  undefined8 local_de8;
  undefined8 local_de0;
  undefined8 local_dd8;
  undefined8 local_dd0;
  undefined8 local_dc8;
  undefined1 auStack_dc0 [8];
  undefined8 local_db8;
  undefined8 local_db0;
  undefined8 local_da8;
  undefined8 local_da0;
  undefined8 local_d98;
  undefined1 auStack_d90 [8];
  undefined8 local_d88;
  undefined8 local_d80;
  undefined1 auStack_d78 [8];
  undefined8 local_d70;
  undefined8 local_d68;
  undefined8 local_d60;
  undefined1 auStack_d58 [8];
  undefined8 local_d50;
  undefined8 local_d48;
  undefined1 auStack_d3c [4];
  undefined8 local_d38;
  undefined4 local_d30;
  undefined1 auStack_d2c [4];
  undefined1 auStack_d28 [72];
  undefined8 local_ce0;
  undefined4 local_cd4;
  undefined1 auStack_cd0 [75];
  undefined1 uStack_c85;
  undefined4 local_c84;
  undefined1 auStack_c80 [11];
  undefined1 uStack_c75;
  undefined4 local_c74;
  undefined1 auStack_c70 [11];
  undefined1 uStack_c65;
  undefined4 local_c64;
  undefined1 auStack_c60 [15];
  undefined1 uStack_c51;
  undefined1 auStack_c50 [8];
  undefined1 auStack_c48 [8];
  undefined1 auStack_c40 [8];
  undefined4 local_c38;
  undefined4 local_c34;
  undefined4 local_c30;
  undefined4 local_c2c;
  undefined1 auStack_c28 [72];
  undefined1 auStack_be0 [72];
  undefined4 local_b98;
  undefined1 auStack_b94 [4];
  undefined4 local_b90;
  undefined1 auStack_b8c [4];
  undefined1 auStack_b88 [8];
  undefined1 auStack_b80 [8];
  undefined8 local_b78;
  undefined1 auStack_b70 [72];
  undefined1 auStack_b28 [7];
  byte local_b21;
  undefined1 auStack_b20 [72];
  undefined1 auStack_ad8 [72];
  undefined1 auStack_a90 [8];
  undefined1 auStack_a88 [8];
  undefined1 auStack_a80 [76];
  undefined1 auStack_a34 [4];
  undefined1 auStack_a30 [8];
  undefined8 local_a28;
  undefined8 local_a20;
  undefined8 local_a18;
  undefined8 local_a10;
  undefined1 auStack_a04 [4];
  undefined8 local_a00;
  undefined1 auStack_9f8 [8];
  undefined8 local_9f0;
  undefined8 local_9e8;
  undefined8 local_9e0;
  undefined1 auStack_9d8 [76];
  undefined1 auStack_98c [4];
  undefined8 local_988;
  undefined1 auStack_980 [8];
  undefined8 local_978;
  undefined1 auStack_970 [72];
  undefined1 auStack_928 [7];
  byte local_921;
  undefined1 auStack_920 [72];
  undefined1 auStack_8d8 [72];
  undefined1 auStack_890 [8];
  undefined8 local_888;
  undefined1 auStack_880 [76];
  undefined1 auStack_834 [4];
  undefined1 auStack_830 [8];
  undefined8 local_828;
  undefined8 local_820;
  undefined8 local_818;
  undefined8 local_810;
  undefined8 local_808;
  undefined8 local_800;
  undefined8 local_7f8;
  undefined8 local_7f0;
  undefined8 local_7e8;
  undefined8 local_7e0;
  undefined1 auStack_7d8 [76];
  undefined1 auStack_78c [4];
  undefined8 local_788;
  undefined1 auStack_780 [8];
  undefined8 local_778;
  undefined1 auStack_770 [76];
  undefined1 auStack_724 [4];
  undefined8 local_720;
  undefined1 auStack_718 [6];
  short local_712;
  undefined8 local_710;
  undefined4 local_704;
  undefined1 auStack_700 [76];
  undefined4 local_6b4;
  undefined1 auStack_6b0 [72];
  undefined4 local_668;
  undefined1 auStack_664 [4];
  undefined1 auStack_660 [72];
  undefined1 auStack_618 [6];
  short local_612;
  undefined1 auStack_610 [6];
  short local_60a;
  undefined4 local_608;
  undefined1 auStack_604 [4];
  undefined1 auStack_600 [72];
  undefined8 local_5b8;
  undefined8 local_5b0;
  undefined8 local_5a8;
  undefined8 local_5a0;
  undefined1 auStack_594 [4];
  undefined1 auStack_590 [4];
  undefined1 auStack_58c [4];
  undefined1 auStack_588 [8];
  undefined1 auStack_580 [12];
  undefined1 auStack_574 [4];
  undefined8 local_570;
  undefined1 auStack_568 [12];
  undefined1 auStack_55c [4];
  undefined8 local_558;
  undefined1 auStack_54c [4];
  undefined1 auStack_548 [7];
  byte local_541;
  undefined1 auStack_540 [72];
  undefined1 auStack_4f8 [72];
  undefined4 local_4b0;
  undefined1 auStack_4ac [4];
  undefined1 auStack_4a8 [76];
  undefined1 auStack_45c [4];
  undefined1 auStack_458 [76];
  undefined1 auStack_40c [4];
  undefined1 auStack_408 [76];
  undefined1 auStack_3bc [4];
  undefined1 auStack_3b8 [76];
  undefined1 auStack_36c [4];
  undefined1 auStack_368 [76];
  undefined1 auStack_31c [4];
  undefined1 auStack_318 [76];
  undefined1 auStack_2cc [4];
  undefined1 auStack_2c8 [76];
  undefined1 auStack_27c [4];
  undefined1 auStack_278 [76];
  undefined1 auStack_22c [4];
  undefined1 auStack_228 [4];
  undefined1 auStack_224 [4];
  undefined1 auStack_220 [4];
  undefined1 auStack_21c [4];
  undefined1 auStack_218 [4];
  undefined1 auStack_214 [4];
  undefined1 auStack_210 [4];
  undefined1 auStack_20c [4];
  undefined1 auStack_208 [4];
  undefined1 auStack_204 [4];
  undefined8 local_200;
  undefined8 local_1f8;
  undefined8 local_1f0;
  undefined8 local_1e8;
  undefined8 local_1e0;
  undefined8 local_1d8;
  undefined1 auStack_1cc [4];
  undefined8 local_1c8;
  undefined1 auStack_1bc [4];
  undefined8 local_1b8;
  undefined1 auStack_1ac [4];
  undefined8 local_1a8;
  undefined1 auStack_19c [4];
  undefined8 local_198;
  undefined4 local_18c;
  undefined1 auStack_188 [78];
  short local_13a;
  undefined8 local_138;
  undefined8 local_130;
  undefined8 local_128;
  undefined8 local_120;
  undefined8 local_118;
  undefined1 auStack_110 [8];
  int local_108;
  undefined1 auStack_104 [4];
  undefined8 local_100;
  undefined8 local_f8;
  undefined1 auStack_ec [4];
  undefined8 local_e8;
  undefined1 uStack_d9;
  undefined1 auStack_d8 [15];
  undefined1 uStack_c9;
  undefined1 auStack_c8 [8];
  undefined1 auStack_c0 [15];
  undefined1 uStack_b1;
  undefined1 auStack_b0 [8];
  undefined1 local_a8 [16];
  undefined1 uStack_91;
  undefined1 local_90 [16];
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
  
  lVar5 = (*(code *)PTR____chkstk_darwin_002340e0)();
  local_28 = lVar5;
  FUN_00002bf4(auStack_30,lVar5,&uStack_31);
  sVar2 = FUN_00002c30(auStack_30);
  if (sVar2 != 0) {
    local_48 = 2;
    goto LAB_0017c7b4;
  }
  uVar6 = __Z26GetExecutionContextSessionv();
  FUN_00002978(auStack_50,uVar6,&uStack_51);
  sVar2 = FUN_000029b4(auStack_50);
  if (sVar2 == 0) {
    plVar7 = (long *)FUN_000029d8(auStack_50);
    local_68 = (**(code **)(*plVar7 + 0x18))();
    FUN_0000df20(auStack_70,local_68,&uStack_71);
    sVar2 = FUN_0000df5c(auStack_70);
    if (sVar2 == 0) {
      plVar7 = (long *)FUN_0001b750(auStack_70);
      local_90 = (**(code **)(*plVar7 + 0x88))();
      FUN_0001b768(auStack_80,local_90,&uStack_91);
      sVar2 = FUN_0001b7a4(auStack_80);
      if (sVar2 == 0) {
        plVar7 = (long *)FUN_000029d8(auStack_50);
        local_a8 = (**(code **)(*plVar7 + 0x28))();
        FUN_0001b840(auStack_b0,local_a8,&uStack_b1);
        sVar2 = FUN_0001b87c(auStack_b0);
        if (sVar2 == 0) {
          FUN_001a2484(auStack_c0);
          sVar2 = FUN_0000e2a4(auStack_c0);
          if (sVar2 == 0) {
            FUN_00017bd4(auStack_c8,local_a8,&uStack_c9);
            sVar2 = FUN_00017c10(auStack_c8);
            if (sVar2 == 0) {
              uVar6 = FUN_0000df80(auStack_70);
              FUN_0000df98(auStack_d8,uVar6,&uStack_d9);
              sVar2 = FUN_0000dfd4(auStack_d8);
              if (sVar2 == 0) {
                FUN_00002cf8(auStack_ec,0x15d31d);
                local_f8 = FUN_00013338(auStack_30,auStack_ec);
                local_e8 = local_f8;
                FUN_00002cf8(auStack_104,0x15d347);
                local_100 = FUN_00013338(auStack_30,auStack_104);
                plVar7 = (long *)FUN_000029d8(auStack_50);
                local_108 = (**(code **)(*plVar7 + 0xf8))();
                FUN_00013490(0);
                sVar2 = FUN_000132fc(&local_f8,auStack_110);
                if (sVar2 == 0 && local_108 != 0) {
                  FUN_00015a50(&local_118);
                  FUN_00015a50(&local_120);
                  FUN_00015a50(&local_128);
                  FUN_00015a50(&local_130);
                  FUN_00015a50(&local_138);
                  local_13a = 1;
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_188,"[GC] Break Vertical",0);
                  plVar7 = (long *)FUN_0000dff8(auStack_d8);
                  local_18c = (**(code **)(*plVar7 + 0x20))(plVar7,auStack_188);
                  uVar8 = FUN_00002e24(&local_18c,&DAT_00208c58);
                  if ((uVar8 & 1) == 0) {
                    FUN_00002cf8(auStack_1bc,0x15d49b);
                    local_1b8 = FUN_00013338(auStack_30,auStack_1bc);
                    FUN_00002cf8(auStack_1cc,0x15d49d);
                    local_1c8 = FUN_00013338(auStack_30,auStack_1cc);
                    local_1e8 = FUN_000157c8(&local_1b8,lVar5 + 0x88);
                    local_1e0 = FUN_000157c8(&local_1e8,&local_f8);
                    local_1d8 = FUN_00015abc(&local_1e0);
                    local_118 = local_1d8;
                    local_200 = FUN_000157c8(&local_1c8,lVar5 + 0x88);
                    local_1f8 = FUN_000157c8(&local_200,&local_f8);
                    local_1f0 = FUN_00015abc(&local_1f8);
                    local_120 = local_1f0;
                  }
                  else {
                    FUN_00002cf8(auStack_19c,0x15d49a);
                    local_198 = FUN_00013338(auStack_30,auStack_19c);
                    local_118 = local_198;
                    FUN_00002cf8(auStack_1ac,0x15d49c);
                    local_1a8 = FUN_00013338(auStack_30,auStack_1ac);
                    local_120 = local_1a8;
                  }
                  FUN_00002cf8(auStack_204,0x15d4c9);
                  FUN_00011dc4(auStack_30,auStack_204,&DAT_00208c52);
                  FUN_00002cf8(auStack_208,0x15d4d5);
                  FUN_00011dc4(auStack_30,auStack_208,&DAT_00208c52);
                  FUN_00002cf8(auStack_20c,0x15d4ca);
                  FUN_00011dc4(auStack_30,auStack_20c,&DAT_00208c52);
                  FUN_00002cf8(auStack_210,0x15d4cb);
                  FUN_00011dc4(auStack_30,auStack_210,&DAT_00208c52);
                  FUN_00002cf8(auStack_214,0x15d4e0);
                  FUN_00011dc4(auStack_30,auStack_214,&DAT_00208c52);
                  FUN_00002cf8(auStack_218,0x15d4e1);
                  FUN_00011dc4(auStack_30,auStack_218,&DAT_00208c52);
                  FUN_00002cf8(auStack_21c,0x15d36c);
                  FUN_00011dc4(auStack_30,auStack_21c,&DAT_00208c50);
                  FUN_00002cf8(auStack_220,0x15d36e);
                  FUN_00011dc4(auStack_30,auStack_220,&DAT_00208c50);
                  FUN_00002cf8(auStack_224,0x15d36a);
                  FUN_00011dc4(auStack_30,auStack_224,&DAT_00208c50);
                  FUN_00002cf8(auStack_228,0x15d367);
                  FUN_00011dc4(auStack_30,auStack_228,&DAT_00208c50);
                  FUN_00002cf8(auStack_22c,0x15d4b3);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_278,"",0);
                  FUN_00013588(auStack_30,auStack_22c,auStack_278);
                  __ZN8PMStringD1Ev(auStack_278);
                  FUN_00002cf8(auStack_27c,0x15d4b4);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_2c8,"",0);
                  FUN_00013588(auStack_30,auStack_27c,auStack_2c8);
                  __ZN8PMStringD1Ev(auStack_2c8);
                  FUN_00002cf8(auStack_2cc,0x15d4b5);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_318,"",0);
                  FUN_00013588(auStack_30,auStack_2cc,auStack_318);
                  __ZN8PMStringD1Ev(auStack_318);
                  FUN_00002cf8(auStack_31c,0x15d369);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_368,"",0);
                  FUN_00013588(auStack_30,auStack_31c,auStack_368);
                  __ZN8PMStringD1Ev(auStack_368);
                  FUN_00002cf8(auStack_36c,0x15d36b);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_3b8,"",0);
                  FUN_00013588(auStack_30,auStack_36c,auStack_3b8);
                  __ZN8PMStringD1Ev(auStack_3b8);
                  FUN_00002cf8(auStack_3bc,0x15d368);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_408,"",0);
                  FUN_00013588(auStack_30,auStack_3bc,auStack_408);
                  __ZN8PMStringD1Ev(auStack_408);
                  FUN_00002cf8(auStack_40c,0x15d36d);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_458,"",0);
                  FUN_00013588(auStack_30,auStack_40c,auStack_458);
                  __ZN8PMStringD1Ev(auStack_458);
                  FUN_00002cf8(auStack_45c,0x15d36f);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_4a8,"",0);
                  FUN_00013588(auStack_30,auStack_45c,auStack_4a8);
                  __ZN8PMStringD1Ev(auStack_4a8);
                  FUN_00002cf8(auStack_4ac,0x15d37a);
                  local_4b0 = 0;
                  FUN_00015b04(auStack_30,auStack_4ac,&local_4b0,1);
                  local_541 = 0;
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_4f8,"[GC] Smart Setup");
                  sVar2 = FUN_001a6604(auStack_4f8);
                  bVar1 = true;
                  if (sVar2 == 0) {
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_540,"[GC] CGS",0);
                    local_541 = 1;
                    sVar2 = FUN_001a6604(auStack_540);
                    bVar1 = sVar2 != 0;
                  }
                  if ((local_541 & 1) != 0) {
                    __ZN8PMStringD1Ev(auStack_540);
                  }
                  __ZN8PMStringD1Ev(auStack_4f8);
                  if (bVar1) {
                    FUN_00002cf8(auStack_548,0x15d4b3);
                    FUN_00011dc4(auStack_30,auStack_548,&DAT_00208c50);
                    FUN_00002cf8(auStack_54c,0x15d4b5);
                    FUN_00011dc4(auStack_30,auStack_54c,&DAT_00208c50);
                    FUN_00002cf8(auStack_55c,&DAT_0015d376);
                    local_558 = FUN_00013338(auStack_30,auStack_55c);
                    FUN_00013490(0);
                    sVar2 = FUN_000132fc(&local_558,auStack_568);
                    bVar1 = false;
                    if (sVar2 != 0) {
                      FUN_00002cf8(auStack_574,&DAT_0015d378);
                      local_570 = FUN_00013338(auStack_30,auStack_574);
                      FUN_00013490(0);
                      sVar2 = FUN_000132fc(&local_570,auStack_580);
                      bVar1 = false;
                      if (sVar2 != 0) {
                        FUN_00013490(0x3ff0000000000000);
                        sVar2 = FUN_00031d38(&local_100,auStack_588);
                        bVar1 = sVar2 != 0;
                      }
                    }
                    if (bVar1) {
                      FUN_00002cf8(auStack_58c,0x15d369);
                      FUN_00011dc4(auStack_30,auStack_58c,&DAT_00208c52);
                    }
                  }
                  else {
                    FUN_00002cf8(auStack_590,0x15d4b3);
                    FUN_00011dc4(auStack_30,auStack_590,&DAT_00208c52);
                    FUN_00002cf8(auStack_594,0x15d4b5);
                    FUN_00011dc4(auStack_30,auStack_594,&DAT_00208c52);
                  }
                  FUN_00013490(&local_5a0);
                  FUN_00013490(0,&local_5a8);
                  FUN_00013490(0,&local_5b0);
                  FUN_00013490(0,&local_5b8);
                  FUN_00002cf8(auStack_604,0x15d306);
                  local_608 = 0xffffffff;
                  FUN_00012c10(auStack_600,auStack_30,auStack_604,&local_608);
                  FUN_00002cf8(auStack_610,0x15d323);
                  local_60a = FUN_00012ef4(auStack_30,auStack_610);
                  FUN_00002cf8(auStack_618,0x15d4a3);
                  local_612 = FUN_00012ef4(auStack_30,auStack_618);
                  FUN_00002cf8(auStack_664,0x15d309);
                  local_668 = 0xffffffff;
                  FUN_00012c10(auStack_660,auStack_30,auStack_664,&local_668);
                  __ZN8PMStringC1ERKS_(auStack_6b0,auStack_660);
                  __ZN8PMString6AppendEPKciNS_14StringEncodingE
                            (auStack_6b0," Break Horizontal",0x7fffffff,0xffffffff);
                  plVar7 = (long *)FUN_0000dff8(auStack_d8);
                  local_6b4 = (**(code **)(*plVar7 + 0x20))(plVar7,auStack_6b0);
                  __ZN8PMStringC1ERKS_(auStack_700,auStack_660);
                  __ZN8PMString6AppendEPKciNS_14StringEncodingE
                            (auStack_700," Image-lines",0x7fffffff,0xffffffff);
                  plVar7 = (long *)FUN_0000dff8(auStack_d8);
                  local_704 = (**(code **)(*plVar7 + 0x20))(plVar7,auStack_700);
                  uVar8 = FUN_0000e010(&local_704,&DAT_00208c58);
                  if ((uVar8 & 1) != 0) {
                    plVar7 = (long *)FUN_00017c34(auStack_c8);
                    local_710 = (**(code **)(*plVar7 + 0x2c0))();
                    local_5b0 = local_710;
                    FUN_00002cf8(auStack_718,0x15d3a9);
                    local_712 = FUN_00012ef4(auStack_30,auStack_718);
                    if (local_712 == 1) {
                      FUN_00002cf8(auStack_724,0x15d3a8);
                      local_720 = FUN_00013338(auStack_30,auStack_724);
                      local_5a8 = local_720;
                    }
                  }
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_770,"[GC-1]",0);
                  sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_660,auStack_770,1,0);
                  __ZN8PMStringD1Ev(auStack_770);
                  if (sVar2 == 0) {
                    plVar7 = (long *)FUN_000029d8(auStack_50);
                    local_778 = (**(code **)(*plVar7 + 0x1c8))();
                    local_5b8 = local_778;
                  }
                  FUN_00013490(0);
                  sVar2 = FUN_00018504(&local_5b0,auStack_780);
                  if (sVar2 == 0) {
                    if ((local_612 == 0) || (local_60a == 0)) {
                      FUN_00013490((double)(long)local_108);
                      local_a28 = FUN_00028080(auStack_a30,&local_118);
                      local_a20 = FUN_00028080(&local_a28,&local_120);
                      local_a18 = FUN_00015a7c(&local_a20,&local_f8);
                      local_a10 = FUN_00028080(&local_a18,&local_5b8);
                      local_5a0 = local_a10;
                    }
                    else {
                      FUN_00002cf8(auStack_98c,0x15d4a9);
                      local_988 = FUN_0001544c(auStack_30,auStack_98c,auStack_600);
                      local_5a0 = local_988;
                      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_9d8,"[GC-1]",0);
                      sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_660,auStack_9d8,1,0);
                      __ZN8PMStringD1Ev(auStack_9d8);
                      if (sVar2 != 0) {
                        local_9e8 = FUN_000157c8(&local_5a0,&local_f8);
                        local_9e0 = FUN_00015abc(&local_9e8);
                        local_9f0 = FUN_00015a7c(&local_9e0,&local_f8);
                        local_5a0 = local_9f0;
                      }
                      FUN_00013490(0x3f50624dd2f1a9fc);
                      sVar2 = FUN_00031d38(&local_5a0,auStack_9f8);
                      if (sVar2 != 0) {
                        FUN_00002cf8(auStack_a04,&DAT_0015d31b);
                        local_a00 = FUN_0001544c(auStack_30,auStack_a04,auStack_600);
                        local_5a0 = local_a00;
                      }
                    }
                    FUN_00002cf8(auStack_a34,0x15d4c9);
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_a80,"rows",0);
                    FUN_00013490();
                    FUN_00013490(0);
                    FUN_001668f4(lVar5,auStack_a34,auStack_a80,&local_5a0,&local_f8,auStack_a88,
                                 auStack_a90);
                    __ZN8PMStringD1Ev(auStack_a80);
                    local_b21 = 0;
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_ad8,"[GC] Smart Setup");
                    sVar2 = FUN_001a6604(auStack_ad8);
                    bVar1 = true;
                    if (sVar2 == 0) {
                      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_b20,"[GC] CGS",0);
                      local_b21 = 1;
                      sVar2 = FUN_001a6604(auStack_b20);
                      bVar1 = sVar2 != 0;
                    }
                    if ((local_b21 & 1) != 0) {
                      __ZN8PMStringD1Ev(auStack_b20);
                    }
                    __ZN8PMStringD1Ev(auStack_ad8);
                    if (bVar1) {
                      FUN_00002cf8(auStack_b28,0x15d37a);
                      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_b70,"rows",0);
                      local_b78 = FUN_00028080(&local_5a0,&local_5b8);
                      FUN_00013490();
                      FUN_00013490(0);
                      FUN_001668f4(lVar5,auStack_b28,auStack_b70,&local_b78,&local_f8,auStack_b80,
                                   auStack_b88);
                      __ZN8PMStringD1Ev(auStack_b70);
                    }
                  }
                  else {
                    if ((local_612 == 0) || (local_60a == 0)) {
                      FUN_00013490((double)(long)local_108);
                      local_828 = FUN_00028080(auStack_830,&local_118);
                      local_820 = FUN_00028080(&local_828,&local_120);
                      local_818 = FUN_00015a7c(&local_820,&local_f8);
                      local_810 = FUN_00028080(&local_818,&local_5b0);
                      local_808 = FUN_00028080(&local_810,&local_5b8);
                      local_5a0 = local_808;
                    }
                    else {
                      FUN_00002cf8(auStack_78c,0x15d4a9);
                      local_788 = FUN_0001544c(auStack_30,auStack_78c,auStack_600);
                      local_5a0 = local_788;
                      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_7d8,"[GC-1]",0);
                      sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_660,auStack_7d8,1,0);
                      __ZN8PMStringD1Ev(auStack_7d8);
                      if (sVar2 != 0) {
                        local_7f0 = FUN_00017c4c(&local_5a0,&local_5b0);
                        local_7e8 = FUN_000157c8(&local_7f0,&local_f8);
                        local_7e0 = FUN_00015abc(&local_7e8);
                        local_800 = FUN_00015a7c(&local_7e0,&local_f8);
                        local_7f8 = FUN_00028080(&local_800,&local_5b0);
                        local_5a0 = local_7f8;
                      }
                    }
                    FUN_00002cf8(auStack_834,0x15d4c9);
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_880,"rows",0);
                    local_888 = FUN_00028080(&local_f8,&local_5b0);
                    FUN_00013490(0);
                    FUN_001668f4(lVar5,auStack_834,auStack_880,&local_5a0,&local_f8,&local_888,
                                 auStack_890);
                    __ZN8PMStringD1Ev(auStack_880);
                    local_921 = 0;
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_8d8,"[GC] Smart Setup");
                    sVar2 = FUN_001a6604(auStack_8d8);
                    bVar1 = true;
                    if (sVar2 == 0) {
                      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_920,"[GC] CGS",0);
                      local_921 = 1;
                      sVar2 = FUN_001a6604(auStack_920);
                      bVar1 = sVar2 != 0;
                    }
                    if ((local_921 & 1) != 0) {
                      __ZN8PMStringD1Ev(auStack_920);
                    }
                    __ZN8PMStringD1Ev(auStack_8d8);
                    if (bVar1) {
                      FUN_00002cf8(auStack_928,0x15d37a);
                      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_970,"rows",0);
                      local_978 = FUN_00028080(&local_f8,&local_5b0);
                      FUN_00013490(0);
                      FUN_001668f4(lVar5,auStack_928,auStack_970,&local_5a0,&local_f8,&local_978,
                                   auStack_980);
                      __ZN8PMStringD1Ev(auStack_970);
                    }
                  }
                  FUN_00002cf8(auStack_b8c,0x15d4c9);
                  local_b90 = 0;
                  FUN_00015b04(auStack_30,auStack_b8c,&local_b90,1);
                  FUN_00002cf8(auStack_b94,0x15d37a);
                  local_b98 = 0;
                  FUN_00015b04(auStack_30,auStack_b94,&local_b98,1);
                  __ZN8PMStringC1ERKS_(auStack_be0,auStack_660);
                  __ZN8PMString6AppendEPKciNS_14StringEncodingE
                            (auStack_be0," Rows",0x7fffffff,0xffffffff);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_c28,"[Pro Ed.] Rows",0);
                  plVar7 = (long *)FUN_0000dff8(auStack_d8);
                  local_c2c = (**(code **)(*plVar7 + 0x20))(plVar7,auStack_c28);
                  uVar8 = FUN_0000e010(&local_c2c,&DAT_00208c58);
                  if ((uVar8 & 1) != 0) {
                    FUN_001a6b18(auStack_c28,auStack_be0);
                  }
                  plVar7 = (long *)FUN_0000dff8(auStack_d8);
                  local_c30 = (**(code **)(*plVar7 + 0x20))(plVar7,auStack_be0);
                  local_c2c = local_c30;
                  uVar8 = FUN_00002e24(&local_c2c,&DAT_00208c58);
                  if ((uVar8 & 1) == 0) {
                    plVar7 = (long *)FUN_0000dff8(auStack_d8);
                    local_c38 = local_c2c;
                    local_c34 = (**(code **)(*plVar7 + 0x40))(plVar7,local_c2c);
                    plVar7 = (long *)FUN_0000dff8(auStack_d8);
                    uVar6 = (**(code **)(*plVar7 + 0x28))(plVar7,local_c34);
                    FUN_000ca9c4(auStack_c40,uVar6);
                    sVar2 = FUN_000caa24(auStack_c40);
                    if (sVar2 == 0) {
                      plVar7 = (long *)FUN_0001c5a8(auStack_b0);
                      uVar6 = FUN_000caa74(auStack_c40);
                      uVar6 = (**(code **)(*plVar7 + 0x18))(plVar7,uVar6,0,1);
                      FUN_000caa8c(auStack_c48,uVar6);
                      uVar6 = FUN_000caac0(auStack_c48);
                      FUN_0016b140(auStack_c50,uVar6,&uStack_c51);
                      sVar2 = FUN_000cac10(auStack_c50);
                      if (sVar2 == 0) {
                        uVar6 = FUN_0000310c(local_68);
                        plVar7 = (long *)FUN_0016b17c(auStack_c50);
                        local_c64 = (**(code **)(*plVar7 + 0x20))(plVar7,0);
                        FUN_0016b194(auStack_c60,uVar6,local_c64,&uStack_c65);
                        uVar6 = FUN_0000310c(local_68);
                        plVar7 = (long *)FUN_0016b17c(auStack_c50);
                        local_c74 = (**(code **)(*plVar7 + 0x20))(plVar7,1);
                        FUN_0016b194(auStack_c70,uVar6,local_c74,&uStack_c75);
                        uVar6 = FUN_0000310c(local_68);
                        plVar7 = (long *)FUN_0016b17c(auStack_c50);
                        local_c84 = (**(code **)(*plVar7 + 0x20))(plVar7,2);
                        FUN_0016b194(auStack_c80,uVar6,local_c84,&uStack_c85);
                        sVar2 = FUN_00187b6c(auStack_c60);
                        if (sVar2 == 0) {
                          sVar2 = FUN_00187b6c(auStack_c70);
                          if (sVar2 != 0) goto LAB_00179800;
                          __ZN8PMStringC1ERKS_(auStack_cd0,auStack_660);
                          __ZN8PMString6AppendEPKciNS_14StringEncodingE
                                    (auStack_cd0," Edit Existing",0x7fffffff,0xffffffff);
                          plVar7 = (long *)FUN_0000dff8(auStack_d8);
                          local_cd4 = (**(code **)(*plVar7 + 0x20))(plVar7,auStack_cd0);
                          uVar8 = FUN_0000e010(&local_cd4,&DAT_00208c58);
                          if ((uVar8 & 1) == 0) {
                            FUN_00002cf8(auStack_d2c,0x15d306);
                            local_d30 = 0xffffffff;
                            FUN_00012c10(auStack_d28,auStack_30,auStack_d2c,&local_d30);
                            FUN_00002cf8(auStack_d3c,&DAT_0015d31b);
                            local_d38 = FUN_0001544c(auStack_30,auStack_d3c,auStack_d28);
                            FUN_00013490(0x4000000000000000);
                            local_d50 = FUN_000157c8(&local_d38,auStack_d58);
                            plVar7 = (long *)FUN_0016b1f8(auStack_c60);
                            local_d60 = (**(code **)(*plVar7 + 0x80))();
                            local_d48 = FUN_00017c4c(&local_d50,&local_d60);
                            FUN_00013490(0x4000000000000000);
                            local_d70 = FUN_000157c8(&local_d38,auStack_d78);
                            plVar7 = (long *)FUN_0016b1f8(auStack_c70);
                            local_d80 = (**(code **)(*plVar7 + 0x80))();
                            local_d68 = FUN_00017c4c(&local_d70,&local_d80);
                            FUN_00013490(&local_d88);
                            FUN_00013490(0);
                            sVar2 = FUN_000132fc(&local_5b0,auStack_d90);
                            if (sVar2 == 0) {
                              FUN_00013490(0);
                              sVar2 = FUN_00018504(&local_5b0,auStack_f60);
                              if (sVar2 != 0) {
                                local_f70 = FUN_00015a7c(&local_f8,&local_118);
                                local_f68 = FUN_00017c4c(&local_5b0,&local_f70);
                                sVar2 = FUN_00018504(&local_d48,&local_f68);
                                if (sVar2 != 0) {
                                  local_f78 = FUN_000157c8(&local_d48,&local_f8);
                                  local_130 = local_f78;
                                  local_f88 = FUN_00028080(&local_d68,&local_d48);
                                  local_f80 = FUN_000157c8(&local_f88,&local_f8);
                                  local_138 = local_f80;
                                  local_f90 = FUN_00015abc(&local_138);
                                  local_f98 = FUN_00015abc(&local_130);
                                  sVar2 = FUN_00014140(&local_f90,&local_f98);
                                  if (sVar2 != 0) {
                                    lVar9 = FUN_0016b1e0(auStack_c80);
                                    if (lVar9 != 0) {
                                      FUN_00013490(0x4000000000000000);
                                      local_fa8 = FUN_000157c8(&local_d38,auStack_fb0);
                                      plVar7 = (long *)FUN_0016b1f8(auStack_c80);
                                      local_fb8 = (**(code **)(*plVar7 + 0x80))();
                                      local_fa0 = FUN_00017c4c(&local_fa8,&local_fb8);
                                      local_d88 = local_fa0;
                                    }
                                    local_fc8 = FUN_00028080(&local_d68,&local_d48);
                                    local_fc0 = FUN_000157c8(&local_fc8,&local_f8);
                                    local_130 = local_fc0;
                                    local_fd8 = FUN_00028080(&local_d88,&local_d68);
                                    local_fd0 = FUN_000157c8(&local_fd8,&local_f8);
                                    local_138 = local_fd0;
                                  }
                                  FUN_00013490((double)(long)local_108);
                                  local_ff0 = FUN_00017c4c(auStack_ff8,&local_138);
                                  local_1000 = FUN_00017c4c(&local_138,&local_130);
                                  local_fe8 = FUN_000157c8(&local_ff0,&local_1000);
                                  local_fe0 = FUN_00015abc(&local_fe8);
                                  local_128 = local_fe0;
                                  local_1010 = FUN_00015a7c(&local_128,&local_130);
                                  FUN_00013490(0x3ff0000000000000);
                                  local_1020 = FUN_00028080(&local_128,auStack_1028);
                                  local_1018 = FUN_00015a7c(&local_1020,&local_138);
                                  local_1008 = FUN_00017c4c(&local_1010,&local_1018);
                                  FUN_00013490((double)(long)local_108);
                                  sVar2 = FUN_000132fc(&local_1008,auStack_1030);
                                  if (sVar2 != 0) {
                                    local_13a = 0;
                                    *(undefined2 *)(lVar5 + 0x90) = 0;
                                    *(undefined2 *)(lVar5 + 0x92) = 0;
                                  }
                                }
                                if (local_13a != 0) {
                                  lVar9 = FUN_0016b1e0(auStack_c80);
                                  if (lVar9 != 0) {
                                    FUN_00013490(0x4000000000000000);
                                    local_1040 = FUN_000157c8(&local_d38,auStack_1048);
                                    plVar7 = (long *)FUN_0016b1f8(auStack_c80);
                                    local_1050 = (**(code **)(*plVar7 + 0x80))();
                                    local_1038 = FUN_00017c4c(&local_1040,&local_1050);
                                    local_d88 = local_1038;
                                  }
                                  local_1068 = FUN_00028080(&local_d68,&local_d48);
                                  local_1060 = FUN_00028080(&local_1068,&local_5a8);
                                  local_1058 = FUN_000157c8(&local_1060,&local_f8);
                                  local_130 = local_1058;
                                  local_1080 = FUN_00028080(&local_d88,&local_d68);
                                  local_1078 = FUN_00028080(&local_1080,&local_5b0);
                                  local_1070 = FUN_000157c8(&local_1078,&local_f8);
                                  local_138 = local_1070;
                                  FUN_00013490((double)(long)local_108);
                                  local_10a8 = FUN_00017c4c(auStack_10b0,&local_138);
                                  local_10a0 = FUN_00028080(&local_10a8,&local_118);
                                  local_1098 = FUN_00028080(&local_10a0,&local_120);
                                  local_10c0 = FUN_00017c4c(&local_138,&local_130);
                                  FUN_00013490(0x3ff0000000000000);
                                  local_10b8 = FUN_00017c4c(&local_10c0,auStack_10c8);
                                  local_1090 = FUN_000157c8(&local_1098,&local_10b8);
                                  local_1088 = FUN_00015abc(&local_1090);
                                  local_128 = local_1088;
                                }
                              }
                            }
                            else {
                              local_d98 = FUN_000157c8(&local_d48,&local_f8);
                              local_130 = local_d98;
                              local_da8 = FUN_00028080(&local_d68,&local_d48);
                              local_da0 = FUN_000157c8(&local_da8,&local_f8);
                              local_138 = local_da0;
                              sVar2 = FUN_00014140(&local_138,&local_130);
                              if (sVar2 != 0) {
                                lVar9 = FUN_0016b1e0(auStack_c80);
                                if (lVar9 != 0) {
                                  FUN_00013490(0x4000000000000000);
                                  local_db8 = FUN_000157c8(&local_d38,auStack_dc0);
                                  plVar7 = (long *)FUN_0016b1f8(auStack_c80);
                                  local_dc8 = (**(code **)(*plVar7 + 0x80))();
                                  local_db0 = FUN_00017c4c(&local_db8,&local_dc8);
                                  local_d88 = local_db0;
                                }
                                local_dd8 = FUN_00028080(&local_d68,&local_d48);
                                local_dd0 = FUN_000157c8(&local_dd8,&local_f8);
                                local_130 = local_dd0;
                                local_de8 = FUN_00028080(&local_d88,&local_d68);
                                local_de0 = FUN_000157c8(&local_de8,&local_f8);
                                local_138 = local_de0;
                              }
                              FUN_00013490(0);
                              sVar2 = FUN_000132fc(&local_118,auStack_df0);
                              if (sVar2 == 0) {
                                FUN_00013490(0);
                                sVar2 = FUN_00018504(&local_118,auStack_e88);
                                if (sVar2 != 0) {
                                  FUN_00013490((double)(long)local_108);
                                  local_ea0 = FUN_00017c4c(auStack_ea8,&local_138);
                                  local_eb0 = FUN_00017c4c(&local_138,&local_130);
                                  local_e98 = FUN_000157c8(&local_ea0,&local_eb0);
                                  local_e90 = FUN_00015abc(&local_e98);
                                  local_128 = local_e90;
                                  local_ec0 = FUN_00015a7c(&local_128,&local_130);
                                  FUN_00013490(0x3ff0000000000000);
                                  local_ed0 = FUN_00028080(&local_128,auStack_ed8);
                                  local_ec8 = FUN_00015a7c(&local_ed0,&local_138);
                                  local_eb8 = FUN_00017c4c(&local_ec0,&local_ec8);
                                  FUN_00013490((double)(long)local_108);
                                  sVar2 = FUN_000132fc(&local_eb8,auStack_ee0);
                                  if (sVar2 != 0) {
                                    local_13a = 0;
                                    *(undefined2 *)(lVar5 + 0x90) = 0;
                                    *(undefined2 *)(lVar5 + 0x92) = 0;
                                  }
                                  if (local_13a != 0) {
                                    lVar9 = FUN_0016b1e0(auStack_c80);
                                    if (lVar9 != 0) {
                                      FUN_00013490(0x4000000000000000);
                                      local_ef0 = FUN_000157c8(&local_d38,auStack_ef8);
                                      plVar7 = (long *)FUN_0016b1f8(auStack_c80);
                                      local_f00 = (**(code **)(*plVar7 + 0x80))();
                                      local_ee8 = FUN_00017c4c(&local_ef0,&local_f00);
                                      local_d88 = local_ee8;
                                    }
                                    local_f10 = FUN_00028080(&local_d68,&local_d48);
                                    local_f08 = FUN_000157c8(&local_f10,&local_f8);
                                    local_130 = local_f08;
                                    local_f20 = FUN_00028080(&local_d88,&local_d68);
                                    local_f18 = FUN_000157c8(&local_f20,&local_f8);
                                    local_138 = local_f18;
                                    FUN_00013490((double)(long)local_108);
                                    local_f48 = FUN_00017c4c(auStack_f50,&local_138);
                                    local_f40 = FUN_00028080(&local_f48,&local_118);
                                    local_f38 = FUN_00028080(&local_f40,&local_120);
                                    local_f58 = FUN_00017c4c(&local_138,&local_130);
                                    local_f30 = FUN_000157c8(&local_f38,&local_f58);
                                    local_f28 = FUN_00015abc(&local_f30);
                                    local_128 = local_f28;
                                  }
                                }
                              }
                              else {
                                FUN_00013490((double)(long)local_108);
                                local_e08 = FUN_00017c4c(auStack_e10,&local_138);
                                local_e18 = FUN_00017c4c(&local_138,&local_130);
                                local_e00 = FUN_000157c8(&local_e08,&local_e18);
                                local_df8 = FUN_00015abc(&local_e00);
                                local_128 = local_df8;
                                FUN_00013490(0);
                                sVar2 = FUN_00018504(&local_120,auStack_e20);
                                if (sVar2 != 0) {
                                  local_e30 = FUN_00015a7c(&local_128,&local_130);
                                  FUN_00013490(0x3ff0000000000000);
                                  local_e40 = FUN_00028080(&local_128,auStack_e48);
                                  local_e38 = FUN_00015a7c(&local_e40,&local_138);
                                  local_e28 = FUN_00017c4c(&local_e30,&local_e38);
                                  FUN_00013490((double)(long)local_108);
                                  sVar2 = FUN_000132fc(&local_e28,auStack_e50);
                                  if (sVar2 != 0) {
                                    local_13a = 0;
                                    *(undefined2 *)(lVar5 + 0x90) = 0;
                                    *(undefined2 *)(lVar5 + 0x92) = 0;
                                  }
                                  if (local_13a != 0) {
                                    FUN_00013490((double)(long)local_108);
                                    local_e70 = FUN_00017c4c(auStack_e78,&local_138);
                                    local_e68 = FUN_00028080(&local_e70,&local_120);
                                    local_e80 = FUN_00017c4c(&local_138,&local_130);
                                    local_e60 = FUN_000157c8(&local_e68,&local_e80);
                                    local_e58 = FUN_00015abc(&local_e60);
                                    local_128 = local_e58;
                                  }
                                }
                              }
                            }
                            __ZN8PMStringD1Ev(auStack_d28);
                          }
                          else {
                            plVar7 = (long *)FUN_00017c34(auStack_c8);
                            local_ce0 = (**(code **)(*plVar7 + 0x100))();
                            local_128 = local_ce0;
                            plVar7 = (long *)FUN_00017c34(auStack_c8);
                            local_13a = (**(code **)(*plVar7 + 0x130))();
                            *(short *)(lVar5 + 0x90) = local_13a;
                            *(short *)(lVar5 + 0x92) = local_13a;
                            FUN_001cbca4(auStack_30,local_a8,&DAT_00208c50,&local_130,&local_138);
                          }
                          if (local_13a == 0) {
                            FUN_00002cf8(auStack_10cc,0x15d4c9);
                            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1118,"rows",0);
                            FUN_00013490((double)(long)local_108);
                            local_1120 = FUN_00015a7c(auStack_1128,&local_f8);
                            FUN_00013490();
                            FUN_00013490(0);
                            FUN_001668f4(lVar5,auStack_10cc,auStack_1118,&local_1120,&local_f8,
                                         auStack_1130,auStack_1138);
                            __ZN8PMStringD1Ev(auStack_1118);
                            FUN_00002cf8(auStack_113c,0x15d4c9);
                            local_1140 = 0;
                            FUN_00015b04(auStack_30,auStack_113c,&local_1140,1);
                          }
                          plVar7 = (long *)FUN_0016b1f8(auStack_c60);
                          local_1144 = (**(code **)(*plVar7 + 0xa0))();
                          FUN_000c535c(auStack_1150);
                          plVar7 = (long *)FUN_000c5388(auStack_1150);
                          local_1154 = local_1144;
                          uVar6 = FUN_0000df80(auStack_70);
                          uVar3 = (**(code **)(*plVar7 + 0x38))(plVar7,local_1154,uVar6);
                          FUN_000c53a0(auStack_1150);
                          local_1148 = uVar3;
                          plVar7 = (long *)FUN_0000e2c8(auStack_c0);
                          (**(code **)(*plVar7 + 0x80))(plVar7,&local_1148);
                          local_1160 = local_130;
                          local_1168 = local_138;
                          local_116c = 3;
                          FUN_000152f0(&local_130,&local_116c);
                          local_1170 = 3;
                          FUN_000152f0(&local_138,&local_1170);
                          __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_11b8,"",0);
                          local_11c0 = FUN_00015abc(&local_128);
                          __ZN8PMString12AppendNumberERK6PMRealiss(auStack_11b8,&local_11c0,0,1);
                          __ZN8PMString6AppendEPKciNS_14StringEncodingE
                                    (auStack_11b8," ",0x7fffffff,0xffffffff);
                          __ZN8PMString6AppendEPKciNS_14StringEncodingE
                                    (auStack_11b8,"rows",0x7fffffff,0xffffffff);
                          __ZN8PMString6AppendEPKciNS_14StringEncodingE
                                    (auStack_11b8," (",0x7fffffff,0xffffffff);
                          local_11c8 = FUN_00015abc(&local_130);
                          __ZN8PMString12AppendNumberERK6PMRealiss(auStack_11b8,&local_11c8,0,1);
                          __ZN8PMString6AppendEPKciNS_14StringEncodingE
                                    (auStack_11b8," lines);",0x7fffffff,0xffffffff);
                          __ZN8PMString6AppendEPKciNS_14StringEncodingE
                                    (auStack_11b8," gutter: ",0x7fffffff,0xffffffff);
                          local_11d0 = FUN_00015abc(&local_138);
                          __ZN8PMString12AppendNumberERK6PMRealiss(auStack_11b8,&local_11d0,0,1);
                          __ZN8PMString6AppendEPKciNS_14StringEncodingE
                                    (auStack_11b8," ",0x7fffffff,0xffffffff);
                          local_11d8 = FUN_00015abc(&local_138);
                          FUN_00013490(0x3ff0000000000000);
                          sVar2 = FUN_00014140(&local_11d8,auStack_11e0);
                          bVar1 = true;
                          if (sVar2 == 0) {
                            local_11e8 = FUN_00015abc(&local_138);
                            FUN_00013490(0);
                            sVar2 = FUN_000132fc(&local_11e8,auStack_11f0);
                            bVar1 = sVar2 != 0;
                          }
                          if (bVar1) {
                            __ZN8PMString6AppendEPKciNS_14StringEncodingE
                                      (auStack_11b8,"lines",0x7fffffff,0xffffffff);
                          }
                          else {
                            __ZN8PMString6AppendEPKciNS_14StringEncodingE
                                      (auStack_11b8,"line",0x7fffffff,0xffffffff);
                          }
                          __ZN8PMString15SetTranslatableEs(auStack_11b8,0);
                          FUN_00002cf8(auStack_11f4,0x15d4c9);
                          FUN_000191b0(auStack_30,auStack_11f4,auStack_11b8,1);
                          plVar7 = (long *)FUN_00017c34(auStack_c8);
                          local_11f6 = (**(code **)(*plVar7 + 0x40))();
                          FUN_00002cf8(auStack_1204,0x15d365);
                          local_1200 = FUN_00013338(auStack_30,auStack_1204);
                          local_1299 = 0;
                          __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                                    (auStack_1250,"[GC] Smart Setup");
                          sVar2 = FUN_001a6604(auStack_1250);
                          bVar1 = true;
                          if (sVar2 == 0) {
                            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1298,"[GC] CGS",0);
                            local_1299 = 1;
                            sVar2 = FUN_001a6604(auStack_1298);
                            bVar1 = sVar2 != 0;
                          }
                          if ((local_1299 & 1) != 0) {
                            __ZN8PMStringD1Ev(auStack_1298);
                          }
                          __ZN8PMStringD1Ev(auStack_1250);
                          if (bVar1) {
                            FUN_00013490(0);
                            sVar2 = FUN_00018504(&local_5b0,auStack_12a8);
                            if (sVar2 == 0) {
                              FUN_00002cf8(auStack_1334,0x15d37a);
                              __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1380,"rows",0);
                              FUN_00013490((double)(long)local_108);
                              local_1398 = FUN_00028080(auStack_13a0,&local_118);
                              local_1390 = FUN_00028080(&local_1398,&local_120);
                              local_1388 = FUN_00015a7c(&local_1390,&local_f8);
                              FUN_00013490();
                              FUN_00013490(0);
                              FUN_001668f4(lVar5,auStack_1334,auStack_1380,&local_1388,&local_f8,
                                           auStack_13a8,auStack_13b0);
                              __ZN8PMStringD1Ev(auStack_1380);
                            }
                            else {
                              FUN_00002cf8(auStack_12ac,0x15d37a);
                              __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_12f8,"rows",0);
                              FUN_00013490((double)(long)local_108);
                              local_1318 = FUN_00028080(auStack_1320,&local_118);
                              local_1310 = FUN_00028080(&local_1318,&local_120);
                              local_1308 = FUN_00015a7c(&local_1310,&local_f8);
                              local_1300 = FUN_00028080(&local_1308,&local_5b0);
                              local_1328 = FUN_00028080(&local_f8,&local_5b0);
                              FUN_00013490(0);
                              FUN_001668f4(lVar5,auStack_12ac,auStack_12f8,&local_1300,&local_f8,
                                           &local_1328,auStack_1330);
                              __ZN8PMStringD1Ev(auStack_12f8);
                            }
                            FUN_00002cf8(auStack_13b4,0x15d37a);
                            FUN_000191b0(auStack_30,auStack_13b4,auStack_11b8,1);
                            FUN_00002cf8(auStack_13b8,0x15d37a);
                            iVar4 = FUN_00013034(auStack_30,auStack_13b8);
                            if (iVar4 == -1) {
                              FUN_00002cf8(auStack_13bc,0x15d37a);
                              local_13c0 = 0;
                              FUN_00015b04(auStack_30,auStack_13bc,&local_13c0,1);
                            }
                            FUN_00013490(0);
                            sVar2 = FUN_00014140(&local_128,auStack_13c8);
                            if (sVar2 != 0) {
                              FUN_00002cf8(auStack_13cc,0x15d369);
                              FUN_00013c20(auStack_30,auStack_13cc,&local_128);
                              FUN_00002cf8(auStack_13d0,0x15d36b);
                              local_13e8 = FUN_00015abc(&local_130);
                              local_13e0 = FUN_00015a7c(&local_13e8,&local_1200);
                              local_13d8 = FUN_00017c4c(&local_13e0,&local_5a8);
                              FUN_00013c20(auStack_30,auStack_13d0,&local_13d8);
                              FUN_00002cf8(auStack_13ec,0x15d368);
                              local_1408 = FUN_00015abc(&local_138);
                              local_1400 = FUN_00015a7c(&local_1408,&local_1200);
                              local_1410 = FUN_00015a7c(&local_5b0,lVar5 + 0x88);
                              local_13f8 = FUN_00017c4c(&local_1400,&local_1410);
                              FUN_00013c20(auStack_30,auStack_13ec,&local_13f8);
                              plVar7 = (long *)FUN_000029d8(auStack_50);
                              local_1418 = FUN_00015abc(&local_130);
                              (**(code **)(*plVar7 + 0x280))(plVar7,&local_1418);
                              plVar7 = (long *)FUN_000029d8(auStack_50);
                              local_1420 = FUN_00015abc(&local_138);
                              (**(code **)(*plVar7 + 0x290))(plVar7,&local_1420);
                            }
                            plVar7 = (long *)FUN_00017c34(auStack_c8);
                            local_1428 = (**(code **)(*plVar7 + 0x50))();
                            FUN_00013490(0);
                            sVar2 = FUN_00014140(&local_1428,auStack_1430);
                            bVar1 = true;
                            if (sVar2 == 0) {
                              plVar7 = (long *)FUN_00017c34(auStack_c8);
                              local_1438 = (**(code **)(*plVar7 + 0x60))();
                              FUN_00013490(0);
                              sVar2 = FUN_00014140(&local_1438,auStack_1440);
                              bVar1 = sVar2 != 0;
                            }
                            if (bVar1) {
                              FUN_00002cf8(auStack_1444,0x15d369);
                              FUN_00011dc4(auStack_30,auStack_1444,&DAT_00208c50);
                            }
                            else {
                              local_1446 = 0;
                              FUN_00002cf8(auStack_144c,&DAT_0015d376);
                              FUN_00011dc4(auStack_30,auStack_144c,&DAT_00208c50);
                              FUN_00002cf8(auStack_1450,&DAT_0015d378);
                              FUN_00011dc4(auStack_30,auStack_1450,&DAT_00208c50);
                              FUN_00013490(0);
                              sVar2 = FUN_00014140(&local_128,auStack_1458);
                              if (sVar2 != 0) {
                                local_1446 = 1;
                                FUN_00002cf8(auStack_145c,0x15d36d);
                                local_1470 = FUN_00015a7c(&local_118,&local_1200);
                                local_1478 = FUN_00015a7c(&local_5b0,lVar5 + 0x88);
                                local_1468 = FUN_00017c4c(&local_1470,&local_1478);
                                FUN_00013c20(auStack_30,auStack_145c,&local_1468);
                                FUN_00002cf8(auStack_147c,0x15d36f);
                                local_1488 = FUN_00015a7c(&local_120,&local_1200);
                                FUN_00013c20(auStack_30,auStack_147c,&local_1488);
                                plVar7 = (long *)FUN_000029d8(auStack_50);
                                (**(code **)(*plVar7 + 0x2a0))(plVar7,&local_118);
                                plVar7 = (long *)FUN_000029d8(auStack_50);
                                (**(code **)(*plVar7 + 0x2b0))(plVar7,&local_120);
                              }
                              bVar1 = true;
                              if ((local_1446 != 0) && (bVar1 = false, local_1446 != 0)) {
                                FUN_00013490(0x3ff0000000000000);
                                sVar2 = FUN_00031d38(&local_100,auStack_1490);
                                bVar1 = sVar2 != 0;
                              }
                              if (bVar1) {
                                FUN_00002cf8(auStack_1494,0x15d36c);
                                FUN_00011dc4(auStack_30,auStack_1494,&local_1446);
                                FUN_00002cf8(auStack_1498,0x15d36e);
                                FUN_00011dc4(auStack_30,auStack_1498,&local_1446);
                                FUN_00002cf8(auStack_149c,0x15d36a);
                                FUN_00011dc4(auStack_30,auStack_149c,&local_1446);
                                FUN_00002cf8(auStack_14a0,0x15d367);
                                FUN_00011dc4(auStack_30,auStack_14a0,&local_1446);
                              }
                            }
                          }
                          FUN_00002cf8(auStack_14a4,0x15d4c9);
                          iVar4 = FUN_00013034(auStack_30,auStack_14a4);
                          if (iVar4 < 1) {
                            FUN_00002cf8(auStack_14c0,0x15d4c9);
                            iVar4 = FUN_00013034(auStack_30,auStack_14c0);
                            if (iVar4 == -1) {
LAB_0017bf58:
                              FUN_00013490(0);
                              sVar2 = FUN_00014140(&local_128,auStack_14d0);
                              bVar1 = sVar2 != 0;
                            }
                            else {
                              FUN_00002cf8(auStack_14c4,0x15d4c9);
                              iVar4 = FUN_00013034(auStack_30,auStack_14c4);
                              bVar1 = false;
                              if (iVar4 == 0) goto LAB_0017bf58;
                            }
                            if (bVar1) {
                              FUN_00002cf8(auStack_14d4,0x15d3fb);
                              FUN_00011dc4(auStack_30,auStack_14d4,&DAT_00208c52);
                              FUN_00002cf8(auStack_14d8,0x15d3fd);
                              FUN_00011dc4(auStack_30,auStack_14d8,&DAT_00208c52);
                              FUN_00002cf8(auStack_14dc,0x15d4c9);
                              __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                                        (auStack_1528,"Custom Setting Applied",0);
                              FUN_000191b0(auStack_30,auStack_14dc,auStack_1528,1);
                              __ZN8PMStringD1Ev(auStack_1528);
                            }
                          }
                          else {
                            FUN_00002cf8(auStack_14a8,0x15d3fe);
                            FUN_00011dc4(auStack_30,auStack_14a8,&DAT_00208c52);
                            FUN_00002cf8(auStack_14ac,0x15d3ff);
                            FUN_00011dc4(auStack_30,auStack_14ac,&DAT_00208c52);
                            FUN_00002cf8(auStack_14b0,0x15d3fe);
                            local_14b4 = 0;
                            FUN_00015b04(auStack_30,auStack_14b0,&local_14b4,1);
                            FUN_00002cf8(auStack_14b8,0x15d3fb);
                            FUN_00011dc4(auStack_30,auStack_14b8,&DAT_00208c52);
                            FUN_00002cf8(auStack_14bc,0x15d3fd);
                            FUN_00011dc4(auStack_30,auStack_14bc,&DAT_00208c52);
                          }
                          if (local_13a == 0) {
                            FUN_00002cf8(auStack_152c,0x15d4cb);
                            FUN_00013178(auStack_30,auStack_152c,&DAT_00208c52);
                            FUN_00002cf8(auStack_1530,0x15d4e1);
                            FUN_00013178(auStack_30,auStack_1530,&DAT_00208c52);
                            FUN_00002cf8(auStack_1534,0x15d4ca);
                            FUN_00013178(auStack_30,auStack_1534,&DAT_00208c50);
                            FUN_00002cf8(auStack_1538,0x15d4e0);
                            FUN_00013178(auStack_30,auStack_1538,&DAT_00208c50);
                          }
                          else {
                            FUN_00002cf8(auStack_153c,0x15d4ca);
                            FUN_00013178(auStack_30,auStack_153c,&DAT_00208c52);
                            FUN_00002cf8(auStack_1540,0x15d4e0);
                            FUN_00013178(auStack_30,auStack_1540,&DAT_00208c52);
                            FUN_00002cf8(auStack_1544,0x15d4cb);
                            FUN_00013178(auStack_30,auStack_1544,&DAT_00208c50);
                            FUN_00002cf8(auStack_1548,0x15d4e1);
                            FUN_00013178(auStack_30,auStack_1548,&DAT_00208c50);
                          }
                          FUN_00002cf8(auStack_154c,0x15d4b3);
                          FUN_00013c20(auStack_30,auStack_154c,&local_128);
                          FUN_00013490(0);
                          sVar2 = FUN_00018504(&local_5b0,auStack_1558);
                          bVar1 = false;
                          if (sVar2 != 0) {
                            FUN_00002cf8(auStack_155c,0x15d4c9);
                            iVar4 = FUN_00013034(auStack_30,auStack_155c);
                            bVar1 = 0 < iVar4;
                          }
                          if (bVar1) {
                            FUN_00013490(&local_1568);
                            FUN_00013490(0,&local_1570);
                            local_1588 = FUN_00015a7c(&local_1160,&local_f8);
                            local_1580 = FUN_00015a7c(&local_1588,lVar5 + 0x88);
                            local_1578 = FUN_00017c4c(&local_1580,&local_5a8);
                            local_1568 = local_1578;
                            local_15a0 = FUN_00015a7c(&local_1168,&local_f8);
                            local_1598 = FUN_00015a7c(&local_15a0,lVar5 + 0x88);
                            local_15a8 = FUN_00015a7c(&local_5b0,lVar5 + 0x88);
                            local_1590 = FUN_00017c4c(&local_1598,&local_15a8);
                            local_1570 = local_1590;
                            FUN_00002cf8(auStack_15ac,0x15d4b4);
                            FUN_00013c20(auStack_30,auStack_15ac,&local_1568);
                            FUN_00002cf8(auStack_15b0,0x15d4b5);
                            FUN_00013c20(auStack_30,auStack_15b0,&local_1570);
                            local_1641 = 0;
                            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                                      (auStack_15f8,"[GC] Smart Setup");
                            sVar2 = FUN_001a6604(auStack_15f8);
                            bVar1 = true;
                            if (sVar2 == 0) {
                              __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                                        (auStack_1640,"[GC] CGS",0);
                              local_1641 = 1;
                              sVar2 = FUN_001a6604(auStack_1640);
                              bVar1 = sVar2 != 0;
                            }
                            if ((local_1641 & 1) != 0) {
                              __ZN8PMStringD1Ev(auStack_1640);
                            }
                            __ZN8PMStringD1Ev(auStack_15f8);
                            if (bVar1) {
                              FUN_00002cf8(auStack_1648,0x15d36b);
                              FUN_00013c20(auStack_30,auStack_1648,&local_1568);
                              FUN_00002cf8(auStack_164c,0x15d368);
                              FUN_00013c20(auStack_30,auStack_164c,&local_1570);
                            }
                          }
                          else {
                            FUN_00013490(0);
                            sVar2 = FUN_00014140(&local_1200,auStack_1658);
                            if (sVar2 == 0) {
                              FUN_00002cf8(auStack_167c,0x15d4b4);
                              local_1690 = FUN_00015a7c(&local_1160,&local_f8);
                              local_1688 = FUN_00015a7c(&local_1690,lVar5 + 0x88);
                              FUN_00013c20(auStack_30,auStack_167c,&local_1688);
                              FUN_00002cf8(auStack_1694,0x15d4b5);
                              local_16a8 = FUN_00015a7c(&local_1168,&local_f8);
                              local_16a0 = FUN_00015a7c(&local_16a8,lVar5 + 0x88);
                              FUN_00013c20(auStack_30,auStack_1694,&local_16a0);
                            }
                            else {
                              FUN_00002cf8(auStack_165c,0x15d4b4);
                              local_1668 = FUN_00015a7c(&local_1160,&local_1200);
                              FUN_00013c20(auStack_30,auStack_165c,&local_1668);
                              FUN_00002cf8(auStack_166c,0x15d4b5);
                              local_1678 = FUN_00015a7c(&local_1168,&local_1200);
                              FUN_00013c20(auStack_30,auStack_166c,&local_1678);
                            }
                          }
                          __ZN8PMStringD1Ev(auStack_11b8);
                          __ZN8PMStringD1Ev(auStack_cd0);
                          local_48 = 0;
                        }
                        else {
LAB_00179800:
                          local_48 = 2;
                        }
                        FUN_0016bd84(auStack_c80);
                        FUN_0016bd84(auStack_c70);
                        FUN_0016bd84(auStack_c60);
                      }
                      else {
                        local_48 = 2;
                      }
                      FUN_000cad2c(auStack_c50);
                      FUN_000caad8(auStack_c48);
                    }
                    else {
                      local_48 = 2;
                    }
                    FUN_000caa48(auStack_c40);
                  }
                  else {
                    local_48 = 2;
                  }
                  __ZN8PMStringD1Ev(auStack_c28);
                  __ZN8PMStringD1Ev(auStack_be0);
                  __ZN8PMStringD1Ev(auStack_700);
                  __ZN8PMStringD1Ev(auStack_6b0);
                  __ZN8PMStringD1Ev(auStack_660);
                  __ZN8PMStringD1Ev(auStack_600);
                  __ZN8PMStringD1Ev(auStack_188);
                }
                else {
                  local_48 = 2;
                }
              }
              else {
                local_48 = 2;
              }
              FUN_0000e040(auStack_d8);
            }
            else {
              local_48 = 2;
            }
            FUN_00017ca4(auStack_c8);
          }
          else {
            local_48 = 2;
          }
          FUN_0000e2f8(auStack_c0);
        }
        else {
          local_48 = 2;
        }
        FUN_0001d138(auStack_b0);
      }
      else {
        local_48 = 2;
      }
      FUN_0001d190(auStack_80);
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
LAB_0017c7b4:
  FUN_00002c54(auStack_30);
  return;
}

//==== FUNC @ 17edb4 -> 0017e768

void FUN_0017e768(long param_1)

{
  bool bVar1;
  short sVar2;
  undefined4 uVar3;
  int iVar4;
  undefined8 uVar5;
  long *plVar6;
  ulong uVar7;
  undefined8 local_a20;
  undefined8 local_a18;
  undefined1 auStack_a0c [4];
  undefined1 auStack_a08 [8];
  undefined8 local_a00;
  undefined8 local_9f8;
  undefined1 auStack_9ec [4];
  undefined1 auStack_9e8 [4];
  undefined1 auStack_9e4 [4];
  undefined8 local_9e0;
  undefined8 local_9d8;
  undefined8 local_9d0;
  undefined8 local_9c8;
  undefined8 local_9c0;
  undefined8 local_9b8;
  undefined8 local_9b0;
  undefined8 local_9a8;
  undefined8 local_9a0;
  undefined1 auStack_994 [4];
  undefined1 auStack_990 [12];
  undefined1 auStack_984 [4];
  undefined1 auStack_980 [4];
  undefined1 auStack_97c [4];
  undefined1 auStack_978 [4];
  undefined1 auStack_974 [4];
  undefined1 auStack_970 [4];
  undefined1 auStack_96c [4];
  undefined1 auStack_968 [4];
  undefined1 auStack_964 [4];
  undefined1 auStack_960 [76];
  undefined1 auStack_914 [4];
  undefined1 auStack_910 [12];
  undefined1 auStack_904 [4];
  undefined1 auStack_900 [4];
  undefined1 auStack_8fc [4];
  undefined1 auStack_8f8 [8];
  undefined8 local_8f0;
  undefined1 auStack_8e8 [8];
  undefined8 local_8e0;
  undefined8 local_8d8;
  undefined8 local_8d0;
  undefined8 local_8c8;
  undefined1 auStack_8c0 [76];
  undefined4 local_874;
  undefined1 auStack_870 [8];
  undefined4 local_868;
  undefined4 local_864;
  undefined1 uStack_85d;
  undefined4 local_85c;
  undefined1 auStack_858 [15];
  undefined1 uStack_849;
  undefined1 auStack_848 [8];
  undefined1 auStack_840 [8];
  undefined1 auStack_838 [12];
  undefined4 local_82c;
  undefined4 local_828;
  undefined4 local_824;
  undefined1 auStack_820 [72];
  undefined4 local_7d8;
  undefined1 auStack_7d4 [4];
  undefined1 auStack_7d0 [8];
  undefined1 auStack_7c8 [8];
  undefined1 auStack_7c0 [76];
  undefined1 auStack_774 [4];
  undefined1 auStack_770 [8];
  undefined8 local_768;
  undefined8 local_760;
  undefined8 local_758;
  undefined8 local_750;
  undefined1 auStack_744 [4];
  undefined8 local_740;
  undefined1 auStack_738 [8];
  undefined8 local_730;
  undefined8 local_728;
  undefined8 local_720;
  undefined1 auStack_718 [76];
  undefined1 auStack_6cc [4];
  undefined8 local_6c8;
  undefined1 auStack_6c0 [8];
  undefined8 local_6b8;
  undefined1 auStack_6b0 [76];
  undefined1 auStack_664 [4];
  undefined1 auStack_660 [8];
  undefined8 local_658;
  undefined8 local_650;
  undefined8 local_648;
  undefined8 local_640;
  undefined8 local_638;
  undefined8 local_630;
  undefined8 local_628;
  undefined8 local_620;
  undefined8 local_618;
  undefined8 local_610;
  undefined1 auStack_608 [76];
  undefined1 auStack_5bc [4];
  undefined8 local_5b8;
  undefined1 auStack_5b0 [8];
  undefined1 auStack_5a8 [8];
  undefined1 auStack_5a0 [8];
  undefined1 auStack_598 [8];
  undefined8 local_590;
  undefined1 auStack_588 [76];
  undefined1 auStack_53c [4];
  undefined8 local_538;
  undefined1 auStack_530 [72];
  undefined8 local_4e8;
  undefined1 auStack_4dc [4];
  undefined8 local_4d8;
  undefined8 local_4d0;
  undefined4 local_4c4;
  undefined1 auStack_4c0 [76];
  undefined4 local_474;
  undefined1 auStack_470 [76];
  undefined4 local_424;
  undefined1 auStack_420 [72];
  undefined4 local_3d8;
  undefined1 auStack_3d4 [4];
  undefined1 auStack_3d0 [72];
  undefined1 auStack_388 [6];
  short local_382;
  undefined1 auStack_380 [6];
  short local_37a;
  undefined4 local_378;
  undefined1 auStack_374 [4];
  undefined1 auStack_370 [72];
  undefined8 local_328;
  undefined8 local_320;
  undefined8 local_318;
  undefined8 local_310;
  undefined1 auStack_308 [76];
  undefined1 auStack_2bc [4];
  undefined1 auStack_2b8 [76];
  undefined1 auStack_26c [4];
  undefined1 auStack_268 [76];
  undefined1 auStack_21c [4];
  undefined1 auStack_218 [4];
  undefined1 auStack_214 [4];
  undefined1 auStack_210 [4];
  undefined1 auStack_20c [4];
  undefined1 auStack_208 [4];
  undefined1 auStack_204 [4];
  undefined1 auStack_200 [4];
  undefined1 auStack_1fc [4];
  undefined8 local_1f8;
  undefined8 local_1f0;
  undefined8 local_1e8;
  undefined8 local_1e0;
  undefined8 local_1d8;
  undefined8 local_1d0;
  undefined1 auStack_1c4 [4];
  undefined8 local_1c0;
  undefined1 auStack_1b4 [4];
  undefined8 local_1b0;
  undefined1 auStack_1a4 [4];
  undefined8 local_1a0;
  undefined1 auStack_194 [4];
  undefined8 local_190;
  undefined4 local_184;
  undefined1 auStack_180 [78];
  short local_132;
  undefined1 auStack_130 [8];
  undefined1 auStack_128 [8];
  undefined8 local_120;
  undefined8 local_118;
  undefined8 local_110;
  undefined1 auStack_108 [12];
  int local_fc;
  undefined8 local_f8;
  undefined1 auStack_ec [4];
  undefined8 local_e8;
  undefined1 uStack_d9;
  undefined1 auStack_d8 [15];
  undefined1 uStack_c9;
  undefined1 auStack_c8 [8];
  undefined1 auStack_c0 [15];
  undefined1 uStack_b1;
  undefined1 local_b0 [16];
  undefined1 auStack_a0 [15];
  undefined1 uStack_91;
  undefined1 auStack_90 [8];
  undefined1 local_88 [16];
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
    goto LAB_00180804;
  }
  uVar5 = __Z26GetExecutionContextSessionv();
  FUN_00002978(auStack_50,uVar5,&uStack_51);
  sVar2 = FUN_000029b4(auStack_50);
  if (sVar2 == 0) {
    plVar6 = (long *)FUN_000029d8(auStack_50);
    local_68 = (**(code **)(*plVar6 + 0x28))();
    FUN_0001b840(auStack_70,local_68,&uStack_71);
    sVar2 = FUN_0001b87c(auStack_70);
    if (sVar2 == 0) {
      plVar6 = (long *)FUN_000029d8(auStack_50);
      local_88 = (**(code **)(*plVar6 + 0x18))();
      FUN_0000df20(auStack_90,local_88,&uStack_91);
      sVar2 = FUN_0000df5c(auStack_90);
      if (sVar2 == 0) {
        plVar6 = (long *)FUN_0001b750(auStack_90);
        local_b0 = (**(code **)(*plVar6 + 0x88))();
        FUN_0001b768(auStack_a0,local_b0,&uStack_b1);
        sVar2 = FUN_0001b7a4(auStack_a0);
        if (sVar2 == 0) {
          FUN_001a2484(auStack_c0);
          sVar2 = FUN_0000e2a4(auStack_c0);
          if (sVar2 == 0) {
            FUN_00017bd4(auStack_c8,local_68,&uStack_c9);
            sVar2 = FUN_00017c10(auStack_c8);
            if (sVar2 == 0) {
              uVar5 = FUN_0000df80(auStack_90);
              FUN_0000df98(auStack_d8,uVar5,&uStack_d9);
              sVar2 = FUN_0000dfd4(auStack_d8);
              if (sVar2 == 0) {
                FUN_00002cf8(auStack_ec,0x15d31d);
                local_f8 = FUN_00013338(auStack_30,auStack_ec);
                local_e8 = local_f8;
                plVar6 = (long *)FUN_000029d8(auStack_50);
                local_fc = (**(code **)(*plVar6 + 0xf8))();
                FUN_00013490(0);
                sVar2 = FUN_000132fc(&local_f8,auStack_108);
                if (sVar2 == 0 && local_fc != 0) {
                  FUN_00015a50(&local_110);
                  FUN_00015a50(&local_118);
                  FUN_00015a50(&local_120);
                  FUN_00015a50(auStack_128);
                  FUN_00015a50(auStack_130);
                  local_132 = 0;
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_180,"[GC] Break Vertical",0);
                  plVar6 = (long *)FUN_0000dff8(auStack_d8);
                  local_184 = (**(code **)(*plVar6 + 0x20))(plVar6,auStack_180);
                  uVar7 = FUN_00002e24(&local_184,&DAT_00208c58);
                  if ((uVar7 & 1) == 0) {
                    FUN_00002cf8(auStack_1b4,0x15d49b);
                    local_1b0 = FUN_00013338(auStack_30,auStack_1b4);
                    FUN_00002cf8(auStack_1c4,0x15d49d);
                    local_1c0 = FUN_00013338(auStack_30,auStack_1c4);
                    local_1e0 = FUN_000157c8(&local_1b0,param_1 + 0x88);
                    local_1d8 = FUN_000157c8(&local_1e0,&local_f8);
                    local_1d0 = FUN_00015abc(&local_1d8);
                    local_110 = local_1d0;
                    local_1f8 = FUN_000157c8(&local_1c0,param_1 + 0x88);
                    local_1f0 = FUN_000157c8(&local_1f8,&local_f8);
                    local_1e8 = FUN_00015abc(&local_1f0);
                    local_118 = local_1e8;
                  }
                  else {
                    FUN_00002cf8(auStack_194,0x15d49a);
                    local_190 = FUN_00013338(auStack_30,auStack_194);
                    local_110 = local_190;
                    FUN_00002cf8(auStack_1a4,0x15d49c);
                    local_1a0 = FUN_00013338(auStack_30,auStack_1a4);
                    local_118 = local_1a0;
                  }
                  FUN_00002cf8(auStack_1fc,0x15d4cf);
                  FUN_00011dc4(auStack_30,auStack_1fc,&DAT_00208c52);
                  FUN_00002cf8(auStack_200,0x15d4d6);
                  FUN_00011dc4(auStack_30,auStack_200,&DAT_00208c52);
                  FUN_00002cf8(auStack_204,0x15d4d0);
                  FUN_00011dc4(auStack_30,auStack_204,&DAT_00208c52);
                  FUN_00002cf8(auStack_208,0x15d4d1);
                  FUN_00011dc4(auStack_30,auStack_208,&DAT_00208c52);
                  FUN_00002cf8(auStack_20c,0x15d4bb);
                  FUN_00011dc4(auStack_30,auStack_20c,&DAT_00208c52);
                  FUN_00002cf8(auStack_210,0x15d4bd);
                  FUN_00011dc4(auStack_30,auStack_210,&DAT_00208c52);
                  FUN_00002cf8(auStack_214,0x15d4c0);
                  FUN_00011dc4(auStack_30,auStack_214,&DAT_00208c52);
                  FUN_00002cf8(auStack_218,0x15d4c1);
                  FUN_00011dc4(auStack_30,auStack_218,&DAT_00208c52);
                  FUN_00002cf8(auStack_21c,0x15d4bb);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_268,"",0);
                  FUN_00013588(auStack_30,auStack_21c,auStack_268);
                  __ZN8PMStringD1Ev(auStack_268);
                  FUN_00002cf8(auStack_26c,0x15d4bc);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_2b8,"",0);
                  FUN_00013588(auStack_30,auStack_26c,auStack_2b8);
                  __ZN8PMStringD1Ev(auStack_2b8);
                  FUN_00002cf8(auStack_2bc,0x15d4bd);
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_308,"",0);
                  FUN_00013588(auStack_30,auStack_2bc,auStack_308);
                  __ZN8PMStringD1Ev(auStack_308);
                  FUN_00013490(&local_310);
                  FUN_00013490(0,&local_318);
                  FUN_00013490(0,&local_320);
                  FUN_00013490(0,&local_328);
                  FUN_00002cf8(auStack_374,0x15d306);
                  local_378 = 0xffffffff;
                  FUN_00012c10(auStack_370,auStack_30,auStack_374,&local_378);
                  FUN_00002cf8(auStack_380,0x15d323);
                  local_37a = FUN_00012ef4(auStack_30,auStack_380);
                  FUN_00002cf8(auStack_388,0x15d4a3);
                  local_382 = FUN_00012ef4(auStack_30,auStack_388);
                  FUN_00002cf8(auStack_3d4,0x15d309);
                  local_3d8 = 0xffffffff;
                  FUN_00012c10(auStack_3d0,auStack_30,auStack_3d4,&local_3d8);
                  __ZN8PMStringC1ERKS_(auStack_420,auStack_3d0);
                  __ZN8PMString6AppendEPKciNS_14StringEncodingE
                            (auStack_420," Edit Existing",0x7fffffff,0xffffffff);
                  plVar6 = (long *)FUN_0000dff8(auStack_d8);
                  local_424 = (**(code **)(*plVar6 + 0x20))(plVar6,auStack_420);
                  __ZN8PMStringC1ERKS_(auStack_470,auStack_3d0);
                  __ZN8PMString6AppendEPKciNS_14StringEncodingE
                            (auStack_470," Break Horizontal",0x7fffffff,0xffffffff);
                  plVar6 = (long *)FUN_0000dff8(auStack_d8);
                  local_474 = (**(code **)(*plVar6 + 0x20))(plVar6,auStack_470);
                  __ZN8PMStringC1ERKS_(auStack_4c0,auStack_3d0);
                  __ZN8PMString6AppendEPKciNS_14StringEncodingE
                            (auStack_4c0," Image-lines",0x7fffffff,0xffffffff);
                  plVar6 = (long *)FUN_0000dff8(auStack_d8);
                  local_4c4 = (**(code **)(*plVar6 + 0x20))(plVar6,auStack_4c0);
                  uVar7 = FUN_0000e010(&local_4c4,&DAT_00208c58);
                  if ((uVar7 & 1) != 0) {
                    plVar6 = (long *)FUN_00017c34(auStack_c8);
                    local_4d0 = (**(code **)(*plVar6 + 0x2c0))();
                    local_318 = local_4d0;
                    FUN_00002cf8(auStack_4dc,0x15d3a8);
                    local_4d8 = FUN_00013338(auStack_30,auStack_4dc);
                    local_310 = local_4d8;
                  }
                  plVar6 = (long *)FUN_00017c34(auStack_c8);
                  local_4e8 = (**(code **)(*plVar6 + 400))();
                  local_120 = local_4e8;
                  FUN_001cbca4(auStack_30,local_68,&DAT_00208c52,auStack_128,auStack_130);
                  plVar6 = (long *)FUN_00017c34(auStack_c8);
                  local_132 = (**(code **)(*plVar6 + 0x1c0))();
                  uVar7 = FUN_0000e010(&local_424,&DAT_00208c58);
                  if ((uVar7 & 1) == 0) {
                    *(undefined2 *)(param_1 + 0x94) = 1;
                    *(undefined2 *)(param_1 + 0x96) = 1;
                  }
                  else {
                    *(short *)(param_1 + 0x94) = local_132;
                    *(short *)(param_1 + 0x96) = local_132;
                  }
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_530,"[GC-1]",0);
                  sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_3d0,auStack_530,1,0);
                  __ZN8PMStringD1Ev(auStack_530);
                  if (sVar2 == 0) {
                    plVar6 = (long *)FUN_000029d8(auStack_50);
                    local_538 = (**(code **)(*plVar6 + 0x1c8))();
                    local_320 = local_538;
                  }
                  if (local_132 == 0) {
                    FUN_00002cf8(auStack_53c,0x15d4cf);
                    __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_588,"rows",0);
                    FUN_00013490((double)(long)local_fc);
                    local_590 = FUN_00015a7c(auStack_598,&local_f8);
                    FUN_00013490();
                    FUN_00013490(0);
                    FUN_001668f4(param_1,auStack_53c,auStack_588,&local_590,&local_f8,auStack_5a0,
                                 auStack_5a8);
                    __ZN8PMStringD1Ev(auStack_588);
                  }
                  else {
                    FUN_00013490(0);
                    sVar2 = FUN_00018504(&local_318,auStack_5b0);
                    if (sVar2 == 0) {
                      if ((local_382 == 0) || (local_37a == 0)) {
                        FUN_00013490((double)(long)local_fc);
                        local_768 = FUN_00028080(auStack_770,&local_110);
                        local_760 = FUN_00028080(&local_768,&local_118);
                        local_758 = FUN_00015a7c(&local_760,&local_f8);
                        local_750 = FUN_00028080(&local_758,&local_320);
                        local_328 = local_750;
                      }
                      else {
                        FUN_00002cf8(auStack_6cc,0x15d4a9);
                        local_6c8 = FUN_0001544c(auStack_30,auStack_6cc,auStack_370);
                        local_328 = local_6c8;
                        __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_718,"[GC-1]",0);
                        sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_3d0,auStack_718,1,0);
                        __ZN8PMStringD1Ev(auStack_718);
                        if (sVar2 != 0) {
                          local_728 = FUN_000157c8(&local_328,&local_f8);
                          local_720 = FUN_00015abc(&local_728);
                          local_730 = FUN_00015a7c(&local_720,&local_f8);
                          local_328 = local_730;
                        }
                        FUN_00013490(0x3f50624dd2f1a9fc);
                        sVar2 = FUN_00031d38(&local_328,auStack_738);
                        if (sVar2 != 0) {
                          FUN_00002cf8(auStack_744,&DAT_0015d31b);
                          local_740 = FUN_0001544c(auStack_30,auStack_744,auStack_370);
                          local_328 = local_740;
                        }
                      }
                      FUN_00002cf8(auStack_774,0x15d4cf);
                      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_7c0,"rows",0);
                      FUN_00013490();
                      FUN_00013490(0);
                      FUN_001668f4(param_1,auStack_774,auStack_7c0,&local_328,&local_f8,auStack_7c8,
                                   auStack_7d0);
                      __ZN8PMStringD1Ev(auStack_7c0);
                    }
                    else {
                      if ((local_382 == 0) || (local_37a == 0)) {
                        FUN_00013490((double)(long)local_fc);
                        local_658 = FUN_00028080(auStack_660,&local_110);
                        local_650 = FUN_00028080(&local_658,&local_118);
                        local_648 = FUN_00015a7c(&local_650,&local_f8);
                        local_640 = FUN_00028080(&local_648,&local_318);
                        local_638 = FUN_00028080(&local_640,&local_320);
                        local_328 = local_638;
                      }
                      else {
                        FUN_00002cf8(auStack_5bc,0x15d4a9);
                        local_5b8 = FUN_0001544c(auStack_30,auStack_5bc,auStack_370);
                        local_328 = local_5b8;
                        __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_608,"[GC-1]",0);
                        sVar2 = __ZNK8PMString7IsEqualERKS_hh(auStack_3d0,auStack_608,1,0);
                        __ZN8PMStringD1Ev(auStack_608);
                        if (sVar2 != 0) {
                          local_620 = FUN_00017c4c(&local_328,&local_318);
                          local_618 = FUN_000157c8(&local_620,&local_f8);
                          local_610 = FUN_00015abc(&local_618);
                          local_630 = FUN_00015a7c(&local_610,&local_f8);
                          local_628 = FUN_00028080(&local_630,&local_318);
                          local_328 = local_628;
                        }
                      }
                      FUN_00002cf8(auStack_664,0x15d4cf);
                      __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_6b0,"rows",0);
                      local_6b8 = FUN_00028080(&local_f8,&local_318);
                      FUN_00013490(0);
                      FUN_001668f4(param_1,auStack_664,auStack_6b0,&local_328,&local_f8,&local_6b8,
                                   auStack_6c0);
                      __ZN8PMStringD1Ev(auStack_6b0);
                    }
                  }
                  FUN_00002cf8(auStack_7d4,0x15d4cf);
                  local_7d8 = 0;
                  FUN_00015b04(auStack_30,auStack_7d4,&local_7d8,1);
                  __ZN8PMStringC1ERKS_(auStack_820,auStack_3d0);
                  __ZN8PMString6AppendEPKciNS_14StringEncodingE
                            (auStack_820," Secondary Rows",0x7fffffff,0xffffffff);
                  plVar6 = (long *)FUN_0000dff8(auStack_d8);
                  local_824 = (**(code **)(*plVar6 + 0x20))(plVar6,auStack_820);
                  uVar7 = FUN_00002e24(&local_824,&DAT_00208c58);
                  if ((uVar7 & 1) == 0) {
                    plVar6 = (long *)FUN_0000dff8(auStack_d8);
                    local_82c = local_824;
                    local_828 = (**(code **)(*plVar6 + 0x40))(plVar6,local_824);
                    plVar6 = (long *)FUN_0000dff8(auStack_d8);
                    uVar5 = (**(code **)(*plVar6 + 0x28))(plVar6,local_828);
                    FUN_000ca9c4(auStack_838,uVar5);
                    sVar2 = FUN_000caa24(auStack_838);
                    if (sVar2 == 0) {
                      plVar6 = (long *)FUN_0001c5a8(auStack_70);
                      uVar5 = FUN_000caa74(auStack_838);
                      uVar5 = (**(code **)(*plVar6 + 0x18))(plVar6,uVar5,0,1);
                      FUN_000caa8c(auStack_840,uVar5);
                      uVar5 = FUN_000caac0(auStack_840);
                      FUN_0016b140(auStack_848,uVar5,&uStack_849);
                      sVar2 = FUN_000cac10(auStack_848);
                      if (sVar2 == 0) {
                        uVar5 = FUN_0000310c(local_88);
                        plVar6 = (long *)FUN_0016b17c(auStack_848);
                        local_85c = (**(code **)(*plVar6 + 0x20))(plVar6,0);
                        FUN_0016b194(auStack_858,uVar5,local_85c,&uStack_85d);
                        sVar2 = FUN_00187b6c(auStack_858);
                        if (sVar2 == 0) {
                          plVar6 = (long *)FUN_0016b1f8(auStack_858);
                          local_864 = (**(code **)(*plVar6 + 0xa0))();
                          FUN_000c535c(auStack_870);
                          plVar6 = (long *)FUN_000c5388(auStack_870);
                          local_874 = local_864;
                          uVar5 = FUN_0000df80(auStack_90);
                          uVar3 = (**(code **)(*plVar6 + 0x38))(plVar6,local_874,uVar5);
                          FUN_000c53a0(auStack_870);
                          local_868 = uVar3;
                          plVar6 = (long *)FUN_0000e2c8(auStack_c0);
                          (**(code **)(*plVar6 + 0x90))(plVar6,&local_868);
                          __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_8c0,"",0);
                          local_8c8 = FUN_00015abc(&local_120);
                          __ZN8PMString12AppendNumberERK6PMRealiss(auStack_8c0,&local_8c8,0,1);
                          __ZN8PMString6AppendEPKciNS_14StringEncodingE
                                    (auStack_8c0," ",0x7fffffff,0xffffffff);
                          __ZN8PMString6AppendEPKciNS_14StringEncodingE
                                    (auStack_8c0,"rows",0x7fffffff,0xffffffff);
                          __ZN8PMString6AppendEPKciNS_14StringEncodingE
                                    (auStack_8c0," (",0x7fffffff,0xffffffff);
                          local_8d0 = FUN_00015abc(auStack_128);
                          __ZN8PMString12AppendNumberERK6PMRealiss(auStack_8c0,&local_8d0,0,1);
                          __ZN8PMString6AppendEPKciNS_14StringEncodingE
                                    (auStack_8c0," lines);",0x7fffffff,0xffffffff);
                          __ZN8PMString6AppendEPKciNS_14StringEncodingE
                                    (auStack_8c0," gutter: ",0x7fffffff,0xffffffff);
                          local_8d8 = FUN_00015abc(auStack_130);
                          __ZN8PMString12AppendNumberERK6PMRealiss(auStack_8c0,&local_8d8,0,1);
                          __ZN8PMString6AppendEPKciNS_14StringEncodingE
                                    (auStack_8c0," ",0x7fffffff,0xffffffff);
                          local_8e0 = FUN_00015abc(auStack_130);
                          FUN_00013490(0x3ff0000000000000);
                          sVar2 = FUN_00014140(&local_8e0,auStack_8e8);
                          bVar1 = true;
                          if (sVar2 == 0) {
                            local_8f0 = FUN_00015abc(auStack_130);
                            FUN_00013490(0);
                            sVar2 = FUN_000132fc(&local_8f0,auStack_8f8);
                            bVar1 = sVar2 != 0;
                          }
                          if (bVar1) {
                            __ZN8PMString6AppendEPKciNS_14StringEncodingE
                                      (auStack_8c0,"lines",0x7fffffff,0xffffffff);
                          }
                          else {
                            __ZN8PMString6AppendEPKciNS_14StringEncodingE
                                      (auStack_8c0,"line",0x7fffffff,0xffffffff);
                          }
                          __ZN8PMString15SetTranslatableEs(auStack_8c0,0);
                          FUN_00002cf8(auStack_8fc,0x15d4cf);
                          FUN_000191b0(auStack_30,auStack_8fc,auStack_8c0,1);
                          FUN_00002cf8(auStack_900,0x15d4cf);
                          iVar4 = FUN_00013034(auStack_30,auStack_900);
                          if (iVar4 == -1) {
LAB_00180270:
                            FUN_00013490(0);
                            sVar2 = FUN_00014140(&local_120,auStack_910);
                            bVar1 = sVar2 != 0;
                          }
                          else {
                            FUN_00002cf8(auStack_904,0x15d4cf);
                            iVar4 = FUN_00013034(auStack_30,auStack_904);
                            bVar1 = false;
                            if (iVar4 == 0) goto LAB_00180270;
                          }
                          if (bVar1) {
                            FUN_00002cf8(auStack_914,0x15d4cf);
                            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                                      (auStack_960,"Custom Setting Applied",0);
                            FUN_000191b0(auStack_30,auStack_914,auStack_960,1);
                            __ZN8PMStringD1Ev(auStack_960);
                          }
                          if (local_132 == 0) {
                            FUN_00002cf8(auStack_964,0x15d4d1);
                            FUN_00013178(auStack_30,auStack_964,&DAT_00208c52);
                            FUN_00002cf8(auStack_968,0x15d4c1);
                            FUN_00013178(auStack_30,auStack_968,&DAT_00208c52);
                            FUN_00002cf8(auStack_96c,0x15d4d0);
                            FUN_00013178(auStack_30,auStack_96c,&DAT_00208c50);
                            FUN_00002cf8(auStack_970,0x15d4c0);
                            FUN_00013178(auStack_30,auStack_970,&DAT_00208c50);
                          }
                          else {
                            FUN_00002cf8(auStack_974,0x15d4d0);
                            FUN_00013178(auStack_30,auStack_974,&DAT_00208c52);
                            FUN_00002cf8(auStack_978,0x15d4c0);
                            FUN_00013178(auStack_30,auStack_978,&DAT_00208c52);
                            FUN_00002cf8(auStack_97c,0x15d4d1);
                            FUN_00013178(auStack_30,auStack_97c,&DAT_00208c50);
                            FUN_00002cf8(auStack_980,0x15d4c1);
                            FUN_00013178(auStack_30,auStack_980,&DAT_00208c50);
                          }
                          FUN_00002cf8(auStack_984,0x15d4bb);
                          FUN_00013c20(auStack_30,auStack_984,&local_120);
                          FUN_00013490(0);
                          sVar2 = FUN_00018504(&local_318,auStack_990);
                          bVar1 = false;
                          if (sVar2 != 0) {
                            FUN_00002cf8(auStack_994,0x15d4cf);
                            iVar4 = FUN_00013034(auStack_30,auStack_994);
                            bVar1 = 0 < iVar4;
                          }
                          if (bVar1) {
                            FUN_00013490(&local_9a0);
                            FUN_00013490(0,&local_9a8);
                            local_9c0 = FUN_00015a7c(auStack_128,&local_f8);
                            local_9b8 = FUN_00015a7c(&local_9c0,param_1 + 0x88);
                            local_9b0 = FUN_00017c4c(&local_9b8,&local_310);
                            local_9a0 = local_9b0;
                            local_9d8 = FUN_00015a7c(auStack_130,&local_f8);
                            local_9d0 = FUN_00015a7c(&local_9d8,param_1 + 0x88);
                            local_9e0 = FUN_00015a7c(&local_318,param_1 + 0x88);
                            local_9c8 = FUN_00017c4c(&local_9d0,&local_9e0);
                            local_9a8 = local_9c8;
                            FUN_00002cf8(auStack_9e4,0x15d4bc);
                            FUN_00013c20(auStack_30,auStack_9e4,&local_9a0);
                            FUN_00002cf8(auStack_9e8,0x15d4bd);
                            FUN_00013c20(auStack_30,auStack_9e8,&local_9a8);
                          }
                          else {
                            FUN_00002cf8(auStack_9ec,0x15d4bc);
                            local_a00 = FUN_00015a7c(auStack_128,&local_f8);
                            local_9f8 = FUN_00015a7c(&local_a00,param_1 + 0x88);
                            FUN_00013c20(auStack_30,auStack_9ec,&local_9f8);
                            FUN_00013490(0);
                            sVar2 = FUN_00014140(auStack_130,auStack_a08);
                            if (sVar2 != 0) {
                              FUN_00002cf8(auStack_a0c,0x15d4bd);
                              local_a20 = FUN_00015a7c(auStack_130,&local_f8);
                              local_a18 = FUN_00015a7c(&local_a20,param_1 + 0x88);
                              FUN_00013c20(auStack_30,auStack_a0c,&local_a18);
                            }
                          }
                          __ZN8PMStringD1Ev(auStack_8c0);
                          local_48 = 0;
                        }
                        else {
                          local_48 = 2;
                        }
                        FUN_0016bd84(auStack_858);
                      }
                      else {
                        local_48 = 2;
                      }
                      FUN_000cad2c(auStack_848);
                      FUN_000caad8(auStack_840);
                    }
                    else {
                      local_48 = 2;
                    }
                    FUN_000caa48(auStack_838);
                  }
                  else {
                    local_48 = 2;
                  }
                  __ZN8PMStringD1Ev(auStack_820);
                  __ZN8PMStringD1Ev(auStack_4c0);
                  __ZN8PMStringD1Ev(auStack_470);
                  __ZN8PMStringD1Ev(auStack_420);
                  __ZN8PMStringD1Ev(auStack_3d0);
                  __ZN8PMStringD1Ev(auStack_370);
                  __ZN8PMStringD1Ev(auStack_180);
                }
                else {
                  local_48 = 2;
                }
              }
              else {
                local_48 = 2;
              }
              FUN_0000e040(auStack_d8);
            }
            else {
              local_48 = 2;
            }
            FUN_00017ca4(auStack_c8);
          }
          else {
            local_48 = 2;
          }
          FUN_0000e2f8(auStack_c0);
        }
        else {
          local_48 = 2;
        }
        FUN_0001d190(auStack_a0);
      }
      else {
        local_48 = 2;
      }
      FUN_0000e06c(auStack_90);
    }
    else {
      local_48 = 2;
    }
    FUN_0001d138(auStack_70);
  }
  else {
    local_48 = 2;
  }
  FUN_00002b9c(auStack_50);
LAB_00180804:
  FUN_00002c54(auStack_30);
  return;
}
