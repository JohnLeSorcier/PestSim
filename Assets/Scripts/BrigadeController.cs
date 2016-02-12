using UnityEngine;
using System.Collections;
using System.Globalization;
using Pathfinding;

public enum typeBrig
{
	Recolt, Strong, Scout, Sabot
}

public enum typeSpecies
{
	Bug
}


public class BrigadeController : MonoBehaviour {
	
	//variable pour la sélection
	Vector3 mousePos;
	Vector3 currentPos;
	Vector3 target;
	[HideInInspector] public bool isSelected=false;

	CafardScript[] children;//récupérer les objets affichés

	//A partir de l'espèce et du type de brigade, toutes les variables sont récupérés
	public typeSpecies Species;
	public typeBrig typeBrigade;
	[HideInInspector] public float speed = 1f;
	float initSpeed;
	float maxLife=100f;
	[HideInInspector] public float life;	
	[HideInInspector] public float foodTaking=0f; //vitesse de récupération de nourriture
	[HideInInspector] public float waterTaking=0f; //vitesse de récupération d'eau
	[HideInInspector] public float constructTaking=0f; //vitesse de récupération d'éléments de construction
	float foodConsum=1f; //consommation de nourriture
	float waterConsum=1f;//consommation d'eau
	[HideInInspector] public float destructCap=0;	//capacité de destruction
	[HideInInspector] public float speedSweep=0;	//vitesse de déminage


	//variable lié à l'empoisonnement
	bool isPoisoned=false;
	[HideInInspector] public float damagePoison=0;
	float lenghtPoison=0;
	float _tempLenght=0;	
	[HideInInspector] public float lenghtPoisonRemain;//for save

	//variable de résistance aux malus et pièges 
	float coldResist=0;
	float hotResist=0;
	float poisonResist=0;
	float physicResist=0;


	//si brigade dans un objet
	[HideInInspector] public bool isInObject=false;
	[HideInInspector] public ObjectScript objectIn;
	
	Vector3 objectPos;
	
	public GUISkin skin;	
	public Texture2D icon;	
	public Texture2D deathIcon;
	
	LevelController lvlC;

	//lié à l'affichage des menus
	bool rightClickMenu=false;
		
	bool clickMeuble=false;
	[HideInInspector]public ObjectScript meubleToGo;
	[HideInInspector]public bool goToMeuble=false;
	[HideInInspector]public bool underMeuble=false;
	
	bool clickPiege=false;
	PoisonScript piege;
	
	[HideInInspector] public bool isColonizing=false;

	int currentLevel;

	bool viewInfo=false;

	//malus pouvant agir sur la brigade
	typeMalus malusActived;

	//éléments nécessaire au pathfinding
	public Path path;
	Seeker seeker;
	public float nextWaypointDistance = 0.00f;
	private int currentWaypoint = 0;



	
	
	public void Awake() 
	{
		
		GameObject levelControllerObject = GameObject.FindWithTag ("LevelController");
		if (levelControllerObject != null)
			lvlC = levelControllerObject.GetComponent <LevelController>();
		else
			Debug.Log ("Cannot find 'LevelController' script");
	
		ConfigBrig();
		
		target=transform.position;
		children=GetComponentsInChildren<CafardScript>();
		life=maxLife;
		initSpeed=speed;

		seeker = GetComponent<Seeker>();

		Invoke("Consume",ListConst.consumDelay);

	}




