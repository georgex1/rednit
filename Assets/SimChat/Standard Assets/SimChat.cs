using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * \mainpage Simple Chat
 * \section s1 Introduction
 * \tableofcontents
 * The SimChat class allows for a simple way to implement a chat message system which does not require the hosting of a server and only uses the <a href="http://docs.unity3d.com/ScriptReference/WWW.html">WWW</a> unity3d class. By only using the WWW class the message system should work on any of the build options for Unity3d (this has only been tested on Windows, Web and Android) and not interfere with any other networking already implemented.
 * \subsection ss1 Notice
 * Do not discuss any private information over the chat system. Although it is unlikely that anyone would be able to see you messages, the network itself has no encryption.
 * \section s2 How To
 * This is a short example on how to use the SimChat class. Most of this will be written with a C# focus, however JS will work also.
 * \subsection ss2 Installation
 * In order to use the SimChat class, simply have the wwwChat.dll file in your project's asset folder or subfolders.
 * \subsection ss3 Declaration
 * A new SimChat object can be declared with or without the sender name stirng.
 * \n C#
 * \code 
 * sc = new SimChat("default",gameObject.GetComponent<MonoBehaviour>(),"myName"); 
 * sc = new SimChat("default",gameObject.GetComponent<MonoBehaviour>()); 
 * \endcode
 * JS
 * \code
 * sc = new SimChat("default",gameObject.GetComponent(MonoBehaviour),"myName");
 * sc = new SimChat("default",gameObject.GetComponent(MonoBehaviour));
 * \endcode
 * If you do not set the name value, you may specify a name when sending each message or the default value of "Me" will be used. You may also set the name later using #SimChat.senderName.
 * \subsection ss4 Start
 * Even though a new Object of SimChat was created it will not check for new messages without being intructed.
 * \code
 * sc.getNewMessages();
 * \endcode
 * or
 * \code
 * sc.continueCheckMessages();
 * \endcode
 * Both commands tell the object to poll the server for new messages, however #SimChat.continueCheckMessages() will force a continuous polling, while SimChat.getNewMessages() will only continue to poll if #SimChat.continueToCheck is TRUE, which is the default.
 * \subsection ss5 Sending A message
 * To send a message to the chat group, use one of the SimChat.sendMessage() functions. Depending on which version of the function, you can send with the previously defined sender name and #SimChat.message values, or pass your own to the function.
 * \code
 * sc.sendMessage("senderName","message");
 * sc.sendMessage("message");
 * sc.sendMessage();
 * //this will only send the "senderName" and "message" values to the server, and is only useful for resending a received message.
 * sc.sendMessage(new SimpleMessage(00001,"senderName","message")); 
 * \endcode
 * \subsection ss6 Receiving Messages
 * After polling the server for new messages, any messages received will be put into the #SimChat.allMessages variable for reading.
 * \subsubsection sss6 Displaying Messages
 * Displaying the messages is really up to you, but the most simple way is inside of a <a href="http://docs.unity3d.com/ScriptReference/GUI.BeginScrollView.html">scrollView</a> in a for loop.
 * \code
 * //Vector2 sp = Vector2.zero;
 * sp = GUILayout.BeginSrollView(sp);
 * foreach(SimpleMessage s in sc.allMessages){
 * 	GUILayout.Label(s.sender+": "+s.message);
 * }
 * GUILayout.EndScrollView();
 * \endcode
 * \subsection ss7 More
 * \subsubsection sss7 Server Delay
 * #SimChat.delay controls the delay between receiving a response from the server, and when it is polled again. The default value is 0.5s, and can be set to any positive float value (including zero). Decreasing the delay will result in a better response time for the chat messages, but also use more resources.  Reducing the delay to zero will not cause the response to be instantaneous, because the system will still need to wait for a response from the server before making a new request.
 * \code
 * sc.delay = 0.2f;
 * \endcode
 * \subsubsection sss8 Stop Checking
 * By default if you call SimChat.getNewMessages() or SimChat.continueCheckMessages() the #SimChat.continueCheck variable is set to TRUE. In order to stop polling the server, simply set SimChat.continueCheck to false. Afterward using SimChat.getNewMessages() will only poll the server once.
 * \code
 * sc.continueCheck = false;
 * \endcode
 * \subsubsection sss9 Max Messages
 * Setting the SimChat.maxMessages to a value greater than zero will keep the SimChat.allMessages list to that length or less.
 * \code
 * sc.maxMessages = 25;
 * \endcode
 * \subsubsection sss10 Getting Chat Members
 * In order to obtain the names of the other users in the same chat group you can either parse through the list of SimChat.allMessages or poll the server to parse through the messages that are currently available. SimChat.getUniqueSenders() parses through the current list of SimChat.allMessages and returns a list of strings that is the unique senders contained within. Calling the SimChat.getSendersFromServer() function polls the server to search through the messages that are currently on the server and return the unique values.
 * \code
 * sc.groupSenders = sc.getUniqueSenders();
 * \endcode
 * \code
 * sc.getSendersFromServer();
 * \endcode
 * Neither solution will obtain a perfect list of currently "connected" group members, especially if the user never sends a message.
 * \subsubsection sss11 Change Chat Group
 * The chat group you are in is set when the SimChat object is initialized, however when the chat group can be changed by calling SimChat.changeIdentifier().
 * \code
 * sc.changeIdentifier("newGroup",true);
 * \endcode
 * \section sb Back-end
 * The server that is performing the chat function is running on the Google Apps Engine platform. The messages are sent to the server where they are temporarely stored in cache. Each of the clients is responsible for polling the server to ask for any new messages. The messages are removed after a period of time or to free up memory, as a result the exact amount of time a message will remain on the server is unclear, and should not be relied upon as storage.
 * \section se Example
 * This example is included in with the Unity Package and shows a basic setup and display of chatting.
 * \include testTwoChat.cs
 */

