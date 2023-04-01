using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class ColoredCube : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    public MeshRenderer cubeMeshRenderer;
    public TextMesh colorblindText;
    public TextMesh indexText;

    public KMSelectable CubeButton;
    public KMSelectable BackFace;
    public KMSelectable RightFace;
    public KMSelectable FrontFace;
    public KMSelectable LeftFace;

    Color[] colorList = { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.white, Color.black };
    string[] colorNamesList = { "R", "G", "B", "Y", "M", "W", "K" };
    string[] colorFullNamesList = { "Red", "Green", "Blue", "Yellow", "Magenta", "White", "Black" };

    List<int> grid = new List<int>
    {0, 4, 2, 5, 1, 6, 3,
     5, 6, 0, 1, 3, 2, 4,
     6, 2, 3, 4, 0, 5, 1,
     1, 3, 4, 2, 6, 0, 5,
     4, 0, 6, 3, 5, 1, 2,
     3, 5, 1, 0, 2, 4, 6,
     2, 1, 5, 6, 4, 3, 0};

    int curIndex;
    int curPosition;
    int targetTime;
    List<int> colorIndexes = new List<int>() { };
    List<int> targetPositions = new List<int>() { };

    void Awake()
    {
        ModuleId = ModuleIdCounter++;

        CubeButton.OnInteract += delegate () { MiddlePress(); return false; };
        BackFace.OnInteract += delegate () { BackPress(); return false; };
        RightFace.OnInteract += delegate () { RightPress(); return false; };
        FrontFace.OnInteract += delegate () { FrontPress(); return false; };
        LeftFace.OnInteract += delegate () { LeftPress(); return false; };
    }

    void MiddlePress()
    {
        Debug.LogFormat("Colored Cube #{0}. The middle button was pressed at the last digit of the timer being {1}.", ModuleId, Math.Floor(Bomb.GetTime() % 60 % 10));
        if (Math.Floor(Bomb.GetTime() % 60 % 10) == targetTime)
        {
            Debug.LogFormat("Colored Cube #{0}. The last digit of the timer matched the number gotten from the table, cycling the cube...", ModuleId);
            curIndex++;
            curIndex %= 3;
            CubeCycle();
        }
        else
        {
            Debug.LogFormat("Colored Cube #{0}. The last digit of the timer did not match the number gotten from the table, submitting current position.", ModuleId);
            Submit();
        }
    }

    void Submit()
    {
        Debug.LogFormat("Colored Cube #{0}. Submitted position is {1}.", ModuleId, curPosition);
        if (targetPositions.Contains(curPosition))
        {
            Debug.LogFormat("Colored Cube #{0}. Submitted position is one of the target positions. Correct!", ModuleId);
            targetPositions.Remove(curPosition);
            if (targetPositions.Count == 0)
            {
                Debug.LogFormat("Colored Cube #{0}. All target positions were submitted, the module is solved!", ModuleId);
                GetComponent<KMBombModule>().HandlePass();
            }
        }
        else
        {
            Debug.LogFormat("Colored Cube #{0}. Submitted position is not one of the target positions, STRIKE!", ModuleId);
            GetComponent<KMBombModule>().HandleStrike();
        }
    }

    void BackPress()
    {
        if (curPosition - 7 < 0)
        {
            Debug.LogFormat("Colored Cube #{0}. The back face of the cube was pressed, but there is a wall in that direction, Strike!", ModuleId);
            GetComponent<KMBombModule>().HandleStrike();
        }
        else
        {
            if (grid[curPosition - 7] == colorIndexes[1] && !targetPositions.Contains(curPosition - 7))
            {
                Debug.LogFormat("Colored Cube #{0}. The back face of the cube was pressed, but the cell in that direction is labeled with second color of the sequence, Strike!", ModuleId);
                GetComponent<KMBombModule>().HandleStrike();
            }
            else
            {
                curPosition -= 7;
                Debug.LogFormat("Colored Cube #{0}. The back face of the cube was pressed, moving up, current position: {1}", ModuleId, curPosition);
            }        
        }
    }

    void RightPress()
    {
        if ((curPosition + 1) % 7 == 0)
        {
            Debug.LogFormat("Colored Cube #{0}. The right face of the cube was pressed, but there is a wall in that direction, Strike!", ModuleId);
            GetComponent<KMBombModule>().HandleStrike();
        }
        else
        {
            if (grid[curPosition + 1] == colorIndexes[1] && !targetPositions.Contains(curPosition + 1))
            {
                Debug.LogFormat("Colored Cube #{0}. The right face of the cube was pressed, but the cell in that direction is labeled with second color of the sequence, Strike!", ModuleId);
                GetComponent<KMBombModule>().HandleStrike();
            }
            else
            {
                curPosition += 1;
                Debug.LogFormat("Colored Cube #{0}. The right face of the cube was pressed, moving right, current position: {1}", ModuleId, curPosition);
            }
        }
    }

    void FrontPress()
    {
        if (curPosition + 7 > 48)
        {
            Debug.LogFormat("Colored Cube #{0}. The front face of the cube was pressed, but there is a wall in that direction, Strike!", ModuleId);
            GetComponent<KMBombModule>().HandleStrike();
        }
        else
        {
            if (grid[curPosition + 7] == colorIndexes[1] && !targetPositions.Contains(curPosition + 7))
            {
                Debug.LogFormat("Colored Cube #{0}. The front face of the cube was pressed, but the cell in that direction is labeled with second color of the sequence, Strike!", ModuleId);
                GetComponent<KMBombModule>().HandleStrike();
            }
            else
            {
                curPosition += 7;
                Debug.LogFormat("Colored Cube #{0}. The front face of the cube was pressed, moving down, current position: {1}", ModuleId, curPosition);
            }
        }
    }

    void LeftPress()
    {
        if (curPosition % 7 == 0)
        {
            Debug.LogFormat("Colored Cube #{0}. The left face of the cube was pressed, but there is a wall in that direction, Strike!", ModuleId);
            GetComponent<KMBombModule>().HandleStrike();
        }
        else
        {
            if (grid[curPosition - 1] == colorIndexes[1] && !targetPositions.Contains(curPosition - 1))
            {
                Debug.LogFormat("Colored Cube #{0}. The left face of the cube was pressed, but the cell in that direction is labeled with second color of the sequence, Strike!", ModuleId);
                GetComponent<KMBombModule>().HandleStrike();
            }
            else
            {
                curPosition -= 1;
                Debug.LogFormat("Colored Cube #{0}. The left face of the cube was pressed, moving left, current position: {1}", ModuleId, curPosition);
            }
        }
    }

    void Start()
    {
        colorIndexes.Add(Rnd.Range(0, 7));
        colorIndexes.Add(Rnd.Range(0, 7));
        colorIndexes.Add(Rnd.Range(0, 7));
        Debug.LogFormat("Colored Cube #{0}. Generated colors are {1}, {2}, {3}.", ModuleId, colorFullNamesList[colorIndexes[0]], colorFullNamesList[colorIndexes[1]], colorFullNamesList[colorIndexes[2]]);

        curPosition = 7 * colorIndexes[1] + colorIndexes[2];
        Debug.LogFormat("Colored Cube #{0}. Starting positiong is {1} in the grid.", ModuleId, "ABCDEFG"[colorIndexes[2]].ToString() + (colorIndexes[1] + 1).ToString());

        CubeCycle();
        CalculateTime();
        CalculateTargetPositions();
    }

    void Update()
    {
        
    }

    void CalculateTime()
    {
        if (colorIndexes[0] == 0)
        {
            targetTime = (Bomb.GetSerialNumberLetters().ToList().Count + 8) % 10;
        }
        else if (colorIndexes[0] == 1)
        {
            targetTime = (Bomb.GetSerialNumberNumbers().ToList().Count + 3) % 10;
        }
        else if (colorIndexes[0] == 2)
        {
            targetTime = (Bomb.GetOffIndicators().Count() + 1) % 10;
        }
        else if (colorIndexes[0] == 3)
        {
            targetTime = (Bomb.GetBatteryCount() + 5) % 10;
        }
        else if (colorIndexes[0] == 4)
        {
            targetTime = (Bomb.GetPortCount() + 9) % 10;
        }
        else if (colorIndexes[0] == 5)
        {
            targetTime = (Bomb.GetOnIndicators().Count() + 4) % 10;
        }
        else if (colorIndexes[0] == 6)
        {
            targetTime = ((Bomb.GetSerialNumberNumbers().ToList())[(Bomb.GetSerialNumberNumbers()).ToList().Count - 1] + 7) % 10;
        }
        Debug.LogFormat("Colored Cube #{0}. To cycle the cube, the middle of the it should be pressed at the last digit of the timer being {1}.", ModuleId, targetTime);
    }

    void CalculateTargetPositions()
    {
        string serialNum = Bomb.GetSerialNumber().ToString();
        List<string> pairs = new List<string> { };
        pairs.Add(serialNum[0].ToString() + serialNum[1].ToString());
        pairs.Add(serialNum[2].ToString() + serialNum[3].ToString());
        pairs.Add(serialNum[4].ToString() + serialNum[5].ToString());

        for (int i = 0; i < 3; i++)
        {
            string curPair = pairs[i];
            List<int> curPairNumber = new List<int> { };

            for (int j = 0; j < 2; j++)
            {
                if ("ABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(curPair[j].ToString()))
                {
                    curPairNumber.Add("ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(curPair[j]) + 1);
                }
                else
                {
                    if (curPair[j].ToString() == "0")
                    {
                        curPairNumber.Add(1);
                    }
                    else
                    {
                        curPairNumber.Add("123456789".IndexOf(curPair[j]) + 1);
                    }
                }
            }
            while (curPairNumber[0] > 7)
            {
                curPairNumber[0] -= 7;
            }
            while (curPairNumber[1] > 7)
            {
                curPairNumber[1] -= 7;
            }

            targetPositions.Add((curPairNumber[0] - 1) * 7 + curPairNumber[1] - 1);
        }
        Debug.LogFormat("Colored Cube #{0}. Target positions are {1}, {2} and {3}", ModuleId, targetPositions[0], targetPositions[1], targetPositions[2]);
    }

    void CubeCycle()
    {
        Color curColor = colorList[colorIndexes[curIndex]];
        string curColorName = colorNamesList[colorIndexes[curIndex]];

        cubeMeshRenderer.material.color = curColor;
        if (colorIndexes[curIndex] != 5 & colorIndexes[curIndex] != 6)
        {
            colorblindText.text = curColorName;
        }
        else
        {
            colorblindText.text = "";
        }

        indexText.text = (curIndex + 1).ToString();
        if (colorIndexes[curIndex] == 5)
        {
            indexText.color = Color.black;
        }
        else
        {
            indexText.color = Color.white;
        }
        Debug.LogFormat("Colored Cube #{0}. Current color is {1}, which is position {2} in the sequence.", ModuleId, colorFullNamesList[colorIndexes[curIndex]], curIndex + 1);
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} help is not a thing. :D";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
    }
}
