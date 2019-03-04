using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Maps;
using Xamarin.Essentials;
using System.Diagnostics;

namespace WingSpan2
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapPage : ContentPage
	{
        double lat;
        double lon;
        string[] addressList = new string[5];
        double distanceZoomedIn = 0.05;
        Xamarin.Forms.Maps.Map map;

        public MapPage ()
		{
			InitializeComponent ();
            getLocationAsync();
		}
       // public async Task showMap()
       public async Task getLocationAsync()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium);
                var location = await Geolocation.GetLocationAsync(request);


                if (location != null)
                {
                    lat = location.Latitude;
                    lon = location.Longitude;
                    Debug.WriteLine("----------------lat: " + lat + ", long:" + lon + "-----------");

                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                    showMapAsync();
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                Debug.WriteLine("-------fnsEx---------");
                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
                Debug.WriteLine("--------fneEx--------");
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
                Debug.WriteLine("--------pEx--------");
            }
            catch (Exception ex)
            {
                // Unable to get location
                Debug.WriteLine("------can't get location----------");
            }
        }


        public async Task showMapAsync()
        {
            try
            {
                  map = new Xamarin.Forms.Maps.Map(
                  MapSpan.FromCenterAndRadius(
                          new Position(lat, lon), Distance.FromMiles(distanceZoomedIn)))
                {
                    IsShowingUser = false,
                    HeightRequest = 100,
                    WidthRequest = 960,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    //HasZoomEnabled = false
                     HasScrollEnabled = false,
                    
                  
                };
                var stack = new StackLayout { Spacing = 0 };
                System.Diagnostics.Debug.WriteLine("Here right now");
                stack.Children.Add(map);
                //stack.Children.Add(slider);
                Content = stack;  //problem here!!! Can't set map without crashing on xamarin live player!
                                  // but it works on the android emulator!
                                  // dropPin(map, lat, lon, "Your Location", );
                                  //   getCoordsAsync();
                dropPin(lat, lon, "Your Location", "home");
                getCoordsAsync();
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error with getting coordinates");
            }
        }

        public void dropPin(double lat1, double lon1, String name, String address)
        {
            var position = new Position(lat1, lon1);
            var pin = new Pin
            {
                Type = PinType.Place,
                Position = position,
                Label = name,
                Address = address
            };
            map.Pins.Add(pin);
        }

        async System.Threading.Tasks.Task getCoordsAsync()
        {
            double lat1 = lat;
            double lon1 = lon;
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    var placemarks = await Geocoding.GetPlacemarksAsync(lat1, lon1);
                    var placemark = placemarks?.FirstOrDefault();
                    if (placemark != null)
                    {
                        var geocodeAddress =
                            $"AdminArea:       {placemark.AdminArea}\n" +
                            $"CountryCode:     {placemark.CountryCode}\n" +
                            $"CountryName:     {placemark.CountryName}\n" +
                            $"FeatureName:     {placemark.FeatureName}\n" +
                            $"Locality:        {placemark.Locality}\n" +
                            $"PostalCode:      {placemark.PostalCode}\n" +
                            $"SubAdminArea:    {placemark.SubAdminArea}\n" +
                            $"SubLocality:     {placemark.SubLocality}\n" +
                            $"SubThoroughfare: {placemark.SubThoroughfare}\n" +
                            $"Thoroughfare:    {placemark.Thoroughfare}\n";
                        //{ placemark.}
                        System.Diagnostics.Debug.WriteLine(geocodeAddress);
                        System.String address = placemark.FeatureName + " " + placemark.Thoroughfare + "\n";
                        System.String cityState = placemark.Locality + ", " + placemark.AdminArea;
                        if (i == 0 || (i > 0 && !(addressList[i + 1].Equals(address + cityState))))
                        {
                            String name = "";
                            if (i == 0) name = "Your Location";
                            
                      /*      addressList[i] = address + cityState;
                            // AddMarkersToMap(lat1, lon1, address, cityState);
                            GroundOverlay g;
                            var house = BitmapDescriptorFactory.FromResource(Resource.Drawable.house);
                            var groundOverlayOptions = new GroundOverlayOptions()
                                                       .InvokeImage(house)
                                                       .Anchor(0, 1)
                                                       .Position(new LatLng(lat1, lon1), 100, 100);
                            g = googleMap.AddGroundOverlay(groundOverlayOptions);*/
                        }
                        else
                        {
                            System.Diagnostics.Debug.Write("There is a repeat: {" + address + cityState + "}");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error with getting coordinates");
                }
                lat1 += .0002;
                //lon1 += .0002;
                lon1 += 0;
            }
        }
    }
}