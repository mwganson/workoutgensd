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
using System.Windows;
using System.Windows.Documents;
using System.Collections;
using System.IO;
using Microsoft.Win32;

namespace WorkoutGenSD
{
    class BinaryFit:IEnumerable
    {
        private List<CommandBlock> blocks;
        //BinaryFit class is basically a collection of command blocks
        //and the assorted methods needed to manage them
        public string Name; //file name body only e.g. W000000c
        private int workoutLength;

        public BinaryFit Clone()
        {
            BinaryFit bf = new BinaryFit(this.Name, this.workoutLength);
            bf.Clear();
            
            foreach (CommandBlock b in this.blocks)
            {
                bf.Add(b.Clone());
               
            }

            return bf;

        }
        
        public BinaryFit Clone(string bfNameParam, BinaryFit bfToBeReplaced, double speedGradient, double inclineGradient)
        {
            BinaryFit bf = new BinaryFit(bfNameParam, workoutLength);
            int nthX5d = 0;
            int idx = 0;
            for (int ii = 0; ii < this.Count; ii++)
            {
                if (this.blocks[ii].Type != BlockType.AdjustSpeedAndIncline)
                {
                    bf.Insert(idx++, this.blocks[ii].Clone());
                }
                else
                {
                    idx++;
                    CommandBlock newX5d = bfToBeReplaced.GetNth0x5dBlock(nthX5d++);
               //     CommandBlock newX5d = this.blocks[ii].Clone();
                 //   CommandBlock newX5d = bf.GetNth0x5dBlock(nthX5d-1);
                    int indexOfNthX5d = bf.IndexOfNth0x5dBlock(nthX5d - 1);
                //    newX5d = oldX5d;
                   

                    if (this.blocks[ii].IsGraduatable && nthX5d!=CountOf0x5dBlocks())//don't increment the final x5d block
                                                                              //since it's used to end the program and set incline to 0
                    {
                        newX5d.CurrentSpeed = this.blocks[ii].CurrentSpeed + speedGradient;
                        newX5d.CurrentSpeedMetric = newX5d.CurrentSpeed * 1.609344;
                        if (newX5d.CurrentSpeed > this.blocks[ii].SpeedSlider.Maximum)
                        {
                            newX5d.CurrentSpeed = this.blocks[ii].SpeedSlider.Maximum;
                        }
                        if (newX5d.CurrentSpeed < this.blocks[ii].SpeedSlider.Minimum)
                        {
                            newX5d.CurrentSpeed = this.blocks[ii].SpeedSlider.Minimum;
                        }
                        newX5d.CurrentIncline = this.blocks[ii].CurrentIncline + inclineGradient;
                        if (newX5d.CurrentIncline > this.blocks[ii].InclineSlider.Maximum)
                        {
                            newX5d.CurrentIncline = this.blocks[ii].InclineSlider.Maximum;
                        }
                        if (newX5d.CurrentIncline < this.blocks[ii].InclineSlider.Minimum)
                        {
                            newX5d.CurrentIncline = this.blocks[ii].InclineSlider.Minimum;
                        }
                    } else
                        if (!this.blocks[ii].IsGraduatable && nthX5d != CountOf0x5dBlocks())//just copy the values
                        {
                            newX5d.CurrentSpeed = this.blocks[ii].CurrentSpeed;
                            newX5d.CurrentSpeedMetric = this.blocks[ii].CurrentSpeed * 1.609344;
                            if (newX5d.CurrentSpeed > this.blocks[ii].SpeedSlider.Maximum)
                            {
                                newX5d.CurrentSpeed = this.blocks[ii].SpeedSlider.Maximum;
                            }
                            if (newX5d.CurrentSpeed < this.blocks[ii].SpeedSlider.Minimum)
                            {
                                newX5d.CurrentSpeed = this.blocks[ii].SpeedSlider.Minimum;
                            }
                            newX5d.CurrentIncline = this.blocks[ii].CurrentIncline;
                            if (newX5d.CurrentIncline > this.blocks[ii].InclineSlider.Maximum)
                            {
                                newX5d.CurrentIncline = this.blocks[ii].InclineSlider.Maximum;
                            }
                            if (newX5d.CurrentIncline < this.blocks[ii].InclineSlider.Minimum)
                            {
                                newX5d.CurrentIncline = this.blocks[ii].InclineSlider.Minimum;
                            }
                        }
                    newX5d.Invalidate();
                    try
                    {
                        bf.blocks[indexOfNthX5d] = newX5d;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString());
                        return bf;
                    }
                    
                }
            }
            return bf;
        }
        
        
        public int Count
        {
            get{return this.blocks.Count;}
        }


        public void Remove(CommandBlock cb)
        {
            blocks.Remove(cb);
        }

