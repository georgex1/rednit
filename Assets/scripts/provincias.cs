using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class provincias : MonoBehaviour {

	public GameObject btnProvincias;
	public GameObject DDProvincias;

	private MainController GMS;

	// Use this for initialization
	void Start () {
		generarProvincias ();

		GameObject GM = GameObject.Find ("MainController");
		GMS = GM.GetComponent<MainController>();
	}

	public void selectProvincia(GameObject opcion){
		btnProvincias.GetComponentInChildren<Text> ().text = opcion.GetComponentInChildren<Text> ().text;
		Debug.Log (opcion.GetComponentInChildren<Text> ().text);
		GMS.userData.ciudad = opcion.GetComponentInChildren<Text> ().text;
		DDProvincias.SetActive (false);
	}

	public void generarProvincias(){
		string[] ArrayProvincias = new string[24]{
			"Buenos Aires",
			"Catamarca",
			"Chaco",
			"Chubut",
			"Córdoba",
			"Corrientes",
			"Distrito Federal",
			"Entre Ríos",
			"Formosa",
			"Jujuy",
			"La Pampa",
			"La Rioja",
			"Mendoza",
			"Misiones",
			"Neuquén",
			"Río Negro",
			"Salta",
			"San Juan",
			"San Luis",
			"Santa Cruz",
			"Santa Fe",
			"Santiago del Estero",
			"Tierra del Fuego",
			"Tucumán"
		};
		
		GameObject OptionDefault = GameObject.Find("DDProvincias/PanelMask/PanelScroll/Option");
		
		foreach (string aux in ArrayProvincias) {
			GameObject clone = Instantiate(OptionDefault, OptionDefault.transform.position, OptionDefault.transform.rotation) as GameObject;
			clone.transform.SetParent(OptionDefault.transform.parent);
			clone.transform.localScale = new Vector3(1, 1, 1);
			clone.GetComponentInChildren<Text> ().text = aux;
		}
		
		Destroy (OptionDefault);
		
		DDProvincias.SetActive(false);
	}

}
