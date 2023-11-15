using System.Collections;
using System.Collections.Generic;
using System.Data;
using System;
using UnityEditor.MPE;
using UnityEngine;
using Unity.VisualScripting;

public class ShellScript : MonoBehaviour
{
	public MeshFilter  ShellMesh;
	public Shader      ShellShader;
	public bool        UpdateStatics;
	public AudioSource Audio;
	public float tempMaxValue;

	[Header("Spectrum Control")]
    [Range(0f, 1000f)]
    public float      Multiplier = 1f;
    public bool       IsSpectrumMusic = true;
	public Resolution SpectrumResolution;
	public int        SpectrumLength;
    [Range(0.0001f, 15f)]
    public float      SpectrumMaxValue = 10f;
	[Range(0.0001f, .3f)]
	public float      SpectrumMinValue = 0.001f;
	public Gradient   SpectrumGradient;
	//[Range(0f, 5f)]
	//public float      MaxValueSampleRate = 1f;


	[Header("Shader Control")]
	[Range(1,256)]
	public int   ShellCount;
	[Range(0.0f, 10.0f)]
	public float shellLength = 0.15f;
	[Range(0f,1f)]
	public float Threshold = 0.1f;
	//[Range(0f, 10f)]
	//public float ColorMultiplier = 1f;
	[Range(0f, 6f)]
	public float CoroutineWaitTime = 0.06f;
	

	private Material     shellMaterial; 
	private GameObject[] shells;
	private float[]      spectrumMono;  // Array storing the Mono values of the spectrum
	private float[]      spectrumRight; // Array storing the Right chanel values of the spectrum
	private float[]      spectrumLeft;  // Array storing the Left chanel values of the spectrum
	private Color[]      fullSpectrum1D;// Array of colors corresponding to every pixel of the spectrum texture
	private float        logRange;
	//private float        maxAudioValue;

	private Texture2D texture;


	public enum Resolution
	{
		_64   = 64,
		_128  = 128,
		_256  = 256,
		_512  = 512,
		_1024 = 1024,
		_2048 = 2048,
		_4096 = 4096,
		_8192 = 8192,
	}

	private void OnEnable()
	{
		texture = new Texture2D((int)SpectrumResolution, SpectrumLength, TextureFormat.RGBA32, false);
		fullSpectrum1D = new Color[(int)SpectrumResolution * SpectrumLength];
		spectrumMono   = new float[(int)SpectrumResolution];
		spectrumRight  = new float[(int)SpectrumResolution];
		spectrumLeft   = new float[(int)SpectrumResolution];

		shellMaterial = new Material(ShellShader);
		shells        = new GameObject[ShellCount];


		shellMaterial.mainTexture = texture;

		logRange = Mathf.Log((int)SpectrumResolution);
		float tempRange = (int)SpectrumResolution / logRange;
		logRange = tempRange;

		for(int i = 0; i < ShellCount; ++i)
		{
			shells[i] = new GameObject("Shell " + i.ToString());
			shells[i].AddComponent<MeshFilter>();

			MeshRenderer temp = shells[i].AddComponent<MeshRenderer>();

			shells[i].GetComponent<MeshFilter>().mesh = ShellMesh.mesh;
			temp.material = shellMaterial;
			shells[i].transform.SetParent(this.transform, false);

			temp.material.SetInt("_ShellCount", ShellCount);
			temp.material.SetInt("_ShellIndex", i);
			temp.material.SetFloat("_ShellLength", shellLength);
			temp.material.SetFloat("_Threshold", Threshold);
			//temp.material.SetFloat("_Multiplier", Multiplier);
			//temp.material.SetFloat("_ColorMultiplier", ColorMultiplier);
			temp.material.SetTexture("_MainTexture", texture);
		}
		StartCoroutine(UpdateShader());
		//StartCoroutine(SampleMaxValue());
	}

	/// <summary>
	/// Intended to be used as a way to adapt the gradient depending on the loudness of the music
	/// Not currently used
	/// </summary>
	/// <returns></returns>
	//IEnumerator SampleMaxValue()
	//{
	//	while (true)
	//	{
    //        float tempMaxValue = 0f;
	//
    //        foreach(float value in spectrumMono)
	//		{ 
	//			if(tempMaxValue < value)
    //                tempMaxValue = value;
	//		}
    //        tempMaxValue = tempMaxValue;
    //        yield return new WaitForSeconds(MaxValueSampleRate);
    //    }
	//}

