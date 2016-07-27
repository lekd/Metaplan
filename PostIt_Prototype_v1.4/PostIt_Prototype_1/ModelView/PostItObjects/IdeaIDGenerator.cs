using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PostIt_Prototype_1.PostItObjects
{
    public class IdeaIdGenerator
    {
        static int _saltVal = 0;
        static int _localId = 1;
        static int Hash(int a, int b)
        {
            return ((a + b) * (a + b + 1) / 2 + b);
        }
        static int GetHashedId(int inputId)
        {
            if (_saltVal == 0)
            {
                var rnd = new Random();
                _saltVal = rnd.Next(1, short.MaxValue / 2);
            }
            return Hash(_saltVal, inputId);
        }
        public static int GenerateId()
        {
            var globalId = GetHashedId(_localId);
            _localId++;
            return globalId;
        }

    }
}
