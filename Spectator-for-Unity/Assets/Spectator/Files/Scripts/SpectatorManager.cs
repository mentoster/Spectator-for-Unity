using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

public class SpectatorManager : MonoBehaviour
{
    #region RecieveData

    private bool allowRotation = true;
    private bool allowMovement = true;
    public bool skinRotation = false;
    private Vector3 _recievePosition;
    private Vector3 _recieveRotation;

    private byte _recieveSkin;
    private byte[] _buffBytes = new byte[4];

    #endregion

    public Camera cameraToLookAt;
    public GameObject Skin;
    public GameObject cloud;
    public TMP_Text textCloud;
    private Animator _skinAnimator;
    private char _skinNow;


    private byte _textInCloud;

    private void Start()
    {
        //cameraToLookAt=Camera.main;
        _skinAnimator = Skin.GetComponent<Animator>();
    }

    public void OnReceivedTransformBytes(byte[] receivedBytes)
    {
        //movement
        if (receivedBytes[0] == 0)
        {
            #region byte to float

            //from byte to float
            //Create _recievePosition
            //x
            for (int i = 1; i < 5; i++)
                _buffBytes[i - 1] = receivedBytes[i];
            _recievePosition.x = BitConverter.ToSingle(_buffBytes, 0);
            //y
            for (int i = 1; i < 5; i++)
                _buffBytes[i - 1] = receivedBytes[i + 4];
            _recievePosition.y = BitConverter.ToSingle(_buffBytes, 0);
            //z
            for (int i = 1; i < 5; i++)
                _buffBytes[i - 1] = receivedBytes[i + 8];
            _recievePosition.z = BitConverter.ToSingle(_buffBytes, 0);

            //Create _recieveRotation
            //x
            for (int i = 1; i < 5; i++)
                _buffBytes[i - 1] = receivedBytes[i + 12];
            _recieveRotation.x = BitConverter.ToSingle(_buffBytes, 0);
            //y
            for (int i = 1; i < 5; i++)
                _buffBytes[i - 1] = receivedBytes[i + 16];
            _recieveRotation.y = BitConverter.ToSingle(_buffBytes, 0);
            //z
            for (int i = 1; i < 5; i++)
                _buffBytes[i - 1] = receivedBytes[i + 20];
            _recieveRotation.z = BitConverter.ToSingle(_buffBytes, 0);

            #endregion
            
            if (allowMovement)
                transform.position += _recievePosition;
            if (allowRotation)
            {
                var thisTransform = transform;
                Vector3 eulerAngles = thisTransform.eulerAngles;
                eulerAngles.x = _recieveRotation.x;
                eulerAngles.y = _recieveRotation.y;
                eulerAngles.z = _recieveRotation.z;
                thisTransform.eulerAngles = eulerAngles;
            }
        }
        //other info
        else
        {
            // 0/   1     /          15       /    hello!
            // type of recieved  info/skin/time/message
            // var base64 = Convert.ToBase64String(byteData);
            if (receivedBytes[1] == 1)
                _skinAnimator.SetTrigger("SkinChange");
            //if we have  message
            if (receivedBytes.Length > 2)
            {
                Debug.Log(receivedBytes[0] + " " + receivedBytes[1] + " " + receivedBytes[2]);
                byte[] textByte = new byte[receivedBytes.Length - 3];
                for (int i = 3; i < receivedBytes.Length; i++)
                    textByte[i - 3] = receivedBytes[i - 1];
                var say = System.Text.Encoding.UTF8.GetString(textByte);
                Debug.Log("Client say: " + say);
                if (say != "")
                {
                    textCloud.text = say;
                    cloud.SetActive(true);
                    Invoke("offCloud", receivedBytes[2]);
                    _textInCloud++;
                }
            }

            Debug.Log(receivedBytes.Length);
        }
    }


    void LateUpdate()
    {
        if (!skinRotation)
            //block skinRotation
            Skin.transform.forward = cameraToLookAt.transform.forward;
    }

    void offCloud()
    {
        if (_textInCloud == 1)
            cloud.SetActive(false);
        _textInCloud--;
    }
}