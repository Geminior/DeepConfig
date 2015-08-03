namespace DeepConfigEditor.Components
{
    using System;
    using System.Windows;
    using System.Windows.Data;

    public class ExtendedBinding : FrameworkElement
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
                                                                        "Source",
                                                                        typeof(object),
                                                                        typeof(ExtendedBinding),
                                                                        new FrameworkPropertyMetadata()
                                                                        {
                                                                            BindsTwoWayByDefault = true,
                                                                            DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                                                                        });

        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(
                                                                        "Target",
                                                                        typeof(object),
                                                                        typeof(ExtendedBinding),
                                                                        new FrameworkPropertyMetadata()
                                                                        {
                                                                            BindsTwoWayByDefault = true,
                                                                            DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                                                                        });

        public object Target
        {
            get { return GetValue(ExtendedBinding.TargetProperty); }
            set { SetValue(ExtendedBinding.TargetProperty, value); }
        }

        public object Source
        {
            get { return GetValue(ExtendedBinding.SourceProperty); }
            set { SetValue(ExtendedBinding.SourceProperty, value); }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property.Name == ExtendedBinding.SourceProperty.Name)
            {
                //no loop wanted
                if (!object.ReferenceEquals(Source, Target))
                {
                    Target = Source;
                }
            }
            else if (e.Property.Name == ExtendedBinding.TargetProperty.Name)
            {
                //no loop wanted
                if (!object.ReferenceEquals(Source, Target))
                {
                    Source = Target;
                }
            }
        }
    }
}
