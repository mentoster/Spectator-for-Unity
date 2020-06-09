using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSManager : MonoBehaviour
{

    public Text fpsText;
    float deltaTime;

    public int targetFps = 30;

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (Application.targetFrameRate != targetFps)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFps;
        }
#endif

        if (fpsText == null)
        {
            return;
        }

        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        if (Time.frameCount % 5 == 0)
        {
            float fps = 1.0f / deltaTime;
            fpsText.text = "FPS: " + Mathf.Ceil(fps).ToString();
        }
    }

    public void Action_SetFPS(int _fps)
    {
        targetFps = _fps;
    }
}