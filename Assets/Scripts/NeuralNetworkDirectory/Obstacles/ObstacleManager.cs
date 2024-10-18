using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace FlappyIa.Obstacles
{
    public class ObstacleManager : MonoBehaviour
    {
        private const float DISTANCE_BETWEEN_OBSTACLES = 6f;
        private const float HEIGHT_RANDOM = 3f;
        private const int MIN_COUNT = 3;

        private static ObstacleManager instance;
        [FormerlySerializedAs("prefab")] public GameObject obstaclePrefab;
        public GameObject coinPrefab;
        private Vector3 coinPos = new(DISTANCE_BETWEEN_OBSTACLES / 2, 0, 0);
        private readonly List<Coin> coins = new();
        private Vector3 obstaclePos = new(DISTANCE_BETWEEN_OBSTACLES, 0, 0);

        private readonly List<Obstacle> obstacles = new();

        public static ObstacleManager Instance
        {
            get
            {
                if (!instance)
                    instance = FindObjectOfType<ObstacleManager>();

                return instance;
            }
        }

        private void Awake()
        {
            instance = this;
        }

        public void Reset()
        {
            for (var i = 0; i < obstacles.Count; i++)
                Destroy(obstacles[i].gameObject);

            for (var i = 0; i < coins.Count; i++)
                Destroy(coins[i].gameObject);

            obstacles.Clear();
            coins.Clear();

            obstaclePos.x = 0;
            coinPos.x = DISTANCE_BETWEEN_OBSTACLES / 2;

            InstantiateObstacle();
            InstantiateCoin();
            InstantiateObstacle();
            InstantiateCoin();
        }


        public Obstacle GetNextObstacle(Vector3 pos)
        {
            for (var i = 0; i < obstacles.Count; i++)
                if (pos.x < obstacles[i].transform.position.x + 2f)
                    return obstacles[i];

            return null;
        }

        public Coin GetNextCoin(Vector3 pos)
        {
            var nearestCoin = coins[0];
            var nearestDist = float.MaxValue;

            foreach (var coin in coins)
                if (coin.transform.position.x > pos.x && coin.transform.position.x - pos.x < nearestDist)
                {
                    nearestCoin = coin;
                    nearestDist = coin.transform.position.x - pos.x;
                }

            return nearestCoin;
        }

        public bool IsColliding(Vector3 pos)
        {
            var collider = Physics2D.OverlapBox(pos, new Vector2(0.3f, 0.3f), 0);

            if (collider)
                return true;

            return false;
        }

        public void CheckAndInstatiate()
        {
            for (var i = 0; i < obstacles.Count; i++) obstacles[i].CheckToDestroy();

            for (var i = 0; i < coins.Count; i++) coins[i].CheckToDestroy();

            while (obstacles.Count < MIN_COUNT)
                InstantiateObstacle();

            while (coins.Count < MIN_COUNT - 1)
                InstantiateCoin();
        }

        private void InstantiateObstacle()
        {
            obstaclePos.x += DISTANCE_BETWEEN_OBSTACLES;
            obstaclePos.y = Random.Range(-HEIGHT_RANDOM, HEIGHT_RANDOM);
            var go = Instantiate(obstaclePrefab, obstaclePos, Quaternion.identity);

            var obstacle = go.GetComponent<Obstacle>();
            obstacle.OnDestroy += OnObstacleDestroy;
            obstacles.Add(obstacle);
        }

        private void InstantiateCoin()
        {
            coinPos.x += DISTANCE_BETWEEN_OBSTACLES;
            coinPos.y = Random.Range(-HEIGHT_RANDOM, HEIGHT_RANDOM);
            var coinGo = Instantiate(coinPrefab, coinPos, Quaternion.identity);
            coinGo.transform.SetParent(transform, false);
            var coin = coinGo.GetComponent<Coin>();
            coin.id = coins.Count;
            coin.OnDestroy += OnCoinDestroy;
            coins.Add(coin);
        }

        private void OnObstacleDestroy(Obstacle obstacle)
        {
            obstacle.OnDestroy -= OnObstacleDestroy;
            obstacles.Remove(obstacle);
        }

        private void OnCoinDestroy(Coin coin)
        {
            coin.OnDestroy -= OnCoinDestroy;
            coins.Remove(coin);
        }
    }
}