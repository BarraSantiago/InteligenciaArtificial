using System;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public abstract class State
{
    public Action<int> OnFlag;
    public abstract List<Action> GetTickBehaviour(params object[] parameters);
    public abstract List<Action> GetOnEnterBehaviour(params object[] parameters);
    public abstract List<Action> GetOnExitBehaviour(params object[] parameters);
}

public sealed class ChaseState : State
{
    public override List<Action> GetTickBehaviour(params object[] parameters)
    {
        Transform OwnerTransform = parameters[0] as Transform;
        Transform TargetTransform = parameters[1] as Transform;
        float speed = Convert.ToSingle(parameters[2]);
        float explodeDistance = Convert.ToSingle(parameters[3]);
        float lostDistance = Convert.ToSingle(parameters[4]);

        List<Action> behaviours = new List<Action>();

        behaviours.Add(() => { Debug.Log("Whistle!"); });

        behaviours.Add(() =>
        {
            OwnerTransform.position += (TargetTransform.position - OwnerTransform.position).normalized * (speed * Time.deltaTime);
        });

        behaviours.Add(() =>
        {
            if (Vector3.Distance(TargetTransform.position, OwnerTransform.position) < explodeDistance)
            {
                OnFlag?.Invoke((int)FLags.OnTargetReach);
            }
        });

        behaviours.Add(() =>
        {
            if (Vector3.Distance(TargetTransform.position, OwnerTransform.position) > lostDistance)
            {
                OnFlag?.Invoke((int)FLags.OnTargetLost);
            }
        });

        return behaviours;
    }

    public override List<Action> GetOnEnterBehaviour(params object[] parameters)
    {
        return new List<Action>();
    }

    public override List<Action> GetOnExitBehaviour(params object[] parameters)
    {
        return new List<Action>();
    }
}

public sealed class PatrolState : State
{
    private bool direction;

    public override List<Action> GetTickBehaviour(params object[] parameters)
    {
        List<Action> behaviours = new List<Action>();

        Transform ownerTransform = parameters[0] as Transform;
        Transform wayPoint1 = parameters[1] as Transform;
        Transform wayPoint2 = parameters[2] as Transform;
        Transform chaseTarget = parameters[3] as Transform;
        float speed = Convert.ToSingle(parameters[4]);
        float chaseDistance = Convert.ToSingle(parameters[5]);

        behaviours.Add(() =>
        {
            if (Vector3.Distance(ownerTransform.position, direction ? wayPoint1.position : wayPoint2.position) < 0.2f)
            {
                direction = !direction;
            }

            ownerTransform.position +=
                (direction ? wayPoint1.position : wayPoint2.position - ownerTransform.position).normalized * (speed * Time.deltaTime);
        });

        behaviours.Add(() =>
        {
            if (Vector3.Distance(ownerTransform.position, chaseTarget.position) < chaseDistance)
            {
                OnFlag?.Invoke((int)FLags.OnTargetNear);
            }
        });

        return behaviours;
    }

    public override List<Action> GetOnEnterBehaviour(params object[] parameters)
    {
        return new List<Action>();
    }

    public override List<Action> GetOnExitBehaviour(params object[] parameters)
    {
        return new List<Action>();
    }
}

public sealed class ExplodeState : State
{
    public override List<Action> GetTickBehaviour(params object[] parameters)
    {
        List<Action> behaviours = new List<Action>();
        behaviours.Add(() => { Debug.Log("F"); });
        
        GameObject ownerObject = parameters[0] as GameObject;
        
        behaviours.Add(() =>
        {
            ownerObject.SetActive( false);
        });
        
        return behaviours;
    }

    public override List<Action> GetOnEnterBehaviour(params object[] parameters)
    {
        List<Action> behaviours = new List<Action>();
        behaviours.Add(() => { Debug.Log("Explode!"); });
        
        
        return behaviours;
    }

    public override List<Action> GetOnExitBehaviour(params object[] parameters)
    {
        return new List<Action>();
    }
}