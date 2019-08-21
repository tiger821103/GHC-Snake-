using System;
using System.Collections.Generic;
using System.Drawing;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Display;
using Gma.UserActivityMonitor;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace snake
{


    public class GHC_snake : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GHC_snake()
          : base("Snake", "Snake",
              "Relax, have fun !",
              "Games", "Games")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Reset", "Re", "Reset the Game", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Left", "Left", "Left", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Right", "Right", "Right", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Up", "Up", "Up", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Down", "Down", "Down", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Boundry", "Boundry", "Boundry", GH_ParamAccess.item);
            pManager.AddRectangleParameter("Mr.Snake", "Mr.Snake", "Mr.Snake", GH_ParamAccess.list);
            pManager.AddRectangleParameter("Fruit", "Fruit", "Fruit", GH_ParamAccess.item);
        }


 


        Point3d currentPosition = new Point3d();
        Vector3d velocity = new Vector3d();
        Random renGen = new Random();
        List<Point3d> history = new List<Point3d>();
        int displayCount;
        Point3d fruit = new Point3d();
        bool gameOver;
        

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool iReset = false;
            bool iLeft = false;
            bool iRight = false;
            bool iUp = false;
            bool iDown = false;

            DA.GetData("Reset", ref iReset);
            DA.GetData("Left", ref iLeft);
            DA.GetData("Right", ref iRight);
            DA.GetData("Up", ref iUp);
            DA.GetData("Down", ref iDown);


            if (iLeft && -velocity != new Vector3d(-1, 0, 0) && gameOver != true)
                velocity = new Vector3d(-1, 0, 0);
            if (iRight && -velocity != new Vector3d(1, 0, 0) && gameOver != true)
                velocity = new Vector3d(1, 0, 0);
            if (iUp && -velocity != new Vector3d(0, 1, 0) && gameOver != true)
                velocity = new Vector3d(0, 1, 0);
            if (iDown && -velocity != new Vector3d(0, -1, 0) && gameOver != true)
                velocity = new Vector3d(0, -1, 0);

            Rectangle3d boundry = new Rectangle3d(Plane.WorldXY, new Point3d(20.1, 20.1, 0), new Point3d(-20.1, -20.1, 0));
            Polyline boundryline = boundry.ToPolyline();

            DA.SetData("Boundry", boundryline);



            if (iReset)
            {
                currentPosition = new Point3d(0.5, 0.5, 0);
                history = new List<Point3d>();
                velocity = new Vector3d(0, 0, 0);
                fruit = new Point3d(renGen.Next(39) - 19.5, renGen.Next(39) - 19.5, 0);
                displayCount = 3;
                gameOver = false;
            }


            else
            {
                currentPosition += velocity;
                history.Add(currentPosition);
            }


            double distance = boundryline.ClosestPoint(currentPosition).DistanceTo(currentPosition);

            if (distance < 0.5)
            {
                currentPosition = new Point3d(0.5, 0.5, 0);
                history = new List<Point3d>();
                velocity = new Vector3d(0, 0, 0);
                fruit = new Point3d(renGen.Next(39) - 19.5, renGen.Next(39) - 19.5, 0);
                gameOver = true; 
            }


            if (currentPosition.DistanceTo(fruit) == 0)
            {
                fruit = new Point3d(renGen.Next(39) - 19.5, renGen.Next(39) - 19.5, 0);
                displayCount += 1;
            }

            if (history.Count > displayCount)
                history.RemoveAt(0);


            Point3d cornerA = new Point3d(fruit.X - 0.5, fruit.Y - 0.5, 0);
            Point3d cornerB = new Point3d(fruit.X + 0.5, fruit.Y + 0.5, 0);
            Plane fruitPlane = new Plane(fruit, new Vector3d(0, 0, 1));
            Rectangle3d fruitBlcok = new Rectangle3d(fruitPlane, cornerA, cornerB);

            DA.SetData(2, fruitBlcok);


            List<Rectangle3d> bodyhistory = new List<Rectangle3d>();

            foreach (Point3d point in history)
            {
                Point3d cornerC = new Point3d(point.X - 0.5, point.Y - 0.5, 0);
                Point3d cornerD = new Point3d(point.X + 0.5, point.Y + 0.5, 0);
                Plane pl = new Plane(point, new Vector3d(0, 0, 1));
                Rectangle3d body = new Rectangle3d(pl, cornerC, cornerD);
                bodyhistory.Add(body);
            }

            DA.SetDataList(1, bodyhistory);


        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            if (gameOver)
            {
                Text3d text1 = new Text3d("Game Over", Plane.WorldXY, 2);
                args.Display.Draw3dText(text1, Color.Black, new Point3d(-5, 2, 0));
                Text3d text2 = new Text3d("Score " + (displayCount-3), Plane.WorldXY, 2);
                args.Display.Draw3dText(text2, Color.Black, new Point3d(-5, -2, 0));
            }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Properties.Resources.snake;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ddcaa141-29e6-4490-a1cd-3c2151c1d0a6"); }
        }
    }
}