/**
 The SimChat class is the basis for sending and receiving messages.
 */
public class SimChat{
	//public
	/** Stores the message to be sent, erased automatically when sent. */
	public string message = "";
	/** A list of all of the messages that have been received */
	public List<SimpleMessage> allMessages = new List<SimpleMessage>();
	/** An array of all unique sender names. */
	public string[] groupSenders = new string[]{};
	/** Used to set/get the current sender name value */
	public string senderName{get{return scn.name;}set{scn.name=value;}}
	/** Used to get and set the password */
	public string password{get{return scn.chatPassword;}set{scn.chatPassword=value;groupSenders = new string[]{};allMessages = new List<SimpleMessage>();}}
	/** used to set/get the #delayTime. The value passed is clamped to be => 0. */
	public float delay{get{return delayTime;}set{delayTime=(value>0)?value:0;}}
	/** Used to set/get the #messageLength. */
	public int maxMessages{get{return messageLength;}set{messageLength=value;}}
	/** Get/Set the #continueToCheck value. */
	public bool continueCheck{get{return continueToCheck;}set{continueToCheck=value;}}
	//private / protected
	private SimChatNetwork scn;
	/** The max number of messages to hold in the #allMessages list. This variable is set using the #maxMessages variable. */
	protected int messageLength = 0;
	/** The time delay set between polls of the server, in seconds. Reducing the #delay time will increase responsiveness, however may reduce performance. */
	protected float delayTime = 0.5f; 
	/** Used to determine if a new poll request should be made after the previous request is received. This variable can be changed by using #continueCheck */
	protected bool continueToCheck = true;
	/** <a href="http://docs.unity3d.com/ScriptReference/MonoBehaviour.html">MonoBehaviour</a> passed to the function. */
	protected MonoBehaviour mb;
	/** Function to call when a new message has been received. */
	protected System.Action<SimpleMessage[]> receiveFunction;
	/** Boolean to determine if #receiveFunction has been set. */
	protected bool receiveFunctionSet = false;
	/** Queue to hold messages to be sent. Used when there is a network error to save the message. */
	protected Queue<SimpleMessage> messageQueue = new Queue<SimpleMessage>();
	/** Used to determine if #allMessages should be cleared, after clearing the server. */
	protected bool clearLocalMessages = false;

