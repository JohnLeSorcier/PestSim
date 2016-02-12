using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
 
/// <summary>
/// Create a New INI file to store or load data
/// </summary>
public class FileGest : MonoBehaviour
{
 
    [DllImport("kernel32")]
    private static extern long WritePrivateProfileString(string section,
        string key, string val, string filePath);
    [DllImport("kernel32")]
    private static extern int GetPrivateProfileString(string section,
             string key, string def, StringBuilder retVal,
        int size, string filePath);


    void Start()
    {
    	//initFile();
    }
 
 	/// <summary>
	/// Write Data to the INI File
  	/// </summary>
 	/// <param name="Section">Section.</param>
 	/// <param name="Key">Key.</param>
  	/// <param name="Value">Value.</param>
 	/// <param name="_path">Path.</param>
    public static void IniWriteValue(string Section, string Key, string Value, string _path)
    {
        WritePrivateProfileString(Section, Key, Value, _path);
    }
 
    /// <summary>
    /// Read Data Value From the Ini File
    /// </summary>
    /// <PARAM name="Section"></PARAM>
    /// <PARAM name="Key"></PARAM>
    /// <PARAM name="Path"></PARAM>
	/// <param name="_path">Path.</param>
    public static string IniReadValue(string Section, string Key, string _path)
    {
        StringBuilder temp = new StringBuilder(255);
        GetPrivateProfileString(Section, Key, "", temp, 255, _path);
        return temp.ToString();
 
    }

    void initFile()
    {
		string path = Application.dataPath + "/Ressources/Bug.ini";

		if(!File.Exists(path))
    		File.WriteAllText(path,"");

		FileGest.IniWriteValue("Recolt","speed","1", path);
		FileGest.IniWriteValue("Recolt","maxLife","100", path);
		FileGest.IniWriteValue("Recolt","foodTaking","1", path);
		FileGest.IniWriteValue("Recolt","waterTaking","1", path);
		FileGest.IniWriteValue("Recolt","constructTaking","0", path);
		FileGest.IniWriteValue("Recolt","foodConsum","1", path);
		FileGest.IniWriteValue("Recolt","waterConsum","1", path);
		FileGest.IniWriteValue("Recolt","destructCap","0", path);
		FileGest.IniWriteValue("Recolt","speedSweep","0", path);
		FileGest.IniWriteValue("Recolt","coldResist","0", path);
		FileGest.IniWriteValue("Recolt","hotResist","0", path);
		FileGest.IniWriteValue("Recolt","poisonResist","0", path);
		FileGest.IniWriteValue("Recolt","physicResist","0", path);
		FileGest.IniWriteValue("Strong","speed","1", path);
		FileGest.IniWriteValue("Strong","maxLife","100", path);
		FileGest.IniWriteValue("Strong","foodTaking","0.5", path);
		FileGest.IniWriteValue("Strong","waterTaking","0.5", path);
		FileGest.IniWriteValue("Strong","constructTaking","1", path);
		FileGest.IniWriteValue("Strong","foodConsum","1", path);
		FileGest.IniWriteValue("Strong","waterConsum","1", path);
		FileGest.IniWriteValue("Strong","destructCap","1", path);
		FileGest.IniWriteValue("Strong","speedSweep","0", path);
		FileGest.IniWriteValue("Strong","coldResist","0.1", path);
		FileGest.IniWriteValue("Strong","hotResist","0.1", path);
		FileGest.IniWriteValue("Strong","poisonResist","0", path);
		FileGest.IniWriteValue("Strong","physicResist","0.2", path);
		FileGest.IniWriteValue("Scout","speed","1.5", path);
		FileGest.IniWriteValue("Scout","maxLife","100", path);
		FileGest.IniWriteValue("Scout","foodTaking","0.5", path);
		FileGest.IniWriteValue("Scout","waterTaking","0.5", path);
		FileGest.IniWriteValue("Scout","constructTaking","0", path);
		FileGest.IniWriteValue("Scout","foodConsum","1", path);
		FileGest.IniWriteValue("Scout","waterConsum","1", path);
		FileGest.IniWriteValue("Scout","destructCap","0", path);
		FileGest.IniWriteValue("Scout","speedSweep","0", path);
		FileGest.IniWriteValue("Scout","coldResist","0.2", path);
		FileGest.IniWriteValue("Scout","hotResist","0.2", path);
		FileGest.IniWriteValue("Scout","poisonResist","0", path);
		FileGest.IniWriteValue("Scout","physicResist","0", path);
		FileGest.IniWriteValue("Sabot","speed","1", path);
		FileGest.IniWriteValue("Sabot","maxLife","100", path);
		FileGest.IniWriteValue("Sabot","foodTaking","0.5", path);
		FileGest.IniWriteValue("Sabot","waterTaking","0.5", path);
		FileGest.IniWriteValue("Sabot","constructTaking","0.5", path);
		FileGest.IniWriteValue("Sabot","foodConsum","1", path);
		FileGest.IniWriteValue("Sabot","waterConsum","1", path);
		FileGest.IniWriteValue("Sabot","destructCap","0", path);
		FileGest.IniWriteValue("Sabot","speedSweep","1", path);
		FileGest.IniWriteValue("Sabot","coldResist","0", path);
		FileGest.IniWriteValue("Sabot","hotResist","0", path);
		FileGest.IniWriteValue("Sabot","poisonResist","0.2", path);
		FileGest.IniWriteValue("Sabot","physicResist","0", path);
    }
}