        public int IndexOf(CommandBlock cb)
        {
            int idx = -1;
            for (int ii = 0; ii < blocks.Count; ii++)
            {
                if (blocks[ii].Equals(cb))
                {
                    return ii;
                }
            }
            return idx;
        }

        public void StripWaves()
        {
            for (int ii = this.Count - 1; ii >= 0; ii--)
            {
                if (blocks[ii].Type == BlockType.FetchWaveFile ||
                    blocks[ii].Type == BlockType.PlayFetchedWaveFile ||
                    blocks[ii].Type == BlockType.PlayWaveFile)
                {
                    this.RemoveAt(ii);
                }
            }

        }



        public double GetSpeedInFastestBlock()
        {
            double topSpeed = 0;
            for (int ii = 0; ii < Count; ii++)
            {
                if (blocks[ii].Type == BlockType.AdjustSpeedAndIncline)
                {
                    if (blocks[ii].CurrentSpeed > topSpeed && blocks[ii].CurrentSpeed != 0xfa)
                    {
                        topSpeed = blocks[ii].CurrentSpeed;
                    }
                }
            }
            return topSpeed;
        }

        public List<double> GetMets()
        {
            List<double> mets = new List<double>(CountOf0x5dBlocks() - 1);
            for (int ii = 0; ii < CountOf0x5dBlocks() - 2; ii++)
            {
                CommandBlock x5d = GetNth0x5dBlock(ii);
                double speed = x5d.CurrentSpeed;
                double incline = x5d.CurrentIncline;
                mets.Add(calculateMets(speed, incline));
            }

            return mets;
            
        }


        private double calculateMets(double speed, double incline)
        {

            double dMetersPerMinute;


            double dGrade = incline;
            double dVO2LitersTotal;
            double dMets;
            double dSpeedKPH = speed * 1.609344;
            dMetersPerMinute = dSpeedKPH * 16.7;


            if (dSpeedKPH >= 5.9545728)
            {
                //Then ‘3.7 mph Average
                dVO2LitersTotal = 0.2 * dMetersPerMinute +
                ((dGrade / 100) *
                dMetersPerMinute * 0.9) + 3.5;
            }
            else
            {

                dVO2LitersTotal = 0.1 * dMetersPerMinute +
                ((dGrade / 100) *
                dMetersPerMinute * 1.8) + 3.5;
            }


            //‘3.5 VO2 = 1 Met
            dMets = (1 / 3.5) * dVO2LitersTotal;




            return dMets;


        }
        
        
        
        public void UnFocusAllSliders()
        {
            //MessageBox.Show("Unfocusing all sliders", this.Name);
        //    Console.Beep();
            for (int ii = 0; ii < this.CountOf0x5dBlocks(); ii++)
            {
                CommandBlock cb = this.GetNth0x5dBlock(ii);
                cb.UnFocus();
            }
        }

 
        public void AdjustShowInitialInclineBlock()
        {
            CommandBlock b = this.GetNthBlockType(BlockType.ShowInitialIncline, 0);
            if (b != null)
            {
                double initialIncline = this.GetInclineInSteepestBlock();
                b.CurrentIncline = initialIncline;
                b.Invalidate();


            }
            else
            {
                MessageBox.Show("Logical program error: NthBlockType.AdjustShowInitalInclineBlock = null",Name);
            }




        }
        
        
        public void AdjustShowInitialSpeedBlock()
        {
            CommandBlock b = this.GetNthBlockType(BlockType.ShowInitialSpeed, 0);
            if (b != null)
            {
                double initialSpeed = this.GetSpeedInFastestBlock();
                b.CurrentSpeed = initialSpeed;
                b.CurrentSpeedMetric = initialSpeed * 1.609344;
                b.Invalidate();
            }
            else
            {
                MessageBox.Show("Logical program error: NthBlockType.AdjustShowInitalSpeedBlock = null",Name);
            }



        }
        
        
        public void AdjustInitialSpeedAndInclineHeaderBlock()
        {
            CommandBlock b = this.GetNthBlockType(BlockType.SetInitialSpeedAndIncline, 0);
            if (b != null)
            {
                double initialSpeed = this.GetNthBlockType(BlockType.AdjustSpeedAndIncline, 0).CurrentSpeed;
                double initialIncline = this.GetNthBlockType(BlockType.AdjustSpeedAndIncline, 0).CurrentIncline;
                b.CurrentSpeed = initialSpeed;
                b.CurrentSpeedMetric = initialSpeed * 1.609344;
                b.CurrentIncline = initialIncline;
                b.Invalidate();


            }
            else
            {
                MessageBox.Show("Logical program error: NthBlockType.SetInitialSpeedAndIncline = null",this.Name);
            }





        }
        
        
        