	void Update()
	{	
		CheckLevel();//vérification du niveau actuel
	
		if(!isInObject)
		{	
			
			if (isSelected && Input.GetMouseButtonDown(1))
			{
				mousePos=Camera.main.ScreenToWorldPoint (Input.mousePosition);
				mousePos.z=transform.position.z;
				piege=null;
				goToMeuble=false;
				meubleToGo=null;
				clickMeuble=false;
				clickPiege=false;
				Collider2D collide = Physics2D.OverlapCircle(mousePos,0.01f);
				if(Input.GetKey(KeyCode.LeftControl))
				{
					
					rightClickMenu=true;
					if(collide!=null)
					{ 
						if(collide.CompareTag("Meuble"))
						{
							clickMeuble=true;
							meubleToGo=collide.GetComponent<ObjectScript>();
						}
						if(collide.CompareTag("Piege"))
						{
							clickPiege=true;
							piege=collide.GetComponent<PoisonScript>();
						}
					}
					
				}
				else
				{
					target=mousePos;
					if(collide!=null && collide.CompareTag("Meuble"))
					{
						meubleToGo=collide.GetComponent<ObjectScript>();
						moveToMeuble(meubleToGo);
					}
					else
					{
						moveToDest(target);	
					}
					if(rightClickMenu)
						closeRCM();
				}		
			}
			
			if(isSelected && Input.GetMouseButtonDown(0))
			{
				isSelected=false;
				selectionBrig();		
			}
			
			if(rightClickMenu && Input.GetMouseButtonUp(0))
				closeRCM();
			
			if (Input.GetMouseButton(0))
			{
				Vector3 camPos = Camera.main.WorldToScreenPoint (transform.position);
				camPos.y=LevelController.InvertMouseY(camPos.y);
				isSelected=LevelController.selectionRect.Contains(camPos);		
				selectionBrig();	
			}


			if (isSelected)
				viewInfo=true;
			else
				viewInfo=false;

		}
		
		
		if(life<=0)
		{
			Die();
		}
			
	}

	void FixedUpdate () 
	{
		if(!isInObject || Vector3.Distance (transform.position,target) < nextWaypointDistance)
		 {
			if (path == null) 
			{
				return;
			}
			if (currentWaypoint >= path.vectorPath.Count) 
			{
				return;
			}	

			transform.position=Vector3.MoveTowards(transform.position, path.vectorPath[currentWaypoint], speed*Time.fixedDeltaTime);
			RotateTowards(path.vectorPath[currentWaypoint]);
					
			
			if (Vector3.Distance (transform.position,path.vectorPath[currentWaypoint]) < nextWaypointDistance)
			{
				currentWaypoint++;
				return;
			}
		}
	}

	public void OnPathComplete (Path p) 
	{
		if (!p.error) 
		{
			path = p;
			//Reset the waypoint counter
			currentWaypoint = 0;

		}
	}

	public void moveToDest(Vector3 dest)
	{	
		seeker.StartPath(transform.position,dest,OnPathComplete);			
	}

	public void moveToMeuble(ObjectScript mb)
	{
		moveToDest(mb.transform.position);
		goToMeuble=true;
		meubleToGo=mb;
	}

	void RotateTowards(Vector3 dest)
	{
		Vector3 vectorToTarget = dest - transform.position;
		float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
		foreach(CafardScript caf in children)
			caf.changeAngle(angle);
	}

	/// <summary>
	/// Configuration des brigades
	/// Voir à metrte dans un fichier externe
	/// </summary>
	void ConfigBrig()
	{
		currentLevel=1;
		malusActived=typeMalus.None;
	
		string path = Application.dataPath + "/Ressources/"+Species+".ini";
		string _type=typeBrigade.ToString();
		speed=float.Parse(FileGest.IniReadValue(_type, "speed", path),CultureInfo.InvariantCulture.NumberFormat); 
		maxLife=float.Parse(FileGest.IniReadValue(_type, "maxLife", path),CultureInfo.InvariantCulture.NumberFormat);
		foodTaking=float.Parse(FileGest.IniReadValue(_type, "foodTaking", path),CultureInfo.InvariantCulture.NumberFormat);
		waterTaking=float.Parse(FileGest.IniReadValue(_type, "waterTaking", path),CultureInfo.InvariantCulture.NumberFormat);
		constructTaking=float.Parse(FileGest.IniReadValue(_type, "constructTaking", path),CultureInfo.InvariantCulture.NumberFormat);
		foodConsum=float.Parse(FileGest.IniReadValue(_type, "foodConsum", path),CultureInfo.InvariantCulture.NumberFormat);
		waterConsum=float.Parse(FileGest.IniReadValue(_type, "waterConsum", path),CultureInfo.InvariantCulture.NumberFormat);
		destructCap=float.Parse(FileGest.IniReadValue(_type, "destructCap", path),CultureInfo.InvariantCulture.NumberFormat);
		speedSweep=float.Parse(FileGest.IniReadValue(_type, "speedSweep", path),CultureInfo.InvariantCulture.NumberFormat);
		coldResist=float.Parse(FileGest.IniReadValue(_type, "coldResist", path),CultureInfo.InvariantCulture.NumberFormat);
		hotResist=float.Parse(FileGest.IniReadValue(_type, "hotResist", path),CultureInfo.InvariantCulture.NumberFormat);
		poisonResist=float.Parse(FileGest.IniReadValue(_type, "poisonResist", path),CultureInfo.InvariantCulture.NumberFormat);
		physicResist=float.Parse(FileGest.IniReadValue(_type, "physicResist", path),CultureInfo.InvariantCulture.NumberFormat);

	}



