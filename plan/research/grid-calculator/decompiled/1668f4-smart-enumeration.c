/*
Smart enumeration, ordering, and browsing (row 16). FUN_001668f4 fills the column or row dropdown: for n = 2.. while
n * L <= T / 2, for g = 1.. while g * L <= n * L + 0.001, q = quantise3((T - n * L) / ((n + g) * L)) must equal Round(q)
(0x132fc) and be < 41 (0x3208c), entry 'q + 1 columns (n lines); gutter: g lines'; then gutterless entries c = 2..K / 2
with K = Round(T / L), K mod c = 0, c < 41. The list is std::sort'ed with the comparator picked by the Sort Columns & Rows
Based On dropdown (vtable + 0xf8): FUN_001d2528 columns, lines, gutter; FUN_001d2648 lines, columns, gutter;
FUN_001d2768 gutter, columns, lines; each key comparator (FUN_001d1c74, FUN_001d1ed0, FUN_001d21fc) parses the entry text and
orders ascending; Descending (vtable + 0x108) reverses. FUN_001256bc is the > and < button observer: > selects index + 1 and
wraps from the last entry to 0, < selects index - 1 and wraps from 0 to the last entry.

PMReal helpers: construct 0x13490, operator/ 0x157c8, operator* 0x15a7c, operator+ 0x17c4c, operator- 0x28080,
Round 0x15abc (floor(x + 0.5)), floor 0x37d70, a > b + 1e-8 0x14140, a >= b - 1e-8 0x15808, a <= b + 1e-8 0x31d38,
a < b - 1e-8 0x3208c, |a - b| < 1e-8 0x132fc, not equal 0x18504, quantise 0x152f0 (mode 3 three decimals, mode 4 four).
Widget access: read value 0x13338, read value in the panel unit 0x1544c, read checkbox 0x12ef4, read dropdown index 0x13034,
write value 0x13c20, set maximum 0x196d0, unit factor at object + 0x88.
*/

//==== FUNC @ 1668f4 -> 001668f4

/* WARNING: Restarted to delay deadcode elimination for space: stack */

void FUN_001668f4(undefined8 param_1,undefined8 param_2,undefined8 param_3,undefined8 param_4,
                 undefined8 *param_5,undefined8 param_6,undefined8 *param_7)

