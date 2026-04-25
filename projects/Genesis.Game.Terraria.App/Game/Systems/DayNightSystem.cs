using Gnosis.ECS.System;
using ECSWorld = Gnosis.ECS.World.World;

namespace Genesis.Game.Terraria.Game.Systems;

public sealed class DayNightSystem : IWorldSystem
{
    private World _world = null!;

    public SystemPhase Phase => SystemPhase.Update;

    public void SetWorld(ECSWorld world) => _world = world;
    public void Initialize() { }
    public void Shutdown() { }

    public void Update(float delta)
    {
        var daytimeQuery = _world.CreateQuery()
            .All<DayTime>()
            .Build();

        foreach (var entityId in daytimeQuery)
        {
            var daytime = _world.GetComponent<DayTime>(entityId);
            daytime.TimeOfDay += delta / daytime.DayDuration;

            if (daytime.TimeOfDay >= 1.0f)
            {
                daytime.TimeOfDay -= 1.0f;
                daytime.DayCount += 1;
                Console.WriteLine($"[Terraria] 第 {daytime.DayCount} 天开始了！");
            }

            _world.SetComponent(entityId, daytime);
        }
    }
}
