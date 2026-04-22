using Genesis.Terraria.Components;
using Genesis.Terraria.ECS;
using Gnosis.ECS;

namespace Genesis.Terraria.Systems;

public sealed class DayNightSystem : ISystem
{
    private readonly EcsWorld _world;
    private EntityId? _dayTimeEntity;

    public SystemPhase Phase => SystemPhase.PreUpdate;

    public DayNightSystem(EcsWorld world)
    {
        _world = world;
    }

    public void Initialize()
    {
        foreach (var entity in _world.QueryEntities<DayTime>())
        {
            _dayTimeEntity = entity;
            break;
        }

        if (!_dayTimeEntity.HasValue)
        {
            var entity = _world.CreateEntity();
            _world.AddComponent(entity, new DayTime(0.25f, 600.0f));
            _dayTimeEntity = entity;
        }
    }

    public void Update(float delta)
    {
        if (!_dayTimeEntity.HasValue)
        {
            return;
        }

        ref var dayTime = ref _world.GetComponentRef<DayTime>(_dayTimeEntity.Value);

        dayTime.TimeOfDay += delta / dayTime.DayDuration;

        if (dayTime.TimeOfDay >= 1.0f)
        {
            dayTime.TimeOfDay -= 1.0f;
            dayTime.DayCount++;
            Console.WriteLine($"[Terraria] 第 {dayTime.DayCount} 天开始了！");
        }
    }

    public void Shutdown()
    {
    }
}
