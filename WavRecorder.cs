using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WavRecorder : MonoBehaviour
{
    public Button startButton;
    public Button stopButton;
    public TextMeshProUGUI text;

    private AudioClip clip;
    private byte[] bytes;
    private bool recording;

    private RunWhisper runWhisper;

    private void Start()
    {
        runWhisper = this.GetComponent<RunWhisper>();

        //给startbutton添加事件
        startButton.onClick.AddListener(StartRecording);
        stopButton.onClick.AddListener(StopRecording);

        stopButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (recording && Microphone.GetPosition(null) >= clip.samples)
        {
            StopRecording();
        }
    }

    public void StartRecording()
    {
        stopButton.gameObject.SetActive(true);
        startButton.gameObject.SetActive(false);

        text.color = Color.white;
        text.text = "录音中......";


        //录制16k的wav
        clip = Microphone.Start(null, false, 30, 16000);
        recording = true;
    }

    public void StopRecording()
    {
        stopButton.gameObject.SetActive(false);
        startButton.gameObject.SetActive(true);

        var position = Microphone.GetPosition(null);
        Microphone.End(null);
        var samples = new float[position * clip.channels];
        clip.GetData(samples, 0);
        bytes = EncodeAsWAV(samples, clip.frequency, clip.channels);
        recording = false;

        //保存wav
        Debug.Log(Application.persistentDataPath);
        //文件路径
        string filename = Application.persistentDataPath + "/output.wav";
        using (FileStream fs = new FileStream(filename,FileMode.Create,FileAccess.Write))
        using (BinaryWriter writer = new BinaryWriter(fs))
        {
            writer.Write(bytes);
        }

        runWhisper.audioClip = clip;
        runWhisper.StartASR();

    }


    private byte[] EncodeAsWAV(float[] samples, int frequency, int channels)
    {
        using (var memoryStream = new MemoryStream(44 + samples.Length * 2))
        {
            using (var writer = new BinaryWriter(memoryStream))
            {
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + samples.Length * 2);
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)channels);
                writer.Write(frequency);
                writer.Write(frequency * channels * 2);
                writer.Write((ushort)(channels * 2));
                writer.Write((ushort)16);
                writer.Write("data".ToCharArray());
                writer.Write(samples.Length * 2);

                foreach (var sample in samples)
                {
                    writer.Write((short)(sample * short.MaxValue));
                }
            }
            return memoryStream.ToArray();
        }
    }
}
