using UnityEngine;

public class GameMissile : MonoBehaviour
{
    public string Id { get; set; }
    public float Speed = 5f;

    void Update()
    {
        // Mueve el misil hacia adelante según su velocidad
        transform.Translate(Vector2.up * Speed * Time.deltaTime);

        Destroy(this.gameObject, 5f);
    }
}
