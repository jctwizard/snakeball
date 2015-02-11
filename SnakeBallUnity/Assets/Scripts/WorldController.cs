using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WorldController : MonoBehaviour 
{
	public GameObject controlBall;
	public float ballRadius;
	public GameObject apple;
	public float appleRadius;
	
	public float baseSpeed;
	private float speed = 2;

	private int score = 0;
	private int highscore = 0;

	private float rotation = 0;
	private Vector2 velocity = new Vector2(0, 0);

	public GameObject head;
	public float headRadius;
	private ArrayList bodyParts = new ArrayList();
	public GameObject bodyPrefab;
	public GameObject nodePrefab;
	public int initialParts;
	public int applePartBonus;

	private bool paused = false;
	private bool gameOver = false;
	private bool hiscores = false;

	private bool lightTheme = true;
	
	public Image background;
	public Text scoreText;
	public Text creditText;
	public Text retryText;
	public Text pauseText;
	public Text hiscoreText;
	public Text nameText;
	public Image hiscorePanel;
	public Text errorText;
	public Text loadingText;
	public Text snakeballText;
	public Image leftButton;
	public Image rightButton;
	public InputField nameInput;
	public GameObject hiscoreList;
	public GameObject entryPrefab;

	public Color blueColour;
	public Color noColour;
	public Color highlightColour;

	void Start() 
	{
		Screen.orientation = ScreenOrientation.Portrait;
		Application.targetFrameRate = 60;

		moveApple();
		addBodyPart(initialParts);
		retryText.gameObject.SetActive(false);

		highscore = PlayerPrefs.GetInt("highscore");
		scoreText.text = 0 + "/" + highscore;

		creditText.gameObject.SetActive(false);
		nameInput.gameObject.SetActive(false);
		errorText.gameObject.SetActive(false);
		loadingText.gameObject.SetActive(false);
		hiscorePanel.gameObject.SetActive(false);
		hiscoreList.gameObject.SetActive(false);
		nameText.gameObject.SetActive(false); 

		Debug.Log("id: " + PlayerPrefs.GetInt("id"));

		if (PlayerPrefs.GetInt("id") != 0)
		{
			StartCoroutine("SetHighscore");
		}
	}

	void FixedUpdate() 
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}

		if (gameOver)
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				for (int i = 0; i < bodyParts.Count; i++)
				{
					Destroy((GameObject)bodyParts[i]);
				}
				
				bodyParts.Clear();
				
				addBodyPart(initialParts);
				score = 0;
				
				retryText.gameObject.SetActive(false);
				pauseText.gameObject.SetActive(true);
				hiscoreText.gameObject.SetActive(true);
				
				scoreText.text = score + "/" + highscore;
				
				gameOver = false;
			}
		}

		else if (!paused)
		{
			bool left = Input.GetKey(KeyCode.LeftArrow) || Input.GetMouseButton(0) || LeftKeyboard();

			bool right = Input.GetKey(KeyCode.RightArrow) || Input.GetMouseButton(1) || RightKeyboard();

			if ((Input.touchCount > 0) && (Input.GetTouch(0).position.y < (Screen.height * 0.5f)))
			{
				left = Input.GetTouch(0).position.x < (Screen.width * 0.5f);
				right = Input.GetTouch(0).position.x > (Screen.width * 0.5f); 
			}

			if (Input.touchCount > 1)
			{
				if (Input.GetTouch(0).position.x < (Screen.width * 0.5f) && Input.GetTouch(1).position.x > (Screen.width * 0.5f))
				{
					left = true;
					right = true;
				}

				else if (Input.GetTouch(0).position.x > (Screen.width * 0.5f) && Input.GetTouch(1).position.x < (Screen.width * 0.5f))
				{
					left = true;
					right = true;
				}
			}

			if (sphereCollision(head, headRadius, apple, appleRadius))
			{
				addBodyPart(applePartBonus);
				moveApple();

				score++;

				if (score >= highscore)
				{
					highscore = score;
					PlayerPrefs.SetInt("highscore", highscore);

					if (PlayerPrefs.GetInt("id") != 0)
					{
						StartCoroutine("SetHighscore");
					}
				}
				
				scoreText.text = score + "/" + highscore;
			}

			moveBodyParts();

			if (left && right)
			{
				speed = baseSpeed + 1;
				leftButton.color = blueColour;
				rightButton.color = blueColour;
			}

			else if (left || right)
			{
				speed = baseSpeed;

				if (left)
				{
					rotation -= 0.1f;
					leftButton.color = blueColour;
					rightButton.color = noColour;
				}
				
				else if (right)
				{
					rotation += 0.1f;
					rightButton.color = blueColour;
					leftButton.color = noColour;
				}
			}

			else
			{
				leftButton.color = noColour;
				rightButton.color = noColour;
			}

			velocity.x = Mathf.Cos(rotation);
			velocity.y = Mathf.Sin(rotation);
			velocity.Scale(new Vector2(speed, speed));

			controlBall.transform.Rotate(velocity.y, velocity.x, 0);

			if (bodyCollision(head, headRadius) && (score > 0))
			{
				gameOver = true;

				leftButton.color = noColour;
				rightButton.color = noColour;

				retryText.gameObject.SetActive(true);
				pauseText.gameObject.SetActive(false);
				hiscoreText.gameObject.SetActive(false);
			}
		}
	}

	void addBodyPart(int count = 1)
	{
		for (int i = 0; i < count; i++)
		{
			GameObject newPart;

			if ((bodyParts.Count + 1) % 3 != 0)
			{
				newPart = GameObject.Instantiate(nodePrefab) as GameObject;
			}

			else
			{
				newPart = GameObject.Instantiate(bodyPrefab) as GameObject;
			}

			newPart.transform.position = new Vector3(0, 0, 0);

			bodyParts.Add(newPart);
		}
	}

	void moveBodyParts()
	{
		for(int index = bodyParts.Count - 1; index > 0; index--)
		{
			GameObject lastPart = bodyParts[index - 1] as GameObject;

			((GameObject)bodyParts[index]).transform.position = lastPart.transform.position;
		}

		((GameObject)bodyParts[0]).transform.position = head.transform.position;
	}

	void moveApple()
	{
		int loops = 0;

		do
		{
			Vector3 randCoord = Vector3.zero;

			while (randCoord == Vector3.zero)
			{
				if (loops > 5)
				{
					break;
				}

				randCoord = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
				randCoord.Normalize();
				randCoord.Scale(new Vector3(ballRadius, ballRadius, ballRadius));

				loops++;
			}

			apple.transform.position = randCoord;
		} while(bodyCollision(apple, appleRadius));
	}

	bool sphereCollision(GameObject a, float aR, GameObject b, float bR)
	{
		Vector3 displacement = new Vector3 (b.transform.position.x - a.transform.position.x, b.transform.position.y - a.transform.position.y, b.transform.position.z - a.transform.position.z);
		float distance = Mathf.Sqrt(displacement.x * displacement.x + displacement.y * displacement.y + displacement.z * displacement.z);
		float radius = aR;

		if (bR > aR)
		{
			radius = bR;
		}

		if (distance < radius)
		{
			return true;
		}

		else
		{
			return false;
		}
	}

	bool bodyCollision(GameObject collider, float colliderRadius)
	{
		for(int i = 5; i < bodyParts.Count - 1; i++)
		{
			if ((i + 1) % 3 != 0)
			{
				if(sphereCollision((GameObject)bodyParts[i], headRadius, collider, colliderRadius))
				{
					return true;
				}
			}
		}

		return false;
	}

	// Accessed from the credit's button component
	public void creditLink()
	{
		Application.OpenURL("http://www.twitter.com/jctwood");
	}

	// Accessed from snakeball button component
	public void switchTheme()
	{
		lightTheme = !lightTheme;

		Color newColor = Color.black;
		background.color = Color.white;

		if (!lightTheme)
		{
			newColor = Color.white;
			background.color = Color.black;
		}

		scoreText.color = newColor;
		retryText.color = newColor;
		snakeballText.color = newColor;
	}

	// Accessed from pause button component
	public void pauseButton()
	{
		paused = !paused;

		if (paused)
		{
			pauseText.text = "unpause";
			creditText.gameObject.SetActive(true);
			leftButton.color = noColour;
			rightButton.color = noColour;
		}

		else
		{
			pauseText.text = "pause";
			creditText.gameObject.SetActive(false);
			hiscoreText.gameObject.SetActive(true);
			hiscorePanel.gameObject.SetActive(false);
			errorText.gameObject.SetActive(false);
			loadingText.gameObject.SetActive(false);
			nameInput.gameObject.SetActive(false);
			hiscoreList.gameObject.SetActive(false);
			nameText.gameObject.SetActive(false);

			foreach (Transform entry in hiscoreList.transform) 
			{
				if (entry.gameObject.tag == "Entry")
				{
					Destroy(entry.gameObject);
				}
			}
		}
	}
	
	// Accessed from retry button component
	public void retryButton()
	{
		if (gameOver)
		{
			for (int i = 0; i < bodyParts.Count; i++)
			{
				Destroy((GameObject)bodyParts[i]);
			}
			
			bodyParts.Clear();
			
			addBodyPart(initialParts);
			score = 0;
			
			retryText.gameObject.SetActive(false);
			pauseText.gameObject.SetActive(true);
			hiscoreText.gameObject.SetActive(true);
			
			scoreText.text = score + "/" + highscore;
			
			gameOver = false;
		}
	}

	// Accessed from hiscore button component
	public void hiscoreButton()
	{
		hiscores = !hiscores;

		if (hiscores)
		{
			if (!paused)
			{
				pauseButton();
			}

			hiscorePanel.gameObject.SetActive(true);
			hiscoreText.gameObject.SetActive(false);
			creditText.gameObject.SetActive(false);
			nameText.gameObject.SetActive(true);

			if (PlayerPrefs.GetInt("id") == 0)
			{
				nameInput.gameObject.SetActive(true);
				nameInput.Select();
				nameInput.ActivateInputField();
			}

			else
			{
				loadingText.gameObject.SetActive(true);
				StartCoroutine("GetRank");
			}
		}
	}

	public void changeName()
	{
		hiscoreList.SetActive(false);

		foreach (Transform entry in hiscoreList.transform) 
		{
			if (entry.gameObject.tag == "Entry")
			{
				Destroy(entry.gameObject);
			}
		}

		nameInput.gameObject.SetActive(true);
		nameInput.Select();
		nameInput.ActivateInputField();
	}

	public void setName()
	{
		PlayerPrefs.SetString("name", nameInput.text);

		nameInput.gameObject.SetActive(false);
		
		if (PlayerPrefs.GetInt("id") == 0)
		{
			StartCoroutine("CreateHighscore");
		}

		else
		{
			StartCoroutine("SetHighscore");
			StartCoroutine("GetRank");
		}
	}

	IEnumerator CreateHighscore()
	{
		Debug.Log("Creating highscore");

		string name = PlayerPrefs.GetString("name");
		string createHighscoreURL = "http://www.snakeball.jctwood.uk/CreateHighscore.php?";
		WWW createHighscorePost = new WWW(createHighscoreURL + "name=" + WWW.EscapeURL(name) + "&score=" + WWW.EscapeURL(highscore.ToString()));

		yield return createHighscorePost;

		if (createHighscorePost.error == null)
		{
			PlayerPrefs.SetInt("id", System.Int32.Parse(createHighscorePost.text));
			
			loadingText.gameObject.SetActive(true);
			StartCoroutine("GetRank");
		}

		else
		{
			Error();
		}
	}

	IEnumerator SetHighscore()
	{
		Debug.Log("Setting highscore");

		string id = PlayerPrefs.GetInt("id").ToString();
		string name = PlayerPrefs.GetString ("name");
		string setHighscoreURL = "http://www.snakeball.jctwood.uk/SetHighscore.php?";
		WWW setHighscorePost = new WWW(setHighscoreURL + "id=" + WWW.EscapeURL(id) + "&name=" + WWW.EscapeURL(name) + "&score=" + WWW.EscapeURL(highscore.ToString()));
		
		yield return setHighscorePost;
		
		if (setHighscorePost.error != null)
		{
			Error();
		}

		else
		{
			Debug.Log("Success");
		}
	}

	IEnumerator GetRank()
	{
		Debug.Log("Getting rank");

		string id = PlayerPrefs.GetInt("id").ToString();
		string getRankURL = "http://www.snakeball.jctwood.uk/GetRank.php?";
		WWW getRankPost = new WWW(getRankURL + "id=" + WWW.EscapeURL(id));

		yield return getRankPost;
		
		if (getRankPost.error == null)
		{
			Debug.Log("rank: " + getRankPost.text);

			PlayerPrefs.SetInt("rank", System.Int32.Parse(getRankPost.text));

			StartCoroutine("GetHighscores");
		}

		else
		{
			loadingText.gameObject.SetActive(false);

			Error();
		}
	}

	IEnumerator GetHighscores()
	{
		Debug.Log("Getting highscores");

		string getHighscoresURL = "http://www.snakeball.jctwood.uk/GetHighscores.php";
		WWW getHighscoresPost = new WWW(getHighscoresURL);

		loadingText.gameObject.SetActive(true);

		yield return getHighscoresPost;
		
		if (getHighscoresPost.error == null)
		{
			loadingText.gameObject.SetActive(false);
			hiscoreList.gameObject.SetActive(true);

			string[] textList = getHighscoresPost.text.Split(new string[]{"\n","\t"}, System.StringSplitOptions.RemoveEmptyEntries);
		
			string[] names = new string[Mathf.FloorToInt(textList.Length/2)];
			string[] scores = new string[names.Length];

			int rank = PlayerPrefs.GetInt("rank");
			string name = PlayerPrefs.GetString("name");

			for (int element = 0; element < textList.Length; element++)
			{
				if (element % 2 == 0)
				{
					names[Mathf.FloorToInt(element / 2)] = textList[element];
				}

				else 
				{
					scores[Mathf.FloorToInt(element / 2)] = textList[element];
				}
			}

			for(int index = 0; index < names.Length; index++)
			{
				GameObject entry = Instantiate(entryPrefab) as GameObject;
				entry.transform.SetParent(hiscoreList.transform, false);

				Text[] values = entry.GetComponentsInChildren<Text>() as Text[];
				values[0].text = "" + (index + 1);
				values[1].text = names[index];
				values[2].text = scores[index];

				if (index == rank)
				{
					values[0].color = highlightColour;
					values[1].color = highlightColour;
					values[2].color = highlightColour;
				}
			}

			if (rank > 10)
			{
				GameObject entry = Instantiate(entryPrefab) as GameObject;
				entry.transform.SetParent(hiscoreList.transform, false);
				
				Text[] values = entry.GetComponentsInChildren<Text>() as Text[];
				values[0].text = rank.ToString();
				values[1].text = name;
				values[2].text = highscore.ToString();

				values[0].color = highlightColour;
				values[1].color = highlightColour;
				values[2].color = highlightColour;
			}
		}
		
		else
		{
			Error();
			
			loadingText.gameObject.SetActive(false);
		}
	}

	void Error()
	{
		errorText.gameObject.SetActive(true);
	}

	bool LeftKeyboard()
	{
		if (Input.GetKey("1") || Input.GetKey("2") || Input.GetKey("3") ||
		    Input.GetKey("4") || Input.GetKey("q") || Input.GetKey("w") ||
		    Input.GetKey("e") || Input.GetKey("r") || Input.GetKey("a") ||
		    Input.GetKey("s") || Input.GetKey("d") || Input.GetKey("f") ||
		    Input.GetKey("z") || Input.GetKey("x") || Input.GetKey("c")	)
		{
			return true;
		}

		else
		{
			return false;
		}
	}

	bool RightKeyboard()
	{
		if (Input.GetKey("7") || Input.GetKey("8") || Input.GetKey("9") ||
		    Input.GetKey("0") || Input.GetKey("-") || Input.GetKey("=") ||
		    Input.GetKey("u") || Input.GetKey("i") || Input.GetKey("o") ||
		    Input.GetKey("p") || Input.GetKey("[") || Input.GetKey("]") ||
		    Input.GetKey("h") || Input.GetKey("j") || Input.GetKey("k") ||
		    Input.GetKey("l") || Input.GetKey(";") || Input.GetKey("'") ||
		    Input.GetKey("n") || Input.GetKey("m") || Input.GetKey(",") ||
		    Input.GetKey(".") || Input.GetKey("/") )
		{
			return true;
		}
		
		else
		{
			return false;
		}
	}
}
