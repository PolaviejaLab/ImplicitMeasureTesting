﻿using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System;


public enum QuestionnaireEvents {
    RoomOff,
    StartQuestionnaire,
    QuestionDisplayed,
    QuestionAnswered,
    }


public enum QuestionnaireStates {
    Idle,
    Start,
    ShowQuestion,
    WaitingForAnswer,
    Delay, 
    End,
}


public class QuestionnaireController : ICStateMachine<QuestionnaireStates, QuestionnaireEvents>
{
    public TrialController trialController;

    public GameObject informationScreen;
    public GameObject display;
    public Text text;

    string[] statements = new string[] {
       "Eu senti como se estivesse a olhar para a minha mão, ao invés de uma mão virtual",
       "Eu senti como se a mão virtual fosse minha mão",
       "Parecia que a mão virtual pertencia a mim",
       "Parecia que a mão virtual fazia parte do meu corpo",
       "Senti como se tivesse mais do que uma mão direita",
       "Eu senti que a minha mão estava a tornar virtual",
       "Senti como se a mão virtual se assemelhasse fisicamente à minha mão real em termos de forma, sardas e outras características",
       "Senti que podia controlar a mão virtual",
       "Senti que os movimentos da mão virtual foram causados por mim",
       "Eu senti como se a mão virtual estivesse a obedecer a minha vontade",
       "Senti que contralava a mão virtual como se fosse parte do meu corpo",
       "Senti que as luzes amarelas estavam a obedecer a minha vontade",
       "Senti que o movimento da minha mão estava a apagar as luzes amarelas",
       "Eu senti como se a mão virtual estivesses a controlar a minha mão",
       "Eu tive a sensção de esquecer a minha própia mão, e que estava a focar apenas no movimento da mão virtual",
       "Eu senti como se a mão virtual causasse o movimento da minha mão",
       "Eu senti como se as luzes alvo estivessem a desligar aleatoriamente",
       "Senti como se não tivesse controlo sobre as luzes alvo",
       "Parecia que minha mão estava no local da mão virtual",
       "Parecia que o movimento da minha mão estava localizado onde a mão virtual estava a mover"
    };

//string[] statements = new string[] {
//    "I felt as if I were looking at my hand, rather than a virtual hand",
//    "I felt as if the virtual hand was my hand",
//    "It seemed like the virtual hand belonged to me",
//    "It seemed that the virtual hand was part of my body",
//    "I felt as if I had more than one right hand",
//    "I felt like my real hand was turning virtual",
//    "I felt as if the virtual hand physically resembled my real hand in terms of shape, freckles and other features",
//    "I felt that I could control the virtual hand",
//    "I felt that the movements of the virtual hand were caused by me",
//    "I felt as if the virtual hand was obeying my will",
//    "I felt that I controlled the virtual hand as if it was part of my body",
//    "I felt that the lights were obeying my will",
//    "I felt that the movement of my hand was turning off the lights on the table",
//    "I felt as if the virtual hand was controlling my hand",
//    "I had the feeling of forgetting my own hand, focusing only on the movement of the virtual hand",
//    "I felt as if the virtual hand caused the movement of my hand",
//    "I felt as if the target lights turned off randomly",
//    "I felt as if I had no control over the target lights",
//    "It seemed like my hand was in the location of the virtual hand",
//    "It seemed as if the movement of my hand was located where the virtual hand was moving"
//};



    public int totalLength;
    public int currentStatement = 0;
    public int[] arrayNum;

    public StreamWriter questionnaireResults;
    public KeyCode cKey;
    public KeyCode responseLikert;

    
    public Light[] roomLights;
    public MaterialChanger[] roomWalls;
    private float intensity_initial;
    private bool lightsOff;


    protected override void OnStart()
    {
        questionnaireResults = OpenResultsFile();
        totalLength = statements.Length;
        Debug.Log("Total questions: " + totalLength.ToString());
        arrayNum = CreateNumericArray();

        intensity_initial = roomLights[0].intensity;
    }

    void Update()
    {
        if (!IsStarted())
            return;

        switch (GetState())
        {
            case QuestionnaireStates.Idle:
                break;

            case QuestionnaireStates.Start:
                if (GetTimeInState() > 1.0f)
                    ChangeState(QuestionnaireStates.ShowQuestion);
                break;

            case QuestionnaireStates.ShowQuestion:
                break;

            case QuestionnaireStates.WaitingForAnswer:
                if (Input.anyKeyDown)
                    foreach (KeyCode cKey in Enum.GetValues(typeof(KeyCode)))
                        if (Input.GetKey(cKey))
                        {
                            Debug.Log("Question Answer: " + cKey);
                            responseLikert = cKey;
                            HandleEvent(QuestionnaireEvents.QuestionAnswered);
                        }
                break;

            case QuestionnaireStates.Delay:
                break;
        }
    }

