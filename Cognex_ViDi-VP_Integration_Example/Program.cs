using Cognex.VisionPro;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.ImageProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cognex_ViDi_VP_Integration_Example
{
    class Program
    {
        static void Main(string[] args)
        {
            // load image
            var bmp = new System.Drawing.Bitmap(@"D:\test.bmp");

            #region Using Vision Pro
            //vision pro tool initializing
            CogCaliperTool calipertool = new CogCaliperTool();
            //or load .vpp file (specific tool vpp)
            calipertool = CogSerializer.LoadObjectFromFile(@"D:\CaliperTool.vpp") as CogCaliperTool;

            //input processing image 
            calipertool.InputImage = new CogImage8Grey(bmp);

            //process
            calipertool.Run();

            //create result record
            var caliper_record = calipertool.CreateLastRunRecord().SubRecords[0];

            //use result
            Console.Write("Edge Position is : X = {0}, Y = {1}", calipertool.Results[0].PositionX, calipertool.Results[0].PositionY);

            //save current tool (it saved specific tool vpp)
            CogSerializer.SaveObjectToFile(calipertool, @"D:\CaliperTool_Saved.vpp");
            #endregion


            #region Using ViDi in Runtime

            //open ViDi control (it has to open only one in the application.)
            ViDi2.Runtime.Local.Control runtime_control = new ViDi2.Runtime.Local.Control();
            //add runtime workspace (and open)
            ViDi2.Runtime.IWorkspace runtime_workspace = runtime_control.Workspaces.Add("TestWorkspace", @"D:\DeeplearningWorkspace.vrws");
            //select stream in workspace
            ViDi2.Runtime.IStream runtime_stream = runtime_workspace.Streams.Single(s => s.Name.Equals("Stream"));


            //pack image for use ViDi 
            ViDi2.IImage runtime_ViDiImg = new ViDi2.FormsImage(bmp);
            //process
            ViDi2.ISample runtime_sample = runtime_stream.Process(runtime_ViDiImg);

            //using red result
            ViDi2.IRedMarking redMarking = runtime_sample.Markings["Analyze"] as ViDi2.IRedMarking;

            //marking has a value for each view result
            var r_score = redMarking.Views[0].Score;
            var r_region = redMarking.Views[0].Regions;
            var r_pose = redMarking.Views[0].Pose;

            //using blue result
            ViDi2.IBlueMarking blueMarking = runtime_sample.Markings["Locate"] as ViDi2.IBlueMarking;
            
            var b_features = blueMarking.Views[0].Features;
            var b_pose = blueMarking.Views[0].Pose;

            //using green result
            ViDi2.IGreenMarking greenMarking = runtime_sample.Markings["Classify"] as ViDi2.IGreenMarking;

            var g_best_tag = greenMarking.Views[0].BestTag;
            var g_tags = greenMarking.Views[0].Tags;

            //free vidi image
            runtime_ViDiImg.Dispose();

            //close workspace
            runtime_workspace.Close();

            //close vidi control
            runtime_control.Dispose();
            #endregion


            #region Using ViDi - Vision Pro Integration
            //open ViDi control (it has to open only one in the application.)
            ViDi2.Runtime.Local.Control integ_control = new ViDi2.Runtime.Local.Control();
            //add runtime workspace (and open)
            ViDi2.Runtime.IWorkspace integ_workspace = integ_control.Workspaces.Add("TestWorkspace", @"D:\DeeplearningWorkspace.vrws");
            //select stream in workspace
            ViDi2.Runtime.IStream integ_stream = integ_workspace.Streams.Single(s => s.Name.Equals("Stream"));

            //vision pro tool initializing
            CogAffineTransformTool transform_tool = new CogAffineTransformTool();
            //or load .vpp file (specific tool vpp)
            transform_tool = CogSerializer.LoadObjectFromFile(@"D:\TransformTool.vpp") as CogAffineTransformTool;

            //input processing image          
            transform_tool.InputImage = new CogImage8Grey(bmp);
            //process Vision Pro
            transform_tool.Run();

            //pack vison pro output image for use ViDi 
            ViDi2.IImage integ_ViDiImg = new ViDi2.VisionPro.Image(transform_tool.OutputImage);
            //process ViDi
            var integ_sample = integ_stream.Process(integ_ViDiImg);

            //create vision pro record
            var vp_record = transform_tool.CreateLastRunRecord().SubRecords[0];

            //create ViDi record
            var integ_redMarking = integ_sample.Markings["Analyze"] as ViDi2.IRedMarking;
            var vidi_red_record = new ViDi2.VisionPro.RedViewRecord(integ_redMarking.Views[0] as ViDi2.IRedView, new ViDi2.VisionPro.Records.DefaultRedToolGraphicCreator());
            
            var integ_blueMarking = integ_sample.Markings["Locate"] as ViDi2.IBlueMarking;
            var vidi_blue_record = new ViDi2.VisionPro.BlueViewRecord(integ_blueMarking.Views[0] as ViDi2.IBlueView, new ViDi2.VisionPro.Records.DefaultBlueToolGraphicCreator());
            
            var integ_greenMarking = integ_sample.Markings["Classify"] as ViDi2.IGreenMarking;
            var vidi_green_record = new ViDi2.VisionPro.GreenViewRecord(integ_greenMarking.Views[0] as ViDi2.IGreenView, new ViDi2.VisionPro.Records.DefaultGreenToolGraphicCreator());

            //free vidi image
            integ_ViDiImg.Dispose();
            //close workspace
            integ_workspace.Close();
            //close vidi control
            integ_control.Dispose();
            #endregion


            //free image
            bmp.Dispose();
        }
    }
}
