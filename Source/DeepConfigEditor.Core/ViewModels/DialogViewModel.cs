namespace DeepConfigEditor.ViewModels
{
    using System;
    using System.Linq;
    using Caliburn.Micro;
    using Caliburn.Micro.Extras;
    using DeepConfigEditor.Utilities;

    public class DialogViewModel : Screen
    {
        public DialogViewModel()
        {
            this.Result = MessageResult.Cancel;
        }

        public string Title { get; set; }

        public string Message { get; set; }

        public MessageButton Buttons { get; set; }

        public MessageImage Icon { get; set; }

        public Uri IconUri
        {
            get
            {
                switch (this.Icon)
                {
                    case MessageImage.Error:
                    {
                        return As.ResourceUri("Error.png");
                    }

                    case MessageImage.Question:
                    {
                        return As.ResourceUri("Question.png");
                    }

                    case MessageImage.Information:
                    {
                        return As.ResourceUri("Info.png");
                    }

                    case MessageImage.Warning:
                    {
                        return As.ResourceUri("Warning.png");
                    }
                }

                return As.ResourceUri("Alert.png");
            }
        }

        public bool IsYesVisible
        {
            get { return ShowButtonIf(MessageButton.YesNo, MessageButton.YesNoCancel); }
        }

        public bool IsNoVisible
        {
            get { return ShowButtonIf(MessageButton.YesNo, MessageButton.YesNoCancel); }
        }

        public bool IsOkVisible
        {
            get { return ShowButtonIf(MessageButton.OKCancel, MessageButton.OK); }
        }

        public bool IsCancelVisible
        {
            get { return ShowButtonIf(MessageButton.OKCancel, MessageButton.YesNoCancel); }
        }

        public bool ShowErrorLogLink
        {
            get { return this.Icon == MessageImage.Error; }
        }

        public MessageResult Result
        {
            get;
            private set;
        }

        public void Yes()
        {
            SetResult(MessageResult.Yes);
        }

        public void No()
        {
            SetResult(MessageResult.No);
        }

        public void Ok()
        {
            SetResult(MessageResult.OK);
        }

        public void Cancel()
        {
            SetResult(MessageResult.Cancel);
        }

        public void OpenLog()
        {
            //TODO: implement this
        }

        private void SetResult(MessageResult result)
        {
            this.Result = result;
            TryClose();
        }

        private bool ShowButtonIf(params MessageButton[] matches)
        {
            return (matches.Contains(this.Buttons));
        }
    }
}
