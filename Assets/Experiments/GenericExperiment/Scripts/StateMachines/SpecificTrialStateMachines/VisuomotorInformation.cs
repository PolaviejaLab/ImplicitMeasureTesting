﻿using UnityEngine;
using System.Collections;


/**
 * Events handles by the Trial statemachine
 */
public enum VisuomotorAgencyEvents
{
    WaveFinished,
    ThreatDone,
};


/**
 * States of the Trial statemachine
 */
public enum VisuomotorAgencyStates
{
    Idle,                       // Get used to the environment
    ExperimentWave,             // One event of reaching-like task
    Interval,                   // In between measures and task
    Threat,                     // Knife
    End,                        // End of the trial
};




public class VisuomotorAgency : ICStateMachine<VisuomotorAgencyStates, VisuomotorAgencyEvents>
{
    // Reference to the experiment controller
    public TrialController trialController;
    public WaveController waveController;
    public Threat threatController;
    
    // Scripts to manipulate the hand and offset according to condition
    public HandSwitcher handSwitcher;
    public Noise noiseType;

    private bool threatDone;

    public void Start()
    {
    }

    protected override void OnStart()
    {
        switch (trialController.noiseType) {
            case 0:
                noiseType = Noise.Control;
                trialController.delayWave = 0.0f;
                break;

            case 1:
                noiseType = Noise.ImpairedMovement;
                trialController.delayWave = 0.0f;
                break;

            case 2:
                noiseType = Noise.ImpairedOutcome;
                trialController.delayWave = 0.5f;
                break;

            case 3:
                noiseType = Noise.BothImpaired;
                trialController.delayWave = 0.5f;
                break;
        }
        WriteLog("Noise type: " + noiseType);
        WriteLog("Delay collision active: " + trialController.delayWave);
    }


    public void HandleEvent(VisuomotorAgencyEvents ev)
    {
        Debug.Log("Event " + ev.ToString());

        if (!IsStarted())
            return;

        switch (GetState())
        {
            case VisuomotorAgencyStates.Idle:
                break;

            case VisuomotorAgencyStates.ExperimentWave:
                if (ev == VisuomotorAgencyEvents.WaveFinished)
                    ChangeState(VisuomotorAgencyStates.Interval);
                break;

            case VisuomotorAgencyStates.Threat:
                if (ev == VisuomotorAgencyEvents.ThreatDone)
                    ChangeState(VisuomotorAgencyStates.End);
                break;

            case VisuomotorAgencyStates.End:
                break;
        }
    }


    public void Update()
    {
        if (!IsStarted())
            return;

        switch (GetState())
        {
            case VisuomotorAgencyStates.Idle:
                if (GetTimeInState() > 1.5f)
                    ChangeState(VisuomotorAgencyStates.ExperimentWave);
                break;


            case VisuomotorAgencyStates.Interval:
                if (trialController.currentWave <= trialController.wavesRequired)
                    ChangeState(VisuomotorAgencyStates.ExperimentWave);
                if (trialController.currentWave == trialController.wavesRequired)
                    ChangeState(VisuomotorAgencyStates.Threat);
                break;

            case VisuomotorAgencyStates.ExperimentWave:
                if (GetTimeInState() > 0.5f)
                    waveController.StartMachine();
                break;
                 
                case VisuomotorAgencyStates.Threat:
                if (!threatDone) {
                    threatController.Stopped += (sender, e) => { HandleEvent(VisuomotorAgencyEvents.ThreatDone); };
                    threatDone = true;
                }
                
                break;

            case VisuomotorAgencyStates.End:
                break;
        }
    }


    protected override void OnEnter(VisuomotorAgencyStates oldState)
    {

        switch (GetState())
        {
            case VisuomotorAgencyStates.Idle:
                handSwitcher.showRightHand = true;
                break;

            case VisuomotorAgencyStates.ExperimentWave:
                break;

            case VisuomotorAgencyStates.Threat:
                threatController.StartMachine();
                threatController.ChangeState(ThreatState.Falling);
                break;
                
            case VisuomotorAgencyStates.End:
                trialController.HandleEvent(TrialEvents.SpecificTrialFinished);
                this.StopMachine();
                break;
        }
    }


    protected override void OnExit(VisuomotorAgencyStates newState)
    {
        switch (GetState())
        {
            case VisuomotorAgencyStates.Idle:
                handSwitcher.showLeftHand = false;
                break;

            case VisuomotorAgencyStates.ExperimentWave:
                waveController.StopMachine();
                break;

            case VisuomotorAgencyStates.Threat:
                threatController.threat.SetActive(false);
                break;

            case VisuomotorAgencyStates.End:
                break;
        }
    }
}