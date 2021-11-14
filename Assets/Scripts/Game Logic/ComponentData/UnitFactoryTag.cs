using System.Collections;
using System.Collections.Generic;

using Unity.Entities;

using UnityEngine;

namespace Amatsugu.Phos
{
    public struct UnitFactoryTag : IComponentData
    {
    }

    public struct FactoryReadyTag : IComponentData
    { }
}
