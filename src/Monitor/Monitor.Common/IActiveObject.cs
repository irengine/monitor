using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monitor.Common
{
    public interface IActiveObject
    {
        /// <summary>
        /// Initialize an active object
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        void Initialize(string name, Action action);

        /// <summary>
        /// Signal the active object to perform its loop action.
        /// </summary>
        /// <remarks>
        /// Application may call this after some simple or complex condition evaluation
        /// </remarks>
        void Signal();

        /// <summary>
        /// Signals to shotdown this active object
        /// </summary>
        void Shutdown();

        void Start();
        void Stop();
    }
}
