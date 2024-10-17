using System;
using UnityEngine;

namespace Agent
{
    public class TankProjectile : MonoBehaviour
    {
        public static Action<int,int,int> OnTankKilled;
        public int damage;
        public float launchForce = 10f;
        private int tankId;
        private int teamId;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void Launch(Vector3 direction, int tankId, int teamId)
        {
            this.tankId = tankId;
            this.teamId = teamId;
            rb.AddForce(direction * launchForce, ForceMode.VelocityChange);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(collision.gameObject.CompareTag("Tank"))
            {
                TankBase tank = collision.gameObject.GetComponent<TankBase>();
                
                if(tank.TakeDamage(damage))
                {
                    OnTankKilled.Invoke(tankId, teamId, tank.team);
                }
            }
            Destroy(gameObject);
        }
        
    }
}