using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MicEncoder : MonoBehaviour {

    AudioSource AudioMic;
    // Use this for initialization
    void Start() {
        StartCoroutine(CaptureMic());
        StartCoroutine(SenderCOR());
    }

    // Update is called once per frame
    void Update() {

    }

    bool stop = false;

    private Queue<byte> AudioBytes = new Queue<byte>();
    public int OutputSampleRate = 11025;
    public int OutputChannels = 1;
    private object _asyncLockAudio = new object();

    int CurrentAudioTimeSample = 0;
    int LastAudioTimeSample = 0;

    IEnumerator CaptureMic()
    {
        if (AudioMic == null) AudioMic = GetComponent<AudioSource>();
        AudioMic.clip = Microphone.Start(null, true, 1, OutputSampleRate);
        AudioMic.loop = true;
        while (!(Microphone.GetPosition(null) > 0)) { }
        Debug.Log("Start Mic(pos): " + Microphone.GetPosition(null));
        AudioMic.Play();

        AudioMic.volume = 0f;

        OutputChannels = AudioMic.clip.channels;

        while (!stop)
        {
            AddMicData();
            yield return null;
        }
        yield return null;
    }


    void AddMicData()
    {
        LastAudioTimeSample = CurrentAudioTimeSample;
        //CurrentAudioTimeSample = AudioMic.timeSamples;
        CurrentAudioTimeSample = Microphone.GetPosition(null);

        if (CurrentAudioTimeSample != LastAudioTimeSample)
        {
            float[] samples = new float[AudioMic.clip.samples];
            AudioMic.clip.GetData(samples, 0);

            if (CurrentAudioTimeSample > LastAudioTimeSample)
            {
                lock (_asyncLockAudio)
                {
                    for (int i = LastAudioTimeSample; i < CurrentAudioTimeSample; i++)
                    {
                        byte[] byteData = BitConverter.GetBytes(samples[i]);
                        foreach (byte _byte in byteData) AudioBytes.Enqueue(_byte);
                    }
                }
            }
            else if (CurrentAudioTimeSample < LastAudioTimeSample)
            {
                lock (_asyncLockAudio)
                {
                    for (int i = LastAudioTimeSample; i < samples.Length; i++)
                    {
                        byte[] byteData = BitConverter.GetBytes(samples[i]);
                        foreach (byte _byte in byteData) AudioBytes.Enqueue(_byte);
                    }
                    for (int i = 0; i < CurrentAudioTimeSample; i++)
                    {
                        byte[] byteData = BitConverter.GetBytes(samples[i]);
                        foreach (byte _byte in byteData) AudioBytes.Enqueue(_byte);
                    }
                }
            }
        }
    }

    //[Header("Send to client")]
    public float interval = 0.05f;
    float next = 0f;
    public UnityEventByteArray OnDataByteReadyEvent;
    //byte[] ImgByte;

    [Header("Pair Encoder & Decoder")]
    public int label = 2001;
    int dataID = 0;
    int maxID = 1024;
    //int chunkSize = 4096;
    int chunkSize = 8096;
    //int chunkSize = 32768;
    int dataLength;

    IEnumerator SenderCOR()
    {
        //Debug.Log("Read TO SEND SEND SEND");
        while (!stop)
        {
            if (Time.realtimeSinceStartup > next)
            {
                next = Time.realtimeSinceStartup + interval;
                EncodeBytes();
            }
            yield return null;
        }
    }

    void EncodeBytes()
    {
        //==================getting byte data==================
        //convert audio date to byte[]
        byte[] dataByte;

        byte[] _samplerateByte = BitConverter.GetBytes(OutputSampleRate);
        byte[] _channelsByte = BitConverter.GetBytes(OutputChannels);
        lock (_asyncLockAudio)
        {
            dataByte = new byte[AudioBytes.Count + _samplerateByte.Length + _channelsByte.Length];

            Buffer.BlockCopy(_samplerateByte, 0, dataByte, 0, _samplerateByte.Length);
            Buffer.BlockCopy(_channelsByte, 0, dataByte, 4, _channelsByte.Length);
            Buffer.BlockCopy(AudioBytes.ToArray(), 0, dataByte, 8, AudioBytes.Count);
            AudioBytes.Clear();
        }
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

    private void OnEnable()
    {
        if (Time.realtimeSinceStartup <= 3f) return;
        StartAll();
    }
    private void OnDisable()
    {
        StopAll();
    }

    void StartAll()
    {
        if (stop)
        {
            stop = false;
            StartCoroutine(SenderCOR());
            StartCoroutine(CaptureMic());
        }
    }
    void StopAll()
    {
        stop = true;
        StopCoroutine(SenderCOR());
        StopCoroutine(CaptureMic());

        AudioMic.Stop();
        Microphone.End(null);
    }
}
