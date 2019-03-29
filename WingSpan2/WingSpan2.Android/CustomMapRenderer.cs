using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using WingSpan2;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using WingSpan2.Droid;
using Xamarin.Forms.Maps.Android;

[assembly: ExportRenderer(typeof(CustomMap), typeof(WingSpan2.Droid.CustomMapRenderer))]
namespace WingSpan2.Droid
{
    class CustomMapRenderer : MapRenderer, GoogleMap.IInfoWindowAdapter
    {
        List<CustomPin> customPins;
        CustomMap map;

        public CustomMapRenderer(Context context) : base(context)
        {
        }
        protected override void OnElementChanged(Xamarin.Forms.Platform.Android.ElementChangedEventArgs<Map> e)
        {
            base.OnElementChanged(e);
           
            if (e.OldElement != null)
            {
                NativeMap.InfoWindowClick -= OnInfoWindowClick;
            }

            if (e.NewElement != null)
            {
                map = (CustomMap)e.NewElement;
                customPins = map.CustomPins;
                Control.GetMapAsync(this);
            }
        }

        protected override void OnMapReady(GoogleMap map)
        {
            base.OnMapReady(map);

           NativeMap.InfoWindowClick += OnInfoWindowClick;
            NativeMap.SetInfoWindowAdapter(this);
            NativeMap.MarkerClick += OnMarkerClick;
            NativeMap.UiSettings.ZoomControlsEnabled = false;
            NativeMap.UiSettings.ScrollGesturesEnabled = false;
        }

        public void OnMarkerClick(object sender, GoogleMap.MarkerClickEventArgs e)
        {
            e.Marker.ShowInfoWindow();
        }
        protected override MarkerOptions CreateMarker(Pin pin)
        { 
            var marker = new MarkerOptions();
            marker.SetPosition(new LatLng(pin.Position.Latitude, pin.Position.Longitude));
            marker.SetTitle(pin.Label);
            marker.SetSnippet(pin.Address);
            if (pin.Id.ToString() == "hasClicked")
            {
                marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.pinBlue));
            }
            else if (pin.Id.ToString() == "yourLocation")
            {
                marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.pinAq));
            }
            else 
            {
                marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.pin));
            }
            return marker;
        }
        

        // happens when info box is clicked
        void OnInfoWindowClick(object sender, GoogleMap.InfoWindowClickEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("info box clicked");
            var customPin = GetCustomPin(e.Marker);
            if (customPin == null)
            {
              //  throw new Exception("Custom pin not found");
                System.Diagnostics.Debug.WriteLine("pin is null");
                return;
            }

            if (!string.IsNullOrWhiteSpace(customPin.Url))
            {
                System.Diagnostics.Debug.WriteLine("decorate box");
            //     var url = Android.Net.Uri.Parse(customPin.Url);
           //    var intent = new Intent(Intent.ActionView, url);
           //   intent.AddFlags(ActivityFlags.NewTask);
           //   Android.App.Application.Context.StartActivity(intent);
            }
        }
        
        public Android.Views.View GetInfoContents(Marker marker)
        {
            var inflater = Android.App.Application.Context.GetSystemService(Context.LayoutInflaterService) as Android.Views.LayoutInflater;
            if (inflater != null)
            {
                Android.Views.View view;

                var customPin = GetCustomPin(marker);
                
                if (customPin == null)
                {
                    System.Diagnostics.Debug.WriteLine("pin not found"); //when click PIN
                    return null;
                }

                if (customPin.Id.ToString() == "Xamarin")
                {
                    view = inflater.Inflate(Resource.Layout.XamarinMapInfoWindow, null);
                }
                else
                {
                    view = inflater.Inflate(Resource.Layout.MapInfoWindow, null);
                }

                var infoTitle = view.FindViewById<TextView>(Resource.Id.InfoWindowTitle);
                var infoSubtitle = view.FindViewById<TextView>(Resource.Id.InfoWindowSubtitle);

                if (infoTitle != null)
                {
                    infoTitle.Text = marker.Title;
                }
                if (infoSubtitle != null)
                {
                    infoSubtitle.Text = marker.Snippet;
                }

                return view;
            }
            return null;
        }

        public Android.Views.View GetInfoWindow(Marker marker)
        {
            return null;
        }
        
        
        CustomPin GetCustomPin(Marker annotation)
        {
            var position = new Position(annotation.Position.Latitude, annotation.Position.Longitude);
            try
            {
                foreach (var pin in customPins) 
                {
                    if (pin.Position == position)
                    {
                        return pin;
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("problem here with getting pin"); //when click PIN
            }
            return null;
        }
    } 
}