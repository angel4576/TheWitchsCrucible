using System.Collections;
using UnityEngine;

[ExecuteAlways]
public class TriggerImageAnimator : MonoBehaviour
{
    [Header("Trigger Settings")]
    public string playerTag = "Player";

    [Header("Animation Settings")]
    public Sprite frame1;
    public Sprite frame2;
    public float frameSwitchInterval = 0.5f;
    public Vector2 displayPosition = Vector2.zero;
    public Vector2 imageSize = Vector2.one;
    public Material spriteMaterial;

    [Header("Rendering Settings")]
    public string sortingLayerName = "Default";
    public int orderInLayer = 0;

    private GameObject displayObject;
    private SpriteRenderer spriteRenderer;
    private Coroutine animationCoroutine;

    private void OnEnable()
    {
        SetupDisplayObject();
        UpdateEditorPreview();
    }

    private void OnValidate()
    {
        SetupDisplayObject();
        UpdateEditorPreview();
    }

    private void SetupDisplayObject()
    {
        if (displayObject == null)
        {
            displayObject = new GameObject("AnimatedImage_" + GetInstanceID());
            displayObject.hideFlags = HideFlags.DontSave;
            spriteRenderer = displayObject.AddComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = displayObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = displayObject.AddComponent<SpriteRenderer>();
        }

        // 设置材质（如果指定）
        if (spriteMaterial != null)
        {
            spriteRenderer.sharedMaterial = spriteMaterial;
        }

        // 设置 sorting layer 和 order
        spriteRenderer.sortingLayerName = sortingLayerName;
        spriteRenderer.sortingOrder = orderInLayer;

        UpdateDisplayTransform();
    }

    private void UpdateDisplayTransform()
    {
        if (displayObject != null)
        {
            displayObject.transform.position = displayPosition;
            displayObject.transform.localScale = new Vector3(imageSize.x, imageSize.y, 1f);
        }
    }

    private void UpdateEditorPreview()
    {
        if (!Application.isPlaying && spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.sprite = frame1;
            UpdateDisplayTransform();

            if (spriteMaterial != null)
            {
                spriteRenderer.sharedMaterial = spriteMaterial;
            }

            spriteRenderer.sortingLayerName = sortingLayerName;
            spriteRenderer.sortingOrder = orderInLayer;
        }
    }

    private void OnDisable()
    {
        if (!Application.isPlaying && displayObject != null)
        {
            DestroyImmediate(displayObject);
        }
    }

    private void OnDestroy()
    {
        if (Application.isPlaying && displayObject != null)
        {
            Destroy(displayObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            if (animationCoroutine == null)
            {
                animationCoroutine = StartCoroutine(PlayImageAnimation());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }
            spriteRenderer.enabled = false;
        }
    }

    private IEnumerator PlayImageAnimation()
    {
        UpdateDisplayTransform();
        spriteRenderer.enabled = true;

        bool showFrame1 = true;

        while (true)
        {
            spriteRenderer.sprite = showFrame1 ? frame1 : frame2;
            showFrame1 = !showFrame1;
            yield return new WaitForSeconds(frameSwitchInterval);
        }
    }
}