{
  int iVar1;
  undefined8 *puVar2;
  bool bVar3;
  short sVar4;
  undefined8 uVar5;
  long *plVar6;
  ulong uVar7;
  ulong uVar8;
  uint local_714;
  uint local_6cc;
  undefined4 local_4bc;
  undefined1 auStack_4b8 [76];
  int local_46c;
  undefined1 auStack_468 [4];
  undefined1 auStack_464 [4];
  undefined1 auStack_460 [4];
  undefined1 auStack_45c [4];
  undefined1 auStack_458 [4];
  undefined1 auStack_454 [4];
  undefined1 auStack_450 [4];
  undefined1 auStack_44c [4];
  undefined1 auStack_448 [72];
  undefined8 local_400;
  undefined8 local_3f8;
  undefined8 local_3f0;
  undefined8 local_3e8;
  undefined8 local_3e0;
  undefined8 local_3d8;
  undefined8 local_3d0;
  undefined8 local_3c8;
  int local_3bc;
  undefined1 auStack_3b8 [8];
  undefined1 auStack_3b0 [8];
  undefined1 auStack_3a8 [8];
  undefined1 auStack_3a0 [72];
  undefined8 local_358;
  undefined8 local_350;
  undefined1 auStack_348 [8];
  undefined1 auStack_340 [8];
  undefined1 auStack_338 [8];
  undefined1 auStack_330 [72];
  undefined1 auStack_2e8 [8];
  int local_2e0;
  int local_2dc;
  undefined8 local_2d8;
  undefined8 local_2d0;
  undefined8 local_2c8;
  undefined8 local_2c0;
  undefined1 auStack_2b8 [8];
  undefined1 auStack_2b0 [8];
  undefined1 auStack_2a8 [8];
  undefined8 local_2a0;
  undefined1 auStack_298 [72];
  undefined1 auStack_250 [72];
  undefined1 auStack_208 [72];
  undefined1 auStack_1c0 [72];
  undefined1 auStack_178 [8];
  undefined8 local_170;
  undefined4 local_164;
  undefined8 local_160;
  undefined8 local_158;
  undefined8 local_150;
  undefined1 auStack_148 [8];
  undefined8 local_140;
  undefined8 local_138;
  undefined1 auStack_130 [8];
  undefined8 local_128;
  int local_11c;
  undefined1 auStack_118 [8];
  undefined8 local_110;
  undefined1 auStack_108 [8];
  undefined8 local_100;
  undefined8 local_f8;
  int local_ec;
  undefined1 auStack_e8 [24];
  undefined1 auStack_d0 [8];
  undefined8 local_c8;
  undefined8 local_c0;
  undefined1 auStack_b8 [8];
  undefined8 local_b0;
  undefined1 uStack_a1;
  undefined1 auStack_a0 [8];
  undefined8 local_98;
  undefined1 uStack_89;
  undefined1 auStack_88 [8];
  undefined1 auStack_80 [8];
  undefined4 local_78;
  undefined1 uStack_61;
  undefined1 auStack_60 [8];
  undefined8 *local_58;
  undefined8 local_50;
  undefined8 *local_48;
  undefined8 local_40;
  undefined8 local_38;
  undefined8 local_30;
  undefined8 local_28;
  
  local_58 = param_7;
  local_50 = param_6;
  local_48 = param_5;
  local_40 = param_4;
  local_38 = param_3;
  local_30 = param_2;
  local_28 = param_1;
  FUN_00002bf4(auStack_60,param_1,&uStack_61);
  sVar4 = FUN_00002c30(auStack_60);
  if (sVar4 == 0) {
    FUN_001a2484(auStack_80);
    sVar4 = FUN_0000e2a4(auStack_80);
    if (sVar4 == 0) {
      uVar5 = __Z26GetExecutionContextSessionv();
      FUN_00002978(auStack_88,uVar5,&uStack_89);
      sVar4 = FUN_000029b4(auStack_88);
      if (sVar4 == 0) {
        plVar6 = (long *)FUN_00003ca8(auStack_60);
        local_98 = (**(code **)(*plVar6 + 0x48))(plVar6,local_30,9999);
        FUN_000147f4(auStack_a0,local_98,&uStack_a1);
        sVar4 = FUN_00014830(auStack_a0);
        if (sVar4 == 0) {
          plVar6 = (long *)FUN_00014854(auStack_a0);
          (**(code **)(*plVar6 + 0x28))(plVar6,1);
          FUN_00013490(&local_b0);
          uVar5 = local_50;
          FUN_00013490(0);
          sVar4 = FUN_000132fc(uVar5,auStack_b8);
          if (sVar4 == 0) {
            FUN_00013490(0x4000000000000000);
            local_c8 = FUN_00015a7c(auStack_d0,local_48);
            local_c0 = FUN_00028080(&local_c8,local_50);
            local_b0 = local_c0;
          }
          else {
            local_b0 = *local_48;
          }
          FUN_000134c4(auStack_e8);
          local_ec = 1;
          FUN_00013490(0x4000000000000000);
          local_100 = FUN_00015a7c(auStack_108,local_48);
          local_f8 = FUN_00017c4c(&local_100,local_50);
          while( true ) {
            uVar5 = local_40;
            FUN_00013490(0x4000000000000000);
            local_110 = FUN_000157c8(uVar5,auStack_118);
            sVar4 = FUN_00031d38(&local_f8,&local_110);
            if (sVar4 == 0) break;
            local_ec = local_ec + 1;
            local_11c = 0;
            FUN_00013490(&local_128);
            puVar2 = local_58;
            FUN_00013490(0);
            sVar4 = FUN_00018504(puVar2,auStack_130);
            if (sVar4 == 0) {
              local_128 = local_f8;
            }
            else {
              local_128 = *local_58;
            }
            local_138 = local_b0;
            while( true ) {
              local_140 = FUN_00028080(&local_138,&local_128);
              FUN_00013490(0x3f50624dd2f1a9fc);
              sVar4 = FUN_00031d38(&local_140,auStack_148);
              if (sVar4 == 0) break;
              local_11c = local_11c + 1;
              local_158 = FUN_00028080(local_40,&local_f8);
              local_160 = FUN_00017c4c(&local_f8,&local_138);
              local_150 = FUN_000157c8(&local_158,&local_160);
              local_164 = 3;
              FUN_000152f0(&local_150,&local_164);
              local_170 = FUN_00015abc(&local_150);
              sVar4 = FUN_000132fc(&local_150,&local_170);
              bVar3 = false;
              if (sVar4 != 0) {
                FUN_00013490(0x4044800000000000);
                sVar4 = FUN_0003208c(&local_150,auStack_178);
                bVar3 = sVar4 != 0;
              }
              if (bVar3) {
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_1c0,"",0);
                if (local_11c == 1) {
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_208,"line",0);
                  FUN_0000dd8c(auStack_1c0);
                  __ZN8PMStringD1Ev(auStack_208);
                }
                else {
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_250,"lines",0);
                  FUN_0000dd8c(auStack_1c0);
                  __ZN8PMStringD1Ev(auStack_250);
                }
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_298,"",0);
                FUN_00013490(0x3ff0000000000000);
                local_2a0 = FUN_00017c4c(&local_150,auStack_2a8);
                __ZN8PMString12AppendNumberERK6PMRealiss(auStack_298,&local_2a0,0,1);
                __ZN8PMString6AppendEPKciNS_14StringEncodingE(auStack_298," ",0x7fffffff,0xffffffff)
                ;
                __ZN8PMString6AppendERKS_i(auStack_298,local_38,0x7fffffff);
                __ZN8PMString6AppendEPKciNS_14StringEncodingE
                          (auStack_298," (",0x7fffffff,0xffffffff);
                FUN_00013490((double)(long)local_ec);
                __ZN8PMString12AppendNumberERK6PMRealiss(auStack_298,auStack_2b0,0,1);
                __ZN8PMString6AppendEPKciNS_14StringEncodingE
                          (auStack_298," lines);",0x7fffffff,0xffffffff);
                __ZN8PMString6AppendEPKciNS_14StringEncodingE
                          (auStack_298," gutter: ",0x7fffffff,0xffffffff);
                FUN_00013490((double)(long)local_11c);
                __ZN8PMString12AppendNumberERK6PMRealiss(auStack_298,auStack_2b8,0,1);
                __ZN8PMString6AppendEPKciNS_14StringEncodingE(auStack_298," ",0x7fffffff,0xffffffff)
                ;
                __ZN8PMString6AppendERKS_i(auStack_298,auStack_1c0,0x7fffffff);
                __ZN8PMString15SetTranslatableEs(auStack_298,0);
                FUN_00032c44(auStack_e8,auStack_298);
                __ZN8PMStringD1Ev(auStack_298);
                __ZN8PMStringD1Ev(auStack_1c0);
              }
              local_2c0 = FUN_00017c4c(&local_138,local_48);
              local_138 = local_2c0;
            }
            local_2c8 = FUN_00017c4c(&local_f8,local_48);
            local_f8 = local_2c8;
          }
          local_2d8 = FUN_000157c8(local_40,local_48);
          local_2d0 = FUN_00015abc(&local_2d8);
          local_2dc = FUN_00032070(&local_2d0);
          for (local_2e0 = 2; uVar5 = local_50, local_2e0 <= local_2dc / 2;
              local_2e0 = local_2e0 + 1) {
            FUN_00013490(0);
            sVar4 = FUN_000132fc(uVar5,auStack_2e8);
            if (sVar4 == 0) {
              local_358 = FUN_000157c8(local_40,local_48);
              local_350 = FUN_0005c3d4(&local_358);
              local_2d0 = local_350;
              local_2dc = FUN_00032070(&local_2d0);
              iVar1 = 0;
              if (local_2e0 != 0) {
                iVar1 = (local_2dc - local_2e0) / local_2e0;
              }
              if ((local_2dc - local_2e0 == iVar1 * local_2e0) && (local_2e0 < 0x29)) {
                iVar1 = 0;
                if (local_2e0 != 0) {
                  iVar1 = (local_2dc - local_2e0) / local_2e0;
                }
                if (1 < iVar1) {
                  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_3a0,"",0);
                  FUN_00013490((double)(long)local_2e0);
                  __ZN8PMString12AppendNumberERK6PMRealiss(auStack_3a0,auStack_3a8,0,1);
                  __ZN8PMString6AppendEPKciNS_14StringEncodingE
                            (auStack_3a0," ",0x7fffffff,0xffffffff);
                  __ZN8PMString6AppendERKS_i(auStack_3a0,local_38,0x7fffffff);
                  __ZN8PMString6AppendEPKciNS_14StringEncodingE
                            (auStack_3a0," (",0x7fffffff,0xffffffff);
                  iVar1 = 0;
                  if (local_2e0 != 0) {
                    iVar1 = (local_2dc - local_2e0) / local_2e0;
                  }
                  FUN_00013490((double)iVar1);
                  __ZN8PMString12AppendNumberERK6PMRealiss(auStack_3a0,auStack_3b0,0,1);
                  __ZN8PMString6AppendEPKciNS_14StringEncodingE
                            (auStack_3a0," lines);",0x7fffffff,0xffffffff);
                  __ZN8PMString6AppendEPKciNS_14StringEncodingE
                            (auStack_3a0," gutter: ",0x7fffffff,0xffffffff);
                  FUN_00013490(0);
                  __ZN8PMString12AppendNumberERK6PMRealiss(auStack_3a0,auStack_3b8,0,1);
                  __ZN8PMString6AppendEPKciNS_14StringEncodingE
                            (auStack_3a0," ",0x7fffffff,0xffffffff);
                  __ZN8PMString6AppendEPKciNS_14StringEncodingE
                            (auStack_3a0,"lines",0x7fffffff,0xffffffff);
                  __ZN8PMString15SetTranslatableEs(auStack_3a0,0);
                  FUN_00032c44(auStack_e8,auStack_3a0);
                  __ZN8PMStringD1Ev(auStack_3a0);
                }
              }
            }
            else {
              iVar1 = 0;
              if (local_2e0 != 0) {
                iVar1 = local_2dc / local_2e0;
              }
              if ((local_2dc == iVar1 * local_2e0) && (local_2e0 < 0x29)) {
                __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_330,"",0);
                FUN_00013490((double)(long)local_2e0);
                __ZN8PMString12AppendNumberERK6PMRealiss(auStack_330,auStack_338,0,1);
                __ZN8PMString6AppendEPKciNS_14StringEncodingE(auStack_330," ",0x7fffffff,0xffffffff)
                ;
                __ZN8PMString6AppendERKS_i(auStack_330,local_38,0x7fffffff);
                __ZN8PMString6AppendEPKciNS_14StringEncodingE
                          (auStack_330," (",0x7fffffff,0xffffffff);
                iVar1 = 0;
                if (local_2e0 != 0) {
                  iVar1 = local_2dc / local_2e0;
                }
                FUN_00013490((double)iVar1);
                __ZN8PMString12AppendNumberERK6PMRealiss(auStack_330,auStack_340,0,1);
                __ZN8PMString6AppendEPKciNS_14StringEncodingE
                          (auStack_330," lines);",0x7fffffff,0xffffffff);
                __ZN8PMString6AppendEPKciNS_14StringEncodingE
                          (auStack_330," gutter: ",0x7fffffff,0xffffffff);
                FUN_00013490(0);
                __ZN8PMString12AppendNumberERK6PMRealiss(auStack_330,auStack_348,0,1);
                __ZN8PMString6AppendEPKciNS_14StringEncodingE(auStack_330," ",0x7fffffff,0xffffffff)
                ;
                __ZN8PMString6AppendEPKciNS_14StringEncodingE
                          (auStack_330,"lines",0x7fffffff,0xffffffff);
                __ZN8PMString15SetTranslatableEs(auStack_330,0);
                FUN_00032c44(auStack_e8,auStack_330);
                __ZN8PMStringD1Ev(auStack_330);
              }
            }
          }
          plVar6 = (long *)FUN_0000e2c8(auStack_80);
          local_3bc = (**(code **)(*plVar6 + 0xf8))();
          if (local_3bc == 0) {
            local_3c8 = FUN_00026ed0();
            local_3d0 = FUN_00026f34(auStack_e8);
            FUN_000c54f8(local_3c8,local_3d0,FUN_001d2528);
          }
          else if (local_3bc == 1) {
            local_3d8 = FUN_00026ed0();
            local_3e0 = FUN_00026f34(auStack_e8);
            FUN_000c54f8(local_3d8,local_3e0,FUN_001d2648);
          }
          else {
            local_3e8 = FUN_00026ed0();
            local_3f0 = FUN_00026f34(auStack_e8);
            FUN_000c54f8(local_3e8,local_3f0,FUN_001d2768);
          }
          plVar6 = (long *)FUN_0000e2c8(auStack_80);
          sVar4 = (**(code **)(*plVar6 + 0x108))();
          if (sVar4 != 0) {
            local_3f8 = FUN_00026ed0();
            local_400 = FUN_00026f34(auStack_e8);
            FUN_000c5540(local_3f8,local_400);
          }
          __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_448,"No ",0);
          uVar5 = local_30;
          FUN_00002cf8(auStack_44c,0x15d4c7);
          uVar7 = FUN_0001417c(uVar5,auStack_44c);
          uVar5 = local_30;
          local_6cc = 1;
          if ((uVar7 & 1) == 0) {
            FUN_00002cf8(auStack_450,&DAT_0015d374);
            local_6cc = FUN_0001417c(uVar5,auStack_450);
          }
          uVar5 = local_30;
          if ((local_6cc & 1) == 0) {
            FUN_00002cf8(auStack_454,0x15d4c8);
            uVar7 = FUN_0001417c(uVar5,auStack_454);
            uVar5 = local_30;
            if ((uVar7 & 1) == 0) {
              FUN_00002cf8(auStack_458,0x15d4cc);
              uVar7 = FUN_0001417c(uVar5,auStack_458);
              uVar5 = local_30;
              if ((uVar7 & 1) == 0) {
                FUN_00002cf8(auStack_45c,0x15d4c9);
                uVar7 = FUN_0001417c(uVar5,auStack_45c);
                uVar5 = local_30;
                local_714 = 1;
                if ((uVar7 & 1) == 0) {
                  FUN_00002cf8(auStack_460,&DAT_0015d37a);
                  local_714 = FUN_0001417c(uVar5,auStack_460);
                }
                uVar5 = local_30;
                if ((local_714 & 1) == 0) {
                  FUN_00002cf8(auStack_464,0x15d3fe);
                  uVar7 = FUN_0001417c(uVar5,auStack_464);
                  uVar5 = local_30;
                  if ((uVar7 & 1) == 0) {
                    FUN_00002cf8(auStack_468,0x15d4cf);
                    uVar7 = FUN_0001417c(uVar5,auStack_468);
                    if ((uVar7 & 1) != 0) {
                      __ZN8PMString6AppendEPKciNS_14StringEncodingE
                                (auStack_448,"Sec. Rows",0x7fffffff,0xffffffff);
                    }
                  }
                  else {
                    __ZN8PMString6AppendEPKciNS_14StringEncodingE
                              (auStack_448,"Subrows",0x7fffffff,0xffffffff);
                  }
                }
                else {
                  __ZN8PMString6AppendEPKciNS_14StringEncodingE
                            (auStack_448,"Rows",0x7fffffff,0xffffffff);
                }
              }
              else {
                __ZN8PMString6AppendEPKciNS_14StringEncodingE
                          (auStack_448,"Sec. Columns",0x7fffffff,0xffffffff);
              }
            }
            else {
              __ZN8PMString6AppendEPKciNS_14StringEncodingE
                        (auStack_448,"Subcolumns",0x7fffffff,0xffffffff);
            }
          }
          else {
            __ZN8PMString6AppendEPKciNS_14StringEncodingE
                      (auStack_448,"Columns",0x7fffffff,0xffffffff);
          }
          __ZN8PMString15SetTranslatableEs(auStack_448,0);
          plVar6 = (long *)FUN_00014854(auStack_a0);
          (**(code **)(*plVar6 + 0x18))(plVar6,auStack_448,0xfffffffe,1);
          for (local_46c = 0; uVar8 = (ulong)local_46c, uVar7 = FUN_00013560(auStack_e8),
              uVar8 < uVar7; local_46c = local_46c + 1) {
            plVar6 = (long *)FUN_00014854(auStack_a0);
            uVar5 = FUN_00026e00(auStack_e8,(long)local_46c);
            (**(code **)(*plVar6 + 0x18))(plVar6,uVar5,0xfffffffe,1);
          }
          __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_4b8,"Custom Setting Applied",0);
          plVar6 = (long *)FUN_00014854(auStack_a0);
          (**(code **)(*plVar6 + 0x18))(plVar6,auStack_4b8,0xfffffffe,1);
          local_4bc = FUN_0001867c(auStack_60,local_30,auStack_4b8);
          FUN_00015844(auStack_60,local_30,&local_4bc,&DAT_00208c50);
          __ZN8PMStringD1Ev(auStack_4b8);
          __ZN8PMStringD1Ev(auStack_448);
          FUN_000136c8(auStack_e8);
          local_78 = 0;
        }
        else {
          local_78 = 2;
        }
        FUN_00014a30(auStack_a0);
      }
      else {
        local_78 = 2;
      }
      FUN_00002b9c(auStack_88);
    }
    else {
      local_78 = 2;
    }
    FUN_0000e2f8(auStack_80);
  }
  else {
    local_78 = 2;
  }
  FUN_00002c54(auStack_60);
  return;
}

