//#***************************************************************************
//#*                                                                         *
//#*   Copyright (c) 2011 Mark Ganson <mwganson@gmail.com>                   *
//#*                                                                         *
//#*   This program is free software; you can redistribute it and/or modify  *
//#*   it under the terms of the GNU Lesser General Public License (LGPL)    *
//#*   as published by the Free Software Foundation; either version 2 of     *
//#*   the License, or (at your option) any later version.                   *
//#*   for detail see the LICENCE text file.                                 *
//#*                                                                         *
//#*   This program is distributed in the hope that it will be useful,       *
//#*   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
//#*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
//#*   GNU Library General Public License for more details.                  *
//#*                                                                         *
//#*   You should have received a copy of the GNU Library General Public     *
//#*   License along with this program; if not, write to the Free Software   *
//#*   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  *
//#*   USA                                                                   *
//#*                                                                         *
//#***************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;


namespace WorkoutGenSD
{
    class LayoutLine:IEnumerable
    {
        public string binaryFitFileName; //excludes .fit extension
        public string soundFitFileName; //excludes .fit extension
        private List<byte> representation; // representation of the line as a sequence
                                            //of bytes
        public int maxWorkouts { get; set; }
        private byte lowbyte(short original)
        {
            return ((byte)(original & 0x00ff));
        }


        public LayoutLine(char[] chars)
        {
            representation = new List<byte>(32);
            foreach (char c in chars)
            {
                representation.Add(lowbyte((short)c));
            }
            this.binaryFitFileName = new string(chars, 0, 8);
            this.soundFitFileName = new string(chars, 9, 8);
        }
        
        
        
        
        
        
        
        public  LayoutLine(string binFitName, string soundFitName)
        {
            this.binaryFitFileName = binFitName;
            this.soundFitFileName = soundFitName;

           
           
         
            
            representation = new List<byte>(32); //32 bytes in each line
            
            for (int jj = 0; jj < 8; jj++)
            {
                representation.Add((byte)binFitName[jj]);
            }
            representation.Add((byte)',');
            for (int kk = 9; kk < 17; kk++)
            {
                representation.Add((byte)soundFitName[kk - 9]);
            }
            representation.Add((byte)',');
            for (int ii = 1; ii <= 8; ii++)
            {
                representation.Add((byte)'*');
            }
            representation.Add((byte)',');

            representation.Add((byte)0x00);
            representation.Add((byte)0x00);
            representation.Add((byte)'*');
            representation.Add((byte)0x0d);
            representation.Add((byte)0x0a);
        }

        public override string ToString()
        {
            string ret="";
            foreach (byte b in representation)
            {
                ret += String.Format("{0:X2}", b);
            }
            return ret;

        }

     
#region IEnumerable Members

IEnumerator  IEnumerable.GetEnumerator()//give our representation as a string of bytes
{
    foreach (byte b in this.representation)
    {
        yield return b;
    }
}

#endregion
}

}
