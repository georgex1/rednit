using UnityEngine;
using System.Collections;

public class chat : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void cargarEscena(string escena){
		Application.LoadLevel (escena);
	}
}
