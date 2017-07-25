using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms;

namespace AiT.MessyColor.Controls
{
    public class AwesomeWrappanel : Layout<View>
    {
        /// <summary>
        ///     Backing Storage for the Orientation property
        /// </summary>
        public static readonly BindableProperty OrientationProperty =
            BindableProperty.Create(nameof(Orientation), typeof(StackOrientation), typeof(AwesomeWrappanel),
                StackOrientation.Vertical,
                propertyChanged: (bindable, oldvalue, newvalue) => ((AwesomeWrappanel)bindable).OnSizeChanged());

        /// <summary>
        ///     Backing Storage for the Spacing property
        /// </summary>
        public static readonly BindableProperty SpacingProperty =
            BindableProperty.Create(nameof(Spacing), typeof(double), typeof(AwesomeWrappanel), 6.0,
                propertyChanged: (bindable, oldvalue, newvalue) => ((AwesomeWrappanel)bindable).OnSizeChanged());

        /// <summary>
        ///     Backing Storage for the Spacing property
        /// </summary>
        public static readonly BindableProperty ItemTemplateProperty =
            BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(AwesomeWrappanel), null,
                propertyChanged: (bindable, oldvalue, newvalue) => ((AwesomeWrappanel)bindable).OnSizeChanged());

        /// <summary>
        ///     Backing Storage for the Spacing property
        /// </summary>
        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(AwesomeWrappanel), null,
                propertyChanged: (bindable, value, newValue) => ItemsSource_OnPropertyChanged((IEnumerable)value, (IEnumerable)newValue));

        public AwesomeWrappanel()
        {
            CollectionChanged += OnCollectionChanged;
        }

        /// <summary>
        ///     Orientation (Horizontal or Vertical)
        /// </summary>
        public StackOrientation Orientation
        {
            get { return (StackOrientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        ///     Spacing added between elements (both directions)
        /// </summary>
        /// <value>The spacing.</value>
        public double Spacing
        {
            get { return (double)GetValue(SpacingProperty); }
            set { SetValue(SpacingProperty, value); }
        }

        /// <summary>
        ///     Spacing added between elements (both directions)
        /// </summary>
        /// <value>The spacing.</value>
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        /// <summary>
        ///     Spacing added between elements (both directions)
        /// </summary>
        /// <value>The spacing.</value>
        public IEnumerable ItemsSource

        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }

            set { SetValue(ItemsSourceProperty, value); }
        }

        private static event EventHandler<NotifyCollectionChangedEventArgs> CollectionChanged;


        private static void ItemsSource_OnPropertyChanged(IEnumerable oldvalue,
            IEnumerable newvalue)
        {
            if (oldvalue != null)
            {
                var coll = (INotifyCollectionChanged)oldvalue;
                // Unsubscribe from CollectionChanged on the old collection
                coll.CollectionChanged -= ItemsSource_OnItemChanged;
            }
            if (newvalue != null)
            {
                var coll = (INotifyCollectionChanged)newvalue;
                // Subscribe to CollectionChanged on the new collection
                coll.CollectionChanged += ItemsSource_OnItemChanged;
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddItems(args.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove: // not tested yet
                    Children.Clear();
                    AddItems(args.NewItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Children.Clear();
                    break;
            }

        }

        private void AddItems(IList items)
        {
            foreach (var item in items)
            {
                var child = ItemTemplate.CreateContent() as View;
                if (child == null)
                    return;
                child.BindingContext = item;
                Children.Add(child);
            }
        }

        private static void ItemsSource_OnItemChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(null, e);
        }

        /// <summary>
        ///     This is called when the spacing or orientation properties are changed - it forces
        ///     the control to go back through a layout pass.
        /// </summary>
        private void OnSizeChanged()
        {
            ForceLayout();
        }

        /// <summary>
        ///     This method is called during the measure pass of a layout cycle to get the desired size of an element.
        /// </summary>
        /// <param name="widthConstraint">The available width for the element to use.</param>
        /// <param name="heightConstraint">The available height for the element to use.</param>
        [Obsolete("Use OnMeasure")]
        protected override SizeRequest OnSizeRequest(double widthConstraint, double heightConstraint)
        {
            if (WidthRequest > 0)
                widthConstraint = Math.Min(widthConstraint, WidthRequest);
            if (HeightRequest > 0)
                heightConstraint = Math.Min(heightConstraint, HeightRequest);
            var internalWidth = double.IsPositiveInfinity(widthConstraint)
                ? double.PositiveInfinity
                : Math.Max(0, widthConstraint);
            var internalHeight = double.IsPositiveInfinity(heightConstraint)
                ? double.PositiveInfinity
                : Math.Max(0, heightConstraint);
            return Orientation == StackOrientation.Vertical
                ? DoVerticalMeasure(internalWidth, internalHeight)
                : DoHorizontalMeasure(internalWidth, internalHeight);
        }
        /// <summary>
        ///     Does the vertical measure.
        /// </summary>
        /// <returns>The vertical measure.</returns>
        /// <param name="widthConstraint">Width constraint.</param>
        /// <param name="heightConstraint">Height constraint.</param>
        private SizeRequest DoVerticalMeasure(double widthConstraint, double heightConstraint)
        {
            var columnCount = 1;
            double width = 0;

            double height = 0;

            double minWidth = 0;

            double minHeight = 0;

            double heightUsed = 0;


            foreach (var item in Children)

            {
#pragma warning disable 618
                var size = item.GetSizeRequest(widthConstraint, heightConstraint);
#pragma warning restore 618

                width = Math.Max(width, size.Request.Width);


                var newHeight = height + size.Request.Height + Spacing;

                if (newHeight > heightConstraint)

                {
                    columnCount++;

                    heightUsed = Math.Max(height, heightUsed);

                    height = size.Request.Height;
                }

                else

                {
                    height = newHeight;
                }


                minHeight = Math.Max(minHeight, size.Minimum.Height);

                minWidth = Math.Max(minWidth, size.Minimum.Width);
            }


            if (columnCount > 1)

            {
                height = Math.Max(height, heightUsed);

                width *= columnCount; // take max width
            }


            return new SizeRequest(new Size(width, height), new Size(minWidth, minHeight));
        }


        /// <summary>
        ///     Does the horizontal measure.
        /// </summary>
        /// <returns>The horizontal measure.</returns>
        /// <param name="widthConstraint">Width constraint.</param>
        /// <param name="heightConstraint">Height constraint.</param>
        private SizeRequest DoHorizontalMeasure(double widthConstraint, double heightConstraint)

        {
            var rowCount = 1;


            double width = 0;

            double height = 0;

            double minWidth = 0;

            double minHeight = 0;

            double widthUsed = 0;


            foreach (var item in Children)

            {
#pragma warning disable 618
                var size = item.GetSizeRequest(widthConstraint, heightConstraint);
#pragma warning restore 618

                height = Math.Max(height, size.Request.Height);


                var newWidth = width + size.Request.Width + Spacing;

                if (newWidth > widthConstraint)

                {
                    rowCount++;

                    widthUsed = Math.Max(width, widthUsed);

                    width = size.Request.Width;
                }

                else

                {
                    width = newWidth;
                }


                minHeight = Math.Max(minHeight, size.Minimum.Height);

                minWidth = Math.Max(minWidth, size.Minimum.Width);
            }


            if (rowCount > 1)

            {
                width = Math.Max(width, widthUsed);

                height = (height + Spacing) * rowCount - Spacing; // via MitchMilam 
            }


            return new SizeRequest(new Size(width, height), new Size(minWidth, minHeight));
        }


        /// <summary>
        ///     Positions and sizes the children of a Layout.
        /// </summary>
        /// <param name="x">A value representing the x coordinate of the child region bounding box.</param>
        /// <param name="y">A value representing the y coordinate of the child region bounding box.</param>
        /// <param name="width">A value representing the width of the child region bounding box.</param>
        /// <param name="height">A value representing the height of the child region bounding box.</param>
        protected override void LayoutChildren(double x, double y, double width, double height)

        {
            if (Orientation == StackOrientation.Vertical)

            {
                double colWidth = 0;

                double yPos = y, xPos = x;


                foreach (var child in Children.Where(c => c.IsVisible))

                {
#pragma warning disable 618
                    var request = child.GetSizeRequest(width, height);
#pragma warning restore 618


                    var childWidth = request.Request.Width;

                    var childHeight = request.Request.Height;

                    colWidth = Math.Max(colWidth, childWidth);


                    if (yPos + childHeight > height)

                    {
                        yPos = y;

                        xPos += colWidth + Spacing;

                        colWidth = 0;
                    }


                    var region = new Rectangle(xPos, yPos, childWidth, childHeight);

                    LayoutChildIntoBoundingRegion(child, region);

                    yPos += region.Height + Spacing;
                }
            }

            else

            {
                double rowHeight = 0;

                double yPos = y, xPos = x;


                foreach (var child in Children.Where(c => c.IsVisible))

                {
#pragma warning disable 618
                    var request = child.GetSizeRequest(width, height);
#pragma warning restore 618


                    var childWidth = request.Request.Width;

                    var childHeight = request.Request.Height;

                    rowHeight = Math.Max(rowHeight, childHeight);


                    if (xPos + childWidth > width)

                    {
                        xPos = x;

                        yPos += rowHeight + Spacing;

                        rowHeight = 0;
                    }


                    var region = new Rectangle(xPos, yPos, childWidth, childHeight);

                    LayoutChildIntoBoundingRegion(child, region);

                    xPos += region.Width + Spacing;
                }
            }
        }
    }
}