using System.Collections;
using UnityEngine;

using System;
using System.Text;
using System.Collections.Generic;

public class GameViewEncoder : MonoBehaviour
{
    void Start()
    {
        Application.runInBackground = true;
        StartCoroutine(SenderCOR());
    }

    public Camera RenderCam;
    public Texture2D sendTexture;
    RenderTexture rt;
    public Vector2 resolution = new Vector2(512, 512);

    [Range(10, 100)]
    public int quality = 40;
    public bool matchScreenAspect = true;

    Texture2D currentTexture;
    public void RenderTextureRefresh()
    {
        if (rt == null)
        {
            if (matchScreenAspect) resolution.y = Mathf.RoundToInt((float)(resolution.x) / (float)(Screen.width) * (float)(Screen.height));
            rt = new RenderTexture(Mathf.RoundToInt(resolution.x), Mathf.RoundToInt(resolution.y), 16, RenderTextureFormat.ARGB32);
            currentTexture = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        }
        else
        {

            if (rt.width != Mathf.RoundToInt(resolution.x) || rt.height != Mathf.RoundToInt(resolution.y))
            {
                Destroy(rt);
                Destroy(currentTexture);

                if (matchScreenAspect) resolution.y = Mathf.RoundToInt(resolution.x / (float)(Screen.width) * (float)(Screen.height));
                rt = new RenderTexture(Mathf.RoundToInt(resolution.x), Mathf.RoundToInt(resolution.y), 16, RenderTextureFormat.ARGB32);
                currentTexture = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
            }
        }

        RenderCam.targetTexture = rt;
        RenderCam.Render();

        // Backup the currently set RenderTexture
        RenderTexture previous = RenderTexture.active;

        // Set the current RenderTexture to the temporary one we created
        RenderTexture.active = rt;

        if (sendTexture == null)
        {
            sendTexture = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        }
        else
        {
            if (sendTexture.width != rt.width || sendTexture.height != rt.height)
            {
                Destroy(sendTexture);
                sendTexture = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            }
        }

        sendTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        sendTexture.Apply();

        // Reset the active RenderTexture
        RenderTexture.active = previous;
    }

    //[Header("Send to client")]
    public float interval = 0.05f;
    float next = 0f;
    bool stop = false;
    public UnityEventByteArray OnDataByteReadyEvent;
	byte[] dataByte;

    [Header("Pair Encoder & Decoder")]
    public int label = 1001;
    int dataID = 0;
    int maxID = 1024;
    //int chunkSize = 4096;
    int chunkSize = 8096;
	//int chunkSize = 32768;

	public int dataLength;
	IEnumerator SenderCOR()
    {
        while (!stop)
        {
            if (Time.realtimeSinceStartup > next)
            {
                next = Time.realtimeSinceStartup + interval;

                //==================getting byte data==================
                RenderTextureRefresh();
				dataByte = sendTexture.EncodeToJPG(quality);
                //==================getting byte data==================

                dataLength = dataByte.Length;
				int _length = dataByte.Length;
				int _offset = 0;

                byte[] _meta_label = BitConverter.GetBytes(label);
                byte[] _meta_id = BitConverter.GetBytes(dataID);
				byte[] _meta_length = BitConverter.GetBytes(_length);

				byte[] _meta_offset = new byte[1];
				byte[] SendByte = new byte[1];
				if (_length > chunkSize)
				{
					//key, id, length, offset, data
					int chunks = dataByte.Length / chunkSize;
					for (int i = 0; i<chunks; i++)
					{
						_meta_offset = BitConverter.GetBytes(_offset);
						SendByte = new byte[chunkSize + 16];
                        Buffer.BlockCopy(_meta_label, 0, SendByte, 0, 4);
                        Buffer.BlockCopy(_meta_id, 0, SendByte, 4, 4);
                        Buffer.BlockCopy(_meta_length, 0, SendByte, 8, 4);
						Buffer.BlockCopy(_meta_offset, 0, SendByte, 12, 4);
						Buffer.BlockCopy(dataByte, _offset, SendByte, 16, SendByte.Length - 16);
                        OnDataByteReadyEvent.Invoke(SendByte);
						_offset += chunkSize;
                    }
                }

				_meta_offset = BitConverter.GetBytes(_offset);
				SendByte = new byte[_length % chunkSize + 16];
                Buffer.BlockCopy(_meta_label, 0, SendByte, 0, 4);
                Buffer.BlockCopy(_meta_id, 0, SendByte, 4, 4);
				Buffer.BlockCopy(_meta_length, 0, SendByte, 8, 4);
				Buffer.BlockCopy(_meta_offset, 0, SendByte, 12, 4);

				Buffer.BlockCopy(dataByte, _offset, SendByte, 16, SendByte.Length - 16);
                OnDataByteReadyEvent.Invoke(SendByte);

                dataID++;
                if (dataID > maxID) dataID = 0;
            }
            yield return null;
        }
    }

    void OnEnable()
    {
        StartAll();
    }
    void OnDisable()
    {
        StopAll();
    }
    void OnApplicationQuit()
    {
        StopAll();
    }
    void OnDestroy()
    {
        StopAll();
    }

    void StopAll()
    {
        stop = true;
        StopAllCoroutines();
    }
    void StartAll()
    {
        if (Time.realtimeSinceStartup < 3f) return;
        stop = false;
        StartCoroutine(SenderCOR());
    }
}
