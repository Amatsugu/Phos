﻿using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class TimedDeathSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity e, ref DeathTime t) =>
        {
            if (Time.ElapsedTime >= t.Value)
                PostUpdateCommands.DestroyEntity(e);
        });
    }

    public struct DeathTime : IComponentData
    {
        public double Value;
    }
}