// zlib/libpng License
//
// Copyright (c) 2018 Sinoa
//
// This software is provided 'as-is', without any express or implied warranty.
// In no event will the authors be held liable for any damages arising from the use of this software.
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it freely,
// subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software.
//    If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.

using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
#if ENABLE_IL2CPP
using UnityEngine.Scripting;
#endif

namespace IceMilkTea.Core
{
    /// <summary>
    /// ステートマシンの更新処理中に発生した、未処理の例外をどう振る舞うかを表現した列挙型です
    /// </summary>
    public enum ImtStateMachineUnhandledExceptionMode
    {
        /// <summary>
        /// Update関数内で発生した例外をそのまま例外として発生させます。
        /// </summary>
        ThrowException,

        /// <summary>
        /// OnUnhandledException ハンドラに転送されます。
        /// </summary>
        CatchException,

        /// <summary>
        /// 現在動作中ステートの Error() に例外が転送されます。
        /// ただし、現在動作中ステートが存在しない場合は ThrowException と同等の振る舞いになります。
        /// </summary>
        CatchStateException,
    }



    #region 標準ステートマシン基底実装
    /// <summary>
    /// コンテキストを持つことのできるステートマシンクラスです
    /// </summary>
    /// <typeparam name="TContext">このステートマシンが持つコンテキストの型</typeparam>
    /// <typeparam name="TEvent">ステートマシンへ送信するイベントの型</typeparam>
    public class ImtStateMachine<TContext, TEvent>
    {
        #region ステートクラス本体と特別ステートクラスの定義
        /// <summary>
        /// ステートマシンが処理する状態を表現するステートクラスです。
        /// </summary>
        public abstract class State
        {
            // メンバ変数定義
            internal Dictionary<TEvent, State> transitionTable;
            internal ImtStateMachine<TContext, TEvent> stateMachine;



            /// <summary>
            /// このステートが所属するステートマシン
            /// </summary>
            protected ImtStateMachine<TContext, TEvent> StateMachine => stateMachine;


            /// <summary>
            /// このステートが所属するステートマシンが持っているコンテキスト
            /// </summary>
            protected TContext Context => stateMachine.Context;



            /// <summary>
            /// ステートに突入したときの処理を行います
            /// </summary>
            protected internal virtual void Enter()
            {
            }


            /// <summary>
            /// ステートを更新するときの処理を行います
            /// </summary>
            protected internal virtual void Update()
            {
            }


            /// <summary>
            /// ステートから脱出したときの処理を行います
            /// </summary>
            protected internal virtual void Exit()
            {
            }


            /// <summary>
            /// ステートマシンの未処理例外が発生した時の処理を行います。
            /// ただし UnhandledExceptionMode が CatchStateException である必要があります。
            /// </summary>
            /// <remarks>
            /// もし、この関数が false を返した場合は、例外が結局未処理状態と判断されステートマシンの
            /// Update() 関数が例外を送出することになります。
            /// </remarks>
            /// <param name="exception">発生した未処理の例外</param>
            /// <returns>例外を処理した場合は true を、未処理の場合は false を返します</returns>
            protected internal virtual bool Error(Exception exception)
            {
                // 通常は未処理として返す
                return false;
            }


            /// <summary>
            /// ステートマシンがイベントを受ける時に、このステートがそのイベントをガードします
            /// </summary>
            /// <param name="eventId">渡されたイベントID</param>
            /// <returns>イベントの受付をガードする場合は true を、ガードせずイベントを受け付ける場合は false を返します</returns>
            protected internal virtual bool GuardEvent(TEvent eventId)
            {
                // 通常はガードしない
                return false;
            }


            /// <summary>
            /// ステートマシンがスタックしたステートをポップする前に、このステートがそのポップをガードします
            /// </summary>
            /// <returns>ポップの動作をガードする場合は true を、ガードせずにポップ動作を続ける場合は false を返します</returns>
            protected internal virtual bool GuardPop()
            {
                // 通常はガードしない
                return false;
            }
        }



        /// <summary>
        /// ステートマシンで "任意" を表現する特別なステートクラスです
        /// </summary>
#if ENABLE_IL2CPP
        [Preserve]
#endif
        public sealed class AnyState : State { }
        #endregion