	/** Constructor for SimChat which expects and identifier,password,MonoBehaviour,senderName. The identifier is the chat room to check, which means that if you want to chat with someone else they need to have the same value for the identifier and password. The MonoBehaviour is required to use the <a href="http://docs.unity3d.com/ScriptReference/WWW.html">WWW</a> class. The sender name is the name to use if only a message is given to send.
		\code
		C#
		void Start(){
			sc = new SimChat("default","password",gameObject.GetComponent<MonoBehaviour>(),"myName");
		}
		JS
		function Start(){
			sc = new SimChat("default","password",gameObject.GetComponent(MonoBehaviour),"myName");
		}
		\endcode
	 */
	public SimChat(string identifier,string password,MonoBehaviour currentMonoBehaviour,string senderName){
		mb = currentMonoBehaviour;
		replaceDeliminator(ref identifier);
		replaceDeliminator(ref senderName);
		scn = new SimChatNetwork(identifier,password,currentMonoBehaviour,senderName);
		scn.setOutputFunction(receiveMessage);
		scn.setReceiveNameFunction(receiveSenders);
		scn.setErrorFunction(networkError);
		scn.setClearFunction(clearComplete);
	}
	/** Constructor which does not require a password, and defaults to "" */
	public SimChat(string identifier,MonoBehaviour currentMonoBehaviour,string senderName):this(identifier,"",currentMonoBehaviour,senderName){}
	/** Constructor which does not require a sender name, and defaults to "Me" */
	public SimChat(string identifier,string password,MonoBehaviour currentMonoBehaviour):this(identifier,password,currentMonoBehaviour,"me"){}
	/** Constructor which does not require a name or password, and sets the default name to be "Me" and the password to ""*/
	public SimChat(string identifier,MonoBehaviour currentMonoBehaviour):this(identifier,currentMonoBehaviour,"Me"){}
	/** Constructor which does not require an identifier, which defaults to "default" */
	public SimChat(MonoBehaviour currentMonoBehaviour):this("default",currentMonoBehaviour){}

	/** Changes the current identifier value. This essentially changes the current chat room. Doing this will also erase #allMessages and #groupSenders by default. */
	public void changeIdentifier(string newIdentifier){
		scn.changeKey(newIdentifier);
		groupSenders = new string[]{};
		allMessages = new List<SimpleMessage>();
	}
	/** Used to set a function to call when a new message is received. */
	public void setReceiveFunction(System.Action<SimpleMessage[]> rf){
		receiveFunction = rf;
		receiveFunctionSet = true;
	}
	/** Changes the current identifier, but only erases #allMessages and #groupSenders if the boolean is TRUE. */
	public void changeIdentifier(string newIdentifier,bool b){
		scn.changeKey(newIdentifier);
		if(b){
			groupSenders = new string[]{};
			allMessages = new List<SimpleMessage>();
		}
	}
	/** Polls the server for the new messages by calling getNewMessages(), and sets #continueCheck to TRUE. */
	public void continueCheckMessages(){
		continueCheck=true;
		getNewMessages();
	}
	/** Used to perform a blockin connection test. This function block the thread it is called in, it is not recommended to perform the check often.
	 * \returns TRUE -> Connection
	 * \returns FALSE -> No Connection
	  */
	public bool checkConnection(int timeoutInMilliseconds){	
		return scn.checkConnection(timeoutInMilliseconds);
	}
	/** Performs a connection test with a timeout of 1000 milliseconds. */
	public bool checkConnection(){	return checkConnection(1000);	}
	/** Function called to poll the server for new messages. The poll to the server is delayed by the #delayTime. */
	public void getNewMessages(){
		mb.StartCoroutine(waitRequestMessages(delayTime));
	}
	/** Function called when new messages are received from the server. */
	protected void receiveMessage(SimpleMessage[] newMessages){
		if(newMessages.Length > 0){
			allMessages.AddRange(newMessages);
			if(messageLength>0 && allMessages.Count>messageLength)
				allMessages.RemoveRange(0,allMessages.Count-messageLength);
			if(receiveFunctionSet)
				receiveFunction(newMessages);
		}
		//send the queued messages
		sendAvailableMessage(true);

		if(continueToCheck)
			getNewMessages();
	}
	/** Used to send the request to the server to get new messages, with a delay */
	protected IEnumerator waitRequestMessages(float wait){
		if(wait > 0f)
			yield return new WaitForSeconds(wait);
		scn.getChatText();
	}
	/** Function to call to poll the server to generate a unique list of senders and send it back. */
	public void getSendersFromServer(){scn.getSendersFromServer();}
	/** The function that is called when a list of senders is received from the server. */
	protected void receiveSenders(string[] sndrs){
		groupSenders = sndrs;
		string ms = "";
		foreach(string s in sndrs){ms+=s+",";}
		Debug.LogWarning(ms);
	}
	/** Sends the SimpleMessage sm to the server using the message and sender contained within. */
	public void sendMessage(SimpleMessage sm){	
		sendAvailableMessage(scn.addChatText(sm));	
	}
	/** Sends the value of the message passed to it, with the sender name of sender. */
	public void sendMessage(string sender,string mess){	
		sendAvailableMessage(scn.addChatText(sender,mess));	
	}
	/** Sends the value of the string passed to it to the server using the previously defined #senderName */
	public void sendMessage(string mess){	
		sendAvailableMessage(scn.addChatText(mess));	
	}
	/** Sends the value of #message to the server, using the previously defined #senderName */
	public void sendMessage(){	
		sendAvailableMessage(scn.addChatText(message));	
	}