        public double GetInclineInSteepestBlock()
        {
            double topIncline = 0;
            for (int ii = 0; ii < Count; ii++)
            {
                if (blocks[ii].Type == BlockType.AdjustSpeedAndIncline)
                {
                    if (blocks[ii].CurrentIncline > topIncline && blocks[ii].CurrentIncline != 0xfa)
                    {
                        topIncline = blocks[ii].CurrentIncline;
                    }
                }
            }
            return topIncline;

        }

        public int IndexOf0x60BlockWithTimeStamp(int seconds)
        {
            int index =-1;//-1 signifies no such 0x60 block exists with this same timestamp

            for (int ii = 0; ii < this.blocks.Count; ii++)
            {
                if (blocks[ii].Type == BlockType.PlayWaveFile)
                {
                    if (blocks[ii].CurrentTimeStamp == seconds)
                    {
                        index = ii;
                        break;
                    }
                }
            }

            return index;
        }


        public CommandBlock GetNthWaveFileBlockAtInterval(int minute, int nth)
        {
            int startIndex = this.IndexOfNth0x5dBlock(minute);
            int endIndex = this.IndexOfNth0x5dBlock(minute + 1);
            int countX5d = CountOf0x5dBlocks();
            if (minute == countX5d-1) //we're finding files at the End of the Workout
            {
                endIndex = Count - 1;
            }
            int counter = 0;
            for (int ii = startIndex; ii < endIndex; ii++)
            {
                if (blocks[ii].Type == BlockType.FetchWaveFile /*0x5a*/ ||
                    blocks[ii].Type == BlockType.PlayWaveFile /*0x60*/)
                {
                    if (counter++ == nth)
                    {
                        return blocks[ii];
                    }
                    
                }
            }
            return null;
        }

        public int IndexOfNthBlockTypeAtInterval(BlockType t, int minute, int nth)
        {
            int startIndex = this.IndexOfNth0x5dBlock(minute);
            int endIndex = this.IndexOfNth0x5dBlock(minute + 1);
            int countX5d = CountOf0x5dBlocks();
            if (minute == countX5d - 1) //we're finding files at the End of the Workout
            {
                endIndex = Count - 1;
            }
            int counter = 0;
            for (int ii = startIndex; ii < endIndex; ii++)
            {
                if (blocks[ii].Type == t)
                {
                    if (counter++ == nth)
                    {
                        return ii;
                    }

                }
            }
            return -1;
        }


        public int CountOfWaveFileBlocksInHeader()
        {
            int startIndex = 0;
            int endIndex = this.IndexOfNth0x5dBlock(0);
            int counter = 0;
            for (int ii = startIndex; ii < endIndex; ii++)
            {
                if (blocks[ii].Type == BlockType.FetchWaveFile )
                {
                    counter++;
                }
            }
            return counter;
        }

        public CommandBlock GetNthWaveFileBlockInHeader(int nth)
        {
            int startIndex = 0;
            int endIndex = this.IndexOfNth0x5dBlock(0);
            int counter = 0;
            for (int ii = startIndex; ii < endIndex; ii++)
            {
                if (blocks[ii].Type == BlockType.FetchWaveFile)
                {
                    if (counter++ == nth)
                    {
                        return blocks[ii];
                    }

                }
            }
            return null;
        }


        
        public int CountOfWaveFileBlocksInThisInterval(int minute)
        {
            int startIndex = this.IndexOfNth0x5dBlock(minute);
            int endIndex = this.IndexOfNth0x5dBlock(minute + 1);
            if (minute == CountOf0x5dBlocks()-1) //we're finding files at the End of the Workout
            {
                endIndex = Count - 1;
            }
            int counter = 0;
            for (int ii = startIndex; ii < endIndex; ii++)
            {
                if (blocks[ii].Type == BlockType.FetchWaveFile /*0x5a*/ ||
                    blocks[ii].Type == BlockType.PlayWaveFile /*0x60*/)
                {
                    counter++;
                }
            }
            return counter;

        }



        public int CountOfBlockType(BlockType t)
        {
            int counter = 0;
            foreach (CommandBlock cb in this.blocks)
            {
                if (cb.Type == t)
                {
                    counter++;
                }

            }
            return counter;



        }
        
        
        public int CountOf0x5dBlocks()
        {
            int counter = 0;
            foreach (CommandBlock cb in this.blocks)
            {
                if (cb.Type == BlockType.AdjustSpeedAndIncline)
                {
                    counter++;
                }

            }
            return counter;



        }

