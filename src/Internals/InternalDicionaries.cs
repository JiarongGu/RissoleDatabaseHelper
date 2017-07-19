using RissoleDatabaseHelper.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RissoleDatabaseHelper.Internals
{
    internal class InternalDicionaries
    {
        private static InternalDicionaries _internalDicionaries;

        private Dictionary<Type, RissoleTable> _tableDictionary;
            
        private InternalDicionaries()
        {
            _tableDictionary = new Dictionary<Type, RissoleTable>();
        }
        
        public InternalDicionaries Instance {
            get {
                if (_internalDicionaries == null)
                {
                    _internalDicionaries = new InternalDicionaries();
                }

                return _internalDicionaries;
            }
        }
    }
}
