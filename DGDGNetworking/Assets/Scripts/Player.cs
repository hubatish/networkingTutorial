using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	
	private float speed = 0.3f;
	
	public void Update()
	{
		if(networkView.isMine)
		{
			MoveByInput();
			InputActions();
		}
		else
			SyncMovement();
	}
	
	void MoveByInput()
	{
		Vector3 toMove = new Vector3(0,0,0);
		toMove.x = Input.GetAxis("Horizontal")*speed;
		toMove.y = Input.GetAxis("Vertical")*speed;
		transform.position += toMove;
	}
	
	//how long does it take between updates?
	private float lastSynchronizationTime = 0f;
	private float syncDelay = 0f;
	private float syncTime = 0f;
	//how far we traveled since last update
	private Vector3 syncStartPosition = Vector3.zero;
	private Vector3 syncEndPosition = Vector3.zero;
	
	private void SyncMovement()
	{
		syncTime += Time.deltaTime;
		transform.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
	}
	
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		Vector3 syncPosition = Vector3.zero;
		if (stream.isWriting)
		{
			syncPosition = transform.position;
			stream.Serialize(ref syncPosition);
		}
		else
		{
			//stream.Serialize(ref syncPosition);
			//transform.position = syncPosition;
			stream.Serialize(ref syncPosition);
			syncTime = 0f;
			syncDelay = Time.time - lastSynchronizationTime;
			lastSynchronizationTime = Time.time;
	 
			syncStartPosition = transform.position;
			syncEndPosition = syncPosition;
		}
	}
	
	private void InputActions()
	{
		if (Input.GetKeyDown(KeyCode.R))
			ChangeColorTo(new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
		//if (Input.GetKeyDown(KeyCode.Space))
		//	Die();
	}
	 
	[RPC] 
	void ChangeColorTo(Vector3 color)
	{
		renderer.material.color = new Color(color.x, color.y, color.z, 1f);
	 
		if (networkView.isMine)
			networkView.RPC("ChangeColorTo", RPCMode.OthersBuffered, color);
	}
	
	public Transform explosion;
	
	private void OnDestroy()
	{
		GameObject.Instantiate(explosion,transform.position,Quaternion.identity);
	}
	
}