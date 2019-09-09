using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ManageGame : MonoBehaviour {

    public Text timerText;
    public Text finalTimerText;

    public GameObject walls;
    public GameObject brick;
    public GameObject finishBrick;
    public GameObject obstacles;
    public GameObject obstacle;
    public int numOfObstacles;
    public float minDistance;
    public float obstacleRadius;
    public int xSize;
    public int ySize;
    public float xMultiplier;
    public float yMultiplier;
    public string seed;
    public bool useRandomSeed;
    public bool gameFinished;

    private int[,] map;
    private GameObject[] obsOnMap;
    private float xOffset;
    private float yOffset;
    private float timer;

    // Use this for initialization
    void Start()
    {
        xOffset = xSize / 2 + 0.5f;
        yOffset = 1;
        //zOffset = zSize / 2;

        map = new int[xSize + 2, ySize + 2];
        int[,] partialMap = CarveMap((xSize/2), 0, xSize, ySize);
        FillMap(ref partialMap, 1, 1, (xSize), (ySize));
        AddBorders();
        GenerateWalls();
        GenerateObstacles();

        gameFinished = false;
        timer = 0f;
        finalTimerText.text = "";
    }

    // ORIGIN AND FINISH COORDINATES WITH RESPECT TO THE GENERAL MAP
    private void FillMap(ref int[,] partialMap, int origX, int origY, int finX, int finY)
    {
        int width = (finX - origX) + 1;
        int height = (finY - origY) + 1;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                map[(origX + i), (origY + j)] = partialMap[i, j];
            }
        }
    }

    private void AddBorders()
    {
        for (int i = 0; i < xSize + 2; i++) // fill bottom border
        {
            map[i, 0] = 1; // bottom border
            //map[i, ySize + 1] = 1; // top border
        }
        for (int i = 1; i < ySize + 2; i++) // fill left border
        {
            map[0, i] = 1; // right border 
            map[xSize+1, i] = 1; // left border
        }
    }

    private int[,] CarveMap(int startX, int startY, int width, int height)
    {
        int[,] partialMap = new int[width, height];

        if (!useRandomSeed)
        {
            Random.InitState(seed.GetHashCode());
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (j >= startY && j < (startY + 3))
                {
                    partialMap[i, j] = 0;
                }
                else
                {
                    partialMap[i, j] = 1;
                }

            }
            
        }

        int currentX = startX;
        int currentY = startY + 3;


        for (int i = currentY; i < height; i++)
        {
            float updateWidth = Random.value;

            if (updateWidth < 0.25f)
            {
                // width of opening = 1
                partialMap[currentX, i] = 0;
            }
            else if (updateWidth < 0.5f)
            {
                // width of opening = 2
                partialMap[currentX, i] = 0;
                if ((currentX - 1) >= 0)
                {
                    partialMap[currentX - 1, i] = 0;
                }

            }
            else if (updateWidth < 0.75f)
            {
                // width of opening = 2
                partialMap[currentX, i] = 0;
                if ((currentX + 1) <= (width - 1))
                {
                    partialMap[currentX + 1, i] = 0;
                }
            }
            else
            {
                // width of opening = 3
                partialMap[currentX, i] = 0;
                if ((currentX + 1) <= (width - 1))
                {
                    partialMap[currentX + 1, i] = 0;
                }
                if ((currentX - 1) >= 0)
                {
                    partialMap[currentX - 1, i] = 0;
                }
            }

            float updateX = Random.value;
            if (updateX < 1 / 3f)
            {
                if ((currentX - 1) >= 0)
                {
                    currentX = currentX - 1;
                }
            }
            else if (updateX < 2 / 3f)
            {
                if ((currentX + 1) <= (width - 1))
                {
                    currentX = currentX + 1;
                }
            }

            if (partialMap[currentX, i] != 0)
            {
                partialMap[currentX, i] = 0;
            }

        }


        return partialMap;
    }

    private void GenerateWalls()
    {
        for (int i = 0; i < xSize + 2; i++)
        {
            for (int j = 0; j < ySize + 2; j++)
            {
                if (map[i, j] == 1)
                {
                    Instantiate(brick, new Vector3((i - xOffset) * xMultiplier, (j - yOffset) * yMultiplier, 0f), Quaternion.identity, walls.transform);
                }
            }
        }

        for (int i = 0; i < xSize + 2; i++)
        {
            Instantiate(finishBrick, new Vector3((i - xOffset) * xMultiplier, (ySize+1 - yOffset) * yMultiplier, 0f), Quaternion.identity, walls.transform);
        }

    }

    private Vector2 GetWorldCoords(float xCoord, float yCoord)
    {
        // for given 2 coordinates, calculate the resulting coordinates that should be used with the offsets
        Vector2 coords = new Vector2(xCoord - (xOffset * xMultiplier), yCoord - (yOffset * yMultiplier));
        return coords;
    }

    private bool IsCoordUnoccupied(float xCoord, float yCoord)
    {
        Vector2 pos = GetWorldCoords(xCoord, yCoord);
        if (Physics2D.OverlapCircle(pos, obstacleRadius))
        {
            return false;
        }
        else
        {
            return true;
        }
    }    

    private bool FarEnough(float xCoord, float yCoord)
    {
        Vector2 pos = GetWorldCoords(xCoord, yCoord);

        obsOnMap = GameObject.FindGameObjectsWithTag("Obstacle");

        foreach (GameObject obs in obsOnMap)
        {
            Vector2 obsPos = new Vector2(obs.transform.position.x, obs.transform.position.y);
            if (Vector2.Distance(obsPos, pos) < minDistance)
            {
                return false;
            }
        }

        if (Vector2.Distance(new Vector2(0, 0), pos) < minDistance)
        {
            return false;
        }

        return true;
    }

    private void GenerateObstacles()
    {
        if (!useRandomSeed)
        {
            Random.InitState(seed.GetHashCode());
        }


        for (int i = 0; i < numOfObstacles; i++)
        {
            // generate attractions
            float obsX;
            float obsY;
            do
            {
                obsX = Mathf.RoundToInt(Random.Range(0f, xSize * xMultiplier)) + 0.5f;
                obsY = Mathf.RoundToInt(Random.Range(4 * xMultiplier, ySize * yMultiplier)) + 0.5f;
            } while (!IsCoordUnoccupied(obsX, obsY) || !FarEnough(obsX, obsY));

            Vector2 coords = GetWorldCoords(obsX, obsY);
            Instantiate(obstacle, new Vector3(coords.x, coords.y, 0f), Quaternion.identity, obstacles.transform);
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (!gameFinished)
        {
            timer = timer + Time.deltaTime;
            timerText.text = "Timer: " + timer.ToString("F2");
        }
        else
        {
            finalTimerText.text = "Timer: " + timer.ToString("F2");
            timerText.text = "";
            if (Input.GetButtonDown("Submit"))
            {
                timerText.text = "";
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        if (Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }
    }
}
