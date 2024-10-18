using System;
using NeuralNetworkDirectory.Agent;
using UnityEngine;

namespace Agent
{
    public class TankProjectile : MonoBehaviour
    {
        public static Action<int, int, int> OnTankKilled;
        public int damage;
        public float launchForce = 10f;

        private Rigidbody rb;
        private int tankId;
        private int teamId;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Tank"))
            {
                var tank = collision.gameObject.GetComponent<TankBase>();

                if (tank.TakeDamage(damage)) OnTankKilled.Invoke(tankId, teamId, tank.team);
            }

            Destroy(gameObject);
        }

        public void Launch(Vector3 direction, int tankId, int teamId)
        {
            this.tankId = tankId;
            this.teamId = teamId;
            rb.AddForce(direction * launchForce, ForceMode.VelocityChange);
        }
    }
}