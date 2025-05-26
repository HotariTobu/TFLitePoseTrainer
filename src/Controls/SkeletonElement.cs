using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

using K4AdotNet;
using K4AdotNet.BodyTracking;
using K4AdotNet.Sensor;

namespace TFLitePoseTrainer.Controls;

[ContentProperty("NoContent")]
public class SkeletonElement : FrameworkElement
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

    private static readonly IEnumerable<Tuple<JointType, JointType>> JointHierarchy;

    #region == BonePen ==

    public static readonly DependencyProperty BonePenProperty =
        DependencyProperty.Register(
            nameof(BonePen),
            typeof(Pen),
            typeof(SkeletonElement),
            new FrameworkPropertyMetadata(
                new Pen(),
                FrameworkPropertyMetadataOptions.AffectsRender));

    public Pen BonePen
    {
        get => (Pen)GetValue(BonePenProperty);
        set => SetValue(BonePenProperty, value);
    }

    #endregion
    #region == JointBrushNone ==

    public static readonly DependencyProperty JointBrushNoneProperty =
        DependencyProperty.Register(
            nameof(JointBrushNone),
            typeof(Brush),
            typeof(SkeletonElement),
            new FrameworkPropertyMetadata(
                Brushes.Transparent,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (d, e) => ((SkeletonElement)d).OnJointBrushNoneChanged(e)));
    public Brush JointBrushNone
    {
        get => (Brush)GetValue(JointBrushNoneProperty);
        set => SetValue(JointBrushNoneProperty, value);
    }

    private void OnJointBrushNoneChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateJointBrushDictionary();
    }

    #endregion
    #region == JointBrushLow ==

    public static readonly DependencyProperty JointBrushLowProperty =
        DependencyProperty.Register(
            nameof(JointBrushLow),
            typeof(Brush),
            typeof(SkeletonElement),
            new FrameworkPropertyMetadata(
                Brushes.Transparent,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (d, e) => ((SkeletonElement)d).OnJointBrushLowChanged(e)));

    public Brush JointBrushLow
    {
        get => (Brush)GetValue(JointBrushLowProperty);
        set => SetValue(JointBrushLowProperty, value);
    }

    private void OnJointBrushLowChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateJointBrushDictionary();
    }

    #endregion
    #region == JointBrushMedium ==

    public static readonly DependencyProperty JointBrushMediumProperty =
        DependencyProperty.Register(
            nameof(JointBrushMedium),
            typeof(Brush),
            typeof(SkeletonElement),
            new FrameworkPropertyMetadata(
                Brushes.Transparent,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (d, e) => ((SkeletonElement)d).OnJointBrushMediumChanged(e)));

    public Brush JointBrushMedium
    {
        get => (Brush)GetValue(JointBrushMediumProperty);
        set => SetValue(JointBrushMediumProperty, value);
    }

    private void OnJointBrushMediumChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateJointBrushDictionary();
    }

    #endregion
    #region == JointBrushHigh ==

    public static readonly DependencyProperty JointBrushHighProperty =
        DependencyProperty.Register(
            nameof(JointBrushHigh),
            typeof(Brush),
            typeof(SkeletonElement),
            new FrameworkPropertyMetadata(
                Brushes.Transparent,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (d, e) => ((SkeletonElement)d).OnJointBrushHighChanged(e)));

    public Brush JointBrushHigh
    {
        get => (Brush)GetValue(JointBrushHighProperty);
        set => SetValue(JointBrushHighProperty, value);
    }

    private void OnJointBrushHighChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateJointBrushDictionary();
    }

    #endregion
    #region == JointRadius ==

    public static readonly DependencyProperty JointRadiusProperty =
        DependencyProperty.Register(
            nameof(JointRadius),
            typeof(double),
            typeof(SkeletonElement),
            new FrameworkPropertyMetadata(
                0.0,
                FrameworkPropertyMetadataOptions.AffectsRender));

    public double JointRadius
    {
        get => (double)GetValue(JointRadiusProperty);
        set => SetValue(JointRadiusProperty, value);
    }

    #endregion
    #region == JointPen ==

    public static readonly DependencyProperty JointPenProperty =
        DependencyProperty.Register(
            nameof(JointPen),
            typeof(Pen),
            typeof(SkeletonElement),
            new FrameworkPropertyMetadata(
                new Pen(),
                FrameworkPropertyMetadataOptions.AffectsRender));

    public Pen JointPen
    {
        get => (Pen)GetValue(JointPenProperty);
        set => SetValue(JointPenProperty, value);
    }

    #endregion

    #region == Calibration ==

    public static readonly DependencyProperty CalibrationProperty =
           DependencyProperty.Register(
               nameof(Calibration),
               typeof(Calibration),
               typeof(SkeletonElement),
               new FrameworkPropertyMetadata(
                   new Calibration(),
                   FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                   (d, e) => ((SkeletonElement)d).OnCalibrationChanged(e)));

    public Calibration Calibration
    {
        get => (Calibration)GetValue(CalibrationProperty);
        set => SetValue(CalibrationProperty, value);
    }

    private void OnCalibrationChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateProjector();
    }

    #endregion
    #region == ProjectionMode ==

    public static readonly DependencyProperty ProjectionModeProperty =
            DependencyProperty.Register(
                nameof(ProjectionMode),
                typeof(SkeletonProjectionMode),
                typeof(SkeletonElement),
                new FrameworkPropertyMetadata(
                    SkeletonProjectionMode.Depth,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, e) => ((SkeletonElement)d).OnProjectionModeChanged(e)));

    public SkeletonProjectionMode ProjectionMode
    {
        get => (SkeletonProjectionMode)GetValue(ProjectionModeProperty);
        set => SetValue(ProjectionModeProperty, value);
    }

    private void OnProjectionModeChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateProjector();
    }

    #endregion

    #region == Skeleton ==

    public static readonly DependencyProperty SkeletonProperty =
            DependencyProperty.Register(
                nameof(Skeleton),
                typeof(Skeleton),
                typeof(SkeletonElement),
                new FrameworkPropertyMetadata(
                    new Skeleton(),
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, e) => ((SkeletonElement)d).OnSkeletonChanged(e)));

    public Skeleton Skeleton
    {
        get => (Skeleton)GetValue(SkeletonProperty);
        set => SetValue(SkeletonProperty, value);
    }

    private void OnSkeletonChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateProjectedPointDictionary();
        UpdateJointBrushDictionary();
    }

    #endregion

    #region == Projector ==

    private static readonly Projector FallbackProjector = (in Joint _) => null;

    private Projector _projector = FallbackProjector;

    private void UpdateProjector()
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

    private delegate Float2? Converter(in Joint joint);
    private delegate Point? Projector(in Joint joint);

    #endregion
    #region == ProjectedPointDictionary ==

    private readonly IDictionary<JointType, Point?> _projectedPointDictionary = new Dictionary<JointType, Point?>(
        from jointType in JointTypes.All
        select new KeyValuePair<JointType, Point?>(jointType, null)
    );

    private void UpdateProjectedPointDictionary()
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

    private readonly IDictionary<JointType, Brush> _jointBrushDictionary = new Dictionary<JointType, Brush>(
        from jointType in JointTypes.All
        select new KeyValuePair<JointType, Brush>(jointType, Brushes.Transparent)
    );

    private void UpdateJointBrushDictionary()
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
