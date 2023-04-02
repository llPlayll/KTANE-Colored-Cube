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
    public KMColorblindMode Colorblind;

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
    bool colorblindActive;

    void Awake()
    {
        ModuleId = ModuleIdCounter++;

        CubeButton.OnInteract += delegate () { MiddlePress(); return false; };
        BackFace.OnInteract += delegate () { BackPress(); return false; };
        RightFace.OnInteract += delegate () { RightPress(); return false; };
        FrontFace.OnInteract += delegate () { FrontPress(); return false; };
        LeftFace.OnInteract += delegate () { LeftPress(); return false; };

        colorblindActive = Colorblind.ColorblindModeActive;
        ColorblindSet();
    }

    void ColorblindSet()
    {
        colorblindText.gameObject.SetActive(colorblindActive);
    }

    void MiddlePress()
    {
        CubeButton.AddInteractionPunch();
        if (ModuleSolved)
        {
            return;
        }
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, CubeButton.transform);

        Debug.Log("Colored Cube #" + ModuleId + ". The middle button was pressed at the last digit of the timer being " + Math.Floor(Bomb.GetTime() % 60 % 10) + ".");
        if (Math.Floor(Bomb.GetTime() % 60 % 10) == targetTime)
        {
            Debug.Log("Colored Cube #" + ModuleId + ". The last digit of the timer matched the number gotten from the table, cycling the cube...");
            curIndex++;
            curIndex %= 3;
            CubeCycle();
        }
        else
        {
            Debug.Log("Colored Cube #" + ModuleId + ". The last digit of the timer did not match the number gotten from the table, submitting current position.");
            Submit();
        }
    }

    void Submit()
    {
        Debug.Log("Colored Cube #" + ModuleId + ". Submitted position is " + "ABCDEFG"[curPosition % 7].ToString() + (curPosition / 7 + 1).ToString() +  ".");
        if (targetPositions.Contains(curPosition))
        {
            Debug.Log("Colored Cube #" + ModuleId + ". Submitted position is one of the target positions. Correct!");
            targetPositions.Remove(curPosition);
            if (targetPositions.Count == 0)
            {
                Debug.Log("Colored Cube #" + ModuleId + ". All target positions were submitted, module solved!");
                GetComponent<KMBombModule>().HandlePass();
                ModuleSolved = true;
            }
        }
        else
        {
            Debug.Log("Colored Cube #" + ModuleId + ". Submitted position is not one of the target positions, Strike!");
            GetComponent<KMBombModule>().HandleStrike();
        }
    }

    void BackPress()
    {
        if (ModuleSolved)
        {
            return;
        }
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, BackFace.transform);

        if (curPosition - 7 < 0)
        {
            Debug.Log("Colored Cube #" + ModuleId + ". The back face of the cube was pressed, but there is a wall in that direction, Strike!");
            GetComponent<KMBombModule>().HandleStrike();
        }
        else
        {
            if (grid[curPosition - 7] == colorIndexes[1] && !targetPositions.Contains(curPosition - 7))
            {
                Debug.Log("Colored Cube #" + ModuleId + ". The back face of the cube was pressed, but the cell in that direction is labeled with second color of the sequence, Strike!");
                GetComponent<KMBombModule>().HandleStrike();
            }
            else
            {
                curPosition -= 7;
                Debug.Log("Colored Cube #" + ModuleId + ". The back face of the cube was pressed, moving up, current position: " + "ABCDEFG"[curPosition % 7].ToString() + (curPosition / 7 + 1).ToString() + ".");
            }        
        }
    }

    void RightPress()
    {
        if (ModuleSolved)
        {
            return;
        }
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, RightFace.transform);

        if ((curPosition + 1) % 7 == 0)
        {
            Debug.Log("Colored Cube #" + ModuleId + ". The right face of the cube was pressed, but there is a wall in that direction, Strike!");
            GetComponent<KMBombModule>().HandleStrike();
        }
        else
        {
            if (grid[curPosition + 1] == colorIndexes[1] && !targetPositions.Contains(curPosition + 1))
            {
                Debug.Log("Colored Cube #" + ModuleId + ". The right face of the cube was pressed, but the cell in that direction is labeled with second color of the sequence, Strike!");
                GetComponent<KMBombModule>().HandleStrike();
            }
            else
            {
                curPosition += 1;
                Debug.Log("Colored Cube #" + ModuleId + ". The right face of the cube was pressed, moving right, current position: " + "ABCDEFG"[curPosition % 7].ToString() + (curPosition / 7 + 1).ToString() + ".");
            }
        }
    }

    void FrontPress()
    {
        if (ModuleSolved)
        {
            return;
        }
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, FrontFace.transform);

        if (curPosition + 7 > 48)
        {
            Debug.Log("Colored Cube #" + ModuleId + ". The front face of the cube was pressed, but there is a wall in that direction, Strike!");
            GetComponent<KMBombModule>().HandleStrike();
        }
        else
        {
            if (grid[curPosition + 7] == colorIndexes[1] && !targetPositions.Contains(curPosition + 7))
            {
                Debug.Log("Colored Cube #" + ModuleId + ". The front face of the cube was pressed, but the cell in that direction is labeled with second color of the sequence, Strike!");
                GetComponent<KMBombModule>().HandleStrike();
            }
            else
            {
                curPosition += 7;
                Debug.Log("Colored Cube #" + ModuleId + ". The front face of the cube was pressed, moving down, current position: " + "ABCDEFG"[curPosition % 7].ToString() + (curPosition / 7 + 1).ToString());
            }
        }
    }

    void LeftPress()
    {
        if (ModuleSolved)
        {
            return;
        }
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, LeftFace.transform);

        if (curPosition % 7 == 0)
        {
            Debug.Log("Colored Cube #" + ModuleId + ". The left face of the cube was pressed, but there is a wall in that direction, Strike!");
            GetComponent<KMBombModule>().HandleStrike();
        }
        else
        {
            if (grid[curPosition - 1] == colorIndexes[1] && !targetPositions.Contains(curPosition - 1))
            {
                Debug.Log("Colored Cube #" + ModuleId + ". The left face of the cube was pressed, but the cell in that direction is labeled with second color of the sequence, Strike!");
                GetComponent<KMBombModule>().HandleStrike();
            }
            else
            {
                curPosition -= 1;
                Debug.Log("Colored Cube #" + ModuleId + ". The left face of the cube was pressed, moving left, current position: " + "ABCDEFG"[curPosition % 7].ToString() + (curPosition / 7 + 1).ToString() + ".");
            }
        }
    }

    void Start()
    {
        colorIndexes.Add(Rnd.Range(0, 7));
        colorIndexes.Add(Rnd.Range(0, 7));
        colorIndexes.Add(Rnd.Range(0, 7));
        Debug.Log("Colored Cube #" + ModuleId + ". Generated colors are " + colorFullNamesList[colorIndexes[0]] + ", " + colorFullNamesList[colorIndexes[1]] + ", " + colorFullNamesList[colorIndexes[2]] + ".");

        curPosition = 7 * colorIndexes[1] + colorIndexes[2];
        Debug.Log("Colored Cube #" + ModuleId + ". Starting position is " + "ABCDEFG"[colorIndexes[2]].ToString() + (colorIndexes[1] + 1).ToString() + " in the grid.");

        CubeCycle();
        CalculateTime();
        CalculateTargetPositions();
    }

    void CalculateTime()
    {
        switch (colorIndexes[0])
        {
            case 0:
                targetTime = (Bomb.GetSerialNumberLetters().ToList().Count + 8) % 10;
                break;
            case 1:
                targetTime = (Bomb.GetSerialNumberNumbers().ToList().Count + 3) % 10;
                break;
            case 2:
                targetTime = (Bomb.GetOffIndicators().Count() + 1) % 10;
                break;
            case 3:
                targetTime = (Bomb.GetBatteryCount() + 5) % 10;
                break;
            case 4:
                targetTime = (Bomb.GetPortCount() + 9) % 10;
                break;
            case 5:
                targetTime = (Bomb.GetOnIndicators().Count() + 4) % 10;
                break;
            case 6:
                targetTime = ((Bomb.GetSerialNumberNumbers().ToList())[(Bomb.GetSerialNumberNumbers()).ToList().Count - 1] + 7) % 10;
                break;
        }
        Debug.Log("Colored Cube #" + ModuleId + ". To cycle the cube, the middle of the cube should be pressed at the last digit of the timer being " + targetTime + ".");
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
        Debug.Log("Colored Cube #" + ModuleId + ". Target positions are " + "ABCDEFG"[targetPositions[0] % 7].ToString() + (targetPositions[0] / 7 + 1).ToString() + ", " + "ABCDEFG"[targetPositions[1] % 7].ToString() + (targetPositions[1] / 7 + 1).ToString() + " and " + "ABCDEFG"[targetPositions[2] % 7].ToString() + (targetPositions[2] / 7 + 1).ToString() + ".");
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
        Debug.Log("Colored Cube #" + ModuleId + ". Current color is " + colorFullNamesList[colorIndexes[curIndex]] + ", which is position " + (curIndex + 1) + " in the sequence.");
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} m/mid/middle to press the middle part of the cube at any time. !{0} m/mid/middle # to press the middle part of the cube at last digit of the timer being #. !{0} move u/b/r/d/f/l to move. Directions can be chained like so: !{0} move uurlf.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        var tokens = Command.ToLowerInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length == 0)
        {
            yield break;
        }

        switch (tokens[0])
        {
            case "m":
            case "mid":
            case "middle":
                if (tokens.Length == 1)
                {
                    CubeButton.OnInteract();
                    yield return new WaitForSeconds(0.1f);
                    break;
                }
                if (!"0123456789".Contains(tokens[1]) || tokens[1].Length > 1)
                {
                    yield return null;
                    yield return "sendtochaterror Invalid press time!";
                    yield break;
                }
                string pressTime = tokens[1];
                yield return null;

                string curLastDigit = Bomb.GetFormattedTime()[Bomb.GetFormattedTime().Length - 1].ToString();
                while (!(curLastDigit == pressTime))
                {
                    yield return null;
                    curLastDigit = Bomb.GetFormattedTime()[Bomb.GetFormattedTime().Length - 1].ToString();
                }
                CubeButton.OnInteract();
                yield return new WaitForSeconds(0.1f);
                break;
            case "move":
                if (tokens.Length == 1)
                {
                    yield return null;
                    yield return "sendtochaterror No moves given!";
                    yield break;
                }
                if (tokens.Length == 2)
                {
                    foreach (char token in tokens[1])
                    {
                        switch (token.ToString().ToLowerInvariant())
                        {
                            case "u":
                            case "b":
                                BackFace.OnInteract();
                                yield return new WaitForSeconds(0.1f);
                                break;
                            case "r":
                                RightFace.OnInteract();
                                yield return new WaitForSeconds(0.1f);
                                break;
                            case "d":
                            case "f":
                                FrontFace.OnInteract();
                                yield return new WaitForSeconds(0.1f);
                                break;
                            case "l":
                                LeftFace.OnInteract();
                                yield return new WaitForSeconds(0.1f);
                                break;
                            default:
                                yield break;
                        }
                    }
                }
                break;
            default:
                yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
    }
}
