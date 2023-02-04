
using System;

using System.Collections.Generic;

using System.Diagnostics;

using System.Windows.Input;


// http://until1.cafe24.com/meBoard.aspx/ContView?tbName=WPF&Idx=325

namespace Radeon_DX_Configurator.ViewModel.Command

{

    public class DelegateCommand<T> : ICommand

    {

        #region Member Fields

        /// <summary>

        /// 해당 View가 제거 된 후 ViewModel이 더 이상 사용 되지 않은 경우에도 CanExecute Command가 항상 발생 되는 문제가 있어 <para />

        /// CanExecute EventHandler를 별도 List로 관리하여 처리 한다.

        /// </summary>

        private List<WeakReference> _canExecuteChangedHandlers;

        private bool _isAutomaticRequeryDisabled = false;

        private readonly Predicate<T> _canExecute;

        private readonly Action<T> _execute;

        #endregion



        #region Construct

        public DelegateCommand(Action<T> execute)

            : this(execute, null)

        {

        }



        public DelegateCommand(Action<T> execute, Predicate<T> canExecute)

            : this(execute, canExecute, false)

        {

        }



        public DelegateCommand(Action<T> execute, Predicate<T> canExecute, bool isAutomaticRequeryDisabled)

        {

            if (execute == null)

            {

                throw new ArgumentNullException("executeMethod");

            }


            _execute = execute;

            _canExecute = canExecute;

            _isAutomaticRequeryDisabled = isAutomaticRequeryDisabled;

        }

        #endregion


        /// <summary>

        /// Property to enable or disable CommandManager's automatic requery on this command

        /// </summary>

        public bool IsAutomaticRequeryDisabled

        {

            get

            {

                return _isAutomaticRequeryDisabled;

            }

            set

            {

                if (_isAutomaticRequeryDisabled != value)

                {

                    if (value)

                    {

                        CommandManagerHelper.RemoveHandlersFromRequerySuggested(_canExecuteChangedHandlers);

                    }

                    else

                    {

                        CommandManagerHelper.AddHandlersToRequerySuggested(_canExecuteChangedHandlers);

                    }



                    _isAutomaticRequeryDisabled = value;

                }

            }

        }



        #region ICommand 구현

        public event EventHandler CanExecuteChanged

        {

            add

            {

                if (!_isAutomaticRequeryDisabled)

                {

                    CommandManager.RequerySuggested += value;

                }

                CommandManagerHelper.AddWeakReferenceHandler(ref _canExecuteChangedHandlers, value, -1);

            }

            remove

            {

                if (!_isAutomaticRequeryDisabled)

                {

                    CommandManager.RequerySuggested -= value;

                }


                CommandManagerHelper.RemoveWeakReferenceHandler(_canExecuteChangedHandlers, value);

            }

        }



        public bool CanExecute(object parameter)

        {

            if (_canExecute == null)

                return true;



            return _canExecute((parameter == null) ?

                default(T) : (T)Convert.ChangeType(parameter, typeof(T)));

        }


        public void Execute(object parameter)

        {

            _execute((parameter == null) ? default(T) : (T)Convert.ChangeType(parameter, typeof(T)));

        }

        #endregion


        /// <summary>

        /// Raises the CanExecuteChaged event

        /// </summary>

        public void RaiseCanExecuteChanged()

        {

            OnCanExecuteChanged();

        }

        protected virtual void OnCanExecuteChanged()

        {

            CommandManagerHelper.CallWeakReferenceHandlers(_canExecuteChangedHandlers);

        }

    }



    /// <summary>

    /// View가 소멸되고 해당 ViewModel이 사용되지 않는 Command가 메모리에 계속 상주해 있는 문제를 해결하는 클래스

    /// Command EventHandler를 약한 참조를 사용하여 연결한다.

    /// https://github.com/crosbymichael/mvvm-async/blob/master/MVVM-Async/Commands/DelegateCommand.cs#L44

    /// </summary>

    internal class CommandManagerHelper

    {

        internal static void CallWeakReferenceHandlers(List<WeakReference> handlers)

        {

            if (handlers != null)

            {

                // Take a snapshot of the handlers before we call out to them since the handlers

                // could cause the array to me modified while we are reading it.

                EventHandler[] callees = new EventHandler[handlers.Count];

                int count = 0;

                for (int i = handlers.Count - 1; i >= 0; i--)

                {

                    WeakReference reference = handlers[i];

                    EventHandler handler = reference.Target as EventHandler;

                    if (handler == null)

                    {

                        // Clean up old handlers that have been collected

                        handlers.RemoveAt(i);

                    }

                    else

                    {

                        callees[count] = handler;

                        count++;

                    }

                }



                // Call the handlers that we snapshotted

                for (int i = 0; i < count; i++)

                {

                    EventHandler handler = callees[i];

                    handler(null, EventArgs.Empty);

                }

            }

        }



        internal static void AddHandlersToRequerySuggested(List<WeakReference> handlers)

        {

            if (handlers != null)

            {

                foreach (WeakReference handlerRef in handlers)

                {

                    EventHandler handler = handlerRef.Target as EventHandler;

                    if (handler != null)

                    {

                        CommandManager.RequerySuggested += handler;

                    }

                }

            }

        }



        internal static void RemoveHandlersFromRequerySuggested(List<WeakReference> handlers)

        {

            if (handlers != null)

            {

                foreach (WeakReference handlerRef in handlers)

                {

                    EventHandler handler = handlerRef.Target as EventHandler;



                    if (handler != null)

                    {

                        CommandManager.RequerySuggested -= handler;

                    }

                }

            }

        }



        internal static void AddWeakReferenceHandler(ref List<WeakReference> handlers, EventHandler handler)

        {

            AddWeakReferenceHandler(ref handlers, handler, -1);

        }



        internal static void AddWeakReferenceHandler(ref List<WeakReference> handlers, EventHandler handler, int defaultListSize)

        {

            if (handlers == null)

            {

                handlers = (defaultListSize > 0 ? new List<WeakReference>(defaultListSize) : new List<WeakReference>());

            }



            handlers.Add(new WeakReference(handler));

        }



        internal static void RemoveWeakReferenceHandler(List<WeakReference> handlers, EventHandler handler)

        {

            if (handlers != null)

            {

                for (int i = handlers.Count - 1; i >= 0; i--)

                {

                    WeakReference reference = handlers[i];

                    EventHandler existingHandler = reference.Target as EventHandler;


                    if ((existingHandler == null) || (existingHandler == handler))

                    {

                        // Clean up old handlers that have been collected

                        // in addition to the handler that is to be removed.

                        handlers.RemoveAt(i);

                    }

                }

            }

        }

    }

}