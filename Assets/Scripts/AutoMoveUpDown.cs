using UnityEngine;

public class AutoMoveUpDown : MonoBehaviour
{
    public float speed = 2f; // 移动速度
    public float maxHeight = 5f; // 最高位置
    public float minHeight = 0f; // 最低位置

    private bool isMovingUp = true; // 当前移动方向

    void Update()
    {
        // 根据当前移动方向更新物体位置
        if (isMovingUp)
        {
            MoveUp();
        }
        else
        {
            MoveDown();
        }
    }

    // 向上移动
    void MoveUp()
    {
        // 计算新位置
        Vector2 newPosition = transform.position;
        newPosition.y += speed * Time.deltaTime;

        // 如果到达最高点，改变方向
        if (newPosition.y >= maxHeight)
        {
            newPosition.y = maxHeight;
            isMovingUp = false;
        }

        // 更新物体位置
        transform.position = newPosition;
    }

    // 向下移动
    void MoveDown()
    {
        // 计算新位置
        Vector2 newPosition = transform.position;
        newPosition.y -= speed * Time.deltaTime;

        // 如果到达最低点，改变方向
        if (newPosition.y <= minHeight)
        {
            newPosition.y = minHeight;
            isMovingUp = true;
        }

        // 更新物体位置
        transform.position = newPosition;
    }
}