	/** Function called to send the next message in #messageQueue */
	protected void sendAvailableMessage(bool okay){
		if(okay && messageQueue.Count > 0){
			sendMessage(messageQueue.Dequeue());
		}
	}

	/** Function to clear all the messages on the server for the current chat group. This may also clear all of the messages in the allMessages array.
	 * \param clearLocal Clear the local message array, #allMessages
	 */
	public void clearServerMessages(bool clearLocal){
		continueCheck = false;
		clearLocalMessages = clearLocal;
		mb.StartCoroutine(waitClearServerMessages());
	}
	/** Function used to wait for the server to not be busy */
	protected IEnumerator waitClearServerMessages(){
		while(scn.isBusy){
			Debug.Log (scn.isBusy);
			yield return new WaitForSeconds(0.25f);
		}
		scn.clearServerMessages();
	}

	/** Function called after the server clears messages. */
	protected void clearComplete(bool success){
		if(clearLocalMessages){
			clearLocalMessages = false;
			clearMessages();
		}
		continueCheck = true;
	}

	/** Clear the local message list, #allMessages */
	public void clearMessages(){
		allMessages = new List<SimpleMessage>();
	}
	 
	/** Returns a list of unique sender names from a List<SimplMessage> */
	public static List<string> uniqueSenderValues(List<SimpleMessage> messageList){
		List<SimpleMessage> sms = new List<SimpleMessage>(messageList);
		sms.Sort();
		string previousName = "";
		List<string> o = new List<string>();
		foreach(SimpleMessage s in sms){
			if(s.sender!=previousName){
				o.Add(s.sender);
				previousName = s.sender;
			}
		}
		return o;
	}
	/** Issues a warning, and returns the string with diliminator replaced */
	protected void replaceDeliminator(ref string s){
		if(s.IndexOf('`') >=0 ){
			Debug.LogWarning("The '`' character is used as a deliminatord and will be replaced with '''");
			s.Replace('`','\'');
		}
	}
	/** Returns a list of unique sender names from all of the current messages. */
	public List<string> getUniqueSenders(){return uniqueSenderValues(allMessages);}

	/** Function called when there is an error with the network */
	protected void networkError(int t,string s,string s2){
		string o = "A network error has occured\n\t";
		switch(t){
			case 0://Error when checking connection
				o += "Not Connected";
				break;
			case 1://error when sending a message
				o += "Saving Message In Queue";
				messageQueue.Enqueue(new SimpleMessage(s,s2));
				break;
			case 2://error when receiving a message
				o += "Message Not Received\r\n"+s;
				break;
			case 3://error when getting list of senders
				o += "Senders Not Received\r\n"+s;
				break;
			case 4://error when getting list of senders
				o += "Error When Clearing\r\n"+s;
				break;
			default:
				break;
		}
		Debug.LogWarning(o);
	}

