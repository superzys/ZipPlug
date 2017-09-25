using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;
using UnityEditor;
using SevenZip.Compression.LZMA;
using System.Collections.Generic;
using ZTools;

public class LZMPMenu : EditorWindow
{
	private static LZMPMenu window;

    static List<UnityEngine.Object> resourcesLis = new List<UnityEngine.Object>();
    static Dictionary<string, UnityEngine.Object> resourcesDic = new Dictionary<string, UnityEngine.Object>();
            
    UnityEngine.Object lisObj ;
    Vector2 scrollPos = new Vector2(0, 0);
    
    
	static List<string> autoPathArr = new List<string>();

	[MenuItem("Tools/AutoComposeDataZIP")]
	private static void LZMAComposeData()
	{
		autoPathArr.Clear();
		string filePath = 		 "./Data";
		GetFilePathToList (filePath);
		CompressLzmaFromDir(false);

		autoPathArr.Clear();
	}


	[MenuItem("Tools/Open_LZMA_UI")]
    private static void LZMATest()
    {
		window = EditorWindow.GetWindow<LZMPMenu>();

        window.titleContent = new GUIContent("LZMA");
    }

    void OnGUI()
    {
        EditorGUILayout.ObjectField(lisObj, typeof(UnityEngine.Object), true);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < resourcesLis.Count; i++)
        {
            EditorGUILayout.ObjectField(resourcesLis[i], typeof(UnityEngine.Object), true);
        }

        EditorGUILayout.EndScrollView();

        if (GUI.changed)
        {
            string[] filePath = DragAndDrop.paths;
            if (filePath == null || filePath.Length <= 0)
            {
                return;
            }

            for (int i = 0; i < filePath.Length; i++)
            {
                GetAssetsToLis(filePath[i]);
            }

            AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Clear"))
        {
            resourcesLis.Clear();
            resourcesDic.Clear();
        }

        if (GUILayout.Button("Compress"))
        {
	        CompressLzmaFromDir(true);
        }

        if (GUILayout.Button("DeCompress"))
        {
            DecompressLzma();
        }
    }
	
	private static void GetFilePathToList(string assetPath)
	{
		if(Directory.Exists(assetPath))    // i only set directory drop or drap
		{
			string[] filePaths = Directory.GetFiles(assetPath, "*.*", SearchOption.AllDirectories);
			for (int i = 0; i < filePaths.Length; i++)
			{
				if(filePaths[i].Contains(".meta")|| filePaths[i].Contains(".DS_Store"))
				{
					continue;
				}

				if(!autoPathArr.Contains(filePaths[i]))
				{
			
					autoPathArr.Add(filePaths[i]);
				}
			}
		}
	}

	/// <summary>
	/// collect assets to resourcesDic
	/// </summary>
	/// <param name="assetPath">Asset path.</param>
    private static void GetAssetsToLis(string assetPath)
    {
        if(Directory.Exists(assetPath))    // i only set directory drop or drap
        {
            string[] filePaths = Directory.GetFiles(assetPath, "*.*", SearchOption.AllDirectories);
            for (int i = 0; i < filePaths.Length; i++)
            {
				if(filePaths[i].Contains(".meta")|| filePaths[i].Contains(".DS_Store"))
                {
                    continue;
                }

                if(!resourcesDic.ContainsKey(filePaths[i]))
                {
                    resourcesDic.Add(filePaths[i], AssetDatabase.LoadMainAssetAtPath(filePaths[i]));
                    resourcesLis.Add(AssetDatabase.LoadMainAssetAtPath(filePaths[i]));
	                autoPathArr.Add(filePaths[i]);
                }
            }
        }
    }

