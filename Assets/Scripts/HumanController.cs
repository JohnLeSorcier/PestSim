using UnityEngine;
using System.Collections;

public class HumanController : MonoBehaviour {

	
	public float speed=1f; //vitesse de déplacement
	//ASSOCIER LES POINTS AVEC UNE DUREE ET UNE ORIENTATION?
	public Transform[] points; //tableau de point de passage
	int nbPoints;
	int iPoint;//point en cours
	Vector3 target;//destination

	float detectLevel=0; //niveau de détection
	public int maxDetectLevel=10; //detection maximum avant piège

	public float fear=1; //peut face aux bestioles (0=aucune réaction)

	public float force=5f; //force pour écraser
	
	Vector3 objectPos;
	
	public Transform Poison;
	
	public GUISkin skin;
	
	LevelController lvlC;
	
	MoveController move;
	public float pointDist=0.05f;//distance de controle par rapport à la destination (poru le pathfinding)

	//en chasse d'une brigade
	BrigadeController brigChase;
	bool isChasing=false;
	Vector3 destChase;
	bool hasCrushed=false;//brigade écrasée
	
	
	void Start () 
	{

		GameObject levelControllerObject = GameObject.FindWithTag ("LevelController");
		if (levelControllerObject != null)
			lvlC = levelControllerObject.GetComponent <LevelController>();
		else
			Debug.Log ("Cannot find 'LevelController' script");

		move=GetComponent<MoveController>();

		//initialisaiton du chemin.
		if(points!=null)
		{
			nbPoints=points.Length;
			target=points[0].position;
			iPoint=1;		
			goTo(target);
		}
		
	}
	
	void Update () 
	{
		//quand on approche de la destination, on choisi la suivante
		if(Vector3.Distance(transform.position,target)<pointDist)
		{
			target=points[iPoint].position;
			iPoint++;
			if (iPoint==nbPoints)
				iPoint=0;
			goTo(target);
		}

		//en chasse, on se dirige vers la brigade repérée
		if (isChasing && brigChase!=null)
		{
			//une fois atteint, la chasse se termine et il rejoint son chemin
			if(Vector3.Distance(transform.position,brigChase.transform.position)<pointDist && !brigChase.isInObject)
			{
				isChasing=false;
				goTo(target);
			}
			//si il atteint la précédente destination de la brigade, il la poursuit à la nuvelle
			else if(Vector3.Distance(transform.position,destChase)<pointDist && !brigChase.isInObject)
			{
				destChase=brigChase.transform.position;
				goTo(destChase);
			}
			else if (brigChase.isInObject)
			{
				isChasing=false;
				goTo(target);
			}
		}
		else if  (isChasing && brigChase==null)
		{
			isChasing=false;
			goTo(target);
		}
	}	

	/// <summary>
	/// Aller à la destination
	/// </summary>
	/// <param name="dest">Destination.</param>
	public void goTo(Vector3 dest)
	{
		move.moveToDest(dest);
	}

	/// <summary>
	/// Detection d'une brigade
	/// </summary>
	/// <param name="brig">Brigade.</param>
	public void detectBrig(BrigadeController brig)
	{
			brigChase=brig;
			destChase=brigChase.transform.position;
			isChasing=true;
			goTo(destChase);

			addDetection(fear);

	}

	/// <summary>
	/// Detects the brigade in object.
	/// </summary>
	/// <param name="brig">Brigade.</param>
	public void detectBrigInObject(BrigadeController brig)
	{
		ObjectScript meuble;
		if(brig.isInObject)
		{
			meuble=brig.objectIn;
			float detectRand=100*Random.value;
			if(detectRand>meuble.hiddenness)
				addDetection(fear);
		}
	}

	void addDetection(float value)
	{
			detectLevel+=value;		
			lvlC.AddDetect(value);		
			if (detectLevel==maxDetectLevel)
				Invoke("actionDetectFull",1f);// le délai sert à éviter un bug de simultanéité
	}

	/// <summary>
	/// Action quand al barre de détection est pleine
	/// </summary>
	void actionDetectFull()
	{
		if (brigChase!=null)
			Instantiate(Poison, brigChase.transform.position, Quaternion.identity);
		detectLevel=0;
	}

	
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other is CircleCollider2D && other.CompareTag("Brigade") && isChasing && !hasCrushed)
		{
			BrigadeController brigade=other.GetComponent<BrigadeController>();
			if(!brigade.isInObject)
			{
				brigade.Hurt(force);
				hasCrushed=true;
				StartCoroutine(crushing());
			}
		}
	}

	/// <summary>
	/// une fois écraser, on n'écrase plus pendant 2 secondes
	/// </summary>
	IEnumerator crushing()
	{
		yield return new WaitForSeconds(2f);// le délait sert à éviter un bug de simultanéité
		hasCrushed=false;
		isChasing=false;
		goTo(target);
	}


	public void OnGUI()
	{
		objectPos=Camera.main.WorldToScreenPoint(transform.position);
		Rect rect = new Rect (objectPos.x-50,Screen.height-objectPos.y-50, 100, 20);
		Rect detectRect=new Rect (objectPos.x-50,Screen.height-objectPos.y-50, (100/maxDetectLevel)*detectLevel, 20);
		
		if (detectLevel>0 && detectLevel<=maxDetectLevel)
			GUI.Box(detectRect, "", skin.box);
		float detectperCent=(100/maxDetectLevel)*detectLevel;
		GUI.Box(rect, "Detection: "+detectperCent+"%");
	}
}