	void CheckLevel()
	{

		//A configurer!!
		if (currentLevel!=lvlC.levelBrig(typeBrigade))
		{
			if(typeBrigade==typeBrig.Recolt)
			{
				foodTaking*=1.2f;
				waterTaking*=1.2f;
			}
			
			if(typeBrigade==typeBrig.Strong)
			{
				constructTaking*=1.2f;
				destructCap*=1.2f;
			}
			
			if(typeBrigade==typeBrig.Scout)
			{
				speed*=1.5f;
				initSpeed=speed;
			}
			
			if(typeBrigade==typeBrig.Sabot)
			{
				speedSweep*=1.2f;
			}
			currentLevel=lvlC.levelBrig(typeBrigade);
		}
	}

	public void select()
	{
		isSelected=true;
		selectionBrig();
	}

	public void unSelect()
	{
		isSelected=false;
		selectionBrig();
	}
	
	
	void selectionBrig()
	{
		if (isSelected)
		{
			foreach (CafardScript caf in children)
				caf.Selected();	
		}
		else
		{
			foreach (CafardScript caf in children)
				caf.unSelected();	
		}
	}
	
	void Die()
	{
		if(isInObject)
			objectIn.killBrig(this);
		lvlC.RemoveBrigade();
		Destroy(this.gameObject);
	}
	
	void OnMouseUp()
	{
		if (!isInObject)
		{
			isSelected=!isSelected;
			selectionBrig();
			if(rightClickMenu)
				closeRCM();
		}
	}



	/// <summary>
	/// entrer dans un objet
	/// </summary>
	/// <param name="obj">Object.</param>
	public void inObject(ObjectScript obj)
	{
		isInObject=true;
		isSelected=false;
		goToMeuble=false;
		meubleToGo=null;
		objectIn=obj;
		if(obj.malus!=typeMalus.None)
			malusActived=obj.malus;
		Disappear();
		if(!IsInvoking("TakeMalus"))
			Invoke ("TakeMalus", ListConst.DamageDelay);
	}

	/// <summary>
	/// Sortir d'un objet
	/// </summary>
	public void outObject()
	{
		isInObject=false;
		objectIn=null;
		malusActived=typeMalus.None;
		Appear();
		if(!IsInvoking("Consume"))
			Invoke ("Consume",ListConst.consumDelay);

		goToMeuble=false;
		meubleToGo=null;

	}

	public void underObjectIn(ObjectScript obj)
	{
		underMeuble=true;
		objectIn=obj;
		foreach (CafardScript child in children)
			child.underIn();
	}

	public void underObjectOut()
	{
		underMeuble=false;
		objectIn=null;
		foreach (CafardScript child in children)
			child.underOut();
	}

	
	public void Disappear()
	{
		foreach (CafardScript child in children)
			child.Disappear();
	}
	
	public void Appear()
	{
		target=transform.position;
		foreach (CafardScript child in children)
			child.Appear();
	}

