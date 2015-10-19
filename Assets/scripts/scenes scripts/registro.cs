using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;

public class registro : MonoBehaviour {

	public GameObject nombre;
	public GameObject email;
	public GameObject ciudad;
	public string sexo;

	private MainController GMS;
	public GameObject buttonSubmit;

	public GameObject menuObj;

	void Start () {
		//al final a subir-foto
		buttonSubmit.SetActive (false);
		GameObject GM = GameObject.Find ("MainController");
		GMS = GM.GetComponent<MainController>();

		fechaNacimiento fechaNacScript = gameObject.GetComponent<fechaNacimiento> ();

		if (GMS.userData.fecha_nacimiento != null && GMS.userData.fecha_nacimiento != "") {
			string[] splitFechaNac = GMS.userData.fecha_nacimiento.Split('/');

			fechaNacScript.btnDay.GetComponentInChildren<Text> ().text = splitFechaNac[0];
			fechaNacScript.btnMonth.GetComponentInChildren<Text> ().text = GMS.userData.format_month_int( splitFechaNac[1] );
			fechaNacScript.btnYear.GetComponentInChildren<Text> ().text = splitFechaNac[2];

			GMS.userData.date_year = splitFechaNac[2];
			GMS.userData.date_month = splitFechaNac[1];
			GMS.userData.date_day = splitFechaNac[0];

		}
		if (GMS.userData.sexo != "") {
			GameObject.Find (GMS.userData.sexo).GetComponent<Toggle>().isOn = true;
		}

		if (GMS.userData.ciudad != "") {
			ciudad.transform.parent.GetComponent<InputField>().text = GMS.userData.ciudad;
		}

		if(GMS.userData.foto != ""){
			string filepath = Application.persistentDataPath + "/" + GMS.userData.foto;
			if (File.Exists (filepath)) {
				GameObject.Find ("backImage").GetComponent<Image>().sprite = GMS.spriteFromFile(GMS.userData.foto);
			}
		}

		email.transform.parent.GetComponent<InputField>().text = GMS.userData.email;
		nombre.transform.parent.GetComponent<InputField>().text = GMS.userData.nombre;
		ciudad.transform.parent.GetComponent<InputField>().text = GMS.userData.ciudad;
		sexo = GMS.userData.sexo;

		if (PlayerPrefs.GetString ("busco_completo") != "1") {
			menuObj.SetActive(false);
		}

		Debug.Log ("datos user: " + nombre.GetComponent<Text>().text + email.GetComponent<Text>().text + ciudad.GetComponent<Text>().text);
	}
	
	void Update(){
		if (nombre.GetComponent<Text>().text != "" && validEmail (email.GetComponent<Text>().text) && ciudad.GetComponent<Text>().text != ""
		    && sexo != "" && GMS.userData.date_day != "" && GMS.userData.date_month != "" && GMS.userData.date_year != "") {

			if(!buttonSubmit.activeSelf){
				Debug.Log("active button");
				buttonSubmit.SetActive (true);
			}
		} else {
			if(buttonSubmit.activeSelf){
				buttonSubmit.SetActive (false);
			}
		}
	}
	
	
	public void submit(){
		
		//string format_birthDate = GMS.userData.date_year + '-' + GMS.userData.date_month + '-' + GMS.userData.date_day;
		string format_birthDate = GMS.userData.date_day + "/" + GMS.userData.date_month + '/' + GMS.userData.date_year;
		
		if(!GMS.haveInet){
			GMS.errorPopup("Verifica tu conexion a internet");
		}else{
			
			GMS.userData.email = email.GetComponent<Text>().text;
			GMS.userData.nombre = nombre.GetComponent<Text>().text;
			GMS.userData.ciudad = ciudad.GetComponent<Text>().text;
			GMS.userData.fecha_nacimiento = format_birthDate;
			GMS.userData.sexo = sexo;

			GMS.userData.foto = GMS.userData.temp_img;

			GMS.showLoading(true);

			GMS.upload_user_foto();
		}
		
	}
	
	public void changeSexo(string sexo_){
		sexo = sexo_;
	}
	
	/*public void showFechaNac(GameObject fechaNac){
		if(fechaNac.activeSelf == true){
			fechaNac.SetActive(false);
		}else{
			fechaNac.SetActive(true);
		}
	}*/
	
	public bool validEmail(string emailaddress){
		return System.Text.RegularExpressions.Regex.IsMatch(emailaddress, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
	}
}
