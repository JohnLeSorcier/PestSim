using UnityEngine;
using System.Collections;
using Pathfinding;


public class MoveController : MonoBehaviour 
{

	//éléments nécessaire au pathfinding
	public Path path;
	Seeker seeker;
	public float nextWaypointDistance = 0.00f;
	private int currentWaypoint = 0;
	public float speed=2f;
	public float turningSpeed=20f;
	
	

	void Awake () 
	{
		seeker = GetComponent<Seeker>();		
	}
	
	void FixedUpdate () 
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
	
	void RotateTowards(Vector3 dest)
	{
		Vector3 dir= transform.position-dest;
		Quaternion rot = transform.rotation;

		Quaternion toTarget = Quaternion.LookRotation(dir, Vector3.forward);		
		rot = Quaternion.Slerp (rot,toTarget,turningSpeed*Time.deltaTime);
		Vector3 euler = rot.eulerAngles;
		euler.x = 0;
		euler.y = 0;
		rot = Quaternion.Euler (euler);
		
		transform.rotation = rot;
	}
	
}
