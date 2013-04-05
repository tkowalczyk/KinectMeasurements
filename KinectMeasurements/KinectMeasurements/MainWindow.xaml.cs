using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using KinectMeasurementsLib;
using Microsoft.Kinect;

namespace KinectMeasurements
{
    public partial class MainWindow : Window
    {
        #region Properties
        /// <summary>
        /// Active Kinect sensor.
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Maximum number of skeletons to track.
        /// </summary>
        private const int skeletonCount = 6;

        /// <summary>
        /// Array for all skeletons
        /// </summary>
        private Skeleton[] allSkeletons = new Skeleton[skeletonCount];

        /// <summary>
        /// Array of arrays of contiguous line segements that represent a skeleton.
        /// </summary>
        private static readonly JointType[][] SkeletonSegmentRuns = new JointType[][]
        {
            new JointType[] 
            { 
                JointType.Head, JointType.ShoulderCenter, JointType.HipCenter 
            },
            new JointType[] 
            { 
                JointType.HandLeft, JointType.WristLeft, JointType.ElbowLeft, JointType.ShoulderLeft,
                JointType.ShoulderCenter,
                JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight
            },
            new JointType[]
            {
                JointType.FootLeft, JointType.AnkleLeft, JointType.KneeLeft, JointType.HipLeft,
                JointType.HipCenter,
                JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight
            }
        };

        /// <summary>
        /// Time until skeleton ceases to be highlighted.
        /// </summary>
        private DateTime highlightTime = DateTime.MinValue;

        /// <summary>
        /// The ID of the skeleton to highlight.
        /// </summary>
        private int highlightId = -1;

        /// <summary>
        /// The ID if the skeleton to be tracked.
        /// </summary>
        private int nearestId = -1;
        #endregion

        #region Ctor
        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region Windows events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;

                    this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    this.sensor.SkeletonFrameReady += this.OnSkeletonFrameReady;

                    var parameters = new TransformSmoothParameters
                    {
                        Smoothing = 0.3f,
                        Correction = 0.0f,
                        Prediction = 0.0f,
                        JitterRadius = 1.0f,
                        MaxDeviationRadius = 0.5f
                    };
                    this.sensor.SkeletonStream.Enable(parameters);

                    break;
                }
            }

            if (null != this.sensor)
            {
                try
                {
                    this.sensor.Start();
                    this.tbStatusText.Text = Properties.Resources.KinectConnected;
                    //this.sensor.ElevationAngle = 0;
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.tbStatusText.Text = Properties.Resources.NoKinectReady;
                return;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
                this.sensor = null;
            }
        }
        #endregion

        private void OnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            // Get the frame.
            using (var frame = e.OpenSkeletonFrame())
            {
                // Ensure we have a frame.
                if (frame != null)
                {
                    // Resize the skeletons array if a new size (normally only on first call).
                    if (allSkeletons.Length != frame.SkeletonArrayLength)
                    {
                        allSkeletons = new Skeleton[frame.SkeletonArrayLength];
                    }

                    // Get the skeletons.
                    frame.CopySkeletonDataTo(allSkeletons);

                    // Assume no nearest skeleton and that the nearest skeleton is a long way away.
                    var newNearestId = -1;
                    var nearestDistance2 = double.MaxValue;

                    // Look through the skeletons.
                    foreach (var skeleton in allSkeletons)
                    {
                        // Only consider tracked skeletons.
                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            // Find the distance squared.
                            var distance2 = (skeleton.Position.X * skeleton.Position.X) +
                                (skeleton.Position.Y * skeleton.Position.Y) +
                                (skeleton.Position.Z * skeleton.Position.Z);

                            Logger.Content = skeleton.AngleBetweenJoints(JointType.Spine, JointType.AnkleLeft, JointType.Head).ToString();
                            //Logger.Content = skeleton.AngleBetweenJoints(JointType.ElbowLeft, JointType.ShoulderLeft, JointType.WristLeft).ToString();
                            
                            xTbCor.Text = (skeleton.Joints[JointType.HipCenter].Position.X * 100).ToString();
                            yTbCor.Text = (skeleton.Joints[JointType.HipCenter].Position.Y * 100).ToString();
                            zTbCor.Text = (skeleton.Joints[JointType.HipCenter].Position.Z * 100).ToString();

                            // Is the new distance squared closer than the nearest so far?
                            if (distance2 < nearestDistance2)
                            {
                                // Use the new values.
                                newNearestId = skeleton.TrackingId;
                                nearestDistance2 = distance2;
                            }
                        }
                    }

                    if (this.nearestId != newNearestId)
                    {
                        this.nearestId = newNearestId;
                    }

                    DrawStickMen(allSkeletons);
                }
            }
        }

        #region Private methods
        /// <summary>
        /// Draw an individual skeleton.
        /// </summary>
        /// <param name="skeleton">The skeleton to draw.</param>
        /// <param name="brush">The brush to use.</param>
        /// <param name="thickness">This thickness of the stroke.</param>
        private void DrawStickMan(Skeleton skeleton, Brush brush, int thickness)
        {
            foreach (var run in SkeletonSegmentRuns)
            {
                var next = this.GetJointPoint(skeleton, run[0]);
                for (var i = 1; i < run.Length; i++)
                {
                    var prev = next;
                    next = this.GetJointPoint(skeleton, run[i]);

                    var line = new System.Windows.Shapes.Line
                    {
                        Stroke = brush,
                        StrokeThickness = thickness,
                        X1 = prev.X,
                        Y1 = prev.Y,
                        X2 = next.X,
                        Y2 = next.Y,
                        StrokeEndLineCap = PenLineCap.Round,
                        StrokeStartLineCap = PenLineCap.Round
                    };

                    StickMen.Children.Add(line);
                }
            }
        }

        /// <summary>
        /// Draw stick men for all the tracked skeletons.
        /// </summary>
        /// <param name="skeletons">The skeletons to draw.</param>
        private void DrawStickMen(Skeleton[] skeletons)
        {
            // Remove any previous skeletons.
            StickMen.Children.Clear();

            foreach (var skeleton in skeletons)
            {
                // Only draw tracked skeletons.
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    // Draw a background for the next pass.
                    this.DrawStickMan(skeleton, Brushes.WhiteSmoke, 7);
                }
            }

            foreach (var skeleton in skeletons)
            {
                // Only draw tracked skeletons.
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    var brush = DateTime.UtcNow < this.highlightTime && skeleton.TrackingId == this.highlightId ? Brushes.Red :
                        skeleton.TrackingId == this.nearestId ? Brushes.Black : Brushes.Gray;

                    // Draw the individual skeleton.
                    this.DrawStickMan(skeleton, brush, 3);
                }
            }
        }

        /// <summary>
        /// Convert skeleton joint to a point on the StickMen canvas.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <param name="jointType">The joint to project.</param>
        /// <returns>The projected point.</returns>
        private Point GetJointPoint(Skeleton skeleton, JointType jointType)
        {
            var joint = skeleton.Joints[jointType];

            // Points are centered on the StickMen canvas and scaled according to its height allowing
            // approximately +/- 1.5m from center line.
            var point = new Point
            {
                X = (StickMen.Width / 2) + (StickMen.Height * joint.Position.X / 3),
                Y = (StickMen.Width / 2) - (StickMen.Height * joint.Position.Y / 3)
            };

            return point;
        }
        #endregion
    }
}