        #region 列挙型定義
        /// <summary>
        /// ステートマシンのUpdate状態を表現します
        /// </summary>
        private enum UpdateState
        {
            /// <summary>
            /// アイドリング中です。つまり何もしていません
            /// </summary>
            Idle,

            /// <summary>
            /// ステートの突入処理中です
            /// </summary>
            Enter,

            /// <summary>
            /// ステートの更新処理中です
            /// </summary>
            Update,

            /// <summary>
            /// ステートの脱出処理中です
            /// </summary>
            Exit,
        }
        #endregion



        // メンバ変数定義
        private UpdateState updateState;
        private List<State> stateList;
        private State currentState;
        private State nextState;
        private Stack<State> stateStack;
        private HashSet<Func<Type, State>> stateFactorySet;



        /// <summary>
        /// ステートマシンの Update() 中に未処理の例外が発生した時のイベントハンドラです。
        /// ただし UnhandledExceptionMode プロパティに CatchException が設定されている必要があります。
        /// false が返されると、例外が未処理と判断され Update() 関数が例外を送出します。
        /// </summary>
        public event Func<Exception, bool> UnhandledException;



        /// <summary>
        /// ステートマシンが保持しているコンテキスト
        /// </summary>
        public TContext Context { get; private set; }


        /// <summary>
        /// ステートマシンが起動しているかどうか
        /// </summary>
        public bool Running => currentState != null;


        /// <summary>
        /// ステートマシンが、更新処理中かどうか。
        /// Update 関数から抜けたと思っても、このプロパティが true を示す場合、
        /// Update 中に例外などで不正な終了の仕方をしている場合が考えられます。
        /// </summary>
        public bool Updating => (Running && updateState != UpdateState.Idle);


        /// <summary>
        /// 現在のスタックしているステートの数
        /// </summary>
        public int StackCount => stateStack.Count;


        /// <summary>
        /// 現在のステートの名前を取得します。
        /// まだステートマシンが起動していない場合は空文字列になります。
        /// </summary>
        public string CurrentStateName => (Running ? currentState.GetType().Name : string.Empty);


        /// <summary>
        /// SendEvent() 関数によって一度、遷移状態になった後に再び SendEvent() による遷移し直しを許可するかどうか
        /// </summary>
        public bool AllowRetransition { get; set; }


        /// <summary>
        /// 未処理の例外が発生した際の振る舞いの設定取得をします
        /// </summary>
        public ImtStateMachineUnhandledExceptionMode UnhandledExceptionMode { get; set; }


        /// <summary>
        /// このステートマシンを最後にUpdateしたスレッドID
        /// </summary>
        public int LastUpdateThreadId { get; private set; }


        /// <summary>
        /// このステートマシンが最後に受け付けたイベントID
        /// </summary>
        public TEvent LastAcceptedEventID { get; private set; }



        /// <summary>
        /// ImtStateMachine のインスタンスを初期化します
        /// </summary>
        /// <param name="context">このステートマシンが持つコンテキスト</param>
        /// <exception cref="ArgumentNullException">context が null です</exception>
        /// <exception cref="InvalidOperationException">ステートクラスのインスタンスの生成に失敗しました</exception>
        public ImtStateMachine(TContext context)
        {
            // 渡されたコンテキストがnullなら
            if (context == null)
            {
                // nullは許されない
                throw new ArgumentNullException(nameof(context));
            }


            // メンバの初期化をする
            Context = context;
            stateList = new List<State>();
            stateStack = new Stack<State>();
            updateState = UpdateState.Idle;
            AllowRetransition = false;
            UnhandledExceptionMode = ImtStateMachineUnhandledExceptionMode.ThrowException;
            stateFactorySet = new HashSet<Func<Type, State>>();
        }


        #region 汎用ロジック系
        /// <summary>
        /// 型からステートインスタンスを生成するファクトリ関数を登録します
        /// </summary>
        /// <param name="stateFactory">登録するファクトリ関数</param>
        /// <exception cref="ArgumentNullException">stateFactory が null です</exception>
        public void RegisterStateFactory(Func<Type, State> stateFactory)
        {
            // ハッシュセットに登録する
            stateFactorySet.Add(stateFactory ?? throw new ArgumentNullException(nameof(stateFactory)));
        }


