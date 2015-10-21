using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class chat : MonoBehaviour {
	SimChat sc;
	string sender;
	protected Vector2 sp = Vector2.zero;
	public Text Mgs;
	float rt1=-3f;
	private MainController GMS;

	// Use this for initialization
	void Start () {
		GameObject GM = GameObject.Find ("MainController");
		GMS = GM.GetComponent<MainController>();

		string chatGroup = GMS.amigoData.chat_group;

		sender = GMS.userData.nombre + "-" + GMS.userData.id;
		sc = new SimChat(chatGroup, gameObject.GetComponent<MonoBehaviour>(), sender);
		
		sc.continueCheckMessages();
		sc.setReceiveFunction(receiveMessage1);
	}
	
	//functions to call when a new message is received
	void receiveMessage1(SimpleMessage[] sm){
		rt1 = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnGUI(){
		GUI.skin.textField.fontSize = GUI.skin.button.fontSize = GUI.skin.label.fontSize = 30;
		
		displayChat(new Rect(0, Screen.height * 0.1f, Screen.width, Screen.height*0.8f),sc,sp);
	}
	
	void displayChat(Rect area,SimChat sc,Vector2 sp){
		sp.y = Mathf.Infinity;
		GUILayout.BeginArea(area);
		
		GUILayout.BeginVertical("box");
		sp = GUILayout.BeginScrollView(sp);
		Color c = GUI.contentColor;
		//loop through each of the messages contained in allMessages
		foreach(SimpleMessage sm in sc.allMessages){
			GUILayout.BeginHorizontal();
			//check if the sender had the same name as me, and change the color
			if(sm.sender == sender){
				GUI.contentColor = Color.red;
				GUILayout.FlexibleSpace();
				GUILayout.Label(sm.message);
			}else{
				GUI.contentColor = Color.green;
				GUILayout.Label(sm.sender+": "+sm.message);
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();
		}
		GUI.contentColor = c;
		GUILayout.EndScrollView();
		
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
	
	public void sendMgs(){
		Debug.Log (Mgs.text);
		sc.message = Mgs.text;
		sc.sendMessage();
		sc.message = "";
		Mgs.transform.parent.GetComponent<InputField> ().text = "";
		Debug.Log ("mgs enviado..");
	}
}
