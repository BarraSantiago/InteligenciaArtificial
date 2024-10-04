using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class ObstacleManager : MonoBehaviour
{
    private const float DISTANCE_BETWEEN_OBSTACLES = 6f;
    private const float HEIGHT_RANDOM = 3f;
    private const int MIN_COUNT = 3;
    [FormerlySerializedAs("prefab")] public GameObject obstaclePrefab;
    public GameObject coinPrefab;
    Vector3 obstaclePos = new Vector3(DISTANCE_BETWEEN_OBSTACLES, 0, 0);
    Vector3 coinPos = new Vector3(DISTANCE_BETWEEN_OBSTACLES/2, 0, 0);

    List<Obstacle> obstacles = new List<Obstacle>();
    List<Coin> coins = new List<Coin>();

    private static ObstacleManager instance = null;

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
        for (int i = 0; i < obstacles.Count; i++)
            Destroy(obstacles[i].gameObject);
        
        for (int i = 0; i < coins.Count; i++)
            Destroy(coins[i].gameObject);

        obstacles.Clear();
        coins.Clear();

        obstaclePos.x = 0;

        InstantiateObstacle();
        InstantiateCoin();
        InstantiateObstacle();
        InstantiateCoin();
    }

  

    public Obstacle GetNextObstacle(Vector3 pos)
    {
        for (int i = 0; i < obstacles.Count; i++)
        {
            if (pos.x < obstacles[i].transform.position.x + 2f)
                return obstacles[i];
        }

        return null;
    }
    
    public Coin GetNextCoin(Vector3 pos)
    {
        for (int i = 0; i < coins.Count; i++)
        {
            if (pos.x < coins[i].transform.position.x + 2f)
                return coins[i];
        }

        return null;
    }

    public bool IsColliding(Vector3 pos)
    {
        Collider2D collider = Physics2D.OverlapBox(pos, new Vector2(0.3f, 0.3f), 0);

        if (collider)
            return true;

        return false;
    }

    public void CheckAndInstatiate()
    {
        for (int i = 0; i < obstacles.Count; i++)
        {
            obstacles[i].CheckToDestroy();
        }

        for (int i = 0; i < coins.Count; i++)
        {
            coins[i].CheckToDestroy();
        }

        while (obstacles.Count < MIN_COUNT)
            InstantiateObstacle();
        
        while (coins.Count < MIN_COUNT-1)
            InstantiateCoin();
    }

    void InstantiateObstacle()
    {
        obstaclePos.x += DISTANCE_BETWEEN_OBSTACLES;
        obstaclePos.y = Random.Range(-HEIGHT_RANDOM, HEIGHT_RANDOM);
        GameObject go = Instantiate(obstaclePrefab, obstaclePos, Quaternion.identity);
        
        Obstacle obstacle = go.GetComponent<Obstacle>();
        obstacle.OnDestroy += OnObstacleDestroy;
        obstacles.Add(obstacle);
    }
    
    private void InstantiateCoin()
    {
        coinPos.x += DISTANCE_BETWEEN_OBSTACLES;
        coinPos.y = Random.Range(-HEIGHT_RANDOM, HEIGHT_RANDOM);
        GameObject coinGo = Instantiate(coinPrefab, coinPos, Quaternion.identity);
        coinGo.transform.SetParent(transform, false);
        Coin coin = coinGo.GetComponent<Coin>();
        coin.OnDestroy += OnCoinDestroy;
        coins.Add(coin);
    }

    void OnObstacleDestroy(Obstacle obstacle)
    {
        obstacle.OnDestroy -= OnObstacleDestroy;
        obstacles.Remove(obstacle);
    }
    
    void OnCoinDestroy(Coin coin)
    {
        coin.OnDestroy -= OnCoinDestroy;
        coins.Remove(coin);
    }
}