        /// <summary>
        /// 登録したファクトリ関数の解除をします
        /// </summary>
        /// <param name="stateFactory">解除するファクトリ関数</param>
        /// <exception cref="ArgumentNullException">stateFactory が null です</exception>
        public void UnregisterStateFactory(Func<Type, State> stateFactory)
        {
            // ハッシュセットから登録を解除する
            stateFactorySet.Remove(stateFactory ?? throw new ArgumentNullException(nameof(stateFactory)));
        }
        #endregion


        #region ステート遷移テーブル構築系
        /// <summary>
        /// ステートの任意遷移構造を追加します。
        /// </summary>
        /// <remarks>
        /// この関数は、遷移元が任意の状態からの遷移を希望する場合に利用してください。
        /// 任意の遷移は、通常の遷移（Any以外の遷移元）より優先度が低いことにも、注意をしてください。
        /// また、ステートの遷移テーブル設定はステートマシンが起動する前に完了しなければなりません。
        /// </remarks>
        /// <typeparam name="TNextState">任意状態から遷移する先になるステートの型</typeparam>
        /// <param name="eventId">遷移する条件となるイベントID</param>
        /// <exception cref="ArgumentException">既に同じ eventId が設定された遷移先ステートが存在します</exception>
        /// <exception cref="InvalidOperationException">ステートマシンは、既に起動中です</exception>
        public void AddAnyTransition<TNextState>(TEvent eventId) where TNextState : State, new()
        {
            // 単純に遷移元がAnyStateなだけの単純な遷移追加関数を呼ぶ
            AddTransition<AnyState, TNextState>(eventId);
        }


        /// <summary>
        /// ステートの遷移構造を追加します。
        /// また、ステートの遷移テーブル設定はステートマシンが起動する前に完了しなければなりません。
        /// </summary>
        /// <typeparam name="TPrevState">遷移する元になるステートの型</typeparam>
        /// <typeparam name="TNextState">遷移する先になるステートの型</typeparam>
        /// <param name="eventId">遷移する条件となるイベントID</param>
        /// <exception cref="ArgumentException">既に同じ eventId が設定された遷移先ステートが存在します</exception>
        /// <exception cref="InvalidOperationException">ステートマシンは、既に起動中です</exception>
        /// <exception cref="InvalidOperationException">ステートクラスのインスタンスの生成に失敗しました</exception>
        public void AddTransition<TPrevState, TNextState>(TEvent eventId) where TPrevState : State, new() where TNextState : State, new()
        {
            // ステートマシンが起動してしまっている場合は
            if (Running)
            {
                // もう設定できないので例外を吐く
                throw new InvalidOperationException("ステートマシンは、既に起動中です");
            }


            // 遷移元と遷移先のステートインスタンスを取得
            var prevState = GetOrCreateState<TPrevState>();
            var nextState = GetOrCreateState<TNextState>();


            // 遷移元ステートの遷移テーブルに既に同じイベントIDが存在していたら
            if (prevState.transitionTable.ContainsKey(eventId))
            {
                // 上書き登録を許さないので例外を吐く
                throw new ArgumentException($"ステート'{prevState.GetType().Name}'には、既にイベントID'{eventId}'の遷移が設定済みです");
            }


            // 遷移テーブルに遷移を設定する
            prevState.transitionTable[eventId] = nextState;
        }


        /// <summary>
        /// ステートマシンが起動する時に、最初に開始するステートを設定します。
        /// </summary>
        /// <typeparam name="TStartState">ステートマシンが起動時に開始するステートの型</typeparam>
        /// <exception cref="InvalidOperationException">ステートマシンは、既に起動中です</exception>
        /// <exception cref="InvalidOperationException">ステートクラスのインスタンスの生成に失敗しました</exception>
        public void SetStartState<TStartState>() where TStartState : State, new()
        {
            // 既にステートマシンが起動してしまっている場合は
            if (Running)
            {
                // 起動してしまったらこの関数の操作は許されない
                throw new InvalidOperationException("ステートマシンは、既に起動中です");
            }


            // 次に処理するステートの設定をする
            nextState = GetOrCreateState<TStartState>();
        }
        #endregion


