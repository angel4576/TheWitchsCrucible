using UnityEngine;

public class MoveLeft : MonoBehaviour
{
    public float speed = 5f;  // Speed

    void Update()
    {
        // move towards left
        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }
}