        public string WaveFileNameAtNth0x5aBlock(int nth)
        {
            int index = -1;
            for (int ii = 0; ii < this.Count; ii++)
            {
                if (blocks[ii].Type == BlockType.FetchWaveFile)//0x5a block
                {

                    if (++index == nth)
                    {
                        return blocks[ii].CurrentWaveFileName;
                    }

                }
            }
            return null;

        }

        public void PropagateForwardInclineFromNth0x5dBlock(int nth)
        {
            double incline = GetNth0x5dBlock(nth).CurrentIncline;
            for (int ii = nth + 1; ii < CountOf0x5dBlocks() - 1; ii++)
            {
                CommandBlock x5d = GetNth0x5dBlock(ii);
                x5d.CurrentIncline = incline;
                x5d.InclineSlider.Value = incline;
                x5d.Invalidate();
            }
        }
        
        
        public void PropagateForwardSpeedFromNth0x5dBlock(int nth)
        {
            double speed = GetNth0x5dBlock(nth).CurrentSpeed;
            for (int ii = nth+1; ii < CountOf0x5dBlocks()-1; ii++)
            {
                CommandBlock x5d = GetNth0x5dBlock(ii);
                x5d.CurrentSpeed = speed;
                x5d.CurrentSpeedMetric = speed * 1.609344;
                x5d.SpeedSlider.Value = speed;
                x5d.Invalidate();
            }
        }
        public int IndexOfNth0x5dBlock(int nth)
        {

            int index = -1;
            for (int ii = 0; ii < this.Count; ii++)
            {
                if (blocks[ii].Type == BlockType.AdjustSpeedAndIncline)//0x5d block
                {

                    if (++index == nth)
                    {
                        return ii;
                    }

                }
            }
            return index;
        }

        public CommandBlock GetNthBlockType(BlockType t, int nth)
        {

            
            int index = -1;
            for (int ii = 0; ii < this.Count; ii++)
            {
                if (blocks[ii].Type == t)//
                {

                    if (++index == nth)
                    {
                        return blocks[ii];
                    }

                }
            }
            return null;





        }
        
        public int IndexOfNthBlockType(BlockType t, int nth)
        {

            int index = -1;
            for (int ii = 0; ii < this.Count; ii++)
            {
                if (blocks[ii].Type == t)//
                {
                    
                    if (++index == nth)
                    {
                        return ii;
                    }
                 
                }
            }
            return -1;





        }

        public void AdjustByte8In0x5dBlocks() //byte #8 refers to the number of 0x60 blocks
                                                //immediately preceeding each 0x5d block
        {

            for (int ii = 0; ii < CountOf0x5dBlocks(); ii++)
            {
                CommandBlock x5d = GetNth0x5dBlock(ii);
                int idx = IndexOfNth0x5dBlock(ii);
                int num0x60s = CountOf0x60sBelowThisIndex(idx);
                int byte8 = 0xf7 - 9 * num0x60s;
                x5d.SetByte(7, (byte)byte8);
                x5d.Invalidate();



            }
        }



        public CommandBlock GetNth0x5dBlock(int nth)//returns the nth BlockType.Adjust..
                                                    // or null if none exist
        {
            CommandBlock b = null;
            int index = 0;
            for (int ii = 0; ii < this.Count; ii++)
            {
                if (blocks[ii].Type == BlockType.AdjustSpeedAndIncline)//0x5d block
                {
                    if (index == nth)
                    {
                        return blocks[ii];
                    }
                    else
                    {
                        index++;
                    }
                }
            }
          
            return b;
        }

        public string GetWaveNameAtTimeStamp(int seconds)
        {
            string waveName = "none"; //return "none" if no such wave 
            for (int ii = 0; ii < blocks.Count; ii++)
            {
                if (blocks[ii].Type == BlockType.PlayWaveFile
                    && blocks[ii].CurrentTimeStamp == seconds)
                {
                    waveName = blocks[ii].CurrentWaveFileName;
                    if (waveName == null)
                    {
                        waveName = "none";
                    }
                    break;

                }


                

            }
            if (waveName != "none")
            {
                int index = waveName.LastIndexOf("\\")+1;
                waveName = waveName.Substring(index, waveName.Length - index-4); //leave off ".wav"
            }
            return waveName;
        }

        public void AdjustCommandCountBlock()
        {
            CommandBlock commandCountBlock = this.blocks[0];
            if (commandCountBlock.Type != BlockType.CommandCount)
            {
                return;
            }
            else
            {
                short count = (short)(this.blocks.Count-1);//we don't count the commandCountBlock
                                                            //since it's not technically a command block
                byte highByte = (byte)((count & 0xff00) >> 8);
                byte lowByte = (byte)(count & 0x00ff);
                commandCountBlock.SetByte(0, highByte);
                commandCountBlock.SetByte(1, lowByte);
            }


        }
        
