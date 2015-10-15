using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

	private string AnimName = "menu";
	private float resetTime = 0.00F;

	// Use this for initialization
	void Start () {
	
	}
	
	public void cargarEscena(string escena){
		Application.LoadLevel (escena);
	}

	public void Salir(){
		Application.Quit();
	}

	public void showMenu(){
		/*Animation panelPrincipal = transform.GetComponent<Animation> ();

		panelPrincipal[AnimName].speed = 1; 
		panelPrincipal[AnimName].time = resetTime;
		panelPrincipal.Play(AnimName);*/
	}
}
