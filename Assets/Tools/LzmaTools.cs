using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;
using SevenZip.Compression.LZMA;
using System.Collections.Generic;

namespace ZTools
{
	public class LzmaTools
	{
		public LzmaTools ()
		{
		}

		public static string zipName = "lzma";

		/// <summary>
		/// Decompresses the lzma.
		/// 解压相应路径的文件到 目标位置;
		/// </summary>
		/// <returns><c>true</c>, if lzma was decompressed, <c>false</c> otherwise.</returns>
		/// <param name="zipfilePath">Zipfile path.</param>
		/// <param name="zipFileName">Zip file name.</param>
		/// <param name="outFilePath">Out file path.</param>
		public static  bool DecompressLzma (string zipfilePath, string zipFileName, string outFilePath)
		{
			string destFolderPath = zipfilePath + "/" + zipFileName;
			Debug.LogError ("wiil unCompose path = "+ destFolderPath);
			FileStream compressStream = File.OpenRead(destFolderPath);//new FileStream (destFolderPath, FileMode.Open);//, FileAccess.Read
			MemoryStream tempStream = new MemoryStream ();
			Debug.LogError ("compressStream  len begin  = "+ compressStream.Length);
			SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder ();
		
			byte[] properties = new byte[5];
			compressStream.Read (properties, 0, 5);
			decoder.SetDecoderProperties (properties);

			Debug.LogError ("compressStream  len read5  = "+ compressStream.Length);

			byte[] compressLen = new byte[8];
			compressStream.Read (compressLen, 0, 8);
			long outsize = 0;
			for (int i = 0; i < 8; i++) {
				outsize |= (long)((byte)compressLen [i] << (8 * i));
			}
			Debug.LogError ("compressStream  len read8  = "+ compressStream.Length);

			long compressRealLen = compressStream.Length - compressStream.Position;
			Debug.LogError ("compressRealLen = "+ compressRealLen);
			decoder.Code (compressStream, tempStream, compressRealLen, outsize, null);

			// importtant !!!
			tempStream.Position = 0;

			try {
				Debug.LogError("zys  try  uncompose tools tempStream.Position "+ tempStream.Position +" tempStream.Length "+ tempStream.Length);
				byte[] head = new byte[1024];
				int index = 0;
				while (tempStream.Position != tempStream.Length) {
					int one = tempStream.ReadByte ();
					if (one != 10) { // separator: \n
						head [index++] = (byte)one;
					} else {
						index = 0;
						string headStr = Encoding.UTF8.GetString (head);
						head = new byte[1024];

						string[] headArr = headStr.Split (',');
						string fileBundleName = headArr [0];
						int lastXIndex = fileBundleName.LastIndexOf ("/");
						string fileName = fileBundleName.Substring (lastXIndex + 1, fileBundleName.Length - lastXIndex - 1);
						string fileFolderName = fileBundleName.Substring (0, lastXIndex);

						int fileSize = 0;
						if (!int.TryParse (headArr [1], out fileSize)) {
							Console.WriteLine ("File size exception!!! -->> " + headArr [1]);

							break;
						}

						string filePath = outFilePath + "/" + fileFolderName;
						if (!Directory.Exists (filePath)) {
							Directory.CreateDirectory (filePath);
						}
						Debug.LogError("warie file "+filePath+ "/" + fileName);
						WriteToLocal (tempStream, fileSize, filePath + "/" + fileName);
					}
				}

				tempStream.Flush ();
				tempStream.Close ();
				Debug.LogError("zys  end uncompose");

			} catch (Exception exe) {
				Console.WriteLine ("unCompose error -->> " + exe);
				return false;
			}
			return true;
		}

		/// <summary>
		/// decompress to corresponding folder
		/// </summary>
		private static void WriteToLocal (MemoryStream file, int fileLen, string intoPath)
		{
			FileStream output = new FileStream (intoPath, FileMode.Create, FileAccess.Write);
			byte[] temp = new byte[fileLen];

			int readResult = file.Read (temp, 0, fileLen);
			output.Write (temp, 0, readResult);

			output.Flush ();
			output.Close ();
		}
	}

}
