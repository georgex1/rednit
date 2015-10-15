using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class buscar : MonoBehaviour {

	private MainController GMS;

	public Text PersonaNombre;
	public Text PersonaEdad;
	public Text PersonaCiudad;
	public Image PersonaFoto;

	public GameObject PanelAviso;
	public GameObject PanelAvisoBtn;

	private string actualPersona;

	// Use this for initialization
	void Start () {
	
		GameObject GM = GameObject.Find ("MainController");
		GMS = GM.GetComponent<MainController>();

		GMS.db.OpenDB(GMS.dbName);
		ArrayList result = GMS.db.BasicQueryArray ("select id, nombre, edad, sexo, ciudad, foto from personas where visto = '0' ");
		GMS.db.CloseDB();

		if (result.Count > 0) {

			foreach (string[] row_ in result) {
				actualPersona = row_[0].ToString();
				PersonaNombre.text = row_[1];
				PersonaEdad.text = row_[2];
				PersonaCiudad.text = row_[4];

				PersonaFoto.sprite = GMS.spriteFromFile(row_[5]);
			}

			if(result.Count < 3){
				GMS.download_personas ();
			}

			GMS.actualizando = false;
		} else {
			PanelAvisoBtn.SetActive (true);
			PanelAviso.SetActive(true);
		}
	}

	public void actualizar(){
		PanelAvisoBtn.SetActive (false);
		GMS.actualizando = true;
		GMS.download_personas ();
	}

	public void votar(string voto_){
		if (voto_ == "SI") {

			string[] fields = {"usuarios_id", "voto_usuarios_id" };
			string[] values = {GMS.userData.id.ToString(), actualPersona};
			GMS.insert_sync(fields, values, "voto_usuario");

		}
		GMS.db.OpenDB(GMS.dbName);
		GMS.db.UpdateSingle("personas", "visto", "1", "id" , actualPersona);
		GMS.db.CloseDB();

		Application.LoadLevel (Application.loadedLevelName);
	}

	// Update is called once per frame
	void Update () {
		if(!GMS.actualizando && PanelAviso.activeSelf){
			StartCoroutine(closeActualizarPanel());
		}
	}

	IEnumerator closeActualizarPanel(){
		yield return new WaitForSeconds (3);
		PanelAviso.SetActive(false);
		Application.LoadLevel (Application.loadedLevelName);
	}

	public void cargarEscena(string escena){
		Application.LoadLevel (escena);
	}

}
