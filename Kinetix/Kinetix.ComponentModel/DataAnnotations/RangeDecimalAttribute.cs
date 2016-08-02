using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Kinetix.ComponentModel.Formatters;

namespace Kinetix.ComponentModel.DataAnnotations {

    /// <summary>
    /// Class attribute for range of decimal objects.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RangeDecimalAttribute : RangeAttribute {

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="minimum">Minium value.</param>
        /// <param name="maximum">Maximum value.</param>
        public RangeDecimalAttribute(double minimum, double maximum)
            : this(minimum, maximum, true, true) {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="minimum">Minium value.</param>
        /// <param name="maximum">Maximum value.</param>
        /// <param name="isMinimumIncluded">Defines whether minimum is included.</param>
        /// <param name="isMaximumIncluded">Defines whether maximum is included.</param>
        public RangeDecimalAttribute(double minimum, double maximum, bool isMinimumIncluded, bool isMaximumIncluded)
            : base(minimum, maximum) {
            this.IsMinimumIncluded = isMinimumIncluded;
            this.IsMaximumIncluded = isMaximumIncluded;
        }

        /// <summary>
        /// Defines whether is the minimum is included in the range.
        /// </summary>
        public bool IsMinimumIncluded {
            get;
            private set;
        }

        /// <summary>
        /// Defines whether is the maximum is included in the range.
        /// </summary>
        public bool IsMaximumIncluded {
            get;
            private set;
        }

        /// <summary>
        /// Format error message.
        /// </summary>
        /// <param name="name">Error value.</param>
        /// <returns>Formated error message.</returns>
        public override string FormatErrorMessage(string name) {
            string s = string.Empty;
            int i = (this.IsMaximumIncluded ? 10 : 0) + (this.IsMinimumIncluded ? 1 : 0);
            switch (i) {
                case 0:
                    s = SR.ErrorRangeDecimalOpen;
                    break;
                case 1:
                    s = SR.ErrorRangeDecimalRightOpen;
                    break;
                case 10:
                    s = SR.ErrorRangeDecimalLeftOpen;
                    break;
                case 11:
                    return base.FormatErrorMessage(name);
            }

            return string.Format(CultureInfo.CurrentCulture, s, this.Minimum, this.Maximum);
        }

        /// <summary>
        /// True if object is valid.
        /// </summary>
        /// <param name="value">Obect to test.</param>
        /// <returns>Boolean.</returns>
        public override bool IsValid(object value) {
            string s = value as string;
            if (string.IsNullOrEmpty(s)) {
                return base.IsValid(value);
            }

            decimal d = (decimal)new FormatterDecimal().ConvertFromString(s);
            return base.IsValid(s.Replace(',', '.')) &&
                (this.IsMinimumIncluded || (double)d > (double)this.Minimum) &&
                (this.IsMaximumIncluded || (double)d < (double)this.Maximum);
        }
    }
}
