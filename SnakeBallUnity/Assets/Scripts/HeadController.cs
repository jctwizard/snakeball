using UnityEngine;
using System.Collections;

public class HeadController : MonoBehaviour 
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

	public float headRadius;
	private ArrayList bodyParts = new ArrayList();
	public GameObject bodyPrefab;
	public int initialParts;
	public int applePartBonus;

	private bool paused = false;

	public GUIText scoreText;
	public GUIText retryText;
	public GUITexture leftButton;
	public GUITexture rightButton;

	public int origW;
	public int origH;

	public bool resizeUI;

	void Start() 
	{
		Screen.orientation = ScreenOrientation.Portrait;

		moveApple();
		addBodyPart(initialParts);
		retryText.enabled = false;
		leftButton.enabled = false;
		rightButton.enabled = false;

		highscore = PlayerPrefs.GetInt("highscore");
		scoreText.text = 0 + "/" + highscore;

		if (resizeUI)
		{
			scaleUI();
		}
	}

	void scaleUI()
	{
		// Acquire new scales
		float scaleX = Screen.width / origW; //your scale x
		float scaleY = Screen.height / origH; //your scale y

		//Find all GUIText object on your scene
		GUIText[] texts = FindObjectsOfType(typeof(GUIText)) as GUIText[]; 

		foreach(GUIText myText in texts) 
		{ 
			//find your element of text
			Vector2 pixOff = myText.pixelOffset; //your pixel offset on screen
			int origSizeText = myText.fontSize;
			myText.pixelOffset = new Vector2(pixOff.x*scaleX, pixOff.y*scaleY); //new position
			myText.fontSize = (int)(origSizeText * scaleX); //new size font
		}

		//Find all GUITexture on your scene
		GUITexture[] textures =  FindObjectsOfType(typeof(GUITexture)) as GUITexture[];

		foreach(GUITexture myTexture in textures) 
		{ 
			//find your element of texture
			Rect pixIns = myTexture.pixelInset; //your dimention of texture

			//Change size pixIns for our screen
			pixIns.x = pixIns.x * scaleX;
			pixIns.y = pixIns.y * scaleY;
			pixIns.width = pixIns.width * scaleX;
			pixIns.height = pixIns.height * scaleY;

			//Sets new rectangle for our texture
			myTexture.pixelInset = pixIns;
		}
	}

	void FixedUpdate() 
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}

		if (paused)
		{
			if (Input.anyKeyDown)
			{
				for (int i = 0; i < bodyParts.Count; i++)
				{
					Destroy((GameObject)bodyParts[i]);
				}

				bodyParts.Clear();

				addBodyPart(initialParts);
				score = 0;

				retryText.enabled = false;

				scoreText.text = score + "/" + highscore;

				paused = false;
			}
		}

		else
		{
			bool left = Input.GetKey(KeyCode.LeftArrow);

			bool right = Input.GetKey(KeyCode.RightArrow);

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

			leftButton.enabled = left ? true : false;
			rightButton.enabled = right ? true : false;

			if (sphereCollision(this.gameObject, headRadius, apple, appleRadius))
			{
				addBodyPart(applePartBonus);
				moveApple();

				score++;

				if (score >= highscore)
				{
					highscore = score;
					PlayerPrefs.SetInt("highscore", highscore);
				}
				
				scoreText.text = score + "/" + highscore;
			}

			moveBodyParts();

			if (left && right)
			{
				speed = baseSpeed + 1;
			}

			else 
			{
				speed = baseSpeed;

				if (left)
				{
					rotation -= 0.1f;
				}
				
				else if (right)
				{
					rotation += 0.1f;
				}
			}

			velocity.x = Mathf.Cos(rotation);
			velocity.y = Mathf.Sin(rotation);
			velocity.Scale(new Vector2(speed, speed));

			controlBall.transform.Rotate(velocity.y, velocity.x, 0);

			if (bodyCollision() && score > 0)
			{
				paused = true;

				retryText.enabled = true;
			}
		}
	}

	void addBodyPart(int count = 1)
	{
		for (int i = 0; i < count; i++)
		{
			GameObject newPart = GameObject.Instantiate(bodyPrefab) as GameObject;

			if ((bodyParts.Count + 1) % 3 != 0)
			{
				newPart.renderer.enabled = false;
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

		((GameObject)bodyParts[0]).transform.position = transform.position;
	}

	void moveApple()
	{
		Vector3 randCoord = Vector3.zero;

		while (randCoord == Vector3.zero)
		{
			randCoord = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1));
			randCoord.Normalize();
			randCoord.Scale(new Vector3(ballRadius, ballRadius, ballRadius));
		}

		apple.transform.position = randCoord;
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
			Debug.Log("larger radius: " + (radius) + ", distance: " + distance);

			return true;
		}

		else
		{
			return false;
		}
	}

	bool bodyCollision()
	{
		for(int i = 5; i < bodyParts.Count - 1; i++)
		{
			if ((i + 1) % 3 != 0)
			{
				if(sphereCollision((GameObject)bodyParts[i], headRadius, this.gameObject, headRadius))
				{
					return true;
				}
			}
		}

		return false;
	}

	void OnGUI()
	{
		if (GUI.Button(new Rect(Screen.width * 0.5f - 60, 20, 150, 20), "created by @jctwood", GUIStyle.none))
		{
			Application.OpenURL("http://www.twitter.com/jctwood");
		}
	}
}
