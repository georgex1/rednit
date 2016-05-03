using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class testExtendClass : MonoBehaviour {
	SimpleChat sc;
	public GUISkin chatSkin;
	
	void Start(){
		string[] names = new string[]{"me","myself","you","nobody","somebody","hotdog","cupcake","Mr.Roboto","Phil","Sour Sally"};
		sc = new SimpleChat("pro_default","pass",gameObject.GetComponent<MonoBehaviour>(),names[Random.Range(0,names.Length)]);
	}
	
	void OnGUI(){
		//show chat toggle
		sc.show = GUI.Toggle(new Rect(0,Screen.height/2,800,100),sc.show,"Show Chat");
		//putting a skin or any type of GUI styling will effect what is in the draw function
		GUI.skin = chatSkin;
		sc.draw();
	}
}

public class SimpleChat:SimChat{
	//gui variables
	public bool show = true;
	public Rect chatRect = new Rect(Screen.width*0.6f,Screen.height*0.6f,Screen.width*0.4f,Screen.height*0.4f);
	public float messageTime = 3f;
	protected float rt = 0f;
	public Color myColor = Color.red,theirColor = Color.green;
	protected Vector2 sp = Vector2.zero;
	protected Color c;
	protected List<string> pending = new List<string>();
	protected TWordFilter wf;
	
	public SimpleChat(string identifier,string password,MonoBehaviour currentMonoBehaviour,string senderName):base(identifier,password,currentMonoBehaviour,senderName){
		continueCheckMessages();
		rt = -messageTime;
		setReceiveFunction(receive);
		TextAsset s = Resources.Load("words", typeof(TextAsset)) as TextAsset;
		if(s==null)
			UnityEngine.Debug.LogWarning("File Not Found 'words'");	
		wf = new TWordFilter(s.text);
	}
	
	protected void receive(SimpleMessage[] sma){
		show = true;
		//check if the last message is from me
		if(allMessages[allMessages.Count-1].sender != senderName)
			rt = Time.time;
		sp.y = Mathf.Infinity; //set the scroll
		pending = new List<string>(); //reset the pending message array
	}
	
	public void draw(){
		//display new message
		if(Time.time - rt < messageTime && allMessages.Count>0){
			GUILayout.Label("New Message: "+allMessages[allMessages.Count-1].sender+": "+allMessages[allMessages.Count-1].message);
		}
		
		//show chat area
		if(show){
			GUI.skin.label.wordWrap = true;
			GUILayout.BeginArea(chatRect);
			GUILayout.BeginVertical("box");
			
				GUILayout.BeginVertical("box");
					sp = GUILayout.BeginScrollView(sp);
					GUILayout.FlexibleSpace();
					c = GUI.contentColor;
					//loop through each of the messages contained in allMessages
					foreach(SimpleMessage  sm in allMessages){
						GUILayout.BeginHorizontal(GUILayout.Width(chatRect.width-25));//GUILayout.MaxWidth(chatRect.width-10)
							if(sm.sender==senderName){
								GUI.contentColor = myColor;
								GUILayout.FlexibleSpace();
								GUILayout.Label(sm.message);
							}else{
								GUI.contentColor = theirColor;
								GUILayout.Label(sm.sender+": "+sm.message);
								GUILayout.FlexibleSpace();
							}
						GUILayout.EndHorizontal();
					}
					//display the pending messages
					GUI.contentColor = myColor;
					foreach(string p in pending){
						GUILayout.BeginHorizontal(GUILayout.Width(chatRect.width-25));
							GUILayout.FlexibleSpace();
							GUILayout.Label(p);
						GUILayout.EndHorizontal();
					}
					GUI.contentColor = c;
					GUILayout.EndScrollView();
				GUILayout.EndVertical();
				
				GUILayout.BeginHorizontal();
					//send a new message
					message = GUILayout.TextField(message);
					if(GUILayout.Button("Send",GUILayout.MaxWidth(100)) || (Event.current.isKey && Event.current.keyCode == KeyCode.Return)){
						wf.cleanString(ref message);
						sendMessage();
						pending.Add(message);
						message = "";
					}
				GUILayout.EndHorizontal();
			
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}
}