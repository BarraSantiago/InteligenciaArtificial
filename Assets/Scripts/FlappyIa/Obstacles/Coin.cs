using UnityEngine;

public class Coin : MonoBehaviour
{
    public System.Action<Coin> OnDestroy;

    public void CheckToDestroy()
    {
        if (this.transform.position.x - Camera.main.transform.position.x < -7.5f)
        {
            if (OnDestroy != null)
                OnDestroy.Invoke(this);

            Destroy(this.gameObject);
        }

    }
}