using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.EventSystems;

public class MainGame : MonoBehaviour {
    public PongGameEditor pongGameEdit;

    Color cameraDefaultColor;
    GameObject pongGame = null;
    float countLoopTime = 0;
    private void Awake()
    {
        //If there is not had any MainCamera, create one
        if (GameObject.FindGameObjectWithTag("MainCamera") == null)
        {
            GameObject cam = new GameObject("Main Camera");
            cam.AddComponent<Camera>();
            cam.AddComponent<AudioListener>();
            cam.AddComponent<FlareLayer>();
            cam.AddComponent<GUILayer>();
            cam.tag = "MainCamera";
            cam.transform.position = new Vector3(0, 0, -10);
            cam.GetComponent<Camera>().orthographic = true;
        }
        //Get the default camera color
        cameraDefaultColor = Camera.main.backgroundColor;
    }
    // Use this for initialization
    void Start () {
        CreateNewPongGame(pongGameEdit);
	}
    ///<summary>
    ///Destroy the current pong game, can only use one after create new Pong Game.
    ///</summary>
    void ExitPongGame()
    {
        if (pongGame != null)
        {
            Destroy(pongGame);
            Camera.main.backgroundColor = cameraDefaultColor;
            pongGame = null;
        }
        else
        {
            Debug.LogWarning("There is no Pong Game!");
        }       
    }
    /// <summary>
    /// Create a new pong game with specific PongGameEditor.
    /// Only allow 1 Pong Game in run time if use this.
    /// </summary>
    /// <param name="pongGameEditor">The specific PongGameEditor to create.</param>
    void CreateNewPongGame(PongGameEditor pongGameEditor)
    {
        if (pongGame == null)
        {
            CreatePongGame(pongGameEditor);
        }
        else
        {
            Debug.LogWarning("There is already had one Pong Game, you need to exit it before create one! Or use CreatePongGame to create multiple Pong Game.");
        }
    }
    /// <summary>
    /// Remove all the Pong Game in currect runtime.
    /// </summary>
    void ExitAllPongGame()
    {
        foreach (ControlPanel item in FindObjectsOfType<ControlPanel>())
        {
            Destroy(item.transform.parent);
        }
    }
    /// <summary>
    /// Create a new pong game with specific PongGameEditor.
    /// Can create multiple Pong Game in run time if use this.
    /// </summary>
    /// <param name="pongGameEditor">The specific PongGameEditor to create.</param>
    /// <return></return>
    void CreatePongGame(PongGameEditor pongGameEditor)
    {
        pongGame = new GameObject("Pong Game");
        Camera.main.backgroundColor = pongGameEditor.cameraColor;
        SpriteRenderer spriteRenderer;
        Rigidbody2D rb;
        //Ball
        GameObject ball = new GameObject("Ball");
        ball.transform.parent = pongGame.transform;
        spriteRenderer = ball.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GameTexture.TextureToSprite(GameTexture.GetCircleTexture(new Color(1, 1, 1, 255)), 8);
        Ball ballScript = ball.AddComponent<Ball>();
        ballScript.startDirection = pongGameEditor.ballStartDirection;
        ballScript.speed = pongGameEditor.ballStartSpeed;
        ballScript.mainGame = this;
        rb = ball.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = pongGameEditor.allowRotationByCollider == true ? false : true;
        PhysicsMaterial2D ballPhysics = new PhysicsMaterial2D()
        {
            name = "Ball Physics",
            bounciness = 1,
            friction = 0
        };
        rb.sharedMaterial = ballPhysics;
        ball.transform.localScale = new Vector3(0.5f, 0.5f, 1);
        ball.AddComponent<CircleCollider2D>();
        //Map
        GameObject map = new GameObject("Map Object");
        map.transform.parent = pongGame.transform;

        GameObject enemyWinWall = new GameObject("EnemyWinWall");
        enemyWinWall.transform.parent = map.transform;
        spriteRenderer = enemyWinWall.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GameTexture.TextureToSprite(GameTexture.GetSquareTexture(new Color(1, 1, 1, 255)), 8);
        if (pongGameEditor.playerStartLocation == PongGameEditor.PlayerStartLocation.Left)
        {
            enemyWinWall.transform.position = new Vector3(pongGameEditor.gameSize.x / 2, 0);
        }
        else enemyWinWall.transform.position = new Vector3(-pongGameEditor.gameSize.x / 2, 0);
        enemyWinWall.transform.localScale = new Vector3(1, pongGameEditor.gameSize.y, 1);
        enemyWinWall.AddComponent<BoxCollider2D>();

        GameObject playerWinWall = new GameObject("PlayerWinWall");
        playerWinWall.transform.parent = map.transform;
        spriteRenderer = playerWinWall.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GameTexture.TextureToSprite(GameTexture.GetSquareTexture(new Color(1, 1, 1, 255)), 8);
        if (pongGameEditor.playerStartLocation == PongGameEditor.PlayerStartLocation.Left)
        {
            playerWinWall.transform.position = new Vector3(-pongGameEditor.gameSize.x / 2, 0);
        }
        else playerWinWall.transform.position = new Vector3(pongGameEditor.gameSize.x / 2, 0);
        playerWinWall.transform.localScale = new Vector3(1, pongGameEditor.gameSize.y, 1);
        playerWinWall.AddComponent<BoxCollider2D>();

        GameObject upWall = GameObjectAdvenced.SimpleSquareObject();
        upWall.name = "UpWall";
        upWall.transform.parent = map.transform;
        upWall.transform.position = new Vector3(0, pongGameEditor.gameSize.y / 2);
        upWall.transform.localScale = new Vector3(pongGameEditor.gameSize.x, 1, 1);

        GameObject downWall = GameObjectAdvenced.SimpleSquareObject();
        downWall.name = "DownWall";
        downWall.transform.parent = map.transform;
        downWall.transform.position = new Vector3(0, -pongGameEditor.gameSize.y / 2);
        downWall.transform.localScale = new Vector3(pongGameEditor.gameSize.x, 1, 1);

        //Player Paddle
        GameObject playerPaddle = GameObjectAdvenced.SimpleSquareObject();
        playerPaddle.transform.parent = pongGame.transform;
        playerPaddle.name = pongGameEditor.playerName;
        if (pongGameEditor.playerStartLocation == PongGameEditor.PlayerStartLocation.Left)
        {
            playerPaddle.transform.position = new Vector3(-pongGameEditor.gameSize.x / 2 + pongGameEditor.distanceFromWall, 0);
        }
        else playerPaddle.transform.position = new Vector3(pongGameEditor.gameSize.x / 2 - pongGameEditor.distanceFromWall, 0);
        playerPaddle.transform.localScale = new Vector3(0.5f, 3, 1);
        rb = playerPaddle.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.mass = 1000;
        Paddle playerPad = playerPaddle.AddComponent<Paddle>();
        playerPad.speed = pongGameEditor.playerSpeed;
        playerPad.autoMove = false;
        playerPad.mainGame = this;
        playerPad.isEnemy = false;

        //Enemy Paddle
        GameObject enemyPaddle = GameObjectAdvenced.SimpleSquareObject();
        enemyPaddle.transform.parent = pongGame.transform;
        enemyPaddle.name = pongGameEditor.enemyName;
        if (pongGameEditor.playerStartLocation == PongGameEditor.PlayerStartLocation.Left)
        {
            enemyPaddle.transform.position = new Vector3(pongGameEditor.gameSize.x / 2 - pongGameEditor.distanceFromWall, 0);
        }
        else enemyPaddle.transform.position = new Vector3(-pongGameEditor.gameSize.x / 2 + pongGameEditor.distanceFromWall, 0);
        enemyPaddle.transform.localScale = new Vector3(0.5f, 3, 1);
        rb = enemyPaddle.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.mass = 1000;
        Paddle enemyPad = enemyPaddle.AddComponent<Paddle>();
        enemyPad.speed = pongGameEditor.enemySpeed;
        enemyPad.autoMove = pongGameEditor.twoPlayer == false ? true : false;
        enemyPad.mainGame = this;
        enemyPad.isEnemy = true;

        //User Interface (UI)
        ControlPanel controlPanel;
        /*Canvas*/
        GameObject canvasDisplay = new GameObject("Canvas Display");
        canvasDisplay.transform.parent = pongGame.transform;
        canvasDisplay.AddComponent<RectTransform>();
        Canvas canvas = canvasDisplay.AddComponent<Canvas>();
        canvasDisplay.AddComponent<CanvasScaler>();
        canvasDisplay.AddComponent<GraphicRaycaster>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasDisplay.layer = 5;

        /*Event System*/
        GameObject eventSystem = new GameObject("Event System");
        eventSystem.transform.parent = pongGame.transform;
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();

        /*Score display*/
        GameObject playerScoreDisplay = new GameObject("Player Score Display");
        playerScoreDisplay.transform.parent = canvasDisplay.transform;
        RectTransform playerRect = playerScoreDisplay.AddComponent<RectTransform>();
        playerRect.localPosition = new Vector3(pongGameEditor.playerStartLocation == PongGameEditor.PlayerStartLocation.Left ? -40 : 40, 143.5f);
        playerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 40);
        playerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 40);
        playerScoreDisplay.AddComponent<CanvasRenderer>();
        Text playerScoreText = playerScoreDisplay.AddComponent<Text>();
        playerScoreText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        playerScoreText.fontStyle = pongGameEditor.playerScoreFontStyle;
        playerScoreText.color = pongGameEditor.playerScoreColor;
        playerScoreText.fontSize = pongGameEditor.playerScoreFontSize;
        playerScoreText.alignment = TextAnchor.MiddleCenter;

        GameObject enemyScoreDisplay = new GameObject("Enemy Score Display");
        enemyScoreDisplay.transform.parent = canvasDisplay.transform;
        RectTransform enemyRect = enemyScoreDisplay.AddComponent<RectTransform>();
        enemyRect.localPosition = new Vector3(pongGameEditor.playerStartLocation == PongGameEditor.PlayerStartLocation.Left ? 40 : -40, 143.5f);
        enemyRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 40);
        enemyRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 40);
        enemyScoreDisplay.AddComponent<CanvasRenderer>();
        Text enemyScoreText = enemyScoreDisplay.AddComponent<Text>();
        enemyScoreText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        enemyScoreText.fontStyle = pongGameEditor.enemyScoreFontStyle;
        enemyScoreText.color = pongGameEditor.enemyScoreColor;
        enemyScoreText.fontSize = pongGameEditor.enemyScoreFontSize;
        enemyScoreText.alignment = TextAnchor.MiddleCenter;

        /*Other display*/

        GameObject statusDisplay = new GameObject("Status Display");
        statusDisplay.transform.parent = canvasDisplay.transform;
        RectTransform statusRect = statusDisplay.AddComponent<RectTransform>();
        statusRect.localPosition = new Vector3(0, 57.4f);
        statusRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 523);
        statusRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 30);
        statusDisplay.AddComponent<CanvasRenderer>();
        Text statusText = statusDisplay.AddComponent<Text>();
        statusText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        statusText.fontStyle = pongGameEditor.statusDisplayFontStyle;
        statusText.color = pongGameEditor.statusDisplayColor;
        statusText.fontSize = pongGameEditor.statusDisplayFontSize;
        statusText.alignment = TextAnchor.MiddleCenter;

        GameObject popupDisplay = new GameObject("Popup Display");
        popupDisplay.transform.parent = canvasDisplay.transform;
        RectTransform popupRect = popupDisplay.AddComponent<RectTransform>();
        popupRect.localPosition = new Vector3(0, -111);
        popupRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 523);
        popupRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 30);
        popupDisplay.AddComponent<CanvasRenderer>();
        Text popupText = popupDisplay.AddComponent<Text>();
        popupText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        popupText.fontStyle = pongGameEditor.popupDisplayFontStyle;
        popupText.fontSize = pongGameEditor.popupDisplayFontSize;
        popupText.color = pongGameEditor.popupDisplayColor;
        popupText.alignment = TextAnchor.MiddleCenter;

        //Other Object
        GameObject timeDisplay = new GameObject("Time Display");
        timeDisplay.transform.parent = canvasDisplay.transform;
        RectTransform timeRect = timeDisplay.AddComponent<RectTransform>();
        timeRect.localPosition = new Vector3(0, 120);
        timeRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 160);
        timeRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 30);
        timeDisplay.AddComponent<CanvasRenderer>();
        Text timeText = timeDisplay.AddComponent<Text>();
        timeText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        timeText.fontSize = pongGameEditor.timeDisplayFontSize;
        timeText.fontStyle = pongGameEditor.timeDisplayFontStyle;
        timeText.color = pongGameEditor.timeDisplayColor;
        timeText.alignment = TextAnchor.MiddleCenter;

        controlPanel = canvasDisplay.AddComponent<ControlPanel>();
        foreach (GameObject item in GameObjectAdvenced.GetChildsOfTheObject(canvasDisplay))
        {
            item.layer = 5;
        }
        controlPanel.playerScoreDisplay = playerScoreDisplay.GetComponent<Text>();
        controlPanel.enemyScoreDisplay = enemyScoreDisplay.GetComponent<Text>();
    }
    // Update is called once per frame
    void Update () {
        if (pongGame != null)
        {
            if (FindObjectOfType<Ball>().isEndGame == true && Input.GetKey(pongGameEdit.exitGame))
            {
                ExitPongGame();
            }
            else if (FindObjectOfType<Ball>().isEndGame)
            {
                countLoopTime += Time.deltaTime;
                if (countLoopTime >= pongGameEdit.timeBetweenPopup)
                {
                    Text text = GameObject.Find("Popup Display").GetComponent<Text>();
                    countLoopTime = 0;
                    if (text.color == new Color(0, 0, 0, 0))
                    {
                        text.color = pongGameEdit.popupDisplayColor;
                    }
                    else text.color = new Color(0, 0, 0, 0);
                }
            }
        }
        if (pongGameEdit.infiniteGameTime)
        {
            pongGameEdit.infiniteScore = false;
        }
	}
    /// <summary>
    /// Calculating the hours, minutes and seconds of given seconds.
    /// </summary>
    /// <param name="s">Given second.</param>
    /// <param name="hours">The hours.</param>
    /// <param name="minutes">The minutes.</param>
    /// <param name="seconds">The seconds.</param>
    public void GetTimeBySecond(int s, out int hours, out int minutes, out int seconds)
    {
        int sec = 0, min = 0, hour = 0;
        sec = s;
        while (sec >= 60)
        {
            sec -= 60;
            min += 1;
            if (min == 60)
            {
                hour += 1;
                min = 0;
            }
        }

        hours = hour < 0 ? 0 : hour;
        minutes = min < 0 ? 0 : min;
        seconds = sec < 0 ? 0 : sec;
    }
}
public class ControlPanel : MonoBehaviour
{
    public Text playerScoreDisplay;
    public Text enemyScoreDisplay;

