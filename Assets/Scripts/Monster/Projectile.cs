using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed;
    public float lifetime;
    public float damage;

    public Vector2 direction;

    public void Initialize(float speed, float lifetime, float damage, Vector2 direction)
    {
        this.speed = speed;
        this.lifetime = lifetime;
        this.damage = damage;
        this.direction = direction;

        // rotate the projectile to face the direction it's moving
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(gameObject, lifetime); // destroy the projectile after lifetime seconds
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().TakeDamage(damage, transform);
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject); // destroy the projectile if it hits anything other than the player
        }
    }



}
