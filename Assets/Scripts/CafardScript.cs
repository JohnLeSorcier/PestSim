using UnityEngine;
using System.Collections;
using Pathfinding;


public class CafardScript : MonoBehaviour 
{
	SpriteRenderer spriteR;
	Color initColor;
	Color secondColor;
	bool isSelected=false;
	public Sprite spriteWhite;
	Sprite initSprite;
	
	void Awake()
	{
		spriteR=GetComponent<SpriteRenderer>();
		initColor=spriteR.color;
		secondColor=new Vector4 (initColor.r, initColor.g, initColor.b, 0.8f);		
		initSprite=spriteR.sprite;
	}
	
	/// <summary>
	/// Change l'angle des sprites
	/// </summary>
	/// <param name="angle">Angle.</param>
	public void changeAngle(float angle)
	{
		angle+=90;
		Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
		transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * 10f);		
	}
	
	public void Selected()
	{
		spriteR.color=secondColor;
		if(!isSelected)
			transform.localScale=new Vector3(1.5f,1.5f,1f);
		isSelected=true;
	}
	
	public void unSelected()
	{
		spriteR.color=initColor;
		transform.localScale=new Vector3(1f,1f,1f);
		isSelected=false;
	}
	
	public void Disappear()
	{
		spriteR.enabled=false;
		unSelected ();
	}
	
	public void Appear()
	{
		spriteR.enabled=true;
	}

	public void underIn()
	{
		spriteR.sprite=spriteWhite;	
		spriteR.sortingLayerName="CafardTop";
	}

	public void underOut()
	{
		spriteR.sprite=initSprite;
		spriteR.sortingLayerName="Cafard";
	}

	
}