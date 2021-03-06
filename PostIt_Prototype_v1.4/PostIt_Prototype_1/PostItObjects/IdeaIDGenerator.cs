﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PostIt_Prototype_1.PostItObjects
{
    public class IdeaIDGenerator
    {
        static int saltVal = 0;
        static int localID = 1;
        static int hash(int a, int b)
        {
            return ((a + b) * (a + b + 1) / 2 + b);
        }
        static int getHashedID(int inputID)
        {
            if (saltVal == 0)
            {
                Random rnd = new Random();
                saltVal = rnd.Next(1, short.MaxValue / 2);
            }
            return hash(saltVal, inputID);
        }
        public static int generateID()
        {
            int globalID = getHashedID(localID);
            localID++;
            return globalID;
        }

    }
}
