using UnityEngine;

public class RealWorldCapture : MonoBehaviour
{
    public Camera realWorldCamera; // 额外的摄像机
    public RenderTexture realWorldTexture; // 用于渲染现实世界的 RenderTexture
    public string targetTag = "RAIL"; // 目标 Tag

    private void Start()
    {
        // 确保摄像机的目标纹理是我们指定的 RenderTexture
        realWorldCamera.targetTexture = realWorldTexture;
    }

    public void CaptureRealWorld()
    {
        // 创建一个临时相机，用于渲染带有指定 Tag 的对象
        var tempCamera = new GameObject("TempCamera").AddComponent<Camera>();
        tempCamera.CopyFrom(realWorldCamera);
        tempCamera.clearFlags = CameraClearFlags.SolidColor;
        tempCamera.backgroundColor = Color.clear;
        tempCamera.targetTexture = realWorldTexture;

        // 设置 CullingMask，排除所有 Layer
        tempCamera.cullingMask = 0;

        // 找到所有带有特定 Tag 的对象，并添加它们的 Renderer 到渲染队列
        var taggedObjects = GameObject.FindGameObjectsWithTag(targetTag);
        foreach (var obj in taggedObjects)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                // 使渲染器仅在当前摄像机中渲染
                renderer.gameObject.layer = LayerMask.NameToLayer("TempRenderLayer");
            }
        }

        // 将临时 Layer 设为相机的渲染层
        tempCamera.cullingMask = 1 << LayerMask.NameToLayer("TempRenderLayer");

        // 渲染一次，将内容写入 RenderTexture
        tempCamera.Render();

        // 删除临时摄像机
        Destroy(tempCamera.gameObject);

        // 重置所有对象的 Layer
        foreach (var obj in taggedObjects)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.gameObject.layer = 0; // 将 Layer 设置回默认
            }
        }
    }
}
