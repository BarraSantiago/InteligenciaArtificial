using Agent;
using UnityEngine;

namespace FlappyIa.AI
{
    public class GameManager : MonoBehaviour
    {
        private PopulationManager[] populationManagers;
        
        public void Init()
        {
            populationManagers = FindObjectsOfType<PopulationManager>();
            
            TankProjectile.OnTankKilled += OnTankKilled;
        }

        private void OnTankKilled(int arg1, int arg2, int arg3)
        {
            populationManagers[arg2].OnTankKilled(arg1, arg3);
        }
    }
}