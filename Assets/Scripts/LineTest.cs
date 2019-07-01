﻿using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class LineTest : MonoBehaviour
{
	public MeshEntityRotatable line;

	// Start is called before the first frame update
	void Start()
	{
		var e = line.Instantiate(new Vector3(50, 10, 50));
		var em = World.Active.EntityManager;
		em.AddComponentData(e, new LineSegment
		{
			Start = new float3(0, 10, 0),
			End = new float3(100, 10, 100),
		});
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}