        #region ステートスタック操作系
        /// <summary>
        /// 現在実行中のステートを、ステートスタックにプッシュします
        /// </summary>
        /// <exception cref="InvalidOperationException">ステートマシンは、まだ起動していません</exception>
        public void PushState()
        {
            // そもそもまだ現在実行中のステートが存在していないなら例外を投げる
            IfNotRunningThrowException();


            // 現在のステートをスタックに積む
            stateStack.Push(currentState);
        }


        /// <summary>
        /// ステートスタックに積まれているステートを取り出し、遷移の準備を行います。
        /// </summary>
        /// <remarks>
        /// この関数の挙動は、イベントIDを送ることのない点を除けば SendEvent 関数と非常に似ています。
        /// 既に SendEvent によって次の遷移の準備ができている場合は、スタックからステートはポップされることはありません。
        /// </remarks>
        /// <returns>スタックからステートがポップされ次の遷移の準備が完了した場合は true を、ポップするステートがなかったり、ステートによりポップがガードされた場合は false を返します</returns>
        /// <exception cref="InvalidOperationException">ステートマシンは、まだ起動していません</exception>
        public virtual bool PopState()
        {
            // そもそもまだ現在実行中のステートが存在していないなら例外を投げる
            IfNotRunningThrowException();


            // そもそもスタックが空であるか、次に遷移するステートが存在 かつ 再遷移が未許可か、ポップする前に現在のステートにガードされたのなら
            if (stateStack.Count == 0 || (nextState != null && !AllowRetransition) || currentState.GuardPop())
            {
                // ポップ自体出来ないのでfalseを返す
                return false;
            }


            // ステートをスタックから取り出して次のステートへ遷移するようにして成功を返す
            nextState = stateStack.Pop();
            return true;
        }


        /// <summary>
        /// ステートスタックに積まれているステートを取り出し、現在のステートとして直ちに直接設定します。
        /// </summary>
        /// <remarks>
        /// この関数の挙動は PopState() 関数と違い、ポップされたステートがそのまま現在処理中のステートとして直ちに設定するため、
        /// 状態の遷移処理は行われず、ポップされたステートの Enter() は呼び出されずそのまま次回から Update() が呼び出されるようになります。
        /// </remarks>
        /// <returns>スタックからステートがポップされ、現在のステートとして設定出来た場合は true を、ポップするステートが無いか、ポップがガードされた場合は false を返します</returns>
        /// <exception cref="InvalidOperationException">ステートマシンは、まだ起動していません</exception>
        public virtual bool PopAndDirectSetState()
        {
            // そもそもまだ現在実行中のステートが存在していないなら例外を投げる
            IfNotRunningThrowException();


            // そもそもスタックが空であるか、ポップする前に現在のステートにガードされたのなら
            if (stateStack.Count == 0 || currentState.GuardPop())
            {
                // ポップ自体出来ないのでfalseを返す
                return false;
            }


            // ステートをスタックから取り出して現在のステートとして設定して成功を返す
            currentState = stateStack.Pop();
            return true;
        }


        /// <summary>
        /// ステートスタックに積まれているステートを一つ取り出し、そのまま捨てます。
        /// </summary>
        /// <remarks>
        /// ステートスタックの一番上に積まれているステートをそのまま捨てたい時に利用します。
        /// </remarks>
        public void PopAndDropState()
        {
            // スタックが空なら
            if (stateStack.Count == 0)
            {
                // 何もせず終了
                return;
            }


            // スタックからステートを取り出して何もせずそのまま捨てる
            stateStack.Pop();
        }


        /// <summary>
        /// ステートスタックに積まれているすべてのステートを捨てます。
        /// </summary>
        public void ClearStack()
        {
            // スタックを空にする
            stateStack.Clear();
        }
        #endregion


