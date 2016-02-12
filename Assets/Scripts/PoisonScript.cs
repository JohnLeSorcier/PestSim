using UnityEngine;
using System.Collections;

public class PoisonScript : MonoBehaviour {

	public float poisonValue=1;
	public float lenght=10;
	
	public float solidity=5;
	
	float speedSweeping=0;
	bool isSweeping=false;
	
	Vector3 objectPos;
	
	LevelController lvlC;
	
	bool PopUp=false;
	
	void Start()
	{
		GameObject levelControllerObject = GameObject.FindWithTag ("LevelController");
		if (levelControllerObject != null)
			lvlC = levelControllerObject.GetComponent <LevelController>();
		else
			Debug.Log ("Cannot find 'LevelController' script");
	}
	
	void FixedUpdate()
	{
		if (PopUp && Input.GetMouseButtonUp(0))
		{
			Invoke ("closePopUp",0.1f);
		}
	}

	
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other is CircleCollider2D && other.CompareTag("Brigade"))
		{
			BrigadeController brigade=other.GetComponent<BrigadeController>();
			if (brigade.typeBrigade!=typeBrig.Sabot)
			{
				brigade.Poison(poisonValue,lenght);
				poisonValue--;
				if (poisonValue==0)
					Destroy (this.gameObject);
			}
			else
				Sweep(brigade.speedSweep);
		}
	}
	
	void OnTriggerExit2D(Collider2D other)
	{
		if(other is CircleCollider2D && other.CompareTag("Brigade"))
		{
			BrigadeController brigade=other.GetComponent<BrigadeController>();
			if (brigade.typeBrigade==typeBrig.Sabot)
				UnSweep(brigade.speedSweep);
		}
	}

	/// <summary>
	/// Déminage
	/// </summary>
	/// <param name="sp">vitesse de déminage à ajouter</param>
	void Sweep (float sp)
	{
		isSweeping=true;
		speedSweeping+=sp;	
		if(!IsInvoking("RunSweep"))
			Invoke("RunSweep",ListConst.sweepDelay);
	}

	/// <summary>
	/// Arrêt du déminage
	/// </summary>
	/// <param name="sp">vitesse de déminage</param>
	void UnSweep(float sp)
	{
		speedSweeping-=sp;
		if(speedSweeping<=0)
		{
			speedSweeping=0;
			isSweeping=false;
		}
	}
	
	void RunSweep()
	{
		if (isSweeping)
		{
			solidity-=speedSweeping;
			if(solidity<=0)
			{
				isSweeping=false;
				lvlC.AddChem (poisonValue);
				Destroy(this.gameObject);
			}
			else if (!IsInvoking("RunSweep"))
				Invoke("RunSweep",ListConst.sweepDelay);
		}
	}
	
	void OnMouseUp()
	{
		PopUp = true;
	}
	
	void OnGUI()
	{
		objectPos=Camera.main.WorldToScreenPoint(transform.position);
		DrawInfo ();
		if (isSweeping)
		{
			DrawSweep();
		}
	}
	
	
	void DrawInfo()
	{
		Rect rect = new Rect (objectPos.x,Screen.height-objectPos.y, 120, 80 );
		Rect close = new Rect (objectPos.x+100,Screen.height-objectPos.y,20,20);
		
		
		if(PopUp)
		{
			GUI.Box(rect, textInfo ());
			if (GUI.Button(close,"X"))
			{
				closePopUp();
			}
		}
	}
	
	void closePopUp()
	{
		PopUp=false;
	}
	
	
	void DrawSweep()
	{
		Rect rect=new Rect (objectPos.x-25, Screen.height-objectPos.y-10,solidity*20, 20);
		GUI.Box(rect, "sweeping");
	}
	
	public string textInfo()
	{
		string Info="Value: "+poisonValue+"\n";
		Info+="Lenght: "+lenght+"s\n";
		Info+="Solidity: "+solidity+"\n";
		
		return Info;
	}

	

}
