using UnityEngine;
using System.Collections;

abstract public class FSMState <T>
{    
    public float Value1 { get; set; }
    public float Value2 { get; set; }

    abstract public void Enter( T owner );
    abstract public void Execute( T owner );
    abstract public void Exit( T owner );
}
