﻿//#***************************************************************************
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
using System.Windows;
using System.Collections;


namespace WorkoutGenSD
{
    class SoundLine:IEnumerable //each line in the SoundFit file
    {
        public string path;//full path to source .wav file
        public string fileBody; //name of the file body (EXcluding .wav extension)
        private List<byte> representation; //fileBody + ",*****\n"

        public SoundLine Clone() //creates and returns a clone of itself
        {
            if (this.path == null && this.fileBody == null)
            {
                return new SoundLine();
            }
            else
            {
                SoundLine clonedLine = new SoundLine(this.path, this.fileBody + ".WAV");
                return clonedLine;
            }
        }
        
        public SoundLine()
        {
            this.path = null;
            this.fileBody = null;
            this.representation = new List<byte>(16);
            for (int ii = 0; ii < 8; ii++)
            {
                this.representation.Add((byte)'*');
            }
            this.representation.Add((byte)',');
            for (int ii = 9; ii < 14; ii++)
            {
                this.representation.Add((byte)'*');
            }
            this.representation.Add((byte)0x0d);
            this.representation.Add((byte)0x0a);
            //this.representation = "********,*****";//initial sound line in s*.fit file

        }
        
        public SoundLine(string pathParam, string safeParam)//initialize with full path to file
        {
            this.representation = new List<byte>(16);
            this.path = pathParam;
            
            this.fileBody = (safeParam.ToUpper());
            if (fileBody.EndsWith(".WAV"))
            {
               // fileBody = fileBody.TrimEnd('.', 'W', 'A', 'V');
                fileBody = fileBody.Remove(8, 4);
            }
            for (int ii = 0; ii < 8; ii++)
            {
                this.representation.Add((byte)(char)fileBody[ii]);
            }
            this.representation.Add((byte)',');
            for (int ii = 9; ii < 14; ii++)
            {
                this.representation.Add((byte)'*');
            }
            this.representation.Add((byte)0x0d);
            this.representation.Add((byte)0x0a);
            
            //this.representation = this.fileBody + ",*****";

        }
        public override string ToString()
        {
            string ret = "";
            foreach (byte b in this.representation)
            {
                ret += System.Convert.ToChar(b).ToString();
            }
           
            return ret;
        }
 
        public bool Equals(SoundLine line)
        {
            bool isEqual = true;

            if (this.fileBody != line.fileBody)
            {
                isEqual = false;
            }

            if (this.path != line.path)
            {
                isEqual = false;
            }

            if (this.representation != line.representation)
            {
                isEqual = false;
            }

            return isEqual;
        }


        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (byte b in this.representation)
            {
                yield return b;
            }
        }

        #endregion
    }
}
