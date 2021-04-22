using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;
using Unity.Physics;

// system runs jobs in background (no attachment to an scene or object needed)
public class TimedDestroySystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float dT = Time.DeltaTime; // time of entity

        Entities
            .WithoutBurst().WithStructuralChanges() // allow to have structural changes
            .ForEach((Entity entity,
                        ref Translation position,
                        ref LifeTimeData lifeTimeData) =>
            {
                lifeTimeData.lifeLeft -= dT;
                if (lifeTimeData.lifeLeft <= 0f)
                    EntityManager.DestroyEntity(entity);
            })
            .Run();

        Entities
            .WithoutBurst().WithStructuralChanges() // allow to have structural changes
            .ForEach((Entity entity,
                        ref Translation position,
                        ref VirusData virusData) =>
            {
                if (!virusData.alive)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        float3 offset = (float3)UnityEngine.Random.insideUnitSphere * 2.0f;
                        var splat = Manager.manager.Instantiate(Manager.whiteBlood);
                        float3 randomDir = new float3(UnityEngine.Random.Range(-1, 1),
                            UnityEngine.Random.Range(-1, 1),
                            UnityEngine.Random.Range(-1, 1));

                        Manager.manager.SetComponentData(splat, new Translation { Value = position.Value + offset });
                        Manager.manager.SetComponentData(splat, new PhysicsVelocity { Linear = randomDir * 2.0f });
                    }
                    EntityManager.DestroyEntity(entity);
                }
            })
            .Run();

        return inputDeps;
    }
}