namespace DeepConfigEditor.Components
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using DeepConfigEditor.Services;

    /// <summary>
    /// This provides a very application specific implementation of a content control that can swap to an alternate view and back again. It expects the main view to be set first, and from then on the main view will not change, only the alternate view will change.
    /// </summary>
    [TemplateVisualState(GroupName = VisualStateGroup, Name = MainState)]
    [TemplateVisualState(GroupName = VisualStateGroup, Name = AlternateState)]
    [TemplatePart(Name = MainContentName, Type = typeof(ContentControl))]
    [TemplatePart(Name = AlternateContentName, Type = typeof(ContentControl))]
    public partial class AlternateContentControl : ContentControl
    {
        private const string VisualStateGroup = "VisualStateGroup";

        private const string MainState = "MainState";
        private const string AlternateState = "AlternateState";

        private const string MainContentName = "Main";
        private const string AlternateContentName = "Alternate";

        private ContentControl _mainContent;
        private ContentControl _alternateContent;
        private Storyboard _transition;

        private string _currentState;

        static AlternateContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AlternateContentControl), new FrameworkPropertyMetadata(typeof(AlternateContentControl)));
        }

        /// <summary>
        /// Builds the visual tree for the TransitioningContentControl control 
        /// when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _mainContent = GetTemplateChild(MainContentName) as ContentControl;
            _alternateContent = GetTemplateChild(AlternateContentName) as ContentControl;

            if (_transition != null)
            {
                _transition.Completed -= OnTransitionCompleted;
            }

            _transition = GetStoryboard(MainState);
            if (_transition != null)
            {
                _transition.Completed += OnTransitionCompleted;
            }

            var curContent = this.Content;
            if (curContent != null)
            {
                StartTransition(null, curContent);
            }
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            StartTransition(oldContent, newContent);
        }

        private void StartTransition(object oldContent, object newContent)
        {
            if (_mainContent == null || _alternateContent == null)
            {
                return;
            }

            Logger.Instance.Debug("State transition initiated: " + newContent.GetType().Name);
            if (_currentState == null)
            {
                _mainContent.Content = newContent;
                _currentState = MainState;
                VisualStateManager.GoToState(this, _currentState, false);
            }
            else if (_currentState == MainState)
            {
                _alternateContent.Content = newContent;
                _currentState = AlternateState;
                VisualStateManager.GoToState(this, _currentState, true);
            }
            else
            {
                _mainContent.Content = newContent;
                _currentState = MainState;
                VisualStateManager.GoToState(this, _currentState, true);
            }

            Logger.Instance.Debug("State transition executed: {0} - {1}", newContent.GetType().Name, _currentState);
        }

        private void OnTransitionCompleted(object sender, EventArgs e)
        {
            if (_currentState == MainState)
            {
                _alternateContent.Content = null;
            }
        }

        private Storyboard GetStoryboard(string relevantTransition)
        {
            if (VisualTreeHelper.GetChildrenCount(this) != 1)
            {
                return null;
            }

            var root = VisualTreeHelper.GetChild(this, 0) as FrameworkElement;
            if (root == null)
            {
                return null;
            }

            var groupName = VisualStateGroup;

            VisualStateGroup presentationGroup = (
                from g in VisualStateManager.GetVisualStateGroups(root).OfType<VisualStateGroup>()
                where string.CompareOrdinal(groupName, g.Name) == 0
                select g).FirstOrDefault<VisualStateGroup>();

            if (presentationGroup != null)
            {
                return presentationGroup.States
                    .OfType<VisualState>()
                    .Where(state => state.Name == relevantTransition)
                    .Select(state => state.Storyboard)
                    .FirstOrDefault();
            }

            return null;
        }
    }
}