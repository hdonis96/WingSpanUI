using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace WingSpan2
{
    public class CustomPin : Pin
    {
        public bool hasClicked { get; set; }
        public bool isYourLocation { get; set; }
        public string Url { get; set; }

        public CustomPin()
        {
        }
    }
}