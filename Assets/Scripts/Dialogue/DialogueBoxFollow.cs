using UnityEngine;

public class DialogueBoxFollow : MonoBehaviour
{
    public float offsetX;
    public float offsetY;
    public Transform playerTransform;
    public RectTransform dialogueBoxRectTransform;
    public Camera mainCamera;

    void Update()
    {
        if (playerTransform != null)
        {
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(playerTransform.position);

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)dialogueBoxRectTransform.parent,
                screenPosition,
                mainCamera,
                out localPoint
            );

            dialogueBoxRectTransform.anchoredPosition = localPoint + new Vector2(offsetX, offsetY);
        }
        else
        {
            Debug.LogWarning("PlayerTransform is null! Check your scene setup.");
        }
    }
}