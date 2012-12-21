using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace KinectMeasurementsLib
{
    public static class KinectMeasurementsTools
    {
        /// <summary>
        /// Returns the distance of tracked skeleton from Kinect device
        /// </summary>
        /// <param name="skeleton">Skeleton</param>
        /// <returns>Distance in meters</returns>
        public static double GetSkeletonDistance(this Skeleton skeleton)
        {
            return (skeleton.Position.X * skeleton.Position.X) +
                   (skeleton.Position.Y * skeleton.Position.Y) +
                   (skeleton.Position.Z * skeleton.Position.Z);
        }

        /// <summary>
        /// Returns the distance of specific joint from Kinect device
        /// </summary>
        /// <param name="skeleton">Skeleton</param>
        /// <param name="jointType">JointType</param>
        /// <returns>Distance in meters</returns>
        public static double GetJointDistance(this Skeleton skeleton, JointType jointType)
        {
            return (skeleton.Joints[jointType].Position.X * skeleton.Joints[jointType].Position.X) +
                   (skeleton.Joints[jointType].Position.Y * skeleton.Joints[jointType].Position.Y) +
                   (skeleton.Joints[jointType].Position.Z * skeleton.Joints[jointType].Position.Z);
        }

        /// <summary>
        /// Returns the distance between two specific joints
        /// </summary>
        /// <param name="skeleton">Skeleton</param>
        /// <param name="firstJointType">JointType</param>
        /// <param name="secondJointType">JointType</param>
        /// <returns>Distance in meters</returns>
        public static double GetDistanceBetweenJoints(this Skeleton skeleton, JointType firstJointType, JointType secondJointType)
        {
            return Math.Sqrt(
                Math.Pow((skeleton.Joints[firstJointType].Position.X - skeleton.Joints[secondJointType].Position.X), 2) +
                Math.Pow((skeleton.Joints[firstJointType].Position.Y - skeleton.Joints[secondJointType].Position.Y), 2) +
                Math.Pow((skeleton.Joints[firstJointType].Position.Z - skeleton.Joints[secondJointType].Position.Z), 2)
                );
        }

        public static float AngleBetweenTwoVectors(Vector3 vectorA, Vector3 vectorB)
        {
            float dotProduct = 0.0f;
            dotProduct = Vector3.Dot(vectorA, vectorB);

            return (float)Math.Acos(dotProduct);
        }

        #region Skeleton height
        // from http://www.codeproject.com/Articles/380152/Kinect-for-Windows-Find-user-height-accurately

        public static double Height(this Skeleton skeleton)
        {
            const double HEAD_DIVERGENCE = 0.1;

            var head = skeleton.Joints[JointType.Head];
            var neck = skeleton.Joints[JointType.ShoulderCenter];
            var spine = skeleton.Joints[JointType.Spine];
            var waist = skeleton.Joints[JointType.HipCenter];
            var hipLeft = skeleton.Joints[JointType.HipLeft];
            var hipRight = skeleton.Joints[JointType.HipRight];
            var kneeLeft = skeleton.Joints[JointType.KneeLeft];
            var kneeRight = skeleton.Joints[JointType.KneeRight];
            var ankleLeft = skeleton.Joints[JointType.AnkleLeft];
            var ankleRight = skeleton.Joints[JointType.AnkleRight];
            var footLeft = skeleton.Joints[JointType.FootLeft];
            var footRight = skeleton.Joints[JointType.FootRight];

            // Find which leg is tracked more accurately.
            int legLeftTrackedJoints = NumberOfTrackedJoints(hipLeft, kneeLeft, ankleLeft, footLeft);
            int legRightTrackedJoints = NumberOfTrackedJoints(hipRight, kneeRight, ankleRight, footRight);

            double legLength = legLeftTrackedJoints > legRightTrackedJoints ? Length(hipLeft, kneeLeft, ankleLeft, footLeft) : Length(hipRight, kneeRight, ankleRight, footRight);

            return Length(head, neck, spine, waist) + legLength + HEAD_DIVERGENCE;
        }

        private static double Length(Joint p1, Joint p2)
        {
            return Math.Sqrt(
                Math.Pow(p1.Position.X - p2.Position.X, 2) +
                Math.Pow(p1.Position.Y - p2.Position.Y, 2) +
                Math.Pow(p1.Position.Z - p2.Position.Z, 2)
                );
        }

        private static double Length(params Joint[] joints)
        {
            double length = 0;

            for (int index = 0; index < joints.Length - 1; index++)
            {
                length += Length(joints[index], joints[index + 1]);
            }

            return length;
        }

        private static int NumberOfTrackedJoints(params Joint[] joints)
        {
            int trackedJoints = 0;

            foreach (var joint in joints)
            {
                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    trackedJoints++;
                }
            }

            return trackedJoints;
        }
        #endregion
    }
}