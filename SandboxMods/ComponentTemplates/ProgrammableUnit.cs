// Decompiled with JetBrains decompiler
// Type: LogicGate
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 93E6D51E-7C50-4F44-83A7-82AAAF7248C3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll

using KSerialization;
using STRINGS;
using System;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
public class ProgrammableUnit : LogicGateBase, ILogicNetworkConnection, ISaveLoadable, ISim200ms
{
    protected override void OnSpawn()
    {
    }


    public void OnLogicNetworkConnectionChanged(bool connected)
    {
        throw new NotImplementedException();
    }

    public void Sim200ms(float dt)
    {
        throw new NotImplementedException();
    }



}
