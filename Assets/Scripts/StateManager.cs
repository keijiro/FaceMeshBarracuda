using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IceMilkTea.Core;

namespace MediaPipe.FaceMesh
{
    public class StateManager : MonoBehaviour
    {

        [SerializeField] FaceSwap _faceSwap;
        [SerializeField] PipeLineManager _pipeLine;

        private class MyState : ImtStateMachine<StateManager, StateEvent>.State { }

        enum StateEvent
        {
            Lost,
            Detect,
            Capture,
            Captured,
            Swap,
            Swaped,
        }

        ImtStateMachine<StateManager, StateEvent> stateMachine;

        private float _lostBuffer = 0.2f;
        private float _currentBuffer = 0;

        void Awake()
        {
            stateMachine = new ImtStateMachine<StateManager, StateEvent>(this);

            stateMachine.AddTransition<DetectingState,IdleState>(StateEvent.Lost);
            stateMachine.AddTransition<CapturingState,IdleState>(StateEvent.Lost);
            stateMachine.AddTransition<CapturedState,IdleState>(StateEvent.Lost);
            stateMachine.AddTransition<SwappingState,IdleState>(StateEvent.Lost);
            stateMachine.AddTransition<SwappedState,IdleState>(StateEvent.Lost);

            stateMachine.AddTransition<IdleState, DetectingState>(StateEvent.Detect);
            stateMachine.AddTransition<DetectingState, CapturingState>(StateEvent.Capture);
            stateMachine.AddTransition<CapturingState, CapturedState>(StateEvent.Captured);
            stateMachine.AddTransition<CapturedState, SwappingState>(StateEvent.Swap);
            stateMachine.AddTransition<SwappingState, SwappedState>(StateEvent.Swaped);
            stateMachine.AddTransition<SwappedState, SwappingState>(StateEvent.Swap);

            stateMachine.SetStartState<IdleState>();
            stateMachine.Update();
        }

        void Update()
        {
            try
            {
                // どのステートであってもステート処理中に発生した例外はここまで転送されます
                stateMachine.Update();
            }
            catch (System.Exception error)
            {
                // 特定の例外ごとにハンドリングを行う場合は対応の型をcatchし行ってください
                Debug.LogError(error.Message);
                throw;
            }


            if (_pipeLine.IsFaceTracking)
            {
               SendDetectEvent();
                _currentBuffer = 0;
            }
            else
            {
                _currentBuffer += Time.deltaTime;
            }

            if(_currentBuffer >= _lostBuffer)
            {
                SendLostEvent();
            }
        }

        private class IdleState : MyState
        {
            protected internal override void Enter()
            {
                Debug.Log("Idle");
                stateMachine.Context._faceSwap.SetDraw(false);
            }

            protected internal override void Update()
            {

            }

            protected internal override void Exit()
            {

            }
        }



        private class DetectingState : MyState
        {
            float duration;
            protected internal override void Enter()
            {
                Debug.Log("Detecting");
                duration = 1.0f;
            }

            protected internal override void Update()
            {
                //認識したら一定時間カウントしてから次の状態に遷移する
                duration -= Time.deltaTime;
                if(duration <= 0)
                {
                    stateMachine.SendEvent(StateEvent.Capture);
                }

            }

            protected internal override void Exit()
            {

            }
        }


        private class CapturingState : MyState
        {
            protected internal override void Enter()
            {
                Debug.Log("Capturing");
                //テクスチャをキャプチャ
                stateMachine.Context._faceSwap.SaveTextureRandom();
                stateMachine.SendEvent(StateEvent.Captured);
            }

            protected internal override void Update()
            {

            }

            protected internal override void Exit()
            {

            }
        }


        private class CapturedState : MyState
        {
            float duration;
            protected internal override void Enter()
            {
                Debug.Log("Captured");
                duration = 1.0f;
            }

            protected internal override void Update()
            {
                //一定時間たったらスワップに遷移する
                duration -= Time.deltaTime;
                if (duration <= 0)
                {
                    stateMachine.SendEvent(StateEvent.Swap);
                }
                
            }

            protected internal override void Exit()
            {

            }
        }


        private class SwappingState : MyState
        {
            protected internal override void Enter()
            {
                Debug.Log("Swapping");
                //フェイススワップ実行
                stateMachine.Context._faceSwap.SetDraw(true);
                stateMachine.Context._faceSwap.SwapTextureParallel(3);
                stateMachine.SendEvent(StateEvent.Swaped);
            }

            protected internal override void Update()
            {

            }

            protected internal override void Exit()
            {

            }
        }

        private class SwappedState : MyState
        {
            float duration;
            protected internal override void Enter()
            {
                Debug.Log("Swapped");
                duration = 10.0f;
            }

            protected internal override void Update()
            {
                //一定時間たったらまたスワップする
                duration -= Time.deltaTime;
                if (duration <= 0)
                {
                    stateMachine.SendEvent(StateEvent.Swap);
                }
            }

            protected internal override void Exit()
            {

            }
        }

        public void SendDetectEvent()
        {
            stateMachine.SendEvent(StateEvent.Detect);
           // Debug.Log("Send Detect Event");
        }

        public void SendLostEvent()
        {
            stateMachine.SendEvent(StateEvent.Lost);
           // Debug.Log("Send Lost Event");
        }
    }
}