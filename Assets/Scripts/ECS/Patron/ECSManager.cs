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
        var ecsEntity = new ECSEntity();
        entities.TryAdd(ecsEntity.GetID(), ecsEntity);
        return ecsEntity.GetID();
    }

    public static void AddComponent<TComponentType>(uint entityID, TComponentType component)
        where TComponentType : ECSComponent
    {
        component.EntityOwnerID = entityID;
        entities[entityID].AddComponentType(typeof(TComponentType));
        components[typeof(TComponentType)].TryAdd(entityID, component);
    }

    public static bool ContainsComponent<TComponentType>(uint entityID) where TComponentType : ECSComponent
    {
        return entities[entityID].ContainsComponentType<TComponentType>();
    }


    public static IEnumerable<uint> GetEntitiesWithComponentTypes(params Type[] componentTypes)
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


    public static ConcurrentDictionary<uint, TComponentType> GetComponents<TComponentType>()
        where TComponentType : ECSComponent
    {
        if (!components.ContainsKey(typeof(TComponentType))) return null;

        ConcurrentDictionary<uint, TComponentType> comps = new ConcurrentDictionary<uint, TComponentType>();

        Parallel.ForEach(components[typeof(TComponentType)], parallelOptions,
            component => { comps.TryAdd(component.Key, component.Value as TComponentType); });

        return comps;
    }

    public static TComponentType GetComponent<TComponentType>(uint entityID) where TComponentType : ECSComponent
    {
        return components[typeof(TComponentType)][entityID] as TComponentType;
    }


    public static void RemoveComponent<TComponentType>(uint entityID) where TComponentType : ECSComponent
    {
        components[typeof(TComponentType)].TryRemove(entityID, out _);
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

    public static void AddFlag<TFlagType>(uint entityID, TFlagType flag)
        where TFlagType : ECSFlag
    {
        flag.EntityOwnerID = entityID;
        entities[entityID].AddComponentType(typeof(TFlagType));
        flags[typeof(TFlagType)].TryAdd(entityID, flag);
    }

    public static bool ContainsFlag<TFlagType>(uint entityID) where TFlagType : ECSFlag
    {
        return entities[entityID].ContainsFlagType<TFlagType>();
    }
    
    public static ConcurrentDictionary<uint, TFlagType> GetFlags<TFlagType>() where TFlagType : ECSFlag
    {
        if (!flags.ContainsKey(typeof(TFlagType))) return null;

        ConcurrentDictionary<uint, TFlagType> flgs = new ConcurrentDictionary<uint, TFlagType>();

        Parallel.ForEach(flags[typeof(TFlagType)], parallelOptions,
            flag => { flgs.TryAdd(flag.Key, flag.Value as TFlagType); });

        return flgs;
    }

    public static TFlagType GetFlag<TFlagType>(uint entityID) where TFlagType : ECSFlag
    {
        return flags[typeof(TFlagType)][entityID] as TFlagType;
    }

    public static void RemoveFlag<TFlagType>(uint entityID) where TFlagType : ECSFlag
    {
        flags[typeof(TFlagType)].TryRemove(entityID, out _);
    }
}