        #region ステートマシン制御系
        /// <summary>
        /// 現在実行中のステートが、指定されたステートかどうかを調べます。
        /// </summary>
        /// <typeparam name="TState">確認するステートの型</typeparam>
        /// <returns>指定されたステートの状態であれば true を、異なる場合は false を返します</returns>
        /// <exception cref="InvalidOperationException">ステートマシンは、まだ起動していません</exception>
        public bool IsCurrentState<TState>() where TState : State
        {
            // そもそもまだ現在実行中のステートが存在していないなら例外を投げる
            IfNotRunningThrowException();


            // 現在のステートと型が一致するかの条件式の結果をそのまま返す
            return currentState.GetType() == typeof(TState);
        }


        /// <summary>
        /// ステートマシンにイベントを送信して、ステート遷移の準備を行います。
        /// </summary>
        /// <remarks>
        /// ステートの遷移は直ちに行われず、次の Update が実行された時に遷移処理が行われます。
        /// また、この関数によるイベント受付優先順位は、一番最初に遷移を受け入れたイベントのみであり Update によって遷移されるまで、後続のイベントはすべて失敗します。
        /// ただし AllowRetransition プロパティに true が設定されている場合は、再遷移が許されます。
        /// さらに、イベントはステートの Enter または Update 処理中でも受け付けることが可能で、ステートマシンの Update 中に
        /// 何度も遷移をすることが可能ですが Exit 中でイベントを送ると、遷移中になるため例外が送出されます。
        /// </remarks>
        /// <param name="eventId">ステートマシンに送信するイベントID</param>
        /// <returns>ステートマシンが送信されたイベントを受け付けた場合は true を、イベントを拒否または、イベントの受付ができない場合は false を返します</returns>
        /// <exception cref="InvalidOperationException">ステートマシンは、まだ起動していません</exception>
        /// <exception cref="InvalidOperationException">ステートが Exit 処理中のためイベントを受け付けることが出来ません</exception>
        public virtual bool SendEvent(TEvent eventId)
        {
            // そもそもまだ現在実行中のステートが存在していないなら例外を投げる
            IfNotRunningThrowException();


            // もし Exit 処理中なら
            if (updateState == UpdateState.Exit)
            {
                // Exit 中の SendEvent は許されない
                throw new InvalidOperationException("ステートが Exit 処理中のためイベントを受け付けることが出来ません");
            }


            // 既に遷移準備をしていて かつ 再遷移が許可されていないなら
            if (nextState != null && !AllowRetransition)
            {
                // イベントの受付が出来なかったことを返す
                return false;
            }


            // 現在のステートにイベントガードを呼び出して、ガードされたら
            if (currentState.GuardEvent(eventId))
            {
                // ガードされて失敗したことを返す
                return false;
            }


            // 次に遷移するステートを現在のステートから取り出すが見つけられなかったら
            if (!currentState.transitionTable.TryGetValue(eventId, out nextState))
            {
                // 任意ステートからすらも遷移が出来なかったのなら
                if (!GetOrCreateState<AnyState>().transitionTable.TryGetValue(eventId, out nextState))
                {
                    // イベントの受付が出来なかった
                    return false;
                }
            }


            // 最後に受け付けたイベントIDを覚えてイベントの受付をした事を返す
            LastAcceptedEventID = eventId;
            return true;
        }


