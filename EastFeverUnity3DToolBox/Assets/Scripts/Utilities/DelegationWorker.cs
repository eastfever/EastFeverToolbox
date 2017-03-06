using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 2016.12.30 by east_fever.
// 특정 작업을 다른 오브젝트로부터 인계 받아서,
// 요구 받은 시점에 실행 시킨다.
public class DelegationWorker : MonoBehaviour
{
    private static DelegationWorker s_instance = null;
    private static DelegationWorker Instance
    {
        get
        {
            if( null == s_instance )
            {
                GameObject newWorker = new GameObject( "DelegationWorker" );
                s_instance = newWorker.AddComponent<DelegationWorker>();
            }
            return s_instance;
        }
    }

    // 예약 작업을 맡길 작업자 생성.
    public static void CreateReservedJob( float waitingSeconds, System.Action job )
    {
        Instance.AssignReservedJob( job, waitingSeconds );
    }

    // 매 프레임마다 작업을 실행해 줄 작업자 생성.
    public static void CreateJobOnUpdate( float life, System.Action job )
    {
        Instance.AssignJobOnUpdate( job, life );
    }

    // 주어진 주기마다 작업을 실행해 줄 작업자 생성.
    public static void CreateJobOnPeriod( float period, float life, System.Action job )
    {
        GameObject workerObject = new GameObject( "WorkerOnPeriod" );
        DelegationWorker worker = workerObject.AddComponent<DelegationWorker>();
        worker.AssignJobOnPeriod( job, period, life );
    }

    // 작업을 어떻게 처리할지 여부.
    private enum eDelegationType
    {
        Unknown,
        ReservedJob,    // 일정 시간 후 실행.
        JobOnUpdate,    // 매 프레임마다 실행.
        JobOnPeriod,    // 일정 간격으로 실행.
    }

    // Worker가 처리해야 할 작업에 대한 정보.
    private class Job
    {
        public System.Action ActionOnTime = null;
        public eDelegationType Type = eDelegationType.Unknown;
        public float ElapsedSeconds = 0f;
        public float PeriodSeconds = 0f;        
        public float Life = 0f;
    }
    private List<Job> _jobList = new List<Job>();
    private List<Job> _jobsToDestroy = new List<Job>();    

    private void OnDestroy()
    {
        _jobsToDestroy.Clear();
        _jobsToDestroy = null;
        _jobList.Clear();
        _jobList = null;

        s_instance = null;
    }

    private void AssignJobOnUpdate( System.Action job, float life )
    {
        Job newJob = new Job();
        newJob.ActionOnTime = job;
        newJob.Life = life;
        newJob.Type = eDelegationType.JobOnUpdate;
        _jobList.Add( newJob );
    }

    private void AssignReservedJob( System.Action job, float waitingSeconds )
    {       
        Job newJob = new Job();
        newJob.ActionOnTime = job;
        newJob.Life = waitingSeconds;
        newJob.Type = eDelegationType.ReservedJob;
        _jobList.Add( newJob );
    }

    private void AssignJobOnPeriod( System.Action job, float period, float life )
    {
        Job newJob = new Job();
        newJob.Life = life;
        newJob.ActionOnTime = job;        
        newJob.PeriodSeconds = period;
        newJob.Type = eDelegationType.JobOnPeriod;
        _jobList.Add( newJob );        
    }

    // Update is called once per frame
    void Update()
    {
        int jobCount = _jobList.Count;
        for( int i = 0; i < jobCount; i++ )
        {
            UpdateJobStates( _jobList[ i ] );            
        }
        RemoveNoLifeJobs();
    }

    private void UpdateJobStates( Job job )
    {
        job.Life -= Time.deltaTime;
        if( job.Life <= 0f )
        {
            // 수명이 다한 작업의 처리.
            if( job.Type == eDelegationType.ReservedJob )
            {
                job.ActionOnTime();
            }
            _jobsToDestroy.Add( job );
            return;
        }

        if( eDelegationType.JobOnUpdate == job.Type )
        {
            // 매 업데이트마다 실행해 주어야 하는 작업.
            job.ActionOnTime();
        }
        else if( eDelegationType.JobOnPeriod == job.Type )
        {
            // 정해진 주기마다 실행해 주어야 하는 작업.
            job.ElapsedSeconds += Time.deltaTime;
            if( job.ElapsedSeconds > job.PeriodSeconds )
            {
                job.ElapsedSeconds -= job.PeriodSeconds;
                Mathf.Min( 0f, job.ElapsedSeconds );
                job.ActionOnTime();
            }
        }        
    }

    private void RemoveNoLifeJobs()
    {
        int countToRemove = _jobsToDestroy.Count;
        if( countToRemove <= 0 )
        {
            return;
        }

        for( int i = 0; i < countToRemove; i++ )
        {
            _jobList.Remove( _jobsToDestroy[ i ] );
        }
        _jobsToDestroy.Clear();
    }
}