	/// <summary>
	/// Consommation de ressource
	/// Pour le moment, l'absence d'eau diminue la vie, celel de nourriture diminue la vitesse
	/// Aucune consommation si ils sont dans un objet
	/// </summary>
	void Consume()
	{
		if(!isInObject)
		{
			if(!lvlC.RemoveFood(foodConsum))
				speed=initSpeed/2;
			else
				speed=initSpeed;
			if(!lvlC.RemoveWater(waterConsum))
				life-=waterConsum;
				
				
			if (!IsInvoking("Consume"))
				Invoke ("Consume", ListConst.consumDelay);
		}
	}




	/// <summary>
	/// Prendre de la vie
	/// </summary>
	/// <param name="value">Valeur.</param>
	void takeDamage(float value)
	{
		life-=value;
	}

	/// <summary>
	/// Blesser la brigade
	/// </summary>
	/// <param name="value">Valeur.</param>
	public void Hurt(float value)
	{
		takeDamage(value*(1-physicResist));
	}

	void TakeMalus()
	{
		if (malusActived!=typeMalus.None)
		{
			float damage=0;
			if(malusActived==typeMalus.Cold)
				damage=ListConst.basicDamageMalus*(1-coldResist);
			if(malusActived==typeMalus.VeryCold)
				damage=2*ListConst.basicDamageMalus*(1-coldResist);
			if(malusActived==typeMalus.Hot)
				damage=ListConst.basicDamageMalus*(1-hotResist);
			if(malusActived==typeMalus.VeryHot)
				damage=2*ListConst.basicDamageMalus*(1-hotResist);
			if(malusActived==typeMalus.Dangerous)
				damage=ListConst.basicDamageMalus*(1-physicResist);
			takeDamage(damage);
			if(!IsInvoking("TakeMalus"))
				Invoke ("TakeMalus", ListConst.DamageDelay);
		}
			
	}

	/// <summary>
	/// Empoisonner la brigade
	/// </summary>
	/// <param name="value">Valeur.</param>
	/// <param name="lenght">Durée.</param>
	public void Poison(float value, float lenght)
	{
		isPoisoned=true;
		//POISON NON CUMULATIF (seul le temps est "prolongé")
		damagePoison=value;
		lenghtPoison=lenght;
		lenghtPoisonRemain=lenghtPoison;
		takeDamagePoison();
	}

	/// <summary>
	/// Prendre les dommages lié au poison (Invoke)
	/// </summary>
	void takeDamagePoison()
	{
		if (_tempLenght<lenghtPoison)
		{
			takeDamage(damagePoison*(1-poisonResist));
			_tempLenght++;
			lenghtPoisonRemain=lenghtPoison-_tempLenght;
			if(isPoisoned && !IsInvoking("takeDamagePoison"))
				Invoke("takeDamagePoison",ListConst.poisonDelay);
		}
		else
		{
			_tempLenght=0;
			isPoisoned=false;
		}
	}
	
	public void OnGUI()
	{
	 	if(!isInObject)
			DrawBar();
		
		if(rightClickMenu)
			DrawRightClickMenu();

		if(viewInfo && !isInObject)
			DrawInfo();
	}
	
	
	void DrawBar()
	{
		objectPos=Camera.main.WorldToScreenPoint(transform.position);
		Rect rect = new Rect (objectPos.x-20,Screen.height-objectPos.y-20, 50, 12);
		Rect lifeRect=new Rect (objectPos.x-20,Screen.height-objectPos.y-20, (50/maxLife)*life, 12);
		Rect iconRect=new Rect (objectPos.x-30,Screen.height-objectPos.y-20, 12, 12);
		string _lifeText="";
		if (isSelected)
		{
			rect = new Rect (objectPos.x-40,Screen.height-objectPos.y-30, 100, 20);
			lifeRect=new Rect (objectPos.x-40,Screen.height-objectPos.y-30, (100/maxLife)*life, 20);
			iconRect=new Rect (objectPos.x-60,Screen.height-objectPos.y-30, 20, 20);
			_lifeText="Life: "+life+"/"+maxLife;;
			Rect deathRect=new Rect (objectPos.x+60,Screen.height-objectPos.y-30, 20, 20);
			GUI.DrawTexture(deathRect,deathIcon);
			if(GUI.Button(deathRect,"", skin.button))
				Die();
			
		}
		GUI.DrawTexture(iconRect,icon);			
		if (life>0)
			GUI.Box(lifeRect, "", skin.box);
		GUI.Box(rect, _lifeText);

		
	}
	
