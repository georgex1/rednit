using UnityEngine;
using System.Collections;

public class testTwoChat : MonoBehaviour {
	//Declare the SimChat variables.
	SimChat sc,sc2;
	//GUI Vars
	//scroll view position
	protected Vector2 sp = Vector2.zero,sp2 = Vector2.zero;
	//list of fake names
	string[] names = new string[]{"me","myself","you","nobody","somebody","hotdog","cupcake","Mr.Roboto","Phil","Sour Sally"};
	//selected fake names, temporary chat group
	string n1,n2,group1="default",group2="default",newGroup1,newGroup2,newPass1,newPass2;
	//Timers
	float rt1=-3f,rt2=-3f;
	//Word Filter
	TWordFilter wf;
	
	void Start(){
		//set chat group name
		newGroup1 = group1;
		newGroup2 = group2;
		//randomly select two different fake names
		int s = Random.Range(0,names.Length),s2=Random.Range(0,names.Length);
		n1 = names[s];
		while(s==s2){s2 = Random.Range(0,names.Length);}
		n2 = names[s2];
		//initialize the SimChat objects
		sc = new SimChat("default","hat",gameObject.GetComponent<MonoBehaviour>(),n1);
		sc2 = new SimChat("default","hatalso",gameObject.GetComponent<MonoBehaviour>(),n2);
		//tell the SimChat Objects to continuously check for new messages
		sc.continueCheckMessages();
		sc2.continueCheckMessages();
		//set the functions to call when a new message is received
		sc.setReceiveFunction(receiveMessage1);
		sc2.setReceiveFunction(receiveMessage2);
		//set the new password values to the current password values
		newPass1 = sc.password;
		newPass2 = sc2.password;
		//initialize the word filter
		TextAsset wd = Resources.Load("words", typeof(TextAsset)) as TextAsset;
		if(wd==null)
			UnityEngine.Debug.LogWarning("File Not Found 'words'");	
		wf = new TWordFilter(wd.text);
	}
	
	//functions to call when a new message is received
	void receiveMessage1(SimpleMessage[] sm){
		rt1 = Time.time;
	}
	void receiveMessage2(SimpleMessage[] sm){
		rt2 = Time.time;
	}

	void OnGUI(){
		//draw the GUI areas to display the information in
		GUI.skin.textField.fontSize = GUI.skin.button.fontSize = GUI.skin.label.fontSize = 17;

		GUI.skin.label.wordWrap = false;
		GUILayout.BeginArea(new Rect(0,0,Screen.width,70));
			GUILayout.BeginVertical("box");
				GUILayout.BeginHorizontal("box");
					GUILayout.FlexibleSpace();
					GUILayout.Label("Chat Group 1: "+group1);
					GUILayout.FlexibleSpace();
					//change the current chat group for both objects
					newGroup1 = GUILayout.TextField(group1);
					if(GUILayout.Button("Change Group 1")){
						sc.changeIdentifier(group1);
						group1 = newGroup1;
					}
					GUILayout.FlexibleSpace();
					GUILayout.Label("Chat Group 2: "+group2);
					GUILayout.FlexibleSpace();
					//change the current chat group for both objects
					newGroup1 = GUILayout.TextField(group2);
					if(GUILayout.Button("Change Group 2")){
						sc2.changeIdentifier(group2);
						group2 = newGroup2;
					}
					GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal("box");
					GUILayout.FlexibleSpace();
					GUILayout.Label("Password 1: "+sc.password);
					GUILayout.FlexibleSpace();
					newPass1 = GUILayout.TextField(newPass1);
					if(GUILayout.Button("Change Password 1")){
						sc.password = newPass1;
					}
					GUILayout.FlexibleSpace();
					GUILayout.Label("Password 2: "+sc2.password);
					GUILayout.FlexibleSpace();
					newPass2 = GUILayout.TextField(newPass2);
					if(GUILayout.Button("Change Password 2")){
						sc2.password = newPass2;
					}
					GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		GUILayout.EndArea();
		//show that a new message has been received
		GUILayout.BeginArea(new Rect(0,Screen.height/2,Screen.width,30));
		GUILayout.BeginHorizontal();
		GUILayout.BeginHorizontal("box");
		GUILayout.FlexibleSpace();
		if(Time.time-rt1<3){
			GUILayout.Label("New Message");
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal("box");
		GUILayout.FlexibleSpace();
		if(Time.time-rt2<3){
			GUILayout.Label("New Message");
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
		//draw the chat areas
		displayChat(new Rect(0,70,Screen.width/2,Screen.height-70),sc,sp,n1);
		displayChat(new Rect(Screen.width/2,70,Screen.width/2,Screen.height-70),sc2,sp2,n2);
	}

	//helper function to display the SimChat
	public void displayChat(Rect area,SimChat sc,Vector2 sp,string sender){
		sp.y = Mathf.Infinity;
		GUILayout.BeginArea(area);
		GUILayout.BeginHorizontal("box");
			GUILayout.Label("Name: "+sender);
		GUILayout.EndHorizontal();
		GUILayout.BeginVertical("box");
		sp = GUILayout.BeginScrollView(sp);
		Color c = GUI.contentColor;
		//loop through each of the messages contained in allMessages
		GUI.skin.label.wordWrap = true;
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
		GUILayout.BeginHorizontal();
		//send a new message
		sc.message = GUILayout.TextField(sc.message);
		if(GUILayout.Button("Send")){
			//filter words from the string
			wf.cleanString(ref sc.message);
			sc.sendMessage();
			sc.message = "";
		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
}