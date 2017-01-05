using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EastFever
{
    // 2016.12.30 by east_fever.
    // 특정 작업을 다른 오브젝트로부터 인계 받아서,
    // 요구 받은 시점에 실행 시킨다.
    public class DelegationWorker : MonoBehaviour
    {
        // 예약 작업을 맡길 작업자 생성.
        public static void CreateReservedJob( float waitingSeconds, System.Action job )
        {
            GameObject workerObject = new GameObject( "WorkerOnReservedJob" );
            DelegationWorker worker = workerObject.AddComponent<DelegationWorker>();
            worker.AssignReservedJob( job, waitingSeconds );
        }

        // 매 프레임마다 작업을 실행해 줄 작업자 생성.
        public static void CreateJobOnUpdate( float life, System.Action job )
        {
            GameObject workerObject = new GameObject( "WorkerOnUpdate" );
            DelegationWorker worker = workerObject.AddComponent<DelegationWorker>();
            worker.AssignJobOnUpdate( job, life );
        }

        // 주어진 주기마다 작업을 실행해 줄 작업자 생성.
        public static void CreateJobOnPeriod( float period, float life, System.Action job )
        {
            GameObject workerObject = new GameObject( "WorkerOnPeriod" );
            DelegationWorker worker = workerObject.AddComponent<DelegationWorker>();
            worker.AssignJobOnPeriod( job, period, life );
        }

        private enum eDelegationType
        {
            ReservedJob,
            JobOnUpdate,
            JobOnPeriod,
        }
        private eDelegationType _jobType = eDelegationType.ReservedJob;
        private System.Action _job;

        private float _elapsedTime = 0f;
        private float _periodJobSeconds = 0f;
        private float _life = float.MaxValue;

        public void AssignJobOnUpdate( System.Action job, float life )
        {
            _job = job;
            _life = life;
            _jobType = eDelegationType.JobOnUpdate;
        }

        public void AssignReservedJob( System.Action job, float waitingSeconds )
        {
            _job = job;
            _life = waitingSeconds;
            _jobType = eDelegationType.ReservedJob;
        }

        public void AssignJobOnPeriod( System.Action job, float period, float life )
        {
            _job = job;
            _life = life;
            _periodJobSeconds = period;
            _jobType = eDelegationType.JobOnPeriod;
        }

        // Update is called once per frame
        void Update()
        {
            _life -= Time.deltaTime;
            if( _life <= 0f )
            {
                if( _jobType == eDelegationType.ReservedJob )
                {
                    _job();
                }
                Destroy( gameObject );
            }

            if( eDelegationType.JobOnUpdate == _jobType )
            {
                _job();
            }
            else if( eDelegationType.JobOnPeriod == _jobType )
            {
                _elapsedTime += Time.deltaTime;
                if( _elapsedTime > _periodJobSeconds )
                {
                    _elapsedTime = 0f;
                    _job();
                }
            }
        }
    }
}