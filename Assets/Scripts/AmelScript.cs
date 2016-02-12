using UnityEngine;
using System.Collections;

public class AmelScript : MonoBehaviour {

	LevelController lvlC;
	
	void Start () 
	{
		lvlC=GetComponent<LevelController>();
	}
	
	public void ImproveRecolt()
	{
		lvlC.ImproveLevel(typeBrig.Recolt);
	}

	public void ImproveStrong()
	{
		lvlC.ImproveLevel(typeBrig.Strong);
	}

	public void ImproveScout()
	{
		lvlC.ImproveLevel(typeBrig.Scout);
	}

	public void ImproveSabot()
	{
		lvlC.ImproveLevel(typeBrig.Sabot);
	}


}