//==== FUNC @ 1d1c74 -> 001d1c74

undefined4 FUN_001d1c74(undefined8 param_1,undefined8 param_2)

{
  undefined8 uVar1;
  undefined4 uVar2;
  int iVar3;
  int iVar4;
  long lVar5;
  undefined1 auStack_f8 [72];
  long local_b0;
  undefined1 auStack_98 [72];
  long local_50;
  undefined4 local_44;
  undefined8 local_40;
  undefined8 local_38;
  long local_30;
  long local_28;
  
  local_44 = 0;
  local_40 = param_2;
  local_38 = param_1;
  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_98," ");
  uVar2 = __ZNK8PMString13IndexOfStringERKS_i(param_1,auStack_98,0);
  lVar5 = __ZNK8PMString9SubstringEii(param_1,0,uVar2);
  __ZN8PMStringD1Ev(auStack_98);
  uVar1 = local_40;
  local_50 = lVar5;
  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_f8," ");
  uVar2 = __ZNK8PMString13IndexOfStringERKS_i(uVar1,auStack_f8,0);
  lVar5 = __ZNK8PMString9SubstringEii(uVar1,0,uVar2);
  __ZN8PMStringD1Ev(auStack_f8);
  local_b0 = lVar5;
  iVar3 = __ZNK8PMString11GetAsNumberEPNS_15ConversionErrorEPi(local_50,0);
  iVar4 = __ZNK8PMString11GetAsNumberEPNS_15ConversionErrorEPi(local_b0,0);
  if (iVar3 < iVar4) {
    local_44 = 1;
  }
  else {
    iVar3 = __ZNK8PMString11GetAsNumberEPNS_15ConversionErrorEPi(local_50,0);
    iVar4 = __ZNK8PMString11GetAsNumberEPNS_15ConversionErrorEPi(local_b0,0);
    if (iVar4 < iVar3) {
      local_44 = 0xffffffff;
    }
  }
  lVar5 = local_50;
  if (local_50 != 0) {
    __ZN8PMStringD1Ev(local_50);
    local_28 = lVar5;
    __ZN8K2Memory27RTLCompatibleDeleteDelegateEPv(lVar5);
  }
  lVar5 = local_b0;
  if (local_b0 != 0) {
    __ZN8PMStringD1Ev(local_b0);
    local_30 = lVar5;
    __ZN8K2Memory27RTLCompatibleDeleteDelegateEPv(lVar5);
  }
  return local_44;
}

