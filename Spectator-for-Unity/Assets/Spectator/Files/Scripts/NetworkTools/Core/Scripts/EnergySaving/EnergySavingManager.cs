using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
public class EnergySavingManager : MonoBehaviour {

    public static EnergySavingManager instance;

    public enum DetectMode { GYRO, ANY}
    public DetectMode DMode;

    public bool isSleeping = false;
    bool isSleeping_old = false;

    public GameObject SleepModePanel;

    [Header ("USE spacebar to wake up (default)")]
    public bool UseSpacebarDebug = true;

    bool getGyroChanged()
    {
        sumAttitude = attitudeCollection[0];
        for (int i = 0; i < attitudeCollection.Length - 1; i++)
        {
            attitudeCollection[i + 1] = attitudeCollection[i];
            sumAttitude += attitudeCollection[i + 1];
        }

        attitudeCollection[0] = Input.gyro.attitude.eulerAngles;
        tmp_averageAttitude = averageAttitude;
        averageAttitude = sumAttitude / attitudeCollection.Length;
        magnitude = (averageAttitude - tmp_averageAttitude).magnitude;

        if (magnitude < shakeThreshold)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    bool getAnyKeyChanged()
    {
        if (Input.anyKey)
        {
            return true;
        }
        return false;
    }

    //[Header("(Timer) Enter Sleep Mode")]
    float sleepTimer = 0f;
    [Header("(Threshold) Enter Sleep Mode")]
    public float sleepThreshold = 5f;
    //[Header("(Timer) Exit Sleep Mode")]
    float awakeTimer = 0f;
    [Header("(Threshold) Delay WakeUp")]
    public float awakeThreshold = 1f;


    Vector3[] attitudeCollection;
    Vector3 sumAttitude;
    Vector3 tmp_averageAttitude;
    Vector3 averageAttitude;

    float magnitude;
    float shakeThreshold = 0.01f;//0.02f;

    [Header("Force it awake")]
    public bool ForceAwake = true;
    [Header("Ignore Force Awake on Build")]
    public bool ignoreForceAwakeOnBuild = true;

    //public bool isLandscape = false;
    [Header("Force it as SleepMode(override)")]
    public bool forceSleepModeOn = false;

    [Header("Disable Objects when enter sleep mode")]
    public GameObject[] disableGrp;

    [Header("On Enter Sleep Mode")]
    public UnityEvent OnSleepModeEnter;

    [Header("On Exit Sleep Mode")]
    public UnityEvent OnSleepModeExit;

    [Header("Debug Clock(x hr, y min, z sec)")]
    public Vector3 DebugClock;

    [Header("reload scene at (x hour, y min)")]
    public bool AutoReloadScene = false;
    public Vector2[] Schedule_Reload;
    [Header("quit at (x hour, y min)")]
    public bool AutoQuitApp = false;
    public Vector2[] Schedule_Quit;

    bool needReloadScene = false;
    bool needQuitApp = false;

    [Header("Force Reload App: touch screen with 5 fingers *Debug shortcut: Keyboard R + Mouse Left*")]
    public bool ForceReloadAppGesture = false;
    [Header("(Threshold) sec")]
    public float ForceReloadAppThreshold = 30f;
    float ForceReloadAppGestureTimer = 0f;

    [Header("Force Quit App: touch screen with 5 fingers *Debug shortcut: Keyboard R + Mouse Left*")]
    public bool ForceQuitAppGesture = false;
    [Header("(Threshold) sec")]
    public float ForceQuitAppThreshold = 30f;
    float ForceQuitAppGestureTimer = 0f;

    void Awake() {
        if (instance == null) {
            instance = this;
        }
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
    void Start()
    {
        Input.gyro.enabled = true;

        attitudeCollection = new Vector3[30];
        for (int i = 0; i < attitudeCollection.Length; i++) {
            attitudeCollection[i] = Vector3.zero;
        }
        averageAttitude = sumAttitude / attitudeCollection.Length;

    }

    void Update() {
        isSleeping_old = isSleeping;

        if (ForceAwake)
        {
            if (ignoreForceAwakeOnBuild)
            {
#if UNITY_EDITOR
                sleepTimer = 0f;
#endif             
            }
            else
            {
                sleepTimer = 0f;
            }
        }

        bool hasInput = true;
        if(DMode == DetectMode.GYRO)
        {
            hasInput = getGyroChanged();
        }
        if(DMode == DetectMode.ANY)
        {
            hasInput = getGyroChanged() || getAnyKeyChanged();
        }


        if (!hasInput)
        {
            sleepTimer += Time.deltaTime;
        }
        else
        {
            sleepTimer -= Time.deltaTime;
        }


        sleepTimer = Mathf.Clamp(sleepTimer, 0f, sleepThreshold);

        if (sleepTimer >= sleepThreshold || forceSleepModeOn) {
            isSleeping = true;
        }
        else if (sleepTimer <= sleepThreshold - 1f) {
            isSleeping = false;
        }

        //actions when it's sleeping
        if (isSleeping) {
            //sleeping mode, showing the cover
            awakeTimer = 0f;
            SleepModePanel.SetActive(true);
        } else {
            awakeTimer += Time.deltaTime;
            awakeTimer = Mathf.Clamp(awakeTimer, 0f, awakeThreshold);
            if (awakeTimer >= awakeThreshold) {
                SleepModePanel.SetActive(false);
            } else {
                SleepModePanel.SetActive(true);
            }
        }

        if (isSleeping == true && isSleeping_old == false)
        {
            //enter sleep mode
            OnSleepModeEnter.Invoke();
            foreach (GameObject obj in disableGrp)
            {
                obj.SetActive(false);
            }
        }
        else if (isSleeping == false && isSleeping_old == true)
        {
            //exit sleep mode
            OnSleepModeExit.Invoke(); 
            foreach (GameObject obj in disableGrp)
            {
                obj.SetActive(true);
            }
        }

        if (AutoReloadScene)
        {
            foreach(Vector2 schedule in Schedule_Reload)
            {
                Action_ReloadScene(Mathf.RoundToInt(schedule.x), Mathf.RoundToInt(schedule.y));
            }
        }
        if (AutoQuitApp)
        {
            foreach (Vector2 schedule in Schedule_Quit)
            {
                Action_QuitApp(Mathf.RoundToInt(schedule.x), Mathf.RoundToInt(schedule.y));
            }
        }

        if (ForceReloadAppGesture)
        {
            if (Input.touchCount >= 5 || (Input.GetMouseButton(0) && Input.GetKey(KeyCode.R)))
            {
                ForceReloadAppGestureTimer += Time.deltaTime;
                if (ForceReloadAppGestureTimer > ForceReloadAppThreshold)
                {
                    Debug.Log("reload scene now!");
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
            else
            {
                ForceReloadAppGestureTimer =0f;
            }
        }
        if (ForceQuitAppGesture)
        {
            if (Input.touchCount >= 5 || (Input.GetMouseButton(0) && Input.GetKey(KeyCode.R)))
            {
                ForceQuitAppGestureTimer += Time.deltaTime;
                if (ForceQuitAppGestureTimer > ForceQuitAppThreshold)
                {
                    Debug.Log("quit app now!");
                    Application.Quit();
                }
            }
            else
            {
                ForceQuitAppGestureTimer = 0f;
            }
        }


        DebugClock = new Vector3(System.DateTime.Now.Hour, System.DateTime.Now.Minute, System.DateTime.Now.Second);
        if (Input.GetKeyDown(KeyCode.Space) && UseSpacebarDebug)
        {
            sleepTimer = 0f;
        }
    }

    public void Action_ReloadScene(int _hr, int _min)
    {
        if (System.DateTime.Now.Hour == _hr && System.DateTime.Now.Minute == _min)
        {
            if(needReloadScene == false)
            {
                Debug.Log("start count down for reload scene!");
                needReloadScene = true;
                StartCoroutine(DelayReloadScene(_hr,_min));
            }
        }
    }
    IEnumerator DelayReloadScene(int _hr, int _min)
    {
        while (needReloadScene)
        {
            //if (System.DateTime.Now.Hour > _hr || System.DateTime.Now.Minute > _min)
            if (System.DateTime.Now.Hour != _hr || System.DateTime.Now.Minute != _min)
            {
                needReloadScene = false;
            }
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("reload scene now!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        yield return null;
    }


    public void Action_QuitApp(int _hr, int _min)
    {
        if (System.DateTime.Now.Hour == _hr && System.DateTime.Now.Minute == _min)
        {
            if (!needQuitApp)
            {
                Debug.Log("start count down for quit app!");
                needQuitApp = true;
                StartCoroutine(DelayQuitApp(_hr, _min));
            }
        }
    }
    IEnumerator DelayQuitApp(int _hr, int _min)
    {
        while (needQuitApp)
        {
            //if (System.DateTime.Now.Hour > _hr || System.DateTime.Now.Minute > _min)
            if (System.DateTime.Now.Hour != _hr || System.DateTime.Now.Minute != _min)
            {
                needQuitApp = false;
            }
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("quit app now!");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
        yield return null;
    }

}