    public void HandleEvent(QuestionnaireEvents ev)
    {
        switch (GetState())
        {
            case QuestionnaireStates.Idle:
                if (ev == QuestionnaireEvents.RoomOff)
                    ChangeState(QuestionnaireStates.Start);
                break;

            case QuestionnaireStates.ShowQuestion:
                if (ev == QuestionnaireEvents.QuestionDisplayed)
                    ChangeState(QuestionnaireStates.WaitingForAnswer);
                break;

            case QuestionnaireStates.WaitingForAnswer:
                if (ev == QuestionnaireEvents.QuestionAnswered)
                    ChangeState(QuestionnaireStates.Delay);
                break;

            case QuestionnaireStates.Delay:
                break;

            case QuestionnaireStates.End:
                trialController.HandleEvent(TrialEvents.QuestionsFinished);
                break;
        }
    }

    protected override void OnEnter(QuestionnaireStates oldState)
    {
        switch (GetState())
        {
            case QuestionnaireStates.Idle:
                break;

            case QuestionnaireStates.Start:
                break;

            case QuestionnaireStates.ShowQuestion:
                display.SetActive(true);
                currentStatement = GetRandomNumber(arrayNum);
                Debug.Log("Question number: " + (currentStatement + 1).ToString());
                DisplayText();
                break;

            case QuestionnaireStates.WaitingForAnswer:

                break;

            case QuestionnaireStates.Delay:
                arrayNum = RemoveNumber(arrayNum, currentStatement);
                if (arrayNum == null)
                    ChangeState(QuestionnaireStates.End);
                else 
                    ChangeState(QuestionnaireStates.ShowQuestion);
                break;

            case QuestionnaireStates.End:
                questionnaireResults.Close();
                resetRoomLight();
                trialController.HandleEvent(TrialEvents.QuestionsFinished);
                break;
        }

    }

    protected override void OnExit(QuestionnaireStates newState)
    {
        switch (GetState())
        {
            case QuestionnaireStates.Idle:
                break;

            case QuestionnaireStates.Start:
                break;

            case QuestionnaireStates.ShowQuestion:

                break;

            case QuestionnaireStates.WaitingForAnswer:
                display.SetActive(false);
                RecordResponse(responseLikert);
                break;

            case QuestionnaireStates.Delay:
                break;

            case QuestionnaireStates.End:
                break;
        }

    }

    private string GetResultsFilename()
    {
        return trialController.experimentController.outputDirectory + "\\" + "Responses Trial " + trialController.experimentController.trialCounter + ".csv";
    }

    public StreamWriter OpenResultsFile()
    {
        Debug.Log("Document opened: " + GetResultsFilename().ToString());
        return questionnaireResults = new StreamWriter(GetResultsFilename(), true);
    }


    public int[] CreateNumericArray() {
        int[] arrayNum = new int[totalLength];
        for (int n = 0; n <= totalLength - 1; n++)
            arrayNum[n] = n;
        Debug.Log("Numeric array created");
        return arrayNum;
    }

    public void DisplayText()
    {
        text.text = statements[currentStatement].ToString();
        HandleEvent(QuestionnaireEvents.QuestionDisplayed);
    }

    public void RecordResponse(KeyCode response) {
        questionnaireResults.Write((currentStatement + 1).ToString());
        questionnaireResults.Write(", ");
        questionnaireResults.Write(response);
        questionnaireResults.WriteLine();
    }



    public int GetRandomNumber(int[] arrayInt)
    {
        int ind_ = UnityEngine.Random.Range(0, arrayInt.Length - 1);
        currentStatement = arrayInt[ind_];
        return currentStatement;
    }


    public int[] RemoveNumber(int[] arrayToRemove, int ind_)
    {
        int length = arrayToRemove.Length;
        int count = 0;
        int[] arrayLess = new int[length - 1];
        if (arrayLess.Length == 0)
        {
            return null;
        }
        else
        {
            for (int n = 0; n < length; n++)
            {
                if (arrayToRemove[n] != ind_)
                {
                    // int aa = count;
                    arrayLess[count] = arrayToRemove[n];
                    count++;
                }
            }
            return arrayLess;
        }
    }



    public void DimLights()
    {
        foreach (Light l in roomLights)
            while (l.intensity > 0)
                l.intensity = l.intensity - 0.0001f;
        
        TurnOffRoom();
    }


    private void TurnOffRoom()
    {
        informationScreen.SetActive(true);
        foreach (MaterialChanger i in roomWalls)
            i.activeMaterial = 1;
        trialController.table.SetActive(false);
        trialController.testLights.SetActive(false);
        trialController.feedback.SetActive(false);
        lightsOff = true;
        HandleEvent(QuestionnaireEvents.RoomOff);
    }


    private void resetRoomLight()
    {
        foreach (MaterialChanger i in roomWalls)
            i.activeMaterial = 0;
        foreach (Light l in roomLights)
            l.intensity = intensity_initial;
        trialController.table.SetActive(true);
        informationScreen.SetActive(false);
        trialController.feedback.SetActive(true);
        lightsOff = false;
    }

} 
