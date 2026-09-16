/*
Columns (row 08). FUN_00131edc: w_col = (T - (c - 1) * g) / c, the gutter and minimum alerts, guide emission.

PMReal helpers: construct 0x13490, operator/ 0x157c8, operator* 0x15a7c, operator+ 0x17c4c, operator- 0x28080,
Round 0x15abc (floor(x + 0.5)), floor 0x37d70, a > b + 1e-8 0x14140, a >= b - 1e-8 0x15808, a <= b + 1e-8 0x31d38,
a < b - 1e-8 0x3208c, |a - b| < 1e-8 0x132fc, not equal 0x18504, quantise 0x152f0 (mode 3 three decimals, mode 4 four).
Widget access: read value 0x13338, read value in the panel unit 0x1544c, read checkbox 0x12ef4, read dropdown index 0x13034,
write value 0x13c20, set maximum 0x196d0, unit factor at object + 0x88.
*/

//==== FUNC @ 132800 -> 00131edc

/* WARNING: Restarted to delay deadcode elimination for space: stack */

void FUN_00131edc(long param_1,short *param_2)

{
  bool bVar1;
  short sVar2;
  int iVar3;
  int iVar4;
  undefined8 uVar5;
  long *plVar6;
  long lVar7;
  undefined1 auStack_958 [72];
  undefined1 auStack_910 [72];
  undefined1 auStack_8c8 [72];
  undefined1 auStack_880 [4];
  undefined1 auStack_87c [4];
  undefined1 auStack_878 [76];
  undefined1 auStack_82c [4];
  undefined1 auStack_828 [72];
  undefined8 local_7e0;
  undefined4 local_7d4;
  undefined1 auStack_7d0 [12];
  undefined4 local_7c4;
  undefined1 auStack_7c0 [8];
  undefined1 local_7b8 [16];
  undefined4 local_7a8;
  int local_7a4;
  undefined1 auStack_7a0 [8];
  undefined8 local_798;
  undefined8 local_790;
  undefined8 local_788;
  undefined1 auStack_780 [8];
  undefined8 local_778;
  undefined1 auStack_770 [12];
  int local_764;
  undefined1 auStack_760 [8];
  undefined1 auStack_758 [8];
  undefined1 auStack_750 [28];
  undefined1 auStack_734 [4];
  undefined1 auStack_730 [4];
  undefined1 auStack_72c [4];
  undefined1 local_728 [16];
  undefined1 auStack_718 [8];
  undefined1 auStack_710 [24];
  undefined1 auStack_6f8 [4];
  undefined1 auStack_6f4 [4];
  undefined1 auStack_6f0 [76];
  undefined1 auStack_6a4 [4];
  undefined1 auStack_6a0 [76];
  undefined1 auStack_654 [4];
  undefined1 auStack_650 [76];
  undefined1 auStack_604 [4];
  undefined1 auStack_600 [8];
  undefined1 local_5f8 [16];
  undefined1 auStack_5e8 [8];
  undefined1 auStack_5e0 [24];
  undefined1 auStack_5c8 [4];
  undefined1 auStack_5c4 [4];
  undefined1 auStack_5c0 [76];
  undefined1 auStack_574 [4];
  undefined1 auStack_570 [76];
  undefined1 auStack_524 [4];
  undefined1 auStack_520 [76];
  undefined1 auStack_4d4 [4];
  undefined1 auStack_4d0 [72];
  undefined1 auStack_488 [8];
  undefined1 auStack_480 [8];
  undefined8 local_478;
  undefined8 local_470;
  undefined8 local_468;
  undefined8 local_460;
  undefined1 auStack_454 [4];
  undefined8 local_450;
  undefined1 auStack_448 [12];
  undefined1 auStack_43c [4];
  undefined8 local_438;
  undefined1 auStack_42c [4];
  undefined8 local_428;
  undefined8 local_420;
  undefined1 auStack_418 [12];
  undefined1 auStack_40c [4];
  undefined1 auStack_408 [72];
  undefined1 auStack_3c0 [8];
  undefined1 auStack_3b8 [76];
  undefined1 auStack_36c [4];
  undefined1 auStack_368 [76];
  undefined1 auStack_31c [4];
  undefined1 auStack_318 [72];
  undefined8 local_2d0;
  undefined1 auStack_2c4 [4];
  undefined8 local_2c0;
  undefined4 local_2b4;
  undefined1 auStack_2b0 [4];
  undefined1 auStack_2ac [4];
  undefined4 local_2a8;
  undefined1 auStack_2a4 [4];
  undefined1 auStack_2a0 [76];
  undefined1 auStack_254 [4];
  undefined1 auStack_250 [76];
  undefined1 auStack_204 [4];
  undefined1 auStack_200 [76];
  undefined1 auStack_1b4 [4];
  undefined1 auStack_1b0 [72];
  undefined1 auStack_168 [72];
  undefined4 local_120;
  undefined1 auStack_11c [4];
  undefined1 auStack_118 [72];
  undefined4 local_d0;
  undefined1 auStack_cc [4];
  undefined1 auStack_c8 [79];
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
    uVar5 = __Z26GetExecutionContextSessionv();
    FUN_00002978(auStack_58,uVar5,&uStack_59);
    sVar2 = FUN_000029b4(auStack_58);
    if (sVar2 == 0) {
      plVar6 = (long *)FUN_000029d8(auStack_58);
      local_70 = (**(code **)(*plVar6 + 0x28))();
      FUN_0001b840(auStack_78,local_70,&uStack_79);
      sVar2 = FUN_0001b87c(auStack_78);
      if (sVar2 == 0) {
        FUN_00002cf8(auStack_cc,0x15d309);
        local_d0 = 0xffffffff;
        FUN_00012c10(auStack_c8,auStack_38,auStack_cc,&local_d0);
        __ZN8PMStringC1ERKS_(auStack_118,auStack_c8);
        __ZN8PMString6AppendEPKciNS_14StringEncodingE
                  (auStack_118," Vertical Column Lines",0x7fffffff,0xffffffff);
        FUN_00002cf8(auStack_11c,0x15d4c7);
        local_120 = 0;
        FUN_00015b04(auStack_38,auStack_11c,&local_120,1);
        if (*local_30 == 0) {
          __ZN8PMStringC1ERKS_(auStack_168,auStack_c8);
          __ZN8PMString6AppendEPKciNS_14StringEncodingE
                    (auStack_168," Vertical Subcolumn Lines",0x7fffffff,0xffffffff);
          __ZN8PMStringC1ERKS_(auStack_1b0,auStack_c8);
          __ZN8PMString6AppendEPKciNS_14StringEncodingE
                    (auStack_1b0," Subcolumns",0x7fffffff,0xffffffff);
          FUN_00002cf8(auStack_1b4,0x15d4af);
          __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_200,"",0);
          FUN_00013588(auStack_38,auStack_1b4,auStack_200);
          __ZN8PMStringD1Ev(auStack_200);
          FUN_00002cf8(auStack_204,0x15d4b0);
          __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_250,"",0);
          FUN_00013588(auStack_38,auStack_204,auStack_250);
          __ZN8PMStringD1Ev(auStack_250);
          FUN_00002cf8(auStack_254,0x15d4b1);
          __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_2a0,"",0);
          FUN_00013588(auStack_38,auStack_254,auStack_2a0);
          __ZN8PMStringD1Ev(auStack_2a0);
          FUN_00002cf8(auStack_2a4,0x15d4c8);
          local_2a8 = 0;
          FUN_00015b04(auStack_38,auStack_2a4,&local_2a8,1);
          FUN_00002cf8(auStack_2ac,0x15d4c8);
          FUN_00011dc4(auStack_38,auStack_2ac,&DAT_00208c50);
          FUN_00002cf8(auStack_2b0,0x15d4d3);
          FUN_00011dc4(auStack_38,auStack_2b0,&DAT_00208c50);
          local_2b4 = 0;
          FUN_001b775c(auStack_38,auStack_1b0,&local_2b4);
          FUN_001b4670(auStack_168);
          __ZN8PMStringD1Ev(auStack_1b0);
          __ZN8PMStringD1Ev(auStack_168);
        }
        FUN_0015d6f4(param_1);
        FUN_00002cf8(auStack_2c4,0x15d4ab);
        local_2c0 = FUN_00013338(auStack_38,auStack_2c4);
        local_2d0 = FUN_00015abc(&local_2c0);
        sVar2 = FUN_00018504(&local_2c0,&local_2d0);
        if (sVar2 == 0) {
          FUN_00013490(0x3ff0000000000000);
          sVar2 = FUN_000132fc(&local_2c0,auStack_3c0);
          if (sVar2 != 0) {
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                      (auStack_408,"The minimum value must be 2.",0);
            __ZN6CAlert16InformationAlertERK8PMString(auStack_408);
            __ZN8PMStringD1Ev(auStack_408);
            FUN_00002cf8(auStack_40c,0x15d4ab);
            FUN_00013490(0x4000000000000000);
            FUN_00013c20(auStack_38,auStack_40c,auStack_418);
            FUN_00013490(0x4000000000000000,&local_420);
            local_2c0 = local_420;
          }
          FUN_00002cf8(auStack_42c,0x15d4ad);
          local_428 = FUN_00013338(auStack_38,auStack_42c);
          FUN_00002cf8(auStack_43c,0x15d4a8);
          local_438 = FUN_00013338(auStack_38,auStack_43c);
          FUN_00013490(0);
          sVar2 = FUN_000132fc(&local_438,auStack_448);
          if (sVar2 != 0) {
            FUN_00002cf8(auStack_454,&DAT_0015d31a);
            local_450 = FUN_00013338(auStack_38,auStack_454);
            local_438 = local_450;
          }
          FUN_00013490(0x3ff0000000000000);
          local_478 = FUN_00028080(&local_2c0,auStack_480);
          local_470 = FUN_00015a7c(&local_478,&local_428);
          local_468 = FUN_00028080(&local_438,&local_470);
          local_460 = FUN_000157c8(&local_468,&local_2c0);
          FUN_00013490(0);
          sVar2 = FUN_0003208c(&local_460,auStack_488);
          if (sVar2 == 0) {
            FUN_00013490(0);
            sVar2 = FUN_000132fc(&local_2c0,auStack_600);
            if (sVar2 == 0) {
              FUN_00002cf8(auStack_72c,0x15d4ac);
              FUN_00013c20(auStack_38,auStack_72c,&local_460);
              FUN_00002cf8(auStack_730,0x15d4af);
              FUN_00011dc4(auStack_38,auStack_730,&DAT_00208c52);
              FUN_00002cf8(auStack_734,0x15d4b1);
              FUN_00011dc4(auStack_38,auStack_734,&DAT_00208c52);
            }
            else {
              FUN_00002cf8(auStack_604,0x15d4ab);
              __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_650,"",0);
              FUN_00013588(auStack_38,auStack_604,auStack_650);
              __ZN8PMStringD1Ev(auStack_650);
              FUN_00002cf8(auStack_654,0x15d4ac);
              __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_6a0,"",0);
              FUN_00013588(auStack_38,auStack_654,auStack_6a0);
              __ZN8PMStringD1Ev(auStack_6a0);
              FUN_00002cf8(auStack_6a4,0x15d4ad);
              __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_6f0,"",0);
              FUN_00013588(auStack_38,auStack_6a4,auStack_6f0);
              __ZN8PMStringD1Ev(auStack_6f0);
              FUN_00002cf8(auStack_6f4,0x15d4af);
              FUN_00011dc4(auStack_38,auStack_6f4,&DAT_00208c50);
              FUN_00002cf8(auStack_6f8,0x15d4b1);
              FUN_00011dc4(auStack_38,auStack_6f8,&DAT_00208c50);
              FUN_0005c474();
              FUN_00013490(0);
              FUN_0005c4a0(auStack_710,auStack_718);
              plVar6 = (long *)FUN_000029d8(auStack_58);
              local_728 = (**(code **)(*plVar6 + 0x28))();
              FUN_001afeec(local_728,auStack_710);
              FUN_001b4670(auStack_118);
              FUN_0005c530(auStack_710);
            }
            FUN_0005c474();
            FUN_00013490(0);
            FUN_00180930(auStack_750,auStack_758);
            FUN_00013490(0x3ff0000000000000);
            sVar2 = FUN_00014140(&local_2c0,auStack_760);
            if (sVar2 != 0) {
              local_764 = 0;
              while( true ) {
                FUN_00013490((double)(long)local_764,auStack_770);
                FUN_00013490(0x3ff0000000000000);
                local_778 = FUN_00028080(&local_2c0,auStack_780);
                sVar2 = FUN_0003208c(auStack_770,&local_778);
                if (sVar2 == 0) break;
                local_788 = FUN_000157c8(&local_460,param_1 + 0x88);
                FUN_0004ef2c(auStack_758,&local_788);
                FUN_00180930(auStack_750,auStack_758);
                local_790 = FUN_000157c8(&local_428,param_1 + 0x88);
                FUN_0004ef2c(auStack_758,&local_790);
                FUN_00180930(auStack_750,auStack_758);
                local_764 = local_764 + 1;
              }
              local_798 = FUN_000157c8(&local_460,param_1 + 0x88);
              FUN_0004ef2c(auStack_758,&local_798);
              FUN_00180930(auStack_750,auStack_758);
              uVar5 = FUN_0000310c(local_70);
              __ZN7UIDListC1EP9IDataBase(auStack_7a0,uVar5);
              local_7a4 = 0;
              while( true ) {
                iVar4 = local_7a4;
                plVar6 = (long *)FUN_0001c5a8(auStack_78);
                iVar3 = (**(code **)(*plVar6 + 0x40))();
                if (iVar3 <= iVar4) break;
                plVar6 = (long *)FUN_0001c5a8(auStack_78);
                local_7a8 = (**(code **)(*plVar6 + 0x50))(plVar6,local_7a4);
                __ZN7UIDList6AppendE6IDTypeI7UID_tagE(auStack_7a0,local_7a8);
                local_7a4 = local_7a4 + 1;
              }
              plVar6 = (long *)FUN_000029d8(auStack_58);
              local_7b8 = (**(code **)(*plVar6 + 0x28))();
              FUN_001afeec(local_7b8,auStack_750);
              FUN_000069c0(&local_7c4,0x50e);
              uVar5 = __ZN8CmdUtils13CreateCommandE6IDTypeI11ClassID_tagE(local_7c4);
              FUN_0000de90(auStack_7c0,uVar5);
              lVar7 = FUN_0000dec4(auStack_7c0);
              if (lVar7 != 0) {
                uVar5 = FUN_0000dec4(auStack_7c0);
                FUN_00002f44(&local_7d4,0x512);
                FUN_001809c0(auStack_7d0,uVar5,local_7d4);
                lVar7 = FUN_00180a04(auStack_7d0);
                if (lVar7 != 0) {
                  plVar6 = (long *)FUN_00180a1c(auStack_7d0);
                  local_7e0 = FUN_000157c8(&local_428,param_1 + 0x88);
                  (**(code **)(*plVar6 + 0x18))(plVar6,&local_7e0);
                  plVar6 = (long *)FUN_0000dedc(auStack_7c0);
                  (**(code **)(*plVar6 + 0x40))(plVar6,auStack_7a0);
                  uVar5 = FUN_0000dec4(auStack_7c0);
                  __ZN8CmdUtils14ProcessCommandEP8ICommand(uVar5);
                }
                FUN_00180a34(auStack_7d0);
              }
              sVar2 = FUN_001a6604(auStack_118);
              if (sVar2 != 0) {
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_828,"columns",0);
                FUN_001c2b94(auStack_38,auStack_828);
                __ZN8PMStringD1Ev(auStack_828);
              }
              FUN_00002cf8(auStack_82c,0x15d4c7);
              __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_878,"Custom Setting Applied",0);
              FUN_000191b0(auStack_38,auStack_82c,auStack_878,1);
              __ZN8PMStringD1Ev(auStack_878);
              FUN_00002cf8(auStack_87c,&DAT_0015d374);
              iVar4 = FUN_00013034(auStack_38,auStack_87c);
              if (0 < iVar4) {
                FUN_00002cf8(auStack_880,&DAT_0015d374);
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_8c8,"Custom Setting Applied",0)
                ;
                FUN_000191b0(auStack_38,auStack_880,auStack_8c8,1);
                __ZN8PMStringD1Ev(auStack_8c8);
              }
              FUN_0000def4(auStack_7c0);
              __ZN7UIDListD1Ev(auStack_7a0);
            }
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_910,"[GC] Smart Setup");
            sVar2 = FUN_001a6604(auStack_910);
            bVar1 = true;
            if (sVar2 == 0) {
              __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_958,"[GC] CGS",0);
              sVar2 = FUN_001a6604(auStack_958);
              bVar1 = sVar2 != 0;
              __ZN8PMStringD1Ev(auStack_958);
            }
            __ZN8PMStringD1Ev(auStack_910);
            if (bVar1) {
              FUN_0010a5d4(param_1);
            }
            FUN_0005c530(auStack_750);
            local_50 = 0;
          }
          else {
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                      (auStack_4d0,"Gutter too large for this number of columns.",0);
            __ZN6CAlert16InformationAlertERK8PMString(auStack_4d0);
            __ZN8PMStringD1Ev(auStack_4d0);
            FUN_00002cf8(auStack_4d4,0x15d4ab);
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_520,"",0);
            FUN_00013588(auStack_38,auStack_4d4,auStack_520);
            __ZN8PMStringD1Ev(auStack_520);
            FUN_00002cf8(auStack_524,0x15d4ac);
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_570,"",0);
            FUN_00013588(auStack_38,auStack_524,auStack_570);
            __ZN8PMStringD1Ev(auStack_570);
            FUN_00002cf8(auStack_574,0x15d4ad);
            __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_5c0,"",0);
            FUN_00013588(auStack_38,auStack_574,auStack_5c0);
            __ZN8PMStringD1Ev(auStack_5c0);
            FUN_00002cf8(auStack_5c4,0x15d4af);
            FUN_00011dc4(auStack_38,auStack_5c4,&DAT_00208c50);
            FUN_00002cf8(auStack_5c8,0x15d4b1);
            FUN_00011dc4(auStack_38,auStack_5c8,&DAT_00208c50);
            FUN_0005c474();
            FUN_00013490(0);
            FUN_0005c4a0(auStack_5e0,auStack_5e8);
            plVar6 = (long *)FUN_000029d8(auStack_58);
            local_5f8 = (**(code **)(*plVar6 + 0x28))();
            FUN_001afeec(local_5f8,auStack_5e0);
            FUN_001b4670(auStack_118);
            local_50 = 2;
            FUN_0005c530(auStack_5e0);
          }
        }
        else {
          __ZN8PMStringC1EPKcNS_19TranslateDuringCallE
                    (auStack_318,
                     "Please enter a whole value, for example: 3, 4 and 5 etc. Values like: 3,5 or 5.4 etc. can not be entered."
                     ,0);
          __ZN6CAlert16InformationAlertERK8PMString(auStack_318);
          FUN_00002cf8(auStack_31c,0x15d4ab);
          __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_368,"",0);
          FUN_00013588(auStack_38,auStack_31c,auStack_368);
          __ZN8PMStringD1Ev(auStack_368);
          FUN_00002cf8(auStack_36c,0x15d4ac);
          __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_3b8,"",0);
          FUN_00013588(auStack_38,auStack_36c,auStack_3b8);
          __ZN8PMStringD1Ev(auStack_3b8);
          FUN_001b4670(auStack_118);
          local_50 = 2;
          __ZN8PMStringD1Ev(auStack_318);
        }
        __ZN8PMStringD1Ev(auStack_118);
        __ZN8PMStringD1Ev(auStack_c8);
      }
      else {
        local_50 = 2;
      }
      FUN_0001d138(auStack_78);
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