	/** Class to perform all of the networking functions for the chat */
	private class SimChatNetwork{
		//protected static string url = "http://simple-chat-pro.appspot.com/";
		protected static string url = "http://haakapp.com/response/chat/";
		protected static string[] splt = new string[] {"\r\n"};
		//protected static char deliminator = '`';
		private static string[] senders = new string[]{};
		public static string[] getSenders{get{return senders;}}
		public bool tooLong{get{return ((key+"`"+password).Length > 245);}}
		//protected const int latestTime = 0,sentText = 1,recieveText = 2,recieveLatest = 3,recieveSenders = 4;
		protected long latest = 0;
		protected string key = "";
		protected string password = "";
		protected SimpleMessage[] receiveMessages;
		protected MonoBehaviour mb;
		public string name = "nobody";
		protected System.Action<SimpleMessage[]> outMessageFunk; //output function to call when messages are received
		protected bool outputFunction = false;
		protected System.Action<int,string,string> errorReceived; //function to call if an error occurs (the strings are used to return the failed message, if that occured)
		protected bool errorFunction = false;
		protected System.Action<string[]> receiveSenderNames;
		protected bool senderFunctionSet = false;
		protected System.Action<bool> clearServer;
		protected bool clearFunctionSet = false;
		protected bool busy = false;
		public bool isBusy{get{return busy;}}
		
		public SimChatNetwork(string identifier,MonoBehaviour currentMonoBehaviour){key=identifier;mb=currentMonoBehaviour;checkLongKey();}
		public SimChatNetwork(string identifier,string pass,MonoBehaviour currentMonoBehaviour){key=identifier;mb=currentMonoBehaviour;password=pass;checkLongKey();}
		public SimChatNetwork(string identifier,MonoBehaviour currentMonoBehaviour,string senderName):this(identifier,currentMonoBehaviour){name=senderName;}
		public SimChatNetwork(string identifier,string pass,MonoBehaviour currentMonoBehaviour,string senderName):this(identifier,pass,currentMonoBehaviour){name=senderName;}

		public void setOutputFunction(System.Action<SimpleMessage[]> outFunk){
			outputFunction = true;
			outMessageFunk = outFunk;
		}
		
		public void setReceiveNameFunction(System.Action<string[]> senderFunk){
			senderFunctionSet = true;
			receiveSenderNames = senderFunk;
		}

		public void setErrorFunction(System.Action<int,string,string> errorFunk){
			errorFunction = true;
			errorReceived = errorFunk;
		}

		public void setClearFunction(System.Action<bool> clearFunk){
			clearFunctionSet = true;
			clearServer = clearFunk;
		}
		
		public void changeKey(string newKey){
			latest = 0;
			key = newKey;
			senders = new string[]{};
		}
		//get and set the password
		public string chatPassword{get{return password;}set{latest=0;password=value;senders = new string[]{};}}

		public void checkLongKey(){
			if(tooLong)
				Debug.LogWarning("Your key+password combination should be shorter than 245 characters. Combinations longer than that may slightly reduce performance.");
		}

		public bool checkConnection(int timeoutMilliseconds){
			WWW www = new WWW(url+"checkConnection.php");
			System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
			stopWatch.Start ();
			while(!www.isDone){
				if(stopWatch.ElapsedMilliseconds > timeoutMilliseconds)
					return false;
			}
			stopWatch.Stop();
			if(www.error == null)
				return true;
			return false;
		}
		
		public bool addChatText(SimpleMessage sm){	return addChatText(sm.sender,sm.message);	}
		public bool addChatText(string message){	return addChatText(name,message);		}
		public bool addChatText(string sender,string message){
			WWWForm form = new WWWForm();
			form.AddField("id", key);
			form.AddField("pass", password);
			form.AddField("msg", message);
			form.AddField("sndr", sender);
			busy = true;
			mb.StartCoroutine(waitForAddChatText(new WWW(url+"postNewLine.php", form),sender,message));
			return true;
		}

		protected IEnumerator waitForAddChatText(WWW www,string sender,string message){
			yield return www;
			busy = false;
			if(www.error == null){
				//success
			}else{
				if(errorFunction)
					errorReceived(1,sender,message);
			}
		}

