using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using VoxelBusters.NativePlugins;
using VoxelBusters.AssetStoreProductUtility.Demo;
using System.IO;

public class user_images : MonoBehaviour {

	private MainController GMS;
	GameObject OptionDefault;
	public GameObject imageTexture;

	private int countImages = 0;
	private GameObject OptionAdd;

	Texture2D actuualText;
	string newImageName;
	string newImageNameId;

	// Use this for initialization
	void Start () {
		GameObject GM = GameObject.Find ("MainController");
		GMS = GM.GetComponent<MainController>();
		
		OptionDefault = GameObject.Find("DDItems/PanelMask/PanelScroll/Option");
		OptionDefault.SetActive (false);


		//precargar las imagenes guardadas
		GMS.db.OpenDB(GMS.dbName);
		ArrayList result = GMS.db.BasicQueryArray ("select id, usuarios_id, foto, isdefault from fotos_usuarios where usuarios_id = '" +GMS.userData.id+ "' ");
		GMS.db.CloseDB();

		if (result.Count > 0) {
			
			foreach (string[] row_ in result) {

				setObjPicture(row_ [2], row_ [0]);

			}
			
		}

		/*for (int i=0; i<1;i++) {
			
			GameObject clone = Instantiate(OptionDefault, OptionDefault.transform.position, OptionDefault.transform.rotation) as GameObject;
			clone.transform.SetParent(OptionDefault.transform.parent);
			clone.transform.localScale = new Vector3(1, 1, 1);
			
			clone.transform.Find("PerfilMask/userImage").GetComponent<Image>().sprite = GMS.spriteFromFile( amigoData.foto );
			
			clone.name = "opcion-" + i;
		}*/

	}

	public void OptDefault(GameObject aceptadoBtn){
		string[] aceptado_ = aceptadoBtn.name.Split('-');
		
		string[] idOpcion = aceptadoBtn.transform.parent.parent.parent.name.Split('-');
		Debug.Log ("id opcion: " + idOpcion[1]);

		GMS.change_gallery_default (idOpcion[1]);
	}

	public void OptBorrar(GameObject aceptadoBtn){
		string[] aceptado_ = aceptadoBtn.name.Split('-');
		
		string[] idOpcion = aceptadoBtn.transform.parent.parent.parent.name.Split('-');
		Debug.Log ("id opcion: " + idOpcion[1]);


		GMS.delete_foto_gallery ( idOpcion[1] );

		Destroy (aceptadoBtn.transform.parent.parent.parent );

		countImages -= 1;

	}

	public void addPhoto(){

		#if UNITY_EDITOR
		test_guardar();
		#else
		NPBinding.MediaLibrary.PickImage(eImageSource.BOTH, 1.0f, PickImageFinished);
		#endif
	}

	private void test_guardar(){
		
		byte[] fileData = File.ReadAllBytes("Assets/Resources/fluence GT2.jpg");
		actuualText = new Texture2D(2, 2);
		actuualText.LoadImage(fileData); //..this will auto-resize the texture dimensions.
		imageTexture.renderer.material.mainTexture = actuualText;
		
		newImageName = GMS.generateId ().ToString () + ".png";
		
	}
	 
	private void setPincture(){

		//guardo fisicamente la imagen en la app
		StartCoroutine (GMS.saveTextureToFile (imageTexture.renderer.material.mainTexture as Texture2D, newImageName, 'g'));
		//StartCoroutine (loadImage ());

		StartCoroutine (setPinctureContinue());
	}

	private IEnumerator setPinctureContinue(){
		yield return new WaitForSeconds(1);

		string isDefault = "N";

		if (countImages == 0) {
			//agrego como default de la galeria
			isDefault = "Y";
		}

		//subo y guardo en la db local
		GMS.upload_foto_gallery (newImageName, newImageNameId, isDefault);
		
		setObjPicture(newImageName, newImageNameId);
	}

	private void setObjPicture(string newImageName_, string newImageNameId_){

		//sumo la cantidad de imagenes cargadas
		countImages += 1;

		//creo le item de foto en la galeria
		GameObject clone = Instantiate(OptionDefault, OptionDefault.transform.position, OptionDefault.transform.rotation) as GameObject;
		clone.transform.SetParent(OptionDefault.transform.parent);
		clone.transform.localScale = new Vector3(1, 1, 1);
		
		clone.transform.Find("PerfilMask/userImage").GetComponent<Image>().sprite = GMS.spriteFromFile( newImageName_ );
		
		clone.name = "opcion-" + newImageNameId_;

		GMS.showLoading (false);
	}
	
	private void PickImageFinished (ePickImageFinishReason _reason, Texture2D _image)
	{
		string reasonString = _reason + "";
		if (reasonString == "SELECTED") {
			GMS.showLoading (true);

			imageTexture.renderer.material.mainTexture = _image;
			
			newImageName = GMS.generateId ().ToString () + ".png";
			
			StartCoroutine (GMS.saveTextureToFile (imageTexture.renderer.material.mainTexture as Texture2D, newImageName, 'g'));

			StartCoroutine (setPinctureContinue());
		}
	}
	
	/*IEnumerator loadImage(){
		yield return new WaitForSeconds(1);
		Sprite sprite = GMS.spriteFromFile (GMS.userData.temp_img);
		GameObject.Find ("backImage").GetComponent<Image>().sprite = sprite;
		GMS.showLoading (false);


		GMS.generateId ();

	}*/
	
	// Update is called once per frame
	void Update () {
		if (countImages == 5 && OptionAdd.activeSelf) {
			OptionAdd.SetActive(false);
		}

		if (countImages < 5 && !OptionAdd.activeSelf) {
			OptionAdd.SetActive(true);
		}
	}

	public void gotoPerfil(){
		Application.LoadLevel ("perfil");
	}
}
