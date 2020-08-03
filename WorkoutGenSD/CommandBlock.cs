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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Effects;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Configuration;

namespace WorkoutGenSD
{
    enum BlockType 
    { 
        //usage: new CommandBlock(BlockType.SomeType[, param1, param2...]) 
        //[,param1,param2,...] denotes optional parameters used depending
        //on BlockType parameter.  If no optional parameters are used, then
        //the command block will be creating using appropriate defaults unless
        //there are no appropriate defaults available in which case an error
        //message is displayed (not pretty, but only the programmer will see
        //these error messages during the development stage if he tries to
        //construct a command block with the incorrect number of parameters

        //Note: *ALL* parameters to the constructor should be integer values even though
        //the actual byte string will be in bytes.  The constructor expects integer
        //parameters and will convert them to bytes or pairs of bytes as necessary

        //Defaults are shown in the form of Commandbyte byte1 byte2 ... checksumbyte

        CommandCount,  //this BlockType is a 2-byte command block containing the number
                        //of command blocks in this workout file
                        //It should be constructed only after we know how many command blocks
                        //we have in the workout
                        //usage:
                        //new CommandBlock(BlockType.CommandCount, (int)numCommands)
                        //Note: constructor converts int param into 2 bytes
                        //this is the only command block type without a checksum byte
                       
                        
        UnknownBlock02, //this is the second command block (counting the 2-byte command count
                        //field as the first command block) in the binary file
                        //at this time we don't know what it does exactly, but it's here for
                        //potential experimenters to try different values and report the results
                        //It is a 3-byte field, including command byte and checksum
                        //Default: 0x04 0x03 0xf9
                        //usage:
                        // new Commandblock(BlockType.UnknownBlock02[, int experimental])
                        
        ShowProgressGraphics, //This block type will tell the equipment to show the progress
                              //bar graph during the workout.  It has 3 bytes.
                                //Default is 0x05 0x09 0xf2  Usage:        
                                //new CommandBlock(BlockType.ShowProgressGraphics[,int experimental)
                                
        ShowInitialTime, //This block type tells the equipment which initial time to show
                        //on the screen before the workout begins.  It has 3 bytes
                        //Default: 0x0c 0x14 0xe0  (0x14 = 20 dec = 20 minutes)
                        //usage: new CommandBlock(BlockType.ShowInitialTime, int minutes)
 


        GenerateBarGraph, //This command block type tells the equipment to generate the 
                        //bar graph displayed during the workout.  Without it, any displayed
                        //progress graph will be nonsensical.
                        //Default: 0x0f 0xf1  (no experimentation possible)
                        //Usage: new CommandBlock(BlockType.GenerateBarGraph); 
       
        MaxRunTime, // This command block type sets the actual maximum run time allowed
                    // for the workout, regardless of what the workout file contains
                    // That is, you can have a 30 minute workout defined, but if this
                    // command block says to limit the workout to 20 minutes, then the
                    // workout will end at 20 minutes.  Obviously, setting this command
                    //block properly is critical.
                    //Default: 0x0d 0x00 0x05 0xdc 0x12
                    //usage: new CommandBlock(BlockType.MaxRunTime, int minutes[,int exp])
                    //Note: exp relaces byte in position #2, if experimenting
        
        ShowInitialSpeed, //This command block (4 bytes) tells the equipment what to show
                         //as the maximum speed the user will have to walk/run at during
                        //the workout.  It is for user information purposes only.
                        //Default: 0x06 0x22 0x00 0xd8
                        //Usage: new CommandBlock(BlockType.ShowInitialSpeed, 
                        //           int mph[,int exp])
                           
        UnknownBlock08, //This is an unknown header block consisting of 4 bytes.
                        //Default: 0x07 0x00 0x00 0xf9
                        //Usage: new CommandBlock(BlockType.UnknownBlock08
                        //                      [, int experimental1, int experimental2])

        ShowInitialIncline, //This command block tells the equipment the max incline 
                            //to show to the user before the workout starts so he will
                            //know the max incline he will have to endure during this
                            //workout.  This is for user information only.
                            //Block consists of 3 bytes.
                            //Default 0x08 0x46 0xb2
                            //Usage: new CommandBlock(BlockType.ShowInitialIncline, int incline);

        UnknownBlock10, //This is an unknown header block consisting of 3 bytes.
                        //Default: 0x1c 0x01 0xe3
                        //Usage: new CommandBlock(BlockType.UnknownBlock10
                        //                      [, int experimental])

        UnknownBlock11, //refer to notes for UnknownBlock10
                        //Default: 0x12 0x01 0xed
        
        UnknownBlock12, //refer to notes for UnknownBlock10
                        //Default: 0x22 0x02 0xdc
        
        UnknownBlock13, //refer to notes for UnknownBlock10
                        //Default: 0x13 0x01 0xec
        
        UnknownBlock14, //refer to notes for UnknownBlock10
                        //Default: 0x2d 0x01 0xd2

        UnknownBlock15,//This is an unknown header block consisting of 4 bytes.
                        //Default: 0x14 0x14 0x00 0xd8
                        //Usage: new CommandBlock(BlockType.UnknownBlock15
                        //                      [, int experimental1, int experimental2])
        
