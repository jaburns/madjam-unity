// Toony Colors Pro+Mobile 2
// (c) 2014,2015 Jean Moreno

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// Menu Options for Toony Colors Pro 2

public static class TCP2_Menu
{
	//Change this path if you want the Toony Colors 2 menu to appear elsewhere in the menu bar
	public const string MENU_PATH = @"Tools/Toony Colors 2/";

	//--------------------------------------------------------------------------------------------------
	// DOCUMENTATION

	[MenuItem(MENU_PATH + "Documentation", false, 0)]
	static void OpenDocumentation()
	{
		TCP2_GUI.OpenHelp();
	}

	//--------------------------------------------------------------------------------------------------
	// UNPACK SHADERS

	[MenuItem(MENU_PATH + "Unpack Shaders/Rim (Desktop)")]
	static void UnpackRim() { UnpackShaders("rim desktop"); }
	[MenuItem(MENU_PATH + "Unpack Shaders/Rim (Mobile)")]
	static void UnpackRimMobile() { UnpackShaders("rim mobile"); }
	[MenuItem(MENU_PATH + "Unpack Shaders/Reflection (Desktop)")]
	static void UnpackReflectionDesktop() { UnpackShaders("reflection desktop"); }
	[MenuItem(MENU_PATH + "Unpack Shaders/Matcap (Mobile)")]
	static void UnpackMatcapMobile() { UnpackShaders("matcap mobile"); }
	[MenuItem(MENU_PATH + "Unpack Shaders/All Shaders")]
	static void UnpackAll() { UnpackShaders(""); }

	static private void UnpackShaders(string filter)
	{
		string[] archFiles = Directory.GetFiles( TCP2_Utils.UnityToSystemPath(Application.dataPath), "TCP2 Packed Shaders.tcp2data", SearchOption.AllDirectories );
		if(archFiles == null || archFiles.Length == 0)
		{
			EditorApplication.Beep();
			Debug.LogError("[TCP2 Unpack Shaders] Couldn't find file: \"TCP2 Packed Shaders.tcp2data\"\nPlease reimport Toony Colors Pro 2.");
			return;
		}
		string archivePath = archFiles[0];
		if(archivePath.EndsWith(".tcp2data"))
		{
			TCP2_Utils.PackedFile[] files = TCP2_Utils.ExtractArchive(archivePath, filter);

			int @continue = 0;
			if(files.Length > 8)
			{
				do
				{
					@continue = EditorUtility.DisplayDialogComplex("TCP2 : Unpack Shaders", "You are about to import " + files.Length + " shaders in Unity.\nIt could take a few minutes!\nContinue?", "Yes", "No", "Help");
					if(@continue == 2)
					{
						TCP2_GUI.OpenHelpFor("Unpack Shaders");
					}
				}
				while(@continue == 2);
			}

			if(@continue == 0 && files.Length > 0)
			{
				string tcpRoot = TCP2_Utils.FindReadmePath();
				foreach(TCP2_Utils.PackedFile f in files)
				{
					string filePath = tcpRoot + f.path;
					string fileDir = Path.GetDirectoryName(filePath);
					if(!Directory.Exists(fileDir))
					{
						Directory.CreateDirectory(fileDir);
					}
					File.WriteAllText(filePath, f.content);
				}
				
				Debug.Log("Toony Colors Pro 2 - Unpack Shaders:\n" + files.Length + (files.Length > 1 ? " shaders extracted." : " shader extracted."));
				AssetDatabase.Refresh();
			}

			if(files.Length == 0)
			{
				Debug.Log("Toony Colors Pro 2 - Unpack Shaders:\nNothing to unpack. Shaders are probably already unpacked!");
			}
		}
	}

	//--------------------------------------------------------------------------------------------------
	// RESET MATERIAL

	[MenuItem(MENU_PATH + "Reset Selected Material(s)")]
	static void ResetSelectedMaterials()
	{
		foreach(Object o in Selection.objects)
		{
			if(o is Material)
			{
				bool user = false;
				List<string> keywordsList = new List<string>((o as Material).shaderKeywords);
				if(keywordsList.Contains("USER"))
					user = true;
				(o as Material).shaderKeywords = user ? new string[]{"USER"} : new string[0];
				if((o as Material).shader != null && (o as Material).shader.name.Contains("Mobile"))
					(o as Material).shader = Shader.Find("Toony Colors Pro 2/Mobile");
				else
					(o as Material).shader = Shader.Find("Toony Colors Pro 2/Desktop");
				Debug.Log("[TCP2] Keywords reset for " + o.name);
			}
		}
	}

	[MenuItem(MENU_PATH + "Reset Selected Material(s)", true)]
	static bool ResetSelectedMaterials_Validate()
	{
		foreach(Object o in Selection.objects)
		{
			if(o is Material)
			{
				return true;
			}
		}

		return false;
	}
}
