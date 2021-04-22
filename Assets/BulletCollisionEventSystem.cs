using Unity.Physics.Systems;
using Unity.Physics;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

// system runs jobs in background (no attachment to an scene or object needed)

    // declare to run after last frame of physics
[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class BulletCollisionEventSystem : JobComponentSystem
{
    BuildPhysicsWorld m_BuildPhysicsWorldSystem;
    StepPhysicsWorld m_StepPhysicsWorldSystem;
    
    protected override void OnCreate()
    {
        m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
        m_StepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
    }
    // collision event 
    struct CollisionEventImpulseJob: ICollisionEventsJob
    {
        // need only to read (optimization)
        [ReadOnly] public ComponentDataFromEntity<BulletData> BulletGroup;
        public ComponentDataFromEntity<VirusData> VirusGroup;

        public void Execute(CollisionEvent collisionEvent)
        {
            Entity entityA = collisionEvent.Entities.EntityA;
            Entity entityB = collisionEvent.Entities.EntityB;

            bool IsTargetA = VirusGroup.Exists(entityA);
            bool IsTargetB = VirusGroup.Exists(entityB);

            bool isBulletA = BulletGroup.Exists(entityA);
            bool isBulletB = BulletGroup.Exists(entityB);

            if (isBulletA && IsTargetB)
            {
                var aliveComponent = VirusGroup[entityB];
                aliveComponent.alive = false;
                VirusGroup[entityB] = aliveComponent;
            }

            if (isBulletB && IsTargetA)
            {
                var aliveComponent = VirusGroup[entityA];
                aliveComponent.alive = false;
                VirusGroup[entityA] = aliveComponent;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        JobHandle jobHandle = new CollisionEventImpulseJob
        {
            BulletGroup = GetComponentDataFromEntity<BulletData>(),
            VirusGroup = GetComponentDataFromEntity<VirusData>()
        }
        .Schedule(m_StepPhysicsWorldSystem.Simulation, ref m_BuildPhysicsWorldSystem.PhysicsWorld, inputDeps);

        jobHandle.Complete();

        return jobHandle;
    }
}