/*
Modular unit (row 05). FUN_00016bc0: u = H / modules / subdivisions, a blank module count becomes Round(H / moduleSize).

PMReal helpers: construct 0x13490, operator/ 0x157c8, operator* 0x15a7c, operator+ 0x17c4c, operator- 0x28080,
Round 0x15abc (floor(x + 0.5)), floor 0x37d70, a > b + 1e-8 0x14140, a >= b - 1e-8 0x15808, a <= b + 1e-8 0x31d38,
a < b - 1e-8 0x3208c, |a - b| < 1e-8 0x132fc, not equal 0x18504, quantise 0x152f0 (mode 3 three decimals, mode 4 four).
Widget access: read value 0x13338, read value in the panel unit 0x1544c, read checkbox 0x12ef4, read dropdown index 0x13034,
write value 0x13c20, set maximum 0x196d0, unit factor at object + 0x88.
*/

//==== FUNC @ 16dd8 -> 00016bc0

undefined1  [16] FUN_00016bc0(undefined8 param_1,ulong *param_2)

{
  ulong *puVar1;
  undefined8 uVar2;
  short sVar3;
  int iVar4;
  long *plVar5;
  undefined1 auVar6 [16];
  undefined1 auStack_140 [8];
  ulong local_138;
  undefined1 auStack_12c [4];
  undefined8 local_128;
  ulong local_120;
  undefined8 local_118;
  undefined1 auStack_110 [12];
  undefined1 auStack_104 [4];
  undefined8 local_100;
  undefined8 local_f8;
  undefined8 local_f0;
  undefined8 local_e8;
  undefined1 auStack_e0 [12];
  undefined1 auStack_d4 [4];
  undefined8 local_d0;
  undefined1 auStack_c8 [12];
  undefined1 auStack_bc [4];
  undefined8 local_b8;
  undefined1 auStack_ac [4];
  undefined8 local_a8;
  undefined4 local_a0;
  undefined1 auStack_9c [4];
  undefined1 auStack_98 [72];
  undefined4 local_50;
  undefined1 auStack_40 [8];
  ulong *local_38;
  undefined8 local_30;
  ulong local_28;
  
  local_38 = param_2;
  local_30 = param_1;
  FUN_00013490(0,&local_28);
  sVar3 = FUN_00002c30(local_30);
  if (sVar3 == 0) {
    FUN_001a7968();
    sVar3 = FUN_00016b78(auStack_40);
    uVar2 = local_30;
    if (sVar3 == 0) {
      FUN_00002cf8(auStack_9c,0x15d306);
      local_a0 = 0xffffffff;
      FUN_00012c10(auStack_98,uVar2,auStack_9c,&local_a0);
      uVar2 = local_30;
      FUN_00002cf8(auStack_ac,&DAT_0015d31b);
      local_a8 = FUN_0001544c(uVar2,auStack_ac,auStack_98);
      uVar2 = local_30;
      FUN_00002cf8(auStack_bc,0x15d31c);
      local_b8 = FUN_00013338(uVar2,auStack_bc);
      FUN_00013490(0);
      sVar3 = FUN_000132fc(&local_b8,auStack_c8);
      uVar2 = local_30;
      if (sVar3 == 0) {
        FUN_00002cf8(auStack_12c,0x15d4a3);
        sVar3 = FUN_00012ef4(uVar2,auStack_12c);
        puVar1 = local_38;
        if (sVar3 == 0) {
          plVar5 = (long *)FUN_00017c8c(auStack_40);
          iVar4 = (**(code **)(*plVar5 + 0xa0))();
          FUN_00013490((double)iVar4);
          local_138 = FUN_000157c8(puVar1,auStack_140);
          local_28 = local_138;
        }
        else {
          local_28 = *local_38;
        }
      }
      else {
        FUN_00002cf8(auStack_d4,0x15d342);
        local_d0 = FUN_00013338(uVar2,auStack_d4);
        FUN_00013490(0);
        sVar3 = FUN_000132fc(&local_d0,auStack_e0);
        if (sVar3 != 0) {
          plVar5 = (long *)FUN_00017c8c(auStack_40);
          local_f8 = (**(code **)(*plVar5 + 0x90))();
          local_f0 = FUN_000157c8(&local_a8,&local_f8);
          local_e8 = FUN_00015abc(&local_f0);
          local_d0 = local_e8;
        }
        uVar2 = local_30;
        FUN_00002cf8(auStack_104,0x15d344);
        local_100 = FUN_00013338(uVar2,auStack_104);
        FUN_00013490(0);
        sVar3 = FUN_000132fc(&local_100,auStack_110);
        if (sVar3 != 0) {
          plVar5 = (long *)FUN_00017c8c(auStack_40);
          iVar4 = (**(code **)(*plVar5 + 0xa0))();
          FUN_00013490((double)iVar4,&local_118);
          local_100 = local_118;
        }
        local_128 = FUN_000157c8(&local_a8,&local_d0);
        local_120 = FUN_000157c8(&local_128,&local_100);
        local_28 = local_120;
      }
      __ZN8PMStringD1Ev(auStack_98);
      local_50 = 0;
    }
    else {
      local_50 = 2;
    }
    FUN_00017158(auStack_40);
  }
  auVar6._8_8_ = 0;
  auVar6._0_8_ = local_28;
  return auVar6;
}
