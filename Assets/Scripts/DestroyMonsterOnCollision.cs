using UnityEngine;

public class DestroyMonsterOnCollision : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            StopMonster(collision.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            StopMonster(other.gameObject);
        }
    }

    private void StopMonster(GameObject monster)
    {
        Monster monsterScript = monster.GetComponent<Monster>();
        if (monsterScript != null)
        {
            monsterScript.moveSpeed = 0f; // 将 Monster 脚本中的 moveSpeed 设为 0
            Debug.Log(monster.name + " moveSpeed set to 0.");
        }
        else
        {
            Debug.LogWarning("Monster script not found on " + monster.name);
        }
    }
}