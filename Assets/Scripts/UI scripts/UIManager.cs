using SFB;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public TextMeshProUGUI MusicFileButtonText;
	public float TimeBeforMusicStart = 3f;
	public GameObject UICanvas;

	public string[] FilePath;

	/// <summary>
	/// List of filter used by the file browser to only show files with the desired exstension
	/// </summary>
	private readonly ExtensionFilter[] Filters = new ExtensionFilter[] { new ExtensionFilter("Sound Files", "mp3" ) };
	

	/// <summary>
	/// 
	/// </summary>
	public void StartMusicCoroutine()
	{
		StartCoroutine(StartMusicAfterTImer());
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	private IEnumerator StartMusicAfterTImer()
	{
		HideUI();

		yield return new WaitForSeconds(TimeBeforMusicStart);

		AudioManager.Instance.MainAudioSource.Play();

	}

	/// <summary>
	/// 
	/// </summary>
	public void HideUI()
	{
		UICanvas.SetActive(false);
	}

	/// <summary>
	/// 
	/// </summary>
	public void SelectAudioFile()
	{
		FilePath = StandaloneFileBrowser.OpenFilePanel("Select audio file", "", Filters, false);
		
		// the file browser returns an array of path but since we only select one file,
		// only the very first element will ever be of use
		StartCoroutine(GetAudioClipFromPath(FilePath[0]));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	public IEnumerator GetAudioClipFromPath(string path)
	{
		// TODO 15/01/24:
		// - find a way to accept other types of audio file formats (currently only mp3/mp2)
		UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG);

		yield return www.SendWebRequest();

		if(www.result == UnityWebRequest.Result.ConnectionError)
		{
			Debug.Log(www.error);
		}
		else
		{
			AudioManager.Instance.SetAudioFile(DownloadHandlerAudioClip.GetContent(www));
			UpdateButtonText(Path.GetFileName(path));

        }
			
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="text"></param>
	private void UpdateButtonText(string text)
	{
		// TODO 15/01/24:
		// - dirty way to change the text of the button, will need to improve it
		MusicFileButtonText.text = text;
    }
}
