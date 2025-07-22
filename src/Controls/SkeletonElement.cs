using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

using K4AdotNet;
using K4AdotNet.BodyTracking;
using K4AdotNet.Sensor;

namespace TFLitePoseTrainer.Controls;

[ContentProperty("NoContent")]
class SkeletonElement : FrameworkElement
{
    static SkeletonElement()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SkeletonElement),
            new FrameworkPropertyMetadata(typeof(SkeletonElement)));

        var jointHierarchy = new List<Tuple<JointType, JointType>>();

        foreach (var jointType in JointTypes.All)
        {
            if (jointType.IsRoot() || jointType.IsFaceFeature())
            {
                continue;
            }

            var parentJointType = jointType.GetParent();
            jointHierarchy.Add(new(parentJointType, jointType));
        }

        JointHierarchy = jointHierarchy;
    }

    static readonly IEnumerable<Tuple<JointType, JointType>> JointHierarchy;

    #region == BonePen ==

    internal static readonly DependencyProperty BonePenProperty =
        DependencyProperty.Register(
            nameof(BonePen),
            typeof(Pen),
            typeof(SkeletonElement),
            new FrameworkPropertyMetadata(
                new Pen(),
                FrameworkPropertyMetadataOptions.AffectsRender));

    internal Pen BonePen
    {
        get => (Pen)GetValue(BonePenProperty);
        set => SetValue(BonePenProperty, value);
    }

    #endregion
    #region == JointBrushNone ==

    internal static readonly DependencyProperty JointBrushNoneProperty =
        DependencyProperty.Register(
            nameof(JointBrushNone),
            typeof(Brush),
            typeof(SkeletonElement),
            new FrameworkPropertyMetadata(
                Brushes.Transparent,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (d, e) => ((SkeletonElement)d).OnJointBrushNoneChanged(e)));
    internal Brush JointBrushNone
    {
        get => (Brush)GetValue(JointBrushNoneProperty);
        set => SetValue(JointBrushNoneProperty, value);
    }

    void OnJointBrushNoneChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateJointBrushDictionary();
    }

    #endregion
    #region == JointBrushLow ==

    internal static readonly DependencyProperty JointBrushLowProperty =
        DependencyProperty.Register(
            nameof(JointBrushLow),
            typeof(Brush),
            typeof(SkeletonElement),
            new FrameworkPropertyMetadata(
                Brushes.Transparent,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (d, e) => ((SkeletonElement)d).OnJointBrushLowChanged(e)));

    internal Brush JointBrushLow
    {
        get => (Brush)GetValue(JointBrushLowProperty);
        set => SetValue(JointBrushLowProperty, value);
    }

    void OnJointBrushLowChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateJointBrushDictionary();
    }

    #endregion
    #region == JointBrushMedium ==

    internal static readonly DependencyProperty JointBrushMediumProperty =
        DependencyProperty.Register(
            nameof(JointBrushMedium),
            typeof(Brush),
            typeof(SkeletonElement),
            new FrameworkPropertyMetadata(
                Brushes.Transparent,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (d, e) => ((SkeletonElement)d).OnJointBrushMediumChanged(e)));

    internal Brush JointBrushMedium
    {
        get => (Brush)GetValue(JointBrushMediumProperty);
        set => SetValue(JointBrushMediumProperty, value);
    }

    void OnJointBrushMediumChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateJointBrushDictionary();
    }

    #endregion
    #region == JointBrushHigh ==

    internal static readonly DependencyProperty JointBrushHighProperty =
        DependencyProperty.Register(
            nameof(JointBrushHigh),
            typeof(Brush),
            typeof(SkeletonElement),
            new FrameworkPropertyMetadata(
                Brushes.Transparent,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (d, e) => ((SkeletonElement)d).OnJointBrushHighChanged(e)));

    internal Brush JointBrushHigh
    {
        get => (Brush)GetValue(JointBrushHighProperty);
        set => SetValue(JointBrushHighProperty, value);
    }

    void OnJointBrushHighChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateJointBrushDictionary();
    }

    #endregion
    #region == JointRadius ==

    internal static readonly DependencyProperty JointRadiusProperty =
        DependencyProperty.Register(
            nameof(JointRadius),
            typeof(double),
            typeof(SkeletonElement),
            new FrameworkPropertyMetadata(
                0.0,
                FrameworkPropertyMetadataOptions.AffectsRender));

    internal double JointRadius
    {
        get => (double)GetValue(JointRadiusProperty);
        set => SetValue(JointRadiusProperty, value);
    }

    #endregion
    #region == JointPen ==

    internal static readonly DependencyProperty JointPenProperty =
        DependencyProperty.Register(
            nameof(JointPen),
            typeof(Pen),
            typeof(SkeletonElement),
            new FrameworkPropertyMetadata(
                new Pen(),
                FrameworkPropertyMetadataOptions.AffectsRender));

    internal Pen JointPen
    {
        get => (Pen)GetValue(JointPenProperty);
        set => SetValue(JointPenProperty, value);
    }

    #endregion

    #region == Calibration ==

    internal static readonly DependencyProperty CalibrationProperty =
           DependencyProperty.Register(
               nameof(Calibration),
               typeof(Calibration),
               typeof(SkeletonElement),
               new FrameworkPropertyMetadata(
                   new Calibration(),
                   FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                   (d, e) => ((SkeletonElement)d).OnCalibrationChanged(e)));

    internal Calibration Calibration
    {
        get => (Calibration)GetValue(CalibrationProperty);
        set => SetValue(CalibrationProperty, value);
    }

    void OnCalibrationChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateProjector();
    }

    #endregion
    #region == ProjectionMode ==

    internal static readonly DependencyProperty ProjectionModeProperty =
            DependencyProperty.Register(
                nameof(ProjectionMode),
                typeof(SkeletonProjectionMode),
                typeof(SkeletonElement),
                new FrameworkPropertyMetadata(
                    SkeletonProjectionMode.Depth,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, e) => ((SkeletonElement)d).OnProjectionModeChanged(e)));

    internal SkeletonProjectionMode ProjectionMode
    {
        get => (SkeletonProjectionMode)GetValue(ProjectionModeProperty);
        set => SetValue(ProjectionModeProperty, value);
    }

    void OnProjectionModeChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateProjector();
    }

    #endregion

    #region == Skeleton ==

    internal static readonly DependencyProperty SkeletonProperty =
            DependencyProperty.Register(
                nameof(Skeleton),
                typeof(Skeleton),
                typeof(SkeletonElement),
                new FrameworkPropertyMetadata(
                    new Skeleton(),
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, e) => ((SkeletonElement)d).OnSkeletonChanged(e)));

    internal Skeleton Skeleton
    {
        get => (Skeleton)GetValue(SkeletonProperty);
        set => SetValue(SkeletonProperty, value);
    }

    void OnSkeletonChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateProjectedPointDictionary();
        UpdateJointBrushDictionary();
    }

    #endregion

    #region == Projector ==

    static readonly Projector FallbackProjector = (in Joint _) => null;

    Projector _projector = FallbackProjector;

    void UpdateProjector()
    {
        var calibration = Calibration;
        if (calibration.IsValid)
        {
            Converter converter = ProjectionMode switch
            {
                SkeletonProjectionMode.Depth => (in Joint joint) => calibration.Convert3DTo2D(joint.PositionMm, CalibrationGeometry.Depth, CalibrationGeometry.Depth),
                SkeletonProjectionMode.Color => (in Joint joint) => calibration.Convert3DTo2D(joint.PositionMm, CalibrationGeometry.Depth, CalibrationGeometry.Color),
                _ => throw new InvalidOperationException($"Invalid {nameof(ProjectionMode)}: {ProjectionMode}"),
            };

            _projector = (in Joint joint) =>
            {
                var float2 = converter(joint);
                return float2.HasValue ? new(float2.Value.X, float2.Value.Y) : null;
            };
        }
        else
        {
            _projector = FallbackProjector;
        }

        UpdateProjectedPointDictionary();
    }

    delegate Float2? Converter(in Joint joint);
    delegate Point? Projector(in Joint joint);

    #endregion
    #region == ProjectedPointDictionary ==

    readonly IDictionary<JointType, Point?> _projectedPointDictionary = new Dictionary<JointType, Point?>(
        from jointType in JointTypes.All
        select new KeyValuePair<JointType, Point?>(jointType, null)
    );

    void UpdateProjectedPointDictionary()
    {
        var skeleton = Skeleton;

        foreach (var jointType in JointTypes.All)
        {
            var joint = skeleton[jointType];
            _projectedPointDictionary[jointType] = _projector(joint);
        }
    }

    #endregion
    #region == JointBrushDictionary ==

    readonly IDictionary<JointType, Brush> _jointBrushDictionary = new Dictionary<JointType, Brush>(
        from jointType in JointTypes.All
        select new KeyValuePair<JointType, Brush>(jointType, Brushes.Transparent)
    );

    void UpdateJointBrushDictionary()
    {
        var skeleton = Skeleton;

        foreach (var jointType in JointTypes.All)
        {
            var joint = skeleton[jointType];
            _jointBrushDictionary[jointType] = joint.ConfidenceLevel switch
            {
                JointConfidenceLevel.None => JointBrushNone,
                JointConfidenceLevel.Low => JointBrushLow,
                JointConfidenceLevel.Medium => JointBrushMedium,
                JointConfidenceLevel.High => JointBrushHigh,
                _ => throw new InvalidOperationException($"Invalid {nameof(JointConfidenceLevel)}: {joint.ConfidenceLevel}"),
            };
        }
    }

    #endregion

    protected override Size MeasureOverride(Size availableSize)
    {
        var calibration = Calibration;
        return calibration.IsValid
            ? ProjectionMode switch
            {
                SkeletonProjectionMode.Depth => new(calibration.DepthCameraCalibration.ResolutionWidth, calibration.DepthCameraCalibration.ResolutionHeight),
                SkeletonProjectionMode.Color => new(calibration.ColorCameraCalibration.ResolutionWidth, calibration.ColorCameraCalibration.ResolutionHeight),
                _ => throw new InvalidOperationException($"Invalid {nameof(ProjectionMode)}: {ProjectionMode}"),
            }
            : new();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        // Draw bones
        var bonePen = BonePen;

        foreach (var jointPair in JointHierarchy)
        {
            var point0 = _projectedPointDictionary[jointPair.Item1];
            var point1 = _projectedPointDictionary[jointPair.Item2];
            if (!point0.HasValue || !point1.HasValue)
            {
                continue;
            }

            drawingContext.DrawLine(bonePen, point0.Value, point1.Value);
        }

        // Draw joints
        var jointPen = JointPen;
        var jointRadius = JointRadius;

        foreach (var jointType in JointTypes.All)
        {
            var point = _projectedPointDictionary[jointType];
            if (!point.HasValue)
            {
                continue;
            }

            var brush = _jointBrushDictionary[jointType];
            drawingContext.DrawEllipse(brush, jointPen, point.Value, jointRadius, jointRadius);
        }
    }
}