//==== FUNC @ 1d1ed0 -> 001d1ed0

undefined4 FUN_001d1ed0(undefined8 param_1,undefined8 param_2)

{
  long lVar1;
  undefined8 uVar2;
  int iVar3;
  int iVar4;
  long lVar5;
  undefined1 auStack_190 [76];
  int local_144;
  undefined1 auStack_140 [76];
  int local_f4;
  long local_f0;
  undefined1 auStack_e8 [72];
  int local_a0;
  undefined1 auStack_90 [72];
  int local_48;
  undefined4 local_44;
  undefined8 local_40;
  undefined8 local_38;
  long local_30;
  long local_28;
  
  local_44 = 0;
  local_40 = param_2;
  local_38 = param_1;
  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_90,"(");
  iVar3 = __ZNK8PMString13IndexOfStringERKS_i(param_1,auStack_90,0);
  __ZN8PMStringD1Ev(auStack_90);
  uVar2 = local_38;
  local_48 = iVar3 + 1;
  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_e8," lines",0);
  iVar3 = __ZNK8PMString13IndexOfStringERKS_i(uVar2,auStack_e8,local_48);
  __ZN8PMStringD1Ev(auStack_e8);
  local_a0 = iVar3;
  local_f0 = __ZNK8PMString9SubstringEii(local_38,local_48,iVar3 - local_48);
  uVar2 = local_40;
  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_140,"(");
  iVar3 = __ZNK8PMString13IndexOfStringERKS_i(uVar2,auStack_140,0);
  __ZN8PMStringD1Ev(auStack_140);
  uVar2 = local_40;
  local_f4 = iVar3 + 1;
  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_190," lines",0);
  iVar3 = __ZNK8PMString13IndexOfStringERKS_i(uVar2,auStack_190,local_f4);
  __ZN8PMStringD1Ev(auStack_190);
  local_144 = iVar3;
  lVar5 = __ZNK8PMString9SubstringEii(local_40,local_f4,iVar3 - local_f4);
  iVar3 = __ZNK8PMString11GetAsNumberEPNS_15ConversionErrorEPi(local_f0,0);
  iVar4 = __ZNK8PMString11GetAsNumberEPNS_15ConversionErrorEPi(lVar5,0);
  if (iVar3 < iVar4) {
    local_44 = 1;
  }
  else {
    iVar3 = __ZNK8PMString11GetAsNumberEPNS_15ConversionErrorEPi(local_f0,0);
    iVar4 = __ZNK8PMString11GetAsNumberEPNS_15ConversionErrorEPi(lVar5,0);
    if (iVar4 < iVar3) {
      local_44 = 0xffffffff;
    }
  }
  lVar1 = local_f0;
  if (local_f0 != 0) {
    __ZN8PMStringD1Ev(local_f0);
    local_28 = lVar1;
    __ZN8K2Memory27RTLCompatibleDeleteDelegateEPv(lVar1);
  }
  if (lVar5 != 0) {
    __ZN8PMStringD1Ev(lVar5);
    local_30 = lVar5;
    __ZN8K2Memory27RTLCompatibleDeleteDelegateEPv(lVar5);
  }
  return local_44;
}

