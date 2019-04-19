using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace WingSpan2
{
    public class CustomMap : Map
    {
        public List<Position> ShapeCoordinates { get; set; }
     //   public List<CustomPin> CustomPins = new List<CustomPin>(); // { get; set; }
        public int polygonPoints;

        public CustomMap()
        {
            ShapeCoordinates = new List<Position>();
            //CustomPins = new List<CustomPin>();
            polygonPoints = 0;
        }
    //    public void addPin(CustomPin p)
    //    {
    //        CustomPins.Add(p);
    //    }
        public int getPolygonPoints()
        {
            return polygonPoints;
        }
        public void setPolygonPoints(int points)
        {
            polygonPoints = points;
        }
    }
}