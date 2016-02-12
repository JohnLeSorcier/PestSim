using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//Malus des objets (
public enum typeMalus
{
	None, Cold, VeryCold, Hot, VeryHot, Dangerous
}

//Bonus des objets 
public enum typeBonus
{
	None, NewWater, InfiniteWater, NewFood, InfiniteFood, newConstruct, InfiniteConstruct, NewFoodAndWater, NewAll, InfiniteFoodAndWater, InfiniteAll
}

//constante
public class ListConst
{
	public const float basicDamageMalus=5f;
	public const float DamageDelay=10f;
	public const float consumDelay=3f;
	public const float poisonDelay=1f;
	public const float takeDelay=5f;
	public const float openDelay=1f;
	public const float contaminDelay=5f;
	public const float sweepDelay=1f;
}

public class LevelController : MonoBehaviour {

	public GameObject Recolt;
	public GameObject Strong;
	public GameObject Scout;
	public GameObject Sabot;
	
	public Texture2D iconLock;
	public Texture2D iconColonie;	

	[HideInInspector] public float waterStock=0;
	[HideInInspector] public float foodStock=0;
	[HideInInspector] public float constructStock=0;
	[HideInInspector] public float chemStock=0;
	//Pour le moment, les limites sont directement dans el LevelController
	//A terme, ils seront directement lié aux colonies
	//(comme l'extension stockage)
	public float limitWater=50;
	public float limitFood=50;
	public float limitConstruct=50;
	public float limitChem=30;
	public float limitBrigade=10;
	[HideInInspector]public float nbbrigade=0;

	public Text waterText;
	public Text foodText;
	public Text constructText;
	public Text chemText;
	public Text brigadeText;
	public Text meubleText;
	public Text timeText;

	GameObject[] meubles;//liste des objets
	float nbMeubles=0;
	float nbMeublesContamin=0;
	float percentContamin=0;

	public float factContamin=1;//vitesse de contamination
	
	float detect=0;
	public float maxDetect=100;//limite max de la détection globale
	
	bool goodEnd=false;
	bool badEnd=false;
	
	public GUISkin skin;
	
	public Texture2D selectHightLight;
	public static Rect selectionRect=new Rect (0,0,0,0);
	private Vector3 startClick=-Vector3.one;

	//niveaux des brigades
	[HideInInspector] public int levelRecolt=1;
	[HideInInspector] public int levelStrong=1;
	[HideInInspector] public int levelScout=1;
	[HideInInspector] public int levelSabot=1;

	//A remplacer avec un fichier de configuration, etc.
	public float ChemRecoltImpr=1; //chimie nécessaire pour améliorer récolteurs
	public float ChemStrongImpr=1;//chimie nécessaire pour améliorer casseurs
	public float ChemScoutImpr=1;//chimie nécessaire pour améliorer exploreurs
	public float ChemSabotImpr=1;//chimie nécessaire pour améliorer saboteurs

	//A remplacer avec un fichier de configuration, etc.
	public float waterRecolt=1f;//eau pour créer des récolteurs
	public float foodRecolt=1f;//food pour créer des récolteurs
	public float waterStrong=1f;//etc
	public float foodStrong=2f;
	public float waterScout=2f;
	public float foodScout=2f;
	public float waterSabot=2f;
	public float foodSabot=3f;


	public float constrStockage=10;//construct nécessaire pour stockage
	public float constrNursery1=10;//construct nécessaire pour nursery 1
	public float constrBatAmel=30;//construct nécessaire pour Améliorateur

	public Button AmelButton;
	[HideInInspector] public bool amelDisp=false;

	float startTime;
	public float timerTime=600f;
	[HideInInspector] public float timeRemain;//pour la save
	[HideInInspector] public float _time;


	void Start () 
	{
		if (meubles==null)
			meubles=GameObject.FindGameObjectsWithTag("Meuble");
		nbMeubles=meubles.Length;
		startTime=Time.time;
		timeRemain=timerTime;
		_time=timeRemain;
	}