    int playerCurrectScore = 0;
    int enemyCurrectScore = 0;

    public void Start()
    {
        playerCurrectScore = FindObjectOfType<MainGame>().pongGameEdit.playerStartScore;
        enemyCurrectScore = FindObjectOfType<MainGame>().pongGameEdit.enemyStartScore;
        playerScoreDisplay.text = playerCurrectScore.ToString();
        enemyScoreDisplay.text = enemyCurrectScore.ToString();
    }
    public IEnumerator WaitFor(float time)
    {
        yield return new WaitForSeconds(time);
        GameObject.Find("Status Display").GetComponent<Text>().text = "";
    }
    public void AddScore(bool enemy, int scoreToAdd = 1)
    {
        if (enemy)
        {
            enemyCurrectScore += scoreToAdd;
            enemyScoreDisplay.text = enemyCurrectScore.ToString();
            if (FindObjectOfType<MainGame>().pongGameEdit.enemyWinScore <= enemyCurrectScore && !FindObjectOfType<MainGame>().pongGameEdit.infiniteScore)
            {
                Win(true);
            }
        }
        else
        {
            playerCurrectScore += scoreToAdd;
            playerScoreDisplay.text = playerCurrectScore.ToString();
            if (FindObjectOfType<MainGame>().pongGameEdit.playerWinScore <= playerCurrectScore && !FindObjectOfType<MainGame>().pongGameEdit.infiniteScore)
            { 
                Win(false);
            }
        }
    }
    public void CheckScore(bool draw)
    {
        if (playerCurrectScore > enemyCurrectScore)
        {
            Win(false);
            draw = false;
        }
        else if (enemyCurrectScore > playerCurrectScore)
        {
            Win(true);
            draw = false;
        }
        else
        {
            draw = true;
            GameObject.Find("Status Display").GetComponent<Text>().text = "Sudden Death Mode!";
            StartCoroutine(WaitFor(2));
        }
    }
    public void ClearStatusDisplay()
    {
        GameObject.Find("Status Display").GetComponent<Text>().text = "";
    }
    public void Win(bool enemy)
    {
        GameObject.Find("Popup Display").GetComponent<Text>().text = "Press " + FindObjectOfType<MainGame>().pongGameEdit.exitGame.ToString() + " to continue.";
        FindObjectOfType<Ball>().isEndGame = true;
        MainGame mainGame = FindObjectOfType<MainGame>();
        FindObjectOfType<Ball>().RestartGame();
        StopAllCoroutines();
        ClearStatusDisplay();
        if (enemy)
        {
            GameObject game = GameObject.Find("Status Display");
            game.GetComponent<Text>().text = mainGame.pongGameEdit.enemyName + " " + mainGame.pongGameEdit.winText;
        }
        else
        {
            GameObject game = GameObject.Find("Status Display");
            game.GetComponent<Text>().text = mainGame.pongGameEdit.playerName + " " + mainGame.pongGameEdit.winText;
        }
    }
}
public class Paddle : MonoBehaviour
{
    public float speed = 3;
    public bool isEnemy = false;
    public bool autoMove = false;
    public MainGame mainGame;

