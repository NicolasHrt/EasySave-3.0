using System;
using System.Collections.Generic;

namespace EventListner
{
    /// <summary>
    /// Class use by the event manager, useful for inheritance
    /// </summary>
    public class IEventListner
    {
        /// <summary>
        /// Methode Update call by the notify method from the event manager
        /// </summary>
        /// <param name="type"></param>
        /// <param name="listUpdate"></param>
        public virtual void Update(string type, Dictionary<string, string> listUpdate) { }
    }
}
