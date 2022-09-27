using UnityEngine;

public class StereoscopicCapture : CameraCapture
{
    private Camera lCamera;
    private Camera rCamera;
    private Rect lRect;
    private Rect rRect;

    public StereoscopicCapture(Camera leftCamera, Camera rightCamera, int width, int height) : base(width, height)
    {
        lCamera = leftCamera;
        rCamera = rightCamera;
        lRect = new Rect(0, 0, 0.5f, 1f);
        rRect = new Rect(0.5f, 0, 0.5f, 1f);
    }

    public override void Update()
    {
        CameraRender(lCamera, ref lRect);
        CameraRender(rCamera, ref rRect);
    }
}