//==== FUNC @ 1d21fc -> 001d21fc

undefined4 FUN_001d21fc(undefined8 param_1,undefined8 param_2)

{
  long lVar1;
  undefined8 uVar2;
  int iVar3;
  int iVar4;
  long lVar5;
  undefined1 auStack_190 [76];
  int local_144;
  undefined1 auStack_140 [76];
  int local_f4;
  long local_f0;
  undefined1 auStack_e8 [72];
  int local_a0;
  undefined1 auStack_90 [72];
  int local_48;
  undefined4 local_44;
  undefined8 local_40;
  undefined8 local_38;
  long local_30;
  long local_28;
  
  local_44 = 0;
  local_40 = param_2;
  local_38 = param_1;
  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_90,"gutter: ");
  iVar3 = __ZNK8PMString13IndexOfStringERKS_i(param_1,auStack_90,0);
  __ZN8PMStringD1Ev(auStack_90);
  uVar2 = local_38;
  local_48 = iVar3 + 8;
  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_e8," line",0);
  iVar3 = __ZNK8PMString13IndexOfStringERKS_i(uVar2,auStack_e8,local_48);
  __ZN8PMStringD1Ev(auStack_e8);
  local_a0 = iVar3;
  local_f0 = __ZNK8PMString9SubstringEii(local_38,local_48,iVar3 - local_48);
  uVar2 = local_40;
  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_140,"gutter: ");
  iVar3 = __ZNK8PMString13IndexOfStringERKS_i(uVar2,auStack_140,0);
  __ZN8PMStringD1Ev(auStack_140);
  uVar2 = local_40;
  local_f4 = iVar3 + 8;
  __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_190," line",0);
  iVar3 = __ZNK8PMString13IndexOfStringERKS_i(uVar2,auStack_190,local_f4);
  __ZN8PMStringD1Ev(auStack_190);
  local_144 = iVar3;
  lVar5 = __ZNK8PMString9SubstringEii(local_40,local_f4,iVar3 - local_f4);
  iVar3 = __ZNK8PMString11GetAsNumberEPNS_15ConversionErrorEPi(local_f0,0);
  iVar4 = __ZNK8PMString11GetAsNumberEPNS_15ConversionErrorEPi(lVar5,0);
  if (iVar3 < iVar4) {
    local_44 = 1;
  }
  else {
    iVar3 = __ZNK8PMString11GetAsNumberEPNS_15ConversionErrorEPi(local_f0,0);
    iVar4 = __ZNK8PMString11GetAsNumberEPNS_15ConversionErrorEPi(lVar5,0);
    if (iVar4 < iVar3) {
      local_44 = 0xffffffff;
    }
  }
  lVar1 = local_f0;
  if (local_f0 != 0) {
    __ZN8PMStringD1Ev(local_f0);
    local_28 = lVar1;
    __ZN8K2Memory27RTLCompatibleDeleteDelegateEPv(lVar1);
  }
  if (lVar5 != 0) {
    __ZN8PMStringD1Ev(lVar5);
    local_30 = lVar5;
    __ZN8K2Memory27RTLCompatibleDeleteDelegateEPv(lVar5);
  }
  return local_44;
}

