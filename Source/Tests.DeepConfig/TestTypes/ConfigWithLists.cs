namespace DeepConfig.TestTypes
{
    using System.Collections.Generic;

    [ConfigSection]
    public class ConfigWithLists
    {
        private IList<string> _stringColOne;
        private List<string> _stringColTwo;
        private DerivedList _derivedColLazyOne;

        [ConfigSetting]
        public ICollection<string> StringsColOne
        {
            get;
            set;
        }

        [ConfigSetting]
        public IList<string> StringsColLazyOne
        {
            get
            {
                if (_stringColOne == null)
                {
                    _stringColOne = new List<string>();
                }

                return _stringColOne;
            }
        }

        [ConfigSetting]
        public List<string> StringsColTwo
        {
            get;
            set;
        }

        [ConfigSetting]
        public List<string> StringsColLazyTwo
        {
            get
            {
                if (_stringColTwo == null)
                {
                    _stringColTwo = new List<string>();
                }

                return _stringColTwo;
            }
        }

        [ConfigSetting]
        public ICollection<int> DerivedColOne
        {
            get;
            set;
        }

        [ConfigSetting]
        public DerivedList DerivedColTwo
        {
            get;
            set;
        }

        [ConfigSetting]
        public IList<int> DerivedColLazyOne
        {
            get
            {
                if (_derivedColLazyOne == null)
                {
                    _derivedColLazyOne = new DerivedList();
                }

                return _derivedColLazyOne;
            }
        }

        public class DerivedList : List<int>
        {
        }
    }
}