	void Update () 
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			Quit();
		}

		CheckSelection();
		CheckGameOver();
		_time=timeRemain-(Time.time-startTime);
		if (_time<=0)
			badEnd=true;
		MaJText();
	}

	void CheckGameOver ()
	{
		if ((foodStock<1 || waterStock<1) && nbbrigade==0)
			badEnd=true;
	}
	
	void CheckSelection()
	{
		if(Input.GetMouseButtonDown(0))
			startClick=Input.mousePosition;
		else if (Input.GetMouseButtonUp(0))
		{
			startClick=-Vector3.one;
		}
		if (Input.GetMouseButton(0))
		{
			selectionRect=new Rect (startClick.x, InvertMouseY (startClick.y),Input.mousePosition.x-startClick.x, InvertMouseY(Input.mousePosition.y)-InvertMouseY(startClick.y));
			if(selectionRect.width<0)
			{
				selectionRect.x+=selectionRect.width;
				selectionRect.width= -selectionRect.width;
			}
			if(selectionRect.height<0)
			{
				selectionRect.y+=selectionRect.height;
				selectionRect.height= -selectionRect.height;
			}
		}
	}
	
	public static float InvertMouseY(float y)
	{
		return Screen.height-y;
	}
	
	public void Quit()
	{
		Application.Quit();
	}
	
	public void AddContamin()
	{
		nbMeublesContamin++;
		if (!IsInvoking("contamPerCent"))
			Invoke("contamPerCent",ListConst.contaminDelay);
		
	}
	
	public void RemoveContamin()
	{
		nbMeublesContamin--;
	}
	
	void contamPerCent()
	{
		percentContamin+=(nbMeublesContamin/nbMeubles)*factContamin;
		if(percentContamin<100)
			Invoke ("contamPerCent",ListConst.contaminDelay);
		else if (nbMeublesContamin/nbMeubles>0.75f)			
			goodEnd=true;
	}
	
	void DrawEnd()
	{
		Rect rect = new Rect (Screen.width/2-100,Screen.height/2-100, 200, 200);
		Rect close = new Rect (Screen.width/2-100+180,Screen.height/2-100,20,20);
		string Info="";
		if (goodEnd)
		{
			Info="Well Done\n";
			Info+="You take "+nbMeublesContamin/nbMeubles*100+"% of objects\n";
			Info+="You contaminate "+percentContamin+"% of batiment\n";
		}
		if (badEnd)
		{
			Info="Game Over\n";
			Info+="You take "+nbMeublesContamin/nbMeubles*100+"% of objects\n";
			Info+="You contaminate "+percentContamin+"% of batiment\n";
		}
		
		GUI.Box(rect, Info);
		if (GUI.Button(close,"X"))
		{
			goodEnd=false;
			badEnd=false;
		}
	}
	
	void OnGUI()
	{
	
		DrawContamin();
		DrawDetect();
		DrawSelect();

		AmelButton.interactable=amelDisp;
		
		if(goodEnd || badEnd)
		{
			DrawEnd();
		}
	}
	
	void DrawSelect()
	{
		if(startClick!=-Vector3.one)
		{
			GUI.color=new Color(1,1,1,0.5f);
			GUI.DrawTexture(selectionRect,selectHightLight);
		}
	}
	
	void DrawContamin()
	{
		Rect rect = new Rect (700,5, 200, 20);
		Rect ContaminRect=new Rect (700,5,(percentContamin/100)*200, 20);
		
		if (percentContamin>0)
			GUI.Box(ContaminRect, "", skin.box);
		GUI.Box(rect, "Contamination: "+percentContamin+"%");
	}
	
	void DrawDetect()
	{
		Rect rect = new Rect (700,30, 200, 20);
		Rect detectRect=new Rect (700,30,detect/maxDetect*200, 20);
		
		if (detect>0)
			GUI.Box(detectRect, "", skin.box);
		GUI.Box(rect, "Detection: "+detect/maxDetect*100+"%");
	}

	public bool VerifStockage()
	{
		return (constructStock>=constrStockage);
	}
		
	public void AddStockage()
	{
		if(constructStock>=constrStockage)
		{
			AddLimitWater(20);	
			AddLimitFood(20);
			AddLimitConstruct(20);
			RemoveConstruct(constrStockage);
		}
	}

	public bool VerifNursery1()
	{
		return (constructStock>=constrNursery1);
	}

	public void AddNursery1()
	{
		if(constructStock>=constrNursery1)
		{
			RemoveConstruct(constrNursery1);

		}
	}

	public bool VerifBatAmel()
	{
		return (constructStock>=constrBatAmel);
	}

	public void AddBatAmel()
	{
		if(constructStock>=constrBatAmel)
		{
			amelDisp=true;
			RemoveConstruct(constrBatAmel);
		}
	}
	
	void AddLimitWater(float nb)
	{
		limitWater+=nb;
	}
	
	void AddLimitFood(float nb)
	{
		limitFood+=nb;
	}
	
	void AddLimitConstruct(float nb)
	{
		limitConstruct+=nb;
		MaJText();
	}
	
	void RemoveLimitWater(float nb)
	{
		limitWater-=nb;
	}
	
	void RemoveLimitFood(float nb)
	{
		limitFood-=nb;
	}
	
	void RemoveLimitCosntruct(float nb)
	{
		limitConstruct-=nb;
	}
	
	public bool AddWater(float nb)
	{
		if(waterStock>=limitWater)
			return false;
		waterStock+=nb;
		if(waterStock>limitWater)
			waterStock=limitWater;
		return true;
	}
	
	
	public bool AddFood(float nb)
	{
		if(foodStock>=limitFood)
			return false;
		foodStock+=nb;
		if (foodStock>limitFood)
			foodStock=limitFood;
		return true;
	}
	
	public bool AddConstruct(float nb)
	{
		if(constructStock>=limitConstruct)
			return false;
		constructStock+=nb;
		if(constructStock>limitConstruct)
			constructStock=limitConstruct;
		return true;
	}
	
	public bool AddChem(float nb)
	{
		if (chemStock>=limitChem)
			return false;
		chemStock+=nb;
		if (chemStock>limitChem)
			chemStock=limitChem;
		return true;
	}
	
	public bool RemoveWater(float nb)
	{
		if(nb>waterStock)
			return false;
		waterStock-=nb;
		return true;
	}
	
	public bool RemoveFood(float nb)
	{
		if(nb>foodStock)
			return false;
		foodStock-=nb;
		return true;
	}
	
	public bool RemoveConstruct(float nb)
	{
		if(nb>constructStock)
			return false;
		constructStock-=nb;
		return true;
	}
	
	public bool RemoveChem(float nb)
	{
		if(nb>chemStock)
			return false;
		chemStock-=nb;
		return true;
	}
	
	public void AddDetect(float nb)
	{
		detect+=nb;
	}
	
	public void MaJText()
	{
		waterText.text=""+waterStock+"/"+limitWater;
		foodText.text=""+foodStock+"/"+limitFood;
		constructText.text=""+constructStock+"/"+limitConstruct;
		brigadeText.text=""+nbbrigade+"/"+limitBrigade+ " units";	
		meubleText.text=""+nbMeublesContamin+"/"+nbMeubles+" obj";
		chemText.text=""+chemStock+"/"+limitChem;

		int _min=0;
		int _sec=0;
		if(_time>0)
		{
			_min=Mathf.FloorToInt(_time/60);
			_sec=Mathf.FloorToInt(_time%60);
		}
		string compSec; //afiche le 0 si seconde sur un seul chiffre.
		if(_sec<10)
			compSec="0";
		else
			compSec="";
		timeText.text=_min+"\'"+compSec+_sec+"\"";
	}
	
	public BrigadeController AddBrigade(typeBrig type, Vector3 spawnPoint)
	{
		if (nbbrigade<limitBrigade)
		{
			Vector3 _spawn=new Vector3(spawnPoint.x, spawnPoint.y, 1);//les meuble sont un z=0 pour rester devant, donc les briagades spawn en z=1;
			GameObject brig=null;
			if (type==typeBrig.Recolt && waterStock>=waterRecolt && foodStock>=foodRecolt)
			{
				brig=(GameObject)Instantiate(Recolt, _spawn, Quaternion.identity);
				RemoveWater(waterRecolt);
				RemoveFood(foodRecolt);
				nbbrigade++;
			}
			else if (type==typeBrig.Strong && waterStock>=waterStrong && foodStock>=foodStrong)
			{
				brig=(GameObject)Instantiate(Strong, _spawn, Quaternion.identity);
				RemoveWater(waterStrong);
				RemoveFood(foodStrong);
				nbbrigade++;
			}
			else if (type==typeBrig.Scout && waterStock>=waterScout && foodStock>=foodScout)		
			{
				brig=(GameObject)Instantiate(Scout, _spawn, Quaternion.identity);	
				RemoveWater(waterScout);
				RemoveFood(foodScout);
				nbbrigade++;
			}
			else if (type==typeBrig.Sabot && waterStock>=waterSabot && foodStock>=foodSabot)	
			{
				brig=(GameObject)Instantiate(Sabot, _spawn, Quaternion.identity);
				RemoveWater(waterSabot);
				RemoveFood(foodSabot);
				nbbrigade++;
			}

			if(brig!=null)
			{
				BrigadeController brigade=brig.GetComponent<BrigadeController>();
				brigade.select();
				return brigade;	
			}
			else
				return null;
		}
		else
			return null;
	}
	
	public void RemoveBrigade()
	{
		nbbrigade--;
	}

	public void ImproveLevel(typeBrig type)
	{
		if(type==typeBrig.Recolt && levelRecolt<4  && chemStock>=ChemRecoltImpr)
		{
			levelRecolt++;
			chemStock-=ChemRecoltImpr;
		}
		if(type==typeBrig.Strong && levelStrong<4 && chemStock>=ChemStrongImpr)
		{
			levelStrong++;
			chemStock-=ChemStrongImpr;
		}
		if(type==typeBrig.Scout && levelScout<4 && chemStock>=ChemScoutImpr)
		{
			levelScout++;
			chemStock-=ChemScoutImpr;
		}
		if(type==typeBrig.Sabot && levelSabot<4 && chemStock>=ChemSabotImpr)
		{
			levelSabot++;
			chemStock-=ChemSabotImpr;
		}
	}

	public int levelBrig(typeBrig type)
	{
		if(type==typeBrig.Recolt)
			return levelRecolt;
		else if(type==typeBrig.Strong)
			return levelStrong;
		else if(type==typeBrig.Scout)
			return levelScout;
		else if(type==typeBrig.Sabot)
			return levelSabot;
		else
			return 0;
	}

	public bool checkRessourceForImprove(typeBrig type)
	{
		if(type==typeBrig.Recolt && chemStock>=ChemRecoltImpr)
			return true;
		else if (type==typeBrig.Strong && chemStock>=ChemStrongImpr)
			return true;
		else if (type==typeBrig.Scout && chemStock>=ChemScoutImpr)
			return true;
		else if (type==typeBrig.Sabot && chemStock>=ChemSabotImpr)
			return true;
		else
			return false;
	}

	public float ressourceForImprove (typeBrig type)
	{

		if(type==typeBrig.Recolt)
			return ChemRecoltImpr;
		else if (type==typeBrig.Strong)
			return ChemStrongImpr;
		else if (type==typeBrig.Scout)
			return ChemScoutImpr;
		else if (type==typeBrig.Sabot)
			return ChemSabotImpr;
		else
			return 999f;
	}

	public void AddLimitBrigade()
	{
		limitBrigade+=10;
	}

}
