using UnityEngine;
using System.Collections;

public class IconScript : MonoBehaviour 
{

	SpriteRenderer sprite;
	
	void Start()
	{
		sprite=GetComponent<SpriteRenderer>();
	}	
		
	void Update () 
	{
		transform.rotation=Quaternion.identity;		
		transform.localPosition=new Vector3 ();
	}
	public void Disappear()
	{
		sprite.enabled=false;
	}
	
	public void Appear()
	{
		sprite.enabled=true;
	}
}
