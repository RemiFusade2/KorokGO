using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class Tools
{
    public static byte[] GetByteArrayFromString(string bitString)
    {
        return System.Text.Encoding.Default.GetBytes(bitString.ToCharArray());
    }

    public static string GetStringFromByteArray(byte[] byteArray)
    {
        return System.Text.Encoding.Default.GetString(byteArray);
    }

    public static void Populate<T>(this T[] arr, T value)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = value;
        }
    }

    public static double getDistanceFromLatLonInM(double lat1, double lon1, double lat2, double lon2)
    {
        double R = 6371; // Radius of the earth in km
        double dLat = UnityEngine.Mathf.Deg2Rad * (lat2 - lat1);
        double dLon = UnityEngine.Mathf.Deg2Rad * (lon2 - lon1);
        double a =
          Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
          Math.Cos(UnityEngine.Mathf.Deg2Rad * (lat1)) * Math.Cos(UnityEngine.Mathf.Deg2Rad * (lat2)) *
          Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
          ;
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        double d = R * c; // Distance in km
        return d * 1000;
    }

    public static double getCompassFromLatLon(double oriLat, double oriLong, double destLat, double destLon)
    {
        double radians = Math.Atan2((destLon - oriLong), (destLat - oriLat));
        return radians;
    }

    public static Quaternion NormalizeQuaternion(float x, float y, float z, float w)
    {
        float lengthD = 1.0f / (w * w + x * x + y * y + z * z);
        w *= lengthD;
        x *= lengthD;
        y *= lengthD;
        z *= lengthD;

        return new Quaternion(x, y, z, w);
    }

    //Changes the sign of the quaternion components. This is not the same as the inverse.
    public static Quaternion InverseSignQuaternion(Quaternion q)
    {
        return new Quaternion(-q.x, -q.y, -q.z, -q.w);
    }

    //Returns true if the two input quaternions are close to each other. This can
    //be used to check whether or not one of two quaternions which are supposed to
    //be very similar but has its component signs reversed (q has the same rotation as
    //-q)
    public static bool AreQuaternionsClose(Quaternion q1, Quaternion q2)
    {
        float dot = Quaternion.Dot(q1, q2);
        if (dot < 0.0f)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
