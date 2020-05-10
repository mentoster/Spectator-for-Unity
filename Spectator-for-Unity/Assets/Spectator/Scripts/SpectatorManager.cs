using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SpectatorManager : MonoBehaviour
{
     public GameObject Skin;
     public GameObject cloud;
     public TMP_Text textCloud; 
     private TMP_Text textWhatSkinNow;
     private TMP_InputField chatBox;
     public GameObject UiPrefab;
     
     public string[] SkinsName=new string[3];

     [Header("Settings")]
     public Camera cameraToLookAt;
     public float timeForCloud = 15;
     public KeyCode changeSkin = KeyCode.E;
     public KeyCode sayButton = KeyCode.T;
     public bool rotation = false;


     private int _whatSkinNow = 0;
      private bool _seeChat=false;
     //it is necessary for the text to disappear on the last message
      private int _textInCloud;
      private Animator _skinAnimator;
    void Start()
    {   //Activating multi-display support
        Debug.Log ("displays connected: " + Display.displays.Length);
        for (int i = 1; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }
        //cameraToLookAt=Camera.main;
         UiPrefab= Instantiate(UiPrefab);
         chatBox= GameObject.Find("ChatBox").GetComponent<TMP_InputField>();
         chatBox.gameObject.SetActive(false);
         textWhatSkinNow= GameObject.Find("SkinNowText").GetComponent<TMP_Text>();
         _skinAnimator = Skin.GetComponent<Animator>();
       
     }

    void Update()
    {   
        if (Input.GetKeyDown(changeSkin)&&!_seeChat)
        { 
            //change skin
            _skinAnimator.SetTrigger("SkinChange");
            //say to canvas about our skin
            textWhatSkinNow.text= SkinsName[_whatSkinNow++];
            if (_whatSkinNow == SkinsName.Length)
                _whatSkinNow = 0;
        }
        if (Input.GetKeyDown(sayButton)&&!chatBox.isFocused)
        {
            _seeChat = !_seeChat;
            chatBox.gameObject.SetActive(_seeChat);
        }
        if (_seeChat==true && Input.GetKeyDown(KeyCode.Return))
        {
            textCloud.text = chatBox.text;
            chatBox.text = "";
            chatBox.gameObject.SetActive(!_seeChat);
            cloud.SetActive(true);
            _seeChat = !_seeChat;
            Invoke("offCloud", timeForCloud);
            _textInCloud++;
        }
    }
    void LateUpdate () {
        if (!rotation)
            //block rotation
            Skin.transform.forward = cameraToLookAt.transform.forward;
    }

  void offCloud()
    {
        if (_textInCloud==1)
            cloud.SetActive(false);
        _textInCloud--;
    }
}
 
