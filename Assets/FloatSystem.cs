using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;
using Unity.Physics;

// system runs jobs in background (no attachment to an scene or object needed)
public class FloatSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float dT = Time.DeltaTime;

        var jobHandle = Entities
            .WithName("FloatSystem")
            .ForEach((ref PhysicsVelocity physics, // pararell loop (thanks to multi core)
                        ref Translation position,
                        ref Rotation rotation,
                        ref FloatData floatData) =>
            {
                // f.e. .Random won't work
                float s = math.sin((dT + position.Value.x) * 0.5f) * floatData.speed; // entity speed
                float c = math.cos((dT + position.Value.y) * 0.5f) * floatData.speed;

                float3 dir = new float3(s, c, s);
                physics.Linear += dir;
            })
            .Schedule(inputDeps);

        jobHandle.Complete();

        return jobHandle;
    }
}