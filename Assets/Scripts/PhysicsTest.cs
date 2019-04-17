using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;

public class PhysicsTest : MonoBehaviour
{

	public MeshEntity meshEntity;
    // Start is called before the first frame update
    void Start()
    {
		var em = World.Active.EntityManager;
		var e = meshEntity.Instantiate(new Vector3(-7, 1.5f, -5));
		em.AddComponent(e, typeof(PhysicsCollider));
	}

    // Update is called once per frame
    void Update()
    {
    }
}
