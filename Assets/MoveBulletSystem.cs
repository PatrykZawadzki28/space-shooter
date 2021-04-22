using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;
using Unity.Physics;

// system runs jobs in background (no attachment to an scene or object needed)
public class MoveBulletSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float dT = Time.DeltaTime;

        var jobHandle = Entities
            .WithName("MoveBulletSystem")
            .ForEach((ref PhysicsVelocity physics, // pararell loop (thanks to multi core)
                        ref Translation position,
                        ref Rotation rotation,
                        ref BulletData bulletData) =>
            {
                physics.Angular = float3.zero; // keep going forward without stop
                physics.Linear += dT * bulletData.speed * math.forward(rotation.Value);
            })
            .Schedule(inputDeps);

        jobHandle.Complete();

        return jobHandle;
    }
}