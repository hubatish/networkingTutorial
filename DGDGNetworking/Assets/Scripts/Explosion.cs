using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour {
	private float time = 2f;
	
	private void Start()
	{
		Invoke("KillMe",time);
	}
	
	private void KillMe()
	{
		GameObject.Destroy(gameObject);
	}
	
}