        /// <summary>
        /// ステートマシンの状態を更新します。
        /// </summary>
        /// <remarks>
        /// ステートマシンの現在処理しているステートの更新を行いますが、まだ未起動の場合は SetStartState 関数によって設定されたステートが起動します。
        /// また、ステートマシンが初回起動時の場合、ステートのUpdateは呼び出されず、次の更新処理が実行される時になります。
        /// </remarks>
        /// <exception cref="InvalidOperationException">現在のステートマシンは、別のスレッドによって更新処理を実行しています。[UpdaterThread={LastUpdateThreadId}, CurrentThread={currentThreadId}]</exception>
        /// <exception cref="InvalidOperationException">現在のステートマシンは、既に更新処理を実行しています</exception>
        /// <exception cref="InvalidOperationException">開始ステートが設定されていないため、ステートマシンの起動が出来ません</exception>
        public virtual void Update()
        {
            // もしステートマシンの更新状態がアイドリング以外だったら
            if (updateState != UpdateState.Idle)
            {
                // もし別スレッドからのUpdateによる多重Updateなら
                int currentThreadId = Thread.CurrentThread.ManagedThreadId;
                if (LastUpdateThreadId != currentThreadId)
                {
                    // 別スレッドからの多重Updateであることを例外で吐く
                    throw new InvalidOperationException($"現在のステートマシンは、別のスレッドによって更新処理を実行しています。[UpdaterThread={LastUpdateThreadId}, CurrentThread={currentThreadId}]");
                }


                // 多重でUpdateが呼び出せない例外を吐く
                throw new InvalidOperationException("現在のステートマシンは、既に更新処理を実行しています");
            }


            // Updateの起動スレッドIDを覚える
            LastUpdateThreadId = Thread.CurrentThread.ManagedThreadId;


            // まだ未起動なら
            if (!Running)
            {
                // 次に処理するべきステート（つまり起動開始ステート）が未設定なら
                if (nextState == null)
                {
                    // 起動が出来ない例外を吐く
                    throw new InvalidOperationException("開始ステートが設定されていないため、ステートマシンの起動が出来ません");
                }


                // 現在処理中ステートとして設定する
                currentState = nextState;
                nextState = null;


                try
                {
                    // Enter処理中であることを設定してEnterを呼ぶ
                    updateState = UpdateState.Enter;
                    currentState.Enter();
                }
                catch (Exception exception)
                {
                    // 起動時の復帰は現在のステートにnullが入っていないとまずいので遷移前の状態に戻す
                    nextState = currentState;
                    currentState = null;


                    // 更新状態をアイドリングにして、例外発生時のエラーハンドリングを行い終了する
                    updateState = UpdateState.Idle;
                    DoHandleException(exception);
                    return;
                }


                // 次に遷移するステートが無いなら
                if (nextState == null)
                {
                    // 起動処理は終わったので一旦終わる
                    updateState = UpdateState.Idle;
                    return;
                }
            }


            try
            {
                // 次に遷移するステートが存在していないなら
                if (nextState == null)
                {
                    // Update処理中であることを設定してUpdateを呼ぶ
                    updateState = UpdateState.Update;
                    currentState.Update();
                }


                // 次に遷移するステートが存在している間ループ
                while (nextState != null)
                {
                    // Exit処理中であることを設定してExit処理を呼ぶ
                    updateState = UpdateState.Exit;
                    currentState.Exit();


                    // 次のステートに切り替える
                    currentState = nextState;
                    nextState = null;


                    // Enter処理中であることを設定してEnterを呼ぶ
                    updateState = UpdateState.Enter;
                    currentState.Enter();
                }


                // 更新処理が終わったらアイドリングに戻る
                updateState = UpdateState.Idle;
            }
            catch (Exception exception)
            {
                // 更新状態をアイドリングにして、例外発生時のエラーハンドリングを行い終了する
                updateState = UpdateState.Idle;
                DoHandleException(exception);
                return;
            }
        }
        #endregion


        #region 内部ロジック系
        /// <summary>
        /// 発生した未処理の例外をハンドリングします
        /// </summary>
        /// <param name="exception">発生した未処理の例外</param>
        /// <exception cref="ArgumentNullException">exception が null です</exception>
        private void DoHandleException(Exception exception)
        {
            // nullを渡されたら
            if (exception == null)
            {
                // 何をハンドリングすればよいのか
                throw new ArgumentNullException(nameof(exception));
            }


            // もし、例外を拾うモード かつ ハンドラが設定されているなら
            if (UnhandledExceptionMode == ImtStateMachineUnhandledExceptionMode.CatchException && UnhandledException != null)
            {
                // イベントを呼び出して、正しくハンドリングされたのなら
                if (UnhandledException(exception))
                {
                    // そのまま終了
                    return;
                }
            }


            // もし、例外を拾ってステートに任せるモード かつ 現在の実行ステートが設定されているのなら
            if (UnhandledExceptionMode == ImtStateMachineUnhandledExceptionMode.CatchStateException && currentState != null)
            {
                // ステートに例外を投げて、正しくハンドリングされたのなら
                if (currentState.Error(exception))
                {
                    // そのまま終了
                    return;
                }
            }


            // 上記のモード以外（つまり ThrowException）か、例外がハンドリングされなかった（false を返された）のなら例外をキャプチャして発生させる
            ExceptionDispatchInfo.Capture(exception).Throw();
        }


