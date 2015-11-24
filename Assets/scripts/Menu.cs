using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Menu : MonoBehaviour {

	private int menuShow = 0;
	private string AnimName = "menu";
	private float resetTime = 0.00F;

	private MainController GMS;
	public GameObject MenuUNombre;
	public GameObject MenuUImagen;

	// Use this for initialization
	void Start () {
		GameObject GM = GameObject.Find ("MainController");
		GMS = GM.GetComponent<MainController>();

		MenuUImagen.GetComponent<Image>().sprite = GMS.spriteFromFile( GMS.userData.foto );
		MenuUNombre.GetComponent<Text>().text = GMS.userData.nombre;
	}
	
	public void cargarEscena(string escena){
		Application.LoadLevel (escena);
	}

	public void Salir(){
		Application.Quit();
	}

	public void showMenu(){
		Animation panelPrincipal = transform.GetComponent<Animation> ();

		if(menuShow == 0){
			panelPrincipal[AnimName].speed = 3; 
			panelPrincipal[AnimName].time = resetTime;
			panelPrincipal.Play(AnimName);
			menuShow = 1;
		}else if(menuShow == 1){
			panelPrincipal[AnimName].speed = -3; 
			resetTime =panelPrincipal[AnimName].time;
			panelPrincipal[AnimName].time = panelPrincipal[AnimName].length;
			panelPrincipal.Play(AnimName);
			menuShow = 0;
		}

	}
}