//==== FUNC @ 1d2528 -> 001d2528

undefined2 FUN_001d2528(undefined8 param_1,undefined8 param_2)

{
  int iVar1;
  undefined2 local_22;
  
  iVar1 = FUN_001d1c74(param_1,param_2);
  if (iVar1 == 1) {
    local_22 = 1;
  }
  else if (iVar1 == -1) {
    local_22 = 0;
  }
  else {
    iVar1 = FUN_001d1ed0(param_1,param_2);
    if (iVar1 == 1) {
      local_22 = 1;
    }
    else if (iVar1 == -1) {
      local_22 = 0;
    }
    else {
      iVar1 = FUN_001d21fc(param_1,param_2);
      if (iVar1 == 1) {
        local_22 = 1;
      }
      else {
        local_22 = 0;
      }
    }
  }
  return local_22;
}

//==== FUNC @ 1d2648 -> 001d2648

undefined2 FUN_001d2648(undefined8 param_1,undefined8 param_2)

{
  int iVar1;
  undefined2 local_22;
  
  iVar1 = FUN_001d1ed0(param_1,param_2);
  if (iVar1 == 1) {
    local_22 = 1;
  }
  else if (iVar1 == -1) {
    local_22 = 0;
  }
  else {
    iVar1 = FUN_001d1c74(param_1,param_2);
    if (iVar1 == 1) {
      local_22 = 1;
    }
    else if (iVar1 == -1) {
      local_22 = 0;
    }
    else {
      iVar1 = FUN_001d21fc(param_1,param_2);
      if (iVar1 == 1) {
        local_22 = 1;
      }
      else {
        local_22 = 0;
      }
    }
  }
  return local_22;
}

//==== FUNC @ 1d2768 -> 001d2768

undefined2 FUN_001d2768(undefined8 param_1,undefined8 param_2)

{
  int iVar1;
  undefined2 local_22;
  
  iVar1 = FUN_001d21fc(param_1,param_2);
  if (iVar1 == 1) {
    local_22 = 1;
  }
  else if (iVar1 == -1) {
    local_22 = 0;
  }
  else {
    iVar1 = FUN_001d1c74(param_1,param_2);
    if (iVar1 == 1) {
      local_22 = 1;
    }
    else if (iVar1 == -1) {
      local_22 = 0;
    }
    else {
      iVar1 = FUN_001d1ed0(param_1,param_2);
      if (iVar1 == 1) {
        local_22 = 1;
      }
      else {
        local_22 = 0;
      }
    }
  }
  return local_22;
}

//==== FUNC @ 1256bc -> 001256bc

/* WARNING: Restarted to delay deadcode elimination for space: stack */

void FUN_001256bc(undefined8 param_1,undefined8 param_2)

