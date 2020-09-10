using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

[GenerateAuthoringComponent]
public struct CharacterControllerData : IComponentData
{
    public float radius; //(.25)
    public float height; //(2)
    public float skin; //distace out from our controller to check for objects we are touching, set to something small (0.01)
    public float maxAngle;//max angle we do not slide down (45)
    public bool onGround; //we read this but usually do not set.
    public float3 footOffset; //we are going to poison the collider by its base instead of it's center to save on some math and mental energy
    public float3 moveDelta; //distance to try to move, like the move() function
    public LayerMask layersToIgnore;//an easy way to set layers, use the getter below for the actual code

    //Some things to make our life much easer
    public float3 center = &gt; footOffset + new float3(0, height / 2, 0);
    public float3 vertexTop = &gt; footOffset + new float3(0, height - radius, 0);
    public float3 vertexBottom = &gt; footOffset + new float3(0, radius, 0);
    public float3 top = &gt; footOffset + new float3(0, height, 0);
    public CollisionFilter Filter
    {
        get
        {
            return new CollisionFilter()
            {
                BelongsTo = (uint)(~layersToIgnore.value),
                CollidesWith = (uint)(~layersToIgnore.value)
            };
        }
    }
}