        UnknownBlock16, //refer to notes for UnknownBlock10
                        //Default: 0x23 0x00 0xdd
        
        UnknownBlock17, //refer to notes for UnknownBlock10
                        //Default: 0x24 0x00 0xdc
        
        PausePriorToStart, //This tells the equipment to pause prior to start of workout
                            //until the user hits Start or Speed Increase button
                            //highly recommended for safety reasons!
                            //Default: 0x51 0xff 0xfb 0x01 0xb4
                            //Usage: new CommandBlock(BlockType.PausePriorToStart
                            //[,int exp1, int exp2, int exp3]) //3 experimental values
                            //probably not worth experimenting, but who knows?  
                            //Besides, why all those bytes for a simple boolean command?

        SetInitialSpeedAndIncline, //No idea why this isn't just done with the 0x5d
                                   //BlockType.AdjustSpeedAndIncline, but it isn't, so...
                                   //Default: 0x15 0x14 0x00 0xd7
                                   //Usage: new CommandBlock(BlockType.SetInitialSpeedAndIncline,
                                    //    int initialSpeed, int initialIncline)
        FetchWaveFile, //Used in the header to fetch a wave file prior to user
                            // pressing the start button to begin the workout
                            //Default: 0x5a 0x02 0xfa 0xfa 0x00 0xb0
                            //Usage: new CommandBlock(BlockType.FetchWaveFile, int fileIndex
                             //[,int exp1, int exp2, int exp3];

        PlayFetchedWaveFile, //Plays the previously fetched wave file
                            //Default: 0x5b 0xff 0xfb 0x01 0xaa
                            //Usage: new CommandBlock(BlockType.PlayFetchedWaveFile
                                //[,int exp1, int exp2, int exp3]);
                                          

        PlayWaveFile, //Plays a .wav file at a specified timeStamp (in seconds) from
                      //beginning of workout.  Param int fileIndex is the order in which
                      //the .wav file appears in the listing in the text file s*.fit
                      //.wav file format: RIFF PCM 64kbps 8000HZ Mono 8bit
                      //I'll (probably) create a SoundFile class to manage this s*.fit file
                      //Default: 0x60 0x00 0x00 0x35 0x08 0xfa 0xfa 0x00 0x6f
                      //usage: new CommandBlock(BlockType.PlayWaveFile, 
                      //                      int timeStamp, int fileIndex);
                      // or, allowing for 4 experimental bytes:
                      //new CommandBlock(BlockType.PlayWaveFile, int timeStamp,
                      //         int fileIndex, int exp1, int exp2, int exp3, int exp4);
                      // experimental bytes to be replaced are at positions 2, 6, 7, 8
 
        AdjustSpeedAndIncline, //this is where we will be periodically adjusting the
                        //rates of speed and percentages of incline during our workouts
                        //It has 9 bytes, 2 of which are experimental constants
                        //Default: 0x5d 0x00 0x00 0x3c 0x19 0x00 0x00 0xe5 0x69
                        //Usage: new CommandBlock(BlockType.AdjustSpeedAndIncline,
                        //      int timeStamp, int speed, int incline, int num0x60s
                        //       [,int exp1, int exp2]); //at #2 and #6 byte positions
                        //Param int num0x60s should be set to the number of 0x60 blocks
                        //immediately precede this 0x5d command block.  We need to know
                       // this in order to be able to set the byte at postion #8.
        
        BikeAdjust,   //used for bikes and ellipticals
                      //similar to adjust speed and incline block type
                      //8 bytes
                      //default:1 0x50 (command byte)
                        // 2      0x00 (constant)
                        // 3-4    0x00 0x3c (time bytes -- in seconds)
                        // 5      0x19 (RPM)
                        // 6      0x00 (Resistance)
                        // 7      0xe5 (number of 0x60 commands)
                        // 8      0x69 (checksum)
                        //usage: new CommandBlock(BlockType.BikeAdjust, int time1, int time2, int rpm, int resistance, int num0x60s)
        
        
        
        EndProgram  //This command block type tells the equipment to wrap it up and
                    //end the workout.
                    //default: 0x01 0xff
                    //Usage: new CommandBlock(BlockType.EndProgram);
        
  
        
        
        
    }   
    
    class CommandBlock :IEnumerable
    {
        private List<byte> bytes; //our string of bytes in this block

        //not all properties are meaningful for all block types
        public BlockType Type {get; set;}
        public double CurrentSpeed { get; set; }
        public double CurrentIncline { get; set; }
        public double CurrentSpeedMetric { get; set; }
        public int CurrentWaveFileIndex { get; set; }
        public string CurrentWaveFileName { get; set; }
        public int CurrentTimeStamp { get; set; }
        public Slider2 SpeedSlider;
        public Slider2 InclineSlider;
        public bool IsGraduatable = true;//false means to just copy this same value forward
                                        //when graduating workouts
      //  public bool IsMetricMode = false;

        public int Length()
        {
            return this.bytes.Count;
        }
        
