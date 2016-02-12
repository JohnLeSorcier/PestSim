using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ImproveButtonScript : MonoBehaviour {

	public typeBrig typeBrigade;
	public int level;

	Button bouton;
	LevelController lvlC;

	void Start () 
	{
		bouton=GetComponent<Button>();

		GameObject levelControllerObject = GameObject.FindWithTag ("LevelController");
		if (levelControllerObject != null)
			lvlC = levelControllerObject.GetComponent <LevelController>();
		else
			Debug.Log ("Cannot find 'LevelController' script");


		
	}
	void Update () 
	{
		if(lvlC.levelBrig(typeBrigade)==level && lvlC.checkRessourceForImprove(typeBrigade))
			bouton.interactable=true;
		else
			bouton.interactable=false;

		string textBut="";
		if (lvlC.levelBrig(typeBrigade)<level)
			textBut="Locked";
		else
		{
			textBut=""+typeBrigade+" to level "+(level+1);
			if(lvlC.levelBrig(typeBrigade)==level)
				textBut+="\n(Need "+lvlC.ressourceForImprove(typeBrigade)+" chemical)";
				
		}
		bouton.GetComponentInChildren<Text>().text=textBut;

	}

}
