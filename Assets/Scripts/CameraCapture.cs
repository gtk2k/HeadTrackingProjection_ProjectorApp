using System.Threading;
using UnityEngine;

public class CameraCapture
{
    public RenderTexture Texture;
    internal Rect normalRect;
    private RenderTexture rt;

    public CameraCapture(int width, int height)
    {
        normalRect = new Rect(0, 0, 1f, 1f);
        Texture = new RenderTexture(width, height, 24); //, RenderTextureFormat.BGRA32, 0);
    }

    public virtual void Update() { }

    internal void CameraRender(Camera cam, ref Rect rect)
    {
        if (cam == null) return;

        rt = cam.targetTexture;
        cam.targetTexture = Texture;
        cam.rect = rect;
        cam.Render();
        cam.rect = normalRect;
        cam.targetTexture = rt;
    }
}
