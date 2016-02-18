using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class gallery : MonoBehaviour {
	
	private MainController GMS;
	
	public Text PersonaNombre;
	public Text PersonaEdad;
	//public Text PersonaCiudad;
	
	public Text PersonaDescripcion;
	
	private string actualPersona;
	
	public GameObject defaultPanelGal;
	
	private bool isGalleryCharged = false;

	public GameObject containerPanel;
	public GameObject[] panels;
	private int totalImg = 0;
	public int currentImg = 1;
	
	private float posXContaniner;
	private float totalWidth = 0;
	
	//horizontal swipe touch
	public float minSwipeDistY;
	public float minSwipeDistX;
	
	private float marginX = Screen.width;
	
	public GameObject arrowLeft;
	public GameObject arrowRight;
	
	private Vector2 startPos;

	// Use this for initialization
	void Start () {

		//get usuario id
		actualPersona = PlayerPrefs.GetString ("usuarios_id");
		
		GameObject GM = GameObject.Find ("MainController");
		GMS = GM.GetComponent<MainController>();
		
		GMS.donwloadinGallery = false;
		
		GMS.db.OpenDB(GMS.dbName);
		string buscarQuery = "select id, nombre, edad, sexo, ciudad, foto, descripcion from personas where id = '"+actualPersona+"'";
		ArrayList result = GMS.db.BasicQueryArray (buscarQuery);

		GMS.db.CloseDB();
		
		if (result.Count > 0) {//verificar si esta en personas

			PersonaNombre.text = ((string[])result [0]) [1];
			PersonaEdad.text = ((string[])result [0]) [2];
			//PersonaCiudad.text = ((string[])result [0]) [4];
			PersonaDescripcion.text = ((string[])result [0]) [6];

			getGallery ();

		} else {
			//verificar si esta en amigos_usuarios
			buscarQuery = "select personas_id, nombre, edad, sexo, ciudad, foto, descripcion from amigos_usuarios where personas_id = '"+actualPersona+"'";
			Debug.Log(buscarQuery);
			GMS.db.OpenDB(GMS.dbName);
			result = GMS.db.BasicQueryArray (buscarQuery);
			GMS.db.CloseDB();

			if (result.Count > 0) {//verificar si esta en personas
				
				PersonaNombre.text = ((string[])result [0]) [1];
				PersonaEdad.text = ((string[])result [0]) [2];
				//PersonaCiudad.text = ((string[])result [0]) [4];
				PersonaDescripcion.text = ((string[])result [0]) [6];
				
				getGallery ();
				
			} else {
				Application.LoadLevel("home");
			}
		}
		
	}
	
	// Update is called once per frame
	void Update () {
		
		if (GMS.donwloadinGallery && GMS.CountPersonasGal == 0 && !isGalleryUpdate) {
			GMS.showLoading (false);
			isGalleryUpdate = true;
			initGallery();
		}
		
		checkHorizontalSwipes();
	}
	
	private bool isGalleryUpdate = false;
	
	private void initGallery(){
		
		Debug.Log ("init gallery");
		
		GMS.db.OpenDB (GMS.dbName);
		
		ArrayList result = GMS.db.BasicQueryArray ("select foto, id from fotos_usuarios where usuarios_id = '"+ actualPersona +"'");
		if (result.Count > 0) {
			foreach (string[] row_ in result) {
				GameObject clone = Instantiate(defaultPanelGal, defaultPanelGal.transform.position, defaultPanelGal.transform.rotation) as GameObject;
				clone.transform.SetParent(defaultPanelGal.transform.parent);
				clone.transform.localScale = new Vector3(1, 1, 1);
				
				clone.transform.Find("PanelGlaImg").GetComponent<Image>().sprite = GMS.spriteFromFile( row_[0] );
				
				clone.name = "opcion-" + row_[1];
			}
		}
		
		Destroy (defaultPanelGal);
		//defaultPanelGal.SetActive (false);
		
		Debug.Log ("startPanelGallery");
		StartCoroutine(startPanelGallery()) ;
		
		
		
		GMS.db.CloseDB ();
	}
	
	private void checkDownloadGallery(){
	}
	
	IEnumerator closeActualizarPanel(){
		yield return new WaitForSeconds (2);
		//PanelAviso.SetActive(false);
		Application.LoadLevel (Application.loadedLevelName);
	}
	
	public void cargarEscena(string escena){
		Application.LoadLevel (escena);
	}

	public void back(){
		Application.LoadLevel (GMS.prevScene);
	}
	
	//galeria
	public void getGallery(){
		
		if (!isGalleryCharged) {
			
			GMS.CountPersonasGal = 0;
			GMS.showLoading (true);
			//intentar bajar la galeria de la persona
			GMS.downloadUserGallery (actualPersona, false, true);
			
			//StartCoroutine (initDownloadImage ());
		} else {
			StartCoroutine(startPanelGallery()) ;
		}
	}
	
	/*private IEnumerator initDownloadImage(){
		yield return new WaitForSeconds (0.5f);
	}*/
	
	public void moveContainer(){
		
		stateArrows();
		
		GameObject _panel = panels[currentImg-1];
		float posX = posXContaniner - (_panel.transform.localPosition.x);
		
		iTween.MoveTo(containerPanel, iTween.Hash("x", posX, "easeType", "easeInOutCirc", "islocal", true));
	}
	
	private IEnumerator startPanelGallery(){
		yield return new WaitForSeconds (0.3f);
		
		isGalleryCharged = true;
		
		panels = GameObject.FindGameObjectsWithTag("galleryTag");
		
		totalImg = panels.Length; //----> 3
		Debug.Log ("total imagen: " + totalImg);

		arrangeImg();
		stateArrows();

		posXContaniner = containerPanel.transform.localPosition.x;//---->
	}
	
	public void arrangeImg(){
		for (int i = 0; i < totalImg; i++) {
			GameObject _panel = panels[i];
			_panel.transform.localPosition = new Vector3(totalWidth, 0, 1); //---> 0 //---> 10 //---> 20
			
			totalWidth = totalWidth + marginX;
		}
	}
	
	
	public void checkHorizontalSwipes(){
		if (Input.touchCount > 0){
			Touch touch = Input.touches[0];
			
			switch (touch.phase) {
			case TouchPhase.Began:
				
				startPos = touch.position;
				
				break;
				
			case TouchPhase.Ended:
				
				float swipeDistHorizontal = (new Vector3(touch.position.x,0, 0) - new Vector3(startPos.x, 0, 0)).magnitude;
				
				if (swipeDistHorizontal > minSwipeDistX){
					
					float swipeValue = Mathf.Sign(touch.position.x - startPos.x);
					
					if (swipeValue > 0){//right swipe
						changeImg(currentImg - 1);
					}else if (swipeValue < 0){//left swipe
						changeImg(currentImg + 1);
					}
				}
				break;
			}
		}
	}
	
	public void changeImg(int _num){
		Debug.Log ("cambiar imagen: " + _num);
		if(_num < 1 || _num > totalImg){
			return;
		}
		currentImg = _num;
		moveContainer ();
	}
	
	public void stateArrows(){
		if (totalImg == 1) {
			arrowLeft.SetActive (false);
			arrowRight.SetActive (false);
		} else if (currentImg == 1) {
			arrowLeft.SetActive (false);
			arrowRight.SetActive (true);
		} else if (currentImg == totalImg) {
			arrowLeft.SetActive (true);
			arrowRight.SetActive (false);
		} else {
			arrowLeft.SetActive (true);
			arrowRight.SetActive (true);
		}
	}
	
	public void btnArrowLeft(){
		changeImg(currentImg - 1);
	}
	
	public void btnArrowRight(){
		Debug.Log ("boton");
		changeImg(currentImg + 1);
	}
	
}