    Rigidbody2D rb;
    Vector3 startPos;
    float vertical;
    float currSpeed;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
        currSpeed = speed;
    }
    private void Update()
    {
        if (FindObjectOfType<Ball>().isStarted)
        {
            Vector3 ballPos = FindObjectOfType<Ball>().gameObject.transform.position;
            if (!autoMove)
            {
                CheckInput();
                rb.velocity = new Vector2(0, vertical * currSpeed);
            }
            else
            {
                if (transform.position.y < ballPos.y - 1 || transform.position.y > ballPos.y + 1)
                {
                    if (!isEnemy)
                    {
                        currSpeed = speed * mainGame.pongGameEdit.playerDashMultipler;
                    }
                    else currSpeed = speed * mainGame.pongGameEdit.enemyDashMultipler;
                }
                else currSpeed = speed;
                if (transform.position.y < ballPos.y - 0.5f)
                {
                    rb.velocity = new Vector2(0, currSpeed);
                }
                else if (transform.position.y > ballPos.y + 0.5f)
                {
                    rb.velocity = new Vector2(0, -currSpeed);
                }
                else
                {
                    rb.velocity = Vector2.zero;
                }
            }
        }
    }
    /// <summary>
    /// Set the velocity of paddle to zero and move it to the starting point
    /// </summary>
    public void RestartPaddle()
    {
        rb.velocity = Vector2.zero;
        transform.position = startPos;
    }
    IEnumerator Wait(float time, System.Action action)
    {
        yield return new WaitForSeconds(time);
        action();
    }
    void CheckInput()
    {
        if (!isEnemy)
        {
            if (Input.GetKey(mainGame.pongGameEdit.playerInput.dash))
            {
                currSpeed = speed * mainGame.pongGameEdit.playerDashMultipler;
            }
            else currSpeed = speed;
            if (Input.GetKey(mainGame.pongGameEdit.playerInput.up))
            {
                vertical = 1;
            }
            else if (Input.GetKey(mainGame.pongGameEdit.playerInput.down))
            {
                vertical = -1;
            }
            else vertical = 0;
        }
        else if (isEnemy)
        {
            if (Input.GetKey(mainGame.pongGameEdit.enemyInput.dash))
            {
                currSpeed = speed * mainGame.pongGameEdit.enemyDashMultipler;
            }
            else currSpeed = speed;
            if (Input.GetKey(mainGame.pongGameEdit.enemyInput.up))
            {
                vertical = 1;
            }
            else if (Input.GetKey(mainGame.pongGameEdit.enemyInput.down))
            {
                vertical = -1;
            }
            else vertical = 0;
        }
    }
}
public class Ball : MonoBehaviour
{
    public float speed = 3;
    public PongGameEditor.BallStartDirection startDirection = PongGameEditor.BallStartDirection.Random;
    public MainGame mainGame;
    [HideInInspector]
    public bool isStarted = false;
    [HideInInspector]
    public bool isEndGame = false;
    [HideInInspector]
    public float currSecond = 0;

