using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class ObjectScript : MonoBehaviour 
{
	public bool isColonie=false;

	public float water=0f; //eau contenu dans l'objet
	public float food=0f; //nourriture contenu dans l'objet
	public float construct=0f; //éléments de construction contenu dans l'objet

	float waterInit;
	float foodInit;
	float constructInit;
	
	public float hiddenness=0f; //facilité à se cacher (inutilisé pour le moment)
	public int solidityLevel=0; //solidité
	public float infectedLevel=0f; //taux d'infection (inuilisé poru le moment)
	public int maxBrig=10;	//nombre de brigade maximum
	public int maxPlacement=10; //nombre d'emplacement amximum poru les extensions (colonie)
	
	public typeMalus malus; 
	public typeBonus bonus;
	public float renouvDelay=60f;


	
	//les extensions sont juste des stings stocké dans un tableau
	int nbPlacement=0;
	string[] placement;

	//variables gérant le minage des ressources
	float speedWater=0f;
	float speedFood=0f;
	float speedConstruct=0f;

	bool isInfected=false; //l'objet est infecté

	//les brigades à l'intérieur sont stockés dans un tableau
	int nbBrigade=0; 
	BrigadeController[] Brigs;

	//gestion du rendu
	SpriteRenderer sprite;
	Color initColor;
	Color infectedColor;


	LevelController lvlC;

	Vector3 objectPos;

	//Point de sortie (child)
	[HideInInspector] public Transform spawnPoint;

	//pour l'affichage des popups,etc...
	bool PopUp=false;
	bool isOpening=false;
	float _openingPercent=100;
	float _openingLvl=0;

	//niveau de la nursery pour les colonies
	int levelNurs=0;
	
	
	void Start()
	{
		GameObject levelControllerObject = GameObject.FindWithTag ("LevelController");
		if (levelControllerObject != null)
			lvlC = levelControllerObject.GetComponent <LevelController>();
		else
			Debug.Log ("Cannot find 'LevelController' script");
			
			
		spawnPoint=transform.GetChild(0);
		
		Brigs=new BrigadeController[maxBrig];
		sprite=GetComponent<SpriteRenderer>();
		initColor=sprite.color;
		infectedColor=new Vector4 (initColor.r, initColor.g-0.5f, initColor.b-0.5f, 1f);
		
		if (isColonie)
		{
			colonize();
		}

		if(bonus==typeBonus.InfiniteWater || bonus==typeBonus.InfiniteFoodAndWater || bonus==typeBonus.InfiniteAll)
			water=9999f;
		if(bonus==typeBonus.InfiniteFood || bonus==typeBonus.InfiniteFoodAndWater || bonus==typeBonus.InfiniteAll)
			food=9999f;
		if(bonus==typeBonus.InfiniteConstruct || bonus==typeBonus.InfiniteAll)
			construct=9999f;
	
		waterInit=water;
		foodInit=food;
		constructInit=construct;

		if(bonus!=typeBonus.None && bonus!=typeBonus.InfiniteWater && bonus!=typeBonus.InfiniteFood 
					&& bonus!=typeBonus.InfiniteConstruct && bonus!=typeBonus.InfiniteFoodAndWater && bonus!=typeBonus.InfiniteAll)
			Invoke("renouvel",renouvDelay);

		textInfo();//affichage des infos
	}
	
	void FixedUpdate()
	{
		if (PopUp && Input.GetMouseButtonUp(0))
		{
			Invoke ("closePopUp",0.1f);
		}
	}

	void renouvel()
	{
		if(bonus==typeBonus.NewWater || bonus==typeBonus.NewFoodAndWater || bonus==typeBonus.NewAll)
			water=waterInit;
		if(bonus==typeBonus.NewFood || bonus==typeBonus.NewFoodAndWater || bonus==typeBonus.NewAll)
			food=foodInit;
		if(bonus==typeBonus.newConstruct || bonus==typeBonus.NewAll)
			construct=constructInit;

		if(isColonie)
			consumeAll();
		Invoke("renouvel",renouvDelay);
	}

	void takeWater()
	{
		if (solidityLevel==0 && water>0)
		{
			if(lvlC.AddWater(speedWater))
			{
				if(bonus!=typeBonus.InfiniteWater && bonus!=typeBonus.InfiniteFoodAndWater && bonus!=typeBonus.InfiniteAll)
					water-=speedWater;
			}
		}		
		if (isInfected && !IsInvoking("takeWater"))
			Invoke("takeWater", ListConst.takeDelay);	
	}
	
	void takeFood()
	{
		if(solidityLevel==0 && food>0)
		{
			if(lvlC.AddFood(speedFood))
			{
				if(bonus!=typeBonus.InfiniteFood && bonus!=typeBonus.InfiniteFoodAndWater && bonus!=typeBonus.InfiniteAll)
					food-=speedFood;
			}
				
		}
		if (isInfected && !IsInvoking("takeFood"))
			Invoke("takeFood", ListConst.takeDelay);
		
	}
	
	void takeConstruct()
	{
		if(solidityLevel==0 && construct>0)
		{
			if (lvlC.AddConstruct(speedConstruct))
			{
				if(bonus!=typeBonus.InfiniteFood && bonus!=typeBonus.InfiniteFoodAndWater && bonus!=typeBonus.InfiniteAll)
					construct-=speedConstruct;
			}			
		}
		if (isInfected && !IsInvoking("takeConstruct"))
			Invoke("takeConstruct", ListConst.takeDelay);
	}
	
	void colonize()
	{
		isColonie=true;
		lvlC.AddLimitBrigade();
		infectedLevel=10;//unused
		isInfected=true;
		sprite.color=infectedColor;
		consumeAll();
		placement=new string[maxPlacement];
		lvlC.AddContamin();
	}

	void consumeAll()
	{
		if(lvlC.AddWater(water))
			water=0;
		if(lvlC.AddFood(food))
			food=0;
		if (lvlC.AddConstruct(construct))
			construct=0;
	}
		
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other is CircleCollider2D && other.CompareTag("Brigade"))
		{
			BrigadeController brigade=other.GetComponent<BrigadeController>();

			//si la brigade n'est pas à destination de ce meuble, elle ne doit pas y rentrer
			if(!brigade.goToMeuble || brigade.meubleToGo!=this)
			{
				brigade.underObjectIn(this);
				return;
			}

			//les brigades ne peuvent pas rentrer dans les meubles solides si elles n'ont pas de capacité de destruction)
			//les brigades ne peuvent pas renrter dans la colonie pour l'instant. 
			if(isColonie || (brigade.destructCap==0 && solidityLevel>0))
				brigade.moveToDest(spawnPoint.position);
			//brigade colon
			else if(brigade.typeBrigade==typeBrig.Scout && brigade.isColonizing)
			{
				colonize ();
				brigade.Hurt (1000f);//meurt
			}
			else
			{
				//les brigades pouvant "ouvrir" les meubles "solides"
				if(brigade.destructCap>0 && solidityLevel>0)
				{
					opening(brigade.destructCap);
				}
				if (nbBrigade<maxBrig)
				{
					//brigade entre, ajoute sa vitesse de collecte
					Brigs[nbBrigade]=brigade;
					speedWater+=brigade.waterTaking;
					speedFood+=brigade.foodTaking;
					speedConstruct+=brigade.constructTaking;
					brigade.inObject(this);//la brigade sait dans quel objet elle est
					Vector3 _pos=new Vector3(transform.position.x, transform.position.y, brigade.transform.position.z);//pour que la brigade reste derrière le meuble
					brigade.transform.position=_pos;
					nbBrigade++;
				}
				else
					brigade.moveToDest(spawnPoint.position);
				if (!isInfected)
				{
					isInfected=true;
					lvlC.AddContamin();
					sprite.color=infectedColor;
					//A la première contamination, on déclenche la prise de ressource
					Invoke("takeWater", ListConst.takeDelay);
					Invoke ("takeFood", ListConst.takeDelay);
					Invoke ("takeConstruct", ListConst.takeDelay);
				}
			}
		}		
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if(other is CircleCollider2D && other.CompareTag("Brigade"))
		{
			BrigadeController brigade=other.GetComponent<BrigadeController>();

			brigade.underObjectOut();
	
		}

	}
	
	
	void OnMouseUp()
	{
		PopUp = true;
	}
	
	void OnGUI()
	{
		if (isColonie)		
			GUI.DrawTexture(new Rect (objectPos.x-15, Screen.height-objectPos.y-15, 30, 30),lvlC.iconColonie);	
		
		if (solidityLevel>0)
		{
			for(int i=0;i<solidityLevel;i++)
			{
				GUI.DrawTexture(new Rect (objectPos.x-10+10*i, Screen.height-objectPos.y-10, 20, 20),lvlC.iconLock);	
			}
		}
	
			
		objectPos=Camera.main.WorldToScreenPoint(transform.position);
		DrawInfo();
		
		if(isOpening)
			DrawOpening();
		
	}
	
	void DrawInfo()
	{
		//gestion de la hauteur de la fenêtre d'affichage PopUp
		float rectHeight;
		if (isColonie)
			rectHeight=350;
		else
			rectHeight=nbBrigade*30+100;
		Rect rect = new Rect (objectPos.x,Screen.height-objectPos.y, 200, rectHeight );
		Rect close = new Rect (objectPos.x+180,Screen.height-objectPos.y,20,20);
		
		if (PopUp)
		{
			GUI.Box(rect, textInfo());
			if (GUI.Button(close,"X"))
			{
				closePopUp();
			}
			if (!isColonie && nbBrigade>0)
			{
				//Boutons pour sortir les brigades
				for (int i=0;i<nbBrigade;i++)
				{
					Rect goOut=new Rect (objectPos.x+10, Screen.height-objectPos.y+90+i*30, 180, 30);
					if (GUI.Button(goOut,"Brigade "+Brigs[i].typeBrigade+"("+Brigs[i].life+"/"+Brigs[i].readMaxLife()+") out"))
					{
						outBrig(i);
						closePopUp();
					}
				}
			}
			else if (isColonie)
			{
				Rect newRecolt=new Rect (objectPos.x+10, Screen.height-objectPos.y+60+nbPlacement*20, 180, 30);
				if (GUI.Button(newRecolt,"Haervester (W:"+lvlC.waterRecolt+",F:"+lvlC.foodRecolt+")"))
				{
					lvlC.AddBrigade(typeBrig.Recolt, spawnPoint.position);
					closePopUp();			
				}
				Rect newStrong=new Rect (objectPos.x+10, Screen.height-objectPos.y+100+nbPlacement*20, 180, 30);
				if (GUI.Button(newStrong,"Strong (W:"+lvlC.waterStrong+",F:"+lvlC.foodStrong+")"))
				{
					lvlC.AddBrigade(typeBrig.Strong, spawnPoint.position);
					closePopUp();			
				}
	
				GUI.enabled=(levelNurs>0);
				Rect newScout=new Rect (objectPos.x+10, Screen.height-objectPos.y+140+nbPlacement*20, 180, 30);
				if (GUI.Button(newScout,"Scout (W:"+lvlC.waterScout+",F:"+lvlC.foodScout+")"))
				{
					lvlC.AddBrigade(typeBrig.Scout, spawnPoint.position);
					closePopUp();			
				}
				Rect newSabot=new Rect (objectPos.x+10, Screen.height-objectPos.y+180+nbPlacement*20, 180, 30);
				if (GUI.Button(newSabot,"Saboteur (W:"+lvlC.waterSabot+",F:"+lvlC.foodSabot+")"))
				{
					lvlC.AddBrigade(typeBrig.Sabot, spawnPoint.position);
					closePopUp();			
				}

				GUI.enabled=lvlC.VerifStockage();
				Rect newStock=new Rect (objectPos.x+10, Screen.height-objectPos.y+230+nbPlacement*20, 180, 30);
				if (GUI.Button(newStock,"Create Stockage ("+lvlC.constrStockage+"c)"))
				{
					if (nbPlacement<maxPlacement)
					{
						lvlC.AddStockage();
						placement[nbPlacement]="Stockage";
						nbPlacement++;
					}
					closePopUp();			
				}

				GUI.enabled=(levelNurs==0 && lvlC.VerifNursery1());
				Rect newCouv=new Rect (objectPos.x+10, Screen.height-objectPos.y+270+nbPlacement*20, 180, 30);
				if (GUI.Button(newCouv,"Create Nursery 1 ("+lvlC.constrNursery1+"c)"))
				{
					if (nbPlacement<maxPlacement)
					{
						lvlC.AddNursery1();
						placement[nbPlacement]="Nursery 1";
						nbPlacement++;
						levelNurs=1;
					}
					closePopUp();			
				}

				GUI.enabled=(lvlC.VerifBatAmel());
				Rect newBatAmel=new Rect (objectPos.x+10, Screen.height-objectPos.y+310+nbPlacement*20, 180, 30);
				if (GUI.Button(newBatAmel,"Create Training Center ("+lvlC.constrBatAmel+"c)"))
				{
					if (nbPlacement<maxPlacement)
					{
						lvlC.AddBatAmel();
						placement[nbPlacement]="BatAmel";
						nbPlacement++;
					}
					closePopUp();			
				}
				GUI.enabled=true;
			}
		}

	}
	
	
	void closePopUp()
	{
		PopUp=false;
	}

	/// <summary>
	/// enlever la brigade situé à l'indice i du tableau
	/// </summary>
	/// <param name="i">indice.</param>
	void outBrig(int i)
	{
		if(nbBrigade>0 && i<nbBrigade)
		{	
			sortirBrig(Brigs[i]);
			speedWater-=Brigs[i].waterTaking;
			speedFood-=Brigs[i].foodTaking;
			speedConstruct-=Brigs[i].constructTaking;
			for (int j=i;j<nbBrigade-1;j++)
			{
				Brigs[j]=Brigs[j+1];
			}
			Brigs[nbBrigade-1]=null;
			nbBrigade--;
		}
		if (nbBrigade==0)
		{
			isInfected=false;
			sprite.color=initColor;
			lvlC.RemoveContamin();
		}
	}

	/// <summary>
	/// Sortir la brigade
	/// </summary>
	/// <param name="brig">Brigade.</param>
	void sortirBrig(BrigadeController brig)
	{
		Vector3 _pos=new Vector3(transform.position.x, transform.position.y, brig.transform.position.z);//pour que la brigade reste derrière le meuble
		brig.transform.position=_pos;			
		brig.outObject();
		Vector3 _spawn=new Vector3(spawnPoint.position.x, spawnPoint.position.y, 1);//les meuble sont un z=0 pour rester devant, donc les briagades spawn en z=1;
		brig.moveToDest(_spawn);
		brig.select();
	}

	/// <summary>
	/// Tuer la brigade
	/// </summary>
	/// <param name="brig">Brigade.</param>
	public void killBrig(BrigadeController brig)
	{
		int i=0;
		while (Brigs[i]!=brig)
			i++;
		outBrig(i);
	}

	/// <summary>
	/// Ouvrir l'objet avec le nivea ude destruction d'une brigade
	/// </summary>
	/// <param name="levelDes">niveau de destruction</param>
	public void opening (float levelDes)
	{
		isOpening=true;
		_openingLvl+=levelDes;
		if(!IsInvoking("open"))
			open ();		
	}
	
	void open()
	{
		_openingPercent-=_openingLvl/solidityLevel*10;
		if (_openingPercent<=0)
		{
			isOpening=false;
			solidityLevel=0;
		}
		if (isOpening && ! IsInvoking("open"))
			Invoke ("open",ListConst.openDelay);
	}
	
	void DrawOpening()
	{
		Rect rect=new Rect (objectPos.x-25, Screen.height-objectPos.y-10,_openingPercent, 20);
		GUI.Box(rect, "opening");
	}

	/// <summary>
	/// Obtenri les infos de l'objet
	/// </summary>
	/// <returns>informations.</returns>
	public string textInfo()
	{
		string Info="";
		if (!isColonie)
		{	
			Info="Locked Level:"+solidityLevel+"\n";
			Info+="Bonus:"+bonus+"\n";
			Info+="Malus:"+malus+"\n";
			Info+="Brigade In: "+nbBrigade+"/"+maxBrig+"\n";
			Info+="W:"+water+" ";
			Info+="F:"+food+" ";
			Info+="C:"+construct+" ";
		}
		else
		{
			Info="Colonie ";
			Info+="Emp: "+nbPlacement+"/"+maxPlacement+"\n";
			Info+="Bonus:"+bonus+"\n";
			Info+="Malus:"+malus+"\n";
			for (int i=0;i<nbPlacement;i++)
			{
				Info+=placement[i]+"\n";
			}			
		}
		return Info;
	}

	
	
}
