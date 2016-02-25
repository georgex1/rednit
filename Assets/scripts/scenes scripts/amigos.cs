using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class amigos : MonoBehaviour {

	private MainController GMS;
	public GameObject MSG_sinamigos;

	// Use this for initialization
	void Start () {
		GameObject GM = GameObject.Find ("MainController");
		GMS = GM.GetComponent<MainController>();
		
		GMS.db.OpenDB(GMS.dbName);
		ArrayList result = GMS.db.BasicQueryArray ("select usuarios_id, amigos_id, aceptado, nombre, email, edad, sexo, ciudad, foto, descripcion from amigos where aceptado = '1' ");
		GMS.db.CloseDB();
		
		GameObject OptionDefault = GameObject.Find("DDItems/PanelMask/PanelScroll/Option");
		
		if (result.Count > 0) {
			Debug.Log("entro");
			MSG_sinamigos.SetActive(false);
			
			foreach (string[] row_ in result) {
				
				AmigoData amigoData = new AmigoData();
				amigoData.populateUser(row_);
				
				GameObject clone = Instantiate(OptionDefault, OptionDefault.transform.position, OptionDefault.transform.rotation) as GameObject;
				clone.transform.SetParent(OptionDefault.transform.parent);
				clone.transform.localScale = new Vector3(1, 1, 1);
				
				clone.transform.Find("PerfilMask/AmigoImagen").GetComponent<Image>().sprite = GMS.spriteFromFile( amigoData.foto );
				clone.transform.Find("Panel/Panel/AmigoNombre").GetComponent<Text>().text = amigoData.nombre;
				//clone.transform.Find("Panel/Panel/AmigoEdad").GetComponent<Text>().text = amigoData.edad;
				clone.transform.Find("Panel/Panel/AmigoDescripcion").GetComponent<Text>().text = amigoData.descripcion;
				
				clone.name = "opcion-" + amigoData.id;
			}
			
		}
		Destroy (OptionDefault);
	}

	public void chatear(GameObject optionBtn){
		string[] idOpcion = optionBtn.name.Split('-');

		Debug.Log ("id opcion: " + idOpcion[1]);

		GMS.db.OpenDB(GMS.dbName);
		ArrayList result = GMS.db.BasicQueryArray ("select usuarios_id, amigos_id, aceptado, nombre, email, edad, sexo, ciudad, foto, descripcion, chat_group from amigos where amigos_id = '" +idOpcion[1]+ "' ");

		GMS.amigoData.populateUser (((string[])result [0]));
		GMS.amigoData.chat_group = ((string[])result [0]) [10];

		GMS.db.CloseDB();

		Application.LoadLevel ("chat");

	}

	public void goBack() {
		Application.LoadLevel("buscar");
	}

}
