using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Common.Logging;

namespace Monitor.Common
{
    /// <summary>
    /// Implements a simple active object pattern implementation
    /// </summary>
    /// <remarks>
    /// Although there exists a vast number of active objects patterns (in Java they are just "runnable")
    /// scattered, one of the best I found is located at http://blog.gurock.com/wp-content/uploads/2008/01/activeobjects.pdf
    /// </remarks>
    public class SignalActiveObject : IActiveObject
    {
        private static ILog logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Name of this active object
        /// </summary>
        private string m_Name;

        /// <summary>
        /// Underlying active thread
        /// </summary>
        private Thread m_ActiveThreadContext;

        /// <summary>
        /// Abstracted action that the active thread executes
        /// </summary>
        private Action m_ActiveAction;

        /// <summary>
        /// Primary signal object for this active thread.
        /// See the Signal() method for more.
        /// </summary>
        private AutoResetEvent m_SignalObject;

        /// <summary>
        /// Signal object for shutting down this active object
        /// </summary>
        private ManualResetEvent m_ShutdownEvent;

        /// <summary>
        /// Interal array of signal objects combining primary signal object and 
        /// shutdown signal object
        /// </summary>
        private WaitHandle[] m_SignalObjects;

        public SignalActiveObject()
        {
        }

        public void Initialize(string name, Action action)
        {
            m_Name = name;
            m_ActiveAction = action;
            m_SignalObject = new AutoResetEvent(false);
            m_ShutdownEvent = new ManualResetEvent(false);
            m_SignalObjects = new WaitHandle[]
                                {
                                    m_ShutdownEvent,
                                    m_SignalObject
                                };

            m_ActiveThreadContext = new Thread(Run);
            m_ActiveThreadContext.Name = string.Concat("ActiveObject.", m_Name);
            m_ActiveThreadContext.Start();
        }

        private bool Guard()
        {
            int index = WaitHandle.WaitAny(m_SignalObjects);
            return index == 0 ? false : true;
        }

        /// <summary>
        /// Signal the active object to perform its loop action.
        /// </summary>
        /// <remarks>
        /// Application may call this after some simple of complex condition evaluation
        /// </remarks>
        public void Signal()
        {
            m_SignalObject.Set();
        }

        public virtual void Start() { }
        public virtual void Stop() { }

        /// <summary>
        /// Signals to shutdown this active object
        /// </summary>
        public void Shutdown()
        {
            m_ShutdownEvent.Set();

            if (m_ActiveThreadContext != null)
            {
                m_ActiveThreadContext.Join();
            }

            m_ActiveThreadContext = null;
        }

        /// <summary>
        /// Core run method of this active thread
        /// </summary>
        private void Run()
        {
            try
            {
                logger.Debug(m => m("{0} started.", m_Name));
                while (Guard())
                {
                    try
                    {
                        m_ActiveAction();
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }
                }
                logger.Debug(m => m("{0} stopped.", m_Name));
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            finally
            {
                m_SignalObject.Close();
                m_ShutdownEvent.Close();

                m_SignalObject = null;
                m_ShutdownEvent = null;
            }
        }
    }
}
