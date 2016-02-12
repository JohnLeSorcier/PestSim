using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public float speed=10f;
	public float zoomSpeed=5f;
	public float offset=0.02f;
	float zoomInit;

	public Camera frgndCam;
	
	void Start()
	{
		zoomInit=Camera.main.orthographicSize;
	}

	void Update()
	{
		
		if((Input.GetKey(KeyCode.RightArrow) || Camera.main.ScreenToViewportPoint(Input.mousePosition).x>1f-offset)&& transform.position.x<30)
			transform.Translate(new Vector3(speed * Time.deltaTime,0,0));
		
		if((Input.GetKey(KeyCode.LeftArrow) || Camera.main.ScreenToViewportPoint(Input.mousePosition).x<offset)&& transform.position.x>-10)
			transform.Translate(new Vector3(-speed * Time.deltaTime,0,0));
		
		if((Input.GetKey(KeyCode.DownArrow) || Camera.main.ScreenToViewportPoint(Input.mousePosition).y<offset)&& transform.position.y>-15)
			transform.Translate(new Vector3(0,-speed * Time.deltaTime,0));
		
		if((Input.GetKey(KeyCode.UpArrow) || Camera.main.ScreenToViewportPoint(Input.mousePosition).y>1f-offset)&& transform.position.y<15)
			transform.Translate(new Vector3(0,speed * Time.deltaTime,0));
		
		
		float scroll = Input.GetAxis ("Mouse ScrollWheel");
		if ((scroll > 0.0f && Camera.main.orthographicSize>1)|| (scroll < 0.0f && Camera.main.orthographicSize<10)) 
		{
			Camera.main.orthographicSize -= scroll * zoomSpeed;
		}
		
		if (Input.GetKey(KeyCode.R))
		{
			Camera.main.orthographicSize=zoomInit;
			transform.position=new Vector3 (0,0, transform.position.z);
		}
		
	}

	void OnGUI()
	{
		if(Event.current.type==EventType.Repaint)
			frgndCam.Render();
	}

}
