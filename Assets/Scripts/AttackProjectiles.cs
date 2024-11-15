using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackProjectiles : MonoBehaviour
{
    [SerializeField]
    private float projectileSpeed;
    private float width;
    private float height;
    private float maxDistance;
    private bool isFacingRight;
    private bool hitEnemy;
    // Start is called before the first frame update
    void Start()
    {
        width = GetComponent<SpriteRenderer>().bounds.size.x;
        height = GetComponent<SpriteRenderer>().bounds.size.y;
        if (GameObject.FindWithTag("Player").GetComponent<PlayerController>().isFacingRight())
        {
            isFacingRight = true;
            maxDistance = GameObject.FindWithTag("Player").transform.position.x + GameObject.FindWithTag("Player").GetComponent<PlayerController>().rangeAttackRange;
        }
        else
        {
            isFacingRight = false;
            maxDistance = GameObject.FindWithTag("Player").transform.position.x - GameObject.FindWithTag("Player").GetComponent<PlayerController>().rangeAttackRange;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Collider2D[] enemiesInsideArea = Physics2D.OverlapAreaAll(new Vector2(transform.position.x - width / 2, transform.position.y - height / 2), new Vector2(transform.position.x + width / 2, transform.position.y + height / 2));
        foreach (Collider2D currEnemy in enemiesInsideArea)
        {
            if (currEnemy.gameObject.CompareTag("Monster"))
            {
                currEnemy.gameObject.GetComponent<Monster>().TakeDamage(GameObject.FindWithTag("Player").GetComponent<PlayerController>().rangeAttackDamage);
                Destroy(gameObject);
            }
        }
        Vector3 newPosition = transform.position;
        if (isFacingRight)
        {
            newPosition.x = newPosition.x + projectileSpeed;
            transform.position = newPosition;
            if(transform.position.x >= maxDistance)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            newPosition.x = newPosition.x - projectileSpeed;
            transform.position = newPosition;
            if (transform.position.x <= maxDistance)
            {
                Destroy(gameObject);
            }
        }
    }
}
