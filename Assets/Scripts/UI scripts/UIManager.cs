using SFB;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    

    public float TimeBeforMusicStart = 3f;
    public GameObject UICanvas;

    public string[] FilePath;


    private ExtensionFilter[] Filters = new ExtensionFilter[] { new ExtensionFilter("Sound Files", "mp3" ) };

    public void StartMusicCoroutine()
    {
        StartCoroutine(StartMusicAfterTImer());
    }

    private IEnumerator StartMusicAfterTImer()
    {
        HideUI();
        

        yield return new WaitForSeconds(TimeBeforMusicStart);

        AudioManager.Instance.MainAudioSource.Play();

    }

    public void HideUI()
    {
        UICanvas.SetActive(false);
    }

    public void SelectAudioFile()
    {
        FilePath = StandaloneFileBrowser.OpenFilePanel("Select audio file", "", Filters, false);

        StartCoroutine(GetAudioClipFromPath(FilePath[0]));
    }

    public IEnumerator GetAudioClipFromPath(string path)
    {
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG);

        yield return www.SendWebRequest();

        Debug.Log(www.result);

        AudioClip audio = DownloadHandlerAudioClip.GetContent(www);

        AudioManager.Instance.SetAudioFile(audio);
        AudioManager.Instance.MainAudioSource.Play();

    }
}