        /// <summary>
        /// ステートマシンが未起動の場合に例外を送出します
        /// </summary>
        /// <exception cref="InvalidOperationException">ステートマシンは、まだ起動していません</exception>
        protected void IfNotRunningThrowException()
        {
            // そもそもまだ現在実行中のステートが存在していないなら
            if (!Running)
            {
                // まだ起動すらしていないので例外を吐く
                throw new InvalidOperationException("ステートマシンは、まだ起動していません");
            }
        }


        /// <summary>
        /// 指定されたステートの型のインスタンスを取得しますが、存在しない場合は生成してから取得します。
        /// 生成されたインスタンスは、次回から取得されるようになります。
        /// </summary>
        /// <typeparam name="TState">取得、または生成するステートの型</typeparam>
        /// <returns>取得、または生成されたステートのインスタンスを返します</returns>
        /// <exception cref="InvalidOperationException">ステートクラスのインスタンスの生成に失敗しました</exception>
        private TState GetOrCreateState<TState>() where TState : State, new()
        {
            // ステートの数分回る
            var stateType = typeof(TState);
            foreach (var state in stateList)
            {
                // もし該当のステートの型と一致するインスタンスなら
                if (state.GetType() == stateType)
                {
                    // そのインスタンスを返す
                    return (TState)state;
                }
            }


            // ループから抜けたのなら、型一致するインスタンスが無いという事なのでインスタンスを生成してキャッシュする
            var newState = CreateStateInstanceCore<TState>() ?? throw new InvalidOperationException("ステートクラスのインスタンスの生成に失敗しました");
            stateList.Add(newState);


            // 新しいステートに、自身の参照と遷移テーブルのインスタンスの初期化も行って返す
            newState.stateMachine = this;
            newState.transitionTable = new Dictionary<TEvent, State>();
            return newState;
        }


        /// <summary>
        /// 指定されたステートの型のインスタンスを生成します。
        /// </summary>
        /// <typeparam name="TState">生成するべきステータスの型</typeparam>
        /// <returns>生成したインスタンスを返します</returns>
        private TState CreateStateInstanceCore<TState>() where TState : State, new()
        {
            // 結果を受け取る変数を宣言
            TState result;


            // 登録されているファクトリ関数分回る
            var stateType = typeof(TState);
            foreach (var factory in stateFactorySet)
            {
                // 生成を試みてインスタンスが生成されたのなら
                result = (TState)factory(stateType);
                if (result != null)
                {
                    // このインスタンスを返す
                    return result;
                }
            }


            // ファクトリ関数でも駄目なら実装側生成関数に頼る
            return CreateStateInstance<TState>();
        }


        /// <summary>
        /// 指定されたステートの型のインスタンスを生成します。
        /// </summary>
        /// <typeparam name="TState">生成するべきステータスの型</typeparam>
        /// <returns>生成したインスタンスを返します</returns>
        protected virtual TState CreateStateInstance<TState>() where TState : State, new()
        {
            // 既定動作はジェネリックのnewをするのみで返す
            return new TState();
        }
        #endregion
    }
    #endregion



    #region 旧intイベント型ベースのステートマシン実装
    /// <summary>
    /// コンテキストを持つことのできるステートマシンクラスです
    /// </summary>
    /// <typeparam name="TContext">このステートマシンが持つコンテキストの型</typeparam>
    public class ImtStateMachine<TContext> : ImtStateMachine<TContext, int>
    {
        /// <summary>
        /// ImtStateMachine のインスタンスを初期化します
        /// </summary>
        /// <param name="context">このステートマシンが持つコンテキスト</param>
        /// <exception cref="ArgumentNullException">context が null です</exception>
        public ImtStateMachine(TContext context) : base(context)
        {
        }
    }
    #endregion
}