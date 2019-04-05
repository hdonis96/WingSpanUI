using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foundation;
using CoreGraphics;
using MapKit;
using UIKit;
using WingSpan2;
using WingSpan2.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Maps.iOS;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms.Maps;
using CoreLocation;
using ObjCRuntime;
using System.Diagnostics;

[assembly: ExportRenderer(typeof(CustomMap), typeof(CustomMapRenderer))]
namespace WingSpan2.iOS
{
    // Terminology:
    //  - annotation = pin
    //  - callout = info window
    class CustomMapRenderer : MapRenderer
    {
        UIView customPinView;
        List<CustomPin> customPins;
        CustomMap map;
        List<Position> shapeCoordinates;
        MKPolygonRenderer polygonRenderer;
        MKPolygon polygon;
        List<MKAnnotationView> shapeMarkers = new List<MKAnnotationView>();
        private static int POLY_POINTS = 4;
        MKMapView nativeMap;

        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                nativeMap = Control as MKMapView;
                if (nativeMap != null)
                {
                    nativeMap.RemoveAnnotations(nativeMap.Annotations);
                    nativeMap.GetViewForAnnotation = null;
                    nativeMap.CalloutAccessoryControlTapped -= OnCalloutAccessoryControlTapped;
                    nativeMap.DidSelectAnnotationView -= OnDidSelectAnnotationView;
                    nativeMap.DidDeselectAnnotationView -= OnDidDeselectAnnotationView;
                    nativeMap.RemoveOverlays(nativeMap.Overlays);
                    nativeMap.OverlayRenderer = null;
                    polygonRenderer = null;
                }
            }

