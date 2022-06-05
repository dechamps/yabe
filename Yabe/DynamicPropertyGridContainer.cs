/**************************************************************************
*                           MIT License
* 
* Copyright (C) 2014 Morten Kvistgaard <mk@pch-engineering.dk>
*
* Permission is hereby granted, free of charge, to any person obtaining
* a copy of this software and associated documentation files (the
* "Software"), to deal in the Software without restriction, including
* without limitation the rights to use, copy, modify, merge, publish,
* distribute, sublicense, and/or sell copies of the Software, and to
* permit persons to whom the Software is furnished to do so, subject to
* the following conditions:
*
* The above copyright notice and this permission notice shall be included
* in all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
* IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
* CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
* TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
* SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*
*********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.BACnet;
using System.Globalization;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.Drawing.Design;

namespace Utilities
{
    /// <summary>
    /// Helper classses for dynamic property grid manipulations.
    /// Note: Following attribute can be helpful also: [System.ComponentModel.TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
    /// </summary>
	class DynamicPropertyGridContainer: CollectionBase,ICustomTypeDescriptor
	{
		/// <summary>
		/// Add CustomProperty to Collectionbase List
		/// </summary>
		/// <param name="Value"></param>
		public void Add(CustomProperty Value)
		{
			base.List.Add(Value);
		}

		/// <summary>
		/// Remove item from List
		/// </summary>
		/// <param name="Name"></param>
		public void Remove(string Name)
		{
			foreach(CustomProperty prop in base.List)
			{
				if(prop.Name == Name)
				{
					base.List.Remove(prop);
					return;
				}
			}
		}

		/// <summary>
		/// Indexer
		/// </summary>
		public CustomProperty this[int index] 
		{
			get 
			{
				return (CustomProperty)base.List[index];
			}
			set
			{
				base.List[index] = (CustomProperty)value;
			}
		}

        public CustomProperty this[string name]
        {
            get
            {
                foreach (CustomProperty p in this)
                {
                    if (p.Name == name) return p;
                }
                return null;
            }
        }

		#region "TypeDescriptor Implementation"
		/// <summary>
		/// Get Class Name
		/// </summary>
		/// <returns>String</returns>
		public String GetClassName()
		{
			return TypeDescriptor.GetClassName(this,true);
		}

		/// <summary>
		/// GetAttributes
		/// </summary>
		/// <returns>AttributeCollection</returns>
		public AttributeCollection GetAttributes()
		{
			return TypeDescriptor.GetAttributes(this,true);
		}

		/// <summary>
		/// GetComponentName
		/// </summary>
		/// <returns>String</returns>
		public String GetComponentName()
		{
			return TypeDescriptor.GetComponentName(this, true);
		}

		/// <summary>
		/// GetConverter
		/// </summary>
		/// <returns>TypeConverter</returns>
		public TypeConverter GetConverter()
		{
			return TypeDescriptor.GetConverter(this, true);
		}

		/// <summary>
		/// GetDefaultEvent
		/// </summary>
		/// <returns>EventDescriptor</returns>
		public EventDescriptor GetDefaultEvent() 
		{
			return TypeDescriptor.GetDefaultEvent(this, true);
		}

		/// <summary>
		/// GetDefaultProperty
		/// </summary>
		/// <returns>PropertyDescriptor</returns>
		public PropertyDescriptor GetDefaultProperty() 
		{
			return TypeDescriptor.GetDefaultProperty(this, true);
		}

		/// <summary>
		/// GetEditor
		/// </summary>
		/// <param name="editorBaseType">editorBaseType</param>
		/// <returns>object</returns>
		public object GetEditor(Type editorBaseType) 
		{
			return TypeDescriptor.GetEditor(this, editorBaseType, true);
		}

		public EventDescriptorCollection GetEvents(Attribute[] attributes) 
		{
			return TypeDescriptor.GetEvents(this, attributes, true);
		}

		public EventDescriptorCollection GetEvents()
		{
			return TypeDescriptor.GetEvents(this, true);
		}

		public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			PropertyDescriptor[] newProps = new PropertyDescriptor[this.Count];
			for (int i = 0; i < this.Count; i++)
			{
				CustomProperty  prop = (CustomProperty) this[i];
				newProps[i] = new CustomPropertyDescriptor(ref prop, attributes);
			}

			return new PropertyDescriptorCollection(newProps);
		}

		public PropertyDescriptorCollection GetProperties()
		{
			
			return TypeDescriptor.GetProperties(this, true);
			
		}

		public object GetPropertyOwner(PropertyDescriptor pd) 
		{
			return this;
		}
		#endregion

        public override string ToString()
        {
            return "Custom type";
        }
	}

	/// <summary>
	/// Custom property class 
	/// </summary>
	public class CustomProperty
	{
		private string m_name = string.Empty;
		private bool m_readonly = false;
        private object m_old_value = null;
		private object m_value = null;
        private Type m_type;
        private object m_tag;
        private DynamicEnum m_options;
        private string m_category;
        // Modif FC : change type
        private BacnetApplicationTags? m_description;

        // Modif FC : constructor
        public CustomProperty(string name, object value, Type type, bool read_only, string category = "", BacnetApplicationTags? description = null, DynamicEnum options = null, object tag = null)
        {
			this.m_name = name;
            this.m_old_value = value;
			this.m_value = value;
            this.m_type = type;
			this.m_readonly = read_only;
            this.m_tag = tag;
            this.m_options = options;
            this.m_category = "BacnetProperty";
            this.m_description = description;
		}

        public DynamicEnum Options
        {
            get { return m_options; }
        }

        public Type Type
        {
            get { return m_type; }
        }

        public string Category
        {
            get { return m_category; }
        }

        // Modif FC
        public string Description
        {
            get { return m_description == null ? null : m_description.ToString(); }
        }

        // Modif FC : added
        public BacnetApplicationTags? bacnetApplicationTags
        {
            get { return m_description; }
        }

		public bool ReadOnly
		{
			get
			{
				return m_readonly;
			}
		}

		public string Name
		{
			get
			{
				return m_name;
			}
		}

		public bool Visible
		{
			get
			{
				return true;
			}
		}

		public object Value
		{
			get
			{
				return m_value;
			}
			set
			{
				m_value = value;
			}
		}

        public object Tag
        {
            get { return m_tag; }
        }

        public void Reset()
        {
            m_value = m_old_value;
        }
	}

    #region " DoubleConvert"

    /// <summary>
    /// A class to allow the conversion of doubles to string representations of
    /// their exact decimal values. The implementation aims for readability over
    /// efficiency.
    /// </summary>
    public class DoubleConverter
    {
        /// <summary>
        /// Converts the given double to a string representation of its
        /// exact decimal value.
        /// </summary>
        /// <param name="d">The double to convert.</param>
        /// <returns>A string representation of the double's exact decimal value.</return>
        public static string ToExactString(double d)
        {
            if (double.IsPositiveInfinity(d))
                return System.Globalization.NumberFormatInfo.CurrentInfo.PositiveInfinitySymbol;
            if (double.IsNegativeInfinity(d))
                return System.Globalization.NumberFormatInfo.CurrentInfo.NegativeInfinitySymbol;
            if (double.IsNaN(d))
                return System.Globalization.NumberFormatInfo.CurrentInfo.NaNSymbol;

            // Translate the double into sign, exponent and mantissa.
            long bits = BitConverter.DoubleToInt64Bits(d);
            // Note that the shift is sign-extended, hence the test against -1 not 1
            bool negative = (bits < 0);
            int exponent = (int)((bits >> 52) & 0x7ffL);
            long mantissa = bits & 0xfffffffffffffL;

            // Subnormal numbers; exponent is effectively one higher,
            // but there's no extra normalisation bit in the mantissa
            if (exponent == 0)
            {
                exponent++;
            }
            // Normal numbers; leave exponent as it is but add extra
            // bit to the front of the mantissa
            else
            {
                mantissa = mantissa | (1L << 52);
            }

            // Bias the exponent. It's actually biased by 1023, but we're
            // treating the mantissa as m.0 rather than 0.m, so we need
            // to subtract another 52 from it.
            exponent -= 1075;

            if (mantissa == 0)
            {
                return "0";
            }

            /* Normalize */
            while ((mantissa & 1) == 0)
            {    /*  i.e., Mantissa is even */
                mantissa >>= 1;
                exponent++;
            }

            /// Construct a new decimal expansion with the mantissa
            ArbitraryDecimal ad = new ArbitraryDecimal(mantissa);

            // If the exponent is less than 0, we need to repeatedly
            // divide by 2 - which is the equivalent of multiplying
            // by 5 and dividing by 10.
            if (exponent < 0)
            {
                for (int i = 0; i < -exponent; i++)
                    ad.MultiplyBy(5);
                ad.Shift(-exponent);
            }
            // Otherwise, we need to repeatedly multiply by 2
            else
            {
                for (int i = 0; i < exponent; i++)
                    ad.MultiplyBy(2);
            }

            // Finally, return the string with an appropriate sign
            if (negative)
                return "-" + ad.ToString();
            else
                return ad.ToString();
        }

        /// <summary>Private class used for manipulating
        class ArbitraryDecimal
        {
            /// <summary>Digits in the decimal expansion, one byte per digit
            byte[] digits;
            /// <summary> 
            /// How many digits are *after* the decimal point
            /// </summary>
            int decimalPoint = 0;

            /// <summary> 
            /// Constructs an arbitrary decimal expansion from the given long.
            /// The long must not be negative.
            /// </summary>
            internal ArbitraryDecimal(long x)
            {
                string tmp = x.ToString(System.Globalization.CultureInfo.InvariantCulture);
                digits = new byte[tmp.Length];
                for (int i = 0; i < tmp.Length; i++)
                    digits[i] = (byte)(tmp[i] - '0');
                Normalize();
            }

            /// <summary>
            /// Multiplies the current expansion by the given amount, which should
            /// only be 2 or 5.
            /// </summary>
            internal void MultiplyBy(int amount)
            {
                byte[] result = new byte[digits.Length + 1];
                for (int i = digits.Length - 1; i >= 0; i--)
                {
                    int resultDigit = digits[i] * amount + result[i + 1];
                    result[i] = (byte)(resultDigit / 10);
                    result[i + 1] = (byte)(resultDigit % 10);
                }
                if (result[0] != 0)
                {
                    digits = result;
                }
                else
                {
                    Array.Copy(result, 1, digits, 0, digits.Length);
                }
                Normalize();
            }

            /// <summary>
            /// Shifts the decimal point; a negative value makes
            /// the decimal expansion bigger (as fewer digits come after the
            /// decimal place) and a positive value makes the decimal
            /// expansion smaller.
            /// </summary>
            internal void Shift(int amount)
            {
                decimalPoint += amount;
            }

            /// <summary>
            /// Removes leading/trailing zeroes from the expansion.
            /// </summary>
            internal void Normalize()
            {
                int first;
                for (first = 0; first < digits.Length; first++)
                    if (digits[first] != 0)
                        break;
                int last;
                for (last = digits.Length - 1; last >= 0; last--)
                    if (digits[last] != 0)
                        break;

                if (first == 0 && last == digits.Length - 1)
                    return;

                byte[] tmp = new byte[last - first + 1];
                for (int i = 0; i < tmp.Length; i++)
                    tmp[i] = digits[i + first];

                decimalPoint -= digits.Length - (last + 1);
                digits = tmp;
            }

            /// <summary>
            /// Converts the value to a proper decimal string representation.
            /// </summary>
            public override String ToString()
            {
                char[] digitString = new char[digits.Length];
                for (int i = 0; i < digits.Length; i++)
                    digitString[i] = (char)(digits[i] + '0');

                // Simplest case - nothing after the decimal point,
                // and last real digit is non-zero, eg value=35
                if (decimalPoint == 0)
                {
                    return new string(digitString);
                }

                // Fairly simple case - nothing after the decimal
                // point, but some 0s to add, eg value=350
                if (decimalPoint < 0)
                {
                    return new string(digitString) +
                           new string('0', -decimalPoint);
                }

                // Nothing before the decimal point, eg 0.035
                if (decimalPoint >= digitString.Length)
                {
                    return "0" + System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator +
                        new string('0', (decimalPoint - digitString.Length)) +
                        new string(digitString);
                }

                // Most complicated case - part of the string comes
                // before the decimal point, part comes after it,
                // eg 3.5
                return new string(digitString, 0,
                                   digitString.Length - decimalPoint) +
                    System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator +
                    new string(digitString,
                                digitString.Length - decimalPoint,
                                decimalPoint);
            }
        }
    }

    #endregion

    public class DynamicEnum : ICollection
    {
        private Dictionary<string, int> m_stringIndex = new Dictionary<string, int>();
        private Dictionary<int, string> m_intIndex = new Dictionary<int, string>();
        public bool IsFlag { get; set; }

        public int this[string name]
        {
            get
            { 
                int value = 0;

                if (name.IndexOf(',') != -1)
                {
                    int num = 0;
                    foreach (string str2 in name.Split(new char[] { ',' }))
                    {
                        m_stringIndex.TryGetValue(str2.Trim(), out value);
                        num |= value;
                    }
                    return num;
                }

                m_stringIndex.TryGetValue(name, out value);
                return value;
            }
        }
        public string this[int value]
        {
            get
            {
                if (IsFlag)
                {
                    string str = "";
                    foreach (KeyValuePair<string, int> entry in m_stringIndex)
                    {
                        if ((value & entry.Value) > 0 || (entry.Value == 0 && value == 0)) str += ", " + entry.Key;
                    }
                    if (str != "") str = str.Substring(2);
                    return str;
                }
                else
                {
                    string name;
                    m_intIndex.TryGetValue(value, out name);
                    return name;
                }
            }
        }
        public void Add(string name, int value)
        {
            m_stringIndex.Add(name, value);
            m_intIndex.Add(value, name);
        }
        public bool Contains(string name)
        {
            return m_stringIndex.ContainsKey(name);
        }
        public bool Contains(int value)
        {
            return m_intIndex.ContainsKey(value);
        }

        public IEnumerator GetEnumerator()
        {
            return m_stringIndex.GetEnumerator();
        }

        public int Count
        {
            get { return m_stringIndex.Count; }
        }

        public void CopyTo(Array array, int index)
        {
            int i = 0;
            foreach (KeyValuePair<string, int> entry in this)
                array.SetValue(entry, i++ + index);
        }

        public bool IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        public object SyncRoot
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class DynamicEnumConverter : TypeConverter
    {
        // Fields
        private DynamicEnum m_e;

        public DynamicEnumConverter(DynamicEnum e)
        {
            m_e = e;
        }

        private static bool is_number(string str)
        {
            if (string.IsNullOrWhiteSpace(str) || str.Length == 0) return false;
            for (int i = 0; i < str.Length; i++)
                if (!char.IsNumber(str, i)) return false;
            return true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string && value != null)
            {
                string str = (string)value;
                str = str.Trim();

                if (m_e.Contains(str)) return m_e[str];
                else if (is_number(str))
                {
                    int int_val;
                    if (str.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
                        int_val = int.Parse(str.Substring(2), System.Globalization.NumberStyles.HexNumber);
                    else
                        int_val = int.Parse(str);
                    return int_val;
                }
                else
                {
                    return m_e[str];
                }
            }
            return base.ConvertFrom(context, culture, value);
        }
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return true;
        }
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return true;
        }
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
                throw new ArgumentNullException("destinationType");

            if ((destinationType == typeof(string)) && (value != null))
            {
                if (value is string)
                {
                    return value;
                }
                else if (value is KeyValuePair<string, int>)
                    return ((KeyValuePair<string, int>)value).Key;

                int val = (int)Convert.ChangeType(value, typeof(int));
                return m_e[val];
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new TypeConverter.StandardValuesCollection(m_e);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return !m_e.IsFlag;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            if (value is string) return m_e.Contains((string)value);

            int val = (int)Convert.ChangeType(value, typeof(int));
            return m_e.Contains(val);
        }
    }

    public class CustomSingleConverter : SingleConverter
    {
        public static bool DontDisplayExactFloats { get; set; }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if ((destinationType == typeof(string)) && (value.GetType() == typeof(float)) && !DontDisplayExactFloats)
            {
                return DoubleConverter.ToExactString((double)(float)value);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
         
    }

    public class BACnetActionCommandConverter : ExpandableObjectConverter
    {

        public override bool CanConvertTo(ITypeDescriptorContext context,
                                  System.Type destinationType)
        {
            if (destinationType == typeof(BACnetActionCommand))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context,
                              System.Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        // Call to change the display
        public override object ConvertTo(ITypeDescriptorContext context,
                               CultureInfo culture,
                               object value,
                               System.Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                 value is BACnetActionCommand)
            {

                BACnetActionCommand ac = (BACnetActionCommand)value;
                
                return ac.Device_Identifier + ":" + ac.Object_Identifier + ":" + ac.Property_Identifier + ":" + ac.Property_Array_Index + ":" + ac.Property_Value + ":" + ac.Priority + ":" + ac.Post_Delay + ":" + ac.Quit_On_Failure + ":" + ac.Write_Successful; 
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
                              CultureInfo culture, object value)
        {
            if (value is string)
            {
                return null; //only read
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    public class BACnetActionListConverter : ExpandableObjectConverter
    {

        public override bool CanConvertTo(ITypeDescriptorContext context,
                                  System.Type destinationType)
        {
            if (destinationType == typeof(BACnetActionList))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context,
                              System.Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        // Call to change the display
        public override object ConvertTo(ITypeDescriptorContext context,
                               CultureInfo culture,
                               object value,
                               System.Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                 value is BACnetActionList)
            {

                BACnetActionList al = (BACnetActionList)value;
                string ret = "";
                foreach (BACnetActionCommand ac in al.actions)
                    ret += ac + "\\";//.Device_Identifier + ":" + ac.Object_Identifier + ":" + ac.Property_Identifier + ":" + ac.Property_Array_Index + ":" + ac.Property_Value + ":" + ac.Priority + ":" + ac.Post_Delay + ":" + ac.Quit_On_Failure + ":" + ac.Write_Successful+ "\\";
                return ret;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
                              CultureInfo culture, object value)
        {
            if (value is string)
            {
                return null; //only read
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    public class BACnetAccumulatorRecordConverter : ExpandableObjectConverter
    {

        public override bool CanConvertTo(ITypeDescriptorContext context,
                                  System.Type destinationType)
        {
            if (destinationType == typeof(BACnetAccumulatorRecord))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context,
                              System.Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        // Call to change the display
        public override object ConvertTo(ITypeDescriptorContext context,
                               CultureInfo culture,
                               object value,
                               System.Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                 value is BACnetAccumulatorRecord)
            {

                BACnetAccumulatorRecord spr = (BACnetAccumulatorRecord)value;

                return spr.Timestamp+":"+spr.Present_Value+":"+spr.Accumulated_Value+":"+spr.Accumulator_Status;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
                              CultureInfo culture, object value)
        {
            if (value is string)
            {
                return null; //only read
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    public class BACnetLightingCommandConverter : ExpandableObjectConverter
    {

        public override bool CanConvertTo(ITypeDescriptorContext context,
                                  System.Type destinationType)
        {
            if (destinationType == typeof(BACnetLightingCommand))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context,
                              System.Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        // Call to change the display
        public override object ConvertTo(ITypeDescriptorContext context,
                               CultureInfo culture,
                               object value,
                               System.Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                 value is BACnetLightingCommand)
            {

                BACnetLightingCommand lc = (BACnetLightingCommand)value;

                return lc.Operation+":"+lc.Target_Level+":"+lc.Ramp_Rate+":"+lc.Step_Increment+":"+lc.Fade_Time+":"+lc.Priority;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
                              CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    string[] s = (value as String).Split(':');
                    return new BACnetLightingCommand((BACnetLightingOperation)Enum.Parse(typeof(BACnetLightingOperation), s[0]), Convert.ToSingle(s[1]), Convert.ToSingle(s[2]),Convert.ToSingle(s[3]),Convert.ToUInt32(s[4]),Convert.ToUInt32(s[5]));
                    
                }
                catch { return null; }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    public class BACnetAccessRuleConverter : ExpandableObjectConverter
    {

        public override bool CanConvertTo(ITypeDescriptorContext context,
                                  System.Type destinationType)
        {
            if (destinationType == typeof(BACnetAccessRule))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context,
                              System.Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        // Call to change the display
        public override object ConvertTo(ITypeDescriptorContext context,
                               CultureInfo culture,
                               object value,
                               System.Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                 value is BACnetAccessRule)
            {

                BACnetAccessRule s = (BACnetAccessRule)value;

                return s;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
                              CultureInfo culture, object value)
        {
            if (value is string)
            {

                try
                {

                    return null;

                }
                catch { return null; }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    public class BACnetScaleConverter : ExpandableObjectConverter
    {

        public override bool CanConvertTo(ITypeDescriptorContext context,
                                  System.Type destinationType)
        {
            if (destinationType == typeof(BACnetScale))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context,
                              System.Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        // Call to change the display
        public override object ConvertTo(ITypeDescriptorContext context,
                               CultureInfo culture,
                               object value,
                               System.Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                 value is BACnetScale)
            {

                BACnetScale s = (BACnetScale)value;

                return s.Type+":"+s.Value;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
                              CultureInfo culture, object value)
        {
            if (value is string)
            {        
                
                try
                {
                    
                    BACnetScale.choicetype ct;
                    string[] s = (value as String).Split(':');
                    Enum.TryParse(s[0], out ct);
                    
                    switch (ct)
                    {
                        case BACnetScale.choicetype.REAL:
                            return new BACnetScale(ct, Convert.ToSingle(s[1]));
                        case BACnetScale.choicetype.INTEGER:
                            return new BACnetScale(ct, Convert.ToInt32(s[1]));
                        default:
                            throw new NotSupportedException();

                    }
                    
                }
                catch { return null; }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }


    public class BACnetPrescaleConverter : ExpandableObjectConverter
    {

        public override bool CanConvertTo(ITypeDescriptorContext context,
                                  System.Type destinationType)
        {
            if (destinationType == typeof(BACnetPrescale))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context,
                              System.Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        // Call to change the display
        public override object ConvertTo(ITypeDescriptorContext context,
                               CultureInfo culture,
                               object value,
                               System.Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                 value is BACnetPrescale)
            {

                BACnetPrescale ps = (BACnetPrescale)value;

                return ps.Multiplier+":"+ps.Modulo_Divide;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
                              CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    string[] s = (value as String).Split(':');
                    return new BACnetPrescale(Convert.ToUInt32(s[0]), Convert.ToUInt32(s[1]));


                }
                catch { return null; }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    public class BACnetSetpointReferenceConverter : ExpandableObjectConverter
    {

        public override bool CanConvertTo(ITypeDescriptorContext context,
                                  System.Type destinationType)
        {
            if (destinationType == typeof(BACnetSetpointReference))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context,
                              System.Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        // Call to change the display
        public override object ConvertTo(ITypeDescriptorContext context,
                               CultureInfo culture,
                               object value,
                               System.Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                 value is BACnetSetpointReference)
            {

                BACnetSetpointReference spr = (BACnetSetpointReference)value;

                return spr.setpoint_reference;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
                              CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    string[] s = (value as String).Split(':');

                    return new BACnetSetpointReference(new BACnetObjectPropertyReference(new BacnetObjectId((BacnetObjectTypes)Enum.Parse(typeof(BacnetObjectTypes), s[0]), Convert.ToUInt16(s[1])), (BacnetPropertyIds)Convert.ToUInt32(s[2]), Convert.ToUInt32(s[3])));


                }
                catch { return null; }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    public class BACnetObjectPropertyReferenceConverter : ExpandableObjectConverter
    {

        public override bool CanConvertTo(ITypeDescriptorContext context,
                                  System.Type destinationType)
        {
            if (destinationType == typeof(BACnetObjectPropertyReference))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context,
                              System.Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        // Call to change the display
        public override object ConvertTo(ITypeDescriptorContext context,
                               CultureInfo culture,
                               object value,
                               System.Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                 value is BACnetObjectPropertyReference)
            {

                BACnetObjectPropertyReference opr = (BACnetObjectPropertyReference)value;

                return opr.Object_Identifier +
                       ":" + opr.Property_Identifier+(opr.Option_Property_Array_Index ? ":"+opr.Array_Index : "");
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
                              CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    string[] s = (value as String).Split(':');
                    
                    return new BACnetObjectPropertyReference(new BacnetObjectId((BacnetObjectTypes)Enum.Parse(typeof(BacnetObjectTypes), s[0]), Convert.ToUInt16(s[1])),(BacnetPropertyIds)Convert.ToUInt32(s[2]),Convert.ToUInt32(s[3]));
                    

                }
                catch { return null; }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    public class BACnetShedLevelConverter : ExpandableObjectConverter
    {

        public override bool CanConvertTo(ITypeDescriptorContext context,
                                  System.Type destinationType)
        {
            if (destinationType == typeof(BACnetShedLevel))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context,
                              System.Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        // Call to change the display
        public override object ConvertTo(ITypeDescriptorContext context,
                               CultureInfo culture,
                               object value,
                               System.Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                 value is BACnetShedLevel)
            {

                BACnetShedLevel sl = (BACnetShedLevel)value;

                return sl.Type +
                       ":" + sl.Value;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
                              CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    string[] s = (value as String).Split(':');
                    BACnetShedLevel.ShedLevelType type = (BACnetShedLevel.ShedLevelType)Enum.Parse(typeof(BACnetShedLevel.ShedLevelType), s[0]);
                    switch(type)
                    {
                        case BACnetShedLevel.ShedLevelType.PERCENT:
                        case BACnetShedLevel.ShedLevelType.LEVEL:
                            return new BACnetShedLevel((BACnetShedLevel.ShedLevelType)Enum.Parse(typeof(BACnetShedLevel.ShedLevelType), s[0]), Convert.ToUInt32(s[1]));
                        case BACnetShedLevel.ShedLevelType.AMOUNT:
                            return new BACnetShedLevel((BACnetShedLevel.ShedLevelType)Enum.Parse(typeof(BACnetShedLevel.ShedLevelType), s[0]), Convert.ToSingle(s[1]));
                        default:
                            throw new NotSupportedException();

                    }

                    
                }
                catch { return null; }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    public class BacnetObjectIdentifierConverter : ExpandableObjectConverter
    {

        public override bool CanConvertTo(ITypeDescriptorContext context,
                                  System.Type destinationType)
        {
            if (destinationType == typeof(BacnetObjectId))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context,
                              System.Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }
        
        // Call to change the display
        public override object ConvertTo(ITypeDescriptorContext context,
                               CultureInfo culture,
                               object value,
                               System.Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                 value is BacnetObjectId)
            {

                BacnetObjectId objId = (BacnetObjectId)value;

                return objId.type +
                       ":" + objId.instance;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
                              CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    string[] s = (value as String).Split(':');
                    return new BacnetObjectId((BacnetObjectTypes)Enum.Parse(typeof(BacnetObjectTypes), s[0]), Convert.ToUInt16(s[1]));
                }
                catch { return null; }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    public class BacnetDeviceObjectPropertyReferenceConverter: ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context,
                            System.Type destinationType)
        {
            if (destinationType == typeof(BacnetDeviceObjectPropertyReference))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context,
                      System.Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertTo(ITypeDescriptorContext context,
                        CultureInfo culture,
                        object value,
                        System.Type destinationType)
        {

            if (destinationType == typeof(System.String) &&
                 value is BacnetDeviceObjectPropertyReference)
            {
                BacnetDeviceObjectPropertyReference pr = (BacnetDeviceObjectPropertyReference)value;

                return "Reference to " +pr.objectIdentifier.ToString();
            }
            else
                return base.ConvertTo(context, culture, value, destinationType);          
          }

        public override object ConvertFrom(ITypeDescriptorContext context,
                              CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    // A realy hidden service !!!
                    // and remember that PRESENT_VALUE = 85
                    // entry like OBJECT_ANALOG_INPUT:0:85 or OBJECT_ANALOG_INPUT:0:85:478 for device 478
                    //
                    string[] s = (value as String).Split(':');
                    if (s.Length == 4)
                        return new BacnetDeviceObjectPropertyReference(
                                new BacnetObjectId((BacnetObjectTypes)Enum.Parse(typeof(BacnetObjectTypes), s[0]), Convert.ToUInt16(s[1])),
                                (BacnetPropertyIds)Convert.ToUInt16(s[2]), new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, Convert.ToUInt32(s[3]))

                        );
                    if (s.Length == 3)
                        return new BacnetDeviceObjectPropertyReference(
                                new BacnetObjectId((BacnetObjectTypes)Enum.Parse(typeof(BacnetObjectTypes), s[0]), Convert.ToUInt16(s[1])),
                                (BacnetPropertyIds)Convert.ToUInt16(s[2])
                        );
                    return null;
                }
                catch { return null; }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    public class BacnetBitStringConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context,
                              System.Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
                      CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    return BacnetBitString.Parse(value as String);
                }
                catch { return null; }
            }
            return base.ConvertFrom(context, culture, value);
        }

    }

    // used for BacnetTime (without Date, but stored in a DateTime struct)
    public class BacnetTimeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context,
                      System.Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
              CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    return DateTime.Parse("1/1/1 "+(string)value,System.Threading.Thread.CurrentThread.CurrentCulture);
                }
                catch { return null; }
            }
            return base.ConvertFrom(context, culture, value);
        }

         public override bool CanConvertTo(ITypeDescriptorContext context,
                            System.Type destinationType)
        {
            if (destinationType == typeof(DateTime))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

         public override object ConvertTo(ITypeDescriptorContext context,
                         CultureInfo culture,
                         object value,
                         System.Type destinationType)
         {
             if (destinationType == typeof(System.String) &&
                 value is DateTime)
             {
                 DateTime dt = (DateTime)value;

                 return dt.ToLongTimeString();
             }
             else
                return base.ConvertTo(context, culture, value, destinationType);
         }
    }

    // http://www.acodemics.co.uk/2014/03/20/c-datetimepicker-in-propertygrid/
    // used for BacnetTime Edition
    public class BacnetTimePickerEditor : UITypeEditor
    {

        IWindowsFormsEditorService editorService;
        DateTimePicker picker = new DateTimePicker();

        public BacnetTimePickerEditor()
        {
            picker.Format = DateTimePickerFormat.Time;
            picker.ShowUpDown = true;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                this.editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            }

            if (this.editorService != null)
            {
                DateTime dt= (DateTime)value;
                // this value is 1/1/1 for the date,  DatetimePicket don't accept it
                picker.Value = new DateTime(2000, 1, 1, dt.Hour, dt.Minute, dt.Second); // only HH:MM:SS is important
                this.editorService.DropDownControl(picker);
                value = picker.Value;
            }

            return value;
        }
    }

    // In order to give a readable list instead of a bitstring
    public class BacnetBitStringToEnumListDisplay : UITypeEditor
    {
        IWindowsFormsEditorService editorService;

        ListBox ObjetList;

        bool LinearEnum;
        Enum currentPropertyEnum;

        // the corresponding Enum is given in parameters
        // and also how the value is fixed 0,1,2... or 1,2,4,8... in the enumeration
        public BacnetBitStringToEnumListDisplay(Enum e, bool LinearEnum, bool DisplayAll=false)
        {
            currentPropertyEnum = e; 
            this.LinearEnum = LinearEnum;

            if (DisplayAll == true)
                ObjetList = new CheckedListBox();
            else
                ObjetList = new ListBox();
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        private static string GetNiceName(String name)
        {
            if (name.StartsWith("OBJECT_")) name = name.Substring(7);
            if (name.StartsWith("SERVICE_SUPPORTED_")) name = name.Substring(18);
            if (name.StartsWith("STATUS_FLAG_")) name = name.Substring(12);
            if (name.StartsWith("EVENT_ENABLE_")) name = name.Substring(13);
            if (name.StartsWith("EVENT_")) name = name.Substring(6);

            name = name.Replace('_', ' ');
            name = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());
            return name;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                this.editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            }
            if (this.editorService != null)
            {
                String bbs = value.ToString();

                for (int i=0;i<bbs.Length;i++)
                {
                    try
                    {
                        String Text;
                        if (LinearEnum==true)
                            Text = Enum.GetName(currentPropertyEnum.GetType(), i); // for 'classic' Enum like 0,1,2,3 ...
                        else
                            Text = Enum.GetName(currentPropertyEnum.GetType(), 1 << i); // for 2^n shift Enum like 1,2,4,8, ...

                        if ((bbs[i] == '1') && !(ObjetList is CheckedListBox))
                            ObjetList.Items.Add(GetNiceName(Text));
                        if (ObjetList is CheckedListBox)
                            (ObjetList as CheckedListBox).Items.Add(GetNiceName(Text), bbs[i] == '1');
                    }
                    catch { }
                }

                if (ObjetList.Items.Count == 0) // when bitstring is only 00000...
                    ObjetList.Items.Add("... Nothing");
                // shows the list
                this.editorService.DropDownControl(ObjetList);
            }
            return value; // do not allows any change
        }
    }

    // In order to remove the default PriorityArray editor which is a problem
    public class BacnetEditPriorityArray : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.None;
        }
    }
    // In order to give a readable name to classic enums
    public class BacnetEnumValueDisplay : UITypeEditor
    {
        ListBox EnumList;
        IWindowsFormsEditorService editorService;

        Enum currentPropertyEnum;

        // the corresponding Enum is given in parameter
        public BacnetEnumValueDisplay(Enum e)
        {
            currentPropertyEnum = e;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public static string GetNiceName(String name)
        {
            if (name == null) return ""; // Outbound enum (proprietary)

            if (name.StartsWith("EVENT_STATE_")) name = name.Substring(12);
            if (name.StartsWith("POLARITY_")) name = name.Substring(9);
            if (name.StartsWith("RELIABILITY_")) name = name.Substring(12);
            if (name.StartsWith("SEGMENTATION_")) name = name.Substring(13);
            if (name.StartsWith("STATUS_")) name = name.Substring(7);
            if (name.StartsWith("NOTIFY_")) name = name.Substring(7);
            if (name.StartsWith("UNITS_")) name = name.Substring(6);

            name = name.Replace('_', ' ');
            name = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());
            return name;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {            
            if (provider != null)
            {
                this.editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            }
            if (this.editorService != null)
            {
                int InitialIdx = (int)(uint)value;

                if (EnumList == null)
                {
                    EnumList = new ListBox();
                    EnumList.Click += new EventHandler(EnumList_Click);
                    // get all the Enum values string
                    String[] sl=Enum.GetNames(currentPropertyEnum.GetType());
                    for (int i = 0; i < sl.Length; i++)
                    {
                        if ((currentPropertyEnum.GetType() == typeof(BacnetObjectTypes)) && (i >= (int)BacnetObjectTypes.MAX_ASHRAE_OBJECT_TYPE))
                            break; // One property with some content not usefull
                        EnumList.Items.Add(i.ToString() + " : " + GetNiceName(sl[i])); // add to the list
                    }
                    if (InitialIdx<EnumList.Items.Count)
                        EnumList.SelectedIndex = InitialIdx; // select the current item if any
                }
                this.editorService.DropDownControl(EnumList); // shows the list

                if ((EnumList.SelectedIndex!=InitialIdx)&&(InitialIdx<EnumList.Items.Count))
                    return (uint)EnumList.SelectedIndex; // change the value if required
            }
            return value;
        }

        void EnumList_Click(object sender, EventArgs e)
        {
            if (this.editorService != null)
                this.editorService.CloseDropDown();

        }
    }
    // used for BacnetTime (without Date, but stored in a DateTime struct)
    public class BacnetEnumValueConverter : TypeConverter
    {
         Enum currentPropertyEnum;

        // the corresponding Enum is given in parameter
        public BacnetEnumValueConverter(Enum e)
        {
            currentPropertyEnum = e;
        }

        public override object ConvertTo(ITypeDescriptorContext context,
                        CultureInfo culture,
                        object value,
                        System.Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                value is uint)
            {
                int i = (int)(uint)value;
                return i.ToString() + " : " + BacnetEnumValueDisplay.GetNiceName(Enum.GetName(currentPropertyEnum.GetType(), (uint)i));
            }
            else
                return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    // by FC
    // this class is used to display 
    //      the priority name instead of the index base 0 for the Priority Array
    //      the priority level name for notification class priority array 
    //      the priority level name for the event time stamp
    //      the 1 base array index for multistate objects text property
    // Thank to Gerd Klevesaat's article in http://www.codeproject.com/Articles/4448/Customized-display-of-collection-data-in-a-Propert

    public class BacnetArrayIndexConverter : ArrayConverter
    {

        public enum BacnetEventStates { TO_OFF_NORMAL, TO_FAULT, TO_NORMAL };

        public class BacnetArrayPropertyDescriptor : PropertyDescriptor
        {
            private PropertyDescriptor m_Property = null;
            private int m_idx;
            private Enum m_enum;

            public BacnetArrayPropertyDescriptor(PropertyDescriptor Property, int Idx, Enum e)
                : base(Property)
            {
                m_Property = Property;
                m_idx = Idx;
                m_enum = e;
            }

            // This is what we want, and only this
            public override string DisplayName
            {
                get
                {
                    if (m_enum!=null)
                    {
                        string s = Enum.GetValues(m_enum.GetType()).GetValue(m_idx).ToString();
                        s = s.Replace('_', ' ');
                        return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                    }
                    else
                        return "[" + m_idx.ToString() + "]"; // special behaviour for State Text in mutlistate objects, only array bounds shift, no more !
                }
            }
            public override bool CanResetValue(object component) { return m_Property.CanResetValue(component); }
            public override Type ComponentType { get { return m_Property.ComponentType; }}
            public override object GetValue(object component) { return m_Property.GetValue(component); }
            public override bool IsReadOnly { get { return m_Property.IsReadOnly; } }
            public override Type PropertyType { get { return m_Property.PropertyType; }}
            public override void ResetValue(object component) { m_Property.ResetValue(component); }
            public override void SetValue(object component, object value) { m_Property.SetValue(component, value); }
            public override bool ShouldSerializeValue(object component) { return m_Property.ShouldSerializeValue(component); }
        }

        Enum _e;
        public BacnetArrayIndexConverter(Enum e)
        {
            _e=e;
        }
 
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            try
            {
                PropertyDescriptorCollection s = base.GetProperties(context, value, attributes);
                PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);

                // PriorityArray is a C# 0 base array for a Bacnet 1 base array
                // use also for StateText property array in multistate objects : http://www.chipkin.com/bacnet-multi-state-variables-state-zero/
                int shift=0;
                if ((_e==null) || (_e.GetType() == typeof(BacnetWritePriority)))
                        shift = 1;

                for (int i = 0; i < s.Count; i++)
                {
                    BacnetArrayPropertyDescriptor pd = new BacnetArrayPropertyDescriptor(s[i], i + shift, _e);
                    pds.Add(pd);
                }
                return pds;
            }
            catch
            {
                return base.GetProperties(context, value, attributes);
            }
        }
 
    }
    /// <summary>
	/// Custom PropertyDescriptor
	/// </summary>
    /// 
	class CustomPropertyDescriptor: PropertyDescriptor
	{
		CustomProperty m_Property;

        static CustomPropertyDescriptor()
        {
            TypeDescriptor.AddAttributes(typeof(BacnetDeviceObjectPropertyReference), new TypeConverterAttribute(typeof(BacnetDeviceObjectPropertyReferenceConverter)));
            TypeDescriptor.AddAttributes(typeof(BacnetObjectId), new TypeConverterAttribute(typeof(BacnetObjectIdentifierConverter)));
            TypeDescriptor.AddAttributes(typeof(BacnetBitString), new TypeConverterAttribute(typeof(BacnetBitStringConverter)));
            TypeDescriptor.AddAttributes(typeof(BACnetScale), new TypeConverterAttribute(typeof(BACnetScaleConverter)));
            TypeDescriptor.AddAttributes(typeof(BACnetActionCommand), new TypeConverterAttribute(typeof(BACnetActionCommandConverter)));
            TypeDescriptor.AddAttributes(typeof(BACnetActionList), new TypeConverterAttribute(typeof(BACnetActionListConverter)));
            TypeDescriptor.AddAttributes(typeof(BACnetAccumulatorRecord), new TypeConverterAttribute(typeof(BACnetAccumulatorRecordConverter)));
            TypeDescriptor.AddAttributes(typeof(BACnetLightingCommand), new TypeConverterAttribute(typeof(BACnetLightingCommandConverter)));
            TypeDescriptor.AddAttributes(typeof(BACnetAccessRule), new TypeConverterAttribute(typeof(BACnetAccessRuleConverter)));
            TypeDescriptor.AddAttributes(typeof(BACnetScale), new TypeConverterAttribute(typeof(BACnetScaleConverter)));
            TypeDescriptor.AddAttributes(typeof(BACnetPrescale), new TypeConverterAttribute(typeof(BACnetPrescaleConverter)));
            TypeDescriptor.AddAttributes(typeof(BACnetSetpointReference), new TypeConverterAttribute(typeof(BACnetSetpointReferenceConverter)));
            TypeDescriptor.AddAttributes(typeof(BACnetObjectPropertyReference), new TypeConverterAttribute(typeof(BACnetObjectPropertyReferenceConverter)));
            TypeDescriptor.AddAttributes(typeof(BACnetShedLevel), new TypeConverterAttribute(typeof(BACnetShedLevelConverter)));

        }

        public CustomPropertyDescriptor(ref CustomProperty myProperty, Attribute [] attrs) :base(myProperty.Name, attrs)
		{
			m_Property = myProperty;
		}

        public CustomProperty CustomProperty
        {
            get { return m_Property; }
        }

		#region PropertyDescriptor specific
		
		public override bool CanResetValue(object component)
		{
			return true;
		}

		public override Type ComponentType
		{
			get 
			{
				return null;
			}
		}

		public override object GetValue(object component)
		{
			return m_Property.Value;
		}

		public override string Description
		{
			get
			{
				return m_Property.Description;
			}
		}
		
		public override string Category
		{
			get
			{
                return m_Property.Category;
			}
		}

		public override string DisplayName
		{
			get
			{
				return m_Property.Name;
			}
			
		}

		public override bool IsReadOnly
		{
			get
			{
				return m_Property.ReadOnly;
			}
		}

		public override void ResetValue(object component)
		{
            m_Property.Reset();
		}

		public override bool ShouldSerializeValue(object component)
		{
			return false;
		}

		public override void SetValue(object component, object value)
		{
			m_Property.Value = value;
		}

        public override Type PropertyType
        {
            get { return m_Property.Type; }
        }

        public override TypeConverter Converter
        {
            get
            {
                if (m_Property.Options != null) return new DynamicEnumConverter(m_Property.Options);
                else if (m_Property.Type == typeof(float)) return new CustomSingleConverter();
                else if (m_Property.bacnetApplicationTags == BacnetApplicationTags.BACNET_APPLICATION_TAG_TIME) return new BacnetTimeConverter();
                
                // A lot of classic Bacnet Enum
                BacnetPropertyReference bpr = (BacnetPropertyReference)m_Property.Tag;
                switch ((BacnetPropertyIds)bpr.propertyIdentifier)
                {
                    case BacnetPropertyIds.PROP_OBJECT_TYPE:
                        return new BacnetEnumValueConverter(new BacnetObjectTypes());
                    case BacnetPropertyIds.PROP_NOTIFY_TYPE:
                        return new BacnetEnumValueConverter(new BacnetEventNotificationData.BacnetNotifyTypes());
                    case BacnetPropertyIds.PROP_EVENT_TYPE:
                        return new BacnetEnumValueConverter(new BacnetEventNotificationData.BacnetEventTypes());
                    case BacnetPropertyIds.PROP_EVENT_STATE:
                        return new BacnetEnumValueConverter(new BacnetEventNotificationData.BacnetEventStates());
                    case BacnetPropertyIds.PROP_POLARITY:
                        return new BacnetEnumValueConverter(new BacnetPolarity());
                    case BacnetPropertyIds.PROP_UNITS:
                    case BacnetPropertyIds.PROP_OUTPUT_UNITS:
                    case BacnetPropertyIds.PROP_CONTROLLED_VARIABLE_UNITS:
                    case BacnetPropertyIds.PROP_PROPORTIONAL_CONSTANT_UNITS:
                    case BacnetPropertyIds.PROP_INTEGRAL_CONSTANT_UNITS:
                    case BacnetPropertyIds.PROP_DERIVATIVE_CONSTANT_UNITS:
                    case BacnetPropertyIds.PROP_CAR_LOAD_UNITS:
                        return new BacnetEnumValueConverter(new BacnetUnitsId());
                    case BacnetPropertyIds.PROP_RELIABILITY:
                        return new BacnetEnumValueConverter(new BacnetReliability());
                    case BacnetPropertyIds.PROP_SEGMENTATION_SUPPORTED:
                        return new BacnetEnumValueConverter(new BacnetSegmentations());
                    case BacnetPropertyIds.PROP_SYSTEM_STATUS:
                        return new BacnetEnumValueConverter(new BacnetDeviceStatus());
                    case BacnetPropertyIds.PROP_LAST_RESTART_REASON:
                        return new BacnetEnumValueConverter(new BacnetRestartReason());
                    case BacnetPropertyIds.PROP_PRIORITY_FOR_WRITING:
                        return new BacnetEnumValueConverter(new BacnetWritePriority()); 
                    case BacnetPropertyIds.PROP_PRIORITY_ARRAY:
                        return new BacnetArrayIndexConverter(new BacnetWritePriority());
                    case BacnetPropertyIds.PROP_PRIORITY :
                        return new BacnetArrayIndexConverter(new BacnetArrayIndexConverter.BacnetEventStates());
                    case BacnetPropertyIds.PROP_EVENT_TIME_STAMPS :
                        return new BacnetArrayIndexConverter(new BacnetArrayIndexConverter.BacnetEventStates());
                    case BacnetPropertyIds.PROP_STATE_TEXT :
                        return new BacnetArrayIndexConverter(null); 
                    case BacnetPropertyIds.PROP_PROGRAM_CHANGE :
                        return new BacnetEnumValueConverter(new BacnetProgramChange());
                    case BacnetPropertyIds.PROP_PROGRAM_STATE:
                        return new BacnetEnumValueConverter(new BacnetProgramState());
                    case BacnetPropertyIds.PROP_REASON_FOR_HALT:
                        return new BacnetEnumValueConverter(new BACnetProgramError());
                    case BacnetPropertyIds.PROP_BACKUP_AND_RESTORE_STATE:
                        return new BacnetEnumValueConverter(new BACnetBackupState());
                    case BacnetPropertyIds.PROP_FILE_ACCESS_METHOD:
                        return new BacnetEnumValueConverter(new BacnetFileAccessMethod());
                    case BacnetPropertyIds.PROP_DOOR_STATUS:
                    case BacnetPropertyIds.PROP_LOCK_STATUS:
                    case BacnetPropertyIds.PROP_CAR_DOOR_STATUS:
                        return new BacnetEnumValueConverter(new BACnetDoorStatus());
                    case BacnetPropertyIds.PROP_CREDENTIAL_DISABLE:
                        return new BacnetEnumValueConverter(new BACnetAccessCredentialDisable());
                    case BacnetPropertyIds.PROP_OCCUPANCY_STATE:
                        return new BacnetEnumValueConverter(new BACnetAccessZoneOccupancyState());
                    case BacnetPropertyIds.PROP_AUTHENTICATION_STATUS:
                        return new BacnetEnumValueConverter(new BACnetAuthenticationStatus());
                    case BacnetPropertyIds.PROP_ESCALATOR_MODE:
                        return new BacnetEnumValueConverter(new BACnetEscalatorMode());
                    case BacnetPropertyIds.PROP_BACNET_IP_MODE:
                        return new BacnetEnumValueConverter(new BACnetIPMode());
                    case BacnetPropertyIds.PROP_MAINTENANCE_REQUIRED:
                        return new BacnetEnumValueConverter(new BACnetMaintenance());
                    case BacnetPropertyIds.PROP_NETWORK_NUMBER_QUALITY:
                        return new BacnetEnumValueConverter(new BACnetNetworkNumberQuality());
                    case BacnetPropertyIds.PROP_COMMAND:
                        return new BacnetEnumValueConverter(new BACnetNetworkPortCommand());
                    case BacnetPropertyIds.PROP_NETWORK_TYPE:
                        return new BacnetEnumValueConverter(new BACnetNetworkType());
                    case BacnetPropertyIds.PROP_BASE_DEVICE_SECURITY_POLICY:
                        return new BacnetEnumValueConverter(new BACnetSecurityLevel());
                    case BacnetPropertyIds.PROP_PROTOCOL_LEVEL:
                        return new BacnetEnumValueConverter(new BACnetProtocolLevel());
                    case BacnetPropertyIds.PROP_SILENCED:
                        return new BacnetEnumValueConverter(new BACnetSilencedState());
                    case BacnetPropertyIds.PROP_TIMER_STATE:
                        return new BacnetEnumValueConverter(new BACnetTimerState());
                    case BacnetPropertyIds.PROP_LAST_STATE_CHANGE:
                        return new BacnetEnumValueConverter(new BACnetTimerTransition());
                    case BacnetPropertyIds.PROP_WRITE_STATUS:
                        return new BacnetEnumValueConverter(new BACnetWriteStatus());
                    case BacnetPropertyIds.PROP_DOOR_ALARM_STATE:
                        return new BacnetEnumValueConverter(new BACnetDoorAlarmState());
                    case BacnetPropertyIds.PROP_SECURED_STATUS:
                        return new BacnetEnumValueConverter(new BACnetDoorSecuredStatus());
                    case BacnetPropertyIds.PROP_ACCESS_EVENT:
                    case BacnetPropertyIds.PROP_LAST_ACCESS_EVENT:
                    case BacnetPropertyIds.PROP_ACCESS_ALARM_EVENTS:
                    case BacnetPropertyIds.PROP_ACCESS_TRANSACTION_EVENTS:
                        return new BacnetEnumValueConverter(new BACnetAccessEvents());
                    case BacnetPropertyIds.PROP_FAULT_TYPE:
                        return new BacnetEnumValueConverter(new BACnetFaultParameter.BACnetFaultType());
                    case BacnetPropertyIds.PROP_ACTION:
                        //because Command Object Type also has PROP_ACTION wich decodes to BACnetActionList
                        if (m_Property.bacnetApplicationTags != BacnetApplicationTags.BACNET_APPLICATION_CONTEXT_SPECIFIC)
                            return new BacnetEnumValueConverter(new BACnetAction());
                        return base.Converter;
                    default:
                        return base.Converter;
                }
            }
        }

        // Give a way to display/modify some specifics values in a ListBox
        public override object GetEditor(Type editorBaseType)
        {
            // All Bacnet Time as this
            if (m_Property.bacnetApplicationTags == BacnetApplicationTags.BACNET_APPLICATION_TAG_TIME) return new BacnetTimePickerEditor();
            
            BacnetPropertyReference bpr=(BacnetPropertyReference)m_Property.Tag;

            // A lot of classic Bacnet Enum & BitString
            switch ((BacnetPropertyIds)bpr.propertyIdentifier)
            {
                case BacnetPropertyIds.PROP_PROTOCOL_OBJECT_TYPES_SUPPORTED:
                    return new BacnetBitStringToEnumListDisplay(new BacnetObjectTypes(), true);
                case BacnetPropertyIds.PROP_PROTOCOL_SERVICES_SUPPORTED:
                    return new BacnetBitStringToEnumListDisplay(new BacnetServicesSupported(), true);
                case BacnetPropertyIds.PROP_STATUS_FLAGS:
                    return new BacnetBitStringToEnumListDisplay(new BacnetStatusFlags(), false, true);
                case BacnetPropertyIds.PROP_LIMIT_ENABLE:
                    return new BacnetBitStringToEnumListDisplay(new BacnetEventNotificationData.BacnetLimitEnable(), false, true);

                case BacnetPropertyIds.PROP_EVENT_ENABLE:
                case BacnetPropertyIds.PROP_ACK_REQUIRED:
                case BacnetPropertyIds.PROP_ACKED_TRANSITIONS:
                    return new BacnetBitStringToEnumListDisplay(new BacnetEventNotificationData.BacnetEventEnable(), false, true);

                case BacnetPropertyIds.PROP_OBJECT_TYPE:
                    return new BacnetEnumValueDisplay(new BacnetObjectTypes());
                case BacnetPropertyIds.PROP_NOTIFY_TYPE:
                    return new BacnetEnumValueDisplay(new BacnetEventNotificationData.BacnetNotifyTypes());
                case BacnetPropertyIds.PROP_EVENT_TYPE:
                    return new BacnetEnumValueDisplay(new BacnetEventNotificationData.BacnetEventTypes());
                case BacnetPropertyIds.PROP_EVENT_STATE:
                    return new BacnetEnumValueDisplay(new BacnetEventNotificationData.BacnetEventStates());
                case BacnetPropertyIds.PROP_POLARITY:
                    return new BacnetEnumValueDisplay(new BacnetPolarity());
                case BacnetPropertyIds.PROP_UNITS:
                case BacnetPropertyIds.PROP_OUTPUT_UNITS:
                case BacnetPropertyIds.PROP_CONTROLLED_VARIABLE_UNITS:
                case BacnetPropertyIds.PROP_PROPORTIONAL_CONSTANT_UNITS:
                case BacnetPropertyIds.PROP_INTEGRAL_CONSTANT_UNITS:
                case BacnetPropertyIds.PROP_DERIVATIVE_CONSTANT_UNITS:
                case BacnetPropertyIds.PROP_CAR_LOAD_UNITS:
                    return new BacnetEnumValueDisplay(new BacnetUnitsId());
                case BacnetPropertyIds.PROP_RELIABILITY:
                    return new BacnetEnumValueDisplay(new BacnetReliability());
                case BacnetPropertyIds.PROP_SEGMENTATION_SUPPORTED:
                    return new BacnetEnumValueDisplay(new BacnetSegmentations());
                case BacnetPropertyIds.PROP_SYSTEM_STATUS:
                    return new BacnetEnumValueDisplay(new BacnetDeviceStatus());
                case BacnetPropertyIds.PROP_LAST_RESTART_REASON:
                    return new BacnetEnumValueDisplay(new BacnetRestartReason());
                case BacnetPropertyIds.PROP_PRIORITY_FOR_WRITING:
                    return new BacnetEnumValueDisplay(new BacnetWritePriority());
                case BacnetPropertyIds.PROP_PROGRAM_CHANGE:
                    return new BacnetEnumValueDisplay(new BacnetProgramChange());
                case BacnetPropertyIds.PROP_PRIORITY_ARRAY:
                    return new BacnetEditPriorityArray();
                case BacnetPropertyIds.PROP_BACKUP_AND_RESTORE_STATE:
                    return new BacnetEnumValueDisplay(new BACnetBackupState());
                case BacnetPropertyIds.PROP_FILE_ACCESS_METHOD:
                    return new BacnetEnumValueDisplay(new BacnetFileAccessMethod());


                default :
                    return base.GetEditor(editorBaseType);
            }
        }

        public DynamicEnum Options
        {
            get
            {
                return m_Property.Options;
            }
        }

		#endregion
			
	}
}
