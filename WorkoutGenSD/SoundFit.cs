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
using Microsoft.Win32;
using System.Windows;
using System.IO;

namespace WorkoutGenSD
{
    class SoundFit:IEnumerable
    {
        private List<SoundLine> lines;
        public string Name; //file name body only e.g. s000023c
        
        public SoundFit(string nameParam)
        {
            this.lines = new List<SoundLine>(120);
            this.lines.Add(new SoundLine());//adds first "*******,*****\n" line
                                            //automatically upon construction

            this.lines.Add(new SoundLine());//append another to the end of the file
            this.Name = String.Format(nameParam);
        }

        public SoundFit Clone(string newName) //creates and returns a clone of itself
                                            //only difference being a new name
        {
            SoundFit copy = new SoundFit(newName);
            for (int ii = 1; ii < lines.Count-1; ii++)
            {
                copy.Add(this.lines[ii].Clone()); //actually inserts at next-to-last index

            }
            return copy;


        }
        
        public string GetFileNameAtIndex(int index)
        {
            return this.lines[index].fileBody;

        }
        public bool Exists(SoundLine l)
        {
            bool isPreExisting = false;

            foreach (SoundLine line in this.lines)
            {
                if (line.Equals(l))
                {
                    isPreExisting = true;
                }

            }



            return isPreExisting;

        }

        private string bodyFromPath(string path)
        {
            
            int lastIdx = path.LastIndexOf("\\")+1;
            string body = path.Substring(lastIdx, path.Length - lastIdx).ToUpper();
           // body = body.Trim('.', 'W', 'A', 'V');
            body = body.Remove(body.Length - 4);
            
            
            
            return body;
        }

        public string GetPathFromBody(string bodyParam)
        {
            string pathRet = null;
            string body = bodyParam;
            if (body.EndsWith(".WAV"))
            {
                body = body.TrimEnd('.','W', 'A', 'V');
            }
            for (int ii = 0; ii < this.lines.Count; ii++)
            {
                SoundLine sL = lines[ii];
                if (sL.fileBody == body)
                {
                    pathRet = sL.path;
                    break;
                }
            }

            return pathRet;
  
        }
        
       
        
        public int GetIndex(string path) //gets the index of the sound line containing
                                        //this full path name or creates new sound line 
                                        //and adds it if it doesn't already exist
        {
            int index = 0;
            bool bFound = false;
            string body = bodyFromPath(path);
            for (int ii = 0; ii < this.lines.Count; ii++)
            {
                if (lines[ii].path == path && lines[ii].fileBody==body)
                {
                    index = ii;
                    bFound = true;
                }
            }

            if (bFound)
            {
                return index;
            }
            else //no such line, so create one and add it, then call this method
                //recursively to fetch the index
            {
                Add(new SoundLine(path, body));
                return GetIndex(path);
            }
        }

        
        public void Add(SoundLine l)
        {
            if (!Exists(l))
            {
                this.lines.Insert(this.lines.Count-1,l);
            }
        }

        public void RemoveAt(int index)
        {
            this.lines.RemoveAt(index);
        }


        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (SoundLine s in this.lines)
            {
                yield return s;
            }
        }

        #endregion

        public void LoadFromDisk(string path)
        {
            lines.Clear();
            int index=path.LastIndexOf(@"\");
            string currentDirectory = path.Substring(0, index)+"\\";
            byte[] inBuf = new byte[16];
            string safe = "";
            BinaryReader objFile=null;
            try
            {
                objFile = new BinaryReader(File.Open(path, FileMode.Open));
            }
            catch (Exception except)
            {
                MessageBox.Show(except.ToString(), "Loading SoundFit File Error");
            }
            bool bContinue = true;
            ASCIIEncoding AE = new ASCIIEncoding();

            try
            {
                while (bContinue)
                {
                    inBuf = objFile.ReadBytes(16);
                    if (inBuf.Length < 16)
                    {
                        bContinue = false;
                        continue;
                    }
                    string inLine="";

                    inLine = new string(AE.GetChars(inBuf));
               //     index = inLine.LastIndexOf(",");
                //    safe = inLine.Substring(0, index);
                    safe = inLine.Substring(0,8);
                    if (safe != "********")
                    {
                        lines.Add(new SoundLine(currentDirectory+safe+".WAV", safe));
                    }
                    else
                    {
                        lines.Add(new SoundLine());//first "********,*****" line in every sound file?
                    }

                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString() + " From SoundFit.LoadFromDisk()");
                bContinue = false;


            }
            finally
            {
                objFile.Close();
              

            }

        }


    }
}