	/// <summary>
	/// 从缓冲的目录中压缩;
	/// </summary>
	/// <param name="isAssetDir"> 是否是 asset 中的目录</param>
	private static void CompressLzmaFromDir(bool  isAssetDir)
	{
		if(autoPathArr == null || autoPathArr.Count <= 0)
		{
			return;
		}
		string assetBundlePath = Application.dataPath + "/StreamingAssets/Data";
		string lzmaFilePath = assetBundlePath + "/Data." +LzmaTools.zipName;
		if(!Directory.Exists(assetBundlePath))
		{
			Directory.CreateDirectory(assetBundlePath);
		}
 try
        {
			if(File.Exists(lzmaFilePath))
			{
				File.Delete(lzmaFilePath);
			}

            MemoryStream memoryStream = new MemoryStream();
            FileStream compressStream = new FileStream(lzmaFilePath, FileMode.OpenOrCreate, FileAccess.Write);

            int lastIndex = Application.dataPath.LastIndexOf("/");
            string prePath = Application.dataPath.Substring(0, lastIndex + 1);
            int filePathCount = autoPathArr.Count;
            for (int i = 0; i < filePathCount; i++)
            {
                string assetPath = autoPathArr[i];
				if(assetPath != null && assetPath != "")
				{
					string filePath = prePath + assetPath;
					string zipBundlePath = "";
					if (isAssetDir)
					{
						zipBundlePath = assetPath.Replace("Assets/", "");
					}
					else
					{
						zipBundlePath = assetPath.Replace("./", "");
						filePath = filePath.Replace("./", "");
					}
				

					FileStream tempFileStream = File.Open(filePath, FileMode.Open);

					StringBuilder sb = new StringBuilder(); // set header info: path + filesie + separator
					sb.Append(zipBundlePath).Append(",").Append(tempFileStream.Length).Append("\n");

					byte[] tempBuff = new byte[tempFileStream.Length];
					byte[] header = Encoding.UTF8.GetBytes(sb.ToString());
					tempFileStream.Read(tempBuff, 0, (int)tempFileStream.Length);     // get file data

					memoryStream.Write(header, 0, header.Length);
					memoryStream.Write(tempBuff, 0, tempBuff.Length);

					tempFileStream.Close();
				}

            }

            // important !!!
            memoryStream.Position = 0;

            SevenZip.Compression.LZMA.Encoder encoder = new SevenZip.Compression.LZMA.Encoder();

            encoder.WriteCoderProperties(compressStream);

            byte[] compressLen = new byte[8];  // file size
            for (int i = 0; i < compressLen.Length; i++)
            {
                compressLen[i] = (byte)(memoryStream.Length >> (8 * i));
            }
            compressStream.Write(compressLen, 0, 8);

            CodeProgress codeProgress = new CodeProgress(); // compress
            codeProgress.totalSize = memoryStream.Length;
            encoder.Code(memoryStream, compressStream, memoryStream.Length, -1, codeProgress);

            memoryStream.Flush();
            memoryStream.Close();
            compressStream.Close();

            AssetDatabase.Refresh(); // refresh asssets
            EditorUtility.ClearProgressBar();
	        
	        Debug.Log("success  compress ");
        }
        catch (Exception exe)
        {
            Debug.LogError(exe.Message);
        }
		
	}
    /// <summary>
    /// decompress lzma file
    /// </summary>
    private static void DecompressLzma()
    {
        string assetbundlePath = Application.streamingAssetsPath + "/Data";
		string tarPath = Application.streamingAssetsPath + "";

		string destFolderPath = assetbundlePath + "/Data."+LzmaTools.zipName;
		string saveZipPath = Application.persistentDataPath + "/Data/Data."+LzmaTools.zipName;
		#if UNITY_STANDALONE_WIN || UNITY_EDITOR
		if(File.Exists(saveZipPath))
		{
			File.Delete(saveZipPath);
		}
		File.Copy (destFolderPath, saveZipPath, true);
//		yield return null;
//		#else
//		Debuger.LogError ("zys  start  CopZIPFile "+ destFolderPath);
//		WWW www = new WWW(destFolderPath);
////		yield return www;
//
//		FileStream oneZip = File.Create (saveZipPath);
//
//		oneZip.Write (www.bytes, 0, www.bytes.Length);
//		oneZip.Close ();
//		Debuger.LogError ("zys  get  CopZIPFile");
		#endif


		LzmaTools.DecompressLzma (Application.persistentDataPath + "/Data","Data."+LzmaTools.zipName,tarPath);
	    Debug.Log("success  Decompress");
    }


    /// <summary>
    /// call back
    /// </summary>
    public class CodeProgress : SevenZip.ICodeProgress
    {
        // bytes
        public long totalSize { get; set; }

        public void SetProgress(Int64 inSize, Int64 outSize)  //show progress
        {
			EditorUtility.DisplayProgressBar(LzmaTools.zipName, "Compress or decompress", (float)inSize / totalSize);
        }
    }



}
