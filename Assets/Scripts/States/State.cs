using System;
using System.Collections.Generic;
using StateMachine;
using UnityEngine;

public abstract class State
{
    public Action<int> OnFlag;
    public abstract List<Action> GetTickBehaviour(params object[] parameters);
    public abstract List<Action> GetOnEnterBehaviour(params object[] parameters);
    public abstract List<Action> GetOnExitBehaviour(params object[] parameters);
}