            if (e.NewElement != null)
            {
                map = (CustomMap)e.NewElement;
                var nativeMap = Control as MKMapView;
                customPins = map.CustomPins;

                nativeMap.GetViewForAnnotation = GetViewForAnnotation;
                nativeMap.CalloutAccessoryControlTapped += OnCalloutAccessoryControlTapped;
                nativeMap.DidSelectAnnotationView += OnDidSelectAnnotationView;
                nativeMap.DidDeselectAnnotationView += OnDidDeselectAnnotationView;
                customPins = map.CustomPins;
                shapeCoordinates = map.ShapeCoordinates;
                nativeMap.DidSelectAnnotationView += onViewClick;
                nativeMap.ScrollEnabled = false;
                nativeMap.ZoomEnabled = false;
                
                
            }
        }
        void onViewClick(object sender, MKAnnotationViewEventArgs e) //override so map doesn't move when pin is clicked
        {
            e.View.CanShowCallout = true;
        }
        protected override MKAnnotationView GetViewForAnnotation(MKMapView mapView, IMKAnnotation annotation)
        {
            MKAnnotationView annotationView = null;

            if (annotation is MKUserLocation)
                return null;

            var customPin = GetCustomPin(annotation as MKPointAnnotation);
            if (customPin == null)
            {
                throw new Exception("Custom pin not found");
            }

            annotationView = mapView.DequeueReusableAnnotation(customPin.Id.ToString());
            if (annotationView == null)
            {
                String id = customPin.Id.ToString();
                annotationView = new CustomMKAnnotationView(annotation, id);
                String image = "ping.png";
                if (id.Equals("hasClicked")) { image = "pinBlue.png"; }
                else if (id.Equals("yourLocation")) { image = "pinAq.png"; }
                annotationView.Image = UIImage.FromFile(image);
                annotationView.CalloutOffset = new CGPoint(0, 0);
                annotationView.LeftCalloutAccessoryView = new UIImageView(UIImage.FromFile("eagle.png"));
                annotationView.RightCalloutAccessoryView = UIButton.FromType(UIButtonType.DetailDisclosure);
                ((CustomMKAnnotationView)annotationView).Id = customPin.Id.ToString();
                ((CustomMKAnnotationView)annotationView).Url = customPin.Url;
            }
            annotationView.CanShowCallout = true;

            return annotationView;
        }

        void OnCalloutAccessoryControlTapped(object sender, MKMapViewAccessoryTappedEventArgs e)
        {
            
            var customView = e.View as CustomMKAnnotationView;
            if (!string.IsNullOrWhiteSpace(customView.Url))
            {
                UIApplication.SharedApplication.OpenUrl(new Foundation.NSUrl(customView.Url));
            }
            AccessoryClicked(customView.Position); //adds polygon
            Debug.WriteLine("callout clicked: " + customView.Url);
            //if e.toString == ? -> deletePolygon();
        }

        void OnDidSelectAnnotationView(object sender, MKAnnotationViewEventArgs e)
        {
            var customView = e.View as CustomMKAnnotationView;
            customPinView = new UIView();

            if (customView.Id == "Xamarin")
            {
                customPinView.Frame = new CGRect(0, 0, 200, 84);
                var image = new UIImageView(new CGRect(0, 0, 200, 84));
                image.Image = UIImage.FromFile("xamarin.png");
                customPinView.AddSubview(image);
                customPinView.Center = new CGPoint(0, -(e.View.Frame.Height + 75));
                e.View.AddSubview(customPinView);
            }
        }

        void OnDidDeselectAnnotationView(object sender, MKAnnotationViewEventArgs e)
        {
            if (!e.View.Selected)
            {
                customPinView.RemoveFromSuperview();
                customPinView.Dispose();
                customPinView = null;
            }
        }

        CustomPin GetCustomPin(MKPointAnnotation annotation)
        {
            var position = new Position(annotation.Coordinate.Latitude, annotation.Coordinate.Longitude);
            foreach (var pin in customPins)
            {
                if (pin.Position == position)
                {
                    return pin;
                }
            }
            return null;
        }
        // on accessory click on info window-- add point to list
        private void AccessoryClicked(Position pos)
        {
            int points = map.getPolygonPoints() + 1;
            if (points > 4) return;
            map.setPolygonPoints(points);
           // e.Marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.pinPolygon));
          //  shapeMarkers.Add(e.Marker);
            double la = pos.Latitude;
            double lo = pos.Longitude;
            map.ShapeCoordinates.Add(new Position(la, lo));
            if (points == 4) { addShape(); }
        }
        //add polygon shape to map
        public void addShape()
        {
            nativeMap.OverlayRenderer = GetOverlayRenderer;
            CLLocationCoordinate2D[] coords = new CLLocationCoordinate2D[POLY_POINTS];
            int index = 0;
            foreach (var position in map.ShapeCoordinates)
            {
                coords[index] = new CLLocationCoordinate2D(position.Latitude, position.Longitude);
                index++;
            }

            polygon = MKPolygon.FromCoordinates(coords);
            nativeMap.AddOverlay(polygon);
           /* NativeMap.MapLongClick += longClick; */
          
          
        }

        //when you click on a part of the callout, delete polygon
        public void removePolygon()
        {
            System.Diagnostics.Debug.WriteLine("delete polygon!------");
            map.setPolygonPoints(0);
            map.ShapeCoordinates.Clear();
            nativeMap.RemoveOverlay(polygon);
            polygonRenderer = null;
            polygon = null;
            clearShapeMarkers();
        }
        public void clearShapeMarkers()
        {
           // foreach (Marker marker in shapeMarkers)
          //  {
          //      marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.pin));
          //  }
            shapeMarkers.Clear();
        }

        MKOverlayRenderer GetOverlayRenderer(MKMapView mapView, IMKOverlay overlayWrapper)
        {
            if (polygonRenderer == null && !Equals(overlayWrapper, null))
            {
                var overlay = Runtime.GetNSObject(overlayWrapper.Handle) as IMKOverlay;
                polygonRenderer = new MKPolygonRenderer(overlay as MKPolygon)
                {
                    FillColor = UIColor.FromRGB(208, 208, 255),
                    StrokeColor = UIColor.FromRGB(0, 0, 255),
                    Alpha = 0.4f,
                    LineWidth = 9
                };
            }
            return polygonRenderer;
        }
    }
}