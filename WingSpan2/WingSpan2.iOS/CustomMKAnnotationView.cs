using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using MapKit;
using Xamarin.Forms.Maps;

namespace WingSpan2.iOS
{
    public class CustomMKAnnotationView : MKAnnotationView
    {
        public string Id { get; set; }
        public Position Position { get; set; }
        public string Url { get; set; }

        public CustomMKAnnotationView(IMKAnnotation annotation, string id)
            : base(annotation, id)
        {
        }
    }
}