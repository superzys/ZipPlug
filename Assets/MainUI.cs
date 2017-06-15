using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;

//using UnityEditor;
using SevenZip.Compression.LZMA;
using System.Collections.Generic;
using ZTools;

public class MainUI : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{


	}

	string tipStr = "";

	void OnGUI ()
	{
		GUI.Label (new Rect (Screen.width / 2 + 20, 0, 200, 300), tipStr);
	}
	// Update is called once per frame
	void Update ()
	{
		
	}

	public void BtnClick_BtnLoadRes ()
	{
		LoaCopyToCatchdData ();

	}

	void  LoaCopyToCatchdData ()
	{

		string saveZipPath = Application.persistentDataPath + "/Data/Data." + LzmaTools.zipName;
		string spriteDir = Application.persistentDataPath + "/Data";
		if (!Directory.Exists (spriteDir)) {
			Directory.CreateDirectory (spriteDir);
		}

		string zipPath = Application.streamingAssetsPath + "/Data/Data." + LzmaTools.zipName;
		Debug.LogError ("zys  get  zip");
		StartCoroutine (CopZIPFile (zipPath, saveZipPath));


	}

	IEnumerator CopZIPFile (string filePath, string saveZipPath)
	{
		string fielURLPath = "" + filePath;

		#if UNITY_STANDALONE_WIN || UNITY_EDITOR
		File.Copy (filePath, saveZipPath, true);
		yield return null;

		#else
		#if UNITY_IPHONE
		fielURLPath = "file://"+ filePath;
		#endif
	

		WWW www = new WWW(fielURLPath);
		yield return www;

		FileStream oneZip = File.Create (saveZipPath);
		oneZip.Write (www.bytes, 0, www.bytes.Length);
		oneZip.Close ();
		#endif

		bool bUnzipDone = UnCommonZip (saveZipPath);


	}

	private bool  UnCommonZip (string path)
	{	
		//		string spriteDir = Application.persistentDataPath + "/Data";
		if (!File.Exists (path)) {
			Debug.LogError ("zys error zip null");
			return true;
		}
		try {
			Debug.LogError ("zys start lzmu unCompose ==  " + path);
			string assetbundlePath = Application.persistentDataPath + "/Data";
			string outFilePath = Application.persistentDataPath + "";
			bool isOk =	LzmaTools.DecompressLzma (assetbundlePath, "Data." + LzmaTools.zipName, outFilePath);

			#if UNITY_EDITOR

			#else
			if(File.Exists(path))
			{
			File.Delete(path);
			}
			#endif

		} catch (System.Exception e) {
			Debug.LogError ("unZIp  error " + e);

			return false;
		}

		return true;
	}


}
