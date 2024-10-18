using UnityEngine;

public class MovementMonoBehaviour : MonoBehaviour
{
    public float velocity;

    private void LateUpdate()
    {
        transform.position += Vector3.right * velocity * Time.deltaTime;
    }
}