    float currSpeed = 0;
    int touched = 0;
    bool isFirstStart = true;
    bool isDraw = false;
    bool showedSuddenDeathModeStatus = false;
    Rigidbody2D rb;
    private void Start()
    {
        currSpeed = speed;
        rb = GetComponent<Rigidbody2D>();
    }
    /// <summary>
    /// Restart the ball and paddle to starting point with no velocity.
    /// </summary>
    public void RestartGame()
    {
        isStarted = false;
        rb.velocity = Vector2.zero;
        transform.position = Vector3.zero;
        touched = 0;
        foreach (Paddle item in FindObjectsOfType<Paddle>())
        {
            item.RestartPaddle();
        }
    }
    private void Update()
    {
        if (mainGame.pongGameEdit.suddenDeath && !showedSuddenDeathModeStatus)
        {
            showedSuddenDeathModeStatus = true;
            GameObject.Find("Status Display").GetComponent<Text>().text = "Sudden Death Mode!";
            StartCoroutine(FindObjectOfType<ControlPanel>().WaitFor(2));
        }
        if (!mainGame.pongGameEdit.infiniteGameTime && !isFirstStart && !isEndGame)
        {
            currSecond += Time.deltaTime;
            Text text = GameObject.Find("Time Display").GetComponent<Text>();
            int hours = 0, minutes = 0, seconds = 0;
            mainGame.GetTimeBySecond((int)mainGame.pongGameEdit.gameTime - (int)currSecond, out hours, out minutes, out seconds);
            string hStr = "", mStr = "", sStr = "";
            if (seconds < 10)
            {
                sStr = "0" + seconds.ToString();
            }
            else sStr = seconds.ToString();
            if (minutes < 10)
            {
                mStr = "0" + minutes.ToString();
            }
            else mStr = seconds.ToString();
            if (hours < 10)
            {
                hStr = "00" + hours.ToString();
            }
            else if (hours > 10 && hours < 100)
            {
                hStr = "0" + hours.ToString();
            }
            else hStr = hours.ToString();
            if (hours == 0)
            {
                text.text = mStr + " : " + sStr;
            }
            else
            {
                text.text = hStr + " : " + mStr + " : " + sStr;
            }
        }
        else if (!isFirstStart && mainGame.pongGameEdit.infiniteGameTime)
        {
            GameObject.Find("Time Display").GetComponent<Text>().text = "";
        }
        else if (!mainGame.pongGameEdit.infiniteGameTime && isFirstStart)
        {
            Text text = GameObject.Find("Time Display").GetComponent<Text>();
            int hours = 0, minutes = 0, seconds = 0;
            mainGame.GetTimeBySecond((int)mainGame.pongGameEdit.gameTime, out hours, out minutes, out seconds);
            string hStr = "", mStr = "", sStr = "";
            if (seconds < 10)
            {
                sStr = "0" + seconds.ToString();
            }
            else sStr = seconds.ToString();
            if (minutes < 10)
            {
                mStr = "0" + minutes.ToString();
            }
            else mStr = seconds.ToString();
            if (hours < 10)
            {
                hStr = "00" + hours.ToString();
            }
            else if (hours > 10 && hours < 100)
            {
                hStr = "0" + hours.ToString();
            }
            else hStr = hours.ToString();
            if (hours == 0)
            {
                text.text = mStr + " : " + sStr;
            }
            else
            {
                text.text = hStr + " : " + mStr + " : " + sStr;
            }
        }
        if (isDraw)
        {
            mainGame.pongGameEdit.suddenDeath = true;
        }
        if ((int)currSecond >= (int)mainGame.pongGameEdit.gameTime)
        {
            ControlPanel control = FindObjectOfType<ControlPanel>();           
            control.CheckScore(isDraw);
        }
        if (mainGame.pongGameEdit.increaseSpeedByBouncing)
        {
            float value = mainGame.pongGameEdit.ballSpeedSlope.Evaluate(Mathf.InverseLerp(0, mainGame.pongGameEdit.maxIncreaseTouched, touched));
            currSpeed = Mathf.Lerp(mainGame.pongGameEdit.ballStartSpeed, mainGame.pongGameEdit.ballMaxSpeed, value);
            currSpeed = (int)(currSpeed * 100) / 100f;
        }
        if (Input.GetKey(mainGame.pongGameEdit.startGame) && !isStarted && !isEndGame)
        {
            if (isFirstStart)
            {
                isFirstStart = false;
            }
            StartBall();
        }
        else if (!isStarted && mainGame.pongGameEdit.autoRestartGame && !isFirstStart && !isEndGame)
        {
            StartBall();
        }
        if (isStarted && !mainGame.pongGameEdit.changeBallSpeedByCollider)
        {
            bool[] isMinus = { false, false };
            isMinus[0] = rb.velocity.x <= 0 ? true : false;
            isMinus[1] = rb.velocity.y <= 0 ? true : false;
            Vector2 dump = new Vector2(Mathf.Abs(rb.velocity.x), Mathf.Abs(rb.velocity.y));
            if (dump != Vector2.one * currSpeed)
            {
                dump = Vector2.one * currSpeed;
                rb.velocity = new Vector2(isMinus[0] == true ? -dump.x : dump.x, isMinus[1] == true ? -dump.y : dump.y);
            }
        }
    }
    void StartBall()
    {
        isStarted = true;
        System.Random ran = new System.Random();
        int y = ran.Next(-1, 1) == 0 ? 1 : -1;
        if (startDirection == PongGameEditor.BallStartDirection.Left)
        {
            rb.velocity = new Vector2(-1, y) * speed;
        }
        else if (startDirection == PongGameEditor.BallStartDirection.Right)
        {
            rb.velocity = new Vector2(1, y) * speed;
        }
        else
        {
            int x = ran.Next(-1, 1) == 0 ? 1 : -1;
            rb.velocity = new Vector2(x, y) * speed;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        touched += 1;
        ControlPanel control = FindObjectOfType<ControlPanel>();
        if (collision.gameObject.name == "EnemyWinWall")
        {
            control.AddScore(false, mainGame.pongGameEdit.goalScore);
            RestartGame();
            if (mainGame.pongGameEdit.suddenDeath)
            {
                control.Win(false);
            }
        }
        else if (collision.gameObject.name == "PlayerWinWall")
        {
            control.AddScore(true, mainGame.pongGameEdit.goalScore);
            RestartGame();
            if (mainGame.pongGameEdit.suddenDeath)
            {
                control.Win(true);
            }
        }
    }
}
public static class GameObjectAdvenced
{
    /// <summary>
    /// Return all the object with the specific name.
    /// </summary>
    /// <param name="name">Name to find.</param>
    /// <returns></returns>
    public static GameObject[] FindObjectsWithName(string name)
    {
        List<GameObject> dump = new List<GameObject>();
        foreach (GameObject item in GameObject.FindObjectsOfType<GameObject>())
        {
            if (item.name == name)
            {
                dump.Add(item);
            }
        }
        return dump.ToArray();
    }
    /// <summary>
    /// Get all the children of the specific object.
    /// If the object doesn't have any child, return null.
    /// </summary>
    /// <param name="parent">The parent to get.</param>
    /// <returns></returns>
    public static GameObject[] GetChildsOfTheObject(GameObject parent)
    {
        List<GameObject> childs = new List<GameObject>();
        if (parent.transform.childCount == 0)
        {
            return null;
        }
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            childs.Add(parent.transform.GetChild(i).gameObject);
        }
        return childs.ToArray();
    }
    /// <summary>
    /// Create a simple square with collider and name "Square".
    /// </summary>
    /// <returns></returns>
    public static GameObject SimpleSquareObject()
    {
        GameObject game = new GameObject("Square");
        SpriteRenderer sprite = game.AddComponent<SpriteRenderer>();
        sprite.sprite = GameTexture.TextureToSprite(GameTexture.GetSquareTexture(new Color(1, 1, 1, 255)), 8);
        game.AddComponent<BoxCollider2D>();
        return game;
    }
    /// <summary>
    /// Create a simple circle with collider and name "Circle".
    /// </summary>
    /// <returns></returns>
    public static GameObject SimpleCircleObject()
    {
        GameObject game = new GameObject("Circle");
        SpriteRenderer sprite = game.AddComponent<SpriteRenderer>();
        sprite.sprite = GameTexture.TextureToSprite(GameTexture.GetCircleTexture(new Color(1, 1, 1, 255)), 8);
        game.AddComponent<CircleCollider2D>();
        return game;
    }
    /// <summary>
    /// Create a simple triangle with collider and name "Triangle".
    /// </summary>
    /// <returns></returns>
    public static GameObject SimpleTriangleObject(bool usePoligonCollider = false)
    {
        GameObject game = new GameObject("Triangle");
        SpriteRenderer sprite = game.AddComponent<SpriteRenderer>();
        sprite.sprite = GameTexture.TextureToSprite(GameTexture.GetTriangleTexture(new Color(1, 1, 1, 255)), 8);
        if (usePoligonCollider)
        {
            game.AddComponent<PolygonCollider2D>();
        }
        else game.AddComponent<BoxCollider2D>();
        return game;
    }
    /// <summary>
    /// Create an world game object with sprite renderer using specific sprite, named name.
    /// </summary>
    /// <param name="sprite">The sprite to create.</param>
    /// <param name="name">The name of the object.</param>
    /// <param name="component">Additional component.</param>
    /// <returns></returns>
    public static GameObject SpriteToGameObject(Sprite sprite, string name, System.Type component = null)
    {
        GameObject game = new GameObject(name);
        game.AddComponent<SpriteRenderer>().sprite = sprite;
        if (component != null)
        {
            game.AddComponent(component);
        }
        return game;
    }
}
public static class GameTexture
{
    /// <summary>
    /// Return a 8x8 filled circle texture with specific color.
    /// </summary>
    /// <param name="color">Circle color.</param>
    /// <returns></returns>
    public static Texture2D GetCircleTexture(Color color)
    {
        Texture2D texture = new Texture2D(8, 8);
        Vector2[] emptyColorPos = {
        new Vector2(0, 0), new Vector2(7, 0), new Vector2(0, 7), new Vector2(7, 7), // The corner
        new Vector2(0, 1), new Vector2(1, 0), // Top left
        new Vector2(6, 0), new Vector2(7, 1), // Top right
        new Vector2(0, 6), new Vector2(1, 7), // Bottom left
        new Vector2(6, 7), new Vector2(7, 6) }; // Bottom right
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Vector2 currPos = new Vector2(x, y);
                bool founded = false;
                foreach (Vector2 item in emptyColorPos)
                {
                    if (currPos == item)
                    {
                        texture.SetPixel(x, y, new Color(0, 0, 0, 0));
                        founded = true;
                        break;
                    }
                }
                if (!founded)
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }
        texture.Apply();

