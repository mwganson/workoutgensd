using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows;
using System.IO;
using Microsoft.Win32;

namespace WorkoutGenSD
{
    class LayoutFit:IEnumerable //encapsulates the Layout.fit binary file
                  //Yes, layout.fit *is* a *binary* file

        /*The LayoutFit class is basically the collection of workouts on the SD card.
         * Layout.fit contains a number of LayoutLines, each of which serves
         * to mate each w*.fit (binary workout) file to each s*.fit (sound text)
         * file.
         * 
         * The plan is to have all the BinaryFit and SoundFit
         * objects stored within this central Layout object, as collections (List<>)
         */
    {
        List<LayoutLine> lines;
        public List<BinaryFit> bFs;//our BinaryFits (workout bins)
        public List<SoundFit> sFs; //our SoundFits (sound text files)
        private int workoutLength;


        public void Replace(int index, LayoutLine l)
        {
            lines.RemoveAt(index);
            lines.Insert(index, l);
        }
        
        public LayoutFit(int numWorkoutsParam, int workoutLengthParam)
        {


            workoutLength = workoutLengthParam;
            lines = new List<LayoutLine>(numWorkoutsParam); //we can only have 17 workouts per layout?
                                            //that's how many ****,**** type lines there
                                            //are in the Heather sample from which we've
                                            //hacked these codes
            bFs = new List<BinaryFit>(numWorkoutsParam);
            sFs = new List<SoundFit>(numWorkoutsParam);
            ASCIIEncoding AE = new ASCIIEncoding();
            byte[] me = new byte[1]; //byte me :> :P :) ;)

            for (int ii = 0; ii < numWorkoutsParam; ii++) //Make our workouts be named workoutA
                                            //through workoutQ (and s000023A...s00023Q)
            {
                me[0] = lowByte((short)ii);
                me[0] += 65;//'A'
                string bFNamer = string.Format("W000000{0}", new String(AE.GetChars(me)));
                string sFNamer = string.Format("S000000{0}", new String(AE.GetChars(me)));
                bFs.Add(new BinaryFit(bFNamer, workoutLength));
                sFs.Add(new SoundFit(sFNamer));
                lines.Add(new LayoutLine(bFNamer, sFNamer));

            }
        }

        private byte lowByte(short original)
        {
            return ((byte)(original & 0x00ff));
        }

        public void ImportLayoutDotFitFile(int numWorkouts, string pathToLayoutFit)
        {

  
            char[] charBuf = new char[32];

            lines.Clear();
            sFs.Clear();
            bFs.Clear();
            System.IO.StreamReader objFile = null;
            objFile = new System.IO.StreamReader(pathToLayoutFit);
            for (int ii = 0; ii < numWorkouts; ii++)
            {
                objFile.ReadBlock(charBuf, 0, 32);
                lines.Add(new LayoutLine(charBuf));

                if (ii == 0)
                {
                    //determine workoutLength
                    string layoutDirectory = pathToLayoutFit.Substring(0,pathToLayoutFit.Length-10);

                    workoutLength = getWorkoutLength(layoutDirectory+lines[ii].binaryFitFileName + ".fit");


                }



                bFs.Add(new BinaryFit(lines[ii].binaryFitFileName, workoutLength));
                sFs.Add(new SoundFit(lines[ii].soundFitFileName));

                //okay, at this point we now have the LayoutFit object constructed
                //along with some empty BinaryFit and SoundFit objects
                //now we need to populate the BinaryFit and SoundFit objects with
                //valid information from the files on the disk

                //first we need to fill in the command blocks for this newly created
                //BinaryFit file

                BinaryFit bF = bFs[ii];
                SoundFit sF = sFs[ii];
                string bFName = new string(charBuf, 0, 8);
                string sFName = new string(charBuf, 9, 8);
                if (bFName.Equals("********")) //nothing to read from disk here
                {
                    ASCIIEncoding AE = new ASCIIEncoding();
                    byte[] me = new byte[1]; //byte me :> :P :) ;)
                    me[0] = lowByte((short)ii);
                    me[0] += 65;//'A'
                    string bFNamer = string.Format("W000000{0}", new String(AE.GetChars(me)));
                    string sFNamer = string.Format("S000000{0}", new String(AE.GetChars(me)));
                    bFs[ii] = new BinaryFit(bFNamer, workoutLength);
                    sFs[ii] = new SoundFit(sFNamer);
                    Replace(ii, new LayoutLine(bFNamer, sFNamer));

                }
                else //each BinaryFit object can load itself from disk
                {


                    string sFPath = pathToLayoutFit;
                    sFPath = sFPath.Replace("layout", sFName);
                    sF.LoadFromDisk(sFPath);

                    string bFPath = pathToLayoutFit;
                    bFPath = bFPath.Replace("layout", bFName);
                    bF.LoadFromDisk(bFPath, sF);




                }

            }

            
        }

        private int getWorkoutLength(string path)
        {
            int numX5ds = 0;
            byte[] buffer = new byte[8];
            if (File.Exists(path))
            {
                BinaryReader binReader =
                    new BinaryReader(File.Open(path, FileMode.Open));
                try
                {
                    bool bDone = false;
                    while (!bDone)
                    {
                        byte b = binReader.ReadByte();
                        if (b != (byte)0x5d)
                        {
                            continue;
                        }
                        else
                        {
                            //this might be an 0x5d block or it might just be some byte in a different block type
                            buffer = binReader.ReadBytes(8);
                            int checksum = b;
                            for (int ii = 0; ii < 8; ii++)
                            {
                                checksum += buffer[ii];
                            }
                            if (checksum % 256 == 0)
                            {
                                numX5ds++;
                            }
                            else
                            {
                                binReader.BaseStream.Position -= 8;
                            }

                        }



                    }
                }
                catch (EndOfStreamException e)
                {
                    if (e != null) { }//disable compile time warning about not using e
               //     MessageBox.Show(e.ToString(), "Error accessing binary workout file: " + path);

                }
                finally
                {
                    binReader.Close();
                }


            }




            return numX5ds-1;
        }


        public override string ToString()
        {
            string str = "Layout Lines: \n";
            foreach (LayoutLine ll in this.lines)
            {
                str += ll.ToString()+"\n";
            }

            str += "\n\nBinaryFits:\n";

            foreach (BinaryFit bF in this.bFs)
            {
                str += bF.ToString() + "\n";
            }
            str += "\n\nSoundFits:\n";
            foreach (SoundFit sF in this.sFs)
            {
                str += sF.ToString()+"\n";

            }

            return str;
        }
        
        
        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach(LayoutLine ll in this.lines){

                yield return ll;

            }
        }

        #endregion
    }
}