{
  short sVar1;
  undefined8 uVar2;
  long *plVar3;
  ulong uVar4;
  undefined1 auStack_1dc [4];
  undefined1 auStack_1d8 [4];
  undefined1 auStack_1d4 [4];
  int local_1d0;
  undefined1 auStack_1cc [4];
  undefined1 auStack_1c8 [4];
  undefined1 auStack_1c4 [4];
  undefined4 local_1c0;
  undefined1 auStack_1bc [4];
  undefined1 auStack_1b8 [76];
  undefined1 auStack_16c [4];
  undefined1 auStack_168 [4];
  undefined1 auStack_164 [4];
  int local_160;
  undefined1 auStack_15c [4];
  undefined1 auStack_158 [4];
  undefined1 auStack_154 [4];
  undefined1 auStack_150 [4];
  undefined1 auStack_14c [4];
  undefined1 auStack_148 [4];
  undefined1 auStack_144 [4];
  undefined1 auStack_140 [72];
  undefined1 auStack_f8 [72];
  int local_b0;
  int local_ac;
  int local_a8;
  undefined1 auStack_a4 [4];
  undefined1 auStack_a0 [4];
  undefined1 auStack_9c [4];
  undefined1 auStack_98 [4];
  undefined1 auStack_94 [4];
  undefined1 auStack_90 [4];
  undefined1 auStack_8c [4];
  undefined1 auStack_88 [4];
  undefined1 auStack_84 [4];
  undefined1 auStack_80 [7];
  undefined1 uStack_79;
  undefined1 local_78 [16];
  undefined1 auStack_68 [15];
  undefined1 uStack_59;
  undefined1 auStack_58 [8];
  undefined4 local_50;
  undefined1 uStack_39;
  undefined1 auStack_38 [8];
  undefined8 local_30;
  undefined8 local_28;
  
  local_30 = param_2;
  local_28 = param_1;
  FUN_00002bf4(auStack_38,param_1,&uStack_39);
  sVar1 = FUN_00002c30(auStack_38);
  if (sVar1 == 0) {
    uVar2 = __Z26GetExecutionContextSessionv();
    FUN_00002978(auStack_58,uVar2,&uStack_59);
    sVar1 = FUN_000029b4(auStack_58);
    if (sVar1 == 0) {
      plVar3 = (long *)FUN_000029d8(auStack_58);
      local_78 = (**(code **)(*plVar3 + 0x28))();
      FUN_00017bd4(auStack_68,local_78,&uStack_79);
      sVar1 = FUN_00017c10(auStack_68);
      if (sVar1 == 0) {
        FUN_00030a74(auStack_80);
        uVar2 = local_30;
        FUN_00002cf8(auStack_84,0x15d4d2);
        uVar4 = FUN_0001417c(uVar2,auStack_84);
        uVar2 = local_30;
        if ((uVar4 & 1) == 0) {
          FUN_00002cf8(auStack_88,0x15d4d3);
          uVar4 = FUN_0001417c(uVar2,auStack_88);
          uVar2 = local_30;
          if ((uVar4 & 1) == 0) {
            FUN_00002cf8(auStack_8c,0x15d4d4);
            uVar4 = FUN_0001417c(uVar2,auStack_8c);
            uVar2 = local_30;
            if ((uVar4 & 1) == 0) {
              FUN_00002cf8(auStack_90,0x15d4d5);
              uVar4 = FUN_0001417c(uVar2,auStack_90);
              uVar2 = local_30;
              if ((uVar4 & 1) == 0) {
                FUN_00002cf8(auStack_94,0x15d3ff);
                uVar4 = FUN_0001417c(uVar2,auStack_94);
                uVar2 = local_30;
                if ((uVar4 & 1) == 0) {
                  FUN_00002cf8(auStack_98,0x15d4d6);
                  uVar4 = FUN_0001417c(uVar2,auStack_98);
                  uVar2 = local_30;
                  if ((uVar4 & 1) == 0) {
                    FUN_00002cf8(auStack_9c,&DAT_0015d375);
                    uVar4 = FUN_0001417c(uVar2,auStack_9c);
                    uVar2 = local_30;
                    if ((uVar4 & 1) == 0) {
                      FUN_00002cf8(auStack_a0,&DAT_0015d37e);
                      uVar4 = FUN_0001417c(uVar2,auStack_a0);
                      uVar2 = local_30;
                      if ((uVar4 & 1) == 0) {
                        FUN_00002cf8(auStack_a4,&DAT_0015d37b);
                        uVar4 = FUN_0001417c(uVar2,auStack_a4);
                        if ((uVar4 & 1) != 0) {
                          FUN_00016b9c(auStack_80,&DAT_00208d5c);
                        }
                      }
                      else {
                        FUN_00016b9c(auStack_80,&DAT_00208d58);
                      }
                    }
                    else {
                      FUN_00016b9c(auStack_80,&DAT_00208d54);
                    }
                  }
                  else {
                    FUN_00016b9c(auStack_80,&DAT_00208d50);
                  }
                }
                else {
                  FUN_00016b9c(auStack_80,&DAT_00208d4c);
                }
              }
              else {
                FUN_00016b9c(auStack_80,&DAT_00208d48);
              }
            }
            else {
              FUN_00016b9c(auStack_80,&DAT_00208d44);
            }
          }
          else {
            FUN_00016b9c(auStack_80,&DAT_00208d40);
          }
        }
        else {
          FUN_00016b9c(auStack_80,&DAT_00208d3c);
        }
        local_ac = FUN_00013034(auStack_38,auStack_80);
        local_b0 = FUN_0001853c(auStack_38,auStack_80);
        FUN_000138a8(auStack_f8,auStack_38,local_30);
        __ZN8PMStringC1EPKcNS_19TranslateDuringCallE(auStack_140,"0x15d300kGCGreaterKey",0);
        sVar1 = __ZNK8PMString7IsEqualERKS_hh(auStack_f8,auStack_140,1,0);
        __ZN8PMStringD1Ev(auStack_140);
        if (sVar1 == 0) {
          if (local_ac == 0) {
            local_a8 = local_b0 + -2;
          }
          else {
            local_a8 = local_ac + -1;
          }
        }
        else if ((local_ac == local_b0 + -2) || (local_ac == local_b0 + -1)) {
          local_a8 = 0;
        }
        else {
          local_a8 = local_ac + 1;
        }
        FUN_00015b04(auStack_38,auStack_80,&local_a8,1);
        uVar2 = local_30;
        FUN_00002cf8(auStack_144,0x15d4d2);
        uVar4 = FUN_0001417c(uVar2,auStack_144);
        uVar2 = local_30;
        if ((uVar4 & 1) == 0) {
          FUN_00002cf8(auStack_148,0x15d4d3);
          uVar4 = FUN_0001417c(uVar2,auStack_148);
          uVar2 = local_30;
          if ((uVar4 & 1) == 0) {
            FUN_00002cf8(auStack_14c,0x15d4d4);
            uVar4 = FUN_0001417c(uVar2,auStack_14c);
            uVar2 = local_30;
            if ((uVar4 & 1) == 0) {
              FUN_00002cf8(auStack_150,0x15d4d5);
              uVar4 = FUN_0001417c(uVar2,auStack_150);
              uVar2 = local_30;
              if ((uVar4 & 1) == 0) {
                FUN_00002cf8(auStack_154,0x15d3ff);
                uVar4 = FUN_0001417c(uVar2,auStack_154);
                uVar2 = local_30;
                if ((uVar4 & 1) == 0) {
                  FUN_00002cf8(auStack_158,0x15d4d6);
                  uVar4 = FUN_0001417c(uVar2,auStack_158);
                  uVar2 = local_30;
                  if ((uVar4 & 1) == 0) {
                    FUN_00002cf8(auStack_15c,&DAT_0015d375);
                    uVar4 = FUN_0001417c(uVar2,auStack_15c);
                    uVar2 = local_30;
                    if ((uVar4 & 1) == 0) {
                      FUN_00002cf8(auStack_16c,&DAT_0015d37e);
                      uVar4 = FUN_0001417c(uVar2,auStack_16c);
                      uVar2 = local_30;
                      if ((uVar4 & 1) == 0) {
                        FUN_00002cf8(auStack_1cc,&DAT_0015d37b);
                        uVar4 = FUN_0001417c(uVar2,auStack_1cc);
                        if ((uVar4 & 1) != 0) {
                          FUN_00002cf8(auStack_1d4,&DAT_0015d37a);
                          local_1d0 = FUN_00013034(auStack_38,auStack_1d4);
                          FUN_00002cf8(auStack_1d8,0x15d4c9);
                          FUN_00015b04(auStack_38,auStack_1d8,&local_1d0,1);
                          FUN_00002cf8(auStack_1dc,0x15d4c9);
                          FUN_0010b924(param_1,auStack_1dc,&DAT_00208c50);
                          if (local_1d0 < 1) {
                            plVar3 = (long *)FUN_00017c34(auStack_68);
                            (**(code **)(*plVar3 + 0x48))(plVar3,&DAT_00208c50);
                          }
                          else {
                            plVar3 = (long *)FUN_00017c34(auStack_68);
                            (**(code **)(*plVar3 + 0x48))(plVar3,&DAT_00208c52);
                          }
                          FUN_0010df8c(param_1);
                        }
                      }
                      else {
                        FUN_00002cf8(auStack_1bc,&DAT_0015d37d);
                        local_1c0 = 0xffffffff;
                        FUN_00012c10(auStack_1b8,auStack_38,auStack_1bc,&local_1c0);
                        FUN_00002cf8(auStack_1c4,0x15d4cc);
                        FUN_000191b0(auStack_38,auStack_1c4,auStack_1b8,1);
                        FUN_0010ae20(param_1);
                        FUN_00002cf8(auStack_1c8,&DAT_0015d37d);
                        FUN_000191b0(auStack_38,auStack_1c8,auStack_1b8,1);
                        __ZN8PMStringD1Ev(auStack_1b8);
                      }
                    }
                    else {
                      FUN_00002cf8(auStack_164,&DAT_0015d374);
                      local_160 = FUN_00013034(auStack_38,auStack_164);
                      FUN_00002cf8(auStack_168,0x15d4c7);
                      FUN_00015b04(auStack_38,auStack_168,&local_160,1);
                      FUN_00108ac4(param_1,&DAT_00208c50);
                      if (local_160 < 1) {
                        plVar3 = (long *)FUN_00017c34(auStack_68);
                        (**(code **)(*plVar3 + 0x38))(plVar3,&DAT_00208c50);
                      }
                      else {
                        plVar3 = (long *)FUN_00017c34(auStack_68);
                        (**(code **)(*plVar3 + 0x38))(plVar3,&DAT_00208c52);
                      }
                      FUN_0010a5d4(param_1);
                    }
                  }
                  else {
                    FUN_0010b924(param_1,auStack_80,&DAT_00208c50);
                  }
                }
                else {
                  FUN_0014186c(param_1);
                }
              }
              else {
                FUN_0010b924(param_1,auStack_80,&DAT_00208c50);
                FUN_0010df8c(param_1);
              }
            }
            else {
              FUN_0010ae20(param_1);
            }
          }
          else {
            FUN_00140f34(param_1);
          }
        }
        else {
          FUN_00108ac4(param_1,&DAT_00208c50);
          FUN_0010a5d4(param_1);
        }
        __ZN8PMStringD1Ev(auStack_f8);
        local_50 = 0;
      }
      else {
        local_50 = 2;
      }
      FUN_00017ca4(auStack_68);
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
