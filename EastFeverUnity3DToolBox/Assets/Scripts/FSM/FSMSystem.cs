using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public class FSMSystem<T> 
{
    private T Owner;
    private FSMState<T> CurrentState;
    private FSMState<T> PreviousState;
    private FSMState<T> GlobalState;

    private Dictionary<int, FSMState<T>> _stateTable = new Dictionary<int, FSMState<T>>();

    public FSMState<T> GetCurrentState() { return CurrentState; }
    public FSMState<T> GetPreviousState() { return PreviousState; }

    public FSMSystem()
    {
        CurrentState = null;
        PreviousState = null;
        GlobalState = null;
    }

    public void AddState( int stateCode, FSMState<T> state )
    {
        if( _stateTable.ContainsKey( stateCode ) )
        {
            Debug.LogError( "[FSMSystem]already registered state - " + stateCode );
            return;
        }

        _stateTable.Add( stateCode, state );
    }    

    public void Initialize( T owner, int stateCode )
    {
        if ( !_stateTable.ContainsKey( stateCode ) )
        {
            Debug.LogError( "[FSMSystem]not registered state - " + stateCode );
            return;
        }

        Initialize( owner, _stateTable[ stateCode ] );
    }

    public void Initialize(T owner, FSMState<T> InitialState)
    {
        Owner = owner;
        ChangeState(InitialState);
    }    

    public void ChangeState( int stateCode, float value1 = .0f, float value2 = .0f )
    {
        if( !_stateTable.ContainsKey( stateCode ) )
        {
            Debug.LogError( "[FSMSystem]not registered state - " + stateCode );
            return;
        }

        _stateTable[ stateCode ].Value1 = value1;
        _stateTable[ stateCode ].Value2 = value2;
        ChangeState( _stateTable[ stateCode ] );
    }

    public void ChangeState(FSMState<T> NewState)
    {
        //Debug.Log("Change State FSMSystem!");

        if( CurrentState == NewState )
        {
            return;
        }        

        PreviousState = CurrentState;

        if (CurrentState != null)
            CurrentState.Exit(Owner);
        CurrentState = NewState;
        if (CurrentState != null)
            CurrentState.Enter(Owner);

        //Debug.Log( "[FSMSystem]Current State - " + GetCurrentStateName() );
    }

    public void RevertToPreviousState()
    {
        if (PreviousState != null)
            ChangeState(PreviousState);
    }

    public void Update()
    {
        if ( GlobalState != null ) GlobalState.Execute( Owner );
        if ( CurrentState != null ) CurrentState.Execute( Owner );
    }

    public string GetCurrentStateName()
    {
        if (CurrentState != null)
            return CurrentState.GetType().Name;

        return string.Empty;
    }

    public string GetPreviousStateName()
    {
        if( PreviousState != null )
            return PreviousState.GetType().Name;

        return string.Empty;
    }
};