	IEnumerator UpdateShader()
	{
		while(true)
		{
			Audio.GetSpectrumData(spectrumRight, 1, FFTWindow.BlackmanHarris);
			Audio.GetSpectrumData(spectrumLeft, 0, FFTWindow.BlackmanHarris);

			ProcessAudioValues();
			ProcessSpectrogramTexture();

			if(UpdateStatics)
			{
				for(int i = 0; i < ShellCount; ++i)
				{
					shells[i].GetComponent<MeshRenderer>().material.SetInt("_ShellIndex", i);
					shells[i].GetComponent<MeshRenderer>().material.SetFloat("_ShellLength", shellLength);
					shells[i].GetComponent<MeshRenderer>().material.SetFloat("_Threshold", Threshold);
					//shells[i].GetComponent<MeshRenderer>().material.SetFloat("_Multiplier", Multiplier);
					//shells[i].GetComponent<MeshRenderer>().material.SetFloat("_ColorMultiplier", ColorMultiplier);
					shells[i].GetComponent<MeshRenderer>().material.SetTexture("_MainTexture", texture);
				}
			}
			yield return new WaitForSeconds(CoroutineWaitTime);
		}
	}

	private void ProcessSpectrogramTexture()
	{
		for(int index = 0; index < (int)SpectrumResolution; index++)
		{
			fullSpectrum1D[index] = SpectrumGradient.Evaluate(spectrumMono[index]);
			//Debug.Log($"Spectrum Value : {spectrum[index]}, Gradient Value {SpectrumGradient.Evaluate(spectrum[index])}");
		}
		Array.Copy(fullSpectrum1D, 0, fullSpectrum1D, ((int)SpectrumResolution), (int)SpectrumResolution * SpectrumLength - (int)SpectrumResolution);

		texture.SetPixels(fullSpectrum1D);
		texture.Apply();
	}

	// TODO
	// Seperate or re-arange the function to better suite 
	// wether it is music spectrum or not
	private void ProcessAudioValues()
	{
		// clearing the array is necessary 
		Array.Clear(spectrumMono, 0, spectrumMono.Length);
        List<int> ints = new List<int>();
        int tempIndex = -1;
        float monoValue = 0f;
        float clampedValue = 0f;

		// loop starts at 1 to not cause problemes with the log function
        for(int i = 1; i < spectrumMono.Length; i++)
		{
			if(IsSpectrumMusic)
			{
                // Combines the Left and Right channels into a single array
                float sum = (spectrumLeft[i - 1] + spectrumRight[i - 1]);
				//sum = 10 * MathF.Log10(Multiplier / sum);

				// clamps the value to a certain maximum threshold
                sum = sum < SpectrumMaxValue ? sum : SpectrumMaxValue;

                if(sum < SpectrumMinValue)
                    sum = 0f;

                monoValue = (Mathf.Log(sum + 1f)) * Multiplier;

				clampedValue = (monoValue - SpectrumMinValue) / (SpectrumMaxValue - SpectrumMinValue);

				// the index at which to assigne the value has to correlate to the 
				// point at which the log was calculated
				int arrayIndex = (int)(logRange * Mathf.Log(i));

                spectrumMono[arrayIndex] = clampedValue;
                // saves the index at which the values changes
                if(arrayIndex != tempIndex)
                {
                    ints.Add(arrayIndex);
                    tempIndex = arrayIndex;
                }
            }
			else
			{
				// Combines the Left and Right channels into a single array
				monoValue = (spectrumLeft[i - 1] + spectrumRight[i - 1]) * Multiplier;
				monoValue = monoValue < SpectrumMaxValue ? monoValue : SpectrumMaxValue;

                //f(monoValue > 0f && monoValue < SpectrumMinValue)
                //	monoValue = SpectrumMinValue;

                if(monoValue < SpectrumMinValue)
                    monoValue = 0f;

                clampedValue = (monoValue - SpectrumMinValue) / (SpectrumMaxValue - SpectrumMinValue);
				spectrumMono[i - 1] = clampedValue;
			}
		}
        // fill in the blanks from the Logarithmic display
        if(IsSpectrumMusic)
        {
        	// "ints.Count - 1" to not have problemes with the last value
            for(int i = 0; i < ints.Count - 1; i++)
        	{
        		int spectrumIndex = ints[i];
        		int iterations = ints[i + 1] - ints[i];
        		float valA = spectrumMono[ints[i]];
        		float valB = spectrumMono[ints[i + 1]];
				//Debug.Log(iterations);
        		for(int j = 0; j < iterations - 1; j++)
        		{
        			float lerpStep = (float)j / (float)iterations;
        			spectrumMono[spectrumIndex + j + 1] = EasingFunction.Linear(valA, valB, lerpStep);
                    //Debug.Log(lerpStep);
                    //Debug.Log($"ValA : {valA}, ValB : {valB}, Lerp : {Mathf.Lerp(valA, valB, lerpStep)}");Mathf.Lerp(valA, valB, lerpStep)
                    // EasingFunction.EaseInExpo(valA, valB, lerpStep)
                }
            }
        	
        }
        ints.Clear();
    }

	void OnDisable()
	{
		for(int i = 0; i < shells.Length; ++i)
		{
			Destroy(shells[i]);
		}

		shells = null;
	}
}