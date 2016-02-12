using UnityEngine;
using System.Collections;

public class DetectScript : MonoBehaviour {

	HumanController human;
	BrigadeController brigade;
	bool hasDetected=false;
	
	void Start () 
	{
		human=GetComponentInParent<HumanController>();
	}
	
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other is CircleCollider2D && other.CompareTag("Brigade"))
		{
			brigade=other.GetComponent<BrigadeController>();
			if(!hasDetected) 
			{
				if(!brigade.isInObject && !brigade.underMeuble)
					human.detectBrig(brigade);
				else
					human.detectBrigInObject(brigade);

				hasDetected=true;
				StartCoroutine(ReDetect());
			}
		}
	}

	/// <summary>
	/// une fois détecté, on attend 3 secondes avant de redétecter un autre
	/// </summary>
	IEnumerator ReDetect()
	{
		yield return new WaitForSeconds(3f);
		hasDetected=false;
	}

}
