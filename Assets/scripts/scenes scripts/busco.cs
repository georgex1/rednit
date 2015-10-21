using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class busco : MonoBehaviour {

	public GameObject btnDesde;
	public GameObject btnHasta;
	private MainController GMS;

	public GameObject DDDdesde;
	public GameObject DDHasta;

	public GameObject busco_ciudad;
	public string busco_sexo;

	public GameObject menuObj;

	// Use this for initialization
	void Start () {
		GameObject GM = GameObject.Find ("MainController");
		GMS = GM.GetComponent<MainController>();

		if (GMS.userData.busco_sexo != "") {
			GameObject.Find (GMS.userData.busco_sexo).GetComponent<Toggle> ().isOn = true;
		} else {

			if (GMS.userData.sexo == "HOMBRE") {
				GameObject.Find ("MUJER").GetComponent<Toggle> ().isOn = true;
			} else {
				GameObject.Find ("HOMBRE").GetComponent<Toggle> ().isOn = true;
			}
		}

		btnDesde.GetComponentInChildren<Text> ().text = GMS.userData.busco_edad_min;
		btnHasta.GetComponentInChildren<Text> ().text = GMS.userData.busco_edad_max;

		busco_ciudad.transform.parent.GetComponent<InputField>().text = GMS.userData.ciudad;

		generarDesdeHasta ();

		if (PlayerPrefs.GetString ("busco_completo") != "1") {
			menuObj.SetActive(false);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void selectDesde(GameObject opcion){
		btnDesde.GetComponentInChildren<Text> ().text = opcion.GetComponentInChildren<Text> ().text;
		Debug.Log (opcion.GetComponentInChildren<Text> ().text);
		GMS.userData.busco_edad_min = opcion.GetComponentInChildren<Text> ().text;
		DDDdesde.SetActive (false);
	}

	public void selectHasta(GameObject opcion){
		btnHasta.GetComponentInChildren<Text> ().text = opcion.GetComponentInChildren<Text> ().text;
		Debug.Log (opcion.GetComponentInChildren<Text> ().text);
		GMS.userData.busco_edad_max = opcion.GetComponentInChildren<Text> ().text;
		DDHasta.SetActive (false);
	}

	public void generarDesdeHasta(){

		GameObject OptionDefaultDesde = DDDdesde.transform.Find("PanelMask/PanelScroll/Option").gameObject;
		GameObject OptionDefault = DDHasta.transform.Find("PanelMask/PanelScroll/Option").gameObject;

		Debug.Log ("OptionDefaultDesde name: " + OptionDefaultDesde.name);
		Debug.Log ("OptionDefault name: " + OptionDefault.name);

		for(int i = 18; i < 51; i++){
			GameObject clone = Instantiate(OptionDefaultDesde, OptionDefaultDesde.transform.position, OptionDefaultDesde.transform.rotation) as GameObject;
			clone.transform.SetParent(OptionDefaultDesde.transform.parent);
			clone.transform.localScale = new Vector3(1, 1, 1);
			clone.GetComponentInChildren<Text> ().text = i.ToString();

			GameObject clone2 = Instantiate(OptionDefault, OptionDefault.transform.position, OptionDefault.transform.rotation) as GameObject;
			clone2.transform.SetParent(OptionDefault.transform.parent);
			clone2.transform.localScale = new Vector3(1, 1, 1);
			clone2.GetComponentInChildren<Text> ().text = i.ToString();

		}
		
		Destroy (OptionDefaultDesde);
		Destroy (OptionDefault);
		
		DDDdesde.SetActive(false);
		DDHasta.SetActive(false);
	}

	public void changeSexo(string sexo_){
		busco_sexo = sexo_;
	}

	public void submit(){

		if(!GMS.haveInet){
			GMS.errorPopup("Verifica tu conexion a internet");
		}else{
			
			GMS.userData.busco_sexo = busco_sexo;
			GMS.userData.busco_ciudad = busco_ciudad.GetComponent<Text>().text;

			GMS.showLoading(true);

			GMS.perfil_busco();

			PlayerPrefs.SetString("busco_completo", "1");
			StartCoroutine(gotoNext());
		}
		
	}

	private IEnumerator gotoNext(){
		yield return new WaitForSeconds (2);
		GMS.showLoading(false);
		Application.LoadLevel ("buscar");
	}
}
