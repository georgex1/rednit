using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class home : MonoBehaviour {

	private MainController GMS;

	// Use this for initialization
	void Start () {
		GameObject GM = GameObject.Find ("MainController");
		GMS = GM.GetComponent<MainController>();

		GMS.db.OpenDB(GMS.dbName);

		ArrayList result = GMS.db.BasicQueryArray ("select usuarios_id, personas_id, aceptado, nombre, email, edad, sexo, ciudad, foto from amigos_usuarios where aceptado = '0' ");
		GMS.db.CloseDB();

		GameObject OptionDefault = GameObject.Find("DDItems/PanelMask/PanelScroll/Option");

		if (result.Count > 0) {
			
			foreach (string[] row_ in result) {

				AmigoData amigoData = new AmigoData();
				amigoData.populateUser(row_);

				GameObject clone = Instantiate(OptionDefault, OptionDefault.transform.position, OptionDefault.transform.rotation) as GameObject;
				clone.transform.SetParent(OptionDefault.transform.parent);
				clone.transform.localScale = new Vector3(1, 1, 1);

				clone.transform.Find("Image").GetComponent<Image>().sprite = GMS.spriteFromFile( amigoData.foto );
				clone.transform.Find("Panel/Panel/AmigoNombre").GetComponent<Text>().text = amigoData.nombre;
				clone.transform.Find("Panel/Panel/AmigoEdad").GetComponent<Text>().text = amigoData.edad;
				clone.transform.Find("Panel/Panel/AmigoCiudad").GetComponent<Text>().text = amigoData.ciudad;

				clone.name = "opcion-" + amigoData.id;
			}

		}
		Destroy (OptionDefault);
	}

	// Update is called once per frame
	void Update () {
		
	}

	public void aceptarAmigo(GameObject aceptadoBtn){
		string[] aceptado_ = aceptadoBtn.name.Split('-');

		string[] idOpcion = aceptadoBtn.transform.parent.parent.parent.name.Split('-');
		Debug.Log ("id opcion: " + idOpcion[1]);

		GMS.db.OpenDB(GMS.dbName);
		string isAceptado = "2";
		if (aceptado_[1] == "SI") {
			isAceptado = "1";
		}

		GMS.db.UpdateSingle("amigos_usuarios", "aceptado", isAceptado, "personas_id" , idOpcion[1]);

		GMS.db.CloseDB();

		string[] fields = {"usuarios_id", "amigos_usuarios_id", "aceptado" };
		string[] values = {GMS.userData.id.ToString(), idOpcion[1], isAceptado};
		GMS.insert_sync(fields, values, "amigos_usuarios_aceptado");

		Destroy (aceptadoBtn.transform.parent.parent.parent.gameObject);
	}

	public void reload(){
		Application.LoadLevel (Application.loadedLevelName);
	}

}