        return texture;
    }
    /// <summary>
    /// Return a 8x8 filled square texture with specific color.
    /// </summary>
    /// <param name="color">Square color.</param>
    /// <returns></returns>
    public static Texture2D GetSquareTexture(Color color)
    {
        Texture2D tex = new Texture2D(8, 8);
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                tex.SetPixel(x, y, color);
            }
        }
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return tex;
    }
    /// <summary>
    /// Return a 8x8 filled triangle texture with specific color.
    /// </summary>
    /// <param name="color">Triangle color.</param>
    /// <returns></returns>
    public static Texture2D GetTriangleTexture(Color color)
    {
        Texture2D texture = new Texture2D(8, 8);
        Point[] emptyPos =
        {
            new Point(7, 7), new Point(0, 7), //The corner
            //The left side
            new Point(1, 7), new Point(2, 7),
            new Point(0, 6), new Point(1, 6), new Point(2, 6),
            new Point(0, 5), new Point(1, 5),
            new Point(0, 4), new Point(1, 4),
            new Point(0, 3), new Point(0, 2),
            //The right side
            new Point(6, 7), new Point(5, 7),
            new Point(7, 6), new Point(6, 6), new Point(5, 6),
            new Point(7, 5), new Point(6, 5),
            new Point(7, 4), new Point(6, 4),
            new Point(7, 3), new Point(7, 2),
        };
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                Point currPos = new Point(x, y);
                bool isEmpty = false;
                foreach (Point item in emptyPos)
                {
                    if (currPos == item)
                    {
                        isEmpty = true;
                        texture.SetPixel(x, y, new Color(0, 0, 0, 0));
                        break;
                    }
                }
                if (!isEmpty)
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        return texture;
    }
    /// <summary>
    /// Return the texture that being fliped. Default is not filp.
    /// </summary>
    /// <param name="texture">The texture to flip.</param>
    /// <param name="verticalFilp">Filp vertically?</param>
    /// <param name="horizontalFilp">Filp horizontally?</param>
    public static Texture2D FilpTexture(Texture2D texture, bool verticalFilp = false, bool horizontalFilp = false)
    {
        Texture2D texture2D = texture;
        if (verticalFilp)
        {
            Color[] colors = new Color[texture.width * texture.height];
            Point[] points = new Point[texture.width * texture.height];
            int count = 0;
            for (int y = 0; y < texture.width; y++)
            {
                for (int x = 0; x < texture.height; x++)
                {
                    colors[count] = texture2D.GetPixel(x, y);
                    points[count] = new Point(x, y);
                    count += 1;
                }
            }
            int[] pointY = new int[texture.width * texture.height];
            for (int i = 0; i < points.Length; i++)
            {
                pointY[i] = points[i].y;
            }
            System.Array.Reverse(pointY);
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new Point(points[i].x, pointY[i]);
            }
            count = 0;
            for (int i = 0; i < colors.Length; i++)
            {
                texture2D.SetPixel(points[i].x, points[i].y, colors[i]);
            }
            texture2D.filterMode = FilterMode.Point;
            texture2D.Apply();
        }
        if (horizontalFilp)
        {
            Color[] colors = new Color[texture.width * texture.height];
            Point[] points = new Point[texture.width * texture.height];
            int count = 0;
            for (int y = 0; y < texture.width; y++)
            {
                for (int x = 0; x < texture.height; x++)
                {
                    colors[count] = texture2D.GetPixel(x, y);
                    points[count] = new Point(x, y);
                    count += 1;
                }
            }
            int[] pointX = new int[texture.width * texture.height];
            for (int i = 0; i < points.Length; i++)
            {
                pointX[i] = points[i].x;
            }
            System.Array.Reverse(pointX);
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new Point(pointX[i], points[i].y);
            }
            for (int i = 0; i < colors.Length; i++)
            {
                texture2D.SetPixel(points[i].x, points[i].y, colors[i]);
            }
            texture2D.filterMode = FilterMode.Point;
            texture2D.Apply();
        }
        return texture2D;
    }
    /// <summary>
    /// Return a sprite with specific texture.
    /// </summary>
    /// <param name="pixelsPerUnit">The number of pixel display in 1 by 1 square in world space.</param>
    /// <param name="texture">The texture to use.</param>
    public static Sprite TextureToSprite(Texture2D texture, int pixelsPerUnit = 100)
    {
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }
    /// <summary>
    /// Return a texture from a two dimension color array.
    /// </summary>
    /// <param name="colors">The two dimension color array.</param>
    /// <returns></returns>
    public static Texture2D TextureFromColorArray(Color[,] colors)
    {
        int width = colors.GetLength(0);
        int height = colors.GetLength(1);
        Texture2D texture = new Texture2D(width, height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                texture.SetPixel(x, y, colors[x, y]);
            }
        }
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        return texture;
    }
    /// <summary>
    /// Generated new texture filled with specific color.
    /// </summary>
    /// <param name="color">Color to fill.</param>
    /// <returns></returns>
    public static Texture2D FillTexture(Color color, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                texture.SetPixel(i, j, color);
            }
        }
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        return texture;
    }
    public static Point VectorToPoint(Vector2 vector)
    {
        return new Point((int)vector.x, (int)vector.y);
    }
    public static Point[] IntegerArrayToPointArray(int[] arr)
    {
        if (arr.Length % 2 != 0)
        {
            System.Array.Resize(ref arr, arr.Length + 1);
            arr[arr.Length - 1] = 0;
        }
        Point[] points = new Point[arr.Length / 2];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = new Point(arr[i * 2], arr[i * 2 + 1]);
        }
        return points;
    }
    public static Point[] VectorToPoint(Vector2[] vectors)
    {
        List<Point> points = new List<Point>();
        foreach (Vector2 item in vectors)
        {
            points.Add(new Point((int)item.x, (int)item.y));
        }
        return points.ToArray();
    }
    public static Size VectorToSize(Vector2 vector)
    {
        return new Size((int)vector.x, (int)vector.y);
    }
    public static Texture2D TextureFromPointAndColor(Point[] points, Color[] colors, Size textureSize)
    {
        Texture2D texture = FillTexture(new Color(0, 0, 0, 0), textureSize.width, textureSize.height);
        for (int i = 0; i < points.Length; i++)
        {
            Point point = new Point(Mathf.Clamp(points[i].x, 0, textureSize.width - 1), Mathf.Clamp(points[i].y, 0, textureSize.height - 1));
            Color color;
            try
            {
                color = colors[i];
            }
            catch (System.Exception)
            {
                color = new Color(0, 0, 0, 0);
            }
            texture.SetPixel(point.x, point.y, color);
        }
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        return texture;
    }
    public static Color[] GenerateSameColor(int size, Color color)
    {
        Color[] colors = new Color[size];
        for (int i = 0; i < size; i++)
        {
            colors[i] = color;
        }
        return colors;
    }
    /// <summary>
    /// Replace all the specific old color to the specific new color of the texture
    /// </summary>
    /// <param name="texture">The texture to set.</param>
    /// <param name="oldColor">Old color.</param>
    /// <param name="newColor">New color.</param>
    public static void ReplaceColor(Texture2D texture, Color oldColor, Color newColor)
    {
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                if (texture.GetPixel(x, y) == oldColor)
                {
                    texture.SetPixel(x, y, newColor);
                } 
            }
        }
        texture.filterMode = FilterMode.Point;
        texture.Apply();
    }
    /// <summary>
    /// Same as ReplaceColor but return a copy of it.
    /// </summary>
    /// <param name="texture">The texture.</param>
    /// <param name="oldColor">Old color.</param>
    /// <param name="newColor">New color.</param>
    /// <returns></returns>
    public static Texture2D GetReplaceTextureColor(Texture2D texture, Color oldColor, Color newColor)
    {
        Texture2D tex = texture;
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                if (tex.GetPixel(x, y) == oldColor)
                {
                    tex.SetPixel(x, y, newColor);
                }
            }
        }
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return tex;
    }
    /// <summary>
    /// Return a 6x10 texture that display a char that in from english alphabet or a number from range (0 - 9).
    /// </summary>
    /// <param name="letter">The char to use.</param>
    /// <param name="color">The color of the texture.</param>
    /// <returns></returns>
    public static Texture2D TextureFromLetterAndNumber(char letter, Color color)
    {
        Texture2D texture2D = FillTexture(new Color(0, 0, 0, 0), 6, 10);
        if (letter == 'a')
        {
            int[] arr = {2, 0, 3, 0, 1, 1, 4, 1, 0, 2, 5, 2, 0, 3, 5, 3, 0, 4, 5, 4, 0, 5, 5, 5, 0, 6, 5, 6, 0, 7, 5, 7, 0, 8,
            5, 8, 0, 9, 5, 9, 1, 6, 2, 6, 3, 6, 4, 6};
            Point[] points = IntegerArrayToPointArray(arr);
            Color[] colors = GenerateSameColor(points.Length, color);
            return TextureFromPointAndColor(points, colors, new Size(6, 10));
        }
        return texture2D;
    }
    /// <summary>
    /// The style options to rendering multiple image.
    /// </summary>
    public enum TileStyle { Single, Multiple }
    /// <summary>
    /// Scale an image to specific scale.
    /// </summary>
    /// <param name="texture">Texture to scale.</param>
    /// <param name="scale">The scale.</param>
    /// <param name="style">The style of the image, default is single.</param>
    /// <returns></returns>
    public static Texture2D ScaleImage(Texture2D texture, int scale, TileStyle style = TileStyle.Single)
    {
        Texture2D dump = new Texture2D(texture.width * scale, texture.height * scale);
        if (style == TileStyle.Single)
        {
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    Color color = texture.GetPixel(x, y);
                    for (int i = 0; i < scale; i++)
                    {
                        for (int j = 0; j < scale; j++)
                        {
                            dump.SetPixel(x * scale + j, y * scale + i, color);
                        }
                    }
                }
            }
        }
        else
        {
            for (int y = 0; y < scale; y++)
            {
                for (int x = 0; x < scale; x++)
                {
                    for (int i = 0; i < texture.height; i++)
                    {
                        for (int j = 0; j < texture.width; j++)
                        {
                            Color color = texture.GetPixel(i, j);
                            dump.SetPixel(x * texture.width + j, y * texture.height + i, color);
                        }
                    }
                }
            }
        }
        dump.filterMode = FilterMode.Point;
        dump.Apply();
        return dump;
    }
}
public struct Point
{
    public Point(int X, int Y)
    {
        x = X;
        y = Y;
    }
    public int x { get; set; }
    public int y { get; set; }
    public Vector2 ToVector2()
    {
        return new Vector2(x, y);
    }
    public static bool operator==(Point left, Point right)
    {
        if (left.x == right.x && left.y == right.y)
        {
            return true;
        }
        return false;
    }
    public static bool operator!=(Point left, Point right)
    {
        if (left.x == right.x && left.y == right.y)
        {
            return false;
        }
        return true;
    }
    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
public struct Size
{
    public Size(int Width, int Height)
    {
        width = Width;
        height = Height;
    }
    public int width { get; set; }
    public int height { get; set; }
    public Vector2 ToVector2()
    {
        return new Vector2(width, height);
    }
}

[System.Serializable]
public class PongGameEditor
{
    [Header("User Interface")]
    public Color cameraColor = new Color(0, 0, 0, 255);
    public KeyCode startGame = KeyCode.Space;
    public KeyCode exitGame = KeyCode.Escape;
    public Color playerScoreColor = new Color(1, 1, 1, 255);
    public int playerScoreFontSize = 16;
    public FontStyle playerScoreFontStyle = FontStyle.Bold;
    [Space]
    public Color enemyScoreColor = new Color(1, 1, 1, 255);
    public int enemyScoreFontSize = 16;
    public FontStyle enemyScoreFontStyle = FontStyle.Bold;
    [Space]
    public Color statusDisplayColor = new Color(1, 1, 1, 255);
    public int statusDisplayFontSize = 18;
    public FontStyle statusDisplayFontStyle = FontStyle.Bold;
    [Space]
    public Color popupDisplayColor = new Color(1, 1, 1, 255);
    public int popupDisplayFontSize = 11;
    public FontStyle popupDisplayFontStyle = FontStyle.Normal;
    public float timeBetweenPopup = 1;
    [Space]
    public Color timeDisplayColor = new Color(1, 1, 1, 255);
    public int timeDisplayFontSize = 16;
    public FontStyle timeDisplayFontStyle = FontStyle.Bold;
    public bool autoRestartGame = true;
    [Header("Gameplay")]
    public Vector2 gameSize = new Vector2(17, 11);
    public int scorePerWin = 1;
    public string winText = "Win!";
    public bool twoPlayer = false;
    public float distanceFromWall = 1;
    public bool infiniteGameTime = false;
    //Can only enable when infinite game time is not enable.
    public bool infiniteScore = false;
    public float gameTime = 300f;
    [Space]
    public string playerName = "Player";
    public int playerStartScore = 0;
    public int playerWinScore = 10;
    public float playerSpeed = 3;
    public float playerDashMultipler = 1.5f;
    public PlayerPongGameInput playerInput;
    public enum PlayerStartLocation { Left, Right };
    public PlayerStartLocation playerStartLocation = PlayerStartLocation.Left;
    [Space]
    public string enemyName = "Enemy";
    public int enemyStartScore = 0;
    public int enemyWinScore = 10;
    public float enemySpeed = 3;
    public float enemyDashMultipler = 1.5f;
    public EnemyPongGameInput enemyInput;
    [Space]
    public float ballStartSpeed = 3;
    public bool increaseSpeedByBouncing = true;
    public float ballMaxSpeed = 6;
    public int maxIncreaseTouched = 100;
    //Can't use if using change ball speed by collider
    public AnimationCurve ballSpeedSlope = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool changeBallSpeedByCollider = false;
    public bool allowRotationByCollider = false;
    public enum BallStartDirection { Left, Right, Random };
    public BallStartDirection ballStartDirection = BallStartDirection.Random;
    [Header("Mod")]
    //If the ball touched the win wall, end the game. 
    public bool suddenDeath = false;
    [Header("Score System")]
    public int goalScore = 10;
}
[System.Serializable]
public class PlayerPongGameInput
{
    public KeyCode up = KeyCode.W;
    public KeyCode down = KeyCode.S;
    public KeyCode dash = KeyCode.LeftShift;
}
[System.Serializable]
public class EnemyPongGameInput
{
    public KeyCode up = KeyCode.UpArrow;
    public KeyCode down = KeyCode.DownArrow;
    public KeyCode dash = KeyCode.RightShift;
}