        public void Add(CommandBlock blockParam)
        {
            this.blocks.Add(blockParam);
            AdjustCommandCountBlock();

        }

        public CommandBlock GetAt(int index)
        {
            return blocks[index];
        }

        public void Clear()
        {
            this.blocks.Clear();
           
        }
        public void RemoveAt(int index)
        {
            if (this.Count > index && index >= 0)
            {
                this.blocks.RemoveAt(index);
                AdjustCommandCountBlock();
            }
            else
            {
                MessageBox.Show("Logical Error in BinaryFit.RemoveAt(): No such block");

            }
        }

        
        public void Insert(int index, CommandBlock b)
        {
            try
            {
                blocks.Insert(index, b);
            }
            catch (ArgumentOutOfRangeException aoore)
            {
                if (aoore != null) { 
                    MessageBox.Show("program error BinaryFit.Insert(idx,block) -- idx out of bounds", "program logical error");
                }
                blocks.Add(b);
            }
            AdjustCommandCountBlock();
        }

        public int x60sFromEnd() //probably will be the usual way to call this
        {
            return CountOf0x60sBelowThisIndex(this.Count-1);

        }
        
        public int CountOf0x60sBelowThisIndex(int index){
            int num0x60s=0;

            //we start at the specified index and work our way back down the list
            //eg if index = 35, we start at blocks[35] and work down to blocks[34]
            //to blocks[33] etc., until we find a block that is something other than
            //an 0x60 style block (BlockType.PlayWaveFile)

            //We need to know this in order to be able to set byte #8 in the 0x5d
            //BlockType.AdjustSpeedAndIncline type command blocks

            if (index > this.blocks.Count - 1)
            {
                MessageBox.Show("Program Logical Error: CountOf0x60sBelowThisIndex.  "
                    + "blocks[index] does not exist!");
            }
            else
            {
                for (int ii = index-1; ii >= 0; ii--)
                {
                    if (blocks[ii].Type == BlockType.PlayWaveFile)
                    {
                        num0x60s++;
                    }
                    else //break out of the loop for any other type of block
                    {
                        break;
                    }
                }
            }

            return num0x60s;
        }

        public void ReplaceBlockType(BlockType t, CommandBlock b)
        {
            //replace first found pre-existing block type t with new command block b
            //this is used to update the information contained in the header
            //blocks which said values were unknown at the time the header
            //blocks were first generated or said values have changed since then

            for (int ii = 0; ii < this.Count; ii++)
            {
                if (blocks[ii].Type == t)
                {
                    //we have our boy, so break out of this loop afterwards
                    RemoveAt(ii);
                    Insert(ii, b);
                    break;
                }

            }


        }

        public double GetPreviousIncline(int index)
        {
            double ret = 25;

            for (int ii = 0; ii < index; ii++)
            {
                if (blocks[ii].Type == BlockType.SetInitialSpeedAndIncline
                    || blocks[ii].Type == BlockType.AdjustSpeedAndIncline)
                {
                    if (blocks[ii].CurrentIncline != 25)
                    {
                        ret = blocks[ii].CurrentIncline;
                    }
                }

            }

            return ret;


        }




        public double GetPreviousSpeed(int index)
        {
            double ret=25;

            for (int ii = 0; ii < index; ii++)
            {
                if (blocks[ii].Type == BlockType.SetInitialSpeedAndIncline
                    || blocks[ii].Type == BlockType.AdjustSpeedAndIncline)
                {
                    if(blocks[ii].CurrentSpeed!=25)
                    {
                        ret = blocks[ii].CurrentSpeed;
                    }
                }

            }

            return ret;


        }



        public BinaryFit(string nameParam, int workoutLengthParam)
        {
            this.blocks = new List<CommandBlock>(250);
            this.workoutLength = workoutLengthParam;
            if (this.workoutLength == -1)
            {
                MessageBox.Show("Invalid workout length of -1");
            }
           // this.blocks.
          //  MessageBox.Show("Hello from BinaryFit");
            this.Name = String.Format(nameParam);
            for (int ii = 0; ii <= workoutLength; ii++)
            {
                Add(new CommandBlock(BlockType.AdjustSpeedAndIncline, ii, 0, 0, 0));


            }

        }

        private bool isGoodChecksum(byte command, byte[] buf)
        {
            bool isGood = false;
            int sum = command;
            foreach (byte b in buf)
            {
                sum += b;
            }
            if (sum % 256 == 0)
            {
                isGood = true;
            }
            return isGood;

        }

