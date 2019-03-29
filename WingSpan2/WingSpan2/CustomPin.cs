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
        public Boolean hasClicked { get; set; }
        public string Url { get; set; }

        public CustomPin()
        {
        }
    }
}