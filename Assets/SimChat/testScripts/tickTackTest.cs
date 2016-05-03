using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class tickTackTest : MonoBehaviour {
	bool initial = true;
	string groupName = "groupName";
	string groupPass = "groupPass";
	string name1 = "Player 1";
	string name2 = "Player 2";
	networkTicTacToe ttt1,ttt2;
	
	void OnGUI(){
		GUILayout.BeginArea(new Rect(0,0,Screen.width,Screen.height));
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if(initial){ //get the group information and player names
			GUILayout.BeginVertical("box");
				GUILayout.BeginHorizontal();
					GUILayout.Label("Group Name:");
					groupName = GUILayout.TextField(groupName);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
					GUILayout.Label("Group Password:");
					groupPass = GUILayout.TextField(groupPass);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
					GUILayout.Label("Name 1:");
					name1 = GUILayout.TextField(name1);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
					GUILayout.Label("Name 2:");
					name2 = GUILayout.TextField(name2);
				GUILayout.EndHorizontal();
				if(GUILayout.Button("Submit")){
					//initialize the tic tac toe objects
					ttt1 = new networkTicTacToe(groupName,groupName,gameObject.GetComponent<MonoBehaviour>(),name1);
					ttt2 = new networkTicTacToe(groupName,groupName,gameObject.GetComponent<MonoBehaviour>(),name2);
					initial = false;
				}
			GUILayout.EndVertical();
		}else{
			//show the tic tac toe
			ttt1.draw();
			ttt2.draw();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.EndArea();
	}
}

public class networkTicTacToe{
	SimChat sc; //the simple chat object
	bool myTurn = true;
	bool gameOver = false;
	//first infor is used to ignore any messages that are initially received (such as in a repeat game);
	bool firstInfo = true;
	int[] boardState = {0,0,0,0,0,0,0,0,0};
	string[] xos = {"\t\t\t","","","","","","","","\t\t\t"};
	int clicked = -1;
	List<int[]> victoryPaths = new List<int[]>();
	
	public networkTicTacToe(string group,string password,MonoBehaviour mono,string name){
		//initialize the SimChat
		sc = new SimChat("%"+group,password,mono,name); // "%" is used so people using the example online can not spam a message stream (unless the message stream group starts with % and they knew that)
		//Continue to check for new messages
		sc.continueCheckMessages();
		//set the receive function
		sc.setReceiveFunction(receive);
		//used as a starting message, used to ensure that firstInfo is triggered 
		sc.sendMessage("Start");
		//set the delay between checking for new messages, default is 0.5;
		sc.delay = 0.1f;
		
		//paths to check for victory
		victoryPaths.Add(new int[]{0,1,2});
		victoryPaths.Add(new int[]{3,4,5});
		victoryPaths.Add(new int[]{6,7,8});
		victoryPaths.Add(new int[]{0,3,6});
		victoryPaths.Add(new int[]{1,4,7});
		victoryPaths.Add(new int[]{2,5,8});
		victoryPaths.Add(new int[]{0,4,8});
		victoryPaths.Add(new int[]{6,4,2});
	}
	
	protected void receive(SimpleMessage[] sma){
		//if this is the first set of messages, ignore it
		if(firstInfo){
			firstInfo = false;
			return;
		}
		//make sure it a single message
		if(sma.Length==1){
			//check if the sender was not me
			if(sma[0].sender != sc.senderName){
				//check if it is a move
				if(sma[0].message.IndexOf("move:")==0){
					//set the move
					int otherMove = int.Parse(sma[0].message.Substring(5));
					move(otherMove,false);
					myTurn = true;
				}
			}
		}
	}
	
	//place an X or O at a board position
	public void move(int pos,bool useX){
		//set the board information
		boardState[pos] = useX?1:2;
		xos[pos] = useX?"X":"O";
		//check for victory
		gameOver = checkForVictory();
	}
	
	//check if any of the victory conditions have been met 
	public bool checkForVictory(){
		foreach(int[] vp in victoryPaths){
			if(boardState[vp[0]]!=0&&boardState[vp[0]]==boardState[vp[1]]&&boardState[vp[0]]==boardState[vp[2]]){
				return true;
			}
		}
		return false;
	}
	
	
	public void draw(){
		draw(50);
	}
	
	public void draw(int fontSize){
		GUILayout.BeginVertical("box");
		GUI.skin.button.fontSize = fontSize;
		//Show the name, and whose turn
		GUILayout.Label(sc.senderName+": "+(myTurn?"My Turn":"Their Turn"));
		//show the board
		clicked = GUILayout.SelectionGrid(clicked,xos,3);
		if(gameOver){ //show reset on Game Over
			GUILayout.Label("Game Over");
			if(GUILayout.Button("Reset")){
				boardState = new int[]{0,0,0,0,0,0,0,0,0};
				xos = new string[]{"\t\t\t","","","","","","","","\t\t\t"};
				gameOver = false;
				myTurn = true;
			}
		}
		GUILayout.EndVertical();
		//check if there was a valid board selection
		if(clicked != -1){
			if(myTurn && boardState[clicked]==0){
				//make move
				sc.sendMessage("move:"+clicked.ToString());
				move(clicked,true);
				myTurn = false;
			}
			clicked = -1;
		}
	}
}