        public CommandBlock Clone()
        {
            CommandBlock targetBlock = new CommandBlock();
            targetBlock.Type = this.Type;
            targetBlock.CurrentSpeed = this.CurrentSpeed;
            targetBlock.CurrentIncline = this.CurrentIncline;
            targetBlock.CurrentWaveFileIndex = this.CurrentWaveFileIndex;
            targetBlock.CurrentWaveFileName = this.CurrentWaveFileName;
            targetBlock.CurrentTimeStamp = this.CurrentTimeStamp;
            targetBlock.IsGraduatable = this.IsGraduatable;
            if (this.Type == BlockType.AdjustSpeedAndIncline)
            {
                targetBlock.SpeedSlider = new Slider2();
                targetBlock.SpeedSlider.OwnerBlock = targetBlock;
                targetBlock.InclineSlider = new Slider2();
                targetBlock.InclineSlider.OwnerBlock = targetBlock;
            }
            
            targetBlock.bytes = new List<byte>(9);
            for (int ii = 0; ii < this.bytes.Count; ii++)
            {
                targetBlock.bytes.Add(this.bytes[ii]);
            }
            
            return targetBlock;

        }
       
        
        public void Focus() //set background to indicate these 2 sliders have focus
        {
            if (this.Type != BlockType.AdjustSpeedAndIncline)
            {
                return; //only 0x5d blocks have valid slider controls
            }
            if (this.SpeedSlider != null && this.InclineSlider != null)
            {
              
                OuterGlowBitmapEffect effect = new OuterGlowBitmapEffect();
                effect.GlowColor = System.Windows.Media.Color.FromRgb((byte)70,(byte)90,(byte)225);
                effect.Noise = 0.0;
                effect.Opacity = 1.0;
                effect.GlowSize = 25;
                this.SpeedSlider.BitmapEffect= effect;
                this.InclineSlider.BitmapEffect = effect;
            
            }
        }

        public void UnFocus() //reset backgrounds to transparent to indicate not focused
        {
            if (this.Type != BlockType.AdjustSpeedAndIncline)
            {
                return; //only 0x5d blocks have valid slider controls
            }
            if (this.SpeedSlider != null && this.InclineSlider != null)
            {
                
                this.SpeedSlider.BitmapEffect = null;
                this.InclineSlider.BitmapEffect = null;
             //   Console.Beep();
                
                
                this.SpeedSlider.Background = Brushes.Transparent;
                this.InclineSlider.Background = Brushes.Transparent;
            }

        }


        public void Invalidate() //called when the bytes need to be adjusted
                                 //because the user modified speed/incline/etc.
        {
            switch (Type)
            {
                case BlockType.AdjustSpeedAndIncline:
                    
                    
                    int time = (int)this.CurrentTimeStamp*60;
                    this.bytes.RemoveAt(bytes.Count - 1);//remove checksum byte 
                    //bytes[0]=0x5d command code   
                    //bytes[1]=exp1 first experimental param
             
                    bytes[1] = lowbyte((short)0x00);
                    bytes[2]=highbyte((short)time);
                    bytes[3]=lowbyte((short)time);
                    bytes[4] = lowbyte((short)(System.Math.Round(this.CurrentSpeed, 1) * 10));
                 //   this.CurrentSpeedMetric = this.CurrentSpeed * 1.609344;
           
                   // bytes[5] = exp2 second experimental param
                    bytes[5] = lowbyte((short)(System.Math.Round(this.CurrentSpeedMetric,1)*10));
                    if (this.CurrentIncline <= 12.0)
                    {
                        bytes[6] = lowbyte((short)(this.CurrentIncline * 10.0));
                    }
                    else
                    {
                        bytes[6] = lowbyte((short)(96.0 + 2.0 * this.CurrentIncline));
                    }
                //bytes[7]=position #8 num0x60s value
                    DoChecksum(); //calcu and put checksum byte back
                    break;
                case BlockType.SetInitialSpeedAndIncline:
                    this.bytes.RemoveAt(bytes.Count - 1);//remove checksum byte 
                    //bytes[0]=0x15 command code 
                    bytes[1] = lowbyte((short)(System.Math.Round(this.CurrentSpeed,1) * 10));
                   // if (IsMetricMode)
                   // {
                   //     this.CurrentSpeedMetric = this.CurrentSpeed * 1.609344;
                        bytes[2] = lowbyte((short)(System.Math.Round(this.CurrentSpeedMetric,1) * 10));
                  //  }
                  //  else
                //    {
                //        bytes[2] = lowbyte((short)(this.CurrentIncline * 10));
                 //   }
                    DoChecksum(); //calcu and put checksum byte back
                    break;
                case BlockType.ShowInitialIncline:
                    this.bytes.RemoveAt(bytes.Count - 1);//remove checksum byte 
                    //bytes[0] = 0x08
                    int incParam = (int)(this.CurrentIncline * 10);
                    if (incParam > 120)
                    {
                        incParam = (int)(96 + 2 * this.CurrentIncline);
                    }
                    
                  //  bytes[1] = lowbyte((short)(this.CurrentIncline * 10));
                    bytes[1] = lowbyte((short)(incParam));
                    DoChecksum();

                    break;
                case BlockType.ShowInitialSpeed:
                    this.bytes.RemoveAt(bytes.Count - 1);//remove checksum byte 
                    //bytes[0]=0x06
                    bytes[1] = lowbyte((short)(System.Math.Round(this.CurrentSpeed,1) * 10));
                  //  this.CurrentSpeedMetric = this.CurrentSpeed * 1.609344;
                    bytes[2] = lowbyte((short)(System.Math.Round(this.CurrentSpeedMetric,1) * 10));
                  //  bytes[2] = (byte)0x00;//forget metric for now
                    DoChecksum();
                    break;
                case BlockType.ShowInitialTime:
                    this.bytes.RemoveAt(bytes.Count - 1);//remove checksum byte 
                    //bytes[0]=0x0c
                    bytes[1] = lowbyte((short)(this.CurrentTimeStamp));
                    DoChecksum();

                    break;

                case BlockType.UnknownBlock15:
                    this.bytes.RemoveAt(bytes.Count - 1); //remove checksum byte
                    //bytes[0]=0x14
                    
                 //   if (this.IsMetricMode)
                 //   {
                        bytes[1] = 0x14;
                        bytes[2] = 0x20;
                 //   }
                    
                    DoChecksum();
                    break;
                default:
                    MessageBox.Show("Program Logic Error: InvalidateBlocks():  Cannot invalidate" +
                        " this block type:"+this.ToString());
                    break;
            }
                    
        }

       
        
