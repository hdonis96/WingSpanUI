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
using System.Collections;

namespace WingSpan2
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        double lat;
        double lon;
        private static int NUM_ADDRESSES = 20;
        int INF = 10000; 
        ArrayList addressInfo = new ArrayList();
        String yourAddress;
        String yourCityState;
        Label loadingPinLabel = new Label { Text = "loading pins...", TextColor = Color.FromHex("#1D8348"), FontSize = 20 };

    public MapPage()
        {
            InitializeComponent();
            var stack = new StackLayout { Spacing = 0 };
            var label = new Label { Text = "Loading..." };
            stack.Children.Add(label);
            Content = stack;
            getLocationAsync();
        }
        // public async Task showMap()
        public async Task getLocationAsync()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Low);
                var location = await Geolocation.GetLocationAsync(request);


                if (location != null)
                {
                    lat = location.Latitude;
                    lon = location.Longitude;
                    Debug.WriteLine("----------------lat: " + lat + ", long:" + lon + "-----------");

                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                    //  showMapAsync(); old
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
                lat = 38.824310;
                lon = -121.282731;
            }
               lat = 38.824310; lon = -121.282731; //rocklin house
            //lat = 38.742248; lon = -121.290019; //roseville house
            //  lat = 38.560923; lon = -121.422378; //<- sac state
            createMapAsync(lat, lon, "Your Location", "home"); 
        }


        public async Task createMapAsync(double lat1, double lon1, String name, String address)
        {
            CustomMap customMap = new CustomMap
            {
                MapType = MapType.Street,
                HeightRequest = 100,
                WidthRequest = 960,
                VerticalOptions = LayoutOptions.FillAndExpand,
            };
            placeMap(customMap);
           // createShape(customMap);
            //lat = up & doqn
            //lon = L & R
            double longStart = lon - 0.0016;
            double latStart = lat + 0.0016;
            await populateAddress(lat, lon, customMap, true, 2); //for your location

            for (int i = 0; i < NUM_ADDRESSES; i++) { 
                for (int j = 0; j < NUM_ADDRESSES; j++) {
                    double lat2 = latStart - (i * .0002);
                    // lat2 += .0005;
                    double long2 = longStart + (j * .0002); //.0004, .0008, .0012, .0016
                    await populateAddress(lat2, long2, customMap, false, j);
                }
            }
            //  createPin(customMap);
            loadingPinLabel.Text = "";
        }
        public void createShape(CustomMap customMap)
        { //38.825171, -121.282382
            //38.825187, -121.281215
            //38.824245, -121.281073
            //38.824242, -121.282343
            customMap.ShapeCoordinates.Add(new Position(38.825171, -121.282382));
            customMap.ShapeCoordinates.Add(new Position(38.825187, -121.281215));
            customMap.ShapeCoordinates.Add(new Position(38.824245, -121.281073));
            customMap.ShapeCoordinates.Add(new Position(38.824242, -121.282343));
        }

        //decide which geolocations are valid and add them to list
        async System.Threading.Tasks.Task populateAddress(double lat1, double lon1, CustomMap customMap, Boolean yourLoc, int index)
        {
            if(index%5==0) loadingPinLabel.Text = "";
            else loadingPinLabel.Text = "loading pins...";
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
                    System.Diagnostics.Debug.WriteLine(geocodeAddress);
                    System.String address = placemark.FeatureName + " " + placemark.Thoroughfare;

                    System.String cityState = placemark.Locality + ", " + placemark.AdminArea;
                    if (!yourLoc && cityState.Equals(yourCityState) && address.Equals(yourAddress)) { return; }
                    String houseInfo = address + " " + placemark.Locality + " " + placemark.AdminArea;
                    if (addressInfo.Contains(houseInfo)) return;
                    if (address.Equals("Unnamed Road Unnamed Road")) return;
                    String id = "Xamarin";
                    if (yourLoc)
                    {
                        id = "yourLocation";
                        yourAddress = address;
                        yourCityState = cityState;
                    }
                    addressInfo.Add(houseInfo);
                    createPinAsync(customMap, lat1, lon1, id, address, cityState);
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.ToString() + "Error with getting coordinates");
            }
        }

        public async Task createPinAsync(CustomMap customMap, double lat1, double lon1, String id, String address, String cityState) {

            try
            {
                var locations = await Geocoding.GetLocationsAsync(address + " " + cityState);
                var location = locations?.FirstOrDefault();
                if (location != null)
                {
                    lat1 = location.Latitude;
                    lon1 = location.Longitude;
                    Debug.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}");
                }
                var pin = new CustomPin
                {
                    Type = PinType.Place,
                    Position = new Position(lat1, lon1),
                    Label = address,
                    Address = cityState,
                    Id = id,
                    Url = "http://xamarin.com/about/",
                    hasClicked = false
                };
                customMap.CustomPins = new List<CustomPin> { pin };
                
                if (id.Equals("Xamarin"))
                {
                    pin.Clicked += async (sender, args) =>
                    {
                        pinClickedAsync(pin, customMap);
                    };
                }
                else
                {
                    pin.Clicked += async (sender, args) =>
                    {
                        popUpInfoAsync(address, cityState);
                    };
                }
                customMap.Pins.Add(pin);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString() + "Error with converting address to coordinates");
            }
        }
            
        public void placeMap(CustomMap customMap)
        {
            var stack = new StackLayout { Spacing = 0 };
            Button buttonZoomIn = new Button
            {
                Text = "+",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HeightRequest = 38

            };
            Button buttonZoomOut = new Button
            {
                Text = "-",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HeightRequest = 38,

            };
            Button buttonLoc = new Button
            {
                Text = "Location",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            };
            Button polygonButton = new Button
            {
                Text = "Get polygon addresses",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,

            };
            polygonButton.Clicked += async (sender, args) =>
            {
                if (customMap.ShapeCoordinates.Count < 4)
                {
                    await DisplayAlert("", "Please select 4 points for the polygon first", "Ok");
                }
                else
                {
                    getPolygonAddresses(customMap);
                }
            };
            buttonZoomIn.Clicked += async (sender, args) =>
            {
                onZoomInClicked(customMap);
            };
            buttonZoomOut.Clicked += async (sender, args) =>
            {
                onZoomOutClicked(customMap);
            };
            
            buttonLoc.Clicked += async (sender, args) =>
            {
                customMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(lat, lon), Distance.FromMiles(0.1))); //1.0
                
                buttonLoc.IsVisible = false;
                stack.Children.Add(buttonZoomIn);
                stack.Children.Add(buttonZoomOut);
                stack.Children.Add(polygonButton);
                stack.Children.Add(loadingPinLabel);
                customMap.HasZoomEnabled = false;
            };
            stack.Children.Add(customMap);
            stack.Children.Add(buttonLoc);
            Content = stack;
            customMap.HasScrollEnabled = false;
            customMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(lat, lon), Distance.FromMiles(10))); //1.0
        }

        //calculate if pins are in the polygon and add them to a list
        private void getPolygonAddresses(CustomMap customMap)
        {
            List<String> polygonAddresses = new List<String>();
            Point[] polygonPoints = new Point[4];
            int i = 0;

            //convert shape coordinates to points
            foreach (Position pos in customMap.ShapeCoordinates)
            {
                Point point = new Point();
                point.x = pos.Latitude;
                point.y = pos.Longitude;
                polygonPoints[i] = point;
                i++;
            }

            //if CustomPin inside polygon: add to polygonAddresses
            foreach (CustomPin pin in customMap)
            {
                Point point = new Point();
                point.x = pin.Position.Latitude;
                point.y = pin.Position.Longitude;

                //   Debug.WriteLine(p + " ");
                if (isInside(polygonPoints, 4, point))
                {
                    polygonAddresses.Add(pin.Label + " " + pin.Address);
                }
            }

            //print out addresses in polygon
            foreach (String add in polygonAddresses)
            {
                Debug.WriteLine("addresses in polygon: " + add);
            }
        }

        private void onZoomInClicked(CustomMap customMap)
        {
            customMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(lat, lon), Distance.FromMiles(0.05))); //1.0
        }
        private void onZoomOutClicked(CustomMap customMap)
        {
            customMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(lat, lon), Distance.FromMiles(0.1))); //1.0
        }


        // when pin info window is clicked, change color and then call pop up info method: 
        private async Task pinClickedAsync(CustomPin pin, CustomMap customMap)
        {
            string address = pin.Label;
            string cityState = pin.Address;
            Position p = pin.Position;
            System.Diagnostics.Debug.WriteLine(address + " " + cityState);
            customMap.Pins.Remove(pin);
            
            var newPin = new CustomPin
            {
                Type = PinType.Place,
                Position = p,
                Label = address,
                Address = cityState,
                Id = "hasClicked",
                Url = "http://xamarin.com/about/",
                hasClicked = true
            };
            customMap.Pins.Add(newPin);
            await popUpInfoAsync(address, cityState);
        }

        //pop up info on a house----
        public async Task popUpInfoAsync(string address, string cityState) { 
            await DisplayAlert("", "-House Information-" + "\n " +  address + "\n" + cityState, "Ok");
            // pop up this info....
        }
        public ArrayList getHouseInfoList()
        {
            return addressInfo;
        }

        //-----------these last functions are to get which pins are inside polygon-------------------

        // Given three colinear points p, q, r, the function checks if 
        // point q lies on line segment 'pr' 
        bool onSegment(Point p, Point q, Point r)
        {
            if (q.x <= max(p.x, r.x) && q.x >= min(p.x, r.x) &&
                    q.y <= max(p.y, r.y) && q.y >= min(p.y, r.y))
                return true;
            return false;
        }

        // To find orientation of ordered triplet (p, q, r). 
        // The function returns following values 
        // 0 --> p, q and r are colinear 
        // 1 --> Clockwise 
        // 2 --> Counterclockwise 
        int orientation(Point p, Point q, Point r)
        {
            double val = (q.y - p.y) * (r.x - q.x) -
                      (q.x - p.x) * (r.y - q.y);

            if (val == 0) return 0;  // colinear 
            return (val > 0) ? 1 : 2; // clock or counterclock wise 
        }
        bool doIntersect(Point p1, Point q1, Point p2, Point q2)
        {
            // Find the four orientations needed for general and 
            // special cases 
            int o1 = orientation(p1, q1, p2);
            int o2 = orientation(p1, q1, q2);
            int o3 = orientation(p2, q2, p1);
            int o4 = orientation(p2, q2, q1);

            // General case 
            if (o1 != o2 && o3 != o4)
                return true;

            // Special Cases 
            // p1, q1 and p2 are colinear and p2 lies on segment p1q1 
            if (o1 == 0 && onSegment(p1, p2, q1)) return true;

            // p1, q1 and p2 are colinear and q2 lies on segment p1q1 
            if (o2 == 0 && onSegment(p1, q2, q1)) return true;

            // p2, q2 and p1 are colinear and p1 lies on segment p2q2 
            if (o3 == 0 && onSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are colinear and q1 lies on segment p2q2 
            if (o4 == 0 && onSegment(p2, q1, q2)) return true;

            return false; // Doesn't fall in any of the above cases 
        }
        // Returns true if the point p lies inside the polygon[] with n vertices 
        bool isInside(Point[] polygon, int n, Point p)
        {
            // There must be at least 3 vertices in polygon[] 
            if (n < 3) return false;

            // Create a point for line segment from p to infinite 
            Point extreme = new Point();
            extreme.x = INF;
            extreme.y = p.y;

            // Count intersections of the above line with sides of polygon 
            int count = 0, i = 0;
            do
            {
                int next = (i + 1) % n;

                // Check if the line segment from 'p' to 'extreme' intersects with the line segment from 'polygon[i]' to 'polygon[next]' 
                if (doIntersect(polygon[i], polygon[next], p, extreme))
                {
                    // If the point 'p' is colinear with line segment 'i-next', 
                    // then check if it lies on segment. If it lies, return true, otherwise false 
                    if (orientation(polygon[i], p, polygon[next]) == 0)
                        return onSegment(polygon[i], p, polygon[next]);

                    count++;
                }
                i = next;
            } while (i != 0);

            // Return true if count is odd, false otherwise 
            return (count % 2 == 1);   
        }

        struct Point
        {
            public double x;
            public double y;
        };
        public double max(double a, double b)
        {
            return (a < b) ? b : a;
        }
        public double min(double a, double b)
        {
            return !(b < a) ? a : b;
        }
    }
}