	void DrawRightClickMenu()
	{
		Vector3 destPos=Camera.main.WorldToScreenPoint(mousePos);
		Rect rect = new Rect (destPos.x,Screen.height-destPos.y, 100, 100);
		Rect close = new Rect (destPos.x+80,Screen.height-destPos.y,20,20);
		
		string _inf="";
		if (clickMeuble)
			_inf=meubleToGo.textInfo();
		else if (clickPiege)
			_inf=piege.textInfo();
		GUI.Box(rect,_inf);
		if (GUI.Button(close,"X"))
			closeRCM();
		
		if(clickMeuble)
		{
			Rect meubleAct=new Rect (destPos.x+5, Screen.height-destPos.y+20, 90, 20);
			if(meubleToGo.solidityLevel==0)
			{
				if (GUI.Button(meubleAct,"Go In"))
				{
					if (meubleToGo!=null)
					{
						moveToMeuble(meubleToGo);
					}
					closeRCM();						
				}
				
				if(typeBrigade==typeBrig.Scout)
				{
					Rect meubleColon=new Rect(destPos.x+5, Screen.height-destPos.y+50, 90, 20);
					if (GUI.Button(meubleColon,"Colonize"))
					{
						if (meubleToGo!=null)
						{
							isColonizing=true;	
							moveToMeuble(meubleToGo);
						}
						closeRCM();
					}
				}				
			}
			else
			{
				if(destructCap==0)
					GUI.Box (meubleAct, "Locked");
				else
				{	
					if (GUI.Button(meubleAct,"Open a way"))
					{
						moveToMeuble(meubleToGo);
					}				
				}
			}			
		}	
		else if(clickPiege)
		{
			Rect meubleAct=new Rect (destPos.x+5, Screen.height-destPos.y+20, 90, 20);			
			if (typeBrigade==typeBrig.Sabot)
			{
				if (GUI.Button(meubleAct,"Sweep"))
				{
					if (piege!=null)
						moveToDest(piege.transform.position);
					closeRCM();						
				}
			}
			else
			{
				if (GUI.Button(meubleAct,"Go"))
				{
					if (piege!=null)
						moveToDest(piege.transform.position);
					closeRCM();						
				}
			}
		}
		else
		{
			Rect moveBut=new Rect (destPos.x+5, Screen.height-destPos.y+20, 90, 20);
			if (GUI.Button(moveBut,"Move here"))
			{
				moveToDest(mousePos);
				closeRCM();		
			}	
		}
	}
	
	void closeRCM()
	{
		rightClickMenu=false;
		clickMeuble=false;
		clickPiege=false;
		piege=null;
	}

	string listeInfo()
	{
		string _info="Type: "+typeBrigade+"\n";
		_info+="Life: "+life+"/"+maxLife+"\n";
		_info+="Speed: "+speed+"\n";
		_info+="Food taking: "+foodTaking+"\n";
		_info+="Water taking: "+waterTaking+"\n";
		_info+="Construct taking: "+constructTaking+"\n";
		_info+="Food consum: "+foodConsum+"\n";
		_info+="Water consum: "+waterConsum+"\n";
		_info+="Destruct cap: "+destructCap+"\n";
		_info+="Speed sweep: "+speedSweep+"\n";
		_info+="Résist: Cold:"+coldResist+" Hot:"+hotResist+"\n";
		_info+="Pois:"+poisonResist+" Phys:"+physicResist+"\n";
		return _info;
	}

	void DrawInfo()
	{
		Rect infoRect= new Rect (0,100, 150, 200);
		GUI.Box(infoRect,listeInfo());
	}


	public float readMaxLife()
	{
		return maxLife;
	}
}
