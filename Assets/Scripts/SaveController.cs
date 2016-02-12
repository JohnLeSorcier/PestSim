using UnityEngine;
using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class SaveController : MonoBehaviour 
{
	LevelController lvlC;

	void Start()
	{
		lvlC=new LevelController();
		GameObject levelControllerObject = GameObject.FindWithTag ("LevelController");
		if (levelControllerObject != null)
			lvlC = levelControllerObject.GetComponent <LevelController>();
		else
			Debug.Log ("Cannot find 'LevelController' script");
	}

	public void Save()
	{
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file= File.Create(Application.persistentDataPath + "/Save.sav");

		DataSave data = CreateData();

		bf.Serialize(file,data);
		file.Close();
	}

	public void Load()
	{
		if(File.Exists(Application.persistentDataPath +"/Save.sav"))
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file= File.Open(Application.persistentDataPath + "/Save.sav",FileMode.Open);
			DataSave data=(DataSave)bf.Deserialize(file);
			file.Close();
			LoadData(data);
		}
	}

	DataSave CreateData ()
	{
		DataSave data = new DataSave();

		data.waterStock=lvlC.waterStock;
		data.foodStock=lvlC.foodStock;
		data.constructStock=lvlC.constructStock;
		data.chemStock=lvlC.chemStock;
		data.levelRecolt=lvlC.levelRecolt;
		data.levelStrong=lvlC.levelStrong;
		data.levelScout=lvlC.levelScout;
		data.levelSabot=lvlC.levelSabot;
		data.timer=lvlC._time;
		data.amelDisp=lvlC.amelDisp;


		GameObject[] brigObjects =GameObject.FindGameObjectsWithTag("Brigade");

		data.nbBrig=brigObjects.Length;
		data.brigadesType=new typeBrig[data.nbBrig];
		data.brigadesPos=new float[data.nbBrig,3];
		data.brigadesLife=new float[data.nbBrig];
		data.brigadesSpeed=new float[data.nbBrig];
		data.brigadesPoison=new float[data.nbBrig,2];

		int i=0;
		foreach (GameObject brigO in brigObjects)
		{
			BrigadeController brig=brigO.GetComponent<BrigadeController>();
			data.brigadesType[i]=brig.typeBrigade;
		 	data.brigadesPos[i,0]=brig.transform.position.x;
			data.brigadesPos[i,1]=brig.transform.position.y;
			data.brigadesPos[i,2]=brig.transform.position.z;
		 	data.brigadesLife[i]=brig.life;
		 	data.brigadesSpeed[i]=brig.speed;
		 	data.brigadesPoison[i,0]=brig.damagePoison;
			data.brigadesPoison[i,1]=brig.lenghtPoisonRemain;

		 	i++;
		}



		return data;
	}

	void LoadData(DataSave data)
	{
		lvlC.waterStock=data.waterStock;
		lvlC.foodStock=data.foodStock;
		lvlC.constructStock=data.constructStock;
		lvlC.chemStock=data.chemStock;
		lvlC.levelRecolt=data.levelRecolt;
		lvlC.levelStrong=data.levelStrong;
		lvlC.levelScout=data.levelScout;
		lvlC.levelSabot=data.levelSabot;
		lvlC.timeRemain=data.timer;
		lvlC.amelDisp=data.amelDisp;

		for (int i=0; i<data.nbBrig; i++)
		{
			Vector3 pos=new Vector3(data.brigadesPos[i,0],data.brigadesPos[i,1],data.brigadesPos[i,2]);
			BrigadeController brig=lvlC.AddBrigade(data.brigadesType[i], pos);
			brig.isSelected=false;
		}

		//penser à "isPoisonned"
	}

}

[Serializable]
class DataSave
{

	[HideInInspector] public float waterStock;
	[HideInInspector] public float foodStock;
	[HideInInspector] public float constructStock;
	[HideInInspector] public float chemStock;
	[HideInInspector] public int levelRecolt;
	[HideInInspector] public int levelStrong;
	[HideInInspector] public int levelScout;
	[HideInInspector] public int levelSabot;
	[HideInInspector] public float timer;
	[HideInInspector] public bool amelDisp;
	[HideInInspector] public int nbBrig;
	[HideInInspector] public typeBrig[] brigadesType;
	[HideInInspector] public float[,] brigadesPos;
	[HideInInspector] public float[] brigadesLife;
	[HideInInspector] public float[] brigadesSpeed;
	[HideInInspector] public float[,] brigadesPoison;
}