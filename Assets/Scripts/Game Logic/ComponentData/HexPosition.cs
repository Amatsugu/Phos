﻿using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct HexPosition : IComponentData
{
	public HexCoords coords;
}
