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
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.IO.Packaging;
using Microsoft.Win32;
using System.Configuration;
//using Istrib.Sound.Filters;
//using Istrib.Sound.Formats;
//using Istrib.Sound;
using System.Speech.Synthesis;
using System.Threading;
using System.ComponentModel;


namespace WorkoutGenSD
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {

        
        
        string versionString = "WorkoutGenSD v0.2011.0130.1 (beta)";
        bool bScaleIntensityCanvas = false;
        string pathToMyDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string pathToAppSettingsIni;
        string pathToTTWFolder;

        long elapseDelay = 75; //delay modifying incline/speed settings in order to allow for fine precision of UI sliders

        DateTime lastValueChange = DateTime.Now; //attempt to workaround bug in slider value changed calls after re-running init() such as after new layout creation or 
                                //such as after opening a new file for editing

        bool bAutoCreateBikeFiles = true;
        bool bAutoCreateEllipticalFiles = true;
        bool bAutoCreateInclineFiles = true;


        bool bFileGenerationInProgress = false;
        bool bTTWInsertionInProgress = false;
        BackgroundWorker _backgroundWorker_GenFiles;
        BackgroundWorker _backgroundWorker_InsertTTWs;
        Canvas menuCanvas;


        FileGenStatusWindow statusWin = null;
        FileGenStatusWindow ttwStatusWin = null;

        string fileGenerationStatusString = "";
        string insertTTWStatusString = "";
        bool bAsync = true; //whether to create ttw files asynchronously
        
  //      string appSettingsPathName;
        int workoutLength=30; //Length of the workouts in this layout default: 30
        LayoutFit lF;
        int wIdx;// index of active workout
        MenuItem[] workoutMenus;
        int numWorkouts=26; //default is 17 (max?)
        bool isWorkoutMenusBuilt = false;

        bool disableSliderFocusOnMouseEnter = false; //disable this feature while 
                                                    //the context menu is active

      //  Mp3SoundCapture mp3Capture = null;
        bool bRecording = false; //signfies we are recording a file rather than from disk or ttw
        bool bTexting = false;//signifies we are creating a text file via the text-to-speech synth

        Package zipFile = null;
        static System.IO.Packaging.CompressionOption compressionOption = System.IO.Packaging.CompressionOption.NotCompressed;
        bool bZipping = false; //signifies we are saving our generated files into a .zip archive file

        
        //experimental variables preset to defaults
        int unknownBlock02Experimental1 = 0x03;
        int unknownBlock08Experimental1 = 0x00;
        int unknownBlock08Experimental2 = 0x00;
        int unknownBlock10Experimental1 = 0x01;
        int unknownBlock11Experimental1 = 0x01;
        int unknownBlock12Experimental1 = 0x02;
        int unknownBlock13Experimental1 = 0x01;
        int unknownBlock14Experimental1 = 0x01;
        int unknownBlock15Experimental1 = 0x14;
        int unknownBlock15Experimental2 = 0x00;
        int unknownBlock16Experimental1 = 0x00;
        int unknownBlock17Experimental1 = 0x00;
        bool doBlock02 = true;
        bool doBlock08 = true;
        bool doBlock10 = true;
        bool doBlock11 = true;
        bool doBlock12 = true;
        bool doBlock13 = true;
        bool doBlock14 = true;
        bool doBlock15 = true;
        bool doBlock16 = true;
        bool doBlock17 = true;

        int showProgressGraphicsExperimental1 = 0x09;
        bool doShowProgressGraphBlock = true;

        
        bool doShowInitialTime = true; //use workoutLength variable here
        bool doGenerateBarGraph = true;
        
        bool doMaxRunTime = true;
        int maxRunTime = 60;//maximum length of workouts for this program is 60 minutes
        int maxRunTimeExperimental1 = 0x00;

        bool doShowInitialSpeed = true;
        int showInitialSpeedExperimental1 = 0x00;

        bool doShowInitialIncline = true;

        bool doPausePriorToStart = true;
        int pausePriorToStartExperimental1 = 0xff;
        int pausePriorToStartExperimental2 = 0xfb;
        int pausePriorToStartExperimental3 = 0x01;

        bool doSetInitialSpeedAndIncline = true;
        string outputDirectory="";
        int userWeight = 150; //pounds (used for estimating calories burned)
     //   bool useMetricMode = false;
        bool bViewHorizontalSliders = true;
        bool bAutoWarmupCooldown = true;
        bool bAutoTTWsAtStartOfWorkouts = false;
        bool bAutoTTWsAtEachSpeedAndInclineAdjustment = false;
   //     bool bOverwriteWaveFilesInOutputDirectory = true;
        double maxSpeed = 12;
        double maxIncline = 12;


        Slider2 currentSmallSpeedSlider;
        Slider2 currentSmallInclineSlider;
        Slider2 speedSlider; //these are the big ones
        Slider2 inclineSlider;
        Label speedSliderLabel; //will have runtime relevant information
        Label inclineSliderLabel;

    




        public void GenerateInitialBlocks()
        {
            for (int ww = 0; ww < numWorkouts; ww++) //first the header blocks
            {
                BinaryFit b = lF.bFs[ww];
                int idx = 0; //insertion index

                b.Insert(idx++, new CommandBlock(BlockType.CommandCount, 0));//just use 0 for now
                if (doBlock02) //block 2
                {
                    b.Insert(idx++, new CommandBlock(BlockType.UnknownBlock02, unknownBlock02Experimental1));
                }

                if (doShowProgressGraphBlock) //block 3
                {
                    b.Insert(idx++, new CommandBlock(BlockType.ShowProgressGraphics, showProgressGraphicsExperimental1));
                }

                if (doShowInitialTime)//block 4
                {
                    b.Insert(idx++, new CommandBlock(BlockType.ShowInitialTime, workoutLength));
                }

                if (doGenerateBarGraph)//block 5
                {
                    b.Insert(idx++, new CommandBlock(BlockType.GenerateBarGraph));
                }

                if (doMaxRunTime) //block 6
                {
                    b.Insert(idx++, new CommandBlock(BlockType.MaxRunTime, maxRunTime, maxRunTimeExperimental1));
                }

                if (doShowInitialSpeed) //block 7
                {
                    //set initial speed displayed to 0, and reset this later before generating .fit files
                    //once we know what the maximum speed in a particular workout will be
                    b.Insert(idx++, new CommandBlock(BlockType.ShowInitialSpeed, 0, showInitialSpeedExperimental1));
                }
                if (doBlock08) //block 8
                {
                    b.Insert(idx++, new CommandBlock(BlockType.UnknownBlock08, unknownBlock08Experimental1, unknownBlock08Experimental2));

                }
                if (doShowInitialIncline) //block 9
                {
                    b.Insert(idx++, new CommandBlock(BlockType.ShowInitialIncline, 0));
                }
                if (doBlock10)
                {
                    b.Insert(idx++, new CommandBlock(BlockType.UnknownBlock10, unknownBlock10Experimental1));
                }
                if (doBlock11)
                {
                    b.Insert(idx++, new CommandBlock(BlockType.UnknownBlock11, unknownBlock11Experimental1));
                }
                if (doBlock12)
                {
                    b.Insert(idx++, new CommandBlock(BlockType.UnknownBlock12, unknownBlock12Experimental1));
                }
                if (doBlock13)
                {
                    b.Insert(idx++, new CommandBlock(BlockType.UnknownBlock13, unknownBlock13Experimental1));
                }
                if (doBlock14)
                {
                    b.Insert(idx++, new CommandBlock(BlockType.UnknownBlock14, unknownBlock14Experimental1));
                }
                if (doBlock15)//block 15 has 2 experimentals
                {
                    b.Insert(idx++, new CommandBlock(BlockType.UnknownBlock15, unknownBlock15Experimental1, unknownBlock15Experimental2));
                }
                if (doBlock16)
                {
                    b.Insert(idx++, new CommandBlock(BlockType.UnknownBlock16, unknownBlock16Experimental1));
                }
                if (doBlock17)
                {
                    b.Insert(idx++, new CommandBlock(BlockType.UnknownBlock17, unknownBlock17Experimental1));
                }

                if (doPausePriorToStart)//block 18
                {
                    b.Insert(idx++, new CommandBlock(BlockType.PausePriorToStart, pausePriorToStartExperimental1, pausePriorToStartExperimental2, pausePriorToStartExperimental3));
                }

                if (doBlock15)//block 15 has 2 experimentals
                    //this is not an error because this block is supposed to appear twice (I think...)
                {
                    b.Insert(idx++, new CommandBlock(BlockType.UnknownBlock15, unknownBlock15Experimental1, unknownBlock15Experimental2));
                }
                
             //   b.Add(new CommandBlock(BlockType.AdjustSpeedAndIncline, 0, 0, 0, 0, 0, 0));
                    idx++; //skip 0th x5d block
                if (doSetInitialSpeedAndIncline)//block 19
                {
                    //use 0 for inital speed and incline for now, and then reset later
                    b.Insert(idx++, new CommandBlock(BlockType.SetInitialSpeedAndIncline, 0, 0));

                }

                //add some 0x5d blocks
             /*   for (int ii = 1; ii < workoutLength; ii++)
                {
                    b.Add(new CommandBlock(BlockType.AdjustSpeedAndIncline, ii, 0, 0, 0, 0, 0));



                }*/
                
                //add one to set the speed and incline back to 0, especially the incline must be
                //at 0 in order for safely storing the treadmill in folded position
               // b.Add(new CommandBlock(BlockType.AdjustSpeedAndIncline, workoutLength, 0, 0, 0, 0, 0));
                b.Add(new CommandBlock(BlockType.EndProgram));

            }

        }

        private void adjustHeaderBlocks()
        {
           // return;
            //this will be called just prior to generating output files (or text dump files)
            //it will insert any missing headers
            //it will delete any that shouldn't be there (such as when user elects not to include one)
            //it will adjust the header block bytes where appropriate (such as in updating command count block)
            int wIdxBak = wIdx;
         
            for (int ww = 0; ww < numWorkouts /*17*/; ww++)
            {
                wIdx = ww;
                setWindowTitle();
                BinaryFit bF = lF.bFs[wIdx];
                int index;

                /* 4 cases for each scenario
                 * 
                 * 1) exists, but user doesn't want it
                 * 2) exists and user wants it, but might have changed experimental
                 * 3) doesn't exist, but user wants it
                 * 4) doesn't exist and user doesn't want it
                 * 
                 * We'll ignore case 4 since there is no need to do anything for that case
                 * We handle case 2 and case 3 in these blocks of code here by first testing
                 * to see if the block already exists and removing it if so (could be out of place
                 * or could have currently invalid experimental/other values) and re-insert a new
                 * block in its place at the correct location relative to the other header blocks.
                 * We handle case 1 (needs removing) at the bottom of these blocks of code
                 * If doesn't exist we always add so we'll know where to add later ones, and then
                 * strip off unwanted ones at the end.
                 **/
                
                //block 1 (index=0)
                
                index = bF.IndexOfNthBlockType(BlockType.CommandCount,0);
                //MessageBox.Show("index of CommandCount block is: " + index.ToString());
                if (index == -1)
                {
                    bF.Insert(0, new CommandBlock(BlockType.CommandCount,0));
                }
                bF.AdjustCommandCountBlock(); //set the block count to current
                                            //this will have to be called again later anyway

                //block 02 (index=1)
                index = bF.IndexOfNthBlockType(BlockType.UnknownBlock02,0);
                if (index != -1)
                {
                    bF.RemoveAt(index);
                }
                bF.Insert(1, new CommandBlock(BlockType.UnknownBlock02, unknownBlock02Experimental1));
                 

                //Block 03 (index =2)
                index = bF.IndexOfNthBlockType(BlockType.ShowProgressGraphics, 0);
                if (index != -1)
                {
                    bF.RemoveAt(index);
                }
                bF.Insert(2, new CommandBlock(BlockType.ShowProgressGraphics, showProgressGraphicsExperimental1));
      
                //Block 04 (index=3)
                index = bF.IndexOfNthBlockType(BlockType.ShowInitialTime, 0);
                if (index != -1)
                {
                    bF.RemoveAt(index);
                }
                bF.Insert(3, new CommandBlock(BlockType.ShowInitialTime,workoutLength));
               
                
                //Block 05 (index=4)
                index = bF.IndexOfNthBlockType(BlockType.GenerateBarGraph, 0);
                if (index != -1)
                {
                    bF.RemoveAt(index);
                }
                bF.Insert(4, new CommandBlock(BlockType.GenerateBarGraph));

                //Block 06 (index=5)
                index = bF.IndexOfNthBlockType(BlockType.MaxRunTime, 0);
                if (index != -1)
                {
                    bF.RemoveAt(index);
                }
                bF.Insert(5, new CommandBlock(BlockType.MaxRunTime, workoutLength * 2, maxRunTimeExperimental1));

                //Block 07 (index=6)
                index = bF.IndexOfNthBlockType(BlockType.ShowInitialSpeed, 0);
                if (index != -1)
                {
                    bF.RemoveAt(index);
                }

               //note: the experimental here is used for the metric speed
                double speed = bF.GetSpeedInFastestBlock();
                CommandBlock showInitSpeedBlock = new CommandBlock(BlockType.ShowInitialSpeed, (int)speed * 10, showInitialSpeedExperimental1);
            //    showInitSpeedBlock.IsMetricMode = this.useMetricMode;
                showInitSpeedBlock.CurrentSpeedMetric = speed * 1.609344;
                showInitSpeedBlock.Invalidate();
                bF.Insert(6, showInitSpeedBlock);

                //Block 08 (index=7)
                index = bF.IndexOfNthBlockType(BlockType.UnknownBlock08, 0);
                if (index != -1)
                {
                    bF.RemoveAt(index);
                }
                bF.Insert(7, new CommandBlock(BlockType.UnknownBlock08, unknownBlock08Experimental1, unknownBlock08Experimental2));

                //Block 09 (index=8)
                index = bF.IndexOfNthBlockType(BlockType.ShowInitialIncline, 0);
                if (index != -1)
                {
                    bF.RemoveAt(index);
                }
                double grade = bF.GetInclineInSteepestBlock();
                bF.Insert(8, new CommandBlock(BlockType.ShowInitialIncline, (int) grade*10));

                //Block 10 (index=9)
                index = bF.IndexOfNthBlockType(BlockType.UnknownBlock10, 0);
                if (index != -1)
                {
                    bF.RemoveAt(index);
                }
                bF.Insert(9, new CommandBlock(BlockType.UnknownBlock10, unknownBlock10Experimental1));

                //Block 11 (index=10)
                index = bF.IndexOfNthBlockType(BlockType.UnknownBlock11, 0);
                if (index != -1)
                {
                    bF.RemoveAt(index);
                }
                bF.Insert(10, new CommandBlock(BlockType.UnknownBlock11, unknownBlock11Experimental1));

                //Block 12 (index=11)
                index = bF.IndexOfNthBlockType(BlockType.UnknownBlock12, 0);
                if (index != -1)
                {
                    bF.RemoveAt(index);
                }
                bF.Insert(11, new CommandBlock(BlockType.UnknownBlock12, unknownBlock12Experimental1));

                //Block 13 (index=12)
                index = bF.IndexOfNthBlockType(BlockType.UnknownBlock13, 0);
                if (index != -1)
                {
                    bF.RemoveAt(index);
                }
                bF.Insert(12, new CommandBlock(BlockType.UnknownBlock13, unknownBlock13Experimental1));

                //Block 14 (index=13)
                index = bF.IndexOfNthBlockType(BlockType.UnknownBlock14, 0);
                if (index != -1)
                {
                    bF.RemoveAt(index);
                }
                bF.Insert(13, new CommandBlock(BlockType.UnknownBlock14, unknownBlock14Experimental1));

                //Block 15 (index = 14);
                //There should be 2 of these blocks in the header
                //This is for the first of them
                //The other will come later.
                index = bF.IndexOfNthBlockType(BlockType.UnknownBlock15, 0);
                if (index != -1)
                {
                    bF.RemoveAt(index);
                }
                //Note: exp1 is standard mph
                //exp2 is used for metric mode
                CommandBlock unkBlock15 = new CommandBlock(BlockType.UnknownBlock15, unknownBlock15Experimental1, unknownBlock15Experimental2);
             //   unkBlock15.IsMetricMode = this.useMetricMode;
                unkBlock15.Invalidate();
                bF.Insert(14, unkBlock15);

                //Block 16 (index=15)
                index = bF.IndexOfNthBlockType(BlockType.UnknownBlock16, 0);
                if (index != -1)
                {
                    bF.RemoveAt(index);
                }
                bF.Insert(15, new CommandBlock(BlockType.UnknownBlock16, unknownBlock16Experimental1));

                //Block 17 (index=16)
                index = bF.IndexOfNthBlockType(BlockType.UnknownBlock17, 0);
                if (index != -1)
                {
                    bF.RemoveAt(index);
                }
                bF.Insert(16, new CommandBlock(BlockType.UnknownBlock17, unknownBlock17Experimental1));

                //Block 18 (index=17+numFetchedWaveBlocks)
                // Block 18+numFetchedWaveBlocks will contain a number of 0x5a and 0x5b
                //blocks, depending on whether the user elected to include wave files prior
                //to the start of the workout

                //Block 18 is actually the PausePriorToStart block, but there will possibly be a few
                //0x5a (fetched wave files) in here, which is okay since we can just count them and
                //skip them
                int numFetchedWaveBlocks = 0;
                index = bF.IndexOfNthBlockType(BlockType.FetchWaveFile, 0);
                if (index == 17) //we have an 0x5A block here
                {
                    numFetchedWaveBlocks++;
                }
                else
                {
                    goto DoneLookingForFetchedWaveBlocks;
                    //wtf?  The infamous goto?!?!  Yup. ;)
                    //could have used a simple for loop, but why waste the chance to
                    //insult the sensibilities of a few purists?
                }
                index = bF.IndexOfNthBlockType(BlockType.PlayFetchedWaveFile, 0);
                if (index == 18) //we have an 0x5B block here
                {
                    numFetchedWaveBlocks++;
                }
               
                index = bF.IndexOfNthBlockType(BlockType.FetchWaveFile, 1); 
                if (index == 19) //we have an 0x5A block here
                {
                    numFetchedWaveBlocks++;
                }
                else
                {
                    goto DoneLookingForFetchedWaveBlocks;
                }
                index = bF.IndexOfNthBlockType(BlockType.PlayFetchedWaveFile, 1);
                if (index == 20) //we have an 0x5B block here
                {
                    numFetchedWaveBlocks++;
                }

                index = bF.IndexOfNthBlockType(BlockType.FetchWaveFile, 2);
                if (index == 21) //we have an 0x5A block here
                {
                    numFetchedWaveBlocks++;
                }
                else
                {
                    goto DoneLookingForFetchedWaveBlocks;
                }
                index = bF.IndexOfNthBlockType(BlockType.PlayFetchedWaveFile, 2);
                if (index == 22) //we have an 0x5B block here
                {
                    numFetchedWaveBlocks++;
                }

                index = bF.IndexOfNthBlockType(BlockType.FetchWaveFile, 3);
                if (index == 23) //we have an 0x5A block here
                {
                    numFetchedWaveBlocks++;
                }
                else
                {
                    goto DoneLookingForFetchedWaveBlocks;
                }
                index = bF.IndexOfNthBlockType(BlockType.PlayFetchedWaveFile, 3);
                if (index == 24) //we have an 0x5B block here
                {
                    numFetchedWaveBlocks++;
                }

                index = bF.IndexOfNthBlockType(BlockType.FetchWaveFile, 4);
                if (index == 25) //we have an 0x5A block here
                {
                    numFetchedWaveBlocks++;
                }
                else
                {
                    goto DoneLookingForFetchedWaveBlocks;
                }
                index = bF.IndexOfNthBlockType(BlockType.PlayFetchedWaveFile, 4);
                if (index == 26) //we have an 0x5B block here
                {
                    numFetchedWaveBlocks++;
                }


            DoneLookingForFetchedWaveBlocks:
                
                index = bF.IndexOfNthBlockType(BlockType.PausePriorToStart,0);
                if (index != -1)
                {
                    //index should be 17 + numFetchedWaveBlocks
                    if (index != 17 + numFetchedWaveBlocks)
                    {
                        MessageBox.Show("Program logical error.  First PausePriorToStart Block is not indexed at" + (17 + numFetchedWaveBlocks).ToString(), "adjustHeaderBlocks()");
                    }
                    
                    bF.RemoveAt(index);
                }
                bF.Insert(17 + numFetchedWaveBlocks, new CommandBlock(BlockType.PausePriorToStart, pausePriorToStartExperimental1, pausePriorToStartExperimental2, pausePriorToStartExperimental3));
                
                //Block 19 (index=18+numFetchedWaveBlocks)
                //Here is where our 2nd UnknownBlock15 block goes
                index = bF.IndexOfNthBlockType(BlockType.UnknownBlock15, 1);//note the 1 param, indicates the 2nd (0-indexed) appearance of this block type
                if (index != -1)
                {
                    if (index != 18 + numFetchedWaveBlocks)
                    {
                        MessageBox.Show("program logic error: 2nd unknownblock15 not at expected index", "adjustHeaderBytes()");
                    }
                    bF.RemoveAt(18 + numFetchedWaveBlocks);
                }

                CommandBlock unkBlock15v2 = new CommandBlock(BlockType.UnknownBlock15, unknownBlock15Experimental1, unknownBlock15Experimental2);
            //    unkBlock15v2.IsMetricMode = this.useMetricMode;
                unkBlock15v2.Invalidate();
                bF.Insert(18 + numFetchedWaveBlocks, unkBlock15v2);
 
                //Block 20 (index=19+numFetchedWaveBlocks) will be the first 0x5d block
                index = bF.IndexOfNth0x5dBlock(0);
                if (index != 19 + numFetchedWaveBlocks)
                {
                    MessageBox.Show("logical program error, first 0x5d block not in expected location", "adjustHeaderBlocks()");

                }

                CommandBlock x5d = bF.GetNth0x5dBlock(0);
                double initSpeed = x5d.CurrentSpeed;
                double initIncline = x5d.CurrentIncline;
                //Block21 (index=20+numFetchedWaveFiles)
                //This is the SetInitialSpeedAndIncline Block

                /* The sequence should be:
                 * PausePriorToStart
                 * (2nd)UnknownBlock15
                 * (1st)0x5d block
                 * SetInitialSpeedAndIncline
                */

                index = bF.IndexOfNthBlockType(BlockType.SetInitialSpeedAndIncline, 0);
                if (index!=-1 && index != 20 + numFetchedWaveBlocks)
                {
                    MessageBox.Show("Program Logic error: SetInitialSpeedAndIncline Block is not in expected location.", "adjustHeaderBlocks()");
                }

                int incParam = (int)(initIncline * 10);
                if (incParam > 120)
                {
                    incParam = (int)(96 + 2 * initIncline);
                }
                
                CommandBlock setInitSpeedAndInclineBlock =new CommandBlock(BlockType.SetInitialSpeedAndIncline, (int)initSpeed * 10, incParam);
              //  setInitSpeedAndInclineBlock.IsMetricMode = this.useMetricMode;
                setInitSpeedAndInclineBlock.CurrentSpeedMetric = initSpeed * 1.609344;
                setInitSpeedAndInclineBlock.Invalidate();

                if (index != -1)
                {
                    bF.RemoveAt(index);
                    bF.Insert(index, setInitSpeedAndInclineBlock );
                }
                else //never did exist
                {
                    bF.Insert(20+numFetchedWaveBlocks, setInitSpeedAndInclineBlock);
                }

                //should be a final 0x5d block to set the speed and incline both to 0
                if (bF.CountOf0x5dBlocks() < workoutLength)
                {
                    CommandBlock last = new CommandBlock(BlockType.AdjustSpeedAndIncline, workoutLength, 0, 0, 0);
                    last.CurrentSpeed = 0;
                    last.CurrentIncline = 0;
                    last.CurrentTimeStamp = workoutLength;
                    bF.Add(last);
                }
                if (bF.IndexOfNthBlockType(BlockType.EndProgram, 0) != bF.Count - 1)
                {
                    bF.Add(new CommandBlock(BlockType.EndProgram));
                }
                
                
                
                //now strip unwanted blocks
                //they are guaranteed to be here since we added them above

                if (!doPausePriorToStart)
                {
                    bF.RemoveAt(bF.IndexOfNthBlockType(BlockType.PausePriorToStart, 0));
                }
                
                if (!doBlock17)
                {
                    bF.RemoveAt(16);
                }
                if (!doBlock16)
                {
                    bF.RemoveAt(15);
                }
                if (!doBlock15)
                {
                    bF.RemoveAt(14);
                }
                if (!doBlock14)
                {
                    bF.RemoveAt(13);
                }
                if (!doBlock13)
                {
                    bF.RemoveAt(12);
                }
                if (!doBlock12)
                {
                    bF.RemoveAt(11);
                }
                if (!doBlock11)
                {
                    bF.RemoveAt(10);
                } 
                if (!doBlock10)
                {
                    bF.RemoveAt(9);
                }
                if (!doShowInitialIncline)
                {
                    bF.RemoveAt(8);
                }
                if (!doBlock08)
                {
                    bF.RemoveAt(7);
                }
                if (!doShowInitialSpeed)
                {
                    bF.RemoveAt(6);
                }
                if (!doMaxRunTime)
                {
                    bF.RemoveAt(5);
                }
                if (!doGenerateBarGraph)
                {
                    bF.RemoveAt(4);
                }
                if (!doShowInitialTime)
                {
                    bF.RemoveAt(3);
                }
                if (!doShowProgressGraphBlock)
                {
                    bF.RemoveAt(2);
                }
                if (!doBlock02)
                {
                    bF.RemoveAt(1);
                }

                doAutoWarmupCooldown();
             
            
            
            }//end of for(ww)

            wIdx = wIdxBak;
            if (bAutoTTWsAtEachSpeedAndInclineAdjustment)
            {
                insertTTWsForAllSpeedAndInclineAdjustments();
            }

            if (bAutoTTWsAtStartOfWorkouts)
            {
                insertTTWsAtStartOfWorkouts();
            }
            setWindowTitle();
        }




        private int parseIntSetting(string name, string full)
        {
            int val;
            string str = full;
            if (!full.StartsWith(name))
            {
                MessageBox.Show("Error reading application settings:" + full + " does not start with " + name);
            }
            str = full.Remove(0, name.Length);
            val = System.Convert.ToInt32(str);
            return val;
        }

        private bool parseBoolSetting(string name, string full)
        {
            bool val;
            string str = full;
            if (!full.StartsWith(name))
            {
                MessageBox.Show("Error reading application settings:" + full + " does not start with " + name);
            }
            str = full.Remove(0, name.Length);
            val = System.Convert.ToBoolean(str);
            return val;
        }

        private string parseStringSetting(string name, string full)
        {
            string val;
            string str = full;
            if (!full.StartsWith(name))
            {
                MessageBox.Show("Error reading application settings:" + full + " does not start with " + name);
            }
            str = full.Remove(0, name.Length);
            val = str;
            return val;


        }

        public void LoadApplicationSettings()
        {
            System.IO.StreamReader objFile = null;
            try
            {
               // appSettingsPathName = System.AppDomain.CurrentDomain.BaseDirectory + @"appsettings.ini";
               // objFile = new System.IO.StreamReader(appSettingsPathName);
                objFile = new System.IO.StreamReader(pathToAppSettingsIni);
                   //read in our application settings
                objFile.ReadLine();//toss out first line file description
                workoutLength = parseIntSetting("workoutLength=",objFile.ReadLine());//default 30
                unknownBlock02Experimental1=parseIntSetting("unknownBlock02Experimental1=", objFile.ReadLine());
                unknownBlock08Experimental1 = parseIntSetting("unknownBlock08Experimental1=", objFile.ReadLine());
                unknownBlock08Experimental2 = parseIntSetting("unknownBlock08Experimental2=", objFile.ReadLine());
                unknownBlock10Experimental1 = parseIntSetting("unknownBlock10Experimental1=", objFile.ReadLine());
                unknownBlock11Experimental1 = parseIntSetting("unknownBlock11Experimental1=", objFile.ReadLine());
                unknownBlock12Experimental1 = parseIntSetting("unknownBlock12Experimental1=", objFile.ReadLine());
                unknownBlock13Experimental1 = parseIntSetting("unknownBlock13Experimental1=", objFile.ReadLine());
                unknownBlock14Experimental1 = parseIntSetting("unknownBlock14Experimental1=", objFile.ReadLine());
                unknownBlock15Experimental1 = parseIntSetting("unknownBlock15Experimental1=", objFile.ReadLine());
                unknownBlock15Experimental2 = parseIntSetting("unknownBlock15Experimental2=", objFile.ReadLine());
                unknownBlock16Experimental1 = parseIntSetting("unknownBlock16Experimental1=", objFile.ReadLine());
                unknownBlock17Experimental1 = parseIntSetting("unknownBlock17Experimental1=", objFile.ReadLine());
                maxRunTimeExperimental1 = parseIntSetting("maxRunTimeExperimental1=", objFile.ReadLine());
                showProgressGraphicsExperimental1=parseIntSetting("showProgressGraphicsExperimental1=",objFile.ReadLine());
                showInitialSpeedExperimental1=parseIntSetting("showInitialSpeedExperimental1=",objFile.ReadLine());
                pausePriorToStartExperimental1=parseIntSetting("pausePriorToStartExperimental1=",objFile.ReadLine());
                pausePriorToStartExperimental2=parseIntSetting("pausePriorToStartExperimental2=",objFile.ReadLine());
                pausePriorToStartExperimental3=parseIntSetting("pausePriorToStartExperimental3=",objFile.ReadLine());

                doBlock02=parseBoolSetting("doBlock02=",objFile.ReadLine());
                doBlock08 = parseBoolSetting("doBlock08=", objFile.ReadLine());
                doBlock10 = parseBoolSetting("doBlock10=", objFile.ReadLine());
                doBlock11 = parseBoolSetting("doBlock11=", objFile.ReadLine());
                doBlock12 = parseBoolSetting("doBlock12=", objFile.ReadLine());
                doBlock13 = parseBoolSetting("doBlock13=", objFile.ReadLine());
                doBlock14 = parseBoolSetting("doBlock14=", objFile.ReadLine());
                doBlock15 = parseBoolSetting("doBlock15=", objFile.ReadLine());
                doBlock16 = parseBoolSetting("doBlock16=", objFile.ReadLine());
                doBlock17 = parseBoolSetting("doBlock17=", objFile.ReadLine());
                doMaxRunTime = parseBoolSetting("doMaxRunTime=", objFile.ReadLine());
                doPausePriorToStart = parseBoolSetting("doPausePriorToStart=", objFile.ReadLine());
                doShowInitialIncline = parseBoolSetting("doShowInitialIncline=", objFile.ReadLine());
                doShowInitialSpeed = parseBoolSetting("doShowInitialSpeed=", objFile.ReadLine());
                doShowProgressGraphBlock = parseBoolSetting("doShowProgressGraphBlock=", objFile.ReadLine());

                outputDirectory = parseStringSetting("outputDirectory=", objFile.ReadLine());
                userWeight = parseIntSetting("userWeight=", objFile.ReadLine());
          //      useMetricMode = parseBoolSetting("useMetricMode=", objFile.ReadLine());
                bViewHorizontalSliders = parseBoolSetting("viewHorizontalSliders=", objFile.ReadLine());
                bAutoWarmupCooldown = parseBoolSetting("autoWarmupCooldown=", objFile.ReadLine());
                bAutoTTWsAtStartOfWorkouts = parseBoolSetting("autoTTWsAtStartOfWorkouts=", objFile.ReadLine());
                bAutoTTWsAtEachSpeedAndInclineAdjustment = parseBoolSetting("autoTTWsAtEachSpeedAndInclineAdjustment=", objFile.ReadLine());
           //     bOverwriteWaveFilesInOutputDirectory = parseBoolSetting("overwriteAllWaveFilesInOutputDirectory=", objFile.ReadLine());
                maxSpeed = parseIntSetting("maxSpeed=", objFile.ReadLine());
                maxIncline = parseIntSetting("maxIncline=", objFile.ReadLine());
                bAutoCreateBikeFiles = parseBoolSetting("autoCreateBikeFiles=", objFile.ReadLine());
                bAutoCreateEllipticalFiles = parseBoolSetting("autoCreateEllipticalFiles=", objFile.ReadLine());
                bAutoCreateInclineFiles = parseBoolSetting("autoCreateInclineFiles=", objFile.ReadLine());
                
               


                //setWindowTitle();
                            

                
                
            }
            catch (Exception e)
            {
                if (e != null)
                {
                    //suppress warning about unused variable
                }
                //   MessageBox.Show(e.ToString());
             //   MessageBox.Show("Using application defaults");
 
            }
            finally
            {
                if (objFile != null)
                {
                    objFile.Close();
                    objFile.Dispose();
                }
            }





        }

        public void SaveApplicationSettings()
        {
            System.IO.StreamWriter objFile = null;
            try
            {
               // appSettingsPathName = System.AppDomain.CurrentDomain.BaseDirectory + @"appsettings.ini";
               // objFile = new System.IO.StreamWriter(appSettingsPathName);
                
                objFile = new System.IO.StreamWriter(pathToAppSettingsIni);
                objFile.WriteLine("WorkoutGenSD Application Settings");
                objFile.WriteLine("workoutLength={0}",workoutLength.ToString());
                objFile.WriteLine("unknownBlock02Experimental1={0}", unknownBlock02Experimental1.ToString());
                objFile.WriteLine("unknownBlock08Experimental1={0}", unknownBlock08Experimental1.ToString());
                objFile.WriteLine("unknownBlock08Experimental2={0}", unknownBlock08Experimental2.ToString());
                objFile.WriteLine("unknownBlock10Experimental1={0}", unknownBlock10Experimental1.ToString());
                objFile.WriteLine("unknownBlock11Experimental1={0}", unknownBlock11Experimental1.ToString());
                objFile.WriteLine("unknownBlock12Experimental1={0}", unknownBlock12Experimental1.ToString());
                objFile.WriteLine("unknownBlock13Experimental1={0}", unknownBlock13Experimental1.ToString());
                objFile.WriteLine("unknownBlock14Experimental1={0}", unknownBlock14Experimental1.ToString());
                objFile.WriteLine("unknownBlock15Experimental1={0}", unknownBlock15Experimental1.ToString());
                objFile.WriteLine("unknownBlock15Experimental2={0}", unknownBlock15Experimental2.ToString());
                objFile.WriteLine("unknownBlock16Experimental1={0}", unknownBlock16Experimental1.ToString());
                objFile.WriteLine("unknownBlock17Experimental1={0}", unknownBlock17Experimental1.ToString());
                objFile.WriteLine("maxRunTimeExperimental1={0}", maxRunTimeExperimental1.ToString());
                objFile.WriteLine("showProgressGraphicsExperimental1={0}", showProgressGraphicsExperimental1.ToString());
                objFile.WriteLine("showInitialSpeedExperimental1={0}", showInitialSpeedExperimental1.ToString());
                objFile.WriteLine("pausePriorToStartExperimental1={0}", pausePriorToStartExperimental1.ToString());
                objFile.WriteLine("pausePriorToStartExperimental2={0}", pausePriorToStartExperimental2.ToString());
                objFile.WriteLine("pausePriorToStartExperimental3={0}", pausePriorToStartExperimental3.ToString());

                objFile.WriteLine("doBlock02={0}", doBlock02.ToString());
                objFile.WriteLine("doBlock08={0}", doBlock08.ToString());
                objFile.WriteLine("doBlock10={0}", doBlock10.ToString());
                objFile.WriteLine("doBlock11={0}", doBlock11.ToString());
                objFile.WriteLine("doBlock12={0}", doBlock12.ToString());
                objFile.WriteLine("doBlock13={0}", doBlock13.ToString());
                objFile.WriteLine("doBlock14={0}", doBlock14.ToString());
                objFile.WriteLine("doBlock15={0}", doBlock15.ToString());
                objFile.WriteLine("doBlock16={0}", doBlock16.ToString());
                objFile.WriteLine("doBlock17={0}", doBlock17.ToString());

                objFile.WriteLine("doMaxRunTime={0}", doMaxRunTime.ToString());
                objFile.WriteLine("doPausePriorToStart={0}", doPausePriorToStart.ToString());
                objFile.WriteLine("doShowInitialIncline={0}", doShowInitialIncline.ToString());
                objFile.WriteLine("doShowInitialSpeed={0}", doShowInitialSpeed.ToString());
                objFile.WriteLine("doShowProgressGraphBlock={0}", doShowProgressGraphBlock.ToString());

                objFile.WriteLine("outputDirectory={0}", outputDirectory.ToString());
                objFile.WriteLine("userWeight={0}", userWeight.ToString());
             //   objFile.WriteLine("useMetricMode={0}", useMetricMode.ToString());
                objFile.WriteLine("viewHorizontalSliders={0}", bViewHorizontalSliders.ToString());
                objFile.WriteLine("autoWarmupCooldown={0}", bAutoWarmupCooldown.ToString());
                objFile.WriteLine("autoTTWsAtStartOfWorkouts={0}", bAutoTTWsAtStartOfWorkouts.ToString());
                objFile.WriteLine("autoTTWsAtEachSpeedAndInclineAdjustment={0}", bAutoTTWsAtEachSpeedAndInclineAdjustment.ToString());
            //    objFile.WriteLine("overwriteAllWaveFilesInOutputDirectory={0}", bOverwriteWaveFilesInOutputDirectory.ToString());
                objFile.WriteLine("maxSpeed={0}", maxSpeed);
                objFile.WriteLine("maxIncline={0}", maxIncline);
                objFile.WriteLine("autoCreateBikeFiles={0}", bAutoCreateBikeFiles.ToString());
                objFile.WriteLine("autoCreateEllipticalFiles={0}", bAutoCreateEllipticalFiles.ToString());
                objFile.WriteLine("autoCreateInclineFiles={0}", bAutoCreateInclineFiles.ToString());

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                if (objFile != null)
                {
                    objFile.Close();
                    objFile.Dispose();
                }
            }


        }


        public void init()
        {

            LoadApplicationSettings();



            lF = new LayoutFit(numWorkouts, workoutLength); //creates numWorkouts(26?) BinaryFits and SoundFits

         
            wIdx = 0;
            setWindowTitle();
            GenerateInitialBlocks(); //fill the generated workouts with some initial blocks
            buildGUI();
            BinaryFit bF = lF.bFs[wIdx];
            currentSmallSpeedSlider = bF.GetNth0x5dBlock(0).SpeedSlider;
            currentSmallSpeedSlider.Focus();



        }
        
        
        public Window1()
        {
            InitializeComponent();

            userWeightLabel.MouseDoubleClick += new MouseButtonEventHandler(userWeightLabel_MouseDoubleClick);
            outputDirectory = Directory.GetCurrentDirectory();

            if(!Directory.Exists(pathToMyDocumentsFolder+"\\WorkoutGenSD"))
            {
                Directory.CreateDirectory(pathToMyDocumentsFolder + "\\WorkoutGenSD");
            }
            pathToAppSettingsIni = pathToMyDocumentsFolder + "\\WorkoutGenSD\\appsettings.ini";
            pathToTTWFolder = pathToMyDocumentsFolder + "\\WorkoutGenSD\\TTWs\\";
            

            _backgroundWorker_GenFiles = new BackgroundWorker();
            _backgroundWorker_InsertTTWs = new BackgroundWorker();

            _backgroundWorker_GenFiles.ProgressChanged += new ProgressChangedEventHandler(_backgroundWorker_GenFiles_ProgressChanged);
            _backgroundWorker_InsertTTWs.ProgressChanged += new ProgressChangedEventHandler(_backgroundWorker_InsertTTWs_ProgressChanged);

            _backgroundWorker_GenFiles.DoWork += _backgroundWorker_GenFiles_DoWork;
            _backgroundWorker_InsertTTWs.DoWork += new DoWorkEventHandler(_backgroundWorker_InsertTTWs_DoWork);

            _backgroundWorker_GenFiles.RunWorkerCompleted +=
                _backgroundWorker_GenFiles_RunWorkerCompleted;
            _backgroundWorker_InsertTTWs.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_backgroundWorker_InsertTTWs_RunWorkerCompleted);

            _backgroundWorker_GenFiles.WorkerReportsProgress = true;
            _backgroundWorker_InsertTTWs.WorkerReportsProgress = true;

            saveAsZipArchiveMenuItem.Click += new RoutedEventHandler(saveAsZipArchiveMenuItem_Click);
            openFromZipArchiveMenuItem.Click += new RoutedEventHandler(openFromZipArchiveMenuItem_Click);
            autoWarmupCooldownMenuItem.Click += new RoutedEventHandler(autoWarmupCooldownMenuItem_Click);

            autoCreateBikeFilesMenuItem.Click += new RoutedEventHandler(autoCreateBikeFilesMenuItem_Click);
            autoCreateEllipticalFilesMenuItem.Click += new RoutedEventHandler(autoCreateEllipticalFilesMenuItem_Click);
            autoCreateInclineFilesMenuItem.Click += new RoutedEventHandler(autoCreateInclineFilesMenuItem_Click);


            listBox1.BorderBrush = autoWarmupCooldownButton.BorderBrush;

            viewHorizontalSlidersMenuItem.IsChecked = bViewHorizontalSliders;
            viewHorizontalSlidersMenuItem.Click += new RoutedEventHandler(viewHorizontalSlidersMenuItem_Click);

            autoInsertTTWs.Click += new RoutedEventHandler(autoInsertTTWs_Click);
            autoInsertTTWsForAllSpeedAndInclineAdjustments.IsCheckable = true;

        
           


            init();
            disableAllButtons();
            //mp3Capture = new Mp3SoundCapture();

            playWaveFileButton.ToolTip = "Play currently selected wav file.";
    
            stopButton.ToolTip = "Stop recording.";
      
            recordButton.ToolTip = "Record a new file and add it here.";
     
            reRecordButton.ToolTip = "Re-record currently selected file.\n  "
                + "(Replaces file's contents with new recording made using \n"
                + "system default recording device, e.g. microphone or line-in.)";
   
            fromDiskButton.ToolTip = "Select wave file from disk and add it here.";
 
            textToWaveButton.ToolTip = "Create a new text-to-speech wave file and add it here.";
            listBox1.SelectionChanged += new SelectionChangedEventHandler(listBox1_SelectionChanged);
            listBox1.LostFocus += new RoutedEventHandler(listBox1_LostFocus);

            mainWindow.Loaded += new RoutedEventHandler(mainWindow_Loaded);
            autoWarmupCooldownButton.Click += new RoutedEventHandler(autoWarmupCooldownButton_Click);
            autoInsertTTWsForAllSpeedAndInclineAdjustments.Click += new RoutedEventHandler(autoInsertTTWsForAllSpeedAndInclineAdjustments_Click);
            newLayoutMenuItem.Click += new RoutedEventHandler(newLayoutMenuItem_Click);
            clearAllExceptActiveWorkoutMenuItem.Click += new RoutedEventHandler(clearAllExceptActiveWorkoutMenuItem_Click);
            stripAllWavesFromCurrentWorkoutMenuItem.Click += new RoutedEventHandler(stripAllWavesFromCurrentWorkoutMenuItem_Click);
            stripAllWavesMenuItem.Click += new RoutedEventHandler(stripAllWavesMenuItem_Click);
            helpUsingMenuItem.Click += new RoutedEventHandler(helpUsingMenuItem_Click);
        }

        void autoCreateInclineFilesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem m = (MenuItem)sender;
            bAutoCreateInclineFiles = !bAutoCreateInclineFiles;
            setWindowTitle();
        }

        void autoCreateEllipticalFilesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem m = (MenuItem)sender;
            bAutoCreateEllipticalFiles = !bAutoCreateEllipticalFiles;
            setWindowTitle();
        }

        void autoCreateBikeFilesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem m = (MenuItem)sender;
            bAutoCreateBikeFiles = !bAutoCreateBikeFiles;
            setWindowTitle();
            
        }

        void helpUsingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://mwganson.freeyellow.com/workoutgensd/using.html");
        }

        void stripAllWavesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            for (int ii = 0; ii < numWorkouts; ii++)
            {
                stripWaves(ii);
            }
        }

        void stripAllWavesFromCurrentWorkoutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            stripWaves(wIdx);
        }

        private void stripWaves(int wIdxParam)
        {
            BinaryFit bF = lF.bFs[wIdxParam];
            bF.StripWaves();
            lF.sFs[wIdxParam] = new SoundFit(lF.sFs[wIdxParam].Name);
        }

        void clearAllExceptActiveWorkoutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            int wIdxBak = wIdx;
            BinaryFit bF = lF.bFs[wIdx];
            SoundFit sF = lF.sFs[wIdx];
           
            BinaryFit bFSaved = bF.Clone();
            SoundFit sFSaved = sF.Clone(sF.Name);
            init();
            lF.bFs[wIdxBak] = bFSaved.Clone();
            lF.sFs[wIdxBak] = sFSaved.Clone(sFSaved.Name);
            wIdx = wIdxBak;
            buildGUI();
            checkWorkoutMenus(wIdx);
            setWindowTitle();
            updateUILabels();
        }

        void newLayoutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("You have selected to replace the current layout with a new one.  You will lose all of your changes in all of the workouts in this layout.  Continue?",
                "Initialize Layout Confirmation Dialog", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return; //user canceled
            }
            init();
            wIdx = 0;
            checkWorkoutMenus(0);
            setWindowTitle();
            updateUILabels();
        }

    /*    void overwriteWaveFilesInOutputDirectory_Click(object sender, RoutedEventArgs e)
        {
            bOverwriteWaveFilesInOutputDirectory = !bOverwriteWaveFilesInOutputDirectory;
            setWindowTitle();
        }*/

        void _backgroundWorker_InsertTTWs_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("Insert TTW Operation Cancelled");
                
            }
            else if (e.Error != null)
            {
                MessageBox.Show(e.ToString(), "Insert TTW Exception");
              
            }
            else
            {
            //    MessageBox.Show("Finished inserting ttw files.");
                
                bTTWInsertionInProgress = false;
                ttwStatusWin.bFileGenerationInProgress = false;
               
                ttwStatusWin.Close();
                                               

            }
        }

        void _backgroundWorker_InsertTTWs_DoWork(object sender, DoWorkEventArgs e)
        {
             //We'll do this at :53 seconds into the previous interval
            //1st: parse through all the intervals in all the workouts
            //We put the file only in those intervals where we are changing speed and/or incline
            //The first interval is a special case where we'll timestamp at :01 seconds into the interval
            //2nd: we determine the timestamp (an int containing the number of seconds from the start of the workout)
            //3rd: we configure the string to be spoken into the file
            //4th: we need a unique name for each .wav file
            //5th: create the file and add it to the SoundFit object using SoundFit.GetIndex(string path)
            //6th: create the 0x60 command block
            //7th: insert the 0x60 using BinaryFit.Insert0x60AccordingToTimeStamp(CommandBlock x60)

            
            //_backgroundWorker_InsertTTWs.ReportProgress(0);
            
            for (int ww = 0; ww < numWorkouts; ww++)
            {
                insertTTWStatusString = "Inserting Text-To-Waves into "+lF.bFs[ww].Name;
                if(bAsync){
                 insertTTWStatusString+=" (asynch)";   
                }
                double dWW = ww * 100;
                double dNumWorkouts = numWorkouts;
                _backgroundWorker_InsertTTWs.ReportProgress((int)(dWW/dNumWorkouts));
                
                BinaryFit bF = lF.bFs[ww];
                SoundFit sF = lF.sFs[ww];
              //  CommandBlock prevX5d = null;
                double prevSpeed = -1;
                double prevIncline = -1;


                for (int ii = 0; ii < workoutLength; ii++)
                {
                    string wordsToSpeakSpeed = "";
                    string wordsToSpeakIncline = "";
                    bool bSpeedIncrease = false;
                    bool bSpeedDecrease = false;
                    bool bInclineIncrease = false;
                    bool bInclineDecrease = false;

                    int timeStamp = 0;
                    if (ii == 0)
                    {
                        timeStamp = 01;
                    }
                    else
                    {
                        timeStamp = (ii - 1) * 60 + 53;
                    }

                    //if we have a pre-existing playwavefile block with this timestamp, we remove it first
                    if (bF.IndexOf0x60BlockWithTimeStamp(timeStamp) != -1)
                    {
                        bF.RemoveAt(bF.IndexOf0x60BlockWithTimeStamp(timeStamp));
                    }

                    CommandBlock x5d = bF.GetNth0x5dBlock(ii);
                    double curSpeed = System.Math.Round(x5d.CurrentSpeed, 1);
                    double curIncline = System.Math.Round(x5d.CurrentIncline, 1);
                    if (curSpeed == prevSpeed && curIncline == prevIncline)
                    {
                        continue; //no need for a wave file here since no adjustments were made
                    }

                    if (curSpeed > prevSpeed)
                    {
                        bSpeedIncrease = true;
                    }
                    else if (curSpeed < prevSpeed)
                    {
                        bSpeedDecrease = true;
                    }
                    if (curIncline > prevIncline)
                    {
                        bInclineIncrease = true;
                    }
                    else if (curIncline < prevIncline)
                    {
                        bInclineDecrease = true;
                    }

                    if (ii == 0)
                    {
                        wordsToSpeakSpeed = "Setting initial speed to ";
                        wordsToSpeakSpeed += curSpeed.ToString() + " miles per hour";
                        wordsToSpeakSpeed += "or " + System.Math.Round(curSpeed * 1.60944, 1).ToString() + " kilometers per hour";
                        wordsToSpeakIncline = "Setting initial incline to ";
                        wordsToSpeakIncline += curIncline.ToString() + " percent";
                    }
                    else
                    {
                        if (bSpeedIncrease)
                        {
                            wordsToSpeakSpeed = "Increasing speed to ";
                            wordsToSpeakSpeed += curSpeed.ToString() + "  miles per hour ";
                            wordsToSpeakSpeed += "      or " + System.Math.Round(curSpeed * 1.60944, 1).ToString() + "  kilometers per hour";
                        }
                        else if (bSpeedDecrease)
                        {
                            wordsToSpeakSpeed = "Decreasing speed to ";
                            wordsToSpeakSpeed += curSpeed.ToString() + " miles per hour ";
                            wordsToSpeakSpeed += "    or " + System.Math.Round(curSpeed * 1.60944, 1).ToString() + "  kilometers per hour";
                        }

                        if (bInclineIncrease)
                        {
                            wordsToSpeakIncline = "Increasing incline to ";
                            wordsToSpeakIncline += curIncline.ToString() + " percent";

                        }
                        else if (bInclineDecrease)
                        {
                            wordsToSpeakIncline = "Decreasing incline to ";
                            wordsToSpeakIncline += curIncline.ToString() + " percent";

                        }

                    }



                    string fileName = pathToTTWFolder + "TTW" + String.Format("{0:D2}_{1:D2}.WAV", ww, ii);
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                    int idx = sF.GetIndex(fileName);

                    wordsToSpeakSpeed += ".";
                    wordsToSpeakIncline += ".";
                    makeWaveFile(fileName, wordsToSpeakSpeed, wordsToSpeakIncline);

                    CommandBlock x60 = new CommandBlock(BlockType.PlayWaveFile, timeStamp, idx);
                    string fileBody = fileName.Substring(fileName.LastIndexOf("\\") + 1, 8);
                    x60.CurrentWaveFileName = fileBody;
                    bF.Insert0x60AccordingToTimeStamp(x60);


                    prevSpeed = curSpeed;
                    prevIncline = curIncline;
                }//end of for(ii) -- intervals
            }//end of for(ww) -- workouts
        }   
        
      

        void _backgroundWorker_InsertTTWs_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            mainWindow.InvalidateVisual();
            ttwStatusWin.UpdateStatus(insertTTWStatusString);
            ttwStatusWin.progressBar.Value = e.ProgressPercentage;
            

        }

        void autoInsertTTWsForAllSpeedAndInclineAdjustments_Click(object sender, RoutedEventArgs e)
        {
            bAutoTTWsAtEachSpeedAndInclineAdjustment = !bAutoTTWsAtEachSpeedAndInclineAdjustment;
            setWindowTitle();
        }

 

        private void makeWaveFile(string fileName, string wordsToSpeakSpeed, string wordsToSpeakIncline)
        {
            using (SpeechSynthesizer speaker = new SpeechSynthesizer())
            {


                System.Speech.AudioFormat.SpeechAudioFormatInfo formatInfo = new System.Speech.AudioFormat.SpeechAudioFormatInfo(8000, System.Speech.AudioFormat.AudioBitsPerSample.Eight, System.Speech.AudioFormat.AudioChannel.Mono);
                speaker.SetOutputToWaveFile(fileName, formatInfo);
                speaker.Rate = 1;
                speaker.Volume = 100;

                try
                {


                    if (wordsToSpeakSpeed != "")
                    {
                        if (!bAsync)
                        {
                            speaker.Speak(wordsToSpeakSpeed);
                        }
                        else
                        {
                            speaker.SpeakAsync(wordsToSpeakSpeed);
                        }
                    }
                    if (wordsToSpeakIncline != "")
                    {
                        if (!bAsync)
                        {
                            speaker.Speak(wordsToSpeakIncline);
                        }
                        else
                        {
                            speaker.SpeakAsync(wordsToSpeakIncline);
                        }
                    }

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString()+" while trying to speak: "+wordsToSpeakSpeed.ToString()+" "+wordsToSpeakIncline.ToString()+" to file: "+fileName.ToString(), "Text-to-Wave File Generation Exception");


                }
                
                try
                {
                    speaker.SetOutputToDefaultAudioDevice();
                }
                catch (InvalidOperationException ioe)
                {
                    if (ioe != null) { /*disable compiler warning message*/ }
                }
            }
                //speaker.Dispose();



        }

        void openFromZipArchiveMenuItem_Click(object sender, RoutedEventArgs e)
        {
         

            string pathToZip;
            string safePath;
            string targetDirectory;
            string pathToLayoutFit = "";
            bool bLayoutFitFound = false;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select a .zip archive containing your layout.fit and associated files.";
            ofd.Filter = "Zip Files|*.zip";
            ofd.DefaultExt = "zip";
          
            if (ofd.ShowDialog() == true)
            {
                pathToZip = ofd.FileName;
                safePath = ofd.SafeFileName;
                safePath = safePath.Substring(0, safePath.Length - 4);
                targetDirectory = pathToMyDocumentsFolder + "\\WorkoutGenSD\\" + safePath;
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }
                
                
                
                
                using (Package package =
     Package.Open(pathToZip, FileMode.Open, FileAccess.Read))
                {
                    
                    foreach (PackagePart part in package.GetParts())
                    {
                 

                        if (part.Uri.OriginalString.ToLower().EndsWith("layout.fit"))
                        {
                            bLayoutFitFound = true;
                            pathToLayoutFit = targetDirectory + part.Uri.OriginalString;
                            pathToLayoutFit = pathToLayoutFit.Replace("/", "\\");

                        }
                        extractPart(part, targetDirectory);

                    }


                }// end:using(Package package) - Close & dispose package.


                if (!bLayoutFitFound)
                {
                    MessageBox.Show("Error: no layout.fit file found in this .zip archive.");
                    return;

                }
                else
                {
                    openLayoutFit(pathToLayoutFit);
                }

            }
            else
            {
                return; //user canceled
            }

  

        }

        private void extractPart(PackagePart part, string targetDirectory)
        {
            string destPath = targetDirectory + part.Uri.OriginalString;
            destPath = destPath.Replace("/", "\\");
            int idx = destPath.LastIndexOf("\\");
            string destDirPath = destPath.Substring(0,idx);
            if (!Directory.Exists(destDirPath))
            {
                Directory.CreateDirectory(destDirPath);
            }
            FileStream outStream = new FileStream(destPath, FileMode.Create, FileAccess.Write);
            Stream inStream = part.GetStream();
            copyStream(inStream, outStream);
            inStream.Close();
            outStream.Close();


        }

        private void copyStream(Stream inStream, FileStream outStream)
        {
            const int bufSize = 0x1000;
            byte[] buf = new byte[bufSize];
            int bytesRead = 0;
            while ((bytesRead = inStream.Read(buf, 0, bufSize)) > 0)
            {
                outStream.Write(buf, 0, bytesRead);
            }
        }

        void saveAsZipArchiveMenuItem_Click(object sender, RoutedEventArgs e)
        {
          /*  MessageBox.Show("This feature has not yet been implemented.", "Feature Not Yet Implemented");
            return;
            */
            
            SaveFileDialog dlg = new SaveFileDialog();
            string path = "";
            dlg.DefaultExt = "zip";
            dlg.Filter = "Zip Files|*.zip";
            dlg.Title = "Place Generated Files Into A Zip Archive File";
            if (dlg.ShowDialog() == true)
            {
                path = dlg.FileName;
                bZipping = true;
                zipFile = Package.Open(path, FileMode.Create);
                generateFiles();


            }
            else
            {
                return; //user canceled
            }


        }

        void autoInsertTTWs_Click(object sender, RoutedEventArgs e)
        {
            bAutoTTWsAtStartOfWorkouts = !bAutoTTWsAtStartOfWorkouts;
            setWindowTitle();
        }

        void autoWarmupCooldownMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem m = (MenuItem)sender;
            bAutoWarmupCooldown = !bAutoWarmupCooldown;
            setWindowTitle();
            doAutoWarmupCooldown();
        }

        void doAutoWarmupCooldown()
        {

            if (bAutoWarmupCooldown)
            {
                bAutoWarmupCooldown = false; //disable recursive calls to this method
            }
            else
            {
                return; 
            }
            BinaryFit bF = lF.bFs[wIdx];
            double interval5Speed = bF.GetNth0x5dBlock(5).SpeedSlider.Value;
            double interval5Incline = bF.GetNth0x5dBlock(5).CurrentIncline;
            double interval6thFromEndSpeed = bF.GetNth0x5dBlock(workoutLength - 6).CurrentSpeed;
            double interval6thFromEndIncline = bF.GetNth0x5dBlock(workoutLength - 6).CurrentIncline;
   

            //warmups
            for (int ii = 0; ii < 5; ii++)
            {
                double[] percentages = { .25, .40, .55, .70, .85 };
                double percentage = percentages[ii];
                CommandBlock x5d = bF.GetNth0x5dBlock(ii);

                x5d.SpeedSlider.Value = rangeCheck(x5d, interval5Speed * percentage);
                x5d.InclineSlider.Value = roundToNearestPoint5(interval5Incline * percentage);
                x5d.IsGraduatable = false;
                x5d.CurrentSpeedMetric = x5d.SpeedSlider.Value * 1.609344;
                x5d.Invalidate();
            }
            //cooldowns
            for (int ii = 0; ii<5 ; ii++)
            {
                double[] percentages = { .80, .60, .50, .35, .20 };
                double percentage = percentages[ii];
                CommandBlock x5d = bF.GetNth0x5dBlock(workoutLength - 5 +ii);
                
                x5d.SpeedSlider.Value = rangeCheck(x5d,interval6thFromEndSpeed * percentage);
                x5d.InclineSlider.Value = roundToNearestPoint5(interval6thFromEndIncline * percentage);
                x5d.IsGraduatable = false;
                x5d.CurrentSpeedMetric = x5d.SpeedSlider.Value * 1.609344;
                x5d.Invalidate();

            }
            bAutoWarmupCooldown = true; //re-enable now that we're done


        }

        private double rangeCheck(CommandBlock x5d, double p)
        {
           int tS = x5d.CurrentTimeStamp;
           double adjusted = p;
           double[] minimums = { 2.5, 2.8, 3.1, 3.1, 3.1, 3.1, 2.8, 2.5, 2.0, 2.0 };
           double[] maximums = { 2.5, 3.1, 3.5, 3.7, 4.0, 5.0, 3.5, 2.5, 2.0, 2.0 };

           if (tS >=0 && tS <= 4)
           {
               if (p > maximums[tS])
               {
                   adjusted = maximums[tS];
               }
               if (p < minimums[tS])
               {
                   adjusted = minimums[tS];
               }
           } else if (tS == workoutLength - 5)
           {
               if (p > maximums[5])
               {
                   adjusted = maximums[5];
               }
               if (p < minimums[5])
               {
                   adjusted = minimums[5];
               }


           }
           else if (tS == workoutLength - 4)
           {
               if (p > maximums[6])
               {
                   adjusted = maximums[6];
               }
               if (p < minimums[6])
               {
                   adjusted = minimums[6];
               }

           }
           else if (tS == workoutLength - 3)
           {
               if (p > maximums[7])
               {
                   adjusted = maximums[7];
               }
               if (p < minimums[7])
               {
                   adjusted = minimums[7];
               }

           }
           else if (tS == workoutLength - 2)
           {
               if (p > maximums[8])
               {
                   adjusted = maximums[8];
               }
               if (p < minimums[8])
               {
                   adjusted = minimums[8];
               }

           }
           else if (tS == workoutLength - 1)
           {
               if (p > maximums[9])
               {
                   adjusted = maximums[9];
               }
               if (p < minimums[9])
               {
                   adjusted = minimums[9];
               }

           }
           return adjusted;
        }
        
        void autoWarmupCooldownButton_Click(object sender, RoutedEventArgs e)
        {
            if (bAutoWarmupCooldown)
            {
                doAutoWarmupCooldown();
            }
            else
            {
                MessageBox.Show("Auto Warmup/Cooldown feature disabled.  Enable this feature in the settings menu and try again.", "Feature Disabled in Settings Menu");
            }

        }

        void viewHorizontalSlidersMenuItem_Click(object sender, RoutedEventArgs e)
        {
            bViewHorizontalSliders = viewHorizontalSlidersMenuItem.IsChecked;
            buildGUI();
        }

  
        void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MessageBoxResult mbResult = MessageBox.Show("This program is being distributed as unsupported freeware." +
                       "  Use it at your own risk.  The program author is not responsible for any damage to your computer," +
                       " exercise equipment, or to the health of any person that might result as a consequence of using this program. " +
                       "Always consult a physician before beginning any exercise program.  Do you agree with these terms?", "WorkoutGenSD Terms of Use", MessageBoxButton.YesNo,MessageBoxImage.Question);
            if (mbResult != MessageBoxResult.Yes)
            {
                Environment.Exit(0);
            }
        }

        
        
        
        void playWaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("playWaveFile_Click() called");
            ListBoxItem l = (ListBoxItem)listBox1.SelectedItem;

            if (l.Tag.GetType().ToString() != "WorkoutGenSD.CommandBlock")
            {
                return;
            }

            CommandBlock cb = (CommandBlock)l.Tag;
            SoundFit sF = lF.sFs[wIdx];
            string path = sF.GetPathFromBody(cb.CurrentWaveFileName);
            
            MediaPlayer mp = new MediaPlayer();
            mp.MediaFailed += new EventHandler<ExceptionEventArgs>(mp_MediaFailed);

            if (File.Exists(path))
            {
                mp.Open(new Uri(path, UriKind.Absolute));
                mp.MediaEnded += new EventHandler(mp_MediaEnded);
                mp.Play();
            }
            else
            {
                MessageBox.Show("program error: cannot play media file: " + path + " because it does not exist.", "file not found error");
            }
        }

        void mp_MediaFailed(object sender, ExceptionEventArgs e)
        {
            MessageBox.Show( e.ErrorException.ToString(), "Windows Media Exception");
        }

        void listBox1_LostFocus(object sender, RoutedEventArgs e)
        {
           // disableAllButtons();
        }

        void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            ListBoxItem l = (ListBoxItem)listBox1.SelectedItem;

     /*       if (l != null)
            {
                if (l.Tag.GetType().ToString() != "WorkoutGenSD.CommandBlock")
                {
                   
                    playWaveFileButton.Height = 0;
                    playWaveFileButton.Width = 0;
                    reRecordButton.Height = 0;
                    reRecordButton.Width = 0;
                    reTextToWaveButton.Width = reRecordButton.Width;
                    reTextToWaveButton.Height = reRecordButton.Height;

                    
                    if (l.Tag.ToString() != "dashes")
                    {
                        recordButton.Height = 23;
                        recordButton.Width = 68;
                        textToWaveButton.Width = 68;
                        textToWaveButton.Height = 23;
                        fromDiskButton.Height = 23;
                        fromDiskButton.Width = 68;
                    }
                    else
                    {
                        recordButton.Height = 0;
                        recordButton.Width = 0;
                        fromDiskButton.Width = 0;
                        fromDiskButton.Height = 0;
                        textToWaveButton.Width = 0;
                        textToWaveButton.Height = 0;
                    }
                }
                else
                {
                 
                    playWaveFileButton.Height = 23;
                    playWaveFileButton.Width = 68;
                    reRecordButton.Height = 23;//record over currently selected file
                    reRecordButton.Width = 68;
                    reTextToWaveButton.Width = reRecordButton.Width;
                    reTextToWaveButton.Height = reRecordButton.Height;
                    fromDiskButton.Height = 0;
                    fromDiskButton.Width = 0;
                    recordButton.Height = 0;
                    recordButton.Width = 0;
                    textToWaveButton.Width = 0;
                    textToWaveButton.Height = 0;
                }
            }*/
        }

      
        void checkWorkoutMenus(int selected)
        {
            for (int ii = 0; ii < numWorkouts; ii++)
            {
                workoutMenus[ii].IsChecked = false;
            }

            workoutMenus[selected].IsChecked = true;

            for (int ii = 0; ii < numWorkouts; ii++)
            {
                if (workoutMenus[ii].IsChecked)
                {
                    wIdx = ii;
                    setWindowTitle();
                    break;
                }
            }



            for (int jj = 0; jj < lF.bFs[wIdx].Count; jj++)
            {
                CommandBlock x5d = lF.bFs[wIdx].GetNth0x5dBlock(jj);
                if (x5d != null)
                {
                    x5d.SpeedSlider.Value = x5d.CurrentSpeed;
                    x5d.InclineSlider.Value = x5d.CurrentIncline;
                }

            }

            buildGUI();



        }

        
        void viewActiveWorkoutMenus_Click(object sender, RoutedEventArgs e)
        {
            for (int ii = 0; ii < numWorkouts; ii++)
            {
                workoutMenus[ii].IsChecked = false;
            }

            MenuItem m = (MenuItem)sender;
            m.IsChecked = true;

            for (int ii = 0; ii < numWorkouts; ii++)
            {
                if (workoutMenus[ii].IsChecked)
                {
                    wIdx = ii;
                    setWindowTitle();
                    break;
                }
            }
 


            for (int jj = 0; jj < lF.bFs[wIdx].Count; jj++)
            {
                CommandBlock x5d = lF.bFs[wIdx].GetNth0x5dBlock(jj);
                if (x5d != null)
                {
                    x5d.SpeedSlider.Value = x5d.CurrentSpeed;
                    x5d.InclineSlider.Value = x5d.CurrentIncline;
                }
               

            }

            buildGUI();
            updateUILabels();

 

        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        public string SelectSoundFile() //let's user select a sound file
                                        //and returns the full path string to caller
        {
            string path = null;
            string safe;
            if (!bRecording && !bTexting)
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.DefaultExt = "wav";
                dlg.Filter = "Wave Files|*.wav";
                if (dlg.ShowDialog() == true)
                {
                    path = dlg.FileName.ToUpper();
                    safe = dlg.SafeFileName.ToUpper(); //without full path
                }
                else
                {
                    return null;
                }

                // MessageBox.Show("SafeFileName = " + dlg.SafeFileName);
                //MessageBox.Show("File Body = " + body);
                if (safe.Length != 12)//we can only work with 8.3 filenames
                {
                    MessageBox.Show("Sorry, but the iFit(TM) system can only work with " +
                                        "files having 8 characters in the " +
                                        "file name body (excluding the .wav extension). " +
                                        "Please rename this file to conform to the 8.3 " +
                                        "format or select another file.",
                                        "Incompatible File Name Length Error");
                    return SelectSoundFile();
                }
            }
            else if (bRecording)
            {


                //bRecording = true
                //Here, instead of opening an existing file, we're going to cue up recording
                //and return the path to the output file to the calling method.
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.DefaultExt = "wav";
                dlg.Filter = "Wave Files|*.wav";
                if (dlg.ShowDialog() == true)
                {
                    path = dlg.FileName.ToUpper();
                    safe = dlg.SafeFileName.ToUpper(); //without full path

           //         if (mp3Capture == null)
           //         {
            //            mp3Capture = new Mp3SoundCapture();
            //        }
           //         mp3Capture.CaptureDevice = SoundCaptureDevice.Default;
            //        mp3Capture.OutputType = Mp3SoundCapture.Outputs.Wav;
            //        mp3Capture.WaveFormat = PcmSoundFormat.Pcm8kHz8bitMono;
            //        mp3Capture.Mp3BitRate = Mp3BitRate.BitRate64;
                    //  mp3Capture.NormalizeVolume = true;
                    //  mp3Capture.WaitOnStop = false;

              //      mp3Capture.Stopped += new EventHandler<Mp3SoundCapture.StoppedEventArgs>(mp3Capture_Stopped);
                    //       mp3Capture.Start(path);

                }
                else
                {
                    //stopButton.Height = 0;
                    //stopButton.Width = 0;
                    disableStopButton();
                    return null;
                }
                if (safe.Length != 12)//we can only work with 8.3 filenames
                {
                    MessageBox.Show("Sorry, but the iFit(TM) system can only work with " +
                                        "files having 8 characters in the " +
                                        "file name body (excluding the .wav extension). " +
                                        "Please rename this file to conform to the 8.3 " +
                                        "format.",
                                        "Incompatible File Name Length Error");
                    return SelectSoundFile();
                }


            }
            else if (bTexting)
            {

                //bTexting = true
                //Here, instead of opening an existing file, we're going to create one
                //using a text to speech engine and return the path to it to the calling method.
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.DefaultExt = "wav";
                dlg.Filter = "Wave Files|*.wav";
                if (dlg.ShowDialog() == true)
                {
                    path = dlg.FileName.ToUpper();
                    safe = dlg.SafeFileName.ToUpper(); //without full path

                    
                    TextToSpeechWindow tts = new TextToSpeechWindow();
                    tts.PathToWav = path;


                    tts.ShowDialog();
                    if (tts.result == false) //user canceled
                    {
                        
                        return null;
                    }



                }
                else
                {
                    //stopButton.Height = 0;
                    //stopButton.Width = 0;
                    disableStopButton();
                    return null;
                }
                if (safe.Length != 12)//we can only work with 8.3 filenames
                {
                    MessageBox.Show("Sorry, but the iFit(TM) system can only work with " +
                                        "files having 8 characters in the " +
                                        "file name body (excluding the .wav extension). " +
                                        "Please rename this file to conform to the 8.3 " +
                                        "format.",
                                        "Incompatible File Name Length Error");
                    return SelectSoundFile();
                }

            }





            return path;

        }

           
        

  //      void mp3Capture_Stopped(object sender, Mp3SoundCapture.StoppedEventArgs e)
  //      {
   //         System.Console.Beep();   
   //     }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            
            
        }

        private void renameWorkoutMenus()
        {
            for (int ii = 0; ii < numWorkouts; ii++)
            {
                if (!lF.bFs[ii].Name.Equals("********"))
                {
                    workoutMenus[ii].Header = lF.bFs[ii].Name;

                }
                else //do nothing and just keep the same default W000000X name
                {


                }

            }

        }
        
        private void BuildWorkoutMenus()
        {
            if (!isWorkoutMenusBuilt) //building for the first time
            //as opposed to opening a new layout.fit file
            {
                workoutMenus = new MenuItem[numWorkouts];


                for (int ii = 0; ii < numWorkouts; ii++)
                {
                    workoutMenus[ii] = new MenuItem();
              
               
                    workoutMenus[ii].Header = lF.bFs[ii].Name;
                    workoutMenus[ii].IsCheckable = true;
                    workoutMenus[ii].Click += new RoutedEventHandler(viewActiveWorkoutMenus_Click);
                    workoutMenus[ii].GotFocus += new RoutedEventHandler(workoutMenus_GotFocus);
                    workoutMenus[ii].LostFocus += new RoutedEventHandler(workoutMenus_LostFocus);
                    workoutMenus[ii].Tag = ii;
                    this.activeWorkoutMenu.Items.Add(workoutMenus[ii]);
                }
                menuCanvas = new Canvas();
                menuCanvas.Width = 150;
                menuCanvas.Height = 50;
                menuCanvas.Background = Brushes.Transparent;
                activeWorkoutMenu.Items.Add(menuCanvas);
                
                workoutMenus[wIdx].IsChecked = true;
                isWorkoutMenusBuilt = true;
            }
            else //this is the 2nd go around, rebuilding menus via information in layout.fit
                //file, so we can do things a bit differently
                //we really just need to rename the menu item headers here, where applicable
            {
                for (int ii = 0; ii < numWorkouts; ii++)
                {
                    if (!lF.bFs[ii].Name.Equals("********"))
                    {
                        workoutMenus[ii].Header = lF.bFs[ii].Name;

                    }
                    else //do nothing and just keep the same default W000000X name
                    {


                    }

                }

            }
        }

        void workoutMenus_LostFocus(object sender, RoutedEventArgs e)
        {
         //   MenuItem mI = (MenuItem)sender;
         //   BinaryFit bF = (BinaryFit)mI.Tag;
        


        }

        void workoutMenus_GotFocus(object sender, RoutedEventArgs e)
        {
            MenuItem mI = (MenuItem)sender;
            BinaryFit bF = lF.bFs[(int)mI.Tag];
            updateIntensityCanvas(Brushes.White, Brushes.Black, bF.GetMets(), calculateMets(maxSpeed, maxIncline), menuCanvas);



        }

        private ContextMenu buildSliderContextMenu()
           
        {
            //build the slider context menu
            ContextMenu sliderContextMenu = new ContextMenu();
            MenuItem isGraduatableItem = new MenuItem();
            isGraduatableItem.IsCheckable = true;
            isGraduatableItem.Header = "Graduatable";
            isGraduatableItem.Click += new RoutedEventHandler(isGraduatableItem_Click);
            sliderContextMenu.Items.Add(isGraduatableItem);
   
  
            MenuItem propagateSpeedForwardItem = new MenuItem();
            propagateSpeedForwardItem.Header = "Propagate _Speed Forward";
            propagateSpeedForwardItem.Click += new RoutedEventHandler(propagateSpeedForwardItem_Click);

            sliderContextMenu.Items.Add(propagateSpeedForwardItem);

            MenuItem propagateInclineForwardItem = new MenuItem();
            propagateInclineForwardItem.Header = "Propagate _Incline Forward";
            propagateInclineForwardItem.Click += new RoutedEventHandler(propagateInclineForwardItem_Click);

            sliderContextMenu.Items.Add(propagateInclineForwardItem);
            return sliderContextMenu;



        }

        void isGraduatableItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            currentSmallSpeedSlider.OwnerBlock.IsGraduatable = item.IsChecked;
            currentSmallSpeedSlider.OwnerBlock.Invalidate();
        }

        void propagateSpeedForwardItem_Click(object sender, RoutedEventArgs e)
        {
            BinaryFit bF = lF.bFs[wIdx];
            int index = currentSmallSpeedSlider.OwnerBlock.CurrentTimeStamp;
            bF.PropagateForwardSpeedFromNth0x5dBlock(index);
            doAutoWarmupCooldown();
        }
        void propagateInclineForwardItem_Click(object sender, RoutedEventArgs e)
        {
            BinaryFit bF = lF.bFs[wIdx];
            int index = currentSmallInclineSlider.OwnerBlock.CurrentTimeStamp;
            bF.PropagateForwardInclineFromNth0x5dBlock(index);
            doAutoWarmupCooldown();
        }




        private void buildGUI()
        {

            if (!isWorkoutMenusBuilt)
            {
                BuildWorkoutMenus();
            }




            myGrid.Children.Clear();
            myGrid.RowDefinitions.Clear();
            myGrid.ColumnDefinitions.Clear();


            myGrid.HorizontalAlignment = HorizontalAlignment.Left;
            myGrid.VerticalAlignment = VerticalAlignment.Bottom;



            RowDefinition r0 = new RowDefinition();
            r0.Height = System.Windows.GridLength.Auto;
            RowDefinition r1 = new RowDefinition();
            r1.Height = System.Windows.GridLength.Auto;
            RowDefinition r2 = new RowDefinition();
            r2.Height = System.Windows.GridLength.Auto;
            RowDefinition r3 = new RowDefinition();
            r3.Height = System.Windows.GridLength.Auto;
            RowDefinition r4 = new RowDefinition();
            r4.Height = System.Windows.GridLength.Auto;
            RowDefinition r5 = new RowDefinition();
            r5.Height = System.Windows.GridLength.Auto;

            
            if (!bViewHorizontalSliders)
            {
                  r0.Height = (GridLength) new GridLengthConverter().ConvertFromString("0");
                   r3.Height = (GridLength)new GridLengthConverter().ConvertFromString("0");
                //r2 is speed sliders row
                    r2.Height = (GridLength)new GridLengthConverter().ConvertFromString("70");
                //r5 is for the incline sliders
                    r5.Height = (GridLength)new GridLengthConverter().ConvertFromString("70");
            }
            else
            {

                //r2 is speed sliders row
                    r2.Height = (GridLength)new GridLengthConverter().ConvertFromString("50");
                //r5 is for the incline sliders
                    r5.Height = (GridLength)new GridLengthConverter().ConvertFromString("50");
     
            }
            myGrid.RowDefinitions.Add(r0);
            myGrid.RowDefinitions.Add(r1);
            myGrid.RowDefinitions.Add(r2);
            myGrid.RowDefinitions.Add(r3);
            myGrid.RowDefinitions.Add(r4);
            myGrid.RowDefinitions.Add(r5);

           
            ColumnDefinition[] colDefs = new ColumnDefinition[workoutLength];//default is 30

            for (int ii = 0; ii < workoutLength; ii++)
            {
                colDefs[ii] = new ColumnDefinition();
                myGrid.ColumnDefinitions.Add(colDefs[ii]);

            }


            speedSlider = new Slider2();
            inclineSlider = new Slider2();
            speedSlider.Orientation = System.Windows.Controls.Orientation.Horizontal;
            inclineSlider.Orientation = System.Windows.Controls.Orientation.Horizontal;
            speedSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(speedSlider_ValueChanged);
            inclineSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(inclineSlider_ValueChanged);
            speedSlider.Maximum = maxSpeed;
            speedSlider.Minimum = 0;
            speedSlider.TickFrequency = 0.01609344;
            speedSlider.MouseWheel += new MouseWheelEventHandler(speedSlider_MouseWheel);
            inclineSlider.Maximum = maxIncline;
            inclineSlider.Minimum = 0;
            inclineSlider.TickFrequency = 0.5;
            speedSlider.IsSnapToTickEnabled = true;
            inclineSlider.IsSnapToTickEnabled = true;
            speedSlider.ToolTip = "Use this slider control for fine tune \nadjustment of the speed of the current interval.\n" +
                "You can also use the UP and DOWN arrow keys on your keyboard.";

            inclineSlider.ToolTip = "Use this slider control for fine \ntune adjustment of the incline of the current interval.\n" +
                "You can also use the arrow keys on your keyboard.";

            speedSliderLabel = new Label();
            Grid.SetColumnSpan(speedSliderLabel, this.workoutLength);
            Grid.SetRow(speedSliderLabel, 1);
            myGrid.Children.Add(speedSliderLabel);
            speedSliderLabel.Content = "Speed Controls";
            speedSliderLabel.Foreground = Brushes.White;

            inclineSliderLabel = new Label();
            Grid.SetColumnSpan(inclineSliderLabel, this.workoutLength);
            Grid.SetRow(inclineSliderLabel, 4);
            myGrid.Children.Add(inclineSliderLabel);
            inclineSliderLabel.Content = "Incline Controls";
            inclineSliderLabel.Foreground = Brushes.White;




            Grid.SetColumnSpan(speedSlider, this.workoutLength);
            Grid.SetRow(speedSlider, 0);
            myGrid.Children.Add(speedSlider);

            Grid.SetColumnSpan(inclineSlider, this.workoutLength);
            Grid.SetRow(inclineSlider, 3);
            myGrid.Children.Add(inclineSlider);




            for (int ii = 0; ii < workoutLength; ii++)
            {
                //temporarily create a few CommandBlocks
                //for testing
                BinaryFit bF = this.lF.bFs[this.wIdx];
                //     
                CommandBlock b = bF.GetNth0x5dBlock(ii);
                if (b == null) //if this nth 0x5d block doesn't exist, we create one here
                {
                    //this should never get executed
                    bF.Add(new CommandBlock(BlockType.AdjustSpeedAndIncline, ii, 0, 0, 0));

                }
                b = bF.GetNth0x5dBlock(ii);
                if (b == null)
                {
                    MessageBox.Show("Error in buildGUI() nth0x5dBlock is null");
                }

                Slider2 s = b.SpeedSlider;

                currentSmallSpeedSlider = s;
                s.ContextMenu = buildSliderContextMenu();
                

                s.Background = Brushes.Transparent;
                s.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                myGrid.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                s.MouseDoubleClick += new MouseButtonEventHandler(s_MouseDoubleClick);
                s.MouseEnter += new MouseEventHandler(s_MouseEnter);
                s.KeyDown += new KeyEventHandler(s_KeyDown);
                s.MouseWheel += new MouseWheelEventHandler(s_MouseWheel);

            

                s.GotFocus += new RoutedEventHandler(s_GotFocus);
                s.LostFocus += new RoutedEventHandler(s_LostFocus);
                s.ContextMenuOpening += new ContextMenuEventHandler(s_ContextMenuOpening);
                s.ContextMenuClosing += new ContextMenuEventHandler(s_ContextMenuClosing);


                s.Orientation = System.Windows.Controls.Orientation.Vertical;
                s.IsSnapToTickEnabled = true;
             
                s.Height = Double.NaN;
                s.Width = 25;
                s.MaxWidth = 25;
                s.TickFrequency = 0.0160944;
                s.Minimum = 0;
                s.Maximum = maxSpeed;
                s.ValueChanged += new RoutedPropertyChangedEventHandler<double>(s_ValueChanged);

                Grid.SetRow(s, 2);



                Grid.SetColumn(s, ii);
                myGrid.Children.Add(s);




                Slider2 i = b.InclineSlider;

                               
                i.ContextMenu = s.ContextMenu;
                i.Background = Brushes.Transparent;
                i.Orientation = System.Windows.Controls.Orientation.Vertical;
                i.IsSnapToTickEnabled = true;
              //  i.Height = 50;
               
                i.Height = Double.NaN;
                i.Width = 25;
                i.TickFrequency = 0.5;
                i.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

                i.Minimum = 0;
                i.Maximum = maxIncline;
                i.MouseWheel += new MouseWheelEventHandler(i_MouseWheel);
                i.MouseEnter += new MouseEventHandler(i_MouseEnter);
                i.MouseLeave += new MouseEventHandler(i_MouseLeave);
                i.MouseDoubleClick += new MouseButtonEventHandler(i_MouseDoubleClick);
                i.ValueChanged += new RoutedPropertyChangedEventHandler<double>(i_ValueChanged);
                i.LostFocus += new RoutedEventHandler(i_LostFocus);
                i.GotFocus += new RoutedEventHandler(i_GotFocus);
                i.ContextMenuOpening += new ContextMenuEventHandler(i_ContextMenuOpening);
                i.ContextMenuClosing += new ContextMenuEventHandler(i_ContextMenuClosing);
                i.ContextMenu = s.ContextMenu;

                Grid.SetRow(i, 5);

                Grid.SetColumn(i, ii);
                myGrid.Children.Add(i);

                if (ii == 0)
                {
                    this.currentSmallSpeedSlider = s;
                    this.currentSmallInclineSlider = i;
                    s.OwnerBlock.Focus();
                    s.Focus();
                }



            }


            this.currentSmallSpeedSlider.Focus();


        }

        void speedSlider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int delta = e.Delta;
            Slider2 speedy = (Slider2)sender;
      
           
            
            if (delta > 0)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    speedy.Value += 0.0160944 * 4;
                }
                else
                {
                    speedy.Value += 0.0160944;
                }

            }
            
            else if (delta < 0 )
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    speedy.Value -= 0.0160944*4;
                }
                else
                {
                    speedy.Value -= 0.0160944;
                }
            }
           
        }

        void i_MouseWheel(object sender, MouseWheelEventArgs e)
        {

            DateTime now = DateTime.Now;
            DateTime last = lastValueChange;
            TimeSpan elapsed = now - last;
            if (elapsed.TotalMilliseconds > elapseDelay)
            {


                int delta = e.Delta;
                Slider2 i = (Slider2)sender;
                if (delta > 0)
                {
                    if (e.LeftButton != MouseButtonState.Pressed)
                    {
                        i.Value += 0.5;
                    }
                    else
                    {
                        i.Value += 2.0;
                    }

                }

                else if (delta < 0)
                {
                    if (e.LeftButton != MouseButtonState.Pressed)
                    {
                        i.Value -= 0.5;
                    }
                    else
                    {
                        i.Value -= 2.0;
                    }
                }

            }
            lastValueChange = DateTime.Now;

        }

        void s_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int delta = e.Delta;
            Slider2 s = (Slider2)sender;




            if (delta > 0)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    s.Value += 0.0160944 * 8;
                }
                else
                {
                    s.Value += 0.0160944;
                }

            }

            else if (delta < 0)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    s.Value -= 0.0160944 * 8;
                }
                else
                {
                    s.Value -= 0.0160944;
                }
            }


        }







        void s_KeyDown(object sender, KeyEventArgs e)
        {
            DateTime now = DateTime.Now;
            DateTime last = lastValueChange;
            TimeSpan elapsed = now - last;
            if (elapsed.TotalMilliseconds > elapseDelay)
            {

                Slider2 s = currentSmallSpeedSlider;
                Slider2 i = currentSmallInclineSlider;
                switch (e.Key)
                {
                    case Key.NumPad8:
                        s.Value += 0.1;
                        break;
                    case Key.NumPad2:
                        s.Value -= 0.1;
                        break;
                    case Key.NumPad4:
                        i.Value -= 0.5;
                        break;
                    case Key.NumPad6:
                        i.Value += 0.5;
                        break;

                    default:
                        break;
                }
            }
            lastValueChange = DateTime.Now;
        }

        void i_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Slider2 i = (Slider2)sender;

            int timeStamp = i.OwnerBlock.CurrentTimeStamp;
            if (timeStamp == 0)
            {
                return;
            }
            double prevIncline = lF.bFs[wIdx].GetNth0x5dBlock(timeStamp - 1).CurrentIncline;
            i.Value = prevIncline;
            i.OwnerBlock.Invalidate();

        }

        void i_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            this.disableSliderFocusOnMouseEnter = false;
        }

        void s_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            this.disableSliderFocusOnMouseEnter = false;
        }

        void i_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            this.disableSliderFocusOnMouseEnter = true;
        }

        void s_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            this.disableSliderFocusOnMouseEnter = true;
        }

        void i_MouseLeave(object sender, MouseEventArgs e)
        {
   /*         Slider2 i = (Slider2)sender;
            lF.bFs[wIdx].UnFocusAllSliders();
            i.OwnerBlock.Focus();
*/   
        }

 

        void i_LostFocus(object sender, RoutedEventArgs e)
        {
            Slider2 i = (Slider2)sender;
            lF.bFs[wIdx].UnFocusAllSliders();
            
            //    s.OwnerBlock.UnFocus();
            inclineSliderLabel.Content = "Incline Controls";
        }

      
 

      



   

   

        void i_GotFocus(object sender, RoutedEventArgs e)
        {
            Slider2 i = (Slider2)sender;
            i_MouseEnter(sender, null);
            BinaryFit bF = lF.bFs[wIdx];
            currentSmallInclineSlider = i;
            inclineSlider.Value = i.Value;
            bF.UnFocusAllSliders();
            i.OwnerBlock.Focus();
            currentSmallSpeedSlider = i.OwnerBlock.SpeedSlider;
            speedSlider.Value = i.OwnerBlock.SpeedSlider.Value;
            updateUILabels();

           

           

        }

        void populateListBox(int interval)
        {
            listBox1.Items.Clear();
            BinaryFit bF= lF.bFs[wIdx];
            ListBoxItem dashes = new ListBoxItem();
            dashes.Content = "---------------------------";
            dashes.Tag = "dashes";
            dashes.Selected += new RoutedEventHandler(dashes_Selected);
            if (interval == 0)
            {
                int numWavsInHeader = bF.CountOfWaveFileBlocksInHeader();
                if (numWavsInHeader > 0)
                {
                    for (int ii = 0; ii < numWavsInHeader; ii++)
                    {
                        ListBoxItem cbItem = new ListBoxItem();
                        CommandBlock cb = bF.GetNthWaveFileBlockInHeader(ii);
                        cbItem.Selected += new RoutedEventHandler(cbItem_Selected);
                        cbItem.Content = cb.CurrentWaveFileName + " -- <Start Of Workout>";
                        cbItem.ToolTip = "Double click to remove or highlight and press Delete key.";
                        cbItem.Tag = cb;
                        cbItem.MouseDoubleClick += new MouseButtonEventHandler(listBox1_DoubleClickRemoveWaveFile);
                        cbItem.KeyDown += new KeyEventHandler(listBoxItem_KeyDown);
                       
                        listBox1.Items.Add(cbItem);
                    }
                }
                ListBoxItem addFileToHeader = new ListBoxItem();
                addFileToHeader.Selected += new RoutedEventHandler(addFile_Selected);
                addFileToHeader.Content = "<Add New Wave File To Start Of Workout>";
                addFileToHeader.ToolTip = "Double click to add new wave file from disk or highlight and click a button to the right.";
                addFileToHeader.Tag = "add to header";
                addFileToHeader.MouseDoubleClick += new MouseButtonEventHandler(listBox1_MouseDoubleClickAddNewWaveFile);
                listBox1.Items.Add(addFileToHeader);
                
                listBox1.Items.Add(dashes);

            }

            int numWavsInInterval = bF.CountOfWaveFileBlocksInThisInterval(interval);
            if (numWavsInInterval > 0)
            {
                for (int ii = 0; ii < numWavsInInterval; ii++)
                {
                    ListBoxItem cbItem = new ListBoxItem();
                    cbItem.Selected+=new RoutedEventHandler(cbItem_Selected);
                    CommandBlock cb = bF.GetNthWaveFileBlockAtInterval(interval, ii);
                    int seconds = cb.CurrentTimeStamp;
                    seconds %= 60;
                    string str = seconds.ToString();
                    if (str == "-1")
                    {
                        str = "<Start Of Interval>";
                    }
                    else
                    {
                        str = "<:" + String.Format("{0:D2}",seconds) + " Seconds Into Interval>";
                    }
                    cbItem.Content = cb.CurrentWaveFileName + " -- " + str;
                    cbItem.ToolTip = "Double click to remove or highlight and hit the Delete key.";
                    cbItem.Tag = cb;
                    cbItem.MouseDoubleClick += new MouseButtonEventHandler(listBox1_DoubleClickRemoveWaveFile);
                    cbItem.KeyDown+=new KeyEventHandler(listBoxItem_KeyDown);
                    listBox1.Items.Add(cbItem);

                }
            }
            ListBoxItem addFile = new ListBoxItem();
            addFile.Selected += new RoutedEventHandler(addFile_Selected);
            addFile.Content = "<Add New Wave File To This Interval>";
            addFile.ToolTip = "Double click to add a new wave file from disk or highlight and select a button to the right.";
            addFile.Tag = interval.ToString();
            addFile.MouseDoubleClick += new MouseButtonEventHandler(listBox1_MouseDoubleClickAddNewWaveFile);
            listBox1.Items.Add(addFile);

            if (interval == workoutLength-1)
            {
                listBox1.Items.Add(dashes);

                int numWavsInFinalInterval = bF.CountOfWaveFileBlocksInThisInterval(workoutLength);
                if (numWavsInFinalInterval > 0)
                {
                    for (int ii = 0; ii < numWavsInFinalInterval; ii++)
                    {
                        ListBoxItem cbItem = new ListBoxItem();
                        cbItem.Selected+=new RoutedEventHandler(cbItem_Selected);
                        CommandBlock cb = bF.GetNthWaveFileBlockAtInterval(workoutLength, ii);
                        int seconds = cb.CurrentTimeStamp;
                        seconds %= 60;
                        string str = seconds.ToString();
                        if (str == "-1")
                        {
                            str = "<End Of Workout>";
                        }
                        else
                        {
                            str = "<:" + String.Format("{0:D2}",seconds) + " Seconds Into Interval>";
                        }
                        cbItem.Content = cb.CurrentWaveFileName + " -- " + str;
                        cbItem.ToolTip = "Double click to remove or select and press the Delete key.";
                        cbItem.Tag = cb;
                        cbItem.MouseDoubleClick += new MouseButtonEventHandler(listBox1_DoubleClickRemoveWaveFile);
                        cbItem.KeyDown += new KeyEventHandler(listBoxItem_KeyDown);
                        listBox1.Items.Add(cbItem);

                    }
                }
                ListBoxItem addFileToEndOfWorkout = new ListBoxItem();
                addFileToEndOfWorkout.Selected+=new RoutedEventHandler(addFile_Selected);
                addFileToEndOfWorkout.Content = "<Add New Wave File To End Of Workout>";
                addFileToEndOfWorkout.ToolTip = "Double click to add a new wave file from disk or highlight and push a button to the right.";
                addFileToEndOfWorkout.Tag = "add to end";
                addFileToEndOfWorkout.MouseDoubleClick += new MouseButtonEventHandler(listBox1_MouseDoubleClickAddNewWaveFile);
                listBox1.Items.Add(addFileToEndOfWorkout);







            }





        }

        void addFile_Selected(object sender, RoutedEventArgs e)
        {
            disableAllButtons();
            enableAllInsertionButtons();
        }

 
        void cbItem_Selected(object sender, RoutedEventArgs e)
        {
            disableAllButtons();
            enableAllWaveButtons();
        }

        void disableAllButtons()
        {

            recordButton.Visibility = Visibility.Collapsed;
            reRecordButton.Visibility = Visibility.Collapsed;
            stopButton.Visibility = Visibility.Collapsed;
            textToWaveButton.Visibility = Visibility.Collapsed;
            reTextToWaveButton.Visibility = Visibility.Collapsed;
            fromDiskButton.Visibility = Visibility.Collapsed;
            playWaveFileButton.Visibility = Visibility.Collapsed;
            removeWaveButton.Visibility = Visibility.Collapsed;
            
        


        }

        private void enableStopButton()
        {
            disableAllButtons();
            stopButton.Visibility = Visibility.Visible;
            stackButtons(stopButton);
        }

        void disableStopButton()
        {
            stopButton.Visibility = Visibility.Collapsed;
  
        }

        void stackButtons(params Button[] btns)
        {
            double left=0, top=140, right=5, bottom=0;
            double separation = 27;


            for (int ii = 0; ii < btns.Length; ii++)
            {
                btns[ii].Margin = new Thickness(left, top + ii * separation, right, bottom);
            }
          
        
        }



        void enableAllInsertionButtons()
        {
            disableAllButtons();
            recordButton.Visibility = Visibility.Visible;
            textToWaveButton.Visibility = Visibility.Visible;
            fromDiskButton.Visibility = Visibility.Visible;
            stackButtons(fromDiskButton, textToWaveButton, recordButton);
       


        }

        void enableAllWaveButtons()
        {
            disableAllButtons();
            playWaveFileButton.Visibility = Visibility.Visible;
            reRecordButton.Visibility = Visibility.Visible;
            reTextToWaveButton.Visibility = Visibility.Visible;
            removeWaveButton.Visibility = Visibility.Visible;
            stackButtons(playWaveFileButton, reTextToWaveButton, reRecordButton, removeWaveButton);


        }
        
        void dashes_Selected(object sender, RoutedEventArgs e)
        {
            disableAllButtons();
        }


        private void removeWaveButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem li = (ListBoxItem)listBox1.SelectedItem;
            listBox1_DoubleClickRemoveWaveFile(li, null);


        }
        
        
        
        
        void listBoxItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                listBox1_DoubleClickRemoveWaveFile(sender, null);
            }
            else
            {
                s_KeyDown(null, e);
            }
        }

        void insertTTWsAtStartOfWorkouts()
        {
            for (int ii = 0; ii < numWorkouts; ii++)
            {

                BinaryFit bF = lF.bFs[ii];



                string path = pathToTTWFolder;
                if(!Directory.Exists(path)){
                    Directory.CreateDirectory(path);
                }
                path += bF.Name + ".WAV";
                path = path.ToUpper();

                int insertionPointForNewWaveFileBlock = -1;

                /* Header block will be:
                   * UnknownBlock17 (if it exists)
                   * 0x5a (fetch wave file)
                   * 0x5b (play fetched wave file)
                   * ...
                   * PausePriorToStart (if it exists)
                   * 2nd UnknownBlock15 (if it exists)
                   * 1st 0x5d block in workout
                   */
                int idx = bF.IndexOfNthBlockType(BlockType.PausePriorToStart, 0);
                if (idx == -1) //no pause prior to start block in this workout
                {
                    idx = bF.IndexOfNthBlockType(BlockType.UnknownBlock15, 1);//2nd one
                    if (idx == -1) //no pause prior to start and no 2nd unknownblock15
                    {
                        idx = bF.IndexOfNthBlockType(BlockType.AdjustSpeedAndIncline, 0);
                    }
                }
                int first0x5a = bF.IndexOfNthBlockType(BlockType.FetchWaveFile, 0);
                if (first0x5a < idx && first0x5a!=-1)
                {
                    idx = first0x5a;
                    CommandBlock x5aBlock = bF.GetAt(first0x5a);
                    if (x5aBlock.CurrentWaveFileName == path.Substring(path.Length-12,12)) //been here done this already
                    {
                        bF.RemoveAt(first0x5a); //strip the 0x5a
                        bF.RemoveAt(first0x5a); //strip the 0x5b
                    }
                }
                insertionPointForNewWaveFileBlock = idx;

                SpeechSynthesizer speaker = new SpeechSynthesizer();
                string ourText = "workout number " + (ii+1).ToString();

                System.Speech.AudioFormat.SpeechAudioFormatInfo formatInfo = new System.Speech.AudioFormat.SpeechAudioFormatInfo(8000, System.Speech.AudioFormat.AudioBitsPerSample.Eight, System.Speech.AudioFormat.AudioChannel.Mono);
                speaker.SetOutputToWaveFile(path, formatInfo);
                speaker.Rate = 1;
                speaker.Volume = 100;
                try
                {
                    speaker.Speak(ourText);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
                speaker.SetOutputToDefaultAudioDevice();
                speaker.Dispose();


                
                
                
                SoundFit sF = lF.sFs[ii];
                int waveIndex = sF.GetIndex(path);

                CommandBlock x5a = new CommandBlock(BlockType.FetchWaveFile, waveIndex);
                CommandBlock x5b = new CommandBlock(BlockType.PlayFetchedWaveFile);
                x5a.CurrentWaveFileName = sF.GetFileNameAtIndex(waveIndex) + ".WAV";

                bF.Insert(insertionPointForNewWaveFileBlock, x5a);
                bF.Insert(insertionPointForNewWaveFileBlock + 1, x5b);

              


            }
        }
        
        
        
        void listBox1_MouseDoubleClickAddNewWaveFile(object sender, MouseButtonEventArgs e)
        {
            disableSliderFocusOnMouseEnter = true;
            string path = SelectSoundFile();
            if (path == null)
            {
                disableSliderFocusOnMouseEnter = false;
                return; //user canceled
            }
            
            
            ListBoxItem l = (ListBoxItem)sender;
            string where = (string)l.Tag;
            BinaryFit bF = lF.bFs[wIdx];
            int interval=-1;
            int seconds=-1;
            int insertionPointForNewWaveFileBlock=-1;
            bool useFetchWaveFileBlocks = true;
            if (where == "add to header")
            {
                /* Header block will be:
                 * UnknownBlock17 (if it exists)
                 * 0x5a (fetch wave file)
                 * 0x5b (play fetched wave file)
                 * ...
                 * PausePriorToStart (if it exists)
                 * 2nd UnknownBlock15 (if it exists)
                 * 1st 0x5d block in workout
                 */
                int idx = bF.IndexOfNthBlockType(BlockType.PausePriorToStart,0);
                if (idx == -1) //no pause prior to start block in this workout
                {
                    idx = bF.IndexOfNthBlockType(BlockType.UnknownBlock15, 1);//2nd one
                    if (idx == -1) //no pause prior to start and no 2nd unknownblock15
                    {
                      idx = bF.IndexOfNthBlockType(BlockType.AdjustSpeedAndIncline, 0);
                    }
                }
                insertionPointForNewWaveFileBlock = idx;
            }
            else if (where == "add to end")
            {
                int idx = bF.IndexOfNthBlockType(BlockType.EndProgram, 0);
                insertionPointForNewWaveFileBlock = idx;

            }
            else
            {
                interval = System.Convert.ToInt32(where);
                //we know the wave file is to go into this particular interval, but we
                //don't yet know whether to append to the last 0x5a-0x5b pair or
                //to use an 0x60 timestamped block, so we have to ask the user

              



                InsertWaveFileWindow win = new InsertWaveFileWindow();
                for (int ii = 0; ii < 60; ii++)
                {
                    string str = String.Format(":{0:D2}", ii);
                    ComboBoxItem cbi = new ComboBoxItem();
                    cbi.Content = str;
                    cbi.Tag = ii;
                    win.comboBox1.Items.Add(cbi);
                }
                win.comboBox1.SelectedIndex = 0;
                win.ShowDialog();
                seconds = win.comboBox1.SelectedIndex;
                if (!win.ResultOK)
                {
                    disableSliderFocusOnMouseEnter = false;
                    return; //user canceled
                }
               // MessageBox.Show(seconds.ToString());
                if (seconds != 0)
                {
                    useFetchWaveFileBlocks = false; //we'll use an 0x60 block instead
                    //now where do we place this new 0x60 block?
                    //It should be sorted in with the other 0x60's in this interval (if any)
                    //sorted according to the seconds
                    int timeStamp = interval * 60 + seconds;
                    if (bF.IndexOf0x60BlockWithTimeStamp(timeStamp) != -1)
                    {
                        //already exists a wave file at this time stamp
                        MessageBox.Show("There is already an existing wave file at this particular time stamp.  Please remove it first or else select another second in which to play this file.  (Double click on the existing file in order to remove it.)", "Existing Wave File");
                        disableSliderFocusOnMouseEnter = false;
                        return;
                    }
                   //since this is an 0x60 block with a time stamp we'll just let the BinaryFit object insert it in proper place

                    
                }
                else //user selected :00 as seconds, so we use 0x5a-0x5b pair for this file
                {
                    int index = bF.IndexOfNth0x5dBlock(interval);
                    insertionPointForNewWaveFileBlock = index + 1;
                    for (int ii = index + 1; ii < bF.IndexOfNth0x5dBlock(interval + 1); ii++)
                    {
                        CommandBlock cb = bF.GetAt(ii);
                        if (cb.Type != BlockType.FetchWaveFile &&
                            cb.Type != BlockType.PlayFetchedWaveFile)
                        {
                            insertionPointForNewWaveFileBlock = ii;
                            break;
                        }
                    }
                 

                }
             }
            //at this point in the code we know whether to use 0x5a-0x5b pairs or a single 0x60 block
            //(useFetchWaveFileBlocks = true for 0x5a-0x5b pairs)
            //and we know the insertion point for the new block
            if (bRecording)
            {
                MessageBox.Show("Press OK to begin recording.  Your "
                    +"recording will begin as soon as this box is "
                    +"cleared from the screen, which might take a "
                    +"moment, depending on the speed of your computer."
                    ,"Begin Recording");
                System.Console.Beep();
    //            mp3Capture.Start(path);
                bRecording = false;
            }
            SoundFit sF = lF.sFs[wIdx];
            int waveIndex = sF.GetIndex(path);
            if (useFetchWaveFileBlocks)
            {
                CommandBlock x5a = new CommandBlock(BlockType.FetchWaveFile, waveIndex);
                CommandBlock x5b = new CommandBlock(BlockType.PlayFetchedWaveFile);
                x5a.CurrentWaveFileName = sF.GetFileNameAtIndex(waveIndex)+".WAV";

                bF.Insert(insertionPointForNewWaveFileBlock, x5a);
                bF.Insert(insertionPointForNewWaveFileBlock + 1, x5b);

            }
            else
            { //use 0x60 block with timeStamp
                CommandBlock x60 = new CommandBlock(BlockType.PlayWaveFile, interval * 60 + seconds, waveIndex);
                x60.CurrentWaveFileName = sF.GetFileNameAtIndex(waveIndex)+".WAV";
                bF.Insert0x60AccordingToTimeStamp(x60);

            }

        
           
            disableSliderFocusOnMouseEnter = false;
     
            currentSmallSpeedSlider.Focus(); //forces listBox1 to repopulate itself.


        }

 


        
        void listBox1_DoubleClickRemoveWaveFile(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem l = (ListBoxItem)sender;
            CommandBlock cb = (CommandBlock)l.Tag;
            BinaryFit bF = lF.bFs[wIdx];
            MessageBoxResult mbr = MessageBox.Show("Remove "+cb.CurrentWaveFileName+"from workout? Yes/No?","Confirm Wave File Removal",MessageBoxButton.YesNoCancel,MessageBoxImage.Question);
            if (mbr == MessageBoxResult.Yes)
            {
                if (cb.Type == BlockType.PlayWaveFile)
                {
                    bF.Remove(cb);
                }
                else if (cb.Type == BlockType.FetchWaveFile)
                {
                    bF.RemoveAt(bF.IndexOf(cb) + 1);
                    bF.Remove(cb);
                }
                
            }
            speedSlider.Focus();
            currentSmallSpeedSlider.Focus();
        }

        void updateUILabels()
        {
            Slider2 s = currentSmallSpeedSlider;
            Slider2 i = currentSmallInclineSlider;
           
            speedSliderLabel.Content = "Speed Controls";
            string intervalString = String.Format("{0:D2}:00-{0:D2}:59", s.OwnerBlock.CurrentTimeStamp);
            currentIntervalLabel.Content = "Current Interval: " + intervalString;

            currentSpeedLabel.Content = "Speed: " + System.Math.Round(s.Value, 1).ToString() + " mph (" + System.Math.Round(s.Value * 1.60944, 1).ToString() + " km/h)";
            currentInclineLabel.Content = "Incline: " + System.Math.Round(s.OwnerBlock.CurrentIncline,1).ToString()+"%";
            currentDistanceLabel.Content = "Interval Distance: " + System.Math.Round(s.Value * 1.0 / 60.0, 3).ToString() + " (" + System.Math.Round(s.Value * 1.60944 * 1.0 / 60.0, 3).ToString() + " km)";

            workoutLengthLabel.Content = "Workout Length: " + String.Format("{0:D2}:00", workoutLength);
            double avgSpeed = 0;
            double avgIncline = 0;
            double totalDistance=0;
            double calories = 0;
            double totalCalories = 0;
            BinaryFit bF = this.lF.bFs[wIdx];
            List<double> intensities = new List<double>(workoutLength);

            for (int ii = 0; ii < workoutLength; ii++)
            {
                CommandBlock x5d = bF.GetNth0x5dBlock(ii);
                if (x5d != null)
                {
                    
                    intensities.Add(calculateMets(x5d.CurrentSpeed,x5d.CurrentIncline));
                    totalCalories += calculateCalories(x5d.CurrentSpeed, x5d.CurrentIncline,userWeight);
                    if (x5d.CurrentSpeed == 0)
                    {
                        totalCalories -= calculateCalories(x5d.CurrentSpeed, x5d.CurrentIncline,userWeight);
                    }
                    avgSpeed += x5d.CurrentSpeed;
                    avgIncline += x5d.CurrentIncline;
                    totalDistance += x5d.CurrentSpeed * 1 / 60;
                }
            }
            calories = calculateCalories(s.Value, i.Value, userWeight);
            if (s.Value == 0)
            {
                calories = 0;
            }
            avgSpeed /= workoutLength;
            double avgSpeedMetric = avgSpeed * 1.60944;
            avgSpeed = System.Math.Round(avgSpeed, 3);
            avgSpeedMetric = System.Math.Round(avgSpeedMetric, 3);
            avgIncline /= workoutLength;
            avgIncline = System.Math.Round(avgIncline, 3);
            averageSpeedLabel.Content = "Avg Speed: " + avgSpeed.ToString() + " mph (" + avgSpeedMetric.ToString() + " km/h)";
            averageInclineLabel.Content = "Average Incline: " + avgIncline.ToString() +"%";
            
            double fastest = bF.GetSpeedInFastestBlock();
            fastestSpeedLabel.Content = "Fastest Speed: " + System.Math.Round(fastest, 1).ToString() + " MPH (" + System.Math.Round(fastest * 1.60944, 1).ToString() + " km/h)";
            double steepest = System.Math.Round(bF.GetInclineInSteepestBlock(),1);
            steepestInclineLabel.Content = "Steepest Incline: " + steepest.ToString() + "%";

            workoutDistanceLabel.Content = "Workout Distance: " + System.Math.Round(totalDistance, 3).ToString() + " ("+System.Math.Round(totalDistance*1.60944,3).ToString()+" km)";
            workoutLapsLabel.ToolTip = "A lap is defined as a quarter of a mile (0.402336 km)";
            workoutLapsLabel.Content = "Laps This Workout: " + System.Math.Round(4*totalDistance, 3).ToString();
            currentLapsLabel.ToolTip = workoutLapsLabel.ToolTip;
            currentLapsLabel.Content = "Laps This interval: " + System.Math.Round(s.Value * 4.0 / 60.0, 3).ToString();
            double paceToMile = 60.0 / s.Value;
            int minutes = (int)System.Math.Floor(paceToMile);
            int seconds = (int)((paceToMile - minutes)*60);
            if (s.Value == 0)
            {
                minutes = 0;
                seconds = 0;
            }
            currentPaceLabel.Content = "Pace This Interval: "+String.Format("{0:D2}:{1:D2}",minutes,seconds);
            currentPaceLabel.ToolTip = "This is in minutes:seconds how long it takes to go a mile (1.60944 km) at the current speed.";
            graduatableLabel.Content = "Graduatable Interval: ";

            currentCalorieLabel.Content = "Est. Cals This Interval: " + String.Format("{0:N2}", calories);
            currentCalorieLabel.ToolTip = "We use 2 different formulas for calculating this estimated value.\n";
            currentCalorieLabel.ToolTip += "For walking speeds (less than 3.7 MPH), we use:\n";
            currentCalorieLabel.ToolTip += "0.00226746*(4.3 + (2.68760448 + 0.268760448*incline)*walkingSpeed)*weightInPounds\n";
            currentCalorieLabel.ToolTip += "For running speeds (3.7 MPH or higher), we use:\n";
            currentCalorieLabel.ToolTip += "0.00226746*(3.5 + (5.37520896 + 0.2418844032*incline)*runningSpeed)*weightInPounds\n";
            currentCalorieLabel.ToolTip += "E-Mail me if you know of a better or more accurate way to estimate calories burned.\n";
            currentMETLabel.Content = "METs: " + String.Format("{0:N2}",calculateMets(s.Value, i.Value));
            currentMETLabel.ToolTip = "METs stands for Metabolic Equivalent, giving a relative intensity comparison\n";
            currentMETLabel.ToolTip += "for various activities.  Resting = 1.0.  The formula we use for this is: \n";
            currentMETLabel.ToolTip += "(walking less than 3.7 MPH) 1.2285714285714284 + (0.7678869942857143 + 0.07678869942857142*incline)*walkingSpeed\n";
            currentMETLabel.ToolTip += "(running 3.7 MPH or more) 1 + (1.5357739885714285 + 0.06910982948571427*incline)*runningSpeed\n";
            currentMETLabel.ToolTip += "E-Mail me if you know of a better formula for calculating METs\n";

            totalCaloriesLabel.Content = "Est. Total Cals Workout: " + String.Format("{0:N2}", totalCalories);
            totalCaloriesLabel.ToolTip = "Just a rough estimate based on average "+userWeight+" pound adult.  \n";
            totalCaloriesLabel.ToolTip += "Useful as a means of comparing the relative intensity of different speed/incline settings.\n";
            userWeightLabel.Content = "User Weight: " + userWeight.ToString() + " pounds (" + (userWeight * 0.454).ToString() + " kg)";
            userWeightLabel.ToolTip = "Weight is used for estimating calories burned.  Double click here to set this variable.";
            

            //calculate average pace over workout
            paceToMile = 60.0 / avgSpeed;
            minutes = (int)System.Math.Floor(paceToMile);
            seconds = (int)((paceToMile - minutes)*60);
            if (avgSpeed == 0)
            {
                minutes = 0;
                seconds = 0;
            }

            averagePaceLabel.Content = "Average Pace: " + String.Format("{0:D2}:{1:D2}", minutes, seconds);
            averagePaceLabel.ToolTip = currentPaceLabel.ToolTip;
            if (s.OwnerBlock.IsGraduatable)
            {
                graduatableLabel.Content += "YES";
            }
            else
            {
                graduatableLabel.Content += "NO";
            }

            double minMets = calculateMets(0, 0);
            double maxMets = calculateMets(s.Maximum, i.Maximum);
            double mets = calculateMets(s.Value, i.Value);
            intensityBar.Minimum = minMets;
            intensityBar.Maximum = maxMets;
            intensityBar.Value = mets;

            updateIntensityCanvas(Brushes.Black,autoWarmupCooldownButton.BorderBrush,/* Brushes.White,*/ intensities, maxMets, intensityCanvas);


            intensityBar.Foreground = intensityColor(mets,0.0);
            
       
            string strSpeed = String.Format("Speed Controls: {0:N1} MPH ({1:N1} km/h)", s.Value,s.Value*1.60944);
            speedSliderLabel.Content = strSpeed;

            string strIncline = String.Format("Incline Controls: {0:N1}%", i.Value);
            inclineSliderLabel.Content = strIncline;


        
        }

        private void updateIntensityCanvas(Brush border, Brush background, List<double> intensities, double maxPossibleIntensity, Canvas c)
        {

            Canvas borderCanvas = new Canvas();
            borderCanvas.Height = c.Height - 2;
            borderCanvas.Width = c.Width - 2;
            borderCanvas.Background = border;
            borderCanvas.HorizontalAlignment = HorizontalAlignment.Left;
            borderCanvas.VerticalAlignment = VerticalAlignment.Top;
            Canvas.SetTop(borderCanvas, 1);
            Canvas.SetLeft(borderCanvas, 1);

            c.Background = background;
            c.Children.Clear();
            c.Children.Add(borderCanvas);
            double avgWidth = (c.Width-2) / intensities.Count;
            double mostIntenseInterval = intensities.Max();
            double modifier;
            if (bScaleIntensityCanvas)
            {
                modifier = 90.0 / mostIntenseInterval; //always use at least 90% of available height
            }
            else
            {
                modifier = 90.0 / maxPossibleIntensity;
            }
           

           for(int ii=0; ii<intensities.Count;ii++)
            {
                Canvas can = new Canvas();
                can.Height = (c.Height-2) * intensities[ii] / 100.0 * modifier;
                can.Width = (c.Width-2) / intensities.Count;
              
                can.Background = intensityColor(intensities[ii], 90);
               
                
                can.HorizontalAlignment = HorizontalAlignment.Left;
                can.VerticalAlignment = VerticalAlignment.Bottom;
                Canvas.SetLeft(can, avgWidth * ii+1);
                Canvas.SetBottom(can, 1);
                can.Opacity = intensities[ii] / mostIntenseInterval;
                if (can.Opacity < .6)
                {
                    can.Opacity = .6;
                }
                can.ToolTip = "interval: "+ii.ToString() + " MET = "+String.Format("{0:N2}",intensities[ii]).ToString();
              
                c.Children.Add(can);


            }



      


        }

        private LinearGradientBrush intensityColor(double p, double angle)
        {
            Slider2 s = currentSmallSpeedSlider;
            Slider2 i = currentSmallInclineSlider;
            
            double maxMets = calculateMets(s.Maximum, i.Maximum);
            double mets = p;

            Brush ourBrush = Brushes.Blue;
          

            
            if (mets <= maxMets * .2)
            {
                ourBrush=Brushes.Blue;
            }
            else if (mets <= maxMets * .4)
            {
                ourBrush=Brushes.LightGreen;
            }
            else if (mets <= maxMets * .6)
            {
                ourBrush=Brushes.Yellow;
            }
            else if (mets <= maxMets * .8)
            {
                ourBrush=Brushes.Orange;
            }
            else if (mets <= maxMets)
            {
                ourBrush=Brushes.Red;
            }


            string strColor = ourBrush.ToString();
            // string strColor is in form of "#AARRGGBB"
            string a = strColor.Substring(1, 2);
            string r = strColor.Substring(3, 2);
            string g = strColor.Substring(5, 2);
            string b = strColor.Substring(7, 2);
            byte byteA = System.Convert.ToByte(a, 16);
            byte byteR = System.Convert.ToByte(r, 16);
            byte byteG = System.Convert.ToByte(g, 16);
            byte byteB = System.Convert.ToByte(b, 16);
            Color ourColor = Color.FromArgb(byteA, byteR, byteG, byteB);


            LinearGradientBrush lb;
            if (angle != 0)
            {
                lb = new LinearGradientBrush(ourColor, Brushes.Blue.Color, angle);
            }
            else
            {
                lb = new LinearGradientBrush(ourColor, ourColor, angle);
            }

            return lb;
        }

        void userWeightLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
          
            Menu_AppSettings_Click(null, null);
        }

        double calculateCalories(double speed, double incline, int weightInPounds)
        {
           
            double dMetersPerMinute;
            double dVO2LitersMinute;
            double dKcalsMinute;
            double dFatBurnedMinute;
            double dKcalsBurned;
            double weight = weightInPounds;//pounds
            double dGrade = incline;
            double dVO2LitersTotal;
            const double KCALS_TO_1_LITER_OF_O2 = 5;
            const double ONE_KCALMINUTE_TO_WATTS = 69.767;
            const double KCALS_TO_1_POUND_OF_FAT = 3500;
            double dSpeedKPH = speed * 1.609344;
            dMetersPerMinute = dSpeedKPH * 16.7;
            double dTimeInSeconds = 60;
            double dWatts;
            double dMets;
            double dFatBurnedGrams;

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
            double dWeightKG = weight * .453492;
            dVO2LitersMinute = dWeightKG * dVO2LitersTotal / 1000;
            double dVO2LitersUptake = dVO2LitersMinute / 60 * dTimeInSeconds;
            dKcalsMinute = dVO2LitersMinute *
            KCALS_TO_1_LITER_OF_O2;
            dKcalsBurned = dKcalsMinute / 60 * dTimeInSeconds;
            dWatts = dKcalsMinute * ONE_KCALMINUTE_TO_WATTS;
            //‘1 [lb->g] 453.5924’ grams to 1 pound
            dFatBurnedMinute = (1 / KCALS_TO_1_POUND_OF_FAT) *
            dKcalsMinute;
            dFatBurnedGrams = ((dFatBurnedMinute / 60) * dTimeInSeconds) *
            453.5924;
            //‘3.5 VO2 = 1 Met
            dMets = (1 / 3.5) * dVO2LitersTotal;




            return dKcalsBurned;


        }

        double calculateMets(double speed, double incline)
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
 

        void s_GotFocus(object sender, RoutedEventArgs e)
        {
            
            Slider2 s = (Slider2)sender;
            s_MouseEnter(sender, null);
            BinaryFit bF = lF.bFs[wIdx];
            currentSmallSpeedSlider = s;
            speedSlider.Value = s.Value;
            bF.UnFocusAllSliders();
            s.OwnerBlock.Focus();
            currentSmallInclineSlider = s.OwnerBlock.InclineSlider;
            inclineSlider.Value = s.OwnerBlock.InclineSlider.Value;
            ContextMenu menu = s.ContextMenu;
            MenuItem item = (MenuItem)menu.Items[0];
            item.IsChecked = s.OwnerBlock.IsGraduatable;


            updateUILabels();
            
            populateListBox(s.OwnerBlock.CurrentTimeStamp);
            

            
           
           
            
        }
        void s_LostFocus(object sender, RoutedEventArgs e)
        {
            
            Slider2 s = (Slider2)sender;
        //    s.OwnerBlock.UnFocus();
            speedSliderLabel.Content = "Speed Controls";
        }

       
        void i_MouseEnter(object sender, MouseEventArgs e)
        {
            Slider2 i = (Slider2)sender;
            if (this.disableSliderFocusOnMouseEnter)
            {
            //    textBox1.Text +="i_MouseEnter while context menu active\n";
                return;
            }
          //  textBox1.Text += "i_MouseEnter while context menu INactive\n";
            this.currentSmallInclineSlider = i;
            lF.bFs[wIdx].UnFocusAllSliders();
            i.OwnerBlock.Focus();
            i.OwnerBlock.SpeedSlider.Focus();
            inclineSlider.Value = i.Value;
            currentSmallSpeedSlider = i.OwnerBlock.SpeedSlider;

            string tip = String.Format("{0:D2}:00 - {0:D2}:59 from beginning\n", i.OwnerBlock.CurrentTimeStamp);
            tip += String.Format("{0:D2}:59 - {0:D2}:00 remaining\n\n", workoutLength - 1 - i.OwnerBlock.CurrentTimeStamp);


            tip += "RIGHT CLICK here to access context menu\n";
            if (i.OwnerBlock.CurrentTimeStamp != 0)
            {
                tip += "DOUBLE CLICK here to set to value from previous interval\n\n";
            }

            tip += "You can use the 8 and 2 keys on the numeric keypad to adjust your speed setting.\n";
            tip += "You can use the 4 and 6 keys on the numeric keypad to adjust your incline setting.\n\n";
            tip += "You may also use your scrollwheel to adjust your incline setting.\n";
            tip += "Select the \"hammer\" and keep your left button down for faster scrolling\n";

            if (i.OwnerBlock.CurrentTimeStamp == workoutLength - 1)
            {
                tip += "\nNote: There is no need to set the speed and incline to zero (0) here \n"
                     + "since WorkoutGenSD will automatically set the final speed and incline \n"
                     + "to zero (0) at the end of your workout for you.\n";
            }



            i.ToolTip = tip;

        }
        
        void s_MouseEnter(object sender, MouseEventArgs e)
        {
            Slider2 s = (Slider2)sender;
            if (disableSliderFocusOnMouseEnter)
            {
               // textBox1.Text+="s_MouseEnter() while context menu is active\n";
                return;
            }
           // textBox1.Text += "s_MouseEnter() while context menu is INactive\n";
            this.currentSmallSpeedSlider = s;
            s.OwnerBlock.Focus();
            
            s.Focus();
            speedSlider.Value = s.Value;
  
            string tip = String.Format("{0:D2}:00 - {0:D2}:59 from beginning\n", s.OwnerBlock.CurrentTimeStamp);
            tip += String.Format("{0:D2}:59 - {0:D2}:00 remaining\n\n", workoutLength - 1-s.OwnerBlock.CurrentTimeStamp);

            
            tip += "RIGHT CLICK here to access context menu\n";
            if (s.OwnerBlock.CurrentTimeStamp != 0)
            {
                tip += "DOUBLE CLICK here to set to value from previous interval\n\n";
            }

            tip += "You can use the 8 and 2 keys on the numeric keypad to adjust your speed setting.\n";
            tip += "You can use the 4 and 6 keys on the numeric keypad to adjust your incline setting.\n\n";
            tip += "You may also use your scrollwheel to adjust your speed setting.\n";
            tip += "Select the \"hammer\" and keep your left button down for faster scrolling\n";

            if (s.OwnerBlock.CurrentTimeStamp == workoutLength - 1)
            {
                tip += "\nNote: There is no need to set the speed and incline to zero (0) here \n"
                     + "since WorkoutGenSD will automatically set the final speed and incline \n"
                     + "to zero (0) at the end of your workout for you.\n";
            }


            s.ToolTip = tip;
            
           

            
            
        }

        void s_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Slider2 s = (Slider2)sender;

            int timeStamp = s.OwnerBlock.CurrentTimeStamp;
            if (timeStamp == 0)
            {
                return;
            }
            double prevSpeed = lF.bFs[wIdx].GetNth0x5dBlock(timeStamp - 1).SpeedSlider.Value;
            s.Value = prevSpeed;
            s.OwnerBlock.CurrentSpeedMetric = s.Value * 1.609344;
            s.OwnerBlock.Invalidate();

            
        }

        void i_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
  
            
            
            
            Slider2 i = (Slider2)sender;
            currentSmallInclineSlider = i;
            inclineSlider.Value = i.Value;
            i.OwnerBlock.CurrentIncline = i.Value;
            i.OwnerBlock.Invalidate();
            if (bAutoWarmupCooldown && (i.OwnerBlock.CurrentTimeStamp == 5 || i.OwnerBlock.CurrentTimeStamp == workoutLength - 6))
            {
                doAutoWarmupCooldown();
            }
            i.OwnerBlock.SpeedSlider.Focus();
            updateUILabels();
            i.Focus();


            
            
            
        }

        void s_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider2 s = (Slider2)sender;
            currentSmallSpeedSlider = s;
            speedSlider.Value = s.Value;
            s.OwnerBlock.CurrentSpeed = s.Value;
            s.OwnerBlock.CurrentSpeedMetric = s.Value * 1.60944;
            s.OwnerBlock.Invalidate();
     
            this.speedSlider.Value = s.Value;
            
            
            updateUILabels();
            s.Focus();
            if (bAutoWarmupCooldown && (s.OwnerBlock.CurrentTimeStamp == 5 || s.OwnerBlock.CurrentTimeStamp == workoutLength - 6))
            {
                doAutoWarmupCooldown();
            }
           
            
        }

        private void myGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show(e.ToString());
        }

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveApplicationSettings();
            if (bFileGenerationInProgress)
            {
                MessageBox.Show("Wait until your files have been generated before exiting.", "File Generation In Progress", MessageBoxButton.OK, MessageBoxImage.Warning);
                e.Cancel = true;
            }
           
        }

        private string toHex(int n)
        {
            return String.Format("0X{0:X2}", n);
        }

        private int fromHex(string hex)
        {
            string val = hex.Trim();
            val = val.ToLower();//in case user used 0X instead of 0x
            if (val.StartsWith("0x"))
            {
                val = val.Remove(0, 2);
                return System.Convert.ToInt32(val, 16);
            }
            else
            {   //assume decimal base 10
                return System.Convert.ToInt32(val, 10);
            }
        }

        private void Menu_AppSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow win = new SettingsWindow();
            win.InitialWorkoutLength = workoutLength;
            win.workoutLengthBox.Text = workoutLength.ToString();
            win.unknownBlock02Exp1.Text = toHex(unknownBlock02Experimental1);
            win.unknownBlock08Exp1.Text = toHex(unknownBlock08Experimental1);
            win.unknownBlock08Exp2.Text = toHex(unknownBlock08Experimental2);
            win.unknownBlock10Exp1.Text = toHex(unknownBlock10Experimental1);
            win.unknownBlock11Exp1.Text = toHex(unknownBlock11Experimental1);
            win.unknownBlock12Exp1.Text = toHex(unknownBlock12Experimental1);
            win.unknownBlock13Exp1.Text = toHex(unknownBlock13Experimental1);
            win.unknownBlock14Exp1.Text = toHex(unknownBlock14Experimental1);
            win.unknownBlock15Exp1.Text = toHex(unknownBlock15Experimental1);
            win.unknownBlock15Exp2.Text = toHex(unknownBlock15Experimental2);
            win.unknownBlock16Exp1.Text = toHex(unknownBlock16Experimental1);
            win.unknownBlock17Exp1.Text = toHex(unknownBlock17Experimental1);
            win.setMaxRunTimeExp1.Text = toHex(maxRunTimeExperimental1);
            win.showProgressGraphicsExp1.Text = toHex(showProgressGraphicsExperimental1);
            win.showInitialSpeedExp1.Text = toHex(showInitialSpeedExperimental1);
            win.pausePriorToStartExp1.Text = toHex(pausePriorToStartExperimental1);
            win.pausePriorToStartExp2.Text = toHex(pausePriorToStartExperimental2);
            win.pausePriorToStartExp3.Text = toHex(pausePriorToStartExperimental3);
            

            
            win.includeBlock02.IsChecked = doBlock02;
            win.includeBlock08.IsChecked = doBlock08;
            win.includeBlock10.IsChecked = doBlock10;
            win.includeBlock11.IsChecked = doBlock11;
            win.includeBlock12.IsChecked = doBlock12;
            win.includeBlock13.IsChecked = doBlock13;
            win.includeBlock14.IsChecked = doBlock14;
            win.includeBlock15.IsChecked = doBlock15;
            win.includeBlock16.IsChecked = doBlock16;
            win.includeBlock17.IsChecked = doBlock17;
            win.includeMaxRunTime.IsChecked = doMaxRunTime;
            win.includePausePriorToStart.IsChecked = doPausePriorToStart;
            win.includeShowInitialIncline.IsChecked = doShowInitialIncline;
            win.includeShowInitialSpeed.IsChecked = doShowInitialSpeed;
            win.includeShowProgressGraphics.IsChecked = doShowProgressGraphBlock;
            win.outputDirectoryBox.Text = outputDirectory;
            win.userWeightTextBox.Text = userWeight.ToString();
         

            win.ShowDialog();
            if (win.OK)
            {
                bool bNeedInit = false;
                if (workoutLength != System.Convert.ToInt32(win.workoutLengthBox.Text))
                {
                    bNeedInit = true;
                }
                workoutLength = System.Convert.ToInt32(win.workoutLengthBox.Text);
                if (workoutLength > 90)
                {
                    workoutLength = 90;
                }
                else if (workoutLength < 20)
                {
                    workoutLength = 20;
                }

                unknownBlock02Experimental1 = fromHex(win.unknownBlock02Exp1.Text);
                unknownBlock08Experimental1 = fromHex(win.unknownBlock08Exp1.Text);
                unknownBlock08Experimental2 = fromHex(win.unknownBlock08Exp2.Text);
                unknownBlock10Experimental1 = fromHex(win.unknownBlock10Exp1.Text);
                unknownBlock11Experimental1 = fromHex(win.unknownBlock11Exp1.Text);
                unknownBlock12Experimental1 = fromHex(win.unknownBlock12Exp1.Text);
                unknownBlock13Experimental1 = fromHex(win.unknownBlock13Exp1.Text);
                unknownBlock14Experimental1 = fromHex(win.unknownBlock14Exp1.Text);
                unknownBlock15Experimental1 = fromHex(win.unknownBlock15Exp1.Text);
                unknownBlock15Experimental2 = fromHex(win.unknownBlock15Exp2.Text);
                unknownBlock16Experimental1 = fromHex(win.unknownBlock16Exp1.Text);
                unknownBlock17Experimental1 = fromHex(win.unknownBlock17Exp1.Text);

                maxRunTimeExperimental1 = fromHex(win.setMaxRunTimeExp1.Text);
                showProgressGraphicsExperimental1 = fromHex(win.showProgressGraphicsExp1.Text);
                showInitialSpeedExperimental1 = fromHex(win.showInitialSpeedExp1.Text);
                pausePriorToStartExperimental1 = fromHex(win.pausePriorToStartExp1.Text);
                pausePriorToStartExperimental2 = fromHex(win.pausePriorToStartExp2.Text);
                pausePriorToStartExperimental3 = fromHex(win.pausePriorToStartExp3.Text);

                doBlock02 = (bool)win.includeBlock02.IsChecked;
                doBlock08 = (bool)win.includeBlock08.IsChecked;
                doBlock10 = (bool)win.includeBlock10.IsChecked;
                doBlock11 = (bool)win.includeBlock11.IsChecked;
                doBlock12 = (bool)win.includeBlock12.IsChecked;
                doBlock13 = (bool)win.includeBlock13.IsChecked;
                doBlock14 = (bool)win.includeBlock14.IsChecked;
                doBlock15 = (bool)win.includeBlock15.IsChecked;
                doBlock16 = (bool)win.includeBlock16.IsChecked;
                doBlock17 = (bool)win.includeBlock17.IsChecked;

                doMaxRunTime = (bool)win.includeMaxRunTime.IsChecked;
                doPausePriorToStart = (bool)win.includePausePriorToStart.IsChecked;
                doShowInitialIncline = (bool)win.includeShowInitialIncline.IsChecked;
                doShowInitialSpeed = (bool)win.includeShowInitialSpeed.IsChecked;
                doShowProgressGraphBlock = (bool)win.includeShowProgressGraphics.IsChecked;
                outputDirectory = win.outputDirectoryBox.Text;
                try
                {
                    userWeight = System.Convert.ToInt32(win.userWeightTextBox.Text);
                }
                catch (Exception exc)
                {
                    MessageBox.Show("Invalid weight value, using default of 150 lbs.", exc.ToString());
                    userWeight = 150;
                }



                SaveApplicationSettings();
                if (bNeedInit)
                {
                    init();
                }
            }
            else
            {
                
            }
            
            
            
        }

        private void exportText_Click(object sender, RoutedEventArgs e)
        {

            bAsync = false;
            adjustHeaderBlocks();
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = "txt";
            dlg.Filter = "Text Files|*.txt";
            dlg.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            StreamWriter strm=null;
            if (dlg.ShowDialog() == true)
            {
                //textBox1.Text = "Exported .text file:\n";
                try
                {
                    strm = new StreamWriter(dlg.FileName);
                    foreach (BinaryFit bf in lF.bFs)
                    {
                        strm.Write(bf.ToString());
                       
                    }
                    
                    foreach (SoundFit sF in lF.sFs)
                    {
                        strm.WriteLine(sF.Name);
                     
                        foreach (SoundLine l in sF)
                        {
                            strm.Write(l.ToString());
                      
                        }
                    }

                }catch (Exception exc)
                {
                    MessageBox.Show(exc.ToString());
                } finally
                {
                    if(strm!=null){
                        strm.Close();
                        strm.Dispose();
                    }

                }
            }
                

        }

        private void setWindowTitle()
        {
            mainWindow.Title = versionString+" -- ";
            mainWindow.Title += lF.bFs[wIdx].Name;
         
            workoutNameTextBox.Text = lF.bFs[wIdx].Name;
            viewHorizontalSlidersMenuItem.IsChecked = bViewHorizontalSliders;
            autoWarmupCooldownMenuItem.IsChecked = bAutoWarmupCooldown;
            autoInsertTTWs.IsChecked = bAutoTTWsAtStartOfWorkouts;
            autoInsertTTWsForAllSpeedAndInclineAdjustments.IsChecked = bAutoTTWsAtEachSpeedAndInclineAdjustment;
            autoCreateBikeFilesMenuItem.IsChecked = bAutoCreateBikeFiles;
            autoCreateInclineFilesMenuItem.IsChecked = bAutoCreateInclineFiles;
            autoCreateEllipticalFilesMenuItem.IsChecked = bAutoCreateEllipticalFiles;


     //       overwriteWaveFilesInOutputDirectory.IsChecked = bOverwriteWaveFilesInOutputDirectory;
            
        }

        private void openLayoutFit(string pathToLayoutFit)
        {

            lF.ImportLayoutDotFitFile(numWorkouts, pathToLayoutFit);
            wIdx = 0;
            setWindowTitle();
            workoutLength = lF.bFs[wIdx].CountOf0x5dBlocks() - 1;//we append one to the end of each

            checkWorkoutMenus(wIdx);

            for (int ii = 0; ii < numWorkouts; ii++)
            {
                wIdx = ii;
                setWindowTitle();
                buildGUI();
            }

            wIdx = 0;
            setWindowTitle();
            renameWorkoutMenus();
            checkWorkoutMenus(0);
            bAsync = true;
            adjustHeaderBlocks();
        
            lF.bFs[wIdx].GetNth0x5dBlock(0).SpeedSlider.Focus();

        }


        private void openLayoutFitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            
            OpenFileDialog ofn = new OpenFileDialog();
            ofn.DefaultExt = "fit";
            ofn.Filter = "Layout.fit file|layout.fit";
            if (ofn.ShowDialog() == true)
            {
                string fn = ofn.FileName;
                if (fn.Contains("read"))//Tread
                {
                    openLayoutFit(ofn.FileName);
                }
                else
                {
                    MessageBox.Show("You must open your layout.fit file from the iFit\\Tread\\ folder even if you are using other types of equipment, such as bikes or incline trainers. ", "Notification");

                }
            }
            else
            {
                return; //user canceled
            }



            

            
            //textBox1.Text = lF.ToString();
            
        }

        private void speedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            currentSmallSpeedSlider.Value = speedSlider.Value;
            updateUILabels();
         
        }
        void inclineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            currentSmallInclineSlider.Value = inclineSlider.Value;
            updateUILabels();
        }


        public void generateBinaryFits()
        {

           //called by background thread
            List<byte> buffer = new List<byte>(9 * (workoutLength * 4 + 20));
            List<byte> iniBuffer = new List<byte>(workoutLength);
            List<byte> bikeBuffer = new List<byte>(workoutLength);
            List<byte> inclineBuffer = new List<byte>(workoutLength);

            int counter = 0;
            foreach (BinaryFit bF in lF.bFs)
            {
                counter++;
                _backgroundWorker_GenFiles.ReportProgress(counter * 2);
                bikeBuffer.Clear();
                inclineBuffer.Clear();
                buffer.Clear();
                iniBuffer.Clear();
                bF.AdjustByte8In0x5dBlocks();
                bF.AdjustInitialSpeedAndInclineHeaderBlock();
                bF.AdjustShowInitialSpeedBlock();
                bF.AdjustShowInitialInclineBlock();

                
    
                foreach (CommandBlock cb in bF)
                {
                    if (cb.Type == BlockType.AdjustSpeedAndIncline)
                    {
                        if (cb.IsGraduatable)
                        {
                            iniBuffer.Add((byte)0x01);
                        }
                        else
                        {
                            iniBuffer.Add((byte)0x00);
                        }
                
                    }
                    
                    
                    foreach (Byte b in cb)
                    {
                        buffer.Add(b);
                    }


                    List<Byte> bikeBlock = new List<Byte>(cb.Length());
                    List<Byte> inclineBlock = new List<Byte>(cb.Length());
                    if (cb.Type != BlockType.CommandCount)
                    {
                        bikeBlock = treadToBike(cb);
                        inclineBlock = treadToIncline(cb);
                    }
                    else //just do a direct copy for the command count block types
                    {
                        foreach (Byte b in cb)
                        {
                            bikeBlock.Add(b);
                            inclineBlock.Add(b);
                        }

                        //bike binary fit files will have 4 fewer command counts than treadmill files
                        byte hb = bikeBlock.ElementAt(0);
                        byte lb = bikeBlock.ElementAt(1);
                        lb -= (byte) 0x04;
                        if (lb >= (byte)252)
                        {
                            hb--;
                        }

                        bikeBlock.Clear();
                        bikeBlock.Add(hb);
                        bikeBlock.Add(lb);

                        //incline trainers will have 1 fewer command count because we drop the UnknownBlock10 types only
                        
                        hb = inclineBlock.ElementAt(0);
                        lb = inclineBlock.ElementAt(1);
                        lb -= (byte)0x01;
                        if (lb >= (byte)255)
                        {
                            hb--;
                        }

                        inclineBlock.Clear();
                        inclineBlock.Add(hb);
                        inclineBlock.Add(lb);

                    }




                    foreach (Byte b in bikeBlock)
                    {
                        bikeBuffer.Add(b);

                    }

                    foreach (Byte b in inclineBlock)
                    {
                        inclineBuffer.Add(b);
                    }
                    
                                    

                }
                if (!bZipping)
                {
                    string path = this.outputDirectory + "\\iFit\\Tread\\" + bF.Name + ".fit";
                    string iniPath = this.outputDirectory + "\\iFit\\Tread\\" + bF.Name + ".bin";
                    string bikePath = this.outputDirectory + "\\iFit\\Bike\\" + bF.Name + ".fit";
                    string inclinePath = this.outputDirectory + "\\iFit\\Incline\\" + bF.Name + ".fit";
                    string ellipticalPath = this.outputDirectory+"\\iFit\\Elliptic\\"+bF.Name+".fit";
                    try
                    {
                        Directory.CreateDirectory(outputDirectory + "\\iFit\\Tread\\");

                        if (bAutoCreateBikeFiles)
                        {
                            Directory.CreateDirectory(outputDirectory + "\\iFit\\Bike\\");
                        }

                        if (bAutoCreateInclineFiles)
                        {
                            Directory.CreateDirectory(outputDirectory + "\\iFit\\Incline\\");
                        }

                        if (bAutoCreateEllipticalFiles)
                        {
                            Directory.CreateDirectory(outputDirectory + "\\iFit\\Elliptic\\");
                        }
                    }
                    catch (Exception except)
                    {
                        MessageBox.Show(except.ToString(), "Exception creating output directory.  Is your SD card or removable drive inserted?");
                        generateBinaryFits();
                        return;
                    }
                    writeBinaryFile(path, buffer);
                    writeBinaryFile(iniPath, iniBuffer);

                    if (bAutoCreateBikeFiles)
                    {
                        writeBinaryFile(bikePath, bikeBuffer);
                    }
                    if (bAutoCreateInclineFiles)
                    {
                        writeBinaryFile(inclinePath, inclineBuffer);
                    }

                    if (bAutoCreateEllipticalFiles)
                    {
                        writeBinaryFile(ellipticalPath, bikeBuffer);
                    }
                }
                else
                {

                    Uri partUriDocumentBf = PackUriHelper.CreatePartUri(new Uri("iFit\\Tread\\" + bF.Name + ".fit", UriKind.Relative));
                    Uri partUriDocumentBfBike = PackUriHelper.CreatePartUri(new Uri("iFit\\Bike\\" + bF.Name + ".fit", UriKind.Relative));
                    Uri partUriDocumentBfIncline = PackUriHelper.CreatePartUri(new Uri("iFit\\Incline\\" + bF.Name + ".fit", UriKind.Relative));
                    Uri partUriDocumentBfElliptical = PackUriHelper.CreatePartUri(new Uri("iFit\\Elliptic\\" + bF.Name + ".fit", UriKind.Relative));
                    Uri partUriDocumentIni = PackUriHelper.CreatePartUri(new Uri("iFit\\Tread\\" + bF.Name + ".ini", UriKind.Relative));
                    
                    PackagePart packagePartDocumentBf = zipFile.CreatePart(partUriDocumentBf, System.Net.Mime.MediaTypeNames.Application.Octet, compressionOption);
                    PackagePart packagePartDocumentBfBike = zipFile.CreatePart(partUriDocumentBfBike, System.Net.Mime.MediaTypeNames.Application.Octet, compressionOption);
                    PackagePart packagePartDocumentBfIncline = zipFile.CreatePart(partUriDocumentBfIncline, System.Net.Mime.MediaTypeNames.Application.Octet, compressionOption);
                    PackagePart packagePartDocumentBfElliptical = zipFile.CreatePart(partUriDocumentBfElliptical, System.Net.Mime.MediaTypeNames.Application.Octet, compressionOption);
                    PackagePart packagePartDocumentIni = zipFile.CreatePart(partUriDocumentIni, System.Net.Mime.MediaTypeNames.Application.Octet, compressionOption);
                    
                    writePackagePart(packagePartDocumentBf.GetStream(), buffer);

                    if (bAutoCreateBikeFiles)
                    {
                        writePackagePart(packagePartDocumentBfBike.GetStream(), bikeBuffer);
                    }

                    if (bAutoCreateInclineFiles)
                    {
                        writePackagePart(packagePartDocumentBfIncline.GetStream(), inclineBuffer);
                    }

                    if (bAutoCreateEllipticalFiles)
                    {
                        writePackagePart(packagePartDocumentBfElliptical.GetStream(), bikeBuffer); //elliptical and bike files appear identical?
                    }
                    
                    writePackagePart(packagePartDocumentIni.GetStream(), iniBuffer);
                    


                }

            }
        }

        private List<byte> treadToBike(CommandBlock cb)
        {
            //take the treadmill command block (cb) and convert to a bike/elliptic block
            List<byte> bikeBlockRet = new List<byte>(cb.Length());
            foreach (byte b in cb)
            {
                bikeBlockRet.Add(b);
            }

            switch (bikeBlockRet.ElementAt(0))
            {
                case 0x5a: //0x5a becomes 0x4d
                    bikeBlockRet.RemoveAt(0);
                    bikeBlockRet.Insert(0, 0x4d);
                    break;

                case 0x5b: //0x5b becomes 0x4e
                    bikeBlockRet.RemoveAt(0);
                    bikeBlockRet.Insert(0, 0x4e);
                    break;

                case 0x60: //0x60 becomes 0x53
                    bikeBlockRet.RemoveAt(0);
                    bikeBlockRet.Insert(0, 0x53);
                    break;

                case 0x5d: //0x5d becomes 0x50
                    bikeBlockRet.RemoveAt(0);
                    bikeBlockRet.Insert(0, 0x50);

                    //we also need to do some additional massaging here
                    int time1 = (int)cb.GetByte(2);
                    int time2 = (int)cb.GetByte(3);
                    int rpm = (int)cb.GetByte(4);
                    //skip GetByte(5) since that was the metric conversion of the speed byte
                    int resistance = (int)cb.GetByte(6);
                    resistance /= 10; //hopefully convert properly to resistance from incline
                    int num0x60s = (int)cb.GetByte(7);
                    
                    //create the new block and calculate the checksum
                    CommandBlock newBlock = new CommandBlock(BlockType.BikeAdjust, time1, time2, rpm, resistance, num0x60s);
                    bikeBlockRet.Clear();
                    foreach (byte b in newBlock)
                    {
                        bikeBlockRet.Add(b);
                    }
                    //one final adjustment
                    byte byte6 = bikeBlockRet.ElementAt(6);
                    bikeBlockRet.RemoveAt(6);
                    byte6++; //the sound file count byte needs to be incremented here for bikes/elliptics
                    bikeBlockRet.Insert(6, byte6);
                    break;
                
                case 0x04: //0x04 0x03 0xchecksum becomes 0x04 0x02 0xnewchecksum
                    bikeBlockRet.RemoveAt(1);
                    bikeBlockRet.Insert(1, 0x02);
                    break;

                case 0x0f: //0f f1 becomes 0e f2
                    bikeBlockRet.RemoveAt(0);
                    bikeBlockRet.Insert(0, 0x0e);
                    break;

                case 0x0d: //skip this block type for bike/elliptics
                    bikeBlockRet.Clear();
                    break;

                case 0x06: //(change to 06 00 fa for elliptic/bike)
                    bikeBlockRet.Clear();
                    bikeBlockRet.Add(0x06);
                    bikeBlockRet.Add(0x00);
                    bikeBlockRet.Add(0xfa);
                    break; 

                case 0x07: //remove one of the 0x00 bytes at index 01 or 02
                    bikeBlockRet.RemoveAt(02);
                    break;
                case 0x08: //gets tricky here because we need to insert an additional block after
                           //this one, so we just make this a longer block and hard code them in
                    bikeBlockRet.Clear();
                    bikeBlockRet.Add(0x08); //had to do with initial incline, which is n/a for bike/elliptics
                    bikeBlockRet.Add(0x00);
                    bikeBlockRet.Add(0xf8);
                    bikeBlockRet.Add(0x09); //begin of next block
                    bikeBlockRet.Add(0x00);
                    bikeBlockRet.Add(0xf7); //checksum at end of 2nd block
                    break;
                
                case 0x1c: //similar to 0x08 case  0x1c becomes 0x11, plus we add another block to the end
                    bikeBlockRet.Clear();
                    bikeBlockRet.Add(0x11);
                    bikeBlockRet.Add(0x01);
                    bikeBlockRet.Add(0xee); //checksum for first block
                    bikeBlockRet.Add(0x19);//start of 2nd block
                    bikeBlockRet.Add(0x02);
                    bikeBlockRet.Add(0xe5); //second block checksum
                    break;

                case 0x22: //0x22 becomes 0x1a
                    bikeBlockRet.Clear();
                    bikeBlockRet.Add(0x1a);
                    bikeBlockRet.Add(0x00);
                    bikeBlockRet.Add(0xe6);
                    break;

                case 0x13: //0x13 becomes 0x1b
                    bikeBlockRet.Clear();
                    bikeBlockRet.Add(0x1b);
                    bikeBlockRet.Add(0x00);
                    bikeBlockRet.Add(0xe5);

                    bikeBlockRet.Add(0x44);//begin 2nd block
                    bikeBlockRet.Add(0xff);
                    bikeBlockRet.Add(0xfb);
                    bikeBlockRet.Add(0x01);
                    bikeBlockRet.Add(0xc1);

                    bikeBlockRet.Add(0x54);//begin 3rd block
                    bikeBlockRet.Add(0x00);
                    bikeBlockRet.Add(0x00);
                    bikeBlockRet.Add(0x00);
                    bikeBlockRet.Add(0xac);
                    
                    break;

                case 0x2d: //just drop the 0x2d blocks
                    bikeBlockRet.Clear();
                    break;

                case 0x14: //drop 0x14 blocks
                    bikeBlockRet.Clear();
                    break;

                case 0x23: //drop them
                    bikeBlockRet.Clear();
                    break;

                case 0x24: //drop it
                    bikeBlockRet.Clear();
                    break;

                case 0x51: //drop it
                    bikeBlockRet.Clear();
                    break;

                case 0x15: //drop it
                    bikeBlockRet.Clear();
                    break;                 
                    


                default:
                    break;



            }

            
            int checksum = 0;
            int sum = 0;
            for (int ii = 0; ii < bikeBlockRet.Count-1; ii++)
            {
                sum += (int)bikeBlockRet.ElementAt(ii);

            }
            checksum = (256 - sum % 256);
            if (bikeBlockRet.Count() > 0) //skip checksum on empty blocks
            {
                bikeBlockRet.RemoveAt(bikeBlockRet.Count - 1);
                bikeBlockRet.Add((byte)checksum);
            }


            return bikeBlockRet;
        }


        private List<byte> treadToIncline(CommandBlock cb)
        {
            //take the treadmill command block (cb) and convert to an incline trainer block
            List<byte> inclineBlockRet = new List<byte>(cb.Length());
            foreach (byte b in cb)
            {
                inclineBlockRet.Add(b);
            }

            switch (inclineBlockRet.ElementAt(0))
            {
 

                case 0x04: //0x04 0x03 0xchecksum becomes 0x04 0x04 0xnewchecksum (03 = tread, 04 = incline, 02 = bike/elliptical)
                    inclineBlockRet.RemoveAt(1);
                    inclineBlockRet.Insert(1, 0x04);
                    break;

          /* //max speed - following blocked out code made the speed always 2.9
           * block out to try to see if the output will still work
           * case 0x06: //(change to 06 00 fa for elliptic/bike)
                            //(but for incline we change to 06 1d 2e af |af=checksum -- recalculated below)
                    inclineBlockRet.Clear();
                    inclineBlockRet.Add(0x06);
                    inclineBlockRet.Add(0x1d);
                    inclineBlockRet.Add(0x2e);
                    inclineBlockRet.Add(0xaf);
                    break;*/

             /*   case 0x08: //change from 08 46 b2 for treads to 08 9b 5d for inclines
                    inclineBlockRet.Clear();
                    inclineBlockRet.Add(0x08); 
                    inclineBlockRet.Add(0x9b);
                    inclineBlockRet.Add(0x5d);
                    break;  */

 

                case 0x22: //UnknownBlock12 type becomes 22 02 dc for incline trainers (was 22 01 d2 for treads)
                    inclineBlockRet.Clear();
                    inclineBlockRet.Add(0x22);
                    inclineBlockRet.Add(0x02);
                    inclineBlockRet.Add(0xdc);
                    break;


  

                case 0x14: //UnknownBlock15 blocks get modified from 14 14 00 d8 for treads to 14 15 21 b6 for incline trainers
                    inclineBlockRet.Clear();
                    inclineBlockRet.Add(0x14);
                    inclineBlockRet.Add(0x15);
                    inclineBlockRet.Add(0x21);
                    inclineBlockRet.Add(0xb6);
                    break;

      

        /*        case 0x15: //SetInitialSpeedAndIncline gets some modification
                            //treads = 15 14 00 d7, but incline trainers = 15 15 21 b5
                    inclineBlockRet.Clear();
                    inclineBlockRet.Add(0x15);
                    inclineBlockRet.Add(0x15);
                    inclineBlockRet.Add(0x21);
                    inclineBlockRet.Add(0xb5);
                    break;*/

                
                //here we drop the command block types that we don't use with incline trainers
                case 0x1c: //UnknownBlock10 not used for incline trainers, so we drop it

                    inclineBlockRet.Clear();
                    break;

                default:
                    break;



            }

            //do checksum here
            int checksum = 0;
            int sum = 0;
            for (int ii = 0; ii < inclineBlockRet.Count - 1; ii++)
            {
                sum += (int)inclineBlockRet.ElementAt(ii);

            }
            checksum = (256 - sum % 256);
            if (inclineBlockRet.Count() > 0) //skip checksum on empty blocks
            {
                inclineBlockRet.RemoveAt(inclineBlockRet.Count - 1);
                inclineBlockRet.Add((byte)checksum);
            }


            return inclineBlockRet;
        }



        
        public void generateSoundFits(){
            //called by background thread

            List<byte> buffer = new List<byte>(9 * (workoutLength * 4 + 20));
            Dictionary<string, string> copyManifest = new Dictionary<string, string>(200);
            string path = "";
            string bikePath = "";
            string inclinePath = "";
            string ellipticalPath = "";
            int counter = 28; //progress window counter
            int whichSF = -1;
            foreach (SoundFit sF in lF.sFs)
            {
                whichSF++;
                counter++;
                fileGenerationStatusString = "Generating " + sF.Name + "...";
                _backgroundWorker_GenFiles.ReportProgress(counter);
                path = this.outputDirectory + "\\iFit\\Tread\\" + sF.Name + ".fit";
                bikePath = this.outputDirectory + "\\iFit\\Bike\\" + sF.Name + ".fit";
                inclinePath = this.outputDirectory + "\\iFit\\Incline\\" + sF.Name + ".fit";
                ellipticalPath = this.outputDirectory + "\\iFit\\Elliptic\\" + sF.Name + ".fit";
                buffer.Clear();
                foreach (SoundLine sL in sF)
                {
                    foreach (byte b in sL)
                    {

                        buffer.Add(b);
                    }


                    string sourcePath = sL.path;
                    string destPath = this.outputDirectory + "\\iFit\\Tread\\" + sL.fileBody + ".wav";
                    string bikeDestPath = this.outputDirectory + "\\iFit\\Bike\\" + sL.fileBody + ".wav";
                    string inclineDestPath = this.outputDirectory + "\\iFit\\Incline\\" + sL.fileBody + ".wav";
                    string ellipticalDestPath = this.outputDirectory + "\\iFit\\Elliptic\\" + sL.fileBody + ".wav";
                    if (sourcePath != null && !sourcePath.Contains("******") && !destPath.Contains("******") && destPath != null)
                    {

                        try
                        {
                            if (!bZipping)
                            {
                                
                                    try
                                    {
                                        if (!copyManifest.ContainsKey(destPath) )
                                        {
                                            copyManifest.Add(destPath, sourcePath);
                                        }
                                        if (bAutoCreateBikeFiles)
                                        {
                                            if (!copyManifest.ContainsKey(bikeDestPath) )
                                            {
                                                copyManifest.Add(bikeDestPath, sourcePath);
                                            }
                                        }
                                        if (bAutoCreateInclineFiles)
                                        {
                                            if (!copyManifest.ContainsKey(inclineDestPath) )
                                            {
                                                copyManifest.Add(inclineDestPath, sourcePath);
                                            }
                                        }
                                        if (bAutoCreateEllipticalFiles)
                                        {
                                            if (!copyManifest.ContainsKey(ellipticalDestPath))
                                            {
                                                copyManifest.Add(ellipticalDestPath, sourcePath);
                                            }
                                        }
                                    }
                                    catch (ArgumentException ae) //sourcePath already exists in manifest
                                    {
                                        if (ae != null)
                                        {
                                            MessageBox.Show(ae.ToString(), "Copy Manifest Exception");
                                        }
                                    }
                            }
                            else
                            {
                                Uri partUriDocumentWave = PackUriHelper.CreatePartUri(new Uri("iFit\\Tread\\" + sL.fileBody + ".wav", UriKind.Relative));
                                Uri partUriDocumentWaveBike = PackUriHelper.CreatePartUri(new Uri("iFit\\Bike\\" + sL.fileBody + ".wav", UriKind.Relative));
                                Uri partUriDocumentWaveIncline = PackUriHelper.CreatePartUri(new Uri("iFit\\Incline\\" + sL.fileBody + ".wav", UriKind.Relative));
                                Uri partUriDocumentWaveElliptical = PackUriHelper.CreatePartUri(new Uri("iFit\\Elliptic\\" + sL.fileBody + ".wav", UriKind.Relative));
                                PackagePart packagePartDocumentWave = zipFile.CreatePart(partUriDocumentWave, System.Net.Mime.MediaTypeNames.Application.Octet, compressionOption);
                                PackagePart packagePartDocumentWaveBike = zipFile.CreatePart(partUriDocumentWaveBike, System.Net.Mime.MediaTypeNames.Application.Octet, compressionOption);
                                PackagePart packagePartDocumentWaveIncline = zipFile.CreatePart(partUriDocumentWaveIncline, System.Net.Mime.MediaTypeNames.Application.Octet, compressionOption);
                                PackagePart packagePartDocumentWaveElliptical = zipFile.CreatePart(partUriDocumentWaveElliptical, System.Net.Mime.MediaTypeNames.Application.Octet, compressionOption);


                                writePackagePart(packagePartDocumentWave.GetStream(), sourcePath);
                                if (bAutoCreateBikeFiles)
                                {
                                    writePackagePart(packagePartDocumentWaveBike.GetStream(), sourcePath);
                                }
                                if (bAutoCreateInclineFiles)
                                {
                                    writePackagePart(packagePartDocumentWaveIncline.GetStream(), sourcePath);
                                }
                                if (bAutoCreateEllipticalFiles)
                                {
                                    writePackagePart(packagePartDocumentWaveElliptical.GetStream(), sourcePath);
                                }
                            }
                        }
                        catch (InvalidOperationException ioe)
                        {
                            //part already exists, so we just move on
                            //no need to delete the part since the .zip file is being created new each time
                            System.Console.Error.WriteLine(ioe.ToString());
                        }
                        catch (Exception except)
                        {
                            MessageBox.Show(except.ToString(), "Failure copying wave file: " + sourcePath + " to " + destPath);
                            return;
                        }

                    }

                }
                if (!bZipping)
                {
                    writeBinaryFile(path, buffer);
                    if (bAutoCreateBikeFiles)
                    {
                        writeBinaryFile(bikePath, buffer);
                    }
                    if (bAutoCreateInclineFiles)
                    {
                        writeBinaryFile(inclinePath, buffer);
                    }
                    if (bAutoCreateEllipticalFiles)
                    {
                        writeBinaryFile(ellipticalPath, buffer);
                    }
                }
                else
                {
                   
                    //writing the actual sound fit file to the archive
                    //copying of .wav files is handled above earlier within this method


                    Uri partUriDocumentSf = PackUriHelper.CreatePartUri(new Uri("iFit\\Tread\\" + sF.Name + ".fit", UriKind.Relative));
                    Uri partUriDocumentSfBike = PackUriHelper.CreatePartUri(new Uri("iFit\\Bike\\" + sF.Name + ".fit", UriKind.Relative));
                    Uri partUriDocumentSfIncline = PackUriHelper.CreatePartUri(new Uri("iFit\\Incline\\" + sF.Name + ".fit", UriKind.Relative));
                    Uri partUriDocumentSfElliptical = PackUriHelper.CreatePartUri(new Uri("iFit\\Elliptic\\" + sF.Name + ".fit", UriKind.Relative));

                    PackagePart packagePartDocumentSf = zipFile.CreatePart(partUriDocumentSf, System.Net.Mime.MediaTypeNames.Application.Octet, compressionOption);
                    PackagePart packagePartDocumentSfBike = zipFile.CreatePart(partUriDocumentSfBike, System.Net.Mime.MediaTypeNames.Application.Octet, compressionOption);
                    PackagePart packagePartDocumentSfIncline = zipFile.CreatePart(partUriDocumentSfIncline, System.Net.Mime.MediaTypeNames.Application.Octet, compressionOption);
                    PackagePart packagePartDocumentSfElliptical = zipFile.CreatePart(partUriDocumentSfElliptical, System.Net.Mime.MediaTypeNames.Application.Octet, compressionOption);
                    

                    writePackagePart(packagePartDocumentSf.GetStream(), buffer);
                    if (bAutoCreateBikeFiles)
                    {
                        writePackagePart(packagePartDocumentSfBike.GetStream(), buffer);
                    }
                    if (bAutoCreateInclineFiles)
                    {
                        writePackagePart(packagePartDocumentSfIncline.GetStream(), buffer);
                    }
                    if (bAutoCreateEllipticalFiles)
                    {
                        writePackagePart(packagePartDocumentSfElliptical.GetStream(), buffer);
                    }

                  

                }

            }

            double fileCounter = 0;
            double counterIncrement = 100.0 / copyManifest.Count;
            int whichFile = 0;
            foreach(KeyValuePair<string,string> kvp in copyManifest){
                whichFile++;

                fileCounter += counterIncrement;
                fileGenerationStatusString = "Copying Wave File " + whichFile.ToString() + " of " + copyManifest.Count.ToString() + "...";
                _backgroundWorker_GenFiles.ReportProgress((int)fileCounter);
                try
                {
                    File.Copy(kvp.Value, kvp.Key, true);
                }
                catch (Exception e)
                {
                    if (!e.ToString().Contains("being used by another process"))
                    {
                        MessageBox.Show(e.ToString(), "File Copy Exception");
                    }
                }



            }



        }

      


        public void generateLayoutFit()
        {
            //called by background thread
            List<byte> buffer = new List<byte>(9 * (workoutLength * 4 + 20));

            //now for the layout.fit file
            string path = this.outputDirectory + "\\iFit\\Tread\\layout.fit";
            string bikePath = this.outputDirectory + "\\iFit\\Bike\\layout.fit";
            string inclinePath = this.outputDirectory + "\\iFit\\Incline\\layout.fit";
            string ellipticalPath = this.outputDirectory + "\\iFit\\Elliptic\\layout.fit";
            buffer.Clear();
            foreach (LayoutLine ll in lF)
            {
                foreach (byte b in ll)
                {
                    buffer.Add(b);

                }
            }
            if (!bZipping)
            {
                writeBinaryFile(path, buffer);
                if (bAutoCreateBikeFiles)
                {
                    writeBinaryFile(bikePath, buffer);
                }
                if (bAutoCreateInclineFiles)
                {
                    writeBinaryFile(inclinePath, buffer);
                }
                if (bAutoCreateEllipticalFiles)
                {
                    writeBinaryFile(ellipticalPath, buffer);
                }
            }
            else
            {
               
                // zipping the layout.fit file into the zip archive

                Uri partUriDocumentLf = PackUriHelper.CreatePartUri(new Uri("iFit\\Tread\\layout.fit", UriKind.Relative));
                Uri partUriDocumentLfBike = PackUriHelper.CreatePartUri(new Uri("iFit\\Bike\\layout.fit", UriKind.Relative));
                Uri partUriDocumentLfIncline = PackUriHelper.CreatePartUri(new Uri("iFit\\Incline\\layout.fit", UriKind.Relative));
                Uri partUriDocumentLfElliptical = PackUriHelper.CreatePartUri(new Uri("iFit\\Elliptic\\layout.fit", UriKind.Relative));

                PackagePart packagePartDocumentLf = zipFile.CreatePart(partUriDocumentLf, System.Net.Mime.MediaTypeNames.Application.Octet, compressionOption);
                PackagePart packagePartDocumentLfBike = zipFile.CreatePart(partUriDocumentLfBike, System.Net.Mime.MediaTypeNames.Application.Octet, compressionOption);
                PackagePart packagePartDocumentLfIncline = zipFile.CreatePart(partUriDocumentLfIncline, System.Net.Mime.MediaTypeNames.Application.Octet, compressionOption);
                PackagePart packagePartDocumentLfElliptical = zipFile.CreatePart(partUriDocumentLfElliptical, System.Net.Mime.MediaTypeNames.Application.Octet, compressionOption);

                writePackagePart(packagePartDocumentLf.GetStream(), buffer);
                if (bAutoCreateBikeFiles)
                {
                    writePackagePart(packagePartDocumentLfBike.GetStream(), buffer);
                }
                if (bAutoCreateInclineFiles)
                {
                    writePackagePart(packagePartDocumentLfIncline.GetStream(), buffer);
                }
                if (bAutoCreateEllipticalFiles)
                {
                    writePackagePart(packagePartDocumentLfElliptical.GetStream(), buffer);
                }


            }

        }

        
        void _backgroundWorker_GenFiles_DoWork(object sender, DoWorkEventArgs e)
        {

      
            fileGenerationStatusString = "Generating binary workout files...";
            _backgroundWorker_GenFiles.ReportProgress(0);
           
            //todo: modify generateBinaryFits() to act according to bZipping boolean
            generateBinaryFits();
           
            fileGenerationStatusString = "Generating sound index files...";
            _backgroundWorker_GenFiles.ReportProgress(50);


            //todo: modify generateSoundFits() to act according to bZipping boolean
            generateSoundFits();
            fileGenerationStatusString = "Generating layout.fit...";
            _backgroundWorker_GenFiles.ReportProgress(85);


            //todo: modify generateLayoutFit() to act according to bZipping boolean
            generateLayoutFit();
            fileGenerationStatusString = "Done generating files...";
            _backgroundWorker_GenFiles.ReportProgress(100);
            
            bFileGenerationInProgress = false;
        }

    
        void _backgroundWorker_GenFiles_RunWorkerCompleted(
            object sender,
            RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("File Generation Cancelled" + " while " + fileGenerationStatusString);
                bZipping = false;
                if (zipFile != null)
                {
                    zipFile.Close();
                }
            }
            else if (e.Error != null)
            {
                MessageBox.Show(e.ToString(), "File Generation Exception" + " while " + fileGenerationStatusString);
                bZipping = false;
                if (zipFile != null)
                {
                    zipFile.Close();
                }
            }
            else
            {
              //  MessageBox.Show("Finished generating files.  You can find them in "+outputDirectory+"\\iFit\\Tread - Be sure to copy the iFit folder and all subfolders and files over to the root directory of your SD card if you didn't set your SD card's root directory as the output directory in the Settings menu.", "File Generation Complete");
                bFileGenerationInProgress = false;
                statusWin.bFileGenerationInProgress = false;
                statusWin.Close();
            //    _backgroundWorker_GenFiles = null;

                bZipping = false;
                if (zipFile != null)
                {
                    zipFile.Close();
                }
               
            }
        }


        private void insertTTWsForAllSpeedAndInclineAdjustments()
        {
            bTTWInsertionInProgress = true;
            ttwStatusWin = new FileGenStatusWindow();

            _backgroundWorker_InsertTTWs.RunWorkerAsync(5000);
            ttwStatusWin.Title = "Insert Text-To-Wave Files";
            ttwStatusWin.ShowDialog();

        }
           
        
        
        public void generateFiles()
        {
            bFileGenerationInProgress = true;

          
            statusWin = new FileGenStatusWindow();

            bAsync = false;
            adjustHeaderBlocks();
           
            // Run the Background Worker
            _backgroundWorker_GenFiles.RunWorkerAsync(5000);
           
           
            statusWin.ShowDialog();

            
              
            
            //MessageBox.Show("Your files are being generated in a separate background thread.  You will be notified when the process is completed.  Please wait for them to finish before exiting or making any further changes to your workouts.", "File Generation Began", MessageBoxButton.OK, MessageBoxImage.Information);
      
       }

        void _backgroundWorker_GenFiles_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
             statusWin.UpdateStatus(fileGenerationStatusString);
             statusWin.progressBar.Value = e.ProgressPercentage;
           // statusWin.UpdateStatus(System.DateTime.Now.ToString());


          
        }


        

     

        private void writeTextFile(string path, string textBuf)
        {
            try
            {
                using (StreamWriter textWriter =
                    new StreamWriter(File.Open(path, FileMode.Create)))
                {


                    textWriter.Write(textBuf);

                }

            }
            catch (EndOfStreamException e)
            {
                MessageBox.Show(e.ToString(), "Error writing text file: " + path);

            }
  
        }
        
        
        
        
        
        
        private void generateFiles_Click(object sender, RoutedEventArgs e)
        {


            
          generateFiles();
            
            
        }

        private void writePackagePart(Stream stream, string sourcePath)
        {
            List<byte> buffer = new List<byte>(4096);
            using (FileStream fStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
            {
                
                int b=-1;
                bool bEndOfStream = false;
                while (!bEndOfStream)
                {
                    try
                    {
                        b = fStream.ReadByte();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString(), "Exception while writing to .zip archive");
                    }
                    if (b != -1)
                    {
                        buffer.Add((byte)b);
                    }
                    else
                    {
                        bEndOfStream = true;
                        break;
                    }
                }
                writePackagePart(stream, buffer);

            }


        }
        
        private void writePackagePart(Stream stream, List<byte> buffer)
        {
            foreach (Byte b in buffer)
            {
                stream.WriteByte(b);
            }
        }

        private void writeBinaryFile(string path, List<byte> buffer)
        {

            try
            {
                using (BinaryWriter binWriter =
                    new BinaryWriter(File.Open(path, FileMode.Create)))
                {



                    foreach (Byte b in buffer)
                    {
                        binWriter.Write((byte)b);
                    }

                }

            }
            catch (EndOfStreamException e)
            {
                MessageBox.Show(e.ToString(), "Error writing binary file: " + path);

            }
            catch (Exception except)
            {
                MessageBox.Show(except.ToString(), "General Exception Handler");

            }
            finally
            {



            }


        }



  

      

        void mp_MediaEnded(object sender, EventArgs e)
        {
           
            MediaPlayer mp = (MediaPlayer)sender;
            
            mp.Close();
            listBox1.Focus();
              
           
        }

        private void recordButton_Click(object sender, RoutedEventArgs e)
        {
            
            enableStopButton();
            ListBoxItem li = (ListBoxItem)listBox1.SelectedItem;
            bRecording = true;
            listBox1_MouseDoubleClickAddNewWaveFile(li, null);
            bRecording = false;
            listBox1.Focus();

     
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
           // MessageBox.Show("Stop Button Clicked");
            
    //            mp3Capture.Stop();

                disableAllButtons();
                listBox1.Focus();
        }

        private void reRecordButton_Click(object sender, RoutedEventArgs e)
        {

           
            enableStopButton();
          

            ListBoxItem li = (ListBoxItem)listBox1.SelectedItem;
            CommandBlock cb = (CommandBlock)li.Tag;
            string path = lF.sFs[wIdx].GetPathFromBody(cb.CurrentWaveFileName);

           
       //       mp3Capture.CaptureDevice = SoundCaptureDevice.Default;
       //       mp3Capture.OutputType = Mp3SoundCapture.Outputs.Wav;
        //      mp3Capture.WaveFormat = PcmSoundFormat.Pcm8kHz8bitMono;
        //      mp3Capture.Mp3BitRate = Mp3BitRate.BitRate64;
            //  mp3Capture.NormalizeVolume = true;
            //  mp3Capture.WaitOnStop = false;
       //       mp3Capture.Stopped += new EventHandler<Mp3SoundCapture.StoppedEventArgs>(mp3Capture_Stopped);

              MessageBoxResult result = MessageBox.Show("Replace contents of " + path + " with a new recording?", "Replace File With New Recording?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
              if (result != MessageBoxResult.Yes)
              {
                  disableStopButton();
                  listBox1.Focus();
                  return;
              }
              System.Console.Beep();
       //       mp3Capture.Start(path);
              listBox1.Focus();
          
        }

   

        private void fromDiskButton_Click(object sender, RoutedEventArgs e)
        {

            
            ListBoxItem li = (ListBoxItem)listBox1.SelectedItem;
    

            
            listBox1_MouseDoubleClickAddNewWaveFile(li, null);
            listBox1.Focus();
            
        }

   
        private void textToWaveButton_Click(object sender, RoutedEventArgs e)
        {


  
            ListBoxItem li = (ListBoxItem)listBox1.SelectedItem;
            bTexting = true;
            listBox1_MouseDoubleClickAddNewWaveFile(li, null);
            bTexting = false;
            listBox1.Focus();


        }

        private void reTextToWaveButton_Click(object sender, RoutedEventArgs e)
        {

            disableAllButtons();
            ListBoxItem li = (ListBoxItem)listBox1.SelectedItem;
            CommandBlock cb = (CommandBlock)li.Tag;
            string path = lF.sFs[wIdx].GetPathFromBody(cb.CurrentWaveFileName);

            MessageBoxResult result = MessageBox.Show("Replace contents of " + path + " with a new recording?", "Replace File With New Recording?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                listBox1.Focus();
                return;
            }

            TextToSpeechWindow tts = new TextToSpeechWindow();
            tts.PathToWav = path;


            tts.ShowDialog();
            if (tts.result == false) //user canceled
            {
                listBox1.Focus();
                return;
            }



        }

        private void graduationItem_Click(object sender, RoutedEventArgs e)
        {
            disableSliderFocusOnMouseEnter = true;
            Graduation win = new Graduation();
            win.numWorkouts = this.numWorkouts;
            for (int ii = 0; ii < numWorkouts; ii++)
            {
                win.templateListBox.Items.Add(lF.bFs[ii].Name);
            }
            win.templateListBox.SelectedIndex = wIdx;
            win.templateListBox.Focus();

            win.numGraduatesListBox.Items.Clear();
            for (int ii = this.numWorkouts - 1; ii > win.templateListBox.SelectedIndex; ii--)
            {
                win.numGraduatesListBox.Items.Add((numWorkouts - ii).ToString());
            }
            win.numGraduatesListBox.SelectedIndex = win.numGraduatesListBox.Items.Count - 1;
            
            for (double dd = -2.0; dd<=2.0; dd+=0.1)
            {
                dd=System.Math.Round(dd, 1);
                string str = String.Format("{0:N1}", dd);
                win.speedGradientListBox.Items.Add(str);
            }
            win.speedGradientListBox.SelectedItem = "0.0";
            win.speedGradientListBox.ScrollIntoView("0.7");

            for (double dd = 0.00; dd <= 0.09; dd += 0.01)
            {
                dd = System.Math.Round(dd, 3);
                string str = String.Format("{0:N2}", dd);
                win.speedGradientPlusListBox.Items.Add(str);
            }
            win.speedGradientPlusListBox.SelectedIndex = 0;

            for (double dd = -2.0; dd <= 2.0; dd += 0.1)
            {
                dd = System.Math.Round(dd, 1);
                string str = String.Format("{0:N1}", dd);
                win.inclineGradientListBox.Items.Add(str);
            }
            win.inclineGradientListBox.SelectedItem = "0.0";
            win.inclineGradientListBox.ScrollIntoView("0.7");
            
            
            win.ShowDialog();

            if (win.bResult != true) //user canceled
            {
                disableSliderFocusOnMouseEnter = false;
                return;
            }

        //user didn't cancel, so here we go

            double inclineGradient = System.Convert.ToDouble(win.inclineGradientListBox.SelectedItem);
            double speedGradient = System.Convert.ToDouble(win.speedGradientListBox.SelectedItem);
            speedGradient += System.Convert.ToDouble(win.speedGradientPlusListBox.SelectedItem);
            int workoutTemplateIndex = win.templateListBox.SelectedIndex;
            int numGraduates = win.numGraduatesListBox.SelectedIndex+1;

            for (int ii = 1; ii <= numGraduates; ii++)
            {
                string sfName= lF.sFs[workoutTemplateIndex + ii].Name;
                lF.sFs[workoutTemplateIndex + ii] = lF.sFs[workoutTemplateIndex].Clone(sfName);
                string bfName = lF.bFs[workoutTemplateIndex + ii].Name;
                lF.bFs[workoutTemplateIndex + ii] = lF.bFs[workoutTemplateIndex].Clone(bfName, lF.bFs[workoutTemplateIndex + ii],System.Math.Round(speedGradient*ii,1),roundToNearestPoint5(inclineGradient*ii));




            }
        
            
            disableSliderFocusOnMouseEnter = false;

        }

        private double roundToNearestPoint5(double val) //take a double and round to nearest 0.5
        {
        
            double floor = System.Math.Floor(val);
            double fraction = val - floor;
            double ret;
            fraction = System.Math.Round(fraction, 1);
            if (fraction >= 0.3 && fraction <= 0.7)
            {
                fraction = 0.5;
            }
            else if (fraction <=0.2)
            {
                fraction = 0.0;
            }
            else if (fraction >= 0.8)
            {
                fraction = 1.0;
            }

            ret = floor + fraction;
            return ret;
        }

        private double roundToNearestPoint1(double val) //take a double and round to nearest 0.5
        {

            double floor = System.Math.Floor(val);
            double fraction = val - floor;
            double ret;
            fraction = System.Math.Round(fraction, 2);
            if (fraction <= 0.4)
            {
                fraction = 0.0;
            }
            else if (fraction >= 0.5)
            {
                fraction = 1.0;
            }

            ret = floor + fraction;
            return ret;
        }
        
        
        
        private void maxSpeedListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string selected = maxSpeedListBox.SelectedItem.ToString();
                selected = selected.Substring(selected.Length - 2, 2);
                maxSpeed = System.Convert.ToDouble(selected);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
           SaveApplicationSettings();
           init();
        }

        private void maxInclineListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string selected = maxInclineListBox.SelectedItem.ToString();
                selected = selected.Substring(selected.Length - 2, 2);
                maxIncline = System.Convert.ToDouble(selected);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            SaveApplicationSettings();
            init();
        }

        private void helpAboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
         
            AboutWindow about = new AboutWindow();
            about.versionLabel.Content = versionString;
            about.copyrightLabel.Content = "(C) 2009 Mark Ganson";
            about.ShowDialog();





        }

        private void graduatableLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Slider2 s = currentSmallSpeedSlider;
            s.OwnerBlock.IsGraduatable = !s.OwnerBlock.IsGraduatable;
            updateUILabels();
            listBox1.Focus();
            s.Focus();
        }

        private void workoutNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (workoutNameTextBox.IsFocused)
            {
                newWorkoutNameLabel.Foreground = Brushes.White;
            }
            string pads = "";
            for (int ii = 0; ii < 8 - workoutNameTextBox.Text.Length; ii++)
            {
                pads += "_";
            }
            newWorkoutNameLabel.Content = "New Name: " + "W"+validateWorkoutName(workoutNameTextBox.Text.ToUpper()+pads).Substring(1,7);
            
        }

        private void workoutNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                nameWorkout();
            }
            
        }

        private void nameWorkout()
        {
            string pads = "";
            for (int ii = 0; ii < 8 - workoutNameTextBox.Text.Length; ii++)
            {
                pads += "_";
            }
            //workout name must have exactly 8 characters in it
            string candidate =  validateWorkoutName(workoutNameTextBox.Text.ToUpper()+pads);
            candidate = "W" + candidate.Substring(1, 7); //ensure this name begins with a "W"
            if (candidate.Length != 8)
            {
                MessageBox.Show("The new workout name must have exactly 8 characters.", "Invalid Workout Name", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //name appears to be valid
            //check for conflicts with other names
            for (int ii = 0; ii < lF.sFs.Count; ii++)
            {
                if (ii != wIdx && lF.sFs[ii].Name == candidate)
                {
                    MessageBox.Show("Workout name is already in use.  Try another name.", "Naming Conflict Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

            }

            for (int ii = 0; ii < lF.bFs.Count; ii++)
            {
                if (ii != wIdx && lF.bFs[ii].Name == candidate)
                {
                    MessageBox.Show("Workout name is already in use.  Try another name.", "Naming Conflict Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

            }



            lF.bFs[wIdx].Name = candidate;
            lF.sFs[wIdx].Name = "S" + candidate.Substring(1, 7);
            LayoutLine l = new LayoutLine(candidate, "S"+candidate.Substring(1,7));
            lF.Replace(wIdx, l);
            BuildWorkoutMenus();
            setWindowTitle();
            newWorkoutNameLabel.Foreground = Brushes.Transparent;
        }

        private void workoutNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            newWorkoutNameLabel.Foreground = Brushes.White;
            workoutNameTextBox.Background = Brushes.White;
            workoutNameTextBox.Foreground = Brushes.Black;
        }

        private void workoutNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            newWorkoutNameLabel.Foreground = Brushes.Transparent;
            workoutNameTextBox.BorderBrush = Brushes.Transparent;
            workoutNameTextBox.Foreground = Brushes.White;
            workoutNameTextBox.Background = Brushes.Transparent;
            nameWorkout();
            
        }

        string validateWorkoutName(string candidateName)
        {
            string newName = candidateName;
            char[] chars = newName.ToCharArray();
            for (int ii = 0; ii < chars.Length; ii++)
            {
                if ('A' > chars[ii] || 'Z' < chars[ii] )
                {
                    if ('0' > chars[ii]|| chars[ii] > '9')
                    {
                        if (chars[ii] != '-' && chars[ii] != '_')
                        {
                            chars[ii] = '_';
                        }
                    }   
                }
            }
            chars[0] = 'W';
            newName = new String(chars);

            return newName;
        }

      

        private void button_MouseEnter(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                listBox1.SelectedIndex = 0;
            }
            listBox1.Focus();
        }

        private void intensityCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            bScaleIntensityCanvas = !bScaleIntensityCanvas;
            updateUILabels();
        }

  
      
    }
}