        public void SetByte(int ii, byte value) //set a byte in the bytes arraylist
        {
            this.bytes[ii] = value;
        }
        public byte GetByte(int ii)
        {
            return this.bytes[ii];
        }

        public void DoChecksum() //append the checksum byte to our List<byte> of bytes
            //the checksum byte is added to the sum of the other bytes in the List
            //in order to force the lower order byte of the sum of the other bytes
            //to 0x00.  The checksum is calculated as 256 - (remainder of sum / 256)
        {
            int checksum = 0;
            int sum = 0;
            for (int ii = 0; ii < this.bytes.Count; ii++)
            {
                sum += (int)this.bytes[ii];
                
            }
            checksum =  (256 - sum%256);
            this.bytes.Add((byte)checksum);

        }

        private byte lowbyte(short original)
        {
            return ((byte)(original & 0x00ff));
        }

        private byte highbyte(short original)
        {
            return ((byte)((original & 0xff00)>>8));
        }
        
        private void ShowError(string msg)
        {
            MessageBox.Show("Programming error: called new CommandBlock constructor"+
                       " using this BlockType: "+msg+
                       ", but did not provide the proper number of parameter arguments"
                , "Error in programming logic", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            //TODO: figure out how to exit the application here after the user hits OK?
            //Probably not, since the developers will be the only ones seeing this anyway.

        }

        public CommandBlock()
        {
            this.bytes = new List<byte>(9); //set capacity to largest known commandblock
            this.SpeedSlider = null;//only 0x5d blocks have valid sliders
            this.InclineSlider = null;


            this.CurrentIncline = -1; //-1 means it's not applicable for this BlockType
            this.CurrentSpeed = -1;
            this.CurrentWaveFileIndex = -1;
            this.CurrentTimeStamp = -1;
            this.IsGraduatable = true;

        }
        
        public CommandBlock(params object[] paramList) //Constructor taking BlockType param
        {
            this.bytes = new List<byte>(9); //set capacity to largest known commandblock
            this.SpeedSlider = null;//only 0x5d blocks have valid sliders
            this.InclineSlider = null;
            BlockType bT = (BlockType) paramList[0];
            this.Type = bT;
            this.CurrentIncline = -1; //-1 means it's not applicable for this BlockType
            this.CurrentSpeed = -1; 
            this.CurrentWaveFileIndex = -1;
            this.CurrentTimeStamp = -1;

            /*
             * We do a lot of conversions here from int to short to byte
             * We could just assume byte parameters where applicable, but
             * it is simpler to just assume all params are ints and do
             * the conversions here rather than force the calling method
             * to know (and remember to cast) the parameters as bytes
             * 
             * This way, all the calling method needs to do is just make every
             * parameter an int and be done with it.
             * 
             * */
            
            
            switch (bT)
            {
                case BlockType.CommandCount: //1st block in header
                    if (paramList.Length == 2) //used a single int parameter
                    {
                        int numCommands = (int)paramList[1];
                        bytes.Add(highbyte((short)numCommands));
                        bytes.Add(lowbyte((short)numCommands));
                    } 
                    else
                    {
                        ShowError(bT.ToString());
                    }
                    break; //Note: This command block type is unique in that we don't
                        //call DoChecksum() for it since it does not have a checksum byte

                case BlockType.UnknownBlock02: //2nd block in header
                    bytes.Add(0x04);//command code for this block type is 0x04
                    if (paramList.Length == 2)
                    {
                        int experimental = (int)paramList[1];
                        bytes.Add(lowbyte((short)experimental)); //use user experimental param
                    }
                    else
                    {
                        bytes.Add((byte)0x03); //not experimenting, so just use default unknown
                    }
                    DoChecksum(); //bytes.Add(checksumbyte)
                    break;
                
                case BlockType.ShowProgressGraphics: //3rd block in header
                    bytes.Add(0x05); //command byte for this block type

                    if (paramList.Length == 2) //experimenting, so...
                    {
                        int exp = (int)paramList[1];
                        bytes.Add(lowbyte((short)exp)); //use user experimental param
                    }
                    else
                    {
                        bytes.Add((byte)0x09); //not experimenting, so just use default unknown
                    }
                    DoChecksum();
                    break;
               
                case BlockType.ShowInitialTime: //4th block in header
                    bytes.Add(0x0c); //command code for showing initial time on LCD

                    if (paramList.Length == 2)
                    {
                        int time = (int)paramList[1];
                        bytes.Add(lowbyte((short) time)); //param is minutes in this workout
                        //this is interpreted as decimal value
                        //e.g. 0x14 = decimal 20 = 20 minute workout
                    }
                    else
                    {
                        ShowError(bT.ToString());
                    }   
                    DoChecksum();
                    break;
                
                case BlockType.GenerateBarGraph: //5th block in header
                    if (paramList.Length != 1)
                    {
                        ShowError(bT.ToString());
                    }
                    bytes.Add((byte)0x0f);
                    DoChecksum();
                    break;
                
                case BlockType.MaxRunTime: //6th block in header
                    bytes.Add((byte)0x0d); //command code for setting max run time is 0x0d
                    
                    if (paramList.Length != 2 && paramList.Length!=3)
                    {
                        ShowError(bT.ToString());
                    }
                    else if (paramList.Length==2) //single parameter (byteList[1]) should be integer value for minutes
                    {
                        bytes.Add((byte)0x00); //constant value of 0x00 used here

                    }
                    else if(paramList.Length==3){
                        int exp = (int)paramList[2];
                        bytes.Add(lowbyte((short)exp)); //replace 0x00 with experimental
                        int minutes = (int)paramList[1]; //equipment expects this in the form
                        int seconds = minutes * 60;//of seconds expressed as 2 bytes
                        bytes.Add(highbyte((short)seconds)); //high byte = seconds / 256
                        bytes.Add(lowbyte((short)seconds)); //low byte is remainder
                   
                    }                                       
                    DoChecksum();
                    break;
                
                case BlockType.ShowInitialSpeed: //7th block in header
                    bytes.Add((byte)0x06); //command code is 0x06 for this block type
                    if (paramList.Length != 2 && paramList.Length != 3)
                    {
                        ShowError(bT.ToString());
                    }
                    else if (paramList.Length == 2) //not experimenting
                    {
                        this.CurrentSpeed = (int)paramList[1];
                        bytes.Add(lowbyte((short)this.CurrentSpeed));
                        this.CurrentSpeed /= 10.0;
                        bytes.Add((byte)0x00); //constant value expected by equipment
                    }else if (paramList.Length==3) //we are experimenting
                    {
                        this.CurrentSpeed = (int)paramList[1];
                        bytes.Add(lowbyte((short)this.CurrentSpeed));
                        this.CurrentSpeed /= 10.0;
                        int exp = (int)paramList[2];
                        bytes.Add(lowbyte((short)exp));//replace default 0x00 with exp
                       
                    }
                        DoChecksum();
                        break;
                                    
                case BlockType.UnknownBlock08: //8th block in header
                    bytes.Add((byte)0x07); //command byte for this block type is 0x07
                    if (paramList.Length == 3)//we're experimenting..
                    {
                        int experimental1 = (int)paramList[1];
                        int experimental2 = (int)paramList[2];
                        bytes.Add((byte)lowbyte((short)experimental1));
                        bytes.Add((byte)lowbyte((short)experimental2));
                    }
                    else if (paramList.Length == 1)//just use the defaults
                    {
                        bytes.Add((byte)0x00);
                        bytes.Add((byte)0x00);
                    }
                    else
                    {
                        ShowError(bT.ToString());
                    }
                    DoChecksum();
                    break;
                case BlockType.ShowInitialIncline: //9th block in header
                    if (paramList.Length != 2)
                    {
                        ShowError(bT.ToString());
                    }
                    else
                    {
                        bytes.Add((byte)0x08); //command byte for this block type is 0x08
                        int incline = (int)paramList[1]; //recall that all params must be int!
                        bytes.Add(lowbyte((short)incline));
                    }
                    DoChecksum();
                    break;

                case BlockType.UnknownBlock10: //10th block in header
                    bytes.Add(0x1c);//command code for this block type is 0x1c
                    if (paramList.Length == 2)
                    {
                        int experimental = (int)paramList[1];
                        bytes.Add(lowbyte((short)experimental)); //use user experimental param
                    }
                    else
                    {
                        bytes.Add((byte)0x01); //not experimenting, so just use default unknown
                    }
                    DoChecksum(); //bytes.Add(checksumbyte)
                    break;
                case BlockType.UnknownBlock11: //11th block in header
                    bytes.Add(0x12);//command code for this block type is 0x12
                    if (paramList.Length == 2)
                    {
                        int experimental = (int)paramList[1];
                        bytes.Add(lowbyte((short)experimental)); //use user experimental param
                    }
                    else
                    {
                        bytes.Add((byte)0x01); //not experimenting, so just use default unknown
                    }
                    DoChecksum(); //bytes.Add(checksumbyte)
                    break;
                case BlockType.UnknownBlock12: //12th block in header
                    bytes.Add(0x22);//command code for this block type is 0x22
                    if (paramList.Length == 2)
                    {
                        int experimental = (int)paramList[1];
                        bytes.Add(lowbyte((short)experimental)); //use user experimental param
                    }
                    else
                    {
                        bytes.Add((byte)0x02); //not experimenting, so just use default unknown
                    }
                    DoChecksum(); //bytes.Add(checksumbyte)
                    break;
                case BlockType.UnknownBlock13: //13th block in header
                    bytes.Add(0x13);//command code for this block type is 0x13
                    if (paramList.Length == 2)
                    {
                        int experimental = (int)paramList[1];
                        bytes.Add(lowbyte((short)experimental)); //use user experimental param
                    }
                    else
                    {
                        bytes.Add((byte)0x01); //not experimenting, so just use default unknown
                    }
                    DoChecksum(); //bytes.Add(checksumbyte)
                    break;

                case BlockType.UnknownBlock14: //14th block in header
                    bytes.Add(0x2d);//command code for this block type is 0x2d
                    if (paramList.Length == 2)
                    {
                        int experimental = (int)paramList[1];
                        bytes.Add(lowbyte((short)experimental)); //use user experimental param
                    }
                    else
                    {
                        bytes.Add((byte)0x01); //not experimenting, so just use default unknown
                    }
                    DoChecksum(); //bytes.Add(checksumbyte)
                    break;

                case BlockType.UnknownBlock15: //15th block in header
                    bytes.Add((byte)0x14); //command byte for this block type is 0x14
                    if (paramList.Length == 3)//we're experimenting..
                    {
                        int experimental1 = (int)paramList[1];
                        int experimental2 = (int)paramList[2];
                        bytes.Add((byte)lowbyte((short)experimental1));
                        bytes.Add((byte)lowbyte((short)experimental2));
                    }
                    else if (paramList.Length == 1)//just use the defaults
                    {
                        bytes.Add((byte)0x14);
                        bytes.Add((byte)0x00);
                    }
                    else
                    {
                        ShowError(bT.ToString());
                    }
                    DoChecksum();
                    break;

                case BlockType.UnknownBlock16: //16th block in header
                    bytes.Add(0x23);//command code for this block type is 0x23
                    if (paramList.Length == 2)
                    {
                        int experimental = (int)paramList[1];
                        bytes.Add(lowbyte((short)experimental)); //use user experimental param
                    }
                    else
                    {
                        bytes.Add((byte)0x00); //not experimenting, so just use default unknown
                    }
                    DoChecksum(); //bytes.Add(checksumbyte)
                    break;
                case BlockType.UnknownBlock17: //17th block in header
                    bytes.Add(0x24);//command code for this block type is 0x24
                    if (paramList.Length == 2)
                    {
                        int experimental = (int)paramList[1];
                        bytes.Add(lowbyte((short)experimental)); //use user experimental param
                    }
                    else
                    {
                        bytes.Add((byte)0x00); //not experimenting, so just use default unknown
                    }
                    DoChecksum(); //bytes.Add(checksumbyte)
                    break;
                
                case BlockType.PausePriorToStart: //18th block in header
                    bytes.Add((byte)0x51); //command code for this block type is 0x51
                    if (paramList.Length == 1) //assume default implementation
                    {
                        bytes.Add((byte)0xff);//constant expected by equipment
                        bytes.Add((byte)0xfb);
                        bytes.Add((byte)0x01);
                    }
                    else if (paramList.Length == 4)//experimenting...
                    {
                        int exp1 = (int)paramList[1];
                        int exp2 = (int)paramList[2];
                        int exp3 = (int)paramList[3];
                        bytes.Add(lowbyte((short)exp1));
                        bytes.Add(lowbyte((short)exp2));
                        bytes.Add(lowbyte((short)exp3));
                    }
                    else //come on, man!
                    {
                        ShowError(bT.ToString());
                    }
                    DoChecksum();
                    break;
                case BlockType.SetInitialSpeedAndIncline: //19th block in header
                    bytes.Add((byte)0x15);//command code for this block type is 0x15
                    if (paramList.Length != 3)
                    {
                        ShowError(bT.ToString());
                    }
                    else //we have 2 (presumably) int parameters and all is good to go
                    {
                        int initialSpeed = (int)paramList[1];
                        int initialIncline = (int)paramList[2];
                        bytes.Add(lowbyte((short)initialSpeed)); //0x14 = dec 20 = 2.0mph
                        bytes.Add(lowbyte((short)initialIncline));//0x14 = dec 20 = 2%
                        this.CurrentSpeed = initialSpeed/10;
                        this.CurrentIncline = initialIncline/10;
                    }
                    DoChecksum();
                    break;
                    
                case BlockType.FetchWaveFile:
                    bytes.Add((byte)0x5a); //command code for this blocktype
                    int fIdx = (int)paramList[1];
                    this.CurrentWaveFileIndex = fIdx;
                    bytes.Add(lowbyte((short)fIdx));//file index
                    
                    if (paramList.Length != 2 && paramList.Length != 5)
                    {
                        ShowError(bT.ToString());

                    }
                    else if (paramList.Length == 2) // not experimenting
                    {
                        bytes.Add((byte)0xfa);//constants
                        bytes.Add((byte)0xfa);
                        bytes.Add((byte)0x00);
                    }
                    else if(paramList.Length==5)//experimenting
                    {
                        bytes.Add(lowbyte((short)(int)paramList[2]));
                        bytes.Add(lowbyte((short)(int)paramList[3]));
                        bytes.Add(lowbyte((short)(int)paramList[4]));

                    }
                    
                    DoChecksum();
                    break;

                case BlockType.PlayFetchedWaveFile:
                    bytes.Add((byte)0x5b);//command code
                    if (paramList.Length == 1)//not experimenting
                    {
                        bytes.Add((byte)0xff);
                        bytes.Add((byte)0xfb);
                        bytes.Add((byte)0x01);
                    }
                    else if (paramList.Length == 4)//use experimental parameters
                    {
                        bytes.Add(lowbyte((short)(int)paramList[1]));
                        bytes.Add(lowbyte((short)(int)paramList[2]));
                        bytes.Add(lowbyte((short)(int)paramList[3]));
                    }
                    else
                    {
                        ShowError(bT.ToString());
                    }

                    DoChecksum();
                    break;
                
                
                
                
                
                //We are now done with the header block!

                case BlockType.PlayWaveFile: //play a wave file indexed from s*.fit file
                    //Example: 0x60 0x00 0x00 0x35 0x08 0xfa 0xfa 0x00 0x6f
                    //9 bytes in this puppy, but a few of them are constants
                    //we expect 2 int parameters: timeStamp and fileIndex
                    //timeStamp will be an int value telling us the number of seconds from
                    //the beginning of the workout is when to play the file
                    //fileIndex is the sequential number representing the appearance of
                    //the .wav file in the s*.fit file associated with this workout
                    //Some other part of the program will have to figure out this index
                    if (paramList.Length != 3 && paramList.Length != 7)
                    {
                        ShowError(bT.ToString()); //show error before we crash and burn
                    }
                    
                    bytes.Add((byte)0x60); //0x60 is the command code for this block type
                    int timeStamp = (int)paramList[1]; //int value in seconds
                    int fileIndex = (int)paramList[2];
                    this.CurrentTimeStamp = timeStamp;//in seconds from start of workout
                    this.CurrentWaveFileIndex = fileIndex;
                   
                    
                    if (paramList.Length == 3)
                    {
                        bytes.Add((byte)0x00); //0x00 is a constant value expected here

                        bytes.Add(highbyte((short)timeStamp));//must be broken down
                        bytes.Add(lowbyte((short)timeStamp)); //into 2 bytes, q.e.d. ;)
                        bytes.Add(lowbyte((short)fileIndex)); //we can only have a maximum of
                        //255 (or maybe 256) files per
                        //workout, which should be plenty
                        bytes.Add((byte)0xfa); //constants expected by equipment here
                        bytes.Add((byte)0xfa); //might be used as delimiters?
                        bytes.Add((byte)0x00);
                    }
                    else if (paramList.Length==7) //experimenting
                    {
                        int exp1 = (int)paramList[3];
                        int exp2 = (int)paramList[4];
                        int exp3 = (int)paramList[5];
                        int exp4 = (int)paramList[6];

                        bytes.Add(lowbyte((short)exp1));// 0x00 was default
                        bytes.Add(highbyte((short)timeStamp)); 
                        bytes.Add(lowbyte((short)timeStamp)); 
                        bytes.Add(lowbyte((short)fileIndex));
                        bytes.Add(lowbyte((short)exp2)); //0xfa was default
                        bytes.Add(lowbyte((short)exp3)); //0xfa was default
                        bytes.Add(lowbyte((short)exp4)); //0x00 was default
                    }
                    DoChecksum();
                    break;

                case BlockType.AdjustSpeedAndIncline: //the heart of the matter...
                    //this baby is where we will be adjusting our speed and incline
                    //in the workout.  The byte in position #8 makes it a tricky one.
                    //This byte tells the equipment how many 0x60 command blocks are
                    //immediately preceding this 0x5d command block.  Not sure why
                    //the manufacturer needed this, but whuuuuutttever.

                    this.SpeedSlider = new Slider2();
                    this.InclineSlider = new Slider2();
               
             
                    bytes.Add((byte)0x5d); //command code for this block type is 0x5d
                    //we have 2 constant bytes in here, so let's allow for some
                    //experimentation here.
                    if (paramList.Length != 5 && paramList.Length != 7)
                    {
                        ShowError(bT.ToString()); //we goofed with param count
                    }
                    //pull our parameters out of the parameter list since it is evidently safe
                    //Note: setting speed to 0xfa (dec 250) means the program will just use
                    //the current speed setting without changing it

                    this.CurrentTimeStamp = (int)paramList[1]; //seconds from beginning of workout
                    int x5dSeconds = this.CurrentTimeStamp * 60;

                    this.CurrentSpeed = ((int)paramList[2]/10.0);
                    this.SpeedSlider.Value = this.CurrentSpeed;
                    this.SpeedSlider.OwnerBlock = this;
                    int inclineByte = (int)paramList[3];
                    if (inclineByte <= 120) //divide by 10 and get 12% (or less) under original formula
                    {
                        this.CurrentIncline = (double)inclineByte / 10.0;

                    }
                    else //to get incline from byte use: incline = byte/2-48
                        //to get byte from incline use: byte = 96 + 2*incline
                    {
                        this.CurrentIncline = (double)inclineByte / 2.0 - 48.0;


                    }
                    
                    
                  
                    this.InclineSlider.Value = this.CurrentIncline;
                    this.InclineSlider.OwnerBlock = this;
                                                    
                    

                    int num0x60s = (int)paramList[4];//number of 0x60 blocks preceding
                                                    //this 0x5d command block and subsequent
                                                    //to the most recent 0x5d block, if any
                    int pos8Code = 0xf7; //default to 0 0x60s
                    pos8Code -= 9 * num0x60s;//subtract 9 bytes for each 0x60 command block
                    // if I've done the math right
                    // we get:
                    // 0xf7 for 0
                    // 0xee for 1
                    // 0xe5 for 2
                    // 0xdc for 3
                    // ... (but do we really need more than 3?)
                    
                    if (paramList.Length == 5)//not experimenting
                    {
                        bytes.Add((byte)0x00); //use this constant since we're not experimenting
                        bytes.Add(highbyte((short)x5dSeconds));
                        bytes.Add(lowbyte((short)x5dSeconds));
                        bytes.Add(lowbyte((short)this.CurrentSpeed));
                        bytes.Add((byte)0x00);//our 2nd constant in this block
                      
                    //    bytes.Add(lowbyte((short)this.CurrentIncline));
                        bytes.Add(lowbyte((short)inclineByte));
                        bytes.Add(lowbyte((short)pos8Code));

                    } else if(paramList.Length==7)//we're experimenting
                    {
                        int exp1 = (int)paramList[5];
                        int exp2 = (int)paramList[6];
                        bytes.Add(lowbyte((short)exp1));//default was 0x00 in position #2 
                        bytes.Add(highbyte((short)x5dSeconds));
                        bytes.Add(lowbyte((short)x5dSeconds));
                        bytes.Add(lowbyte((short)this.CurrentSpeed));
                        bytes.Add(lowbyte((short)exp2));//default was 0x00 in position #6
                        
                       // bytes.Add(lowbyte((short)this.CurrentIncline));
                        bytes.Add(lowbyte((short)inclineByte));
                        bytes.Add(lowbyte((short)pos8Code));
                    }
                    
                    DoChecksum();
                    break;
                case BlockType.BikeAdjust:
                    //this should only be called from where we are generating the binary fit file
                    //by code where we already have the valid 0x5d treadmill adjust block setup
                    //we use many of the bytes from the 0x5d block as-is, translations are done
                    //by the calling code prior to calling

                    bytes.Add((byte)0x50); //command code for this block type is 0x50
                    
                    if (paramList.Length != 6)
                    {
                        ShowError(bT.ToString()); //we goofed with param count
                    }
                    //pull our parameters out of the parameter list since it is evidently safe
                    //Note: setting speed to 0xfa (dec 250) means the program will just use
                    //the current speed setting without changing it

                    int time1 = (int)paramList[1]; //seconds from beginning of workout
                    int time2 = (int)paramList[2];
                    int rpm = (int)paramList[3];
                    int resistance = (int)paramList[4];
                    int num0x60sCode = (int)paramList[5];//number of 0x60 blocks preceding


                    bytes.Add((byte)0x00); //use this constant since we're not experimenting
                    bytes.Add(lowbyte((short)time1));
                    bytes.Add(lowbyte((short)time2));
                    bytes.Add(lowbyte((short)rpm));
                    bytes.Add(lowbyte((short)resistance));
                    bytes.Add(lowbyte((short)num0x60sCode));
                    DoChecksum();
                    break;
                
                
                
                case BlockType.EndProgram:
                    if (paramList.Length != 1) //no additional parameters for this one
                    {
                        ShowError(bT.ToString());
                    }
                    bytes.Add((byte)0x01);//command code for this block type is 0x01
                    DoChecksum();
                    break;
                
                default:
                    string Msg = String.Format("LOGIC ERROR: Unknown or Unimplemented BlockType Command Block: {0}", bT.ToString());
                    MessageBox.Show(Msg);
                    break;

            }
        }

        public override string ToString()
        {
            string ret = "";
            foreach (byte b in this.bytes)
            {
                ret += String.Format("{0:X2} ",b);
            }

            ret = ret.Trim();
            ret += ";";
            ret += this.Type.ToString();
            if (this.Type == BlockType.AdjustSpeedAndIncline)
            {
                ret += String.Format(" cur minute:{0:D2} cur speed:{1:N2} cur inc.:{2:N2}", CurrentTimeStamp, CurrentSpeed, CurrentIncline);
            }
            else if (this.Type == BlockType.PlayWaveFile)
            {
                ret += String.Format(" cur sec:{0:D2} cur wavefile: {1}", CurrentTimeStamp, CurrentWaveFileName);
            }
            else if (this.Type == BlockType.FetchWaveFile
                        || this.Type == BlockType.PlayFetchedWaveFile)
            {
                ret += String.Format(" cur index:{0:D2} cur wavefile: {1}",CurrentWaveFileIndex, CurrentWaveFileName);
            }
            return ret;

        }
        
        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (byte b in this.bytes)
            {
                yield return b;
            }
        }

        #endregion
    }

}
