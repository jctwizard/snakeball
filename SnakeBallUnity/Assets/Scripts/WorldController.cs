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

	private bool lightTheme = true;
	
	public Image background;
	public Text scoreText;
	public Text creditText;
	public Text retryText;
	public Text pauseText;
	public Text snakeballText;
	public Toggle pauseToggle;
	public Image leftButton;
	public Image rightButton;

	public Color blueColour;

	private int lastTouchCount = 0;

	void Start() 
	{
		Screen.orientation = ScreenOrientation.Portrait;
		Application.targetFrameRate = 60;

		moveApple();
		addBodyPart(initialParts);
		retryText.enabled = false;
		leftButton.enabled = false;
		rightButton.enabled = false;

		highscore = PlayerPrefs.GetInt("highscore");
		scoreText.text = 0 + "/" + highscore;
		creditText.enabled = false;
	}

	void FixedUpdate() 
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}

		if (gameOver)
		{
			if (Input.GetKeyDown(KeyCode.Space) || ((Input.touchCount > 0) && (lastTouchCount == 0) && (Input.GetTouch(0).position.y > (Screen.height * 0.2f))))
			{
				for (int i = 0; i < bodyParts.Count; i++)
				{
					Destroy((GameObject)bodyParts[i]);
				}

				bodyParts.Clear();

				addBodyPart(initialParts);
				score = 0;

				retryText.enabled = false;
				pauseText.enabled = true;

				scoreText.text = score + "/" + highscore;

				gameOver = false;
			}
		}

		else if (paused)
		{
			if (!pauseToggle.isOn)
			{
				paused = false;
				pauseText.text = "pause";
				creditText.enabled = false;
			}
		}

		else
		{
			bool left = Input.GetKey(KeyCode.LeftArrow);

			bool right = Input.GetKey(KeyCode.RightArrow);

			// Pause the game
			if (pauseToggle.isOn)
			{
				paused = true;
				pauseText.text = "unpause";
				creditText.enabled = true;
			}

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

			if (sphereCollision(head, headRadius, apple, appleRadius))
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
					rightButton.color = Color.white;
				}
				
				else if (right)
				{
					rotation += 0.1f;
					rightButton.color = blueColour;
					leftButton.color = Color.white;
				}
			}

			else
			{
				leftButton.color = Color.white;
				rightButton.color = Color.white;
			}

			velocity.x = Mathf.Cos(rotation);
			velocity.y = Mathf.Sin(rotation);
			velocity.Scale(new Vector2(speed, speed));

			controlBall.transform.Rotate(velocity.y, velocity.x, 0);

			if (bodyCollision(head, headRadius) && (score > 0))
			{
				gameOver = true;

				leftButton.color = Color.white;
				rightButton.color = Color.white;

				retryText.enabled = true;
				pauseText.enabled = false;
			}
		}

		lastTouchCount = Input.touchCount;
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
}
