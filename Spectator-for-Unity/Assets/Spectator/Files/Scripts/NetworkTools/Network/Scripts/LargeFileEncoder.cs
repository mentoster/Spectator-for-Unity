using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LargeFileEncoder : MonoBehaviour {

    public bool Sending = false;
    //public float TestingMB=1;
    void Start()
    {
        Application.runInBackground = true;
    }

    private void Update()
    {
        //if(Input.GetKeyDown(KeyCode.S)) Action_SendLargeByte(new byte[(int)(1024f*1024f*TestingMB)]);
    }

    //[Header("Send to client")]
    public UnityEventByteArray OnDataByteReadyEvent;

    [Header("Pair Encoder & Decoder")]
    public int label = 8001;
    int dataID = 0;
    int maxID = 1024;
    int chunkSize = 8096;

    int dataLength;
    public void Action_SendLargeByte(byte[] _data)
    {
        StartCoroutine(SenderCOR(_data));
    }
    IEnumerator SenderCOR(byte[] dataByte)
    {
        yield return null;
        if (!Sending)
        {
            Sending = true;

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
                //Debug.Log(chunks);
                for (int i = 0; i < chunks; i++)
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
                    if (i % 10 == 0) yield return new WaitForEndOfFrame();
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

            Sending = false;
        }
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
        StopAllCoroutines();
    }
}
