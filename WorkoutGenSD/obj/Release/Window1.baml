   M S B A M L   `   `   `  ���� [  WSystem.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089Y@clr-namespace:System.Windows.Forms;assembly=System.Windows.FormsSystem.Windows.Forms   WorkoutGenSD   WorkoutGenSD.Window1   ^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Owf@clr-namespace:System.Windows.Forms;assembly=System.Windows.Forms     -   $��
mainWindow��
���   �  � Title$  WorkoutGenSD��$	��580��$	��768�� � WindowStyle$ ThreeDBorderWindow=� � 
ResizeMode$ CanMinimize=� J� SnapsToDevicePixels .  J� 	AllowDrop .  � WindowStartupLocation$ CenterScreen=� � SizeToContent$ Width=�$��Center=�$	��Top=�.��� -   $
��grid��$	��580��$	��748��$��0,0,0,0q�$	��Top=�$
��Left=� . .+��� -   $��listBox1��
������ 
������� K� ZIndex$ 1��$��0,140.028,80.016,276.722q�$��Right=�$	��261��]� -   $��
dockPanel1��$ 1��$��114.32��$	��Top=�$��
-2,0,-16,0q���.  o� -   $
��menu��$ 1��$��22��$	��Top=�$��
-2,0,-40,0q�$
��Left=�$	��Top=� .  .@�m� -   $��fileMenu��$��_File��.@�m� -   $��newLayoutMenuItem��$��_New Layout��m� -   $��openLayoutFitMenuItem��$��_Open Layout.Fit File...��m� -	   $ ��openFromZipArchiveMenuItem��$ ��Open From ._Zip Archive...��m� -
   $��saveAsZipArchiveMenuItem��$��_Save As .Zip Archive...��m� -   $��generateFilesMenuItem��$��_Generate Files��m� -   $��
exportText��$��_Export Text File...��m� -   $
��exit��$��E_xit��m� -   $��editMenu��$��_Edit��.@�m� -   $)��#clearAllExceptActiveWorkoutMenuItem��$&�� _Clear All Except Active Workout��m� -   $-��'stripAllWavesFromCurrentWorkoutMenuItem��$/��)_Strip All Wave Files From Active Workout��m� -   $��stripAllWavesMenuItem��$-��'S_trip All Wave Files From All Workouts��m� $��	_Settings��.@�m� -   $��_Application Settings...��m� -   $ ��autoWarmupCooldownMenuItem��$��A_uto Warmup Cooldown�� m� IsCheckable . m� -   $��autoInsertTTWs��$0��*Auto Insert TTWs At Start Of Each _Workout��	 � ToolTip$`	 ZAutomatically insert a text-to-wave file at start of each workout, e.g. 'workout number 3'�� . m� -   $4��.autoInsertTTWsForAllSpeedAndInclineAdjustments��$9��3Auto _Insert TTWs For All Speed/Incline Adjustments�� . m� -   $!��autoCreateBikeFilesMenuItem��$��Auto Create _Bike Files��$J	 DAutomatically create files for bikes when generating treadmill files�� . m� -   $'��!autoCreateEllipticalFilesMenuItem��$#��Auto Create _Elliptical Files��$X	 RAutomatically create files for elliptical trainers when generating treadmill files�� . m� -   $$��autoCreateInclineFilesMenuItem��$ ��Auto Create _Incline Files��$U	 OAutomatically create files for incline trainers when generating treadmill files�� . m� -   $��maxSpeedMenuItem��$��Maximum Treadmill Speed��.@��� -   $��maxSpeedListBox��.@��� .��4�� .��5�� .��6�� .��7�� .��8�� .��9�� .��10�� .��11�� .��12�� .��13�� .��14�� .��15�� .��16�� .��17�� .��18�� .��19�� .��20m� -   $��maxInclineMenuItem��$��Maximum Treadmill Incline��.@��� -   $��maxInclineListBox��.@��� .��4�� .��5�� .��6�� .��7�� .��8�� .��9�� .��10�� .��11�� .��12�� .��13�� .��14�� .��15�� .��16�� .��17�� .��18�� .��19�� .��20�� .��21�� .��22�� .��23�� .��24�� .��25�� .��26�� .��27�� .��28�� .��29�� .��30�� .��31�� .��32�� .��33�� .��34�� .��35�� .��36�� .��37�� .��38�� .��39�� .��40m� $��_View��.@�m� -   $#��viewHorizontalSlidersMenuItem��$��_Horizontal Sliders�� . m� -   $��activeWorkoutMenu��$��Active _Workout��$5	 /Select the active workout from those available.��m� -   $��graduationItem��$��_Graduate...��$a	 [Takes the active workout and uses it as a template for creating more/less intense workouts.��m� $��_Help��.@�m� -    $��helpUsingMenuItem��$��_Using WorkoutGenSD...��m� -!   $��helpAboutMenuItem��$��	_About...���� -"   $��image1��$ 0��$��
-8,-4,-8,4q�$��background.jpg���� -#   $��fromDiskButton��
������ 
�������$��Right=�$��68��$��23��$	��Top=�.��From Disk...�� -$   $��recordButton��
������ 
�������$��23��$��Right=�$	��Top=�$��68��.��	Record...�� -%   $��
stopButton��
������ 
�������$��23��$��Right=�$	��Top=�$��68��.��Stop�� -&   $��reRecordButton��
������ 
�������$��Right=�$��68��$��23��$	��Top=�.��	Re-Record�� -'   $��playWaveFileButton��$��Right=�
������ 
�������$��68��$��23��$	��Top=�.��Play�� -(   $��textToWaveButton��
������ 
�������$��Right=�$��68��$��23��$	��Top=�.��TTW...�� -)   $��reTextToWaveButton��
������ 
�������$��23��$��Right=�$	��Top=�$��68��$J	 DOverwrites currently selected WAV file with a new text-to-wave file.��.��ReTTW�� -*   $��removeWaveButton��
������ 
�������$��Right=�$��0,0,24.442,257q�$��68��$��23��$	��Top=�.��Remove�� -+   $��workoutNameLabel��$��5,125.025,0,0q�
�������$
��Left=�$��90��$��25��$	��Top=�.��Active Workout:�� -,   $��newWorkoutNameLabel��$��200,125.025,0,0q�
������ $
��Left=�$	��143��$��25��$	��Top=�.��New Name: W�� --   $��workoutNameTextBox��
 �� 	MaxLength$
 8��$��22��
������ $
��Left=�$��89.5,127,0,0q�$	��Top=�$
��72.5��
������ 
�������$<	 6Click here to rename this workout. (8 character limit)���� -.   $��currentIntervalLabel��
������ 
�������$ 1��$��25��$
��Left=�$��5,140.028,0,0q�$	��Top=�$��194.344��.��Label�� -/   $��currentSpeedLabel��
������ 
�������$��25��$
��Left=�$��5,155.031,0,0q�$ 1��$	��Top=�$��194.344��.��Label�� -0   $��currentInclineLabel��
������ 
�������$��25��$
��Left=�$��5,170.034,0,0q�$ 1��$	��Top=�$��194.344��.��Label�� -1   $��currentDistanceLabel��
������ 
�������$��25��$
��Left=�$��5,185.037,0,0q�$ 1��$	��Top=�$��194.344��.��Label�� -2   $��currentLapsLabel��
������ 
�������$��25��$
��Left=�$��5,200.04,0,0q�$ 1��$	��Top=�$��194.344��.��Label�� -3   $��currentPaceLabel��
������ 
�������$
��Left=�$��5,215.043,0,0q�$ 1��$��194.344��$��25��$	��Top=�.��Label�� -4   $��currentCalorieLabel��
������ 
�������$
��Left=�$��5,230.046,0,0q�$ 1��$��194.344��$��25��$	��Top=�.��Label�� -5   $��currentMETLabel��
������ 
�������$
��Left=�$��5,245.049,0,0q�$ 1��$��71��$��24.889��$	��Top=�.��Label� -6   $��intensityBar��
������ 
������ $
��Left=�$��73.348,251.717,0,0q�$��82��$��10��$	��Top=��� -7   $��workoutLengthLabel��
������ 
�������$ 1��$��25��$��200,140.028,353.656,0q�$	��Top=�.��Label�� -8   $��averageSpeedLabel��
������ 
�������$ 1��$��25��$��200,155.031,353.656,0q�$	��Top=�.��Label�� -9   $��averageInclineLabel��
������ 
�������$ 1��$��25��$��200,170.034,353.656,0q�$	��Top=�.��Label�� -:   $��fastestSpeedLabel��
������ 
�������$ 1��$��25��$��200,185.031,353.656,0q�$	��Top=�.��Label�� -;   $��steepestInclineLabel��
������ 
�������$ 1��$��25��$��200,200.034,353.656,0q�$	��Top=�.��Label�� -<   $��workoutDistanceLabel��
������ 
�������$ 1��$��25��$��200,215.375,353.656,0q�$	��Top=�.��Label�� -=   $��workoutLapsLabel��
������ 
�������$ 1��$��25��$��200,230.378,353.656,0q�$	��Top=�.��Label�� ->   $��averagePaceLabel��
������ 
�������$ 1��$��200,245.381,353.656,0q�$��25��$	��Top=�.��Label�� -?   $��totalCaloriesLabel��
������ 
�������$ 1��$��200,260.384,353.656,0q�$��25��$	��Top=�.��Label�� -@   $��userWeightLabel��
������ 
�������$ 1��$!��200,275.387,353.656,286.724q�$��25��.��Label�� -A   $��graduatableLabel��
������ 
�������$��25��$ ��200,290.39,353.656,270.054q�$ 1��$7	 1You can double click here to change this setting.��.��Label�� -B   $��waveFileManagementLabel��$��25��$��0,0,136.694,250.05q�$��Bottom=�
�������$��Right=�$��154.569��.��Wave File Management Area�� -C   $��intensityCanvas��$��12,0,0,238.381q�$
��Left=�$	��150��$��50��$��Bottom=�$)	 #Click me to adjust my scaling mode.���� -D   $��autoWarmupCooldownButton��
������ 
�������$o	 iAutomatically generate an incremental warmup (first 5 minutes) and incremental cool down (last 3 minutes)��$��200.04,0,0,238.381q�$��23.222��$��Bottom=�$
��Left=�$��142.96��.��Auto Warmup/Cooldown� -E   $��myGrid��$ 1��$	��192��$
��Left=�$��Bottom=�$��0,0,0,35q