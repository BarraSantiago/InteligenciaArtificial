using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECS.Patron;

public static class ECSManager
{
    private static readonly ParallelOptions parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 32 };

    private static ConcurrentDictionary<uint, ECSEntity> entities = null;
    private static ConcurrentDictionary<Type, ConcurrentDictionary<uint, ECSComponent>> components = null;
    private static ConcurrentDictionary<Type, ConcurrentDictionary<uint, ECSFlag>> flags = null;
    private static ConcurrentDictionary<Type, ECSSystem> systems = null;

    public static void Init()
    {
        entities = new ConcurrentDictionary<uint, ECSEntity>();
        components = new ConcurrentDictionary<Type, ConcurrentDictionary<uint, ECSComponent>>();
        flags = new ConcurrentDictionary<Type, ConcurrentDictionary<uint, ECSFlag>>();
        systems = new ConcurrentDictionary<Type, ECSSystem>();

        foreach (Type classType in typeof(ECSSystem).Assembly.GetTypes())
        {
            if (typeof(ECSSystem).IsAssignableFrom(classType) && !classType.IsAbstract)
            {
                systems.TryAdd(classType, Activator.CreateInstance(classType) as ECSSystem);
            }
        }

        foreach (KeyValuePair<Type, ECSSystem> system in systems)
        {
            system.Value.Initialize();
        }

        foreach (Type classType in typeof(ECSComponent).Assembly.GetTypes())
        {
            if (typeof(ECSComponent).IsAssignableFrom(classType) && !classType.IsAbstract)
            {
                components.TryAdd(classType, new ConcurrentDictionary<uint, ECSComponent>());
            }
        }

        foreach (Type classType in typeof(ECSFlag).Assembly.GetTypes())
        {
            if (typeof(ECSFlag).IsAssignableFrom(classType) && !classType.IsAbstract)
            {
                flags.TryAdd(classType, new ConcurrentDictionary<uint, ECSFlag>());
            }
        }
    }

    public static void Tick(float deltaTime)
    {
        Parallel.ForEach(systems, parallelOptions, system => { system.Value.Run(deltaTime); });
    }

    public static uint CreateEntity()
    {
        ECSEntity ecsEntity;
        ecsEntity = new ECSEntity();
        entities.TryAdd(ecsEntity.GetID(), ecsEntity);
        return ecsEntity.GetID();
    }

    public static void AddComponent<ComponentType>(uint entityID, ComponentType component)
        where ComponentType : ECSComponent
    {
        component.EntityOwnerID = entityID;
        entities[entityID].AddComponentType(typeof(ComponentType));
        components[typeof(ComponentType)].TryAdd(entityID, component);
    }

    public static bool ContainsComponent<ComponentType>(uint entityID) where ComponentType : ECSComponent
    {
        return entities[entityID].ContainsComponentType<ComponentType>();
    }


    public static IEnumerable<uint> GetEntitiesWhitComponentTypes(params Type[] componentTypes)
    {
        ConcurrentBag<uint> matchs = new ConcurrentBag<uint>();
        Parallel.ForEach(entities, parallelOptions, entity =>
        {
            for (int i = 0; i < componentTypes.Length; i++)
            {
                if (!entity.Value.ContainsComponentType(componentTypes[i]))
                    return;
            }

            matchs.Add(entity.Key);
        });
        return matchs;
    }


    public static ConcurrentDictionary<uint, ComponentType> GetComponents<ComponentType>()
        where ComponentType : ECSComponent
    {
        if (!components.ContainsKey(typeof(ComponentType))) return null;

        ConcurrentDictionary<uint, ComponentType> comps = new ConcurrentDictionary<uint, ComponentType>();

        Parallel.ForEach(components[typeof(ComponentType)], parallelOptions,
            component => { comps.TryAdd(component.Key, component.Value as ComponentType); });

        return comps;
    }

    public static ComponentType GetComponent<ComponentType>(uint entityID) where ComponentType : ECSComponent
    {
        return components[typeof(ComponentType)][entityID] as ComponentType;
    }


    public static void RemoveComponent<ComponentType>(uint entityID) where ComponentType : ECSComponent
    {
        components[typeof(ComponentType)].TryRemove(entityID, out _);
    }

    public static IEnumerable<uint> GetEntitiesWhitFlagTypes(params Type[] flagTypes)
    {
        ConcurrentBag<uint> matchs = new ConcurrentBag<uint>();
        Parallel.ForEach(entities, parallelOptions, entity =>
        {
            for (int i = 0; i < flagTypes.Length; i++)
            {
                if (!entity.Value.ContainsFlagType(flagTypes[i]))
                    return;
            }

            matchs.Add(entity.Key);
        });
        return matchs;
    }

    public static void AddFlag<FlagType>(uint entityID, FlagType flag)
        where FlagType : ECSFlag
    {
        flag.EntityOwnerID = entityID;
        entities[entityID].AddComponentType(typeof(FlagType));
        flags[typeof(FlagType)].TryAdd(entityID, flag);
    }

    public static bool ContainsFlag<FlagType>(uint entityID) where FlagType : ECSFlag
    {
        return entities[entityID].ContainsFlagType<FlagType>();
    }
    
    public static ConcurrentDictionary<uint, FlagType> GetFlags<FlagType>() where FlagType : ECSFlag
    {
        if (!flags.ContainsKey(typeof(FlagType))) return null;

        ConcurrentDictionary<uint, FlagType> flgs = new ConcurrentDictionary<uint, FlagType>();

        Parallel.ForEach(flags[typeof(FlagType)], parallelOptions,
            flag => { flgs.TryAdd(flag.Key, flag.Value as FlagType); });

        return flgs;
    }

    public static FlagType GetFlag<FlagType>(uint entityID) where FlagType : ECSFlag
    {
        return flags[typeof(FlagType)][entityID] as FlagType;
    }

    public static void RemoveFlag<FlagType>(uint entityID) where FlagType : ECSFlag
    {
        flags[typeof(FlagType)].TryRemove(entityID, out _);
    }
}