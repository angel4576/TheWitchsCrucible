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
            // 将 moveSpeed 设为 0
            monsterScript.moveSpeed = 0f;

            // 将 chaseRange 设为 0
            monsterScript.chaseRange = 0f;

            // 将 Monster 的位置重置为 (0, 0, 0)
            monster.transform.position = Vector3.zero;

            // 锁定 Monster 的位置，使其不可移动
            Rigidbody2D rb = monster.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;   // 停止所有运动
                rb.angularVelocity = 0f;      // 停止旋转
                rb.isKinematic = true;        // 将物体设置为 Kinematic，禁用物理引擎的影响
            }

            // 禁用 Monster 脚本
            monsterScript.enabled = false;

            // 禁用 Monster 的 Mesh Renderer（让物体不可见）
            MeshRenderer meshRenderer = monster.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false; // 禁用渲染器，使物体不可见
            }

            Debug.Log(monster.name + " moveSpeed, chaseRange set to 0, position reset to (0,0,0), locked, Monster script disabled, and MeshRenderer disabled.");
        }
        else
        {
            Debug.LogWarning("Monster script not found on " + monster.name);
        }
    }
}