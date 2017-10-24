using UnityEngine;
using System.Collections;
	
public class interface_touch: MonoBehaviour {
		
	GameObject cLight;		
		
	void Start () {
		cLight = GameObject.Find("2DLight");
			
		StartCoroutine (LoopUpdate ());
			
	}

	IEnumerator LoopUpdate () {
			
		while (true) {
			Vector3 pos = cLight.transform.position;
			pos.x += Input.GetAxis ("Horizontal") * 30f * Time.deltaTime;
			pos.y += Input.GetAxis ("Vertical") * 30f * Time.deltaTime;
			yield return new WaitForEndOfFrame ();
			cLight.transform.position = pos;
				
		}
			
			
	}
		
		
		
}