		public void getAllChatText(){
			WWWForm form = new WWWForm();
			form.AddField("id", key);
			form.AddField("pass", password);
			busy = true;
			mb.StartCoroutine(waitForGetChat(new WWW(url+"getChat.php", form)));
		}
		
		public void getChatText(){
			WWWForm form = new WWWForm();
			form.AddField("id", key);
			form.AddField("pass", password);
			form.AddField("tm",latest.ToString());
			busy = true;
			mb.StartCoroutine(waitForGetChat(new WWW(url+"getChat.php", form)));
		}

		protected IEnumerator waitForGetChat(WWW www){
			yield return www;
			busy = false;
			if(www.error==null){
				receiveMessages = parseChatText(www.text);
				if(receiveMessages.Length>0 && www.text.Length>0)
					latest = receiveMessages[receiveMessages.Length-1].time;
				if(outputFunction)
					outMessageFunk(receiveMessages);
			}else{
				if(errorFunction)
					errorReceived(2,www.error,"");
			}
		}
		
		public void getSendersFromServer(){
			WWWForm form = new WWWForm();
			form.AddField("id", key);
			form.AddField("pass", password);
			busy = true;
			mb.StartCoroutine(waitForGetSenders(new WWW(url+"getChatSender.php", form)));
		}

		protected IEnumerator waitForGetSenders(WWW www){
			yield return www;
			if(www.error==null){
				senders = www.text.Split('`');;
				if(senderFunctionSet)
					receiveSenderNames(senders);
			}else{
				if(errorFunction)
					errorReceived(3,www.error,"");
			}
			busy = false;
		}

		public void clearServerMessages(){
			WWWForm form = new WWWForm();
			form.AddField("id",key);
			form.AddField("pass",password);
			mb.StartCoroutine(waitForClearServerMessages(new WWW(url+"clearMessages.php", form)));
		}

		protected IEnumerator waitForClearServerMessages(WWW www){
			yield return www;

			if(www.error==null){
				if(clearFunctionSet)
					clearServer(true);
			}else{
				if(clearFunctionSet)
					clearServer(false);
				if(errorFunction)
					errorReceived(4,www.error,"");
			}
		}
		
		public static SimpleMessage[] parseChatText(string s){
			string[] lines = s.Split(SimChatNetwork.splt,System.StringSplitOptions.RemoveEmptyEntries);
			SimpleMessage[] o = new SimpleMessage[lines.Length];
			for(int l = 0;l<lines.Length;l++){	o[l] = new SimpleMessage(lines[l]);	}
			return o;
		}
	}
	
}

/** Class used to contain a message from the server. This class is extended from IComparable to allow for sorting by the senders name, for use when extracting only unique sender names from a list of messages. */
public class SimpleMessage:System.IComparable<SimpleMessage>{
	/** The time the message was sent. This is usually represented as the number of milliseconds since epoc. */
	public long time = 0;
	/** The name of the sender. */
	public string sender = "me";
	/** The message. */
	public string message = "";

	/** Constructor which expects the three values of time,sender,message \code SimpleMessage sm = new SimpleMessage(00001,"my name","my message"); \endcode */
	public SimpleMessage(long t,string s,string m){
		time = t;
		sender = s;
		message = m;
	}
	/** Constructor which expects only sender and message \code SimpleMessage sm = new SimpleMessage("my name","my message"); \endcode */
	public SimpleMessage(string s,string m){
		sender = s;
		message = m;
	}
	/** Constructor which expects a single string separated by a ",". Time,sender,message \code SimpleMessage sm = new SimpleMessage("00001,my name,my message"); \endcode */
	public SimpleMessage(string mes){
		string[] mess = mes.Split('`');
		if(!long.TryParse(mess[0],out time))
			time = -1;
		sender = mess[1];
		message = mess[2];
	}
	
	/** Compares two SimpleMessage objects by comparing the sender string */
	public int CompareTo(SimpleMessage other){
		return sender.CompareTo(other.sender);
	}
	/** Returns the string values separated by ':' */
	public override string ToString(){
		return time.ToString()+":"+sender+":"+message;
	}
}