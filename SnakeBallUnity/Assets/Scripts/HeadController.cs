using UnityEngine;
using UnityEngine.UI;
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
	private bool gameOver = false;

	public Text scoreText;
	public Text creditText;
	public Text retryText;
	public Text pauseText;
	public Image leftButton;
	public Image rightButton;

	public int origW;
	public int origH;

	private int lastTouchCount = 0;

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
		creditText.enabled = false;

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
	}

	void FixedUpdate() 
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}

		if (gameOver)
		{
			if (Input.anyKeyDown && (lastTouchCount == 0))
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

				gameOver = false;
			}
		}

		else if (paused)
		{
			/*// Go to link
			if ((Input.GetMouseButtonDown(0) && creditText.HitTest(Input.mousePosition)) ||
			    ((Input.touchCount > 0) && creditText.HitTest(Input.GetTouch(0).position)))
			{
				Application.OpenURL("http://www.twitter.com/jctwood");
			}

			if ((Input.GetMouseButtonDown(0) && pauseText.HitTest(Input.mousePosition)) ||
			    (((Input.touchCount > 0) && pauseText.HitTest(Input.GetTouch(0).position)) &&
			    (lastTouchCount == 0)))
			{
				paused = false;
				pauseText.text = "pause";
				creditText.enabled = false;
			}*/
		}

		else
		{
			bool left = Input.GetKey(KeyCode.LeftArrow);

			bool right = Input.GetKey(KeyCode.RightArrow);

			// Pause the game
			/*if ((Input.GetMouseButtonDown(0) && pauseText.HitTest(Input.mousePosition)) ||
			    (((Input.touchCount > 0) && pauseText.HitTest(Input.GetTouch(0).position)) &&
				(lastTouchCount == 0)))
			{
				paused = true;
				pauseText.text = "unpause";
				creditText.enabled = true;
			}*/

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

			if (bodyCollision(this.gameObject, headRadius) && (score > 0))
			{
				gameOver = true;

				retryText.enabled = true;
			}
		}

		lastTouchCount = Input.touchCount;
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
		do
		{
			Vector3 randCoord = Vector3.zero;

			while (randCoord == Vector3.zero)
			{
				randCoord = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
				randCoord.Normalize();
				randCoord.Scale(new Vector3(ballRadius, ballRadius, ballRadius));
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
}
