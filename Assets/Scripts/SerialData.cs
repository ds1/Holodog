using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

public class Buffer: MonoBehaviour { 
	public int packetSize = 5; // this the perfect size
	public int readSize = 100;
	public int bufferSize = 1000;
	private byte[] data;
	private int startIndex;
	private int endIndex;
	private int count {
		get {
			return endIndex - startIndex;
		}
	}
	private bool hasRoomToRead {
		get {
			return (count - endIndex) < 0;
		}
	}

	public Buffer() {
		data = new byte[bufferSize];
	}

	public void sinkSerialPort(SerialPort sp) {
		resetBufferIfNeeded ();
		var bytesRead = sp.Read (data, endIndex, 100);
		endIndex += bytesRead;
	}

	private void resetBufferIfNeeded() {
		if (hasRoomToRead) { 
			startIndex = 0; 
			endIndex = 0;
		}
	}

	public int popValue() {
		if (count > packetSize) {
			var copiedBytes = copyBytesOfPacketSize (startIndex);
			startIndex += packetSize;
			string unicodeString = System.Text.Encoding.ASCII.GetString (copiedBytes);
			print(unicodeString);
			var unicodeValue = int.Parse (unicodeString); 
			return unicodeValue;
		} 
		else {
			return -1;
		}
	}

	private byte[] copyBytesOfPacketSize(int start) {
		var copiedBytes = new byte[packetSize];
		for(int i = 0; i < packetSize; i++) {
			copiedBytes[i] = data[start + i];
		}
		return copiedBytes;
	}
}

public class SerialData : MonoBehaviour {
	//Setup parameters to connect to Arduino
	//dan's computer port is "/dev/cu.usbmodem1411"
	public static SerialPort sp = new SerialPort ("/dev/cu.usbmodem1411", 9600, Parity.None, 8, StopBits.One);
	public float updatePeriod = 0.0f;
	//public GameObject pushObject;
	//private float lastUpdate;
	public List<MovieTexture> tooNearMovieTextures;
	public List<MovieTexture> tooFarMovieTextures;
	public List<MovieTexture> idleMovieTextures;
	public float countdownTimer = 4.0f;
	private float originalTimer;
	public bool timerHasEnded { 
		get {
			return countdownTimer < 0;
		}
	}
	private Buffer buffer = new Buffer ();

	// Use this for initialization
	void Start () {
		OpenConnection ();
		originalTimer = countdownTimer;
	}

	void Update () {
		//StartCoroutine(ReadInfo);
		// lookl for delimited
		// read 6 bytes before delimiter 
		tickTimer();
		buffer.sinkSerialPort (sp);
		var distanceValue = buffer.popValue();
		if (timerHasEnded) {
			if (distanceValue > 0 && distanceValue <= 100) {
				if (distanceValue >= 50) {
					print ("far" + distanceValue);
					PlayFarMovieIfPossible ();
				} else if (distanceValue > 0 && distanceValue < 14) {
					print ("close" + distanceValue);
					PlayCloseMovieIfPossible ();
				} else {
					print ("middle" + distanceValue);
					PlayMiddleMovieIfPossible ();
				}
			}
		} else { 
			print ("middle");
			PlayMiddleMovieIfPossible ();
		}
	}

	private void tickTimer() {
		countdownTimer -= Time.deltaTime;
	}

	//Function connecting to Arduino
	public void OpenConnection () {
		if (sp != null) {
			if (sp.IsOpen) {
				sp.Close ();
				print ("Closing port, because it was already open!");
				//message = "Closing port, because it was already open!";
			}
			else {
				sp.Open ();  // opens the connection
				sp.ReadTimeout = 1000;  // sets the timeout value before reporting error
				print ("Port Opened!");
				//		message = "Port Opened!";
			}
		} 
		else {
			if (sp.IsOpen) {
				print ("Port is already open");
			} else {
				print ("Port == null");
			}
		}
	}

	void OnApplicationQuit () {
		sp.Close ();
	}
		
	void PlayFarMovieIfPossible() {
		var didPlay = PlayMovieIfPossible (tooFarMovieTextures);
		if (didPlay) {
			resetTimer ();
		}
	}

	void PlayCloseMovieIfPossible() {
		var didPlay = PlayMovieIfPossible (tooNearMovieTextures);
		if (didPlay) {
			resetTimer ();
		}
	}

	void PlayMiddleMovieIfPossible() {
		PlayMovieIfPossible (idleMovieTextures);
	}
		
	bool PlayMovieIfPossible (List<MovieTexture> movieTextureList) {
		Renderer r = GetComponent<Renderer> ();
		MovieTexture currentMovie = r.material.mainTexture as MovieTexture;
		if (!currentMovie.isPlaying) { 
			int count = movieTextureList.Count;
			int randomIndex = Random.Range (0, count - 1);
		
			MovieTexture movie = movieTextureList [randomIndex];
			AudioSource audioSource = GetComponent<AudioSource> ();

			audioSource.clip = movie.audioClip;
			r.material.mainTexture = movie;

			movie.Stop ();
			audioSource.Stop ();	
			
			movie.Play ();
			audioSource.Play ();
			return true;
		} 
		return false;
	}

	void resetTimer() {
		countdownTimer = originalTimer;
	}
}
	