        private void diskError()
        {
            MessageBox.Show("Error in checksum reading bytes from disk.");
        }

 
        
        
        public void LoadFromDisk(string path, SoundFit sF)
        {
            blocks.Clear(); //clear out any pre-existing command blocks in this object
            blocks = new List<CommandBlock>(250);
            BinaryReader objFile = new BinaryReader(File.Open(path, FileMode.Open));
            BinaryReader iniFile = null;
            string iniPath = path.Substring(0, path.Length - 4) + ".bin";
            bool bIniExists = File.Exists(iniPath);
            if (bIniExists)
            {
                try
                {
                    iniFile = new BinaryReader(File.Open(iniPath, FileMode.Open));
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    bIniExists = false; //couldn't open it, so ignore it
                }
            }


          //  int ii = 0;
            byte[] byteBuf = new byte[9]; //9 bytes is the largest command block type
            byte[] iniBuf = new byte[1]; //only need a one-byte buffer since we'll only be reading one byte at a time
            
            
            byteBuf=objFile.ReadBytes( 2);//first 2 bytes always command count
            blocks.Add(new CommandBlock(BlockType.CommandCount, byteBuf[0] * 256 + byteBuf[1]));
            //now we have to peek ahead to see how many bytes to read for each new block
            bool bContinue = true;
            while (bContinue)//we'll break out at the end of the file
            {
                byte commandByte;
                try
                {
                   commandByte = objFile.ReadByte();
                }
                catch (EndOfStreamException e)
                {
             //       MessageBox.Show(e.ToString());
                    EndOfStreamException eos = e;
                    objFile.Close();
                    bContinue = false;
                    commandByte = 0xff;
                   
                }

                
                    int exp1, exp2, exp3, exp4;
                    int timeParam, speedParam, inclineParam, wavParam;
                    BlockType t;
                    switch (commandByte)
                    {
                        case 0x04:
                            t = BlockType.UnknownBlock02; //3 bytes in this block
                            byteBuf = objFile.ReadBytes(2);//already read the first byte
                            if (!isGoodChecksum(commandByte, byteBuf))
                            {
                                diskError();
                                return;
                            }
                            else //the checksum test passed, so we're good to go with this block
                            {
                                exp1 = byteBuf[0];
                                blocks.Add(new CommandBlock(t, exp1));
                            }
                            break;

                        case 0x05:
                            t = BlockType.ShowProgressGraphics;//3 bytes
                            byteBuf = objFile.ReadBytes(2);
                            if (!isGoodChecksum(commandByte, byteBuf))
                            {
                                diskError();
                                return;
                            }
                            else
                            {
                                exp1 = byteBuf[0];
                                blocks.Add(new CommandBlock(t, exp1));
                            }
                            break;
                        case 0x0c:
                            t = BlockType.ShowInitialTime;
                            byteBuf = objFile.ReadBytes(2);
                            if (!isGoodChecksum(commandByte, byteBuf))
                            {
                                diskError();
                                return;
                            }
                            else
                            {
                                timeParam = byteBuf[0];
                                blocks.Add(new CommandBlock(t, timeParam));
                            }
                            break;
                        case 0x0f:
                            t = BlockType.GenerateBarGraph; //2 bytes
                            byteBuf = objFile.ReadBytes(1);
                            if (!isGoodChecksum(commandByte, byteBuf))
                            {
                                diskError();
                                return;
                            }
                            else
                            {
                                blocks.Add(new CommandBlock(t));
                            }
                            break;
                        case 0x0d:
                            t = BlockType.MaxRunTime;
                            byteBuf = objFile.ReadBytes(4);
                            if (!isGoodChecksum(commandByte, byteBuf))
                            {
                                diskError();
                                return;
                            }
                            else
                            {
                                exp1 = byteBuf[0];
                                timeParam = (byteBuf[1] * 256 + byteBuf[2]) / 60;
                                //timeParam is converted from seconds to minutes
                                blocks.Add(new CommandBlock(t, timeParam, exp1));
                            }
                            break;
                        case 0x06:
                            t = BlockType.ShowInitialSpeed;
                            byteBuf = objFile.ReadBytes(3);
                            if (!isGoodChecksum(commandByte, byteBuf))
                            {
                                diskError();
                                return;
                            }
                            else
                            {
                                speedParam = byteBuf[0];
                                exp1 = byteBuf[1];
                                blocks.Add(new CommandBlock(t, speedParam, exp1));
                            }
                            break;
                        case 0x07:
                            t = BlockType.UnknownBlock08;
                            byteBuf = objFile.ReadBytes(3);//already read the first byte
                            if (!isGoodChecksum(commandByte, byteBuf))
                            {
                                diskError();
                                return;
                            }
                            else
                            {
                                exp1 = byteBuf[0];
                                exp2 = byteBuf[1];
                                blocks.Add(new CommandBlock(t, exp1, exp2));
                            }
                            break;
                        case 0x08:
                            t = BlockType.ShowInitialIncline;
                            byteBuf = objFile.ReadBytes(2);
                            if (!isGoodChecksum(commandByte, byteBuf))
                            {
                                diskError();
                                return;
                            }
                            else
                            {
                                inclineParam = byteBuf[0];
                                blocks.Add(new CommandBlock(t, inclineParam));
                            }
                            break;

                        case 0x1c:
                            t = BlockType.UnknownBlock10;
                            byteBuf = objFile.ReadBytes(2);
                            if (!isGoodChecksum(commandByte, byteBuf))
                            {
                                diskError();
                                return;
                            }
                            else
                            {
                                exp1 = byteBuf[0];
                                blocks.Add(new CommandBlock(t, exp1));
                            }
                            break;
                        case 0x12:
                            t = BlockType.UnknownBlock11;
                            byteBuf = objFile.ReadBytes(2);
                            if (!isGoodChecksum(commandByte, byteBuf))
                            {
                                diskError();
                                return;
                            }
                            else
                            {
                                exp1 = byteBuf[0];
                                blocks.Add(new CommandBlock(t, exp1));
                            }
                            break;
                        case 0x22:
                            t = BlockType.UnknownBlock12;
                            byteBuf = objFile.ReadBytes(2);
                            if (!isGoodChecksum(commandByte, byteBuf))
                            {
                                diskError();
                                return;
                            }
                            else
                            {
                                exp1 = byteBuf[0];
                                blocks.Add(new CommandBlock(t, exp1));
                            }
                            break;

                        case 0x13:
                            t = BlockType.UnknownBlock13;
                            byteBuf = objFile.ReadBytes(2);
                            if (!isGoodChecksum(commandByte, byteBuf))
                            {
                                diskError();
                                return;
                            }
                            else
                            {
                                exp1 = byteBuf[0];
                                blocks.Add(new CommandBlock(t, exp1));
                            }
                            break;
                        case 0x2d:
                            t = BlockType.UnknownBlock14;
                            byteBuf = objFile.ReadBytes(2);
                            if (!isGoodChecksum(commandByte, byteBuf))
                            {
                                diskError();
                                return;
                            }
                            else
                            {
                                exp1 = byteBuf[0];
                                blocks.Add(new CommandBlock(t, exp1));
                            }
                            break;
                        case 0x14:
                            t = BlockType.UnknownBlock15;
                            byteBuf = objFile.ReadBytes(3);
                            if (!isGoodChecksum(commandByte, byteBuf))
                            {
                                diskError();
                                return;
                            }
                            else
                            {
                                exp1 = byteBuf[0];
                                exp2 = byteBuf[1];
                                blocks.Add(new CommandBlock(t, exp1, exp2));
                            }
                            break;
                        case 0x23:
                            t = BlockType.UnknownBlock16;
                            byteBuf = objFile.ReadBytes(2);
                            if (!isGoodChecksum(commandByte, byteBuf))
                            {
                                diskError();
                                return;
                            }
                            else
                            {
                                exp1 = byteBuf[0];
                                blocks.Add(new CommandBlock(t, exp1));
                            }
                            break;
                        case 0x24:
                            t = BlockType.UnknownBlock17;
                            byteBuf = objFile.ReadBytes(2);
                            if (!isGoodChecksum(commandByte, byteBuf))
                            {
                                diskError();
                                return;
                            }
                            else
                            {
                                exp1 = byteBuf[0];
                                blocks.Add(new CommandBlock(t, exp1));
                            }
                            break;
                        case 0x51:
                            t = BlockType.PausePriorToStart;
                            byteBuf = objFile.ReadBytes(4);
                            if (!isGoodChecksum(commandByte, byteBuf))
                            {
                                diskError();
                                return;
                            }
                            else
                            {
                                exp1 = byteBuf[0];
                                exp2 = byteBuf[1];
                                exp3 = byteBuf[2];
                                blocks.Add(new CommandBlock(t, exp1, exp2, exp3));
                            }
                            break;
                        case 0x15:
                            t = BlockType.SetInitialSpeedAndIncline;
                            byteBuf = objFile.ReadBytes(3);
                            if (!isGoodChecksum(commandByte, byteBuf))
                            {
                                diskError();
                                return;
                            }
                            else
                            {
                                speedParam = byteBuf[0];
                                inclineParam = byteBuf[1];
                                blocks.Add(new CommandBlock(t, speedParam, inclineParam));
                            }
                            break;
                        case 0x5a:
                            t = BlockType.FetchWaveFile;
                            byteBuf = objFile.ReadBytes(5);
                            if (!isGoodChecksum(commandByte, byteBuf))
                            {
                                diskError();
                                return;
                            }
                            else
                            {
                                wavParam = byteBuf[0];
                                exp1 = byteBuf[1];
                                exp2 = byteBuf[2];
                                exp3 = byteBuf[3];
                                CommandBlock block = new CommandBlock(t, wavParam, exp1, exp2, exp3);
                                block.CurrentWaveFileName = sF.GetFileNameAtIndex(wavParam)+".WAV";
                                blocks.Add(block);
                            }
                            break;
                        case 0x5b:
                            t = BlockType.PlayFetchedWaveFile;
                            byteBuf = objFile.ReadBytes(4);
                            if (!isGoodChecksum(commandByte, byteBuf))
                            {
                                diskError();
                                return;
                            }
                            else
                            {

                                exp1 = byteBuf[0];
                                exp2 = byteBuf[1];
                                exp3 = byteBuf[2];
                                blocks.Add(new CommandBlock(t, exp1, exp2, exp3));
                            }
                            break;
                        case 0x60:
                            t = BlockType.PlayWaveFile;
                            byteBuf = objFile.ReadBytes(8);
                            if (!isGoodChecksum(commandByte, byteBuf))
                            {
                                diskError();
                                return;
                            }
                            else
                            {

                                exp1 = byteBuf[0];
                                timeParam = byteBuf[1] * 256 + byteBuf[2];
                                wavParam = byteBuf[3];
                                exp2 = byteBuf[4];
                                exp3 = byteBuf[5];
                                exp4 = byteBuf[6];
                                CommandBlock block = new CommandBlock(t, timeParam, wavParam, exp1, exp2, exp3, exp4);
                                block.CurrentWaveFileName = sF.GetFileNameAtIndex(wavParam)+".WAV";
                                blocks.Add(block);
                            }
                            break;
                        case 0x5d:
                            t = BlockType.AdjustSpeedAndIncline;
                            byteBuf = objFile.ReadBytes(8);
                            if (bIniExists)
                            {
                                try
                                {
                                    iniBuf = iniFile.ReadBytes(1);
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show(e.ToString());
                                    bIniExists = false; //couldn't read ini file, so ignore it
                                }
                            }
                            if (!isGoodChecksum(commandByte, byteBuf))
                            {
                                diskError();
                                return;
                            }
                            else
                            {
                                exp1 = byteBuf[0];
                              
                                int hiSeconds = byteBuf[1];
                                int loSeconds = byteBuf[2];
                                
                                timeParam = (hiSeconds*256+loSeconds)/60;
                                speedParam = byteBuf[3];
                                exp2 = byteBuf[4];
                                inclineParam = byteBuf[5];
                                int x60s = x60sFromEnd();
                                int xParam = 0xf7 - 9 * x60s;
                                CommandBlock block = new CommandBlock(t, timeParam, speedParam, inclineParam, xParam, exp1, exp2);
                                if (bIniExists && iniBuf[0] == 0x00)
                                {
                                    block.IsGraduatable = false;
                                }
                                else
                                {
                                    block.IsGraduatable = true;
                                }
                                blocks.Add(block);
                            }
                            break;
                        case 0x01:
                            t = BlockType.EndProgram;
                            byteBuf = objFile.ReadBytes(1);

                            if (!isGoodChecksum(commandByte, byteBuf))
                            {
                                diskError();
                                return;
                            }
                            else
                            {
                                blocks.Add(new CommandBlock(t));
                            }
                            break;

                        case 0xff://end of file has been reached
                            //0xff is not a real command, we just use it as a
                            //signal that we reached the end of the file stream
                            break;


                        default:
                            MessageBox.Show("Logical Error in BinaryFit.LoadFromDisk(): unknown command block type");
                            break;
                    }
                


            }
        }

        
        
        public override string ToString()
        {
            string ret=this.Name+"\n";
            foreach (CommandBlock b in this.blocks)
            {
                ret += b.ToString()+"\n";
            }

            return ret;
        }



        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (CommandBlock cb in this.blocks)
            {
                yield return cb;
            }
        }

        #endregion

        public void Insert0x60AccordingToTimeStamp(CommandBlock x60)
        {
            //we have to insert this 0x60 block type in its proper place
            int timeStamp = x60.CurrentTimeStamp;
            int interval = timeStamp / 60;
            int seconds = timeStamp % 60;
            int index = IndexOfNth0x5dBlock(interval)+1;
            int indexNext = IndexOfNth0x5dBlock(interval + 1);
            
            CommandBlock cb;
            for (int ii = index; ii < indexNext; ii++)
            {
                cb = GetAt(ii);
                if (cb.Type == BlockType.PlayWaveFile)
                {
                    if (cb.CurrentTimeStamp > timeStamp)
                    {
                        Insert(ii, x60);
                        return;
                    }
                }
            }

            Insert(indexNext, x60);

        }
    }
}
